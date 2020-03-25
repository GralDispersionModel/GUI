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
 * Date: 16.09.2016
 * Time: 20:30
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using GralStaticFunctions;

namespace Gral
{
	public partial class Main
	{
		private void WriteGralLogFile(int select, string info, string file)
		{
			// Kuntner: writes a log File with Informations about the GRAMM field computation
			string log_path;
			string pathy;
			log_path = Path.Combine(ProjectName, @"Computation","Logfile_GRAL.txt");
			
			switch(select)
			{
				case 1: // Write Logfile at computation start
					try
					{
						using (StreamWriter mywriter = File.AppendText(log_path)) // Using closes mywriter and does error-handling
						{

							mywriter.WriteLine(" ");
							mywriter.WriteLine("".PadRight(120,'-'));
							mywriter.WriteLine(" ");
							mywriter.WriteLine("GRAL computation started at " + Convert.ToString(DateTime.Now));
							mywriter.WriteLine(file + "  \t  // GRAL Version");
							mywriter.WriteLine(" ");
							mywriter.WriteLine("Meteorology".PadRight(80,'='));
							mywriter.WriteLine(MetfileName + "  \t  // Windfield file");
							mywriter.WriteLine(Convert.ToString(numericUpDown1.Value).PadLeft(10) + "  \t  // Sectors");
							mywriter.WriteLine(textBox1.Text.PadLeft(10) + "  \t  // Sector size [deg]");
							mywriter.WriteLine(Convert.ToString(numericUpDown7.Value).PadLeft(10) + "  \t  // Anemometer height above ground level [m]");
							mywriter.WriteLine(Convert.ToString(numericUpDown2.Value).PadLeft(10) + "  \t  // Number of wind speed classes");
							mywriter.WriteLine("Wind speed classes");
							string lastvalue="0";
							for (int i = 0; i < WindSpeedClasses-1; i++)
							{
								mywriter.WriteLine(lastvalue.PadLeft(5) + " m/s  -  " + NumUpDown[i].Text.PadLeft(5) + "m/s" );
								lastvalue =  NumUpDown[i].Text;
							}
							
							mywriter.WriteLine(" ");
							mywriter.WriteLine("General".PadRight(40,'='));
							mywriter.WriteLine(Convert.ToString(numericUpDown4.Value).PadLeft(10) + "  \t  // Dispersion time");
							mywriter.WriteLine(Convert.ToString(numericUpDown6.Value).PadLeft(10) + "  \t  // Number of particles per second");
							mywriter.WriteLine(Convert.ToString(numericUpDown5.Value).PadLeft(10) + "  \t  // Start with dispersion situation");

							if (File.Exists(Landusefile))
                            {
                                mywriter.WriteLine(Landusefile + "\t  // Surface roughness from landuse file");
                            }
                            else
                            {
                                mywriter.WriteLine(numericUpDown38.Value.ToString().PadLeft(10) + "\t  // Surface roughness");
                            }

                            mywriter.WriteLine(" ");
							mywriter.WriteLine("Concentration grids".PadRight(40,'='));
							mywriter.WriteLine(Convert.ToString(numericUpDown8.Value).PadLeft(10) + "  \t  // Vertical thickness of concentration layers");
							mywriter.WriteLine(Convert.ToString(numericUpDown9.Value).PadLeft(10) + "  \t  // Horizontal grid resolution");
							mywriter.WriteLine(Convert.ToString(numericUpDown3.Value).PadLeft(10) + "  \t  // Number of horizontal slices");
							for (int i = 0; i < numericUpDown3.Value; i++)
							{
							    mywriter.WriteLine(TBox3[i].Value.ToString().PadLeft(10) + "\t  // Slice height above ground");
							}
							
							mywriter.WriteLine(" ");
							mywriter.WriteLine("GRAL Domain".PadRight(40,'='));
							mywriter.WriteLine(Convert.ToString(CellsGralX).PadLeft(10) + "  \t  // Number of cells for counting grid in x-direction");
							mywriter.WriteLine(Convert.ToString(CellsGralY).PadLeft(10) + "  \t  // Number of cells for counting grid in y-direction");
							mywriter.WriteLine(textBox6.Text.PadLeft(10) + "  \t  // Western border of GRAL model domain [m]");
							mywriter.WriteLine(textBox7.Text.PadLeft(10) + "  \t  // Eastern border of GRAL model domain [m]");
							mywriter.WriteLine(textBox5.Text.PadLeft(10) + "  \t  // Southern border of GRAL model domain [m]");
							mywriter.WriteLine(textBox2.Text.PadLeft(10) + "  \t  // Northern border of GRAL model domain [m]");
							
							
							mywriter.WriteLine(" ");
							mywriter.WriteLine("Topography".PadRight(40,'='));
							if (radioButton1.Checked)
                            {
                                mywriter.WriteLine("Flat terrain");
                            }
                            else
							{
								mywriter.WriteLine("Complex terrain");
								if (checkBox25.Checked)
                                {
                                    mywriter.WriteLine("Use original topographical data for GRAL");
                                }
                            }
							
							mywriter.WriteLine(" ");
							mywriter.WriteLine("Buildings".PadRight(40,'='));
							mywriter.WriteLine(GRALSettings.BuildingMode.ToString() + "\t  // Buildings microscale windfield model");
							if (GRALSettings.BuildingMode > 0) // microscale Windfield model
							{
								mywriter.WriteLine(Convert.ToString(numericUpDown10.Value).PadLeft(14) + "  \t  // Horizontal Grid resolution");
								mywriter.WriteLine(Convert.ToString(numericUpDown11.Value).PadLeft(14) + "  \t  // Vertical thickness of first layer");
								mywriter.WriteLine(Convert.ToString(numericUpDown12.Value).PadLeft(14) + "  \t  // Vertical stretching factor");
								
								double n = Convert.ToDouble(numericUpDown10.Text);
								double c = Convert.ToDouble(numericUpDown9.Text);
								mywriter.WriteLine(Convert.ToString(Math.Round(CellsGralX / n * c,0)).PadLeft(14) + "  \t  // Numbers of flow field cells in x-direction");
								mywriter.WriteLine(Convert.ToString(Math.Round(CellsGralY / n * c,0)).PadLeft(14) + "  \t  // Numbers of flow field cells in y-direction");
								mywriter.WriteLine(Convert.ToString(numericUpDown26.Value).PadLeft(14) + "  \t  // Numbers of flow field cells in z-direction");
								
//							mywriter.WriteLine(Convert.ToString(numericUpDown28.Value).PadLeft(14) + "  \t  // Relaxation velocity");
//							mywriter.WriteLine(Convert.ToString(numericUpDown27.Value).PadLeft(14) + "  \t  // Relaxation pressure correction");
								
								if (checkBox22.Checked)
								{
									mywriter.WriteLine("Write until steady-state");
								}
								mywriter.WriteLine(Convert.ToString(numericUpDown29.Value).PadLeft(14) + "  \t  // Maximum iterations");
								mywriter.WriteLine(Convert.ToString(numericUpDown30.Value).PadLeft(14) + "  \t  // Minimum iterations");
								mywriter.WriteLine(Convert.ToString(numericUpDown31.Value).PadLeft(14) + "  \t  // Roughness of building walls");
							}
							
							mywriter.WriteLine(" ");
							
							if (GRALSettings.Compressed == 1)
                            {
                                mywriter.WriteLine("Write compressed GRAL result files");
                            }
                            else
                            {
                                mywriter.WriteLine("Write uncompressed GRAL result files");
                            }

                            mywriter.WriteLine(" ");
							mywriter.WriteLine("Receptor points".PadRight(40,'='));
							
							pathy = Path.Combine(ProjectName, @"Computation","Receptor.dat");
							if (File.Exists(pathy))
							{
								try{
									using (StreamReader myreader = new StreamReader(pathy))
									{
										string text;
										text = myreader.ReadLine(); // first line in trashbox
										while ((text = myreader.ReadLine()) != null)
										{
											mywriter.WriteLine(text.Replace(',','\t'));
										}
									}
								}
								catch
								{}
							}
							
							mywriter.WriteLine(" ");
							mywriter.WriteLine("GRAL Sources".PadRight(40,'='));

							if (DefinedSourceGroups.Count > 0)
							{
								foreach (SG_Class _sg in DefinedSourceGroups)
								{
									mywriter.WriteLine(Convert.ToString(_sg.SG_Name).PadLeft(10) + "\t   Defined source-groups");
								}
							}

							
							pathy = Path.Combine(ProjectName, @"Computation","point.dat");
							if (File.Exists(pathy))
							{
								try
								{
									long linennumber = St_F.CountLinesInFile(pathy); // gets number of lines
									mywriter.WriteLine("Point sources".PadRight(40,'='));
									using (StreamReader myreader = new StreamReader(pathy))
									{
										mywriter.WriteLine(Convert.ToString(linennumber-2).PadLeft(10) + "\t   Point sources");
										if (linennumber >2 && linennumber <52)
										{
											string text;
											text = myreader.ReadLine();  //first line in trashbox
											while (!myreader.EndOfStream)
											{
												text = myreader.ReadLine();
												mywriter.WriteLine(text.Replace(',','\t'));
											}
										}
										else
										{
											mywriter.WriteLine(pathy);
										}
									}
								}
								catch{}
							}
							
							pathy = Path.Combine(ProjectName, @"Computation","portals.dat");
							if (File.Exists(pathy))
							{
								try
								{
									long linennumber = St_F.CountLinesInFile(pathy); // gets number of lines
									mywriter.WriteLine("Portal sources".PadRight(40,'='));
									using (StreamReader myreader = new StreamReader(pathy))
									{
										mywriter.WriteLine(Convert.ToString(linennumber-2).PadLeft(10) + "\t   Portal sources");
										if (linennumber >2 && linennumber <52)
										{
											string text;
											text = myreader.ReadLine();  //first line in trashbox
											while (!myreader.EndOfStream)
											{
												text = myreader.ReadLine();
												mywriter.WriteLine(text.Replace(',','\t'));
											}
										}
										else
										{
											mywriter.WriteLine(pathy);
										}
									}
								}
								catch
								{}
							}

							
							pathy = Path.Combine(ProjectName, @"Emissions","Asources.txt");
							if (File.Exists(pathy))
							{
								try
								{
									long linennumber = St_F.CountLinesInFile(pathy); // gets number of lines
									mywriter.WriteLine("Area sources".PadRight(40,'='));
									using (StreamReader myreader = new StreamReader(pathy))
									{
										mywriter.WriteLine(Convert.ToString(linennumber-2).PadLeft(10) + "\t   Area sources");
										if (linennumber >2 && linennumber <52)
										{
											string text;
											text = myreader.ReadLine();  //first line in trashbox
											while (!myreader.EndOfStream)
											{
												text = myreader.ReadLine();
												mywriter.WriteLine(text.Replace(',','\t'));
											}
										}
										else
										{
											mywriter.WriteLine(pathy);
										}
									}
								}
								catch{}
								
							}

							pathy = Path.Combine(ProjectName, @"Emissions","Lsources.txt");
							if (!File.Exists(pathy))
							{
								pathy = Path.Combine(ProjectName, @"Emissions", "LineSourceData.txt");
							}
							if (File.Exists(pathy))
							{
								try
								{
									long linennumber = St_F.CountLinesInFile(pathy); // gets number of lines
									mywriter.WriteLine("Line sources".PadRight(40,'='));
									using (StreamReader myreader = new StreamReader(pathy))
									{
										mywriter.WriteLine(Convert.ToString(linennumber-2).PadLeft(10) + "\t  Line sources");
										if (linennumber >2 && linennumber <52)
										{
											string text;
											text = myreader.ReadLine();  //first line in trashbox
											while (!myreader.EndOfStream)
											{
												text = myreader.ReadLine();
												mywriter.WriteLine(text.Replace(',','\t'));
											}
										}
										else
										{
											mywriter.WriteLine(pathy);
										}
									}
								}
								catch{}
								
								
							}
						}
					}
					catch{}
					break;
					
				case 2: // Write Log File
					try
					{
						using (StreamWriter mywriter = File.AppendText(log_path)) // Using closes mywriter and does error-handling
						{
							
							mywriter.WriteLine(" ");
							mywriter.WriteLine("".PadRight(120,'-'));
							mywriter.WriteLine(" ");
							mywriter.WriteLine("GRAL post-processing started at " + Convert.ToString(DateTime.Now));
							mywriter.WriteLine(" ");
							mywriter.WriteLine(file);
							mywriter.WriteLine(info);
							
						}
					}
					catch{}
					break;
					
				case 3: // Write Log File
					try
					{
						using (StreamWriter mywriter = File.AppendText(log_path)) // Using closes mywriter and does error-handling
						{
							
							mywriter.WriteLine(" ");
							mywriter.WriteLine("".PadRight(120,'-'));
							mywriter.WriteLine(" ");
							mywriter.WriteLine("GRAL breaked at " + Convert.ToString(DateTime.Now));
							mywriter.WriteLine(" ");
							if (numericUpDown3.Value != 0 && DefinedSourceGroups.Count != 0)
							{
								if (numericUpDown43.Value > 0) // compressed results
								{
									mywriter.WriteLine(Convert.ToString(CountConFiles()).PadLeft(10) + "\t Number of calculated weather situations");
								}
								else
								{
									mywriter.WriteLine(Convert.ToString(CountConFiles() / Convert.ToInt32(numericUpDown3.Value) /
									                                    Convert.ToInt32(DefinedSourceGroups.Count)).PadLeft(10) + "\t Number of calculated weather situations"); // divide by number of vertical slices and sourcegroups
								}
							}
							pathy = Path.Combine(ProjectName, @"Computation","meteopgt.all");
							if (File.Exists(pathy))
							{
								mywriter.WriteLine(Convert.ToString(St_F.CountLinesInFile(pathy)-2).PadLeft(10) + "\t Number of weather situations");
							}
							
						}
					}
					catch{}
					break;
					
			}
		}
		
