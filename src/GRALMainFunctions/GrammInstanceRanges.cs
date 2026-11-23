using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Gral
{
    internal static class GrammInstanceRanges
    {
        internal static List<(int First, int Last)> Create(string computationDirectory, int first, int last,
            int requested, int sunriseOriginalCount)
        {
            int count = last - first + 1;
            int instances = Math.Min(requested, count);
            var ranges = new List<(int First, int Last)>(instances);
            long[] weights = sunriseOriginalCount >= last && instances > 1 && instances < count
                ? ReadSunriseWeights(computationDirectory, first, last) : null;

            if (weights == null || weights.All(weight => weight == weights[0]))
            {
                for (int instance = 0; instance < instances; instance++)
                {
                    int size = count / instances + (instance < count % instances ? 1 : 0);
                    ranges.Add((first, first + size - 1));
                    first += size;
                }
                return ranges;
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

        private static long[] ReadSunriseWeights(string directory, int first, int last)
        {
            try
            {
                using var input = File.OpenText(Path.Combine(directory, "GRAMMin.dat"));
                if (input.ReadLine()?.Contains("Version") != true ||
                    !string.Equals(input.ReadLine(), "y", StringComparison.OrdinalIgnoreCase)) { return null; }

                // IIN.dat is positional. Model times <= 1 use the domain-crossing scheme.
                string[] settings = File.ReadLines(Path.Combine(directory, "IIN.dat")).Take(29).ToArray();
                if (settings.Length < 29 || !ReadSetting(settings[3], out int seconds) || seconds <= 1 ||
                    !ReadSetting(settings[25], out int transient) || transient != 0 ||
                    !ReadSetting(settings[28], out int format) || format != 0) { return null; }

                using var meteo = File.OpenText(Path.Combine(directory, "meteopgt.all"));
                meteo.ReadLine();
                meteo.ReadLine();
                var weights = new long[last - first + 1];
                char[] separators = { ' ', ';', ',', '\t' };
                for (int id = 1; id <= last; id++)
                {
                    string line = meteo.ReadLine();
                    if (line == null) { return null; }
                    if (id < first) { continue; }
                    string[] fields = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    if (fields.Length < 3 || !double.TryParse(fields[1], NumberStyles.Float,
                        CultureInfo.InvariantCulture, out double speed) || !double.IsFinite(speed) ||
                        !int.TryParse(fields[2], out int stability) || stability < 1 || stability > 7) { return null; }
                    // GRAMM Meteopgtall.Integrationtime, dynamic sunrise. Centiseconds retain 0.17 T exactly.
                    weights[id - first] = stability switch
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
                return weights;
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
