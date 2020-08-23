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
 * Time: 20:03
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Globalization;

namespace GralData
{
    /// <summary>
    /// Data container for meteopgt.all lines with sortable frequency 
    /// </summary>
    public class PGTAll : IEquatable<PGTAll> , IComparable<PGTAll>
	{
		public string PGTString { get; set; }
		public double PGTFrq { get; set; }
		public int PGTNumber { get; set; }
		private static CultureInfo ic = CultureInfo.InvariantCulture;
		
		public override string ToString()
		{
		    
			return PGTString + "," + Convert.ToString((double) Math.Round(PGTFrq,1), ic);
		}
//		public override bool Equals(object obj)
//		{
//			if (obj == null) return false;
//			PGT_ALL  objAsPGT_ALL= obj as PGT_ALL;
//			if (objAsPGT_ALL == null) return false;
//			else return Equals(objAsPGT_ALL);
//		}
		public int SortByNameAscending(string name1, string name2)
		{
			return name1.CompareTo(name2);
		}

		// Default comparer for Part type.
		public int CompareTo(PGTAll comparePart)
		{
			// A null value means that this object is greater.
			if (comparePart == null)
            {
                return 1;
            }
            else
            {
                return PGTFrq.CompareTo(comparePart.PGTFrq);
            }
        }
		
		public bool Equals(PGTAll other)
		{
			if (other == null)
            {
                return false;
            }

            return (PGTFrq.Equals(other.PGTFrq));
		}
		// Should also override == and != operators.

	}
}
