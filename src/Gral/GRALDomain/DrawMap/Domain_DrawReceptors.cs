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
 * Time: 11:29
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using GralItemData;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw Receptors
        /// </summary>
        private void DrawReceptors(Graphics g, DrawingObjects _drobj, double form1_west, double form1_north,
                                   double factor_x, double factor_y, Font LabelFont, Brush LabelBrush)
        {
            int pb1_height = picturebox1.Height;
            int pb1_width  = picturebox1.Width;

            StringFormat StringFormat2 = new StringFormat
            {
                LineAlignment = StringAlignment.Far,
                Alignment = StringAlignment.Center
            }; //format for names of line sources


            int transparency = _drobj.Transparancy;
            int linewidth_l = _drobj.LineWidth;
            bool FillYesNo = _drobj.FillYesNo;

            string[] text1 = new string[28];
            int n = -1;
            
            bool draw_label = (_drobj.Label == 2 || _drobj.Label == 3) && LabelFont.Height > 2;
            
            Pen penrec;
            if (_drobj.LineColors[0] == Color.Transparent || _drobj.LineColor == Color.Transparent)
            {
                penrec = new Pen (Color.Red);
                _drobj.LineColor = Color.Red;
            }
            else
                penrec = new Pen(_drobj.LineColors[0]);
            
            int width = 4;
            if (_drobj.LineWidth <= 1)
                if (_drobj.LineWidth == 0)
                    width = 4;
                else
                    width = Math.Max (1, Math.Min(200, Convert.ToInt32(1 / BmpScale / MapSize.SizeX))); // 4 m lenght
                else
                    width = Math.Max (1, Math.Min(200, Convert.ToInt32((double) _drobj.LineWidth / 8 * factor_x)));
            
            //width = 3;
            penrec.Width = width;
            width *= 2;

            Pen p1 = new Pen(Color.Green, 2)
            {
                DashStyle = DashStyle.Dot
            };

            ReceptorData _rd;
            
            for (int ii = 0; ii < EditR.ItemData.Count; ii++)
            {
                _rd = EditR.ItemData[ii];
                try
                {
                    n++;
                    
                    int x1 = (int) ((_rd.Pt.X - form1_west ) * factor_x + TransformX);
                    int y1 = (int) ((_rd.Pt.Y - form1_north) * factor_y + TransformY);
                   
                    if ((x1 < 0) || (y1 < 0) || (x1 > pb1_width) || (y1 > pb1_height))
                    {
                    }
                    else
                    {
                        //g.DrawImage(receptor, x1 - 10, y1 - 10, 20, 20);
                        g.DrawLine(penrec, x1, y1 - width * 2 +2 , x1, y1 + width * 2 -2);
                        g.DrawLine(penrec , x1 - width*2 + 2, y1, x1 + width * 2 - 2, y1);
                        
                        //draw filled circle, if display values are enabled
                        if (FillYesNo == true && text1.Length > 3)
                        {
                            //apply color scale
                            double display_value = Math.Abs(_rd.DisplayValue);
                            int index = -1;
                            for (int r = _drobj.FillColors.Count - 1; r > -1; r--)
                                if (display_value >= _drobj.ItemValues[r])
                            {
                                index = r;
                                break;
                            }
                            //values below the minimum are drawn completely transparant
                            int transparancy_new = transparency;
                            if (index==-1)
                            {
                                transparancy_new = 0;
                                index = 0;
                            }
                            
                            Brush br1 = new SolidBrush(Color2Transparent(transparancy_new, _drobj.FillColors[index]));
                            g.FillEllipse(br1, x1 - width * 2, y1 - width * 2, width * 4, width * 4);
                            br1.Dispose();
                            
                            g.DrawEllipse(penrec, x1 - width * 2, y1 - width * 2, width * 4, width * 4);
                            if (draw_label)
                            {
                                g.DrawString( _rd.Name.Trim(), LabelFont, LabelBrush, x1, y1 - width * 2, StringFormat2);
                            }
                            
                        }
                        else if (draw_label)
                        {
                            g.DrawString( _rd.Name.Trim(), LabelFont, LabelBrush, x1 + penrec.Width, y1 - penrec.Width - LabelFont.Height);
                        }
                        
                        if ((n == EditR.ItemDisplayNr) && ((MouseControl == 25) || (MouseControl == 24)))
                        {
                            Pen penmarked = new Pen(Color.Green)
                            {
                                Width = width / 2
                            };
                            g.DrawLine(penmarked, x1, y1 - width * 2 +2 , x1, y1 + width * 2 -2);
                            g.DrawLine(penmarked, x1 - width*2 + 2, y1, x1 + width * 2 - 2, y1);
                            penmarked.Dispose();
                            g.DrawRectangle(p1, x1 - width * 2 , y1 - width * 2 , width* 4, width * 4);
                        }
                        
                    }
                }
                catch{}
            } 
            p1.Dispose();
            penrec.Dispose();
            StringFormat2.Dispose();
        }
    }
}
