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

using GralIO;
using System;
using System.IO;
using System.Windows.Forms;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
    {

        /// <summary>
        /// set a reference to a GRAMM wind field and load the grid, landuse, and meteorology from existing project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetReferenceToExistingWindfields(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ProjectName))
            {
                return; // exit if no project loaded
            }

            bool other_folder = true; // Project folder and windfield folder are different -> otherwise change windfeld.txt only!

            //define path and name of existing GRAMM project
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = "Select GRAMM project directory",
            };
#if NET6_0_OR_GREATER
            dialog.UseDescriptionForTitle = true;
#endif
            dialog.SelectedPath = Path.GetDirectoryName(ProjectName);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                GRAMMproject = dialog.SelectedPath + Path.DirectorySeparatorChar;
                //import GRAMM grid file "ggeom.asc"
                try
                {
                    if (File.Exists(Path.Combine(GRAMMproject, @"GRAMM.geb")) == false) // not the computation folder?
                    {
                        GRAMMproject = Path.Combine(GRAMMproject, "Computation") + Path.DirectorySeparatorChar; // try path "Computation"
                    }

                    if (Path.Equals(GRAMMproject, Path.Combine(ProjectName, "Computation") + Path.DirectorySeparatorChar)) // Windfield is at the project folder?
                    {
                        other_folder = false;
                    }

                    if (other_folder)
                    {
                        File.Delete(Path.Combine(ProjectName, @"Computation", "ggeom.asc"));
                        using (StreamWriter mywriter = new StreamWriter(Path.Combine(ProjectName, @"Computation", "ggeom.asc")))
                        {
                            mywriter.WriteLine(Path.Combine(GRAMMproject, @"ggeom.asc"));
#if __MonoCS__
                            mywriter.WriteLine(Path.Combine(GRAMMproject, @"ggeom.asc"));
#endif
                        }
                    }
                    //File.Delete(Path.Combine(projectname, @"Computation", "ggeom.asc"));
                    //File.Copy(Path.Combine(GRAMMproject, @"ggeom.asc"), Path.Combine(projectname, @"Computation", "ggeom.dat"), true);
                    Topofile = Path.Combine(GRAMMproject, @"ggeom.asc");
                    textBoxGrammTerrain.Text = string.Empty;
                    ShowTopo();
                }
                catch
                {
                    MessageBox.Show(this, "Could not copy GRAMM grid 'ggeom.asc'.", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //import GRAMM control file "GRAMM.geb"
                try
                {
                    if (other_folder)
                    {
                        File.Delete(Path.Combine(ProjectName, @"Computation", "GRAMM.geb"));
                        File.Copy(Path.Combine(GRAMMproject, @"GRAMM.geb"), Path.Combine(ProjectName, @"Computation", "GRAMM.geb"), true);
                    }
                    string newPath2 = Path.Combine(ProjectName, @"Computation", "GRAMM.geb");
                    EmifileReset = false;

                    try
                    {
                        using (StreamReader myreader = new StreamReader(newPath2))
                        {
                            string[] text = new string[10];
                            double numbcell = 1;
                            text = myreader.ReadLine().Split(new char[] { '!' });
                            text[0] = text[0].Trim();
                            text[0] = text[0].Replace(".", DecimalSep);
                            numbcell = Convert.ToDouble(text[0]);
                            text = myreader.ReadLine().Split(new char[] { '!' });
                            text = myreader.ReadLine().Split(new char[] { '!' });
                            text[0] = text[0].Trim();
                            text[0] = text[0].Replace(".", DecimalSep);
                            numericUpDown16.Value = Convert.ToDecimal(text[0]);
                            text = myreader.ReadLine().Split(new char[] { ',', '!' });
                            text[0] = text[0].Trim();
                            textBox13.Text = text[0];
                            GrammDomRect.West = Convert.ToDouble(text[0]);
                            text = myreader.ReadLine().Split(new char[] { ',', '!' });
                            text[0] = text[0].Trim();
                            textBox12.Text = text[0];
                            GrammDomRect.East = Convert.ToDouble(text[0]);
                            text = myreader.ReadLine().Split(new char[] { ',', '!' });
                            text[0] = text[0].Trim();
                            textBox14.Text = text[0];
                            GrammDomRect.South = Convert.ToDouble(text[0]);
                            text = myreader.ReadLine().Split(new char[] { ',', '!' });
                            text[0] = text[0].Trim();
                            textBox15.Text = text[0];
                            GrammDomRect.North = Convert.ToDouble(text[0]);
                            GRAMMHorGridSize = (GrammDomRect.East - GrammDomRect.West) / numbcell;
                            numericUpDown18.Value = Convert.ToDecimal(GRAMMHorGridSize);
                            groupBox9.Visible = true;
                            textBoxGrammTerrain.Visible = true;
                            button22.Visible = true;
                            label95.Visible = true;
                            button19.Visible = true;
                        }
                    }
                    catch
                    {
                    }

                    if (other_folder)
                    {
                        // copy file GRAMMin.dat
                        try
                        {
                            string grammin = Path.Combine(ProjectName, "Computation", "GRAMMin.dat");
                            if (File.Exists(grammin))
                            {
                                File.Delete(grammin);
                            }

                            File.Copy(Path.Combine(Path.GetDirectoryName(GRAMMproject), "GRAMMin.dat"), grammin, true);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    EmifileReset = true;
                }
                catch
                {
                    MessageBox.Show(this, "Could not copy GRAMM control file 'GRAMM.geb'.", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //import GRAMM control file "IIN.dat"
                try
                {
                    if (other_folder)
                    {
                        File.Delete(Path.Combine(ProjectName, @"Computation", "IIN.dat"));
                        File.Copy(Path.Combine(GRAMMproject, @"IIN.dat"), Path.Combine(ProjectName, @"Computation", "IIN.dat"), true);
                    }
                    EmifileReset = false;
                    string newPath2 = Path.Combine(ProjectName, @"Computation", "IIN.dat");

                    try
                    {
                        using (StreamReader myreader = new StreamReader(newPath2))
                        {
                            string[] text = new string[10];
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            numericUpDown20.Value = Convert.ToInt32(text[1]);
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            numericUpDown21.Value = Convert.ToInt32(text[1]);
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            numericUpDown25.Value = Convert.ToInt32(text[1]);
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            numericUpDown22.Value = Convert.ToDecimal(text[1].Replace(".", DecimalSep));
                            text = myreader.ReadLine().Split(new char[] { ':', '!' });
                            numericUpDown23.Value = Convert.ToDecimal(text[1].Replace(".", DecimalSep));
                        }
                    }
                    catch
                    {
                    }
                    EmifileReset = true;
                }
                catch
                {
                    MessageBox.Show("Could not copy GRAMM control file 'IIN.dat'.", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //import GRAMM control file "reinit.dat"
                try
                {
                    if (File.Exists(Path.Combine(GRAMMproject, @"reinit.dat")) == true)
                    {
                        checkBox20.Checked = false;
                        if (other_folder)
                        {
                            File.Copy(Path.Combine(GRAMMproject, @"reinit.dat"), Path.Combine(ProjectName, @"Computation", "reinit.dat"), true);
                        }
                        EmifileReset = false;
                        string newPath2 = Path.Combine(ProjectName, @"Computation", "reinit.dat");

                        try
                        {
                            using (StreamReader myreader = new StreamReader(newPath2))
                            {
                                string text;
                                text = myreader.ReadLine();
                                int click = Convert.ToInt32(text);
                                if (click > 0)
                                {
                                    checkBox20.Checked = true;
                                }
                                else
                                {
                                    checkBox20.Checked = false;
                                }
                            }
                        }
                        catch
                        {
                        }
                        EmifileReset = true;
                    }
                }
                catch
                {
                    //MessageBox.Show("Could not copy GRAMM control file 'reinit.dat'.");
                }

                //import GRAMM meteorology files "meteopgt.all"
                try
                {
                    if (other_folder)
                    {
                        File.Delete(Path.Combine(ProjectName, @"Computation", "meteopgt.all"));
                        File.Copy(Path.Combine(GRAMMproject, @"meteopgt.all"), Path.Combine(ProjectName, @"Computation", "meteopgt.all"), true);
                    }
                    textBoxMeteoFile.Text = Path.Combine(GRAMMproject, @"meteopgt.all");
                    Textbox16_Set("GRAMM: " + GRAMMproject); // the actual met data are from a GRAMMProject
                                                             //button7.Visible = true;
                                                             //label32.Visible = true;
                                                             //label32.ForeColor = Color.Red;
                                                             //label63.Visible = false;
                }
                catch
                {
                    MessageBox.Show("Could not copy GRAMM meteo file 'meteopgt.all'.", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Set classified checkbox depending on the file meteopgt.all
                IO_ReadFiles OpenProject = new IO_ReadFiles
                {
                    ProjectName = ProjectName,
                    DispsituationFrequ = DispSituationfrequ
                };
                OpenProject.ReadMeteopgtAllFile();
                checkBox19.Checked = OpenProject.MeteoClassification; //  classification
                OpenProject = null;

                //import GRAMM meteorology file "mettimeseries.dat"
                try
                {
                    if (other_folder)
                    {
                        File.Delete(Path.Combine(ProjectName, @"Computation", "mettimeseries.dat"));
                        File.Copy(Path.Combine(GRAMMproject, @"mettimeseries.dat"), Path.Combine(ProjectName, @"Computation", "mettimeseries.dat"), true);
                    }
                    //label32.ForeColor = Color.Green;
                    //label63.Visible = true;
                    MetfileName = "";
                    File.Delete(Path.Combine(ProjectName, @"Settings", "Meteorology.txt"));
                    textBoxMeteoFile.Text = string.Empty;
                    Textbox16_Set("GRAMM: " + GRAMMproject); // the actual met data are from a GRAMMProject
                    groupBox1.Visible = false;
                    groupBox2.Visible = false;
                    checkBox19.Visible = false;
                    groupBox3.Visible = false;

                    label100.Visible = false;
                    groupBox20.Visible = false;
                    label101.Visible = false;
                    label102.Visible = false;

                    groupBox23.Visible = false; // Anemometer height

                    button6.Visible = false;
                }
                catch
                {
                    MessageBox.Show("Could not copy GRAMM meteo file 'mettimeseries.dat'.", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //import GRAMM landuse file "landuse.asc"
                try
                {
                    if (other_folder)
                    {
                        File.Delete(Path.Combine(ProjectName, @"Computation", "landuse.asc"));
                        File.Copy(Path.Combine(GRAMMproject, @"landuse.asc"), Path.Combine(ProjectName, @"Computation", "landuse.asc"), true);
                    }
                    Landusefile = Path.Combine(GRAMMproject, @"landuse.asc");
                    textBoxGrammLandUseFile.Text = string.Empty;
                    ShowLanduse();
                }
                catch
                {
                    MessageBox.Show("Could not copy GRAMM landuse file 'landuse.asc'.", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //import GRAMM windfield file "windfeld.txt"
                GRAMMwindfield = GRAMMproject;
                if (WriteFileGRAMMWindfeld_txt(ProjectName, GRAMMwindfield, false))
                {
                    Textbox16_Set("GRAMM: " + GRAMMwindfield); // the actual met data are from a GRAMMProject
                }
                else
                {
                    MessageBox.Show("Could not update 'windfeld.txt'.", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            Cursor = Cursors.Default;

            if (Directory.Exists(GRAMMwindfield))
            {
                if (CountWindFiles(GRAMMwindfield) > 0) // if windfields exist, then lock GRAMM
                {
                    GRAMM_Locked = true;                    // lock GRAMM project
                    Gramm_locked_buttonClick(null, null);   // change locked-Button
                }
            }
            //enable/disable GRAMM simulations
            Enable_GRAMM();
        }

    }
}