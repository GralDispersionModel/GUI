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
 * Date: 27.10.2018
 * Time: 19:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using GralDomain;
using GralStaticFunctions;

namespace GralItemData
{
	/// <summary>
    /// This class represents the area source data
    /// </summary>
	[Serializable]
	public class AreaSourceData 
	{
		public string Name		{ get; set;}
		public float Height		{ get; set;}
		public float Area		{ get; set;}
		public float VerticalExt{ get; set;}
		public float RasterSize	{ get; set;}
		public List<GralDomain.PointD> Pt  { get; set;}
		public PollutantsData Poll	{ get; set;}

		private Deposition[] dep;
		public Deposition[] GetDep()
		{
			return dep;
		}
		public void SetDep(Deposition[] value)
		{
			dep = value;
		}

		private static CultureInfo ic = CultureInfo.InvariantCulture;
		
		/// <summary>
		/// Create a new empty area source data object
		/// </summary>
		public AreaSourceData()
		{
			Name = "AS";
			Poll = new PollutantsData();
			SetDep(new Deposition[10]);
			VerticalExt = 3;
			Pt = new List<PointD>();
			
			for (int i = 0; i < 10; i++)
			{
				GetDep()[i] = new Deposition(); // initialize Deposition array
				GetDep()[i].init();
			}
		}
		
		/// <summary>
		/// Create a new area source data object from string
		/// </summary>
		public AreaSourceData(string sourcedata) // new item from string
		{
			int index = 0;
			int version = 0;
			string[] text = new string[1];
			text = sourcedata.Split(new char[] { ',' });
			SetDep(new Deposition[10]);
			Pt = new List<PointD>();
			
			try
			{
				if (text.Length > 10) // otherwise the file is corrupt
				{
					version = Convert.ToInt32(text[index++]);
					Name   = text[index++];
					Height      = (float) (St_F.TxtToDbl(text[index++], false));
					VerticalExt = (float) (St_F.TxtToDbl(text[index++], false));
					RasterSize  = (float) (St_F.TxtToDbl(text[index++], false));

                    Poll = new PollutantsData
                    {
                        SourceGroup = Convert.ToInt32(text[index++])
                    };
                    Area = (float) (St_F.TxtToDbl(text[index++], false));
					
					for (int i = 0; i < 10; i++)
					{
						Poll.Pollutant[i]    = Convert.ToInt32(text[index++]);
						Poll.EmissionRate[i] = St_F.TxtToDbl(text[index++], false);
					}
					
					int vertices_number = Convert.ToInt32(text[index++]); // number of vertices
					for (int i = 0; i < vertices_number; i++)
					{
						float x = (float) (St_F.TxtToDbl(text[index++], false));
						float y = (float) (St_F.TxtToDbl(text[index++], false));
						Pt.Add(new PointD(x, y));
					}
					
					int depostart = text.Length;
					for (int i = 4; i < text.Length; i++)
                    {
                        if (text[i] == "Dep@_")
					{
						depostart = i + 1;
						break;
					}
                    }

                    if (text.Length > depostart + 2) // read deposition
					{
						try
						{
							for (int i = 0; i < 10; i++)
							{
								GetDep()[i] = new Deposition(); // initialize Deposition array
								GetDep()[i].String_to_Val(depostart + i * 10, text);
							}
						}
						catch{}
					}
					else
					{
						for (int i = 0; i < 10; i++)
						{
							GetDep()[i] = new Deposition(); // initialize Deposition array
							GetDep()[i].init();
						}
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
				Poll = new PollutantsData();
				SetDep(new Deposition[10]);
				Height = 0;
				VerticalExt = 3;
				Pt = new List<PointD>();
				
				for (int i = 0; i < 10; i++)
				{
					GetDep()[i] = new Deposition(); // initialize Deposition array
					GetDep()[i].init();
				}
			}
		}
		
		/// <summary>
		/// Create a new area source data object as copy from other object
		/// </summary>
		public AreaSourceData(AreaSourceData other) // Deep copy for new item
		{
			Name = other.Name;
			Height = other.Height;
			VerticalExt = other.VerticalExt;
			RasterSize = other.RasterSize;
			Area = other.Area;
			
			Pt = new List<PointD>();
			foreach (PointD _pt in other.Pt)
			{
				Pt.Add(_pt);
			}
			
			Poll = new PollutantsData();
			SetDep(new Deposition[10]);
			
			Poll.SourceGroup = other.Poll.SourceGroup;
			for (int i = 0; i < 10; i++)
			{
				Poll.EmissionRate[i] = other.Poll.EmissionRate[i];
				Poll.Pollutant[i] = other.Poll.Pollutant[i];
			}

			SetDep(new Deposition[10]);
			for (int i = 0; i < 10; i++)
			{
				GetDep()[i] = new Deposition(other.GetDep()[i]); // initialize Deposition array
			}
		}
		
		/// <summary>
		/// Convert object data to a string as used in the item file
		/// </summary>
		public override string ToString()
		{
			if (Name == null || Name == String.Empty)
			{
				Name = "AS";
			}
			
			string dummy = St_F.RemoveinvalidChars(Name) + "," +
				Height.ToString(ic) + "," +
				VerticalExt.ToString(ic) + "," +
				RasterSize.ToString(ic) + "," +
				Poll.SourceGroup.ToString() + "," + 
				Area.ToString(ic) + ",";
			
			for (int i = 0; i < 10; i++)
			{
				dummy += Poll.Pollutant[i].ToString() + "," +
					St_F.DblToIvarTxt(Poll.EmissionRate[i]) + ",";
			}
			
			dummy += Pt.Count.ToString();
			foreach(PointD _pt in Pt)
			{
				dummy += "," + Math.Round(_pt.X, 1).ToString(ic) + "," + Math.Round(_pt.Y, 1).ToString(ic);
			}
			
			dummy += ",Dep@_,";
			for (int i = 0; i < 10; i++)
			{
				dummy += GetDep()[i].ToString() + ",";
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

