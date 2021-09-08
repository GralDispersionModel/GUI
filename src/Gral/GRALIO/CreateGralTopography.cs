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
 * User: Markus
 * Date: 18.11.2018
 * Time: 18:30
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using System.Linq;
using Gral;
namespace GralIO
{
    /// <summary>
    /// CreateGralTopography Reads raster elevation data and creates the file "GRAL_topofile.txt"
    /// </summary>
    public class CreateGralTopography
    {
        private Main form1 = null;
        private static CultureInfo ic = CultureInfo.InvariantCulture;

        /// <summary>
        /// Generates the Gral_topofile.txt file
        /// Called from await -> async!
        /// </summary> 
        public bool CreateTopography(Main f)
        {
            form1 = f;
            if (form1 == null)
            {
                return false;
            }

            return ReadTopoData(Main.HR_topofile);
        }

        /// <summary>
        /// Get the filename for the HighResolution (HR) topo file
        /// </summary> 
        public bool GetFilename()
        {
            bool OK = false;

            using (OpenFileDialog dialog1 = new OpenFileDialog
            {
                Filter = "Topo files (*.txt;*.dat)|*.txt;*.dat",
                Title = "Select topography"
            })
            {
                try
                {
                    if (string.IsNullOrEmpty(Main.HR_topofile))
                    {
                        Main.HR_topofile = Main.ProjectName;
                    }
                    dialog1.InitialDirectory = Path.GetDirectoryName(Main.HR_topofile);
                    dialog1.FileName = Path.GetFileName(Main.HR_topofile);

                    if (dialog1.ShowDialog() == DialogResult.OK)
                    {
                        Main.HR_topofile = dialog1.FileName;
                        // write Path to "DefaultPath"
                        using (StreamWriter write = new StreamWriter(Path.Combine(Main.App_Settings_Path, @"DefaultPath")))
                        {
                            write.WriteLine(Main.ProjectName);
                            write.WriteLine(Main.HR_topofile);
                            write.WriteLine(Main.CopyCorestoProject.ToString());
                            write.WriteLine(Main.CompatibilityToVersion1901.ToString());
                            write.WriteLine(Main.CalculationCoresPath);
                            write.WriteLine(Main.VectorMapAutoScaling.ToString());
                            write.WriteLine(Main.IgnoreMeteo00Values.ToString("d"));
                            write.WriteLine(Main.FilesDeleteToRecyclingBin.ToString());
                        }
                        OK = true;
                    }
                }
                catch { }
            }
            return OK;
        }

