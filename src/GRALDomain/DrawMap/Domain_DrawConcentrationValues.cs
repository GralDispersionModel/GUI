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

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw concentration values 
        /// </summary>
        private void DrawConcentrationValues(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north,
                               double factor_x, double factor_y, Font LabelFont, Brush LabelBrush)
        {
            int pb1_height = picturebox1.Height;
            int pb1_width  = picturebox1.Width;

            StringFormat StringFormat2 = new StringFormat
            {
                LineAlignment = StringAlignment.Far,
                Alignment = StringAlignment.Center
            }; 
            
            Pen penrec;
            if (_drobj.LineColors[0] == Color.Transparent || _drobj.LineColor == Color.Transparent)
            {
                penrec = new Pen (Color.Blue);
                _drobj.LineColor = Color.Blue;
            }
            else
            {
                penrec = new Pen(_drobj.LineColors[0]);
            }

            Brush whitebrush = new SolidBrush(Color.White);
            Pen framePen = new Pen(Color.Black);

            int width = 2;
            if (_drobj.LineWidth <= 1)
            {
                if (_drobj.LineWidth == 0)
                {
                    width = 2;
                }
                else
                {
                    width = Convert.ToInt32(Math.Max (1, Math.Min(200, 1 / BmpScale / MapSize.SizeX))); // 4 m lenght
                }
            }
            else
            {
                width = Convert.ToInt32(Math.Max (1, Math.Min(200, (double) _drobj.LineWidth / 8 * factor_x)));
            }

            //width = 3;
            penrec.Width = width;
            width *= 2;
            
            bool draw_label = (_drobj.Label == 2 || _drobj.Label == 3) && LabelFont.Height > 2;
            
            for (int i = 0; i < _drobj.ContourPolygons.Count; i++)  // loop over all concentration values
            {
                try
                {
                    if (_drobj.ContourPolygons[i].EdgePoints.Length > 1)
                    {
                        float x1 = (float)((_drobj.ContourPolygons[i].EdgePoints[0].X - form1_west) * factor_x + TransformX);
                        float y1 = (float)((_drobj.ContourPolygons[i].EdgePoints[0].Y - form1_north) * factor_y + TransformY);
                        if ((x1 < 0) || (y1 < 0) || (x1 > pb1_width) || (y1 > pb1_height))
                        {
                        }
                        else
                        {
                            string labelText = "";
                            if (_drobj.Label == 2)
                            {
                                labelText += _drobj.ContourFilename + "\n" + _drobj.LegendTitle + " ";
                            }

                            labelText += (Math.Round(_drobj.ContourPolygons[i].EdgePoints[1].X / 1E12, _drobj.DecimalPlaces)).ToString() +
                                " " + _drobj.LegendUnit;

                            g.DrawLine(penrec, x1 - width, y1 - width, x1 + width, y1 + width);
                            g.DrawLine(penrec, x1 + width, y1 - width, x1 - width, y1 + width);

                            SizeF stringSize = g.MeasureString(labelText, LabelFont);
                            RectangleF stringPos = new RectangleF(x1, y1 - width, stringSize.Width, stringSize.Height);
                            if (_drobj.FillYesNo)
                            {
                                g.FillRectangle(whitebrush, stringPos);
                                g.DrawRectangle(framePen, stringPos.Left, stringPos.Top, stringPos.Width, stringPos.Height);
                            }
                            g.DrawString(labelText, LabelFont, LabelBrush, stringPos, StringFormat2);
                        }
                    }
                }
                catch
                { }
            }
            framePen.Dispose();
            whitebrush.Dispose();
            penrec.Dispose();
        }
    }
}