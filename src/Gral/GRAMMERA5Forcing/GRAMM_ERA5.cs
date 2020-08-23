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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gral
{
    partial class Main
    {
        //enable ERA5 transient forcing for GRAMM
        public void checkBox35_CheckedChanged(object sender, EventArgs e)
        {
            SaveIINDatFile();
        }

        //sets the start time for transient GRAMM simulations using ERA5
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            SaveIINDatFile();
        }
    }
}
