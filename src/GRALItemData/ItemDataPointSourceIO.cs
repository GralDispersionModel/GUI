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

using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GralItemData
{
    /// <summary>
    /// This class represents supports reading/writing the point source data
    /// </summary>
    public class PointSourceDataIO
	{
		public bool LoadPointSources(List<PointSourceData> _data, string _filename) // overload with 2 parameters only
		{
			return LoadPointSources(_data, _filename, false, new RectangleF());
		}
		
		public bool LoadPointSources(List<PointSourceData> _data, string _filename, bool _filterData, RectangleF _domainRect)
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
								_data.Add(new PointSourceData(text));
							}
							else  // filter data -> import data inside domain area
							{
								PointSourceData _dta = new PointSourceData(text);
								PointF _ptin = new PointF((float) _dta.Pt.X, (float) _dta.Pt.Y);
								if (_domainRect.Contains(_ptin))
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
		
		public bool SavePointSources(List<PointSourceData> _data, string _projectPath)
		{
			string newPath = Path.Combine(_projectPath, @"Emissions", "Psources.txt");
			bool writing_ok = false;
			try
			{
				using (StreamWriter myWriter = File.CreateText(newPath))
				{
					myWriter.WriteLine("List of all point sources within the model domain Version_19");
					myWriter.Write("Name, x, y, z, exit-velocity [m/s], exit-temp[K], stack-diameter[m], source-group, pollutants(10), deposition parameters, velocity and temperature time series references");
					myWriter.WriteLine();
					foreach (PointSourceData _dta in _data)
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
