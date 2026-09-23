using GralIO;
using GralMainForms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    static readonly List<object> Results = new();
    static string Output;
    static int Assertions;
    static void Check(bool value, string message) { Assertions++; if (!value) throw new Exception(message); }
    static T Field<T>(object instance, string name) => (T)instance.GetType().GetField(name, Private).GetValue(instance);
    static void Set(object instance, string name, object value) => instance.GetType().GetField(name, Private).SetValue(instance, value);
    static object Invoke(object instance, string name, params object[] args)
    {
        try { return instance.GetType().GetMethod(name, Private).Invoke(instance, args); }
        catch (TargetInvocationException e) { throw e.InnerException; }
    }
    static void Mode(string directory)
    {
        try { typeof(Gral.Main).GetMethod("ValidateIndependentGrammMode", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { directory }); }
        catch (TargetInvocationException e) { throw e.InnerException; }
    }
    static string Fixture(string name, int forcing = 0, int format = 0, string meteo = "y")
    {
        string directory = Path.Combine(Output, name);
        Directory.CreateDirectory(directory);
        File.WriteAllLines(Path.Combine(directory, "GRAMMin.dat"), new[] { "Version 17.01", meteo, "0.2", "1,0", "yes", "1008" });
        string[] input = Enumerable.Range(0, 29).Select(n => "unused : 0 ! fixture").ToArray();
        input[25] = "TRANSIENT FORCING : " + forcing + " ! mode";
        input[28] = "Flowfield Output Format : " + format + " ! format";
        File.WriteAllLines(Path.Combine(directory, "IIN.dat"), input);
        return directory;
    }
    static void Rejected(string directory, string fragment)
    {
        Exception error = null;
        try { Mode(directory); } catch (Exception e) { error = e; }
        Check(error is InvalidDataException, "Mode must reject with InvalidDataException");
        Check(error.Message.Contains(fragment, StringComparison.OrdinalIgnoreCase), "Mode rejection must identify cause");
    }
    static void SupportedModes()
    {
        string directory = Fixture("supported_sunrise");
        Mode(directory); Check(true, "Independent Sunrise mode accepted");
        File.WriteAllLines(Path.Combine(directory, "GRAMMin.dat"), new[] { "Y", "0.2", "1" });
        Mode(directory); Check(true, "Legacy uppercase independent mode accepted");
        File.WriteAllLines(Path.Combine(directory, "GRAMMin.dat"), new[] { "Version 17.01", "y", "0.2", "1,0", "yes", "0" });
        Mode(directory); Check(true, "Independent non-Sunrise mode accepted");
    }
    static void UnsupportedModes()
    {
        foreach (int forcing in new[] { 1, 2, 4 }) Rejected(Fixture("forcing_" + forcing, forcing), "Time-dependent");
        Rejected(Fixture("no_header_format", 0, 3), "output format 0");
        Rejected(Fixture("no_meteopgt", 0, 0, "n"), "independent");
    }
    static void InvalidInputs()
    {
        string directory = Fixture("short_iin");
        File.WriteAllText(Path.Combine(directory, "IIN.dat"), "short\n");
        Rejected(directory, "incomplete");
        directory = Fixture("bad_iin");
        string[] lines = File.ReadAllLines(Path.Combine(directory, "IIN.dat")); lines[25] = "missing separator";
        File.WriteAllLines(Path.Combine(directory, "IIN.dat"), lines); Rejected(directory, "Invalid IIN.dat");
        directory = Fixture("locked_iin");
        using (var locked = new FileStream(Path.Combine(directory, "IIN.dat"), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            bool rejected = false; try { Mode(directory); } catch (IOException) { rejected = true; }
            Check(rejected, "Unreadable input must stop preflight");
        }
        directory = Fixture("missing_control"); File.Delete(Path.Combine(directory, "GRAMMin.dat"));
        bool missing = false; try { Mode(directory); } catch (FileNotFoundException) { missing = true; }
        Check(missing, "Missing control must stop preflight");
    }
    static Gral.Main MainFixture(string name)
    {
        string project = Path.Combine(Output, name); Directory.CreateDirectory(Path.Combine(project, "Computation"));
        Gral.Main.ProjectName = project;
        var form = new Gral.Main();
        form.CreateControl();
        Check(!form.InvokeRequired, "Tests run on owning STA thread");
        Field<ProgressBar>(form, "progressBar1").Maximum = 1000;
        Field<ProgressBar>(form, "progressBar2").Maximum = 100;
        return form;
    }
    static void UiUpdateOwnership()
    {
        using var form = MainFixture("direct_updates");
        Field<Label>(form,"label66").Text="queue pending 100";
        Field<Label>(form,"label67").Text="queue completed 900 / 1000";
        Field<ProgressBar>(form,"progressBar1").Value=900;
        Field<ProgressBar>(form,"progressBar2").Value=7;
        using var token = new CancellationTokenSource(); Set(form,"grammResumeCancellation",token);
        Invoke(form,"UpdateLabel66","stale first instance"); Invoke(form,"UpdateLabel67","stale percent");
        Invoke(form,"UpdateProgressBar1",90); Invoke(form,"UpdateProgressBar2",99);
        Check(Field<Label>(form,"label66").Text=="queue pending 100","Pending label protected");
        Check(Field<Label>(form,"label67").Text=="queue completed 900 / 1000","Completion label protected");
        Check(Field<ProgressBar>(form,"progressBar1").Value==900,"Aggregate count protected");
        Check(Field<ProgressBar>(form,"progressBar2").Value==7,"Per-instance percent blocked");
        Set(form,"grammResumeCancellation",null);
        Invoke(form,"UpdateLabel66","legacy situation"); Invoke(form,"UpdateLabel67","legacy percent");
        Invoke(form,"UpdateProgressBar1",90); Invoke(form,"UpdateProgressBar2",99);
        Check(Field<Label>(form,"label66").Text=="legacy situation","Legacy pending label restored");
        Check(Field<Label>(form,"label67").Text=="legacy percent","Legacy percent restored");
        Check(Field<ProgressBar>(form,"progressBar1").Value==90,"Legacy progress restored");
        Check(Field<ProgressBar>(form,"progressBar2").Value==99,"Legacy detail progress restored");
    }
    static void WatcherOwnership()
    {
        using var form = MainFixture("watcher_updates");
        string directory=Path.Combine(Gral.Main.ProjectName,"Computation");
        File.WriteAllText(Path.Combine(directory,"DispNrGramm.txt"),"91\n");
        File.WriteAllText(Path.Combine(directory,"PercentGramm.txt"),"5\n");
        Field<Label>(form,"label66").Text="queue pending";Field<Label>(form,"label67").Text="queue completed";
        Field<ProgressBar>(form,"progressBar1").Value=900;Field<ProgressBar>(form,"progressBar2").Value=7;
        using var token = new CancellationTokenSource();Set(form,"grammResumeCancellation",token);
        Invoke(form,"DispnrGrammChanged",null,new FileSystemEventArgs(WatcherChangeTypes.Changed,directory,"DispNrGramm.txt"));
        Invoke(form,"PercentGrammChanged",null,new FileSystemEventArgs(WatcherChangeTypes.Changed,directory,"PercentGramm.txt"));
        Check(Field<Label>(form,"label66").Text=="queue pending","DispNr callback blocked");
        Check(Field<Label>(form,"label67").Text=="queue completed","Percent callback blocked");
        Check(Field<ProgressBar>(form,"progressBar1").Value==900,"Watcher aggregate unchanged");
        Check(Field<ProgressBar>(form,"progressBar2").Value==7,"Watcher detail unchanged");
        Set(form,"grammResumeCancellation",null);
        File.WriteAllText(Path.Combine(directory,"DispNrGramm.txt"),"1\n");
        Invoke(form,"DispnrGrammChanged",null,new FileSystemEventArgs(WatcherChangeTypes.Changed,directory,"DispNrGramm.txt"));
        Check(Field<ProgressBar>(form,"progressBar1").Value==0,"Single-instance watcher still updates");
        Check(Field<Label>(form,"label67").Text=="Actual flow situation: 0 %","Single-instance status preserved");
    }
    static void CancellationHooks()
    {
        using var form=MainFixture("cancellation_hooks");
        string control=Path.Combine(Gral.Main.ProjectName,"Computation","GRAMMin.dat");
        File.WriteAllText(control,"preserved control bytes\r\n");
        Field<NumericUpDown>(form,"numericUpDown24").Value=91;
        foreach(string method in new[]{"GRAMMStopCalculation","GRAMMPauseCalculation"})
        {
            using var token=new CancellationTokenSource();Set(form,"grammResumeCancellation",token);
            Invoke(form,method,null,EventArgs.Empty);
            Check(token.IsCancellationRequested,method+" cancels entire queue");
            Check(Field<NumericUpDown>(form,"numericUpDown24").Value==91,method+" does not reset obsolete first-instance counter");
            Check(File.ReadAllText(control)=="preserved control bytes\r\n",method+" preserves scientific controls");
            Set(form,"grammResumeCancellation",null);
        }
    }
    static void ScientificInputLock()
    {
        using var form=MainFixture("input_lock");
        using var token=new CancellationTokenSource();Set(form,"grammResumeCancellation",token);
        form.GRAMM_Locked=true;Invoke(form,"Gramm_locked_buttonClick",null,null);
        Check(!Field<NumericUpDown>(form,"numericUpDown21").Enabled,"Integration time disabled by input lock");
        Check(!Field<NumericUpDown>(form,"numericUpDown33").Enabled,"Instance count disabled while locking");
        Check(!Field<CheckBox>(form,"checkBox30").Enabled,"Sunrise disabled by input lock");
        Check(form.GRAMM_Locked,"Scientific input lock remains set");
        Set(form,"grammResumeCancellation",null);
    }
    static GrammProgressSnapshot DashboardSnapshot(int count)
    {
        var instances=Enumerable.Range(1,count).Select(id=>new GrammInstanceProgress(id,
            GrammInstanceState.Running,
            90+(id-1)*100+1,100+(id-1)*100,id%7,id%7,4000+id,TimeSpan.FromMinutes(id),new GrammConsoleStatus {Available=true,
                Situation=90+(id-1)*100+1+id%7,SimulationSeconds=1200+id*120,TargetSeconds=21600,Text="Test console output"})).ToArray();
        return new GrammProgressSnapshot(count*100,count*90,instances.Sum(x=>x.Completed),0,instances);
    }
    static IEnumerable<Control> Descendants(Control parent)
    {
        yield return parent;
        foreach(Control child in parent.Controls)
        foreach(Control item in Descendants(child)) yield return item;
    }
    static void RenderReady(Control control)
    {
        // Native progress bars animate toward their new values before a screenshot is representative.
        var clock=Stopwatch.StartNew();
        while(clock.ElapsedMilliseconds<650) {Application.DoEvents();Thread.Sleep(10);}
        control.Refresh();
    }
    static void DashboardLayouts()
    {
        foreach(int count in new[]{1,2,4,8,10,32,64})
        foreach(int width in new[]{420,640,900,1240,1380})
        {
            using var form=new GrammProgressForm("GRAMM Incheon Allsituation",count);
            form.ClientSize=new Size(width,760);form.StartPosition=FormStartPosition.Manual;
            form.Location=new Point(-32000,-32000);form.Opacity=0;form.Show();
            form.Apply(DashboardSnapshot(count)); form.PerformLayout();
            using var bitmap=new Bitmap(form.Width,form.Height); form.DrawToBitmap(bitmap,new Rectangle(0,0,form.Width,form.Height));
            Check(form.InstanceCards.Count==count,"Every configured instance has a card");
            Check(form.BackColor==SystemColors.Control,"Dashboard follows the original system control background");
            Check(form.Font.Name=="Segoe UI"&&form.Font.Size==9,"Dashboard uses the original body font");
            Check(form.InstanceCards.All(c=>c is GroupBox&&c.BackColor==SystemColors.Control),"Instances use native GRAL-style group boxes");
            Check(form.Controls.Find("overall_summary",true)[0] is GroupBox,"Summary uses a native group box");
            Check(form.Controls.Find("overall_progress",true)[0] is ProgressBar,"Overall progress uses the native progress bar");
            Check(form.InstanceCards.All(c=>c.Controls["instance_progress"] is ProgressBar),"Every instance uses a native progress bar");
            Check(form.InstanceCards.All(c=>c.Controls["instance_details"] is Button),"Details uses a standard button");
            Check(Descendants(form).All(c=>!System.Text.RegularExpressions.Regex.IsMatch(c.Text,@"[가-힣]")),"All supplied dashboard labels are English");
            Check(form.ColumnCount>=1&&form.ColumnCount<=4,"One to four responsive columns");
            Check(form.InstanceCards.Select(c=>c.Left).Distinct().Count()==Math.Min(form.ColumnCount,count),"Actual wrapping matches column count");
            var panel=(FlowLayoutPanel)form.Controls.Find("instance_cards",true)[0];
            Check(!panel.HorizontalScroll.Visible,"No horizontal scrolling");
            Check(form.InstanceCards.All(c=>c.Right<=panel.ClientSize.Width),"No clipped cards");
            Check(form.InstanceCards.All(c=>c.Controls.Cast<Control>().All(child=>child.Right<=c.ClientSize.Width&&child.Bottom<=c.ClientSize.Height)),"Card contents stay inside card");
            Check(form.Controls.Find("overall_completed",true)[0].Text.Contains("Completed:"),"Overall completion visible");
            Check(form.Controls.Find("overall_remaining",true)[0].Text.Contains("Queued:"),"Unassigned work visible");
            Check(form.InstanceCards.All(c=>c.Controls["instance_current"].Text.Contains("Current situation:")),"Each instance shows current weather progress");
            Check(form.InstanceCards.All(c=>c.Controls["instance_details"].Enabled),"Each instance exposes its console detail");
            Check(form.InstanceCards.All(c=>c.Controls["instance_cpu"].Text.StartsWith("CPU: ")),"Every instance shows CPU placement");
            Check(form.InstanceCards.All(c=>c.Controls["instance_session"].Text.StartsWith("Current range: ")),"Current range is distinct from total assignment");
            if((count==10&&(width==420||width==1380))||(count==64&&width==900)||(count==1&&width==640))
            {
                RenderReady(form);form.DrawToBitmap(bitmap,new Rectangle(0,0,form.Width,form.Height));
                bitmap.Save(Path.Combine(Output,$"dashboard_{count}_{width}.png"),ImageFormat.Png);
            }
        }
    }
    static void DashboardActions()
    {
        using var form=new GrammProgressForm("Action test",8);
        form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-32000,-32000);form.Opacity=0;form.Show();
        form.ShowChecking(400,1008);
        Check(form.Controls.Find("overall_completed",true)[0].Text.Contains("Checked: 400 / 1,008"),"Scan progress is distinct from completion");
        form.Apply(DashboardSnapshot(8));
        var firstCard=form.InstanceCards[0];
        ((Button)firstCard.Controls["instance_details"]).PerformClick();
        var console=Field<Form>(firstCard,"detailForm");
        Check(console!=null&&console.Text=="Instance 01 - Recent console output","Details opens the instance console in English");
        var consoleText=Field<TextBox>(firstCard,"detailText");
        Check(consoleText.ReadOnly&&consoleText.Text.Contains("Test console output"),"Console detail remains read-only");
        Check(consoleText.Text.Contains("Assigned situations:")&&consoleText.Text.Contains("CPU:"),"Details includes assignment and CPU placement");
        console.Close();
        int requests=0;form.PauseRequested+=(s,e)=>requests++;
        var pause=(Button)form.Controls.Find("pause_all",true)[0];
        pause.PerformClick();
        Check(requests==1,"Pause delegates to owned queue once");Check(!pause.Enabled,"Pause cannot be submitted twice");
        form.Finish("Paused");Check(form.Controls.Find("run_phase",true)[0].Text=="Paused","Terminal status stays visible");
        form.Apply(DashboardSnapshot(8));Check(form.Controls.Find("run_phase",true)[0].Text=="Paused","Late progress cannot replace terminal status");
        var closing=new FormClosingEventArgs(CloseReason.UserClosing,false);
        typeof(Form).GetMethod("OnFormClosing",Private).Invoke(form,new object[]{closing});
        Check(closing.Cancel,"Close hides dashboard without disposing it");Check(requests==1,"Close does not cancel computation");
        foreach(var state in Enum.GetValues<GrammInstanceState>())
        {
            form.Apply(new GrammProgressSnapshot(10,0,2,0,new[]{new GrammInstanceProgress(1,state,1,10,2,2,0,TimeSpan.Zero)}));
            Check(form.InstanceCards[0].AccessibleName.Contains("Remaining: 8"),"Accessible remaining count in "+state);
        }
    }
    static void DashboardMainIntegration()
    {
        using var form=MainFixture("dashboard_main");
        Invoke(form,"SetGrammProgressLayout",true);
        var button=Field<Button>(form,"grammProgressButton");
        Check(button.Parent==Field<GroupBox>(form,"groupBox15"),"Dashboard button belongs to GRAMM controls");
        Check(button.Width>180,"Dashboard button uses available width");
        Check(button.Text=="Instance progress","Main dashboard button is English");
        Check(button.Font.Equals(Field<Label>(form,"label67").Font),"Main dashboard button preserves the existing control font");
        Invoke(form,"UpdateGrammSnapshot",DashboardSnapshot(10));
        Check(Field<ProgressBar>(form,"progressBar1").Value==927,"Main progress aggregates every slot");
        Check(Field<Label>(form,"label66").Text=="Missing situations: 73","Main remaining count is aggregate");
        Invoke(form,"SetGrammProgressLayout",false);
        Check(!button.Visible,"Single instance retains existing layout");
        Invoke(form,"SetGrammProgressLayout",true);
        using var preview=new Form {ClientSize=new Size(230,305),StartPosition=FormStartPosition.Manual,Location=new Point(-32000,-32000),Opacity=0,ShowInTaskbar=false};
        var group=Field<GroupBox>(form,"groupBox15");preview.Controls.Add(group);group.Location=new Point(6,6);group.Visible=true;Gral.Main.LoopAllControls(preview.Controls);preview.Show();
        Check(group.Visible&&button.Visible,"Main summary preview shows the actual GRAMM controls");
        using var image=new Bitmap(preview.Width,preview.Height);preview.DrawToBitmap(image,new Rectangle(0,0,preview.Width,preview.Height));
        image.Save(Path.Combine(Output,"main_gramm_summary.png"),ImageFormat.Png);
    }
    static GrammProgressSnapshot BalancedSnapshot(bool nextRange=false, int thirdCompleted=0, string thirdCpu="Performance class 0; CPU sets 6, 7, 8")
    {
        int[] first={739,749,759,865,875,885,990,1000};
        int[] last={748,758,762,874,884,889,999,1008};
        string[] ranges={"739-748","749-758","759-762; 859-864","865-874","875-884","885-889; 985-989","990-999","1,000-1,008"};
        if(nextRange) { first[2]=859;last[2]=864; }
        var slots=Enumerable.Range(0,8).Select(i=>new GrammInstanceProgress(i+1,GrammInstanceState.Running,
            first[i],last[i],i==2?thirdCompleted:0,i==2?thirdCompleted:0,7000+i,TimeSpan.FromMinutes(7),
            new GrammConsoleStatus {Available=true,Situation=first[i],SimulationSeconds=1620,TargetSeconds=21600,Text="Synthetic console output for UI validation"},
            assigned:i==7?9:10,assignedRanges:ranges[i],cpuAssignment:i==2?thirdCpu:"Automatic; CPU sets "+(i*3+1)+", "+(i*3+2)+", "+(i*3+3))).ToArray();
        return new GrammProgressSnapshot(1008,929,thirdCompleted,0,slots);
    }
    static void BalancedAssignmentDashboard()
    {
        using var form=new GrammProgressForm("GRAMM Incheon Allsituation - UI validation",8);
        form.ClientSize=new Size(1380,760);form.StartPosition=FormStartPosition.Manual;
        form.Location=new Point(-32000,-32000);form.Opacity=0;form.Show();
        form.Apply(BalancedSnapshot());form.PerformLayout();
        Check(form.InstanceCards.Select(c=>c.Progress.Assigned).SequenceEqual(new[]{10,10,10,10,10,10,10,9}),"79 situations display evenly across eight instances");
        Check(form.InstanceCards.Sum(c=>c.Progress.Remaining)==79,"All pending work belongs to an instance");
        Check(form.Controls.Find("overall_remaining",true)[0].Text.Contains("Queued: 0"),"No undistributed work is hidden behind the visible instances");
        Check(form.InstanceCards[2].Controls["instance_range"].Text=="Situations: 759-762; 859-864","Third assignment displays both separate ranges");
        Check(form.InstanceCards[5].Controls["instance_range"].Text=="Situations: 885-889; 985-989","Sixth assignment displays both separate ranges");
        Check(form.InstanceCards[7].Controls["instance_remaining"].Text=="Remaining: 9","Remainder instance displays nine situations");
        RenderReady(form);
        using(var image=new Bitmap(form.Width,form.Height))
        { form.DrawToBitmap(image,new Rectangle(0,0,form.Width,form.Height));image.Save(Path.Combine(Output,"dashboard_balanced_79.png"),ImageFormat.Png); }
        form.Apply(BalancedSnapshot(false,4));
        var third=form.InstanceCards[2];
        Check(third.Controls["instance_completed"].Text=="Completed: 4 / 10","Completed first range counts toward total assignment");
        int before=((ProgressBar)third.Controls["instance_progress"]).Value;
        form.Apply(BalancedSnapshot(true,4,"Performance class 1; CPU sets 1, 2, 3"));
        Check(third.Controls["instance_completed"].Text=="Completed: 4 / 10","Completion does not reset on the second range");
        Check(third.Controls["instance_remaining"].Text=="Remaining: 6","Remaining count survives range transition");
        Check(((ProgressBar)third.Controls["instance_progress"]).Value==before&&before==4,"Progress bar preserves completed work across ranges");
        Check(third.Controls["instance_session"].Text=="Current range: 859-864","Current range advances without replacing full assignment");
        Check(third.Controls["instance_cpu"].Text.Contains("Performance class 1"),"CPU placement refreshes while the process keeps running");
        Check(third.Progress.ProcessId==7002,"Changing displayed CPU placement preserves process identity");
        Check(form.Controls.Find("overall_completed",true)[0].Text.Contains("933"),"Aggregate progress includes the completed first range");
        ((Button)third.Controls["instance_details"]).PerformClick();
        var detail=Field<TextBox>(third,"detailText");
        Check(detail.ReadOnly&&detail.Text.Contains("759-762; 859-864")&&detail.Text.Contains("CPU sets 1, 2, 3"),"Details preserves full assignment and current CPU placement");
        Field<Form>(third,"detailForm").Close();
        form.Apply(BalancedSnapshot(true,5));
        Check(third.Controls["instance_completed"].Text=="Completed: 5 / 10","Second-range completion increments the existing total");
    }
    static GrammCpuPolicy CpuPolicy()
    {
        return (GrammCpuPolicy)typeof(Gral.Main).GetMethod("ReadGrammCpuPolicy",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
    }
    static void CpuDialog(Gral.Main owner,int expected,int selection,bool save,bool render=false)
    {
        Exception failure=null;bool handled=false;
        using var timer=new System.Windows.Forms.Timer {Interval=1};
        timer.Tick+=(sender,e)=>
        {
            var dialog=Application.OpenForms.Cast<Form>().FirstOrDefault(f=>f.Text=="GRAMM CPU placement");
            if(dialog==null)return;
            timer.Stop();handled=true;
            dialog.Opacity=0;dialog.StartPosition=FormStartPosition.Manual;dialog.Location=new Point(-32000,-32000);
            try
            {
                var choice=(ComboBox)dialog.Controls["cpu_policy"];
                Check(choice.SelectedIndex==expected,"CPU dialog restores the selected policy");
                Check(choice.Items.Cast<string>().SequenceEqual(new[]{"Automatic - performance / preferred cores","Prefer larger L3 cache","Windows scheduling"}),"CPU dialog exposes three English policies");
                Check(choice.DropDownStyle==ComboBoxStyle.DropDownList,"CPU selection rejects free-form values");
                Check(dialog.BackColor==SystemColors.Control&&dialog.Font.Name=="Segoe UI","CPU dialog follows the native GRAL style");
                Check(Descendants(dialog).All(c=>!System.Text.RegularExpressions.Regex.IsMatch(c.Text,@"[가-힣]")),"CPU dialog labels remain English");
                Check(dialog.Controls.Cast<Control>().All(c=>c.Right<=dialog.ClientSize.Width&&c.Bottom<=dialog.ClientSize.Height),"CPU dialog controls remain within its bounds");
                choice.SelectedIndex=selection;
                if(render)
                {
                    using var image=new Bitmap(dialog.Width,dialog.Height);dialog.DrawToBitmap(image,new Rectangle(0,0,dialog.Width,dialog.Height));
                    image.Save(Path.Combine(Output,"cpu_placement_dialog.png"),ImageFormat.Png);
                }
                ((Button)(save?dialog.AcceptButton:dialog.CancelButton)).PerformClick();
            }
            catch(Exception ex) { failure=ex;dialog.DialogResult=DialogResult.Cancel;dialog.Close(); }
        };
        timer.Start();Invoke(owner,"ShowGrammCpuOptions");timer.Stop();
        if(failure!=null)throw failure;
        Check(handled,"CPU dialog was exercised through its actual modal flow");
    }
    static void CpuSettingsAndMainIntegration()
    {
        using var form=MainFixture("cpu_settings_main");
        form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-32000,-32000);form.Opacity=0;form.ShowInTaskbar=false;
        string previous=Gral.Main.GUISettings.AppSettingsPath;
        string settings=Path.Combine(Output,"cpu_policy_settings");Directory.CreateDirectory(settings);
        string path=Path.Combine(settings,"GRAMM_cpu_policy.txt");
        try
        {
            Gral.Main.GUISettings.AppSettingsPath=settings;
            Check(CpuPolicy()==GrammCpuPolicy.Automatic,"Missing preference defaults to automatic scheduling");
            CpuDialog(form,0,1,true,true);
            Check(File.ReadAllText(path)==((GrammCpuPolicy)1).ToString()&&CpuPolicy()==(GrammCpuPolicy)1,"Larger-cache preference survives save and reread");
            Check(Directory.GetFiles(settings,"*.tmp").Length==0,"Saving leaves no temporary settings file");
            string saved=File.ReadAllText(path);
            CpuDialog(form,1,2,false);
            Check(File.ReadAllText(path)==saved&&CpuPolicy()==(GrammCpuPolicy)1,"Cancel preserves the previous preference");
            CpuDialog(form,1,2,true);
            Check(CpuPolicy()==(GrammCpuPolicy)2,"Windows scheduling survives save and reread");
            foreach(string invalid in new[]{"unknown setting","99","-1",""})
            { File.WriteAllText(path,invalid);Check(CpuPolicy()==GrammCpuPolicy.Automatic,"Malformed preference falls back to automatic: "+invalid); }
            CpuDialog(form,0,0,true);
            Check(CpuPolicy()==GrammCpuPolicy.Automatic,"Automatic preference can be saved explicitly");
            var group=Field<GroupBox>(form,"groupBox31");var button=Field<Button>(form,"grammCpuButton");
            Check(button.Parent==group&&button.Text=="CPU...","CPU settings button belongs to the existing processor controls");
            Check(button.Right<=group.ClientSize.Width&&button.Bottom<=group.ClientSize.Height,"CPU settings button stays inside its existing group");
            Check(!button.Bounds.IntersectsWith(Field<CheckBox>(form,"checkBoxAVX").Bounds),"CPU settings button does not overlap the AVX option");
            Check(!group.Controls.Cast<Control>().Any(c => c != button && button.Bounds.IntersectsWith(c.Bounds)),"CPU settings button does not overlap NUMA or other processor controls");
            using var preview=new Form {ClientSize=new Size(group.Width+12,group.Height+12),StartPosition=FormStartPosition.Manual,Location=new Point(-32000,-32000),Opacity=0,ShowInTaskbar=false};
            preview.Controls.Add(group);group.Location=new Point(6,6);group.Visible=true;Gral.Main.LoopAllControls(preview.Controls);preview.Show();
            Check(button.Visible,"CPU settings button is visible with the processor controls");
            using var image=new Bitmap(preview.Width,preview.Height);preview.DrawToBitmap(image,new Rectangle(0,0,preview.Width,preview.Height));
            image.Save(Path.Combine(Output,"main_cpu_settings.png"),ImageFormat.Png);
        }
        finally { Gral.Main.GUISettings.AppSettingsPath=previous; }
        Check(Gral.Main.GUISettings.AppSettingsPath==previous,"Tests restore the original settings location");
    }
    static void Run(string name,Action test)
    {
        int before=Assertions;
        try { test();Results.Add(new{name,status="pass",assertions=Assertions-before});Console.WriteLine("PASS "+name); }
        catch(Exception e){Results.Add(new{name,status="fail",error=e.ToString(),assertions=Assertions-before});Console.WriteLine("FAIL "+name+" "+e.Message);}
    }
    [STAThread]
    static int Main(string[] args)
    {
        Output=Path.GetFullPath(args.Length>0?args[0]:"results");Directory.CreateDirectory(Output);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Run("supported_independent_modes",SupportedModes);Run("reject_time_dependent_and_headerless_modes",UnsupportedModes);
        Run("invalid_or_unreadable_inputs",InvalidInputs);Run("queue_owns_progress_updates",UiUpdateOwnership);
        Run("queue_ignores_shared_status_watchers",WatcherOwnership);Run("stop_and_pause_cancel_queue_preserve_controls",CancellationHooks);
        Run("scientific_input_lock",ScientificInputLock);
        Run("responsive_instance_layouts",DashboardLayouts);Run("dashboard_pause_close_and_states",DashboardActions);
        Run("dashboard_main_integration",DashboardMainIntegration);
        Run("balanced_assignments_and_range_transition",BalancedAssignmentDashboard);
        Run("cpu_settings_and_main_integration",CpuSettingsAndMainIntegration);
        string serialized=JsonSerializer.Serialize(Results);string status=serialized.Contains("\"fail\"")?"fail":"pass";
        string assembly=typeof(Gral.Main).Assembly.Location;
        File.WriteAllText(Path.Combine(Output,"gui_results.json"),JsonSerializer.Serialize(new{status,tests=Results.Count,assertions=Assertions,assembly,assembly_sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly))),results=Results},new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine(status.ToUpperInvariant()+" "+Results.Count+" tests "+Assertions+" assertions");
        return status=="pass"?0:1;
    }
}
