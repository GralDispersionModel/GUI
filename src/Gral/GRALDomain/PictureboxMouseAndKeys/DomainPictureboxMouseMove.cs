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
 * Date: 06.01.2019
 * Time: 12:03
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using GralStaticFunctions;

namespace GralDomain
{
    public partial class Domain
	{
		/// <summary>
        /// Mouse Move Events on the picturebox
        /// </summary>
		public void Picturebox1_MouseMove(object sender, MouseEventArgs e)
		{
			double lenght=0;
			
			if ((MouseControl == 6) || (MouseControl == 8) || (MouseControl == 17) || (MouseControl == 10)
			    || (MouseControl == 15) || (MouseControl == 24) || (MouseControl == 3) || (MouseControl == 12) 
			    || (MouseControl == 75) || (MouseControl == 79))
				Activate();

			double x_real = (e.X-TransformX) * BmpScale * MapSize.SizeX + MapSize.West;
			double y_real = (e.Y-TransformY) * BmpScale * MapSize.SizeY + MapSize.North;
			
			textBox1.Text = x_real.ToString("F1");
			textBox2.Text = y_real.ToString("F1");
			toolStripTextBox1.Text = textBox1.Text;
			toolStripTextBox2.Text = textBox2.Text;
			
			if (CellHeightsType == 2) // Show GRAL height
			{
				try
				{
					double flowfieldraster = Convert.ToDouble(MainForm.numericUpDown10.Value);
					if (flowfieldraster > 0)
					{
						int i = (int) ((x_real - MainForm.GralDomRect.West)/flowfieldraster) + 1;
						int j = (int) ((y_real - MainForm.GralDomRect.South)/flowfieldraster) + 1;

                        if ((x_real - MainForm.GralDomRect.West) > 0 && (y_real - MainForm.GralDomRect.South) > 0 && i <= CellHeights.GetUpperBound(0) && j <= CellHeights.GetUpperBound(1))
                        {
                            string heightString = CellHeights[i, j].ToString("F1");
                            toolStripTextBox3.Text = heightString;
                            textBox3.Text = heightString;

                            // change cell height
                            if (MouseControl == 9999 && e.Button == MouseButtons.Left)
                            {
                                if (TopoModifyBlocked[i, j] == false)
                                {
                                    for (int x = i - TopoModify.Raster; x <= i + TopoModify.Raster; x++)
                                    {
                                        for (int y = j - TopoModify.Raster; y <= j + TopoModify.Raster; y++)
                                        {
                                            if (x > 0 && y > 0 && x < CellHeights.GetUpperBound(0) && y < CellHeights.GetUpperBound(1))
                                            {
                                                if (TopoModifyBlocked[x, y] == false)
                                                {
                                                    TopoModifyBlocked[x, y] = true;
                                                    // set new cell height
                                                    if (TopoModify.AbsoluteHeight == false)
                                                    {
                                                        float temp = CellHeights[x, y] + TopoModify.Height;
                                                        //Cell_Height[x, y] += Topo_Modify.height;
                                                        // limit cell height to max and min values
                                                        if (temp > TopoModify.Hmax)
                                                        {
                                                            temp = TopoModify.Hmax;
                                                            CellHeights[x, y] = Math.Max(temp, CellHeights[x, y]);
                                                        }
                                                        else if (temp < TopoModify.Hmin)
                                                        {
                                                            temp = TopoModify.Hmin;
                                                            CellHeights[x, y] = Math.Min(temp, CellHeights[x, y]);
                                                        }
                                                        else
                                                        {
                                                            CellHeights[x, y] = temp;
                                                        }

                                                    }
                                                    else
                                                    {
                                                        CellHeights[x, y] = TopoModify.Height;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // draw "pen"
                                    int step = (int)(flowfieldraster * (TopoModify.Raster * 2 + 1) / BmpScale / MapSize.SizeX / 2);
                                    using (Graphics g = Graphics.FromImage(PictureBoxBitmap))
                                    {
                                        Brush br = new SolidBrush(Color.FromArgb(128, 255, 255, 0));
                                        if (TopoModify.AbsoluteHeight == false)
                                        {
                                            if (TopoModify.Height >= 0)
                                                br = new SolidBrush(Color.FromArgb(128, 0, 0, 255));
                                            else if (TopoModify.Height < 0)
                                                br = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
                                        }

                                        float x0 = (float)(MainForm.GralDomRect.West + (i - 1) * flowfieldraster + flowfieldraster / 2);
                                        float y0 = (float)(MainForm.GralDomRect.South + (j - 1) * flowfieldraster + flowfieldraster / 2);
                                        int x1 = Convert.ToInt32((x0 - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                                        int y1 = Convert.ToInt32((y0 - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;

                                        Rectangle rect = new Rectangle(x1 - step, y1 - step, step * 2, step * 2);
                                        g.FillRectangle(br, rect);
                                        br.Dispose();
                                    }

                                    if (picturebox1.Image != null)
                                        picturebox1.Image.Dispose();

                                    if (PictureBoxBitmap != null)
                                        picturebox1.Image = PictureBoxBitmap.Clone(new Rectangle(0, 0, PictureBoxBitmap.Width, PictureBoxBitmap.Height), PictureBoxBitmap.PixelFormat);
                                }
                            }
                        }
                        else
                        {
                            toolStripTextBox3.Text = string.Empty;
                            textBox3.Text = string.Empty;
                        }
					}
				}
				catch
				{}
			}
			if (CellHeightsType == 1) // Show GRAMM height
			{
				try
				{
					if (MainForm.GRAMMHorGridSize > 0)
					{
                        int i = (int)((x_real - MainForm.GrammDomRect.West) / MainForm.GRAMMHorGridSize) + 1;
                        int j = (int)((y_real - MainForm.GrammDomRect.South) / MainForm.GRAMMHorGridSize) + 1;
                        if ((x_real - MainForm.GrammDomRect.West) > 0 && (y_real - MainForm.GrammDomRect.South) > 0 && i <= CellHeights.GetUpperBound(0) && j <= CellHeights.GetUpperBound(1))
                        {
                            string heightString = CellHeights[i, j].ToString("F1");
                            toolStripTextBox3.Text = heightString;
                            textBox3.Text = heightString;
                        }
                        else
                        {
                            toolStripTextBox3.Text = string.Empty;
                            textBox3.Text = string.Empty;
                        }
					}
				}
				catch{}
			}

			//draw GRAL model domain
			if ((MouseControl == 5) && (XDomain > 0))
			{
				int x1 = Math.Min(e.X, XDomain);
				int y1 = Math.Min(e.Y, YDomain);
				int x2 = Math.Max(e.X, XDomain);
				int y2 = Math.Max(e.Y, YDomain);
				int recwidth = x2 - x1;
				int recheigth = y2 - y1;
				GRALDomain = new Rectangle(x1, y1, recwidth, recheigth);
				//this.picturebox1_Paint();
				DrawRubberRect(GRALDomain);
			}

			//draw GRAMM model domain
			if ((MouseControl == 31) && (XDomain > 0))
			{
				int x1 = Math.Min(e.X, XDomain);
				int y1 = Math.Min(e.Y, YDomain);
				int x2 = Math.Max(e.X, XDomain);
				int y2 = Math.Max(e.Y, YDomain);
				int recwidth = x2 - x1;
				int recheigth = y2 - y1;
				GRAMMDomain = new Rectangle(x1, y1, recwidth, recheigth);
				DrawRubberRect(GRAMMDomain);
				//this.picturebox1_Paint();
			}

			//draw GRAMM sub-domain for export
			if ((MouseControl == 301) && (XDomain > 0))
			{
				int x1 = Math.Min(e.X, XDomain);
				int y1 = Math.Min(e.Y, YDomain);
				int x2 = Math.Max(e.X, XDomain);
				int y2 = Math.Max(e.Y, YDomain);
				int recwidth = x2 - x1;
				int recheigth = y2 - y1;
				GRAMMDomain = new Rectangle(x1, y1, recwidth, recheigth);
				DrawRubberRect(GRAMMDomain);
				//this.picturebox1_Paint();
			}

			//draw area source
			if ((MouseControl == 8)||(MouseControl==23))
			{
				CornerAreaSource[EditAS.CornerAreaCount + 1] = new Point(e.X, e.Y);
				DrawRubberLine(EditAS.CornerAreaCount, 1);
				//this.picturebox1_Paint();
				RubberLineCoors[0].X = -2; // reset lenght label
			}
			
			// Copy or Move Point Source
			if (CopiedItem.PointSource != null || MouseControl == 6000) 
			{
				DrawRubberPS(e.X, e.Y, 0);
			}
			if (CopiedItem.Building != null)
			{
				DrawRubberBuilding(1);
			}
			if (CopiedItem.LineSource != null)
			{
				DrawRubberLineSource();
			}
			//Copy or Move Receptor
			if (CopiedItem.Receptor != null || MouseControl == 2400)
			{
				DrawRubberPS(e.X, e.Y, 1);
			}
			if (CopiedItem.AreaSource != null)
			{
				DrawRubberBuilding(2);
			}
			if (CopiedItem.PortalSource != null)
			{
				DrawRubberPortal();
			}
			
			//draw rubber line for changing edge points
			if ((MouseControl == 108) ||(MouseControl == 100) || (MouseControl == 117) || (MouseControl == 101))
			{
				CornerAreaSource[2] = new Point(e.X, e.Y);
				DrawRubberLine(1, 1);
				RubberLineCoors[0].X = -2; // reset lenght label
			}
			
			// draw rubberlines of edited buildings, area sources, line sources, walls
			if (MouseControl == 1170 || MouseControl == 1080 || MouseControl == 1081 ||
			    (MouseControl == 1000) || (MouseControl == 1001))
			{
				CornerAreaSource[2] = new Point(e.X, e.Y);
				DrawRubberLine(1, 2);
				RubberLineCoors[0].X = -2; // reset lenght label
			}
			
			//draw vegetation
			if ((MouseControl == 79))
			{
				CornerAreaSource[EditVegetation.CornerVegetation + 1] = new Point(e.X, e.Y);
				DrawRubberLine(EditVegetation.CornerVegetation, 1);
				//this.picturebox1_Paint();
				RubberLineCoors[0].X = -2; // reset lenght label
			}
			
			//draw building
			if (MouseControl == 17)
			{
				CornerAreaSource[EditB.CornerBuilding + 1] = new Point(e.X, e.Y);
				DrawRubberLine(EditB.CornerBuilding, 1);
				//this.picturebox1_Paint();
				RubberLineCoors[0].X = -2; // reset lenght label
			}
			//draw line source, tunnel portals source, measuring tool distance or section drawing for windfields
			if ((MouseControl == 10) || (MouseControl == 22) || (MouseControl == 15) || (MouseControl == 44) || (MouseControl == 45))
			{
				CornerAreaSource[EditLS.CornerLineSource + 1] = new Point(e.X, e.Y);
				DrawRubberLine(EditLS.CornerLineSource, 1);
				//this.picturebox1_Paint();
				RubberLineCoors[0].X = -2; // reset lenght label
			}

			//draw walls
			if (MouseControl == 75)
			{
				CornerAreaSource[EditWall.CornerWallCount + 1] = new Point(e.X, e.Y);
				DrawRubberLine(EditWall.CornerWallCount, 1);
				//this.picturebox1_Paint();
				RubberLineCoors[0].X = -2; // reset lenght label
			}
			
			//draw rectangle for panel zoom
			if ((MouseControl == 14) && (XDomain > 0))
			{
				int x1 = Math.Min(e.X, XDomain);
				int y1 = Math.Min(e.Y, YDomain);
				int x2 = Math.Max(e.X, XDomain);
				int y2 = Math.Max(e.Y, YDomain);
				int recwidth = x2 - x1;
				int recheigth = y2 - y1;
				PanelZoom = new Rectangle(x1, y1, recwidth, recheigth);
				DrawRubberRect(PanelZoom);
				//this.picturebox1_Paint();
			}
			
			#if __MonoCS__
			#else
			// Tooltip for picturebox1
			if (ToolTipMousePosition.Active == true) // show lenght at the mouse position
			{
				if (RubberLineCoors[0].X == -1 && RubberLineCoors[0].Y == -1) // first point of lenght label
				{
					FirstPointLenght.X = (float) St_F.TxtToDbl(textBox1.Text, false);
					FirstPointLenght.Y = (float) St_F.TxtToDbl(textBox2.Text, false);
					RubberLineCoors[0].X = -2;
				}
				else
				{
					lenght = Math.Sqrt(Math.Pow((St_F.TxtToDbl(textBox1.Text, false) - FirstPointLenght.X),2)  +
					                   Math.Pow((St_F.TxtToDbl(textBox2.Text, false) -FirstPointLenght.Y),2));
					ToolTipMousePosition.ToolTipTitle = Convert.ToString(Math.Round(lenght,1));
				}
			}
			#endif
		}		
	}
}