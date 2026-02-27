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

using System;
using System.Globalization;
using System.IO;

namespace GralIO
{
    /// <summary>
    /// Windfield_Reader: read a GRAMM wind field "*.wnd" and write GRAMM windfields (for the GRAMM export function)
    /// </summary>
    public class Windfield_Reader
    {
        private string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        private string _pathWindfield;
        public string PathWindfield { set { _pathWindfield = value; } }
        private double _DDX;
        public double DDX { set { _DDX = value; } }
        private int _NI;
        public int NI { set { _NI = value; } }
        private int _NJ;
        public int NJ { set { _NJ = value; } }
        private int _NK;
        public int NK { set { _NK = value; } }
        private float[,,] _U;
        public float[,,] U { set { _U = value; } }
        private float[,,] _V;
        public float[,,] V { set { _V = value; } }
        private float[,,] _W;
        public float[,,] W { set { _W = value; } }


        // methode to read windfield-data 
        /// <summary>
        ///Read a GRAMM wind field "*.wnd" 
        ///Called from await - async! 
        /// </summary>
        public bool Windfield_read(string filename, int NX, int NY, int NZ, ref float[,,] UWI, ref float[,,] VWI, ref float[,,] WWI)
        {
            if (!File.Exists(filename))
            {
                return false;
            }

            try
            {
                using (FileStream str_windfield = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (BufferedStream buf_windfield = new BufferedStream(str_windfield, 32768))
                    {
                        BinaryReader windfieldb = new BinaryReader(buf_windfield);
                        int dummy = windfieldb.ReadInt32(); // read 4 bytes from stream = "Header"
                        if (dummy == -1) // Compact wnd File-format
                        {
                            dummy = windfieldb.ReadInt32(); // read 4 bytes from stream = Nx
                            dummy = windfieldb.ReadInt32(); // read 4 bytes from stream = Ny
                            dummy = windfieldb.ReadInt32(); // read 4 bytes from stream = Nz
                            float temp = windfieldb.ReadInt32(); // read 4 bytes from stream = DXX

                            byte[] _row = new byte[NZ * 6];

                            for (int i = 1; i <= NX; i++)
                            {
                                for (int j = 1; j <= NY; j++)
                                {
                                    _row = windfieldb.ReadBytes(NZ * 6);
                                    for (int k = 1; k <= NZ; k++)
                                    {
                                        int index = (k - 1) * 6;
                                        //UWI[i, j, k] = (float)windfieldb.ReadInt16() * 0.01F; // 2 Bytes  = word integer value;
                                        //VWI[i, j, k] = (float)windfieldb.ReadInt16() * 0.01F;
                                        //WWI[i, j, k] = (float)windfieldb.ReadInt16() * 0.01F;
                                        UWI[i, j, k] = BitConverter.ToInt16(_row, index) * 0.01F;
                                        VWI[i, j, k] = BitConverter.ToInt16(_row, index + 2) * 0.01F;
                                        WWI[i, j, k] = BitConverter.ToInt16(_row, index + 4) * 0.01F;
                                    }
                                }
                            }
                            windfieldb.Close(); windfieldb.Dispose();
                        }
                        else // classic windfield file format
                        {
                            windfieldb.Close(); windfieldb.Dispose();
                            string[] text = new string[1];
                            string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

                            using (StreamReader windfield = new StreamReader(filename))
                            {
                                for (int i = 1; i <= NX; i++)
                                {
                                    for (int j = 1; j <= NY; j++)
                                    {
                                        for (int k = 1; k <= NZ; k++)
                                        {
                                            text = windfield.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                            UWI[i, j, k] = (float)Convert.ToDouble(text[0].Replace(".", decsep));
                                            VWI[i, j, k] = (float)Convert.ToDouble(text[1].Replace(".", decsep));
                                            WWI[i, j, k] = (float)Convert.ToDouble(text[2].Replace(".", decsep));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return true; // Reader OK
            }
            catch
            {
                return false; // Reader Error
            }
        }

        // methode to export windfield-data for sub-domains
        /// <summary>
        /// Write a GRAMM wind field "*.wnd" for GRAMM sub-domains
        /// </summary>
        public bool Windfield_export()
        {
            try
            {
                using (FileStream str_windfield = new FileStream(_pathWindfield, FileMode.Create, FileAccess.Write))
                {
                    using (BufferedStream buf_windfield = new BufferedStream(str_windfield, 32768))
                    {
                        using (BinaryWriter writer = new BinaryWriter(buf_windfield))
                        {
                            int header = -1;
                            Int16 dummy;
                            float GRAMMhorgridsize = (float)_DDX;

                            //there are two different formats: IOUTPUT = 0 (standard output for GRAL-GUI users) and IOUTPUT = 3 for SOUNDPLAN USERS
                            writer.Write(header);
                            writer.Write(_NI);
                            writer.Write(_NJ);
                            writer.Write(_NK);
                            writer.Write(GRAMMhorgridsize);

                            for (int i = 1; i <= _NI; i++)
                            {
                                for (int j = 1; j <= _NJ; j++)
                                {
                                    for (int k = 1; k <= _NK; k++)
                                    {
                                        try
                                        {
                                            dummy = Convert.ToInt16(_U[i, j, k] * 100);
                                        }
                                        catch
                                        {
                                            if (_U[i, j, k] > 0)
                                            {
                                                dummy = Int16.MaxValue;
                                            }
                                            else
                                            {
                                                dummy = Int16.MinValue;
                                            }
                                        }
                                        writer.Write(dummy);

                                        try
                                        {
                                            dummy = Convert.ToInt16(_V[i, j, k] * 100);
                                        }
                                        catch
                                        {
                                            if (_V[i, j, k] > 0)
                                            {
                                                dummy = Int16.MaxValue;
                                            }
                                            else
                                            {
                                                dummy = Int16.MinValue;
                                            }
                                        }
                                        writer.Write(dummy);
                                        try
                                        {
                                            dummy = Convert.ToInt16(_W[i, j, k] * 100);
                                        }
                                        catch
                                        {
                                            if (_W[i, j, k] > 0)
                                            {
                                                dummy = Int16.MaxValue;
                                            }
                                            else
                                            {
                                                dummy = Int16.MinValue;
                                            }
                                        }
                                        writer.Write(dummy);
                                    }
                                }
                            }
                        }
                    }
                }
                return true; // Writer OK
            }
            catch
            {
                return false; // Writer Error
            }

        }
    }
}
