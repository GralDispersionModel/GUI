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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Gral
{
    public partial class Main
    {
        private void LoadProject(string foldername)
        {
            CultureInfo ic = CultureInfo.InvariantCulture;

            //try
            {
                if (!Directory.Exists(foldername))
                {
                    MessageBox.Show(this, "Can't find the project folder", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (File.Exists(Path.Combine(foldername, @"Settings", "Settings.ini"))) // old settings file exists
                {
                    if (File.Exists(Path.Combine(foldername, @"Settings", "Settings2.ini")) == false &&
                        File.Exists(Path.Combine(foldername, @"Settings", "Settings1.ini")) == false) // old project before V16.01 -> cannot open that file
                    {
                        MessageBox.Show(this, @"This project has been created with a version before GRAL V 16.01. Please open this project with a version between V 16.01 or V 19.01 and " +
                            "open the domain window and change any drawing setting (eg the order in the 'Object Manager'). " +
                            "Afterwards it should be possible to open the project with this GUI version", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                Cursor = Cursors.WaitCursor;

                EmifileReset = false;
                IndatReset = false;

                try
                {
                    if (DomainGISForm != null)
                    {
                        DomainGISForm.Close();
                        if (DomainClosedHandle != null)
                        {
                            DomainGISForm.DomainClosed -= DomainClosedHandle;
                        }

                        DomainGISForm = null;
                    }
                }
                catch { }

                ProjectName = foldername;

                // Reset old project
                textBoxGrammTerrain.Text = string.Empty;
                textBoxGrammLandUseFile.Text = string.Empty;

                label95.Visible = false;
                textBoxGrammTerrain.Visible = false;
                button19.Visible = false;
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
                groupBox15.Visible = false;
                groupBox16.Visible = false;
                groupBox17.Visible = false;

                GRAMM_Locked = false;
                GRAMMwindfield = null;
                GrammDomRect.East = 0; GrammDomRect.West = 0; GrammDomRect.North = 0; GrammDomRect.South = 0;

                //set header in main window
                Text = "GRAL GUI  / " + Path.GetFileName(ProjectName);
                GralStaticFunctions.St_F.SetTrimmedTextToTextBox(label87, ProjectName);

                //remove start up banner
                //this.panel1.Visible = false;
                button44.Visible = true; // show button for comments

                // test, if path exists, otherwise ask, if a new project should be created
                string newPath = Path.Combine(ProjectName, "Settings");
                if (Directory.Exists(newPath) == false)
                {
                    if (MessageBox.Show(this, "Create a new project?", "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        Cursor = Cursors.Default;
                        return;
                    }
                }

                newPath = System.IO.Path.Combine(ProjectName, "Metfiles");

                GUISettings.PreviousUsedProjectPath = ProjectName;
                GUISettings.WriteToFile();

                try
                {
                    //create subdirectories
                    System.IO.Directory.CreateDirectory(newPath);

                    newPath = System.IO.Path.Combine(ProjectName, "Computation");
                    System.IO.Directory.CreateDirectory(newPath);

                    newPath = System.IO.Path.Combine(ProjectName, "Emissions");
                    System.IO.Directory.CreateDirectory(newPath);

                    newPath = System.IO.Path.Combine(ProjectName, "Maps");
                    System.IO.Directory.CreateDirectory(newPath);

                    newPath = System.IO.Path.Combine(ProjectName, "Settings");
                    System.IO.Directory.CreateDirectory(newPath);

                    Thread.Sleep(250);
                }
                catch
                {
                    Cursor = Cursors.Arrow;
                    MessageBox.Show(this, "Can't open this project. Are the folders write protected?", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // load paths for evaluations results and the emission modulation
                ProjectSetting = new GralData.ProjectSettings(ProjectName);
                ProjectSetting.ReadFromFile();

                MaxProcFileRead(); // read and set the count of max processors
                listBox4.Items.Clear(); // delete sourcegroups
                listView1.Clear(); // delete sourcegroups

                //start file watcher for Percent.txt
#if __MonoCS__
#else
                try
                {
                    percentGRAL.Path = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                    percentGRAL.Filter = "Percent.txt";
                    percentGRAL.NotifyFilter = NotifyFilters.LastWrite;
                    percentGRAL.Changed += new FileSystemEventHandler(PercentChanged);
                    //percent.SynchronizingObject = this;
                    percentGRAL.EnableRaisingEvents = false;
                }
                catch { }

                try
                {
                    //start file watcher for DispNr.txt
                    dispnrGRAL.Path = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                    dispnrGRAL.Filter = "DispNr.txt";
                    dispnrGRAL.NotifyFilter = NotifyFilters.LastWrite;
                    dispnrGRAL.Changed += new FileSystemEventHandler(DispNrChanged);
                    //dispnr.SynchronizingObject = this;
                    dispnrGRAL.EnableRaisingEvents = false;
                }
                catch { }

                //start file watcher for PercentGramm.txt
                try
                {
                    percentGramm.Path = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                    percentGramm.Filter = "PercentGramm.txt";
                    percentGramm.NotifyFilter = NotifyFilters.LastWrite;
                    percentGramm.Changed += new FileSystemEventHandler(PercentGrammChanged);
                    //percentGramm.SynchronizingObject = this;
                    percentGramm.EnableRaisingEvents = false;
                }
                catch { }
                try
                {
                    //start file watcher for DispNrGramm.txt
                    dispnrGramm.Path = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                    dispnrGramm.Filter = "DispNrGramm.txt";
                    dispnrGramm.NotifyFilter = NotifyFilters.LastWrite;
                    dispnrGramm.Changed += new FileSystemEventHandler(DispnrGrammChanged);
                    //dispnrGramm.SynchronizingObject = this;
                    dispnrGramm.EnableRaisingEvents = false;
                }
                catch { }

                //start file watcher for Problemreport.txt
                try
                {
                    ProblemreportGRAMM.Path = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                    ProblemreportGRAMM.Filter = "Problemreport.txt";
                    ProblemreportGRAMM.NotifyFilter = NotifyFilters.LastWrite;
                    ProblemreportGRAMM.Changed += new FileSystemEventHandler(ProblemreportGrammChanged);
                    //Problemreport_GRAMM.SynchronizingObject = this;
                    ProblemreportGRAMM.EnableRaisingEvents = true;
                }
                catch { }

                //start file watcher for Problemreport_GRAL.txt
                try
                {
                    ProblemreportGRAL.Path = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                    ProblemreportGRAL.Filter = "Problemreport_GRAL.txt";
                    ProblemreportGRAL.NotifyFilter = NotifyFilters.LastWrite;
                    ProblemreportGRAL.Changed += new FileSystemEventHandler(ProblemreportGralChanged);
                    //Problemreport_GRAL.SynchronizingObject = this;
                    ProblemreportGRAL.EnableRaisingEvents = true;
                }
                catch { }
#endif

                IO_ReadFiles OpenProject = new IO_ReadFiles
                {
                    ProjectName = ProjectName
                };

                //show message window during loading process
                GralMessage.MessageWindow message = new GralMessage.MessageWindow
                {
                    Titlebar = "Load project info"
                };
                message.Show();
                message.listBox1.Items.Add("Loading project " + ProjectName);

                //open file GRAMMin.dat
                message.listBox1.Items.Add("Loading control file GRAMMin.dat...");
                message.Refresh();

                if (OpenProject.ReadGrammInFile() == true)
                {
                    numericUpDown38.Value = Convert.ToDecimal(OpenProject.Roughness, ic);
                    GRALSettings.Roughness = (double)numericUpDown38.Value;

                    if (OpenProject.GRAMMstartsituation > 0 && OpenProject.GRAMMstartsituation <= numericUpDown24.Maximum)
                    {
                        numericUpDown24.Value = OpenProject.GRAMMstartsituation;
                    }

                    label95.Visible = false;

                    if (OpenProject.GRAMMsmooth.Length > 0)
                    {
                        label95.Text = "Number of cells used for smoothing orography laterally: " + OpenProject.GRAMMsmooth;
                        label95.Visible = true;
                        CellNrTopographySmooth = Convert.ToInt32(OpenProject.GRAMMsmooth); // that ensures, that the file is written alright, if an new calculation is started at a highermet situation
                    }

                    GRAMM_Sunrise = OpenProject.GRAMMsunrise;

                    checkBox27.Checked = OpenProject.GRAMMsteadystate;
                    if (GRAMM_Sunrise > 0)
                    {
                        checkBox30.Checked = true;
                    }
                    else
                    {
                        checkBox30.Checked = false;
                    }
                }
                else
                {
                    message.listBox1.Items.Add("Can't open control file GRAMMin.dat...");
                    message.Refresh();
                }

                //check current status of the simulations
                progressBar1.Value = 0;
                progressBar2.Value = 0;
                progressBar3.Value = 0;
                progressBar4.Value = 0;
                label68.Text = "Actual dispersion situation: 0%";
                label69.Text = "Dispersion situation: " + "0" + "/" + "0" + "%";
                label66.Text = "Flow situation: " + "0" + "/" + "0" + "%";
                label67.Text = "Actual flow situation: 0%";

                message.listBox1.Items.Add("Loading GRAL DispNr.txt...");
                message.Refresh();

                if (OpenProject.ReadDispNrFile() == true && OpenProject.GRALTrackbar > 0) // GRAL trackbar info readable
                {
                    double frequency = 0;
                    int trackbar = OpenProject.GRALTrackbar;

                    //get information about the maximum number of dispersion situations for the progress bar
                    OpenProject.DispsituationFrequ = DispSituationfrequ;
                    OpenProject.ReadMeteopgtAllFile();
                    checkBox19.Checked = OpenProject.MeteoClassification; //  classification
                    DispSituationfrequ = OpenProject.DispsituationFrequ;
                    frequency = OpenProject.MeteoFrequ;

                    //check if GRAL is operated in transient mode
                    int transient = 1;
                    int weathersit_count = 0;
                    try
                    {
                        InDatVariables data1 = new InDatVariables();
                        InDatFileIO ReadInData1 = new InDatFileIO();
                        data1.InDatPath = Path.Combine(ProjectName, "Computation", "in.dat");
                        ReadInData1.Data = data1;
                        if (ReadInData1.ReadInDat() == true)
                        {
                            if (data1.Transientflag == 0)
                            {
                                transient = 0;
                                //read mettimeseries.dat
                                string mettimeseries = Path.Combine(ProjectName, @"Computation", "mettimeseries.dat");
                                List<string> data_mettimeseries = new List<string>();
                                ReadMetTimeSeries(mettimeseries, ref data_mettimeseries);
                                weathersit_count = Math.Max(data_mettimeseries.Count, 1);
                                //if (data_mettimeseries.Count == 0) // no data available
                                //{
                                //    MessageBox.Show(this, "mettimeseries.dat not available -> correct visualization of the simulation progress not possible");
                                //}
                            }
                        }
                    }
                    catch
                    { }

                    if (transient == 1)
                    {
                        progressBar4.Maximum = 100;
                        frequency = 0;
                        for (int i = 0; i < trackbar; i++)
                        {
                            frequency += DispSituationfrequ[i];
                        }

                        if (frequency <= progressBar4.Maximum)
                        {
                            progressBar4.Value = Convert.ToInt32(frequency);
                        }
                    }
                    else
                    {
                        progressBar4.Maximum = Convert.ToInt32(weathersit_count);
                        frequency = trackbar / weathersit_count;
                        if (frequency <= progressBar4.Maximum)
                        {
                            progressBar4.Value = Convert.ToInt32(frequency);
                        }
                    }

                    label69.Text = "Dispersion situation: " + Convert.ToString(trackbar) + "/" + Convert.ToString(Math.Round(frequency / 10, 1)) + "%";
                    progressBar4.Invalidate();
                    Refresh();
                }
                else
                {
                    message.listBox1.Items.Add("Can't open GRAL DispNr.txt...");
                    message.Refresh();
                }

                message.listBox1.Items.Add("Loading GRAMM DispNrGRAMM.txt...");
                message.Refresh();

                if (OpenProject.ReadDispGrammNrFile() == true) // GRAMM trackbar info readable
                {
                    double frequency = 0;
                    int trackbar = OpenProject.GRAMMTrackbar;

                    OpenProject.DispsituationFrequ = DispSituationfrequ;
                    OpenProject.ReadMeteopgtAllFile();
                    DispSituationfrequ = OpenProject.DispsituationFrequ;
                    frequency = OpenProject.MeteoFrequ;
                    progressBar1.Maximum = Convert.ToInt32(frequency);

                    frequency = 0;
                    for (int i = 0; i < trackbar - 1; i++)
                    {
                        frequency += DispSituationfrequ[i];
                    }

                    if (frequency <= progressBar1.Maximum)
                    {
                        progressBar1.Value = Convert.ToInt32(frequency);
                    }

                    label66.Text = "Flow situation: " + Convert.ToString(trackbar - 1) + "/" + Convert.ToString(Math.Round(frequency / 10, 1)) + "%";

                    progressBar4.Invalidate();
                    Refresh();
                }
                else
                {
                    message.listBox1.Items.Add("Can't open GRAMM DispNrGRAMM.txt...");
                    message.Refresh();
                }

                /*if (File.Exists(Path.Combine(projectname, @"Settings\Windfield.txt")) == false)
                {
                    GRAMMwindfield = Path.Combine(projectname, @"Computation\");
                    StreamWriter mywriter = new StreamWriter(Path.Combine(projectname, @"Settings\Windfield.txt"));
                    mywriter.WriteLine(GRAMMwindfield);
                    mywriter.Close();
                }*/

                IndatReset = false;
                //delete all actual loaded data
                for (int i = 0; i < 10; i++)
                {
                    if (i > 0)
                    {
                        groupBox5.Controls.Remove(TBox3[i]);
                        groupBox5.Controls.Remove(LbTbox3[i]);
                    }
                    groupBox5.Controls.Remove(TBox[i]);
                    groupBox5.Controls.Remove(LbTbox[i]);
                    groupBox5.Controls.Remove(NumUpDown[i]);
                    groupBox5.Controls.Remove(LbTbox2[i]);
                }
                textBoxMeteoFile.Text = string.Empty;

                listBox4.Items.Clear();
                listBox5.Items.Clear();

                listView1.Items.Clear();
                Pollmod.Clear();
                button10.Visible = false;
                button6.Visible = false;
                button7.Visible = false;
                button18.Visible = false;
                button21.Visible = false;
                button48.Visible = false;
                button49.Visible = false;
                button51.Visible = false;
                button13.Visible = false;
                button9.Visible = false;
                groupBox23.Visible = false; // Anemometer height
                label100.Visible = false;
                groupBox20.Visible = false;
                ChangeButtonLabel(ButtonColorEnum.ButtonControl, ButtonColorEnum.Invisible);  // Control not visible
                ChangeButtonLabel(ButtonColorEnum.ButtonMeteo, ButtonColorEnum.Invisible);  // meteo not visible
                ChangeButtonLabel(Gral.ButtonColorEnum.ButtonEmission, ButtonColorEnum.Invisible); // Emission label not visible
                ChangeButtonLabel(ButtonColorEnum.ButtonBuildings, ButtonColorEnum.Invisible); // Building label not visible

                GralDomRect.East = 0;
                GralDomRect.West = 0;
                GralDomRect.North = 0;
                GralDomRect.South = 0;
                textBox2.Text = "";
                textBox5.Text = "";
                textBox6.Text = "";
                textBox7.Text = "";
                textBox12.Text = "";
                textBox13.Text = "";
                textBox14.Text = "";
                textBox15.Text = "";
                groupBox8.Visible = false;
                groupBox2.Visible = false;
                checkBox19.Visible = false;
                groupBox3.Visible = false;
                groupBox1.Visible = false;
                groupBox9.Visible = false;

                CellsGralX = 0;
                CellsGralY = 0;
                TBox3[0].Value = 3;
                GRALSettings.BuildingMode = BuildingModeEnum.None;
                radioButton3.Checked = true;

                groupBox4.Visible = true;
                groupBox5.Visible = true;
                groupBox7.Visible = true;
                button10.Visible = true;
                //button11.Visible = true;
                //button12.Visible = true;
                button14.Visible = true;
                button15.Visible = true;
                OpenMetFile.Visible = true;
                radioButton1.Checked = true;
                radioButton2.Checked = false;
                checkBox25.Visible = false;
                checkBox25.Checked = false;

                //load existing source group definitions if available
                message.listBox1.Items.Add("Loading source group definitions...");
                message.Refresh();
                if (OpenProject.ReadSourcegroupsTextFile() == true)
                {
                    DefinedSourceGroups.Clear();
                    DefinedSourceGroups = OpenProject.Source_Group_List;
                }
                else
                {
                    message.listBox1.Items.Add("Can't open source group definitions...");
                    message.Refresh();
                }

                //load the file "in.dat"
                message.listBox1.Items.Add("Loading control file \"in.dat\"...");
                message.Refresh();

                //InDatVariables data = new InDatVariables();
                InDatFileIO ReadInData = new InDatFileIO();

                GRALSettings.InDatPath = Path.Combine(ProjectName, @"Computation", "in.dat");
                //data.Horslices = Horslices; // initialize array object !
                ReadInData.Data = GRALSettings;

                if (ReadInData.ReadInDat() == true)
                {
                    numericUpDown6.Value = Convert.ToDecimal(GRALSettings.ParticleNumber);
                    numericUpDown4.Value = Convert.ToDecimal(GRALSettings.DispersionTime);
                    numericUpDown38.Value = Convert.ToDecimal(GRALSettings.Roughness);
                    if (GRALSettings.AdaptiveRoughness > 0)
                    {
                        checkBox29.Checked = true;
                    }
                    else
                    {
                        checkBox29.Checked = false;
                    }
                    numericUpDown45.Value = Convert.ToDecimal(GRALSettings.AdaptiveRoughness);

                    numericUpDown39.Value = Convert.ToDecimal(GRALSettings.Latitude.ToString());
                    numericUpDown42.Value = (decimal)GRALSettings.PrognosticSubDomains;
                    GRALSettings.NumHorSlices = GRALSettings.NumHorSlices; // number of slices
                    numericUpDown8.Value = Convert.ToDecimal(GRALSettings.Deltaz);
                    if (GRALSettings.DispersionSituation > 0 && GRALSettings.DispersionSituation <= numericUpDown5.Maximum)
                    {
                        numericUpDown5.Value = Convert.ToDecimal(GRALSettings.DispersionSituation);
                    }

                    SetBuildingRadioButton();

                    ChangeButtonLabel(ButtonColorEnum.ButtonControl, ButtonColorEnum.BlackHook); // Control label OK
                    checkBox23.Checked = GRALSettings.BuildingHeightsWrite; // write building heights?
                    numericUpDown43.Value = GRALSettings.Compressed;        // use compressed *.con files?

                    // check if vertical concetration file should be written
                    string name = Path.Combine(ProjectName, @"Computation", "GRAL_Vert_Conc.txt");
                    if (File.Exists(name))
                    {
                        checkBox34.Checked = true;
                    }
                    else
                    {
                        checkBox34.Checked = false;
                    }

                    if (GRALSettings.Transientflag == 1)
                    {
                        checkBox32.Checked = false;
                        checkBox34.Enabled = false;
                        checkBox34.Checked = false;
                        label13.Enabled = false;
                        numericUpDown34.Enabled = false;
                        groupBox21.Visible = false; // Wet Deposition settings
                        numericUpDown5.Enabled = true; // enable setting for start dispersion situation
                    }
                    else
                    {
                        checkBox32.Checked = true;
                        checkBox34.Enabled = true;
                        label13.Enabled = true;
                        numericUpDown34.Enabled = true;
                        numericUpDown5.Value = 1;
                        numericUpDown5.Enabled = false; // disable setting for start dispersion situation
                        groupBox21.Visible = true; // Wet Deposition settings
                        try
                        {
                            //read cut-off concentration for transient simulations
                            message.listBox1.Items.Add("Loading cut-off concentration \"GRAL_Trans_Conc_Threshold.txt\"...");
                            message.Refresh();
                            OpenProject.ReadTransConcThreshold();
                            numericUpDown34.Value = OpenProject.TransConcThreshold;
                        }
                        catch { }
                    }
                }
                else
                {
                    message.listBox1.Items.Add("Can't open control file \"in.dat\"...");
                    message.Refresh();
                }

                //data = null;
                ReadInData = null;


                string[] sourcegr = new string[101];
                OpenProject.Source_group_list = sourcegr; // define object!

                //read the file "GRAL.geb"
                message.listBox1.Items.Add("Loading domain information \"GRAL.geb\"...");
                message.Refresh();
                if (OpenProject.ReadGRALGebFile() == true) // GRAMM trackbar info readable
                {
                    sourcegr = OpenProject.Source_group_list;
                    numericUpDown10.ValueChanged -= new System.EventHandler(NumericUpDown10_ValueChanged); // remove event listener
                    numericUpDown10.Maximum = OpenProject.FlowfieldGrid;
                    numericUpDown10.Value = OpenProject.FlowfieldGrid;
                    numericUpDown10.ValueChanged += new System.EventHandler(NumericUpDown10_ValueChanged); // add event listener

                    numericUpDown11.Value = OpenProject.FlowfieldVertical;
                    numericUpDown12.Value = OpenProject.FlowfieldStretch;
                    FlowFieldStretchFlexible = OpenProject.FlowFieldStretchFlexible;

                    CellsGralX = OpenProject.CellsGralX;
                    CellsGralY = OpenProject.CellsGralY;
                    GRALSettings.NumHorSlices = OpenProject.NumhorSlices;

                    textBox6.Text = Convert.ToString(OpenProject.West);
                    GralDomRect.West = OpenProject.West;
                    textBox7.Text = Convert.ToString(OpenProject.East);
                    GralDomRect.East = OpenProject.East;
                    textBox5.Text = Convert.ToString(OpenProject.South);
                    GralDomRect.South = OpenProject.South;
                    textBox2.Text = Convert.ToString(OpenProject.North);
                    GralDomRect.North = OpenProject.North;

                    HorGridSize = (GralDomRect.East - GralDomRect.West) / CellsGralX;
                    numericUpDown9.Value = Convert.ToDecimal(HorGridSize);

                    numericUpDown10.Value = Math.Min(numericUpDown10.Value, numericUpDown9.Value);
                    numericUpDown10.Maximum = numericUpDown9.Value; // Maximum of Flow Grid = counting grid!

                    message.listBox1.Items.Add("Update source groups within the model domain...");
                    message.Refresh();

                    //update source groups within the defined model domain
                    SelectAllUsedSourceGroups();

                    message.listBox1.Items.Add("Update selected source groups for the simulation...");
                    message.Refresh();
                    //fill listview1 with the selected source groups for the simulation
                    for (int k = 0; k < sourcegr.Length - 2; k++)
                    {
                        sourcegr[k] = sourcegr[k].Trim();
                        int count = 0;
                        if (DefinedSourceGroups.Count > 0)
                        {
                            bool exist = false;
                            foreach (SG_Class _sg in DefinedSourceGroups)
                            {
                                if (sourcegr[k] == _sg.SG_Number.ToString())
                                {
                                    listView1.Items.Add(DefinedSourceGroups[count].SG_Name + ": " + DefinedSourceGroups[count].SG_Number.ToString());
                                    listView1.Items[listView1.Items.Count - 1].SubItems[0].BackColor = Color.LightGoldenrodYellow;
                                    exist = true;
                                    break;
                                }
                                count += 1;
                            }
                            if (exist == false)
                            {
                                listView1.Items.Add(sourcegr[k]);
                                listView1.Items[listView1.Items.Count - 1].SubItems[0].BackColor = Color.LightGoldenrodYellow;
                            }
                        }
                        else
                        {
                            listView1.Items.Add(sourcegr[k]);
                            listView1.Items[listView1.Items.Count - 1].SubItems[0].BackColor = Color.LightGoldenrodYellow;
                        }
                    }
                }
                else
                {
                    message.listBox1.Items.Add("Can't open domain information \"GRAL.geb\"...");
                    message.Refresh();
                }

                //read file micro_vert_layers.txt
                message.listBox1.Items.Add("Loading flow field microlayers");
                if (OpenProject.ReadMicroVertLayers() == true)
                {
                    numericUpDown26.Value = OpenProject.FlowfieldMicrolayers;
                }
                else
                {
                    message.listBox1.Items.Add("Can't open flow field microlayers");
                }

                //read file relaxation_factors.txt
                message.listBox1.Items.Add("Loading flow field relaxation factors");
                if (OpenProject.ReadRelaxFactorsFile() == true)
                {
                    ProjectSetting.RelaxationFactorGRALVelocity = (double)OpenProject.RelaxVel;
                    ProjectSetting.RelaxationFactorGRALPressure = (double)OpenProject.RelaxPress;
                }
                else
                {
                    message.listBox1.Items.Add("No indiuvidual flow field relaxation factors defined");
                }

                //read file building_roughness.txt
                message.listBox1.Items.Add("Loading flow field building roughness");
                if (OpenProject.ReadBuildingRoughnessFile() == true)
                {
                    numericUpDown31.Value = OpenProject.BuildingRough;
                }
                else
                {
                    message.listBox1.Items.Add("Can't open flow field building roughness");
                }

                //read file GRAL_Topography.txt
                string graltopo = Path.Combine(ProjectName, @"Computation", "GRAL_Topography.txt");
                if (File.Exists(graltopo))
                {
                    checkBox24.Checked = true;
                }
                else
                {
                    checkBox24.Checked = false;
                }

                //read file GRAL_FlowFields.txt
                string gralflowfields = Path.Combine(ProjectName, @"Computation", "GRAL_FlowFields.txt");

                if (File.Exists(gralflowfields))
                {
                    int _compression = 0;
                    try
                    {
                        using (StreamReader reader = new StreamReader(gralflowfields))
                        {
                            if (!reader.EndOfStream)
                            {
                                _compression = Convert.ToInt32(reader.ReadLine());
                            }
                        }
                    }
                    catch { }
                    checkBox26.Checked = true;
                    numericUpDown41.Value = (decimal)Math.Max(Math.Min(_compression, numericUpDown41.Maximum), 0);
                }
                else
                {
                    checkBox26.Checked = false;
                    numericUpDown41.Value = 0;
                }

                //read file Integrationtime.txt
                message.listBox1.Items.Add("Loading GRAL Integrationtime.txt");
                if (OpenProject.ReadIntegrationTimeFile() == true)
                {
                    if (OpenProject.IntegrationMin > 1 && OpenProject.IntegrationMin < 10000)
                    {
                        numericUpDown29.Value = OpenProject.IntegrationMin;
                    }

                    if (OpenProject.IntegrationMax > 99 && OpenProject.IntegrationMax < 10002)
                    {
                        numericUpDown30.Value = OpenProject.IntegrationMax;
                    }

                    //check, if user selected steady-state conditions
                    if ((numericUpDown29.Value == 10000) && (numericUpDown30.Value == 10001))
                    {
                        checkBox22.Checked = true;
                        numericUpDown29.Visible = false;
                        numericUpDown30.Visible = false;
                        label78.Visible = false;
                        label79.Visible = false;
                    }
                    else
                    {
                        numericUpDown29.Visible = true;
                        numericUpDown30.Visible = true;
                        label78.Visible = true;
                        label79.Visible = true;
                    }
                }
                else
                {
                    message.listBox1.Items.Add("Can't open GRAL Integrationtime.txt");
                }


                //read the file "GRAMM.geb"
                message.listBox1.Items.Add("Loading domain information \"GRAMM.geb\"...");
                message.Refresh();

                if (OpenProject.ReadGrammGebFile() == true)
                {
                    numericUpDown16.Value = OpenProject.GRAMMvertlayers;
                    textBox13.Text = Convert.ToString(OpenProject.West);
                    GrammDomRect.West = OpenProject.West;
                    textBox12.Text = Convert.ToString(OpenProject.East);
                    GrammDomRect.East = OpenProject.East;
                    textBox14.Text = Convert.ToString(OpenProject.South);
                    GrammDomRect.South = OpenProject.South;
                    textBox15.Text = Convert.ToString(OpenProject.North);
                    GrammDomRect.North = OpenProject.North;
                    if (OpenProject.GRAMMnumbcell > 0)
                    {
                        GRAMMHorGridSize = (GrammDomRect.East - GrammDomRect.West) / OpenProject.GRAMMnumbcell;
                    }

                    numericUpDown18.Value = Convert.ToDecimal(GRAMMHorGridSize);
                    groupBox9.Visible = true;
                    textBoxGrammTerrain.Visible = true;
                    button22.Visible = true;
                    label95.Visible = true;
                    button19.Visible = true;
                }
                else
                {
                    message.listBox1.Items.Add("Can't open domain information \"GRAMM.geb\"...");
                }

                //read the file "IIN.dat"
                message.listBox1.Items.Add("Loading GRAMM control file information \"IIN.dat\".....");
                message.Refresh();

                if (OpenProject.ReadGrammIinFile() == true)
                {
                    int yy = 2000;
                    int mm = 1;
                    int dd = 1;
                    int hh = 0;
                    int mi = 0;

                    numericUpDown20.Value = OpenProject.GRAMMTimeStep; // max Time step
                    numericUpDown21.Value = OpenProject.GRAMMModellingTime;   // Modelling time
                    numericUpDown25.Value = OpenProject.GRAMMDebugLevel; // Debug level
                    numericUpDown22.Value = OpenProject.GRAMMRelaxvel; // Relax vel
                    numericUpDown23.Value = OpenProject.GRAMMRelaxscal; // Relax scal
                    checkBox21.Checked = OpenProject.GRAMMCatabatic; // catabatic forcing
                    checkBox31.Checked = OpenProject.GRAMMBoundaryCondition; //flat terrain settings
                    checkBox35.Checked = OpenProject.GRAMMERA5;  //large-scale forcing using ERA5
                    try
                    {
                        if (OpenProject.GRAMMERA5DateTime.Length == 10)
                        {
                            yy = 2000 + Convert.ToInt16(OpenProject.GRAMMERA5DateTime.Substring(0, 2));
                            mm = Convert.ToInt16(OpenProject.GRAMMERA5DateTime.Substring(2, 2));
                            dd = Convert.ToInt16(OpenProject.GRAMMERA5DateTime.Substring(4, 2));
                            hh = Convert.ToInt16(OpenProject.GRAMMERA5DateTime.Substring(6, 2));
                            mi = Convert.ToInt16(OpenProject.GRAMMERA5DateTime.Substring(8, 2));
                        }
                        else if (OpenProject.GRAMMERA5DateTime.Length == 12)
                        {
                            yy = Convert.ToInt16(OpenProject.GRAMMERA5DateTime.Substring(0, 4));
                            mm = Convert.ToInt16(OpenProject.GRAMMERA5DateTime.Substring(4, 2));
                            dd = Convert.ToInt16(OpenProject.GRAMMERA5DateTime.Substring(6, 2));
                            hh = Convert.ToInt16(OpenProject.GRAMMERA5DateTime.Substring(8, 2));
                            mi = Convert.ToInt16(OpenProject.GRAMMERA5DateTime.Substring(10, 2));
                        }
                        dateTimePicker1.Value = new DateTime(yy, mm, dd, hh, mi, 00);
                    }
                    catch { }
                }
                else
                {
                    message.listBox1.Items.Add("Can't open GRAMM control file information \"IIN.dat\".....");
                    message.Refresh();
                }

                message.listBox1.Items.Add("Loading GRAMM coordinate system \"GRAMM-coordinatesystem.txt\".....");
                message.Refresh();
                try
                {
                    if (File.Exists(Path.Combine(ProjectName, @"Computation", "GRAMM-coordinatesystem.txt")))
                    {
                        using (StreamReader myreader = new StreamReader(Path.Combine(ProjectName, @"Computation", "GRAMM-coordinatesystem.txt")))
                        {
                            Int32 text;
                            if (!myreader.EndOfStream)
                            {
                                text = Convert.ToInt32(myreader.ReadLine());
                                numericUpDown40.Value = Convert.ToDecimal(text);
                            }
                        }
                    }
                }
                catch { }

                message.listBox1.Items.Add("Loading GRAMM control file information \"reinit.dat\".....");
                message.Refresh();
                //read the file "reinit.dat"
                try
                {
                    checkBox20.Checked = false;
                    string newPath2 = Path.Combine(ProjectName, @"Computation", "reinit.dat");
                    using (StreamReader myreader = new StreamReader(newPath2))
                    {
                        try
                        {
                            string text = "0";
                            if (!myreader.EndOfStream)
                            {
                                text = myreader.ReadLine();
                            }
                            int clicked = Convert.ToInt32(text);
                            if (clicked > 0)
                            {
                                checkBox20.Checked = true;
                            }
                            else
                            {
                                checkBox20.Checked = false;
                            }
                        }
                        catch
                        { }
                    }
                }
                catch
                {
                }

                numericUpDown3.Value = GRALSettings.NumHorSlices;
                Object sender1 = new object();
                EventArgs e3 = new EventArgs();
                NumericUpDown3_ValueChanged(sender1, e3);
                for (int i = 0; i < GRALSettings.NumHorSlices; i++)
                {
                    TBox3[i].Value = (decimal)GRALSettings.HorSlices[i];
                }
                IndatReset = true;

                message.listBox1.Items.Add("Checking source group emission modulations...");
                message.Refresh();
                //load information, for which source groups emission modulations have already been defined
                try
                {
                    for (int index = 0; index < listView1.Items.Count; index++)
                    {
                        string[] sg = new string[2];
                        sg = listView1.Items[index].SubItems[0].Text.Split(new char[] { ':' });
                        string name = "";
                        try
                        {
                            sg[1] = sg[1].Trim();
                            name = "00" + sg[1];
                        }
                        catch
                        {
                            name = "00" + sg[0];
                        }
                        name = name.Substring(name.Length - 3);
                        newPath = Path.Combine(ProjectName, @"Computation", "emissions" + name + ".dat");
                        if (Directory.Exists(ProjectSetting.EmissionModulationPath))
                        {
                            newPath = Path.Combine(ProjectSetting.EmissionModulationPath, "emissions" + name + ".dat");
                        }
                        if (File.Exists(newPath))
                        {
                            listView1.Items[index].SubItems[0].BackColor = Color.LightGreen;
                        }
                    }
                }
                catch
                {
                }
                WriteGralGebFile();

                message.listBox1.Items.Add("Update pollutants for the selected source groups...");
                message.Refresh();
                CollectAllUsedPollutants();

                //fill listbox5 with the existing pollutants for the selected source groups within the model domain
                foreach (string text in Pollmod)
                {
                    button18.Visible = true;
                    button21.Visible = true;
                    button48.Visible = true;
                    ChangeButtonLabel(Gral.ButtonColorEnum.ButtonEmission, ButtonColorEnum.RedDot); // Emission label red

                    listBox5.Items.Add(text);
                    listBox5.SelectedIndex = 0;
                }

                //Read Building Coverage Threshold
                ReadBuildingCoverageThreshold();

                //set index in listbox5 to the selected pollutant
                string newpath = Path.Combine(ProjectName, @"Computation", "Pollutant.txt");
                if (File.Exists(newpath) == false)
                {
                    newpath = Path.Combine(ProjectName, @"Settings", "Pollutant.txt");
                }

                string selpollutant;
                try
                {
                    //get pollutant information
                    using (StreamReader myreader = new StreamReader(newpath))
                    {
                        selpollutant = myreader.ReadLine();

                        try
                        {
                            if (myreader.EndOfStream == false)
                            {
                                string[] a = myreader.ReadLine().Split('\t');
                                double cW = Convert.ToDouble(a[0], ic);
                                a = myreader.ReadLine().Split('\t');
                                double alphaW = Convert.ToDouble(a[0], ic);
                                numericUpDown36.Value = (decimal)(cW * 1E6D);
                                numericUpDown35.Value = (decimal)alphaW;
                                if (cW > 0 && alphaW > 0)
                                {
                                    checkBox33.Checked = true;
                                }
                                else
                                {
                                    checkBox33.Checked = false;
                                }

                                a = myreader.ReadLine().Split('\t');

                                string temp = myreader.ReadLine();
                                if (!string.IsNullOrEmpty(temp))
                                {
                                    string[] txt = temp.Split(new char[] { '\t', '!' }); // split groups
                                    for (int i = 0; i < txt.Length; i++)
                                    {
                                        string[] _values = txt[i].Split(new char[] { ':' }); // split source group number and decay rate
                                        if (_values.Length > 1)
                                        {
                                            int sg_number = 0;
                                            if (int.TryParse(_values[0], out sg_number))
                                            {
                                                double decay = 0;
                                                if (sg_number > 0 && sg_number < 100)
                                                {
                                                    try
                                                    {
                                                        decay = Convert.ToDouble(_values[1], ic);
                                                    }
                                                    catch { }
                                                    if (decay > 0)
                                                    {
                                                        GralData.DecayRates dr = new GralData.DecayRates();
                                                        dr.SourceGroup = sg_number;
                                                        dr.DecayRate = decay;
                                                        DecayRate.Add(dr);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                DecayRate.Sort();
                            }
                        }
                        catch { }
                    }
                    int j = 0;
                    foreach (string text in listBox5.Items)
                    {
                        if (text == selpollutant)
                        {
                            listBox5.SelectedIndex = j;
                        }
                        j += 1;
                    }
                }
                catch
                { }

                //check whether GRAL emission files exist
                bool fileexist = false;
                string pathy = Path.Combine(ProjectName, @"Computation", "point.dat");
                if (File.Exists(pathy))
                {
                    fileexist = true;
                }

                pathy = Path.Combine(ProjectName, @"Computation", "portals.dat");
                if (File.Exists(pathy))
                {
                    fileexist = true;
                }

                pathy = Path.Combine(ProjectName, @"Computation", "cadastre.dat");
                if (File.Exists(pathy))
                {
                    fileexist = true;
                }

                pathy = Path.Combine(ProjectName, @"Computation", "line.dat");
                if (File.Exists(pathy))
                {
                    fileexist = true;
                }

                if (fileexist == true)
                {
                    ChangeButtonLabel(Gral.ButtonColorEnum.ButtonEmission, ButtonColorEnum.BlackHook);
                }

                message.listBox1.Items.Add("Loading meteorological data...");
                message.Refresh();

                //load meteorological data
                try
                {
                    string newPath2 = Path.Combine(ProjectName, @"Settings", "Meteorology.txt");
                    if (File.Exists(newPath2))
                    {
                        using (StreamReader myreader = new StreamReader(newPath2))
                        {
                            try
                            {
                                MetfileName = myreader.ReadLine();
                                MetoColumnSeperator = Convert.ToChar(myreader.ReadLine());
                                MeteoDecSeperator = myreader.ReadLine();
                                Anemometerheight = Convert.ToDouble(myreader.ReadLine().Replace(".", DecimalSep));
                                numericUpDown7.Value = Convert.ToDecimal(Anemometerheight);
                                numericUpDown1.Value = Convert.ToDecimal(myreader.ReadLine().Replace(".", DecimalSep));
                                numericUpDown2.Value = Convert.ToInt16(myreader.ReadLine());
                                Textbox16_Set(MetfileName); // write metfile to tab "Computation"
                                WindSpeedClasses = Convert.ToInt32(numericUpDown2.Value);
                                for (int i = 0; i < WindSpeedClasses - 1; i++)
                                {
                                    NumUpDown[i].Value = Convert.ToDecimal(myreader.ReadLine(), ic);
                                    //add values in text boxes
                                    if (i > 0)
                                    {
                                        TBox[i].Text = NumUpDown[i - 1].Value.ToString();
                                    }
                                }
                                TBox[0].Text = "0";
                                TBox[WindSpeedClasses - 1].Text = NumUpDown[WindSpeedClasses - 2].Value.ToString(); ;
                            }
                            catch
                            { }
                        }
                    }
                    //				else
                    //					MessageBox.Show(this, "Could not read Meteorology.txt");

                    //reading the file and storing the data in variables
                    if (MetfileName != string.Empty)
                    {
                        if (MetfileName == null)
                        {
                            MetfileName = string.Empty;
                        }

                        if (!File.Exists(MetfileName))
                        {
                            newPath2 = Path.Combine(ProjectName, @"Metfiles", Path.GetFileName(MetfileName));
                        }
                        else
                        {
                            newPath2 = MetfileName;
                        }
                    }
                    else
                    {
                        newPath2 = "";
                    }

                    if (File.Exists(newPath2))
                    {
                        bool reader_ok = true;
                        List<GralData.WindData> winddata = new List<GralData.WindData>();
                        IO_ReadFiles readwindfile = new IO_ReadFiles
                        {
                            WindDataFile = newPath2,
                            WindData = winddata
                        };

                        if (MetoColumnSeperator == '\0') // check if seperators are set
                        {
                            MetoColumnSeperator = ',';
                        }

                        if (MeteoDecSeperator == String.Empty)
                        {
                            MeteoDecSeperator = ".";
                        }

                        if (readwindfile.ReadMeteoFiles(1000000, MetoColumnSeperator, DecimalSep, MeteoDecSeperator, Main.GUISettings.IgnoreMeteo00Values) == false)
                        {
                            MessageBox.Show(this, "Error when reading Meteo-File" + newPath2 + " in line" + winddata.Count, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        winddata = readwindfile.WindData;

                        if (winddata.Count == 0)
                        {
                            reader_ok = false;
                        }

                        readwindfile = null;

                        if (reader_ok)
                        {
                            string[] date = new string[3];

                            MeteoTimeSeries.Clear();
                            MeteoTimeSeries.TrimExcess();
                            MeteoTimeSeries = winddata;
                            //						foreach(Wind_Data wd in winddata)
                            //						{
                            //							datum[n1] = wd.Date;
                            //							zeit[n1] = wd.Time;
                            //							stunde[n1] = wd.Hour;
                            //							windge[n1] = wd.Vel;
                            //							windri[n1] = wd.Dir;
                            //							akla[n1] = wd.Stab_Class;
                            //							n1++;
                            //						}

                            groupBox1.Visible = true;
                            groupBox2.Visible = true;
                            checkBox19.Visible = true;
                            OpenProject.ReadMeteopgtAllFile();
                            checkBox19.Checked = OpenProject.MeteoClassification; //  classification
                            groupBox3.Visible = true;
                            button6.Visible = true;
                            button7.Visible = true;
                            ChangeButtonLabel(ButtonColorEnum.ButtonMeteo, ButtonColorEnum.RedDot); // meteo button red

                            groupBox23.Visible = true; // Anemometer height
                            label100.Visible = true;
                            groupBox20.Visible = true;

                            if (Path.GetDirectoryName(newPath2).Length > 85)
                            {
                                textBoxMeteoFile.Text = newPath2.Substring(0, 85) + "...." + Path.DirectorySeparatorChar +
                                    Path.GetFileName(newPath2);
                            }
                            else
                            {
                                textBoxMeteoFile.Text = newPath2;
                            }
                        }
                        winddata = null;
                    }

                    newPath2 = Path.Combine(ProjectName, @"Computation", "meteopgt.all");
                    if (File.Exists(newPath2))
                    {
                        ChangeButtonLabel(ButtonColorEnum.ButtonMeteo, Gral.ButtonColorEnum.BlackHook); // meteo button green
                    }
                }
                catch
                {
                    MessageBox.Show(this, "Could not read meteorology file");
                }

                //loading building information
                message.listBox1.Items.Add("Loading building information...");
                message.Refresh();
                try
                {
                    if (GRALSettings.BuildingMode == BuildingModeEnum.None ||
                        (GralStaticFunctions.St_F.CountLinesInFile(Path.Combine(ProjectName, @"Computation", "Buildings.txt")) < 3) &&
                        (GralStaticFunctions.St_F.CountLinesInFile(Path.Combine(ProjectName, @"Emissions", "Walls.txt")) < 3) &&
                        (GralStaticFunctions.St_F.CountLinesInFile(Path.Combine(ProjectName, @"Computation", "Vegetation.txt")) < 3)) // no buildings, walls or vegetation
                    {
                        ChangeButtonLabel(ButtonColorEnum.ButtonBuildings, ButtonColorEnum.Invisible); // no buildings
                    }
                    else
                    {
                        ChangeButtonLabel(ButtonColorEnum.ButtonBuildings, ButtonColorEnum.GreenDot); // Building green dot
                        button9.Visible = true;

                        string newPath2 = Path.Combine(ProjectName, "Computation", "buildings.dat");
                        if (File.Exists(newPath2))
                        {
                            ChangeButtonLabel(ButtonColorEnum.ButtonBuildings, ButtonColorEnum.BlackHook); // Building label OK
                        }
                        else
                        {
                            ChangeButtonLabel(ButtonColorEnum.ButtonBuildings, ButtonColorEnum.RedDot); // building label red
                        }
                    }
                }
                catch
                {
                    MessageBox.Show(this, "Could not read buildings.dat file");
                }
                if (GRALSettings.BuildingMode == BuildingModeEnum.Prognostic)
                {
                    numericUpDown42.Enabled = !Project_Locked;
                }
                else
                {
                    numericUpDown42.Enabled = false;
                }

                //loading topography information
                message.listBox1.Items.Add("Loading topography information...");
                message.Refresh();
                try
                {
                    //read file information
                    using (StreamReader myreader = new StreamReader(Path.Combine(ProjectName, @"Settings", "Topography.txt")))
                    {
                        Topofile = myreader.ReadLine();
                        numericUpDown19.Value = Convert.ToDecimal(myreader.ReadLine().Replace(".", DecimalSep));
                        try
                        {
                            numericUpDown17.Value = Convert.ToDecimal(myreader.ReadLine().Replace(".", DecimalSep));
                        }
                        catch
                        { }
                    }

                    //show file name in listbox
                    textBoxGrammTerrain.Text = Topofile;
                    if (Topofile != "None")
                    {
                        button22.Visible = true;
                        groupBox12.Visible = true;
                        if (GRALSettings.BuildingMode != BuildingModeEnum.GRAMM)
                        {
                            textBoxGrammLandUseFile.Visible = true;
                            radioButton1.Checked = false;
                            radioButton2.Checked = true;
                            checkBox25.Visible = true;
                            button20.Visible = true;
                            checkBox36.Visible = true;
                            button43.Visible = true;
                            button23.Visible = true;
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
                        //compute top height of GRAMM domain
                        ComputeGrammModelHeights();

                    }
                    else
                    {
                        textBoxGrammLandUseFile.Visible = false;
                        button20.Visible = false;
                        checkBox36.Visible = false;
                        button43.Visible = false;
                        //button22.Visible = false;
                        button23.Visible = false;
                        radioButton1.Checked = true;
                        radioButton2.Checked = false;
                        checkBox25.Visible = false;
                        checkBox25.Checked = false;
                        groupBox12.Visible = false;
                    }
                }
                catch
                {
                }

                //loading landuse information
                message.listBox1.Items.Add("Loading landuse information...");
                message.Refresh();
                try
                {
                    //read file information
                    using (StreamReader myreader = new StreamReader(Path.Combine(ProjectName, @"Settings", "Landuse.txt")))
                    {
                        if (!myreader.EndOfStream)
                        {
                            Landusefile = myreader.ReadLine();
                        }
                    }
                    //show file name in textbox
                    textBoxGrammLandUseFile.Text = Landusefile;
                }
                catch
                {
                }

                //loading GRAMM windfield information
                message.listBox1.Items.Add("Loading GRAMM windfield information...");
                message.Refresh();

                // A windfield has been defined previously....
                if (File.Exists(Path.Combine(ProjectName, "Computation", "windfeld.txt")))
                {
                    bool GRAMMWindFieldsAvailable = true;

                    //get path of wind field files from windfeld.txt
                    GRAMMwindfield = ReadFileGRAMMWindfeld_txt(ProjectName);

                    //check if directory still exists
                    if (Directory.Exists(GRAMMwindfield) == false)
                    {
                        GRAMMWindFieldsAvailable = false;
                    }

                    //check if local Computation folder contains *.wnd files -> ask user if this folder should be used
                    //search for flow files
                    if (!Path.GetFullPath(Path.Combine(ProjectName, @"Computation") + Path.DirectorySeparatorChar).Equals(Path.GetFullPath(GRAMMwindfield)))
                    {
                        FileInfo[] files_flow = null;
                        try
                        {
                            DirectoryInfo di = new DirectoryInfo(Path.Combine(ProjectName, @"Computation"));
                            files_flow = di.GetFiles("*.wnd");
                        }
                        catch { }
                        if (files_flow.Length > 0)
                        {
                            //ask, if Computation folder should be used
                            if (MessageBox.Show(this, "Are you supposed to use the GRAMM wind fields in the project Computation folder?", "GRAL GUI",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                            {
                                GRAMMwindfield = Path.Combine(ProjectName, @"Computation") + Path.DirectorySeparatorChar;
                                WriteFileGRAMMWindfeld_txt(ProjectName, GRAMMwindfield, false);
                                GRAMMWindFieldsAvailable = true;
                            }
                        }
                    }

                    Textbox16_Set("GRAMM Windfield: " + GRAMMwindfield); // write metfile to tab "Computation"

                    if (GRAMMWindFieldsAvailable == true)
                    {
                        //search for flow files
                        FileInfo[] files_flow = null;
                        try
                        {
                            DirectoryInfo di = new DirectoryInfo(GRAMMwindfield);
                            files_flow = di.GetFiles("*.wnd");
                        }
                        catch { }
                        if (files_flow.Length == 0)
                        {
                            GRAMMWindFieldsAvailable = false;
                            GRAMM_Locked = false; 					// unlock GRAMM project
                        }
                        else
                        {
                            GRAMM_Locked = true; 					// lock GRAMM project
                        }
                    }
                    //Directory not valid or no wind field files available
                    if (GRAMMWindFieldsAvailable == false)
                    {
                        try
                        {
                            bool repeat = true;
                            while (repeat)
                            {
                                repeat = false;
                                using (FolderBrowserDialog dialogGRAMM = new FolderBrowserDialog
                                {
                                    Description = "GRAMM wind field files not found - please enter path",
                                    SelectedPath = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar)
                                })
                                {
#if NET6_0_OR_GREATER
                                    dialogGRAMM.UseDescriptionForTitle = true;
#endif
                                    if (dialogGRAMM.ShowDialog() == DialogResult.OK)
                                    {
                                        GRAMMwindfield = dialogGRAMM.SelectedPath + Path.DirectorySeparatorChar;
                                        if (File.Exists(Path.Combine(GRAMMwindfield, "GRAMM.geb")) == false) // not the computation folder?
                                        {
                                            GRAMMwindfield = Path.Combine(GRAMMwindfield, "Computation") + Path.DirectorySeparatorChar; // try path "Computation"
                                        }

                                        if (File.Exists(Path.Combine(GRAMMwindfield, "GRAMM.geb")))
                                        {
                                            // check if Gramm.geb in original folder and in the new folder match
                                            if (File.Exists(Path.Combine(ProjectName, "Computation", "GRAMM.geb")))
                                            {
                                                //Read GRAMMGeb from project path
                                                if (OpenProject.ReadGrammGebFile() == true)
                                                {
                                                    Decimal vL = OpenProject.GRAMMvertlayers;
                                                    double dW = OpenProject.West;
                                                    double dE = OpenProject.East;
                                                    double dS = OpenProject.South;
                                                    double dN = OpenProject.North;

                                                    //Read GRAMMGeb from new selected path
                                                    string pathUp = Path.GetDirectoryName(Path.GetDirectoryName(GRAMMwindfield));
                                                    if (string.IsNullOrEmpty(pathUp))
                                                    {
                                                        pathUp = GRAMMwindfield;
                                                    }
                                                    IO_ReadFiles OpenNewGrammIn = new IO_ReadFiles
                                                    {
                                                        ProjectName = pathUp
                                                    };
                                                    if (OpenNewGrammIn.ReadGrammGebFile() == true)
                                                    {
                                                        if (vL == OpenNewGrammIn.GRAMMvertlayers && Math.Abs(dW - OpenNewGrammIn.West) < 0.1 && Math.Abs(dE - OpenNewGrammIn.East) < 0.1 &&
                                                            Math.Abs(dS - OpenNewGrammIn.South) < 0.1 && Math.Abs(dN - OpenNewGrammIn.North) < 0.1)
                                                        {
                                                            GRAMMWindFieldsAvailable = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (!GRAMMWindFieldsAvailable)
                                        {
                                            if (MessageBox.Show(this, "Selected GRAMM folder does not match the current project! \nTry another folder (OK) or set a new reference to the wind field in the Topography tab (Cancel)",
                                                                    "GRAL GUI", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                                            {
                                                repeat = true;
                                            }
                                        }
                                    }
                                }
                            }

                            // found a valid and matching GRAMM wind field 
                            if (GRAMMWindFieldsAvailable)
                            {
                                //write file information for GRAMM windfield
                                WriteFileGRAMMWindfeld_txt(ProjectName, GRAMMwindfield, false);

                                Textbox16_Set("GRAMM Windfield: " + GRAMMwindfield); // write metfile to tab "Computation"

                                //write file information for landuse file
                                using (StreamWriter GRAMMwrite = new StreamWriter(Path.Combine(ProjectName, @"Settings", "Landuse.txt")))
                                {
                                    GRAMMwrite.WriteLine(Path.Combine(GRAMMwindfield, @"landuse.asc"));
#if __MonoCS__
                                    GRAMMwrite.WriteLine(Path.Combine(GRAMMwindfield,@"landuse.asc"));
#endif
                                }
                                //show file name in listbox
                                Landusefile = Path.Combine(GRAMMwindfield, @"landuse.asc");
                                textBoxGrammLandUseFile.Text = Landusefile;
                                //write file information for topography file only if ggeom.asc contains a path and no grid info
                                using (StreamReader ggeomread = new StreamReader(Path.Combine(ProjectName, "Computation", "ggeom.asc")))
                                {
                                    try
                                    {
                                        //try to read the file ggeom.asc to find out if there is only pathinformation in it
                                        string[] dumread = new string[1];
                                        dumread = ggeomread.ReadLine().Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                        int NX = Convert.ToInt32(dumread[0]);
                                    }
                                    catch
                                    {
                                        //new path info is stored in the file ggeom.asc
                                        using (StreamWriter GRAMMwrite = new StreamWriter(Path.Combine(ProjectName, "Computation", "ggeom.asc")))
                                        {
                                            GRAMMwrite.WriteLine(Path.Combine(GRAMMwindfield, @"ggeom.asc"));
#if __MonoCS__
                                            GRAMMwrite.WriteLine(Path.Combine(GRAMMwindfield, @"ggeom.asc"));
#endif
                                        }
                                    }
                                }

                                //show file name in listbox
                                Topofile = Path.Combine(GRAMMwindfield, @"ggeom.asc");
                                textBoxGrammTerrain.Text = Topofile;
                                //read and overwrite existing topography file in Settings
                                string[] text = new string[3];
                                using (StreamReader sr = new StreamReader(Path.Combine(ProjectName, @"Settings", "Topography.txt")))
                                {
                                    text[0] = sr.ReadLine();
                                    text[1] = sr.ReadLine();
                                    text[2] = sr.ReadLine();
                                }
                                //write file information for topography file
                                using (StreamWriter GRAMMwrite = new StreamWriter(Path.Combine(ProjectName, @"Settings", "Topography.txt")))
                                {
                                    GRAMMwrite.WriteLine(Path.Combine(GRAMMwindfield, @"ggeom.asc"));
                                    GRAMMwrite.WriteLine(text[1]);
                                    GRAMMwrite.WriteLine(text[2]);
                                }

                                //search for flow files
                                FileInfo[] files_flow = null;
                                try
                                {
                                    DirectoryInfo di = new DirectoryInfo(GRAMMwindfield);
                                    files_flow = di.GetFiles("*.wnd");
                                }
                                catch { }
                                if (files_flow.Length == 0)
                                {
                                    GRAMMWindFieldsAvailable = false;
                                    GRAMM_Locked = false;                   // unlock GRAMM project
                                }
                                else
                                {
                                    GRAMM_Locked = true;                    // lock GRAMM project
                                }
                            }

                        }
                        catch
                        { }
                    }
                }

                // Show Logfile Button?
                Check_for_Existing_Logfiles();

                //search for GRAL concentration files
                message.listBox1.Items.Add("Search for GRAL concentration files...");
                message.Refresh();

                CheckConFiles();

                Gramm_locked_buttonClick(null, null);   // change locked-Button

                if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift) // if shift key pressed, don't close the message window
                {
                    message.Close();
                }

                EmifileReset = true;

                OpenProject = null; // relase object

                //enable/disable GRAL simulations
                Enable_GRAL();
                //enable/disable GRAMM simulations
                Enable_GRAMM();

                Cursor = Cursors.Default;

                // set new MostRecentFiles file
                global::GralMainForms.MostRecentFiles MRF = new global::GralMainForms.MostRecentFiles
                {
                    NewFile = foldername
                };
                MRF.Write();
                MRF.Close();
            }
            //catch(Exception ex)
            {
                //MessageBox.Show(this, ex.Message.ToString());
            }
        }

        /// <summary>
        /// Write the file windfile.txt -> this file contains the absolute path to the GRAMM wind *.wnd data
        /// </summary>
        /// <param name="ProjectDir">Project Directory</param>
        /// <param name="GRAMMwindfield">path to the GRAMM wind fields</param>
        /// <returns></returns>
        public bool WriteFileGRAMMWindfeld_txt(string ProjectDir, string GRAMMwindfield, bool message)
        {
            try
            {
                using (StreamWriter GRAMMwrite = new StreamWriter(Path.Combine(ProjectDir, @"Computation", "windfeld.txt")))
                {
                    GRAMMwrite.WriteLine(GRAMMwindfield);
#if __MonoCS__
                                GRAMMwrite.WriteLine(GRAMMwindfield);
#endif
                }
            }
            catch (Exception ex)
            {
                if (message)
                {
                    MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read the file windfile.txt -> this file contains the absolute path to the GRAMM wind *.wnd data
        /// </summary>
        /// <param name="ProjectDir"></param>
        /// <returns>the path to the wind fields</returns>
        public string ReadFileGRAMMWindfeld_txt(string ProjectDir)
        {
            string GRAMMWindfieldPath = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(Path.Combine(ProjectDir, @"Computation", "windfeld.txt")))
                {
                    if (!sr.EndOfStream)
                    {
                        GRAMMWindfieldPath = sr.ReadLine();
                    }
                    //Catch empty string
                    if (string.IsNullOrEmpty(GRAMMWindfieldPath))
                    {
                        GRAMMWindfieldPath = string.Empty;
                    }
                }
                //repair files without Path.DirectorySeparatorChar as final character
                if (GRAMMWindfieldPath.Length > 1 && GRAMMWindfieldPath[GRAMMWindfieldPath.Length - 1] != Path.DirectorySeparatorChar)
                {
                    GRAMMWindfieldPath += Path.DirectorySeparatorChar;
                    WriteFileGRAMMWindfeld_txt(ProjectName, GRAMMWindfieldPath, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return GRAMMWindfieldPath;
        }

        private void SetBuildingRadioButton()
        {
            if (GRALSettings.BuildingMode == BuildingModeEnum.None)
            {
                radioButton3.Checked = true;
            }
            else if (GRALSettings.BuildingMode == BuildingModeEnum.Diagnostic)
            {
                radioButton4.Checked = true;
            }
            else if (GRALSettings.BuildingMode == BuildingModeEnum.Prognostic)
            {
                radioButton5.Checked = true;
            }
        }

    }
}