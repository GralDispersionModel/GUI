using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Gral
{
    internal static class GrammInstanceRanges
    {
        internal static bool IsSteadyBinaryProject(string directory) => ReadSteadyModelTime(directory) != null;

        internal static List<(int First, int Last)> SelectForEngine(bool steadyCaseReset, string computationDirectory,
            int first, int last, int requested, int sunriseOriginalCount)
        {
            if (steadyCaseReset && ReadSteadyModelTime(computationDirectory, verifyScheduling: true) is string modelTime)
            {
                return CreateBalanced(computationDirectory, first, last, requested, sunriseOriginalCount, modelTime);
            }

            // V2701 added a rounded offset to each inclusive start. Older cores can depend on these starts.
            int offset = Convert.ToInt32(Math.Max(1, (last - first) / (double)requested));
            var ranges = new List<(int First, int Last)>();
            while (true)
            {
                int end = ranges.Count + 1 == requested || last - first <= offset ? last : first + offset;
                ranges.Add((first, end));
                if (end == last) { return ranges; }
                first = end + 1;
            }
        }

        internal static List<(int First, int Last)> Create(string computationDirectory, int first, int last,
            int requested, int sunriseOriginalCount)
        {
            return CreateBalanced(computationDirectory, first, last, requested, sunriseOriginalCount,
                ReadSteadyModelTime(computationDirectory));
        }

        private static List<(int First, int Last)> CreateBalanced(string computationDirectory, int first, int last,
            int requested, int sunriseOriginalCount, string modelTime)
        {
            int count = last - first + 1;
            int instances = Math.Min(requested, count);
            long[] weights = sunriseOriginalCount >= last && instances > 1 && instances < count &&
                modelTime != null && ReadSetting(modelTime, out int seconds) && seconds > 1
                ? ReadSunriseWeights(computationDirectory, first, last, seconds) : null;

            if (weights == null || weights.All(weight => weight == weights[0]))
            {
                // Give one extra situation to each of the first count % instances workers.
                int Offset(int worker) => worker * (count / instances) + Math.Min(worker, count % instances);
                return Enumerable.Range(0, instances).Select(i => (first + Offset(i), first - 1 + Offset(i + 1))).ToList();
            }

            // Find the smallest maximum integration time feasible with this many contiguous ranges.
            long low = weights.Max();
            long high = weights.Sum();
            while (low < high)
            {
                long limit = low + (high - low) / 2;
                int needed = 1;
                long load = 0;
                foreach (long weight in weights)
                {
                    if (load + weight > limit)
                    {
                        needed++;
                        load = 0;
                    }
                    load += weight;
                }
                if (needed <= instances) { high = limit; }
                else { low = limit + 1; }
            }

            var ranges = new List<(int First, int Last)>(instances);
            int next = 0;
            for (int remaining = instances - 1; remaining >= 0; remaining--)
            {
                int start = next;
                long load = weights[next++];
                while (next < count - remaining && load + weights[next] <= low)
                {
                    load += weights[next++];
                }
                ranges.Add((first + start, first + next - 1));
            }
            return ranges;
        }

        private static string ReadSteadyModelTime(string directory, bool verifyScheduling = false)
        {
            try
            {
                var settings = ReadSteadySettings(directory);
                if (settings == null || verifyScheduling && !HasVerifiedWorkerSettings(directory, settings[17])) { return null; }
                return settings[3];
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException) { return null; }
        }

        // GRAMM reads ISTAT and IOUT by position. A supported run uses steady binary meteopgt.all fields.
        internal static string[] ReadSteadySettings(string directory)
        {
            using var input = File.OpenText(Path.Combine(directory, "GRAMMin.dat"));
            string meteo = input.ReadLine();
            if (meteo?.Contains("Version") == true) { meteo = input.ReadLine(); }
            string[] settings = File.ReadLines(Path.Combine(directory, "IIN.dat")).Take(29).ToArray();
            if (string.IsNullOrWhiteSpace(meteo) || settings.Length < 29 || !ReadSetting(settings[25], out int steady) ||
                !ReadSetting(settings[28], out int format))
            {
                throw new InvalidDataException("The GRAMM output settings could not be read.");
            }
            return string.Equals(meteo, "y", StringComparison.OrdinalIgnoreCase) && steady == 0 && format == 0 ? settings : null;
        }

        private static bool HasVerifiedWorkerSettings(string directory, string scalarFlags)
        {
            if (!ReadSetting(scalarFlags, out int flags) || flags < 0 || flags > 1111111 ||
                flags / 100 % 10 != 0 || flags.ToString(CultureInfo.InvariantCulture).Any(c => c != '0' && c != '1'))
            {
                return false; // ICTE is the hundreds digit of IFLAGS2.
            }
            string limit = File.ReadLines(Path.Combine(directory, "Max_Proc.txt")).FirstOrDefault();
            if (!int.TryParse(limit, NumberStyles.Integer, CultureInfo.InvariantCulture, out int threads) || threads != 1) { return false; }

            using var meteo = File.OpenText(Path.Combine(directory, "meteopgt.all"));
            string[] header = meteo.ReadLine()?.Split(new[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (header == null || header.Length < 3) { return false; }
            // TEMPINT reads anemometer height, time-series flag and sector width. A zero flag randomizes direction.
            return FiniteNumber(header[0], out double height) && height > 0 &&
                FiniteNumber(header[1], out double timeSeries) && timeSeries != 0 &&
                FiniteNumber(header[2], out double sectorWidth) && sectorWidth >= 0;
        }

        private static bool FiniteNumber(string text, out double value) =>
            double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) && !double.IsNaN(value) && !double.IsInfinity(value);

        private static long[] ReadSunriseWeights(string directory, int first, int last, int seconds)
        {
            try
            {
                var weights = new long[last - first + 1];
                char[] separators = { ' ', ';', ',', '\t' };
                int index = 0;
                foreach (string line in File.ReadLines(Path.Combine(directory, "meteopgt.all")).Skip(first + 1).Take(weights.Length))
                {
                    string[] fields = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    if (fields.Length < 3 || !FiniteNumber(fields[1], out double speed) ||
                        !int.TryParse(fields[2], out int stability) || stability < 1 || stability > 7) { return null; }
                    // GRAMM Meteopgtall.Integrationtime, dynamic sunrise. Centiseconds retain 0.17 T exactly.
                    weights[index++] = stability switch
                    {
                        <= 3 => 21600L * 100,
                        4 => Math.Min(17L * seconds, 600L * 100),
                        6 when speed <= 2 => 300L * seconds,
                        7 when speed < 0.5 => Math.Min(200L * seconds, 7200L * 100),
                        7 when speed < 1 => Math.Min(150L * seconds, 7200L * 100),
                        7 => Math.Min(100L * seconds, 3600L * 100),
                        _ => 100L * seconds
                    };
                }
                return index == weights.Length ? weights : null;
            }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
        }

        private static bool ReadSetting(string line, out int value)
        {
            return int.TryParse(line.Split(':', '!').ElementAtOrDefault(1), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out value);
        }
    }
}
