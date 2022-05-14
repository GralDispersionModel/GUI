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
    /// This class holds the data for the map size
    /// </summary>
    public class MapSizes
	{
    	private double _west = 0;
    	private double _north = 0;
    	private double _sizeX = 1;
    	private double _sizeY = -1;
    	
    	public double West     { get {return _west;} set {_west = value;}}     // Western border of map
    	public double North    { get {return _north;} set {_north = value;}}   // Northern border of map
        public double SizeX    { get {return _sizeX;}                          // Natural Pixel size of map in E-W direction
            set
            {
                if (value == 0)
                {
                    _sizeX = 1E-6;
                }
                else
                {
                    _sizeX = value;
                }
            }
        }
        public double SizeY    { get {return _sizeY;}                          // Natural Pixel size of map in N-S direction
            set
            {
                if (value == 0)
                {
                    _sizeY = 1E-6;
                }
                else
                {
                    _sizeY = value;
                }
            }
        }
    	
    	public MapSizes()
    	{
    		West = 0;
    		North = 0;
    		SizeX = 1;
			SizeY = -1;
    	}
        
	}
}
