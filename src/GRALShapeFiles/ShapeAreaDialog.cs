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
using System.Globalization;
using System.Windows.Forms;
using GralStaticFunctions;
using Gral;
using GralDomain;
using GralItemData;
using System.Collections.Generic;

namespace GralShape
{
    /// <summary>
    /// Dialog to import an item
    /// </summary>
    public partial class ShapeAreaDialog : Form
    {
        private GralDomain.Domain domain = null;
        
        private readonly string ShapeFileName;
        private double areapolygon = 0;  //area of a polygon
        private readonly Deposition[] dep = new Deposition[10];
        private readonly Button[] but1 = new Button[10];                     //Buttons for deposition
        private DataTable dt;
        private readonly CultureInfo ic = CultureInfo.InvariantCulture;
        private readonly string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;  
        private readonly GralData.DomainArea GralDomRect;
        private readonly List<ComboBox> ComboPollNames = new List<ComboBox>();
        private readonly List<ComboBox> ComboColumnNames = new List<ComboBox>();
        private GralMessage.Waitprogressbar WaitDlg;

        /// <summary>
        /// Dialog for the shape import of area sources
        /// </summary>
        /// <param name="d">Domain form reference</param>
        /// <param name="GralDomainRectangle">Bounds of GRAL domain rectangle</param>
        /// <param name="Filename">Filename of Shape File</param>
        public ShapeAreaDialog(GralDomain.Domain d, GralData.DomainArea GralDomainRectangle, string Filename)
        {
            InitializeComponent();
            domain = d;
            GralDomRect = GralDomainRectangle;
            ShapeFileName = Filename;
            
            // init deposition data
            for (int i = 0; i < 10; i++)
            {
                dep[i] = new Deposition(); // initialize Deposition array
                dep[i].init();
            }
            
            int x = 0; int y = comboBox1P.Top - 3 - comboBox6P.Height; int a = comboBox6P.Width + 5;
            for (int nr = 0; nr < 10; nr++)
            {
                if (nr == 5)
                {
                    x = 0;
                    y = comboBox6P.Top - 3 - comboBox6P.Height;
                }
                //Buttons for Deposition
                but1[nr] = new Button
                {
                    Width = comboBox6P.Width,
                    Height = comboBox6P.Height
                };

                int x_b = 0;
                if (x == 0)
                {
                    x_b = comboBox1P.Left;
                }
                else if (x == 1)
                {
                    x_b = comboBox2P.Left;
                }
                else if (x == 2)
                {
                    x_b = comboBox3P.Left;
                }
                else if (x == 3)
                {
                    x_b = comboBox4P.Left;
                }
                else
                {
                    x_b = comboBox5P.Left;
                }

                but1[nr].Location = new System.Drawing.Point(x_b, y);
                but1[nr].Font = new System.Drawing.Font("Microsoft Sans Serif", 2.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Millimeter, ((byte)(0)));
                but1[nr].Text = "Deposition " + (nr + 1).ToString();
                Controls.Add(but1[nr]);
                toolTip1.SetToolTip(but1[nr], "Click to set deposition - green: deposition set");
                but1[nr].Click += new EventHandler(Edit_deposition);
                x++;
            }		
        }

