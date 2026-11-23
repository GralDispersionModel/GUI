using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace Gral
{
    internal sealed class GrammProgressRun
    {
        internal readonly List<(int Instance, int First, int Last, int ResumeFrom)> Ranges = new();
        private const string Checkpoint = ".gramm-gui-pending-windfields.json";
        private readonly DirectoryInfo computation;
        private readonly record struct Stamp(long Ticks, long Bytes);
        private readonly record struct Outputs(Stamp Wind, Stamp Scalar);
        private readonly Dictionary<int, Outputs> baseline = new();
        private Dictionary<int, Outputs> pending = new();
        private readonly HashSet<int> resumeFields = new();
        internal bool Continuing { get; init; }
        internal volatile bool Retired;
        internal bool LegacyResume { get; private set; }
        private int nx, ny, nz;
        private long fieldLength;
        private bool capturedFiles;
        internal int PlannedSituations { get; private set; }
        internal string UnavailableReason { get; private set; }

        internal GrammProgressRun(string directory)
        {
            computation = new DirectoryInfo(directory);
        }

        internal void CaptureBaseline(int first, int last)
        {
            PlannedSituations = last - first + 1;
            try
            {
                // A narrower Start or Continue must preserve pending outputs outside its assignment.
                ReadCheckpoint();
                foreach (FileInfo file in computation.EnumerateFiles())
                {
                    if (Retired) { return; }
                    bool wind = file.Extension.Equals(".wnd", StringComparison.OrdinalIgnoreCase);
                    if (!wind && !file.Extension.Equals(".scl", StringComparison.OrdinalIgnoreCase)) { continue; }
                    int id = Situation(file);
                    if (id < first || id > last) { continue; }
                    baseline.TryGetValue(id, out Outputs pair);
                    var stamp = new Stamp(file.LastWriteTimeUtc.Ticks, file.Length);
                    baseline[id] = wind ? pair with { Wind = stamp } : pair with { Scalar = stamp };
                }
                capturedFiles = true;

                // GRAMM reads ISTAT and IOUT by position in IIN.dat.
                string[] settings = File.ReadLines(Path.Combine(computation.FullName, "IIN.dat")).Take(29).ToArray();
                using var input = File.OpenText(Path.Combine(computation.FullName, "GRAMMin.dat"));
                string meteo = input.ReadLine();
                if (meteo != null && meteo.Contains("Version")) { meteo = input.ReadLine(); }
                if (string.IsNullOrWhiteSpace(meteo) || settings.Length < 29 ||
                    !ReadSetting(settings[25], out int steady) || !ReadSetting(settings[28], out int format))
                {
                    UnavailableReason = "Counts unavailable: the GRAMM output settings could not be read.";
                    return;
                }
                if (!string.Equals(meteo, "y", StringComparison.OrdinalIgnoreCase) || steady != 0 || format != 0)
                {
                    LegacyResume = true;
                    UnavailableReason = "Counts require steady-state meteopgt.all and binary wind fields (format 0).";
                    return;
                }

                using var grid = File.OpenText(Path.Combine(computation.FullName, "GRAMM.geb"));
                if (!ReadDimension(grid, out nx) || !ReadDimension(grid, out ny) || !ReadDimension(grid, out nz))
                {
                    UnavailableReason = "Counts unavailable: the GRAMM grid dimensions could not be read.";
                    return;
                }
                fieldLength = checked(20 + 6L * nx * ny * nz);

                if (!Continuing) { return; }
                foreach (var (id, pair) in baseline)
                {
                    if (Retired) { return; }
                    if (pair.Wind.Bytes != fieldLength || pair.Scalar.Bytes == 0) { continue; }
                    if (pending.TryGetValue(id, out var old) && (old.Wind == pair.Wind || old.Scalar == pair.Scalar)) { continue; }
                    string name = id.ToString("D5", CultureInfo.InvariantCulture);
                    if (ClosedWind(new FileInfo(Path.Combine(computation.FullName, name + ".wnd")), id) && ClosedScalar(name))
                    {
                        resumeFields.Add(id);
                    }
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is OverflowException)
            {
                UnavailableReason = "Counts unavailable: the run inputs or existing wind fields could not be checked. " + ex.Message;
            }
        }

        private void ReadCheckpoint()
        {
            try
            {
                using var file = File.OpenRead(Path.Combine(computation.FullName, Checkpoint));
                var options = new JsonSerializerOptions { RespectRequiredConstructorParameters = true };
                pending = JsonSerializer.Deserialize<Dictionary<int, Outputs>>(file, options);
                if (pending == null || pending.Any(p => p.Key < 1 || p.Value == default ||
                    !ValidStamp(p.Value.Wind) || !ValidStamp(p.Value.Scalar))) { throw new JsonException(); }
            }
            catch (FileNotFoundException) { } // Projects without a previous multi-instance launch.
            catch (JsonException ex) { throw new IOException("Invalid GRAMM GUI restart checkpoint.", ex); }
        }

        private static bool ValidStamp(Stamp stamp) =>
            stamp == default || (stamp.Ticks > 0 && stamp.Ticks <= DateTime.MaxValue.Ticks && stamp.Bytes >= 0);

        internal static bool HasCheckpoint(string directory) => File.Exists(Path.Combine(directory, Checkpoint));

        internal void SavePending(List<(int First, int Last)> assignments)
        {
            if (LegacyResume) { return; }
            if (UnavailableReason != null && (Continuing || !capturedFiles)) { throw new IOException(UnavailableReason); }
            foreach (var range in assignments)
            {
                int from = NextSituation(range.First, range.Last);
                for (int id = range.First; id <= range.Last; id++)
                {
                    if (id >= from && baseline.TryGetValue(id, out var old)) { pending[id] = old; }
                    else { pending.Remove(id); }
                }
            }
            // An empty checkpoint still identifies a multi-instance run after the project is reopened.
            // Only pre-existing outputs scheduled for replacement need individual entries.
            string target = Path.Combine(computation.FullName, Checkpoint);
            string temp = target + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    JsonSerializer.Serialize(file, pending);
                    file.Flush(true);
                }
                File.Move(temp, target, true);
            }
            finally { File.Delete(temp); }
        }

        private static bool ReadSetting(string line, out int value) =>
            int.TryParse(line.Split(':', '!').ElementAtOrDefault(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        private static bool ReadDimension(StreamReader grid, out int value) =>
            int.TryParse(grid.ReadLine()?.Split('!')[0], out value) && value > 0;

        private static int Situation(FileInfo file)
        {
            string name = Path.GetFileNameWithoutExtension(file.Name);
            return int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out int id) &&
                name == id.ToString("D5", CultureInfo.InvariantCulture) ? id : 0;
        }

        internal int NextSituation(int first, int last)
        {
            while (first <= last && resumeFields.Contains(first)) { first++; }
            return first;
        }

        private bool HasHeader(BinaryReader reader)
        {
            if (reader.ReadInt32() != -1 || reader.ReadInt32() != nx || reader.ReadInt32() != ny ||
                reader.ReadInt32() != nz) { return false; }
            float dx = reader.ReadSingle();
            return dx > 0 && float.IsFinite(dx);
        }

        private bool ClosedWind(FileInfo file, int id, Dictionary<int, DateTime> verified = null)
        {
            try
            {
                // The stock writer holds an exclusive handle until the field is closed.
                if (verified != null && verified.TryGetValue(id, out var written) && written == file.LastWriteTimeUtc) { return true; }
                using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 20);
                if (stream.Length != fieldLength) { return false; }
                using var reader = new BinaryReader(stream);
                if (!HasHeader(reader)) { return false; }
                if (verified != null) { verified[id] = file.LastWriteTimeUtc; }
                return true;
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }

        private bool ClosedScalar(string name)
        {
            try
            {
                using var stream = File.OpenRead(Path.Combine(computation.FullName, name + ".scl"));
                long size = 20 + 2L * nx * ny;
                using var reader = new BinaryReader(stream);
                bool raw = reader.ReadInt32() == -1;
                stream.Position = 0;
                if (raw) { return stream.Length == size && HasHeader(reader); }
                using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
                return zip.Entries.Count == 3 && zip.GetEntry(name + ".scl")?.Length == size &&
                    zip.GetEntry(name + ".ust")?.Length == size && zip.GetEntry(name + ".obl")?.Length == size;
            }
            catch (InvalidDataException) { return false; }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }

        internal int[] CountWindFields(Dictionary<int, DateTime> verified, CancellationToken cancellation)
        {
            int[] counts = new int[Ranges.Count];
            foreach (FileInfo file in computation.EnumerateFiles("*.wnd"))
            {
                if (cancellation.IsCancellationRequested) { break; }
                int id = Situation(file);
                int row = Ranges.FindIndex(range => id >= range.First && id <= range.Last);
                if (row < 0) { continue; }
                try
                {
                    var stamp = new Stamp(file.LastWriteTimeUtc.Ticks, file.Length);
                    if (stamp.Bytes != fieldLength) { continue; }
                    if (id >= Ranges[row].ResumeFrom && baseline.TryGetValue(id, out var previous) && previous.Wind == stamp) { continue; }
                    if (ClosedWind(file, id, verified)) { counts[row]++; }
                }
                catch (IOException) { } // Locked or removed during this scan. Retry on the next refresh.
                catch (UnauthorizedAccessException) { }
            }
            return counts;
        }
    }
}
