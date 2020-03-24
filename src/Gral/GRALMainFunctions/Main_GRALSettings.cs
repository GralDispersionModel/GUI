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

/*
 * Created by SharpDevelop.
 * User: Markus
 * Date: 17.01.2019
 * Time: 17:08
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Windows.Forms;
using GralIO;
using System.Drawing;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
	{
        /// <summary>
        /// change the number of horizontal slices of the couting grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetNumberOfHorizontalConcentrationGridSlices(object sender, EventArgs e)
		{
			Graphics g = CreateGraphics();
			float dx = 96;
			try
			{
				dx = g.DpiX;
			}
			finally
			{
				g.Dispose();
			}

			for (int i = 1; i < GRALSettings.NumHorSlices + 1; i++)
			{
				RemoveTextbox1(i);
			}

            GRALSettings.NumHorSlices = Convert.ToInt32(numericUpDown3.Value);
			TBox3[0].Value = Math.Ceiling(numericUpDown8.Value);
			for (int i = 1; i < GRALSettings.NumHorSlices; i++)
			{
				CreateTextbox1(Convert.ToInt32(209 * dx / 96) + Convert.ToInt32((i * 30) * dx / 96), 80, 22, i);
				TBox3[i].Value = (decimal)(Math.Round(i * (double)numericUpDown8.Value) + (double)TBox3[0].Value);
			}
			WriteGralGebFile();
			ResetInDat();
		}

        /// <summary>
        /// save control input file In.dat
        /// </summary>
        public void GenerateInDat()
		{
			try
			{
                string filename = Path.Combine("Computation", "in.dat");
				string newPath = Path.Combine(ProjectName, filename);

                //get vertical grid extension for GRAL counting grid
                GRALSettings.Deltaz = Convert.ToDouble(numericUpDown8.Value);

				int ok = CheckGralInputData();
				//save all input data to files "in.dat"
				if (ok > 0)
				{
                    GRALSettings.DispersionTime = Convert.ToInt32(numericUpDown4.Value);
                    GRALSettings.ParticleNumber = Convert.ToInt32(numericUpDown6.Value);
                    GRALSettings.DispersionSituation = Convert.ToInt32(numericUpDown5.Value);
					
                    //save data to "in.dat"
                    GRALSettings.InDatPath = newPath;
                    GRALSettings.BuildingHeightsWrite = checkBox23.Checked;
                    GRALSettings.PrognosticSubDomains = (int)numericUpDown42.Value;
               
					if (checkBox32.Checked == true)
                        GRALSettings.Transientflag = 0;
					else
                        GRALSettings.Transientflag = 1;

					InDatFileIO writer = new InDatFileIO
					{
						Data = GRALSettings
					};
					if (writer.WriteInDat()) // if writing = OK
					{
						Change_Label(0, 2); // green dot at conrol button
					}
					else // error writing in.dat
					{
						Change_Label(0, 0); // red dot at control button
					}
					writer = null;
				}
			}
			catch
			{
				MessageBox.Show("Please define Project Folder first. Click on the 'New' or 'Open' button", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			//enable/disable GRAL simulations
			Enable_GRAL();
		}

        /// <summary>
        /// change the horizontal grid resolution of the couting grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetGRALConcentrationHorizontalGridSize(object sender, EventArgs e)
		{
			try
			{
				if (EmifileReset == true) // not if a project is loaded
				{
					//when the horizontal grid size of the concentration grid is changed the model domain has to be defined again
					HorGridSize = Convert.ToDouble(Math.Round(numericUpDown9.Value, 0));
					textBox2.Text = "";
					textBox5.Text = "";
					textBox6.Text = "";
					textBox7.Text = "";
					GralDomRect.East = 0;
					GralDomRect.North = 0;
					GralDomRect.South = 0;
					GralDomRect.West = 0;
					listBox4.Items.Clear();
					listView1.Items.Clear();

					try
					{
						string newPath = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
						DirectoryInfo di = new DirectoryInfo(newPath);
						FileInfo[] files_emission = di.GetFiles("emissions*.dat");
						for (int i = 0; i < files_emission.Length; i++)
							files_emission[i].Delete();
					}
					catch { }

					Change_Label(3, 0); // Building label red & delete buildings.dat

					numericUpDown10.ValueChanged -= new System.EventHandler(NumericUpDown10_ValueChanged); // remove event listener
					numericUpDown10.Value = 1;
					numericUpDown10.Maximum = numericUpDown9.Value; // Maximum of Flow Grid = counting grid!
					numericUpDown10.Value = numericUpDown9.Value;
					numericUpDown10.ValueChanged += new System.EventHandler(NumericUpDown10_ValueChanged); // add event listener

				}
			}
			catch
			{
			}
			ResetInDat();
		}

        /// <summary>
        /// changing the horizontal grid size of the cartesian grid in GRAL used for wind field calculations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetGRALFowFieldHorizontalGridSize(object sender, EventArgs e)
		{
			//try
			{
				if (EmifileReset == true)
				{
					try
					{
						string newPath1 = Path.Combine(ProjectName, @"Computation", "GRAL.geb");
						File.Delete(newPath1);
					}
					catch { }

					Change_Label(3, 0); // Building label red & delete buildings.dat

					WriteGralGebFile();

					DeleteGralTopofile();
					DeleteGralGffFile();
				}

				if (numericUpDown10.Value != numericUpDown9.Value) // if value is changed -> values allowed as val divided by 2!
				{
					numericUpDown10.ValueChanged -= new System.EventHandler(NumericUpDown10_ValueChanged); // remove event listener
					decimal val = Convert.ToDecimal(numericUpDown10.Text); // get last value (still in the Text)

					if ((numericUpDown10.Value > numericUpDown9.Value) || val < numericUpDown10.Value)
					{
						int wertint = (int)(numericUpDown10.Value / numericUpDown9.Value);
						decimal wertdec = (numericUpDown10.Value / numericUpDown9.Value);
						if ((decimal)wertint != wertdec)
						{
							//search for the next lower value until a natural divisor is found
							wertint = (int)(Math.Floor(numericUpDown10.Value / numericUpDown9.Value));
							if (wertint != 0)
								numericUpDown10.Value = numericUpDown9.Value * (decimal)wertint;
							else
								numericUpDown10.Value = numericUpDown9.Value;
						}
					}
					else
					{
						int wertint = (int)(numericUpDown9.Value / numericUpDown10.Value);
						decimal wertdec = (numericUpDown9.Value / numericUpDown10.Value);
						if ((decimal)wertint != wertdec)
						{
							//search for the next lower value until a natural divisor is found
							wertint = (int)(Math.Ceiling(numericUpDown9.Value / numericUpDown10.Value));
							numericUpDown10.Value = numericUpDown9.Value / (decimal)wertint;
						}

					}

					/*
					if (val > numericUpDown10.Value) // increase button
					{
						if ((decimal)(val / 2)  > (decimal) 1.2 && Math.Round(val /2, 2) == (val /2))
							numericUpDown10.Value = (decimal) (val / 2);
						else
							numericUpDown10.Value = (decimal) (val);
					}
					else
					{
						if ((val * 2) <= numericUpDown10.Maximum)
							numericUpDown10.Value = val * 2;
						else
							numericUpDown10.Value = numericUpDown10.Maximum;
					}
					 */

					numericUpDown10.ValueChanged += new System.EventHandler(NumericUpDown10_ValueChanged); // add event listener
				}
			}
			//catch
			{
			}
		}

        /// <summary>
        /// change the way, buildings are treated No/diagnostic/prognostic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetBuildingMethode(object sender, EventArgs e)
		{
			int buildings = GRALSettings.BuildingMode;
			if ((buildings == 0) || (buildings == 3))
				groupBox8.Visible = false;
			else
			{
				groupBox8.Visible = true;

				try
				{
					if (EmifileReset == true)
					{
						string newPath1 = Path.Combine(ProjectName, @"Computation", "buildings.txt");
						if (File.Exists(newPath1))
						{
							Change_Label(3, 0); // Building label red
							button9.Visible = true;
						}
						newPath1 = Path.Combine(ProjectName, @"Emissions", "Walls.txt");
						if (File.Exists(newPath1))
						{
							Change_Label(3, 0); // Building label red
							button9.Visible = true;
						}
						newPath1 = Path.Combine(ProjectName, @"Computation", "Vegetation.txt");
						if (File.Exists(newPath1))
						{
							Change_Label(3, 0); // Building label red
							button9.Visible = true;
						}
					}
				}
				catch
				{
				}
			}
			if (buildings == 0)
			{
				try
				{
					if (EmifileReset == true)
					{
						string newPath1 = Path.Combine(ProjectName, @"Computation", "buildings.dat");
						File.Delete(newPath1);
						Change_Label(3, -1); // Building not available
						button9.Visible = false;
					}
				}
				catch
				{
				}
			}
			ResetInDat();

			numericUpDown10.Maximum = numericUpDown9.Value; // Maximum of Flow Grid = counting grid!
			//numericUpDown10.Value = numericUpDown9.Value;
			//enable/disable GRAL simulations
			Enable_GRAL();
			//enable/disable GRAMM simulations
			Enable_GRAMM();
		}

		private void SetWetDepositionData()
		{
			string poll = Convert.ToString(listBox5.SelectedItem);
			if (poll.Contains("PM10"))
			{
				numericUpDown36.Value = (decimal)(1.5e-4 * 1e6D);
				numericUpDown35.Value = (decimal)0.8;
			}
			else if (poll.Contains("PM2.5"))
			{
				numericUpDown36.Value = (decimal)(3e-5 * 1e6D);
				numericUpDown35.Value = (decimal)0.8;
			}
			else if (poll.Contains("SO2"))
			{
				numericUpDown36.Value = (decimal)(2e-5 * 1e6D);
				numericUpDown35.Value = (decimal)1;
			}
			else if (poll.Contains("NO2"))
			{
				numericUpDown36.Value = (decimal)(1e-7 * 1e6D);
				numericUpDown35.Value = (decimal)1;
			}
			else if (poll.Contains("NH3"))
			{
				numericUpDown36.Value = (decimal)(1.2e-4 * 1e6D);
				numericUpDown35.Value = (decimal)0.6;
			}
			else
			{
				numericUpDown36.Value = (decimal)0;
				numericUpDown35.Value = (decimal)0;
			}
		}

        /// <summary>
        /// changing the thickness of the vertical layer of the cartesian grid in GRAL used for wind field calculations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetGRALFlowFieldVerticalGridSize(object sender, EventArgs e)
		{
			try
			{
				if (EmifileReset == true)
				{
					string newPath1 = Path.Combine(ProjectName, @"Computation", "GRAL.geb");
					File.Delete(newPath1);
					WriteGralGebFile();

					DeleteGralGffFile();
                }
            }
			catch
			{
			}
		}

        /// <summary>
        /// changing the streching factor of the vertical cartesian grid in GRAL used for wind field calculations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetGRALFlowFieldStretchingFactor(object sender, EventArgs e)
		{
			try
			{
				if (EmifileReset == true)
				{
					string newPath1 = Path.Combine(ProjectName, @"Computation", "GRAL.geb");

					File.Delete(newPath1);
					WriteGralGebFile();

					DeleteGralGffFile();
                }
            }
			catch
			{
			}
		}

        /// <summary>
        /// define the number of cells in vertical direction for the microscale flow field model of GRAL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetGRALFlowFieldVerticalCellNumber(object sender, EventArgs e)
		{
			try
			{
				if (EmifileReset == true)
				{
					string newPath1 = Path.Combine(ProjectName, @"Computation", "micro_vert_layers.txt");
					try
					{
						using (StreamWriter mywriter = new StreamWriter(newPath1))
						{
							mywriter.WriteLine(Convert.ToString(numericUpDown26.Value, ic));
						}
					}
					catch
					{
						MessageBox.Show("Could not write file \"Computation\\micro_vert_layers.txt\"", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
            }
			catch
			{
			}
		}

        /// <summary>
        /// change relaxation factor of velocity for microscale flow field model of GRAL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetGRALFlowFieldVelRelaxFactor(object sender, EventArgs e)
		{
			try
			{
				if (EmifileReset == true)
				{
					string newPath1 = Path.Combine(ProjectName, @"Computation", "relaxation_factors.txt");
					try
					{
						using (StreamWriter mywriter = new StreamWriter(newPath1))
						{
							mywriter.WriteLine(Convert.ToString(numericUpDown28.Value, ic));
							mywriter.WriteLine(Convert.ToString(numericUpDown27.Value, ic));
						}
					}
					catch
					{
						MessageBox.Show("Could not write file \"Computation\\relaxation_factors.txt\"", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
			catch
			{
			}
		}

        /// <summary>
        /// change relaxation factor for pressure correction of microscale wind field model of GRAL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetGRALFlowFieldPreRelaxFactor(object sender, EventArgs e)
		{
			try
			{
				if (EmifileReset == true)
				{
					string newPath1 = Path.Combine(ProjectName, @"Computation", "relaxation_factors.txt");
					try
					{
						using (StreamWriter mywriter = new StreamWriter(newPath1))
						{
							mywriter.WriteLine(Convert.ToString(numericUpDown28.Value, ic));
							mywriter.WriteLine(Convert.ToString(numericUpDown27.Value, ic));
						}
					}
					catch
					{
						MessageBox.Show("Could not write file \"Computation\\relaxation_factors.txt\"", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
			catch
			{
			}
		}

        /// <summary>
        /// change minimum number of iterations over timesteps in the microscale flow field model of GRAL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetGRALFlowFieldMinIterationNumber(object sender, EventArgs e)
		{
			try
			{
				if (EmifileReset == true)
				{
					//number of maximum iteratsions needs to be larger than the minimum number of iterations
					if (numericUpDown30.Value < numericUpDown29.Value)
						numericUpDown30.Value = Math.Min(numericUpDown29.Value + 1, 10001);

					string newPath1 = Path.Combine(ProjectName, @"Computation", "Integrationtime.txt");
					try
					{
						using (StreamWriter mywriter = new StreamWriter(newPath1))
						{
							mywriter.WriteLine(Convert.ToString(numericUpDown29.Value, ic));
							mywriter.WriteLine(Convert.ToString(numericUpDown30.Value, ic));
						}
					}
					catch
					{
						MessageBox.Show("Could not write file \"Computation\\Integrationtime.txt\"", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
			catch
			{
			}
		}

        /// <summary>
        /// change the number of maximum iterations over timesteps in the microscale flow field model of GRAL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetGRALFlowFieldMaxIterationNumber(object sender, EventArgs e)
		{
			try
			{
				if (EmifileReset == true)
				{
					string newPath1 = Path.Combine(ProjectName, @"Computation", "Integrationtime.txt");
					try
					{
						using (StreamWriter mywriter = new StreamWriter(newPath1))
						{
							mywriter.WriteLine(Convert.ToString(numericUpDown29.Value, ic));
							mywriter.WriteLine(Convert.ToString(numericUpDown30.Value, ic));
						}
					}
					catch
					{
						MessageBox.Show("Could not write file \"Computation\\Integrationtime.txt\"", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
			catch
			{
			}
		}

        /// <summary>
        /// define roughness of building walls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetGRALFlowFieldBuildingRoughness(object sender, EventArgs e)
		{
			try
			{
				if (EmifileReset == true)
				{
					string newPath1 = Path.Combine(ProjectName, @"Computation", "building_roughness.txt");
					try
					{
						using (StreamWriter mywriter = new StreamWriter(newPath1))
						{
							mywriter.WriteLine(Convert.ToString(numericUpDown31.Value, ic));
						}
					}
					catch
					{
						MessageBox.Show("Could not write file \"Computation\\building_roughness.txt\"", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
			catch
			{
			}
		}

        /// <summary>
        /// decide, whether iterations shall be performed until steady-state conditions are reached, or not
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SetGRALFlowFieldSteadyStateIteration(object sender, EventArgs e)
		{
			if (checkBox22.Checked == true)
			{
				numericUpDown29.Value = 10000;
				numericUpDown30.Value = 10001;
			}
			else
			{
				numericUpDown29.Value = 100;
				numericUpDown30.Value = 500;
			}
		}
		
		//decide, whether iterations shall be performed until steady-state conditions are reached, or not
		private void SetGRALFlowFieldSteadyStateIterationChanged(object sender, EventArgs e)
		{
			if (checkBox22.Checked == true)
			{
				numericUpDown29.Value = 10000;
				numericUpDown30.Value = 10001;
				numericUpDown29.Visible = false;
				numericUpDown30.Visible = false;
				label78.Visible = false;
				label79.Visible = false;
			}
			else
			{
				numericUpDown29.Visible = true;
				numericUpDown30.Visible = true;
				label78.Visible = true;
				label79.Visible = true;
			}
		}

	}
}
