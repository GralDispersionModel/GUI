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
 * Time: 08:07
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Globalization;

namespace GralDomain
{
    /// <summary>
    /// PointD Structure used by Items 
    /// </summary>
   public struct PointD 
	{
		public double X;
		public double Y;

		public PointD(double x, double y) 
		{
			X = Math.Round(x, 1);
			Y = Math.Round(y, 1);
		}
		
		public PointD(string x, string y, CultureInfo cul) 
		{
			X = Math.Round(Convert.ToDouble(x, cul), 1);
			Y = Math.Round(Convert.ToDouble(y, cul), 1);
		}

		public Point ToPoint() 
		{
			return new Point((int)X, (int)Y);
		}
		
		public PointF ToPointF() 
		{
			return new PointF((float)X, (float)Y);
		}

		public override bool Equals(object obj) 
		{
			return obj is PointD && this == (PointD)obj;
		}
		public override int GetHashCode() 
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}
		public static bool operator ==(PointD a, PointD b) 
		{
			return a.X == b.X && a.Y == b.Y;
		}
		public static bool operator !=(PointD a, PointD b) 
		{
			return !(a == b);
		}
	}
}
