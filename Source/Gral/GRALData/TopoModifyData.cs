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


// Class used by the Modify Topography class 
namespace GralData
{
	/// <summary>
	/// Class that holds data for the topograhy modification
	/// </summary>
	public class TopoModifyClass
	{
		public float Height = 0;
        public bool AbsoluteHeight = false;
        public int Raster = 0;
        public float Hmax = 10000;
        public float Hmin = 0;
    }
}
