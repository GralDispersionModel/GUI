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
 * User: Markus_2
 * Date: 19.06.2015
 * Time: 19:52
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Drawing.Drawing2D;
//using System.Windows.Media.Media3D;
using GralIO;
using GralStaticFunctions;

namespace GralDomForms
{

    public delegate void Section_Closed(object sender, EventArgs e);

	/// <summary>
    /// Draw a section along a section line; optional drawing of wind vectors
    /// </summary>
	public partial class Sectiondrawing : Form
	{
		public WindfieldSectionDrawings myData;
		public List<Single> GRAMMsurface = new List<Single>();   // List of GRAMM surface heights
		public List<Single> GRAMMcell = new List<Single>();   // List of GRAMM cell heights
		public List<Single> GRAL_surface = new List<Single>();   // List of GRAL surface heights
		public List<Int32> GRAL_KKart = new List<Int32>();      // List of GRAL highest cell index under the surface
		public List<Single> buildings = new List<Single>();      // List of buildings heights
		public List<Single> Uw = new List<Single>();            // List of uk values
		public List<Single> Vw = new List<Single>();            // List of vk values
		public List<Single> Ww = new List<Single>();            // List of wk values
		public List<Single> GRAL_HoKart = new List<Single>();            // List of height slice values

		public double SectionLenght;
		public double VerticalFactor = 0.6;
		public double HorizontalFactor = 1;
		public string DecSep;                        //global decimal separator of the system
		public int VerticalOffset = 0;
		public int HorOffset = 0;
		public Single GRAL_cellsize_ff = 0;
		public Single GRAMM_cellsize_ff = 0;
		public Single GRAL_speed = 0;
		public Single GRAL_direction = 0;
		public int GRAL_AK = 0;
		public double Ws_factor = 1;
		public double DD_factor = 1;
		public double SectionAngle;

		// delegate to send Message, that section-form is closed!
		public event Section_Closed Form_Section_Closed;

		public Sectiondrawing(WindfieldSectionDrawings windfield_data)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			//User defineddecimal seperator
			DecSep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

			myData = windfield_data;


			SectionLenght = Math.Sqrt(Math.Pow((myData.X0-myData.X1),2) + Math.Pow((myData.Y0-myData.Y1),2));
			SectionLenght = Math.Max(myData.cellsize, SectionLenght);

