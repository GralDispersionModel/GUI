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


namespace GralShape
{
    /// <summary>
    /// Shapefile Polygon class
    /// </summary>
    public class SHPPolygon
    {
        public double[] Box;
        public int NumParts;
        public int NumPoints;
        public int[] Parts;
        public GralDomain.PointD[] Points;

        /// <summary>
        /// Create an empty Shape Polygon Class 
        /// </summary>
        public SHPPolygon()
        {
            Box = new double[4];
            NumParts = 0;
            NumPoints = 0;
        }

        /// <summary>
        /// Create a deep copy of a Shape Polygon Class
        /// </summary>
        /// <param name="Other">Other Shape Polygon class</param>
        /// <param name="PointsIncluded">Copy shape points?</param>
        public SHPPolygon(SHPPolygon Other, bool PointsIncluded)
        {
            Box = new double[Other.Box.GetLength(0)];
            System.Array.Copy(Other.Box, Box, Other.Box.GetLength(0));
            NumParts = Other.NumParts;
            NumPoints = Other.NumPoints;
            Parts = new int[Other.Parts.GetLength(0)];
            System.Array.Copy(Other.Parts, Parts, Other.Parts.GetLength(0));
            if (PointsIncluded)
            {
                Points = new GralDomain.PointD[Other.Points.GetLength(0)];
                System.Array.Copy(Other.Points, Points, Other.Points.GetLength(0));
            }
        }
    }
}
