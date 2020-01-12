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
 * Date: 21.01.2019
 * Time: 17:05
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using GralDomForms;
using GralIO;
using GralItemData;
using GralStaticFunctions;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Left mousekey down events on the picturebox
        /// </summary>
        private void Picturebox1MouseDownLeft(object sender, MouseEventArgs e)
        {
            bool shift_key_pressed = false; // needed for endpoints of GRAMM and GRAL domain when using manual input coordinates
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) // Kuntner: manual coordinate-Input
            {
                using (InputCoordinates inp = new InputCoordinates(textBox1.Text, textBox2.Text))
                {
                    inp.TopMost = true;
                    inp.ShowDialog();
                    textBox1.Text = inp.Input_x.Text;
                    textBox2.Text = inp.Input_y.Text;
                    toolStripTextBox1.Text = textBox1.Text;
                    toolStripTextBox2.Text = textBox2.Text;
                    shift_key_pressed = true;
                }

                //textBox1.Text = Convert.ToString(Math.Round((e.X-transformx) * bmppbx * PixelXSize + WesternBorderX0, 1, MidpointRounding.AwayFromZero));
                //textBox2.Text = Convert.ToString(Math.Round((e.Y-transformy) * bmppbx * PixelYSize + NorthernBorderY0, 1, MidpointRounding.AwayFromZero));
            }

#if __MonoCS__
#else
            //ToolTip for lenght measurement
            if ((MouseControl == 10 || MouseControl == 17 || MouseControl == 8 ||
                 MouseControl == 22 || MouseControl == 23 || MouseControl == 75 || MouseControl == 79) && ShowLenghtLabel)
            {
                ToolTipMousePosition.Active = true; // show tool tip lenght of rubberline segment
                FirstPointLenght.X = (float)St_F.TxtToDbl(textBox1.Text, false);
                FirstPointLenght.Y = (float)St_F.TxtToDbl(textBox2.Text, false);
            }
