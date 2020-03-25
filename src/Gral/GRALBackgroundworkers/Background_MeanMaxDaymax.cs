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
using System;
using System.IO;
using System.Collections.Generic;
using GralIO;

namespace GralBackgroundworkers
{
    public partial class ProgressFormBackgroundworker
	{
		/// <summary>
        /// Calculat Mean and/or Max and DayMax values 
        /// </summary>
		private void MeanMaxDaymax(GralBackgroundworkers.BackgroundworkerData mydata,
                                   System.ComponentModel.DoWorkEventArgs e)
        {
			//reading emission variations
			int maxsource = mydata.MaxSource;
			string decsep = mydata.Decsep;
			double[,] emifac_day = new double[24, maxsource];
			double[,] emifac_mon = new double[12, maxsource];
			string[] text = new string[5];
			string newpath;
			string[] sg_numbers = new string[maxsource];
			string[] sg_names = mydata.Sel_Source_Grp.Split(',');
			bool deposition_files_exists = false;
            bool no_classification = mydata.Checkbox19;
			
			//get variation for source group
			int itm=0;
			try
			{
				foreach (string source_group_name in sg_names)
				{
					sg_numbers[itm] = GetSgNumbers(source_group_name);
					newpath = Path.Combine("Computation", "emissions" + sg_numbers[itm].PadLeft(3,'0') + ".dat");

                    using (StreamReader myreader = new StreamReader(Path.Combine(mydata.Projectname, newpath)))
                    {
                        for (int j = 0; j < 24; j++)
                        {
                            text = myreader.ReadLine().Split(new char[] { ',' });
                            emifac_day[j, itm] = Convert.ToDouble(text[1].Replace(".", decsep));
                            if (j < 12)
                            {
                                emifac_mon[j, itm] = Convert.ToDouble(text[2].Replace(".", decsep));
                            }
                        }
                    }
					itm++;
				}
			}
			catch(Exception ex)
			{
				BackgroundThreadMessageBox (ex.Message);
				return;
			}
			
			List<string> wgmet = new List<string>();
			List<string> wrmet = new List<string>();
			List<string> akmet = new List<string>();
			string[] text2 = new string[5];
			string[] text3 = new string[2];
			int hourplus = 0;
			string wgmettime;
			string wrmettime;
			string akmettime;
			string month;
			string day;
			string monthold="a";
			string dayold="a";
			string hour;
			int numhour = 1;
			int nnn = 0;
			//int indexi = 0;
			//int indexj = 0;
			float[, ,] conc = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource];
			float[, ,] dep = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource];
			float[, ,] concmit = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource + 1];
			float[, ,] depmit = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource + 1];
			float[, ,] concmax = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource + 1];
			float[, ,] depmax = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource + 1];
			float[,] concmaxdummy = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
			float[,] depmaxdummy = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
			float[, ,] concdaymax = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource + 1];
			float[, ,] depdaymax = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource + 1];
			float[, ,] concdaymaxdummy = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource + 1];
			float[, ,] depdaymaxdummy = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource + 1];
			double[] fmod = new double[maxsource];

			//read meteopgt.all
			List<string> data_meteopgt = new List<string>();
			ReadMeteopgtAll(Path.Combine(mydata.Projectname, "Computation", "meteopgt.all"), ref data_meteopgt);
			if (data_meteopgt.Count == 0) // no data available
			{ 
				BackgroundThreadMessageBox ("Error reading meteopgt.all");
			}

			foreach(string line_meteopgt in data_meteopgt)
			{
				try
				{
					text = line_meteopgt.Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					wrmet.Add(text[0]);
					wgmet.Add(text[1]);
					akmet.Add(text[2]);
				}
				catch
				{}
			}
			
			//read mettimeseries.dat to get file length necessary to define some array lengths
			newpath = Path.Combine("Computation", "mettimeseries.dat");
			int mettimefilelength = 0;
			//read mettimeseries.dat to get file length necessary to define some array lengths
			try
			{
				newpath = Path.Combine("Computation", "mettimeseries.dat");
				using (StreamReader sr = new StreamReader(Path.Combine(mydata.Projectname, newpath)))
				{
					//text2 = sr.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					while (sr.EndOfStream == false)
					{
						mettimefilelength = mettimefilelength + 1;
						sr.ReadLine();
					}
				}
				if (mettimefilelength == 0) // File-lenght must > 0
				{
					BackgroundThreadMessageBox ("Can´t read mettimeseries.dat");
					return; // leave method
				}
			}
			catch(Exception ex)
			{
				BackgroundThreadMessageBox (ex.Message);
				return;
			}
			//mettimefilelength--;
			
			//if file emissions_timeseries.txt exists, these modulation factors will be used
			int[] sg_time = new int[maxsource];
			double[,] emifac_timeseries = new double[mettimefilelength + 1, maxsource];
			
			//it is necessary to set all values of the array emifac_timeseries equal to 1
			for (int i = 0; i < mettimefilelength + 1; i++)
			{
				for (int n = 0; n < maxsource; n++)
				{
					emifac_timeseries[i, n] = 1;
				}
			}

			newpath = Path.Combine(mydata.Projectname, "Computation", "emissions_timeseries.txt");
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
								if (sg_numbers[itm1] == sg_temp)
								{
									sg_time[itm1] = i;

									//set emifac_day and emifac_mon equal one -> only for those source groups that are defined in emissions_timeseries.txt
									for (int j = 0; j < 24; j++)
									{
										emifac_day[j, itm1] = 1;
										if (j < 12)
                                        {
                                            emifac_mon[j, itm1] = 1;
                                        }
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
					BackgroundThreadMessageBox (ex.Message + " Can´t read emissions_timeseries.txt - evaluation stopped");
					return;
				}
			}

            //in transient GRAL mode, it is necessary to set all modulation factors equal to one as they have been considered already in the GRAL simulations
            int transient = 1;
            try
            {
                InDatVariables data = new InDatVariables();
                InDatFileIO ReadInData = new InDatFileIO();
                data.InDatPath = Path.Combine(mydata.Projectname, "Computation","in.dat");
                ReadInData.Data = data;
                if (ReadInData.ReadInDat() == true)
                {
                    if (data.Transientflag == 0)
                    {
                        transient = 0;
                        for (int n = 0; n < maxsource; n++)
                        {
                            for (int j = 0; j < 24; j++)
                            {
                                emifac_day[j, n] = 1;
                                if (j < 12)
                                {
                                    emifac_mon[j, n] = 1;
                                }
                            }
                        }
                        for (int i = 0; i < mettimefilelength; i++)
                        {
                            for (int n = 0; n < maxsource; n++)
                            {
                                emifac_timeseries[i, n] = 1;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BackgroundThreadMessageBox(ex.Message);
                return;
            }

			//read mettimeseries.dat
			List<string> data_mettimeseries = new List<string>();
			ReadMettimeseries(Path.Combine(mydata.Projectname, "Computation", "mettimeseries.dat"), ref data_mettimeseries);
			if (data_mettimeseries.Count == 0) // no data available
			{ 
				BackgroundThreadMessageBox ("Error reading mettimeseries.dat");
			}

			int count_ws = -1;
			//while (text2[0] != "")
			foreach(string mettimeseries in data_mettimeseries)
			{
				//MessageBox.Show(mettimeseries);
				text2 = mettimeseries.Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
				
				count_ws++;
				
				if (Rechenknecht.CancellationPending)
				{
                    e.Cancel = true;
                    return;
				}
				if (count_ws % 4 == 0)
        		{
				    Rechenknecht.ReportProgress((int) (count_ws / (double) data_mettimeseries.Count * 100D));
				}
				
				month=text3[1];
				day=text3[0];
				hour=text2[1];
				if (hour == "24")
                {
                    hourplus = 1;
                }

                wgmettime =text2[2];
				wrmettime=text2[3];
				akmettime=text2[4];

                //in case of transient simulation there is no need to loop over meteopgt.all
                int meteo_loop = wrmet.Count;
                if (transient == 0 || no_classification == true)
                {
                    meteo_loop = 1;
                }

				//search in file meteopgt.all for the corresponding dispersion situation
                for (int n = 0; n < meteo_loop; n++)
				{
                    //in case of transient simulation there is no need to loop over meteopgt.all
                    bool meteo_situation_found = false;                    
                    if (transient == 0 || no_classification==true)
                    {
                        meteo_situation_found = true;
                    }
                    else
                    {
                        if ((wgmet[n] == wgmettime) && (wrmet[n] == wrmettime) && (akmet[n] == akmettime))
                        {
                            meteo_situation_found = true;
                        }
                    }

                    if (meteo_situation_found == true)
					{
						//GRAL filenames
						bool exist = true;
						string[] con_files = new string[100];
						bool existdep = true;
						string[] dep_files = new string[100];

                        //get correct weather number in dependence on steady-state or transient simulation
                        int weanumb = n;
                        if (transient == 0 || no_classification == true)
                        {
                            weanumb = count_ws;
                        }
						
						int itmp = 0;
						Object thisLock = new Object();
						foreach (string source_group_name in sg_names)
						//						Parallel.For(0, sg_names.Length, itmp =>
						{
						    bool parallel_existdep = true;
						    
							if (sg_names.Length > 0)
							{
                                con_files[itmp] = Convert.ToString(weanumb + 1).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + sg_numbers[itmp].PadLeft(2, '0') + ".con";
                                dep_files[itmp] = Convert.ToString(weanumb + 1).PadLeft(5, '0') + "-" + sg_numbers[itmp].PadLeft(2, '0') + ".dep";
							}
							else
							{
                                con_files[itmp] = Convert.ToString(weanumb + 1).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + Convert.ToString(sg_numbers[itmp]).PadLeft(2, '0') + ".con";
                                dep_files[itmp] = Convert.ToString(weanumb + 1).PadLeft(5, '0') + "-" + Convert.ToString(sg_numbers[itmp]).PadLeft(2, '0') + ".dep";
							}

							if (File.Exists(Path.Combine(mydata.Projectname, @"Computation", dep_files[itmp])) == false &&
                                File.Exists(Path.Combine(mydata.Projectname, @"Computation", Convert.ToString(weanumb + 1).PadLeft(5, '0') + ".grz")) == false)
							{
								lock(thisLock)
								{
									existdep = false;
								}
								parallel_existdep = false;
							}

							if (File.Exists(Path.Combine(mydata.Projectname, @"Computation", con_files[itmp])) == false &&
                                File.Exists(Path.Combine(mydata.Projectname, @"Computation", Convert.ToString(weanumb + 1).PadLeft(5, '0') + ".grz")) == false)
							{
								lock(thisLock)
								{
									exist = false;
								} 
								// break;
							}
							
							//set variables to zero
							for (int i = 0; i <= mydata.CellsGralX; i++)
							{
								for (int j = 0; j <= mydata.CellsGralY; j++)
								{
									conc[i, j, itmp] = 0;
									dep[i, j, itmp] = 0;
								}
							}
							
							//read GRAL concentration files
							string filename = Path.Combine(mydata.Projectname, @"Computation", con_files[itmp]);
							if (!ReadConFiles(filename, mydata, itmp, ref conc))
                            {
                                // Error reading one *.con file
                                exist = false;
                                existdep = false;
                            }
							
							if (parallel_existdep)
							{
								//read GRAL deposition files
								filename = Path.Combine(mydata.Projectname, @"Computation", dep_files[itmp]);
								bool depo_files_exists = ReadConFiles(filename, mydata, itmp, ref dep);
								if (depo_files_exists)
								{
									lock(thisLock)
									{
										deposition_files_exists = true;
									}
								}
							}
							itmp++;
						}//);
						
						thisLock = null;

                        SetText("Day.Month: " + day + "." + month);

						if ((exist == true) || (existdep == true))
						{
							
							//number of dispersion situation
							nnn = nnn + 1;
							//number of hours of specific day
							numhour = numhour + 1;
							int std = Convert.ToInt32(hour);
							int mon = Convert.ToInt32(month) - 1;
							for (int ii = 0; ii <= mydata.CellsGralX; ii++)
                            {
                                for (int j = 0; j <= mydata.CellsGralY; j++)
							{
								//reset max. daily concentrations for each source group
								concmaxdummy[ii, j] = 0;
								if (((dayold == day) && (monthold == month)) || (nnn == 1))
								{}
								else
								{
									concdaymax[ii, j, maxsource] = Math.Max(concdaymax[ii, j, maxsource], concdaymaxdummy[ii, j, maxsource] / (float)numhour);
									depdaymax[ii, j, maxsource] = Math.Max(depdaymax[ii, j, maxsource], depdaymaxdummy[ii, j, maxsource] / (float)numhour);
									itm = 0;
									foreach (string source_group_name in sg_names)
									{
										concdaymax[ii, j, itm] = Math.Max(concdaymax[ii, j, itm], concdaymaxdummy[ii, j, itm] / (float)numhour);
										depdaymax[ii, j, itm] = Math.Max(depdaymax[ii, j, itm], depdaymaxdummy[ii, j, itm] / (float)numhour);
										itm++;
									}

									concdaymaxdummy[ii, j, maxsource] = 0;
									depdaymaxdummy[ii, j, maxsource] = 0;
									itm = 0;
									foreach (string source_group_name in sg_names)
									{
										concdaymaxdummy[ii, j, itm] = 0;
										depdaymaxdummy[ii, j, itm] = 0;
										itm++;
									}

									if ((ii >= mydata.CellsGralX) && (j >= mydata.CellsGralY)) // Kuntner: changed from == to >=
                                        {
                                            numhour = 1;
                                        }
                                    }
								
								itm = 0;
								foreach (string source_group_name in sg_names)
								{
									//compute mean emission modulation factor
									if ((ii == 0) && (j == 0))
                                        {
                                            fmod[itm] = fmod[itm] + emifac_day[std - hourplus, itm] * emifac_mon[mon, itm] * emifac_timeseries[count_ws, itm];
                                        }

                                        float fac = conc[ii, j, itm] * (float)emifac_day[std - hourplus, itm] * (float)emifac_mon[mon, itm] * (float)emifac_timeseries[count_ws, itm];
									float facdep = dep[ii, j, itm] * (float)emifac_day[std - hourplus, itm] * (float)emifac_mon[mon, itm] * (float)emifac_timeseries[count_ws, itm];
									//compute mean concentrations for each source group and in total
									concmit[ii, j, itm] = concmit[ii, j, itm] + fac;
									concmit[ii, j, maxsource] = concmit[ii, j, maxsource] + fac;
									depmit[ii, j, itm] = depmit[ii, j, itm] + facdep;
									depmit[ii, j, maxsource] = depmit[ii, j, maxsource] + facdep;
									//compute max concentrations for each source group and in total
									concmax[ii, j, itm] = Math.Max(concmax[ii, j, itm], fac);
									concmaxdummy[ii, j] = concmaxdummy[ii, j] + fac;
									depmax[ii, j, itm] = Math.Max(depmax[ii, j, itm], facdep);
									depmaxdummy[ii, j] = depmaxdummy[ii, j] + facdep;
									//compute max. daily concentrations for each source group
									concdaymaxdummy[ii, j, itm] = concdaymaxdummy[ii, j, itm] + fac;
									concdaymaxdummy[ii, j, maxsource] = concdaymaxdummy[ii, j, maxsource] + fac;
									depdaymaxdummy[ii, j, itm] = depdaymaxdummy[ii, j, itm] + facdep;
									depdaymaxdummy[ii, j, maxsource] = depdaymaxdummy[ii, j, maxsource] + facdep;
									itm++;
								}
								concmax[ii, j, maxsource] = Math.Max(concmax[ii, j, maxsource], concmaxdummy[ii, j]);
								depmax[ii, j, maxsource] = Math.Max(depmax[ii, j, maxsource], depmaxdummy[ii, j]);
							}
                            }
                        }
					}
				}
				
				dayold = day;
				monthold = month;
				
			}
			
			//final computations
			if (nnn > 0)
			{
				itm = 0;
				foreach (string source_group_name in sg_names)
				{
					fmod[itm] = fmod[itm] / Convert.ToDouble(nnn);
					
					for (int i = 0; i <= mydata.CellsGralX; i++)
					{
						for (int j = 0; j <= mydata.CellsGralY; j++)
						{
							concmit[i, j, itm] = concmit[i, j, itm] / (float)nnn;
							depmit[i, j, itm] = depmit[i, j, itm] / (float)nnn;
						}
					}
					itm++;
				}
				//total concentration
				for (int i = 0; i <= mydata.CellsGralX; i++)
                {
                    for (int j = 0; j <= mydata.CellsGralY; j++)
				{
					concmit[i, j, maxsource] = concmit[i, j, maxsource] / (float)nnn;
					depmit[i, j, maxsource] = depmit[i, j, maxsource] / (float)nnn;
				}
                }
            }

			string file;
			string name;
            GralIO.WriteESRIFile Result = new GralIO.WriteESRIFile
            {
                NCols = mydata.CellsGralX,
                NRows = mydata.CellsGralY,
                YllCorner = mydata.DomainSouth,
                XllCorner = mydata.DomainWest,
                CellSize = mydata.Horgridsize
            };

            //write mean concentration hour files for each source group
            if (mydata.Checkbox2 == true)
			{
				itm = 0;
				foreach (string source_group_name in sg_names)
				{
                    if (Rechenknecht.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    if (sg_names.Length > 0)
					{
						string[] text1a = new string[2];
						text1a = Convert.ToString(sg_names[itm]).Split(new char[] { ':' });
						name = mydata.Prefix + mydata.Pollutant + "_" + text1a[0];
					}
					else
                    {
                        name = mydata.Prefix + mydata.Pollutant + "_" + sg_names[itm];
                    }

                    file = Path.Combine(mydata.Projectname, @"Maps", "Mean_" + name + "_" + mydata.Slicename + ".txt");
					
					Result.Unit = Gral.Main.My_p_m3;
					Result.Round = 5;
					Result.Z = itm;
					Result.Values = concmit;
					Result.FileName = file;
					Result.WriteFloatResult();
															
					if (deposition_files_exists)
					{
						file = Path.Combine(mydata.Projectname, @"Maps", "Deposition_Mean_" + "_" +name + ".txt");
						Result.Unit = Gral.Main.mg_p_m2;
						Result.Round = 9;
						Result.Z = itm;
						Result.Values = depmit;
						Result.FileName = file;
						Result.WriteFloatResult();
					}

					itm++;
				}

                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                //write mean total concentration
                name = mydata.Prefix + mydata.Pollutant + "_total" + "_" + mydata.Slicename;
				file = Path.Combine(mydata.Projectname, @"Maps", "Mean_" + name + ".txt");
				Result.Unit = Gral.Main.My_p_m3;
				Result.Round = 5;
				Result.Z = maxsource;
				Result.Values = concmit;
				Result.FileName = file;
				Result.WriteFloatResult();

                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                if (deposition_files_exists)
				{
					file = Path.Combine(mydata.Projectname, @"Maps", "Deposition_Mean_" + mydata.Prefix + mydata.Pollutant + "_total.txt");
					Result.Unit = Gral.Main.mg_p_m2;
					Result.Round = 9;
					Result.Z = maxsource;
					Result.Values = depmit;
					Result.FileName = file;
					Result.WriteFloatResult();
				}
			}

			//write max concentration files for each source group
			if (mydata.Checkbox1 == true)
			{
				itm = 0;
				string depo_name="";
				foreach (string source_group_name in sg_names)
				{
                    if (Rechenknecht.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    if (sg_names.Length > 0)
					{
						string[] text1a = new string[2];
						text1a = Convert.ToString(sg_names[itm]).Split(new char[] { ':' });
						name = mydata.Prefix + mydata.Pollutant + "_" + text1a[0] + "_" + mydata.Slicename;
						depo_name = mydata.Prefix + mydata.Pollutant + "_" + text1a[0];
					}
					else
					{
						name = mydata.Prefix + mydata.Pollutant + "_" + sg_names[itm] + "_" + mydata.Slicename;
						depo_name = mydata.Prefix + mydata.Pollutant + "_" + sg_names[itm];
					}
					
					file = Path.Combine(mydata.Projectname, @"Maps", "Max_" + name + ".txt");
					Result.Unit = Gral.Main.My_p_m3;
					Result.Round = 5;
					Result.Z = itm;
					Result.Values = concmax;
					Result.FileName = file;
					Result.WriteFloatResult();
										
					if (deposition_files_exists)
					{
						file = Path.Combine(mydata.Projectname, @"Maps", "Deposition_Max" + depo_name + ".txt");
						Result.Unit = Gral.Main.mg_p_m2;
						Result.Round = 9;
						Result.Z = itm;
						Result.Values = depmax;
						Result.FileName = file;
						Result.WriteFloatResult();
					}

					itm++;
				}

                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                //write max total concentration file
                name = mydata.Prefix + mydata.Pollutant + "_total" + "_" + mydata.Slicename;
				file = Path.Combine(mydata.Projectname, @"Maps", "Max_" + name + ".txt");
				Result.Unit = Gral.Main.My_p_m3;
				Result.Round = 5;
				Result.Z = maxsource;
				Result.Values = concmax;
				Result.FileName = file;
				Result.WriteFloatResult();

                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                if (deposition_files_exists)
				{
					file = Path.Combine(mydata.Projectname, @"Maps", "Deposition_Max" + mydata.Prefix + mydata.Pollutant + "_total.txt");
					Result.Unit = Gral.Main.mg_p_m2;
					Result.Round = 9;
					Result.Z = maxsource;
					Result.Values = depmax;
					Result.FileName = file;
					Result.WriteFloatResult();		
				}
			}


			//write max daily concentration files for each source group
			if (mydata.Checkbox3 == true)
			{
				itm = 0;
				string depo_name;
				foreach (string source_group_name in sg_names)
				{
                    if (Rechenknecht.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    if (sg_names.Length > 0)
					{
						string[] text1a = new string[2];
						text1a = Convert.ToString(sg_names[itm]).Split(new char[] { ':' });
						name = mydata.Prefix + mydata.Pollutant + "_" + text1a[0] + "_" + mydata.Slicename;
						depo_name = mydata.Prefix + mydata.Pollutant + "_" + text1a[0];
					}
					else
					{
						name = mydata.Prefix + mydata.Pollutant + "_" + sg_names[itm] + "_" + mydata.Slicename;
						depo_name = mydata.Prefix + mydata.Pollutant + "_" + sg_names[itm];
					}
					file = Path.Combine(mydata.Projectname, @"Maps", "DayMax_" + name + ".txt");
					Result.Unit = Gral.Main.My_p_m3;
					Result.Round = 5;
					Result.Z = itm;
					Result.Values = concdaymax;
					Result.FileName = file;
					Result.WriteFloatResult();
										
					if (deposition_files_exists)
					{
						file = Path.Combine(mydata.Projectname, @"Maps", "Deposition_DayMax_" + depo_name + ".txt");
						Result.Unit = Gral.Main.mg_p_m2;
						Result.Round = 9;
						Result.Z = itm;
						Result.Values = depdaymax;
						Result.FileName = file;
						Result.WriteFloatResult();
					}

					itm++;
				}

                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                //write max daily total concentration file
                name = mydata.Prefix + mydata.Pollutant + "_total" + "_" + mydata.Slicename;
				file = Path.Combine(mydata.Projectname, @"Maps", "Daymax_" + name + ".txt");
				Result.Unit = Gral.Main.My_p_m3;
				Result.Round = 5;
				Result.Z = maxsource;
				Result.Values = concdaymax;
				Result.FileName = file;
				Result.WriteFloatResult();

                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                if (deposition_files_exists)
				{
					file = Path.Combine(mydata.Projectname, @"Maps", "Deposition_Daymax_" + mydata.Prefix + mydata.Pollutant + "_total.txt");
					Result.Unit = Gral.Main.mg_p_m2;
					Result.Round = 9;
					Result.Z = maxsource;
					Result.Values = depdaymax;
					Result.FileName = file;
					Result.WriteFloatResult();
				}
			}
			Computation_Completed = true; // set flag, that computation was successful
		}
	}
}