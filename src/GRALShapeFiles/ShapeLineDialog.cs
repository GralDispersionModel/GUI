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
using GralDomain;
using GralItemData;
using GralStaticFunctions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace GralShape
{
    /// <summary>
    /// Dialog to import an item
    /// </summary>
    public partial class ShapeLineDialog : Form
    {
        private readonly GralDomain.Domain domain = null;
        
        private readonly string ShapeFileName;
        private readonly Deposition[] dep = new Deposition[10];
        private readonly Button[] but1 = new Button[10];                     //Buttons for deposition
        private DataTable dt;
        private readonly CultureInfo ic = CultureInfo.InvariantCulture;
        private readonly List<int> SG_Number = new List<int>();
        private readonly string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        private readonly GralData.DomainArea GralDomRect;
        private readonly List<ComboBox> ComboPollNames = new List<ComboBox>();
        private readonly List<ComboBox> ComboColumnNames = new List<ComboBox>();
        private GralMessage.Waitprogressbar WaitDlg;

        /// <summary>
        /// Dialog for the shape import of Line Sources
        /// </summary>
        /// <param name="d">Domain form reference</param>
        /// <param name="GralDomainRectangle">Bounds of GRAL domain rectangle</param>
        /// <param name="Filename">Filename of Shape File</param>
        public ShapeLineDialog(GralDomain.Domain d, GralData.DomainArea GralDomainRectangle, string Filename)
        {
            InitializeComponent();
            domain = d;
            GralDomRect = GralDomainRectangle;
            ShapeFileName = Filename;
            
            Load_Source_Grp();

            //fill the combobox30 with available base years
            for (int i = 0; i < 60; i++)
            {
                comboBox30.Items.Add(Convert.ToString(1990 + i));
            }

            comboBox30.SelectedIndex = 25;
            
            // init deposition data
            for (int i = 0; i < 10; i++)
            {
                dep[i] = new Deposition(); // initialize Gral.Deposition array
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
                //Buttons for Gral.Deposition
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
                but1[nr].Text = "Gral.Deposition " + (nr + 1).ToString();
                Controls.Add(but1[nr]);
                toolTip1.SetToolTip(but1[nr], "Click to set deposition - green: deposition set");
                but1[nr].Click += new EventHandler(Edit_deposition);
                x++;
            }
        }

        //add line data
        private void Button1_Click(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex != 0 && comboBox25.SelectedIndex != 0 &&
                comboBox4.SelectedItem != null && comboBox25.SelectedItem != null)
            {
                string name = "Line";
                string section = "1";
                int sgroup = 1;
                
                bool inside;
                
                GralMessage.Waitprogressbar wait = new GralMessage.Waitprogressbar("Import shape file");
                wait.Show();
                
                GralData.DouglasPeucker douglas = new GralData.DouglasPeucker();
                
                int SHP_Line = 0;
                ShapeReader shape = new ShapeReader(domain);
                foreach (object shp in shape.ReadShapeFile(ShapeFileName))
                {
                    if (shp is SHPLine)
                    {
                        SHPLine lines = (SHPLine) shp;
                        
                        inside = false;
                        int numbpt = lines.Points.Length;
                        List <PointD> pt = new List<PointD>();
                        //add only lines inside the GRAL domain
                        for (int j = 0; j < numbpt; j++)
                        {
                            PointD ptt = new PointD(Math.Round(lines.Points[j].X, 1), Math.Round(lines.Points[j].Y, 1));
                            pt.Add(ptt);
                            
                            if ((pt[j].X > GralDomRect.West) && (pt[j].X < GralDomRect.East) && (pt[j].Y > GralDomRect.South) && (pt[j].Y < GralDomRect.North))
                            {
                                inside = true;
                            }
                        }

                        if (inside == true && numbpt > 1)
                        {
                            LineSourceData _ls = new LineSourceData();

                            //check for height
                            double height = 0;
                            if (comboBox2.SelectedIndex != 0)
                            {
                                try
                                {
                                    if (checkBox1.Checked == true) // absolute heights
                                    {
                                        height = (-1) * Math.Abs(St_F.TxtToDbl(dt.Rows[SHP_Line][Convert.ToString(comboBox2.SelectedItem)].ToString(), false));
                                    }
                                    else
                                    {
                                        height = St_F.TxtToDbl(dt.Rows[SHP_Line][Convert.ToString(comboBox2.SelectedItem)].ToString(), false);
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

                            _ls.Height = (float)height;

                            // Copy Line Points
                            if (checkBox3.Checked && lines.PointsZ != null && lines.PointsZ.Length == pt.Count) // 3D Lines
                            {
                                // 3D Lines
                                douglas.DouglasPeuckerRun(pt, (double)numericUpDown1.Value);
                                _ls.Lines3D = true;
                                
                                int abs = 1;
                                if (checkBox1.Checked == true) // absolute heights
                                {
                                    abs = -1;
                                }

                                // select unfiltered points 
                                int index = 0;
                                for (int j = 0; j < numbpt; j++)
                                {
                                    if (index < pt.Count && Math.Abs(lines.Points[j].X - pt[index].X) < 0.1 && Math.Abs(lines.Points[j].Y - pt[index].Y) < 0.1)
                                    {
                                        _ls.Pt.Add(new GralData.PointD_3d(pt[index].X, pt[index].Y, lines.PointsZ[j] * abs));
                                        index++;
                                    }
                                }
                            }
                            else // 2D Lines
                            {
                                douglas.DouglasPeuckerRun(pt, (double)numericUpDown1.Value);
                                _ls.Lines3D = false;
                                foreach (PointD _pt in pt)
                                {
                                    _ls.Pt.Add(new GralData.PointD_3d(_pt.X, _pt.Y, height));
                                }
                            }

                            numbpt = _ls.Pt.Count;
                            
                            //check for names
                            if (comboBox3.SelectedIndex != 0)
                            {
                                name = Convert.ToString(dt.Rows[SHP_Line][Convert.ToString(comboBox3.SelectedItem)]).Trim();
                                if (name.Length < 1) // a name is needed
                                {
                                    name = "Line " + Convert.ToString(SHP_Line);
                                }
                            }
                            else
                            {
                                name = "Line " + Convert.ToString(SHP_Line);
                            }

                            _ls.Name = name;
                            
                            //check for line sections
                            if (comboBox1.SelectedIndex != 0)
                            {
                                try
                                {
                                    section = Convert.ToString(Convert.ToInt32(dt.Rows[SHP_Line][Convert.ToString(comboBox1.SelectedItem)]), ic);
                                    if (section == "")
                                    {
                                        section = "1";
                                    }
                                }
                                catch
                                {
                                    section = "1";
                                }
                            }
                            else
                            {
                                section = "1";
                            }

                            _ls.Section = section;
                                                    
                            // check for vertical extension
                            double vertical_extension = 0;
                            if (comboBox31.SelectedIndex != 0)
                            {
                                try
                                {
                                    vertical_extension = St_F.TxtToDbl(dt.Rows[SHP_Line][Convert.ToString(comboBox31.SelectedItem)].ToString(), false);
                                    
                                    vertical_extension = Math.Round(vertical_extension, 1);
                                    if (vertical_extension == 0)
                                    {
                                        vertical_extension = 3;
                                    }
                                }
                                catch
                                {
                                    vertical_extension = 3;
                                }
                            }
                            else
                            {
                                vertical_extension = 3;
                            }

                            _ls.VerticalExt = (float) vertical_extension;
                            
                            //check for source group
                            if (comboBox4.SelectedIndex != 0)
                            {
                                try
                                {
                                    string _sgroup = Convert.ToString(Convert.ToInt32(dt.Rows[SHP_Line][Convert.ToString(comboBox4.SelectedItem)]), ic);
                                    if (string.IsNullOrEmpty(_sgroup))
                                    {
                                        _sgroup = "1";
                                    }

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
                                catch
                                {
                                    sgroup = 1;
                                }
                            }
                            else
                            {
                                sgroup = 1;
                            }

                            PollutantsData _poll = new PollutantsData
                            {
                                SourceGroup = sgroup
                            };

                            //check for width
                            double width = 7;
                            if (comboBox25.SelectedIndex != 0)
                            {
                                try
                                {
                                    width = St_F.TxtToDbl(dt.Rows[SHP_Line][Convert.ToString(comboBox25.SelectedItem)].ToString(), false);
                                    width = Math.Round(width, 1);
                                    if (width == 0)
                                    {
                                        width = 7;
                                    }
                                }
                                catch
                                {
                                    width = 7;
                                }
                            }
                            _ls.Width = (float) width;
                            
                            //check for average daily traffic
                            double aadt = 0;
                            if (comboBox26.SelectedIndex != 0)
                            {
                                try
                                {
                                    aadt = Math.Round(St_F.TxtToDbl(dt.Rows[SHP_Line][Convert.ToString(comboBox26.SelectedItem)].ToString(), false), 0);
                                }
                                catch
                                {
                                    aadt = 0;
                                }
                            }
                            _ls.Nemo.AvDailyTraffic = (int) aadt;
                            
                            //check for share of heavy duty vehicles
                            double hdv = 0;
                            if (comboBox27.SelectedIndex != 0)
                            {
                                try
                                {
                                    hdv = Math.Round(St_F.TxtToDbl(dt.Rows[SHP_Line][Convert.ToString(comboBox27.SelectedItem)].ToString(), false), 1);
                                }
                                catch
                                {
                                    hdv = 0;
                                }
                            }
                            _ls.Nemo.ShareHDV = (float) hdv;
                            
                            //check for the slope of roads
                            double slope = 0;
                            if (comboBox28.SelectedIndex != 0)
                            {
                                try
                                {
                                    slope = Math.Round(St_F.TxtToDbl(dt.Rows[SHP_Line][Convert.ToString(comboBox28.SelectedItem)].ToString(), false), 1);
                                }
                                catch
                                {
                                    slope = 0;
                                }
                            }
                            _ls.Nemo.Slope = (float) slope;
                            
                            //check for the average passenger car speed
                            double speed = 0;
                            if (comboBox29.SelectedIndex != 0)
                            {
                                try
                                {
                                    speed = Math.Round(St_F.TxtToDbl(dt.Rows[SHP_Line][Convert.ToString(comboBox29.SelectedItem)].ToString(), false), 0);
                                }
                                catch
                                {
                                    speed = 0;
                                }
                            }
                            
                            //select traffic situation according to the speed and slope
                            
                            //read velocities for each vehicle-type according to the driving pattern and the slope
                            Assembly _assembly;
                            _assembly = Assembly.GetExecutingAssembly();

                            string[] line = new string[400];
                            string[] velo = new string[11];
                            using (StreamReader myReader = new StreamReader(_assembly.GetManifestResourceStream("Gral.Resources.NEMO_velocities.txt")))
                            {
                                line[0] = myReader.ReadLine();
                                for (int k = 1; k < 400; k++)
                                {
                                    try
                                    {
                                        line[k] = myReader.ReadLine();
                                    }
                                    catch
                                    {
                                        break;
                                    }
                                }
                            }
                            
                            string trafficsituation = String.Empty;
                            double diff = 100000;
                            for (int k = 1; k < 400; k++)
                            {
                                try
                                {
                                    velo = line[k].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
                                    //check for the slope
                                    double slpnemo = Convert.ToDouble(velo[2].Replace(".", decsep));
                                    double slpinput = slope;
                                    if (slpnemo >= slpinput)
                                    {
                                        double vPC = Convert.ToDouble(velo[4].Replace(".", decsep));
                                        double difference = Math.Abs(speed - vPC);
                                        
                                        if ((difference < diff) && (velo[0] != "Dummy"))
                                        {
                                            diff = difference;
                                            trafficsituation = velo[0];
                                        }
                                    }
                                }
                                catch
                                {
                                    break;
                                }
                            }
                            
                            //get the corresponding index of the traffic situation
                            int trafficsitnumber = 0;
                            for (int k = 0; k < Main.NemoTrafficSituations.Length; k++)
                            {
                                if (Main.NemoTrafficSituations[k] == trafficsituation)
                                {
                                    trafficsitnumber = k;
                                }
                            }
                            
                            _ls.Nemo.TrafficSit = trafficsitnumber;

                            //check for the base year
                            string year = String.Empty;
                            if (comboBox30.SelectedItem != null)
                            {
                                year = Convert.ToString(comboBox30.Items[comboBox30.SelectedIndex]);
                            }
                            else
                            {
                                year = "2020";
                            }
                            _ls.Nemo.BaseYear = (int) (St_F.TxtToDbl(year, false));
                            
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
                                    _poll.EmissionRate[_count] = St_F.TxtToDbl(dt.Rows[SHP_Line][Convert.ToString(ComboColumnNames[_count].SelectedItem)].ToString(), false) * emission_factor;
                                }
                                _poll.Pollutant[_count] = _combo.SelectedIndex;
                                _count++;
                            }
                            
                            _ls.Poll.Add(new PollutantsData(_poll));
                            
                            for (int di = 0; di < 10; di++) // save deposition
                            {
                                _ls.GetDep()[di] = dep[di];
                            }
                            
                            domain.EditLS.ItemData.Add(_ls);
                            Application.DoEvents();

                            if (SG_Number.Contains(sgroup) == false) // this source group is not defined
                            {
                                Add_Source_Grp("Line-source-import-" + sgroup + "," + sgroup);
                            }
                            
                            lines = null;
                        }
                        
                        pt.Clear();
                        
                    }
                    SHP_Line++;
                }
                shape = null;
                               
                // set new source groups to the source dialogs
                domain.LoadSourceGroups();
                
                wait.Close();
                wait.Dispose();
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(this, "Please define a source group, width and base year column", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //read attribute table from dbf file
        private void Shape_Line_Dialog_Load(object sender, EventArgs e)
        {
            //open dbf file
            Cursor = Cursors.WaitCursor;
            WaitDlg = new GralMessage.Waitprogressbar("Import data base");
            WaitDlg.Show();

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

            ShapeImport_AddColumn.SetDataGridViewSource(dataGridView1, dt, ShowEntireDataset.Checked);

            WaitDlg.Close();
            WaitDlg.Dispose();
            Cursor = Cursors.Default;

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
            ComboColumnNames.Add(comboBox25);
            ComboColumnNames.Add(comboBox26);
            ComboColumnNames.Add(comboBox27);
            ComboColumnNames.Add(comboBox28);
            ComboColumnNames.Add(comboBox29);
            ComboColumnNames.Add(comboBox31);

            //fill comboboxes with the name of available pollutants
            foreach (ComboBox _combo in ComboPollNames)
            {
                for (int i = 0; i < Main.PollutantList.Count; i++)
                {
                    _combo.Items.Add(Main.PollutantList[i]);
                }
                _combo.SelectedIndex = 0;
            }

            //fill combobox with row names
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
        private void Button2_Click(object sender, EventArgs e)
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
        private void Button3_Click(object sender, EventArgs e)
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
        
        void Shape_Line_DialogFormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (Button but in but1)
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
            
        private void Load_Source_Grp()
        {
            // import source groups and fill source dialogs 
            try
            {
                //import source group definitions
                string newPath = Path.Combine(Main.ProjectName, @"Settings", "Sourcegroups.txt");
                SG_Number.Clear();
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
                            SG_Number.Add(St_F.GetSgNumber(text[1]));
                        }
                    }
                }
            }
            catch
            {
            }
        }
        
        private void Add_Source_Grp(string a)
        {
            // add a source group and fill source dialogs 
            try
            {
                //write additional source group definition
                string newPath = Path.Combine(Main.ProjectName, @"Settings", "Sourcegroups.txt");
                
                using (StreamWriter myWriter = new StreamWriter(newPath, true))
                {
                    myWriter.WriteLine(a);
                }
            }
            catch
            {
            }
            
            SortSourceGroupFile(Path.Combine(Main.ProjectName, @"Settings", "Sourcegroups.txt"));
            Load_Source_Grp();
        }
        
        /// <summary>
        /// Sort the source group files dependend on the source group number
        /// </summary>
        /// <param name="filename">Full path and name of a text file</param> 
        private void SortSourceGroupFile(string filename)
        {
            List<Gral.SG_Class> sg = new List<Gral.SG_Class>();
            
            try
            {
                //import source group definitions
                using (StreamReader myReader = new StreamReader(filename))
                {
                    string[] text = new string[2];
                    string text1;
                   
                    while (myReader.EndOfStream == false)
                    {
                        text1 = myReader.ReadLine();
                        text = text1.Split(new char[] { ',' });
                        if (text.Length > 1)
                        {
                            int s = 1;      
                            if (int.TryParse(text[1], out s))
                            {
                              sg.Add(new Gral.SG_Class() {SG_Name = text[0], SG_Number = s});
                            }
                        }
                    }
                }
                
                sg.Sort();
                
                //write source group definition file
                using (StreamWriter myWriter = new StreamWriter(filename))
                {
                    foreach(Gral.SG_Class _sg in sg)
                    {
                        myWriter.WriteLine(_sg.ToString());
                    }
                }
                
            }
            catch
            {
            }
            
            sg.Clear();
         }

        private void Button4_Click(object sender, EventArgs e)
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