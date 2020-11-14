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
 * Time: 18:16
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;
using GralIO;

namespace GralBackgroundworkers
{
    public partial class ProgressFormBackgroundworker
    {
        /// <summary>
        /// Calculate the mean wind velocity for a GRAMM wind field
        /// </summary>
        private void MeanWindVelocity(GralBackgroundworkers.BackgroundworkerData mydata,
                                      System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                SetText("Reading ggeom.asc");
                //reading geometry file "ggeom.asc"
                GGeomFileIO ggeom = new GGeomFileIO
                {
                    PathWindfield = mydata.Path_GRAMMwindfield
                };

                double[,] AH = new double[1, 1];
                double[,,] ZSP = new double[1, 1, 1];
                int NX = 1;
                int NY = 1;
                int NZ = 1;
                string[] text = new string[1];
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
                else
                {
                    throw new FileNotFoundException("Error reading ggeom.asc");
                }

                double schnitt = mydata.Schnitt;
                //obtain index in the vertical direction
                for (int k = 1; k <= NZ; k++)
                {
                    if (ZSP[2, 2, k] - AH[2, 2] >= schnitt)
                    {
                        ischnitt = k;
                        break;
                    }
                }

                SetText("Reading meteopgt.all");
                System.Threading.Thread.Sleep(200);

                // read meteopgt
                List<string> data_meteopgt = new List<string>();
                ReadMeteopgtAll(Path.Combine(mydata.Path_GRAMMwindfield, @"meteopgt.all"), ref data_meteopgt);
                if (data_meteopgt.Count == 0) // no data available
                {
                    BackgroundThreadMessageBox("Can't read meteopgt.all");
                }

                //windfield file Readers
                //PSFReader windfield = new PSFReader(form1.GRAMMwindfield, true);
                //StreamReader meteopgt = new StreamReader(Path.Combine(Path.GetDirectoryName(mydata.Path_GRAMMwindfield), @"meteopgt.all"));
                //loop over all weather situations
                double Uoben = 0;
                double Voben = 0;
                double Uunten = 0;
                double Vunten = 0;
                double Umittel = 0;
                double Vmittel = 0;
                double[] frequ = new double[data_meteopgt.Count + 2];

                float[,,] UWI = new float[NX + 1, NY + 1, NZ + 1];
                float[,,] VWI = new float[NX + 1, NY + 1, NZ + 1];
                float[,,] WWI = new float[NX + 1, NY + 1, NZ + 1];
                double[,] UMEAN = new double[NX + 1, NY + 1];

                double totalfrequ = 0;
                int iiwet = 0;
                foreach (string line_meteopgt in data_meteopgt)
                {
                    iiwet++;

                    if (Rechenknecht.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    if (iiwet % 4 == 0)
                    {
                        Rechenknecht.ReportProgress((int)(iiwet / (double)data_meteopgt.Count * 100D));
                    }

                    try
                    {
                        text = line_meteopgt.Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        frequ[iiwet] = Convert.ToDouble(text[3].Replace(".", mydata.Decsep));
                    }
                    catch
                    {
                        break;
                    }

                    try
                    {
                        //read wind fields
                        SetText("Reading windfield nr." + Convert.ToString(iiwet));
                        string wndfilename = Path.Combine(Path.GetDirectoryName(mydata.Path_GRAMMwindfield), Convert.ToString(iiwet).PadLeft(5, '0') + ".wnd");
                        Windfield_Reader Reader = new Windfield_Reader();
                        if (Reader.Windfield_read(wndfilename, NX, NY, NZ, ref UWI, ref VWI, ref WWI) == false)
                        {
                            throw new IOException();
                        }

                    }
                    catch
                    {
                        break;
                    }

                    for (int i = 1; i <= NX; i++)
                    {
                        for (int j = 1; j <= NY; j++)
                        {
                            Uoben = UWI[i, j, ischnitt];
                            Voben = VWI[i, j, ischnitt];
                            if (ischnitt > 1)
                            {
                                Uunten = UWI[i, j, ischnitt - 1];
                                Vunten = VWI[i, j, ischnitt - 1];
                                Umittel = Uunten + (Uoben - Uunten) / (ZSP[i, j, ischnitt] - ZSP[i, j, ischnitt - 1]) *
                                    (schnitt + AH[i, j] - ZSP[i, j, ischnitt - 1]);
                                Vmittel = Vunten + (Voben - Vunten) / (ZSP[i, j, ischnitt] - ZSP[i, j, ischnitt - 1]) *
                                    (schnitt + AH[i, j] - ZSP[i, j, ischnitt - 1]);
                            }
                            else
                            {
                                Umittel = Uoben / (ZSP[i, j, ischnitt] - AH[i, j]) * schnitt;
                                Vmittel = Voben / (ZSP[i, j, ischnitt] - AH[i, j]) * schnitt;
                            }
                            UMEAN[i, j] = UMEAN[i, j] + Math.Sqrt(Umittel * Umittel + Vmittel * Vmittel) * frequ[iiwet] / 1000;
                        }
                    }

                    totalfrequ += frequ[iiwet] / 1000;
                }
                //windfield.Close();
                //meteopgt.Close();

                //save result to file
                SetText("SaveFileDialog results");

                string file = mydata.Filename;
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }
                using (StreamWriter myWriter = new StreamWriter(file))
                {
                    myWriter.WriteLine("ncols         " + Convert.ToString(NX));
                    myWriter.WriteLine("nrows         " + Convert.ToString(NY));
                    myWriter.WriteLine("Xllcorner     " + Convert.ToString(mydata.GrammWest));
                    myWriter.WriteLine("Yllcorner     " + Convert.ToString(mydata.GrammSouth));
                    myWriter.WriteLine("cellsize      " + Convert.ToString(mydata.GRAMMhorgridsize));
                    myWriter.WriteLine("NODATA_value  " + "-9999" + "\t UNIT \t m/s");
                    double min = 1000;
                    double max = -1;
                    for (int j = NY; j > 0; j--)
                    {
                        for (int i = 1; i <= NX; i++)
                        {
                            myWriter.Write(Convert.ToString(Math.Round(UMEAN[i, j] / totalfrequ, 3), ic) + ",");
                            min = Math.Min(min, UMEAN[i, j]);
                            max = Math.Max(max, UMEAN[i, j]);
                        }
                        myWriter.WriteLine();
                    }
                }
            }
            catch
            {
                BackgroundThreadMessageBox("Compute Mean Wind Velocity IO Error");
                return;
            }
            Computation_Completed = true; // set flag, that computation was successful
        }
    }
}
