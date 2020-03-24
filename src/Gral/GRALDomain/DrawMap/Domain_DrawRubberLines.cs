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
 * User: U0178969
 * Date: 24.01.2019
 * Time: 14:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw rubber circles and crosses for copied or moved point sources (mode = 0) and receptors (mode = 1) 
        /// </summary>
        private void DrawRubberPS(int x, int y, int mode)
        {
            
            if (PictureBoxBitmap != null && RubberRedrawAllowed == 0)
            {
                RubberRedrawAllowed = 2;
                try // if picture_box is used in an other thread
                {
                    if (picturebox1.Image != null)
                        picturebox1.Image.Dispose();
                    // restore old image
                    picturebox1.Image = PictureBoxBitmap.Clone(new Rectangle(0, 0, PictureBoxBitmap.Width, PictureBoxBitmap.Height), PictureBoxBitmap.PixelFormat);
                    
                    using (Graphics g = Graphics.FromImage(picturebox1.Image))
                    {
                        Pen pen = new Pen(Color.Black);
                        Pen pen2 = new Pen(Color.White)
                        {
                            DashPattern = new float[] { 5, 5 }
                        };

                        if (mode == 0) // point source
                        {
                            g.DrawArc(pen, x- 10,y-10,20,20,1,360);
                            g.DrawArc(pen2, x- 10,y-10,20,20,1,360);
                        }
                        else if (mode == 1) // Receptor
                        {
                            g.DrawLine(pen, x - 20, y, x + 20, y);
                            g.DrawLine(pen, x, y - 20, x, y + 20);
                            g.DrawLine(pen2, x - 20, y, x + 20, y);
                            g.DrawLine(pen2, x, y - 20, x, y + 20);
                        }
                        pen.Dispose();
                        pen2.Dispose();
                    }
                }
                catch
                {}
                RubberRedrawAllowed = 0;
            }
        }
        
        /// <summary>
        /// Draw rubber lines for copied buildings
        /// </summary>
        private void DrawRubberBuilding(int mode)
        {
            if (mode == 1 && CopiedItem.Building == null)
            {
                return;
            }
            if (mode == 1 && CopiedItem.Building.Pt.Count < 2)
            {
                return;
            }
            
            if (mode == 2 && CopiedItem.AreaSource == null)
            {
                return;
            }
            if (mode == 2 && CopiedItem.AreaSource.Pt.Count < 2)
            {
                return;
            }
            
            if (PictureBoxBitmap != null && RubberRedrawAllowed == 0)
            {
                RubberRedrawAllowed = 2;
                double form1_west = MapSize.West, form1_north = MapSize.North;
                double factor_x = 1 / BmpScale / MapSize.SizeX;
                double factor_y = 1 / BmpScale / MapSize.SizeY;
                
                try // if picture_box is used in an other thread
                {
                    if (picturebox1.Image != null)
                        picturebox1.Image.Dispose();
                    // restore old image
                    picturebox1.Image = PictureBoxBitmap.Clone(new Rectangle(0, 0, PictureBoxBitmap.Width, PictureBoxBitmap.Height), PictureBoxBitmap.PixelFormat);
                    
                    using (Graphics g = Graphics.FromImage(picturebox1.Image))
                    {
                        Pen pen = new Pen(Color.Black);
                        Pen pen2 = new Pen(Color.White)
                        {
                            DashPattern = new float[] { 5, 5 }
                        };
                        List<PointD> PtList = new List<PointD>();
                        double x = Convert.ToDouble(textBox1.Text);
                        double y = Convert.ToDouble(textBox2.Text);
                        double x0;
                        double y0;
                        
                        Point[] DrawPoint;
                        if (mode == 1)
                        {
                            DrawPoint = new Point[CopiedItem.Building.Pt.Count];
                            PtList = CopiedItem.Building.Pt;
                            x0 = x - CopiedItem.Building.Pt[0].X;
                            y0 = y - CopiedItem.Building.Pt[0].Y;
                        }
                        else
                        {
                            DrawPoint = new Point[CopiedItem.AreaSource.Pt.Count];
                            PtList = CopiedItem.AreaSource.Pt;
                            x0 = x - CopiedItem.AreaSource.Pt[0].X;
                            y0 = y - CopiedItem.AreaSource.Pt[0].Y;
                        }
                        
                        int i = 0;
                        foreach(PointD _pt in PtList)
                        {
                            int x1 = (int)((x0 + _pt.X - form1_west) * factor_x + TransformX);
                            int y1 = (int)((y0 + _pt.Y - form1_north) * factor_y + TransformY);
                            DrawPoint[i] = new Point(x1, y1);
                            i++;
                        }
                        g.DrawPolygon(pen, DrawPoint);
                        g.DrawPolygon(pen2, DrawPoint);
                        
                        pen.Dispose();
                        pen2.Dispose();
                    }
                }
                catch
                {}
                RubberRedrawAllowed = 0;
            }
        }
        
        /// <summary>
        /// Draw rubber lines for copied line sources
        /// </summary>
        private void DrawRubberLineSource()
        {
            if (CopiedItem.LineSource == null)
            {
                return;
            }
            if (CopiedItem.LineSource.Pt.Count < 2)
            {
                return;
            }
            
            if (PictureBoxBitmap != null && RubberRedrawAllowed == 0)
            {
                RubberRedrawAllowed = 2;
                double form1_west = MapSize.West, form1_north = MapSize.North;
                double factor_x = 1 / BmpScale / MapSize.SizeX;
                double factor_y = 1 / BmpScale / MapSize.SizeY;
                
                try // if picture_box is used in an other thread
                {
                    if (picturebox1.Image != null)
                        picturebox1.Image.Dispose();
                    // restore old image
                    picturebox1.Image = PictureBoxBitmap.Clone(new Rectangle(0, 0, PictureBoxBitmap.Width, PictureBoxBitmap.Height), PictureBoxBitmap.PixelFormat);
                    
                    using (Graphics g = Graphics.FromImage(picturebox1.Image))
                    {
                        Pen pen = new Pen(Color.Black);
                        Pen pen2 = new Pen(Color.White)
                        {
                            DashPattern = new float[] { 5, 5 }
                        };

                        Point[] DrawPoint = new Point[CopiedItem.LineSource.Pt.Count];
                        double x = Convert.ToDouble(textBox1.Text);
                        double y = Convert.ToDouble(textBox2.Text);
                        double x0 = x - CopiedItem.LineSource.Pt[0].X;
                        double y0 = y - CopiedItem.LineSource.Pt[0].Y;
                        
                        int i = 0;
                        foreach(GralData.PointD_3d _pt in CopiedItem.LineSource.Pt)
                        {
                            int x1 = (int)((x0 + _pt.X - form1_west) * factor_x + TransformX);
                            int y1 = (int)((y0 + _pt.Y - form1_north) * factor_y + TransformY);
                            DrawPoint[i] = new Point(x1, y1);
                            i++;
                        }
                        g.DrawLines(pen, DrawPoint);
                        g.DrawLines(pen2, DrawPoint);
                        
                        pen.Dispose();
                        pen2.Dispose();
                    }
                }
                catch
                {}
                RubberRedrawAllowed = 0;
            }
        }
        
        /// <summary>
        /// Draw rubber lines for copied portal sources
        /// </summary>
        private void DrawRubberPortal()
        {
            if (CopiedItem.PortalSource == null)
            {
                return;
            }
            
            if (PictureBoxBitmap != null && RubberRedrawAllowed == 0)
            {
                RubberRedrawAllowed = 2;
                try // if picture_box is used in an other thread
                {
                    if (picturebox1.Image != null)
                        picturebox1.Image.Dispose();
                    
                    double form1_west = MapSize.West, form1_north = MapSize.North;
                    double factor_x = 1 / BmpScale / MapSize.SizeX;
                    double factor_y = 1 / BmpScale / MapSize.SizeY;
                    
                    // restore old image
                    picturebox1.Image = PictureBoxBitmap.Clone(new Rectangle(0, 0, PictureBoxBitmap.Width, PictureBoxBitmap.Height), PictureBoxBitmap.PixelFormat);
                    
                    using (Graphics g = Graphics.FromImage(picturebox1.Image))
                    {
                        Pen pen = new Pen(Color.Black);
                        Pen pen2 = new Pen(Color.White)
                        {
                            DashPattern = new float[] { 5, 5 }
                        };

                        double x = Convert.ToDouble(textBox1.Text);
                        double y = Convert.ToDouble(textBox2.Text);
                        double x0 = x - CopiedItem.PortalSource.Pt1.X;
                        double y0 = y - CopiedItem.PortalSource.Pt1.Y;
                        PointD _pt = CopiedItem.PortalSource.Pt1;
                        int x1 = (int)((x0 + _pt.X - form1_west) * factor_x + TransformX);
                        int y1 = (int)((y0 + _pt.Y - form1_north) * factor_y + TransformY);
                        _pt = CopiedItem.PortalSource.Pt2;
                        int x2 = (int)((x0 + _pt.X - form1_west) * factor_x + TransformX);
                        int y2 = (int)((y0 + _pt.Y - form1_north) * factor_y + TransformY);
                        
                        g.DrawLine(pen, x1, y1, x2, y2);
                        g.DrawLine(pen2, x1, y1, x2, y2);
                        
                        pen.Dispose();
                        pen2.Dispose();
                    }
                }
                catch
                {}
                RubberRedrawAllowed = 0;
            }
        }
        
        /// <summary>
        /// Draw rubber lines 
        /// </summary>
        private void DrawRubberLine(int cornerline, int mode)
        {
            if (cornerline > 0 && PictureBoxBitmap != null && RubberRedrawAllowed == 0)
            {
                RubberRedrawAllowed = 2;
                try // if picture_box is used in an other thread
                {
                    if (picturebox1.Image != null)
                        picturebox1.Image.Dispose();
                    // restore old image
                    picturebox1.Image = PictureBoxBitmap.Clone(new Rectangle(0, 0, PictureBoxBitmap.Width, PictureBoxBitmap.Height), PictureBoxBitmap.PixelFormat);
                    
                    using (Graphics g = Graphics.FromImage(picturebox1.Image))
                    {
                        Pen pen = new Pen(Color.Black);
                        Pen pen2 = new Pen(Color.White)
                        {
                            DashPattern = new float[] { 5, 5 }
                        };

                        g.DrawLine(pen, CornerAreaSource[cornerline].X, CornerAreaSource[cornerline].Y, CornerAreaSource[cornerline + 1].X, CornerAreaSource[cornerline + 1].Y);
                        g.DrawLine(pen2, CornerAreaSource[cornerline].X, CornerAreaSource[cornerline].Y, CornerAreaSource[cornerline + 1].X, CornerAreaSource[cornerline + 1].Y);
                        
                        if (mode == 2)
                        {
                            g.DrawLine(pen,  RubberLineCoors[1].X, RubberLineCoors[1].Y, CornerAreaSource[cornerline + 1].X, CornerAreaSource[cornerline + 1].Y);
                            g.DrawLine(pen2, RubberLineCoors[1].X, RubberLineCoors[1].Y, CornerAreaSource[cornerline + 1].X, CornerAreaSource[cornerline + 1].Y);
                        }
                        
                        pen.Dispose();
                        pen2.Dispose();
                    }
                }
                catch
                {}
                RubberRedrawAllowed = 0;
            }
        }

        /// <summary>
        /// Draw rubber rectangles
        /// </summary>
        private void DrawRubberRect(Rectangle myrect)
        {
            try // if picture_box is used in an other thread
            {
                if (PictureBoxBitmap != null && RubberRedrawAllowed == 0)
                {
                    RubberRedrawAllowed = 2;
                    if (picturebox1.Image != null)
                        picturebox1.Image.Dispose();
                    // restore old image
                    picturebox1.Image = PictureBoxBitmap.Clone(new Rectangle(0, 0, PictureBoxBitmap.Width, PictureBoxBitmap.Height), PictureBoxBitmap.PixelFormat);
                    
                    using (Graphics g = Graphics.FromImage(picturebox1.Image))
                    {
                        Pen pen = new Pen(Color.Black);
                        Pen pen2 = new Pen(Color.White)
                        {
                            DashPattern = new float[] { 5, 5 }
                        };

                        g.DrawRectangle(pen, myrect);
                        g.DrawRectangle(pen2, myrect);
                        pen.Dispose();
                        pen2.Dispose();
                    }
                }
            }
            catch
            {}
            RubberRedrawAllowed = 0;
        }
    }
}
