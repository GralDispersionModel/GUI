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
 * Date: 31.10.2018
 * Time: 09:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Globalization;

using GralData;
using GralDomain;
using GralStaticFunctions;

namespace GralItemData
{
	/// <summary>
    /// This class represents the wall data
    /// </summary>
	[Serializable]
	public class WallData 
	{
		public string Name			{ get; set;}
		public float Lenght 		{ get; set;}
		public List<PointD_3d> Pt   { get; set;}
		
		private static readonly CultureInfo ic = CultureInfo.InvariantCulture;
		
		/// <summary>
		/// Create a new empty wall data object
		/// </summary>
		public WallData()
		{
			Name = "Wall";
			Pt = new List<PointD_3d>();
		}
		
		/// <summary>
		/// Create a new wall data object from string
		/// </summary>
		public WallData(string sourcedata) // new item from string
		{
			int index = 0;
			int version = 0;
			string[] text = new string[1];
			text = sourcedata.Split(new char[] { ',' });
			Pt = new List<PointD_3d>();
			
			try
			{
				if (text.Length > 8) // otherwise the file is corrupt
				{
					version = Convert.ToInt32(text[index++]);
					Name   = text[index++];
					int vertices_number = Convert.ToInt32(text[index++]); // number of vertices
					Lenght = (float) (St_F.TxtToDbl(text[index++], false));
					
					for (int i = 0; i < vertices_number; i++)
					{
						float x = (float) (St_F.TxtToDbl(text[index++], false));
						float y = (float) (St_F.TxtToDbl(text[index++], false));
						float z = (float) (St_F.TxtToDbl(text[index++], false));
						Pt.Add(new PointD_3d(x, y, z));
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
				Pt = new List<PointD_3d>();
			}
		}
		
		/// <summary>
		/// Create a new wall data object from other object
		/// </summary>
		public WallData(WallData other) // Deep copy for new item
		{
			Name = other.Name;
			Lenght = other.Lenght;
			Pt = new List<PointD_3d>();
			foreach (PointD_3d _pt in other.Pt)
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
				Name = "Wall";
			}
			
			string dummy = St_F.RemoveinvalidChars(Name) + "," +
				Pt.Count.ToString() + "," +
				Lenght.ToString(ic);
			
			foreach (PointD_3d _pt in Pt)
			{
				dummy += "," + Math.Round(_pt.X, 1).ToString(ic) + "," + Math.Round(_pt.Y, 1).ToString(ic) + "," + Math.Round(_pt.Z, 1).ToString(ic);
			}
			
			return dummy;
		}
		
		public double CalcLenght()
		{
			if (Pt != null)
			{
				List<PointD> _pt = new List<PointD>();
				foreach(PointD_3d _pt3D in Pt)
				{
					_pt.Add(new PointD(_pt3D.X, _pt3D.Y));
				}
				double lenght = St_F.CalcLenght(_pt);
				_pt.Clear();
				_pt.TrimExcess();
				_pt = null;
				return lenght;
			}
			else
			{
				return 0;
			}
		}
	}
}

