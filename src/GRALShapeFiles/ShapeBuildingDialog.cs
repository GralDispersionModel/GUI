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

using GralDomain;
using GralItemData;
using GralStaticFunctions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Forms;

namespace GralShape
{
    /// <summary>
    /// Dialog to import an item
    /// </summary>
    public partial class ShapeBuildingDialog : Form
    {
        private GralDomain.Domain domain = null;

        private readonly string ShapeFileName;
        private double areapolygon = 0;
        public bool Wall = false;
        private DataTable dt;
        private readonly CultureInfo ic = CultureInfo.InvariantCulture;
        private readonly string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        private readonly GralData.DomainArea GralDomRect;
        private readonly List<ComboBox> ComboColumnNames = new List<ComboBox>();
        private GralMessage.Waitprogressbar WaitDlg;

        /// <summary>
        /// Dialog for the shape import of Buildings and Walls
        /// </summary>
        /// <param name="d">Domain form reference</param>
        /// <param name="GralDomainRectangle">Bounds of GRAL domain rectangle</param>
        /// <param name="Filename">Filename of Shape File</param>
        /// <param name="Wall">true, if Walls should be imported</param>
        public ShapeBuildingDialog(GralDomain.Domain d, GralData.DomainArea GralDomainRectangle, string Filename)
        {
            InitializeComponent();
            domain = d;
            GralDomRect = GralDomainRectangle;
            ShapeFileName = Filename;
        }

        /// <summary>
        /// Add building data to the domain item list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != 0 && comboBox1.SelectedItem != null)
            {
                string name = "Building";
                if (Wall)
                {
                    name = "Wall";
                }
                string dataColumnHeight = null;
                string dataColumnName = null;
                string dataColumnStreet = null;
                string dataColumnNumbers = null;
                if (comboBox1.SelectedIndex != 0)
                {
                    dataColumnHeight = Convert.ToString(comboBox1.SelectedItem);
                }
                if (comboBox3.SelectedIndex != 0)
                {
                    dataColumnName = Convert.ToString(comboBox3.SelectedItem);
                }
                if (comboBox2.SelectedIndex != 0)
                {
                    dataColumnStreet = Convert.ToString(comboBox2.SelectedItem);
                }
                if (comboBox4.SelectedIndex != 0)
                {
                    dataColumnNumbers = Convert.ToString(comboBox4.SelectedItem);
                }

                bool inside;

                WaitDlg = new GralMessage.Waitprogressbar("Import data base");
                WaitDlg.Show();

                GralData.DouglasPeucker douglas = new GralData.DouglasPeucker();

                int SHP_Line = 0;
                ShapeReader shape = new ShapeReader(domain);
                foreach (object shp in shape.ReadShapeFile(ShapeFileName))
                {
                    // Walls
                    if (Wall && shp is SHPLine)
                    {
                        SHPLine lines = (SHPLine)shp;

                        inside = false;
                        int numbpt = lines.Points.Length;
                        List<PointD> pt = new List<PointD>();
                        //add only lines inside the GRAL domain
                        for (int j = 0; j < numbpt; j++)
                        {
                            PointD ptt = new PointD
                            {
                                X = Math.Round(lines.Points[j].X, 1),
                                Y = Math.Round(lines.Points[j].Y, 1)
                            };
                            pt.Add(ptt);

                            if ((pt[j].X > GralDomRect.West) && (pt[j].X < GralDomRect.East) && (pt[j].Y > GralDomRect.South) && (pt[j].Y < GralDomRect.North))
                            {
                                inside = true;
                            }
                        }
                        if (inside == true && numbpt > 1)
                        {
                            WallData _wd = new WallData();

                            douglas.DouglasPeuckerRun(pt, (double)numericUpDown1.Value);

                            //check for height
                            double height = 0;
                            if (!string.IsNullOrEmpty(dataColumnHeight))
                            {
                                try
                                {
                                    if (checkBox1.Checked == true) // absolute heights
                                    {
                                        height = (-1) * Math.Abs(St_F.TxtToDbl(dt.Rows[SHP_Line][dataColumnHeight].ToString(), false));
                                    }
                                    else
                                    {
                                        height = Convert.ToDouble(dt.Rows[SHP_Line][dataColumnHeight], ic);
                                    }

                                    height = Math.Round(height, 1);
                                }
                                catch
                                {
                                    height = 0;
                                }
                            }
                            else
                            {
                                height = 0;
                            }

                            if (checkBox3.Checked && lines.PointsZ != null && lines.PointsZ.Length == pt.Count) // 3D Lines
                            {
                                // 3D Lines
                                int abs = 1;
                                if (checkBox1.Checked == true) // absolute heights
                                {
                                    abs = -1;
                                }

                                int i = 0;
                                foreach (PointD _pt in pt)
                                {
                                    _wd.Pt.Add(new GralData.PointD_3d(_pt.X, _pt.Y, lines.PointsZ[i++] * abs));
                                }
                            }
                            else
                            {
                                foreach (PointD _pt in pt)
                                {
                                    _wd.Pt.Add(new GralData.PointD_3d(_pt.X, _pt.Y, (float)(height)));
                                }
                            }

                            numbpt = pt.Count;

                            //check for names
                            if (!string.IsNullOrEmpty(dataColumnName))
                            {
                                name = Convert.ToString(dt.Rows[SHP_Line][dataColumnName]).Trim();
                                if (name.Length < 1) // a name is needed
                                {
                                    name = "Wall " + Convert.ToString(SHP_Line);
                                }
                            }
                            else
                            {
                                name = "Wall " + Convert.ToString(SHP_Line);
                            }

                            _wd.Name = name;
                            _wd.Lenght = (float)Math.Round(_wd.CalcLenght(), 1);

                            domain.EditWall.ItemData.Add(_wd);
                        }
                        lines = null;
                    }

                    // Load Buildings
                    if (!Wall && shp is SHPPolygon)
                    {
                        SHPPolygon polygons = (SHPPolygon)shp;

                        inside = false;
                        int numbpt = polygons.Points.Length;

                        PointD[] pt = new PointD[numbpt];
                        //add only buildings inside the GRAL domain
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
                            BuildingData _bd = new BuildingData();
                            for (int i = 0; i < numbpt - 1; i++)
                            {
                                _bd.Pt.Add(new PointD(pt[i].X, pt[i].Y));
                            }

                            douglas.DouglasPeuckerRun(_bd.Pt, (double)numericUpDown1.Value);

                            //check if names are defined
                            if (!string.IsNullOrEmpty(dataColumnName))
                            {
                                name = Convert.ToString(dt.Rows[SHP_Line][dataColumnName]).Trim();
                                if (name.Length < 1) // a name is needed
                                {
                                    name = "Building " + Convert.ToString(SHP_Line);
                                }
                            }
                            else
                            {
                                name = "Building " + Convert.ToString(SHP_Line);
                            }

                            // Check if streets are defined
                            if (!string.IsNullOrEmpty(dataColumnStreet))
                            {
                                name = name + '\t' + Convert.ToString(dt.Rows[SHP_Line][dataColumnStreet]).Trim();
                                // Check if numbers are defined
                                if (!string.IsNullOrEmpty(dataColumnNumbers))
                                {
                                    name = name + '\t' + Convert.ToString(dt.Rows[SHP_Line][dataColumnNumbers]).Trim();
                                }
                            }
                            _bd.Name = name;

                            //compute area
                            calc_area(numbpt, pt);
                            _bd.Area = (float)(Math.Round(areapolygon, 1));

                            double height = St_F.TxtToDbl(dt.Rows[SHP_Line][dataColumnHeight].ToString(), false);
                            height = Math.Round(height, 1);

                            if (checkBox1.Checked == true) // absolute heights
                            {
                                height *= -1.0;
                            }
                            _bd.Height = (float)height;

                            domain.EditB.ItemData.Add(_bd);
                        }
                        polygons = null;
                    }
                    SHP_Line++;
                }

