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
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace GralShape
{
    /// <summary>
    /// Start the shape reader to read the data table
    /// </summary>
    public class ShapeReader
    {
        private int filecode, filelength, version, shapetype;
        private double xMin, yMin, xMax, yMax, zMin, zMax, mMin, mMax;
        private readonly GralDomain.Domain domain = null;
        private readonly string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        private bool  _readbuildings = true;
        public bool ReadBuildings {set {_readbuildings = value;}}
        private double _west;
        public double West {get {return _west;}}
        private double _north;
        public double North {get {return _north;}}
        private double _pixelmx;
        public double PixelMx {get {return _pixelmx;}}
        private int _pixelmy;
        public int PixelMy {get {return _pixelmy;}}

        public ShapeReader(GralDomain.Domain d)
        {
            domain = d;
        }

        /// <summary>
        /// Reads and yields one element from the shape file
        /// </summary>
        /// <param name="filename">Shape file name</param>
        /// <returns></returns>
        public IEnumerable<object> ReadShapeFile(string filename)
        {
            //read file
            FileStream fs = new FileStream(filename, FileMode.Open);
            long fileLength = fs.Length;
            
            Byte[] data = new Byte[fileLength];
            fs.Read(data, 0, (int)fileLength);
            fs.Close();
            fs.Dispose();
            
            filecode = readIntBig(data, 0);
            filelength = readIntBig(data, 24);
            version = readIntLittle(data, 28);
            shapetype = readIntLittle(data, 32);
            xMin = readDoubleLittle(data, 36);
            yMin = readDoubleLittle(data, 44);
            yMin = 0 - yMin;
            xMax = readDoubleLittle(data, 52);
            yMax = readDoubleLittle(data, 60);
            yMax = 0 - yMax;
            zMin = readDoubleLittle(data, 68);
            zMax = readDoubleLittle(data, 76);
            mMin = readDoubleLittle(data, 84);
            mMax = readDoubleLittle(data, 92);

            //set map properties
            _west = xMin;
            _north = -yMax;
            _pixelmx = (xMax - xMin) / 100;
            _pixelmy = Convert.ToInt32((yMax - yMin) / Math.Max((xMax - xMin),1) * 100);
            _pixelmy = Math.Max(_pixelmy, 1);
            
            int currentPosition = 100;
            while (currentPosition < fileLength)
            {
                int recordStart = currentPosition;
                int recordNumber = readIntBig(data, recordStart);
                int contentLength = readIntBig(data, recordStart + 4);
                int recordContentStart = recordStart + 8;

                //Shape-Type: Point
                if (shapetype == 1)
                {
                    GralDomain.PointD point = new GralDomain.PointD();
                    int recordShapeType = readIntLittle(data, recordContentStart);
                    point.X = readDoubleLittle(data, recordContentStart + 4);
                    point.Y = readDoubleLittle(data, recordContentStart + 12);
                    //domain.shppoints[index].Add(point);
                    yield return point;
                        
                }

                //Shape-Type: PointZ
                if (shapetype == 11)
                {
                    GralData.PointD_3d point = new GralData.PointD_3d();
                    int recordShapeType = readIntLittle(data, recordContentStart);
                    point.X = readDoubleLittle(data, recordContentStart + 4);
                    point.Y = readDoubleLittle(data, recordContentStart + 12);
                    point.Z = readDoubleLittle(data, recordContentStart + 20);
                    //domain.shppoints[index].Add(point);
                    yield return point;
                }

                //Shape-Type: Polyline
                if (shapetype == 3)
                {
                    SHPLine line = new SHPLine();
                    int recordShapeType = readIntLittle(data, recordContentStart);
                    line.Box = new Double[4];
                    line.Box[0] = readDoubleLittle(data, recordContentStart + 4);
                    line.Box[1] = readDoubleLittle(data, recordContentStart + 12);
                    line.Box[2] = readDoubleLittle(data, recordContentStart + 20);
                    line.Box[3] = readDoubleLittle(data, recordContentStart + 28);
                    line.NumParts = readIntLittle(data, recordContentStart + 36);
                    line.Parts = new int[line.NumParts];
                    line.NumPoints = readIntLittle(data, recordContentStart + 40);
                    line.Points = new GralDomain.PointD[line.NumPoints];
                    int partStart = recordContentStart + 44;
                    for (int i = 0; i < line.NumParts; i++)
                    {
                        line.Parts[i] = readIntLittle(data, partStart + i * 4);
                    }
                    int pointStart = recordContentStart + 44 + 4 * line.NumParts;

                    for (int i = 0; i < line.NumPoints; i++)
                    {
                        line.Points[i].X = readDoubleLittle(data, pointStart + (i * 16));
                        line.Points[i].Y = readDoubleLittle(data, pointStart + (i * 16) + 8);
                    }
                    //domain.shplines[index].Add(line);
                    yield return line;
                }

                //Shape-Type: PolylineZ
                if (shapetype == 13) 
                {
                    SHPLine line = new SHPLine();
                    int recordShapeType = readIntLittle(data, recordContentStart);
                    line.Box = new Double[4];
                    line.Box[0] = readDoubleLittle(data, recordContentStart + 4);
                    line.Box[1] = readDoubleLittle(data, recordContentStart + 12);
                    line.Box[2] = readDoubleLittle(data, recordContentStart + 20);
                    line.Box[3] = readDoubleLittle(data, recordContentStart + 28);
                    line.NumParts = readIntLittle(data, recordContentStart + 36);
                    line.Parts = new int[line.NumParts];
                    line.NumPoints = readIntLittle(data, recordContentStart + 40);
                    line.Points = new GralDomain.PointD[line.NumPoints];
                    line.PointsZ = new double[line.NumPoints];

                    int partStart = recordContentStart + 44;
                    for (int i = 0; i < line.NumParts; i++)
                    {
                        line.Parts[i] = readIntLittle(data, partStart + i * 4);
                    }

                    int pointStart = recordContentStart + 44 + 4 * line.NumParts;
                    for (int i = 0; i < line.NumPoints; i++)
                    {
                        line.Points[i].X = readDoubleLittle(data, pointStart + (i * 16));
                        int index = pointStart + (i * 16) + 8;
                        line.Points[i].Y = readDoubleLittle(data, index);
                        index = pointStart + 16 * line.NumPoints + 16 + 8 * i;
                        line.PointsZ[i]  = readDoubleLittle(data, index);
                    }
                    //domain.shplines[index].Add(line);
                    yield return line;
                }

                //Shape-Type: PolylineM
                if (shapetype == 23)
                {
                    GralShape.SHPLine line = new GralShape.SHPLine();
                    int recordShapeType = readIntLittle(data, recordContentStart);
                    line.Box = new Double[4];
                    line.Box[0] = readDoubleLittle(data, recordContentStart + 4);
                    line.Box[1] = readDoubleLittle(data, recordContentStart + 12);
                    line.Box[2] = readDoubleLittle(data, recordContentStart + 20);
                    line.Box[3] = readDoubleLittle(data, recordContentStart + 28);
                    line.NumParts = readIntLittle(data, recordContentStart + 36);
                    line.Parts = new int[line.NumParts];
                    line.NumPoints = readIntLittle(data, recordContentStart + 40);
                    line.Points = new GralDomain.PointD[line.NumPoints];
                    int partStart = recordContentStart + 44;
                    for (int i = 0; i < line.NumParts; i++)
                    {
                        line.Parts[i] = readIntLittle(data, partStart + i * 4);
                    }
                    int pointStart = recordContentStart + 44 + 4 * line.NumParts;

                    for (int i = 0; i < line.NumPoints; i++)
                    {
                        line.Points[i].X = readDoubleLittle(data, pointStart + (i * 16));
                        line.Points[i].Y = readDoubleLittle(data, pointStart + (i * 16) + 8);
                    }
                    //domain.shplines[index].Add(line);
                    yield return line;
                }

                //Shape-Type: Polygon
                if (shapetype == 5)
                {
                    SHPPolygon polygon = new SHPPolygon();
                    int recordShapeType = readIntLittle(data, recordContentStart);
                    polygon.Box = new Double[4];
                    polygon.Box[0] = readDoubleLittle(data, recordContentStart + 4);
                    polygon.Box[1] = readDoubleLittle(data, recordContentStart + 12);
                    polygon.Box[2] = readDoubleLittle(data, recordContentStart + 20);
                    polygon.Box[3] = readDoubleLittle(data, recordContentStart + 28);
                    polygon.NumParts = readIntLittle(data, recordContentStart + 36);
                    polygon.Parts = new int[polygon.NumParts];
                    polygon.NumPoints = readIntLittle(data, recordContentStart + 40);
                    if (polygon.NumParts > 1)
                    {
                        polygon.Points = new GralDomain.PointD[polygon.NumPoints + polygon.NumParts];
                    }
                    else
                    {
                        polygon.Points = new GralDomain.PointD[polygon.NumPoints];
                    }

                    int partStart = recordContentStart + 44;
                    for (int i = 0; i < polygon.NumParts; i++)
                    {
                        polygon.Parts[i] = readIntLittle(data, partStart + i * 4);
                    }
                    int pointStart = recordContentStart + 44 + 4 * polygon.NumParts;
                                        
                    if(_readbuildings) // read buildings data
                    {
                        int parts = 0;
                        GralDomain.PointD[] ruecksprung = new GralDomain.PointD[polygon.NumParts];
                        for (int i = 0; i < polygon.NumPoints; i++)
                        {
                            polygon.Points[i].X = readDoubleLittle(data, pointStart + (i * 16));
                            polygon.Points[i].Y = readDoubleLittle(data, pointStart + (i * 16) + 8);
                            if (parts <  polygon.NumParts - 1)
                            {
                                if (i == polygon.Parts[parts + 1])
                                {
                                    ruecksprung[parts].X = polygon.Points[i].X;
                                    ruecksprung[parts].Y = polygon.Points[i].Y;
                                    if (parts < polygon.NumParts - 1)
                                    {
                                        parts++;
                                    }
                                }
                            }
                        }
                        
                        if (polygon.NumParts > 1)
                        {
                            parts --;
                            for (int i = polygon.NumPoints; i < (polygon.NumPoints + polygon.NumParts -1); i++)
                            {
                                polygon.Points[i].X = ruecksprung[parts].X;
                                polygon.Points[i].Y = ruecksprung[parts].Y;
                                parts--;
                            }
                        }
                        //SHPPolygons[index].Add(polygon);
                        yield return polygon;
                    }
                    else // read shapefile for view only - seperate shapefile-parts
                    {
                        int c = 0;
                        for (int parts = 0; parts < polygon.NumParts; parts++)
                        {
                            int end = 0;
                            if ((polygon.NumParts - parts) > 1)
                            {
                                end = polygon.Parts[parts + 1];
                            }
                            else
                            {
                                end = polygon.NumPoints;
                            }

                            polygon.Points = null;
                            polygon.Points = new GralDomain.PointD[end - polygon.Parts[parts]];
                            int cp = 0;
                            for (int i = polygon.Parts[parts]; i < end; i++)
                            {
                                polygon.Points[cp].X = readDoubleLittle(data, pointStart + (c * 16));
                                polygon.Points[cp].Y = readDoubleLittle(data, pointStart + (c * 16) + 8);
                                c++;cp++;
                            }
                            //SHPPolygons[index].Add(polygon); // add this ring
                            yield return polygon;							
                        }
                    }
                }

                //Shape-Type: PolygonZ
                if (shapetype == 15)
                {
                    SHPPolygon polygon = new SHPPolygon();
                    int recordShapeType = readIntLittle(data, recordContentStart);
                    polygon.Box = new Double[4];
                    polygon.Box[0] = readDoubleLittle(data, recordContentStart + 4);
                    polygon.Box[1] = readDoubleLittle(data, recordContentStart + 12);
                    polygon.Box[2] = readDoubleLittle(data, recordContentStart + 20);
                    polygon.Box[3] = readDoubleLittle(data, recordContentStart + 28);
                    polygon.NumParts = readIntLittle(data, recordContentStart + 36);
                    polygon.Parts = new int[polygon.NumParts];
                    polygon.NumPoints = readIntLittle(data, recordContentStart + 40);
                    if (polygon.NumParts > 1)
                    {
                        polygon.Points = new GralDomain.PointD[polygon.NumPoints + polygon.NumParts];
                    }
                    else
                    {
                        polygon.Points = new GralDomain.PointD[polygon.NumPoints];
                    }

                    int partStart = recordContentStart + 44;
                    
                    for (int i = 0; i < polygon.NumParts; i++)
                    {
                        polygon.Parts[i] = readIntLittle(data, partStart + i * 4);
                    }
                    int pointStart = recordContentStart + 44 + 4 * polygon.NumParts;
                    
                    if(_readbuildings) // read buildings data
                    {
                        int parts = 0;
                        GralDomain.PointD[] ruecksprung = new GralDomain.PointD[polygon.NumParts];
                        for (int i = 0; i < polygon.NumPoints; i++)
                        {
                            polygon.Points[i].X = readDoubleLittle(data, pointStart + (i * 16));
                            polygon.Points[i].Y = readDoubleLittle(data, pointStart + (i * 16) + 8);
                            if (parts <  polygon.NumParts - 1)
                            {
                                if (i == polygon.Parts[parts + 1])
                                {
                                    ruecksprung[parts].X = polygon.Points[i].X;
                                    ruecksprung[parts].Y = polygon.Points[i].Y;
                                    if (parts < polygon.NumParts - 1)
                                    {
                                        parts++;
                                    }
                                }
                            }
                        }
                        
                        if (polygon.NumParts > 1)
                        {
                            parts --;
                            for (int i = polygon.NumPoints; i < (polygon.NumPoints + polygon.NumParts -1); i++)
                            {
                                polygon.Points[i].X = ruecksprung[parts].X;
                                polygon.Points[i].Y = ruecksprung[parts].Y;
                                parts--;
                            }
                        }
                        //SHPPolygons[index].Add(polygon);
                        yield return polygon;
                    }
                    else // read shapefile for view only - seperate shapefile-parts
                    {
                        int c = 0;
                        for (int parts = 0; parts < polygon.NumParts; parts++)
                        {
                            int end = 0;
                            if ((polygon.NumParts - parts) > 1)
                            {
                                end = polygon.Parts[parts + 1];
                            }
                            else
                            {
                                end = polygon.NumPoints;
                            }

                            polygon.Points = null;
                            polygon.Points = new GralDomain.PointD[end - polygon.Parts[parts]];
                            int cp = 0;
                            for (int i = polygon.Parts[parts]; i < end; i++)
                            {
                                polygon.Points[cp].X = readDoubleLittle(data, pointStart + (c * 16));
                                polygon.Points[cp].Y = readDoubleLittle(data, pointStart + (c * 16) + 8);
                                c++;cp++;
                            }
                            //SHPPolygons[index].Add(polygon); // add this ring
                            yield return polygon;							
                        }
                    }
                    
                }
                currentPosition = recordStart + (4 + contentLength) * 2;
            }
            
            data = null; // release memory
            
            yield break;
        }

        private int readIntBig(byte[] data, int pos)
        {
            byte[] bytes = new byte[4];
            bytes[0] = data[pos];
            bytes[1] = data[pos+1];
            bytes[2] = data[pos+2];
            bytes[3] = data[pos+3];
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        private int readIntLittle(byte[] data, int pos)
        {
            byte[] bytes = new byte[4];
            bytes[0] = data[pos];
            bytes[1] = data[pos + 1];
            bytes[2] = data[pos + 2];
            bytes[3] = data[pos + 3];
            return BitConverter.ToInt32(bytes, 0);
        }

        private double readDoubleLittle(byte[] data, int pos)
        {
            byte[] bytes = new byte[8];
            bytes[0] = data[pos];
            bytes[1] = data[pos + 1];
            bytes[2] = data[pos + 2];
            bytes[3] = data[pos + 3];
            bytes[4] = data[pos + 4];
            bytes[5] = data[pos + 5];
            bytes[6] = data[pos + 6];
            bytes[7] = data[pos + 7];
            return BitConverter.ToDouble(bytes, 0);
        }
    }
}