#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace GralIO
{
    public enum GrammCpuPolicy { Automatic, PreferLargeCache, WindowsDefault }

    /// <summary>Optional soft CPU placement for owned GRAMM processes; never changes engine threads.</summary>
    public sealed class GrammCpuScheduler : IDisposable
    {
        internal sealed class Cpu
        {
            public uint Id;
            public ushort Group;
            public byte Logical, Core, Efficiency, Scheduling, Flags;
            public uint CacheBytes;
            public bool Available => (Flags & 3) == 0; // Exclude parked and exclusively allocated sets.
        }

        private sealed class Assignment
        {
            public int Slot;
            public Process Process;
            public uint[] Ids = Array.Empty<uint>();
            public string Description = "Windows scheduling";
        }

        private readonly object gate = new object();
        private readonly List<Assignment> active = new List<Assignment>();
        private Cpu[] cpus;
        private readonly GrammCpuPolicy policy;
        private readonly Func<Cpu[]> refreshTopology;
        private readonly int coresPerInstance, maxInstances;
        private readonly Action<string> log;
        private readonly Func<Process, uint[], bool> apply;
        private bool cacheFirst, apiFailed;
        private string fallback;
        private bool disposed;

        public GrammCpuScheduler(int coresPerInstance, int maxInstances, Action<string> log = null,
            GrammCpuPolicy policy = GrammCpuPolicy.Automatic)
            : this(coresPerInstance, maxInstances, policy, log, ReadTopologySafely(log), SetCpuSets)
        { refreshTopology = () => ReadTopologySafely(log); }

        // Source-linked tests exercise the policy separately from Windows and the calculation engine.
        internal GrammCpuScheduler(int coresPerInstance, int maxInstances, GrammCpuPolicy policy,
            Action<string> log, Cpu[] topology, Func<Process, uint[], bool> apply)
        {
            if (coresPerInstance < 1) throw new ArgumentOutOfRangeException(nameof(coresPerInstance));
            if (maxInstances < 1) throw new ArgumentOutOfRangeException(nameof(maxInstances));
            this.coresPerInstance = coresPerInstance;
            this.maxInstances = maxInstances;
            this.log = log;
            this.apply = apply;
            this.policy = policy;
            ConfigureTopology(topology);
        }

        private void ConfigureTopology(Cpu[] topology)
        {
            fallback = null;
            topology = topology ?? Array.Empty<Cpu>();
            cpus = topology.Where(c => c.Available).ToArray();
            cacheFirst = policy == GrammCpuPolicy.PreferLargeCache && cpus.Length > 0 &&
                cpus.All(c => c.CacheBytes > 0) && cpus.Select(c => c.CacheBytes).Distinct().Count() > 1;
            if (policy == GrammCpuPolicy.WindowsDefault) fallback = "selected by user";
            else if (topology.Length == 0) fallback = "CPU topology unavailable";
            else if (topology.Select(c => c.Group).Distinct().Count() != 1) fallback = "multiple processor groups";
            else if (cpus.Length == 0) fallback = "no unparked, unreserved CPU sets";
            else if (!cacheFirst && cpus.Select(c => new { c.Efficiency, c.Scheduling }).Distinct().Count() < 2)
                fallback = "uniform performance ranks; OS/CPPC preference retained";
            Log("[CPU] " + (fallback != null ? "Windows scheduling: " + fallback :
                (cacheFirst ? "Cache first" : "Performance class first") + "; available logical CPUs=" + cpus.Length));
        }

        public void Register(int slotId, Process process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));
            lock (gate)
            {
                if (disposed) throw new ObjectDisposedException(nameof(GrammCpuScheduler));
                if (active.Any(a => a.Slot == slotId)) throw new InvalidOperationException("CPU slot is already registered.");
                active.Add(new Assignment { Slot = slotId, Process = process });
                Rebalance();
            }
        }

        public void Unregister(int slotId)
        {
            lock (gate)
            {
                var old = active.FirstOrDefault(a => a.Slot == slotId);
                if (old == null) return;
                Restore(old);
                active.Remove(old);
                if (!disposed) Rebalance();
            }
        }

        public string Describe(int slotId)
        {
            lock (gate)
                return active.FirstOrDefault(a => a.Slot == slotId)?.Description ?? "Windows scheduling";
        }

        private long Rank(Cpu cpu)
        {
            bool primary = cpu.Logical == cpus.Where(c => c.Group == cpu.Group && c.Core == cpu.Core).Min(c => c.Logical);
            // AMD GDC 2024, slide 30: SchedulingClass is a runtime Windows preference (higher is better).
            return (cacheFirst ? (long)cpu.CacheBytes << 17 : 0) + ((long)cpu.Efficiency << 9) +
                (primary ? 256 : 0) + cpu.Scheduling;
        }

        private void Rebalance()
        {
            if (!apiFailed && refreshTopology != null) ConfigureTopology(refreshTopology());
            string reason = fallback;
            if (reason == null && (active.Count > maxInstances || (long)active.Count * coresPerInstance > cpus.Length))
                reason = "requested threads exceed available CPU sets";
            if (reason != null)
            {
                foreach (var item in active)
                {
                    Restore(item);
                    item.Description = "Windows scheduling (" + reason + ")";
                }
                if (active.Count > 0) Log("[CPU] " + active[0].Description);
                return;
            }

            var available = new List<Cpu>(cpus);
            // Keep registration order: finishing a preferred worker promotes a still-running worker.
            foreach (var item in active)
            {
                // Within one performance tier use separate physical cores before their SMT siblings.
                var ordered = available.OrderByDescending(Rank).ThenBy(c => c.Group).ThenBy(c => c.Logical).ToArray();
                var selected = ordered.Take(coresPerInstance).ToArray();
                var prior = available.Where(c => item.Ids.Contains(c.Id)).ToArray();
                // Avoid moving a running worker to an equivalent core only to change its CPU number.
                if (prior.Length == coresPerInstance && prior.Select(Rank).OrderByDescending(x => x)
                    .SequenceEqual(selected.Select(Rank).OrderByDescending(x => x))) selected = prior;
                uint[] ids = selected.Select(c => c.Id).OrderBy(id => id).ToArray();
                if (!ids.SequenceEqual(item.Ids))
                {
                    if (!TryApply(item.Process, ids))
                    {
                        apiFailed = true;
                        fallback = "CPU Set API failed";
                        // A failed API request must never abort a calculation or leave other workers pinned.
                        foreach (var registered in active)
                        {
                            Restore(registered);
                            registered.Description = "Windows scheduling requested (CPU Set API failed)";
                        }
                        Log("[CPU] CPU Set API failed; Windows scheduling requested for all owned workers.");
                        return;
                    }
                    item.Ids = ids;
                    item.Description = (cacheFirst ? "L3 " + string.Join("/", selected.Select(c => c.CacheBytes / 1048576).Distinct().OrderByDescending(x => x)) + " MB" :
                        "Performance class " + string.Join("/", selected.Select(c => c.Efficiency).Distinct().OrderByDescending(x => x))) +
                        " | rank " + string.Join("/", selected.Select(c => c.Scheduling).Distinct().OrderByDescending(x => x)) +
                        " | CPU " + string.Join(",", selected.Select(c => c.Logical).OrderBy(x => x));
                    Log("[CPU] Instance " + item.Slot + ": " + item.Description);
                }
                available.RemoveAll(c => ids.Contains(c.Id));
            }
        }

        private bool TryApply(Process process, uint[] ids)
        {
            try { return apply(process, ids); }
            catch (Exception ex) when (ex is Win32Exception || ex is InvalidOperationException ||
                ex is DllNotFoundException || ex is EntryPointNotFoundException || ex is PlatformNotSupportedException)
            { Log("[CPU] " + ex.GetType().Name + ": " + ex.Message); return false; }
        }

        private void Restore(Assignment item)
        {
            if (item.Ids.Length == 0) return;
            if (TryApply(item.Process, Array.Empty<uint>())) item.Ids = Array.Empty<uint>();
            else Log("[CPU] Unable to clear CPU sets for instance " + item.Slot + "; the process may have exited.");
        }

        private void Log(string text) { try { log?.Invoke(text); } catch { /* Diagnostics must not stop GRAMM. */ } }

        public void Dispose()
        {
            lock (gate)
            {
                if (disposed) return;
                foreach (var item in active) Restore(item);
                active.Clear();
                disposed = true;
            }
        }

        private static Cpu[] ReadTopologySafely(Action<string> log)
        {
            try { return ReadTopology(); }
            catch (Exception ex) when (ex is Win32Exception || ex is DllNotFoundException ||
                ex is EntryPointNotFoundException || ex is PlatformNotSupportedException || ex is InvalidOperationException)
            {
                try { log?.Invoke("[CPU] Topology unavailable: " + ex.Message); } catch { }
                return Array.Empty<Cpu>();
            }
        }

        internal static Cpu[] ReadTopology()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !Environment.Is64BitProcess)
                return Array.Empty<Cpu>();
            GetSystemCpuSetInformation(IntPtr.Zero, 0, out uint length, IntPtr.Zero, 0);
            if (length == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
            IntPtr buffer = Marshal.AllocHGlobal(checked((int)length));
            try
            {
                if (!GetSystemCpuSetInformation(buffer, length, out length, IntPtr.Zero, 0))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                var result = ParseCpuSets(buffer, checked((int)length));
                if (result.Select(c => c.Group).Distinct().Count() == 1)
                {
                    using (var current = Process.GetCurrentProcess())
                    {
                        ulong mask = unchecked((ulong)current.ProcessorAffinity.ToInt64());
                        result = result.Where(c => c.Logical < 64 && (mask & (1UL << c.Logical)) != 0).ToArray();
                    }
                    ReadCacheSizes(result);
                }
                return result;
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }

        internal static Cpu[] ParseCpuSets(IntPtr buffer, int length)
        {
            var cpus = new List<Cpu>();
            for (int offset = 0; offset + 8 <= length;)
            {
                IntPtr item = IntPtr.Add(buffer, offset);
                int size = Marshal.ReadInt32(item);
                if (size < 8 || size > length - offset) throw new InvalidOperationException("Invalid CPU topology record.");
                if (Marshal.ReadInt32(item, 4) == 0)
                {
                    if (size < 32) throw new InvalidOperationException("Truncated CPU Set record.");
                    cpus.Add(new Cpu { Id = unchecked((uint)Marshal.ReadInt32(item, 8)), Group = unchecked((ushort)Marshal.ReadInt16(item, 12)),
                        Logical = Marshal.ReadByte(item, 14), Core = Marshal.ReadByte(item, 15),
                        Efficiency = Marshal.ReadByte(item, 18), Flags = Marshal.ReadByte(item, 19), Scheduling = Marshal.ReadByte(item, 20) });
                }
                offset += size;
            }
            return cpus.ToArray();
        }

        private static void ReadCacheSizes(Cpu[] cpus)
        {
            uint length = 0;
            GetLogicalProcessorInformationEx(2, IntPtr.Zero, ref length); // RelationCache
            if (length == 0) return;
            IntPtr buffer = Marshal.AllocHGlobal(checked((int)length));
            try
            {
                if (!GetLogicalProcessorInformationEx(2, buffer, ref length)) return;
                for (int offset = 0; offset + 8 <= length;)
                {
                    IntPtr record = IntPtr.Add(buffer, offset);
                    int size = Marshal.ReadInt32(record, 4);
                    if (size < 8 || size > length - offset) return;
                    // x64 CACHE_RELATIONSHIP starts at +8; GROUP_AFFINITY starts at +40.
                    if (Marshal.ReadInt32(record) == 2 && size >= 56 && Marshal.ReadByte(record, 8) == 3)
                    {
                        uint bytes = unchecked((uint)Marshal.ReadInt32(record, 12));
                        int count = Math.Max(1, (int)unchecked((ushort)Marshal.ReadInt16(record, 38)));
                        for (int group = 0; group < count && 40 + (group + 1) * 16 <= size; group++)
                        {
                            int at = 40 + group * 16;
                            ulong mask = unchecked((ulong)Marshal.ReadInt64(record, at));
                            ushort number = unchecked((ushort)Marshal.ReadInt16(record, at + 8));
                            foreach (var cpu in cpus.Where(c => c.Group == number && c.Logical < 64 && (mask & (1UL << c.Logical)) != 0))
                                cpu.CacheBytes = Math.Max(cpu.CacheBytes, bytes);
                        }
                    }
                    offset += size;
                }
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }

        internal static bool SetCpuSets(Process process, uint[] ids)
        {
            if (process.HasExited) return true;
            if (!SetProcessDefaultCpuSets(process.Handle, ids.Length == 0 ? null : ids, (uint)ids.Length))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return true;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSystemCpuSetInformation(IntPtr information, uint bufferLength,
            out uint returnedLength, IntPtr process, uint flags);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetLogicalProcessorInformationEx(int relationship, IntPtr buffer, ref uint returnedLength);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetProcessDefaultCpuSets(IntPtr process, uint[] cpuSetIds, uint count);
    }
}
#endif
