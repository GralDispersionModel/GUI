# GRAMM initial conditions

Open **Topography > GRAMM input > Initial conditions...**. Save the GRAMM latitude and IIN initial values on the first tab. These settings are stored in the project's `Computation/IIN.dat`; ordinary control-file generation preserves them.

For `meteopgt.all` simulations, GRAMM derives temperature and humidity from its stability settings. To override them, open the CustomInit tab, load the weather situations, then edit the table or import a CSV template. **Fill from initial values** copies air, surface and soil temperatures and humidity to all draft rows. Saving IIN alone does not update CustomInit.

CSV files begin with `situation` and may contain any of the columns in the exported template. Situation numbers must be unique and cover every row of the current weather file. Comma or semicolon separators and decimal points are supported. Import matches explicit numbers; it does not infer an hour from a timestamp. Leave a cell blank to use the model default. Zero humidity and zero lapse-rate overrides cannot be represented by the existing GRAMM format and are rejected; blank cells retain the default.

The table uses Celsius and humidity percent. CustomInit stores kelvin and humidity fractions. Temperatures are reference values at sea level; account for station elevation before using observed temperatures. The snow threshold uses altitude above sea level, and inversion height is above the lowest terrain cell. CustomInit sets initial conditions, not continuous boundary forcing. Hourly inputs require one weather situation per hour in the same order. ERA5 generation is disabled because this feature does not supply hourly ERA5 forcing.

**Save CustomInit.txt** applies the draft. Replacing an existing IIN or CustomInit file retains a timestamped `.bak` file. **Disable CustomInit** renames the active file with a `.disabled` suffix. Load that file and save CustomInit to enable it again. Saving settings does not recompute existing wind fields. Use a fresh computation folder when producing a new wind-field set.

## Regression checks

Build the GUI and an unmodified GRAMM V2701 core. The test harness uses both real assemblies:

```powershell
dotnet build src/Gral.csproj -c Release -o artifacts/gui
dotnet build tests/GrammInitialConditions -c Release -o artifacts/tests -p:GuiAssembly="${PWD}/artifacts/gui/GRAL_GUI.dll" -p:GrammAssembly="${PWD}/../GRAMM/src/bin/Release/net10.0/GRAMM.dll"
dotnet artifacts/tests/GrammInitialConditions.dll artifacts/initial-conditions-qa SampleProjects/AscendingBridge/Computation/IIN.dat
```

Run on Windows with .NET 10 and use a new QA directory. The tests cover locale-independent conversion, real GRAMM parsing, CSV validation, an annual table, backups, GUI persistence and ERA5 behavior. The harness creates hidden windows and exports PNG layouts for visual review.
