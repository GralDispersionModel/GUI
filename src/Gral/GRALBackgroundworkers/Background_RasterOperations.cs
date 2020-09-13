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
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Drawing;

namespace GralBackgroundworkers
{
    public partial class ProgressFormBackgroundworker
    {
        /// <summary>
        /// Mathematical raster operations
        /// </summary>
        private void MathRasterOperation(GralBackgroundworkers.BackgroundworkerData mydata,
                                         System.ComponentModel.DoWorkEventArgs e)
        {
            double[][] A = null;
            double[][] B = null;
            double[][] C = null;
            double[][] D = null;
            double[][] G = null;
            double[][] F = null;
            ESRIHeader[] _fileHeader = new ESRIHeader[5]; // Header for File A to E
            string[] FileName = new string[5] { mydata.RasterA, mydata.RasterB, mydata.RasterC, mydata.RasterD, mydata.RasterE };


            if (string.IsNullOrEmpty(mydata.RasterF))
            {
                BackgroundThreadMessageBox("No output raster has been defined");
                return;
            }
            if (string.IsNullOrEmpty(mydata.Unit))
            {
                BackgroundThreadMessageBox("No unit has been defined");
                return;
            }

            // Read all ESRI files
            for (int i = 0; i < 5; i++)
            {
                if (!string.IsNullOrEmpty(FileName[i]))
                {
                    if (i == 0)
                    {
                        SetText("Reading Raster A");
                        A = ReadESRIFile(FileName[i], ref _fileHeader[i]);
                    }
                    else if (i == 1)
                    {
                        SetText("Reading Raster B");
                        B = ReadESRIFile(FileName[i], ref _fileHeader[i]);
                        if (B == null)
                            BackgroundThreadMessageBox("Error when reading raster B");
                    }
                    else if (i == 2)
                    {
                        SetText("Reading Raster C");
                        C = ReadESRIFile(FileName[i], ref _fileHeader[i]);
                        if (C == null)
                            BackgroundThreadMessageBox("Error when reading raster C");
                    }
                    else if (i == 3)
                    {
                        SetText("Reading Raster D");
                        D = ReadESRIFile(FileName[i], ref _fileHeader[i]);
                        if (D == null)
                            BackgroundThreadMessageBox("Error when reading raster D");
                    }
                    else if (i == 4)
                    {
                        SetText("Reading Raster G");
                        G = ReadESRIFile(FileName[i], ref _fileHeader[i]);
                        if (G == null)
                            BackgroundThreadMessageBox("Error when reading raster G");
                    }
                }
            }

            // Check if raster A is available
            if (A != null)
            {
                // create result raster
                F = CreateArray<double[]>(_fileHeader[0].NCols, () => new double[_fileHeader[0].NRows]);

                // calculate
                SetText("Performing calculations");

                for (int x = 0; x < _fileHeader[0].NCols; x++)
                {
                    if (Rechenknecht.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    if (x % 50 == 0)
                    {
                        Rechenknecht.ReportProgress((int)(x / (double)_fileHeader[0].NCols * 100D));
                    }

                    Parallel.ForEach(Partitioner.Create(0, _fileHeader[0].NRows, Math.Max(4, (int)(_fileHeader[0].NRows / Environment.ProcessorCount))), range =>
                    {
                        double[] variables = new double[5];
                        GralDomain.PointD PointCoors = new GralDomain.PointD();
                        bool inside = true;

                        MathParserMathos.MathParserMathos parser = new MathParserMathos.MathParserMathos(true, true, true, false);
                        for (int y = range.Item1; y < range.Item2; y++)
                        //for (int y = 0; y < _fileHeader[0].NRows; y++)
                        {
                            Point PointIndex = new Point(x, y);
                            //BackgroundThreadMessageBox(PointIndex.X.ToString() + "/" + PointIndex.Y.ToString());

                            if (A != null)
                            {
                                variables[0] = A[x][y];
                                PointCoors = ESRICellCoors(PointIndex, _fileHeader[0]);
                                
                                if (B != null)
                                {
                                    PointIndex = ESRIGetIndex(PointCoors, _fileHeader[1]);
                                    inside = CheckIndexInArray(PointIndex, _fileHeader[1]);
                                    if (inside)
                                    {
                                        variables[1] = B[PointIndex.X][PointIndex.Y];
                                    }
                                }
                                if (C != null && inside)
                                {
                                    PointIndex = ESRIGetIndex(PointCoors, _fileHeader[2]);
                                    inside = CheckIndexInArray(PointIndex, _fileHeader[2]);
                                    if (inside)
                                    {
                                        variables[2] = C[PointIndex.X][PointIndex.Y];
                                    }
                                }
                                if (D != null && inside)
                                {
                                    PointIndex = ESRIGetIndex(PointCoors, _fileHeader[3]);
                                    inside = CheckIndexInArray(PointIndex, _fileHeader[3]);
                                    if (inside)
                                    {
                                        variables[3] = D[PointIndex.X][PointIndex.Y];
                                    }
                                }
                                if (G != null && inside)
                                {
                                    PointIndex = ESRIGetIndex(PointCoors, _fileHeader[4]);
                                    inside = CheckIndexInArray(PointIndex, _fileHeader[4]);
                                    if (inside)
                                    {
                                        variables[4] = G[PointIndex.X][PointIndex.Y];
                                    }
                                }
                            }

                            string mathtext = mydata.TextBox1.Replace(mydata.Decsep, ".");
                            if (inside)
                            {
                                F[x][y] = parser.Parse(mathtext, variables);
                            }
                            else
                            {
                                F[x][y] = -9999;
                            }
                        }
                    });
                }


                SetText("Writing output file");

                GralIO.WriteESRIFile _writer = new GralIO.WriteESRIFile();
                _writer.CellSize = _fileHeader[0].Cellsize;
                _writer.NCols = _fileHeader[0].NCols;
                _writer.NRows = _fileHeader[0].NRows;
                _writer.XllCorner = _fileHeader[0].XllCorner;
                _writer.YllCorner = _fileHeader[0].YllCorner;
                _writer.Round = 8;
                _writer.FileName = mydata.RasterF;
                _writer.Unit = mydata.Unit;

                if (_writer.WriteJaggedDblArrResult(F) == false)
                {
                    BackgroundThreadMessageBox("Error when writing the result file");
                    return;
                }
                _writer = null;

                Computation_Completed = true; // set flag, that computation was successful
            }
            else
            {
                BackgroundThreadMessageBox("Error when reading raster A");
            }
        }

        /// <summary>
        /// Check if an index is within an array
        /// </summary>
        /// <param name="pt">Index point</param>
        /// <param name="header">ESRI file header</param>
        /// <returns>true: valid index, false: invalid index</returns>
        bool CheckIndexInArray(Point pt, ESRIHeader header)
        {
            if (pt.X < 0 || pt.Y < 0 || pt.X >= header.NCols || pt.Y >= header.NRows)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read an ESRI ASCii File 
        /// </summary>
        /// <param name="Filename"></param>
        /// <param name="Header"></param>
        /// <returns>Array and Header object</returns>
        private double[][] ReadESRIFile(string Filename, ref ESRIHeader Header)
        {
            double[][] A = null;
            Header = new ESRIHeader();
            try
            {
                string[] data;
                using (StreamReader myReader = new StreamReader(Filename))
                {
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    Header.NCols = Convert.ToInt32(data[1]);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    Header.NRows = Convert.ToInt32(data[1]);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    Header.XllCorner = Convert.ToDouble(data[1], ic);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    Header.YllCorner = Convert.ToDouble(data[1], ic);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    Header.Cellsize = Convert.ToDouble(data[1], ic);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    Header.NoDataValue = Convert.ToDouble(data[1]);

                    int col = Header.NCols;
                    int Row = Header.NRows;

                    A = CreateArray<double[]>(col, () => new double[Row]);
                    for (int y = 0; y < Header.NRows; y++)
                    {
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int x = 0; x < Header.NCols; x++)
                        {
                            A[x][y] = Convert.ToDouble(data[x], ic);
                        }
                    }
                }
            }
            catch
            {
                A = null;
            }

            return A;
        }

        /// <summary>
        /// Calculate an index within an ESRI grid 
        /// </summary>
        /// <param name="Coors">Coordinate of the checked point</param>
        /// <param name="Header">ESRI file header info</param>
        /// <returns>Index within an ESRI grid may be less zero or larger ncols or nrows</returns>
        private Point ESRIGetIndex(GralDomain.PointD Coors, ESRIHeader Header)
        {
            Point pt = new Point
            {
                X = (int)((Coors.X - Header.XllCorner) / Header.Cellsize),
                Y = Header.NRows - (int)((Coors.Y - Header.YllCorner) / Header.Cellsize)
            };
            return pt;
        }

        /// <summary>
        /// Calculate the center coordinates of a cell of an ESRI grid
        /// </summary>
        /// <param name="Index">Integer indes fo a cell in the ESRI grid</param>
        /// <param name="Header">ESRI file header info</param>
        /// <returns>Center coordinates of a cell in the ESRI grid</returns>
        private GralDomain.PointD ESRICellCoors(Point Index, ESRIHeader Header)
        {
            GralDomain.PointD pt = new GralDomain.PointD
            {
                X = Header.XllCorner + Index.X * Header.Cellsize + 0.5 * Header.Cellsize,
                Y = Header.YllCorner + (Header.NRows - Index.Y) * Header.Cellsize + 0.5 * Header.Cellsize
            };
            return pt;
        }

        /// <summary>
        /// Class for the ESRI file header info
        /// </summary>
        private class ESRIHeader
        {
            private int _ncols = 0;
            private int _nrows = 0;
            private double _xllCorner = 0;
            private double _yllCorner = 0;
            private double _cellSize = 1;
            private double _noDataVal = -9999;

            public int NCols { get => _ncols; set => _ncols = value; }
            public int NRows { get => _nrows; set => _nrows = value; }
            public double XllCorner { get => _xllCorner; set => _xllCorner = value; }
            public double YllCorner { get => _yllCorner; set => _yllCorner = value; }
            public double Cellsize { get => _cellSize; set => _cellSize = value; }
            public double NoDataValue { get => _noDataVal; set => _noDataVal = value; }
        }

    }
}
