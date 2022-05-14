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
			MouseControl = MouseMode.PortalSourceSel;
			ContextMenuStrip m = sender as ContextMenuStrip;
			if (m != null)
			{
				m.Dispose();
			}
		}

		/// <summary>
		/// ContextMenu Edit Portal Source
		/// </summary>
		private void RightClickPortalEdit(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			if (mi != null)
			{
				int i = Convert.ToInt32(mi.Tag);
				EditPortals.SetTrackBar(i + 1);
				EditPortals.ItemDisplayNr = i;
				EditPortals.FillValues();
				TunnelPortalsToolStripMenuItemClick(null, null);
			}
		}
        /// <summary>
        /// ContextMenu Delete Portal Source
        /// </summary>
        private void RightClickPortalDelete(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
			if (mi != null)
			{
				int i = Convert.ToInt32(mi.Tag);
				EditPortals.SetTrackBar(i + 1);
				EditPortals.ItemDisplayNr = i;
				EditPortals.FillValues();
				EditPortals.RemoveOne(true);
				EditAndSavePortalSourceData(null, null); // save changes
				Picturebox1_Paint();
				if (EditPortals.ItemData.Count > 0)
				{
					MouseControl = MouseMode.PortalSourceSel;
				}
			}
		}
		/// <summary>
		/// ContextMenu Flip Portal Source
		/// </summary>
		private void RightClickPortalFlip(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			if (mi != null)
			{
				PortalsData _pdata = EditPortals.ItemData[Convert.ToInt32(mi.Tag)];
				PointD p1 = _pdata.Pt2;
				_pdata.Pt2 = _pdata.Pt1;
				_pdata.Pt1 = p1;
				//MessageBox.Show(_pdata.Pt1.X.ToString() + "/" + _pdata.Pt2.X.ToString() + "/" + p1.X.ToString());

				EditAndSavePortalSourceData(null, null); // save changes
				Picturebox1_Paint();
				MouseControl = MouseMode.PortalSourceSel;
			}
		}
		/// <summary>
		/// ContextMenu Copy Portal Source
		/// </summary>
		private void RightClickPortalCopy(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			if (mi != null)
			{
				CopiedItem.PortalSource = new PortalsData(EditPortals.ItemData[Convert.ToInt32(mi.Tag)]);
			}
		}
    }
}