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
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
            double[][] A; 
            double[][] B;
            double[][] C;
            double[][] D;
            double[][] E;
            double[][] F;

            int nxA = 1;
            int nxB = 1;
            int nxC = 1;
            int nxD = 1;
            int nxE = 1;
            int nyA = 1;
            int nyB = 1;
            int nyC = 1;
            int nyD = 1;
            int nyE = 1;
            double dxA = 1;
            double dxB = 1;
            double dxC = 1;
            double dxD = 1;
            double dxE = 1;
            double x11A = 1;
            double x11B = 1;
            double x11C = 1;
            double x11D = 1;
            double x11E = 1;
            double y11A = 1;
            double y11B = 1;
            double y11C = 1;
            double y11D = 1;
            double y11E = 1;
            int nodataA = 1;
            int nodataB = 1;
            int nodataC = 1;
            int nodataD = 1;
            int nodataE = 1;
            int caseswitch = 0;

            //read raster A
            try
            {
                SetText("Reading Raster A");
                //Cursor = Cursors.WaitCursor;
                string[] data = new string[100];
                //open data of raster file
                //StreamReader myReader = new StreamReader(mydata.rasterA);
                using (StreamReader myReader = new StreamReader(mydata.RasterA))
                {
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    nxA = Convert.ToInt32(data[1]);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    nyA = Convert.ToInt32(data[1]);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    x11A = Convert.ToDouble(data[1], ic);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    y11A = Convert.ToDouble(data[1], ic);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    dxA = Convert.ToDouble(data[1], ic);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    nodataA = Convert.ToInt32(data[1]);
                    data = new string[nxA];
                    A = CreateArray<double[]>(nxA, () => new double[nyA]);
                    for (int i = nyA - 1; i > -1; i--)
                    {
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int j = 0; j < nxA; j++)
                        {
                            A[j][i] = Convert.ToDouble(data[j], ic);
                        }
                    }
                }
                //myReader.Close();
                //myReader.Dispose();

                SetText("Reading finished");
            }
            catch
            {
                BackgroundThreadMessageBox("Unable to read the data set A");
                return;
            }
            //Cursor = Cursors.Default;

            //read raster B
            if ((mydata.RasterB != null) && (mydata.RasterC == null) && (mydata.RasterD == null) && (mydata.RasterE == null))
            {
                try
                {
                    SetText("Reading Raster B");
                    //Cursor = Cursors.WaitCursor;
                    string[] data = new string[100];
                    //open data of raster file
                    //StreamReader myReader = new StreamReader(mydata.rasterB);

                    using (StreamReader myReader = new StreamReader(mydata.RasterB))
                    {
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        nxB = Convert.ToInt32(data[1]);
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        nyB = Convert.ToInt32(data[1]);
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        x11B = Convert.ToDouble(data[1], ic);
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        y11B = Convert.ToDouble(data[1], ic);
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        dxB = Convert.ToDouble(data[1], ic);
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        nodataB = Convert.ToInt32(data[1]);

                        //check if raster B is equal to raster A
                        if (nxB != nxA)
                        {
                            caseswitch = 1;
                        }

                        if (nyB != nyA)
                        {
                            caseswitch = 1;
                        }

                        if (Math.Abs(x11B - x11A) > 0.1 )
                        {
                            caseswitch = 1;
                        }

                        if (Math.Abs(y11B - y11A) > 0.1)
                        {
                            caseswitch = 1;
                        }

                        if (Math.Abs(dxB - dxA) > 0.1)
                        {
                            caseswitch = 1;
                        }

                        switch (caseswitch)
                        {
                            case 0:
                                data = new string[nxB];
                                B = CreateArray<double[]>(nxB, () => new double [nyB]);
                                F = CreateArray<double[]>(nxB, () => new double[nyB]);
                                //double result;
                                for (int i = nyB - 1; i > -1; i--)
                                {
                                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    for (int j = 0; j < nxB; j++)
                                    {
                                        B[j][i] = Convert.ToDouble(data[j], ic);
                                    }
                                }
                                //myReader.Close();
                                //myReader.Dispose();

                                SetText("Performing calculations");

                                for (int j = 0; j < nxB; j++)
                                {
                                    if (Rechenknecht.CancellationPending)
                                    {
                                        e.Cancel = true;
                                        return;
                                    }
                                    if (j % 50 == 0)
                                    {
                                        Rechenknecht.ReportProgress((int)(j / (double)nxB * 100D));
                                    }
                                    string valueA; string valueB;
                                    //Parallel.For(0, nyB, i =>
                                    //for (int i = nyB - 1; i > -1; i--)
                                    Parallel.ForEach(Partitioner.Create(0, nyB, Math.Max(4, (int)(nyB / Environment.ProcessorCount))), range =>
                                    {
                                        MathParserMathos.MathParserMathos parser = new MathParserMathos.MathParserMathos(true, true, true, false);
                                        for (int i = range.Item1; i < range.Item2; i++)
                                        {
                                            double a = A[j][i];
                                            double b = B[j][i];

                                            try
                                            {
                                                string mathtext = mydata.TextBox1.Replace(mydata.Decsep, ".");

                                                valueA = "(" + Convert.ToString(a, ic) + ")";
                                                valueB = "(" + Convert.ToString(b, ic) + ")";
                                                mathtext = mathtext.Replace("A", valueA).Replace("B", valueB);

                                                double result = parser.Parse(mathtext);

                                                F[j][i] = Convert.ToDouble(result);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    });
                                }


                                SetText("Writing output file");

                                //write result to output raster F
                                if (mydata.RasterF != null)
                                {
                                    string file = mydata.RasterF;
                                    if (File.Exists(file))
                                    {
                                        try
                                        {
                                            File.Delete(file);
                                        }
                                        catch { }
                                    }
                                    using (StreamWriter mywriter = new StreamWriter(file))
                                    {
                                        mywriter.WriteLine("ncols             " + Convert.ToString(nxA));
                                        mywriter.WriteLine("nrows             " + Convert.ToString(nyA));
                                        mywriter.WriteLine("xllcorner         " + Convert.ToString(x11A, ic));
                                        mywriter.WriteLine("yllcorner         " + Convert.ToString(y11A, ic));
                                        mywriter.WriteLine("cellsize          " + Convert.ToString(dxA, ic));
                                        if (mydata.Unit.Length > 0)
                                        {
                                            mywriter.WriteLine("NODATA_value      " + Convert.ToString(nodataA) + "\t" + "UNIT \t" + mydata.Unit);
                                        }
                                        else
                                        {
                                            mywriter.WriteLine("NODATA_value      " + Convert.ToString(nodataA));
                                        }

                                        for (int i = nyB - 1; i > -1; i--)
                                        {
                                            for (int j = 0; j < nxB; j++)
                                            {
                                                mywriter.Write(Convert.ToString(Math.Round(F[j][i], 3), ic) + " ");
                                            }
                                            mywriter.WriteLine();
                                        }
                                    }
                                }
                                else
                                {
                                    //BackgroundThreadMessageBox("No output raster has been defined");
                                    return;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch(Exception ex)
                {
                    BackgroundThreadMessageBox("Unable to read the data set B " + ex.Message.ToString());
                    return;
                }
                //Cursor = Cursors.Default;
            }

            //read raster B and read raster C
            if ((mydata.RasterB != null) && (mydata.RasterC != null) && (mydata.RasterD == null) && (mydata.RasterE == null))
            {
                try
                {
                    //Cursor = Cursors.WaitCursor;
                    string[] data = new string[100];
                    string[] dataC = new string[100];
                    //open data of raster B file
                    //StreamReader myReaderB = new StreamReader(mydata.rasterB);
                    using (StreamReader myReaderB = new StreamReader(mydata.RasterB))
                    {
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        nxB = Convert.ToInt32(data[1]);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        nyB = Convert.ToInt32(data[1]);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        x11B = Convert.ToDouble(data[1], ic);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        y11B = Convert.ToDouble(data[1], ic);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        dxB = Convert.ToDouble(data[1], ic);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        nodataB = Convert.ToInt32(data[1]);

                        //open data of raster C file
                        //StreamReader myReaderC = new StreamReader(mydata.rasterC);

                        using (StreamReader myReaderC = new StreamReader(mydata.RasterC))
                        {
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            nxC = Convert.ToInt32(data[1]);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            nyC = Convert.ToInt32(data[1]);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            x11C = Convert.ToDouble(data[1], ic);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            y11C = Convert.ToDouble(data[1], ic);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            dxC = Convert.ToDouble(data[1], ic);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            nodataC = Convert.ToInt32(data[1]);

                            //check if raster B and raster C are equal to raster A
                            if (nxB != nxA)
                            {
                                caseswitch = 1;
                            }

                            if (nyB != nyA)
                            {
                                caseswitch = 1;
                            }

                            if (Math.Abs(x11B - x11A) > 0.1 )
                            {
                                caseswitch = 1;
                            }

                            if (Math.Abs(y11B - y11A) > 0.1 )
                            {
                                caseswitch = 1;
                            }

                            if (Math.Abs(dxB - dxA) > 0.1 )
                            {
                                caseswitch = 1;
                            }

                            if (nxC != nxA)
                            {
                                caseswitch = 1;
                            }

                            if (nyC != nyA)
                            {
                                caseswitch = 1;
                            }

                            if (Math.Abs(x11C - x11A) > 0.1 )
                            {
                                caseswitch = 1;
                            }

                            if (Math.Abs(y11C - y11A) > 0.1 )
                            {
                                caseswitch = 1;
                            }

                            if (Math.Abs(x11C -  x11A) > 0.1 )
                            {
                                caseswitch = 1;
                            }

                            switch (caseswitch)
                            {
                                case 0:
                                    data = new string[nxB];
                                    dataC = new string[nxC];
                                    B = CreateArray<double[]>(nxB, () => new double[nyB]);
                                    C = CreateArray<double[]>(nxC, () => new double[nyC]);
                                    F = CreateArray<double[]>(nxA, () => new double[nyA]);

                                    SetText("Reading Raster B");
                                    for (int i = nyB - 1; i > -1; i--)
                                    {
                                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                        for (int j = 0; j < nxB; j++)
                                        {
                                            B[j][i] = Convert.ToDouble(data[j], ic);
                                        }
                                    }
                                    //myReaderB.Close();
                                    //myReaderB.Dispose();

                                    SetText("Reading Raster C");
                                    for (int i = nyB - 1; i > -1; i--)
                                    {
                                        dataC = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                        for (int j = 0; j < nxB; j++)
                                        {
                                            C[j][i] = Convert.ToDouble(dataC[j], ic);
                                        }
                                    }
                                    //myReaderC.Close();
                                    //myReaderC.Dispose();

                                    SetText("Performing calculations");
                                    //Parallel.For(0, nxB, j =>
                                    for (int j = 0; j < nxB; j++)
                                    {
                                        if (Rechenknecht.CancellationPending)
                                        {
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (j % 50 == 0)
                                        {
                                            Rechenknecht.ReportProgress((int)(j / (double)nxB * 100D));
                                        }
                                        string valueA; string valueB; string valueC;
                                        //for (int i = nyB - 1; i > -1; i--)
                                        //Parallel.For(0, nyB, i =>
                                        Parallel.ForEach(Partitioner.Create(0, nyB, Math.Max(4, (int)(nyB / Environment.ProcessorCount))), range =>
                                        {
                                            MathParserMathos.MathParserMathos parser = new MathParserMathos.MathParserMathos(true, true, true, false);
                                            for (int i = range.Item1; i < range.Item2; i++)
                                            {
                                                double a = A[j][i];
                                                double b = B[j][i];
                                                double c = C[j][i];
                                                try
                                                {
                                                    string mathtext = mydata.TextBox1.Replace(mydata.Decsep, ".");

                                                    valueA = "(" + Convert.ToString(a, ic) + ")";
                                                    valueB = "(" + Convert.ToString(b, ic) + ")";
                                                    valueC = "(" + Convert.ToString(c, ic) + ")";
                                                    mathtext = mathtext.Replace("A", valueA).Replace("B", valueB).Replace("C", valueC);
                                                    double result = parser.Parse(mathtext);
                                                    F[j][i] = Convert.ToDouble(result);
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        });
                                    }

                                    SetText("Writing output file");
                                    //write result to output raster F
                                    if (mydata.RasterF != null)
                                    {
                                        string file = mydata.RasterF;
                                        if (File.Exists(file))
                                        {
                                            try
                                            {
                                                File.Delete(file);
                                            }
                                            catch { }
                                        }

                                        using (StreamWriter mywriter = new StreamWriter(file))
                                        {
                                            mywriter.WriteLine("ncols             " + Convert.ToString(nxA));
                                            mywriter.WriteLine("nrows             " + Convert.ToString(nyA));
                                            mywriter.WriteLine("xllcorner         " + Convert.ToString(x11A, ic));
                                            mywriter.WriteLine("yllcorner         " + Convert.ToString(y11A, ic));
                                            mywriter.WriteLine("cellsize          " + Convert.ToString(dxA, ic));
                                            if (mydata.Unit.Length > 0)
                                            {
                                                mywriter.WriteLine("NODATA_value      " + Convert.ToString(nodataA) + "\t" + "UNIT \t" + mydata.Unit);
                                            }
                                            else
                                            {
                                                mywriter.WriteLine("NODATA_value      " + Convert.ToString(nodataA));
                                            }

                                            for (int i = nyB - 1; i > -1; i--)
                                            {
                                                for (int j = 0; j < nxB; j++)
                                                {
                                                    mywriter.Write(Convert.ToString(Math.Round(F[j][i], 3), ic) + " ");
                                                }
                                                mywriter.WriteLine();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        BackgroundThreadMessageBox("No output raster has been defined");
                                        return;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                catch
                {
                    BackgroundThreadMessageBox("Unable to read the data set B or C");
                    return;
                }
                //Cursor = Cursors.Default;
            }

            //read raster B and read raster C and read raster D
            if ((mydata.RasterB != null) && (mydata.RasterC != null) && (mydata.RasterD != null) && (mydata.RasterE == null))
            {
                try
                {
                    //Cursor = Cursors.WaitCursor;
                    string[] data = new string[100];
                    string[] dataC = new string[100];
                    string[] dataD = new string[100];
                    //open data of raster B file
                    //StreamReader myReaderB = new StreamReader(mydata.rasterB);
                    using (StreamReader myReaderB = new StreamReader(mydata.RasterB))
                    {
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        nxB = Convert.ToInt32(data[1]);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        nyB = Convert.ToInt32(data[1]);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        x11B = Convert.ToDouble(data[1], ic);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        y11B = Convert.ToDouble(data[1], ic);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        dxB = Convert.ToDouble(data[1], ic);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        nodataB = Convert.ToInt32(data[1]);

                        //open data of raster C file
                        //StreamReader myReaderC = new StreamReader(mydata.rasterC);
                        using (StreamReader myReaderC = new StreamReader(mydata.RasterC))
                        {
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            nxC = Convert.ToInt32(data[1]);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            nyC = Convert.ToInt32(data[1]);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            x11C = Convert.ToDouble(data[1], ic);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            y11C = Convert.ToDouble(data[1], ic);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            dxC = Convert.ToDouble(data[1], ic);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            nodataC = Convert.ToInt32(data[1]);

                            //open data of raster D file
                            //StreamReader myReaderD = new StreamReader(mydata.rasterD);
                            using (StreamReader myReaderD = new StreamReader(mydata.RasterD))
                            {
                                data = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                nxD = Convert.ToInt32(data[1]);
                                data = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                nyD = Convert.ToInt32(data[1]);
                                data = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                x11D = Convert.ToDouble(data[1], ic);
                                data = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                y11D = Convert.ToDouble(data[1], ic);
                                data = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                dxD = Convert.ToDouble(data[1], ic);
                                data = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                nodataD = Convert.ToInt32(data[1]);

                                //check if raster B and raster C and raster D are equal to raster A
                                if (nxB != nxA)
                                {
                                    caseswitch = 1;
                                }

                                if (nyB != nyA)
                                {
                                    caseswitch = 1;
                                }

                                if (Math.Abs(x11B - x11A) > 0.1 )
                                {
                                    caseswitch = 1;
                                }

                                if (Math.Abs(y11B - y11A) > 0.1 )
                                {
                                    caseswitch = 1;
                                }

                                if (Math.Abs(dxB - dxA) > 0.1 )
                                {
                                    caseswitch = 1;
                                }

                                if (nxC != nxA)
                                {
                                    caseswitch = 1;
                                }

                                if (nyC != nyA)
                                {
                                    caseswitch = 1;
                                }

                                if (Math.Abs(x11C -x11A) > 0.1 )
                                {
                                    caseswitch = 1;
                                }

                                if (Math.Abs(y11C - y11A) > 0.1 )
                                {
                                    caseswitch = 1;
                                }

                                if (Math.Abs(dxC -  dxA) > 0.1 )
                                {
                                    caseswitch = 1;
                                }

                                if (nxD != nxA)
                                {
                                    caseswitch = 1;
                                }

                                if (nyD != nyA)
                                {
                                    caseswitch = 1;
                                }

                                if (Math.Abs(x11D -  x11A) > 0.1 )
                                {
                                    caseswitch = 1;
                                }

                                if (Math.Abs(y11D -  y11A) > 0.1 )
                                {
                                    caseswitch = 1;
                                }

                                if (Math.Abs(dxD -  dxA) > 0.1 )
                                {
                                    caseswitch = 1;
                                }

                                switch (caseswitch)
                                {
                                    case 0:
                                        data = new string[nxB];
                                        dataC = new string[nxC];
                                        dataD = new string[nxD];
                                        B = CreateArray<double[]>(nxB, () => new double[nyB]);
                                        C = CreateArray<double[]>(nxC, () => new double[nyC]);
                                        D = CreateArray<double[]>(nxD, () => new double[nyD]);
                                        F = CreateArray<double[]>(nxA, () => new double[nyA]);

                                        SetText("Reading Raster B");
                                        for (int i = nyB - 1; i > -1; i--)
                                        {
                                            data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                            for (int j = 0; j < nxB; j++)
                                            {
                                                B[j][i] = Convert.ToDouble(data[j], ic);
                                            }
                                        }
                                        //myReaderB.Close();
                                        //myReaderB.Dispose();

                                        SetText("Reading Raster C");
                                        for (int i = nyB - 1; i > -1; i--)
                                        {
                                            dataC = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                            for (int j = 0; j < nxB; j++)
                                            {
                                                C[j][i] = Convert.ToDouble(dataC[j], ic);
                                            }
                                        }
                                        //myReaderC.Close();
                                        //myReaderC.Dispose();

                                        SetText("Reading Raster D");
                                        for (int i = nyB - 1; i > -1; i--)
                                        {
                                            dataD = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                            for (int j = 0; j < nxB; j++)
                                            {
                                                D[j][i] = Convert.ToDouble(dataD[j], ic);
                                            }
                                        }
                                        //myReaderD.Close();
                                        //myReaderD.Dispose();

                                        SetText("Performing calculations");
                                        //Parallel.For(0, nxB, j =>
                                        for (int j = 0; j < nxB; j++)
                                        {
                                            if (Rechenknecht.CancellationPending)
                                            {
                                                e.Cancel = true;
                                                return;
                                            }
                                            if (j % 50 == 0)
                                            {
                                                Rechenknecht.ReportProgress((int)(j / (double)nxB * 100D));
                                            }
                                            string valueA; string valueB; string valueC; string valueD;
                                            //for (int i = nyB - 1; i > -1; i--)    
                                            //Parallel.For(0, nyB, i =>
                                            Parallel.ForEach(Partitioner.Create(0, nyB, Math.Max(4, (int) (nyB / Environment.ProcessorCount))), range =>
                                            {
                                                MathParserMathos.MathParserMathos parser = new MathParserMathos.MathParserMathos(true, true, true, false);
                                                for (int i = range.Item1; i < range.Item2; i++)
                                                {
                                                    double a = A[j][i];
                                                    double b = B[j][i];
                                                    double c = C[j][i];
                                                    double d = D[j][i];
                                                    try
                                                    {
                                                        string mathtext = mydata.TextBox1.Replace(mydata.Decsep, ".");

                                                        valueA = "(" + Convert.ToString(a, ic) + ")";
                                                        valueB = "(" + Convert.ToString(b, ic) + ")";
                                                        valueC = "(" + Convert.ToString(c, ic) + ")";
                                                        valueD = "(" + Convert.ToString(d, ic) + ")";
                                                        mathtext = mathtext.Replace("A", valueA).Replace("B", valueB).Replace("C", valueC).Replace("D", valueD);
                                                        double result = parser.Parse(mathtext);
                                                        F[j][i] = Convert.ToDouble(result);
                                                    }
                                                    catch
                                                    {
                                                    }
                                                }
                                            });
                                        }

                                        SetText("Writing output file");
                                        //write result to output raster F
                                        if (mydata.RasterF != null)
                                        {

                                            string file = mydata.RasterF;
                                            if (File.Exists(file))
                                            {
                                                try
                                                {
                                                    File.Delete(file);
                                                }
                                                catch { }
                                            }

                                            using (StreamWriter mywriter = new StreamWriter(file))
                                            {
                                                mywriter.WriteLine("ncols             " + Convert.ToString(nxA));
                                                mywriter.WriteLine("nrows             " + Convert.ToString(nyA));
                                                mywriter.WriteLine("xllcorner         " + Convert.ToString(x11A, ic));
                                                mywriter.WriteLine("yllcorner         " + Convert.ToString(y11A, ic));
                                                mywriter.WriteLine("cellsize          " + Convert.ToString(dxA, ic));
                                                if (mydata.Unit.Length > 0)
                                                {
                                                    mywriter.WriteLine("NODATA_value      " + Convert.ToString(nodataA) + "\t" + "UNIT \t" + mydata.Unit);
                                                }
                                                else
                                                {
                                                    mywriter.WriteLine("NODATA_value      " + Convert.ToString(nodataA));
                                                }

                                                for (int i = nyB - 1; i > -1; i--)
                                                {
                                                    for (int j = 0; j < nxB; j++)
                                                    {
                                                        mywriter.Write(Convert.ToString(Math.Round(F[j][i], 3), ic) + " ");
                                                    }
                                                    mywriter.WriteLine();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            BackgroundThreadMessageBox("No output raster has been defined");
                                            return;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    BackgroundThreadMessageBox("Unable to read the data set B, C or D");
                    return;
                }
                //Cursor = Cursors.Default;
            }

            //read raster B, raster C, raster D, and raster E
            if ((mydata.RasterB != null) && (mydata.RasterC != null) && (mydata.RasterD != null) && (mydata.RasterE != null))
            {
                try
                {
                    //Cursor = Cursors.WaitCursor;
                    string[] data = new string[100];
                    string[] dataC = new string[100];
                    string[] dataD = new string[100];
                    string[] dataE = new string[100];
                    //open data of raster B file
                    //StreamReader myReaderB = new StreamReader(mydata.rasterB);
                    using (StreamReader myReaderB = new StreamReader(mydata.RasterB))
                    {
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        nxB = Convert.ToInt32(data[1]);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        nyB = Convert.ToInt32(data[1]);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        x11B = Convert.ToDouble(data[1], ic);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        y11B = Convert.ToDouble(data[1], ic);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        dxB = Convert.ToDouble(data[1], ic);
                        data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        nodataB = Convert.ToInt32(data[1]);

                        //open data of raster C file
                        //StreamReader myReaderC = new StreamReader(mydata.rasterC);
                        using (StreamReader myReaderC = new StreamReader(mydata.RasterC))
                        {
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            nxC = Convert.ToInt32(data[1]);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            nyC = Convert.ToInt32(data[1]);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            x11C = Convert.ToDouble(data[1], ic);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            y11C = Convert.ToDouble(data[1], ic);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            dxC = Convert.ToDouble(data[1], ic);
                            data = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            nodataC = Convert.ToInt32(data[1]);

                            //open data of raster D file
                            //StreamReader myReaderD = new StreamReader(mydata.rasterD);
                            using (StreamReader myReaderD = new StreamReader(mydata.RasterD))
                            {
                                data = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                nxD = Convert.ToInt32(data[1]);
                                data = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                nyD = Convert.ToInt32(data[1]);
                                data = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                x11D = Convert.ToDouble(data[1], ic);
                                data = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                y11D = Convert.ToDouble(data[1], ic);
                                data = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                dxD = Convert.ToDouble(data[1], ic);
                                data = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                nodataD = Convert.ToInt32(data[1]);

                                //open data of raster E file
                                //StreamReader myReaderE = new StreamReader(mydata.rasterE);
                                using (StreamReader myReaderE = new StreamReader(mydata.RasterE))
                                {
                                    data = myReaderE.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    nxE = Convert.ToInt32(data[1]);
                                    data = myReaderE.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    nyE = Convert.ToInt32(data[1]);
                                    data = myReaderE.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    x11E = Convert.ToDouble(data[1], ic);
                                    data = myReaderE.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    y11E = Convert.ToDouble(data[1], ic);
                                    data = myReaderE.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    dxE = Convert.ToDouble(data[1], ic);
                                    data = myReaderE.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    nodataE = Convert.ToInt32(data[1]);

                                    //check if raster B, raster C, raster D, and raster E are equal to raster A
                                    if (nxB != nxA)
                                    {
                                        caseswitch = 1;
                                    }

                                    if (nyB != nyA)
                                    {
                                        caseswitch = 1;
                                    }

                                    if (Math.Abs(x11B - x11A) > 0.1 )
                                    {
                                        caseswitch = 1;
                                    }

                                    if (Math.Abs(y11B - y11A) > 0.1 )
                                    {
                                        caseswitch = 1;
                                    }

                                    if (Math.Abs(dxB - dxA) > 0.1 )
                                    {
                                        caseswitch = 1;
                                    }

                                    if (nxC != nxA)
                                    {
                                        caseswitch = 1;
                                    }

                                    if (nyC != nyA)
                                    {
                                        caseswitch = 1;
                                    }

                                    if (Math.Abs(x11C -x11A) > 0.1 )
                                    {
                                        caseswitch = 1;
                                    }

                                    if (Math.Abs(y11C - y11A) > 0.1 )
                                    {
                                        caseswitch = 1;
                                    }

                                    if (Math.Abs(dxC - dxA) > 0.1 )
                                    {
                                        caseswitch = 1;
                                    }

                                    if (nxD != nxA)
                                    {
                                        caseswitch = 1;
                                    }

                                    if (nyD != nyA)
                                    {
                                        caseswitch = 1;
                                    }

                                    if (Math.Abs(x11D -  x11A) > 0.1 )
                                    {
                                        caseswitch = 1;
                                    }

                                    if (Math.Abs(y11D - y11A) > 0.1 )
                                    {
                                        caseswitch = 1;
                                    }

                                    if (Math.Abs(dxD - dxA) > 0.1 )
                                    {
                                        caseswitch = 1;
                                    }

                                    if (nxE != nxA)
                                    {
                                        caseswitch = 1;
                                    }

                                    if (nyE != nyA)
                                    {
                                        caseswitch = 1;
                                    }

                                    if (Math.Abs(x11E - x11A) > 0.1 )
                                    {
                                        caseswitch = 1;
                                    }

                                    if (Math.Abs(y11E - y11A) > 0.1 )
                                    {
                                        caseswitch = 1;
                                    }

                                    if (Math.Abs(dxE - dxA) > 0.1 )
                                    {
                                        caseswitch = 1;
                                    }

                                    switch (caseswitch)
                                    {
                                        case 0:
                                            data = new string[nxB];
                                            dataC = new string[nxC];
                                            dataD = new string[nxD];
                                            dataE = new string[nxE];
                                            B = CreateArray<double[]>(nxB, () => new double[nyB]);
                                            C = CreateArray<double[]>(nxC, () => new double[nyC]);
                                            D = CreateArray<double[]>(nxD, () => new double[nyD]);
                                            E = CreateArray<double[]>(nxE, () => new double[nyE]);
                                            F = CreateArray<double[]>(nxA, () => new double[nyA]);

                                            SetText("Reading Raster B");
                                            for (int i = nyB - 1; i > -1; i--)
                                            {
                                                data = myReaderB.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                                for (int j = 0; j < nxB; j++)
                                                {
                                                    B[j][i] = Convert.ToDouble(data[j], ic);
                                                }
                                            }
                                            //myReaderB.Close();
                                            //myReaderB.Dispose();

                                            SetText("Reading Raster C");
                                            for (int i = nyB - 1; i > -1; i--)
                                            {
                                                dataC = myReaderC.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                                for (int j = 0; j < nxB; j++)
                                                {
                                                    C[j][i] = Convert.ToDouble(dataC[j], ic);
                                                }
                                            }
                                            //myReaderC.Close();
                                            //myReaderC.Dispose();

                                            SetText("Reading Raster D");
                                            for (int i = nyB - 1; i > -1; i--)
                                            {
                                                dataD = myReaderD.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                                for (int j = 0; j < nxB; j++)
                                                {
                                                    D[j][i] = Convert.ToDouble(dataD[j], ic);
                                                }
                                            }
                                            //myReaderD.Close();
                                            //myReaderD.Dispose();

                                            SetText("Reading Raster E");
                                            for (int i = nyB - 1; i > -1; i--)
                                            {
                                                dataE = myReaderE.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                                for (int j = 0; j < nxB; j++)
                                                {
                                                    E[j][i] = Convert.ToDouble(dataE[j], ic);
                                                }
                                            }
                                            //myReaderE.Close();
                                            //myReaderE.Dispose();

                                            SetText("Performing calculations");
                                            //Parallel.For(0, nxB, j =>
                                            for (int j = 0; j < nxB; j++)
                                            {
                                                if (Rechenknecht.CancellationPending)
                                                {
                                                    e.Cancel = true;
                                                    return;
                                                }
                                                if (j % 50 == 0)
                                                {
                                                    Rechenknecht.ReportProgress((int)(j / (double)nxB * 100D));
                                                }
                                                string valueA; string valueB; string valueC; string valueD; string valueE;
                                                //for (int i = nyB - 1; i > -1; i--)
                                                //Parallel.For(0, nyB, i =>
                                                Parallel.ForEach(Partitioner.Create(0, nyB, Math.Max(4, (int)(nyB / Environment.ProcessorCount))), range =>
                                                {
                                                    MathParserMathos.MathParserMathos parser = new MathParserMathos.MathParserMathos(true, true, true, false);
                                                    for (int i = range.Item1; i < range.Item2; i++)
                                                    {
                                                        double a = A[j][i];
                                                        double b = B[j][i];
                                                        double c = C[j][i];
                                                        double d = D[j][i];
                                                        double e1 = E[j][i];
                                                        try
                                                        {
                                                            string mathtext = mydata.TextBox1.Replace(mydata.Decsep, ".");

                                                            valueA = "(" + Convert.ToString(a, ic) + ")";
                                                            valueB = "(" + Convert.ToString(b, ic) + ")";
                                                            valueC = "(" + Convert.ToString(c, ic) + ")";
                                                            valueD = "(" + Convert.ToString(d, ic) + ")";
                                                            valueE = "(" + Convert.ToString(e1, ic) + ")";
                                                            mathtext = mathtext.Replace("A", valueA).Replace("B", valueB).Replace("C", valueC).Replace("D", valueD).Replace("E", valueE);
                                                            double result = parser.Parse(mathtext);
                                                            F[j][i] = Convert.ToDouble(result);
                                                        }
                                                        catch
                                                        {
                                                        }
                                                    }
                                                });
                                            }

                                            //write result to output raster F
                                            if (mydata.RasterF != null)
                                            {
                                                string file = mydata.RasterF;
                                                if (File.Exists(file))
                                                {
                                                    try
                                                    {
                                                        File.Delete(file);
                                                    }
                                                    catch { }
                                                }

                                                using (StreamWriter mywriter = new StreamWriter(file))
                                                {
                                                    mywriter.WriteLine("ncols             " + Convert.ToString(nxA));
                                                    mywriter.WriteLine("nrows             " + Convert.ToString(nyA));
                                                    mywriter.WriteLine("xllcorner         " + Convert.ToString(x11A, ic));
                                                    mywriter.WriteLine("yllcorner         " + Convert.ToString(y11A, ic));
                                                    mywriter.WriteLine("cellsize          " + Convert.ToString(dxA, ic));
                                                    if (mydata.Unit.Length > 0)
                                                    {
                                                        mywriter.WriteLine("NODATA_value      " + Convert.ToString(nodataA) + "\t" + "UNIT \t" + mydata.Unit);
                                                    }
                                                    else
                                                    {
                                                        mywriter.WriteLine("NODATA_value      " + Convert.ToString(nodataA));
                                                    }

                                                    for (int i = nyB - 1; i > -1; i--)
                                                    {
                                                        for (int j = 0; j < nxB; j++)
                                                        {
                                                            mywriter.Write(Convert.ToString(F[j][i], ic) + " ");
                                                        }
                                                        mywriter.WriteLine();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                BackgroundThreadMessageBox("No output raster has been defined");
                                                return;
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }

                catch
                {
                    BackgroundThreadMessageBox("Unable to read the data set B, C, D or E");
                    return;
                }
                //Cursor = Cursors.Default;
            }
            else if ((mydata.RasterB == null) && (mydata.RasterC == null) && (mydata.RasterD == null) && (mydata.RasterE == null))
            {
                SetText("Perform calculations");
                MathParserMathos.MathParserMathos parser = new MathParserMathos.MathParserMathos(true, true, true, false);
                F = CreateArray<double[]>(nxA, () => new double[nyA]);
                //double result;

                //Stopwatch sw = new Stopwatch();
                //sw.Start();

                //parallelization of loop
                //Parallel.For(0, nxA, j =>
                for (int j = 0; j < nxA; j++)
                {
                    if (Rechenknecht.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    if (j % 50 == 0)
                    {
                        Rechenknecht.ReportProgress((int)(j / (double)nxA * 100D));
                    }
                    string valueA;
                    //for (int i = nyA - 1; i > -1; i--)
                    Parallel.For(0, nyA, i =>
                    {
                        string mathtext = mydata.TextBox1.Replace(mydata.Decsep, ".");
                        mathtext = mathtext.Replace(",", mydata.Decsep);
                        double a = A[j][i];
                        //mp.Parameters.Clear();
                        //mp.Parameters.Add(MathFunctions.Parameters.A, a);
                        try
                        {
                            //result = mp.Calculate(mathtext);
                            //C[j, i] = result;
                            mathtext = mydata.TextBox1.Replace(mydata.Decsep, ".");

                            valueA = "(" + Convert.ToString(a, ic) + ")";
                            mathtext = mathtext.Replace("A", valueA);
                            //result = Calculator.Calc(mathtext);
                            double result = parser.Parse(mathtext);

                            F[j][i] = Convert.ToDouble(result);
                        }
                        catch
                        {
                        }
                    });
                }
                //sw.Stop();
                //MessageBox.Show(Convert.ToString(sw.Elapsed));


                SetText("Final output");
                //write result to output raster F
                if (mydata.RasterF != null)
                {
                    string file = mydata.RasterF;
                    if (File.Exists(file))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch{}
                    }
                    
                    CultureInfo ic = CultureInfo.InvariantCulture;
                    using (StreamWriter mywriter = new StreamWriter (file)) 
                    {
                        mywriter.WriteLine ("ncols             " + Convert.ToString (nxA));
                        mywriter.WriteLine ("nrows             " + Convert.ToString (nyA));
                        mywriter.WriteLine ("xllcorner         " + Convert.ToString (x11A, ic));
                        mywriter.WriteLine ("yllcorner         " + Convert.ToString (y11A, ic));
                        mywriter.WriteLine ("cellsize          " + Convert.ToString (dxA, ic));
                        if (mydata.Unit.Length > 0)
                        {
                            mywriter.WriteLine ("NODATA_value      " + Convert.ToString (nodataA) + "\t" + "UNIT \t" + mydata.Unit);
                        }
                        else
                        {
                            mywriter.WriteLine ("NODATA_value      " + Convert.ToString (nodataA));
                        }

                        for (int i = nyA - 1; i > -1; i--) {
                            for (int j = 0; j < nxA; j++) 
                            {
                                if (Double.IsNaN(F[j][i]) || Double.IsInfinity(F[j][i]))
                                {
                                    F[j][i] = 0;
                                }
                                mywriter.Write (Convert.ToString (F [j][i], ic) + " ");
                            }
                            mywriter.WriteLine ();
                        }
                    }
                }
                else
                {
                    BackgroundThreadMessageBox ("No output raster has been defined");
                    return;
                }
            }
            Computation_Completed = true; // set flag, that computation was successful
        }
    }
}
