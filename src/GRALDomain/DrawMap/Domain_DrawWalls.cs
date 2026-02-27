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

using GralData;
using GralItemData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw Walls
        /// </summary>
        private void DrawWalls(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north,
                               double factor_x, double factor_y, Font LabelFont, Brush LabelBrush)
        {
            int pb1_height = picturebox1.Height;
            int pb1_width = picturebox1.Width;

            //format for names of line sources
            StringFormat StringFormat2 = new StringFormat
            {
                LineAlignment = StringAlignment.Far,
                Alignment = StringAlignment.Center
            };

            Brush SelectedBrush = new SolidBrush(Color2Transparent(150, Color.Green));

            int n = -1;

            bool draw_label = (_drobj.Label == 2 || _drobj.Label == 3) && LabelFont.Height > 2;

            bool filter_l = _drobj.Filter;
            double x1 = 0;
            double y1 = 0;
            double x2 = 0;
            double y2 = 0;

            WallData _wd;

            for (int ii = 0; ii < EditWall.ItemData.Count; ii++)
            {
                _wd = EditWall.ItemData[ii];
                try
                {
                    n++;
                    double streetlength = 0;
                    if (_wd.Pt.Count > 0)
                    {
                        int width = Convert.ToInt32(Convert.ToDouble(MainForm.numericUpDown10.Value) / (BmpScale + 0.0000001) / MapSize.SizeX);
                        width = Math.Max(width, 2);
                        width = Math.Max(width, _drobj.LineWidth);

                        Pen mypen = new Pen(Color2Transparent(200, Color.Green), width);
                        List<PointD_3d> _pt = _wd.Pt;

                        x1 = (_pt[0].X - form1_west) * factor_x + TransformX;
                        y1 = (_pt[0].Y - form1_north) * factor_y + TransformY;

                        int point_counter = 0;
                        Point[] linePoints = new Point[_pt.Count + 1];
                        linePoints[point_counter] = new Point((int)x1, (int)y1); // start point

                        for (int i = 1; i < _pt.Count; i++)
                        {
                            x2 = (_pt[i].X - form1_west) * factor_x + TransformX; // end point
                            y2 = (_pt[i].Y - form1_north) * factor_y + TransformY;

                            if (filter_l == true &&
                                (Math.Abs(x1 - x2) < 5 && Math.Abs(y1 - y2) < 5) &&
                                i < (_pt.Count - 1))
                            {
                                // do nothing
                            }
                            else // not filtered -> draw point
                            {
                                if ((n == EditWall.ItemDisplayNr) && ((MouseControl == MouseMode.WallSel) || (MouseControl == MouseMode.WallSet)))
                                {
                                    Brush edges = new SolidBrush(Color.Green);
                                    g.DrawLine(mypen, (int)x1, (int)y1, (int)x2, (int)y2);
                                    g.FillRectangle(edges, new RectangleF((float)(x1 - 6), (float)(y1 - 6), 12, 12));
                                    if (i == _pt.Count - 1)
                                    {
                                        g.FillRectangle(edges, new RectangleF((float)(x2 - 6), (float)(y2 - 6), 12, 12));
                                    }
                                    edges.Dispose();
                                }
                                else
                                {
                                    point_counter++;
                                    linePoints[point_counter] = new Point((int)x2, (int)y2); // 
                                }

                                if (draw_label)
                                {
                                    streetlength += Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

                                    if (streetlength > Convert.ToDouble(_drobj.ContourLabelDist) * factor_x)
                                    {
                                        streetlength = 0;
                                        Matrix m = new Matrix();
                                        float angle = 90;
                                        if (x2 != x1)
                                        {
                                            angle = (float)(Math.Atan((y2 - y1) / (x2 - x1)) * 180 / 3.14);
                                        }

                                        m.RotateAt(angle, new Point((int)((x1 + x2) * 0.5), (int)((y1 + y2) * 0.5)));
                                        g.Transform = m;

                                        g.DrawString(_drobj.Name, LabelFont, LabelBrush, (int)((x1 + x2) * 0.5), (int)((y1 + y2) * 0.5), StringFormat2);

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
                                Pen pen1 = new Pen(Color2Transparent(_drobj.Transparancy, _drobj.FillColors[0]), width);
                                g.DrawLines(pen1, linePoints);
                                pen1.Dispose();
                            }
                            Pen pen2 = new Pen(Color2Transparent(_drobj.Transparancy, _drobj.LineColors[0]));
                            g.DrawLines(pen2, linePoints);
                            pen2.Dispose();
                        }

                        linePoints = null;

                    }
                    if (CancelDrawing)
                    {
                        break;
                    }
                }
                catch { }
            } // foreach

            SelectedBrush.Dispose();
            StringFormat2.Dispose();
        }
    }
}