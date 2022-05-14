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
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Drawing.Drawing2D;

namespace GralDomForms
{
    /// <summary>
    /// Show vertical profiles
    /// </summary>
    public partial class VerticalProfileConcentration : Form
	{
		public double X;
		public double Y;
		public string filename;
		private List<double> height = new List<double>();
		private List<double> concentration = new List<double>();
		private string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
		private int zlevels_userdefined = 0; //user defined zlevels to be drawn
		private int zlevels_userdefined_old = 0; //user defined zlevels to be drawn
		private GralDomain.Domain domain = null;
		
		public VerticalProfileConcentration(GralDomain.Domain f)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			domain = f;
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void VerticalProfileConcentrationLoad(object sender, EventArgs e)
		{
			
		}
		
		public void Init()
		{
			// try to load the File
			zlevels_userdefined = 0;
			height.Clear();
			concentration.Clear();
			
			if (File.Exists(filename))
			{
				try
				{
					using (StreamReader rf = new StreamReader(filename))
					{
						string[] txt = new string[2];
						char sep = '\t';
						
						txt = rf.ReadLine().Split(sep);
						double west = Convert.ToDouble(txt[0].Replace(".",decsep));
						
						rf.ReadLine(); // east
						
						txt = rf.ReadLine().Split(sep);
						double south = Convert.ToDouble(txt[0].Replace(".",decsep));
						
						txt = rf.ReadLine().Split(sep);
						double north = Convert.ToDouble(txt[0].Replace(".",decsep));
						
						txt = rf.ReadLine().Split(sep);
						int NII = Convert.ToInt32(txt[0].Replace(".",decsep));
						
						txt = rf.ReadLine().Split(sep);
						int NJJ = Convert.ToInt32(txt[0].Replace(".",decsep));
						
						txt = rf.ReadLine().Split(sep);
						int NKK = Convert.ToInt32(txt[0].Replace(".",decsep));
						
						txt = rf.ReadLine().Split(sep);
						double deltax = Convert.ToDouble(txt[0].Replace(".",decsep));

						txt = rf.ReadLine().Split(sep);
						double model_base_height = Convert.ToDouble(txt[0].Replace(".",decsep));
						
						txt = rf.ReadLine().Split(sep); // height values
						for (int i = 0; i < txt.Length - 1; i++) // last entry = description
						{
							double val = Convert.ToDouble(txt[i].Replace(".",decsep));
							height.Add(val);
						}
						
						//MessageBox.Show(height.Count.ToString());
						
						// calculate index here!!
						int xi = (int) ((X - west) / deltax);
						int yi = (int) ((north - Y) / deltax);
						
						//MessageBox.Show(xi.ToString() + "/" + yi.ToString());
						CultureInfo ic = CultureInfo.InvariantCulture;
						
						if (xi < NII && yi < NJJ && xi > 0 && yi > 0) // index inside model
						{
							rf.ReadLine(); 
							for (int h = 0; h < height.Count; h++) // loop over height
							{
								rf.ReadLine(); // lines between concentration blocks
								
								for (int j = 1; j < NJJ + 1;  j++)
								{
									string line = rf.ReadLine(); // read entire line
									if (j == yi) // if index has been found
									{
										//line.Replace(".",decsep);
										txt = line.Split(sep);
										if (xi < txt.Length -1)
										{
											double _val = Convert.ToDouble(txt[xi], ic); // read value
											concentration.Add(_val);
											//MessageBox.Show(txt[xi] + "/" + _val.ToString());
										}
									}
								}
							}
						}
						
					}
				}
				catch(Exception ex)
				{MessageBox.Show(ex.Message);}
				
				if (zlevels_userdefined_old > 0 && zlevels_userdefined_old < concentration.Count)
				{
					zlevels_userdefined = zlevels_userdefined_old;
				}
				else
				{
					zlevels_userdefined = concentration.Count;
				}
				pictureBox1.Refresh();
			} // File exists
		}
	
