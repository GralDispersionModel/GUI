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

namespace GralData
{
    /// <summary>
    /// Data container for coordinates transmitted by an event
    /// </summary>
    public class EventArgsCoors : EventArgs
    {
        private double _x;
        private double _y;

        public EventArgsCoors(double X, double Y)
        {
            _x = X;
            _y = Y;
        }

        public EventArgsCoors(string X, string Y)
        {
            _x = double.Parse(X);
            _y = double.Parse(Y);
        }

        public double X { get { return _x; } }
        public double Y { get { return _y; } }
    }
}
