# GRAL Dispersion Model GUI .NET6RC1<br>
This is the branch for the .NET6RC1 version of the graphical user interface. Since the .NETFramework is no longer developed by Microsoft, we expect the following advantages from the migration in the future:
* High dpi support
* Performance improvements
* Publishing of single files including the entire required framework
* Improvements due to future developments by Microsoft
* Native Windows-ARM support<br>

The migration from .NETFramework to .NET6 results in significant code adjustments for an application as large as the GUI. Many problems of the migration have been fixed and this version is already being used productively internally, although Version 22.03 is still recommended for unexperienced or new users.<br>

The MonoDevelop branch contains a version of the GUI that is intended to be used with MonoDevelop under Linux (the distribution we use is Debian 11).<br>

This application is a graphical user interface (GUI), designed to simplify the numerous input values for GRAL (atmospheric dispersion model) and GRAMM (non-hydrostatic mesoscale wind-field model), [edit sources and obstacles](ReadMe/Items.md), import data from shape files, [analyze and display results](ReadMe/Maps.md) as contour lines, visualize wind vectors, and verify the input and output of the GRAL and GRAMM model. It is also possible to visualize and classify [meteorological input data](ReadMe/WindAnalysis.md) (wind roses, stability or velocity classes, diurnal frequencies of wind directions, diurnal mean wind velocity).<br>
There is also a [youtube](https://www.youtube.com/watch?v=vfEVl-j4P5s) tutorial that shows and explains some basic functions of the GUI.<br>

## Built With
Windows [Visual Studio 2022 or higher](https://visualstudio.microsoft.com/de/downloads/) <br>
Linux [MonoDevelop](https://www.monodevelop.com/)<br>

## Official Release and Documentation
The current validated GUI versions for Windows and a comprehensive manual are available at the [GRAL homepage](http://lampz.tugraz.at/~gral/)

## Contributing
Everyone is invited to contribute to the project [Contributing](Contributing.md)
 
## Versioning
The version number includes the release year and the release month, e.g. 20.01.

## License
This project is licensed under the GPL 3.0 License - see the [License](License.md) file for details
