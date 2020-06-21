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
 * Date: 11.05.2016
 * Time: 12:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using GralStaticFunctions;

namespace GralIO
{
    /// <summary>
    /// InDatFileIO: read and write "in.dat" file
    /// </summary>
    public class InDatFileIO
	{
		private InDatVariables _data;
		private CultureInfo ic = CultureInfo.InvariantCulture;
		
		public InDatVariables Data {get {return _data;} set {_data = value;} }
		
		/// <summary>
    	/// Write file in.dat
    	/// </summary>
		public bool WriteInDat()
		{
			bool ok = true;
			
			try
			{
				using (StreamWriter myWriter = File.CreateText(_data.InDatPath))
				{
					myWriter.WriteLine(Convert.ToString(_data.ParticleNumber) + " \t ! Number of released particles per second");
					myWriter.WriteLine(Convert.ToString(_data.DispersionTime) + " \t ! Dispersion time");
					myWriter.WriteLine(Convert.ToString(_data.Transientflag)  + " \t ! Steady state GRAL mode = 1, Transient GRAL mode = 0");
					myWriter.WriteLine("4" + " \t ! Meteorology input: inputzr.dat = 0, meteo.all = 1, elimaeki.prn = 2, SONIC.dat = 3, meteopgt.all = 4");
					if (Convert.ToInt32(_data.Receptorflag) > 0)
                    {
                        myWriter.WriteLine("1 \t ! Receptor points: Yes = 1, No = 0");
                    }
                    else
                    {
                        myWriter.WriteLine("0 \t ! Receptor points: Yes = 1, No = 0");
                    }

                    myWriter.WriteLine(Convert.ToString(_data.Roughness, ic) + " \t ! Surface roughness in [m]");
					myWriter.WriteLine(Convert.ToString(_data.Latitude, ic) + " \t ! Latitude");
					myWriter.WriteLine("N" + " \t ! Meandering Effect Off = J, On = N");
					myWriter.WriteLine(_data.Pollutant + " \t ! Pollutant: not used since version 19.01, new: Pollutant.txt");
					
					for (int i = 0; i < _data.NumHorSlices; i++)
                    {
                        myWriter.Write(Convert.ToString(_data.HorSlices[i], ic) + ",");
                    }

                    myWriter.Write(" \t ! Horizontal slices [m] seperated by a comma (number of slices need to be defined in GRAL.geb!)");
					myWriter.WriteLine();
					myWriter.WriteLine(Convert.ToString(_data.Deltaz, ic) + " \t ! Vertical grid spacing in [m]");
					myWriter.WriteLine(Convert.ToString(_data.DispersionSituation) + " \t ! Start the calculation with this weather number");
					myWriter.WriteLine(Convert.ToString(_data.BuildingMode) +","+ _data.PrognosticSubDomains.ToString(ic) +
                        " \t ! How to take buildings into account? 1 = simple mass conservation, 2 = mass conservation with Poisson equation + advection, Factor for the prognostic sub domain size");
					
					if(_data.BuildingHeightsWrite == false)
                    {
                        myWriter.WriteLine("0" + " \t ! Stream output for Soundplan 1 = activated, 2 = write buildings height");
                    }
                    else
                    {
                        myWriter.WriteLine("2" + " \t ! Stream output for Soundplan 1 = activated, 2 = write buildings height");
                    }

                    if (_data.Compressed == 1)
                    {
                        myWriter.WriteLine("compressed \t ! Write compressed output files");
                    }
                    else if (_data.Compressed == 2)
                    {
                        myWriter.WriteLine("compressed V02 \t ! Write strong compressed output files");
                    }
                    else if (_data.Compressed == 3)
                    {
                        myWriter.WriteLine("compressed V03 \t ! Write strong compressed output files");
                    }
                    else
                    {
                        myWriter.WriteLine("not compressed \t ! Write uncompressed output files");
                    }

                    if (_data.WaitForKeyStroke)
                    {
                        myWriter.WriteLine("WaitForKeyStroke \t ! Wait for keystroke when exiting GRAL");
                    }
                    else
                    {
                        myWriter.WriteLine("NoKeyStroke \t ! Do not wait for a keystroke when exiting GRAL");
                    }

                    if (_data.WriteESRIResult)
                    {
                        myWriter.WriteLine("ASCiiResults 1 \t ! Additional ASCii result files Yes = 1, No = 0");
                    }
                    else
                    {
                        myWriter.WriteLine("ASCiiResults 0 \t ! Additional ASCii result files Yes = 1, No = 0");
                    }

                    myWriter.WriteLine(_data.AdaptiveRoughness.ToString(ic) + "\t ! Adaptive surface roughness - max value [m]. Default: 0 = no adaptive surface roughness");
                }
				
			}
			catch
			{
				MessageBox.Show("Error writing in.dat","I/O error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				ok = false;
			}
			return ok;
		}
		
		/// <summary>
    	/// Read the file in.dat
    	/// </summary>
		public bool ReadInDat()
		{
			try
			{
                using (StreamReader myreader = new StreamReader(_data.InDatPath))
                {

                    string dummy;
                    string[] text = new string[3];
                    string[] slices = new string[10];

                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = St_F.ICultToLCult(text[0].Trim());
                    _data.ParticleNumber = Convert.ToInt32(text[0]);

                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = St_F.ICultToLCult(text[0].Trim());
                    _data.DispersionTime = Convert.ToInt32(text[0]);

                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = text[0].Trim();
                    _data.Transientflag = Convert.ToInt16(text[0]);

                    dummy = myreader.ReadLine();
                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = text[0].Trim();
                    _data.Receptorflag = text[0];

                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    _data.Roughness = Convert.ToDouble(text[0], ic);

                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    _data.Latitude = Convert.ToDouble(text[0], ic);

                    dummy = myreader.ReadLine();
                    dummy = myreader.ReadLine();
                    dummy = myreader.ReadLine();

                    slices = dummy.Split(new char[] { ',' });
                    _data.NumHorSlices = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        try
                        {
                            _data.HorSlices[i] = Convert.ToDouble(slices[i], ic);
                            _data.NumHorSlices++;
                        }
                        catch
                        {
                            break;
                        }
                    }

                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = St_F.ICultToLCult(text[0].Trim());
                    _data.Deltaz = Convert.ToDouble(text[0]);

                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = St_F.ICultToLCult(text[0].Trim());
                    _data.DispersionSituation = Convert.ToInt32(text[0]);

                    text = myreader.ReadLine().Split(new char[] { '!' }, StringSplitOptions.RemoveEmptyEntries); // Remove comment
                    text = text[0].Split(new char[] { ' ', ',', '\r', '\n', ';', '!' }, StringSplitOptions.RemoveEmptyEntries);
                    _data.BuildingMode = Convert.ToInt32(text[0]);
                    _data.PrognosticSubDomains = 15;
                    if (text.Length > 1)
                    {
                        try
                        {
                            _data.PrognosticSubDomains = Convert.ToInt32(text[1]);
                        }
                        catch { }
                    }

                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = St_F.ICultToLCult(text[0].Trim());
                    if (Math.Abs(Convert.ToInt32(text[0])) > 1)
                    {
                        _data.BuildingHeightsWrite = true;
                    }
                    else
                    {
                        _data.BuildingHeightsWrite = false;
                    }

                    if (myreader.EndOfStream == false) // read compressed value
                    {
                        text = myreader.ReadLine().Split(new char[] { ',', '!', ' ' });
                        text[0] = text[0].Trim();
                        if (text[0].ToUpper() == "COMPRESSED")
                        {
                            _data.Compressed = 1;

                            if (text.Length > 1 && text[1].Equals("V02"))
                            {
                                _data.Compressed = 2;
                            }
                            else if (text.Length > 1 && text[1].Equals("V03"))
                            {
                                _data.Compressed = 3;
                            }
                        }
                        else
                        {
                            _data.Compressed = 0;
                        }
                    }
                    else
                    {
                        _data.Compressed = 0;
                    }

                    if (myreader.EndOfStream == false) // read compressed value
                    {
                        text = myreader.ReadLine().Split(new char[] { ',', '!', ' ' });
                        text[0] = text[0].Trim();
                        
                        if (text[0].ToLower().Equals("nokeystroke"))
                        {
                            _data.WaitForKeyStroke = false;
                        }
                        else
                        {
                            _data.WaitForKeyStroke = true;
                        }
                    }

                    if (myreader.EndOfStream == false) // read compressed value
                    {
                        text = myreader.ReadLine().Split(new char[] { ',', '!', ' ' });
                        text[0] = text[0].Trim();
                        if (text[0].Equals("ASCiiResults"))
                        {
                            if (text.Length > 1 && text[1].Equals("1"))
                            {
                                _data.WriteESRIResult = true;
                            }
                            else
                            {
                                _data.WriteESRIResult = false;
                            }
                        }
                    }

                    if (myreader.EndOfStream == false) // read compressed value
                    {
                        text = myreader.ReadLine().Split(new char[] { ',', '!', ' ' });
                        if (text.Length > 0)
                        {
                            if (double.TryParse(text[0], NumberStyles.Any, ic, out double _val))
                            {
                                _data.AdaptiveRoughness = _val;
                            }
                        }
                    }
                }
			}
			catch
			{
				return false;
			}
			return true;
		}
    }
}
