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
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using GralItemData;

namespace GralShape
{
    /// <summary>
    /// Write a shape file
    /// </summary>
    public class ShapeWriter
    {
        double xMin, yMin, xMax, yMax;
        public GralDomain.Domain domain = null;
        public string DecSep;
        public string ShxFile;

        public ShapeWriter(GralDomain.Domain d)
        {
            domain = d;
            //User defined column seperator and decimal seperator
            DecSep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        }

        public void WriteShapeFile(string filename, int shapetype)
        {
            Byte[] data = new Byte[4];
            Byte[] data8 = new Byte[8];
            //export line sources
            if (shapetype == 3)
            {
                //write main file header
                (FileStream fs, FileStream fshx) = writeHeader(filename, shapetype, domain.EditLS.ItemData.Count);
                if (fs != null && fshx != null)
                {
                    //bounding box
                    xMin = 10000000;
                    yMin = 10000000;
                    xMax = -10000000;
                    yMax = -10000000;
                    double x1 = 0;
                    double y1 = 0;
                    double x2 = 0;
                    double y2 = 0;
                    string[] text1 = new string[1000];
                    foreach (LineSourceData _ls in domain.EditLS.ItemData)
                    {
                        for (int i = 0; i < _ls.Pt.Count - 1; i++)
                        {
                            x1 = _ls.Pt[i].X;
                            y1 = _ls.Pt[i].Y;
                            x2 = _ls.Pt[i + 1].X;
                            y2 = _ls.Pt[i + 1].Y;
                            xMin = Math.Min(xMin, x1);
                            xMin = Math.Min(xMin, x2);
                            yMin = Math.Min(yMin, y1);
                            yMin = Math.Min(yMin, y2);
                            xMax = Math.Max(xMax, x1);
                            xMax = Math.Max(xMax, x2);
                            yMax = Math.Max(yMax, y1);
                            yMax = Math.Max(yMax, y2);
                        }
                    }

                    data8 = WriteDoubleLittle(xMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(xMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    int recordNumber = 0;
                    int offset = 50;
                    //write records: header and record to .shp file
                    //offset and index to .shx file
                    foreach (LineSourceData _ls in domain.EditLS.ItemData)
                    {
                        recordNumber += 1;
                        int sourcegroups = _ls.Poll.Count;
                        //record number
                        data = WriteIntBig(recordNumber);
                        fs.Write(data, 0, 4);
                        //offset
                        data = WriteIntBig(offset);
                        fshx.Write(data, 0, 4);
                        offset = offset + 4 + 24 + 8 * _ls.Pt.Count;
                        //content length
                        data = WriteIntBig(24 + 8 * _ls.Pt.Count);
                        fs.Write(data, 0, 4);
                        fshx.Write(data, 0, 4);
                        //shape type
                        data = WriteIntLittle(3);
                        fs.Write(data, 0, 4);
                        //Box
                        xMin = 10000000;
                        yMin = 10000000;
                        xMax = -10000000;
                        yMax = -10000000;
                        for (int i = 0; i < _ls.Pt.Count - 1; i++)
                        {
                            x1 = _ls.Pt[i].X;
                            y1 = _ls.Pt[i].Y;
                            x2 = _ls.Pt[i + 1].X;
                            y2 = _ls.Pt[i + 1].Y;
                            xMin = Math.Min(xMin, x1);
                            xMin = Math.Min(xMin, x2);
                            yMin = Math.Min(yMin, y1);
                            yMin = Math.Min(yMin, y2);
                            xMax = Math.Max(xMax, x1);
                            xMax = Math.Max(xMax, x2);
                            yMax = Math.Max(yMax, y1);
                            yMax = Math.Max(yMax, y2);
                        }
                        data8 = WriteDoubleLittle(xMin);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(yMin);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(xMax);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(yMax);
                        fs.Write(data8, 0, 8);
                        //the number of parts in the polyline
                        data = WriteIntLittle(1);
                        fs.Write(data, 0, 4);
                        //the total number of points of all polylines
                        data = WriteIntLittle(_ls.Pt.Count);
                        fs.Write(data, 0, 4);
                        //index of first point of each polyline
                        data = WriteIntLittle(0);
                        fs.Write(data, 0, 4);
                        //points of each polyline
                        for (int i = 0; i < _ls.Pt.Count; i++)
                        {
                            x1 = _ls.Pt[i].X;
                            y1 = _ls.Pt[i].Y;
                            data8 = WriteDoubleLittle(x1);
                            fs.Write(data8, 0, 8);
                            data8 = WriteDoubleLittle(y1);
                            fs.Write(data8, 0, 8);
                        }
                    }

                    //filelength updated
                    double dummy = Convert.ToDouble(fs.Length * 0.5);
                    int filelength = (int)dummy;

                    data = WriteIntBig(filelength);
                    fs.Position = 24;
                    fs.Write(data, 0, 4);
                    fs.Close(); fshx.Close();
                    fs.Dispose(); fshx.Dispose();
                }
            }
            //export area sources
            else if (shapetype == 5)
            {
                //write main file header
                (FileStream fs, FileStream fshx) = writeHeader(filename, shapetype, domain.EditAS.ItemData.Count);
                if (fs != null && fshx != null)
                {
                    //bounding box
                    xMin = 10000000;
                    yMin = 10000000;
                    xMax = -10000000;
                    yMax = -10000000;
                    double x1 = 0;
                    double y1 = 0;

                    foreach (AreaSourceData _as in domain.EditAS.ItemData)
                    {
                        for (int i = 0; i < _as.Pt.Count; i++)
                        {
                            x1 = _as.Pt[i].X;
                            y1 = _as.Pt[i].Y;
                            xMin = Math.Min(xMin, x1);
                            yMin = Math.Min(yMin, y1);
                            xMax = Math.Max(xMax, x1);
                            yMax = Math.Max(yMax, y1);
                        }
                    }
                    data8 = WriteDoubleLittle(xMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(xMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    int recordNumber = 0;
                    int offset = 50;

                    //write records: header and record
                    foreach (AreaSourceData _as in domain.EditAS.ItemData)
                    {
                        recordNumber += 1;
                        //record number
                        data = WriteIntBig(recordNumber);
                        fs.Write(data, 0, 4);
                        //offset
                        data = WriteIntBig(offset);
                        fshx.Write(data, 0, 4);
                        offset = offset + 4 + 24 + 8 * (_as.Pt.Count + 1);
                        //content length
                        data = WriteIntBig(24 + 8 * (_as.Pt.Count + 1));
                        fs.Write(data, 0, 4);
                        fshx.Write(data, 0, 4);
                        //shape type
                        data = WriteIntLittle(5);
                        fs.Write(data, 0, 4);
                        //Box
                        xMin = 10000000;
                        yMin = 10000000;
                        xMax = -10000000;
                        yMax = -10000000;

                        for (int i = 0; i < _as.Pt.Count; i++)
                        {
                            x1 = _as.Pt[i].X;
                            y1 = _as.Pt[i].Y;
                            xMin = Math.Min(xMin, x1);
                            yMin = Math.Min(yMin, y1);
                            xMax = Math.Max(xMax, x1);
                            yMax = Math.Max(yMax, y1);
                        }
                        data8 = WriteDoubleLittle(xMin);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(yMin);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(xMax);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(yMax);
                        fs.Write(data8, 0, 8);
                        //the number of parts in the polyline
                        data = WriteIntLittle(1);
                        fs.Write(data, 0, 4);
                        //the total number of points of all polylines
                        data = WriteIntLittle(_as.Pt.Count + 1);
                        fs.Write(data, 0, 4);
                        //index of first point of each polyline
                        data = WriteIntLittle(0);
                        fs.Write(data, 0, 4);
                        //points of each polyline -> have to exported in clockwise direction!
                        //seek the most northern point
                        double ynorth = _as.Pt[0].Y;
                        double xnorth = _as.Pt[0].X;
                        double x_minus;
                        double x_plus;
                        double y_minus;
                        double y_plus;
                        int inorth = 0;
                        for (int i = 1; i < _as.Pt.Count; i++)
                        {
                            x1 = _as.Pt[i].X;
                            y1 = _as.Pt[i].Y;
                            if (y1 > ynorth)
                            {
                                xnorth = x1;
                                ynorth = y1;
                                inorth = i;
                            }
                        }
                        //check direction
                        if (inorth > 0)
                        {
                            x_minus = Convert.ToDouble(_as.Pt[(inorth - 1)].X);
                            y_minus = Convert.ToDouble(_as.Pt[(inorth - 1)].Y);
                        }
                        else
                        {
                            x_minus = Convert.ToDouble(_as.Pt[_as.Pt.Count - 1].X);
                            y_minus = Convert.ToDouble(_as.Pt[_as.Pt.Count - 1].Y);
                        }
                        //check direction
                        if (inorth < _as.Pt.Count - 1)
                        {
                            x_plus = Convert.ToDouble(_as.Pt[(inorth + 1)].X);
                            y_plus = Convert.ToDouble(_as.Pt[(inorth + 1)].Y);
                        }
                        else
                        {
                            x_plus = Convert.ToDouble(_as.Pt[0].X);
                            y_plus = Convert.ToDouble(_as.Pt[0].Y);
                        }

                        bool clockwise = true;
                        if (x_plus < x_minus)
                        {
                            clockwise = false;
                        }

                        if ((x_plus < xnorth) && (x_minus < xnorth))
                        {
                            if (y_minus < y_plus)
                            {
                                clockwise = false;
                            }
                        }

                        if ((x_plus > xnorth) && (x_minus > xnorth))
                        {
                            if (y_minus > y_plus)
                            {
                                clockwise = false;
                            }
                        }

                        if (clockwise == true)
                        {
                            for (int i = 0; i < _as.Pt.Count; i++)
                            {
                                x1 = Convert.ToDouble(_as.Pt[i].X);
                                y1 = Convert.ToDouble(_as.Pt[i].Y);

                                data8 = WriteDoubleLittle(x1);
                                fs.Write(data8, 0, 8);
                                data8 = WriteDoubleLittle(y1);
                                fs.Write(data8, 0, 8);
                            }
                            //write first point again to close the polygon
                            x1 = Convert.ToDouble(_as.Pt[0].X);
                            y1 = Convert.ToDouble(_as.Pt[0].Y);
                            data8 = WriteDoubleLittle(x1);
                            fs.Write(data8, 0, 8);
                            data8 = WriteDoubleLittle(y1);
                            fs.Write(data8, 0, 8);
                        }
                        else
                        {
                            for (int i = _as.Pt.Count - 1; i > -1; i--)
                            {
                                x1 = Convert.ToDouble(_as.Pt[i].X);
                                y1 = Convert.ToDouble(_as.Pt[i].Y);
                                data8 = WriteDoubleLittle(x1);
                                fs.Write(data8, 0, 8);
                                data8 = WriteDoubleLittle(y1);
                                fs.Write(data8, 0, 8);
                            }
                            //write last point again to close the polygon
                            x1 = Convert.ToDouble(_as.Pt[_as.Pt.Count - 1].X);
                            y1 = Convert.ToDouble(_as.Pt[_as.Pt.Count - 1].Y);
                            data8 = WriteDoubleLittle(x1);
                            fs.Write(data8, 0, 8);
                            data8 = WriteDoubleLittle(y1);
                            fs.Write(data8, 0, 8);
                        }
                    }

                    //filelength updated
                    double dummy = Convert.ToDouble(fs.Length * 0.5);
                    int filelength = (int)dummy;

                    data = WriteIntBig(filelength);
                    fs.Position = 24;
                    fs.Write(data, 0, 4);
                    fs.Close(); fshx.Close();
                    fs.Dispose(); fshx.Dispose();
                }
            }
            //export point sources
            else if (shapetype == 1)
            {
                //write main file header
                (FileStream fs, FileStream fshx) = writeHeader(filename, shapetype, domain.EditPS.ItemData.Count);
                if (fs != null && fshx != null)
                {
                    //bounding box
                    xMin = 10000000;
                    yMin = 10000000;
                    xMax = -10000000;
                    yMax = -10000000;
                    double x1 = 0;
                    double y1 = 0;

                    foreach (PointSourceData _psdata in domain.EditPS.ItemData)
                    {
                        x1 = _psdata.Pt.X;
                        y1 = _psdata.Pt.Y;
                        xMin = Math.Min(xMin, x1);
                        yMin = Math.Min(yMin, y1);
                        xMax = Math.Max(xMax, x1);
                        yMax = Math.Max(yMax, y1);
                    }
                    data8 = WriteDoubleLittle(xMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(xMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    int recordNumber = 0;
                    int offset = 50;

                    //write records: header and record
                    foreach (PointSourceData _psdata in domain.EditPS.ItemData)
                    {
                        recordNumber += 1;

                        //record number
                        data = WriteIntBig(recordNumber);
                        fs.Write(data, 0, 4);
                        //offset
                        data = WriteIntBig(offset);
                        fshx.Write(data, 0, 4);
                        offset = offset + 4 + 2 + 8;
                        //content length
                        data = WriteIntBig(2 + 8);
                        fs.Write(data, 0, 4);
                        fshx.Write(data, 0, 4);
                        //shape type
                        data = WriteIntLittle(1);
                        fs.Write(data, 0, 4);
                        //write points
                        x1 = _psdata.Pt.X;
                        y1 = _psdata.Pt.Y;
                        data8 = WriteDoubleLittle(x1);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(y1);
                        fs.Write(data8, 0, 8);
                    }

                    //filelength updated
                    double dummy = Convert.ToDouble(fs.Length * 0.5);
                    int filelength = (int)dummy;

                    data = WriteIntBig(filelength);
                    fs.Position = 24;
                    fs.Write(data, 0, 4);
                    fs.Close(); fshx.Close();
                    fs.Dispose(); fshx.Dispose();
                }
            }
            
            //export receptors
            else if (shapetype == 0)
            {
                shapetype = 1;
                //write main file header
                (FileStream fs, FileStream fshx) = writeHeader(filename, shapetype, domain.EditR.ItemData.Count);
                if (fs != null && fshx != null)
                {
                    //bounding box
                    xMin = 10000000;
                    yMin = 10000000;
                    xMax = -10000000;
                    yMax = -10000000;
                    double x1 = 0;
                    double y1 = 0;

                    foreach (ReceptorData _rd in domain.EditR.ItemData)
                    {
                        x1 = Math.Round(_rd.Pt.X, 1);
                        y1 = Math.Round(_rd.Pt.Y, 1);
                        xMin = Math.Min(xMin, x1);
                        yMin = Math.Min(yMin, y1);
                        xMax = Math.Max(xMax, x1);
                        yMax = Math.Max(yMax, y1);
                    }

                    data8 = WriteDoubleLittle(xMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(xMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    int recordNumber = 0;
                    int offset = 50;
                    //write records: header and record
                    foreach (ReceptorData _rd in domain.EditR.ItemData)
                    {
                        recordNumber++;
                        //record number
                        data = WriteIntBig(recordNumber);
                        fs.Write(data, 0, 4);
                        //offset
                        data = WriteIntBig(offset);
                        fshx.Write(data, 0, 4);
                        offset = offset + 4 + 2 + 8;
                        //content length
                        data = WriteIntBig(2 + 8);
                        fs.Write(data, 0, 4);
                        fshx.Write(data, 0, 4);
                        //shape type
                        data = WriteIntLittle(1);
                        fs.Write(data, 0, 4);
                        //write points
                        x1 = Math.Round(_rd.Pt.X, 1);
                        y1 = Math.Round(_rd.Pt.Y, 1);
                        data8 = WriteDoubleLittle(x1);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(y1);
                        fs.Write(data8, 0, 8);
                    }

                    //filelength updated
                    double dummy = Convert.ToDouble(fs.Length * 0.5);
                    int filelength = (int)dummy;

                    data = WriteIntBig(filelength);
                    fs.Position = 24;
                    fs.Write(data, 0, 4);
                    fs.Close(); fshx.Close();
                    fs.Dispose(); fshx.Dispose();
                }
            }
            //export buildings
            else if (shapetype == 10)
            {
                shapetype = 5; // shapetype polygon
                //write main file header
                (FileStream fs, FileStream fshx) = writeHeader(filename, shapetype, domain.EditB.ItemData.Count);
                if (fs != null && fshx != null)
                {
                    //bounding box
                    xMin = 10000000;
                    yMin = 10000000;
                    xMax = -10000000;
                    yMax = -10000000;
                    double x1 = 0;
                    double y1 = 0;

                    foreach (BuildingData _bu in domain.EditB.ItemData)
                    {
                        for (int i = 0; i < _bu.Pt.Count; i++)
                        {
                            x1 = _bu.Pt[i].X;
                            y1 = _bu.Pt[i].Y;
                            xMin = Math.Min(xMin, x1);
                            yMin = Math.Min(yMin, y1);
                            xMax = Math.Max(xMax, x1);
                            yMax = Math.Max(yMax, y1);
                        }
                    }
                    data8 = WriteDoubleLittle(xMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(xMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    int recordNumber = 0;
                    int offset = 50;

                    //write records: header and record
                    foreach (BuildingData _bu in domain.EditB.ItemData)
                    {
                        recordNumber += 1;
                        //record number
                        data = WriteIntBig(recordNumber);
                        fs.Write(data, 0, 4);
                        //offset
                        data = WriteIntBig(offset);
                        fshx.Write(data, 0, 4);
                        offset = offset + 4 + 24 + 8 * (_bu.Pt.Count);
                        //content length
                        data = WriteIntBig(24 + 8 * (_bu.Pt.Count));
                        fs.Write(data, 0, 4);
                        fshx.Write(data, 0, 4);
                        //shape type
                        data = WriteIntLittle(5);
                        fs.Write(data, 0, 4);
                        //Box
                        xMin = 10000000;
                        yMin = 10000000;
                        xMax = -10000000;
                        yMax = -10000000;

                        for (int i = 0; i < _bu.Pt.Count; i++)
                        {
                            x1 = _bu.Pt[i].X;
                            y1 = _bu.Pt[i].Y;
                            xMin = Math.Min(xMin, x1);
                            yMin = Math.Min(yMin, y1);
                            xMax = Math.Max(xMax, x1);
                            yMax = Math.Max(yMax, y1);
                        }
                        data8 = WriteDoubleLittle(xMin);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(yMin);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(xMax);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(yMax);
                        fs.Write(data8, 0, 8);
                        //the number of parts in the polyline
                        data = WriteIntLittle(1);
                        fs.Write(data, 0, 4);
                        //the total number of points of all polylines
                        data = WriteIntLittle(_bu.Pt.Count);
                        fs.Write(data, 0, 4);
                        //index of first point of each polyline
                        data = WriteIntLittle(0);
                        fs.Write(data, 0, 4);
                        //points of each polyline -> have to exported in clockwise direction!
                        //seek the most northern point
                        double ynorth = _bu.Pt[0].Y;
                        double xnorth = _bu.Pt[0].X;
                        double x_minus;
                        double x_plus;
                        double y_minus;
                        double y_plus;
                        int inorth = 0;
                        for (int i = 1; i < _bu.Pt.Count; i++)
                        {
                            x1 = _bu.Pt[i].X;
                            y1 = _bu.Pt[i].Y;
                            if (y1 > ynorth)
                            {
                                xnorth = x1;
                                ynorth = y1;
                                inorth = i;
                            }
                        }
                        //check direction
                        if (inorth > 0)
                        {
                            x_minus = Convert.ToDouble(_bu.Pt[(inorth - 1)].X);
                            y_minus = Convert.ToDouble(_bu.Pt[(inorth - 1)].Y);
                        }
                        else
                        {
                            x_minus = Convert.ToDouble(_bu.Pt[_bu.Pt.Count - 1].X);
                            y_minus = Convert.ToDouble(_bu.Pt[_bu.Pt.Count - 1].Y);
                        }
                        //check direction
                        if (inorth < _bu.Pt.Count - 1)
                        {
                            x_plus = Convert.ToDouble(_bu.Pt[(inorth + 1)].X);
                            y_plus = Convert.ToDouble(_bu.Pt[(inorth + 1)].Y);
                        }
                        else
                        {
                            x_plus = Convert.ToDouble(_bu.Pt[0].X);
                            y_plus = Convert.ToDouble(_bu.Pt[0].Y);
                        }

                        bool clockwise = true;
                        if (x_plus < x_minus)
                        {
                            clockwise = false;
                        }

                        if ((x_plus < xnorth) && (x_minus < xnorth))
                        {
                            if (y_minus < y_plus)
                            {
                                clockwise = false;
                            }
                        }

                        if ((x_plus > xnorth) && (x_minus > xnorth))
                        {
                            if (y_minus > y_plus)
                            {
                                clockwise = false;
                            }
                        }

                        if (clockwise == true)
                        {
                            for (int i = 0; i < _bu.Pt.Count; i++)
                            {
                                x1 = Convert.ToDouble(_bu.Pt[i].X);
                                y1 = Convert.ToDouble(_bu.Pt[i].Y);

                                data8 = WriteDoubleLittle(x1);
                                fs.Write(data8, 0, 8);
                                data8 = WriteDoubleLittle(y1);
                                fs.Write(data8, 0, 8);
                            }
                            //write first point again to close the polygon
                        }
                        else
                        {
                            for (int i = _bu.Pt.Count - 1; i > -1; i--)
                            {
                                x1 = Convert.ToDouble(_bu.Pt[i].X);
                                y1 = Convert.ToDouble(_bu.Pt[i].Y);
                                data8 = WriteDoubleLittle(x1);
                                fs.Write(data8, 0, 8);
                                data8 = WriteDoubleLittle(y1);
                                fs.Write(data8, 0, 8);
                            }
                        }
                    }

                    //filelength updated
                    double dummy = Convert.ToDouble(fs.Length * 0.5);
                    int filelength = (int)dummy;

                    data = WriteIntBig(filelength);
                    fs.Position = 24;
                    fs.Write(data, 0, 4);
                    fs.Close(); fshx.Close();
                    fs.Dispose(); fshx.Dispose();
                }
            }
            //export domain area
            else if (shapetype == 11)
            {
                shapetype = 5; // shapetype polygon
                int edgePoints = 5;
                //write main file header
                (FileStream fs, FileStream fshx) = writeHeader(filename, shapetype, edgePoints);
                if (fs != null && fshx != null)
                {
                    //bounding box
                    xMin = domain.MainForm.GralDomRect.West;
                    yMin = domain.MainForm.GralDomRect.North;
                    xMax = domain.MainForm.GralDomRect.East;
                    yMax = domain.MainForm.GralDomRect.South;

                    data8 = WriteDoubleLittle(xMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(xMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    int recordNumber = 0;
                    int offset = 50;

                    recordNumber += 1;
                    //record number
                    data = WriteIntBig(recordNumber);
                    fs.Write(data, 0, 4);
                    //offset
                    data = WriteIntBig(offset);
                    fshx.Write(data, 0, 4);
                    offset = offset + 4 + 24 + 8 * edgePoints;
                    //content length
                    data = WriteIntBig(24 + 8 * edgePoints);
                    fs.Write(data, 0, 4);
                    fshx.Write(data, 0, 4);
                    //shape type
                    data = WriteIntLittle(5);
                    fs.Write(data, 0, 4);
                    //Box
                    xMin = 10000000;
                    yMin = 10000000;
                    xMax = -10000000;
                    yMax = -10000000;

                    data8 = WriteDoubleLittle(xMin);
                    fs.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMin);
                    fs.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(xMax);
                    fs.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMax);
                    fs.Write(data8, 0, 8);
                    //the number of parts in the polyline
                    data = WriteIntLittle(1);
                    fs.Write(data, 0, 4);
                    //the total number of points of all polylines
                    data = WriteIntLittle(edgePoints);
                    fs.Write(data, 0, 4);
                    //index of first point of each polyline
                    data = WriteIntLittle(0);
                    fs.Write(data, 0, 4);
                    //points of each polyline -> have to exported in clockwise direction!

                    data8 = WriteDoubleLittle(domain.MainForm.GralDomRect.West);
                    fs.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(domain.MainForm.GralDomRect.North);
                    fs.Write(data8, 0, 8);

                    data8 = WriteDoubleLittle(domain.MainForm.GralDomRect.East);
                    fs.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(domain.MainForm.GralDomRect.North);
                    fs.Write(data8, 0, 8);

                    data8 = WriteDoubleLittle(domain.MainForm.GralDomRect.East);
                    fs.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(domain.MainForm.GralDomRect.South);
                    fs.Write(data8, 0, 8);

                    data8 = WriteDoubleLittle(domain.MainForm.GralDomRect.West);
                    fs.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(domain.MainForm.GralDomRect.South);
                    fs.Write(data8, 0, 8);

                    data8 = WriteDoubleLittle(domain.MainForm.GralDomRect.West);
                    fs.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(domain.MainForm.GralDomRect.North);
                    fs.Write(data8, 0, 8);


                    //filelength updated
                    double dummy = Convert.ToDouble(fs.Length * 0.5);
                    int filelength = (int)dummy;

                    data = WriteIntBig(filelength);
                    fs.Position = 24;
                    fs.Write(data, 0, 4);
                    fs.Close(); fshx.Close();
                    fs.Dispose(); fshx.Dispose();
                }
            }
            //export contour lines
            else if (shapetype == 12)
            {
                shapetype = 3; // shapetype polyline
                // search drawing object
                GralDomain.DrawingObjects _drobj = new GralDomain.DrawingObjects("");
                foreach (GralDomain.DrawingObjects _dr in domain.ItemOptions)
                {
                    if (_dr.Name.StartsWith("CM") && _dr.Show)
                    {
                        _drobj = _dr;
                        break;
                    }
                }

                //write main file header
                (FileStream fs, FileStream fshx) = writeHeader(filename, shapetype, _drobj.ContourPolygons.Count);
                if (fs != null && fshx != null)
                {
                    //bounding box
                    xMin = 10000000;
                    yMin = 10000000;
                    xMax = -10000000;
                    yMax = -10000000;
                    double x1 = 0;
                    double y1 = 0;
                    double x2 = 0;
                    double y2 = 0;

                    for (int j = 0; j < _drobj.ContourPolygons.Count; j++)  // loop over all iso-rings
                    {
                        GralDomain.PointD[] _pts = _drobj.ContourPolygons[j].EdgePoints;
                        for (int i = 0; i < _pts.Length; i++)
                        {
                            x1 = _pts[i].X;
                            y1 = _pts[i].Y;

                            xMin = Math.Min(xMin, x1);
                            yMin = Math.Min(yMin, y1);
                            xMax = Math.Max(xMax, x1);
                            yMax = Math.Max(yMax, y1);
                        }
                    }

                    data8 = WriteDoubleLittle(xMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMin);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(xMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(yMax);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    data8 = WriteDoubleLittle(0);
                    fs.Write(data8, 0, 8);
                    fshx.Write(data8, 0, 8);
                    int recordNumber = 0;
                    int offset = 50;
                    //write records: header and record to .shp file
                    //offset and index to .shx file
                    for (int j = 0; j < _drobj.ContourPolygons.Count; j++)  // loop over all iso-rings
                    {
                        GralDomain.PointD[] _pts = _drobj.ContourPolygons[j].EdgePoints;

                        recordNumber += 1;

                        //record number
                        data = WriteIntBig(recordNumber);
                        fs.Write(data, 0, 4);
                        //offset
                        data = WriteIntBig(offset);
                        fshx.Write(data, 0, 4);
                        offset = offset + 4 + 24 + 8 * (_pts.Length + 1);
                        //content length
                        data = WriteIntBig(24 + 8 * (_pts.Length + 1));
                        fs.Write(data, 0, 4);
                        fshx.Write(data, 0, 4);
                        //shape type
                        data = WriteIntLittle(3);
                        fs.Write(data, 0, 4);
                        //Box
                        xMin = 10000000;
                        yMin = 10000000;
                        xMax = -10000000;
                        yMax = -10000000;
                        for (int i = 0; i < _pts.Length; i++)
                        {
                            x1 = _pts[i].X;
                            y1 = _pts[i].Y;
                            if (i == _pts.Length - 1) // last point
                            {
                                x2 = _pts[0].X;
                                y2 = _pts[0].Y;
                            }
                            else
                            {
                                x2 = _pts[i + 1].X;
                                y2 = _pts[i + 1].Y;
                            }
                            xMin = Math.Min(xMin, x1);
                            xMin = Math.Min(xMin, x2);
                            yMin = Math.Min(yMin, y1);
                            yMin = Math.Min(yMin, y2);
                            xMax = Math.Max(xMax, x1);
                            xMax = Math.Max(xMax, x2);
                            yMax = Math.Max(yMax, y1);
                            yMax = Math.Max(yMax, y2);
                        }
                        data8 = WriteDoubleLittle(xMin);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(yMin);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(xMax);
                        fs.Write(data8, 0, 8);
                        data8 = WriteDoubleLittle(yMax);
                        fs.Write(data8, 0, 8);
                        //the number of parts in the polyline
                        data = WriteIntLittle(1);
                        fs.Write(data, 0, 4);
                        //the total number of points of all polylines
                        data = WriteIntLittle(_pts.Length + 1);
                        fs.Write(data, 0, 4);
                        //index of first point of each polyline
                        data = WriteIntLittle(0);
                        fs.Write(data, 0, 4);
                        //points of each polyline
                        for (int i = 0; i <= _pts.Length; i++)
                        {
                            if (i < _pts.Length)
                            {
                                x1 = _pts[i].X;
                                y1 = _pts[i].Y;
                            }
                            else // last point
                            {
                                x1 = _pts[0].X;
                                y1 = _pts[0].Y;
                            }
                            data8 = WriteDoubleLittle(x1);
                            fs.Write(data8, 0, 8);
                            data8 = WriteDoubleLittle(y1);
                            fs.Write(data8, 0, 8);
                        }
                    }

                    //filelength updated
                    double dummy = Convert.ToDouble(fs.Length * 0.5);
                    int filelength = (int)dummy;

                    data = WriteIntBig(filelength);
                    fs.Position = 24;
                    fs.Write(data, 0, 4);
                    fs.Close(); fshx.Close();
                    fs.Dispose(); fshx.Dispose();
                }
            }
            else
            {
                MessageBox.Show("Unknown shape type, unable to export file", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private (FileStream, FileStream) writeHeader(string fileName, int shapeType, int numberOfItems)
        {
            FileStream fs = null;
            FileStream fshx = null;
            try
            {
                //construct file name for index file .shx
                string shxFile = fileName.Replace(".shp", ".shx");
                //write main file header
                fs = new FileStream(fileName, FileMode.Create);
                fshx = new FileStream(shxFile, FileMode.Create);
                Byte[] data = new Byte[4];
                Byte[] data8 = new Byte[8];
                //version
                data = WriteIntBig(9994);
                fs.Write(data, 0, 4);
                fshx.Write(data, 0, 4);
                //not used
                data = WriteIntBig(0);
                fs.Write(data, 0, 4);
                fshx.Write(data, 0, 4);
                data = WriteIntBig(0);
                fs.Write(data, 0, 4);
                fshx.Write(data, 0, 4);
                data = WriteIntBig(0);
                fs.Write(data, 0, 4);
                fshx.Write(data, 0, 4);
                data = WriteIntBig(0);
                fs.Write(data, 0, 4);
                fshx.Write(data, 0, 4);
                data = WriteIntBig(0);
                fs.Write(data, 0, 4);
                fshx.Write(data, 0, 4);
                //filelength (needs to be updated at the end of the stream
                data = WriteIntBig(1000);
                fs.Write(data, 0, 4);
                //filelength of .shx file =50+4*records
                int filelong = 50 + 4 * numberOfItems;
                data = WriteIntBig(filelong);
                fshx.Write(data, 0, 4);
                //version
                data = WriteIntLittle(1000);
                fs.Write(data, 0, 4);
                fshx.Write(data, 0, 4);
                //shape type
                data = WriteIntLittle(shapeType);
                fs.Write(data, 0, 4);
                fshx.Write(data, 0, 4);
            }
            catch
            {
                fs = null;
                fshx = null;
            }
            return (fs, fshx);
        }

        private byte[] WriteIntBig(int numb)
        {
            byte[] bytes = new byte[4];
            bytes = BitConverter.GetBytes(numb);
            Array.Reverse(bytes);
            return bytes;
        }

        private byte[] WriteIntLittle(int numb)
        {
            byte[] bytes = new byte[4];
            bytes = BitConverter.GetBytes(numb);
            return bytes;
        }

        private byte[] WriteDoubleLittle(double numb)
        {
            byte[] bytes = new byte[8];
            bytes = BitConverter.GetBytes(numb);
            return bytes;
        }
    }
}