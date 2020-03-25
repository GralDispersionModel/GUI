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
using System.Windows.Forms;
using GralItemData;

namespace GralDomain
{
    public partial class Domain
	{
		/// <summary>
        /// Paste a portal source
        /// </summary>
        private void RightClickPortalPaste(object sender, System.EventArgs e)
		{
		    double x = Convert.ToDouble(textBox1.Text);
		    double y = Convert.ToDouble(textBox2.Text);
		    double x0 = CopiedItem.PortalSource.Pt1.X - CopiedItem.PortalSource.Pt2.X;
		    double y0 = CopiedItem.PortalSource.Pt1.Y - CopiedItem.PortalSource.Pt2.Y;
			CopiedItem.PortalSource.Pt1 = new PointD(x, y);
			CopiedItem.PortalSource.Pt2 = new PointD(x - x0, y - y0);
			
			PortalsData Temp = new PortalsData(CopiedItem.PortalSource);
			Temp.Name += "-Copy";
			EditPortals.ItemData.Add(Temp);
			EditPortals.SetTrackBarMaximum();
			CopiedItem.PortalSource = null;
			EditAndSavePortalSourceData(null, null); // save changes
			Picturebox1_Paint();
			MouseControl = 16;
		}
			
		/// <summary>
        /// Right mouse click to a portal source -> show context menu
        /// </summary>
		private void RightClickPortal(object sender, System.EventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			int i = Convert.ToInt32(mi.Tag);
			
			if (mi.Index == 0) // Edit Portal Source
			{
				EditPortals.SetTrackBar(i + 1);
				EditPortals.ItemDisplayNr = i;
				EditPortals.FillValues();
				TunnelPortalsToolStripMenuItemClick(null, null);
			}
			if (mi.Index == 1) // Delete Portal Source
			{
				EditPortals.SetTrackBar(i + 1);
				EditPortals.ItemDisplayNr = i;
				EditPortals.FillValues();
				EditPortals.RemoveOne(true);
				EditAndSavePortalSourceData(null, null); // save changes
				Picturebox1_Paint();
				if (EditPortals.ItemData.Count > 0)
                {
                    MouseControl = 16;
                }
            }
			if (mi.Index == 2) // Flip exit surface
			{
			    PortalsData _pdata = EditPortals.ItemData[Convert.ToInt32(mi.Tag)];
			    PointD p1 = _pdata.Pt2;
			    _pdata.Pt2 = _pdata.Pt1;
                _pdata.Pt1 = p1;
                //MessageBox.Show(_pdata.Pt1.X.ToString() + "/" + _pdata.Pt2.X.ToString() + "/" + p1.X.ToString());

			    EditAndSavePortalSourceData(null, null); // save changes
				Picturebox1_Paint();
				MouseControl = 16;
			}
			if (mi.Index == 3) // Copy source
			{
			    CopiedItem.PortalSource = new PortalsData(EditPortals.ItemData[Convert.ToInt32(mi.Tag)]);
			}
			Menu m = sender as Menu;
			m.Dispose ();
		}
    }
}