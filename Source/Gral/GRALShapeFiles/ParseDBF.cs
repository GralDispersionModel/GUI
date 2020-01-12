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
 * Date: 30.08.2017
 * Time: 13:29
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace GralShape
{
    /// <summary>
    /// Parse a *.dbf file 
    /// </summary>
    public partial class ParseDBF : Form
	{
		private DataTable _dt;
		public DataTable dt {set {_dt = value;} get {return _dt;} }
		private Encoding encoding =  Encoding.GetEncoding(1252);
		private CultureInfo cultureSettings = CultureInfo.InvariantCulture;
			
		// This is the file header for a DBF. We do this special layout with everything
		// packed so we can read straight from disk into the structure to populate it
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		private struct DBFHeader
		{
			public byte Version;
			public byte UpdateYear;
			public byte UpdateMonth;
			public byte UpdateDay;
			public Int32 NumRecords;
			public Int16 HeaderLen;
			public Int16 RecordLen;
			public Int16 Reserved1;
			public byte IncompleteTrans;
			public byte EncryptionFlag;
			public Int32 Reserved2;
			public Int64 Reserved3;
			public byte MDX;
			public byte Language;
			public Int16 Reserved4;
		}

		// This is the field descriptor structure. There will be one of these for each column in the table.
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		private struct FieldDescriptor
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
			public string fieldName;
			public char fieldType;
			public Int32 address;
			public byte fieldLen;
			public byte count;
			public Int16 reserved1;
			public byte workArea;
			public Int16 reserved2;
			public byte flag;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
			public byte[] reserved3;
			public byte indexFlag;
		}
		
		public ParseDBF()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			if (radioButton1.Checked)
				encoding = Encoding.UTF8;
			if (radioButton2.Checked)
				encoding = Encoding.GetEncoding(1252);
			if (radioButton3.Checked)
				encoding = Encoding.ASCII;
			if (radioButton4.Checked)
				encoding = Encoding.GetEncoding(28591);
			if (radioButton5.Checked)
				encoding = Encoding.UTF7;
			if (radioButton6.Checked)
				encoding = Encoding.Unicode;
			if (radioButton7.Checked)
				encoding = Encoding.UTF32;

            if (radioButton9.Checked) // use local culture to parse numbers
            {
                cultureSettings = CultureInfo.CurrentCulture;
            }
		}
				
		// Read an entire standard DBF file into a DataTable
		public void ReadDBF(string dbfFile)
		{
			ShowDialog();
					
			long start = DateTime.Now.Ticks;
			_dt = new DataTable();
			BinaryReader recReader;
			string number;
			string year;
			string month;
			string day;
			long lDate;
			long lTime;
			DataRow row;
			int	fieldIndex;

			// If there isn't even a file, just return an empty DataTable
			if ((false == File.Exists(dbfFile)))
			{
				return;
			}

			BinaryReader br = null;
			try
			{
				// Read the header into a buffer
				br = new BinaryReader(File.OpenRead(dbfFile));
				byte[] buffer = br.ReadBytes(Marshal.SizeOf(typeof(DBFHeader)));

				// Marshall the header into a DBFHeader structure
				GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
				DBFHeader header = (DBFHeader) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DBFHeader));
				handle.Free();
				
				// Read in all the field descriptors. Per the spec, 13 (0D) marks the end of the field descriptors
				ArrayList fields = new ArrayList();
				while ((13 != br.PeekChar()))
				{
					buffer = br.ReadBytes(Marshal.SizeOf(typeof(FieldDescriptor)));
					handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
					fields.Add((FieldDescriptor)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FieldDescriptor)));
					handle.Free();
				}

				// Read in the first row of records, we need this to help determine column types below
				((FileStream) br.BaseStream).Seek(header.HeaderLen + 1, SeekOrigin.Begin);
				buffer = br.ReadBytes(header.RecordLen);
				recReader = new BinaryReader(new MemoryStream(buffer));

				// Create the columns in our new DataTable
				DataColumn col = null;
				foreach (FieldDescriptor field in fields)
				{
					//Oettl, 2011: Enable "," as decimal seperator
					number = Encoding.Default.GetString(recReader.ReadBytes(field.fieldLen));

					switch (field.fieldType)
					{
						case 'N':
							if (number.IndexOf(".") > -1)
							{
								col = new DataColumn(field.fieldName, typeof(decimal));
							}
							else
							{
								col = new DataColumn(field.fieldName, typeof(int));
							}
							break;
						case 'C':
							col = new DataColumn(field.fieldName, typeof(string));
							break;
						case 'T':
							// You can uncomment this to see the time component in the grid
							//col = new DataColumn(field.fieldName, typeof(string));
							col = new DataColumn(field.fieldName, typeof(DateTime));
							break;
						case 'D':
							col = new DataColumn(field.fieldName, typeof(DateTime));
							break;
						case 'L':
							col = new DataColumn(field.fieldName, typeof(bool));
							break;
						case 'F':
							col = new DataColumn(field.fieldName, typeof(Double));
							break;
					}
					_dt.Columns.Add(col);
					_dt.AcceptChanges();
				}

				// Skip past the end of the header.
				((FileStream) br.BaseStream).Seek(header.HeaderLen, SeekOrigin.Begin);
				
				
				// Read in all the records
				for (int counter = 0; counter <= header.NumRecords - 1; counter++)
				{
					// First we'll read the entire record into a buffer and then read each field from the buffer
					// This helps account for any extra space at the end of each record and probably performs better
					
					byte[] bufferloc = br.ReadBytes(header.RecordLen);
					
					recReader = new BinaryReader(new MemoryStream(bufferloc));

					// All dbf field records begin with a deleted flag field. Deleted - 0x2A (asterisk) else 0x20 (space)
					if (recReader.ReadChar() == '*')
					{
						continue;
					}

					// Loop through each field in a record
					fieldIndex = 0;
					
					row = _dt.NewRow();
					foreach (FieldDescriptor field in fields)
					{
						switch (field.fieldType)
						{
							case 'N':  // Number
								// If you port this to .NET 2.0, use the Decimal.TryParse method
								number = Encoding.Default.GetString(recReader.ReadBytes(field.fieldLen));
								decimal value;
								bool b = decimal.TryParse(number,NumberStyles.Any, cultureSettings, out value);
								//Oettl, 2011: To enable floating point numbers (xxe+xx) this conversion is performed
								try
								{
									number = value.ToString();
								}
								catch
								{
									number = "0";
								}
                                
                                if (IsNumber(number))
								{
									if (number.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) > -1)
									{
                                        try
                                        {
                                            row[fieldIndex] = decimal.Parse(number);
                                        }
                                        catch
                                        {
                                            row[fieldIndex] = 0;
                                        }
									}
									else
									{
                                        try
                                        {
                                            row[fieldIndex] = int.Parse(number);
                                        }
                                        catch
                                        {
                                            row[fieldIndex] = 0;
                                        }
									}
								}
								else
								{
									row[fieldIndex] = 0;
								}

								break;

							case 'C': // String
                                      //row[fieldIndex] = Encoding.Default.GetString(recReader.ReadBytes(field.fieldLen));
                                try
                                {
                                    row[fieldIndex] = encoding.GetString(recReader.ReadBytes(field.fieldLen));
                                }
                                catch
                                {
                                    row[fieldIndex] = "String error";
                                }
								break;

							case 'D': // Date (YYYYMMDD)
								year = Encoding.Default.GetString(recReader.ReadBytes(4));
								month = Encoding.Default.GetString(recReader.ReadBytes(2));
								day = Encoding.Default.GetString(recReader.ReadBytes(2));
								//row[fieldIndex] = System.DBNull.Value;
								try
								{
									if (IsNumber(year) && IsNumber(month) && IsNumber(day))
									{
										if ((Int32.Parse(year) > 1900))
										{
											row[fieldIndex] = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
										}
									}
								}
								catch
								{
                                    row[fieldIndex] = new DateTime(1901, 1, 1);
                                }

								break;

							case 'T': // Timestamp, 8 bytes - two integers, first for date, second for time
								// Date is the number of days since 01/01/4713 BC (Julian Days)
								// Time is hours * 3600000L + minutes * 60000L + Seconds * 1000L (Milliseconds since midnight)
								lDate = recReader.ReadInt32();
								lTime = recReader.ReadInt32() * 10000L;
                                try
                                {
                                    row[fieldIndex] = JulianToDateTime(lDate).AddTicks(lTime);
                                }
                                catch
                                {
                                    row[fieldIndex] = JulianToDateTime(0).AddTicks(1);
                                }
								break;

							case 'L': // Boolean (Y/N) or T/F - Kuntner 20.8.2017
								byte bbool = recReader.ReadByte();
								if (bbool == 'Y'|| bbool == 84)
								{
									row[fieldIndex] = true;
								}
								else
								{
									row[fieldIndex] = false;
								}

								break;

							case 'F':
								number = Encoding.Default.GetString(recReader.ReadBytes(field.fieldLen));
								b = decimal.TryParse(number,NumberStyles.Any, cultureSettings, out value);
                                //Oettl, 2011: To enable floating point numbers (xxe+xx) this conversion is performed
                                //number = Convert.ToString(Convert.ToDouble(number.Replace(".", decsep))).Replace(decsep, ".");
                                try
                                {
                                    number = value.ToString();
                                    if (IsNumber(number))
                                    {
                                        row[fieldIndex] = double.Parse(number);
                                    }
                                    else
                                    {
                                        row[fieldIndex] = 0.0F;
                                    }
                                }
                                catch
                                {
                                    row[fieldIndex] = 0.0F;
                                }
								break;
						}
						fieldIndex++;
					}

					recReader.Close();
					recReader.Dispose();
					bufferloc = null;
					
					_dt.Rows.Add(row);

				}
				
				handle.Free();
			}

			catch
			{
				//MessageBox.Show(ex.Message);
				//throw;
			}
			finally
			{
				if (null != br)
				{
					br.Close();
					br.Dispose();
				}
				
			}

			long count = DateTime.Now.Ticks - start;
			
			
			Close();
			return;
		}

		/// <summary>
		/// Simple function to test is a string can be parsed using the local culture
		/// </summary>
		/// <param name="numberString">string to test for parsing</param>
		/// <returns>true if string can be parsed</returns>
		public bool IsNumber(string numberString)
		{
            double _val = 0;
            bool isnumber = double.TryParse(numberString, out _val);
            return isnumber;
        }

		/// <summary>
		/// Convert a Julian Date to a .NET DateTime structure
		/// Implemented from pseudo code at http://en.wikipedia.org/wiki/Julian_day
		/// </summary>
		/// <param name="lJDN">Julian Date to convert (days since 01/01/4713 BC)</param>
		/// <returns>DateTime</returns>
		private  DateTime JulianToDateTime(long lJDN)
		{
            try
            {
                double p = Convert.ToDouble(lJDN);
                double s1 = p + 68569;
                double n = Math.Floor(4 * s1 / 146097);
                double s2 = s1 - Math.Floor((146097 * n + 3) / 4);
                double i = Math.Floor(4000 * (s2 + 1) / 1461001);
                double s3 = s2 - Math.Floor(1461 * i / 4) + 31;
                double q = Math.Floor(80 * s3 / 2447);
                double d = s3 - Math.Floor(2447 * q / 80);
                double s4 = Math.Floor(q / 11);
                double m = q + 2 - 12 * s4;
                double j = 100 * (n - 49) + i + s4;
                return new DateTime(Convert.ToInt32(j), Convert.ToInt32(m), Convert.ToInt32(d));
            }
            catch
            {
                return new DateTime(1901, 1, 1);
            }            
        }

    }
}
