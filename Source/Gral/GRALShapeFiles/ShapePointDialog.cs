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

using Gral;
using GralItemData;

namespace GralShape
{
	/// <summary>
    /// Dialog to import an item
    /// </summary>
    public partial class ShapePointDialog : Form
	{
		private GralDomain.Domain domain = null;
		private string file;
		private Deposition[] dep = new Deposition[10];
		public Button[] but1 = new Button[10];                     //Buttons for deposition
		private DataTable dt;
        private CultureInfo ic = CultureInfo.InvariantCulture;
        private string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;  
        private GralData.DomainArea GralDomRect;

        /// <summary>
        /// Dialog for the shape import of Point Sources
        /// </summary>
        /// <param name="d">Domain form reference</param>
        /// <param name="GralDomainRectangle">Bounds of GRAL domain rectangle</param>
        /// <param name="Filename">Filename of Shape File</param>
		public ShapePointDialog(GralDomain.Domain d, GralData.DomainArea GralDomainRectangle, string Filename)
		{
			InitializeComponent();
			domain = d;
			GralDomRect = GralDomainRectangle;
			file = Filename;
			button1.DialogResult = DialogResult.OK;

			for (int i = 0; i < Main.PollutantList.Count; i++)
			{
				comboBox5.Items.Add(Main.PollutantList[i]);
				comboBox6.Items.Add(Main.PollutantList[i]);
				comboBox7.Items.Add(Main.PollutantList[i]);
				comboBox8.Items.Add(Main.PollutantList[i]);
				comboBox9.Items.Add(Main.PollutantList[i]);
				comboBox10.Items.Add(Main.PollutantList[i]);
				comboBox11.Items.Add(Main.PollutantList[i]);
				comboBox12.Items.Add(Main.PollutantList[i]);
				comboBox13.Items.Add(Main.PollutantList[i]);
				comboBox14.Items.Add(Main.PollutantList[i]);
			}
			comboBox5.SelectedIndex = 0;
			comboBox6.SelectedIndex = 0;
			comboBox7.SelectedIndex = 0;
			comboBox8.SelectedIndex = 0;
			comboBox9.SelectedIndex = 0;
			comboBox10.SelectedIndex = 0;
			comboBox11.SelectedIndex = 0;
			comboBox12.SelectedIndex = 0;
			comboBox13.SelectedIndex = 0;
			comboBox14.SelectedIndex = 0;
			
			// init deposition data
			for (int i = 0; i < 10; i++)
			{
				dep[i] = new Deposition(); // initialize Gral.Deposition array
				dep[i].init();
			}
			
			int x = 0; int y = comboBox6.Top - 3 - comboBox14.Height; int a = comboBox14.Width + 5;
			for (int nr = 0; nr < 10; nr++)
			{
				if (nr == 5)
				{
					x = 0;
					y = comboBox14.Top - 3 - comboBox14.Height;
				}
                //Buttons for Gral.Deposition
                but1[nr] = new Button
                {
                    Width = comboBox14.Width,
                    Height = comboBox14.Height
                };

                int x_b = 0;
				if (x == 0)
					x_b = comboBox6.Left;
				else if (x == 1)
					x_b = comboBox5.Left;
				else if (x == 2)
					x_b = comboBox7.Left;
				else if (x == 3)
					x_b = comboBox8.Left;
				else
					x_b = comboBox9.Left;
				
				but1[nr].Location = new System.Drawing.Point(x_b, y);
				but1[nr].Font = new System.Drawing.Font("Microsoft Sans Serif", 2.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Millimeter, ((byte)(0)));
				but1[nr].Text = "Gral.Deposition " + (nr + 1).ToString();
				Controls.Add(but1[nr]);
				toolTip1.SetToolTip(but1[nr], "Click to set deposition - green: deposition set");
				but1[nr].Click += new EventHandler(edit_deposition);
				x++;
			}
		}

