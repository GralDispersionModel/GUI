# GRAL Dispersion Model GUI<br>
This application is a graphical user interface (GUI), designed to simplify the numerous input values for GRAL (atmospheric dispersion model) and GRAMM (non-hydrostatic mesoscale wind-field model), [edit sources and obstacles](ReadMe/Items.md), import data from shape files, [analyze and display results](ReadMe/Maps.md) as contour lines, visualize wind vectors, and verify the input and output of the GRAL and GRAMM model. It is also possible to visualize and classify [meteorological input data](ReadMe/WindAnalysis.md) (wind roses, stability or velocity classes, diurnal frequencies of wind directions, diurnal mean wind velocity).<br>
There is also a [youtube](https://www.youtube.com/watch?v=vfEVl-j4P5s) tutorial that shows and explains some basic functions of the GUI.<br><br>

The application has been migrated from .NETFramework to .NET6 after the release of version 22.03 and to .NET8 for version 23.11. Since the .NETFramework is no longer developed by Microsoft, we expect the following advantages from the migration in the future:
* High dpi support
* Performance improvements
* Publishing of single files including the entire required framework
* Improvements due to future developments by Microsoft
* Native Windows-ARM support<br>

## Build and run on Linux

The Linux GUI requires the .NET 10 SDK to build and Mono to run. On Ubuntu, install `mono-devel` for the runtime and the `resgen` resource compiler. Open the GUI in an X11 session. It launches the engines in GNOME Terminal using Bash.

Run these commands from the repository root.

```sh
dotnet build linux/Gral.Mono.csproj -c Release
mono linux/bin/Release/net48/GRAL_GUI.exe
```

Keep the executable, its configuration file and its DLLs together. When you start a calculation, select the Linux GRAL or GRAMM executable or its matching .NET DLL. The GUI runs a Linux GRAL distribution from its selected directory and passes the project's Computation folder to the engine, including when **Copy cores to project** is selected.

The [platform checks](tests/PlatformChecks.md) cover strict builds, mixed output files, multi-instance GRAMM and engine stop requests.

## Built With
Windows [Visual Studio 2022 or higher](https://visualstudio.microsoft.com/de/downloads/) <br>
Linux [Mono](https://www.mono-project.com/) and the [.NET SDK](https://dotnet.microsoft.com/download)<br>

## Official Release and Documentation
The current validated GUI versions for Windows and a comprehensive manual are available at the [GRAL homepage](https://gral.tugraz.at/)

## Contributing
Everyone is invited to contribute to the project [Contributing](Contributing.md)
 
## Versioning
The version number includes the release year and the release month, e.g. 20.01.

## License
This project is licensed under the GPL 3.0 License - see the [License](License.md) file for details
