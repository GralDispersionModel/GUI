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
 * Date: 14.10.2018
 * Time: 16:29
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace GralItemData
{
	/// <summary>
	/// This class represents the pollution data; Source group number is limited between 1 to 99
	/// </summary>
	[Serializable]
	public class PollutantsData
	{
		private int _sourcegroup = 1;
		
		public int SourceGroup
		{
			get
			{
				return _sourcegroup;
			}
			set
			{
				if (value < 1)
				{
					_sourcegroup = 1;
				}
				else if (value > 99)
				{
					_sourcegroup = 99;
				}
				else
				{
					_sourcegroup = value;
				}
			}
		}
		public int[] Pollutant       { get; set; }
		public double[] EmissionRate { get; set; }
		
		public PollutantsData()
		{
			SourceGroup = 0;
			Pollutant = new int[10];
			EmissionRate = new double[10];
		}
		
		public PollutantsData(int SorceGroup)
		{
			SourceGroup = SourceGroup;
			Pollutant = new int[10];
			EmissionRate = new double[10];
		}
		
		public PollutantsData(PollutantsData other)
		{
			SourceGroup = other.SourceGroup;
			Pollutant = new int[10];
			EmissionRate = new double[10];
			for (int i = 0; i <10; i++)
			{
				Pollutant[i] = other.Pollutant[i];
				EmissionRate[i] = other.EmissionRate[i];
			}
		}
	}
}
