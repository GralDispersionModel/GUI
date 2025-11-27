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

using GralItemData;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Search an item at the mouse position and if more than 1 item is found, a selection dialog appears
        /// </summary>
        /// <returns>-1: no match or number of item</returns>
        private int RightClickSearchReceptor(MouseEventArgs e)
        {
            int i = 0;
            // search items
            List<string> ItemNames = new List<string>();
            List<int> ItemNumber = new List<int>();
            double fx = 1 / BmpScale / MapSize.SizeX;
            double fy = 1 / BmpScale / MapSize.SizeY;

            foreach (ReceptorData _rd in EditR.ItemData)
            {
                int x1 = (int)((_rd.Pt.X - MapSize.West) * fx) + TransformX;
                int y1 = (int)((_rd.Pt.Y - MapSize.North) * fy) + TransformY;
                if ((e.X >= x1 - 10) && (e.X <= x1 + 10) && (e.Y >= y1 - 10) && (e.Y <= y1 + 10))
                {
                    ItemNames.Add(_rd.Name);
                    ItemNumber.Add(i);
                }
                i++;
            }

            return SelectOverlappedItem(e, ItemNames, ItemNumber);
        }

        /// <summary>
        /// Paste a receptor
        /// </summary>
        private void RightClickReceptorPaste(object sender, System.EventArgs e)
        {
            CopiedItem.Receptor.Pt = new PointD(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text));

            ReceptorData Temp = new ReceptorData(CopiedItem.Receptor);
            Temp.Name += "-Copy";
            EditR.ItemData.Add(Temp);
            EditR.SetTrackBarMaximum();
            CopiedItem.Receptor = null;
            EditAndSaveReceptorData(null, null); // save changes
            Picturebox1_Paint();
            MouseControl = MouseMode.ReceptorSel;
            ContextMenuStrip m = sender as ContextMenuStrip;
            if (m != null)
            {
                m.Dispose();
            }
        }


        /// <summary>
        /// ContextMenu Edit Receptor
        /// </summary>
        private void RightClickReceptorEdit(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi != null)
            {
                ReceptorPointsToolStripMenuItemClick(null, null);
            }
        }
        /// <summary>
		/// ContextMenu Move Receptor
		/// </summary>
		private void RightClickReceptorMove(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi != null)
            {
                MouseControl = MouseMode.ReceptorInlineEdit;
            }
        }
        /// <summary>
		/// ContextMenu Delete Receptor
		/// </summary>
		private void RightClickReceptorDelete(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi != null)
            {
                EditR.RemoveOne(true);
                EditAndSaveReceptorData(null, null); // save changes
                Picturebox1_Paint();
                if (EditR.ItemData.Count > 0)
                {
                    MouseControl = MouseMode.ReceptorSel;
                }
            }
        }
        /// <summary>
		/// ContextMenu Copy Receptor
		/// </summary>
		private void RightClickReceptorCopy(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi != null)
            {
                CopiedItem.Receptor = new ReceptorData(EditR.ItemData[Convert.ToInt32(mi.Tag)]);
            }
        }
    }
}