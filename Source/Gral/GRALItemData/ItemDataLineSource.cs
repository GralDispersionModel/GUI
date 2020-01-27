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
 * Date: 26.10.2018
 * Time: 12:27
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using GralStaticFunctions;
using GralDomain;

namespace GralItemData
{
	/// <summary>
    /// This class represents the line source data
    /// </summary>
	[Serializable]
	public class LineSourceData
	{
		public string Name		{ get; set;}
		public string Section		{ get; set;}
		public float Height		{ get; set;}
		public float VerticalExt{ get; set;}
		public float Width   	{ get; set;}
		public bool  Lines3D    { get; set;}
		public List<GralData.PointD_3d> Pt  { get; set;}
		public List <PollutantsData> Poll	{ get; set;}
		public NemoData Nemo		{ get; set;}
		public Deposition[] Dep	{ get; set;}
		
		private static CultureInfo ic = CultureInfo.InvariantCulture;
		
		/// <summary>
		/// Create a new empty line source data object
		/// </summary>
		public LineSourceData()
		{
			Name = "LS";
			Section = "1";
			VerticalExt = 3;
			Width = 7;
			Lines3D = false;
			Poll = new List<PollutantsData>();
			Nemo = new NemoData();
			Dep = new Deposition[10];
			Pt = new List<GralData.PointD_3d>();
			
			for (int i = 0; i < 10; i++)
			{
				Dep[i] = new Deposition(); // initialize Deposition array
				Dep[i].init();
			}
		}
		
