#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2020]  [Dietmar Oettl, Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using GralIO;
using System.Text;

namespace GralBackgroundworkers
{
    public partial class ProgressFormBackgroundworker
    {
        /// <summary>
        /// Generate a time series for one or multiple points from *.con files
        /// </summary>
        private void GenerateTimeSeries(GralBackgroundworkers.BackgroundworkerData mydata,
                                       System.ComponentModel.DoWorkEventArgs e)
        {
            int maxsource = mydata.MaxSource;
            string[] sg_names = mydata.SelectedSourceGroup.Split(',');
            bool no_classification = mydata.MeteoNotClassified;

            try
            {
                //read meteopgt.all, mettimeseries.dat
                bool transientMode = CheckForTransientMode(mydata.ProjectName);
                List<string> meteoTimeSeries = new List<string>();
                List<string> meteoPGTALL = new List<string>();
                ReadMettimeseries(Path.Combine(mydata.ProjectName, "Computation", @"mettimeseries.dat"), ref meteoTimeSeries);
                ReadMeteopgtAll(Path.Combine(mydata.ProjectName, "Computation", @"meteopgt.all"), ref meteoPGTALL);
                List<string> meteoInput = meteoPGTALL;
                if (transientMode)
                {
                    meteoInput = meteoTimeSeries;
                    AddInfoText(Environment.NewLine + "Transient simulation -> emission modulation was considered in GRAL" + Environment.NewLine);
                }

                //Read Emission modulation
                (double[,] EmissionFactHour, double[,] EmissionFactMonth, string[] sg_numbers) = ReadEmissionModulationFactors(maxsource, sg_names, mydata.PathEmissionModulation);
                if (sg_numbers == null)
                {
                    throw new Exception("Error reading the emission modulation factors");
                }
                double[,] EmissionFacTimeSeries = ReadEmissionModulationTimeSeries(meteoTimeSeries.Count, maxsource, mydata.ProjectName, 
                                                                               sg_numbers, ref EmissionFactHour, ref EmissionFactMonth, sg_names, mydata.PathEmissionModulation);

                if (meteoPGTALL.Count == 0) // no data available
                {
                    throw new Exception("Error reading meteorological input");
                }

                // Read GRAL Geometries
                GRALGeometry GRALGeom = new GRALGeometry();
                List<EvalPointsIndices> evalPoints = ReadGRALGeometries(GRALGeom, mydata.EvalPoints, mydata);

                //Create Arrays for each evaluation point and for the conc array
                float[][][][] recConc = CreateArray<float[][][]>(evalPoints.Count, () => CreateArray<float[][]>(mydata.Slice, () => CreateArray<float[]>(maxsource + 1, () => new float[meteoInput.Count])));
                float[][][] conc = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource]));

                //loop over all weather situations
                System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                for (int meteoSit = 0; meteoSit < meteoInput.Count; meteoSit++)
                {
                    int sgNumber = 0;
                    Object thisLock = new Object();
                    foreach (string sourceGroupName in sg_names)
                    {
                        for (int _slice = 0; _slice < mydata.Slice; _slice++)
                        {
                            SetText("Processing meteo situation nr. " + (meteoSit + 1).ToString() + " - source group " + sourceGroupName);
                            if (Rechenknecht.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }
                            if (meteoSit % 4 == 0)
                            {
                                Rechenknecht.ReportProgress((int)(meteoSit / (double)meteoInput.Count * 100D));
                            }

                            //set variables to zero
                            for (int i = 0; i <= mydata.CellsGralX; i++)
                            {
                                for (int j = 0; j <= mydata.CellsGralY; j++)
                                {
                                    conc[i][j][sgNumber] = 0;
                                }
                            }

                            string ConFile = Convert.ToString(meteoSit + 1).PadLeft(5, '0') + "-" + Convert.ToString(_slice + 1) + sg_numbers[sgNumber].PadLeft(2, '0') + ".con";
                            string filename = Path.Combine(mydata.ProjectName, @"Computation", ConFile);
                            if (ReadConFiles(filename, mydata, sgNumber, ref conc))
                            {
                                int ptCount = 0;
                                foreach (EvalPointsIndices _pt in evalPoints)
                                {
                                    if (_pt.IxGRAL > 0 && _pt.IxGRAL < GRALGeom.NX &&
                                        _pt.IyGRAL > 0 && _pt.IyGRAL < GRALGeom.NY)
                                    {
                                        recConc[ptCount][_slice][sgNumber][meteoSit] = conc[_pt.IxGRAL][_pt.IyGRAL][sgNumber];
                                    }
                                    ptCount++;
                                }
                            }
                            else //reading threw an error -> set concentration to -1 as a flag for an invalid value
                            {
                                int ptCount = 0;
                                foreach (EvalPointsIndices _pt in evalPoints)
                                {
                                    if (_pt.IxGRAL > 0 && _pt.IxGRAL < GRALGeom.NX &&
                                        _pt.IyGRAL > 0 && _pt.IyGRAL < GRALGeom.NY)
                                    {
                                        recConc[ptCount][_slice][sgNumber][meteoSit] = -1;
                                    }
                                    ptCount++;
                                }
                            }
                        }
                        sgNumber++;
                    }
                }

