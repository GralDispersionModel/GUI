#if __MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gral
{
    internal static class UnixTerminal
    {
        // The only background job in this shell is the engine. Bash job %1 cannot refer to a recycled PID.
        private const string Run = """
            state=$1; report=$2; shift 2
            mkfifo -m 600 "$state/control" && exec 3<>"$state/control" || exit 1
            program=$(command -v -- "$1") || exit 127
            [[ -x "$program" ]] || exit 126
            "$@" 3>&- </dev/tty &
            engine=$!
            if ! { printf '%s\n' "$engine" > "$state/pid.tmp" && mv "$state/pid.tmp" "$state/pid"; }; then
                kill -KILL %1 2>/dev/null; wait "$engine"; exit 1
            fi
            if ! IFS= read -r -t 12 decision <&3 || [[ $decision != accept ]]; then
                kill -KILL %1 2>/dev/null; wait "$engine"; exit 125
            fi
            while [[ -d "$state" ]] && kill -0 "$engine" 2>/dev/null; do
                if IFS= read -r -t 0.25 message <&3 && [[ $message == stop && $(jobs -p %1) == "$engine" ]]; then
                    kill -KILL %1 2>/dev/null
                fi
            done
            wait "$engine"; code=$?
            if [[ -n "$report" ]]; then printf '%s\n' "$code" > "$report"; fi
            if [[ -d "$state" ]]; then printf '%s\n' "$code" > "$state/exit.tmp" && mv "$state/exit.tmp" "$state/exit"; fi
            exec 3>&-
            exec /bin/bash -i
            """;

        [DllImport("libc", SetLastError = true)]
        private static extern IntPtr mkdtemp(StringBuilder template);

        [DllImport("libc", SetLastError = true)]
        private static extern int access(string path, int mode);

        internal static void AppendEngineCommand(List<string> arguments, string executable)
        {
            string extension = Path.GetExtension(executable);
            if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase)) { arguments.Add("dotnet"); }
            else if (extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)) { arguments.Add("mono"); }
            arguments.Add(executable);
        }

        internal static Engine StartEngine(string executable, string computation, IEnumerable<string> engineArguments,
            IDictionary<string, string> environment = null, string exitReport = null)
        {
            if (!File.Exists(executable)) { throw new FileNotFoundException("Engine not found.", executable); }
            string extension = Path.GetExtension(executable);
            if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) || extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                try { AssemblyName.GetAssemblyName(executable); }
                catch (BadImageFormatException error) { throw new IOException("Select a managed engine assembly or a native Linux executable.", error); }
            }
            else if (access(executable, 1) != 0) { throw new IOException("The selected engine does not have Linux execute permission: " + executable); }
            var template = new StringBuilder(Path.Combine(Path.GetTempPath(), "gral-terminal-XXXXXX"));
            if (mkdtemp(template) == IntPtr.Zero) { throw new IOException("Cannot create a private terminal status directory."); }
            var engine = new Engine(Path.GetFullPath(computation), template.ToString());
            try
            {
                var command = new List<string> { "--wait", "--working-directory", engine.ComputationPath,
                    "--", "/bin/bash", "-c", Run.Replace("\r\n", "\n"), "gral-terminal", engine.StateDirectory, exitReport ?? "" };
                AppendEngineCommand(command, Path.GetFullPath(executable));
                command.AddRange(engineArguments);
                var start = new ProcessStartInfo("gnome-terminal", MonoCompatibility.QuoteArguments(command))
                {
                    UseShellExecute = false,
                    WorkingDirectory = engine.ComputationPath
                };
                if (environment != null)
                {
                    foreach (var entry in environment) { start.EnvironmentVariables[entry.Key] = entry.Value; }
                }
                engine.Start(start);
                return engine;
            }
            catch
            {
                engine.AbortStartup();
                throw;
            }
        }

        internal sealed class Engine(string computation, string stateDirectory) : IDisposable
        {
            private Process terminal;
            private int? exitCode;
            internal string ComputationPath { get; } = computation;
            internal string StateDirectory { get; } = stateDirectory;
            internal int Pid { get; private set; }
            internal int? ExitCode
            {
                get
                {
                    if (!exitCode.HasValue && terminal != null)
                    {
                        exitCode = ReadStatus("exit");
                        if (!exitCode.HasValue && terminal.HasExited) { exitCode = int.MinValue; }
                    }
                    return exitCode;
                }
            }

            internal void Start(ProcessStartInfo start)
            {
                terminal = Process.Start(start);
                var elapsed = Stopwatch.StartNew();
                while (elapsed.Elapsed < TimeSpan.FromSeconds(10))
                {
                    int? pid = ReadStatus("pid");
                    if (pid > 0)
                    {
                        Pid = pid.Value;
                        if (ExitCode is int code && code != 0) { throw new IOException("The engine exited during startup (code " + code + ")."); }
                        SendControl("accept");
                        return;
                    }
                    if (terminal.WaitForExit(25)) { throw new IOException("The terminal could not start the engine (code " + terminal.ExitCode + ")."); }
                }
                throw new IOException("The terminal did not report an engine PID within 10 seconds.");
            }

            private int? ReadStatus(string name)
            {
                string path = Path.Combine(StateDirectory, name);
                try { return int.Parse(File.ReadAllText(path), CultureInfo.InvariantCulture); }
                catch (FileNotFoundException) { return null; }
            }

            private void SendControl(string command)
            {
                using var control = new FileStream(Path.Combine(StateDirectory, "control"), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                byte[] request = Encoding.ASCII.GetBytes(command + "\n");
                control.Write(request, 0, request.Length);
            }

            internal void AbortStartup()
            {
                try { if (File.Exists(Path.Combine(StateDirectory, "control"))) { SendControl("abort"); } }
                finally { Dispose(); }
            }

            internal async Task KillAsync()
            {
                if (ExitCode.HasValue) { return; }
                SendControl("stop");
                var elapsed = Stopwatch.StartNew();
                while (!ExitCode.HasValue && elapsed.Elapsed < TimeSpan.FromSeconds(5)) { await Task.Delay(25); }
                if (!ExitCode.HasValue) { throw new IOException("The engine did not confirm the pause request."); }
            }

            public void Dispose()
            {
                if (terminal != null)
                {
                    _ = ExitCode;
                    terminal.Dispose();
                    terminal = null;
                }
                if (Directory.Exists(StateDirectory)) { Directory.Delete(StateDirectory, true); }
            }
        }
    }
}
#endif
