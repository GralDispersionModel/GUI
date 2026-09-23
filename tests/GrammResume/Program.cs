using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using GralIO;

internal static class Program
{
    private static string Root;
    private static string Fixtures;
    private static readonly List<object> Results = new List<object>();
    private static int Failures;

    private static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--scan") return Scan(args);
        Root = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.Combine(Path.GetTempPath(), "GrammResume_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);
        Fixtures = Path.Combine(Root, "fixtures_" + Guid.NewGuid().ToString("N"));
        Test("scan_progress_and_cancellation", () =>
        {
            string dir=Fixture("scan_progress");
            foreach(int n in Enumerable.Range(1,8)) Pair(dir,n);
            var before=Hashes(dir);var scanned=new List<int>();
            var complete=GrammResumePlan.Create(dir,1,8,2,scanProgress:(done,total)=>{Check(total==8,"scan total");scanned.Add(done);});
            Check(complete.CompletedCount==8&&scanned.SequenceEqual(new[]{4,8}),"bounded scan progress");
            using var stop=new CancellationTokenSource();bool cancelled=false;
            try { GrammResumePlan.Create(dir,1,8,2,scanProgress:(done,total)=>stop.Cancel(),cancellationToken:stop.Token); }
            catch(OperationCanceledException) {cancelled=true;}
            Check(cancelled,"scan cancellation propagated");
            Check(before.OrderBy(x=>x.Key).SequenceEqual(Hashes(dir).OrderBy(x=>x.Key)),"cancelled scan is read only");
        });
        Test("cold_1000_ten_instances", () =>
        {
            string dir = Fixture("cold");
            GrammResumePlan plan = GrammResumePlan.Create(dir, 1, 1000, 10);
            Check(plan.CompletedCount == 0 && plan.PendingCount == 1000 && plan.FirstPending == 1, "cold counts");
            Check(plan.PendingRanges.Count == 10 && plan.MaxInstances == 10, "cold worker ranges");
            for (int i = 0; i < 10; i++)
                Check(plan.PendingRanges[i].First == 100 * i + 1 && plan.PendingRanges[i].Last == 100 * (i + 1), "balanced cold chunks");
            Coverage(plan, Enumerable.Range(1, 1000));
        });
        Test("user_900_complete_100_pending", () =>
        {
            string dir = Fixture("user_example");
            for (int worker = 0; worker < 10; worker++)
                for (int offset = 1; offset <= 90; offset++) Pair(dir, worker * 100 + offset);
            Dictionary<string, string> before = Hashes(dir);
            GrammResumePlan plan = GrammResumePlan.Create(dir, 1, 1000, 10);
            Check(plan.CompletedCount == 900 && plan.PendingCount == 100 && plan.FirstPending == 91, "user counts");
            Check(plan.PendingRanges.Count == 10 && plan.PendingRanges.All(r => r.Count == 10), "user ten gaps");
            Coverage(plan, Enumerable.Range(0, 10).SelectMany(w => Enumerable.Range(w * 100 + 91, 10)));
            Check(before.OrderBy(x => x.Key).SequenceEqual(Hashes(dir).OrderBy(x => x.Key)), "planning preserves all bytes");
        });
        Test("all_complete_starts_nothing", () =>
        {
            string dir = Fixture("complete");
            foreach (int i in Enumerable.Range(1, 8)) Pair(dir, i);
            GrammResumePlan plan = GrammResumePlan.Create(dir, 1, 8, 10);
            Check(plan.CompletedCount == 8 && plan.PendingCount == 0 && plan.PendingRanges.Count == 0 && plan.InstanceAssignments.Count == 0 && plan.FirstPending == 0 && plan.MaxInstances == 0, "all-complete no-op");
        });
        Test("uneven_gaps_change_instance_count", () =>
        {
            string dir = Fixture("uneven");
            int[] complete = { 2, 3, 6, 9, 10, 14, 20 };
            foreach (int i in complete) Pair(dir, i);
            int[] pending = Enumerable.Range(1, 20).Except(complete).ToArray();
            foreach (int workers in new int[] { 1, 2, 7, 32 })
            {
                GrammResumePlan plan = GrammResumePlan.Create(dir, 1, 20, workers);
                Coverage(plan, pending);
                Check(plan.MaxInstances == Math.Min(workers, pending.Length), "worker limit");
                Check(plan.PendingRanges.All(r => r.Count <= (pending.Length + workers - 1) / workers), "range size cap");
            }
        });
        Test("user_79_pending_eight_instances_have_balanced_totals", () =>
        {
            string dir = Fixture("user_79");
            int[] pending = Enumerable.Range(739, 24).Concat(Enumerable.Range(859, 31))
                .Concat(Enumerable.Range(985, 24)).ToArray();
            foreach (int i in Enumerable.Range(1, 1008).Except(pending)) Pair(dir, i);
            Dictionary<string, string> before = Hashes(dir);
            GrammResumePlan plan = GrammResumePlan.Create(dir, 1, 1008, 8);
            Check(plan.CompletedCount == 929 && plan.PendingCount == 79, "actual project counts");
            Coverage(plan, pending);
            Check(plan.InstanceAssignments.Select(a => a.Sum(r => r.Count))
                .SequenceEqual(new[] { 10, 10, 10, 10, 10, 10, 10, 9 }), "79 divided evenly over eight instances");
            Check(plan.InstanceAssignments[2].Select(r => (r.First, r.Last))
                .SequenceEqual(new[] { (759, 762), (859, 864) }), "third instance crosses completed gap without recalculation");
            Check(before.OrderBy(x => x.Key).SequenceEqual(Hashes(dir).OrderBy(x => x.Key)), "planning preserves completed project files");
        });
        Test("many_single_situation_gaps_still_balance_instances", () =>
        {
            string dir = Fixture("many_gaps");
            int[] pending = Enumerable.Range(1, 1025).Where(i => i % 2 != 0).ToArray();
            foreach (int i in Enumerable.Range(1, 1025).Except(pending)) Pair(dir, i);
            foreach (int workers in new[] { 1, 8, 64, 1024, int.MaxValue })
            {
                GrammResumePlan plan = GrammResumePlan.Create(dir, 1, 1025, workers);
                Coverage(plan, pending);
                Check(plan.PendingRanges.All(r => r.Count == 1), "never includes completed situations in a run");
                Check(plan.MaxInstances == Math.Min(workers, pending.Length), "nonempty instance limit");
            }
        });
        Test("cancel_after_final_scan_prevents_assignment", () =>
        {
            string dir = Fixture("cancel_assignment");
            using var stop = new CancellationTokenSource();
            Throws<OperationCanceledException>(() => GrammResumePlan.Create(dir, 1, 3, 2,
                scanProgress: (done, total) => { if (done == total) stop.Cancel(); }, cancellationToken: stop.Token));
        });
        Test("requested_subrange_only", () =>
        {
            string dir = Fixture("subset");
            foreach (int i in new int[] { 1, 3, 5, 7, 9 }) Pair(dir, i);
            GrammResumePlan plan = GrammResumePlan.Create(dir, 3, 7, 10);
            Check(plan.CompletedCount == 3 && plan.PendingCount == 2 && plan.MaxInstances == 2, "subset counts");
            Coverage(plan, new int[] { 4, 6 });
        });
        Test("wind_without_scalar_is_pending", () =>
        {
            string dir = Fixture("missing_scalar"); Pair(dir, 1); File.Delete(Output(dir, 1, ".scl")); PendingOne(dir);
        });
        Test("scalar_without_wind_is_pending", () =>
        {
            string dir = Fixture("missing_wind"); Pair(dir, 1); File.Delete(Output(dir, 1, ".wnd")); PendingOne(dir);
        });
        Test("truncated_wind_is_pending", () =>
        {
            string dir = Fixture("truncated_wind"); Pair(dir, 1);
            using (FileStream file = File.OpenWrite(Output(dir, 1, ".wnd"))) file.SetLength(file.Length - 1);
            PendingOne(dir);
        });
        Test("extra_wind_payload_is_pending", () =>
        {
            string dir = Fixture("extra_wind"); Pair(dir, 1);
            using (FileStream file = new FileStream(Output(dir, 1, ".wnd"), FileMode.Append)) file.WriteByte(1);
            PendingOne(dir);
        });
        Test("wrong_wind_grid_is_pending", () =>
        {
            string dir = Fixture("wrong_wind_grid"); Pair(dir, 1);
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(Output(dir, 1, ".wnd")))) { writer.BaseStream.Position = 4; writer.Write(9); }
            PendingOne(dir);
        });
        Test("wrong_wind_spacing_is_pending", () =>
        {
            string dir = Fixture("wrong_spacing"); Pair(dir, 1);
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(Output(dir, 1, ".wnd")))) { writer.BaseStream.Position = 16; writer.Write(101f); }
            PendingOne(dir);
        });
        Test("truncated_zip_is_pending", () =>
        {
            string dir = Fixture("truncated_zip"); Pair(dir, 1);
            using (FileStream file = File.OpenWrite(Output(dir, 1, ".scl"))) file.SetLength(file.Length - 10);
            PendingOne(dir);
        });
        Test("missing_zip_entry_is_pending", () =>
        {
            string dir = Fixture("missing_zip_entry"); Pair(dir, 1); Scalar(dir, 1, 1, new string[] { ".ust", ".scl" }); PendingOne(dir);
        });
        Test("duplicate_zip_entry_is_pending", () =>
        {
            string dir = Fixture("duplicate_zip_entry"); Pair(dir, 1); Scalar(dir, 1, 1, new string[] { ".ust", ".ust", ".scl" }); PendingOne(dir);
        });
        Test("wrong_scalar_grid_is_pending", () =>
        {
            string dir = Fixture("wrong_scalar_grid"); Pair(dir, 1); Scalar(dir, 1, 1, nx: 4); PendingOne(dir);
        });
        Test("short_scalar_payload_is_pending", () =>
        {
            string dir = Fixture("short_scalar"); Pair(dir, 1); Scalar(dir, 1, 1, scalarBytes: 10); PendingOne(dir);
        });
        Test("zip_crc_mismatch_is_pending", () =>
        {
            string dir = Fixture("crc"); Pair(dir, 1);
            string path = Output(dir, 1, ".scl"); byte[] bytes = File.ReadAllBytes(path); bool changed = false;
            for (int i = 0; i < bytes.Length - 20; i++)
                if (BitConverter.ToUInt32(bytes, i) == 0x02014b50U) { bytes[i + 16] ^= 0x40; changed = true; break; }
            Check(changed, "central directory found"); File.WriteAllBytes(path, bytes); PendingOne(dir);
        });
        Test("numeric_alias_does_not_duplicate_or_fake_completion", () =>
        {
            string dir = Fixture("aliases"); Pair(dir, 1);
            File.Copy(Output(dir, 1, ".wnd"), Path.Combine(dir, "1.wnd")); File.Copy(Output(dir, 1, ".scl"), Path.Combine(dir, "1.scl"));
            GrammResumePlan plan = GrammResumePlan.Create(dir, 1, 1, 1); Check(plan.CompletedCount == 1, "alias no double count");
            File.Delete(Output(dir, 1, ".wnd")); File.Delete(Output(dir, 1, ".scl")); PendingOne(dir);
        });
        Test("locked_result_is_pending", () =>
        {
            string dir = Fixture("locked"); Pair(dir, 1);
            using (FileStream locked = new FileStream(Output(dir, 1, ".wnd"), FileMode.Open, FileAccess.Read, FileShare.None)) PendingOne(dir);
        });
        Test("completion_rechecks_new_and_later_damaged_files", () =>
        {
            string dir = Fixture("recheck"); GrammResumePlan plan = GrammResumePlan.Create(dir, 1, 1, 1);
            Check(!plan.IsComplete(1), "initial missing"); Pair(dir, 1); Check(plan.IsComplete(1), "new output detected");
            File.WriteAllText(Output(dir, 1, ".scl"), "broken"); Check(!plan.IsComplete(1), "later corruption detected");
        });
        Test("sunrise_requires_every_original_and_intermediate_pair", () =>
        {
            string dir = Fixture("sunrise"); Meteo(dir, "30,1.1,3,0", "30,3.1,3,0", "30,3.2,3,0");
            Pair(dir, 1); Pair(dir, 2); Pair(dir, 3); Pair(dir, 4, 1); Pair(dir, 5, 3);
            GrammResumePlan plan = GrammResumePlan.Create(dir, 1, 3, 10, 3);
            Check(plan.CompletedCount == 2 && plan.PendingCount == 1, "incomplete expanded family"); Coverage(plan, new int[] { 3 });
            Pair(dir, 6, 3); Check(plan.IsComplete(3), "last intermediate detected");
            Pair(dir, 6, 6); Check(!plan.IsComplete(3), "zip inner number must be original");
        });
        Test("cold_sunrise_refreshes_generated_mapping", () =>
        {
            string dir = Fixture("sunrise_cold"); Meteo(dir);
            GrammResumePlan plan = GrammResumePlan.Create(dir, 1, 3, 3, 3);
            Check(plan.PendingCount == 3, "cold original range");
            Meteo(dir, "30,1.1,3,0", "30,3.1,3,0"); Pair(dir, 1);
            Check(!plan.IsComplete(1), "new expanded mapping is reloaded"); Pair(dir, 4, 1);
            Check(plan.IsComplete(1), "fresh expanded results pass");
        });
        Test("sunrise_two_digit_intermediate_index", () =>
        {
            string dir = Fixture("sunrise_ten"); Meteo(dir, Enumerable.Range(1, 11).Select(i => "30,1." + i + ",3,0").ToArray());
            foreach (int i in Enumerable.Range(1, 3)) Pair(dir, i);
            foreach (int i in Enumerable.Range(4, 11)) Pair(dir, i, 1);
            Check(GrammResumePlan.Create(dir, 1, 3, 3, 3).CompletedCount == 3, "1.10 retains original and step semantics");
        });
        Test("sunrise_invalid_and_duplicate_mapping_rejected", () =>
        {
            foreach (string[] rows in new string[][]
            {
                new string[] { "30,4.1,3,0" }, new string[] { "30,1.1,3,1" },
                new string[] { "30,1.1,3,0", "30,1.1,3,0" }, new string[] { "30,1.2,3,0" },
                new string[] { "30,3.1,3,0", "30,1.1,3,0" }
            })
            {
                string dir = Fixture("bad_mapping_" + Guid.NewGuid().ToString("N")); Meteo(dir, rows);
                Throws<InvalidDataException>(() => GrammResumePlan.Create(dir, 1, 3, 3, 3));
            }
        });
        Test("malformed_grid_and_invalid_arguments_rejected", () =>
        {
            string dir = Fixture("bad_grid"); File.WriteAllText(Path.Combine(dir, "GRAMM.geb"), "2\n3\n");
            Throws<InvalidDataException>(() => GrammResumePlan.Create(dir, 1, 3, 1));
            Throws<ArgumentOutOfRangeException>(() => GrammResumePlan.Create(dir, 0, 3, 1));
            Throws<ArgumentOutOfRangeException>(() => GrammResumePlan.Create(dir, 1, 3, 0));
            Throws<ArgumentOutOfRangeException>(() => GrammResumePlan.Create(dir, 1, 4, 1, 3));
        });
        Test("large_result_number_uses_engine_padding", () =>
        {
            string dir = Fixture("large_number"); Pair(dir, 100001);
            Check(GrammResumePlan.Create(dir, 100001, 100001, 1).CompletedCount == 1, "six-digit output accepted");
        });
        string report = JsonSerializer.Serialize(new { status = Failures == 0 ? "pass" : "fail", tests = Results.Count, passed = Results.Count - Failures, failed = Failures, results = Results }, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(Root, "harness_results.json"), report);
        Console.WriteLine("tests=" + Results.Count + " passed=" + (Results.Count - Failures) + " failed=" + Failures);
        return Failures == 0 ? 0 : 1;
    }

    private static int Scan(string[] args)
    {
        if (args.Length != 7)
            throw new ArgumentException("--scan computation first final instances sunriseCount jsonOut");
        string directory = Path.GetFullPath(args[1]);
        string output = Path.GetFullPath(args[6]);
        if (output.StartsWith(directory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Scan report must be outside the read-only computation directory.");
        int first = int.Parse(args[2], CultureInfo.InvariantCulture);
        int final = int.Parse(args[3], CultureInfo.InvariantCulture);
        int instances = int.Parse(args[4], CultureInfo.InvariantCulture);
        int sunrise = int.Parse(args[5], CultureInfo.InvariantCulture);
        Stopwatch total = Stopwatch.StartNew();
        Dictionary<string, string> beforeMetadata = Metadata(directory);
        string[] winds = Directory.GetFiles(directory, "*.wnd").OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
        HashSet<string> sampled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < Math.Min(10, winds.Length); i++)
        {
            string wind = winds[(int)((long)i * (winds.Length - 1) / Math.Max(1, Math.Min(10, winds.Length) - 1))];
            sampled.Add(wind);
            string scalar = Path.ChangeExtension(wind, ".scl");
            if (File.Exists(scalar)) sampled.Add(scalar);
        }
        foreach (string name in new string[] { "GRAMM.geb", "GRAMMin.dat", "meteopgt.all", "IIN.dat" })
        {
            string path = Path.Combine(directory, name);
            if (File.Exists(path)) sampled.Add(path);
        }
        Dictionary<string, string> beforeHashes = SampleHashes(sampled);
        Stopwatch validation = Stopwatch.StartNew();
        GrammResumePlan plan = GrammResumePlan.Create(directory, first, final, instances, sunrise);
        validation.Stop();
        Dictionary<string, string> afterMetadata = Metadata(directory);
        Dictionary<string, string> afterHashes = SampleHashes(sampled);
        total.Stop();
        bool metadataUnchanged = beforeMetadata.OrderBy(p => p.Key).SequenceEqual(afterMetadata.OrderBy(p => p.Key));
        bool hashesUnchanged = beforeHashes.OrderBy(p => p.Key).SequenceEqual(afterHashes.OrderBy(p => p.Key));
        var report = new
        {
            status = metadataUnchanged && hashesUnchanged ? "pass" : "fail",
            mode = "read_only_existing_project_scan",
            scope = "Existing result integrity and resume planning only; no GRAMM calculation or production validation performed.",
            computation_directory = directory,
            first, final, instances, original_meteo_count = sunrise,
            wind_file_count = winds.Length,
            scalar_file_count = Directory.GetFiles(directory, "*.scl").Length,
            completed_original_situations = plan.CompletedCount,
            pending_original_situations = plan.PendingCount,
            first_pending = plan.FirstPending,
            active_instance_limit = plan.MaxInstances,
            pending_ranges = plan.PendingRanges.Select(r => new { first = r.First, last = r.Last, count = r.Count }).ToArray(),
            instance_assignments = plan.InstanceAssignments.Select((ranges, index) => new
            {
                instance = index + 1,
                count = ranges.Sum(r => r.Count),
                ranges = ranges.Select(r => new { first = r.First, last = r.Last, count = r.Count }).ToArray()
            }).ToArray(),
            validation_seconds = validation.Elapsed.TotalSeconds,
            total_seconds = total.Elapsed.TotalSeconds,
            files_metadata_checked = beforeMetadata.Count,
            metadata_unchanged = metadataUnchanged,
            sampled_sha256_count = beforeHashes.Count,
            sampled_sha256_unchanged = hashesUnchanged,
            sampled_sha256_before = beforeHashes,
            sampled_sha256_after = afterHashes
        };
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        File.WriteAllText(output, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("scan completed=" + plan.CompletedCount + " pending=" + plan.PendingCount
            + " ranges=" + plan.PendingRanges.Count + " validation_seconds=" + validation.Elapsed.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture)
            + " unchanged=" + (metadataUnchanged && hashesUnchanged));
        return metadataUnchanged && hashesUnchanged ? 0 : 1;
    }
    private static Dictionary<string, string> Metadata(string directory)
    {
        return Directory.GetFiles(directory).ToDictionary(Path.GetFileName, path =>
        {
            FileInfo file = new FileInfo(path);
            return file.Length.ToString(CultureInfo.InvariantCulture) + ":" + file.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture);
        });
    }
    private static Dictionary<string, string> SampleHashes(IEnumerable<string> paths)
    {
        return paths.ToDictionary(Path.GetFileName, path =>
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Convert.ToHexString(SHA256.HashData(stream));
        });
    }
    private static void Test(string name, Action action)
    {
        try { action(); Results.Add(new { name, status = "pass", error = "" }); Console.WriteLine("PASS " + name); }
        catch (Exception ex) { Failures++; Results.Add(new { name, status = "fail", error = ex.ToString() }); Console.WriteLine("FAIL " + name + " " + ex.Message); }
    }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }
    private static string Fixture(string name)
    {
        string directory = Path.Combine(Fixtures, name); Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "GRAMM.geb"), "2 ! nx\n3 ! ny\n2 ! nz\n1000\n1200\n2000\n2300\n");
        return directory;
    }
    private static string Output(string directory, int number, string extension) => Path.Combine(directory, number.ToString("D5", CultureInfo.InvariantCulture) + extension);
    private static void Header(BinaryWriter writer, int nx = 2) { writer.Write(-1); writer.Write(nx); writer.Write(3); writer.Write(2); writer.Write(100f); }
    private static void Pair(string directory, int number, int original = 0)
    {
        using (BinaryWriter writer = new BinaryWriter(File.Create(Output(directory, number, ".wnd")))) { Header(writer); writer.Write(new byte[72]); }
        Scalar(directory, number, original == 0 ? number : original);
    }
    private static void Scalar(string directory, int number, int original, string[] entries = null, int nx = 2, int scalarBytes = 12)
    {
        using (FileStream stream = File.Create(Output(directory, number, ".scl")))
        using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
            foreach (string extension in entries ?? new string[] { ".ust", ".obl", ".scl" })
            {
                ZipArchiveEntry entry = archive.CreateEntry(original.ToString("D5", CultureInfo.InvariantCulture) + extension);
                using (BinaryWriter writer = new BinaryWriter(entry.Open())) { Header(writer, nx); writer.Write(new byte[scalarBytes]); }
            }
    }
    private static void Meteo(string directory, params string[] added)
    {
        File.WriteAllLines(Path.Combine(directory, "meteopgt.all"), new string[] { "10,0,0,", "Wind direction sector,Wind speed class,stability class, frequency", "3,2,3,0.333", "3,2,4,0.333", "3,2,3,0.334" }.Concat(added));
    }
    private static void PendingOne(string directory)
    {
        GrammResumePlan plan = GrammResumePlan.Create(directory, 1, 1, 1);
        Check(plan.CompletedCount == 0 && plan.PendingCount == 1 && !plan.IsComplete(1), "invalid pair must be recalculated");
    }
    private static void Coverage(GrammResumePlan plan, IEnumerable<int> expected)
    {
        int[] actual = plan.PendingRanges.SelectMany(r => Enumerable.Range(r.First, r.Count)).ToArray();
        Check(actual.SequenceEqual(expected), "pending coverage without skipped or duplicate situations");
        Check(actual.Distinct().Count() == actual.Length && actual.Length == plan.PendingCount, "coverage count");
        Check(plan.InstanceAssignments.Count == plan.MaxInstances, "one assignment per used instance");
        int[] perInstance = plan.InstanceAssignments.Select(a => a.Sum(r => r.Count)).ToArray();
        Check(perInstance.All(n => n > 0) && perInstance.Max() - perInstance.Min() <= 1, "instance totals differ by at most one");
        Check(plan.InstanceAssignments.SelectMany(a => a).SelectMany(r => Enumerable.Range(r.First, r.Count))
            .SequenceEqual(actual), "instance assignments preserve all pending ranges exactly once");
    }
    private static Dictionary<string, string> Hashes(string directory)
    {
        return Directory.GetFiles(directory).ToDictionary(Path.GetFileName, path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))));
    }
}