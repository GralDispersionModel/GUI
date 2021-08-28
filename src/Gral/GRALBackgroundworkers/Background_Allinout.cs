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
using GralIO;

namespace GralBackgroundworkers
{
    public partial class ProgressFormBackgroundworker
    {
        /// <summary>
        /// Calculate odour hours based on an All-In All-Out stable
        /// </summary>
        private void OdourAllinAllout(GralBackgroundworkers.BackgroundworkerData mydata,
                                      System.ComponentModel.DoWorkEventArgs e)
        {
            //reading emission variations
            int maxsource = mydata.MaxSource;
            string decsep = mydata.DecSep;
            double[,] emifac_day = new double[24, maxsource];
            double[,] emifac_mon = new double[12, maxsource];
            string[] text = new string[5];
            string newpath;
            string[] sg_numbers = new string[maxsource];
            string[] sg_names = mydata.SelectedSourceGroup.Split(',');
            
            // int allin_index=0; // Kuntner never used 
            float[] freq = new float[maxsource];

            //get variation for source group
            int itm=0;
            try
            {
                foreach (string source_group_name in sg_names)
                {
                    sg_numbers[itm] = GetSgNumbers(source_group_name);
                    newpath = Path.Combine("Computation","emissions" + sg_numbers[itm].PadLeft(3,'0') + ".dat");

                    using (StreamReader myreader = new StreamReader(Path.Combine(mydata.ProjectName, newpath)))
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

            //in transient GRAL mode, it is necessary to set all modulation factors equal to one as they have been considered already in the GRAL simulations
            try
            {
                InDatVariables data = new InDatVariables();
                InDatFileIO ReadInData = new InDatFileIO();
                data.InDatPath = Path.Combine(mydata.ProjectName, "Computation","in.dat");
                ReadInData.Data = data;
                if (ReadInData.ReadInDat() == true)
                {
                    if (data.Transientflag == 0)
                    {
                        for (int j = 0; j < 24; j++)
                        {
                            emifac_day[j, itm] = 1;
                            if (j < 12)
                            {
                                emifac_mon[j, itm] = 1;
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
            List<string> wgmettime = new List<string>();
            List<string> wrmettime = new List<string>();
            List<string> akmettime = new List<string>();
            List<string> hour = new List<string>();
            List<string> month = new List<string>();
            string[] text2 = new string[5];
            string[] text3 = new string[2];
            int hourplus = 0;
            string day="0";
            float division = 24;
            float emptytimes = 0;
            
            newpath = Path.Combine("Computation","mettimeseries.dat");
            try
            {
                using (StreamReader read = new StreamReader(Path.Combine(mydata.ProjectName, newpath)))
                {
                    text2 = read.ReadLine().Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
                    while (text2[0] != "")
                    {
                        //check if averaging time is one hour or half an hour
                        if (hour.Count > 1)
                        {
                            if ((hour[hour.Count - 1] == hour[hour.Count - 2]) && (day == text2[0]))
                            {
                                division = 48;
                            }
                        }

                        month.Add(text3[1]);
                        hour.Add(text2[1]);
                        day = text2[0];
                        if (hour[hour.Count - 1] == "24")
                        {
                            hourplus = 1;
                        }

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

            //breeding cylce in days
            division = mydata.Division;

            //empty times between two breeding cycles in days
            emptytimes = mydata.Emptytimes;

            //source group specifications for all in all out systems
            itm = 0;
            foreach (string source_group_name in sg_names)
            {
                //define frequencies for odour emissions for each source group
                freq[itm] = 1;

                //save index of the source group for which a breeding cycle has been defined
                if (sg_names[itm] == mydata.AllInnSelSourceGroup)
                {
                    //allin_index = itm; //Remark Kuntner: never used
                    freq[itm] = 1 / (division+emptytimes);
                }
                itm++;
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
            int wl = 0;
            int nnn = 0;
            int n_daytime = 0;
            int n_nighttime = 0;
            int n_evening = 0;
            double ntot = 0;
//        	int indexi = 0;
//        	int indexj = 0;
            float[][][] conc = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 2]));
            float[][][] concp = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 2]));
            float[][][] concm = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 2]));
            float[][][] Q_cv0 = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 2]));
            float[][][] td = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 2]));
            float[][][] concmit = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 6]));
            float[][][] concmitp = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 2]));
            float[][][] concmitm = CreateArray<float[][]>(mydata.CellsGralX + 1, () => CreateArray<float[]>(mydata.CellsGralY + 1, () => new float[maxsource + 2]));

            float[,] conctot = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] conctotp = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] conctotm = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] R90_array = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] CFI = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            float[,] counter = new float[mydata.CellsGralX + 1, mydata.CellsGralY + 1];
            double[] fmod = new double[maxsource];
            float normemission;

            foreach(string line_meteopgt in data_meteopgt)
            {
                try
                {
                    //set variables to zero
                    itm = 0;
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
                            con_files[itm] = Convert.ToString(wl).PadLeft(5, '0') + "-" + Convert.ToString(mydata.Slice) + Convert.ToString(sg_names[itm]).PadLeft(2, '0') + ".odr";
                        }

                        if (File.Exists(Path.Combine(mydata.ProjectName, @"Computation", con_files[itm])) == false &&
                            File.Exists(Path.Combine(mydata.ProjectName, @"Computation", Convert.ToString(wl).PadLeft(5, '0') + ".grz")) == false)
                        {
                            exist = false;
                            break;
                        }

                        //check if vertical adjecent files to compute concentration variance exist
                        if (File.Exists(Path.Combine(mydata.ProjectName, @"Computation", odr_files[itm])) == true ||
                            File.Exists(Path.Combine(mydata.ProjectName, @"Computation", Convert.ToString(wl).PadLeft(5, '0') + ".grz")) == true)
                        {
                            exist_conp = true;
                        }
                        
                        //read GRAL concentration files
                        string filename = Path.Combine(mydata.ProjectName, @"Computation", con_files[itm]);
                        if (!ReadConFiles(filename, mydata, itm, ref conc))
                        {
                            // Error reading one *.con file
                            exist = false;
                            exist_conp = false;
                        }

                        //read GRAL output files for quantities needed to compute the concentration variance
                        if (exist_conp == true)
                        {
                            string filename_p = Path.Combine(mydata.ProjectName, @"Computation", odr_files[itm]);
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
                        ntot += frequency / 10;

                        for (int i = 0; i < hour.Count; i++)
                        {
                            if ((wgmet == wgmettime[i]) && (wrmet == wrmettime[i]) && (akmet == akmettime[i]))
                            {
                                nnn += 1;
                                int std = Convert.ToInt16(hour[i]);
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
                                
                                SetText("Dispersion situation " + Convert.ToString(wl) + ": " + Convert.ToString(Math.Round(ntot, 1)) +
                                         "%    " + "Month: " + month[i] + "- Hour: " + hour[i] + ":00h");
                                
                                itm = 0;
                                foreach (string source_group_name in sg_names)
                                {
                                    for (int ii = 0; ii <= mydata.CellsGralX; ii++)
                                    {
                                        for (int j = 0; j <= mydata.CellsGralY; j++)
                                        {
                                            float emission_modulation = (float)(emifac_day[std - hourplus, itm] * emifac_mon[mon, itm]);
                                            if ((ii == 0) && (j == 0))
                                            {
                                                fmod[itm] += emission_modulation;
                                            }

                                            if (freq[itm] == 1)
                                            {
                                                if (mydata.Peakmean < 0)
                                                {
                                                    //compute spatially dependent R90 for each sourcegroup
                                                    conctot[ii, j] += conc[ii][j][itm] * emission_modulation;
                                                    conctotp[ii, j] += concp[ii][j][itm] * emission_modulation;
                                                    conctotm[ii, j] += concm[ii][j][itm] * emission_modulation;
                                                    Q_cv0[ii][j][maxsource] = Math.Max(Q_cv0[ii][j][maxsource], Q_cv0[ii][j][itm]);
                                                    td[ii][j][maxsource] = Math.Max(td[ii][j][maxsource], td[ii][j][itm]);

                                                    float R90 = 4;
                                                    float Q_cv = 0;
                                                    //compute spatially dependent R90 for each plume
                                                    //note that the quantities Q_cv0 and td are flow properties and thus equal for all source groups
                                                    if ((ii != 0) && (j != 0) && (ii < mydata.CellsGralX) && (j < mydata.CellsGralY) && (conctot[ii, j] != 0) && (td[ii][j][itm] != 0))
                                                    {
                                                        float conc_centre = conc[ii][j][itm] * emission_modulation;
                                                        float conc_zplus = concp[ii][j][itm] * emission_modulation;
                                                        float conc_zminus = concm[ii][j][itm] * emission_modulation;
                                                        float conc_xplus = conc[ii + 1][j][itm] * emission_modulation;
                                                        float conc_xminus = conc[ii - 1][j][itm] * emission_modulation;
                                                        float conc_yplus = conc[ii][j + 1][itm] * emission_modulation;
                                                        float conc_yminus = conc[ii][j - 1][itm] * emission_modulation;
                                                        GralConcentrationVarianceModel.Concentration_Variance_Model.R90_calculate(ii, j, conc_xplus, conc_xminus,
                                                            conc_yplus, conc_yminus, conc_zplus, conc_zminus, conc_centre,
                                                            Q_cv0[ii][j][itm], td[ii][j][itm], (float)mydata.Horgridsize, (float)mydata.VertgridSize, ref R90, ref Q_cv);
                                                    }

                                                    if (conc[ii][j][itm] * R90 * 0.001 * emission_modulation >= (float)mydata.OdourThreshold)
                                                    {
                                                        concmit[ii][j][itm]++;
                                                    }
                                                }
                                                else
                                                {
                                                    conctot[ii, j] += conc[ii][j][itm] * emission_modulation;
                                                    if (conc[ii][j][itm] * (float)(mydata.Peakmean) * 0.001 * emission_modulation >= (float)(mydata.OdourThreshold))
                                                    {
                                                        concmit[ii][j][itm]++;
                                                    }
                                                }                                                
                                            }
                                            else
                                            {
                                                if (mydata.Peakmean < 0)
                                                {
                                                    for (int zyklus = 0; zyklus <= Convert.ToInt32(division + emptytimes); zyklus++)
                                                    {
                                                        normemission = freq[itm] * Math.Max((float)zyklus - emptytimes, 0);
                                                        concmit[ii][j][maxsource + 1] = conc[ii][j][itm] * emission_modulation;
                                                        concmitp[ii][j][maxsource + 1] = concp[ii][j][itm] * emission_modulation;
                                                        concmitm[ii][j][maxsource + 1] = concm[ii][j][itm] * emission_modulation;

                                                        float R90 = 4;
                                                        float Q_cv = 0;
                                                        //compute spatially dependent R90 for each plume
                                                        //note that the quantities Q_cv0 and td are flow properties and thus equal for all source groups
                                                        if ((ii != 0) && (j != 0) && (ii < mydata.CellsGralX) && (j < mydata.CellsGralY) && (conctot[ii, j] != 0) && (td[ii][j][maxsource] != 0))
                                                        {
                                                            float conc_centre = conc[ii][j][itm] * emission_modulation * normemission;
                                                            float conc_zplus = concp[ii][j][itm] * emission_modulation * normemission;
                                                            float conc_zminus = concm[ii][j][itm] * emission_modulation * normemission;
                                                            float conc_xplus = conc[ii + 1][j][itm] * emission_modulation * normemission;
                                                            float conc_xminus = conc[ii - 1][j][itm] * emission_modulation * normemission;
                                                            float conc_yplus = conc[ii][j + 1][itm] * emission_modulation * normemission;
                                                            float conc_yminus = conc[ii][j - 1][itm] * emission_modulation * normemission;

                                                            GralConcentrationVarianceModel.Concentration_Variance_Model.R90_calculate(ii, j, conc_xplus, conc_xminus,
                                                                conc_yplus, conc_yminus, conc_zplus, conc_zminus, conc_centre,
                                                                Q_cv0[ii][j][maxsource], td[ii][j][maxsource], (float)mydata.Horgridsize, (float)mydata.VertgridSize, ref R90, ref Q_cv);
                                                        }

                                                        if (conc[ii][j][itm] * R90 * 0.001 * emission_modulation * normemission >= (float)(mydata.OdourThreshold))
                                                        {
                                                            concmit[ii][j][itm] += freq[itm];
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    for (int zyklus = 0; zyklus <= Convert.ToInt32(division + emptytimes); zyklus++)
                                                    {
                                                        normemission = freq[itm] * Math.Max((float)zyklus - emptytimes, 0);
                                                        concmit[ii][j][maxsource + 1] = conc[ii][j][itm] * emission_modulation;
                                                        if (conc[ii][j][itm] * (float)(mydata.Peakmean) * 0.001 * emission_modulation * normemission >= (float)(mydata.OdourThreshold))
                                                        {
                                                            concmit[ii][j][itm] += freq[itm];
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    itm++;
                                }

                                Parallel.For(0, mydata.CellsGralX + 1, ii =>
                                {
                                    for (int j = 0; j <= mydata.CellsGralY; j++)
                                    {
                                        //compute total odour hours
                                        if (mydata.Peakmean < 0)
                                        {
                                            float R90 = 4;
                                            float Q_cv = 0;
                                            //compute spatially dependent R90 for all plumes
                                            //note that the quantities Q_cv0 and td are flow properties and thus equal for all source groups
                                            if ((ii != 0) && (j != 0) && (ii < mydata.CellsGralX) && (j < mydata.CellsGralY) && (conctot[ii, j] != 0) && (td[ii][j][0] != 0))
                                            {
                                                GralConcentrationVarianceModel.Concentration_Variance_Model.R90_calculate(ii, j, conctot[ii + 1, j], conctot[ii - 1, j],
                                                    conctot[ii, j + 1], conctot[ii, j - 1], conctotp[ii, j], conctotm[ii, j], conctot[ii, j],
                                                    Q_cv0[ii][j][0], td[ii][j][0], (float)mydata.Horgridsize, (float)mydata.VertgridSize, ref R90, ref Q_cv);

                                                CFI[ii, j] += Q_cv;
                                                R90_array[ii, j] += R90;
                                                counter[ii, j]++;
                                            }

                                            //reset arrays to zero
                                            Q_cv0[ii][j][maxsource] = 0;
                                            td[ii][j][maxsource] = 0;

                                            if (conctot[ii, j] * R90 * 0.001 >= (float)(mydata.OdourThreshold))
                                            {
                                                concmit[ii][j][maxsource]++;

                                                //daytime odour-hour frequency
                                                if ((std >= 6) && (std < 19))
                                                {
                                                    concmit[ii][j][maxsource + 2]++;
                                                }
                                                //evening odour-hour frequency
                                                if ((std >= 19) && (std < 22))
                                                {
                                                    concmit[ii][j][maxsource + 3]++;
                                                }
                                                //nightime odour-hour frequency
                                                if ((std >= 22) || (std < 6))
                                                {
                                                    concmit[ii][j][maxsource + 4]++;
                                                }
                                            }
                                            else
                                            {
                                                int itm1 = 0;
                                                foreach (string source_group_name in sg_names)
                                                {
                                                    if (freq[itm1] != 1)
                                                    {
                                                        for (int zyklus = 0; zyklus <= Convert.ToInt32(division + emptytimes); zyklus++)
                                                        {
                                                            normemission = freq[itm1] * Math.Max((float)zyklus - emptytimes, 0);

                                                            
                                                            //compute spatially dependent R90 for each plume
                                                            //note that the quantities Q_cv0 and td are flow properties and thus equal for all source groups
                                                            if ((ii != 0) && (j != 0) && (ii < mydata.CellsGralX) && (j < mydata.CellsGralY) && (conctot[ii, j] != 0) && (td[ii][j][0] != 0))
                                                            {
                                                                float conc_centre = (conctot[ii, j] + concmit[ii][j][maxsource + 1] * normemission);
                                                                float conc_zplus = (conctotp[ii, j] + concmitp[ii][j][maxsource + 1] * normemission);
                                                                float conc_zminus = (conctotm[ii, j] + concmitm[ii][j][maxsource + 1] * normemission);
                                                                float conc_xplus = (conctot[ii + 1, j] + concmit[ii + 1][j][maxsource + 1] * normemission);
                                                                float conc_xminus = (conctot[ii - 1, j] + concmit[ii - 1][j][maxsource + 1] * normemission);
                                                                float conc_yplus = (conctot[ii, j + 1] + concmit[ii][j + 1][maxsource + 1] * normemission);
                                                                float conc_yminus = (conctot[ii, j - 1] + concmit[ii][j - 1][maxsource + 1] * normemission);

                                                                GralConcentrationVarianceModel.Concentration_Variance_Model.R90_calculate(ii, j, conc_xplus, conc_xminus,
                                                                    conc_yplus, conc_yminus, conc_zplus, conc_zminus, conc_centre,
                                                                    Q_cv0[ii][j][0], td[ii][j][0], (float)mydata.Horgridsize, (float)mydata.VertgridSize, ref R90, ref Q_cv);
                                                            }

                                                            if ((conctot[ii, j] + concmit[ii][j][maxsource + 1] * normemission) * R90 * 0.001 >= (float)(mydata.OdourThreshold))
                                                            {
                                                                concmit[ii][j][maxsource] += freq[itm1];

                                                                //daytime odour-hour frequency
                                                                if ((std >= 6) && (std < 19))
                                                                {
                                                                    concmit[ii][j][maxsource + 2] += freq[itm1];
                                                                }
                                                                //evening odour-hour frequency
                                                                if ((std >= 19) && (std < 22))
                                                                {
                                                                    concmit[ii][j][maxsource + 3] += freq[itm1];
                                                                }
                                                                //nightime odour-hour frequency
                                                                if ((std >= 22) || (std < 6))
                                                                {
                                                                    concmit[ii][j][maxsource + 4] += freq[itm1];
                                                                }
                                                                //annual weighted odour hours
                                                                float weight = 1f;
                                                                if ((mon < 4) || (mon < 11))
                                                                {
                                                                    weight = 0.6447f;
                                                                    if ((std >= 6) && (std < 18))
                                                                    {
                                                                        weight = 1f;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    weight = 0.8381f;
                                                                    if ((std >= 6) && (std < 21))
                                                                    {
                                                                        weight = 1.3f;
                                                                    }
                                                                }
                                                                concmit[ii][j][maxsource + 5] += freq[itm1] * weight;
                                                            }
                                                        }
                                                    }
                                                    itm1++;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (conctot[ii, j] * (float)(mydata.Peakmean) * 0.001 >= (float)(mydata.OdourThreshold))
                                            {
                                                concmit[ii][j][maxsource]++;
                                            }
                                            else
                                            {
                                                int itm1 = 0;
                                                foreach (string source_group_name in sg_names)
                                                {
                                                    if (freq[itm1] != 1)
                                                    {
                                                        for (int zyklus = 0; zyklus <= Convert.ToInt32(division + emptytimes); zyklus++)
                                                        {
                                                            normemission = freq[itm1] * Math.Max((float)zyklus - emptytimes, 0);
                                                            if ((conctot[ii, j] + concmit[ii][j][maxsource + 1] * normemission) * (float)(mydata.Peakmean) * 0.001 >= (float)(mydata.OdourThreshold))
                                                            {
                                                                concmit[ii][j][maxsource] += freq[itm1];

                                                                //daytime odour-hour frequency
                                                                if ((std >= 6) && (std < 19))
                                                                {
                                                                    concmit[ii][j][maxsource + 2] += freq[itm1];
                                                                }
                                                                //evening odour-hour frequency
                                                                if ((std >= 19) && (std < 22))
                                                                {
                                                                    concmit[ii][j][maxsource + 3] += freq[itm1];
                                                                }
                                                                //nightime odour-hour frequency
                                                                if ((std >= 22) || (std < 6))
                                                                {
                                                                    concmit[ii][j][maxsource + 4] += freq[itm1];
                                                                }
                                                                //annual weighted odour hours
                                                                float weight = 1f;
                                                                if ((mon < 4) || (mon < 11))
                                                                {
                                                                    weight = 0.6447f;
                                                                    if ((std >= 6) && (std < 18))
                                                                    {
                                                                        weight = 1f;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    weight = 0.8381f;
                                                                    if ((std >= 6) && (std < 21))
                                                                    {
                                                                        weight = 1.3f;
                                                                    }
                                                                }
                                                                concmit[ii][j][maxsource + 5] += freq[itm1] * weight;
                                                            }
                                                        }
                                                    }
                                                    itm1++;
                                                }
                                            }
                                        }
                                        conctot[ii, j] = 0;
                                        conctotp[ii, j] = 0;
                                        conctotm[ii, j] = 0;
                                    }
                                });

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
                {}
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
                        concmit[i][j][itm] = concmit[i][j][itm] / (float)nnn * 100;
                    }
                    }

                    itm++;
                }

                //total concentration
                Parallel.For(0, mydata.CellsGralX + 1, i =>
                {
                    for (int j = 0; j <= mydata.CellsGralY; j++)
                    {
                        concmit[i][j][maxsource] = concmit[i][j][maxsource] / (float)nnn * 100;
                        concmit[i][j][maxsource + 2] = concmit[i][j][maxsource + 2] / (float)n_daytime * 100;
                        concmit[i][j][maxsource + 3] = concmit[i][j][maxsource + 3] / (float)n_evening * 100;
                        concmit[i][j][maxsource + 4] = concmit[i][j][maxsource + 4] / (float)n_nighttime * 100;
                        concmit[i][j][maxsource + 5] = concmit[i][j][maxsource + 5] / (float)nnn * 100;

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
                {
                    name = mydata.Prefix + mydata.Pollutant + "_" + sg_names[itm] + "_" + mydata.Slicename + "_" + Convert.ToString(mydata.OdourThreshold) + "GE_PM" + Convert.ToString(mydata.Peakmean);
                }

                file = Path.Combine(mydata.ProjectName, @"Maps","Mean_All_in_All_out_" + name + ".txt");
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
            file = Path.Combine(mydata.ProjectName, @"Maps", "Mean_All_in_All_out_" + name + ".txt");
            Result.Z = maxsource;
            Result.Values = concmit;
            Result.FileName = file;
            Result.WriteFloatResult();

            //write mean total daytime odour hour file
            /*
            name = mydata.Prefix + mydata.Pollutant + "_total_6-18h" + "_" + mydata.Slicename + "_" + Convert.ToString(mydata.OdourThreshold) + "GE_PM" + Convert.ToString(mydata.Peakmean);
            file = Path.Combine(mydata.Projectname, @"Maps", "Mean_" + name + ".txt");
            Result.Z = maxsource + 2;
            Result.Values = concmit;
            Result.FileName = file;
            Result.Write_Result();
            
            //write mean total evening odour hour file
            name = mydata.Prefix + mydata.Pollutant + "_total_19-21h" + "_" + mydata.Slicename + "_" + Convert.ToString(mydata.OdourThreshold) + "GE_PM" + Convert.ToString(mydata.Peakmean);
            file = Path.Combine(mydata.Projectname, @"Maps", "Mean_" + name + ".txt");
            Result.Z = maxsource + 3;
            Result.Values = concmit;
            Result.FileName = file;
            Result.Write_Result();
            
            //write mean total nighttime odour hour file
            name = mydata.Prefix + mydata.Pollutant + "_total_22-5h" + "_" + mydata.Slicename + "_" + Convert.ToString(mydata.OdourThreshold) + "GE_PM" + Convert.ToString(mydata.Peakmean);
            file = Path.Combine(mydata.Projectname, @"Maps", "Mean_" + name + ".txt");
            Result.Z = maxsource + 4;
            Result.Values = concmit;
            Result.FileName = file;
            Result.Write_Result();
            
            //write mean total weighted odour hour file
            name = mydata.Prefix + mydata.Pollutant + "_total_weighted" + "_" + mydata.Slicename + "_" + Convert.ToString(mydata.OdourThreshold) + "GE_PM" + Convert.ToString(mydata.Peakmean);
            file = Path.Combine(mydata.Projectname, @"Maps", "Mean_" + name + ".txt");
            Result.Z = maxsource + 5;
            Result.Values = concmit;
            Result.FileName = file;
            Result.Write_Result();
             */

            if (Rechenknecht.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            if (mydata.Peakmean < 0 && mydata.WriteDepositionOrOdourData) // use new odour model
            {
                //write mean total R90
                string name5 = mydata.Prefix + mydata.Pollutant + "_" + mydata.Slicename + "_total";
                string file5 = Path.Combine(mydata.ProjectName, @"Maps", "R90_" + name5 + ".txt");
                Result.Z = -1;
                Result.Round = 2;
                Result.Unit = "-";
                Result.TwoDim = R90_array;
                Result.FileName = file5;
                Result.WriteFloatResult();
                
                //write mean total concentration flucutation intensity
                name5 = mydata.Prefix + mydata.Pollutant + "_" + mydata.Slicename + "_total";
                file5 = Path.Combine(mydata.ProjectName, @"Maps", "ConcentrationFluctuationIntensity_" + name5 + ".txt");
                Result.TwoDim = CFI;
                Result.FileName = file5;
                Result.WriteFloatResult();
            }
            Computation_Completed = true; // set flag, that computation was successful
        }	
    }
}