		//add point data
		private void button1_Click(object sender, EventArgs e)
		{
			if (comboBox4.SelectedIndex != 0 && comboBox4.SelectedItem != null)
			{
				GralMessage.Waitprogressbar wait = new GralMessage.Waitprogressbar("Import data base");
				wait.Show();
				
				bool inside;
                
				// for (int SHP_Line = 0; i < domain.shppoints[index].Count; SHP_Line++)
				int SHP_Line = 0;
				ShapeReader shape = new ShapeReader(domain);
                
                foreach (object shp in shape.ReadShapeFile(file))
				{
                    if (shp is PointF || shp is GralDomain.PointD)
					{
                        GralDomain.PointD _ptshp = (GralDomain.PointD)shp;
                        GralDomain.PointD pt = new GralDomain.PointD();
                        pt.X =  Math.Round(_ptshp.X, 1);
                		pt.Y =  Math.Round(_ptshp.Y, 1);
                        inside = false;
						
						//add only point sources inside the GRAL domain
						if ((pt.X > GralDomRect.West) && (pt.X < GralDomRect.East) && (pt.Y > GralDomRect.South) && (pt.Y < GralDomRect.North))
							inside = true;

						if (inside == true)
						{
							PointSourceData _psdata = new PointSourceData();
							
							//check for names
							if (comboBox3.SelectedIndex != 0)
							{
								_psdata.Name = Convert.ToString(dataGridView1[Convert.ToString(comboBox3.SelectedItem), SHP_Line].Value).Trim();
								if (_psdata.Name.Length < 1) // a name is needed
									_psdata.Name = "Point " + Convert.ToString(SHP_Line);
							}
							else
								_psdata.Name = "Point " + Convert.ToString(SHP_Line);
							
							
							//check for stack heights
							if (comboBox1.SelectedIndex != 0)
							{
								_psdata.Height = Convert.ToSingle(dataGridView1[Convert.ToString(comboBox1.SelectedItem), SHP_Line].Value, ic);
								_psdata.Height = (float) (Math.Round(_psdata.Height, 1));
							}
							else
								_psdata.Height = 0;
							
							//check for exit velocity
							if (comboBox2.SelectedIndex != 0)
							{
								_psdata.Velocity = Convert.ToSingle(dataGridView1[Convert.ToString(comboBox2.SelectedItem), SHP_Line].Value, ic);
								_psdata.Velocity = (float) (Math.Round(_psdata.Velocity, 1));
							}
							else
								_psdata.Velocity = 0;
							
							//check for source group
							int sgroup = 1;
							if (comboBox4.SelectedIndex != 0)
							{
								try
								{
									string _sgroup = Convert.ToString(dataGridView1[Convert.ToString(comboBox4.SelectedItem), SHP_Line].Value);
									// Plausibility check for source groups
									if (_sgroup == "")
										_sgroup = "1";
									
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
							
							_psdata.Poll.SourceGroup = sgroup;
							
							//check for exit temperature
							if (comboBox25.SelectedIndex != 0)
							{
								double tempkelvin = 273 + Convert.ToDouble(dataGridView1[Convert.ToString(comboBox25.SelectedItem), SHP_Line].Value, ic);
								_psdata.Temperature = (float) (Math.Round(tempkelvin, 1));
							}
							else
								_psdata.Temperature = 273;
							
							//check for stack diameter
							if (comboBox26.SelectedIndex != 0)
							{
								_psdata.Diameter = Convert.ToSingle(dataGridView1[Convert.ToString(comboBox26.SelectedItem), SHP_Line].Value, ic);
								_psdata.Diameter = (float) (Math.Round(_psdata.Diameter, 1));
							}
							
							double emission_factor = Convert.ToDouble(numericUpDown2.Value);
							if (checkBox2.Checked)
							{
								emission_factor = 1 / emission_factor;
							}
							
                            //check for pollutant1
                            if (comboBox19.SelectedIndex != 0)
								_psdata.Poll.EmissionRate[0] = Convert.ToDouble(dataGridView1[Convert.ToString(comboBox19.SelectedItem), SHP_Line].Value, ic) * emission_factor;
							
							_psdata.Poll.Pollutant[0] = comboBox6.SelectedIndex;
							
                            //check for pollutant2
							if (comboBox18.SelectedIndex != 0)
								_psdata.Poll.EmissionRate[1] = Convert.ToDouble(dataGridView1[Convert.ToString(comboBox18.SelectedItem), SHP_Line].Value, ic) * emission_factor;
							
							_psdata.Poll.Pollutant[1] = comboBox5.SelectedIndex;
							
                            //check for pollutant3
							if (comboBox17.SelectedIndex != 0)
								_psdata.Poll.EmissionRate[2] = Convert.ToDouble(dataGridView1[Convert.ToString(comboBox17.SelectedItem), SHP_Line].Value, ic) * emission_factor;
							_psdata.Poll.Pollutant[2] = comboBox7.SelectedIndex;
							
                            //check for pollutant4
							if (comboBox16.SelectedIndex != 0)
								_psdata.Poll.EmissionRate[3] = Convert.ToDouble(dataGridView1[Convert.ToString(comboBox16.SelectedItem), SHP_Line].Value, ic) * emission_factor;
							_psdata.Poll.Pollutant[3] = comboBox8.SelectedIndex;
							
                            //check for pollutant5
							if (comboBox15.SelectedIndex != 0)
								_psdata.Poll.EmissionRate[4] = Convert.ToDouble(dataGridView1[Convert.ToString(comboBox15.SelectedItem), SHP_Line].Value, ic) * emission_factor;
							_psdata.Poll.Pollutant[4] = comboBox9.SelectedIndex;
							
                            //check for pollutant6
							if (comboBox24.SelectedIndex != 0)
								_psdata.Poll.EmissionRate[4] = Convert.ToDouble(dataGridView1[Convert.ToString(comboBox24.SelectedItem), SHP_Line].Value, ic) * emission_factor;
							_psdata.Poll.Pollutant[5] = comboBox14.SelectedIndex;
							
                            //check for pollutant7
							if (comboBox23.SelectedIndex != 0)
								_psdata.Poll.EmissionRate[4] = Convert.ToDouble(dataGridView1[Convert.ToString(comboBox23.SelectedItem), SHP_Line].Value, ic) * emission_factor;
							_psdata.Poll.Pollutant[6] = comboBox13.SelectedIndex;
							
                            //check for pollutant8
							if (comboBox22.SelectedIndex != 0)
								_psdata.Poll.EmissionRate[4] = Convert.ToDouble(dataGridView1[Convert.ToString(comboBox22.SelectedItem), SHP_Line].Value, ic) * emission_factor;
							_psdata.Poll.Pollutant[7] = comboBox12.SelectedIndex;
							
                            //check for pollutant9
							if (comboBox21.SelectedIndex != 0)
								_psdata.Poll.EmissionRate[4] = Convert.ToDouble(dataGridView1[Convert.ToString(comboBox21.SelectedItem), SHP_Line].Value, ic) * emission_factor;
							_psdata.Poll.Pollutant[8] = comboBox11.SelectedIndex;
							
                            //check for pollutant10
							if (comboBox20.SelectedIndex != 0)
								_psdata.Poll.EmissionRate[4] = Convert.ToDouble(dataGridView1[Convert.ToString(comboBox20.SelectedItem), SHP_Line].Value, ic) * emission_factor;
							_psdata.Poll.Pollutant[9] = comboBox10.SelectedIndex;
							
							_psdata.Pt = pt;
							
							for (int di = 0; di < 10; di++) // save deposition
							{
								_psdata.GetDep()[di] = dep[di];
							}
							
							domain.EditPS.ItemData.Add(_psdata);
							
						}
					}
					SHP_Line++;
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
				MessageBox.Show(this, "No source group defined - data not imported", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			Close();
		}

		//read attribute table from dbf file
		private void Shape_Point_Dialog_Load(object sender, EventArgs e)
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
			comboBox15.Items.Add("None");
			comboBox16.Items.Add("None");
			comboBox17.Items.Add("None");
			comboBox18.Items.Add("None");
			comboBox19.Items.Add("None");
			comboBox20.Items.Add("None");
			comboBox21.Items.Add("None");
			comboBox22.Items.Add("None");
			comboBox23.Items.Add("None");
			comboBox24.Items.Add("None");
			comboBox25.Items.Add("None");
			comboBox26.Items.Add("None");
			for (int i = 0; i < dataGridView1.Columns.Count; i++)
			{
				comboBox1.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox2.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox3.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox4.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox15.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox16.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox17.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox18.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox19.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox20.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox21.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox22.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox23.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox24.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox25.Items.Add(dataGridView1.Columns[i].HeaderText);
				comboBox26.Items.Add(dataGridView1.Columns[i].HeaderText);
			}
			comboBox1.SelectedIndex = 0;
			comboBox2.SelectedIndex = 0;
			comboBox3.SelectedIndex = 0;
			comboBox4.SelectedIndex = 0;
			comboBox15.SelectedIndex = 0;
			comboBox16.SelectedIndex = 0;
			comboBox17.SelectedIndex = 0;
			comboBox18.SelectedIndex = 0;
			comboBox19.SelectedIndex = 0;
			comboBox20.SelectedIndex = 0;
			comboBox21.SelectedIndex = 0;
			comboBox22.SelectedIndex = 0;
			comboBox23.SelectedIndex = 0;
			comboBox24.SelectedIndex = 0;
			comboBox25.SelectedIndex = 0;
			comboBox26.SelectedIndex = 0;
			
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
                    //add value to column
                    string header = shpimport.textBox1.Text;
                    dataGridView1.Columns.Add(header, header);
                    comboBox1.Items.Add(header);
                    comboBox2.Items.Add(header);
                    comboBox3.Items.Add(header);
                    comboBox4.Items.Add(header);
                    comboBox15.Items.Add(header);
                    comboBox16.Items.Add(header);
                    comboBox17.Items.Add(header);
                    comboBox18.Items.Add(header);
                    comboBox19.Items.Add(header);
                    comboBox20.Items.Add(header);
                    comboBox21.Items.Add(header);
                    comboBox22.Items.Add(header);
                    comboBox23.Items.Add(header);
                    comboBox24.Items.Add(header);
                    comboBox25.Items.Add(header);
                    comboBox26.Items.Add(header);

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
			string header = "Header";
			if (GralStaticFunctions.St_F.InputBoxValue("Remove column", "Name:", ref header, this) == DialogResult.OK)
			{
				dataGridView1.Columns.Remove(header);
				comboBox1.Items.Remove(header);
				comboBox2.Items.Remove(header);
				comboBox3.Items.Remove(header);
				comboBox4.Items.Remove(header);
				comboBox15.Items.Remove(header);
				comboBox16.Items.Remove(header);
				comboBox17.Items.Remove(header);
				comboBox18.Items.Remove(header);
				comboBox19.Items.Remove(header);
				comboBox20.Items.Remove(header);
				comboBox21.Items.Remove(header);
				comboBox22.Items.Remove(header);
				comboBox23.Items.Remove(header);
				comboBox24.Items.Remove(header);
				comboBox25.Items.Remove(header);
				comboBox26.Items.Remove(header);
			}
		}
		
		void LabelMouseDoubleClick(object sender, MouseEventArgs e)
		{
			//MessageBox.Show(sender.ToString());
			edit_deposition(sender, e);
		}
		
		private void edit_deposition(object sender, EventArgs e)
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
					edit.Pollutant = comboBox6.SelectedIndex;
				else if (nr == 1)
					edit.Pollutant = comboBox5.SelectedIndex;
				else if (nr == 2)
					edit.Pollutant = comboBox7.SelectedIndex;
				else if (nr == 3)
					edit.Pollutant = comboBox8.SelectedIndex;
				else if (nr == 4)
					edit.Pollutant = comboBox9.SelectedIndex;
				else if (nr == 5)
					edit.Pollutant = comboBox14.SelectedIndex;
				else if (nr == 6)
					edit.Pollutant = comboBox13.SelectedIndex;
				else if (nr == 7)
					edit.Pollutant = comboBox12.SelectedIndex;
				else if (nr == 8)
					edit.Pollutant = comboBox11.SelectedIndex;
				else if (nr == 9)
					edit.Pollutant = comboBox10.SelectedIndex;
				
				if (edit.ShowDialog() == DialogResult.OK)
					edit.Hide();
				
				edit.Dispose();
				
				if (dep[nr].V_Dep1 > 0 || dep[nr].V_Dep2 > 0 || dep[nr].V_Dep3 > 0)
					but1[nr].BackColor = Color.LightGreen; // mark that deposition is set
				else
					but1[nr].BackColor = SystemColors.ButtonFace;
			}
		}
		
		private void comma1(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == ',') e.KeyChar = '.';
			int asc = (int)e.KeyChar; //get ASCII code
			switch (asc)
			{
					case 8: break;
					case 44: break;
					case 45: break;
					case 46: break;
					case 48: break;
					case 49: break;
					case 50: break;
					case 51: break;
					case 52: break;
					case 53: break;
					case 54: break;
					case 55: break;
					case 56: break;
					case 57: break;
				default:
					e.Handled = true; break;
			}
		}
		
		void Shape_Point_DialogFormClosed(object sender, FormClosedEventArgs e)
		{
			foreach(Button but in but1)
				but.Click -= new EventHandler(edit_deposition);
		}
		
	}
}