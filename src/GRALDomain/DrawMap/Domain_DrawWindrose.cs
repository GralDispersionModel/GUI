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

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw Wind Roses
        /// </summary>
        private void DrawWindRose(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north,
                                  double factor_x, double factor_y, Font LabelFont, Brush LabelBrush)
        {
            Point[] windrosepoints = new Point[49];
            int diameter = 	Convert.ToInt32(Math.Max(1, Math.Min(2000, _drobj.ContourLabelDist * factor_x )));
            if (diameter > 14)
            {
                try
                {
                    int transparency = _drobj.Transparancy;
                    bool drawFrame = _drobj.Fill;

                    Pen PenGray = new Pen(Color.Gray, 1);

                    Pen[] windpen = new Pen[8];
                    Brush blackbrush = new SolidBrush(Color.Black); windpen[0] = new Pen(Color.Blue);
                    windpen[1] = new Pen(Color.LightSkyBlue);	windpen[2] = new Pen(Color.Green);
                    windpen[3] = new Pen(Color.YellowGreen);	windpen[4] = new Pen(Color.Yellow);
                    windpen[5] = new Pen(Color.Orange);			windpen[6] = new Pen(Color.Red);
                    windpen[7] = new Pen(Color.Brown); 
                    
                    Brush[] windbrush = new Brush[8];
                    if (_drobj.Filter) // colors for wind velocity
                    {
                        windbrush[0] = new SolidBrush(Color2Transparent(transparency, Color.Blue));	windbrush[1] = new SolidBrush(Color2Transparent(transparency, Color.LightSkyBlue));
                        windbrush[2] = new SolidBrush(Color2Transparent(transparency,Color.Green));	windbrush[3] = new SolidBrush(Color2Transparent(transparency,Color.YellowGreen));
                        windbrush[4] = new SolidBrush(Color2Transparent(transparency,Color.Yellow)); windbrush[5] = new SolidBrush(Color2Transparent(transparency,Color.Orange));
                        windbrush[6] = new SolidBrush(Color2Transparent(transparency,Color.Red));	windbrush[7] = new SolidBrush(Color2Transparent(transparency,Color.Brown));
                    }
                    else // colors for stability classes
                    {
                        windbrush[0] = new SolidBrush(Color2Transparent(transparency, Color.Brown));	windbrush[1] = new SolidBrush(Color2Transparent(transparency, Color.DarkRed));
                        windbrush[2] = new SolidBrush(Color2Transparent(transparency,Color.Red));	windbrush[3] = new SolidBrush(Color2Transparent(transparency,Color.Orange));
                        windbrush[4] = new SolidBrush(Color2Transparent(transparency,Color.YellowGreen)); windbrush[5] = new SolidBrush(Color2Transparent(transparency,Color.Green));
                        windbrush[6] = new SolidBrush(Color2Transparent(transparency,Color.LightBlue));	windbrush[7] = new SolidBrush(Color2Transparent(transparency,Color.Blue));
                    }
                    bool draw_label = (_drobj.Label == 2 || _drobj.Label == 3) && LabelFont.Height > 2;
                    string[] LabelNames = new string[1];
                    if (draw_label)
                    {
                        string[] w = _drobj.ContourFilename.Split(new char[] { '\t' });
                        LabelNames = new string[w.Length];
                        int count = 0;
                        foreach(string _wnd in w)
                        {
                            string[] _name = _wnd.Split(new char[] { '?' }); // get 1st entry = filename
                            LabelNames[count] = System.IO.Path.GetFileName(_name[0]);
                            count++;
                        }
                    }
                    
                    int _count = 0;
                    const double sectorangle = Math.PI / 8;
                    double sectorwidth = 0.196;
                    if (_drobj.FillYesNo)
                    {
                        sectorwidth = Math.PI / 18;
                    }

                    foreach (List<PointF> _ptList in _drobj.ContourPoints)
                    {
                        if (_ptList.Count > 64)
                        {
                            PointF Pt0 = _ptList[0];
                            int x0 = (int)((Pt0.X - form1_west) * factor_x) + TransformX;
                            int y0 = (int)((Pt0.Y - form1_north) * factor_y) + TransformY;
                            
                            double[] sektsum = new double[16];
                            double sektmax = 0;
                            
                            for (int i = 0; i < 16; i++)
                            {
                                for (int n = 0; n < 7; n += 2)
                                {
                                    sektsum [i] += _ptList[1 + i * 4 + (int) (n / 2)].X;
                                    sektsum [i] += _ptList[1 + i * 4 + (int) (n / 2)].Y;
                                }
                                sektmax = Math.Max(sektsum[i], sektmax);
                            }
                            
                            if (_drobj.ContourAreaMin > 0.05) // fixed scale
                            {
                                sektmax = _drobj.ContourAreaMin / 100D;
                            }
                            double scale = diameter / sektmax;

                            for (int n = 7; n >= 0; n--)
                            {
                                for (int i = 0; i < 16; i++)
                                {
                                    int radius = Convert.ToInt32(sektsum[i] * scale);
#if __MonoCS__
                                    float startAngle = (float)((sectorangle * i - sectorwidth) * 180 / Math.PI - 90);
                                    float sectorAnglePie = (float)((sectorwidth * 2) * 180 / Math.PI);
#else
                                    float startAngle = (float)(sectorangle * i - sectorwidth) * 180 / MathF.PI - 90;
                                    float sectorAnglePie = (float)(sectorwidth * 2) * 180 / MathF.PI;
#endif
                                    if (n % 2 == 0)
                                    {
                                        sektsum[i] = sektsum[i] - _ptList[1 + i * 4 + (int)(n / 2)].X;
                                    }
                                    else
                                    {
                                        sektsum[i] = sektsum[i] - _ptList[1 + i * 4 + (int)(n / 2)].Y;
                                    }

                                    if (radius > 0)
                                    {
                                        if (n >= 0 && n < windbrush.Length)
                                        {
                                            g.FillPie(windbrush[n], new Rectangle(x0 - radius, y0 - radius, radius * 2, radius * 2), startAngle, sectorAnglePie);
                                        }
                                        if (n >= 0 && n < windpen.Length)
                                        {
                                            if (drawFrame)
                                            {
                                                g.DrawPie(PenGray, new Rectangle(y0 - radius, y0- radius, radius * 2, radius * 2), startAngle, sectorAnglePie);
                                            }
                                            else
                                            {
                                                g.DrawPie(windpen[n], new Rectangle(x0- radius, y0 - radius, radius * 2, radius * 2), startAngle, sectorAnglePie);
                                            }
                                        }
                                    }
                                }
                            }

                            //draw axis of windrose
                            double div = Math.Round(sektmax * scale / 4, 0);
                            Pen pcircle = new Pen(Color.Black, 1);
                            try
                            {
                                g.DrawLine(pcircle, Convert.ToInt32(x0 - div * 5), y0, Convert.ToInt32(x0 + div * 5), y0);
                                g.DrawLine(pcircle, x0, Convert.ToInt32(y0 - div* 5), x0, Convert.ToInt32(y0 + div * 5));
                            }
                            catch{}
                            //base.OnPaint(e);

                            //draw circles
                            pcircle.DashStyle = DashStyle.Dot;
                            for (int i = 1; i < 5; i++)
                            {
                                try
                                {
                                    int _x1 = Convert.ToInt32(div * i);
                                    g.DrawEllipse(pcircle, x0 - _x1, y0 - _x1, 2 * _x1, 2 * _x1);
                                    g.DrawString(Convert.ToString(Math.Round(i * 0.25 * sektmax * 100, 0)) + "%", LabelFont, LabelBrush, x0 + _x1, y0 + 5);
                                }
                                catch{}
                                //base.OnPaint(e);
                            }
                            pcircle.Dispose();
                            
                            if (draw_label)
                            {
                                if (_count < LabelNames.Length)
                                {
                                    g.DrawString(LabelNames[_count], LabelFont, LabelBrush, x0, Convert.ToInt32(y0 - div* 5));
                                }
                            }
                        }
                        _count++;
                    }
                    
                    for (int i = 0; i < 8; i++)
                    {
                        windbrush[i].Dispose();
                        windpen[i].Dispose();
                    }
                    PenGray.Dispose();
                    blackbrush.Dispose();
                }
                catch{}
            }
        }
    }
}
