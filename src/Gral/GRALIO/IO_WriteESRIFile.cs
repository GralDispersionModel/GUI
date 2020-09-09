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
 * User: Markus Kuntner
 * Date: 15.01.2019
 * Time: 18:29
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Text;

namespace GralIO
{
    /// <summary>
    /// Write ASCII ESRI File for float or double arrays
    /// </summary>
   public class WriteESRIFile
	{
		private string _filename;
		public string FileName {set {_filename = value;}}
		private int _ncols;
		public int NCols {set {_ncols = value;}}
		private int _nrows;
		public int NRows {set {_nrows = value;}}
		private double _xllcorner;
		public double XllCorner {set {_xllcorner = value;}}
		private double _yllcorner;
		public double YllCorner {set {_yllcorner = value;}}
		private double _Cellsize;
		public double CellSize {set {_Cellsize = value;}}
		private string _unit;
		public string Unit {set {_unit = value;}}
	    private int _round = 3;
		public int Round {set {_round = value;}}
		
		private int _z;
		public int Z {set {_z = value;}}
		
		public float[][][] Values;
		public float[,] TwoDim;
		public double [,] DblArr; 
		
		// Write float result files to disc
		/// <summary>
    	/// Write an ESRII ASCII File with header and unit for 2 or 3 dimensional float arrays Values or TwoDim
    	/// </summary>
		public bool WriteFloatResult()
		{
			CultureInfo ic = CultureInfo.InvariantCulture;
			try
			{
				if (File.Exists(_filename))
				{
					try{
						File.Delete(_filename);
					}
					catch{}
				}
					
				using (StreamWriter myWriter = new StreamWriter(_filename))
				{
					// Header
					myWriter.WriteLine("ncols         " + Convert.ToString(_ncols, ic));
					myWriter.WriteLine("nrows         " + Convert.ToString(_nrows, ic));
					myWriter.WriteLine("xllcorner     " + Convert.ToString(_xllcorner, ic));
					myWriter.WriteLine("yllcorner     " + Convert.ToString(_yllcorner, ic));
					myWriter.WriteLine("cellsize      " + Convert.ToString(_Cellsize, ic));
					if (_unit.Length > 0)
                    {
                        myWriter.WriteLine("NODATA_value  " + "-9999 \t Unit:\t" + _unit);
                    }
                    else
                    {
                        myWriter.WriteLine("NODATA_value  " + "-9999");
                    }

                    StringBuilder SB = new StringBuilder();
					for (int j = _nrows - 1; j > -1; j--)
					{
                        SB.Clear();
						for (int i = 0; i < _ncols; i++)
						{
                            if (_z < 0)
                            {
                                SB.Append(Math.Round(TwoDim[i, j], _round).ToString(ic));
                                SB.Append(" ");
                                //myWriter.Write(Convert.ToString(Math.Round(TwoDim[i, j], _round), ic) + " ");
                            }
                            else
                            {
                                SB.Append(Math.Round(Values[i][j][_z], _round).ToString(ic));
                                SB.Append(" ");
                                //myWriter.Write(Convert.ToString(Math.Round(Values[i, j, _z], _round), ic) + " ");
                            }
						}
						myWriter.WriteLine(SB.ToString());
                        SB.Clear();
					}
                    SB = null;
                }
				
				return true;
			}
			catch(Exception e)
			{
				{MessageBox.Show(e.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);}
				return false;
			}
		}
		
		// Write double result files to disc
		/// <summary>
    	/// Write an ESRII ASCII File for a 2 dimensional array DblArr with header and unit 
    	/// </summary>
		public bool WriteDblArrResult()
		{
			CultureInfo ic = CultureInfo.InvariantCulture;
			
			try
			{
				if (File.Exists(_filename))
				{
					try{
						File.Delete(_filename);
					}
					catch{}
				}
					
				using (StreamWriter myWriter = new StreamWriter(_filename))
				{
					// Header
					myWriter.WriteLine("ncols         " + Convert.ToString(_ncols, ic));
					myWriter.WriteLine("nrows         " + Convert.ToString(_nrows, ic));
					myWriter.WriteLine("xllcorner     " + Convert.ToString(_xllcorner, ic));
					myWriter.WriteLine("yllcorner     " + Convert.ToString(_yllcorner, ic));
					myWriter.WriteLine("cellsize      " + Convert.ToString(_Cellsize, ic));
					if (_unit.Length > 0)
                    {
                        myWriter.WriteLine("NODATA_value  " + "-9999 \t Unit:\t" + _unit);
                    }
                    else
                    {
                        myWriter.WriteLine("NODATA_value  " + "-9999");
                    }

                    StringBuilder SB = new StringBuilder();
                    for (int j = _nrows; j > 0; j--)
					{
                        SB.Clear();
						for (int i = 1; i < _ncols + 1; i++)
						{
                            SB.Append(Math.Round(DblArr[i, j], _round).ToString(ic));
                            SB.Append(" ");
							//myWriter.Write(Convert.ToString(Math.Round(DblArr[i, j], _round), ic) + " ");
						}
						myWriter.WriteLine(SB.ToString());
                        SB.Clear();
					}
                    SB = null;
				}
				
				return true;
			}
			catch(Exception e)
			{
				{MessageBox.Show(e.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);}
				return false;
			}	
		}

        // Write double result files to disc
        /// <summary>
        /// Write an ESRII ASCII File for a 2 dimensional array DblArr with header and unit 
        /// </summary>
        public bool WriteJaggedDblArrResult(double[][] Res)
        {
            CultureInfo ic = CultureInfo.InvariantCulture;

            try
            {
                if (File.Exists(_filename))
                {
                    try
                    {
                        File.Delete(_filename);
                    }
                    catch { }
                }

                using (StreamWriter myWriter = new StreamWriter(_filename))
                {
                    // Header
                    myWriter.WriteLine("ncols         " + Convert.ToString(_ncols, ic));
                    myWriter.WriteLine("nrows         " + Convert.ToString(_nrows, ic));
                    myWriter.WriteLine("xllcorner     " + Convert.ToString(_xllcorner, ic));
                    myWriter.WriteLine("yllcorner     " + Convert.ToString(_yllcorner, ic));
                    myWriter.WriteLine("cellsize      " + Convert.ToString(_Cellsize, ic));
                    if (_unit.Length > 0)
                    {
                        myWriter.WriteLine("NODATA_value  " + "-9999 \t Unit:\t" + _unit);
                    }
                    else
                    {
                        myWriter.WriteLine("NODATA_value  " + "-9999");
                    }

                    StringBuilder SB = new StringBuilder();
                    for (int j = 0; j < _nrows; j++)
                    {
                        SB.Clear();
                        for (int i = 0; i < _ncols; i++)
                        {
                            SB.Append(Math.Round(Res[i][j], _round).ToString(ic));
                            SB.Append(" ");
                            //myWriter.Write(Convert.ToString(Math.Round(DblArr[i, j], _round), ic) + " ");
                        }
                        myWriter.WriteLine(SB.ToString());
                        SB.Clear();
                    }
                    SB = null;
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}
