using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

internal static class Program
{
    static Assembly gui;
    static string output;
    static List<object> results = new();
    static int assertions;
    static void Check(bool value, string message) { assertions++; if (!value) throw new Exception(message); }
    static void Set(object obj, string key, object val) { var type = obj.GetType(); var f = type.GetField(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); if (f != null) f.SetValue(obj, val); else type.GetProperty(key).SetValue(obj, val); }
    static object Get(object obj, string key) => obj.GetType().GetField(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj);
    static void Test(string name, Action body) { try { body(); results.Add(new { name, status = "pass" }); Console.WriteLine("PASS " + name); } catch (Exception ex) { results.Add(new { name, status = "fail", error = ex.ToString() }); Console.WriteLine("FAIL " + name + " " + ex.Message); } }
    static string Render(string name, float u, float v, float w, int mode = 0, double scale = 15, bool degenerate = false, bool expectOverflow = false)
    {
        var data = Activator.CreateInstance(gui.GetType("GralDomForms.WindfieldSectionDrawings"));
        Set(data, "X0", 0d); Set(data, "Y0", 0d); Set(data, "X1", 400d); Set(data, "Y1", 0d);
        Set(data, "cellsize", 0d); Set(data, "GrammCellsize", 100d); Set(data, "WindSectorSize", 10d);
        Set(data, "Path", output); Set(data, "GRAMM_path", Path.Combine(output, "missing", "ggeom.asc")); Set(data, "PROJECT_path", output);
        using var form = (Form)Activator.CreateInstance(gui.GetType("GralDomForms.Sectiondrawing"), data);
        Set(data, "GrammCellsize", 100d); Set(data, "Nkk", 2); if (degenerate) Set(data, "X1", 0d);
        var picture = (PictureBox)Get(form, "section_picture"); picture.Size = new Size(850, 430);
        Set(form, "GRAMMsurface", new List<float> { 20, 20, 20, 20 });
        Set(form, "GRAMMcell", Enumerable.Range(0, 12).Select(i => 20f + (i % 3) * 85).ToList());
        Set(form, "Uw", Enumerable.Repeat(u, 12).ToList()); Set(form, "Vw", Enumerable.Repeat(v, 12).ToList()); Set(form, "Ww", Enumerable.Repeat(w, 12).ToList());
        Set(form, "GRAMM_cellsize_ff", 1f); Set(form, "GRAL_cellsize_ff", 0f); Set(form, "Ws_factor", scale);
        Set(form, "VerticalOffset", 0); Set(form, "VerticalFactor", 1.5d); Set(form, "HorizontalFactor", 1d); Set(form, "HorOffset", 0);
        ((CheckBox)Get(form, "show_gramm")).Checked = true; ((CheckBox)Get(form, "show_gral")).Checked = false; ((CheckBox)Get(form, "show_buildings")).Checked = false;
        ((DomainUpDown)Get(form, "domainUpDown1")).SelectedIndex = mode;
        using var bitmap = new Bitmap(850, 430); using var graph = Graphics.FromImage(bitmap); graph.Clear(Color.White);
        try { gui.GetType("GralDomForms.Sectiondrawing").GetMethod("Section_picturePaint", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, new object[] { picture, new PaintEventArgs(graph, new Rectangle(0, 0, 850, 430)) }); }
        catch (TargetInvocationException ex) when (expectOverflow && ex.InnerException is OverflowException)
        {
            File.WriteAllText(Path.Combine(output, name + "_expected_exception.txt"), ex.InnerException.ToString()); return "expected_overflow";
        }
        Check(!expectOverflow, "Original calm wind did not reproduce OverflowException");
        if (name.Contains("invalid_scale"))
        {
            gui.GetType("GralDomForms.Sectiondrawing").GetMethod("Wind_data_pictureboxPaint", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(form, new object[] { picture, new PaintEventArgs(graph, new Rectangle(0, 0, 850, 430)) });
        }
        var path = Path.Combine(output, name + ".png"); bitmap.Save(path, ImageFormat.Png);
        if (name.Contains("invalid") || name.Contains("degenerate") || name.Contains("zoom"))
        {
            int red = 0; for (int y = 10; y < 25; y++) for (int x = 35; x < 700; x++) { var c = bitmap.GetPixel(x, y); if (c.R > 70 && c.G < 80 && c.B < 80) red++; }
            Check(red > 20, "Missing on-canvas invalid-vector warning");
        }
        return Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    }
    static void MathTests()
    {
        Type math = gui.GetType("GralDomForms.SectionWindMath"); Check(math != null, "missing patch math");
        var project = math.GetMethod("TryProject", BindingFlags.Static | BindingFlags.NonPublic);
        var random = new Random(2601); double maxError = 0;
        for (int i = 0; i < 10000; i++)
        {
            double dx = random.NextDouble() * 2000 - 1000, dy = random.NextDouble() * 2000 - 1000, u = random.NextDouble() * 60 - 30, v = random.NextDouble() * 60 - 30;
            object[] a = { dx, dy, u, v, 0d, 0d }; Check((bool)project.Invoke(null, a), "finite projection rejected");
            double angle = Vector3D.AngleBetween(new Vector3D(dx, dy, 0), new Vector3D(u, v, 0)) * Math.PI / 180;
            double speed = Math.Sqrt(u * u + v * v), oldAlong = speed * Math.Cos(angle), oldAcross = speed * Math.Sin(angle);
            maxError = Math.Max(maxError, Math.Max(Math.Abs((double)a[4] - oldAlong), Math.Abs((double)a[5] - oldAcross)));
            Check(Math.Abs((double)a[4] - oldAlong) < 1e-10 && Math.Abs((double)a[5] - oldAcross) < 1e-10, "projection differs from legacy finite wind");
        }
        // The legacy MONO angle avoids AngleBetween's undefined zero-vector angle.
        Check(Math.Atan2(0d, 0d) == 0d, "MONO calm angle");
        foreach (double transverse in new[] { -4d, 4d })
        {
            double angle = Math.Atan2(100 * transverse, 100 * 3d);
            double monoAcross = 5d * Math.Sin(angle);
            object[] a = { 100d, 0d, 3d, transverse, 0d, 0d };
            Check((bool)project.Invoke(null, a), "MONO comparison rejected");
            Check(Math.Abs((double)a[4] - 5d * Math.Cos(angle)) < 1e-12, "MONO along changed");
            Check(Math.Abs((double)a[5] - Math.Abs(monoAcross)) < 1e-12, "Windows transverse colour changed");
        }
        File.WriteAllText(Path.Combine(output, "projection_metrics.json"), JsonSerializer.Serialize(new { cases = 10000, maximum_absolute_error_m_s = maxError }));
        foreach (var values in new[] { new[] { 100d, 0d, 0d, 0d }, new[] { 1e300, 1e300, 0d, 0d }, new[] { 1e-300, 0d, 5d, 0d } }) { object[] a = { values[0], values[1], values[2], values[3], 0d, 0d }; Check((bool)project.Invoke(null, a), "calm/scaled axis rejected"); }
        foreach (var values in new[] { new[] { 0d, 0d, 5d, 1d }, new[] { 100d, 0d, double.NaN, 1d }, new[] { double.PositiveInfinity, 1d, 0d, 0d } }) { object[] a = { values[0], values[1], values[2], values[3], 0d, 0d }; Check(!(bool)project.Invoke(null, a), "invalid input accepted"); }
        var pixel = math.GetMethod("TryCoordinate", BindingFlags.Static | BindingFlags.NonPublic);
        foreach (double value in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, (double)int.MaxValue + 1, (double)int.MinValue - 1 }) { object[] a = { value, 0 }; Check(!(bool)pixel.Invoke(null, a), "invalid coordinate accepted"); }
        foreach (double value in new[] { 0d, 0.5, 1.5, -1.5, (double)int.MaxValue, (double)int.MinValue }) { object[] a = { value, 0 }; Check((bool)pixel.Invoke(null, a) && (int)a[1] == Convert.ToInt32(value), "rounding changed"); }
    }
    [STAThread]
    static int Main(string[] args)
    {
        string dll = Path.GetFullPath(args[0]); output = Path.GetFullPath(args[1]); Directory.CreateDirectory(output); Directory.SetCurrentDirectory(output);
        gui = Assembly.LoadFrom(dll); bool patched = args[2] == "patched"; bool monoOnly = args[2] == "mono-only";
        Test("finite_projection_paint", () => Render("finite_projection", 300, 100, 25));
        Test("finite_sum_paint", () => Render("finite_sum", 300, 100, 25, 1));
        Test("calm_horizontal", () => Render("calm", 0, 0, 0, expectOverflow: !patched && !monoOnly));
        Test("vertical_only", () => Render("vertical", 0, 0, 100, expectOverflow: !patched && !monoOnly));
        if (monoOnly)
        {
            Test("mono_invalid_nan_still_overflows", () => Render("mono_nan", float.NaN, 100, 0, expectOverflow: true));
            Test("mono_excessive_zoom_still_overflows", () => Render("mono_scale", 300, 100, 0, scale: 1e200, expectOverflow: true));
        }
        if (patched)
        {
            Test("calm_sum", () => Render("calm_sum", 0, 0, 0, 1));
            Test("invalid_nan", () => Render("invalid_nan", float.NaN, 100, 0));
            Test("invalid_infinity", () => Render("invalid_inf", float.PositiveInfinity, 100, 0));
            Test("invalid_vertical", () => Render("invalid_vertical", 300, 100, float.NaN));
            Test("invalid_sum", () => Render("invalid_sum", float.PositiveInfinity, 100, 0, 1));
            Test("degenerate_section", () => Render("degenerate", 300, 100, 0, degenerate: true));
            Test("excessive_zoom", () => Render("zoom", 300, 100, 0, scale: 1e200));
            Test("invalid_scale", () => Render("invalid_scale", 300, 100, 0, scale: double.NaN));
            Test("infinite_scale_and_legend", () => Render("invalid_scale_inf", 300, 100, 0, scale: double.PositiveInfinity));
            Test("finite_math_10000_and_bounds", MathTests);
        }
        var report = new { status = results.Any(x => JsonSerializer.Serialize(x).Contains("\"fail\"")) ? "fail" : "pass", dll, dll_sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(dll))), runtime = Environment.Version.ToString(), tests = results, assertions };
        File.WriteAllText(Path.Combine(output, "test_results.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        return report.status == "pass" ? 0 : 1;
    }
}
