using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace GralIO
{
    /// <summary>GRAMM IIN.dat values and the positional CustomInit.txt input contract.</summary>
    public sealed class GrammInitialConditions
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        public double Latitude { get; set; } = 47;
        public double HumidityPercent { get; set; } = 20;
        public double AirTemperatureC { get; set; } = 6.85;
        public double SurfaceTemperatureC { get; set; } = 6.85;
        public double SoilTemperatureC { get; set; } = 6.85;
        public double NeutralHeight { get; set; } = 5000;
        public double TemperatureGradient { get; set; } = -0.0065;
        public static readonly string[] CustomColumns = { "snow_height_m", "air_temperature_C", "surface_temperature_C", "soil_temperature_1m_C", "water_temperature_delta_K", "relative_humidity_percent", "inversion_height_m", "air_gradient_K_per_m", "inversion_gradient_K_per_m", "soil_gradient_K_per_m" };
        private static readonly int[] IinLines = { 6, 8, 9, 10, 11, 12, 13 };

        public static GrammInitialConditions ReadIin(string path, double fallbackLatitude)
        {
            var value = new GrammInitialConditions { Latitude = fallbackLatitude };
            if (!File.Exists(path)) return value;
            string[] lines = File.ReadAllLines(path);
            if (lines.Length < 14) throw new InvalidDataException("IIN.dat has fewer than 14 lines.");
            var data = IinLines.Select(i => ParseNumber(lines[i].Split(':', '!')[1].Trim(), "IIN.dat line " + (i + 1))).ToArray();
            value.HumidityPercent = data[0]; value.AirTemperatureC = data[1] - 273.15;
            value.TemperatureGradient = data[2]; value.NeutralHeight = data[3];
            value.SurfaceTemperatureC = data[4] - 273.15; value.SoilTemperatureC = data[5] - 273.15; value.Latitude = data[6];
            value.Validate(); return value;
        }

        public void Validate()
        {
            if (!Finite(Latitude) || Latitude < -90 || Latitude > 90) throw new InvalidDataException("Latitude must be between -90 and 90 degrees.");
            if (!Finite(HumidityPercent) || HumidityPercent <= 0 || HumidityPercent > 100) throw new InvalidDataException("Relative humidity must be greater than 0 and at most 100 percent.");
            foreach (double temp in new[] { AirTemperatureC, SurfaceTemperatureC, SoilTemperatureC })
                if (!Finite(temp) || temp <= -273.15) throw new InvalidDataException("Temperatures must be above absolute zero.");
            if (!Finite(NeutralHeight) || NeutralHeight <= 0 || !Finite(TemperatureGradient)) throw new InvalidDataException("Check the neutral-layer height and temperature gradient.");
        }

        public void SaveIin(string path)
        {
            Validate(); string[] lines = File.ReadAllLines(path);
            double[] values = { HumidityPercent, AirTemperatureC + 273.15, TemperatureGradient, NeutralHeight, SurfaceTemperatureC + 273.15, SoilTemperatureC + 273.15, Latitude };
            if (lines.Length < 14) throw new InvalidDataException("IIN.dat has fewer than 14 lines.");
            for (int n = 0; n < IinLines.Length; n++)
            {
                int i = IinLines[n], colon = lines[i].IndexOf(':'), comment = lines[i].IndexOf('!');
                if (colon < 0) throw new InvalidDataException("Missing ':' in IIN.dat line " + (i + 1));
                lines[i] = lines[i].Substring(0, colon + 1) + "  " + Number(values[n]) + "  " + (comment >= 0 ? lines[i].Substring(comment) : "");
            }
            SaveWithBackup(path, lines);
        }

        public static double ParseNumber(string text, string field)
        {
            if (!double.TryParse(text, NumberStyles.Float, Invariant, out double value) || !Finite(value))
                throw new InvalidDataException(field + ": enter a finite number using a decimal point.");
            return value;
        }
        private static bool Finite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
        public static string Number(double v) => v.ToString("0.############", Invariant);
        private static string[] Tokens(string line) => line.Split(new[] { ' ', ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        public static List<GrammCustomRow> ReadSituations(string path)
        {
            string[] lines = File.ReadAllLines(path);
            if (lines.Length < 3) throw new InvalidDataException("meteopgt.all must contain two header lines and at least one weather situation.");
            var rows = new List<GrammCustomRow>();
            for (int i = 2; i < lines.Length; i++)
            {
                string[] tokens = Tokens(lines[i]);
                if (tokens.Length != 4) throw new InvalidDataException("meteopgt.all line " + (i + 1) + ": expected four values.");
                foreach (string token in tokens) ParseNumber(token, "meteopgt.all line " + (i + 1));
                rows.Add(new GrammCustomRow { Situation = i - 1, Meteo = tokens });
            }
            return rows;
        }

        public static bool IsGuiGenerated(string path)
        {
            if (!File.Exists(path)) return false;
            using (var reader = new StreamReader(path))
                return reader.ReadLine() == "GRAMM custom initial conditions; values at sea level";
        }

        public static List<GrammCustomRow> ReadCustom(string customPath, string meteoPath)
        {
            var rows = ReadSituations(meteoPath); string[] lines = File.ReadAllLines(customPath);
            if (lines.Length != rows.Count + 2) throw new InvalidDataException("CustomInit.txt must have one row for every meteopgt.all situation, including situations before the selected start number.");
            for (int i = 0; i < rows.Count; i++)
            {
                string[] tokens = Tokens(lines[i + 2]);
                if (tokens.Length < 4 || tokens.Length > 14) throw new InvalidDataException("CustomInit.txt situation " + (i + 1) + ": expected 4 to 14 values.");
                for (int n = 0; n < 4; n++)
                    if (ParseNumber(tokens[n], "CustomInit weather value") != ParseNumber(rows[i].Meteo[n], "meteopgt weather value"))
                        throw new InvalidDataException("CustomInit.txt weather values differ from meteopgt.all at situation " + (i + 1) + ". Reload the weather situations before generating a replacement.");
                for (int n = 4; n < tokens.Length; n++)
                {
                    double value = ParseNumber(tokens[n], "CustomInit situation " + (i + 1));
                    if (value == 0) continue; // Zero is the GRAMM sentinel for the model default.
                    if (n >= 5 && n <= 7) value -= 273.15;
                    if (n == 9) value *= 100;
                    rows[i].Values[n - 4] = value;
                }
                rows[i].Validate();
            }
            return rows;
        }

        public static void SaveCustom(string path, string meteoPath, IList<GrammCustomRow> rows)
        {
            var current = ReadSituations(meteoPath);
            if (rows.Count != current.Count) throw new InvalidDataException("The weather situation count has changed. Reload meteopgt.all.");
            var lines = new List<string> { "GRAMM custom initial conditions; values at sea level", "direction,speed,stability,frequency,snow_m,air_K,surface_K,soil_1m_K,water_delta_K,RH_fraction,inversion_m,air_K_per_m,inversion_K_per_m,soil_K_per_m" };
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i]; row.Validate();
                if (row.Situation != i + 1 || !row.Meteo.SequenceEqual(current[i].Meteo)) throw new InvalidDataException("The weather situation order has changed. Reload meteopgt.all.");
                var values = row.Values.Select((value, n) => Number(!value.HasValue ? 0 : n >= 1 && n <= 3 ? value.Value + 273.15 : n == 5 ? value.Value / 100 : value.Value));
                lines.Add(string.Join(",", row.Meteo.Concat(values)));
            }
            SaveWithBackup(path, lines);
        }

        // Import/export uses explicit situation numbers, not an inferred timestamp mapping.
        public static void ImportCsv(string path, IList<GrammCustomRow> rows)
        {
            string[] lines = File.ReadAllLines(path);
            if (lines.Length < 2) throw new InvalidDataException("The CSV file is empty.");
            char delimiter = lines[0].Contains(";") ? ';' : ',';
            string[] header = lines[0].Split(delimiter).Select(s => s.Trim()).ToArray();
            if (header[0] != "situation") throw new InvalidDataException("The first CSV column must be situation. Export a CSV template for the supported columns.");
            int[] columns = header.Skip(1).Select(s => Array.IndexOf(CustomColumns, s)).ToArray();
            if (columns.Length == 0 || columns.Any(n => n < 0) || columns.Distinct().Count() != columns.Length) throw new InvalidDataException("Unknown or duplicate CSV columns. Export a CSV template.");
            if (lines.Length != rows.Count + 1) throw new InvalidDataException("The CSV must contain exactly one row for each weather situation.");
            var pending = rows.Select(r => (double?[])r.Values.Clone()).ToArray(); var seen = new HashSet<int>();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] data = lines[i].Split(delimiter);
                if (data.Length != header.Length || !int.TryParse(data[0], NumberStyles.None, Invariant, out int number) || number < 1 || number > rows.Count || !seen.Add(number)) throw new InvalidDataException("Invalid or duplicate situation in CSV line " + (i + 1));
                for (int n = 0; n < columns.Length; n++) pending[number - 1][columns[n]] = string.IsNullOrWhiteSpace(data[n + 1]) ? (double?)null : ParseNumber(data[n + 1], "CSV line " + (i + 1));
                new GrammCustomRow { Situation = number, Values = pending[number - 1] }.Validate();
            }
            for (int i = 0; i < rows.Count; i++) rows[i].Values = pending[i];
        }

        public static void ExportCsv(string path, IList<GrammCustomRow> rows)
        {
            var lines = new List<string> { "situation," + string.Join(",", CustomColumns) };
            foreach (var row in rows) { row.Validate(); lines.Add(row.Situation + "," + string.Join(",", row.Values.Select(v => v.HasValue ? Number(v.Value) : ""))); }
            SaveWithBackup(path, lines);
        }

        public static void SaveWithBackup(string path, IEnumerable<string> lines)
        {
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllLines(temporary, lines, new UTF8Encoding(false));
                if (File.Exists(path)) File.Replace(temporary, path, path + "." + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "." + Guid.NewGuid().ToString("N") + ".bak");
                else File.Move(temporary, path);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }

    public sealed class GrammCustomRow
    {
        public int Situation { get; set; }
        public string[] Meteo { get; set; } = new string[4];
        public double?[] Values { get; set; } = new double?[10];
        public void Validate()
        {
            if (Values.Length != 10) throw new InvalidDataException("Expected ten custom initial-condition columns.");
            for (int n = 0; n < Values.Length; n++)
            {
                if (!Values[n].HasValue) continue;
                double v = Values[n].Value;
                if (double.IsNaN(v) || double.IsInfinity(v) ||
                    ((n == 0 || n == 6) && v <= 0) ||
                    (n >= 1 && n <= 3 && v <= -273.15) ||
                    (n == 5 && (v <= 0 || v > 100)) || (n >= 7 && v == 0))
                    throw new InvalidDataException("Situation " + Situation + ", " + GrammInitialConditions.CustomColumns[n] + ": invalid value. Leave the cell blank to use the model default.");
            }
        }
    }
}
