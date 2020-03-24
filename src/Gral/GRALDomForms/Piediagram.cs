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
using System.IO;

namespace GralDomForms
{
	/// <summary>
    /// Show a pie diagram
    /// </summary>
	public partial class Piediagram : Form
	{
		public FileInfo[] FilesConc;
		public double[] Concentration;
		private int xdomain = 0;
		private int ydomain = 0;

		public Piediagram(int _xdomain, int _ydomain)
		{
			xdomain = _xdomain;
			ydomain = _ydomain;
			InitializeComponent();
		}

		private void Piediagram_Load(object sender, EventArgs e)
		{
			PiediagramSizeChanged(null, null);
		}

		
		void PictureBox1Paint(object sender, PaintEventArgs e)
		{

            StringFormat format1 = new StringFormat
            {
                LineAlignment = StringAlignment.Near,
                Alignment = StringAlignment.Center
            };
            Font smallFont = new Font("Arial", 8);
			Font midFont = new Font("Arial", 10);
			Brush blackbrush = new SolidBrush(Color.Black);
			Pen black = new Pen(Color.Black);
			
			try
			{
				Color[] aColors = GetColorArray();

				Graphics g = e.Graphics;
				
				
				
				//compute total concentration
				double totalcon = Concentration[FilesConc.Length+1];
				string[] snumb = new string[5];
				for (int i = 0; i < FilesConc.Length; i++)
				{
					//extract names of the source groups
					snumb = FilesConc[i].Name.Split(new char[] { '_' });
					if (Concentration[i] > 0 && snumb.GetUpperBound(0) > 1)
					{
						if(snumb[snumb.GetUpperBound(0) - 1]!="total")
						{
							//MessageBox.Show(snumb[snumb.GetUpperBound(0) - 1]);
							totalcon = totalcon + Concentration[i];
						}
					}
				}

				//draw diagram
				int round = 1;
				if (totalcon < 1)
					round = 2;
				if (totalcon < 0.1)
					round = 3;
				if (totalcon < 0.01)
					round = 4;
				if (totalcon < 0.001)
					round = 5;
				
				base.OnPaint(e);
				int angle;
				int anglesum=0;
				int zahl = 0;
				int _x = (int) Math.Max(150, pictureBox1.Width * 0.9);
				int _y = Math.Max(20, Math.Min(_x-150, pictureBox1.Height - 80));
				_x = Math.Min(_x, _y + 180);
				
				if (snumb.GetUpperBound(0) > 2)
				{
					g.DrawString("Total conc. of "+snumb[snumb.GetUpperBound(0) - 2]+": "+Convert.ToString(Math.Round(totalcon, round)) + " [" + Gral.Main.My_p_m3 + "] at "
					             + snumb[snumb.GetUpperBound(0)].Replace(".txt","")+" above ground level  " + 
					             "X:" + xdomain.ToString() + " Y:" + ydomain.ToString()
					             , midFont, blackbrush, new Rectangle(1,1, _x, 36), format1);
				}
				
				for (int i = 0; i < FilesConc.Length; i++)
				{
					//extract names of the source groups
					snumb = FilesConc[i].Name.Split(new char[] { '_' });
					if (Concentration[i]  > 0 || checkBox1.Checked == true)
					{
						if (snumb.GetUpperBound(0) > 0 && snumb[snumb.GetUpperBound(0) - 1] != "total")
						{
							//draw pie chart
							angle = Convert.ToInt32(360 * Concentration[i] / totalcon);
							g.FillPie(new SolidBrush(aColors[i % 20]), new Rectangle(10, 35, _y-10, _y-10), anglesum, angle + 0.2F);
							g.DrawPie(black, new Rectangle(10, 35, _y-10, _y-10), anglesum, angle);
							anglesum = anglesum + angle;
							//draw legend
							g.FillRectangle(new SolidBrush(aColors[i % 20]), new Rectangle(_x - 160, 40 + zahl * 20, 20, 20));
							g.DrawRectangle(black, new Rectangle(_x - 160, 40 + zahl * 20, 20, 20));
							
							if (snumb.GetUpperBound(0) > 2)
							{
								g.DrawString(Math.Round(Concentration[i] / totalcon * 100, 1).ToString("0.0").PadLeft(5) + "% : " + snumb[snumb.GetUpperBound(0) - 1], smallFont, blackbrush, _x - 135, 42 + zahl * 20);
							}
							zahl = zahl + 1;
						}
					}
				}
				
				//draw background
				if (Concentration[FilesConc.Length + 1] > 0)
				{
					//draw pie chart
					angle = Convert.ToInt32(360 * Concentration[FilesConc.Length + 1] / totalcon);
					g.FillPie(new SolidBrush(aColors[FilesConc.Length % 20]), new Rectangle(10, 25, _y-10, _y-10), anglesum, angle);
					//g.FillPie(new SolidBrush(aColors[files_conc.Length]), new Rectangle(10, 20, 300, 300), anglesum, angle);
					anglesum = anglesum + angle;
					
					//draw legend
					g.FillRectangle(new SolidBrush(aColors[FilesConc.Length % 20]), new Rectangle(_x - 160, 30 + zahl * 20, 20, 20));
					g.DrawRectangle(black, new Rectangle(_x - 160, 30 + zahl * 20, 20, 20));
					//g.FillRectangle(new SolidBrush(aColors[files_conc.Length]), new Rectangle(330, 30 + zahl * 20, 20, 20));
					//g.DrawRectangle(black, new Rectangle(330, 30 + zahl * 20, 20, 20));
					g.DrawString("Background" + ": " + Convert.ToString(Math.Round(Concentration[FilesConc.Length+1] / totalcon * 100, 1)) + "%", smallFont, blackbrush, _x - 135, 32 + zahl * 20);
				}
				
				
			}
			catch
			{}
			format1.Dispose();
			smallFont.Dispose();
			midFont.Dispose();
			blackbrush.Dispose();
			black.Dispose();
		}
		
