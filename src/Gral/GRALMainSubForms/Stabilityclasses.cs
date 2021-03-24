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
    /// Form to show the stability classes of a meteo file
    /// </summary>
    public partial class Stabilityclasses : Form
    {
        public double[] ScClassFrequency = new double[7];
        public string MetFile;
		public List <GralData.WindData> Wind;
        public GralData.WindRoseSettings WindRoseSetting;
        
        public Stabilityclasses()
        {
            InitializeComponent();
        }
        protected override void OnPaint(PaintEventArgs e)
        {            
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            StringFormat format1 = new StringFormat();
            Font _smallFont = new Font("Arial", 8);
            Font _mediumFont = new Font("Arial", 9);
            Font _largeFont = new Font("Arial", 10);
            Brush _blackBrush = new SolidBrush(Color.Black);
                    
            format1.LineAlignment = StringAlignment.Near;
            format1.Alignment = StringAlignment.Near;
            
            SizeF _lenght = g.MeasureString(MetFile, _smallFont);
            int distance = (int) _lenght.Height + 2;
            
            float _x = Math.Max(0, panel1.Width - _lenght.Width - 5);
            g.DrawString(MetFile, _smallFont, _blackBrush, _x, 2, format1);
            
            string _data = "Data points: " + Convert.ToString(Wind.Count);
            _lenght = g.MeasureString(_data, _smallFont);
            _x = Math.Max(0, panel1.Width - _lenght.Width - 5);
            g.DrawString(_data, _smallFont, _blackBrush, _x, 2 + distance, format1);
            
            if (Wind.Count > 1)
			{
			    _data = Wind[0].Date + " - " + Wind[Wind.Count - 1].Date;
			   _lenght = g.MeasureString(_data, _smallFont);
			    _x = Math.Max(0, panel1.Width - _lenght.Width - 5);
			    g.DrawString(_data, _smallFont, _blackBrush, _x, 2 + 2 * distance, format1);
			}
            
			
			float _scale = Math.Min(panel1.Width / 784F, panel1.Height / 473F);
			g.ScaleTransform(_scale, _scale);

			//scaling factor
            double classmax = 0;
            for (int i = 0; i < 7; i++)
            {
                classmax = Math.Max(ScClassFrequency[i], classmax);
            }
            if (WindRoseSetting.MaxScaleVertical > 0)
            {
                classmax = WindRoseSetting.MaxScaleVertical / 10D;
            }
            double scale = 400 / classmax;

            

            //draw diagram
            for (int i = 0; i < 7; i++)
            {
                int hoehe = Convert.ToInt32(ScClassFrequency[i] * scale);
                int ecke1 = Convert.ToInt32(55 + i * 80);
                g.FillRectangle(new HatchBrush(HatchStyle.Percent80, Color.White), ecke1, 440 - hoehe, 80, hoehe);
                g.DrawRectangle(new Pen(Color.Black), ecke1, 440 - hoehe, 80, hoehe);
                //base.OnPaint(e);
            }

            //draw axis
            Pen p1 = new Pen(Color.Black, 3);
            Pen p2 = new Pen(Color.Black, 3);
            Pen p3 = new Pen(Color.Black, 1)
            {
                DashStyle = DashStyle.Dot
            };
            p2.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(p1, 55, 440, 615, 440);
            g.DrawLine(p2, 55, 440, 55, 20);
            g.DrawString("very unstable", _smallFont, _blackBrush, 60, 450);
            g.DrawString("unstable", _smallFont, _blackBrush, 150, 450);
            g.DrawString("slightly unstable", _smallFont, _blackBrush, 215, 450);
            g.DrawString("neutral", _smallFont, _blackBrush, 315, 450);
            g.DrawString("slightly stable", _smallFont, _blackBrush, 382, 450);
            g.DrawString("stable", _smallFont, _blackBrush, 475, 450);
            g.DrawString("very stable", _smallFont, _blackBrush, 545, 450);
            g.DrawString("Frequency [%]", _mediumFont, _blackBrush, 12, 5);
            g.DrawString("  0", _largeFont, _blackBrush, 25, 435);
            //base.OnPaint(e);

          	//draw frequency levels
            for (int i = 1; i < 11; i += 1)
            {
                int levels = Convert.ToInt32(Convert.ToDouble(i) / 10 * scale);
                int lev1 = Convert.ToInt32(440 - levels);
                string s = (i * 10).ToString();
                if (i * 10 > Convert.ToInt32(classmax * 100))
                {
                    break;
                }
                g.DrawLine(p3, 55, lev1, 615, lev1);
                g.DrawString(s, _largeFont, _blackBrush, 25, lev1 - 5);
                base.OnPaint(e);
            }
            p1.Dispose();p2.Dispose();p3.Dispose();
            format1.Dispose();
            _smallFont.Dispose();
            _mediumFont.Dispose();
            _largeFont.Dispose();
            _blackBrush.Dispose();
        }

        private void Form5_Load(object sender, EventArgs e)
        {
        	Text = "Frequency distribution stability classes   -  " + MetFile; // Kuntner
        }

        //save image to clipboard
        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap bitMap = new Bitmap(panel1.Width, panel1.Height);
            panel1.DrawToBitmap(bitMap, new Rectangle(0, 0, panel1.Width, panel1.Height));
            Clipboard.SetDataObject(bitMap);
        }

        
        void StabilityclassesFormClosed(object sender, FormClosedEventArgs e)
        {
        	toolTip1.Dispose();
        	panel1.Dispose();
        	button1.Dispose();
        }
        
        void StabilityclassesResizeEnd(object sender, EventArgs e)
        {
            if (ClientSize.Width > 60)
            {
                panel1.Width = ClientSize.Width - 55;
                panel1.Height = ClientSize.Height;
            }
            
            panel1.Invalidate();
			panel1.Update();
        }
        
        void StabilityclassesResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized) 
            {
                StabilityclassesResizeEnd(null, null);
                // Maximized!
            }
            if (WindowState == FormWindowState.Normal) 
            {
                StabilityclassesResizeEnd(null, null);
                // Restored!
            }
        }
    }
}