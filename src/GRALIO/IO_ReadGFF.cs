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
using System.IO;
using System.Globalization;
using System.IO.Compression;

namespace GralIO
{
    /// <summary>
    /// Read GRAL flow field "*.gff" files
    /// </summary>
    public class ReadFlowFieldFiles
    {
        //User defineddecimal seperator
        private string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

        private string _filename;
        public string filename { set { _filename = value; } get { return _filename; } }

        private int _NKK;
        public int NKK { get { return _NKK; } }
        private int _NJJ;
        public int NJJ { get { return _NJJ; } }
        private int _NII;
        public int NII { get { return _NII; } }

        private int _AK;
        public int AK { get { return _AK; } }

        private float _Direction;
        public float Direction { get { return _Direction; } }
        private float _Speed;
        public float Speed { get { return _Speed; } }

        private float _Cellsize;
        public float Cellsize { get { return _Cellsize; } }

        private float[][][] _uk;
        public float[][][] Uk { set { _uk = value; } get { return _uk; } }
        private float[][][] _vk;
        public float[][][] Vk { set { _vk = value; } get { return _vk; } }
        private float[][][] _wk;
        public float[][][] Wk { set { _wk = value; } get { return _wk; } }
        private float[][][] _tke;
        public float[][][] TKE { set { _tke = value; } get { return _tke; } }


