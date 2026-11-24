#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Gral
{
    internal sealed class WindowsRunProcesses : IDisposable
    {
        private readonly List<(string Directory, Process Worker)> workers = new();

        // The caller keeps the CreateProcess handle alive until this method has acquired its own handle.
        internal bool Track(string computation, int pid)
        {
            DiscardExited();
            if (pid <= 0) { return false; }
            Process worker;
            try { worker = Process.GetProcessById(pid); }
            catch (ArgumentException) { return false; }
            try
            {
                _ = worker.SafeHandle;
                workers.Add((Path.TrimEndingDirectorySeparator(Path.GetFullPath(computation)), worker));
                return true;
            }
            catch { worker.Dispose(); throw; }
        }

        internal Process[] InProject(string computation)
        {
            DiscardExited();
            string path = Path.TrimEndingDirectorySeparator(Path.GetFullPath(computation));
            return workers.Where(w => string.Equals(w.Directory, path, StringComparison.OrdinalIgnoreCase))
                .Select(w => w.Worker).ToArray();
        }

        internal void Forget(Process process)
        {
            if (workers.RemoveAll(w => ReferenceEquals(w.Worker, process)) > 0) { process.Dispose(); }
        }

        private void DiscardExited()
        {
            for (int i = workers.Count - 1; i >= 0; i--)
            {
                if (!workers[i].Worker.HasExited) { continue; }
                workers[i].Worker.Dispose();
                workers.RemoveAt(i);
            }
        }

        public void Dispose()
        {
            foreach (var w in workers) { w.Worker.Dispose(); }
            workers.Clear();
        }
    }
}
#endif
