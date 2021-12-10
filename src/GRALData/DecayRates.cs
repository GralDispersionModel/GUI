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

using System.Globalization;

namespace GralData
{
    /// <summary>
    /// Data container for decay rates
    /// </summary>
    public class DecayRates
    {
        public int SourceGroup { get; set; }
        public double DecayRate { get; set; }
        private readonly static CultureInfo ic = CultureInfo.InvariantCulture;

        public override string ToString()
        {
            return SourceGroup.ToString(ic) + ":" + DecayRate.ToString(ic);
        }

        // Default comparer for Part type.
        public int CompareTo(DecayRates comparePart)
        {
            // A null value means that this object is greater.
            if (comparePart == null)
            {
                return 1;
            }
            else
            {
                return SourceGroup.CompareTo(comparePart.SourceGroup);
            }
        }

        public bool Equals(DecayRates other)
        {
            if (other == null)
            {
                return false;
            }

            return (SourceGroup.Equals(other.SourceGroup));
        }
    }
}
