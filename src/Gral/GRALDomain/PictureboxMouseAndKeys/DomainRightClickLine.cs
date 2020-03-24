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
        private int RightClickSearchLineSource(MouseEventArgs e)
        {
            int i = 0;
            // search items
            List <string> ItemNames = new List<string>();
            List <int> ItemNumber = new List<int>();
            double fx = 1 / BmpScale / MapSize.SizeX;
            double fy = 1 / BmpScale / MapSize.SizeY;
            
            PointD MousePosition = new PointD(e.X, e.Y);
            
            PointD[] poly = new PointD[4];
            
            foreach (LineSourceData _ls in EditLS.ItemData)
            {
                double x2 = 0;
                double y2 = 0;
                int lastpoint = _ls.Pt.Count - 1;
                for (int j = 0; j < _ls.Pt.Count; j++)
                {
                    double x1 = (_ls.Pt[j].X - MapSize.West) * fx + TransformX;
                    double y1 = (_ls.Pt[j].Y - MapSize.North) * fy + TransformY;
                    
                    if (j < lastpoint)
                    {
                        x2 = (_ls.Pt[j + 1].X - MapSize.West) * fx + TransformX;
                        y2 = (_ls.Pt[j + 1].Y - MapSize.North) * fy + TransformY;
                    }
                    else if (j > 0)
                    {
                        x2 = (_ls.Pt[j - 1].X - MapSize.West) * fx + TransformX;
                        y2 = (_ls.Pt[j - 1].Y - MapSize.North) * fy + TransformY;
                    }
                    
                    double length = Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
                    
                    if (Math.Abs (x1 - e.X) < 200 && Math.Abs(y1 - e.Y) < 200 && length > 0)
                    {
                        double cosalpha = (x2 - x1) / length;
                        double sinalpha = (y1 - y2) / length;
                        double dx = Math.Max(_ls.Width / 2 * fx, 2) * sinalpha;
                        double dy = Math.Max(_ls.Width / 2 * fy, 2) * cosalpha;
                        poly[0] = new PointD((int)(x1 + dx), (int)(y1 + dy));
                        poly[1] = new PointD((int)(x1 - dx), (int)(y1 - dy));
                        poly[2] = new PointD((int)(x2 - dx), (int)(y2 - dy));
                        poly[3] = new PointD((int)(x2 + dx), (int)(y2 + dy));
                        
                        if (St_F.PointInPolygonArray(MousePosition, poly))
                        {
                            ItemNames.Add(_ls.Name);
                            ItemNumber.Add(i);
                            j = _ls.Pt.Count + 1; // break search
                        }
                    }   
                }
                i++;
            }
            
            return SelectOverlappedItem(e, ItemNames, ItemNumber);
        }
        
		/// <summary>
        /// Paste a line source
        /// </summary>
        private void RightClickLineSourcePaste(object sender, System.EventArgs e)
		{
			if (CopiedItem.LineSource != null && CopiedItem.LineSource.Pt.Count > 1)
			{
				double x = Convert.ToDouble(textBox1.Text);
				double y = Convert.ToDouble(textBox2.Text);
				double x0 = CopiedItem.LineSource.Pt[0].X;
				double y0 = CopiedItem.LineSource.Pt[0].Y;
				
				for(int i = 0; i < CopiedItem.LineSource.Pt.Count; i++)
				{
					CopiedItem.LineSource.Pt[i] = new GralData.PointD_3d(x  - x0 + CopiedItem.LineSource.Pt[i].X, 
																		 y - y0 + CopiedItem.LineSource.Pt[i].Y,
																		 CopiedItem.LineSource.Pt[i].Z);
				}
				
				LineSourceData Temp = new LineSourceData(CopiedItem.LineSource);
				Temp.Name += "-Copy";
				EditLS.ItemData.Add(Temp);
				EditLS.SetTrackBarMaximum();
				CopiedItem.LineSource = null;
				EditAndSaveLineSourceData(null, null); // save changes
				Picturebox1_Paint();
				MouseControl = 11;
			}
		}
		
		/// <summary>
        /// Right mouse click to a line source -> show context menu
        /// </summary>
		private void RightClickLine(object sender, System.EventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			
			if (mi.Index == 0) // Edit Line Source
			{
				int i = Convert.ToInt32(mi.Tag);
				EditLS.SetTrackBar(i + 1);
				EditLS.ItemDisplayNr = i;
				EditLS.FillValues();
				LineSourcesToolStripMenuItemClick(null, null);
			}
			
			if (mi.Index == 1) // Move edge point
			{
				PointD coor = (PointD) mi.Tag;
				textBox1.Text = coor.X.ToString();
				textBox2.Text = coor.Y.ToString();
				MoveEdgepointLine();
				MouseControl = 1000;
			}
			
			if (mi.Index == 2) // Add edge point
			{
				SelectedPointNumber pt = (SelectedPointNumber) (mi.Tag);
				int i = pt.Index;
				
				EditLS.SetTrackBar(i + 1);
				EditLS.ItemDisplayNr = i;
				EditLS.FillValues();
				
				int j = 0;
				int indexmin = 0;
				double min = 100000000;
				foreach(GralData.PointD_3d _pt in EditLS.ItemData[i].Pt)
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
				if (indexmin < EditLS.ItemData[i].Pt.Count)
				{
					int indexnext = indexmin + 1;
					if (indexnext >= EditLS.ItemData[i].Pt.Count)
					{
						indexnext = 0;
					}
					EditLS.ItemData[i].Pt.Insert(indexmin + 1, GetPointBetween(EditLS.ItemData[i].Pt[indexmin], EditLS.ItemData[i].Pt[indexnext]));
				}

				int count = 0;
				foreach(GralData.PointD_3d _pt in EditLS.ItemData[i].Pt)
				{
					EditLS.CornerLineX[count] = _pt.X;
					EditLS.CornerLineY[count] = _pt.Y;
					EditLS.CornerLineZ[count] = _pt.Z;
					count++;
				}
				EditLS.SetNumberOfVerticesText(EditLS.ItemData[i].Pt.Count.ToString());
				
				EditAndSaveLineSourceData(sender, null); // save changes
				
				if (EditLS.ItemData.Count > 0)
					MouseControl = 11;
				Picturebox1_Paint();
			}
			if (mi.Index == 3) // Delete edge point
			{
				SelectedPointNumber pt = (SelectedPointNumber) (mi.Tag);
				int i = pt.Index;
				
				EditLS.SetTrackBar(i + 1);
				EditLS.ItemDisplayNr = i;
				EditLS.FillValues();
				
				int j = 0;
				int indexmin = 0;
				double min = 100000000;
				foreach(GralData.PointD_3d _pt in EditLS.ItemData[i].Pt)
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
				if (indexmin < EditLS.ItemData[i].Pt.Count)
				{
					EditLS.ItemData[i].Pt.RemoveAt(indexmin);
				}
				int count = 0;
				foreach(GralData.PointD_3d _pt in EditLS.ItemData[i].Pt)
				{
					EditLS.CornerLineX[count] = _pt.X;
					EditLS.CornerLineY[count] = _pt.Y;
					EditLS.CornerLineZ[count] = _pt.Z;
					count++;
				}
				EditLS.SetNumberOfVerticesText(EditLS.ItemData[i].Pt.Count.ToString());
				
				EditAndSaveLineSourceData(sender, null); // save changes
				
				if (EditLS.ItemData.Count > 0)
					MouseControl = 11;
				Picturebox1_Paint();
			}
			if (mi.Index == 4) // Delete line Source
			{
				MouseControl = 0;
				int i = Convert.ToInt32(mi.Tag);
				EditLS.SetTrackBar(i + 1);
				EditLS.ItemDisplayNr = i;
				EditLS.FillValues();
				EditLS.RemoveOne(true);
				EditAndSaveLineSourceData(sender, null); // save changes
				Picturebox1_Paint();
				if (EditLS.ItemData.Count > 0)
					MouseControl = 11;
			}
			if (mi.Index == 5) // Copy line source
			{
				CopiedItem.LineSource = new LineSourceData(EditLS.ItemData[Convert.ToInt32(mi.Tag)]);
			}
			Menu m = sender as Menu;
			m.Dispose ();
		}
    }
}