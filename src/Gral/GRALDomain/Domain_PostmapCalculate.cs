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
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GralDomain
{
    public partial class Domain
	{
		/// <summary>
        /// Routine computing postmap of a raster data set
        /// </summary>
		public void PostmapCalc(string file, DrawingObjects _drobj)
		{
			if (ReDrawContours == true)
			{
				try
				{
					string[] data = new string[100];
					//open data of raster file
					StreamReader myReader = new StreamReader(file);
					data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
					int nx = Convert.ToInt32(data[1]);
					data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
					int ny = Convert.ToInt32(data[1]);
					data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
					double x11 = Convert.ToDouble(data[1].Replace(".", decsep));
					data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
					double y11 = Convert.ToDouble(data[1].Replace(".", decsep));
					data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
					double dx = Convert.ToDouble(data[1].Replace(".", decsep));
					data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
					int nodata = Convert.ToInt32(data[1]);
					data = new string[nx];
					double[,] zlevel = new double[nx, ny];
					double min = 100000000;
					double max = -100000000;
					for (int i = ny - 1; i > -1; i--)
					{
						data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
						for (int j = 0; j < nx; j++)
						{
							zlevel[j, i] = Convert.ToDouble(data[j].Replace(".", decsep));
							if (Convert.ToInt32(zlevel[j, i]) != nodata)
							{
								min = Math.Min(min, zlevel[j, i]);
								max = Math.Max(max, zlevel[j, i]);
							}
						}
					}
					myReader.Close();
					myReader.Dispose();

					//clear actual contourpoints
					_drobj.ContourPoints.Clear();
					_drobj.ContourPolygons.Clear();

					//save geometry information of the raster grid
					_drobj.ContourGeometry = new DrawingObjects.ContourGeometries(x11, y11, dx, nx, ny);

                    //define postmap levels
                    _drobj.ItemValues[0] = min;
					_drobj.ItemValues[9] = max;
					//apply color gradient between light green and red
					int r1 = _drobj.FillColors[0].R;
					int g1 = _drobj.FillColors[0].G;
					int b1 = _drobj.FillColors[0].B;
					int r2 = _drobj.FillColors[9].R;
					int g2 = _drobj.FillColors[9].G;
					int b2 = _drobj.FillColors[9].B;
					for (int i = 0; i < 8; i++)
					{
						_drobj.ItemValues[i + 1] = min + (i + 1) * (max - min) / 9;
						int intr = r1 + (r2 - r1) / 9 * (i + 1);
						int intg = g1 + (g2 - g1) / 9 * (i + 1);
						int intb = b1 + (b2 - b1) / 9 * (i + 1);
						_drobj.FillColors[i + 1] = Color.FromArgb(intr, intg, intb);
						_drobj.LineColors[i + 1] = Color.FromArgb(intr, intg, intb);
					}

					//define integer field for fill colors
					if (_drobj.Fill == true)
					{
						// create new index if array == null or array dimension != nx or ny
						if (_drobj.ContourColor == null || _drobj.ContourColor.GetUpperBound(0) != (nx - 1) || _drobj.ContourColor.GetUpperBound(1) != (ny - 1))
						{
							_drobj.ContourColor = new int[nx, ny];
						}
						int[,] _contcol = _drobj.ContourColor;
						Parallel.For(0, nx, i =>
			             {
			             	for (int j = 0; j < ny; j++)
			             	{
			             		_drobj.ContourColor[i, j] = -1;
			             		for (int k = _drobj.ItemValues.Count - 1; k > -1; k--)
			             		{
			             			if (zlevel[i, j] > _drobj.ItemValues[k])
			             			{
			             				_contcol[i, j] = k;
			             				break;
			             			}
			             		}
			             	}
			             });
					}

				}
				catch
				{
					if (GRAMMOnline == false)
                    {
                        MessageBox.Show(this, "Unable to open, read or process the data","GRAL GUI",MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                }
                if (!GRAMMOnline)
                {
                    Cursor = Cursors.Default;
                }
			}
			ReDrawContours = false;
		}

		/// <summary>
        /// Add a post map PM: to the drawing list
        /// </summary>
		public void AddPostMap(string file)
		{
			//add postmap map to object list
			DrawingObjects _drobj = new DrawingObjects("PM: " + Path.GetFileNameWithoutExtension(file));
			
			//compute values for 10 contours
			for (int i = 0; i < 10; i++)
			{
				_drobj.ItemValues.Add(0);
				_drobj.FillColors.Add(Color.Red);
				_drobj.LineColors.Add(Color.Red);
			}
			_drobj.FillColors[0] = Color.Yellow;
			_drobj.LineColors[0] = Color.Yellow;
			_drobj.ColorScale    = Convert.ToString(picturebox1.Width - 150) + "," + Convert.ToString(picturebox1.Height - 200) + "," + "1";
			_drobj.LegendTitle   = Path.GetFileNameWithoutExtension(file);
			_drobj.LegendUnit    = String.Empty;
			_drobj.ContourFilename = file;
			//
			//add list to save contourpoints
			//
			_drobj.Filter = true;
			
			ItemOptions.Insert(0, _drobj);
		}		
	}
}
