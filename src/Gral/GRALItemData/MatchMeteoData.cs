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

namespace GralItemData
{
    /// <summary>
    /// Contains the data for added meteo stations to the match dialog
    /// </summary>
    public class MatchMeteoData
	{
		public int FileLenght     	{ get; set; }

		private int[] hour;
		public int[] GetHour()
		{
			return hour;
		}
    	public void SetHour(int[] value)
		{
			hour = value;
		}

		private double[] windVel;
		public double[] GetWindVel()
		{
			return windVel;
		}
		public void SetWindVel(double[] value)
		{
			windVel = value;
		}

		private double[] windDir;
		public double[] GetWindDir()
		{
			return windDir;
		}
		public void SetWindDir(double[] value)
		{
			windDir = value;
		}
		private int[] sC;
		public int[] GetSC()
		{
			return sC;
		}
		public void SetSC(int[] value)
		{
			sC = value;
		}

		private string[] date;
		public string[] GetDate()
		{
			return date;
		}
		public void SetDate(string[] value)
		{
			date = value;
		}

		private string[] time;
		public string[] GetTime()
		{
			return time;
		}
		public void SetTime(string[] value)
		{
			time = value;
		}

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
			SetHour(new int[1000000]);
			SetWindVel(new double[1000000]);
			SetWindDir(new double[1000000]);
			SetSC(new int[1000000]);
			SetDate(new string[1000000]);
			SetTime(new string[1000000]);
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
