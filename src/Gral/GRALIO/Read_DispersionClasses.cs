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
 * Date: 18.07.2016
 * Time: 19:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Windows.Forms;
using System.IO.Compression;
using System.Globalization;

namespace GralIO
{
	/// <summary>
	/// Read GRAMM dispersion classes from "*.scl" files
	/// </summary>
	public class ReadSclUstOblClasses
	{
		private string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
		private string _filename;
		public string FileName {get {return _filename;} set {_filename = value;} } // Filename
		private float _GRAMMhorgridsize;
		public float GRAMMhorgridsize { set { _GRAMMhorgridsize = value; } get { return _GRAMMhorgridsize; } }

		private int NI;
		private int NJ;

		private int _NX;
		public int NX { set { _NX = value; } }
		private int _NY;
		public int NY { set { _NY = value; } }
		private int _NZ;
		public int NZ { set { _NZ = value; } }
		private int _Y0 = 0;
		public int Y0 { set { _Y0 = value; } }
		private int _X0 = 0;
		public int X0 { set { _X0 = value; } }
		
		private double[,] _Stabclasses;
		public double[,] Stabclasses { set { _Stabclasses = value; } get {return _Stabclasses;} }
		private double[,] _MOlength;
		public double[,] MOlength { set { _MOlength = value; } get{return _MOlength;} }
		private double[,] _Ustar;
		public double[,] Ustar { set { _Ustar = value; } get{return _Ustar;} }

		/// <summary>
		/// Read GRAMM dispersion classes from "*.scl" files
		/// </summary>
		public bool ReadSclFile() // read complete file to _Stabclasses, _MO_Lenght and _Ustar
		{
			try
			{
				if (File.Exists(_filename))
				{
					if (ReadFlowFieldFiles.CheckIfFileIsZipped(_filename)) // file zipped?
					{
						using(ZipArchive archive = ZipFile.OpenRead(_filename)) //open Zip archive
						{
							foreach (ZipArchiveEntry entry in archive.Entries)  // search for a scl file
							{
								if (entry.FullName.Contains("scl"))
								{
									using (BinaryReader stability = new BinaryReader(entry.Open())) //OPen Zip entry
									{
										readvalues(stability, ref _Stabclasses);
									}
								}
								if (entry.FullName.Contains("ust"))
								{
									using (BinaryReader stability = new BinaryReader(entry.Open())) //OPen Zip entry
									{
										readvalues(stability, ref _Ustar);
									}
								}
								if (entry.FullName.Contains("obl"))
								{
									using (BinaryReader stability = new BinaryReader(entry.Open())) //OPen Zip entry
									{
										readvalues(stability, ref _MOlength);
									}
								}
							}
						}
					}
					else // not zipped
					{
						using(BinaryReader stability = new BinaryReader(File.Open(_filename, FileMode.Open)))
						{
							readvalues(stability, ref _Stabclasses);
						}
						
					}
				}
				else
					throw new FileNotFoundException(_filename + @"not found");

				return true; // Reading OK
			}
			catch
			{
				return false;
			}
			
		}
		
		private bool readvalues(BinaryReader stability, ref double [,] Scl_Array)
		{
			try
			{
				// read the header
				stability.ReadInt32();
				NI = stability.ReadInt32();
				NJ = stability.ReadInt32();
				int NK = stability.ReadInt32();

				_GRAMMhorgridsize = stability.ReadSingle();
				Scl_Array = null; // delete array -
				Scl_Array = new double[NI,NJ]; // create new array

				for (int i = 0; i < NI; i++)
					for (int j = 0; j < NJ; j++)
				{
					short temp = stability.ReadInt16();
					Scl_Array[i, j] = Convert.ToDouble(temp);
				}
				return true;
			}
			catch
			{
				return false;
			}
		}
		
