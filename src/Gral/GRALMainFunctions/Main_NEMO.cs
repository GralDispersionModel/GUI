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
 * Date: 28.10.2016
 * Time: 18:26
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using GralStaticFunctions;
using GralItemData;

namespace Gral
{
	public partial class Main
	{
		public class Nemo
		{
			//////////////////////////////////////////////////////////////////////////
			//
			//generate NEMO input files
			//
			//////////////////////////////////////////////////////////////////////////
			private bool sep;                                     //flag determining if line source emissions computed by NEMO are split into several source groups
			private int[] sgroups = { 1, 1, 1, 1 };               //source groups road emissions splitting after NEMO computations
			private string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;    //decimal separator of the system
			private CultureInfo ic = CultureInfo.InvariantCulture;
			
			public void NemoInput(bool seperation,int sg1,int sg2,int sg3,int sg4)
			{
				sep = seperation;
                //select directory of NEMO executable
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "(Nemo*.exe)|Nemo*.exe",
                    Title = "Select NEMO executable"
                };
                string inputfile;
				string newPath;
				sgroups[0] = sg1;
				sgroups[1] = sg2;
				sgroups[2] = sg3;
				sgroups[3] = sg4;
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						// Load Line Source Data
						List<LineSourceData> ItemData = new List<LineSourceData>(); //collection of all line source data
						LineSourceDataIO _ls = new LineSourceDataIO();
						string _file = Path.Combine(Main.ProjectName,"Emissions","Lsources.txt");
						_ls.LoadLineSources(ItemData, _file);
						_ls = null;
						
						//search for the file "nemostat.txt"
						//string[] filePaths = Directory.GetFiles(Path.GetDirectoryName(dialog.FileName), "nemostat.txt", SearchOption.AllDirectories);
						//copy job file "GRAL.nemo"
						File.Copy(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath),@"GRAL.nemo"),Path.Combine(Path.GetDirectoryName(dialog.FileName),@"GRAL.nemo"),true);
						string[] filePaths = Directory.GetFiles(Path.GetDirectoryName(dialog.FileName), "GRAL.nemo", SearchOption.AllDirectories);

						//retrieve information about directories, where NEMO input files have to be placed (stored in GRAL.nemo)
						//Achtung ab NEMO2.0 muessen die Kommentarzeilen, beginnend mit c+Leerzeichen übersprungen werden
						StreamReader myreader = new StreamReader(filePaths[0]);
						inputfile = myreader.ReadLine();
						int count = 0;
						while (inputfile != "")
						{
							if (inputfile.Length > 1)
							{
								if ((inputfile.Substring(0, 2) != "c ") && (inputfile.Substring(0, 2) != "C "))
								{
									count = count + 1;
								}
								if (count == 7)
									break;
							}
							inputfile = myreader.ReadLine();
						}
						myreader.Close();

						//write location of NEMO input file into the NEMO control file (usually "Strassennetz.txt")
						File.Delete(inputfile);
						newPath = Path.Combine(Main.ProjectName, @"Emissions","NEMO.csv");
						StreamWriter mywriter = new StreamWriter(inputfile);
						mywriter.WriteLine(newPath,Encoding.Default);
						//mywriter.WriteLine("e");
						mywriter.Close();

						//generate temporary output directory for NEMO results
						string newdir = Path.Combine(Main.ProjectName, "Ergebnisse");
						System.IO.Directory.CreateDirectory(newdir);

						//generate NEMO input file NEMO.csv
						GralMessage.MessageWindow message = new GralMessage.MessageWindow();
						message.listBox1.Items.Add("Generating NEMO input file....");
						message.Show();

						//remove existing file
						File.Delete(newPath);

