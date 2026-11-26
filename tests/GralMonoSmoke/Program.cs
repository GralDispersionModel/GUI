using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{
    private const BindingFlags InstanceMember = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags StaticMember = BindingFlags.Static | BindingFlags.NonPublic;

    [STAThread]
    private static int Main()
    {
        try
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);

            string project = Path.GetFullPath("Test project");
            string computation = Directory.CreateDirectory(Path.Combine(project, "Computation")).FullName;
            string externalFlow = Directory.CreateDirectory("External flow").FullName;
            string wind = Directory.CreateDirectory("Wind").FullName;
            File.WriteAllLines(Path.Combine(computation, "GFF_FilePath.txt"), new[] { "", externalFlow });

            using (var form = new Gral.Main())
            {
                Gral.Main.GUISettings.AppSettingsPath = Environment.GetEnvironmentVariable("HOME");
                Gral.Main.ProjectName = project;
                Gral.Main.GRAMMwindfield = wind;
                form.Show();

                // The fixture supplies result files without running project setup or an engine.
                ControlNamed<GroupBox>(form, "groupBox16").Visible = true;
                ControlNamed<GroupBox>(form, "groupBox15").Visible = true;
                CheckTotals(form, computation, externalFlow);
                CheckGrammRows(form, computation);
                Gral.Main.ProjectName = string.Empty;
                form.Close();
            }
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static void CheckTotals(Form form, string computation, string externalFlow)
    {
        var tabs = ControlNamed<TabControl>(form, "tabControl1");
        var computationTab = ControlNamed<TabPage>(form, "Computation");
        TabPage otherTab = tabs.SelectedTab;
        var concentrations = ControlNamed<Label>(form, "label57");
        var concentrationSize = ControlNamed<Label>(form, "label84");
        var flows = ControlNamed<Label>(form, "label72");
        var flowSize = ControlNamed<Label>(form, "label85");

        tabs.SelectedTab = computationTab;
        WaitFor(() => concentrations.Visible && concentrationSize.Visible && flows.Visible && flowSize.Visible &&
            concentrations.Text == "0 files" && concentrationSize.Text == "0MByte" &&
            flows.Text == "0 files" && flowSize.Text == "0MByte", "empty results in Computation");
        Console.WriteLine("PASS empty concentration and flow totals");

        tabs.SelectedTab = otherTab;
        CreateSizedFile(computation, "test.con", 200000);
        CreateSizedFile(computation, "test.grz", 300000);
        CreateSizedFile(computation, "ignored.gff", 900000);
        CreateSizedFile(externalFlow, "test.gff", 700000);
        Application.DoEvents();
        Require(!concentrations.Visible && concentrations.Text == "0 files", "on leaving Computation");

        tabs.SelectedTab = computationTab;
        WaitFor(() => concentrations.Visible && concentrationSize.Visible && flows.Visible && flowSize.Visible &&
            concentrations.Text == "2 files" && concentrationSize.Text == "0.5MByte" &&
            flows.Text == "1 files" && flowSize.Text == "0.7MByte", "mixed results after returning to Computation");
        Console.WriteLine("PASS mixed .con/.grz totals and external .gff totals on tab return");
    }

    private static void CheckGrammRows(Form form, string computation)
    {
        File.WriteAllText(Path.Combine(computation, "GRAMMin.dat"), "y\n");
        File.WriteAllLines(Path.Combine(computation, "IIN.dat"), Enumerable.Repeat("setting: 0", 29));
        File.WriteAllText(Path.Combine(computation, "GRAMM.geb"), "1\n1\n1\n");

        // Supply the production progress panel with one assignment and no running engine.
        Type runType = typeof(Gral.Main).Assembly.GetType("Gral.GrammProgressRun", throwOnError: true);
        object run = Activator.CreateInstance(runType, new object[] { computation });
        var ranges = (List<ValueTuple<int, int, int, int>>)runType.GetField("Ranges", InstanceMember).GetValue(run);
        ranges.Add((1, 1, 2, 1));
        runType.GetMethod("CaptureBaseline", InstanceMember).Invoke(run, new object[] { 1, 2 });
        WriteWind(Path.Combine(computation, "00001_backup.wnd"));
        WriteWind(Path.Combine(computation, "00002.wnd"));
        Require(FirstMissingWind(runType, computation) == 1, "backup name excluded from native Continue");

        typeof(Gral.Main).GetMethod("ShowGrammProgress", InstanceMember).Invoke(form, new[] { run });
        var rows = ControlNamed<ListView>(form, "grammInstanceList");
        var summary = ControlNamed<Label>(form, "grammProgressSummary");
        bool rowDrawn = false;
        rows.DrawSubItem += (_, args) =>
        {
            if (args.ColumnIndex == 0 && args.Item.SubItems[1].Text == "1/2") { rowDrawn = true; }
        };
        WaitFor(() => rows.Visible && rowDrawn && summary.Text.Contains("1/2"), "drawn GRAMM row after one canonical output");
        Require(rows.Items[0].SubItems[2].Text == "1", "one GRAMM situation left");
        Console.WriteLine("PASS GRAMM owner drawing, 1/2 complete and 1 left");

        File.Move(Path.Combine(computation, "00001_backup.wnd"), Path.Combine(computation, "00001.wnd"));
        Require(FirstMissingWind(runType, computation) == 3, "canonical names advance native Continue");
        Console.WriteLine("PASS native Continue skips only canonical wind filenames");
    }

    private static int FirstMissingWind(Type runType, string computation)
    {
        object ids = runType.GetMethod("ExistingWindNumbers", StaticMember).Invoke(null, new object[] { new DirectoryInfo(computation) });
        return (int)runType.GetMethod("NextMissingWind", StaticMember).Invoke(null, new[] { 1, 2, ids });
    }

    private static void WriteWind(string path)
    {
        using (var writer = new BinaryWriter(File.Create(path)))
        {
            writer.Write(-1);
            writer.Write(1);
            writer.Write(1);
            writer.Write(1);
            writer.Write(10f);
            writer.Write(new byte[6]);
        }
    }

    private static void CreateSizedFile(string directory, string name, int length)
    {
        using (var file = File.Create(Path.Combine(directory, name))) { file.SetLength(length); }
    }

    private static T ControlNamed<T>(Control parent, string name) where T : Control
    {
        return parent.Controls.Find(name, true).OfType<T>().Single();
    }

    private static void WaitFor(Func<bool> condition, string description)
    {
        var timer = Stopwatch.StartNew();
        while (!condition() && timer.Elapsed < TimeSpan.FromSeconds(8))
        {
            Application.DoEvents();
            Thread.Sleep(20);
        }
        Require(condition(), description);
    }

    private static void Require(bool condition, string description)
    {
        if (!condition) { throw new InvalidOperationException("Smoke check failed: " + description); }
    }
}