		void PictureBox1Paint(object sender, PaintEventArgs e)
		{
			if (height.Count < 2)
            {
                return;
            }

            if (concentration.Count < 2)
            {
                return;
            }

            if (zlevels_userdefined < 1)
            {
                return;
            }

            Graphics g = e.Graphics;
			
			int wi = Math.Max(0, Width - 40);
			int he = Math.Max(0, Height - 20);
			int leftbound = Convert.ToInt32(wi / 7);
			int rightbound = wi - Convert.ToInt32(wi / 20);
			int bottombound = he - Convert.ToInt32(he / 7);
			int topbound = Convert.ToInt32(he / 20);
			double horscale = 0;
			double vertscale = 0;

			Font font = new Font("Arial", Convert.ToInt32(Math.Max(he / 50, 1)));
			Pen p1 = new Pen(Color.Red, 3);
            Pen p2 = new Pen(Color.Black, 2)
            {
                EndCap = LineCap.ArrowAnchor
            };
            Brush black_brush = new SolidBrush(Color.Black);

			StringFormat format1 = new StringFormat();
			StringFormat format2 = new StringFormat();
		
			format1.LineAlignment = StringAlignment.Center;
			format1.Alignment = StringAlignment.Center;
			format2.Alignment = StringAlignment.Far;
		

			//draw axis
			g.DrawLine(p2, leftbound, bottombound, rightbound, bottombound);
			g.DrawLine(p2, leftbound, bottombound, leftbound, topbound);

			//compute minimum/maximum
			double min = 10000000;
			double max = -10000000;
			for (int i = 0; i < zlevels_userdefined; i++)
			{
				min = Math.Min(min, concentration[i]);
				max = Math.Max(max, concentration[i]);
			}

			
			if (zlevels_userdefined > 0)
			{
				//horizontal/vertical scales
				if (max - min > 0)
                {
                    horscale = (rightbound - leftbound) / (max - min);
                }
                else
                {
                    horscale = 1;
                }

                double vert_diff = height[zlevels_userdefined - 1] - height[0];
				if (vert_diff > 0)
                {
                    vertscale = (bottombound - topbound) / vert_diff;
                }

                int x1 = Convert.ToInt32(leftbound-2);
				int y1 = Convert.ToInt32(bottombound+2);
				
				//g.DrawString(Convert.ToString(Math.Round(min,0)), font, black_brush, x1, y1 - font.Size*2, format2);
				//g.DrawString(Convert.ToString(zlevels_userdefined), font, black_brush, x1, y1 - font.Size * 2 - (bottombound - topbound), format2);
				//g.DrawString("<" + Convert.ToString(Math.Round(min, 2)) + ">", font, black_brush, x1 - font.Size * 2, y1 + 2 * font.Size, format1);
				//g.DrawString("<" + Convert.ToString(Math.Round(max, 2)) + ">", font, black_brush, x1 + (rightbound - leftbound) - font.Size * 2, y1 + 2 * font.Size, format1);
				g.DrawString("µg/m³", font, black_brush, x1 + (rightbound - leftbound) / 2 , y1 + 2 * font.Size, format1);
        	
				
				
				int y_step = 1000;
				if (height[zlevels_userdefined - 1] < 4000)
                {
                    y_step = 500;
                }

                if (height[zlevels_userdefined - 1]< 2000)
                {
                    y_step = 250;
                }

                if (height[zlevels_userdefined - 1] < 600)
                {
                    y_step = 100;
                }

                if (height[zlevels_userdefined - 1] < 250)
                {
                    y_step = 50;
                }

                if (height[zlevels_userdefined - 1] < 120)
                {
                    y_step = 20;
                }

                if (height[zlevels_userdefined - 1] < 40)
                {
                    y_step = 10;
                }

                if (height[zlevels_userdefined - 1] < 20)
                {
                    y_step = 5;
                }

                double x_step = (max - min) / 4;
				int exp = 0;
				if (x_step != 0.0)
                {
                    exp = (int) Math.Floor(Math.Log10(x_step)) * (-1);
                }

                double norm = x_step * Math.Pow(10, exp);
				if (norm < 1.5)
                {
                    norm = 1;
                }
                else if (norm < 3.4)
                {
                    norm  = 2;
                }
                else if (norm < 6.5)
                {
                    norm = 5;
                }
                else if (norm < 9)
                {
                    norm = 8;
                }
                else
                {
                    norm = 10;
                }

                exp *= -1;
				x_step = norm * Math.Pow(10, exp);

                Pen p3 = new Pen(Color.Black, 1)
                {
                    DashStyle = DashStyle.Dash
                };


                for (int i = 1; i < 10; i++)
				{
					int y = (int) (bottombound - y_step * i * vertscale);
					
					if (y > (y1 - font.Size * 2 - (bottombound - topbound)+10))
					{
						g.DrawLine(p3, leftbound, y, rightbound, y);
						g.DrawString(Convert.ToString(y_step * i), font, black_brush, x1, y, format2);
					}
				}
				
				int start = (int) (min / x_step);
				for (int i = start; i < start + 10; i++)
				{
					int x = (int) (leftbound + (x_step * i - min) * horscale);
					
					if (x > x1 && x < rightbound)
					{
						g.DrawLine(p3, x, bottombound, x, topbound);
						g.DrawString(Convert.ToString(x_step * i), font, black_brush, x, y1 + font.Size, format1);
					}
				}
				p3.Dispose();
				
			}
			//draw profile
			for (int i = 0; i < concentration.Count - 1; i++)
			{
				int _x1=Convert.ToInt32(leftbound + (concentration[i] - min) * horscale);
				int _y1 = Convert.ToInt32(bottombound - height[i] * vertscale);
				int _x2 = Convert.ToInt32(leftbound + (concentration[i + 1] - min) * horscale);
				int _y2 = Convert.ToInt32(bottombound - height[i + 1] * vertscale);
				g.DrawLine(p1, _x1, _y1, _x2, _y2);
			}

			black_brush.Dispose();
			p1.Dispose();
			p2.Dispose();
			font.Dispose();
			format1.Dispose();
			format2.Dispose();
			//g.Dispose();
		}
		
		void VerticalProfileConcentrationResizeEnd(object sender, EventArgs e)
		{
			pictureBox1.Refresh();
		}
		
		void Button6Click(object sender, EventArgs e)
		{
			zlevels_userdefined = Math.Max(8, zlevels_userdefined - 1);
			zlevels_userdefined_old = zlevels_userdefined;
			pictureBox1.Refresh();
		}
		
		void Button5Click(object sender, EventArgs e)
		{
			zlevels_userdefined = Math.Min(concentration.Count, zlevels_userdefined + 1);
			zlevels_userdefined_old = zlevels_userdefined;
			
			pictureBox1.Refresh();
		}
		
		void VerticalProfileConcentrationFormClosed(object sender, FormClosedEventArgs e)
		{
			domain.VerticalProfileForm = null; // Kuntner release objectmanager!
			pictureBox1.Dispose();
			height.Clear();
			concentration.Clear();
		}
		
		void Button7Click(object sender, EventArgs e)
		{
			
			Bitmap bitMap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
			pictureBox1.DrawToBitmap(bitMap, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
			Clipboard.SetDataObject(bitMap);
		}
		
		void VerticalProfileConcentrationKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Add)
			{
				Button6Click(null, null);
			}
			if (e.KeyCode == Keys.Subtract)
			{
				Button5Click(null, null);
			}
			
		}
	}
}
