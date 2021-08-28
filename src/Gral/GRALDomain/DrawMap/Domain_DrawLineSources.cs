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
 * User: Markus Kuntner
 * Date: 16.01.2019
 * Time: 11:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using GralItemData;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw Line Sources
        /// </summary>
        private void DrawLineSources(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north,
                                     double factor_x, double factor_y, Font LabelFont, Brush LabelBrush)
        {
            int pb1_height = picturebox1.Height;
            int pb1_width  = picturebox1.Width;

            StringFormat StringFormat2 = new StringFormat
            {
                LineAlignment = StringAlignment.Far,
                Alignment = StringAlignment.Center
            }; //format for names of line sources

            Brush SelectedBrush = new SolidBrush(Color2Transparent(150, Color.Green));
            
            int n = -1;
            bool draw_label = (_drobj.Label == 2 || _drobj.Label == 3) && LabelFont.Height > 2;
            bool filter_l = _drobj.Filter;
            
            LineSourceData _ls;
            PollutantsData _poll;
            
            for (int ii = 0; ii < EditLS.ItemData.Count; ii++)
            {
                _ls = EditLS.ItemData[ii];
                n++;
                try
                {
                    //int sourcegroups = _ls.Poll.Count(); // number of stored pollutants
                    double x1 = 0;
                    double y1 = 0;
                    double x2 = 0;
                    double y2 = 0;
                    double streetlength = 0;

                    int index = -2;
                    if ((_drobj.Item == -1) && (_drobj.SourceGroup == -1))
                    {
                        index = 0;
                    }
                    else
                    {
                        //filter source group
                        double emi = 0;
                        for (int jj = 0; jj <_ls.Poll.Count; jj++)
                        {
                            _poll = _ls.Poll[jj];
                            
                            if (_drobj.SourceGroup == _poll.SourceGroup)
                            {
                                if (_drobj.Item == -1)
                                {
                                    index = 0;
                                }
                                else
                                {
                                    //filter pollutant
                                    for (int j = 0; j < 10; j++)
                                    {
                                        if (_poll.Pollutant[j] == _drobj.Item)
                                        {
                                            for (int r = _drobj.LineColors.Count - 1; r > -1; r--)
                                            {
                                                if (_poll.EmissionRate[j] >= _drobj.ItemValues[r])
                                                {
                                                    index = Math.Max(r, index);
                                                    break;
                                                }
                                            }
                                            //break;
                                        }
                                    }
                                }
                                break;
                            }
                            else if (_drobj.SourceGroup == -1)
                            {
                                //filter pollutant and sum up over all source groups
                                for (int j = 0; j < 10; j++)
                                {
                                    if (_poll.Pollutant[j] == _drobj.Item)
                                    {
                                        emi += _poll.EmissionRate[j];
                                        break;
                                    }
                                }
                            }
                            if (_drobj.SourceGroup == -1)
                            {
                                for (int r = _drobj.LineColors.Count - 1; r > -1; r--)
                                {
                                    if (emi >= _drobj.ItemValues[r])
                                    {
                                        index = Math.Max(r, index);
                                        break;
                                    }
                                }
                            }
                        }
                        //filter other items
                        if (_drobj.Item == 100)
                        {
                            for (int r = _drobj.LineColors.Count - 1; r > -1; r--)
                            {
                                if ( _ls.Nemo.AvDailyTraffic >= _drobj.ItemValues[r])
                                {
                                    index = Math.Max(r, index);
                                    break;
                                }
                            }
                        }
                        if (_drobj.Item == 200)
                        {
                            for (int r = _drobj.LineColors.Count - 1; r > -1; r--)
                            {
                                if (_ls.Nemo.Slope >= _drobj.ItemValues[r])
                                {
                                    index = Math.Max(r, index);
                                    break;
                                }
                            }
                        }
                        if (_drobj.Item == 300)
                        {
                            for (int r = _drobj.LineColors.Count - 1; r > -1; r--)
                            {
                                if (_ls.Nemo.ShareHDV >= _drobj.ItemValues[r])
                                {
                                    index = Math.Max(r, index);
                                    break;
                                }
                            }
                        }
                    } // no filter of pollutants

                    int width = Convert.ToInt32(_ls.Width / (BmpScale + 0.0000001) / MapSize.SizeX);
                    width = Math.Max(width, 2);
                    width = Math.Max(width, _drobj.LineWidth);
                    Pen mypen = new Pen(Color2Transparent(200, Color.Green), width);
                    
                    List <GralData.PointD_3d> _pts = _ls.Pt;
                    
                    x1 = (_pts[0].X - form1_west) * factor_x + TransformX;
                    y1 = (_pts[0].Y - form1_north) * factor_y + TransformY;
                    
                    int lastpoint = _pts.Count;
                    int point_counter = 0;
                    //List<Point> linePoints = new List<Point>();
                    Point[] linePoints = new Point[lastpoint + 1];
                    linePoints[point_counter] = new Point((int) x1, (int) y1); // start point
                   
                    for (int i = 0; i < lastpoint; i++)
                    {
                        x2 = (_pts[i].X - form1_west) * factor_x + TransformX; // end point
                        y2 = (_pts[i].Y - form1_north) * factor_y + TransformY;
                        
                        if (filter_l == true &&
                            (Math.Abs(x1 - x2) < 5 && Math.Abs(y1 - y2) < 5) &&
                            i < lastpoint)
                        {
                            // do nothing
                        }
                        else // not filtered -> draw point
                        {
                            if ((n == EditLS.ItemDisplayNr) && ((MouseControl == MouseMode.LineSourceSel) || (MouseControl == MouseMode.LineSourcePos)))
                            {
                                Brush edges = new SolidBrush(Color.Green);
                                g.DrawLine(mypen, (int) x1, (int) y1, (int) x2, (int) y2);
                                g.FillRectangle(edges, new RectangleF((float) (x1 -6), (float) (y1 - 6), 12, 12));
                                if (i == lastpoint - 1)
                                {
                                    g.FillRectangle(edges, new RectangleF((float) (x2 -6), (float) (y2 - 6), 12, 12));
                                }
                                edges.Dispose();
                            }
                            else
                            {
                                if (index != -2)
                                {
                                    point_counter++;
                                    linePoints[point_counter] = new Point((int) x2, (int) y2); // 
                                }
                            }
                            
                            if (draw_label)
                            {
                                streetlength += Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
                                
                                if (streetlength > _drobj.ContourLabelDist * factor_x)
                                {
                                    streetlength = 0;
                                    Matrix m = new Matrix();
                                    float angle = 90;
                                    if (x2 != x1)
                                    {
                                        angle = (float)(Math.Atan((y2 - y1) / (x2 - x1)) * 180 / 3.14);
                                    }

                                    m.RotateAt(angle, new Point((int) ((x1 + x2) * 0.5), (int) ((y1 + y2) * 0.5)));
                                    g.Transform = m;
                                    g.DrawString(_ls.Name, LabelFont, LabelBrush, (int) ((x1 + x2) * 0.5), (int) ((y1 + y2) * 0.5), StringFormat2);
                                    g.ResetTransform();
                                }
                            }
                            
                            x1 = x2; y1 = y2; // new start point = end point
                        }
                    }
                   
                    if (point_counter > 0)
                    {
                        point_counter++;
                        if (point_counter < linePoints.Length)
                        {
                            Array.Resize(ref linePoints, point_counter);
                        }
                        
                        if (_drobj.FillYesNo == true)
                        {
                            Pen pen1 = new Pen(Color2Transparent(_drobj.Transparancy, _drobj.FillColors[index]), width);
                            g.DrawLines(pen1, linePoints);
                            pen1.Dispose();
                        }
                        Pen pen2 = new Pen(Color2Transparent(_drobj.Transparancy, _drobj.LineColors[index]));
                        g.DrawLines(pen2,linePoints);
                        pen2.Dispose();
                    }
                    
                    linePoints = null;
                    
                    if (CancelDrawing)
                    {
                        break;
                    }
                }
                catch{}
            }

            SelectedBrush.Dispose();
            StringFormat2.Dispose();
        }
    }
}