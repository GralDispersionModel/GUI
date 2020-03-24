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
 * User: Markus Kuntner
 * Date: 15.01.2019
 * Time: 19:17
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace GralItemData
{
    /// <summary>
    /// This class represents the deposition data
    /// </summary>
    public class Deposition_Settings
	{
		public string Title   { get; set; }
        /// <summary>
        /// Fraction for PM10 particle class
        /// </summary>
        public int Frac_10 	  { get; set; }
        /// <summary>
        /// Fraction for PM2,5 particle class
        /// </summary>
		public int Frac_25	  { get; set; }
        /// <summary>
        /// Diameter for PM30 particle class
        /// </summary>
		public int DM_30 	  { get; set; }
        /// <summary>
        /// Deposition velocity for PM2,5 or gases
        /// </summary>
		public double V_Dep1  { get; set; }
        /// <summary>
        /// Deposition velocity for PM10
        /// </summary>
		public double V_Dep2  { get; set; }
        /// <summary>
        /// Deposition velocity for PM30
        /// </summary>
		public double V_Dep3  { get; set; }
        /// <summary>
        /// Density for that particle
        /// </summary>
		public double Density { get; set; }
	}
}