		/// <summary>
		/// Read one cell of GRAMM dispersion classes from "*.scl" files
		/// </summary>
		public int ReadSclFile(int x, int y) // read one value from *.scl
		{
			try
			{
				short temp = 0;
				if (File.Exists(_filename) && x >= 0 && y >= 0)
				{
					if (ReadFlowFieldFiles.CheckIfFileIsZipped(_filename)) // file zipped?
					{
						using(ZipArchive archive = ZipFile.OpenRead(_filename)) //open Zip archive
						{
							foreach (ZipArchiveEntry entry in archive.Entries) // search for a scl file
							{
								if (entry.FullName.Contains("scl"))
								{
									using (BinaryReader stability = new BinaryReader(entry.Open())) //OPen Zip entry
									{
										// read the header
										stability.ReadInt32();
										int NI = stability.ReadInt32();
										int NJ = stability.ReadInt32();
										int NK = stability.ReadInt32();
										_GRAMMhorgridsize = stability.ReadSingle();
										
										long position = (x * NJ + y); // Position in bytes 20 Bytes = Header
										
										if (x < NI && y < NJ)
										{
											// Seek doesn't work in zipped files
											// stability.BaseStream.Seek(position, SeekOrigin.Begin);
											for (int i = 0; i < position; i++) // seek manually
												stability.ReadInt16();
											
											temp = stability.ReadInt16(); // read this value
										}
									}
								}
							}
						}
					}
					else // not zipped
					{
						using(BinaryReader stability = new BinaryReader(File.Open(_filename, FileMode.Open)))
						{
							// read the header
							stability.ReadInt32();
							int NI = stability.ReadInt32();
							int NJ = stability.ReadInt32();
							int NK = stability.ReadInt32();
							_GRAMMhorgridsize = stability.ReadSingle();
							
							long position = (x * NJ + y) * 2 + 20; // Position in bytes 20 Bytes = Header
							
							long lenght = stability.BaseStream.Length; // data set lenght
							if (position < lenght && x < NI && y < NJ)
							{
								stability.BaseStream.Seek(position, SeekOrigin.Begin);
								temp = stability.ReadInt16(); // read this value
							}
						}
						
					}
				}
				else
					throw new FileNotFoundException(_filename + @"not found");

				return temp; // Reading OK
			}
			catch(Exception e)
			{
				MessageBox.Show(e.Message.ToString(), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return 0;
			}
		}
		
		/// <summary>
		/// Read a mean value of 3x3 cells of GRAMM dispersion classes from "*.scl" files
		/// </summary>
		public int ReadSclMean(int x, int y) // read a mean (3x3) value from *.scl file and return the stability class
			// define Filename!
		{
			int SCL = 0;
			if (ReadSclFile()) // read complete file
			{
				SCL = SclMean(x, y);
			}
			
			return SCL; // return mean stability class
		}
		
		/// <summary>
		/// Read a mean value of 3x3 cells of GRAMM dispersion classes from "*.scl" files
		/// </summary>
		public int SclMean(int x, int y)
		{
			int counter = 0;
			double sum = 0;
			try
			{
				for (int i = x - 1; i < x + 2; i++)
				{
					for (int j = y - 1; j < y + 2; j++)
					{
						if (i >= 0 && j >= 0 && i < NI && j < NJ) // inside _Stabclassesay
						{
							sum += _Stabclasses[i,j];
							counter++;
							if (i == x && j == y) // double weighting of center
							{
								sum += _Stabclasses[i,j];
								counter++;
							}
							
						}
					}
				}
			}
			catch {}
			
			if (counter > 0)
				return (int) Math.Round(sum / counter); // compute nearest value
			else
				return 0;
			
			
		}

		/// <summary>
		/// Export the stability classes, friction velocity, and Obukhov length for a GRAMM sub domain
		/// </summary>
		public bool ExportSclFile() //output, export for stability classes, friction velocity, and Obukhov length
		{
			try
			{
				// write a Zip file
				int header = -1;
				Int16 dummy;
				string stabclassfilename = (_filename + ".scl");
				try
				{
					using (BinaryWriter writer = new BinaryWriter(File.Open(stabclassfilename, FileMode.Create)))
					{ ; }
					
					using (FileStream zipToOpen = new FileStream(stabclassfilename, FileMode.Create))
					{
						using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
						{
							string ustarfilename = (Path.GetFileNameWithoutExtension(_filename) + ".ust");
							ZipArchiveEntry write_entry1 = archive.CreateEntry(ustarfilename);
							using (BinaryWriter writer = new BinaryWriter(write_entry1.Open()))
							{
								writer.Write(header);
								writer.Write(_NX - _X0);
								writer.Write(_NY - _Y0);
								writer.Write(_NZ);
								writer.Write(_GRAMMhorgridsize);
								for (int i = _X0; i < _NX; i++)
									for (int j = _Y0; j < _NY; j++)
								{
									dummy = Convert.ToInt16(_Ustar[i, j]);
									writer.Write(dummy);
								}
							}

							string obukhovfilename = (Path.GetFileNameWithoutExtension(_filename) + ".obl");
							ZipArchiveEntry write_entry2 = archive.CreateEntry(obukhovfilename);
							using (BinaryWriter writer = new BinaryWriter(write_entry2.Open()))
							{
								writer.Write(header);
								writer.Write(_NX - _X0);
								writer.Write(_NY - _Y0);
								writer.Write(_NZ);
								writer.Write(_GRAMMhorgridsize);
								for (int i = _X0; i < _NX; i++)
									for (int j = _Y0; j < _NY; j++)
								{
									dummy = Convert.ToInt16(_MOlength[i, j]);
									writer.Write(dummy);
								}
							}

							//computation and ouput of stability classes
							string stabilityfile = (Path.GetFileNameWithoutExtension(_filename) + ".scl");
							ZipArchiveEntry write_entry3 = archive.CreateEntry(stabilityfile);
							using (BinaryWriter writer = new BinaryWriter(write_entry3.Open()))
							{
								writer.Write(header);
								writer.Write(_NX - _X0);
								writer.Write(_NY - _Y0);
								writer.Write(_NZ);
								writer.Write(_GRAMMhorgridsize);
								for (int i = _X0; i < _NX; i++)
									for (int j = _Y0; j < _NY; j++)
								{
									dummy = Convert.ToInt16(_Stabclasses[i, j]);
									writer.Write(dummy);
								}
							}
						} // archive
					} // Zip File
				} // catch
				catch { }

				return true; // Reading OK
			}
			catch
			{
				return false;
			}

		}
		
		
		public bool close()
		{
			_filename = null;
			return true;
		}
		
	}
}
