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

using GralItemData;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw Point Sources
        /// </summary>
        private void DrawPointSources(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north,
                               double factor_x, double factor_y, Font LabelFont, Brush LabelBrush)
        {
            int pb1_height = picturebox1.Height;
            int pb1_width = picturebox1.Width;

            StringFormat StringFormat2 = new StringFormat
            {
                LineAlignment = StringAlignment.Far,
                Alignment = StringAlignment.Center
            };

            int n = -1;

            bool draw_label = (_drobj.Label == 2 || _drobj.Label == 3) && LabelFont.Height > 2;

            Pen penrec;
            if (_drobj.LineColors[0] == Color.Transparent || _drobj.LineColor == Color.Transparent)
            {
                penrec = new Pen(Color.Red);
                _drobj.LineColor = Color.Red;
            }
            else
            {
                penrec = new Pen(_drobj.LineColors[0]);
            }

            int width = 4;
            if (_drobj.LineWidth <= 1)
            {
                if (_drobj.LineWidth == 0)
                {
                    width = 4;
                }
                else
                {
                    width = Convert.ToInt32(Math.Max(1, Math.Min(200, 1.5 / BmpScale / MapSize.SizeX))); // 6 m diameter
                }
            }
            else
            {
                width = Math.Max(1, Math.Min(200, Convert.ToInt32((double)_drobj.LineWidth / 6 * factor_x)));
            }

            //width = 4;
            penrec.Width = width;
            width *= 2;

            Pen p1 = new Pen(Color.Green, 2)
            {
                DashStyle = DashStyle.Dot
            };

            PointSourceData _psdata;

            for (int ii = 0; ii < EditPS.ItemData.Count; ii++)
            {
                _psdata = EditPS.ItemData[ii];
                try
                {
                    n++;
                    float x1 = (float)((_psdata.Pt.X - form1_west) * factor_x + TransformX);
                    float y1 = (float)((_psdata.Pt.Y - form1_north) * factor_y + TransformY);

                    if ((x1 < 0) || (y1 < 0) || (x1 > pb1_width) || (y1 > pb1_height))
                    {
                    }
                    else
                    {
                        //g.DrawImage(pointsource, x1 - 10, y1 - 10, 20, 20);
                        g.DrawArc(penrec, x1 - width, y1 - width, width * 2, width * 2, 1, 360);

                        if ((n == EditPS.ItemDisplayNr) && ((MouseControl == MouseMode.PointSourceSel) || (MouseControl == MouseMode.PointSourcePos)))
                        {
                            Pen penmarked = new Pen(Color.Green)
                            {
                                Width = width / 2
                            };
                            g.DrawRectangle(p1, x1 - width - width / 4 - 2, y1 - width - width / 4 - 2, width * 2 + width / 2 + 4, width * 2 + width / 2 + 4);
                            g.DrawArc(penmarked, x1 - width, y1 - width, width * 2, width * 2, 1, 360);
                            penmarked.Dispose();
                        }
                        if (draw_label)
                        {
                            g.DrawString(_psdata.Name, LabelFont, LabelBrush, x1, y1 - width, StringFormat2);
                        }
                    }

                }
                catch { }
            }
            p1.Dispose();
            penrec.Dispose();
            StringFormat2.Dispose();
        }
    }
}