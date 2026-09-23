using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GralIO;

internal static class Program
{
    static int checks;
    static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; Console.WriteLine("[PASS] " + name); }
    static GrammCpuScheduler.Cpu Cpu(int id, int core, int performance = 0, int cacheMb = 32,
        int scheduling = 0, int flags = 0, int group = 0) => new GrammCpuScheduler.Cpu {
            Id = (uint)(100 + id), Logical = (byte)id, Core = (byte)core, Efficiency = (byte)performance,
            Scheduling = (byte)scheduling, CacheBytes = (uint)(cacheMb * 1048576), Flags = (byte)flags, Group = (ushort)group };
    static GrammCpuScheduler Create(GrammCpuScheduler.Cpu[] cpus, int width,
        List<uint[]> writes, GrammCpuPolicy policy = GrammCpuPolicy.Automatic) =>
        new GrammCpuScheduler(width, 8, policy, null, cpus, (_, ids) => { writes.Add(ids.ToArray()); return true; });

    static async Task<int> Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--child")
        {
            for (;;) { Thread.SpinWait(3000); Console.WriteLine(GetCurrentProcessorNumber()); Thread.Sleep(10); }
        }
        try
        {
            PolicyTests(); ParserTests();
            await NativeTests(args.Length > 0 ? args[0] : null);
            Console.WriteLine("[TOTAL] " + checks + " checks");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    static void PolicyTests()
    {
        using var p1 = new Process(); using var p2 = new Process(); using var p3 = new Process();
        var writes = new List<uint[]>();
        var hybrid = new[] { Cpu(0, 0, 1), Cpu(1, 1, 1), Cpu(2, 2), Cpu(3, 3) };
        using (var scheduler = Create(hybrid, 2, writes))
        {
            scheduler.Register(1, p1); scheduler.Register(2, p2);
            Check(writes[0].SequenceEqual(new uint[] { 100, 101 }), "hybrid high-performance CPUs first");
            Check(writes[1].SequenceEqual(new uint[] { 102, 103 }), "second worker uses remaining CPUs");
            scheduler.Unregister(1);
            Check(writes.Last().SequenceEqual(new uint[] { 100, 101 }), "live lower-tier worker promoted after first worker exits");
            Check(scheduler.Describe(2).Contains("Performance class 1"), "description follows promotion");
            int before = writes.Count;
            scheduler.Register(1, p3);
            Check(writes.Count == before + 1, "running preferred worker remains placed when slot is reused");
            Check(writes.Last().SequenceEqual(new uint[] { 102, 103 }), "new worker fills free lower tier");
        }
        Check(writes.Last().Length == 0, "dispose clears owned placement");

        writes.Clear();
        using (var scheduler = Create(new[] { Cpu(0, 0, 1), Cpu(1, 0, 1), Cpu(2, 1, 1), Cpu(3, 1, 1), Cpu(4, 2) }, 1, writes))
        {
            scheduler.Register(1, p1); scheduler.Register(2, p2);
            Check(writes[0].Single() == 100 && writes[1].Single() == 102, "distinct physical P cores before SMT siblings across workers");
        }
        writes.Clear();
        using (var scheduler = Create(new[] { Cpu(0, 0, scheduling: 10), Cpu(1, 0, scheduling: 10),
            Cpu(2, 1, scheduling: 30), Cpu(3, 1, scheduling: 30) }, 1, writes))
        {
            scheduler.Register(1, p1); scheduler.Register(2, p2);
            Check(writes[0].Single() == 102, "AMD Windows preferred rank first");
            Check(writes[1].Single() == 100, "physical core before preferred SMT sibling");
        }
        writes.Clear();
        using (var scheduler = Create(new[] { Cpu(0, 0, cacheMb: 32, scheduling: 30), Cpu(1, 1, cacheMb: 96, scheduling: 10) }, 1, writes, GrammCpuPolicy.PreferLargeCache))
        {
            scheduler.Register(1, p1);
            Check(writes.Single().Single() == 101, "explicit cache preference precedes OS frequency preference");
            Check(scheduler.Describe(1).Contains("L3 96 MB"), "cache description uses measured size");
        }
        writes.Clear();
        using (var scheduler = Create(new[] { Cpu(0, 0, cacheMb: 32), Cpu(1, 1, cacheMb: 96) }, 1, writes))
        {
            scheduler.Register(1, p1);
            Check(writes.Count == 0, "uniform ranks do not assume larger cache is faster");
        }
        writes.Clear();
        using (var scheduler = Create(hybrid, 2, writes, GrammCpuPolicy.WindowsDefault))
        {
            scheduler.Register(1, p1);
            Check(writes.Count == 0 && scheduler.Describe(1).Contains("selected by user"), "Windows option leaves OS placement unchanged");
        }
        writes.Clear();
        using (var scheduler = Create(hybrid, 2, writes))
        {
            scheduler.Register(1, p1); scheduler.Register(2, p2); scheduler.Register(3, p3);
            Check(writes.Skip(2).All(ids => ids.Length == 0), "oversubscription clears placement instead of crowding preferred cores");
            Check(scheduler.Describe(3).Contains("exceed"), "oversubscription is visible");
            scheduler.Unregister(3);
            Check(writes.TakeLast(2).All(ids => ids.Length == 2), "placement resumes after oversubscription ends");
        }
        writes.Clear();
        using (var scheduler = Create(new[] { Cpu(0, 0, 1, flags: 1), Cpu(1, 1, 1, flags: 2), Cpu(2, 2, 1), Cpu(3, 3) }, 1, writes))
        {
            scheduler.Register(1, p1);
            Check(writes.Single().Single() == 102, "parked and exclusively reserved CPUs excluded");
        }
        writes.Clear();
        using (var scheduler = Create(new[] { Cpu(0, 0, 1), Cpu(1, 1, group: 1) }, 1, writes))
        {
            scheduler.Register(1, p1);
            Check(writes.Count == 0 && scheduler.Describe(1).Contains("multiple processor groups"), "multi-group topology safely uses OS scheduling");
        }
        writes.Clear();
        using (var scheduler = Create(Array.Empty<GrammCpuScheduler.Cpu>(), 1, writes))
        {
            scheduler.Register(1, p1);
            Check(writes.Count == 0 && scheduler.Describe(1).Contains("unavailable"), "unknown topology uses OS scheduling");
        }
        int attempts = 0;
        using (var scheduler = new GrammCpuScheduler(1, 2, GrammCpuPolicy.Automatic, null, hybrid,
            (_, ids) => { attempts++; return attempts != 2; }))
        {
            scheduler.Register(1, p1); scheduler.Register(2, p2);
            Check(attempts == 3, "API failure clears previously placed worker");
            Check(scheduler.Describe(1).Contains("requested"), "API failure fallback is explicit");
        }
        writes.Clear();
        var changing = new[] { Cpu(0, 0, scheduling: 30), Cpu(1, 1, scheduling: 10) };
        using (var scheduler = Create(changing, 1, writes))
        {
            scheduler.Register(1, p1);
            changing[0].Scheduling = 10; changing[1].Scheduling = 30;
            scheduler.Register(2, p2);
            Check(writes[1].Single() == 101, "updated Windows rank moves running worker at scheduling event");
            Check(writes[2].Single() == 100, "rank update keeps CPU sets disjoint");
        }
    }

    static void ParserTests()
    {
        var bytes = new byte[80]; // future-sized record + unrecognized record + standard record
        BitConverter.GetBytes(40).CopyTo(bytes, 0); BitConverter.GetBytes(77).CopyTo(bytes, 8);
        bytes[12] = 2; bytes[14] = 7; bytes[15] = 3; bytes[18] = 1; bytes[20] = 42;
        BitConverter.GetBytes(8).CopyTo(bytes, 40); BitConverter.GetBytes(99).CopyTo(bytes, 44);
        BitConverter.GetBytes(32).CopyTo(bytes, 48); BitConverter.GetBytes(88).CopyTo(bytes, 56);
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            var parsed = GrammCpuScheduler.ParseCpuSets(handle.AddrOfPinnedObject(), bytes.Length);
            Check(parsed.Length == 2 && parsed[0].Id == 77 && parsed[1].Id == 88, "variable-size CPU records and unknown types handled");
            Check(parsed[0].Group == 2 && parsed[0].Logical == 7 && parsed[0].Core == 3 && parsed[0].Efficiency == 1 && parsed[0].Scheduling == 42, "documented CPU field offsets decoded");
            bytes[0] = 0;
            bool rejected = false;
            try { GrammCpuScheduler.ParseCpuSets(handle.AddrOfPinnedObject(), bytes.Length); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected, "invalid record size fails safely");
        }
        finally { handle.Free(); }
    }

    static async Task NativeTests(string directory)
    {
        if (!OperatingSystem.IsWindows()) return;
        var topology = GrammCpuScheduler.ReadTopology();
        Check(topology.Length > 0, "native Windows CPU topology read");
        Console.WriteLine("[HOST] " + string.Join("; ", topology.GroupBy(c => new { c.Efficiency, c.Scheduling, c.CacheBytes, c.Flags }).Select(g =>
            "class=" + g.Key.Efficiency + ",rank=" + g.Key.Scheduling + ",L3=" + g.Key.CacheBytes / 1048576 + ",flags=" + g.Key.Flags + ",CPUs=" + string.Join(",", g.Select(c => c.Logical)))));
        var available = topology.Where(c => c.Available).ToArray();
        Check(available.Length >= 2, "at least two usable logical CPUs for owned-child migration");
        var start = new ProcessStartInfo(Environment.ProcessPath, "--child") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true };
        using var child = Process.Start(start);
        try
        {
            int originalPid = child.Id;
            Check(ReadCpuSets(child).Length == 0, "own child begins without CPU Set restriction");
            Check(GrammCpuScheduler.SetCpuSets(child, new[] { available[0].Id }), "native first CPU placement succeeds");
            Check(ReadCpuSets(child).SequenceEqual(new[] { available[0].Id }), "native process default CPU Set round trip");
            await ObserveCpu(child, available[0].Logical);
            Check(true, "already-running child thread executes on first selected CPU");
            Check(GrammCpuScheduler.SetCpuSets(child, new[] { available[1].Id }), "native in-place CPU migration succeeds");
            await ObserveCpu(child, available[1].Logical);
            Check(child.Id == originalPid && !child.HasExited, "same live child thread executes on new CPU without restart");
            Check(GrammCpuScheduler.SetCpuSets(child, Array.Empty<uint>()) && ReadCpuSets(child).Length == 0, "native CPU Set clear restores OS scheduling");
            using (var scheduler = new GrammCpuScheduler(1, 2, Console.WriteLine, GrammCpuPolicy.PreferLargeCache))
            {
                scheduler.Register(1, child);
                var selected = ReadCpuSets(child);
                if (available.Select(c => c.CacheBytes).Distinct().Count() > 1)
                    Check(selected.Length == 1 && topology.Single(c => c.Id == selected[0]).CacheBytes == available.Max(c => c.CacheBytes), "actual asymmetric-cache host selects large L3 when requested");
                scheduler.Unregister(1);
                Check(ReadCpuSets(child).Length == 0, "scheduler unregister restores own live test child");
            }
            using (var scheduler = new GrammCpuScheduler(1, 2, Console.WriteLine))
            {
                scheduler.Register(1, child);
                var selected = ReadCpuSets(child);
                if (available.Select(c => new { c.Efficiency, c.Scheduling }).Distinct().Count() > 1)
                {
                    byte bestClass = available.Max(c => c.Efficiency);
                    byte bestRank = available.Where(c => c.Efficiency == bestClass).Max(c => c.Scheduling);
                    Check(selected.Length == 1 && topology.Single(c => c.Id == selected[0]).Efficiency == bestClass &&
                        topology.Single(c => c.Id == selected[0]).Scheduling == bestRank, "actual Automatic selects Windows preferred performance rank");
                }
                scheduler.Unregister(1);
                Check(ReadCpuSets(child).Length == 0, "Automatic releases owned CPU placement");
            }
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "cpu_validation.json"), JsonSerializer.Serialize(new {
                    passed = true, checks, processId = originalPid, nativeThreadMigration = true,
                    topology = topology.Select(c => new { c.Id, c.Group, c.Logical, c.Core, c.Efficiency, c.Scheduling, c.CacheBytes, c.Flags })
                }, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        finally { if (!child.HasExited) { child.Kill(); child.WaitForExit(5000); } }
    }

    static async Task ObserveCpu(Process process, uint expected)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        for (;;) if (await process.StandardOutput.ReadLineAsync(timeout.Token) == expected.ToString()) return;
    }
    static uint[] ReadCpuSets(Process process)
    {
        var ids = new uint[256];
        if (!GetProcessDefaultCpuSets(process.Handle, ids, (uint)ids.Length, out uint count)) throw new Exception("GetProcessDefaultCpuSets failed");
        return ids.Take((int)count).ToArray();
    }
    [DllImport("kernel32.dll")] static extern uint GetCurrentProcessorNumber();
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetProcessDefaultCpuSets(IntPtr process, uint[] ids, uint count, out uint required);
}
