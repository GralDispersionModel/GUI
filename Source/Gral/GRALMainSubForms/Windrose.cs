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
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;
using GralStaticFunctions;
using Gral;

namespace GralMainForms
{
    /// <summary>
    /// Show a wind rose for wind speed or wind stability classes
    /// </summary>
    public partial class Windrose : Form
    {
        private Point[] WindRosePoints = new Point[49];
        public double[,] SectFrequ = new double[16, 8];
        public double[] WndClasses = new double[7];
        public string MetFileName;
        public List<GralData.WindData> WindData;

        private int IniScale = 270;
        public int StartHour;
        public int FinalHour;
        public bool DrawFrames = false;
        public bool SmallSectors = false;
        public int BiasCorrection = 0;

        /// <summary>
        /// Windspeed:0 WindStabilityClasses:1
        /// </summary>
        public int DrawingMode = 0;

        private double SektMax = 0;
        private double XScale = 0;

        private Pen PenBlue = new Pen(Color.Blue);
        private Pen PenLightBlue = new Pen(Color.LightSkyBlue);
        private Pen PenGreen = new Pen(Color.Green);
        private Pen PenYellowGreen = new Pen(Color.YellowGreen);
        private Pen PenYellow = new Pen(Color.Yellow);
        private Pen PenOrange = new Pen(Color.Orange);
        private Pen PenRed = new Pen(Color.Red);
        private Pen PenDarkRed = new Pen(Color.DarkRed);
        private Pen PenBrown = new Pen(Color.Brown);
        private Pen PenGray = new Pen(Color.Gray, 1);

        private Brush BrushBlue = new SolidBrush(Color.Blue);
        private Brush BrushRedGreen = new SolidBrush(Color.LightSkyBlue);
        private Brush BrushGreen = new SolidBrush(Color.Green);
        private Brush BrushYellowGreen = new SolidBrush(Color.YellowGreen);
        private Brush BrushYellow = new SolidBrush(Color.Yellow);
        private Brush BrushOrange = new SolidBrush(Color.Orange);
        private Brush BrushRed = new SolidBrush(Color.Red);
        private Brush BrushBrown = new SolidBrush(Color.Brown);
        private Brush BrushDarkRed = new SolidBrush(Color.DarkRed);
        private Brush BrushLightBlue = new SolidBrush(Color.LightSkyBlue);
        private Brush BrushBlack = new SolidBrush(Color.Black);
        private StringFormat StringFormatNearFar;

        private Rectangle LegendPosition = St_F.WindRoseLegend;
        private Point MousedXdY = new Point();
        private bool MoveLegend = false;
        private Rectangle InfoPosition = St_F.WindRoseInfo;
        private bool MoveInfo = false;

        /// <summary>
        /// Show a wind speed or wind SC wind rose
        /// </summary>
        public Windrose()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IniScale = Convert.ToInt32(IniScale * 1.1);
            Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IniScale = Convert.ToInt32(IniScale * 0.9);
            Refresh();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            if (St_F.WindRoseFormSize.Height > 100)
            {
                this.Size = St_F.WindRoseFormSize;
            }

            if (St_F.Pin_Wind_Scale > 0.1)
            {
                button7.BackgroundImage = Gral.Properties.Resources.Pin_Down;
            }

            if (DrawingMode == 1)
            {
                Text = "Windrose stability -  " + MetFileName;
            }
            else
            {
                Text = "Windrose   -  " + MetFileName;
            }
            pictureBox1.Width = Math.Max(1, ClientSize.Width - 69);
            pictureBox1.Height = Math.Max(1, ClientRectangle.Height - 1);
            LegendPosition = St_F.WindRoseLegend;
            InfoPosition = St_F.WindRoseInfo;
        }

