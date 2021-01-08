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
using System.Globalization;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace GralIO
{
    /////////////////////////////////////
    /// routine to generate GRAMM grid
    /////////////////////////////////////

    /// <summary>
    /// This class creates the GRAMM grid
    /// </summary>
    public class CreateGrammGrid
    {
        private bool _writeCompressedFile = false;
        public bool WriteCompressedFile { set { _writeCompressedFile = value; } }
        public int SmoothBorderCellNr;
        public string ProjectName;

        /// <summary>
        /// Generates the ggeom.asc file
        /// </summary> 
        public bool GenerateGgeomFile()
        {
            GralMessage.Waitprogressbar wait = new GralMessage.Waitprogressbar("Reading GRAMM topography" + "0 %");
            wait.Show();

            //User defined column seperator and decimal seperator
            string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            char[] splitchar = null;
            if (decsep == ",")
            {
                splitchar = new char[] { ' ', '\t', ';' };
            }
            else
            {
                splitchar = new char[] { ' ', '\t', ',', ';' };
            }

            GralMessage.MessageWindow mw = new GralMessage.MessageWindow();

            //reading field sizes from file GRAMM.geb
            int GrammNx = 0, GrammNY = 0, GrammNz = 0;
            int GrammWest = 0, GrammEast = 0, GrammSouth = 0, GrammNorth = 0;
            double DDZ = 0, ADZ = 0;
            string TopoFileName = string.Empty;
            try
            {
                using (StreamReader StReader = new StreamReader(Path.Combine(ProjectName, @"Computation", "GRAMM.geb")))
                {
                    string[] line = StReader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                    GrammNx = Convert.ToInt32(line[0]);  //number of horizontal cells in x direction
                    line = StReader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                    GrammNY = Convert.ToInt32(line[0]);  //number of horizontal cells in y direction
                    line = StReader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                    GrammNz = Convert.ToInt32(line[0]);  //number of vertical cells in z direction
                    line = StReader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                    GrammWest = Convert.ToInt32(line[0].Replace(".", decsep)); //western boarder
                    line = StReader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                    GrammEast = Convert.ToInt32(line[0].Replace(".", decsep)); //eastern boarder
                    line = StReader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                    GrammSouth = Convert.ToInt32(line[0].Replace(".", decsep)); //southern boarder
                    line = StReader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                    GrammNorth = Convert.ToInt32(line[0].Replace(".", decsep)); //northern boarder
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading Gramm.geb" + Environment.NewLine + ex.Message.ToString(), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                wait.Close();
                wait.Dispose();
                return false;
            }

            //reading the filename, the height of the lowest cell height, and the vertical streching factor
            try
            {
                using (StreamReader StReader = new StreamReader(Path.Combine(ProjectName, @"Computation", "geom.in")))
                {
                    TopoFileName = StReader.ReadLine();  //filename of the chosen topography
                    string[] line = StReader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                    DDZ = Convert.ToDouble(line[0].Replace(".", decsep)); //cell height of the first level
                    line = StReader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                    ADZ = Convert.ToDouble(line[0].Replace(".", decsep)); //vertical streching factor
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading geom.in" + Environment.NewLine + ex.Message.ToString(), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                wait.Close();
                wait.Dispose();
                return false;
            }

            int NX1 = GrammNx + 1, NY1 = GrammNY + 1, NZ1 = GrammNz + 1, NX2 = GrammNx + 2, NY2 = GrammNY + 2, NZ2 = GrammNz + 2;

            double[] XKO = new double[NX2];  //x-coordinates of cells
            double[] YKO = new double[NY2];  //y-coordinates of cells
            double[] ZKO = new double[NZ2];  //z-coordinates of cells
            double[,] AH = new double[NX1, NY1]; //height of the topography at each cell point
            double[,,] VOL = new double[NX1, NY1, NZ1]; //volume of each cell
            double[,,] AREAX = new double[NX2, NY1, NZ1]; //western cell face
            double[,,] AREAY = new double[NX1, NY2, NZ1]; //eastern cell face
            double[,,] AREAZ = new double[NX1, NY1, NZ2]; //bottom cell face
            double[,,] AREAZX = new double[NX1, NY1, NZ2]; //x-projection of bottom cell face
            double[,,] AREAZY = new double[NX1, NY1, NZ2]; //y-projection of bottom cell face
            double[,,] AHE = new double[NX2, NY2, NZ2]; //heights of the corner points of a cell
            double[,,] ZSP = new double[NX1, NY1, NZ1]; //height of the center of cells
            double[] DDX = new double[NX1]; //cell size in x-direction
            double[] DDY = new double[NY1]; //cell size in y-direction
            double[] ZAX = new double[NX1]; //x-coordinate of cell center
            double[] ZAY = new double[NY1]; //y-coordinate of cell center
            double[] X = new double[NX2]; //x-coordinate of cell faces
            double[] Y = new double[NY2]; //y-coordinate of cell faces
            double[] DX = new double[NX1]; //cell size for each cell in x-direction
            double[] DY = new double[NY1]; //cell size for each cell in y-direction
            double[] Z = new double[NZ2]; //absolute cell height for each cell in z-direction
            double[] DW = new double[NZ2]; //cell height for each cell in z-direction
            double[,,] AREAXYZ = new double[NX1, NY1, NZ1]; //area of intersecting face between two half-cells
            double[,,] AREA = new double[NX1, NY1, NZ1]; //area of bottom face
            double NODDATA = 0;

            //reading topography file
            wait.Show();
            wait.TopLevel = true;
            double GRAMMDeltaX = (GrammEast - GrammWest) / GrammNx;
            (float[][] ADH, EsriGridHeader Header) = ReadEsriTerrainFile(TopoFileName, GrammWest, GrammNorth, GrammEast, GrammSouth, wait);

            // Remove NoData Values
            ADH = RemoveNoDataValues(ADH, Header);

            double GrammDeltaX = (GrammEast - GrammWest) / GrammNx;

            // Terrain interpolation
            Gral.TerrainInterpolation TerrIpl = new Gral.TerrainInterpolation(ADH, Header.XllCorner, Header.NorthernBorder, Header.CellSize, Header.CellSize);
            
            //interpolate heigt values for the GRAMM grid edge points
            for (int xInd = 1; xInd <= NX1; xInd++)
            {
                for (int yInd = 1; yInd <= NY1; yInd++)
                {
                    double x = GrammWest  + (xInd - 1) * GrammDeltaX;
                    double y = GrammSouth + (yInd - 1) * GrammDeltaX;
                    double height;
                    if (Header.CellCenter)
                    {
                        height = TerrIpl.InterpolationCenter(x, y);
                    }
                    else
                    {
                        height = TerrIpl.InterpolationBorder(x, y);
                    }
                    AHE[xInd, yInd, 1] = height;
                }
            } 

            //computation of cell heights
            double AHMIN = 10000;
            double AHMAX = -10000;
            double AHMIN_BORDER = 10000;
            for (int x = 0; x < Header.NCols; x++)
            {
                for (int y = 0; y < Header.NRows; y++)
                {
                    //minimum of terrain data
                    if (ADH[x][y] < AHMIN)
                    {
                        AHMIN = ADH[x][y];
                    }

                    //minimum elevation at the border
                    if ((ADH[x][y] < AHMIN_BORDER) && ((x == 0) || (y == 0) || (x == Header.NCols) || (y == Header.NRows)))
                    {
                        AHMIN_BORDER = ADH[x][y];
                    }
                }
            }

            //flatten topography towards domain borders
            /*
            for(int I=2;I<NX+1;I++)
            {
                AHE[I,3,1]    = AHE[I,4,1];
                AHE[I,2,1]    = AHE[I,3,1];
                AHE[I,1,1]    = AHE[I,2,1];
                AHE[I,NY-1,1] = AHE[I,NY-2,1];
                AHE[I,NY,1]   = AHE[I,NY-1,1];
                AHE[I,NY+1,1] = AHE[I,NY,1];
            }
            for(int J=1;J<NY+2;J++)
            {
                AHE[3,J,1]    = AHE[4,J,1];
                AHE[2,J,1]    = AHE[3,J,1];
                AHE[1,J,1]    = AHE[2,J,1];
                AHE[NX-1,J,1] = AHE[NX-2,J,1];
                AHE[NX,J,1]   = AHE[NX-1,J,1];
                AHE[NX+1,J,1] = AHE[NX,J,1];
            }
             */


            //coordinates of cells in x-direction
            int NK = GrammNz;
            for (int I = 1; I < GrammNx + 1; I++)
            {
                DDX[I] = Convert.ToDouble(GrammDeltaX);
            }

            for (int J = 1; J < GrammNY + 1; J++)
            {
                DDY[J] = Convert.ToDouble(GrammDeltaX);
            }

            X[1] = 0;
            for (int I = 2; I < GrammNx + 2; I++)
            {
                X[I] = X[I - 1] + DDX[I - 1];
            }

            for (int I = 1; I < GrammNx; I++)
            {
                ZAX[I] = (X[I + 1] + DDX[I + 1] / 2) - (X[I] + DDX[I] / 2);
            }

            //flatten topography towards domain borders
            double abstand = 0;
            double totaldistance = 0; //horizontal distance between model boundary and the last cell, which is smoothed, yet
            for (int smooth = Math.Min(SmoothBorderCellNr, GrammNY); smooth > 0; smooth--)
            {
                totaldistance += DDY[smooth];
            }

            if (SmoothBorderCellNr > 0)
            {
                for (int I = 1; I < GrammNx + 2; I++)
                {
                    Application.DoEvents(); // Kuntner
                    abstand = 0;
                    for (int smooth = Math.Min(SmoothBorderCellNr, GrammNY); smooth > 0; smooth--)
                    {
                        //AHE[I, smooth, 1] = AHE[I, smooth + 1, 1] - (AHE[I, smooth + 1, 1] - AHMIN) / 4;

                        //lineare Interpolation zum Rand hin
                        abstand += DDY[smooth];
                        AHE[I, smooth, 1] = AHE[I, SmoothBorderCellNr + 1, 1] - (AHE[I, SmoothBorderCellNr + 1, 1] - AHMIN_BORDER) * abstand / totaldistance;
                    }
                    /*
                    AHE[I, 6, 1] = AHE[I, 7, 1] - (AHE[I, 7, 1] - AHMIN) / 4;
                    AHE[I, 5, 1] = AHE[I, 6, 1] - (AHE[I, 7, 1] - AHMIN) / 4;
                    AHE[I, 4, 1] = AHE[I, 5, 1] - (AHE[I, 7, 1] - AHMIN) / 4;
                    AHE[I, 3, 1] = AHMIN;
                    AHE[I, 2, 1] = AHMIN;
                    AHE[I, 1, 1] = AHMIN;
                     */

                    abstand = 0;
                    for (int smooth = Math.Min(SmoothBorderCellNr, GrammNY); smooth > 0; smooth--)
                    {
                        //AHE[I, NY - smooth + 2, 1] = AHE[I, NY - smooth + 1, 1] - (AHE[I, NY - smooth + 1, 1] - AHMIN) / 4;

                        //lineare Interpolation zum Rand hin
                        abstand += DDY[GrammNY - smooth + 1];
                        AHE[I, GrammNY - smooth + 2, 1] = AHE[I, GrammNY - SmoothBorderCellNr + 1, 1] - (AHE[I, GrammNY - SmoothBorderCellNr + 1, 1] - AHMIN_BORDER) * abstand / totaldistance;
                    }

                    /*
                    AHE[I, NY - 4, 1] = AHE[I, NY - 5, 1] - (AHE[I, NY - 5, 1] - AHMIN) / 4;
                    AHE[I, NY - 3, 1] = AHE[I, NY - 4, 1] - (AHE[I, NY - 5, 1] - AHMIN) / 4;
                    AHE[I, NY - 2, 1] = AHE[I, NY - 3, 1] - (AHE[I, NY - 5, 1] - AHMIN) / 4;
                    AHE[I, NY - 1, 1] = AHMIN;
                    AHE[I, NY, 1] = AHMIN;
                    AHE[I, NY + 1, 1] = AHMIN;
                     */
                }


                totaldistance = 0;
                for (int smooth = Math.Min(SmoothBorderCellNr, GrammNx); smooth > 0; smooth--)
                {
                    totaldistance += DDX[smooth];
                }

                for (int J = 1; J < GrammNY + 2; J++)
                {
                    Application.DoEvents(); // Kuntner
                    abstand = 0;
                    for (int smooth = Math.Min(SmoothBorderCellNr, GrammNx); smooth > 0; smooth--)
                    {
                        //AHE[smooth, J, 1] = AHE[smooth + 1, J, 1] - (AHE[smooth + 1, J, 1] - AHMIN) / 4;

                        //lineare Interpolation zum Rand hin
                        abstand += DDX[smooth];
                        AHE[smooth, J, 1] = AHE[SmoothBorderCellNr + 1, J, 1] - (AHE[SmoothBorderCellNr + 1, J, 1] - AHMIN_BORDER) * abstand / totaldistance;
                    }

                    /*
                    AHE[6, J, 1] = AHE[7, J, 1] - (AHE[7, J, 1] - AHMIN) / 4;
                    AHE[5, J, 1] = AHE[6, J, 1] - (AHE[7, J, 1] - AHMIN) / 4;
                    AHE[4, J, 1] = AHE[5, J, 1] - (AHE[7, J, 1] - AHMIN) / 4;
                    AHE[3, J, 1] = AHMIN;
                    AHE[2, J, 1] = AHMIN;
                    AHE[1, J, 1] = AHMIN;
                     */

                    abstand = 0;
                    for (int smooth = Math.Min(SmoothBorderCellNr, GrammNx); smooth > 0; smooth--)
                    {
                        //AHE[NX - smooth + 2, J, 1] = AHE[NX - smooth + 1, J, 1] - (AHE[NX - smooth + 1, J, 1] - AHMIN) / 4;

                        //lineare Interpolation zum Rand hin
                        abstand += DDY[GrammNY - smooth + 1];
                        AHE[GrammNx - smooth + 2, J, 1] = AHE[GrammNx - SmoothBorderCellNr + 1, J, 1] - (AHE[GrammNx - SmoothBorderCellNr + 1, J, 1] - AHMIN_BORDER) * abstand / totaldistance;
                    }

                    /*
                    AHE[NX - 4, J, 1] = AHE[NX - 5, J, 1] - (AHE[NX - 5, J, 1] - AHMIN) / 4;
                    AHE[NX - 3, J, 1] = AHE[NX - 4, J, 1] - (AHE[NX - 5, J, 1] - AHMIN) / 4;
                    AHE[NX - 2, J, 1] = AHE[NX - 3, J, 1] - (AHE[NX - 5, J, 1] - AHMIN) / 4;
                    AHE[NX - 1, J, 1] = AHMIN;
                    AHE[NX, J, 1] = AHMIN;
                    AHE[NX + 1, J, 1] = AHMIN;
                     */
                }
            }

            //using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, "AHE.txt")))
            //{
            //    int k = 1;
            //    for (int j = 1; j < NY1; j++)
            //    {
            //        for (int i = 1; i < NX1; i++)
            //        {
            //            writer.Write(Convert.ToString(Math.Round(AHE[i, j, k], 2)) + " ");
            //        }
            //        writer.WriteLine();
            //    }
            //}

            //minimum and maximum elevations
            for (int J = 1; J < GrammNY + 2; J++)
            {
                for (int I = 1; I < GrammNx + 2; I++)
                {
                    if (AHE[I, J, 1] < AHMIN)
                    {
                        AHMIN = AHE[I, J, 1];
                    }

                    if (AHE[I, J, 1] > AHMAX)
                    {
                        AHMAX = AHE[I, J, 1];
                    }
                }
            }
            mw.listBox1.Items.Add("Minimum elevation: " + Convert.ToString(Math.Round(AHMIN, 0)));
            mw.listBox1.Items.Add("Maximum elevation: " + Convert.ToString(Math.Round(AHMAX, 0)));
            mw.Show();

            mw.listBox1.Items.Add("Number of cells in E-W direction: " + Convert.ToString(GrammNx));
            mw.listBox1.Items.Add("Cell length in E-W direction: " + Convert.ToString(DDX[1]));
            mw.listBox1.Items.Add("E-W extents: " + Convert.ToString(Math.Round(X[GrammNx + 1], 0)));
            mw.Refresh();

            //coordinates of cells in y-direction
            Y[1] = 0;
            for (int J = 2; J < GrammNY + 2; J++)
            {
                Y[J] = Y[J - 1] + DDY[J - 1];
            }

            for (int J = 1; J < GrammNY; J++)
            {
                ZAY[J] = (Y[J + 1] + DDY[J + 1] / 2) - (Y[J] + DDY[J] / 2);
            }

            mw.listBox1.Items.Add("Number of cells in S-N direction: " + Convert.ToString(GrammNY));
            mw.listBox1.Items.Add("Cell length in S-N direction: " + Convert.ToString(DDY[1]));
            mw.listBox1.Items.Add("S-N extents: " + Convert.ToString(Math.Round(Y[GrammNY + 1], 0)));
            mw.Refresh();

            //coordinates of cells in z-direction
            Z[1] = AHMIN;
            int K = 1;
            mw.listBox1.Items.Add("Z" + Convert.ToString(K) + "= " + Convert.ToString(Math.Round(Z[K], 1)) + " m");
            mw.Refresh();
            for (K = 2; K < GrammNz + 2; K++)
            {
                Z[K] = Z[K - 1] + DDZ * Math.Pow(ADZ, K - 2);
            }

            for (K = 1; K < GrammNz + 1; K++)
            {
                DW[K] = Z[K + 1] - Z[K];
                mw.listBox1.Items.Add("Z" + Convert.ToString(K + 1) + "= " + Convert.ToString(Math.Round(Z[K + 1], 1)) + " m");
            }
            mw.Refresh();

            //top of model domain needs to be larger than 3 times the maximum elevation
            if ((Z[GrammNz + 1] - AHMIN) < (AHMAX - AHMIN) * 3)
            {
                MessageBox.Show("Height of the model domain is too low.\rIncrease vertical streching factor or\rIncrease the number of vertical grid points or\rIncrease the height of the first layer", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                wait.Close();
                wait.Dispose();
                return false;
            }

            double DWMAX = Z[GrammNz + 1] - AHMIN;

            //computation of the heights of the cell corners
            for (int I = 1; I < GrammNx + 2; I++)
            {
                for (int J = 1; J < GrammNY + 2; J++)
                {
                    for (K = 2; K < GrammNz + 2; K++)
                    {
                        AHE[I, J, K] = AHE[I, J, K - 1] + DW[K - 1] / DWMAX * (Z[GrammNz + 1] - AHE[I, J, 1]);
                    }
                }
            }

            //computation of areas and volumes
            for (int I = 1; I < GrammNx + 2; I++)
            {
                for (int J = 1; J < GrammNY + 1; J++)
                {
                    for (K = 1; K < GrammNz + 1; K++)
                    {
                        AREAX[I, J, K] = (AHE[I, J, K + 1] - AHE[I, J, K] + AHE[I, J + 1, K + 1] - AHE[I, J + 1, K]) * 0.5 * DDY[J];
                    }
                }
            }

            for (int I = 1; I < GrammNx + 1; I++)
            {
                for (int J = 1; J < GrammNY + 2; J++)
                {
                    for (K = 1; K < GrammNz + 1; K++)
                    {
                        AREAY[I, J, K] = (AHE[I, J, K + 1] - AHE[I, J, K] + AHE[I + 1, J, K + 1] - AHE[I + 1, J, K]) * 0.5 * DDX[I];
                    }
                }
            }

            for (int I = 1; I < GrammNx + 1; I++)
            {
                for (int J = 1; J < GrammNY + 1; J++)
                {
                    for (K = 1; K < GrammNz + 2; K++)
                    {
                        AREAZX[I, J, K] = ((AHE[I, J + 1, K] - AHE[I + 1, J + 1, K]) + (AHE[I, J, K] - AHE[I + 1, J, K])) * 0.5 * DDY[J];
                        AREAZY[I, J, K] = ((AHE[I + 1, J, K] - AHE[I + 1, J + 1, K]) + (AHE[I, J, K] - AHE[I, J + 1, K])) * 0.5 * DDX[I];
                        AREAZ[I, J, K] = Math.Sqrt(DDX[I] * DDX[I] * DDY[J] * DDY[J] + AREAZX[I, J, K] * AREAZX[I, J, K] + AREAZY[I, J, K] * AREAZY[I, J, K]);
                    }
                }
            }

            for (int I = 1; I < GrammNx + 1; I++)
            {
                for (int J = 1; J < GrammNY + 1; J++)
                {
                    for (K = 1; K < GrammNz + 1; K++)
                    {
                        VOL[I, J, K] = ((2.0 * AHE[I, J, K + 1] + AHE[I + 1, J, K + 1] + 2.0 * AHE[I + 1, J + 1, K + 1] + AHE[I, J + 1, K + 1]) - (2.0 * AHE[I, J, K] + AHE[I + 1, J, K] + 2.0 * AHE[I + 1, J + 1, K] + AHE[I, J + 1, K])) / 6.0 * DDX[I] * DDY[J];
                        ZSP[I, J, K] = (AHE[I, J, K + 1] + AHE[I + 1, J, K + 1] + AHE[I + 1, J + 1, K + 1] + AHE[I, J + 1, K + 1] + AHE[I, J, K] + AHE[I + 1, J, K] + AHE[I + 1, J + 1, K] + AHE[I, J + 1, K]) / 8.0;
                    }
                    AH[I, J] = (AHE[I, J, 1] + AHE[I + 1, J, 1] + AHE[I + 1, J + 1, 1] + AHE[I, J + 1, 1]) / 4.0;
                }
            }

            //write ggeom.asc
            GGeomFileIO ggeom = new GGeomFileIO
            {
                PathWindfield = Path.Combine(ProjectName, @"Computation", "ggeom.asc"),
                ProjectName = ProjectName,
                WriteCompressedFile = _writeCompressedFile,
                NX = GrammNx,
                NY = GrammNY,
                NZ = GrammNz,
                AH = AH,
                X = X,
                Y = Y,
                Z = Z,
                VOL = VOL,
                AREAX = AREAX,
                AREAY = AREAY,
                AREAZ = AREAZ,
                AREAZX = AREAZX,
                AREAZY = AREAZY,
                ZSP = ZSP,
                DDX = DDX,
                DDY = DDY,
                ZAX = ZAX,
                ZAY = ZAY,
                IKOOA = GrammWest,
                JKOOA = GrammSouth,
                Winkel = 0,
                AHE = AHE,
                NODATA = NODDATA
            };
            ggeom.WriteGGeomFile();

            wait.Close();
            wait.Dispose();

            return true;
        }

        private (float[][], EsriGridHeader) ReadEsriTerrainFile(string FileName, double West, double North, double East, double South, GralMessage.Waitprogressbar Wait)
        {
            float[][] ADH = null;
            EsriGridHeader Header = null;

            try
            {
                using (StreamReader reader = new StreamReader(FileName))
                {
                    Header = new EsriGridHeader();
                    if (Header.ReadEsriGridHeader(reader))
                    {
                        //check, if domain fits within selected topography file
                        if (Header.XllCorner > West)
                        {
                            MessageBox.Show("Selected area is to far in the west", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            throw new Exception();
                        }
                        if (Header.EasternBorder < East)
                        {
                            MessageBox.Show("Selected area is to far in the east", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            throw new Exception();
                        }
                        if (Header.YllCorner > South)
                        {
                            MessageBox.Show("Selected area is to far in the south", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            throw new Exception();
                        }
                        if (Header.NorthernBorder < North)
                        {
                            MessageBox.Show("Selected area is to far in the north", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            throw new Exception();
                        }

                        // Read needed area of topography file to ADH[][]
                        int TerrWestIndex = (int)Math.Max(0, ((West - Header.XllCorner) / Header.CellSize) - 2);
                        int TerrEastIndex = (int)Math.Min(Header.NCols, ((East - Header.XllCorner) / Header.CellSize) + 2);
                        int TerrNorthIndex = (int)Math.Max(0, ((Header.NorthernBorder - North) / Header.CellSize) - 2);
                        int TerrSouthIndex = (int)Math.Min(Header.NRows, ((Header.NorthernBorder - South) / Header.CellSize) + 2);

                        if (TerrEastIndex <= TerrWestIndex || TerrSouthIndex <= TerrNorthIndex)
                        {
                            throw new Exception();
                        }

                        ADH = Landuse.CreateArray<float[]>(TerrEastIndex - TerrWestIndex, () => new float[TerrSouthIndex - TerrNorthIndex]);

                        // Read the height Data
                        char[] splitChar = new char[] { ' ', '\t', ';' };
                        int yPart = 0;
                        for (int y = 0; y < TerrSouthIndex; y++)
                        {
                            string Line = reader.ReadLine();

                            //inside the GRAMM domain?
                            if (y >= TerrNorthIndex)
                            {
                                string[] text = Line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                                int xPart = 0;
                                for (int x = TerrWestIndex; x < TerrEastIndex; x++)
                                {
                                    ADH[xPart][yPart] = Convert.ToSingle(text[x], CultureInfo.InvariantCulture);
                                    xPart++;
                                }
                                yPart++;
                            }

                            if (y % 40 == 0)
                            {
                                Wait.Text = "Reading GRAMM topography " + ((int)((float)y / Header.NRows * 100F)).ToString() + "%";
                            }
                        }

                        Wait.Text = "Reading GRAMM topography 100 %";

                        // Set header to the reduced ADH size
                        double north = Header.NorthernBorder;
                        Header.EasternBorder = Header.XllCorner + TerrEastIndex * Header.CellSize;
                        Header.NorthernBorder = Header.NorthernBorder - TerrNorthIndex * Header.CellSize;
                        Header.XllCorner = Header.XllCorner + TerrWestIndex * Header.CellSize;
                        Header.YllCorner = north - TerrSouthIndex * Header.CellSize;
                        Header.NCols = TerrEastIndex - TerrWestIndex;
                        Header.NRows = TerrSouthIndex - TerrNorthIndex;
                    }
                    else
                    {
                        Header = null;
                    }
                }
            }
            catch
            {
                ADH = null;
                Header = null;
            }
            return (ADH, Header);
        }

        private float[][] RemoveNoDataValues(float[][] ADH, EsriGridHeader Header)
        {
            double TotalWeighting, Weighting, Value;

            //computation of missing mean value from the 4 corners
            for (int x = 0; x < Header.NCols; x++)
            {
                for (int y = 0; y < Header.NRows; y++)
                {
                    if (Math.Abs(ADH[x][y] - Header.NoDataValue) < 0.1) // invalid value
                    {
                        TotalWeighting = 0;
                        Weighting = 0;
                        Value = 0;
                        //seeking north/south
                        for (int NorthSouth = y + 1; NorthSouth < Header.NRows; NorthSouth++)
                        {
                            if (Math.Abs(ADH[x][NorthSouth] - Header.NoDataValue) > 0.1) // found a valid value
                            {
                                Weighting = Math.Abs(1 / ((NorthSouth - y) * Header.CellSize));
                                TotalWeighting += Weighting;
                                Value = ADH[x][NorthSouth] * Weighting + Value;
                                break;
                            }
                        }
                        //seeking north/south
                        for (int NorthSouth = y - 1; NorthSouth > 0; NorthSouth--)
                        {
                            if (Math.Abs(ADH[x][NorthSouth] - Header.NoDataValue) > 0.1) // found a valid value
                            {
                                Weighting = Math.Abs(1 / ((NorthSouth - y) * Header.CellSize));
                                TotalWeighting += Weighting;
                                Value = ADH[x][NorthSouth] * Weighting + Value;
                                break;
                            }
                        }
                        //seeking west/east
                        for (int EastWest = x + 1; EastWest < Header.NCols; EastWest++)
                        {
                            if (Math.Abs(ADH[EastWest][y] - Header.NoDataValue) > 0.1) // found a valid value
                            {
                                Weighting = Math.Abs(1 / ((EastWest - x) * Header.CellSize));
                                TotalWeighting += Weighting;
                                Value = ADH[EastWest][y] * Weighting + Value;
                                break;
                            }
                        }
                        //seeking west/east
                        for (int EastWest = x - 1; EastWest > 0; EastWest--)
                        {
                            if (Math.Abs(ADH[EastWest][y] - Header.NoDataValue) > 0.1) // found a valid value
                            {
                                Weighting = Math.Abs(1 / ((EastWest - x) * Header.CellSize));
                                TotalWeighting += Weighting;
                                Value = ADH[EastWest][y] * Weighting + Value;
                                break;
                            }
                        }

                        //calculating total weight
                        ADH[x][y] = (float)Math.Max(Value / TotalWeighting, 0);
                    }
                }
            }
            return ADH;
        }

    }
}
