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
 * Time: 18:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GralDomain
{
    public partial class Domain
	{
        /// <summary>
        /// Draw Raster
        /// </summary>
		private void DrawRaster(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north,
		                        double factor_x, double factor_y, Font LabelFont, Brush LabelBrush)
		{
			Pen raster = new Pen(_drobj.LineColors[0], 1);
            Brush fontCBrush = new SolidBrush(_drobj.LineColors[0]);
			int pb1_height = picturebox1.Height;
			int pb1_width  = picturebox1.Width;
			
			raster.DashStyle = DashStyle.Dot;
			double step =  Convert.ToDouble(_drobj.ContourLabelDist) * factor_x;
			Font fontc = new Font(_drobj.LabelFont.Name, _drobj.LabelFont.Size, _drobj.LabelFont.Style);
			SizeF stringSize = new SizeF();
			stringSize = g.MeasureString("1234567", fontc); // get screen dimensions of that font

			if (step > 20) // don´t show small grids
			{
				double xcoor0 = Math.Round((0-TransformX) * BmpScale * MapSize.SizeX + MapSize.West, 1, MidpointRounding.AwayFromZero);
				double ycoor0 = Math.Round((0-TransformY) * BmpScale * MapSize.SizeY + MapSize.North, 1, MidpointRounding.AwayFromZero);
				double x0 = Math.Floor(xcoor0 / Convert.ToDouble(_drobj.ContourLabelDist));
				x0 = x0 *  Convert.ToDouble(_drobj.ContourLabelDist);
				int  x0string = (int) x0;
				x0 = Convert.ToInt32((x0 - form1_west) * factor_x) + TransformX;
				int st = _drobj.ContourLabelDist;
				Matrix mat = new Matrix();
				
				try
				{
					for (double xD = x0; xD <= pb1_width; xD += step)
					{
						if (xD > 0)
						{
							int x = (int) (xD + 0.5);
							g.DrawLine(raster, x, 0, x, pb1_height);
							
							if (step > 100)
							{
								mat.Reset();
								mat.RotateAt(270F, new Point( (int) (x - stringSize.Height), (int) (pb1_height - 5)));
								g.Transform = mat;
								g.DrawString(x0string.ToString(), fontc, fontCBrush, (int) (x - stringSize.Height), pb1_height - 5);
								g.ResetTransform();
								
								x0string += st;
							}
						}
						else
							x0string += st;
					}

                    x0 = Math.Floor(ycoor0 / Convert.ToDouble(_drobj.ContourLabelDist));
					x0 = x0 *  Convert.ToDouble(_drobj.ContourLabelDist);
					int  y0string = (int) x0;
					x0 = Convert.ToInt32((x0 - form1_north) * factor_y) + TransformY;
					
					for (double yD = x0; yD <= pb1_height; yD += step)
					{
						if (yD > 0)
						{
							int y = (int) (yD + 0.5);
							g.DrawLine(raster, 0 , y, pb1_width, y);
							
							if (step > 100)
							{
								g.DrawString(y0string.ToString(), fontc, fontCBrush, 5, y - stringSize.Height);
								y0string -= st;
							}
						}
						else
							y0string -= st;
					}
				}
				catch{}
				
				mat.Dispose();
			}
            fontCBrush.Dispose();
			raster.Dispose();
			fontc.Dispose();				
		}
	}
}