		//save imapge to clipboard
		private void button22_Click(object sender, EventArgs e)
		{
			Bitmap bitMap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
			pictureBox1.DrawToBitmap(bitMap, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
			Clipboard.SetDataObject(bitMap);
		}

		Color[] GetColorArray()
		{

			// declare an Array for 20 colors
			Color[] aColors = new Color[20];

			// fill the array of colors for chart items
			// use browser-safe colors (multiples of #33)
			aColors[0] = Color.FromArgb(204, 0, 0);      // red
			aColors[1] = Color.FromArgb(255, 153, 0);    // orange
			aColors[2] = Color.FromArgb(255, 255, 0);    // yellow
			aColors[3] = Color.FromArgb(0, 255, 0);      // green
			aColors[4] = Color.FromArgb(0, 255, 255);    // cyan
			aColors[5] = Color.FromArgb(51, 102, 255);   // blue
			aColors[6] = Color.FromArgb(255, 0, 255);    // magenta
			aColors[7] = Color.FromArgb(102, 0, 102);    // purple
			aColors[8] = Color.FromArgb(153, 0, 0);      // dark red
			aColors[9] = Color.FromArgb(153, 153, 0);    // khaki
			aColors[10] = Color.FromArgb(0, 102, 0);     // dark green
			aColors[11] = Color.FromArgb(51, 51, 102);   // dark blue
			aColors[12] = Color.FromArgb(102, 51, 0);    // brown
			aColors[13] = Color.FromArgb(204, 204, 204); // light gray
			aColors[14] = Color.FromArgb(0, 0, 0);       // black
			aColors[15] = Color.FromArgb(102, 204, 255); // sky
			aColors[16] = Color.FromArgb(255, 204, 255); // pink
			aColors[17] = Color.FromArgb(255, 255, 204); // chiffon
			aColors[18] = Color.FromArgb(255, 204, 204); // flesh
			aColors[19] = Color.FromArgb(153, 255, 204); // pale green

			return aColors;

		}
		
		void PiediagramSizeChanged(object sender, EventArgs e)
		{
			pictureBox1.Width = Width;
			pictureBox1.Height = Height;
			pictureBox1.Refresh();
		}
        
        void CheckBox1CheckedChanged(object sender, EventArgs e)
        {
        	pictureBox1.Refresh();
        }
	}
}