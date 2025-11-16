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
using System.Diagnostics;
using System.Threading;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
    {
        /// <summary>
        /// start GRAMM simulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMStartCalculation(object sender, EventArgs e)
        {
            //set the maximum of the progressbar for the actual dispersion situation (simulation time)
            progressBar2.Maximum = Convert.ToInt32(numericUpDown21.Value);

            //delete Problemreport.txt if existent and complete new computation start
            if (File.Exists(Path.Combine(ProjectName, @"Computation", "Problemreport.txt")) && numericUpDown24.Value == 1)
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "Problemreport.txt"));
            }

            //get information about the maximum number of dispersion situations for the progress bar
            IO_ReadFiles ReadFile = new IO_ReadFiles
            {
                ProjectName = ProjectName,
                DispsituationFrequ = DispSituationfrequ
            };

            if (ReadFile.ReadMeteopgtAllFile() == true)
            {
                if (ReadFile.MeteoFrequ > 0)
                {
                    progressBar1.Maximum = Convert.ToInt32(ReadFile.MeteoFrequ);
                }

                DispSituationfrequ = ReadFile.DispsituationFrequ;
            }
            ReadFile = null;

            //search for GRAMM executables
            string Project_Computation_Path = Path.Combine(ProjectName, "Computation");
            string[] filePaths = Directory.GetFiles(Project_Computation_Path, "GRAMM*.exe", SearchOption.TopDirectoryOnly);
            string dir;
            if (filePaths.Length > 0)
            {
                dir = Path.GetDirectoryName(filePaths[0]);
            }
            else
            {
                #if __MonoCS__
                filePaths = Directory.GetFiles(Project_Computation_Path, "GRAMM*.dll", SearchOption.TopDirectoryOnly);
                #else
                filePaths = Directory.GetFiles(Project_Computation_Path, "GRAMM*.bat", SearchOption.TopDirectoryOnly);
                #endif
                if (filePaths.Length > 0)
                {
                    dir = Path.GetDirectoryName(filePaths[0]);
                }
                else
                {
                    dir = Path.GetDirectoryName(Application.ExecutablePath);
                }
            }

            OpenFileDialog dialog = new OpenFileDialog();
            
            #if __MonoCS__
            dialog.Filter = "GRAMM executable (GRAMM*.dll;GRAMM)|GRAMM*.dll;GRAMM";
            #else
            dialog.Filter = "GRAMM executable (GRAMM*.exe;GRAMM*.bat)|GRAMM*.exe;GRAMM*.bat";
            #endif
            dialog.Title = "Select GRAMM executable";
            // dialog.ShowHelp = true;

            if (GUISettings.CopyCoresToProject)
            {
                dialog.InitialDirectory = dir;
            }
            else
            {
                dialog.InitialDirectory = GUISettings.DefaultPathForGRAMM;
            }
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogExe;
#endif

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string GRAMM_Program_Path = dialog.FileName;
                    string GRAMM_Project_Path = String.Empty;

                    if (GUISettings.CopyCoresToProject == true)
                    {
                        //copy the file GRAMM*.exe in the directory "Computation"
                        GRAMM_Program_Path = Path.Combine(ProjectName, @"Computation", Path.GetFileName(dialog.FileName));

                        try
                        {
                            File.Copy(dialog.FileName, GRAMM_Program_Path, true);

                            // copy additional files for dotnet version
                            string batch = String.Empty;
                            #if __MonoCS__
                            if (Path.GetExtension(dialog.FileName).ToLower() == ".dll")
                                batch = "GRAMM*.dll";
                            #else
                            if (Path.GetExtension(dialog.FileName).ToLower() == ".bat")
                            {
                                batch = "GRAMM*.bat";
                            }
                            #endif

                            if (batch != String.Empty)
                            {
                                filePaths = Directory.GetFiles(Path.GetDirectoryName(dialog.FileName),
                                                               Path.GetFileNameWithoutExtension(dialog.FileName) + ".*",
                                                               SearchOption.TopDirectoryOnly);
                                foreach (string file in filePaths)
                                {
                                    string newpathl = Path.Combine(ProjectName, @"Computation", Path.GetFileName(file));
                                    File.Copy(file, newpathl, true);
                                }
                            }
                        }
                        catch { }
                    }
                    else // do not copy the core to the project folder
                    {
                        GRAMM_Project_Path = Path.Combine(ProjectName, @"Computation");
                        GUISettings.DefaultPathForGRAMM = Path.GetDirectoryName(dialog.FileName);
                        GUISettings.WriteToFile();
                    }

                    // write Log File
                    try
                    {
                        string filename = Path.Combine(ProjectName, "Computation", "meteopgt.all");
                        long metsitcount = GralStaticFunctions.St_F.CountLinesInFile(filename) - 2;
                        Write_Gramm_Log(1, Convert.ToString(metsitcount), GRAMM_Program_Path); // write GRAMM LOG-File
                    }
                    catch { }

                    GRAMM_Locked = true;                    // lock project
                    Gramm_locked_buttonClick(null, null);   // change locked-Button

                    if (numericUpDown33.Value == 1) // one instance -> start GRAMM as usual
                    {
                        //start computation routine GRAMM*.exe to compute wind fields
                        #if __MonoCS__
                        try
                        {
                            string command = String.Empty;
                            if (Path.GetFileName(GRAMM_Program_Path).Equals("GRAMM")) // new GRAMM version from .NET5
                            {
                                command = "gnome-terminal -x bash -ic 'cd '" + Path.GetDirectoryName(GRAMM_Program_Path) + "'; ./" + Path.GetFileName(GRAMM_Program_Path) + " " + '"' + GRAMM_Project_Path + '"' + " ; bash'";
                                GRAMMProcess = new Process();
                                GRAMMProcess.StartInfo.FileName = "/bin/bash";
                                GRAMMProcess.StartInfo.Arguments = "-c \" " + command + " \"";
                                GRAMMProcess.StartInfo.UseShellExecute = false;
                                GRAMMProcess.StartInfo.RedirectStandardOutput = true;
                                GRAMMProcess.Start();
                                Thread.Sleep(500);
                            }
                            else
                            {
                                if (Path.GetExtension(GRAMM_Program_Path).ToLower() == ".dll") // .sh file -> call sh file
                                {
                                    command = "gnome-terminal -x bash -ic 'cd '" + Path.GetDirectoryName(GRAMM_Program_Path) + "'; dotnet " + Path.GetFileName(GRAMM_Program_Path) + " " + '"' + GRAMM_Project_Path + '"' + " ; bash'";
                                }
                                else // .exe file
                                {
                                    command = "gnome-terminal -x bash -ic 'cd '" + Path.GetDirectoryName(GRAMM_Program_Path) + "'; mono " + Path.GetFileName(GRAMM_Program_Path) + " ; bash'";
                                }
                                //start routine GRAMM*.exe to compute wind fields
                                GRAMMProcess = new Process();
                                GRAMMProcess.StartInfo.FileName = "/bin/bash";
                                GRAMMProcess.StartInfo.Arguments = "-c \" " + command + " \"";
                                GRAMMProcess.StartInfo.UseShellExecute = false;
                                GRAMMProcess.StartInfo.RedirectStandardOutput = true;
                                GRAMMProcess.Start();
                                Thread.Sleep(500);
                            }
                        }

                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        #else

                        GRAMMProcess = new Process
                        {
                            EnableRaisingEvents = true
                        };
                        GRAMMProcess.Exited += new System.EventHandler(GrammExited);
                        GRAMMProcess.StartInfo.FileName = GRAMM_Program_Path;
                        GRAMMProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                        GRAMMProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(GRAMM_Program_Path);
                        GRAMMProcess.StartInfo.Arguments = "\"" + GRAMM_Project_Path + "\"";
                        GRAMMProcess.Start();
                        
                        #endif
                    }
                    else // multi instances -> start multiple instances of GRAMM
                    {
                        int first_sit = Convert.ToInt32(numericUpDown24.Value); // start situation from Tab "Topography"
                        int final_sit = 0;
                        // get final situation
                        IO_ReadFiles OpenProject = new IO_ReadFiles
                        {
                            ProjectName = ProjectName
                        };
                        if (OpenProject.ReadGrammInFile() == true) // get final situation from GRAMMin.dat if sunrise is activated
                        {
                            final_sit = OpenProject.GRAMMsunrise;
                        }

                        if (final_sit == 0) // try to get final situation from lenght of meteopgt.all
                        {
                            string filename = Path.Combine(ProjectName, "Computation", "meteopgt.all");
                            final_sit = (int)GralStaticFunctions.St_F.CountLinesInFile(filename) - 2;
                        }
                        OpenProject = null;

                        if (final_sit < first_sit)
                        {
                            throw new ArgumentOutOfRangeException("Final situation < Start situation");
                        }

                        // now loop through the GRAMM instances
                        int offset = Convert.ToInt32(Math.Max(1, (final_sit - first_sit) / (double)numericUpDown33.Value));
                        int instance_start = first_sit;
                        int instance_end;
                        int count = 1;
                        while (instance_start <= final_sit)
                        {
                            instance_end = instance_start + offset;
                            if (count == (int)numericUpDown33.Value) // avoid rounding errors
                            {
                                instance_end = final_sit;
                            }

                            if (count == 1) // first instance
                            {
                                //start computation routine GRAMM*.exe to compute wind fields
                                #if __MonoCS__
                                try
                                {
                                    if (Path.GetFileName(GRAMM_Program_Path).Equals("GRAMM")) // new GRAMM version from .NET5
                                    {
                                        string gramm_start = " " + instance_start.ToString() + " " + instance_end.ToString();
                                        string command = "gnome-terminal -x bash -ic 'cd '" + Path.GetDirectoryName(GRAMM_Program_Path) + "'; ./" + Path.GetFileName(GRAMM_Program_Path) + " " + '"' + GRAMM_Project_Path + '"' + gramm_start + " ; bash'";
                                        GRAMMProcess = new Process();
                                        GRAMMProcess.StartInfo.FileName = "/bin/bash";
                                        GRAMMProcess.StartInfo.Arguments = "-c \" " + command + " \"";
                                        GRAMMProcess.StartInfo.UseShellExecute = false;
                                        GRAMMProcess.StartInfo.RedirectStandardOutput = true;
                                        GRAMMProcess.Start();
                                    }
                                    else
                                    {

                                        string gramm_start = " " + instance_start.ToString() + " " + instance_end.ToString();
                                        string command = "gnome-terminal -x bash -ic 'cd '" + Path.GetDirectoryName(GRAMM_Program_Path) + "'; mono " + Path.GetFileName(GRAMM_Program_Path) + " " + gramm_start + " ; bash'";
                                        GRAMMProcess = new Process();
                                        GRAMMProcess.StartInfo.FileName = "/bin/bash";
                                        GRAMMProcess.StartInfo.Arguments = "-c \" " + command + " \"";
                                        GRAMMProcess.StartInfo.UseShellExecute = false;
                                        GRAMMProcess.StartInfo.RedirectStandardOutput = true;
                                        GRAMMProcess.Start();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                #else
                                GRAMMProcess = new Process
                                {
                                    EnableRaisingEvents = true
                                };
                                GRAMMProcess.Exited += new System.EventHandler(GrammExited);
                                GRAMMProcess.StartInfo.FileName = GRAMM_Program_Path;
                                GRAMMProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                                GRAMMProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(GRAMM_Program_Path);
                                if (string.IsNullOrEmpty(GRAMM_Project_Path))
                                {
                                    GRAMMProcess.StartInfo.Arguments = " " + instance_start.ToString() + " " + instance_end.ToString();
                                }
                                else
                                {
                                    GRAMMProcess.StartInfo.Arguments = " " + "\"" + GRAMM_Project_Path + "\"" + " " + instance_start.ToString() + " " + instance_end.ToString();
                                }

                                GRAMMProcess.Start();
                                #endif
                                Thread.Sleep(5000);
                            }
                            else // further instances
                            {
                                //start computation routine GRAMM*.exe to compute
#if __MonoCS__
                                try
                                {
                                    Process gramm_local;
                                    string gramm_start = " " + instance_start.ToString() + " " + instance_end.ToString();
                                    if (Path.GetFileName(GRAMM_Program_Path).Equals("GRAMM")) // new GRAMM version from .NET5
                                    {
                                        string command = "gnome-terminal -x bash -ic 'cd '" + Path.GetDirectoryName(GRAMM_Program_Path) + "'; ./" + Path.GetFileName(GRAMM_Program_Path) + " " + '"' + GRAMM_Project_Path + '"' + gramm_start + " ; bash'";
                                        GRAMMProcess = new Process();
                                        GRAMMProcess.StartInfo.FileName = "/bin/bash";
                                        GRAMMProcess.StartInfo.Arguments = "-c \" " + command + " \"";
                                        GRAMMProcess.StartInfo.UseShellExecute = false;
                                        GRAMMProcess.StartInfo.RedirectStandardOutput = true;
                                        GRAMMProcess.Start();
                                    }
                                    else
                                    {
                                        string command = "gnome-terminal -x bash -ic 'cd '" + Path.GetDirectoryName(GRAMM_Program_Path) + "'; mono " + Path.GetFileName(GRAMM_Program_Path) + " " + gramm_start + " ; bash'";
                                        gramm_local = new Process();
                                        gramm_local.StartInfo.FileName = "/bin/bash";
                                        gramm_local.StartInfo.Arguments = "-c \" " + command + " \"";
                                        gramm_local.StartInfo.UseShellExecute = false;
                                        gramm_local.StartInfo.RedirectStandardOutput = true;
                                        gramm_local.Start();
                                    }
                                    Thread.Sleep(800);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
#else
                                Process gramm_local;
                                gramm_local = new Process();
                                gramm_local.StartInfo.FileName = GRAMM_Program_Path;
                                gramm_local.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                                gramm_local.StartInfo.WorkingDirectory = Path.GetDirectoryName(GRAMM_Program_Path);
                                if (GRAMM_Project_Path == String.Empty)
                                {
                                    gramm_local.StartInfo.Arguments = " " + instance_start.ToString() + " " + instance_end.ToString();
                                }
                                else
                                {
                                    gramm_local.StartInfo.Arguments = " " + "\"" + GRAMM_Project_Path + "\"" + " " + instance_start.ToString() + " " + instance_end.ToString();
                                }
                                gramm_local.Start();
                                #endif
                                Thread.Sleep(500);
                            }


                            // create batch-files
                            string filename_batch = Path.Combine(Project_Computation_Path, "GRAMM_instance_" + count.ToString() + ".bat");
                            try
                            {
                                string gramm_start = String.Empty;
                                if (GRAMM_Project_Path == String.Empty)
                                {
                                    gramm_start = " " + instance_start.ToString() + " " + instance_end.ToString();
                                }
                                else
                                {
                                    gramm_start = " " + "\"" + GRAMM_Project_Path + "\"" + " " + instance_start.ToString() + " " + instance_end.ToString();
                                }

                                using (StreamWriter batchfile = new StreamWriter(filename_batch))
                                {
                                    if (GUISettings.CopyCoresToProject == true)
                                    {
                                        batchfile.WriteLine(Path.GetFileName(GRAMM_Program_Path) + gramm_start);
                                    }
                                    else if (Path.GetExtension(GRAMM_Program_Path).EndsWith("exe"))
                                    {
                                        batchfile.WriteLine("\"" + GRAMM_Program_Path + "\"" + gramm_start);
                                    }
                                    else
                                    {
                                        batchfile.WriteLine("dotnet " + "\"" + GRAMM_Program_Path.Replace(".bat", ".dll") + "\"" + gramm_start);
                                    }
                                }
                            }
                            catch
                            { }

                            // create bash files at Linux
#if __MonoCS__
                            filename_batch = Path.Combine(Path.GetDirectoryName(GRAMM_Program_Path), "GRAMM_instance_" + count.ToString() + ".sh");
                            try
                            {
                                using (StreamWriter batchfile = new StreamWriter(filename_batch))
                                {
                                    batchfile.WriteLine("#!/bin/bash");
                                    string gramm_start = " " + instance_start.ToString() + " " + instance_end.ToString();
                                    if (Path.GetFileName(GRAMM_Program_Path).Equals("GRAMM")) // new GRAMM version from .NET5
                                    {
                                        batchfile.WriteLine(Path.GetFileName(GRAMM_Program_Path) + gramm_start);
                                    }
                                    else
                                    {
                                        batchfile.WriteLine("dotnet " + Path.GetFileName(GRAMM_Program_Path) + gramm_start);
                                    }
                                }
                            }
                            catch
                            { }
#endif
                            // new start value, new instance
                            instance_start = instance_end + 1;
                            count++;
                        }
                    }

                    //pointer for wind field files
                    WriteFileGRAMMWindfeld_txt(ProjectName, Path.Combine(ProjectName, "Computation") + Path.DirectorySeparatorChar, true);
                    Textbox16_Set("GRAMM Windfield: " + Path.Combine(ProjectName, "Computation") + Path.DirectorySeparatorChar); // write metfile to tab "Computation"
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        //GRAMM simulations finished or interrupted
        private void GrammExited(object sender, EventArgs e)
        {
            #if __MonoCS__
            #else
            GRAMMProcess.EnableRaisingEvents = false;
            GRAMMProcess.Dispose();
            System.Threading.Thread.Sleep(1000);
            //check if GRAL simulation should be enabled
            Invoke(new showTopo(Enable_GRAL));
            MessageBox.Show("GRAMM simulation stopped or interrupted", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            #endif
        }

        /// <summary>
        /// stop GRAMM simulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMStopCalculation(object sender, EventArgs e)
        {
            #if __MonoCS__
            MessageBox.Show("This function is not available at LINUX", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
            #else
            try
            {
                GRAMMProcess.Kill();
            }
            catch
            { }
            progressBar2.Value = 0;
            numericUpDown24.Value = 1;
            label67.Text = "Actual flow situation: " + "0" + "%";
            //save GRAMM control file "GRAMMin.dat"
            GRAMMin(EmifileReset);
            //check if GRAL simulation should be enabled
            Enable_GRAL();
            #endif
        }

        /// <summary>
        /// pause GRAMM simulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMPauseCalculation(object sender, EventArgs e)
        {
            #if __MonoCS__
            MessageBox.Show("This function is not available at LINUX", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
            #endif

            try
            {
                GRAMMProcess.Kill();
            }
            catch
            { }

            //refresh actual computed dispersion situation
            int trackbar = 1;
            try
            {
                using (StreamReader myreader = new StreamReader(Path.Combine(ProjectName, "Computation", "DispNrGramm.txt")))
                {
                    trackbar = Convert.ToInt32(myreader.ReadLine().Replace(" ", ""));
                }
            }
            catch
            { }
            numericUpDown24.Value = trackbar;
            progressBar2.Value = 0;
            label67.Text = "Actual flow situation: " + "0" + "%";
            //save GRAMM control file "GRAMMin.dat"
            GRAMMin(EmifileReset);
            //check if GRAL simulation should be enabled
            Enable_GRAL();
        }
    }
}