        void PictureBox1Paint(object sender, PaintEventArgs e)
        {
            int mid_x = pictureBox1.Width / 2; // Kuntner
            int mid_y = pictureBox1.Height / 2;

            //draw file name
            Graphics g = e.Graphics;

            StringFormat format2 = new StringFormat
            {
                Alignment = StringAlignment.Center
            };

            StringFormatNearFar = new StringFormat
            {
                LineAlignment = StringAlignment.Near,
                Alignment = StringAlignment.Near
            };

            Font kleinfont;
            if (St_F.Small_Font != null)
                kleinfont = new Font(St_F.Small_Font.FontFamily, (float)(St_F.Small_Font.Size * IniScale / 270), St_F.Small_Font.Style);
            else

                kleinfont = new Font("Arial", 8);

            int distance = (int)(g.MeasureString("J", kleinfont, 200).Height * 1.04);
            int y_text = pictureBox1.Height - distance * 7;

            InfoPosition.Width = (int)g.MeasureString("Bias correction not applied", kleinfont).Width;
            InfoPosition.Height = distance * 5;
            if (InfoPosition.X == 0 && InfoPosition.Y == 0)
            {
                InfoPosition.X = mid_x * 2 - 20 - InfoPosition.Width; 
                InfoPosition.Y = y_text;
            }
          
            g.DrawString("Data points: " + Convert.ToString(WindData.Count), kleinfont, BrushBlack, InfoPosition.X, InfoPosition.Y + distance, StringFormatNearFar);
            //g.DrawString(Convert.ToString(wind.Count), kleinfont, blackbrush, mid_x*2-20, InfoPosition.Y + distance, format1);
            //g.DrawString(wind[0].Date, kleinfont, blackbrush, mid_x*2-85, InfoPosition.Y + 2 * distance, format1);
            if (WindData.Count > 1)
                g.DrawString(WindData[0].Date + " - " + WindData[WindData.Count - 1].Date, kleinfont, BrushBlack, InfoPosition.X, InfoPosition.Y + 2 * distance, StringFormatNearFar);
            g.DrawString(Convert.ToString(StartHour) + ":00h-" + Convert.ToString(FinalHour) + ":00h", kleinfont, BrushBlack, InfoPosition.X, InfoPosition.Y + 3 * distance, StringFormatNearFar);
            g.DrawString(MetFileName, kleinfont, BrushBlack, InfoPosition.X, InfoPosition.Y + 4 * distance, StringFormatNearFar);
            if (BiasCorrection == 1)
            {
                g.DrawString("Bias correction applied", kleinfont, BrushBlack, InfoPosition.X, InfoPosition.Y + 5 * distance, StringFormatNearFar);
            }
            else if (BiasCorrection == 2)
            {
                g.DrawString("Bias correction not applied", kleinfont, BrushBlack, InfoPosition.X, InfoPosition.Y + 5 * distance, StringFormatNearFar);
            }

            g.DrawString("N", new Font("Arial", 15), BrushBlack, mid_x, 2, format2);



            //draw windrose
            double[] sektsum = new double[16];
            SektMax = 0;

            for (int i = 0; i < 16; i++)
            {
                for (int n = 0; n < 8; n++)
                {
                    sektsum[i] = SectFrequ[i, n] + sektsum[i];
                }
                SektMax = Math.Max(sektsum[i], SektMax);
            }

            SektMax = SektMax + XScale;

            //scaling factor to maximise wind rose in window
            if (SektMax <= 0)
                return;

            if (St_F.Pin_Wind_Scale > 0.1) // if x-axis is pinned
                SektMax = St_F.Pin_Wind_Scale;

            double scale = IniScale / SektMax;

            int x_legend = pictureBox1.Width - (int)(g.MeasureString("0.0 - 2.2 m/s", kleinfont).Width) - 30;

            if (x_legend < 0) return;
            StringFormatNearFar.Alignment = StringAlignment.Near;
            if (LegendPosition.X == 0 && LegendPosition.Y == 0)
            {
                LegendPosition.X = x_legend;
                LegendPosition.Y = 13;
            }

            LegendPosition.Width = (int)(25 + g.MeasureString(Convert.ToString(WndClasses[0]).PadLeft(2) + " -" + Convert.ToString(WndClasses[1]).PadLeft(2) + " m/s", kleinfont).Width);
            LegendPosition.Height = LegendPosition.Y + (int)Math.Max(20, distance * 1.01F) * 7;
            
            for (int n = 7; n >= 0; n--)
            {
                int VertDist = (int)Math.Max(20, distance * 1.01F);
                int y0 = LegendPosition.Y + VertDist * (7 - n);

                if (DrawingMode == 0)
                {
                    DrawWindSpeedScale(g, n, VertDist, y0, kleinfont);
                }
                else if (DrawingMode == 1)
                {
                    DrawWindSCScale(g, n, VertDist, y0, kleinfont);
                }
            }

            const double sectorangle = Math.PI / 8;

            double sectorwidth = 0.196;
            if (SmallSectors)
            {
                sectorwidth = Math.PI / 18;
            }

            for (int n = 7; n >= 0; n--)
            {
                int l = 1;
                for (int i = 0; i < 16; i++)
                {
                    //coordinates wind rose - drawing
                    int x1 = 0; int y1 = 0; int x2 = 0; int y2 = 0;
                    try
                    {
                        x1 = Convert.ToInt32(Math.Sin(sectorangle * i - sectorwidth) * sektsum[i] * scale);
                        y1 = Convert.ToInt32(Math.Cos(sectorangle * i - sectorwidth) * sektsum[i] * scale);
                        x2 = Convert.ToInt32(Math.Sin(sectorangle * i + sectorwidth) * sektsum[i] * scale);
                        y2 = Convert.ToInt32(Math.Cos(sectorangle * i + sectorwidth) * sektsum[i] * scale);
                    }
                    catch
                    { }
                    WindRosePoints[i + l - 1] = new Point(mid_x, mid_y);
                    WindRosePoints[i + l] = new Point(mid_x + x1, mid_y - y1);
                    WindRosePoints[i + l + 1] = new Point(mid_x + x2, mid_y - y2);
                    WindRosePoints[i + l + 2] = new Point(mid_x, mid_y);
                    sektsum[i] = sektsum[i] - SectFrequ[i, n];
                    l = l + 2;
                }

                if (DrawingMode == 0)
                {
                    DrawWindSpeedRose(g, n);
                }
                else if (DrawingMode == 1)
                {
                    DrawWindSCRose(g, n);
                }
            }

            //draw axis of windrose
            double div = Math.Round(SektMax * scale / 4, 0);
            Pen p = new Pen(Color.Black, 1);
            try
            {
                g.DrawLine(p, Convert.ToInt32(mid_x - div * 6), mid_y, Convert.ToInt32(mid_x + div * 6), mid_y);
                g.DrawLine(p, mid_x, Convert.ToInt32(mid_y - div * 6), mid_x, Convert.ToInt32(mid_y + div * 6));
            }
            catch { }
            //base.OnPaint(e);

            //draw circles
            p = new Pen(Color.Black, 1)
            {
                DashStyle = DashStyle.Dot
            };
            for (int i = 1; i < 5; i++)
            {
                try
                {
                    int x1 = Convert.ToInt32(div * i);
                    g.DrawEllipse(p, mid_x - x1, mid_y - x1, 2 * x1, 2 * x1);
                    g.DrawString(Convert.ToString(Math.Round((Convert.ToDouble(i) * 0.25 * SektMax * 100), 0)) + "%", kleinfont, BrushBlack, mid_x + x1, mid_y + 5);
                }
                catch { }
            }

            p.Dispose();
            kleinfont.Dispose();
            format2.Dispose();
            StringFormatNearFar.Dispose();
        }

