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

using GralStaticFunctions;
using System;

namespace GralItemData
{
    /// <summary>
    /// This class represents the NEMO data
    /// </summary>
    [Serializable]
    public class NemoData
    {
        public int AvDailyTraffic { get; set; }
        public int BaseYear { get; set; }
        public int TrafficSit { get; set; }
        public float ShareHDV { get; set; }
        public float Slope { get; set; }

        public NemoData()
        { }

        public NemoData(NemoData other)
        {
            AvDailyTraffic = other.AvDailyTraffic;
            BaseYear = other.BaseYear;
            TrafficSit = other.TrafficSit;
            ShareHDV = other.ShareHDV;
            Slope = other.Slope;
        }

        public override string ToString()
        {
            string dummy = Convert.ToString(AvDailyTraffic) + "," +
                St_F.DblToIvarTxt(Math.Round(ShareHDV, 3)) + "," +
                St_F.DblToIvarTxt(Math.Round(Slope, 3)) + "," +
                Convert.ToString(TrafficSit) + "," +
                Convert.ToString(BaseYear);
            return dummy;
        }
    }
}
