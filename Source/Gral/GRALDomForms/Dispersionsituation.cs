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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gral;
using GralIO;

namespace GralDomForms
{
    /// <summary>
    /// Select a dispersion situation
    /// </summary>
    public partial class SelectDispersionSituation : Form
    {
        private readonly GralDomain.Domain domain = null;
        private readonly Main form1 = null;
        private DataTable _data;
        public int selected_situation = 0;
        public int selectGRAMM_GRAL = 0;
        public string GrammPath = string.Empty;
        public string SCLPath = string.Empty;
        public string GRZPath = string.Empty;
        public string GFFPath = string.Empty;
        private bool transient = false;

        public SelectDispersionSituation (GralDomain.Domain d, Main f)
        {
            InitializeComponent ();
            domain = d;
            form1 = f;
            button1.DialogResult = DialogResult.OK;
            button2.DialogResult = DialogResult.Cancel;
        }

        private void Dispersionsituation_Load (object sender, EventArgs e)
        {
            if (selectGRAMM_GRAL == 0)
            {
                groupBox1.Visible = false;
                if (GrammPath != string.Empty || SCLPath != string.Empty)
                {
                    radioButton1.Checked = true;
                }
                else if (GFFPath != string.Empty || GRZPath != string.Empty)
                {
                    radioButton2.Checked = true;
                }
            }
            else if (selectGRAMM_GRAL == 1) // GRAMM
            {
                groupBox1.Visible = true;
                radioButton1.Checked = true;
            }
            else if (selectGRAMM_GRAL == 2) // GRAL only
            {
                groupBox1.Visible = true;
                radioButton1.Visible = false;
                radioButton2.Checked = true;
            }

            //in case of steady-state simulations meteopgt.all is used in transient simulations mettimeseries.dat
            
            try 
            {
                InDatVariables data = new InDatVariables ();
                InDatFileIO ReadInData = new InDatFileIO ();
                data.InDatPath = Path.Combine (Main.ProjectName, "Computation", "in.dat");
                ReadInData.Data = data;
                if (ReadInData.ReadInDat () == true && form1.checkBox19.Checked == true) // Transient mode & no classification 
                {
                    if (data.Transientflag == 0) 
                    {
                        transient = true;
                    }
                }
            } 
            catch { }
            
            if (transient)
            {
                //fill Listbox
                _data = new DataTable ();
                _data.Columns.Add ("Number", typeof (string));
                _data.Columns.Add ("Date", typeof (string));
                _data.Columns.Add ("Hour", typeof (string));
                _data.Columns.Add ("Wind speed", typeof (string));
                _data.Columns.Add ("Wind sector", typeof (string));
                _data.Columns.Add ("Stability class", typeof (string));
            }
            else
            {
                //fill Listbox
                _data = new DataTable ();
                _data.Columns.Add ("Number", typeof (string));
                _data.Columns.Add ("Wind sector", typeof (string));
                _data.Columns.Add ("Wind speed", typeof (string));
                _data.Columns.Add ("Stability class", typeof (string));
                _data.Columns.Add ("Frequency [1/1000]", typeof (string));
            }

            AddGridViewEntries();
        }

