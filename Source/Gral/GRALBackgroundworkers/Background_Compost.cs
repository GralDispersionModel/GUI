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
using System.Threading.Tasks;

namespace GralBackgroundworkers
{
    public partial class ProgressFormBackgroundworker
	{
		/// <summary>
        /// Calculate odour hours based on typical composting faciity
        /// </summary>
		private void OdourCompost(GralBackgroundworkers.BackgroundworkerData mydata,
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
			
			double totfreq = mydata.OdFreq[0] * mydata.OdFreq[1] * mydata.OdFreq[2];
			double totemi = mydata.Odemi[0] + mydata.Odemi[1] + mydata.Odemi[2];
			
			//get variation for source group
			int itm=0;
			try
			{
				foreach (string source_group_name in sg_names)
				{
					sg_numbers[itm] = GetSgNumbers(source_group_name);
					newpath = Path.Combine("Computation", "emissions" + sg_numbers[itm].PadLeft(3,'0') + ".dat");

					StreamReader myreader = new StreamReader(Path.Combine(mydata.Projectname, newpath));
					for (int j = 0; j < 24; j++)
					{
						text = myreader.ReadLine().Split(new char[] { ',' });
						emifac_day[j, itm] = Convert.ToDouble(text[1].Replace(".", decsep));
						if (j < 12)
							emifac_mon[j, itm] = Convert.ToDouble(text[2].Replace(".", decsep));
					}
					myreader.Close();
					itm++;
				}
			}
			catch(Exception ex)
			{
				BackgroundThreadMessageBox (ex.Message);
				return;
			}

			//read mettimeseries.dat
			List<string> wgmettime = new List<string>();
			List<string> wrmettime = new List<string>();
			List<string> akmettime = new List<string>();
			List<string> hour = new List<string>();
			List<string> month = new List<string>();
			string[] text2 = new string[5];
			string[] text3 = new string[2];
			int hourplus = 0;
			
			newpath = Path.Combine("Computation", "mettimeseries.dat");
			try
			{
                using (StreamReader read = new StreamReader(Path.Combine(mydata.Projectname, newpath)))
                {
                    text2 = read.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);

                    while (text2[0] != "")
                    {
                        month.Add(text3[1]);
                        hour.Add(text2[1]);
                        if (hour[hour.Count - 1] == "24")
                            hourplus = 1;
                        wgmettime.Add(text2[2]);
                        wrmettime.Add(text2[3]);
                        akmettime.Add(text2[4]);
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
                }
			}
			catch(Exception ex)
			{
				BackgroundThreadMessageBox (ex.Message);
				return;
			}

			//read meteopgt.all
			List<string> data_meteopgt = new List<string>();
			ReadMeteopgtAll(Path.Combine(mydata.Projectname, "Computation", "meteopgt.all"), ref data_meteopgt);
			if (data_meteopgt.Count == 0) // no data available
			{ 
				BackgroundThreadMessageBox ("Can't read meteopgt.all");
			}

			string wgmet;
			string wrmet;
			string akmet;
			double frequency;
			int wl = 0;
            int nnn = 0;
            int n_daytime = 0;
            int n_nighttime = 0;
            int n_evening = 0;
			double ntot = 0;
            float[, ,] conc = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource];
            float[, ,] concp = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource];
            float[, ,] concm = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource + 1];
            float[, ,] Q_cv0 = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource + 1];
            float[, ,] td = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource];
            float[,] R90_array = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] CFI = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] counter = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[, ,] concmit = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1, maxsource + 5];
			double[] fmod = new double[maxsource];

			foreach(string line_meteopgt in data_meteopgt)	
			{
				try
				{
					//set variables to zero
					itm = 0;
					foreach (string source_group_name in sg_names)
					{
						for (int i = 0; i <= mydata.CellsGralX; i++)
							for (int j = 0; j <= mydata.CellsGralY; j++)
						{
                            conc[i, j, itm] = 0;
                            concp[i, j, itm] = 0;
                            concm[i, j, itm] = 0;
						}
						itm++;
					}

					//meteopgt.all
					wl = wl + 1;
					
					if (Rechenknecht.CancellationPending)
					{
                        e.Cancel = true;
                        return;
					}
					if (wl % 4 == 0)
        			{
					   Rechenknecht.ReportProgress((int) (wl / (double) data_meteopgt.Count * 100D));
					}
					
                    bool exist = true;
                    bool exist_conp = false;
					text = line_meteopgt.Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					wrmet = text[0];
					wgmet = text[1];
					akmet = text[2];
					frequency = Convert.ToDouble(text[3].Replace(".", decsep));
					//GRAL filenames
                    string[] con_files = new string[100];
                    string[] odr_files = new string[100];
					string[] concdata = new string[3];
					itm = 0;
					foreach (string source_group_name in sg_names)
					{
                        if (sg_names.Length > 0)
                        {
                            con_files[itm] = Convert.ToString(wl).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + sg_numbers[itm].PadLeft(2, '0') + ".con";
                            odr_files[itm] = Convert.ToString(wl).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + sg_numbers[itm].PadLeft(2, '0') + ".odr";
                        }
                        else
                        {
                            con_files[itm] = Convert.ToString(wl).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + Convert.ToString(sg_names[itm]).PadLeft(2, '0') + ".con";
                            odr_files[itm] = Convert.ToString(wl).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + Convert.ToString(sg_names[itm]).PadLeft(2, '0') + ".odr";
                        }

						if (File.Exists(Path.Combine(mydata.Projectname, @"Computation", con_files[itm])) == false &&
                            File.Exists(Path.Combine(mydata.Projectname, @"Computation", Convert.ToString(wl).PadLeft(5, '0') + ".grz")) == false)
						{
							exist = false;
							break;
                        }

                        //check if vertical adjecent files to compute concentration variance exist
                         if (File.Exists(Path.Combine(mydata.Projectname, @"Computation", odr_files[itm])) == true ||
                            File.Exists(Path.Combine(mydata.Projectname, @"Computation", Convert.ToString(wl).PadLeft(5, '0') + ".grz")) == true)
                        {
                            exist_conp = true;
                        }

						//read GRAL concentration files
						string filename = Path.Combine(mydata.Projectname, @"Computation", con_files[itm]);
                        if (!ReadConFiles(filename, mydata, itm, ref conc))
                        {
                            // Error reading one *.con file
                            exist = false;
                            exist_conp = false;
                        }

                        //read GRAL output files for quantities needed to compute the concentration variance
                        if (exist_conp == true)
                        {
                            string filename_p = Path.Combine(mydata.Projectname, @"Computation", odr_files[itm]);
                            if (!ReadOdrFiles(filename_p, mydata, itm, ref concp, ref concm, ref Q_cv0, ref td))
                            {
                                // Error reading the odr file -> Analysis not possible
                                exist = false;
                            }
                        }
						
						itm++;
					}
					if (exist == true)
					{
						ntot = ntot + frequency / 10;
						
						SetText("Dispersion situation " + Convert.ToString(wl) + ":" + Convert.ToString(Math.Round(ntot, 1) + "%"));

						for (int i = 0; i < hour.Count; i++)
						{
							if ((wgmet == wgmettime[i]) && (wrmet == wrmettime[i]) && (akmet == akmettime[i]))
							{
								nnn = nnn + 1;
								int std = Convert.ToInt32(hour[i]);
								int mon = Convert.ToInt32(month[i]) - 1;

                                //daytime odour-hour frequency
                                if ((std >= 6) && (std < 19))
                                {
                                    n_daytime++;
                                }
                                //evening odour-hour frequency
                                if ((std >= 19) && (std < 22))
                                {
                                    n_evening++;
                                }
                                //nightime odour-hour frequency
                                if ((std >= 22) || (std < 6))
                                {
                                    n_nighttime++;
                                }

                                Parallel.For(0, mydata.CellsGralX + 1, ii =>
                                {
                                    for (int j = 0; j <= mydata.CellsGralY; j++)
                                    {
                                        int n_source = 0;
                                        foreach (string source_group_name in sg_names)
                                        {
                                            float emission_modulation = (float)(emifac_day[std - hourplus, n_source] * emifac_mon[mon, n_source]);
                                            
                                            double frequ = 0;
                                            double frequ2 = 0;
                                            double emi2 = 0;
                                            float R90 = 4;
                                            float Q_cv = 0;

                                            if ((ii == 0) && (j == 0))
                                                fmod[n_source] += emission_modulation;

                                            if (mydata.Peakmean < 0)
                                            {
                                                //compute spatially dependent R90 for all plumes
                                                //note that the quantities Q_cv0 and td are flow properties and thus equal for all source groups
                                                if ((ii != 0) && (j != 0) && (ii < mydata.CellsGralX) && (j < mydata.CellsGralY) && (conc[ii, j, n_source] != 0) && (td[ii, j, n_source] != 0))
                                                {
                                                    GralConcentrationVarianceModel.Concentration_Variance_Model.R90_calculate(ii, j, conc[ii + 1, j, n_source], conc[ii - 1, j, n_source],
                                                        conc[ii, j + 1, n_source], conc[ii, j - 1, n_source], concp[ii, j, n_source], concm[ii, j, n_source], conc[ii, j, n_source],
                                                        Q_cv0[ii, j, n_source], td[ii, j, n_source], (float)mydata.Horgridsize, (float)mydata.VertgridSize, ref R90, ref Q_cv);

                                                    CFI[ii, j] += Q_cv;
                                                    R90_array[ii, j] += R90;
                                                    counter[ii, j]++;
                                                }
                                            }

                                            //frequency for all sources together
                                            if (mydata.Peakmean < 0)
                                            {
                                                if (conc[ii, j, n_source] * mydata.Scale * totemi * R90 * 0.001 * emission_modulation >= (float)(mydata.OdourThreshold))
                                                    frequ = Math.Max(frequ, totfreq);
                                            }
                                            else
                                            {
                                                if (conc[ii, j, n_source] * mydata.Scale * totemi * (float)(mydata.Peakmean) * 0.001 * emission_modulation >= (float)(mydata.OdourThreshold))
                                                    frequ = Math.Max(frequ, totfreq);
                                            }

                                            //frequency of single processes
                                            if (mydata.Peakmean < 0)
                                            {
                                                for (int r = 0; r < 3; r++)
                                                {
                                                    if (conc[ii, j, n_source] * mydata.Scale * mydata.Odemi[r] * R90 * 0.001 * emission_modulation >= (float)(mydata.OdourThreshold))
                                                        frequ = Math.Max(frequ, mydata.OdFreq[r]);
                                                }
                                            }
                                            else
                                            {
                                                for (int r = 0; r < 3; r++)
                                                {
                                                    if (conc[ii, j, n_source] * mydata.Scale * mydata.Odemi[r] * (float)(mydata.Peakmean) * 0.001 * emission_modulation >= (float)(mydata.OdourThreshold))
                                                        frequ = Math.Max(frequ, mydata.OdFreq[r]);
                                                }
                                            }

                                            //frequency of 2 source combinations
                                            if (mydata.Peakmean < 0)
                                            {
                                                for (int r = 2; r > -1; r--)
                                                {
                                                    for (int l = r - 1; l > -1; l--)
                                                    {
                                                        emi2 = mydata.Odemi[r] + mydata.Odemi[l];
                                                        frequ2 = mydata.OdFreq[r] * mydata.OdFreq[l];
                                                        if (conc[ii, j, n_source] * mydata.Scale * emi2 * R90 * 0.001 * emission_modulation >= (float)(mydata.OdourThreshold))
                                                            frequ = Math.Max(frequ, frequ2);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                for (int r = 2; r > -1; r--)
                                                {
                                                    for (int l = r - 1; l > -1; l--)
                                                    {
                                                        emi2 = mydata.Odemi[r] + mydata.Odemi[l];
                                                        frequ2 = mydata.OdFreq[r] * mydata.OdFreq[l];
                                                        if (conc[ii, j, n_source] * mydata.Scale * emi2 * (float)(mydata.Peakmean) * 0.001 * emission_modulation >= (float)(mydata.OdourThreshold))
                                                            frequ = Math.Max(frequ, frequ2);
                                                    }
                                                }
                                            }

                                            //annual unweighted odour hours
                                            concmit[ii, j, n_source] += (float)frequ;
                                            
                                            //annual weighted odour hours
                                            double weight = 1;
                                            if ((mon < 4)||(mon < 11))
                                            {
                                                weight = 0.6447;
                                                if ((std >= 6) && (std < 18))
                                                    weight = 1;

                                            }
                                            else
                                            {
                                                weight = 0.8381;
                                                if ((std >= 6) && (std < 21))
                                                    weight = 1.3;
                                            }
                                            concmit[ii, j, maxsource + 4] += (float)(frequ * weight);

                                            //daytime odour-hour frequency
                                            if ((std >= 6) && (std < 19))
                                            {
                                                concmit[ii, j, maxsource + 1] += (float)frequ;
                                            }
                                            //evening odour-hour frequency
                                            if ((std >= 19) && (std < 22))
                                            {
                                                concmit[ii, j, maxsource + 2] += (float)frequ;
                                            }
                                            //nightime odour-hour frequency
                                            if ((std >= 22) || (std < 6))
                                            {
                                                concmit[ii, j, maxsource + 3] += (float)frequ;
                                            }

                                            n_source++;
                                        }
                                    }
                                });                                

								hour.RemoveAt(i);
								month.RemoveAt(i);
								wgmettime.RemoveAt(i);
								wrmettime.RemoveAt(i);
								akmettime.RemoveAt(i);
								i = i - 1;
							}
						}
					}
				}
				catch
				{
					//break;
				}
			}
			
			//final computations
			if (nnn > 0)
			{
				itm = 0;
				foreach (string source_group_name in sg_names)
				{
					fmod[itm] = fmod[itm] / Convert.ToDouble(nnn);
					for (int i = 0; i <= mydata.CellsGralX; i++)
						for (int j = 0; j <= mydata.CellsGralY; j++)
					{
                        concmit[i, j, itm] = concmit[i, j, itm] / (float)nnn * 100;
					}
					itm++;
				}

                Parallel.For(0, mydata.CellsGralX + 1, i =>
                {
                    for (int j = 0; j <= mydata.CellsGralY; j++)
                    {
                        concmit[i, j, maxsource + 1] = concmit[i, j, maxsource + 1] / (float)n_daytime * 100;
                        concmit[i, j, maxsource + 2] = concmit[i, j, maxsource + 2] / (float)n_evening * 100;
                        concmit[i, j, maxsource + 3] = concmit[i, j, maxsource + 3] / (float)n_nighttime * 100;
                        concmit[i, j, maxsource + 4] = concmit[i, j, maxsource + 4] / (float)nnn * 100;
                        if (counter[i, j] > 0)
                        {
                            R90_array[i, j] = R90_array[i, j] / (float)counter[i, j];
                            CFI[i, j] = CFI[i, j] / (float)counter[i, j];
                        }
                    }
                });
			}

			//write mean odour hour files for each source group
			string file;
			string name;
            GralIO.WriteESRIFile Result = new GralIO.WriteESRIFile
            {
                NCols = mydata.CellsGralX,
                NRows = mydata.CellsGralY,
                YllCorner = mydata.DomainSouth,
                XllCorner = mydata.DomainWest,
                CellSize = mydata.Horgridsize,
                Unit = "%",
                Round = 3
            };

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
					name = mydata.Prefix + mydata.Pollutant + "_" + text1a[0] + "_" + mydata.Slicename + "_" + Convert.ToString(mydata.OdourThreshold) + "GE_PM" + Convert.ToString(mydata.Peakmean);
				}
				else
					name = mydata.Prefix + mydata.Pollutant + "_" + sg_names[itm] + "_" + mydata.Slicename + "_" + Convert.ToString(mydata.OdourThreshold) + "GE_PM" + Convert.ToString(mydata.Peakmean);
				file = Path.Combine(mydata.Projectname, @"Maps", "Mean_Compost_" + name + ".txt");
				Result.Z = itm;
				Result.Values = concmit;
				Result.FileName = file;
				Result.WriteFloatResult();
				
				itm++;
			}

            //write mean total daytime odour hour file
            /*
            name = mydata.Prefix + mydata.Pollutant + "_total_6-18h" + "_" + mydata.Slicename + "_" + Convert.ToString(mydata.OdourThreshold) + "GE_PM" + Convert.ToString(mydata.Peakmean);
            file = Path.Combine(mydata.Projectname, @"Maps", "Mean_" + name + ".txt");
            Result.Z = maxsource + 1;
			Result.Values = concmit;
			Result.FileName = file;
			Result.Write_Result();
           
            //write mean total evening odour hour file
            name = mydata.Prefix + mydata.Pollutant + "_total_19-21h" + "_" + mydata.Slicename + "_" + Convert.ToString(mydata.OdourThreshold) + "GE_PM" + Convert.ToString(mydata.Peakmean);
            file = Path.Combine(mydata.Projectname, @"Maps", "Mean_" + name + ".txt");
            Result.Z = maxsource + 2;
			Result.Values = concmit;
			Result.FileName = file;
			Result.Write_Result();
			
            //write mean total nighttime odour hour file
            name = mydata.Prefix + mydata.Pollutant + "_total_22-5h" + "_" + mydata.Slicename + "_" + Convert.ToString(mydata.OdourThreshold) + "GE_PM" + Convert.ToString(mydata.Peakmean);
            file = Path.Combine(mydata.Projectname, @"Maps", "Mean_" + name + ".txt");
            Result.Z = maxsource + 3;
			Result.Values = concmit;
			Result.FileName = file;
			Result.Write_Result();
            
            //write mean total weighted odour hour file
            name = mydata.Prefix + mydata.Pollutant + "_total_weighted" + "_" + mydata.Slicename + "_" + Convert.ToString(mydata.OdourThreshold) + "GE_PM" + Convert.ToString(mydata.Peakmean);
            file = Path.Combine(mydata.Projectname, @"Maps", "Mean_" + name + ".txt");
            Result.Z = maxsource + 4;
			Result.Values = concmit;
			Result.FileName = file;
			Result.Write_Result();
             */
            
            if (mydata.Peakmean < 0) // use new odour model
            {
            	//write mean total R90
            	string name5 = mydata.Prefix + mydata.Pollutant + "_" + mydata.Slicename + "_total";
            	string file5 = Path.Combine(mydata.Projectname, @"Maps", "R90_" + name5 + ".txt");
            	Result.Z = -1;
            	Result.Round = 2;
                Result.Unit = "-";
				Result.TwoDim = R90_array;
				Result.FileName = file5;
				Result.WriteFloatResult();
            	
            	//write mean total concentration flucutation intensity
            	name5 = mydata.Prefix + mydata.Pollutant + "_" + mydata.Slicename + "_total";
            	file5 = Path.Combine(mydata.Projectname, @"Maps", "ConcentrationFluctuationIntensity_" + name5 + ".txt");
            	Result.TwoDim = CFI;
				Result.FileName = file5;
				Result.WriteFloatResult();
            	
            }
            
            Computation_Completed = true; // set flag, that computation was successful
		}
	}
}