                shape = null;

                WaitDlg.Close();
                WaitDlg.Dispose();
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Please define a height column", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //read attribute table from dbf file
        private void Shape_Building_Dialog_Load(object sender, EventArgs e)
        {
            //open dbf file
            Cursor = Cursors.WaitCursor;

            GralMessage.Waitprogressbar wait = new GralMessage.Waitprogressbar("Import data base");
            wait.Show();

            if (Wall) // Wall mode
            {
                Text = "Import walls";
                label2.Visible = false;
                comboBox2.Visible = false;
                label4.Visible = false;
                comboBox4.Visible = false;
                label1.Text = "Wall height";
                checkBox3.Visible = true;
            }

            ParseDBF dbf_reader = new ParseDBF
            {
                dt = dt
            };
            dbf_reader.StartPosition = FormStartPosition.Manual;
            dbf_reader.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
            dbf_reader.Top = GralStaticFunctions.St_F.GetTopScreenAtMousePosition() + 150;
            dbf_reader.ReadDBF(ShapeFileName.Replace(".shp", ".dbf"));
            dt = dbf_reader.dt;
            dbf_reader.Close();
            dbf_reader.Dispose();

            ShapeImport_AddColumn.SetDataGridViewSource(dataGridView1, dt, ShowEntireDataset.Checked);

            wait.Close();
            wait.Dispose();
            Cursor = Cursors.Default;

            //fill combobox with row names
            ComboColumnNames.Add(comboBox1);
            ComboColumnNames.Add(comboBox2);
            ComboColumnNames.Add(comboBox3);
            ComboColumnNames.Add(comboBox4);

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

            // disable sorting!			
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
                for (int i = 0; i < numpoints - 1; i++)
                {
                    areapolygon = areapolygon + (polypoints[i + 1].X - polypoints[i].X) * polypoints[i].Y +
                        (polypoints[i + 1].X - polypoints[i].X) * (polypoints[i + 1].Y - polypoints[i].Y) / 2;
                }
                areapolygon = Math.Abs(areapolygon);
            }
        }

        void Shape_Building_DialogFormClosed(object sender, FormClosedEventArgs e)
        {
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
            if (WaitDlg != null)
            {
                WaitDlg.Close();
                WaitDlg.Dispose();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ShowEntireDataset_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            ShapeImport_AddColumn.SetDataGridViewSource(dataGridView1, dt, ShowEntireDataset.Checked);
        }
    }
}