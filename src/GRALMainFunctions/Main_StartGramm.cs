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
using System.Linq;
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
            dialog.Filter = "GRAMM executable (GRAMM;GRAMM*.dll;GRAMM*.exe)|GRAMM;GRAMM*.dll;GRAMM*.exe";
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

                        if (!string.Equals(Path.GetFullPath(dialog.FileName), Path.GetFullPath(GRAMM_Program_Path),
                            Path.DirectorySeparatorChar == '/' ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
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
                    bool single = instances == 1;
                    bool trackSingle = single && (engine.Generation != GrammEngine.Legacy || GrammProgressRun.HasCheckpoint(Project_Computation_Path));
                    if (trackSingle && continueCalculation && GrammInstanceRanges.IsSteadyBinaryProject(Project_Computation_Path))
                    {
                        single = false;
                    }
                    if (single)
                    {
                        var nativeRun = trackSingle ? new GrammProgressRun(Project_Computation_Path) { EngineGeneration = engine.Generation } : null;
                        nativeRun?.CaptureNativeBaseline();
                        int startSituation = (int)numericUpDown24.Value;
                        if (continueCalculation)
                        {
                            startSituation = GrammProgressRun.NextMissingWind(1, 10000, GrammProgressRun.ExistingWindNumbers(di));
                            if (nativeRun != null) { startSituation = nativeRun.NativeResumeFrom(1, startSituation); }
                        }
                        nativeRun?.SaveNativePending(startSituation);
#if __MonoCS__
                        StartUnixGramm(GRAMM_Program_Path, di.FullName,
                            continueCalculation ? startSituation : (int?)null, continueCalculation ? 9999999 : (int?)null);
#else
                        string param = $"\"{GRAMM_Program_Path}\"";
                        if (!string.IsNullOrEmpty(GRAMM_Project_Path) || !continueCalculation) { param += $" \"{GRAMM_Project_Path}\""; }
                        // Omitting the range lets GRAMM use its control file and write single-instance receptor output.
                        if (continueCalculation) { param += $" {startSituation} 9999999"; }
                        StartWindowsGramm(param, NumaNode(CPUNode), di.FullName);
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
                                return GrammInstanceRanges.SelectForEngine(engine.Generation == GrammEngine.Reset, Project_Computation_Path,
                                    first_sit, final_sit, instances, sunriseOriginalCount);
                            });
                        }
                        finally
                        {
                            if (grammProgress == run && !IsDisposed) { groupBox30.Enabled = true; }
                        }
                        if (grammProgress != run || IsDisposed || Disposing) { return; }
                        int[] nativeStarts = null;
                        if (continueCalculation && run.LegacyResume)
                        {
                            var winds = GrammProgressRun.ExistingWindNumbers(di);
                            nativeStarts = assignments.Select(range =>
                                run.NativeResumeFrom(range.First, GrammProgressRun.NextMissingWind(range.First, range.Last, winds))).ToArray();
                        }
                        int[] starts = run.SavePending(assignments, nativeStarts);
                        string launchError = null;
                        for (int index = 0; index < assignments.Count; index++)
                        {
                            int count = index + 1;
                            var (assignedFirst, instance_end) = assignments[index];
                            int instance_start = starts[index];
                            bool retained = instance_start > instance_end;
                            bool launched = false;
                            // Reversed ranges make GRAMM fall back to its default full run.
                            if (!retained)
                            {
                                try
                                {
#if __MonoCS__
                                    run.Workers.Add(count, StartUnixGramm(GRAMM_Program_Path, di.FullName, instance_start, instance_end));
                                    launched = true;
#else
                                    string pathArgument = GRAMM_Project_Path.Length == 0 ? "" : $"\"{di.FullName}\" ";
                                    string param = $"\"{GRAMM_Program_Path}\" {pathArgument}{instance_start} {instance_end}";
                                    int node = NumaNode(CPUNode);
                                    if (count > 1 && CPUNode == 4) { node = count % 2; }
                                    else if (count > 1 && CPUNode == 5) { node = count % 4; }
                                    launched = StartWindowsGramm(param, node, di.FullName);
#endif
                                }
                                catch (Exception ex) { launchError ??= ex.Message; }
                            }

                            if (launched || retained)
                            {
                                run.Ranges.Add((count, assignedFirst, instance_end, instance_start));
                            }

#if __MonoCS__
                            string script = Path.Combine(di.FullName, $"GRAMM_instance_{count}.sh");
                            try
                            {
                                var args = new List<string>();
                                UnixTerminal.AppendEngineCommand(args, GRAMM_Program_Path);
                                args.Add(di.FullName);
                                args.Add(instance_start.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                args.Add(instance_end.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                string command = retained ? "# This range is complete." : "exec " + string.Join(" ", args.Select(ShellQuote));
                                File.WriteAllText(script, "#!/bin/sh\ncd " + ShellQuote(di.FullName) + " || exit 1\n" + command + "\n");
                            }
                            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                            {
                                launchError ??= ex.Message;
                            }
#else
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

#if __MonoCS__
        private static string ShellQuote(string value) => "'" + value.Replace("'", "'\\''") + "'";

        private UnixTerminal.Engine StartUnixGramm(string executable, string computation, int? first, int? last)
        {
            for (int i = unixGrammWorkers.Count - 1; i >= 0; i--)
            {
                if (!unixGrammWorkers[i].ExitCode.HasValue) { continue; }
                unixGrammWorkers[i].Dispose();
                unixGrammWorkers.RemoveAt(i);
            }
            var args = new List<string> { computation };
            if (first.HasValue)
            {
                args.Add(first.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                args.Add(last.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            var worker = UnixTerminal.StartEngine(executable, computation, args);
            unixGrammWorkers.Add(worker);
            return worker;
        }

#else
        private bool StartWindowsGramm(string command, int node, string computation)
        {
            int pid = NumaProcessStarter.StartProcessOnNumaNode(command, node, computation, out var handle);
            using (handle) { return windowsGrammWorkers.Track(computation, pid); }
        }
#endif

        private async Task GRAMMStopCalculation(object sender, EventArgs e)
        {
            // Retire a pending plan before its background scan can launch a new worker.
            if (grammProgress?.Ranges.Count == 0) { ResetGrammProgress(); }
            string project = ProjectName;
            string computation = Path.GetFullPath(Path.Combine(project, "Computation"));
#if __MonoCS__
            var workers = unixGrammWorkers.Where(w => w.ComputationPath == computation).ToArray();
#else
            var workers = windowsGrammWorkers.InProject(computation);
#endif
            var failures = await Task.WhenAll(workers.Select(async worker =>
            {
                try
                {
#if __MonoCS__
                    if (!worker.ExitCode.HasValue) { await worker.KillAsync(); }
                    worker.Dispose();
                    unixGrammWorkers.Remove(worker);
#else
                    using var timeout = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                    if (!worker.HasExited) { worker.Kill(); await worker.WaitForExitAsync(timeout.Token); }
                    windowsGrammWorkers.Forget(worker);
#endif
                    return null;
                }
                catch (InvalidOperationException) when (IsDisposed || Disposing) { return null; }
                catch (Exception ex) { return ex.Message; }
            }));

            if (IsDisposed || Disposing || project != ProjectName) { return; }
            var errors = failures.Where(message => message != null).Distinct().ToArray();
            if (errors.Length > 0)
            {
                MessageBox.Show(this, string.Join(Environment.NewLine, errors), "GRAMM stop could not be confirmed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            progressBar2.Value = 0;
            if (grammProgress == null) { numericUpDown24.Value = 1; }
            label67.Text = "Actual flow situation: 0%";
            GRAMMin(EmifileReset);
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
