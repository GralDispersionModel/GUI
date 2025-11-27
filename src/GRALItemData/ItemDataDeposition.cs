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
    [Serializable]
    public class Deposition
    {
        public int Frac_2_5 { get; set; }
        public int Frac_10 { get; set; }
        public int Frac_30 { get; set; }
        public int DM_30 { get; set; }
        public int Preset { get; set; }
        public int Conc { get; set; }
        public double V_Dep1 { get; set; }
        public double V_Dep2 { get; set; }
        public double V_Dep3 { get; set; }
        public double Density { get; set; }

        public Deposition()
        { }

        public Deposition(Deposition other) // Deep copy for new item
        {
            Frac_2_5 = other.Frac_2_5;
            Frac_10 = other.Frac_10;
            Frac_30 = other.Frac_30;
            DM_30 = other.DM_30;
            Preset = other.Preset;
            Conc = other.Conc;
            V_Dep1 = other.V_Dep1;
            V_Dep2 = other.V_Dep2;
            V_Dep3 = other.V_Dep3;
            Density = other.Density;
        }

        public override string ToString()
        {
            string dummy = Convert.ToString(Frac_2_5) + "," +
                Convert.ToString(Frac_10) + "," +
                Convert.ToString(Frac_30) + "," +
                Convert.ToString(DM_30) + "," +
                St_F.DblToIvarTxt(Math.Round(Density, 3)) + "," +
                St_F.DblToIvarTxt(Math.Round(V_Dep1, 4)) + "," +
                St_F.DblToIvarTxt(Math.Round(V_Dep2, 4)) + "," +
                St_F.DblToIvarTxt(Math.Round(V_Dep3, 4)) + "," +
                Convert.ToString(Conc) + "," +
                Convert.ToString(Preset);
            return dummy;
        }

        public void init()
        {
            Frac_2_5 = 0;
            Frac_10 = 0;
            Frac_30 = 0;
            DM_30 = 0;
            Preset = 0;
            Conc = 0;
            V_Dep1 = 0;
            V_Dep2 = 0;
            V_Dep3 = 0;
            Density = 0;
        }

        public void String_to_Val(int i, string[] text)
        {
            if (text.Length > i + 8)
            {
                Frac_2_5 = Convert.ToInt32(text[i]);
                Frac_10 = Convert.ToInt32(text[i + 1]);
                Frac_30 = Convert.ToInt32(text[i + 2]);
                DM_30 = Convert.ToInt32(text[i + 3]);
                Density = St_F.TxtToDbl(text[i + 4], false);
                V_Dep1 = St_F.TxtToDbl(text[i + 5], false);
                V_Dep2 = St_F.TxtToDbl(text[i + 6], false);
                V_Dep3 = St_F.TxtToDbl(text[i + 7], false);
                Conc = Convert.ToInt32(text[i + 8]);
                Preset = Convert.ToInt32(text[i + 9]);
            }
            else
            {
                init();
            }
        }
    }
}
