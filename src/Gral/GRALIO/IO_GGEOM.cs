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
 * Date: 31.07.2016
 * Time: 13:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Globalization;
using System.Windows.Forms;

namespace GralIO
{
    /// <summary>
    /// GGEOMFileIO: read and write ggeom.asc in binary or ascii mode
    /// </summary>
    public class GGeomFileIO
	{
		private readonly string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
		private string _pathwindfield;
        public string PathWindfield { set { _pathwindfield = value; } }
        private string _projectname;
        public string ProjectName { set { _projectname = value; } }

        private bool _write_compressed_file = false;
        public bool WriteCompressedFile { set { _write_compressed_file = value; } get { return _write_compressed_file; } }

        private double[,] _AH;
        public double[,] AH { set { _AH = value; } get { return _AH; } }
        private double _AHmin;
        public double AHmin { get { return _AHmin; } }
        private double _AHmax;
        public double AHmax { get { return _AHmax; } }

        private int _NX;
        public int NX { set { _NX = value; } get { return _NX; } }
        private int _NY;
        public int NY { set { _NY = value; } get { return _NY; } }
        private int _NZ;
        public int NZ { set { _NZ = value; } get { return _NZ; } }

        private double[] _X;
        public double[] X { set { _X = value; } get { return _X; } }
        private double[] _Y;
        public double[] Y { set { _Y = value; } get { return _Y; } }
        private double[] _Z;
        public double[] Z { set { _Z = value; } get { return _Z; } }
        private double[, ,] _VOL;
        public double[, ,] VOL { set { _VOL = value; } get { return _VOL; } }
        private double[, ,] _AREAX;
        public double[, ,] AREAX { set { _AREAX = value; } get { return _AREAX; } }
        private double[, ,] _AREAY;
        public double[, ,] AREAY { set { _AREAY = value; } get { return _AREAY; } }
        private double[, ,] _AREAZ;
        public double[, ,] AREAZ { set { _AREAZ = value; } get { return _AREAZ; } }
        private double[, ,] _AREAZY;
        public double[, ,] AREAZY { set { _AREAZY = value; } get { return _AREAZY; } }
        private double[, ,] _AREAZX;
        public double[, ,] AREAZX { set { _AREAZX = value; } get { return _AREAZX; } }
        private double[, ,] _ZSP;
        public double[, ,] ZSP { set { _ZSP = value; } get { return _ZSP; } }
        private double[] _DDX;
        public double[] DDX { set { _DDX = value; } get { return _DDX; } }
        private double[] _DDY;
        public double[] DDY { set { _DDY = value; } get { return _DDY; } }
        private double[] _ZAX;
        public double[] ZAX { set { _ZAX = value; } get { return _ZAX; } }
        private double[] _ZAY;
        public double[] ZAY { set { _ZAY = value; } get { return _ZAY; } }
        private int _IKOOA;
        public int IKOOA { set { _IKOOA = value; } get { return _IKOOA; } }
        private int _JKOOA;
        public int JKOOA { set { _JKOOA = value; } get { return _JKOOA; } }
        private double _winkel;
        public double Winkel { set { _winkel = value; } }
        private double[, ,] _AHE;
        public double[, ,] AHE { set { _AHE = value; } get { return _AHE; } }
        private double _NODATA;
        public double NODATA { set { _NODATA = value; } get { return _NODATA; } }
        private static CultureInfo ic = CultureInfo.InvariantCulture;
		
        
        /// <summary>
    	/// Read the ggeom.asc file
    	/// </summary>
    	/// <param name="mode">0: read AH and ZSP, 1: read AH, 2: read NX, NY, NZ, 3: read the complete file</param> 
		public bool ReadGGeomAsc(int mode)
		{
            // mode 0 = read AH and ZSP, 1 = read AH, 2 = read NX, NY, NZ, 3 = read the complete file
			try
			{
				bool result = false;
				
				
				string[] text = new string[1];
				using (StreamReader reader = new StreamReader(Path.Combine(_pathwindfield, @"ggeom.asc")))
				{
					text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				}

                if (Convert.ToDouble(text[0]) < 0)
                {
                    result = ReadGgeomBinary(mode);
                    _write_compressed_file = true;
                }
                else
                {
                    result = ReadGgeomStream(mode);
                    _write_compressed_file = false;
                }
				
				return result;
			}
			catch
			{
				return false;
			}
		}
		
