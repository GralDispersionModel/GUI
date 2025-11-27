#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2021]  [Markus Kuntner]
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

    public class ReadESRIFile
    {
        private readonly CultureInfo ic = CultureInfo.InvariantCulture;

        /// <summary>
        /// Read an ESRI ASCii File to jagged array
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns>Array, Header object and min and max double values exception string</returns>
#if NET6_0_OR_GREATER
        public (double[][], ESRIHeader, double, double, string) ReadESRIFileJagged(string FileName)
#else
        public double[][] ReadESRIFileJagged(string FileName, ref ESRIHeader header)
#endif
        {
            double[][] A = null;
#if NET6_0_OR_GREATER
            ESRIHeader header = new ESRIHeader();
            double min = 100000000;
            double max = -100000000;
            string exception = string.Empty;
#else
            double min = 100000000;
            double max = -100000000;
            string exception = string.Empty;
#endif
            try
            {
                using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader myReader = new StreamReader(fs))
                    {
                        if (!header.ReadESRIHeader(myReader))
                        {
                            throw new IOException();
                        }

                        int col = header.NCols;
                        int Row = header.NRows;
                        int nodata = header.NoDataValue;
                        double value = 0;
                        A = CreateArray<double[]>(col, () => new double[Row]);
                        char[] seperat = new char[] { ' ', '\t', ';', ',' };
                        string oneLine = String.Empty;

                        for (int y = header.NRows - 1; y >= 0; y--)
                        {
#if NET6_0_OR_GREATER
                            oneLine = myReader.ReadLine();
                            ReadOnlySpan<char> _span = oneLine.AsSpan();
                            int nextSep = 0;
                            int startSearch = 0;
                            int spanLenght = _span.Length;
                            int x = 0;
                            while ((nextSep = _span.Slice(startSearch).IndexOfAny(seperat)) >= 0)
                            {
                                if (Double.TryParse(_span.Slice(startSearch, nextSep), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
                                {
                                    A[x++][y] = value;
                                    if ((int)(value) != nodata)
                                    {
                                        min = Math.Min(min, value);
                                        max = Math.Max(max, value);
                                    }
                                }
                                else
                                {
                                    A[x++][y] = nodata;
                                }
                                startSearch += nextSep + 1;
                            }
                            if (startSearch < nextSep)
                            {
                                if (Double.TryParse(_span.Slice(startSearch, nextSep), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
                                {
                                    A[x][y] = value;
                                    if ((int)(value) != nodata)
                                    {
                                        min = Math.Min(min, value);
                                        max = Math.Max(max, value);
                                    }
                                }
                                else
                                {
                                    A[x][y] = nodata;
                                }
                            }
#else
                            string[] data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int x = 0; x < header.NCols; x++)
                            {
                                if (Double.TryParse(data[x], System.Globalization.NumberStyles.Float, ic, out value))
                                {
                                    A[x][y] = value;
                                    if ((int)(value) != nodata)
                                    {
                                        min = Math.Min(min, value);
                                        max = Math.Max(max, value);
                                    }
                                }
                            }
#endif
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                A = null;
                exception = ex.Message;
            }
#if NET6_0_OR_GREATER
            return (A, header, min, max, exception);
#else
            return A;
#endif
        }

        /// <summary>
        /// Read an ESRI ASCii File to multi dimensional array
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns>Array, Header object and min and max double values</returns
#if NET6_0_OR_GREATER
        public (double[,], ESRIHeader, double, double, string) ReadESRIFileMultiDim(string FileName)
#else
        public double[,] ReadESRIFileMultiDim(string FileName, ref ESRIHeader header, ref double min, ref double max, ref string exception)
#endif
        {
            double[,] A = null;
#if NET6_0_OR_GREATER
            ESRIHeader header = new ESRIHeader();
            double min = 100000000;
            double max = -100000000;
            string exception = string.Empty;
#else
            min = 100000000;
            max = -100000000;
            exception = string.Empty;
#endif
            try
            {
                using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader myReader = new StreamReader(fs))
                    {
                        if (!header.ReadESRIHeader(myReader))
                        {
                            throw new IOException();
                        }

                        int col = header.NCols;
                        int Row = header.NRows;
                        int nodata = header.NoDataValue;
                        double value = 0;
                        A = new double[col, Row];
                        char[] seperat = new char[] { ' ', '\t', ';', ',' };
                        string oneLine = String.Empty;

                        for (int y = header.NRows - 1; y >= 0; y--)
                        {
#if NET6_0_OR_GREATER
                            oneLine = myReader.ReadLine();
                            ReadOnlySpan<char> _span = oneLine.AsSpan();
                            int nextSep = 0;
                            int startSearch = 0;
                            int spanLenght = _span.Length;
                            int x = 0;
                            while ((nextSep = _span.Slice(startSearch).IndexOfAny(seperat)) >= 0)
                            {
                                if (Double.TryParse(_span.Slice(startSearch, nextSep), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
                                {
                                    A[x++, y] = value;
                                    if ((int)(value) != nodata)
                                    {
                                        min = Math.Min(min, value);
                                        max = Math.Max(max, value);
                                    }
                                }
                                else
                                {
                                    A[x++, y] = nodata;
                                }
                                startSearch += nextSep + 1;
                            }
                            if (startSearch < nextSep)
                            {
                                if (Double.TryParse(_span.Slice(startSearch, nextSep), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
                                {
                                    A[x, y] = value;
                                    if ((int)(value) != nodata)
                                    {
                                        min = Math.Min(min, value);
                                        max = Math.Max(max, value);
                                    }
                                }
                                else
                                {
                                    A[x, y] = nodata;
                                }
                            }
#else
                            string[] data = myReader.ReadLine().Split(seperat, StringSplitOptions.RemoveEmptyEntries);
                            for (int x = 0; x < header.NCols; x++)
                            {
                                if (Double.TryParse(data[x], System.Globalization.NumberStyles.Float, ic, out value))
                                {
                                    A[x, y] = value;
                                    if ((int)(value) != nodata)
                                    {
                                        min = Math.Min(min, value);
                                        max = Math.Max(max, value);
                                    }
                                }
                            }
#endif
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                A = null;
                exception = ex.Message;
            }
#if NET6_0_OR_GREATER
            return (A, header, min, max, exception);
#else
            return A;
#endif
        }

        ///<summary>
        /// Create a jagged array
        /// </summary>
        private static T[] CreateArray<T>(int cnt, Func<T> itemCreator)
        {
            T[] result = new T[cnt];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = itemCreator();
            }
            return result;
        }

    }

    /// <summary>
    /// Class for the ESRI file header info
    /// </summary>
    public class ESRIHeader
    {
        private int _ncols = 0;
        private int _nrows = 0;
        private double _xllCorner = 0;
        private double _yllCorner = 0;
        private double _cellSize = 1;
        private int _noDataVal = -9999;
        private string _unit = string.Empty;
        private bool _verticalConcentrationMap = false;
        private readonly CultureInfo ic = CultureInfo.InvariantCulture;
        private float _maxHeight = 0;

        public int NCols { get => _ncols; }
        public int NRows { get => _nrows; }
        public double XllCorner { get => _xllCorner; }
        public double YllCorner { get => _yllCorner; }
        public double Cellsize { get => _cellSize; }
        public int NoDataValue { get => _noDataVal; }
        public string Unit { get => _unit; }
        public bool VerticalConcentrationMap { get => _verticalConcentrationMap; }
        public float MaxHeight { get => _maxHeight; }

        /// <summary>
        /// Read the header of an ESRI ASCii file and set the public variables
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns>true if successful</returns>
        public bool ReadESRIHeader(string FileName)
        {
            bool ok = false;
            try
            {
                if (File.Exists(FileName))
                {
                    using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader myReader = new StreamReader(fs))
                        {
                            ok = ReadESRIHeader(myReader);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return ok;
        }

        /// <summary>
        /// Read the ESRI Header
        /// </summary>
        /// <param name="myReader">A streamreader object</param>
        /// <returns>true if successful, otherwise false</returns>
        public bool ReadESRIHeader(StreamReader myReader)
        {
            try
            {
                string[] data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                _ncols = Convert.ToInt32(data[1]);
                data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                _nrows = Convert.ToInt32(data[1]);
                data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                _xllCorner = Convert.ToDouble(data[1], ic);
                data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                _yllCorner = Convert.ToDouble(data[1], ic);
                data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                _cellSize = Convert.ToDouble(data[1], ic);
                data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                _noDataVal = Convert.ToInt32(data[1]);
                if (data.Length > 3) // read unit, if available
                {
                    _unit = data[3].Trim();
                }
                if (data.Length > 4) // Check if map is a vertical concentration profile
                {
                    try
                    {
                        _maxHeight = Convert.ToSingle(data[data.Length - 2], ic);
                    }
                    catch { }

                    if (data[data.Length - 3].Contains("Vertical_concentration"))
                    {
                        _verticalConcentrationMap = true;
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
