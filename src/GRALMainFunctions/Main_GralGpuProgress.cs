using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace Gral
{
    partial class Main
    {
        private readonly List<GralGpuRun> activeGpuRuns = new();
        private Timer gpuRunsTimer;
        private GralGpuRun displayedGpuRun;
        private GralGpuRun latestGpuRun;
        private Panel gpuRunPanel;
        private Label gpuRunStatus;
        private Label gpuRunElapsed;
        private ProgressBar gpuRunProgress;
        private Button gpuRunLog;

        private GralGpuRun PrepareGralGpuRun(string computation, string executable)
        {
            DisplayGralGpuRun(null);
            foreach (var previous in activeGpuRuns.Where(run => RunMatchesComputation(run, computation))) { previous.ShowProgress = false; }
            if (!gpuUseGpu) { return null; }

            try
            {
                var run = new GralGpuRun(computation, executable, GralGpuCheckSummary())
                {
                    ShowProgress = !gpuCpuThisStart && gpuFallbackThisStart == null
                };
                latestGpuRun = run;
                activeGpuRuns.Add(run);
                if (run.ShowProgress) { DisplayGralGpuRun(run); }
                if (gpuRunsTimer == null)
                {
                    gpuRunsTimer = new Timer(components) { Interval = 1000 };
                    gpuRunsTimer.Tick += UpdateGralGpuRuns;
                    FormClosed += (_, _) =>
                    {
                        foreach (var active in activeGpuRuns) { active.CloseMonitoring(); }
                    };
                }
                gpuRunsTimer.Start();
                return run;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                MessageBox.Show(this, "The GPU run log could not be created. GRAL will use its standard logs." + Environment.NewLine + error.Message,
                    "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private void RestoreGralGpuProgress()
        {
            string computation = Path.Combine(ProjectName, "Computation");
            DisplayGralGpuRun(activeGpuRuns.LastOrDefault(run => run.ShowProgress && !run.Finished && RunMatchesComputation(run, computation)));
        }

        private string GralGpuCheckSummary()
        {
            string mode = gpuCpuThisStart ? "CPU requested for this start." : gpuFallbackThisStart != null
                ? "CPU fallback expected after the GPU check." : "GPU requested.";
            if (gpuLastCheck is not { } check) { return "unavailable. " + mode; }

            var summary = new List<string> { check.Status + ". " + mode };
            if (!string.IsNullOrWhiteSpace(check.Message)) { summary.Add(check.Message); }
            JsonElement report = check.Report;
            if (report.ValueKind == JsonValueKind.Object)
            {
                foreach (var field in new[] { ("name", "Device"), ("backend", "Backend"), ("driver_cuda", "Driver runtime"),
                    ("nvrtc_version", "Runtime compiler"), ("target_arch", "Target architecture"), ("settings_file", "GPU settings file"), ("manifest", "GPU manifest") })
                {
                    if (report.TryGetProperty(field.Item1, out JsonElement value) && value.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(value.GetString()))
                    {
                        summary.Add(field.Item2 + ": " + value.GetString());
                    }
                }
                if (report.TryGetProperty("total_memory_bytes", out JsonElement memory) && memory.ValueKind == JsonValueKind.Number && memory.TryGetInt64(out long bytes))
                {
                    summary.Add("GPU memory: " + (bytes / (1024 * 1024)).ToString(CultureInfo.InvariantCulture) + " MiB");
                }
            }
            return string.Join(Environment.NewLine, summary);
        }

        private void DisplayGralGpuRun(GralGpuRun run)
        {
            displayedGpuRun = run;
            if (run != null && gpuRunPanel == null)
            {
                gpuRunPanel = new Panel { Name = "gralGpuRunPanel", AccessibleName = "GRAL GPU progress", Font = label69.Font };
                gpuRunPanel.SetBounds(progressBar4.Left, label69.Top, progressBar4.Width, progressBar3.Bottom - label69.Top);
                gpuRunStatus = new Label { Name = "gralGpuRunStatus", UseMnemonic = false, AutoEllipsis = true };
                gpuRunElapsed = new Label { Name = "gralGpuRunElapsed", UseMnemonic = false, AutoEllipsis = true };
                gpuRunProgress = new ProgressBar { Name = "gralGpuRunProgress", AccessibleName = "GRAL GPU weather situation results" };
                gpuRunLog = new Button { Name = "gralGpuRunLog", Text = "Open GPU run &log", AccessibleName = "Open GPU run log", UseVisualStyleBackColor = true };
                int line = label69.Height;
                gpuRunStatus.SetBounds(0, 0, gpuRunPanel.Width, line + 1);
                gpuRunElapsed.SetBounds(0, gpuRunStatus.Bottom, gpuRunPanel.Width, line + 1);
                gpuRunProgress.SetBounds(0, gpuRunElapsed.Bottom + 3, gpuRunPanel.Width, progressBar4.Height);
                gpuRunLog.SetBounds(0, gpuRunProgress.Bottom + 5, gpuRunPanel.Width, Math.Max(line + 7, gpuRunPanel.Height - gpuRunProgress.Bottom - 5));
                gpuRunLog.Click += (_, _) => OpenGralGpuRunLog(displayedGpuRun.LogPath);
                toolTip1.SetToolTip(gpuRunProgress, "Completed GPU results by weather situation. Progress within a single situation is unavailable.");
                toolTip1.SetToolTip(gpuRunLog, "Open the support log for this run. The instance diagnostics are stored beside it.");
                gpuRunPanel.Controls.AddRange(new Control[] { gpuRunStatus, gpuRunElapsed, gpuRunProgress, gpuRunLog });
                groupBox16.Controls.Add(gpuRunPanel);
            }
            if (gpuRunPanel == null) { return; }

            gpuRunPanel.Visible = run != null;
            label69.Visible = progressBar4.Visible = label68.Visible = progressBar3.Visible = run == null;
            if (run != null) { RefreshGralGpuProgress(); }
        }

        private void UpdateGralGpuRuns(object sender, EventArgs e)
        {
            foreach (var run in activeGpuRuns) { run.Poll(); }
            activeGpuRuns.RemoveAll(run => run.Finished);
            if (displayedGpuRun != null) { RefreshGralGpuProgress(); }
            if (activeGpuRuns.Count == 0) { gpuRunsTimer.Stop(); }
        }

        private void RefreshGralGpuProgress()
        {
            var run = displayedGpuRun;
            var status = run.Status;
            if (gpuRunStatus.Text != status)
            {
                gpuRunStatus.Text = status;
                toolTip1.SetToolTip(gpuRunStatus, status);
            }
            string elapsed = $"Elapsed: {(long)run.Elapsed.TotalHours:00}:{run.Elapsed.Minutes:00}:{run.Elapsed.Seconds:00}";
            if (gpuRunElapsed.Text != elapsed) { gpuRunElapsed.Text = elapsed; }
            bool measured = run.Completed > 0 && run.Planned > 0;
            gpuRunProgress.Style = measured || run.Finished ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
            gpuRunProgress.Maximum = Math.Max(1, run.Planned ?? 1);
            gpuRunProgress.Value = measured ? Math.Min(run.Completed, gpuRunProgress.Maximum) : 0;
            gpuRunProgress.AccessibleDescription = measured
                ? $"{run.Completed} of {run.Planned} weather situations have returned a GPU result."
                : run.Finished ? "GRAL is no longer running. See the run log for GPU results." : "Waiting for a measurable GPU result.";
            if (run.LogError != null)
            {
                gpuRunLog.Text = "Open partial run &log";
                gpuRunLog.AccessibleName = "Open partial GPU run log";
                toolTip1.SetToolTip(gpuRunLog, "The log could not be updated. " + run.LogError);
            }
            else if (gpuRunLog.Text != "Open GPU run &log")
            {
                gpuRunLog.Text = "Open GPU run &log";
                gpuRunLog.AccessibleName = "Open GPU run log";
                toolTip1.SetToolTip(gpuRunLog, "Open the support log for this run. The instance diagnostics are stored beside it.");
            }
        }

        private string LatestGralGpuLog(string computation)
        {
            if (RunMatchesComputation(latestGpuRun, computation)) { return latestGpuRun.LogPath; }
            string runs = Path.Combine(computation, "GRAL_GPU_runs");
            try
            {
                return Directory.EnumerateDirectories(runs)
                    .Select(path => Path.Combine(path, "Run log.txt"))
                    .Where(File.Exists)
                    .OrderByDescending(File.GetCreationTimeUtc)
                    .FirstOrDefault() ?? Path.Combine(computation, "GRAL_GPU_runtime.log");
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                return Path.Combine(computation, "GRAL_GPU_runtime.log");
            }
        }

        private static bool RunMatchesComputation(GralGpuRun run, string computation) => run != null &&
            string.Equals(run.ComputationPath, Path.GetFullPath(computation).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        private void OpenGralGpuRunLog(string fileName)
        {
            try { Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true })?.Dispose(); }
            catch (Exception error)
            {
                MessageBox.Show(this, error.Message + Environment.NewLine + fileName, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

#if __MonoCS__
        private static void StartLinuxGralGpuInstance(GralGpuRun.Instance instance, string executable, string computation, List<string> arguments)
        {
            var start = new ProcessStartInfo("gnome-terminal") { UseShellExecute = false, WorkingDirectory = computation };
            var command = start.ArgumentList;
            foreach (string part in new[] { "--wait", "--", "/bin/bash", "-c", "status=$1; shift; \"$@\"; code=$?; printf '%s\\n' \"$code\" > \"$status\"; exec /bin/bash -i", "gral", instance.ExitPath })
            {
                command.Add(part);
            }
            if (Path.GetExtension(executable).Equals(".dll", StringComparison.OrdinalIgnoreCase)) { command.Add("dotnet"); }
            command.Add(executable);
            foreach (string argument in arguments) { command.Add(argument); }
            start.Environment["INU_GPU_METRICS"] = instance.MetricsPath;
            start.Environment["INU_GPU_STDERR"] = instance.RuntimePath;
            var process = Process.Start(start);
            instance.Started(process.Id, process, () =>
            {
                try
                {
                    if (File.Exists(instance.ExitPath) &&
                        int.TryParse(File.ReadAllText(instance.ExitPath), NumberStyles.Integer, CultureInfo.InvariantCulture, out int code)) { return code; }
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
                return process.HasExited ? int.MinValue : (int?)null;
            });
        }
#endif
    }
}