		private void Write_Gramm_Log(int select, string info, string file)
		{
			// Kuntner: writes a log File with Informations about the GRAMM field computation
			string log_path;
			log_path = Path.Combine(ProjectName, "Computation","Logfile_GRAMM.txt");
			try
			{
				using (StreamWriter mywriter = File.AppendText(log_path)) // Using closes mywriter and does error-handling
				{
					switch(select)
					{
						case 1: // new GRAMM computation started
							mywriter.WriteLine(" ");
							mywriter.WriteLine("".PadRight(120,'-'));
							mywriter.WriteLine(" ");
							mywriter.WriteLine("GRAMM computation started at " + Convert.ToString(DateTime.Now));
							mywriter.WriteLine(file + "  \t  // GRAMM Version");
							mywriter.WriteLine(" ");
							mywriter.WriteLine("Meteorology".PadRight(40,'='));
							mywriter.WriteLine(MetfileName + "  \t  // Windfield file");
							mywriter.WriteLine(Convert.ToString(numericUpDown1.Value).PadLeft(10) + "  \t  // Sectors");
							mywriter.WriteLine(textBox1.Text.PadLeft(10) + "  \t  // Sector size [deg]");
							mywriter.WriteLine(Convert.ToString(numericUpDown7.Value).PadLeft(10) + "  \t  // Anemometer height above ground level [m]");
							mywriter.WriteLine(Convert.ToString(numericUpDown2.Value).PadLeft(10) + "  \t  // Number of wind speed classes");
							mywriter.WriteLine("Wind speed classes");
							string lastvalue="0";
							for (int i = 0; i < WindSpeedClasses-1; i++)
							{
								mywriter.WriteLine(lastvalue.PadLeft(5) + " m/s  -  " + NumUpDown[i].Text.PadLeft(5) + "m/s" );
								lastvalue =  NumUpDown[i].Text;
							}
							
							mywriter.WriteLine(" ");
							mywriter.WriteLine("Topography".PadRight(40,'='));
							if (checkBox31.Checked == true)
                            {
                                mywriter.WriteLine("Flat terrain option activated");
                            }

                            mywriter.WriteLine(Topofile + "  \t  // Topography file");
							mywriter.WriteLine(Landusefile + "  \t  // Landuse file");
							mywriter.WriteLine(textBox15.Text.PadLeft(10) + "  \t  // Northern border of GRAMM model domain [m]");
							mywriter.WriteLine(textBox12.Text.PadLeft(10) + "  \t  // Eastern border of GRAMM model domain [m]");
							mywriter.WriteLine(textBox13.Text.PadLeft(10) + "  \t  // Western border of GRAMM model domain [m]");
							mywriter.WriteLine(textBox14.Text.PadLeft(10) + "  \t  // Southern border of GRAMM model domain [m]");
							mywriter.WriteLine(Convert.ToString(Convert.ToInt32((GrammDomRect.East - GrammDomRect.West) / GRAMMHorGridSize)).PadLeft(10) + "  \t  // Number of cells in x-direction");
							mywriter.WriteLine(Convert.ToString(Convert.ToInt32((GrammDomRect.North - GrammDomRect.South) / GRAMMHorGridSize)).PadLeft(10) + "  \t  // Number of cells in y-direction");
							mywriter.WriteLine(Convert.ToString(numericUpDown17.Value).PadLeft(10) + "  \t  // Vertical thickness of first layer");
							mywriter.WriteLine(Convert.ToString(numericUpDown16.Value).PadLeft(10) + "  \t  // Number of cells in z-direction");
							mywriter.WriteLine(Convert.ToString(numericUpDown19.Value).PadLeft(10) + "  \t  // Vertical streching factor");
							
							for (int i = 0; i < GRAMMmodelheight.Items.Count; i++)
                            {
                                mywriter.WriteLine(Convert.ToString(GRAMMmodelheight.Items[i]).PadLeft(10) + "  \t  // Relative top level height");
                            }

                            mywriter.WriteLine(" ");
							mywriter.WriteLine("GRAMM input".PadRight(40,'='));
							mywriter.WriteLine(Convert.ToString(numericUpDown20.Value).PadLeft(10) + "  \t  // Max. time step [s]");
							mywriter.WriteLine(Convert.ToString(numericUpDown21.Value).PadLeft(10) + "  \t  // Modelling time [s]");
							mywriter.WriteLine(Convert.ToString(numericUpDown22.Value).PadLeft(10) + "  \t  // Relaxation velocity");
							mywriter.WriteLine(Convert.ToString(numericUpDown23.Value).PadLeft(10) + "  \t  // Relaxation scalars");
							
							mywriter.WriteLine(info + "  \t  // Dispersion situations");
							if (checkBox30.Checked) // sunrise option selected
                            {
                                mywriter.WriteLine("Sunrise option activated - original meteopgt.all line numbers: " + GRAMM_Sunrise.ToString());
                            }
                            else
                            {
                                mywriter.WriteLine("Sunrise option not activated");
                            }

                            break;
					}
				}
				
			}
			catch{}
		}
		
	}
	
}
