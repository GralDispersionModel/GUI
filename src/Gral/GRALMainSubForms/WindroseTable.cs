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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GralMainForms
{
    /// <summary>
    /// Show a table for windrose velocity and windrose stability class
    /// </summary>
    public partial class WindroseTable : Form
    {
        public string MetfileName;
        public double[,] SectFrequency = new double[16, 8];
        public double[] WndClasses = new double[7];
        /// <summary>
        /// Wind speed: 0, stability classes: 1
        /// </summary>
        public int Mode;

        /// <summary>
        /// Show table for wind data
        /// </summary>
        public WindroseTable()
        {
            InitializeComponent();
        }

        private void WindroseTable_Load(object sender, EventArgs e)
        {
            double summe = 0;//raw titles
            string[] richtungen = new string[16];

            Text = "Windrose statistics -  " + MetfileName; // Kuntner

            richtungen[0] = "N";
            richtungen[1] = "NNE";
            richtungen[2] = "NE";
            richtungen[3] = "ENE";
            richtungen[4] = "E";
            richtungen[5] = "ESE";
            richtungen[6] = "SE";
            richtungen[7] = "SSE";
            richtungen[8] = "S";
            richtungen[9] = "SSW";
            richtungen[10] = "SW";
            richtungen[11] = "WSW";
            richtungen[12] = "W";
            richtungen[13] = "WNW";
            richtungen[14] = "NW";
            richtungen[15] = "NNW";

            int nmax = 8;
            //fill Listbox
            DataTable _data = new DataTable();
            if (Mode == 0) // Wind speed
            {
                _data.Columns.Add("Dir.", typeof(string));
                _data.Columns.Add("0 - " + Convert.ToString(WndClasses[0]) + " m/s [%]", typeof(string));
                _data.Columns.Add(Convert.ToString(WndClasses[0]) + " - " + Convert.ToString(WndClasses[1]) + " m/s [%]", typeof(string));
                _data.Columns.Add(Convert.ToString(WndClasses[1]) + " - " + Convert.ToString(WndClasses[2]) + " m/s [%]", typeof(string));
                _data.Columns.Add(Convert.ToString(WndClasses[2]) + " - " + Convert.ToString(WndClasses[3]) + " m/s [%]", typeof(string));
                _data.Columns.Add(Convert.ToString(WndClasses[3]) + " - " + Convert.ToString(WndClasses[4]) + " m/s [%]", typeof(string));
                _data.Columns.Add(Convert.ToString(WndClasses[4]) + " - " + Convert.ToString(WndClasses[5]) + " m/s [%]", typeof(string));
                _data.Columns.Add(Convert.ToString(WndClasses[5]) + " - " + Convert.ToString(WndClasses[6]) + " m/s [%]", typeof(string));
                _data.Columns.Add("> " + Convert.ToString(WndClasses[6]) + " m/s [%]", typeof(string));
                _data.Columns.Add("sum [%]", typeof(string));
            }
            else if (Mode == 1) // stability classes
            {
                _data.Columns.Add("Dir.", typeof(string));
                for (int i = 1; i < 8; i++)
                {
                    _data.Columns.Add("SC " + i.ToString() + " [%]", typeof(string));
                }
                _data.Columns.Add("sum [%]", typeof(string));
                nmax = 7;
            }

            double[] sum_classes = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            for (int i = 0; i < 16; i++)
            {
                DataRow workrow;
                workrow = _data.NewRow();
                summe = 0;
                workrow[0] = richtungen[i].ToString();
                int n = 0;
                
                for (; n < nmax; n++)
                {
                    double prozent = 0;
                    if (Mode == 0)
                    {
                        prozent = Math.Round(SectFrequency[i, n] * 100, 1);
                        sum_classes[n] += prozent;
                        summe = summe + SectFrequency[i, n];
                    }
                    else if (Mode == 1)
                    {
                        prozent = Math.Round(SectFrequency[i, n + 1] * 100, 1);
                        sum_classes[n] += prozent;
                        summe = summe + SectFrequency[i, n + 1];
                    }
                workrow[n + 1] = prozent.ToString();
            }
                workrow[++n] = Math.Round(summe * 100, 1).ToString();
                sum_classes[nmax] += summe * 100;
                _data.Rows.Add(workrow);
            }

            // add sum for all rows
            DataRow workrow1;
            workrow1 = _data.NewRow();
            workrow1[0] = "Sum";
            for (int n = 0; n < nmax + 1; n++)
            {
                workrow1[n + 1] = sum_classes[n].ToString();
            }
            _data.Rows.Add(workrow1);

            DataView datasorted = new DataView();
            datasorted = new DataView(_data); // create DataView from DataTable
            dataGridView1.DataSource = datasorted; // connect DataView to GridView
            dataGridView1.ColumnHeadersHeight = 26;
            dataGridView1.Columns.Cast<DataGridViewColumn>().ToList().ForEach(f => f.SortMode = DataGridViewColumnSortMode.NotSortable);
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.Font = new System.Drawing.Font(Font, FontStyle.Bold);   // Set Bold
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
    }
}
