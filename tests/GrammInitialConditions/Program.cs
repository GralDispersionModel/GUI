using System.IO;
using GralIO;
using Gral.GralMainForms;
using GRAMM_2001;
using System.Reflection;
using System.Globalization;
using System.Text.Json;
using System.Drawing.Imaging;

class Harness
{
    static int checks;
    static void Check(bool result,string message) { checks++; if(!result) throw new Exception(message); }
    static void Reject(Action action,string message) { try{action();}catch(InvalidDataException){checks++;return;}throw new Exception(message); }
    static void Near(double a,double b,string message)=>Check(Math.Abs(a-b)<1e-9,message);
    static Control Find(Control root,string name)=>root.Controls.Find(name,true).Single();
    static void Invoke(object value,string method)=>value.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(value,null);
    [STAThread] static void Main(string[] args)
    {
        string root=Path.GetFullPath(args[0]);Directory.CreateDirectory(root); Directory.SetCurrentDirectory(root);
        string project=Path.Combine(root,"project"),folder=Path.Combine(project,"Computation");Directory.CreateDirectory(folder);
        string iin=Path.Combine(folder,"IIN.dat"),met=Path.Combine(folder,"meteopgt.all"),custom=Path.Combine(folder,"CustomInit.txt");
        File.Copy(args[1],iin);
        var cases=new List<string>();
        foreach(var culture in new[]{"en-US","de-DE","ko-KR"})
        {
            CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
            var initial=GrammInitialConditions.ReadIin(iin,47);
            initial.Latitude=-37.46;initial.AirTemperatureC=22.5;initial.SurfaceTemperatureC=24.1;initial.SoilTemperatureC=18.75;initial.HumidityPercent=73;initial.NeutralHeight=4200;
            var previous=File.ReadAllLines(iin);initial.SaveIin(iin);var loaded=GrammInitialConditions.ReadIin(iin,90);
            Near(loaded.Latitude,-37.46,"IIN latitude");Near(loaded.AirTemperatureC,22.5,"IIN temperature");Near(loaded.HumidityPercent,73,"IIN humidity");
            var after=File.ReadAllLines(iin);foreach(int line in Enumerable.Range(0,previous.Length).Except(new[]{6,8,9,10,11,12,13}))Check(previous[line]==after[line],"unrelated IIN value changed");
            File.WriteAllText(met,"10,1,10\nheader\n27,2,4,0.25\n9,1,7,0.25\n18,3,2,0.5\n");
            var rows=GrammInitialConditions.ReadSituations(met);
            rows[0].Values=new double?[]{1000,-5.5,-3,1.25,-1.5,85,250,-.0065,.012,-.004};
            rows[1].Values=new double?[]{2000,29.75,30.5,20,1.5,55,1000,-.005,.009,-.006};
            GrammInitialConditions.SaveCustom(custom,met,rows);
            var read=GrammInitialConditions.ReadCustom(custom,met);
            for(int r=0;r<rows.Count;r++) for(int c=0;c<10;c++) Check(rows[r].Values[c]==null ? read[r].Values[c]==null : Math.Abs(rows[r].Values[c]!.Value-read[r].Values[c]!.Value)<1e-9,"custom round trip");
            Directory.SetCurrentDirectory(folder);
            var program=typeof(CustomAirSoilInit).Assembly.GetType("GRAMM_2001.Program")!;
            for(int r=0;r<3;r++)
            {
                program.GetField("IWETTER")!.SetValue(null,r+1);
                var engine=new CustomAirSoilInit(280,281,282,.5);
                Near(engine.TAir2m,r==0?267.65:r==1?302.9:280,"GRAMM air kelvin");Near(engine.RelHumidity,r==0?.85:r==1?.55:.5,"GRAMM humidity fraction");
                Near(engine.AirTempGradient,r==0?-.0065:r==1?-.005:-.0065,"GRAMM signed gradient");Check(engine.UserdefinedAirTemp==(r<2),"GRAMM default sentinel");
            }
            Directory.SetCurrentDirectory(root);
            string csv=Path.Combine(root,"template.csv");GrammInitialConditions.ExportCsv(csv,rows);
            var imported=GrammInitialConditions.ReadSituations(met);GrammInitialConditions.ImportCsv(csv,imported);Near(imported[1].Values[1]!.Value,29.75,"CSV units");
            File.WriteAllText(csv,"situation;air_temperature_C;relative_humidity_percent\n3;10;80\n1;20;60\n2;25;65\n");GrammInitialConditions.ImportCsv(csv,imported);Near(imported[0].Values[1]!.Value,20,"CSV explicit ID ordering");
            File.WriteAllText(csv,"situation,air_temperature_C\n1,2\n1,3\n3,4\n");Reject(()=>GrammInitialConditions.ImportCsv(csv,imported),"duplicate ID accepted");Near(imported[0].Values[1]!.Value,20,"partial import changed draft");
            cases.Add("roundtrip_and_actual_engine_"+culture);
        }
        var draft=GrammInitialConditions.ReadSituations(met);
        foreach(double v in new[]{0,-1,101,double.NaN,double.PositiveInfinity}) {draft[0].Values[5]=v;Reject(()=>GrammInitialConditions.SaveCustom(custom,met,draft),"invalid humidity");} draft[0].Values[5]=null;
        draft[0].Values[1]=-273.15;Reject(()=>GrammInitialConditions.SaveCustom(custom,met,draft),"absolute zero");draft[0].Values[1]=null;
        draft[0].Values[7]=0;Reject(()=>GrammInitialConditions.SaveCustom(custom,met,draft),"zero gradient sentinel");draft[0].Values[7]=null;
        string beforeCustom=File.ReadAllText(custom);File.AppendAllText(met,"0,1,4,1\n");Reject(()=>GrammInitialConditions.SaveCustom(custom,met,draft),"weather count drift");Check(File.ReadAllText(custom)==beforeCustom,"invalid save overwrote file");
        File.WriteAllLines(met,File.ReadAllLines(met).Take(5));
        Check(Directory.GetFiles(folder,"IIN.dat.*.bak").Length>=3,"IIN backup");Check(Directory.GetFiles(folder,"CustomInit.txt.*.bak").Length>=2,"custom backup");
        Check(GrammInitialConditions.IsGuiGenerated(custom),"generated input marker");
        string annualMet=Path.Combine(root,"annual_meteopgt.all"),annualCustom=Path.Combine(root,"annual_CustomInit.txt");
        File.WriteAllLines(annualMet,new[]{"10,1,10","header"}.Concat(Enumerable.Range(1,8760).Select(i=>$"{i%36},{1+i%5},4,1")));
        var annual=GrammInitialConditions.ReadSituations(annualMet);
        for(int i=0;i<annual.Count;i++) {annual[i].Values[1]=(i%40)-10;annual[i].Values[5]=50+i%50;}
        GrammInitialConditions.SaveCustom(annualCustom,annualMet,annual);
        var annualRead=GrammInitialConditions.ReadCustom(annualCustom,annualMet);Check(annualRead.Count==8760,"annual rows");
        for(int i=0;i<annual.Count;i++){Near(annualRead[i].Values[1]!.Value,(i%40)-10,"annual temperature");Near(annualRead[i].Values[5]!.Value,50+i%50,"annual humidity");}
        CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;
        using(var form=new GrammInitialConditionsDialog(folder,47,false))
        {
            form.ShowInTaskbar=false;form.Opacity=0;form.Show();Application.DoEvents();form.PerformLayout();
            foreach(var field in new[]{"Latitude","AirTemperatureC","HumidityPercent","NeutralHeight"})Check(Find(form,field).Width>0,"missing input "+field);
            ((TextBox)Find(form,"Latitude")).Text="37.46";((TextBox)Find(form,"AirTemperatureC")).Text="23.25";
            var value=(GrammInitialConditions)form.GetType().GetMethod("GetInitial",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(form,null)!;value.SaveIin(iin);
            var tabs=(TabControl)Find(form,"ConditionsTabs");
            foreach(int width in new[]{1050,850})
            {
                form.Size=new Size(width,740);
                for(int page=0;page<2;page++)
                {
                    tabs.SelectedIndex=page;form.PerformLayout();Application.DoEvents();
                    using var bitmap=new Bitmap(form.Width,form.Height);form.DrawToBitmap(bitmap,new Rectangle(0,0,form.Width,form.Height));bitmap.Save(Path.Combine(root,$"gui_{width}_{page}.png"),ImageFormat.Png);
                }
            }
            var grid=(DataGridView)Find(form,"CustomConditionsGrid");Check(grid.Rows.Count==3 && grid.Columns.Count==14,"grid schema");
            grid.Rows[2].Cells[5].Value="12.5";grid.Rows[2].Cells[9].Value="92";Invoke(form,"SaveCustom");
            var saved=GrammInitialConditions.ReadCustom(custom,met);Near(saved[2].Values[1]!.Value,12.5,"GUI grid temperature");Near(saved[2].Values[5]!.Value,92,"GUI grid humidity");
            Check(!grid.Columns[0].SortMode.Equals(DataGridViewColumnSortMode.Automatic),"sortable weather rows");
        }
        using(var era=new GrammInitialConditionsDialog(folder,47,true)) Check(!Find(era,"SaveCustomInit").Enabled,"ERA5 hourly forcing incorrectly offered");
        using(var main=new Gral.Main())
        {
            Gral.Main.ProjectName=project; main.EmifileReset=false;
            ((NumericUpDown)Find(main,"numericUpDown39")).Value=48;
            main.EmifileReset=true;Invoke(main,"SaveIINDatFile");main.EmifileReset=false;
            var saved=GrammInitialConditions.ReadIin(iin,0);Near(saved.Latitude,37.46,"ordinary control save lost GRAMM latitude");Near(saved.AirTemperatureC,23.25,"ordinary control save lost initial temperature");
            Check(main.Controls.Find("GrammInitialConditions",true).Length==1,"main entry missing");
        }
        File.WriteAllText(Path.Combine(root,"results.json"),JsonSerializer.Serialize(new{status="pass",checks,cases},new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine("PASS GUI "+checks);
    }
}
