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
 * User: Markus
 * Date: 03.01.2019
 * Time: 17:01
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using Gral;
using System.Linq;

namespace GralDomForms
{
	/// <summary>
	/// This dialog is used to load wind roses
	/// </summary>
	public partial class DomainWindRoseDialog : Form
	{
		public GralDomain.DrawingObjects DrawingObject; // Drawing Object with Windrose infos
		private CultureInfo ic = CultureInfo.InvariantCulture;
		private List<GralData.WindData> MeteoTimeSeries = new List<GralData.WindData>();      // time series of meteorological  data
		private List <WindFileData> Windfiles = new List<WindFileData>();
		
		public DomainWindRoseDialog()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void DomainWindRoseDialogLoad(object sender, EventArgs e)
		{
			string[] w = DrawingObject.ContourFilename.Split(new char[] { '\t' });
			Windfiles.Clear();
			Windfiles.TrimExcess();
			
			foreach(string _wnd in w)
			{
				string[] f = _wnd.Split(new char[] { '?' }); // get 1st entry = filename
				if (f.Length > 2)
				{
					try
					{
						WindFileData _wdata = new WindFileData
						{
							Filename = f[0],
							DecSep = f[1],
							RowSep = Convert.ToChar(f[2]),
							X0 = Convert.ToDouble(f[3], ic),
							Y0 = Convert.ToDouble(f[4], ic),
							Z0 = Convert.ToDouble(f[5], ic)
						};
						Windfiles.Add(_wdata);
					}
					catch{}
				}
			}
			FillListBox();
			
			numericUpDown1.Value = Math.Min(15000, Math.Max(1, Convert.ToDecimal(DrawingObject.ContourLabelDist)));
			numericUpDown2.Value = Math.Min(15, Math.Max (6, Convert.ToDecimal(DrawingObject.LabelInterval)));
			if (DrawingObject.Filter)
			{
				radioButton1.Checked = true;
			}
			else
			{
				radioButton2.Checked = true;
			}

            checkBox1.Checked = DrawingObject.ContourDrawBorder;
            checkBox2.Checked = DrawingObject.Fill;
            checkBox3.Checked = DrawingObject.FillYesNo;
		}
		
		void FillListBox()
		{
			listBox1.Items.Clear();
			foreach(WindFileData _wdata in Windfiles)
			{
				listBox1.Items.Add(Path.GetFileName(_wdata.Filename));
			}
		}
		
