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

using System.Windows.Forms;

namespace GralMessage
{
    /// <summary>
    /// Show a progress bar
    /// </summary>
    public partial class Waitprogressbar : Form
    {
        public Waitprogressbar(string text)
        {
            InitializeComponent();
            Text = text;
            this.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
            this.Top = GralStaticFunctions.St_F.GetTopScreenAtMousePosition() + 140;
        }

    }
}