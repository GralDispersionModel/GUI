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
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using GralStaticFunctions;

namespace GralMainForms
{
    /// <summary>
    /// Show a wind rose for wind speed or wind stability classes
    /// </summary>
    public partial class Windrose : Form
    {
        public double[,] SectFrequ = new double[16, 8];
        public double[] WndClasses = new double[7];
public string MetFileName;
        public List<GralData.WindData> WindData;
        public int WindSectorCount = 16;

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
        private Pen PenBlack = new Pen(Color.Black, 1);
        private Pen[] pensWind;
        private Pen[] pensSC;
        private Brush[] brushesWind;
        private Brush[] brushesSC;
        private Pen PenGrayTransparent = new Pen(Color.FromArgb(60, Color.Gray), 1);

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
        private System.Drawing.Point MousedXdY = new System.Drawing.Point();
        private bool MoveLegend = false;
        private Rectangle InfoPosition = St_F.WindRoseInfo;
        private bool MoveInfo = false;
        private int CopyToClipboardScale = 1;

        /// <summary>
        /// Show a wind speed or wind SC wind rose
        /// </summary>
        public Windrose()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Increase Scale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            IniScale = Convert.ToInt32(IniScale * 1.1);
            Refresh();
        }

        /// <summary>
        /// Decrease Scale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            IniScale = Convert.ToInt32(IniScale * 0.9);
            Refresh();
        }

        void form_MouseWheel(object sender, MouseEventArgs e) // Kuntner
        {
            if (e.Delta > 0)
            {
                if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
                {
                    button1_Click(null, null); // Scrollrad Up
                }
                else
                {
                    button5_Click(null, null);
                }
            }

            if (e.Delta < 0)
            {
                if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
                {
                    button2_Click(null, null);// Scrollrad Down
                }
                else
                {
                    button4_Click(null, null);
                }
            }
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
            MouseWheel += new MouseEventHandler(form_MouseWheel);
            pensWind = new Pen[] { PenBlue, PenLightBlue, PenGreen, PenYellowGreen, PenYellow, PenOrange, PenRed, PenBrown };
            brushesWind = new Brush[] { BrushBlue, BrushRedGreen, BrushGreen, BrushYellowGreen, BrushYellow, BrushOrange, BrushRed, BrushBrown };
            pensSC = new Pen[] { PenBlue, PenBlue, PenLightBlue, PenGreen, PenYellowGreen, PenOrange, PenRed, PenDarkRed };
            brushesSC = new Brush[] { BrushBlue, BrushBlue, BrushLightBlue, BrushGreen, BrushYellowGreen, BrushOrange, BrushRed, BrushDarkRed };
            PenBlack.Width = 1.3F;
            PenBlack.Alignment = PenAlignment.Inset;
        }

        void PictureBox1Paint(object sender, PaintEventArgs e)
        {
            if (pictureBox1.Width < 20 || pictureBox1.Height < 20)
            {
                return;
            }

            int mid_x = pictureBox1.Width / 2; // Kuntner
            int mid_y = pictureBox1.Height / 2;

            e.Graphics.Clear(Color.White);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            Graphics g = e.Graphics;

            //draw file name
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
            {
                kleinfont = new Font(St_F.Small_Font.FontFamily, (float)(St_F.Small_Font.Size * IniScale / 270), St_F.Small_Font.Style);
            }
            else
            {
                kleinfont = new Font("Arial", 8);
            }

            int distance = (int)(g.MeasureString("J", kleinfont, 200).Height * 1.04);
            int y_text = pictureBox1.Height - distance * 7;

            InfoPosition.Width = (int)g.MeasureString("Bias correction not applied", kleinfont).Width;
            InfoPosition.Height = distance * 5;
            if (InfoPosition.X == 0 && InfoPosition.Y == 0)
            {
                InfoPosition.X = mid_x * 2 - 20 - InfoPosition.Width;
                InfoPosition.Y = y_text;
            }

            {
                int _x0 = InfoPosition.X * CopyToClipboardScale;
                int _y0 = InfoPosition.Y * CopyToClipboardScale;
                g.DrawString("Data points: " + Convert.ToString(WindData.Count), kleinfont, BrushBlack, _x0, _y0 + distance, StringFormatNearFar);
                //g.DrawString(Convert.ToString(wind.Count), kleinfont, blackbrush, mid_x*2-20, _y0 + distance, format1);
                //g.DrawString(wind[0].Date, kleinfont, blackbrush, mid_x*2-85, _y0 + 2 * distance, format1);
                if (WindData.Count > 1)
                {
                    g.DrawString(WindData[0].Date + " - " + WindData[WindData.Count - 1].Date, kleinfont, BrushBlack, _x0, _y0 + 2 * distance, StringFormatNearFar);
                }

                g.DrawString(Convert.ToString(StartHour) + ":00h-" + Convert.ToString(FinalHour) + ":00h", kleinfont, BrushBlack, _x0, _y0 + 3 * distance, StringFormatNearFar);
                g.DrawString(MetFileName, kleinfont, BrushBlack, _x0, _y0 + 4 * distance, StringFormatNearFar);
                if (BiasCorrection == 1)
                {
                    g.DrawString("Bias correction applied", kleinfont, BrushBlack, _x0, _y0 + 5 * distance, StringFormatNearFar);
                }
                else if (BiasCorrection == 2)
                {
                    g.DrawString("Bias correction not applied", kleinfont, BrushBlack, _x0, _y0 + 5 * distance, StringFormatNearFar);
                }
            }

            //draw windrose
            double[] sektsum = new double[WindSectorCount];
            SektMax = 0;
            double sektMin = double.MaxValue;
            int MinSektNr = 0;

            for (int i = 0; i < WindSectorCount; i++)
            {
                for (int n = 0; n < 8; n++)
                {
                    sektsum[i] = SectFrequ[i, n] + sektsum[i];
                }
                SektMax = Math.Max(sektsum[i], SektMax);
                if (sektsum[i] < sektMin)
                {
                    sektMin = sektsum[i];
                    MinSektNr = i;
                }
            }

            SektMax += XScale;

            SektMax = Math.Round(SektMax * 100) * 0.01;

            //scaling factor to maximise wind rose in window
            if (SektMax <= 0)
            {
                return;
            }

            if (St_F.Pin_Wind_Scale > 0.1) // if x-axis is pinned
            {
                SektMax = St_F.Pin_Wind_Scale;
            }

            int FrStep = 30; // frequency step
            if (SektMax > 0.7 && SektMax < 2)
            {
                FrStep = 20;
            }
            else if (SektMax > 0.4)
            {
                FrStep = 10;
            }
            else if (SektMax > 0.2)
            {
                FrStep = 5;
            }
            else if (SektMax > 0.1)
            {
                FrStep = 4;
            }
            else
            {
                FrStep = 2;
            }

            int NumberOfScales = (int)(SektMax * 100 / FrStep);

            double scale = IniScale / SektMax;
            double sectorangle = Math.PI * 2 / WindSectorCount;

            //draw axis of windrose
            //double div = SektMax * scale / NumberOfScales;
            double div = SektMax * scale / (SektMax * 100 / FrStep);

            using (Pen p = new Pen(Color.Black, 1))
            {
                Pen p2 = new Pen(Color.DarkGray, 1)
                {
                    DashStyle = DashStyle.DashDot
                };
                StringFormat StringFormatCenter = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };

                try
                {
                    double r = div * NumberOfScales;
                    string wi = "";
                    SizeF str = g.MeasureString("235,5 °", kleinfont, 200);

                    for (int i = 0; i < WindSectorCount; i++)
                    {
                        g.DrawLine(p2, mid_x, mid_y, Convert.ToInt32(mid_x + r * Math.Sin(i * sectorangle)), Convert.ToInt32(mid_y - r * Math.Cos(i * sectorangle)));
                        wi = Convert.ToString(Math.Round(i * sectorangle * 180 / Math.PI, 1)) + " " + "\x00B0";

                        double dx = (r + 0.5 * (str.Width + str.Height)) * Math.Sin(i * sectorangle);
                        double dy = -(r + 0.5 * (str.Width + str.Height)) * Math.Cos(i * sectorangle);

                        g.DrawString(wi, kleinfont, BrushBlack, Convert.ToInt32(mid_x + dx), Convert.ToInt32(mid_y + dy), StringFormatCenter);
                    }
                }
                catch { }
                //base.OnPaint(e);

                p2 = new Pen(Color.DarkGray, 1)
                {
                    DashStyle = DashStyle.Dot
                };

                //draw circles and text for frequency
                Brush brushWhite = new SolidBrush(Color.White);
                for (int i = 1; i < NumberOfScales + 1; i++)
                {
                    try
                    {
                        int x1 = Convert.ToInt32(div * i);
                        if (i < NumberOfScales)
                        {
                            g.DrawEllipse(p2, mid_x - x1, mid_y - x1, 2 * x1, 2 * x1);
                        }
                        else
                        {
                            g.DrawEllipse(p, mid_x - x1, mid_y - x1, 2 * x1, 2 * x1);
                        }

                        int dx = Convert.ToInt32(x1 * Math.Sin(MinSektNr * sectorangle));
                        int dy = -Convert.ToInt32(x1 * Math.Cos(MinSektNr * sectorangle));
                        string fr = Convert.ToString(i * FrStep) + "%";
                        SizeF str = g.MeasureString(fr, kleinfont, 200);
                        g.FillRectangle(brushWhite, mid_x + dx - str.Width / 2, mid_y + dy - str.Height / 2, str.Width, str.Height);
                        g.DrawString(fr, kleinfont, BrushBlack, mid_x + dx, mid_y + dy, StringFormatCenter);
                    }
                    catch { }
                }
                StringFormatCenter.Dispose();
                brushWhite.Dispose();
            }

            int x_legend = pictureBox1.Width - (int)(g.MeasureString("0.0 - 2.2 m/s", kleinfont).Width) - 30;

            if (x_legend < 0)
            {
                return;
            }

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
                int y0 = LegendPosition.Y * CopyToClipboardScale + VertDist * (7 - n);

                if (DrawingMode == 0)
                {
                    DrawWindSpeedScale(g, n, VertDist, y0, kleinfont);
                }
                else if (DrawingMode == 1)
                {
                    DrawWindSCScale(g, n, VertDist, y0, kleinfont);
                }
            }

            double sectorwidth = Math.PI / WindSectorCount;
            if (SmallSectors)
            {
                sectorwidth = Math.PI / WindSectorCount * 0.85;
            }

            for (int n = 7; n >= 0; n--)
            {
                for (int i = 0; i < WindSectorCount; i++)
                {
                    int radius = Convert.ToInt32(sektsum[i] * scale);
                    float startAngle = (float)(sectorangle * i - sectorwidth) * 180 / MathF.PI - 90;
                    float sectorAnglePie = (float)(sectorwidth * 2) * 180 / MathF.PI;
                    
                    sektsum[i] = sektsum[i] - SectFrequ[i, n];
                    
                    if (DrawingMode == 0)
                    {
                        DrawWindSpeedRose(g, pensWind, brushesWind, n, mid_x, mid_y, radius, startAngle, sectorAnglePie);
                    }
                    else if (DrawingMode == 1 && n > 0)
                    {
                        DrawWindSpeedRose(g, pensSC, brushesSC, 8 - n, mid_x, mid_y, radius, startAngle, sectorAnglePie);
                    }
                }

                PenGrayTransparent.DashStyle = DashStyle.Dot;

                //Draw circles with transparent color above areas with wind sectors
                for (int i = 1; i < NumberOfScales + 1; i++)
                {
                    int x1 = Convert.ToInt32(div * i);
                    if (i < NumberOfScales)
                    {
                        g.DrawEllipse(PenGrayTransparent, mid_x - x1, mid_y - x1, 2 * x1, 2 * x1);
                    }
                    else
                    {
                        g.DrawEllipse(PenGrayTransparent, mid_x - x1, mid_y - x1, 2 * x1, 2 * x1);
                    }
                }

                kleinfont.Dispose();
                format2.Dispose();
                StringFormatNearFar.Dispose();
            }
        }

        private void DrawWindSpeedScale(Graphics g, int n, int VertDist, int y0, Font kleinfont)
        {
            int x_legend = LegendPosition.X * CopyToClipboardScale;
           
           
            if (n >= 0 && n < 8 && n < pensWind.Length && n < brushesWind.Length)
            {
                switch (n)
                {
                    case 7:
                        g.DrawRectangle(pensWind[7 - n], x_legend, y0, 20, VertDist);
                        g.FillRectangle(brushesWind[7 - n], x_legend, y0, 20, VertDist);
                        g.DrawString("0.0 - " + Convert.ToString(WndClasses[0]) + " m/s",
                                     kleinfont, BrushBlack, x_legend + 26, y0, StringFormatNearFar);
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        g.DrawRectangle(pensWind[7 - n], x_legend, y0, 20, VertDist);
                        g.FillRectangle(brushesWind[7 - n], x_legend, y0, 20, VertDist);
                        g.DrawString(Convert.ToString(WndClasses[6 - n]).PadLeft(2) + " -" + Convert.ToString(WndClasses[7 - n]).PadLeft(2) + " m/s",
                                     kleinfont, BrushBlack, x_legend + 26, y0, StringFormatNearFar);
                        break;
                    case 0:
                        g.DrawRectangle(pensWind[7 - n], x_legend, y0, 20, VertDist);
                        g.FillRectangle(brushesWind[7 - n], x_legend, y0, 20, VertDist);
                        g.DrawString(">" + Convert.ToString(WndClasses[6]).PadLeft(2) + "m/s",
                                     kleinfont, BrushBlack, x_legend + 26, y0, StringFormatNearFar);
                        break;
                }
            }
        }

        private void DrawWindSCScale(Graphics g, int n, int VertDist, int y0, Font kleinfont)
        {
            if (n > 0 && n < 8 && n < pensSC.Length && n < brushesSC.Length)
            {
                int x_legend = LegendPosition.X * CopyToClipboardScale;
                g.DrawRectangle(pensSC[n], x_legend, y0, 25, VertDist);
                g.FillRectangle(brushesSC[n], x_legend, y0, 25, VertDist);
                g.DrawString("SC " + (8 - n).ToString(), kleinfont, BrushBlack, x_legend + 30, y0, StringFormatNearFar);
            }
        }

        /// <summary>
        /// Draw Wind Rose Sectors
        /// </summary>
        /// <param name="g"></param>
        /// <param name="n"></param>
        private void DrawWindSpeedRose(Graphics g, Pen[] pen, Brush[] brush, int n, int mid_x, int mid_y, int radius, float startAngle, float sectorAngle)
        {
            if (radius > 0)
            {
                if (n >= 0 && n < brush.Length)
                {
                    g.FillPie(brush[n], new Rectangle(mid_x - radius, mid_y - radius, radius * 2, radius * 2), startAngle, sectorAngle);
                }
                if (n >= 0 && n < brush.Length)
                {
                    if (DrawFrames)
                    {
                        g.DrawPie(PenBlack, new Rectangle(mid_x - radius, mid_y - radius, radius * 2, radius * 2), startAngle, sectorAngle);
                    }
                    else
                    {
                        g.DrawPie(pen[n], new Rectangle(mid_x - radius, mid_y - radius, radius * 2, radius * 2), startAngle, sectorAngle);
                    }
                }             
            }
        }

        /// <summary>
        /// save image to clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            CopyToClipboardScale = 2;
            if (pictureBox1.Width < 500)
            {
                CopyToClipboardScale = 4;
            }
            pictureBox1.Width *= CopyToClipboardScale;
            pictureBox1.Height *= CopyToClipboardScale;
            IniScale *= CopyToClipboardScale;
            pictureBox1.Refresh();
            System.Windows.Forms.Application.DoEvents();
            Bitmap bitMap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.DrawToBitmap(bitMap, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
            Clipboard.SetDataObject(bitMap);
            pictureBox1.Width /= CopyToClipboardScale;
            pictureBox1.Height /= CopyToClipboardScale;
            IniScale /= CopyToClipboardScale;
            CopyToClipboardScale = 1;
            pictureBox1.Refresh();
        }

        /// <summary>
        /// increase the scale of the x-axis by one step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            XScale += 0.01;
            Refresh();
        }

        /// <summary>
        /// decrease the scale of the x-axis by one step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            XScale -= 0.01;
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
            PenBlack.Dispose();
            PenDarkRed.Dispose();
            PenGrayTransparent.Dispose();

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
            MouseWheel -= new MouseEventHandler(form_MouseWheel);
        }

        /// <summary>
        /// Set Font for the wind rose view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button6Click(object sender, EventArgs e)
        {
            St_F.SetSmallFont();
            pictureBox1.Refresh();
        }

        /// <summary>
        /// Pin the scale for drawing wind roses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// show a table with frequencies for WindSectorCount directions and 8 (7) classes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            WindroseTable wrt = new WindroseTable
            {
                MetfileName = "wind velocity " + MetFileName,
                SectFrequency = SectFrequ,
                WndClasses = WndClasses,
                Mode = DrawingMode,
                StartPosition = FormStartPosition.Manual
            };
            if (DrawingMode == 1)
            {
                wrt.MetfileName = "stability classes " + MetFileName;
            }
            wrt.Location = new Point(Left + 20, Top + 20);
            wrt.Show();
        }

        /// <summary>
        /// Use the mouse cursor to move the legend or the scale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (LegendPosition.Contains(e.Location))
            {
                MoveLegend = true;
                MousedXdY = new Point(e.X - LegendPosition.X, e.Y - LegendPosition.Y);
            }
            else if (InfoPosition.Contains(e.Location)) // legend has priority
            {
                MoveInfo = true;
                MousedXdY = new Point(e.X - InfoPosition.X, e.Y - InfoPosition.Y);
            }
        }


        /// <summary>
        /// Move the legend or the scale using the mouse cursor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //show SizeAll cursor when moving mouse above legend or info box
            if (LegendPosition.Contains(e.Location) || InfoPosition.Contains(e.Location))
            {
                pictureBox1.Cursor = Cursors.SizeAll;
            }
            else if (pictureBox1.Cursor != Cursors.Arrow)
            {
                pictureBox1.Cursor = Cursors.Arrow;
            }

            if (MoveLegend)
            {
                LegendPosition.X = e.X - MousedXdY.X;
                LegendPosition.Y = e.Y - MousedXdY.Y;
                St_F.WindRoseLegend.X = LegendPosition.X;
                St_F.WindRoseLegend.Y = LegendPosition.Y;
                pictureBox1.Refresh();
            }
            if (MoveInfo)
            {
                InfoPosition.X = e.X - MousedXdY.X;
                InfoPosition.Y = e.Y - MousedXdY.Y;
                St_F.WindRoseInfo.X = InfoPosition.X;
                St_F.WindRoseInfo.Y = InfoPosition.Y;
                pictureBox1.Refresh();
            }
        }

        /// <summary>
        /// Stop moving the scale or the legend
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (MoveLegend)
            {
                MoveLegend = false;
                pictureBox1.Cursor = Cursors.Arrow;
            }
            if (MoveInfo)
            {
                MoveInfo = false;
                pictureBox1.Cursor = Cursors.Arrow;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Oemplus || keyData == Keys.Add)
            {
                button5_Click(null, null);
            }
            else if (keyData == Keys.OemMinus || keyData == Keys.Subtract)
            {
                button4_Click(null, null);
            }
            else if (keyData == (Keys.Subtract | Keys.Shift))
            {
                button2_Click(null, null);
            }
            else if (keyData == (Keys.Add | Keys.Shift))
            {
                button1_Click(null, null);
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
