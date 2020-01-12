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
 * User: U0178969
 * Date: 06.03.2019
 * Time: 07:39
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace GralDomForms
{
    /// <summary>
    /// Wind file class used by the wind rose display in the GIS window
    /// </summary>
    public class WindFileData
	{
		public string Filename { get; set; }
		public char RowSep     { get; set; }
		public string DecSep   { get; set; } 
		public int MaxValue    { get; set; }
		public int Size        { get; set; }
		public double X0	   { get; set; }
		public double Y0	   { get; set; }
		public double Z0	   { get; set; }
	}
}
