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
 * Date: 06.01.2019
 * Time: 16:09
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Move the map using arrow keys, zoom using the map, reset copy item with ESC
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // no override if an item form has the focus
            if (EditB.ContainsFocus || EditPS.ContainsFocus || EditAS.ContainsFocus || EditLS.ContainsFocus || EditPortals.ContainsFocus || EditR.ContainsFocus || EditWall.ContainsFocus || EditVegetation.ContainsFocus)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            int movex = 0;
            int movey = 0;
            double xfac_old = XFac;
            double transformx_old = TransformX;
            double transformy_old = TransformY;

            if (keyData == Keys.Escape) // Reset copy objects 
            {
                CopiedItem.PointSource = null;
                CopiedItem.Building = null;
                CopiedItem.LineSource = null;
                CopiedItem.Receptor = null;
                CopiedItem.AreaSource = null;
                CopiedItem.PortalSource = null;

                // reset move line sources
                if (MouseControl == MouseMode.LineSourceInlineEdit)
                {
                    MouseControl = MouseMode.LineSourceSel;
                }
                // reset move walls
                if (MouseControl == MouseMode.WallInlineEdit)
                {
                    MouseControl = MouseMode.WallSel;
                }
                // reset move vegetation
                if (MouseControl == MouseMode.VegetationInlineEdit)
                {
                    MouseControl = MouseMode.VegetationSel;
                }
                // reset move area source
                if (MouseControl == MouseMode.AreaInlineEdit)
                {
                    MouseControl = MouseMode.AreaSourceSel;
                }
                // reset move building
                if (MouseControl == MouseMode.BuildingInlineEdit)
                {
                    MouseControl = MouseMode.BuildingSel;
                }
                // reset move point source 
                if (MouseControl == MouseMode.PointSourceInlineEdit)
                {
                    MouseControl = MouseMode.PointSourceSel;
                }
                // reset move receptor 
                if (MouseControl == MouseMode.ReceptorInlineEdit)
                {
                    MouseControl = MouseMode.ReceptorSel;
                }
                // reset lenght and area measurement
                if (MouseControl == MouseMode.ViewDistanceMeasurement || MouseControl == MouseMode.ViewAreaMeasurement)
                {
                    if (EditLS.CornerLineSource > 0 || EditAS.CornerAreaCount > 1)
                    {
                        EditLS.CornerLineSource = 0;
                        EditAS.CornerAreaCount = 0;
                        
                        // Tooltip for picturebox1
                        ToolTipMousePosition.Active = false; // don't show tool tip lenght of rubberline segments anymore
                        // Reset Rubber-Line Drawing
                        Cursor.Clip = Rectangle.Empty;
                        RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                        Picturebox1_Paint();
                    }
                }

                //reset zoom and move map 
                if (MouseControl == MouseMode.ZoomOut || MouseControl == MouseMode.ZoomIn || MouseControl == MouseMode.ViewMoveMap)
                {
                    MouseControl = MouseMode.Default;
                    Cursor = Cursors.Default;
                }
            }

            // Shift topmost map start
            if (MouseControl == MouseMode.BaseMapMoveScale)
            {
                // search top base map at object manager
                DrawingObjects _drobj = null;
                foreach (DrawingObjects _dr in ItemOptions)
                {
                    if ((_dr.Name.Substring(0, 3) == "BM:"))
                    {
                        _drobj = _dr;
                        break;
                    }
                }

                if (_drobj != null)
                {
                    if (BaseMapOldValues.Destrec.Width == 0) // Save old values temporarily for cancel
                    {
                        BaseMapOldValues.Destrec = _drobj.DestRec;
                        BaseMapOldValues.West = _drobj.West;
                        BaseMapOldValues.North = _drobj.North;
                        BaseMapOldValues.PixelMX = _drobj.PixelMx;
                    }

                    int zoom = 0;
                    int delta = 2;
                    if (_drobj.DestRec.Width > 1 && _drobj.SourceRec.Width > 1)
                    {
                        delta = Math.Max(2, _drobj.DestRec.Width / 8000);

                        if (keyData == Keys.Left)
                        {
                            movex = -delta;
                        }

                        if (keyData == Keys.Right)
                        {
                            movex = delta;
                        }

                        if (keyData == Keys.Up)
                        {
                            movey = -delta;
                        }

                        if (keyData == Keys.Down)
                        {
                            movey = delta;
                        }

                        if (keyData == Keys.Oemplus || keyData == Keys.Add)
                        {
                            zoom = delta / 2;
                        }

                        if (keyData == Keys.OemMinus || keyData == Keys.Subtract)
                        {
                            zoom = -delta / 2;
                        }

                        int destrec_width = _drobj.DestRec.Width;
                        _drobj.DestRec = new Rectangle(_drobj.DestRec.Left + movex, _drobj.DestRec.Top + movey, Math.Max(1, _drobj.DestRec.Width + zoom), Math.Max(1, (_drobj.DestRec.Width + zoom) * _drobj.SourceRec.Height / _drobj.SourceRec.Width));

                        _drobj.West = (_drobj.DestRec.Left - TransformX) * BmpScale * MapSize.SizeX + MapSize.West;
                        _drobj.North = (-_drobj.DestRec.Top + TransformY) * BmpScale * MapSize.SizeX + MapSize.North;

                        _drobj.PixelMx = _drobj.PixelMx * _drobj.DestRec.Width / destrec_width;

                        Picturebox1_Paint();
                    }
                }

                if (keyData == Keys.Escape) // cancel the shift + zoom option
                {
                    MouseControl = MouseMode.Default;
                    // reset to old values
                    _drobj.DestRec = BaseMapOldValues.Destrec;
                    _drobj.West = BaseMapOldValues.West;
                    _drobj.North = BaseMapOldValues.North;
                    _drobj.PixelMx = BaseMapOldValues.PixelMX;

                    toolStripButton1.PerformClick();
                    Picturebox1_Paint();

                }
                return base.ProcessCmdKey(ref msg, keyData);
            }
            // Shift topmost map end


            if (keyData == Keys.Left)
            {
                movex = -Convert.ToInt32(100);
            }

            if (keyData == Keys.Right)
            {
                movex = Convert.ToInt32(100);
            }

            if (keyData == Keys.Up)
            {
                movey = -Convert.ToInt32(100);
            }

            if (keyData == Keys.Down)
            {
                movey = Convert.ToInt32(100);
            }

            TransformX += movex;
            TransformY += movey;

            // Rotate a copied item
            if (keyData == Keys.L || keyData == (Keys.L | Keys.Shift))
            {
                RotateCopiedItem(0);
            }
            if (keyData == Keys.R || keyData == (Keys.R | Keys.Shift))
            {
                RotateCopiedItem(1);
            }

            //new coordinates for base maps
            foreach (DrawingObjects _dr in ItemOptions)
            {
                try
                {
                    _dr.DestRec = new Rectangle(TransformX + Convert.ToInt32((_dr.West - MapSize.West) / BmpScale / MapSize.SizeX),
                                                TransformY - Convert.ToInt32((_dr.North - MapSize.North) / BmpScale / MapSize.SizeX),
                                                Convert.ToInt32(_dr.Picture.Width * _dr.PixelMx / MapSize.SizeX * XFac),
                                                Convert.ToInt32(_dr.Picture.Height * _dr.PixelMx / MapSize.SizeX * XFac));
                    _dr.SourceRec = new Rectangle(0, 0, _dr.Picture.Width, _dr.Picture.Height);
                }
                catch { }
            }

            //zoom in
            if (keyData == Keys.Oemplus || keyData == Keys.Add)
            {
                XFac *= 1.5;
                BmpScale = 1 / XFac;
                TransformX = Convert.ToInt32((TransformX * 1.5 + (picturebox1.Width / 2 - (picturebox1.Width / 2) * 1.5)));
                TransformY = Convert.ToInt32((TransformY * 1.5 + (picturebox1.Height / 2 - (picturebox1.Height / 2) * 1.5)));

                foreach (DrawingObjects _dr in ItemOptions)
                {
                    try
                    {
                        _dr.DestRec = new Rectangle(TransformX + Convert.ToInt32((_dr.West - MapSize.West) / BmpScale / MapSize.SizeX),
                                                    TransformY - Convert.ToInt32((_dr.North - MapSize.North) / BmpScale / MapSize.SizeX),
                                                    Convert.ToInt32(_dr.Picture.Width * _dr.PixelMx / MapSize.SizeX * XFac),
                                                    Convert.ToInt32(_dr.Picture.Height * _dr.PixelMx / MapSize.SizeX * XFac));
                        _dr.SourceRec = new Rectangle(0, 0, _dr.Picture.Width, _dr.Picture.Height);
                    }
                    catch { }
                }
            }

            //zoom out
            if (keyData == Keys.OemMinus || keyData == Keys.Subtract)
            {
                XFac /= 1.5;
                BmpScale = 1 / XFac;
                TransformX = Convert.ToInt32((TransformX / 1.5 + (picturebox1.Width / 2 - (picturebox1.Width / 2) / 1.5)));
                TransformY = Convert.ToInt32((TransformY / 1.5 + (picturebox1.Height / 2 - (picturebox1.Height / 2) / 1.5)));

                foreach (DrawingObjects _dr in ItemOptions)
                {
                    try
                    {
                        _dr.DestRec = new Rectangle(TransformX + Convert.ToInt32((_dr.West - MapSize.West) / BmpScale / MapSize.SizeX),
                                                    TransformY - Convert.ToInt32((_dr.North - MapSize.North) / BmpScale / MapSize.SizeX),
                                                    Convert.ToInt32(_dr.Picture.Width * _dr.PixelMx / MapSize.SizeX * XFac),
                                                    Convert.ToInt32(_dr.Picture.Height * _dr.PixelMx / MapSize.SizeX * XFac));
                        _dr.SourceRec = new Rectangle(0, 0, _dr.Picture.Width, _dr.Picture.Height);
                    }
                    catch { }
                }
            }

            //new coordinates for GRAL model domain
            try
            {
                int x1 = Convert.ToInt32((MainForm.GralDomRect.West - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                int y1 = Convert.ToInt32((MainForm.GralDomRect.North - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                int x2 = Convert.ToInt32((MainForm.GralDomRect.East - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                int y2 = Convert.ToInt32((MainForm.GralDomRect.South - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                int width = x2 - x1;
                int height = y2 - y1;
                GRALDomain = new Rectangle(x1, y1, width, height);
            }
            catch
            {
            }

            //new coordinates for GRAMM model domain
            try
            {
                int x1 = Convert.ToInt32((MainForm.GrammDomRect.West - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                int y1 = Convert.ToInt32((MainForm.GrammDomRect.North - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                int x2 = Convert.ToInt32((MainForm.GrammDomRect.East - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                int y2 = Convert.ToInt32((MainForm.GrammDomRect.South - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                int width = x2 - x1;
                int height = y2 - y1;
                GRAMMDomain = new Rectangle(x1, y1, width, height);
            }
            catch
            {
            }

            // new coordinates of edited items
            MoveCoordinatesOfEditedItems(xfac_old, transformx_old, transformy_old);

            object sender = new object();
            EventArgs e = new EventArgs();

            //delete selected area source
            if (MouseControl == MouseMode.AreaSourceSel)
            {
                if (keyData == (Keys.Shift | Keys.Delete)) // delete mutiple elements
                {
                    ItemsDelete("area source");
                    EditAndSaveAreaSourceData(sender, e);
                }
                else if (keyData == Keys.Delete) // delete last element
                {
                    EditAS.RemoveOne(true);
                    InfoBoxCloseAllForms();
                    EditAndSaveAreaSourceData(sender, e);
                }
            }
            //delete selected line source
            if (MouseControl == MouseMode.LineSourceSel)
            {
                if (keyData == (Keys.Shift | Keys.Delete)) // delete mutiple elements
                {
                    ItemsDelete("line source");
                    EditAndSaveLineSourceData(sender, e);
                }
                else if (keyData == Keys.Delete) // delete last element
                {
                    EditLS.RemoveOne(true);
                    InfoBoxCloseAllForms();
                    EditAndSaveLineSourceData(sender, e);
                }
            }
            //delete selected point source
            if (MouseControl == MouseMode.PointSourceSel)
            {
                if (keyData == (Keys.Shift | Keys.Delete)) // delete mutiple elements
                {
                    ItemsDelete("point source");
                    EditAndSavePointSourceData(sender, e);
                }
                else if (keyData == Keys.Delete) // delete last element
                {
                    EditPS.RemoveOne(true);
                    InfoBoxCloseAllForms();
                    EditAndSavePointSourceData(sender, e);
                }
            }
            //delete selected tunnel portal
            if (MouseControl == MouseMode.PortalSourceSel)
            {
                if (keyData == (Keys.Shift | Keys.Delete)) // delete mutiple elements
                {
                    ItemsDelete("portal");
                    EditAndSavePortalSourceData(sender, e);
                }
                else if (keyData == Keys.Delete) // delete last element
                {
                    EditPortals.RemoveOne(true);
                    InfoBoxCloseAllForms();
                    EditAndSavePortalSourceData(sender, e);
                }
            }
            //delete selected building
            if (MouseControl == MouseMode.BuildingSel)
            {
                if (keyData == (Keys.Shift | Keys.Delete)) // delete mutiple elements
                {
                    ItemsDelete("building");
                    EditAndSaveBuildingsData(sender, e);
                }
                else if (keyData == Keys.Delete) // delete last element
                {
                    EditB.RemoveOne(true, true);
                    InfoBoxCloseAllForms(); // close all infoboxes
                    EditAndSaveBuildingsData(sender, e);
                }

            }
            Picturebox1_Paint();
            Picturebox1_MouseMove(null, new MouseEventArgs(MouseButtons.None, 0,
                picturebox1.PointToClient(Cursor.Position).X, 
                picturebox1.PointToClient(Cursor.Position).Y, 0)); // needed to redraw rubber lines 
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Move the coordinates of temporarily stored coordinates while editing a shape and move the map
        /// </summary>
        private void MoveCoordinatesOfEditedItems(double xfac_old, double transformx_old, double transformy_old)
        {
            if (MouseControl == MouseMode.SetPointConcFile)
            {
                InfoBoxCloseAllForms(); // delete info boxes at concentration info
            }

            //new coordinates for edited line source, walls or section drawing for wind fields
            if ((MouseControl == MouseMode.LineSourcePos) || (MouseControl == MouseMode.ViewDistanceMeasurement) || 
                (MouseControl == MouseMode.SectionWindSel) || (MouseControl == MouseMode.SectionConcSel) || (MouseControl == MouseMode.LineSourceEditFinal) || (MouseControl == MouseMode.LineSourceInlineEdit))

            {
                if (MouseControl == MouseMode.LineSourceEditFinal)
                {
                    CornerAreaSource[1].X = Convert.ToInt32(TransformX + (CornerAreaSource[1].X - transformx_old) / xfac_old * XFac);
                    CornerAreaSource[1].Y = Convert.ToInt32(TransformY + (CornerAreaSource[1].Y - transformy_old) / xfac_old * XFac);
                }
                else
                {
                    for (int i = 0; i <= Math.Max(1, EditLS.CornerLineSource); i++)
                    {
                        CornerAreaSource[i].X = Convert.ToInt32(TransformX + (CornerAreaSource[i].X - transformx_old) / xfac_old * XFac);
                        CornerAreaSource[i].Y = Convert.ToInt32(TransformY + (CornerAreaSource[i].Y - transformy_old) / xfac_old * XFac);
                    }
                }
            }

            //new coordinates for edited area source
            if ((MouseControl == MouseMode.AreaSourcePos) || (MouseControl == MouseMode.ViewAreaMeasurement) || (MouseControl == MouseMode.AreaSourceEditFinal) || (MouseControl == MouseMode.AreaInlineEdit))
            {
                if (MouseControl == MouseMode.AreaSourceEditFinal)
                {
                    CornerAreaSource[1].X = Convert.ToInt32(TransformX + (CornerAreaSource[1].X - transformx_old) / xfac_old * XFac);
                    CornerAreaSource[1].Y = Convert.ToInt32(TransformY + (CornerAreaSource[1].Y - transformy_old) / xfac_old * XFac);
                }
                else
                {
                    for (int i = 0; i <= EditAS.CornerAreaCount; i++)
                    {
                        CornerAreaSource[i].X = Convert.ToInt32(TransformX + (CornerAreaSource[i].X - transformx_old) / xfac_old * XFac);
                        CornerAreaSource[i].Y = Convert.ToInt32(TransformY + (CornerAreaSource[i].Y - transformy_old) / xfac_old * XFac);
                    }
                }
            }

            //new coordinates for edited building
            if ((MouseControl == MouseMode.BuildingPos) || (MouseControl == MouseMode.BuildingEditFinal) || (MouseControl == MouseMode.BuildingInlineEdit))
            {
                if (MouseControl == MouseMode.BuildingEditFinal)
                {
                    CornerAreaSource[1].X = Convert.ToInt32(TransformX + (CornerAreaSource[1].X - transformx_old) / xfac_old * XFac);
                    CornerAreaSource[1].Y = Convert.ToInt32(TransformY + (CornerAreaSource[1].Y - transformy_old) / xfac_old * XFac);
                }
                else
                {
                    for (int i = 0; i <= EditB.CornerBuilding; i++)
                    {
                        CornerAreaSource[i].X = Convert.ToInt32(TransformX + (CornerAreaSource[i].X - transformx_old) / xfac_old * XFac);
                        CornerAreaSource[i].Y = Convert.ToInt32(TransformY + (CornerAreaSource[i].Y - transformy_old) / xfac_old * XFac);
                    }
                }
            }

            //new coordinates for edited walls
            if ((MouseControl == MouseMode.WallSet) || (MouseControl == MouseMode.WallEditFinal) || (MouseControl == MouseMode.WallInlineEdit) || (MouseControl == MouseMode.AreaInlineEdit))
            {
                if (MouseControl == MouseMode.WallEditFinal)
                {
                    CornerAreaSource[1].X = Convert.ToInt32(TransformX + (CornerAreaSource[1].X - transformx_old) / xfac_old * XFac);
                    CornerAreaSource[1].Y = Convert.ToInt32(TransformY + (CornerAreaSource[1].Y - transformy_old) / xfac_old * XFac);
                }
                else
                {
                    for (int i = 0; i <= Math.Max(1, EditWall.CornerWallCount); i++)
                    {
                        CornerAreaSource[i].X = Convert.ToInt32(TransformX + (CornerAreaSource[i].X - transformx_old) / xfac_old * XFac);
                        CornerAreaSource[i].Y = Convert.ToInt32(TransformY + (CornerAreaSource[i].Y - transformy_old) / xfac_old * XFac);
                    }
                }
            }

            //new coordinates for edited forests
            if ((MouseControl == MouseMode.AreaPosCorner) || (MouseControl == MouseMode.VegetationEditFinal) || (MouseControl == MouseMode.VegetationInlineEdit))
            {
                if (MouseControl == MouseMode.VegetationEditFinal)
                {
                    CornerAreaSource[1].X = Convert.ToInt32(TransformX + (CornerAreaSource[1].X - transformx_old) / xfac_old * XFac);
                    CornerAreaSource[1].Y = Convert.ToInt32(TransformY + (CornerAreaSource[1].Y - transformy_old) / xfac_old * XFac);
                }
                else
                {
                    for (int i = 0; i <= EditVegetation.CornerVegetation; i++)
                    {
                        CornerAreaSource[i].X = Convert.ToInt32(TransformX + (CornerAreaSource[i].X - transformx_old) / xfac_old * XFac);
                        CornerAreaSource[i].Y = Convert.ToInt32(TransformY + (CornerAreaSource[i].Y - transformy_old) / xfac_old * XFac);
                    }
                }
            }

            RubberLineCoors[1].X = Convert.ToInt32(TransformX + (RubberLineCoors[1].X - transformx_old) / xfac_old * XFac);
            RubberLineCoors[1].Y = Convert.ToInt32(TransformY + (RubberLineCoors[1].Y - transformy_old) / xfac_old * XFac);
        }

        /// <summary>
        /// Rotate a copied item 
        /// </summary>
        /// <param name="dir">0: counterclockwise, 1: clockwise</param>
        private void RotateCopiedItem(int dir)
        {
            // rotate an item?
            if (CopiedItem.AreaSource != null)
            {
                if (CopiedItem.AreaSource.Pt.Count < 2)
                {
                    return;
                }
                PointD pt0 = CopiedItem.AreaSource.Pt[0];
                for (int i = 1; i < CopiedItem.AreaSource.Pt.Count; i++)
                {
                    PointD pt = CopiedItem.AreaSource.Pt[i];
                    CopiedItem.AreaSource.Pt[i] = Rotate(dir, pt, pt0);
                }
            }

            if (CopiedItem.Building != null)
            {
                if (CopiedItem.Building.Pt.Count < 2)
                {
                    return;
                }
                PointD pt0 = CopiedItem.Building.Pt[0];
                for (int i = 1; i < CopiedItem.Building.Pt.Count; i++)
                {
                    PointD pt = CopiedItem.Building.Pt[i];
                    CopiedItem.Building.Pt[i] = Rotate(dir, pt, pt0);
                }
            }

            if (CopiedItem.LineSource != null)
            {
                if (CopiedItem.LineSource.Pt.Count < 2)
                {
                    return;
                }
                GralData.PointD_3d pt0 = CopiedItem.LineSource.Pt[0];
                for (int i = 1; i < CopiedItem.LineSource.Pt.Count; i++)
                {
                    GralData.PointD_3d pt = CopiedItem.LineSource.Pt[i];
                    CopiedItem.LineSource.Pt[i] = Rotate(dir, pt, pt0);
                }
            }
            
            if (CopiedItem.PortalSource != null)
            {
                PointD pt0 = CopiedItem.PortalSource.Pt1;
                PointD pt = CopiedItem.PortalSource.Pt2;
                CopiedItem.PortalSource.Pt2 = Rotate(dir, pt, pt0);
            }           
        }

        /// <summary>
        /// Rotate the PointD pt around point pt0 in 10 degree steps
        /// </summary>
        /// <param name="Dir">0: counterclockwise, 1: clockwise</param>
        /// <param name="pt"></param>
        /// <param name="pt0"></param>
        /// <returns>Rotated PointD struct</returns>
        private PointD Rotate(int Dir, PointD pt, PointD pt0)
        {
            PointD temp = new PointD();
            double angleInRadians = 10 * (Math.PI / 180);
            if (Dir == 1)
            {
                angleInRadians = -10 * (Math.PI / 180);
            }

            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);

            temp.X = cosTheta * (pt.X - pt0.X) -
                   sinTheta * (pt.Y - pt0.Y) + pt0.X;
            temp.Y = sinTheta * (pt.X - pt0.X) +
                    cosTheta * (pt.Y - pt0.Y) + pt0.Y;
            return temp;
        }
        /// <summary>
        /// Rotate the PointD_3D pt around point pt0 in 10 degree steps
        /// </summary>
        /// <param name="Dir">0: counterclockwise, 1: clockwise</param>
        /// <param name="pt"></param>
        /// <param name="pt0"></param>
        /// <returns>Rotated PointD struct</returns>
        private GralData.PointD_3d Rotate(int Dir, GralData.PointD_3d pt, GralData.PointD_3d pt0)
        {
            GralData.PointD_3d temp = new GralData.PointD_3d();
            double angleInRadians = 10 * (Math.PI / 180);
            if (Dir == 1)
            {
                angleInRadians = -10 * (Math.PI / 180);
            }

            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);

            temp.X = cosTheta * (pt.X - pt0.X) -
                   sinTheta * (pt.Y - pt0.Y) + pt0.X;
            temp.Y = sinTheta * (pt.X - pt0.X) +
                    cosTheta * (pt.Y - pt0.Y) + pt0.Y;
            return temp;
        }

    }
}
