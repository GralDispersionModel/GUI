using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;
using GralItemData;

internal static class SourceGroupTests
{
    static readonly List<object> Results = new List<object>();
    static int Assertions;
    static string Output;
    static void Check(bool value, string message)
    {
        Assertions++;
        if (!value) throw new InvalidOperationException(message);
    }
    static void Run(string name, Action test)
    {
        int start = Assertions;
        test();
        Results.Add(new { name, status = "pass", assertions = Assertions - start });
        Console.WriteLine("PASS " + name);
    }
    static object Invoke(object instance, string name, params object[] args)
    {
        try { return instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, args); }
        catch (TargetInvocationException ex) { throw ex.InnerException; }
    }
    static T Field<T>(object instance, string name) => (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(instance);
    static void FilenameProtocol()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int id = 1; id <= 1295; id++)
        {
            string code = Gral.SourceGroupFileName.Encode(id);
            Check(code.Length == 2 && names.Add(code), "Windows filename collision");
            Check(Gral.SourceGroupFileName.TryDecode(code, out int decoded) && decoded == id, "token round trip");
            Check(Gral.SourceGroupFileName.TryModulationStem("emissions" + Gral.SourceGroupFileName.ModulationToken(id), out decoded) && decoded == id, "modulation round trip");
            if (id < 100) Check(Gral.SourceGroupFileName.ModulationToken(id) == id.ToString("D3"), "legacy modulation name");
        }
        foreach (var pair in new[] { (1,"01"),(99,"99"),(100,"A0"),(359,"Z9"),(360,"0A"),(619,"9Z"),(620,"AA"),(1295,"ZZ") })
            Check(Gral.SourceGroupFileName.Encode(pair.Item1) == pair.Item2, "encoding boundary");
        foreach (string bad in new[] { "", "00", "100", "A*", "é0", "ſ0" })
            Check(!Gral.SourceGroupFileName.TryDecode(bad,out _), "invalid filename token");
    }
    static void Storage()
    {
        foreach (int id in new[] { 1, 99, 100, 300, 1001, 1295 })
        {
            var value = new PollutantsData { SourceGroup = id };
            Check(value.SourceGroup == id, "setter " + id);
            Check(new PollutantsData(id).SourceGroup == id, "constructor " + id);
            Check(new PollutantsData(value).SourceGroup == id, "copy " + id);
        }
        Check(new PollutantsData().SourceGroup == 1, "legacy default");
        bool rejected = false;
        try { new PollutantsData(1296); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "out-of-range ID must not be clamped or saved");

    }
    static void Serialization()
    {
        foreach (int id in new[] { 1, 99, 100, 300, 1001, 1295 })
        {
            var ps = new PointSourceData(); ps.Poll.SourceGroup = id;
            Check(new PointSourceData("1," + ps.ToString()).Poll.SourceGroup == id, "point " + id);
            var area = new AreaSourceData(); area.Poll.SourceGroup = id;
            Check(new AreaSourceData("1," + area.ToString()).Poll.SourceGroup == id, "area " + id);
            var line = new LineSourceData(); line.Poll.Add(new PollutantsData { SourceGroup = id });
            Check(new LineSourceData("2," + line.ToString(2)).Poll.Any(x => x.SourceGroup == id), "line " + id);
            var portal = new PortalsData(); portal.Poll.Add(new PollutantsData(id));
            Check(new PortalsData("1," + portal.ToString()).Poll.Any(x => x.SourceGroup == id), "portal " + id);
        }
    }
    static void Catalog()
    {
        string file = Path.Combine(Output, "definitions.txt");
        File.WriteAllLines(file, new[] { "Hour100,100", "Hour300,300", "Sparse,1295" });
        var groups = Gral.SourceGroupCatalog.ReadDefinitions(file);
        Check(groups.Count == 3 && groups[1295] == "Sparse", "read sparse definitions");
        var choices = Gral.SourceGroupCatalog.Choices(groups);
        Check(choices.Count == 102, "compact choices");
        Check(choices.Contains("Hour100,100"), "100 selection");
        Check(Gral.SourceGroupCatalog.DisplayName(choices, 1295) == "Sparse,1295", "sparse label");
        Check(Gral.SourceGroupCatalog.DisplayName(choices, 1294) == "1294", "unnamed source");
        foreach (string invalid in new[] { "A,1296", "A,0", "A,-1", "A,2147483648", "A,100\nB,100" })
        {
            File.WriteAllText(file, invalid);
            bool rejected = false;
            try { Gral.SourceGroupCatalog.ReadDefinitions(file); } catch (InvalidDataException) { rejected = true; }
            Check(rejected, "invalid definition rejected");
            Check(File.ReadAllText(file) == invalid, "invalid input preserved");
        }
    }
    static void Editor()
    {
        string project = Path.Combine(Output, "editor");
        Directory.CreateDirectory(Path.Combine(project, "Settings"));
        string file = Path.Combine(project, "Settings", "Sourcegroups.txt");
        File.WriteAllLines(file, new[] { "Keep,1", "Hour100,100", "Sparse,1295" });
        Gral.Main.ProjectName = project;
        using (var main = new Gral.Main())
        using (var form = new GralMainForms.Sourcegroups(main))
        {
            form.CreateControl();
            Invoke(form, "Sourcegroups_Load", null, EventArgs.Empty);
            var table = Field<DataTable>(form, "sourceGroups");
            var grid = Field<DataGridView>(form, "dataGridView1");
            Check(grid.AllowUserToAddRows, "new rows enabled");
            Check(!grid.Columns[0].ReadOnly, "ID input enabled");
            Check(table.Rows.Count == 101, "sparse editor allocation");
            for (int id = 101; id <= 300; id++) table.Rows.Add(id, "Hour" + id);
            Invoke(form, "Button3_Click", null, EventArgs.Empty);
        }
        var saved = Gral.SourceGroupCatalog.ReadDefinitions(file);
        Check(saved.Count == 203, "all named rows saved");
        Check(saved[1] == "Keep" && saved[100] == "Hour100" && saved[300] == "Hour300" && saved[1295] == "Sparse", "IDs preserved on save");
        using (var main = new Gral.Main())
        using (var form = new GralMainForms.Sourcegroups(main))
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-30000,-30000);
            form.ShowInTaskbar = false;
            form.Show(); Application.DoEvents();
            var table = Field<DataTable>(form, "sourceGroups");
            Check(table.Rows.Count == 301, "reopen all definitions");
            var grid = Field<DataGridView>(form, "dataGridView1");
            grid.FirstDisplayedScrollingRowIndex = Math.Max(0, grid.RowCount - 9);
            for (int id = 301; id < 1295; id++) table.Rows.Add(id, "Hour" + id);
            foreach (DataRow row in table.Rows)
                if (string.IsNullOrWhiteSpace(Convert.ToString(row[1]))) row[1] = "Hour" + row[0];
            Invoke(form, "Button3_Click", null, EventArgs.Empty);
        }
        Check(Gral.SourceGroupCatalog.ReadDefinitions(file).Count == 1295, "maximum catalog saved");
        using (var main = new Gral.Main())
        using (var form = new GralMainForms.Sourcegroups(main))
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-30000,-30000); form.ShowInTaskbar = false;
            form.Show(); Application.DoEvents();
            var table = Field<DataTable>(form, "sourceGroups");
            Check(table.Rows.Count == 1295, "maximum catalog reopened");
            var grid = Field<DataGridView>(form, "dataGridView1");
            grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 9;
            using (var bitmap = new Bitmap(form.Width, form.Height))
            { form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size)); bitmap.Save(Path.Combine(Output, "source_group_editor.png")); }
        }
    }
    static bool Legacy;
    static int[] ExtendedIds() => Legacy ? Enumerable.Range(1, 99).ToArray() : Enumerable.Range(1, 300).Concat(new[] { 1001, 1295 }).ToArray();
    static string Project(string name)
    {
        string path = Path.Combine(Output, name);
        foreach (string part in new[] { "Settings", "Computation", "Emissions", "Maps" })
            Directory.CreateDirectory(Path.Combine(path, part));
        Gral.Main.ProjectName = path;
        Gral.Main.ProjectSetting.EmissionModulationPath = Path.Combine(path, "Computation");
        return path;
    }
    static void Selectors()
    {
        using var ps = new GralItemForms.EditPointSources();
        using var area = new GralItemForms.EditAreaSources();
        foreach (object form in new object[] { ps, area })
        {
            var combo = Field<ComboBox>(form, "comboBox1");
            combo.Items.Add("Legacy,1");
            Invoke(form, "combo", 1295);
            Check(Gral.SourceGroupCatalog.GetNumber(combo.Text) == 1295, "unnamed imported ID selected");
            Check(combo.Items.Count == 2, "sparse combo allocation");
        }
    }
    static void FileIO()
    {
        using var main = new Gral.Main();
        string path = Project("file_io");
        var points = new List<PointSourceData>();
        var areas = new List<AreaSourceData>();
        var lines = new List<LineSourceData>();
        var portals = new List<PortalsData>();
        foreach (int id in ExtendedIds())
        {
            var point = new PointSourceData { Name = "P" + id, Pt = new GralDomain.PointD(80, 80), Height = 5, Diameter = 1, Temperature = 293 };
            point.Poll.SourceGroup = id; points.Add(point);
            var area = new AreaSourceData(); area.Poll.SourceGroup = id;
            area.Pt.AddRange(new[] { new GralDomain.PointD(40,40), new GralDomain.PointD(60,40), new GralDomain.PointD(60,60), new GralDomain.PointD(40,60) }); areas.Add(area);
            var line = new LineSourceData(); line.Poll.Add(new PollutantsData { SourceGroup = id });
            line.Pt.AddRange(new[] { new GralData.PointD_3d(40,40,1), new GralData.PointD_3d(60,60,1) }); lines.Add(line);
            var portal = new PortalsData { Pt1 = new GralDomain.PointD(40,40), Pt2 = new GralDomain.PointD(60,60) };
            portal.Poll.Add(new PollutantsData { SourceGroup = id }); portals.Add(portal);
        }
        Check(new PointSourceDataIO().SavePointSources(points, path), "save points");
        Check(new AreaSourceDataIO().SaveAreaData(areas, path), "save areas");
        Check(new LineSourceDataIO().SaveLineSources(lines, path), "save lines");
        Check(new PortalsDataIO().SavePortalSources(portals, path), "save portals");
        points.Clear(); areas.Clear(); lines.Clear(); portals.Clear();
        Check(new PointSourceDataIO().LoadPointSources(points, Path.Combine(path,"Emissions","Psources.txt")), "load points");
        Check(new AreaSourceDataIO().LoadAreaData(areas, Path.Combine(path,"Emissions","Asources.txt")), "load areas");
        Check(new LineSourceDataIO().LoadLineSources(lines, Path.Combine(path,"Emissions","Lsources.txt")), "load lines");
        Check(new PortalsDataIO().LoadPortalSources(portals, Path.Combine(path,"Emissions","Portalsources.txt")), "load portals");
        Check(points.Select(p => p.Poll.SourceGroup).SequenceEqual(ExtendedIds()), "point IDs");
        Check(areas.Select(p => p.Poll.SourceGroup).SequenceEqual(ExtendedIds()), "area IDs");
        Check(lines.Select(p => p.Poll[0].SourceGroup).SequenceEqual(ExtendedIds()), "line IDs");
        Check(portals.Select(p => p.Poll[0].SourceGroup).SequenceEqual(ExtendedIds()), "portal IDs");
    }
    static void Modulation()
    {
        string path = Project("modulation"), file = Path.Combine(path,"Settings","emissionmodulations.txt");
        File.WriteAllLines(file, Enumerable.Range(1, 300).Select(id => id + ",D,S"));
        Gral.SourceGroupCatalog.UpdateModulation(file, 299, "Updated", "Winter");
        Gral.SourceGroupCatalog.UpdateModulation(file, 1295, "Sparse", "Annual");
        string[] rows = File.ReadAllLines(file);
        Check(rows.Length == 301 && rows[299] == "300,D,S" && rows[298] == "299,Updated,Winter" && rows[300] == "1295,Sparse,Annual", "preserve all rows and append full ID");
        var selected = ExtendedIds().Reverse().ToArray();
        string times = Path.Combine(path,"Computation","emissions_timeseries.txt");
        // Extra unselected column, reversed selection, missing selected ID 1001, and a true zero.
        var columns = Enumerable.Range(1,300).Concat(new[] { 1295, 1294 }).ToArray();
        File.WriteAllLines(times, new[] { "date;hour;" + string.Join(";", columns),
            "01.01.2022;00;" + string.Join(";", columns.Select(id => id == 1 ? "0" : "2")),
            "01.01.2022;01;" + string.Join(";", columns.Select(id => id == 1 ? "0" : "4")) });
        var means = Gral.SourceGroupCatalog.ReadMeanFactors(times, selected, new List<string>());
        Check(means.Count == 301 && means[1295] == 3 && means[1] == 0 && !means.ContainsKey(1001), "ID mapped means and zero/default");
        using var main = new Gral.Main();
        foreach (int id in selected) main.listView1.Items.Add("Hour: " + id);
        var totals = selected.ToDictionary(id => id, _ => 1.0);
        using (var form = new GralMainForms.TotalEmissions(totals, main, "NOx", true))
        {
            Invoke(form, "TotalEmissionsLoad", null, EventArgs.Empty);
            var factors = Field<double[]>(form, "EmissionFactor");
            var used = Field<bool[]>(form, "TimeSeriesUsed");
            for (int i=0;i<selected.Length;i++)
            {
                Check(factors[i] == (selected[i] == 1001 ? 1 : selected[i] == 1 ? 0 : 3), "selected factor " + selected[i]);
                Check(used[i] == (selected[i] != 1001), "selected flag " + selected[i]);
            }
        }
        File.Delete(times);
        foreach (int id in selected)
            File.WriteAllLines(Path.Combine(path,"Computation","emissions" + Gral.SourceGroupFileName.ModulationToken(id) + ".dat"),
                Enumerable.Range(0,24).Select(h => h + ",2" + (h < 12 ? ",3" : "")));
        using (var form = new GralMainForms.TotalEmissions(totals, main, "NOx", false))
        {
            Invoke(form, "TotalEmissionsLoad", null, EventArgs.Empty);
            Check(Field<double[]>(form,"EmissionFactor").All(v => Math.Abs(v-6)<1e-12), "full modulation filenames and compact factors");
        }
        File.WriteAllText(times, "date hour 100 1295\n01.01.2022 00 2 4\n01.01.2022 01 4 6\n");
        var spaced = Gral.SourceGroupCatalog.ReadMeanFactors(times, selected, new List<string>());
        Check(spaced[100] == 3 && spaced[1295] == 5, "legacy space-delimited factors");
        foreach (string bad in new[] { "date;hour;100;100\n01.01.2022;00;1;1", "date;hour;2147483648\n01.01.2022;00;1",
            "date;hour;100\n01.01.2022;00;NaN", "date;hour;100\n01.01.2022;00;Infinity" })
        {
            File.WriteAllText(times,bad); bool rejected=false;
            try { Gral.SourceGroupCatalog.ReadMeanFactors(times, selected, new List<string>()); } catch (Exception ex) when (ex is InvalidDataException || ex is FormatException) { rejected=true; }
            Check(rejected,"invalid factors rejected");
        }
    }
    static void Decay()
    {
        using var form = new GralMainForms.DecayRateForm { DecayRate = new List<GralData.DecayRates>(), SourceGroups = ExtendedIds().ToList() };
        form.CreateControl(); Invoke(form,"DecayRateForm_Load",null,EventArgs.Empty);
        var grid=Field<DataGridView>(form,"dataGridView1");
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.IsNewRow) continue;
            int id=int.Parse(row.Cells[0].Value.ToString().TrimEnd(':'));
            row.Cells[1].Value=id==1295 ? 0.125 : id==100 ? 0.25 : 0;
        }
        Invoke(form,"button3_Click",null,EventArgs.Empty);
        Check(form.DecayRate.Count==2,"sparse decay count");
        Check(form.DecayRate.Any(d=>d.SourceGroup==1295 && d.DecayRate==0.125),"sparse decay mapping");
        Check(form.DecayRate.Any(d=>d.SourceGroup==100 && d.DecayRate==0.25),"ID100 decay mapping");
    }
    static void Receptors()
    {
        string path=Project("receptors"), comp=Path.Combine(path,"Computation");
        int[] groups=ExtendedIds(), selected=groups.Reverse().Where(id=>id!=1001).ToArray();
        File.WriteAllText(Path.Combine(comp,"Receptor.dat"),"1\n1,100,100,2,Test\n");
        File.WriteAllText(Path.Combine(comp,"mettimeseries.dat"),"01.01.2022 00 1 27 4\n");
        File.WriteAllText(Path.Combine(comp,"meteopgt.all"),"10,1,10\nwind_dir,wind_speed,stability,frequency\n27,1,4,1\n");
        var headers=new[] { string.Join("\t",groups.Select(_=>"Test"))+"\t",string.Join("\t",groups)+"\t",
            string.Join("\t",groups.Select(_=>"100"))+"\t",string.Join("\t",groups.Select(_=>"100"))+"\t",
            string.Join("\t",groups.Select(_=>"2"))+"\t",string.Join("\t",groups.Select(_=>"-"))+"\t" };
        File.WriteAllLines(Path.Combine(comp,"ReceptorConcentrations.dat"), headers.Concat(new[]{ string.Join("\t",groups.Select((_,i)=>(i+1).ToString()))+"\t" }));
        var data=new GralBackgroundworkers.BackgroundworkerData {
            ProjectName=path, PathEmissionModulation=comp, PathEvaluationResults=Path.Combine(path,"Maps"),
            SelectedSourceGroup=string.Join(",",selected.Select(id=>"Hour: "+id)),
            ComputedSourceGroup=string.Join(",",groups)+",", MaxSource=selected.Length, MaxSourceComputed=groups.Length,
            DecSep=".", Pollutant="NOx", Prefix="", UserText="", MeteoNotClassified=true };
        using var form=new GralBackgroundworkers.ProgressFormBackgroundworker(data);
        Invoke(form,"ReceptorConcentration",data,new System.ComponentModel.DoWorkEventArgs(null));
        string[] result=File.ReadAllLines(Path.Combine(path,"Maps","ReceptorTimeSeries_NOx.txt"),System.Text.Encoding.Unicode);
        var header=result[1].Split('\t').Skip(2).Where(s=>s.Length>0).Select(int.Parse).ToArray();
        Check(header.SequenceEqual(selected),"receptor selected header order");
        var values=result.Last(l=>l.StartsWith("01.01\t")).Split('\t').Skip(2).Where(s=>s.Length>0).Select(double.Parse).ToArray();
        Check(values.Length==selected.Length,"receptor selected output count");
        for(int i=0;i<values.Length;i++) Check(values[i]==Array.IndexOf(groups,selected[i])+1,"receptor column "+selected[i]);
    }
    static string CoreDll;
    static void Handoff()
    {
        string path=Project("handoff"), comp=Path.Combine(path,"Computation");
        int[] groups=ExtendedIds();
        using var main=new Gral.Main();
        main.GralDomRect.West=0; main.GralDomRect.South=0; main.GralDomRect.East=200;main.GralDomRect.North=200;main.HorGridSize=20;
        main.Pollmod.Add("NOx");main.listBox5.Items.Add("NOx");main.listBox5.SelectedIndex=0;
        foreach(int id in groups) main.listView1.Items.Add("Hour: "+id);
        foreach(var entry in new[]{("textBox6","0"),("textBox7","200"),("textBox5","0"),("textBox2","200")})
            Field<TextBox>(main,entry.Item1).Text=entry.Item2;
        var points=new List<PointSourceData>();
        foreach(int id in groups)
        {
            var source=new PointSourceData { Pt=new GralDomain.PointD(80,80),Height=1,Diameter=0.2f,Temperature=293,Velocity=0.1f };
            source.Poll.SourceGroup=id;source.Poll.EmissionRate[0]=1;points.Add(source);
        }
        Check(new PointSourceDataIO().SavePointSources(points,path),"write source input");
        main.EmifileReset=true; main.WriteGralGebFile(); main.EmifileReset=false;
        Invoke(main,"CreateGralEmissionFiles");
        var written=File.ReadAllLines(Path.Combine(comp,"point.dat")).Skip(2).Select(l=>int.Parse(l.Split(',')[10])).ToArray();
        Check(written.SequenceEqual(groups),"GUI point.dat IDs");
        Check(File.ReadAllLines(Path.Combine(comp,"GRAL.geb"))[6].Split('!')[0].Split(',',StringSplitOptions.RemoveEmptyEntries).Where(s=>!string.IsNullOrWhiteSpace(s)).Select(int.Parse).SequenceEqual(groups),"GUI GRAL.geb IDs");
        var totals=new Dictionary<int,double>(); Invoke(main,"ComputeTotalEmissions",totals);
        Check(totals.Count==groups.Length && totals.Values.All(v=>Math.Abs(v-8.76)<1e-12),"all source emission totals");
        // Keep GUI-written source files and domain; provide synthetic meteorology and run settings.
        File.WriteAllLines(Path.Combine(comp,"in.dat"), new[]{"100","30","0","4","0","0.1","47","Y","NOX","2","4","1","0","0","compressed V03","nokeystroke","ASCiiResults 0","0","0","0","0","1"});
        File.WriteAllText(Path.Combine(comp,"Max_Proc.txt"),"1\n");
        File.WriteAllText(Path.Combine(comp,"micro_vert_layers.txt"),"40\n");
        File.WriteAllText(Path.Combine(comp,"meteopgt.all"),"10,1,10\nwind_dir,wind_speed,stability,frequency\n27,1,4,1\n");
        File.WriteAllText(Path.Combine(comp,"mettimeseries.dat"),"01.01.2022 00 1 27 4\n01.01.2022 01 1 27 4\n");
        File.WriteAllLines(Path.Combine(comp,"emissions_timeseries.txt"),new[]{"date;hour;"+string.Join(";",groups.Reverse()),
            "01.01.2022;00;"+string.Join(";",groups.Reverse().Select(id=>id==1295?"1":"0")),
            "01.01.2022;01;"+string.Join(";",groups.Reverse().Select(id=>id==300?"1":"0"))});
        var start=new System.Diagnostics.ProcessStartInfo("dotnet") { WorkingDirectory=comp,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true };
        start.ArgumentList.Add(CoreDll);
        using(var process=System.Diagnostics.Process.Start(start))
        {
            var stdout=process.StandardOutput.ReadToEndAsync();var stderr=process.StandardError.ReadToEndAsync();
            if(!process.WaitForExit(120000)) { process.Kill(true);throw new TimeoutException("core"); }
            File.WriteAllText(Path.Combine(path,"console.log"),stdout.Result+stderr.Result);
            Check(process.ExitCode==0,"core exit");
        }
        foreach(int hour in new[]{1,2})
        {
            using var archive=System.IO.Compression.ZipFile.OpenRead(Path.Combine(comp,hour.ToString("D5")+".grz"));
            Check(archive.Entries.Count==groups.Length,"core output group count");
            foreach(int id in groups) Check(archive.GetEntry(hour.ToString("D5")+"-1"+Gral.SourceGroupFileName.Encode(id)+".con")!=null,"core output "+id);
        }
        var data=new GralBackgroundworkers.BackgroundworkerData { ProjectName=path,PathEmissionModulation=comp,
            PathEvaluationResults=Path.Combine(path,"Maps"), SelectedSourceGroup=string.Join(",",groups.Select(id=>"Hour"+id+": "+id)),
            MaxSource=groups.Length,MaxSourceComputed=groups.Length, DecSep=".",Pollutant="NOx",Prefix="",UserText="",
            CalculateMean=true,CellsGralX=10,CellsGralY=10,Horgridsize=20,Slice=1,Slicename="2m" };
        using var worker=new GralBackgroundworkers.ProgressFormBackgroundworker(data);
        Field<System.ComponentModel.BackgroundWorker>(worker,"Rechenknecht").WorkerReportsProgress=true;
        Invoke(worker,"Mean",data,new System.ComponentModel.DoWorkEventArgs(null));
        Check(Directory.GetFiles(Path.Combine(path,"Maps"),"Mean_*.txt").Length==groups.Length+1,"GUI reads all core result groups");
        foreach (int id in new[]{1,300,1295})
        {
            using var zip=System.IO.Compression.ZipFile.OpenRead(Path.Combine(comp,"00001.grz"));
            using var stream=zip.GetEntry("00001-1"+Gral.SourceGroupFileName.Encode(id)+".con").Open();
            using var memory=new MemoryStream();stream.CopyTo(memory);
            byte[] payload=memory.ToArray();
            Check(BitConverter.ToInt32(payload,0)==-3 && payload.Length==428,"binary result shape");
            var grid=Enumerable.Range(0,100).Select(i=>(double)BitConverter.ToSingle(payload,28+4*i)).ToArray();
            Check(grid.All(v=>double.IsFinite(v)&&v>=0),"finite core values");
            Check(id==1295 ? grid.Sum()>0 : grid.Sum()==0,"hour1 active group identity");
            var gui=File.ReadAllLines(Path.Combine(path,"Maps","Mean_NOx_Hour"+id+"_2m.txt")).Skip(6)
                .SelectMany(line=>line.Split(' ',StringSplitOptions.RemoveEmptyEntries)).Select(double.Parse).OrderBy(v=>v).ToArray();
            var expected=grid.OrderBy(v=>v).ToArray();
            Check(gui.Length==100,"GUI result cell count");
            for(int i=0;i<100;i++) Check(Math.Abs(gui[i]-expected[i])<=0.000011,"GUI numeric result mapping");
        }
    }

    [STAThread]
    static int Main(string[] args)
    {
        if (args.Length != 2) { Console.Error.WriteLine("Usage: SourceGroups OUTPUT CORE_DLL|--legacy"); return 2; }
        Output = Path.GetFullPath(args[0]);
        if (Directory.Exists(Output) && Directory.EnumerateFileSystemEntries(Output).Any())
        { Console.Error.WriteLine("Use a new or empty output directory."); return 2; }
        Directory.CreateDirectory(Output);
        Legacy = args.Length > 1 && args[1] == "--legacy";
        CoreDll = args.Length > 1 && !Legacy ? Path.GetFullPath(args[1]) : null;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        using var watchdog = new System.Threading.Timer(_ => Environment.Exit(90), null, 300000, System.Threading.Timeout.Infinite);
        try
        {
            if (Legacy)
            {
                Run("legacy_four_source_files_99", FileIO);
                File.WriteAllText(Path.Combine(Output,"legacy.json"), JsonSerializer.Serialize(new { status="pass", assertions=Assertions }));
                return 0;
            }
            Run("bounded_source_group_storage", Storage);
            Run("windows_safe_filename_protocol", FilenameProtocol);
            Run("four_source_serialization", Serialization);
            Run("sparse_definitions_and_validation", Catalog);
            Run("editor_save_and_reopen_1295", Editor);
            Run("unnamed_imported_source_selection", Selectors);
            Run("four_source_files_302_groups", FileIO);
            Run("modulation_302_groups_and_sparse_headers", Modulation);
            Run("decay_sparse_IDs", Decay);
            Run("receptor_302_computed_301_selected", Receptors);
            Run("GUI_core_GUI_handoff", Handoff);
            File.WriteAllText(Path.Combine(Output, "results.json"), JsonSerializer.Serialize(new { status = "pass", assertions = Assertions, tests = Results }, new JsonSerializerOptions { WriteIndented = true }));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            File.WriteAllText(Path.Combine(Output, "failure.txt"), ex.ToString());return 1;
        }
    }
}
