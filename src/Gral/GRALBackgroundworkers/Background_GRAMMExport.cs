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
using GralIO;

namespace GralBackgroundworkers
{
    public partial class ProgressFormBackgroundworker
    {
		/// <summary>
        /// Export a sub domain of a GRAMM wind field
        /// </summary>
        private void GRAMMExport(GralBackgroundworkers.BackgroundworkerData mydata,
                                      System.ComponentModel.DoWorkEventArgs e)
        {
            try
            { 
                float[,,] UWI = new float[mydata.GRAMMCells.NX + 1, mydata.GRAMMCells.NY + 1, mydata.GRAMMCells.NZ + 1];
                float[,,] VWI = new float[mydata.GRAMMCells.NX + 1, mydata.GRAMMCells.NY + 1, mydata.GRAMMCells.NZ + 1];
                float[,,] WWI = new float[mydata.GRAMMCells.NX + 1, mydata.GRAMMCells.NY + 1, mydata.GRAMMCells.NZ + 1];
                double[,] stabclasses = new double[mydata.GRAMMCells.NX + 1, mydata.GRAMMCells.NY + 1];
                double[,] ustar = new double[mydata.GRAMMCells.NX + 1, mydata.GRAMMCells.NY + 1];
                double[,] MOlength = new double[mydata.GRAMMCells.NX + 1, mydata.GRAMMCells.NY + 1];

                //defining local variables for sub-domain extensions
                float[,,] subU = new float[mydata.GRAMMSubCells.NX + 1, mydata.GRAMMSubCells.NY + 1, mydata.GRAMMSubCells.NZ + 1];
                float[,,] subV = new float[mydata.GRAMMSubCells.NX + 1, mydata.GRAMMSubCells.NY + 1, mydata.GRAMMSubCells.NZ + 1];
                float[,,] subW = new float[mydata.GRAMMSubCells.NX + 1, mydata.GRAMMSubCells.NY + 1, mydata.GRAMMSubCells.NZ + 1];

                Windfield_Reader wfexport = new Windfield_Reader
                {
                    DDX = mydata.Horgridsize,
                    NI = mydata.GRAMMSubCells.NX,
                    NJ = mydata.GRAMMSubCells.NY,
                    NK = mydata.GRAMMSubCells.NZ
                };

                int lines = (int) GralStaticFunctions.St_F.CountLinesInFile(Path.Combine(mydata.Projectname, @"Computation", "meteopgt.all"));

                ReadSclUstOblClasses ReadStablity = new ReadSclUstOblClasses
                {
                    GRAMMhorgridsize = (float)mydata.Horgridsize
                };

                for (int dissit = 1; dissit < lines - 1; dissit++)
                {
                    if (Rechenknecht.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    if (dissit % 4 == 0 && lines > 0)
                    {
                       Rechenknecht.ReportProgress((int) (dissit / (double)lines * 100D));
                    }

                    string wndfilename = Path.Combine(mydata.Path_GRAMMwindfield, Convert.ToString(dissit).PadLeft(5, '0') + ".wnd");
                    if (File.Exists(wndfilename))
                    {
                        try
                        {
                            //read wind fields
                            if (wfexport.Windfield_read(wndfilename, mydata.GRAMMCells.NX, mydata.GRAMMCells.NY,
                                                        mydata.GRAMMCells.NZ, ref UWI, ref VWI, ref WWI) == false)
                            {
                                throw new IOException();
                            }

                            //export wind-fields and scl-files for sub-domain
                            int ni = mydata.XDomain;
                            int nj = mydata.YDomain;
                            for (int i = 1; i < mydata.GRAMMSubCells.NX + 1; i++)
                            {
                                for (int j = 1; j < mydata.GRAMMSubCells.NY + 1; j++)
                                {
                                    for (int k = 1; k < mydata.GRAMMSubCells.NZ + 1; k++)
                                    {
                                        subU[i, j, k] = UWI[ni, nj, k];
                                        subV[i, j, k] = VWI[ni, nj, k];
                                        subW[i, j, k] = WWI[ni, nj, k];
                                    }
                                    nj++;
                                }
                                ni++;
                                nj = mydata.YDomain; 
                            }

                            wfexport.U = subU;
                            wfexport.V = subV;
                            wfexport.W = subW;
                            wfexport.PathWindfield = Path.Combine(Path.Combine(mydata.Projectname, "Computation"), Convert.ToString(dissit).PadLeft(5, '0') + ".wnd");

                            wfexport.Windfield_export();
                            
                            //read scl-files
                            string stabilityfilename = Path.Combine(mydata.Path_GRAMMwindfield, Convert.ToString(dissit).PadLeft(5, '0') + ".scl");
                            if (File.Exists(stabilityfilename))
                            {
                                ReadStablity.FileName = stabilityfilename;

                                ReadStablity.ReadSclFile();

                                //export scl, ust, obl-files for sub-domain
                                ReadStablity.FileName = Path.Combine(mydata.Projectname, "Computation", Convert.ToString(dissit).PadLeft(5, '0'));
                                ReadStablity.X0 = Math.Max(0, mydata.XDomain - 1);
                                ReadStablity.Y0 = Math.Max(0, mydata.YDomain - 1);
                                ReadStablity.NX = Math.Max(0, mydata.XDomain + mydata.GRAMMSubCells.NX - 1);
                                ReadStablity.NY = Math.Max(0, mydata.YDomain + mydata.GRAMMSubCells.NY - 1);

                                if (ReadStablity.ExportSclFile() == false)
                                {
                                    
                                }
                                ReadStablity.X0 = 0;
                                ReadStablity.Y0 = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            BackgroundThreadMessageBox(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BackgroundThreadMessageBox(ex.Message);
            }

            Computation_Completed = true; // set flag, that computation was successful
        }
    }
}
