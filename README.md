# GRAL Dispersion Model<br>
This application is a graphical user interface (GUI), designed to simplify the numerous input values for GRAL (atmospheric dispersion model) and GRAMM (non-hydrostatic mesoscale wind-field model), [edit sources and obstacles](ReadMe/Items.md), import data from shape files, [analyze and display results](ReadMe/Maps.md) as contour lines, visualize wind vectors, and verify the input and output of the GRAL and GRAMM model. It is also possible to visualize and classify [meteorological input data](ReadMe/WindAnalysis.md) (wind roses, stability or velocity classes, diurnal frequencies of wind directions, diurnal mean wind velocity).<br>
There is also a [youtube](https://www.youtube.com/watch?v=vfEVl-j4P5s) tutorial that shows and explains some basic functions of the GUI.<br>

## Built With
Windows [Visual Studio 2017 or higher](https://visualstudio.microsoft.com/de/downloads/) <br>
Linux  [MonoDevelop](https://www.monodevelop.com/) <br>

When building the GUI in Linux, the following manual changes are needed:
* Disable compilation (set compilation to "None") for the WPF forms in the subfolder GRAL3DFunctions: X_3D_Win.xaml, X_3D_MeshExtensions.cs and X_3D_Win.xaml.cs. Click on the file with the right mouse button and select "None".
* Changes in the file "Domain.Designer.cs", located in the subfolder "GRALDomain": move the item "menustrip1" behind the item "GRAMMWindFieldsToolStripMenuItem"; otherwise the MenuItem.Check property is not working (seems to be a Mono Winforms bug)
* Conditional compilation for Mono is performed with the preprocessor directive `#if __MonoCS__`. Before compiling for Linux the symbol `__MonoCS__`must be defined in the compilation tab of MonoDevelop.

## Official Release and Documentation
The current validated and signed GUI versions for Windows and Linux and a comprehensive manual are available at the [GRAL homepage](http://lampz.tugraz.at/~gral/)

## Contributing
Everyone is invited to contribute to the project [Contributing](Contributing.md)
 
## Versioning
The version number includes the release year and the release month, e.g. 20.01.

## License
This project is licensed under the GPL 3.0 License - see the [License](License.md) file for details
