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

using GralDomForms;
using GralIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace GralDomain
{
    public partial class Domain
	{
        /// <summary>
        /// Routine evaluationg the vertical concentrations of transient GRAL results
        /// </summary>
        private void Evaluate3DConcentrations(double x0, double y0, double x1, double y1)
        {
            string filename = Path.Combine(Gral.Main.ProjectName, "Computation", "Vertical_Concentrations.txt");

            if (File.Exists(filename))
            {
                if (MeteoDialog != null) // close possible open Dialog
                {
                    MeteoDialog.Close();
                    MeteoDialog.Dispose();
                }
                MeteoDialog = new DialogCreateMeteoStation
                {
                    Meteo_Title = "GRAL GUI Evaluate vertical concentration profile",
                    Meteo_Init = "Vertical_Concentration.txt",
                    Meteo_Ext = ".txt",
                    Meteo_Height = 200,
                    Meteo_Model = 32,   // Show height & filename only
                    X1 = Left + 70,
                    Y1 = Top + 50,
                    X0 = x0,
                    Y0 = y0,
                    Xs = x1,
                    Ys = y1
                };

                MeteoDialog.Start_computation += new StartCreateMeteoStation(Evaluate_3D_Conc_Run);
                //  met_st.Start_computation += new Dialog_CreateMeteoStation.start_create_meteo_station(mettimeseries); // delegate from Dialog -> OK
                MeteoDialog.Cancel_computation += new CancelCreateMeteoStation(Evaluate_3D_Conc_Cancel);

                MeteoDialog.Show();
            }
        }

        private void Evaluate_3D_Conc_Cancel(object sender, EventArgs e)
        {
            MeteoDialog.Start_computation -= new StartCreateMeteoStation(Evaluate_3D_Conc_Run);
            MeteoDialog.Cancel_computation -= new CancelCreateMeteoStation(Evaluate_3D_Conc_Cancel);
            MeteoDialog.Close();
            MeteoDialog.Dispose();
        }


        private void Evaluate_3D_Conc_Run(object sender, EventArgs e)
        {
            List<float> height_of_layers = new List<float>();
            CultureInfo ic = CultureInfo.InvariantCulture;
            string filename = Path.Combine(Gral.Main.ProjectName, "Computation", "Vertical_Concentrations.txt");

            float x0 = (float)MeteoDialog.X0;
            float y0 = (float)MeteoDialog.Y0;
            float x1 = (float)MeteoDialog.Xs;
            float y1 = (float)MeteoDialog.Ys;
            float MaxHeight = (float)MeteoDialog.Meteo_Height;

            if (File.Exists(filename))
            {
                try
                {
                    using (StreamReader rf = new StreamReader(filename))
                    {
                        string[] txt = new string[2];
                        char sep = '\t';

                        txt = rf.ReadLine().Split(sep);
                        double west = Convert.ToDouble(txt[0], ic);

                        rf.ReadLine(); // east

                        txt = rf.ReadLine().Split(sep);
                        double south = Convert.ToDouble(txt[0], ic);

                        txt = rf.ReadLine().Split(sep);
                        double north = Convert.ToDouble(txt[0], ic);

                        txt = rf.ReadLine().Split(sep);
                        int NII = Convert.ToInt32(txt[0], ic);

                        txt = rf.ReadLine().Split(sep);
                        int NJJ = Convert.ToInt32(txt[0], ic);

                        txt = rf.ReadLine().Split(sep);
                        int NKK = Convert.ToInt32(txt[0], ic);

                        txt = rf.ReadLine().Split(sep);
                        double deltax = Convert.ToDouble(txt[0], ic);

                        txt = rf.ReadLine().Split(sep);
                        double model_base_height = Convert.ToDouble(txt[0], ic);

                        txt = rf.ReadLine().Split(sep); // height values
                        for (int i = 0; i < txt.Length - 1; i++) // last entry = description
                        {
                            float val = Convert.ToSingle(txt[i], ic);
                            height_of_layers.Add(val);
                        }


                        // Define array for all values and the height
                        float[][][] Concentration = Landuse.CreateArray<float[][]>(NII + 1, () => Landuse.CreateArray<float[]>(NJJ + 1, () => new float[height_of_layers.Count]));
                        float[][] Topography = Landuse.CreateArray<float[]>(NII + 2, () => new float[NJJ + 2]);
                        float Topo_Min = 100000;
                        float Topo_Max = 0;

                        // Read the complete file
                        rf.ReadLine();
                        for (int h = 0; h < height_of_layers.Count; h++) // loop over height
                        {
                            string xxx = rf.ReadLine(); // lines between concentration blocks

                            for (int j = 1; j <= NJJ; j++)
                            {
                                string line = rf.ReadLine(); // read entire line
                                txt = line.Split(sep);
                                for (int i = 1; i <= NII + 1; i++)
                                {
                                    if (i < txt.GetUpperBound(0))
                                    {
                                        Concentration[i][j][h] = Convert.ToSingle(txt[i], ic);
                                    }
                                }
                            }
                        }

                        string line2 = rf.ReadLine();
                        //MessageBox.Show (line2);
                        for (int j = 1; j <= NJJ; j++)
                        {
                            string line = rf.ReadLine(); // read entire line
                            txt = line.Split(sep);
                            for (int i = 1; i < NII; i++)
                            {
                                if (i < txt.GetUpperBound(0))
                                {
                                    Topography[i][j] = Convert.ToSingle(txt[i], ic);
                                    Topo_Max = Math.Max(Topo_Max, Topography[i][j]);
                                    Topo_Min = Math.Min(Topo_Min, Topography[i][j]);
                                }
                            }
                        }

                        // calculate index of original data here!!
                        int _x0 = (int)((x0 - west) / deltax);
                        int _y0 = (int)((north - y0) / deltax);
                        int _x1 = (int)((x1 - west) / deltax);
                        int _y1 = (int)((north - y1) / deltax);

                        // The Raster for the vertical concentration grid is deltax
                        float maxheight = Math.Min(MaxHeight, (Topo_Max - Topo_Min + height_of_layers[height_of_layers.Count - 1]));

                        int Y_Raster_Number = (int)(maxheight / deltax);
                        int X_Raster_Number = 0;

                        // Count the numbers of the concentration section
                        foreach (Point raster in GralStaticFunctions.Bresenham.GetNewPoint(_x0, _y0, _x1, _y1))
                        {
                            if (raster.X > 0 && raster.Y > 0 && raster.X < NII && raster.Y < NJJ)
                            {
                                X_Raster_Number++;
                            }
                        }

                        if (X_Raster_Number > 1)
                        {

                            float[][] Result = Landuse.CreateArray<float[]>(X_Raster_Number + 1, () => new float[Y_Raster_Number]);
                            float cellsize = (float)(Math.Round(Math.Sqrt(Math.Pow((x0 - x1), 2) + Math.Pow((y0 - y1), 2)) / X_Raster_Number, 1)); // Cell size depends on the lenght of the section
                            int x_result = 0;

                            foreach (Point raster in GralStaticFunctions.Bresenham.GetNewPoint(_x0, _y0, _x1, _y1))
                            {
                                if (raster.X > 0 && raster.Y > 0 && raster.X < NII && raster.Y < NJJ)
                                {
                                    if (x_result < X_Raster_Number)
                                    {
                                        for (int k = 0; k < Y_Raster_Number; k++)
                                        {
                                            float absolute_height = Topo_Min + k * cellsize; // absolute height for ESRI File

                                            if (absolute_height < Topography[raster.X][raster.Y]) // lower than actual topography
                                            {
                                                Result[x_result][k] = -9999;
                                            }
                                            else // above topography
                                            {
                                                int vertical_layer = 0;
                                                foreach (float height in height_of_layers) // search index of height layer
                                                {
                                                    if (height < (absolute_height - Topography[raster.X][raster.Y]))
                                                    {
                                                        vertical_layer++;
                                                    }
                                                }

                                                if (vertical_layer > height_of_layers.Count - 2)
                                                {
                                                    Result[x_result][k] = 0;
                                                }
                                                else
                                                {
                                                    Result[x_result][k] = Concentration[raster.X][raster.Y][vertical_layer];
                                                }
                                            }
                                        } // loop over vertical layers
                                    }
                                    x_result++;
                                } // inside model domain
                            } // search all points along the section line


                            string result_file = MeteoDialog.Meteo_Init;

                            string file = Path.Combine(Gral.Main.ProjectName, "Maps", result_file);
                            using (StreamWriter myWriter = new StreamWriter(file))
                            {
                                myWriter.WriteLine("ncols         " + Convert.ToString(X_Raster_Number));
                                myWriter.WriteLine("nrows         " + Convert.ToString(Y_Raster_Number));
                                myWriter.WriteLine("Xllcorner     " + Convert.ToString(x0, ic));
                                myWriter.WriteLine("Yllcorner     " + Convert.ToString(y0, ic));
                                myWriter.WriteLine("cellsize      " + Convert.ToString(cellsize, ic));
                                myWriter.WriteLine("NODATA_value  " + "-9999\t Unit:\t" + "µg/m³\tVertical_concentration\t" + model_base_height.ToString(ic) + "\tmodel_base_height_[m]");
                                for (int j = Y_Raster_Number - 1; j > -1; j--)
                                {
                                    for (int i = 0; i < X_Raster_Number; i++)
                                    {
                                        myWriter.Write(Convert.ToString(Result[i][j], ic) + " ");
                                    }
                                    myWriter.WriteLine();
                                }
                            }

                            CreateContourMap(file);
                            Picturebox1_Paint();

                            //MessageBox.Show(xi.ToString() + "/" + yi.ToString());

                        } // X_Raster_Number too small
                    }
                }
                catch
                {

                }
            } // if file.exist
            MeteoDialog.Start_computation -= new StartCreateMeteoStation(Evaluate_3D_Conc_Run);
            MeteoDialog.Cancel_computation -= new CancelCreateMeteoStation(Evaluate_3D_Conc_Cancel);
            MeteoDialog.Close();
            MeteoDialog.Dispose();
        } // Evaluate 3D Concentrations
    }
}