		private bool ReadGgeomBinary(int mode)
		{
			// mode 0 = read AH and ZSP, 1 = read AH, 2 = read NX, NY, NZ, -1 = read the complete file
			try
			{
                string ggeomfile = Path.Combine(_pathwindfield, @"ggeom.asc");
				
				using (BinaryReader readbin = new BinaryReader(File.Open(ggeomfile, FileMode.Open)))
				{
					// read 1st line inclusive carriage return and line feed
					byte[] header;
					header = readbin.ReadBytes(6);
					
					//obtain array size in x,y,z direction
					_NX = readbin.ReadInt32();
					_NY = readbin.ReadInt32();
					_NZ = readbin.ReadInt32();
					
					if (mode < 2) // read further details
					{
						//obtain surface heights
						_AHmin = 10000000; _AHmax = 0;
						_AH = new double[_NX + 1, _NY + 1];
						
						// read AH[] array
						for (int j = 1; j < _NY + 1; j++)
						{
							for (int i = 1; i < _NX + 1; i++)
							{
								_AH[i, j] = readbin.ReadSingle();
								_AHmin = Math.Min(_AHmin, _AH[i, j]);
								_AHmax = Math.Max(_AHmax, _AH[i, j]);
							}
						}
						
						if (mode < 1) // read also ZSP
						{
							//obtain cell heights
							_ZSP = new double[_NX + 1, _NY + 1, _NZ + 1];
							
							// read ZSP[] array
							for (int k = 1; k < _NZ + 1; k++)
							{
								for (int j = 1; j < _NY + 1; j++)
								{
									for (int i = 1; i < _NX + 1; i++)
									{
										_ZSP[i, j, k] = readbin.ReadSingle();
									}
								}
							}

                            if (mode < 0) // read complete file
                            {
                                _X = new double[_NX + 2];
                                _Y = new double[_NY + 2];
                                _Z = new double[_NZ + 2];
                                _VOL = new double[_NX + 1, _NY + 1, _NZ + 1];
                                _AREAX = new double[_NX + 2, _NY + 1, _NZ + 1];
                                _AREAY = new double[_NX + 1, _NY + 2, _NZ + 1];
                                _AREAZ = new double[_NX + 1, _NY + 1, _NZ + 2];
                                _AREAZX = new double[_NX + 1, _NY + 1, _NZ + 2];
                                _AREAZY = new double[_NX + 1, _NY + 1, _NZ + 2];
                                _AHE = new double[_NX + 2, _NY + 2, _NZ + 2];
                                _DDX = new double[_NX + 1];
                                _DDY = new double[_NY + 1];
                                _ZAX = new double[_NX + 1];
                                _ZAY = new double[_NY + 1];

                                //obtain X, Y, and Z
                                for (int i = 1; i < _NX + 2; i++)
                                {
                                    _X[i] = readbin.ReadSingle();
                                }
                                for (int i = 1; i < _NY + 2; i++)
                                {
                                    _Y[i] = readbin.ReadSingle();
                                }
                                for (int i = 1; i < _NZ + 2; i++)
                                {
                                    _Z[i] = readbin.ReadSingle();
                                }

                                //obtain grid volumes
                                for (int k = 1; k < _NZ + 1; k++)
                                {
                                    for (int j = 1; j < _NY + 1; j++)
                                    {
                                        for (int i = 1; i < _NX + 1; i++)
                                        {
                                            _VOL[i, j, k] = readbin.ReadSingle();
                                        }
                                    }
                                }

                                //obtain areas in x-direction
                                for (int k = 1; k < _NZ + 1; k++)
                                {
                                    for (int j = 1; j < _NY + 1; j++)
                                    {
                                        for (int i = 1; i < _NX + 2; i++)
                                        {
                                            _AREAX[i, j, k] = readbin.ReadSingle();
                                        }
                                    }
                                }

                                //obtain areas in y-direction
                                for (int k = 1; k < _NZ + 1; k++)
                                {
                                    for (int j = 1; j < _NY + 2; j++)
                                    {
                                        for (int i = 1; i < _NX + 1; i++)
                                        {
                                            _AREAY[i, j, k] = readbin.ReadSingle();
                                        }
                                    }
                                }

                                //obtain projection of z-area in x-direction
                                for (int k = 1; k < _NZ + 2; k++)
                                {
                                    for (int j = 1; j < _NY + 1; j++)
                                    {
                                        for (int i = 1; i < _NX + 1; i++)
                                        {
                                            _AREAZX[i, j, k] = readbin.ReadSingle();
                                        }
                                    }
                                }

                                //obtain projection of z-area in y-direction
                                for (int k = 1; k < _NZ + 2; k++)
                                {
                                    for (int j = 1; j < _NY + 1; j++)
                                    {
                                        for (int i = 1; i < _NX + 1; i++)
                                        {
                                            _AREAZY[i, j, k] = readbin.ReadSingle();
                                        }
                                    }
                                }

                                //obtain area in z-direction
                                for (int k = 1; k < _NZ + 2; k++)
                                {
                                    for (int j = 1; j < _NY + 1; j++)
                                    {
                                        for (int i = 1; i < _NX + 1; i++)
                                        {
                                            _AREAZ[i, j, k] = readbin.ReadSingle();
                                        }
                                    }
                                }
                                //obtain grid cells sizes in x-direction
                                for (int i = 1; i < _NX + 1; i++)
                                {
                                    _DDX[i] = readbin.ReadSingle();
                                }
                                //obtain grid cells sizes in y-direction
                                for (int i = 1; i < _NY + 1; i++)
                                {
                                    _DDY[i] = readbin.ReadSingle();
                                }
                                //obtain distances of neighbouring grid cells in x-direction
                                for (int i = 1; i < _NX + 1; i++)
                                {
                                    _ZAX[i] = readbin.ReadSingle();
                                }
                                //obtain distances of neighbouring grid cells in y-direction
                                for (int i = 1; i < _NY + 1; i++)
                                {
                                    _ZAY[i] = readbin.ReadSingle();
                                }

                                //obtain western and southern borders of the model domain and the angle (not used anymore) between the main model domain axis and north
                                _IKOOA = readbin.ReadInt32();
                                _JKOOA = readbin.ReadInt32();
                                _winkel = readbin.ReadDouble(); // angle not used
                                
                                for (int k = 1; k < _NZ + 2; k++)
                                {
                                	Application.DoEvents(); // Kuntner
                                	for (int j = 1; j < _NY + 2; j++)
                                	{
                                		for (int i = 1; i < _NX + 2; i++)
                                		{
                                			_AHE[i, j, k] = readbin.ReadSingle();
                                		}
                                	}
                                }
                                

                            }// mode -1
						} // mode 0
					} // mode 1
				}
				return true;
			}
			
			catch(Exception e)
			{
				MessageBox.Show(e.Message,"GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}
				
		private bool ReadGgeomStream(int mode)
		{
            // mode 0 = read AH and ZSP, 1 = read AH, 2 = read NX, NY, NZ, -1 = read the complete file
			try
			{
				StreamReader reader = new StreamReader(Path.Combine(_pathwindfield, @"ggeom.asc"));
				string[] text = new string[1];
				int count = 0;
				while (count < 3)
				{
					text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					count += text.Length;
				}
				//obtain array size in x,y,z direction
				_NX = Convert.ToInt32(text[0]);
				_NY = Convert.ToInt32(text[1]);
				_NZ = Convert.ToInt32(text[2]);

                if ((mode < 2) && (mode > -1)) // read further details
				{
					int ix = 0;
					int iy = 1;
					
					//obtain surface heights
					_AHmin = 10000000; _AHmax = 0;
					
					_AH = new double[_NX + 1, _NY + 1];
					while (count <= (_NX * _NY + _NX + _NY + _NZ + 6))
					{
						Application.DoEvents(); // Kuntner
						text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
						count += text.Length;
						if ((count >= (_NX + _NY + _NZ + 6)) && (count - text.Length < (_NX * _NY + _NX + _NY + _NZ + 6)))
						{
							for (int i = 1; i <= text.Length; i++)
							{
								if (((count - text.Length + i) > (_NX + _NY + _NZ + 6)) && ((count - text.Length + i) <= (_NX * _NY + _NX + _NY + _NZ + 6)))
								{
									ix += 1;
									if (ix == _NX + 1)
									{
										ix = 1;
										iy += 1;
									}
									_AH[ix, iy] = Convert.ToDouble(text[i - 1].Replace(".", decsep));
									_AHmin = Math.Min(_AHmin, _AH[ix, iy]);
									_AHmax = Math.Max(_AHmax, _AH[ix, iy]);
								}
							}
						}
					}
					
					if (mode < 1) // read all
					{
						//obtain cell heights
						_ZSP = new double[_NX + 1, _NY + 1, _NZ + 1];
						int begin = (_NX * _NY + _NX + _NY + _NZ + 6) + _NX * _NY * _NZ + (_NX + 1) * _NY * _NZ + _NX * (_NY + 1) * _NZ + _NX * _NY * (_NZ + 1) + _NX * _NY * (_NZ + 1) + _NX * _NY * (_NZ + 1);
						ix = 0;
						iy = 1;
						int iz = 1;
						while (count <= (begin + _NX * _NY * _NZ))
						{
							Application.DoEvents(); // Kuntner
							text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
							count += text.Length;
							if ((count >= (begin)) && (count - text.Length < (begin + _NX * _NY * _NZ)))
							{
								for (int i = 1; i <= text.Length; i++)
								{
									if (((count - text.Length + i) > (begin)) && ((count - text.Length + i) <= (begin + _NX * _NY * _NZ)))
									{

										ix += 1;
										if (ix == _NX + 1)
										{
											ix = 1;
											iy += 1;
										}
										if (iy == _NY + 1)
										{
											iy = 1;
											iz += 1;
										}
										_ZSP[ix, iy, iz] = Convert.ToDouble(text[i - 1].Replace(".", decsep));
									}
								}
							}
							if (ix > 2 && iy > 2 && iz >=_NZ)
                            {
                                break;
                            }
                        }
					} // mode 0
				} // mode 1

                if (mode < 0) // read complete file
                {
                    _X = new double[_NX + 2];
                    _Y = new double[_NY + 2];
                    _Z = new double[_NZ + 2];
                    _AH = new double[_NX + 1, _NY + 1];
                    _ZSP = new double[_NX + 1, _NY + 1, _NZ + 1];
                    _VOL = new double[_NX + 1, _NY + 1, _NZ + 1];
                    _AREAX = new double[_NX + 2, _NY + 1, _NZ + 1];
                    _AREAY = new double[_NX + 1, _NY + 2, _NZ + 1];
                    _AREAZ = new double[_NX + 1, _NY + 1, _NZ + 2];
                    _AREAZX = new double[_NX + 1, _NY + 1, _NZ + 2];
                    _AREAZY = new double[_NX + 1, _NY + 1, _NZ + 2];
                    _AHE = new double[_NX + 2, _NY + 2, _NZ + 2];
                    _DDX = new double[_NX + 1];
                    _DDY = new double[_NY + 1];
                    _ZAX = new double[_NX + 1];
                    _ZAY = new double[_NY + 1];

                    //obtain X, Y, and Z
                    for (int i = 3; i < 3 + _NX + 1; i++)
                    {
                        _X[i - 2] = Convert.ToSingle(text[i].Replace(".", decsep));
                    }
                    int n = 0;
                    for (int i = 3 + _NX + 1; i < 3 + _NX + 1 + _NY + 1; i++)
                    {
                        n++;
                        _Y[n] = Convert.ToSingle(text[i].Replace(".", decsep));
                    }
                    n = 0;
                    for (int i = 3 + _NX + 1 + _NY + 1; i < text.Length; i++)
                    {
                        n++;
                        _Z[n] = Convert.ToSingle(text[i].Replace(".", decsep));
                    }

                    //obtain surface heights
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 1; j < _NY + 1; j++)
                    {
                        for (int i = 1; i < _NX + 1; i++)
                        {
                            _AH[i, j] = Convert.ToSingle(text[n].Replace(".", decsep));
                            n++;
                        }
                    }
                    //obtain grid volumes
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int k = 1; k < _NZ + 1; k++)
                    {
                        for (int j = 1; j < _NY + 1; j++)
                        {
                            for (int i = 1; i < _NX + 1; i++)
                            {
                                _VOL[i, j, k] = Convert.ToSingle(text[n].Replace(".", decsep));
                                n++;
                            }
                        }
                    }
                    //obtain areas in x-direction
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int k = 1; k < _NZ + 1; k++)
                    {
                        for (int j = 1; j < _NY + 1; j++)
                        {
                            for (int i = 1; i < _NX + 2; i++)
                            {
                                _AREAX[i, j, k] = Convert.ToSingle(text[n].Replace(".", decsep));
                                n++;
                            }
                        }
                    }
                    //obtain areas in y-direction
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int k = 1; k < _NZ + 1; k++)
                    {
                        for (int j = 1; j < _NY + 2; j++)
                        {
                            for (int i = 1; i < _NX + 1; i++)
                            {
                                _AREAY[i, j, k] = Convert.ToSingle(text[n].Replace(".", decsep));
                                n++;
                            }
                        }
                    }
                    //obtain projection of z-area in x-direction
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int k = 1; k < _NZ + 2; k++)
                    {
                        for (int j = 1; j < _NY + 1; j++)
                        {
                            for (int i = 1; i < _NX + 1; i++)
                            {
                                _AREAZX[i, j, k] = Convert.ToSingle(text[n].Replace(".", decsep));
                                n++;
                            }
                        }
                    }
                    //obtain projection of z-area in y-direction
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int k = 1; k < _NZ + 2; k++)
                    {
                        for (int j = 1; j < _NY + 1; j++)
                        {
                            for (int i = 1; i < _NX + 1; i++)
                            {
                                _AREAZY[i, j, k] = Convert.ToSingle(text[n].Replace(".", decsep));
                                n++;
                            }
                        }
                    }
                    //obtain area in z-direction
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int k = 1; k < _NZ + 2; k++)
                    {
                        for (int j = 1; j < _NY + 1; j++)
                        {
                            for (int i = 1; i < _NX + 1; i++)
                            {
                                _AREAZ[i, j, k] = Convert.ToSingle(text[n].Replace(".", decsep));
                                n++;
                            }
                        }
                    }
                    //obtain cell-heights
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int k = 1; k < _NZ + 1; k++)
                    {
                        for (int j = 1; j < _NY + 1; j++)
                        {
                            for (int i = 1; i < _NX + 1; i++)
                            {
                                _ZSP[i, j, k] = Convert.ToSingle(text[n].Replace(".", decsep));
                                n++;
                            }
                        }
                    }
                    //obtain grid cells sizes in x-direction
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < _NX + 1; i++)
                    {
                        _DDX[i] = Convert.ToSingle(text[n].Replace(".", decsep));
                        n++;
                    }
                    //obtain grid cells sizes in y-direction
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < _NY + 1; i++)
                    {
                        _DDY[i] = Convert.ToSingle(text[n].Replace(".", decsep));
                        n++;
                    }
                    //obtain distances of neighbouring grid cells in x-direction
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < _NX + 1; i++)
                    {
                        _ZAX[i] = Convert.ToSingle(text[n].Replace(".", decsep));
                        n++;
                    }
                    //obtain distances of neighbouring grid cells in y-direction
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < _NY + 1; i++)
                    {
                        _ZAY[i] = Convert.ToSingle(text[n].Replace(".", decsep));
                        n++;
                    }
                    //obtain western and southern borders of the model domain and the angle (not used anymore) between the main model domain axis and north
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    _IKOOA = Convert.ToInt32(text[0]);
                    _JKOOA = Convert.ToInt32(text[1]);
                    _winkel = Convert.ToDouble(text[2].Replace(".", decsep));

                    //obtain heights of the cell corners
                    n = 0;
                    text = reader.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int k = 1; k < _NZ + 2; k++)
                    {
                    	Application.DoEvents(); // Kuntner
                    	for (int j = 1; j < _NY + 2; j++)
                    	{
                    		for (int i = 1; i < _NX + 2; i++)
                    		{
                                _AHE[i, j, k] = Convert.ToDouble(text[n].Replace(".", decsep));
                                n++;
                    		}
                    	}
                    }
                    
                } // mode -1
				
				reader.Close();
				reader.Dispose();
				return true;
			}
			
			catch(Exception e)
			{
				MessageBox.Show(e.Message,"GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		/// <summary>
    	/// Write the ggeom.asc file
    	/// </summary>
        public bool WriteGGeomFile()
        {
            try
            {
                StreamWriter writer;
                // write ggeom asc the tradidional way as an ASCII File
                if (_write_compressed_file == false)
                {
                    writer = new StreamWriter(_pathwindfield, false);
                    writer.Write(Convert.ToString(_NX) + " " + Convert.ToString(_NY) + " " + Convert.ToString(_NZ) + " ");
                    for (int i = 1; i < _NX + 2; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_X[i], 2), ic) + " ");
                    }
                    for (int i = 1; i < _NY + 2; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_Y[i], 2), ic) + " ");
                    }
                    for (int i = 1; i < _NZ + 2; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_Z[i], 2), ic) + " ");
                    }
                    writer.WriteLine();
                    for (int j = 1; j < _NY + 1; j++)
                    {
                        Application.DoEvents(); // Kuntner
                        for (int i = 1; i < _NX + 1; i++)
                        {
                            writer.Write(Convert.ToString(Math.Round(_AH[i, j], 2), ic) + " ");
                        }
                    }
                    writer.WriteLine();
                    for (int k = 1; k < _NZ + 1; k++)
                    {
                        Application.DoEvents(); // Kuntner
                        for (int j = 1; j < _NY + 1; j++)
                        {
                            for (int i = 1; i < _NX + 1; i++)
                            {
                                writer.Write(Convert.ToString(Math.Round(_VOL[i, j, k], 2), ic) + " ");
                            }
                        }
                    }
                    writer.WriteLine();
                    for (int k = 1; k < _NZ + 1; k++)
                    {
                        Application.DoEvents(); // Kuntner
                        for (int j = 1; j < _NY + 1; j++)
                        {
                            for (int i = 1; i < _NX + 2; i++)
                            {
                                writer.Write(Convert.ToString(Math.Round(_AREAX[i, j, k], 2), ic) + " ");
                            }
                        }
                    }
                    writer.WriteLine();
                    for (int k = 1; k < _NZ + 1; k++)
                    {
                        Application.DoEvents(); // Kuntner
                        for (int j = 1; j < _NY + 2; j++)
                        {
                            for (int i = 1; i < _NX + 1; i++)
                            {
                                writer.Write(Convert.ToString(Math.Round(_AREAY[i, j, k], 2), ic) + " ");
                            }
                        }
                    }
                    writer.WriteLine();
                    for (int k = 1; k < _NZ + 2; k++)
                    {
                        Application.DoEvents(); // Kuntner
                        for (int j = 1; j < _NY + 1; j++)
                        {
                            for (int i = 1; i < _NX + 1; i++)
                            {
                                writer.Write(Convert.ToString(Math.Round(_AREAZX[i, j, k], 2), ic) + " ");
                            }
                        }
                    }
                    writer.WriteLine();
                    for (int k = 1; k < _NZ + 2; k++)
                    {
                        Application.DoEvents(); // Kuntner
                        for (int j = 1; j < _NY + 1; j++)
                        {
                            for (int i = 1; i < _NX + 1; i++)
                            {
                                writer.Write(Convert.ToString(Math.Round(_AREAZY[i, j, k], 2), ic) + " ");
                            }
                        }
                    }
                    writer.WriteLine();
                    for (int k = 1; k < _NZ + 2; k++)
                    {
                        Application.DoEvents(); // Kuntner
                        for (int j = 1; j < _NY + 1; j++)
                        {
                            for (int i = 1; i < _NX + 1; i++)
                            {
                                writer.Write(Convert.ToString(Math.Round(_AREAZ[i, j, k], 2), ic) + " ");
                            }
                        }
                    }
                    writer.WriteLine();
                    for (int k = 1; k < _NZ + 1; k++)
                    {
                        Application.DoEvents(); // Kuntner
                        for (int j = 1; j < _NY + 1; j++)
                        {
                            for (int i = 1; i < _NX + 1; i++)
                            {
                                writer.Write(Convert.ToString(Math.Round(_ZSP[i, j, k], 2), ic) + " ");
                            }
                        }
                    }
                    writer.WriteLine();
                    for (int i = 1; i < _NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_DDX[i], 2), ic) + " ");
                    }
                    writer.WriteLine();
                    for (int i = 1; i < _NY + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_DDY[i], 2), ic) + " ");
                    }
                    writer.WriteLine();
                    for (int i = 1; i < _NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_ZAX[i], 2), ic) + " ");
                    }
                    writer.WriteLine();
                    for (int i = 1; i < _NY + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_ZAY[i], 2), ic) + " ");
                    }
                    writer.WriteLine();
                    writer.Write(Convert.ToString(_IKOOA, ic) + " ");
                    writer.Write(Convert.ToString(_JKOOA, ic) + " ");
                    writer.Write(Convert.ToString(_winkel, ic) + " ");
                    writer.WriteLine();
                    for (int k = 1; k < _NZ + 2; k++)
                    {
                        Application.DoEvents(); // Kuntner
                        for (int j = 1; j < _NY + 2; j++)
                        {
                            for (int i = 1; i < _NX + 2; i++)
                            {
                                writer.Write(Convert.ToString(Math.Round(_AHE[i, j, k], 2), ic) + " ");
                            }
                        }
                    }
                    writer.Close();
                }
                else // write ggeom asc as binary file
                {
                    try
                    {
                        string ggeom = Path.Combine(_projectname, @"Computation","ggeom.asc");
                        using (BinaryWriter writebin = new BinaryWriter(File.Open(ggeom, FileMode.Create)))
                        {
                            // write first Line = -99 +chr(13) +chr(10)
                            writebin.Write((byte)45);
                            writebin.Write((byte)57);
                            writebin.Write((byte)57);
                            writebin.Write((byte)32);
                            writebin.Write((byte)13);
                            writebin.Write((byte)10);

                            // write Header values
                            writebin.Write((int)_NX);
                            writebin.Write((int)_NY);
                            writebin.Write((int)_NZ);

                            // write AH[] array
                            for (int j = 1; j < _NY + 1; j++)
                            {
                                Application.DoEvents(); // Kuntner
                                for (int i = 1; i < _NX + 1; i++)
                                {
                                    writebin.Write((float)Math.Round(_AH[i, j], 2));
                                }
                            }
                            // write ZSP[] array
                            for (int k = 1; k < _NZ + 1; k++)
                            {
                                Application.DoEvents(); // Kuntner
                                for (int j = 1; j < _NY + 1; j++)
                                {
                                    for (int i = 1; i < _NX + 1; i++)
                                    {
                                        writebin.Write((float)Math.Round(_ZSP[i, j, k], 2));
                                    }
                                }
                            }

                            for (int i = 1; i < _NX + 2; i++)
                            {
                                writebin.Write((float)Math.Round(_X[i], 2));
                            }
                            for (int i = 1; i < _NY + 2; i++)
                            {
                                writebin.Write((float)Math.Round(_Y[i], 2));
                            }
                            for (int i = 1; i < _NZ + 2; i++)
                            {
                                writebin.Write((float)Math.Round(_Z[i], 2));
                            }

                            for (int k = 1; k < _NZ + 1; k++)
                            {
                                Application.DoEvents(); // Kuntner
                                for (int j = 1; j < _NY + 1; j++)
                                {
                                    for (int i = 1; i < _NX + 1; i++)
                                    {
                                        writebin.Write((float)_VOL[i, j, k]);
                                    }
                                }
                            }

                            for (int k = 1; k < _NZ + 1; k++)
                            {
                                Application.DoEvents(); // Kuntner
                                for (int j = 1; j < _NY + 1; j++)
                                {
                                    for (int i = 1; i < _NX + 2; i++)
                                    {
                                        writebin.Write((float)_AREAX[i, j, k]);
                                    }
                                }
                            }

                            for (int k = 1; k < _NZ + 1; k++)
                            {
                                Application.DoEvents(); // Kuntner
                                for (int j = 1; j < _NY + 2; j++)
                                {
                                    for (int i = 1; i < _NX + 1; i++)
                                    {
                                        writebin.Write((float)_AREAY[i, j, k]);
                                    }
                                }
                            }

                            for (int k = 1; k < _NZ + 2; k++)
                            {
                                Application.DoEvents(); // Kuntner
                                for (int j = 1; j < _NY + 1; j++)
                                {
                                    for (int i = 1; i < _NX + 1; i++)
                                    {
                                        writebin.Write((float)_AREAZX[i, j, k]);
                                    }
                                }
                            }

                            for (int k = 1; k < _NZ + 2; k++)
                            {
                                Application.DoEvents(); // Kuntner
                                for (int j = 1; j < _NY + 1; j++)
                                {
                                    for (int i = 1; i < _NX + 1; i++)
                                    {
                                        writebin.Write((float)_AREAZY[i, j, k]);
                                    }
                                }
                            }

                            for (int k = 1; k < _NZ + 2; k++)
                            {
                                Application.DoEvents(); // Kuntner
                                for (int j = 1; j < _NY + 1; j++)
                                {
                                    for (int i = 1; i < _NX + 1; i++)
                                    {
                                        writebin.Write((float)_AREAZ[i, j, k]);
                                    }
                                }
                            }

                            for (int i = 1; i < _NX + 1; i++)
                            {
                                writebin.Write((float)_DDX[i]);
                            }

                            for (int i = 1; i < _NY + 1; i++)
                            {
                                writebin.Write((float)_DDY[i]);
                            }

                            for (int i = 1; i < _NX + 1; i++)
                            {
                                writebin.Write((float)_ZAX[i]);
                            }

                            for (int i = 1; i < _NY + 1; i++)
                            {
                                writebin.Write((float)_ZAY[i]);
                            }

                            writebin.Write((int)_IKOOA);
                            writebin.Write((int)_JKOOA);
                            writebin.Write((double)_winkel);

                            for (int k = 1; k < _NZ + 2; k++)
                            {
                                Application.DoEvents(); // Kuntner
                                for (int j = 1; j < _NY + 2; j++)
                                {
                                    for (int i = 1; i < _NX + 2; i++)
                                    {
                                        writebin.Write((float)_AHE[i, j, k]);
                                    }
                                }
                            }
                        } // Using 
                    }
                    catch
                    {
                        MessageBox.Show("Error writing ggeom.asc", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                writer = new StreamWriter(Path.Combine(_projectname, @"Maps","ggeom.txt"), false);
                writer.WriteLine("ncols             " + Convert.ToString(_NX));
                writer.WriteLine("nrows             " + Convert.ToString(_NY));
                writer.WriteLine("xllcorner         " + Convert.ToString(_IKOOA + Convert.ToInt32(_DDX[1] * 0.5), ic));
                writer.WriteLine("yllcorner         " + Convert.ToString(_JKOOA + Convert.ToInt32(_DDY[1] * 0.5), ic));
                writer.WriteLine("cellsize          " + Convert.ToString(_DDX[1], ic));
                writer.WriteLine("NODATA_value      " + Convert.ToString(_NODATA) + "\t UNIT \t m");
                for (int i = _NY; i > 0; i--)
                {
                    Application.DoEvents(); // Kuntner
                    for (int j = 1; j < _NX + 1; j++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_AH[j, i], 1), ic) + " ");
                    }
                    writer.WriteLine();
                }
                writer.Close();
                writer.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
	}
}
