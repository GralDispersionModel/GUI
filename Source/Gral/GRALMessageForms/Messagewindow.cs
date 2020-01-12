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
using System.Linq;

namespace GralMessage
{
	/// <summary>
    /// Show a message form
    /// </summary>
    public partial class MessageWindow : Form
    {
		private string _titlebar = "";
		public string Titlebar { set {_titlebar = value;} }
    	
        public MessageWindow()
        {
            InitializeComponent();
            this.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
            this.Top = 80;
        }

        private void Messagewindow_Load(object sender, EventArgs e)
        {
        	if (_titlebar.Length > 1)
        		Text = _titlebar;
        }

        void MessagewindowResize(object sender, EventArgs e)
        {
        	listBox1.Size = ClientSize;
        }
        
        void ListBox1KeyDown(object sender, KeyEventArgs e)
        {
        	if (e.Control == true && e.KeyCode == Keys.C) 
        		Clipboard.SetText( string.Join( Environment.NewLine, listBox1.SelectedItems.OfType<string>().ToArray() ) );
        }
    }
}