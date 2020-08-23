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
using System.Windows.Forms;
using System.Globalization;
using GralStaticFunctions;
using GralItemData;

namespace GralShape
{
    /// <summary>
    /// Dialog to import an item
    /// </summary>
    public partial class Shape_Receptor_Dialog : Form
    {
        private readonly GralDomain.Domain domain = null;
        private readonly string ShapeFileName;
        private double areapolygon = 0;  //area of a polygon
        private DataTable dt;
        private bool  _vegetation = false;
        public bool Vegetation {set {_vegetation = value;}}
        private readonly CultureInfo ic = CultureInfo.InvariantCulture;
        private readonly string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;  
        private readonly GralData.DomainArea GralDomRect;
        private readonly double _deltaZ;

        /// <summary>
        /// Dialog for the shape import of Receptors and Vegetation Areas
        /// </summary>
        /// <param name="d">Domain form reference</param>
        /// <param name="GralDomainRectangle">Bounds of GRAL domain rectangle</param>
        /// <param name="DeltaZ">Vertical Dimension of Concentration Layers</param>
        /// <param name="Filename">Filename of Shape File</param>
        /// <param name="Vegeatation">true, if vegetation should be imported </param>
        public Shape_Receptor_Dialog(GralDomain.Domain d, GralData.DomainArea GralDomainRectangle, double DeltaZ, string Filename)
        {
            InitializeComponent();
            domain = d;
            GralDomRect = GralDomainRectangle;
            _deltaZ = DeltaZ;
            ShapeFileName = Filename;
            button1.DialogResult = DialogResult.OK;
        }

