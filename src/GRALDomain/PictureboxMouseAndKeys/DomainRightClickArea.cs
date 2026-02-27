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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Search an item next to the mouse position and if more than 1 item is found, show a selection dialog
        /// </summary>
        /// <returns>-1: no item at mouse position or number of item</returns>
        private int RightClickSearchAreaSource(MouseEventArgs e)
        {
            int i = 0;

            // search items
            List<string> ItemNames = new List<string>();
            List<int> ItemNumber = new List<int>();
            double fx = 1 / BmpScale / MapSize.SizeX;
            double fy = 1 / BmpScale / MapSize.SizeY;
            Point MousePosition = new Point(e.X, e.Y);

            foreach (AreaSourceData _as in EditAS.ItemData)
            {
                List<Point> poly = new List<Point>();
                poly.Clear();

                List<PointD> _pt = _as.Pt;
                int x1 = 0;
                int y1 = 0;
                for (int j = 0; j < _pt.Count; j++)
                {
                    x1 = (int)((_pt[j].X - MapSize.West) * fx) + TransformX;
                    y1 = (int)((_pt[j].Y - MapSize.North) * fy) + TransformY;
                    poly.Add(new Point(x1, y1));
                }

                if (St_F.PointInPolygon(MousePosition, poly))
                {
                    ItemNames.Add(_as.Name);
                    ItemNumber.Add(i);
                }

                i++;
            }

            return SelectOverlappedItem(e, ItemNames, ItemNumber);
        }

        /// <summary>
        /// Starts the ItemSelect Dialog of overlapped items if needed
        /// </summary>
        /// <returns>-1: no item at mouse position or number of item</returns>
        private int SelectOverlappedItem(MouseEventArgs e, List<string> ItemNames, List<int> ItemNumber)
        {
            if (ItemNames.Count == 0)
            {
                return -1;
            }
            else if (ItemNames.Count == 1)
            {
                return ItemNumber[0];
            }
            else
            {
                SelectItem selectitem = new SelectItem()
                {
                    ItemNames = ItemNames,
                    StartPosition = FormStartPosition.Manual,
                    Left = e.X + this.Left,
                    Top = e.Y,
                    Owner = this
                };
                selectitem.ShowDialog();
                int i = selectitem.SelectedIndex;
                selectitem.Dispose();
                if (i >= 0 && i < ItemNumber.Count)
                {
                    return ItemNumber[i];
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Paste an area source
        /// </summary>
        private void RightClickAreaSourcePaste(object sender, System.EventArgs e)
        {
            if (CopiedItem.AreaSource != null && CopiedItem.AreaSource.Pt.Count > 1)
            {
                double x = Convert.ToDouble(textBox1.Text);
                double y = Convert.ToDouble(textBox2.Text);
                double x0 = CopiedItem.AreaSource.Pt[0].X;
                double y0 = CopiedItem.AreaSource.Pt[0].Y;

                for (int i = 0; i < CopiedItem.AreaSource.Pt.Count; i++)
                {
                    CopiedItem.AreaSource.Pt[i] = new PointD(x - x0 + CopiedItem.AreaSource.Pt[i].X, y - y0 + CopiedItem.AreaSource.Pt[i].Y);
                }
                AreaSourceData Temp = new AreaSourceData(CopiedItem.AreaSource);
                Temp.Name += "-Copy";
                EditAS.ItemData.Add(Temp);
                EditAS.SetTrackBarMaximum();
                CopiedItem.AreaSource = null;
                EditAndSaveAreaSourceData(null, null); // save changes
                Picturebox1_Paint();
                MouseControl = MouseMode.BuildingSel;
                ContextMenuStrip m = sender as ContextMenuStrip;
                if (m != null)
                {
                    m.Dispose();
                }
            }
        }


        /// <summary>
        /// ContextMenu Edit Area Source
        /// </summary>
        private void RightClickAreaEdit(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi != null)
            {
                int i = Convert.ToInt32(mi.Tag);
                EditAS.SetTrackBar(i + 1);
                EditAS.ItemDisplayNr = i;
                EditAS.FillValues();
                AreaSourcesToolStripMenuItemClick(null, null);
            }
        }
        /// <summary>
		/// ContextMenu Move Edge Point Area Source
		/// </summary>
		private void RightClickAreaMoveEdge(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi != null)
            {
                PointD coor = (PointD)mi.Tag;
                textBox1.Text = coor.X.ToString();
                textBox2.Text = coor.Y.ToString();
                MoveEdgepointArea();
                MouseControl = MouseMode.AreaInlineEdit;
            }
        }
        /// <summary>
		/// ContextMenu Add Edge Point Area Source
		/// </summary>
		private void RightClickAreaAddEdge(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi != null)
            {
                SelectedPointNumber pt = (SelectedPointNumber)(mi.Tag);
                int i = pt.Index;

                EditAS.SetTrackBar(i + 1);
                EditAS.ItemDisplayNr = i;
                EditAS.FillValues();

                int j = 0;
                int indexmin = 0;
                double min = 100000000;
                foreach (PointD _pt in EditAS.ItemData[i].Pt)
                {
                    double dx = pt.X - _pt.X;
                    double dy = pt.Y - _pt.Y;
                    if (Math.Sqrt(dx * dx + dy * dy) < min) // search min
                    {
                        min = Math.Sqrt(dx * dx + dy * dy);
                        indexmin = j;
                    }
                    j++;
                }
                if (indexmin < EditAS.ItemData[i].Pt.Count)
                {
                    int indexnext = indexmin + 1;
                    if (indexnext >= EditAS.ItemData[i].Pt.Count)
                    {
                        indexnext = 0;
                    }
                    EditAS.ItemData[i].Pt.Insert(indexmin + 1, GetPointBetween(EditAS.ItemData[i].Pt[indexmin], EditAS.ItemData[i].Pt[indexnext]));
                }
                int count = 0;
                foreach (PointD _pt in EditAS.ItemData[i].Pt)
                {
                    EditAS.CornerAreaX[count] = _pt.X;
                    EditAS.CornerAreaY[count] = _pt.Y;
                    count++;
                }
                EditAS.SetNumberOfVerticesText(EditAS.ItemData[i].Pt.Count.ToString());

                EditAndSaveAreaSourceData(sender, null); // save changes

                if (EditAS.ItemData.Count > 0)
                {
                    MouseControl = MouseMode.AreaSourceSel;
                }

                Picturebox1_Paint();
            }
        }
        /// <summary>
		/// ContextMenu Delete Edge Point Area Source
		/// </summary>
		private void RightClickAreaDelEdge(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi != null)
            {
                SelectedPointNumber pt = (SelectedPointNumber)(mi.Tag);
                int i = pt.Index;

                EditAS.SetTrackBar(i + 1);
                EditAS.ItemDisplayNr = i;
                EditAS.FillValues();

                int j = 0;
                int indexmin = 0;
                double min = 100000000;
                foreach (PointD _pt in EditAS.ItemData[i].Pt)
                {
                    double dx = pt.X - _pt.X;
                    double dy = pt.Y - _pt.Y;
                    if (Math.Sqrt(dx * dx + dy * dy) < min) // search min
                    {
                        min = Math.Sqrt(dx * dx + dy * dy);
                        indexmin = j;
                    }
                    j++;
                }
                if (indexmin < EditAS.ItemData[i].Pt.Count)
                {
                    EditAS.ItemData[i].Pt.RemoveAt(indexmin);
                }
                int count = 0;
                foreach (PointD _pt in EditAS.ItemData[i].Pt)
                {
                    EditAS.CornerAreaX[count] = _pt.X;
                    EditAS.CornerAreaY[count] = _pt.Y;
                    count++;
                }
                EditAS.SetNumberOfVerticesText(EditAS.ItemData[i].Pt.Count.ToString());

                EditAndSaveAreaSourceData(sender, null); // save changes

                if (EditAS.ItemData.Count > 0)
                {
                    MouseControl = MouseMode.AreaSourceSel;
                }
                Picturebox1_Paint();
            }
        }
        /// <summary>
		/// ContextMenu Delete Area Source
		/// </summary>
		private void RightClickAreaDelete(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi != null)
            {
                int i = Convert.ToInt32(mi.Tag);
                EditAS.SetTrackBar(i + 1);
                EditAS.ItemDisplayNr = i;
                EditAS.FillValues();
                EditAS.RemoveOne(true);
                EditAndSaveAreaSourceData(null, null); // save changes
                Picturebox1_Paint();
                if (EditAS.ItemData.Count > 0)
                {
                    MouseControl = MouseMode.AreaSourceSel;
                }
            }
        }
        /// <summary>
		/// ContextMenu Copy Area Source
		/// </summary>
		private void RightClickAreaCopy(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi != null)
            {
                CopiedItem.AreaSource = new AreaSourceData(EditAS.ItemData[Convert.ToInt32(mi.Tag)]);
            }
        }

    }
}