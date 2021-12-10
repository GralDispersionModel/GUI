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
    /// Selected Point number and coordinates if an item is selected in the domain-form
    /// </summary>
    public struct SelectedPointNumber
	{
		public double X;
		public double Y;
		public int Index;
		public SelectedPointNumber(string x, string y, int index, CultureInfo cul) 
		{
			X = Math.Round(Convert.ToDouble(x, cul), 1);
			Y = Math.Round(Convert.ToDouble(y, cul), 1);
			Index = index;
		}
	}
}
