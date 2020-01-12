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
using System.Drawing;
using System.Windows.Forms;

using GralData;
using GralItemData;
using GralStaticFunctions;

namespace GralDomain
{
    public partial class Domain
	{
        /// <summary>
        /// Search an item at the mouse position and if more than 1 item is found, a selection dialog appears
        /// </summary>
        /// <returns>-1: no match or number of item</returns>
        private int RightClickSearchWalls(MouseEventArgs e)
        {
            int i = 0;
            // search items
            List <string> ItemNames = new List<string>();
            List <int> ItemNumber = new List<int>();
            double fx = 1 / BmpScale / MapSize.SizeX;
            double fy = 1 / BmpScale / MapSize.SizeY;
            
            PointD MousePosition = new PointD(e.X, e.Y);
            PointD[] poly = new PointD[4];
            double wall_width = Math.Max(Convert.ToDouble(MainForm.numericUpDown10.Value) / 2 / BmpScale / MapSize.SizeX, 2);
            
            foreach (WallData _wd in EditWall.ItemData)
            {
                double x2 = 0;
                double y2 = 0;
                int lastpoint = _wd.Pt.Count - 1;
                for (int j = 0; j < _wd.Pt.Count; j++)
                {
                    double x1 = (_wd.Pt[j].X - MapSize.West) * fx + TransformX;
                    double y1 = (_wd.Pt[j].Y - MapSize.North) * fy + TransformY;
                    
                    if (j < lastpoint)
                    {
                        x2 = (_wd.Pt[j + 1].X - MapSize.West) * fx + TransformX;
                        y2 = (_wd.Pt[j + 1].Y - MapSize.North) * fy + TransformY;
                    }
                    else if (j > 0)
                    {
                        x2 = (_wd.Pt[j - 1].X - MapSize.West) * fx + TransformX;
                        y2 = (_wd.Pt[j - 1].Y - MapSize.North) * fy + TransformY;
                    }
                    
                    double length = Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
                    
                    if (Math.Abs (x1 - e.X) < 200 && Math.Abs(y1 - e.Y) < 200 && length > 0)
                    {
                        double cosalpha = (x2 - x1) / length;
                        double sinalpha = (y1 - y2) / length;
                        double dx = wall_width  * sinalpha;
                        double dy = wall_width  * cosalpha;
                        poly[0] = new PointD((int)(x1 + dx), (int)(y1 + dy));
                        poly[1] = new PointD((int)(x1 - dx), (int)(y1 - dy));
                        poly[2] = new PointD((int)(x2 - dx), (int)(y2 - dy));
                        poly[3] = new PointD((int)(x2 + dx), (int)(y2 + dy));
                        
                        if (St_F.PointInPolygonArray(MousePosition, poly))
                        {
                            ItemNames.Add(_wd.Name);
                            ItemNumber.Add(i);
                            j = _wd.Pt.Count + 1; // break search
                        }
                    }   
                }
                i++;
            }
            
            return SelectOverlappedItem(e, ItemNames, ItemNumber);
        }
        
		/// <summary>
        /// Right mouse click to a wall -> show context menu
        /// </summary>
       	private void RightClickWall(object sender, System.EventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			
			if (mi.Index == 0) // Edit wall
			{
				int i = Convert.ToInt32(mi.Tag);
				EditWall.SetTrackBar(i + 1);
				EditWall.ItemDisplayNr = i;
				EditWall.FillValues();
				WallsToolStripMenuItemClick(null, null);
			}
			
			if (mi.Index == 1) // Move wall edge point
			{
				PointD coor = (PointD) mi.Tag;
				textBox1.Text = coor.X.ToString();
				textBox2.Text = coor.Y.ToString();
				MoveEdgepointWall();
				MouseControl = 1001;
			}
			
			if (mi.Index == 2) // Add edge point
			{
				SelectedPointNumber pt = (SelectedPointNumber) (mi.Tag);
				int i = pt.Index;
				
				EditWall.SetTrackBar(i + 1);
				EditWall.ItemDisplayNr = i;
				EditWall.FillValues();
				
				int j = 0;
				int indexmin = 0;
				double min = 100000000;
				foreach(PointD_3d _pt in EditWall.ItemData[i].Pt)
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
				if (indexmin < EditWall.ItemData[i].Pt.Count)
				{
					int indexnext = indexmin + 1;
					if (indexnext >= EditWall.ItemData[i].Pt.Count)
					{
						indexnext = 0;
					}
					PointD pt1 = new PointD(EditWall.ItemData[i].Pt[indexmin].X, EditWall.ItemData[i].Pt[indexmin].Y);
					PointD pt2 = new PointD(EditWall.ItemData[i].Pt[indexnext].X, EditWall.ItemData[i].Pt[indexnext].Y);
					PointD ptbetween = GetPointBetween(pt1, pt2);
					EditWall.ItemData[i].Pt.Insert(indexmin + 1, new PointD_3d(ptbetween.X, ptbetween.Y, EditWall.ItemData[i].Pt[indexmin].Z));
				}
				int count = 0;
				foreach(PointD_3d _pt in EditWall.ItemData[i].Pt)
				{
					EditWall.CornerWallX[count] = _pt.X;
					EditWall.CornerWallY[count] = _pt.Y;
					EditWall.CornerWallZ[count] = (float) (_pt.Z);
					count++;
				}
				EditWall.SetNumberOfVerticesText(EditWall.ItemData[i].Pt.Count.ToString());
				
				EditAndSaveWallData(sender, null); // save changes
				
				if (EditWall.ItemData.Count > 0)
					MouseControl = 76;
				Picturebox1_Paint();
			}
			if (mi.Index == 3) // Delete edge point
			{
				SelectedPointNumber pt = (SelectedPointNumber) (mi.Tag);
				int i = pt.Index;
				
				EditWall.SetTrackBar(i + 1);
				EditWall.ItemDisplayNr = i;
				EditWall.FillValues();
				
				int j = 0;
				int indexmin = 0;
				double min = 100000000;
				foreach(PointD_3d _pt in EditWall.ItemData[i].Pt)
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
				if (indexmin < EditWall.ItemData[i].Pt.Count)
				{
					EditWall.ItemData[i].Pt.RemoveAt(indexmin);
				}
				int count = 0;
				foreach(PointD_3d _pt in EditWall.ItemData[i].Pt)
				{
					EditWall.CornerWallX[count] = _pt.X;
					EditWall.CornerWallY[count] = _pt.Y;
					EditWall.CornerWallZ[count] = (float) (_pt.Z);
					count++;
				}
				EditWall.SetNumberOfVerticesText(EditWall.ItemData[i].Pt.Count.ToString());
				
				EditAndSaveWallData(sender, null); // save changes
				
				if (EditWall.ItemData.Count > 0)
					MouseControl = 76;
				Picturebox1_Paint();
			}
			
			
			if (mi.Index == 4) // Delete wall
			{
				MouseControl = 0;
				int i = Convert.ToInt32(mi.Tag);
				EditWall.SetTrackBar(i + 1);
				EditWall.ItemDisplayNr = i;
				EditWall.FillValues();
				EditWall.RemoveOne(true);
				EditAndSaveWallData(null, null); // save changes
				Picturebox1_Paint();
				if (EditWall.ItemData.Count > 0)
					MouseControl = 76;
			}
			Menu m = sender as Menu;
			m.Dispose ();
		}
    }
}