        private void DrawWindSpeedScale(Graphics g, int n, int VertDist, int y0, Font kleinfont)
        {
            int x_legend = LegendPosition.X;

            switch (n)
            {
                case 7:
                    g.DrawRectangle(PenBlue, x_legend, y0, 20, VertDist);
                    g.FillRectangle(BrushBlue, x_legend, y0, 20, VertDist);
                    g.DrawString("0.0 - " + Convert.ToString(WndClasses[0]) + " m/s",
                                 kleinfont, BrushBlack, x_legend + 26, y0, StringFormatNearFar);
                    break;
                case 6:
                    g.DrawRectangle(PenLightBlue, x_legend, y0, 20, VertDist);
                    g.FillRectangle(BrushRedGreen, x_legend, y0, 20, VertDist);
                    g.DrawString(Convert.ToString(WndClasses[0]).PadLeft(2) + " -" + Convert.ToString(WndClasses[1]).PadLeft(2) + " m/s",
                                 kleinfont, BrushBlack, x_legend + 26, y0, StringFormatNearFar);
                    break;
                case 5:
                    g.DrawRectangle(PenGreen, x_legend, y0, 20, VertDist);
                    g.FillRectangle(BrushGreen, x_legend, y0, 20, VertDist);
                    g.DrawString(Convert.ToString(WndClasses[1]).PadLeft(2) + " -" + Convert.ToString(WndClasses[2]).PadLeft(2) + " m/s",
                                 kleinfont, BrushBlack, x_legend + 26, y0, StringFormatNearFar);
                    break;
                case 4:
                    g.DrawRectangle(PenYellowGreen, x_legend, y0, 20, VertDist);
                    g.FillRectangle(BrushYellowGreen, x_legend, y0, 20, VertDist);
                    g.DrawString(Convert.ToString(WndClasses[2]).PadLeft(2) + " -" + Convert.ToString(WndClasses[3]).PadLeft(2) + " m/s",
                                 kleinfont, BrushBlack, x_legend + 26, y0, StringFormatNearFar);
                    break;
                case 3:
                    g.DrawRectangle(PenYellow, x_legend, y0, 20, VertDist);
                    g.FillRectangle(BrushYellow, x_legend, y0, 20, VertDist);
                    g.DrawString(Convert.ToString(WndClasses[3]).PadLeft(2) + " -" + Convert.ToString(WndClasses[4]).PadLeft(2) + " m/s",
                                 kleinfont, BrushBlack, x_legend + 26, y0, StringFormatNearFar);
                    break;
                case 2:
                    g.DrawRectangle(PenOrange, x_legend, y0, 20, VertDist);
                    g.FillRectangle(BrushOrange, x_legend, y0, 20, VertDist);
                    g.DrawString(Convert.ToString(WndClasses[4]).PadLeft(2) + " -" + Convert.ToString(WndClasses[5]).PadLeft(2) + " m/s",
                                 kleinfont, BrushBlack, x_legend + 26, y0, StringFormatNearFar);
                    break;
                case 1:
                    g.DrawRectangle(PenRed, x_legend, y0, 20, VertDist);
                    g.FillRectangle(BrushRed, x_legend, y0, 20, VertDist);
                    g.DrawString(Convert.ToString(WndClasses[5]).PadLeft(2) + " -" + Convert.ToString(WndClasses[6]).PadLeft(2) + " m/s",
                                 kleinfont, BrushBlack, x_legend + 26, y0, StringFormatNearFar);
                    break;
                case 0:
                    g.DrawRectangle(PenBrown, x_legend, y0, 20, VertDist);
                    g.FillRectangle(BrushBrown, x_legend, y0, 20, VertDist);
                    g.DrawString(">" + Convert.ToString(WndClasses[6]).PadLeft(2) + "m/s",
                                 kleinfont, BrushBlack, x_legend + 26, y0, StringFormatNearFar);
                    break;
            }
        }