		// Add a Windrose
		void Button1Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dialog = new OpenFileDialog
			       {
			       	Filter = "Met files (*.met)|*.met|DWD (*.akterm; *.akt)|*.akterm;*.akt",
			       	Title = "Select meteorological data",
			       	InitialDirectory = Path.Combine(Main.ProjectName, "Mefiles"),
			       	Multiselect = true
			       })
			{
				double Anemometerheight = 0;
				double x0 = double.MaxValue;
				double y0 = 0;
				int filelength = 0;
				
				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					foreach (string metfile in dialog.FileNames)
					{
						MeteoTimeSeries.Clear();
						MeteoTimeSeries.TrimExcess();
						
						string metfileext = Path.GetExtension(metfile).ToLower();
						try
						{
							using (StreamReader streamreader = new StreamReader(metfile))
							{
								string reihe;

								//Evaluating the length of the file
								if (Path.GetExtension(metfile).ToLower() == ".met")
								{
									int ret;
									while (streamreader.EndOfStream == false)
									{
										reihe = streamreader.ReadLine();
										if (reihe.ToUpper().Contains("X="))
										{
											x0 = EvalMetFileHeader(reihe);
										}
										if (reihe.ToUpper().Contains("Y="))
										{
											y0 = EvalMetFileHeader(reihe);
										}
										if (reihe.ToUpper().Contains("Z="))
										{
											Anemometerheight = EvalMetFileHeader(reihe);
										}
										if (Int32.TryParse(reihe.Substring(0, 1), out ret) == true)
											filelength = filelength + 1;
									}
								}
								else if (Path.GetExtension(metfile).ToLower() == ".akterm")
								{
									string[] text = new string[50];

									while (streamreader.EndOfStream == false)
									{
										reihe = streamreader.ReadLine();
										text = reihe.Split(new char[] { ' ', '\t' });
										if (text[0] == "AK")
										{
											text = reihe.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
											if ((text[7] != "9") && (text[8] != "9") && (text[12] != "7") && (text[12] != "9"))
												filelength = filelength + 1;
										}
										if (text[0] == "+")
										{
											try
											{
												text = reihe.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
												Anemometerheight = Convert.ToDouble(text[10]) / 10;
											}
											catch { }
										}
									}
								}
								else if (Path.GetExtension(metfile).ToLower() == ".akt")
								{
									while (streamreader.EndOfStream == false)
									{
										reihe = streamreader.ReadLine();
										filelength = filelength + 1;
									}
									//at this stage it is not allowed to delete existing meteoinput-data
									//numericUpDown7.Value = 10; // Anemometer height
								}


								// no coordinate-Info available
								if (x0 > 1.6E300)
								{
									using (InputCoordinates inp = new InputCoordinates("0", "0"))
									{
										inp.TopMost = true;
										inp.ShowDialog();
										double.TryParse(inp.Input_x.Text, out x0);
										double.TryParse(inp.Input_y.Text, out y0);
									}
								}

								using (GralMainForms.MeteoInput FormatMetFile = new GralMainForms.MeteoInput(null)
								       {
								       	MetFile1 = metfile,
								       	FfileLength1 = filelength,
								       	DecSep1 = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator,
								       	Met_Time_Ser = MeteoTimeSeries,
								       	Owner = this
								       })
								{
									FormatMetFile.ShowDialog();

									WindFileData _wdata = new WindFileData
									{
										Filename = metfile,
										DecSep = FormatMetFile.DecSepUser,
										RowSep = FormatMetFile.RowSep,
										X0 = x0,
										Y0 = y0,
										Z0 = Anemometerheight
									};
									Windfiles.Add(_wdata);

									listBox1.Items.Add(Path.GetFileName(metfile));
								}
								
							}
							
						}
						catch{}
					}
				}
			}
			MeteoTimeSeries.Clear();
			MeteoTimeSeries.TrimExcess();
		}
		
		private double EvalMetFileHeader(string s)
		{
			string[] _st = s.Split(new char[] {'='});
			double x = double.MaxValue;
			if (s.Contains("=") && _st.Length > 0)
			{
				try
				{
					x = Convert.ToDouble(_st[1], ic);
				}
				catch
				{}
			}
			return x;
		}
		
		// Remove selected item
		void Button2Click(object sender, EventArgs e)
		{
            if (listBox1.SelectedIndex >= 0)
			{
                // Remove each item in reverse order to maintain integrity
                var selectedIndices = new List<int>(listBox1.SelectedIndices.Cast<int>());
                selectedIndices.Reverse();
                selectedIndices.ForEach(index => Windfiles.RemoveAt(index));
				FillListBox();
			}
		}
		
		// cancel
		void Button3Click(object sender, EventArgs e)
		{
			return;
		}
		
		//OK
		void Button4Click(object sender, EventArgs e)
		{
			DrawingObject.ContourLabelDist = (int) numericUpDown1.Value;
			DrawingObject.LabelInterval = (int) numericUpDown2.Value;
            DrawingObject.ContourDrawBorder = checkBox1.Checked;
            DrawingObject.Fill = checkBox2.Checked;
            DrawingObject.FillYesNo = checkBox3.Checked;

            if (DrawingObject.LabelFont != null)
			{
				DrawingObject.LabelFont.Dispose();
			}
			if (DrawingObject.ScaleLabelFont)
			{
				DrawingObject.LabelFont = new Font("Arial", (int) (DrawingObject.ContourLabelDist / 20));
			}
			DrawingObject.ContourFilename = string.Empty;
			
			if (radioButton1.Checked) // velocity
			{
				DrawingObject.Filter = true;
			}
			else // stability
			{
				DrawingObject.Filter = false;
			}
			
			if (Windfiles.Count > 0)
			{
				foreach(WindFileData _wdata in Windfiles)
				{
					if (DrawingObject.ContourFilename.Length > 0)
					{
						DrawingObject.ContourFilename += "\t";
					}
					
					string txt = _wdata.Filename + "?" +_wdata.DecSep + "?" + Convert.ToString(_wdata.RowSep) + "?"
						+ _wdata.X0.ToString(ic) + "?" + _wdata.Y0.ToString(ic) + "?" + _wdata.Z0.ToString(ic);
					
					DrawingObject.ContourFilename += txt;
				}
			}
		}
		
		// remove a item via delete key
		void ListBox1KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				Button2Click(null, null);
			}
		}
		
		// select all listbox entries with control + A
		void ListBox1KeyUp(object sender, KeyEventArgs e)
		{
		    if (e.Control && e.KeyCode == Keys.A)
		    {
		        listBox1.BeginUpdate();
		        for (var i = 0; i < listBox1.Items.Count; i++)
		            listBox1.SetSelected(i, true);
		        listBox1.EndUpdate();
		    }
		    
		}
	}
}
