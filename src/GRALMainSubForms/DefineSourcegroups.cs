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
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Data;
using Gral;

namespace GralMainForms
{
    /// <summary>
    /// Dialog to set user defined source group names
    /// </summary>
    public partial class Sourcegroups : Form
    {
        private readonly Main form1 = null;

        public Sourcegroups(Main f)
        {
            InitializeComponent();
            form1 = f;
        }

        //load existing sourcegroup definitions
        private void Sourcegroups_Load(object sender, EventArgs e)
        {
            string[] _sgname = new string[101];
            for (int i = 0; i < 101; i++)
            {
                _sgname[i] = string.Empty;
            }

            try
            {
                string newPath = Path.Combine(Main.ProjectName, @"Settings", "Sourcegroups.txt");
                if (File.Exists(newPath))
                {
                    using (StreamReader myReader = new StreamReader(newPath))
                    {
                        string[] text = new string[2];
                        string text1;
                        while (myReader.EndOfStream == false)
                        {
                            text1 = myReader.ReadLine();
                            text = text1.Split(new char[] { ',' });

                            if (text.Length > 1)
                            {
                                // Plausibility check for source groups
                                int s = 0;
                                if (int.TryParse(text[1], out s) == true)
                                {
                                    s = Math.Max(1, Math.Min(99, s));
                                    _sgname[s] = text[0];
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show(this, "Error when reading file", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            DataTable _data = new DataTable();
            _data.Columns.Add("Number", typeof(int));
            _data.Columns.Add("Name", typeof(string));
            for (int i = 1; i < 100; i++)
            {
                DataRow workrow;
                workrow = _data.NewRow();
                workrow[0] = i;
                workrow[1] = _sgname[i];
                _data.Rows.Add(workrow);
            }

            DataView datasorted = new DataView();
            datasorted = new DataView(_data); // create DataView from DataTable
            dataGridView1.DataSource = datasorted; // connect DataView to GridView
            dataGridView1.Columns["Number"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns["Number"].ReadOnly = true;
            dataGridView1.Columns["Name"].ReadOnly = false;
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Number"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns["Name"].SortMode = DataGridViewColumnSortMode.NotSortable;

            for (int i = 1; i < 100; i++)
            {
                if (string.IsNullOrEmpty(_sgname[i]))
                {
                    dataGridView1.Rows[i - 1].DefaultCellStyle.BackColor = Color.Beige;
                }
            }
            dataGridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataGridView1KeyDown);
        }

        //close the form and save source group definitions
        private void Button3_Click(object sender, EventArgs e)
        {
            //check for invalid names
            for (int i = 0; i < 99; i++)
            {
                if (dataGridView1.Rows[i].Cells[1].Value != null)
                {
                    string a = Convert.ToString(dataGridView1.Rows[i].Cells[1].Value);
                    if (a.Contains("_") || a.Contains(",") || a.Contains(":") || a.Contains(@"/") || a.Contains(@"\") || a.Contains("."))
                    {
                        a = a.Replace("_", "-");
                        a = a.Replace(",", "-");
                        a = a.Replace(":", "-");
                        a = a.Replace(@"/", "-");
                        a = a.Replace(@"\", "-");
                        a = a.Replace(".", " - ");
                        dataGridView1.Rows[i].Cells[1].Value = a;
                    }
                }
            }

            //check for double counting of source group names
            for (int i = 0; i < 99; i++)
            {
                if (dataGridView1.Rows[i].Cells[1].Value != null)
                {
                    string a = Convert.ToString(dataGridView1.Rows[i].Cells[1].Value);
                    string _check_if_empty = a.Replace(" ", string.Empty);

                    if (!string.IsNullOrEmpty(_check_if_empty))
                    {
                        for (int j = i + 1; j < 99; j++)
                        {
                            if (dataGridView1.Rows[j].Cells[1].Value != null)
                            {
                                string b = Convert.ToString(dataGridView1.Rows[j].Cells[1].Value);
                                if (!string.IsNullOrEmpty(b) && a == b) // Change SG Name
                                {
                                    dataGridView1.Rows[j].Cells[1].Value = b + "-";
                                }
                            }
                        }
                    }
                }
            }

            try
            {
                string newPath = Path.Combine(Main.ProjectName, @"Settings", "Sourcegroups.txt");
                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                }

                Main.DefinedSourceGroups.Clear();

                using (StreamWriter myWriter = new StreamWriter(newPath, false))
                {
                    for (int i = 0; i < 99; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[1].Value != null)
                        {
                            string a = Convert.ToString(dataGridView1.Rows[i].Cells[1].Value);
                            string _check_if_empty = a.Replace(" ", string.Empty);

                            if (!string.IsNullOrEmpty(_check_if_empty))
                            {
                                myWriter.WriteLine(Convert.ToString(a) + "," + Convert.ToString(i + 1));
                                Main.DefinedSourceGroups.Add(new SG_Class() { SG_Name = Convert.ToString(a), SG_Number = i + 1 });
                            }
                            else // check if a source group has been deleted
                            {
                                if (dataGridView1.Rows[i].DefaultCellStyle.BackColor != Color.Beige) // there was an entry
                                {
                                    // Remove item from listbox4 in Main()
                                    try
                                    {
                                        string[] _text;
                                        for (int j = 0; j < form1.listBox4.Items.Count; j++)
                                        {
                                            _text = Convert.ToString(form1.listBox4.Items[j]).Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (_text.Length > 1)
                                            {
                                                int s_numb = 0;
                                                if (int.TryParse(_text[1], out s_numb))
                                                {
                                                    if (s_numb == i + 1) // item to remove
                                                    {
                                                        form1.listBox4.Items.RemoveAt(j);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }

                Main.DefinedSourceGroups.Sort();
                //St_F.Sort_Source_Group_File(newPath);

                Close();
            }
            catch
            {
                MessageBox.Show(this, "Error when writing file \"Sourcegroups.txt\" in the directory \"Settings\".", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void SourcegroupsResizeEnd(object sender, EventArgs e)
        {
            dataGridView1.Height = Math.Max(1, button1.Top - 10);
        }

        void Button1Click(object sender, EventArgs e)
        {
            Close();
        }

        //paste text from the clipboard
        void DataGridView1KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Modifiers == Keys.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.C:
                            break;

                        case Keys.V:
                            PasteClipboard();
                            break;
                    }
                }
            }
            catch
            { }
        }
        void PasteClipboard()
        {
            try
            {
                string s = Clipboard.GetText();
                string[] lines = s.Split('\n');

                int iRow = dataGridView1.CurrentCell.RowIndex;
                int iCol = dataGridView1.CurrentCell.ColumnIndex;
                DataGridViewCell oCell;

                foreach (string line in lines)
                {
                    if (iRow < dataGridView1.RowCount && line.Length > 0)
                    {
                        string[] sCells = line.Split('\t');
                        for (int i = 0; i < sCells.GetLength(0); ++i)
                        {
                            if (iCol + i < dataGridView1.ColumnCount)
                            {
                                oCell = dataGridView1[iCol + i, iRow];
                                oCell.Value = Convert.ChangeType(sCells[i].Replace("\r", ""), oCell.ValueType);
                            }
                            else
                            {
                                break;
                            }
                        }
                        iRow++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (FormatException)
            {
                return;
            }
        }

        private void Sourcegroups_FormClosing(object sender, FormClosingEventArgs e)
        {
            dataGridView1.KeyDown -= new System.Windows.Forms.KeyEventHandler(this.DataGridView1KeyDown);
            dataGridView1.Dispose();
        }
    }
}