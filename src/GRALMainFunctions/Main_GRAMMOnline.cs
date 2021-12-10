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
using System.IO;
using System.Windows.Forms;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
    {
        ////////////////////////////////////////////////////////////////
        //  GRAMM ONLINE
        ////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show GRAMM or GRAL onlilne map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button36_Click(object sender, EventArgs e)
        {
            GralDomain.Domain domain = new GralDomain.Domain(this, true)
            {
                ReDrawContours = true,
                ReDrawVectors = true
            };
            domain.StartPosition = FormStartPosition.Manual;
            domain.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition();
            domain.Show();
        }
    }
}