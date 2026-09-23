using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GralIO;

internal static class Program
{
    private static int checks;
    private static readonly List<string> passed = new List<string>();
    private static string root;
    private static string exe;

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--gramm-console-monitor") { GrammConsoleMonitor.RunHost(); return 0; }
        if (args.Length == 7 && args[0] == "--probe-project") return await ProbeProject(args);
        if (args.Length == 4 && args[0] == "--real-core") return await TestRealCore(args[1], args[2], args[3]);
        if (args.Length == 2 && int.TryParse(args[0], out int first)) return await Child(first, int.Parse(args[1]));
        if (args.Length == 1 && args[0] == "--descendant")
        {
            File.WriteAllText("descendant.pid", Environment.ProcessId.ToString());
            await Task.Delay(Timeout.Infinite);
            return 0;
        }
        root = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.Combine(Path.GetTempPath(), "gramm_runner_" + Guid.NewGuid().ToString("N"));
        exe = Path.ChangeExtension(typeof(Program).Assembly.Location, ".exe");
        Directory.CreateDirectory(root);
        try
        {
            TestConsoleParser();
            await TestConsoleMonitor();
            await TestConsoleWait("input_wait");
            await TestConsoleWait("false_finish");
            await TestLiveInstanceProgress();
            await TestGapsAndPreservation();
            await TestBalancedDisconnectedAssignments();
            await TestCancelAndRestart();
            await TestExit("exit0", false);
            await TestExit("exit7", false);
            await TestExit("valid_exit0", true);
            await TestExit("valid_exit7", false);
            await TestFailFast();
            await TestPartialOutput();
            await TestMissingInitialisation();
            await TestFreshness();
            await TestDll();
            File.WriteAllText(Path.Combine(root, "runner_test_summary.json"),
                System.Text.Json.JsonSerializer.Serialize(new { status = "pass", checks, passed }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("PASS checks=" + checks);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            File.WriteAllText(Path.Combine(root, "runner_test_failure.txt"), ex.ToString());
            return 1;
        }
    }

    private static void TestConsoleParser()
    {
        var status = GrammConsoleStatus.Parse("WEATHER-SIT. TIME[s] TIMESTEP[s] ENDTIME[s] PRESS-ITERATIONS\n 739/1 793.5 2.5 21600 8\n");
        Check(status.Situation==739&&status.SimulationSeconds==793.5&&status.TargetSeconds==21600,"parse actual project console");
        Check(!status.WaitingForInput&&!status.Finished,"normal calculation is not waiting");
        status=GrammConsoleStatus.Parse("WEATHER-SIT. TIME[s] TIMESTEP[s] ENDTIME[s]\n 12 1,5 0,5 10,0\n");
        Check(status.Situation==12&&status.SimulationSeconds==1.5,"decimal comma console");
        Check(GrammConsoleStatus.Parse("File 'IIN.dat' not found - Execution stopped - press ESC to continue").WaitingForInput,"input wait recognized");
        status=GrammConsoleStatus.Parse("GRAMM simulations for weather situations 1 to 2 finished. Press any key to continue...");
        Check(status.Finished&&!status.WaitingForInput,"normal final key wait distinguished");
        passed.Add("console_status_parser");
    }
    private static async Task TestConsoleMonitor()
    {
        string dir=Fixture("console_progress","monitor");
        var snapshots=new List<GrammProgressSnapshot>();
        var result=await new GrammResumeRunner().RunAsync(GrammResumePlan.Create(dir,1,2,1),exe,dir,CancellationToken.None,
            instanceProgress:s=>snapshots.Add(s));
        File.WriteAllText(Path.Combine(dir,"monitor_snapshots.json"),System.Text.Json.JsonSerializer.Serialize(snapshots,new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
        Check(result.CompletedCount==2,"console observation preserves calculation");
        var statuses=snapshots.SelectMany(s=>s.Instances).Select(s=>s.ConsoleStatus).Where(s=>s?.Available==true).ToArray();
        Check(statuses.Any(s=>s.TargetSeconds==10&&s.SimulationSeconds>0),"native monitor sees internal progress");
        Check(statuses.Select(s=>s.Situation).Distinct().Count()>=2,"native monitor follows subsequent weather");
        Check(CountAlive(dir)==0,"monitored core reaped");
        passed.Add("native_console_internal_progress");
    }
    private static async Task TestConsoleWait(string mode)
    {
        string dir=Fixture(mode,mode); bool failed=false;
        using var timeout=new CancellationTokenSource(15000);
        try { await new GrammResumeRunner().RunAsync(GrammResumePlan.Create(dir,1,1,1),exe,dir,timeout.Token); }
        catch(InvalidOperationException) { failed=true; }
        Check(failed,"waiting console fails visibly: "+mode);
        Check(CountAlive(dir)==0,"waiting core reaped: "+mode);
        Check(Directory.GetFiles(dir,"*.console.txt",SearchOption.AllDirectories).Length==1,"console diagnostic retained: "+mode);
        passed.Add(mode);
    }
    private static async Task<int> ProbeProject(string[] args)
    {
        string output=Path.GetFullPath(args[3]);
        if(Directory.Exists(output)) throw new InvalidOperationException("Probe output must be new.");
        Directory.CreateDirectory(output);
        var names=new[]{"GRAMM.geb","GRAMMin.dat","IIN.dat","ggeom.asc","landuse.asc","Max_Proc.txt","meteopgt.all","CustomInit.txt","albeq.dat","geom.in","in.dat","mettimeseries.dat"};
        var before=names.ToDictionary(n=>n,n=>Hash(Path.Combine(args[2],n)));
        foreach(string name in names) File.Copy(Path.Combine(args[2],name),Path.Combine(output,name));
        int first=int.Parse(args[4]),last=int.Parse(args[5]);
        var plan=GrammResumePlan.Create(output,first,last,8,1008);
        var snapshots=new List<GrammProgressSnapshot>();
        using var cancel=new CancellationTokenSource(int.Parse(args[6])*1000);
        try { await new GrammResumeRunner().RunAsync(plan,args[1],output,cancel.Token,instanceProgress:s=>snapshots.Add(s),requestedInstances:8); }
        catch(OperationCanceledException) { }
        var observations=snapshots.SelectMany(s=>s.Instances.Where(i=>i.ConsoleStatus?.TargetSeconds>0).Select(i=>new {
            at=s.CapturedAt,instance=i.Id,pid=i.ProcessId,situation=i.ConsoleStatus.Situation,
            seconds=i.ConsoleStatus.SimulationSeconds,target=i.ConsoleStatus.TargetSeconds})).Distinct().ToArray();
        int observed=observations.Select(s=>s.instance).Distinct().Count();
        bool unchanged=before.All(pair=>pair.Value==Hash(Path.Combine(args[2],pair.Key)));
        bool pass=observed==8&&unchanged&&snapshots.Last().Active==0;
        File.WriteAllText(Path.Combine(output,"probe_summary.json"),System.Text.Json.JsonSerializer.Serialize(new {
            status=pass?"pass":"fail",observed_instances=observed,inputs_unchanged=unchanged,
            scope="Eight native instances observed for a bounded interval; no full production completion.",
            final_active=snapshots.Last().Active,observations},new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine((pass?"PASS":"FAIL")+" project instances="+observed+" original_unchanged="+unchanged);
        return pass?0:1;
    }
    private static async Task<int> TestRealCore(string executable, string fixture, string output)
    {
        output = Path.GetFullPath(output);
        if (Directory.Exists(output)) throw new InvalidOperationException("The real-core test requires a new output directory.");
        Directory.CreateDirectory(output);
        foreach (string name in new[] { "GRAMM.geb", "GRAMMin.dat", "IIN.dat", "ggeom.asc", "landuse.asc", "Max_Proc.txt", "meteopgt.all" })
            File.Copy(Path.Combine(fixture, name), Path.Combine(output, name));
        var timer = Stopwatch.StartNew();
        try
        {
            var coldPlan = GrammResumePlan.Create(output, 1, 3, 2);
            Check(coldPlan.PendingCount == 3, "real cold count");
            var cold = await new GrammResumeRunner().RunAsync(coldPlan, executable, output, CancellationToken.None);
            Check(cold.CompletedCount == 3 && Enumerable.Range(1, 3).All(coldPlan.IsComplete), "real cold completion");
            var retained = new[] { "00001.wnd", "00001.scl", "00003.wnd", "00003.scl" }
                .ToDictionary(name => Path.Combine(output, name), name => (Hash(Path.Combine(output, name)), File.GetLastWriteTimeUtc(Path.Combine(output, name))));
            File.Delete(Path.Combine(output, "00002.wnd"));
            File.Delete(Path.Combine(output, "00002.scl"));
            var resumePlan = GrammResumePlan.Create(output, 1, 3, 2);
            Check(resumePlan.CompletedCount == 2 && resumePlan.PendingCount == 1, "real gap detected");
            var resume = await new GrammResumeRunner().RunAsync(resumePlan, executable, output, CancellationToken.None);
            Check(resume.CompletedCount == 1 && resume.RangeCount == 1 && Enumerable.Range(1, 3).All(resumePlan.IsComplete), "real gap resumed");
            Check(retained.All(pair => pair.Value.Item1 == Hash(pair.Key) && pair.Value.Item2 == File.GetLastWriteTimeUtc(pair.Key)), "real retained byte and time identity");
            File.WriteAllText(Path.Combine(output, "real_runner_summary.json"), System.Text.Json.JsonSerializer.Serialize(new
            {
                status = "pass", checks, elapsed_seconds = timer.Elapsed.TotalSeconds,
                core = Path.GetFullPath(executable), cold_pending = 3, resume_pending = 1,
                cold_log = cold.LogDirectory, resume_log = resume.LogDirectory,
                retained_bytes_and_mtimes_unchanged = true
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("PASS real-core checks=" + checks);
            return 0;
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(output, "real_runner_failure.txt"), ex.ToString());
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
    private static async Task TestLiveInstanceProgress()
    {
        string dir = Fixture("live_progress", "stream");
        var plan = GrammResumePlan.Create(dir, 1, 8, 2);
        var snapshots = new List<GrammProgressSnapshot>();
        var result = await new GrammResumeRunner().RunAsync(plan, exe, dir, CancellationToken.None,
            instanceProgress: s => snapshots.Add(s), requestedInstances: 4);
        Check(result.CompletedCount == 8, "live progress completion");
        Check(snapshots.All(s => s.Instances.Count == 4 && s.Instances.Select(i => i.Id).SequenceEqual(new[] {1,2,3,4})), "stable instance slots");
        Check(snapshots.All(s => s.CompletedThisRun + s.Queued + s.Instances.Sum(i => i.Remaining) == 8), "queued and assigned remaining reconcile");
        Check(snapshots.All(s => s.Active <= 2 && s.Instances.All(i => i.Completed >= 0 && i.Remaining >= 0)), "live bounds");
        Check(snapshots.Any(s => s.Instances.Any(i => i.State == GrammInstanceState.Running && i.Completed > 0 && i.Remaining > 0)), "intermediate progress before range ends");
        Check(snapshots.Select(s => s.Completed).SequenceEqual(snapshots.Select(s => s.Completed).Order()), "overall live progress monotonic");
        Check(snapshots.Last().Remaining == 0 && snapshots.Last().Active == 0 && snapshots.Last().Instances.Count(i => i.State == GrammInstanceState.Unused) == 2, "final and unused states");
        File.WriteAllText(Path.Combine(dir,"snapshots.json"), System.Text.Json.JsonSerializer.Serialize(snapshots));
        passed.Add("per_instance_live_progress_and_remaining");
    }

    private static async Task TestGapsAndPreservation()
    {
        string dir = Fixture("gaps", "normal");
        int[] existing = { 1, 2, 5, 6, 9, 10 };
        foreach (int n in existing) WritePair(dir, n);
        var before = Snapshot(dir);
        var plan = GrammResumePlan.Create(dir, 1, 12, 2);
        var reported = new List<int>();
        int maxAlive = 0;
        var task = new GrammResumeRunner().RunAsync(plan, exe, dir, CancellationToken.None,
            (done, total) => { lock (reported) reported.Add(done); Check(total == 6, "progress total"); });
        while (!task.IsCompleted)
        {
            maxAlive = Math.Max(maxAlive, CountAlive(dir));
            await Task.Delay(50);
        }
        var result = await task;
        Check(result.CompletedCount == 6 && result.RangeCount == 4, "gap completion counts");
        Check(maxAlive == 2, "bounded concurrency reached cap");
        Check(Enumerable.Range(1, 12).All(plan.IsComplete), "gap output completion");
        Check(before.All(pair => pair.Value == Hash(pair.Key)), "existing bytes preserved");
        Check(ReadLaunches(dir).Order().SequenceEqual(new[] { "3-4", "7-7", "8-8", "11-12" }.Order()), "only missing ranges launched");
        Check(reported.Last() == 6 && reported.SequenceEqual(reported.Order()), "progress monotonic");
        Check(CountAlive(dir) == 0, "ReadKey waiting children reaped");
        Check(Directory.GetFiles(result.LogDirectory, "range_*.log").Length == 4, "range logs retained");
        passed.Add("gaps_preserved_bounded_ReadKey");
    }

    private static async Task TestBalancedDisconnectedAssignments()
    {
        string dir = Fixture("balanced_79", "normal");
        int[] missing = Enumerable.Range(739, 24).Concat(Enumerable.Range(859, 31)).Concat(Enumerable.Range(985, 24)).ToArray();
        foreach (int n in Enumerable.Range(1, 1008).Except(missing)) WritePair(dir, n);
        var before = Snapshot(dir);
        var plan = GrammResumePlan.Create(dir, 1, 1008, 8);
        var snapshots = new List<GrammProgressSnapshot>();
        var result = await new GrammResumeRunner().RunAsync(plan, exe, dir, CancellationToken.None,
            instanceProgress: s => snapshots.Add(s), requestedInstances: 8);
        int[] counts = { 10,10,10,10,10,10,10,9 };
        Check(snapshots.All(s => s.Instances.Select(i => i.Assigned).SequenceEqual(counts)), "balanced total assignment stable through gaps");
        Check(snapshots.All(s => s.CompletedThisRun + s.Instances.Sum(i => i.Remaining) == 79), "whole-assignment remaining reconciles");
        Check(snapshots.All(s => s.Instances.All(i => i.State != GrammInstanceState.Completed || i.Remaining == 0)), "no false completion between disconnected ranges");
        for (int id = 1; id <= 8; id++) {
            var countsForSlot = snapshots.Select(s => s.Instances[id - 1].Completed);
            Check(countsForSlot.SequenceEqual(countsForSlot.Order()), "slot completion monotonic " + id);
        }
        Check(result.CompletedCount == 79 && result.RangeCount == 10, "balanced 79 completed across ten native ranges");
        Check(snapshots.Last().Instances.All(i => i.Remaining == 0), "all balanced assignments completed");
        Check(before.All(pair => pair.Value == Hash(pair.Key)), "929 completed results preserved");
        Check(ReadLaunches(dir).Order().SequenceEqual(plan.PendingRanges.Select(r => r.First + "-" + r.Last).Order()), "only planned ranges launched once");
        Check(CountAlive(dir) == 0, "balanced children reaped");
        Check(File.Exists(Path.Combine(result.LogDirectory, "cpu_placement.log")), "CPU placement audit exists");
        File.WriteAllText(Path.Combine(dir, "snapshots.json"), System.Text.Json.JsonSerializer.Serialize(snapshots));
        passed.Add("balanced_noncontiguous_79_integration");
    }

    private static async Task TestCancelAndRestart()
    {
        string dir = Fixture("cancel_restart", "cancel");
        var plan = GrammResumePlan.Create(dir, 1, 8, 2);
        using (var cancel = new CancellationTokenSource())
        {
            var task = new GrammResumeRunner().RunAsync(plan, exe, dir, cancel.Token);
            await Until(() => File.Exists(Path.Combine(dir, "descendant.pid")) && CountAlive(dir) >= 2);
            cancel.Cancel();
            await ExpectCancelled(task);
        }
        Check(CountAlive(dir) == 0, "cancel kills all workers");
        int descendant = int.Parse(File.ReadAllText(Path.Combine(dir, "descendant.pid")));
        Check(!Alive(descendant), "cancel kills descendant tree");
        // Simulate a result that completed just before interruption; a new plan must retain it.
        WritePair(dir, 1);
        string saved = Hash(Path.Combine(dir, "00001.wnd"));
        File.WriteAllText(Path.Combine(dir, "mode.txt"), "normal");
        var restarted = GrammResumePlan.Create(dir, 1, 8, 3);
        Check(restarted.CompletedCount == 1 && restarted.PendingCount == 7, "restart rescans completed files");
        var done = await new GrammResumeRunner().RunAsync(restarted, exe, dir, CancellationToken.None);
        Check(done.CompletedCount == 7 && Enumerable.Range(1, 8).All(restarted.IsComplete), "restart finishes remaining only");
        Check(saved == Hash(Path.Combine(dir, "00001.wnd")), "restart preserves previous result");
        passed.Add("cancel_tree_and_restart");
    }

    private static async Task TestExit(string mode, bool success)
    {
        string dir = Fixture(mode, mode);
        var plan = GrammResumePlan.Create(dir, 1, 2, 1);
        bool actual = true;
        try { await new GrammResumeRunner().RunAsync(plan, exe, dir, CancellationToken.None); }
        catch (InvalidOperationException) { actual = false; }
        Check(actual == success, "exit semantics " + mode);
        Check(CountAlive(dir) == 0, "exit process reaped " + mode);
        passed.Add(mode);
    }

    private static async Task TestFailFast()
    {
        string dir = Fixture("fail_fast", "fail_fast");
        var plan = GrammResumePlan.Create(dir, 1, 4, 2);
        bool failed = false;
        try { await new GrammResumeRunner().RunAsync(plan, exe, dir, CancellationToken.None); }
        catch (InvalidOperationException) { failed = true; }
        Check(failed, "one child failure fails the run");
        Check(ReadLaunches(dir).Length == 2 && CountAlive(dir) == 0, "one child failure terminates peer");
        passed.Add("worker_failure_cancels_peers");
    }
    private static async Task TestPartialOutput()
    {
        string dir = Fixture("partial", "partial");
        var plan = GrammResumePlan.Create(dir, 1, 2, 2);
        using (var cancel = new CancellationTokenSource(2200))
        {
            await ExpectCancelled(new GrammResumeRunner().RunAsync(plan, exe, dir, cancel.Token));
        }
        Check(!plan.IsComplete(1) && !plan.IsComplete(2), "partial output not accepted");
        Check(CountAlive(dir) == 0, "partial workers reaped");
        passed.Add("partial_output_not_complete");
    }

    private static async Task TestMissingInitialisation()
    {
        string dir = Fixture("missing_albeq", "normal");
        File.Delete(Path.Combine(dir, "albeq.dat"));
        var plan = GrammResumePlan.Create(dir, 2, 3, 2);
        bool rejected = false;
        try { await new GrammResumeRunner().RunAsync(plan, exe, dir, CancellationToken.None); }
        catch (InvalidOperationException ex) { rejected = ex.Message.Contains("albeq.dat"); }
        Check(rejected && ReadLaunches(dir).Length == 0, "missing albeq rejected before start");
        passed.Add("missing_albeq_preflight");
    }

    private static async Task TestFreshness()
    {
        string dir = Fixture("freshness", "oldtime");
        var plan = GrammResumePlan.Create(dir, 1, 2, 1);
        using (var cancel = new CancellationTokenSource(2200))
        {
            await ExpectCancelled(new GrammResumeRunner().RunAsync(plan, exe, dir, cancel.Token));
        }
        Check(plan.IsComplete(1) && plan.IsComplete(2), "freshness fixture structurally complete");
        Check(CountAlive(dir) == 0, "stale timestamps not accepted");
        passed.Add("fresh_output_required");
    }

    private static async Task TestDll()
    {
        string dir = Fixture("dll", "valid_exit0");
        var plan = GrammResumePlan.Create(dir, 1, 1, 1);
        var result = await new GrammResumeRunner().RunAsync(plan, typeof(Program).Assembly.Location, dir, CancellationToken.None);
        Check(result.CompletedCount == 1 && plan.IsComplete(1), "dll launcher");
        Check(File.ReadAllText(Path.Combine(dir, "old_albeq_seen.txt")) == "False", "cold launch removes stale albeq before children");
        var complete = GrammResumePlan.Create(dir, 1, 1, 10);
        var empty = await new GrammResumeRunner().RunAsync(complete, exe, dir, CancellationToken.None);
        Check(empty.CompletedCount == 0 && empty.RangeCount == 0, "all complete no process");
        passed.Add("dll_and_allcomplete");
    }

    private static async Task<int> Child(int first, int last)
    {
        Console.SetCursorPosition(0, 0);
        File.WriteAllText("child_" + first + "_" + last + "_" + Environment.ProcessId + ".pid", Environment.ProcessId.ToString());
        File.WriteAllText("launch_" + first + "_" + last + "_" + Environment.ProcessId + ".txt", first + "-" + last);
        string mode = File.ReadAllText("mode.txt");
        if (first == 1)
        {
            File.WriteAllText("old_albeq_seen.txt", File.Exists("albeq.dat").ToString());
            File.WriteAllText("albeq.dat", "fresh test cache");
        }
        if (mode == "input_wait" || mode == "false_finish")
        {
            Console.WriteLine(mode == "input_wait" ? "File 'IIN.dat' not found - Execution stopped - press ESC to continue" :
                "GRAMM simulations for weather situations 1 to 1 finished. Press any key to continue...");
            Console.ReadKey(true); return 0;
        }
        if (mode == "exit0") return 0;
        if (mode == "exit7") return 7;
        if (mode == "fail_fast")
        {
            if (first == 1) { await Task.Delay(600); return 7; }
            await Task.Delay(Timeout.Infinite);
        }
        if (mode == "cancel")
        {
            if (first == 1)
            {
                var start = new ProcessStartInfo
                {
                    FileName = Path.ChangeExtension(typeof(Program).Assembly.Location, ".exe"),
                    UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = Environment.CurrentDirectory
                };
                start.ArgumentList.Add("--descendant");
                Process.Start(start);
            }
            await Task.Delay(Timeout.Infinite);
        }
        for (int i = first; i <= last; i++)
        {
            if (mode == "monitor")
            {
                for (int step=1;step<=4;step++)
                {
                    Console.WriteLine("WEATHER-SIT. TIME[s] TIMESTEP[s] ENDTIME[s]");
                    Console.WriteLine($" {i}/1 {step} 1 10");
                    await Task.Delay(1000);
                }
            }
            WritePair(Environment.CurrentDirectory, i, mode == "partial");
            if (mode == "oldtime")
            {
                File.SetLastWriteTimeUtc(i.ToString("D5") + ".wnd", DateTime.UtcNow.AddHours(-1));
                File.SetLastWriteTimeUtc(i.ToString("D5") + ".scl", DateTime.UtcNow.AddHours(-1));
            }
            await Task.Delay(mode == "stream" ? 750 : 100);
        }
        if (mode == "valid_exit0") return 0;
        if (mode == "valid_exit7") return 7;
        Console.WriteLine("GRAMM simulations for weather situations " + first + " to " + last + " finished. Press any key to continue...");
        Console.ReadKey(true);
        return 0;
    }

    private static string Fixture(string name, string mode)
    {
        string dir = Path.Combine(root, name);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "GRAMM.geb"), "2\n2\n2\n0\n20\n0\n20\n");
        File.WriteAllText(Path.Combine(dir, "albeq.dat"), "test");
        File.WriteAllText(Path.Combine(dir, "mode.txt"), mode);
        return dir;
    }

    private static void WritePair(string dir, int n, bool partial = false)
    {
        string prefix = Path.Combine(dir, n.ToString("D5", CultureInfo.InvariantCulture));
        using (var f = File.Create(prefix + ".wnd"))
        using (var w = new BinaryWriter(f))
        {
            Header(w);
            for (int i = 0; i < 24; i++) w.Write((short)(n % 200));
        }
        if (partial) return;
        using (var f = File.Create(prefix + ".scl"))
        using (var zip = new ZipArchive(f, ZipArchiveMode.Create))
            foreach (string ext in new[] { ".ust", ".obl", ".scl" })
            {
                var entry = zip.CreateEntry(n.ToString("D5") + ext);
                using (var s = entry.Open())
                using (var w = new BinaryWriter(s))
                {
                    Header(w);
                    for (int i = 0; i < 4; i++) w.Write((short)(n % 200));
                }
            }
    }

    private static void Header(BinaryWriter w)
    {
        w.Write(-1); w.Write(2); w.Write(2); w.Write(2); w.Write(10f);
    }
    private static Dictionary<string, string> Snapshot(string dir) =>
        Directory.GetFiles(dir).Where(p => p.EndsWith(".wnd") || p.EndsWith(".scl")).ToDictionary(p => p, Hash);
    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private static string[] ReadLaunches(string dir) => Directory.GetFiles(dir, "launch_*.txt").Select(File.ReadAllText).ToArray();
    private static int CountAlive(string dir) => Directory.GetFiles(dir, "child_*.pid").Select(File.ReadAllText)
        .Select(int.Parse).Count(Alive);
    private static bool Alive(int pid)
    {
        try { using (var p = Process.GetProcessById(pid)) return !p.HasExited; }
        catch (ArgumentException) { return false; }
    }
    private static async Task Until(Func<bool> condition)
    {
        var timeout = Stopwatch.StartNew();
        while (!condition())
        {
            if (timeout.Elapsed.TotalSeconds > 10) throw new Exception("Timed out waiting for child startup.");
            await Task.Delay(50);
        }
    }
    private static async Task ExpectCancelled(Task task)
    {
        bool cancelled = false;
        try { await task; }
        catch (OperationCanceledException) { cancelled = true; }
        Check(cancelled, "cancellation propagated");
    }
    private static void Check(bool okay, string name)
    {
        if (!okay) throw new Exception("FAIL " + name);
        Interlocked.Increment(ref checks);
    }
}
