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
using System.Linq;

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
            for (int k = 0; k < 15; k++) text[k] = "";

            string dummy = string.Empty;
            string newpath = "";
            int[] sg_numbers = new int[maxsource];
            string[] sg_names = mydata.Sel_Source_Grp.Split(',');
            string[] computed_sourcegroups = mydata.Comp_Source_Grp.Split(',');

            //in transient GRAL mode, it is simply to read the File GRAL_meteozeitreihe.dat and convert it to .met files
            bool transient = false;
            InDatVariables data = new InDatVariables();
            InDatFileIO ReadInData = new InDatFileIO();
            data.InDatPath = Path.Combine(mydata.Projectname, "Computation", "in.dat");
            ReadInData.Data = data;
            if (ReadInData.ReadInDat() == true)
            {
                if (data.Transientflag == 0)
                {
                    transient = true;
                }
            }

            //get variation for source group
            if (!string.IsNullOrEmpty(mydata.Sel_Source_Grp)) // otherwise just analyze the wind data
            {
                // Read the emission factors of all selected source-groups
                for (int itm = 0; itm < maxsource; itm++)
                {
                    try
                    {
                        for (int sg = 0; sg < sg_names.Length; sg++) // check all selected source groups
                        {
                            if ((itm + 1) == Convert.ToInt32(GetSgNumbers(sg_names[sg]))) // sourcegroup selected?
                            {
                                sg_numbers[itm] = Convert.ToInt32(GetSgNumbers(sg_names[sg])); // Get number of the Source-group
                                // MessageBox.Show(itm.ToString()+"/"+sg_numbers[itm]);
                                // Read modulation of that source-group
                                newpath = Path.Combine("Computation", "emissions" + Convert.ToString(itm + 1).PadLeft(3, '0') + ".dat");
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
                    catch (Exception ex)
                    {
                        BackgroundThreadMessageBox(ex.Message);
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
                BackgroundThreadMessageBox("Error reading mettimeseries.txt");
                return; // leave method
            }
            //mettimefilelength--;

            //if file emissions_timeseries.txt exists, these modulation factors will be used
            int[] sg_time = new int[maxsource];
            double[,] emifac_timeseries = new double[mettimefilelength + 1, maxsource];

            if (!string.IsNullOrEmpty(mydata.Sel_Source_Grp)) // otherwise just analyze the wind data
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
                    catch (Exception ex)
                    {
                        BackgroundThreadMessageBox(ex.Message + " Error reading emissions_timeseries.txt - evaluation stopped");
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

            List<string> rec_names = new List<string>();

            //get number of receptor points and names of receptors
            string receptorfile = Path.Combine(mydata.Projectname, "Computation", "Receptor.dat");
            if (File.Exists(receptorfile))
            {
                try
                {
                    using (StreamReader read = new StreamReader(receptorfile))
                    {
                        dummy = read.ReadLine(); // 1st line with number of receptor points

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
                                    for (int k = 5; k < text.Length; k++) // make it possible, that "," is part of the Receptor point name
                                    {
                                        if (text[k] != "") a = a + "_" + text[k];
                                    }
                                }

                                // if the receptor name contains invalid characters for a file!
                                a = string.Join("_", a.Split(Path.GetInvalidFileNameChars()));
                                rec_names.Add(a);
                            }
                        }
                    }
                }
                catch
                {
                    BackgroundThreadMessageBox("Error reading Receptor.dat");
                    return;
                }

            }

            //MessageBox.Show(Convert.ToString(rec_names.Count));

            //read meteopgt.all
            newpath = Path.Combine("Computation", "meteopgt.all");
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
                        BackgroundThreadMessageBox("Error reading meteopgt.all");
                        return;
                    }
                }
            }

            //BackgroundThreadMessageBox(wrmet[0] + "/" + wgmet[0] + "/" + akmet[0]);
            double[][][] conc = GralIO.Landuse.CreateArray<double[][]>(xrec.Count, () 
                                                        => GralIO.Landuse.CreateArray<double[]>(maxsource, () 
                                                                                 => new double[wrmet.Count]));
            double fmod = 1;

            //read all computed concentrations from file zeitreihe.dat or ReceptorConcentration.dat
            bool writeConcentrationFiles = false;
            string[] text5 = new string[xrec.Count * sg_names.Length];
            //number of existing weather situations
            int existingWeatherSituations = 0;

            //switch between new (V21.01) and old GRAL concentration files
            receptorfile = Path.Combine(mydata.Projectname, "Computation", "ReceptorConcentrations.dat");
            bool NewFileFormat = false;
            if (File.Exists(receptorfile))
            {
                NewFileFormat = true;
            }
            else
            {
                receptorfile = Path.Combine(mydata.Projectname, "Computation", "zeitreihe.dat");
            }

            // Read concentration file into conc[][][]
            string[] ConcentrationHeader = new string[6];
            int NumberOfReceptors = xrec.Count;

            if (File.Exists(receptorfile) && mydata.Sel_Source_Grp != string.Empty)
            {
                writeConcentrationFiles = true;
                try
                {
                    using (StreamReader read = new StreamReader(receptorfile))
                    {
                        // Read header of new file format
                        if (NewFileFormat)
                        {
                            for (int ianz = 0; ianz < 6; ianz++)
                            {
                                ConcentrationHeader[ianz] = read.ReadLine();
                            }

                            if (sg_names.Count() > 0)
                            {
                                NumberOfReceptors = (int)ConcentrationHeader[0].Count(ch => ch == '\t') / sg_names.Count();

                                //check all source groups within the file ReceptorConcentrations.dat
                                List<int> containedSourceGroups = new List<int>();
                                string[] headerSourceGroups = ConcentrationHeader[1].Split(new char[] {'\t'});
                                for (int i = 0; i < headerSourceGroups.Length; i++)
                                {
                                    int _sg = 0;
                                    if (Int32.TryParse(headerSourceGroups[i], out _sg))
                                    {
                                        if (!containedSourceGroups.Contains(_sg))
                                        {
                                            containedSourceGroups.Add(_sg);
                                        }
                                    }
                                }
                                if (containedSourceGroups.Count() != sg_names.Count())
                                {
                                    BackgroundThreadMessageBox("The number of source groups between calculation and current project does not match!");
                                }

                                //if the project has been changed - who knows what user are doing...
                                if (NumberOfReceptors > xrec.Count)
                                {
                                    conc = GralIO.Landuse.CreateArray<double[][]>(NumberOfReceptors, ()
                                                            => GralIO.Landuse.CreateArray<double[]>(maxsource, ()
                                                                                     => new double[wrmet.Count]));
                                }
                            }
                        }

                        // read concentration file
                        existingWeatherSituations = 0;
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
                                    //check if this situation has been computed, otherwise this line is 0
                                    if (text5.Length > count)
                                    {
                                        conc[numbrec][sg_number][existingWeatherSituations] = Convert.ToDouble(text5[count].Replace(".", decsep));
                                    }
                                    count++;
                                }
                            }

                            existingWeatherSituations++; // use weather situations from ReceptorConcentrations.dat
                        }
                    }
                }
                catch
                {
                    BackgroundThreadMessageBox ("Error reading" + Path.GetFileName(receptorfile) +  "Is this file available?" + Environment.NewLine +
                                                "Has the number of receptors or source groups changed?");
                    return;
                }			
            }
            
            //read all wind speeds from file GRAL_Meteozeitreihe.dat
            bool meteoDataAvailable = false;
            bool local_SCL = false;
            string[] text7 = new string[xrec.Count];
            
            string GRAL_metfile = Path.Combine(mydata.Projectname, "Computation","GRAL_Meteozeitreihe.dat");
            int zeitreihe_lenght = (int) GralStaticFunctions.St_F.CountLinesInFile(GRAL_metfile);
            
            double[,] GRAL_u = new double[NumberOfReceptors, Math.Max(zeitreihe_lenght, wrmet.Count) + 1];
            double[,] GRAL_v = new double[NumberOfReceptors, Math.Max(zeitreihe_lenght, wrmet.Count) + 1];
            int[,] GRAL_SC = new int[NumberOfReceptors, Math.Max(zeitreihe_lenght, wrmet.Count) + 1];
            string[] ReceptorMeteoCoors = new string[5];
            
            if (File.Exists(GRAL_metfile))
            {
                meteoDataAvailable = true;
                int weatherSit = 0;         
                if (Read_Meteo_Zeitreihe(GRAL_metfile, ref weatherSit, ref local_SCL, ref GRAL_u, ref GRAL_v, ref GRAL_SC, ref ReceptorMeteoCoors) == false)
                {
                    meteoDataAvailable = false;
                }
                else
                {
                    existingWeatherSituations = weatherSit; // use number of weather situations from MeteoTimeseries
                }
            }

            //BackgroundThreadMessageBox(numbwet.ToString());

            if (!string.IsNullOrEmpty(mydata.Sel_Source_Grp)) // otherwise just analyze the wind data
            {
                if (writeConcentrationFiles == true) // write mettime series for all receptor points
                {
                    try
                    {
                        string writerRecTimeSeries = Path.Combine(mydata.Projectname, "Computation","Receptor_TimeSeries_"+ mydata.Prefix + mydata.Pollutant + ".txt");
                        if (File.Exists(writerRecTimeSeries))
                        {
                            try
                            {
                                File.Delete(writerRecTimeSeries);
                            }
                            catch{}
                        }

                        //write results to file ReceptorTimeSeries.txt in Unicode encoding
                        using (StreamWriter recwrite = new StreamWriter(writerRecTimeSeries, false, System.Text.Encoding.Unicode))
                        {
                            //write header line
                            if (NewFileFormat)
                            {
                                string[] headerInfo = {"Name", "Source group", "X", "Y", "Z", "-----"};

                                System.Globalization.CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentCulture;

                                for (int ianz = 0; ianz < 6; ianz++)
                                {
                                    // use local culture for user files
                                    if (ianz > 0)
                                    {
                                        ConcentrationHeader[ianz] = ConcentrationHeader[ianz].Replace(".", ci.NumberFormat.CurrencyDecimalSeparator);
                                    }
                                    recwrite.WriteLine("\t" + headerInfo[ianz] + "\t" + ConcentrationHeader[ianz]);
                                }
                            }

                            //write source groups
                            string[] text6 = new string[2];
                            recwrite.Write("Date \t Time \t"); // Header Date, Time

                            foreach (string hy in sg_names)
                            {
                                for (int numbrec = 0; numbrec < NumberOfReceptors; numbrec++)
                                {
                                    text6 = hy.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                    recwrite.Write(text6[0] + "-Rec:" + Convert.ToString(numbrec + 1) + "\t"); // Header source-groups use Tabulator
                                }
                            }
                            recwrite.WriteLine();
                            
                            //read mettimeseries.dat
                            newpath = Path.Combine("Computation", "mettimeseries.dat");
                            using (StreamReader readMetTimeSeries = new StreamReader(Path.Combine(mydata.Projectname, newpath)))
                            {
                                text2 = readMetTimeSeries.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);

                                //consider, if meteopgt.all represents a time series or a statistics
                                int dispersionsituations = 0;
                                if (mydata.Checkbox19 == true)
                                    dispersionsituations = existingWeatherSituations + 1;
                                else
                                    dispersionsituations = wrmet.Count;

                                int count_dispsit_in_mettime = 0;
                                int count_ws = -1;

                                while (!string.IsNullOrEmpty(text2[0]))
                                {
                                    count_ws++;
                                    count_dispsit_in_mettime += 1;

                                    if ((count_dispsit_in_mettime >= existingWeatherSituations) && (mydata.Checkbox19 == true))
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
                                        if ((wgmet[n].Equals(wgmettime)) && (wrmet[n].Equals(wrmettime)) && (akmet[n].Equals(akmettime)))
                                        {
                                            //take care if not all dispersion situations have been computed
                                            if (n >= existingWeatherSituations)
                                            {
                                                //write results
                                                recwrite.WriteLine(string.Empty);
                                            }
                                            else
                                            {
                                                SetText("Day.Month: " + day + "." + month);

                                                int std = Convert.ToInt32(hour);
                                                int mon = Convert.ToInt32(month) - 1;

                                                //write results
                                                dummy = day + "." + month + "\t" + hour + ":00\t";
                                                fmod = 1;
                                                foreach (string hy in sg_names)
                                                {
                                                    {
                                                        int itm = Convert.ToInt32(GetSgNumbers(hy)) - 1;

                                                        //compute emission modulation factor
                                                        fmod = emifac_day[std - hourplus, itm] * emifac_mon[mon, itm] * emifac_timeseries[count_ws, itm];

                                                        for (int numbrec = 0; numbrec < NumberOfReceptors; numbrec++)
                                                        {
                                                            dummy = dummy + Convert.ToString(conc[numbrec][itm][n] * fmod) + "\t";
                                                        }
                                                    }
                                                }
                                                recwrite.WriteLine(dummy);
                                                //BackgroundThreadMessageBox(dummy);

                                                //consider, if meteopgt.all represents a time series or a statistics
                                                if (mydata.Checkbox19 == true)
                                                    break;
                                            }
                                            n = dispersionsituations; // situation found -> stop searching
                                        }
                                    }
                                    try
                                    {
                                        if (readMetTimeSeries.EndOfStream)
                                        {
                                            text2[0] = string.Empty; //quit reading
                                        }
                                        else
                                        {
                                            text2 = readMetTimeSeries.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                            text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
                                        }
                                    }
                                    catch
                                    {
                                        break;
                                    }
                                }
                            } // using reader of mettimeseries
                            
                        } // using rec write
                    }
                    catch
                    {
                        BackgroundThreadMessageBox ("Error writing the file Receptor_TimeSeries.txt");
                        return;
                    }
                }
                else
                {
                    BackgroundThreadMessageBox ("File zeitreihe.dat not found");
                }
            }
            
            if (meteoDataAvailable == true) // write meteorological data for all receptor points
            {
                try
                {
                    string[] recname = Array.Empty<string>();
                    string[] recX = Array.Empty<string>();
                    string[] recY = Array.Empty<string>();
                    string[] recZ = Array.Empty<string>();
                   
                    if (!string.IsNullOrEmpty(ReceptorMeteoCoors[0]))
                    {
                        recname = ReceptorMeteoCoors[0].Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        recX = ReceptorMeteoCoors[1].Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        recY = ReceptorMeteoCoors[2].Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        recZ = ReceptorMeteoCoors[3].Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    }

                    for (int k = 0; k < rec_names.Count; k++)
                    {
                        //write results to file(s) GRAL_*metstations*.met
                        double windspeed_GRAL = 0;
                        double winddirection_GRAL = 0;

                        string meteoFile = Path.Combine(mydata.Projectname, @"Metfiles", "GRAL" + Convert.ToString(k + 1) + "_" + rec_names[k] + ".met");
                        if (File.Exists(meteoFile))
                        {
                            try
                            {
                                File.Delete(meteoFile);
                            }
                            catch { }
                        }

                        using (StreamWriter meteoWriter = new StreamWriter(meteoFile))
                        {
                            //write header lines
                            if (string.IsNullOrEmpty(ReceptorMeteoCoors[0]) || k >= recname.Length)
                            {
                                meteoWriter.WriteLine("//" + Path.GetFileName(meteoFile));
                            }
                            else
                            {
                                try
                                {
                                    meteoWriter.WriteLine(recname[k]);
                                    meteoWriter.WriteLine(@"\\X=" + recX[k]);
                                    meteoWriter.WriteLine(@"\\Y=" + recY[k]);
                                    meteoWriter.WriteLine(@"\\Z=" + recZ[k]);
                                }
                                catch { }
                            }

                            string[] text6 = new string[2];
                            
                            //read mettimeseries.dat
                            using (StreamReader read = new StreamReader(Path.Combine(mydata.Projectname, "Computation", "mettimeseries.dat")))
                            {
                                text2 = read.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
                                string dummy_year = "1990";
                                int year_increase=0;
                                int monthold = -1;

                                //consider, if meteopgt.all represents a time series or a statistics
                                int dispersionsituations = 0;
                                if (mydata.Checkbox19 == true || transient == true)
                                    dispersionsituations = existingWeatherSituations + 1;
                                else
                                    dispersionsituations = wrmet.Count;
                                
                                int count_dispsit_in_mettime = -1;

                                while (!string.IsNullOrEmpty(text2[0]))
                                {
                                    count_dispsit_in_mettime++;
                                    
                                    if ((count_dispsit_in_mettime >= existingWeatherSituations) && (mydata.Checkbox19 == true || transient == true))
                                    {
                                        break;
                                    }

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

                                    int corr_situation = -2;
                                    if (transient == false) // non - transient mode: search for corr. situation in meteopgt.all
                                    {
                                        //search in file meteopgt.all for the corresponding dispersion situation
                                        for (int n = 0; n < dispersionsituations; n++)
                                        {
                                            if ((wgmet[n].Equals(wgmettime)) && (wrmet[n].Equals(wrmettime)) && (akmet[n].Equals(akmettime)))
                                            {
                                                //take care if not all dispersion situations have been computed
                                                if (n >= existingWeatherSituations)
                                                {
                                                    //write results
                                                    //recwrite.WriteLine(string.Empty);
                                                    corr_situation = -3;
                                                    n = dispersionsituations;
                                                }
                                                else
                                                {
                                                    corr_situation = n;
                                                    n = dispersionsituations; // // situation found -> stop searching
                                                }
                                            }
                                        }
                                    }
                                    else // transient mode: situation = line number of mettimeseries 
                                    {
                                        corr_situation = count_dispsit_in_mettime;
                                    }

                                    if (corr_situation > -1)
                                    {
                                        int n  = corr_situation;
                                        SetText("Day.Month: " + day + "." + month);
                                        
                                        //write results
                                        dummy_year = Convert.ToString(1990 + year_increase);
                                        dummy = day + "." + month + "." + dummy_year + "," + hour + ":00,";
                                        
                                        //compute wind speed and direction
                                        windspeed_GRAL = Math.Round(Math.Pow(Math.Pow(GRAL_u[k,n], 2) + Math.Pow(GRAL_v[k,n], 2), 0.5),2);

                                        if (windspeed_GRAL == 0) // wind speed 0 => this sit was not calculated
                                        {
                                            winddirection_GRAL = 0;
                                            dummy += "0,0,4";
                                        }
                                        else
                                        {
                                            if (GRAL_v[k, n] == 0)
                                                winddirection_GRAL = 90;
                                            else
                                                winddirection_GRAL = Convert.ToInt32(Math.Abs(Math.Atan(GRAL_u[k, n] / GRAL_v[k, n])) * 180 / Math.PI);

                                            if ((GRAL_v[k, n] > 0) && (GRAL_u[k, n] <= 0))
                                                winddirection_GRAL = 180 - winddirection_GRAL;
                                            if ((GRAL_v[k, n] >= 0) && (GRAL_u[k, n] > 0))
                                                winddirection_GRAL = 180 + winddirection_GRAL;
                                            if ((GRAL_v[k, n] < 0) && (GRAL_u[k, n] >= 0))
                                                winddirection_GRAL = 360 - winddirection_GRAL;

                                            dummy += Convert.ToString(windspeed_GRAL, ic) + "," + Convert.ToString(Math.Round(winddirection_GRAL, 0), ic);

                                            if (local_SCL == true) // 11.9.2017 Kuntner -> new File format?
                                            {
                                                if (GRAL_SC[k, n] > 0 && GRAL_SC[k, n] < 8) // if this situation is not available yet
                                                {
                                                    dummy += "," + Convert.ToString(GRAL_SC[k, n], ic);
                                                }
                                                else
                                                {
                                                    dummy += "," + Convert.ToString(akmettime, ic);
                                                }
                                            }
                                            else
                                            {
                                                dummy += "," + Convert.ToString(akmettime, ic);
                                            }
                                        }
                                        meteoWriter.WriteLine(dummy);
                                    }
                                    else if (corr_situation > -3) // found no corresponding situation but end of numbwet not reached
                                    {
                                        dummy_year = Convert.ToString(1990 + year_increase);
                                        dummy = day + "." + month + "." + dummy_year + "," + hour + ":00,";
                                        dummy = dummy + Convert.ToString(0, ic) + "," + Convert.ToString(0, ic) + "," + Convert.ToString(4);
                                        //BackgroundThreadMessageBox(dummy);
                                        meteoWriter.WriteLine(dummy);
                                    }

                                    try
                                    {
                                        if (read.EndOfStream)
                                        {
                                            text2[0] = string.Empty; //quit reading
                                        }
                                        else
                                        {
                                            text2 = read.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                            text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
                                        }
                                    }
                                    catch
                                    {
                                        text2[0] = string.Empty; //quit reading
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
            Computation_Completed = true; // set flag, that computation was successful
        }
        
        private bool Read_Meteo_Zeitreihe(string GRAL_metfile, ref int numbwet, ref bool local_SCL, 
                                          ref double[,] GRAL_u, ref double[,] GRAL_v, ref int[,] GRAL_SC, ref string[] ReceptorHeader)
        {
            numbwet = 0;
            int columnOffset = 2; // old file format
            int headerLineNumber = 0;

            try // 11.9.2017 Kuntner -> new File format?
            {
                using(StreamReader read1 = new StreamReader(GRAL_metfile))
                {
                    string header = read1.ReadLine().Trim();
                    if (header.Equals("U,V,SC"))
                    {
                        local_SCL = true;
                        columnOffset = 3; // U,V,SC
                        headerLineNumber = 1;
                    }
                    else if (header.StartsWith("U,V,SC,BLH"))
                    {
                        local_SCL = true;
                        columnOffset = 4; // U,V,SC,BLH
                        headerLineNumber = 1;
                    }
                    else if (header.StartsWith("U,V,BLH"))
                    {
                        local_SCL = false;
                        columnOffset = 3; //U,V,BLH
                        headerLineNumber = 1;
                    }
                    if (header.EndsWith("+")) // Additional header lines for name and coordinates of receptors
                    {
                        headerLineNumber = 6;
                    }
                }
            }
            catch{}
                
            using (StreamReader read = new StreamReader(GRAL_metfile))
            {
                 try
                {
                    if (headerLineNumber > 0) // Read header lines
                    {
                        ReceptorHeader = new string[headerLineNumber + 1];

                        if (headerLineNumber > 0) // compatibility to old projects
                        {
                            read.ReadLine();
                        }

                        // read all available header lines
                        for (int i = 0; i < headerLineNumber - 1; i++)
                        {
                            if (headerLineNumber > 1 && headerLineNumber < ReceptorHeader.Length)
                            {
                                ReceptorHeader[i] = read.ReadLine();
                            }
                        }
                    }
                    
                    // read entire file
                    while (read.EndOfStream == false)
                    {
                        string[] columns = read.ReadLine().Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        int numberOfReceptors = (int)(columns.Length / columnOffset);
                        
                        int count = 0;
                        
                        for (int numbrec = 0; numbrec < numberOfReceptors; numbrec++)
                        {
                            //check if this situation has been computed, otherwise this line is 0
                            if (columns.Length > count + 2 && GRAL_u.GetUpperBound(0) >= (numberOfReceptors - 1))
                            {
                                GRAL_u[numbrec, numbwet] = Convert.ToDouble(columns[count], ic);
                                GRAL_v[numbrec, numbwet] = Convert.ToDouble(columns[count + 1], ic);
                                if (local_SCL) // 11.9.2017 Kuntner -> new File format
                                {
                                    GRAL_SC[numbrec, numbwet] = Convert.ToInt32(columns[count + 2], ic);
                                }
                                count += columnOffset;
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
            return true;
        } // Read_GRAL_Meteozeitreihe
        
    }
}