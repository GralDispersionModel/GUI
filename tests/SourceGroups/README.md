# Extended source groups in the GUI

Use this GUI with the [companion GRAL core](https://github.com/Borealis-Thoon/GRAL/tree/codex/extend-source-groups) for projects beyond 99 source groups. IDs must be positive Int32 values, through `2147483647`. Array sizes follow the groups present or selected, not the largest ID.

The Define Source Groups dialog keeps 1–99 as initial choices and allows additional rows and explicit IDs. Paste tab-separated Number/Name columns or add a row at the bottom. Only named rows are saved as definitions; imported sources may use unnamed IDs. Changing a definition's number does not remap IDs already assigned to sources. Unmodified GUIs can still clamp extended IDs to 99.

## Regression tests

Use Windows and the .NET 10 SDK, with the patched GRAL repository beside this repository. From the GUI repository root:

```powershell
dotnet build src/Gral.csproj -c Release -o artifacts/source-groups/gui
dotnet build ../GRAL/src/GRAL.csproj -c Release -o artifacts/source-groups/core
dotnet run --project tests/SourceGroups/SourceGroups.csproj -c Release "-p:GralGuiAssembly=$PWD/artifacts/source-groups/gui/GRAL_GUI.dll" -- "$PWD/artifacts/source-groups/results" "$PWD/artifacts/source-groups/core/GRAL.dll"
```

Use a new results directory. The harness calls actual GUI methods on the STA thread, renders the definition dialog offscreen, writes synthetic inputs, runs the core in an isolated computation directory, and checks GUI mean-result evaluation.

There are ten cases and 1920 assertions: sparse IDs; all four source serializers and file readers/writers; definition save/reopen; unnamed imported source selection; 302-group modulation/totals; decay mapping; 302 computed/301 selected receptor groups in reversed order; and a two-step GUI → core → GUI test. The group set is 1–300, 1001, and 2147483647. Numeric checks verify the active first-step group and GUI concentration values.

For legacy comparison, build an unmodified GUI at the same base. Run the compiled harness with `--legacy` in separate harness directories containing each corresponding `GRAL_GUI.dll` and runtime dependencies:

```powershell
dotnet SourceGroups.dll path/to/new/legacy-results --legacy
```

Compare the five files under `legacy-results/file_io/Emissions` by SHA256. This covers four source types and IDs 1–99. Use identical harness code and inputs for both builds.

These tests do not cover every interactive workflow or establish production-scale performance, scientific accuracy, or convergence. Memory, particles, and storage grow with selected group count. The existing core checkpoint format requires the identical ordered group list and unchanged inputs on restart. The Section drawing fix is separate.
