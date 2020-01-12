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
 * Time: 18:03
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace GralItemData
{
	/// <summary>
	/// Contains the data for added meteo stations to the match dialog
	/// </summary>
	public class MatchMeteoData
	{
		public int FileLenght     	{ get; set; }
		public int[] Hour 			{ get; set; }
        public double[] WindVel		{ get; set; }
        public double[] WindDir		{ get; set; }
        public int[] SC 			{ get; set; }
        public string[] Date 		{ get; set; }
        public string[] Time 		{ get; set; }
        public string MetFileExt 	{ get; set; }
        public char MetColumnSeperator { get; set; }
        public string MetDecSeperator  { get; set; }
        public double AnemometerHeight { get; set; }
        public int WsClasses 		{ get; set; }
        public int WdClasses 		{ get; set; }
        public bool Meteo			{ get; set; }
		
        /// <summary>
	    /// Contains the data for added meteo stations to the match dialog
	    /// </summary>
		public MatchMeteoData()
		{
			FileLenght = 0;
			Hour = new int[1000000];
			WindVel = new double[1000000];
			WindDir = new double[1000000];
			SC = new int[1000000];
			Date = new string[1000000];
			Time = new string[1000000];
			MetFileExt = string.Empty;
			MetColumnSeperator = ',';
			MetDecSeperator = ".";
			AnemometerHeight = 0;
			WsClasses = 6;
			WdClasses = 36;
			Meteo = false;
		}
	}
}
