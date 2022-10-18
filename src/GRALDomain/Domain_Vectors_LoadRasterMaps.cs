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
using GralMessage;
using GralStaticFunctions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Routine loading vectors of a vector data set from file
        /// </summary>
        public void LoadVectors(string file, DrawingObjects _drobj)
        {
            if (ReDrawVectors == true)
            {
                CultureInfo ic = CultureInfo.InvariantCulture;
                try
                {
                    if (!File.Exists(file))
                    {
                        (string tempfilename, bool saveNewFilePath) = St_F.SearchAbsoluteAndRelativeFilePath(Gral.Main.ProjectName, file, "Maps");
                        if (File.Exists(tempfilename))
                        {
                            file = tempfilename;
                        }
                    }

                    if (File.Exists(file))
                    {
                        using (StreamReader myReader = new StreamReader(file))
                        {
                            if (!GRAMMOnline)
                            {
                                Cursor = Cursors.WaitCursor;
                            }
                            lock (_drobj)
                            {
                                //clear actual vectorpoints
                                _drobj.ContourPoints.Clear();
                                _drobj.ContourPoints.Add(new List<PointF>());
                                _drobj.ContourPoints[0].Clear();
                            }
                            string[] data = new string[100];
                            //open vector file
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
                            data = new string[2 * nx];
                            double[,] umean = new double[nx, ny];
                            double[,] udir = new double[nx, ny];

                            // create new index if array == null or array dimension != nx or ny
                            if (_drobj.ContourColor == null || _drobj.ContourColor.GetUpperBound(0) != (nx - 1) || _drobj.ContourColor.GetUpperBound(1) != (ny - 1))
                            {
                                lock (_drobj)
                                {
                                    _drobj.ContourColor = new int[nx, ny];
                                }
                            }
                            int[,] _contcol = _drobj.ContourColor;
                            double min = 1000;
                            double max = 0;
                            for (int i = ny - 1; i > -1; i--)
                            {
                                data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                int n = -1;
                                for (int j = 0; j < 2 * nx; j++)
                                {
                                    double u = Convert.ToDouble(data[j], ic);
                                    j += 1;
                                    double v = Convert.ToDouble(data[j], ic);
                                    n += 1;
                                    umean[n, i] = Math.Sqrt(u * u + v * v);
                                    //compute maximum and minimum
                                    min = Math.Min(min, umean[n, i]);
                                    max = Math.Max(max, umean[n, i]);
                                    //compute wind direction
                                    if (Math.Abs(v) < 0.00000001)
                                    {
                                        udir[n, i] = 90;
                                    }
                                    else
                                    {
                                        udir[n, i] = Convert.ToInt32(Math.Abs(Math.Atan(u / v)) * 180 / 3.14);
                                    }

                                    if ((v > 0) && (u <= 0))
                                    {
                                        udir[n, i] = 180 - udir[n, i];
                                    }

                                    if ((v >= 0) && (u > 0))
                                    {
                                        udir[n, i] = 180 + udir[n, i];
                                    }

                                    if ((v < 0) && (u >= 0))
                                    {
                                        udir[n, i] = 360 - udir[n, i];
                                    }
                                    //define integer field for fill colors
                                    _contcol[n, i] = -1;

                                    for (int k = _drobj.ItemValues.Count - 1; k > -1; k--)
                                    {
                                        if (umean[n, i] > _drobj.ItemValues[k])
                                        {
                                            _contcol[n, i] = k;
                                            break;
                                        }
                                    }
                                }
                            }

                            //save geometry information of the vector map
                            lock (_drobj)
                            {
                                _drobj.ContourGeometry = new DrawingObjects.ContourGeometries(x11, y11, dx, nx, ny);
                            }

                            if (!Gral.Main.GUISettings.VectorMapAutoScaling)
                            {
                                max = 10; // default fix scaling factor
                            }
                            
                            //compute arrow-polygons
                            double xcenter = 0.0;
                            double ycenter = 0.0;
                            double x1 = 0.0;
                            double y1 = 0.0;
                            double xrot = 0.0;
                            double yrot = 0.0;
                            double scale = dx / (max + 1) * _drobj.VectorScale;
                            if (Math.Abs(max) < 0.000001) // if umean max == 0 -> no vectors -> scale  = 0!
                            {
                                scale = 0;
                            }

                            double length = 1;
                            double anglecor = 3.14 / 180;

                            lock (_drobj)
                            {
                                for (int j = ny - 1; j > -1; j--)
                                {
                                    for (int i = 0; i < nx; i++)
                                    {
                                        //x,y coordinates of the cell center
                                        xcenter = x11 + (dx * (i + 1) - dx * 0.5);
                                        ycenter = y11 + (dx * (j + 1) - dx * 0.5);
                                        length = scale * (umean[i, j] + 1); // + 1 -> avoid lenght 0!

                                        //point 1 of arrow
                                        x1 = xcenter - length * 0.5;
                                        y1 = ycenter - length * 0.1;
                                        double dircos = Math.Cos((udir[i, j] - 270) * anglecor);
                                        double dirsin = Math.Sin((udir[i, j] - 270) * anglecor);

                                        //rotation
                                        xrot = xcenter + (x1 - xcenter) * dircos + (y1 - ycenter) * dirsin;
                                        yrot = ycenter - (x1 - xcenter) * dirsin + (y1 - ycenter) * dircos;
                                        _drobj.ContourPoints[0].Add(new PointF((float)xrot, (float)yrot));
                                        //point 2 of arrow
                                        x1 = xcenter - length * 0.5;
                                        y1 = ycenter + length * 0.1;
                                        //rotation
                                        xrot = xcenter + (x1 - xcenter) * dircos + (y1 - ycenter) * dirsin;
                                        yrot = ycenter - (x1 - xcenter) * dirsin + (y1 - ycenter) * dircos;
                                        _drobj.ContourPoints[0].Add(new PointF((float)xrot, (float)yrot));
                                        //point 3 of arrow
                                        x1 = xcenter + length * 0.05;
                                        y1 = ycenter + length * 0.1;
                                        //rotation
                                        xrot = xcenter + (x1 - xcenter) * dircos + (y1 - ycenter) * dirsin;
                                        yrot = ycenter - (x1 - xcenter) * dirsin + (y1 - ycenter) * dircos;
                                        _drobj.ContourPoints[0].Add(new PointF((float)xrot, (float)yrot));
                                        //point 4 of arrow
                                        x1 = xcenter + length * 0.05;
                                        y1 = ycenter + length * 0.35;
                                        //rotation
                                        xrot = xcenter + (x1 - xcenter) * dircos + (y1 - ycenter) * dirsin;
                                        yrot = ycenter - (x1 - xcenter) * dirsin + (y1 - ycenter) * dircos;
                                        _drobj.ContourPoints[0].Add(new PointF((float)xrot, (float)yrot));
                                        //point 5 of arrow
                                        x1 = xcenter + length * 0.5;
                                        y1 = ycenter;
                                        //rotation
                                        xrot = xcenter + (x1 - xcenter) * dircos + (y1 - ycenter) * dirsin;
                                        yrot = ycenter - (x1 - xcenter) * dirsin + (y1 - ycenter) * dircos;
                                        _drobj.ContourPoints[0].Add(new PointF((float)xrot, (float)yrot));
                                        //point 6 of arrow
                                        x1 = xcenter + length * 0.05;
                                        y1 = ycenter - length * 0.35;
                                        //rotation
                                        xrot = xcenter + (x1 - xcenter) * dircos + (y1 - ycenter) * dirsin;
                                        yrot = ycenter - (x1 - xcenter) * dirsin + (y1 - ycenter) * dircos;
                                        _drobj.ContourPoints[0].Add(new PointF((float)xrot, (float)yrot));
                                        //point 7 of arrow
                                        x1 = xcenter + length * 0.05;
                                        y1 = ycenter - length * 0.1;
                                        //rotation
                                        xrot = xcenter + (x1 - xcenter) * dircos + (y1 - ycenter) * dirsin;
                                        yrot = ycenter - (x1 - xcenter) * dirsin + (y1 - ycenter) * dircos;
                                        _drobj.ContourPoints[0].Add(new PointF((float)xrot, (float)yrot));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (GRAMMOnline == false)
                    {
                        MessageBox.Show(this, "Unable to read vector file: " + ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                }

                if (!GRAMMOnline)
                {
                    Cursor = Cursors.Default;
                }
            }
            ReDrawVectors = false;
        }

        /// <summary>
        /// Routine computing a vector map for GRAMM or GRAL wind field data
        /// </summary>
        private async void ComputeVectorMap(object sender, EventArgs e)
        {
            int windfieldenable = 0;
            //check which wind fields are existent
            string newPath1 = St_F.GetGffFilePath(Path.Combine(Gral.Main.ProjectName, "Computation") + Path.DirectorySeparatorChar);
            DirectoryInfo di = new DirectoryInfo(newPath1);
            FileInfo[] files_wind = di.GetFiles("*.gff");
            if (files_wind.Length == 0)
            {
            }
            else
            {
                windfieldenable = 2;
            }
            if (File.Exists(Path.Combine(Gral.Main.ProjectName, @"Computation", "windfeld.txt")))
            {
                windfieldenable += 1;
            }
            else
            {
            }
            //check for tke-files
            int tkefieldenable = 0;
            files_wind = di.GetFiles("*.tke");
            if (files_wind.Length > 0)
            {
                tkefieldenable = 1;
            }

            //check if no wind fields are available
            if (windfieldenable == 0)
            {
                MessageBox.Show(this, "Neither GRAMM nor GRAL windfields are available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                DialogCreateMeteoStation met_st = new DialogCreateMeteoStation
                {
                    Meteo_Title = "GRAL GUI Compute vector map",
                    Meteo_Init = "Vectormap",
                    Meteo_Ext = ".vec",
                    Meteo_Model = windfieldenable,
                    Meteo_Height = 10,
                    X1 = Left + 70,
                    Y1 = Top + 50,
                    Xs = 0,
                    Ys = 0,
                    ShowAbsHeightBox = true
                };

                if (met_st.ShowDialog() == DialogResult.OK)
                {
                    int trans = met_st.Meteo_Height;
                    bool AbsoluteHeight = met_st.AbsoluteHeight;
                    string file = Path.Combine(Gral.Main.ProjectName, @"Maps", met_st.Meteo_Init);

                    windfieldenable = met_st.Meteo_Model;
                    {
                        //select dispersion situation
                        SelectDispersionSituation disp = new SelectDispersionSituation(this, MainForm);
                        
                        if (windfieldenable == 2)
                        {
                            disp.GFFPath = St_F.GetGffFilePath(Path.Combine(Gral.Main.ProjectName, "Computation"));
                        }
                        else if (windfieldenable > 2 || windfieldenable == 1)
                        {
                            disp.GrammPath = Path.GetDirectoryName(MainForm.GRAMMwindfield);
                        }
                        else
                        {

                        }

                        disp.StartPosition = FormStartPosition.Manual;
                        disp.Location = new Point(met_st.Right + 20, met_st.Top);

                        if (disp.ShowDialog() == DialogResult.OK)
                        {
                            int dissit = disp.selected_situation;

                            if (windfieldenable == 1) // GRAMM
                            {
                                // compute GRAMM vector map
                                if (!GRAMMOnline)
                                {
                                    Cursor = Cursors.WaitCursor;
                                }
                                
                                MessageWindow message = new MessageWindow();
                                message.Show();
                                message.listBox1.Items.Add("Compute vector map...");
                                message.Refresh();

                                //reading geometry file "ggeom.asc"
                                message.listBox1.Items.Add("Reading ggeom.asc ...");
                                message.Refresh();
                                double[,] AH = new double[1, 1];
                                double[,,] ZSP = new double[1, 1, 1];
                                GGeomFileIO ggeom = new GGeomFileIO
                                {
                                    PathWindfield = Path.GetDirectoryName(MainForm.GRAMMwindfield)
                                };

                                if (ggeom.ReadGGeomAsc(0) == true)
                                {
                                    int NX = ggeom.NX;
                                    int NY = ggeom.NY;
                                    int NZ = ggeom.NZ;
                                    int ischnitt = 0; // Height index in the GRAMM wind field above ground/above sea
                                    AH = ggeom.AH;
                                    ZSP = ggeom.ZSP;
                                    ggeom = null;
                                    try
                                    {
                                        double schnitt = Math.Abs(Convert.ToDouble(trans));
                                        int[,] vertIndexArray = null;

                                        //obtain index in the vertical direction for relative height above ground
                                        if (!AbsoluteHeight)
                                        {
                                            for (int k = 1; k <= NZ; k++)
                                            {
                                                if (ZSP[2, 2, k] - AH[2, 2] >= schnitt)
                                                {
                                                    ischnitt = k;
                                                    break;
                                                }
                                            }
                                        }
                                        else //create the height index for each point when using absolute heights
                                        {
                                            vertIndexArray = new int[NX + 1, NY + 1];
                                            for (int j = 1; j <= NY; j++)
                                            {
                                                for (int i = 1; i <= NX; i++)
                                                {
                                                    if (schnitt < ZSP[i, j, 1]) // below terrain
                                                    {
                                                        vertIndexArray[i, j] = -1;
                                                    }
                                                    else
                                                    {
                                                        for (int k = 1; k <= NZ; k++)
                                                        {
                                                            if (ZSP[i, j, k] >= schnitt)
                                                            {
                                                                vertIndexArray[i, j] = k;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        //windfield file Readers
                                        //PSFReader windfield = new PSFReader(form1.GRAMMwindfield, true);
                                        //loop over all weather situations
                                        float[,,] UWI = new float[NX + 1, NY + 1, NZ + 1];
                                        float[,,] VWI = new float[NX + 1, NY + 1, NZ + 1];
                                        float[,,] WWI = new float[NX + 1, NY + 1, NZ + 1];

                                        //read wind fields
                                        message.listBox1.Items.Add("Reading wind field...");
                                        message.Refresh();

                                        string wndfilename = Path.Combine(Path.GetDirectoryName(MainForm.GRAMMwindfield), Convert.ToString(dissit).PadLeft(5, '0') + ".wnd");
                                        Windfield_Reader Reader = new Windfield_Reader();
                                        if (await Task.Run(() => Reader.Windfield_read(wndfilename, NX, NY, NZ, ref UWI, ref VWI, ref WWI)) == false)
                                        {
                                            throw new IOException();
                                        }

                                        message.Close();

                                        double min = 1000;
                                        double max = -1;
                                        double umean;

                                        StringBuilder sb = new StringBuilder();

                                        //write vector file
                                        using (StreamWriter myWriter = new StreamWriter(file))
                                        {
                                            GralIO.WriteESRIFile writeHeader = new GralIO.WriteESRIFile
                                            {
                                                NCols = NX,
                                                NRows = NY,
                                                XllCorner = MainForm.GrammDomRect.West,
                                                YllCorner = MainForm.GrammDomRect.South,
                                                CellSize = MainForm.GRAMMHorGridSize,
                                                Unit = string.Empty,
                                                Round = 4
                                            };
                                            if (!writeHeader.WriteEsriHeader(myWriter))
                                            {
                                                throw new Exception();
                                            }
                                            for (int j = NY; j > 0; j--)
                                            {
                                                sb.Clear();
                                                for (int i = 1; i <= NX; i++)
                                                {
                                                    if (AbsoluteHeight)
                                                    {
                                                        ischnitt = vertIndexArray[i, j];
                                                    }
                                                    
                                                    if (ischnitt >= 0)
                                                    {
                                                        sb.Append(Convert.ToString(Math.Round(UWI[i, j, ischnitt], 4), ic) + "," + Convert.ToString(Math.Round(VWI[i, j, ischnitt], 4), ic) + ",");
                                                        umean = Math.Sqrt(UWI[i, j, ischnitt] * UWI[i, j, ischnitt] + VWI[i, j, ischnitt] * VWI[i, j, ischnitt]);
                                                    }
                                                    else
                                                    {
                                                        sb.Append("0,0,");
                                                        umean = 0;
                                                    }
                                                    //compute maximum and minimum
                                                    min = Math.Min(min, umean);
                                                    max = Math.Max(max, umean);
                                                }
                                                myWriter.WriteLine(sb.ToString());
                                            }
                                        }

                                        //write u-component of vector file
                                        string file3;
                                        file3 = Path.Combine(Gral.Main.ProjectName, @"Maps", Path.GetFileNameWithoutExtension(file) + ".u");
                                        using (StreamWriter myWriter = new StreamWriter(file3))
                                        {
                                            GralIO.WriteESRIFile writeHeader = new GralIO.WriteESRIFile
                                            {
                                                NCols = NX,
                                                NRows = NY,
                                                XllCorner = MainForm.GrammDomRect.West,
                                                YllCorner = MainForm.GrammDomRect.South,
                                                CellSize = MainForm.GRAMMHorGridSize,
                                                Unit = string.Empty,
                                                Round = 4
                                            };
                                            if (!writeHeader.WriteEsriHeader(myWriter))
                                            {
                                                throw new Exception();
                                            }

                                            for (int j = NY; j > 0; j--)
                                            {
                                                sb.Clear();
                                                for (int i = 1; i <= NX; i++)
                                                {
                                                    if (AbsoluteHeight)
                                                    {
                                                        ischnitt = vertIndexArray[i, j];
                                                    }
                                                    if (ischnitt >= 0)
                                                    {
                                                        sb.Append(Convert.ToString(Math.Round(UWI[i, j, ischnitt], 4), ic) + ",");
                                                    }
                                                    else
                                                    {
                                                        sb.Append("0,");
                                                    }
                                                }
                                                myWriter.WriteLine(sb.ToString());
                                            }
                                        }

                                        //write v-component of vector file
                                        file3 = Path.Combine(Gral.Main.ProjectName, @"Maps", Path.GetFileNameWithoutExtension(file) + ".v");
                                        using (StreamWriter myWriter = new StreamWriter(file3))
                                        {
                                            GralIO.WriteESRIFile writeHeader = new GralIO.WriteESRIFile
                                            {
                                                NCols = NX,
                                                NRows = NY,
                                                XllCorner = MainForm.GrammDomRect.West,
                                                YllCorner = MainForm.GrammDomRect.South,
                                                CellSize = MainForm.GRAMMHorGridSize,
                                                Unit = string.Empty,
                                                Round = 4
                                            };
                                            if (!writeHeader.WriteEsriHeader(myWriter))
                                            {
                                                throw new Exception();
                                            }
                                            
                                            for (int j = NY; j > 0; j--)
                                            {
                                                sb.Clear();
                                                for (int i = 1; i <= NX; i++)
                                                {
                                                    if (AbsoluteHeight)
                                                    {
                                                        ischnitt = vertIndexArray[i, j];
                                                    }
                                                    if (ischnitt >= 0)
                                                    {
                                                        sb.Append(Convert.ToString(Math.Round(VWI[i, j, ischnitt], 4), ic) + ",");
                                                    }
                                                    else
                                                    {
                                                        sb.Append("0,");
                                                    }
                                                }
                                                myWriter.WriteLine(sb.ToString());
                                            }
                                        }

                                        sb.Clear();
                                        sb = null;

                                        //add vector map to object list
                                        DrawingObjects _drobj = new DrawingObjects("VM: " + Path.GetFileNameWithoutExtension(file));

                                        //compute values for 10 contours
                                        for (int i = 0; i < 10; i++)
                                        {
                                            _drobj.ItemValues.Add(0);
                                            _drobj.FillColors.Add(Color.Red);
                                            _drobj.LineColors.Add(Color.Black);
                                        }
                                        _drobj.FillColors[0] = Color.Yellow;
                                        _drobj.LineColors[0] = Color.Black;

                                        _drobj.ItemValues[0] = min;
                                        _drobj.ItemValues[9] = max;
                                        //apply color gradient between yellow and red
                                        int r1 = _drobj.FillColors[0].R;
                                        int g1 = _drobj.FillColors[0].G;
                                        int b1 = _drobj.FillColors[0].B;
                                        int r2 = _drobj.FillColors[9].R;
                                        int g2 = _drobj.FillColors[9].G;
                                        int b2 = _drobj.FillColors[9].B;
                                        for (int i = 0; i < 8; i++)
                                        {
                                            _drobj.ItemValues[i + 1] = min + (max - min) / 10 * Convert.ToDouble(i + 1);
                                            int intr = r1 + (r2 - r1) / 10 * (i + 1);
                                            int intg = g1 + (g2 - g1) / 10 * (i + 1);
                                            int intb = b1 + (b2 - b1) / 10 * (i + 1);
                                            _drobj.FillColors[i + 1] = Color.FromArgb(intr, intg, intb);
                                        }

                                        _drobj.LegendTitle = "Vectormap";
                                        _drobj.LegendUnit = "m/s";
                                        //
                                        //add list to save contourpoints
                                        //
                                        _drobj.ContourFilename = file;

                                        ItemOptions.Insert(0, _drobj);
                                        SaveDomainSettings(1);

                                        //compute vectors
                                        ReDrawVectors = true;
                                        LoadVectors(file, _drobj);
                                        Picturebox1_Paint();
                                    }
                                    catch
                                    {
                                        MessageBox.Show(this, "Error reading windfields", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        if (!GRAMMOnline)
                                        {
                                            Cursor = Cursors.Default;
                                        }
                                        return;
                                    }
                                }
                            }
                            //Cursor = Cursors.Default;

                            //compute GRAL vector map
                            if (windfieldenable == 2) // GRAL
                            {
                                if (!GRAMMOnline)
                                {
                                    Cursor = Cursors.WaitCursor;
                                }
                               
                                //obtain surface heights
                                Single[,] AH = new Single[1, 1];
                                Single[,] Building_heights = new Single[1, 1];
                                int[,] KKart = new int[1, 1];
                                Int32 nkk = 0;
                                Int32 njj = 0;
                                Int32 nii = 0;
                                int GRALwest = 0;
                                int GRALsued = 0;
                                Single DZK = 1;
                                Single stretch = 1;
                                Single AHMIN = 0;
                                List<float[]> StretchFlexible = new List<float[]>();

                                //reading geometry file "GRAL_geometries.txt"
                                try
                                {
                                    using (BinaryReader reader = new BinaryReader(File.Open(Path.Combine(Gral.Main.ProjectName, @"Computation", "GRAL_geometries.txt"), FileMode.Open)))
                                    {
                                        nkk = reader.ReadInt32();
                                        njj = reader.ReadInt32();
                                        nii = reader.ReadInt32();
                                        GRALwest = reader.ReadInt32();
                                        GRALsued = reader.ReadInt32();
                                        DZK = reader.ReadSingle();
                                        stretch = reader.ReadSingle();
                                        if (stretch < 0.1)
                                        {
                                            int sliceCount = reader.ReadInt32();

                                            for (int i = 0; i < sliceCount; i++)
                                            {
                                                StretchFlexible.Add(new float[2]);
                                                StretchFlexible[i][0] = reader.ReadSingle(); // Height
                                                StretchFlexible[i][1] = reader.ReadSingle(); // Stretch
                                            }
                                        }

                                        AHMIN = reader.ReadSingle();

                                        AH = new Single[nii + 2, njj + 2];
                                        KKart = new int[nii + 2, njj + 2];
                                        Building_heights = new Single[nii + 2, njj + 2];

                                        for (int i = 1; i <= nii + 1; i++)
                                        {
                                            for (int j = 1; j <= njj + 1; j++)
                                            {
                                                AH[i, j] = reader.ReadSingle();
                                                KKart[i, j] = (short)reader.ReadInt32();
                                                Building_heights[i, j] = reader.ReadSingle();
                                            }
                                        }
                                    }
                                }

                                catch
                                {
                                    MessageBox.Show(this, "Error reading GRAL geometries.txt");
                                }

                                //windfield file Readers
                                float[][][] uk;
                                float[][][] vk;
                                float[][][] wk;
                                Single GRAL_cellsize_ff = 0;

                                float[][][] tke;

                                //reading *.gff-file
                                ReadFlowFieldFiles gff = new ReadFlowFieldFiles
                                {
                                    filename = Path.Combine(St_F.GetGffFilePath(Path.Combine(Gral.Main.ProjectName, "Computation")),
                                                                                Convert.ToString(dissit).PadLeft(5, '0') + ".gff")
                                };

                                CancellationTokenReset();
                                if (gff.ReadGffFile(CancellationTokenSource.Token) == true)
                                {
                                    nkk = gff.NKK;
                                    njj = gff.NJJ;
                                    nii = gff.NII;
                                    uk = gff.Uk;
                                    vk = gff.Vk;
                                    wk = gff.Wk;
                                    GRAL_cellsize_ff = gff.Cellsize;
                                    gff = null;
                                }
                                else
                                {
                                    if (CancellationTokenSource.IsCancellationRequested)
                                    {
                                        return;
                                    }
                                    MessageBox.Show(this, "Error reading windfields", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    if (!GRAMMOnline)
                                    {
                                        Cursor = Cursors.Default;
                                    }
                                    return;
                                }

                                //computation of slice
                                Single[] HOKART = new Single[nkk + 1];  //height of vertical layers starting from zero
                                Single DZKdummy = DZK;
                                int flexstretchindex = 0;
                                float stretching = 1;

                                for (int k = 1; k <= nkk; k++)
                                {
                                    HOKART[k] = HOKART[k - 1] + DZKdummy;

                                    if (stretch > 0.99)
                                    {
                                        DZKdummy *= stretch;
                                    }
                                    else
                                    {
                                        if (flexstretchindex < StretchFlexible.Count - 1)
                                        {
                                            if (StretchFlexible[flexstretchindex + 1][1] > 0.99 &&
                                                HOKART[k - 1] > StretchFlexible[flexstretchindex + 1][0])
                                            {
                                                stretching = StretchFlexible[flexstretchindex + 1][1];
                                                flexstretchindex++;
                                            }
                                        }
                                        DZKdummy *= stretching;
                                    }
                                }

                                double schnitt = Convert.ToDouble(trans);
                                int[,] slice = new int[nii + 1, njj + 1];
                                for (int i = 1; i < nii + 1; i++)
                                {
                                    for (int j = 1; j < njj + 1; j++)
                                    {
                                        if (!AbsoluteHeight)
                                        {
                                            //slice for height above ground: check if slice is above model domain
                                            if (AH[i, j] - Building_heights[i, j] + schnitt >= AHMIN + HOKART[nkk])
                                            {
                                                slice[i, j] = nkk;
                                            }
                                            //check if slice is below model domain
                                            else if (AH[i, j] - Building_heights[i, j] + schnitt <= AHMIN + HOKART[1])
                                            {
                                                slice[i, j] = 0;
                                            }
                                            else
                                            {
                                                for (int k = 1; k < nkk + 1; k++)
                                                {
                                                    //check if point is above desired slice above ground
                                                        if (AHMIN + HOKART[k] >= AH[i, j] - Building_heights[i, j] + schnitt)
                                                        {
                                                            slice[i, j] = k;
                                                            break;
                                                        }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //slice for absolute height above ground
                                            if (schnitt >= AHMIN + HOKART[nkk])
                                            {
                                                slice[i, j] = nkk;
                                            }
                                            //check if slice is below model domain
                                            else if (schnitt <= AHMIN + HOKART[1] + Building_heights[i, j])
                                            {
                                                slice[i, j] = 0;
                                            }
                                            else
                                            {
                                                for (int k = 1; k < nkk + 1; k++)
                                                {
                                                    //check if point is above desired slice above abs. height
                                                    if (AHMIN + HOKART[k] > schnitt)
                                                    {
                                                        slice[i, j] = k;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                double min = 1000;
                                double max = -1;
                                double umean;
                                //write vector file
                                try
                                {
                                    using (StreamWriter myWriter = new StreamWriter(file))
                                    {
                                        GralIO.WriteESRIFile writeHeader = new GralIO.WriteESRIFile
                                        {
                                            NCols = nii,
                                            NRows = njj,
                                            XllCorner = GRALwest,
                                            YllCorner = GRALsued,
                                            CellSize = GRAL_cellsize_ff,
                                            Unit = string.Empty,
                                            Round = 4
                                        };
                                        if (!writeHeader.WriteEsriHeader(myWriter))
                                        {
                                            throw new Exception();
                                        }

                                        for (int j = njj; j > 0; j--)
                                        {
                                            for (int i = 1; i <= nii; i++)
                                            {
                                                myWriter.Write(Convert.ToString((uk[i][j][slice[i, j]] + uk[i + 1][j][slice[i, j]]) * 0.5F, ic) + "," + Convert.ToString((vk[i][j][slice[i, j]] + vk[i][j + 1][slice[i, j]]) * 0.5F, ic) + ",");
                                                umean = Math.Sqrt(uk[i][j][slice[i, j]] * uk[i][j][slice[i, j]] + vk[i][j][slice[i, j]] * vk[i][j][slice[i, j]]);
                                                //compute maximum and minimum
                                                min = Math.Min(min, umean);
                                                max = Math.Max(max, umean);
                                            }
                                            myWriter.WriteLine();
                                        }
                                    }
                                }
                                catch { }

                                try
                                {
                                    //write u-component of vector file
                                    string file3;
                                    file3 = Path.Combine(Gral.Main.ProjectName, @"Maps", Path.GetFileNameWithoutExtension(file) + "_u.txt");
                                    using (StreamWriter myWriter = new StreamWriter(file3))
                                    {
                                        GralIO.WriteESRIFile writeHeader = new GralIO.WriteESRIFile
                                        {
                                            NCols = nii,
                                            NRows = njj,
                                            XllCorner = GRALwest,
                                            YllCorner = GRALsued,
                                            CellSize = GRAL_cellsize_ff,
                                            Unit = string.Empty,
                                            Round = 4
                                        };
                                        if (!writeHeader.WriteEsriHeader(myWriter))
                                        {
                                            throw new Exception();
                                        }

                                        for (int j = njj; j > 0; j--)
                                        {
                                            for (int i = 1; i <= nii; i++)
                                            {
                                                myWriter.Write(Convert.ToString(((uk[i][j][slice[i, j]] + uk[i + 1][j][slice[i, j]]) * 0.5F), ic) + ",");
                                            }
                                            myWriter.WriteLine();
                                        }
                                    }

                                    //write v-component of vector file
                                    file3 = Path.Combine(Gral.Main.ProjectName, @"Maps", Path.GetFileNameWithoutExtension(file) + "_v.txt");
                                    using (StreamWriter myWriter = new StreamWriter(file3))
                                    {
                                        GralIO.WriteESRIFile writeHeader = new GralIO.WriteESRIFile
                                        {
                                            NCols = nii,
                                            NRows = njj,
                                            XllCorner = GRALwest,
                                            YllCorner = GRALsued,
                                            CellSize = GRAL_cellsize_ff,
                                            Unit = string.Empty,
                                            Round = 4
                                        };
                                        if (!writeHeader.WriteEsriHeader(myWriter))
                                        {
                                            throw new Exception();
                                        }

                                        for (int j = njj; j > 0; j--)
                                        {
                                            for (int i = 1; i <= nii; i++)
                                            {
                                                myWriter.Write(Convert.ToString(((vk[i][j][slice[i, j]] + vk[i][j + 1][slice[i, j]]) * 0.5F), ic) + ",");
                                            }
                                            myWriter.WriteLine();
                                        }
                                    }

                                    //write w-component of vector file
                                    file3 = Path.Combine(Gral.Main.ProjectName, @"Maps", Path.GetFileNameWithoutExtension(file) + "_w.txt");
                                    using (StreamWriter myWriter = new StreamWriter(file3))
                                    {
                                        GralIO.WriteESRIFile writeHeader = new GralIO.WriteESRIFile
                                        {
                                            NCols = nii,
                                            NRows = njj,
                                            XllCorner = GRALwest,
                                            YllCorner = GRALsued,
                                            CellSize = GRAL_cellsize_ff,
                                            Unit = string.Empty,
                                            Round = 4
                                        };
                                        if (!writeHeader.WriteEsriHeader(myWriter))
                                        {
                                            throw new Exception();
                                        }
                                        
                                        for (int j = njj; j > 0; j--)
                                        {
                                            for (int i = 1; i <= nii; i++)
                                            {
                                                myWriter.Write(Convert.ToString(((wk[i][j][slice[i, j]] + wk[i][j + 1][slice[i, j] + 1]) * 0.5F), ic) + ",");
                                            }
                                            myWriter.WriteLine();
                                        }
                                    }

                                    //optionally reading *.tke-file
                                    if (tkefieldenable == 1)
                                    {
                                        try
                                        {
                                            gff = new ReadFlowFieldFiles
                                            {
                                                filename = Path.Combine(St_F.GetGffFilePath(Path.Combine(Gral.Main.ProjectName, "Computation")),
                                                                                            Convert.ToString(dissit).PadLeft(5, '0') + ".tke")
                                            };

                                            CancellationTokenReset();
                                            if (await Task.Run(() => gff.ReadTKEFile(CancellationTokenSource.Token)) == true)
                                            {
                                                nkk = gff.NKK;
                                                njj = gff.NJJ;
                                                nii = gff.NII;
                                                tke = gff.TKE;
                                                GRAL_cellsize_ff = gff.Cellsize;
                                                gff = null;

                                                //write tke-component
                                                file3 = Path.Combine(Gral.Main.ProjectName, @"Maps", Path.GetFileNameWithoutExtension(file) + "_tke.txt");
                                                using (StreamWriter myWriter = new StreamWriter(file3))
                                                {
                                                    GralIO.WriteESRIFile writeHeader = new GralIO.WriteESRIFile
                                                    {
                                                        NCols = nii,
                                                        NRows = njj,
                                                        XllCorner = GRALwest,
                                                        YllCorner = GRALsued,
                                                        CellSize = GRAL_cellsize_ff,
                                                        Unit = string.Empty,
                                                        Round = 4
                                                    };
                                                    if (!writeHeader.WriteEsriHeader(myWriter))
                                                    {
                                                        throw new Exception();
                                                    }

                                                    for (int j = njj; j > 0; j--)
                                                    {
                                                        for (int i = 1; i <= nii; i++)
                                                        {
                                                            myWriter.Write(Convert.ToString((tke[i][j][slice[i, j]]), ic) + ",");
                                                        }
                                                        myWriter.WriteLine();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (CancellationTokenSource.IsCancellationRequested)
                                                {
                                                    return;
                                                }
                                                MessageBox.Show(this, "Error reading tke-fields", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                if (!GRAMMOnline)
                                                {
                                                    Cursor = Cursors.Default;
                                                }
                                                uk = null; vk = null; wk = null; tke = null;
                                                return;
                                            }
                                        }
                                        catch { }
                                    }
                                }
                                catch
                                {
                                    MessageBox.Show(this, "Error reading windfields", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    if (!GRAMMOnline)
                                    {
                                        Cursor = Cursors.Default;
                                    }
                                    uk = null; vk = null; wk = null; tke = null;
                                    return;
                                }
                                              
                                //add vector map to object list
                                DrawingObjects _drobj = new DrawingObjects("VM: " + Path.GetFileNameWithoutExtension(file));

                                //compute values for 10 contours
                                for (int i = 0; i < 10; i++)
                                {
                                    _drobj.ItemValues.Add(0);
                                    _drobj.FillColors.Add(Color.Red);
                                    _drobj.LineColors.Add(Color.Black);
                                }
                                _drobj.FillColors[0] = Color.Yellow;
                                _drobj.LineColors[0] = Color.Black;

                                _drobj.ItemValues[0] = min;
                                _drobj.ItemValues[9] = max;
                                //apply color gradient between yellow and red
                                int r1 = _drobj.FillColors[0].R;
                                int g1 = _drobj.FillColors[0].G;
                                int b1 = _drobj.FillColors[0].B;
                                int r2 = _drobj.FillColors[9].R;
                                int g2 = _drobj.FillColors[9].G;
                                int b2 = _drobj.FillColors[9].B;
                                for (int i = 0; i < 8; i++)
                                {
                                    _drobj.ItemValues[i + 1] = min + (max - min) / 10 * Convert.ToDouble(i + 1);
                                    int intr = r1 + (r2 - r1) / 10 * (i + 1);
                                    int intg = g1 + (g2 - g1) / 10 * (i + 1);
                                    int intb = b1 + (b2 - b1) / 10 * (i + 1);
                                    _drobj.FillColors[i + 1] = Color.FromArgb(intr, intg, intb);
                                    //linecolors[0][i + 1] = Color.FromArgb(intr, intg, intb);
                                }

                                _drobj.LegendTitle = "Vectormap";
                                _drobj.LegendUnit = "m/s";
                                //
                                //add list to save contourpoints
                                //

                                _drobj.ContourFilename = file;

                                ItemOptions.Insert(0, _drobj);
                                SaveDomainSettings(1);

                                //compute vectors
                                ReDrawVectors = true;
                                LoadVectors(file, _drobj);
                                Picturebox1_Paint();
                                uk = null; vk = null; wk = null; tke = null;
                            }
                            if (!GRAMMOnline)
                            {
                                Cursor = Cursors.Default;
                            }
                        }
                        disp.Dispose();
                    }
                }
                met_st.Close();
                met_st.Dispose();
            }
        }
    }
}