        private void DrawWindSCScale(Graphics g, int n, int VertDist, int y0, Font kleinfont)
        {
            int x_legend = LegendPosition.X;

            switch (n)
            {
                case 7:
                    g.DrawRectangle(PenDarkRed, x_legend, y0, 25, VertDist);
                    g.FillRectangle(BrushDarkRed, x_legend, y0, 25, VertDist);
                    g.DrawString("SC 1", kleinfont, BrushBlack, x_legend + 30, y0, StringFormatNearFar);
                    break;
                case 6:
                    g.DrawRectangle(PenRed, x_legend, y0, 25, VertDist);
                    g.FillRectangle(BrushRed, x_legend, y0, 25, VertDist);
                    g.DrawString("SC 2", kleinfont, BrushBlack, x_legend + 30, y0, StringFormatNearFar);
                    break;
                case 5:
                    g.DrawRectangle(PenOrange, x_legend, y0, 25, VertDist);
                    g.FillRectangle(BrushOrange, x_legend, y0, 25, VertDist);
                    g.DrawString("SC 3", kleinfont, BrushBlack, x_legend + 30, y0, StringFormatNearFar);
                    break;
                case 4:
                    g.DrawRectangle(PenYellowGreen, x_legend, y0, 25, VertDist);
                    g.FillRectangle(BrushYellowGreen, x_legend, y0, 25, VertDist);
                    g.DrawString("SC 4", kleinfont, BrushBlack, x_legend + 30, y0, StringFormatNearFar);
                    break;
                case 3:
                    g.DrawRectangle(PenGreen, x_legend, y0, 25, VertDist);
                    g.FillRectangle(BrushGreen, x_legend, y0, 25, VertDist);
                    g.DrawString("SC 5", kleinfont, BrushBlack, x_legend + 30, y0, StringFormatNearFar);
                    break;
                case 2:
                    g.DrawRectangle(PenLightBlue, x_legend, y0, 25, VertDist);
                    g.FillRectangle(BrushLightBlue, x_legend, y0, 25, VertDist);
                    g.DrawString("SC 6", kleinfont, BrushBlack, x_legend + 30, y0, StringFormatNearFar);
                    break;
                case 1:
                    g.DrawRectangle(PenBlue, x_legend, y0, 25, VertDist);
                    g.FillRectangle(BrushBlue, x_legend, y0, 25, VertDist);
                    g.DrawString("SC 7", kleinfont, BrushBlack, x_legend + 30, y0, StringFormatNearFar);
                    break;
            }
        }

