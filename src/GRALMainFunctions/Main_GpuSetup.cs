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
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace Gral
{
    /// <summary>
    /// Checks for the GPU option: GPU driver, runtime compiler and device, so that the user is told what is missing
    /// </summary>
    partial class Main
    {
        private const string NvidiaDriverPage = "https://www.nvidia.com/en-us/drivers/";
        private const string AmdDriverPage = "https://www.amd.com/en/support/download/drivers.html";
        /// <summary>
        /// The GRAL executable and the GPU check of the last GRAL start in this session, shown by the GPU options
        /// </summary>
        private string gpuLastGralExecutable = string.Empty;
        private bool gpuCheckDone;
        private (string Status, string Message, JsonElement Report)? gpuLastCheck;
        private System.Drawing.Image gpuOptionsImageDisabled;

        /// <summary>
        /// The GPU options button follows the GPU check box: visible with it, usable when it is ticked
        /// </summary>
        private void ShowGpuOptionsButton()
        {
            buttonGPUOptions.Visible = checkBoxGPU.Visible;
            buttonGPUOptions.Enabled = checkBoxGPU.Checked;
            if (gpuOptionsImageDisabled == null)
            {
                gpuOptionsImageDisabled = ToolStripRenderer.CreateDisabledImage(Gral.Properties.Resources.WrenchBlueTransparent);
            }
            buttonGPUOptions.BackgroundImage = checkBoxGPU.Checked ? Gral.Properties.Resources.WrenchBlueTransparent : gpuOptionsImageDisabled;
        }

        /// <summary>
        /// GPU options: device and start behaviour (GPU settings file of GRAL), reproducible sums (in.dat line 24), status and log
        /// </summary>
        private void ButtonGPUOptions_Click(object sender, EventArgs e)
        {
            using (global::GralMainForms.GpuOptions options = new global::GralMainForms.GpuOptions())
            {
                options.GralExecutable = gpuLastGralExecutable;
                options.CheckDone = gpuCheckDone;
                if (gpuLastCheck != null)
                {
                    options.CheckStatus = gpuLastCheck.Value.Status;
                    options.CheckMessage = gpuLastCheck.Value.Message ?? string.Empty;
                    options.CheckReport = gpuLastCheck.Value.Report;
                }
                options.GRALReproducibleResults = GRALSettings.ReproducibleResults;
                options.GpuReproducibleSums = GRALSettings.GpuReproducibleSums;
                options.GpuLogFile = Path.Combine(ProjectName, "Computation", "GRAL_GPU_runtime.log");
                options.Icon = Icon;
                options.StartPosition = FormStartPosition.Manual;
                options.Left = Left + 80;
                options.Top = Top + 40;

                if (options.ShowDialog(this) == DialogResult.OK && options.GpuReproducibleSums != GRALSettings.GpuReproducibleSums && !Project_Locked)
                {
                    // Stored in the project's in.dat (line 24), written at once like the GPU option itself
                    GRALSettings.GpuReproducibleSums = options.GpuReproducibleSums;
                    GralIO.InDatFileIO write_in_dat = new GralIO.InDatFileIO
                    {
                        Data = GRALSettings
                    };
                    write_in_dat.WriteInDat();
                }
            }
        }

        /// <summary>
        /// When the GPU option is ticked: check that a driver with GPU compute support is installed
        /// </summary>
        private void CheckGpuDriverOnTick()
        {
            if (!OperatingSystem.IsWindows())
            {
                return; // the driver files and the registry are looked up for Windows only; elsewhere GRAL reports a missing driver at the start
            }
            string system = Environment.SystemDirectory;
            if (File.Exists(Path.Combine(system, "nvcuda.dll")) || File.Exists(Path.Combine(system, "amdhip64_7.dll")))
            {
                return;
            }
            string vendor = GpuVendorFromRegistry();
            string page = vendor == "AMD" ? AmdDriverPage : NvidiaDriverPage;
            if (MessageBox.Show(this, "No GPU driver for GRAL was found: neither the NVIDIA driver (nvcuda.dll) nor an AMD Adrenalin driver with the HIP 7 runtime (amdhip64_7.dll)." +
                                Environment.NewLine + "Until one is installed, each GRAL start asks whether to run on the CPU; clear the box to always use the CPU." +
                                Environment.NewLine + Environment.NewLine +
                                "Open the " + (vendor ?? "NVIDIA") + " driver download page?", "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                OpenWebPage(page);
            }
        }

        /// <summary>
        /// Vendor of the installed display adapters from the registry: "NVIDIA", "AMD" or null
        /// </summary>
        private static string GpuVendorFromRegistry()
        {
            if (!OperatingSystem.IsWindows())
            {
                return null;
            }
            try
            {
                using (Microsoft.Win32.RegistryKey display = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"))
                {
                    if (display == null)
                    {
                        return null;
                    }
                    string vendor = null;
                    foreach (string name in display.GetSubKeyNames())
                    {
                        string id;
                        try
                        {
                            // Adapters are 0000, 0001, ...; other subkeys such as "Properties" may deny read access
                            using (Microsoft.Win32.RegistryKey adapter = display.OpenSubKey(name))
                            {
                                id = (adapter?.GetValue("MatchingDeviceId") as string ?? string.Empty).ToUpperInvariant();
                            }
                        }
                        catch
                        {
                            continue;
                        }
                        if (id.Contains("VEN_10DE"))
                        {
                            return "NVIDIA";
                        }
                        if (id.Contains("VEN_1002"))
                        {
                            vendor = "AMD";
                        }
                    }
                    return vendor;
                }
            }
            catch
            {
                return null;
            }
        }

        private void OpenWebPage(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo() { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + Environment.NewLine + url, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Runs "GRAL --gpu-check" with the GUI's environment
        /// </summary>
        /// <returns>Status and message, or null if this GRAL build has no check mode</returns>
        private static (string Status, string Message, JsonElement Report)? RunGralGpuCheck(string GralExecutable)
        {
            if (!string.Equals(Path.GetExtension(GralExecutable), ".exe", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            // An older GRAL treats the argument as a project folder: an empty working folder and closed input make it stop at once
            string folder = Path.Combine(Path.GetTempPath(), "GRAL_GPU_check_" + Environment.ProcessId.ToString());
            try
            {
                Directory.CreateDirectory(folder);
                ProcessStartInfo start = new ProcessStartInfo(GralExecutable)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    WorkingDirectory = folder
                };
                start.ArgumentList.Add("--gpu-check");
                using (Process gral = Process.Start(start))
                {
                    gral.StandardInput.Close();
                    System.Threading.Tasks.Task<string> output = gral.StandardOutput.ReadToEndAsync();
                    if (!gral.WaitForExit(60000))
                    {
                        gral.Kill(true);
                        return ("timeout", "GRAL --gpu-check did not finish within 60 s", default);
                    }
                    foreach (string line in output.Result.Split('\n'))
                    {
                        if (line.StartsWith("GRAL_GPU_CHECK ", StringComparison.Ordinal))
                        {
                            using (JsonDocument report = JsonDocument.Parse(line.Substring(15)))
                            {
                                JsonElement root = report.RootElement.Clone();
                                string status = root.TryGetProperty("status", out JsonElement s) ? s.GetString() : "failed";
                                string message = root.TryGetProperty("message", out JsonElement m) ? m.GetString() : string.Empty;
                                return (status, message, root);
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return ("failed", ex.Message, default);
            }
            finally
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Asks GRAL whether the GPU can be used and helps with what is missing
        /// </summary>
        /// <returns>False if the start is cancelled; true to start, on the GPU or, if the user chose so, on the CPU</returns>
        private bool EnsureGralGpuReady(string GralExecutable)
        {
            for (int attempt = 0; attempt < 3; attempt++)
            {
                var check = RunGralGpuCheck(GralExecutable);
                gpuCheckDone = true;
                gpuLastCheck = check;
                if (check == null || check.Value.Status == "ok")
                {
                    return true; // ready, or an older GRAL that reports problems itself at the start
                }
                string status = check.Value.Status;
                string message = check.Value.Message;
                DialogResult answer;
                if (status == "only_integrated")
                {
                    // The choice is kept in the GPU settings file of GRAL (allow_integrated = 1). A GRAL that does not report the file does not read it
                    // and needs INU_GPU_BACKEND=hip, which is then set for this start only
                    bool readsSettingsFile = check.Value.Report.ValueKind == JsonValueKind.Object && check.Value.Report.TryGetProperty("settings_file_status", out _);
                    string settingsFile = GralIO.GpuSettingsFile.FilePath();
                    bool saved = false;
                    try
                    {
                        saved = GralIO.GpuSettingsFile.Read(settingsFile).TryGetValue("allow_integrated", out string allowed) && allowed == "1";
                    }
                    catch { }
                    answer = saved && !readsSettingsFile ? DialogResult.Yes :
                             MessageBox.Show(this, "Only an integrated AMD GPU is available. GRAL has been compared with the CPU path on a discrete GPU only." + Environment.NewLine + Environment.NewLine +
                                             "Yes: use the integrated GPU (remembered)" + Environment.NewLine + "No: run on the CPU this time" + Environment.NewLine + "Cancel: do not start",
                                             "GRAL GUI", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (answer == DialogResult.Yes)
                    {
                        if (!saved && settingsFile != null)
                        {
                            try
                            {
                                GralIO.GpuSettingsFile.Update(settingsFile, new System.Collections.Generic.Dictionary<string, string> { ["allow_integrated"] = "1" });
                                saved = true;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(this, "The choice could not be saved in " + settingsFile + ": " + ex.Message + Environment.NewLine + "The integrated GPU is used for this start only.", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        if (!saved || !readsSettingsFile)
                        {
                            Environment.SetEnvironmentVariable("INU_GPU_BACKEND", "hip");
                            gpuBackendSetForStart = true;
                        }
                        continue;
                    }
                }
                else if (status == "no_driver" || status == "driver_error")
                {
                    bool amd = GpuVendorFromRegistry() == "AMD";
                    if (amd && OperatingSystem.IsWindows() && !File.Exists(Path.Combine(Environment.SystemDirectory, "amdhip64_7.dll")))
                    {
                        message = "the AMD driver has no HIP 7 runtime (amdhip64_7.dll); a current AMD Adrenalin driver installs it";
                    }
                    string page = amd ? AmdDriverPage : NvidiaDriverPage;
                    answer = MessageBox.Show(this, "GRAL cannot use the GPU: " + message + Environment.NewLine + Environment.NewLine +
                                             "Yes: open the driver download page" + Environment.NewLine + "No: run on the CPU this time" + Environment.NewLine + "Cancel: do not start",
                                             "GRAL GUI", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    if (answer == DialogResult.Yes)
                    {
                        OpenWebPage(page);
                        return false;
                    }
                }
                else if ((status == "no_nvrtc" || status == "no_hiprtc" ||
                          // a compiler was found but cannot serve this GPU: the tested version in the per-user folder may help
                          ((status == "nvrtc_rejected" || status == "hiprtc_rejected") &&
                           !Directory.Exists((status == "hiprtc_rejected" ? GpuToolchainPackage.Hiprtc : GpuToolchainPackage.Nvrtc).TargetFolder))) &&
                         // GRAL uses only the compiler folder that is set, so a download would not help
                         !GpuCompilerFolderIsSet(status.Contains("hiprtc"), check.Value.Report))
                {
                    answer = AskForGpuCompiler(status.StartsWith("no_hiprtc") || status.StartsWith("hiprtc"), status.EndsWith("_rejected"));
                    if (answer == DialogResult.Retry)
                    {
                        continue;
                    }
                }
                else
                {
                    // hiprtc's amd_comgr needs the Visual C++ runtime
                    string runtimeHint = !OperatingSystem.IsWindows() || File.Exists(Path.Combine(Environment.SystemDirectory, "vcruntime140_1.dll")) ? string.Empty :
                                         Environment.NewLine + "The Microsoft Visual C++ x64 runtime is missing: https://aka.ms/vs/17/release/vc_redist.x64.exe";
                    if (status == "invalid_setting")
                    {
                        runtimeHint = Environment.NewLine + "Correct the INU_GPU_* environment variable, or the GPU settings file (GPU options, \"Edit settings file\").";
                    }
                    answer = MessageBox.Show(this, "GRAL cannot use the GPU (" + status + "): " + message + runtimeHint + Environment.NewLine + Environment.NewLine +
                                             "Run on the CPU this time?", "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes ? DialogResult.No : DialogResult.Cancel;
                }
                if (answer == DialogResult.No) // CPU run this time; the next start uses the GPU option again
                {
                    Environment.SetEnvironmentVariable("INU_GPU_EXPERIMENTAL", "0"); // overrides in.dat line 23 for this start; cleared again after the start
                    gpuCpuThisStart = true;
                    return true;
                }
                return false;
            }
            MessageBox.Show(this, "GRAL still cannot use the GPU. Clear the GPU option to run on the CPU.", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        /// <summary>
        /// A folder for the runtime compiler is set: by INU_GPU_NVRTC_DIR or INU_GPU_HIPRTC_DIR, or by nvrtc_dir or hiprtc_dir of the GPU settings file
        /// when this GRAL reads that file. GRAL then looks nowhere else
        /// </summary>
        private static bool GpuCompilerFolderIsSet(bool Hip, JsonElement Report)
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Hip ? "INU_GPU_HIPRTC_DIR" : "INU_GPU_NVRTC_DIR")))
            {
                return true;
            }
            if (Report.ValueKind != JsonValueKind.Object || !Report.TryGetProperty("settings_file_status", out _))
            {
                return false;
            }
            try
            {
                return GralIO.GpuSettingsFile.Read(GralIO.GpuSettingsFile.FilePath()).ContainsKey(Hip ? "hiprtc_dir" : "nvrtc_dir");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// The runtime compiler (NVRTC or hiprtc) is missing or does not serve this GPU: offer the installation
        /// </summary>
        /// <returns>Retry = check again, No = run on the CPU, Cancel or other = do not start</returns>
        private DialogResult AskForGpuCompiler(bool Hip, bool Rejected)
        {
            string page = Hip ? "https://www.amd.com/en/developer/resources/rocm-hub/hip-sdk.html" : "https://developer.nvidia.com/cuda-downloads";
            GpuToolchainPackage package = Hip ? GpuToolchainPackage.Hiprtc : GpuToolchainPackage.Nvrtc;
            DialogResult answer = MessageBox.Show(this, "GRAL needs " + (Hip ? "hiprtc, the AMD HIP runtime compiler," : "NVRTC, the NVIDIA CUDA runtime compiler,") +
                                                  (Rejected ? " but the installed one cannot compile for this GPU." : " which is not installed.") + Environment.NewLine + Environment.NewLine +
                                                  "Yes: download " + package.Title + "," + Environment.NewLine + "about " + (package.DownloadBytes >> 20).ToString() +
                                                  " MB (" + (package.PackageBytes >> 20).ToString() + " MB if the server does not support partial downloads), into %LOCALAPPDATA%\\GRAL\\GpuToolchain\\" +
                                                  package.Kind + Environment.NewLine + "(licence: " + package.LicenseUrl + ")" + Environment.NewLine +
                                                  "No: run on the CPU this time" + Environment.NewLine + "Cancel: do not start",
                                                  "GRAL GUI", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes)
            {
                return answer;
            }
            string error = InstallGpuCompiler(package);
            if (error == null)
            {
                return DialogResult.Retry;
            }
            if (error.Length == 0) // download cancelled
            {
                return DialogResult.Cancel;
            }
            answer = MessageBox.Show(this, "The download failed: " + error + Environment.NewLine + Environment.NewLine +
                                     "Yes: open the download page of the " + (Hip ? "AMD HIP SDK" : "NVIDIA CUDA Toolkit") + Environment.NewLine +
                                     "No: run on the CPU this time" + Environment.NewLine + "Cancel: do not start",
                                     "GRAL GUI", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (answer == DialogResult.Yes)
            {
                OpenWebPage(page);
                return DialogResult.Cancel;
            }
            return answer;
        }

        /// <summary>
        /// Download and install a runtime compiler with a progress window
        /// </summary>
        /// <returns>null on success, an empty string if cancelled, otherwise the error message</returns>
        private string InstallGpuCompiler(GpuToolchainPackage Package)
        {
            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            GralMessage.WaitProgressbarCancel wait = new GralMessage.WaitProgressbarCancel("Downloading " + Package.Kind + " for GRAL", ref cts);
            wait.ProgressbarUpdate(this, 100);
            // The main window stays disabled, so no second start can begin while the messages are pumped
            Enabled = false;
            wait.Show();
            long received = 0, total = Package.DownloadBytes;
            System.Threading.Tasks.Task install = System.Threading.Tasks.Task.Run(() => Package.Install((bytes, size) =>
            {
                System.Threading.Interlocked.Exchange(ref total, size);
                System.Threading.Interlocked.Exchange(ref received, bytes);
            }, cts.Token));
            int shown = 0;
            while (!install.IsCompleted)
            {
                int percent = (int)Math.Min(99, System.Threading.Interlocked.Read(ref received) * 100 / Math.Max(1, System.Threading.Interlocked.Read(ref total)));
                for (; shown < percent; shown++)
                {
                    wait.ProgressbarUpdate(this, 0);
                }
                Application.DoEvents();
                System.Threading.Thread.Sleep(50);
            }
            wait.Close();
            Enabled = true;
            bool cancelled = cts.IsCancellationRequested;
            cts.Dispose();
            if (install.Exception == null)
            {
                return null;
            }
            return cancelled ? string.Empty : install.Exception.GetBaseException().Message;
        }
    }
}