#endif

            switch (MouseControl)
            {
                case 1:
                    //Zoom in
                    ZoomPlusMinus(1, e);
                    break;

                case -1:
                    //Zoom out
                    ZoomPlusMinus(-1, e);
                    break;

                case 13:
                    //Panel zoom
                    XDomain = e.X;
                    YDomain = e.Y;
                    MouseControl = 14;
                    break;

                case 2:
                    //Move map
                    OldXPosition = e.X;
                    OldYPosition = e.Y;
                    break;

                case 3:
                    //Georeferencing1
                    //Convert PictureBox-Coordinates in Picture-Coordinates
                    GeoReferenceOne.XMouse = (Convert.ToDouble(e.X) - (TransformX + Convert.ToInt32((ItemOptions[0].West - MapSize.West) / BmpScale / MapSize.SizeX))) / ItemOptions[0].PixelMx * MapSize.SizeX / XFac;
                    GeoReferenceOne.YMouse = (Convert.ToDouble(e.Y) - (TransformY - Convert.ToInt32((ItemOptions[0].North - MapSize.North) / BmpScale / MapSize.SizeX))) / ItemOptions[0].PixelMx * MapSize.SizeX / XFac;
                    GeoReferenceOne.Refresh();
                    break;

                case 12:
                    //Georeferencing2
                    //Convert PictureBox-Coordinates in Picture-Coordinates
                    GeoReferenceTwo.XMouse = (Convert.ToDouble(e.X) - (TransformX + Convert.ToInt32((ItemOptions[0].West - MapSize.West) / BmpScale / MapSize.SizeX))) / ItemOptions[0].PixelMx * MapSize.SizeX / XFac;
                    GeoReferenceTwo.YMouse = (Convert.ToDouble(e.Y) - (TransformY - Convert.ToInt32((ItemOptions[0].North - MapSize.North) / BmpScale / MapSize.SizeX))) / ItemOptions[0].PixelMx * MapSize.SizeX / XFac;
                    GeoReferenceTwo.Refresh();
                    break;

                case 5:
                    // set endpoint of GRAL-Domain when using shift-Key
                    // calculate the GRAL-Domain
                    {
                        int xm = Convert.ToInt32((Convert.ToDouble(textBox1.Text.Replace(".", decsep)) - MapSize.West) / (BmpScale * MapSize.SizeX) + TransformX);
                        int ym = Convert.ToInt32((Convert.ToDouble(textBox2.Text.Replace(".", decsep)) - MapSize.North) / (BmpScale * MapSize.SizeY) + TransformY);

                        int x1 = Math.Min(xm, XDomain);
                        int y1 = Math.Min(ym, YDomain);
                        int x2 = Math.Max(xm, XDomain);
                        int y2 = Math.Max(ym, YDomain);
                        int recwidth = x2 - x1;
                        int recheigth = y2 - y1;
                        GRALDomain = new Rectangle(x1, y1, recwidth, recheigth);

                        Picturebox1_MouseUp(null, e); // force button up event
                    }
                    break;

                case 4:
                    //get starting point for drawing GRAL model domain
                    if (Gral.Main.Project_Locked == false)
                    {
                        XDomain = Convert.ToInt32((Convert.ToDouble(textBox1.Text.Replace(".", decsep)) - MapSize.West) / (BmpScale * MapSize.SizeX) + TransformX);
                        YDomain = Convert.ToInt32((Convert.ToDouble(textBox2.Text.Replace(".", decsep)) - MapSize.North) / (BmpScale * MapSize.SizeY) + TransformY);
                        Cursor.Clip = Bounds;
                        MouseControl = 5;
                    }
                    break;

                case 6:
                case 6000:
                    //digitize position of point source
                    if (Gral.Main.Project_Locked == false)
                    {
                        //get x,y coordinates
                        EditPS.SetXCoorText(textBox1.Text);
                        EditPS.SetYCoorText(textBox2.Text);
                        EditPS.SaveArray();
                        if (MouseControl == 6000) // set new position inline editing
                        {
                            EditAndSavePointSourceData(sender, null);
                            InfoBoxCloseAllForms();
                            MouseControl = 700;
                        }

                        Picturebox1_Paint(); // 
                    }
                    break;

                case 8:
                    //digitize position of the corner points of area sources
                    if ((Control.ModifierKeys & Keys.Control) == Keys.Control && EditAS.CorneAareaX.Length > 1) // Kuntner: change one edge-point of area source
                    {
                        // Change one edge of the area source
                        MoveEdgepointArea();
                    }
                    else
                    {
                        if (EditSourceShape == false && Gral.Main.Project_Locked == false)
                        {
                            if (MessageBox.Show(this, "Input new and delete current shape?", "Edit vertices", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                EditSourceShape = true;
                            }

                        }
                        if (EditSourceShape)
                        {
                            //set new area source - get x,y coordinates
                            CornerAreaSource[EditAS.CornerAreaCount] = new Point(e.X, e.Y);
                            EditAS.CorneAareaX[EditAS.CornerAreaCount] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                            EditAS.CornerAreaY[EditAS.CornerAreaCount] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                            EditAS.CornerAreaCount = EditAS.CornerAreaCount + 1;
                            EditAS.SetNumberOfVerticesText(Convert.ToString(EditAS.CornerAreaCount));
                            // Reset Rubber-Line Drawing
                            Cursor.Clip = Bounds;
                            RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                            Picturebox1_Paint(); // 
                        }
                    }
                    break;

                case 17:
                    //digitize position of the corner points of buildings
                    if (Gral.Main.Project_Locked == false)
                    {
                        if ((Control.ModifierKeys & Keys.Control) == Keys.Control && EditB.CornerBuildingX.Length > 1) // Kuntner: change edge point of a building
                        {
                            // Change one edge of the building
                            MoveEdgepointBuilding();
                        }
                        else
                        {
                            if (EditSourceShape == false && Gral.Main.Project_Locked == false)
                            {
                                if (MessageBox.Show(this, "Input new and delete current shape?", "Edit vertices", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                {
                                    EditSourceShape = true;
                                }

                            }
                            if (EditSourceShape)
                            {
                                //set new building - get x,y coordinates
                                CornerAreaSource[EditB.CornerBuilding] = new Point(e.X, e.Y);
                                EditB.CornerBuildingX[EditB.CornerBuilding] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                                EditB.CornerBuildingY[EditB.CornerBuilding] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                                EditB.CornerBuilding++;
                                EditB.SetNumberOfVerticesText(Convert.ToString(EditB.CornerBuilding));
                                // Reset Rubber-Line Drawing
                                Cursor.Clip = Bounds;
                                RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                                Picturebox1_Paint(); // 
                            }
                        }
                    }
                    break;

                case 79:
                    //digitize position of the corner points of forests
                    if ((Control.ModifierKeys & Keys.Control) == Keys.Control && EditAS.CorneAareaX.Length > 1) // Kuntner: change one edge-point of area source
                    {
                        // Change one edge of the area source
                        MoveEdgepointVegetation();
                    }
                    else
                    {
                        if (EditSourceShape == false && Gral.Main.Project_Locked == false)
                        {
                            if (MessageBox.Show(this, "Input new and delete current shape?", "Edit vertices", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                EditSourceShape = true;
                            }

                        }
                        if (EditSourceShape)
                        {
                            //set new area source - get x,y coordinates
                            CornerAreaSource[EditVegetation.CornerVegetation] = new Point(e.X, e.Y);
                            EditVegetation.CornerVegX[EditVegetation.CornerVegetation] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                            EditVegetation.CornerVegY[EditVegetation.CornerVegetation] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                            EditVegetation.CornerVegetation = EditVegetation.CornerVegetation + 1;
                            EditVegetation.SetNumberOfVerticesText(Convert.ToString(EditVegetation.CornerVegetation));
                            // Reset Rubber-Line Drawing
                            Cursor.Clip = Bounds;
                            RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                            Picturebox1_Paint(); // 
                        }
                    }
                    break;

                case 10:
                    //digitize position of the corner points of line sources

                    if ((Control.ModifierKeys & Keys.Control) == Keys.Control && EditLS.CornerLineX.Length > 1) // Kuntner: change point of line source
                    {
                        // Change one edge of the line source
                        MoveEdgepointLine();
                    }
                    else
                    {
                        if (EditSourceShape == false && Gral.Main.Project_Locked == false)
                        {
                            if (MessageBox.Show(this, "Input new and delete current shape?", "Edit vertices", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                EditSourceShape = true;
                            }

                        }
                        if (EditSourceShape)
                        {
                            // set new line-source - get x,y coordinates
                            CornerAreaSource[EditLS.CornerLineSource] = new Point(e.X, e.Y);
                            EditLS.CornerLineX[EditLS.CornerLineSource] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                            EditLS.CornerLineY[EditLS.CornerLineSource] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));

                            EditLS.CornerLineSource = EditLS.CornerLineSource + 1;
                            EditLS.SetNumberOfVerticesText(Convert.ToString(EditLS.CornerLineSource));
                            // Reset Rubber-Line Drawing
                            Cursor.Clip = Bounds;
                            RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                            Picturebox1_Paint(); // 
                        }
                    }
                    break;

                case 75:
                    //digitize position of the corner points of walls
                    if (Gral.Main.Project_Locked == false)
                    {
                        if ((Control.ModifierKeys & Keys.Control) == Keys.Control && EditLS.CornerLineX.Length > 1) // Kuntner: change point of wall
                        {
                            // Change one edge of the line source
                            MoveEdgepointWall();
                        }
                        else
                        {
                            if (EditSourceShape == false && Gral.Main.Project_Locked == false)
                            {
                                if (MessageBox.Show(this, "Input new and delete current shape?", "Edit vertices", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                {
                                    EditSourceShape = true;
                                }
                            }
                            if (EditSourceShape)
                            {
                                // set new wall - get x,y coordinates
                                CornerAreaSource[EditWall.CornerWallCount] = new Point(e.X, e.Y);
                                EditWall.CornerWallX[EditWall.CornerWallCount] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                                EditWall.CornerWallY[EditWall.CornerWallCount] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                                EditWall.CornerWallZ[EditWall.CornerWallCount] = EditWall.GetNumericUpDownHeightValue();
                                if (EditWall.CheckboxAbsHeightChecked()) // absolute height
                                    EditWall.CornerWallZ[EditWall.CornerWallCount] *= -1;

                                EditWall.CornerWallCount = EditWall.CornerWallCount + 1;
                                EditWall.SetNumberOfVerticesText(Convert.ToString(EditWall.CornerWallCount));
                                // Reset Rubber-Line Drawing
                                Cursor.Clip = Bounds;
                                RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                                Picturebox1_Paint(); // 
                            }
                        }
                    }
                    break;

                case 24:
                case 2400:
                    //digitize position of receptors
                    if (Gral.Main.Project_Locked == false)
                    {
                        //get x,y coordinates
                        EditR.SetXCoorText(textBox1.Text);
                        EditR.SetYCoorText(textBox2.Text);
                        EditR.SaveArray();
                        if (MouseControl == 2400) // set new position inline editing
                        {
                            EditAndSaveReceptorData(sender, null);

                            InfoBoxCloseAllForms();
                            MouseControl = 26;
                        }

                        Picturebox1_Paint(); // 
                    }
                    break;

                    // Tooltip for picturebox1

                case 7:
                    //select point sources
                    {
                        int i = 0;
                        bool stop = false;
                        double[] emission = new double[Gral.Main.PollutantList.Count];

                        foreach (PointSourceData _psdata in EditPS.ItemData)
                        {
                            Array.Clear(emission, 0, emission.Length);
                            if (stop) break;

                            int x1 = (int)((_psdata.Pt.X - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                            int y1 = (int)((_psdata.Pt.Y - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;

                            if ((e.X >= x1 - 10) && (e.X <= x1 + 10) && (e.Y >= y1 - 10) && (e.Y <= y1 + 10))
                            {
                                EditPS.SetTrackBar(i + 1);
                                EditPS.ItemDisplayNr = i;
                                SelectedItems.Add(i);
                                EditPS.FillValues();

                                double height = _psdata.Height;

                                // show info in a Tooltip
                                string infotext = "Name: " + _psdata.Name + "\n";
                                if (height >= 0)
                                    infotext += "Stack height [m]: " + height.ToString() + "\n";
                                else
                                    infotext += "Stack height (abs) [m]: " + Math.Abs(height).ToString() + "\n";

                                infotext += "Exit velocity [m/s]: " + _psdata.Velocity.ToString() + "\n";
                                infotext += "Exit temperature [K]: " + _psdata.Temperature.ToString() + "\n";
                                infotext += "Diameter [m]: " + _psdata.Diameter.ToString() + "\n";
                                infotext += "Source group: " + Convert.ToString(_psdata.Poll.SourceGroup) + "\n";
                                for (int r = 0; r < 10; r++)
                                {
                                    try
                                    {
                                        int index = _psdata.Poll.Pollutant[r];
                                        emission[index] = emission[index] + Convert.ToDouble(_psdata.Poll.EmissionRate[r]);
                                    }
                                    catch { }
                                }
                                for (int r = 0; r < Gral.Main.PollutantList.Count; r++)
                                {
                                    if (emission[r] > 0)
                                        infotext += Convert.ToString(Gral.Main.PollutantList[r]) + "[kg/h or MOU/h]: " + Convert.ToString(Math.Round(emission[r], 4)) + "\n";
                                }

                                InfoBoxTip.Show(infotext, this, e.X, e.Y, 8000);

                                stop = true;
                                break;
                            }
                            i = i + 1;
                        }

                        Picturebox1_Paint(); // 
                        Focus();
                    }
                    break;

                case 700:
                    // delete one mouseclick from queue
                    MouseControl = 7;
                    break;

                case 25:
                    //select receptors
                    {
                        int i = 0;
                        bool stop = false;
                        foreach (ReceptorData _rd in EditR.ItemData)
                        {
                            if (stop) break;
                            int x1 = Convert.ToInt32((_rd.Pt.X - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                            int y1 = Convert.ToInt32((_rd.Pt.Y - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                            if ((e.X >= x1 - 10) && (e.X <= x1 + 10) && (e.Y >= y1 - 10) && (e.Y <= y1 + 10))
                            {
                                EditR.SetTrackBar(i + 1);
                                EditR.ItemDisplayNr = i;
                                SelectedItems.Add(i);
                                EditR.FillValues();

                                //Ausgabe der Info in Infobox
                                string infotext = "Name: " + _rd.Name + "\n";
                                infotext += "Receptor height [m]: " + Math.Round(_rd.Height, 1).ToString();
                                InfoBoxTip.Show(infotext, this, e.X, e.Y, 8000);

                                stop = true;
                                break;
                            }
                            i++;
                        }
                        Picturebox1_Paint(); // 
                        Focus();
                    }
                    break;

                case 26:
                    // delete one mouseclick from queue
                    MouseControl = 25;
                    break;

                case 9:
                    //select area sources
                    {
                        int i = 0;
                        bool stop = false;
                        double[] emission = new double[Gral.Main.PollutantList.Count];

                        foreach (AreaSourceData _as in EditAS.ItemData)
                        {
                            Array.Clear(emission, 0, emission.Length);
                            if (stop) break;

                            //filter source group
                            List<Point> poly = new List<Point>();

                            foreach (DrawingObjects _drobj in ItemOptions)
                            {
                                poly.Clear();

                                if (_drobj.Name == "AREA SOURCES")
                                {
                                    if ((_drobj.SourceGroup == _as.Poll.SourceGroup) || (_drobj.SourceGroup == -1))
                                    {
                                        poly.Clear();
                                        List<PointD> _points = _as.Pt;
                                        int x1 = 0;
                                        int y1 = 0;
                                        for (int j = 0; j < _points.Count; j++)
                                        {
                                            x1 = Convert.ToInt32((_points[j].X - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                                            y1 = Convert.ToInt32((_points[j].Y - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                                            poly.Add(new Point(x1, y1));
                                        }

                                        if (St_F.PointInPolygon(new Point(e.X, e.Y), poly))
                                        {
                                            EditAS.SetTrackBar(i + 1);
                                            EditAS.ItemDisplayNr = i;
                                            SelectedItems.Add(i);
                                            EditAS.FillValues();

                                            double height = _as.Height;

                                            //Ausgabe der Info in Infobox

                                            string infotext = "Name: " + _as.Name + "\n";
                                            if (height >= 0)
                                                infotext += "Mean height [m]: " + height.ToString() + "\n";
                                            else
                                                infotext += "Mean height (abs) [m]: " + Math.Abs(height).ToString() + "\n";

                                            infotext += "Vertical extension [m]: " + _as.VerticalExt.ToString() + "\n";
                                            infotext += @"Area [m" + Gral.Main.SquareString + "]: " + Math.Round(_as.Area, 1).ToString() + "\n";
                                            infotext += "Source group: " + _as.Poll.SourceGroup + "\n";
                                            for (int r = 0; r < 10; r++)
                                            {
                                                try
                                                {
                                                    int index = _as.Poll.Pollutant[r];
                                                    emission[index] = emission[index] + _as.Poll.EmissionRate[r];
                                                }
                                                catch { }
                                            }
                                            for (int r = 0; r < Gral.Main.PollutantList.Count; r++)
                                            {
                                                if (emission[r] > 0)
                                                    infotext += Convert.ToString(Gral.Main.PollutantList[r]) + "[kg/h or OU/h]: " + Convert.ToString(Math.Round(emission[r], 4)) + "\n";
                                            }
                                            InfoBoxTip.Show(infotext, this, e.X, e.Y, 8000);

                                        }
                                    }
                                    i = i + 1;
                                }
                            }
                        }
                        Picturebox1_Paint(); // 
                        Focus();
                    }
                    break;

                case 77:
                    //select vegetation
                    {
                        int i = 0;
                        bool stop = false;
                        int x1 = 0;
                        int y1 = 0;
                        List<Point> poly = new List<Point>();

                        foreach (VegetationData _vdata in EditVegetation.ItemData)
                        {
                            if (stop) break;
                            poly.Clear();
                            List<PointD> _points = _vdata.Pt;
                            for (int j = 0; j < _points.Count; j++)
                            {
                                x1 = Convert.ToInt32((_points[j].X - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                                y1 = Convert.ToInt32((_points[j].Y - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                                poly.Add(new Point(x1, y1));
                            }

                            if (St_F.PointInPolygon(new Point(e.X, e.Y), poly))
                            {
                                EditVegetation.SetTrackBar(i + 1);
                                EditVegetation.ItemDisplayNr = i;
                                SelectedItems.Add(i);
                                EditVegetation.FillValues();
                                double height = _vdata.VerticalExt;

                                //Ausgabe der Info in Infobox
                                string infotext = "Name: " + _vdata.Name + "\n";
                                infotext += "Height [m]: " + Math.Abs(height) + "\n";
                                infotext += @"Area [m" + Gral.Main.SquareString + "]: " + _vdata.Area + "\n";
                                InfoBoxTip.Show(infotext, this, e.X, e.Y, 8000);
                                stop = true;
                                break;
                            }
                            i = i + 1;
                        }
                        Picturebox1_Paint(); // 
                        Focus();
                    }
                    break;

                case 19:
                    //select buildings
                    {
                        int i = 0;
                        bool stop = false;
                        int x1 = 0;
                        int y1 = 0;
                        List<Point> poly = new List<Point>();
                        foreach (BuildingData _bd in EditB.ItemData)
                        {
                            if (stop) break;
                            List<PointD> _pt = _bd.Pt;
                            poly.Clear();
                            for (int j = 0; j < _pt.Count; j++)
                            {
                                x1 = Convert.ToInt32((_pt[j].X - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                                y1 = Convert.ToInt32((_pt[j].Y - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                                poly.Add(new Point(x1, y1));
                            }

                            if (St_F.PointInPolygon(new Point(e.X, e.Y), poly))
                            {
                                EditB.SetTrackBar(i + 1);
                                EditB.ItemDisplayNr = i;
                                SelectedItems.Add(i);
                                EditB.FillValues();
                                double height = _bd.Height;

                                //Ausgabe der Info in Infobox
                                string infotext = "Name: " + _bd.Name + "\n";
                                if (height >= 0)
                                    infotext += "Height [m]: " + _bd.Height.ToString() + "\n";
                                else
                                    infotext += "Height (abs) [m]: " + St_F.DblToIvarTxt(Math.Abs(height)) + "\n";

                                infotext += "Lower bound [m]: " + _bd.LowerBound + "\n";
                                infotext += @"Area [m" + Gral.Main.SquareString + "]: " + Math.Round(_bd.Area, 1).ToString() + "\n";
                                InfoBoxTip.Show(infotext, this, e.X, e.Y, 8000);
                                stop = true;
                                break;
                            }
                            i = i + 1;
                        }
                        Picturebox1_Paint(); // 
                        Focus();
                    }
                    break;

                case 11:
                    //select line sources
                    {
                        int i = 0;
                        bool stop = false;
                        double[] emission = new double[Gral.Main.PollutantList.Count];
                        List<Point> poly = new List<Point>();
                        foreach (LineSourceData _ls in EditLS.ItemData)
                        {
                            Array.Clear(emission, 0, emission.Length);
                            if (stop) break;
                            poly.Clear();
                            //Point[] poly = new Point[4];

                            for (int j = 0; j < _ls.Pt.Count - 1; j++)
                            {
                                double x1 = (_ls.Pt[j].X - MapSize.West) / BmpScale / MapSize.SizeX + TransformX;
                                double y1 = (_ls.Pt[j].Y - MapSize.North) / BmpScale / MapSize.SizeY + TransformY;
                                double x2 = (_ls.Pt[j + 1].X - MapSize.West) / BmpScale / MapSize.SizeX + TransformX;
                                double y2 = (_ls.Pt[j + 1].Y - MapSize.North) / BmpScale / MapSize.SizeY + TransformY;

                                double length = Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
                                if (length == 0)
                                    length = 0.1;

                                double cosalpha = (x2 - x1) / length;
                                double sinalpha = (y1 - y2) / length;
                                double dx = Math.Max(_ls.Width / 2 / BmpScale / MapSize.SizeX, 1) * sinalpha;
                                double dy = Math.Max(_ls.Width / 2 / BmpScale / MapSize.SizeX, 1) * cosalpha;
                                poly.Add(new Point(Convert.ToInt32(x1 + dx), Convert.ToInt32(y1 + dy)));
                                poly.Add(new Point(Convert.ToInt32(x1 - dx), Convert.ToInt32(y1 - dy)));
                                poly.Add(new Point(Convert.ToInt32(x2 - dx), Convert.ToInt32(y2 - dy)));
                                poly.Add(new Point(Convert.ToInt32(x2 + dx), Convert.ToInt32(y2 + dy)));

                                if (St_F.PointInPolygon(new Point(e.X, e.Y), poly))
                                {
                                    EditLS.SetTrackBar(i + 1);
                                    EditLS.ItemDisplayNr = i;
                                    SelectedItems.Add(i);
                                    EditLS.FillValues();
                                    stop = true;

                                    //Ausgabe der Info in Infobox
                                    string infotext = "Name: " + _ls.Name + "\n";
                                    if (_ls.Height >= 0)
                                    {
                                        infotext += "Height (rel): " + Math.Abs(_ls.Height).ToString() + "\n";
                                    }
                                    else
                                    {
                                        infotext += "Height (abs): " + Math.Abs(_ls.Height).ToString() + "\n";
                                    }
                                    infotext += "Vert. extension: " + _ls.VerticalExt.ToString() + "\n";
                                    infotext += "Width: " + _ls.Width.ToString() + "\n";

                                    infotext += "Veh/Day: " + _ls.Nemo.AvDailyTraffic.ToString() + "\n";
                                    infotext += "Heavy Duty Veh [%]: " + _ls.Nemo.ShareHDV.ToString() + "\n";
                                    infotext += "Slope [%]: " + _ls.Nemo.Slope.ToString() + "\n";
                                    infotext += "Reference Year: " + _ls.Nemo.BaseYear.ToString() + "\n";

                                    length = St_F.CalcLenght(_ls.Pt);

                                    infotext += "Length [km]: " + Convert.ToString(Math.Round(length, 1)) + "\n";
                                    infotext += "Traffic Situation: " + EditLS.GetSelectedListBox1Item() + "\n";

                                    foreach (PollutantsData _poll in _ls.Poll)
                                    {
                                        for (int r = 0; r < 10; r++)
                                        {
                                            int index = Convert.ToInt32(_poll.Pollutant[r]);
                                            try
                                            {
                                                emission[index] += _poll.EmissionRate[r];
                                            }
                                            catch { }
                                        }
                                    }
                                    for (int r = 0; r < Gral.Main.PollutantList.Count; r++)
                                    {
                                        if (emission[r] > 0)
                                            infotext += Convert.ToString(Gral.Main.PollutantList[r]) + "[kg/h/km]: " + Convert.ToString(Math.Round(emission[r], 4)) + "\n";
                                    }
                                    InfoBoxTip.Show(infotext, this, e.X, e.Y, 8000);

                                    break;
                                }
                            }
                            i = i + 1;
                        }
                        Picturebox1_Paint(); // 
                        Focus();
                    }
                    break;

                case 16:
                    //select portal sources
                    {
                        int i = 0;
                        bool stop = false;
                        double[] emission = new double[Gral.Main.PollutantList.Count];

                        foreach (PortalsData _po in EditPortals.ItemData)
                        {
                            if (stop) break;

                            int sourcegroups = _po.Poll.Count;
                            double x1 = (_po.Pt1.X - MapSize.West) / BmpScale / MapSize.SizeX + TransformX;
                            double y1 = (_po.Pt1.Y - MapSize.North) / BmpScale / MapSize.SizeY + TransformY;
                            double x2 = (_po.Pt2.X - MapSize.West) / BmpScale / MapSize.SizeX + TransformX;
                            double y2 = (_po.Pt2.Y - MapSize.North) / BmpScale / MapSize.SizeY + TransformY;

                            int xmean = Convert.ToInt32((x1 + x2) * 0.5);
                            int ymean = Convert.ToInt32((y1 + y2) * 0.5);
                            if ((e.X >= xmean - 10) && (e.X <= xmean + 10) && (e.Y >= ymean - 10) && (e.Y <= ymean + 10))
                            {
                                EditPortals.SetTrackBar(i + 1);
                                EditPortals.ItemDisplayNr = i;
                                SelectedItems.Add(i);
                                EditPortals.FillValues();

                                //Ausgabe der Info in Infobox
                                string infotext = "Name: " + _po.Name + "\n";
                                if (_po.BaseHeight >= 0)
                                {
                                    infotext += "Base height (rel): " + Math.Abs(_po.BaseHeight).ToString() + "\n";
                                }
                                else
                                {
                                    infotext += "Base height (abs): " + Math.Abs(_po.BaseHeight).ToString() + "\n";
                                }
                                infotext += "Height [m]: " + _po.Height.ToString() + "\n";
                                infotext += "Section [m�]: " + _po.Crosssection.ToString() + "\n";
                                if (_po.Direction.Contains("1"))
                                {
                                    infotext += "Bidirectional \n";
                                }
                                else
                                {
                                    infotext += "Unidirectional \n";
                                }

                                foreach (PollutantsData _poll in _po.Poll)
                                {
                                    for (int r = 0; r < 10; r++)
                                    {
                                        int index = Convert.ToInt32(_poll.Pollutant[r]);
                                        try
                                        {
                                            emission[index] += _poll.EmissionRate[r];
                                        }
                                        catch { }
                                    }
                                }
                                for (int r = 0; r < Gral.Main.PollutantList.Count; r++)
                                {
                                    if (emission[r] > 0)
                                        infotext += Convert.ToString(Gral.Main.PollutantList[r]) + "[kg/h]: " + Convert.ToString(Math.Round(emission[r], 4)) + "\n";
                                }
                                InfoBoxTip.Show(infotext, this, e.X, e.Y, 8000);

                                stop = true;
                                break;
                            }
                            i = i + 1;
                        }
                        Picturebox1_Paint(); // 
                        Focus();
                    }
                    break;

                case 76:
                    //select walls
                    {
                        int i = 0;
                        bool stop = false;

                        List<Point> poly = new List<Point>();
                        foreach (WallData _wd in EditWall.ItemData)
                        {
                            if (stop) break;
                            poly.Clear();

                            for (int j = 0; j < _wd.Pt.Count - 1; j++)
                            {
                                double x1 = (_wd.Pt[j].X - MapSize.West) / BmpScale / MapSize.SizeX + TransformX;
                                double y1 = (_wd.Pt[j].Y - MapSize.North) / BmpScale / MapSize.SizeY + TransformY;
                                double x2 = (_wd.Pt[j + 1].X - MapSize.West) / BmpScale / MapSize.SizeX + TransformX;
                                double y2 = (_wd.Pt[j + 1].Y - MapSize.North) / BmpScale / MapSize.SizeY + TransformY;

                                double length = Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
                                if (length == 0)
                                    length = 0.1;

                                double cosalpha = (x2 - x1) / length;
                                double sinalpha = (y1 - y2) / length;
                                double dx = Math.Max(Convert.ToDouble(MainForm.numericUpDown10.Value) / 2 / BmpScale / MapSize.SizeX, 1) * sinalpha;
                                double dy = Math.Max(Convert.ToDouble(MainForm.numericUpDown10.Value) / 2 / BmpScale / MapSize.SizeX, 1) * cosalpha;
                                poly.Add(new Point(Convert.ToInt32(x1 + dx), Convert.ToInt32(y1 + dy)));
                                poly.Add(new Point(Convert.ToInt32(x1 - dx), Convert.ToInt32(y1 - dy)));
                                poly.Add(new Point(Convert.ToInt32(x2 - dx), Convert.ToInt32(y2 - dy)));
                                poly.Add(new Point(Convert.ToInt32(x2 + dx), Convert.ToInt32(y2 + dy)));

                                if (St_F.PointInPolygon(new Point(e.X, e.Y), poly))
                                {
                                    EditWall.SetTrackBar(i + 1);
                                    EditWall.ItemDisplayNr = i;
                                    SelectedItems.Add(i);
                                    EditWall.FillValues();
                                    stop = true;

                                    //Ausgabe der Info in Infobox
                                    InfoBoxTip.Show("Name: " + _wd.Name, this, e.X, e.Y, 8000);

                                    break;
                                }
                            }
                            i = i + 1;
                        }
                        Picturebox1_Paint(); // 
                        Focus();
                    }
                    break; ;

                case 20:
                    //digitize position of north arrow
                    {
                        //get x,y coordinates
                        NorthArrow.X = e.X;
                        NorthArrow.Y = e.Y;

                        bool exist = false;
                        foreach (DrawingObjects _drobj in ItemOptions)
                            if (_drobj.Name == "NORTH ARROW")
                            {
                                exist = true;
                                _drobj.ContourLabelDist = (int)(NorthArrow.Scale * 100);
                                break;
                            }
                        if (exist == false)
                        {
                            DrawingObjects _drobj = new DrawingObjects("NORTH ARROW")
                            {
                                Label = 0,
                                LabelFont = new Font("Arial", 12),
                                ContourLabelDist = (int)(NorthArrow.Scale * 100)
                            };
                            ItemOptions.Insert(0, _drobj);
                        }

                        SaveDomainSettings(1);
                        Picturebox1_Paint(); // 
                    }
                    break;

                case 21:
                    //digitize position of map scale bar
                    {
                        //get x,y coordinates
                        MapScale.X = e.X;
                        MapScale.Y = e.Y;
                        bool exist = false;
                        foreach (DrawingObjects _drobj in ItemOptions)
                        {
                            if (_drobj.Name.Equals("SCALE BAR"))
                            {
                                exist = true;
                                _drobj.ContourLabelDist = MapScale.Length;
                                break;
                            }
                        }

                        if (exist == false)
                        {
                            DrawingObjects _drobj = new DrawingObjects("SCALE BAR")
                            {
                                Label = 2,
                                ContourLabelDist = MapScale.Length
                            };
                            ItemOptions.Insert(0, _drobj);
                        }

                        SaveDomainSettings(1);
                        Picturebox1_Paint(); // 
                    }
                    break;

                case 15:
                    //digitize position of portal source
                    if (Gral.Main.Project_Locked == false)
                    {
                        //get x,y coordinates
                        //get x,y coordinates
                        EditPortals.CornerPortalX[0] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditPortals.CornerPortalY[0] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));

                        CornerAreaSource[EditLS.CornerLineSource] = new Point(e.X, e.Y);
                        EditLS.CornerLineSource = EditLS.CornerLineSource + 1;
                        Graphics g = picturebox1.CreateGraphics();
                        if (EditLS.CornerLineSource > 1)
                        {
                            Pen p = new Pen(Color.LightBlue, 3);
                            g.DrawLine(p, CornerAreaSource[EditLS.CornerLineSource - 2], CornerAreaSource[EditLS.CornerLineSource - 1]);
                            p.Dispose();
                        }
                        Cursor.Clip = Bounds;
                        Picturebox1_Paint(); // 
                    }
                    break;

                case 22:
                    //measuring tool "distance"
                    {
                        //get x,y coordinates
                        CornerAreaSource[EditLS.CornerLineSource] = new Point(e.X, e.Y);
                        EditLS.CornerLineX[EditLS.CornerLineSource] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditLS.CornerLineY[EditLS.CornerLineSource] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                        EditLS.CornerLineSource = EditLS.CornerLineSource + 1;
                        // Reset Rubber-Line Drawing
                        Cursor.Clip = Bounds;
                        RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                        Picturebox1_Paint(); // 
                    }
                    break;

                case 44:
                case 45:
                    // select section for windfiled section drawing
                    {
                        //get x,y coordinates
                        if (EditLS.CornerLineSource == 0)
                        {
                            CornerAreaSource[EditLS.CornerLineSource] = new Point(e.X, e.Y);
                        }
                        EditLS.CornerLineX[EditLS.CornerLineSource] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditLS.CornerLineY[EditLS.CornerLineSource] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                        EditLS.CornerLineSource = 1;
                        // Reset Rubber-Line Drawing
                        Cursor.Clip = Bounds;
                        RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                        Picturebox1_Paint(); // 
                    }
                    break;

                case 23:
                    //measuring tool "area"
                    {
                        //get x,y coordinates
                        CornerAreaSource[EditAS.CornerAreaCount] = new Point(e.X, e.Y);
                        EditAS.CorneAareaX[EditAS.CornerAreaCount] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditAS.CornerAreaY[EditAS.CornerAreaCount] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                        EditAS.CornerAreaCount = EditAS.CornerAreaCount + 1;
                        // Reset Rubber-Line Drawing
                        Cursor.Clip = Bounds;
                        RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                        Picturebox1_Paint(); // 
                    }
                    break;

                case 28:
                    //position of legend
                    {
                        if (ActualEditedDrawingObject != null)
                        {
                            string[] dummy = new string[3];
                            dummy = ActualEditedDrawingObject.ColorScale.Split(new char[] { ',' });
                            ActualEditedDrawingObject.ColorScale = Convert.ToString(e.X) + "," + Convert.ToString(e.Y) + "," + dummy[2];
                            Picturebox1_Paint();
                        }
                    }
                    break;

                case 31:
                    // set endpoint of GRAMM model domain with shift button
                    if (shift_key_pressed)
                    {
                        // calculate the GRAMM-Domain
                        int xm = Convert.ToInt32((Convert.ToDouble(textBox1.Text.Replace(".", decsep)) - MapSize.West) / (BmpScale * MapSize.SizeX) + TransformX);
                        int ym = Convert.ToInt32((Convert.ToDouble(textBox2.Text.Replace(".", decsep)) - MapSize.North) / (BmpScale * MapSize.SizeY) + TransformY);

                        int x1 = Math.Min(xm, XDomain);
                        int y1 = Math.Min(ym, YDomain);
                        int x2 = Math.Max(xm, XDomain);
                        int y2 = Math.Max(ym, YDomain);
                        int recwidth = x2 - x1;
                        int recheigth = y2 - y1;
                        GRAMMDomain = new Rectangle(x1, y1, recwidth, recheigth);

                        Picturebox1_MouseUp(null, e); // force button up event
                    }
                    break;

                case 301:
                    // set endpoint for exporting GRAMM sub-domain with shift button
                    if (shift_key_pressed)
                    {
                        // calculate the GRAMM-Subdomain
                        int xm = Convert.ToInt32((Convert.ToDouble(textBox1.Text.Replace(".", decsep)) - MapSize.West) / (BmpScale * MapSize.SizeX) + TransformX);
                        int ym = Convert.ToInt32((Convert.ToDouble(textBox2.Text.Replace(".", decsep)) - MapSize.North) / (BmpScale * MapSize.SizeY) + TransformY);

                        int x1 = Math.Min(xm, XDomain);
                        int y1 = Math.Min(ym, YDomain);
                        int x2 = Math.Max(xm, XDomain);
                        int y2 = Math.Max(ym, YDomain);
                        int recwidth = x2 - x1;
                        int recheigth = y2 - y1;
                        GRAMMDomain = new Rectangle(x1, y1, recwidth, recheigth);

                        Picturebox1_MouseUp(null, e); // force button up event
                    }
                    break;

                case 30:
                    //get starting point for drawing GRAMM model domain
                    {
                        XDomain = Convert.ToInt32((Convert.ToDouble(textBox1.Text.Replace(".", decsep)) - MapSize.West) / (BmpScale * MapSize.SizeX) + TransformX);
                        YDomain = Convert.ToInt32((Convert.ToDouble(textBox2.Text.Replace(".", decsep)) - MapSize.North) / (BmpScale * MapSize.SizeY) + TransformY);
                        //							xdomain = e.X;
                        //							ydomain = e.Y;
                        MouseControl = 31;
                        Cursor.Clip = Bounds;
                    }
                    break;

                case 300:
                    //get starting point for exporting GRAMM sub-domain
                    {
                        XDomain = Convert.ToInt32((Convert.ToDouble(textBox1.Text.Replace(".", decsep)) - MapSize.West) / (BmpScale * MapSize.SizeX) + TransformX);
                        YDomain = Convert.ToInt32((Convert.ToDouble(textBox2.Text.Replace(".", decsep)) - MapSize.North) / (BmpScale * MapSize.SizeY) + TransformY);
                        GRAMMDomain = new Rectangle(XDomain, YDomain, 0, 0);
                        MouseControl = 301;
                        Cursor.Clip = Bounds;
                    }
                    break;

                case 32:
                    //get sample point for computing meteorological time series from GRAMM windfield
                    {
                        XDomain = Convert.ToInt32(Convert.ToDouble(textBox1.Text.Replace(".", decsep)));
                        YDomain = Convert.ToInt32(Convert.ToDouble(textBox2.Text.Replace(".", decsep)));
                        if ((XDomain < MainForm.GrammDomRect.West) || (XDomain > MainForm.GrammDomRect.East) || (YDomain < MainForm.GrammDomRect.South) || (YDomain > MainForm.GrammDomRect.North))
                        {
                            MessageBox.Show(this, "Point is outside GRAMM domain", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        MeteoDialog.X_Coor.Text = XDomain.ToString();
                        MeteoDialog.Y_Coor.Text = YDomain.ToString();
                    }
                    break;

                case 65:
                    //get sample point for re-ordering GRAMM windfield to meet observed wind data better
                    {
                        XDomain = Convert.ToInt32(Convert.ToDouble(textBox1.Text.Replace(".", decsep)));
                        YDomain = Convert.ToInt32(Convert.ToDouble(textBox2.Text.Replace(".", decsep)));
                        if ((XDomain < MainForm.GrammDomRect.West) || (XDomain > MainForm.GrammDomRect.East) || (YDomain < MainForm.GrammDomRect.South) || (YDomain > MainForm.GrammDomRect.North))
                        {
                            MessageBox.Show(this, "Point is outside GRAMM domain", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MouseControl = 0;
                            Cursor = Cursors.Default;
                            ReorderGrammWindfields();
                        }
                    }
                    break;

                case 66:
                    //get sample point for re-ordering GRAMM windfield to meet newly observed wind and stability data at any location within the model domain
                    {
                        XDomain = Convert.ToInt32(Convert.ToDouble(textBox1.Text.Replace(".", decsep)));
                        YDomain = Convert.ToInt32(Convert.ToDouble(textBox2.Text.Replace(".", decsep)));
                        if ((XDomain < MainForm.GrammDomRect.West) || (XDomain > MainForm.GrammDomRect.East) || (YDomain < MainForm.GrammDomRect.South) || (YDomain > MainForm.GrammDomRect.North))
                        {
                            MessageBox.Show(this, "Point is outside GRAMM domain", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            //set new coordinates manually
                            if (MMO.dataGridView1.CurrentCell != null) // Kuntner: check if line does exist!
                            {
                                int zeilenindex = MMO.dataGridView1.CurrentCell.RowIndex;
                                MMO.dataGridView1.Rows[zeilenindex].Cells[1].Value = Convert.ToInt32(XDomain);
                                MMO.dataGridView1.Rows[zeilenindex].Cells[2].Value = Convert.ToInt32(YDomain);
                            }

                        }
                    }
                    break;

                case 35:
                    //get sample point for computing source apportionment
                    {
                        XDomain = Convert.ToInt32(Convert.ToDouble(textBox1.Text.Replace(".", decsep)));
                        YDomain = Convert.ToInt32(Convert.ToDouble(textBox2.Text.Replace(".", decsep)));
                        MouseControl = 0;
                        Cursor = Cursors.Default;
                        SourceApportionment(XDomain, YDomain);
                    }
                    break;

                case 50:
                    //get sample point to get a concentration value 
                    {
                        XDomain = Convert.ToInt32(Convert.ToDouble(textBox1.Text.Replace(".", decsep)));
                        YDomain = Convert.ToInt32(Convert.ToDouble(textBox2.Text.Replace(".", decsep)));
                        //mousecontrol = 0;
                        GetConcentrationFromFile(ConcFilename);
                    }
                    break;

                case 40:
                    //get sample point for vertical profile for GRAMM online evaluations
                    {
                        MouseControl = 0;
                        Cursor = Cursors.Default;
                        XDomain = Convert.ToInt32(Convert.ToDouble(textBox1.Text.Replace(".", decsep)));
                        YDomain = Convert.ToInt32(Convert.ToDouble(textBox2.Text.Replace(".", decsep)));
                        VertProfile();
                    }
                    break;

                case 200:
                    //get sample point for vertical 3D profile of GRAL concentrations
                    {
                        XDomain = Convert.ToInt32(Convert.ToDouble(textBox1.Text.Replace(".", decsep)));
                        YDomain = Convert.ToInt32(Convert.ToDouble(textBox2.Text.Replace(".", decsep)));
                        Vert3DConcentration();
                    }
                    break;

                case 62:
                    //get sample point for vertical profile for GRAMM windfields
                    {
                        XDomain = Convert.ToInt32(Convert.ToDouble(textBox1.Text.Replace(".", decsep)));
                        YDomain = Convert.ToInt32(Convert.ToDouble(textBox2.Text.Replace(".", decsep)));
                        VertProfile2();
                    }
                    break;

                case 70:
                    // check single value at GRAMM grid
                    {
                        int sel = 0;
                        foreach (DrawingObjects _drobj in ItemOptions)
                        {
                            if (_drobj.ContourFilename.EndsWith(".scl"))
                            {
                                sel = Convert.ToInt32(Path.GetFileNameWithoutExtension(_drobj.ContourFilename)); // get the number of this file
                                if (sel > 0)
                                    break; // if first file found
                            }
                        }

                        ReadSclUstOblClasses reader = new ReadSclUstOblClasses
                        {
                            FileName = Path.Combine(Gral.Main.ProjectName, @"Computation", Convert.ToString(sel).PadLeft(5, '0') + ".scl")
                        };
                        double[,] arr = new double[1, 1];
                        int x1 = 1;
                        int y1 = 1;
                        XDomain = Convert.ToInt32(Convert.ToDouble(textBox1.Text.Replace(".", decsep)));
                        YDomain = Convert.ToInt32(Convert.ToDouble(textBox2.Text.Replace(".", decsep)));

                        if (MainForm.textBox13.Text != "")
                        {
                            x1 = Convert.ToInt32(Math.Floor((XDomain - MainForm.GrammDomRect.West) / MainForm.GRAMMHorGridSize));
                            y1 = Convert.ToInt32(Math.Floor((YDomain - MainForm.GrammDomRect.South) / MainForm.GRAMMHorGridSize));
                        }
                        else
                        {
                            x1 = Convert.ToInt32(Math.Floor((XDomain - Convert.ToInt32(MainForm.textBox6.Text)) / Convert.ToDouble(MainForm.numericUpDown10.Value)));
                            y1 = Convert.ToInt32(Math.Floor((YDomain - Convert.ToInt32(MainForm.textBox5.Text)) / Convert.ToDouble(MainForm.numericUpDown10.Value)));
                        }
                        //MessageBox.Show(this, Convert.ToString(x1) +"/" + Convert.ToString(y1));

                        int result = 0;
                        if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                            result = reader.ReadSclMean(x1, y1);
                        else
                            result = reader.ReadSclFile(x1, y1); // true => reader = OK

                        if (result > 0)
                        {
                            MessageBox.Show(this, "Stability: " + Convert.ToString(result), "GRAL GUI - Stability class", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        reader.close();
                    }
                    break;

                case 1000:
                    //final corner point of changed line source point
                    SetNewEdgepointLine();
                    break;

                case 1001:
                    //final corner point of changed wall
                    SetNewEdgepointWall();
                    break;

                case 1170:
                    //final corner point of changed building edge point
                    SetNewEdgepointBuilding();
                    break;

                case 108:
                case 1080:
                    //final corner point of changed area source point
                    SetNewEdgepointArea();
                    break;

                case 109:
                case 1081:
                    //final corner point of changed vegetation
                    SetNewEdgepointVegetation();
                    break;
            }
        }
    }
}