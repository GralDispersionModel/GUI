#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2019-2020]  [Dietmar Oettl, Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using GralMessage;
using GralStaticFunctions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GralDomForms
{
    public partial class MatchMultipleObservations : Form
    {
        /// <summary>
        /// Tune match process
        /// </summary>
        /// <param name="MatchSettings">All setting for one match process</param>
        /// <param name="cts">Cancel token</param>
        /// <returns>List for a matched metTimeSeries, Number of used weather situations</returns>
        private (List<string>, int) MatchTuning(MatchMultipleObservationsData MatchSettings, System.Threading.CancellationToken cts)
        {
            CultureInfo ic = CultureInfo.InvariantCulture;
            Waitprogressbar wait = new Waitprogressbar("");
#if __MonoCS__
            wait.Width = 350;
#endif
            MessageWindow MessageInfoForm = null;
            List<string> synchroErrorList = new List<string>();
            if (!MatchSettings.AutomaticMode)
            {
                MessageInfoForm = new MessageWindow
                {
                    Text = "GRAMM Match to Observation error",
                    ShowInTaskbar = false
                }; // Kuntner
            }

            List<string> metTimeSeries = new List<string>();
            // List for used meteo situations, used if the number of situations should be minimized
            HashSet<int> UsedMeteoSituations = new HashSet<int>();

            int NumberofWeatherSituations = (int)St_F.CountLinesInFile(Path.Combine(GRAMMPath, @"meteopgt.all")) - 2;

            foreach (GralData.PGTAll reset in MatchSettings.PGT) // reset PGT_FRQ
            {
                reset.PGTFrq = -1;
            }

            NumberofWeatherSituations -= 1; // the maximum of weather situations in the original meteopgt.all

            if (!MatchSettings.AutomaticMode)
            {
                wait.Hide();
                wait.Text = "Match to Observation for GRAMM flow fields";
                wait.Show();
            }

            // Create new pointers for the time series synchronization
            if (TimeSeriesPointer == null)
            {
                TimeSeriesPointer = CreateTimeSeriesPointers(MatchSettings);
            }

            object lockObj = new object();
            //avoid unboxing inside the loop and pre-calculate the factor - 0.1 - 1.0
            double ReduceSituationsFactor = 1;
            if (MatchSettings.ReduceSituations > 0)
            {
                ReduceSituationsFactor = Math.Max(0, Math.Min(1, (100 - MatchSettings.ReduceSituations) / 100d));
            }

            try
            {
                string[] dummytext = new string[4];

                //loop over the entire time series
                //__________________________________________________________________________________________________________________
                for (int met_count = 0; met_count < MetFileLenght[0]; met_count++) // Loop over all Weather situations
                {
                    cts.ThrowIfCancellationRequested();

                    if (TimeSeriesPointer[0][met_count] != -1) // otherwise problem with the time stamp of station0
                    {
                        int met_year = 0, met_day = 0, met_month = 0, met_hour = 0;
                        // read date and time from forst element
                        dummytext = DateObsMetFile[0][met_count].Split(new char[] { ' ', ':', '-', '.' });
                        met_year = Convert.ToInt32(dummytext[2]);
                        //in case that the year is given only by two digits the century has to be guessed
                        if (met_year < 100)
                        {
                            if (met_year < 70)
                            {
                                met_year += 2000;
                            }
                            else
                            {
                                met_year += 1900;
                            }
                        }
                        met_day = Convert.ToInt32(dummytext[0]);
                        met_month = Convert.ToInt32(dummytext[1]);
                        met_hour = Convert.ToInt32(HourObsMetFile[0][met_count]);
                        DateTime date_station0 = new DateTime(met_year, met_month, met_day, met_hour, 0, 0);

                        //
                        //double debugwr=0; double debugwsp=0;
                        try
                        {
                            int bestFitSituation = 1; // best fitting weather number
                            double bestFitErrorVal = 100000000; // error of best fitting weather number

                            //search for the GRAMM wind field, which fits best the observed wind data at the observation sites
                            //for (int n = 1; n <= NumberofWeatherSituations; n++) // n = number of calculated GRAMM fields
                            int _cores = Math.Max(1, Environment.ProcessorCount - 2);
                            int range_parallel = (int)Math.Max(10, ((NumberofWeatherSituations + 1) / _cores));
                            range_parallel = Math.Min((NumberofWeatherSituations + 1), range_parallel); // if NumberofWeatherSituations < range_parallel
                            Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(1, (NumberofWeatherSituations + 1), range_parallel), range =>
                            {
                                double u_METEO, v_METEO, wg_METEO, richtung = 0;
                                double err = 10000; double err_min = 1000000;
                                double[] err_st = new double[MetFileNames.Count];
                                double bestFitErrorVal_local = 100000000;
                                int bestFitSituation_local = 1;

                                for (int cmpSit = range.Item1; cmpSit < range.Item2; cmpSit++)
                                {
                                    for (int j = 0; j < MetFileNames.Count; j++) // j = number of actual Met-Station
                                    {
                                        /* TEST OETTL 3.2.17
                                        richtung = (270-Convert.ToDouble(wind_direction[j][counter[j]]))*Math.PI/180;
                                        u_METEO = wind_speeds[j][counter[j]]* Math.Cos(richtung);
                                        v_METEO = wind_speeds[j][counter[j]]* Math.Sin(richtung);
                                        wg_METEO = Math.Sqrt(u_METEO*u_METEO + v_METEO*v_METEO);
                                         */

                                        //comparison is only possible when date and time are similar
                                        int metTimeSeriesPointer = TimeSeriesPointer[j][met_count];
                                        if (metTimeSeriesPointer != -1)
                                        {
                                            richtung = (270 - Convert.ToDouble(WindDirectionObs[j][metTimeSeriesPointer])) * Math.PI / 180;
                                            u_METEO = WindVelocityObs[j][metTimeSeriesPointer] * Math.Cos(richtung);
                                            v_METEO = WindVelocityObs[j][metTimeSeriesPointer] * Math.Sin(richtung);
                                            wg_METEO = Math.Sqrt(u_METEO * u_METEO + v_METEO * v_METEO);

                                            //difference in wind directions between GRAMM and Observations - used as additional weighting factor

                                            double Wind_Dir_Meas = Convert.ToDouble(WindDirectionObs[j][metTimeSeriesPointer]); // Wind direction from Measurement

                                            if (MatchSettings.Optimization == 2) // error using components
                                            {
                                                // error for direction
                                                err = Math.Abs(WindDir[j, cmpSit] - Wind_Dir_Meas);
                                                if (err > 180)
                                                {
                                                    err = 360 - err;
                                                }

                                                err = Math.Abs(err);
                                                err = Math.Pow(Math.Max(0, (err - 12)), 1.8) * MatchSettings.WeightingDirection[j]; // weighting factor
                                                                                                                                    // error for speed - normalized
                                                err += Math.Abs(WGramm[j, cmpSit] - wg_METEO) / Math.Max(0.35, Math.Min(WGramm[j, cmpSit], wg_METEO)) * 100;
                                            }
                                            else if (MatchSettings.Optimization == 1) // error using vectors
                                            {
                                                // error using vectors
                                                err = Math.Sqrt(Math.Pow((UGramm[j, cmpSit] - u_METEO) * 400, 2) +
                                                                Math.Pow((VGramm[j, cmpSit] - v_METEO) * 400, 2));
                                            }

                                            // use weighting factor for met-station-error
                                            if (MatchSettings.WeightingFactor[j] >= 0)
                                            {
                                                err_st[j] = err * MatchSettings.WeightingFactor[j];
                                            }
                                            else
                                            {
                                                err_st[j] = err;
                                            }

                                            //Reduce number of situations -> reduce error, if situation are already used
                                            if (ReduceSituationsFactor < 1)
                                            {
                                                if (UsedMeteoSituations.Contains(cmpSit))
                                                {
                                                    err_st[j] *= ReduceSituationsFactor;
                                                }
                                            }

                                            //	double xxx;
                                            //	error for stability - +- 1 ak = no error - stability error not weighted
                                            if (MatchSettings.StrongerWeightedSC1AndSC7 &&
                                            StabilityClassObs[0][met_count] <= 2 || StabilityClassObs[0][met_count] >= 6) // keep original SC 6 and 7
                                            {
                                                double temp = Math.Abs(LocalStabilityClass[0, cmpSit] - StabilityClassObs[0][met_count]) * 200; // stability
                                                // xxx = temp;
                                                if (MatchSettings.Optimization == 2) // error using components
                                                {
                                                    err_st[j] += temp; // stability
                                                }
                                                else // error using vectors
                                                {
                                                    err_st[j] += Math.Sqrt(err_st[j] * err_st[j] + temp * temp);
                                                }
                                            }
                                            else
                                            {
                                                double temp = Math.Max(0, Math.Abs(LocalStabilityClass[0, cmpSit] - StabilityClassObs[0][met_count]) - 1) * 200; // stability
                                                temp += Math.Abs(LocalStabilityClass[0, cmpSit] - StabilityClassObs[0][met_count]) * 4; // little additional error. that best SCL can be found
                                                                                                                                        //  xxx = temp;
                                                if (MatchSettings.Optimization == 2) // error using components
                                                {
                                                    err_st[j] += temp; // stability
                                                }
                                                else // error using vectors
                                                {
                                                    err_st[j] += Math.Sqrt(err_st[j] * err_st[j] + temp * temp);
                                                }
                                            }
                                            // mess.listBox1.Items.Add(Convert.ToString(LocalStabilityClass[n]) + "/" + Convert.ToString(err_st[j]-xxx) + "/" + Convert.ToString(xxx));
                                            // mess.Show();

                                            err_min = Math.Min(err_min, err_st[j]); // error min
                                        } // synchron fits
                                        else // no synchronity
                                        {
                                            //just 1 times per met_count
                                            if (cmpSit == 1)
                                            {
                                                if (!MatchSettings.AutomaticMode && MatchSettings.WeightingAutoMode[j] > 0.000001)
                                                {
                                                    lock (lockObj)
                                                    {
                                                        synchroErrorList.Add("No date sync with obs. station " + (j + 1).ToString() + " at line " + (met_count + 1).ToString() + " and orig. date " + date_station0.ToString()); // Kuntner
                                                    }
                                                }
                                                err_st[j] = 0;
                                            }
                                        }
                                    } // Number of actual Met-Station

                                    int match_station = 0; // counter, how much stations matched
                                    err = 0;

                                    // find sum error without stations, when auto mode factor = 0
                                    for (int j = 0; j < MetFileNames.Count; j++) // j = number of actual Met-Station
                                    {
                                        if (TimeSeriesPointer[j][met_count] != -1)
                                        {
                                            if (MatchSettings.WeightingAutoMode[j] > 0.000001)
                                            {
                                                // ignore bad stations if remove outliers = true;
                                                if ((MatchSettings.Outliers == false) || ((err_st[j] / Math.Max(0.01, MatchSettings.WeightingFactor[j])) < err_min * 2))
                                                {
                                                    err += err_st[j];
                                                    match_station++;
                                                }
                                            }
                                        }
                                    }

                                    if (match_station > 0)
                                    {
                                        err /= match_station; // normalize to the count of matches stations
                                    }
                                    else
                                    {
                                        err = err_st[0];
                                    }

                                    if (err < bestFitErrorVal_local) // find best fitting local weather situation 
                                    {
                                        bestFitErrorVal_local = err;  // error of best fitting situation
                                        bestFitSituation_local = cmpSit;    // best fitting situation
                                    }
                                } // ParallelForEach() range.item loop

                                if (bestFitErrorVal_local < bestFitErrorVal) // find best fitting total weather situation -> 1st check
                                {
                                    lock (lockObj)
                                    {
                                        if (bestFitErrorVal_local < bestFitErrorVal) // find best fitting weather situation -> locked check
                                        {
                                            bestFitErrorVal = bestFitErrorVal_local;  // error of best fitting situation
                                            bestFitSituation = bestFitSituation_local; // best fitting situation
                                        }
                                    }
                                }
                            }); // Parallel.ForEach loop for all calculated GRAMM - Fields

                            Application.DoEvents();

                            //show the messageinfo form if synchro errors occured
                            if (MessageInfoForm != null && !MatchSettings.AutomaticMode)
                            {
                                try
                                {
                                    if (synchroErrorList.Count > 0)
                                    {
                                        MessageInfoForm.Show();
                                    }
                                    foreach (string _err in synchroErrorList)
                                    {
                                        MessageInfoForm.listBox1.Items.Add(_err);
                                    }
                                }
                                catch { }
                                synchroErrorList.Clear();
                                synchroErrorList.TrimExcess();
                            }
                            // write actual weather-situation of Mettimeseries.dat with the original data from meteopgt.all
                            metTimeSeries.Add(met_day + "." + met_month + "," + met_hour + ","
                                                      + Convert.ToString(WindVelMeteoPGT[bestFitSituation], ic) + "," +
                                                        Convert.ToString(WindDirMeteoPGT[bestFitSituation], ic) + "," +
                                                        Convert.ToString(StabClassMeteoPGT[bestFitSituation], ic));

                            if (MatchSettings.PGT[bestFitSituation - 1].PGTFrq < 0)
                            {
                                MatchSettings.PGT[bestFitSituation - 1].PGTFrq = 0; // marker, that situation is used
                            }

                            MatchSettings.PGT[bestFitSituation - 1].PGTFrq = MatchSettings.PGT[bestFitSituation - 1].PGTFrq + (double)1000d / MetFileLenght[0];
                            MatchSettings.PGT[bestFitSituation - 1].PGTNumber = bestFitSituation; // original number of weather situation

                            //Add bestFitSituation
                            UsedMeteoSituations.Add(bestFitSituation);

                            // Compute error values for the Match-Fine tuning
                            for (int j = 0; j < MetFileNames.Count; j++) // j = number of actual Met-Station
                            {
                                if (TimeSeriesPointer[j][met_count] >= 0)
                                {
                                    double richtung = (270 - Convert.ToDouble(WindDirectionObs[j][TimeSeriesPointer[j][met_count]])) * Math.PI / 180;
                                    double u_METEO = WindVelocityObs[j][TimeSeriesPointer[j][met_count]] * Math.Cos(richtung);
                                    double v_METEO = WindVelocityObs[j][TimeSeriesPointer[j][met_count]] * Math.Sin(richtung);
                                    double wg_METEO = Math.Sqrt(u_METEO * u_METEO + v_METEO * v_METEO);

                                    double err = Math.Sqrt(Math.Pow((UGramm[j, bestFitSituation] - u_METEO), 2) + Math.Pow((VGramm[j, bestFitSituation] - v_METEO), 2));
                                    //double wg_Matched = Math.Sqrt(Math.Pow(UGramm[j, best_fit], 2) + Math.Pow(VGramm[j, best_fit], 2));

                                    //double err = Math.Abs(wg_METEO - wg_Matched);

                                    //evaluation of error values for user feedback
                                    double errp = 0;
                                    if (wg_METEO > 0.5)
                                    {
                                        errp = err / wg_METEO;
                                    }
                                    else
                                    {
                                        errp = err / 0.5;
                                    }

                                    if (errp <= 0.1)
                                    {
                                        MatchSettings.VectorErrorSum[j, 0]++;
                                    }

                                    if (errp <= 0.2)
                                    {
                                        MatchSettings.VectorErrorSum[j, 1]++;
                                    }

                                    if (errp <= 0.4)
                                    {
                                        MatchSettings.VectorErrorSum[j, 2]++;
                                    }

                                    if (errp <= 0.6)
                                    {
                                        MatchSettings.VectorErrorSum[j, 3]++;
                                    }

                                    err = Math.Abs(LocalStabilityClass[j, bestFitSituation] - StabilityClassObs[j][TimeSeriesPointer[j][met_count]]);
                                    if (err == 0)
                                    {
                                        MatchSettings.SCErrorSum[j, 0]++;
                                    }

                                    if (err <= 1)
                                    {
                                        MatchSettings.SCErrorSum[j, 1]++;
                                    }
                                }
                            }
                            //write_debug.WriteLine(Convert.ToString(best_fit)+"/"+Convert.ToString((double) 1000/filelength[0]));
                            //write_debug.WriteLine(Convert.ToString(MatchData.PGT[best_fit-1].PGT_Number)+"/"+Convert.ToString(MatchData.PGT[best_fit-1].PGT_FRQ));
                            //write_debug.WriteLine(met_day + "." + met_month + "," + met_hour + "," + Convert.ToString(Math.Round(debugwsp,1)).Replace(decsep, ".") +"," + Convert.ToString(Math.Round(debugwr,1)).Replace(decsep, "."));
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                if (MessageInfoForm == null && !MatchSettings.AutomaticMode)
                                {
                                    MessageInfoForm = new MessageWindow
                                    {
                                        Text = "GRAMM Match to Observation error",
                                        ShowInTaskbar = false
                                    }; // Kuntner
                                    Application.DoEvents();
                                }
                                if (MessageInfoForm != null && !MatchSettings.AutomaticMode)
                                {
                                    //int linenum = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                                    MessageInfoForm.listBox1.Items.Add(ex.Message + " Error when computing wind deviations Met-File line" + Convert.ToString(met_count)); // + "(" + linenum.ToString() + ")"); // Kuntner
                                    MessageInfoForm.Show();
                                }
                            }
                            catch { }
                        }
                    } // Problem with time stamp of station 0

                    cts.ThrowIfCancellationRequested(); // Cancel procedure?
                } // loop over all weather situations "met_count"
            }
            catch
            {
                metTimeSeries = null;
            }
            //write_debug.Close();

            lockObj = null;
            Match_Mode = 1;                 // tune match process
            if (wait != null)
            {
                wait.Close();
            }
            return (metTimeSeries, UsedMeteoSituations.Count);
        }

        /// <summary>
        /// Create pointers for each time stamp of the 1st meteo station to all other stations
        /// </summary>
        /// <param name="MatchSettings"></param>
        /// <returns></returns>
        private int[][] CreateTimeSeriesPointers(MatchMultipleObservationsData MatchSettings)
        {
            int[][] pointer = GralIO.Landuse.CreateArray<int[]>(MetFileNames.Count, () => new int[MetFileLenght[0]]);
            int[] counter = new int[MetFileNames.Count]; //Points to the recent processed index of the time series
            MessageWindow MessageInfoForm = null;

            //loop over the entire time series
            //__________________________________________________________________________________________________________________
            for (int met_count = 0; met_count < MetFileLenght[0]; met_count++) // Loop over all Weather situations
            {
                DateTime date_station0 = new DateTime(1800, 1, 1, 0, 0, 0);
                DateTime date_stationj = new DateTime(1800, 1, 1, 0, 0, 0);
                int met_year = 0, met_day = 0, met_month = 0, met_hour = 0;
                Application.DoEvents();

                try
                {
                    // read date and time from forst element
                    string[] dummytext = DateObsMetFile[0][met_count].Split(new char[] { ' ', ':', '-', '.' });

                    met_year = Convert.ToInt32(dummytext[2]);
                    //in case that the year is given only by two digits the century has to be guessed
                    if (met_year < 100)
                    {
                        if (met_year < 70)
                        {
                            met_year += 2000;
                        }
                        else
                        {
                            met_year += 1900;
                        }
                    }
                    met_day = Convert.ToInt32(dummytext[0]);
                    met_month = Convert.ToInt32(dummytext[1]);
                    met_hour = Convert.ToInt32(HourObsMetFile[0][met_count]);

                    date_station0 = new DateTime(met_year, met_month, met_day, met_hour, 0, 0);
                    pointer[0][met_count] = met_count;
                }
                catch
                {
                    if (MessageInfoForm == null && !MatchSettings.AutomaticMode)
                    {
                        try
                        {
                            MessageInfoForm = new MessageWindow
                            {
                                Text = "GRAMM Match to Observation error",
                                ShowInTaskbar = false
                            }; // Kuntner
                            Application.DoEvents();
                            MessageInfoForm.listBox1.Items.Add("Can't parse date of obs. station 1 at line " + met_count.ToString()); // Kuntner
                            MessageInfoForm.Show();
                        }
                        catch { }
                    }
                    //set all pointers to -1 = error
                    for (int j = 0; j < MetFileNames.Count; j++)
                    {
                        pointer[j][met_count] = -1; // error
                    }
                }

                if (pointer[0][met_count] != -1) // otherwise problem with the time stamp of station0
                {
                    //synchronizing date and time of all meteo-stations
                    for (int j = 1; j < MetFileNames.Count; j++) // loop over all Met-stations
                    {
                        //get year, month, and day regardless the separator character
                        pointer[j][met_count] = -1; // set pointer to error by default

                        for (int timestep = counter[j]; timestep < MetFileLenght[j]; timestep++)
                        {
                            int comp = -1;
                            try
                            {
                                string[] dummytext = DateObsMetFile[j][timestep].Split(new char[] { ' ', ':', '-', '.' });
                                int met_yearj = Convert.ToInt32(dummytext[2]);
                                //in case that the year is given only by two digits the century has to be guessed
                                if (met_yearj < 100)
                                {
                                    if (met_yearj < 70)
                                    {
                                        met_yearj += 2000;
                                    }
                                    else
                                    {
                                        met_yearj += 1900;
                                    }
                                }
                                int met_dayj = Convert.ToInt32(dummytext[0]);
                                int met_monthj = Convert.ToInt32(dummytext[1]);
                                int met_hourj = Convert.ToInt32(HourObsMetFile[j][timestep]);

                                date_stationj = new DateTime(met_yearj, met_monthj, met_dayj, met_hourj, 0, 0);
                                comp = DateTime.Compare(date_stationj, date_station0);
                                //MessageBox.Show(date_station0 + "/" + date_stationj);
                            }
                            catch
                            {
                                try
                                {
                                    if (MessageInfoForm == null && !MatchSettings.AutomaticMode)
                                    {
                                        MessageInfoForm = new MessageWindow
                                        {
                                            Text = "GRAMM Match to Observation error",
                                            ShowInTaskbar = false
                                        }; // Kuntner
                                        Application.DoEvents();

                                        MessageInfoForm.listBox1.Items.Add("Can't parse date of obs. station " + (j + 1).ToString() + " at line " + timestep.ToString()); // Kuntner
                                        MessageInfoForm.Show();
                                    }
                                }
                                catch { }
                                comp = -1;
                            }

                            if (comp < 0) // t1 is earlier than t2 -> increase t1, do nothing else
                            {
                            }
                            else if (comp > 0) // t1 is later than t2 -> interrupt for-next loop
                            {
                                pointer[j][met_count] = -1; // set pointer to error by default
                                timestep = MetFileLenght[j];   //break loop
                            }
                            else if (comp == 0) // found same time stamp
                            {
                                pointer[j][met_count] = timestep; // set pointer to the synchronized situation
                                counter[j] = timestep;
                                timestep = MetFileLenght[j]; //break loop
                            }
                        } // time loop for each met station
                    } //loop over met stations
                }
            }
            return pointer;
        }
    }
}