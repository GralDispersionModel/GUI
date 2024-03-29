﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace GralMainForms
{
    public partial class WindDistribution : Form
    {
        public double[] WClassFrequency;
        public string MetFile;
        public int StartHour;
        public int FinalHour;
        public int MaxWind;
        public double MeanWindSpeed;
        public string StartDate = string.Empty;
        public string EndDate = string.Empty;
        private const float HorSize = 762F;
        private const float VertSize = 508F;

        public WindDistribution()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int CopyToClipboardScale = 3;
            if (panel1.Width < 500)
            {
                CopyToClipboardScale = 5;
            }
            panel1.Width *= CopyToClipboardScale;
            panel1.Height *= CopyToClipboardScale;
            panel1.Refresh();
            Application.DoEvents();
            Bitmap bitMap = new Bitmap(panel1.Width, panel1.Height);
            panel1.DrawToBitmap(bitMap, new Rectangle(0, 0, panel1.Width, panel1.Height));
            Clipboard.SetDataObject(bitMap);
            panel1.Width /= CopyToClipboardScale;
            panel1.Height /= CopyToClipboardScale;
            panel1.Refresh();
        }

        private void WindDistribution_Load(object sender, EventArgs e)
        {
            Text = "Wind velocity - cumulative frequency distribution - " + MetFile; 
        }

        private void WindDistribution_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindDistribution_ResizeEnd(null, null);
                // Maximized!
            }
            if (WindowState == FormWindowState.Normal)
            {
                WindDistribution_ResizeEnd(null, null);
                // Restored!
            }
        }

        private void WindDistribution_ResizeEnd(object sender, EventArgs e)
        {
            if (ClientSize.Width > 60)
            {
                panel1.Width = ClientSize.Width - 55;
                panel1.Height = ClientSize.Height;
            }

            panel1.Invalidate();
            panel1.Update();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (panel1.Width < 20 || panel1.Height < 20)
            {
                return;
            }

            Graphics g = e.Graphics;
            g.Clear(Color.White);
            float _scale = Math.Min(panel1.Width / HorSize, panel1.Height / VertSize);
            g.ScaleTransform(_scale, _scale);

            StringFormat format1 = new StringFormat
            {
                LineAlignment = StringAlignment.Near,
                Alignment = StringAlignment.Near
            };

            Font _smallFont = new Font("Arial", 10);
            Font _mediumFont = new Font("Arial", 12);
            Font _largeFont = new Font("Arial", 15);
            Brush _blackBrush = new SolidBrush(Color.Black);
            Brush _greyBrush = new SolidBrush(Color.Gray);
            Pen p1 = new Pen(Color.Black, 3);
            Pen p2 = new Pen(Color.Black, 1);
            Pen p3 = new Pen(Color.DarkGray, 1)
            {
                DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot
            };
            RectangleF ClipBounds = new RectangleF(0, 0, HorSize, VertSize);

            SizeF _lenght  = g.MeasureString(MetFile, _mediumFont);
            SizeF _lenght2 = g.MeasureString("100", _smallFont);

            int x0 = (int)(_lenght2.Width + _lenght2.Height) + 10;
            int y0 = (int) VertSize - (int)_lenght2.Height * 2 - 30;

            g.DrawString(MetFile, _mediumFont, _blackBrush, (HorSize - _lenght.Width) / 2, _lenght.Height + 4, format1);
            string infodata = StartDate + " - " + EndDate + " " + StartHour.ToString() + ":00 - " + FinalHour.ToString() + ":00  Mean wind speed: " + Math.Round(MeanWindSpeed, 1).ToString() + "m/s";
            _lenght = g.MeasureString(infodata, _smallFont);
            g.DrawString(infodata, _smallFont, _blackBrush, (HorSize - _lenght.Width) / 2, _lenght.Height * 2.5F + 4, format1);

            double xfac = (HorSize - x0 - 10) / (MaxWind - 1);
            double yfac = (y0 - 80) / 10;
            int ymin = (int)(y0 - 10 * yfac);
            int xmax = (int)(x0 + (MaxWind - 1) * xfac);

            if (xfac < 0 || yfac < 0)
            {
                return;
            }

            //draw frequency levels
            for (int i = 0; i < 11; i++)
            {
                int yr = (int)(y0 - i * yfac);
                g.DrawLine(p3, x0 - 3, yr, xmax, yr);
                g.DrawString((i * 10).ToString(), _smallFont, _blackBrush, x0 - _lenght2.Width - 1, (int)(yr - _lenght2.Height / 2));
            }

            int xr = 0;
            for (int i = 0; i < (MaxWind); i++)
            {
                string fs = Convert.ToString(i);
                xr = (int)(x0 + i * xfac);
                g.DrawString(fs, _smallFont, _blackBrush, new PointF(xr - _lenght2.Height / 2, y0 + 5), format1);
                g.DrawLine(p3, xr, ymin, xr, y0 + 2);
            }
            
            //draw axis
            p3.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            g.DrawLine(p2, x0, ymin, x0, y0 + 5);
            g.DrawLine(p2, x0 - 5, y0, HorSize - 10, y0);
            g.DrawLine(p2, x0, ymin, HorSize - 10, ymin);
            g.DrawLine(p2, xmax, ymin, xmax, y0);

            ClipBounds.Width = xr;
            
            g.DrawString("v [m/s]", _smallFont, _blackBrush, new PointF((int) (HorSize / 2 - 30), (int) (y0 + _lenght2.Height)), format1);

            StringFormat verticalString = new StringFormat
            {
                FormatFlags = StringFormatFlags.DirectionVertical,
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near
            };
            SizeF _lenght3 = g.MeasureString("Frequency [%]", _smallFont);
            g.DrawString("Frequency [%]", _smallFont, _blackBrush, 4, (int) ((VertSize - _lenght3.Width) / 2), verticalString);
            
            // Draw Graph
            Pen p4 = new Pen(Color.Blue, 3);
            PointF[] _pt = new PointF[WClassFrequency.Length];
            double vw = 0;
            g.Clip = new Region(ClipBounds);
            for (int i = 0; i < WClassFrequency.Length; i++)
            {
                _pt[i].X = (float) (x0 + vw * xfac);
                _pt[i].Y = (float)(y0 - WClassFrequency[i] * 10 * yfac);
                vw += 0.25;
            }
             
            g.DrawCurve(p4, _pt, 0F);
            g.ResetClip();
            
            p1.Dispose(); p2.Dispose(); p3.Dispose(); p4.Dispose();
            verticalString.Dispose();
            format1.Dispose();
            _smallFont.Dispose();
            _mediumFont.Dispose();
            _largeFont.Dispose();
            _blackBrush.Dispose();
            _greyBrush.Dispose();
        }
    }
}
