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
using System.Drawing;
using System.IO;
using GralDomain;

namespace GralItemData
{
	/// <summary>
    /// This class represents supports reading/writing the line source data
    /// </summary>
	public class LineSourceDataIO
	{
				
		public bool LoadLineSources(List<LineSourceData> _data, string _filename) // overload with 2 parameters only
		{
			return LoadLineSources(_data, _filename, false, new RectangleF());
		}
		
		public bool LoadLineSources(List<LineSourceData> _data, string _filename, bool _filterData, RectangleF _domainRect)
		{
			bool reading_ok = false;
			try
			{
				//string _file = Path.Combine(_filename,"Emissions","Lsources.txt");
				if (File.Exists(_filename) && _data != null)
				{
					using(StreamReader myReader = new StreamReader(_filename))
					{
						string text; // read header
						int version = 0;
						text = myReader.ReadLine(); // header 1st line
						if (text.EndsWith("Version_19"))
						{
							version = 1;
						}
						text = myReader.ReadLine(); // header 2nd line
						
						while (myReader.EndOfStream == false) // read until EOF
						{
							text = version.ToString() + "," + myReader.ReadLine(); // read data and add version number
							
							if (_filterData == false)
							{
								_data.Add(new LineSourceData(text));
							}
							else  // filter data -> import data inside domain area
							{
								LineSourceData _dta = new LineSourceData(text);
								bool inside = false;
								foreach(PointD _pti in _dta.Pt)
								{
									if (_domainRect.Contains(_pti.ToPointF()))
									{
										inside = true;
										break;
									}
								}
								if (inside)
								{
									_data.Add(_dta);
								}
							}
						}
					}
					reading_ok = true;
				}
			}
			catch
			{
				reading_ok = false;
			}
			
			return reading_ok;
		}
		
		public bool SaveLineSources(List<LineSourceData> _data, string _projectPath)
		{
			string newPath = Path.Combine(_projectPath, @"Emissions", "Lsources.txt");
			bool writing_ok = false;
			try
			{
				using (StreamWriter myWriter = File.CreateText(newPath))
				{
					myWriter.WriteLine("List of all line sources within the model domain Version_19");
					myWriter.Write("Name, section, height[m], width[m], AADT, HDV[%], slope[%], traffic-situation, baseyear,#of source groups, source-group-pollutants(10),.....,#of corner-points, corner points (x[m],y[m]),...., deposition data, Vertical Extension[m]");
					myWriter.WriteLine();
					foreach (LineSourceData _dta in _data)
					{
						myWriter.WriteLine(_dta.ToString());
					}
				}
				writing_ok = true;
			}
			catch
			{	
			}
			return writing_ok;
		}
	}
}
