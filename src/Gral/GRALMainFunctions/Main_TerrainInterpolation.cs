#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2020]  [Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gral
{
    /// <summary>
    /// Terrain interpolation using the 3 closest nodes and a nearest neighbour inverse distance weighted interpolation
    /// </summary>
    public class TerrainInterpolation
    {
        /// <summary>
        /// Zero-bound Terrain data without NoData values
        /// </summary>
        private readonly float[][] Terrain;
        /// <summary>
        /// Left edge of the terrain grid
        /// </summary>
        private readonly double X0Terr;
        /// <summary>
        /// Top edge of the terrain grid
        /// </summary>
        private readonly double Y0Terr;
        /// <summary>
        /// Delta x of the terrain grid
        /// </summary>
        private readonly double DxTerr;
        /// <summary>
        /// Delta y of the terrain grid
        /// </summary>
        private readonly double DyTerr;
        /// <summary>
        /// Number of terrain grid cells in x direction
        /// </summary>
        private readonly int TerrainMaxX;
        /// <summary>
        /// Number of terrain grid cells in y direction
        /// </summary>
        private readonly int TerrainMaxY;

        /// <summary>
        /// Init a new Terrain Interpolation instance
        /// </summary>
        /// <param name="TerrainData">Zero bound Terrain data without NoData values</param>
        /// <param name="X0">Left edge of the terrain grid</param>
        /// <param name="Y0">Top edge of the terrain grid</param>
        /// <param name="Dx">Delta x of the terrain grid</param>
        /// <param name="Dy">Delta y of the terrain grid</param>
        public TerrainInterpolation(float[][] TerrainData, double X0, double Y0, double Dx, double Dy)
        {
            Terrain = TerrainData;
            X0Terr = X0;
            Y0Terr = Y0;
            DxTerr = Dx;
            DyTerr = Dy;
            TerrainMaxX = TerrainData.Length - 1;
            if (TerrainMaxX > 0)
            {
                TerrainMaxY = TerrainData[0].Length - 1;
            }
        }

        /// <summary>
        /// Height Interpolation using the 4 edge nodes and a nearest neighbour inverse distance weighted interpolation
        /// </summary>
        /// <param name="Xs">X position</param>
        /// <param name="Ys">Y position</param>
        /// <returns>Interpolated height</returns>
        public double InterpolationBorder(double Xs, double Ys)
        {
            // Check indices
            if (TerrainMaxX < 0 || TerrainMaxY < 0)
            {
                return 0;
            }

            // index of (Xs/Ys) within Terrain array (starting with 0)
            System.Drawing.Point cellPosition = new System.Drawing.Point(
                                       (int)Math.Max(1, Math.Min(TerrainMaxX, ((Xs - X0Terr) / DxTerr))),
                                       (int)Math.Max(1, Math.Min(TerrainMaxY, ((Y0Terr - Ys) / DyTerr))));
            (double _xLeft, double _yLeft) = TerrainCellLeft(cellPosition);

            double sumdenom = 0;
            double sumnom = 0;

            if (Math.Abs(Xs - _xLeft) < 0.5 && Math.Abs(Ys - _yLeft) < 0.5)
            {
                return Math.Max(0, Terrain[cellPosition.X][cellPosition.Y]);
            }
            else
            {
                //store distance and height in one list
                List<(double, float)> distance = new List<(double, float)>();
                double pH = 0.5;
                for (int xi = cellPosition.X; xi < cellPosition.X + 2; xi++)
                {
                    for (int yi = cellPosition.Y - 1; yi <= cellPosition.Y; yi++)
                    {
                        if (xi >= 0 && yi >= 0 && xi <= TerrainMaxX && yi <= TerrainMaxY)
                        {
                            (_xLeft, _yLeft) = TerrainCellLeft(new System.Drawing.Point(xi, yi));

                            double weightedDistance = Math.Max(0.001, Math.Pow((Xs - _xLeft) * (Xs - _xLeft) + (Ys - _yLeft) * (Ys - _yLeft), pH));
                            sumdenom += Terrain[xi][yi] / weightedDistance;
                            sumnom += 1 / weightedDistance;
                        }
                    }
                }

                if (sumnom == 0)
                {
                    return Math.Max(0, Terrain[cellPosition.X][cellPosition.Y]);
                }
                else
                {
                    return Math.Max(0, sumdenom / sumnom);
                }
            }
        }

        /// <summary>
        /// Height Interpolation using the 3 closest nodes (cell centers) and a nearest neighbour inverse distance weighted interpolation
        /// </summary>
        /// <param name="Xs">X position</param>
        /// <param name="Ys">Y position</param>
        /// <returns>Interpolated height</returns>
        public double InterpolationCenter(double Xs, double Ys)
        {
            // Check indices
            if (TerrainMaxX < 0 || TerrainMaxY < 0)
            {
                return 0;
            }

            // index of (Xs/Ys) within Terrain array (starting with 0)
            System.Drawing.Point cellPosition = new System.Drawing.Point(
                                       (int)Math.Max(1, Math.Min(TerrainMaxX, ((Xs - X0Terr) / DxTerr))),
                                       (int)Math.Max(1, Math.Min(TerrainMaxY, ((Y0Terr - Ys) / DyTerr))));
            (double _xCenter, double _yCenter) = TerrainCellCenter(cellPosition);
            
            double sumdenom = 0;
            double sumnom = 0;

            if (Math.Abs(Xs - _xCenter) < 0.5 && Math.Abs(Ys - _yCenter) < 0.5)
            {
                return Math.Max(0, Terrain[cellPosition.X][cellPosition.Y]);
            }
            else
            {
                //store distance and height in one list
                List<(double, float)> distance = new List<(double, float)>();
                distance.Add((Math.Max(0.001, (Xs - _xCenter) * (Xs - _xCenter) + (Ys - _yCenter) * (Ys - _yCenter)), Terrain[cellPosition.X][cellPosition.Y]));

                //generate a list of test points, depending on the location of the desired point
                List<System.Drawing.Point> TestPoints = new List<System.Drawing.Point>();

                // search two nearest contigous cell centers
                if (Xs < _xCenter) // on the left
                {
                    if (Ys < _yCenter) // on the bottom
                    {
                        TestPoints.Add(new System.Drawing.Point(cellPosition.X - 1, cellPosition.Y));
                        TestPoints.Add(new System.Drawing.Point(cellPosition.X - 1, cellPosition.Y + 1));
                        TestPoints.Add(new System.Drawing.Point(cellPosition.X, cellPosition.Y + 1));
                    }
                    else // on the top
                    {
                        TestPoints.Add(new System.Drawing.Point(cellPosition.X - 1, cellPosition.Y));
                        TestPoints.Add(new System.Drawing.Point(cellPosition.X - 1, cellPosition.Y - 1));
                        TestPoints.Add(new System.Drawing.Point(cellPosition.X, cellPosition.Y - 1));
                    }
                }
                else // on the right
                {
                    if (Ys < _yCenter) // on the bottom
                    {
                        TestPoints.Add(new System.Drawing.Point(cellPosition.X, cellPosition.Y + 1));
                        TestPoints.Add(new System.Drawing.Point(cellPosition.X + 1, cellPosition.Y + 1));
                        TestPoints.Add(new System.Drawing.Point(cellPosition.X + 1, cellPosition.Y));
                    }
                    else // on the top
                    {
                        TestPoints.Add(new System.Drawing.Point(cellPosition.X, cellPosition.Y - 1));
                        TestPoints.Add(new System.Drawing.Point(cellPosition.X + 1, cellPosition.Y - 1));
                        TestPoints.Add(new System.Drawing.Point(cellPosition.X + 1, cellPosition.Y));
                    }
                }

                //check distance for each TestPoint
                foreach (System.Drawing.Point pt in TestPoints)
                {
                    //clamp index to valid array values
                    System.Drawing.Point indexTest = new System.Drawing.Point(Math.Max(0, Math.Min(TerrainMaxX, pt.X)),
                                                                         Math.Max(0, Math.Min(TerrainMaxY, pt.Y)));
                    double xt, yt;
                    (xt, yt) = TerrainCellCenter(indexTest);
                    distance.Add((Math.Max(0.001, (Xs - xt) * (Xs - xt) + (Ys - yt) * (Ys - yt)), Terrain[indexTest.X][indexTest.Y]));
                }

                //sort distance
                distance = distance.OrderBy(i => i.Item1).ToList();

                // use closest 3 nodes
                double pH = 0.5;
                for (int index = 0; index < 3; index++)
                {
                    double weightedDistance = Math.Pow(distance[index].Item1, pH);
                    sumdenom += distance[index].Item2 / weightedDistance;
                    sumnom += 1 / weightedDistance;
                }

                if (sumnom == 0)
                {
                    return Math.Max(0, Terrain[cellPosition.X][cellPosition.Y]);
                }
                else
                {
                    return Math.Max(0, sumdenom / sumnom);
                }
            }
        }

        /// <summary>
        /// Calculate the center cooridinates of a terrain grid cell
        /// </summary>
        /// <param name="Pt"></param>
        /// <returns>X and Y coordinate of a terrain cell</returns>
        private (double, double) TerrainCellCenter(System.Drawing.Point Pt)
        {
            return (X0Terr + Pt.X * DxTerr + DxTerr * 0.5, Y0Terr - Pt.Y * DyTerr - DyTerr * 0.5);
        }
        /// <summary>
        /// Calculate the left/upper cooridinates of a terrain grid cell
        /// </summary>
        /// <param name="Pt"></param>
        /// <returns>X and Y coordinate of a terrain cell</returns>
        private (double, double) TerrainCellLeft(System.Drawing.Point Pt)
        {
            return (X0Terr + Pt.X * DxTerr, Y0Terr - Pt.Y * DyTerr);
        }
    }
}
