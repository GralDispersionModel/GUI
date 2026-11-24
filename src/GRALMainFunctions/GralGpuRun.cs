using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Gral
{
    internal sealed class GralGpuRun
    {
        private readonly Stopwatch clock = Stopwatch.StartNew();
        private readonly List<Instance> instances = new();
        private bool stopRequested;
        private bool stopNotDelivered;
        private bool startFailed;
        private TimeSpan? stoppedAt;
        private int lastLoggedMinute;

        internal string ComputationPath { get; }
        internal string DirectoryPath { get; }
        internal string LogPath { get; }
        internal string LogError { get; private set; }
        internal bool ShowProgress { get; set; }
        internal TimeSpan Elapsed => stoppedAt ?? clock.Elapsed;
        internal bool Finished => stoppedAt.HasValue;
        internal bool HasLaunched => instances.Any(instance => instance.Pid > 0);
        internal IEnumerable<Instance> RunningInstances => instances.Where(instance => instance.IsRunning);
        internal int Completed => instances.Sum(instance => instance.Completed);
        internal int? Planned => instances.Any(instance => !instance.Planned.HasValue)
            ? null : instances.Sum(instance => instance.Planned.Value);
        private string ResultCount => Planned is int planned ? $"{Completed}/{planned}" : Completed.ToString(CultureInfo.InvariantCulture);

        internal GralGpuRun(string computation, string executable, string deviceCheck)
        {
            ComputationPath = Path.GetFullPath(computation).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string name = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + "-" + Guid.NewGuid().ToString("N");
            DirectoryPath = Path.Combine(ComputationPath, "GRAL_GPU_runs", name);
            Directory.CreateDirectory(DirectoryPath);
            LogPath = Path.Combine(DirectoryPath, "Run log.txt");
            File.WriteAllLines(LogPath, new[] {
                "GRAL GPU run",
                "Started: " + DateTimeOffset.Now.ToString("o"),
                "Computation folder: " + ComputationPath,
                "GRAL executable: " + executable,
                "GPU readiness check: " + deviceCheck,
                "Each instance folder contains GRAL's GPU diagnostics when the engine writes them.",
                "The progress count records GPU results. GRAL may still be writing output or waiting for a key in its console.",
                "The GUI updates this summary while open. The GRAL core writes its own logs as it runs.",
                "For engine warnings and input details, also check Logfile_GRALCore*.txt and Problemreport_GRAL.txt in the Computation folder."
            });
        }

        internal Instance AddInstance(int first, int? last, bool explicitRange)
        {
            var instance = new Instance(this, instances.Count + 1, first, last);
            instances.Add(instance);
            string range = last.HasValue ? $"{first}–{last.Value}" : $"{first} onward (total unknown)";
            string engineLog = explicitRange ? $"Logfile_GRALCore({first}_{last}).txt" : "Logfile_GRALCore.txt";
            Write($"Instance {instance.Number}: weather situations {range}. Engine log: {engineLog}.");
            Write($"Instance {instance.Number} GPU runtime log: {Path.GetRelativePath(DirectoryPath, instance.RuntimePath)}");
            Write($"Instance {instance.Number} GPU metrics: {Path.GetRelativePath(DirectoryPath, instance.MetricsPath)}");
            return instance;
        }

        internal bool RequestStop()
        {
            if (stopRequested || Finished) { return false; }
            stopRequested = true;
            stopNotDelivered = false;
            Write("Stop requested in GUI.");
            return true;
        }

        internal void RetryStopAllowed()
        {
            stopRequested = false;
            stopNotDelivered = true;
        }

        internal void CloseMonitoring()
        {
            Write("The GUI closed and stopped updating this summary. Any running core continues writing its own diagnostics.");
            foreach (var instance in instances) { instance.CloseMonitoring(); }
        }

        internal void StartFailed(Exception error)
        {
            startFailed = true;
            Write("The requested GRAL launch did not finish. " + error.Message);
        }

        internal void Poll()
        {
            foreach (var instance in instances) { instance.Poll(); }
            if (!Finished && instances.All(instance => instance.Finished))
            {
                stoppedAt = clock.Elapsed;
                Write(Status);
            }
            int elapsedMinutes = (int)Elapsed.TotalMinutes;
            if (!Finished && elapsedMinutes > lastLoggedMinute)
            {
                lastLoggedMinute = elapsedMinutes;
                Write($"Elapsed: {elapsedMinutes} min. {Status}");
            }
        }

        internal string Status
        {
            get
            {
                if (Finished)
                {
                    if (startFailed || instances.Any(instance => instance.ExitCode != 0))
                    {
                        return $"GRAL stopped · GPU {ResultCount}";
                    }
                    if (instances.Count == 0) { return "No GRAL situations started."; }
                    return (stopRequested ? "GRAL stopped" : "GRAL exited") + $" · GPU {ResultCount}";
                }
                if (stopRequested) { return "Stop requested…"; }
                if (stopNotDelivered) { return "Stop not sent. Retry Stop."; }
                if (Completed == 0)
                {
                    return instances.Any(instance => instance.RuntimeSeen)
                        ? "GPU results pending…" : "Preparing or running GRAL…";
                }
                return $"GPU results: {ResultCount}";
            }
        }

        private void Write(string message)
        {
            try { File.AppendAllText(LogPath, $"[{DateTimeOffset.Now:HH:mm:ss zzz}] {message}{Environment.NewLine}"); }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException) { LogError = error.Message; }
        }

        internal sealed class Instance
        {
            private readonly GralGpuRun run;
            private readonly int first;
            private readonly int? last;
            private readonly HashSet<int> situations = new();
            private readonly List<byte> pendingMetric = new();
            private long metricsOffset;
            private bool discardingMetric;
            private string metricsReadError;
            private int exitReadRetries;
            private Func<int?> readExitCode;
            private IDisposable process;

            internal int Number { get; }
            internal int Pid { get; private set; }
            internal string MetricsPath { get; }
            internal string RuntimePath { get; }
            internal string ExitPath { get; }
            internal bool RuntimeSeen { get; private set; }
            internal int Completed => situations.Count;
            internal int? Planned => last - first + 1;
            internal bool Finished { get; private set; }
            internal int? ExitCode { get; private set; }
            internal bool IsRunning => !Finished && readExitCode != null && !readExitCode().HasValue;

            internal Instance(GralGpuRun run, int number, int first, int? last)
            {
                this.run = run;
                this.first = first;
                this.last = last;
                Number = number;
                string directory = Path.Combine(run.DirectoryPath, $"Instance {number}");
                Directory.CreateDirectory(directory);
                MetricsPath = Path.Combine(directory, "GRAL_GPU_metrics.jsonl");
                RuntimePath = Path.Combine(directory, "GRAL_GPU_runtime.log");
                ExitPath = Path.Combine(directory, "exit.txt");
            }

            internal void Started(int pid, IDisposable process, Func<int?> readExitCode)
            {
                Pid = pid;
                this.process = process;
                this.readExitCode = readExitCode;
                run.Write($"Instance {Number}: launched (PID {pid}).");
            }

            internal void RecordStopDelivery(string message)
            {
                run.Write($"Instance {Number}: {message}");
            }

            internal void CloseMonitoring()
            {
                if (!Finished && process != null)
                {
                    readExitCode = null;
                    process.Dispose();
                }
            }

            internal void StartFailed(Exception error)
            {
                Finished = true;
                run.Write($"Instance {Number}: could not start GRAL. {error.Message}");
            }

            internal void Poll()
            {
                if (Finished || readExitCode == null) { return; }
                if (!RuntimeSeen && File.Exists(RuntimePath))
                {
                    RuntimeSeen = true;
                    run.Write($"Instance {Number}: GPU runtime log opened. The GPU reports progress when a situation returns its result.");
                }
                int? exitCode = readExitCode();
                if (!ReadMetrics() || !exitCode.HasValue) { return; }
                if (metricsReadError != null)
                {
                    if (++exitReadRetries < 5) { return; }
                    run.Write($"Instance {Number}: the final GPU metrics could not be read. The count may be incomplete. {metricsReadError}");
                }

                Finished = true;
                ExitCode = exitCode;
                process.Dispose();
                run.Write(exitCode == int.MinValue
                    ? $"Instance {Number}: the console closed before GRAL reported an exit code."
                    : $"Instance {Number}: GRAL exited with code {exitCode}; GPU results recorded: {Completed}.");
            }

            // Read only new, newline-terminated records. An incomplete write is carried into the next poll.
            private bool ReadMetrics()
            {
                metricsReadError = null;
                try
                {
                    using var stream = new FileStream(MetricsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    if (metricsOffset >= stream.Length) { return true; }
                    stream.Position = metricsOffset;
                    var buffer = new byte[(int)Math.Min(stream.Length - metricsOffset, 64 * 1024)];
                    int length = stream.Read(buffer, 0, buffer.Length);
                    metricsOffset += length;
                    for (int i = 0; i < length; i++)
                    {
                        if (buffer[i] == (byte)'\n')
                        {
                            if (!discardingMetric) { RecordMetric(); }
                            discardingMetric = false;
                            pendingMetric.Clear();
                        }
                        else if (!discardingMetric)
                        {
                            if (pendingMetric.Count == 1024 * 1024)
                            {
                                discardingMetric = true;
                                pendingMetric.Clear();
                            }
                            else { pendingMetric.Add(buffer[i]); }
                        }
                    }
                    return metricsOffset >= stream.Length;
                }
                catch (FileNotFoundException) { return true; }
                catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
                {
                    metricsReadError = error.Message;
                    return true;
                }
            }

            private void RecordMetric()
            {
                try
                {
                    using var record = JsonDocument.Parse(pendingMetric.ToArray());
                    JsonElement root = record.RootElement;
                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("situation", out var value) && value.ValueKind == JsonValueKind.Number &&
                        value.TryGetInt32(out int situation) && situation >= first && (!last.HasValue || situation <= last.Value) && situations.Add(situation))
                    {
                        run.Write($"Instance {Number}: GPU result received for weather situation {situation}.");
                    }
                }
                catch (JsonException) { }
            }
        }
    }
}
