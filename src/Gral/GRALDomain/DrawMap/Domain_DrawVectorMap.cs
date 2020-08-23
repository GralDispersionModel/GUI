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
 * Time: 10:55
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw the Vector Maps
        /// </summary>
        private void DrawVectorMap(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north, double factor_x, double factor_y)
        {
            int pb1_height = picturebox1.Height;
			int pb1_width  = picturebox1.Width;
			
            List<PointF> _pts = _drobj.ContourPoints[0];
            int step = -7;
            int frequ_x = 0;
            //int frequ_y = 0;
            
            Point[] mypoint = new Point[7];
            int indexx = -1;
            int indexy = _drobj.ContourGeometry.NY - 1;
            int frequency = _drobj.ContourLabelDist;

            double west = MapSize.West;
            double north = MapSize.North;
            
            double gridsize = MainForm.GRAMMHorGridSize * factor_x;
            
            if (gridsize > 5 && gridsize < 20 && _drobj.Filter == true) // smart draw for vectors
            {
                int MaxXIndex = _drobj.ContourGeometry.NX;
                while (step < _pts.Count - 8)
                {
                    frequ_x ++;
                    step += 7;
                    indexx ++;
                    if (indexx >= MaxXIndex) // nx reached
                    {
                        indexx = 0;
                        indexy --;
                    }
                    
                    if (frequency <= frequ_x && (indexy % frequency) == 0)  // horizontal vector spacing
                    {
                        frequ_x = 0;
                        int x1 = (int)((_pts[step].X - west) * factor_x) + TransformX;
                        int y1 = (int)((_pts[step].Y - north) * factor_y) + TransformY;
                        int x7 = (int)((_pts[step + 6].X - west) * factor_x) + TransformX;
                        int y7 = (int)((_pts[step + 6].Y - north) * factor_y) + TransformY;
                        
                        if (((x1 < 0) || (x1 > pb1_width)) && ((y1 < 0) || (y1 > pb1_height)) && ((x7 < 0) || (x7 > pb1_width)) && ((y7 < 0) || (y7 > pb1_height)))
                        {
                        }
                        else
                        {
                            if (_drobj.ContourColor[indexx, indexy] > -1 && (x1 != x7 && y1 != y7))
                            {
                                Pen mypen = new Pen(Color2Transparent(255, _drobj.FillColors[_drobj.ContourColor[indexx, indexy]]))
                                {
                                    Width = 1,
                                    EndCap = LineCap.ArrowAnchor
                                };
                                g.DrawLine(mypen, x1, y1, x7, y7);
                                mypen.Dispose();
                            }
                        }
                    }
                    if (CancelDrawing)
                    {
                        break;
                    }
                }
            }
            
            else if (gridsize >= 20 || _drobj.Filter == false) // don´t show small grids
            {
                int MaxXIndex = _drobj.ContourGeometry.NX;
                while (step < _pts.Count - 8)
                {
                    frequ_x ++;
                    step += 7;
                    indexx ++;
                    if (indexx >= MaxXIndex)
                    {
                        indexx = 0;
                        indexy --;
                    }
                    
                    if (frequency <= frequ_x && (indexy % frequency) == 0) // horizontal vector spacing
                    {
                        frequ_x = 0;
                        int x1 = (int)((_pts[step].X - west) * factor_x) + TransformX;
                        int y1 = (int)((_pts[step].Y - north) * factor_y) + TransformY;
                        int x2 = (int)((_pts[step + 1].X - west) * factor_x) + TransformX;
                        int y2 = (int)((_pts[step + 1].Y - north) * factor_y) + TransformY;
                        int x3 = (int)((_pts[step + 2].X - west) * factor_x) + TransformX;
                        int y3 = (int)((_pts[step + 2].Y - north) * factor_y) + TransformY;
                        int x4 = (int)((_pts[step + 3].X - west) * factor_x) + TransformX;
                        int y4 = (int)((_pts[step + 3].Y - north) * factor_y) + TransformY;
                        int x5 = (int)((_pts[step + 4].X - west) * factor_x) + TransformX;
                        int y5 = (int)((_pts[step + 4].Y - north) * factor_y) + TransformY;
                        int x6 = (int)((_pts[step + 5].X - west) * factor_x) + TransformX;
                        int y6 = (int)((_pts[step + 5].Y - north) * factor_y) + TransformY;
                        int x7 = (int)((_pts[step + 6].X - west) * factor_x) + TransformX;
                        int y7 = (int)((_pts[step + 6].Y - north) * factor_y) + TransformY;
                        mypoint[0] = new Point(x1, y1);
                        mypoint[1] = new Point(x2, y2);
                        mypoint[2] = new Point(x3, y3);
                        mypoint[3] = new Point(x4, y4);
                        mypoint[4] = new Point(x5, y5);
                        mypoint[5] = new Point(x6, y6);
                        mypoint[6] = new Point(x7, y7);
                        
                        if (((x1 < 0) || (x1 > pb1_width)) && ((y1 < 0) || (y1 > pb1_height)) && ((x7 < 0) || (x7 > pb1_width)) && ((y7 < 0) || (y7 > pb1_height)))
                        {
                        }
                        else
                        {
                            if (_drobj.ContourColor[indexx, indexy] > -1)
                            {
                                if (_drobj.FillYesNo == true)
                                {
                                    Brush br1 = new SolidBrush(Color2Transparent(_drobj.Transparancy, _drobj.FillColors[_drobj.ContourColor[indexx, indexy]]));
                                    g.FillPolygon(br1, mypoint);
                                    br1.Dispose();
                                }
                                if (_drobj.LineWidth > 0)
                                {
                                    Pen pen1 = new Pen(Color2Transparent(_drobj.Transparancy, _drobj.LineColors[_drobj.ContourColor[indexx, indexy]]), _drobj.LineWidth);
                                    g.DrawPolygon(pen1, mypoint);
                                    pen1.Dispose();
                                }
                            }
                        }
                    }
                    if (CancelDrawing)
                    {
                        break;
                    }
                }
            }
        }
    }
}