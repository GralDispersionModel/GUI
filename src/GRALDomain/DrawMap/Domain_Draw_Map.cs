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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GralDomain
{
    public partial class Domain
    {
        protected bool CancelDrawing = false;

        /// <summary>
        /// Draws the map in domain.picturebox 
        /// </summary> 
        private void DrawMap()
        {

            if (PictureBoxBitmap == null)
            {
                PictureBoxBitmap = new Bitmap(picturebox1.Width, picturebox1.Height);
            }
            else
            {
                if (PictureBoxBitmap.Width != picturebox1.Width || PictureBoxBitmap.Height != picturebox1.Height)
                {
                    PictureBoxBitmap.Dispose();
                    PictureBoxBitmap = new Bitmap(picturebox1.Width, picturebox1.Height);
                }
            }

            Graphics g = Graphics.FromImage(PictureBoxBitmap);
            g.Clear(Color.White);
            g.CompositingQuality = CompositingQuality.GammaCorrected;

            int pb1_height = picturebox1.Height;
            int pb1_width = picturebox1.Width;

            StringFormat format1 = new StringFormat(); //format for names of sources
            StringFormat format2 = new StringFormat(); //format for names of line sources
            format1.LineAlignment = StringAlignment.Center;
            format1.Alignment = StringAlignment.Center;
            format2.LineAlignment = StringAlignment.Far;
            format2.Alignment = StringAlignment.Center;

            double factor_x = 1 / BmpScale / MapSize.SizeX;
            double factor_y = 1 / BmpScale / MapSize.SizeY;
            double form1_west = MapSize.West, form1_north = MapSize.North;  // Kuntner
            float _bmppbx_save = (float)Math.Max(BmppbXSave, 1E-7); // avoid division by 0

            // Reset Rubber-Line Drawing
            RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;

            // time check!
            System.Timers.Timer CancelTimer = new System.Timers.Timer
            {
                Interval = 10000 // 10 seconds = cancel
            };
            CancelTimer.Elapsed += (st, et) => CancelDrawing = true;
            CancelTimer.Start();

            DrawingObjects _drobj;

            // loop over all objects
            for (int name = ItemOptions.Count - 1; name > -1; name--)
            {
                _drobj = ItemOptions[name];
                string obname = _drobj.Name;

                // create LabelFont, LabelBrush and SelectedBrush
                Font LabelFont, NorthFont;
                if (_drobj.ScaleLabelFont)
                {
                    LabelFont = new Font(_drobj.LabelFont.Name, Convert.ToInt32(Math.Max(1, Math.Min(2000, _drobj.LabelFont.Size * 0.5F * factor_x))));
                    NorthFont = new Font(_drobj.LabelFont.Name, Convert.ToInt32(Math.Max(1, Math.Min(2000, _drobj.LabelFont.Size * 0.5F * factor_x))), FontStyle.Bold);
                }
                else
                {
                    LabelFont = new Font(_drobj.LabelFont.Name, _drobj.LabelFont.Size * Convert.ToInt32(Math.Min(32000, 1 / _bmppbx_save)));
                    NorthFont = new Font(_drobj.LabelFont.Name, _drobj.LabelFont.Size * Convert.ToInt32(Math.Min(32000, 1 / _bmppbx_save)), FontStyle.Bold);
                }
                Brush LabelBrush = new SolidBrush(_drobj.LabelColor);
                if (_drobj.LabelColor == Color.Empty)
                {
                    LabelBrush = new SolidBrush(Color.Blue);
                }
                Brush SelectedBrush = new SolidBrush(Color2Transparent(150, Color.Green));


                //draw base map
                if ((obname.Substring(0, 3) == "BM:") && (_drobj.Show == true))
                {
                    DrawBaseMap(g, _drobj);
                }

                //draw shape map
                if ((obname.Substring(0, 3) == "SM:") && (_drobj.Show == true))
                {
                    DrawShapeMap(g, _drobj, form1_west, form1_north, factor_x, factor_y);
                }

                CancelDrawing = false; CancelTimer.Start();

                //draw vector maps
                try
                {
                    if (obname.Substring(0, 3) == "VM:")
                    {
                        if (_drobj.Show == true)
                        {
                            DrawVectorMap(g, _drobj, form1_west, form1_north, factor_x, factor_y);
                        }
                    }
                }
                catch
                { }
                CancelDrawing = false; CancelTimer.Start();

                //draw contour maps
                try
                {
                    if (obname.Substring(0, 3) == "CM:")
                    {
                        if (_drobj.Show == true)
                        {
                            DrawContourMap(g, _drobj, form1_west, form1_north, factor_x, factor_y, LabelFont, LabelBrush);
                        }
                    }
                }
                catch
                { }

                CancelDrawing = false; CancelTimer.Start();
                g.ResetClip(); // Reset Clipping area

                //draw postmaps
                try
                {
                    if (obname.Substring(0, 3) == "PM:")
                    {
                        if (_drobj.Show == true)
                        {
                            DrawPostMap(g, _drobj, form1_west, form1_north, factor_x, factor_y);
                        }
                    }
                }
                catch
                {
                }

                //draw area sources
                if ((obname == "AREA SOURCES") && (_drobj.Show == true))
                {
                    DrawAreaSources(g, _drobj, form1_west, form1_north, factor_x, factor_y, LabelFont, LabelBrush);
                }
                CancelDrawing = false; CancelTimer.Start();


                //draw buildings
                if ((obname == "BUILDINGS") && (_drobj.Show == true))
                {
                    DrawBuildings(g, _drobj, form1_west, form1_north, factor_x, factor_y, LabelFont, LabelBrush);
                }

                //draw line sources
                if ((obname == "LINE SOURCES") && (_drobj.Show == true))
                {
                    DrawLineSources(g, _drobj, form1_west, form1_north, factor_x, factor_y, LabelFont, LabelBrush);
                }

                //draw walls
                if ((obname == "WALLS") && (_drobj.Show == true))
                {
                    DrawWalls(g, _drobj, form1_west, form1_north, factor_x, factor_y, LabelFont, LabelBrush);
                }

                //draw point sources
                if ((obname == "POINT SOURCES") && (_drobj.Show == true))
                {
                    DrawPointSources(g, _drobj, form1_west, form1_north, factor_x, factor_y, LabelFont, LabelBrush);
                }

                //draw receptors
                if ((obname == "RECEPTORS") && (_drobj.Show == true))
                {
                    DrawReceptors(g, _drobj, form1_west, form1_north, factor_x, factor_y, LabelFont, LabelBrush);
                }

                //draw portal sources
                if ((obname == "TUNNEL PORTALS") && (_drobj.Show == true))
                {
                    DrawPortalSources(g, _drobj, form1_west, form1_north, factor_x, factor_y, LabelFont, LabelBrush);
                }

                //draw Vegetation
                try
                {
                    if ((obname == "VEGETATION") && (_drobj.Show == true))
                    {
                        DrawVegetation(g, _drobj, form1_west, form1_north, factor_x, factor_y, LabelFont, LabelBrush);
                    } // Vegetation
                }
                catch { }

                // draw windroses
                if ((obname == "WINDROSE" && (_drobj.Show == true)))
                {
                    DrawWindRose(g, _drobj, form1_west, form1_north, factor_x, factor_y, LabelFont, LabelBrush);
                }

                // draw concentration values
                if ((obname == "CONCENTRATION VALUES" && (_drobj.Show == true)))
                {
                    DrawConcentrationValues(g, _drobj, form1_west, form1_north, factor_x, factor_y, LabelFont, LabelBrush);
                }

                // draw info boxes
                if ((obname == "ITEM INFO" && (_drobj.Show == true)))
                {
                    DrawItemInfo(g, _drobj, form1_west, form1_north, factor_x, factor_y, LabelFont, LabelBrush);
                }

                // draw Bitmap - Coordinate Raster
                if ((obname.Substring(0, 3) == "BM:") && (_drobj.Show == true))
                {
                    // Draw raster
                    if (_drobj.Label == 2 || _drobj.Label == 3)
                    {
                        DrawRaster(g, _drobj, form1_west, form1_north, factor_x, factor_y, LabelFont, LabelBrush);
                    }
                }

                //draw map scale bar
                try
                {
                    if ((obname == "SCALE BAR") && (_drobj.Show == true))
                    {
                        DrawScaleBar(g, _drobj, _bmppbx_save, LabelFont, LabelBrush, factor_x, factor_y, form1_west, form1_north);
                    }
                }
                catch { }

                //draw GRAL model domain
                try
                {
                    if ((obname == "GRAL DOMAIN") && (_drobj.Show == true) && (MouseControl != MouseMode.GralDomainEndPoint))
                    {
                        int x1 = Convert.ToInt32((MainForm.GralDomRect.West - form1_west) * factor_x) + TransformX;
                        int y1 = Convert.ToInt32((MainForm.GralDomRect.North - form1_north) * factor_y) + TransformY;
                        int x2 = Convert.ToInt32((MainForm.GralDomRect.East - form1_west) * factor_x) + TransformX;
                        int y2 = Convert.ToInt32((MainForm.GralDomRect.South - form1_north) * factor_y) + TransformY;
                        int width = x2 - x1;
                        int height = y2 - y1;
                        Rectangle GRALDom = new Rectangle(x1, y1, width, height);

                        if (_drobj.LineWidth > 0)
                        {
                            Pen pen1 = new Pen(Color2Transparent(_drobj.Transparancy, _drobj.LineColors[0]), _drobj.LineWidth);
                            g.DrawRectangle(pen1, GRALDom);
                            pen1.Dispose();
                        }
                        // Draw GRAL Raster
                        if (_drobj.FillYesNo == true)
                        {
                            Pen raster = new Pen(_drobj.LineColors[0], 1)
                            {
                                DashStyle = DashStyle.Dot
                            };
                            double step = MainForm.HorGridSize * factor_x;

                            if (step > 10) // don´t show small grids
                            {
                                for (double x = x1; x <= x2; x += step)
                                {
                                    if (x > 0 && x < picturebox1.Width)
                                    {
                                        int xr = (int)(x);
                                        int _y0 = Math.Max(0, y1);
                                        int _y1 = Math.Min(picturebox1.Height, y2);
                                        g.DrawLine(raster, xr, _y0, xr, _y1);
                                    }
                                }
                                for (double y = y1; y <= y2; y += step)
                                {
                                    if (y > 0 && y < picturebox1.Height)
                                    {
                                        int yr = (int)(y);
                                        int _x0 = Math.Max(0, x1);
                                        int _x1 = Math.Min(picturebox1.Width, x2);
                                        g.DrawLine(raster, _x0, yr, _x1, yr);
                                    }
                                }
                            }
                            raster.Dispose();
                        }

                    }
                }
                catch { }

                //draw GRAMM model domain
                try
                {
                    if ((obname == "GRAMM DOMAIN") && (_drobj.Show == true) && (MouseControl != MouseMode.GrammDomainEndPoint))
                    {
                        int x1 = Convert.ToInt32((MainForm.GrammDomRect.West - form1_west) * factor_x) + TransformX;
                        int y1 = Convert.ToInt32((MainForm.GrammDomRect.North - form1_north) * factor_y) + TransformY;
                        int x2 = Convert.ToInt32((MainForm.GrammDomRect.East - form1_west) * factor_x) + TransformX;
                        int y2 = Convert.ToInt32((MainForm.GrammDomRect.South - form1_north) * factor_y) + TransformY;
                        int width = x2 - x1;
                        int height = y2 - y1;
                        Rectangle GRAMMDom = new Rectangle(x1, y1, width, height);

                        if (_drobj.LineWidth > 0)
                        {
                            Pen pen1 = new Pen(Color2Transparent(_drobj.Transparancy, _drobj.LineColors[0]), _drobj.LineWidth);
                            g.DrawRectangle(pen1, GRAMMDom);
                            pen1.Dispose();
                        }

                        // Draw GRAMM Raster
                        if (_drobj.FillYesNo == true)
                        {
                            Pen raster = new Pen(_drobj.LineColors[0], 1)
                            {
                                DashStyle = DashStyle.Dot
                            };
                            double step = MainForm.GRAMMHorGridSize * factor_x;

                            if (step > 10) // don´t show small grids
                            {
                                for (double x = x1; x <= x2; x += step)
                                {
                                    int xr = (int)(x);
                                    g.DrawLine(raster, xr, y1, xr, y2);
                                }
                                for (double y = y1; y <= y2; y += step)
                                {
                                    int yr = (int)(y);
                                    g.DrawLine(raster, x1, yr, x2, yr);
                                }
                            }
                            raster.Dispose();
                        }
                    }
                }
                catch { }

                //draw north arrow
                try
                {
                    if ((obname == "NORTH ARROW") && (_drobj.Show == true))
                    {

                        int xpos1 = Convert.ToInt32(NorthArrow.X / _bmppbx_save);
                        int ypos1 = Convert.ToInt32(NorthArrow.Y / _bmppbx_save);
                        int size = Convert.ToInt32(Math.Min(32000, 20 * Convert.ToDouble(_drobj.ContourLabelDist / 100D) / _bmppbx_save));
                        System.Drawing.PointF[] points = new System.Drawing.PointF[3];
                        points[0] = new PointF(xpos1, ypos1);
                        points[1] = new PointF(xpos1 - size, ypos1 + size * 2);
                        points[2] = new PointF(xpos1, ypos1 + size * 1.5F);
                        using (Pen north = new Pen(Color.Black, 2))
                        {
                            g.DrawPolygon(north, points);
                            points[0] = new PointF(xpos1 + 1, ypos1);
                            points[1] = new PointF(xpos1 + size, ypos1 + size * 2);
                            points[2] = new PointF(xpos1 + 1, ypos1 + size * 1.5F);
                            g.DrawPolygon(north, points);
                        }
                        using (SolidBrush brush = new SolidBrush(Color.Black))
                        {
                            g.FillPolygon(brush, points);
                        }
                        SizeF fsize = g.MeasureString("N", NorthFont);
                        g.DrawString("N", NorthFont, LabelBrush, xpos1 - fsize.Width / 2, ypos1 - fsize.Height * 1.2F);
                    }
                }
                catch { }

                LabelFont.Dispose();
                LabelBrush.Dispose();
                SelectedBrush.Dispose();
                NorthFont.Dispose();
            } // Object-Loop

            Pen p = new Pen(Color.LightBlue, 3);
            //draw actual edited GRAL model domain
            if (MouseControl == MouseMode.GralDomainEndPoint)
            {
                g.DrawRectangle(p, GRALDomain);
            }

            //draw actual edited GRAMM model domain
            if (MouseControl == MouseMode.GrammDomainEndPoint)
            {
                g.DrawRectangle(p, GRAMMDomain);
            }

            //draw actual edited line source
            if ((EditLS.CornerLineSource > 0) && (MouseControl == MouseMode.LineSourcePos))
            {
                for (int i = 0; i < EditLS.CornerLineSource; i++)
                {
                    g.DrawLine(p, CornerAreaSource[i], CornerAreaSource[i + 1]);
                }
            }

            //draw actual edited wall
            if ((EditWall.CornerWallCount > 0) && (MouseControl == MouseMode.WallSet))
            {
                for (int i = 0; i < EditWall.CornerWallCount; i++)
                {
                    g.DrawLine(p, CornerAreaSource[i], CornerAreaSource[i + 1]);
                }
            }

            //draw actual edited tunnel portal
            if ((EditLS.CornerLineSource > 0) && (MouseControl == MouseMode.PortalSourcePos))
            {
                //				for (int i = 0; i < editls.cornerline + 1; i++)
                //					g.DrawLine(p, cornerareasource[i], cornerareasource[i + 1]);
            }

            //draw measuring distance
            if ((EditLS.CornerLineSource > 0) && (MouseControl == MouseMode.ViewDistanceMeasurement))
            {
                for (int i = 0; i < EditLS.CornerLineSource; i++)
                {
                    g.DrawLine(p, CornerAreaSource[i], CornerAreaSource[i + 1]);
                }
            }

            //draw line for windfield section drawing
            if ((EditLS.CornerLineSource > 0) && (MouseControl == MouseMode.SectionWindSel))
            {
                for (int i = 0; i < EditLS.CornerLineSource; i++)
                {
                    g.DrawLine(p, CornerAreaSource[i], CornerAreaSource[i + 1]);
                }
            }

            //draw acutal edited area source
            if ((EditAS.CornerAreaCount > 0) && ((MouseControl == MouseMode.AreaSourcePos) || (MouseControl == MouseMode.ViewAreaMeasurement)))
            {
                for (int i = 0; i < EditAS.CornerAreaCount; i++)
                {
                    g.DrawLine(p, CornerAreaSource[i], CornerAreaSource[i + 1]);
                }
            }

            //draw acutal edited vegetation
            if ((EditVegetation.CornerVegetation > 0) && ((MouseControl == MouseMode.VegetationPosCorner)))
            {
                for (int i = 0; i < EditVegetation.CornerVegetation; i++)
                {
                    g.DrawLine(p, CornerAreaSource[i], CornerAreaSource[i + 1]);
                }
            }

            //draw actual edited building
            if ((EditB.CornerBuilding > 0) && (MouseControl == MouseMode.BuildingPos))
            {
                for (int i = 0; i < EditB.CornerBuilding; i++)
                {
                    g.DrawLine(p, CornerAreaSource[i], CornerAreaSource[i + 1]);
                }
            }
            p.Dispose();

            if ((MouseControl == MouseMode.LineSourceEditFinal) || (MouseControl == MouseMode.AreaSourceEditFinal)
                || (MouseControl == MouseMode.VegetationEditFinal) || (MouseControl == MouseMode.BuildingEditFinal)) // Kuntner: if edge-point is moved
            {
                g.DrawRectangle(new Pen(Color.Black, 1), CornerAreaSource[1].X - 3, CornerAreaSource[1].Y - 3, 6, 6);
            }

            Pen pz = new Pen(Color.White, 3)
            {
                DashStyle = DashStyle.Dot
            };            //Pen for drawing panelzoom rectangle

            //draw panelzoom rectangle
            if (MouseControl == MouseMode.ViewPanelZoomArea)
            {
                g.DrawRectangle(new Pen(Color.Black, 3), PanelZoom);
                g.DrawRectangle(pz, PanelZoom);
            }
            pz.Dispose();

            //draw color scales
            DrawColorScales(g, _bmppbx_save, factor_x, factor_y, form1_west, form1_north);

            // draw section Lines
            if (sectionpoints.Count > 1)
            {
                Pen mypen = new Pen(Color.Red, 4)
                {
                    DashStyle = DashStyle.DashDot,
                    //mypen.StartCap = LineCap.RoundAnchor;
                    EndCap = LineCap.ArrowAnchor,
                    Alignment = PenAlignment.Center
                };
                int x1;
                int y1;
                int x2;
                int y2;

                for (int i = 0; i < sectionpoints.Count; i += 2)
                {
                    x1 = (int)((sectionpoints[i].X - form1_west) * factor_x + TransformX);
                    y1 = (int)((sectionpoints[i].Y - form1_north) * factor_y + TransformY);
                    x2 = (int)((sectionpoints[i + 1].X - form1_west) * factor_x + TransformX);
                    y2 = (int)((sectionpoints[i + 1].Y - form1_north) * factor_y + TransformY);
                    g.DrawLine(mypen, x1, y1, x2, y2);
                    mypen.DashStyle = DashStyle.Solid;
                    mypen.Width = 3;
                    g.DrawEllipse(mypen, new Rectangle(x1 - 6, y1 - 6, 12, 12));
                }

                mypen.Dispose();
            }

            // draw Vertice-Edit Point
            if (MarkerPoint.X != 0 || MarkerPoint.Y != 0)
            {
                int x1 = Convert.ToInt32((MarkerPoint.X - form1_west) * factor_x + TransformX);
                int y1 = Convert.ToInt32((MarkerPoint.Y - form1_north) * factor_y + TransformY);
                Pen mypen = new Pen(Color.Red, 4);
                g.DrawEllipse(mypen, new Rectangle(x1 - 6, y1 - 6, 12, 12));
                mypen.Dispose();
            }

            CancelTimer.Stop();
            CancelTimer.Dispose();

            g.Dispose();
        }

        //setting transparent colors
        private Color Color2Transparent(int transvalue, Color c)
        {
            return Color.FromArgb(transvalue, c.R, c.G, c.B); // setting opacity to about 50%
        }
    }
}