                //Evaluate results and write result files
                SetText("Final calculations");
                List<string> ReceptorTimeResult = new List<string>();
                int sitCount = 0;
                StringBuilder sb = new StringBuilder();
                foreach (string mettimeseries in meteoTimeSeries)
                {
                    sb.Clear();
                    if (Rechenknecht.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    //get hour and month for emission modulation factors in steady state mode
                    (int hour, int month) = GetHourandMonth(mettimeseries);
                    string[] text2 = mettimeseries.Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    sb.Append(text2[0]);
                    sb.Append("\t");
                    sb.Append(text2[1]);
                    sb.Append("\t");

                    int ptCount = 0;
                    foreach (Point_3D item in mydata.EvalPoints)
                    {
                        for (int _slice = 0; _slice < mydata.Slice; _slice++)
                        {
                            int sgNumber = 0;
                            foreach (string sourceGroupName in sg_names)
                            {
                                if (transientMode)
                                {
                                    double _conc = recConc[ptCount][_slice][sgNumber][sitCount];
                                    if (_conc >= 0)
                                    {
                                        sb.Append(_conc.ToString());
                                    }
                                    else
                                    {
                                        sb.Append(" ");
                                    }
                                    sb.Append("\t");
                                }
                                else
                                {
                                    //search the corresponding meteopgt.all line
                                    int cmpSit = sitCount;
                                    if (!no_classification) // classified meteorology -> search corresponding entry in meteopgt.all
                                    {
                                        cmpSit = SearchCorrespondingMeteopgtAllSituation(meteoTimeSeries, meteoPGTALL, sitCount);
                                    }
                                    if (cmpSit >= 0)
                                    {
                                        double _conc = recConc[ptCount][_slice][sgNumber][cmpSit];
                                        if (_conc >= 0)
                                        {
                                            double fac = EmissionFactHour[hour, sgNumber] * EmissionFactMonth[month, sgNumber] * EmissionFacTimeSeries[sitCount, sgNumber];
                                            sb.Append((recConc[ptCount][_slice][sgNumber][cmpSit] * fac).ToString());
                                        }
                                        else
                                        {
                                            sb.Append(" ");
                                        }
                                    }
                                    else
                                    {
                                        sb.Append(" ");
                                    }
                                    sb.Append("\t");

                                }
                                sgNumber++;
                            }
                        }
                        ptCount++; ;
                    }
                    ReceptorTimeResult.Add(sb.ToString());
                    sitCount++;
                }

                //write the results - Prefix contains the  filename in this case
                string file = Path.Combine(mydata.Prefix);
                AddInfoText(Environment.NewLine + "Writing result file " + file);
                using (StreamWriter mywriter = new StreamWriter(file, false))
                {
                    //write header
                    {
                        string[] header = new string[5];
                        header[0] = "Name\t\t";
                        header[1] = "X-Coor \t\t";
                        header[2] = "Y-Coor \t\t";
                        header[3] = "Source Group \t\t";
                        header[4] = "Slice Height \t\t";
                        foreach (Point_3D item in mydata.EvalPoints)
                        {
                            for (int _slice = 0; _slice < mydata.Slice; _slice++)
                            {
                                foreach (string sourceGroupName in sg_names)
                                {
                                    header[0] += (item.FileName + "\t");
                                    header[1] += (item.X.ToString() + "\t");
                                    header[2] += (item.Y.ToString() + "\t");
                                    header[3] += (sourceGroupName + "\t");
                                    if (mydata.SliceHeights != null && _slice < mydata.SliceHeights.Length)
                                    {
                                        header[4] += (mydata.SliceHeights[_slice].ToString() + "\t");
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < header.Length; i++)
                        {
                            mywriter.WriteLine(header[i]);
                        }
                    }

                    //write data
                    foreach(string _res in ReceptorTimeResult)
                    {
                        mywriter.WriteLine(_res);
                    }
                }
                AddInfoText(Environment.NewLine + "Process finished ");
            }
            catch(Exception ex)
            {
                BackgroundThreadMessageBox(ex.Message);
            }
        }

        /// <summary>
        /// Extract hour and month from a mettimeseries string
        /// </summary>
        /// <param name="mettimeseries">mettimeseries string</param>
        /// <returns>hour and month as integer</returns>
        private (int, int) GetHourandMonth(string mettimeseries)
        {
            //get hour and month for emission modulation factors in steady state mode
            string[] text2 = mettimeseries.Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            string[] text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
            string month = "1";
            string hour = "1";
            if (text3.Length > 1 && text2.Length > 1)
            {
                month = text3[1];
                hour = text2[1];
            }
            int std = Convert.ToInt32(hour);
            int mon = Convert.ToInt32(month) - 1;
            int hourplus = 0;
            if (hour == "24")
            {
                hourplus = 1;
            }
            std = std - hourplus;
            return (std, mon);
        }

        /// <summary>
        /// Read the emission modulation factors
        /// </summary>
        /// <param name="MaxSource">Max count of source groups</param>
        /// <param name="SourceGroupNames">Source group names</param>
        /// <param name="ModulationPath">Path to the emission modulationt</param>
        /// <returns>Arrays for daily and monthly emission modulation factors</returns>
        private (double[,], double[,], string[]) ReadEmissionModulationFactors(int MaxSource, string[] SourceGroupNames, string ModulationPath)
        {
            double[,] emifacHours = new double[24, MaxSource];
            double[,] emifacMonths = new double[12, MaxSource];
            string[] SourceGroupNumbers = new string[MaxSource];
            
            //get variation for source group
            int itm = 0;
            try
            {
                foreach (string sourceGroupName in SourceGroupNames)
                {
                    //reset all emission modulation factors to 1
                    for (int j = 0; j < 24; j++)
                    {
                        emifacHours[j, itm] = 1;
                        if (j < 12)
                        {
                            emifacMonths[j, itm] = 1;
                        }
                    }

                    SourceGroupNumbers[itm] = GetSgNumbers(sourceGroupName);
                    string newpath = Path.Combine(ModulationPath, "emissions" + SourceGroupNumbers[itm].PadLeft(3, '0') + ".dat");

                    if (File.Exists(newpath))
                    {
                        try
                        {
                            using (StreamReader myreader = new StreamReader(newpath))
                            {
                                for (int j = 0; j < 24; j++)
                                {
                                    string[] text = myreader.ReadLine().Split(new char[] { ',' });
                                    emifacHours[j, itm] = Convert.ToDouble(text[1], ic);
                                    if (j < 12)
                                    {
                                        emifacMonths[j, itm] = Convert.ToDouble(text[2], ic);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            AddInfoText(Environment.NewLine + "Error reading file: " + newpath);
                        }
                    }
                    else
                    {
                        AddInfoText(Environment.NewLine + "The file " + newpath + " does not exist");
                    }
                    itm++;
                }
                AddInfoText(Environment.NewLine + Environment.NewLine + "Reading emission modulation factors from: " + ModulationPath);
            }
            catch 
            {
                AddInfoText(Environment.NewLine + Environment.NewLine + "Error reading the emission modulation factors from: " + ModulationPath);
                return (null, null, null);
            }

            return (emifacHours, emifacMonths, SourceGroupNumbers);
        }

        /// <summary>
        /// Read the emission time series file
        /// </summary>
        /// <param name="MettimeFileLength">Langht of the meteo time series</param>
        /// <param name="MaxSource">Maximum number of source groups</param>
        /// <param name="ProjectName">Project path</param>
        /// <param name="SGNumbers">Numbers of used source groups</param>
        /// <param name="emifac_day">diurnal emission factors</param>
        /// <param name="emifac_mon">monthly emission factors</param>
        /// <param name="ModulationPath">Path to an emission timeseries file</param>
        /// <returns>Emission time series</returns>
        private double[,] ReadEmissionModulationTimeSeries(int MettimeFileLength, int MaxSource, string ProjectName, 
                                                           string[] SGNumbers, ref double[,] emifac_day, ref double[,] emifac_mon, string[] sg_names, string ModulationPath)
        {
            double[,] emifac_timeseries = new double[MettimeFileLength + 1, MaxSource];
            int[] sg_time = new int[MaxSource];
            double[] sg_mean_modulation_sum = new double[MaxSource];
            int[] sg_mean_modulation_count = new int[MaxSource];

            //it is necessary to set all values of the array emifac_timeseries equal to 1
            for (int i = 0; i < MettimeFileLength + 1; i++)
            {
                for (int n = 0; n < MaxSource; n++)
                {
                    emifac_timeseries[i, n] = 1;
                }
            }
            string newpath = Path.Combine(ModulationPath, "emissions_timeseries.txt");
            
            bool timeseries = false;
            if (File.Exists(newpath) == true)
            {
                try
                {
                    //read timeseries of emissions
                    string[] text10 = new string[1];
                    using (StreamReader read1 = new StreamReader(newpath))
                    {
                        //get source group numbers
                        text10 = read1.ReadLine().Split(new char[] { ' ', ':', '-', '\t', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 2; i < text10.Length; i++)
                        {
                            //get the column corresponding with the source group number stored in sg_numbers
                            string sg_temp = text10[i];
                            for (int itm1 = 0; itm1 < MaxSource; itm1++)
                            {
                                if (SGNumbers[itm1] == sg_temp)
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


                        for (int i = 0; i < MettimeFileLength; i++)
                        {
                            text10 = read1.ReadLine().Split(new char[] { ' ', ':', '-', '\t', ';',',' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int n = 0; n < MaxSource; n++)
                            {
                                if (sg_time[n] == 0)
                                {
                                    emifac_timeseries[i, n] = 1;
                                    for (int h = 0; h < 24; h++)
                                    {
                                        for (int m = 0; m < 12; m++)
                                        {
                                            sg_mean_modulation_sum[n] += emifac_mon[m, n] * emifac_day[h, n];
                                            sg_mean_modulation_count[n]++;
                                        }
                                    }
                                }
                                else
                                {
                                    emifac_timeseries[i, n] = Convert.ToDouble(text10[sg_time[n]], ic);
                                    sg_mean_modulation_count[n]++;
                                    sg_mean_modulation_sum[n] += emifac_timeseries[i, n];
                                }
                            }
                        }
                    }

                    AddInfoText(Environment.NewLine + "Reading emissionstimeseries.txt from: " + ModulationPath);
                    for (int n = 0; n < sg_names.Length; n++)
                    {
                        if (sg_time[n] == 0)
                        {
                            AddInfoText(Environment.NewLine + "Mean modulation factor (annual/diurnal factors)  for source group  " + SGNumbers[n].ToString() + " = " + Math.Round(sg_mean_modulation_sum[n] / Math.Max(sg_mean_modulation_count[n], 1), 2));
                        }
                        else
                        {
                            AddInfoText(Environment.NewLine + "Mean modulation factor (emissionstimeseries.txt) for source group " + SGNumbers[n].ToString() + " = " + Math.Round(sg_mean_modulation_sum[n] / Math.Max(sg_mean_modulation_count[n], 1), 2));
                        }
                    }
                    timeseries = true;
                }
                catch (Exception ex)
                {
                    BackgroundThreadMessageBox(ex.Message + " Error reading " + newpath);
                    return null;
                }
            }

            if (!timeseries)
            {
                for (int n = 0; n < MaxSource; n++)
                {
                    double sum = 0;
                    int count = 0;
                    if (!string.IsNullOrEmpty(SGNumbers[n]))
                    {
                        for (int h = 0; h < 24; h++)
                        {
                            for (int m = 0; m < 12; m++)
                            {
                                sum += emifac_mon[m, n] * emifac_day[h, n];
                                count++;
                            }
                        }
                        AddInfoText(Environment.NewLine + "Mean modulation factor (annual/diurnal factors) for source group " + SGNumbers[n].ToString() + " = " + Math.Round(sum / Math.Max(count, 1), 2));
                    }
                }
            }

            return emifac_timeseries;
        }
    }
}