        private void DrawWindSpeedRose(Graphics g, int n)
        {
            switch (n)
            {
                case 7:
                    g.FillPolygon(BrushBrown, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenBrown, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
                case 6:
                    g.FillPolygon(BrushRed, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenRed, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
                case 5:
                    g.FillPolygon(BrushOrange, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenOrange, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
                case 4:
                    g.FillPolygon(BrushYellow, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenYellow, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
                case 3:
                    g.FillPolygon(BrushYellowGreen, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenYellowGreen, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
                case 2:
                    g.FillPolygon(BrushGreen, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenGreen, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
                case 1:
                    g.FillPolygon(BrushRedGreen, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenLightBlue, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
                case 0:
                    g.FillPolygon(BrushBlue, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenBlue, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
            }
        }

        private void DrawWindSCRose(Graphics g, int n)
        {
            switch (n)
            {
                case 7:
                    g.FillPolygon(BrushBlue, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenBlue, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
                case 6:
                    g.FillPolygon(BrushLightBlue, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenLightBlue, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
                case 5:
                    g.FillPolygon(BrushGreen, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenGreen, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
                case 4:
                    g.FillPolygon(BrushYellowGreen, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenYellowGreen, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
                case 3:
                    g.FillPolygon(BrushOrange, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenOrange, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
                case 2:
                    g.FillPolygon(BrushRed, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenRed, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
                case 1:
                    g.FillPolygon(BrushDarkRed, WindRosePoints);
                    if (!DrawFrames)
                    {
                        g.DrawPolygon(PenDarkRed, WindRosePoints);
                    }
                    else
                    {
                        g.DrawPolygon(PenGray, WindRosePoints);
                    }
                    break;
            }
        }

        //save image to clipboard
        private void button3_Click(object sender, EventArgs e)
        {
            int scale = 2;
            if (pictureBox1.Width < 500)
            {
                scale = 4;
            }
            pictureBox1.Width *= scale;
            pictureBox1.Height *= scale;
            IniScale *= scale;
            pictureBox1.Refresh();
            Application.DoEvents();
            Bitmap bitMap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.DrawToBitmap(bitMap, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
            Clipboard.SetDataObject(bitMap);
            pictureBox1.Width /= scale;
            pictureBox1.Height /= scale;
            IniScale /= scale;
            pictureBox1.Refresh();
        }

        //increase the scale of the x-axis by one
        private void button5_Click(object sender, EventArgs e)
        {
            XScale = XScale + 0.01;
            Refresh();
        }

        //decrease the scale of the x-axis by one
        private void button4_Click(object sender, EventArgs e)
        {
            XScale = XScale - 0.01;
            Refresh();
        }

        void WindroseResizeEnd(object sender, EventArgs e) // Kuntner
        {
            pictureBox1.Width = Math.Max(1, ClientSize.Width - 69);
            pictureBox1.Height = Math.Max(1, ClientRectangle.Height - 1);

            pictureBox1.Refresh();
        }

        void WindroseFormClosed(object sender, FormClosedEventArgs e)
        {
            toolTip1.Dispose();
            pictureBox1.Dispose();

            PenBlue.Dispose();
            PenBrown.Dispose();
            PenGreen.Dispose();
            PenLightBlue.Dispose();
            PenOrange.Dispose();
            PenRed.Dispose();
            PenDarkRed.Dispose();
            PenYellowGreen.Dispose();
            PenYellow.Dispose();
            PenGray.Dispose();
            PenDarkRed.Dispose();

            BrushBlue.Dispose();
            BrushBrown.Dispose();
            BrushGreen.Dispose();
            BrushRedGreen.Dispose();
            BrushOrange.Dispose();
            BrushRed.Dispose();
            BrushYellowGreen.Dispose();
            BrushYellow.Dispose();
            BrushBlack.Dispose();
            BrushLightBlue.Dispose();
        }

        void Button6Click(object sender, EventArgs e)
        {
            St_F.SetSmallFont();
            pictureBox1.Refresh();
        }

        void Button7Click(object sender, EventArgs e)
        {
            if (St_F.Pin_Wind_Scale < 0.1)
            {
                St_F.Pin_Wind_Scale = SektMax;
                button7.BackgroundImage = Gral.Properties.Resources.Pin_Down;
            }
            else
            {
                St_F.Pin_Wind_Scale = 0;
                button7.BackgroundImage = Gral.Properties.Resources.Pin;
            }
        }

        void WindroseResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindroseResizeEnd(null, null);
                // Maximized!
            }
            else if (WindowState == FormWindowState.Normal)
            {
                WindroseResizeEnd(null, null);
                // Restored!
                // restore legend position
                LegendPosition.X = 0;
                LegendPosition.Y = 0;
                InfoPosition.X = 0;
                InfoPosition.Y = 0;
            }
            
            St_F.WindRoseFormSize = this.Size;

        }

        private void button8_Click(object sender, EventArgs e)
        {
            // show a table with frequencies for 16 directions and 8 (7) classes
            WindroseTable wrt = new WindroseTable
            {
                MetfileName = "wind velocity " + MetFileName,
                SectFrequency = SectFrequ,
                WndClasses = WndClasses,
                Mode = DrawingMode
            };
            if (DrawingMode == 1)
            {
                wrt.MetfileName = "stability classes " + MetFileName;
            }
            wrt.Show();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            //check if mouse cursor is within the legend -> move legend 
            if (LegendPosition.Contains(e.Location))
            {
                MoveLegend = true;
                MousedXdY = new Point(e.X - LegendPosition.X, e.Y - LegendPosition.Y);
            }

            //MessageBox.Show(InfoPosition.Left.ToString() + "/" + InfoPosition.Right.ToString() + "/" + e.X.ToString() +"/" +e.Y.ToString());

            if (InfoPosition.Contains(e.Location))
            {
                MoveInfo = true;
                MousedXdY = new Point(e.X - InfoPosition.X, e.Y - InfoPosition.Y);
            }
        }


        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (MoveLegend)
            {
                LegendPosition.X = e.X - MousedXdY.X;
                LegendPosition.Y = e.Y - MousedXdY.Y;
                St_F.WindRoseLegend.X = LegendPosition.X;
                St_F.WindRoseLegend.Y = LegendPosition.Y;
                pictureBox1.Refresh();
            }
            if(MoveInfo)
            {
                InfoPosition.X = e.X - MousedXdY.X;
                InfoPosition.Y = e.Y - MousedXdY.Y;
                St_F.WindRoseInfo.X = InfoPosition.X;
                St_F.WindRoseInfo.Y = InfoPosition.Y;
                pictureBox1.Refresh();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (MoveLegend)
            {
                MoveLegend = false;
            }
            if (MoveInfo)
            {
                MoveInfo = false;
            }
        }

    }
}