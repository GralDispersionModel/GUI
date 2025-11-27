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
    /// PointD 3d Data structure for Walls and other Items
    /// </summary>
    public struct PointD_3d
    {
        public double X;
        public double Y;
        public double Z;

        public PointD_3d(double x, double y, double z)
        {
            X = Math.Round(x, 1);
            Y = Math.Round(y, 1);
            Z = Math.Round(z, 1);
        }
        public PointD_3d(string x, string y, string z, CultureInfo cul)
        {
            X = Math.Round(Convert.ToDouble(x, cul), 1);
            Y = Math.Round(Convert.ToDouble(y, cul), 1);
            Z = Math.Round(Convert.ToDouble(z, cul), 1);
        }
        public GralDomain.PointD ToPointD()
        {
            return new GralDomain.PointD(X, Y);
        }
        public System.Drawing.PointF ToPointF()
        {
            return new System.Drawing.PointF((float)X, (float)Y);
        }
        public override bool Equals(object obj)
        {
            return obj is PointD_3d && this == (PointD_3d)obj;
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }
        public static bool operator ==(PointD_3d a, PointD_3d b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }
        public static bool operator !=(PointD_3d a, PointD_3d b)
        {
            return !(a == b);
        }
    }
}
