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

using GralMessage;
using GralStaticFunctions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Search contour lines using the Labek/Kuntner algorithm
        /// </summary>
        private void SimpleContour(double[,] zlevel, int nx, int ny, double x11, double y11, double dx,
                                    MessageWindow mess, DrawingObjects _drobj, bool[,] buildingsindex)
        {
            // Simple Contour line algorithm
            // Developed by K.Labek and M. Kuntner in the late 1990s
            // Improved in Dec. 2018
            if (_drobj.ContourPolygons != null)
            {
                _drobj.ContourPolygons.Clear();
                _drobj.ContourPolygons.TrimExcess();
            }
            else
            {
                _drobj.ContourPolygons = new List<GralData.ContourPolygon>();
            }
            Object _obj = new Object();

            nx++; // use additional (empty) right column

            Parallel.For(0, _drobj.ItemValues.Count, k =>
            //for (int k = 0; k < _drobj.ItemValues.Count; k++) // check all contour line values
            {
                GralData.DouglasPeucker _dp = new GralData.DouglasPeucker();
                byte[,] b = new byte[nx + 1, ny + 1];
                // Array.Clear(b, 0, b.Length); // clear the "Belegungs" array

                int ny_min = ny + 1;
                int ny_max = 0;
                int nx_min = nx + 1;
                int nx_max = 0;

                double contour_value = _drobj.ItemValues[k];

                // set belegung if building with 0 value exists -> remove contour lines within buildings
                for (int i = 0; i < nx - 1; i++)
                {
                    for (int j = 0; j < ny; j++)
                    {
                        if (buildingsindex[i, j])
                        {
                            b[i, j] = 1;
                        }
                    }
                }

                // search for lowest index from column 0, otherwise from column 2
                int startx = 0;
                if (k > 0)
                {
                    startx = 2;
                }

                for (int i = startx; i < nx - 2; i++)
                {
                    Application.DoEvents();
                    for (int j = 0; j < ny - 1; j++)
                    {
                        // found value higher than contour_value && no "Belegung"
                        if ((zlevel[i, j] >= contour_value) && (b[i, j] != 1))
                        {
                            List<PointD> pts = new List<PointD>();
                            // search along the contour line
                            // 1st: remember start values
                            int xz = i; int yz = j; // start coordinates
                            int x_start = xz;
                            int y_start = yz;
                            double w1 = zlevel[xz, yz]; // w1 is the value at the last "test" and is higher than contour_value
                            double w2 = 0;              // w2 ist the recent testet value and is lower than contour_value
                            double x_a, y_a;                // actual values for x and y
                            double x_a_s, y_a_s;            // start values for x and y
                            double x_a_l = 0;               // last values for x and y
                            double y_a_l = double.MaxValue; // last values for x and y

                            //int qb = 2; // initial search direction = up
                            int q = 2;  // initial search direction
                            double ds = 0;  // interpolated step

                            if (xz > 0) // border protection
                            {
                                w2 = zlevel[xz - 1, yz];
                                ds = (w1 - contour_value) * dx / (w1 - w2); // interpolation
                            }
                            else
                                ds = 0; // at the border bottom left

                            x_a = x11 + xz * dx - ds;
                            x_a_s = x_a;
                            y_a = y11 + yz * dx;
                            y_a_s = y_a;
                            x_a_l = x_a; y_a_l = y_a;

                            pts.Add(new PointD(x_a_s, y_a_s)); //1st point

                            b[xz, yz] = 1; // set "Belegung" for this cell
                            ny_max = Math.Max(ny_max, yz);
                            nx_max = Math.Max(nx_max, xz);
                            ny_min = Math.Min(ny_min, yz);
                            nx_min = Math.Min(nx_min, xz);

                            // filter sinlge "islands"
                            double checkvalue = contour_value * 0.99;
                            if (xz > 0 && yz > 0 && zlevel[xz + 1, yz] > checkvalue && zlevel[xz - 1, yz] > checkvalue &&
                                zlevel[xz, yz + 1] > checkvalue && zlevel[xz, yz - 1] > checkvalue)
                            {; }
                            else
                            {
                                do
                                {
                                    switch (q)
                                    {
                                        case 1: // direction right
                                            if (xz < nx - 1) // border?
                                            {
                                                w2 = zlevel[xz + 1, yz];
                                                if (w2 < contour_value) // if lower
                                                {
                                                    ds = (w1 - contour_value) * dx / (w1 - w2); // interpolation
                                                    x_a = x11 + xz * dx + ds;
                                                    y_a = y11 + yz * dx;

                                                    if ((Math.Abs(x_a_s - x_a) > 0.5 || Math.Abs(y_a_s - y_a) > 0.5) &&
                                                        (Math.Abs(x_a_l - x_a) > 0.5 || Math.Abs(y_a_l - y_a) > 0.5)) // filter near points
                                                    {
                                                        pts.Add(new PointD(x_a, y_a));
                                                        x_a_l = x_a; y_a_l = y_a;
                                                    }
                                                    q = 4; // turn to right
                                                }
                                                else // if higher
                                                {
                                                    w1 = w2;
                                                    xz++;
                                                    b[xz, yz] = 1; // set "Belegung" for this cell
                                                    if (yz == ny)  // draw at the border
                                                    {
                                                        x_a = x11 + xz * dx;
                                                        y_a = y11 + yz * dx;
                                                        pts.Add(new PointD(x_a, y_a));
                                                        x_a_l = x_a; y_a_l = y_a;
                                                    }
                                                    q = 2; // turn to up
                                                }
                                            }
                                            else // at the border
                                                q = 4; // turn down

                                            break;

                                        case 2: // direction up
                                            if (yz < ny - 1) // border?
                                            {
                                                w2 = zlevel[xz, yz + 1];
                                                if (w2 < contour_value) // if lower
                                                {
                                                    ds = (w1 - contour_value) * dx / (w1 - w2); // interpolation
                                                    x_a = x11 + xz * dx;
                                                    y_a = y11 + yz * dx + ds;
                                                    if ((Math.Abs(x_a_s - x_a) > 0.5 || Math.Abs(y_a_s - y_a) > 0.5) &&
                                                        (Math.Abs(x_a_l - x_a) > 0.5 || Math.Abs(y_a_l - y_a) > 0.5)) // filter near points
                                                    {
                                                        pts.Add(new PointD(x_a, y_a));
                                                        x_a_l = x_a; y_a_l = y_a;
                                                    }
                                                    q = 1; // turn to right
                                                }
                                                else // if higher
                                                {
                                                    w1 = w2;
                                                    yz++;
                                                    b[xz, yz] = 1; // set "Belegung" for this cell
                                                    if (xz == 0)  // draw at the border
                                                    {
                                                        x_a = x11 + xz * dx;
                                                        y_a = y11 + yz * dx;
                                                        pts.Add(new PointD(x_a, y_a));
                                                        x_a_l = x_a; y_a_l = y_a;
                                                    }
                                                    q = 3; // turn to left
                                                }
                                            }
                                            else // at the border
                                                q = 1; // turn right
                                            break;

                                        case 3: // direction left
                                            if (xz > 0) // border?
                                            {
                                                w2 = zlevel[xz - 1, yz];
                                                if (w2 < contour_value) // if lower
                                                {
                                                    ds = (w1 - contour_value) * dx / (w1 - w2); // interpolation
                                                    x_a = x11 + xz * dx - ds;
                                                    y_a = y11 + yz * dx;
                                                    if ((Math.Abs(x_a_s - x_a) > 0.5 || Math.Abs(y_a_s - y_a) > 0.5) &&
                                                        (Math.Abs(x_a_l - x_a) > 0.5 || Math.Abs(y_a_l - y_a) > 0.5)) // filter near points
                                                    {
                                                        pts.Add(new PointD(x_a, y_a));
                                                        x_a_l = x_a; y_a_l = y_a;
                                                    }
                                                    q = 2; // turn to up
                                                }
                                                else // if higher
                                                {
                                                    w1 = w2;
                                                    xz--;
                                                    b[xz, yz] = 1; // set "Belegung" for this cell
                                                    if (yz == 0)  // draw at the border
                                                    {
                                                        x_a = x11 + xz * dx;
                                                        y_a = y11 + yz * dx;
                                                        pts.Add(new PointD(x_a, y_a));
                                                        x_a_l = x_a; y_a_l = y_a;
                                                    }
                                                    q = 4; // turn to down
                                                }
                                            }
                                            else // at the border
                                                q = 2; // turn up

                                            break;

                                        case 4: // direction down
                                            if (yz > 0) // border?
                                            {
                                                w2 = zlevel[xz, yz - 1];
                                                if (w2 < contour_value) // if lower
                                                {
                                                    ds = (w1 - contour_value) * dx / (w1 - w2); // interpolation
                                                    x_a = x11 + xz * dx;
                                                    y_a = y11 + yz * dx - ds;
                                                    if ((Math.Abs(x_a_s - x_a) > 0.5 || Math.Abs(y_a_s - y_a) > 0.5) &&
                                                        (Math.Abs(x_a_l - x_a) > 0.5 || Math.Abs(y_a_l - y_a) > 0.5)) // filter near points
                                                    {
                                                        pts.Add(new PointD(x_a, y_a));
                                                        x_a_l = x_a; y_a_l = y_a;
                                                    }
                                                    q = 3; // turn to left
                                                }
                                                else // if higher
                                                {
                                                    w1 = w2;
                                                    yz--;
                                                    b[xz, yz] = 1; // set "Belegung" for this cell
                                                    if (xz == nx)  // draw at the border
                                                    {
                                                        x_a = x11 + xz * dx;
                                                        y_a = y11 + yz * dx;
                                                        pts.Add(new PointD(x_a, y_a));
                                                        x_a_l = x_a; y_a_l = y_a;
                                                    }
                                                    q = 1; // turn to right
                                                }
                                            }
                                            else // at the border
                                                q = 3; // turn left

                                            break;
                                    }
                                    //MessageBox.Show(contour_value.ToString());
                                } while (xz != x_start || yz != y_start);
                            }

                            try
                            {
                                // Remove spikes
                                if (pts.Count > 3 && _drobj.RemoveSpikes > 1)
                                {
                                    double angle = 0;
                                    int checkAngle = _drobj.RemoveSpikes;
                                    for (int p = 0; p < pts.Count; p++)
                                    {
                                        if (p < pts.Count - 2)
                                        {
                                            angle = GetAngleBetweenLines(pts[p], pts[p + 1], pts[p + 1], pts[p + 2]);
                                        }
                                        else if (p == pts.Count - 2)
                                        {
                                            angle = GetAngleBetweenLines(pts[p], pts[p + 1], pts[p + 1], pts[0]);
                                        }
                                        else if (p == pts.Count - 1)
                                        {
                                            angle = GetAngleBetweenLines(pts[p], pts[0], pts[0], pts[1]);
                                        }

                                        if (angle < checkAngle)
                                        {
                                            if (p < pts.Count - 1)
                                            {
                                                pts.RemoveAt(p + 1);
                                            }
                                            else
                                            {
                                                pts.RemoveAt(0);
                                            }
                                            if (p > 0) // test same line again
                                            {
                                                p--;
                                            }
                                        }
                                        if (pts.Count < 3) // almost all points removed
                                        {
                                            p = pts.Count; // break
                                        }
                                    }
                                }
                            }
                            catch
                            {
                            }

                            if (pts.Count > 2)
                            {
                                //pts.Add(new PointD(x_a_s, y_a_s));     // first point

                                double polyArea = CalcOrientation(pts);

                                if (Math.Abs(polyArea) > _drobj.ContourAreaMin) // otherwise drop this contour line
                                {
                                    if (_drobj.ContourFilter > 0.01)   // filter edge points
                                    {
                                        _dp.DouglasPeuckerRun(pts, _drobj.ContourFilter);
                                    }

                                    int k_level = k;

                                    // check orientation clockwise && 1st point is not in the 1st column-> value inside is lower than the threshold
                                    if (polyArea < 0 && (pts[0].X > x11 + dx * 0.2))
                                    {
                                        {
                                            k_level = k - 1;
                                        }
                                    }

                                    GralData.ContourPolygon _ctp = new GralData.ContourPolygon
                                    {
                                        ItemValueIndex = k_level,
                                        EdgePoints = pts.ToArray(),
                                        DrawingOrder = 0
                                    };

                                    // increase Drawing order if column > 0, because in column 1 only the lowest contour_Value can be found 
                                    if (i > 0)
                                    {
                                        _ctp.DrawingOrder++;
                                    }

                                    lock (_obj)
                                    {
                                        _drobj.ContourPolygons.Add(_ctp);
                                    }
                                }
                            }
                            pts.Clear();
                            pts.TrimExcess();
                        }
                    }
                }
            });

            _drobj.ContourClipping = new RectangleF((float)(x11 + dx), (float)(y11 + (ny - 2) * dx), (float)((nx - 3) * dx), (float)((ny - 2) * dx));

            // analyze drawing order
            // 1st step: get min & max values of the contoue lines
            int[] drawingorder = new int[_drobj.ContourPolygons.Count];

            Parallel.For(0, _drobj.ContourPolygons.Count, i =>
            {
                PointD[] _polypoints = _drobj.ContourPolygons[i].EdgePoints;
                for (int pt = 0; pt < _polypoints.Length; pt++)
                {
                    if (_polypoints[pt].X > (x11 + 2 * dx) && _polypoints[pt].Y > (y11 + 2 * dx) &&
                        _polypoints[pt].X < (x11 + (nx - 2) * dx) && _polypoints[pt].Y < (y11 + (ny - 2) * dx))
                    {
                        if (_drobj.ContourPolygons[i].Bounds.East < _polypoints[pt].X)
                        {
                            _drobj.ContourPolygons[i].Bounds.East = _polypoints[pt].X;
                        }
                        if (_drobj.ContourPolygons[i].Bounds.West > _polypoints[pt].X)
                        {
                            _drobj.ContourPolygons[i].Bounds.West = _polypoints[pt].X;
                        }
                        if (_drobj.ContourPolygons[i].Bounds.North < _polypoints[pt].Y)
                        {
                            _drobj.ContourPolygons[i].Bounds.North = _polypoints[pt].Y;
                        }
                        if (_drobj.ContourPolygons[i].Bounds.South > _polypoints[pt].Y)
                        {
                            _drobj.ContourPolygons[i].Bounds.South = _polypoints[pt].Y;
                        }
                    }
                }
            });

            // 2nd step: find drawing order
            Parallel.For(0, _drobj.ContourPolygons.Count, i =>
            //for (int i = 0; i < _drobj.ContourPolygons.Count; i++) // check all polygons
            {
                PointD[] _polypoints = _drobj.ContourPolygons[i].EdgePoints;
                PointD _checkpoint = _polypoints[0];
                GralData.ContourPolygon ctp = _drobj.ContourPolygons[i];
                double x_l = x11 + 3 * dx;
                double x_r = x11 + (nx - 6) * dx;
                double y_u = y11 + 3 * dx;
                double y_o = y11 + (ny - 6) * dx;
                //MessageBox.Show(x_l.ToString() + "/" +x_r.ToString() + "/" +y_u.ToString() + "/" + y_o.ToString());
                for (int pt = 0; pt < _polypoints.Length; pt++) // find point inside domain not at the border
                {
                    _checkpoint = _polypoints[pt];
                    if (_checkpoint.X > x_l && _checkpoint.Y > y_u && _checkpoint.X < x_r && _checkpoint.Y < y_o)
                    {
                        break;
                    }
                }

                for (int j = 0; j < _drobj.ContourPolygons.Count; j++) // check all polygons
                {
                    if (i != j)
                    {
                        if (_drobj.ContourPolygons[i].Bounds.East < _drobj.ContourPolygons[j].Bounds.West ||
                            _drobj.ContourPolygons[i].Bounds.West > _drobj.ContourPolygons[j].Bounds.East ||
                            _drobj.ContourPolygons[i].Bounds.North < _drobj.ContourPolygons[j].Bounds.South ||
                            _drobj.ContourPolygons[i].Bounds.South > _drobj.ContourPolygons[j].Bounds.North)
                        {; } // i is outside of j 
                        else
                        {
                            GralData.ContourPolygon ctpcomp = _drobj.ContourPolygons[j];
                            PointD[] _pts = ctpcomp.EdgePoints;
                            if (St_F.PointInPolygonArray(_checkpoint, _pts))
                            {
                                _drobj.ContourPolygons[i].DrawingOrder++;
                            }
                        }
                    }
                }
            });

            _drobj.ContourPolygons.Sort();
            _obj = null;
        }
        
        /// <summary>
    	/// Compute the inner angle of two lines
    	/// </summary>
    	/// <param name="polypoints">List for polygon points</param> 
        private double GetAngleBetweenLines(PointD p11, PointD p12, PointD p21, PointD p22)
        {
            double sin = (p11.X - p12.X) * (p21.Y - p22.Y) - (p21.X - p22.X) * (p11.Y - p12.Y);
            double cos = (p11.X - p12.X) * (p21.X - p22.X) + (p11.Y - p12.Y) * (p21.Y - p22.Y);

            double angle = 180 + Math.Atan2(sin, cos) * (180 / Math.PI);

            if (angle > 180)
            {
                angle = 360 - angle;
            }
            return angle;
        }

        /// <summary>
    	/// Compute the area and the orientation of a polygon
    	/// </summary>
    	/// <param name="polypoints">List for polygon points</param> 
		private double CalcOrientation(List<GralDomain.PointD> polypoints)
        {
            double area = 0;
            if (polypoints.Count > 2)
            {
                polypoints.Add(polypoints[0]);
                for (int i = 0; i < polypoints.Count - 1; i++)
                {
                    area += (Convert.ToDouble(polypoints[i + 1].X) - Convert.ToDouble(polypoints[i].X)) * Convert.ToDouble(polypoints[i].Y) +
                        (Convert.ToDouble(polypoints[i + 1].X) - Convert.ToDouble(polypoints[i].X)) * (Convert.ToDouble(polypoints[i + 1].Y) - Convert.ToDouble(polypoints[i].Y)) / 2;
                }
                polypoints.RemoveAt(polypoints.Count - 1);
                area = Math.Round(area, 1);
            }
            //else
            //MessageBox.Show(this, "Number of vertices is below 3\t\nNo area could be computed");
            if (double.IsNaN(area))
            {
                area = 10E9;
            }
            else if (double.IsNegativeInfinity(area))
            {
                area = -10E9;
            }
            else if (double.IsPositiveInfinity(area))
            {
                area = 10E9;
            }
            return area;
        }
    }
}
