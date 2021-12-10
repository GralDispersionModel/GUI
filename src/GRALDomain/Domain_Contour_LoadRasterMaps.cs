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
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using GralIO;
using GralMessage;

namespace GralDomain
{
    public partial class Domain
	{
		/// <summary>
        /// Load and create contour maps
        /// </summary>
		public void Contours(string file, DrawingObjects _drobj)
		{
			CultureInfo ic = CultureInfo.InvariantCulture;
			int nodata = -9999;

			if (ReDrawContours == true) // new contour line build blocked?
			{
				MessageWindow message = new MessageWindow();
				message.Show();
				message.listBox1.Items.Add("Compute contour lines...");
				message.Refresh();
				message.listBox1.Items.Add("Reading data...");
				Application.DoEvents();
				
				int nx = 0;	int ny = 0;
				double xll = 0;	double yll = 0;	double dx = 0;
				double [,] zlevel = new double[1,1];
				double[,] zlevelfilter = new double[1, 1];
				bool evaluate_terrain = false;

                try
				{
					if (file.EndsWith ("scl"))  // Stability class reader
					{
                        ReadSclUstOblClasses reader = new ReadSclUstOblClasses
                        {
                            FileName = file,
                            Stabclasses = zlevel
                        };

                        if (reader.ReadSclFile()) // true => reader = OK
						{
							zlevel = reader.Stabclasses;
                            // read NX and NY from ggeom.asc
                            GGeomFileIO ggeom = new GGeomFileIO
                            {
                                PathWindfield = Path.GetDirectoryName(MainForm.GRAMMwindfield)
                            };
                            if (ggeom.ReadGGeomAsc(2) == true)
							{
								nx = ggeom.NX;
								ny = ggeom.NY;
							}
							else
                            {
                                throw new FileNotFoundException("Error reading ggeom.asc");
                            }

                            ggeom = null;
							
							xll = MainForm.GrammDomRect.West;
							yll = MainForm.GrammDomRect.South;
							dx = MainForm.GRAMMHorGridSize;

							zlevelfilter = new double[nx, ny];
						}
						else
						{
							throw new FileNotFoundException("Error");
						}
					}
					else // Textfile reader
					{
						Cursor = Cursors.WaitCursor;

						// Read ESRI file
						GralIO.ReadESRIFile readESRIFile = new GralIO.ReadESRIFile();
#if NET6_0_OR_GREATER
						(zlevelfilter, GralIO.ESRIHeader header, double min, double max, string exception) = readESRIFile.ReadESRIFileMultiDim(file);
#else
						double min = 0, max = 0;
						string exception = string.Empty;
						GralIO.ESRIHeader header = new GralIO.ESRIHeader();
						zlevelfilter = readESRIFile.ReadESRIFileMultiDim(file, ref header, ref min, ref max, ref exception);
#endif
						if (zlevelfilter == null)
						{
							throw new IOException(exception);
						}

						if (!_drobj.Filter)
                        {
							zlevel = zlevelfilter;
                        }
						else
                        {
							zlevel = new double[header.NCols, header.NRows];
                        }
						nx = header.NCols;
						ny = header.NRows;
						xll = header.XllCorner;
						yll = header.YllCorner;
						dx = header.Cellsize;

						List<PointF> VTC_List = new List<PointF>();
                        if (header.VerticalConcentrationMap) // Read if map is a vertical concentration profile
                        {
                            PointF pt1 = new PointF((float)header.XllCorner, (float)header.YllCorner);
                            VTC_List.Add(pt1);
                            PointF pt2 = new PointF((float)header.Cellsize, (float)header.NoDataValue);
                            VTC_List.Add(pt2);
                            PointF pt3 = new PointF((float)header.NCols, (float)header.NRows);
                            VTC_List.Add(pt3);
                            PointF pt4 = new PointF(header.MaxHeight, 0);
                            VTC_List.Add(pt4);
                            _drobj.ShpPoints = VTC_List;
                            evaluate_terrain = true;
                        }

                        if (evaluate_terrain) // evaluate terrain for vertical concentration maps
						{
							for (int j = 0; j < nx; ++j)
							{
								int i = 0;
								int terrain_index = 0;
								while (i < ny - 1)
								{
									double value = 0;
									if (_drobj.Filter == true)
                                    {
                                        value = zlevelfilter[j, i];
                                    }
                                    else
                                    {
                                        value = zlevel[j, i];
                                    }

                                    if (Math.Abs (value - nodata) > 0.1) // Value != NODATA
									{
										terrain_index = i;
										i = ny; // break
									}
									i++;
								}

								float _xh = (float) (xll + (j) * dx); // coordintaes of this terrain point
								float _yh = (float) (yll - (ny - terrain_index) * dx);
								PointF pt_x = new PointF(_xh, _yh);
								VTC_List.Add (pt_x);
							}
							
							_drobj.ShpPoints = VTC_List; // add terrain data to shppoints
							//MessageBox.Show (shppoints[index][12].X.ToString () + "/" + shppoints[index][12].Y.ToString () );
							//MessageBox.Show (shppoints[index][5].X.ToString () + "/" + shppoints[index][5].Y.ToString () );
						} // evaluate terrain

					}
				}
				catch
				{
					MessageBox.Show(this, "Unable to open, read or process the data","Process raster data",MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
					message.Close();
					Cursor = Cursors.Default;
					//picturebox1_Paint();
					return;
				}

                bool[,] buildingsindex = new bool[nx, ny];
                if (_drobj.Filter == true)
                {
                    ReadBuildingIndex(buildingsindex, zlevelfilter, dx, xll, yll, nx, ny);
                }
                else
                {
                    ReadBuildingIndex(buildingsindex, zlevel, dx, xll, yll, nx, ny);
                }

                //apply low pass filter
                if (_drobj.Filter == true)
				{
					message.listBox1.Items.Add("Applying low pass filter...");
					Application.DoEvents();
					LowPassFilter(zlevel, zlevelfilter, buildingsindex, nx, ny, dx, xll, yll, nodata);				
				} 
				// no low-pass - set nodata values to 0 for vertical concentration maps
				else if (evaluate_terrain) 
				{
					Parallel.For (0, nx, i => {
						for (int j = 0; j < ny; ++j)
						{
							if (Math.Abs(zlevel[i, j] - nodata) < 0.001)
							{
								zlevel[i, j] = 0;
							}
						}
					});
				}

				message.listBox1.Items.Add("Adding geometry...");
                button56.Visible = true;
				Application.DoEvents();
				
				//clear actual contourpoints
				_drobj.ContourPoints.Clear();
				
				//add a Point-List for each user defined level
				for (int k = 0; k < _drobj.ItemValues.Count; ++k)
				{
					_drobj.ContourPoints.Add(new List<PointF>());
					_drobj.ContourPoints[k].Clear();
				}

				//save geometry information of the raster grid
				_drobj.ContourGeometry = new DrawingObjects.ContourGeometries(xll, yll, dx, nx, ny);

				// simple_contour algorithm by M.Kuntner
				if (_drobj.DrawSimpleContour)
				{
				    message.listBox1.Items.Add("Computing polygons...");
					Application.DoEvents();
					
				    SimpleContour(zlevel, nx, ny, xll, yll, dx, message, _drobj, buildingsindex);
				    message.Close();
				    Cursor = Cursors.Default;
				    ReDrawContours = false;
				    return;
				}
				
				// delete Contour Polygons, created by SimpleContour()
				if (_drobj.ContourPolygons != null)
				{
					_drobj.ContourPolygons.Clear();
					_drobj.ContourPolygons.TrimExcess();
				}
				
				//define integer field for fill colors
				if (_drobj.Fill == true)
				{
					// create new index if array == null or array dimension != nx or ny
					if (_drobj.ContourColor == null || _drobj.ContourColor.GetUpperBound(0) != (nx - 1) || _drobj.ContourColor.GetUpperBound(1) != (ny - 1))
					{
						//MessageBox.Show(contourcolor[index].GetUpperBound(0).ToString() + "/" + contourcolor[index].GetUpperBound(1).ToString() + "/" + nx.ToString() + "/" + ny.ToString());
						_drobj.ContourColor = new int[nx, ny];
					}
					
					Parallel.For(0, nx, i =>
		             {
		             	for (int j = 0; j < ny; ++j)
		             	{
		             		_drobj.ContourColor[i, j] = -1;
		             		for (int k = _drobj.ItemValues.Count - 1; k >= 0; --k)
		             		{
		             			if (zlevel[i, j] > _drobj.ItemValues[k])
		             			{
		             				_drobj.ContourColor[i, j] = k;
		             				break;
		             			}
		             		}
		             	}
		             });
				}

				if (_drobj.LineWidth > 0)
				{
					//compute contour polygons according to Paul D. Bourke
					//
					//     Local declarations
					//
					message.listBox1.Items.Add("Computing polygons...");
					Application.DoEvents();
					
					bool largeArray = false;
					if ((nx * ny) > 200000)
                    {
                        largeArray = true; // >200.000 raster cells -> draw contour lines faster, reduce vertices
                    }

                    int point_counter = 0;
					List<List<RectangleF>> unsorted = new List<List<RectangleF>>();   // unsorted lines
					for (int k = 0; k < _drobj.ItemValues.Count; k++)
					{
						unsorted.Add(new List<RectangleF>());
					}
					
					Object thisLock = new Object();
					
					int[] im = { 0, 1, 1, 0 };
					int[] jm = { 0, 0, 1, 1 };
					int[, ,] castab = new int[,,] { { { 0, 0, 8 }, { 0, 2, 5 }, { 7, 6, 9 } }, { { 0, 3, 4 }, { 1, 3, 1 }, { 4, 3, 0 } }, { { 9, 6, 7 }, { 5, 2, 0 }, { 8, 0, 0 } } };
					double item_max = _drobj.ItemValues[0];
					double item_min = _drobj.ItemValues[_drobj.ItemValues.Count - 1];
					Application.DoEvents();

					int yOffset = ny - 1;
					if (CheckForEmptyLastRow(zlevel, nx, ny))
					{
						yOffset = ny - 2;
					}

					Parallel.For(0, yOffset , j =>
		            {
		             	float[] h = new float[5];
		             	int[] sh = new int[5];
		             	float[] xh = new float[5];
		             	float[] yh = new float[5];
		             	byte m1;
		             	byte m2;
		             	byte m3;
		             	int case_value;
		             	float dmin;
		             	float dmax;
		             	float x1 = 0.0F;
		             	float x2 = 0.0F;
		             	float y1 = 0.0F;
		             	float y2 = 0.0F;
		             	float temp;

		             	//
		             	// scan the arrays, top down, left to right within rows
		             	//
		             	//Application.DoEvents();
		             	for (int i = 0; i < nx - 2; i++)
		             	{
		             		dmin = (float) Math.Min(Math.Min(zlevel[i, j], zlevel[i, j + 1]), Math.Min(zlevel[i + 1, j], zlevel[i + 1, j + 1]));
		             		dmax = (float) Math.Max(Math.Max(zlevel[i, j], zlevel[i, j + 1]), Math.Max(zlevel[i + 1, j], zlevel[i + 1, j + 1]));
		             		if ((dmax >= item_max) && (dmin <= item_min))
		             		{
		             			for (int k = 0; k < _drobj.ItemValues.Count; k++)
		             			{
		             				double contour_value = _drobj.ItemValues[k];
		             				
		             				if ((contour_value >= dmin) && (contour_value <= dmax))
		             				{
		             					byte start;
		             					byte anz;
		             					if (!largeArray) // small arrays - 4 triangles
		             					{
		             						for (Int16 m = 4; m > -1; m--)
		             						{
		             							if (m > 0)
		             							{
		             								h[m] = (float) (zlevel[i + im[m - 1], j + jm[m - 1]] - contour_value);
		             								xh[m] = (float) (xll + (i + im[m - 1]) * dx);
		             								yh[m] = (float) (yll + (j + jm[m - 1]) * dx);
		             							}
		             							else
		             							{
		             								h[0] = 0.25F * (h[1] + h[2] + h[3] + h[4]);
		             								xh[0] = (float) (0.5F * (xll + (i) * dx + xll + (i + 1) * dx));
		             								yh[0] = (float) (0.5F * (yll + (j) * dx + yll + (j + 1) * dx));
		             							}
		             							if (h[m] > 0)
                                                 {
                                                     sh[m] = 2;
                                                 }
                                                 else if (h[m] < 0)
                                                 {
                                                     sh[m] = 0;
                                                 }
                                                 else
                                                 {
                                                     sh[m] = 1;
                                                 }
                                             }
		             						anz = 5;
		             						start = 1;
		             					}
		             					else // large aray -faster contour draw 2 triangles
		             					{
		             						for (byte m = 0; m < im.Length; m++)
		             						{
		             							h[m] = (float) (zlevel[i + im[m], j + jm[m]] - contour_value);
		             							xh[m] = (float) (xll + (i + im[m]) * dx);
		             							yh[m] = (float) (yll + (j + jm[m]) * dx);
		             							
		             							if (h[m] > 0)
                                                 {
                                                     sh[m] = 2;
                                                 }
                                                 else if (h[m] < 0)
                                                 {
                                                     sh[m] = 0;
                                                 }
                                                 else
                                                 {
                                                     sh[m] = 1;
                                                 }
                                                 //Note: at this stage the relative heights of the corners are in the h array, and the corresponding coordinates are
                                                 //in the xh and yh arrays. The 4 corners are indexed by 0 to 3 as shown below.
                                                 //Each triangle is then indexed by the parameter m, and the 3
                                                 //vertices of each triangle are indexed by parameters m1,m2,and m3.

                                                 //vertex 3 +-------------------+ vertex 2
                                                 //|                 / |
                                                 //|               /   |
                                                 //|             /     |
                                                 //|           /       |
                                                 //|  m=1    /         |
                                                 //|       /           |
                                                 //|     /             |
                                                 //|   /    m=0        |
                                                 //| /                 |
                                                 //vertex 0 +-------------------+ vertex 1
                                                 //  */
                                             }
                                             anz = 2;
		             						start = 0;
		             					}
		             					
		             					//
		             					// Note: at this stage the relative heights of the corners and the
		             					// centre are in the h array, and the corresponding coordinates are
		             					// in the xh and yh arrays. The centre of the box is indexed by 0
		             					// and the 4 corners by 1 to 4 as shown below.
		             					// Each triangle is then indexed by the parameter m, and the 3
		             					// vertices of each triangle are indexed by parameters m1,m2,and
		             					// m3.
		             					// It is assumed that the centre of the box is always vertex 2
		             					// though this isimportant only when all 3 vertices lie exactly on
		             					// the same contour level, in which case only the side of the box
		             					// is drawn.
		             					//
		             					//
		             					//      vertex 4 +-------------------+ vertex 3
		             					//               | \               / |
		             					//               |   \    m-3    /   |
		             					//               |     \       /     |
		             					//               |       \   /       |
		             					//               |  m=2    X   m=2   |       the centre is vertex 0
		             					//               |       /   \       |
		             					//               |     /       \     |
		             					//               |   /    m=1    \   |
		             					//               | /               \ |
		             					//      vertex 1 +-------------------+ vertex 2
		             					//
		             					//
		             					//
		             					//               Scan each triangle in the box
		             					//
		             					m1 = 0; m2 = 1;	m3 = 2;
		             					for (byte m = start; m < anz; m++)
		             					{
		             						if (largeArray == false)
		             						{
		             							m1 = m;
		             							m2 = 0;
		             							if (m != 4)
		             							{
		             								m3 = m;
		             								m3++;
		             							}
		             							else
                                                 {
                                                     m3 = 1;
                                                 }
                                             }
		             						else
		             						{
		             							if (m == 1)
		             							{
		             								m1 = 2; m2 = 3; m3 = 0 ;
		             							}
		             						}
		             						
		             						case_value = castab[sh[m1], sh[m2], sh[m3]];
		             						if (case_value != 0)
		             						{
		             							switch (case_value)
		             							{
		             								case 1: //line between vertices 1 and 2
		             									x1 = xh[m1];
		             									y1 = yh[m1];
		             									x2 = xh[m2];
		             									y2 = yh[m2];
		             									break;
		             								case 2: //line between vertices 2 and 3
		             									x1 = xh[m2];
		             									y1 = yh[m2];
		             									x2 = xh[m3];
		             									y2 = yh[m3];
		             									break;
		             								case 3: //line between vertices 3 and 1
		             									x1 = xh[m3];
		             									y1 = yh[m3];
		             									x2 = xh[m1];
		             									y2 = yh[m1];
		             									break;
		             								case 4: //line between vertex 1 and side 2-3
		             									x1 = xh[m1];
		             									y1 = yh[m1];
		             									temp = 1 / (h[m3] - h[m2]);
		             									x2 = (h[m3] * xh[m2] - h[m2] * xh[m3]) * temp; //xsect(m2, m3, h, xh);
		             									y2 = (h[m3] * yh[m2] - h[m2] * yh[m3]) * temp; //ysect(m2, m3, h, yh);
		             									break;
		             								case 5: //line between vertex 2 and side 3-1
		             									x1 = xh[m2];
		             									y1 = yh[m2];
		             									temp = 1 / (h[m1] - h[m3]);
		             									x2 = (h[m1] * xh[m3] - h[m3] * xh[m1]) * temp; //xsect(m3, m1, h, xh);
		             									y2 = (h[m1] * yh[m3] - h[m3] * yh[m1]) * temp; //ysect(m3, m1, h, yh);
		             									break;
		             								case 6: //line between vertex 3 and side 1-2
		             									x1 = xh[m3];
		             									y1 = yh[m3];
		             									temp = 1 / (h[m2] - h[m1]);
		             									x2 = (h[m2] * xh[m1] - h[m1] * xh[m2]) * temp; //xsect(m1, m2, h, xh);
		             									y2 = (h[m2] * yh[m1] - h[m1] * yh[m2]) * temp; //ysect(m1, m2, h, yh);
		             									break;
		             								case 7: //line between sides 1-2 and 2-3
		             									temp  = 1 / (h[m2] - h[m1]);
		             									x1 = (h[m2] * xh[m1] - h[m1] * xh[m2]) * temp; //xsect(m1, m2, h, xh);
		             									y1 = (h[m2] * yh[m1] - h[m1] * yh[m2]) * temp; //ysect(m1, m2, h, yh);
		             									temp = 1 / (h[m3] - h[m2]);
		             									x2 = (h[m3] * xh[m2] - h[m2] * xh[m3]) * temp; //xsect(m2, m3, h, xh);
		             									y2 = (h[m3] * yh[m2] - h[m2] * yh[m3]) * temp; //ysect(m2, m3, h, yh);
		             									break;
		             								case 8: //line between sides 2-3 and 3-1
		             									temp = 1 / (h[m3] - h[m2]);
		             									x1 = (h[m3] * xh[m2] - h[m2] * xh[m3]) * temp; //xsect(m2, m3, h, xh);
		             									y1 = (h[m3] * yh[m2] - h[m2] * yh[m3]) * temp; //ysect(m2, m3, h, yh);
		             									temp =  1 / (h[m1] - h[m3]);
		             									x2 = (h[m1] * xh[m3] - h[m3] * xh[m1]) * temp; //xsect(m3, m1, h, xh);
		             									y2 = (h[m1] * yh[m3] - h[m3] * yh[m1]) * temp; //ysect(m3, m1, h, yh);
		             									break;
		             								case 9: //line between sides 3-1 and 1-2
		             									temp = 1 / (h[m1] - h[m3]);
		             									x1 = (h[m1] * xh[m3] - h[m3] * xh[m1]) * temp; //xsect(m3, m1, h, xh);
		             									y1 = (h[m1] * yh[m3] - h[m3] * yh[m1]) * temp; //ysect(m3, m1, h, yh);
		             									temp =  1 / (h[m2] - h[m1]);
		             									x2 = (h[m2] * xh[m1] - h[m1] * xh[m2]) * temp; //xsect(m1, m2, h, xh);
		             									y2 = (h[m2] * yh[m1] - h[m1] * yh[m2]) * temp; //ysect(m1, m2, h, yh);
		             									break;
		             								default:
		             									break;
		             							}
		             							
		             							RectangleF rect = new RectangleF(x1, y1, x2, y2);
		             							lock (thisLock) // Kuntner .Add must be locked
		             							{
		             								unsorted[k].Add(rect);
//														_drobj.ContourPoints[k].Add(new PointF((float)x1, (float)y1));
//														_drobj.ContourPoints[k].Add(new PointF((float)x2, (float)y2));
		             							}
		             							Interlocked.Increment(ref point_counter);
		             						}
		             					}
		             				}
		             			}
		             		}
		             	}
		             });
					
					thisLock = null;
					
					//MessageBox.Show(Convert.ToString(point_counter)+"/" +Convert.ToString(nx * ny));
					List<List<PointF>> _contPts = _drobj.ContourPoints;
					if (point_counter < 400000) // less than 400.000 line segments
					{
						message.listBox1.Items.Add("Sort polygons...");
						Application.DoEvents();

                        // conrec done! now sort and filter the lines and write to counterpoints()
                        Parallel.For(0, _drobj.ItemValues.Count, k =>
                        {
                             int firstline = 0;
                             double area = 0; // area of this contour line
                             int newpolygoncounter = 0;
                             firstline = Math.Max(0, Math.Min(k * 2, unsorted[k].Count - 1)); // move first point
                                                                                              //Application.DoEvents();

                             if (unsorted[k].Count > 0)
                             {
                                 //Application.DoEvents();
                                 while (unsorted[k].Count > 0)
                                 {
                                     // first point
                                     _contPts[k].Add(new PointF(unsorted[k][firstline].X, unsorted[k][firstline].Y));
                                     float xA = unsorted[k][firstline].Width;
                                     float yA = unsorted[k][firstline].Height;
                                     float xf = xA; // first point of a contour
                                     float yf = yA;
                                     _contPts[k].Add(new PointF(xA, yA));
                                     area += ((xA - unsorted[k][firstline].X) * unsorted[k][firstline].Y + (xA - unsorted[k][firstline].X) * (yA - unsorted[k][firstline].Y) / 2);
                                     unsorted[k].RemoveAt(firstline);

                                     int i = 0;
                                     while (i < unsorted[k].Count)
                                     {
										 RectangleF _unsorted = unsorted[k][i];
										 if (Math.Abs(xA - _unsorted.X) < 0.01 && Math.Abs(yA - _unsorted.Y) < 0.01)
                                         {
                                             _contPts[k].Add(new PointF(_unsorted.X, _unsorted.Y));
                                             xA = _unsorted.Width;
                                             yA = _unsorted.Height;
                                             _contPts[k].Add(new PointF(xA, yA));
                                             area += ((xA - _unsorted.X) * _unsorted.Y + (xA - _unsorted.X) * (yA - _unsorted.Y) / 2);

                                             unsorted[k].RemoveAt(i);
                                             if (Math.Abs(xA - xf) < 0.01 && Math.Abs(yA - yf) < 0.01) // contour line closed
                                             {
                                                 i = unsorted[k].Count; // start new contour line
                                             }
                                             else
                                             {
                                                 i = -1; // start searching
                                             }
                                         }
                                         else if (Math.Abs(xA - _unsorted.Width) < 0.01 && Math.Abs(yA - _unsorted.Height) < 0.01)
                                         {
                                             _contPts[k].Add(new PointF(_unsorted.Width, _unsorted.Height)); // reverse order in list
                                             xA = _unsorted.X;
                                             yA = _unsorted.Y;
                                             _contPts[k].Add(new PointF(xA, yA));
                                             area += ((xA - _unsorted.Width) * _unsorted.Height + (xA - _unsorted.Width) * (yA - _unsorted.Height) / 2);

                                             unsorted[k].RemoveAt(i);
                                             if (Math.Abs(xA - xf) < 0.01 && Math.Abs(yA - yf) < 0.01) // contour line closed
                                             {
                                                 i = unsorted[k].Count; // start new contour line
                                             }
                                             else
                                             {
                                                 i = -1;// start searching
                                             }
                                         }

                                         i++;
                                     }

                                     firstline = 0;
                                     // delete small contour - areas

                                     if (Math.Abs(area) < _drobj.ContourAreaMin)
                                     {
                                         _contPts[k].RemoveRange(newpolygoncounter, _contPts[k].Count - newpolygoncounter);
                                     }
                                     newpolygoncounter = _contPts[k].Count;
                                     area = 0; // new polygon
                                 }
                             }
                        });
                    }
					else // do not sort and filter > 400.000 line segments!
					{
						//for (int k = 0; k < itemvalue[index].Count; k++)
						Parallel.For(0, _drobj.ItemValues.Count, k =>
			             {
			             	for (int i = 0; i < unsorted[k].Count ; i++)
			             	{
			             		_contPts[k].Add(new PointF(unsorted[k][i].X, unsorted[k][i].Y));
			             		_contPts[k].Add(new PointF(unsorted[k][i].Width, unsorted[k][i].Height));
			             	}
			             	
			             });
					}
				}
				
				message.Close();
				Cursor = Cursors.Default;
				//picturebox1_Paint();

			}
			ReDrawContours = false;
		}

		//sub routine needed for contour mapping
		readonly Func<int, int, double[], double[], double> xsect = (int p1, int p2, double[] h, double[] xh) =>
			(h[p2] * xh[p1] - h[p1] * xh[p2]) / (h[p2] - h[p1]);

		//sub routine needed for contour mapping
		readonly Func<int, int, double[], double[], double> ysect = (int p1, int p2, double[] h, double[] yh) =>
			(h[p2] * yh[p1] - h[p1] * yh[p2]) / (h[p2] - h[p1]);

        /// <summary>
        ///Read building.dat and set all building cells with concentration 0 to true
        /// </summary>
        private void ReadBuildingIndex(bool[,] buildingsindex, double[,] zlevel, double dx, double x11, double y11, int nx, int ny)
        {
            //read file buildings.dat which is necessary to exclude cells within buildings
            try
            {
                string[] text1 = new string[15];
                using (StreamReader myReader = new StreamReader(Path.Combine(Gral.Main.ProjectName, @"Computation", "buildings.dat")))
                {
                    String dummy;
                    double x1;
                    double y1;
                    while ((dummy = myReader.ReadLine()) != null)
                    {                      
                        try
                        {
                            text1 = dummy.Split(new char[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            x1 = Convert.ToDouble(text1[0], ic);
                            y1 = Convert.ToDouble(text1[1], ic);
                            int i = Convert.ToInt32(Math.Truncate((x1 - x11) / dx) );
                            int j = Convert.ToInt32(Math.Truncate((y1 - y11) / dx) );
                                                  
                            if (i > 0 && j > 0 && i < nx && j < ny && zlevel[i, j] < float.Epsilon)
                            { 
                                buildingsindex[i, j] = true;
                            }
                        }
                        catch
                        { }
                    }
                }
            }
            catch { }
        }

		/// <summary>
        /// Apply a low pass filter to zlevel for contour lines and buildings
        /// </summary>
		private void LowPassFilter(double[,] zlevel, double[,] zlevelfilter, bool[,] buildingsindex, int nx, int ny, double dx, double x11, double y11, int nodata)
		{		
			Parallel.For(0, nx , i =>
             {
             	int i_p1 = i + 1;
             	int i_p2 = i + 2;
             	int i_m1 = i - 1;
             	int i_m2 = i - 2;
             	for (int j = 0; j < ny; ++j)
             	{
					if (i > 1 && j > 1 && i < nx -2 && j < ny - 2)
					{
						if (((buildingsindex[i, j] != true) || (zlevelfilter[i, j] != 0)) && Math.Abs(zlevelfilter[i, j] - nodata) > 0.001)
	             		{
	             			int j_p1 = j + 1;
	             			int j_p2 = j + 2;
	             			int j_m1 = j - 1;
	             			int j_m2 = j - 2;
							double _zlevel = 0;
	  					    
							double weighting_factor = 0;
							double _val = zlevelfilter[i_m2, j_p2];
							if (((buildingsindex[i_m2, j_p2] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001 )
							{
								_zlevel = 0.354 * _val;
								weighting_factor += 0.354;
							}
							_val = zlevelfilter[i_m2, j_m2];
							if (((buildingsindex[i_m2, j_m2] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.354 * _val;
								weighting_factor += 0.354;
							}
							_val = zlevelfilter[i_p2, j_p2];
							if (((buildingsindex[i_p2, j_p2] != true)  || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.354 * _val;
								weighting_factor += 0.354;
							}
							_val = zlevelfilter[i_p2, j_m2];
							if (((buildingsindex[i_p2, j_m2] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.354 * _val;
								weighting_factor += 0.354;
							}
							_val = zlevelfilter[i_m2, j_p1];
							if (((buildingsindex[i_m2, j_p1] != true)  || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.447 * _val;
								weighting_factor += 0.447;
							}
							_val = zlevelfilter[i_m2, j_m1];
							if (((buildingsindex[i_m2, j_m1] != true)  || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.447 * _val;
								weighting_factor += 0.447;
							}
							_val = zlevelfilter[i_p2, j_p1];
							if (((buildingsindex[i_p2, j_p1] != true)  || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.447 * _val;
								weighting_factor += 0.447;
							}
							_val = zlevelfilter[i_p2, j_m1];
							if (((buildingsindex[i_p2, j_m1] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.447 * _val;
								weighting_factor += 0.447;
							}
							_val = zlevelfilter[i_m1, j_p2];
							if (((buildingsindex[i_m1, j_p2] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.447 *_val;
								weighting_factor += 0.447;
							}
							_val = zlevelfilter[i_m1, j_m2];
							if (((buildingsindex[i_m1, j_m2] != true)  || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.447 * _val;
								weighting_factor += 0.447;
							}
							_val = zlevelfilter[i_p1, j_p2];
							if (((buildingsindex[i_p1, j_p2] != true)  || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.447 * _val;
								weighting_factor += 0.447;
							}
							_val = zlevelfilter[i_p1, j_m2];
							if (((buildingsindex[i_p1, j_m2] != true)  || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.447 * _val;
								weighting_factor += 0.447;
							}
							_val = zlevelfilter[i, j_p2];
							if (((buildingsindex[i, j_p2] != true)  || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.5 * _val;
								weighting_factor += 0.5;
							}
							_val = zlevelfilter[i, j_m2];
							if (((buildingsindex[i, j_m2] != true)  || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.5 * _val;
								weighting_factor += 0.5;
							}
							_val = zlevelfilter[i_p2, j];
							if (((buildingsindex[i_p2, j] != true)  || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.5 * _val;
								weighting_factor += 0.5;
							}
							_val = zlevelfilter[i_m2, j];
							if (((buildingsindex[i_m2, j] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.5 * _val;
								weighting_factor += 0.5;
							}
							_val = zlevelfilter[i_m1, j];
							if (((buildingsindex[i_m1, j] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 1.0 * _val;
								weighting_factor += 1.0;
							}
							_val = zlevelfilter[i_p1, j];
							if (((buildingsindex[i_p1, j] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 1.0 * _val;
								weighting_factor += 1.0;
							}
							_val = zlevelfilter[i, j_p1];
							if (((buildingsindex[i, j_p1] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 1.0 * _val;
								weighting_factor += 1.0;
							}
							_val = zlevelfilter[i, j_m1];
							if (((buildingsindex[i, j_m1] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 1.0 * _val;
								weighting_factor += 1.0;
							}
							_val = zlevelfilter[i_m1, j_p1];
							if (((buildingsindex[i_m1, j_p1] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.707 * _val;
								weighting_factor += 0.707;
							}
							_val = zlevelfilter[i_p1, j_p1];
							if (((buildingsindex[i_p1, j_p1] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.707 * _val;
								weighting_factor += 0.707;
							}
							_val = zlevelfilter[i_m1, j_m1];
							if (((buildingsindex[i_m1, j_m1] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.707 * _val;
								weighting_factor += 0.707;
							}
							_val = zlevelfilter[i_p1, j_m1];
							if (((buildingsindex[i_p1, j_m1] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 0.707 * _val;
								weighting_factor += 0.707;
							}
							_val = zlevelfilter[i, j];
							if (((buildingsindex[i, j] != true) || (_val > 0)) && Math.Abs(_val - nodata) > 0.001)
							{
								_zlevel += 2.0 * _val;
								weighting_factor += 2.0;
							}

							zlevel[i, j] = _zlevel / weighting_factor;
	             			
						}
						else
						{
							zlevel[i, j] = 0;
						}
					} // buildingsindex inside +- 2 cells 
					else
					{
							zlevel[i, j] = zlevelfilter[i, j];
					}
             	}
             });		    
		}
	}
}
