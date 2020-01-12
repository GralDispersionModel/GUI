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
 * Time: 11:32
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using GralItemData;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw Portal Sources
        /// </summary>
        private void DrawPortalSources(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north,
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
            
            bool draw_label = (_drobj.Label == 2 || _drobj.Label == 3) && LabelFont.Height > 2;
            
            PortalsData _pd;
            Point[] mypoint = new Point[7];
            
            for (int ii = 0; ii < EditPortals.ItemData.Count; ii++)
            {
                _pd = EditPortals.ItemData[ii];
                try
                {
                    n++;
                    
                    int sourcegroups = _pd.Poll.Count;
                    if (_pd.Height > 0)
                    {
                        double x1 = (_pd.Pt1.X - form1_west) * factor_x + TransformX;
                        double y1 = (_pd.Pt1.Y - form1_north) * factor_y + TransformY;
                        double x2 = (_pd.Pt2.X - form1_west) * factor_x + TransformX;
                        double y2 = (_pd.Pt2.Y - form1_north) * factor_y + TransformY;
                        if ((x1 < 0) || (y1 < 0) || (x1 > pb1_width) || (y1 > pb1_height))
                        {
                        }
                        if ((x2 < 0) || (y2 < 0) || (x2 > pb1_width) || (y2 > pb1_height))
                        {
                        }
                        int xmean = Convert.ToInt32((x1 + x2) * 0.5);
                        int ymean = Convert.ToInt32((y1 + y2) * 0.5);

                        double length = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

                        g.DrawLine(new Pen(Color2Transparent(_drobj.Transparancy, _drobj.LineColors[0]), 2), Convert.ToInt32(x1), Convert.ToInt32(y1), Convert.ToInt32(x2), Convert.ToInt32(y2));

                        if ((n == EditPortals.ItemDisplayNr) && ((MouseControl == 15) || (MouseControl == 16)))
                        {
                            g.DrawLine(new Pen(Color2Transparent(200, Color.Green), 2), Convert.ToInt32(x1), Convert.ToInt32(y1), Convert.ToInt32(x2), Convert.ToInt32(y2));
                        }

                        Matrix m = new Matrix();
                        float angle = 0;
                        if (x2 != x1)
                        {
                            angle = (float)(Math.Atan((y2 - y1) / (x2 - x1)) * 180 / 3.14);
                            if ((x1 < x2) && (y1 < y2))
                                angle = 90 + angle;
                            if ((x1 > x2) && (y1 < y2))
                                angle = 270 + angle;
                            if ((x1 < x2) && (y1 > y2))
                                angle = 90 + angle;
                            if ((x1 > x2) && (y1 > y2))
                                angle = angle - 90;
                        }
                        else
                            if (y1 < y2)
                                angle = 180;
                        if (angle == 0)
                            if (x1 < x2)
                                angle = 90;
                        else if (x1 > x2)
                            angle = -90;
                        m.RotateAt(angle, new Point(Convert.ToInt32((x1 + x2) * 0.5), Convert.ToInt32((y1 + y2) * 0.5)));
                        g.Transform = m;
                        //g.DrawImage(portal, xmean - 15, ymean - 15, 30, 30);

                        if ((n == EditPortals.ItemDisplayNr) && ((MouseControl == 15) || (MouseControl == 16)))
                        {
                            //g.DrawRectangle(p1, xmean - 15, ymean - 15, 30, 30);
                        }

                        //compute vector for indicating outlet direction
                        
                        double xrot;
                        double yrot;
                        //x,y coordinates of the cell center
                        //length = 50;
                        
                        if (_drobj.LineWidth <= 1)
                            if (_drobj.LineWidth == 0)
                                length = 50;
                            else
                                length = Math.Max (1, Math.Min(200, Convert.ToInt32(14 / BmpScale / MapSize.SizeX))); // 14 m lenght
                            else
                                length = Math.Max (1, Math.Min(200, Convert.ToInt32((double) _drobj.LineWidth / BmpScale / MapSize.SizeX)));
                        
                        //point 1 of arrow
                        x1 = xmean - length * 0.5;
                        y1 = ymean - length * 0.1;
                        //rotation
                        xrot = xmean + (x1 - xmean);
                        yrot = ymean + (y1 - ymean);
                        mypoint[0] = new Point(Convert.ToInt32(xrot), Convert.ToInt32(yrot));
                        //point 2 of arrow
                        x1 = xmean - length * 0.5;
                        y1 = ymean + length * 0.1;
                        //rotation
                        xrot = xmean + (x1 - xmean);
                        yrot = ymean + (y1 - ymean);
                        mypoint[1] = new Point(Convert.ToInt32(xrot), Convert.ToInt32(yrot));
                        //point 3 of arrow
                        x1 = xmean + length * 0.05;
                        y1 = ymean + length * 0.1;
                        //rotation
                        xrot = xmean + (x1 - xmean);
                        yrot = ymean + (y1 - ymean);
                        mypoint[2] = new Point(Convert.ToInt32(xrot), Convert.ToInt32(yrot));
                        //point 4 of arrow
                        x1 = xmean + length * 0.05;
                        y1 = ymean + length * 0.35;
                        //rotation
                        xrot = xmean + (x1 - xmean);
                        yrot = ymean + (y1 - ymean);
                        mypoint[3] = new Point(Convert.ToInt32(xrot), Convert.ToInt32(yrot));
                        //point 5 of arrow
                        x1 = xmean + length * 0.5;
                        y1 = ymean;
                        //rotation
                        xrot = xmean + (x1 - xmean);
                        yrot = ymean + (y1 - ymean);
                        mypoint[4] = new Point(Convert.ToInt32(xrot), Convert.ToInt32(yrot));
                        //point 6 of arrow
                        x1 = xmean + length * 0.05;
                        y1 = ymean - length * 0.35;
                        //rotation
                        xrot = xmean + (x1 - xmean);
                        yrot = ymean + (y1 - ymean);
                        mypoint[5] = new Point(Convert.ToInt32(xrot), Convert.ToInt32(yrot));
                        //point 7 of arrow
                        x1 = xmean + length * 0.05;
                        y1 = ymean - length * 0.1;
                        //rotation
                        xrot = xmean + (x1 - xmean);
                        yrot = ymean + (y1 - ymean);
                        mypoint[6] = new Point(Convert.ToInt32(xrot), Convert.ToInt32(yrot));
                        
                        Brush br1 = new SolidBrush(Color2Transparent(_drobj.Transparancy, _drobj.LineColors[0]));
                        g.FillPolygon(br1, mypoint);
                        br1.Dispose();
                        
                        if ((n == EditPortals.ItemDisplayNr) && ((MouseControl == 15) || (MouseControl == 16)))
                        {
                            g.FillPolygon(SelectedBrush, mypoint);
                        }

                        g.ResetTransform();

                        if (draw_label)
                        {
                            g.DrawString(_pd.Name, LabelFont, LabelBrush, xmean, ymean - 20, StringFormat1);
                        }
                    }
                }
                catch{}
            }

            StringFormat1.Dispose();
        }
    }
}
