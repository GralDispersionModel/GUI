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
 * Time: 10:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GralItemData
{
	/// <summary>
    /// This class represents supports reading/writing the receptor data
    /// </summary>
	public class ReceptorDataIO
	{
		public bool LoadReceptors(List<ReceptorData> _data, string _filename) // overload with 2 parameters only
		{
			return LoadReceptors(_data, _filename, false, new RectangleF());
		}
		
		public bool LoadReceptors(List<ReceptorData> _data, string _filename, bool _filterData, RectangleF _domainRect)
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
						
						while (myReader.EndOfStream == false) // read until EOF
						{
							text = version.ToString() + "," + myReader.ReadLine(); // read data and add version number
							
							if (_filterData == false)
							{
								_data.Add(new ReceptorData(text));
							}
							else  // filter data -> import data inside domain area
							{
								ReceptorData _dta = new ReceptorData(text);
								
								if (_domainRect.Contains(_dta.Pt.ToPointF()))
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
		
		public bool SaveReceptor(List<ReceptorData> _data, string _projectPath)
		{
			string newPath = Path.Combine(_projectPath, @"Computation","Receptor.dat");
			bool writing_ok = false;
			try
			{
				using (StreamWriter myWriter = File.CreateText(newPath))
				{
					myWriter.WriteLine(_data.Count.ToString());
					int count = 0;
					foreach (ReceptorData _dta in _data)
					{
						count++; 
						myWriter.WriteLine(count.ToString() + "," + _dta.ToString());
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
