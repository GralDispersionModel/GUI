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
using System.IO;
using System.Windows.Forms;
using GralIO;
using GralMessage;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
    {
        ///////////////////////////////////////////////////////////////////////////////////////
        //
        //generate GRAMM grid "ggeom.asc"
        //
        ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Load and create the GRAMM topography grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMLoadCreateTopography(object sender, EventArgs e)
        {
            if (GRALSettings.BuildingMode == BuildingModeEnum.GRAMM)
            {
                //Special mode: generate flat topography file when GRAMM is used to compute flow around buildings
                Topofile = Path.Combine(ProjectName, @"Maps", "Flat_topo.txt");
                try
                {
                    using (StreamWriter myWriter = new StreamWriter(Topofile))
                    {
                        int NX = Convert.ToInt32((GrammDomRect.East - GrammDomRect.West) / GRAMMHorGridSize) + 2;
                        int NY = Convert.ToInt32((GrammDomRect.North - GrammDomRect.South) / GRAMMHorGridSize) + 2;
                        myWriter.WriteLine("ncols         " + Convert.ToString(NX));
                        myWriter.WriteLine("nrows         " + Convert.ToString(NY));
                        myWriter.WriteLine("xllcorner     " + Convert.ToString(GrammDomRect.West - GRAMMHorGridSize));
                        myWriter.WriteLine("yllcorner     " + Convert.ToString(GrammDomRect.South - GRAMMHorGridSize));
                        myWriter.WriteLine("cellsize      " + Convert.ToString(GRAMMHorGridSize));
                        myWriter.WriteLine("NODATA_value  " + "-9999" + "\t UNIT \t m");
                        for (int j = NY; j > 0; j--)
                        {
                            for (int i = 1; i <= NX; i++)
                            {
                                myWriter.Write("0 ");
                            }
                            myWriter.WriteLine();
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Error when reading Flat_topo.txt", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // Default mode: load topography and use Topo Data
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "Topo files (*.txt;*.dat)|*.txt;*.dat",
                    Title = "Select topography",
                    ShowHelp = true
#if NET6_0_OR_GREATER
                    ,ClientGuid = GralStaticFunctions.St_F.FileDialogTopo
#endif
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Topofile = dialog.FileName;

                    //check whether defined GRAMM domain is within the selected topography file
                    string[] data = new string[100];
                    int nx = 0, ny = 0;
                    double x11 = 0, y11 = 0, dx = 1;
                    try
                    {
                        using (StreamReader myreader = new StreamReader(Topofile))
                        {
                            data = myreader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            nx = Convert.ToInt32(data[1]);
                            data = myreader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            ny = Convert.ToInt32(data[1]);
                            data = myreader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            x11 = Convert.ToDouble(data[1].Replace(".", DecimalSep));
                            data = myreader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            y11 = Convert.ToDouble(data[1].Replace(".", DecimalSep));
                            data = myreader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            dx = Convert.ToDouble(data[1].Replace(".", DecimalSep));
                        }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Error reading " + Path.GetFileName(Topofile) + Environment.NewLine + ex.Message.ToString(), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if ((GrammDomRect.West < x11) || (GrammDomRect.East > x11 + dx * nx) || (GrammDomRect.South < y11) || (GrammDomRect.North > y11 + dx * ny))
                    {
                        MessageBox.Show("GRAMM Domain is outside the borders of the selected topography file", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if (dx > Convert.ToDouble(numericUpDown18.Value))
                    {
                        MessageBox.Show("The cellsize of the topography file (" + dx.ToString() + " m)" + Environment.NewLine +
                                        "is lower than the GRAMM grid resolution"
                                        , "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dialog.Dispose();
                        return;
                    }
                }
            }

            try
            {
                if (string.IsNullOrEmpty(textBox15.Text) || string.IsNullOrEmpty(textBox12.Text))
                {
                    throw new Exception("No GRAMM domain defined! Please define a GRAMM domain area.");
                }
                //clear Gramm Terrain Box
                textBoxGrammTerrain.Text = string.Empty;
                //generate the file geom.in
                //string file = Path.GetDirectoryName(Application.ExecutablePath);
                //string newpath = Path.Combine(Path.GetDirectoryName(topofile), @"geom.in");
                string newpath = Path.Combine(ProjectName, @"Computation", "geom.in");
                try
                {
                    using (StreamWriter mywriter = new StreamWriter(newpath))
                    {
                        mywriter.WriteLine(Topofile);
                        mywriter.WriteLine(Convert.ToString(numericUpDown17.Value, ic));
                        mywriter.WriteLine(Convert.ToString(numericUpDown19.Value, ic));
                    }
                }
                catch { MessageBox.Show("Error writing geom.in", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error); }

                Cursor = Cursors.WaitCursor;

                //user can define the number of grid cells at the boundaries used to smooth the topography
                int n = (int)numericUpDown18.Value * 5; // smooth = max 1/5 of cell count!
                n = (int)Math.Min((Math.Abs(Convert.ToDouble(textBox12.Text) - Convert.ToDouble(textBox13.Text)) / n), Math.Abs(Convert.ToDouble(textBox14.Text) - Convert.ToDouble(textBox15.Text)) / n);
                
                // n= minimal number of cells in x/y direction allowed for smoothing
                CellNrTopographySmooth = Math.Min(CellNrTopographySmooth, n);
                if (InputBox1("Define the number of cells at boundaries for smoothing topography", "Nr. of cells (default 0): ", 0, n, ref CellNrTopographySmooth) == DialogResult.Cancel)
                {
                    Cursor = Cursors.Arrow;
                    return;
                }

                CreateGrammGrid gr = new CreateGrammGrid
                {
                    SmoothBorderCellNr = CellNrTopographySmooth,
                    ProjectName = ProjectName
                };
                
                if (GRALSettings.Compressed > 0) // write compressed ggeom file if compressed con files = true
                {
                    gr.WriteCompressedFile = true;
                }

                if (gr.GenerateGgeomFile())
                {
                    Invoke(new showtopo(ShowTopo));
                }
                else
                {
                    throw new FileLoadException();
                }
                //generate pointer for location of wind field files
                GRAMMwindfield = Path.Combine(ProjectName, @"Computation") + Path.DirectorySeparatorChar;
                WriteFileGRAMMWindfeld_txt(ProjectName, GRAMMwindfield, true);
                
                Cursor = Cursors.Default;
                Textbox16_Set("GRAMM: " + GRAMMwindfield);
                label95.Text = "Number of cells used for smoothing orography laterally: " + Convert.ToString(CellNrTopographySmooth); // show number of smooth cells
                label95.Visible = true;
                //save GRAMM control file "GRAMMin.dat"
                GRAMMin(true);
            }
            catch(Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show("Unable to generate the GRAMM grid!" + Environment.NewLine + ex.Message.ToString(), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //enable/disable GRAMM simulations
            Enable_GRAMM();
        }


        private void ComputeGrammModelHeights()
        {
            double top = Convert.ToDouble(numericUpDown17.Value);
            GRAMMmodelheight.Items.Clear();
            GRAMMmodelheight.Items.Add("(0)" + "   " + Convert.ToString(Math.Round(top, 0)).PadLeft(2));
            for (int i = 2; i <= Convert.ToInt32(numericUpDown16.Value) + 1; i++)
            {
                top += Convert.ToDouble(numericUpDown17.Value) * Math.Pow(Convert.ToDouble(numericUpDown19.Value), i - 2);
                GRAMMmodelheight.Items.Add("(" + Convert.ToString(i - 1) + ")   " + Convert.ToString(Math.Round(top, 0)).PadLeft(2));
            }
            GRAMMmodelheight.SetSelected(Convert.ToInt32(numericUpDown16.Value), true);

        }


        //compute top height of GRAMM domain
        private void NumericUpDown17_ValueChanged(object sender, EventArgs e)
        {
            if (EmifileReset == true)
            {
                ComputeGrammModelHeights();

                //show file name in listbox
                textBoxGrammTerrain.Text = string.Empty;
                //delete files
                if (File.Exists(Path.Combine(ProjectName, @"Computation", "ggeom.asc")) ||
                    File.Exists(Path.Combine(ProjectName, @"Computation", "windfeld.txt")))
                {
                    using (FileDeleteMessage fdm = new FileDeleteMessage())
                    {
                        System.Collections.Generic.List<string> _message = new System.Collections.Generic.List<string>();
                        
                        if (File.Exists(Path.Combine(ProjectName, @"Computation", "ggeom.asc")))
                        {
                            _message.Add("..Computation" + Path.DirectorySeparatorChar + "ggeom.asc");
                        }

                        if (File.Exists(Path.Combine(ProjectName, @"Computation", "windfeld.txt")))
                        {
                            _message.Add("..Computation" + Path.DirectorySeparatorChar + "windfeld.txt");
                        }
                        fdm.ListboxEntries = _message;

                        if (fdm.ShowDialog() == DialogResult.OK)
                        {
                            DeleteFile(Path.Combine(ProjectName, @"Computation", "ggeom.asc"));
                            DeleteFile(Path.Combine(ProjectName, @"Computation", "windfeld.txt"));
                        }
                    }
                }

                //enable/disable GRAMM simulations
                Enable_GRAMM();
            }
        }

                private delegate void showtopo();
        
        //generating GRAMM grid was successful
        private void ShowTopo()
        {
            //show file name in listbox
            textBoxGrammTerrain.Text = Topofile;
            Cursor = Cursors.Default;

            //save file information
            try
            {
                using (StreamWriter mywriter = new StreamWriter(Path.Combine(ProjectName, @"Settings", "Topography.txt")))
                {
                    mywriter.WriteLine(Topofile);
                    mywriter.WriteLine(Convert.ToString(numericUpDown19.Value, ic));
                    mywriter.WriteLine(Convert.ToString(numericUpDown17.Value, ic));
                }
            }
            catch { MessageBox.Show("Error writing Topography.txt", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error); }

            //save windfield information
            /*GRAMMwindfield = Path.Combine(projectname, @"Computation\");
            mywriter = new StreamWriter(Path.Combine(projectname, @"Settings\Windfield.txt"));
            mywriter.WriteLine(GRAMMwindfield);
            mywriter.Close();*/

            //show controls for landuse file generation
            if (GRALSettings.BuildingMode != BuildingModeEnum.GRAMM)
            {
                button20.Visible = true;
                checkBox36.Visible = true;
                button43.Visible = true;
                textBoxGrammLandUseFile.Visible = true;
                button23.Visible = true;
                radioButton1.Checked = false;
                radioButton2.Checked = true;
                checkBox25.Visible = true;
                //check if GRAL_topofile.txt exists
                if (File.Exists(Path.Combine(ProjectName, @"Computation", "GRAL_topofile.txt")))
                {
                    checkBox25.Checked = true;
                }
                else
                {
                    checkBox25.Checked = false;
                }
            }

            button22.Visible = true;
            groupBox12.Visible = true;
        }
        
        //generating GRAMM grid failed
        public void HideTopo()
        {
            if (EmifileReset == true)
            {
                //show file name in listbox
                textBoxGrammTerrain.Text = string.Empty;    
                Cursor = Cursors.Default;

                DeleteFile(Path.Combine(ProjectName, @"Settings", "Topography.txt"));
                //save file information
                using (StreamWriter mywriter = new StreamWriter(Path.Combine(ProjectName, @"Settings", "Topography.txt")))
                {
                    mywriter.WriteLine("None");
                    mywriter.WriteLine(Convert.ToString(numericUpDown19.Value, ic));
                    mywriter.WriteLine(Convert.ToString(numericUpDown17.Value, ic));
                }

                //hide controls for landuse file generation
                button20.Visible = false;
                checkBox36.Visible = false;
                button43.Visible = false;
                button22.Visible = false;
                button23.Visible = false;
                textBoxGrammLandUseFile.Visible = false;
                radioButton1.Checked = true;
                radioButton2.Checked = false;
                checkBox25.Visible = false;
                checkBox25.Checked = false;
                groupBox12.Visible = false;

                //delete files
                if (File.Exists(Path.Combine(ProjectName, @"Computation", "ggeom.asc")) ||
                    File.Exists(Path.Combine(ProjectName, @"Computation", "windfeld.txt")) ||
                    File.Exists(Path.Combine(ProjectName, @"Computation", "GRAMM.geb")))
                {
                    using (FileDeleteMessage fdm = new FileDeleteMessage())
                    {

                        System.Collections.Generic.List<string> _message = new System.Collections.Generic.List<string>();

                        if (File.Exists(Path.Combine(ProjectName, @"Computation", "windfeld.txt")))
                        {
                            _message.Add("..Computation" + Path.DirectorySeparatorChar + "windfeld.txt");
                        }

                        if (File.Exists(Path.Combine(ProjectName, @"Computation", "ggeom.asc")))
                        {
                            _message.Add("..Computation" + Path.DirectorySeparatorChar + "ggeom.asc");
                        }

                        if (File.Exists(Path.Combine(ProjectName, @"Computation", "GRAMM.geb")))
                        {
                            _message.Add("..Computation" + Path.DirectorySeparatorChar + "GRAMM.geb");
                        }

                        fdm.ListboxEntries = _message;

                        if (fdm.ShowDialog() == DialogResult.OK)
                        {
                            DeleteFile(Path.Combine(ProjectName, @"Computation", "ggeom.asc"));
                            DeleteFile(Path.Combine(ProjectName, @"Computation", "GRAMM.geb"));
                            DeleteFile(Path.Combine(ProjectName, @"Computation", "windfeld.txt"));
                        }
                    }
                }

                //clear textboxes
                textBox12.Text = "";
                textBox13.Text = "";
                textBox14.Text = "";
                textBox15.Text = "";

                //delete landuse files
                HideLanduse();
            }
        }

        //remove GRAMM grid
        private void Button22_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete all GRAMM settings", "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                HideTopo();
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////
        //
        //generate GRAMM landuse file "landuse.asc"
        //
        ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Generate GRAMM landuse file "landuse.asc"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMLoadCreateLanduse(object sender, EventArgs e)
        {
            //open dialog window to select between existing land-use file or to use homogenous default values
            using (GralMainForms.LanduseSelect LS = new GralMainForms.LanduseSelect()
            {
                Owner = this
            })
            {
                if (LS.ShowDialog() == DialogResult.OK)
                {
                    if (LS.radioButton2.Checked == true)
                    {
                        //start procedure to generate land-use file landuse.asc
                        OpenFileDialog dialog = new OpenFileDialog
                        {
                            Filter = "Landuse files (*.txt;*.dat)|*.txt;*.dat",
                            Title = "Select Landuse File",
                            ShowHelp = true
#if NET6_0_OR_GREATER
                            ,ClientGuid = GralStaticFunctions.St_F.FileDialogTopo
#endif
                        };
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Landusefile = dialog.FileName;

                            //check whether defined GRAMM domain is within the selected landuse file
                            string[] data = new string[100];

                            StreamReader myreader = new StreamReader(Landusefile);
                            data = myreader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            int nx = Convert.ToInt32(data[1]);
                            data = myreader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            int ny = Convert.ToInt32(data[1]);
                            data = myreader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            double x11 = Convert.ToDouble(data[1].Replace(".", DecimalSep));
                            data = myreader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            double y11 = Convert.ToDouble(data[1].Replace(".", DecimalSep));
                            data = myreader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            double dx = Convert.ToDouble(data[1].Replace(".", DecimalSep));
                            myreader.Close();
                            myreader.Dispose();

                            if ((GrammDomRect.West < x11) || (GrammDomRect.East > x11 + dx * nx) || (GrammDomRect.South < y11) || (GrammDomRect.North > y11 + dx * ny))
                            {
                                MessageBox.Show("GRAMM Domain is outside the borders of the selected landuse file", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            if (dx > Convert.ToDouble(numericUpDown18.Value))
                            {
                                MessageBox.Show("The cellsize of the landuse file (" + dx.ToString() + " m)" + Environment.NewLine +
                                                "is lower than the GRAMM grid resolution"
                                                , "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                dialog.Dispose();
                                return;
                            }

                            try
                            {
                                Cursor = Cursors.WaitCursor;
                                //clear textbox
                                textBoxGrammLandUseFile.Text = string.Empty;
                                //string file = Path.GetDirectoryName(Application.ExecutablePath);

                                Cursor = Cursors.WaitCursor;
                                Landuse lu = new Landuse();
                                lu.GenerateLanduseFile(Landusefile, checkBox36.Checked);
                                if (lu.ok == true)
                                {
                                    Invoke(new showtopo(ShowLanduse));
                                }

                                Cursor = Cursors.Default;
                            }
                            catch
                            {
                                MessageBox.Show("Unable to generate landuse file", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Invoke(new showtopo(HideLanduse));
                            }
                            Cursor = Cursors.Arrow;
                        }
                    }
                    //use default value stored in the file Landuse_Default.txt
                    else
                    {
                        Landusefile = "Default-Values";

                        try
                        {
                            Cursor = Cursors.WaitCursor;
                            //clear listbox
                            textBoxGrammLandUseFile.Text = string.Empty;
                            //string file = Path.GetDirectoryName(Application.ExecutablePath);

                            Cursor = Cursors.WaitCursor;
                            Landuse lu = new Landuse();
                            lu.GenerateLanduseFile(Landusefile, checkBox36.Checked);
                            if (lu.ok == true)
                            {
                                Invoke(new showtopo(ShowLanduse));
                            }

                            Cursor = Cursors.Default;
                        }
                        catch
                        {
                            MessageBox.Show("Unable to generate landuse file", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Invoke(new showtopo(HideLanduse));
                        }
                    }

                    //enable/disable GRAMM simulations
                    Enable_GRAMM();
                }
            }
        }

       //generating GRAMM landuse file was successful
        private void ShowLanduse()
        {
            //show file name in textbox
            textBoxGrammLandUseFile.Text = Landusefile;
            Cursor = Cursors.Default;

            //save file information
            try
            {
                using (StreamWriter mywriter = new StreamWriter(Path.Combine(ProjectName, @"Settings", "Landuse.txt")))
                {
                    mywriter.WriteLine(Landusefile);
                }
            }
            catch
            {
                MessageBox.Show("Unable to write Landuse.txt", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        //generating GRAMM landuse file failed
        private void HideLanduse()
        {
            if (EmifileReset == true)
            {
                //delete file if content is available
                string landusefile = string.Empty;
                try
                {
                    //read file information
                    using (StreamReader myreader = new StreamReader(Path.Combine(ProjectName, @"Settings", "Landuse.txt")))
                    {
                        if (!myreader.EndOfStream)
                        {
                            landusefile = myreader.ReadLine();
                        }
                    }
                }
                catch { }

                if (!(String.IsNullOrEmpty(landusefile) || landusefile.Equals("None")))
                {
                    DeleteFile(Path.Combine(ProjectName, @"Computation", "landuse.asc"));
                    DeleteFile(Path.Combine(Path.Combine(ProjectName, @"Settings", "Landuse.txt")));
                }
                //delete file name in textbox
                textBoxGrammLandUseFile.Text = string.Empty;

                //save file information
                try
                {
                    using (StreamWriter mywriter = new StreamWriter(Path.Combine(ProjectName, @"Settings", "Landuse.txt")))
                    {
                        mywriter.WriteLine("None");
                    }
                }
                catch { MessageBox.Show("Error writing Landuse.txt", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                //File.Delete(Path.Combine(ProjectName, @"Computation", "windfeld.txt"));
            }
        }
        
        //remove landuse file
        private void Button23_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete all landuse settings?", "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                HideLanduse();
            }
        }    
    }
}