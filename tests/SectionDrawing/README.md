# GRAMM section wind rendering tests

A zero horizontal wind vector has no direction. `Vector3D.AngleBetween` returns NaN for this input, and converting the projected arrow length to Int32 raises an OverflowException. Direct projection onto the section axis handles calm wind without computing an angle.

The projection keeps the unsigned transverse colour convention of the Windows implementation. Invalid vectors and excessive arrow lengths are omitted with a visible warning. A vertical arrow can be drawn when its horizontal projection is too short to display. GRAL wind rendering is unchanged.

## Run

Requirements: Windows and .NET 10 SDK with Windows Desktop support. From the repository root:

```powershell
dotnet build src/Gral.csproj -c Release -o artifacts/section-drawing/gui
dotnet build tests/SectionDrawing/SectionDrawing.csproj -c Release -o artifacts/section-drawing/tests
dotnet artifacts/section-drawing/tests/SectionDrawing.dll "artifacts/section-drawing/gui/GRAL_GUI.dll" "artifacts/section-drawing/results" patched
```

To verify the regression against an unmodified GUI build, pass that DLL, a separate results directory, and `original` instead of `patched`. In original mode, calm and vertical-only fixtures must raise the expected OverflowException.

The harness constructs the actual Sectiondrawing form offscreen and invokes its paint methods on a bitmap. It uses synthetic section data; no project or wind-field download is required. It tests calm wind, vertical-only wind, both display modes, NaN/infinity, a degenerate section, excessive scaling, and legend handling. A separate comparison checks 10000 finite vectors against the old angle formula.

Results are written as JSON, PNGs, and expected exception text. The test returns a nonzero exit code on failure. Use separate output folders when comparing original and patched builds.

## Interpretation

The synthetic normal-wind images match the original byte for byte. The formulas are mathematically equivalent for finite nonzero wind, but floating-point operation order differs. Real wind fields can differ at pixel or colour rounding boundaries; exact image identity is not a general guarantee.

These tests exercise the rendering functions, not the full interactive project-opening workflow. Linux/Mono execution is not tested. The GUI source-group limit is outside this change.
