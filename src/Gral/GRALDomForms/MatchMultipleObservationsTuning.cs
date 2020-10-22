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
using System.Windows.Forms;
using System.IO;
using GralMessage;
using GralStaticFunctions;
using System.Globalization;
using System.Collections.Generic;

namespace GralDomForms
{
    public partial class MatchMultipleObservations : Form
    {
        /// <summary>
        /// Tune match process
        /// </summary>
        private List<string> MatchTuning(MatchMultipleObservationsData MatchSettings)
        {

            Waitprogressbar wait = new Waitprogressbar(""); 
#if __MonoCS__
            wait.Width = 350;
#endif
            MessageWindow MessageInfoForm = null;
            CultureInfo ic = CultureInfo.InvariantCulture;

            double u_METEO;
            double v_METEO;
            double wg_METEO = 0;
            double richtung = 0;

            List<string> metTimeSeries = new List<string>();
            long NumberofWeatherSituations = St_F.CountLinesInFile(Path.Combine(GRAMMPath, @"meteopgt.all")) - 2;

            foreach (GralData.PGTAll reset in MatchSettings.PGT) // reset PGT_FRQ
            {
                reset.PGTFrq = -1;
            }

            NumberofWeatherSituations -= 1; // the maximum of weather situations in the original meteopgt.all

            if (!MatchSettings.AutomaticMode)
            {
                wait.Hide();
                wait.Text = "Matching GRAMM flow fields";
                wait.Show();
            }

            int[] counter = new int[metfiles.Count]; //zeitzeiger fuer die zeitreihen der einzelnen stationen
            Array.Clear(counter, 0, metfiles.Count);
            bool[] synchron = new bool[metfiles.Count];
            string[] dummytext = new string[4];

            //newPath = Path.Combine(form1.projectname,@"Computation\debug.txt");
            //StreamWriter write_debug = File.CreateText(newPath);

            //loop over the entire time series
            //__________________________________________________________________________________________________________________
            for (int met_count = 0; met_count < filelength[0]; met_count++) // Loop over all Weather situations
            {
                counter[0] = met_count;
                synchron[0] = true;
                DateTime date_station0 = new DateTime(1800, 1, 1, 0, 0, 0);
                DateTime date_stationj = new DateTime(1800, 1, 1, 0, 0, 0);
                int met_year = 0, met_day = 0, met_month = 0, met_hour = 0;

                try
                {
                    // read date and time from forst element
                    dummytext = datum[0][met_count].Split(new char[] { ' ', ':', '-', '.' });

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
                    met_hour = Convert.ToInt32(stunde[0][met_count]);

                    date_station0 = new DateTime(met_year, met_month, met_day, met_hour, 0, 0);
                }
                catch
                {
                    if (MessageInfoForm == null && !MatchSettings.AutomaticMode)
                    {
                        MessageInfoForm = new MessageWindow
                        {
                            Text = "GRAMM matching error",
                            ShowInTaskbar = false
                        }; // Kuntner
                        Application.DoEvents();
                        MessageInfoForm.listBox1.Items.Add("Can't parse date of obs. station 1 at line " + met_count.ToString()); // Kuntner
                        MessageInfoForm.Show();
                    }
                    synchron[0] = false;
                }

                if (synchron[0] == true) // otherwise problem with the time stamp of station0
                {
                    //synchronizing date and time of all meteo-stations
                    for (int j = 1; j < metfiles.Count; j++) // loop over all Met-stations
                    {

                        //get year, month, and day regardless the separator character
                        synchron[j] = false;

                        for (int timestep = counter[j]; timestep < filelength[j]; timestep++)
                        {
                            int comp = -1;
                            try
                            {
                                dummytext = datum[j][timestep].Split(new char[] { ' ', ':', '-', '.' });
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
                                int met_hourj = Convert.ToInt32(stunde[j][timestep]);

                                date_stationj = new DateTime(met_yearj, met_monthj, met_dayj, met_hourj, 0, 0);
                                comp = DateTime.Compare(date_stationj, date_station0);
                                //MessageBox.Show(date_station0 + "/" + date_stationj);
                            }
                            catch
                            {
                                if (MessageInfoForm == null && !MatchSettings.AutomaticMode)
                                {
                                    MessageInfoForm = new MessageWindow
                                    {
                                        Text = "GRAMM matching error",
                                        ShowInTaskbar = false
                                    }; // Kuntner
                                    Application.DoEvents();

                                    MessageInfoForm.listBox1.Items.Add("Can't parse date of obs. station " + (j + 1).ToString() + " at line " + timestep.ToString()); // Kuntner
                                    MessageInfoForm.Show();
                                }

                                comp = -1;
                            }

                            if (comp < 0) // t1 is earlier than t2 -> increase t1, do nothing else
                            {
                                //counter[j] = timestep;
                                //MessageInfoForm.listBox1.Items.Add(date_station0.ToString() + "waiting...." + date_stationj.ToString());
                            }
                            else if (comp > 0) // t1 is later than t2 -> interrupt for-next loop
                            {
                                synchron[j] = false;
                                timestep = filelength[j]; //break loop
                                                              //mess.listBox1.Items.Add("Wating for sync ");
                                                              //MessageInfoForm.listBox1.Items.Add(date_station0.ToString() + "waiting...." + date_stationj.ToString());
                            }
                            else if (comp == 0) // found same time stamp
                            {
                                synchron[j] = true;
                                counter[j] = timestep;
                                timestep = filelength[j]; //break loop
                                                              //mess.listBox1.Items.Add("Date sync at station " + j.ToString() + " and line " + counter[j].ToString());
                            }
                        } // time loop for each met station
                    } //loop over met stations

                    //
                    //double debugwr=0; double debugwsp=0;
                    try
                    {
                        int best_fit = 1; // best fitting weather number
                        double best_err = 100000000; // error of best fitting weather number

                        //search for the GRAMM wind field, which fits best the observed wind data at the observation sites
                        for (int n = 1; n <= NumberofWeatherSituations; n++) // n = number of calculated GRAMM fields
                        {                               //MatchData.Optimization criterion: wind vector + weighted wind direction + weighted wind speed
                            double err = 10000; double err_min = 1000000;
                            double[] err_st = new double[metfiles.Count];

                            for (int j = 0; j < metfiles.Count; j++) // j = number of actual Met-Station
                            {
                                /* TEST OETTL 3.2.17
                                richtung = (270-Convert.ToDouble(wind_direction[j][counter[j]]))*Math.PI/180;
                                u_METEO = wind_speeds[j][counter[j]]* Math.Cos(richtung);
                                v_METEO = wind_speeds[j][counter[j]]* Math.Sin(richtung);
                                wg_METEO = Math.Sqrt(u_METEO*u_METEO + v_METEO*v_METEO);
                                 */

                                //comparison is only possible when date and time are similar
                                if (synchron[j] == true)
                                {
                                    richtung = (270 - Convert.ToDouble(wind_direction[j][counter[j]])) * Math.PI / 180;
                                    u_METEO = wind_speeds[j][counter[j]] * Math.Cos(richtung);
                                    v_METEO = wind_speeds[j][counter[j]] * Math.Sin(richtung);
                                    wg_METEO = Math.Sqrt(u_METEO * u_METEO + v_METEO * v_METEO);

                                    //difference in wind directions between GRAMM and Observations - used as additional weighting factor

                                    double Wind_Dir_Meas = Convert.ToDouble(wind_direction[j][counter[j]]); // Wind direction from Measurement

                                    if (MatchSettings.Optimization == 2) // error using components
                                    {
                                        // error for direction
                                        err = Math.Abs(WindDir[j, n] - Wind_Dir_Meas);
                                        if (err > 180)
                                        {
                                            err = 360 - err;
                                        }

                                        err = Math.Abs(err);
                                        err = Math.Pow(Math.Max(0, (err - 12)), 1.8) * MatchSettings.WeightingDirection[j]; // weighting factor
                                                                                                                  // error for speed - normalized
                                        err += Math.Abs(WGramm[j, n] - wg_METEO) / Math.Max(0.35, Math.Min(WGramm[j, n], wg_METEO)) * 100;
                                    }
                                    else if (MatchSettings.Optimization == 1) // error using vectors
                                    {
                                        // error using vectors
                                        err = Math.Sqrt(Math.Pow((UGramm[j, n] - u_METEO) * 400, 2) +
                                                        Math.Pow((VGramm[j, n] - v_METEO) * 400, 2));
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

                                    //	double xxx;
                                    //	error for stability - +- 1 ak = no error - stability error not weighted
                                    if (MatchSettings.StrongerWeightedSC1AndSC7 &&
                                        stability[0][met_count] <= 2 || stability[0][met_count] >= 6) // keep original SC 6 and 7
                                    {
                                        double temp = Math.Abs(LocalStabilityClass[0, n] - stability[0][met_count]) * 200; // stability
                                                                                                                                   //											    xxx = temp;
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
                                        double temp = Math.Max(0, Math.Abs(LocalStabilityClass[0, n] - stability[0][met_count]) - 1) * 200; // stability
                                        temp += Math.Abs(LocalStabilityClass[0, n] - stability[0][met_count]) * 4; // little additional error. that best SCL can be found
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
                                    if (n == 1)
                                    {
                                        if (MessageInfoForm == null && !MatchSettings.AutomaticMode)
                                        {
                                            MessageInfoForm = new MessageWindow
                                            {
                                                Text = "GRAMM matching error",
                                                ShowInTaskbar = false
                                            }; // Kuntner

                                            MessageInfoForm.listBox1.Items.Add("No date sync with obs. station " + (j + 1).ToString() + " at line " + counter[j].ToString() + " and orig. date " + date_station0.ToString()); // Kuntner
                                            MessageInfoForm.Show();
                                        }
                                        err_st[j] = 0;
                                    }
                                }
                            } // Number of actual Met-Station

                            int match_station = 0; // counter,how much stations did match
                            err = 0;
                            // find sum error without bad met-stations
                            for (int j = 0; j < metfiles.Count; j++) // j = number of actual Met-Station
                            {
                                if (synchron[j] == true)
                                {
                                    if ((MatchSettings.Outliers == false) || ((err_st[j] / Math.Max(0.01, MatchSettings.WeightingFactor[j])) < err_min * 2)) // don´t use bad stations if outliers = true;
                                    {
                                        err += err_st[j];
                                        match_station++;
                                    }
                                }
                            }

                            if (match_station > 0)
                            {
                                err /= match_station; // norm to the count of matches stations
                            }
                            else
                            {
                                err = err_st[0];
                            }

                            if (err < best_err) // find best fitting weather situation
                            {
                                best_err = err;  // error of best fitting situation
                                best_fit = n;    // best fitting situation
                                                 //debugwr = WindDir[0, n];
                                                 //debugwsp = WGramm[0,n];
                            }
                        } // loop over calculated GRAMM - Fields
                       
                        Application.DoEvents();
                        // write actual weather-situation of Mettimeseries.dat with the original data from meteopgt.all
                        metTimeSeries.Add(met_day + "." + met_month + "," + met_hour + ","
                                                      + Convert.ToString(WindVelMeteoPGT[best_fit], ic) + "," +
                                                      Convert.ToString(WindDirMeteoPGT[best_fit], ic) + "," +
                                                      Convert.ToString(StabClassMeteoPGT[best_fit], ic));

                        if (MatchSettings.PGT[best_fit - 1].PGTFrq < 0)
                        {
                            MatchSettings.PGT[best_fit - 1].PGTFrq = 0; // marker, that situation is used
                        }

                        MatchSettings.PGT[best_fit - 1].PGTFrq = MatchSettings.PGT[best_fit - 1].PGTFrq + (double)1000d / filelength[0];
                        MatchSettings.PGT[best_fit - 1].PGTNumber = best_fit; // original number of weather situation

                        // Compute error values for the Match-Fine tuning
                        for (int j = 0; j < metfiles.Count; j++) // j = number of actual Met-Station
                        {
                            richtung = (270 - Convert.ToDouble(wind_direction[j][counter[j]])) * Math.PI / 180;
                            u_METEO = wind_speeds[j][counter[j]] * Math.Cos(richtung);
                            v_METEO = wind_speeds[j][counter[j]] * Math.Sin(richtung);
                            wg_METEO = Math.Sqrt(u_METEO * u_METEO + v_METEO * v_METEO);

                            double err = Math.Sqrt(Math.Pow((UGramm[j, best_fit] - u_METEO), 2) + Math.Pow((VGramm[j, best_fit] - v_METEO), 2));
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

                            err = Math.Abs(LocalStabilityClass[j, best_fit] - stability[j][met_count]);
                            if (err == 0)
                            {
                                MatchSettings.SCErrorSum[j, 0]++;
                            }

                            if (err <= 1)
                            {
                                MatchSettings.SCErrorSum[j, 1]++;
                            }
                        }
                        //write_debug.WriteLine(Convert.ToString(best_fit)+"/"+Convert.ToString((double) 1000/filelength[0]));
                        //write_debug.WriteLine(Convert.ToString(MatchData.PGT[best_fit-1].PGT_Number)+"/"+Convert.ToString(MatchData.PGT[best_fit-1].PGT_FRQ));
                        //write_debug.WriteLine(met_day + "." + met_month + "," + met_hour + "," + Convert.ToString(Math.Round(debugwsp,1)).Replace(decsep, ".") +"," + Convert.ToString(Math.Round(debugwr,1)).Replace(decsep, "."));
                    }
                    catch(Exception ex)
                    {
                        if (MessageInfoForm == null && !MatchSettings.AutomaticMode)
                        {
                            MessageInfoForm = new MessageWindow
                            {
                                Text = "GRAMM matching error",
                                ShowInTaskbar = false
                            }; // Kuntner
                            Application.DoEvents();

                            MessageInfoForm.listBox1.Items.Add(ex.Message + " Error when computing wind deviations Met-File line" + Convert.ToString(met_count)); // Kuntner
                            MessageInfoForm.Show();
                        }
                    }

                } // Problem with time stamp of station 0
            } // loop over all weather situations "met_count"
            //write_debug.Close();

            Match_Mode = 1;                 // tune match process
            
            wait.Close();
            return metTimeSeries;
        }
    }
}