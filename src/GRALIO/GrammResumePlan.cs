#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GralIO
{
    /// <summary>A contiguous range accepted by the GRAMM first/last command line.</summary>
    public sealed class GrammSituationRange
    {
        public int First { get; }
        public int Last { get; }
        public int Count => Last - First + 1;

        public GrammSituationRange(int first, int last)
        {
            if (first < 1 || last < first)
            {
                throw new ArgumentOutOfRangeException(nameof(first));
            }
            First = first;
            Last = last;
        }
    }

    /// <summary>Plans only missing, incomplete or incompatible GRAMM result pairs.</summary>
    public sealed class GrammResumePlan
    {
        private readonly string _directory;
        private readonly GridHeader _grid;
        private Dictionary<int, List<int>> _sunriseOutputs;
        private readonly int _originalMeteoCount;
        private readonly object _sunriseSync = new object();
        private long _sunriseFileLength;
        private DateTime _sunriseLastWriteUtc;
        private readonly int _first;
        private readonly int _final;
        private static readonly uint[] CrcTable = CreateCrcTable();
        public IReadOnlyList<GrammSituationRange> PendingRanges { get; private set; }
        public IReadOnlyList<IReadOnlyList<GrammSituationRange>> InstanceAssignments { get; private set; }
        public int CompletedCount { get; private set; }
        public int PendingCount { get; private set; }
        public int FirstPending { get; private set; }
        public int MaxInstances { get; private set; }

        private GrammResumePlan(string directory, int first, int final, int originalMeteoCount)
        {
            _directory = directory;
            _first = first;
            _final = final;
            _grid = ReadGrid(Path.Combine(directory, "GRAMM.geb"));
            _originalMeteoCount = originalMeteoCount;
            _sunriseOutputs = ReadSunriseOutputs(directory, originalMeteoCount);
            if (originalMeteoCount > 0)
            {
                FileInfo meteo = new FileInfo(Path.Combine(directory, "meteopgt.all"));
                _sunriseFileLength = meteo.Length;
                _sunriseLastWriteUtc = meteo.LastWriteTimeUtc;
            }
            PendingRanges = Array.Empty<GrammSituationRange>();
            InstanceAssignments = Array.Empty<IReadOnlyList<GrammSituationRange>>();
        }

        public static GrammResumePlan Create(string computationDirectory, int first, int final,
            int maxInstances, int originalMeteoCount = 0, Action<int, int> scanProgress = null,
            CancellationToken cancellationToken = default)
        {
            if (first < 1 || final < first || final == int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(first));
            }
            if (maxInstances < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxInstances));
            }
            if (originalMeteoCount < 0 || (originalMeteoCount > 0 && final > originalMeteoCount))
            {
                throw new ArgumentOutOfRangeException(nameof(originalMeteoCount));
            }
            GrammResumePlan plan = new GrammResumePlan(computationDirectory, first, final, originalMeteoCount);
            List<int> pending = new List<int>();
            for (int situation = first; situation <= final; situation++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (plan.IsComplete(situation))
                {
                    plan.CompletedCount++;
                }
                else
                {
                    pending.Add(situation);
                }
                if ((situation - first + 1) % 4 == 0 || situation == final)
                    scanProgress?.Invoke(situation - first + 1, final - first + 1);
            }
            plan.PendingCount = pending.Count;
            plan.FirstPending = pending.Count == 0 ? 0 : pending[0];
            plan.MaxInstances = Math.Min(maxInstances, pending.Count);
            if (pending.Count == 0)
            {
                return plan;
            }

            // Balance the total number assigned to each instance, not the size of each gap.
            // The engine accepts only first/last, so completed gaps remain separate serial runs.
            List<GrammSituationRange> ranges = new List<GrammSituationRange>();
            List<IReadOnlyList<GrammSituationRange>> assignments = new List<IReadOnlyList<GrammSituationRange>>();
            int baseCount = pending.Count / plan.MaxInstances;
            int extraCount = pending.Count % plan.MaxInstances;
            int index = 0;
            for (int instance = 0; instance < plan.MaxInstances; instance++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int end = index + baseCount + (instance < extraCount ? 1 : 0);
                List<GrammSituationRange> assigned = new List<GrammSituationRange>();
                int rangeFirst = pending[index];
                int rangeLast = rangeFirst;
                for (index++; index < end; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int next = pending[index];
                    if (next != rangeLast + 1)
                    {
                        assigned.Add(new GrammSituationRange(rangeFirst, rangeLast));
                        rangeFirst = next;
                    }
                    rangeLast = next;
                }
                assigned.Add(new GrammSituationRange(rangeFirst, rangeLast));
                assignments.Add(assigned.AsReadOnly());
                ranges.AddRange(assigned);
            }
            plan.InstanceAssignments = assignments.AsReadOnly();
            plan.PendingRanges = ranges.AsReadOnly();
            return plan;
        }

        /// <summary>Rechecks current disk files, including all Sunrise outputs for this original situation.</summary>
        public bool IsComplete(int situation)
        {
            if (situation < _first || situation > _final)
            {
                throw new ArgumentOutOfRangeException(nameof(situation));
            }
            if (!IsCompletePair(situation, situation))
            {
                return false;
            }
            if (CurrentSunriseOutputs().TryGetValue(situation, out List<int> outputs))
            {
                foreach (int output in outputs)
                {
                    if (!IsCompletePair(output, situation))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private Dictionary<int, List<int>> CurrentSunriseOutputs()
        {
            if (_originalMeteoCount == 0)
            {
                return _sunriseOutputs;
            }
            lock (_sunriseSync)
            {
                FileInfo meteo = new FileInfo(Path.Combine(_directory, "meteopgt.all"));
                if (meteo.Length != _sunriseFileLength || meteo.LastWriteTimeUtc != _sunriseLastWriteUtc)
                {
                    // A cold Sunrise run appends its intermediate-output mapping before writing results.
                    _sunriseOutputs = ReadSunriseOutputs(_directory, _originalMeteoCount);
                    _sunriseFileLength = meteo.Length;
                    _sunriseLastWriteUtc = meteo.LastWriteTimeUtc;
                }
                return _sunriseOutputs;
            }
        }

        private bool IsCompletePair(int output, int originalSituation)
        {
            string prefix = output.ToString("D5", CultureInfo.InvariantCulture);
            string originalPrefix = originalSituation.ToString("D5", CultureInfo.InvariantCulture);
            try
            {
                using (FileStream wind = new FileStream(Path.Combine(_directory, prefix + ".wnd"),
                    FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (wind.Length != _grid.WindLength || !ReadHeader(wind).Matches(_grid))
                    {
                        return false;
                    }
                    byte[] buffer = new byte[65536];
                    long remaining = _grid.WindLength - 20;
                    while (remaining > 0)
                    {
                        int count = wind.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));
                        if (count == 0)
                        {
                            return false;
                        }
                        remaining -= count;
                    }
                }
                using (FileStream scalar = new FileStream(Path.Combine(_directory, prefix + ".scl"),
                    FileMode.Open, FileAccess.Read, FileShare.Read))
                using (ZipArchive archive = new ZipArchive(scalar, ZipArchiveMode.Read))
                {
                    if (archive.Entries.Count != 3)
                    {
                        return false;
                    }
                    HashSet<string> required = new HashSet<string>(StringComparer.Ordinal)
                    {
                        originalPrefix + ".ust", originalPrefix + ".obl", originalPrefix + ".scl"
                    };
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (!required.Remove(entry.FullName) || entry.Length != _grid.ScalarLength)
                        {
                            return false;
                        }
                        using (Stream content = entry.Open())
                        {
                            byte[] header = new byte[20];
                            content.ReadExactly(header);
                            using (MemoryStream headerStream = new MemoryStream(header))
                            {
                                if (!ReadHeader(headerStream).Matches(_grid))
                                {
                                    return false;
                                }
                            }
                            uint crc = UpdateCrc(uint.MaxValue, header, header.Length);
                            byte[] buffer = new byte[65536];
                            long read = header.Length;
                            int count;
                            while ((count = content.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                read += count;
                                if (read > _grid.ScalarLength)
                                {
                                    return false;
                                }
                                crc = UpdateCrc(crc, buffer, count);
                            }
                            if (read != _grid.ScalarLength || ~crc != entry.Crc32)
                            {
                                return false;
                            }
                        }
                    }
                    return required.Count == 0;
                }
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
            catch (InvalidDataException) { return false; }
            catch (OverflowException) { return false; }
        }

        private static GridHeader ReadGrid(string path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    int nx = int.Parse(ReadValue(reader), CultureInfo.InvariantCulture);
                    int ny = int.Parse(ReadValue(reader), CultureInfo.InvariantCulture);
                    int nz = int.Parse(ReadValue(reader), CultureInfo.InvariantCulture);
                    double west = double.Parse(ReadValue(reader), CultureInfo.InvariantCulture);
                    double east = double.Parse(ReadValue(reader), CultureInfo.InvariantCulture);
                    double south = double.Parse(ReadValue(reader), CultureInfo.InvariantCulture);
                    double north = double.Parse(ReadValue(reader), CultureInfo.InvariantCulture);
                    if (nx <= 0 || ny <= 0 || nz <= 0 || !double.IsFinite(west) || !double.IsFinite(east)
                        || !double.IsFinite(south) || !double.IsFinite(north) || east <= west || north <= south)
                    {
                        throw new InvalidDataException("Invalid GRAMM grid dimensions.");
                    }
                    GridHeader grid = new GridHeader(nx, ny, nz, (float)((east - west) / nx));
                    if (!float.IsFinite(grid.Dx) || grid.Dx <= 0)
                    {
                        throw new InvalidDataException("Invalid GRAMM grid spacing.");
                    }
                    _ = grid.WindLength;
                    _ = grid.ScalarLength;
                    return grid;
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                throw new InvalidDataException("Cannot read GRAMM.geb for result validation.", ex);
            }
        }

        private static string ReadValue(StreamReader reader)
        {
            string line = reader.ReadLine();
            if (line == null)
            {
                throw new InvalidDataException("Incomplete GRAMM.geb.");
            }
            string[] values = line.Split(new char[] { ' ', '\t', ',', ';', '!' }, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length == 0)
            {
                throw new InvalidDataException("Empty GRAMM.geb value.");
            }
            return values[0];
        }

        private static Dictionary<int, List<int>> ReadSunriseOutputs(string directory, int originalCount)
        {
            Dictionary<int, List<int>> outputs = new Dictionary<int, List<int>>();
            if (originalCount == 0)
            {
                return outputs;
            }
            using (StreamReader reader = new StreamReader(Path.Combine(directory, "meteopgt.all")))
            {
                if (reader.ReadLine() == null || reader.ReadLine() == null)
                {
                    throw new InvalidDataException("Incomplete Sunrise meteopgt.all header.");
                }
                int row = 0;
                int previousOriginal = 0;
                int previousStep = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        throw new InvalidDataException("Empty Sunrise meteopgt.all data row.");
                    }
                    row++;
                    if (row <= originalCount)
                    {
                        continue;
                    }
                    string[] fields = line.Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] mapping = fields.Length >= 2 ? fields[1].Split('.') : Array.Empty<string>();
                    if (fields.Length < 4 || mapping.Length != 2
                        || !int.TryParse(mapping[0], NumberStyles.None, CultureInfo.InvariantCulture, out int original)
                        || !int.TryParse(mapping[1], NumberStyles.None, CultureInfo.InvariantCulture, out int step)
                        || original < 1 || original > originalCount || step < 1
                        || original < previousOriginal
                        || (original == previousOriginal ? step != previousStep + 1 : step != 1)
                        || !double.TryParse(fields[3], NumberStyles.Float, CultureInfo.InvariantCulture,
                            out double frequency) || frequency != 0)
                    {
                        throw new InvalidDataException("Invalid Sunrise mapping at data row " + row + ".");
                    }
                    previousOriginal = original;
                    previousStep = step;
                    if (!outputs.TryGetValue(original, out List<int> numbers))
                    {
                        numbers = new List<int>();
                        outputs.Add(original, numbers);
                    }
                    numbers.Add(row);
                }
                if (row < originalCount)
                {
                    throw new InvalidDataException("Sunrise original count exceeds meteopgt.all data rows.");
                }
            }
            return outputs;
        }

        private static GridHeader ReadHeader(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true))
            {
                if (reader.ReadInt32() != -1)
                {
                    throw new InvalidDataException("Unknown GRAMM binary header.");
                }
                return new GridHeader(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadSingle());
            }
        }

        private sealed class GridHeader
        {
            public int Nx { get; }
            public int Ny { get; }
            public int Nz { get; }
            public float Dx { get; }
            public long WindLength => checked(20L + 6L * Nx * Ny * Nz);
            public long ScalarLength => checked(20L + 2L * Nx * Ny);
            public GridHeader(int nx, int ny, int nz, float dx)
            {
                Nx = nx;
                Ny = ny;
                Nz = nz;
                Dx = dx;
            }
            public bool Matches(GridHeader other)
            {
                return Nx == other.Nx && Ny == other.Ny && Nz == other.Nz && float.IsFinite(Dx)
                    && Math.Abs((double)Dx - other.Dx) <= Math.Max(0.0001, other.Dx * 0.00001);
            }
        }

        private static uint UpdateCrc(uint crc, byte[] buffer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                crc = CrcTable[(crc ^ buffer[i]) & 255] ^ (crc >> 8);
            }
            return crc;
        }

        private static uint[] CreateCrcTable()
        {
            uint[] table = new uint[256];
            for (uint i = 0; i < table.Length; i++)
            {
                uint crc = i;
                for (int bit = 0; bit < 8; bit++)
                {
                    crc = (crc & 1) != 0 ? 0xedb88320U ^ (crc >> 1) : crc >> 1;
                }
                table[i] = crc;
            }
            return table;
        }
    }
}
#endif
