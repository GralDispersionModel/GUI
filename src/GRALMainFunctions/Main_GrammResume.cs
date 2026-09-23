#if !__MonoCS__
using GralIO;
using GralMainForms;
using System.Drawing;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gral
{
    partial class Main
    {
        // Separate from GRAMM_Locked, which protects the model inputs and results.
        private CancellationTokenSource grammResumeCancellation;
        private GrammProgressForm grammProgressForm;
        private Button grammProgressButton;
        private Button grammCpuButton;

        private void InitializeGrammCpuOptions()
        {
            grammCpuButton = new Button { Name = "grammCpuOptions", Text = "CPU...",
                Font = new Font("Segoe UI", 8.25F), UseVisualStyleBackColor = true,
                Location = new Point(groupBox31.ClientSize.Width - 56, Math.Min(checkBoxAVX.Top - 1, groupBox31.ClientSize.Height - 26)), Size = new Size(46, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Right };
            toolTip1.SetToolTip(grammCpuButton, "GRAMM multi-instance CPU placement. Automatic prefers Windows performance classes and preferred cores.");
            grammCpuButton.Click += (sender, e) => ShowGrammCpuOptions();
            groupBox31.Controls.Add(grammCpuButton);
        }

        private static GrammCpuPolicy ReadGrammCpuPolicy()
        {
            try
            {
                string text = File.ReadAllText(Path.Combine(GUISettings.AppSettingsPath, "GRAMM_cpu_policy.txt")).Trim();
                if (Enum.TryParse(text, out GrammCpuPolicy policy) && Enum.IsDefined(policy)) return policy;
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return GrammCpuPolicy.Automatic;
        }

        private void ShowGrammCpuOptions()
        {
            using var dialog = new Form { Text = "GRAMM CPU placement", Font = new Font("Segoe UI", 9F),
                BackColor = SystemColors.Control, ForeColor = SystemColors.ControlText,
                StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false, ShowInTaskbar = false, ClientSize = new Size(450, 240) };
            var choice = new ComboBox { Name = "cpu_policy", DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(16, 18), Size = new Size(418, 24) };
            choice.Items.AddRange(new object[] { "Automatic - performance / preferred cores", "Prefer larger L3 cache", "Windows scheduling" });
            choice.SelectedIndex = (int)ReadGrammCpuPolicy();
            var explanation = new Label { Location = new Point(16, 58), Size = new Size(418, 125), Text =
                "Automatic uses the performance classes and preferred-core ranking reported by Windows.\r\n\r\n" +
                "Larger L3 cache can help some workloads on asymmetric cache CPUs, but is not always faster.\r\n\r\n" +
                "Running instances can move to available preferred CPUs without restarting. Applies to GRAMM multi-instance runs." };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(264, 200), Size = new Size(80, 27), UseVisualStyleBackColor = true };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(354, 200), Size = new Size(80, 27), UseVisualStyleBackColor = true };
            dialog.Controls.AddRange(new Control[] { choice, explanation, ok, cancel });
            dialog.AcceptButton = ok; dialog.CancelButton = cancel;
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                string path = Path.Combine(GUISettings.AppSettingsPath, "GRAMM_cpu_policy.txt");
                string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                try { File.WriteAllText(temp, ((GrammCpuPolicy)choice.SelectedIndex).ToString()); File.Move(temp, path, true); }
                finally { if (File.Exists(temp)) File.Delete(temp); }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            { MessageBox.Show(this, "Could not save the CPU preference. " + ex.Message, "GRAMM CPU placement", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }


        private void SetGrammProgressLayout(bool multi)
        {
            if (grammProgressButton == null)
            {
                grammProgressButton = new Button { Text = "Instance progress", Name = "grammInstanceProgress",
                    Location = new Point(progressBar2.Left, progressBar2.Top - 3), Size = new Size(groupBox15.ClientSize.Width - 16, 26),
                    UseVisualStyleBackColor = true, Font = label67.Font };
                grammProgressButton.Click += (sender, e) => {
                    if (grammProgressForm == null || grammProgressForm.IsDisposed) return;
                    grammProgressForm.Show(this); grammProgressForm.Activate();
                };
                groupBox15.Controls.Add(grammProgressButton);
            }
            progressBar2.Visible = !multi;
            grammProgressButton.Visible = multi;
            label66.AutoSize = label67.AutoSize = false;
            label66.Width = label67.Width = groupBox15.ClientSize.Width - 12;
            label66.AutoEllipsis = label67.AutoEllipsis = true;
            if (!multi && grammProgressForm != null) grammProgressForm.Hide();
        }

        private void UpdateGrammSnapshot(GrammProgressSnapshot snapshot)
        {
            progressBar1.Maximum = Math.Max(1, snapshot.Total);
            progressBar1.Value = Math.Clamp(snapshot.Completed, 0, progressBar1.Maximum);
            label66.Text = $"Missing situations: {snapshot.Remaining:N0}";
            label67.Text = $"Completed: {snapshot.Completed:N0} / {snapshot.Total:N0}";
            grammProgressForm?.Apply(snapshot);
        }

        private async Task StartGrammResumeAsync(string selectedExecutable)
        {
            string computation = Path.GetFullPath(Path.Combine(ProjectName, "Computation"));
            int requestedInstances = (int)numericUpDown33.Value;
            int coresPerInstance = (int)numericUpDown32.Value;
            GrammCpuPolicy cpuPolicy = ReadGrammCpuPolicy();
            int finalSituation = 0;
            int sunriseCount = 0;
            string reportDirectory = null;
            GrammResumePlan plan = null;
            bool previousCoresEnabled = numericUpDown32.Enabled;
            bool previousDispWatcher = dispnrGramm.EnableRaisingEvents;
            bool previousPercentWatcher = percentGramm.EnableRaisingEvents;
            using (var cancellation = new CancellationTokenSource())
            {
                grammResumeCancellation = cancellation;
                button32.Enabled = false;
                grammCpuButton.Enabled = false;
                numericUpDown32.Enabled = false;
                SetGrammProgressLayout(true);
                grammProgressForm?.Dispose();
                grammProgressForm = new GrammProgressForm(new DirectoryInfo(ProjectName).Name, requestedInstances);
                grammProgressForm.PauseRequested += (sender, e) => grammResumeCancellation?.Cancel();
                grammProgressForm.Show(this);
                dispnrGramm.EnableRaisingEvents = false;
                percentGramm.EnableRaisingEvents = false;
                try
                {
                    // A second patched GUI must not run another queue in this project.
                    using var projectLease = new FileStream(Path.Combine(computation, "GRAMM_resume.lock"),
                        FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    var control = new IO_ReadFiles { ProjectName = ProjectName };
                    if (!control.ReadGrammInFile())
                        throw new InvalidDataException("Cannot read GRAMMin.dat.");
                    sunriseCount = control.GRAMMsunrise;
                    ValidateIndependentGrammMode(computation);
                    finalSituation = sunriseCount > 0 ? sunriseCount :
                        checked((int)GralStaticFunctions.St_F.CountLinesInFile(Path.Combine(computation, "meteopgt.all")) - 2);
                    if (finalSituation < 1)
                        throw new InvalidDataException("No meteorological situations found.");

                    GRAMM_Locked = true;
                    Gramm_locked_buttonClick(null, null);
                    label66.Text = "Checking all situations";
                    label67.Text = "Checking existing results...";
                    // Scan from 1, not the first instance's saved progress counter.
                    var scanProgress = new Progress<int>(scanned => {
                        if (grammResumeCancellation != cancellation || plan != null || IsDisposed) return;
                        label67.Text = $"Checked: {scanned:N0} / {finalSituation:N0}";
                        grammProgressForm.ShowChecking(scanned, finalSituation);
                    });
                    plan = await Task.Run(() => GrammResumePlan.Create(computation, 1,
                        finalSituation, requestedInstances, sunriseCount,
                        (scanned, count) => ((IProgress<int>)scanProgress).Report(scanned), cancellation.Token), cancellation.Token);
                    cancellation.Token.ThrowIfCancellationRequested();
                    reportDirectory = Path.Combine(computation, "GRAMM_resume_plans",
                        DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture) + "_" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(reportDirectory);
                    File.WriteAllText(Path.Combine(reportDirectory, "plan.json"), JsonSerializer.Serialize(new
                    {
                        schema = 1,
                        version = Application.ProductVersion,
                        computation,
                        first = 1,
                        final = finalSituation,
                        sunrise_original_count = sunriseCount,
                        requested_instances = requestedInstances,
                        active_instances = plan.MaxInstances,
                        cores_per_instance = coresPerInstance, cpu_policy = cpuPolicy.ToString(),
                        assignments = plan.InstanceAssignments.Select((ranges, index) => new { instance = index + 1,
                            count = ranges.Sum(r => r.Count), ranges = ranges.Select(r => new { first = r.First, last = r.Last }) }),
                        completed = plan.CompletedCount,
                        pending = plan.PendingCount,
                        ranges = plan.PendingRanges.Select(r => new { first = r.First, last = r.Last })
                    }, new JsonSerializerOptions { WriteIndented = true }));
                    UpdateGrammSnapshot(new GrammProgressTracker(plan, requestedInstances).Snapshot());
                    if (plan.PendingCount == 0)
                    {
                        File.WriteAllText(Path.Combine(reportDirectory, "result.json"),
                            "{\"status\":\"pass\",\"calculated\":0}");
                        grammProgressForm.Finish("Completed");
                        MessageBox.Show(this, "All meteorological situations are complete.", "GRAMM");
                        return;
                    }
                    if (plan.FirstPending > 1 && !File.Exists(Path.Combine(computation, "albeq.dat")))
                        throw new InvalidDataException("albeq.dat is missing. Restore it from this unchanged project's Computation backup before resuming.");

                    string executable = PrepareGrammResumeExecutable(selectedExecutable, computation);
                    GRAMM_Locked = true;
                    Gramm_locked_buttonClick(null, null);
                    WriteFileGRAMMWindfeld_txt(ProjectName, computation + Path.DirectorySeparatorChar, true);
                    Textbox16_Set("GRAMM Windfield: " + computation + Path.DirectorySeparatorChar);
                    Write_Gramm_Log(1, finalSituation.ToString(CultureInfo.InvariantCulture), executable);
                    GrammProgressSnapshot pendingProgress = null;
                    using var progressTimer = new System.Windows.Forms.Timer { Interval = 250 };
                    void ApplyPendingProgress()
                    {
                        var snapshot = Interlocked.Exchange(ref pendingProgress, null);
                        if (snapshot != null && !IsDisposed) UpdateGrammSnapshot(snapshot);
                    }
                    progressTimer.Tick += (sender, e) => ApplyPendingProgress();
                    progressTimer.Start();
                    GrammResumeRunResult result;
                    try
                    {
                        result = await new GrammResumeRunner().RunAsync(plan, executable, computation,
                            cancellation.Token, instanceProgress: snapshot => Interlocked.Exchange(ref pendingProgress, snapshot),
                            requestedInstances: requestedInstances, coresPerInstance: coresPerInstance, cpuPolicy: cpuPolicy);
                    }
                    finally
                    {
                        progressTimer.Stop();
                        ApplyPendingProgress();
                    }
                    var finalPlan = await Task.Run(() => GrammResumePlan.Create(computation, 1,
                        finalSituation, requestedInstances, sunriseCount, cancellationToken: cancellation.Token));
                    cancellation.Token.ThrowIfCancellationRequested();
                    if (finalPlan.PendingCount != 0)
                        throw new InvalidDataException("Some GRAMM results are missing or incomplete after the run. Start again to recalculate only those situations.");
                    progressBar1.Value = finalSituation;
                    label66.Text = "Missing situations: 0";
                    label67.Text = $"Completed: {finalSituation:N0} / {finalSituation:N0}";
                    grammProgressForm.Finish("Completed");
                    File.WriteAllText(Path.Combine(reportDirectory, "result.json"), JsonSerializer.Serialize(new
                    {
                        status = "pass", completed_before = plan.CompletedCount,
                        calculated = result.CompletedCount, ranges = result.RangeCount,
                        logs = result.LogDirectory
                    }, new JsonSerializerOptions { WriteIndented = true }));
                    MessageBox.Show(this, $"GRAMM calculation completed.\nCalculated: {result.CompletedCount:N0} missing situations. Preserved: {plan.CompletedCount:N0} previously completed situations.", "GRAMM");
                }
                catch (OperationCanceledException)
                {
                    label67.Text = "Paused - results preserved";
                    grammProgressForm.Finish("Paused");
                    if (reportDirectory != null)
                        File.WriteAllText(Path.Combine(reportDirectory, "result.json"), "{\"status\":\"paused\"}");
                }
                catch (Exception ex)
                {
                    label67.Text = "Stopped - see error details";
                    grammProgressForm.Finish("Stopped after error", true);
                    if (reportDirectory != null)
                        File.WriteAllText(Path.Combine(reportDirectory, "result.json"), JsonSerializer.Serialize(new { status = "error", error = ex.Message }));
                    MessageBox.Show(this, ex.Message, "GRAMM resume", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    dispnrGramm.EnableRaisingEvents = previousDispWatcher;
                    percentGramm.EnableRaisingEvents = previousPercentWatcher;
                    grammResumeCancellation = null;
                    button32.Enabled = true;
                    grammCpuButton.Enabled = true;
                    numericUpDown32.Enabled = previousCoresEnabled;
                    progressBar2.Value = 0;
                    // Keep the scientific input lock. Instance count is safe to change while idle.
                    numericUpDown33.Enabled = true;
                    Enable_GRAL();
                }
            }
        }

        private static void ValidateIndependentGrammMode(string computation)
        {
            string[] control = File.ReadAllLines(Path.Combine(computation, "GRAMMin.dat"));
            int meteoLine = control.Length > 0 && control[0].Contains("Version") ? 1 : 0;
            if (control.Length <= meteoLine || !control[meteoLine].Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Missing-only multi-instance resume requires independent meteopgt.all situations (GRAMMin.dat: y).");
            string[] input = File.ReadAllLines(Path.Combine(computation, "IIN.dat"));
            if (input.Length < 29)
                throw new InvalidDataException("IIN.dat is incomplete; cannot verify the GRAMM mode.");
            int forcing = ParseGrammIntegerSetting(input[25]);
            int outputFormat = ParseGrammIntegerSetting(input[28]);
            if (forcing != 0)
                throw new InvalidDataException("Time-dependent or ERA5 simulations must use the existing single-instance mode. Independent missing ranges cannot be used.");
            if (outputFormat != 0)
                throw new InvalidDataException("Missing-only resume requires GRAMM output format 0 (binary with header).");
        }

        private static int ParseGrammIntegerSetting(string line)
        {
            string[] parts = line.Split(':');
            if (parts.Length < 2) throw new InvalidDataException("Invalid IIN.dat setting: " + line);
            string value = parts[1].Split('!')[0].Trim();
            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        private string PrepareGrammResumeExecutable(string selected, string computation)
        {
            string executable = Path.GetFullPath(selected);
            if (Path.GetExtension(executable).Equals(".bat", StringComparison.OrdinalIgnoreCase))
            {
                executable = Path.ChangeExtension(executable, ".dll");
                if (!File.Exists(executable))
                    throw new InvalidDataException("Select GRAMM.exe or a GRAMM launcher with a matching .dll. Old GRAMM_instance_*.bat files cannot resume missing results.");
            }
            if (GUISettings.CopyCoresToProject)
            {
                string stem = Path.GetFileNameWithoutExtension(executable);
                string sourceDirectory = Path.GetDirectoryName(executable);
                foreach (string suffix in new[] { ".exe", ".dll", ".deps.json", ".runtimeconfig.json" })
                {
                    string source = Path.Combine(sourceDirectory, stem + suffix);
                    string destination = Path.Combine(computation, stem + suffix);
                    if (File.Exists(source) && !source.Equals(destination, StringComparison.OrdinalIgnoreCase))
                        File.Copy(source, destination, true);
                }
                executable = Path.Combine(computation, Path.GetFileName(executable));
            }
            else
            {
                GUISettings.DefaultPathForGRAMM = Path.GetDirectoryName(executable);
                GUISettings.WriteToFile();
            }
            return executable;
        }
    }
}
#endif
