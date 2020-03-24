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
using System.IO;

namespace Gral
{    
    partial class Main
    {
        /// <summary>
        /// enable ERA5 transient forcing for GRAMM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetERA5Forcing(object sender, EventArgs e)
        {
            SaveIINDatFile();
            GRALGenerateMeteoFilesForERA5();
            PythonScriptForERA5();
        }

        /// <summary>
        /// sets the start time for transient GRAMM simulations using ERA5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetERA5SetStartTime(object sender, EventArgs e)
        {
            SaveIINDatFile();
        }

        /// <summary>
        /// set the time interval for re-initialzing the GRAMM simulations, when driven by ERA5 data in hours
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMReInitializingERA5(object sender, EventArgs e)
        {
            SaveIINDatFile();
        }

        /// <summary>
        /// set the time interval for updating boundary conditions in the GRAMM simulations, when driven by ERA5 data in hours
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMBoundaryConditionsERA5(object sender, EventArgs e)
        {
            SaveIINDatFile();
        }

        /// <summary>
        /// set the coordinate system used in the GRAMM simulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetERA5SetCoordinateSystem(object sender, EventArgs e)
        {
            if (EmifileReset == true)
            {
                try
                {
                    using (StreamWriter mywriter = new StreamWriter(Path.Combine(ProjectName, @"Computation", "GRAMM-coordinatesystem.txt")))
                    {
                        mywriter.WriteLine(numericUpDown40.Value.ToString());
                    }
                }
                catch
                {}
            }
        }
    }
}
