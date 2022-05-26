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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace GralDomain
{
   public partial class Domain
   {
        /// <summary>
        /// Draw Contour Maps
        /// </summary>
        private void DrawContourMap(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north, 
                                    double factor_x, double factor_y, Font LabelFont, Brush LabelBrush)
        {
            int minx;
            int maxx;
            int miny;
            int maxy;
            
            int pb1_height = picturebox1.Height;
            int pb1_width  = picturebox1.Width;
            
            int[,] contourcolor_name 	= _drobj.ContourColor;
            int transparancy_name 		= _drobj.Transparancy;
            GralDomain.DrawingObjects.ContourGeometries contourgeometry_name = _drobj.ContourGeometry;
            
            StringFormat StringFormat1 = new StringFormat(); //format for names of sources
            StringFormat StringFormat2 = new StringFormat(); //format for names of line sources
            StringFormat1.LineAlignment = StringAlignment.Center;
            StringFormat1.Alignment = StringAlignment.Center;
            StringFormat2.LineAlignment = StringAlignment.Far;
            StringFormat2.Alignment = StringAlignment.Center;

            #region Draw_Vertical_Concentration
            if (_drobj.ShpPoints.Count > 2 ) // draw white box & Axes
            {
                Brush pen_white = new SolidBrush(Color2Transparent(transparancy_name, Color.White));
                Pen pen_terrain = new Pen(Color.Brown, 2);
                Pen pen_black = new Pen (Color.Black);
                RectangleF RectWhite = new RectangleF();
                
                float _x0 = _drobj.ShpPoints[0].X;
                float _y0 = _drobj.ShpPoints[0].Y;
                float _dx = _drobj.ShpPoints[1].X;
                int _NII   = (int) _drobj.ShpPoints[2].X;
                int _NJJ = (int) _drobj.ShpPoints[2].Y - 1;
                float maxh = _drobj.ShpPoints[3].X;

                int lenght_of_string = (int) g.MeasureString("9999", LabelFont).Width;
                RectWhite.X = (float) ((_x0 - form1_west) * factor_x) +TransformX - 10 - lenght_of_string;
                RectWhite.Y = (float) ((_y0  - form1_north) * factor_y - _dx * _NJJ * factor_x) +TransformY - LabelFont.Height - 10;
                RectWhite.Width = (float) (_dx * _NII * factor_x) + 20 + 2* lenght_of_string;
                RectWhite.Height = (float) (_dx * _NJJ * factor_x) + 30 + 2 * LabelFont.Height;

                g.FillRectangle (pen_white, RectWhite );
                g.DrawRectangle(pen_black, (int) RectWhite.X, (int) RectWhite.Y, (int) RectWhite.Width, (int) RectWhite.Height);

                RectWhite.X = (float) ((_x0 - form1_west) * factor_x) + TransformX;
                RectWhite.Y = (float) ((_y0  - form1_north) * factor_y - _dx * (_NJJ) * factor_x) + TransformY;
                RectWhite.Width = (float) (_dx * _NII * factor_x);
                RectWhite.Height = (float) (_dx * _NJJ * factor_x);

                // draw coordinate system
                pen_black.EndCap = LineCap.ArrowAnchor;
                int _xmin = (int) Math.Max(RectWhite.X, 10 + lenght_of_string);
                g.DrawLine (pen_black, _xmin, (int) RectWhite.Bottom, (int) RectWhite.Right, (int) RectWhite.Bottom);
                pen_black.EndCap = LineCap.Flat;
                
                double x_step = (_dx * _NII) / 10;
                int exp = 0;
                if (x_step != 0.0)
                {
                    exp = (int) Math.Floor(Math.Log10(x_step)) * (-1);
                }

                double norm = x_step * Math.Pow(10, exp);
                if (norm < 1.5)
                {
                    norm = 1;
                }
                else if (norm < 3.4)
                {
                    norm  = 2;
                }
                else if (norm < 6.5)
                {
                    norm = 5;
                }
                else if (norm < 9)
                {
                    norm = 8;
                }
                else
                {
                    norm = 10;
                }

                exp *= -1;
                x_step = norm * Math.Pow(10, exp);
                
                int _xc = 0; int i = 0;
                while (_xc < RectWhite.Right && i < 100)
                {
                    if (_xc >= _xmin)
                    {
                        g.DrawLine (pen_black, _xc, (int) RectWhite.Bottom - 4, _xc , (int) RectWhite.Bottom + 4);
                        g.DrawString ((i * x_step).ToString (), LabelFont, LabelBrush, _xc, (int) RectWhite.Bottom + LabelFont.Height + 5, StringFormat1);
                    }
                    ++i;
                    _xc = (int) (RectWhite.X + x_step * i * factor_x);
                }

                x_step = (_dx * _NJJ) / 10;
                exp = 0;
                if (x_step != 0.0)
                {
                    exp = (int) Math.Floor(Math.Log10(x_step)) * (-1);
                }

                norm = x_step * Math.Pow(10, exp);
                if (norm < 1.5)
                {
                    norm = 1;
                }
                else if (norm < 3.4)
                {
                    norm  = 2;
                }
                else if (norm < 6.5)
                {
                    norm = 5;
                }
                else if (norm < 9)
                {
                    norm = 8;
                }
                else
                {
                    norm = 10;
                }

                exp *= -1;
                x_step = norm * Math.Pow(10, exp);
                
                if (RectWhite.Right > 50) // if right edge inside actual view
                {
                    int _y0_ = 0;
                    if (x_step >= 0)
                    {
                        _y0_ = (int)(x_step * (int)(maxh / x_step));
                    }
                    int _yc = 0;
                    i = 0;
                    if (_y0_ == 0)
                    {
                        _yc = (int) (RectWhite.Bottom);
                    }
                    else
                    {
                        _yc = (int) (RectWhite.Bottom - maxh / _y0_ * factor_x);
                    }
                    pen_black.EndCap = LineCap.ArrowAnchor;
                    g.DrawLine (pen_black, _xmin, (int) RectWhite.Bottom, _xmin, (int) RectWhite.Top);
                    pen_black.EndCap = LineCap.Flat;
                    
                    while (_yc > RectWhite.Top && i < 100)
                    {
                        g.DrawLine (pen_black, (int) _xmin -4, _yc, (int) _xmin  + 4 , _yc);
                        g.DrawString ((i * x_step + _y0_).ToString (), LabelFont, LabelBrush, (int) (_xmin - lenght_of_string), _yc - LabelFont.Height / 2);
                        ++i;
                        if (_y0_ == 0)
                        {
                            _yc = (int) (RectWhite.Bottom -  i * factor_x * x_step);
                        }
                        else
                        {
                            _yc = (int) (RectWhite.Bottom - maxh / _y0_ * factor_x -  i * factor_x * x_step);
                        }
                    }
                }
                
                // draw terrain
                for (i = 4; i < _drobj.ShpPoints.Count - 1; i++)
                {
                    int __x0 = (int) ((_drobj.ShpPoints[i].X - form1_west) * factor_x) +TransformX;
                    int __y0 = (int) ((_drobj.ShpPoints[i].Y  - form1_north) * factor_y - _dx * _NJJ * factor_x) +TransformY;
                    int __x1 = (int) ((_drobj.ShpPoints[i + 1].X - form1_west) * factor_x) +TransformX;
                    int __y1 = (int) ((_drobj.ShpPoints[i + 1].Y  - form1_north) * factor_y - _dx * _NJJ * factor_x) +TransformY;
                    
                    if (__x1 >= _xmin)
                    {
                        __x0 = Math.Max(__x0, _xmin);
                        if (__y0 < RectWhite.Bottom && __y1 < RectWhite.Bottom)
                        {
                            g.DrawLine (pen_terrain, __x0, __y0, __x1, __y0);
                            
                            if ( i > 3) // draw vertical lines
                            {
                                g.DrawLine (pen_terrain, __x1, __y0, __x1, __y1);
                            }
                        }
                    }

                }

                Pen p3 = new Pen(Color.Black, 1)
                {
                    DashStyle = DashStyle.Dash
                };
                pen_white.Dispose ();
                pen_black.Dispose ();
                pen_terrain.Dispose ();
            }
            #endregion Draw_Vertical_Concentration
//

            if (_drobj.DrawSimpleContour && _drobj.ContourPolygons != null) // draw simple contour lines
            {
                
                int label_interval = Math.Max(1, _drobj.LabelInterval);
                Matrix mat = new Matrix();
                GraphicsPath gp = new GraphicsPath();
                Region excludeRegion = new Region(gp);
                
                float x1 = _drobj.ContourClipping.Left;
                float w = _drobj.ContourClipping.Width;
                float y1 = _drobj.ContourClipping.Top;
                float h = _drobj.ContourClipping.Height;
                
                RectangleF clipping = new RectangleF(
                    (float) (((x1 - form1_west) * factor_x) + TransformX),
                    (float) (((y1 - form1_north) * factor_y) + TransformY),
                    (float) (w * factor_x ),
                    (float) ((-h)  * factor_y ));
                
                if(_drobj.ContourDrawBorder)
                {
                    Pen penClip = new Pen(_drobj.LineColor, _drobj.LineWidth);
                    g.DrawRectangle(penClip, clipping.X, clipping.Y, clipping.Width, clipping.Height);
                    penClip.Dispose();
                }
                
                g.SetClip(clipping, CombineMode.Replace);
                double labellenght = Convert.ToDouble(_drobj.ContourLabelDist) * factor_x;
                int lenght_of_string = (int) g.MeasureString("9999", LabelFont).Width;
                labellenght = Math.Max(lenght_of_string, labellenght * 2);
                
                for (int i = 0; i < _drobj.ContourPolygons.Count; i++)  // loop over all iso-rings
                {
                    double labellenghtact = 0;
                    string label_value;
                    
                    int k = _drobj.ContourPolygons[i].ItemValueIndex; // index of contour value
                    k = Math.Max(0, Math.Min(k, _drobj.FillColors.Count));
                    
                    int transparancy_new = transparancy_name;
                    if (_drobj.FillColors[k] == Color.White)
                    {
                        transparancy_new = 0;
                    }

                    Brush fill_contour = new SolidBrush(Color2Transparent(transparancy_new, _drobj.FillColors[k]));
                    
                    Pen myPen;
                    if ((label_interval > 1) && (k % label_interval) == 0) // linewidth +1 if label-line
                    {
                        myPen = new Pen(Color2Transparent(255, _drobj.LineColors[k]), _drobj.LineWidth + 1);
                    }
                    else
                    {
                        myPen = new Pen(Color2Transparent(255, _drobj.LineColors[k]), _drobj.LineWidth);
                    }

                    Pen labelPen = new Pen(Color2Transparent(255, _drobj.LineColors[k]), Math.Min(2, _drobj.LineWidth));
                    
                    PointD[] _pts = _drobj.ContourPolygons[i].EdgePoints;
                    int numberofvertices = _pts.Length;
                    
                    if (numberofvertices > 4)
                    {
                        PointF[] pts = new PointF[numberofvertices];
                        for (int j = 0; j < numberofvertices; j++)
                        {
                            pts[j].X = (float) (((_pts[j].X - form1_west) * factor_x) + TransformX);
                            pts[j].Y = (float) (((_pts[j].Y - form1_north) * factor_y) + TransformY);
                        }
                        
                        if (_drobj.FillYesNo == true)
                        {
                            // Draw ISO Rings if transparency is set
                            if (_drobj.Transparancy < 252)
                            {
                                using (GraphicsPath IsoLineRing = new GraphicsPath())
                                {
                                    IsoLineRing.AddClosedCurve(pts, _drobj.ContourTension);
                                    
                                    // loop over all higher iso-rings
                                    for (int ik = i + 1; ik < _drobj.ContourPolygons.Count; ik++)  
                                    {
                                        // if ring is one step above recent ring -> hole
                                        if (_drobj.ContourPolygons[i].ItemValueIndex == _drobj.ContourPolygons[ik].ItemValueIndex - 1)
                                        {
                                            // is the ring i outside of the ring ik ?
                                            if (_drobj.ContourPolygons[i].Bounds.East < _drobj.ContourPolygons[ik].Bounds.West ||
                                                _drobj.ContourPolygons[i].Bounds.West > _drobj.ContourPolygons[ik].Bounds.East ||
                                                _drobj.ContourPolygons[i].Bounds.North < _drobj.ContourPolygons[ik].Bounds.South ||
                                                _drobj.ContourPolygons[i].Bounds.South > _drobj.ContourPolygons[ik].Bounds.North)
                                            {;} 
                                            else
                                            {
                                                PointD[] _ptsk = _drobj.ContourPolygons[ik].EdgePoints;
                                                PointF[] pts_k = new PointF[_ptsk.Length];
                                                for (int j = 0; j < _ptsk.Length; j++)
                                                {
                                                    pts_k[j].X = (float) (((_ptsk[j].X - form1_west) * factor_x) + TransformX);
                                                    pts_k[j].Y = (float) (((_ptsk[j].Y - form1_north) * factor_y) + TransformY);
                                                }
                                                IsoLineRing.AddClosedCurve(pts_k, _drobj.ContourTension);
                                            }
                                        }
                                    }
                                    g.FillPath(fill_contour, IsoLineRing);
                                    IsoLineRing.Reset();
                                }
                            }
                            else
                            {
                                g.FillClosedCurve(fill_contour, pts, FillMode.Alternate, _drobj.ContourTension);
                            }
                        }
                        
                        for(int j = 0; j < numberofvertices; j++)
                        {
                            PointF pt1 = pts[j];
                            PointF pt2;
                            if (j < numberofvertices - 1)
                            {
                                pt2 = pts[j + 1];
                            }
                            else
                            {
                                pt2 = pts[0];
                            }
                            
                            //show label_value
                            if ((_drobj.Label == 2 || _drobj.Label == 3) && (k % label_interval) == 0) // show labels and label interval = OK?
                            {
                                labellenghtact += Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2));
                                
                                if (labellenghtact > labellenght)
                                {
                                    //labelpoints.Add(new PointF((float)x1,(float)y1)); // to calculate the distance between label_value
                                    labellenghtact = 0;
                                    
                                    int x = Convert.ToInt32(pt1.X);
                                    int y = Convert.ToInt32(pt1.Y);
                                    
                                    if (x > 0 && y > 0 && x < pb1_width && y < pb1_height)
                                    {
                                        label_value = Convert.ToString(Math.Round(_drobj.ItemValues[k], _drobj.DecimalPlaces));
                                        double angle = 90;
                                        if (pt2.X != pt1.X)
                                        {
                                            angle = Math.Atan((pt2.Y - pt1.Y) / (pt2.X - pt1.X)) * 180 / 3.14;
                                        }

                                        SizeF stringSize = g.MeasureString(label_value, LabelFont);
                                        int sW = (int) (stringSize.Width * 0.5) + 2 ;
                                        int sH = (int) (stringSize.Height * 0.5) + 1;
                                        
                                        gp.AddRectangle(new Rectangle(x - sW , y- sH,  sW * 2, sH * 2 ));
                                        
                                        mat.Reset();
                                        mat.RotateAt((float) angle, new Point(x, y));

                                        gp.Transform(mat);
                                        g.Transform = mat;

                                        if (_drobj.LabelColor == Color.White)
                                        {
                                            g.DrawString(label_value, LabelFont, fill_contour, x, y, StringFormat1);
                                        }
                                        else
                                        {
                                            g.DrawString(label_value, LabelFont, LabelBrush, x, y, StringFormat1);
                                        }
                                        
                                        g.ResetTransform();

                                        #if __MonoCS__
                                        #else
                                        excludeRegion.Union (gp);
                                        if (_drobj.Label == 3)
                                        {
                                            g.DrawPath(labelPen, gp); // draw rectangle around label
                                        }

                                        g.ExcludeClip(excludeRegion); // exclude label region from redraw
                                        gp.Reset();
                                        excludeRegion.MakeEmpty();
                                        #endif
                                    }
                                }
                            }
                            if (CancelDrawing)
                            {
                                break;
                            }
                        } // show labels
                        
                        if (_drobj.LineWidth > 0) // draw contour lines
                        {
                            g.DrawClosedCurve(myPen, pts, _drobj.ContourTension, FillMode.Alternate);
                        }
                        
                    }
                    myPen.Dispose();
                    fill_contour.Dispose();
                }
                g.ResetClip();
            }
            else // Bourke - Contour lines
            {
                //fill contour map
                if (_drobj.FillYesNo == true)
                {
                    //when pixel size is larger than contour map resolution it is faster to loop over the pixels
                    if ((BmpScale * MapSize.SizeX) * 2 > contourgeometry_name.DX)
                    {
                        minx = (int)((contourgeometry_name.X0 - form1_west) * factor_x) + TransformX;
                        maxx = (int)((contourgeometry_name.X0 + contourgeometry_name.DX * contourgeometry_name.NX - form1_west) * factor_x) + TransformX;
                        miny = (int)((contourgeometry_name.Y0 + contourgeometry_name.DX * contourgeometry_name.NY - form1_north) * factor_y) + TransformY;
                        maxy = (int)((contourgeometry_name.Y0 - form1_north) * factor_y) + TransformY;

                        double factor_x1 = BmpScale * MapSize.SizeX / contourgeometry_name.DX;
                        double factor_y1 = BmpScale * MapSize.SizeY / contourgeometry_name.DX;

                        //ParallelOptions pOptions = new ParallelOptions
                        //{
                        //    MaxDegreeOfParallelism = Environment.ProcessorCount
                        //};
                        Object thisLock = new Object();

                        //for (int i = minx + 1; i < maxx; i++)
                        //Parallel.For(minx + 1, maxx, pOptions, i =>
                        Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(minx + 1, maxx, Math.Max(4, (int)((maxx - minx - 1) / Environment.ProcessorCount))), range =>
                        {
                            Bitmap bm_loc = new Bitmap(range.Item2 - range.Item1, maxy - miny);   //bitmap for drawing one pixel (for filling contour maps)
                            bool inside = false;
                            List<Color> _colLocal = _drobj.FillColors;
#if NET6_0_OR_GREATER
                            // Avoid SetPixel for better performance
                            Color _trans = Color2Transparent(0, Color.White);
                            byte[] _transByte = new byte[4] { _trans.B, _trans.R, _trans.G, _trans.A};
                            
                            System.Drawing.Imaging.BitmapData bmdata = bm_loc.LockBits(new Rectangle(0, 0, bm_loc.Width, bm_loc.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            int stride = bmdata.Stride;
                            unsafe
                            {
                                byte* ptr_bmloc = (byte*)bmdata.Scan0;
#endif                           
                                int x_bmloc = 0;
                                for (int i = range.Item1; i < range.Item2; i++)
                                {
                                    int y_loc = 0;
                                    int indexx_loc;
                                    int indexy_loc;
                                    int col;
                                    for (int j = miny + 1; j < maxy; j++)
                                    {
                                        if ((i >= 0) && (i <= pb1_width) && (j >= 0) && (j <= pb1_height))
                                        {
                                            inside = true;
                                            indexx_loc = (int)((i - minx) * factor_x1);
                                            indexy_loc = (int)(-(maxy - j) * factor_y1);
                                            col = contourcolor_name[indexx_loc, indexy_loc];
                                            if (col > -1)
                                            {
                                                //white color is drawn completely transparant
                                                int transparancy_new = transparancy_name;
                                                if (_colLocal[col] == Color.White)
                                                {
                                                    transparancy_new = 0;
                                                }
#if NET6_0_OR_GREATER
                                                Color _col = Color2Transparent(transparancy_new, _colLocal[col]);
                                                int _pos = x_bmloc * 4 + y_loc * stride;
                                                ptr_bmloc[_pos] = _col.B;
                                                ptr_bmloc[_pos + 1] = _col.G;
                                                ptr_bmloc[_pos + 2] = _col.R;
                                                ptr_bmloc[_pos + 3] = _col.A;
#else
                                                bm_loc.SetPixel(x_bmloc, y_loc, Color2Transparent(transparancy_new, _drobj.FillColors[col]));
#endif
                                            }
#if NET6_0_OR_GREATER
                                            else
                                            {
                                                int _pos = x_bmloc * 4 + y_loc * stride;
                                                ptr_bmloc[_pos] = _trans.B;
                                                ptr_bmloc[_pos + 1] = _trans.G;
                                                ptr_bmloc[_pos + 2] = _trans.R;
                                                ptr_bmloc[_pos + 3] = _trans.A;
                                            }
#endif
                                        }

                                        y_loc++;
                                    }
                                    x_bmloc++;
                                }
#if NET6_0_OR_GREATER
                            }
#endif
                            
#if NET6_0_OR_GREATER
                            bm_loc.UnlockBits(bmdata);
#endif
                            if (inside)
                            {
                                lock (thisLock)
                                {
                                    g.DrawImageUnscaled(bm_loc, range.Item1, miny + 1);
                                }
                            }

                            bm_loc.Dispose();
                         });
                        thisLock = null;
                    }
                    else
                    {
                        double west = MapSize.West;
                        double north = MapSize.North;
                        int recwidth = (int)(_drobj.ContourGeometry.DX / BmpScale / MapSize.SizeX);

                        //some parameters needed to improve filling the contours due to round-off errors
                        double x0 = contourgeometry_name.X0;
                        double y0 = contourgeometry_name.Y0;
                        double dx = contourgeometry_name.DX;
                        int ixold = (int)((x0 - dx * 0.5 - west) * factor_x) + TransformX;
                        int addonex = 0;
                        int iyold = (int)((y0 - dx * 0.5 - north) * factor_y) + TransformY;
                        int addoney = 0;
                        
                        int i_max = contourgeometry_name.NX;
                        int j_max = contourgeometry_name.NY;
                        

                        for (int i = 0; i < i_max; i++)
                        {
                            int ix = (int)((x0 + dx * i - west) * factor_x) + TransformX;
                            addonex = 0;
                            if (ix > ixold + recwidth)
                            {
                                ix--;
                                addonex = 1;
                            }
                            else if (ix < ixold + recwidth)
                            {
                                ix++;
                                addonex = -1;
                            }
                            ixold = ix + addonex;
                            for (int j = 0; j < j_max; j++)
                            {
                                int iy = (int)((y0 + dx * j - north) * factor_y) + TransformY;
                                addoney = 0;
                                if (iy + recwidth > iyold)
                                {
                                    addoney = -1;
                                }
                                else if (iy + recwidth < iyold)
                                {
                                    addoney = 1;
                                }
                                iyold = iy;
                                if ((ix >= 0) && (ix <= pb1_width) && (iy >= 0) && (iy <= pb1_height) && (contourcolor_name[i, j] > -1))
                                {
                                    //white color is drawn completely transparant
                                    int transparancy_new = transparancy_name;
                                    Color fillit = new Color();
                                    fillit = _drobj.FillColors[contourcolor_name[i, j]];
                                    if (fillit == Color.White)
                                    {
                                        transparancy_new = 0;
                                    }

                                    Brush br1 = new SolidBrush(Color2Transparent(transparancy_new, _drobj.FillColors[contourcolor_name[i, j]]));
                                    g.FillRectangle(br1, ix, iy - recwidth, recwidth + addonex, recwidth + addoney);
                                    br1.Dispose();
                                }
                            }
                            if (CancelDrawing)
                            {
                                break;
                            }
                        }
                    }
                }
                
                if (_drobj.LineWidth > 0) // draw contour lines
                {
                    //number of z-levels
                    List<Point>  contouredges = new List<Point>();
                    int label_interval = Math.Max(1, _drobj.LabelInterval);
                    //MessageBox.Show(label_distance.ToString() + "/" + _drobj.ContourLabelDist.ToString());
                    double labellenght = Convert.ToDouble(_drobj.ContourLabelDist) * factor_x;
                    int lenght_of_string = (int) g.MeasureString("9999", LabelFont).Width;
                    labellenght = Math.Max(lenght_of_string, labellenght * 2);
                    List<Color> _colLocal = _drobj.LineColors;
                    int _LineWidthLocal = _drobj.LineWidth;

                    for (int i = 0; i < _drobj.ContourPoints.Count; i++)
                    {
                        //number of points per z-level
                        int step = -2;
                        double labellenghtact = 0;
                        string label_value;
                        
                        Pen myPen;
                        if ((label_interval > 1) && (i % label_interval) == 0) // linewidth +1 if label-line
                        {
                            myPen = new Pen(Color2Transparent(255, _colLocal[i]), _LineWidthLocal + 1);
                        }
                        else
                        {
                            myPen = new Pen(Color2Transparent(255, _colLocal[i]), _LineWidthLocal);
                        }

                        Pen labelPen = new Pen(Color2Transparent(255, _colLocal[i]), Math.Min(2, _LineWidthLocal));
                        Brush myBrush = new SolidBrush(_drobj.LineColors[i]);
                        Matrix mat = new Matrix();
                        GraphicsPath gp = new GraphicsPath();
                        Region excludeRegion = new Region(gp);
                        PointF[] contourpoints_name_i = _drobj.ContourPoints[i].ToArray();
                        int step_max = _drobj.ContourPoints[i].Count - 3;
                        
                        double x1 = 0, x2 = 0, y1 = 0, y2 = 0;
                        while (step < step_max)
                        {
                            step += 2;

                            x1 = ((contourpoints_name_i[step].X - form1_west) * factor_x) + TransformX;
                            x2 = ((contourpoints_name_i[step + 1].X - form1_west) * factor_x) + TransformX;
                            y1 = ((contourpoints_name_i[step].Y - form1_north) * factor_y) + TransformY;
                            y2 = ((contourpoints_name_i[step + 1].Y - form1_north) * factor_y) + TransformY;

                            //show label_value
                            if ((_drobj.Label == 2 || _drobj.Label == 3) && (i % label_interval) == 0) // show labels and label interval = OK?
                            {
                                labellenghtact += Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
                                
                                if (labellenghtact > labellenght)
                                {
                                    //labelpoints.Add(new PointF((float)x1,(float)y1)); // to calculate the distance between label_value
                                    labellenghtact = 0;
                                    
                                    int x = Convert.ToInt32((x1 + x2) * 0.5); int y = Convert.ToInt32((y1 + y2) * 0.5);
                                    if (x > 0 && y > 0 && x < pb1_width && y < pb1_height)
                                    {
                                        label_value = Convert.ToString(Math.Round(_drobj.ItemValues[i], _drobj.DecimalPlaces));
                                        double angle = 90;
                                        if (x2 != x1)
                                        {
                                            angle = Math.Atan((y2 - y1) / (x2 - x1)) * 180 / 3.14;
                                        }

                                        SizeF stringSize = g.MeasureString(label_value, LabelFont);
                                        
                                        int sW = (int) (stringSize.Width * 0.5) + 2 ; int sH = (int) (stringSize.Height * 0.5) + 1;
                                        
                                        gp.AddRectangle(new Rectangle(x - sW , y- sH,  sW * 2, sH * 2 ));
                                        
                                        mat.Reset();
                                        mat.RotateAt((float) angle, new Point(x, y));

                                        gp.Transform(mat);
                                        g.Transform = mat;

                                        if (_drobj.LabelColor == Color.White)
                                        {
                                            g.DrawString(label_value, LabelFont, myBrush, x, y, StringFormat1);
                                        }
                                        else
                                        {
                                            g.DrawString(label_value, LabelFont, LabelBrush, x, y, StringFormat1);
                                        }
                                        
                                        g.ResetTransform();

#if __MonoCS__
#else
                                        excludeRegion.Union (gp);
                                        if (_drobj.Label == 3)
                                        {
                                            g.DrawPath(labelPen, gp); // draw rectangle around label
                                        }

                                        g.ExcludeClip(excludeRegion); // exclude label region from redraw
                                        gp.Reset();
                                        excludeRegion.MakeEmpty();
#endif
                                    }
                                }
                            } // show labels
                            
                            if (Math.Max(x1, x2) > 0 && Math.Max(y1, y2) > 0 && Math.Min(x1, x2) < pb1_width && Math.Min(y1, y2) < pb1_height)
                            {
                                contouredges.Add(new Point((int) x1, (int) y1));
                                contouredges.Add(new Point((int) x2, (int) y2));
                            }
                            //g.DrawLine(myPen, Convert.ToInt32(x1), Convert.ToInt32(y1), Convert.ToInt32(x2), Convert.ToInt32(y2));
                            //	}
                            if (CancelDrawing)
                            {
                                break;
                            }
                        } // while all points
                        
                        // draw the lines
                        for (int ed = 0; ed < contouredges.Count; ed += 2)
                        {
                            g.DrawLine(myPen, contouredges[ed].X, contouredges[ed].Y, contouredges[ed+1].X, contouredges[ed+1].Y);
                        }
                        
                        g.ResetClip(); // Reset Clipping area
                        excludeRegion.Dispose();
                        contouredges.Clear();
                        myPen.Dispose();
                        myBrush.Dispose();
                        gp.Dispose();
                        labelPen.Dispose();
                        if (CancelDrawing)
                        {
                            break;
                        }
                    }
                }
            }
            StringFormat1.Dispose();
            StringFormat2.Dispose();
        }
    }
}