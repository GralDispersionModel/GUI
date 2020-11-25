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
 * Time: 16:54
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
        private int RightClickSearchPointSource(MouseEventArgs e)
        {
            int i = 0;
            // search items
            List <string> ItemNames = new List<string>();
            List <int> ItemNumber = new List<int>();
            double fx = 1 / BmpScale / MapSize.SizeX;
            double fy = 1 / BmpScale / MapSize.SizeY;
            
            foreach (PointSourceData _psdata in EditPS.ItemData)
            {
                int x1 = (int) ((_psdata.Pt.X - MapSize.West) * fx)+TransformX;
                int y1 = (int) ((_psdata.Pt.Y - MapSize.North) * fy)+TransformY;
                
                if ((e.X >= x1 - 10) && (e.X <= x1 + 10) && (e.Y >= y1 - 10) && (e.Y <= y1 + 10))
                {
                    ItemNames.Add(_psdata.Name);
                    ItemNumber.Add(i);
                }
                i++;
            }
            
            return SelectOverlappedItem(e, ItemNames, ItemNumber);
        }
        
		/// <summary>
        /// Paste a point source
        /// </summary>
       	private void RightClickPointSourcePaste(object sender, System.EventArgs e)
		{
			CopiedItem.PointSource.Pt = new PointD(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text));
			
			PointSourceData Temp = new PointSourceData(CopiedItem.PointSource);
			Temp.Name += "-Copy";
			EditPS.ItemData.Add(Temp);
			EditPS.SetTrackBarMaximum();
			CopiedItem.PointSource = null;
			EditAndSavePointSourceData(null, null); // save changes
			Picturebox1_Paint();
			MouseControl = MouseMode.PointSourceSel;
		}
		
		/// <summary>
        /// Right mouse click to a point source -> show context menu
        /// </summary>
		private void RightClickPointSource(object sender, System.EventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			int i = Convert.ToInt32(mi.Tag);
			
			if (mi.Index == 0) // Edit Point Source
			{
				PointSourcesToolStripMenuItemClick(null, null);
			}
			if (mi.Index == 1) // new Position of Point Source
			{
				MouseControl = MouseMode.PointSourceInlineEdit;
			}
			if (mi.Index == 2) // Delete source
			{
				EditPS.RemoveOne(true);
				EditAndSavePointSourceData(null, null); // save changes
				Picturebox1_Paint();
				if (EditPS.ItemData.Count > 0)
                {
                    MouseControl = MouseMode.PointSourceSel;
                }
            }
			if (mi.Index == 3) // Copy source
			{
				CopiedItem.PointSource = new PointSourceData(EditPS.ItemData[Convert.ToInt32(mi.Tag)]);
			}
			Menu m = sender as Menu;
			m.Dispose ();
		} 
    }
}