        /// <summary>
        /// Read gral flow field file *.gff
        /// Called from await -> async!
        /// </summary>
        public bool ReadGffFile(System.Threading.CancellationToken cts)
        {
            if (!File.Exists(_filename))
            {
                return false;
            }
            try
            {
                if (CheckIfFileIsZipped(_filename)) // file zipped?
                {
                    using (FileStream fs = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (BufferedStream bs = new BufferedStream(fs, 32768))
                        {
                            using (ZipArchive archive = new ZipArchive(bs, ZipArchiveMode.Read, false)) //open Zip archive
                            {
                                string filename = archive.Entries[0].FullName;
                                using (BinaryReader reader = new BinaryReader(archive.Entries[0].Open())) //OPen Zip entry
                                {
                                    return ReadGffData(reader, _filename, cts);
                                }
                            }
                        }
                    }
                }
                else // read uncompressed file
                {
                    using (FileStream fs = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (BufferedStream bs = new BufferedStream(fs, 32768))
                        {
                            using (BinaryReader reader = new BinaryReader(bs))
                            {
                                return ReadGffData(reader, _filename, cts);
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private bool ReadGffData(BinaryReader reader, string GRALflowfield, System.Threading.CancellationToken cts)
        {
            try
            {
                _NKK = Math.Abs(reader.ReadInt32()); // negative values at strong compressed files
                _NJJ = reader.ReadInt32();
                _NII = reader.ReadInt32();
                _Direction = reader.ReadSingle();
                _Speed = reader.ReadSingle();
                _AK = reader.ReadInt32();
                _Cellsize = reader.ReadSingle();
                int header = reader.ReadInt32();


                _uk = Landuse.CreateArray<float[][]>(_NII + 2, () => Landuse.CreateArray<float[]>(_NJJ + 2, () => new float[_NKK + 2]));
                _vk = Landuse.CreateArray<float[][]>(_NII + 2, () => Landuse.CreateArray<float[]>(_NJJ + 2, () => new float[_NKK + 2]));
                _wk = Landuse.CreateArray<float[][]>(_NII + 2, () => Landuse.CreateArray<float[]>(_NJJ + 2, () => new float[_NKK + 2]));
                
                if (header == -1)
                {
                    for (int i = 1; i <= _NII + 1; i++)
                    {
                        for (int j = 1; j <= _NJJ + 1; j++)
                        {
                            byte[] readData = reader.ReadBytes((_NKK + 1) * 6); // read all vertical bytes of one point
                            int index = 0;
                            cts.ThrowIfCancellationRequested();

                            for (int k = 1; k <= _NKK + 1; k++)
                            {
                                _uk[i][j][k - 1] = (float)(BitConverter.ToInt16(readData, index) * 0.01F); // 2 Bytes  = word integer value
                                index += 2;
                                _vk[i][j][k - 1] = (float)(BitConverter.ToInt16(readData, index) * 0.01F);
                                index += 2;
                                _wk[i][j][k] = (float)(BitConverter.ToInt16(readData, index) * 0.01F);
                                index += 2;
                            }
                        }
                    }
                }
                else if (header == -2)
                {
                    for (int k = 1; k <= _NKK + 1; k++)
                    {
                        for (int i = 1; i <= _NII + 1; i++)
                        {
                           
                            cts.ThrowIfCancellationRequested();

                            // read each component after the other one -> better compression
                            byte[] readData = reader.ReadBytes((_NJJ + 1) * 6); // read one horizontal row 
                            //byte[] readDataVK = reader.ReadBytes((_NJJ + 1) * 2); // read one horizontal row 
                            //byte[] readDataWK = reader.ReadBytes((_NJJ + 1) * 2); // read one horizontal row 
                            int offset = (_NJJ + 1) * 2;
                            System.Threading.Tasks.Parallel.For(1, _NJJ + 1, j =>
                            {
                                int index = (j - 1) * 2;
                                
                                _uk[i][j][k - 1] = (float)((Int16)(readData[index] | (readData[index + 1] << 8))) * 0.01F; // 2 Bytes  = word integer value
                                index += offset;
                                _vk[i][j][k - 1] = (float)((Int16)(readData[index] | (readData[index + 1] << 8))) * 0.01F;
                                index += offset;
                                _wk[i][j][k] = (float)((Int16)(readData[index] | (readData[index + 1] << 8))) * 0.01F;
                            });
                        }
                    }
                }
                else if (header == -3)
                {
                    Int16[,] _KKART = new Int16[_NII + 2, _NJJ + 2];
                    for (int i = 1; i < _NII + 1; i++)
                    {
                        cts.ThrowIfCancellationRequested();

                        for (int j = 1; j < _NJJ + 1; j++)
                        {
                            _KKART[i, j] = reader.ReadInt16();
                        }
                    }

                    byte[] readData;

                    for (int i = 1; i <= _NII + 1; i++)
                    {
                        cts.ThrowIfCancellationRequested();

                        for (int j = 1; j <= _NJJ + 1; j++)
                        {
                            int KKART = _KKART[i, j];
                            if (KKART < 1)
                            {
                                KKART = 1;
                            }
                            readData = reader.ReadBytes((_NKK + 2 - KKART) * 6); // read all vertical bytes of one point
                            int index = 0;

                            for (int k = 1; k < KKART; k++)
                            {
                                _uk[i][j][k - 1] = 0;
                                _vk[i][j][k - 1] = 0;
                                _wk[i][j][k] = 0;
                            }
                            for (int k = KKART; k <= _NKK + 1; k++)
                            {
                                _uk[i][j][k - 1] = (float)(BitConverter.ToInt16(readData, index) * 0.01F); // 2 Bytes  = word integer value
                                index += 2;
                            }
                            for (int k = KKART; k <= _NKK + 1; k++)
                            {
                                _vk[i][j][k - 1] = (float)(BitConverter.ToInt16(readData, index) * 0.01F);
                                index += 2;
                            }
                            for (int k = KKART; k <= _NKK + 1; k++)
                            {
                                _wk[i][j][k] = (float)(BitConverter.ToInt16(readData, index) * 0.01F);
                                index += 2;
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check, if a file is zipped
        /// </summary>
        public static bool CheckIfFileIsZipped(string file)
        {
            try
            {
                int zipheader = 0;
                const int ZIP_LEAD_BYTES = 0x04034b50;

                if (File.Exists(file) == false)
                {
                    return false;
                }

                using (BinaryReader header = new BinaryReader(File.Open(file, FileMode.Open)))
                {
                    zipheader = header.ReadInt32();
                    if (zipheader == ZIP_LEAD_BYTES) // file is zipped
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Read gral flow field file *.gff and TKE data
        /// Called from await -> asnc!
        /// </summary>
        public bool ReadTKEFile(System.Threading.CancellationToken cts)
        {
            try
            {
                if (CheckIfFileIsZipped(_filename)) // file zipped?
                {
                    using (FileStream fs = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Read, false)) //open Zip archive
                        {
                            string filename = archive.Entries[0].FullName;
                            using (BinaryReader reader = new BinaryReader(archive.Entries[0].Open())) //OPen Zip entry
                            {
                                return ReadTKEData(reader, _filename, cts);
                            }
                        }
                    }
                }
                else // read uncompressed file
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(_filename, FileMode.Open)))
                    {
                        return ReadTKEData(reader, _filename, cts);
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private bool ReadTKEData(BinaryReader reader, string GRALflowfield, System.Threading.CancellationToken cts)
        {
            try
            {
                _NKK = reader.ReadInt32();
                _NJJ = reader.ReadInt32();
                _NII = reader.ReadInt32();
                _Direction = reader.ReadSingle();
                _Speed = reader.ReadSingle();
                _AK = reader.ReadInt32();
                _Cellsize = reader.ReadSingle();
                int header = reader.ReadInt32();

                _tke = Landuse.CreateArray<float[][]>(_NII + 1, () => Landuse.CreateArray<float[]>(_NJJ + 1, () => new float[_NKK + 1]));

                for (int i = 1; i <= _NII; i++)
                {
                    for (int j = 1; j <= _NJJ; j++)
                    {
                        cts.ThrowIfCancellationRequested();

                        for (int k = 1; k <= _NKK; k++)
                        {
                            _tke[i][j][k] = (float)(reader.ReadInt16() * 0.01F);
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
