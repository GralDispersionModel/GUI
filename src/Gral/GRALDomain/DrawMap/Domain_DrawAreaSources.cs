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
 * Time: 11:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using GralItemData;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw Area Sources
        /// </summary>
        private void DrawAreaSources(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north,
                                     double factor_x, double factor_y, Font LabelFont, Brush LabelBrush) 
        {
            int pb1_height = picturebox1.Height;
			int pb1_width  = picturebox1.Width;

            StringFormat StringFormat1 = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            }; //format for names of sources

            Brush SelectedBrush = new SolidBrush(Color2Transparent(150, Color.Green));
			
            int n = -1;
            int transparency = _drobj.Transparancy;
            int linewidth_l = _drobj.LineWidth;
            bool FillYesNo = _drobj.FillYesNo;
            
            bool draw_label = (_drobj.Label == 2 || _drobj.Label == 3) && LabelFont.Height > 2;
            
            AreaSourceData _as;
            
            for (int ii = 0; ii < EditAS.ItemData.Count; ii++)
            {
                _as = EditAS.ItemData[ii];
                try
                {
                    n++;
                    List <PointD> _pt = _as.Pt;
                    int vertices = _pt.Count;
                    
                    if (vertices > 2)
                    {
                        Point[] myPoints = new Point[vertices];
                        int xmean = 0;
                        int ymean = 0;
                        int index = -2;
                        if ((_drobj.Item == -1) && (_drobj.SourceGroup == -1))
                        {
                            index = 0;
                        }

                        bool dismiss = true;

                        for (int i = 0; i < vertices; i++)
                        {
                            int x1 = Convert.ToInt32((_pt[i].X - form1_west) * factor_x) + TransformX;
                            int y1 = Convert.ToInt32((_pt[i].Y - form1_north) * factor_y) + TransformY;
                            myPoints[i] = new Point(x1, y1);
                            xmean += x1;
                            ymean += y1;
                            
                            if (dismiss && (x1 > 0) && (x1 < pb1_width) && (y1 > 0) && (y1 < pb1_height))
                            {
                                dismiss = false;
                            }

                            // at last point
                            if ((i == vertices - 1) && (dismiss == false))
                            {
                                if ((n == EditAS.ItemDisplayNr) && ((MouseControl == 9) || (MouseControl == 8)))
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
                                    //filter source group
                                    if ((_drobj.SourceGroup == _as.Poll.SourceGroup) || (_drobj.SourceGroup == -1))
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
                                                if (_as.Poll.Pollutant[j] == _drobj.Item)
                                                {
                                                    for (int r = _drobj.FillColors.Count - 1; r > -1; r--)
                                                    {
                                                        if (_as.Poll.EmissionRate[j] >= (_drobj.ItemValues[r] - 5* float.Epsilon))
                                                        {
                                                            index = r;
                                                            break;
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (index != -2)
                                    {
                                        if (FillYesNo == true)
                                        {
                                            Brush br1 = new SolidBrush(Color2Transparent(transparency, _drobj.FillColors[index]));
                                            g.FillPolygon(br1, myPoints);
                                            br1.Dispose();
                                        }
                                    }
                                    if (linewidth_l > 0)
                                    {
                                        Pen pen1 = new Pen(Color2Transparent(transparency, _drobj.LineColors[0]), linewidth_l);
                                        g.DrawPolygon(pen1, myPoints);
                                        pen1.Dispose();
                                    }
                                }
                            }
                        }

                        if ((draw_label) && (dismiss == false))
                        {
                            if (vertices > 1)
                            {
                                xmean = xmean / vertices;
                                ymean = ymean / vertices;
                            }
                            g.DrawString( _as.Name, LabelFont, LabelBrush, xmean, ymean, StringFormat1);
                        }
                    }
                }
                catch
                {}
            } // foreach
            
            SelectedBrush.Dispose();
            StringFormat1.Dispose();
        }
    }
}
