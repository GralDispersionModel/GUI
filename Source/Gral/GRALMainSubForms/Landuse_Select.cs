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

namespace GralMainForms
{
    public partial class LanduseSelect : Form
    {
        public LanduseSelect()
        {
            InitializeComponent();
            button1.DialogResult = DialogResult.OK;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
        }
        
        void Landuse_SelectLoad(object sender, EventArgs e)
        {
        	if (Owner != null)
        		Location = new Point(Math.Max(0,Owner.Location.X + Owner.Width / 2 - Width / 2 - 100),
        		                    Math.Max(0, Owner.Location.Y + Owner.Height / 2 - Height / 2 -100));
        }
    }
}
