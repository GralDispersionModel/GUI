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
 * Time: 10:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw Shape Maps
        /// </summary>
        private void DrawShapeMap(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north, double factor_x, double factor_y)
        {
            List <PointF> _shpPoint =  _drobj.ShpPoints;
            foreach (PointF _pt in _shpPoint)
            {
                int x = (int)((_pt.X - form1_west) * factor_x + TransformX);
                int y = (int)((_pt.Y - form1_north) * factor_y + TransformY);
                g.DrawEllipse(Pens.Black, x, y, 4, 4);
            }
            
            if (_drobj.LineWidth > 0)
            {
                double west = _drobj.Item;
                double north = _drobj.SourceGroup;
                if (west == -1 && north == -1)
                {
                    west = 0; north = 0;
                }
                Pen myPen = new Pen(Color2Transparent(_drobj.Transparancy, _drobj.LineColors[0]), _drobj.LineWidth);
                
                List <GralShape.SHPLine> _shpLine = _drobj.ShpLines;
                foreach (GralShape.SHPLine _line in _shpLine)
                {
                    try
                    {
                        if (_line.Points.Length > 1)
                        {
                            PointF[] pt = new PointF[_line.Points.Length];
                            for (int j = 0; j < pt.Length; j++)
                            {
                                pt[j].X = (float)((_line.Points[j].X - form1_west + west) * factor_x + TransformX);
                                pt[j].Y = (float)((_line.Points[j].Y - form1_north + north) * factor_y + TransformY);
                            }
                            g.DrawLines(myPen, pt);
                        }

                        if (CancelDrawing)
                        {
                            break;
                        }
                    }
                    catch { }
                }
                myPen.Dispose(); // Kuntner dispose Pen
            }
            
            List <GralShape.SHPPolygon> _shppoly = _drobj.ShpPolygons;
            Brush mybrush = new SolidBrush(Color2Transparent(_drobj.Transparancy, _drobj.FillColors[0]));
            foreach (GralShape.SHPPolygon _poly in _shppoly)
            {
                if (_poly.Points.Length > 2)
                {
                    double west = _drobj.Item;
                    double north = _drobj.SourceGroup;
                    if (west == -1 && north == -1)
                    {
                        west = 0; north = 0;
                    }
                    PointF[] pt = new PointF[_poly.Points.Length];
                    float _west = (float)(form1_west + west);
                    float _north = (float)(form1_north + north);

                    for (int j = 0; j < pt.Length; j++)
                    {
                        PointF _pt = _poly.Points[j].ToPointF();
                        pt[j].X = (float)((_pt.X - _west) * factor_x + TransformX);
                        pt[j].Y = (float)((_pt.Y - _north) * factor_y + TransformY);
                    }
                    if (_drobj.FillYesNo == true)
                    {
                        g.FillPolygon(mybrush, pt);
                    }
                    if (_drobj.LineWidth > 0)
                    {
                        g.DrawPolygon(Pens.Black, pt);
                    }
                }
                if (CancelDrawing)
                {
                    break;
                }
            }
            mybrush.Dispose();
        }
    }
}
