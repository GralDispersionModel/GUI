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
using System.Globalization;
using GralIO;

namespace GralBackgroundworkers
{
    public partial class ProgressFormBackgroundworker
    {
        private static readonly CultureInfo ic = CultureInfo.InvariantCulture;
        
        /// <summary>
        /// Compute high percentiles of pollutant concentrations of a time series
        /// </summary>
        private void HighPercentiles(GralBackgroundworkers.BackgroundworkerData mydata,
                                     System.ComponentModel.DoWorkEventArgs e)
        {
            if (mydata.EmissionFactor == 0) // set emission factor to 1 if this value = 0 
            {
                mydata.EmissionFactor = 1;
            }

            int maxsource = mydata.MaxSource;
            double[,] emifac_day = new double[24, maxsource];
            double[,] emifac_mon = new double[12, maxsource];
            string[] text = new string[5];
            string newpath;
            string decsep = mydata.Decsep;
            string[] sg_numbers = new string[maxsource];
            string[] sg_names = mydata.Sel_Source_Grp.Split(',');
            bool no_classification = mydata.Checkbox19;
            
            //get variation for source group
            int itm=0;
            try
            {
                foreach (string source_group_name in sg_names)
                {
                    sg_numbers[itm] = GetSgNumbers(source_group_name);
                    newpath = Path.Combine("Computation", "emissions" + sg_numbers[itm].PadLeft(3,'0') + ".dat");

                    using (StreamReader myreader = new StreamReader (Path.Combine (mydata.Projectname, newpath)))
                    {
                        for (int j = 0; j < 24; j++) 
                        {
                            text = myreader.ReadLine ().Split (new char[] { ',' });
                            emifac_day [j, itm] = Convert.ToDouble (text [1].Replace (".", decsep));
                            if (j < 12)
                            {
                                emifac_mon [j, itm] = Convert.ToDouble (text [2].Replace (".", decsep));
                            }
                        }
                    }
                    itm++;
                }
            }
            catch(Exception ex)
            {
                BackgroundThreadMessageBox ("Reading emissionsxxx.dat " + ex.Message);
                return;
            }

            //reading emission variations
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
            string hour;
            int numhour = 1;
            int nnn = 0;
            //int indexi = 0;
            //int indexj = 0;
            float[][][] conc = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource]));
           
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
                {
                    BackgroundThreadMessageBox ("Error when reading meteopgt.all");
                }
            }
            
            //read mettimeseries.dat to get file length necessary to define some array lengths
            newpath = Path.Combine("Computation", "mettimeseries.dat");
            int mettimefilelength = 0;
            //read mettimeseries.dat to get file length necessary to define some array lengths
            newpath = Path.Combine("Computation", "mettimeseries.dat");
            using (StreamReader sr = new StreamReader(Path.Combine(mydata.Projectname, newpath)))
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
                BackgroundThreadMessageBox ("Error when reading mettimeseries.dat");
                return; // leave method
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
                    using (StreamReader read = new StreamReader(newpath))
                    {
                        //get source group numbers
                        text10 = read.ReadLine().Split(new char[] { ' ', ':', '-', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
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
                            text10 = read.ReadLine().Split(new char[] { ' ', ':', '-', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
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
                    BackgroundThreadMessageBox (ex.Message + " Error when reading emissions_timeseries.txt - evaluation stopped");
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
                BackgroundThreadMessageBox("Reset emission factors for transient calculation " + ex.Message);
                return;
            }
            
            
            int arraylength = Math.Max (2, Convert.ToInt32(mettimefilelength * (100-mydata.Percentile)/100));   
            float[][][][] concpercentile = CreateArray<float[][][]>(mydata.CellsGralX + 1, () => CreateArray<float[][]>(mydata.CellsGralY + 1, () => CreateArray<float[]>(maxsource + 1, () => new float[arraylength])));

            
//			float fac = 0;
//			float fac_total = 0;

            //read mettimeseries.dat
            List<string> data_mettimeseries = new List<string>();
            ReadMettimeseries(Path.Combine(mydata.Projectname, "Computation", "mettimeseries.dat"), ref data_mettimeseries);
            if (data_mettimeseries.Count == 0) // no data available
            { 
                BackgroundThreadMessageBox ("Error reading mettimeseries.dat");
            }

            int count_ws = -1;
            
            foreach(string mettimeseries in data_mettimeseries)
            {
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
                
                month = text3[1];
                day = text3[0];
                hour = text2[1];
                if (hour == "24")
                {
                    hourplus = 1;
                }

                wgmettime = text2[2];
                wrmettime = text2[3];
                akmettime = text2[4];
                //MessageBox.Show(wgmettime+"/"+wrmettime+"/"+akmettime);

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

                    if (transient == 0 || no_classification == true)
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

                        //get correct weather number in dependence on steady-state or transient simulation
                        int weanumb = n;
                        if (transient == 0 || no_classification == true)
                        {
                            weanumb = count_ws;
                        }

                        //itm = 0;
                        //foreach (string source_group_name in sg_names)
                        Object thisLock = new Object();
                        Parallel.For(0, sg_names.Length, itmp =>
                        {
                            if (sg_names.Length > 0) // counts the number of elements in sg_names >0
                            {
                                con_files[itmp] = Convert.ToString(weanumb + 1).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + sg_numbers[itmp].PadLeft(2, '0') + ".con";
                            }
                            else
                            {
                                con_files[itmp] = Convert.ToString(weanumb + 1).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + Convert.ToString(sg_numbers[itmp]).PadLeft(2, '0') + ".con";
                            }

                            if (File.Exists(Path.Combine(mydata.Projectname, @"Computation", con_files[itmp])) == false &&
                                File.Exists(Path.Combine(mydata.Projectname, @"Computation", Convert.ToString(weanumb + 1).PadLeft(5, '0') + ".grz")) == false)
                            {
                                lock(thisLock)
                                {
                                    exist = false;
                                }
                                //break;
                            }
                            //set variables to zero
                            for (int i = 0; i <= mydata.CellsGralX; i++)
                            {
                                for (int j = 0; j <= mydata.CellsGralY; j++)
                            {
                                conc[i][j][itmp] = 0;
                            }
                            }
                            //read GRAL concentration files
                            string filename = Path.Combine(mydata.Projectname, @"Computation", con_files[itmp]);
                            if (!ReadConFiles(filename, mydata, itmp, ref conc))
                            {
                                // Error reading one *.con file
                                exist = false;
                            }
                            //itm++;
                        });
                        thisLock = null; 

                        SetText("Day.Month: " + day + "." + month);
                        
                        if (exist == true) // con file does exist
                        {
                            
                            //number of dispersion situation
                            nnn += 1;
                            //number of hours of specific day
                            numhour += 1;
                            int std = Convert.ToInt32(hour);
                            int mon = Convert.ToInt32(month) - 1;		
                            
                            //for (int ii = 0; ii <= mydata.CellsGralX; ii++)
                            Parallel.For(0, mydata.CellsGralX, ii =>
                            {
                                for (int j = 0; j <= mydata.CellsGralY; j++)
                                {
                                    float fac = 0;
                                    float fac_total = 0;
                                    float[] _ConcCell = conc[ii][j];
                                    float[][] _ConcPerc = concpercentile[ii][j];

                                    int itmi = 0;
                                    foreach (string source_group_name in sg_names)
                                    {
                                        fac = mydata.EmissionFactor * _ConcCell[itmi] * (float)emifac_day[std - hourplus, itmi] * (float)emifac_mon[mon, itmi] * (float)emifac_timeseries[count_ws, itmi];
                                        fac_total += fac;

                                        //compute percentile concentrations for each source group and in total
                                        float[] _conc = _ConcPerc[itmi];
                                        //check whether actual concentration is higher than already stored concentrations
                                        int pcomp = BinarySearch(_conc, fac);
                                        if (pcomp >= 0)
                                        {
                                            Array.Copy(_conc, 1, _conc, 0, pcomp);
                                            _conc[pcomp] = fac;
                                        }
                                        itmi++;
                                    }

                                    //compute total percentile over all source groups
                                    { 
                                        float[] _conc = _ConcPerc[maxsource];
                                        //check whether actual concentration is higher than already stored concentrations
                                        int pcomp = BinarySearch(_conc, fac_total);
                                        if (pcomp >= 0)
                                        {
                                            Array.Copy(_conc, 1, _conc, 0, pcomp);
                                            _conc[pcomp] = fac_total;
                                        }
                                    }
                                }
                            });
                        }
                    }
                }
                
            }
            
            string file;
            string name;
            //write mean concentration hour files for each source group
            if (mydata.Checkbox2)  // sel_sg_checkbox2 = checked
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
                        name = Convert.ToString(Math.Round(mydata.Percentile,1)).Replace(decsep, "_")  + "_" + mydata.Prefix + mydata.Pollutant + "_" + text1a[0] + "_" + mydata.Slicename;
                    }
                    else
                    {
                        name = Convert.ToString(Math.Round(mydata.Percentile, 1)).Replace(decsep, "_") + "_" + mydata.Prefix + mydata.Pollutant + "_" + sg_names[itm] + "_" + mydata.Slicename;
                    }

                    file = Path.Combine(mydata.Projectname, @"Maps", name + ".txt");
                    
                    try
                    {
                        if (File.Exists(file))
                        {
                            try{
                                File.Delete(file);
                            }
                            catch{}
                        }

                        using (StreamWriter myWriter = new StreamWriter(file, false))
                        {
                            mydata.Writer=myWriter; WriteHeader(mydata, mydata.Unit);
                            
                            for (int j = mydata.CellsGralY - 1; j > -1; j--)
                            {
                                for (int i = 0; i < mydata.CellsGralX; i++)
                                {
                                    myWriter.Write(Convert.ToString(Math.Round(concpercentile[i][j][itm][0], 5), ic) + " ");
                                }
                                myWriter.WriteLine();
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        BackgroundThreadMessageBox ("Write results for source groups " + ex.Message);
                    }
                    
                    itm++;
                }

                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                //write mean total concentration
                name = Convert.ToString(Math.Round(mydata.Percentile, 1)).Replace(decsep, "_") + "_" + mydata.Prefix + mydata.Pollutant + "_total" + "_" + mydata.Slicename;
                file = Path.Combine(mydata.Projectname, @"Maps", name + ".txt");
                
                try
                {
                    if (File.Exists(file))
                    {
                        try{
                            File.Delete(file);
                        }
                        catch{}
                    }
                        
                    using (StreamWriter mywriter = new StreamWriter(file, false))
                    {
                        mydata.Writer=mywriter; WriteHeader(mydata, mydata.Unit);
                        
                        for (int j = mydata.CellsGralY - 1; j > -1; j--)
                        {
                            for (int i = 0; i < mydata.CellsGralX; i++)
                            {
                                mywriter.Write(Convert.ToString(Math.Round(concpercentile[i][j][maxsource][0], 5), ic) + " ");
                            }
                            mywriter.WriteLine();
                        }
                    }
                }
                catch(Exception ex)
                {
                    BackgroundThreadMessageBox ("Write total result file " + ex.Message);
                }
            }
            Computation_Completed = true; // set flag, that computation was successful
        }

        /// <summary>
        /// Find the index of a value in Perc[] that exceeds the value Conc
        /// </summary>
        /// <param name="Perc">Percentile array</param> 
        /// <param name="Conc">Concentration to compare with Perc[]</param> 
        private int BinarySearch(float[] Perc, float Conc)
        {
            int low = 0;
            
            if (Conc < Perc[0])
            {
                low = -1; // return -1 if below Perc[0]
            }
            else if (Conc > Perc[Perc.Length - 1])
            {
                low = Perc.Length - 1;
            }
            else
            {
                int high = Perc.Length - 1;
                int mid = 0;
                while (low <= high)
                {
                    mid = (low + high) >> 1;
                    if (Perc[mid] >= Conc)
                    {
                        high = mid - 1;
                    }
                    else
                    {
                        low = mid + 1;
                    }
                }

                low--;
                if (low >= Perc.Length)
                {
                    low = Perc.Length - 1;
                }
            }
            return low;
        }
    }
}