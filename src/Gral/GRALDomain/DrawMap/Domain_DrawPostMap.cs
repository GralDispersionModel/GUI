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
 * User: Markus Kuntner
 * Date: 16.01.2019
 * Time: 11:02
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
        /// Draw Post Maps
        /// </summary>
        private void DrawPostMap(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north,
                                 double factor_x, double factor_y)
        {
            int pb1_height = picturebox1.Height;
			int pb1_width  = picturebox1.Width;
			
            int minx;
            int maxx;
            int miny;
            int maxy;
            int indexx;
            int indexy;

            try
            {
                double x0 = _drobj.ContourGeometry.X0;
                double y0 = _drobj.ContourGeometry.Y0;
                double dx = _drobj.ContourGeometry.DX;
                int nx = _drobj.ContourGeometry.NX;
                int ny = _drobj.ContourGeometry.NY;

                //fill contour map
                if (_drobj.FillYesNo == true)
                {
                    //when pixel size is larger than contour map resolution it is faster to loop over the pixels
                    if (BmpScale * MapSize.SizeX > dx)
                    {
                        minx = (int)((x0 - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                        maxx = (int)((x0 + dx * nx - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                        miny = (int)((y0 + dx * ny - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                        maxy = (int)((y0 - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                        double factorx = MapSize.SizeX * BmpScale;
                        double factory = MapSize.SizeY * BmpScale;
                        
                        Bitmap _bm = _drobj.BM;
                        for (int i = minx + 1; i < maxx; i++)
                        {
                            for (int j = miny + 1; j < maxy; j++)
                            {
                                if ((i >= 0) && (i <= pb1_width) && (j >= 0) && (j <= pb1_height))
                                {
                                    indexx = (int)(((i - minx) * factor_x) / dx);
                                    indexy = (int)((-(maxy - j) * factor_y) / dx);
                                    if (_drobj.ContourColor[indexx, indexy] > -1)
                                    {
                                        try
                                        {
                                            _bm.SetPixel(0, 0, Color2Transparent(_drobj.Transparancy, _drobj.FillColors[_drobj.ContourColor[indexx, indexy]]));
                                            g.DrawImageUnscaled(_bm, i, j);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                            }
                            if (CancelDrawing)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        int recwidth = (int)(dx / BmpScale / MapSize.SizeX);
                        //some parameters needed to improve filling the contours due to round-off errors
                        int ixold = (int)((x0 - form1_west) / BmpScale / MapSize.SizeX) + TransformX;
                        int addonex = 0;
                        int iyold = (int)((y0 - form1_north) / BmpScale / MapSize.SizeY) + TransformY;
                        int addoney = 0;

                        for (int i = 0; i < nx; i++)
                        {
                            int ix = (int)((x0 + dx * i - form1_west) * factor_x) + TransformX;
                            addonex = 0;
                            if (ix > ixold + recwidth)
                            {
                                ix = ix - 1;
                                addonex = 1;
                            }
                            else if (ix < ixold + recwidth)
                            {
                                ix = ix + 1;
                                addonex = -1;
                            }
                            ixold = ix + addonex;
                            for (int j = 0; j < ny; j++)
                            {
                                int iy = Convert.ToInt32((y0 + dx * j - form1_north) * factor_y) + TransformY;
                                addoney = 0;
                                if (iy + recwidth > iyold)
                                {
                                    addoney = -1;
                                }
                                else if (iy + recwidth < iyold)
                                {
                                    addoney = 1;
                                }
                                iyold = iy;
                                if ((ix >= 0) && (ix <= pb1_width) && (iy >= 0) && (iy <= pb1_height) && (_drobj.ContourColor[i, j] > -1))
                                {
                                    Brush br1 = new SolidBrush(Color2Transparent(_drobj.Transparancy, _drobj.FillColors[_drobj.ContourColor[i, j]]));
                                    g.FillRectangle(br1, ix, iy - recwidth, recwidth + addonex, recwidth + addoney);
                                    br1.Dispose();
                                }
                            }
                            if (CancelDrawing)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}