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

namespace GralStaticFunctions
{
    /// <summary>
    /// Class for the bresenham algorithm
    /// </summary>
    public static class Bresenham
    {
        /// <summary>
        /// Return raster points along a line between two points x0/y0 and x1/y1
        /// </summary>
        ///<returns>Yields a new Point or break</returns>  
        public static IEnumerable<Point> GetNewPoint(int x0, int y0, int x1, int y1)
        {
            int x, y, t, dx, dy, incx, incy, pdx, pdy, ddx, ddy, es, el, err;
            // difference
            dx = x1 - x0;
            dy = y1 - y0;

            if (dx == 0 & dy == 0)
            {
                yield return new Point(x0, y0);
            }
            else
            {
                // sign of the increment
                incx = Math.Sign(dx);
                incy = Math.Sign(dy);
                if (dx < 0)
                {
                    dx = -dx;
                }

                if (dy < 0)
                {
                    dy = -dy;
                }

                // where is the highest distance?
                if (dx > dy)
                {
                    // x the faster direction
                    pdx = incx; pdy = 0;    // pd. is the parellel step
                    ddx = incx; ddy = incy; // dd. is the diagonal step
                    es = dy; el = dx;   // error steps
                }
                else
                {
                    // y is the faster direction
                    pdx = 0; pdy = incy;
                    ddx = incx; ddy = incy;
                    es = dx; el = dy;
                }

                // initialisation of the loop
                x = x0;
                y = y0;
                err = el / 2;

                yield return new Point(x, y);

                //SetPixel(x,y); // starting point

                // step through the raster
                for (t = 0; t < el; ++t) // t counts the steps
                {
                    // actualization of the error term
                    err -= es;
                    if (err < 0)
                    {
                        // error term must be positive
                        err += el;
                        // next step in slower direction
                        x += ddx;
                        y += ddy;
                    }
                    else
                    {
                        // Step in faster direction
                        x += pdx;
                        y += pdy;
                    }

                    yield return new Point(x, y);

                }
            }

            yield break;
        }

    }
}
