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
 * Time: 17:02
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;

using GralItemData;

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
            List <string> ItemNames = new List<string>();
            List <int> ItemNumber = new List<int>();
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
			MouseControl = 25;
		}
		
		/// <summary>
        /// Right mouse click to a receptor -> show context menu
        /// </summary>
		private void RightClickReceptor(object sender, System.EventArgs e)
		{
			MenuItem mi = sender as MenuItem;

			int i = Convert.ToInt32(mi.Tag);
			
			if (mi.Index == 0) // Edit Receptor
			{
				ReceptorPointsToolStripMenuItemClick(null, null);
			}
			if (mi.Index == 1) // new Position of Receptor
			{
				MouseControl = 2400;
			}
			if (mi.Index == 2) // Delete Receptor
			{
				EditR.RemoveOne(true);
				EditAndSaveReceptorData(null, null); // save changes
				Picturebox1_Paint();
				if (EditR.ItemData.Count > 0)
                {
                    MouseControl = 25;
                }
            }
			if (mi.Index == 3) // Copy receptor
			{
				CopiedItem.Receptor = new ReceptorData(EditR.ItemData[Convert.ToInt32(mi.Tag)]);
			}

			Menu m = sender as Menu;
			m.Dispose ();
		}
    }
}