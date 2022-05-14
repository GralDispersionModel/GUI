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
using GralStaticFunctions;
using GralDomain;

namespace GralItemData
{
	/// <summary>
    /// This class represents the vegetation data
    /// </summary>
	[Serializable]
	public class VegetationData 
	{
		public string Name		{ get; set;}
		public float Area		{ get; set;}
		public float VerticalExt{ get; set;}
		public float TrunkZone	{ get; set;}
		public float LADTrunk	{ get; set;}
		public float LADCrown	{ get; set;}
		public float Coverage	{ get; set;}
		public List<GralDomain.PointD> Pt  { get; set;}
		
		private static readonly CultureInfo ic = CultureInfo.InvariantCulture;
		
		/// <summary>
		/// Create a new empty vegetation data object
		/// </summary>
		public VegetationData()
		{
			Name = "Veg";
			Pt = new List<PointD>();
		}
		
		/// <summary>
		/// Create a new vegetation data object from string
		/// </summary>
		public VegetationData(string sourcedata) // new item from string
		{
			int index = 0;
			int version = 0;
			string[] text = new string[1];
			text = sourcedata.Split(new char[] { ',' });
			Pt = new List<PointD>();
			
			try
			{
				if (text.Length > 10) // otherwise the file is corrupt
				{
					version = Convert.ToInt32(text[index++]);
					Name   = text[index++];
					int vertices_number = Convert.ToInt32(text[index++]); // number of vertices
					Area = (float) (St_F.TxtToDbl(text[index++], false));
					VerticalExt = (float) (St_F.TxtToDbl(text[index++], false));
					TrunkZone  = (float) (St_F.TxtToDbl(text[index++], false));
					LADTrunk  = (float) (St_F.TxtToDbl(text[index++], false));
					LADCrown  = (float) (St_F.TxtToDbl(text[index++], false));
					Coverage  = (float) (St_F.TxtToDbl(text[index++], false));
					
					index += 2; // 2 empty fields
					
					for (int i = 0; i < vertices_number; i++)
					{
						float x = (float) (St_F.TxtToDbl(text[index++], false));
						float y = (float) (St_F.TxtToDbl(text[index++], false));
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
		/// Create a new vegetation data object from other object
		/// </summary>
		public VegetationData(VegetationData other) // Deep copy for new item
		{
			Name = other.Name;
			VerticalExt = other.VerticalExt;
			Area = other.Area;
			TrunkZone = other.TrunkZone;
			LADTrunk  = other.LADTrunk;
			LADCrown  = other.LADCrown;
			
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
				Name = "Vegetation";
			}
			
			string dummy = St_F.RemoveinvalidChars(Name) + "," +
				Pt.Count.ToString() + "," +
				Area.ToString(ic) + "," +
				VerticalExt.ToString(ic) + "," +
				TrunkZone.ToString(ic) + "," +  
				LADTrunk.ToString(ic) + "," + 
				LADCrown.ToString(ic) + "," + 
				Coverage.ToString(ic) + ", , ";
			
			foreach(PointD _pt in Pt)
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
