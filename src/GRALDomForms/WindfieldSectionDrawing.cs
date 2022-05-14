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
using System.Collections.Generic;

namespace GralDomForms
{
	/// <summary>
	/// Class for windfield section drawings
	/// </summary>       
    public class WindfieldSectionDrawings
    {
        public double GralWest { get; set; }
        public double GralSouth { get; set; }
        public int Nii { get; set; }
        public int Njj { get; set; }
        public int Nkk { get; set; }
        public Single Layer1 { get; set; }
        public Single Stretch { get; set; }
        public List<float[]> StretchFlexible { get; set; }
        public Single AHmin { get; set; }
        public double cellsize {get; set;}
        public double X0 { get; set; }
        public double Y0 { get; set; }
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public string Path { get; set; }
        public string GRAMM_path { get; set; }
        public string PROJECT_path { get; set; }
        public double GrammWest { get; set; }
        public double GrammSouth { get; set; }
        public double GrammCellsize { get; set; }
        public double WindSectorSize { get; set; }
    }

}
