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
using System.IO;

namespace GralBackgroundworkers
{
    public partial class ProgressFormBackgroundworker
    {
        /// <summary>
        /// Calculate the mean values in steady state mode
        /// </summary>
#if NET7_0_OR_GREATER
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
#endif
        private void Mean(GralBackgroundworkers.BackgroundworkerData mydata,
                          System.ComponentModel.DoWorkEventArgs e)
        {
            //reading emission variations
            int maxsource = mydata.MaxSource;
            string decsep = mydata.DecSep;
            string[] text = new string[5];
            string newpath;
            string[] sg_names = mydata.SelectedSourceGroup.Split(',');
            bool deposition_files_exists = false;
            int itm = 0;

            //get emission modulations for all source groups
            (double[,] emifac_day, double[,] emifac_mon, string[] sg_numbers) = ReadEmissionModulationFactors(maxsource, sg_names, mydata.PathEmissionModulation);

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

            AddInfoText(Environment.NewLine + "Using diurnal and annual factors for the emission modulation" + Environment.NewLine);
            for (int n = 0; n < maxsource; n++)
            {
                int count = 0;
                double sum = 0;
                for (int h = 0; h < 24; h++)
                {
                    for (int m = 0; m < 12; m++)
                    {
                        sum += emifac_mon[m, n] * emifac_day[h, n];
                        count++;
                    }
                }
                if (n < sg_names.Length)
                {
                    AddInfoText("Mean modulation factor for source group " + sg_numbers[n].ToString() + " = " + Math.Round(sum / Math.Max(count, 1), 2) + Environment.NewLine);
                }
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
                using (StreamReader read = new StreamReader(Path.Combine(mydata.ProjectName, newpath)))
                {
                    if (!read.EndOfStream)
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
                            if (read.EndOfStream)
                            {
                                break;
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
                    }
                }
            }
            catch (Exception ex)
            {
                BackgroundThreadMessageBox(ex.Message);
                return;
            }

            //read meteopgt.all
            List<string> data_meteopgt = new List<string>();
            ReadMeteopgtAll(Path.Combine(mydata.ProjectName, "Computation", "meteopgt.all"), ref data_meteopgt);

            if (data_meteopgt.Count == 0) // no data available
            {
                BackgroundThreadMessageBox("Error reading meteopgt.all");
            }

            string wgmet;
            string wrmet;
            string akmet;
            double frequency;
            int wl = 0;
            int nnn = 0;
            int situationCount = 0;
            double ntot = 0;

            float[][][] conc = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource]));
            float[][][] dep = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource]));
            float[][][] concmit = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 1]));
            float[][][] depmit = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 1]));

            double[] emmit = new double[maxsource];
            double[] fmod = new double[maxsource];

            foreach (string line_meteopgt in data_meteopgt)
            {
                try
                {
                    //meteopgt.all
                    wl += 1;

                    if (Rechenknecht.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    if (wl % 4 == 0)
                    {
                        Rechenknecht.ReportProgress((int)(wl / (double)data_meteopgt.Count * 100D));
                    }

                    bool exist = true;
                    bool exist_dep = true;
                    text = line_meteopgt.Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    wrmet = text[0];
                    wgmet = text[1];
                    akmet = text[2];
                    frequency = Convert.ToDouble(text[3].Replace(".", decsep));

                    //GRAL filenames
                    string[] con_files = new string[100];
                    string[] dep_files = new string[100];
                    itm = 0;
                    foreach (string source_group_name in sg_names)
                    {
                        if (sg_names.Length > 0)
                        {
                            con_files[itm] = Convert.ToString(wl).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + sg_numbers[itm].PadLeft(2, '0') + ".con";
                            dep_files[itm] = Convert.ToString(wl).PadLeft(5, '0') + "-" + sg_numbers[itm].PadLeft(2, '0') + ".dep";
                        }
                        else
                        {
                            con_files[itm] = Convert.ToString(wl).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + Convert.ToString(sg_names[itm]).PadLeft(2, '0') + ".con";
                            dep_files[itm] = Convert.ToString(wl).PadLeft(5, '0') + "-" + Convert.ToString(sg_names[itm]).PadLeft(2, '0') + ".dep";
                        }

                        if (File.Exists(Path.Combine(mydata.ProjectName, @"Computation", dep_files[itm])) == false &&
                            File.Exists(Path.Combine(mydata.ProjectName, @"Computation", Convert.ToString(wl).PadLeft(5, '0') + ".grz")) == false)
                        {
                            exist_dep = false;
                        }

                        if (File.Exists(Path.Combine(mydata.ProjectName, @"Computation", con_files[itm])) == false &&
                            File.Exists(Path.Combine(mydata.ProjectName, @"Computation", Convert.ToString(wl).PadLeft(5, '0') + ".grz")) == false)
                        {
                            exist = false;
                            break;
                        }
                        itm++;
                    }

                    if ((exist == true) || (exist_dep == true))
                    {
                        //set variables to zero
                        itm = 0;
                        foreach (string source_group_name in sg_names)
                        {
                            emmit[itm] = 0;
                            itm++;
                        }

                        //mean emission factor for this dispersion situation
                        int wl_freq = 0;
                        for (int i = 0; i < hour.Count; i++)
                        {
                            if ((wgmet == wgmettime[i]) && (wrmet == wrmettime[i]) && (akmet == akmettime[i]))
                            {
                                wl_freq++; // frequency of this weather situation
                                int std = Convert.ToInt32(hour[i]);
                                int mon = Convert.ToInt32(month[i]) - 1;
                                itm = 0;
                                foreach (string source_group_name in sg_names)
                                {
                                    emmit[itm] = emmit[itm] + emifac_day[std - hourplus, itm] * emifac_mon[mon, itm];

                                    fmod[itm] = fmod[itm] + emifac_day[std - hourplus, itm] * emifac_mon[mon, itm];
                                    itm++;
                                }

                                hour.RemoveAt(i);
                                month.RemoveAt(i);
                                wgmettime.RemoveAt(i);
                                wrmettime.RemoveAt(i);
                                akmettime.RemoveAt(i);
                                i -= 1;
                            }
                        }

                        ntot += frequency / 10;
                        SetText("Dispersion situation " + Convert.ToString(wl) + ": " + Convert.ToString(Math.Round(ntot, 1) + "%"));

                        string[] concdata = new string[3];

                        //read GRAL concentration and deposition files
                        itm = 0;
                        foreach (string source_group_name in sg_names)
                        {
                            RestoreJaggedArray(conc);
                            RestoreJaggedArray(dep);

                            //read GRAL concentration files
                            string filename = Path.Combine(mydata.ProjectName, @"Computation", con_files[itm]);
                            bool ConFileOK = ReadConFiles(filename, mydata, itm, ref conc);

                            // at least one con file OK -> add frequency of this weather situation to the total frequency
                            if (itm == 0 && ConFileOK)
                            {
                                nnn += wl_freq;
                                situationCount++;
                            }

                            //read GRAL deposition files
                            filename = Path.Combine(mydata.ProjectName, @"Computation", dep_files[itm]);
                            bool DepFileOK = ReadConFiles(filename, mydata, itm, ref dep);
                            if (DepFileOK)
                            {
                                deposition_files_exists = true;
                            }

                            for (int ii = 0; ii <= mydata.CellsGralX; ii++)
                            {
                                for (int jj = 0; jj <= mydata.CellsGralY; jj++)
                                {
                                    if (ConFileOK)
                                    {
                                        concmit[ii][jj][itm] += (float)(emmit[itm] * conc[ii][jj][itm]);
                                        concmit[ii][jj][maxsource] += (float)(emmit[itm] * conc[ii][jj][itm]);
                                    }
                                    if (DepFileOK)
                                    {
                                        depmit[ii][jj][itm] += (float)(emmit[itm] * dep[ii][jj][itm]);
                                        depmit[ii][jj][maxsource] += (float)(emmit[itm] * dep[ii][jj][itm]);
                                    }
                                }
                            }
                            itm++;
                        } // read GRAL concentration and deposition files
                    }
                }
                catch
                {
                    //break;
                }
            }

            SetText("Finalizing....");

            //final computations
            if (nnn > 0)
            {
                itm = 0;
                foreach (string source_group_name in sg_names)
                {
                    fmod[itm] = fmod[itm] / Convert.ToDouble(nnn);

                    if (mydata.CalculateMean == true)
                    {
                        for (int i = 0; i <= mydata.CellsGralX; i++)
                            for (int j = 0; j <= mydata.CellsGralY; j++)
                            {
                                concmit[i][j][itm] = concmit[i][j][itm] / (float)nnn;
                                depmit[i][j][itm] = depmit[i][j][itm] / (float)nnn;
                            }
                    }
                    itm++;
                }
                if (mydata.CalculateMean == true)
                {
                    //total concentration
                    for (int i = 0; i <= mydata.CellsGralX; i++)
                        for (int j = 0; j <= mydata.CellsGralY; j++)
                        {
                            concmit[i][j][maxsource] = concmit[i][j][maxsource] / (float)nnn;
                            depmit[i][j][maxsource] = depmit[i][j][maxsource] / (float)nnn;
                        }
                }
            }

            GralIO.WriteESRIFile Result = new GralIO.WriteESRIFile
            {
                NCols = mydata.CellsGralX,
                NRows = mydata.CellsGralY,
                YllCorner = mydata.DomainSouth,
                XllCorner = mydata.DomainWest,
                CellSize = mydata.Horgridsize
            };

            if (mydata.CalculateMean == true)
            {
                //write mean concentration files for each source group
                string file;
                string name;
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

                    file = Path.Combine(mydata.PathEvaluationResults, "Mean_" + name + "_" + mydata.Slicename + ".txt");
                    Result.Unit = Gral.Main.My_p_m3;
                    Result.Round = 5;
                    Result.Z = itm;
                    Result.Values = concmit;
                    Result.FileName = file;
                    Result.WriteFloatResult();
                    AddInfoText(Environment.NewLine + "Writing result file " + file);

                    if (deposition_files_exists && mydata.WriteDepositionOrOdourData)
                    {
                        file = Path.Combine(mydata.PathEvaluationResults, "Deposition_Mean_" + "_" + name + ".txt");

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

                //write mean total concentration and deposition file
                name = mydata.Prefix + mydata.Pollutant + "_total" + "_" + mydata.Slicename;
                file = Path.Combine(mydata.PathEvaluationResults, "Mean_" + name + ".txt");

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

                if (deposition_files_exists && mydata.WriteDepositionOrOdourData)
                {
                    file = Path.Combine(mydata.PathEvaluationResults, "Deposition_Mean_" + mydata.Prefix + mydata.Pollutant + "_total.txt");

                    Result.Unit = Gral.Main.mg_p_m2;
                    Result.Round = 9;
                    Result.Z = maxsource;
                    Result.Values = depmit;
                    Result.FileName = file;
                    Result.WriteFloatResult();
                    AddInfoText(Environment.NewLine + "Writing result file " + file);
                }
            }
            string errorText = string.Empty;
            if (wl > situationCount)
            {
                errorText = " -- " + (wl - situationCount).ToString() + " situation";
                if (wl - situationCount > 1)
                {
                    errorText += "s";
                }
                errorText += " not available or not readable ";
            }
            AddInfoText(Environment.NewLine + "Process finished - " + situationCount.ToString() + " *.con files processed " + errorText + DateTime.Now.ToShortTimeString());
            Computation_Completed = true; // set flag, that computation was successful          
        }
    }
}