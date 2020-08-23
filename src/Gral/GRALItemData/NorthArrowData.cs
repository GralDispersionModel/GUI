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
 * User: Markus
 * Date: 31.01.2019
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace GralItemData
{
    /// <summary>
    /// This class holds the data for the north arrow bitmap
    /// </summary>
    public class NorthArrowData
	{
    	public int X     { get; set; }                     	   //x-coordinate of north arrow
        public int Y     { get; set; }                         //y-coordinate of north arrow
        public decimal Scale  { get; set; }                    //scale of north arrow
        
        public NorthArrowData()
        {
        	X = 0;
        	Y = 0;
        	Scale = 1;
        }
	}
}