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
using System.IO;
using System.Collections.Generic;
using GralIO;
using System.Threading.Tasks;

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
            string decsep = mydata.DecSep;
            string[] text = new string[5];
            string newpath;
            string[] sg_names = mydata.SelectedSourceGroup.Split(',');
            double[] sg_mean_modulation_sum = new double[maxsource];
            int[] sg_mean_modulation_count = new int[maxsource];
            bool deposition_files_exists = false;
            bool no_classification = mydata.MeteoNotClassified;

            //get emission modulations for all source groups
            (double[,] emifac_day, double[,] emifac_mon, string[] sg_numbers) = ReadEmissionModulationFactors(maxsource, sg_names, mydata.ProjectName);

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
            string MonthPrevious="a";
            string DayPrevious="a";
            string hour;
            int WeatherSituationsOfThisDay = 1;
            int nnn = 0;
            int situationCount = 0;
            //int indexi = 0;
            //int indexj = 0;
            float[][][] conc = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource]));
            float[][][] dep = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource]));
            float[][][] concmit = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 1]));
            float[][][] depmit = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 1]));
            float[][][] concmax = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 1]));
            float[][][] depmax = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 1]));
            float[][][] concdaymax = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 1]));
            float[][][] depdaymax = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 1]));
            float[][][] ConcDaymaxTempSum = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 1]));
            float[][][] DepDaymaxTempSum = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 1]));
            float[,] concmaxdummy = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] depmaxdummy = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];

            //read meteopgt.all
            List<string> data_meteopgt = new List<string>();
            ReadMeteopgtAll(Path.Combine(mydata.ProjectName, "Computation", "meteopgt.all"), ref data_meteopgt);
            if (data_meteopgt.Count == 0) // no data available
            { 
                BackgroundThreadMessageBox ("Can't read meteopgt.all");
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
                using (StreamReader sr = new StreamReader(Path.Combine(mydata.ProjectName, newpath)))
                {
                    //text2 = sr.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    while (sr.EndOfStream == false)
                    {
                        mettimefilelength += 1;
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

            //in transient GRAL mode, it is necessary to set all modulation factors equal to one as they have been considered already in the GRAL simulations
            bool transientMode = CheckForTransientMode(mydata.ProjectName);
            if (transientMode)
            {
                AddInfoText(Environment.NewLine + "Transient simulation -> emission modulation was considered in GRAL" + Environment.NewLine);
                //set emifac_day and emifac_mon equal one in transient mode
                for (int itm1 = 0; itm1 < maxsource; itm1++)
                {
                    for (int j = 0; j < 24; j++)
                    {
                        emifac_day[j, itm1] = 1;
                        if (j < 12)
                            emifac_mon[j, itm1] = 1;
                    }
                }
            }

            //if file emissions_timeseries.txt exists, these modulation factors will be used
            double[,] emifac_timeseries = new double[mettimefilelength + 1, maxsource];
            
            //it is necessary to set all values of the array emifac_timeseries equal to 1
            for (int i = 0; i < mettimefilelength + 1; i++)
            {
                for (int n = 0; n < maxsource; n++)
                {
                    emifac_timeseries[i, n] = 1;
                }
            }

            // modulation = 1 in transient mode
            if (!transientMode)
            {
                emifac_timeseries = ReadEmissionModulationTimeSeries(mettimefilelength, maxsource, mydata.ProjectName,
                                                                               sg_numbers, ref emifac_day, ref emifac_mon, sg_names);
            }

            //read mettimeseries.dat
            List<string> data_mettimeseries = new List<string>();
            ReadMettimeseries(Path.Combine(mydata.ProjectName, "Computation", "mettimeseries.dat"), ref data_mettimeseries);
            if (data_mettimeseries.Count == 0) // no data available
            { 
                BackgroundThreadMessageBox ("Can't read mettimeseries.dat");
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
                    hourplus = 1;
                wgmettime=text2[2];
                wrmettime=text2[3];
                akmettime=text2[4];

                //in case of transient simulation there is no need to loop over meteopgt.all
                int meteo_loop = wrmet.Count;
                if (transientMode || no_classification == true)
                {
                    meteo_loop = 1;
                }

                //search in file meteopgt.all for the corresponding dispersion situation
                for (int n = 0; n < meteo_loop; n++)
                {
                    //in case of transient simulation there is no need to loop over meteopgt.all
                    bool meteo_situation_found = false;                    
                    if (transientMode || no_classification==true)
                    {
                        meteo_situation_found = true;
                    }
                    else
                    {
                        if ((wgmet[n] == wgmettime) && (wrmet[n] == wrmettime) && (akmet[n] == akmettime))
                            meteo_situation_found = true;
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
                        if (transientMode || no_classification == true)
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

                            if (File.Exists(Path.Combine(mydata.ProjectName, @"Computation", dep_files[itmp])) == false &&
                                File.Exists(Path.Combine(mydata.ProjectName, @"Computation", Convert.ToString(weanumb + 1).PadLeft(5, '0') + ".grz")) == false)
                            {
                                lock(thisLock)
                                {
                                    existdep = false;
                                }
                                parallel_existdep = false;
                            }

                            if (File.Exists(Path.Combine(mydata.ProjectName, @"Computation", con_files[itmp])) == false &&
                                File.Exists(Path.Combine(mydata.ProjectName, @"Computation", Convert.ToString(weanumb + 1).PadLeft(5, '0') + ".grz")) == false)
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
                                    conc[i][j][itmp] = 0;
                                    dep[i][j][itmp] = 0;
                                }
                            }
                            
                            //read GRAL concentration files
                            string filename = Path.Combine(mydata.ProjectName, @"Computation", con_files[itmp]);
                            if (!ReadConFiles(filename, mydata, itmp, ref conc))
                            {
                                // Error reading one *.con file
                                exist = false;
                                existdep = false;
                            }
                            
                            if (parallel_existdep)
                            {
                                //read GRAL deposition files
                                filename = Path.Combine(mydata.ProjectName, @"Computation", dep_files[itmp]);
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
                            nnn += 1;
                            situationCount++;
                            //number of hours of specific day
                            WeatherSituationsOfThisDay += 1;
                            int std = Convert.ToInt32(hour);
                            int mon = Convert.ToInt32(month) - 1;
                            bool SameDayAndMonth = ((DayPrevious == day) && (MonthPrevious == month)) || (nnn == 1);

                            //for (int ii = 0; ii <= mydata.CellsGralX; ii++)
                            Parallel.For(0, mydata.CellsGralX + 1, ii =>
                            {
                                for (int j = 0; j <= mydata.CellsGralY; j++)
                                {
                                    int _itm = 0;
                                    float[] ConcDayMaxTempSumL = ConcDaymaxTempSum[ii][j];
                                    float[] DepDayMaxTempSumL  = DepDaymaxTempSum[ii][j];
                                    float[] ConcAverageL = concmit[ii][j];
                                    float[] DepAverageL  = depmit[ii][j];
                                    float[] ConcMaxL = concmax[ii][j];
                                    float[] DepMaxL  = depmax[ii][j];

                                    //reset max. daily concentrations for each source group
                                    concmaxdummy[ii, j] = 0;
                                    if (SameDayAndMonth)
                                    { }
                                    else // reset daymax temp values and calculate daymax value
                                    {
                                        float[] ConcDayMaxL = concdaymax[ii][j];
                                        float[] DepDayMaxL  = depdaymax[ii][j];
                                        
                                        ConcDayMaxL[maxsource] = Math.Max(ConcDayMaxL[maxsource], ConcDayMaxTempSumL[maxsource] / WeatherSituationsOfThisDay);
                                        DepDayMaxL[maxsource] = Math.Max(DepDayMaxL[maxsource], DepDayMaxTempSumL[maxsource] / WeatherSituationsOfThisDay);
                                        _itm = 0;
                                        foreach (string source_group_name in sg_names)
                                        {
                                            ConcDayMaxL[_itm] = Math.Max(ConcDayMaxL[_itm], ConcDayMaxTempSumL[_itm] / WeatherSituationsOfThisDay);
                                            DepDayMaxL[_itm] = Math.Max(DepDayMaxL[_itm], DepDayMaxTempSumL[_itm] / WeatherSituationsOfThisDay);
                                            _itm++;
                                        }

                                        //reset temp arrays
                                        ConcDayMaxTempSumL[maxsource] = 0;
                                        DepDayMaxTempSumL[maxsource] = 0;
                                        _itm = 0;
                                        foreach (string source_group_name in sg_names)
                                        {
                                            ConcDayMaxTempSumL[_itm] = 0;
                                            DepDayMaxTempSumL[_itm] = 0;
                                            _itm++;
                                        }
                                    } // reset daymax values and calculate daymax value

                                    // Evaluate mean and max values
                                    _itm = 0;
                                    foreach (string source_group_name in sg_names)
                                    {
                                        //compute mean emission modulation factor
                                        float fac = conc[ii][j][_itm] * (float)emifac_day[std - hourplus, _itm] * (float)emifac_mon[mon, _itm] * (float)emifac_timeseries[count_ws, _itm];
                                        float facdep = dep[ii][j][_itm] * (float)emifac_day[std - hourplus, _itm] * (float)emifac_mon[mon, _itm] * (float)emifac_timeseries[count_ws, _itm];
                                        //compute mean concentrations for each source group and in total
                                        ConcAverageL[_itm] = ConcAverageL[_itm] + fac;
                                        ConcAverageL[maxsource] = ConcAverageL[maxsource] + fac;
                                        DepAverageL[_itm] = DepAverageL[_itm] + facdep;
                                        DepAverageL[maxsource] = DepAverageL[maxsource] + facdep;

                                        //compute max concentrations for each source group and in total
                                        ConcMaxL[_itm] = Math.Max(ConcMaxL[_itm], fac);
                                        concmaxdummy[ii, j] = concmaxdummy[ii, j] + fac;
                                        DepMaxL[_itm] = Math.Max(DepMaxL[_itm], facdep);
                                        depmaxdummy[ii, j] = depmaxdummy[ii, j] + facdep;

                                        //compute max. daily concentrations for each source group
                                        ConcDayMaxTempSumL[_itm] = ConcDayMaxTempSumL[_itm] + fac;
                                        ConcDayMaxTempSumL[maxsource] = ConcDayMaxTempSumL[maxsource] + fac;
                                        DepDayMaxTempSumL[_itm] = DepDayMaxTempSumL[_itm] + facdep;
                                        DepDayMaxTempSumL[maxsource] = DepDayMaxTempSumL[maxsource] + facdep;
                                        _itm++;
                                    }
                                    ConcMaxL[maxsource] = Math.Max(ConcMaxL[maxsource], concmaxdummy[ii, j]);
                                    DepMaxL[maxsource] = Math.Max(DepMaxL[maxsource], depmaxdummy[ii, j]);
                                }
                            });

                            if (SameDayAndMonth)
                            { }
                            else // reset number of weather situations per day
                            {
                                WeatherSituationsOfThisDay = 1; // the number of weather situations per day might be 24, 48 (0.5 h values) or any other multiple
                            }
                        }
                    }
                } // search in meteopgt.all

                // save the last used day to recognize a new day
                DayPrevious = day;
                MonthPrevious = month;		

            } // Loop over all weather situations of a year
            
            //final computations
            if (nnn > 0)
            {
                int itm = 0;
                foreach (string source_group_name in sg_names)
                {
                    for (int i = 0; i <= mydata.CellsGralX; i++)
                    {
                        for (int j = 0; j <= mydata.CellsGralY; j++)
                        {
                            concmit[i][j][itm] = concmit[i][j][itm] / (float)nnn;
                            depmit[i][j][itm] = depmit[i][j][itm] / (float)nnn;
                        }
                    }
                    itm++;
                }
                //total concentration
                for (int i = 0; i <= mydata.CellsGralX; i++)
                    for (int j = 0; j <= mydata.CellsGralY; j++)
                {
                    concmit[i][j][maxsource] = concmit[i][j][maxsource] / (float)nnn;
                    depmit[i][j][maxsource] = depmit[i][j][maxsource] / (float)nnn;
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
            if (mydata.CalculateMean == true)
            {
                int itm = 0;
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
                        name = mydata.Prefix + mydata.Pollutant + "_" + sg_names[itm];
                    
                    file = Path.Combine(mydata.ProjectName, @"Maps", "Mean_" + name + "_" + mydata.Slicename + ".txt");
                    
                    Result.Unit = Gral.Main.My_p_m3;
                    Result.Round = 5;
                    Result.Z = itm;
                    Result.Values = concmit;
                    Result.FileName = file;
                    Result.WriteFloatResult();
                    AddInfoText(Environment.NewLine + "Writing result file " + file);

                    if (deposition_files_exists)
                    {
                        file = Path.Combine(mydata.ProjectName, @"Maps", "Deposition_Mean_" + "_" +name + ".txt");
                        Result.Unit = Gral.Main.mg_p_m2;
                        Result.Round = 9;
                        Result.Z = itm;
                        Result.Values = depmit;
                        Result.FileName = file;
                        Result.WriteFloatResult();
                        AddInfoText(Environment.NewLine + "Writing result file " + file);
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
                file = Path.Combine(mydata.ProjectName, @"Maps", "Mean_" + name + ".txt");
                Result.Unit = Gral.Main.My_p_m3;
                Result.Round = 5;
                Result.Z = maxsource;
                Result.Values = concmit;
                Result.FileName = file;
                Result.WriteFloatResult();
                AddInfoText(Environment.NewLine + "Writing result file " + file);

                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                if (deposition_files_exists)
                {
                    file = Path.Combine(mydata.ProjectName, @"Maps", "Deposition_Mean_" + mydata.Prefix + mydata.Pollutant + "_total.txt");
                    Result.Unit = Gral.Main.mg_p_m2;
                    Result.Round = 9;
                    Result.Z = maxsource;
                    Result.Values = depmit;
                    Result.FileName = file;
                    Result.WriteFloatResult();
                    AddInfoText(Environment.NewLine + "Writing result file " + file);
                }
            }

            //write max concentration files for each source group
            if (mydata.CalculateMaxHour == true)
            {
                int itm = 0;
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
                    
                    file = Path.Combine(mydata.ProjectName, @"Maps", "Max_" + name + ".txt");
                    Result.Unit = Gral.Main.My_p_m3;
                    Result.Round = 5;
                    Result.Z = itm;
                    Result.Values = concmax;
                    Result.FileName = file;
                    Result.WriteFloatResult();
                    AddInfoText(Environment.NewLine + "Writing result file " + file);

                    if (deposition_files_exists)
                    {
                        file = Path.Combine(mydata.ProjectName, @"Maps", "Deposition_Max" + depo_name + ".txt");
                        Result.Unit = Gral.Main.mg_p_m2;
                        Result.Round = 9;
                        Result.Z = itm;
                        Result.Values = depmax;
                        Result.FileName = file;
                        Result.WriteFloatResult();
                        AddInfoText(Environment.NewLine + "Writing result file " + file);
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
                file = Path.Combine(mydata.ProjectName, @"Maps", "Max_" + name + ".txt");
                Result.Unit = Gral.Main.My_p_m3;
                Result.Round = 5;
                Result.Z = maxsource;
                Result.Values = concmax;
                Result.FileName = file;
                Result.WriteFloatResult();
                AddInfoText(Environment.NewLine + "Writing result file " + file);

                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                if (deposition_files_exists)
                {
                    file = Path.Combine(mydata.ProjectName, @"Maps", "Deposition_Max" + mydata.Prefix + mydata.Pollutant + "_total.txt");
                    Result.Unit = Gral.Main.mg_p_m2;
                    Result.Round = 9;
                    Result.Z = maxsource;
                    Result.Values = depmax;
                    Result.FileName = file;
                    Result.WriteFloatResult();
                    AddInfoText(Environment.NewLine + "Writing result file " + file);
                }
            }

            //write max daily concentration files for each source group
            if (mydata.CalculateDayMax == true)
            {
                int itm = 0;
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
                    file = Path.Combine(mydata.ProjectName, @"Maps", "DayMax_" + name + ".txt");
                    Result.Unit = Gral.Main.My_p_m3;
                    Result.Round = 5;
                    Result.Z = itm;
                    Result.Values = concdaymax;
                    Result.FileName = file;
                    Result.WriteFloatResult();
                    AddInfoText(Environment.NewLine + "Writing result file " + file);

                    if (deposition_files_exists)
                    {
                        file = Path.Combine(mydata.ProjectName, @"Maps", "Deposition_DayMax_" + depo_name + ".txt");
                        Result.Unit = Gral.Main.mg_p_m2;
                        Result.Round = 9;
                        Result.Z = itm;
                        Result.Values = depdaymax;
                        Result.FileName = file;
                        Result.WriteFloatResult();
                        AddInfoText(Environment.NewLine + "Writing result file " + file);
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
                file = Path.Combine(mydata.ProjectName, @"Maps", "DayMax_" + name + ".txt");
                Result.Unit = Gral.Main.My_p_m3;
                Result.Round = 5;
                Result.Z = maxsource;
                Result.Values = concdaymax;
                Result.FileName = file;
                Result.WriteFloatResult();
                AddInfoText(Environment.NewLine + "Writing result file " + file);

                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                if (deposition_files_exists)
                {
                    file = Path.Combine(mydata.ProjectName, @"Maps", "Deposition_DayMax_" + mydata.Prefix + mydata.Pollutant + "_total.txt");
                    Result.Unit = Gral.Main.mg_p_m2;
                    Result.Round = 9;
                    Result.Z = maxsource;
                    Result.Values = depdaymax;
                    Result.FileName = file;
                    Result.WriteFloatResult();
                    AddInfoText(Environment.NewLine + "Writing result file " + file);
                }
            }
            AddInfoText(Environment.NewLine + "Process finished - " + situationCount.ToString() + " *.con files processed " + DateTime.Now.ToShortTimeString());
            Computation_Completed = true; // set flag, that computation was successful
        }
    }
}