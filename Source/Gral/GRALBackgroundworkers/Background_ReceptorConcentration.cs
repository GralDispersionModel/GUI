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
 * User: Markus_2
 * Date: 29.05.2015
 * Time: 18:11
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using GralIO;
using System;
using System.Collections.Generic;
using System.IO;

namespace GralBackgroundworkers
{
	public partial class ProgressFormBackgroundworker
	{
    	private string decsep;
    	private List<double> xrec = new List<double>();
    	
		/// <summary>
        /// Calculate receptor concentrations and GRAL flow field receptor wind fields 
        /// </summary>
		private void ReceptorConcentration(GralBackgroundworkers.BackgroundworkerData mydata,
                                           System.ComponentModel.DoWorkEventArgs e)
        {
			//reading emission variations
			int maxsource = 100; //mydata.MaxSource; allow all source-group numbers!
			int maxcomputedsourcegroup = mydata.MaxSourceComputed;
			decsep = mydata.Decsep;
			
			double[,] emifac_day = new double[24, maxsource];
			double[,] emifac_mon = new double[12, maxsource];
			string[] text = new string[15];
			for (int k=0; k <15; k++) text[k] = "";
			
			string dummy;
			string newpath="";
			int[]  sg_numbers = new int[maxsource];
			string[] sg_names = mydata.Sel_Source_Grp.Split(',');
			string[] computed_sourcegroups = mydata.Comp_Source_Grp.Split(',');
			
			//in transient GRAL mode, it is simply to read the File GRAL_meteozeitreihe.dat and convert it to .met files
			bool transient = false;
			InDatVariables data = new InDatVariables();
			InDatFileIO ReadInData = new InDatFileIO();
			data.InDatPath = Path.Combine(mydata.Projectname, "Computation","in.dat");
			ReadInData.Data = data;
			if (ReadInData.ReadInDat() == true)
			{
				if (data.Transientflag == 0)
				{
					transient = true;
				}
			}	
			
			//get variation for source group
			if (mydata.Sel_Source_Grp != string.Empty) // otherwise just analyze the wind data
			{
				// Read the emission factors of all selected source-groups
				for (int itm = 0; itm < maxsource; itm++)
				{
					try
					{
						for (int sg = 0; sg <sg_names.Length; sg++) // check all selected source groups
						{
							if ((itm + 1) == Convert.ToInt32(GetSgNumbers(sg_names[sg]))) // sourcegroup selected?
							{
								sg_numbers[itm] = Convert.ToInt32(GetSgNumbers(sg_names[sg])); // Get number of the Source-group
								// MessageBox.Show(itm.ToString()+"/"+sg_numbers[itm]);
								// Read modulation of that source-group
								newpath = Path.Combine("Computation", "emissions" + Convert.ToString(itm + 1).PadLeft(3,'0') + ".dat");
								using (StreamReader myreader = new StreamReader(Path.Combine(mydata.Projectname, newpath)))
								{
									for (int j = 0; j < 24; j++)
									{
										text = myreader.ReadLine().Split(new char[] { ',' });
										emifac_day[j, itm] = Convert.ToDouble(text[1].Replace(".", decsep));
										if (j < 12)
											emifac_mon[j, itm] = Convert.ToDouble(text[2].Replace(".", decsep));
									}
								}
								sg = sg_names.Length + 1; // break
							}
							else // source group not selected
							{
								for (int j = 0; j < 24; j++)
								{
									emifac_day[j, itm] = 0;
									if (j < 12)
										emifac_mon[j, itm] = 0;
								}
								sg_numbers[itm] = -1;
							}
						}
					}
					catch(Exception ex)
					{
						BackgroundThreadMessageBox (ex.Message);
					}
				}
			}
			
			//read mettimeseries.dat to get file length necessary to define some array lengths
			newpath = Path.Combine("Computation", "mettimeseries.dat");
			int mettimefilelength = 0;
			string[] text2 = new string[5];
			using (StreamReader sr = new StreamReader(Path.Combine(mydata.Projectname, newpath)))
			{
				//text2 = sr.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				while (sr.EndOfStream == false)
				{
					mettimefilelength++;
					sr.ReadLine();
				}
			}
			if (mettimefilelength == 0) // File-lenght must > 0
			{
				BackgroundThreadMessageBox ("Error reading mettimeseries.txt");
				return; // leave method
			}
			//mettimefilelength--;
			
			//if file emissions_timeseries.txt exists, these modulation factors will be used
			int[] sg_time = new int[maxsource];
			double[,] emifac_timeseries = new double[mettimefilelength + 1, maxsource];

			if (mydata.Sel_Source_Grp != string.Empty) // otherwise just analyze the wind data
			{
				//it is necessary to set all values of the array emifac_timeseries equal to 1
				for (int i = 0; i < mettimefilelength + 1; i++)
				{
					for (int n = 0; n < maxsource; n++)
					{
						emifac_timeseries[i, n] = 1;
					}
				}

				// read value from emissions_timeseries.txt -> emifac_day[] and emifac_mon[] not used
				newpath = Path.Combine(mydata.Projectname, "Computation","emissions_timeseries.txt");
				if (File.Exists(newpath) == true)
				{
					try
					{
						//read timeseries of emissions
						string[] text10 = new string[1];
						using (StreamReader read1 = new StreamReader(newpath))
						{
							//get source group numbers
							text10 = read1.ReadLine().Split(new char[] { ' ', ':', '-', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
							for (int i = 2; i < text10.Length; i++)
							{
								//get the column corresponding with the source group number stored in sg_numbers
								string sg_temp = text10[i];
								for (int itm1 = 0; itm1 < maxsource; itm1++)
								{
									if (sg_numbers[itm1] == Convert.ToInt32(sg_temp))
									{
										sg_time[itm1] = i;

										//set emifac_day and emifac_mon equal one -> only for those source groups that are defined in emissions_timeseries.txt
										for (int j = 0; j < 24; j++)
										{
											emifac_day[j, itm1] = 1;
											if (j < 12)
												emifac_mon[j, itm1] = 1;
										}
									}
								}

							}

							for (int i = 0; i < mettimefilelength; i++)
							{
								text10 = read1.ReadLine().Split(new char[] { ' ', ':', '-', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
								for (int n = 0; n < maxsource; n++)
								{
									if (sg_time[n] == 0)
									{
										emifac_timeseries[i, n] = 1;
									}
									else
									{
										emifac_timeseries[i, n] = Convert.ToDouble(text10[sg_time[n]].Replace(".", decsep));
									}
								}
							}
						}
						
					}
					catch(Exception ex)
					{
						BackgroundThreadMessageBox (ex.Message + " Error reading emissions_timeseries.txt - evaluation stopped");
						return;
					}
				} // read value from emissions_timeseries.txt
			}
			
			List<string> wgmet = new List<string>();
			List<string> wrmet = new List<string>();
			List<string> akmet = new List<string>();
			string[] text3 = new string[2];
			int hourplus = 0;
			string wgmettime;
			string wrmettime;
			string akmettime;
			string month;
			string hour;
			string day;
			
			List<string> rec_names=new List<string>();
			
			//get number of receptor points and names of receptors
			string receptorfile=Path.Combine(mydata.Projectname, "Computation","Receptor.dat");
			if(File.Exists(receptorfile))
			{
				try
				{
					using(StreamReader read=new StreamReader(receptorfile))
					{
						dummy=read.ReadLine(); // 1st line with number of receptor points
						
						while (read.EndOfStream == false)
						{
							text = read.ReadLine().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
							
							if (text.Length > 1) // valid line?
							{
								xrec.Add(Convert.ToDouble(text[1].Replace(".", decsep)));
								
								string a = text[0]; 	// take number as name
								if (text.Length > 4) 	// get name from line
									a = text[4];   
								
								if (text.Length >= 5) // if the Receptor-Name contains ","
								{
									for (int k = 5; k < text.Length; k++) // make it possible, that "," is in the Receptor point
									{
										if (text[k] != "") a = a + "_" + text[k];
									}
								}
								
								// if the receptor name contains invalid characters for a file!
								a =string.Join("_", a.Split(Path.GetInvalidFileNameChars()));
								rec_names.Add(a);
							}
						}
					}
				}
				catch
				{
					BackgroundThreadMessageBox ("Error reading Receptor.dat");
					return;
				}
				
			}
			
			//MessageBox.Show(Convert.ToString(rec_names.Count));
			
			//read meteopgt.all
			newpath = Path.Combine("Computation","meteopgt.all");
			using (StreamReader myReader = new StreamReader(Path.Combine(mydata.Projectname, newpath)))
			{
				text = myReader.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				text = myReader.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				while (myReader.EndOfStream == false)
				{
					try
					{
						text = myReader.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
						wrmet.Add(text[0]);
						wgmet.Add(text[1]);
						akmet.Add(text[2]);
					}
					catch
					{
						BackgroundThreadMessageBox ("Error reading meteopgt.all");
						return;
					}
				}
			}
			
			double[][][] conc = GralIO.Landuse.CreateArray<double[][]>(xrec.Count, () 
			                                            => GralIO.Landuse.CreateArray<double[]>(maxsource, () 
			                                                                     => new double[wrmet.Count]));
			double fmod = 1;

			//read all computed concentrations from file zeitreihe.dat
			bool zeitflag = false;
			string[] text5 = new string[xrec.Count * sg_names.Length];
			int numbwet = 0;
			receptorfile=Path.Combine(mydata.Projectname, "Computation","zeitreihe.dat");
			if (File.Exists(receptorfile) && mydata.Sel_Source_Grp != string.Empty)
			{
				zeitflag = true;
				
				try
				{
                    using (StreamReader read = new StreamReader(receptorfile))
                    {
                        while (read.EndOfStream == false)
                        {
                            text5 = read.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            int count = 0;
                            // read all conc data of all computed source-groups
                            for (int numbsource = 0; numbsource < maxcomputedsourcegroup; numbsource++)
                            {
                                int sg_number = Convert.ToInt32(computed_sourcegroups[numbsource]) - 1; // Source-Group Number of computed sourcegroups

                                for (int numbrec = 0; numbrec < xrec.Count; numbrec++)
                                {
                                    conc[numbrec][sg_number][numbwet] = Convert.ToDouble(text5[count].Replace(".", decsep));
                                    count++;
                                }
                            }
                            numbwet++;
                        }
                    }
                }
                catch
				{
					BackgroundThreadMessageBox ("Error reading zeitreihe.dat. Is this file available?" + Environment.NewLine +
												"Has the number of receptors or source groups changed?");
					return;
				}
				
				
			}
			
			//read all wind speeds from file GRAL_Meteozeitreihe.dat
			bool metflag = false;
			bool local_SCL = false;
			string[] text7 = new string[xrec.Count];
			
			string GRAL_metfile = Path.Combine(mydata.Projectname, "Computation","GRAL_Meteozeitreihe.dat");
			int zeitreihe_lenght = (int) GralStaticFunctions.St_F.CountLinesInFile(GRAL_metfile);
			
			double[,] GRAL_u = new double[xrec.Count, Math.Max(zeitreihe_lenght, wrmet.Count) + 1];
			double[,] GRAL_v = new double[xrec.Count, Math.Max(zeitreihe_lenght, wrmet.Count) + 1];
			int[,] GRAL_SC = new int[xrec.Count, Math.Max(zeitreihe_lenght, wrmet.Count) + 1];
			
			
			if (File.Exists(GRAL_metfile))
			{
				metflag = true;
				numbwet = 0;
				
				if (Read_Meteo_Zeitreihe(GRAL_metfile, ref numbwet, ref local_SCL, ref GRAL_u, ref GRAL_v, ref GRAL_SC) == false)
				{
					return;
				}
			}

			if (mydata.Sel_Source_Grp != string.Empty) // otherwise just analyze the wind data
			{
				if (zeitflag == true) // write mettime series for all receptor points
				{
					try
					{
						string file = Path.Combine(mydata.Projectname, "Computation","Receptor_timeseries_"+ mydata.Prefix + mydata.Pollutant + ".txt");
						if (File.Exists(file))
						{
							try
							{
								File.Delete(file);
							}
							catch{}
						}

						//write results to file receptor_timeseries.txt
						using (StreamWriter recwrite = new StreamWriter(file))
						{
							//write header line
							string[] text6 = new string[2];
							//write source groups
							recwrite.Write("Date \t Time \t"); // Header Date, Time
							
							foreach (string hy in sg_names)
							{
								for (int numbrec = 0; numbrec < xrec.Count; numbrec++)
								{
									text6 = hy.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
									recwrite.Write(text6[0]+"-Rec:"+Convert.ToString(numbrec+1) + "\t"); // Header source-groups use Tabulator
								}
							}
							recwrite.WriteLine();
							
							//read mettimeseries.dat
							newpath = Path.Combine("Computation", "mettimeseries.dat");
							StreamReader read = new StreamReader(Path.Combine(mydata.Projectname, newpath));
							text2 = read.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
							text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);

							//consider, if meteopgt.all represents a time series or a statistics
							int dispersionsituations = 0;
							if (mydata.Checkbox19 == true)
								dispersionsituations = numbwet + 1;
							else
								dispersionsituations = wrmet.Count;
							
							int count_dispsit_in_mettime = 0;
							int count_ws = -1;

							while (text2[0] != "")
							{
								count_ws++;
								count_dispsit_in_mettime += 1;
								
								if ((count_dispsit_in_mettime > numbwet) && (mydata.Checkbox19 == true))
									break;

								month = text3[1];
								day = text3[0];
								hour = text2[1];
								if (hour == "24")
									hourplus = 1;
								wgmettime = text2[2];
								wrmettime = text2[3];
								akmettime = text2[4];

								//search in file meteopgt.all for the corresponding dispersion situation

								for (int n = 0; n < dispersionsituations; n++)
								{
									if ((wgmet[n] == wgmettime) && (wrmet[n] == wrmettime) && (akmet[n] == akmettime))
									{
										//take care if not all dispersion situations have been computed
										if (n >= numbwet)
										{
											//write results
											dummy = "";
											recwrite.WriteLine(dummy);
										}
										else
										{
											SetText("Day.Month: " + day + "." + month);
											
											int std = Convert.ToInt32(hour);
											int mon = Convert.ToInt32(month) - 1;

											//write results
											dummy = "";
											dummy = day + "." + month + "\t" + hour+":00\t";
											fmod = 1;
											foreach (string hy in sg_names)
											{
												{
													int itm = Convert.ToInt32(GetSgNumbers(hy)) - 1;
													
													//compute emission modulation factor
													fmod = emifac_day[std - hourplus, itm] * emifac_mon[mon, itm] * emifac_timeseries[count_ws, itm];
													
													for (int numbrec = 0; numbrec < xrec.Count; numbrec++)
													{
													    dummy = dummy + Convert.ToString(conc[numbrec][itm][n] * fmod) + "\t";
													}
												}
												
											}
											recwrite.WriteLine(dummy);
											//consider, if meteopgt.all represents a time series or a statistics
											if (mydata.Checkbox19 == true)
												break;
										}
									}
								}
								try
								{
									text2 = read.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
									text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
								}
								catch
								{
									break;
								}
							}
							
						} // using
					}
					catch
					{
						BackgroundThreadMessageBox ("Error writing the file receptor_timeseries.txt");
						return;
					}
				}
				else
				{
					BackgroundThreadMessageBox ("File zeitreihe.dat not found");
				}
			}
			
			if (metflag == true) // write meteorological data for all receptor points
			{
				try
				{
					for (int k = 0; k < rec_names.Count; k++)
					{
						//write results to file(s) GRAL_*metstations*.met
						double windspeed_GRAL = 0;
						double winddirection_GRAL = 0;

						string file = Path.Combine(mydata.Projectname, @"Metfiles","GRAL" + Convert.ToString(k +1) + "_"+rec_names[k]+".met");
						if (File.Exists(file))
						{
							try
							{
								File.Delete(file);
							}
							catch{}
						}

						using (StreamWriter recwrite = new StreamWriter(file))
						{
							//write header lines
							recwrite.WriteLine("//" + Path.GetFileName(file));
							string[] text6 = new string[2];

							//read mettimeseries.dat
							newpath = Path.Combine("Computation", "mettimeseries.dat");
							
							using (StreamReader read = new StreamReader(Path.Combine(mydata.Projectname, newpath)))
							{
								text2 = read.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
								text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
								string dummy_year = "1990";
								int year_increase=0;
								int monthold = -1;

								//consider, if meteopgt.all represents a time series or a statistics
								int dispersionsituations = 0;
								if (mydata.Checkbox19 == true || transient == true)
									dispersionsituations = numbwet + 1;
								else
									dispersionsituations = wrmet.Count;
								
								int count_dispsit_in_mettime = 0;

								while (text2[0] != "")
								{
									count_dispsit_in_mettime++;
									if ((count_dispsit_in_mettime > numbwet) && (mydata.Checkbox19 == true || transient == true))
										break;

									month = text3[1];
									day = text3[0];
									hour = text2[1];
									if (hour == "24")
										hourplus = 1;
									wgmettime = text2[2];
									wrmettime = text2[3];
									akmettime = text2[4];

									//artificially increase year by one
									if (Convert.ToInt32(month) < monthold)
										year_increase += 1;
									monthold = Convert.ToInt32(text3[1]);

									int corr_situation = 0;
									
									if (transient == false) // non - transient mode: search for corr. situation in meteopgt.all
									{
										//search in file meteopgt.all for the corresponding dispersion situation
										for (int n = 0; n < dispersionsituations; n++)
										{
											if ((wgmet[n] == wgmettime) && (wrmet[n] == wrmettime) && (akmet[n] == akmettime))
											{
												//take care if not all dispersion situations have been computed
												if (n >= numbwet)
												{
													//write results
													//dummy = "";
													//recwrite.WriteLine(dummy);
												}
												else
												{
													corr_situation = n;
													break;
												}
											}
										}
									}
									else // transient mode: situation = line number of mettimeseries 
									{
										corr_situation = count_dispsit_in_mettime;
									}
									
									if (corr_situation > 0)
									{
										int n  = corr_situation;
										SetText("Day.Month: " + day + "." + month);
										
										int std = Convert.ToInt32(hour);
										int mon = Convert.ToInt32(month) - 1;

										//write results
										dummy = "";
										dummy_year = Convert.ToString(1990 + year_increase);
										dummy = day + "." + month + "." +dummy_year + "," + hour + ":00,";
										//compute wind speed and direction
										windspeed_GRAL = Math.Round(Math.Pow(Math.Pow(GRAL_u[k,n], 2) + Math.Pow(GRAL_v[k,n], 2), 0.5),2);
										if (GRAL_v[k,n] == 0)
											winddirection_GRAL = 90;
										else
											winddirection_GRAL = Convert.ToInt32(Math.Abs(Math.Atan(GRAL_u[k, n] / GRAL_v[k, n])) * 180 / 3.14);
										if ((GRAL_v[k, n] > 0) && (GRAL_u[k, n] <= 0))
											winddirection_GRAL = 180 - winddirection_GRAL;
										if ((GRAL_v[k, n] >= 0) && (GRAL_u[k, n] > 0))
											winddirection_GRAL = 180 + winddirection_GRAL;
										if ((GRAL_v[k, n] < 0) && (GRAL_u[k, n] >= 0))
											winddirection_GRAL = 360 - winddirection_GRAL;

										dummy = dummy + Convert.ToString(windspeed_GRAL, ic) + "," + Convert.ToString(Math.Round(winddirection_GRAL, 0));

										if (local_SCL == true) // 11.9.2017 Kuntner -> new File format?
										{
											if (GRAL_SC[k, n] > 0) // if this situation is not available yet
											{
												dummy = dummy + "," + Convert.ToString(GRAL_SC[k, n]);
											}
											else
											{
												dummy = dummy + "," + Convert.ToString(akmettime);
											}
										}
										else
										{
											dummy = dummy + "," + Convert.ToString(akmettime);
										}
										
										recwrite.WriteLine(dummy);
										//consider, if meteopgt.all represents a time series or a statistics
//										if (mydata.Checkbox19 == true || transient == true)
//											break;
									}
									try
									{
										text2 = read.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
										text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
									}
									catch
									{
										break;
									}
								}
							} // using read
						} // using recwrite
					}
				}
				catch
				{
					BackgroundThreadMessageBox ("Error writing GRAL-Metfiles");
					return;
				}
				
			}
			else
			{
				//MessageBox.Show("File GRAL_Meteozeitreihe.dat not found", "Receptor Concentrations", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		
		private bool Read_Meteo_Zeitreihe(string GRAL_metfile, ref int numbwet, ref bool local_SCL, ref double[,] GRAL_u, ref double[,] GRAL_v, ref int[,] GRAL_SC)
		{
			string[] text7 = new string[1];
			numbwet = 1;
			
			try // 11.9.2017 Kuntner -> new File format?
			{
				using(StreamReader read1 = new StreamReader(GRAL_metfile))
				{
					if (read1.ReadLine().Trim() == "U,V,SC")
					{
						local_SCL = true;
					}
				}
			}
			catch{}
				
			using (StreamReader read = new StreamReader(GRAL_metfile))
			{
				try
				{
					if (local_SCL) // Read Header
					{
						read.ReadLine();
					}
					
					while (read.EndOfStream == false)
					{
						text7 = read.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
						int count = 0;
						for (int numbrec = 0; numbrec < xrec.Count; numbrec++)
						{
							GRAL_u[numbrec, numbwet] = Convert.ToDouble(text7[count].Replace(".", decsep));
							GRAL_v[numbrec, numbwet] = Convert.ToDouble(text7[count + 1].Replace(".", decsep));
							if (local_SCL) // 11.9.2017 Kuntner -> new File format
							{
								GRAL_SC[numbrec, numbwet] = Convert.ToInt32(text7[count + 2].Replace(".", decsep));
								count += 3;
							}
							else
							{
								count += 2;
							}
						}
						numbwet++;
					}
				}
				catch
				{
					BackgroundThreadMessageBox ("Error reading GRAL_Meteozeitreihe.dat");
					return false;
				}
			}
			
			Computation_Completed = true; // set flag, that computation was successful
			return true;
		} // Read_GRAL_Meteozeitreihe
		
	}
}