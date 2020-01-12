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
 * User: MaKuAdmin
 * Date: 02.02.2019
 * Time: 19:01
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace GralData
{
    /// <summary>
    /// Data container for meteopgt.all lines with sortable frequency 
    /// </summary>
    public class DomainArea
	{
		public double North{ get; set; }
		public double East{ get; set; }
		public double South{ get; set; }
		public double West{ get; set; }
		
		public DomainArea()
		{
			North = 0;
			West = 0;
			East = 0;
			South = 0;
		}
	}
}
