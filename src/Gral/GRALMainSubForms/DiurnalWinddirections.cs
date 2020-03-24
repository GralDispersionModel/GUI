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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GralMainForms
{
	/// <summary>
    /// Form shows the diurnal wind directions
    /// </summary>
    public partial class DiurnalWinddirections : Form
    {
        public double[,] meanwinddir = new double[24,16];
        public string metfile;
		public List <GralData.WindData> wind;

        public int sector;
        public double scale;
        public int hoehe2;
        public double classmax;
        public int[] check = new int[16];

        public DiurnalWinddirections()
        {
            InitializeComponent();
        }

        private void WindDirectionsFormLoad(object sender, EventArgs e)
        {
        	Text = "Diurnal wind direction frequencies   -  " + metfile; // Kuntner
            //scaling factor
            classmax = 0;
            for (int i = 0; i < 24; i++)
            {
                for (int n = 0; n < 16; n++)
                {
                    if (classmax < meanwinddir[i, n])
                    {
                        classmax = meanwinddir[i, n];
                        sector = n;
                    }
                }
            }

            scale = 400 / classmax;

            //check the sector in the list box
            checkedListBox1.SelectedIndex = sector;
            checkedListBox1.SetItemCheckState(sector, CheckState.Checked);
            check[sector] = 1;
        }

        protected override void OnPaint(PaintEventArgs e)
        {            
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            //draw diagram
            StringFormat format1 = new StringFormat
            {
                LineAlignment = StringAlignment.Near,
                Alignment = StringAlignment.Near
            };

            Font _smallFont = new Font("Arial", 8);
            Font _mediumFont = new Font("Arial", 9);
            Font _largeFont = new Font("Arial", 10);
            Brush _blackBrush = new SolidBrush(Color.Black);
            Pen p4 = new Pen(Color.Black, 3);
			
            string[] wrnamen ={ "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };
            
             SizeF _lenght = g.MeasureString(metfile, _smallFont);
            int distance = (int) _lenght.Height + 2;
            
            float _x = Math.Max(0, panel1.Width - _lenght.Width - 5);
            g.DrawString(metfile, _smallFont, _blackBrush, _x, 2, format1);
            
            string _data = "Data points: " + Convert.ToString(wind.Count);
            _lenght = g.MeasureString(_data, _smallFont);
            _x = Math.Max(0, panel1.Width - _lenght.Width - 5);
            g.DrawString(_data, _smallFont, _blackBrush, _x, 2 + distance, format1);
            
            if (wind.Count > 1)
			{
			    _data = wind[0].Date + " - " + wind[wind.Count - 1].Date;
			   _lenght = g.MeasureString(_data, _smallFont);
			    _x = Math.Max(0, panel1.Width - _lenght.Width - 5);
			    g.DrawString(_data, _smallFont, _blackBrush, _x, 2 + 2 * distance, format1);
			}
            
			float _scale = Math.Min(panel1.Width / 852F, panel1.Height / 495F);
			g.ScaleTransform(_scale, _scale);

            // draw all checked wind directions
            Color[] aColors = GetColorArray();
            int j = 0;
            
            for (int n = 0; n < 16; n++)
            {
                if (check[n] == 1)
                {
                    p4.Color = aColors[n];
                    p4.EndCap = LineCap.RoundAnchor;
                    p4.StartCap = LineCap.RoundAnchor;
                    g.DrawLine(p4, 700, 50 + Convert.ToInt32(j * 15.25), 720, 50 + Convert.ToInt32(j * 15.25));
                    g.DrawString(wrnamen[n], _smallFont, _blackBrush, 725, 43 + Convert.ToInt32(j * 15.25));
                    j = j + 1;
                    for (int i = 0; i < 24; i++)
                    {
                        int hoehe = Convert.ToInt32(meanwinddir[i, n] * scale);
                        int ecke1 = Convert.ToInt32(55 + i * 25);
                        if (i > 0) g.DrawLine(p4, 55 + 25 * (i + 1), 440 - hoehe, 55 + 25 * (i), 440 - hoehe2);
                        hoehe2 = hoehe;
                       // base.OnPaint(e);
                    }
                }
            }

            //draw axis
            Pen p1 = new Pen(Color.Black, 2);
            Pen p2 = new Pen(Color.Black, 2);
            Pen p3 = new Pen(Color.Black, 1);
            
            
            p2.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(p1, 55, 440, 695, 440);
            g.DrawLine(p2, 55, 440, 55, 20);
            
            StringFormat string_Format = new StringFormat()
            {
            	Alignment = StringAlignment.Center //Horizontale Orientieren
            };
            
            for (int i = 0; i < 24; i+= 3)
            {
            	string a = i.ToString("D2") + ":00";
            	_x = 55 + (i + 1) * 25;
            	g.DrawString(a, _mediumFont, _blackBrush, _x, 450, string_Format);
            	g.DrawLine(p3, _x, 444, _x, 436);
            }
            string_Format.Dispose();
            
            p3.DashStyle = DashStyle.Dot;

            g.DrawString("  0", _largeFont, _blackBrush, 25, 435);
            //base.OnPaint(e);

            //draw frequency levels
            int off = 1;
            if (classmax > 0.5)
            	off = 2;
            
            for (int i = 1; i < 20; i += off)
            {
                int levels = Convert.ToInt32(Convert.ToDouble(i) / 20 * scale);
                int lev1 = Convert.ToInt32(440 - levels);
                string s = (i * 5).ToString();
                if (i * 5 > Convert.ToInt32(classmax * 100))
                {
                    break;
                }
                g.DrawLine(p3, 55, lev1, 695, lev1);
                g.DrawString(s, _largeFont, _blackBrush, 25, lev1 - 5);
                //base.OnPaint(e);
            }
            
            p1.Dispose();p2.Dispose();p3.Dispose(); p4.Dispose();
            format1.Dispose();
            _smallFont.Dispose();
            _mediumFont.Dispose();
            _largeFont.Dispose();
            _blackBrush.Dispose();
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i=0;i<16;i++)
                check[i] =0;

            foreach (int indexChecked in checkedListBox1.CheckedIndices)
            {
                check[indexChecked] = 1;
            }
            Refresh();

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

        //save image to clipboard
        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap bitMap = new Bitmap(panel1.Width, panel1.Height);
            panel1.DrawToBitmap(bitMap, new Rectangle(0, 0, panel1.Width * 2, panel1.Height * 2));
            Clipboard.SetDataObject(bitMap);
        }
		void DiurnalWinddirectionsFormClosed(object sender, FormClosedEventArgs e)
		{
			toolTip1.Dispose();
			panel1.Dispose();
			button1.Dispose();
		}
        
        void DiurnalWinddirectionsResizeEnd(object sender, EventArgs e)
        {
            if (ClientSize.Width > 60)
            {
                panel1.Width = ClientSize.Width - 55;
                panel1.Height = ClientSize.Height;
                checkedListBox1.Left = panel1.Width - checkedListBox1.Width;
            }
            
            panel1.Invalidate();
			panel1.Update();
        }
        
        
        void DiurnalWinddirectionsResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized) 
            {
                DiurnalWinddirectionsResizeEnd(null, null);
                // Maximized!
            }
            if (WindowState == FormWindowState.Normal) 
            {
                DiurnalWinddirectionsResizeEnd(null, null);
                // Restored!
            }
        }
    }
}