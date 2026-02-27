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
using GralDomForms;
using GralItemData;
using GralStaticFunctions;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Right mousekey down events on the picturebox
        /// </summary>
        private void Picturebox1MouseDownRight(object sender, MouseEventArgs e)
        {
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
                }

            }

            // paste an item?
            if (CopiedItem.PointSource != null)
            {
                ContextMenuStrip m = new ContextMenuStrip();
                ToolStripMenuItem mi = new ToolStripMenuItem();
                mi.Text = "Paste Point Source";
                mi.Click += RightClickPointSourcePaste;
                m.Items.Add(mi);
                m.Show(picturebox1, new Point(e.X, e.Y));
                return;
            }
            if (CopiedItem.AreaSource != null)
            {
                ContextMenuStrip m = new ContextMenuStrip();
                ToolStripMenuItem mi = new ToolStripMenuItem();
                mi.Text = "Paste Area Source";
                mi.Click += RightClickAreaSourcePaste;
                m.Items.Add(mi);
                m.Show(picturebox1, new Point(e.X, e.Y));
                return;
            }
            if (CopiedItem.Building != null)
            {
                ContextMenuStrip m = new ContextMenuStrip();
                ToolStripMenuItem mi = new ToolStripMenuItem();
                mi.Text = "Paste Building";
                mi.Click += RightClickBuildingPaste;
                m.Items.Add(mi);
                m.Show(picturebox1, new Point(e.X, e.Y));
                return;
            }
            if (CopiedItem.LineSource != null)
            {
                ContextMenuStrip m = new ContextMenuStrip();
                ToolStripMenuItem mi = new ToolStripMenuItem();
                mi.Text = "Paste Line Source";
                mi.Click += RightClickLineSourcePaste;
                m.Items.Add(mi);
                m.Show(picturebox1, new Point(e.X, e.Y));
                return;
            }
            if (CopiedItem.Receptor != null)
            {
                ContextMenuStrip m = new ContextMenuStrip();
                ToolStripMenuItem mi = new ToolStripMenuItem();
                mi.Text = "Paste Receptor Point";
                mi.Click += RightClickReceptorPaste;
                m.Items.Add(mi);
                m.Show(picturebox1, new Point(e.X, e.Y));
                return;
            }
            if (CopiedItem.PortalSource != null)
            {
                ContextMenuStrip m = new ContextMenuStrip();
                ToolStripMenuItem mi = new ToolStripMenuItem();
                mi.Text = "Paste Portal Source";
                mi.Click += RightClickPortalPaste;
                m.Items.Add(mi);
                m.Show(picturebox1, new Point(e.X, e.Y));
                return;
            }


            // Tooltip for picturebox1
            ToolTipMousePosition.Active = false; // don't show tool tip lenght of rubberline segments anymore

            switch (MouseControl)
            {
                case MouseMode.PointSourceSel:
                    // Edit Point Source - Right Mouse
                    {
                        int i = RightClickSearchPointSource(e); // get number of point source at mouse position

                        if (i >= 0 && i < EditPS.ItemData.Count)
                        {
                            EditPS.SetTrackBar(i + 1);
                            EditPS.ItemDisplayNr = i;
                            EditPS.FillValues();

                            Picturebox1_Paint();

                            ContextMenuStrip m = new ContextMenuStrip();
                            ToolStripMenuItem mi = new ToolStripMenuItem();
                            mi.Text = "Edit " + EditPS.ItemData[i].Name.Substring(0, Math.Min(10, EditPS.ItemData[i].Name.Length));
                            ToolStripMenuItem it = new ToolStripMenuItem("Edit Point Source");
                            it.Tag = i;
                            it.Click += RightClickPSEdit;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Move Point Source");
                            it.Tag = i;
                            it.Click += RightClickPSMove;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Delete Point Source");
                            it.Tag = i;
                            it.Click += RightClickPSDelete;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Copy Point Source");
                            it.Tag = i;
                            it.Click += RightClickPSCopy;
                            mi.DropDownItems.Add(it);

                            if (Gral.Main.Project_Locked == true)
                            {
                                mi.DropDownItems[0].Text = "Show Point Source Data";
                                mi.DropDownItems[1].Enabled = false;
                                mi.DropDownItems[2].Enabled = false;
                                mi.DropDownItems[3].Enabled = false;
                            }
                            m.Items.Add(mi);
                            m.Show(picturebox1, new Point(e.X, e.Y));
                        }
                    }
                    break;

                case MouseMode.ReceptorSel:
                    //select receptors - Right Mouse
                    {
                        int i = RightClickSearchReceptor(e);
                        if (i >= 0 && i < EditR.ItemData.Count)
                        {
                            EditR.SetTrackBar(i + 1);
                            EditR.ItemDisplayNr = i;
                            EditR.FillValues();

                            Picturebox1_Paint();

                            ContextMenuStrip m = new ContextMenuStrip();
                            ToolStripMenuItem mi = new ToolStripMenuItem();
                            mi.Text = "Edit " + EditR.ItemData[i].Name.Substring(0, Math.Min(10, EditR.ItemData[i].Name.Length));
                            ToolStripMenuItem it = new ToolStripMenuItem("Edit Receptor");
                            it.Tag = i;
                            it.Click += RightClickReceptorEdit;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Move Receptor");
                            it.Tag = i;
                            it.Click += RightClickReceptorMove;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Delete Receptor");
                            it.Tag = i;
                            it.Click += RightClickReceptorDelete;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Copy Receptor");
                            it.Tag = i;
                            it.Click += RightClickReceptorCopy;
                            mi.DropDownItems.Add(it);

                            if (Gral.Main.Project_Locked == true)
                            {
                                mi.DropDownItems[0].Text = "Show Receptor Point Data";
                                mi.DropDownItems[1].Enabled = false;
                                mi.DropDownItems[2].Enabled = false;
                                mi.DropDownItems[3].Enabled = false;
                            }
                            m.Items.Add(mi);
                            m.Show(picturebox1, new Point(e.X, e.Y));
                        }
                    }
                    break;

                case MouseMode.AreaSourceSel:
                    //select area sources - Right Mouse
                    {
                        int i = RightClickSearchAreaSource(e);

                        if (i >= 0 && i < EditAS.ItemData.Count)
                        {
                            ContextMenuStrip m = new ContextMenuStrip();
                            ToolStripMenuItem mi = new ToolStripMenuItem();
                            mi.Text = "Edit " + EditAS.ItemData[i].Name.Substring(0, Math.Min(10, EditAS.ItemData[i].Name.Length));
                            ToolStripMenuItem it = new ToolStripMenuItem("Edit Area Source");
                            it.Tag = i;
                            it.Click += RightClickAreaEdit;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Move Edge Point");
                            it.Tag = new PointD(textBox1.Text,
                                                            textBox2.Text,
                                                            CultureInfo.CurrentCulture);
                            it.Click += RightClickAreaMoveEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Add Edge Point");
                            it.Tag = new SelectedPointNumber(textBox1.Text, textBox2.Text, i, CultureInfo.CurrentCulture);
                            it.Click += RightClickAreaAddEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Delete Edge Point");
                            it.Tag = new SelectedPointNumber(textBox1.Text, textBox2.Text, i, CultureInfo.CurrentCulture);
                            it.Click += RightClickAreaDelEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Delete Area Source");
                            it.Tag = i;
                            it.Click += RightClickAreaDelete;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Copy Area Source");
                            it.Tag = i;
                            it.Click += RightClickAreaCopy;
                            mi.DropDownItems.Add(it);

                            if (Gral.Main.Project_Locked == true)
                            {
                                mi.DropDownItems[0].Text = "Show Area Source Data";
                                mi.DropDownItems[1].Enabled = false;
                                mi.DropDownItems[2].Enabled = false;
                                mi.DropDownItems[3].Enabled = false;
                                mi.DropDownItems[4].Enabled = false;
                                mi.DropDownItems[5].Enabled = false;
                            }
                            if (EditAS.ItemData[i].Pt.Count < 4) // at least 3 edge points
                            {
                                mi.DropDownItems[3].Enabled = false;
                            }
                            EditAS.SetTrackBar(i + 1);
                            EditAS.ItemDisplayNr = i;
                            EditAS.FillValues();
                            m.Items.Add(mi);
                            m.Show(picturebox1, new Point(e.X, e.Y));
                        }
                    }
                    break;

                case MouseMode.LineSourceSel:
                    //select line sources - Right Mouse
                    {
                        int i = RightClickSearchLineSource(e);

                        if (i >= 0 && i < EditLS.ItemData.Count)
                        {
                            ContextMenuStrip m = new ContextMenuStrip();
                            ToolStripMenuItem mi = new ToolStripMenuItem();
                            mi.Text = "Edit " + EditLS.ItemData[i].Name.Substring(0, Math.Min(10, EditLS.ItemData[i].Name.Length));
                            ToolStripMenuItem it = new ToolStripMenuItem("Edit Line Source");
                            it.Tag = i;
                            it.Click += RightClickLineEdit;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Move Edge Point");
                            it.Tag = new PointD(textBox1.Text,
                                                            textBox2.Text,
                                                            CultureInfo.CurrentCulture);
                            it.Click += RightClickLineMoveEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Add Edge Point");
                            mi.Tag = new PointD(textBox1.Text,
                                                            textBox2.Text,
                                                            CultureInfo.CurrentCulture);
                            it.Click += RightClickLineAddEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Delete Edge Point");
                            it.Tag = new SelectedPointNumber(textBox1.Text,
                                                                         textBox2.Text,
                                                                         i,
                                                                         CultureInfo.CurrentCulture);
                            it.Click += RightClickLineDelEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Delete Line Source");
                            it.Tag = i;
                            it.Click += RightClickLineDelete;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Copy Line Source");
                            it.Tag = i;
                            it.Click += RightClickLineCopy;
                            mi.DropDownItems.Add(it);

                            if (Gral.Main.Project_Locked == true)
                            {
                                mi.DropDownItems[0].Text = "Show Line Source Data";
                                mi.DropDownItems[1].Enabled = false;
                                mi.DropDownItems[2].Enabled = false;
                                mi.DropDownItems[3].Enabled = false;
                                mi.DropDownItems[4].Enabled = false;
                                mi.DropDownItems[5].Enabled = false;
                            }
                            if (EditLS.ItemData[i].Pt.Count < 3) // at least 2 edge points
                            {
                                mi.DropDownItems[3].Enabled = false;
                            }

                            EditLS.SetTrackBar(i + 1);
                            EditLS.ItemDisplayNr = i;
                            EditLS.FillValues();

                            m.Items.Add(mi);
                            m.Show(picturebox1, new Point(e.X, e.Y));
                        }
                    }
                    break;

                case MouseMode.WallSel:

                    //select wall - Right Mouse
                    {
                        int i = RightClickSearchWalls(e);

                        if (i >= 0 && i < EditWall.ItemData.Count)
                        {
                            ContextMenuStrip m = new ContextMenuStrip();
                            ToolStripMenuItem mi = new ToolStripMenuItem();
                            mi.Text = "Edit " + EditWall.ItemData[i].Name.Substring(0, Math.Min(10, EditWall.ItemData[i].Name.Length));
                            ToolStripMenuItem it = new ToolStripMenuItem("Edit Wall");
                            it.Tag = i;
                            it.Click += RightClickWallEdit;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Move Edge Point");
                            it.Tag = new PointD(textBox1.Text,
                                                            textBox2.Text,
                                                            CultureInfo.CurrentCulture);
                            it.Click += RightClickWallMoveEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Add Edge Point");
                            mi.Tag = new PointD(textBox1.Text,
                                                            textBox2.Text,
                                                            CultureInfo.CurrentCulture);
                            it.Click += RightClickWallAddEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Delete Edge Point");
                            it.Tag = new SelectedPointNumber(textBox1.Text,
                                                                         textBox2.Text,
                                                                         i,
                                                                         CultureInfo.CurrentCulture);
                            it.Click += RightClickWallDelEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Delete Wall");
                            it.Tag = i;
                            it.Click += RightClickWallDelete;
                            mi.DropDownItems.Add(it);

                            if (Gral.Main.Project_Locked == true)
                            {
                                mi.DropDownItems[0].Text = "Show Line Source Data";
                                mi.DropDownItems[1].Enabled = false;
                                mi.DropDownItems[2].Enabled = false;
                                mi.DropDownItems[3].Enabled = false;
                                mi.DropDownItems[4].Enabled = false;
                            }
                            if (EditWall.ItemData[i].Pt.Count < 3) // at least 2 edge points
                            {
                                mi.DropDownItems[3].Enabled = false;
                            }

                            EditWall.SetTrackBar(i + 1);
                            EditWall.ItemDisplayNr = i;
                            EditWall.FillValues();

                            m.Items.Add(mi);
                            m.Show(picturebox1, new Point(e.X, e.Y));
                        }
                    }
                    break;

                case MouseMode.PortalSourceSel:
                    //select portal sources Right Mouse
                    {
                        int i = 0;
                        bool stop = false;
                        foreach (PortalsData _po in EditPortals.ItemData)
                        {
                            int sourcegroups = _po.Poll.Count;
                            double x1 = (_po.Pt1.X - MapSize.West) / BmpScale / MapSize.SizeX + TransformX;
                            double y1 = (_po.Pt1.Y - MapSize.North) / BmpScale / MapSize.SizeY + TransformY;
                            double x2 = (_po.Pt2.X - MapSize.West) / BmpScale / MapSize.SizeX + TransformX;
                            double y2 = (_po.Pt2.Y - MapSize.North) / BmpScale / MapSize.SizeY + TransformY;

                            int xmean = (int)((x1 + x2) * 0.5);
                            int ymean = (int)((y1 + y2) * 0.5);
                            if ((e.X >= xmean - 10) && (e.X <= xmean + 10) && (e.Y >= ymean - 10) && (e.Y <= ymean + 10))
                            {

                                ContextMenuStrip m = new ContextMenuStrip();
                                ToolStripMenuItem mi = new ToolStripMenuItem();
                                mi.Text = "Edit " + EditPortals.ItemData[i].Name.Substring(0, Math.Min(10, EditPortals.ItemData[i].Name.Length));
                                ToolStripMenuItem it = new ToolStripMenuItem("Edit Portal Source");
                                it.Tag = i;
                                it.Click += RightClickPortalEdit;
                                mi.DropDownItems.Add(it);
                                it = new ToolStripMenuItem("Delete Portal Source");
                                it.Tag = i;
                                it.Click += RightClickPortalDelete;
                                mi.DropDownItems.Add(it);
                                it = new ToolStripMenuItem("Flip Exit Surface");
                                it.Tag = i;
                                it.Click += RightClickPortalFlip;
                                mi.DropDownItems.Add(it);
                                it = new ToolStripMenuItem("Copy Portal Source");
                                it.Tag = i;
                                it.Click += RightClickPortalCopy;
                                mi.DropDownItems.Add(it);

                                if (Gral.Main.Project_Locked == true)
                                {
                                    mi.DropDownItems[0].Text = "Show Portal Source Data";
                                    mi.DropDownItems[1].Enabled = false;
                                    mi.DropDownItems[2].Enabled = false;
                                    mi.DropDownItems[3].Enabled = false;
                                }
                                m.Items.Add(mi);
                                m.Show(picturebox1, new Point(e.X, e.Y));

                                stop = true;
                            }
                            i += 1;
                            if (stop)
                            {
                                break;
                            }
                        }
                    }
                    break;

                case MouseMode.BuildingSel:

                    //select buildings  Right Mouse
                    {
                        int i = RightClickSearchBuildings(e);

                        if (i >= 0 && i < EditB.ItemData.Count)
                        {
                            ContextMenuStrip m = new ContextMenuStrip();
                            ToolStripMenuItem mi = new ToolStripMenuItem();
                            mi.Text = "Edit " + EditB.ItemData[i].Name.Substring(0, Math.Min(10, EditB.ItemData[i].Name.Length));
                            ToolStripMenuItem it = new ToolStripMenuItem("Edit Building");
                            it.Tag = i;
                            it.Click += RightClickBuildingEdit;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Move Edge Point");
                            it.Tag = new PointD(textBox1.Text,
                                                            textBox2.Text,
                                                            CultureInfo.CurrentCulture);
                            it.Click += RightClickBuildingMoveEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Add Edge Point");
                            it.Tag = new SelectedPointNumber(textBox1.Text, textBox2.Text, i, CultureInfo.CurrentCulture);
                            it.Click += RightClickAreaAddEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Delete Edge Point");
                            it.Tag = new SelectedPointNumber(textBox1.Text, textBox2.Text, i, CultureInfo.CurrentCulture);
                            it.Click += RightClickBuildingDelEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Delete Building");
                            it.Tag = i;
                            it.Click += RightClickBuildingDelete;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Copy Building");
                            it.Tag = i;
                            it.Click += RightClickBuildingCopy;
                            mi.DropDownItems.Add(it);

                            if (Gral.Main.Project_Locked == true)
                            {
                                mi.DropDownItems[0].Text = "Show Building Data";
                                mi.DropDownItems[1].Enabled = false;
                                mi.DropDownItems[2].Enabled = false;
                                mi.DropDownItems[3].Enabled = false;
                                mi.DropDownItems[4].Enabled = false;
                                mi.DropDownItems[5].Enabled = false;
                            }
                            if (EditB.ItemData[i].Pt.Count < 4) // at least 3 edge points
                            {
                                mi.DropDownItems[3].Enabled = false;
                            }
                            EditB.SetTrackBar(i + 1);
                            EditB.ItemDisplayNr = i;
                            EditB.FillValues();
                            m.Items.Add(mi);
                            m.Show(picturebox1, new Point(e.X, e.Y));
                        }
                    }
                    break;

                case MouseMode.VegetationSel:
                    //select vegetation  Right Mouse
                    {
                        int i = RightClickSearchVegetation(e);

                        if (i >= 0 && i < EditVegetation.ItemData.Count)
                        {
                            ContextMenuStrip m = new ContextMenuStrip();
                            ToolStripMenuItem mi = new ToolStripMenuItem();
                            mi.Text = "Edit " + EditVegetation.ItemData[i].Name.Substring(0, Math.Min(10, EditVegetation.ItemData[i].Name.Length));
                            ToolStripMenuItem it = new ToolStripMenuItem("Edit Vegetation");
                            it.Tag = i;
                            it.Click += RightClickVegetationEdit;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Move Edge Point");
                            it.Tag = new PointD(textBox1.Text,
                                                            textBox2.Text,
                                                            CultureInfo.CurrentCulture);
                            it.Click += RightClickVegetationMoveEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Add Edge Point");
                            it.Tag = new SelectedPointNumber(textBox1.Text, textBox2.Text, i, CultureInfo.CurrentCulture);
                            it.Click += RightClickVegetationAddEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Delete Edge Point");
                            it.Tag = new SelectedPointNumber(textBox1.Text, textBox2.Text, i, CultureInfo.CurrentCulture);
                            it.Click += RightClickVegetationDelEdge;
                            mi.DropDownItems.Add(it);
                            it = new ToolStripMenuItem("Delete Vegetation Area");
                            it.Tag = i;
                            it.Click += RightClickVegetationDelete;
                            mi.DropDownItems.Add(it);

                            if (Gral.Main.Project_Locked == true)
                            {
                                mi.DropDownItems[0].Text = "Show Area Source Data";
                                mi.DropDownItems[1].Enabled = false;
                                mi.DropDownItems[2].Enabled = false;
                                mi.DropDownItems[3].Enabled = false;
                                mi.DropDownItems[4].Enabled = false;
                            }
                            if (EditVegetation.ItemData[i].Pt.Count < 4) // at least 3 edge points
                            {
                                mi.DropDownItems[3].Enabled = false;
                            }
                            EditVegetation.SetTrackBar(i + 1);
                            EditVegetation.ItemDisplayNr = i;
                            EditVegetation.FillValues();

                            m.Items.Add(mi);
                            m.Show(picturebox1, new Point(e.X, e.Y));
                        }
                    }
                    break;

                case MouseMode.AreaSourcePos:
                    //final corner point of area source
                    if (EditAS.CornerAreaCount > 1)
                    {
                        //get x,y coordinates
                        CornerAreaSource[EditAS.CornerAreaCount] = new Point(e.X, e.Y);
                        EditAS.CornerAreaX[EditAS.CornerAreaCount] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditAS.CornerAreaY[EditAS.CornerAreaCount] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                        EditAS.SetNumberOfVerticesText(Convert.ToString(EditAS.CornerAreaCount + 1));
                        PointD[] mypoint = new PointD[1000];
                        for (int i = 0; i < EditAS.CornerAreaCount + 1; i++)
                        {
                            mypoint[i] = new PointD(EditAS.CornerAreaX[i], EditAS.CornerAreaY[i]);
                        }

                        double areapolygon = St_F.CalcArea(EditAS.CornerAreaCount + 1, mypoint);
                        EditAS.SetRasterSize(Convert.ToDecimal(Math.Max(Math.Round(Math.Sqrt(areapolygon / 25), 1), 0.5)));
                        EditAS.SaveArray(false);
                        for (int i = 0; i <= EditAS.CornerAreaCount; i++)
                        {
                            CornerAreaSource[i] = new Point();
                        }
                        EditAS.CornerAreaCount = 0;
                        // Reset Rubber-Line Drawing
                        Cursor.Clip = Rectangle.Empty;
                        RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                        Picturebox1_Paint();
                    }
                    break;

                case MouseMode.VegetationPosCorner:
                    //final corner point of vegetation
                    if (EditVegetation.CornerVegetation > 0)
                    {
                        //get x,y coordinates
                        CornerAreaSource[EditVegetation.CornerVegetation] = new Point(e.X, e.Y);
                        EditVegetation.CornerVegX[EditVegetation.CornerVegetation] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditVegetation.CornerVegY[EditVegetation.CornerVegetation] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));

                        EditVegetation.SetNumberOfVerticesText(Convert.ToString(EditVegetation.CornerVegetation + 1));
                        EditVegetation.SaveArray(false);
                        for (int i = 0; i <= EditVegetation.CornerVegetation; i++)
                        {
                            CornerAreaSource[i] = new Point();
                        }
                        EditVegetation.CornerVegetation = 0;
                        // Reset Rubber-Line Drawing
                        Cursor.Clip = Rectangle.Empty;
                        RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                        Picturebox1_Paint();
                    }
                    break;

                case MouseMode.BuildingPos:
                    //final corner point of buildings
                    if (EditB.CornerBuilding > 1)
                    {
                        //get x,y coordinates
                        CornerAreaSource[EditB.CornerBuilding] = new Point(e.X, e.Y);
                        EditB.CornerBuildingX[EditB.CornerBuilding] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditB.CornerBuildingY[EditB.CornerBuilding] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                        EditB.SetNumberOfVerticesText(Convert.ToString(EditB.CornerBuilding + 1));
                        PointD[] mypoint = new PointD[1000];
                        for (int i = 0; i < EditB.CornerBuilding + 1; i++)
                        {
                            mypoint[i] = new PointD(EditB.CornerBuildingX[i], EditB.CornerBuildingY[i]);
                        }

                        EditB.SaveArray(false);
                        for (int i = 0; i <= EditB.CornerBuilding; i++)
                        {
                            CornerAreaSource[i] = new Point();
                        }
                        EditB.CornerBuilding = 0;
                        // Reset Rubber-Line Drawing
                        Cursor.Clip = Rectangle.Empty;
                        RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                        Picturebox1_Paint();
                    }
                    break;

                case MouseMode.BuildingEditFinal:
                    // case 1170 //
                    //final corner point of changed building edge point
                    SetNewEdgepointBuilding();
                    break;

                case MouseMode.LineSourcePos:
                    //final corner point of line source
                    if (EditLS.CornerLineSource > 0)
                    {
                        //get x,y coordinates
                        CornerAreaSource[EditLS.CornerLineSource] = new Point(e.X, e.Y);
                        EditLS.CornerLineX[EditLS.CornerLineSource] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditLS.CornerLineY[EditLS.CornerLineSource] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                        EditLS.SetNumberOfVerticesText(Convert.ToString(EditLS.CornerLineSource + 1));
                        EditLS.SaveArray(false);
                        for (int i = 0; i <= EditLS.CornerLineSource; i++)
                        {
                            CornerAreaSource[i] = new Point();
                        }
                        EditLS.CornerLineSource = 0;
                        // Reset Rubber-Line Drawing
                        Cursor.Clip = Rectangle.Empty;
                        RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                        Picturebox1_Paint();
                    }
                    break;

                case MouseMode.LineSourceEditFinal:
                    /* case 1000: */
                    //final corner point of changed line source point
                    SetNewEdgepointLine();
                    break;

                case MouseMode.WallSet:
                    //final corner point of wall
                    if (EditWall.CornerWallCount > 0)
                    {
                        //get x,y coordinates
                        CornerAreaSource[EditWall.CornerWallCount] = new Point(e.X, e.Y);
                        EditWall.CornerWallX[EditWall.CornerWallCount] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditWall.CornerWallY[EditWall.CornerWallCount] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                        EditWall.CornerWallZ[EditWall.CornerWallCount] = EditWall.GetNumericUpDownHeightValue();
                        if (EditWall.CheckboxAbsHeightChecked()) // absolute height
                        {
                            EditWall.CornerWallZ[EditWall.CornerWallCount] *= -1;
                        }

                        EditWall.SetNumberOfVerticesText(Convert.ToString(EditWall.CornerWallCount + 1));
                        EditWall.SaveArray(false);
                        for (int i = 0; i <= EditWall.CornerWallCount; i++)
                        {
                            CornerAreaSource[i] = new Point();
                        }
                        EditWall.CornerWallCount = 0;
                        // Reset Rubber-Line Drawing
                        Cursor.Clip = Rectangle.Empty;
                        RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                        Picturebox1_Paint();
                    }
                    break;

                case MouseMode.WallEditFinal:
                    /*case 1001:*/
                    //final corner point of changed wall edge
                    SetNewEdgepointWall();
                    break;

                case MouseMode.PortalSourcePos:
                    //second point of a portal source
                    if (Gral.Main.Project_Locked == false)
                    {
                        //get x,y coordinates
                        EditPortals.CornerPortalX[1] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditPortals.CornerPortalY[1] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                        EditPortals.SaveArray(false);
                        //abstand der eckpunkte und fläche berechnen
                        try
                        {
                            double abstand = Math.Sqrt(Math.Pow((EditPortals.CornerPortalX[1] - EditPortals.CornerPortalX[0]), 2) + Math.Pow((EditPortals.CornerPortalY[1] - EditPortals.CornerPortalY[0]), 2));
                            EditPortals.SetCrossSectionText(Convert.ToString(Math.Round(EditPortals.GetNumericUpDownHeightValue() * abstand)));
                        }
                        catch { }
                        Cursor.Clip = Rectangle.Empty;
                        Picturebox1_Paint();
                    }

                    //digitizing tunnel portals
                    if (EditLS.CornerLineSource > 0)
                    {
                        Pen p = new Pen(Color.LightBlue, 3);
                        //get x,y coordinates
                        CornerAreaSource[EditLS.CornerLineSource] = new Point(e.X, e.Y);
                        Graphics g = picturebox1.CreateGraphics();
                        g.DrawLine(p, CornerAreaSource[EditLS.CornerLineSource - 1], CornerAreaSource[EditLS.CornerLineSource]);
                        for (int i = 0; i <= EditLS.CornerLineSource; i++)
                        {
                            CornerAreaSource[i] = new Point();
                        }
                        EditLS.CornerLineSource = 0;
                        Cursor.Clip = Rectangle.Empty;
                        Picturebox1_Paint();
                        p.Dispose();
                    }
                    break;

                case MouseMode.ViewDistanceMeasurement:
                    //measuring tool "distance"
                    if (EditLS.CornerLineSource > 0)
                    {
                        Pen p = new Pen(Color.LightBlue, 3);
                        //get x,y coordinates
                        CornerAreaSource[EditLS.CornerLineSource] = new Point(e.X, e.Y);
                        EditLS.CornerLineX[EditLS.CornerLineSource] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditLS.CornerLineY[EditLS.CornerLineSource] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                        Graphics g = picturebox1.CreateGraphics();
                        g.DrawLine(p, CornerAreaSource[EditLS.CornerLineSource - 1], CornerAreaSource[EditLS.CornerLineSource]);
                        double DistanceMeasurement = 0;
                        for (int i = 0; i < EditLS.CornerLineSource; i++)
                        {
                            DistanceMeasurement += Math.Sqrt(Math.Pow(EditLS.CornerLineX[i + 1] - EditLS.CornerLineX[i], 2) +
                                                  Math.Pow(EditLS.CornerLineY[i + 1] - EditLS.CornerLineY[i], 2));
                        }
                        for (int i = 0; i <= EditLS.CornerLineSource; i++)
                        {
                            CornerAreaSource[i] = new Point();
                        }
                        EditLS.CornerLineSource = 0;

                        MessageBox.Show(this, "Distance [m]: " + Convert.ToString(Math.Round(DistanceMeasurement, 1)), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.None);

                        DistanceMeasurement = 0;

                        // Reset Rubber-Line Drawing
                        Cursor.Clip = Rectangle.Empty;
                        RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                        Picturebox1_Paint();
                        p.Dispose();
                    }
                    break;

                case MouseMode.SectionWindSel:
                    //Section drawing
                    if (EditLS.CornerLineSource > 0)
                    {
                        Pen p = new Pen(Color.LightBlue, 3);
                        //get x,y coordinates
                        CornerAreaSource[EditLS.CornerLineSource] = new Point(e.X, e.Y);
                        EditLS.CornerLineX[EditLS.CornerLineSource] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditLS.CornerLineY[EditLS.CornerLineSource] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                        Graphics g = picturebox1.CreateGraphics();
                        g.DrawLine(p, CornerAreaSource[EditLS.CornerLineSource - 1], CornerAreaSource[EditLS.CornerLineSource]);
                        p.Dispose();

                        // to draw the section
                        sectionpoints.Clear();
                        sectionpoints.Add(new Point(Convert.ToInt32(EditLS.CornerLineX[0]), Convert.ToInt32(EditLS.CornerLineY[0])));
                        sectionpoints.Add(new Point(Convert.ToInt32(EditLS.CornerLineX[1]), Convert.ToInt32(EditLS.CornerLineY[1])));

                        WindfieldSectionDrawings windfield_data = new WindfieldSectionDrawings
                        {
                            GralWest = MainForm.GralDomRect.West,
                            GralSouth = MainForm.GralDomRect.South,
                            cellsize = (double)MainForm.numericUpDown10.Value,
                            GrammWest = MainForm.GrammDomRect.West,
                            GrammSouth = MainForm.GrammDomRect.South,
                            GrammCellsize = MainForm.GRAMMHorGridSize,
                            WindSectorSize = 360 / Convert.ToDouble(MainForm.numericUpDown1.Value),
                            Nii = Convert.ToInt32(Math.Abs(MainForm.GralDomRect.East - MainForm.GralDomRect.West) / MainForm.HorGridSize),
                            Njj = Convert.ToInt32(Math.Abs(MainForm.GralDomRect.North - MainForm.GralDomRect.South) / MainForm.HorGridSize),
                            X0 = EditLS.CornerLineX[0],
                            X1 = EditLS.CornerLineX[1],
                            Y0 = EditLS.CornerLineY[0],
                            Y1 = EditLS.CornerLineY[1],
                            Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                            PROJECT_path = Gral.Main.ProjectName
                        };


                        string ggeom_path;
                        if (Gral.Main.GRAMMwindfield != null)
                        {
                            ggeom_path = Path.Combine(Path.GetDirectoryName(Gral.Main.GRAMMwindfield), "ggeom.asc");
                        }
                        else
                        {
                            ggeom_path = Path.Combine(Gral.Main.ProjectName, "Computation", "ggeom.asc");
                        }

                        windfield_data.GRAMM_path = ggeom_path;

                        //						#if __MonoCS__
                        //							MessageBox.Show(this, "This function is not available at LINUX yet");
                        //						#else
                        // show Section-Form
                        Sectiondrawing section = new Sectiondrawing(windfield_data);
                        section.Form_Section_Closed += new Section_Closed(section_Form_Section_Closed);
                        section.StartPosition = FormStartPosition.Manual;
                        section.Left = St_F.GetScreenAtMousePosition() + 20;
                        section.Top = St_F.GetTopScreenAtMousePosition() + 150;
                        section.Show();
                        //						#endif

                        // Reset Rubber-Line Drawing
                        Cursor.Clip = Rectangle.Empty;
                        RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                        EditLS.CornerLineSource = 0;
                        Picturebox1_Paint();
                    }
                    break;

                case MouseMode.SectionConcSel:
                    // Vertical concentration section
                    if (EditLS.CornerLineSource > 0)
                    {
                        Pen p = new Pen(Color.LightBlue, 3);
                        //get x,y coordinates
                        CornerAreaSource[EditLS.CornerLineSource] = new Point(e.X, e.Y);
                        EditLS.CornerLineX[EditLS.CornerLineSource] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditLS.CornerLineY[EditLS.CornerLineSource] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));
                        Graphics g = picturebox1.CreateGraphics();
                        g.DrawLine(p, CornerAreaSource[EditLS.CornerLineSource - 1], CornerAreaSource[EditLS.CornerLineSource]);
                        p.Dispose();

                        // Evaluate the section data
                        Evaluate3DConcentrations(EditLS.CornerLineX[EditLS.CornerLineSource - 1], EditLS.CornerLineY[EditLS.CornerLineSource - 1],
                                                   EditLS.CornerLineX[EditLS.CornerLineSource], EditLS.CornerLineY[EditLS.CornerLineSource]);

                        // to draw the section
                        sectionpoints.Clear();
                        sectionpoints.Add(new Point(Convert.ToInt32(EditLS.CornerLineX[0]), Convert.ToInt32(EditLS.CornerLineY[0])));
                        sectionpoints.Add(new Point(Convert.ToInt32(EditLS.CornerLineX[1]), Convert.ToInt32(EditLS.CornerLineY[1])));

                        // Reset Rubber-Line Drawing
                        Cursor.Clip = Rectangle.Empty;
                        RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                        EditLS.CornerLineSource = 0;
                        Picturebox1_Paint();
                    }
                    break;

                case MouseMode.ViewAreaMeasurement:
                    //measuring tool "area"
                    if (EditAS.CornerAreaCount > 1)
                    {
                        //get x,y coordinates
                        CornerAreaSource[EditAS.CornerAreaCount] = new Point(e.X, e.Y);
                        EditAS.CornerAreaX[EditAS.CornerAreaCount] = Convert.ToDouble(textBox1.Text.Replace(".", decsep));
                        EditAS.CornerAreaY[EditAS.CornerAreaCount] = Convert.ToDouble(textBox2.Text.Replace(".", decsep));

                        PointD[] _mypoint = new PointD[EditAS.CornerAreaCount + 2];
                        for (int i = 0; i < EditAS.CornerAreaCount + 2; i++)
                        {
                            _mypoint[i] = new PointD(EditAS.CornerAreaX[i], EditAS.CornerAreaY[i]);
                        }
                        double areapolygon = St_F.CalcArea(EditAS.CornerAreaCount, _mypoint);

                        PointF[] mypoint = new PointF[EditAS.CornerAreaCount];
                        for (int i = 0; i < EditAS.CornerAreaCount; i++)
                        {
                            mypoint[i] = new Point(CornerAreaSource[i].X, CornerAreaSource[i].Y);
                        }
                        Graphics g = picturebox1.CreateGraphics();
                        g.FillPolygon(new SolidBrush(Color2Transparent(200, Color.Green)), mypoint);

                        MessageBox.Show(this, @"Area [m" + Gral.Main.SquareString + "]: " + Convert.ToString(areapolygon), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.None);
                        for (int i = 0; i <= EditAS.CornerAreaCount; i++)
                        {
                            CornerAreaSource[i] = new Point();
                        }
                        EditAS.CornerAreaCount = 0;
                        // Reset Rubber-Line Drawing
                        Cursor.Clip = Rectangle.Empty;
                        RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;
                        Picturebox1_Paint();
                    }
                    break;
            }
        }
    }
}
