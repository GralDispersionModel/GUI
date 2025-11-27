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

using GralDomain;
using GralStaticFunctions;
using System;
using System.Globalization;

namespace GralItemData
{
    /// <summary>
    /// This class represents the receptor data
    /// </summary>
    [Serializable]
    public class ReceptorData
    {
        public string Name { get; set; }
        public float Height { get; set; }
        public float DisplayValue { get; set; }
        public GralDomain.PointD Pt { get; set; }

        private static CultureInfo ic = CultureInfo.InvariantCulture;

        /// <summary>
        /// Create a new empty receptor data object
        /// </summary>
        public ReceptorData()
        {
            Name = "Receptor";
            Pt = new PointD(Double.NaN, Double.NaN);
        }

        /// <summary>
        /// Create a new receptor data object from string
        /// </summary>
        public ReceptorData(string sourcedata) // new item from file - string
        {
            int index = 0;
            int version = 0;
            string[] text = new string[1];
            text = sourcedata.Split(new char[] { ',' });
            Pt = new PointD(0, 0);

            //try
            {
                if (text.Length > 4) // otherwise the string or file is corrupt
                {
                    version = Convert.ToInt32(text[index++]);
                    index++; // number of receptor
                    double x = St_F.TxtToDbl(text[index++], false);
                    double y = St_F.TxtToDbl(text[index++], false);
                    Height = (float)(St_F.TxtToDbl(text[index++], false));
                    Pt = new PointD(x, y);
                    if (text.Length > 5)
                    {
                        Name = text[index++];
                    }
                    else
                    {
                        Name = "Receptor";
                    }
                    if (text.Length > 6)
                    {
                        DisplayValue = (float)(St_F.TxtToDbl(text[index++], false));
                    }
                    else
                    {
                        DisplayValue = 0;
                    }
                }
            }
            //catch
            //			{
            //				Name = String.Empty;
            //				Height = 10;
            //				Pt = new PointD(0, 0);
            //			}
        }

        /// <summary>
        /// Create a new receptor data object from other object
        /// </summary>
        public ReceptorData(ReceptorData other) // Deep copy for new item
        {
            Name = other.Name;
            Height = other.Height;
            DisplayValue = other.DisplayValue;
            Pt = other.Pt;
        }

        /// <summary>
        /// Convert object data to a string as used in the item file
        /// </summary>
        public override string ToString()
        {
            if (Name == null || Name == String.Empty)
            {
                Name = "Rec";
            }

            string dummy = Math.Round(Pt.X, 1).ToString(ic) + "," + Math.Round(Pt.Y, 1).ToString(ic) + "," +
                Height.ToString(ic) + "," +
                St_F.RemoveinvalidChars(Name) + "," +
                DisplayValue.ToString(ic);
            return dummy;
        }
    }
}
