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
using GralItemData;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw Buildings
        /// </summary>
#if NET7_0_OR_GREATER
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
#endif
        private void DrawBuildings(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north,
                                     double factor_x, double factor_y, Font LabelFont, Brush LabelBrush)
        {
            int pb1_height = picturebox1.Height;
            int pb1_width  = picturebox1.Width;

            //format for names of sources
            StringFormat StringFormat1 = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            }; 

            Brush SelectedBrush = new SolidBrush(Color.Green);
            int n = -1;
            
            bool draw_label = (_drobj.Label == 2 || _drobj.Label == 3) && LabelFont.Height > 2;
            int transparency = _drobj.Transparancy;
            int linewidth_l = _drobj.LineWidth;
            bool FillYesNo = _drobj.FillYesNo;
            bool filter_l = _drobj.Filter;
            
            BuildingData _bd;
            
            for (int ii = 0; ii < EditB.ItemData.Count; ii++)
            {
                try
                {
                    _bd = EditB.ItemData[ii];
                    n++;
                    List <PointD> _pt = _bd.Pt;
                    int vertices = _pt.Count;
                    int point_counter = 0;

                    if (vertices > 2)
                    {
                        Point[] myPoints = new Point[vertices];
                        int xmean = 0, xold = 0;
                        int ymean = 0, yold = 0;
                        bool dismiss = true;

                        for (int i = 0; i < _pt.Count; i++)
                        {
                            int x1 = (int)((_pt[i].X - form1_west) * factor_x + TransformX);
                            int y1 = (int)((_pt[i].Y - form1_north) * factor_y + TransformY);
                            
                            if (dismiss && (x1 > 0) && (x1 < pb1_width) && (y1 > 0) && (y1 < pb1_height))
                            {
                                dismiss = false;
                            }

                            xmean +=  x1;
                            ymean +=  y1;

                            if (filter_l == false || i == 0 || i == (vertices - 1) || Math.Abs(x1 - xold) > 2 || Math.Abs(y1 - yold) > 2)
                            {
                                myPoints[point_counter] = new Point(x1, y1);
                                point_counter++;
                                xold = x1; yold = y1;
                            }

                            // draw building if last point is reached and building inside the screen 
                            if ((i == vertices - 1) && (dismiss == false))
                            {
                                //apply color scale
                                double height = Math.Abs(_bd.Height);
                                int index = 0;
                                for (int r = _drobj.FillColors.Count - 1; r > -1; r--)
                                {
                                    if (height >= _drobj.ItemValues[r])
                                    {
                                        index = Math.Min(r, Math.Min(_drobj.FillColors.Count, _drobj.LineColors.Count));
                                        break;
                                    }
                                }

                                if (point_counter < myPoints.Length)
                                {
                                    Array.Resize(ref myPoints, point_counter);
                                }
                                
                                if ((n == EditB.ItemDisplayNr) && ((MouseControl == MouseMode.BuildingSel) || (MouseControl == MouseMode.BuildingPos)))
                                {
                                    g.FillPolygon(SelectedBrush, myPoints);
                                    Brush edges = new SolidBrush(Color.Green);
                                    for (int ep = 0; ep < myPoints.Length; ep++)
                                    {
                                        g.FillRectangle(edges, new RectangleF(myPoints[ep].X - 6, myPoints[ep].Y - 6, 12, 12));
                                    }
                                    edges.Dispose();
                                }
                                else
                                {
                                    if (FillYesNo == true)
                                    {
                                        Brush br1 = new SolidBrush(Color2Transparent(transparency, _drobj.FillColors[index]));
                                        g.FillPolygon(br1, myPoints);
                                        br1.Dispose();
                                    }

                                    if (linewidth_l > 0)
                                    {
                                        Pen pen1 = new Pen(Color2Transparent(transparency, _drobj.LineColors[index]), linewidth_l);
                                        g.DrawPolygon(pen1, myPoints);
                                        pen1.Dispose();
                                    }
                                }
                                myPoints = null;
                            }
                        } // for all vertices

                        if ((draw_label) && (dismiss == false))
                        {
                            if (vertices > 1)
                            {
                                xmean /= vertices;
                                ymean /= vertices;
                            }
                            g.DrawString(_bd.Name, LabelFont, LabelBrush, xmean, ymean, StringFormat1);
                        }
                    }
                    if (CancelDrawing)
                    {
                        break;
                    }
                }
                catch{  	}
            }
            
            SelectedBrush.Dispose();
            StringFormat1.Dispose();
        }
    }
}