        //add area data
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex != 0 && comboBox4.SelectedItem != null)
            {
                string name = "Area";
                float height = 0;
                float vert = 0;
                float raster = 0.5F;
                int   sgroup =  1;
                
                WaitDlg = new GralMessage.Waitprogressbar("Import data base");
                WaitDlg.Show();
                
                bool inside;
                GralData.DouglasPeucker douglas = new GralData.DouglasPeucker();
            
                int SHP_Line = 0;
                ShapeReader shape = new ShapeReader(domain);
                //for (int i = 0; i < SHPPolygons[index].Count; i++)
                foreach (object shp in shape.ReadShapeFile(ShapeFileName))
                {
                    if (shp is SHPPolygon)
                    {
                        SHPPolygon polygons = (SHPPolygon) shp;
                        
                        inside = false;
                        int numbpt = polygons.Points.Length;
                        GralDomain.PointD[] pt = new GralDomain.PointD[numbpt];
                        //add only areas inside the GRAL domain
                        for (int j = 0; j < numbpt; j++)
                        {
                            pt[j].X = Math.Round(polygons.Points[j].X, 1);
                            pt[j].Y = Math.Round(polygons.Points[j].Y, 1);
                            if ((pt[j].X > GralDomRect.West) && (pt[j].X < GralDomRect.East) && (pt[j].Y > GralDomRect.South) && (pt[j].Y < GralDomRect.North))
                            {
                                inside = true;
                            }
                        }
                        if (inside == true && numbpt > 2)
                        {
                            AreaSourceData _as = new AreaSourceData();
                            //check for names
                            if (comboBox3.SelectedIndex != 0)
                            {
                                name = Convert.ToString(dt.Rows[SHP_Line][Convert.ToString(comboBox3.SelectedItem)]).Trim();
                                if (name.Length < 1) // a name is needed
                                {
                                    name = "Area " + Convert.ToString(SHP_Line);
                                }
                            }
                            else
                            {
                                name = "Area " + Convert.ToString(SHP_Line);
                            }

                            _as.Name = name;
                            
                            //check for heights
                            if (comboBox1.SelectedIndex != 0)
                            {
                                height = Convert.ToSingle(St_F.TxtToDbl(dt.Rows[SHP_Line][Convert.ToString(comboBox1.SelectedItem)].ToString(), false));
                                height = (float) (Math.Round(height, 1));
                            }
                            else
                            {
                                height = 0;
                            }

                            _as.Height = height;
                            
                            //check for vertical extension
                            if (comboBox2.SelectedIndex != 0)
                            {
                                vert = Convert.ToSingle(St_F.TxtToDbl(dt.Rows[SHP_Line][Convert.ToString(comboBox2.SelectedItem)].ToString(), false));
                                vert = (float) (Math.Round(vert, 1));
                            }
                            else
                            {
                                vert = 0;
                            }

                            _as.VerticalExt = vert;
                            
                            //compute area
                            calc_area(numbpt, pt);
                            _as.Area = (float) Math.Round(areapolygon, 1);
                            
                            //compute raster size
                            raster =  (float) (Math.Max(Math.Round(Math.Sqrt(areapolygon / Convert.ToDouble(numericUpDown1.Value)), 1), 0.5));
                            _as.RasterSize = raster;
                            
                            //check for source group
                            if (comboBox4.SelectedIndex != 0)
                            {
                                string _sgroup = Convert.ToString(dt.Rows[SHP_Line][Convert.ToString(comboBox4.SelectedItem)]);
                                // Plausibility check for source groups
                                int s = 0;
                                if (int.TryParse(_sgroup, out s) == true)
                                {
                                    if (s < 1 || s > 99)
                                    {
                                        sgroup = 1;
                                    }
                                    else
                                    {
                                        sgroup = s;
                                    }
                                }
                                else
                                {
                                    sgroup = 1;
                                }
                            }
                            else
                            {
                                sgroup = 1;
                            }

                            _as.Poll.SourceGroup = sgroup;
                            
                            double emission_factor = Convert.ToDouble(numericUpDown2.Value);
                            if (checkBox2.Checked)
                            {
                                emission_factor = 1 / emission_factor;
                            }


                            //check for 10 pollutants
                            int _count = 0;
                            foreach (ComboBox _combo in ComboPollNames)
                            {
                                if (ComboColumnNames[_count].SelectedIndex != 0)
                                {
                                    _as.Poll.EmissionRate[_count] = St_F.TxtToDbl(dt.Rows[SHP_Line][Convert.ToString(ComboColumnNames[_count].SelectedItem)].ToString(), false) * emission_factor;
                                }
                                _as.Poll.Pollutant[_count] = _combo.SelectedIndex;
                                _count++;
                            }
                            
                            for (int i = 0; i < numbpt - 1; i ++)
                            {
                                _as.Pt.Add(new PointD(pt[i].X, pt[i].Y));
                            }
                            
                            douglas.DouglasPeuckerRun(_as.Pt, (double) numericUpDown3.Value);
                            
                            for (int di = 0; di < 10; di++) // save deposition
                            {
                                _as.GetDep()[di] = dep[di];
                            }
                            
                            domain.EditAS.ItemData.Add(_as);
                            Application.DoEvents();
                        }
                        polygons = null;
                    }
                    SHP_Line ++;
                }
                
                shape = null;
             
                WaitDlg.Close();
                WaitDlg.Dispose();
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(this, "Please define a source group column", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        //read attribute table from dbf file
        private void Shape_Building_Dialog_Load(object sender, EventArgs e)
        {
            //open dbf file
            Cursor = Cursors.WaitCursor;
            GralMessage.Waitprogressbar wait = new GralMessage.Waitprogressbar("Import data base");
            wait.Show();

            ParseDBF dbf_reader = new ParseDBF
            {
                dt = dt
            };
            dbf_reader.StartPosition = FormStartPosition.Manual;
            dbf_reader.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
            dbf_reader.Top = GralStaticFunctions.St_F.GetTopScreenAtMousePosition() + 150;
            dbf_reader.ReadDBF(ShapeFileName.Replace(".shp", ".dbf"));
            dt = dbf_reader.dt;

            ShapeImport_AddColumn.SetDataGridViewSource(dataGridView1, dt, ShowEntireDataset.Checked);
            
            dbf_reader.Close();
            dbf_reader.Dispose();
            wait.Close();
            wait.Dispose();
            Cursor = Cursors.Default;

            //fill combobox with row names
            ComboPollNames.Add(comboBox1P);
            ComboPollNames.Add(comboBox2P);
            ComboPollNames.Add(comboBox3P);
            ComboPollNames.Add(comboBox4P);
            ComboPollNames.Add(comboBox5P);
            ComboPollNames.Add(comboBox6P);
            ComboPollNames.Add(comboBox7P);
            ComboPollNames.Add(comboBox8P);
            ComboPollNames.Add(comboBox9P);
            ComboPollNames.Add(comboBox10P);

            ComboColumnNames.Add(comboBox1E);
            ComboColumnNames.Add(comboBox2E);
            ComboColumnNames.Add(comboBox3E);
            ComboColumnNames.Add(comboBox4E);
            ComboColumnNames.Add(comboBox5E);
            ComboColumnNames.Add(comboBox6E);
            ComboColumnNames.Add(comboBox7E);
            ComboColumnNames.Add(comboBox8E);
            ComboColumnNames.Add(comboBox9E);
            ComboColumnNames.Add(comboBox10E);
            ComboColumnNames.Add(comboBox1);
            ComboColumnNames.Add(comboBox2);
            ComboColumnNames.Add(comboBox3);
            ComboColumnNames.Add(comboBox4);
            
            foreach (ComboBox _combo in ComboPollNames)
            {
                for (int i = 0; i < Main.PollutantList.Count; i++)
                {
                    _combo.Items.Add(Main.PollutantList[i]);
                }
                _combo.SelectedIndex = 0;
            }

            foreach (ComboBox _combo in ComboColumnNames)
            {
                _combo.Items.Add("None");
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    _combo.Items.Add(dataGridView1.Columns[i].HeaderText);
                }
                _combo.SelectedIndex = 0;
            }

            // disable sort!			
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        //add column
        private void button2_Click(object sender, EventArgs e)
        {
            using (ShapeImport_AddColumn shpimport = new ShapeImport_AddColumn())
            {
                shpimport.Left = St_F.GetScreenAtMousePosition() + 40;
                if (shpimport.ShowDialog() == DialogResult.OK)
                {
                    if (shpimport.AddColumn(dt, dataGridView1, shpimport.ColumnName, shpimport.Equation, ShowEntireDataset.Checked))
                    {
                        string column = shpimport.ColumnName;
                        foreach (ComboBox _combo in ComboColumnNames)
                        {
                            _combo.Items.Add(column);
                        }
                    }
                }
            }

            // disable sort!			
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        //delete column
        private void button3_Click(object sender, EventArgs e)
        {
            using (ShapeImport_AddColumn shpimport = new ShapeImport_AddColumn())
            {
                string header = shpimport.RemoveColumn(dt, dataGridView1, ShowEntireDataset.Checked);
                if (!string.IsNullOrEmpty(header))
                {
                    try
                    {
                        foreach (ComboBox _combo in ComboColumnNames)
                        {
                            _combo.Items.Remove(header);
                        }
                    }
                    catch
                    { }
                }
            }
        }
        
        //compute area of a polygon
        private void calc_area(int numpoints, GralDomain.PointD[] polypoints)
        {
            if (numpoints > 2)
            {
                areapolygon = 0;
                try
                {
                  for (int i = 0; i < numpoints-1; i++)
                  {
                        areapolygon = areapolygon + (polypoints[i + 1].X - polypoints[i].X) * polypoints[i].Y +
                            (polypoints[i + 1].X - polypoints[i].X) * (polypoints[i + 1].Y - polypoints[i].Y) / 2;
                  }
                }
                catch{}
                areapolygon = Math.Abs(areapolygon);
            }
        }
        
        void LabelMouseDoubleClick(object sender, MouseEventArgs e)
        {
            //MessageBox.Show(this, sender.ToString());
            Edit_deposition(sender, e);
        }
        
        private void Edit_deposition(object sender, EventArgs e)
        {
            int nr = -1;
            for (int i = 0; i < 10; i++)
            {
                if (sender == but1[i]) // found the button
                {
                    nr = i;
                }
            }
            if (nr >= 0)
            {
                GralItemForms.EditDeposition edit = new GralItemForms.EditDeposition
                {
                    Dep = dep[nr], // set actual values
                    Emission = 0
                };
                if (nr == 0)
                {
                    edit.Pollutant = comboBox1P.SelectedIndex;
                }
                else if (nr == 1)
                {
                    edit.Pollutant = comboBox2P.SelectedIndex;
                }
                else if (nr == 2)
                {
                    edit.Pollutant = comboBox3P.SelectedIndex;
                }
                else if (nr == 3)
                {
                    edit.Pollutant = comboBox4P.SelectedIndex;
                }
                else if (nr == 4)
                {
                    edit.Pollutant = comboBox5P.SelectedIndex;
                }
                else if (nr == 5)
                {
                    edit.Pollutant = comboBox6P.SelectedIndex;
                }
                else if (nr == 6)
                {
                    edit.Pollutant = comboBox7P.SelectedIndex;
                }
                else if (nr == 7)
                {
                    edit.Pollutant = comboBox8P.SelectedIndex;
                }
                else if (nr == 8)
                {
                    edit.Pollutant = comboBox9P.SelectedIndex;
                }
                else if (nr == 9)
                {
                    edit.Pollutant = comboBox10P.SelectedIndex;
                }

                if (edit.ShowDialog() == DialogResult.OK)
                {
                    edit.Hide();
                }

                edit.Dispose();
                
                if (dep[nr].V_Dep1 > 0 || dep[nr].V_Dep2 > 0 || dep[nr].V_Dep3 > 0)
                {
                    but1[nr].BackColor = Color.LightGreen; // mark that deposition is set
                }
                else
                {
                    but1[nr].BackColor = SystemColors.ButtonFace;
                }
            }
        }
        
        void Shape_Area_DialogFormClosed(object sender, FormClosedEventArgs e)
        {
            foreach(Button but in but1)
            {
                but.Click -= new EventHandler(Edit_deposition);
            }

            if (dataGridView1 != null)
            {
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
            }

#if __MonoCS__
#else
            if (dt != null)
            {
                dt.Clear();
                DataRowCollection itemColumns = dt.Rows; // mark elements as deleted
                for (int i = itemColumns.Count - 1; i >= 0; i--)
                {
                    itemColumns[i].Delete();
                }
                dt.AcceptChanges(); // accept deletion
                dt.Clear(); // clear data table
            }
#endif

            if (dt != null)
            {
                dt.Dispose();
            }

            if (dataGridView1 != null)
            {
                dataGridView1.Dispose(); // Kuntner
            }

            dataGridView1 = null;
            ComboColumnNames.Clear();
            ComboPollNames.Clear();
            if (WaitDlg != null)
            {
                WaitDlg.Close();
                WaitDlg.Dispose();
            }
        }

        private void ShowEntireDataset_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            ShapeImport_AddColumn.SetDataGridViewSource(dataGridView1, dt, ShowEntireDataset.Checked);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}