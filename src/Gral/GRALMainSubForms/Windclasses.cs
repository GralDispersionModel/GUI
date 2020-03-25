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
    /// Form to show the wind speed classes
    /// </summary>
    public partial class Windclasses : Form
    {
        public double[] WClassFrequency;
        public string MetFile;
		public List <GralData.WindData> Wind;
        private float dpi;
        public int StartHour;
        public int FinalHour;

        public Windclasses()
        {
            InitializeComponent();
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {            
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            StringFormat format1 = new StringFormat
            {
                LineAlignment = StringAlignment.Near,
                Alignment = StringAlignment.Near
            };
            Font _smallFont = new Font("Arial", 8);
            Font _mediumFont = new Font("Arial", 9);
            Font _largeFont = new Font("Arial", 10);
            Brush _blackBrush = new SolidBrush(Color.Black);
            Brush _greyBrush = new SolidBrush(Color.Gray);
            Pen p1 = new Pen(Color.Black, 3);
            Pen p2 = new Pen(Color.Black, 3);
            Pen p3 = new Pen(Color.Black, 1);
            
             SizeF _lenght = g.MeasureString(MetFile, _smallFont);
            int distance = (int) _lenght.Height + 2;
            
            float _x = Math.Max(0, panel1.Width - _lenght.Width - 5);
            g.DrawString(MetFile, _smallFont, _blackBrush, _x, 2, format1);
            
            string _data = "Data points: " + Convert.ToString(Wind.Count);
            _lenght = g.MeasureString(_data, _smallFont);
            _x = Math.Max(0, panel1.Width - _lenght.Width - 5);
            g.DrawString(_data, _smallFont, _blackBrush, _x, 2 + distance, format1);
			
			//g.DrawString(wind[0].Date, _smallFont, _blackBrush, 675, 2 + 2 * distance, format1);
			if (Wind.Count > 1)
			{
			    _data = Wind[0].Date + " - " + Wind[Wind.Count - 1].Date;
			   _lenght = g.MeasureString(_data, _smallFont);
			    _x = Math.Max(0, panel1.Width - _lenght.Width - 5);
			    g.DrawString(_data, _smallFont, _blackBrush, _x, 2 + 2 * distance, format1);
			}
			
            _data = Convert.ToString(StartHour) + ":00 - " + Convert.ToString(FinalHour) +":00";
            _lenght = g.MeasureString(_data, _smallFont);
            _x = Math.Max(0, panel1.Width - _lenght.Width - 5);
            g.DrawString(_data, _smallFont, _blackBrush, _x, 2 + 3 * distance, format1);
            
            float _scale = Math.Min(panel1.Width / 762F, panel1.Height / 508F);
			g.ScaleTransform(_scale, _scale);
            
            //scaling factor
            double classmax = 0;
            int maxwind = WClassFrequency.Length - 1;
            for (int i = 0; i < (maxwind + 1); i++)
            {
                classmax = Math.Max(WClassFrequency[i], classmax);
            }
            double scale = 400 / classmax;

            //draw diagram
            for (int i = 0; i < (maxwind + 1); i++)
            {
                int hoehe = Convert.ToInt32(WClassFrequency[i] * scale);
                int ecke1 = Convert.ToInt32(55 + i * 64);
                g.FillRectangle(_greyBrush, ecke1, 440 - hoehe, 64, hoehe);
                g.DrawRectangle(p3, ecke1, 440 - hoehe, 64, hoehe);
                base.OnPaint(e);
            }

            //draw axis
           
            
            p3.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            p2.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(p1, 55, 440, Math.Max(759, 55 + (maxwind + 1) * 64), 440);
            g.DrawLine(p2, 55, 440, 55, 20);
            //g.DrawString("< 0.5 m/s", _smallFont,  _blackBrush, 65, 450);

            StringFormat stringFormat = new StringFormat
            {
                FormatFlags = StringFormatFlags.DirectionVertical
            };

            e.Graphics.DrawString("< 0.5 m/s", _smallFont,  _blackBrush, new PointF(65, 445), stringFormat);
            for (int i = 0; i < (maxwind-1); i++)
            {
            	string fs = Convert.ToString(i);
            	string ls = Convert.ToString(i+1);
            	if (i == 0)
                {
                    e.Graphics.DrawString(fs +".5-" + ls + " m/s", _smallFont,  _blackBrush, new PointF(132 + i *64, 445), stringFormat);
                }
                else
                {
                    e.Graphics.DrawString(fs +" - " + ls + " m/s", _smallFont,  _blackBrush, new PointF(132 + i *64, 445), stringFormat);
                }
            }
            e.Graphics.DrawString(">" + Convert.ToString(maxwind-1.0) + ".0 m/s", _smallFont,  _blackBrush, new PointF(132 + (maxwind - 1) *64, 445), stringFormat);
            
            g.DrawString("Frequency [%]", _mediumFont,  _blackBrush, 12, 5);
            g.DrawString("  0", _largeFont,  _blackBrush, 25, 435);
            base.OnPaint(e);

            //draw frequency levels
            for (int i = 1; i < 11; i++)
            {
                int levels = Convert.ToInt32(Convert.ToDouble(i) / 10 * scale);
                int lev1 = Convert.ToInt32(440 - levels);
                string s = (i * 10).ToString();
                if (i * 10 > Convert.ToInt32(classmax * 100))
                {
                    break;
                }
                g.DrawLine(p3, 55, lev1, Math.Max(759, 55 + (maxwind + 1) * 64), lev1);
                g.DrawString(s, _largeFont,  _blackBrush, 25, lev1 - 5);
                base.OnPaint(e);
            }
            p1.Dispose();p2.Dispose();p3.Dispose();
            
            stringFormat.Dispose();
            format1.Dispose();
            _smallFont.Dispose();
            _mediumFont.Dispose();
            _largeFont.Dispose();
            _blackBrush.Dispose();
            _greyBrush.Dispose();
        }

        //save imapge to clipboard
        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap bitMap = new Bitmap(panel1.Width, panel1.Height);
            panel1.DrawToBitmap(bitMap, new Rectangle(0, 0, panel1.Width*2, panel1.Height*2));
            Clipboard.SetDataObject(bitMap);
        }

        
        void WindclassesLoad(object sender, EventArgs e)
        {
        	Graphics g = CreateGraphics();
        	float dx = 96;
        	try
        	{
        		dx = g.DpiX;
        	}
        	finally
        	{
        		g.Dispose();
        	}
			dpi = dx;
			
        	Text = "Frequency distribution wind classes  -  " + MetFile; // Kuntner
        }
        
        void WindclassesResizeEnd(object sender, EventArgs e)
        {
            if (ClientSize.Width > 60)
            {
                panel1.Width = ClientSize.Width - 55;
                panel1.Height = ClientSize.Height;
            }
            
            panel1.Invalidate();
			panel1.Update();
        }
        
        void WindclassesResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized) 
            {
                WindclassesResizeEnd(null, null);
                // Maximized!
            }
            if (WindowState == FormWindowState.Normal) 
            {
                WindclassesResizeEnd(null, null);
                // Restored!
            }
        }
    }
}