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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using Gral;
using GralDomain;
using GralItemData;
using GralStaticFunctions;

namespace GralShape
{
	/// <summary>
    /// Dialog to import an item
    /// </summary>
    public partial class ShapeBuildingDialog : Form
	{
		private GralDomain.Domain domain = null;
		
		private string file;
		private double areapolygon = 0;
		public bool Wall = false;
		private DataTable dt;
        private CultureInfo ic = CultureInfo.InvariantCulture;
        private string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;  
        private GralData.DomainArea GralDomRect;

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
			file = Filename;
			button1.DialogResult = DialogResult.OK;
		}

		//add building data
		private void button1_Click(object sender, EventArgs e)
		{
			if (comboBox1.SelectedIndex != 0 && comboBox1.SelectedItem != null)
			{
				string name="Building";
				if (Wall)
				{
				    name = "Wall";
				}
				
				bool inside;
								
				GralMessage.Waitprogressbar wait = new GralMessage.Waitprogressbar("Import data base");
				wait.Show();
				
				GralData.DouglasPeucker douglas = new GralData.DouglasPeucker();
				
				int SHP_Line = 0;
				ShapeReader shape = new ShapeReader(domain);
				foreach (object shp in shape.ReadShapeFile(file))
				{
				    // Walls
				    if (Wall && shp is SHPLine)
                	{
                		SHPLine lines = (SHPLine) shp;
                		
                		inside = false;
                		int numbpt = lines.Points.Length;
                		List <PointD> pt = new List<PointD>();
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
                				inside = true;
                		}
                		if (inside == true && numbpt > 1)
                		{
                			WallData _wd = new WallData();
                			
                			douglas.DouglasPeuckerRun(pt, (double) numericUpDown1.Value);
                			
                			//check for height
                			double height = 0;
                			if (comboBox1.SelectedIndex != 0)
                			{
                			    try
                			    {
                			        if (checkBox1.Checked == true) // absolute heights
                			            height = (-1) * Math.Abs(Convert.ToDouble(dataGridView1[Convert.ToString(comboBox1.SelectedItem), SHP_Line].Value, ic));
                			        else
                			            height = Convert.ToDouble(dataGridView1[Convert.ToString(comboBox1.SelectedItem), SHP_Line].Value, ic);
                			        
                			        height = Math.Round(height, 1);
                			    }
                			    catch
                			    {
                			        height = 0;
                			    }
                			}
                			else
                			    height = 0;
                			
                			foreach(PointD _pt in pt)
                			{
                			    _wd.Pt.Add(new GralData.PointD_3d(_pt.X, _pt.Y, (float) (height)));
                			}
                			
                			numbpt = pt.Count;
                			
                			//check for names
                			if (comboBox3.SelectedIndex != 0)
                			{
                				name = Convert.ToString(dataGridView1[Convert.ToString(comboBox3.SelectedItem), SHP_Line].Value).Trim();
                				if (name.Length < 1) // a name is needed
                					name = "Wall " + Convert.ToString(SHP_Line);
                			}
                			else
                				name = "Wall " + Convert.ToString(SHP_Line);
                			
                			_wd.Name = name;
                			_wd.Lenght = (float) Math.Round(_wd.CalcLenght(), 1);
                			
                			domain.EditWall.ItemData.Add(_wd);
                		}
                		lines = null;
                	}
				    
				    // Load Buildings
				    if (!Wall && shp is SHPPolygon)
					{
						SHPPolygon polygons = (SHPPolygon) shp;
						
						inside = false;
						int numbpt = polygons.Points.Length;
						
						PointD[] pt = new PointD[numbpt];
						//add only buildings inside the GRAL domain
						for (int j = 0; j < numbpt; j++)
						{
							pt[j].X = Math.Round(polygons.Points[j].X, 1);
							pt[j].Y = Math.Round(polygons.Points[j].Y, 1);
							if ((pt[j].X > GralDomRect.West) && (pt[j].X < GralDomRect.East) && (pt[j].Y > GralDomRect.South) && (pt[j].Y < GralDomRect.North))
								inside = true;
						}
						if (inside == true && numbpt > 2)
						{
							BuildingData _bd = new BuildingData();
							for (int i = 0; i < numbpt - 1; i ++)
							{
								_bd.Pt.Add(new PointD(pt[i].X, pt[i].Y));
							}
							
							douglas.DouglasPeuckerRun(_bd.Pt, (double) numericUpDown1.Value);
							
							//check if names are defined
							if (comboBox3.SelectedIndex != 0)
							{
								name = Convert.ToString(dataGridView1[Convert.ToString(comboBox3.SelectedItem), SHP_Line].Value).Trim();
								if (name.Length < 1) // a name is needed
									name = "Building " + Convert.ToString(SHP_Line);
							}
							else
								name = "Building " + Convert.ToString(SHP_Line);
							
							// Check if streets are defined
							if (comboBox2.SelectedIndex != 0)
							{
								name = name + '\t' + Convert.ToString(dataGridView1[Convert.ToString(comboBox2.SelectedItem), SHP_Line].Value).Trim();
								// Check if numbers are defined
								if (comboBox4.SelectedIndex != 0)
								{
									name = name + '\t' + Convert.ToString(dataGridView1[Convert.ToString(comboBox4.SelectedItem), SHP_Line].Value).Trim();
								}
							}
							_bd.Name = name;
							
							//compute area
							calc_area(numbpt, pt);
							_bd.Area = (float) (Math.Round(areapolygon, 1));
							
							double height = St_F.TxtToDbl(Convert.ToString(dataGridView1[Convert.ToString(comboBox1.SelectedItem), SHP_Line].Value), false);
							height = Math.Round(height, 1);
							
							if (checkBox1.Checked == true) // absolute heights
							{
								height *= -1.0;
							}
							_bd.Height = (float) height;
							
							domain.EditB.ItemData.Add(_bd);
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
					dt.Dispose();
				
				#endif
				
				if (dt != null)
					dt.Dispose();
				
				if (dataGridView1 != null) dataGridView1.Dispose(); // Kuntner
				dataGridView1 = null;
				
				wait.Close();
				wait.Dispose();
				
			}
			else
			{
				MessageBox.Show("No heights defined - data is not imported", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			Close();
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
            }

            ParseDBF dbf_reader = new ParseDBF
            {
                dt = dt
            };
            dbf_reader.StartPosition = FormStartPosition.Manual;
            dbf_reader.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
            dbf_reader.Top = 80;
            dbf_reader.ReadDBF(file.Replace(".shp", ".dbf"));
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
			for (int i = 0; i < dataGridView1.Columns.Count; i++)
			{
				comboBox1.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox2.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox3.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox4.Items.Add(dataGridView1.Columns[i].HeaderText);
			}
			comboBox1.SelectedIndex = 0;
			comboBox2.SelectedIndex = 0;
			comboBox3.SelectedIndex = 0;
			comboBox4.SelectedIndex = 0;
			
			AcceptButton = button1;
			ActiveControl = button1;
			

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
                if (shpimport.ShowDialog() == DialogResult.OK)
                {
                    string header = shpimport.textBox1.Text;
                    dataGridView1.Columns.Add(header, header);
                    comboBox1.Items.Add(header);
                    comboBox2.Items.Add(header);
                    comboBox3.Items.Add(header);
                    comboBox4.Items.Add(header);
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
                            dataGridView1[header, i].Value = trans;
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
			string header = "Height";
			if (St_F.InputBoxValue("Remove column", "Name:", ref header, this) == DialogResult.OK)
			{
				dataGridView1.Columns.Remove(header);
				comboBox1.Items.Remove(header);
				comboBox2.Items.Remove(header);
				comboBox3.Items.Remove(header);
				comboBox4.Items.Remove(header);
			}
		}

		//compute area of a polygon
		public void calc_area(int numpoints, GralDomain.PointD[] polypoints)
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
			
		}
	}
}