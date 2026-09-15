using GralIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Gral.GralMainForms
{
    public sealed class GrammInitialConditionsDialog : Form
    {
        private readonly string computation;
        private readonly bool era5;
        private readonly double gralLatitude;
        private readonly Dictionary<string, TextBox> inputs = new Dictionary<string, TextBox>();
        private readonly DataGridView grid = new DataGridView();
        private readonly Label status = new Label();
        private readonly Label customStatus = new Label();
        private List<GrammCustomRow> rows = new List<GrammCustomRow>();
        private string IinPath => Path.Combine(computation, "IIN.dat");
        private string MeteoPath => Path.Combine(computation, "meteopgt.all");
        private string CustomPath => Path.Combine(computation, "CustomInit.txt");

        public GrammInitialConditionsDialog(string computationPath, double latitude, bool useEra5)
        {
            computation = computationPath; gralLatitude = latitude; era5 = useEra5;
            Text = "GRAMM initial conditions"; Name = "GrammInitialConditionsDialog";
            Font = new Font("Segoe UI", 10); AutoScaleMode = AutoScaleMode.Font;
            StartPosition = FormStartPosition.CenterParent; MinimumSize = new Size(850, 620); Size = new Size(1050, 740);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), RowCount = 3, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            var tabs = new TabControl { Dock = DockStyle.Fill, Name = "ConditionsTabs" };
            var initial = new TabPage("Initial values"); var custom = new TabPage("CustomInit by weather situation"); tabs.TabPages.Add(initial); tabs.TabPages.Add(custom);
            layout.Controls.Add(tabs, 0, 0);
            status.Dock = DockStyle.Fill; status.AutoEllipsis = true; status.Name = "SaveStatus"; layout.Controls.Add(status, 0, 1);
            var close = new Button { Text = "Close", AutoSize = true, Dock = DockStyle.Right, DialogResult = DialogResult.Cancel };
            layout.Controls.Add(close, 0, 2); CancelButton = close; Controls.Add(layout);
            BuildInitial(initial); BuildCustom(custom);
            LoadInitial(); TryAction(LoadCustom);
        }

        private void BuildInitial(TabPage page)
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 2, Padding = new Padding(16) };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 410)); panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var note = new Label { AutoSize = true, MaximumSize = new Size(910, 0), Margin = new Padding(0, 0, 0, 18), Text = "GRAMM has its own latitude. Temperatures below are reference values at sea level.\nWith meteopgt.all, GRAMM derives temperature and humidity from stability settings.\nUse the CustomInit tab to override those derived values. ERA5 controls its own atmospheric profiles." };
            panel.Controls.Add(note, 0, 0); panel.SetColumnSpan(note, 2);
            string[,] fields = { { "Latitude", "GRAMM latitude [degrees north; south is negative]" }, { "AirTemperatureC", "Air temperature at 2 m, sea-level reference [°C]" }, { "HumidityPercent", "Relative humidity [%; greater than 0]" }, { "SurfaceTemperatureC", "Surface temperature, sea-level reference [°C]" }, { "SoilTemperatureC", "Soil temperature at 1 m depth, sea-level reference [°C]" }, { "NeutralHeight", "Neutral-layer height above ground [m]" }, { "TemperatureGradient", "IIN temperature gradient [K/m]" } };
            for (int i = 0; i < fields.GetLength(0); i++)
            {
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                panel.Controls.Add(new Label { Text = fields[i, 1], AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 8, 8) }, 0, i + 1);
                var box = new TextBox { Name = fields[i, 0], Width = 130, Anchor = AnchorStyles.Left }; inputs.Add(fields[i, 0], box); panel.Controls.Add(box, 1, i + 1);
            }
            var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 18, 0, 0) };
            buttons.Controls.Add(ActionButton("Use GRAL latitude", () => inputs["Latitude"].Text = GrammInitialConditions.Number(gralLatitude)));
            buttons.Controls.Add(ActionButton("Save initial values", () => { GetInitial().SaveIin(IinPath); status.Text = "Saved IIN.dat. The previous file was kept as a .bak file."; }));
            panel.Controls.Add(buttons, 0, 8); panel.SetColumnSpan(buttons, 2);
            panel.Controls.Add(new Label { Text = "Changes to the GRAL latitude do not overwrite an existing GRAMM latitude.\nSaving these values does not change an existing CustomInit.txt. Decimal separator: point.", AutoSize = true, MaximumSize = new Size(910, 0), Margin = new Padding(0, 18, 0, 0) }, 0, 9); panel.SetColumnSpan(panel.GetControlFromPosition(0, 9), 2);
            page.Controls.Add(panel);
        }

        private Button ActionButton(string text, Action action)
        {
            var button = new Button { Text = text, AutoSize = true, MinimumSize = new Size(90, 32), Margin = new Padding(3) };
            button.Click += (s, e) => TryAction(action); return button;
        }
        private void TryAction(Action action)
        {
            try { action(); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is FormatException || ex is ArgumentException || ex is IndexOutOfRangeException)
            { status.Text = "Not saved: " + ex.Message; MessageBox.Show(this, ex.Message, "GRAMM initial conditions", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
        private void LoadInitial()
        {
            var values = GrammInitialConditions.ReadIin(IinPath, gralLatitude);
            foreach (var entry in inputs) entry.Value.Text = GrammInitialConditions.Number((double)typeof(GrammInitialConditions).GetProperty(entry.Key).GetValue(values));
        }
        private GrammInitialConditions GetInitial()
        {
            var values = new GrammInitialConditions();
            foreach (var entry in inputs) typeof(GrammInitialConditions).GetProperty(entry.Key).SetValue(values, GrammInitialConditions.ParseNumber(entry.Value.Text, entry.Key));
            values.Validate(); return values;
        }
        private void BuildCustom(TabPage page)
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(8) };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "One row per meteopgt.all weather situation, starting at 1. Hourly input requires one weather situation per hour in the same order.\nThis file sets initial conditions; it does not continuously force temperature or humidity during a simulation.\nBlank cells use GRAMM defaults. Enter temperatures in °C and humidity in %. The saved file uses K and a humidity fraction.\nScroll horizontally for snow, inversion and gradient settings. CSV import accepts explicit situation numbers." }, 0, 0);
            var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
            actions.Controls.Add(ActionButton("Reload weather situations", () => { rows = GrammInitialConditions.ReadSituations(MeteoPath); DisplayRows(); status.Text = "Loaded weather situations. Blank draft; CustomInit.txt has not changed."; }));
            actions.Controls.Add(ActionButton("Fill from initial values", () =>
            {
                var v = GetInitial(); ReadGrid();
                foreach (var row in rows) { row.Values[1] = v.AirTemperatureC; row.Values[2] = v.SurfaceTemperatureC; row.Values[3] = v.SoilTemperatureC; row.Values[5] = v.HumidityPercent; }
                DisplayRows(); status.Text = "Filled the draft table. Save CustomInit.txt to apply it.";
            }));
            actions.Controls.Add(ActionButton("Import CSV", ImportCsv)); actions.Controls.Add(ActionButton("Export CSV template", ExportCsv));
            actions.Controls.Add(ActionButton("Load CustomInit file", () =>
            {
                using (var dialog = new OpenFileDialog { Filter = "CustomInit files|*.txt;*.disabled;*.bak|All files|*.*", InitialDirectory = computation })
                    if (dialog.ShowDialog(this) == DialogResult.OK) { rows = GrammInitialConditions.ReadCustom(dialog.FileName, MeteoPath); DisplayRows(); status.Text = "Loaded the draft. Save CustomInit.txt to apply it."; }
            }));
            var save = ActionButton("Save CustomInit.txt", SaveCustom); save.Name = "SaveCustomInit"; save.Enabled = !era5; actions.Controls.Add(save);
            actions.Controls.Add(ActionButton("Disable CustomInit", () =>
            {
                if (File.Exists(CustomPath)) File.Move(CustomPath, CustomPath + "." + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + ".disabled");
                UpdateCustomStatus(); status.Text = "CustomInit.txt is disabled. The previous file is retained; use Load CustomInit file to restore it.";
            }));
            layout.Controls.Add(actions, 0, 1);
            grid.Dock = DockStyle.Fill; grid.Name = "CustomConditionsGrid"; grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false;
            grid.RowHeadersVisible = false; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            grid.SelectionMode = DataGridViewSelectionMode.CellSelect; grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            string[] labels = { "Situation", "Direction\n[10°]", "Wind\n[m/s]", "Stability", "Snow above\n[m ASL]", "Air at 2 m\n[°C, sea level]", "Surface\n[°C, sea level]", "Soil at 1 m\n[°C, sea level]", "Water delta\n[K]", "Humidity\n[%]", "Inversion\n[m above minimum terrain]", "Air gradient\n[K/m]", "Inversion gradient\n[K/m]", "Soil gradient\n[K/m]" };
            for (int i = 0; i < labels.Length; i++)
            {
                var col = new DataGridViewTextBoxColumn { HeaderText = labels[i], Name = "Field" + i, Width = i < 4 ? 85 : 132, ReadOnly = i < 4, SortMode = DataGridViewColumnSortMode.NotSortable };
                if (i < 4) col.DefaultCellStyle.BackColor = SystemColors.Control;
                grid.Columns.Add(col);
            }
            int[] displayOrder = { 0, 1, 2, 3, 5, 9, 6, 7, 8, 4, 10, 11, 12, 13 };
            for (int i = 0; i < displayOrder.Length; i++) grid.Columns[displayOrder[i]].DisplayIndex = i;
            grid.ColumnHeadersHeight = 62; grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            layout.Controls.Add(grid, 0, 2); customStatus.Dock = DockStyle.Fill; customStatus.Name = "CustomFileStatus"; layout.Controls.Add(customStatus, 0, 3);
            if (era5) status.Text = "ERA5 forcing is active. CustomInit generation is unavailable because it does not provide hourly ERA5 boundary forcing.";
            page.Controls.Add(layout);
        }
        private void LoadCustom()
        {
            if (File.Exists(MeteoPath)) rows = File.Exists(CustomPath) ? GrammInitialConditions.ReadCustom(CustomPath, MeteoPath) : GrammInitialConditions.ReadSituations(MeteoPath);
            DisplayRows(); UpdateCustomStatus();
        }
        private void UpdateCustomStatus() => customStatus.Text = (File.Exists(CustomPath) ? "CustomInit.txt is active. " : "CustomInit.txt is absent. ") + rows.Count + " weather situations in the draft.";
        private void DisplayRows()
        {
            grid.Rows.Clear();
            foreach (var row in rows)
            {
                object[] values = new object[14]; values[0] = row.Situation;
                for (int i = 0; i < 3; i++) values[i + 1] = row.Meteo[i];
                for (int i = 0; i < 10; i++) values[i + 4] = row.Values[i].HasValue ? GrammInitialConditions.Number(row.Values[i].Value) : "";
                grid.Rows.Add(values);
            }
            UpdateCustomStatus();
        }
        private void ReadGrid()
        {
            grid.EndEdit();
            var pending = new List<double?[]>();
            for (int i = 0; i < rows.Count; i++)
            {
                var values = new double?[10];
                for (int j = 0; j < 10; j++)
                {
                    string text = Convert.ToString(grid.Rows[i].Cells[j + 4].Value).Trim();
                    values[j] = text.Length == 0 ? (double?)null : GrammInitialConditions.ParseNumber(text, "Situation " + (i + 1) + ", " + GrammInitialConditions.CustomColumns[j]);
                }
                new GrammCustomRow { Situation = i + 1, Values = values }.Validate(); pending.Add(values);
            }
            for (int i = 0; i < rows.Count; i++) rows[i].Values = pending[i];
        }
        private void SaveCustom()
        {
            ReadGrid(); GrammInitialConditions.SaveCustom(CustomPath, MeteoPath, rows); UpdateCustomStatus();
            status.Text = "Saved CustomInit.txt. Existing wind fields are unchanged; run GRAMM in a fresh computation folder to generate new fields.";
        }
        private void ImportCsv()
        {
            using (var dialog = new OpenFileDialog { Filter = "CSV files|*.csv|All files|*.*", InitialDirectory = computation })
                if (dialog.ShowDialog(this) == DialogResult.OK) { ReadGrid(); GrammInitialConditions.ImportCsv(dialog.FileName, rows); DisplayRows(); status.Text = "Imported the draft. Save CustomInit.txt to apply it."; }
        }
        private void ExportCsv()
        {
            using (var dialog = new SaveFileDialog { Filter = "CSV files|*.csv", FileName = "GRAMM_initial_conditions.csv", InitialDirectory = computation })
                if (dialog.ShowDialog(this) == DialogResult.OK) { ReadGrid(); GrammInitialConditions.ExportCsv(dialog.FileName, rows); status.Text = "Exported a CSV template in °C and %."; }
        }
    }
}
