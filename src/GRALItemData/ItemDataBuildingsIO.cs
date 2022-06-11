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
using Gral;
using GralDomain;

namespace GralItemData
{
    /// <summary>
    /// This class represents supports reading/writing the buidling data
    /// </summary>
    public class BuildingDataIO
    {
        public bool LoadBuildings(List<BuildingData> _data, string _filename)
        {
            return LoadBuildings(_data, _filename, false, new RectangleF());
        }
        
        public bool LoadBuildings(List<BuildingData> _data, string _filename, bool _filterData, RectangleF _domainRect)
        {
            bool reading_ok = false;
            try
            {
                string _file = Path.Combine(_filename,"Emissions","Buildings.txt");
                if (File.Exists(_file) == false) // try old Computation path
                {
                    _file = Path.Combine(_filename,"Computation","Buildings.txt");
                }
                
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
                                _data.Add(new BuildingData(text));
                            }
                            else  // filter data -> import data inside domain area
                            {
                                BuildingData _dta = new BuildingData(text);
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
        
        public bool SaveBuildings(List<BuildingData> _data, string _projectPath, bool Compatibility)
        {
            bool writing_ok = false;
            string newPath = Path.Combine(_projectPath, @"Emissions", "Buildings.txt");
            writing_ok = WriteFile(_data, newPath);
            if (Compatibility)
            {
                newPath = Path.Combine(Main.ProjectName,"Computation","Buildings.txt");
                writing_ok = WriteFile(_data, newPath);
            }
            return writing_ok;
        }
        
        private bool WriteFile(List<BuildingData> _data, string newPath)
        {
            bool ok = false;
            try
            {
                using (StreamWriter myWriter = File.CreateText(newPath))
                {
                    myWriter.WriteLine("List of all buildings within the model domain");
                    myWriter.Write("Name, Top-height[m], Bottom-heigth[m], Area, Number of vertices, Corner points (x[m],y[m])");
                    myWriter.WriteLine();
                    foreach (BuildingData _dta in _data)
                    {
                        myWriter.WriteLine(_dta.ToString());
                    }
                }
                ok = true;
            }
            catch
            {	
            }
            return ok;
        }
    }
}

