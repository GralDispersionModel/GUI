#if !__MonoCS__
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GralIO
{
    public sealed class GrammConsoleStatus
    {
        public bool Available { get; set; }
        public string Text { get; set; } = "";
        public string Error { get; set; } = "";
        public int Situation { get; set; }
        public double SimulationSeconds { get; set; }
        public double TargetSeconds { get; set; }
        public bool WaitingForInput { get; set; }
        public bool Finished { get; set; }
        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
        public static GrammConsoleStatus Parse(string text)
        {
            var status = new GrammConsoleStatus { Available = true, Text = text };
            var rows = Regex.Matches(text, @"WEATHER-SIT\.\s+TIME\[s\][^\r\n]*\r?\n\s*(\d+)(?:/\d+)?\s+([\d.,]+)\s+[\d.,]+\s+([\d.,]+)");
            if (rows.Count > 0)
            {
                var row = rows[rows.Count - 1];
                if (int.TryParse(row.Groups[1].Value, out int situation) &&
                    double.TryParse(row.Groups[2].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double time) &&
                    double.TryParse(row.Groups[3].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double target) && target > 0)
                { status.Situation = situation; status.SimulationSeconds = time; status.TargetSeconds = target; }
            }
            status.Finished = text.Contains("GRAMM simulations", StringComparison.OrdinalIgnoreCase) &&
                text.Contains("finished. Press any key", StringComparison.OrdinalIgnoreCase);
            status.WaitingForInput = !status.Finished && (
                text.Contains("Execution stopped", StringComparison.OrdinalIgnoreCase) && text.Contains("press ESC", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Press any key to continue", StringComparison.OrdinalIgnoreCase));
            return status;
        }
    }

    // One lightweight helper reads all owned consoles. The GUI never attaches to a console,
    // and GRAMM keeps its real screen buffer and normal input/output handles.
    public sealed class GrammConsoleMonitor : IDisposable
    {
        readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);
        Process host;
        bool unavailable;
        public async Task<GrammConsoleStatus> ReadAsync(int pid, CancellationToken token)
        {
            await gate.WaitAsync(token).ConfigureAwait(false);
            try
            {
                if (unavailable) return new GrammConsoleStatus { Error = "Console monitor is unavailable." };
                if (host == null)
                {
                    string assembly = Assembly.GetEntryAssembly().Location;
                    string executable = Path.ChangeExtension(assembly, ".exe");
                    var start = new ProcessStartInfo { FileName = File.Exists(executable) ? executable : "dotnet",
                        UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true,
                        RedirectStandardOutput = true, RedirectStandardError = true,
                        StandardInputEncoding = new UTF8Encoding(false), StandardOutputEncoding = Encoding.UTF8 };
                    if (!File.Exists(executable)) start.ArgumentList.Add(assembly);
                    start.ArgumentList.Add("--gramm-console-monitor");
                    host = Process.Start(start);
                    host.ErrorDataReceived += (sender, e) => { };
                    host.BeginErrorReadLine();
                }
                await host.StandardInput.WriteLineAsync(pid.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
                await host.StandardInput.FlushAsync().ConfigureAwait(false);
                string line = await host.StandardOutput.ReadLineAsync(token).AsTask().WaitAsync(TimeSpan.FromSeconds(3), token).ConfigureAwait(false);
                if (line == null) throw new IOException("Console monitor stopped.");
                return JsonSerializer.Deserialize<GrammConsoleStatus>(line) ?? new GrammConsoleStatus { Error = "Empty console response." };
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
            catch (Exception ex) when (ex is IOException || ex is InvalidOperationException || ex is TimeoutException ||
                ex is JsonException || ex is System.ComponentModel.Win32Exception)
            {
                unavailable = true;
                StopHost();
                return new GrammConsoleStatus { Error = ex.Message };
            }
            finally { gate.Release(); }
        }
        void StopHost()
        {
            if (host == null) return;
            try { if (!host.HasExited) { host.Kill(true); host.WaitForExit(3000); } }
            catch (InvalidOperationException) { }
            host.Dispose(); host = null;
        }
        public void Dispose() { StopHost(); gate.Dispose(); }

        public static void RunHost()
        {
            // Keep the pipe streams before AttachConsole changes the standard handle table.
            using var reader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);
            using var writer = new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false)) { AutoFlush = true };
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                GrammConsoleStatus status;
                if (!int.TryParse(line, NumberStyles.None, CultureInfo.InvariantCulture, out int pid) || pid < 1)
                    status = new GrammConsoleStatus { Error = "Invalid process id." };
                else status = ReadConsole(pid);
                writer.WriteLine(JsonSerializer.Serialize(status));
            }
        }
        static GrammConsoleStatus ReadConsole(int pid)
        {
            IntPtr output = new IntPtr(-1);
            bool attached = false;
            try
            {
                FreeConsole();
                attached = AttachConsole((uint)pid);
                if (!attached) { int error = Marshal.GetLastWin32Error(); return new GrammConsoleStatus { Error = "AttachConsole: " + error }; }
                output = CreateFile("CONOUT$", 0x80000000, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
                if (output == new IntPtr(-1) || !GetConsoleScreenBufferInfo(output, out ConsoleInfo info))
                    return new GrammConsoleStatus { Error = "Console buffer: " + Marshal.GetLastWin32Error() };
                short first = (short)Math.Max(0, info.Cursor.Y - 80);
                int length = Math.Min(65536, Math.Min(info.Size.Y - first, 90) * info.Size.X);
                var buffer = new StringBuilder(length + 1);
                if (!ReadConsoleOutputCharacter(output, buffer, (uint)length, new Coord { X = 0, Y = first }, out uint count))
                    return new GrammConsoleStatus { Error = "Console read: " + Marshal.GetLastWin32Error() };
                string raw = buffer.ToString();
                var text = new StringBuilder();
                for (int offset = 0; offset < raw.Length; offset += info.Size.X)
                    text.AppendLine(raw.Substring(offset, Math.Min(info.Size.X, raw.Length - offset)).TrimEnd());
                return GrammConsoleStatus.Parse(text.ToString().TrimEnd());
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            { return new GrammConsoleStatus { Error = ex.Message }; }
            finally
            {
                if (output != new IntPtr(-1)) CloseHandle(output);
                if (attached) FreeConsole();
            }
        }
        [StructLayout(LayoutKind.Sequential)] struct Coord { public short X, Y; }
        [StructLayout(LayoutKind.Sequential)] struct SmallRect { public short Left, Top, Right, Bottom; }
        [StructLayout(LayoutKind.Sequential)] struct ConsoleInfo
        { public Coord Size, Cursor; public ushort Attributes; public SmallRect Window; public Coord MaxSize; }
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool AttachConsole(uint pid);
        [DllImport("kernel32.dll")] static extern bool FreeConsole();
        [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr CreateFile(string name, uint access, uint share, IntPtr security, uint disposition, uint flags, IntPtr template);
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool GetConsoleScreenBufferInfo(IntPtr output, out ConsoleInfo info);
        [DllImport("kernel32.dll", EntryPoint = "ReadConsoleOutputCharacterW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool ReadConsoleOutputCharacter(IntPtr output, StringBuilder buffer, uint length, Coord position, out uint read);
    }
}
#endif
