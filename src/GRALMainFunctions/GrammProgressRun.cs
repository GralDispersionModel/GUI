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
    internal sealed class GrammProgressRun(string directory)
    {
        internal readonly List<(int Instance, int First, int Last, int ResumeFrom)> Ranges = new();
        private const string Checkpoint = ".gramm-gui-pending-windfields.json";
        private readonly DirectoryInfo computation = new(directory);
        private readonly record struct Stamp(long Ticks, long Bytes)
        {
            internal static Stamp Read(FileInfo file) => new(file.LastWriteTimeUtc.Ticks, file.Length);
        }
        private readonly record struct Outputs(Stamp Wind, Stamp Scalar);
        private sealed record EngineCheckpoint(string Engine, Dictionary<int, Outputs> Pending);
        private readonly Dictionary<int, Outputs> baseline = new();
        private Dictionary<int, Outputs> pending = new();
        private readonly HashSet<int> resumeFields = new();
        internal bool Continuing { get; init; }
        internal string EngineGeneration { get; init; } = GrammEngine.Legacy;
        private bool generationTracked;
        internal volatile bool Retired;
        internal bool LegacyResume { get; private set; }
        private int nx, ny, nz;
        private long fieldLength;
        private bool capturedFiles;
        internal int PlannedSituations { get; private set; }
        internal string UnavailableReason { get; private set; }

        internal void CaptureNativeBaseline() => CaptureFiles(1, int.MaxValue);

        private bool CaptureFiles(int first, int last)
        {
            string previous = ReadCheckpoint();
            bool engineChanged = previous != EngineGeneration || EngineGeneration == GrammEngine.Unknown;
            foreach (FileInfo file in computation.EnumerateFiles())
            {
                if (Retired) { return engineChanged; }
                bool wind = file.Extension.Equals(".wnd", StringComparison.OrdinalIgnoreCase);
                if (!wind && !file.Extension.Equals(".scl", StringComparison.OrdinalIgnoreCase)) { continue; }
                int id = Situation(file);
                if (id == 0 || (!engineChanged && (id < first || id > last))) { continue; }
                baseline.TryGetValue(id, out Outputs pair);
                var stamp = Stamp.Read(file);
                baseline[id] = wind ? pair with { Wind = stamp } : pair with { Scalar = stamp };
            }
            // A narrower launch must still invalidate untouched fields from a different engine.
            if (engineChanged) { pending = new Dictionary<int, Outputs>(baseline); }
            capturedFiles = true;
            return engineChanged;
        }

        internal void CaptureBaseline(int first, int last)
        {
            PlannedSituations = last - first + 1;
            try
            {
                bool engineChanged = CaptureFiles(first, last);
                if (Retired) { return; }

                if (GrammInstanceRanges.ReadSteadySettings(computation.FullName) == null)
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

                if (!Continuing || engineChanged) { return; }
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
            catch (InvalidDataException ex) { UnavailableReason = "Counts unavailable: " + ex.Message; }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or OverflowException)
            {
                UnavailableReason = "Counts unavailable: the run inputs or existing wind fields could not be checked. " + ex.Message;
            }
        }

        private string ReadCheckpoint()
        {
            try
            {
                using var file = File.OpenRead(Path.Combine(computation.FullName, Checkpoint));
                var options = new JsonSerializerOptions { RespectRequiredConstructorParameters = true };
                using var json = JsonDocument.Parse(file);
                var root = json.RootElement;
                if (root.ValueKind != JsonValueKind.Object) { throw new JsonException(); }
                generationTracked = root.TryGetProperty(nameof(EngineCheckpoint.Engine), out _);
                var saved = generationTracked ? root.Deserialize<EngineCheckpoint>(options) :
                    new EngineCheckpoint(GrammEngine.Legacy, root.Deserialize<Dictionary<int, Outputs>>(options));
                if (saved.Engine is not (GrammEngine.Legacy or GrammEngine.Reset or GrammEngine.Unknown) ||
                    saved.Pending == null || saved.Pending.Any(p => p.Key < 1 || p.Value == default ||
                        !ValidStamp(p.Value.Wind) || !ValidStamp(p.Value.Scalar))) { throw new JsonException(); }
                pending = saved.Pending;
                return saved.Engine;
            }
            catch (FileNotFoundException) { return GrammEngine.Legacy; } // An untracked folder has no evidence of corrected fields.
            catch (JsonException ex) { throw new IOException("Invalid GRAMM GUI restart checkpoint.", ex); }
        }

        private static bool ValidStamp(Stamp stamp) =>
            stamp == default || (stamp.Ticks > 0 && stamp.Ticks <= DateTime.MaxValue.Ticks && stamp.Bytes >= 0);

        internal static bool HasCheckpoint(string directory) => File.Exists(Path.Combine(directory, Checkpoint));

        internal int NativeResumeFrom(int first, int nextMissing)
        {
            if (nextMissing > first) { CheckUnknownResume(); }
            // Native formats resume by wind filename. Retain that rule only for winds rewritten since the checkpoint.
            return pending.Where(item => item.Key >= first && item.Key < nextMissing && item.Value.Wind != default &&
                    baseline.TryGetValue(item.Key, out var current) && current.Wind == item.Value.Wind)
                .Select(item => item.Key).DefaultIfEmpty(nextMissing).Min();
        }

        private void CheckUnknownResume()
        {
            if (EngineGeneration == GrammEngine.Unknown)
            {
                throw new IOException("The selected launcher does not identify the engine that generated the existing wind fields. Use Start to recalculate the selected situations or select the matching GRAMM.exe.");
            }
        }

        internal void SaveNativePending(int from)
        {
            foreach (var (id, old) in baseline)
            {
                if (id >= from) { pending[id] = old; }
            }
            WriteCheckpoint();
        }

        internal int[] SavePending(List<(int First, int Last)> assignments, int[] starts = null)
        {
            starts ??= assignments.Select(range => NextSituation(range.First, range.Last)).ToArray();
            if (LegacyResume && EngineGeneration == GrammEngine.Legacy && !generationTracked) { return starts; }
            if (!LegacyResume && UnavailableReason != null && (Continuing || !capturedFiles)) { throw new IOException(UnavailableReason); }
            for (int index = 0; index < assignments.Count; index++)
            {
                var range = assignments[index];
                if (Continuing && !LegacyResume && baseline.TryGetValue(range.First, out var prefix) &&
                    prefix.Wind.Bytes > 0 && prefix.Scalar.Bytes > 0) { CheckUnknownResume(); }
                foreach (int id in Enumerable.Range(range.First, range.Last - range.First + 1))
                {
                    if (id >= starts[index] && baseline.TryGetValue(id, out var old)) { pending[id] = old; }
                    else if (!LegacyResume) { pending.Remove(id); }
                }
            }
            WriteCheckpoint();
            return starts;
        }

        private void WriteCheckpoint()
        {
            // An empty checkpoint still identifies a multi-instance run after the project is reopened.
            // Only pre-existing outputs scheduled for replacement need individual entries.
            string target = Path.Combine(computation.FullName, Checkpoint);
            string temp = target + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    object checkpoint = EngineGeneration != GrammEngine.Legacy || generationTracked
                        ? new EngineCheckpoint(EngineGeneration, pending) : pending;
                    JsonSerializer.Serialize(file, checkpoint);
                    file.Flush(true);
                }
                File.Move(temp, target, true);
            }
            finally { File.Delete(temp); }
        }

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

        private bool ClosedWind(FileInfo file, int id, Stamp stale = default, Dictionary<int, DateTime> verified = null)
        {
            try
            {
                var stamp = Stamp.Read(file);
                if (stamp.Bytes != fieldLength || stamp == stale) { return false; }
                // The stock writer holds an exclusive handle until the field is closed.
                if (verified != null && verified.TryGetValue(id, out var written) && written == file.LastWriteTimeUtc) { return true; }
                using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 20);
                if (stream.Length != fieldLength) { return false; }
                using var reader = new BinaryReader(stream);
                if (!HasHeader(reader)) { return false; }
                if (verified != null) { verified[id] = file.LastWriteTimeUtc; }
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return false; }
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
            catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException) { return false; }
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
                baseline.TryGetValue(id, out var previous);
                var stale = id >= Ranges[row].ResumeFrom ? previous.Wind : default;
                if (ClosedWind(file, id, stale, verified)) { counts[row]++; }
            }
            return counts;
        }
    }
}
