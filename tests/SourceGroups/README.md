# Source groups through 1295 in the GUI

Use this GUI with [core PR #54](https://github.com/GralDispersionModel/GRAL/pull/54). The public range is 1..1295. [FilenameProtocol.md](FilenameProtocol.md) defines the shared, Windows-safe names. Original names for IDs 1..99 remain unchanged.

The definition dialog supports additional rows, explicit IDs, paste, save and reopen. Only named rows are stored. Changing a definition number does not remap existing sources. An imported ID may be unnamed, but must still be in range. Unsupported IDs are rejected rather than clamped.

Source files, temporal and receptor headers, and decay settings keep numeric IDs. Output and modulation filenames use the shared codec in all result workers and modulation read/write/delete/copy paths. The earlier Int32 extension is preserved on [codex/archive-source-groups-int32-20260915](https://github.com/Borealis-Thoon/GUI/tree/codex/archive-source-groups-int32-20260915).

## Regression tests

Use Windows and the .NET 10 SDK with the core repository beside the GUI repository.

```powershell
dotnet build src/Gral.csproj -c Release -o artifacts/source-groups/gui
dotnet build ../GRAL/src/GRAL.csproj -c Release -o artifacts/source-groups/core
dotnet run --project tests/SourceGroups/SourceGroups.csproj -c Release "-p:GralGuiAssembly=$PWD/artifacts/source-groups/gui/GRAL_GUI.dll" -- "$PWD/artifacts/source-groups/results" "$PWD/artifacts/source-groups/core/GRAL.dll"
```

Use a new output directory. The harness has 11 cases and 5923 assertions. It checks all 1295 filename tokens for uniqueness without case sensitivity and round trips, 1295 definition rows saved and reopened, all four source types, 302 groups in file I/O and modulation/decay paths, 302 computed and 301 selected receptor groups in reverse order, and GUI inputs through the core to GUI mean-result evaluation.

The source-file and integration group set is 1..300, 1001 and 1295. The GUI integration runs two synthetic steps. IDs beyond 1295 are rejected. A separate core test runs all 1295 groups.

For legacy comparison, run this compiled harness with --legacy in separate harness directories containing the original or patched GRAL_GUI.dll and matching runtime dependencies. Compare the five files in file_io/Emissions by SHA256. This covers four source types and IDs 1..99.

These tests invoke GUI methods and offscreen rendering; they do not cover every interactive workflow. The core memory change reduces sparse concentration storage but does not establish production-scale speed, convergence or cross-device reproducibility. The Section drawing fix remains independent in [PR #95](https://github.com/GralDispersionModel/GUI/pull/95).
