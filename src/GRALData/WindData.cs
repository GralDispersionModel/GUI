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
using System.Globalization;

namespace GralData
{
    /// <summary>
    /// Wind data from *.met or *.akt or *.akterm file 
    /// </summary>
    public class WindData
    {
        /// <summary>
        /// Date like used in *.met files as string
        /// </summary>
        public string Date { get; set; }
        /// <summary>
        /// Time like used in *.met files as string
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// Stability class 
        /// </summary>
        public int StabClass { get; set; }
        /// <summary>
        /// Hour 
        /// </summary>
        public int Hour { get; set; }
        /// <summary>
        /// Wind velocity
        /// </summary>
        public double Vel { get; set; }
        /// <summary>
        /// Wind direction
        /// </summary>
        public double Dir { get; set; }

        public override string ToString()
        {
            CultureInfo ic = CultureInfo.InvariantCulture;
            return Date + "," + Time + "," + Math.Round(Vel, 2).ToString(ic) + "," + Math.Round(Dir).ToString(ic) + "," + StabClass.ToString(ic);
        }
    }
}
