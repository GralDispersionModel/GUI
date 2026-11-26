# GUI platform regressions

| Problem reproduced | Fix and check |
| --- | --- |
| A Windows Release rebuild produced ten warnings. | The CLI can now resolve the 62 rules from `MinimumRecommendedRules.ruleset`. Unused catch variables and redundant value-type null checks were removed. The GRAL stop button awaits the stop request. The engine process fields belong to the platforms that use them. Rebuild with both warning-as-error switches. |
| The sources in the PR could not be built for Mono's .NET Framework 4.8 target. | The Linux project compiles the shared sources with the Mono references, resources and framework-compatible JSON dependencies. The Linux application runs directly from the build output. Keep its configuration and dependent DLLs alongside the executable. |
| On Mono, opening the GRAMM progress list raised a `MissingMethodException`. Several labels were clipped and the concentration and flow-field delete icons were blank. | The Mono progress bars use the drawing surface's vertical DPI. Check all four worker rows, the processor settings and both file-delete buttons. Canceling each confirmation must leave the files intact. |
| A Linux multi-instance GRAMM run could use the wrong runtime for a DLL or record a terminal PID even if the engine failed to start. | Start a Linux apphost and a managed DLL with their respective runtimes. Each instance must identify its engine process. A failed launch must be reported, and a worker that exits with an error must not appear to be still computing. |
| Continue on Linux bypassed the saved restart position. Filename prefix matching could also treat a backup as a finished wind field. | The two platforms share the range planner and checkpoint. In a completed test project, remove one scalar file and click Continue. Only the incomplete range should restart. A second Continue should preserve all file hashes and modification times. A file such as `00001_backup.wnd` does not satisfy `00001.wnd`. |
| Terminal PIDs and a shared Windows PID list made stop requests unreliable. An unresponsive CPU engine could lose its retry handle. | Stop targets the engines started for the current model and project. Test both CPU and GPU workers, including a timeout followed by a retry. Check the engine exit code and the final GUI count. A failed GRAMM stop must preserve the control file. |
| Copying a Linux apphost or DLL on its own could leave its runtime or GPU files behind. | Use the selected engine distribution and pass the project's Computation directory as a separate argument. Test both an executable and a DLL from a directory whose name contains spaces. |
| On Windows, retrying a failed engine launch on NUMA node 0 lost the first attempt's allocation and could obscure the original error. | Both attempts release their native resources. Check a successful launch and repeated failures with a missing executable. The failure should report the Windows error code without creating a worker. |

## Rebuild

```sh
dotnet build src/Gral.csproj -c Release -t:Rebuild -warnaserror -p:TreatWarningsAsErrors=true
dotnet build linux/Gral.Mono.csproj -c Release -t:Rebuild -warnaserror -p:TreatWarningsAsErrors=true
```

Run the first command on Windows and the second on Linux with Mono's `resgen` on the path. With an X11 display available, run `bash tests/GralMonoSmoke/run.sh -warnaserror -p:TreatWarningsAsErrors=true` to check tab changes, mixed file totals and GRAMM row rendering. On a headless Linux machine, prefix the command with `xvfb-run -a`.

For the GPU check, open the Computation tab and start a GPU run. Unpack several `.con` files into Computation while the run is active. The panel should count the archives and the unpacked files and show their combined size in decimal MB. Check the count after the first GPU result and again at exit. Flow fields have a separate count and can be stored in the directory selected by `GFF_FilePath.txt`.