			if (SectionLenght < 10) {
				MessageBox.Show(this, "Section lenght too short","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			double dx = myData.X1 - myData.X0;
			double dy = myData.Y1 - myData.Y0;
			SectionAngle = Math.Atan2(dx,dy) * 180 / Math.PI+180;
			if (SectionAngle < 0)
            {
                SectionAngle += 360;
            }

            if (SectionAngle > 360)
            {
                SectionAngle -= 360;
            }
            //angle = Math.Round(angle/myData.Wind_sector_size) * myData.Wind_sector_size;
            //
            //			int x0 = Convert.ToInt32(Math.Floor((myData.x0-myData.GRAMM_west) / myData.GRAMM_cellsize )) ;
            //			int x1 = Convert.ToInt32(Math.Floor((myData.x1-myData.GRAMM_west) / myData.GRAMM_cellsize )) ;
            //			int y0 = Convert.ToInt32(Math.Floor((myData.y0-myData.GRAMM_south) / myData.GRAMM_cellsize));
            //			int y1 = Convert.ToInt32(Math.Floor((myData.y1-myData.GRAMM_south) / myData.GRAMM_cellsize));
            //			int x0 = Convert.ToInt32(Math.Floor((myData.x0-myData.GRAL_west) / myData.cellsize)) + 1;
            //			int x1 = Convert.ToInt32(Math.Floor((myData.x1-myData.GRAL_west) / myData.cellsize)) + 1;
            //			int y0 = Convert.ToInt32(Math.Floor((myData.y0-myData.GRAL_south) / myData.cellsize)) + 1;
            //			int y1 = Convert.ToInt32(Math.Floor((myData.y1-myData.GRAL_south) / myData.cellsize)) +1;
            //
            string caption = "GRAL GUI - Section drawing" ; //  + "  x0: "+
//				Convert.ToString(x0 ) + " / y0: " +
//				Convert.ToString(y0 ) + " / x1: " +
//				Convert.ToString(x1 ) + " / y1: " +
//				Convert.ToString(y1 ) + " / lenght: " +
//				Convert.ToString(Math.Round(section_lenght)) + " / angle:" +
//				Convert.ToString(Math.Round(section_angle));
			Text = caption;


			section_picture.Height = Math.Max( 1, ClientSize.Height - 120);
			section_picture.Width = Math.Max(1, ClientSize.Width -2);
			filterdissit.Checked = false;
			domainUpDown1.Items.Add("u,v projection");
			domainUpDown1.Items.Add("Vector Sum");
			domainUpDown1.SelectedIndex = 0;

			Fill_situations();

			Read_ggeom();

			if (myData.cellsize > 0)
            {
                Read_GRAL_geometries();
            }

            SectiondrawingResizeEnd(null, null);
			vScrollBar1.Maximum = 4000;
			vScrollBar1.Minimum = 0;
			vScrollBar1.Value = 2020;
			vScrollBar1.LargeChange = 10;
			vScrollBar1.SmallChange = 5;
			hScrollBar1.Maximum = 1000;
			hScrollBar1.Value = hScrollBar1.Maximum / 2;
		}

		private void Fill_situations()
		{
			double dx = myData.X1 - myData.X0;
			double dy = myData.Y1 - myData.Y0;
			double angle = Math.Atan2(dx,dy) * 180 / Math.PI +180;
			if (angle < 0)
            {
                angle += 360;
            }

            if (SectionAngle > 360)
            {
                SectionAngle -= 360;
            }

            select_situation.Clear();
			select_situation.GridLines = true;
			select_situation.Columns.Add("Situation", 55, HorizontalAlignment.Center);
			select_situation.Columns.Add("Wind sector", 83, HorizontalAlignment.Center);
			select_situation.Columns.Add("Wind speed", 83, HorizontalAlignment.Center);
			select_situation.Columns.Add("Stability class", 83, HorizontalAlignment.Center);
			select_situation.Columns.Add("Frequency [1/1000]", 135, HorizontalAlignment.Center);

			//read file meteopgt.all
			try
			{
				StreamReader myreader = new StreamReader(Path.Combine(myData.Path, @"meteopgt.all"));
				string[] text = new string[5];

				int direction;
				text = myreader.ReadLine().Split(new char[] { ' ', ';', ',', '\t' });

				// nearest sector size
				angle = Math.Round(angle/myData.WindSectorSize) * myData.WindSectorSize;
				//MessageBox.Show(Convert.ToString(myData.Wind_sector_size)+ "/"+Convert.ToString(angle));


				text = myreader.ReadLine().Split(new char[] { ' ', ';', ',', '\t' });
				text = myreader.ReadLine().Split(new char[] {' ',';',',','\t'});
				int count = 0;
				int dissit = 0;
				while (text[0] != "")
				{
					direction = Convert.ToInt32(Convert.ToDouble(text[0].Replace(".", DecSep))*10);
					dissit++; // count all situations
					if ((filterdissit.Checked == false) | ((Math.Abs(direction - angle) <= myData.WindSectorSize * 1.2) | (Math.Abs(Math.Abs(direction - angle) -360) <= myData.WindSectorSize * 1.2))
					    | ((Math.Abs(direction - angle - 180) <= myData.WindSectorSize * 1.2) | (Math.Abs(Math.Abs(direction - angle - 180) -360) <= myData.WindSectorSize * 1.2)))
					{
					    string gff = Path.Combine(St_F.GetGffFilePath(myData.Path), Convert.ToString(dissit).PadLeft(5, '0') + ".gff");
						string wnd = Path.Combine(Path.GetDirectoryName(myData.GRAMM_path), Convert.ToString(dissit).PadLeft(5, '0') + ".wnd");
						if (File.Exists(gff) | File.Exists(wnd))
						{
							ListViewItem item = new ListViewItem(Convert.ToString(dissit));
							item.SubItems.Add(Convert.ToString(direction));
							item.SubItems.Add(text[1]);
							item.SubItems.Add(text[2]);
							item.SubItems.Add(text[3]);
							select_situation.Items.Add(item);
							count++;
						}
					}
					try
					{
						text = myreader.ReadLine().Split(new char[] { ' ', ';', ',', '\t' });
					}
					catch
					{
						break;
					}
				}
				myreader.Close();
				if (count == 0)
				{
					ListViewItem item = new ListViewItem(Convert.ToString(0));
					item.SubItems.Add("No data");
					item.SubItems.Add("No data");
					item.SubItems.Add("No data");
					item.SubItems.Add("No data");
					select_situation.Items.Add(item);
				}
			}
			catch
			{
				ListViewItem item = new ListViewItem(Convert.ToString(0));
				item.SubItems.Add("No data");
				item.SubItems.Add("No data");
				item.SubItems.Add("No data");
				item.SubItems.Add("No data");
				select_situation.Items.Add(item);
			}
		}

		private void Read_ggeom()
		{
			//reading geometry file "ggeom.asc"
			try
			{
                GGeomFileIO ggeom = new GGeomFileIO
                {
                    PathWindfield = Path.GetDirectoryName(myData.GRAMM_path)
                };

                double[,] AH = new double[1, 1];
				int NX = 1;
				int NY = 1;
				int NZ = 1;
				string[] text = new string[1];

				if (ggeom.ReadGGeomAsc(1) == true)
				{
					NX = ggeom.NX;
					NY = ggeom.NY;
					NZ = ggeom.NZ;
					AH = ggeom.AH;
					ggeom = null;
				}
				else
                {
                    throw new FileNotFoundException("Error reading ggeom.asc");
                }

                int x0 = Convert.ToInt32(Math.Floor((myData.X0-myData.GrammWest) / myData.GrammCellsize )) ;
				int x1 = Convert.ToInt32(Math.Floor((myData.X1-myData.GrammWest) / myData.GrammCellsize )) ;
				int y0 = Convert.ToInt32(Math.Floor((myData.Y0-myData.GrammSouth) / myData.GrammCellsize));
				int y1 = Convert.ToInt32(Math.Floor((myData.Y1-myData.GrammSouth) / myData.GrammCellsize));

				GRAMMsurface.Clear();

				// Now get the elements between the start and the end point of the section line

				/*--------------------------------------------------------------
				 * Bresenham-algorithm; find a straight line on a raster data set
				 *
				 * input parameters:
				 *    int xstart, ystart        = coordinates start
				 *    int xend, yend            = xoordinates end
				 *---------------------------------------------------------------
				 */

				int x, y, t, dx, dy, incx, incy, pdx, pdy, ddx, ddy, es, el, err;

				// difference
				dx = x1 - x0;
				dy = y1 - y0;

				if (dx == 0 & dy == 0)
                {
                    GRAMMsurface.Add(Convert.ToSingle(-0.01));
                }
                else
				{
					// sign of the increment
					incx = Math.Sign(dx);
					incy = Math.Sign(dy);
					if(dx<0)
                    {
                        dx = -dx;
                    }

                    if (dy<0)
                    {
                        dy = -dy;
                    }

                    // where is the highest distance?
                    if (dx>dy)
					{
						// x the faster direction
						pdx=incx; pdy=0;    // pd. is the parellel step
						ddx=incx; ddy=incy; // dd. is the diagonal step
						es =dy;   el =dx;   // error steps
					}
					else
					{
						// y is the faster direction
						pdx=0;    pdy=incy;
						ddx=incx; ddy=incy;
						es =dx;   el =dy;
					}

					// initialisation of the loop
					x = x0;
					y = y0;
					err = el/2;

					if (x > 0 & y > 0 & x < NX & y < NY)
					{
						GRAMMsurface.Add(Convert.ToSingle(AH[x+1,y+1]));
					}
					else // marker for items out of the GRAL area
					{
						GRAMMsurface.Add(Convert.ToSingle(-0.1));
					}

					//SetPixel(x,y); // starting point

					// step through the raster
					for(t=0; t<el; ++t) // t counts the steps
					{
						// actualization of the error term
						err -= es;
						if(err<0)
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

						if (x > 0 & y > 0 & x < NX & y < NY)
						{
							GRAMMsurface.Add(Convert.ToSingle(AH[x+1,y+1]));
						}
						else // marker for items out of the GRAL area
						{
							GRAMMsurface.Add(Convert.ToSingle(-0.1));
						}
					}
				}
			}
			catch
			{
				//MessageBox.Show(this, "Error reading GRAMM geometry data","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
				myData.GrammCellsize = 0; // Marker for corrupt GRAMM data
				show_gramm.Checked = false;
				show_gral.Checked = true;
				show_buildings.Checked = true;
				return;
			}
		}

		private void Read_GRAL_geometries()
        {
			try
			{
                if (myData.StretchFlexible == null)
                {
                    myData.StretchFlexible = new List<float[]>();
                }
				string filename = Path.Combine(Path.GetDirectoryName(myData.Path), "GRAL_geometries.txt");
				if (File.Exists(filename))
				{
                    Single[,] AH, BH;
                    int[,] LKart;
                    using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
                    {
                        myData.Nkk = reader.ReadInt32();
                        myData.Njj = reader.ReadInt32();
                        myData.Nii = reader.ReadInt32();
                        myData.GralWest = reader.ReadInt32();
                        myData.GralSouth = reader.ReadInt32();
                        myData.Layer1 = reader.ReadSingle();
                        myData.Stretch = reader.ReadSingle();
                        if (myData.Stretch < 0.1)
                        {
                            int sliceCount = reader.ReadInt32();
                            myData.StretchFlexible.Clear();

                            for (int i = 0; i < sliceCount; i++)
                            {
                                myData.StretchFlexible.Add(new float[2]);
                                myData.StretchFlexible[i][0] = reader.ReadSingle(); // Height
                                myData.StretchFlexible[i][1] = reader.ReadSingle(); // Stretch
                            }
                        }

                        myData.AHmin = reader.ReadSingle();

                        // obtain surface heights
                        AH = new Single[myData.Nii + 2, myData.Njj + 2];
                        // obtain builing heights
                        BH = new Single[myData.Nii + 2, myData.Njj + 2];
                        // obtain KKart values
                        LKart = new int[myData.Nii + 2, myData.Njj + 2];

                        // read the complete dataset
                        for (int i = 1; i <= myData.Nii + 1; i++)
                        {
                            for (int j = 1; j <= myData.Njj + 1; j++)
                            {
                                AH[i, j] = reader.ReadSingle();
                                LKart[i, j] = reader.ReadInt32();
                                BH[i, j] = reader.ReadSingle();
                            }
                        }
                    }

					// compute start and end index of section line
					int x0 = Convert.ToInt32(Math.Floor((myData.X0-myData.GralWest) / myData.cellsize)) + 1;
					int x1 = Convert.ToInt32(Math.Floor((myData.X1-myData.GralWest) / myData.cellsize)) + 1;
					int y0 = Convert.ToInt32(Math.Floor((myData.Y0-myData.GralSouth) / myData.cellsize)) + 1;
					int y1 = Convert.ToInt32(Math.Floor((myData.Y1-myData.GralSouth) / myData.cellsize)) +1;

					// Now get the elements between the start and the end point of the section line

					/*--------------------------------------------------------------
					 * Bresenham-algorithm; find a straight line on a raster data set
					 *
					 * input parameters:
					 *    int xstart, ystart        = coordinates start
					 *    int xend, yend            = xoordinates end
					 *---------------------------------------------------------------
					 */

					int x, y, t, dx, dy, incx, incy, pdx, pdy, ddx, ddy, es, el, err;

					// difference
					dx = x1 - x0;
					dy = y1 - y0;

					if (dx == 0 & dy == 0)
					{
						GRAL_KKart.Add(-1);
					}
					else
					{
						// sign of the increment
						incx = Math.Sign(dx);
						incy = Math.Sign(dy);
						if(dx<0)
                        {
                            dx = -dx;
                        }

                        if (dy<0)
                        {
                            dy = -dy;
                        }

                        // where is the highest distance?
                        if (dx>dy)
						{
							// x the faster direction
							pdx=incx; pdy=0;    // pd. is the parellel step
							ddx=incx; ddy=incy; // dd. is the diagonal step
							es =dy;   el =dx;   // error steps
						}
						else
						{
							// y is the faster direction
							pdx=0;    pdy=incy;
							ddx=incx; ddy=incy;
							es =dx;   el =dy;
						}

						// initialisation of the loop
						x = x0;
						y = y0;
						err = el/2;

						if (x > 0 & y > 0 & x <= (myData.Nii+1) & y <= (myData.Njj+1))
						{
							GRAL_surface.Add(AH[x,y]);
							buildings.Add(BH[x,y]);
							GRAL_KKart.Add(LKart[x,y]);
						}
						else // marker for items out of the GRAL area
						{
							GRAL_KKart.Add(-1);
						}

						//SetPixel(x,y); // starting point

						// step through the raster
						for(t=0; t<el; ++t) // t counts the steps
						{
							// actualization of the error term
							err -= es;
							if(err<0)
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

							if (x > 0 & y > 0 & x <= (myData.Nii+1) & y <= (myData.Njj+1))
							{
								GRAL_surface.Add(AH[x,y]);
								buildings.Add(BH[x,y]);
								GRAL_KKart.Add(LKart[x,y]);
							}
							else // marker for items out of the GRAL area
							{
								GRAL_KKart.Add(-1);
							}
						}
					}
				}
			}
            catch
            {
                MessageBox.Show(this, "Error reading GRAL geometry file ", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                myData.cellsize = 0; // Marker for corrupt GRAL Files
            }

        }


		void SectiondrawingLoad(object sender, EventArgs e)
		{
			section_picture.Height = Math.Max(10, hScrollBar1.Top - 4 - section_picture.Top);
			vScrollBar1.Top = section_picture.Top;
			vScrollBar1.Height = section_picture.Height;

		}


		void Show3DClick(object sender, EventArgs e)
		{

			bool smooth = false;
            bool GRAL_Topo = false;
            double vert_fac=1;
            using (Dialog_3D dial = new Dialog_3D
            {
                Smoothing = false,
                VertFactor = vert_fac
            })
            {
                if (GRAL_surface.Count > 0 && show_gral.Checked == true) // Allow GRAL selection
                {
                    dial.GRAL_Topo = true;
                }

                if (dial.ShowDialog() == DialogResult.Cancel)
                {
                    return; // Cancel
                }

                smooth = dial.Smoothing;
                vert_fac = dial.VertFactor;
                GRAL_Topo = dial.GRAL_Topo;
            }

//			DialogResult r= MessageBox.Show("Smooth-Mode", "3D Viewer - Set Surface Mode", MessageBoxButtons.YesNoCancel , MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
//			if (r == DialogResult.No)	smooth = false;
//			if (r == DialogResult.Cancel) return;

			// Get surface min and max range of the section GRAMM and GRAL surfaces
			double AH_Min = 10000000; double AH_MAX = 0;

			Random zufall = new Random();
			int randnum = zufall.Next(1,100000);

			string path = Path.Combine(myData.PROJECT_path, @"Settings", "3D_VIEW" + Convert.ToString(randnum) + ".dta");
			string path2 = Path.Combine(myData.PROJECT_path, @"Settings", "3D_ARROW" + Convert.ToString(randnum) + ".dta");

			if (GRAL_Topo == false) // GRAMM Topo
			{
				for(int i=0; i<GRAMMsurface.Count;i++)
				{
					if (GRAMMsurface[i] > 0)
					{
						AH_Min = Math.Min(AH_Min, GRAMMsurface[i]);
						AH_MAX = Math.Max(AH_MAX, GRAMMsurface[i]);
					}
				}
			}
			else
			{
				for(int i = 0; i < GRAL_surface.Count; i++)
				{
					if (GRAL_surface[i] >= 0)
					{
						AH_Min = Math.Min(AH_Min, GRAL_surface[i]);
						AH_MAX = Math.Max(AH_MAX, GRAL_surface[i]);
					}
				}
			}

			if (AH_Min == 10000000)
            {
                AH_Min = 0; // no element in range
            }
            //MessageBox.Show(Convert.ToString(AH_Min));
            //double vert_fac;
            //			if ((AH_MAX-AH_Min) >10)
            //				vert_fac = 5 / (AH_MAX-AH_Min); // 0 -5
            //			else
            //				vert_fac = 0.5; // 0 -5

            double	x_step = 10 / (double) GRAMMsurface.Count; // -5 to +5 = 10
			double x = -5;
			vert_fac = vert_fac * x_step / myData.GrammCellsize;

			if (GRAMMsurface.Count > 0 && show_gramm.Checked && myData.GrammCellsize > 0 && GRAL_Topo == false)
			{
				try
				{
                    using (BinaryWriter mywriter = new BinaryWriter(File.Open(path, FileMode.Create)))
                    {
                        mywriter.Write(x_step); // x-Step
                        mywriter.Write(1D); // y-Step
                        mywriter.Write(GRAMMsurface.Count -1); // x-Anzahl
                        mywriter.Write(2); // y-Anzahl

						for(int i = 0; i < GRAMMsurface.Count; i++)
						{
							double height = -1;
							if (GRAMMsurface[i] >=0) // otherwise outside the GRAMM domain
                            {
                                height = (GRAMMsurface[i]-AH_Min) * vert_fac; // 0 - 5
                            }

                            for (int z = -1; z <= 1; z += 1)
							{
                                mywriter.Write(height);
							}

							x += x_step;
						}

						mywriter.Flush();
					}
					//mywriter.Close();

				}
				catch
				{	}

				// draw arrows
				if (GRAMM_cellsize_ff > 0)
				{
					try
					{
						x_step = 10 / (double) GRAMMsurface.Count; // -5 to +5 = 10
						x = -5;

                        using (BinaryWriter mywriter = new BinaryWriter(File.Open(path2, FileMode.Create)))
                        {
                            mywriter.Write(x_step); // x-Step
                            mywriter.Write(1D); // y-Step
                            mywriter.Write(GRAMMsurface.Count - 1); // x-Anzahl
                            mywriter.Write(myData.Nkk); // z-Anzahl

                            for(int i=0; i< GRAMMsurface.Count; i++)
							{
								int index = i * (1 + myData.Nkk); // offset for this point (row)
								for (int k = 1; k <= myData.Nkk; k++) // Get all height data at this point
								{
									if ((index + k) >= 0 && (index + k) < Uw.Count) // inside domain
									{
										double height = (GRAMMcell[index + k]-AH_Min) * vert_fac;

										if ((height - (GRAMMsurface[i]-AH_Min) * vert_fac) < 0 ) // lower then surface
										{
											mywriter.Write(-1D);
                                            mywriter.Write(0D);
                                            mywriter.Write(0D);
                                            mywriter.Write(0D);
                                            mywriter.Write(0D);
                                        }
										else
										{
											double angle = SectionAngle -90;
											double v = Math.Sqrt(Math.Pow(Uw[index + k] * 0.01 ,2) + Math.Pow(Vw[index + k] * 0.01,2) + Math.Pow(Ww[index + k] * 0.01,2));
											double uws = Uw[index + k] * Math.Sin(angle/180*Math.PI)+ Vw[index + k]  * Math.Cos(angle/180*Math.PI);
											double vws = Uw[index + k] * Math.Cos(angle/180*Math.PI) - Vw[index + k] * Math.Sin(angle/180*Math.PI);
											double vvert = Ww[index + k];
											uws = uws * x_step / 5 * DD_factor; // Skalierung 5 m/s = x_Step
											vws = vws * x_step / 5 * DD_factor; // Skalierung 5 m/s = x_Step
											vvert = vvert * x_step / 5 * DD_factor; // Skalierung 5 m/s = x_Step
											mywriter.Write(height);
                                            mywriter.Write(uws);
                                            mywriter.Write(vws);
                                            mywriter.Write(vvert);
                                            mywriter.Write(v);
                                        }
									}
									else // out of domain
									{
                                        mywriter.Write(-1D);
                                        mywriter.Write(0D);
                                        mywriter.Write(0D);
                                        mywriter.Write(0D);
                                        mywriter.Write(0D);
                                    }
								}
							}
							mywriter.Flush();
						}
						//mywriter.Close();
					}
					catch{}
				} // GRAMM ARROWS

			} //GRAMM DATA

			else if (GRAL_surface.Count > 0 & (show_buildings.Checked || show_gral.Checked) & myData.cellsize > 0 && GRAL_KKart.Count > 2 && GRAL_Topo == true) // GRAL DATA
			{
				if ((AH_MAX-AH_Min) >10)
                {
                    vert_fac = 5 / (AH_MAX-AH_Min); // 0 -5
                }
                else
                {
                    vert_fac = 0.5; // 0 -5
                }

                x_step = 10 / (double) GRAL_KKart.Count; // -5 to +5 = 10
				x = -5;
				try
				{
                    using (BinaryWriter mywriter = new BinaryWriter(File.Open(path, FileMode.Create)))
                    {
                        mywriter.Write(x_step); // x-Step
                        mywriter.Write(1D); // y-Step
                        mywriter.Write(GRAL_KKart.Count - 1); // x-Anzahl
                        mywriter.Write(2); // y-Anzahl

                        int pointer = -1;
						for(int i = 0; i < GRAL_KKart.Count; i++)
						{
							double height = -1;
							if (GRAL_KKart[i] >= 0) // otherwise outside the GRAMM domain
							{
								pointer++;
								height = ((double) GRAL_surface[pointer]-AH_Min) * vert_fac; // 0 - 5
								double bheight = ((double) buildings[pointer]-AH_Min) * vert_fac;
								height = Math.Max(height, bheight);
							}

							for (int z = -1; z <= 1; z += 1)
							{
                                mywriter.Write(height);
                            }

						}

						mywriter.Flush();
					}
					//mywriter.Close();
				}
				catch
				{}

				// write GRAL arrows
				if (GRAL_cellsize_ff != 0) // marker that windfield is loaded
				{
					try
					{
                        using (BinaryWriter mywriter = new BinaryWriter(File.Open(path2, FileMode.Create)))
                        {
                            mywriter.Write(x_step); // x-Step
                            mywriter.Write(1D); // y-Step
                            mywriter.Write(GRAL_KKart.Count - 1); // x-Anzahl
                            mywriter.Write(myData.Nkk / 2); // z-Anzahl

                            int pointer = -1;
							for(int i = 0; i < GRAL_KKart.Count; i++)
							{
								double height = -1;
								if (GRAL_KKart[i] >= 0) // otherwise outside the GRAMM domain
								{
									pointer++;

									int index = pointer * (1 + myData.Nkk); // offset for this point (row)
									for (int k = 0; k < myData.Nkk / 2; k++) // Get all height data at this point
									{
										if (GRAL_HoKart[k] > GRAL_surface[pointer] && (index + k) >= 0 && (index + k) < Uw.Count) // vector is higher than the surface
										{
											height = (GRAL_HoKart[k]-AH_Min) * vert_fac;
//										if (height > 0)
//										{
											double angle = SectionAngle -90;
											double v = Math.Sqrt(Math.Pow(Uw[index + k] * 0.01 ,2) + Math.Pow(Vw[index + k] * 0.01,2) + Math.Pow(Ww[index + k] * 0.01,2));
											double uws = Uw[index + k] * Math.Sin(angle/180*Math.PI)+ Vw[index + k] * Math.Cos(angle/180*Math.PI);
											double vws = Uw[index + k] * Math.Cos(angle/180*Math.PI)- Vw[index + k] * Math.Sin(angle/180*Math.PI);
											double vvert = Ww[index + k] * 0.01;
											uws = uws * x_step / 5 * DD_factor; // Skalierung 5 m/s = x_Step
											vws = vws * x_step / 5 * DD_factor; // Skalierung 5 m/s = x_Step
											vvert = vvert * x_step / 5 * DD_factor; // Skalierung 5 m/s = x_Step

                                            mywriter.Write(height);
                                            mywriter.Write(uws);
                                            mywriter.Write(vws);
                                            mywriter.Write(vvert);
                                            mywriter.Write(v);

//										}

										} // over the surface or out of domain
										else
										{
											mywriter.Write(-2D);
                                            mywriter.Write(0D);
                                            mywriter.Write(0D);
                                            mywriter.Write(0D);
                                            mywriter.Write(0D);
                                        }
								    } // all height data
								} // in the aerea
								else
								{
									for (int k = 0; k <= myData.Nkk / 2; k++) // Get all height data at this point
									{
										mywriter.Write(-1D);
                                        mywriter.Write(0D);
                                        mywriter.Write(0D);
                                        mywriter.Write(0D);
                                        mywriter.Write(0D);
                                    }
								}
							} // alle i

							mywriter.Flush();
						}
						//mywriter.Close();
					}
					catch
					{}
				} // Windfiield loaded
				// Draw GRAL wind vectors


			} // GRAL Surface

			#if __MonoCS__
			#else

			Gral3DFunctions.X_3D_Win win = new Gral3DFunctions.X_3D_Win(path, path2, "", smooth);
			System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(win);
			win.Show();
			#endif
		}

		void Section_picturePaint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			Pen p = new Pen(Color.Black, 1);

			int xmax = Math.Max(1, section_picture.Width - 50);
			int ymax = Math.Max(1, section_picture.Height -30);

			g.DrawLine(p, 26,ymax,xmax,ymax);
			g.DrawLine(p, 30,20,30,ymax + 4);

			// clip output
			if (section_picture.Width > 30)
			{
	        	g.Clip = new Region(new Rectangle(30, 10, section_picture.Width - 30, section_picture.Height));
			}

			// Get surface min and max range of the section GRAMM and GRAL surfaces
			double AH_Min = 10000000; double AH_MAX = 0;

			for(int i=0; i<GRAMMsurface.Count;i++)
			{
				if (GRAMMsurface[i] > 0)
				{
					AH_Min = Math.Min(AH_Min, GRAMMsurface[i]);
					AH_MAX = Math.Max(AH_MAX, GRAMMsurface[i]);
				}
			}

			if (GRAL_surface.Count > 0)
			{
				for(int i = 0; i < GRAL_surface.Count; i++)
				{
					if (GRAL_surface[i] > 0)
					{
						AH_Min = Math.Min(AH_Min, GRAL_surface[i]);
						AH_MAX = Math.Max(AH_MAX, GRAL_surface[i]);
					}
				}
			}


			Brush myBrush = new SolidBrush(Color.LightGreen);
			Pen mypen = new Pen(Color.Green);
            Pen windpen = new Pen(Color.Red)
            {
                Width = 1
            };
            Pen nullpen = new Pen(BackColor);
            // ownerdrawn cap arrow
            GraphicsPath capPath = new GraphicsPath();
			capPath.AddLine	(-3, 0, 3, 0);
			capPath.AddLine(-3, 0, 0, 3);
			capPath.AddLine(0, 3, 3, 0);

			if (AH_Min == 10000000)
            {
                AH_Min = 0; // no element in range
            }

            AH_Min = Convert.ToInt32(AH_Min/10)*10 + VerticalOffset; // normalize to 10m step

			// draw GRAMM surface and GRAMM wind vectors
			if (GRAMMsurface.Count > 0 & show_gramm.Checked & myData.GrammCellsize > 0)
			{
				double	x_step = ((double) xmax / (double) GRAMMsurface.Count) * HorizontalFactor;
				double x = 31 - HorOffset;

				for(int i=0; i< GRAMMsurface.Count; i++)
				{
					if (GRAMMsurface[i] >= 0) // otherwise outside the GRAMM domain
					{
						int height = Convert.ToInt32((GRAMMsurface[i]-AH_Min) * VerticalFactor);
						int b = Convert.ToInt32(Math.Max(1,x_step));
						int xl = Convert.ToInt32(x);
						if (b == 1)
                        {
                            g.DrawLine(mypen, xl,ymax,xl,height);
                        }
                        else
						{
							int y0 = Math.Min(25000, Math.Max(-25000, ymax - height));
							int h = Math.Min(25000, Math.Max(-25000, height));
							g.DrawRectangle(mypen, xl, y0, b, h);
							g.FillRectangle(myBrush, xl, y0, b, h);
						}

						// Draw GRAMM wind vectors
						if (GRAMM_cellsize_ff > 0)
						{
							int index = i * (1 + myData.Nkk); // offset for this point (row)


							for (int k = 1; k <= myData.Nkk; k++) // Get all height data at this point
							{
								if ((index + k) >= 0 && (index + k) < Uw.Count)
								{
									height = Convert.ToInt32((GRAMMcell[index + k]-AH_Min) * VerticalFactor);

									if (height > 0)
									{
										if (domainUpDown1.SelectedIndex == 1) // Vector Sum
										{
											double v = Math.Sqrt(Math.Pow(Uw[index + k] * 0.01 ,2) + Math.Pow(Vw[index + k] * 0.01,2));
											int vd = Convert.ToInt32(v * Ws_factor);

											int y0 = Math.Min(32000, Math.Max(-32000, ymax - height));

											g.DrawLine(windpen, xl, y0, xl+ vd, y0);
											g.DrawLine(windpen, xl+vd, y0 -2, xl+ vd, y0 + 2);
											g.DrawLine(windpen, xl, y0 -2, xl, y0 + 2);
										}


										if (domainUpDown1.SelectedIndex == 0) // u,v projection and w
										{

											double dx = myData.X1 - myData.X0;
											double dy = myData.Y1 - myData.Y0;
											double dw = 0;
											#if __MonoCS__

											double sin = dx * Vw[index + k] - Uw[index + k] * dy;
											double cos = dx * Uw[index + k] + dy * Vw[index + k];
											dw = Math.Atan2(sin, cos) * 180/Math.PI;

											#else

											System.Windows.Media.Media3D.Vector3D plane = new System.Windows.Media.Media3D.Vector3D(dx, dy, 0);
											System.Windows.Media.Media3D.Vector3D arrow = new System.Windows.Media.Media3D.Vector3D(Uw[index + k], Vw[index + k], 0);
											dw = System.Windows.Media.Media3D.Vector3D.AngleBetween(plane, arrow);

											#endif

											windpen.CustomEndCap = new System.Drawing.Drawing2D.CustomLineCap(null, capPath);
											double v = Math.Sqrt(Math.Pow(Uw[index + k] * 0.01 ,2) + Math.Pow(Vw[index + k] * 0.01,2));
											int vd = Convert.ToInt32(v * Math.Cos(dw * Math.PI / 180) * Ws_factor);
											double vv = v * Math.Sin(dw * Math.PI / 180);

											int vvert = Convert.ToInt32(Ww[index + k] * 0.01 * Ws_factor);
											//windpen.EndCap = LineCap.ArrowAnchor;

											windpen.Color = Arrow_color(vv);

											int y0 = Math.Min(20000, Math.Max(-20000, ymax - height));

                                            if (Math.Abs(vd) > 2)
                                            {
                                                g.DrawLine(windpen, xl, y0, xl + vd, y0 - vvert);
                                            }
                                            else
                                            {
                                                g.DrawLine(nullpen, xl, y0, xl + 1, y0);
                                            }
                                        }
										//windpen.EndCap = LineCap.Flat;

									}

								} // height
							} // index

						}

					}
					x += x_step;
				}
			}

			windpen.StartCap = LineCap.Flat;
			windpen.EndCap = LineCap.Flat;

			Brush GRALbrush = new SolidBrush(Color.Green);
			Pen GRALpen = new Pen(Color.Green);
			Brush nullbrush = new SolidBrush(BackColor);

			Pen buildingspen = new Pen(Color.LightSkyBlue);
			Brush buildingbrush = new SolidBrush(Color.LightSkyBlue);
            Pen gralwind = new Pen(Color.Black)
            {
                CustomStartCap = new System.Drawing.Drawing2D.CustomLineCap(null, capPath)
            };

            // Draw GRAL surface and buildings
            if  (GRAL_surface != null && GRAL_KKart.Count > 0 && GRAL_surface.Count > 0 && (show_buildings.Checked || show_gral.Checked) && myData.cellsize > 0)
			{

				double x_step = (double) xmax/ (double) GRAL_KKart.Count * HorizontalFactor;
				double x = 31 - HorOffset;
				int pointer = -1;

				for(int i = 0; i < GRAL_KKart.Count; i++)
				{
					if (GRAL_KKart[i] >= 0) // otherwise the point is outside the GRAL domain
					{
						pointer++;
						int height = Convert.ToInt32((GRAL_surface[pointer]-AH_Min) * VerticalFactor);
						int bheight = Convert.ToInt32((buildings[pointer]) * VerticalFactor);
						int b = Convert.ToInt32(Math.Max(1,x_step + 0.5));
						int xl = Convert.ToInt32(x);

						if (b == 1) // small rectangles are lines
						{
							if (AH_MAX > 0.2) // surface height exists
							{
								int y0 = Math.Min(32000, Math.Max(-32000, ymax - height));
								int ym = Math.Min(32000, Math.Max(-32000, ymax));

								if (show_gral.Checked)
                                {
                                    g.DrawLine(mypen, xl, ym, xl, y0);
                                }

                                if (bheight> 1 & show_buildings.Checked)
								{
									g.DrawLine(buildingspen, xl, y0, xl, Math.Min(y0 + bheight, ym));
								}
							}
							else // just buildings
							{
								if (bheight> 1 & show_buildings.Checked)
								{
									int y0 = Math.Min(32000, Math.Max(-32000, ymax - bheight));
									int ym = Math.Min(32000, Math.Max(-32000, ymax));
									g.DrawLine(buildingspen,xl, ym, xl, y0);
								}
							}
						}
						else
						{
							if (AH_MAX > 0.2) // surface height exists
							{
								if (show_gral.Checked)
								{
									int y0 = Math.Min(32000, Math.Max(-32000, ymax - height));
									int h = Math.Min(32000, Math.Max(-32000, height));

									g.DrawRectangle(GRALpen, xl, y0, b, h);
									g.FillRectangle(GRALbrush, xl, y0, b, h);
								}
								if (bheight>1 & show_buildings.Checked)
								{
									int y0 = (int) Math.Min(32000f, Math.Max(-32000f, ymax - height));
									int h = (int) Math.Min(32000f, Math.Max(-32000f, height));
									g.FillRectangle(buildingbrush, xl, y0, b, Math.Min(bheight, h));
								}

							}
							else // just buildings
							{
								if (bheight>1)
								{
									int y0 = (int) Math.Min(32000f, Math.Max(-32000f, ymax - bheight));
									int ym = (int) Math.Min(32000f, Math.Max(-32000f, bheight));
									g.FillRectangle(buildingbrush, xl, y0, b, ym);
								}
							}
						}

						// Draw GRAL wind-vecors
						if (GRAL_cellsize_ff != 0) // marker that windfield is loaded
						{
							int index = pointer * (1 + myData.Nkk); // offset for this point (row)

							for (int k = 0; k <= myData.Nkk; k++) // Get all height data at this point
							{
								if (GRAL_HoKart[k] > GRAL_surface[pointer] && (index + k) < Uw.Count && (index + k) >= 0) // vector is higher than the surface
								{
									height = Convert.ToInt32((GRAL_HoKart[k]-AH_Min) * VerticalFactor);
									if (height > 0  && (index + k) >= 0 && (index + k) < Uw.Count)
									{
										if (domainUpDown1.SelectedIndex == 1) // Vector Sum
										{
											double v = Math.Sqrt(Math.Pow(Uw[index + k],2) + Math.Pow(Vw[index + k],2) + Math.Pow(Ww[index + k],2));
											int vd = Convert.ToInt32(v * Ws_factor);

											int y0 = Math.Min(20000, Math.Max(-20000, ymax - height));

											g.DrawLine(windpen,xl,y0,xl+ vd,y0);
											g.DrawLine(windpen,xl+vd,y0 -2,xl+ vd,y0+2);
											g.DrawLine(windpen,xl,y0 -2,xl,y0+2);
										}
										if (domainUpDown1.SelectedIndex == 0) // u,v projection and w
										{
											double v = Uw[index + k]  * Math.Sin(SectionAngle/180*Math.PI)+ Vw[index + k] * Math.Cos(SectionAngle/180*Math.PI);
											double vv = Uw[index + k] * Math.Cos(SectionAngle/180*Math.PI)+ Vw[index + k] * Math.Sin(SectionAngle/180*Math.PI);

											int vd = Convert.ToInt32(v * Ws_factor);
											int vvert = Convert.ToInt32(Ww[index + k] * Ws_factor);

											gralwind.Color = Arrow_color(vv);
											int y0 = Math.Min(20000, Math.Max(-20000, ymax - height));

											if (vd < -2)
											{
												vd = Math.Max(1,Math.Abs(vd));
												g.DrawLine(gralwind, xl + vd, y0, xl, y0 + vvert);
											}
											else if (vd > 2)
											{
												vd = Math.Max(1,vd);
												g.DrawLine(gralwind, xl,  y0, xl+vd, y0 + vvert);
											}
                                            else
                                            {
                                                g.DrawLine(nullpen, xl, y0, xl + 1, y0);
                                            }

										}
									}
								}
							}
						}
						// Draw GRAL wind vectors
					}

					x += x_step;
				}
			}



			windpen.Dispose();
			nullpen.Dispose();
			nullbrush.Dispose();
			buildingspen.Dispose();
			buildingbrush.Dispose();
			GRALpen.Dispose();
			GRALbrush.Dispose();
			gralwind.Dispose();


			g.ResetClip();

			// scale x - direction
			string value="";
			Font drawFont = new Font("Arial", 8);
            StringFormat drawFormat = new StringFormat
            {
                Alignment = StringAlignment.Center
            };

            Brush stringBrush = new SolidBrush(Color.Black);

			float xR_norm = (float) Math.Max(1, Normalize(SectionLenght / 10 )); // 1 unit
			float xR_step = (float) (xR_norm / SectionLenght * xmax * HorizontalFactor);

			for (int i = 0; i < 20; i++)
			{
				g.DrawLine(mypen, 30 - HorOffset + xR_step * i, ymax + 2, 30 - HorOffset + xR_step * i, ymax -2);
				value = Convert.ToString(Math.Round(i * xR_norm));
				g.DrawString(value, drawFont, stringBrush, 30 - HorOffset + xR_step * i, ymax + 4, drawFormat);
			}

			drawFormat.LineAlignment = StringAlignment.Center;

			//scale y - direction
			for (int i = 0; i < 10; i++)
			{
				int y = Convert.ToInt32(ymax - ymax / 10 * i );
				g.DrawLine(mypen, 28, y , 32, y);
				value = Convert.ToString(Math.Round(AH_Min + (ymax -y)/VerticalFactor));
				g.DrawString(value, drawFont, stringBrush, 15, y, drawFormat);
			}

			drawFont.Dispose();
			stringBrush.Dispose();
			myBrush.Dispose();
			mypen.Dispose();
			capPath.Dispose();
			//g.Dispose ();
		}


		private double Normalize(double val)
        {
        	int exp = 0;
        	if (val != 0.0)
            {
                exp = (int) Math.Floor(Math.Log10(val)) * (-1);
            }

            double norm = val * Math.Pow(10, exp);

        	if (norm < 1.5)
            {
                norm = 1;
            }
            else if (norm < 3.4)
            {
                norm  = 2;
            }
            else if (norm < 6.5)
            {
                norm = 5;
            }
            else if (norm < 9)
            {
                norm = 8;
            }
            else
            {
                norm = 10;
            }

            exp *= -1;
        	return norm * Math.Pow(10, exp);
        }

		Color Arrow_color(double vv)
		{
			Color col;
			if (Math.Abs(vv) < 0.2)
            {
                col = Color.Gray;
            }
            else
			{
				//vv *= ws_factor;


				if (vv>0)
				{
					int gb = Convert.ToInt16(Math.Max(0,Math.Min(132, 177-Math.Abs(vv*45)))); // 0-3 m/s light colors
					int r = 255;
					if (gb == 0)
                    {
                        r =Convert.ToInt16(Math.Max(0,255-Math.Abs((vv-3.8)*50)));
                    }

                    col = Color.FromArgb(r,gb,gb);
				}
				else
				{
					int gb = Convert.ToInt16(Math.Max(0,Math.Min(192, 222-Math.Abs(vv*30)))); // 0-4 m/s light colors
					int b = 255;
					if (gb == 0)
                    {
                        b =Convert.ToInt16(Math.Max(0,255-Math.Max(0,Math.Abs((vv-7)*40))));
                    }

                    col = Color.FromArgb(gb,gb,b);
				}

			}
			return col;
		}

		void SectiondrawingResizeEnd(object sender, EventArgs e)
		{
			section_picture.Height = Math.Max(10, hScrollBar1.Top - 4 - section_picture.Top);
			vScrollBar1.Top = section_picture.Top;
			vScrollBar1.Height = section_picture.Height;
			vScrollBar1.Left = ClientSize.Width - vScrollBar1.Width;
			hScrollBar1.Width = Math.Max (1, ClientSize.Width - hScrollBar1.Left);
			section_picture.Width = Math.Max(1, ClientSize.Width - section_picture.Left);

			section_picture.Refresh();
		}
		void SectiondrawingResize(object sender, EventArgs e)
		{
            if (WindowState == FormWindowState.Maximized)
            {
                SectiondrawingResizeEnd(null, null);
                // Maximized!
            }
            if (WindowState == FormWindowState.Normal)
            {
                SectiondrawingResizeEnd(null, null);
                // Restored!
            }

            vScrollBar1.Left = ClientSize.Width - vScrollBar1.Width;
			hScrollBar1.Width = Math.Max (1, ClientSize.Width - hScrollBar1.Left);
			//SectiondrawingResizeEnd(null, null);
		}


		void Show_grammCheckedChanged(object sender, EventArgs e)
		{
			section_picture.Refresh();
		}

		void Show_gralCheckedChanged(object sender, EventArgs e)
		{
			section_picture.Refresh();
		}

		void Show_buildingsCheckedChanged(object sender, EventArgs e)
		{
			section_picture.Refresh();
		}


		void Show_windfiedClick(object sender, EventArgs e)
		{
			try
			{
				int dissit;
				if (select_situation.SelectedItems.Count > 0)
				{
					dissit = Convert.ToInt32(select_situation.SelectedItems[0].SubItems[0].Text);
					GRAL_direction = Convert.ToSingle(select_situation.SelectedItems[0].SubItems[1].Text);
					GRAL_cellsize_ff = 0;
				}
				else
				{
					MessageBox.Show(this, "Select a dispersion situation, please!","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
					GRAL_cellsize_ff = 0; // Marker for corrupt winddata
					wind_data_picturebox.Refresh();
					section_picture.Refresh();
					return;
				}

				if (show_gral.Checked == true)
                {
                    Read_gral_windfield(dissit);
                }
                else
					if (show_gramm.Checked == true)
                {
                    Read_gramm_windfield(dissit);
                }
            }
			catch
			{
				MessageBox.Show(this, "Select a dispersion situation, please!","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				GRAL_cellsize_ff = 0; // Marker for corrupt winddata
			}
		}

		private async void Read_gral_windfield(int dissit)
		{
			// new dispersion situation selected
			//windfield file Readers
            if (myData.cellsize < 0.1)
            {
                return;
            }

			int nkk = 0;
			int njj = 0;
			int nii = 0;
            float[][][] uk;
            float[][][] vk;
            float[][][] wk;

			//reading *.gff-file
			try
			{
			    string gffname = Path.Combine(St_F.GetGffFilePath(myData.Path), Convert.ToString(dissit).PadLeft(5, '0') + ".gff");

                ReadFlowFieldFiles gff = new ReadFlowFieldFiles
                {
                    filename = gffname
                };

                System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                if (gff.ReadGffFile(cts.Token) == true)
				{
					nkk = gff.NKK;
					njj = gff.NJJ;
					nii = gff.NII;
					uk = gff.Uk; // Attention: arrays must be smaller than 2 GBytes
					vk = gff.Vk;
					wk = gff.Wk;

					GRAL_speed = gff.Speed;
					GRAL_AK = gff.AK;
					GRAL_cellsize_ff = gff.Cellsize;
					gff = null;
				}
				else
				{
					throw new IOException();
				}
                cts = null;

                // compute start and end index of section line

                int x0 = Convert.ToInt32((myData.X0-myData.GralWest) / myData.cellsize + 0.5) ;
				int x1 = Convert.ToInt32((myData.X1-myData.GralWest) / myData.cellsize + 0.5) ;
				int y0 = Convert.ToInt32((myData.Y0-myData.GralSouth) / myData.cellsize + 0.5);
				int y1 = Convert.ToInt32((myData.Y1-myData.GralSouth) / myData.cellsize + 0.5);
				if (Math.Min(y0,y1) <= 0 || Math.Min(x1,x0) <= 0 || Math.Max(x1,x0) >= nii || Math.Max(y1,y0) >= njj)
				{
					MessageBox.Show(this, "Section line outside the GRAL Domain","GRAL GUI", MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    uk = null; vk = null; wk = null;
					return;
				}

				//MessageBox.Show(Convert.ToString(dissit));
				// now get the wind-values for the section-cells

				//computation of slice-heights
				GRAL_HoKart.Clear();
				Single DZKdummy = myData.Layer1;
				Single h = 0; Single halt = 0;
                int flexstretchindex = 0;
                float stretching = 1;

                for (int k = 1; k <= nkk + 1; k++)
				{
					halt = h;
					h += DZKdummy;
					GRAL_HoKart.Add(myData.AHmin + h - (h-halt)/2); // GRAL_HoKart contains all interpolated middle slice heights
                    if (myData.Stretch > 0.99)
                    {
                        DZKdummy *= myData.Stretch;
                    }
                    else
                    {
                        if (flexstretchindex < myData.StretchFlexible.Count - 1)
                        {
                            if (myData.StretchFlexible[flexstretchindex + 1][1] > 0.99 &&
                                h > myData.StretchFlexible[flexstretchindex + 1][0])
                            {
                                stretching = myData.StretchFlexible[flexstretchindex + 1][1];
                                flexstretchindex++;
                            }
                        }
                        DZKdummy *= stretching;
                    }
				}

				Uw.Clear();
				Vw.Clear();
				Ww.Clear();


				// Now get the elements between the start and the end point of the section line

				/*--------------------------------------------------------------
				 * Bresenham-algorithm; find a straight line on a raster data set
				 *
				 * input parameters:
				 *    int xstart, ystart        = coordinates start
				 *    int xend, yend            = xoordinates end
				 *---------------------------------------------------------------
				 */

				int x, y, t, dx, dy, incx, incy, pdx, pdy, ddx, ddy, es, el, err;

				// difference
				dx = x1 - x0;
				dy = y1 - y0;

				if (dx == 0 & dy == 0)
				{
				}
				else
				{
					// sign of the increment
					incx = Math.Sign(dx);
					incy = Math.Sign(dy);
					if(dx<0)
                    {
                        dx = -dx;
                    }

                    if (dy<0)
                    {
                        dy = -dy;
                    }

                    // where is the highest distance?
                    if (dx>dy)
					{
						// x the faster direction
						pdx=incx; pdy=0;    // pd. is the parellel step
						ddx=incx; ddy=incy; // dd. is the diagonal step
						es =dy;   el =dx;   // error steps
					}
					else
					{
						// y is the faster direction
						pdx=0;    pdy=incy;
						ddx=incx; ddy=incy;
						es =dx;   el =dy;
					}

					// initialisation of the loop
					x = x0;
					y = y0;
					err = el/2;

					if (x > 0 & y > 0 & x <= (nii) & y <= (njj)) // found point inside GRAL area
					{
						for (int k = 0; k <= nkk; k++) // Get all height dependend wind data at this point
						{
							Uw.Add(uk[x + 1][y + 1][k]);
							Vw.Add(vk[x + 1][y + 1][k]);
							Ww.Add(wk[x + 1][y + 1][k]);
						}
					}

					//SetPixel(x,y); // starting point

					// step through the raster
					for(t=0; t<el; ++t) // t counts the steps
					{
						// actualization of the error term
						err -= es;
						if(err<0)
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


						if (x > 0 & y > 0 & x <= (nii) & y <= (njj)) // found point inside GRAL area
						{
							for (int k = 0; k <= nkk; k++) // Get all height dependend wind data at this point
                            {
								Uw.Add(uk[x + 1][y + 1][k]);
								Vw.Add(vk[x + 1][y + 1][k]);
								Ww.Add(wk[x + 1][y + 1][k]);
							}
						}

					}
				}

				// Calculate x_Step
				int xmax = Math.Max(1, section_picture.Width - 50);
				double x_step = xmax/GRAL_KKart.Count*HorizontalFactor;
				double ws = Math.Max(2, Math.Ceiling(GRAL_speed/2)*2); // wind speed in steps of 2 m/s
				Ws_factor = x_step/ws;

				wind_data_picturebox.Refresh();
				section_picture.Refresh();
			}

            catch
            {
                MessageBox.Show(this, "Error reading GRAL windfields", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GRAL_cellsize_ff = 0; // Marker for corrupt winddata
                wind_data_picturebox.Refresh();
                section_picture.Refresh();
            }
            uk = null;
            vk = null;
            wk = null;
        }


		private async void Read_gramm_windfield(int dissit)
		{
			try
			{
                //read GRAMM wind field

                //reading geometry file "ggeom.asc"

                GGeomFileIO ggeom = new GGeomFileIO
                {
                    PathWindfield = Path.GetDirectoryName(myData.GRAMM_path)
                };

                double[,] AH = new double[1, 1];
				double[, ,] ZSP = new double[1, 1, 1];
				int NX = 1;
				int NY = 1;
				int NZ = 1;
				string[] text = new string[1];

				if (ggeom.ReadGGeomAsc(0) == true)
				{
					NX = ggeom.NX;
					NY = ggeom.NY;
					NZ = ggeom.NZ;
					AH = ggeom.AH;
					ZSP = ggeom.ZSP;
					ggeom = null;
				}
				else
                {
                    throw new FileNotFoundException("Error reading ggeom.asc");
                }

                // compute start and end index of section line
                int x0 = Convert.ToInt32((myData.X0-myData.GrammWest) / myData.GrammCellsize + 0.5) ;
				int x1 = Convert.ToInt32((myData.X1-myData.GrammWest) / myData.GrammCellsize + 0.5) ;
				int y0 = Convert.ToInt32((myData.Y0-myData.GrammSouth) / myData.GrammCellsize + 0.5);
				int y1 = Convert.ToInt32((myData.Y1-myData.GrammSouth) / myData.GrammCellsize + 0.5);

				if (Math.Min(y0,y1) <= 0 || Math.Min(x1,x0) <= 0 || Math.Max(x1,x0) >= NX || Math.Max(y1,y0) >= NY)
				{
					MessageBox.Show(this, "Section line outside GRAMM Domain","GRAL GUI", MessageBoxButtons.OK,MessageBoxIcon.Warning);
					return;
				}

				//				StreamWriter writer = new StreamWriter(myData.path + @"Test.txt");
//				for (ix=0; ix<=NX;ix++)
//					for(iy=0;iy<=NY;iy++)
//				{
//				writer.WriteLine(Convert.ToString(ZSP[ix,iy,1]) + "/" + Convert.ToString(AH[ix,iy]));
//				}
//				writer.Close();


				//read wind fields
				Single[, ,] UWI = new Single[NX + 1, NY + 1, NZ + 1];
				Single[, ,] VWI = new Single[NX + 1, NY + 1, NZ + 1];
				Single[, ,] WWI = new Single[NX + 1, NY + 1, NZ + 1];

				string wndfilename = Path.Combine(Path.GetDirectoryName(myData.GRAMM_path), Convert.ToString(dissit).PadLeft(5, '0') + ".wnd");
				Windfield_Reader Reader = new Windfield_Reader();
				if (await System.Threading.Tasks.Task.Run(() => Reader.Windfield_read(wndfilename, NX, NY, NZ, ref UWI, ref VWI, ref WWI)) == false)
				{
					throw new IOException();
				}

				// get section data
				Uw.Clear();
				Vw.Clear();
				Ww.Clear();
				GRAMMsurface.Clear();


				// Now get the elements between the start and the end point of the section line

				/*--------------------------------------------------------------
				 * Bresenham-algorithm; find a straight line on a raster data set
				 *
				 * input parameters:
				 *    int xstart, ystart        = coordinates start
				 *    int xend, yend            = xoordinates end
				 *---------------------------------------------------------------
				 */

				int x, y, t, dx, dy, incx, incy, pdx, pdy, ddx, ddy, es, el, err;

				// difference
				dx = x1 - x0;
				dy = y1 - y0;

				if (dx == 0 & dy == 0)
				{
					GRAMMsurface.Add(Convert.ToSingle(-0.01));
				}
				else
				{
					// sign of the increment
					incx = Math.Sign(dx);
					incy = Math.Sign(dy);
					if(dx<0)
                    {
                        dx = -dx;
                    }

                    if (dy<0)
                    {
                        dy = -dy;
                    }

                    // where is the highest distance?
                    if (dx>dy)
					{
						// x the faster direction
						pdx=incx; pdy=0;    // pd. is the parellel step
						ddx=incx; ddy=incy; // dd. is the diagonal step
						es =dy;   el =dx;   // error steps
					}
					else
					{
						// y is the faster direction
						pdx=0;    pdy=incy;
						ddx=incx; ddy=incy;
						es =dx;   el =dy;
					}

					// initialisation of the loop
					x = x0;
					y = y0;
					err = el/2;

					if (x > 0 & y > 0 & x < NX & y < NY) // found point inside GRAL area
					{
						for (int k = 0; k <= NZ; k++) // Get all height data at this point
						{
							Uw.Add(UWI[x+1, y+1, k]*100);
							Vw.Add(VWI[x+1, y+1, k]*100);
							Ww.Add(WWI[x+1, y+1, k]*100);
							GRAMMcell.Add((float) ZSP[x+1, y+1, k]);
						}
						GRAMMsurface.Add(Convert.ToSingle(AH[x+1,y+1]));
					}
					else
					{
						GRAMMsurface.Add(Convert.ToSingle(-0.1));
					}
					//SetPixel(x,y); // starting point

					// step through the raster
					for(t=0; t<el; ++t) // t counts the steps
					{
						// actualization of the error term
						err -= es;
						if(err<0)
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


						if (x > 0 & y > 0 & x < NX & y < NY) // found point inside GRAL area
						{
							for (int k = 0; k <= NZ; k++) // Get all height data at this point
							{
								Uw.Add(UWI[x+1, y+1, k]*100);
								Vw.Add(VWI[x+1, y+1, k]*100);
								Ww.Add(WWI[x+1, y+1, k]*100);
								GRAMMcell.Add((float) ZSP[x+1, y+1, k]);
							}
							GRAMMsurface.Add(Convert.ToSingle(AH[x+1,y+1]));
						}
						else
						{
							GRAMMsurface.Add(Convert.ToSingle(-0.1));
						}

					}
				}

				// Calculate x_Step
				int xmax = section_picture.Width - 50;
				double x_step = xmax/GRAMMsurface.Count*HorizontalFactor;
				double ws = 1;
				Ws_factor = x_step/ws;
				GRAMM_cellsize_ff = 1; // Marker for valid winddata
				myData.Nkk = NZ;

				wind_data_picturebox.Refresh();
				section_picture.Refresh();

			}
			catch
			{
				MessageBox.Show(this, "Can´t read the wind field data","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
				GRAMM_cellsize_ff = 0; // Marker for corrupt GRAMM winddata
			}
		}

		void VScrollBar1ValueChanged(object sender, EventArgs e)
		{
			VerticalOffset = 2000-vScrollBar1.Value;
			section_picture.Refresh();
		}

		void Picturebox_centerClick(object sender, EventArgs e)
		{
			VerticalFactor = 0.6;
			VerticalOffset = 0;
			vScrollBar1.Value = 2000;

			section_picture.Refresh();

		}

		void HScrollBar1ValueChanged(object sender, EventArgs e)
		{
			HorOffset = hScrollBar1.Value - hScrollBar1.Maximum / 2;
			section_picture.Refresh();
		}

		// draw info picture
		void Wind_data_pictureboxPaint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			if (GRAL_cellsize_ff > 0)
			{
				Pen p = new Pen(Color.Black, 1);
				Font drawFont = new Font("Arial", 7);
                StringFormat drawFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near
                };

                Brush stringBrush = new SolidBrush(Color.Black);

				int y = 3;
				g.DrawString("GRAL windfield", drawFont, stringBrush, 10, y, drawFormat);
				y+=10;
				g.DrawString("Wind sector: " + Convert.ToString(GRAL_direction) + " " + "\x00B0", drawFont, stringBrush, 10, y, drawFormat);
				y+=10;
				g.DrawString("Wind speed: " + Convert.ToString(Math.Round(GRAL_speed,2)) + " m/s", drawFont, stringBrush, 10, y, drawFormat);
				y+=10;
				g.DrawString("SC: " + Convert.ToString(GRAL_AK), drawFont, stringBrush, 10, y, drawFormat);

				y+=30;

				double ws;
				if (Ws_factor > 0)
				{
					ws = Math.Round((wind_data_picturebox.Width - 40) / Ws_factor, 2);

					Pen windpen	 = new Pen(Color.Gray);
					int x = wind_data_picturebox.Width - 40;

					g.DrawLine(windpen, 20, y, 20 + x, y);
					g.DrawLine(windpen, 20, y - 3, 20, y + 3);
					g.DrawLine(windpen, 20 + x, y - 3, 20 + x, y + 3);
					g.DrawString(ws.ToString() +"m/s",drawFont, stringBrush, 20 + x / 4, y - 12, drawFormat);

					y+=8;
					int xx=20;
					int step = Convert.ToInt32(Math.Max(1,x/Math.Max(1,Math.Ceiling(ws))));

					for (double vv=0; vv<ws; vv+=1)
					{
						Rectangle rect = new Rectangle(xx, y, step,5);
						g.FillRectangle(new SolidBrush(Arrow_color(vv)),rect);

						rect = new Rectangle(xx, y+5, step,5);
						g.FillRectangle(new SolidBrush(Arrow_color(-vv)),rect);
						//g.DrawRectangle(windpen, rect);
						xx+= step;
					}

					windpen.Dispose();
					p.Dispose();
					drawFont.Dispose();
					stringBrush.Dispose();
				}
			}
			if (GRAMM_cellsize_ff > 0)
			{
				Pen p = new Pen(Color.Black, 1);
				Font drawFont = new Font("Arial", 7);
                StringFormat drawFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near
                };

                Brush stringBrush = new SolidBrush(Color.Black);

				int y = 3;
				g.DrawString("GRAMM windfield", drawFont, stringBrush, 10, y, drawFormat);
				y+=10;
				g.DrawString("Wind sector: " + select_situation.SelectedItems[0].SubItems[1].Text + " " + "\x00B0", drawFont, stringBrush, 10, y, drawFormat);
				y+=10;
				g.DrawString("Wind speed: " + select_situation.SelectedItems[0].SubItems[2].Text + " m/s", drawFont, stringBrush, 10, y, drawFormat);
				y+=10;
				g.DrawString("SC: " + select_situation.SelectedItems[0].SubItems[3].Text, drawFont, stringBrush, 10, y, drawFormat);

				y+=30;

				double ws;
				if (Ws_factor > 0)
				{
					ws = Math.Round((wind_data_picturebox.ClientSize.Width - 40)/Ws_factor,2);

					Pen windpen	 = new Pen(Color.Gray);
					int x = Convert.ToInt32(ws * Ws_factor);

					g.DrawLine(windpen, 20,y, 20+x,y);
					g.DrawLine(windpen, 20, y-3, 20, y+3);
					g.DrawLine(windpen, 20 + x, y-3, 20+ x, y+3);
					g.DrawString(Convert.ToString(ws) +"m/s",drawFont, stringBrush, 20+x/4,y-12, drawFormat);

					y+=8;
					int xx=20;
					int step = Convert.ToInt32(Math.Max(1,x/Math.Max(1,Math.Ceiling(ws))));

					for (double vv=0; vv<ws; vv+=1)
					{
						Rectangle rect = new Rectangle(xx, y, step,5);
						g.FillRectangle(new SolidBrush(Arrow_color(vv)),rect);

						rect = new Rectangle(xx, y+5, step,5);
						g.FillRectangle(new SolidBrush(Arrow_color(-vv)),rect);
						//g.DrawRectangle(windpen, rect);
						xx+= step;
					}


					windpen.Dispose();
					p.Dispose();
					drawFont.Dispose();
					stringBrush.Dispose();
				}
			}

			if (GRAL_cellsize_ff > 0 || GRAMM_cellsize_ff > 0)
			{

			}

		}


		void FilterdissitCheckedChanged(object sender, EventArgs e)
		{
			Fill_situations();
		}

		void DomainUpDown1SelectedItemChanged(object sender, EventArgs e)
		{
			wind_data_picturebox.Refresh();
			section_picture.Refresh();
		}

		void Section_clipboardClick(object sender, EventArgs e)
		{
			Bitmap bitMap = new Bitmap(section_picture.Width, section_picture.Height);
			section_picture.DrawToBitmap(bitMap, new Rectangle(0, 0, section_picture.Width, section_picture.Height));
			Clipboard.SetDataObject(bitMap);
		}


		void Vert_plusClick(object sender, EventArgs e)
		{
			if (Control.ModifierKeys == Keys.Shift || sender == button2)
			{
				Ws_factor *= 1.5;
				DD_factor *= 1.5; // for 3D-view
				wind_data_picturebox.Refresh();
			}
			else if (Control.ModifierKeys == Keys.Control || sender == button4)
			{
				HorizontalFactor *= 1.5;
				int xmax = Math.Max(1, section_picture.Width + 50);
				hScrollBar1.Maximum = (int) Math.Min(8000, Math.Max (500, (int) HorizontalFactor * xmax * 2));

				hScrollBar1.Value = hScrollBar1.Maximum / 2;
			}
			else
			{
				VerticalFactor *= 1.5;
			}
			section_picture.Refresh();
		}

		void Vert_minusClick(object sender, EventArgs e)
		{
			if (Control.ModifierKeys == Keys.Shift || sender == button1)
			{
				Ws_factor /= 1.5;
				DD_factor /= 1.5; // for 3D-view
				wind_data_picturebox.Refresh();
			}
			else if (Control.ModifierKeys == Keys.Control || sender == button3)
			{
				HorizontalFactor /= 1.5;
				int xmax = Math.Max(1, section_picture.Width + 50);
				hScrollBar1.Maximum = (int) Math.Min(8000, Math.Max (500, (int) HorizontalFactor * xmax * 2));

				hScrollBar1.Value = hScrollBar1.Maximum / 2;
			}
			else
			{
				VerticalFactor /= 1.5;
			}
			section_picture.Refresh();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			//zoom in
			if (keyData == Keys.Oemplus)
			{
				Vert_plusClick(null, null);
			}


			//zoom out
			if (keyData == Keys.OemMinus)
			{
				Vert_minusClick(null, null);
			}


			if (keyData == Keys.Add)
			{
				Ws_factor *= 1.5;
				wind_data_picturebox.Refresh();
				section_picture.Refresh();
			}

			if (keyData == Keys.Subtract)
			{
				Ws_factor /= 1.5;
				wind_data_picturebox.Refresh();
				section_picture.Refresh();
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}



		void SectiondrawingFormClosed(object sender, FormClosedEventArgs e)
			// send Message to domain Form, that Section-Form is closed
		{
			try
			{
				if (Form_Section_Closed != null)
                {
                    Form_Section_Closed(this, null); // call function closed()
                }
            }
			catch{}
		}


	}

}
