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
 * Date: 14.10.2018
 * Time: 16:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Gral
{
	/// <summary>
	/// This class represents the source group data
	/// </summary>
	public class SG_Class : IEquatable<SG_Class> , IComparable<SG_Class>
    {
        public string SG_Name { get; set; }

        public int SG_Number { get; set; }

        public override string ToString()
        {
            return SG_Name + "," + SG_Number.ToString();
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            SG_Class objAsSG = obj as SG_Class;
            if (objAsSG == null) return false;
            else return Equals(objAsSG);
        }
        
        public int SortByNameAscending(string name1, string name2)
        {

            return name1.CompareTo(name2);
        }

        // Default comparer for Part type.
        public int CompareTo(SG_Class compareSG)
        {
            // A null value means that this object is greater.
            if (compareSG == null)
                return 1;

            else
                return SG_Number.CompareTo(compareSG.SG_Number);
        }
        
        public override int GetHashCode()
        {
            return SG_Number;
        }
        
        public bool Equals(SG_Class other)
        {
            if (other == null) return false;
            return (SG_Number.Equals(other.SG_Number));
        }
     }
}
