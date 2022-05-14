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
using System.Globalization;
using GralDomain;
using GralStaticFunctions;

namespace GralItemData
{
	/// <summary>
    /// This class represents the point source data
    /// </summary>
	[Serializable]
	public class PointSourceData
	{
		public string Name		{ get; set;}
		public float Height		{ get; set;}
		public float Velocity   { get; set;}
		public float Temperature{ get; set;}
		public float Diameter	{ get; set;}
		public GralDomain.PointD Pt 		{ get; set;}
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

		public string VelocityTimeSeries		{ get; set;}
		public string TemperatureTimeSeries	{ get; set;}
		
		private static CultureInfo ic = CultureInfo.InvariantCulture;
		
		/// <summary>
		/// Create a new empty point source data object
		/// </summary>
		public PointSourceData()
		{
			Name = "PS";
			Poll = new PollutantsData();
			SetDep(new Deposition[10]);
			Temperature = 20;
			Diameter = 0.2F;
			Height = 10;
			Pt = new PointD(0, 0);
			
			for (int i = 0; i < 10; i++)
			{
				GetDep()[i] = new Deposition(); // initialize Deposition array
				GetDep()[i].init();
			}
		}
		
		/// <summary>
		/// Create a new point source data object from string
		/// </summary>
		public PointSourceData(string sourcedata) // new item from string
		{
			int index = 0;
			int version = 0;
			string[] text = new string[1];
			text = sourcedata.Split(new char[] { ',' });
			SetDep(new Deposition[10]);
			Pt = new PointD(0, 0);
			
			try
			{
				if (text.Length > 10) // otherwise the file is corrupt
				{
					version = Convert.ToInt32(text[index++]);
					Name   = text[index++];
					double x = St_F.TxtToDbl(text[index++], false);
					double y = St_F.TxtToDbl(text[index++], false);
					Pt = new PointD(x, y);
					Height      = (float) (St_F.TxtToDbl(text[index++], false));
					Velocity    = (float) (St_F.TxtToDbl(text[index++], false));
					Temperature = (float) (St_F.TxtToDbl(text[index++], false));
					Diameter = (float) (St_F.TxtToDbl(text[index++], false));

                    Poll = new PollutantsData
                    {
                        SourceGroup = Convert.ToInt32(text[index++])
                    };
                    for (int i = 0; i < 10; i++)
					{
						Poll.Pollutant[i]    = Convert.ToInt32(text[index++]);
						Poll.EmissionRate[i] = St_F.TxtToDbl(text[index++], false);
					}
					
					{
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
					
					for (int i = text.Length - 1; i > 3; i--)
					{
						if (text[i] == "Vel@_" && text.Length > (i + 1))
						{
							VelocityTimeSeries = text[i + 1];
							break;
						}
					}
					
					for (int i = text.Length - 1; i > 3; i--)
					{
						if (text[i] == "Temp@_" && text.Length > (i + 1))
						{
							TemperatureTimeSeries = text[i + 1];
							break;
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
				Temperature = 20;
				Diameter = 0.2F;
				Height = 10;
				Pt = new PointD(0, 0);
				
				for (int i = 0; i < 10; i++)
				{
					GetDep()[i] = new Deposition(); // initialize Deposition array
					GetDep()[i].init();
				}
			}
		}
		
		/// <summary>
		/// Create a new point source data object from other object
		/// </summary>
		public PointSourceData(PointSourceData other) // Deep copy for new item
		{
			Name = other.Name;
			Temperature = other.Temperature;
			Diameter = other.Diameter;
			Height = other.Height;
			Velocity = other.Velocity;
		     
			Pt = other.Pt;
			
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
			
			if (other.TemperatureTimeSeries != null)
			{
				TemperatureTimeSeries = other.TemperatureTimeSeries;
			}
			if (other.VelocityTimeSeries != null)
			{
				VelocityTimeSeries = other.VelocityTimeSeries;
			}
		}
		
		/// <summary>
		/// Convert object data to a string as used in the item file
		/// </summary>
		public override string ToString()
		{
			if (Name == null || Name == String.Empty)
			{
				Name = "PS";
			}
			
			string dummy = St_F.RemoveinvalidChars(Name) + "," +
				Math.Round(Pt.X, 1).ToString(ic) + "," + Math.Round(Pt.Y, 1).ToString(ic) + "," +
				Height.ToString(ic) + "," +
				Velocity.ToString(ic) + "," +
				Temperature.ToString(ic) + "," +
				Diameter.ToString(ic) + "," +
				Poll.SourceGroup.ToString() + ",";
			
			for (int i = 0; i < 10; i++)
			{
				dummy += Poll.Pollutant[i].ToString() + "," +
					St_F.DblToIvarTxt(Poll.EmissionRate[i]) + ",";
			}
			
			dummy += "Dep@_,";
			for (int i = 0; i < 10; i++)
			{
				dummy += GetDep()[i].ToString() + ",";
			}
			
			if (string.IsNullOrEmpty(VelocityTimeSeries) == false)
			{
				dummy += "Vel@_," + VelocityTimeSeries + ",";
			}
			
			if (string.IsNullOrEmpty(TemperatureTimeSeries) == false)
			{
				dummy += "Temp@_," + TemperatureTimeSeries + ",";
			}
			
			return dummy;
		}
	}
}
