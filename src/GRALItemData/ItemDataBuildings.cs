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
using System.Collections.Generic;
using System.Globalization;
using GralDomain;
using GralStaticFunctions;

namespace GralItemData
{
	/// <summary>
    /// This class represents the building data
    /// </summary>
	[Serializable]
	public class BuildingData
	{
		public string Name			{ get; set;}
		public float Height 		{ get; set;}
		public float LowerBound		{ get; set;}
		public float Area	 		{ get; set;}
		public List<GralDomain.PointD> Pt   { get; set;}
		
		private static CultureInfo ic = CultureInfo.InvariantCulture;
		
		/// <summary>
		/// Create a new empty building data object
		/// </summary>
		public BuildingData()
		{
			Name = "Bu";
			Pt = new List<PointD>();
		}
		
		/// <summary>
		/// Create a new building data object from string
		/// </summary>
		public BuildingData(string sourcedata) // new item from string
		{
			int index = 0;
			int version = 0;
			string[] text = new string[1];
			text = sourcedata.Split(new char[] { ',' });
			Pt = new List<PointD>();
			
			try
			{
				if (text.Length > 5) // otherwise the file is corrupt
				{
					version = Convert.ToInt32(text[index++]);
					Name   = text[index++];
					Height = (float) (St_F.TxtToDbl(text[index++], false));
					LowerBound = (float) (St_F.TxtToDbl(text[index++], false));
					Area   = (float) (St_F.TxtToDbl(text[index++], false));
					
					int vertices_number = Convert.ToInt32(text[index++]); // number of vertices
					
					for (int i = 0; i < vertices_number; i++)
					{
						double x = St_F.TxtToDbl(text[index++], false);
						double y = St_F.TxtToDbl(text[index++], false);
						Pt.Add(new PointD(x, y));
					}
				}
				else
				{
					throw new ArgumentException();
				}
			}
			catch
			{
				Name = String.Empty;
				Pt = new List<PointD>();
			}
		}
		
		/// <summary>
		/// Create a new building data object from other object
		/// </summary>
		public BuildingData(BuildingData other) // Deep copy for new item
		{
			Name = other.Name;
			Area = other.Area;
			Height = other.Height;
			LowerBound = other.LowerBound;
			
			Pt = new List<PointD>();
			foreach (PointD _pt in other.Pt)
			{
				Pt.Add(_pt);
			}			
		}
		
		/// <summary>
		/// Convert object data to a string as used in the item file
		/// </summary>
		public override string ToString()
		{
			if (Name == null || Name == String.Empty)
			{
				Name = "Building";
			}
			
			string dummy = St_F.RemoveinvalidChars(Name) + "," +
				Height.ToString(ic) + "," +
				LowerBound.ToString(ic) + "," +
				Area.ToString(ic) + "," +
				Pt.Count.ToString();
				
			foreach (PointD _pt in Pt)
			{
				dummy += "," + Math.Round(_pt.X, 1).ToString(ic) + "," + Math.Round(_pt.Y, 1).ToString(ic);
			}
			
			return dummy;
		}

		public double CalcArea()
		{
			double area = 0;
			if (Pt.Count > 2)
            {
                for (int i = 0; i < Pt.Count - 1; i++)
                {
                    area += (Pt[i + 1].X - Pt[i].X) * Pt[i].Y + (Pt[i + 1].X - Pt[i].X) * (Pt[i + 1].Y - Pt[i].Y) / 2;
                }
                area = Math.Round(Math.Abs(area), 1);
            }
            return area;
		}
	}
}

