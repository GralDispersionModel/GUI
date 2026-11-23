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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        private async void GRAMMStartCalculation(object sender, EventArgs e)
        {
            bool continueCalculation = sender == button31;

            Random rnd = new Random();

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
                    ResetGrammProgress();
                    string GRAMM_Program_Path = dialog.FileName;
                    string GRAMM_Project_Path = String.Empty;

                    var engine = GrammEngine.Inspect(dialog.FileName);
                    // Keep a managed apphost with its DLL, runtime configuration and dependency tree.
                    bool copyCore = GUISettings.CopyCoresToProject && !engine.UsesAdjacentAssembly;
                    if (copyCore)
                    {
                        //copy the file GRAMM*.exe in the directory "Computation"
                        GRAMM_Program_Path = Path.Combine(ProjectName, @"Computation", Path.GetFileName(dialog.FileName));

                        if (!string.Equals(Path.GetFullPath(dialog.FileName), Path.GetFullPath(GRAMM_Program_Path), StringComparison.OrdinalIgnoreCase))
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
                        engine = GrammEngine.Inspect(GRAMM_Program_Path);
                    }
                    else // do not copy the core to the project folder
                    {
                        GRAMM_Project_Path = Path.Combine(ProjectName, @"Computation");
                        if (!GUISettings.CopyCoresToProject)
                        {
                            GUISettings.DefaultPathForGRAMM = Path.GetDirectoryName(dialog.FileName);
                            GUISettings.WriteToFile();
                        }
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

                    DirectoryInfo di = new DirectoryInfo(Project_Computation_Path);

                    int instances = (int)numericUpDown33.Value;
                    bool steadyCaseReset = engine.Generation == GrammEngine.Reset;
                    bool single = instances == 1;
#if !__MonoCS__
                    bool trackSingle = single && (engine.Generation != GrammEngine.Legacy || GrammProgressRun.HasCheckpoint(Project_Computation_Path));
                    if (trackSingle && continueCalculation && GrammInstanceRanges.IsSteadyBinaryProject(Project_Computation_Path))
                    {
                        single = false;
                    }
#endif
                    if (single)
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
                        GrammProgressRun nativeRun = null;
                        if (trackSingle)
                        {
                            nativeRun = new GrammProgressRun(Project_Computation_Path) { EngineGeneration = engine.Generation };
                            nativeRun.CaptureNativeBaseline();
                        }
                        int startSituation = (int)numericUpDown24.Value;
                        string param = $"\"{GRAMM_Program_Path}\"";
                        if (continueCalculation)
                        {
                            startSituation = GetNextMissingSituation(1, 10000, di.GetFiles("*.wnd"));
                            if (nativeRun != null) { startSituation = nativeRun.NativeResumeFrom(1, startSituation); }
                            if (GRAMM_Project_Path == String.Empty && startSituation > 0)
                            {
                                param += " " + startSituation.ToString() + " " + 9999999.ToString();
                            }
                            else
                            {
                                param += " " + "\"" + GRAMM_Project_Path + "\"" + " " + startSituation.ToString() + " " + 9999999.ToString();
                            }
                        }
                        else
                        {
                            // No explicit range: GRAMM uses its control file and enables receptor output.
                            param += " " + "\"" + GRAMM_Project_Path + "\"";
                        }
                        nativeRun?.SaveNativePending(startSituation);
                        int node = NumaNode(CPUNode);
                        int processID = NumaProcessStarter.StartProcessOnNumaNode(param, node);
                        if (processID > 0) //sucessful start
                        {
                            CoreProcessID.Add(processID);
                        }
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

                        int sunriseOriginalCount = final_sit;
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
                        var run = new GrammProgressRun(Project_Computation_Path)
                        {
                            Continuing = continueCalculation,
                            EngineGeneration = engine.Generation
                        };
                        grammProgress = run;
                        // Keep the wind-field delete dialog out of the pre-launch snapshot.
                        groupBox30.Enabled = false;
                        List<(int First, int Last)> assignments;
                        try
                        {
                            assignments = await Task.Run(() =>
                            {
                                run.CaptureBaseline(first_sit, final_sit);
                                return GrammInstanceRanges.SelectForEngine(steadyCaseReset, Project_Computation_Path,
                                    first_sit, final_sit, instances, sunriseOriginalCount);
                            });
                        }
                        finally
                        {
                            if (grammProgress == run && !IsDisposed) { groupBox30.Enabled = true; }
                        }
                        if (grammProgress != run || IsDisposed || Disposing) { return; }
#if !__MonoCS__
                        int[] starts = new int[assignments.Count];
                        FileInfo[] nativeWinds = continueCalculation && run.LegacyResume ? di.GetFiles("*.wnd") : null;
                        for (int i = 0; i < assignments.Count; i++)
                        {
                            var (first, last) = assignments[i];
                            starts[i] = !continueCalculation ? first : nativeWinds == null ? run.NextSituation(first, last)
                                : run.NativeResumeFrom(first, GetNextMissingSituation(first, last, nativeWinds));
                        }
                        run.SavePending(assignments, starts);
#endif
                        string launchError = null;
                        for (int index = 0; index < assignments.Count; index++)
                        {
                            int count = index + 1;
                            var (assignedFirst, instance_end) = assignments[index];
                            int instance_start = assignedFirst;
                            bool retained = false;
                            bool launched = false;
#if __MonoCS__
                            if (count == 1) // first instance
                            {
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
                                        launched = GRAMMProcess.Start();
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
                                        launched = GRAMMProcess.Start();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else // further instances
                            {
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
                                        launched = GRAMMProcess.Start();
                                    }
                                    else
                                    {
                                        string command = "gnome-terminal -x bash -ic 'cd '" + Path.GetDirectoryName(GRAMM_Program_Path) + "'; mono " + Path.GetFileName(GRAMM_Program_Path) + " " + gramm_start + " ; bash'";
                                        gramm_local = new Process();
                                        gramm_local.StartInfo.FileName = "/bin/bash";
                                        gramm_local.StartInfo.Arguments = "-c \" " + command + " \"";
                                        gramm_local.StartInfo.UseShellExecute = false;
                                        gramm_local.StartInfo.RedirectStandardOutput = true;
                                        launched = gramm_local.Start();
                                    }
                                    Thread.Sleep(800);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
#else
                            if (continueCalculation)
                            {
                                instance_start = starts[index];
                                retained = instance_start > instance_end;
                            }
                            // Reversed ranges make GRAMM fall back to its default full run.
                            if (!retained)
                            {
                                string pathArgument = GRAMM_Project_Path.Length == 0 ? "" : $"\"{di.FullName}\" ";
                                string param = $"\"{GRAMM_Program_Path}\" {pathArgument}{instance_start} {instance_end}";
                                int node = NumaNode(CPUNode);
                                if (count > 1 && CPUNode == 4) { node = count % 2; }
                                else if (count > 1 && CPUNode == 5) { node = count % 4; }
                                try
                                {
                                    int processID = NumaProcessStarter.StartProcessOnNumaNode(param, node, di.FullName);
                                    if (processID > 0)
                                    {
                                        CoreProcessID.Add(processID);
                                        launched = true;
                                    }
                                }
                                catch (Exception ex) { launchError ??= ex.Message; }
                            }
#endif

                            if (launched || retained)
                            {
                                run.Ranges.Add((count, assignedFirst, instance_end, instance_start));
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
                                    if (retained)
                                    {
                                        batchfile.WriteLine("rem No situations to resume in this instance.");
                                    }
                                    else if (copyCore)
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
                        }
                        if (instances > 1) { ShowGrammProgress(run); }
                        if (launchError != null)
                        {
                            MessageBox.Show(launchError, "GRAMM could not start all instances", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (grammProgress?.Ranges.Count == 0) { ResetGrammProgress(); }
#if __MonoCS__
            MessageBox.Show("This function is not available at LINUX", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
#else
            foreach (int processID in CoreProcessID)
            {
                try
                {
                    Process localById = Process.GetProcessById(processID);
                    localById.Kill();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            }
            CoreProcessID.Clear();

            progressBar2.Value = 0;
            if (grammProgress == null) { numericUpDown24.Value = 1; }
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
            if (grammProgress?.Ranges.Count == 0) { ResetGrammProgress(); }
#if __MonoCS__
            MessageBox.Show("This function is not available at LINUX", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
#endif
            foreach (int processID in CoreProcessID)
            {
                try
                {
                    Process localById = Process.GetProcessById(processID);
                    localById.Kill();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            }
            CoreProcessID.Clear();

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
            if (grammProgress == null) { numericUpDown24.Value = trackbar; }
            progressBar2.Value = 0;
            label67.Text = "Actual flow situation: " + "0" + "%";
            //save GRAMM control file "GRAMMin.dat"
            GRAMMin(EmifileReset);
            //check if GRAL simulation should be enabled
            Enable_GRAL();
        }

        /// <summary>
        /// Check any existing calculated results when continuing a calculation
        /// </summary>
        /// <param name="StartSituation">1st situation that should be calculated</param>
        /// <param name="FinalSituation">final situation that should be calculated</param>
        /// <param name="Files">FileInfo array</param>
        /// <returns></returns>
        private int GetNextMissingSituation(int StartSituation, int FinalSituation, FileInfo[] Files)
        {
            int firstnewSit = StartSituation;
            for (; firstnewSit <= FinalSituation; firstnewSit++)
            {
                string number = firstnewSit.ToString("00000");
                if (!Array.Exists(Files, element => element.Name.StartsWith(number)))
                {
                    break;
                }
            }
            return firstnewSit;
        }
    }
}