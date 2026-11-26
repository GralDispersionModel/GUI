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
        private enum StopRequest { None, Requested, Failed }
        private StopRequest stop;
        private bool startFailed;
        private int lastLoggedMinute;

        internal string ComputationPath { get; }
        internal string DirectoryPath { get; }
        internal string LogPath { get; }
        internal string LogError { get; private set; }
        internal bool ShowProgress { get; set; }
        internal TimeSpan Elapsed => clock.Elapsed;
        internal bool Finished => !clock.IsRunning;
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
            Directory.CreateDirectory(instance.DirectoryPath);
            instances.Add(instance);
            string range = last.HasValue ? $"{first}–{last.Value}" : $"{first} onward (total unknown)";
            string engineLog = explicitRange ? $"Logfile_GRALCore({first}_{last}).txt" : "Logfile_GRALCore.txt";
            Write($"Instance {instance.Number}: weather situations {range}. Engine log: {engineLog}.");
            Write($"Instance {instance.Number} GPU runtime log: {Path.Combine(Path.GetFileName(instance.DirectoryPath), Path.GetFileName(instance.RuntimePath))}");
            Write($"Instance {instance.Number} GPU metrics: {Path.Combine(Path.GetFileName(instance.DirectoryPath), Path.GetFileName(instance.MetricsPath))}");
            return instance;
        }

        internal bool RequestStop()
        {
            if (stop == StopRequest.Requested || Finished) { return false; }
            stop = StopRequest.Requested;
            Write("Stop requested in GUI.");
            return true;
        }

        internal void RetryStopAllowed() => stop = StopRequest.Failed;

        internal void CloseMonitoring()
        {
            Write("The GUI closed and stopped updating this summary. Any running core continues writing its own diagnostics.");
            foreach (var instance in instances) { instance.CloseMonitoring(); }
        }

        internal void StartFailed(Exception error)
        {
            startFailed = true;
            foreach (var instance in instances.Where(instance => instance.Pid == 0)) { instance.StartFailed(error); }
            Write("The requested GRAL launch did not finish. " + error.Message);
        }

        internal void Poll()
        {
            foreach (var instance in instances) { instance.Poll(); }
            if (!Finished && instances.All(instance => instance.Finished))
            {
                clock.Stop();
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
                    if (!startFailed && instances.Count == 0) { return "No GRAL situations started."; }
                    bool stopped = startFailed || stop == StopRequest.Requested || instances.Any(instance => instance.ExitCode != 0);
                    return (stopped ? "GRAL stopped" : "GRAL exited") + $" · GPU {ResultCount}";
                }
                if (stop == StopRequest.Requested) { return "Stop requested…"; }
                if (stop == StopRequest.Failed) { return "Stop not sent. Retry Stop."; }
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

        internal sealed class Instance(GralGpuRun run, int number, int first, int? last)
        {
            private readonly HashSet<int> situations = new();
            private readonly List<byte> pendingMetric = new();
            private long metricsOffset;
            private int exitReadRetries;
            private Func<int?> readExitCode;
            private IDisposable process;

            internal int Number => number;
            internal int Pid { get; private set; }
            internal string DirectoryPath { get; } = Path.Combine(run.DirectoryPath, $"Instance {number}");
            internal string MetricsPath => Path.Combine(DirectoryPath, "GRAL_GPU_metrics.jsonl");
            internal string RuntimePath => Path.Combine(DirectoryPath, "GRAL_GPU_runtime.log");
            internal string ExitPath => Path.Combine(DirectoryPath, "exit.txt");
            internal bool RuntimeSeen { get; private set; }
            internal int Completed => situations.Count;
            internal int? Planned => last - first + 1;
            internal bool Finished { get; private set; }
            internal int? ExitCode { get; private set; }
            internal bool IsRunning => !Finished && readExitCode != null && !readExitCode().HasValue;

            internal void Started(int pid, IDisposable process, Func<int?> readExitCode)
            {
                Pid = pid;
                this.process = process;
                this.readExitCode = readExitCode;
                Log($"launched (PID {pid}).");
            }

            internal void Log(string message) => run.Write($"Instance {Number}: {message}");

            internal void CloseMonitoring()
            {
                readExitCode = null;
                process?.Dispose();
            }

            internal void StartFailed(Exception error)
            {
                Finished = true;
                Log($"could not start GRAL. {error.Message}");
            }

            internal void Poll()
            {
                if (Finished || readExitCode == null) { return; }
                if (!RuntimeSeen && File.Exists(RuntimePath))
                {
                    RuntimeSeen = true;
                    Log("GPU runtime log opened. The GPU reports progress when a situation returns its result.");
                }
                int? exitCode = readExitCode();
                try
                {
                    if (!ReadMetrics()) { return; }
                }
                catch (FileNotFoundException) { } // Engines without metrics still have a process exit status.
                catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
                {
                    if (!exitCode.HasValue || ++exitReadRetries < 5) { return; }
                    Log($"the final GPU metrics could not be read. The count may be incomplete. {error.Message}");
                }
                if (!exitCode.HasValue) { return; }

                Finished = true;
                ExitCode = exitCode;
                CloseMonitoring();
                Log(exitCode == int.MinValue
                    ? "the console closed before GRAL reported an exit code."
                    : $"GRAL exited with code {exitCode}; GPU results recorded: {Completed}.");
            }

            // Read only new, newline-terminated records. An incomplete write is carried into the next poll.
            private bool ReadMetrics()
            {
                const int maxRecordBytes = 1024 * 1024;
                using var stream = new FileStream(MetricsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                if (metricsOffset >= stream.Length) { return true; }
                stream.Position = metricsOffset;
                var buffer = new byte[(int)Math.Min(stream.Length - metricsOffset, 64 * 1024)];
                int read = stream.Read(buffer, 0, buffer.Length);
                foreach (byte value in buffer.AsSpan(0, read))
                {
                    if (value == (byte)'\n')
                    {
                        if (pendingMetric.Count <= maxRecordBytes) { RecordMetric(); }
                        pendingMetric.Clear();
                    }
                    else if (pendingMetric.Count <= maxRecordBytes)
                    {
                        pendingMetric.Add(value); // One extra byte marks an oversized record until its newline.
                    }
                }
                metricsOffset += read;
                return metricsOffset >= stream.Length;
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
                        Log($"GPU result received for weather situation {situation}.");
                    }
                }
                catch (JsonException) { }
            }
        }
    }
}
