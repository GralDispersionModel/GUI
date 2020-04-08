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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GralIO;
using Gral;

namespace GralMainForms
{
	/// <summary>
    /// Load a meteorological file
    /// </summary>
    public partial class MeteoInput: Form
	{
		public string MetFile1;
		public int FfileLength1;
		public string DecSep1;
		public string DecSepUser=".";
		public char RowSep = ',';
		public List<GralData.WindData> Met_Time_Ser; 
		
		private Main form1 = null;
		private string[] _startdate = new string[3];
		private string[] _enddate = new string[3];
		private List<GralData.WindData> winddata = new List<GralData.WindData>();

		public MeteoInput(Main f)
		{
			InitializeComponent();
			form1 = f;
		}


		private void Button1_Click(object sender, EventArgs e)
		{
			//hide calendars
			label1.Visible = false;
			label2.Visible = false;
			monthCalendar1.Visible = false;
			monthCalendar2.Visible = false;

			//fill Listbox
			listView1.Clear();
			listView1.GridLines = true;
			listView1.Columns.Add("Date", 50, HorizontalAlignment.Center);
			listView1.Columns.Add("Hour", 40, HorizontalAlignment.Center);
			listView1.Columns.Add("Vel. [m/s]", 70, HorizontalAlignment.Center);
			listView1.Columns.Add("Dir. [deg]", 70, HorizontalAlignment.Center);
			listView1.Columns.Add("SC", 40, HorizontalAlignment.Center);

			//reading the file and storing the data in variables
			winddata.Clear();
            IO_ReadFiles readwindfile = new IO_ReadFiles
            {
                WindDataFile = MetFile1,
                WindData = winddata
            };
            readwindfile.ReadMeteoFiles(1000000, RowSep, DecSep1, DecSepUser);
			
			int index = Math.Min(50, winddata.Count);
			int n1 = 0;
			foreach(GralData.WindData wd in winddata)
			{
				
				if (n1 < index) // fill preview
				{
					
					ListViewItem item = new ListViewItem(wd.Date);
					//if (zeile.GetLength(0) > 4)
					try
					{
						item.SubItems.Add(wd.Time);
						item.SubItems.Add(wd.Vel.ToString());
						item.SubItems.Add(wd.Dir.ToString());
						item.SubItems.Add(wd.StabClass.ToString());
					}
					catch
					{
					}
					listView1.Items.Add(item);
				}
				
				n1++;
			}

			//show month calender to select the desired time period of the time series
			try
			{
				_startdate = winddata[0].Date.Split(new char[] { '.', ':', '-' });
				_enddate = winddata[winddata.Count - 1].Date.Split(new char[] { '.', ':', '-' });
				if (Convert.ToInt16(_startdate[2]) < 1900)
				{
					if (Convert.ToInt16(_startdate[2]) < 60)
                    {
                        _startdate[2] = "20" + _startdate[2];
                    }
                    else
                    {
                        _startdate[2] = "19" + _startdate[2];
                    }
                }
				if (Convert.ToInt16(_enddate[2]) < 1900)
				{
					if (Convert.ToInt16(_enddate[2]) < 60)
                    {
                        _enddate[2] = "20" + _enddate[2];
                    }
                    else
                    {
                        _enddate[2] = "19" + _enddate[2];
                    }
                }

				monthCalendar1.MinDate = new System.DateTime(Convert.ToInt16(_startdate[2]), Convert.ToInt16(_startdate[1]), Convert.ToInt16(_startdate[0]), 0, 0, 0, 0);
				monthCalendar1.MaxDate = new System.DateTime(Convert.ToInt16(_enddate[2]), Convert.ToInt16(_enddate[1]), Convert.ToInt16(_enddate[0]), 0, 0, 0, 0);
				monthCalendar2.MinDate = new System.DateTime(Convert.ToInt16(_startdate[2]), Convert.ToInt16(_startdate[1]), Convert.ToInt16(_startdate[0]), 0, 0, 0, 0);
				monthCalendar2.MaxDate = new System.DateTime(Convert.ToInt16(_enddate[2]), Convert.ToInt16(_enddate[1]), Convert.ToInt16(_enddate[0]), 0, 0, 0, 0);
				label1.Visible = true;
				label2.Visible = true;
				monthCalendar1.Visible = true;
				monthCalendar2.Visible = true;
				monthCalendar1.SelectionStart = new System.DateTime(Convert.ToInt16(_startdate[2]), Convert.ToInt16(_startdate[1]), Convert.ToInt16(_startdate[0]), 0, 0, 0, 0);

				monthCalendar1.SetDate(monthCalendar1.MinDate);
				monthCalendar2.SetDate(monthCalendar2.MaxDate);
				#if __MonoCS__
				#else
				monthCalendar2.TodayDate = monthCalendar2.MaxDate;			
				#endif
				
				readwindfile.WindData.Clear();
				readwindfile.WindData.TrimExcess();
				readwindfile = null;
			}
			catch
			{
				
			}
		}

		private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			
		}

