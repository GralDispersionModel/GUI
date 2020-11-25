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
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using GralData;
using GralItemData;

namespace GralDomain
{
    public partial class Domain
	{
        /// <summary>
        /// Search an item at the mouse position and if more than 1 item is found, a selection dialog appears
        /// </summary>
        /// <returns>-1: no item at mouse position or number of item</returns>
        private int RightClickSearchVegetation(MouseEventArgs e)
        {
            int i = 0;
            
            // search items
            List <string> ItemNames = new List<string>();
            List <int> ItemNumber = new List<int>();
            double fx = 1 / BmpScale / MapSize.SizeX;
            double fy = 1 / BmpScale / MapSize.SizeY;
            Point MousePosition = new Point(e.X, e.Y);
            
            foreach (VegetationData _vd in EditVegetation.ItemData)
            {
                List <Point> poly = new List <Point>();
                poly.Clear();
                
                List <PointD> _pt = _vd.Pt;
                int x1 = 0;
                int y1 = 0;
                for (int j = 0; j < _pt.Count; j++)
                {
                    x1 = (int)((_pt[j].X - MapSize.West) * fx) + TransformX;
                    y1 = (int)((_pt[j].Y - MapSize.North) * fy) + TransformY;
                    poly.Add(new Point(x1, y1));
                }
                
                if (GralStaticFunctions.St_F.PointInPolygon(MousePosition, poly))
                {
                    ItemNames.Add(_vd.Name);
                    ItemNumber.Add(i);
                }
                
                i++;
            }
            
            return SelectOverlappedItem(e, ItemNames, ItemNumber);
        }
        
		/// <summary>
        /// Right mouse click to a vegetation area -> show context menu
        /// </summary>
       	private void RightClickVegetation(object sender, System.EventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			
			if (mi.Index == 0) // Edit Area Source
			{
				int i = Convert.ToInt32(mi.Tag);
				EditVegetation.SetTrackBar(i + 1);
				EditVegetation.ItemDisplayNr = i;
				EditVegetation.FillValues();
				VegetationToolStripMenuItemClick( null, null);
			}
			if (mi.Index == 1) // Move edge point
			{
				PointD coor = (PointD) mi.Tag;
				textBox1.Text = coor.X.ToString();
				textBox2.Text = coor.Y.ToString();
				MoveEdgepointVegetation();
				MouseControl = MouseMode.VegetationInlineEdit;
			}
			if (mi.Index == 2) // Add edge point
			{
				SelectedPointNumber pt = (SelectedPointNumber) (mi.Tag);
				int i = pt.Index;
				
				EditVegetation.SetTrackBar(i + 1);
				EditVegetation.ItemDisplayNr = i;
				EditVegetation.FillValues();
				
				int j = 0;
				int indexmin = 0;
				double min = 100000000;
				foreach(PointD _pt in EditVegetation.ItemData[i].Pt)
				{
					double dx = pt.X - _pt.X;
					double dy = pt.Y - _pt.Y;
					if (Math.Sqrt(dx*dx+dy*dy) < min) // search min
					{
						min = Math.Sqrt(dx*dx+dy*dy);
						indexmin = j;
					}
					j++;
				}
				if (indexmin < EditVegetation.ItemData[i].Pt.Count)
				{
					int indexnext = indexmin + 1;
					if (indexnext >= EditVegetation.ItemData[i].Pt.Count)
					{
						indexnext = 0;
					}
					EditVegetation.ItemData[i].Pt.Insert(indexmin + 1, GetPointBetween(EditVegetation.ItemData[i].Pt[indexmin], EditVegetation.ItemData[i].Pt[indexnext]));
				}
				int count = 0;
				foreach(PointD _pt in EditVegetation.ItemData[i].Pt)
				{
					EditVegetation.CornerVegX[count] = _pt.X;
					EditVegetation.CornerVegY[count] = _pt.Y;
					count++;
				}
				EditVegetation.SetNumberOfVerticesText(EditVegetation.ItemData[i].Pt.Count.ToString());
				
				EditAndSaveVegetationData(sender, null); // save changes
				
				if (EditVegetation.ItemData.Count > 0)
                {
                    MouseControl = MouseMode.VegetationSel;
                }

                Picturebox1_Paint();
			}
			if (mi.Index == 3) // Delete edge point
			{
				SelectedPointNumber pt = (SelectedPointNumber) (mi.Tag);
				int i = pt.Index;
				
				EditVegetation.SetTrackBar(i + 1);
				EditVegetation.ItemDisplayNr = i;
				EditVegetation.FillValues();
				
				int j = 0;
				int indexmin = 0;
				double min = 100000000;
				foreach(PointD _pt in EditVegetation.ItemData[i].Pt)
				{
					double dx = pt.X - _pt.X;
					double dy = pt.Y - _pt.Y;
					if (Math.Sqrt(dx*dx+dy*dy) < min) // search min
					{
						min = Math.Sqrt(dx*dx+dy*dy);
						indexmin = j;
					}
					j++;
				}
				if (indexmin < EditVegetation.ItemData[i].Pt.Count)
				{
					EditVegetation.ItemData[i].Pt.RemoveAt(indexmin);
				}
				int count = 0;
				foreach(PointD _pt in EditVegetation.ItemData[i].Pt)
				{
					EditVegetation.CornerVegX[count] = _pt.X;
					EditVegetation.CornerVegY[count] = _pt.Y;
					count++;
				}
				EditVegetation.SetNumberOfVerticesText(EditVegetation.ItemData[i].Pt.Count.ToString());
				
				EditAndSaveVegetationData(sender, null); // save changes
				
				if (EditVegetation.ItemData.Count > 0)
                {
                    MouseControl = MouseMode.VegetationSel;
                }

                Picturebox1_Paint();
			}
			if (mi.Index == 4) // Delete Vegetation
			{
				int i = Convert.ToInt32(mi.Tag);
				EditVegetation.SetTrackBar(i + 1);
				EditVegetation.ItemDisplayNr = i;
				EditVegetation.FillValues();
				EditVegetation.RemoveOne(true);
				EditAndSaveVegetationData(null, null); // save changes
				Picturebox1_Paint();
				if (EditVegetation.ItemData.Count > 0)
                {
                    MouseControl = MouseMode.VegetationSel;
                }
            }
			Menu m = sender as Menu;
			m.Dispose ();
		} 
    }
}