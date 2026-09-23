#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GralIO
{
    /// <summary>Runs only pending weather ranges and owns every child until exit.</summary>
    public sealed class GrammResumeRunner
    {
        public async Task<GrammResumeRunResult> RunAsync(
            GrammResumePlan plan, string executable, string computationDir,
            CancellationToken cancellationToken, Action<int, int> progress = null,
            Action<GrammProgressSnapshot> instanceProgress = null, int requestedInstances = 0, int coresPerInstance = 1,
            GrammCpuPolicy cpuPolicy = GrammCpuPolicy.Automatic)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (string.IsNullOrWhiteSpace(executable)) throw new ArgumentException("Select a GRAMM executable.", nameof(executable));
            executable = Path.GetFullPath(executable);
            computationDir = Path.GetFullPath(computationDir);
            if (!File.Exists(executable)) throw new FileNotFoundException("GRAMM executable was not found.", executable);
            string extension = Path.GetExtension(executable);
            if (!extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The resume runner requires a GRAMM .exe or .dll.", nameof(executable));

            int requested = requestedInstances > 0 ? requestedInstances : Math.Max(1, plan.MaxInstances);
            if (requested < plan.MaxInstances) throw new ArgumentOutOfRangeException(nameof(requestedInstances));
            var tracker = new GrammProgressTracker(plan, requested);
            instanceProgress?.Invoke(tracker.Snapshot());
            cancellationToken.ThrowIfCancellationRequested();
            if (plan.PendingCount == 0) return new GrammResumeRunResult(0, 0, string.Empty);
            if (plan.FirstPending > 1 && !File.Exists(Path.Combine(computationDir, "albeq.dat")))
                throw new InvalidOperationException("albeq.dat is missing. GRAMM must finish its initial weather situation before resuming from a later situation.");

            string logDirectory = Path.Combine(computationDir, "GRAMM_resume_logs",
                DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture) + "_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(logDirectory);
            if (plan.FirstPending == 1 && !plan.IsComplete(1))
            {
                // The stock first-situation core deletes this shared radiation cache itself.
                // Remove it before any child starts so later workers cannot consume the old cache.
                File.Delete(Path.Combine(computationDir, "albeq.dat"));
            }
            var assignments = plan.InstanceAssignments;
            int completed = 0;
            int launched = 0;
            object gate = new object();
            ExceptionDispatchInfo failure = null;
            using var monitor = new GrammConsoleMonitor();
            using var cpuLog = new StreamWriter(Path.Combine(logDirectory, "cpu_placement.log"), false, new UTF8Encoding(false)) { AutoFlush = true };
            using var cpu = new GrammCpuScheduler(coresPerInstance, plan.MaxInstances,
                line => cpuLog.WriteLine(DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture) + " " + line), cpuPolicy);
            using (var stop = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                async Task Worker(int slot)
                {
                    try
                    {
                        foreach (var range in assignments[slot - 1])
                        {
                            stop.Token.ThrowIfCancellationRequested();
                            lock (gate)
                            {
                                tracker.Assign(slot, range);
                                instanceProgress?.Invoke(tracker.Snapshot());
                            }
                            // Recheck just before launch, preserving results completed after the plan was made.
                            int first = range.First;
                            while (first <= range.Last)
                            {
                                stop.Token.ThrowIfCancellationRequested();
                                if (plan.IsComplete(first))
                                {
                                    Report(slot, 1, GrammInstanceState.Running);
                                    first++;
                                    continue;
                                }
                                int last = first;
                                while (last < range.Last && !plan.IsComplete(last + 1)) last++;
                                int verified = 0;
                                await RunRangeAsync(plan, executable, computationDir, first, last, logDirectory, stop.Token,
                                    (count, state, pid, status) => {
                                        int added = count - verified;
                                        verified = count;
                                        Report(slot, added, state, pid, status);
                                    }, monitor, cpu, slot).ConfigureAwait(false);
                                Interlocked.Increment(ref launched);
                                first = last + 1;
                            }
                        }
                        Report(slot, 0, GrammInstanceState.Completed);
                    }
                    catch (Exception ex)
                    {
                        cpu.Unregister(slot);
                        Report(slot, 0, ex is OperationCanceledException ? GrammInstanceState.Paused : GrammInstanceState.Failed);
                        lock (gate)
                        {
                            if (failure == null && !(ex is OperationCanceledException && stop.IsCancellationRequested))
                                failure = ExceptionDispatchInfo.Capture(ex);
                        }
                        stop.Cancel();
                        throw;
                    }
                }

                void Report(int slot, int count, GrammInstanceState state, int pid = 0, GrammConsoleStatus status = null)
                {
                    lock (gate)
                    {
                        completed += count;
                        tracker.Update(slot, count, state, pid, status);
                        for (int id = 1; id <= requested; id++) tracker.SetCpuAssignment(id, cpu.Describe(id));
                        if (count > 0) progress?.Invoke(completed, plan.PendingCount);
                        instanceProgress?.Invoke(tracker.Snapshot());
                    }
                }

                var workers = new List<Task>();
                for (int i = 0; i < assignments.Count; i++) workers.Add(Worker(i + 1));
                try
                {
                    await Task.WhenAll(workers).ConfigureAwait(false);
                }
                catch
                {
                    if (failure != null) failure.Throw();
                    cancellationToken.ThrowIfCancellationRequested();
                    throw;
                }
            }
            return new GrammResumeRunResult(completed, launched, logDirectory);
        }

        private static async Task RunRangeAsync(GrammResumePlan plan, string executable, string computationDir,
            int first, int last, string logDirectory, CancellationToken cancellationToken,
            Action<int, GrammInstanceState, int, GrammConsoleStatus> report, GrammConsoleMonitor monitor,
            GrammCpuScheduler cpu, int slot)
        {
            string logPath = Path.Combine(logDirectory, "range_" + first.ToString("D5", CultureInfo.InvariantCulture) +
                "_" + last.ToString("D5", CultureInfo.InvariantCulture) + ".log");
            using (var log = new StreamWriter(logPath, false, new UTF8Encoding(false)) { AutoFlush = true })
            {
                cancellationToken.ThrowIfCancellationRequested();
                DateTime launchedUtc = DateTime.UtcNow;
                log.WriteLine("START utc=" + launchedUtc.ToString("O", CultureInfo.InvariantCulture) + " first=" + first + " last=" + last);
                // GRAMM uses Console.SetCursorPosition and ReadKey. A hidden real console is required;
                // redirecting stdout or using CreateNoWindow can fail inside the unchanged stock core.
                using (var process = StartWithHiddenConsole(executable, computationDir, first, last, p => cpu.Register(slot, p)))
                using (cancellationToken.Register(() => Kill(process)))
                {
                    log.WriteLine("PROCESS pid=" + process.Id);
                    bool completed = false;
                    int nextUnverified = first;
                    report(0, GrammInstanceState.Running, process.Id, null);
                    DateTime? verifiedSinceUtc = null;
                    DateTime nextHeartbeatUtc = launchedUtc;
                    DateTime nextConsoleUtc = launchedUtc;
                    GrammConsoleStatus consoleStatus = null;
                    DateTime? finishedWithoutOutputs = null;
                    try
                    {
                        while (true)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            bool exited = process.HasExited;
                            if (!exited && DateTime.UtcNow >= nextHeartbeatUtc)
                            {
                                try
                                {
                                    process.Refresh();
                                    log.WriteLine("HEARTBEAT utc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture) +
                                        " pid=" + process.Id + " next=" + nextUnverified + " last=" + last +
                                        " cpu_ms=" + process.TotalProcessorTime.TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture) +
                                        " memory_bytes=" + process.WorkingSet64);
                                }
                                catch (InvalidOperationException) { /* The process may exit while metrics are read. */ }
                                catch (Win32Exception) { /* Diagnostic access must not fail a valid calculation. */ }
                                nextHeartbeatUtc = DateTime.UtcNow.AddSeconds(30);
                            }
                            // Each situation is validated once as the engine advances through the range.
                            // Do not repeatedly read earlier large wind fields every polling cycle.
                            while (nextUnverified <= last && IsFreshSituation(plan, computationDir, nextUnverified, launchedUtc))
                                nextUnverified++;
                            bool valid = nextUnverified > last;
                            if (!exited && DateTime.UtcNow >= nextConsoleUtc)
                            {
                                consoleStatus = await monitor.ReadAsync(process.Id, cancellationToken).ConfigureAwait(false);
                                nextConsoleUtc = DateTime.UtcNow.AddSeconds(3);
                                if (consoleStatus.WaitingForInput)
                                {
                                    File.WriteAllText(logPath + ".console.txt", consoleStatus.Text, new UTF8Encoding(false));
                                    throw new InvalidOperationException("GRAMM is waiting for keyboard input after an error. Range " +
                                        first + "-" + last + ". Error log: " + logPath + ".console.txt");
                                }
                            }
                            if (!valid && consoleStatus?.Finished == true)
                            {
                                finishedWithoutOutputs ??= DateTime.UtcNow;
                                if ((DateTime.UtcNow - finishedWithoutOutputs.Value).TotalSeconds >= 2)
                                {
                                    File.WriteAllText(logPath + ".console.txt", consoleStatus.Text, new UTF8Encoding(false));
                                    throw new InvalidOperationException("GRAMM finished the range, but complete results could not be verified: " + first + "-" + last + ". Log: " + logPath);
                                }
                            }
                            report(nextUnverified - first, valid ? GrammInstanceState.Verifying : GrammInstanceState.Running, process.Id, consoleStatus);
                            if (exited)
                            {
                                log.WriteLine("EXIT code=" + process.ExitCode + " valid=" + valid);
                                if (process.ExitCode != 0 || !valid || !IsFreshComplete(plan, computationDir, first, last, launchedUtc))
                                    throw new InvalidOperationException("GRAMM range " + first + "-" + last +
                                        " stopped before verified completion (exit " + process.ExitCode + "). Log: " + logPath);
                                completed = true;
                                break;
                            }
                            if (valid)
                            {
                                if (verifiedSinceUtc == null)
                                {
                                    verifiedSinceUtc = DateTime.UtcNow;
                                    log.WriteLine("OUTPUTS_VALID utc=" + verifiedSinceUtc.Value.ToString("O", CultureInfo.InvariantCulture));
                                }
                                // Revalidate after two seconds, allowing the stock core to finish its final writes
                                // before releasing a slot while it waits for a console key.
                                else if ((DateTime.UtcNow - verifiedSinceUtc.Value).TotalSeconds >= 2)
                                {
                                    log.WriteLine("OUTPUTS_SETTLED action=stop_console_wait");
                                    Kill(process);
                                    await process.WaitForExitAsync().ConfigureAwait(false);
                                    cancellationToken.ThrowIfCancellationRequested();
                                    if (!IsFreshComplete(plan, computationDir, first, last, launchedUtc))
                                        throw new InvalidOperationException("GRAMM range " + first + "-" + last +
                                            " failed final output verification. Log: " + logPath);
                                    completed = true;
                                    break;
                                }
                            }
                            else verifiedSinceUtc = null;
                            await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        Kill(process);
                        await process.WaitForExitAsync().ConfigureAwait(false);
                        cpu.Unregister(slot);
                        // Capture a fully written final situation even when pause arrives between polls.
                        while (nextUnverified <= last && IsFreshSituation(plan, computationDir, nextUnverified, launchedUtc))
                            nextUnverified++;
                        report(nextUnverified - first, completed ? GrammInstanceState.Completed :
                            cancellationToken.IsCancellationRequested ? GrammInstanceState.Paused : GrammInstanceState.Failed, 0, consoleStatus);
                        log.WriteLine((completed ? "COMPLETE" : cancellationToken.IsCancellationRequested ? "CANCELLED" : "FAILED") +
                            " utc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        private static bool IsFreshSituation(GrammResumePlan plan, string directory, int situation, DateTime launchedUtc)
        {
            string prefix = Path.Combine(directory, situation.ToString("D5", CultureInfo.InvariantCulture));
            return File.GetLastWriteTimeUtc(prefix + ".wnd") >= launchedUtc &&
                File.GetLastWriteTimeUtc(prefix + ".scl") >= launchedUtc && plan.IsComplete(situation);
        }

        private static bool IsFreshComplete(GrammResumePlan plan, string directory, int first, int last, DateTime launchedUtc)
        {
            // GRAMM processes the range in order. Probe its last situation first so a long
            // calculation does not reread every already-finished large field once per second.
            string lastPrefix = Path.Combine(directory, last.ToString("D5", CultureInfo.InvariantCulture));
            if (File.GetLastWriteTimeUtc(lastPrefix + ".wnd") < launchedUtc ||
                File.GetLastWriteTimeUtc(lastPrefix + ".scl") < launchedUtc || !plan.IsComplete(last))
                return false;
            for (int situation = first; situation < last; situation++)
            {
                string prefix = Path.Combine(directory, situation.ToString("D5", CultureInfo.InvariantCulture));
                // Original-situation files are written after any Sunrise intermediates. Old final files
                // cannot signal completion while a missing Sunrise intermediate is being regenerated.
                if (File.GetLastWriteTimeUtc(prefix + ".wnd") < launchedUtc ||
                    File.GetLastWriteTimeUtc(prefix + ".scl") < launchedUtc || !plan.IsComplete(situation))
                    return false;
            }
            return true;
        }

        private static Process StartWithHiddenConsole(string executable, string directory, int first, int last, Action<Process> onCreated)
        {
            bool dll = Path.GetExtension(executable).Equals(".dll", StringComparison.OrdinalIgnoreCase);
            var command = new StringBuilder(dll ? "dotnet " + Quote(executable) : Quote(executable));
            command.Append(' ').Append(first.ToString(CultureInfo.InvariantCulture));
            command.Append(' ').Append(last.ToString(CultureInfo.InvariantCulture));
            var startup = new StartupInfo { Size = Marshal.SizeOf<StartupInfo>(), Flags = 0x00000001, ShowWindow = 0 };
            ProcessInformation info;
            if (!CreateProcess(null, command, IntPtr.Zero, IntPtr.Zero, false, 0x00000010 | 0x00000004,
                IntPtr.Zero, directory, ref startup, out info))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not start the GRAMM hidden console.");
            Process process = null;
            try
            {
                // The suspended start prevents a very short-lived process from disappearing before
                // the runner obtains its managed handle, including failures during startup.
                process = Process.GetProcessById((int)info.ProcessId);
                _ = process.Handle;
                onCreated(process);
                if (ResumeThread(info.Thread) == uint.MaxValue)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not resume the GRAMM process.");
                return process;
            }
            catch
            {
                TerminateProcess(info.Process, 1);
                process?.Dispose();
                throw;
            }
            finally
            {
                CloseHandle(info.Thread);
                CloseHandle(info.Process);
            }
        }

        private static string Quote(string value)
        {
            // Windows file paths cannot contain double quotes. Both supported command arguments
            // are absolute file paths, so neither can end in a directory separator.
            return "\"" + value + "\"";
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct StartupInfo
        {
            public int Size;
            public string Reserved;
            public string Desktop;
            public string Title;
            public uint X, Y, XSize, YSize, XCountChars, YCountChars, FillAttribute, Flags;
            public ushort ShowWindow, ReservedSize;
            public IntPtr ReservedBytes, StandardInput, StandardOutput, StandardError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessInformation
        {
            public IntPtr Process, Thread;
            public uint ProcessId, ThreadId;
        }

        [DllImport("kernel32.dll", EntryPoint = "CreateProcessW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateProcess(string applicationName, StringBuilder commandLine, IntPtr processAttributes,
            IntPtr threadAttributes, bool inheritHandles, uint creationFlags, IntPtr environment, string currentDirectory,
            ref StartupInfo startupInfo, out ProcessInformation processInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint ResumeThread(IntPtr thread);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TerminateProcess(IntPtr process, uint exitCode);
        private static void Kill(Process process)
        {
            try { if (!process.HasExited) process.Kill(true); }
            catch (InvalidOperationException) { }
        }
    }

    public sealed class GrammResumeRunResult
    {
        public int CompletedCount { get; }
        public int RangeCount { get; }
        public string LogDirectory { get; }
        public GrammResumeRunResult(int completedCount, int rangeCount, string logDirectory)
        {
            CompletedCount = completedCount;
            RangeCount = rangeCount;
            LogDirectory = logDirectory;
        }
    }
}
#endif
