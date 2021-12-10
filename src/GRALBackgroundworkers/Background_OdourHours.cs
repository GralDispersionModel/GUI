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
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace GralBackgroundworkers
{
    public partial class ProgressFormBackgroundworker
    {
        /// <summary>
        /// Calculate odour hours based on several approaches
        /// </summary>
        private void OdourHours(GralBackgroundworkers.BackgroundworkerData mydata,
                                System.ComponentModel.DoWorkEventArgs e)
        {
            //reading emission variations
            int maxsource = mydata.MaxSource;
            string decsep = mydata.DecSep;
            double[,] emifac_day = new double[24, maxsource];
            double[,] emifac_mon = new double[12, maxsource];
            string [] text=new string[5];
            string newpath;
            string[] sg_numbers = new string[maxsource];
            string[] sg_names = mydata.SelectedSourceGroup.Split(',');			

            //get variation for source group
            int itm=0;
            try
            {
                foreach (string source_group_name in sg_names)
                {
                    sg_numbers[itm] = GetSgNumbers(source_group_name);
                    newpath = Path.Combine("Computation", "emissions" + sg_numbers[itm].PadLeft(3,'0') + ".dat");

                    StreamReader myreader = new StreamReader(Path.Combine(mydata.ProjectName, newpath));
                    for (int j = 0; j < 24; j++)
                    {
                        text = myreader.ReadLine().Split(new char[] { ',' });
                        emifac_day[j, itm] = Convert.ToDouble(text[1].Replace(".", decsep));
                        if (j < 12)
                        {
                            emifac_mon[j, itm] = Convert.ToDouble(text[2].Replace(".", decsep));
                        }
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
            List<string> wgmettime=new List<string>();
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
                    text2 = read.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
                    while (text2[0] != "")
                    {
                        month.Add(text3[1]);
                        hour.Add(text2[1]);
                        if (hour[hour.Count - 1] == "24")
                        {
                            hourplus = 1;
                        }

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
            catch(Exception ex)
            {
                BackgroundThreadMessageBox (ex.Message);
                return;
            }

            //if file emissions_timeseries.txt exists, these modulation factors will be used
            int[] sg_time = new int[maxsource];
            double[,] emifac_timeseries = new double[month.Count, maxsource];

            //it is necessary to set all values of the array emifac_timeseries equal to 1
            for (int i = 0; i < month.Count; i++)
            {
                for (int n = 0; n < maxsource; n++)
                {
                    emifac_timeseries[i, n] = 1;
                }
            }

            newpath = Path.Combine(mydata.ProjectName, "Computation", "emissions_timeseries.txt");
            if (File.Exists(newpath) == true)
            {
                try
                {
                    //read timeseries of emissions
                    string[] text10 = new string[1];
                    using (StreamReader read = new StreamReader(newpath))
                    {
                        //get source group numbers
                        text10 = read.ReadLine().Split(new char[] { ' ', ':', '-', '\t', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
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

                        for (int i = 0; i < month.Count; i++)
                        {
                            text10 = read.ReadLine().Split(new char[] { ' ', ':', '-', '\t', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
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

            //read meteopgt.all
            List<string> data_meteopgt = new List<string>();
            ReadMeteopgtAll(Path.Combine(mydata.ProjectName, "Computation", "meteopgt.all"), ref data_meteopgt);
            if (data_meteopgt.Count == 0) // no data available
            { 
                BackgroundThreadMessageBox ("Error reading meteopgt.all");
            }

            string wgmet;
            string wrmet;
            string akmet;
            double frequency;
            int wl=0;
            int nnn = 0;
            int n_daytime = 0;
            int n_nighttime = 0;
            int n_evening = 0;
            double ntot = 0;
            
            float[][][] conc = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource]));
            float[][][] concp = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource]));
            float[][][] concm = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource]));
            float[][][] Q_cv0 = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 1]));
            float[][][] td = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 1]));
            float[][][] concmit = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 5]));

            float[,] conctot = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] conctotp = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] conctotm = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] R90_array = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] CFI = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] Conc_standard = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] counter = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            double[] fmod = new double[maxsource];

            foreach(string line_meteopgt in data_meteopgt)	
            {
                try
                {
                    //set variables to zero
                    RestoreJaggedArray(conc);
                    RestoreJaggedArray(concp);
                    RestoreJaggedArray(concm);

                    //meteopgt.all
                    wl += 1;
                    
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
                    
                    text = line_meteopgt.Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    wrmet = text[0];
                    wgmet = text[1];
                    akmet = text[2];
                    frequency = Convert.ToDouble(text[3].Replace(".",decsep));
                    //GRAL filenames
                    string[] con_files = new string[100];
                    string[] odr_files = new string[100];
                    
                    //itm=0;
                    //foreach (string source_group_name in sg_names)
                    Object thisLock = new Object();
                    Parallel.For(0, sg_names.Length, itmp =>
                    {
                        bool parallel_existconp = false;
                        if (sg_names.Length > 0)
                        {
                            con_files[itmp] = Convert.ToString(wl).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + sg_numbers[itmp].PadLeft(2, '0') + ".con";
                            odr_files[itmp] = Convert.ToString(wl).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + sg_numbers[itmp].PadLeft(2, '0') + ".odr";
                        }
                        else
                        {
                            con_files[itmp] = Convert.ToString(wl).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + Convert.ToString(sg_names[itmp]).PadLeft(2, '0') + ".con";
                            odr_files[itmp] = Convert.ToString(wl).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + Convert.ToString(sg_names[itmp]).PadLeft(2, '0') + ".odr";
                        }

                        if (File.Exists(Path.Combine(mydata.ProjectName, @"Computation", con_files[itmp])) == false &&
                            File.Exists(Path.Combine(mydata.ProjectName, @"Computation", Convert.ToString(wl).PadLeft(5, '0') + ".grz")) == false) 
                        {
                            lock(thisLock)
                            {
                                exist = false;
                            }
                            //break;
                        }

                        //check if vertical adjecent files to compute concentration variance exist
                        if (File.Exists(Path.Combine(mydata.ProjectName, @"Computation", odr_files[itmp])) == true |
                            File.Exists(Path.Combine(mydata.ProjectName, @"Computation", Convert.ToString(wl).PadLeft(5, '0') + ".grz")) == true)
                        {
                            parallel_existconp = true;
                        }

                        //read GRAL concentration files
                        string filename = Path.Combine(mydata.ProjectName, @"Computation", con_files[itmp]);
                        if (!ReadConFiles(filename, mydata, itmp, ref conc))
                        {
                            // Error reading one *.con file
                            lock (thisLock)
                            {
                                exist = false;
                                parallel_existconp = false;
                            }
                        }

                        //read GRAL output files for quantities needed to compute the concentration variance
                        if (parallel_existconp == true)
                        {
                            string filename_p = Path.Combine(mydata.ProjectName, @"Computation", odr_files[itmp]);
                            if (!ReadOdrFiles(filename_p, mydata, itmp, ref concp, ref concm, ref Q_cv0, ref td))
                            {
                                // Error reading the odr file -> Analysis not possible
                                lock (thisLock)
                                {
                                    exist = false;
                                }
                            }
                        }
                    });
                    thisLock = null;
                    
                    if (exist == true)
                    {
                        ntot += frequency / 10;

                        SetText("Dispersion situation " + Convert.ToString(wl) + ":" + Convert.ToString(Math.Round(ntot, 1) + "%"));
                        
                        for (int i = 0; i < hour.Count; i++)
                        {
                            if ((wgmet == wgmettime[i]) && (wrmet == wrmettime[i]) && (akmet == akmettime[i]))
                            {
                                nnn += 1;
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

                                //for (int ii = 0; ii <= mydata.CellsGralX; ii++)
                                Parallel.For(0, mydata.CellsGralX + 1, ii =>
                                {
                                    for (int j = 0; j <= mydata.CellsGralY; j++)
                                    {
                                        conctot[ii, j] = 0;
                                        conctotp[ii, j] = 0;
                                        conctotm[ii, j] = 0;
                                        int n_source = 0;

                                        foreach (string source_group_name in sg_names)
                                        {
                                            float emission_modulation = (float)(emifac_day[std - hourplus, n_source] * emifac_mon[mon, n_source] * emifac_timeseries[nnn - 1, n_source]);
                                            if ((ii == 0) && (j == 0))
                                            {
                                                fmod[n_source] += emission_modulation;
                                            }

                                            conctot[ii, j] += conc[ii][j][n_source] * emission_modulation;

                                            if (mydata.Peakmean < 0)
                                            {
                                                //compute spatially dependent R90 for each sourcegroup
                                                conctotp[ii, j] += concp[ii][j][n_source] * emission_modulation;
                                                conctotm[ii, j] += concm[ii][j][n_source] * emission_modulation;
                                                Q_cv0[ii][j][maxsource] = Math.Max(Q_cv0[ii][j][maxsource], Q_cv0[ii][j][n_source]);
                                                td[ii][j][maxsource] = Math.Max(td[ii][j][maxsource], td[ii][j][n_source]);

                                                float R90 = 4;
                                                float Q_cv = 0;
                                                //compute spatially dependent R90 for each plume
                                                //note that the quantities Q_cv0 and td are flow properties and thus equal for all source groups
                                                if ((ii != 0) && (j != 0) && (ii < mydata.CellsGralX) && (j < mydata.CellsGralY) && (conc[ii][j][n_source] != 0) && (td[ii][j][n_source] != 0))
                                                {
                                                    float conc_centre = conc[ii][j][n_source] * emission_modulation;
                                                    float conc_zplus = concp[ii][j][n_source] * emission_modulation;
                                                    float conc_zminus = concm[ii][j][n_source] * emission_modulation;
                                                    float conc_xplus =  conc[ii + 1][j][n_source] * emission_modulation;
                                                    float conc_xminus = conc[ii - 1][j][n_source] * emission_modulation;
                                                    float conc_yplus =  conc[ii][j + 1][n_source] * emission_modulation;
                                                    float conc_yminus = conc[ii][j - 1][n_source] * emission_modulation;
                                                    GralConcentrationVarianceModel.Concentration_Variance_Model.R90_calculate(ii, j, conc_xplus, conc_xminus,
                                                        conc_yplus, conc_yminus, conc_zplus, conc_zminus, conc_centre,
                                                        Q_cv0[ii][j][n_source], td[ii][j][n_source], (float)mydata.Horgridsize, (float)mydata.VertgridSize, ref R90, ref Q_cv);
                                                }

                                                if (conc[ii][j][n_source] * R90 * 0.001 * emission_modulation >= (float)mydata.OdourThreshold)
                                                {
                                                    concmit[ii][j][n_source]++;
                                                }
                                            }
                                            else
                                            {
                                                if (conc[ii][j][n_source] * (float)mydata.Peakmean * 0.001 * emission_modulation >= (float)mydata.OdourThreshold)
                                                {
                                                    concmit[ii][j][n_source]++;
                                                }
                                            }
                                            n_source++;
                                        }
                                        //compute spatially dependent R90 for all plumes not possible at this stage as conctot has to be computed entirly first
                                        if (mydata.Peakmean < 0)
                                        { }
                                        else
                                        {
                                            if (conctot[ii, j] * (float)mydata.Peakmean * 0.001 >= (float)mydata.OdourThreshold)
                                            {
                                                concmit[ii][j][maxsource]++;
                                            }
                                        }
                                    }
                                });


                                if (mydata.Peakmean < 0)
                                {
                                    //for (int ii = 0; ii <= mydata.CellsGralX; ii++)
                                    Parallel.For(0, mydata.CellsGralX + 1, ii =>
                                    {
                                        for (int j = 0; j <= mydata.CellsGralY; j++)
                                        {
                                            float R90 = 4;
                                            float Q_cv = 0;

                                            //compute spatially dependent R90 for all plumes
                                            //note that the quantities Q_cv0 and td are flow properties and thus equal for all source groups
                                            if ((ii != 0) && (j != 0) && (ii < mydata.CellsGralX) && (j < mydata.CellsGralY) && (conctot[ii, j] != 0) && (td[ii][j][maxsource] != 0))
                                            {

                                                GralConcentrationVarianceModel.Concentration_Variance_Model.R90_calculate(ii, j, conctot[ii + 1, j], conctot[ii - 1, j],
                                                    conctot[ii, j + 1], conctot[ii, j - 1], conctotp[ii, j], conctotm[ii, j], conctot[ii, j],
                                                    Q_cv0[ii][j][maxsource], td[ii][j][maxsource], (float)mydata.Horgridsize, (float)mydata.VertgridSize, ref R90, ref Q_cv);

                                                CFI[ii, j] += Q_cv;
                                                R90_array[ii, j] += R90;
                                                Conc_standard[ii, j] += Q_cv * conctot[ii, j];
                                                counter[ii, j]++;
                                            }

                                            //reset arrays to zero
                                            Q_cv0[ii][j][maxsource] = 0;
                                            td[ii][j][maxsource] = 0;

                                            if (conctot[ii, j] * R90 * 0.001 >= (float)mydata.OdourThreshold)
                                            {
                                                concmit[ii][j][maxsource] = concmit[ii][j][maxsource] + 1;
                                                //daytime odour-hour frequency
                                                if ((std >= 6) && (std < 19))
                                                {
                                                    concmit[ii][j][maxsource + 1]++;
                                                }
                                                //evening odour-hour frequency
                                                if ((std >= 19) && (std < 22))
                                                {
                                                    concmit[ii][j][maxsource + 2]++;
                                                }
                                                //nightime odour-hour frequency
                                                if ((std >= 22) || (std < 6))
                                                {
                                                    concmit[ii][j][maxsource + 3]++;
                                                }
                                                //annual weighted odour hours
                                                double weight = 1;
                                                if ((mon < 4) || (mon < 11))
                                                {
                                                    weight = 0.6447;
                                                    if ((std >= 6) && (std < 18))
                                                    {
                                                        weight = 1;
                                                    }
                                                }
                                                else
                                                {
                                                    weight = 0.8381;
                                                    if ((std >= 6) && (std < 21))
                                                    {
                                                        weight = 1.3;
                                                    }
                                                }
                                                concmit[ii][j][maxsource + 4] += (float)(weight);
                                            }
                                        }
                                    });
                                }

                                hour.RemoveAt(i);
                                month.RemoveAt(i);
                                wgmettime.RemoveAt(i);
                                wrmettime.RemoveAt(i);
                                akmettime.RemoveAt(i);
                                i -= 1;
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
                float _number = (float)nnn;

                foreach (string source_group_name in sg_names)
                {
                    fmod[itm] = fmod[itm] / Convert.ToDouble(nnn);
                    Parallel.For(0, mydata.CellsGralX + 1, i =>
                    {
                        for (int j = 0; j <= mydata.CellsGralY; j++)
                        {
                            concmit[i][j][itm] = concmit[i][j][itm] / _number * 100;
                        }
                    });
                    itm++;
                }

                //total concentration
                Parallel.For(0, mydata.CellsGralX + 1, i =>
                {
                    for (int j = 0; j <= mydata.CellsGralY; j++)
                    {
                        concmit[i][j][maxsource] = concmit[i][j][maxsource] / _number * 100;
                        concmit[i][j][maxsource + 1] = concmit[i][j][maxsource + 1] / (float)n_daytime * 100;
                        concmit[i][j][maxsource + 2] = concmit[i][j][maxsource + 2] / (float)n_evening * 100;
                        concmit[i][j][maxsource + 3] = concmit[i][j][maxsource + 3] / (float)n_nighttime * 100;
                        concmit[i][j][maxsource + 4] = concmit[i][j][maxsource + 4] / _number * 100;
                        if (counter[i, j] > 0)
                        {
                            R90_array[i, j] = R90_array[i, j] / (float)counter[i, j];
                            CFI[i, j] = CFI[i, j] / (float)counter[i, j];
                            Conc_standard[i, j] = Conc_standard[i, j] / (float)counter[i, j];
                        }
                    }
                });
                //total concentration must be larger than each single source group concentration
                itm = 0;
                foreach (string source_group_name in sg_names)
                {
                    Parallel.For(0, mydata.CellsGralX + 1, i =>
                    {
                        for (int j = 0; j <= mydata.CellsGralY; j++)
                        {
                            concmit[i][j][maxsource] = Math.Max(concmit[i][j][maxsource], concmit[i][j][itm]);
                        }
                    });
                    itm++;
                }
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
                {
                    name = mydata.Prefix + mydata.Pollutant	+ "_" + sg_names[itm] + "_" + mydata.Slicename + "_" + Convert.ToString(mydata.OdourThreshold) + "GE_PM" + Convert.ToString(mydata.Peakmean);
                }

                file = Path.Combine(mydata.ProjectName, @"Maps", "Mean_" + name + ".txt");
                Result.Z = itm;
                Result.Values = concmit;
                Result.FileName = file;
                Result.WriteFloatResult();
                
                itm++;
            }

            if (Rechenknecht.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            //write mean total odour hour file
            name = mydata.Prefix + mydata.Pollutant + "_total" + "_" + mydata.Slicename + "_" + Convert.ToString(mydata.OdourThreshold) + "GE_PM" + Convert.ToString(mydata.Peakmean);
            file = Path.Combine(mydata.ProjectName, @"Maps", "Mean_" + name + ".txt");
            Result.Z = maxsource;
            Result.Values = concmit;
            Result.FileName = file;
            Result.WriteFloatResult();

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
                        
            if (mydata.Peakmean < 0 && mydata.WriteDepositionOrOdourData) // use new odour model and write additional data
            {
                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                //write mean total R90
                string name5 = mydata.Prefix + mydata.Pollutant + "_" + mydata.Slicename + "_total";
                string file5 = Path.Combine(mydata.ProjectName, @"Maps", "R90_" + name5 + ".txt");
                Result.Z = -1;
                Result.Round = 2;
                Result.Unit = "-";
                Result.TwoDim = R90_array;
                Result.FileName = file5;
                Result.WriteFloatResult();

                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                //write mean total concentration flucutation intensity
                name5 = mydata.Prefix + mydata.Pollutant + "_" + mydata.Slicename + "_total";
                file5 = Path.Combine(mydata.ProjectName, @"Maps", "ConcentrationFluctuationIntensity_" + name5 + ".txt");
                Result.TwoDim = CFI;
                Result.FileName = file5;
                Result.WriteFloatResult();

                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                //write mean total standard deviation of the concentration flucutations
                name5 = mydata.Prefix + mydata.Pollutant + "_" + mydata.Slicename + "_total";
                file5 = Path.Combine(mydata.ProjectName, @"Maps", "ConcentrationStandardDeviation_" + name5 + ".txt");
                Result.Unit =  @"OU/m" + Gral.Main.CubeString;
                Result.TwoDim = Conc_standard;
                Result.FileName = file5;
                Result.WriteFloatResult();
                
            }
            
            Computation_Completed = true; // set flag, that computation was successful
        }
    }
}
