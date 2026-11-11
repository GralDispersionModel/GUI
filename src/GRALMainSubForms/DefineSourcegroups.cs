#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2019]  [Dietmar Oettl, Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using Gral;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace GralMainForms
{
    public partial class Sourcegroups : Form
    {
        private readonly Main form1;
        private readonly HashSet<int> originalIds = new HashSet<int>();
        private DataTable sourceGroups;

        public Sourcegroups(Main f)
        {
            InitializeComponent();
            form1 = f;
            dataGridView1.AllowUserToAddRows = true;
            dataGridView1.AllowUserToDeleteRows = true;
            dataGridView1.DefaultValuesNeeded += (s, e) => e.Row.Cells[0].Value = NextUnusedId();
            dataGridView1.DataError += (s, e) =>
            {
                MessageBox.Show(this, "Enter a positive whole source group number (up to 2147483647).",
                    "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            };
        }

        private void Sourcegroups_Load(object sender, EventArgs e)
        {
            SortedDictionary<int, string> definitions;
            try
            {
                definitions = SourceGroupCatalog.ReadDefinitions(Path.Combine(Main.ProjectName, "Settings", "Sourcegroups.txt"));
            }
            catch (Exception ex)
            {
                button3.Enabled = false;
                MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            sourceGroups = new DataTable();
            sourceGroups.Columns.Add("Number", typeof(int));
            sourceGroups.Columns.Add("Name", typeof(string));
            foreach (string choice in SourceGroupCatalog.Choices(definitions))
            {
                int id = SourceGroupCatalog.GetNumber(choice);
                sourceGroups.Rows.Add(id, definitions.TryGetValue(id, out string name) ? name : string.Empty);
            }
            originalIds.UnionWith(definitions.Keys);
            dataGridView1.DataSource = sourceGroups.DefaultView;
            dataGridView1.Columns["Number"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns["Number"].ReadOnly = false;
            dataGridView1.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (DataGridViewColumn column in dataGridView1.Columns)
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            foreach (DataGridViewRow row in dataGridView1.Rows)
                if (!row.IsNewRow && string.IsNullOrWhiteSpace(Convert.ToString(row.Cells[1].Value)))
                    row.DefaultCellStyle.BackColor = Color.Beige;
            dataGridView1.KeyDown += DataGridView1KeyDown;
        }

        private int NextUnusedId()
        {
            var used = new HashSet<int>();
            if (sourceGroups != null)
                foreach (DataRow row in sourceGroups.Rows)
                    if (row.RowState != DataRowState.Deleted && row[0] != DBNull.Value) used.Add((int)row[0]);
            int next = 1;
            while (next <= SourceGroupFileName.MaximumId && used.Contains(next)) next++;
            return next;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (!dataGridView1.EndEdit()) return;
            BindingContext[sourceGroups.DefaultView].EndCurrentEdit();
            var definitions = new SortedDictionary<int, string>();
            var names = new HashSet<string>();
            foreach (DataRow row in sourceGroups.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;
                string name = Convert.ToString(row[1]).Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (row[0] == DBNull.Value || !SourceGroupFileName.IsSupported((int)row[0]) || definitions.ContainsKey((int)row[0]))
                {
                    MessageBox.Show(this, "Each named group needs a unique number in 1..1295.",
                        "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                name = name.Replace("_", "-").Replace(",", "-").Replace(":", "-")
                    .Replace("/", "-").Replace(@"\", "-").Replace(".", " - ");
                while (!names.Add(name)) name += "-";
                definitions.Add((int)row[0], name);
            }
            try
            {
                var lines = new List<string>();
                foreach (var group in definitions)
                    lines.Add(group.Value + "," + group.Key.ToString(CultureInfo.InvariantCulture));
                File.WriteAllLines(Path.Combine(Main.ProjectName, "Settings", "Sourcegroups.txt"), lines);
                Main.DefinedSourceGroups.Clear();
                foreach (var group in definitions)
                    Main.DefinedSourceGroups.Add(new SG_Class { SG_Name = group.Value, SG_Number = group.Key });
                for (int i = form1.listBox4.Items.Count - 1; i >= 0; i--)
                {
                    int id = SourceGroupCatalog.GetNumber(Convert.ToString(form1.listBox4.Items[i]));
                    if (originalIds.Contains(id) && !definitions.ContainsKey(id)) form1.listBox4.Items.RemoveAt(i);
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Cannot save Sourcegroups.txt: " + ex.Message,
                    "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void SourcegroupsResizeEnd(object sender, EventArgs e)
        {
            dataGridView1.Height = Math.Max(1, button1.Top - 10);
        }

        void Button1Click(object sender, EventArgs e) { Close(); }

        void DataGridView1KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                PasteClipboard();
                e.Handled = true;
            }
        }

        void PasteClipboard()
        {
            if (dataGridView1.CurrentCell == null || sourceGroups == null) return;
            int rowIndex = dataGridView1.CurrentCell.RowIndex;
            int columnIndex = dataGridView1.CurrentCell.ColumnIndex;
            dataGridView1.EndEdit();
            BindingContext[sourceGroups.DefaultView].EndCurrentEdit();
            try
            {
                foreach (string line in Clipboard.GetText().Split('\n'))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (rowIndex >= sourceGroups.Rows.Count) sourceGroups.Rows.Add(NextUnusedId(), string.Empty);
                    string[] cells = line.TrimEnd('\r').Split('\t');
                    for (int column = 0; column < cells.Length && columnIndex + column < 2; column++)
                    {
                        int targetColumn = columnIndex + column;
                        sourceGroups.Rows[rowIndex][targetColumn] = targetColumn == 0
                            ? (object)int.Parse(cells[column], CultureInfo.InvariantCulture) : cells[column];
                    }
                    rowIndex++;
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                MessageBox.Show(this, "Enter numbers in 1..1295 in the Number column.",
                    "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Sourcegroups_FormClosing(object sender, FormClosingEventArgs e)
        {
            dataGridView1.KeyDown -= DataGridView1KeyDown;
            dataGridView1.Dispose();
        }
    }
}
