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
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// OpenFileDialog for contour maps and read map data from file
        /// </summary>
        private void CreateContourMap(string file)
        {
            CultureInfo ic = CultureInfo.InvariantCulture;

            if (file.Length < 1)
            {
                using (OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "(*.dat;*.txt)|*.dat;*.txt",
                    Title = "Select raster data (ASCII Format)",
                    InitialDirectory = Path.Combine(Gral.Main.ProjectName, "Maps" + Path.DirectorySeparatorChar)
                })
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        file = dialog.FileName;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (File.Exists(file))
            {
                try
                {
                    Cursor = Cursors.WaitCursor;

                    //open ASCII Raster file format
                    string[] data = new string[100];
                    //open data of raster file
                    StreamReader myReader = new StreamReader(file);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    int nx = Convert.ToInt32(data[1]);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    int ny = Convert.ToInt32(data[1]);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    double x11 = Convert.ToDouble(data[1], ic);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    double y11 = Convert.ToDouble(data[1], ic);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    double dx = Convert.ToDouble(data[1], ic);
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    string unit = "";
                    int nodata = Convert.ToInt32(data[1]);
                    if (data.Length > 3) // read unit, if available
                    {
                        unit = data[3].Trim();
                    }

                    bool Vertical_Concentration = false;
                    if (data.Length > 4) // Read if map is a vertical concentration profile
                    {
                        if (data[data.Length - 3].Contains("Vertical_concentration"))
                        {
                            Vertical_Concentration = true;
                        }
                    }

                    data = new string[nx];
                    double min = 100000000;
                    double max = -100000000;
                    double[,] zlevel = new double[nx, ny];
                    for (int i = ny - 1; i > -1; i--)
                    {
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int j = 0; j < nx; j++)
                        {
                            zlevel[j, i] = Convert.ToDouble(data[j], ic);
                            if ((Int32)(zlevel[j, i]) != nodata)
                            {
                                min = Math.Min(min, zlevel[j, i]);
                                max = Math.Max(max, zlevel[j, i]);
                            }
                        }
                    }
                    myReader.Close();
                    myReader.Dispose();

                    if (Math.Abs(max - min) < 0.000000000001)
                    {
                        MessageBox.Show(this, "Blank raster dataset", "Process raster data", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        Cursor = Cursors.Default;
                        return;
                    }

                    //add contour map to object list
                    const int cellsizemax = 200000;

                    DrawingObjects _drobj = new DrawingObjects("CM: " + Path.GetFileNameWithoutExtension(file));

                    if (Path.GetFileName(file) == "building_heights.txt" || Path.GetFileName(file).Contains("steady_state.txt")
                        || Path.GetFileName(file).Contains("TPI_STDI.txt")
                        || Path.GetFileName(file).Contains("TPI_SlopeMax.txt")
                        || Path.GetFileName(file).Contains("TPI_SlopeMin.txt")
                        || Path.GetFileName(file).Contains("TPI_Base.txt")
                        || (nx * ny) > cellsizemax) // set special settings for buildings, steady state files and large contour maps
                    {
                        _drobj.FillYesNo = true;
                    }
                    else
                    {
                        _drobj.FillYesNo = false;
                    }

                    if (Path.GetFileName(file) == "building_heights.txt") // set special settings for buildings
                    {
                        //compute values for 6 contours
                        for (int i = 0; i < 6; i++)
                        {
                            _drobj.ItemValues.Add(0);
                            _drobj.FillColors.Add(Color.Red);
                            _drobj.LineColors.Add(Color.Red);
                        }
                        _drobj.FillColors[0] = Color.Yellow;
                        _drobj.LineColors[0] = Color.Yellow;

                        // initial scale of contour map
                        for (int i = 0; i < 6; i++)
                        {
                            _drobj.ItemValues[i] = i * 3 + 3;
                            Color c = Color.FromArgb(255 - (i + 1) * 33, 255 - (i + 1) * 33, 255 - (i + 1) * 12);
                            _drobj.FillColors[i] = c;
                            _drobj.LineColors[i] = c;
                        }

                        _drobj.LegendTitle = "Buildings";
                        _drobj.LegendUnit = "m";
                        _drobj.Filter = false; // no filter
                        _drobj.LineWidth = 0; // no Lines
                    }
                    else if (Path.GetFileName(file).Contains("TPI_STDI.txt")) // set special settings for TPI Maps
                    {
                        //compute values for 10 contours
                        for (int i = 0; i < 9; i++)
                        {
                            _drobj.ItemValues.Add(0);
                            _drobj.FillColors.Add(Color.Red);
                            _drobj.LineColors.Add(Color.Red);
                        }
                        _drobj.FillColors[0] = Color.Yellow;
                        _drobj.LineColors[0] = Color.Yellow;
                        _drobj.FillYesNo = true;

                        // initial scale of contour map
                        for (int i = 0; i < 9; i++)
                        {
                            _drobj.ItemValues[i] = i;
                            Color c = Color.FromArgb(159, 19, 19);
                            if (i == 0)
                            {
                                c = Color.FromArgb(0, 255, 255);
                            }
                            else if (i == 1)
                            {
                                c = Color.FromArgb(0, 180, 180);
                            }
                            else if (i == 2)
                            {
                                c = Color.FromArgb(128, 128, 255);
                            }
                            else if (i == 3)
                            {
                                c = Color.FromArgb(128, 200, 255);
                                c = Color.FromArgb(16, 16, 16);
                            }
                            else if (i == 4)
                            {
                                c = Color.FromArgb(255, 255, 0);
                                c = Color.FromArgb(32, 32, 32);
                            }
                            else if (i == 5)
                            {
                                c = Color.FromArgb(255, 128, 64);
                            }
                            else if (i == 6)
                            {
                                c = Color.FromArgb(208, 67, 0);
                            }
                            else if (i == 7)
                            {
                                c = Color.FromArgb(235, 93, 93);
                            }
                            else if (i == 8)
                            {
                                c = Color.FromArgb(219, 26, 26);
                            }

                            _drobj.FillColors[i] = c;
                            _drobj.LineColors[i] = c;
                        }

                        _drobj.LegendTitle = "TPI Map";
                        _drobj.LegendUnit = "TPI";
                        _drobj.Filter = false; // no filter
                        _drobj.LineWidth = 0; // no Lines
                    }
                    else if (Path.GetFileName(file).Contains("steady_state.txt")) // set special settings for GRAMM steady state files
                    {
                        //compute values for 8 contours
                        for (int i = 0; i < 8; i++)
                        {
                            _drobj.ItemValues.Add(0);
                            _drobj.FillColors.Add(Color.Red);
                            _drobj.LineColors.Add(Color.Red);
                        }
                        _drobj.ItemValues[0] = -0.1;
                        _drobj.FillColors[0] = Color.Red;
                        _drobj.LineColors[0] = Color.Red;

                        // initial scale of contour map
                        for (int i = 1; i < 8; i++)
                        {
                            _drobj.ItemValues[i] = Math.Round(i - 0.1, 1);
                            Color c = Color.FromArgb(0, i * 32, 255 - i * 32);
                            _drobj.FillColors[i] = c;
                            _drobj.LineColors[i] = c;
                        }

                        _drobj.LegendTitle = "SteadyStateError";
                        _drobj.LegendUnit = "er";
                        _drobj.Filter = false; // no filter
                        _drobj.LineWidth = 0; // no Lines
                    }
                    else
                    {
                        //compute values for 9 contours
                        for (int i = 0; i < 9; i++)
                        {
                            _drobj.ItemValues.Add(0);
                            _drobj.FillColors.Add(Color.Red);
                            _drobj.LineColors.Add(Color.Red);
                        }

                        _drobj.FillColors[0] = Color.Yellow;
                        _drobj.LineColors[0] = Color.Yellow;

                        // initial scale of contour map
                        _drobj.ItemValues[0] = min + (max - min) / Math.Pow(2, Convert.ToDouble(8));
                        _drobj.ItemValues[8] = max;

                        //apply color gradient between light green and red
                        int r1 = _drobj.FillColors[0].R;
                        int g1 = _drobj.FillColors[0].G;
                        int b1 = _drobj.FillColors[0].B;
                        int r2 = _drobj.FillColors[8].R;
                        int g2 = _drobj.FillColors[8].G;
                        int b2 = _drobj.FillColors[8].B;
                        for (int i = 0; i < 7; i++)
                        {
                            _drobj.ItemValues[i + 1] = min + (max - min) / Math.Pow(2, Convert.ToDouble(8 - (i + 1)));
                            int intr = r1 + (r2 - r1) / 10 * (i + 1);
                            int intg = g1 + (g2 - g1) / 10 * (i + 1);
                            int intb = b1 + (b2 - b1) / 10 * (i + 1);
                            _drobj.FillColors[i + 1] = Color.FromArgb(intr, intg, intb);
                            _drobj.LineColors[i + 1] = Color.FromArgb(intr, intg, intb);
                        }

                        if (Vertical_Concentration)
                        {
                            _drobj.LegendTitle = "Vertical Concentration";
                        }
                        else
                        {
                            _drobj.LegendTitle = "Title";
                        }

                        if (unit.Length == 0) // unit not available from file -> loot to filename
                        {
                            string temp = Path.GetFileName(file).ToUpper();
                            if (temp.Contains("ODOUR"))
                            {
                                _drobj.LegendUnit = "%";
                            }
                            else if (temp.Contains("WINDSPEED") && temp.Contains("TXT"))
                            {
                                _drobj.LegendUnit = "m/s";
                            }
                            else if (temp.Contains("DEPOSITION"))
                            {
                                _drobj.LegendUnit = Gral.Main.mg_p_m2;
                            }
                            else if (temp.Contains("TPI_Base.txt"))
                            {
                                _drobj.LegendUnit = "m";
                            }
                            else if (temp.Contains("TPI_SlopeMin.txt") || temp.Contains("TPI_SlopeMax.txt"))
                            {
                                _drobj.LegendUnit = "°";
                            }
                            else
                            {
                                _drobj.LegendUnit = Gral.Main.My_p_m3;
                            }
                        }
                        else // unit read from file
                        {
                            _drobj.LegendUnit = unit;
                        }


                        if (Path.GetFileName(file).Contains("roughness.txt")
                            || Vertical_Concentration // set special settings for GRAMM roughness lenght
                            || Path.GetFileName(file).Contains("TPI_Base.txt"))
                        {
                            _drobj.Filter = false;
                        }
                        else // set filter on by default
                        {
                            _drobj.Filter = true;
                        }

                        if ((nx * ny) <= cellsizemax) // few points -> show contour lines
                        {
                            _drobj.LineWidth = 1;
                            _drobj.ColorScale = "-999,-999,-999";
                        }
                        else // large area -> don't show lines!
                        {
                            _drobj.LineWidth = 0;
                            _drobj.ColorScale = Convert.ToString(picturebox1.Width - 150) + "," + Convert.ToString(picturebox1.Height - 200) + "," + "1";
                        }
                    }
                    //
                    //add list to save contourpoints
                    //
                    _drobj.ContourFilename = file;
                    ItemOptions.Insert(0, _drobj);
                    SaveDomainSettings(1);

                    //compute contour polygons
                    ReDrawContours = true;
                    Contours(file, _drobj);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Process raster data", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                Cursor = Cursors.Default;
            }

        }
    }
}
