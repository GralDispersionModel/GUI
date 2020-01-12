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
 * User: Markus
 * Date: 16.01.2019
 * Time: 18:43
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
        /// Draw the Scale Bar
        /// </summary>
		private void DrawScaleBar(Graphics g, DrawingObjects _drobj, double _bmppbx_save, Font LabelFont, Brush LabelBrush)
		{
			Pen raster = new Pen(_drobj.LineColors[0], 1);
			int pb1_height = picturebox1.Height;
			int pb1_width  = picturebox1.Width;
			int linelength = Convert.ToInt32(Math.Min(32000, _drobj.ContourLabelDist / MapSize.SizeX / BmpScale));

            try
            {
                if (linelength > 10)
                {
                    int mapscalex = MapScale.X;
                    int mapscaley = MapScale.Y;

                    int xpos1 = Convert.ToInt32(Math.Min(32000, Convert.ToDouble(mapscalex / _bmppbx_save - Convert.ToInt32(linelength * 0.5))));
                    int ypos1 = Convert.ToInt32(Math.Min(32000, Convert.ToDouble(mapscaley) / _bmppbx_save));
                    int xpos2 = Convert.ToInt32(Math.Min(32000, Convert.ToDouble(mapscalex / _bmppbx_save + Convert.ToInt32(linelength * 0.5))));
                    int ypos2 = Convert.ToInt32(Math.Min(32000, Convert.ToDouble(mapscaley + 10) / _bmppbx_save));
                    g.DrawLine(new Pen(Color2Transparent(_drobj.Transparancy, _drobj.LineColors[0]), Convert.ToInt32(Math.Min(32000, 4 / _bmppbx_save))), xpos1, ypos1, xpos2, ypos1);
                    ypos1 = Convert.ToInt32(Convert.ToDouble(mapscaley - 10) / _bmppbx_save);
                    g.DrawLine(new Pen(Color2Transparent(_drobj.Transparancy, _drobj.LineColors[0]), Convert.ToInt32(Math.Min(32000, 4 / _bmppbx_save))), xpos1, ypos2, xpos1, ypos1);
                    g.DrawLine(new Pen(Color2Transparent(_drobj.Transparancy, _drobj.LineColors[0]), Convert.ToInt32(Math.Min(32000, 4 / _bmppbx_save))), xpos2, ypos2, xpos2, ypos1);
                    xpos1 = Convert.ToInt32(Math.Min(32000, Convert.ToDouble((mapscalex - 10) / _bmppbx_save))) - Convert.ToInt32(linelength * 0.5);
                    ypos1 = Convert.ToInt32(Math.Min(32000, Convert.ToDouble(mapscaley + 20) / _bmppbx_save));

                    g.DrawString("0 m", LabelFont, LabelBrush, xpos1, ypos1);
                    xpos1 = Convert.ToInt32(Math.Min(32000, Convert.ToDouble(mapscalex / _bmppbx_save + Convert.ToInt32(linelength * 0.5) - 15)));

                    if (_drobj.ContourLabelDist > 5000)
                    {
                        g.DrawString(Convert.ToString(_drobj.ContourLabelDist / 1000) + " km", LabelFont, LabelBrush, xpos1, ypos1);
                    }
                    else
                    {
                        g.DrawString(Convert.ToString(_drobj.ContourLabelDist) + " m", LabelFont, LabelBrush, xpos1, ypos1);
                    }
                    for (int i = 1; i < MapScale.Division; i++)
                    {
                        xpos1 = Convert.ToInt32(Math.Min(32000, Convert.ToDouble(mapscalex / _bmppbx_save - Convert.ToInt32(linelength * 0.5 - i * linelength / MapScale.Division))));
                        ypos1 = Convert.ToInt32(Math.Min(32000, Convert.ToDouble(mapscaley + 7) / _bmppbx_save));
                        ypos2 = Convert.ToInt32(Math.Min(32000, Convert.ToDouble(mapscaley - 7) / _bmppbx_save));
                        Pen pen1 = new Pen(Color2Transparent(_drobj.Transparancy, _drobj.LineColors[0]), Convert.ToInt32(Math.Min(32000, 2 / _bmppbx_save)));
                        g.DrawLine(pen1, xpos1, ypos1, xpos1, ypos2);
                        pen1.Dispose();
                    }
                }
            }
            catch { }
		}
	}
}