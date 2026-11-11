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

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace GralMainForms
{
    /// <summary>
    /// Dialog to set user defined decay rates
    /// </summary>
    public partial class DecayRateForm : Form
    {
        public List<GralData.DecayRates> DecayRate;
        public bool ProjectLocked;
        public List<int> SourceGroups = new List<int>();

        public DecayRateForm()
        {
            InitializeComponent();
        }

        private void DecayRateForm_Load(object sender, EventArgs e)
        {
            DataTable _data = new DataTable();
            _data.Columns.Add("Number", typeof(string));
            _data.Columns.Add("Decay", typeof(double));

            var groups = new SortedSet<int>(SourceGroups);
            for (int id = 1; id <= 99; id++) groups.Add(id);
            foreach (var group in Gral.Main.DefinedSourceGroups) groups.Add(group.SG_Number);
            foreach (var rate in DecayRate) groups.Add(rate.SourceGroup);
            groups.RemoveWhere(id => id <= 0);
            foreach (int i in groups)
            {
                DataRow workrow;
                workrow = _data.NewRow();
                workrow[0] = i.ToString() + ":";
                workrow[1] = 0;
                foreach (GralData.DecayRates dr in DecayRate)
                {
                    if (dr.SourceGroup == i) // this value is defined
                    {
                        workrow[1] = dr.DecayRate;
                    }
                }
                _data.Rows.Add(workrow);
            }
            DataView datasorted = new DataView();
            datasorted = new DataView(_data); // create DataView from DataTable
            dataGridView1.DataSource = datasorted; // connect DataView to GridView
            dataGridView1.Columns["Number"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns["Number"].ReadOnly = true;

            if (ProjectLocked)
            {
                dataGridView1.Columns["Decay"].ReadOnly = true;
            }
            else
            {
                dataGridView1.Columns["Decay"].ReadOnly = false;
            }
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Number"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns["Decay"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns["Decay"].HeaderText = "Decay rate [1/s]";
            dataGridView1.Columns["Decay"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns["Number"].HeaderText = "Source group";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (ProjectLocked == false)
            {
                if (!dataGridView1.EndEdit()) return;
                DecayRate.Clear();
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].IsNewRow) continue;
                    double decay = Convert.ToDouble(dataGridView1.Rows[i].Cells[1].Value);
                    if (decay != 0)
                    {
                        GralData.DecayRates dr = new GralData.DecayRates();
                        dr.SourceGroup = int.Parse(dataGridView1.Rows[i].Cells[0].Value.ToString().TrimEnd(':'));
                        dr.DecayRate = decay;
                        DecayRate.Add(dr);
                    }
                }
            }

            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show(this, "Only numerical inputs are allowed", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            e.Cancel = false;
        }
    }
}
