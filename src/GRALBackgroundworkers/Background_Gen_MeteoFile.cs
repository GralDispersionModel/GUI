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
using System.Threading;
using System.IO;
using System.Collections.Generic;
using GralIO;

namespace GralBackgroundworkers
{
    public partial class ProgressFormBackgroundworker
    { 
        /// <summary>
        /// Generate one or multiple meteo-file(s) from GRAMM windfields
        /// </summary>
        private void GenerateMeteofile(GralBackgroundworkers.BackgroundworkerData mydata, 
                                       System.ComponentModel.DoWorkEventArgs e)
        {
            //reading geometry file "ggeom.asc"
            GGeomFileIO ggeom = new GGeomFileIO
            {
                PathWindfield = mydata.Path_GRAMMwindfield
            };

            double[,] AH = new double[1, 1];
            double[, ,] ZSP = new double[1, 1, 1];
            string[] text = new string[1];
            int NX = 1;
            int NY = 1;
            int NZ = 1;
            int ischnitt = 1;
            
            if (ggeom.ReadGGeomAsc(0) == true)
            {
                NX = ggeom.NX;
                NY = ggeom.NY;
                NZ = ggeom.NZ;
                AH = ggeom.AH;
                ZSP = ggeom.ZSP;
                ggeom = null;
            }
                                    
            //windfield file Readers
            //PSFReader windfield = new PSFReader(form1.GRAMMwindfield, true);
            
            // read meteopgt
            List<string> data_meteopgt = new List<string>();
            ReadMeteopgtAll(Path.Combine(mydata.Path_GRAMMwindfield, @"meteopgt.all"), ref data_meteopgt);
            if (data_meteopgt.Count == 0) // no data available
            { 
                BackgroundThreadMessageBox ("Error reading meteopgt.all");
            }

            // read mettimeseries
            List<string> data_mettimeseries = new List<string>();
            ReadMettimeseries(Path.Combine(mydata.Path_GRAMMwindfield, @"mettimeseries.dat"), ref data_mettimeseries);
            if (data_mettimeseries.Count == 0) // no data available
            { 
                BackgroundThreadMessageBox ("Error reading mettimeseries.dat");
            }

            //loop over all weather situations
            double Uoben = 0;
            double Voben = 0;
            double Uunten = 0;
            double Vunten = 0;
            double Umittel = 0;
            double Vmittel = 0;
            double[] nsektor = new double[100000];
            double[] wg=new double[100000];
            
            int[][] local_akla = GralIO.Landuse.CreateArray<int[]>(mydata.EvalPoints.Count, () => new int[data_meteopgt.Count + 5]);
            int[][] iwr = GralIO.Landuse.CreateArray<int[]>(mydata.EvalPoints.Count, () => new int[data_meteopgt.Count + 5]);
            float[][] wgi = GralIO.Landuse.CreateArray<float[]>(mydata.EvalPoints.Count, () => new float[data_meteopgt.Count + 5]);
            int[] iakla = new int[100000];
            //int[] local_akla = new int[100000];
            //int[] iwr = new int[100000];
            int iiwet = 0;
            //double[] wgi = new double[100000];
            float[, ,] UWI = new float[NX + 1, NY + 1, NZ + 1];
            float[, ,] VWI = new float[NX + 1, NY + 1, NZ + 1];
            float[, ,] WWI = new float[NX + 1, NY + 1, NZ + 1];
            ReadSclUstOblClasses ReadStablity = new ReadSclUstOblClasses();
            Windfield_Reader Reader = new Windfield_Reader();

            int n = 1;
            foreach(string line_meteopgt in data_meteopgt)
            {
                SetText("Reading windfields: "+ Convert.ToString(iiwet));
                
                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (n % 4 == 0)
                {
                    Rechenknecht.ReportProgress((int) (n / (double) data_meteopgt.Count * 100D));
                }
                
                iiwet += 1;
                bool fileOK = true;
                try
                {
                    text = line_meteopgt.Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    nsektor[n] = Convert.ToDouble(text[0].Replace(".", mydata.DecSep));
                    wg[n] = Convert.ToDouble(text[1].Replace(".", mydata.DecSep));
                    iakla[n] = Convert.ToInt32(text[2]);
                }
                catch
                {
                    fileOK = false;
                }
                
                try
                {
                    //read wind fields
                    string wndfilename = Path.Combine(mydata.Path_GRAMMwindfield, Convert.ToString(iiwet).PadLeft(5, '0') + ".wnd");
                    if (Reader.Windfield_read(wndfilename, NX, NY, NZ, ref UWI, ref VWI, ref WWI) == false)
                    {
                        throw new IOException();
                    }
                }
                catch
                {
                    fileOK = false;
                }

                if (fileOK)
                {
                    bool local_stability_OK = false;
                    if (mydata.LocalStability) // use local stability?
                    {
                        string stabilityfilename = Path.Combine(mydata.Path_GRAMMwindfield, Convert.ToString(iiwet).PadLeft(5, '0') + ".scl");
                        ReadStablity.FileName = stabilityfilename;
                        local_stability_OK = ReadStablity.ReadSclFile(); // Read entire file
                    }

                    int item_number = 0;
                    foreach (Point_3D item in mydata.EvalPoints)
                    {
                        //MessageBox.Show(item.X.ToString() + "/" +item.Y.ToString() +"/" +item.Z.ToString() +"/"+ item.filename);
                        double xsi = item.X - mydata.GrammWest;
                        double eta = item.Y - mydata.GrammSouth;

                        //obtain indices of selected point
                        int ix = Convert.ToInt32(Math.Floor(xsi / mydata.GRAMMhorgridsize)) + 1;
                        int iy = Convert.ToInt32(Math.Floor(eta / mydata.GRAMMhorgridsize)) + 1;

                        if (ix > 0 && iy > 0 && ix < NX && iy < NY)
                        {
                            double schnittZ = item.Z;

                            //obtain index in the vertical direction
                            for (int k = 1; k <= NZ; k++)
                            {
                                if (ZSP[ix, iy, k] - AH[ix, iy] >= schnittZ)
                                {
                                    ischnitt = k;
                                    break;
                                }
                            }

                            if (mydata.LocalStability && local_stability_OK) // use local stability?
                            {
                                int result = ReadStablity.SclMean(ix - 1, iy - 1); // get local SCL
                                if (result > 0) // valid result
                                {
                                    local_akla[item_number][n] = result;
                                }
                                else
                                {
                                    local_akla[item_number][n] = iakla[n];
                                }
                            }
                            else // use global stability
                            {
                                local_akla[item_number][n] = iakla[n];
                            }

                            Uoben = UWI[ix, iy, ischnitt];
                            Voben = VWI[ix, iy, ischnitt];
                            if (ischnitt > 1)
                            {
                                Uunten = UWI[ix, iy, ischnitt - 1];
                                Vunten = VWI[ix, iy, ischnitt - 1];
                                Umittel = Uunten + (Uoben - Uunten) / (ZSP[ix, iy, ischnitt] - ZSP[ix, iy, ischnitt - 1]) *
                                    (schnittZ + AH[ix, iy] - ZSP[ix, iy, ischnitt - 1]);
                                Vmittel = Vunten + ((Voben - Vunten) / (ZSP[ix, iy, ischnitt] - ZSP[ix, iy, ischnitt - 1]) *
                                    (schnittZ + AH[ix, iy] - ZSP[ix, iy, ischnitt - 1]));
                            }
                            else
                            {
                                Umittel = Uoben / (ZSP[ix, iy, ischnitt] - AH[ix, iy]) * schnittZ;
                                Vmittel = Voben / (ZSP[ix, iy, ischnitt] - AH[ix, iy]) * schnittZ;
                            }

                            iwr[item_number][n] = WindDirection(Umittel, Vmittel);
                            wgi[item_number][n] = (float)Math.Sqrt(Umittel * Umittel + Vmittel * Vmittel);
                        }
                        item_number++;
                    }
                }
                n++; // next line
            } // loop over all met situations
            ReadStablity = null; // Release memory of SCL file
            Reader = null; //Release windfield reader

            double wge = 0;
            double wr = 0;
            int ak = 0;
            string [] month=new string[2];
            
            //loop over mettimeseries.dat
            SetText("Writing meteofile");
            Thread.Sleep(500); // short delay
            
            int item_count = 0;
            foreach(Point_3D item in mydata.EvalPoints)
            {
                int fictiousyear = Math.Max(1900, mydata.FictiousYear);
                int monthold = -1;

                string file = Path.Combine(mydata.ProjectName, @"Metfiles", Path.GetFileName(item.FileName));
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch{}
                }

                using (StreamWriter mywriter = new StreamWriter(file, false))
                {
                    // write header lines
                    mywriter.WriteLine(@"//"+Path.GetFileName(file));
                    mywriter.WriteLine(@"//X=" + item.X.ToString(ic));
                    mywriter.WriteLine(@"//Y=" + item.Y.ToString(ic));
                    mywriter.WriteLine(@"//Z=" + item.Z.ToString(ic));
                    
                    foreach(string mettimeseries in data_mettimeseries)
                    {
                        try
                        {
                            text = mettimeseries.Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            wge = Convert.ToDouble(text[2].Replace(".", mydata.DecSep));
                            wr = Convert.ToDouble(text[3].Replace(".", mydata.DecSep));
                            ak = Convert.ToInt32(text[4]);
                            //new year
                            month = text[0].Split(new char[] { '.',':','-' }, StringSplitOptions.RemoveEmptyEntries);
                            if (Convert.ToInt32(month[1]) < monthold)
                            {
                                fictiousyear += 1;
                            }

                            monthold = Convert.ToInt32(month[1]);
                        }
                        catch
                        {
                            break;
                        }
                        
                        for (int ipgt = 1; ipgt <= data_meteopgt.Count; ipgt++)
                        {
                            if((Math.Abs(wge - wg[ipgt]) < 0.01) && (Math.Abs(wr - nsektor[ipgt]) < 0.01) && (ak == iakla[ipgt])) // find corresponding meteo file
                            {
                                mywriter.WriteLine(text[0] + "."+Convert.ToString(fictiousyear, ic)+"," + text[1] + ":00," + Convert.ToString(Math.Round(wgi[item_count][ipgt],2), ic) + "," + Convert.ToString(iwr[item_count][ipgt], ic) + "," + Convert.ToString(local_akla[item_count][ipgt], ic));
                                break;
                            }
                        }
                    }
                    
                } // using
                
                item_count++;
            } // loop over all receptor points
            
        }
    }
}