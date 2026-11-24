using Gral;

DirectoryInfo testDirectory = Directory.CreateTempSubdirectory("gral-gpu-run-");
try { RunChecks(testDirectory.FullName); }
finally { testDirectory.Delete(true); }

static void RunChecks(string computation)
{
    var run = new GralGpuRun(computation, "GRAL-Test", "Device: Test GPU");
    var instance1 = run.AddInstance(1, 2, true);
    var instance2 = run.AddInstance(3, 4, true);
    var sibling = new GralGpuRun(computation, "GRAL-Test", "Same project");
    var siblingInstance = sibling.AddInstance(1, 2, true);
    using var first = new MemoryStream();
    using var second = new MemoryStream();
    using var siblingHandle = new MemoryStream();
    int? firstExit = null;
    int? secondExit = null;
    instance1.Started(101, first, () => firstExit);
    instance2.Started(102, second, () => secondExit);
    siblingInstance.Started(201, siblingHandle, () => null);

    Require(run.Planned == 4 && run.Completed == 0 && run.Status == "Preparing or running GRAL…", "Starting state");
    Require(run.DirectoryPath != sibling.DirectoryPath && instance1.MetricsPath != instance2.MetricsPath, "Concurrent runs and instances own distinct logs");
    File.WriteAllText(instance1.RuntimePath, "Runtime test log");
    run.Poll();
    Require(run.Status == "GPU results pending…", "Runtime startup is indeterminate");

    File.WriteAllText(instance1.MetricsPath, "{\"situation\":1}");
    run.Poll();
    Require(run.Completed == 0, "Partial records do not count");
    File.AppendAllText(instance1.MetricsPath, "\n{\"situation\":1}\n{\"situation\":8}\n{\"situation\":\"2\"}\nnot-json\n");
    run.Poll();
    sibling.Poll();
    Require(run.Completed == 1 && run.Status == "GPU results: 1/4" && sibling.Completed == 0, "Only fresh, valid, unique and in-range records count");

    File.WriteAllText(instance2.MetricsPath, "{\"situation\":3}\n");
    firstExit = 0;
    run.Poll();
    Require(run.Completed == 2 && !run.Finished && !first.CanRead, "Each process finishes independently");
    Require(run.RequestStop() && !run.RequestStop(), "Stop delivery is serialized per run");
    run.RetryStopAllowed();
    Require(run.Status == "Stop not sent. Retry Stop." && run.RequestStop(), "Failed delivery can be retried");
    secondExit = 3;
    run.Poll();
    Require(run.Finished && run.Completed == 2 && run.Status == "GRAL stopped · GPU 2/4" && !second.CanRead, "Error keeps the measured count and disposes process handles");
    Require(File.ReadAllText(run.LogPath).Contains("GRAL exited with code 3") && File.ReadAllText(run.LogPath).Contains("Instance 2"), "Support log records the exit reason and instance");
    sibling.CloseMonitoring();
    Require(!siblingHandle.CanRead && File.ReadAllText(sibling.LogPath).Contains("stopped updating this summary"), "Closing the GUI records that monitoring ended");

    var unknown = new GralGpuRun(computation, "GRAL-Test", "Older build");
    var unknownInstance = unknown.AddInstance(9, null, false);
    int? unknownStatus = null;
    using var unknownProcess = new MemoryStream();
    unknownInstance.Started(301, unknownProcess, () => unknownStatus);
    File.WriteAllText(unknownInstance.MetricsPath, "{\"situation\":9}\n");
    unknown.Poll();
    Require(unknown.Planned == null && unknown.Status == "GPU results: 1", "Unknown denominator stays indeterminate");
    unknownStatus = 0;
    unknown.Poll();
    Require(unknown.Status == "GRAL exited · GPU 1", "Normal exit preserves the GPU result count");

    var locked = new GralGpuRun(computation, "GRAL-Test", "File lock test");
    var lockedInstance = locked.AddInstance(1, 1, false);
    using var lockedProcess = new MemoryStream();
    lockedInstance.Started(401, lockedProcess, () => 0);
    File.WriteAllText(lockedInstance.MetricsPath, "{\"situation\":1}\n");
    using (var held = new FileStream(lockedInstance.MetricsPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
    {
        locked.Poll();
        Require(!locked.Finished, "A transient lock keeps the final metrics eligible for reread");
    }
    locked.Poll();
    Require(locked.Finished && locked.Completed == 1, "Final metrics are read after a transient lock is released");

    Console.WriteLine("PASS: GRAL GPU run state, log separation, streaming metrics, stop/retry, final file lock and diagnostics.");
}

static void Require(bool condition, string description)
{
    if (!condition) { throw new InvalidOperationException(description); }
}