        //add point data
        private void button1_Click(object sender, EventArgs e)
        {
            if (_vegetation == false && comboBox1.SelectedIndex != 0 && comboBox1.SelectedItem != null) // Receptor
            {
                Import_Receptor_Data();
            }
            else if (_vegetation == true && comboBox1.SelectedIndex != 0 && comboBox1.SelectedItem != null &&  // Load Vegetation area
                     comboBox2.SelectedIndex != 0 && comboBox2.SelectedItem != null &&
                     comboBox3.SelectedIndex != 0 && comboBox3.SelectedItem != null &&
                     comboBox4.SelectedIndex != 0 && comboBox4.SelectedItem != null &&
                     comboBox5.SelectedIndex != 0 && comboBox5.SelectedItem != null &&
                     comboBox6.SelectedIndex != 0 && comboBox6.SelectedItem != null)
            {
                Import_Vegegation_Data();
            }
            else
            {
                MessageBox.Show(this, "Not all needed values defined - data is not imported", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        
        private void Import_Receptor_Data()
        {
            double height = 3;
                        
            GralMessage.Waitprogressbar wait = new GralMessage.Waitprogressbar("Import data base");
            wait.Show();

            bool inside;
            // for (int i = 0; i < domain.shppoints[index].Count; i++)
            int SHP_Line = 0;
            ShapeReader shape = new ShapeReader(domain);
            foreach (object shp in shape.ReadShapeFile(ShapeFileName))
            {
                if (shp is PointF || shp is GralDomain.PointD || shp is GralData.PointD_3d)
                {
                    GralData.PointD_3d _ptshp;
                    if (shp is GralData.PointD_3d)
                    {
                        _ptshp = (GralData.PointD_3d)shp;
                    }
                    else
                    {
                        _ptshp = ((GralDomain.PointD)shp).ToPoint3d();
                    }
                    
                    GralDomain.PointD pt = new GralDomain.PointD();
                    pt.X = Math.Round(_ptshp.X, 1);
                    pt.Y = Math.Round(_ptshp.Y, 1);
                    double ptZ = Math.Round(_ptshp.Z, 1); 

                    inside = false;
                    if ((pt.X > GralDomRect.West) && (pt.X < GralDomRect.East) && (pt.Y > GralDomRect.South) && (pt.Y < GralDomRect.North))
                    {
                        inside = true;
                    }

                    if (inside == true)
                    {
                        ReceptorData _rd = new ReceptorData();

                        //check for names
                        if (comboBox3.SelectedIndex != 0)
                        {
                            _rd.Name = Convert.ToString(dataGridView1[Convert.ToString(comboBox3.SelectedItem), SHP_Line].Value).Trim();
                        }
                        else
                        {
                            _rd.Name = "Receptor" + Convert.ToString(SHP_Line);
                        }

                        //check for stack heights
                        if (comboBox1.SelectedIndex != 0)
                        {
                            height = Convert.ToDouble(dataGridView1[Convert.ToString(comboBox1.SelectedItem), SHP_Line].Value, ic);
                            height = Math.Round(height, 1);
                        }
                        else if (shp is GralData.PointD_3d)
                        {
                            height = ptZ;
                        }
                        else
                        {
                            height = 3;
                        }

                        if (height - _deltaZ * 0.5 < 0)
                        {
                            height = _deltaZ;
                        }

                        _rd.Height = (float) Math.Round(height, 1);
                        _rd.Pt = new GralDomain.PointD(pt.X, pt.Y);
                        
                        domain.EditR.ItemData.Add(_rd);

                    }
                    
                    SHP_Line++;
                }
            }
            
            shape = null;
            #if __MonoCS__
            #else
            if (dataGridView1 != null)
            {
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
            }

            DataRowCollection itemColumns = dt.Rows; // mark elements as deleted
            for (int i = itemColumns.Count -1 ; i >= 0; i--)
            {
                itemColumns[i].Delete();
            }
            dt.AcceptChanges(); // accept deletion
            dt.Clear(); // clear data table
            
            if (dt != null)
            {
                dt.Dispose();
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
            wait.Close();
            wait.Dispose();
            
            Close();
        }
        
        private void Import_Vegegation_Data()
        {
            string name="Vegetation";
            bool inside;
            CultureInfo ic  = CultureInfo.InvariantCulture;
            
            GralMessage.Waitprogressbar wait = new GralMessage.Waitprogressbar("Import data base");
            wait.Show();
            
            //for (int i = 0; i < SHPPolygons[index].Count; i++)
            int SHP_Line = 0;
            ShapeReader shape = new ShapeReader(domain);
            foreach (object shp in shape.ReadShapeFile(ShapeFileName))
            {
                if (shp is SHPPolygon)
                {
                    SHPPolygon polygons = (SHPPolygon) shp;
                    
                    inside = false;
                    int numbpt = polygons.Points.Length;

                    GralDomain.PointD[] pt = new GralDomain.PointD[numbpt];
                    
                    //add only Vedgetation with at least one point inside the GRAL domain
                    for (int j = 0; j < numbpt; j++)
                    {
                        pt[j].X =  Math.Round(polygons.Points[j].X, 1);
                        pt[j].Y =  Math.Round(polygons.Points[j].Y, 1);
                        if ((pt[j].X > GralDomRect.West) && (pt[j].X < GralDomRect.East) && (pt[j].Y > GralDomRect.South) && (pt[j].Y < GralDomRect.North))
                        {
                            inside = true;
                        }
                    }
                    
                    if (inside == true)
                    {
                        VegetationData _vdata = new VegetationData();
                        for (int j = 0; j < numbpt - 1; j++)
                        {
                            _vdata.Pt.Add(new GralDomain.PointD(pt[j].X, pt[j].Y));
                        }
                        
                        //check if names are defined
                        if (comboBox3.SelectedIndex != 0)
                        {
                            name = Convert.ToString(dataGridView1[Convert.ToString(comboBox3.SelectedItem), SHP_Line].Value).Trim();
                            if (name.Length < 1) // a name is needed
                            {
                                name = "Vegetation " + Convert.ToString(SHP_Line);
                            }
                        }
                        else
                        {
                            name = "Vegetation " + Convert.ToString(SHP_Line);
                        }

                        _vdata.Name = name;
                        
                        //compute area
                        calc_area(numbpt, pt);
                        _vdata.Area = (float) Math.Round(areapolygon);
                        
                        double height = St_F.TxtToDbl(Convert.ToString(dataGridView1[Convert.ToString(comboBox1.SelectedItem), SHP_Line].Value), false);
                        _vdata.VerticalExt = (float) Math.Round(height, 1);
                        
                        double TrunkZone = St_F.TxtToDbl(Convert.ToString(dataGridView1[Convert.ToString(comboBox2.SelectedItem), SHP_Line].Value), false);
                        _vdata.TrunkZone = (float) Math.Round(Math.Max(0, Math.Min(TrunkZone, 100)), 1);
                        
                        double Trunk = St_F.TxtToDbl(Convert.ToString(dataGridView1[Convert.ToString(comboBox4.SelectedItem), SHP_Line].Value), false);
                        _vdata.LADTrunk = (float) Math.Round(Math.Max(0, Math.Min(Trunk, 10)), 1);
                        
                        double Crown = St_F.TxtToDbl(Convert.ToString(dataGridView1[Convert.ToString(comboBox5.SelectedItem), SHP_Line].Value), false);
                        _vdata.LADCrown = (float) Math.Round(Math.Max(0, Math.Min(Crown, 10)), 1);
                        
                        double Coverage = St_F.TxtToDbl(Convert.ToString(dataGridView1[Convert.ToString(comboBox6.SelectedItem), SHP_Line].Value), false);
                        _vdata.Coverage = (float) Math.Round(Math.Max(0, Math.Min(Coverage, 100)), 1);
                        
                        domain.EditVegetation.ItemData.Add(_vdata);
                        
                    }
                    polygons = null;
                }
                SHP_Line ++;
            }
            
            shape = null;
            
            #if __MonoCS__
            #else
            if (dataGridView1 != null)
            {
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
            }

            DataRowCollection itemColumns = dt.Rows; // mark elements as deleted
            for (int i = itemColumns.Count -1 ; i >= 0; i--)
            {
                itemColumns[i].Delete();
            }
            dt.AcceptChanges(); // accept deletion
            dt.Clear(); // clear data table
            
            if (dt != null)
            {
                dt.Dispose();
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
            
            wait.Close();
            wait.Dispose();
        }

        //read attribute table from dbf file
        private void Shape_VegetationReceptor_Dialog_Load(object sender, EventArgs e)
        {
            if (_vegetation)
            {
                Text = "Import vegetation areas";
            }
            
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
            dbf_reader.Top = 80;
            dbf_reader.ReadDBF(ShapeFileName.Replace(".shp", ".dbf"));
            dt = dbf_reader.dt;
            dbf_reader.Close();
            dbf_reader.Dispose();
            
            dataGridView1.DataSource = dt;
            wait.Close();
            wait.Dispose();
            Cursor = Cursors.Default;

            //fill combobox with row names
            comboBox1.Items.Add("None");
            comboBox2.Items.Add("None");
            comboBox3.Items.Add("None");
            comboBox4.Items.Add("None");
            comboBox5.Items.Add("None");
            comboBox6.Items.Add("None");
            
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                comboBox1.Items.Add(dataGridView1.Columns[i].HeaderText);
                comboBox2.Items.Add(dataGridView1.Columns[i].HeaderText);
                comboBox3.Items.Add(dataGridView1.Columns[i].HeaderText);
                comboBox4.Items.Add(dataGridView1.Columns[i].HeaderText);
                comboBox5.Items.Add(dataGridView1.Columns[i].HeaderText);
                comboBox6.Items.Add(dataGridView1.Columns[i].HeaderText);
            }
            
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
            comboBox6.SelectedIndex = 0;
            
            AcceptButton = button1;
            ActiveControl = button1;
            
            // disable sort!
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            
            if (_vegetation == false)
            {
                label2.Visible = false;
                label4.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
                comboBox2.Visible = false;
                comboBox4.Visible = false;
                comboBox5.Visible = false;
                comboBox6.Visible = false;
            }
            
        }

        //add column
        private void button2_Click(object sender, EventArgs e)
        {
            using (ShapeImport_AddColumn shpimport = new ShapeImport_AddColumn())
            {
                if (shpimport.ShowDialog() == DialogResult.OK)
                {
                    //add value to column
                    string header = shpimport.textBox1.Text;
                    dataGridView1.Columns.Add(header, header);
                    comboBox1.Items.Add(header);
                    comboBox2.Items.Add(header);
                    comboBox3.Items.Add(header);
                    comboBox4.Items.Add(header);
                    comboBox5.Items.Add(header);
                    comboBox6.Items.Add(header);


                    //check if there is a fix value or an equation given
                    if (shpimport.textBox2.Text.Substring(0, 1) == "=")
                    {
                        //math operation
                        MathFunctions.MathParser mp = new MathFunctions.MathParser();
                        string mathtext;
                        decimal a;
                        decimal result;
                        mathtext = shpimport.textBox2.Text.Replace(".", decsep);
                        mathtext = mathtext.Replace(",", decsep);
                        int index = 0;
                        int stringcount = 0;
                        string headername = "";

                        //search for column names in mathtext
                        for (int j = 0; j < dataGridView1.ColumnCount; j++)
                        {
                            if (mathtext.Contains(dataGridView1.Columns[j].HeaderText) == true)
                            {
                                if (dataGridView1.Columns[j].HeaderText.Length > stringcount)
                                {
                                    index = j;
                                    stringcount = dataGridView1.Columns[j].HeaderText.Length;
                                    headername = dataGridView1.Columns[j].HeaderText.ToString();
                                }
                            }
                        }
                        mathtext = mathtext.Replace(headername, "A");
                        mathtext = mathtext.Remove(0, 1);

                        //perform calculation
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            try
                            {
                                a = (decimal)Convert.ToDecimal(dataGridView1.Rows[i].Cells[index].Value);
                                mp.Parameters.Clear();
                                mp.Parameters.Add(MathFunctions.Parameters.A, a);
                                result = mp.Calculate(mathtext);
                                dataGridView1[header, i].Value = Convert.ToString(result);
                            }
                            catch
                            {
                            }
                        }
                    }
                    else
                    {
                        //fill data grid with fix value
                        string trans = shpimport.textBox2.Text;
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            dataGridView1[header, i].Value = trans;
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
            string header = "Header";
            if (St_F.InputBoxValue("Remove column", "Name:", ref header, this) == DialogResult.OK)
            {
                dataGridView1.Columns.Remove(header);
                comboBox1.Items.Remove(header);
                comboBox2.Items.Remove(header);
                comboBox3.Items.Remove(header);
                comboBox4.Items.Remove(header);
                comboBox5.Items.Remove(header);
                comboBox6.Items.Remove(header);
            }
        }
        
        void Shape_Receptor_DialogFormClosed(object sender, FormClosedEventArgs e)
        {
            
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
        
    }
}