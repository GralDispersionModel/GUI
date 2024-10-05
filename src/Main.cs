#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2019]  [Dietmar Oettl, Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using GralData;
using GralIO;
using GralMessage;
using GralStaticFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
#if __MonoCS__
#else
using System.Runtime.Intrinsics.X86;
#endif
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace Gral
{
    /// <summary>
    /// Enumeration for input file button style
    /// </summary>
    public enum ButtonColorEnum
    {
        RedDot = 0, GreenDot = 1, BlackHook = 2, Invisible = -1,
        ButtonControl = 10, ButtonMeteo = 11, ButtonEmission = 12, ButtonBuildings = 13
    }

    /// <summary>
    /// Enumeration for building mode
    /// </summary>
    public enum BuildingModeEnum
    {
        None = 0, Diagnostic = 1, Prognostic = 2, GRAMM = 3
    }

    /// <summary>
    /// Enumeration for Wind Data Handling for wind velocity = 0
    /// </summary>
    public enum WindData00Enum
    {
        All = 0, Reject00 = 1, Shuffle00 = 2
    }

    public partial class Main : Form
    {
        /// <summary>
        /// Path and name of the project folder
        /// </summary>
        public static string ProjectName = "";
        /// <summary>
        /// Traffic situations for NEMO
        /// </summary>
        public static string[] NemoTrafficSituations;
        /// <summary>
        /// Global decimal separator of the system
        /// </summary>
        private string DecimalSep;
        /// <summary>
        /// Path of existing GRAMM project
        /// </summary>
        public string GRAMMproject;
        /// <summary>
        /// List of the names of defined source groups and the corresponding numbers
        /// </summary>
        public static List<SG_Class> DefinedSourceGroups = new List<SG_Class>();
        /// <summary>
        /// A time series of the recent meteorological data
        /// </summary>
        public static List<GralData.WindData> MeteoTimeSeries = new List<GralData.WindData>();
        /// <summary>
        /// Name of met. time series
        /// </summary>
        public string MetfileName;
        /// <summary>
        /// Columnseperator for met-file input
        /// </summary>
        public char MetoColumnSeperator;
        /// <summary>
        /// Decimalseperator for met-file input
        /// </summary>
        public string MeteoDecSeperator;
        /// <summary>
        /// User classified wind direction angle
        /// </summary>
        public double WindSectorAngle;
        /// <summary>
        /// User classified number of wind speed classes
        /// </summary>
        public int WindSpeedClasses = 1;
        /// <summary>
        /// Text box for input of wind speed classes
        /// </summary>
        public TextBox[] TBox = new TextBox[10];
        /// <summary>
        /// NumericUpDown for input of wind speed classes
        /// </summary>
        public NumericUpDown[] NumUpDown = new NumericUpDown[10];
        /// <summary>
        /// Label for text box for input of wind speed classes
        /// </summary>
        public Label[] LbTbox = new Label[10];
        /// <summary>
        /// Label for text box for input of wind speed classes
        /// </summary>
        public Label[] LbTbox2 = new Label[10];
        /// <summary>
        /// Height of classified wind observations
        /// </summary>
        public double Anemometerheight = 10;
        /// <summary>
        /// Height of recent wind observations
        /// </summary>
        private double AnemometerheightRecent = 10;
        /// <summary>
        /// Numericupdown for input of horizontal slices for concentration grids in GRAL
        /// </summary>
        public NumericUpDown[] TBox3 = new NumericUpDown[10];
        /// <summary>
        /// Label for the text box for horizontal slices for concentration grids
        /// </summary>
        public Label[] LbTbox3 = new Label[10];
        /// <summary>
        /// All GRAL Settings for the file in.dat
        /// </summary>
        public InDatVariables GRALSettings = new InDatVariables();
        /// <summary>
        /// The GUI Settings
        /// </summary>
        public static GralData.GuiSettings GUISettings;
        /// <summary>
        /// Horizontal grid size of concentration grid
        /// </summary>
        public double HorGridSize = 10;
        /// <summary>
        /// Horizontal grid size of GRAMM grid
        /// </summary>
        public double GRAMMHorGridSize = 300;
        /// <summary>
        /// Domain Area for GRAL
        /// </summary>
        public GralData.DomainArea GralDomRect = new GralData.DomainArea();
        /// <summary>
        /// Domain Area for GRAMM
        /// </summary>
        public GralData.DomainArea GrammDomRect = new GralData.DomainArea();
        /// <summary>
        /// Number of cells in x-direction for the concentration grid
        /// </summary>
        public int CellsGralX;
        /// <summary>
        /// Number of cells in y-direction for the concentration grid
        /// </summary>
        public int CellsGralY;
        /// <summary>
        /// Flag to control whether the file "in.dat" should be deleted or not (it should not be deleted during loading)
        /// </summary>
        private bool IndatReset = false;
        /// <summary>
        /// Flag to control whether the emission files for GRAL should be deleted or not (it should not be deleted during loading)
        /// </summary>
        public bool EmifileReset = false;
        /// <summary>
        /// Collection of pollutants within the defined model domain
        /// </summary>
        public List<string> Pollmod = new List<string>();
        /// <summary>
        /// Default list for pollutants that can be used in the simulations
        /// </summary>
        public static List<string> PollutantList = new List<string>();
        /// <summary>
        /// Name of Gramm topography file
        /// </summary>
        public string Topofile;
        /// <summary>
        /// Name of GRAMM landuse file
        /// </summary>
        public string Landusefile;
        /// <summary>
        /// Path of GRAMM windfield
        /// </summary>
        public string GRAMMwindfield;
        /// <summary>
        /// GRAL computation
        /// </summary>
        private Process GRALProcess;
        /// <summary>
        /// Control changes in the file Percent.txt, which indicates the GRAL simulation status for the actual computed dispersion simulation
        /// </summary>
        private readonly FileSystemWatcher percentGRAL = new FileSystemWatcher();
        /// <summary>
        /// Control changes in the file DispNr.txt, which indicates the GRAL simulation status for the actual computed dispersion simulation
        /// </summary>
        private readonly FileSystemWatcher dispnrGRAL = new FileSystemWatcher();
        /// <summary>
        /// Frequencies of each dispersion situation
        /// </summary>
        private double[] DispSituationfrequ = new double[100000];
        /// <summary>
        /// GRAMM computations
        /// </summary>
        private Process GRAMMProcess;
        /// <summary>
        /// Control changes in the file PercentGramm.txt, which indicates the GRAMM simulation status for the actual computed dispersion simulation
        /// </summary>
        private FileSystemWatcher percentGramm = new FileSystemWatcher();
        /// <summary>
        /// Control changes in the file DispNrGramm.txt, which indicates the GRAMM simulation status for the actual computed dispersion simulation
        /// </summary>
        private FileSystemWatcher dispnrGramm = new FileSystemWatcher();
        /// <summary>
        /// Defines the number of cells at the boundaries of the GRAMM domain, over which the topography is smoothed
        /// </summary>
        private int CellNrTopographySmooth = 0;
        /// <summary>
        /// Control changes in the file Problemreport.txt, which provides information about the numerical stability of GRAMM simulations
        /// </summary>
        private FileSystemWatcher ProblemreportGRAMM = new FileSystemWatcher();
        /// <summary>
        /// Control changes in the file Problemreport_GRAL.txt, which provides information about the numerical stability of GRAL simulations
        /// </summary>
        private FileSystemWatcher ProblemreportGRAL = new FileSystemWatcher();
        /// <summary>
        /// Refresh interval for online GRAMM and GRAL
        /// </summary>
        public int OnlineRefreshInterval = 2;
        /// <summary>
        /// Marker, if GRAL project is locked
        /// </summary>
        public static bool Project_Locked = false;
        /// <summary>
        /// Marker, if GRAMM project is locked
        /// </summary>
        public bool GRAMM_Locked = false;
        /// <summary>
        /// Flag: wind data changed?
        /// </summary>
        private bool ChangedWindData = false;
        /// <summary>
        /// Sunrise option for GRAMM Computation
        /// </summary>
        private int GRAMM_Sunrise = 0;
        /// <summary>
        /// GIS form reference
        /// </summary>
        private GralDomain.Domain DomainGISForm;
        /// <summary>
        /// Prefix for the result files
        /// </summary>
        private string FilePrefix = String.Empty;
        /// <summary>
        /// Control Button OK = green dot
        /// </summary>
        private bool Control_OK = false;
        /// <summary>
        /// Meteo Button OK = green dot
        /// </summary>
        private bool Meteo_OK = false;
        /// <summary>
        /// Building Button OK = green dot
        /// </summary>
        private bool Building_OK = false;
        /// <summary>
        /// Emission Button OK = green dot
        /// </summary>
        private bool Emission_OK = false;
        /// <summary>
        /// Initial settings for Wind Rose drawing
        /// </summary>
        private GralData.WindRoseSettings WindroseSetting = new GralData.WindRoseSettings();
        /// <summary>
        /// Store the data for flexible stretching factors
        /// </summary>
        private List<float[]> FlowFieldStretchFlexible = new List<float[]>();
        /// <summary>
        /// Store the data for decay rates for each source group
        /// </summary>
        private List<GralData.DecayRates> DecayRate = new List<GralData.DecayRates>();
        /// <summary>
        /// Path and name of the meteo data folder
        /// </summary>
        private string MeteoDirectory = String.Empty;
        /// <summary>
        /// Number of checkpoints to detect, if a building covers a cell
        /// </summary>
        private int BuildingCellCoverageThreshold = 5;
        /// <summary>
        /// Rectangle with coordinates for opeining the mail to function
        /// </summary>
        private Rectangle OpenMailToIVT;
        /// <summary>
        /// Project settings and project paths
        /// </summary>
        public static ProjectSettings ProjectSetting = new ProjectSettings("");

        private Bitmap EmissionModulationMap;
        public static readonly string SquareString = "²";
        public static readonly string CubeString = "³";
        public static readonly string MikroString = "µ";
        public static readonly string My_p_m3 = "µg/m³";
        public static readonly string mg_p_m2 = "mg/m².d";
        private GralDomain.DomainformClosed DomainClosedHandle;

        public Int32 OnlineParmeters = 0;

        /// <summary>
        /// Start the main form of this application
        /// </summary>
        public Main()
        {
            InitializeComponent();
            //CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            St_F.Small_Font = new Font("Arial", 8);
            St_F.Pin_Wind_Scale = 0;
#if __MonoCS__
            // set Numericupdowns-Alignement to Left in Linux
            var allNumUpDowns = GetAllControls<NumericUpDown>(this);
            foreach (NumericUpDown nu in allNumUpDowns)
            {
                nu.TextAlign = HorizontalAlignment.Left;
            }
            button37.AutoSize = false;
            button37.Width = 166;
            button14.BackColor = button21.BackColor;
            button44.BackColor = button21.BackColor;
            numericUpDown34.Left = 142;
            groupBox19.Top = 228;
            //checkBox29.Enabled = false;
#else
#endif
            //MessageBox.Show(Convert.ToString(DPI_X));
            // reduce flickering
            DoubleBuffered = true;
            // use double buffer to minimize flickering.
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //User defineddecimal seperator
            DecimalSep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

            //create first text box for input of the height of horizontal slice
            int _y0 = label6.Bottom + 5;
            CreateTextbox1(_y0, 80, 22, 0);
            TBox3[0].Value = 3;
            GRALSettings.HorSlices[0] = 3;
            GRALSettings.BuildingMode = BuildingModeEnum.None;
            SetBuildingRadioButton();

            //define default pollutants
            PollutantList.AddRange(new string[]{
                                    "NOx",
                                    "PM10",
                                    "Odour",
                                    "SO2",
                                    "PM2.5",
                                    "NH3",
                                    "NO2",
                                    "NMVOC",
                                    "HC",
                                    "HF",
                                    "HCl",
                                    "H2S",
                                    "F",
                                    "CO",
                                    "BaP",
                                    "Pb",
                                    "Cd",
                                    "Ni",
                                    "As",
                                    "Hg",
                                    "Tl",
                                    "TCE",
                                    "Unknown",
                                    "Bioaerosols" });
            NemoTrafficSituations = new string[] {
                                    "None",
                                    "IO_HVS1",
                                    "IO_HVS2",
                                    "IO_HVS3",
                                    "IO_LSA1",
                                    "IO_LSA2",
                                    "IO_LSA3",
                                    "IO_Kern",
                                    "IO_Nebenstr_dicht",
                                    "IO_Nebenstr_locker",
                                    "IO_Stop+Go",
                                    "AO_HVS1",
                                    "AO_HVS2",
                                    "AO_HVS3",
                                    "AO_Nebenstr",
                                    "AB>120",
                                    "AB_120",
                                    "AB_100",
                                    "AB_80",
                                    "AB_60",
                                    "AB>120_gebunden",
                                    "AB_120_gebunden",
                                    "AB_100_gebunden",
                                    "AB_80_gebunden",
                                    "AB_60_gebunden",
                                    "AB_Baust1",
                                    "AB_Baust2",
                                    "AB_Stop+Go"};

            checkBox35.Checked = false;
            groupBox25.Enabled = true;
            dateTimePicker1.CustomFormat = "MMMM dd, yyyy '-' HH:mm";
            dateTimePicker1.Value = DateTime.Today;
            button105.Image = (System.Drawing.Image)(Properties.Resources.Unlocked);
            button104.Image = (System.Drawing.Image)(Properties.Resources.Unlocked);
            button101.Image = (System.Drawing.Image)(Properties.Resources.Unlocked);

            //initialize the GUI Settings
            GUISettings = new GralData.GuiSettings();
            GUISettings.ReadFromFile();

            if (GUISettings.AutoCheckForUpdates)
            {
                AutoUpdateStart(false);
            }

            button14.Tag = 0; // allow calls to the GIS window
        }

        /// <summary>
        /// Get all control item of the type T
        /// </summary>
        public static IList<T> GetAllControls<T>(Control control) where T : Control
        {
            var lst = new List<T>();
            foreach (Control item in control.Controls)
            {
                var ctr = item as T;
                if (ctr != null)
                {
                    lst.Add(ctr);
                }
                else
                {
                    lst.AddRange(GetAllControls<T>(item));
                }
            }
            return lst;
        }

        /// <summary>
        /// Close the application
        /// </summary>
        void MainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (Directory.Exists(Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar))) // a project selected?
            {
                if (Control_OK == false) // indat not valid
                {
                    if (MessageBox.Show("The current GRAL settings are not saved", "Close the GRAL user interface?", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)
                        == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
                else if (File.Exists(Path.Combine(ProjectName, @"Computation", "in.dat")) == false) // in.dat does not exist
                {
                    if (MessageBox.Show("The current GRAL settings are not saved", "Close the GRAL user interface?", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)
                        == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
                else if (ChangedWindData) // meteo set but not saved
                {
                    if (MessageBox.Show("The current meteorological settings are not saved", "Close the GRAL user interface?", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)
                        == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        /// <summary>
        ///Check the wind speed class input
        /// </summary>
        private void CheckWindClassesInput(object sender, EventArgs e)
        {
            for (int nr = 0; nr < WindSpeedClasses - 1; nr++)
            {
                if (NumUpDown[nr] != null)
                {
                    NumUpDown[nr].ValueChanged -= new System.EventHandler(CheckWindClassesInput);
                }
            }
            for (int nr = 0; nr < WindSpeedClasses - 1; nr++)
            {
                TBox[nr + 1].Text = NumUpDown[nr].Value.ToString();
                if (NumUpDown[nr + 1] != null)
                {
                    if (NumUpDown[nr + 1].Value < NumUpDown[nr].Value)
                    {
                        NumUpDown[nr + 1].Value = NumUpDown[nr].Value + 1M;
                    }
                }
            }
            for (int nr = 0; nr < WindSpeedClasses - 1; nr++)
            {
                if (NumUpDown[nr] != null)
                {
                    NumUpDown[nr].ValueChanged += new System.EventHandler(CheckWindClassesInput);
                }
            }
        }
        //////////////////////////////////////////////////////////////
        //
        //        open a new or existing project
        //
        //////////////////////////////////////////////////////////////
        /// <summary>
        ///Open a new or an existing project
        /// </summary>
        private void Button8_Click(object sender, EventArgs e)
        {
            //define path and name of new project
            using (FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = "Select/Define project path and name",
                ShowNewFolderButton = true
            })
            {
                //load last project path
                GUISettings.ReadFromFile();

                dialog.SelectedPath = GUISettings.PreviousUsedProjectPath;
#if NET6_0_OR_GREATER
                dialog.UseDescriptionForTitle = true;
#endif

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadProject(dialog.SelectedPath); // load the project data
                }
            }
        }

        /// <summary>
        ///Create a height slice textbox
        /// </summary>
        private void CreateTextbox1(int y0, int b, int h, int nr)
        {
            //text box for input of horizontal slices
            TBox3[nr] = new NumericUpDown
            {
                //tbox3[nr].Location = new System.Drawing.Point(98, y0);
                Location = new System.Drawing.Point(139, y0),
                Size = new System.Drawing.Size(b, h),
                DecimalPlaces = 1,
                Maximum = 1000,
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)))
            };
            groupBox5.Controls.Add(TBox3[nr]);
#if __MonoCS__
#else
            TBox3[nr].TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
#endif
            //tbox3[nr].ValueChanged +=  new System.EventHandler(input); //input values are written to the following text box
            //labels for text boxes
            LbTbox3[nr] = new Label
            {
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                //lbtbox3[nr].Location = new System.Drawing.Point(147, y0 + 3);
                Location = new System.Drawing.Point(169 + 55, y0 + 3),
                Size = new System.Drawing.Size(46, 16),
                Text = "m"
            };
            groupBox5.Controls.Add(LbTbox3[nr]);

            TBox3[nr].ValueChanged += new System.EventHandler(ChangeSliceHeight);
        }

        /// <summary>
        ///Remove a height slice textbox
        /// </summary>
        private void RemoveTextbox1(int nr)
        {
            if (TBox3[nr] != null)
            {
                TBox3[nr].ValueChanged -= new System.EventHandler(ChangeSliceHeight);
            }
            groupBox5.Controls.Remove(TBox3[nr]);
            groupBox5.Controls.Remove(LbTbox3[nr]);
        }

        /// <summary>
        ///Check the input data of the GRAL Settings Tab
        /// </summary>
        private int CheckGralInputData()
        {
            int ok = 0;
            //check input in textbox for roughness length
            if (checkBox29.Checked)
            {
                GRALSettings.AdaptiveRoughness = (double)numericUpDown45.Value;
            }
            else
            {
                GRALSettings.AdaptiveRoughness = 0;
            }

            try
            {
                if (numericUpDown38.Value <= 0)
                {
                    MessageBox.Show("Roughness length needs to be larger than zero", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    numericUpDown38.Value = 0.001M;
                    GRALSettings.Roughness = 0.001;
                    ok = 0;
                }
                else if (numericUpDown38.Value > 3)
                {
                    MessageBox.Show("Roughness length is extremely large!", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    GRALSettings.Roughness = (double)numericUpDown38.Value;
                    ok = 1;
                }
                else
                {
                    GRALSettings.Roughness = (double)numericUpDown38.Value;
                    ok = 1;
                }
            }
            catch
            {
                ok = 0;
                MessageBox.Show("Roughness length needs to be a valid number", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                numericUpDown38.Value = 0.001M;
                GRALSettings.Roughness = 0.001;
            }
            //check input in textbox for latitude
            try
            {
                if (numericUpDown39.Value < -90M)
                {
                    MessageBox.Show("Latitude needs to be larger than -90 deg", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    numericUpDown39.Value = -90M;
                    GRALSettings.Latitude = -90;
                    ok = 0;
                }
                else if (numericUpDown39.Value > 90M)
                {
                    MessageBox.Show("Latitude needs to be smaller than 90 deg", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    numericUpDown39.Value = 90M;
                    GRALSettings.Latitude = 90;
                    ok = 0;
                }
                else
                {
                    GRALSettings.Latitude = (double)(numericUpDown39.Value);
                    ok = 1;
                }
            }
            catch
            {
                ok = 0;
                MessageBox.Show("Latitude needs to be a valid number", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                numericUpDown39.Value = 90M;
                GRALSettings.Latitude = 90;
            }
            //check input in textboxes for horizontal slices
            try
            {
                double zm = GRALSettings.Deltaz;
                for (int i = 0; i < GRALSettings.NumHorSlices; i++)
                {
                    if ((double)TBox3[i].Value - GRALSettings.Deltaz * 0.5 < 0)
                    {
                        MessageBox.Show("Horizontal slice needs to be larger than zero plus half of the chosen vertical layer thickness", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        zm += GRALSettings.Deltaz * 0.5;
                        TBox3[i].Value = (decimal)(zm);
                        ok = 0;
                    }
                    else if (TBox3[i].Value > 500)
                    {
                        MessageBox.Show("Horizontal slice is quite high above the ground level!", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        GRALSettings.HorSlices[i] = Convert.ToDouble(TBox3[i].Value);
                        ok = 1 * ok;
                    }
                    else
                    {
                        GRALSettings.HorSlices[i] = Convert.ToDouble(TBox3[i].Value);
                        ok = 1 * ok;
                    }
                }
            }
            catch
            {
                ok = 0;
                MessageBox.Show("Horizontal slices need to be valid numbers", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return ok;
        }

        //save control input
        /// <summary>
        ///Save In.Dat File
        /// </summary>
        private void Button10_Click(object sender, EventArgs e)
        {
            GenerateInDat();
        }

        /// <summary>
        ///open acutal map or generate a blank, unreferenced sheet
        /// </summary>
        private void Button14_Click(object sender, EventArgs e)
        {
            if ((int)(button14.Tag) == 0) // is GIS Button locked?
            {
                if (DomainGISForm != null)
                {
                    DomainGISForm.Show();
                    DomainGISForm.Activate();
                    DomainGISForm.WindowState = FormWindowState.Maximized;
                }
                else if (DomainGISForm == null)
                {
                    button14.Tag = 1; // block 2nd call to open the GIS window
                    DomainGISForm = new GralDomain.Domain(this, false)
                    {
                        ReDrawVectors = true,
                        ReDrawContours = true
                    };
                    DomainClosedHandle = new GralDomain.DomainformClosed(SetDomainClosed);
                    DomainGISForm.DomainClosed += DomainClosedHandle;

                    DomainGISForm.StartPosition = FormStartPosition.Manual;
                    DomainGISForm.Left = St_F.GetScreenAtMousePosition();
                    DomainGISForm.Show();
                    button14.Tag = 0; // allow calls to the GIS window
                }
            }
        }

        /// <summary>
        ///close the acutal map
        /// </summary>
        private void SetDomainClosed(object sender, EventArgs e)
        {
            if (DomainClosedHandle != null)
            {
                DomainGISForm.DomainClosed -= DomainClosedHandle;
            }

            DomainGISForm = null;
            //GC.Collect(); // Debug only
        }

        /// <summary>
        ///open the emission modulation interface
        /// </summary>
        private void ListView1_DoubleClick(object sender, EventArgs e)
        {
            string newPath = Path.Combine(Main.ProjectName, @"Settings", "emissionmodulations.txt");
            if (File.Exists(Path.Combine(ProjectSetting.EmissionModulationPath, "emissionmodulations.txt")))
            {
                newPath = Path.Combine(ProjectSetting.EmissionModulationPath, "emissionmodulations.txt");
            }
            GralMainForms.Emissionvariation emvar = new GralMainForms.Emissionvariation(this, newPath)
            {
                StartPosition = FormStartPosition.Manual,
                Location = new System.Drawing.Point(this.Left, this.Top),
                Owner = this
            };
            emvar.Show();
        }

        //save settings of meteorological input data
        /// <summary>
        /// Save the file Meteorology.txt in the settings folder
        /// </summary>
        public void SaveMeteoDataFile()
        {
            string newPath = Path.Combine(ProjectName, @"Settings", "Meteorology.txt");
            try
            {
                using (StreamWriter myWriter = File.CreateText(newPath))
                {
                    myWriter.WriteLine(MetfileName);
                    myWriter.WriteLine(MetoColumnSeperator);
                    myWriter.WriteLine(MeteoDecSeperator);
                    myWriter.WriteLine(Convert.ToString(Anemometerheight, ic));
                    myWriter.WriteLine(numericUpDown1.Value);
                    myWriter.WriteLine(numericUpDown2.Value);
                    for (int i = 0; i < WindSpeedClasses - 1; i++)
                    {
                        myWriter.WriteLine(NumUpDown[i].Value.ToString(ic));
                    }

                    Textbox16_Set(MetfileName);
                }
            }
            catch
            {
            }
        }
        ////////////////////////////////////////////////////////
        //
        //Generate emission files for GRAL
        //
        ////////////////////////////////////////////////////////
        /// <summary>
        /// Generate emission files for GRAL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button18_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            MessageWindow message = new MessageWindow();
            message.Show();
            message.listBox1.Items.Add("Generating emission files for GRAL....");
            message.Refresh();
            try
            {
                // generate emission files
                CreateGralEmissionFiles();
                // in transient mode -> generate emission_timeseries.txt
                if (checkBox32.Checked == true)
                {
                    if (Write_Emission_Timeseries())
                    {
                        // write Precipitation.txt for wet deposition
                        if (checkBox33.Checked == true)
                        {
                            Write_Precipitation_txt();
                        }
                    }
                }
                message.Close();
                message.Dispose();
                ChangeButtonLabel(Gral.ButtonColorEnum.ButtonEmission, ButtonColorEnum.BlackHook); // Emission label green
            }
            catch
            {
            }
            //enable/disable GRAL simulations
            Enable_GRAL();
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Compute mettimeseries.dat and meteopgt.all
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button7_Click(object sender, EventArgs e)
        {
            if (checkBox35.Checked == false)
            {
                GRALGenerateMeteoFiles(sender, e);
            }
            else
            {
                GRALGenerateMeteoFilesForERA5();
            }
        }

        /// <summary>
        /// Save pollutant information in the file Pollutant.txt including wet deposition and the decay rate to computation folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            CultureInfo ic = CultureInfo.InvariantCulture;
            if (EmifileReset == true)
            {
                if (sender != numericUpDown35 && sender != numericUpDown36)
                {
                    SetWetDepositionData();
                }

                //save pollutant information
                string newpath = Path.Combine(ProjectName, @"Settings", "Pollutant.txt");
                try
                {
                    using (StreamWriter mywriter = new StreamWriter(newpath, false))
                    {
                        for (int i = 0; i < PollutantList.Count; i++)
                        {
                            if (PollutantList[i] == Convert.ToString(listBox5.SelectedItem))
                            {
                                mywriter.WriteLine(PollutantList[i]);
                                break;
                            }
                        }
                    }
                }
                catch { }

                //save pollutant information to computation folder - since GRAL 18.03
                double cW = 0; double alphaW = 0;
                if (checkBox33.Checked)
                {
                    cW = (double)numericUpDown36.Value / 1E6D;
                    alphaW = (double)numericUpDown35.Value;
                }
                if (Convert.ToString(listBox5.SelectedItem) == "Bioaerosols" || Convert.ToString(listBox5.SelectedItem) == "Unknown")
                {
                    groupBox24.Visible = true;
                }
                else
                {
                    groupBox24.Visible = false;
                }
                newpath = Path.Combine(ProjectName, "Computation", "Pollutant.txt");
                try
                {
                    using (StreamWriter mywriter = new StreamWriter(newpath, false))
                    {
                        for (int i = 0; i < PollutantList.Count; i++)
                        {
                            if (PollutantList[i] == Convert.ToString(listBox5.SelectedItem))
                            {
                                mywriter.WriteLine(PollutantList[i]);
                                break;
                            }
                        }
                        mywriter.WriteLine(cW.ToString(ic) + "\t ! Wet deposition cW setting");
                        mywriter.WriteLine(alphaW.ToString(ic) + "\t ! Wet deposition alphaW setting");
                        mywriter.WriteLine("0" + "\t ! Decay rate for all source groups");
                        if (Convert.ToString(listBox5.SelectedItem) == "Bioaerosols" || Convert.ToString(listBox5.SelectedItem) == "Unknown")
                        {
                            string decay = string.Empty;
                            foreach (GralData.DecayRates dr in DecayRate)
                            {
                                decay += dr.ToString() + "\t";
                            }
                            decay += "! decay rates for defined source groups";
                            mywriter.WriteLine(decay);
                        }
                    }
                }
                catch { }

                DeleteEmissionFiles();
            } // emifilereset
            // enable wet deposition settings, if checkbox is checked
            label32.Enabled = checkBox33.Checked;
            label33.Enabled = checkBox33.Checked;
            numericUpDown35.Enabled = checkBox33.Checked;
            numericUpDown36.Enabled = checkBox33.Checked;
        }

        /// <summary>
        /// Delete all emission files for GRAL
        /// </summary>
        public void DeleteEmissionFiles()
        {
            ChangeButtonLabel(Gral.ButtonColorEnum.ButtonEmission, ButtonColorEnum.RedDot); // Emission label red
            string newPath = Path.Combine(ProjectName, @"Computation", "point.dat");
            File.Delete(newPath);
            newPath = Path.Combine(ProjectName, @"Computation", "portals.dat");
            File.Delete(newPath);
            newPath = Path.Combine(ProjectName, @"Computation", "line.dat");
            File.Delete(newPath);
            newPath = Path.Combine(ProjectName, @"Computation", "cadastre.dat");
            File.Delete(newPath);
        }
        /////////////////////////////////////////////////////////////////////////////
        //
        //    Reset controlfile in.dat
        //
        /////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///When a height slice has been changed -> ResetInDat
        /// </summary>
        private void ChangeSliceHeight(object sender, EventArgs e)
        {
            ResetInDat();
        }
        //change the dispersion time
        private void NumericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            GRALSettings.DispersionTime = Convert.ToInt32(numericUpDown4.Value);
            ResetInDat();
        }
        //change number of particles
        private void NumericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            GRALSettings.ParticleNumber = Convert.ToInt32(numericUpDown6.Value);
            ResetInDat();
        }
        //change the roughness length
        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            St_F.CheckInput(sender, e);

            if (sender == numericUpDown45) // optional adaptive roughness lenght
            {
                if (checkBox29.Checked)
                {
                    GRALSettings.AdaptiveRoughness = (double)numericUpDown45.Value;
                }
                else
                {
                    GRALSettings.AdaptiveRoughness = 0;
                }
            }
            ResetInDat();
        }
        //change the latitude
        private void TextBox4_TextChanged(object sender, EventArgs e)
        {
            St_F.CheckInput(sender, e);
            ResetInDat();
        }
        //change the number of the dispersion situation
        private void NumericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            GRALSettings.DispersionSituation = Convert.ToInt32(numericUpDown5.Value);
            // read and write indat and do not ResetInDat

            InDatVariables indattemp = new InDatVariables();
            InDatFileIO InDataIO = new InDatFileIO();
            indattemp.InDatPath = Path.Combine(ProjectName, "Computation", "in.dat");

            InDataIO.Data = indattemp;
            if (InDataIO.ReadInDat() == true)
            {
                indattemp.DispersionSituation = GRALSettings.DispersionSituation;
                InDataIO.WriteInDat();
            }
        }

        //change the vertical thickness of the counting grid
        private void NumericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            GRALSettings.Deltaz = Convert.ToDouble(numericUpDown8.Value);
            ResetInDat();
        }
        //change the factor for prognostic sub domains
        private void NumericUpDown42_ValueChanged(object sender, EventArgs e)
        {
            GRALSettings.PrognosticSubDomains = (int)numericUpDown42.Value;
            ResetInDat();
        }
        // No buildings
        private void RadioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                GRALSettings.BuildingMode = BuildingModeEnum.None;
            }
            numericUpDown42.Enabled = false;
            SetBuildingMethode(sender, e);
            ResetInDat();
        }
        // Diagnostic buildings
        private void RadioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                GRALSettings.BuildingMode = BuildingModeEnum.Diagnostic;
            }
            numericUpDown42.Enabled = false;
            SetBuildingMethode(sender, e);
            ResetInDat();
        }
        // Prognostic buildings
        private void RadioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
            {
                GRALSettings.BuildingMode = BuildingModeEnum.Prognostic;
                numericUpDown42.Enabled = !Project_Locked;
            }

            SetBuildingMethode(sender, e);
            ResetInDat();
        }
        //change the horizontal grid resolution of the couting grid
        private void NumericUpDown9_ValueChanged_1(object sender, EventArgs e)
        {
            SetGRALConcentrationHorizontalGridSize(sender, e);
        }
        //changing the horizontal grid size of the cartesian grid in GRAL used for wind field calculations
        private void NumericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            SetGRALFowFieldHorizontalGridSize(sender, e);
        }
        //changing the thickness of the vertical layer of the cartesian grid in GRAL used for wind field calculations
        private void NumericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            SetGRALFlowFieldVerticalGridSize(sender, e);
        }
        //changing the streching factor of the vertical cartesian grid in GRAL used for wind field calculations
        private void NumericUpDown12_ValueChanged(object sender, EventArgs e)
        {
            SetGRALFlowFieldStretchingFactor(sender, e);
        }
        //define the number of cells in vertical direction for the microscale flow field model of GRAL
        private void NumericUpDown26_ValueChanged(object sender, EventArgs e)
        {
            SetGRALFlowFieldVerticalCellNumber(sender, e);
        }
        //change relaxation factor of velocity for microscale flow field model of GRAL
        private void NumericUpDown28_ValueChanged(object sender, EventArgs e)
        {
            SetGRALFlowFieldVelRelaxFactor(sender, e);
        }
        //change relaxation factor for pressure correction of microscale wind field model of GRAL
        private void NumericUpDown27_ValueChanged(object sender, EventArgs e)
        {
            SetGRALFlowFieldPreRelaxFactor(sender, e);
        }
        //change minimum number of iterations over timesteps in the microscale flow field model of GRAL
        private void NumericUpDown29_ValueChanged(object sender, EventArgs e)
        {
            SetGRALFlowFieldMinIterationNumber(sender, e);
        }
        //change the number of maximum iterations over timesteps in the microscale flow field model of GRAL
        private void NumericUpDown30_ValueChanged(object sender, EventArgs e)
        {
            SetGRALFlowFieldMaxIterationNumber(sender, e);
        }
        //define roughness of building walls
        private void NumericUpDown31_ValueChanged(object sender, EventArgs e)
        {
            SetGRALFlowFieldBuildingRoughness(sender, e);
        }
        //decide, whether iterations shall be performed until steady-state conditions are reached, or not
        void CheckBox22Click(object sender, EventArgs e)
        {
            SetGRALFlowFieldSteadyStateIteration(sender, e);
        }
        //decide, whether iterations shall be performed until steady-state conditions are reached, or not
        private void CheckBox22_CheckedChanged(object sender, EventArgs e)
        {
            SetGRALFlowFieldSteadyStateIterationChanged(sender, e);
        }
        /// <summary>change the number of horizontal slices of the couting grid
        private void NumericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            SetNumberOfHorizontalConcentrationGridSlices(sender, e);
        }

        //delete the file "in.dat" whenever a value is changed
        /// <summary>
        /// Delete the file "in.dat" whenever a value is changed
        /// </summary>
        private void ResetInDat()
        {
            if (IndatReset == true)
            {
                ChangeButtonLabel(ButtonColorEnum.ButtonControl, ButtonColorEnum.RedDot); // red dot at control button
                //				try
                //				{
                //					string newpath = Path.Combine(projectname, "Computation","in.dat");
                //					//File.Delete(newpath);
                //				}
                //				catch
                //				{
                //				}
            }
        }

        //Emission Tab
        private void AddSourceGroups_Click(object sender, EventArgs e)
        {
            AddSourceGroups(sender, e);
        }
        private void RemoveSourceGroup_Click(object sender, EventArgs e)
        {
            RemoveSourceGroups(sender, e);
        }
        public void ListView1ItemActivate(object sender, EventArgs e)
        {
            RedrawPreviewOfModulation(sender, e);
        }
        private void ShowTotalEmissions_Click(object sender, EventArgs e)
        {
            ShowTotalEmissions(sender, e);
        }

        //Domain Tab
        //define source groups
        private void Button15_Click(object sender, EventArgs e)
        {
            ShowDefineSourceGroupsDialog(sender, e);
            TabControl1Click(null, null);
            SelectAllUsedSourceGroups(); //update listbox4
        }

        // Meteorology Tab
        private void OpenMeteoFile_Click(object sender, EventArgs e)
        {
            OpenMeteoFile(sender, e);
        }
        private void ShowWindRoseVelocity_Click(object sender, EventArgs e)
        {
            ShowWindRoseVelocity(sender, e);
        }
        private void ShowWindRoseStability_Click(object sender, EventArgs e)
        {
            ShowWindRoseStability(sender, e);
        }
        private void ShowWindVelocityClasses_Click(object sender, EventArgs e)
        {
            ShowWindVelocityClasses(sender, e);
        }
        private void ShowWindVelocityClassesDistr_Click(object sender, EventArgs e)
        {
            ShowWindVelocityDistribution(sender, e);
        }
        private void ShowWindStabilityClasses_Click(object sender, EventArgs e)
        {
            ShowWindStabilityClasses(sender, e);
        }
        private void ShowWindVelocityMean_Click(object sender, EventArgs e)
        {
            ShowWindVelocityMean(sender, e);
        }
        private void ShowWindDirectionFrequency_Click(object sender, EventArgs e)
        {
            ShowWindDirectionFrequency(sender, e);
        }
        private void ClassififyMeteo_Click(object sender, EventArgs e)
        {
            checkBox35.Checked = false; //make sure that ERA5 driving is not used, whenever meteodata is generated from observations
            ClassifiyMeteoData(sender, e);
        }
        private void MeteoSectorAngle_ValueChanged(object sender, EventArgs e)
        {
            MeteoSectorAngleSet(sender, e);
        }
        private void MeteoVelClassesCount_ValueChanged(object sender, EventArgs e)
        {
            MeteoVelocityClassesCount(sender, e);
        }
        private void MeteoAnemometerHeight_ValueChanged(object sender, EventArgs e)
        {
            MeteoAnemometerHeight(sender, e);
        }
        private void MeteoUseNotClassifiedData_CheckedChanged(object sender, EventArgs e)
        {
            MeteoUseNotClassifiedData(sender, e);
        }

        //Topography Tab

        // import GRAMM wind field, grid, landuse, and meteorology from existing project
        private void Button24_Click(object sender, EventArgs e)
        {
            GRAMMSetReferenceToExistingWindfields(sender, e);
        }
        //load and create the GRAMM topography
        private void Button19_Click(object sender, EventArgs e)
        {
            //calculate top height for the GRAMM mesh
            double top = Convert.ToDouble(numericUpDown17.Value);
            for (int i = 2; i <= Convert.ToInt32(numericUpDown16.Value) + 1; i++)
            {
                top += Convert.ToDouble(numericUpDown17.Value) * Math.Pow(Convert.ToDouble(numericUpDown19.Value), i - 2);
            }
            if (top < 10000 || MessageBox.Show("A maximum layer height above 10000 m can lead to unstable GRAMM calculations due to low air pressure.\n" +
                "It is recommended to reduce the number of vertical layers or the vertical stretching factor.\nWould you like to continue?", "Create GRAMM Topography",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                GRAMMLoadCreateTopography(sender, e);
            }
        }
        //load and create the GRAMM landuse file
        private void Button20_Click(object sender, EventArgs e)
        {
            GRAMMLoadCreateLanduse(sender, e);
        }
        //change the horizontal grid resolution of the GRAMM grid
        private void NumericUpDown18_ValueChanged(object sender, EventArgs e)
        {
            GRAMMSetHorizontalGridSize(sender, e);
        }
        //change the number of vertical layers in the GRAMM grid
        private void NumericUpDown16_ValueChanged(object sender, EventArgs e)
        {
            GRAMMSetVerticalLayerNumber(sender, e);
        }
        //compute top level height in dependence of the GRAMM streching factor
        private void NumericUpDown19_ValueChanged(object sender, EventArgs e)
        {
            GRAMMComputeTopLevelHeight(sender, e);
        }
        //change the maximum time step for the GRAMM simulations
        private void NumericUpDown20_ValueChanged(object sender, EventArgs e)
        {
            GRAMMSetMaximumTimeStep(sender, e);
        }
        //change the modelling time for the GRAMM simulations
        private void NumericUpDown21_ValueChanged(object sender, EventArgs e)
        {
            GRAMMSetModellingTime(sender, e);
        }
        //change the relaxation factor for velocity for the GRAMM simulations
        private void NumericUpDown22_ValueChanged(object sender, EventArgs e)
        {
            GRAMMSetRelaxVelFactor(sender, e);
        }
        //change the relaxation factor for scalars for the GRAMM simulations
        private void NumericUpDown23_ValueChanged(object sender, EventArgs e)
        {
            GRAMMSetRelaxScalFactor(sender, e);
        }
        //save file "reinit.dat" when reinitialization is switched on or off
        private void CheckBox20_CheckedChanged(object sender, EventArgs e)
        {
            GRAMMSetReInitOption(sender, e);
        }
        //save file "IIN.dat" when catabatic forcing is switched on/off
        private void CheckBox21_CheckedChanged(object sender, EventArgs e)
        {
            GRAMMSetCatabaticForcing(sender, e);
        }
        //save file "IIN.dat" when Debuging level is changed
        private void NumericUpDown25_ValueChanged(object sender, EventArgs e)
        {
            GRAMMSetDebugingLevel(sender, e);
        }
        //change the starting number of the flow situation and the steady state option
        private void NumericUpDown24_ValueChanged(object sender, EventArgs e)
        {
            GRAMMSetStartingSituation(sender, e);
        }
        //set sunrise option
        void CheckBox30Click(object sender, EventArgs e)
        {
            GRAMMSetSunriseOption(sender, e);
        }
        //save flat terrain adjustments in the file IIN.dat for GRAMM simulations
        private void CheckBox31_CheckedChanged(object sender, EventArgs e)
        {
            GRAMMSetFlatTerrainOption(sender, e);
        }
        //enable ERA5 transient forcing for GRAMM
        public void CheckBox35_CheckedChanged(object sender, EventArgs e)
        {
            GRAMMSetERA5Forcing(sender, e);
        }
        //sets the start time for transient GRAMM simulations using ERA5
        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            GRAMMSetERA5SetStartTime(sender, e);
        }
        //set the coordinate system used in the GRAMM simulations
        private void NumericUpDown40_ValueChanged(object sender, EventArgs e)
        {
            GRAMMSetERA5SetCoordinateSystem(sender, e);
        }
        //set the time interval for re-initialzing the GRAMM simulations, when driven by ERA5 data in hours
        private void NumericUpDown44_ValueChanged(object sender, EventArgs e)
        {
            GRAMMReInitializingERA5(sender, e);
        }
        //set the time interval for updating boundary conditions in the GRAMM simulations, when driven by ERA5 data in hours
        private void NumericUpDown37_ValueChanged(object sender, EventArgs e)
        {
            GRAMMBoundaryConditionsERA5(sender, e);
        }


        // Computation Tab
        //Start GRAL simulation
        private void Button33_Click(object sender, EventArgs e)
        {
            GRALStartCalculation(sender, e);
        }
        //stop GRAL simulations
        private void Button34_Click(object sender, EventArgs e)
        {
            GRALStopCalculation(sender, e);
        }
        //pause for GRAL simulations
        private void Button35_Click(object sender, EventArgs e)
        {
            GRALPauseCalculation(sender, e);
        }
        //start GRAMM simulation
        private void Button32_Click(object sender, EventArgs e)
        {
            GRAMMStartCalculation(sender, e);
        }
        //stop GRAMM simulations
        private void Button30_Click(object sender, EventArgs e)
        {
            GRAMMStopCalculation(sender, e);
        }
        //pause GRAMM simulations
        private void Button31_Click(object sender, EventArgs e)
        {
            GRAMMPauseCalculation(sender, e);
        }
        //Computing mean concentrations
        private void Button28_Click(object sender, EventArgs e)
        {
            AnalyseMeanConcentration(sender, e);
        }
        //routine computes mean, max, and daily maximum concentrations
        private void Button25_Click(object sender, EventArgs e)
        {
            AnalyseMeanMaxMinConcentration(sender, e);
        }
        //routine, which computes time series and wind data at given receptor points
        private void Button37_Click(object sender, EventArgs e)
        {
            AnalyseReceptorConcentration(sender, e);
        }
        //routine, which computes wind data at given receptor points
        void Button47Click(object sender, EventArgs e)
        {
            AnalyseReceptorMeteoData(sender, e);
        }
        //computes high percentiles of pollutant concentrations of a time series
        private void Button40_Click(object sender, EventArgs e)
        {
            AnalyseHighPercentiles(sender, e);
        }
        //"Multisource": computes odour hours for multiple sources considering diurnal/seasonal emission modulations
        private void Button27_Click(object sender, EventArgs e)
        {
            AnalyseOdourMultipleSources(sender, e);
        }
        //compute odour hours from compost works (up to three different processes with varying frequencies)
        private void Button26_Click(object sender, EventArgs e)
        {
            AnalyseOdourCompostWorks(sender, e);
        }
        //compute odour hours for "all in all out" stables
        private void Button29_Click(object sender, EventArgs e)
        {
            AnalyseOdourAllInAllOut(sender, e);
        }
        // deletes all Windfield files
        void Button45Click(object sender, EventArgs e)
        {
            GRAMMDeleteWindFields(sender, e);
        }
        //deletes all intermediate GRAL *.con and *.odr files
        private void Button41_Click(object sender, EventArgs e)
        {
            GRALDeleteConcentrationFiles(sender, e);
        }
        //deletes all intermediate GRAL *.gff files
        private void Button42_Click(object sender, EventArgs e)
        {
            GRALDeleteFlowFieldFiles(sender, e);
        }

        //////////////////////////////////////////////////////////////////////
        //
        //     additional tools
        //
        //////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raster buildings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button9_Click(object sender, EventArgs e)
        {
            if (GRALSettings.BuildingMode != BuildingModeEnum.None)
            {
                Cursor = Cursors.WaitCursor;
                try
                {
                    //disable computation of vector and contour maps
                    GenerateObstacleRaster();
                }
                catch
                {
                }
                Cursor = Cursors.Default;
            }
            //enable/disable GRAL simulations
            Enable_GRAL();
            //enable/disable GRAMM simulations
            Enable_GRAMM();
        }

        /// <summary>
        /// Compute road emissions with the Network Emission Model NEMO from TU-Graz, Austria
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button13_Click(object sender, EventArgs e)
        {
#if __MonoCS__
            MessageBox.Show("This function is not available at LINUX", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
#else
            try
            {
                Main_NEMO NemoWork = new Main.Main_NEMO();
                //source group seperation
                GralMainForms.Nemostartwindow nemo = new GralMainForms.Nemostartwindow(this);
                DialogResult dr = new DialogResult();
                dr = nemo.ShowDialog();
                string[] text = new string[2];
                int sg1 = 1;
                int sg2 = 1;
                int sg3 = 1;
                int sg4 = 1;
                if (dr == DialogResult.OK)
                {
                }
                //check, whether source seperation has been chosen or not
                if (nemo.checkBox1.Checked == true)
                {
                    //generate NEMO input files
                    text = nemo.comboBox1.Text.Split(new char[] { ',' });
                    try
                    {
                        sg1 = Convert.ToInt32(text[1].Replace(" ", ""));
                    }
                    catch
                    {
                        sg1 = Convert.ToInt32(text[0]);
                    }
                    text = nemo.comboBox4.Text.Split(new char[] { ',' });
                    try
                    {
                        sg2 = Convert.ToInt32(text[1].Replace(" ", ""));
                    }
                    catch
                    {
                        sg2 = Convert.ToInt32(text[0]);
                    }
                    text = nemo.comboBox2.Text.Split(new char[] { ',' });
                    try
                    {
                        sg3 = Convert.ToInt32(text[1].Replace(" ", ""));
                    }
                    catch
                    {
                        sg3 = Convert.ToInt32(text[0]);
                    }
                    text = nemo.comboBox3.Text.Split(new char[] { ',' });
                    try
                    {
                        sg4 = Convert.ToInt32(text[1].Replace(" ", ""));
                    }
                    catch
                    {
                        sg4 = Convert.ToInt32(text[0]);
                    }
                    NemoWork.NemoInput(true, sg1, sg2, sg3, sg4);
                }
                else
                {
                    //generate NEMO input files
                    NemoWork.NemoInput(false, sg1, sg2, sg3, sg4);
                }
                NemoWork = null;
                if (nemo != null)
                {
                    nemo.Close();
                    nemo.Dispose();
                }
            }
            catch
            {
                //MessageBox.Show (ex.Message);
            }
#endif
        }

        /// <summary>
        /// Temporary input box
        /// </summary>
        private DialogResult InputBox1(string title, string promptText, int lowlimit, int uplimit, ref int trans)
        {
            DialogResult dialogResult = DialogResult.Cancel;
            using (Form form = new Form())
            {
                Label label = new Label();
                NumericUpDown numdown = new NumericUpDown();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();
                form.Text = title;
                label.Text = promptText;
                numdown.Maximum = uplimit;
                numdown.Minimum = lowlimit;
                numdown.Value = trans;
                numdown.Increment = 1;
                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.AutoSize = false;
                label.SetBounds(9, 10, 372, 13);
                label.Location = new Point(9, 10);
                label.Size = new System.Drawing.Size(372, 13);

                numdown.SetBounds(12, 36, 372, 20);
                numdown.Location = new Point(12, 36);
                numdown.Size = new System.Drawing.Size(372, 20);

                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);
                buttonOk.Location = new Point(228, 72);
                buttonOk.Size = new System.Drawing.Size(75, 23);
                buttonCancel.Location = new Point(309, 72);
                buttonCancel.Size = new System.Drawing.Size(75, 23);

                numdown.Anchor |= AnchorStyles.Top;
                buttonOk.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                buttonCancel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                form.ClientSize = new Size(396, 107);
                form.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

                form.Controls.AddRange(new Control[] { label, numdown, buttonOk, buttonCancel });
                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;
                dialogResult = form.ShowDialog();
                trans = Convert.ToInt32(numdown.Value);
            }
            return dialogResult;
        }

        /// <summary>
        /// Temporary input box for floating values
        /// </summary>
        private DialogResult InputBox2(string title, string promptText, decimal lowlimit, decimal uplimit, ref decimal trans)
        {
            DialogResult dialogResult = DialogResult.Cancel;
            using (Form form = new Form())
            {
                Label label = new Label();
                NumericUpDown numdown = new NumericUpDown();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();
                form.Text = title;
                label.Text = promptText;
                numdown.Maximum = uplimit;
                numdown.Minimum = lowlimit;
                numdown.Value = trans;
                numdown.Increment = 0.1M;
                numdown.DecimalPlaces = 1;
                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.AutoSize = false;
                label.SetBounds(9, 10, 372, 13);
                label.Location = new Point(9, 10);
                label.Size = new System.Drawing.Size(372, 13);

                numdown.SetBounds(12, 36, 372, 20);
                numdown.Location = new Point(12, 36);
                numdown.Size = new System.Drawing.Size(372, 20);

                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);
                buttonOk.Location = new Point(228, 72);
                buttonOk.Size = new System.Drawing.Size(75, 23);
                buttonCancel.Location = new Point(309, 72);
                buttonCancel.Size = new System.Drawing.Size(75, 23);

                numdown.Anchor |= AnchorStyles.Left;
                buttonOk.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                buttonCancel.Anchor = AnchorStyles.Top | AnchorStyles.Left;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new Control[] { label, numdown, buttonOk, buttonCancel });
                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;
                dialogResult = form.ShowDialog();
                trans = numdown.Value;
            }
            return dialogResult;
        }

        /// <summary>
        /// Check if *.con files are existing for postprocessing routines
        /// </summary>
        private void CheckConFiles()
        {
            if ((CountConFiles() > 0) && (File.Exists(Path.Combine(ProjectName, "Computation", "mettimeseries.dat")) == true))
            {
                if (Convert.ToString(listBox5.SelectedItem) != "Odour")
                {
                    groupBox13.Visible = true;
                    groupBox14.Visible = false;
                    if (File.Exists(Path.Combine(ProjectName, @"Computation", "zeitreihe.dat")) ||
                        File.Exists(Path.Combine(ProjectName, @"Computation", "ReceptorConcentrations.dat")))
                    {
                        button37.Visible = true;
                    }
                    else
                    {
                        button37.Visible = false;
                    }
                    //check transient GRAL simulation
                    InDatVariables data = new InDatVariables();
                    InDatFileIO ReadInData = new InDatFileIO();
                    data.InDatPath = Path.Combine(ProjectName, "Computation", "in.dat");
                    ReadInData.Data = data;
                    // at transient computatinons mean() is not available
                    if (ReadInData.ReadInDat() == true && data.Transientflag == 0)
                    {
                        button28.Visible = false;
                        button37.Text = "&Receptor met files";
                    }
                    else
                    {
                        // mean-routine does not work for non-classified meteorological situations and not for emission-timeseries.txt files
                        string emissionPath = Path.Combine(ProjectName, "Computation", "emissions_timeseries.txt");
                        if (Directory.Exists(ProjectSetting.EmissionModulationPath))
                        {
                            emissionPath = Path.Combine(ProjectSetting.EmissionModulationPath, "emissions_timeseries.txt");
                        }
                        if (checkBox19.Checked == false
                            && (File.Exists(Path.Combine(ProjectName, "Computation", "emissions_timeseries.txt")) == false))
                        {
                            button28.Visible = true;
                        }
                        else
                        {
                            button28.Visible = false;
                        }
                        button37.Text = "&Receptor Concentrations";
                    }
                }
                else
                {
                    groupBox14.Visible = true;
                    groupBox13.Visible = false;
                    //check transient GRAL simulation
                    InDatVariables data = new InDatVariables();
                    InDatFileIO ReadInData = new InDatFileIO();
                    data.InDatPath = Path.Combine(ProjectName, "Computation", "in.dat");
                    ReadInData.Data = data;
                    if (ReadInData.ReadInDat() == true)
                    {
                        if (data.Transientflag == 0)
                        {
                            button26.Visible = false;
                            button29.Visible = false;
                        }
                        else
                        {
                            button26.Visible = true;
                            button29.Visible = true;
                        }
                    }
                }
                Project_Locked = true;                  // lock project
                ProjectLockedButtonClick(null, null); // change locked-Button
            }
            else if (CountConFiles() == 0)
            {
                groupBox13.Visible = false;
                groupBox14.Visible = false;
                Project_Locked = false;                 // unlock project
                ProjectLockedButtonClick(null, null); // change locked-Button
            }
        }


        /// <summary>
        /// Count the number of *.con files in the path ProjectName/Computation
        /// </summary>
        /// <returns></returns>
        private int CountConFiles()
        {
            string newPath = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
            if (Directory.Exists(newPath))
            {
                DirectoryInfo di = new DirectoryInfo(newPath);
                FileInfo[] GRAL_con = di.GetFiles("*.con");
                int temp = GRAL_con.Length;
                if (temp == 0) // compressed files?
                {
                    GRAL_con = di.GetFiles("*.grz");
                    temp = GRAL_con.Length;
                }
                GRAL_con = di.GetFiles("*.odr");
                return Math.Max(temp, GRAL_con.Length);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Count the number of *.wnd files in the path newPath
        /// </summary>
        /// <param name="newPath"></param>
        /// <returns></returns>
        private int CountWindFiles(string newPath)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(newPath);
                FileInfo[] GRAMM_wnd = di.GetFiles("*.wnd");
                return Convert.ToInt32(GRAMM_wnd.Length);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Check if the most important input files for a GRAL computation are available and enable/disable GRAL simulations
        /// </summary>
        public void Enable_GRAL()
        {
            //enable/disable GRAL simulations
            bool enable = true;
            if ((button10.Visible == true) && (Control_OK == false))
            {
                enable = false;
            }

            if ((button7.Visible == true) && (Meteo_OK == false))
            {
                enable = false;
            }

            if ((button18.Visible == true) && (Emission_OK == false))
            {
                enable = false;
            }

            if (button18.Visible == false)
            {
                enable = false;
            }

            if ((button9.Visible == true) && (Building_OK == false))
            {
                enable = false;
            }

            if (ProjectName != null)
            {
                //get number of wind field files
                //DirectoryInfo di = new DirectoryInfo(Path.Combine(projectname, @"Computation"));
                //FileInfo[] GRAMM_wind = di.GetFiles("*.wnd");
                if (File.Exists(Path.Combine(ProjectName, @"Computation", "meteopgt.all")) == false)
                {
                    enable = false;
                }

                if ((File.Exists(Path.Combine(ProjectName, @"Computation", "ggeom.asc")) == true) && (File.Exists(Path.Combine(ProjectName, @"Computation", "windfeld.txt")) == false))
                {
                    enable = false;
                }

                if (File.Exists(Path.Combine(ProjectName, @"Computation", "GRAL.geb")) == false)
                {
                    enable = false;
                }
            }
            else
            {
                enable = false;
            }

            //if ERA5 data is used, but no wind fields have yet been generated
            if (checkBox35.Checked == true)
            {
                string newPath = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                DirectoryInfo di = new DirectoryInfo(newPath);
                FileInfo[] files_conc = di.GetFiles("*.wnd");
                if (files_conc.Length > 0)
                {
                    enable = true;
                }
                else
                {
                    enable = false;
                }
            }

            //MessageBox.Show (enable.ToString ());
            if (enable == true)
            {
                groupBox16.Visible = true; // GRAl-Start Group Box
                if (GRALSettings.BuildingMode == BuildingModeEnum.Prognostic) // Prognostic GRAL -> Online GroupBox visible
                {
                    groupBox17.Visible = true;
                }
            }
            else
            {
                groupBox16.Visible = false;
            }
        }

        /// <summary>
        /// Check if all files for a GRAMM computation are available and enable/disable GRAMM simulations
        /// </summary>
        public void Enable_GRAMM()
        {
            //enable/disable GRAMM simulations
            bool enable = true;
            if (ProjectName != null)
            {
                if (File.Exists(Path.Combine(ProjectName, @"Computation", "meteopgt.all")) == false)
                {
                    enable = false;
                }

                if (File.Exists(Path.Combine(ProjectName, @"Computation", "ggeom.asc")) == false)
                {
                    enable = false;
                }

                if (File.Exists(Path.Combine(ProjectName, @"Computation", "IIN.dat")) == false)
                {
                    enable = false;
                }

                if (File.Exists(Path.Combine(ProjectName, @"Computation", "GRAMM.geb")) == false)
                {
                    enable = false;
                }
            }
            else
            {
                enable = false;
            }

            if (enable == true)
            {
                groupBox15.Visible = true;
                groupBox17.Visible = true;
            }
        }

        /// <summary>
        /// Save user comments 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button38_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ProjectName))
            {
                return; // exit if no project loaded
            }

            string name = Path.Combine(ProjectName, @"Settings", "comments.txt");
            try
            {
                using (StreamWriter write = new StreamWriter(name, false))
                {
                    write.WriteLine(Convert.ToString(textBox17.Text));
                }
            }
            catch { }
        }

        /// <summary>
        /// Set a flag, such that GRAL writes building heights to the file building_heights.txt in the subdirectory Computation 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox23_CheckedChanged(object sender, EventArgs e)
        {
            if (EmifileReset == true)
            {
                GRALSettings.BuildingHeightsWrite = checkBox23.Checked;
                //modify control file for GRAL simulations in.dat
                GenerateInDat();
            }
        }

        /// <summary>
        /// Set a flag, such that GRAL writes topographical heights to the file GRAL_Topography.txt in the subdirectory Computation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox24_CheckedChanged(object sender, EventArgs e)
        {
            if (EmifileReset == true)
            {
                bool istda = new bool();
                string name = Path.Combine(ProjectName, @"Computation", "GRAL_Topography.txt");
                istda = File.Exists(name);
                if (checkBox24.Checked == true)
                {
                    if (istda == false)
                    {
                        File.Create(name).Close();
                    }
                }
                else
                {
                    if (istda == true)
                    {
                        File.Delete(name);
                    }
                }
            }
        }

        /// <summary>
        /// Enable the usage of high resolution topographical data for GRAL simulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox25_CheckedChanged(object sender, EventArgs e)
        {
            if (EmifileReset == true)
            {
                if (checkBox25.Checked == true)
                {
                    //do nothing, GRAL_topofile.txt is generated when launching GRAL for the first time
                }
                else
                {
                    File.Delete(Path.Combine(ProjectName, @"Computation", "GRAL_topofile.txt"));
                }
            }
        }

        /// <summary>
        /// Get *.con, *.gff and *.wnd file size and display it in the main form
        /// </summary>
        private void GralFileSizes()
        {
            if (string.IsNullOrEmpty(ProjectName))
            {
                return;
            }

            try // Kuntner: error if no project exists
            {
                string _file = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                DirectoryInfo _directory = new DirectoryInfo(_file);
                FileInfo[] files_conc = _directory.GetFiles("*.con");
                if (files_conc.Length == 0) // compressed files?
                {
                    files_conc = _directory.GetFiles("*.grz");
                }

                long file_size = 0;
                for (int i = 0; i < files_conc.Length; i++)
                {
                    file_size += files_conc[i].Length;
                }
                if (file_size < 1000000000)
                {
                    label84.Text = Convert.ToString(Math.Round(file_size * 0.000001, 1)) + "MByte";
                }
                else
                {
                    label84.Text = Convert.ToString(Math.Round(file_size * 0.000000001, 1)) + "GByte";
                }
                label57.Text = files_conc.Length.ToString() + " files";
            }
            catch { }

            try
            {
                //size of GRAL flow field files
                DirectoryInfo _directory = new DirectoryInfo(St_F.GetGffFilePath(Path.Combine(ProjectName, "Computation")) + Path.DirectorySeparatorChar);
                FileInfo[] files_gff = _directory.GetFiles("*.gff");
                long file_size = 0;
                for (int i = 0; i < files_gff.Length; i++)
                {
                    file_size += files_gff[i].Length;
                }
                if (file_size < 1000000000)
                {
                    label85.Text = Convert.ToString(Math.Round(file_size * 0.000001, 1)) + "MByte";
                }
                else
                {
                    label85.Text = Convert.ToString(Math.Round(file_size * 0.000000001, 1)) + "GByte";
                }
                label72.Text = files_gff.Length.ToString() + " files";
            }
            catch
            { }

            try
            {
                // Wnd Files
                string _file = Path.Combine(GRAMMwindfield);
                DirectoryInfo _directory = new DirectoryInfo(_file);
                FileInfo[] files_wnd = _directory.GetFiles("*.wnd");
                long file_size = 0;
                for (int i = 0; i < files_wnd.Length; i++)
                {
                    file_size += files_wnd[i].Length;
                }
                if (file_size < 1000000000)
                {
                    label97.Text = Convert.ToString(Math.Round(file_size * 0.000001, 1)) + "MByte";
                }
                else
                {
                    label97.Text = Convert.ToString(Math.Round(file_size * 0.000000001, 1)) + "GByte";
                }
                label96.Text = files_wnd.Length.ToString() + " files";
            }
            catch { }
        }

        /// <summary>
        /// When checked, GRAL stores intermediate flow field files in *gff files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox26_CheckedChanged(object sender, EventArgs e)
        {
            if (EmifileReset == true)
            {
                string name = Path.Combine(ProjectName, @"Computation", "GRAL_FlowFields.txt");
                if (checkBox26.Checked == true)
                {
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(name))
                        {
                            writer.WriteLine(numericUpDown41.Value.ToString(ic));
                        }
                    }
                    catch { }
                }
                else
                {
                    if (File.Exists(name))
                    {
                        try
                        {
                            File.Delete(name);
                        }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// The default land-use values (linked with category 0) can be set / New land-use categories can be defined / Existing land-use categories can be amended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button43_Click(object sender, EventArgs e)
        {
            GralMainForms.Amend_Landuse AL = new GralMainForms.Amend_Landuse()
            {
                Owner = this,
                StartPosition = FormStartPosition.Manual,
                Location = new System.Drawing.Point(this.Left, this.Top)
            };
            AL.Show();
        }

        /// <summary>
        /// Message, that GRAL project is locked
        /// </summary>
        public static void ProjectLockedMessage()
        {
            MessageBoxTemporary Box = new MessageBoxTemporary("The project is locked - Modifications not saved", Main.ActiveForm.Location);
            Box.Show();
            //MessageBox.Show(new Form {TopMost = true }, "The project is locked - Changes not saved", "Project locked", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Message, that GRAMM project is locked
        /// </summary>
        public static void GRAMMLockedMessage()
        {
            MessageBoxTemporary Box = new MessageBoxTemporary("The GRAMM project is locked - Modifications not saved", Main.ActiveForm.Location);
            Box.Show();
            //MessageBox.Show(new Form {TopMost = true }, "The GRAMM project is locked - Changes not saved", "GRAMM project locked", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Lock / unlock GRAMM project manually
        /// </summary>
        void Gramm_locked_buttonClick(object sender, EventArgs e)
        {
            if (sender != null && GRAMM_Locked == true) // Project is unlocked by the user!
            {
                if (MessageBox.Show("The GRAMM project is locked - Unlocking the Project may invalidate the calculated windfields", "GRAMM project locked", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                {
                    GRAMM_Locked = false;
                }
            }
            else if (sender != null && GRAMM_Locked == false) // Project is locked by the user!
            {
                GRAMM_Locked = true;
            }

            if (GRAMM_Locked) // if locked Lock is closed!
            {
                gramm_locked_button.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.lock_closed));
                toolTip1.SetToolTip(gramm_locked_button, "GRAMM project is locked");
                toolTip1.SetToolTip(button105, "GRAMM project is locked");
                GrammLockedLockElements(GRAMM_Locked);
                numericUpDown33.Enabled = false;
            }
            else
            {
                gramm_locked_button.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.lock_open));
                toolTip1.SetToolTip(gramm_locked_button, "GRAMM project is unlocked");
                toolTip1.SetToolTip(button105, "GRAMM project is unlocked");
                GrammLockedLockElements(GRAMM_Locked);
                numericUpDown33.Enabled = true;
            }
        }

        /// <summary>
        /// Lock / unlock GRAL project manually
        /// </summary>
        void ProjectLockedButtonClick(object sender, EventArgs e)
        {
            if (sender != null && Project_Locked == true) // Project is unlocked by the user!
            {
                if (MessageBox.Show("The project is locked - Unlocking the Project may invalidate the calculated concentration files", "Project locked", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                {
                    Project_Locked = false;
                }
            }
            else if (sender != null && Project_Locked == false) // Project is locked by the user!
            {
                Project_Locked = true;
            }

            if (Project_Locked) // if locked Lock is closed!
            {
                project_locked_button.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.lock_closed));
                toolTip1.SetToolTip(project_locked_button, "GRAL project is locked - unlock: delete all *.con files");
                toolTip1.SetToolTip(button101, "GRAL project is locked");
                toolTip1.SetToolTip(button104, "GRAL project is locked");
                toolTip1.SetToolTip(button106, "GRAL project is locked");
                ProjectLockedLockElements(Project_Locked);
            }
            else
            {
                project_locked_button.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.lock_open));
                toolTip1.SetToolTip(project_locked_button, "GRAL project is unlocked");
                toolTip1.SetToolTip(button101, "GRAL project is unlocked");
                toolTip1.SetToolTip(button104, "GRAL project is unlocked");
                if (GRAMM_Locked != true)
                {
                    toolTip1.SetToolTip(button106, "GRAL project is unlocked");
                }

                ProjectLockedLockElements(Project_Locked);
            }
        }

        /// <summary>
        /// Enable/disable GRAMM settings
        /// </summary>
        void GrammLockedLockElements(bool locked)
        {
            // if locked = true: readonly / edit = false: read and write
            if (Project_Locked == false) // lock because of GRAMM if not locked because of GRAL
            {
                numericUpDown1.Enabled = !locked; // Meteo Tab
                numericUpDown2.Enabled = !locked; // Meteo Tab
                numericUpDown7.Enabled = !locked; // Meteo Tab
                checkBox19.Enabled = !locked; // Meteorology classified
            }
            numericUpDown16.Enabled = !locked;
            numericUpDown17.Enabled = !locked;
            numericUpDown18.Enabled = !locked;
            numericUpDown19.Enabled = !locked;
            numericUpDown20.Enabled = !locked;
            numericUpDown21.Enabled = !locked;
            numericUpDown22.Enabled = !locked;
            numericUpDown23.Enabled = !locked;
            numericUpDown39.Enabled = !locked; // Latidude
            button19.Enabled = !locked;
            button20.Enabled = !locked;
            button22.Enabled = !locked;
            button23.Enabled = !locked;
            //button24.Enabled = !locked; // load new windfield
            button43.Enabled = !locked;
            // set increment in upDowns to 0 if locked = true
            int inc = 0;
            if (locked == false)
            {
                inc = 1;
            }

            if (Project_Locked == false) // lock because of GRAMM if not locked because of GRAL
            {
                numericUpDown1.Increment = inc; // Meteo Tab
                numericUpDown2.Increment = inc; // Meteo Tab
                numericUpDown7.Increment = Convert.ToDecimal(inc * 0.5); // Meteo Tab
            }
            numericUpDown16.Increment = inc;
            numericUpDown17.Increment = Convert.ToDecimal(inc * 0.5);
            numericUpDown18.Increment = Convert.ToDecimal(inc * 20);
            numericUpDown19.Increment = Convert.ToDecimal(inc * 0.01);
            numericUpDown20.Increment = inc;
            numericUpDown21.Increment = inc;
            numericUpDown22.Increment = Convert.ToDecimal(inc * 0.01);
            numericUpDown23.Increment = Convert.ToDecimal(inc * 0.01);
            checkBox20.Enabled = !locked;
            checkBox21.Enabled = !locked;
            checkBox31.Enabled = !locked;
            if (checkBox31.Checked == false) // check flat terrain option
            {
                checkBox30.Enabled = !locked;
            }
            else
            {
                checkBox30.Enabled = false; // lock sunrise at flat terrain
            }

            if (locked)
            {
                button105.Image = (System.Drawing.Image)(Properties.Resources.Locked);
            }
            else
            {
                button105.Image = (System.Drawing.Image)(Properties.Resources.Unlocked);
            }
            if (Project_Locked == false) // lock because of GRAMM if not locked because of GRAL
            {
                if (locked)
                {
                    button106.Image = (System.Drawing.Image)(Properties.Resources.Locked);
                }
                else
                {
                    button106.Image = (System.Drawing.Image)(Properties.Resources.Unlocked);
                }
            }
            try
            {
                for (int nr = 0; nr < WindSpeedClasses - 1; nr++) // Meteo Tab
                {
                    NumUpDown[nr].Enabled = !locked;
                }
            }
            catch { }
        }

        /// <summary>
        /// Enable/disable GRAL settings
        /// </summary>
        void ProjectLockedLockElements(bool locked)
        {
            // if locked = true: readonly / edit = false: read and write
            if (GRAMM_Locked == false)
            {
                numericUpDown1.Enabled = !locked; // Meteo Tab
                numericUpDown2.Enabled = !locked; // Meteo Tab
                numericUpDown7.Enabled = !locked; // Meteo Tab
                checkBox19.Enabled = !locked; // Meteorology classified
            }
            numericUpDown3.Enabled = !locked;
            numericUpDown4.Enabled = !locked;
            //numericUpDown5.Enabled = !locked; // start dispersion situation
            numericUpDown6.Enabled = !locked;
            numericUpDown8.Enabled = !locked;
            numericUpDown9.Enabled = !locked;
            numericUpDown10.Enabled = !locked;
            numericUpDown11.Enabled = !locked;
            numericUpDown12.Enabled = !locked;
            numericUpDown26.Enabled = !locked;
            numericUpDown27.Enabled = !locked;
            numericUpDown28.Enabled = !locked;
            numericUpDown29.Enabled = !locked;
            numericUpDown30.Enabled = !locked;
            numericUpDown31.Enabled = !locked;
            numericUpDown34.Enabled = !locked; // GRAL transient
            numericUpDown38.Enabled = !locked; // Surface roughness lenght
            checkBox29.Enabled = !locked;      // Surface roughness lenght
            numericUpDown45.Enabled = !locked; // Adaptive surface roughness
            numericUpDown46.Enabled = !locked; // Building raster checkpoints

            numericUpDown39.Enabled = !locked; // Latidude
            numericUpDown43.Enabled = !locked; // Compressed mode for GRAL output

            if (GRALSettings.BuildingMode == BuildingModeEnum.Prognostic)
            {
                numericUpDown42.Enabled = !locked; // Sub domain factor
            }
            else
            {
                numericUpDown42.Enabled = false; // Sub domain factor
            }
            groupBox21.Enabled = !locked; // Wet Deposition settings
            //groupBox24.Enabled = !locked; // Decay rate settings
            try
            {
                for (int i = 0; i < GRALSettings.NumHorSlices; i++)
                {
                    TBox3[i].Enabled = !locked;
                }
            }
            catch { }
            try
            {
                if (GRAMM_Locked == false)
                {
                    for (int nr = 0; nr < WindSpeedClasses - 1; nr++) // Meteo Tab
                    {
                        NumUpDown[nr].Enabled = !locked;
                    }
                }
            }
            catch { }

            radioButton3.Enabled = radioButton3.Checked || !locked;
            radioButton4.Enabled = radioButton4.Checked || !locked;
            radioButton5.Enabled = radioButton5.Checked || !locked;

            listBox5.Enabled = !locked;
            button7.Enabled = !locked;
            button9.Enabled = !locked;
            button10.Enabled = !locked;
            //button12.Enabled = !locked;
            //button11.Enabled = !locked; // set *.gff File path
            button13.Enabled = !locked;
            button15.Enabled = !locked;
            button16.Enabled = !locked;
            button17.Enabled = !locked;
            if (checkBox32.Checked) // GRAL transient mode
            {
                button18.Enabled = !locked; // lock emission files in transient mode!
                button48.Enabled = !locked;  // create emission timeseries.txt
                button51.Enabled = !locked;  // delete emission timeseries.txt
            }
            // set increment in upDowns to 0 if locked = true
            int inc = 0;
            if (locked == false)
            {
                inc = 1;
            }

            if (GRAMM_Locked == false)
            {
                numericUpDown1.Increment = inc; // Meteo Tab
                numericUpDown2.Increment = inc; // Meteo Tab
                numericUpDown7.Increment = Convert.ToDecimal(inc * 0.5); // Meteo Tab
            }
            numericUpDown3.Increment = inc;
            numericUpDown4.Increment = Convert.ToDecimal(inc * 20);
            //numericUpDown5.Increment = inc;
            numericUpDown6.Increment = Convert.ToDecimal(inc * 20);
            numericUpDown8.Increment = Convert.ToDecimal(inc * 0.1);
            numericUpDown9.Increment = Convert.ToDecimal(inc * 2);
            numericUpDown10.Increment = Convert.ToDecimal(inc * 2);
            numericUpDown11.Increment = Convert.ToDecimal(inc * 0.1);
            numericUpDown12.Increment = Convert.ToDecimal(inc * 0.01);
            numericUpDown26.Increment = inc;
            numericUpDown27.Increment = Convert.ToDecimal(inc * 0.01);
            numericUpDown28.Increment = Convert.ToDecimal(inc * 0.01);
            numericUpDown29.Increment = Convert.ToDecimal(inc * 10);
            numericUpDown30.Increment = Convert.ToDecimal(inc * 10);
            numericUpDown31.Increment = Convert.ToDecimal(inc * 0.001);
            numericUpDown34.Increment = Convert.ToDecimal(inc * 0.0001);
            checkBox22.Enabled = !locked;
            checkBox32.Enabled = !locked; // GRAL transient
            checkBox34.Enabled = !locked; // GRAL transient - Vertical Concentration file
            label13.Enabled = !locked;    // GRAL transient
            checkBox25.Enabled = !locked;
            if (checkBox32.Checked == true) // lock emission modulation at GRAL transient
            {
                listView1.Enabled = !locked;
            }
            else
            {
                listView1.Enabled = true;
            }
            radioButton1.Enabled = !locked;
            radioButton2.Enabled = !locked;
            if (locked)
            {
                button106.Image = (System.Drawing.Image)(Properties.Resources.Locked);
                button104.Image = (System.Drawing.Image)(Properties.Resources.Locked);
                button101.Image = (System.Drawing.Image)(Properties.Resources.Locked);
            }
            else
            {
                if (GRAMM_Locked != true)
                {
                    button106.Image = (System.Drawing.Image)(Properties.Resources.Unlocked);
                }

                button104.Image = (System.Drawing.Image)(Properties.Resources.Unlocked);
                button101.Image = (System.Drawing.Image)(Properties.Resources.Unlocked);
            }
        }

        /// <summary>
        /// Check, if Logfile*.txt files exist
        /// </summary>
        void Check_for_Existing_Logfiles()
        {
            try
            {
                //check for existing Logfile*.txt files
                string newPath1 = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                DirectoryInfo di = new DirectoryInfo(newPath1);
                FileInfo[] files_conc = di.GetFiles("Logfile*.txt");
                if (files_conc.Length > 0)
                {
                    button52.Visible = true;
                }
                else
                {
                    button52.Visible = false;
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Control settings when a new tab has been selected
        /// </summary>
        void TabControl1Click(object sender, EventArgs e)
        {
            double n = 0;
            if (tabControl1.SelectedIndex == 0) // Project
            {
                textBox17.Hide();
                button38.Hide();
                panel1.Show();
                Check_for_Existing_Logfiles();
            }
            if (tabControl1.SelectedIndex == 1) // GRAL Settings
            {
                SetButton12Bitmap();
                SetButton57Bitmap();
            }
            if (tabControl1.SelectedIndex == 2) // Domain
            {
                listView2.Items.Clear();
                foreach (SG_Class _sg in DefinedSourceGroups)
                {
                    ListViewItem item = new ListViewItem(_sg.SG_Number.ToString());
                    item.SubItems.Add(_sg.SG_Name);
                    listView2.Items.Add(item);
                }
            }
            if (tabControl1.SelectedIndex == 3) // Emissions
            {
                if (listView1.Items.Count > 0)
                {

                    listView1.Items[0].Selected = true;
                    listView1.Select();
                    ListView1ItemActivate(null, null);
                    if (Convert.ToString(listBox5.SelectedItem) == "Bioaerosols" || Convert.ToString(listBox5.SelectedItem) == "Unknown")
                    {
                        groupBox24.Visible = true;
                    }
                    else
                    {
                        groupBox24.Visible = false;
                    }
                    if (checkBox32.Checked) // GRAL Transient mode
                    {
                        groupBox27.Text = "Emission-time-series (mandatory - transient mode)";
                        button60.Enabled = false;
                        ProjectSetting.EmissionModulationPath = Path.Combine(ProjectName, @"Computation");
                        ProjectSetting.EvaluationPath = Path.Combine(ProjectName, @"Maps");
                    }
                    else
                    {
                        groupBox27.Text = "Emission-time-series (optional)";
                        button60.Enabled = true;
                    }
                    CheckIfAllSourcegroupsHaveDefinedEmissionModulations(false);

                    GralStaticFunctions.St_F.SetTrimmedTextToTextBox(LabelEmissionPath, ProjectSetting.EmissionModulationPath);

                    string newpath = Path.Combine(ProjectName, "Computation", "emissions_timeseries.txt");
                    if (Directory.Exists(ProjectSetting.EmissionModulationPath))
                    {
                        newpath = Path.Combine(ProjectSetting.EmissionModulationPath, "emissions_timeseries.txt");
                    }
                    if (File.Exists(newpath)) // emission_timeseries is used instead of modulation settings
                    {
                        button49.Visible = true; // show the button for emissions_timeseries
                        button51.Visible = true; // show the button for emissions_timeseries
                    }
                    else
                    {
                        button49.Visible = false;
                        button51.Visible = false;
                    }
                }
            }
            if (tabControl1.SelectedIndex == 4) // Meteorology
            {
                string filename = Path.Combine(ProjectName, @"Computation", "meteopgt.all"); // Set maximum value of start numericupdown to leght of meteogt.all
                int final_sit = 0;
                int i = 0;
                if (File.Exists(filename))
                {
                    i = (int)St_F.CountLinesInFile(filename) - 2;
                    i = Math.Max(i, 1);
                    label100.Text = i.ToString();
                }
                if (File.Exists(Path.Combine(ProjectName, @"Computation", "GRAMMin.dat")))
                {
                    IO_ReadFiles OpenProject = new IO_ReadFiles
                    {
                        ProjectName = ProjectName
                    };
                    if (OpenProject.ReadGrammInFile() == true) // get final situation from GRAMMin.dat if sunrise is activated
                    {
                        final_sit = OpenProject.GRAMMsunrise;
                    }

                    OpenProject = null;
                    if (final_sit > 0)
                    {
                        label100.Text = final_sit.ToString();
                        label102.Text = i.ToString();
                        label101.Visible = true;
                        label102.Visible = true;
                    }
                    else
                    {
                        label101.Visible = false;
                        label102.Visible = false;
                    }
                }
                else
                {
                    label101.Visible = false;
                    label102.Visible = false;
                }
            }
            if (tabControl1.SelectedIndex == 1 || tabControl1.SelectedIndex == 5) // GRAL Settings, Topography
            {
                string filename = Path.Combine(ProjectName, @"Computation", "meteopgt.all"); // Set maximum value of start numericupdown to leght of meteogt.all
                if (File.Exists(filename))
                {
                    int i = (int)St_F.CountLinesInFile(filename) - 2;
                    i = Math.Max(i, 1);
                    numericUpDown5.Value = Math.Min(i, numericUpDown5.Value);
                    numericUpDown5.Maximum = i;
                    numericUpDown24.Value = Math.Min(i, numericUpDown24.Value);
                    numericUpDown24.Maximum = i;
                }
            }
            if (tabControl1.SelectedIndex == 2) // Domain
            {
                try
                {
                    // show number of cells
                    if (CellsGralX > 0)
                    {
                        label91.Text = "Concentration cells : " + Convert.ToString(CellsGralX) + " x " + Convert.ToString(CellsGralY) + " x " + numericUpDown3.Text;
                    }
                    else
                    {
                        label91.Text = "0 x 0 cells";
                    }
                }
                catch
                {
                    label91.Text = "0 x 0 cells";
                }
                try
                {
                    // show number of cells
                    if (CellsGralX > 0 && numericUpDown10.Value > 0 && GRALSettings.BuildingMode != BuildingModeEnum.None)
                    {
                        n = Convert.ToDouble(numericUpDown10.Text);
                        double c = Convert.ToDouble(numericUpDown9.Text);
                        label93.Text = "Flow field cells :         " + Convert.ToString(Math.Round(CellsGralX / n * c, 0)) + " x " + Convert.ToString(Math.Round(CellsGralY / n * c, 0)) + " x " + numericUpDown26.Text;
                    }
                    else
                    {
                        label93.Text = "";
                    }
                }
                catch
                { }
                n = Convert.ToDouble(numericUpDown18.Text);
                try
                {
                    if (n > 0)
                    {
                        label92.Text = Convert.ToString(Math.Abs(Convert.ToDouble(textBox12.Text) - Convert.ToDouble(textBox13.Text)) / n)
                            + " x " + Convert.ToString(Math.Abs(Convert.ToDouble(textBox14.Text) - Convert.ToDouble(textBox15.Text)) / n) + " x " + numericUpDown16.Text + " cells";
                    }
                    else
                    {
                        label92.Text = "0 x 0 cells";
                    }
                }
                catch
                {
                    label92.Text = "0 x 0 cells";
                }
            }
            // Computation
            if (tabControl1.SelectedIndex == 6)
            {
                CheckConFiles(); // check, if project is locked
                GralFileSizes(); // calculate file sizes

                // Check, if GRAMM Windfield exists
                if (Directory.Exists(GRAMMwindfield))
                {
                    DirectoryInfo di = new DirectoryInfo(GRAMMwindfield);
                    FileInfo[] files_flow = di.GetFiles("*.wnd");
                    if (files_flow.Length == 0)
                    {
                        GRAMM_Locked = false;                   // unlock GRAMM project
                    }
                    else
                    {
                        GRAMM_Locked = true;                    // lock GRAMM project
                    }

                    if (GRAMMwindfield.Equals(Path.Combine(ProjectName, "Computation") + Path.DirectorySeparatorChar))
                    {
                        button45.Enabled = true; // delete wind fields allowed
                    }
                    else
                    {
                        button45.Enabled = false; // delete wind fields not allowed
                    }

                }
                else
                {
                    GRAMM_Locked = false;                   // lock GRAMM project
                }

                Gramm_locked_buttonClick(null, null);
                //enable/disable GRAL/GRAMM simulations
                Enable_GRAL();
                Enable_GRAMM();
                // override the visible flag of the dot or hook pictureboxes depending if button is visible
                pictureBox1.Visible = button10.Visible;
                pictureBox2.Visible = button7.Visible;
                pictureBox3.Visible = button18.Visible;
                pictureBox4.Visible = button9.Visible;
                // refresh the progress bars
                PercentChanged(null, null);
                DispNrChanged(null, null);
                PercentGrammChanged(null, null);
                DispnrGrammChanged(null, null);
                // activate FileSystemWatcher
#if __MonoCS__
                checkBoxAVX.Visible = button10.Visible;
                if (checkBoxAVX.Visible)
                {
                    if (GRALSettings.AVX512Usage == 1)
                    {
                        checkBoxAVX.Checked = true;
                    }
                    else
                    {
                        checkBoxAVX.Checked = false;
                    }
                }
#else
                if (percentGRAL != null && Directory.Exists(percentGRAL.Path))
                {
                    percentGRAL.EnableRaisingEvents = true;
                }

                if (dispnrGRAL != null && Directory.Exists(dispnrGRAL.Path))
                {
                    dispnrGRAL.EnableRaisingEvents = true;
                }

                if (percentGramm != null && Directory.Exists(percentGramm.Path))
                {
                    percentGramm.EnableRaisingEvents = true;
                }

                if (dispnrGramm != null && Directory.Exists(dispnrGramm.Path))
                {
                    dispnrGramm.EnableRaisingEvents = true;
                }

                checkBoxAVX.Visible = button10.Visible;
                if (checkBoxAVX.Visible)
                {
                    if (GRALSettings.AVX512Usage == 1 && System.Runtime.Intrinsics.Vector512.IsHardwareAccelerated)
                    {
                        checkBoxAVX.Checked = true;
                    }
                    else
                    {
                        checkBoxAVX.Checked = false;
                    }
                    if (System.Runtime.Intrinsics.Vector512.IsHardwareAccelerated)
                    {
                        checkBoxAVX.Enabled = true;
                    }
                    else
                    {
                        checkBoxAVX.Enabled = false;
                    }
                }
#endif
            }
            else  // not the computation tab -> stop FileSystemWatcher
            {
#if __MonoCS__
#else
                if (percentGRAL != null)
                {
                    percentGRAL.EnableRaisingEvents = false;
                }

                if (dispnrGRAL != null)
                {
                    dispnrGRAL.EnableRaisingEvents = false;
                }

                if (percentGramm != null)
                {
                    percentGramm.EnableRaisingEvents = false;
                }

                if (dispnrGramm != null)
                {
                    dispnrGramm.EnableRaisingEvents = false;
                }
#endif
            }
            // Lock elements because they do not exist if project is loaded
            GrammLockedLockElements(GRAMM_Locked);
            ProjectLockedLockElements(Project_Locked);
        }

        /// <summary>
        /// Output to the textbox "meteorological input" in the computation tab
        /// </summary>
        void Textbox16_Set(string a)
        {
            if (textBoxGrammTerrain.Text.Length == 0)
            {
                GralStaticFunctions.St_F.SetTrimmedTextToTextBox(textBox16, a);
            }
            else
            {
                GralStaticFunctions.St_F.SetTrimmedTextToTextBox(textBox16, GRAMMwindfield);
            }
        }

        /// <summary>
        /// Set the bitmap for the vertical stretching factor button
        /// </summary>
        void SetButton12Bitmap()
        {
            if (FlowFieldStretchFlexible.Count == 0)
            {
                button12.BackgroundImage = Gral.Properties.Resources.Konfig;
            }
            else
            {
                button12.BackgroundImage = Gral.Properties.Resources.KonfigYellow;
            }
        }

        /// <summary>
        /// Set the bitmap for the special options button
        /// </summary>
        void SetButton57Bitmap()
        {
            if (GRALSettings.WriteESRIResult ||
                File.Exists(Path.Combine(Main.ProjectName, "Computation", "KeepAndReadTransientTempFiles.dat")) ||
                !GRALSettings.WaitForKeyStroke ||
                GRALSettings.PrognosticSubDomainsSizeSourceRadius >= 50)
            {
                button57.BackgroundImage = Gral.Properties.Resources.WrenchYellow;
            }
            else
            {
                button57.BackgroundImage = Gral.Properties.Resources.WrenchBlue;
            }
        }

        /// <summary>
        /// Load and show comments
        /// </summary>
        void Button44Click(object sender, EventArgs e)
        {
            //loading comments
            textBox17.Text = String.Empty;
            string name11 = Path.Combine(ProjectName, @"Settings", "comments.txt");
            try
            {
                using (StreamReader ready = new StreamReader(name11, false))
                {
                    textBox17.Text = ready.ReadToEnd();
                }
            }
            catch
            {
                string a = "Project:	" + ProjectName + Environment.NewLine;
                a += "Machine: 	" + Environment.MachineName + Environment.NewLine;
                a += "User:    	" + Environment.UserName + Environment.NewLine;
                a += "Domain:  	" + Environment.UserDomainName + Environment.NewLine;
                a += "Proc count: " + Environment.ProcessorCount + Environment.NewLine;
                a += "GRAMM Windfield: " + GRAMMwindfield;
                textBox17.Text = a;
            }
            panel1.Visible = false;
            panel1.Hide();
            textBox17.ReadOnly = false;
            textBox17.Visible = true;
            textBox17.Show();
            button38.Visible = true;
            button38.Show();
        }

        void TabControl1Deselected(object sender, TabControlEventArgs e)
        {
            if (tabControl1.SelectedIndex != 1)
            {
                return;
            }

            CheckGralInputData(); // check if
        }

        /// <summary>
        /// Read and set the number of the processor cores in the file Max_Proc.txt
        /// </summary>
        void MaxProcFileRead()
        {
            //Read the actual number of threads to be used in each parallelized region
            int prozessoranzahl = Math.Min(63, Environment.ProcessorCount);
            try
            {
                string maxproc = Path.Combine(ProjectName, @"Computation", "Max_Proc.txt");
                if (File.Exists(maxproc))
                {
                    try
                    {
                        using (StreamReader myreader = new StreamReader(maxproc))
                        {
                            string text = myreader.ReadLine();
                            prozessoranzahl = Math.Min(63, Math.Max(1, Convert.ToInt32(text)));
                        }
                    }
                    catch { }
                }
            }
            catch
            { }
            numericUpDown32.Value = prozessoranzahl;
        }

        /// <summary>
        /// Save the number of the processor cores to the file Max_Proc.txt
        /// </summary>
        void NumericUpDown32ValueChanged(object sender, EventArgs e)
        {
            if (ProjectName.Length > 0) // if a project exists
            {
                //user defines maximum number of processors to be used in the simulations
                string maxproc = Path.Combine(ProjectName, @"Computation", "Max_Proc.txt");
                try
                {
                    using (StreamWriter procwrite = new StreamWriter(maxproc))
                    {
                        procwrite.WriteLine(Convert.ToString(numericUpDown32.Value));
                    }
                }
                catch
                {
                    MessageBox.Show("Unable to write file 'Max_Proc.txt'", "GUI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Load the main form
        /// </summary>
        void MainLoad(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Write steady state files checkbox changed
        /// </summary>
        private void CheckBox27_CheckedChanged(object sender, EventArgs e)
        {
            //save GRAMM control file "GRAMMin.dat"
            GRAMMin(EmifileReset);
        }

        /// <summary>
        /// Open one of the topmost 10 used project files
        /// </summary>
        void Button46Click(object sender, EventArgs e)
        {
            using (GralMainForms.MostRecentFiles MRF = new GralMainForms.MostRecentFiles())
            {
                if (MRF.ShowDialog() == DialogResult.OK)
                {
                    string folder = MRF.SelectedFile;
                    MRF.Close();
                    MRF.Dispose();
                    if (folder.Length > 2) // if a folder is selected
                    {
                        LoadProject(folder);
                    }
                }
            }
        }

        /// <summary>
        /// Save user comments
        /// </summary>
        private void Button38_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ProjectName))
            {
                return; // exit if no project loaded
            }

            string name = Path.Combine(ProjectName, @"Settings", "comments.txt");
            try
            {
                using (StreamWriter write = new StreamWriter(name, false))
                {
                    write.WriteLine(Convert.ToString(textBox17.Text));
                }
            }
            catch { }
        }

        /// <summary>
        /// Set the compression mode for GRAL result files
        /// </summary>
        private void NumericUpDown43_ValueChanged(object sender, EventArgs e)
        {
            GRALSettings.Compressed = (int)numericUpDown43.Value;
            ResetInDat();
        }

        /// <summary>
        /// Change the label color for the emission botton
        /// </summary>
        public void SetEmissionFilesInvalid()
        {
            ChangeButtonLabel(Gral.ButtonColorEnum.ButtonEmission, ButtonColorEnum.RedDot); // Emission label red
        }
        /// <summary>
        /// Change the label color for the computation buttons
        /// </summary>
        /// <param name="button">10 = Control, 11= meteo, 12 = emission, 13 = building</param>
        /// <param name="Mode">0 = red dot, 1 = green dot, 2 = black hook, -1 invisible</param>
        public void ChangeButtonLabel(ButtonColorEnum button, ButtonColorEnum Mode)
        {
            if (button == ButtonColorEnum.ButtonControl)
            {
                if (Mode == ButtonColorEnum.Invisible)
                {
                    pictureBox1.Visible = false;
                    Control_OK = false;
                }
                else
                {
                    pictureBox1.Visible = true;
                }

                if (Mode == ButtonColorEnum.RedDot)
                {
                    pictureBox1.Image = Gral.Properties.Resources.RedDot;
                    Control_OK = false;
                }
                if (Mode == ButtonColorEnum.GreenDot)
                {
                    pictureBox1.Image = Gral.Properties.Resources.GreenDot;
                    Control_OK = false;
                }
                if (Mode == ButtonColorEnum.BlackHook)
                {
                    pictureBox1.Image = Gral.Properties.Resources.BlackHook;
                    Control_OK = true;
                }
            }
            if (button == ButtonColorEnum.ButtonMeteo)
            {
                if (Mode == ButtonColorEnum.Invisible)
                {
                    pictureBox2.Visible = false;
                    Meteo_OK = false;
                }
                else
                {
                    pictureBox2.Visible = true;
                }

                if (Mode == ButtonColorEnum.RedDot)
                {
                    pictureBox2.Image = Gral.Properties.Resources.RedDot;
                    Meteo_OK = false;
                }
                if (Mode == ButtonColorEnum.GreenDot)
                {
                    pictureBox2.Image = Gral.Properties.Resources.GreenDot;
                    Meteo_OK = false;
                }
                if (Mode == ButtonColorEnum.BlackHook)
                {
                    pictureBox2.Image = Gral.Properties.Resources.BlackHook;
                    Meteo_OK = true;
                }
            }
            if (button == ButtonColorEnum.ButtonEmission)
            {
                if (Mode == ButtonColorEnum.Invisible)
                {
                    pictureBox3.Visible = false;
                    Emission_OK = false;
                }
                else
                {
                    pictureBox3.Visible = true;
                }

                if (Mode == ButtonColorEnum.RedDot)
                {
                    pictureBox3.Image = Gral.Properties.Resources.RedDot;
                    Emission_OK = false;
                }
                if (Mode == ButtonColorEnum.GreenDot)
                {
                    pictureBox3.Image = Gral.Properties.Resources.GreenDot;
                    Emission_OK = false;
                }
                if (Mode == ButtonColorEnum.BlackHook)
                {
                    pictureBox3.Image = Gral.Properties.Resources.BlackHook;
                    Emission_OK = true;
                }
            }
            if (button == ButtonColorEnum.ButtonBuildings)
            {
                if (Mode == ButtonColorEnum.Invisible)
                {
                    pictureBox4.Visible = false;
                    Building_OK = true; // no buildings!
                }
                else
                {
                    pictureBox4.Visible = true;
                }

                if (Mode == ButtonColorEnum.RedDot)
                {
                    pictureBox4.Image = Gral.Properties.Resources.RedDot;
                    Building_OK = false;
                    try // delete buildings.dat if file exists
                    {
                        string newPath1 = Path.Combine(ProjectName, @"Computation", "buildings.dat");
                        if (Project_Locked == false)
                        {
                            File.Delete(newPath1);
                        }
                    }
                    catch { }
                    try // delete vegetation.dat if file exists
                    {
                        string newPath1 = Path.Combine(ProjectName, @"Computation", "vegetation.dat");
                        if (Project_Locked == false)
                        {
                            File.Delete(newPath1);
                        }
                    }
                    catch { }
                }
                if (Mode == ButtonColorEnum.GreenDot)
                {
                    pictureBox4.Image = Gral.Properties.Resources.GreenDot;
                    Building_OK = false;
                }
                if (Mode == ButtonColorEnum.BlackHook)
                {
                    pictureBox4.Image = Gral.Properties.Resources.BlackHook;
                    Building_OK = true;
                }
            }
        }

        /// <summary>
        /// Enable the Transient GRAL mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox32_CheckedChanged_1(object sender, EventArgs e)
        {
            if (EmifileReset == true)
            {
                if (checkBox32.Checked == false || MessageBox.Show(this, "Enable GRAL transient mode? Caution: the computation slows down" + Environment.NewLine + "and GRAL version 18.03 or higher is needed!",
                                    "GRAL GUI", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    if (checkBox32.Checked == true)
                    {
                        label13.Enabled = true;
                        numericUpDown34.Enabled = true;
                        checkBox34.Enabled = true;
                        groupBox21.Visible = true; // Wet Deposition settings
                        groupBox24.Visible = true; // Decay rate
                        numericUpDown5.Value = 1;  // lock setting of start dispersion situtation
                        numericUpDown5.Enabled = false; // lock setting of start dispersion situtation
                        button60.Enabled = false;
                        ProjectSetting.EmissionModulationPath = Path.Combine(ProjectName, @"Computation");
                        ProjectSetting.EvaluationPath = Path.Combine(ProjectName, @"Maps");
                    }
                    else
                    {
                        numericUpDown5.Enabled = true; // release setting of start dispersion situtation
                        label13.Enabled = false;
                        numericUpDown34.Enabled = false;
                        checkBox34.Enabled = false;
                        groupBox21.Visible = false; // Wet Deposition settings
                        groupBox24.Visible = false; // Decay rate
                    }
                    ResetInDat();
                }
                else
                {
                    checkBox32.Checked = false;
                    checkBox34.Checked = false;
                }
            }
        }

        /// <summary>
        /// Set the cut-off concentration for transient simulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown34_ValueChanged(object sender, EventArgs e)
        {
            CultureInfo ic = CultureInfo.InvariantCulture;
            try
            {
                // write set cut-off concentration to file
                string path = Path.Combine(ProjectName, @"Computation", "GRAL_Trans_Conc_Threshold.txt");
                using (StreamWriter write = new StreamWriter(path))
                {
                    write.WriteLine(numericUpDown34.Value.ToString("0.0000", ic));
                }
            }
            catch { }
        }

        /// <summary>
        /// Create emission_timeseries.txt based on the defined emission modulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button48Click(object sender, EventArgs e)
        {
            if (Write_Emission_Timeseries())
            {
                button49.Visible = true;
                button51.Visible = true;
                TabControl1Click(null, null);
            }
        }

        /// <summary>
        /// Show the emission time series in a new form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button49Click(object sender, EventArgs e)
        {
            GralMainForms.ShowEmissionTimeseries ets = new GralMainForms.ShowEmissionTimeseries()
            {
                SG_Show = -1
            };

            ets.ModulationPath = ProjectSetting.EmissionModulationPath;
            ets.StartPosition = FormStartPosition.Manual;
            ets.Left = St_F.GetScreenAtMousePosition() + 20;
            ets.Top = GralStaticFunctions.St_F.GetTopScreenAtMousePosition() + 150;
            ets.Show();
        }

        /// <summary>
        /// Delete the emission time series file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DeleteEmissionTimeseriesFile_Click(object sender, EventArgs e)
        {
            DeleteEmissionTimeseriesFile(sender, e);
        }

        /// <summary>
        /// Save changes in the decay rate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NumericUpDown35ValueChanged(object sender, EventArgs e)
        {
            ListBox5_SelectedIndexChanged(sender, null);
        }

        /// <summary>
        /// Force GRAL to save a vertical concentration grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CheckBox34Click(object sender, EventArgs e)
        {
            if (checkBox32.Checked == true)
            {
                string name = Path.Combine(ProjectName, @"Computation", "GRAL_Vert_Conc.txt");
                try
                {
                    if (checkBox34.Checked)
                    {
                        File.Create(name).Close();
                    }
                    else
                    {
                        File.Delete(name);
                    }
                }
                catch { }
            }
            else
            {
                checkBox34.Checked = false;
            }
        }

        /// <summary>
        /// Open the GUI settings form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button50Click(object sender, EventArgs e)
        {
            GralMainForms.GUI_Settings settings = new GralMainForms.GUI_Settings
            {
                StartPosition = FormStartPosition.Manual,
                Location = new System.Drawing.Point(this.Left, this.Top),
                Owner = this
            };
            settings.Show();
        }

        /// <summary>
        /// Load and show log file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button52Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Path.Combine(ProjectName, "Computation");
                dialog.Filter = "(Logfile*.txt)|Logfile*.txt";
                dialog.Title = "Select Logfile";
                // dialog.ShowHelp = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBox17.Text = String.Empty;
                    try
                    {
                        using (StreamReader ready = new StreamReader(dialog.FileName))
                        {
                            textBox17.Text = ready.ReadToEnd();
                        }
                    }
                    catch
                    {
                        textBox17.Text += "Error reading the log file";
                    }
                    panel1.Visible = false;
                    panel1.Hide();
                    button38.Hide();
                    textBox17.ReadOnly = true;
                    textBox17.Visible = false;
                    textBox17.Show();
                }
            }
        }

        /// <summary>
        /// Set the path for *.gff files 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button11Click(object sender, EventArgs e)
        {
            if (Project_Locked)
            {
                MessageBox.Show(this, St_F.GetGffFilePath(Path.Combine(ProjectName, "Computation")), "GRAL GUI *.gff File path", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string gff_filepath = St_F.GetGffFilePath(Path.Combine(ProjectName, "Computation"));
                using (FolderBrowserDialog dialog = new FolderBrowserDialog
                {
                    Description = "Select the path for gff files",
                    SelectedPath = gff_filepath
                })
                {
#if NET6_0_OR_GREATER
                    dialog.UseDescriptionForTitle = true;
#endif
                    dialog.ShowDialog();
                    gff_filepath = dialog.SelectedPath;
                    if (Directory.Exists(gff_filepath))
                    {
                        try
                        {
                            using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, "Computation", "GFF_FilePath.txt")))
                            {
                                writer.WriteLine(gff_filepath);
#if __MonoCS__
                                writer.WriteLine(gff_filepath);
#endif
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// Set the compression type for *.gff files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown41_ValueChanged(object sender, EventArgs e)
        {
            if (EmifileReset == true)
            {
                CheckBox26_CheckedChanged(null, null);
            }
        }

        /// <summary>
        /// Set a flexible vertical stretching factor for the flow field grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button12_Click(object sender, EventArgs e)
        {
            using (GralMainForms.FlexibleStretchingFactors FStF = new GralMainForms.FlexibleStretchingFactors()
            {
                FlexibleStretchingFactor = FlowFieldStretchFlexible,
                ProjectLocked = Project_Locked,
                StartPosition = FormStartPosition.Manual,
                Left = this.Right,
                Top = this.Top + 200
            })
            {
                FStF.ShowDialog();
                if (!Project_Locked)
                {
                    FlowFieldStretchFlexible = FStF.FlexibleStretchingFactor;
                    if (FlowFieldStretchFlexible.Count > 0)
                    {
                        numericUpDown12.Value = 1.00M;
                    }
                    else
                    {
                        numericUpDown12.Value = 1.01M;
                    }
                    SetButton12Bitmap();
                    WriteGralGebFile();
                    ResetInDat();
                }
            }
        }

        /// <summary>
        /// Set decay rates for bioaerosols and unknown pollutants
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button54_Click(object sender, EventArgs e)
        {
            using (GralMainForms.DecayRateForm DRF = new GralMainForms.DecayRateForm()
            {
                DecayRate = DecayRate,
                ProjectLocked = Project_Locked,
                StartPosition = FormStartPosition.Manual,
                Left = this.Right,
                Top = this.Top + 200
            })
            {
                DRF.ShowDialog();
                if (!Project_Locked)
                {
                    DecayRate = DRF.DecayRate;
                    ListBox5_SelectedIndexChanged(null, null);
                }
            }
        }

        /// <summary>
        /// Show the vertical flow field grid layer heights
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button53_Click(object sender, EventArgs e)
        {
            using (GralMainForms.VerticalLayerHeights VLH = new GralMainForms.VerticalLayerHeights()
            {
                StretchFlexible = FlowFieldStretchFlexible,
                StretchingFactor = (float)numericUpDown12.Value,
                LowestCellHeight = (float)numericUpDown11.Value,
                StartPosition = FormStartPosition.Manual,
                Left = this.Right,
                Top = this.Top + 250
            })
            {
                VLH.ShowDialog();
            }
        }

        /// <summary>
        /// Show a simple App info form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button55_Click(object sender, EventArgs e)
        {
            bool open = false;
            foreach (Form f in Application.OpenForms)
            {
                if (f.Name.Contains("AppInfo"))
                {
                    open = true;
                    f.BringToFront();
                    f.WindowState = FormWindowState.Normal;
                }
            }
            if (!open)
            {
                GralMainForms.AppInfo Ai = new GralMainForms.AppInfo()
                {
                    StartPosition = FormStartPosition.Manual,
                    Location = new System.Drawing.Point(this.Left, this.Top),
                    Owner = this
                };
                Ai.Show();
            }
        }

        /// <summary>
        /// Special GRAL options - Write Ascii results - KeepAndReadTransientFiles
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button57_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "These special settings are only intended for a few applications. Do not proceed if you cannot assess the effects", "GRAL GUI", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                using (GralMainForms.Main_SpecialSettings MSp = new GralMainForms.Main_SpecialSettings())
                {
                    MSp.WriteASCiiOutput = GRALSettings.WriteESRIResult;
                    MSp.KeyStrokeWhenExitGRAL = GRALSettings.WaitForKeyStroke;
                    MSp.LogLevel = GRALSettings.Loglevel;
                    MSp.RadiusForPrognosticFlowField = GRALSettings.PrognosticSubDomainsSizeSourceRadius;
                    MSp.GRALOnlineFunctions = GRALSettings.UseGRALOnlineFunctions;
                    MSp.GRALReproducibleResults = GRALSettings.ReproducibleResults;

                    MSp.StartPosition = FormStartPosition.Manual;
                    MSp.Left = Left + 80;
                    MSp.Top = Top + 40;

                    if (MSp.ShowDialog() == DialogResult.OK)
                    {
                        if (MSp.WriteASCiiOutput != GRALSettings.WriteESRIResult ||
                            MSp.KeyStrokeWhenExitGRAL != GRALSettings.WaitForKeyStroke ||
                            MSp.RadiusForPrognosticFlowField != GRALSettings.PrognosticSubDomainsSizeSourceRadius ||
                            MSp.GRALOnlineFunctions != GRALSettings.UseGRALOnlineFunctions ||
                            MSp.GRALReproducibleResults != GRALSettings.ReproducibleResults)
                        {
                            GRALSettings.WriteESRIResult = MSp.WriteASCiiOutput;
                            GRALSettings.WaitForKeyStroke = MSp.KeyStrokeWhenExitGRAL;
                            GRALSettings.PrognosticSubDomainsSizeSourceRadius = MSp.RadiusForPrognosticFlowField;
                            GRALSettings.UseGRALOnlineFunctions = MSp.GRALOnlineFunctions;
                            GRALSettings.ReproducibleResults = MSp.GRALReproducibleResults;
                            ResetInDat();
                        }
                        GRALSettings.Loglevel = MSp.LogLevel;
                        SetButton57Bitmap();
                    }
                }
            }
        }

        private void CheckBox29_CheckStateChanged(object sender, EventArgs e)
        {
            if (IndatReset)
            {
                if (checkBox29.Checked && GRALSettings.AdaptiveRoughness < 0.01)
                {
                    numericUpDown45.Value = 0.5M;
                }
                else
                {
                    numericUpDown45.Value = 0;
                }
                ResetInDat();
            }
        }

        private void NumericUpDown46_ValueChanged(object sender, EventArgs e)
        {
            BuildingCellCoverageThreshold = Convert.ToInt32(numericUpDown46.Value);
            WriteBuildingCoverageThreshold();
        }

        private void WriteBuildingCoverageThreshold()
        {
            try
            {
                using (StreamWriter mywriter = new StreamWriter(Path.Combine(ProjectName, @"Settings", "BuildCovThreshold.txt")))
                {
                    mywriter.WriteLine(BuildingCellCoverageThreshold.ToString(ic));
                }
            }
            catch
            { }
        }
        private void ReadBuildingCoverageThreshold()
        {
            try
            {
                using (StreamReader myreader = new StreamReader(Path.Combine(ProjectName, @"Settings", "BuildCovThreshold.txt")))
                {
                    BuildingCellCoverageThreshold = Convert.ToInt32(myreader.ReadLine(), ic);
                }
            }
            catch
            {
                BuildingCellCoverageThreshold = 5;
            }
            numericUpDown46.ValueChanged -= new System.EventHandler(this.NumericUpDown46_ValueChanged);
            numericUpDown46.Value = BuildingCellCoverageThreshold;
            numericUpDown46.ValueChanged += new System.EventHandler(this.NumericUpDown46_ValueChanged);
        }


        /// <summary>
        /// Save meteo time series as new *.met file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button58_Click(object sender, EventArgs e)
        {
            SaveMetData(MeteoTimeSeries);
        }

        /// <summary>
        /// Start the form for selecting the GRAMM/GRAL online parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button59_Click(object sender, EventArgs e)
        {
            GralMainForms.OnlineParameters online = new GralMainForms.OnlineParameters()
            {
                GRAMMGroupBoxVisible = groupBox15.Visible,
                OnlineGroupBoxVisible = groupBox17.Visible,
                OnlineRefreshInterval = this.OnlineRefreshInterval,
                NumberOfGRAMMLayers = Convert.ToInt32(numericUpDown16.Value),
                NumberOfGRALayers = Convert.ToInt32(numericUpDown26.Value),
                ProjectName = ProjectName
            };
            online.Closing += new System.ComponentModel.CancelEventHandler(OnlineParametersFormClosing);
            online.Location = new Point(this.Left + 150, this.Top + 50);
            online.Show();
        }
        /// <summary>
        /// The online parameter selection form has been closed -> take the refresh interval
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnlineParametersFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GralMainForms.OnlineParameters online = (GralMainForms.OnlineParameters)sender;
            OnlineRefreshInterval = online.OnlineRefreshInterval;
            OnlineParmeters = online.OnlineCheckBoxes;
        }

        /// <summary>
        /// Check for an update of a new GUI/GRAL version
        /// </summary>
        /// <param name="ReportError">Show "error" and "No update available" messages?</param>
        public static void AutoUpdateStart(bool ReportError)
        {
            GralMainForms.UpdateNotification upd = new GralMainForms.UpdateNotification()
            {
                RecentVersion = Application.ProductVersion,
                ShowUserInfo = ReportError
            };
            upd.LoadUpdateFile();
        }

        /// <summary>
        /// Open mail application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel2_MouseClick(object sender, MouseEventArgs e)
        {
            Point point = panel1.PointToClient(Cursor.Position);
            MessageBox.Show(point.ToString());
        }

        /// <summary>
        /// Check for mouse click on mail adress of IVT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            Point point = panel1.PointToClient(Cursor.Position);
            if (OpenMailToIVT.Contains(point))
            {
                Clipboard.SetText("gral@ivt.tugraz.at");
                MessageBox.Show("The mail address was copied to the clipboard");
            }
        }

        /// <summary>
        /// Set a directory for the emission modulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button60_Click(object sender, EventArgs e)
        {
            string emissionModulation = Path.Combine(ProjectName, "Computation");
            if (Directory.Exists(Main.ProjectSetting.EmissionModulationPath))
            {
                emissionModulation = Main.ProjectSetting.EmissionModulationPath;
            }
            using (FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = "Select the path for the emission modulation",
                SelectedPath = emissionModulation
            })
            {
#if NET6_0_OR_GREATER
                dialog.UseDescriptionForTitle = true;
#endif
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    emissionModulation = dialog.SelectedPath;
                    if (Directory.Exists(emissionModulation))
                    {
                        ProjectSetting.EmissionModulationPath = emissionModulation;
                        ProjectSetting.EvaluationPath = emissionModulation;
                        ProjectSetting.WriteToFile();
                        GralStaticFunctions.St_F.SetTrimmedTextToTextBox(LabelEmissionPath, emissionModulation);

                        //Copy emission files to the new folder
                        if (!string.Equals(ProjectSetting.EmissionModulationPath, Path.Combine(ProjectName, "Computation")))
                        {
                            string src, dest;
                            for (int itm = 1; itm < 100; itm++)
                            {
                                src = Path.Combine(ProjectName, "Computation", "emissions" + itm.ToString().PadLeft(3, '0') + ".dat");
                                dest = Path.Combine(ProjectSetting.EmissionModulationPath, "emissions" + itm.ToString().PadLeft(3, '0') + ".dat");
                                CopyFilesIfNotExistant(src, dest);
                            }
                            src = Path.Combine(ProjectName, "Computation", "emissions_timeseries.txt");
                            dest = Path.Combine(ProjectSetting.EmissionModulationPath, "emissions_timeseries.txt");
                            CopyFilesIfNotExistant(src, dest);
                            src = Path.Combine(ProjectName, "Settings", "emissionmodulations.txt");
                            dest = Path.Combine(ProjectSetting.EmissionModulationPath, "emissionmodulations.txt");
                            CopyFilesIfNotExistant(src, dest);
                        }
                        else
                        {
                            string emissionModulationPath = Path.Combine(ProjectName, @"Computation");
                            ProjectSetting.EmissionModulationPath = emissionModulationPath;
                            ProjectSetting.EvaluationPath = Path.Combine(ProjectName, @"Maps");
                            ProjectSetting.WriteToFile();
                            GralStaticFunctions.St_F.SetTrimmedTextToTextBox(LabelEmissionPath, emissionModulationPath);
                        }
                    }
                    else
                    {
                        //Default values
                        string emissionModulationPath = Path.Combine(ProjectName, @"Computation");
                        ProjectSetting.EmissionModulationPath = emissionModulationPath;
                        ProjectSetting.EvaluationPath = Path.Combine(ProjectName, @"Maps");
                        ProjectSetting.WriteToFile();
                        GralStaticFunctions.St_F.SetTrimmedTextToTextBox(LabelEmissionPath, emissionModulationPath);
                    }
                    //force a redraw of the preview
                    RedrawPreviewOfModulation(sender, e);
                }
            }
        }

        private void CopyFilesIfNotExistant(string src, string dest)
        {
            try
            {
                if (File.Exists(src) && !File.Exists(dest))
                {
                    File.Copy(src, dest);
                }
            }
            catch { }
        }

        private void listBox5_DoubleClick(object sender, EventArgs e)
        {
            ShowTotalEmissions(sender, e);
        }

        private void checkBoxAVX_Click(object sender, EventArgs e)
        {
#if __MonoCS__
            if (checkBoxAVX.Checked)
            {
                GRALSettings.AVX512Usage = 1;
            }
            else
            {
                GRALSettings.AVX512Usage = 0;
            }

#else
            if (checkBoxAVX.Checked && System.Runtime.Intrinsics.Vector512.IsHardwareAccelerated)
            {
                GRALSettings.AVX512Usage = 1;
            }
            else
            {
                GRALSettings.AVX512Usage = 0;
            }
#endif
            InDatFileIO write_in_dat = new InDatFileIO
            {
                Data = GRALSettings
            };
            write_in_dat.WriteInDat();
            write_in_dat = null;
        }
    }
}
