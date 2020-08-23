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
 * Time: 16:24
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GralItemData
{
    /// <summary>
    /// This class represents supports reading/writing the portal source data
    /// </summary>
    public class PortalsDataIO
	{
		public bool LoadPortalSources(List<PortalsData> _data, string _filename) // overload with 2 parameters only
		{
			return LoadPortalSources(_data, _filename, false, new RectangleF());
		}
		
		public bool LoadPortalSources(List<PortalsData> _data, string _filename, bool _filterData, RectangleF _domainRect)
		{
			bool reading_ok = false;
			try
			{
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
								_data.Add(new PortalsData(text));
							}
							else  // filter data -> import data inside domain area
							{
								PortalsData _dta = new PortalsData(text);
								
								if (_domainRect.Contains(_dta.Pt1.ToPointF()) && _domainRect.Contains(_dta.Pt2.ToPointF()))
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
		
		public bool SavePortalSources(List<PortalsData> _data, string _projectPath)
		{
			string newPath = Path.Combine(_projectPath, @"Emissions", "Portalsources.txt");
			bool writing_ok = false;
			try
			{
				using (StreamWriter myWriter = File.CreateText(newPath))
				{
					myWriter.WriteLine("List of all portal sources within the model domain Version_19");
					myWriter.Write(@"Name, Height[m], Exit vel.[m/s], Temp.difference [K], Traffic direction, Crosssection [m²], AADT, HDV[%], slope[%], traffic-situation, baseyear, speed of PC [km/h], #of source groups, source-group-pollutants(10),.....,#of corner-points, corner points (x1[m],y1[m],x2[m],y2[m]), deposition data, Height above ground [m], velocity and temperature time series references");
					myWriter.WriteLine();
					foreach (PortalsData _dta in _data)
					{
						if (_dta.Poll.Count == 0) //if no pollution is defined, might happen when using NEMO
						{
							_dta.Poll.Add(new PollutantsData());
						}
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