		/// <summary>
		/// Create a new line source data object from string
		/// </summary>
		public LineSourceData(string sourcedata) // new item from string
		{
			int index = 0;
			int version = 0;
			string[] text = new string[1];
			text = sourcedata.Split(new char[] { ',' });
			Dep = new Deposition[10];
			Nemo = new NemoData();
			Pt = new List<GralData.PointD_3d> ();
			
			try
			{
				if (text.Length > 15) // otherwise the file is corrupt
				{
					version = Convert.ToInt32(text[index++]);
					Name   = text[index++];
					Section = text[index++];
					Height = (float) (St_F.TxtToDbl(text[index++], false));
					
					Width = (float) (St_F.TxtToDbl(text[index++], false));
					Nemo.AvDailyTraffic = Convert.ToInt32(text[index++]);
					Nemo.ShareHDV = (float)(St_F.TxtToDbl(text[index++], false));
					Nemo.Slope = (float)(St_F.TxtToDbl(text[index++], false));
					Nemo.TrafficSit = Convert.ToInt32(text[index++]);
					Nemo.BaseYear = Convert.ToInt32(text[index++]);
					
					int sg_count = Convert.ToInt32(text[index++]);
					
					if (sg_count > 0)
					{
						Poll = new List<PollutantsData>();
						for (int sg = 0; sg < sg_count; sg++)
						{
                            PollutantsData _poll = new PollutantsData
                            {
                                SourceGroup = Convert.ToInt32(text[index++])
                            };
                            for (int i = 0; i < 10; i++)
							{
								_poll.Pollutant[i]    = Convert.ToInt32(text[index++]);
								_poll.EmissionRate[i] = St_F.TxtToDbl(text[index++], false);
							}
							Poll.Add(_poll);
						}
					}

					if (version > 1)
					{
						int _3dLines = Convert.ToInt32(text[index++]);
						if (_3dLines == 1)
						{
							Lines3D = true;
						}
					}
					
					int vertices_number = Convert.ToInt32(text[index++]); // number of vertices
					
					for (int i = 0; i < vertices_number; i++)
					{
						double x = St_F.TxtToDbl(text[index++], false);
						double y = St_F.TxtToDbl(text[index++], false);
						double z = Height;
						if (version > 1)
						{
							z = St_F.TxtToDbl(text[index++], false);
						}
						Pt.Add(new GralData.PointD_3d(x, y, z));
					}
					
					int depostart = text.Length;
					for (int i = 4; i < text.Length; i++)
						if (text[i] == "Dep@_")
					{
						depostart = i + 1;
						break;
					}
					if (text.Length > depostart + 2) // read deposition
					{
						try
						{
							for (int i = 0; i < 10; i++)
							{
								Dep[i] = new Deposition(); // initialize Deposition array
								Dep[i].String_to_Val(depostart + i * 10, text);
							}
						}
						catch{}
					}
					else
					{
						for (int i = 0; i < 10; i++)
						{
							Dep[i] = new Deposition(); // initialize Deposition array
							Dep[i].init();
						}
					}
										
					try
					{
						// get vertical extension
						if (text.Length > depostart + 10 * 10) // read vertical extension
						{
							VerticalExt = (float) (St_F.TxtToDbl(text[depostart + 10 * 10], false));
							if (VerticalExt < 0.1F) // compatibility to old projects
								VerticalExt = 3;
						}
						else
							VerticalExt = 3; // old standard value = 3 m
					}
					catch
					{
						VerticalExt = 3; // old standard value = 3 m
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
				VerticalExt = 3;
				Width = 7;
				
				Poll.Clear();
				
				Poll = new List<PollutantsData>();
				Nemo = new NemoData();
				Dep = new Deposition[10];
				Lines3D = false;
				Pt = new List<GralData.PointD_3d>();
				
				for (int i = 0; i < 10; i++)
				{
					Dep[i] = new Deposition(); // initialize Deposition array
					Dep[i].init();
				}
			}
		}
		
		/// <summary>
		/// Create a new line source data object from other object
		/// </summary>
		public LineSourceData(LineSourceData other) // Deep copy for new item
		{
			Name = other.Name;
			VerticalExt = other.VerticalExt;
			Height = other.Height;
			Section = other.Section;
			Width = other.Width;
			Pt = new List<GralData.PointD_3d>();
			foreach (GralData.PointD_3d _pt in other.Pt)
			{
				Pt.Add(_pt);
			}
			
			Poll = new List<PollutantsData>();
			foreach (PollutantsData _poll in other.Poll)
			{
				PollutantsData _pollnew = new PollutantsData();
				for (int i = 0; i < 10; i++)
				{
					_pollnew.EmissionRate[i] = _poll.EmissionRate[i];
					_pollnew.Pollutant[i] = _poll.Pollutant[i];
				}
				_pollnew.SourceGroup = _poll.SourceGroup;
				Poll.Add(_pollnew);
			}
						
			Nemo = new NemoData(other.Nemo);
			Dep = new Deposition[10];
			for (int i = 0; i < 10; i++)
			{
				Dep[i] = new Deposition(other.Dep[i]); // initialize Deposition array
			}
		}
		
		/// <summary>
		/// Convert object data to a string as used in the item file
		/// </summary>
		/// <param name="version">0 and 1: return 2D line format, 2: write 3D format</param>
		public string ToString(int version)
		{
			if (Name == null || Name == String.Empty)
			{
				Name = "LS";
			}
			
			string dummy = St_F.RemoveinvalidChars(Name) + "," +
				Convert.ToString(Section) + "," +
				St_F.DblToIvarTxt(Math.Round(Height, 1)) + "," +
				St_F.DblToIvarTxt(Math.Round(Width, 1)) + "," +
				Nemo.ToString() + "," +
				Convert.ToString(Poll.Count) + ",";
			
			foreach (PollutantsData _poll in Poll)
			{
				dummy += _poll.SourceGroup.ToString() + ",";
				for (int i = 0; i < 10; i++)
				{
					dummy += _poll.Pollutant[i].ToString() + "," +
						St_F.DblToIvarTxt(_poll.EmissionRate[i]) + ",";
				}
			}

			if (version > 1)
			{
				if (Lines3D)
				{
					dummy += "1,";
				}
				else
				{
					dummy += "0,";
				}
			}

			dummy += Pt.Count.ToString();
			if (version > 1)
			{
				foreach (GralData.PointD_3d _pt in Pt)
				{
					dummy += "," + Math.Round(_pt.X, 1).ToString(ic) + "," + Math.Round(_pt.Y, 1).ToString(ic) + "," + Math.Round(_pt.Z, 1).ToString(ic);
				}
			}
			else
			{
				foreach (GralData.PointD_3d _pt in Pt)
				{
					dummy += "," + Math.Round(_pt.X, 1).ToString(ic) + "," + Math.Round(_pt.Y, 1).ToString(ic);
				}
			}
			
			dummy += ",Dep@_,";
			for (int i = 0; i < 10; i++)
			{
				dummy += Dep[i].ToString() + ",";
			}
			
			dummy += St_F.DblToIvarTxt(Math.Round(VerticalExt, 1)) + ",";
			return dummy;
		}

		public double Lenght()
		{
			if (Pt != null)
			{
				return St_F.CalcLenght(Pt); 
			}
			else
			{
				return 0;
			}
		}
		
		public double Area()
		{
			return Width * Lenght();
		}
	}
}
