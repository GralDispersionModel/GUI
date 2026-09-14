// Source group definitions and display names. GPL-3.0, same license as the GUI.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Gral
{
    public static class SourceGroupCatalog
    {
        public static int GetNumber(string text)
        {
            string[] parts = (text ?? string.Empty).Split(',', ':');
            return int.TryParse(parts[parts.Length - 1].Trim(), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out int id) && id > 0 ? id : 0;
        }

        public static SortedDictionary<int, string> ReadDefinitions(string path)
        {
            var definitions = new SortedDictionary<int, string>();
            if (!File.Exists(path)) return definitions;
            foreach (string line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] fields = line.Split(',');
                if (fields.Length != 2 || !int.TryParse(fields[1], NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out int id) || id <= 0 || definitions.ContainsKey(id))
                    throw new InvalidDataException("Source groups must have unique positive Int32 numbers: " + line);
                definitions.Add(id, fields[0]);
            }
            return definitions;
        }

        public static List<string> Choices(IDictionary<int, string> definitions)
        {
            var choices = new SortedDictionary<int, string>();
            // Keep the familiar unnamed legacy choices; this is not an ID limit.
            for (int id = 1; id <= 99; id++) choices.Add(id, id.ToString(CultureInfo.InvariantCulture));
            foreach (var group in definitions)
                choices[group.Key] = string.IsNullOrWhiteSpace(group.Value)
                    ? group.Key.ToString(CultureInfo.InvariantCulture)
                    : group.Value + "," + group.Key.ToString(CultureInfo.InvariantCulture);
            return new List<string>(choices.Values);
        }

        public static string DisplayName(IEnumerable<string> choices, int id)
        {
            foreach (string choice in choices)
                if (GetNumber(choice) == id) return choice;
            return id.ToString(CultureInfo.InvariantCulture);
        }

        public static void UpdateModulation(string path, int id, string diurnal, string seasonal)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var lines = File.Exists(path) ? new List<string>(File.ReadAllLines(path)) : new List<string>();
            string entry = id.ToString(CultureInfo.InvariantCulture) + "," + diurnal + "," + seasonal;
            int index = lines.FindIndex(line => int.TryParse(line.Split(',')[0], out int number) && number == id);
            if (index < 0) lines.Add(entry);
            else lines[index] = entry;
            File.WriteAllLines(path, lines);
        }

        // Match engine column mapping and defaults without allocating by the largest ID.
        public static Dictionary<int, double> ReadMeanFactors(string path, IEnumerable<int> selected,
            List<string> dates)
        {
            var wanted = new HashSet<int>(selected);
            var means = new Dictionary<int, double>();
            char[] separators = { ',', ':', '-', '\t', ';' };
            using (var reader = new StreamReader(path))
            {
                string header = reader.ReadLine();
                if (header == null) throw new InvalidDataException("Empty emissions time series.");
                // Also retain the GUI's legacy space-delimited time-series format.
                var splitOptions = StringSplitOptions.None;
                if (header.IndexOfAny(separators) < 0)
                {
                    separators = new[] { ' ' };
                    splitOptions = StringSplitOptions.RemoveEmptyEntries;
                }
                string[] columns = header.Split(separators, splitOptions);
                var seen = new HashSet<int>();
                var positions = new Dictionary<int, int>();
                for (int i = 2; i < columns.Length; i++)
                {
                    if (!int.TryParse(columns[i], out int id) || id <= 0 || !seen.Add(id))
                        throw new InvalidDataException("Invalid or duplicate source group in emissions time series.");
                    if (wanted.Contains(id)) { positions.Add(id, i); means.Add(id, 0); }
                }
                int count = 0;
                while (!reader.EndOfStream)
                {
                    string[] fields = reader.ReadLine().Split(separators, splitOptions);
                    foreach (var group in positions)
                    {
                        float factor = group.Value < fields.Length
                            ? float.Parse(fields[group.Value], CultureInfo.InvariantCulture) : 1;
                        if (!float.IsFinite(factor) || factor < 0)
                            throw new InvalidDataException("Emission factors must be finite and nonnegative.");
                        means[group.Key] += factor;
                    }
                    if (fields.Length >= 2) dates.Add(fields[0] + ". " + fields[1] + ":00");
                    count++;
                }
                foreach (int id in positions.Keys) means[id] = count > 0 ? means[id] / count : 1;
            }
            return means;
        }
    }
}
