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
 * User: U0178969
 * Date: 30.01.2019
 * Time: 16:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using GralDomForms;

namespace GralItemData
{
    /// <summary>
    /// This class is used for vertical wind velocity and wind direction window
    /// </summary>
    public class VerticalWindProfile
	{
		public int DispersionSituation   { get; set; }
		public int GRALorGRAMM       	 { get; set; }
		public VerticalProfile_Static VertProfileVelocity { get; set; }
        public VerticalProfile_Static VertProfileDirection{ get; set; }
	}
}
