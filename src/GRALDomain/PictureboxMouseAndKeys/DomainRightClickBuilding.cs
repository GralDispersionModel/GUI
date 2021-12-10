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
        private int RightClickSearchBuildings(MouseEventArgs e)
        {
            int i = 0;
            
            // search items
            List <string> ItemNames = new List<string>();
            List <int> ItemNumber = new List<int>();
            double fx = 1 / BmpScale / MapSize.SizeX;
            double fy = 1 / BmpScale / MapSize.SizeY;
            Point MousePosition = new Point(e.X, e.Y);
            
            foreach (BuildingData _bd in EditB.ItemData)
            {
                List <Point> poly = new List <Point>();
                poly.Clear();
                
                List <PointD> _pt = _bd.Pt;
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
                    ItemNames.Add(_bd.Name);
                    ItemNumber.Add(i);
                }
                
                i++;
            }
            
            return SelectOverlappedItem(e, ItemNames, ItemNumber);
        }
        
		/// <summary>
        /// Paste a building
        /// </summary>
        private void RightClickBuildingPaste(object sender, System.EventArgs e)
		{
			if (CopiedItem.Building != null && CopiedItem.Building.Pt.Count > 1)
			{
				double x = Convert.ToDouble(textBox1.Text);
				double y = Convert.ToDouble(textBox2.Text);
				double x0 = CopiedItem.Building.Pt[0].X;
				double y0 = CopiedItem.Building.Pt[0].Y;
				
				for(int i = 0; i < CopiedItem.Building.Pt.Count; i++)
				{
					CopiedItem.Building.Pt[i] = new PointD(x  - x0 + CopiedItem.Building.Pt[i].X, y - y0 + CopiedItem.Building.Pt[i].Y);
				}
				
				BuildingData Temp = new BuildingData(CopiedItem.Building);
				Temp.Name += "-Copy";
				EditB.ItemData.Add(Temp);
				EditB.SetTrackBarMaximum();
				CopiedItem.Building = null;
				EditAndSaveBuildingsData(null, null); // save changes
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
		/// ContextMenu Edit Building
		/// </summary>
		private void RightClickBuildingEdit(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			if (mi != null)
			{
				int i = Convert.ToInt32(mi.Tag);
				EditB.SetTrackBar(i + 1);
				EditB.ItemDisplayNr = i;
				EditB.FillValues();
				BuildingsToolStripMenuItemClick(null, null);
			}
		}
		/// <summary>
		/// ContextMenu Building Move Edge Point
		/// </summary>
		private void RightClickBuildingMoveEdge(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			if (mi != null)
			{
				PointD coor = (PointD)mi.Tag;
				textBox1.Text = coor.X.ToString();
				textBox2.Text = coor.Y.ToString();
				MoveEdgepointBuilding();
				MouseControl = MouseMode.BuildingInlineEdit;
			}
		}
		/// <summary>
		/// ContextMenu Building Add Edge Point
		/// </summary>
		private void RightClickBuildingAddEdge(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			if (mi != null)
			{
				SelectedPointNumber pt = (SelectedPointNumber)(mi.Tag);
				int i = pt.Index;

				EditB.SetTrackBar(i + 1);
				EditB.ItemDisplayNr = i;
				EditB.FillValues();

				int j = 0;
				int indexmin = 0;
				double min = 100000000;
				foreach (PointD _pt in EditB.ItemData[i].Pt)
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
				if (indexmin < EditB.ItemData[i].Pt.Count)
				{
					int indexnext = indexmin + 1;
					if (indexnext >= EditB.ItemData[i].Pt.Count)
					{
						indexnext = 0;
					}
					EditB.ItemData[i].Pt.Insert(indexmin + 1, GetPointBetween(EditB.ItemData[i].Pt[indexmin], EditB.ItemData[i].Pt[indexnext]));
				}
				int count = 0;
				foreach (PointD _pt in EditB.ItemData[i].Pt)
				{
					EditB.CornerBuildingX[count] = _pt.X;
					EditB.CornerBuildingY[count] = _pt.Y;
					count++;
				}
				EditB.SetNumberOfVerticesText(EditB.ItemData[i].Pt.Count.ToString());

				EditAndSaveBuildingsData(sender, null); // save changes

				if (EditB.ItemData.Count > 0)
				{
					MouseControl = MouseMode.BuildingSel;
				}
				Picturebox1_Paint();
			}
		}
		/// <summary>
		/// ContextMenu Building Delete Edge Point
		/// </summary>
		private void RightClickBuildingDelEdge(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			if (mi != null)
			{
				SelectedPointNumber pt = (SelectedPointNumber)(mi.Tag);
				int i = pt.Index;

				EditB.SetTrackBar(i + 1);
				EditB.ItemDisplayNr = i;
				EditB.FillValues();

				int j = 0;
				int indexmin = 0;
				double min = 100000000;
				foreach (PointD _pt in EditB.ItemData[i].Pt)
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
				if (indexmin < EditB.ItemData[i].Pt.Count)
				{
					EditB.ItemData[i].Pt.RemoveAt(indexmin);
				}
				int count = 0;
				foreach (PointD _pt in EditB.ItemData[i].Pt)
				{
					EditB.CornerBuildingX[count] = _pt.X;
					EditB.CornerBuildingY[count] = _pt.Y;
					count++;
				}
				EditB.SetNumberOfVerticesText(EditB.ItemData[i].Pt.Count.ToString());

				EditAndSaveBuildingsData(sender, null); // save changes

				if (EditB.ItemData.Count > 0)
				{
					MouseControl = MouseMode.BuildingSel;
				}

				Picturebox1_Paint();
			}
		}
		/// <summary>
		/// ContextMenu Building Delete 
		/// </summary>
		private void RightClickBuildingDelete(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			if (mi != null)
			{
				int i = Convert.ToInt32(mi.Tag);
				EditB.SetTrackBar(i + 1);
				EditB.ItemDisplayNr = i;
				EditB.FillValues();
				EditB.RemoveOne(true, true);
				EditAndSaveBuildingsData(null, null); // save changes
				Picturebox1_Paint();
				if (EditB.ItemData.Count > 0)
				{
					MouseControl = MouseMode.BuildingSel;
				}
			}
		}
		/// <summary>
		/// ContextMenu Building Copy
		/// </summary>
		private void RightClickBuildingCopy(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			if (mi != null)
			{
				CopiedItem.Building = new BuildingData(EditB.ItemData[Convert.ToInt32(mi.Tag)]);
			}
		}
    }
}