        private void AddGridViewEntries()
        {
            dataGridView1.SelectionChanged -= new System.EventHandler(DataGridView1SelectionChanged);
            dataGridView1.DataSource = null;
            if (_data != null)
            {
                _data.Clear();
            }

            string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

            int selectentries = 0;
            if (radioButton1.Checked && !string.IsNullOrEmpty(GrammPath))
            {
                selectentries = 1;
                textBox1.Text = GrammPath;
            }
            if (radioButton2.Checked && !string.IsNullOrEmpty(GFFPath))
            {
                selectentries = 2;
                textBox1.Text = GFFPath;
            }
            if (radioButton1.Checked && !string.IsNullOrEmpty(SCLPath))
            {
                selectentries = 3;
                textBox1.Text = SCLPath;
            }
            if (radioButton2.Checked && !string.IsNullOrEmpty(GRZPath))
            {
                selectentries = 4;
                textBox1.Text = GRZPath;
            }
            
            //read file meteopgt.all
            if (transient == false)
            {
                string ggeom_file = Path.Combine(Main.ProjectName, @"Computation", "meteopgt.all");
                if (File.Exists(ggeom_file) == false)
                {
                    MessageBox.Show(this, "Unable to open meteopgt.all", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    Close();
                }
                try
                {
                    using (StreamReader myreader = new StreamReader(ggeom_file))
                    {
                        string[] text = new string[5];
                        int count = 0;
                        text = myreader.ReadLine().Split(new char[] { ' ', ';', ',', '\t' });
                        text = myreader.ReadLine().Split(new char[] { ' ', ';', ',', '\t' });
                        text = myreader.ReadLine().Split(new char[] { ' ', ';', ',', '\t' });

                        while (text[0] != "")
                        {
                            if (text.Length > 3)
                            {
                                count = count + 1;

                                // select existing situations
                                if (showSituations.Checked || selectentries == 0 ||
                                                          (selectentries == 1 && File.Exists(Path.Combine(GrammPath, Convert.ToString(count).PadLeft(5, '0') + ".wnd"))) ||
                                                          (selectentries == 2 && File.Exists(Path.Combine(GFFPath, Convert.ToString(count).PadLeft(5, '0') + ".gff"))) ||
                                                          (selectentries == 3 && File.Exists(Path.Combine(SCLPath, Convert.ToString(count).PadLeft(5, '0') + ".scl"))) ||
                                                          (selectentries == 4 && File.Exists(Path.Combine(GRZPath, Convert.ToString(count).PadLeft(5, '0') + ".grz"))))
                                {
                                    DataRow workrow;
                                    workrow = _data.NewRow();
                                    workrow[0] = count.ToString();
                                    float direction = 0;
                                    if (float.TryParse(text[0].Replace(".", decsep), out direction))
                                    {
                                        workrow[1] = (direction * 10).ToString();
                                    }
                                    else
                                    {
                                        workrow[1] = text[0];
                                    }
                                    workrow[2] = text[1].Replace(".", decsep);
                                    workrow[3] = text[2].Replace(".", decsep);
                                    workrow[4] = text[3].Replace(".", decsep);
                                    _data.Rows.Add(workrow);
                                }
                            }
                            try
                            {
                                text = myreader.ReadLine().Split(new char[] { ' ', ';', ',', '\t' });
                            }
                            catch
                            {
                                break;
                            }
                        }
                    }
                }
                catch { }
            }
            else
            {
                string ggeom_file = Path.Combine(Main.ProjectName, @"Computation", "mettimeseries.dat");
                if (File.Exists(ggeom_file) == false)
                {
                    MessageBox.Show(this, "Unable to open mettimeseries.dat", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    Close();
                }
                try
                {
                    using (StreamReader myreader = new StreamReader(ggeom_file))
                    {
                        string[] text = new string[5];
                        int count = 0;
                        text = myreader.ReadLine().Split(new char[] { ' ', ';', ',', '\t' });
                        while (text[0] != "")
                        {
                            if (text.Length > 3)
                            {
                                count = count + 1;

                                // select existing situations
                                if (showSituations.Checked || selectentries == 0 || 
                                                          (selectentries == 1 && File.Exists(Path.Combine(GrammPath, Convert.ToString(count).PadLeft(5, '0') + ".wnd"))) ||
                                                          (selectentries == 2 && File.Exists(Path.Combine(GFFPath, Convert.ToString(count).PadLeft(5, '0') + ".gff"))) ||
                                                          (selectentries == 3 && File.Exists(Path.Combine(SCLPath, Convert.ToString(count).PadLeft(5, '0') + ".scl"))) ||
                                                          (selectentries == 4 && File.Exists(Path.Combine(GRZPath, Convert.ToString(count).PadLeft(5, '0') + ".grz"))))
                                {
                                    DataRow workrow;
                                    workrow = _data.NewRow();
                                    workrow[0] = count.ToString();

                                    float direction = 0;
                                    if (float.TryParse(text[3].Replace(".", decsep), out direction))
                                    {
                                        workrow[4] = (direction * 10).ToString();
                                    }
                                    else
                                    {
                                        workrow[4] = text[3];
                                    }
                                    workrow[1] = text[0];
                                    workrow[2] = text[1].Replace(".", decsep);
                                    workrow[3] = text[2].Replace(".", decsep);
                                    workrow[5] = text[4].Replace(".", decsep);
                                    _data.Rows.Add(workrow);
                                }
                            }
                            try
                            {
                                text = myreader.ReadLine().Split(new char[] { ' ', ';', ',', '\t' });
                            }
                            catch
                            {
                                break;
                            }
                        }
                    }
                }
                catch { }
            }
            
            if (_data != null && _data.Rows.Count > 0)
            {
                DataView datasorted = new DataView();
                datasorted = new DataView(_data); // create DataView from DataTable
                dataGridView1.DataSource = datasorted; // connect DataView to GridView
                dataGridView1.ColumnHeadersHeight = 26;
                dataGridView1.Columns.Cast<DataGridViewColumn>().ToList().ForEach(f => f.SortMode = DataGridViewColumnSortMode.NotSortable);
                dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                dataGridView1.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView1.Columns[0].Width = 45;
                for (int i = 1; i < 5; i++)
                {
                    dataGridView1.Columns[i].Width = 60;
                    dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                dataGridView1.SelectionChanged += new System.EventHandler(DataGridView1SelectionChanged);
            }
        }

        private void button1_Click (object sender, EventArgs e)
        {
            try {
#if __MonoCS__
#else
                selected_situation = Convert.ToInt32 (dataGridView1.Rows [dataGridView1.SelectedCells [0].RowIndex].Cells [0].Value);
#endif
                if (radioButton1.Checked)
                    selectGRAMM_GRAL = 1;
                else if (radioButton2.Checked)
                    selectGRAMM_GRAL = 2;
                else
                    selectGRAMM_GRAL = 0;
                //this.Close();
            } 
            catch
            {
                MessageBox.Show (this, "No situation selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //domain.dissit = 0;
                selected_situation = 0;
            }
#if __MonoCS__
#else
            dataGridView1.DataSource = null;

            dataGridView1.Rows.Clear();
            _data.Dispose();
            _data = null;
            dataGridView1.Dispose();
#endif
        }

        void DispersionsituationResizeEnd (object sender, EventArgs e)
        {
            dataGridView1.Width = ClientSize.Width - 5;
            dataGridView1.Height = Math.Max (10, label1.Top - 5);
            textBox1.Width = Math.Max(5, ClientSize.Width - textBox1.Left  - 15);
        }

        void button2_Click (object sender, EventArgs e) // Cancel
        {
            selected_situation = 0;
#if __MonoCS__
#else
            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            _data.Dispose();
            _data = null;
            dataGridView1.Dispose();
#endif
        }

        // Change the selection of the datagridview for LINUX compatibility
        void DataGridView1SelectionChanged (object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0) // no rows selected
            {
                //selected_situation = 0;
            } 
            else
            {
                selected_situation = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);
            }         
        }

        void DispersionsituationFormClosed(object sender, FormClosedEventArgs e)
        {
            if (dataGridView1.DataSource != null)
                dataGridView1.DataSource = null;
            if (_data != null)
                _data.Dispose();
            _data = null;
            if (dataGridView1 != null)
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Dispose();
            }
        }
            
        void DataGridView1RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            selected_situation = dataGridView1.SelectedRows[0].Index + 1;
            button1_Click(null, null);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            AddGridViewEntries();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            //AddGridViewEntries();
        }

        private void showSituations_CheckedChanged(object sender, EventArgs e)
        {
            AddGridViewEntries();
        }
    }
}