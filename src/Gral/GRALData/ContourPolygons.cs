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
 * Date: 11.12.2018
 * Time: 20:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace GralData
{
    /// <summary>
    /// Contour Polygons with the value of the polygon and the drawing order 
    /// </summary>
    public class ContourPolygon : IEquatable<ContourPolygon> , IComparable<ContourPolygon>
	 {
        public int ItemValueIndex { get; set; }
        public int DrawingOrder   { get; set; }
        public GralDomain.PointD[] EdgePoints{ get; set; } // List of edge points for each polygon
        public DomainArea Bounds  { get; set; }
        
        public ContourPolygon()
        {
            Bounds = new DomainArea()
            {
                West = double.MaxValue,
                East = double.MinValue,
                North = double.MinValue,
                South =  double.MaxValue,
            };
        }
        
        // Default comparer for Part type.
        public int CompareTo(ContourPolygon comparePart)
        {
            // A null value means that this object is greater.
            if (comparePart == null)
            {
                return 1;
            }
            else
            {
                return DrawingOrder.CompareTo(comparePart.DrawingOrder);
            }
        }
        
        public bool Equals(ContourPolygon other)
        {
            if (other == null)
            {
                return false;
            }

            return (DrawingOrder.Equals(other.DrawingOrder));
        }
    }
}