        private bool ReadTopoData(string TopoFile)
        {
            //User defineddecimal seperator
            string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            char[] splitchar = null;
            if (decsep == ",")
            {
                splitchar = new char[] { ' ', '\t', ';' };
            }
            else
            {
                splitchar = new char[] { ' ', '\t', ',', ';' };
            }

            bool GRALdomainOK = false;
            int nx = 0;
            int ny = 0;
            double x11 = 0;
            double y11 = 0;
            double dx = 1;
            float[][] ADH = GralIO.Landuse.CreateArray<float[]>(1, () => new float[1]);
            string[] text = new string[1];

            //check whether defined GRAL domain is within the selected topography file or not
            string[] data = new string[100];
            StreamReader myreader = new StreamReader(TopoFile);
            try
            {
                data = myreader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                nx = Convert.ToInt32(data[1]);
                data = myreader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                ny = Convert.ToInt32(data[1]);
                data = myreader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                x11 = Convert.ToDouble(data[1].Replace(".", decsep));
                data = myreader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                y11 = Convert.ToDouble(data[1].Replace(".", decsep));
                data = myreader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                dx = Convert.ToDouble(data[1].Replace(".", decsep));
                string dummytext = myreader.ReadLine();
                if ((form1.GralDomRect.West < x11) || (form1.GralDomRect.East > x11 + dx * nx) || (form1.GralDomRect.South < y11) || (form1.GralDomRect.North > y11 + dx * ny))
                {
                    MessageBox.Show("GRAL Domain is outside the borders of the selected topography file", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    GRALdomainOK = true;
                }

                if (GRALdomainOK == true)
                {

                    GralMessage.Waitprogressbar wait = new GralMessage.Waitprogressbar("Reading GRAL topography");
                    wait.Show();
                    //read topographical data
                    try
                    {
                        // ytopo min und ytopo max
                        double dxk = Convert.ToDouble(form1.numericUpDown10.Value);
                        int njj = Convert.ToInt32(Math.Floor((form1.GralDomRect.North - form1.GralDomRect.South) / (float)form1.numericUpDown10.Value));
                        double ytopo = form1.GralDomRect.South + (njj - 1) * dxk + 0.5 * dxk;
                        int j_ori_min = -Convert.ToInt32(Math.Ceiling((ytopo - y11) / dx) - ny);
                        ytopo = form1.GralDomRect.South + (0.0) * dxk + 0.5 * dxk;
                        int j_ori_max = -Convert.ToInt32(Math.Ceiling((ytopo - y11) / dx) - ny);

                        //
                        //ADH = new float[nx + 2, ny + 2];

                        int j_span = Math.Max(2, j_ori_max - j_ori_min); // number of cells in y-direction

                        ADH = GralIO.Landuse.CreateArray<float[]>(nx + 2, () => new float[j_span + 2]);
                        text = new string[nx];
                        string readline;
                        int j_count = 0;
                        for (int i = 1; i < ny + 1; i++)
                        {
                            if (i % 40 == 0)
                            {
                                wait.Text = "Reading GRAL topography " + ((int)((float)i / (ny + 2) * 100F)).ToString() + "%";
                            }

                            readline = myreader.ReadLine();

                            if (i >= j_ori_min && i <= j_ori_max) // inside GRAL domain?
                            {
                                text = readline.Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                                if (text.Count() < nx)
                                {
                                    throw new ArgumentOutOfRangeException("Topo file data corrupt at line " + (i + 6).ToString());
                                }

                                for (int j = 1; j < nx + 1; j++)
                                {
                                    ADH[j][j_count] = (float)Convert.ToDouble(text[j - 1].Replace(".", decsep));
                                }
                                j_count++;
                            }
                            Application.DoEvents();
                        }
                    }
                    catch (Exception ex)
                    {
                        myreader.Close();
                        GRALdomainOK = false;
                        MessageBox.Show(ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    wait.Close();
                    wait.Dispose();
                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            myreader.Close();
            myreader.Dispose();

            if (GRALdomainOK == true)
            {
                //generate file GRAL_topofile.txt        
                try
                {
                    using (StreamWriter graltopo = new StreamWriter(Path.Combine(Main.ProjectName, @"Computation", "GRAL_topofile.txt")))
                    {
                        double dxk = Convert.ToDouble(form1.numericUpDown10.Value);
                        int nii = Convert.ToInt32(Math.Floor((form1.GralDomRect.East - form1.GralDomRect.West) / (float)form1.numericUpDown10.Value));
                        int njj = Convert.ToInt32(Math.Floor((form1.GralDomRect.North - form1.GralDomRect.South) / (float)form1.numericUpDown10.Value));
                        int i_ori = 0;
                        int j_ori = 0;
                        graltopo.WriteLine("ncols         " + Convert.ToString(nii));
                        graltopo.WriteLine("nrows         " + Convert.ToString(njj));
                        graltopo.WriteLine("xllcorner     " + Convert.ToString(form1.GralDomRect.West));
                        graltopo.WriteLine("yllcorner     " + Convert.ToString(form1.GralDomRect.South));
                        graltopo.WriteLine("cellsize      " + Convert.ToString(form1.numericUpDown10.Value, ic));
                        graltopo.WriteLine("NODATA_value  " + "-9999" + "\t UNIT \t m");
                        double ytopo = form1.GralDomRect.South + (njj - 1) * dxk + 0.5 * dxk;
                        int j_ori_min = -Convert.ToInt32(Math.Ceiling((ytopo - y11) / dx) - ny); // compute minimum index number of original grid

                        double xtopo = 0;
                        ytopo = 0;

                        System.Text.StringBuilder SB = new System.Text.StringBuilder();
                        for (int j = njj - 1; j > -1; j--)
                        {
                            ytopo = form1.GralDomRect.South + j * dxk + 0.5 * dxk;
                            j_ori = -Convert.ToInt32(Math.Ceiling((ytopo - y11) / dx) - ny) - j_ori_min;
                            SB.Clear();
                            for (int i = 0; i < nii; i++)
                            {
                                xtopo = form1.GralDomRect.West + i * dxk + 0.5 * dxk;
                                i_ori = Convert.ToInt32(Math.Ceiling((xtopo - x11) / dx));

                                SB.Append(Math.Round(ADH[i_ori][j_ori], 1).ToString(ic));
                                SB.Append(",");
                                //graltopo.Write(Convert.ToString(Math.Round(ADH[i_ori][j_ori], 1), ic) + ",");
                            }
                            graltopo.WriteLine(SB.ToString());
                            Application.DoEvents();
                        }
                        SB.Clear();
                        SB = null;
                    }
                }
                catch
                {
                    GRALdomainOK = false;
                }
            }
            return GRALdomainOK;
        }
    }
}
