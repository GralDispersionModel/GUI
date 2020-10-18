#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2020]  [Markus Kuntner]
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
        /// Generate one or multiple meteo-file(s) from GRAL windfields
        /// </summary>
        private void GenerateGRALMeteofile(GralBackgroundworkers.BackgroundworkerData mydata,
                                       System.ComponentModel.DoWorkEventArgs e)
        {
            bool transientMode = CheckForTransientMode(mydata.Projectname);
            List<string> meteoTimeSeries = new List<string>();
            List<string> meteoPGTALL = new List<string>();
            ReadMettimeseries(Path.Combine(mydata.Projectname, "Computation", @"mettimeseries.dat"), ref meteoTimeSeries);
            ReadMeteopgtAll(Path.Combine(mydata.Projectname, "Computation", @"meteopgt.all"), ref meteoPGTALL);

            if (meteoPGTALL.Count == 0) // no data available
            {
                BackgroundThreadMessageBox("Error reading meteorological input");
                return;
            }

            // GRAMM project -> use Stability Class from GRAMM 
            GRALGeometry GRAMMgeom = null;
            bool useLocalStabilityClass = false;
            if (!string.IsNullOrEmpty(mydata.Path_GRAMMwindfield))
            {
                if (mydata.LocalStability) // use local stability?
                {
                    useLocalStabilityClass = true;
                }
                GRAMMgeom = ReadGRAMMGeometry(mydata.Path_GRAMMwindfield);
            }

            //Create Arrays for each evaluation point
            int[][] locSCL = GralIO.Landuse.CreateArray<int[]>(mydata.EvalPoints.Count, () => new int[meteoPGTALL.Count + 5]);
            float[][] windVel = GralIO.Landuse.CreateArray<float[]>(mydata.EvalPoints.Count, () => new float[meteoPGTALL.Count + 5]);
            float[][] windDir = GralIO.Landuse.CreateArray<float[]>(mydata.EvalPoints.Count, () => new float[meteoPGTALL.Count + 5]);
            GRALGeometry GRALGeom = new GRALGeometry();

            // Read GRAL Geometries
            List<EvalPointsIndices> evalPoints = ReadGRALGeometries(GRALGeom, mydata.EvalPoints, mydata);

            //loop over all weather situations
            string gffPath = GralStaticFunctions.St_F.GetGffFilePath(Path.Combine(mydata.Projectname, "Computation"));

            ReadFlowFieldFiles gff = new ReadFlowFieldFiles();
            ReadSclUstOblClasses ReadStability = new ReadSclUstOblClasses();
            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();

            // gff files are always available for meteopgt.all situations
            for (int meteoSit = 1; meteoSit <= meteoPGTALL.Count; meteoSit++)
            {
                SetText("Processing meteo situation nr. " + meteoSit.ToString());
                if (Rechenknecht.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (meteoSit % 4 == 0)
                {
                    Rechenknecht.ReportProgress((int)(meteoSit / (double)meteoPGTALL.Count * 100D));
                }

                gff.filename = Path.Combine(gffPath, Convert.ToString(meteoSit).PadLeft(5, '0') + ".gff");
                if (gff.ReadGffFile(cts.Token) == true)
                {
                    int ptCount = 0;

                    foreach (EvalPointsIndices _pt in evalPoints)
                    {
                        if (_pt.IxGRAL > 0 && _pt.IxGRAL < gff.NII &&
                            _pt.IyGRAL > 0 && _pt.IyGRAL < gff.NJJ &&
                            _pt.IzGRAL > 0 && _pt.IzGRAL < gff.NKK)
                        {
                            float uk = gff.Uk[_pt.IxGRAL][_pt.IyGRAL][_pt.IzGRAL];
                            float vk = gff.Vk[_pt.IxGRAL][_pt.IyGRAL][_pt.IzGRAL];
                            float wk = gff.Wk[_pt.IxGRAL][_pt.IyGRAL][_pt.IzGRAL];

                            windVel[ptCount][meteoSit - 1] = (float)Math.Sqrt(uk * uk + vk * vk);
                            windDir[ptCount][meteoSit - 1] = WindDirection(uk, vk);
                        }
                        int scl = gff.AK;
                        if (useLocalStabilityClass)
                        {
                            scl = ReadGRAMMStability(_pt.IxGRAMM, _pt.IyGRAMM, meteoSit, scl, mydata.Path_GRAMMwindfield, ReadStability);
                        }
                        locSCL[ptCount][meteoSit - 1] = scl;
                        ptCount++;
                    }
                }
            }
            gff = null;
            ReadStability = null;

            //write result files
            int ptNumber = 0;
            foreach (Point_3D item in mydata.EvalPoints)
            {
                string file = Path.Combine(mydata.Projectname, @"Metfiles", Path.GetFileName(item.filename));
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }

                int fictiousyear = 1901;
                int sitCount = 1;
                string[] text;
                int monthold = 1;
                using (StreamWriter mywriter = new StreamWriter(file, false))
                {
                    // write header lines
                    mywriter.WriteLine(@"//" + Path.GetFileName(file));
                    mywriter.WriteLine(@"//X=" + item.X.ToString(ic));
                    mywriter.WriteLine(@"//Y=" + item.Y.ToString(ic));
                    mywriter.WriteLine(@"//Z=" + item.Z.ToString(ic));
                    foreach (string mettimeseries in meteoTimeSeries)
                    {
                        try
                        {
                            text = mettimeseries.Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            //new year
                            string[] month = text[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
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
                        string result = text[0] + "." + Convert.ToString(fictiousyear, ic) + "," + text[1] + ":00,";

                        int cmpSit = SearchCorrespondingMeteopgtAllSituation(meteoTimeSeries, meteoPGTALL, sitCount - 1);
                        result += Convert.ToString(Math.Round(windVel[ptNumber][cmpSit], 2), ic) + "," + Convert.ToString(windDir[ptNumber][cmpSit], ic) + "," + Convert.ToString(locSCL[ptNumber][cmpSit], ic);
                        
                        mywriter.WriteLine(result);
                        sitCount++;
                    }
                }
                ptNumber++;
            }
        }

        /// <summary>
        /// Search a corresponding meteopgt.all situation for an index in the mettime series
        /// </summary>
        /// <param name="MetTimeSeries">Mettime series</param>
        /// <param name="MeteoPgtALL">Meteopgt.all data</param>
        /// <param name="MetTimeSeriesIndex">Index in the mettime series</param>
        /// <returns>Index in meteopgt.all matching the mettime series at the index MetTimeSeriesIndex</returns>
        private int SearchCorrespondingMeteopgtAllSituation(List<string> MetTimeSeries, List<string> MeteoPgtALL, int MetTimeSeriesIndex)
        {
            int MeteopgtIndex = 0;
            string[] text;
            double wVel = 0, wDir = 0;
            int wSCL = 0;
            if (MetTimeSeriesIndex < MetTimeSeries.Count)
            {
                text = MetTimeSeries[MetTimeSeriesIndex].Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (text.Length > 4)
                {
                    wVel = Convert.ToDouble(text[2], ic);
                    wDir = Convert.ToDouble(text[3], ic);
                    wSCL = Convert.ToInt32(text[4]);
                }
            }

            for (int cmpSit = 0; cmpSit < MeteoPgtALL.Count; cmpSit++)
            {
                text = MeteoPgtALL[cmpSit].Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (text.Length > 2)
                {
                    double cmpDir = Convert.ToDouble(text[0], ic);
                    double cmpVel = Convert.ToDouble(text[1], ic);
                    int cmpSCL = Convert.ToInt32(text[2]);

                    if ((Math.Abs(wVel - cmpVel) < 0.05) && (Math.Abs(wDir - cmpDir) < 0.05) && (wSCL == cmpSCL)) // find corresponding meteo file
                    {
                        MeteopgtIndex = cmpSit;
                        break;
                    }
                }
            }

            return MeteopgtIndex;
        }

        /// <summary>
        /// Check if the calculation was in steady state or transient mode
        /// </summary>
        /// <param name="ProjectPath"></param>
        /// <returns>true in transient mode</returns>
        private bool CheckForTransientMode(string ProjectPath)
        {
            bool transient = false;
            try
            {
                InDatVariables data = new InDatVariables();
                InDatFileIO ReadInData = new InDatFileIO();
                data.InDatPath = Path.Combine(ProjectPath, "Computation", "in.dat");
                ReadInData.Data = data;
                if (ReadInData.ReadInDat() == true)
                {
                    if (data.Transientflag == 0)
                    {
                        transient = true;
                    }
                }
            }
            catch (Exception ex)
            {
                BackgroundThreadMessageBox(ex.Message);
            }
            return transient;
        }

        /// <summary>
        /// Read the GRAL geometry and get the horizontal and vertical indices for all evaluation points
        /// </summary>
        /// <param name="Geom">GRAL geometry basic data</param>
        /// <param name="EvalPoints">Coordinates of all evalution points</param>
        /// <param name="mydata">Data with GFF grid size</param>
        /// <returns>List with grid data for all evaluation points</returns>
        private List<EvalPointsIndices> ReadGRALGeometries(GRALGeometry Geom, List<Point_3D> EvalPoints, BackgroundworkerData mydata)
        {
            List<EvalPointsIndices> EvalPointIndices = new List<EvalPointsIndices>();

            Geom.StretchFlexible = new List<float[]>();

            //reading geometry file "GRAL_geometries.txt"
            try
            {
                string filename = Path.Combine(Gral.Main.ProjectName, @"Computation", "GRAL_geometries.txt");
                if (File.Exists(filename))
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
                    {
                        Geom.NZ = reader.ReadInt32();
                        Geom.NY = reader.ReadInt32();
                        Geom.NX = reader.ReadInt32();
                        int GRALwest = reader.ReadInt32();
                        int GRALsouth = reader.ReadInt32();
                        Geom.DZK = reader.ReadSingle();
                        Geom.Stretch = reader.ReadSingle();
                        if (Geom.Stretch < 0.1)
                        {
                            int sliceCount = reader.ReadInt32();

                            for (int i = 0; i < sliceCount; i++)
                            {
                                Geom.StretchFlexible.Add(new float[2]);
                                Geom.StretchFlexible[i][0] = reader.ReadSingle(); // Height
                                Geom.StretchFlexible[i][1] = reader.ReadSingle(); // Stretch
                            }
                        }
                        Geom.AHMin = reader.ReadSingle();

                        // Create GRAL Eval horizontal point indices
                        foreach (Point_3D _pt in EvalPoints)
                        {
                            EvalPointsIndices _newPoint = new EvalPointsIndices();
                            _newPoint.IxGRAL = Convert.ToInt32(Math.Floor((_pt.X - GRALwest) / mydata.GFFGridSize)) + 1;
                            _newPoint.IyGRAL = Convert.ToInt32(Math.Floor((_pt.Y - GRALsouth) / mydata.GFFGridSize)) + 1;
                            _newPoint.Height = _pt.Z;

                            // GRAMM indices
                            if (! string.IsNullOrEmpty(mydata.Path_GRAMMwindfield))
                            {
                                double xsi = _pt.X - mydata.GrammWest;
                                double eta = _pt.Y - mydata.GrammSouth;
                                //obtain indices of selected point
                                _newPoint.IxGRAMM = Convert.ToInt32(Math.Floor(xsi / mydata.GRAMMhorgridsize)) + 1;
                                _newPoint.IyGRAMM = Convert.ToInt32(Math.Floor(eta / mydata.GRAMMhorgridsize)) + 1;
                            }

                            EvalPointIndices.Add(_newPoint);
                        }

                        // Create vertical point indices
                        float[] HOKART = CalculateVerticalHeightSlices(Geom);
                        for (int i = 1; i <= Geom.NX + 1; i++)
                        {
                            for (int j = 1; j <= Geom.NY + 1; j++)
                            {
                                float AH = reader.ReadSingle();
                                int KKart = (short)reader.ReadInt32();
                                float Building_heights = reader.ReadSingle();

                                foreach (EvalPointsIndices _pt in EvalPointIndices)
                                {
                                    if (i == _pt.IxGRAL && j == _pt.IyGRAL)
                                    {
                                        for (int k = 1; k <= Geom.NZ; k++)
                                        {
                                            // Point height > Terrain 
                                            if (HOKART[k] + Geom.AHMin >= AH + _pt.Height) //check if point is above ground level
                                            {
                                                _pt.IzGRAL = k;
                                                k = Geom.NZ + 2; // break
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return EvalPointIndices;
        }

        /// <summary>
        /// Calulate the vertical height slices for the flexible vertical GRAL grid
        /// </summary>
        /// <param name="Geom">GRAL geometry basic data</param>
        /// <returns>Array with the height slices for the flow field grid</returns>
        private float[] CalculateVerticalHeightSlices(GRALGeometry Geom)
        {
            //computation of slice
            Single[] HOKART = new Single[Geom.NZ + 1];  //height of vertical layers starting from zero
            Single DZKdummy = Geom.DZK;
            int flexstretchindex = 0;
            float stretching = 1;

            for (int k = 1; k <= Geom.NZ; k++)
            {
                HOKART[k] = HOKART[k - 1] + DZKdummy;

                if (Geom.Stretch > 0.99)
                {
                    DZKdummy *= Geom.Stretch;
                }
                else
                {
                    if (flexstretchindex < Geom.StretchFlexible.Count - 1)
                    {
                        if (Geom.StretchFlexible[flexstretchindex + 1][1] > 0.99 &&
                            HOKART[k - 1] > Geom.StretchFlexible[flexstretchindex + 1][0])
                        {
                            stretching = Geom.StretchFlexible[flexstretchindex + 1][1];
                            flexstretchindex++;
                        }
                    }
                    DZKdummy *= stretching;
                }
            }
            return HOKART;
        }

        /// <summary>
        /// Read the GRAMM geometry
        /// </summary>
        /// <param name="GgeomPath"></param>
        /// <returns>Number of cells in the GRAMM grid</returns>
        private GRALGeometry ReadGRAMMGeometry(string GgeomPath)
        {
            GRALGeometry GRAMMGeom = null;

            //reading geometry file "ggeom.asc"
            GGeomFileIO ggeom = new GGeomFileIO
            {
                PathWindfield = GgeomPath
            };
           
            if (ggeom.ReadGGeomAsc(0) == true)
            {
                GRAMMGeom = new GRALGeometry();
                GRAMMGeom.NX = ggeom.NX;
                GRAMMGeom.NY = ggeom.NY;
                GRAMMGeom.NZ = ggeom.NZ;
                ggeom = null;
            }
            return GRAMMGeom;
        }

        /// <summary>
        /// Calculate the wind direction
        /// </summary>
        /// <param name="Umittel">Wind U component</param>
        /// <param name="Vmittel">Wind V component</param>
        /// <returns>Wind direction in degrees</returns>
        private float WindDirection(float Umittel, float Vmittel)
        {
            float wr = 0;
            if (Vmittel == 0)
            {
                wr = 90;
            }
            else
            {
                wr = Convert.ToInt32(Math.Abs(Math.Atan(Umittel / Vmittel)) * 180 / 3.14);
            }

            if ((Vmittel > 0) && (Umittel <= 0))
            {
                wr = 180 - wr;
            }

            if ((Vmittel >= 0) && (Umittel > 0))
            {
                wr = 180 + wr;
            }

            if ((Vmittel < 0) && (Umittel >= 0))
            {
                wr = 360 - wr;
            }
            return wr;
        }

        /// <summary>
        /// Read the local GRAMM stability class
        /// </summary>
        /// <param name="X">X cell of the GRAMM grid</param>
        /// <param name="Y">Y cell of the GRAMM grid</param>
        /// <param name="Situation">Number of the weather situation</param>
        /// <param name="SCL">Stability class preset</param>
        /// <param name="GRAMMPath">Path to the GRAMM wind field data</param>
        /// <param name="ReadStability">Stability class reader</param>
        /// <returns>Local stability class at (x,y)</returns>
        private int ReadGRAMMStability(int X, int Y, int Situation, int SCL, string GRAMMPath, ReadSclUstOblClasses ReadStability)
        {
            string stabilityfilename = Path.Combine(GRAMMPath, Convert.ToString(Situation).PadLeft(5, '0') + ".scl");
            ReadStability.FileName = stabilityfilename;
            if (ReadStability.ReadSclFile()) // Read entire file
            {
                int result = ReadStability.SclMean(X - 1, Y - 1); // get local SCL
                if (result > 0) // valid result
                {
                    SCL = result;
                }
            }
            return SCL;
        }

        /// <summary>
        /// Stores the indices for an evaluation point
        /// </summary>
        private class EvalPointsIndices
        {
            /// <summary>
            /// x index in the flow field grid
            /// </summary>
            public int IxGRAL { get; set; }
            /// <summary>
            /// y index in the flow field grid
            /// </summary>
            public int IyGRAL { get; set; }
            /// <summary>
            /// z index in the flow field grid
            /// </summary>
            public int IzGRAL { get; set; }
            /// <summary>
            /// x index in the GRAMM grid
            /// </summary>
            public int IxGRAMM { get; set; }
            /// <summary>
            /// y index in the GRAMM grid
            /// </summary>
            public int IyGRAMM { get; set; }
            /// <summary>
            /// Relative height above ground
            /// </summary>
            public double Height { get; set; }
        }

        /// <summary>
        /// Stores the basic GRAL geometry
        /// </summary>
        private class GRALGeometry
        {
            /// <summary>
            /// Number of cells in x direction
            /// </summary>
            public int NX { get; set; }
            /// <summary>
            /// Number of cells in y direction
            /// </summary>
            public int NY { get; set; }
            /// <summary>
            /// Number of cells in z direction
            /// </summary>
            public int NZ { get; set; }
            /// <summary>
            /// Minimum terrain height inside the domain area
            /// </summary>
            public float AHMin { get; set; }
            /// <summary>
            /// Height of the 1st vertical cell
            /// </summary>
            public float DZK { get; set; }
            /// <summary>
            /// Stretching factor for vertical cells
            /// </summary>
            public float Stretch { get; set; }
            /// <summary>
            /// Flexible stretching factors for vertical cells
            /// </summary>
            public List<float[]> StretchFlexible { get; set; }

        }
    }
}