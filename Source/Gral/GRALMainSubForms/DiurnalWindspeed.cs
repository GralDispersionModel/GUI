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
    /// Form shows the diurnal wind speed
    /// </summary>
    public partial class DiurnalWindspeed : Form
    {
        public double[] meanwind = new double[24];
        public string metfile;
		public List <GralData.WindData> wind;
        private double _mean_wind_speed = 0;

        public DiurnalWindspeed()
        {
            InitializeComponent();         
        }
        
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
                        
            //scaling factor
            double classmax = 0;
            for (int i = 0; i < 24; i++)
            {
                classmax = Math.Max(meanwind[i], classmax);
            }
            
            double scale = Math.Min(400 / classmax, 1000000);

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
            
            SizeF _lenght = g.MeasureString(metfile, _smallFont);
            int distance = (int) _lenght.Height + 2;
            
            float _x = Math.Max(0, panel1.Width - _lenght.Width - 5);
            g.DrawString(metfile, _smallFont, _blackBrush, _x, 2, format1);
            
            string _data = "Data points: " + Convert.ToString(wind.Count);
            _lenght = g.MeasureString(_data, _smallFont);
            _x = Math.Max(0, panel1.Width - _lenght.Width - 5);
            g.DrawString(_data, _smallFont, _blackBrush, _x, 2 + distance, format1);
			
			//g.DrawString(wind[0].Date, _smallFont, _blackBrush, 675, 2 + 2 * distance, format1);
			if (wind.Count > 1)
			{
			    _data = wind[0].Date + " - " + wind[wind.Count - 1].Date;
			   _lenght = g.MeasureString(_data, _smallFont);
			    _x = Math.Max(0, panel1.Width - _lenght.Width - 5);
			    g.DrawString(_data, _smallFont, _blackBrush, _x, 2 + 2 * distance, format1);
			}
			
			_data = "Mean wind speed " + Math.Round(_mean_wind_speed, 1).ToString() + " m/s";
			_lenght = g.MeasureString(_data, _smallFont);
			_x = Math.Max(0, panel1.Width - _lenght.Width - 5);
			g.DrawString(_data, _smallFont, _blackBrush, _x, 4 + 3 * distance, format1);
			
     		float _scale = Math.Min(panel1.Width / 719F, panel1.Height / 475F);
			g.ScaleTransform(_scale, _scale);

			int hoehe2 = 0;
            Pen p4 = new Pen(Color.Blue, 3)
            {
                EndCap = LineCap.RoundAnchor,
                StartCap = LineCap.RoundAnchor
            };

            for (int i = 0; i < 24; i++)
            {
                int hoehe = Convert.ToInt32(meanwind[i] * scale);
                int ecke1 = Convert.ToInt32(55 + i * 25);
                if (hoehe2 > 0) g.DrawLine(p4, 55 + 25 * (i + 1), 440 - hoehe, 55 + 25 * (i), 440 - hoehe2);
                hoehe2 = hoehe;
                base.OnPaint(e);
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
            
            g.DrawString("Mean wind speed [m/s]", _mediumFont, _blackBrush, 12, 5);
            g.DrawString("  0", _largeFont, _blackBrush, 25, 435);
            base.OnPaint(e);

            //draw frequency levels
            double step;

            for (int i = 1; i < 20; i++)
            {
                if (classmax > 4)
                    step = 1;
                else
                    step = 0.5;

                int levels = Convert.ToInt32(Convert.ToDouble(i) * step * scale);
                int lev1 = Convert.ToInt32(440 - levels);
                string s = (i * step).ToString();
                if (i * step > Convert.ToInt32(classmax))
                {
                    break;
                }
                g.DrawLine(p3, 55, lev1, 695, lev1);
                g.DrawString(s, _largeFont, _blackBrush, 25, lev1 - 5);
                base.OnPaint(e);
            }
            
            p1.Dispose();p2.Dispose();p3.Dispose();p4.Dispose();
            format1.Dispose();
            _smallFont.Dispose();
            _mediumFont.Dispose();
            _largeFont.Dispose();
            _blackBrush.Dispose();
        }

        private void Form6_Load(object sender, EventArgs e)
        {
        	_mean_wind_speed = 0;
			if (meanwind.Length > 0)
			{
			    for (int i = 0; i < meanwind.Length; i++)
			    {
			        _mean_wind_speed += meanwind[i];
			    }
			    _mean_wind_speed /= meanwind.Length;
			}
			
         	Text = "Diurnal mean wind speed   -  " + metfile; // Kuntner
        }

        //save image to clipboard
        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap bitMap = new Bitmap(panel1.Width, panel1.Height);
            panel1.DrawToBitmap(bitMap, new Rectangle(0, 0, panel1.Width * 2, panel1.Height * 2));
            Clipboard.SetDataObject(bitMap);
        }
		void DiurnalWindspeedFormClosed(object sender, FormClosedEventArgs e)
		{
			toolTip1.Dispose();
			panel1.Dispose();
			button1.Dispose();
		}
        
        void DiurnalWindspeedResizeEnd(object sender, EventArgs e)
        {
            if (ClientSize.Width > 60)
            {
                panel1.Width = ClientSize.Width - 55;
                panel1.Height = ClientSize.Height;
            }
            
            panel1.Invalidate();
			panel1.Update();
        }
        
        void DiurnalWindspeedResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized) 
            {
                DiurnalWindspeedResizeEnd(null, null);
                // Maximized!
            }
            if (WindowState == FormWindowState.Normal) 
            {
                DiurnalWindspeedResizeEnd(null, null);
                // Restored!
            }
        }
    }
}