#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2020]  [Markus Kuntner]
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
using System.Windows.Forms;

namespace GralIO
{
    /// <summary>
    /// Data class for the Esri Ascii file header
    /// </summary>
    public class EsriGridHeader
    {
        /// <summary>
        /// Number of cells in X-direction
        /// </summary>
        public int NCols { set { _NCols = value;} get {return _NCols;}}
        private int _NCols;
        /// <summary>
        /// Number of cells in Y-direction
        /// </summary>
        public int NRows { set { _NRows = value; } get { return _NRows;}}
        private int _NRows;
        /// <summary>
        /// X coordinate of the origin (west)
        /// </summary>
        public double XllCorner { set { _XllCorner = value; } get { return _XllCorner; } }
        private double _XllCorner;
        /// <summary>
        /// Y coordinate of the origin (south)
        /// </summary>
        public double YllCorner { set { _YllCorner = value; } get { return _YllCorner; } }
        private double _YllCorner;
        /// <summary>
        /// Cell size
        /// </summary>
        public double CellSize { set { _CellSize = value; } get { return _CellSize; } }
        private double _CellSize;
        /// <summary>
        /// NoData value
        /// </summary>
        public double NoDataValue { set { _NoDataValue = value; } get { return _NoDataValue; } }
        private double _NoDataValue;
        /// <summary>
        /// Y coordinate of the northern border
        /// </summary>
        public double NorthernBorder { set { _NorthernBorder = value; } get { return _NorthernBorder; } }
        private double _NorthernBorder;
        /// <summary>
        /// X coordinate of the eastern border
        /// </summary>
        public double EasternBorder { set { _EasternBorder = value; } get { return _EasternBorder; } }
        private double _EasternBorder;
        /// <summary>
        /// Cell center
        /// </summary>
        public bool CellCenter { set { _CellCenter = value; } get { return _CellCenter; } }
        private bool _CellCenter = false;
        private readonly string DecSep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;


        /// <summary>
        /// Read the header of an Esri Ascii File
        /// </summary>
        /// <param name="FileName">The Filename for reading the header</param>
        /// <returns></returns>
        public bool ReadEsriGridHeader(string FileName)
        {
            bool ReadingOk = false;
            try
            {
                using (StreamReader reader = new StreamReader(FileName))
                {
                    ReadingOk = ReadHeader(reader);
                }
            }
            catch
            {
                ReadingOk = false;
            }
            return ReadingOk;
        }

        /// <summary>
        /// Read the header of an Esri Ascii File
        /// </summary>
        /// <param name="Reader">An already open StreamReader</param>
        /// <returns></returns>
        public bool ReadEsriGridHeader(StreamReader Reader)
        {
            return ReadHeader(Reader);
        }

        private bool ReadHeader(StreamReader Reader)
        {
            try
            {
                char[] splitchar = new char[] { ' ' };
                bool centerX = false;
                bool centerY = false;

                string[] text = Reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                _NCols = Convert.ToInt32(text[1]);
                text = Reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                _NRows = Convert.ToInt32(text[1]);
                text = Reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                _XllCorner = Convert.ToDouble(text[1].Replace(".", DecSep));
                centerX = text[0].ToUpper().Equals("XLLCENTER");

                text = Reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                centerX = text[0].ToUpper().Equals("YLLCENTER");
                _YllCorner = Convert.ToDouble(text[1].Replace(".", DecSep));
                text = Reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                _CellSize = Convert.ToDouble(text[1].Replace(".", DecSep));  //grid size

                // if cell center is used in the Esri File
                if (centerX && centerY)
                {
                    CellCenter = true;
                }
                
                text = Reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                _NoDataValue = Convert.ToDouble(text[1].Replace(".", DecSep));  //no-data value
                _NorthernBorder = _YllCorner + _NRows * _CellSize;
                _EasternBorder = _XllCorner + _NCols * _CellSize;
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
