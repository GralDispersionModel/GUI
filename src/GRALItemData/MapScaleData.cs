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

namespace GralItemData
{
    /// <summary>
    /// This class holds the data for the map scale bar
    /// </summary>
    public class MapScaleData
	{
    	public int X     { get; set; }                     	   //x-coordinate of map scale bar
        public int Y     { get; set; }                         //y-coordinate of map scale bar
        public int Division  { get; set; }                     //number of divisions of the map scale bar
        public int Length    { get; set; }                     //lenght in m of the map scale bar
        public int RelativeTo { get; set; }                   // position of scale relative to the map or the screen
        public const int ToScreen = 0;
        public const int ToMap = 1;

        public MapScaleData()
        {
        	X = 0;
        	Y = 0;
        	Division = 3;
        	Length = 100;
            RelativeTo = ToScreen;
        }
	}
}