		private void Button2_Click(object sender, EventArgs e)
		{
			//reading the file and storing the data in variables
			bool met = true;
			
			winddata.Clear();
            IO_ReadFiles readwindfile = new IO_ReadFiles
            {
                WindDataFile = MetFile1,
                WindData = winddata
            };

            if (readwindfile.ReadMeteoFiles(1000000, RowSep, DecSep1, DecSepUser) == false)
			{
				MessageBox.Show(this, "Error when reading Meteo-File in line" + readwindfile.WindData.Count, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
				if (winddata.Count == 0)
                {
                    met = false;
                }
            }
			
			int length = 0;
			
			Met_Time_Ser.Clear();
			Met_Time_Ser.TrimExcess();
			
			if (met)
			{
				int n1 = 0; 
				string[] date = new string[3];
				int error_count = 0;

				foreach(GralData.WindData wd in winddata)
				{
					date = wd.Date.Split(new char[] { ':', '-', '.' });
					//check whether the date is within the chosen time interval
					if (Convert.ToInt16(date[2]) < 1900)
					{
						if (Convert.ToInt16(date[2]) < 60)
                        {
                            date[2] = "20" + date[2];
                        }
                        else
                        {
                            date[2] = "19" + date[2];
                        }
                    }
					
					if (((Convert.ToInt32(date[2]) * 10000 + Convert.ToInt16(date[1]) * 100 + Convert.ToInt16(date[0])) >= (Convert.ToInt32(_startdate[2]) * 10000 + Convert.ToInt16(_startdate[1]) * 100 + Convert.ToInt16(_startdate[0]))) &&
					    ((Convert.ToInt32(date[2]) * 10000 + Convert.ToInt16(date[1]) * 100 + Convert.ToInt16(date[0])) <= (Convert.ToInt32(_enddate[2]) * 10000 + Convert.ToInt16(_enddate[1]) * 100 + Convert.ToInt16(_enddate[0]))))
					{
						wd.Date = date[0] + "." + date[1] + "." + date[2];
							
						//check for unplausible values
						if(wd.Vel > 100 || wd.Vel < 0)
						{
							if (error_count < 4)
                            {
                                MessageBox.Show(this, "Wind speed implausible - check line number" + Convert.ToString(n1 + 1), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            error_count++;
						}
						else if (wd.Dir > 360 || wd.Dir < 0)
						{
							if (error_count < 4)
                            {
                                MessageBox.Show(this, "Wind direction implausible - check line number" + Convert.ToString(n1 + 1), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            error_count++;
						}
						else if (wd.Dir == 0 && wd.Vel == 0 && wd.StabClass == 0) // invalid line
						{
							error_count++;
						}
						else if (wd.StabClass > 7 || wd.StabClass < 1)
						{
							if (error_count < 4)
                            {
                                MessageBox.Show(this, "Stability class implausible - check line number" + Convert.ToString(n1 + 1), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            error_count++;
						}
						else
                        {
							//add meteo data if no error was found
							Met_Time_Ser.Add(wd);
							length++;
                        }
                    }
					n1++;
				}
				
				if (error_count > 0)
                {
                    MessageBox.Show(this, error_count.ToString() + " invalid wind data lines", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
			
			readwindfile.WindData.Clear();
			readwindfile.WindData.TrimExcess();
			readwindfile.WindData = null;
			readwindfile = null;
			
			//draw met. analysis tools only when met. data was successfully loaded
			if (form1 != null)
			{
				if (met == true)
				{
					form1.groupBox1.Visible = true;
					form1.button6.Visible = true;
				}
				form1.MetoColumnSeperator = RowSep;
				form1.MeteoDecSeperator = DecSepUser;
			}
			
			winddata.Clear();
			winddata.TrimExcess();
			winddata = null;
			Close();
			Dispose();
			//GC.Collect();
		}

		//different row-and decimal separators defined by the user

		private void RadioButton2_CheckedChanged(object sender, EventArgs e)
		{
			DecSepUser = ".";
		}

		private void RadioButton1_CheckedChanged(object sender, EventArgs e)
		{
			DecSepUser = ",";
		}

		private void RadioButton3_CheckedChanged(object sender, EventArgs e)
		{
			RowSep = ',';
		}

		private void RadioButton4_CheckedChanged(object sender, EventArgs e)
		{
			RowSep = ' ';
		}

		private void RadioButton5_CheckedChanged(object sender, EventArgs e)
		{
			RowSep = ';';
		}

		private void RadioButton6_CheckedChanged(object sender, EventArgs e)
		{
			RowSep = '\t';
		}

		private void Form2_Load(object sender, EventArgs e)
		{
			if (Owner != null)
            {
                Location = new Point(Math.Max(0,Owner.Location.X + Owner.Width / 2 - Width / 2 - 100),
				                     Math.Max(0, Owner.Location.Y + Owner.Height / 2 - Height / 2 -100));
            }
        }

		//select start date for met data input
		private void MonthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
		{
			monthCalendar1.TodayDate = monthCalendar1.SelectionStart;
			DateTime startD = monthCalendar1.TodayDate;
			_startdate[2] = Convert.ToString(startD.Year);
			_startdate[1] = Convert.ToString(startD.Month);
			_startdate[0] = Convert.ToString(startD.Day);
		}

		//select end date for met data input
		private void MonthCalendar2_DateChanged(object sender, DateRangeEventArgs e)
		{
			monthCalendar2.TodayDate = monthCalendar2.SelectionStart;
			DateTime startD = monthCalendar2.TodayDate;
			_enddate[2] = Convert.ToString(startD.Year);
			_enddate[1] = Convert.ToString(startD.Month);
			_enddate[0] = Convert.ToString(startD.Day);
		}
        
        void MeteoInputFormClosed(object sender, FormClosedEventArgs e)
        {
        	listView1.Items.Clear();
        	listView1.Dispose();
        	monthCalendar1.Dispose();
        	monthCalendar2.Dispose();
        }
	}
}