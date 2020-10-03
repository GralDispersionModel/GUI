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

/*
 * Created by SharpDevelop.
 * User: U0178969
 * Date: 05.02.2019
 * Time: 15:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw Color Scales
        /// </summary>
        private void DrawColorScales(Graphics g, float _bmppbx_save)
        {
            StringFormat format1 = new StringFormat()  //format for names of sources
            {
                LineAlignment = StringAlignment.Near,
                Alignment = StringAlignment.Center,
            };
            StringFormat string_Format = new StringFormat()
            {
                Alignment = StringAlignment.Far, 
                LineAlignment = StringAlignment.Center,  
            };
            Brush blackBrush = new SolidBrush(Color.Black);
            Brush whiteBrush = new SolidBrush(Color.White);
            Pen blackPen = new Pen(Color.Black, 1);

            for (int name = ItemOptions.Count - 1; name > -1; name--)
            {
                DrawingObjects _drobj = ItemOptions[name];
                
                if ((_drobj.ColorScale != "-999,-999,-999") && (_drobj.Show == true))
                {
                    try
                    {
                        Font font = new Font(_drobj.LabelFont.Name, _drobj.LabelFont.Size);
                        string[] dummy1 = _drobj.ColorScale.Split(new char[] { ',' });

                        int xpos1 = 100;
                        int ypos1 = 100;
                        float scale = 1;
                        if (dummy1.Length > 2)
                        {
                            scale = Math.Max(0.01F, Convert.ToSingle(dummy1[2].Replace(".", decsep)) / _bmppbx_save);
                            xpos1 = Convert.ToInt32(Math.Min(32000, Convert.ToDouble(dummy1[0]) / _bmppbx_save / scale));
                            ypos1 = Convert.ToInt32(Math.Min(32000, Convert.ToDouble(dummy1[1]) / _bmppbx_save / scale));
                        }

                        g.ScaleTransform(scale, scale);

                        int ydist1 = Math.Max(15, Convert.ToInt32(Math.Min(32000, 170 / _drobj.FillColors.Count)));
                        
                        int titleWidth = (int) g.MeasureString(_drobj.LegendTitle, font).Width;
                        int titleHeight = (int) g.MeasureString(_drobj.LegendTitle, font).Height;
                        int rectHeight = _drobj.FillColors.Count * ydist1 + titleHeight + 20;

                        SizeF size_of_string = new SizeF(0, 0);
                        
                        for (int h = 0; h < _drobj.FillColors.Count; h++)
                        {
                            string s = Convert.ToString(Math.Round(_drobj.ItemValues[h], _drobj.DecimalPlaces)) + "  " + _drobj.LegendUnit;
                            size_of_string = g.MeasureString(s, font);
                            titleWidth = (int) Math.Max(titleWidth, size_of_string.Width);
                        }
                        
                        int width_rect = titleWidth + 50;
                        
                        g.FillRectangle(whiteBrush, xpos1 + 2, ypos1 + 2, width_rect, rectHeight);
                        g.DrawRectangle(blackPen, xpos1 + 2, ypos1 + 2, width_rect, rectHeight);
                        g.DrawString(_drobj.LegendTitle, font, blackBrush, (int) (xpos1 + width_rect * 0.5), ypos1 + 5, format1);

                        ypos1 += titleHeight + 15;

                        for (int h = 0; h < _drobj.FillColors.Count; h++)
                        {
                            string s = Convert.ToString(Math.Round(_drobj.ItemValues[h], _drobj.DecimalPlaces)) + "  " + _drobj.LegendUnit;
                            g.DrawString(s, font, blackBrush, xpos1 + width_rect - 4, ypos1 + ydist1 * h, string_Format);
                        }
                        
                        width_rect = Math.Max(10, width_rect - titleWidth - 20);
                        
                        int transparency = _drobj.Transparancy;
                        if (_drobj.DrawSimpleContour == false)
                        {
                        	transparency = 255;
                        }
                        
                        for (int h = 0; h < _drobj.FillColors.Count; h++)
                        {
                            using (Brush br1 = new SolidBrush(Color2Transparent(transparency, _drobj.FillColors[h]))) // Fill Color
                            {
                                int x0 = xpos1 + 8;
                                int y0 = ypos1 + ydist1 * h;
                                
//                                if (_drobj.DrawSimpleContour)
//								  {
//                                	g.FillRectangle(br1, x0, y0, width_rect, (int) (ydist1 * (_drobj.FillColors.Count - h)/ skalierung1));
//                                }
//                                else
//                                {
                                	g.FillRectangle(br1, x0, y0, width_rect, ydist1);
//                                }
                                
                                if (h < _drobj.LineColors.Count) // Line Color
                                {
                                    using (Pen pen1 = new Pen(_drobj.LineColors[h], 3))
                                    {
                                        g.DrawLine(pen1, x0, y0, x0 + width_rect + 7, y0);
                                    }
                                }
                            }
                        }
                        font.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
            g.ResetTransform();
            blackBrush.Dispose();
            whiteBrush.Dispose();
            blackPen.Dispose();
            string_Format.Dispose();
            format1.Dispose();
        }
    }
}