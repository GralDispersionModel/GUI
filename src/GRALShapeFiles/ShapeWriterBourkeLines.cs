#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2024]  [Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using GralDomain;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace GralShape
{
    /// <summary>
    /// Write a shape file
    /// </summary>
    public partial class ShapeWriter
    {
#if __MonoCS__
        public void WriteBourkeLineShapeFile(string filename, DrawingObjects _drobj, ref DataSet dsline)
        {
            Byte[] data = new Byte[4];
            Byte[] data8 = new Byte[8];
#else
        public void WriteBourkeLineShapeFile(string filename, DrawingObjects _drobj, ref readonly DataSet dsline)
        {
            Span<byte> data = stackalloc byte[4];
            Span<byte> data8 = stackalloc Byte[8];
#endif
            //shape Type PolyLine
            const int shapetype = 3;

            //bounding box
            xMin = double.MaxValue;
            yMin = double.MaxValue;
            xMax = double.MinValue;
            yMax = double.MinValue;
            double x1, y1, x2, y2 = 0;
            
            // loop over all contour points, create the bounding box
            for (int i = 0; i < _drobj.ContourPoints.Count; i++)
            {
                int step = -2;
                int step_max = _drobj.ContourPoints[i].Count - 3;
                System.Drawing.PointF[] contourpoints_name_i = _drobj.ContourPoints[i].ToArray();
                while (step < step_max)
                {
                    step += 2;
                    x1 = contourpoints_name_i[step].X;
                    y1 = contourpoints_name_i[step].Y;
                    x2 = contourpoints_name_i[step + 1].X;
                    y2 = contourpoints_name_i[step + 1].Y;
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

            // loop over all contour points, creation of polylines that are as contiguous as possible
            List<BourkeLineData> points = new List<BourkeLineData>();
            for (int i = 0; i < _drobj.ContourPoints.Count; i++)
            {
                int step = 0;
                int step_max = _drobj.ContourPoints[i].Count - 2;
                System.Drawing.PointF[] contourpoints_name_i = _drobj.ContourPoints[i].ToArray();
                byte[] used = new byte[contourpoints_name_i.Length];

                while (step < step_max)
                {
                    // add the next polyline 
                    if (used[step] == 0)
                    {
                        //add the 1st and 2nd point
                        points.Add(new BourkeLineData());
                        int recentPolyline = points.Count - 1;
                        points[recentPolyline].ItemValue = _drobj.ItemValues[i];
                        points[recentPolyline].Pt.Add(new PointD(contourpoints_name_i[step].X, contourpoints_name_i[step].Y));
                        used[step] = 1;
                        points[recentPolyline].Pt.Add(new PointD(contourpoints_name_i[step + 1].X, contourpoints_name_i[step + 1].Y));
                        used[step + 1] = 1;

                        //search additional points
                        int innerLoop = 2;
                        while (innerLoop < step_max)
                        {
                            if (used[innerLoop] == 0)
                            {
                                // add a contigous line segment and start searching
                                if (Math.Abs(points[recentPolyline].Pt[points[recentPolyline].Pt.Count - 1].X - contourpoints_name_i[innerLoop].X) < 0.4 &&
                                    Math.Abs(points[recentPolyline].Pt[points[recentPolyline].Pt.Count - 1].Y - contourpoints_name_i[innerLoop].Y) < 0.4)
                                {
                                    used[innerLoop] = 1;
                                    points[recentPolyline].Pt.Add(new PointD(contourpoints_name_i[innerLoop + 1].X, contourpoints_name_i[innerLoop + 1].Y));
                                    used[innerLoop + 1] = 1;
                                    innerLoop = 0;
                                }
                                // add a contigous line segment and start searching
                                if (innerLoop > 2 && Math.Abs(points[recentPolyline].Pt[points[recentPolyline].Pt.Count - 1].X - contourpoints_name_i[innerLoop + 1].X) < 0.4 &&
                                    Math.Abs(points[recentPolyline].Pt[points[recentPolyline].Pt.Count - 1].Y - contourpoints_name_i[innerLoop + 1].Y) < 0.4)
                                {
                                    used[innerLoop] = 1;
                                    points[recentPolyline].Pt.Add(new PointD(contourpoints_name_i[innerLoop].X, contourpoints_name_i[innerLoop].Y));
                                    used[innerLoop + 1] = 1;
                                    innerLoop = 0;
                                }
                            }
                            innerLoop += 2;
                        }
                    }
                    step += 2;
                }
            }

            for (int loop = 0; loop < points.Count; loop++)
            {
                if (points[loop].Pt.Count == 0)
                {
                    points.RemoveAt(loop);
                    loop = 0;
                }
            }

            //write main file header
            (FileStream fs, FileStream fshx) = writeHeader(filename, shapetype, points.Count);
            if (fs != null && fshx != null)
            {
                //write bounding box
                data8 = WriteDoubleLittle(xMin);
                writeBytes(fs, data8);
                writeBytes(fshx, data8);
                data8 = WriteDoubleLittle(yMin);
                writeBytes(fs, data8);
                writeBytes(fshx, data8);
                data8 = WriteDoubleLittle(xMax);
                writeBytes(fs, data8);
                writeBytes(fshx, data8);
                data8 = WriteDoubleLittle(yMax);
                writeBytes(fs, data8);
                writeBytes(fshx, data8);
                data8 = WriteDoubleLittle(0);
                writeBytes(fs, data8);
                writeBytes(fshx, data8);
                data8 = WriteDoubleLittle(0);
                writeBytes(fs, data8);
                writeBytes(fshx, data8);
                data8 = WriteDoubleLittle(0);
                writeBytes(fs, data8);
                writeBytes(fshx, data8);
                data8 = WriteDoubleLittle(0);
                writeBytes(fs, data8);
                writeBytes(fshx, data8);
                int recordNumber = 0;
                int offset = 50;

                //write records: header and record to .shp file
                //offset and index to .shx file
                foreach (BourkeLineData _pts in points)
                {
                    if (_pts.Pt.Count > 0)
                    {
                        dsline.Tables[0].Rows.Add(_pts.ItemValue.ToString(), _drobj.LegendUnit.Replace("µ", "u").Replace("³", "3").Replace("²", "2"));
                        recordNumber += 1;
                        //record number
                        data = WriteIntBig(recordNumber);
                        writeBytes(fs, data);
                        //offset
                        data = WriteIntBig(offset);
                        writeBytes(fshx, data);
                        offset = offset + 4 + 24 + 8 * _pts.Pt.Count;
                        //content length
                        data = WriteIntBig(24 + 8 * _pts.Pt.Count);
                        writeBytes(fs, data);
                        writeBytes(fshx, data);
                        //shape type
                        data = WriteIntLittle(3);
                        writeBytes(fs, data);
                        //Box
                        xMin = double.MaxValue;
                        yMin = double.MaxValue;
                        xMax = double.MinValue;
                        yMax = double.MinValue;
                        for (int i = 0; i < _pts.Pt.Count - 1; i++)
                        {
                            x1 = _pts.Pt[i].X;
                            y1 = _pts.Pt[i].Y;
                            x2 = _pts.Pt[i + 1].X;
                            y2 = _pts.Pt[i + 1].Y;
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
                        writeBytes(fs, data8);
                        data8 = WriteDoubleLittle(yMin);
                        writeBytes(fs, data8);
                        data8 = WriteDoubleLittle(xMax);
                        writeBytes(fs, data8);
                        data8 = WriteDoubleLittle(yMax);
                        writeBytes(fs, data8);
                        //the number of parts in the polyline
                        data = WriteIntLittle(1);
                        writeBytes(fs, data);
                        //the total number of points of all polylines
                        data = WriteIntLittle(_pts.Pt.Count);
                        writeBytes(fs, data);
                        //index of first point of each polyline
                        data = WriteIntLittle(0);
                        writeBytes(fs, data);
                        //points of each polyline
                        for (int i = 0; i < _pts.Pt.Count; i++)
                        {
                            x1 = _pts.Pt[i].X;
                            y1 = _pts.Pt[i].Y;
                            data8 = WriteDoubleLittle(x1);
                            writeBytes(fs, data8);
                            data8 = WriteDoubleLittle(y1);
                            writeBytes(fs, data8);
                        }
                    }
                }

                //filelength updated
                long dummy = fs.Length / 2;
                int filelength = (int)dummy;
                data = WriteIntBig(filelength);
                fs.Position = 24;
                writeBytes(fs, data);
                fs.Close(); fshx.Close();
                fs.Dispose(); fshx.Dispose();
                //clean point list
                foreach (BourkeLineData _pts in points)
                {
                    _pts.Pt.Clear();
                    _pts.Pt.TrimExcess();
                }
                points.Clear();
                points.TrimExcess();
            }
        }
#if !__MonoCS__
        private void writeBytes(FileStream fs, Span<byte> bytes)
        {
            fs.Write(bytes);
        }
#endif
        private void writeBytes(FileStream fs, byte[] bytes)
        {
            fs.Write(bytes, 0, bytes.Length);
        }

        private class BourkeLineData
        {
            public double ItemValue { get; set; }
            public List<PointD> Pt { get; set; }

            public BourkeLineData()
            {
                ItemValue = 0;
                Pt = new List<PointD>();
            }
        }
    }
}