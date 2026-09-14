// Section wind projection. GPL-3.0, same license as GRAL GUI.
using System;

namespace GralDomForms
{
    internal static class SectionWindMath
    {
        // Avoid sending unbounded wind-arrow coordinates to GDI+ after excessive zoom.
        internal const int MaximumArrowPixels = 32000;

        internal static bool TryProject(double dx, double dy, double u, double v,
            out double along, out double across)
        {
            along = across = 0;
            if (!double.IsFinite(dx) || !double.IsFinite(dy) ||
                !double.IsFinite(u) || !double.IsFinite(v)) return false;
            double scale = Math.Max(Math.Abs(dx), Math.Abs(dy));
            if (scale == 0) return false;
            double ex = dx / scale;
            double ey = dy / scale;
            double length = Math.Sqrt(ex * ex + ey * ey);
            ex /= length;
            ey /= length;
            along = u * ex + v * ey;
            across = ex * v - ey * u;
#if !__MonoCS__
            // Preserve the unsigned transverse colour used by Windows AngleBetween.
            across = Math.Abs(across);
#endif
            return double.IsFinite(along) && double.IsFinite(across);
        }

        internal static bool TryCoordinate(double value, out int pixels)
        {
            pixels = 0;
            if (!double.IsFinite(value)) return false;
            double rounded = Math.Round(value, MidpointRounding.ToEven);
            if (rounded < int.MinValue || rounded > int.MaxValue) return false;
            pixels = (int)rounded;
            return true;
        }

        internal static bool TryArrowPixels(double velocity, double scale, out int pixels)
        {
            pixels = 0;
            if (!double.IsFinite(velocity) || !double.IsFinite(scale) || scale < 0) return false;
            double value = velocity * scale;
            return Math.Abs(value) <= MaximumArrowPixels && TryCoordinate(value, out pixels);
        }
    }
}
