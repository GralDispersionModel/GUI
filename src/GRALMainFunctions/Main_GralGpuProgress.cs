using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using GralMainForms;

namespace Gral
{
    partial class Main
    {
        private readonly List<GralGpuRun> activeGpuRuns = new();
        private Timer gpuRunsTimer;
        private readonly Stopwatch gpuFileScanClock = Stopwatch.StartNew();
        private long nextGpuFileScan;
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
                        activeGpuRuns.ForEach(active => active.CloseMonitoring());
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
                    string value = GpuOptions.JsonText(report, field.Item1);
                    if (!string.IsNullOrWhiteSpace(value)) { summary.Add(field.Item2 + ": " + value); }
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
                gpuRunLog = new Button { Name = "gralGpuRunLog", UseVisualStyleBackColor = true };
                int line = label69.Height;
                gpuRunStatus.SetBounds(0, 0, gpuRunPanel.Width, line + 1);
                gpuRunElapsed.SetBounds(0, gpuRunStatus.Bottom, gpuRunPanel.Width, line + 1);
                gpuRunProgress.SetBounds(0, gpuRunElapsed.Bottom + 3, gpuRunPanel.Width, progressBar4.Height);
                gpuRunLog.SetBounds(0, gpuRunProgress.Bottom + 5, gpuRunPanel.Width, Math.Max(line + 7, gpuRunPanel.Height - gpuRunProgress.Bottom - 5));
                gpuRunLog.Click += (_, _) => GpuOptions.OpenFile(this, displayedGpuRun.LogPath);
                toolTip1.SetToolTip(gpuRunProgress, "Completed GPU results by weather situation. Progress within a single situation is unavailable.");
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
            foreach (var run in activeGpuRuns)
            {
                int previousResults = run.Completed;
                run.Poll();
                bool firstResult = previousResults == 0 && run.Completed > 0;
                if (run == displayedGpuRun && tabControl1.SelectedTab == Computation &&
                    (firstResult || run.Finished || gpuFileScanClock.ElapsedMilliseconds >= nextGpuFileScan))
                {
                    nextGpuFileScan = gpuFileScanClock.ElapsedMilliseconds + 3000;
                    RefreshFileSizes(includeWind: false);
                }
            }
            activeGpuRuns.RemoveAll(run => run.Finished);
            if (displayedGpuRun != null) { RefreshGralGpuProgress(); }
            if (activeGpuRuns.Count == 0) { gpuRunsTimer.Stop(); }
        }

        private void RefreshGralGpuProgress()
        {
            var run = displayedGpuRun;
            gpuRunStatus.Text = run.Status;
            toolTip1.SetToolTip(gpuRunStatus, gpuRunStatus.Text);
            var elapsed = run.Elapsed;
            gpuRunElapsed.Text = $"Elapsed: {(long)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
            bool measured = run.Completed > 0 && run.Planned > 0;
            gpuRunProgress.Style = measured || run.Finished ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
            gpuRunProgress.Maximum = Math.Max(1, run.Planned ?? 1);
            gpuRunProgress.Value = measured ? Math.Min(run.Completed, gpuRunProgress.Maximum) : 0;
            gpuRunProgress.AccessibleDescription = measured
                ? $"{run.Completed} of {run.Planned} weather situations have returned a GPU result."
                : run.Finished ? "GRAL is no longer running. See the run log for GPU results." : "Waiting for a measurable GPU result.";
            gpuRunLog.Text = run.LogError == null ? "Open GPU run &log" : "Open partial run &log";
            gpuRunLog.AccessibleName = run.LogError == null ? "Open GPU run log" : "Open partial GPU run log";
            toolTip1.SetToolTip(gpuRunLog, run.LogError == null ? "Open the support log for this run. The instance diagnostics are stored beside it."
                : "The log could not be updated. " + run.LogError);
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
                Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    }
}