						StreamWriter myWriter = File.CreateText(newPath);
						int linenumber = 1;
						try
						{
							myWriter.WriteLine("c Strassennetzdaten Modell NEMO");
							myWriter.WriteLine("c Version 5.0.0");
							myWriter.WriteLine("c");
							myWriter.WriteLine("c Bezugsjahr");
							//get the year for NEMO simulations
							string[] text1 = new string[1000];
							
							foreach (LineSourceData _lines in ItemData)
							{
								if (_lines.Nemo.TrafficSit != 0)
								{
									myWriter.WriteLine(_lines.Nemo.BaseYear.ToString());
									break;
								}
							}
							myWriter.WriteLine("c Strassenname,Abschnittsnummer,L [km],Stg [%],Strassentyp [int],JDTV,vmittel_PKW [km/h],vmittel_LNF [km/h],vmittel_Solo-LKW [km/h],vmittel_LSZ [km/h],vmittel_RB [km/h],vmittel_SB [km/h],vmittel_MR-2T [km/h],vmittel_MR-4T [km/h],vmittel_KKR [km/h],vmittel_Free [km/h],Ant. LNF,Ant. SNF,davon Ant. Solo LKW,davon Ant. LSZ,davon Ant. Rb,davon Ant. Libus,Ant. 2 Räder,davon Ant. MR,davon Ant. KKR,JDTP_PKW,JDTP_LNF,JDTP_Solo-LKW,JDTP_LSZ,JDTP_RB,JDTP_SB,JDTP_MR,JDTP_KKR,JDTP_Free,QGr,xvon,yvon,zvon,xbis,ybis,zbis,StrBreit,LSW,pStop PKW [%] ,aBrake PKW [m/s^2],pStop LNF [%] ,aBrake LNF [m/s^2],pStop Solo-LKW [%] ,aBrake Solo-LKW [m/s^2],pStop LSZ [%] ,aBrake LSZ [m/s^2],pStop RB [%] ,aBrake RB [m/s^2],pStop SB [%] ,aBrake SB [m/s^2],pStop MR-2T [%] ,aBrake MR-2T [m/s^2],pStop  MR-4T [%] ,aBrake  MR-4T [m/s^2],pStop KKR [%] ,aBrake KKR [m/s^2],pStop Free [%] ,aBrake Free [m/s^2]");

							//read velocities for each vehicle-type according to the driving pattern and the slope
							Assembly _assembly;
							_assembly = Assembly.GetExecutingAssembly();
							StreamReader myReader = new StreamReader(_assembly.GetManifestResourceStream("Gral.Resources.NEMO_velocities.txt"));
							string[] line = new string[400];
							line[0] = myReader.ReadLine();
							string[] velo = new string[13];
							string[] velo_prev = new string[13];
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
							myReader.Close();

							//read kinematic parameters for each vehicle-type according to the driving pattern and the slope
							Assembly _assembly_kin;
							_assembly_kin = Assembly.GetExecutingAssembly();
							myReader = new StreamReader(_assembly_kin.GetManifestResourceStream("Gral.Resources.abreak.txt"));
							string[] kinematic = new string[400];
							kinematic[0] = myReader.ReadLine();
							string[] kine_prev = new string[12];
							string[] kine = new string[12];
							for (int k = 1; k < 550; k++)
							{
								try
								{
									kinematic[k] = myReader.ReadLine();
								}
								catch
								{
									break;
								}
							}
							myReader.Close();
							
							//read more kinematic parameters for each vehicle-type according to the driving pattern and the slope
							Assembly _assembly_kin_KKR;
							_assembly_kin_KKR = Assembly.GetExecutingAssembly();
							myReader = new StreamReader(_assembly_kin_KKR.GetManifestResourceStream("Gral.Resources.abreak_KKR.txt"));
							string[] kinematic_KKR = new string[50];
							kinematic_KKR[0] = myReader.ReadLine();
							string[] kine_KKR = new string[17];
							string[] kine_KKR_prev = new string[17];
							for (int k = 1; k < 50; k++)
							{
								try
								{
									kinematic_KKR[k] = myReader.ReadLine();
								}
								catch
								{
									break;
								}
							}
							myReader.Close();

							Assembly _assembly_kin_LNF;
							_assembly_kin_LNF = Assembly.GetExecutingAssembly();
							myReader = new StreamReader(_assembly_kin_LNF.GetManifestResourceStream("Gral.Resources.abreak_LNF.txt"));
							string[] kinematic_LNF = new string[50];
							kinematic_LNF[0] = myReader.ReadLine();
							string[] kine_LNF = new string[17];
							string[] kine_LNF_prev = new string[17];
							for (int k = 1; k < 50; k++)
							{
								try
								{
									kinematic_LNF[k] = myReader.ReadLine();
								}
								catch
								{
									break;
								}
							}
							myReader.Close();

							Assembly _assembly_kin_LSZ;
							_assembly_kin_LSZ = Assembly.GetExecutingAssembly();
							myReader = new StreamReader(_assembly_kin_LSZ.GetManifestResourceStream("Gral.Resources.abreak_LSZ.txt"));
							string[] kinematic_LSZ = new string[50];
							kinematic_LSZ[0] = myReader.ReadLine();
							string[] kine_LSZ = new string[17];
							string[] kine_LSZ_prev = new string[17];
							for (int k = 1; k < 50; k++)
							{
								try
								{
									kinematic_LSZ[k] = myReader.ReadLine();
								}
								catch
								{
									break;
								}
							}
							myReader.Close();

							Assembly _assembly_kin_LKW;
							_assembly_kin_LKW = Assembly.GetExecutingAssembly();
							myReader = new StreamReader(_assembly_kin_LSZ.GetManifestResourceStream("Gral.Resources.abreak_SoloLKW.txt"));
							string[] kinematic_LKW = new string[50];
							kinematic_LKW[0] = myReader.ReadLine();
							string[] kine_LKW = new string[17];
							string[] kine_LKW_prev = new string[17];
							for (int k = 1; k < 50; k++)
							{
								try
								{
									kinematic_LKW[k] = myReader.ReadLine();
								}
								catch
								{
									break;
								}
							}
							myReader.Close();

							Assembly _assembly_kin_MR_2T;
							_assembly_kin_MR_2T = Assembly.GetExecutingAssembly();
							myReader = new StreamReader(_assembly_kin_MR_2T.GetManifestResourceStream("Gral.Resources.abreak_MR-2T.txt"));
							string[] kinematic_MR_2T = new string[50];
							kinematic_MR_2T[0] = myReader.ReadLine();
							string[] kine_MR_2T = new string[17];
							string[] kine_MR_2T_prev = new string[17];
							for (int k = 1; k < 50; k++)
							{
								try
								{
									kinematic_MR_2T[k] = myReader.ReadLine();
								}
								catch
								{
									break;
								}
							}
							myReader.Close();

							Assembly _assembly_kin_MR_4T;
							_assembly_kin_MR_4T = Assembly.GetExecutingAssembly();
							myReader = new StreamReader(_assembly_kin_MR_4T.GetManifestResourceStream("Gral.Resources.abreak_MR-4T.txt"));
							string[] kinematic_MR_4T = new string[50];
							kinematic_MR_4T[0] = myReader.ReadLine();
							string[] kine_MR_4T = new string[17];
							string[] kine_MR_4T_prev = new string[17];
							for (int k = 1; k < 50; k++)
							{
								try
								{
									kinematic_MR_4T[k] = myReader.ReadLine();
								}
								catch
								{
									break;
								}
							}
							myReader.Close();

							Assembly _assembly_kin_PKW;
							_assembly_kin_PKW = Assembly.GetExecutingAssembly();
							myReader = new StreamReader(_assembly_kin_PKW.GetManifestResourceStream("Gral.Resources.abreak_PKW.txt"));
							string[] kinematic_PKW = new string[50];
							kinematic_PKW[0] = myReader.ReadLine();
							string[] kine_PKW = new string[17];
							string[] kine_PKW_prev = new string[17];
							for (int k = 1; k < 50; k++)
							{
								try
								{
									kinematic_PKW[k] = myReader.ReadLine();
								}
								catch
								{
									break;
								}
							}
							myReader.Close();

							Assembly _assembly_kin_RB;
							_assembly_kin_RB = Assembly.GetExecutingAssembly();
							myReader = new StreamReader(_assembly_kin_RB.GetManifestResourceStream("Gral.Resources.abreak_RB.txt"));
							string[] kinematic_RB = new string[50];
							kinematic_RB[0] = myReader.ReadLine();
							string[] kine_RB = new string[17];
							string[] kine_RB_prev = new string[17];
							for (int k = 1; k < 50; k++)
							{
								try
								{
									kinematic_RB[k] = myReader.ReadLine();
								}
								catch
								{
									break;
								}
							}
							myReader.Close();

							Assembly _assembly_kin_SB;
							_assembly_kin_SB = Assembly.GetExecutingAssembly();
							myReader = new StreamReader(_assembly_kin_SB.GetManifestResourceStream("Gral.Resources.abreak_SB.txt"));
							string[] kinematic_SB = new string[50];
							kinematic_SB[0] = myReader.ReadLine();
							string[] kine_SB = new string[17];
							string[] kine_SB_prev = new string[17];
							for (int k = 1; k < 50; k++)
							{
								try
								{
									kinematic_SB[k] = myReader.ReadLine();
								}
								catch
								{
									break;
								}
							}
							myReader.Close();

							Assembly _assembly_kin_SoloLKW;
							_assembly_kin_SoloLKW = Assembly.GetExecutingAssembly();
							myReader = new StreamReader(_assembly_kin_SoloLKW.GetManifestResourceStream("Gral.Resources.abreak_SoloLKW.txt"));
							string[] kinematic_SoloLKW = new string[50];
							kinematic_SoloLKW[0] = myReader.ReadLine();
							string[] kine_SoloLKW = new string[17];
							string[] kine_SoloLKW_prev = new string[17];
							for (int k = 1; k < 50; k++)
							{
								try
								{
									kinematic_SoloLKW[k] = myReader.ReadLine();
								}
								catch
								{
									break;
								}
							}
							myReader.Close();

							foreach (LineSourceData _lines in ItemData)
                            {
                                //treat only those line sources with a defined traffic situation
                                if (_lines.Nemo.TrafficSit != 0)
                                {
                                    double v1 = 0;
									double v3 = 0;
									double v5 = 0;
									double v6 = 0;
									double v7 = 0;
									double v9 = 0;

									//get velocities for each vehicle-type according to the driving pattern and the slope
									for (int n = 0; n < 400; n++)
									{
										try
										{
											velo = line[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if ((velo[0] == Main.NemoTrafficSituations[_lines.Nemo.TrafficSit]) && (Convert.ToDouble(velo[2].Replace(".", decsep)) >= _lines.Nemo.Slope))
											{
												velo_prev = line[n-1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												v1 = Convert.ToDouble(velo_prev[4].Replace(".", decsep)) + (Convert.ToDouble(velo[4].Replace(".", decsep)) - Convert.ToDouble(velo_prev[4].Replace(".", decsep))) / (Convert.ToDouble(velo[2].Replace(".", decsep)) - Convert.ToDouble(velo_prev[2].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(velo_prev[2].Replace(".", decsep)));
												v3 = Convert.ToDouble(velo_prev[6].Replace(".", decsep)) + (Convert.ToDouble(velo[6].Replace(".", decsep)) - Convert.ToDouble(velo_prev[6].Replace(".", decsep))) / (Convert.ToDouble(velo[2].Replace(".", decsep)) - Convert.ToDouble(velo_prev[2].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(velo_prev[2].Replace(".", decsep)));
												v5 = Convert.ToDouble(velo_prev[8].Replace(".", decsep)) + (Convert.ToDouble(velo[8].Replace(".", decsep)) - Convert.ToDouble(velo_prev[8].Replace(".", decsep))) / (Convert.ToDouble(velo[2].Replace(".", decsep)) - Convert.ToDouble(velo_prev[2].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(velo_prev[2].Replace(".", decsep)));
												v6 = Convert.ToDouble(velo_prev[9].Replace(".", decsep)) + (Convert.ToDouble(velo[9].Replace(".", decsep)) - Convert.ToDouble(velo_prev[9].Replace(".", decsep))) / (Convert.ToDouble(velo[2].Replace(".", decsep)) - Convert.ToDouble(velo_prev[2].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(velo_prev[2].Replace(".", decsep)));
												v7 = Convert.ToDouble(velo_prev[10].Replace(".", decsep)) + (Convert.ToDouble(velo[10].Replace(".", decsep)) - Convert.ToDouble(velo_prev[10].Replace(".", decsep))) / (Convert.ToDouble(velo[2].Replace(".", decsep)) - Convert.ToDouble(velo_prev[2].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(velo_prev[2].Replace(".", decsep)));
												v9 = Convert.ToDouble(velo_prev[12].Replace(".", decsep)) + (Convert.ToDouble(velo[12].Replace(".", decsep)) - Convert.ToDouble(velo_prev[12].Replace(".", decsep))) / (Convert.ToDouble(velo[2].Replace(".", decsep)) - Convert.ToDouble(velo_prev[2].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(velo_prev[2].Replace(".", decsep)));
												break;
											}
										}
										catch
										{
										}
									}
									//compute length of street
									double length = St_F.CalcLenght(_lines.Pt);

                                    //compute HDV-share
                                    double hdv_share = Math.Round(_lines.Nemo.ShareHDV / 100, 2);

                                    //compute kinematic parameters for each vehicle type in dependence on vehicle speed and slope
                                    double slope = _lines.Nemo.Slope;
									int str_typ = Convert.ToInt32(velo[3]);
									double v_int = 0;
									double a_PKW = 0;
									double a_PKW1 = 0;
									double a_PKW2 = 0;
									double a_LNF = 0;
									double a_LNF1 = 0;
									double a_LNF2 = 0;
									double a_LKW = 0;
									double a_LKW1 = 0;
									double a_LKW2 = 0;
									double a_LSZ = 0;
									double a_LSZ1 = 0;
									double a_LSZ2 = 0;
									double a_RB = 0;
									double a_RB1 = 0;
									double a_RB2 = 0;
									double a_SB = 0;
									double a_SB1 = 0;
									double a_SB2 = 0;
									double a_MR_2T = 0;
									double a_MR_2T1 = 0;
									double a_MR_2T2 = 0;
									double a_MR_4T = 0;
									double a_MR_4T1 = 0;
									double a_MR_4T2 = 0;
									double a_KKR = 0;
									double a_KKR1 = 0;
									double a_KKR2 = 0;

									double stop_PKW = 0;
									double stop_LNF = 0;
									double stop_LKW = 0;
									double stop_LSZ = 0;
									double stop_RB = 0;
									double stop_SB = 0;
									double stop_MR_2T = 0;
									double stop_MR_4T = 0;
									double stop_KKR = 0;


									//get kinematic parameters for PKW and LNF according to vehicle speed and the slope (from abreak.txt)
									for (int n = 1; n < 550; n++)
									{
										try
										{
											kine = kinematic[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if ((v1 < Convert.ToDouble(kine[0])) && (_lines.Nemo.Slope < Convert.ToDouble(kine[1].Replace(".", decsep))))
											{
												//interpolate values
												kine_prev = kinematic[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												v_int = Convert.ToDouble(kine_prev[0].Replace(".", decsep));
												a_PKW1 = Convert.ToDouble(kine_prev[2].Replace(".", decsep)) + (Convert.ToDouble(kine[2].Replace(".", decsep)) - Convert.ToDouble(kine_prev[2].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												kine = kinematic[n - 13].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												kine_prev = kinematic[n - 14].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												a_PKW2 = Convert.ToDouble(kine_prev[2].Replace(".", decsep)) + (Convert.ToDouble(kine[2].Replace(".", decsep)) - Convert.ToDouble(kine_prev[2].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												a_PKW = a_PKW2 + (a_PKW1 - a_PKW2) / (v_int - Convert.ToDouble(kine[0])) * (v1 - Convert.ToDouble(kine[0]));

												kine = kinematic[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												kine_prev = kinematic[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												v_int = Convert.ToDouble(kine_prev[0].Replace(".", decsep));
												a_LNF1 = Convert.ToDouble(kine_prev[3].Replace(".", decsep)) + (Convert.ToDouble(kine[3].Replace(".", decsep)) - Convert.ToDouble(kine_prev[3].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												kine = kinematic[n - 13].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												kine_prev = kinematic[n - 14].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												a_LNF2 = Convert.ToDouble(kine_prev[3].Replace(".", decsep)) + (Convert.ToDouble(kine[3].Replace(".", decsep)) - Convert.ToDouble(kine_prev[3].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												a_LNF = a_LNF2 + (a_LNF1 - a_LNF2) / (v_int - Convert.ToDouble(kine[0])) * (v1 - Convert.ToDouble(kine[0]));
												
												break;
											}
										}
										catch
										{
										}
									}

									//get kinematic parameters for LKW and LSZ according to vehicle speed and the slope (from abreak.txt)
									for (int n = 1; n < 550; n++)
									{
										try
										{
											kine = kinematic[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if ((v3 < Convert.ToDouble(kine[0])) && (_lines.Nemo.Slope < Convert.ToDouble(kine[1].Replace(".", decsep))))
											{
												//interpolate values
												kine_prev = kinematic[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												v_int = Convert.ToDouble(kine_prev[0].Replace(".", decsep));
												a_LKW1 = Convert.ToDouble(kine_prev[4].Replace(".", decsep)) + (Convert.ToDouble(kine[4].Replace(".", decsep)) - Convert.ToDouble(kine_prev[4].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												kine = kinematic[n - 13].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												kine_prev = kinematic[n - 14].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												a_LKW2 = Convert.ToDouble(kine_prev[4].Replace(".", decsep)) + (Convert.ToDouble(kine[4].Replace(".", decsep)) - Convert.ToDouble(kine_prev[4].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												a_LKW = a_LKW2 + (a_LKW1 - a_LKW2) / (v_int - Convert.ToDouble(kine[0])) * (v3 - Convert.ToDouble(kine[0]));

												kine = kinematic[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												kine_prev = kinematic[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												v_int = Convert.ToDouble(kine_prev[0].Replace(".", decsep));
												a_LSZ1 = Convert.ToDouble(kine_prev[5].Replace(".", decsep)) + (Convert.ToDouble(kine[5].Replace(".", decsep)) - Convert.ToDouble(kine_prev[5].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												kine = kinematic[n - 13].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												kine_prev = kinematic[n - 14].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												a_LSZ2 = Convert.ToDouble(kine_prev[5].Replace(".", decsep)) + (Convert.ToDouble(kine[5].Replace(".", decsep)) - Convert.ToDouble(kine_prev[5].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												a_LSZ = a_LSZ2 + (a_LSZ1 - a_LSZ2) / (v_int - Convert.ToDouble(kine[0])) * (v3 - Convert.ToDouble(kine[0]));

												break;
											}
										}
										catch
										{
										}
									}

									//get kinematic parameters for RB according to vehicle speed and the slope (from abreak.txt)
									for (int n = 1; n < 550; n++)
									{
										try
										{
											kine = kinematic[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if ((v5 < Convert.ToDouble(kine[0])) && (_lines.Nemo.Slope < Convert.ToDouble(kine[1].Replace(".", decsep))))
											{
												//interpolate values
												kine_prev = kinematic[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												v_int = Convert.ToDouble(kine_prev[0].Replace(".", decsep));
												a_RB1 = Convert.ToDouble(kine_prev[6].Replace(".", decsep)) + (Convert.ToDouble(kine[6].Replace(".", decsep)) - Convert.ToDouble(kine_prev[6].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												kine = kinematic[n - 13].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												kine_prev = kinematic[n - 14].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												a_RB2 = Convert.ToDouble(kine_prev[6].Replace(".", decsep)) + (Convert.ToDouble(kine[6].Replace(".", decsep)) - Convert.ToDouble(kine_prev[6].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												a_RB = a_RB2 + (a_RB1 - a_RB2) / (v_int - Convert.ToDouble(kine[0])) * (v5 - Convert.ToDouble(kine[0]));

												break;
											}
										}
										catch
										{
										}
									}

									//get kinematic parameters for SB according to vehicle speed and the slope (from abreak.txt)
									for (int n = 1; n < 550; n++)
									{
										try
										{
											kine = kinematic[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if ((v6 < Convert.ToDouble(kine[0])) && (_lines.Nemo.Slope < Convert.ToDouble(kine[1].Replace(".", decsep))))
											{
												//interpolate values
												kine_prev = kinematic[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												v_int = Convert.ToDouble(kine_prev[0].Replace(".", decsep));
												a_SB1 = Convert.ToDouble(kine_prev[7].Replace(".", decsep)) + (Convert.ToDouble(kine[7].Replace(".", decsep)) - Convert.ToDouble(kine_prev[7].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												kine = kinematic[n - 13].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												kine_prev = kinematic[n - 14].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												a_SB2 = Convert.ToDouble(kine_prev[7].Replace(".", decsep)) + (Convert.ToDouble(kine[7].Replace(".", decsep)) - Convert.ToDouble(kine_prev[7].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												a_SB = a_SB2 + (a_SB1 - a_SB2) / (v_int - Convert.ToDouble(kine[0])) * (v6 - Convert.ToDouble(kine[0]));

												break;
											}
										}
										catch
										{
										}
									}

									//get kinematic parameters for MR-2T and MR-4T according to vehicle speed and the slope (from abreak.txt)
									for (int n = 1; n < 550; n++)
									{
										try
										{
											kine = kinematic[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if ((v7 < Convert.ToDouble(kine[0])) && (_lines.Nemo.Slope < Convert.ToDouble(kine[1].Replace(".", decsep))))
											{
												//interpolate values
												kine_prev = kinematic[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												v_int = Convert.ToDouble(kine_prev[0].Replace(".", decsep));
												a_MR_2T1 = Convert.ToDouble(kine_prev[8].Replace(".", decsep)) + (Convert.ToDouble(kine[8].Replace(".", decsep)) - Convert.ToDouble(kine_prev[8].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												kine = kinematic[n - 13].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												kine_prev = kinematic[n - 14].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												a_MR_2T2 = Convert.ToDouble(kine_prev[8].Replace(".", decsep)) + (Convert.ToDouble(kine[8].Replace(".", decsep)) - Convert.ToDouble(kine_prev[8].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												a_MR_2T = a_MR_2T2 + (a_MR_2T1 - a_MR_2T2) / (v_int - Convert.ToDouble(kine[0])) * (v7 - Convert.ToDouble(kine[0]));


												kine = kinematic[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												kine_prev = kinematic[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												v_int = Convert.ToDouble(kine_prev[0].Replace(".", decsep));
												a_MR_4T1 = Convert.ToDouble(kine_prev[9].Replace(".", decsep)) + (Convert.ToDouble(kine[9].Replace(".", decsep)) - Convert.ToDouble(kine_prev[9].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												kine = kinematic[n - 13].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												kine_prev = kinematic[n - 14].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												a_MR_4T2 = Convert.ToDouble(kine_prev[9].Replace(".", decsep)) + (Convert.ToDouble(kine[9].Replace(".", decsep)) - Convert.ToDouble(kine_prev[9].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												a_MR_4T = a_MR_4T2 + (a_MR_4T1 - a_MR_4T2) / (v_int - Convert.ToDouble(kine[0])) * (v7 - Convert.ToDouble(kine[0]));

												break;
											}
										}
										catch
										{
										}
									}

									//get kinematic parameters for KKR according to vehicle speed and the slope (from abreak.txt)
									for (int n = 1; n < 550; n++)
									{
										try
										{
											kine = kinematic[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if ((v9 < Convert.ToDouble(kine[0])) && (_lines.Nemo.Slope < Convert.ToDouble(kine[1].Replace(".", decsep))))
											{
												//interpolate values
												kine_prev = kinematic[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												v_int = Convert.ToDouble(kine_prev[0].Replace(".", decsep));
												a_KKR1 = Convert.ToDouble(kine_prev[10].Replace(".", decsep)) + (Convert.ToDouble(kine[10].Replace(".", decsep)) - Convert.ToDouble(kine_prev[10].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												kine = kinematic[n - 13].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												kine_prev = kinematic[n - 14].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												a_KKR2 = Convert.ToDouble(kine_prev[10].Replace(".", decsep)) + (Convert.ToDouble(kine[10].Replace(".", decsep)) - Convert.ToDouble(kine_prev[10].Replace(".", decsep))) / (Convert.ToDouble(kine[1].Replace(".", decsep)) - Convert.ToDouble(kine_prev[1].Replace(".", decsep))) * (_lines.Nemo.Slope - Convert.ToDouble(kine_prev[1].Replace(".", decsep)));
												a_KKR = a_KKR2 + (a_KKR1 - a_KKR2) / (v_int - Convert.ToDouble(kine[0])) * (v9 - Convert.ToDouble(kine[0]));

												break;
											}
										}
										catch
										{
										}
									}

									//select correct colum in the files abreak_***.txt according to street type
									int styp_col = 13 + str_typ;

									//get additional kinematic parameters for each vehicle-type according to vehicle speed and the slope (from abreak_***.txt)
									for (int n = 1; n < 50; n++)
									{
										try
										{
											kine_PKW = kinematic_PKW[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if (v1 < Convert.ToDouble(kine_PKW[0]))
											{
												//interpolate values
												kine_PKW_prev = kinematic_PKW[n-1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												stop_PKW = Convert.ToDouble(kine_PKW_prev[styp_col].Replace(".", decsep)) + (Convert.ToDouble(kine_PKW[styp_col].Replace(".", decsep)) - Convert.ToDouble(kine_PKW_prev[styp_col].Replace(".", decsep))) / (Convert.ToDouble(kine_PKW[0].Replace(".", decsep)) - Convert.ToDouble(kine_PKW_prev[0].Replace(".", decsep))) * (v1 - Convert.ToDouble(kine_PKW_prev[0].Replace(".", decsep)));
												break;
											}
										}
										catch
										{
										}
									}

									for (int n = 1; n < 50; n++)
									{
										try
										{
											kine_LNF = kinematic_LNF[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if (v1 < Convert.ToDouble(kine_LNF[0]))
											{
												//interpolate values
												kine_LNF_prev = kinematic_LNF[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												stop_LNF = Convert.ToDouble(kine_LNF_prev[styp_col].Replace(".", decsep)) + (Convert.ToDouble(kine_LNF[styp_col].Replace(".", decsep)) - Convert.ToDouble(kine_LNF_prev[styp_col].Replace(".", decsep))) / (Convert.ToDouble(kine_LNF[0].Replace(".", decsep)) - Convert.ToDouble(kine_LNF_prev[0].Replace(".", decsep))) * (v1 - Convert.ToDouble(kine_LNF_prev[0].Replace(".", decsep)));
												break;
											}
										}
										catch
										{
										}
									}

									for (int n = 1; n < 50; n++)
									{
										try
										{
											kine_LKW = kinematic_LKW[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if (v3 < Convert.ToDouble(kine_LKW[0]))
											{
												//interpolate values
												kine_LKW_prev = kinematic_LKW[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												stop_LKW = Convert.ToDouble(kine_LKW_prev[styp_col].Replace(".", decsep)) + (Convert.ToDouble(kine_LKW[styp_col].Replace(".", decsep)) - Convert.ToDouble(kine_LKW_prev[styp_col].Replace(".", decsep))) / (Convert.ToDouble(kine_LKW[0].Replace(".", decsep)) - Convert.ToDouble(kine_LKW_prev[0].Replace(".", decsep))) * (v3 - Convert.ToDouble(kine_LKW_prev[0].Replace(".", decsep)));
												break;
											}
										}
										catch
										{
										}
									}

									for (int n = 1; n < 50; n++)
									{
										try
										{
											kine_LSZ = kinematic_LSZ[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if (v3 < Convert.ToDouble(kine_LSZ[0]))
											{
												//interpolate values
												kine_LSZ_prev = kinematic_LSZ[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												stop_LSZ = Convert.ToDouble(kine_LSZ_prev[styp_col].Replace(".", decsep)) + (Convert.ToDouble(kine_LSZ[styp_col].Replace(".", decsep)) - Convert.ToDouble(kine_LSZ_prev[styp_col].Replace(".", decsep))) / (Convert.ToDouble(kine_LSZ[0].Replace(".", decsep)) - Convert.ToDouble(kine_LSZ_prev[0].Replace(".", decsep))) * (v3 - Convert.ToDouble(kine_LSZ_prev[0].Replace(".", decsep)));
												break;
											}
										}
										catch
										{
										}
									}

									for (int n = 1; n < 50; n++)
									{
										try
										{
											kine_RB = kinematic_RB[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if (v5 < Convert.ToDouble(kine_RB[0]))
											{
												//interpolate values
												kine_RB_prev = kinematic_RB[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												stop_RB = Convert.ToDouble(kine_RB_prev[styp_col].Replace(".", decsep)) + (Convert.ToDouble(kine_RB[styp_col].Replace(".", decsep)) - Convert.ToDouble(kine_RB_prev[styp_col].Replace(".", decsep))) / (Convert.ToDouble(kine_RB[0].Replace(".", decsep)) - Convert.ToDouble(kine_RB_prev[0].Replace(".", decsep))) * (v5 - Convert.ToDouble(kine_RB_prev[0].Replace(".", decsep)));
												break;
											}
										}
										catch
										{
										}
									}

									for (int n = 1; n < 50; n++)
									{
										try
										{
											kine_SB = kinematic_SB[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if (v6 < Convert.ToDouble(kine_SB[0]))
											{
												//interpolate values
												kine_SB_prev = kinematic_SB[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												stop_SB = Convert.ToDouble(kine_SB_prev[styp_col].Replace(".", decsep)) + (Convert.ToDouble(kine_SB[styp_col].Replace(".", decsep)) - Convert.ToDouble(kine_SB_prev[styp_col].Replace(".", decsep))) / (Convert.ToDouble(kine_SB[0].Replace(".", decsep)) - Convert.ToDouble(kine_SB_prev[0].Replace(".", decsep))) * (v6 - Convert.ToDouble(kine_SB_prev[0].Replace(".", decsep)));
												break;
											}
										}
										catch
										{
										}
									}

									for (int n = 1; n < 50; n++)
									{
										try
										{
											kine_MR_2T = kinematic_MR_2T[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if (v7 < Convert.ToDouble(kine_MR_2T[0]))
											{
												//interpolate values
												kine_MR_2T_prev = kinematic_MR_2T[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												stop_MR_2T = Convert.ToDouble(kine_MR_2T_prev[styp_col].Replace(".", decsep)) + (Convert.ToDouble(kine_MR_2T[styp_col].Replace(".", decsep)) - Convert.ToDouble(kine_MR_2T_prev[styp_col].Replace(".", decsep))) / (Convert.ToDouble(kine_MR_2T[0].Replace(".", decsep)) - Convert.ToDouble(kine_MR_2T_prev[0].Replace(".", decsep))) * (v7 - Convert.ToDouble(kine_MR_2T_prev[0].Replace(".", decsep)));
												break;
											}
										}
										catch
										{
										}
									}

									for (int n = 1; n < 50; n++)
									{
										try
										{
											kine_MR_4T = kinematic_MR_4T[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if (v7 < Convert.ToDouble(kine_MR_4T[0]))
											{
												//interpolate values
												kine_MR_4T_prev = kinematic_MR_4T[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												stop_MR_4T = Convert.ToDouble(kine_MR_4T_prev[styp_col].Replace(".", decsep)) + (Convert.ToDouble(kine_MR_4T[styp_col].Replace(".", decsep)) - Convert.ToDouble(kine_MR_4T_prev[styp_col].Replace(".", decsep))) / (Convert.ToDouble(kine_MR_4T[0].Replace(".", decsep)) - Convert.ToDouble(kine_MR_4T_prev[0].Replace(".", decsep))) * (v7 - Convert.ToDouble(kine_MR_4T_prev[0].Replace(".", decsep)));
												break;
											}
										}
										catch
										{
										}
									}

									for (int n = 1; n < 50; n++)
									{
										try
										{
											kine_KKR = kinematic_KKR[n].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
											if (v9 < Convert.ToDouble(kine_KKR[0]))
											{
												//interpolate values
												kine_KKR_prev = kinematic_KKR[n - 1].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
												stop_KKR = Convert.ToDouble(kine_KKR_prev[styp_col].Replace(".", decsep)) + (Convert.ToDouble(kine_KKR[styp_col].Replace(".", decsep)) - Convert.ToDouble(kine_KKR_prev[styp_col].Replace(".", decsep))) / (Convert.ToDouble(kine_KKR[0].Replace(".", decsep)) - Convert.ToDouble(kine_KKR_prev[0].Replace(".", decsep))) * (v9 - Convert.ToDouble(kine_KKR_prev[0].Replace(".", decsep)));
												break;
											}
										}
										catch
										{
										}
									}

									//consider source group definitions
									if (seperation == false)
									{
										//write line sources to NEMO input file
										myWriter.WriteLine(velo[0] + "," + _lines.Section + "," + Convert.ToString(length, ic) + "," +
                                                           Convert.ToString(_lines.Nemo.Slope) + "," + velo[3] + "," + Convert.ToString(_lines.Nemo.AvDailyTraffic) + "," + Convert.ToString(v1, ic) + "," + Convert.ToString(v1, ic) + "," + Convert.ToString(v3, ic) + "," + Convert.ToString(v3, ic) + "," + Convert.ToString(v5, ic) + "," + Convert.ToString(v6, ic) + "," + Convert.ToString(v7, ic) + "," + Convert.ToString(v7, ic) + "," + Convert.ToString(v9, ic) + ",0," +
										                   "-1," + Convert.ToString(hdv_share, ic) + "," + "-1,-1,-1,-1,-1,-1,-1,0,0,0,0,0,0,0,0,0," + Convert.ToString(_lines.Poll[0].Pollutant[0]) + "," +
										                   "0,0," + Convert.ToString(_lines.Height) + "," + "0,1000," + Convert.ToString(_lines.Height) + "," + Convert.ToString(_lines.Width) + ",0," +
										                   Convert.ToString(stop_PKW, ic) + "," + Convert.ToString(a_PKW, ic) + "," + Convert.ToString(stop_LNF, ic) + "," + Convert.ToString(a_LNF, ic) + "," +
										                   Convert.ToString(stop_LKW, ic) + "," + Convert.ToString(a_LKW, ic) + "," + Convert.ToString(stop_LSZ, ic) + "," + Convert.ToString(a_LSZ, ic) + "," +
										                   Convert.ToString(stop_RB, ic) + "," + Convert.ToString(a_RB, ic) + "," + Convert.ToString(stop_SB, ic) + "," + Convert.ToString(a_SB, ic) + "," +
										                   Convert.ToString(stop_MR_2T, ic) + "," + Convert.ToString(a_MR_2T, ic) + "," + Convert.ToString(stop_MR_4T, ic) + "," + Convert.ToString(a_MR_4T, ic) + "," +
										                   Convert.ToString(stop_KKR, ic) + "," + Convert.ToString(a_KKR, ic) + "," + Convert.ToString(0, ic) + "," + Convert.ToString(0, ic));
									}
									else
									{
										double hdv = 0;
										double aadt = 0;
										for (int k = 0; k < 2; k++)
										{
											//share of HDV for the two source groups PC and HDV
											if (k == 0)
												hdv = 0;
											if (k == 1)
												hdv = 1;
											//anual average daily traffic
											if (k == 0)
												aadt = _lines.Nemo.AvDailyTraffic - _lines.Nemo.AvDailyTraffic * hdv_share;
											if (k == 1)
												aadt = _lines.Nemo.AvDailyTraffic * hdv_share;
											//write line sources to NEMO input file
											myWriter.WriteLine(velo[0] + "," + _lines.Section + "," + Convert.ToString(length, ic) + "," +
                                                               Convert.ToString(_lines.Nemo.Slope) + "," + velo[3] + "," + Convert.ToString(aadt, ic) + "," + Convert.ToString(v1, ic) + "," + Convert.ToString(v1, ic) + "," + Convert.ToString(v3, ic) + "," + Convert.ToString(v3, ic) + "," + Convert.ToString(v5, ic) + "," + Convert.ToString(v6, ic) + "," + Convert.ToString(v7, ic) + "," + Convert.ToString(v7, ic) + "," + Convert.ToString(v9, ic) + ",0," +
											                   "-1," + Convert.ToString(hdv, ic) + "," + "-1,-1,-1,-1,-1,-1,-1,0,0,0,0,0,0,0,0,0," + Convert.ToString(sgroups[k]) + "," +
											                   "0,0," + Convert.ToString(_lines.Height) + "," + "1000,0," + Convert.ToString(_lines.Height) + "," + Convert.ToString(_lines.Width) + ",0," +
											                   Convert.ToString(stop_PKW, ic) + "," + Convert.ToString(a_PKW, ic) + "," + Convert.ToString(stop_LNF, ic) + "," + Convert.ToString(a_LNF, ic) + "," +
											                   Convert.ToString(stop_LKW, ic) + "," + Convert.ToString(a_LKW, ic) + "," + Convert.ToString(stop_LSZ, ic) + "," + Convert.ToString(a_LSZ, ic) + "," +
											                   Convert.ToString(stop_RB, ic) + "," + Convert.ToString(a_RB, ic) + "," + Convert.ToString(stop_SB, ic) + "," + Convert.ToString(a_SB, ic) + "," +
											                   Convert.ToString(stop_MR_2T, ic) + "," + Convert.ToString(a_MR_2T, ic) + "," + Convert.ToString(stop_MR_4T, ic) + "," + Convert.ToString(a_MR_4T, ic) + "," +
											                   Convert.ToString(stop_KKR, ic) + "," + Convert.ToString(a_KKR, ic) + "," + Convert.ToString(0, ic) + "," + Convert.ToString(0, ic));
										}
									}
								}
								linenumber=linenumber+1;
							}
							//myWriter.WriteLine("e");
						}
						catch
						{
							MessageBox.Show("Error when writing file " + newPath + " in line " + Convert.ToString(linenumber + 1),"GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
						myWriter.Close();

						message.listBox1.Items.Add("Starting NEMO...");
						message.listBox1.Items.Add("Results can be viewed at the end");
						message.listBox1.Items.Add("of the computations in the editor mode (DOMAIN)");
						message.Refresh();

                        //start fortran routine NEMOxxx.exe to compute road emissions
                        Process nemo = new Process
                        {
                            EnableRaisingEvents = true
                        };
                        nemo.Exited += new System.EventHandler(NemoExited);
						//generate batch file
						string batch = Path.Combine(Main.ProjectName, @"Emissions","NEMO.bat");
						mywriter = new StreamWriter(batch);
						mywriter.WriteLine(dialog.FileName + " " + filePaths[0] + " " + newdir + Path.DirectorySeparatorChar);
						mywriter.Close();
						nemo.StartInfo.FileName = batch;
						nemo.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

						nemo.Start();
						
						ItemData.Clear();
						ItemData.TrimExcess();

						message.Close();
					}
					catch
					{ }
				}
			}

			//Transfer NEMO results to the file Lsources.txt
			private void NemoExited(object sender, EventArgs e)
			{
				// Load Line Source Data
				List<LineSourceData> ItemData = new List<LineSourceData>(); //collection of all line source data
				LineSourceDataIO _ls = new LineSourceDataIO();
				string _file = Path.Combine(Main.ProjectName,"Emissions","Lsources.txt");
				_ls.LoadLineSources(ItemData, _file);
				_ls = null;
				

				//read NEMO results
				string line;
				string[] text2 = new string[1000];
				double PM10 = 0;
				double PM2_5 = 0;
				
				string newpath = Path.Combine(Main.ProjectName, "Ergebnisse", "NEMO.nem");
				
				try
				{
					using (StreamReader myreader=new StreamReader(newpath))
						
					{
						//five header lines
						line = myreader.ReadLine();
						line = myreader.ReadLine();
						line = myreader.ReadLine();
						line = myreader.ReadLine();
						line = myreader.ReadLine();
						//road emissions
						foreach (LineSourceData _lines in ItemData)
						{
							//all line sources with a defined traffic situation are updated
							if (_lines.Nemo.TrafficSit != 0)
							{
								//delete all existing entries first to avoid mulitple emission calculations after mulitple NEMO runs
								_lines.Poll.Clear();

								line = myreader.ReadLine();
								text2 = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
								if (sep == true)
								{
									//source group seperation
									if ((sgroups[0] == sgroups[2]) && (sgroups[1] == sgroups[3]))
									{
										//Passenger cars exhaust
										PM10 = Convert.ToDouble(text2[19].Replace(".", decsep)) + Convert.ToDouble(text2[24].Replace(".", decsep));
										PM2_5 = Convert.ToDouble(text2[20].Replace(".", decsep)) + Convert.ToDouble(text2[25].Replace(".", decsep));
                                        PollutantsData _poll = new PollutantsData
                                        {
                                            SourceGroup = sgroups[0]
                                        };										

                                        _poll.Pollutant[0] = 0; _poll.EmissionRate[0] = St_F.TxtToDbl(text2[14], false);
										_poll.Pollutant[1] = 1; _poll.EmissionRate[1] = PM10;
										_poll.Pollutant[2] = 4; _poll.EmissionRate[2] = PM2_5;
										_poll.Pollutant[3] = 3; _poll.EmissionRate[3] = St_F.TxtToDbl(text2[35], false);
										_poll.Pollutant[4] = 5; _poll.EmissionRate[4] = St_F.TxtToDbl(text2[37], false);
										_poll.Pollutant[5] = 6; _poll.EmissionRate[5] = St_F.TxtToDbl(text2[31], false);
										_poll.Pollutant[6] = 7; _poll.EmissionRate[6] = St_F.TxtToDbl(text2[39], false);
										_poll.Pollutant[7] = 8; _poll.EmissionRate[7] = St_F.TxtToDbl(text2[16], false);
										_poll.Pollutant[8] = 13; _poll.EmissionRate[8] = St_F.TxtToDbl(text2[17], false);
										
										_lines.Poll.Add(_poll);
										
										line = myreader.ReadLine();
										text2 = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
										
										//HDV cars exhaust
										PM10 = Convert.ToDouble(text2[19].Replace(".", decsep)) + Convert.ToDouble(text2[24].Replace(".", decsep));
										PM2_5 = Convert.ToDouble(text2[20].Replace(".", decsep)) + Convert.ToDouble(text2[25].Replace(".", decsep));
                                        PollutantsData _pollHDV = new PollutantsData
                                        {
                                            SourceGroup = sgroups[1]
                                        };
                                        _pollHDV.Pollutant[0] = 0; _pollHDV.EmissionRate[0] = St_F.TxtToDbl(text2[14], false);
										_pollHDV.Pollutant[1] = 1; _pollHDV.EmissionRate[1] = PM10;
										_pollHDV.Pollutant[2] = 4; _pollHDV.EmissionRate[2] = PM2_5;
										_pollHDV.Pollutant[3] = 3; _pollHDV.EmissionRate[3] = St_F.TxtToDbl(text2[35], false);
										_pollHDV.Pollutant[4] = 5; _pollHDV.EmissionRate[4] = St_F.TxtToDbl(text2[37], false);
										_pollHDV.Pollutant[5] = 6; _pollHDV.EmissionRate[5] = St_F.TxtToDbl(text2[31], false);
										_pollHDV.Pollutant[6] = 7; _pollHDV.EmissionRate[6] = St_F.TxtToDbl(text2[39], false);
										_pollHDV.Pollutant[7] = 8; _pollHDV.EmissionRate[7] = St_F.TxtToDbl(text2[16], false);
										_pollHDV.Pollutant[8] = 13; _pollHDV.EmissionRate[8] = St_F.TxtToDbl(text2[17], false);
										_lines.Poll.Add(_pollHDV);
									}
									else
									{
										//Passenger cars exhaust
										PM10 = Convert.ToDouble(text2[19].Replace(".", decsep));
										PM2_5 = Convert.ToDouble(text2[20].Replace(".", decsep));
                                        PollutantsData _poll = new PollutantsData
                                        {
                                            SourceGroup = sgroups[0]
                                        };
                                        _poll.Pollutant[0] = 0; _poll.EmissionRate[0] = St_F.TxtToDbl(text2[14], false);
										_poll.Pollutant[1] = 1; _poll.EmissionRate[1] = PM10;
										_poll.Pollutant[2] = 4; _poll.EmissionRate[2] = PM2_5;
										_poll.Pollutant[3] = 3; _poll.EmissionRate[3] = St_F.TxtToDbl(text2[35], false);
										_poll.Pollutant[4] = 5; _poll.EmissionRate[4] = St_F.TxtToDbl(text2[37], false);
										_poll.Pollutant[5] = 6; _poll.EmissionRate[5] = St_F.TxtToDbl(text2[31], false);
										_poll.Pollutant[6] = 7; _poll.EmissionRate[6] = St_F.TxtToDbl(text2[39], false);
										_poll.Pollutant[7] = 8; _poll.EmissionRate[7] = St_F.TxtToDbl(text2[16], false);
										_poll.Pollutant[8] = 13; _poll.EmissionRate[8] = St_F.TxtToDbl(text2[17], false);
										_lines.Poll.Add(_poll);
										
										//Passenger cars-non-exhaust
										PM10 = Convert.ToDouble(text2[24].Replace(".", decsep));
										PM2_5 = Convert.ToDouble(text2[25].Replace(".", decsep));
                                        PollutantsData _pollNonEx = new PollutantsData
                                        {
                                            SourceGroup = sgroups[2]
                                        };
                                        _pollNonEx.Pollutant[0] = 0; _pollNonEx.EmissionRate[0] = 0;
										_pollNonEx.Pollutant[1] = 1; _pollNonEx.EmissionRate[1] = PM10;
										_pollNonEx.Pollutant[2] = 4; _pollNonEx.EmissionRate[2] = PM2_5;
										_pollNonEx.Pollutant[3] = 3;
										_pollNonEx.Pollutant[4] = 5;
										_pollNonEx.Pollutant[5] = 6;
										_pollNonEx.Pollutant[6] = 7;
										_pollNonEx.Pollutant[7] = 8;
										_pollNonEx.Pollutant[8] = 13;
										_lines.Poll.Add(_pollNonEx);
										
										line = myreader.ReadLine();
										text2 = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
										//HDV cars exhaust
										PM10 = Convert.ToDouble(text2[19].Replace(".", decsep));
										PM2_5 = Convert.ToDouble(text2[20].Replace(".", decsep));
                                        PollutantsData _pollHDV = new PollutantsData
                                        {
                                            SourceGroup = sgroups[1]
                                        };
                                        _pollHDV.Pollutant[0] = 0; _pollHDV.EmissionRate[0] = St_F.TxtToDbl(text2[14], false);
										_pollHDV.Pollutant[1] = 1; _pollHDV.EmissionRate[1] = PM10;
										_pollHDV.Pollutant[2] = 4; _pollHDV.EmissionRate[2] = PM2_5;
										_pollHDV.Pollutant[3] = 3; _pollHDV.EmissionRate[3] = St_F.TxtToDbl(text2[35], false);
										_pollHDV.Pollutant[4] = 5; _pollHDV.EmissionRate[4] = St_F.TxtToDbl(text2[37], false);
										_pollHDV.Pollutant[5] = 6; _pollHDV.EmissionRate[5] = St_F.TxtToDbl(text2[31], false);
										_pollHDV.Pollutant[6] = 7; _pollHDV.EmissionRate[6] = St_F.TxtToDbl(text2[39], false);
										_pollHDV.Pollutant[7] = 8; _pollHDV.EmissionRate[7] = St_F.TxtToDbl(text2[16], false);
										_pollHDV.Pollutant[8] = 13; _pollHDV.EmissionRate[8] = St_F.TxtToDbl(text2[17], false);
										_lines.Poll.Add(_pollHDV);
										
										//HDV cars-non-exhaust
										PM10 = Convert.ToDouble(text2[24].Replace(".", decsep));
										PM2_5 = Convert.ToDouble(text2[25].Replace(".", decsep));
                                        PollutantsData _pollHDVNonEx = new PollutantsData
                                        {
                                            SourceGroup = sgroups[3]
                                        };
                                        _pollHDVNonEx.Pollutant[0] = 0; _pollHDVNonEx.EmissionRate[0] = 0;
										_pollHDVNonEx.Pollutant[1] = 1; _pollHDVNonEx.EmissionRate[1] = PM10;
										_pollHDVNonEx.Pollutant[2] = 4; _pollHDVNonEx.EmissionRate[2] = PM2_5;
										_pollHDVNonEx.Pollutant[3] = 3;
										_pollHDVNonEx.Pollutant[4] = 5;
										_pollHDVNonEx.Pollutant[5] = 6;
										_pollHDVNonEx.Pollutant[6] = 7;
										_pollHDVNonEx.Pollutant[7] = 8;
										_pollHDVNonEx.Pollutant[8] = 13;
										_lines.Poll.Add(_pollHDVNonEx);
									}
								}
								else
								{
									PM10 = Convert.ToDouble(text2[19].Replace(".", decsep)) + Convert.ToDouble(text2[24].Replace(".", decsep));
									PM2_5 = Convert.ToDouble(text2[20].Replace(".", decsep)) + Convert.ToDouble(text2[25].Replace(".", decsep));
									PollutantsData _poll = _lines.Poll[0];
									_poll.Pollutant[0] = 0; _poll.EmissionRate[0] = St_F.TxtToDbl(text2[14], false);
									_poll.Pollutant[1] = 1; _poll.EmissionRate[1] = PM10;
									_poll.Pollutant[2] = 4; _poll.EmissionRate[2] = PM2_5;
									_poll.Pollutant[3] = 3; _poll.EmissionRate[3] = St_F.TxtToDbl(text2[35], false);
									_poll.Pollutant[4] = 5; _poll.EmissionRate[4] = St_F.TxtToDbl(text2[37], false);
									_poll.Pollutant[5] = 6; _poll.EmissionRate[5] = St_F.TxtToDbl(text2[31], false);
									_poll.Pollutant[6] = 7; _poll.EmissionRate[6] = St_F.TxtToDbl(text2[39], false);
									_poll.Pollutant[7] = 8; _poll.EmissionRate[7] = St_F.TxtToDbl(text2[16], false);
									_poll.Pollutant[8] = 13; _poll.EmissionRate[8] = St_F.TxtToDbl(text2[17], false);
								}
							}
						}
					}
				}
				catch
				{}
				
				// Save modified Line Source Data
				LineSourceDataIO _savels = new LineSourceDataIO();
				_savels.SaveLineSources(ItemData, Main.ProjectName);
				_savels = null;
				
				File.Copy(newpath, Path.Combine(Main.ProjectName, @"Emissions","NEMO.nem"), true);
				Directory.Delete(Path.Combine(Main.ProjectName, @"Ergebnisse"),true);
				MessageBox.Show("NEMO computations finished","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
				
				ItemData.Clear();
				ItemData.TrimExcess();
			}
		}
	}
}
