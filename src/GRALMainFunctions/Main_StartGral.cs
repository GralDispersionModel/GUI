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
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
        /// Environment variables of the GPU build of GRAL that the GUI owns: it sets or clears them at every start (INU_GPU_EXPERIMENTAL overrides use_gpu of GPU.txt).
        /// Every other INU_GPU_* variable inherited by the GUI is passed on unchanged; GRAL writes those to its log file
        /// </summary>
        private static readonly string[] gpuVariableNames = { "INU_GPU_EXPERIMENTAL", "INU_GPU_MANIFEST", "INU_GPU_METRICS", "INU_GPU_STDERR" };
        /// <summary>
        /// The GPU option of the project (use_gpu of GPU.txt in the Computation folder), read when the Computation tab is shown, when the box is clicked and once at each start
        /// </summary>
        private bool gpuUseGpu;
        /// <summary>
        /// reproducible_sums of GPU.txt, read together with the GPU option; not set means on
        /// </summary>
        private bool gpuReproducibleSums = true;
        /// <summary>
        /// Why the GUI could not read the GPU.txt of the project at the last reading, or null
        /// </summary>
        private string gpuSettingsReadError;
        /// <summary>
        /// INU_GPU_BACKEND=hip was set for this start only, for a GRAL that does not read GPU.txt
        /// </summary>
        private bool gpuBackendSetForStart;
        private string gpuBackendBeforeStart;
        /// <summary>
        /// The user chose "run on the CPU this time" at the GPU check of the start that is being prepared
        /// </summary>
        private bool gpuCpuThisStart;
        /// <summary>
        /// Status of the GPU check of the start that is being prepared, if the GPU was not ready and GRAL continues on the CPU ("Continue on the CPU" of the GPU options)
        /// </summary>
        private string gpuFallbackThisStart;

        /// <summary>
        /// Set or clear the environment variables that switch the GPU build of GRAL to the GPU.
        /// The GRAL processes started by the GUI inherit them.
        /// </summary>
        /// <param name="GralExecutable">Selected GRAL executable</param>
        /// <returns>False if the start is cancelled</returns>
        private bool SetGralGpuEnvironment(string GralExecutable)
        {
            gpuCpuThisStart = false;
            gpuFallbackThisStart = null;
            // use_gpu of the project's GPU.txt switches GRAL to the GPU; values of the owned variables inherited by the GUI must not change that
            foreach (string name in gpuVariableNames)
            {
                Environment.SetEnvironmentVariable(name, null);
            }
            ReadGpuProjectSettings(); // once per start: the check, the core copy and the log file of this start use the same answer
            if (gpuSettingsReadError != null)
            {
                // GRAL may read the file all the same (UTF-16) and use the GPU without the files and logs the GUI sets up, or it stops at once (a file in use)
                MessageBox.Show(this, "The GPU settings of the project cannot be read (GPU.txt in the Computation folder): " + gpuSettingsReadError + Environment.NewLine +
                                "GRAL is not started until the file can be read.", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!gpuUseGpu)
            {
                return true;
            }

            // The manifest and the kernel source must be in the same folder; the GRAL build writes them to gpu/runtime
            string folder = Path.GetDirectoryName(GralExecutable);
            string manifest = null;
            foreach (string candidate in new[] { Path.Combine(folder, "gpu", "runtime"), folder })
            {
                if (File.Exists(Path.Combine(candidate, "fields_manifest.json")) && File.Exists(Path.Combine(candidate, "generated.cu")))
                {
                    manifest = Path.Combine(candidate, "fields_manifest.json");
                    break;
                }
            }
            if (manifest == null)
            {
                MessageBox.Show(this, "GPU option: this GRAL has no GPU runtime files (fields_manifest.json and generated.cu next to the executable or in its gpu/runtime subfolder)."
                                + Environment.NewLine
                                + "Select a GRAL build with GPU support, or clear the GPU option.",
                                "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            string computation = Path.Combine(ProjectName, "Computation");
            Environment.SetEnvironmentVariable("INU_GPU_MANIFEST", manifest);
            Environment.SetEnvironmentVariable("INU_GPU_METRICS", Path.Combine(computation, "GRAL_GPU_metrics.jsonl"));
            Environment.SetEnvironmentVariable("INU_GPU_STDERR", Path.Combine(computation, "GRAL_GPU_runtime.log"));
            return EnsureGralGpuReady(GralExecutable);
        }

        /// <summary>
        /// Read the GPU option and the reproducible sums of the project from GPU.txt; a missing file means GPU off and every default.
        /// A file that cannot be read shows the same in the window, and gpuSettingsReadError keeps the reason for the next start
        /// </summary>
        private void ReadGpuProjectSettings()
        {
            gpuUseGpu = false;
            gpuReproducibleSums = true;
            gpuSettingsReadError = null;
            try
            {
                System.Collections.Generic.Dictionary<string, string> settings = GralIO.GpuSettingsFile.Read(GralIO.GpuSettingsFile.FilePath(ProjectName));
                gpuUseGpu = settings.TryGetValue("use_gpu", out string use) && use == "1";
                gpuReproducibleSums = !(settings.TryGetValue("reproducible_sums", out string sums) && sums == "0");
            }
            catch (Exception ex)
            {
                gpuSettingsReadError = ex.Message;
            }
        }

        /// <summary>
        /// Start GRAL simulation
        /// </summary>
        /// <param name="StartSituation">First situation when starting a chunk of situations</param>
        /// <param name="FinalSituation">Final situation when starting a chunk of situations</param>
        private void GRALStartCalculation(object sender, int StartSituation, int FinalSituation)
        {
            bool continueCalculation = sender == button35;

            if (Convert.ToString(listBox5.SelectedItem).Contains("Odour")) // check if the lowest conc. layer > 1.5 * vert. extension
            {
                if (Convert.ToDouble(TBox3[0].Value) < 1.5 * Convert.ToDouble(numericUpDown8.Value))
                {
                    if (MessageBox.Show(
                        "Lowest heigth above ground < 1.5 * vertical dimension" + Environment.NewLine +
                        "The odour concentration-variance model is not available" + Environment.NewLine +
                        "Start calculation although?", "GRAL GUI",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        return;
                    }
                }
            }

            //get information about the maximum number of dispersion situations for the progress bar
            IO_ReadFiles ReadFile = new IO_ReadFiles
            {
                ProjectName = ProjectName,
                DispsituationFrequ = DispSituationfrequ
            };

            //check if GRAL is operated in transient mode
            int transient = 1; // steady state mode
            int weathersit_count = 1;
            int? knownWeatherSituations = null;
            int firstInDatSituation = 1;
            try
            {
                InDatVariables data = new InDatVariables();
                InDatFileIO ReadInData = new InDatFileIO();
                data.InDatPath = Path.Combine(ProjectName, "Computation", "in.dat");
                ReadInData.Data = data;
                if (ReadInData.ReadInDat() == true)
                {
                    firstInDatSituation = Math.Max(1, data.DispersionSituation);
                    if (data.Transientflag == 0)
                    {
                        transient = 0; // transient mode
                        //read mettimeseries.dat
                        string mettimeseries = Path.Combine(ProjectName, "Computation", "mettimeseries.dat");
                        List<string> data_mettimeseries = new List<string>();
                        if (ReadMetTimeSeries(mettimeseries, ref data_mettimeseries) && data_mettimeseries.Count > 0)
                        {
                            knownWeatherSituations = data_mettimeseries.Count;
                        }
                        weathersit_count = Math.Max(data_mettimeseries.Count, 1);
                        //if (data_mettimeseries.Count == 0) // no data available
                        //{
                        //	MessageBox.Show("mettimeseries.dat not available -> correct visualization of the simulation progress not possible");
                        //}
                    }
                }
            }
            catch
            { }

            if (transient == 1) // steady state
            {
                if (ReadFile.ReadMeteopgtAllFile() == true)
                {
                    if (ReadFile.MeteoFrequ > 0)
                    {
                        progressBar4.Maximum = 100;
                    }

                    DispSituationfrequ = ReadFile.DispsituationFrequ;
                }
            }
            else //transient
            {
                if (weathersit_count > 0)
                {
                    progressBar4.Maximum = Convert.ToInt32(weathersit_count);
                }
                DispSituationfrequ = ReadFile.DispsituationFrequ;
            }

            ReadFile = null;

            //search for GRAL executables
            string[] filePaths = Directory.GetFiles(Path.Combine(ProjectName, "Computation"), "GRAL*.exe", SearchOption.TopDirectoryOnly);
            string dir;
            if (filePaths.Length > 0)
            {
                dir = Path.GetDirectoryName(filePaths[0]);
            }
            else
            {
#if __MonoCS__
                filePaths = Directory.GetFiles(Path.Combine(ProjectName, "Computation"), "GRAL*.dll", SearchOption.TopDirectoryOnly);
                if (filePaths.Length == 0)
                {
                    filePaths = Directory.GetFiles(Path.Combine(ProjectName, "Computation"), "GRAL", SearchOption.TopDirectoryOnly);
                }
#else
                filePaths = Directory.GetFiles(Path.Combine(ProjectName, "Computation"), "GRAL*.bat", SearchOption.TopDirectoryOnly);
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
            dialog.Filter = "GRAL executables (GRAL*.dll;GRAL)|GRAL*.dll;GRAL";
#else
            dialog.Filter = "GRAL executables (GRAL*.exe;GRAL*.bat)|GRAL*.exe;GRAL*.bat";
#endif

            dialog.Title = "Select GRAL executable";
            // dialog.ShowHelp = true;

            if (GUISettings.CopyCoresToProject)
            {
                dialog.InitialDirectory = dir;
            }
            else
            {
                dialog.InitialDirectory = Main.GUISettings.DefaultPathForGRAL;
            }
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogExe;
#endif

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                GralGpuRun gpuRun = null;
                string GRAL_Program_Path = dialog.FileName;
                try
                {
                    string GRAL_Project_Path = String.Empty;

                    if (!SetGralGpuEnvironment(dialog.FileName))
                    {
                        return;
                    }

                    bool copyCore = GUISettings.CopyCoresToProject;
#if __MonoCS__
                    // Run Linux apphosts and managed DLLs beside their runtime files and native libraries.
                    copyCore = copyCore && !gpuUseGpu && Path.GetExtension(dialog.FileName).Equals(".exe", StringComparison.OrdinalIgnoreCase);
#endif
                    if (copyCore)
                    {
                        //copy the file GRAL*.exe in the directory "Computation"
                        GRAL_Program_Path = Path.Combine(ProjectName, @"Computation", Path.GetFileName(dialog.FileName));
                        //the GPU runtime files (manifest and kernel source) go with the core in the same layout, so that the copied GRAL also finds them when it is started without the GUI
                        string gpuManifest = Environment.GetEnvironmentVariable("INU_GPU_MANIFEST");
                        string gpuSource = gpuUseGpu && !string.IsNullOrEmpty(gpuManifest) ? Path.GetDirectoryName(gpuManifest) : null;
#if __MonoCS__
                        string relativeGpu = gpuSource == null ? null : MonoCompatibility.GetRelativePath(Path.GetDirectoryName(dialog.FileName), gpuSource);
#else
                        string relativeGpu = gpuSource == null ? null : Path.GetRelativePath(Path.GetDirectoryName(dialog.FileName), gpuSource);
#endif
                        if (relativeGpu != null && relativeGpu != "." &&
                            !string.Equals(relativeGpu, Path.Combine("gpu", "runtime"), StringComparison.OrdinalIgnoreCase))
                        {
                            throw new IOException("The GPU runtime folder must be beside the selected core or in its gpu/runtime directory.");
                        }
                        string gpuTarget = relativeGpu == null ? null : Path.GetFullPath(Path.Combine(ProjectName, "Computation", relativeGpu));
                        try
                        {
                            File.Copy(dialog.FileName, GRAL_Program_Path, true);

                            //only after the core: when the core cannot be replaced (it is running), it keeps the GPU runtime files it was copied with
                            if (gpuTarget != null && !string.Equals(Path.GetFullPath(gpuSource), gpuTarget, StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    Directory.CreateDirectory(gpuTarget);
                                    foreach (string name in new[] { "fields_manifest.json", "generated.cu" })
                                    {
                                        string target = Path.Combine(gpuTarget, name);
                                        if (File.Exists(target))
                                        {
                                            File.SetAttributes(target, FileAttributes.Normal); // a copy of a read-only file is read-only and could not be replaced
                                        }
                                        File.Copy(Path.Combine(gpuSource, name), target, true);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (MessageBox.Show(this, "The GPU runtime files of the GRAL core could not be copied to the project folder: " + ex.Message + Environment.NewLine +
                                                        "This start uses the files of the selected GRAL. The copied core may not find matching files when it is started without the GUI." + Environment.NewLine + Environment.NewLine +
                                                        "Start anyway?", "GRAL GUI", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                                    {
                                        return;
                                    }
                                }
                            }

                            //copy System.Numerics.Vectors.dll for GRAL Version 19.01
                            string numerics_dll = Path.Combine(Path.GetDirectoryName(dialog.FileName), "System.Numerics.Vectors.dll");
                            if (Path.GetExtension(dialog.FileName) == ".exe" && File.Exists(numerics_dll))
                            {
                                string numerics_dest = Path.Combine(ProjectName, @"Computation", Path.GetFileName(numerics_dll));
                                File.Copy(numerics_dll, numerics_dest, true);
                            }

                            string batch = String.Empty;
#if __MonoCS__
                            if (Path.GetExtension(dialog.FileName).ToLower() == ".dll")
                                batch = "GRAL*.dll";
#else
                            if (Path.GetExtension(dialog.FileName).ToLower() == ".bat")
                            {
                                batch = "GRAL*.bat";
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
                        catch (Exception ex)
                        {
                            // A copy fails silently, as before, while the core in the project folder is running or is the selected file itself.
                            // With the GPU option a core or GPU runtime files that are missing after the copy must not go unnoticed
                            if (gpuUseGpu && (!File.Exists(GRAL_Program_Path) || (gpuTarget != null && (!File.Exists(Path.Combine(gpuTarget, "fields_manifest.json")) || !File.Exists(Path.Combine(gpuTarget, "generated.cu"))))) &&
                                MessageBox.Show(this, "The GRAL core or its GPU runtime files could not be copied to the project folder: " + ex.Message + Environment.NewLine + Environment.NewLine +
                                                "Start anyway?", "GRAL GUI", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                            {
                                return;
                            }
                        }
                    }
                    else // do not copy the core to the project folder
                    {
                        GRAL_Project_Path = Path.Combine(ProjectName, @"Computation");
                        if (!GUISettings.CopyCoresToProject)
                        {
                            GUISettings.DefaultPathForGRAL = Path.GetDirectoryName(dialog.FileName);
                            GUISettings.WriteToFile();
                        }
                    }

                    //delete existing concentration fields
                    DirectoryInfo di = new DirectoryInfo(Path.Combine(ProjectName, "Computation"));
                    if (StartSituation == 0 && !continueCalculation) // no deletion of exiting files when running a chunk of situations or continue a calculation
                    {
                        FileInfo[] files_conc = di.GetFiles("*.con");
                        if (files_conc.Length == 0) // compressed files?
                        {
                            files_conc = di.GetFiles("*.grz");
                        }

                        if (transient == 1) // steady state mode
                        {
                            if (files_conc.Length >= numericUpDown5.Value)
                            {
                                DialogResult res = MessageBox.Show("The existing *.con files higher than dispersion number "
                                                                   + Convert.ToString(numericUpDown5.Value)
                                                                   + " will be deleted", "Delete existing concentration files",
                                                                   MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                                if (res == DialogResult.Cancel)
                                {
                                    return; // exit, if user cancels
                                }
                            }

                            for (int i = 0; i < files_conc.Length; i++)
                            {
                                try
                                {
                                    if (Convert.ToInt32(files_conc[i].Name.Substring(0, 5)) >= Convert.ToInt32(numericUpDown5.Value))
                                    {
                                        files_conc[i].Delete();
                                    }
                                }
                                catch { }
                            }
                            files_conc = di.GetFiles("*.odr"); // delete *.odr files
                            if (files_conc.Length > 0)
                            {
                                for (int i = 0; i < files_conc.Length; i++)
                                {
                                    try
                                    {
                                        if (Convert.ToInt32(files_conc[i].Name.Substring(0, 5)) >= Convert.ToInt32(numericUpDown5.Value))
                                        {
                                            files_conc[i].Delete();
                                        }
                                    }
                                    catch { }
                                }
                            }
                        } // delete *.con & *.odr files in transient mode
                    }

                    Project_Locked = true;                  // lock project
                    ProjectLockedButtonClick(null, null); // change locked-Button

                    //generate high resolution GRAL topography from original data defined by the user, if it doesn't exist already
                    if (!File.Exists(Path.Combine(ProjectName, @"Computation", "GRAL_topofile.txt")) && checkBox25.Checked == true)
                    {
                        GralIO.CreateGralTopography Topo = new GralIO.CreateGralTopography();
                        if (Topo.GetFilename())
                        {
                            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                            if (Topo.CreateTopography(this) == false)
                            {
                                MessageBox.Show(this, "Unable to write file 'GRAL_topofile.txt'", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }

                    //start routine GRAL*.exe to compute concentrations
                    gpuRun = PrepareGralGpuRun(di.FullName, GRAL_Program_Path);
                    // multiple instances
                    int numberOfInstances = (int)numericUpDown33.Value;
                    try
                    {
                        if (transient == 1) // steady state mode, in transient mode, weathersitcount is already available
                        {
                            weathersit_count = CountSteadySituations(di);
                            if (gpuRun?.ShowProgress == true && weathersit_count > 0) { knownWeatherSituations = weathersit_count; }
                        }
                        else
                        {
                            //read mettimeseries.dat
                            List<string> data_mettimeseries = new List<string>();
                            bool read = ReadMetTimeSeries(Path.Combine(ProjectName, "Computation", "mettimeseries.dat"), ref data_mettimeseries);
                            knownWeatherSituations = read && data_mettimeseries.Count > 0 ? data_mettimeseries.Count : null;
                            weathersit_count = Math.Max(data_mettimeseries.Count, 1);
                        }
                    }
                    catch { }

                    int first_sit = Convert.ToInt32(numericUpDown5.Value);
                    int final_sit = weathersit_count;
                    int offset = Convert.ToInt32(Math.Max(1, (final_sit - first_sit) / (double)numberOfInstances));
                    int instance_end = first_sit + offset;
                    if (knownWeatherSituations.HasValue) { instance_end = Math.Min(instance_end, final_sit); }

#if __MonoCS__
                    string param = GRAL_Program_Path;
#else
                    string param = gpuRun == null ? GRAL_Program_Path : "\"" + GRAL_Program_Path + "\"";

                    if (gpuRun != null)
                    {
                        param += " \"" + di.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + "\"";
                    }
                    else if (GUISettings.CopyCoresToProject == false)
                    {
                        param += " " + "\"" + GRAL_Project_Path + "\"";
                    }
                    if (GRALSettings.Loglevel > 0)
                    {
                        param += " " + "\"" + "LOGLEVEL0" + GRALSettings.Loglevel.ToString(ic) + "\"";
                    }
#endif
                    bool chunk = StartSituation > 0 && FinalSituation >= StartSituation;
                    bool explicitRange = chunk || numberOfInstances > 1 || continueCalculation;
                    int first = explicitRange ? first_sit : firstInDatSituation;
                    int? last = explicitRange ? final_sit : knownWeatherSituations;
                    if (chunk) { (first, last) = (StartSituation, FinalSituation); }
                    else if (numberOfInstances > 1) { last = instance_end; }
                    StartGralInstance(param, NumaNode(CPUNode), di, gpuRun, first, last, explicitRange);

                    if (StartSituation == 0) // not a chunk of situations->start multiple instances in an own thread to avoid inresponsible UI
                    {
                        StartMultipleInstances(instance_end + 1, final_sit, offset, numberOfInstances, param, di, gpuRun);
                    }

                    Project_Locked = true;                  // lock project
                    ProjectLockedButtonClick(null, null); // change locked-Button
                    WriteGralLogFile(1, "", GRAL_Program_Path);
                }
                catch (Exception error)
                {
                    if (gpuRun != null)
                    {
                        gpuRun.StartFailed(error);
                        if (gpuRun.HasLaunched) { WriteGralLogFile(1, "", GRAL_Program_Path); }
                    }
                    MessageBox.Show(this, "GRAL could not start all requested instances." + Environment.NewLine + error.Message,
                        "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // "run on the CPU this time" sets INU_GPU_EXPERIMENTAL=0, and an older GRAL gets INU_GPU_EXPERIMENTAL=1, for the GRAL instances of this start only; all of them have been started here
                    Environment.SetEnvironmentVariable("INU_GPU_EXPERIMENTAL", null);
                    if (gpuBackendSetForStart)
                    {
                        Environment.SetEnvironmentVariable("INU_GPU_BACKEND", gpuBackendBeforeStart); // the value the GUI was started with, usually none
                        gpuBackendSetForStart = false;
                    }
                }
            }
        }

        private readonly CancellationTokenSource gralStopClosed = new();

        /// <summary>
        /// stop GRAL simulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task GRALStopCalculation(object sender, EventArgs e)
        {
#if __MonoCS__ || NET6_0_OR_GREATER
            string project = ProjectName;
            string computation = Path.Combine(project, "Computation");
            var tracked = activeGpuRuns.Where(run => RunMatchesComputation(run, computation) && run.RunningInstances.Any()).ToList();
            var gpuStops = Task.WhenAll(tracked.Where(run => run.RequestStop()).Select(StopTrackedGralGpuRun));
#endif
#if __MonoCS__
            var cpuStops = Task.WhenAll(UnixGralEngines(computation).Select(engine => SendGralStop(engine.Pid, () => !engine.ExitCode.HasValue)));
#elif NET6_0_OR_GREATER
            var cpuStops = Task.WhenAll(windowsGralWorkers.InProject(computation).Select(engine => SendGralStop(engine.Id, () => !engine.HasExited)));
#else
            try
            {
                GRALProcess.Kill();
            }
            catch
            { }
#endif
#if __MonoCS__ || NET6_0_OR_GREATER
            await Task.WhenAll(gpuStops, cpuStops);
            if (gralStopClosed.IsCancellationRequested || IsDisposed || Disposing || ProjectName != project) { return; }
            if (cpuStops.Result.Contains(false))
            {
                MessageBox.Show(this, "GRAL did not receive the stop request. Retry Stop or stop the engine in its console.",
                    "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
#endif
            //check if *.con files are existing for postprocessing routines
            CheckConFiles();
        }
        public async Task<bool> WaitForClientConnect(int processID)
        {
            using (System.IO.Pipes.NamedPipeServerStream server = new System.IO.Pipes.NamedPipeServerStream("GRALServerStream" + processID.ToString(), System.IO.Pipes.PipeDirection.Out))
            {
                await server.WaitForConnectionAsync();
                using (StreamWriter writer = new StreamWriter(server) { AutoFlush = true })
                {
                    await writer.WriteLineAsync("StopGRAL");
                }
            }
            return true;
        }

        private async Task StopTrackedGralGpuRun(GralGpuRun run)
        {
            bool[] delivered = await Task.WhenAll(run.RunningInstances.Select(StopTrackedGralGpuInstance));
            if (delivered.Contains(false)) { run.RetryStopAllowed(); }
        }

        private Task<bool> StopTrackedGralGpuInstance(GralGpuRun.Instance instance) =>
            SendGralStop(instance.Pid, () => instance.IsRunning, instance.Log);

        private async Task<bool> SendGralStop(int pid, Func<bool> running, Action<string> log = null)
        {
            bool StillRunning()
            {
                if (gralStopClosed.IsCancellationRequested) { return false; }
                try { return running(); }
                catch (InvalidOperationException) { return false; } // Tracking ended while the connection was pending.
            }
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(gralStopClosed.Token);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));
            try
            {
                using var server = new System.IO.Pipes.NamedPipeServerStream("GRALServerStream" + pid,
                    System.IO.Pipes.PipeDirection.Out, 1, System.IO.Pipes.PipeTransmissionMode.Byte, System.IO.Pipes.PipeOptions.Asynchronous);
#if __MonoCS__
                using var cancellation = timeout.Token.Register(server.Dispose);
#endif
                await server.WaitForConnectionAsync(timeout.Token);
                if (!StillRunning()) { return true; }
                byte[] stop = System.Text.Encoding.UTF8.GetBytes("StopGRAL" + Environment.NewLine);
                await server.WriteAsync(stop, 0, stop.Length, timeout.Token);
                await server.FlushAsync(timeout.Token);
                log?.Invoke("stop message sent to GRAL.");
                return true;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is OperationCanceledException ||
                                          (timeout.IsCancellationRequested && error is ObjectDisposedException))
            {
                if (!StillRunning()) { return true; }
                log?.Invoke("stop message could not be delivered. Press Stop again or stop GRAL in its console. " + error.Message);
                return false;
            }
        }

        /// <summary>
        /// Start multiple GRAL instances
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartMultipleInstances(int FirstSit, int FinalSit, int Offset, int NumberOfInstances, string GRAL_Arguments, DirectoryInfo Di, GralGpuRun gpuRun)
        {
            Random rnd = new Random();
            int instance_start = FirstSit;
            int instance_end;
            int count = 2; //start with instance 2
            int nodeCounter = 0;

            while (instance_start <= FinalSit)
            {
                System.Threading.Thread.Sleep(100); // wait for 0.1 seconds -> use all CPU groups
                instance_end = instance_start + Offset;
                if (count == NumberOfInstances) // avoid rounding errors
                {
                    instance_end = FinalSit;
                }
                int node = CPUNode;
                if (CPUNode == 4)
                {
                    nodeCounter++;
                    node = nodeCounter % 2;
                }
                else if (CPUNode == 5)
                {
                    nodeCounter++;
                    node = nodeCounter % 4;
                }
                StartGralInstance(GRAL_Arguments, node, Di, gpuRun, instance_start, instance_end, true);
                // new start value, new instance
                instance_start = instance_end + 1;
                count++;
            }
        }

        private static int CountSteadySituations(DirectoryInfo computation) =>
            (int)GralStaticFunctions.St_F.CountLinesInFile(Path.Combine(computation.FullName, "meteopgt.all")) - 2;

        private void StartGralInstance(string command, int node, DirectoryInfo computation, GralGpuRun gpuRun, int first, int? last, bool explicitRange)
        {
            if (explicitRange)
            {
                first = GetNextMissingSituation(first, last.Value, computation.GetFiles("*.grz"));
            }
#if __MonoCS__
            if (first > last) { return; }
            var args = new List<string> { computation.FullName };
            if (GRALSettings.Loglevel > 0) { args.Add("LOGLEVEL0" + GRALSettings.Loglevel.ToString(ic)); }
            if (explicitRange) { args.Add($"SITUATIONS:{first}:{last}"); }
            if (gpuRun == null)
            {
                UnixGralEngines(computation.FullName);
                unixGralEngines.Add(UnixTerminal.StartEngine(command, computation.FullName, args));
            }
            else
            {
                var instance = gpuRun.AddInstance(first, last, explicitRange);
                var environment = new Dictionary<string, string> {
                    ["INU_GPU_METRICS"] = instance.MetricsPath,
                    ["INU_GPU_STDERR"] = instance.RuntimePath
                };
                var engine = UnixTerminal.StartEngine(command, computation.FullName, args, environment, instance.ExitPath);
                instance.Started(engine.Pid, engine, () => engine.ExitCode);
            }
#else
            if (explicitRange) { command += $" \"SITUATIONS:{first}:{last}\""; }
            if (gpuRun == null)
            {
                int cpuProcessID = NumaProcessStarter.StartProcessOnNumaNode(command, node, null, out var process);
                using (process)
                {
                    if (cpuProcessID > 0) { windowsGralWorkers.Track(computation.FullName, cpuProcessID); }
                }
                return;
            }
            if (first > last) { return; }
            var instance = gpuRun.AddInstance(first, last, explicitRange);
            string metrics = Environment.GetEnvironmentVariable("INU_GPU_METRICS");
            string runtime = Environment.GetEnvironmentVariable("INU_GPU_STDERR");
            try
            {
                Environment.SetEnvironmentVariable("INU_GPU_METRICS", instance.MetricsPath);
                Environment.SetEnvironmentVariable("INU_GPU_STDERR", instance.RuntimePath);
                int processID = NumaProcessStarter.StartProcessOnNumaNode(command, node, computation.FullName, out var process);
                instance.Started(processID, process, () => NumaProcessStarter.ReadExitCode(process));
            }
            finally
            {
                Environment.SetEnvironmentVariable("INU_GPU_METRICS", metrics);
                Environment.SetEnvironmentVariable("INU_GPU_STDERR", runtime);
            }
#endif
        }

#if __MonoCS__
        private readonly List<UnixTerminal.Engine> unixGralEngines = new();

        private List<UnixTerminal.Engine> UnixGralEngines(string computation)
        {
            foreach (var engine in unixGralEngines.Where(engine => engine.ExitCode.HasValue).ToArray())
            {
                unixGralEngines.Remove(engine);
                engine.Dispose();
            }
            string path = Path.GetFullPath(computation);
            return unixGralEngines.Where(engine => engine.ComputationPath == path).ToList();
        }
#elif NET6_0_OR_GREATER
        private readonly WindowsRunProcesses windowsGralWorkers = new();
#endif

        private void CloseGralProcessHandles()
        {
            gralStopClosed.Cancel();
            gralStopClosed.Dispose();
#if __MonoCS__
            foreach (var worker in unixGralEngines) { worker.Dispose(); }
            unixGralEngines.Clear();
#elif NET6_0_OR_GREATER
            windowsGralWorkers.Dispose();
#endif
        }

        private static int NumaNode(int CPUNode)
        {
            Random rnd = new Random();
            int node = CPUNode;
            if (CPUNode == 4)
            {
                node = rnd.Next(0, 2);
            }
            else if (CPUNode == 5)
            {
                node = rnd.Next(0, 4);
            }
            return node;
        }
    }


    public class NumaProcessStarter
    {
        // Necessary P/Invoke constants and structs
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool CreateProcess(
            string lpApplicationName,
            System.Text.StringBuilder lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFOEX lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool InitializeProcThreadAttributeList(
            IntPtr lpAttributeList,
            int dwAttributeCount,
            int dwFlags,
            ref IntPtr lpSize);

        [DllImport("kernel32.dll")]
        static extern void DeleteProcThreadAttributeList(IntPtr lpAttributeList);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool UpdateProcThreadAttribute(
            IntPtr lpAttributeList,
            uint dwFlags,
            IntPtr attribute,
            IntPtr lpValue,
            IntPtr cbSize,
            IntPtr lpPreviousValue,
            IntPtr lpReturnSize);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll")]
        private static extern uint WaitForSingleObject(SafeProcessHandle handle, uint timeout);

        [DllImport("kernel32.dll")]
        private static extern bool GetExitCodeProcess(SafeProcessHandle handle, out int exitCode);

        internal static int? ReadExitCode(SafeProcessHandle handle)
        {
            const uint WaitObject0 = 0;
            const uint WaitTimeout = 258;
            uint wait = WaitForSingleObject(handle, 0);
            if (wait == WaitTimeout) { return null; }
            return wait == WaitObject0 && GetExitCodeProcess(handle, out int code) ? code : int.MinValue;
        }

        // PROC_THREAD_ATTRIBUTE_PREFERRED_NODE = 0x00020004
        static readonly IntPtr PROC_THREAD_ATTRIBUTE_PREFERRED_NODE = (IntPtr)0x00020004;
        const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;

        public static int StartProcessOnNumaNode(string commandLine, int numaNode) =>
            StartProcessOnNumaNode(commandLine, numaNode, null);

        public static int StartProcessOnNumaNode(string commandLine, int numaNode, string workingDirectory)
        {
            int id = StartProcessOnNumaNode(commandLine, numaNode, workingDirectory, out var process);
            process.Dispose();
            return id;
        }

        internal static int StartProcessOnNumaNode(string commandLine, int numaNode, string workingDirectory, out SafeProcessHandle process)
        {
            int error = TryStartOnNode(commandLine, numaNode, workingDirectory, out var pi);
            if (error != 0 && numaNode != 0)
            {
                error = TryStartOnNode(commandLine, 0, workingDirectory, out pi);
            }
            if (error != 0) { throw new System.ComponentModel.Win32Exception(error); }
            try
            {
                process = new SafeProcessHandle(pi.hProcess, true);
                pi.hProcess = IntPtr.Zero; // Ownership transferred to the caller.
                return pi.dwProcessId;
            }
            finally
            {
                CloseHandle(pi.hThread);
                if (pi.hProcess != IntPtr.Zero) { CloseHandle(pi.hProcess); }
            }
        }

        private static unsafe int TryStartOnNode(string commandLine, int numaNode, string workingDirectory, out PROCESS_INFORMATION pi)
        {
            pi = default;
            var si = new STARTUPINFOEX();
            si.StartupInfo.cb = Marshal.SizeOf(typeof(STARTUPINFOEX));
            IntPtr size = IntPtr.Zero;
            InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref size);
            if (size == IntPtr.Zero) { return Marshal.GetLastWin32Error(); }

            si.lpAttributeList = Marshal.AllocHGlobal(size);
            bool initialized = false;
            ushort node = (ushort)numaNode;
            try
            {
                initialized = InitializeProcThreadAttributeList(si.lpAttributeList, 1, 0, ref size);
                if (!initialized ||
                    !UpdateProcThreadAttribute(si.lpAttributeList, 0, PROC_THREAD_ATTRIBUTE_PREFERRED_NODE,
                        (IntPtr)(&node), (IntPtr)sizeof(ushort), IntPtr.Zero, IntPtr.Zero))
                {
                    return Marshal.GetLastWin32Error();
                }
                bool started = CreateProcess(null, new System.Text.StringBuilder(commandLine), IntPtr.Zero, IntPtr.Zero, false,
                    EXTENDED_STARTUPINFO_PRESENT, IntPtr.Zero, workingDirectory, ref si, out pi);
                return started ? 0 : Marshal.GetLastWin32Error();
            }
            finally
            {
                if (initialized) { DeleteProcThreadAttributeList(si.lpAttributeList); }
                Marshal.FreeHGlobal(si.lpAttributeList);
            }
        }
    }
}
