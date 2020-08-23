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

namespace GralDomForms
{
	/// <summary>
    /// Simple info box for item data
    /// </summary>
    public partial class Infobox : Form
    {
    	private Point ToolTipMousePosition;
        
        private void MainFormMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ToolTipMousePosition = new Point(-e.X, -e.Y);
        }

        private void MainFormMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(ToolTipMousePosition.X, ToolTipMousePosition.Y);
                Location = mousePos;
            }
        }  

        public Infobox()
        {     	
        	InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
        
        void InfoboxLoad(object sender, EventArgs e)
        {
        	System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
        	Left = point.X;
			Top = point.Y; 
		}
    }
}