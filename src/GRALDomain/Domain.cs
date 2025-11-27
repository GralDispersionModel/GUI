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

using Gral.GRALDomForms;
using GralDomForms;
using GralIO;
using GralItemData;
using GralItemForms;
using GralMessage;
using GralStaticFunctions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace GralDomain
{
    public delegate void DomainformClosed(object sender, EventArgs e);

    /// <summary>
    /// delegate to send Message, that function should start
    /// </summary>
    public delegate void SendCoordinates(object sender, EventArgs e);

    public partial class Domain : Form
    {
        private readonly string decsep;                                                // global decimal separator of the system
        public readonly Gral.Main MainForm = null;
        private string MapFileName;
        private string ImageFileNameFromWorldFile;
        /// <summary>
        /// Scaling factor for the picturebox picture
        /// </summary>
        private double XFac = 1;
        /// <summary>
        /// Recent function using the mouse
        /// </summary>
        public MouseMode MouseControl = MouseMode.Default;
        /// <summary>
        /// Position whren moving a map with middle mouse button
        /// </summary>
        private int OldXPosition;
        /// <summary>
        /// Position whren moving a map with middle mouse button
        /// </summary>
        private int OldYPosition;
        /// <summary>
        /// Selected DrawingObject in the object manager
        /// </summary>
        public DrawingObjects ActualEditedDrawingObject;
        /// <summary>
        /// Drawing options for each item in the drawing list
        /// </summary>
        public List<DrawingObjects> ItemOptions = new List<DrawingObjects>();
        /// <summary>
        /// Copied object container 
        /// </summary>
        private readonly CopyObjects CopiedItem = new CopyObjects();
        /// <summary>
        /// form for georeferencing bitmap file using one reference point and a map scale
        /// </summary>
        public Georeference1 GeoReferenceOne = new Georeference1();
        /// <summary>
        /// form for georeferencing bitmap file using two reference points
        /// </summary>
        public Georeference2 GeoReferenceTwo = new Georeference2();
        /// <summary>
        /// Form for editing point sources
        /// </summary>
        public EditPointSources EditPS = new EditPointSources();
        /// <summary>
        /// Form for editing area sources
        /// </summary>
        public EditAreaSources EditAS = new EditAreaSources();
        /// <summary>
        /// Form for editing line sources
        /// </summary>
        public EditLinesources EditLS = new EditLinesources();
        /// <summary>
        /// Form for editing buildings
        /// </summary>
        public EditBuildings EditB = new EditBuildings();
        /// <summary>
        /// Form for editing portal sources
        /// </summary>
        public EditPortalSources EditPortals = new EditPortalSources();
        /// <summary>
        /// Form for editing receptors
        /// </summary>
        public EditReceptors EditR = new EditReceptors();
        /// <summary>
        /// Form for editing walls
        /// </summary>
        public EditWalls EditWall = new EditWalls();
        /// <summary>
        /// Form for editing vegetation areas
        /// </summary>
        public EditVegetation EditVegetation = new EditVegetation();
        /// <summary>
        /// Dialog for meteo stations
        /// </summary>
        private DialogCreateMeteoStation MeteoDialog = new DialogCreateMeteoStation();

        /// <summary>
        /// input of new shape allowed?
        /// </summary>
        public static bool EditSourceShape = true;
        /// <summary>
        /// Point for VerticesEdit
        /// </summary>        
        public static PointD MarkerPoint;
        /// <summary>
        /// Scale of the background picture
        /// </summary>  
        private double BmpScale = 1;
        /// <summary>
        /// X position of starting point of model domain
        /// </summary>  
        private int XDomain;
        /// <summary>
        /// Y position of starting point of model domain
        /// </summary> 
        private int YDomain;

        private Rectangle GRALDomain = new Rectangle();   //rectangle to draw GRAL model domain
        private Rectangle GRAMMDomain = new Rectangle();  //rectangle to draw GRAMM model domain
        private Rectangle PanelZoom = new Rectangle();    //rectangle to draw panelzoom area
        /// <summary>
        /// Block rubberlines at redraw of online GRAMM/GRAL vectors
        /// </summary> 
        private int RubberRedrawAllowed = 0;

        private PointF FirstPointLenght;				  // Lenght measurement
        private readonly ToolTip ToolTipMousePosition;	  // Tooltip for picturebox1
        /// <summary>
        ///  x,y corner points of an area/line source in pixel for intermediate drawing during editing
        /// </summary> 
        private Point[] CornerAreaSource = new Point[10000];
        /// <summary>
        /// [0] = 1st point for lenght label [1] = Point for Rubberline
        /// </summary> 
        private readonly Point[] RubberLineCoors = new Point[2];

        private Bitmap NorthArrowBitmap;                       // Icon for north arrow
        private Bitmap PictureBoxBitmap;					   // Bitmap for the picture box
        private readonly NorthArrowData NorthArrow = new NorthArrowData(); // Data for the north arrow
        private readonly MapScaleData MapScale = new MapScaleData();       //Data for the MapScale

        /// <summary>
        /// Map transformation X when zooming and shifting
        /// </summary> 
        private int TransformX = 0;
        /// <summary>
        /// Map transformation Y when zooming and shifting
        /// </summary> 
        private int TransformY = 0;

        /// <summary>
        /// transformation in x direction of color scale, north arrow, map scale during saving process
        /// </summary>    
        public int TransformXSave = 0;
        /// <summary>
        /// transformation in x direction of color scale, north arrow, map scale during saving process
        /// </summary>    
        public int TransformYSave = 0;
        /// <summary>
        /// scaling of color scale, north arrow, map scale during saving process
        /// </summary>            
        public double BmppbXSave = 1;

        private string GRAMMmeteofile;                         //Meteo-File generated from GRAMM windfield

        /// <summary>
        /// true = contour map should be reloaded and redrawn 
        /// </summary>            
        public bool ReDrawContours = true;
        /// <summary>
        /// true = vector map should be reloaded and redrawn 
        /// </summary>
        public bool ReDrawVectors = true;

        private readonly FileWatcherCollection FileWatch = new FileWatcherCollection(); // Contains all FileSystemWatchers

        public List<Point> sectionpoints = new List<Point>(); // List of section points for the redraw
        /// <summary>
        /// Online Counter for GRAMM and GRAL online and recording of animated GIF files 
        /// </summary>
        private int OnlineCounter = 0;
        /// <summary>
        /// Online or Domain mode?
        /// </summary>
        private readonly bool GRAMMOnline;
        /// <summary>
        /// Event to redraw online GRAL and GRAMM
        /// </summary>
        public event ForceDomainRedraw OnlineRedraw;
        /// <summary>
        /// Lock the redraw 
        /// </summary>
        private int LockOnlineRedraw = 0;

        /// <summary>
        /// Objectmanager form: it is possible to close the objectmanager if domain is closed
        /// </summary>
        public Objectmanager ObjectManagerForm;
        /// <summary>
        /// List of all selectet Items of one type to delete all selected items
        /// </summary>
        private readonly List<int> SelectedItems = new List<int>();
        private string ConcFilename = "";						  // filename for concentration files
        /// <summary>
        /// Array to display GRAMM or GRAL cell heights
        /// </summary>
        private float[,] CellHeights = new float[1, 1];           // Cell heights
        /// <summary>
        /// Height - Type: 0 = no, 1= GRAMM, 2 = GRAL, -1 GRAMM edge points
        /// </summary>
        private int CellHeightsType = 0;
        /// <summary>
        /// variables needed for the routine to import newly observed meteo data
        /// </summary>
        private readonly MatchMeteoData MMOData = new MatchMeteoData();
        /// <summary>
        /// form for matching GRAMM wind fields with multiple observations
        /// </summary>
        public MatchMultipleObservations MMO = new MatchMultipleObservations();

        public VerticalProfileConcentration VerticalProfileForm;
        public DomainformClosed DomainClosed;

        private readonly ShowFirstItem ShowFirst = new ShowFirstItem();	          // contains info about the first visible item form
        /// <summary>
        /// Visible Columns in the search datagridview
        /// </summary>
        private bool[] SearchDatagridviewVisible = new bool[100];
        /// <summary>
        /// Size of the search form
        /// </summary>
        private Size SearchFormSize = new Size(760, 450);
        /// <summary>
        /// Show the lenght label?
        /// </summary>
        private bool ShowLenghtLabel = true;
        /// <summary>
        /// Save the BaseMapData when shifting or zooming a map for reset (ESC key)
        /// </summary>
        private readonly GralData.BaseMapData BaseMapOldValues = new GralData.BaseMapData();

        private GralData.TopoModifyClass TopoModify = new GralData.TopoModifyClass();
        private bool[,] TopoModifyBlocked = new bool[1, 1];
        /// <summary>
        /// Size & Position of Geo-Referenced Map
        /// </summary>
        private readonly MapSizes MapSize = new MapSizes();

        private bool GRAL_Locked = false;
        private bool GRAMM_Locked = false;

        private MessageWindow MessageInfoForm;

        private readonly VerticalWindProfile ProfileConcentration = new VerticalWindProfile();

        /// <summary>
        /// Delegate to force a redraw from child forms
        /// </summary>
        private readonly ForceDomainRedraw DomainRedrawDelegate;

        /// <summary>
        ///Cancel Token for all created await tasks
        /// </summary>
        public static System.Threading.CancellationTokenSource CancellationTokenSource = new System.Threading.CancellationTokenSource();

        /// <summary>
        /// An object for locking purposes
        /// </summary>
        public static readonly object LockObject = new object();

        /// <summary>
        /// Send a event with clicked coordinates to all registered forms
        /// </summary>
        private event SendCoordinates SendCoors;

        /// <summary>
        /// Start the Domain (GIS) Form of this application
        /// </summary>
        /// <param name="f">reference to the Gral.Main form</param>
        /// <param name="online">true: GRAMM/GRAL online mode while computation</param>
        public Domain(Gral.Main f, bool online)
        {
            GRAMMOnline = online; // Online or Domain Mode?
            InitializeComponent();
            DomainRedrawDelegate = new ForceDomainRedraw(Picturebox_Redraw);

            //User defineddecimal seperator
            decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            // reduce flickering
            //this.DoubleBuffered = true;
            // use double buffer to minimize flickering.
            SetStyle(ControlStyles.ResizeRedraw, true);
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //SetStyle(ControlStyles.UserPaint, true);
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            MainForm = f;

            MouseWheel += new MouseEventHandler(form1_MouseWheel); // Kuntner

            //event when matching process should start
            MMO.StartMatchProcess += new StartMatchingDelegate(StartMatchingProcess);
            MMO.CancelMatchProcess += new CancelMatchingDelegate(MatchCancel);
            MMO.FinishMatchProcess += new FinishMatchingDelegate(MatchFinish);
            MMO.LoadWindData += new LoadWindFileData(LoadWindFileForMatching);

            //GRAMM Online options
            if (GRAMMOnline == true)
            {
                Text = "GRAL/GRAMM Online / " + Path.GetFileName(Gral.Main.ProjectName);
            }
            else
            {
                if (Gral.Main.Project_Locked == true)
                {
                    GRAL_Locked = Gral.Main.Project_Locked;
                    GRAMM_Locked = MainForm.GRAMM_Locked;

                    if (MainForm.GRAMM_Locked)
                    {
                        Text = "Domain / " + Path.GetFileName(Gral.Main.ProjectName) + "   (GRAL locked / GRAMM locked)";
                    }
                    else
                    {
                        Text = "Domain / " + Path.GetFileName(Gral.Main.ProjectName) + "   (GRAL locked)";
                    }
                }
                else
                {
                    if (MainForm.GRAMM_Locked)
                    {
                        Text = "Domain / " + Path.GetFileName(Gral.Main.ProjectName) + "   (GRAMM locked)";
                    }
                    else
                    {
                        Text = "Domain / " + Path.GetFileName(Gral.Main.ProjectName);
                    }
                }
            }

            panel1.Top = SystemInformation.MenuHeight;
            panel1.Height = Math.Max(600, ClientSize.Height - SystemInformation.MenuHeight);

#if __MonoCS__
            panel1.Dock = DockStyle.None;
            panel1.Left = 0; panel1.Top =  40;
            groupBox1.Visible = true;
            groupBox2.Visible = true;
#endif

            using (Stream s1 = GetType().Assembly.GetManifestResourceStream("Gral.Resources.North.gif"))
            {
                NorthArrowBitmap = new Bitmap(s1);
            }

            EditPS.PointSourceRedraw += DomainRedrawDelegate; // Redraw from Edit Point Sources
            EditAS.AreaSourceRedraw += DomainRedrawDelegate; // Redraw from areaedit
            EditB.BuildingRedraw += DomainRedrawDelegate; // Redraw from editbuilding
            EditLS.LinesourceRedraw += DomainRedrawDelegate; // Redraw from editls
            EditPortals.PortalSourceRedraw += DomainRedrawDelegate; // Redraw from portal
            EditR.ReceptorRedraw += DomainRedrawDelegate; // Redraw from editreceptors
            EditWall.WallRedraw += DomainRedrawDelegate;  // Redraw from editwalls
            EditVegetation.VegetationRedraw += DomainRedrawDelegate; // Redraw from editforests
            OnlineRedraw += DomainRedrawDelegate; // Redraw from Online GRAL/GRAMM

            EditPS.ItemFormOK += EditAndSavePointSourceData; // OK button from dialog 
            EditPS.ItemFormCancel += CancelItemForms;
            EditAS.ItemFormOK += EditAndSaveAreaSourceData; // OK button from dialog
            EditAS.ItemFormCancel += CancelItemForms;
            EditB.ItemFormOK += EditAndSaveBuildingsData; // OK button from dialog
            EditB.ItemFormCancel += CancelItemForms;
            EditLS.ItemFormOK += EditAndSaveLineSourceData; // OK button from dialog 
            EditLS.ItemFormCancel += CancelItemForms;
            EditPortals.ItemFormOK += EditAndSavePortalSourceData; // OK button from dialog 
            EditPortals.ItemFormCancel += CancelItemForms;
            EditR.ItemFormOK += EditAndSaveReceptorData; // OK button from dialog 
            EditR.ItemFormCancel += CancelItemForms;
            EditWall.ItemFormOK += EditAndSaveWallData; // OK button from dialog
            EditWall.ItemFormCancel += CancelItemForms;
            EditVegetation.ItemFormOK += EditAndSaveVegetationData; // OK button from dialog 
            EditVegetation.ItemFormCancel += CancelItemForms;

            EditPS.StartPosition = FormStartPosition.Manual;
            EditAS.StartPosition = FormStartPosition.Manual;
            EditB.StartPosition = FormStartPosition.Manual;
            EditLS.StartPosition = FormStartPosition.Manual;
            EditPortals.StartPosition = FormStartPosition.Manual;
            EditR.StartPosition = FormStartPosition.Manual;
            EditWall.StartPosition = FormStartPosition.Manual;
            EditVegetation.StartPosition = FormStartPosition.Manual;

            EditPS.TopLevel = false;
            EditAS.TopLevel = false;
            EditB.TopLevel = false;
            EditLS.TopLevel = false;
            EditPortals.TopLevel = false;
            EditR.TopLevel = false;
            EditWall.TopLevel = false;
            EditVegetation.TopLevel = false;
            Controls.Add(EditPS);
            Controls.Add(EditAS);
            Controls.Add(EditB);
            Controls.Add(EditLS);
            Controls.Add(EditPortals);
            Controls.Add(EditR);
            Controls.Add(EditWall);
            Controls.Add(EditVegetation);

            GeoReferenceOne.Form_Georef1_Closed += new Georeference1_Closed(CloseGeoRef1); // Message, that georef1 is closed
            GeoReferenceTwo.Form_Georef2_Closed += new Georeference2_Closed(CloseGeoRef2); // Message, that georef2 is closed

            EditR.MinReceptorHeight = MainForm.GRALSettings.Deltaz * 0.5; // minimum Height from Main()


            //load user defined settings
            LoadSettingsAndMaps();

            //show windfield analysis tools for GRAMM when a windfield is existent
            if (Gral.Main.ProjectName != null && File.Exists(Path.Combine(Gral.Main.ProjectName, @"Computation", "windfeld.txt")))
            {
                groupBox3.Visible = true;
                button48.Visible = true;
                GRAMMWindFieldsToolStripMenuItem.Enabled = true;
                windfieldAnalysisToolStripMenuItem.Enabled = true;
                //check if wind fields are imported -> if yes, then the function match-local-observations can be utilized
                string newPath1 = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar);
                DirectoryInfo di = new DirectoryInfo(newPath1);
                FileInfo[] files_wind = di.GetFiles("*.wnd");
                if (files_wind.Length > 0)
                {
                    button43.Visible = false;
                    matchToObservationToolStripMenuItem.Enabled = false;
                }

                // GRAL topography allowed?
                if (Gral.Main.Project_Locked == false &&
                    MainForm.GralDomRect.East != MainForm.GralDomRect.West && MainForm.GralDomRect.North != MainForm.GralDomRect.South)
                {
                    originalGRALTopographyToolStripMenuItem.Enabled = true;
                }
                else
                {
                    originalGRALTopographyToolStripMenuItem.Enabled = false;
                }
            }
            else
            {
                GRAMMWindFieldsToolStripMenuItem.Enabled = false;
                originalGRALTopographyToolStripMenuItem.Enabled = false;
            }

            // show section button if ggeom.asc does exist
            string ggeom_path;
            if (Gral.Main.GRAMMwindfield != null)
            {
                ggeom_path = Path.Combine(Path.GetDirectoryName(Gral.Main.GRAMMwindfield), @"ggeom.asc");
            }
            else
            {
                ggeom_path = Path.Combine(Gral.Main.ProjectName, @"Computation", "ggeom.asc");
            }

            if (File.Exists(ggeom_path))
            {
                groupBox3.Visible = true;
                button_section_wind.Visible = true;

                windfieldAnalysisToolStripMenuItem.Enabled = true;
            }

            //show windfield analysis tools for GRAL when a windfield is existent
            //check if wind fields are existent
            bool windfieldfiles = WindfieldsAvailable();
            if (windfieldfiles)
            {
                groupBox3.Visible = true;
                button32.Visible = true;
                button41.Visible = true;
                button_section_wind.Visible = true;
                windfieldAnalysisToolStripMenuItem.Enabled = true;
            }

            // Reset Rubber-Line Drawing
            RubberLineCoors[0].X = -1; RubberLineCoors[0].Y = -1;

            ToolTipMousePosition = new ToolTip(); // Tooltip for picturebox1
            ToolTipMousePosition.SetToolTip(picturebox1, "[m]");
            ToolTipMousePosition.UseFading = false;
            ToolTipMousePosition.Active = false;

#if __MonoCS__
            
#else
            ToolTipMousePosition.BackColor = Color.Transparent;
#endif
            ToolTipMousePosition.UseAnimation = false;

            // Try to load cell height information
            TryToLoadCellHeights();
        }

        /// <summary>
        /// Load all settings for the domain window and the background maps
        /// </summary>
        private void LoadSettingsAndMaps()
        {
            try
            {
                LoadDomainSettings();

                //load contour, vector, and shape maps
                try
                {
                    int k = -1;
                    Cursor = Cursors.WaitCursor;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name.Substring(0, 3) == "CM:")
                        {
                            try
                            {
                                //check if contour map file exists
                                if (File.Exists(_drobj.ContourFilename) == false)
                                {
                                    //check first, if map is located in a sub directory of this project
                                    (string tempfilename, bool saveNewFilePath) = St_F.SearchAbsoluteAndRelativeFilePath(Gral.Main.ProjectName, _drobj.ContourFilename, "");
                                    if (File.Exists(tempfilename))
                                    {
                                        _drobj.ContourFilename = tempfilename;
                                        if (saveNewFilePath)
                                        {
                                            SaveDomainSettings(1);
                                        }
                                    }
                                    else
                                    {
                                        //user can define new location of the file
                                        using (OpenFileDialog dialog = new OpenFileDialog
                                        {
                                            Filter = "(*.txt;*.dat)|*.txt;*.dat",
                                            InitialDirectory = Path.Combine(Gral.Main.ProjectName, @"Maps"),
                                            Title = "Contour map " + Convert.ToString(Path.GetFileName(_drobj.ContourFilename)) + " not found - please enter new path",
                                            FileName = Convert.ToString(Path.GetFileName(_drobj.ContourFilename))
                                            //ShowHelp = true
#if NET6_0_OR_GREATER
                                            ,
                                            ClientGuid = GralStaticFunctions.St_F.FileDialogMaps
#endif
                                        })
                                        {
                                            dialog.HelpRequest += new System.EventHandler(LoadSettingsAndMaps_HelpRequest);
                                            if (dialog.ShowDialog(this) == DialogResult.OK)
                                            {
                                                _drobj.ContourFilename = dialog.FileName;
                                                SaveDomainSettings(1);
                                            }
                                        }
                                    }
                                }
                                ReDrawContours = true;
                                if (File.Exists(_drobj.ContourFilename) && _drobj.Show)
                                {
                                    Contours(_drobj.ContourFilename, _drobj);
                                }
                            }
                            catch
                            {
                                RemoveMap(k);
                                MessageBox.Show(this, "Unable to open\n\t" + _drobj.ContourFilename, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        if (_drobj.Name.Substring(0, 3) == "VM:")
                        {
                            try
                            {
                                //check if vector map file exists
                                if (File.Exists(_drobj.ContourFilename) == false)
                                {
                                    //check first, if map is located in a sub directory of this project
                                    (string tempfilename, bool saveNewFilePath) = St_F.SearchAbsoluteAndRelativeFilePath(Gral.Main.ProjectName, _drobj.ContourFilename, "Maps");
                                    if (File.Exists(tempfilename))
                                    {
                                        _drobj.ContourFilename = tempfilename;
                                        if (saveNewFilePath)
                                        {
                                            SaveDomainSettings(1);
                                        }
                                    }
                                    else
                                    {
                                        //user can define new location of the file
                                        using (OpenFileDialog dialog = new OpenFileDialog
                                        {
                                            Filter = "(*.txt;*.dat)|*.txt;*.dat",
                                            InitialDirectory = Path.Combine(Gral.Main.ProjectName, @"Maps"),
                                            Title = "Vector map " + Convert.ToString(Path.GetFileName(_drobj.ContourFilename)) + " not found - please enter new path",
                                            FileName = Path.GetFileName(_drobj.ContourFilename)
#if NET6_0_OR_GREATER
                                            ,
                                            ClientGuid = GralStaticFunctions.St_F.FileDialogMaps
#endif
                                        })
                                        {
                                            dialog.HelpRequest += new System.EventHandler(LoadSettingsAndMaps_HelpRequest);
                                            if (dialog.ShowDialog(this) == DialogResult.OK)
                                            {
                                                _drobj.ContourFilename = dialog.FileName;
                                                SaveDomainSettings(1);
                                            }
                                        }
                                    }
                                }
                                if (File.Exists(_drobj.ContourFilename))
                                {
                                    ReDrawVectors = true;
                                    LoadVectors(_drobj.ContourFilename, _drobj);
                                }
                            }
                            catch
                            {
                                RemoveMap(k);
                                MessageBox.Show(this, "Unable to open\n\t" + _drobj.ContourFilename, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        if (_drobj.Name.Substring(0, 3) == "SM:")
                        {
                            GralShape.ShapeReader shape = new GralShape.ShapeReader(this)
                            {
                                ReadBuildings = false // don't read buildings, read simple background image
                            };
                            Waitprogressbar wait = new Waitprogressbar("Loading shape file");
                            wait.Show();
                            try
                            {
                                //check if shape file exists
                                if (File.Exists(_drobj.ContourFilename) == false)
                                {
                                    //check first, if map is located in a sub directory of this project
                                    (string tempfilename, bool saveNewFilePath) = St_F.SearchAbsoluteAndRelativeFilePath(Gral.Main.ProjectName, _drobj.ContourFilename, "Maps");
                                    if (File.Exists(tempfilename))
                                    {
                                        _drobj.ContourFilename = tempfilename;
                                        if (saveNewFilePath)
                                        {
                                            SaveDomainSettings(1);
                                        }
                                    }
                                    else
                                    {
                                        //user can define new location of the file
                                        using (OpenFileDialog dialog = new OpenFileDialog
                                        {
                                            Filter = "(*.shp)|*.shp",
                                            InitialDirectory = Path.Combine(Gral.Main.ProjectName, @"Maps"),
                                            Title = "Shape file " + Convert.ToString(Path.GetFileName(_drobj.ContourFilename)) + " not found - please enter new path",
                                            FileName = Path.GetFileName(_drobj.ContourFilename)
#if NET6_0_OR_GREATER
                                            ,
                                            ClientGuid = GralStaticFunctions.St_F.FileDialogMaps
#endif
                                        })
                                        {
                                            dialog.HelpRequest += new System.EventHandler(LoadSettingsAndMaps_HelpRequest);
                                            if (dialog.ShowDialog(this) == DialogResult.OK)
                                            {
                                                _drobj.ContourFilename = dialog.FileName;
                                                SaveDomainSettings(1);
                                            }
                                        }
                                    }
                                }

                                if (File.Exists(_drobj.ContourFilename))
                                {
                                    int count = 0;
                                    foreach (object shp in shape.ReadShapeFile(_drobj.ContourFilename))
                                    {
                                        if (shp is GralShape.SHPLine)
                                        {
                                            _drobj.ShpLines.Add((GralShape.SHPLine)shp);
                                        }

                                        if (shp is GralShape.SHPPolygon)
                                        {
                                            _drobj.ShpPolygons.Add((GralShape.SHPPolygon)shp);
                                        }

                                        if (shp is PointF)
                                        {
                                            _drobj.ShpPoints.Add((PointF)shp);
                                        }

                                        if (count == 0)
                                        {
                                            _drobj.West = shape.West;
                                            _drobj.North = shape.North;
                                            _drobj.PixelMx = shape.PixelMx;
                                            _drobj.Picture = new Bitmap(100, Math.Abs(shape.PixelMy));
                                        }

                                        count++;

                                        if (count % 200 == 0)
                                        {
                                            wait.Text = "Loading shape file " + count.ToString();
                                        }
                                    }
                                }

                            }
                            catch
                            {
                                MessageBox.Show(this, "Unable to open\n\t" + _drobj.ContourFilename, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                RemoveMap(k);
                            }
                            shape = null;
                            wait.Close();
                            wait.Dispose();
                            wait = null;
                        }
                    }
                }
                catch
                {
                }

                Cursor = Cursors.Default;
            }
            catch
            {
            }
        }

        private void LoadSettingsAndMaps_HelpRequest(object sender, System.EventArgs e)
        {
            MessageBox.Show(this, "The map is not available. Maybe the path has changed \n Point to the file or press Cancel");
        }

        /// <summary>
        /// Try to load cell height information
        /// </summary>
        private bool TryToLoadCellHeights()
        {
            modifyTopographyToolStripMenuItem.Enabled = false;
            saveTopographyToolStripMenuItem.Enabled = false;
            restoreGRALTopographyToolStripMenuItem.Enabled = false;
            lowPassGRALTopographyToolStripMenuItem.Enabled = false;

            if ((CellHeightsType == 0 || CellHeightsType == 2) && ReadGralGeometry()) // GRAL geometry is available
            {
                bool windfieldfiles = WindfieldsAvailable();
                SetCellHeightsType(2);
                // check, if Topography-Modification is allowed
                if (windfieldfiles == false && Gral.Main.Project_Locked == false)
                {
                    modifyTopographyToolStripMenuItem.Enabled = true;
                    saveTopographyToolStripMenuItem.Enabled = true;
                    restoreGRALTopographyToolStripMenuItem.Enabled = true;
                    lowPassGRALTopographyToolStripMenuItem.Enabled = true;
                }
            }

            if (CellHeightsType < 2) // Try to read GRAMM geometry
            {
                GGeomFileIO ggeom = new GGeomFileIO
                {
                    PathWindfield = Path.GetDirectoryName(Gral.Main.GRAMMwindfield)
                };
                // Mean cell height
                if (CellHeightsType == 1 || CellHeightsType == 0)
                {
                    double[,] AH = new double[1, 1];
                    ggeom.AH = AH;
                    //read mean height only
                    if (ggeom.ReadGGeomAsc(1) == true)
                    {
                        AH = ggeom.AH;
                        ggeom = null;
                        CellHeights = new float[AH.GetUpperBound(0) + 1, AH.GetUpperBound(1) + 1];
                        for (int i = 1; i <= AH.GetUpperBound(0); i++)
                        {
                            for (int j = 1; j <= AH.GetUpperBound(1); j++)
                            {
                                CellHeights[i, j] = (float)Math.Round(AH[i, j], 1);
                            }
                        }
                        SetCellHeightsType(1);
                    }
                }
                // Edge cell height
                else if (CellHeightsType == -1)
                {
                    double[,,] AHE = new double[1, 1, 1];
                    ggeom.AHE = AHE;
                    // read entire ggeom.asc file
                    if (ggeom.ReadGGeomAsc(-1) == true)
                    {
                        AHE = ggeom.AHE;
                        ggeom = null;

                        CellHeights = new float[AHE.GetUpperBound(0) + 1, AHE.GetUpperBound(1) + 1];
                        for (int i = 1; i <= AHE.GetUpperBound(0); i++)
                        {
                            for (int j = 1; j <= AHE.GetUpperBound(1); j++)
                            {
                                CellHeights[i, j] = (float)Math.Round(AHE[i, j, 1], 1);
                            }
                        }
                        SetCellHeightsType(-1);
                    }
                }
            }

            if (Math.Abs(CellHeightsType) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check if *.gff wind fields are available
        /// </summary>
        private bool WindfieldsAvailable()
        {
            //check if wind fields are existent
            string newPath4 = St_F.GetGffFilePath(Path.Combine(Gral.Main.ProjectName, "Computation") + Path.DirectorySeparatorChar);
            DirectoryInfo di2 = new DirectoryInfo(newPath4);
            FileInfo[] files_windgral = di2.GetFiles("*.gff");

            if (files_windgral.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Zoom in using the button in the toolbox
        /// </summary>
        private void Button1_Click(object sender, EventArgs e)
        {
            MouseControl = MouseMode.ZoomIn;
            CursorConverter cv = new CursorConverter();
            Cursor cursor = (Cursor)cv.ConvertFrom(Gral.Properties.Resources.zoom_in);
            Cursor = cursor;
            //prevent parallel editing
            HideWindows(0);
        }

        /// <summary>
        /// Panel zoom using the button in the toolbox
        /// </summary>
        private void Button7_Click(object sender, EventArgs e)
        {
            MouseControl = MouseMode.ViewPanelZoom;
            CursorConverter cv = new CursorConverter();
            Cursor cursor = (Cursor)cv.ConvertFrom(Gral.Properties.Resources.harrow);
            Cursor = cursor;
            //prevent parallel editing
            HideWindows(0);
        }

        /// <summary>
        /// Zoom out using the button in the toolbox
        /// </summary>
        private void Button2_Click(object sender, EventArgs e)
        {
            MouseControl = MouseMode.ZoomOut;
            CursorConverter cv = new CursorConverter();
            Cursor cursor = (Cursor)cv.ConvertFrom(Gral.Properties.Resources.zoom_out);
            Cursor = cursor;
            //prevent parallel editing
            HideWindows(0);
        }

        /// <summary>
        /// Zoom to full extend using the button in the toolbox
        /// </summary>
        private void Button3_Click_1(object sender, EventArgs e)
        {
            HideWindows(0);

            //fit picture to panel (show full map)
            MouseControl = MouseMode.Default;
            Cursor = Cursors.Default;

            //compute deltax and deltay between all base maps
            double xmin = 10000000;
            double ymin = 10000000;
            double xmax = -10000000;
            double ymax = -10000000;
            double fac1 = 1;
            double fac2 = 1;

            foreach (DrawingObjects _dr in ItemOptions)
            {
                if ((_dr.Name.Substring(0, 3) == "BM:") || (_dr.Name.Substring(0, 3) == "SM:"))
                {
                    xmin = Math.Min(xmin, _dr.West);
                    xmax = Math.Max(xmax, _dr.West + _dr.PixelMx * _dr.Picture.Width);
                    ymax = Math.Max(ymax, _dr.North);
                    ymin = Math.Min(ymin, _dr.North - _dr.PixelMx * _dr.Picture.Height);
                }
            }

            fac1 = Convert.ToDouble(picturebox1.Width) / (xmax - xmin) * MapSize.SizeX;
            fac2 = Convert.ToDouble(picturebox1.Height) / (ymax - ymin) * MapSize.SizeX;
            XFac = Math.Min(fac1, fac2);
            BmpScale = 1 / XFac;

            TransformX = Convert.ToInt32(-(xmin - MapSize.West) / BmpScale / MapSize.SizeX);
            TransformY = Convert.ToInt32(-(ymax - MapSize.North) / BmpScale / MapSize.SizeY);

            //set source - and destination rectangle
            foreach (DrawingObjects _dr in ItemOptions)
            {
                try
                {
                    _dr.DestRec = new Rectangle(TransformX + Convert.ToInt32((_dr.West - MapSize.West) / BmpScale / MapSize.SizeX),
                                                TransformY - Convert.ToInt32((_dr.North - MapSize.North) / BmpScale / MapSize.SizeX),
                                                Convert.ToInt32(_dr.Picture.Width * _dr.PixelMx / MapSize.SizeX * XFac),
                                                Convert.ToInt32(_dr.Picture.Height * _dr.PixelMx / MapSize.SizeX * XFac));
                    _dr.SourceRec = new Rectangle(0, 0, _dr.Picture.Width, _dr.Picture.Height);
                }
                catch { }
            }

            //compute GRAL model domain
            try
            {
                int x1 = Convert.ToInt32((MainForm.GralDomRect.West - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                int y1 = Convert.ToInt32((MainForm.GralDomRect.North - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                int x2 = Convert.ToInt32((MainForm.GralDomRect.East - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                int y2 = Convert.ToInt32((MainForm.GralDomRect.South - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                int width = x2 - x1;
                int height = y2 - y1;
                GRALDomain = new Rectangle(x1, y1, width, height);
            }
            catch
            {
            }

            //compute GRAMM model domain
            try
            {
                int x1 = Convert.ToInt32((MainForm.GrammDomRect.West - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                int y1 = Convert.ToInt32((MainForm.GrammDomRect.North - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                int x2 = Convert.ToInt32((MainForm.GrammDomRect.East - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                int y2 = Convert.ToInt32((MainForm.GrammDomRect.South - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                int width = x2 - x1;
                int height = y2 - y1;
                GRAMMDomain = new Rectangle(x1, y1, width, height);
            }
            catch
            {
            }

            Picturebox1_Paint();
        }

        /// <summary>
        /// Move map using the mouse or set or reset the zoom&shift values
        /// </summary>
        void Button4Click(object sender, EventArgs e)
        {
            MouseControl = MouseMode.ViewMoveMap;
            CursorConverter cv = new CursorConverter();
            Cursor cursor = (Cursor)cv.ConvertFrom(Gral.Properties.Resources.lmove);
            Cursor = cursor;
            //prevent parallel editing
            HideWindows(0);
        }

        /// <summary>
        /// Delete Buildings ouside the GRAL domain area
        /// </summary>
        void Button44Click(object sender, EventArgs e)
        {
            if (MainForm.GralDomRect.East > MainForm.GralDomRect.West && MainForm.GralDomRect.South < MainForm.GralDomRect.North) // GRAL domain visible
            {
                SelectedItems.Clear(); // clear the selection list

                int i = 0;
                double x1 = 0;
                double y1 = 0;

                foreach (BuildingData _bd in EditB.ItemData)
                {
                    List<PointD> _pt = _bd.Pt;
                    for (int j = 0; j < _pt.Count; j++)
                    {
                        x1 = _pt[j].X;
                        y1 = _pt[j].Y;

                        if (x1 <= (MainForm.GralDomRect.West + 5 * MainForm.HorGridSize) ||
                            y1 <= (MainForm.GralDomRect.South + 5 * MainForm.HorGridSize) ||
                            y1 >= (MainForm.GralDomRect.North - 5 * MainForm.HorGridSize) ||
                            x1 >= (MainForm.GralDomRect.East - 5 * MainForm.HorGridSize))
                        { // building outside GRAL domain
                            SelectedItems.Add(i);
                            break;
                        }
                    }
                    i++;
                }
                if (SelectedItems.Count > 0)
                {
                    MouseMode temp = MouseControl;
                    MouseControl = MouseMode.BuildingSel; // edit buildings
                    ItemsDelete("building");
                    InfoBoxCloseAllForms(); // close all infoboxes
                    EditAndSaveBuildingsData(sender, e); // save the changes
                    MouseControl = temp;
                    Picturebox1_Paint();
                    MessageBoxTemporary Box = new MessageBoxTemporary("All buildings outside GRAL domain area deleted", Location);
                    Box.Show();
                }
                else
                {
                    MessageBox.Show(this, "No buildings found outside the GRAL domain area", "Delete buildings outside GRAL domain area", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show(this, "Please define GRAL domain", "Delete buildings outside GRAL domain area", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Delete all selected items 
        /// </summary>
        private void ItemsDelete(string a)
        {
            if (SelectedItems.Count > 0)
            {
                if (SelectedItems.Count == 1)
                {
                    a = Convert.ToString(SelectedItems.Count) + " " + a + "?";
                }
                else
                {
                    a = Convert.ToString(SelectedItems.Count) + " " + a + "s?";
                }

                if (St_F.InputBoxYesNo("Attention", "Do you really want to delete " + a, St_F.GetScreenAtMousePosition() + 340, St_F.GetTopScreenAtMousePosition() + 150) == DialogResult.Yes)
                {
                    int i_alt = -1;
                    SelectedItems.Sort(new SortIntDescending()); // sort descending
                    foreach (int i in SelectedItems) // delete all buildings							{
                    {
                        Application.DoEvents();
                        if (i != i_alt) // if one element was selected twice
                        {

                            i_alt = i;
                            if (MouseControl == MouseMode.BuildingSel)
                            {
                                EditB.ItemDisplayNr = i;
                                EditB.RemoveOne(false, false);
                            }
                            else if (MouseControl == MouseMode.PortalSourceSel)
                            {
                                EditPortals.ItemDisplayNr = i;
                                EditPortals.RemoveOne(false);
                            }
                            else if (MouseControl == MouseMode.PointSourceSel)
                            {
                                EditPS.ItemDisplayNr = i;
                                EditPS.RemoveOne(false);
                            }
                            else if (MouseControl == MouseMode.LineSourceSel)
                            {
                                EditLS.ItemDisplayNr = i;
                                EditLS.RemoveOne(false);
                            }
                            else if (MouseControl == MouseMode.AreaSourceSel)
                            {
                                EditAS.ItemDisplayNr = i;
                                EditAS.RemoveOne(false);
                            }
                        }
                    }
                    InfoBoxCloseAllForms(); // close all infoboxes
                    Picturebox1_Paint(); // Kuntner
                }
            }

        }

        /// <summary>
        /// Change to arrow cursor
        /// </summary>
        private void Button5_Click(object sender, EventArgs e)
        {
            MouseControl = MouseMode.Default;
            Cursor = Cursors.Default;
            //prevent parallel editing
            HideWindows(0);
        }

        /// <summary>
        /// Activate domain form
        /// </summary>
        public void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            //if ((MouseControl == MouseMode.PointSourcePos) || (MouseControl == MouseMode.AreaSourcePos) || (MouseControl == MouseMode.BuildingPos) || (MouseControl == MouseMode.LineSourcePos) || (MouseControl == MouseMode.PortalSourcePos) || (MouseControl ==  MouseMode.ReceptorPos) || (MouseControl == MouseMode.BaseMapGeoReference1) || (MouseControl == MouseMode.BaseMapGeoReference2) || (MouseControl == MouseMode.WallSet) || (MouseControl == MouseMode.AreaPosCorner))

            if ((MouseControl == MouseMode.BaseMapGeoReference1) || (MouseControl == MouseMode.BaseMapGeoReference2))
            {
                Activate();
            }
        }

        /// <summary>
        /// Start a georeferencing with one point and a scale
        /// </summary>
        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            GeoReferencingOne();

            //enable/disable GRAL simulations
            MainForm.Enable_GRAL();
            //enable/disable GRAMM simulations
            MainForm.Enable_GRAMM();
        }

        /// <summary>
        /// Start a georeferencing with two points
        /// </summary>
        private void ToolStripButton4_Click(object sender, EventArgs e)
        {
            GeoReferencingTwo();

            //enable/disable GRAL simulations
            MainForm.Enable_GRAL();
            //enable/disable GRAMM simulations
            MainForm.Enable_GRAMM();
        }

        /// <summary>
        /// Move an already georeferenced bitmap or shape file
        /// </summary>
        void ToolStripButton1Click(object sender, EventArgs e)
        {
            if (toolStripButton1.Checked == true) // move and scale base map
            {
                // search top base map at object manager
                DrawingObjects _drobj = null;
                foreach (DrawingObjects _dr in ItemOptions)
                {
                    if ((_dr.Name.Substring(0, 3) == "SM:"))
                    {
                        _drobj = _dr;
                        break;
                    }
                }

                if (_drobj != null) // Shape Map -> coordinate transformation
                {
                    double west = _drobj.Item;
                    double north = _drobj.SourceGroup;
                    if (west == -1 && north == -1)
                    {
                        west = 0; north = 0;
                    }
                    string x0 = west.ToString();
                    string y0 = north.ToString();

                    try
                    {
                        using (InputCoordinates inp = new InputCoordinates(x0, y0))
                        {
                            inp.Text = "SHP-File: coordinate offset";
                            inp.Show_at_Mouse_Position = false;
                            inp.TopMost = true;
                            inp.ShowDialog();
                            _drobj.Item = Convert.ToInt32(inp.Input_x.Text);
                            _drobj.SourceGroup = Convert.ToInt32(inp.Input_y.Text);
                        }

                        SaveDomainSettings(1);
                    }
                    catch { }
                    toolStripButton1.PerformClick();
                    Picturebox1_Paint();
                }
                else // Bitmap -> Shift
                {

                    BaseMapOldValues.Destrec = new Rectangle(0, 0, 0, 0);

                    MouseControl = MouseMode.BaseMapMoveScale;
                    CursorConverter cv = new CursorConverter();
                    Cursor cursor = (Cursor)cv.ConvertFrom(Gral.Properties.Resources.lmove);
                    Cursor = cursor;
                }
            }
            else // save new settings
            {
                if (MouseControl == MouseMode.BaseMapMoveScale) // otherwise cancel the procedure
                {
                    SaveDomainSettings(1);
                }
                Cursor = DefaultCursor;
                MouseControl = MouseMode.Default;
            }
        }

        /// <summary>
        /// Define a new GRAL domain area
        /// </summary>
        private void Button6_Click_1(object sender, EventArgs e)
        {
            //define model domain
            MouseControl = MouseMode.GralDomainStartPoint;
            Cursor = Cursors.Cross;
            //prevent parallel editing
            HideWindows(0);
        }

        /// <summary>
        /// Define a new GRAMM domain area
        /// </summary>
        private void Button29_Click(object sender, EventArgs e)
        {
            //define model domain
            MouseControl = MouseMode.GrammDomainStartPoint;
            Cursor = Cursors.Cross;
            //prevent parallel editing
            HideWindows(0);
        }

        //////////////////////////////////////////////////////////////////
        //
        //       Draw map
        //
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Redraw the picturebox
        /// </summary>
        private void Picturebox1_Paint()
        {
            //Avoid multiple redraw events from different threads
            if (System.Threading.Interlocked.CompareExchange(ref LockOnlineRedraw, 1, 0) == 0)
            {
                DrawMap();

                if (picturebox1.Image != null)
                {
                    picturebox1.Image.Dispose();
                }

                if (PictureBoxBitmap != null)
                {
                    picturebox1.Image = PictureBoxBitmap.Clone(new Rectangle(0, 0, PictureBoxBitmap.Width, PictureBoxBitmap.Height), PictureBoxBitmap.PixelFormat);
                }

                //Release drawing lock
                System.Threading.Interlocked.Exchange(ref LockOnlineRedraw, 0);
            }
        }

        /// <summary>
        /// Catch the event to redraw the picturebox
        /// </summary>
        private void Picturebox_Redraw(object sender, EventArgs e)
        {
            Picturebox1_Paint();
        }

        /// <summary>
        /// Close the (all) item form(s) 
        /// </summary>
        private void ItemFormHide(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                checkBox4.Checked = false;
            }
            if (checkBox5.Checked)
            {
                checkBox5.Checked = false;
            }
            if (checkBox8.Checked)
            {
                checkBox8.Checked = false;
            }
            if (checkBox12.Checked)
            {
                checkBox12.Checked = false;
            }
            if (checkBox15.Checked)
            {
                checkBox15.Checked = false;
            }
            if (checkBox20.Checked)
            {
                checkBox20.Checked = false;
            }
            if (checkBox25.Checked)
            {
                checkBox25.Checked = false;
            }
            if (checkBox26.Checked)
            {
                checkBox26.Checked = false;
            }
        }

        /// <summary>
        /// Hide all other windows
        /// </summary>
        private void HideWindows(int checkboxnr)
        {
            GeoReferenceOne.Hide();
            GeoReferenceTwo.Hide();
            if (EditPS.Visible)
            {
                EditPS.Hide();
                EditPS.ReloadItemData();
            }
            if (EditAS.Visible)
            {
                EditAS.Hide();
                EditAS.ReloadItemData();
            }
            EditPortals.Hide();
            if (EditB.Visible)
            {
                EditB.Hide();
                EditB.ReloadItemData();
            }
            if (EditLS.Visible)
            {
                EditLS.Hide();
                EditLS.ReloadItemData();
            }
            if (EditR.Visible)
            {
                EditR.Hide();
                EditR.ResetItemData();
            }
            if (EditWall.Visible)
            {
                EditWall.Hide();
                EditWall.ReloadItemData();
            }
            if (EditVegetation.Visible)
            {
                EditVegetation.Hide();
                EditVegetation.ReloadItemData();
            }
            //this.Width = ScreenWidth; Auskommentiert Kuntner
            //this.Height = ScreenHeight - 50; Auskommentiert Kuntner
            //prevent parallel editing
            // Kuntner which checkbox should be unchecked?
            if (checkboxnr != 4)
            {
                checkBox4.Checked = false;
            }

            if (checkboxnr != 5)
            {
                checkBox5.Checked = false;
            }

            if (checkboxnr != 8)
            {
                checkBox8.Checked = false;
            }

            if (checkboxnr != 12)
            {
                checkBox12.Checked = false;
            }

            if (checkboxnr != 15)
            {
                checkBox15.Checked = false;
            }

            if (checkboxnr != 20)
            {
                checkBox20.Checked = false;
            }

            if (checkboxnr != 25)
            {
                checkBox25.Checked = false;
            }

            if (checkboxnr != 26)
            {
                checkBox26.Checked = false;
            }
        }

        /// <summary>
        /// Force to write the file in.dat
        /// </summary>
        private void WriteInDat()
        {
            //save data to "in.dat"
            MainForm.GRALSettings.InDatPath = Path.Combine(Gral.Main.ProjectName, @"Computation", "in.dat");
            MainForm.GRALSettings.Pollutant = "NOx";

            InDatFileIO write_in_dat = new InDatFileIO
            {
                Data = MainForm.GRALSettings
            };
            write_in_dat.WriteInDat();

            write_in_dat = null;
        }

        /// <summary>
        /// Load an additional base map
        /// </summary>
        private void Button26_Click(object sender, EventArgs e)
        {
            double xmin = 10000000;
            double ymin = 10000000;
            double xmax = -10000000;
            double ymax = -10000000;
            double fact1 = 1;
            double fact2 = 1;

            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "(*.bmpw;*.gifw;*.jpgw;*.pngw;*.tfw;*.shp;*.bmp;*.gif;*.jpg;*.png;*.tif;*.tiff;*.jgw;*.pgw;*.gfw;*.bpw)|*.bmpw;*.gifw;*.jpgw;*.pngw;*.tfw;*.shp;*.bmp;*.gif;*.jpg;*.png;*.tif;*.tiff;*.jgw;*.pgw;*.gfw;*.bpw",
                Title = "Select georeferenced image map",
                InitialDirectory = Path.Combine(Gral.Main.ProjectName, "Maps" + Path.DirectorySeparatorChar)
#if NET6_0_OR_GREATER
                ,
                ClientGuid = GralStaticFunctions.St_F.FileDialogMaps
#endif
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                if (dialog.FileName.EndsWith("w"))
                {
                    try
                    {
                        double PixelMy;
                        MapFileName = dialog.FileName;
                        ReadWorldFileHeader header = new ReadWorldFileHeader
                        {
                            Mapfile = MapFileName
                        };

                        DrawingObjects _drobj = new DrawingObjects("BM: " + Path.GetFileNameWithoutExtension(MapFileName));

                        if (header.ReadHeader() == false)
                        {
                            throw new FileLoadException();
                        }
                        else
                        {
                            _drobj.PixelMx = header.PixelMx;
                            PixelMy = header.PixelMy;
                            _drobj.West = header.West;
                            _drobj.North = header.North;
                            ImageFileNameFromWorldFile = header.Imagefile;
                        }
                        header = null;

                        //File name available but does not exist on the folder -> try to load from local folder
                        if (!string.IsNullOrEmpty(ImageFileNameFromWorldFile) && !File.Exists(ImageFileNameFromWorldFile))
                        {
                            string _fn = Path.GetFileName(ImageFileNameFromWorldFile);
                            string testfile = Path.Combine(Path.GetDirectoryName(MapFileName), _fn);
                            if (File.Exists(testfile))
                            {
                                ImageFileNameFromWorldFile = testfile;
                            }
                        }

                        // ImageFile from world file is not available or does not exist -> try with changed filename from world file in local folder
                        if ((string.IsNullOrEmpty(ImageFileNameFromWorldFile)) || (File.Exists(ImageFileNameFromWorldFile) == false))
                        {
                            //check tiff extension
                            if (Path.GetExtension(MapFileName).ToLower() == ".tfw")
                            {
                                ImageFileNameFromWorldFile = Path.Combine(Path.GetDirectoryName(MapFileName), Path.GetFileNameWithoutExtension(MapFileName) + ".tif");
                                if (File.Exists(ImageFileNameFromWorldFile) == false)
                                {
                                    ImageFileNameFromWorldFile = Path.Combine(Path.GetDirectoryName(MapFileName), Path.GetFileNameWithoutExtension(ImageFileNameFromWorldFile) + ".tiff"); // try tiff
                                }
                            }

                            string testfile;
                            if (Path.GetExtension(MapFileName).ToLower() == ".jgw")
                            {
                                testfile = Path.GetFileNameWithoutExtension(MapFileName) + ".jpg";
                            }
                            else if (Path.GetExtension(MapFileName).ToLower() == ".pgw")
                            {
                                testfile = Path.GetFileNameWithoutExtension(MapFileName) + ".png";
                            }
                            else if (Path.GetExtension(MapFileName).ToLower() == ".jpgw" == true)
                            {
                                testfile = Path.GetFileNameWithoutExtension(MapFileName) + ".jpg";
                            }
                            else if (Path.GetExtension(MapFileName).ToLower() == ".gfw")
                            {
                                testfile = Path.GetFileNameWithoutExtension(MapFileName) + ".gif";
                            }
                            else if (Path.GetExtension(MapFileName).ToLower() == ".bpw")
                            {
                                testfile = Path.GetFileNameWithoutExtension(MapFileName) + ".bmp";
                            }
                            else
                            {
                                testfile = MapFileName.Remove(MapFileName.Length - 1, 1);
                            }

                            testfile = Path.Combine(Path.GetDirectoryName(MapFileName), testfile);
                            // check if file exists
                            if (File.Exists(testfile))
                            {
                                ImageFileNameFromWorldFile = testfile;
                            }
                        }

                        // read the picture
                        MapFileName = ImageFileNameFromWorldFile;

                        using (FileStream fs = new FileStream(MapFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            _drobj.Picture.Dispose();
                            _drobj.Picture = new Bitmap(fs);
                        }

                        //add base map to object list
                        _drobj.Label = 0;
                        _drobj.ContourFilename = MapFileName;
                        _drobj.DestRec = new Rectangle(TransformX + Convert.ToInt32((_drobj.West - MapSize.West) / BmpScale / MapSize.SizeX), TransformY - Convert.ToInt32((_drobj.North - MapSize.North) / BmpScale / MapSize.SizeX), Convert.ToInt32(_drobj.Picture.Width * _drobj.PixelMx / MapSize.SizeX * XFac), Convert.ToInt32(_drobj.Picture.Height * _drobj.PixelMx / MapSize.SizeX * XFac));
                        _drobj.SourceRec = new Rectangle(0, 0, _drobj.Picture.Width, _drobj.Picture.Height);

                        ItemOptions.Insert(0, _drobj);
                        SaveDomainSettings(1);
                        SwitchMenuGeoreference(); // Kuntner
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Error when reading world file" + Environment.NewLine + ex.Message.ToString(), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (dialog.FileName.EndsWith(".shp"))
                {
                    try
                    {
                        //add new list of shape features to the new shape map
                        DrawingObjects _drobj = new DrawingObjects("SM: " + Path.GetFileNameWithoutExtension(dialog.FileName));

                        GralShape.ShapeReader shape = new GralShape.ShapeReader(this)
                        {
                            ReadBuildings = false // don't read buildings, read simple background image
                        };

                        int count = 0;
                        foreach (object shp in shape.ReadShapeFile(dialog.FileName))
                        {
                            if (shp is GralShape.SHPLine)
                            {
                                _drobj.ShpLines.Add((GralShape.SHPLine)shp);
                            }
                            else if (shp is GralShape.SHPPolygon)
                            {
                                _drobj.ShpPolygons.Add((GralShape.SHPPolygon)shp);
                            }
                            else if (shp is PointF _ptF)
                            {
                                _drobj.ShpPoints.Add(_ptF);
                            }
                            else if (shp is GralData.PointD_3d _pt3D)
                            {
                                _drobj.ShpPoints.Add(_pt3D.ToPointF());
                            }
                            else if (shp is PointD _ptD)
                            {
                                _drobj.ShpPoints.Add(_ptD.ToPointF());
                            }

                            if (count == 0)
                            {
                                _drobj.West = shape.West;
                                _drobj.North = shape.North;
                                _drobj.PixelMx = shape.PixelMx;
                                _drobj.Picture = new Bitmap(100, Math.Abs(shape.PixelMy));
                            }
                            count++;
                        }
                        _drobj.ContourFilename = dialog.FileName;
                        //shape.readShapeFile(dialog.FileName,0);
                        shape = null;

                        _drobj.Label = 0;

                        ItemOptions.Insert(0, _drobj);
                        SaveDomainSettings(1);
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when reading .shp file");
                    }
                }
                else
                {
                    MapFileName = dialog.FileName;

                    DrawingObjects _drobj = new DrawingObjects("BM: " + Path.GetFileNameWithoutExtension(MapFileName));

                    using (FileStream fs = new FileStream(MapFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        _drobj.Picture.Dispose();
                        _drobj.Picture = new Bitmap(fs);
                    }

                    //compute artificial coordinates, such that image appears with full extent at screen


                    _drobj.West = Convert.ToInt32((-TransformX) * MapSize.SizeX * BmpScale + MapSize.West);
                    _drobj.North = Convert.ToInt32((TransformY) * MapSize.SizeX * BmpScale + MapSize.North);
                    double dummy1 = Convert.ToDouble(picturebox1.Width) / Convert.ToDouble(_drobj.Picture.Width) * MapSize.SizeX / XFac;
                    double dummy2 = Convert.ToDouble(picturebox1.Height) / Convert.ToDouble(_drobj.Picture.Height) * MapSize.SizeX / XFac;
                    _drobj.PixelMx = Math.Min(dummy1, dummy2);
                    _drobj.Label = 0;
                    _drobj.ContourFilename = MapFileName;

                    ItemOptions.Insert(0, _drobj);
                    SaveDomainSettings(1);
                }
            }

            //compute deltax and deltay between all base maps
            xmin = 10000000;
            ymin = 10000000;
            xmax = -10000000;
            ymax = -10000000;
            fact1 = 1;
            fact2 = 1;

            foreach (DrawingObjects _dr in ItemOptions)
            {
                if ((_dr.Name.Substring(0, 3) == "BM:") || (_dr.Name.Substring(0, 3) == "SM:"))
                {
                    xmin = Math.Min(xmin, _dr.West);
                    xmax = Math.Max(xmax, _dr.West + _dr.PixelMx * _dr.Picture.Width);
                    ymax = Math.Max(ymax, _dr.North);
                    ymin = Math.Min(ymin, _dr.North - _dr.PixelMx * _dr.Picture.Height);
                }
            }

            fact1 = Convert.ToDouble(picturebox1.Width) / (xmax - xmin) * MapSize.SizeX;
            fact2 = Convert.ToDouble(picturebox1.Height) / (ymax - ymin) * MapSize.SizeX;
            XFac = Math.Min(fact1, fact2);
            BmpScale = 1 / XFac;

            TransformX = Convert.ToInt32(-(xmin - MapSize.West) / BmpScale / MapSize.SizeX);
            TransformY = Convert.ToInt32(-(ymax - MapSize.North) / BmpScale / MapSize.SizeY);

            //set source - and destination rectangle
            foreach (DrawingObjects _dr in ItemOptions)
            {
                try
                {
                    _dr.DestRec = new Rectangle(TransformX + Convert.ToInt32((_dr.West - MapSize.West) / BmpScale / MapSize.SizeX),
                                                TransformY - Convert.ToInt32((_dr.North - MapSize.North) / BmpScale / MapSize.SizeX),
                                                Convert.ToInt32(_dr.Picture.Width * _dr.PixelMx / MapSize.SizeX * XFac),
                                                Convert.ToInt32(_dr.Picture.Height * _dr.PixelMx / MapSize.SizeX * XFac));
                    _dr.SourceRec = new Rectangle(0, 0, _dr.Picture.Width, _dr.Picture.Height);
                }
                catch { }
            }
            Picturebox1_Paint();
        }

        private void SelectPointsStartComputation(object sender, EventArgs e)
        {
            // Release send coors
            if (sender is SelectMultiplePoints _sl)
            {
                SendCoors -= _sl.ReceiveClickedCoordinates;

                _sl.Close();
            }
        }

        private void SelectPointsCancelComputation(object sender, EventArgs e)
        {
            // Release send coors
            if (sender is SelectMultiplePoints _sl)
            {
                SendCoors -= _sl.ReceiveClickedCoordinates;
                _sl.Close();
            }
        }

        //////////////////////////////////////////////////////////////////
        //
        //       select items
        //
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start selecting a point source by mouse
        /// </summary>
        private void Button8_Click(object sender, EventArgs e)
        {
            ResetSelectionChecked();
            pointSourcesToolStripMenuItem1.Checked = true;

            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox8.Checked = false;
            checkBox12.Checked = false;
            checkBox15.Checked = false;
            checkBox20.Checked = false;
            checkBox25.Checked = false;
            checkBox26.Checked = false;

            InfoBoxCloseAllForms(); // close all infoboxes
            MouseControl = MouseMode.PointSourceSel;
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Start selecting an area source by mouse
        /// </summary>
        private void Button10_Click(object sender, EventArgs e)
        {
            ResetSelectionChecked();
            areaSourcesToolStripMenuItem1.Checked = true;

            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox8.Checked = false;
            checkBox12.Checked = false;
            checkBox15.Checked = false;
            checkBox20.Checked = false;
            checkBox25.Checked = false;
            checkBox26.Checked = false;

            InfoBoxCloseAllForms(); // close all infoboxes
            MouseControl = MouseMode.AreaSourceSel;
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Start selecting a building by mouse
        /// </summary>
        private void Button16_Click(object sender, EventArgs e)
        {
            ResetSelectionChecked();
            buildingsToolStripMenuItem1.Checked = true;

            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox8.Checked = false;
            checkBox12.Checked = false;
            checkBox15.Checked = false;
            checkBox20.Checked = false;
            checkBox25.Checked = false;
            checkBox26.Checked = false;

            InfoBoxCloseAllForms(); // close all infoboxes
            MouseControl = MouseMode.BuildingSel;
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Start selecting a receptor point by mouse
        /// </summary>
        private void Button23_Click(object sender, EventArgs e)
        {
            ResetSelectionChecked();
            receptorPointsToolStripMenuItem1.Checked = true;

            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox8.Checked = false;
            checkBox12.Checked = false;
            checkBox15.Checked = false;
            checkBox20.Checked = false;
            checkBox25.Checked = false;
            checkBox26.Checked = false;

            InfoBoxCloseAllForms(); // close all infoboxes
            MouseControl = MouseMode.ReceptorSel;
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Start selecting a line source by mouse
        /// </summary>
        private void Button12_Click(object sender, EventArgs e)
        {
            ResetSelectionChecked();
            lineSourcesToolStripMenuItem1.Checked = true;

            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox8.Checked = false;
            checkBox12.Checked = false;
            checkBox15.Checked = false;
            checkBox20.Checked = false;
            checkBox25.Checked = false;
            checkBox26.Checked = false;

            InfoBoxCloseAllForms(); // close all infoboxes
            MouseControl = MouseMode.LineSourceSel;
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Start selecting a wall by mouse
        /// </summary>
        void Button49Click(object sender, EventArgs e)
        {
            ResetSelectionChecked();
            wallsToolStripMenuItem1.Checked = true;

            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox8.Checked = false;
            checkBox12.Checked = false;
            checkBox15.Checked = false;
            checkBox20.Checked = false;
            checkBox25.Checked = false;
            checkBox26.Checked = false;

            InfoBoxCloseAllForms(); // close all infoboxes
            MouseControl = MouseMode.WallSel;
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Start selecting a vegetation area by mouse
        /// </summary>
        void Button50Click(object sender, EventArgs e)
        {
            ResetSelectionChecked();
            VegetationtToolStripMenuItem1.Checked = true;

            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox8.Checked = false;
            checkBox12.Checked = false;
            checkBox15.Checked = false;
            checkBox20.Checked = false;
            checkBox25.Checked = false;
            checkBox26.Checked = false;

            InfoBoxCloseAllForms(); // close all infoboxes
            MouseControl = MouseMode.VegetationSel;
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Start selecting a tunnel portal source by mouse
        /// </summary>
        private void Button14_Click(object sender, EventArgs e)
        {
            ResetSelectionChecked();
            tunnelPortalsToolStripMenuItem1.Checked = true;

            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox8.Checked = false;
            checkBox12.Checked = false;
            checkBox15.Checked = false;
            checkBox20.Checked = false;
            checkBox25.Checked = false;
            checkBox26.Checked = false;

            InfoBoxCloseAllForms(); // close all infoboxes
            MouseControl = MouseMode.PortalSourceSel;
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Cancel selecting an item by mouse
        /// </summary>
        private void ResetSelectionChecked()
        {
            pointSourcesToolStripMenuItem1.Checked = false;
            areaSourcesToolStripMenuItem1.Checked = false;
            lineSourcesToolStripMenuItem1.Checked = false;
            tunnelPortalsToolStripMenuItem1.Checked = false;
            buildingsToolStripMenuItem1.Checked = false;
            receptorPointsToolStripMenuItem1.Checked = false;
            wallsToolStripMenuItem1.Checked = false;
            VegetationtToolStripMenuItem1.Checked = false;
        }

        //////////////////////////////////////////////////////////////////
        //
        //       import existing GRAL files (sources, buildings)
        //
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Import existing GRAL point source data
        /// </summary>
        private void Button9_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ImportPointSources(sender, e);
            Cursor = Cursors.Default;
            Picturebox1_Paint();
        }

        /// <summary>
        /// Import existing GRAL receptor points
        /// </summary>
        private void Button24_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ImportReceptorPoints(sender, e);
            Picturebox1_Paint();
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Import vegetation data from shape file
        /// </summary>
        void Button51Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ImportVegetationAreas(sender, e);
            Picturebox1_Paint();
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Import existing GRAL area source data
        /// </summary>
        private void Button11_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ImportAreaSources(sender, e);
            Picturebox1_Paint();
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Import existing GRAL building data
        /// </summary>
        private void Button17_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ImportBuildings(sender, e);
            Picturebox1_Paint();
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Import existing GRAL line source data
        /// </summary>
        private void Button13_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ImportGralLineSource(sender, e);
            Picturebox1_Paint();
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Import existing GRAL portal source data
        /// </summary>
        private void Button15_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ImportTunnelPortals(sender, e);
            Picturebox1_Paint();
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Import wall data
        /// </summary>
        void Button54Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ImportWalls(sender, e);
            Picturebox1_Paint();
            Cursor = Cursors.Default;
        }

        //////////////////////////////////////////////////////////////////
        //
        //       Export to ESRI-.shp files
        //
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Write line source data to ESRI-shape file
        /// </summary>
        private void Button36_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ExportShapeLineSource(sender, e);
        }

        /// <summary>
        /// Write area source data to ESRI-shape file
        /// </summary>
        private void Button37_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ExportShapeAreaSource(sender, e);
        }

        /// <summary>
        /// Write point source data to ESRI-shape file
        /// </summary>
        private void Button38_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ExportShapePointSource(sender, e);
        }

        /// <summary>
        /// Write receptor points to ESRI-shape file
        /// </summary>
        private void Button39_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ExportShapeReceptor(sender, e);
        }
        /// <summary>
        /// Write buildings to an ESRI-shape file
        /// </summary>
        private void Button58_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ExportShapeBuildings(sender, e);
        }
        /// <summary>
        /// Write the domain area to an ESRI-shape file
        /// </summary>
        private void domainAreaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ExportShapeDomainArea(sender, e);
        }
        /// <summary>
        /// Write contour lines to ESRI-shape file
        /// </summary>
        private void Button56_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            ExportShapeContourLine(sender, e);
        }

        //////////////////////////////////////////////////////////////////
        //
        //       Addtional tools
        //
        //////////////////////////////////////////////////////////////////

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

                buttonOk.SetBounds(9, 72, 75, 23);
                buttonCancel.SetBounds(109, 72, 75, 23);
                buttonOk.Location = new Point(9, 72);
                buttonOk.Size = new System.Drawing.Size(75, 23);
                buttonCancel.Location = new Point(109, 72);
                buttonCancel.Size = new System.Drawing.Size(75, 23);

                numdown.Anchor |= AnchorStyles.Left;
                buttonOk.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                buttonCancel.Anchor = AnchorStyles.Top | AnchorStyles.Left;

                form.Controls.AddRange(new Control[] { label, numdown, buttonOk, buttonCancel });
                form.ClientSize = new Size(Math.Max(300, label.Width + 20), 110);
                form.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

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
        /// Edit and define the size of the north arrow
        /// </summary>
        private void Button18_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            MouseControl = MouseMode.ViewNorthArrowPos;

            int trans = Convert.ToInt32(NorthArrow.Scale * 100);
            foreach (DrawingObjects _drobj in ItemOptions)
            {
                if (_drobj.Name.Equals("NORTH ARROW"))
                {
                    trans = _drobj.ContourLabelDist;
                    break;
                }
            }

            if (InputBox1("Define the size of the north arrow", "Scaling factor (1000=10 times larger):", 0, 1000, ref trans) == DialogResult.OK)
            {
                NorthArrow.Scale = Convert.ToDecimal(trans) / 100;
            }
            Picturebox1_Paint();
            SaveDomainSettings(1);
            Cursor = Cursors.Cross;
        }

        /// <summary>
        /// Show/hide north arrow
        /// </summary>
        private void CheckBox18_CheckedChanged(object sender, EventArgs e)
        {
            Picturebox1_Paint();
        }

        /// <summary>
        /// Edit the scale bar
        /// </summary>
        private void Button19_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms

            MapScaleInput msi = new MapScaleInput()
            {
                Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 260,
                Top = GralStaticFunctions.St_F.GetTopScreenAtMousePosition() + 180
            };
            msi.MapScale = MapScale;
            if (msi.ShowDialog() == DialogResult.OK)
            {
                MouseControl = MouseMode.ViewScaleBarPos;
                SaveDomainSettings(1);
            }
            Picturebox1_Paint();
            Cursor = Cursors.Cross;
        }

        /// <summary>
        /// Show/hide the scale bar
        /// </summary>
        private void CheckBox19_CheckedChanged(object sender, EventArgs e)
        {
            Picturebox1_Paint();
        }

        /// <summary>
        /// Measure distances
        /// </summary>
        private void Button20_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            MouseControl = MouseMode.ViewDistanceMeasurement;
            Cursor = Cursors.Cross;
        }

        /// <summary>
        /// Measure an area
        /// </summary>
        private void Button21_Click(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            MouseControl = MouseMode.ViewAreaMeasurement;
            Cursor = Cursors.Cross;
        }

        void Button_section_windMouseClick(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            MouseControl = MouseMode.SectionWindSel;
            Cursor = Cursors.Cross;
        }

        void Button_section_concentrationClick(object sender, EventArgs e)
        {
            HideWindows(0); // Kuntner - close all edit forms
            MouseControl = MouseMode.SectionConcSel;
            Cursor = Cursors.Cross;
        }

        /// <summary>
        /// Save the current map to a bitmap file
        /// </summary>
        private void Button22_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = saveFileDialog1;
            dialog.Filter = "*.gif|*.gif|*.jpeg|*.jpeg|*.png|*.png|*.bmp|*.bmp|*.emf|*.emf|*.tiff|*.tiff|*.wmf|*.wmf";
            dialog.Title = "Save map";
            dialog.InitialDirectory = Path.Combine(Gral.Main.ProjectName, "Maps" + Path.DirectorySeparatorChar);
            // dialog.ShowHelp = true;
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogMaps;
#endif

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                Graphics g = CreateGraphics();
                float dx = 96;
                try
                {
                    dx = g.DpiX;
                }
                finally
                {
                    g.Dispose();
                }

                string dotsperinch = Convert.ToString(dx);
                if (St_F.InputBoxValue("Adjust bitmap resolution", "Dots per inch [dpi]", ref dotsperinch, this) == DialogResult.OK)
                {
                    double dpifactor = 1;
                    try
                    {
                        dpifactor = Convert.ToDouble(dotsperinch) / Convert.ToDouble(dx);
                    }
                    catch
                    {
                        MessageBox.Show(this, "Invalid resolution. Image saved with " + dotsperinch + " dpi resolution", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    //set resolution
                    XFac *= dpifactor;
                    BmpScale = 1 / XFac;
                    BmppbXSave = 1 / dpifactor;
                    TransformX = Convert.ToInt32(TransformX * dpifactor);
                    TransformY = Convert.ToInt32(TransformY * dpifactor);
                    foreach (DrawingObjects _dr in ItemOptions)
                    {
                        try
                        {
                            _dr.DestRec = new Rectangle(TransformX + Convert.ToInt32((_dr.West - MapSize.West) / BmpScale / MapSize.SizeX),
                                                        TransformY - Convert.ToInt32((_dr.North - MapSize.North) / BmpScale / MapSize.SizeX),
                                                        Convert.ToInt32(_dr.Picture.Width * _dr.PixelMx / MapSize.SizeX * XFac),
                                                        Convert.ToInt32(_dr.Picture.Height * _dr.PixelMx / MapSize.SizeX * XFac));
                            _dr.SourceRec = new Rectangle(0, 0, _dr.Picture.Width, _dr.Picture.Height);
                        }
                        catch { }
                    }

                    int height = picturebox1.Height; // get actual height and width with dock-style fill
                    int width = picturebox1.Width;
                    picturebox1.Dock = DockStyle.None;
                    picturebox1.Anchor = AnchorStyles.None;
                    Application.DoEvents();

                    picturebox1.Height = Convert.ToInt32(height * dpifactor);
                    picturebox1.Width = Convert.ToInt32(width * dpifactor);

                    Picturebox1_Paint();

                    Bitmap bitMap = new Bitmap(picturebox1.Width, picturebox1.Height);
                    //Graphics gra = Graphics.FromImage(bitMap);
                    picturebox1.DrawToBitmap(bitMap, new Rectangle(0, 0, picturebox1.Width, picturebox1.Height));

                    picturebox1.Height = Convert.ToInt32(picturebox1.Height / dpifactor);
                    picturebox1.Width = Convert.ToInt32(picturebox1.Width / dpifactor);

                    //set resolution and picturebox dock style back
                    picturebox1.Dock = DockStyle.Fill;
                    picturebox1.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
                    XFac /= dpifactor;
                    BmpScale = 1 / XFac;
                    BmppbXSave = 1;
                    TransformX = Convert.ToInt32(TransformX / dpifactor);
                    TransformY = Convert.ToInt32(TransformY / dpifactor);
                    foreach (DrawingObjects _dr in ItemOptions)
                    {
                        try
                        {
                            _dr.DestRec = new Rectangle(TransformX + Convert.ToInt32((_dr.West - MapSize.West) / BmpScale / MapSize.SizeX),
                                                        TransformY - Convert.ToInt32((_dr.North - MapSize.North) / BmpScale / MapSize.SizeX),
                                                        Convert.ToInt32(_dr.Picture.Width * _dr.PixelMx / MapSize.SizeX * XFac),
                                                        Convert.ToInt32(_dr.Picture.Height * _dr.PixelMx / MapSize.SizeX * XFac));
                            _dr.SourceRec = new Rectangle(0, 0, _dr.Picture.Width, _dr.Picture.Height);
                        }
                        catch { }
                    }

                    string extension = Path.GetExtension(dialog.FileName).ToLower();
                    if (extension == ".gif")
                    {
                        bitMap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                    }

                    if (extension == ".jpeg")
                    {
                        bitMap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }

                    if (extension == ".png")
                    {
                        bitMap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    }

                    if (extension == ".bmp")
                    {
                        bitMap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                    }

                    if (extension == ".emf")
                    {
                        bitMap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Emf);
                    }

                    if (extension == ".tiff")
                    {
                        bitMap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Tiff);
                    }

                    if (extension == ".wmf")
                    {
                        bitMap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Wmf);
                    }

                    Picturebox1_Paint();
                }
            }
        }

        /// <summary>
        /// Copy picture to clipboard
        /// </summary>
        void Domain_clipboardClick(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(PictureBoxBitmap);
        }

        /// <summary>
        /// Show the object (layer) manager
        /// </summary>
        private void Button25_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
            MouseControl = MouseMode.Default;
            if (ObjectManagerForm == null) // Kuntner: if no objectmanager was created, create an new one
            {
                ObjectManagerForm = new Objectmanager(this);
                ObjectManagerForm.Object_redraw += DomainRedrawDelegate; // Redraw from Edit Point Sources
                ObjectmanagerUpdateListbox();
                ObjectManagerForm.Show();
            }
            else
            {
                ObjectmanagerUpdateListbox();
                ObjectManagerForm.Show();
            }
        }

        /// <summary>
        /// Update the object (layer) managers listbox
        /// </summary>
        private void ObjectmanagerUpdateListbox()
        {
            // send Message to object manager, that an update is needed
            try
            {
                if (ObjectManagerForm != null)
                {
                    ObjectManagerForm.UpdateListbox(this, null);
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Create a contour map
        /// </summary>
        private void Button27_Click(object sender, EventArgs e)
        {
            CreateContourMap("");
            Picturebox1_Paint();
        }

        /// <summary>
        /// mathematical operations for multiple raster sets
        /// </summary>
        private void Button28_Click(object sender, EventArgs e)
        {
            Mathrasteroperation mathrasteroperation = new Mathrasteroperation(MainForm, this);
            mathrasteroperation.Show();
        }

        /// /////////////////////////////////////////////////////////////////////////////
        //
        //  GRAMM windfield analysis tools
        //
        /////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Compute meteorological time series at a specific point
        /// </summary>
        private void Button30_Click(object sender, EventArgs e)
        {
            MouseControl = MouseMode.SetPointMetTimeSeries;
            Cursor = Cursors.Cross;

            GralDomForms.SelectMultiplePoints _selmp = new SelectMultiplePoints();
            SendCoors += _selmp.ReceiveClickedCoordinates;
            _selmp.CancelComputation += CancelMetTimeSeries;
            _selmp.StartComputation += MetTimeSeries;
            if (Gral.Main.GRAMMwindfield != null && File.Exists(Path.Combine(Gral.Main.GRAMMwindfield, "00001.scl"))) // at least one stability file exists
            {
                _selmp.LocalStability = true;
            }
            _selmp.MeteoModel = 0;
            if (Gral.Main.GRAMMwindfield != null)
            {
                _selmp.MeteoModel = MeteoModelEmum.GRAMM;
            }
            if (WindfieldsAvailable())
            {
                _selmp.MeteoModel = _selmp.MeteoModel | MeteoModelEmum.GRAL;
            }

            _selmp.StartPosition = FormStartPosition.Manual;
            _selmp.Location = new Point(GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160, St_F.GetTopScreenAtMousePosition() + 150);
            _selmp.Owner = this;
            _selmp.Show();

            //// open & show meteo station
            //if (MeteoDialog != null) // close possible open Dialog
            //{
            //    MeteoDialog.Close();
            //    MeteoDialog.Dispose();
            //}
            //MeteoDialog = new DialogCreateMeteoStation
            //{
            //    Meteo_Title = "GRAL GUI Compute wind statistics",
            //    Meteo_Init = "Meteo",
            //    Meteo_Ext = ".met",
            //    Meteo_Height = 10,
            //    Meteo_Model = 0, // Receptors & Coors visible
            //    X1 = Left + 70,
            //    Y1 = Top + 50,
            //    Xs = XDomain,
            //    Ys = YDomain
            //};
            //if (Gral.Main.GRAMMwindfield != null && File.Exists(Path.Combine(Gral.Main.GRAMMwindfield, "00001.scl"))) // at least one stability file exists
            //{
            //    MeteoDialog.Local_Stability = true;
            //}

            ////check whether receptor points are set in the project
            //if (EditR.ItemData.Count > 0)
            //{
            //    MeteoDialog.Receptor_Points = true;
            //}

            //MeteoDialog.Start_computation += new StartCreateMeteoStation(MetTimeSeries);
            ////  met_st.Start_computation += new Dialog_CreateMeteoStation.start_create_meteo_station(mettimeseries); // delegate from Dialog -> OK
            //MeteoDialog.Cancel_computation += new CancelCreateMeteoStation(CancelMetTimeSeries);
            //MeteoDialog.Show();
        }

        /// <summary>
        /// Computes mean wind velocity at a given height for the whole GRAMM model domain
        /// </summary>
        private void Button31_Click(object sender, EventArgs e)
        {
            ComputeMeanWindVelocity(sender, e);
            Picturebox1_Paint();
        }

        /// <summary>
        /// Computes vector map
        /// </summary>
        private void Button32_Click(object sender, EventArgs e)
        {
            ComputeVectorMap(sender, e);
            Picturebox1_Paint();
        }

        /// <summary>
        /// Eporting sub-domains of GRAMM windfields
        /// </summary>
        private void Button48_Click(object sender, EventArgs e)
        {
            //define model domain
            MouseControl = MouseMode.GrammExportStart;
            Cursor = Cursors.Cross;
            //prevent parallel editing
            HideWindows(0);
        }

        /// <summary>
        /// Analyze Stability class/MO Lenght/Ustern
        /// </summary>
        void ButtonstabilityclassClick(object sender, EventArgs e)
        {
            // set mousecontrol for single point checks
            MouseControl = MouseMode.SetPointGRAMMGrid;
            //select dispersion situation
            using (SelectDispersionSituation disp = new SelectDispersionSituation(this, MainForm))
            {
                disp.StartPosition = FormStartPosition.Manual;
                disp.Location = GetScreenPositionForNewDialog(2);

                string grammpath = Path.Combine(Gral.Main.ProjectName, @"Computation");
                if (Gral.Main.GRAMMwindfield != null) // try GRAMMPATH
                {
                    grammpath = Gral.Main.GRAMMwindfield;
                }
                disp.SCLPath = grammpath;

                if (disp.ShowDialog() == DialogResult.OK) // Situation selected!
                {
                    int sel = disp.selected_situation;
                    ReadSclUstOblClasses reader = new ReadSclUstOblClasses();

                    string filename = Path.Combine(Gral.Main.ProjectName, @"Computation", Convert.ToString(sel).PadLeft(5, '0') + ".scl");
                    if (File.Exists(filename) == false && Gral.Main.GRAMMwindfield != null) // try GRAMMPATH
                    {
                        filename = Path.Combine(Gral.Main.GRAMMwindfield, Convert.ToString(sel).PadLeft(5, '0') + ".scl");
                    }

                    reader.FileName = filename;

                    double[,] arr = new double[1, 1];

                    reader.Stabclasses = arr;

                    if (reader.ReadSclFile()) // true => reader = OK
                    {
                        arr = reader.Stabclasses;
                        //add contour map to object list
                        DrawingObjects _drobj = new DrawingObjects("CM: " + Path.GetFileNameWithoutExtension(reader.FileName));

                        _drobj.ContourFilename = reader.FileName;
                        //compute values for 7 contours
                        for (int i = 0; i < 7; i++)
                        {
                            _drobj.ItemValues.Add(1);
                            _drobj.FillColors.Add(Color.Red);
                            _drobj.LineColors.Add(Color.Red);
                        }
                        _drobj.FillColors[0] = Color.Yellow;
                        _drobj.LineColors[0] = Color.Yellow;

                        // initial scale of contour map
                        for (int i = 0; i < 7; i++)
                        {
                            _drobj.ItemValues[i] = Math.Round(i + 1 - 0.1, 1);
                            Color c = Color.Snow;
                            if (i == 0)
                            {
                                c = Color.FromArgb(164, 0, 0);
                            }
                            else if (i == 1)
                            {
                                c = Color.FromArgb(164, 116, 0);
                            }
                            else if (i == 2)
                            {
                                c = Color.FromArgb(255, 255, 0);
                            }
                            else if (i == 3)
                            {
                                c = Color.FromArgb(0, 255, 0);
                            }
                            else if (i == 4)
                            {
                                c = Color.FromArgb(0, 112, 0);
                            }
                            else if (i == 5)
                            {
                                c = Color.FromArgb(0, 128, 255);
                            }
                            else if (i == 6)
                            {
                                c = Color.FromArgb(255, 0, 255);
                            }

                            _drobj.FillColors[i] = c;
                            _drobj.LineColors[i] = c;
                        }

                        _drobj.ColorScale = Convert.ToString(picturebox1.Width - 150) + "," + Convert.ToString(picturebox1.Height - 200) + "," + "1";
                        _drobj.LegendTitle = "Stability class";
                        _drobj.LegendUnit = "SC";
                        _drobj.LineWidth = 0; // no Lines
                        _drobj.DecimalPlaces = 0;
                        ItemOptions.Insert(0, _drobj);
                        SaveDomainSettings(1);

                        //compute contour polygons
                        ReDrawContours = true;
                        Contours(reader.FileName, _drobj);
                    }
                    else
                    {
                        MessageBox.Show(this, "Unable to open, read or process the data", "Process raster data", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                    reader.close();
                }
            }
            Picturebox1_Paint();
        }

        /// <summary>
        /// Re-order already computed wind fields in order to match them with
        /// new observations at any site in the model domain
        /// </summary>
        private void Button43_Click(object sender, EventArgs e)
        {
            MouseControl = MouseMode.SetPointMatch;
            Cursor = Cursors.Cross;
            if (Gral.Main.GRAMMwindfield != null && File.Exists(Path.Combine(Gral.Main.GRAMMwindfield, "00001.scl"))) // at least one stability file exists
            {
                MMO.LocalStabilityUsed = true;
            }
            else
            {
                MMO.LocalStabilityUsed = false;
            }

            MMO.SettingsPath = Path.Combine(Gral.Main.ProjectName, "Settings" + Path.DirectorySeparatorChar);
            MMO.GRAMMPath = Gral.Main.GRAMMwindfield;
            MMO.Match_Mode = 0;    // start matching process
            MMO.StartPosition = FormStartPosition.Manual;
            MMO.Left = Math.Max(150, St_F.GetScreenAtMousePosition() + 150);
            MMO.Top = St_F.GetTopScreenAtMousePosition() + 150;
            MMO.Show();
        }

        /// <summary>
        /// Re-order wind fields to meet observed wind fields better
        /// </summary>
        private void Button42_Click(object sender, EventArgs e)
        {
            MouseControl = MouseMode.SetPointReOrder;
            Cursor = Cursors.Cross;
        }

        /// <summary>
        /// Start the Re-order wind fields function
        /// </summary>
        private void ReorderGrammWindfields(PointD TestPt)
        {
            //select height above ground for the windfield analysis
            int trans = Convert.ToInt32(10);
            if (InputBox1("Height above ground", "Height above ground [m]:", 0, 10000, ref trans) == DialogResult.OK)
            {

                WriteGrammLog(2, Convert.ToString(TestPt.X), Convert.ToString(TestPt.Y), Convert.ToString(trans));

                GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData
                {
                    VericalIndex = Convert.ToDouble(trans),
                    MeteoFileName = GRAMMmeteofile,
                    ProjectName = Gral.Main.ProjectName,
                    Path_GRAMMwindfield = Path.GetDirectoryName(Gral.Main.GRAMMwindfield),
                    XDomain = Convert.ToInt32(TestPt.X),
                    YDomain = Convert.ToInt32(TestPt.Y),
                    GrammWest = MainForm.GrammDomRect.West,
                    GrammSouth = MainForm.GrammDomRect.South,
                    GRAMMhorgridsize = MainForm.GRAMMHorGridSize,
                    DecSep = decsep,
                    UserText = @"The process may take some minutes. Re-ordered wind field data is stored in the subdirectory \Re-ordered\",
                    Caption = "Re-Order Wind Fields ",
                    BackgroundWorkerFunction = GralBackgroundworkers.BWMode.ReOrder // ; 1 = re-order the GRAMM_Windfield
                };

                GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart =
                    new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                    {
                        Text = DataCollection.Caption
                    };
                BackgroundStart.Show();

                // now the backgroundworker works
            }
        }

        /// <summary>
        /// Extract vertical profiles of wind fields
        /// </summary>
        private void Button41_Click(object sender, EventArgs e)
        {
            try
            {
                //select dispersion situation
                using (SelectDispersionSituation disp = new SelectDispersionSituation(this, MainForm))
                {
                    bool windfieldfiles = WindfieldsAvailable();

                    if (Gral.Main.ProjectName != null && File.Exists(Path.Combine(Gral.Main.ProjectName, @"Computation", "windfeld.txt")))
                    {
                        if (windfieldfiles == false)
                        {
                            disp.selectGRAMM_GRAL = 0; // default: no selection
                            string grammpath = Path.Combine(Gral.Main.ProjectName, @"Computation");
                            if (Gral.Main.GRAMMwindfield != null) // try GRAMMPATH
                            {
                                grammpath = Gral.Main.GRAMMwindfield;
                            }
                            disp.GrammPath = grammpath;
                            disp.GFFPath = St_F.GetGffFilePath(Path.Combine(Gral.Main.ProjectName, "Computation"));
                        }
                        else
                        {
                            disp.selectGRAMM_GRAL = 1; // default: select GRAMM
                            string grammpath = Path.Combine(Gral.Main.ProjectName, @"Computation");
                            if (Gral.Main.GRAMMwindfield != null) // try GRAMMPATH
                            {
                                grammpath = Gral.Main.GRAMMwindfield;
                            }
                            disp.GrammPath = grammpath;
                            disp.GFFPath = St_F.GetGffFilePath(Path.Combine(Gral.Main.ProjectName, "Computation"));
                        }
                    }
                    else if (Gral.Main.ProjectName != null)
                    {
                        disp.selectGRAMM_GRAL = 2; // select GRAL
                        disp.GFFPath = St_F.GetGffFilePath(Path.Combine(Gral.Main.ProjectName, "Computation"));
                    }

                    disp.StartPosition = FormStartPosition.Manual;
                    disp.Location = GetScreenPositionForNewDialog(2);
                    if (disp.ShowDialog() == DialogResult.OK)
                    {
                        ProfileConcentration.DispersionSituation = disp.selected_situation;
                        ProfileConcentration.GRALorGRAMM = disp.selectGRAMM_GRAL;
                        Cursor = Cursors.Default;
                        Cursor = Cursors.Cross;
                        MouseControl = MouseMode.SetPointVertWindProfile;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Start the source apportionment function
        /// </summary>
        private void Button33_Click(object sender, EventArgs e)
        {
            MouseControl = MouseMode.SetPointSourceApport;
            Cursor = Cursors.Cross;
        }

        /// <summary>
        /// Computes source apportionment for GRAL results
        /// </summary>
        private void SourceApportionment(PointD TestPt)
        {
            if (TestPt.X < MainForm.GralDomRect.West || TestPt.Y < MainForm.GralDomRect.South ||
                TestPt.X > MainForm.GralDomRect.East || TestPt.Y > MainForm.GralDomRect.North)
            {
                MessageBox.Show(this, "Sample point is outside the GRAL domain", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor = Cursors.WaitCursor;
            //sample all files with GRAL results
            string files = Path.Combine(Gral.Main.ProjectName, @"Maps");

            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "(Mean*_total_*.txt)|Mean*_total_*.txt",
                Title = "Select concentration file",
                InitialDirectory = files
#if NET6_0_OR_GREATER
                ,
                ClientGuid = GralStaticFunctions.St_F.FileDialogMaps
#endif
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                string fname = dialog.FileName.Replace("total", "*");
                double[] conc = new double[101];               //concentrations for source apportionment
                FileInfo[] files_conc = new FileInfo[100];     //list of GRAL concentration files MEAN*.txt used for source apportionment

                DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(dialog.FileName));
                files_conc = di.GetFiles(Path.GetFileName(fname));

                string[] dummy = new string[1000000];

                //user defined background concentration
                int background = 0;
                if (InputBox1("Define background concentration", "Background concentration:", 0, 1000, ref background) == DialogResult.OK)
                {
                }

                MessageInfoForm = new MessageWindow();
                MessageInfoForm.FormClosed += new FormClosedEventHandler(MessageFormClosed);
                MessageInfoForm.Show();

                //read each of the files and extract the concentration at the selected location
                for (int i = 0; i < files_conc.Length; i++)
                {
                    try
                    {
                        if (MessageInfoForm == null)
                        {
                            MessageInfoForm = new MessageWindow();
                            MessageInfoForm.FormClosed += new FormClosedEventHandler(MessageFormClosed);
                            MessageInfoForm.Show();
                        }
                        MessageInfoForm.listBox1.Items.Add("Reading file: " + files_conc[i].Name);
                        MessageInfoForm.Refresh();
                    }
                    catch { }

                    string file = Path.Combine(files_conc[i].DirectoryName, files_conc[i].Name);

                    try
                    {
                        using (StreamReader myreader = new StreamReader(file))
                        {
                            dummy = myreader.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            int numbcol = Convert.ToInt32(dummy[1].Replace(".", decsep));
                            dummy = myreader.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            int numbraw = Convert.ToInt32(dummy[1].Replace(".", decsep));
                            dummy = myreader.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            double x11 = Convert.ToDouble(dummy[1].Replace(".", decsep));
                            dummy = myreader.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            double y11 = Convert.ToDouble(dummy[1].Replace(".", decsep));
                            dummy = myreader.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            double cellsize = Convert.ToDouble(dummy[1].Replace(".", decsep));
                            dummy = myreader.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);

                            //compute raw and column to extract data (no interpolation is applied)
                            int col = (int)((TestPt.X - x11) / cellsize) + 1;
                            int raw = (int)((TestPt.Y - y11) / cellsize) + 1;

                            if ((TestPt.X > x11 + cellsize * numbcol) || (TestPt.X < x11) || (TestPt.Y > y11 + cellsize * numbraw) || (TestPt.Y < y11))
                            {
                                MessageBox.Show(this, "Sample point is outside the GRAL domain", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                int finish = numbraw - raw + 2;
                                for (int j = 1; j < finish; j++)
                                {
                                    if (j < finish - 1)
                                    {
                                        myreader.ReadLine();
                                    }
                                    else
                                    {
                                        dummy = myreader.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    }
                                }
                                conc[i] = Convert.ToDouble(dummy[col - 1].Replace(".", decsep));
                            }
                        }
                    }
                    catch
                    { }
                }
                conc[files_conc.Length + 1] = Convert.ToDouble(background);

                //show pie chart
                Piediagram pie = new Piediagram(TestPt.X, TestPt.Y)
                {
                    FilesConc = files_conc,
                    Concentration = conc,
                    StartPosition = FormStartPosition.Manual
                };
                pie.Location = new Point(St_F.GetScreenAtMousePosition() + 600, St_F.GetTopScreenAtMousePosition() + 150);
                pie.Show();
                Cursor = Cursors.Default;
                if (MessageInfoForm != null)
                {
                    MessageInfoForm.Close();
                }
                files_conc = null;
            }
            dialog.Dispose();
        }

        /// <summary>
        /// Extract concentration value at a given location
        /// </summary>
        private void Button35_Click(object sender, EventArgs e)
        {
            InfoBoxCloseAllForms();
            string files = Path.Combine(Gral.Main.ProjectName, "Maps" + Path.DirectorySeparatorChar);
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "(*.dat;*.txt)|*.dat;*.txt",
                Title = "Select a concentration file",
                InitialDirectory = files
#if NET6_0_OR_GREATER
                ,
                ClientGuid = GralStaticFunctions.St_F.FileDialogMaps
#endif
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                ConcFilename = dialog.FileName;
                MouseControl = MouseMode.SetPointConcFile;
                Cursor = Cursors.Cross;
            }
            dialog.Dispose();
        }


        /// <summary>
        /// Extract the concentration value at a given location async
        /// </summary>
        /// <param name="FileName">File name with ESRi data</param>
        /// <param name="TestPt">Coordinates of the desired point</param>
        private async void GetConcentrationFromFile(string FileName, PointD TestPt)
        {
            Cursor = Cursors.WaitCursor;
            if (await System.Threading.Tasks.Task.Run(() => GetConcentration(FileName, TestPt)) == false)
            {
                MessageBoxTemporary Box = new MessageBoxTemporary("File not readable or sample point is outside the GRAL domain", Location);
                Box.Show();
            }
            Picturebox1_Paint();
            Cursor = Cursors.Cross;
        }

        /// <summary>
        /// Extract concentration value at a given location
        /// </summary>
        /// <param name="FileName">Name of file with ESRi data</param>
        /// <param name="TestPt">Coordinates of location</param>
        /// <returns>True if reading OK, otherwise false</returns>
        private bool GetConcentration(string FileName, PointD TestPt)
        {
            bool readingOK = false;
            //sample the selected files with GRAL results
            {
                string[] dummy = new string[1000000];

                //read the file and extract the concentration at the selected location
                try
                {
                    int index = CheckForExistingDrawingObject("CONCENTRATION VALUES");
                    DrawingObjects _drobj = ItemOptions[index];
                    if (_drobj.ContourFilename != Path.GetFileName(FileName)) // new File -> delete exiting infos
                    {
                        _drobj.ContourPolygons.Clear();
                        _drobj.ContourPolygons.TrimExcess();
                        _drobj.ContourFilename = Path.GetFileName(FileName);
                    }

                    using (StreamReader myreader = new StreamReader(FileName))
                    {
                        dummy = myreader.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        int numbcol = Convert.ToInt32(dummy[1].Replace(".", decsep));
                        dummy = myreader.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        int numbraw = Convert.ToInt32(dummy[1].Replace(".", decsep));
                        dummy = myreader.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        double x11 = Convert.ToDouble(dummy[1].Replace(".", decsep));
                        dummy = myreader.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        double y11 = Convert.ToDouble(dummy[1].Replace(".", decsep));
                        dummy = myreader.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        double cellsize = Convert.ToDouble(dummy[1].Replace(".", decsep));
                        dummy = myreader.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        string unit = "";
                        if (dummy.Length > 3) // read unit, if available
                        {
                            unit = dummy[3].Trim();
                        }

                        //compute row and column to extract data (no interpolation is applied)
                        int col = (int)((TestPt.X - x11) / cellsize) + 1;
                        int row = (int)((TestPt.Y - y11) / cellsize) + 1;

                        if ((TestPt.X > x11 + cellsize * numbcol) || (TestPt.X < x11) || (TestPt.Y > y11 + cellsize * numbraw) || (TestPt.Y < y11))
                        {

                        }
                        else
                        {
                            int finish = numbraw - row + 2;
                            for (int j = 1; j < finish; j++)
                            {
                                if (j < finish - 1)
                                {
                                    myreader.ReadLine();
                                }
                                else
                                {
                                    dummy = myreader.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                }
                            }
                            double concentration = Convert.ToDouble(dummy[col - 1].Replace(".", decsep));

                            if (unit.Length == 0) // unit not available from file -> loot to filename
                            {
                                string temp = Path.GetFileName(FileName).ToUpper();

                                GralData.ContourPolygon _d = new GralData.ContourPolygon();
                                _d.EdgePoints = new PointD[2];
                                _d.EdgePoints[0] = new PointD(TestPt.X, TestPt.Y);
                                _d.EdgePoints[1] = new PointD(concentration * 1E12, 0);

                                _drobj.ContourPolygons.Add(_d); // position and concentration

                                if (temp.Contains("ODOUR"))
                                {
                                    _drobj.LegendTitle = "Odour hours";
                                    _drobj.LegendUnit = @" %";
                                }
                                else if (temp.Contains("DEPOSITION"))
                                {
                                    _drobj.LegendTitle = "Deposition";
                                    _drobj.LegendUnit = Gral.Main.mg_p_m2;
                                }
                                else if (temp.Contains("ROUGHNESS.TXT") || temp.Contains("ROUGHNESSLENGTHSGRAL.TXT"))
                                {
                                    _drobj.LegendTitle = "Roughness";
                                    _drobj.LegendUnit = "m";
                                }
                                else if (temp.Contains("GGEOM.TXT"))
                                {
                                    _drobj.LegendTitle = "Height";
                                    _drobj.LegendUnit = " m";
                                }
                                else if (temp.Contains("TOPOGRAPHY"))
                                {
                                    _drobj.LegendTitle = "Height";
                                    _drobj.LegendUnit = " m";
                                }
                                else if (temp.ToLower().Contains("height"))
                                {
                                    _drobj.LegendTitle = "Height";
                                    _drobj.LegendUnit = " m";
                                }
                                else if (temp.Contains("MOISTURE.TXT"))
                                {
                                    _drobj.LegendTitle = "Moisture";
                                    _drobj.LegendUnit = string.Empty;
                                }
                                else
                                {
                                    _drobj.LegendTitle = "Concentration";
                                    _drobj.LegendUnit = Gral.Main.My_p_m3;
                                }
                                readingOK = true;
                            }
                            else
                            {
                                string temp = Path.GetFileName(FileName).ToUpper();

                                GralData.ContourPolygon _d = new GralData.ContourPolygon();
                                _d.EdgePoints = new PointD[2];
                                _d.EdgePoints[0] = new PointD(TestPt.X, TestPt.Y);
                                _d.EdgePoints[1] = new PointD(concentration * 1E12, 0);
                                _drobj.ContourPolygons.Add(_d); // position and concentration

                                if (unit.Contains("m" + Gral.Main.SquareString) || unit.Contains("ha") | unit.Contains("km" + Gral.Main.SquareString))
                                {
                                    _drobj.LegendTitle = "Deposition";
                                    _drobj.LegendUnit = unit;
                                }
                                else if (unit.Contains("%"))
                                {
                                    _drobj.LegendTitle = "Odour hours";
                                    _drobj.LegendUnit = " %";
                                }
                                else if (temp.Contains("ROUGHNESS.TXT"))
                                {
                                    _drobj.LegendTitle = "Roughness";
                                    _drobj.LegendUnit = string.Empty;
                                }
                                else if (temp.Contains("GGEOM.TXT"))
                                {
                                    _drobj.LegendTitle = "Height";
                                    _drobj.LegendUnit = " m";
                                }
                                else if (temp.Contains("MOISTURE.TXT"))
                                {
                                    _drobj.LegendTitle = "Moisture";
                                    _drobj.LegendUnit = string.Empty;
                                }
                                else
                                {
                                    _drobj.LegendTitle = "Concentration";
                                    _drobj.LegendUnit = unit;
                                }
                                readingOK = true;
                            }
                        }

                    }
                }
                catch
                { readingOK = false; }
                return readingOK;
            }
        }

        /// <summary>
        /// Remove a map from the opbject manager
        /// </summary>
        public void RemoveMap(int index)
        {
            RemoveItems(index);
            SaveDomainSettings(0);
        }

        /// <summary>
        /// Routine converting GRAL *.con files into ESRI-ASCII format
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button40_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            //sample the selected files with GRAL results
            string files = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar);
            string tempPath = Path.Combine(Gral.Main.ProjectName, "Computation", "Temp");

            DirectoryInfo di = new DirectoryInfo(files);
            FileInfo[] GRAL_con = di.GetFiles("*.grz");
            int temp = GRAL_con.Length;

            bool compressed = false; // compressed files?
            string compressed_file = "";

            if (temp != 0) // compressed files?
            {
                //select dispersion situation
                SelectDispersionSituation disp = new SelectDispersionSituation(this, MainForm)
                {
                    selectGRAMM_GRAL = 2,
                    ShowConcentrationFiles = true,
                    GRZPath = files
                };

                disp.StartPosition = FormStartPosition.Manual;
                disp.Location = GetScreenPositionForNewDialog(2);
                if (disp.ShowDialog() == DialogResult.OK && disp.selected_situation > 0) // unzip the *.con files from this situation
                {
                    int dissit = disp.selected_situation; // selected situation
                    compressed_file = Convert.ToString(dissit).PadLeft(5, '0') + ".grz";
                    string comp_file = Path.Combine(files, compressed_file);   // filename of a compressed file

                    // unzip compressed file if uncompressed results not available
                    if (File.Exists(comp_file))
                    {
                        try
                        {
                            await System.Threading.Tasks.Task.Run(() => ZipFile.ExtractToDirectory(comp_file, tempPath));
                            compressed = true;
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("already exists") == false) // otherwise continue, because results are available!
                            {
                                Cursor = Cursors.Default;
                                MessageBox.Show(this, "Can't unzip " + comp_file, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return; //error
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, "File " + comp_file + " not found", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else // cancel
                {
                    Cursor = Cursors.Default;
                    return;
                }
                disp.Dispose();
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "(*.con)|*.con",
                Title = "Select a concentration file",
                InitialDirectory = Path.Combine(Gral.Main.ProjectName, "Computation")
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    CancellationTokenReset();
                    if (await System.Threading.Tasks.Task.Run(() => ReadConAndWriteESRI(dialog.FileName, string.Empty, CancellationTokenSource.Token)) == true)
                    {
                        MessageBoxTemporary Box = new MessageBoxTemporary("File successfully converted", Location);
                        Box.Show();
                    }
                    else
                    {
                        MessageBox.Show(this, "Could not read selected *.con file", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch { }
            } // select *.con file

            if (compressed) // delete all unzipped *.con and *.odr files
            {
                try
                {
                    System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(tempPath);
                    foreach (FileInfo file in directory.GetFiles())
                    {
                        file.Delete();
                    }
                    Directory.Delete(tempPath);
                }
                catch { }
            } // delete unzipped files

            Cursor = Cursors.Default;
        }

        private void CancelAnimatedGIF(object sender, EventArgs e)
        {
            OnlineCounter = 1000;
        }

        /// <summary>
        /// Create an animated GIF picture from *.con or *.grz files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateAnmiatedGIF(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            //sample the selected files with GRAL results
            string files = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar);

            DirectoryInfo di = new DirectoryInfo(files);
            FileInfo[] GRAL_con = di.GetFiles("*.grz");
            int grzFileCount = GRAL_con.Length;

            if (grzFileCount == 0)
            {
                Cursor = Cursors.Cross;
                MessageBox.Show(this, "No *.grz files (GRAL result files) available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // find the position of the displayed *.txt file within the drawing list
            DrawingObjects _drobj = null;
            string SliceAndSourceGroup = "-101.con";

            foreach (DrawingObjects _dr in ItemOptions)
            {
                if (_dr.Name.StartsWith("CM:"))
                {
                    // check contour file name
                    string contfilename = Path.GetFileNameWithoutExtension(_dr.ContourFilename);
                    if (contfilename.Contains("-") && contfilename.IndexOf("-") == 5)
                    {
                        _drobj = _dr;
                        SliceAndSourceGroup = contfilename.Substring(contfilename.IndexOf("-")) + ".con";
                        break;
                    }
                }
            }

            if (_drobj == null)
            {
                Cursor = Cursors.Cross;
                MessageBox.Show(this, "No contour map for a single situation available \n Please convert a concentration file to a *.txt file and load that file as contour lines", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string ResultFile = Path.Combine(Gral.Main.ProjectName, "Maps", "AnimatedGIF.gif");
            int FrameDelay = 150;
            int FirstSit = 1;
            int FinalSit = 10;
            using (CreateAnimatedGIFs animgifs = new CreateAnimatedGIFs
            {

                GIFFileName = ResultFile,
                FinalSituation = grzFileCount,
                StartSituation = FirstSit,
                FrameDelay = FrameDelay
            })
            {
                animgifs.StartPosition = FormStartPosition.Manual;
                animgifs.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 260;
                animgifs.Top = St_F.GetTopScreenAtMousePosition() + 150;

                if (animgifs.ShowDialog(this) == DialogResult.OK)
                {
                    ResultFile = animgifs.GIFFileName;
                    FrameDelay = animgifs.FrameDelay;
                    FirstSit = animgifs.StartSituation;
                    FinalSit = animgifs.FinalSituation;
                }
                else
                {
                    Cursor = Cursors.Cross;
                    return;
                }
            }

            //MessageBox.Show(this, SliceAndSourceGroup , "GRAL GUI");
            bool compressed = false; // compressed files?
            string compressed_file = "";

            button55.Click -= new System.EventHandler(this.CreateAnmiatedGIF);
            button55.Click += new System.EventHandler(this.CancelAnimatedGIF);
            button55.BackgroundImage = Gral.Properties.Resources.StopRecording;
            toolTip1.SetToolTip(button55, "Stop capturing");
            OnlineCounter = -1;
            string _originalLegendTitle = _drobj.LegendTitle;

            using (GralDomForms.GifWriter gfw = new GifWriter(ResultFile, FrameDelay, -1))
            {
                for (int dissit = FirstSit; dissit < FinalSit; dissit++)
                {
                    try
                    {
                        Application.DoEvents();

                        compressed_file = Convert.ToString(dissit).PadLeft(5, '0') + ".grz";
                        string comp_file = Path.Combine(files, compressed_file);   // filename of a compressed file
                                                                                   //MessageBox.Show(compressed_file);
                                                                                   // unzip compressed file if uncompressed results not available
                        if (File.Exists(comp_file))
                        {
                            try
                            {
                                ZipFile.ExtractToDirectory(comp_file, files);
                                compressed = true;
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message.Contains("already exists") == false) // otherwise continue, because results are available!
                                {
                                    //Cursor = Cursors.Default;
                                    //return; //error
                                }
                            }
                        }

                        string confilename = Path.Combine(files, Path.GetFileNameWithoutExtension(compressed_file) + SliceAndSourceGroup);
                        //MessageBox.Show(confilename + "/"+ _drobj.ContourFilename);
                        CancellationTokenReset();
                        if (ReadConAndWriteESRI(confilename, _drobj.ContourFilename, CancellationTokenSource.Token) == true)
                        {
                            MessageBoxTemporary Box = new MessageBoxTemporary("File successfully converted", Location);
                            Box.Show();
                        }
                        else
                        {
                            MessageBoxTemporary Box = new MessageBoxTemporary("Could not read selected *.con file", Location);
                            Box.Show();
                            //MessageBox.Show(this, "Could not read selected *.con file", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        //compute contour polygons
                        ReDrawContours = true;
                        _drobj.LegendTitle = _originalLegendTitle + " / " + compressed_file;
                        Contours(_drobj.ContourFilename, _drobj);
                        Picturebox1_Paint();
                        gfw.WriteFrame(picturebox1.Image);
                        _drobj.LegendTitle = _originalLegendTitle;

                        if (compressed) // delete all unzipped *.con and *.odr files
                        {
                            GRAL_con = di.GetFiles("*.con");
                            if (GRAL_con.Length > 0)
                            {
                                for (int i = 0; i < GRAL_con.Length; i++)
                                {
                                    try
                                    {
                                        if (GRAL_con[i].Name.Substring(0, 5) == compressed_file.Substring(0, 5))
                                        {
                                            GRAL_con[i].Delete();
                                        }
                                    }
                                    catch { }
                                }
                            }

                            GRAL_con = di.GetFiles("*.odr"); // delete *.odr files
                            if (GRAL_con.Length > 0)
                            {
                                for (int i = 0; i < GRAL_con.Length; i++)
                                {
                                    try
                                    {
                                        if (GRAL_con[i].Name.Substring(0, 5) == compressed_file.Substring(0, 5))
                                        {
                                            GRAL_con[i].Delete();
                                        }
                                    }
                                    catch { }
                                }
                            }
                        } // delete unzipped files
                    }
                    catch { _drobj.LegendTitle = _originalLegendTitle; }

                    if (OnlineCounter == 1000) // cancel from user
                    {
                        dissit = FinalSit;
                    }
                }
            }

            Cursor = Cursors.Cross;

            // reset button behaviour
            button55.Click -= new System.EventHandler(this.CancelAnimatedGIF);
            this.button55.Click += new System.EventHandler(this.CreateAnmiatedGIF);
            button55.BackgroundImage = Gral.Properties.Resources.RedDotTransparent;
            toolTip1.SetToolTip(button55, "Create animated GIF from a series of *.con files");
            OnlineCounter = 0;
        }


        /// <summary>
        /// Read a concentration file and write ASCII ESRI file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="filenameESRI">ESRI filename -> if string.empty -> filename + .txt</param>
        private bool ReadConAndWriteESRI(string filename, string filenameESRI, System.Threading.CancellationToken cts)
        {
            bool resultOK = false;
            try
            {
                GralBackgroundworkers.BackgroundworkerData bwd = new GralBackgroundworkers.BackgroundworkerData();
                bwd.Horgridsize = (float)MainForm.HorGridSize;
                bwd.DomainWest = MainForm.GralDomRect.West;
                bwd.DomainSouth = MainForm.GralDomRect.South;
                bwd.CellsGralX = MainForm.CellsGralX;
                bwd.CellsGralY = MainForm.CellsGralY;
                float[][][] conc = Landuse.CreateArray<float[][]>(bwd.CellsGralX + 1, () =>
                                   Landuse.CreateArray<float[]>(bwd.CellsGralY + 1, () => new float[1]));

                bool ReadingOK = false;

                // use the existing *.con and *.grz reader from the backgroundworker
                using (GralBackgroundworkers.ProgressFormBackgroundworker Background = new GralBackgroundworkers.ProgressFormBackgroundworker(bwd))
                {
                    ReadingOK = Background.ReadConFiles(filename, bwd, 0, ref conc);
                }

                if (!ReadingOK)
                {
                    return false;
                }

                //write ESRI-ASCII File
                string name = Path.GetFileNameWithoutExtension(filename);
                string file = Path.Combine(Gral.Main.ProjectSetting.EvaluationPath, name + ".txt");
                if (!string.IsNullOrEmpty(filenameESRI))
                {
                    file = filenameESRI;
                }
                WriteESRIFile Result = new WriteESRIFile
                {
                    NCols = MainForm.CellsGralX,
                    NRows = MainForm.CellsGralY,
                    YllCorner = MainForm.GralDomRect.South,
                    XllCorner = MainForm.GralDomRect.West,
                    CellSize = (float)MainForm.HorGridSize,
                    Unit = Gral.Main.My_p_m3,
                    Round = 3,
                    Z = 0,
                    Values = conc,
                    FileName = file
                };
                Result.WriteFloatResult();

                resultOK = true;
            }
            catch
            {
                resultOK = false;
            }
            return resultOK;
        }

        /// <summary>
        /// Write a log file for the GRAMM calculation settings
        /// </summary>
        private void WriteGrammLog(int select, string info1, string info2, string info3)
        {
            // Kuntner: writes a log File with Informations about the GRAMM field computation
            string log_path;
            log_path = Path.Combine(Gral.Main.ProjectName, @"Computation", "Logfile_GRAMM.txt");

            using (StreamWriter mywriter = File.AppendText(log_path))
            {

                if (select == 2)
                {
                    mywriter.WriteLine(" ");
                    mywriter.WriteLine("Re-Order, started at " + Convert.ToString(DateTime.Now).PadRight(65, '_'));
                    mywriter.WriteLine(info1.PadLeft(10) + "\t  // X-coordinate of reference point");
                    mywriter.WriteLine(info2.PadLeft(10) + "\t  // Y-coordinate of reference point");
                    mywriter.WriteLine(info3.PadLeft(10) + "\t  // Z-coordinate of reference point");
                }

                if (select == 3)
                {
                    mywriter.WriteLine(" ");
                    mywriter.WriteLine("Match with meteorological observation, started at " + Convert.ToString(DateTime.Now).PadRight(65, '_'));
                    if (MMO.radioButton1.Checked == true)
                    {
                        mywriter.WriteLine(" Optimization methode: vectorial");
                    }

                    if (MMO.radioButton2.Checked == true)
                    {
                        mywriter.WriteLine(" Optimization methode: components");
                    }

                    if (MMO.checkBox1.Checked == false)
                    {
                        mywriter.WriteLine(" Remove outliers: off");
                    }
                    else
                    {
                        mywriter.WriteLine(" Remove outliers: on");
                    }

                    if (MMO.checkBox2.Checked == true)
                    {
                        mywriter.WriteLine(" Use local stability classes");
                    }
                    else
                    {
                        mywriter.WriteLine(" Use global stability classes");
                    }

                    if (MMO.concatenate.Value > 0.0M)
                    {
                        mywriter.WriteLine(" Concatenation for situations less " + Convert.ToString(MMO.concatenate.Value) + " per mil ");
                    }

                    for (int i = 0; i < MMO.MetFileNames.Count; i++)
                    {
                        mywriter.WriteLine(MMO.MetFileNames[i] + "\t  // Used Metfiles");
                        mywriter.WriteLine(Convert.ToString(MMO.dataGridView1.Rows[i].Cells[1].Value).PadLeft(10) + "\t  // X-coordinate of met. station");
                        mywriter.WriteLine(Convert.ToString(MMO.dataGridView1.Rows[i].Cells[2].Value).PadLeft(10) + "\t  // Y-coordinate of met. station");
                        mywriter.WriteLine(Convert.ToString(MMO.dataGridView1.Rows[i].Cells[3].Value).PadLeft(10) + "\t  // Z-coordinate of met. station");
                        mywriter.WriteLine("Weighting factor " + MMO.dataGridView1.Rows[i].Cells[6].Value.ToString());
                        mywriter.WriteLine("Directional weighting factor " + MMO.dataGridView1.Rows[i].Cells[7].Value.ToString());
                    }
                }

                if (select == 4) // Error
                {
                    mywriter.WriteLine(" ");
                    mywriter.WriteLine(info1);
                }
            }

        }

        /// <summary>
        /// Set or load a viewframe 
        /// </summary>
        private void button57_Click(object sender, EventArgs e)
        {
            Point pt = new Point(Math.Max(0, Right - 700), Math.Min(Screen.FromControl(button57).Bounds.Height - 200, Top + 250));
            using (GralDomForms.ViewFrameSaveAndLoad viewframe = new ViewFrameSaveAndLoad())
            {
                viewframe.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                viewframe.Location = pt;
                if (viewframe.ShowDialog() == DialogResult.OK)
                {
                    if (viewframe.SelText.Length > 0)
                    {
                        ViewFrameSet(viewframe.SelText);
                    }
                    if (viewframe.NewText.Length > 0)
                    {
                        ViewFrameSave(viewframe.NewText);
                        ViewFrameMenuBarLoad();
                    }
                    if (viewframe.SelEntry > -1)
                    {
                        ViewFrameDelete(viewframe.SelEntry);
                        ViewFrameMenuBarLoad();
                    }
                }
            }
        }

        //void ViewframeKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Enter && e.Shift == false)
        //    {
        //        // If Return is pressed save view if Text <> Empty and Reload Combobox
        //        e.Handled = true;
        //        string Text = toolStripComboBox1.Text;

        //        //if (Text.Length > 0)
        //        {
        //            ViewFrameSave(Text);
        //            //MessageBox.Show(this, "Enter pressed", Text);
        //            if (sender == toolStripComboBox1)
        //                toolStripComboBox1.Items.Clear();

        //            ViewFrameMenuBarLoad();
        //        }
        //        groupBox2.Focus(); // Remove Focus from Combobox
        //    }

        //    if (e.KeyCode == Keys.Enter && e.Shift)
        //    {
        //        e.Handled = true;

        //        if (toolStripComboBox1.SelectedIndex < 0 || toolStripComboBox1.Items.Count == 0) return;
        //        if (MessageBox.Show(this, "Delete viewframe " + toolStripComboBox1.SelectedText +" ?", "Viewframe delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;
        //        ViewFrameDelete(toolStripComboBox1.Items.Count - toolStripComboBox1.SelectedIndex);
        //        toolStripComboBox1.Items.Clear();

        //        Application.DoEvents();
        //        ViewFrameMenuBarLoad();
        //        groupBox2.Focus(); // Remove Focus from Combobox
        //    }

        //}

        /// <summary>
        /// Load a viewframe 
        /// </summary>
        void ViewframeSelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender == toolStripComboBox1)
            {
                string Text = toolStripComboBox1.SelectedItem.ToString();
                groupBox2.Focus(); // Remove Focus from Combobox
                ViewFrameSet(Text);
                //				viewframe.SelectedIndexChanged -= new System.EventHandler(ViewframeSelectedIndexChanged);
                //				viewframe.Text = toolStripComboBox1.Text;
                //				viewframe.SelectedIndexChanged += new System.EventHandler(ViewframeSelectedIndexChanged);
            }
            //MessageBox.Show(this, "Selektiert", Text);
        }

        /// <summary>
        /// Set a viewframe 
        /// </summary>
        private void ViewFrameSet(string Text)
        {
            // Read View.TXT until Text is found and set new zoom
            try
            {
                string path;
                path = Path.Combine(Gral.Main.ProjectName, @"Settings", "sections.txt");

                using (StreamReader myreader = new StreamReader(path)) // Open File
                {
                    string s;
                    while (myreader.EndOfStream == false)
                    {
                        s = myreader.ReadLine();
                        if (s == Text) // found selected Text
                        {
                            s = myreader.ReadLine();
                            double x0 = St_F.TxtToDbl(s, false);
                            s = myreader.ReadLine();
                            double xmax = St_F.TxtToDbl(s, false);
                            s = myreader.ReadLine();
                            double y0 = St_F.TxtToDbl(s, false);
                            ZoomSection(x0, xmax, y0);
                        }
                    }
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Move and zoom the map to given coordinates
        /// </summary>
        private void ZoomSection(double x0, double xmax, double y0)
        {
            double temp = picturebox1.Width * MapSize.SizeX / (xmax - x0);

            if (temp > 0)
            {
                double xfac_old = XFac;
                double transformx_old = TransformX;
                double transformy_old = TransformY;
                XFac = temp;
                BmpScale = 1 / XFac;

                TransformX = Convert.ToInt32(-(x0 - MapSize.West) * XFac / MapSize.SizeX);
                TransformY = Convert.ToInt32(-(y0 - MapSize.North) * XFac / MapSize.SizeY);

                try // Kuntner: catch 1/0 and convert.ToInt32 Overflow
                {

                    foreach (DrawingObjects _dr in ItemOptions)
                    {
                        try
                        {
                            _dr.DestRec = new Rectangle(TransformX + Convert.ToInt32((_dr.West - MapSize.West) / BmpScale / MapSize.SizeX),
                                                        TransformY - Convert.ToInt32((_dr.North - MapSize.North) / BmpScale / MapSize.SizeX),
                                                        Convert.ToInt32(_dr.Picture.Width * _dr.PixelMx / MapSize.SizeX * XFac),
                                                        Convert.ToInt32(_dr.Picture.Height * _dr.PixelMx / MapSize.SizeX * XFac));
                            _dr.SourceRec = new Rectangle(0, 0, _dr.Picture.Width, _dr.Picture.Height);
                        }
                        catch { }
                    }

                    MoveCoordinatesOfEditedItems(xfac_old, transformx_old, transformy_old);
                }
                catch
                { }
                Picturebox1_Paint();
            }
        }

        /// <summary>
        /// Set the viewframe names to the combo box
        /// </summary>
        private void ViewFrameMenuBarLoad()
        {
            // Load all
            toolStripComboBox1.Items.Clear(); // clear all items

            // Read View.TXT and put all Elements to the Combobox
            try
            {
                string path;
                path = Path.Combine(Gral.Main.ProjectName, @"Settings", "sections.txt");

                using (StreamReader myreader = new StreamReader(path)) // Open File
                {
                    string s;
                    while (myreader.EndOfStream == false)
                    {
                        s = myreader.ReadLine();
                        toolStripComboBox1.Items.Insert(0, s);
                        s = myreader.ReadLine();
                        s = myreader.ReadLine();
                        s = myreader.ReadLine();
                        s = myreader.ReadLine();
                    }
                }

            }
            catch
            { }
        }

        /// <summary>
        /// Write the viewframes to the file sections.txt 
        /// </summary>
        private void ViewFrameSave(string Text)
        {
            try
            {
                string path;
                path = Path.Combine(Gral.Main.ProjectName, @"Settings", "sections.txt");

                using (StreamWriter mywriter = new StreamWriter(path, true)) // Append File
                {
                    mywriter.WriteLine(Text);

                    double x0 = Math.Round((0 - TransformX) * BmpScale * MapSize.SizeX + MapSize.West, 1, MidpointRounding.AwayFromZero);
                    double xmax = Math.Round((picturebox1.Width - TransformX) * BmpScale * MapSize.SizeX + MapSize.West, 1, MidpointRounding.AwayFromZero);
                    double y0 = Math.Round((0 - TransformY) * BmpScale * MapSize.SizeY + MapSize.North, 1, MidpointRounding.AwayFromZero);

                    mywriter.WriteLine(St_F.DblToIvarTxt(x0));
                    mywriter.WriteLine(St_F.DblToIvarTxt(xmax));
                    mywriter.WriteLine(St_F.DblToIvarTxt(y0));
                    mywriter.WriteLine("----------------------");
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Deleta a viewframe 
        /// </summary>
        private void ViewFrameDelete(int index)
        {
            try
            {
                string path = Path.Combine(Gral.Main.ProjectName, @"Settings", "sections.txt");
                string path2 = Path.Combine(Gral.Main.ProjectName, @"Settings", "view_temp.txt");
                string[] s = new string[5];
                if (File.Exists(path2))
                {
                    File.Delete(path2); // delete old path
                }

                File.Move(path, path2);

                using (StreamReader myreader = new StreamReader(path2)) // Open File
                {
                    using (StreamWriter mywriter = new StreamWriter(path, false)) // Write new File
                    {
                        int i = 1;
                        while (!myreader.EndOfStream)
                        {
                            s[0] = myreader.ReadLine();
                            s[1] = myreader.ReadLine();
                            s[2] = myreader.ReadLine();
                            s[3] = myreader.ReadLine();
                            s[4] = myreader.ReadLine();

                            if (i != index) // don't write index which should be deleted
                            {
                                mywriter.WriteLine(s[0]);
                                mywriter.WriteLine(s[1]);
                                mywriter.WriteLine(s[2]);
                                mywriter.WriteLine(s[3]);
                                mywriter.WriteLine(s[4]);
                            }
                            i++;
                        }
                        mywriter.Flush();
                    }
                }

                File.Delete(path2); // delete old path
            }
            catch
            { }

        }

        /// <summary>
        /// Start a 3D view 
        /// </summary>
        void Button3DClick(object sender, EventArgs e)
        {
#if __MonoCS__
            MessageBox.Show(this, "This function is not available at LINUX yet");
#else
            bool smooth = true;
            double vert_fac = 2;
            bool GRAL_Topo = false;
            if (CellHeightsType == 2) // Show GRAL height -> show GRAL 3D
            {
                GRAL_Topo = true;
            }

            using (Dialog_3D dial = new Dialog_3D
            {
                Smoothing = smooth,
                VertFactor = vert_fac,
                GRAL_Topo = GRAL_Topo
            })
            {
                dial.StartPosition = FormStartPosition.Manual;
                dial.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 260;
                dial.Top = St_F.GetTopScreenAtMousePosition() + 150;

                if (dial.ShowDialog() == DialogResult.Cancel)
                {
                    dial.Dispose();
                    return; // Cancel
                }

                smooth = dial.Smoothing;
                vert_fac = dial.VertFactor;
                GRAL_Topo = dial.GRAL_Topo;
            }

            if (GRAL_Topo == true) // Show GRAL height
            {
                GRAL_3D_View(smooth, vert_fac);
            }
            else
            {
                Domain3DView(smooth, vert_fac);
            }
#endif
        }

        /// <summary>
        /// Sort integer values descending direction
        /// </summary>
        private class SortIntDescending : IComparer<int>
        {
            int IComparer<int>.Compare(int a, int b) //implement Compare
            {
                if (a > b)
                {
                    return -1; //normally greater than = 1
                }

                if (a < b)
                {
                    return 1; // normally smaller than = -1
                }
                else
                {
                    return 0; // equal
                }
            }
        }

        /// <summary>
        /// Show or hide the toolbox panel 
        /// </summary>
        void ToolBoxToolStripMenuItemClick(object sender, EventArgs e)
        {
            toolBoxToolStripMenuItem.Checked = !toolBoxToolStripMenuItem.Checked;
            if (toolBoxToolStripMenuItem.Checked)
            {
                panel1.Show();
            }
            else
            {
                panel1.Hide();
            }
            Cursor = Cursors.Arrow;
        }

        void PointSourcesToolStripMenuItemClick(object sender, EventArgs e)
        {
            checkBox4.Checked = !checkBox4.Checked;
            ResetSelectionChecked();
        }

        void AreaSourcesToolStripMenuItemClick(object sender, EventArgs e)
        {
            checkBox5.Checked = !checkBox5.Checked;
            ResetSelectionChecked();
        }

        void LineSourcesToolStripMenuItemClick(object sender, EventArgs e)
        {
            checkBox8.Checked = !checkBox8.Checked;
            ResetSelectionChecked();
        }

        void TunnelPortalsToolStripMenuItemClick(object sender, EventArgs e)
        {
            checkBox12.Checked = !checkBox12.Checked;
            ResetSelectionChecked();
        }

        void BuildingsToolStripMenuItemClick(object sender, EventArgs e)
        {
            checkBox15.Checked = !checkBox15.Checked;
            ResetSelectionChecked();
        }

        void ReceptorPointsToolStripMenuItemClick(object sender, EventArgs e)
        {
            checkBox20.Checked = !checkBox20.Checked;
            ResetSelectionChecked();
        }

        void WallsToolStripMenuItemClick(object sender, EventArgs e)
        {
            checkBox25.Checked = !checkBox25.Checked;
            ResetSelectionChecked();
        }

        void VegetationToolStripMenuItemClick(object sender, EventArgs e)
        {
            checkBox26.Checked = !checkBox26.Checked;
            ResetSelectionChecked();
        }

        void OnePointAndScaleToolStripMenuItemClick(object sender, EventArgs e)
        {
            onePointAndScaleToolStripMenuItem.Checked = !onePointAndScaleToolStripMenuItem.Checked;
            toolStripButton3.PerformClick();
        }
        void CloseGeoRef1(object sender, EventArgs e)
        {
            OnePointAndScaleToolStripMenuItemClick(null, null);
        }
        void TwoPointsToolStripMenuItemClick(object sender, EventArgs e)
        {
            twoPointsToolStripMenuItem.Checked = !twoPointsToolStripMenuItem.Checked;
            toolStripButton4.PerformClick();
        }
        void CloseGeoRef2(object sender, EventArgs e)
        {
            TwoPointsToolStripMenuItemClick(null, null);
        }
        void MoveAndZoomMapToolStripMenuItemClick(object sender, EventArgs e)
        {
            ResetSelectionChecked();
            moveAndZoomMapToolStripMenuItem.Checked = !moveAndZoomMapToolStripMenuItem.Checked;
            toolStripButton1.PerformClick();
        }

        /// <summary>
        /// Move toolbar from left to right
        /// </summary>
        void Button46Click(object sender, EventArgs e)
        {
#if __MonoCS__
            if (panel1.Left < picturebox1.Width / 2)
                panel1.Left = picturebox1.Width - 150;
            else
                panel1.Left = 1;
            return;
#endif
            if (panel1.Dock == DockStyle.Right)
            {
                panel1.Visible = false;
                panel1.Dock = DockStyle.Left;
                panel1.Visible = true;
            }
            else
            {
                panel1.Visible = false;
                panel1.Dock = DockStyle.Right;
                panel1.Visible = true;
            }
        }


        /// <summary>
        /// Delete all selected items 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DeleteSelectedItemsToolStripMenuItemClick(object sender, EventArgs e)
        {
            //delete selected area source
            if (MouseControl == MouseMode.AreaSourceSel)
            {
                ItemsDelete("area source");
                EditAndSaveAreaSourceData(sender, e);
            }
            //delete selected line source
            else if (MouseControl == MouseMode.LineSourceSel)
            {
                ItemsDelete("line source");
                EditAndSaveLineSourceData(sender, e);
            }
            //delete selected point source
            else if (MouseControl == MouseMode.PointSourceSel)
            {
                ItemsDelete("point source");
                EditAndSavePointSourceData(sender, e);
            }
            //delete selected tunnel portal
            else if (MouseControl == MouseMode.PortalSourceSel)
            {
                ItemsDelete("portal");
                EditAndSavePortalSourceData(sender, e);
            }
            //delete selected building
            else if (MouseControl == MouseMode.BuildingSel)
            {
                ItemsDelete("building");
                EditAndSaveBuildingsData(sender, e);
            }
            else
            {
                MessageBox.Show(this, "No items selected", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Picturebox1_Paint();
        }

        /// <summary>
        /// Save item data to disk
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WriteAllItemsToDisk(object sender, EventArgs e)
        {
            EditAndSaveAreaSourceData(sender, e);
            if (EditAS.ItemData.Count > 0)
            {
                EditAS.FillValues();
            }
            EditAndSaveLineSourceData(sender, e);
            if (EditLS.ItemData.Count > 0)
            {
                EditLS.FillValues();
            }
            EditAndSavePointSourceData(sender, e);
            if (EditPS.ItemData.Count > 0)
            {
                EditPS.FillValues();
            }
            EditAndSavePortalSourceData(sender, e);
            if (EditPortals.ItemData.Count > 0)
            {
                EditPortals.FillValues();
            }
            EditAndSaveBuildingsData(sender, e);
            if (EditB.ItemData.Count > 0)
            {
                EditB.FillValues();
            }
            EditAndSaveReceptorData(sender, e);
            if (EditR.ItemData.Count > 0)
            {
                EditR.FillValues();
            }
            EditAndSaveWallData(sender, e);
            if (EditWall.ItemData.Count > 0)
            {
                EditWall.FillValues();
            }
            EditAndSaveVegetationData(sender, e);
            if (EditVegetation.ItemData.Count > 0)
            {
                EditVegetation.FillValues();
            }

            Picturebox1_Paint();
        }

        void CrossCursorToolStripMenuItemClick(object sender, EventArgs e)
        {
            Cursor = Cursors.Cross;
        }

        void ExitGISWindowToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        void ToolStripMenuItem18Click(object sender, EventArgs e)
        {
            ShowLenghtLabel = !ShowLenghtLabel;
            toolStripMenuItem18.Checked = ShowLenghtLabel;
        }

        /// <summary>
        /// Read the GRAL geometry from the file GRAL_topofile.txt
        /// </summary>
        bool ReadGralGeometry()
        {
            bool ok = false;
            string file = Path.Combine(Gral.Main.ProjectName, "Computation", "GRAL_topofile.txt");
            if (File.Exists(file))
            {
                try
                {
                    string[] data;
                    using (StreamReader myReader = new StreamReader(file))
                    {
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        int nx = Convert.ToInt32(data[1]);
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        int ny = Convert.ToInt32(data[1]);
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        double x11 = Convert.ToDouble(data[1].Replace(".", decsep));
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        double y11 = Convert.ToDouble(data[1].Replace(".", decsep));
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        double dx = Convert.ToDouble(data[1].Replace(".", decsep));
                        data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        int nodata = Convert.ToInt32(data[1]);
                        data = new string[nx + 1];
                        CellHeights = new float[nx + 1, ny + 1];
                        for (int i = ny; i > 0; i--)
                        {
                            data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int j = 1; j <= nx; j++)
                            {
                                CellHeights[j, i] = Convert.ToSingle(data[j - 1].Replace(".", decsep));
                            }
                        }
                    }
                    ok = true;
                }
                catch
                { }

            }
            return ok;
        }

        /// <summary>
        /// Write the GRAL geometry to the file GRAL_topofile.txt
        /// </summary>
        bool WriteGralGeometry(System.Threading.CancellationToken cts)
        {
            bool ok = false;
            string file = Path.Combine(Gral.Main.ProjectName, "Computation", "GRAL_topofile.txt");
            GralMessage.Waitprogressbar wait = new GralMessage.Waitprogressbar("Reading GRAL topography");

            if (File.Exists(file))
            {
                {
                    try
                    {
                        string[] data;
                        string[] header = new string[6];

                        // read  the header
                        using (StreamReader myReader = new StreamReader(file))
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                header[i] = myReader.ReadLine();
                            }
                        }

                        data = header[0].Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        int nx = Convert.ToInt32(data[1]);
                        data = header[1].Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        int ny = Convert.ToInt32(data[1]);

                        if (ny > 0 && nx > 0 && nx <= CellHeights.GetUpperBound(0) && ny <= CellHeights.GetUpperBound(1))
                        {
                            wait.Show();

                            File.Delete(file);
                            System.Text.StringBuilder SB = new System.Text.StringBuilder();
                            using (StreamWriter myWriter = new StreamWriter(file))
                            {
                                for (int i = 0; i < 6; i++)
                                {
                                    myWriter.WriteLine(header[i]);
                                }

                                for (int i = ny; i > 0; i--)
                                {
                                    if (i % 40 == 0)
                                    {
                                        wait.Text = "Writing GRAL topography " + ((int)(100 - (float)i / (ny + 2) * 100F)).ToString() + "%";
                                        cts.ThrowIfCancellationRequested();
                                    }

                                    //string line = String.Empty;
                                    SB.Clear();
                                    for (int j = 1; j <= nx; j++)
                                    {
                                        SB.Append(Math.Round(CellHeights[j, i], 1).ToString(ic));
                                        SB.Append(',');

                                        //line += Math.Round(CellHeights[j, i], 1).ToString(ic) + ",";
                                    }
                                    myWriter.WriteLine(SB.ToString());
                                    Application.DoEvents();
                                }
                            }
                            SB.Clear();
                            SB = null;
                            ok = true;
                        }
                    }
                    catch
                    {
                        ok = false;
                    }
                }

            }
            wait.Close();
            wait.Dispose();
            return ok;
        }

        /// <summary>
        /// Import the original GRAL Topography from ESRI-ASCII file
        /// </summary>
        void OriginalGRALTopographyToolStripMenuItemClick(object sender, EventArgs e)
        {
            GralIO.CreateGralTopography Topo = new GralIO.CreateGralTopography();
            if (Topo.GetFilename()) // get filename
            {
                if (Topo.CreateTopography(MainForm) == true) // Creation successful
                {
                    if (ReadGralGeometry()) // GRAL geometry is available
                    {
                        SetCellHeightsType(2);
                        MainForm.checkBox25.Checked = true;
                        MainForm.checkBox25.Enabled = true;

                        bool windfieldfiles = WindfieldsAvailable();

                        // check, if Topography-Modification is allowed
                        if (windfieldfiles == false && Gral.Main.Project_Locked == false)
                        {
                            modifyTopographyToolStripMenuItem.Enabled = true;
                            saveTopographyToolStripMenuItem.Enabled = true;
                            restoreGRALTopographyToolStripMenuItem.Enabled = true;
                            lowPassGRALTopographyToolStripMenuItem.Enabled = true;
                        }
                        MessageBoxTemporary Box = new MessageBoxTemporary("Topography data import successful", Location);
                        Box.Show();
                    }
                }
                else
                {
                    if (CancellationTokenSource != null && CancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                    MessageBox.Show(this, "Unable to write file 'GRAL_topofile.txt'", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        /// <summary>
        /// Change icon & Title if the project is locked
        /// </summary>
        void DomainActivated(object sender, EventArgs e)
        {
            if (GRAMM_Locked != MainForm.GRAMM_Locked || GRAL_Locked != Gral.Main.Project_Locked)
            {
                GRAL_Locked = Gral.Main.Project_Locked;
                GRAMM_Locked = MainForm.GRAMM_Locked;
                if (GRAMMOnline == true)
                {
                }
                else
                {
                    if (Gral.Main.Project_Locked == true)
                    {
                        Bitmap myIcon = Gral.Properties.Resources.Locked;
                        myIcon.MakeTransparent(Color.White);
                        IntPtr Hicon = myIcon.GetHicon();
                        Icon newIcon = Icon.FromHandle(Hicon);
                        Icon = newIcon;
                        newIcon.Dispose();
                        myIcon.Dispose();
                        if (MainForm.GRAMM_Locked)
                        {
                            Text = "Domain / " + Path.GetFileName(Gral.Main.ProjectName) + "   (GRAL locked / GRAMM locked)";
                        }
                        else
                        {
                            Text = "Domain / " + Path.GetFileName(Gral.Main.ProjectName) + "   (GRAL locked)";
                        }
                    }
                    else
                    {
                        Bitmap myIcon = Gral.Properties.Resources.Unlocked;
                        myIcon.MakeTransparent(Color.White);
                        IntPtr Hicon = myIcon.GetHicon();
                        Icon newIcon = Icon.FromHandle(Hicon);
                        Icon = newIcon;
                        newIcon.Dispose();
                        myIcon.Dispose();
                        if (MainForm.GRAMM_Locked)
                        {
                            Text = "Domain / " + Path.GetFileName(Gral.Main.ProjectName) + "   (GRAMM locked)";
                        }
                        else
                        {
                            Text = "Domain / " + Path.GetFileName(Gral.Main.ProjectName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Modify the GRAL topography
        /// </summary>
        void ModifyTopographyToolStripMenuItemClick(object sender, EventArgs e)
        {
            // restore Modify array
            if (TopoModifyBlocked != null &&
                TopoModifyBlocked.GetUpperBound(0) == CellHeights.GetUpperBound(0) &&
                TopoModifyBlocked.GetUpperBound(1) == CellHeights.GetUpperBound(1))
            {
                Array.Clear(TopoModifyBlocked, 0, TopoModifyBlocked.Length);
            }
            else
            {
                TopoModifyBlocked = new bool[CellHeights.GetUpperBound(0) + 1, CellHeights.GetUpperBound(1) + 1];
            }

            // Go to the dialog
            using (DialogModifyGRALTopography mod = new DialogModifyGRALTopography
            {
                modify = TopoModify,
                StartPosition = FormStartPosition.Manual
            })
            {
                mod.Location = new Point(St_F.GetScreenAtMousePosition() + 200, St_F.GetTopScreenAtMousePosition() + 150);
                if (mod.ShowDialog() == DialogResult.OK)
                {
                    TopoModify = mod.modify;
                }
            }

            // 4th: set the mousecontrol flag
            MouseControl = MouseMode.GRALTopographyModify;
        }

        /// <summary>
        /// Restore an unsafed GRAL topography to its original values
        /// </summary>
        void RestoreGRALTopographyToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Restore the GRAL topography?", "GRAL GUI", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                string file = Path.Combine(Gral.Main.ProjectName, "Computation", "GRAL_topofile.txt");
                if (File.Exists(file))
                {
                    ReadGralGeometry(); // read actual saved geometry file
                }
                else
                {
                    file = Path.Combine(Gral.Main.ProjectName, "Computation", "GRAL_topofile_Orig.txt");
                    if (File.Exists(file))
                    {
                        string filecopy = Path.Combine(Gral.Main.ProjectName, "Computation", "GRAL_topofile.txt");
                        try
                        {
                            File.Delete(filecopy);
                            File.Copy(file, filecopy);
                            ReadGralGeometry(); // read restored geometry file
                        }
                        catch
                        { }
                    }
                }
            }
        }

        /// <summary>
        /// Low pass to the GRAL topography
        /// </summary>
        async void LowPassGRALTopographyToolStripMenuItemClick(object sender, EventArgs e)
        {

            if (MessageBox.Show(this, "Apply low pass filter to the GRAL topography?", "GRAL GUI", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                await System.Threading.Tasks.Task.Run(() => LowPassGralTopographyApply());
                MessageBoxTemporary Box = new MessageBoxTemporary("Low pass filter applied", Location);
                Box.Show();
            }
        }

        /// <summary>
        /// Low pass filter for the GRAL topography
        /// </summary>
        void LowPassGralTopographyApply()
        {
            // create deep copy of cell height
            float[,] filter = new float[CellHeights.GetUpperBound(0), CellHeights.GetUpperBound(1)];

            for (int x = 0; x < CellHeights.GetUpperBound(0); x++)
            {
                for (int y = 0; y < CellHeights.GetUpperBound(1); y++)
                {
                    filter[x, y] = CellHeights[x, y];
                }
            }

            // filter
            for (int x = 3; x < CellHeights.GetUpperBound(0) - 3; x++)
            {
                for (int y = 3; y < CellHeights.GetUpperBound(1) - 3; y++)
                {
                    CellHeights[x, y] = (float)(0.06321 * (0.354 * filter[x - 2, y + 2] + 0.354 * filter[x - 2, y - 2] + 0.354 * filter[x + 2, y + 2] + 0.354 * filter[x + 2, y - 2] +
                                                            0.447 * filter[x - 2, y + 1] + 0.447 * filter[x - 2, y - 1] + 0.447 * filter[x + 2, y + 1] + 0.447 * filter[x + 2, y - 1] +
                                                            0.447 * filter[x - 1, y + 2] + 0.447 * filter[x - 1, y - 2] + 0.447 * filter[x + 1, y + 2] + 0.447 * filter[x + 1, y - 2] +
                                                            0.5 * filter[x, y + 2] + 0.5 * filter[x, y - 2] + 0.5 * filter[x + 2, y] + 0.5 * filter[x - 2, y] +
                                                            filter[x - 1, y] + filter[x + 1, y] + filter[x, y + 1] + filter[x, y - 1] +
                                                            0.707 * filter[x - 1, y + 1] + 0.707 * filter[x + 1, y + 1] + 0.707 * filter[x - 1, y - 1] + 0.707 * filter[x + 1, y - 1] +
                                                            2 * filter[x, y]));
                }
            }
        }

        /// <summary>
        /// Save the GRAL microscale topography
        /// </summary>
        void SaveTopographyToolStripMenuItemClick(object sender, EventArgs e)
        {
            // 1st: copy the original topography
            string file = Path.Combine(Gral.Main.ProjectName, "Computation", "GRAL_topofile.txt");
            string filecopy = Path.Combine(Gral.Main.ProjectName, "Computation", "GRAL_topofile_Orig.txt");
            if (File.Exists(file))
            {
                try
                {
                    File.Copy(file, filecopy);
                }
                catch
                { }
            }

            if (MessageBox.Show(this, "Write new GRAL_topofile.txt?", "GRAL GUI", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                CancellationTokenReset();
                if (WriteGralGeometry(CancellationTokenSource.Token) == false)
                {
                    File.Copy(filecopy, file);
                    MessageBoxTemporary Box = new MessageBoxTemporary("Error when saving GRAL geometry", Location);
                }
                else
                {
                    MessageBoxTemporary Box = new MessageBoxTemporary("GRAL geometry saved", Location);
                }
            }
        }

        /// <summary>
        /// Search the left or right postition of the recent screen
        /// </summary>
        /// <param name="Mode">0 = relative, 1 = absolute</param>
        /// <returns></returns>
        private System.Drawing.Point GetScreenPositionForNewDialog(int Mode)
        {
            int x = GralStaticFunctions.St_F.GetScreenAtMousePosition(); // get screen
            int y = 60;

            if (Mode == 2)
            {
                y = GralStaticFunctions.St_F.GetTopScreenAtMousePosition() + 150;
            }

            if (panel1.Dock == DockStyle.Left) // Panel on the right side
            {
                if (Mode == 0)
                {
                    x = 200;
                }
                x += 150;
            }
            else
            {
                if (Mode == 0)
                {
                    x = this.Width - 450;
                }
                else
                {
                    x += panel1.Left - 350;
                }
            }
            if (Mode == 2)
            {
                x = St_F.GetScreenAtMousePosition() + 250;
            }
            x = Math.Max(10, x);
            return new System.Drawing.Point(x, y);
        }

        /// <summary>
        /// Show GRAMM terrain grid heights
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuCellHeightsGramm(object sender, EventArgs e)
        {
            if (!MenuEntryCellHeightsGramm.Checked)
            {
                SetCellHeightsType(1);
                if (!TryToLoadCellHeights())
                {
                    SetCellHeightsType(0);
                }
            }
        }

        /// <summary>
        /// Show GRAMM terrain edge point heights
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuEntryCellHeightsGrammEdge_Click(object sender, EventArgs e)
        {
            if (!MenuEntryCellHeightsGrammEdge.Checked)
            {
                SetCellHeightsType(-1);
                if (!TryToLoadCellHeights())
                {
                    SetCellHeightsType(0);
                }
            }
        }

        /// <summary>
        /// Show GRAL terrain grid heights
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuCellHeightsGral(object sender, EventArgs e)
        {
            if (!MenuEntryCellHeightsGral.Checked)
            {

                SetCellHeightsType(2);
                if (!TryToLoadCellHeights())
                {
                    SetCellHeightsType(0);
                }
            }
        }

        /// <summary>
        /// Set the type of cell height to be displayed
        /// </summary>
        /// <param name="type">0: No cell height, 1: GRAMM, 2: GRAL, -1: GRAMM edge points</param>
        private void SetCellHeightsType(int type)
        {
            CellHeightsType = type;
            if (CellHeightsType == 0)
            {
                MenuEntryCellHeightsGramm.Checked = false;
                MenuEntryCellHeightsGral.Checked = false;
                MenuEntryCellHeightsGrammEdge.Checked = false;
            }
            else if (CellHeightsType == 1)
            {
                MenuEntryCellHeightsGramm.Checked = true;
                MenuEntryCellHeightsGral.Checked = false;
                MenuEntryCellHeightsGrammEdge.Checked = false;
            }
            else if (CellHeightsType == 2)
            {
                MenuEntryCellHeightsGramm.Checked = false;
                MenuEntryCellHeightsGral.Checked = true;
                MenuEntryCellHeightsGrammEdge.Checked = false;
            }
            else if (CellHeightsType == -1)
            {
                MenuEntryCellHeightsGramm.Checked = false;
                MenuEntryCellHeightsGral.Checked = false;
                MenuEntryCellHeightsGrammEdge.Checked = true;
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            ShowPointSourceDialog(sender, e);
        }
        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            ShowAreaSourceDialog(sender, e);
        }
        private void checkBox15_CheckedChanged(object sender, EventArgs e)
        {
            ShowBuildingDialog(sender, e);
        }
        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            ShowLineSourceDialog(sender, e);
        }
        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            ShowPortalSourceDialog(sender, e);
        }
        private void checkBox20_CheckedChanged(object sender, EventArgs e)
        {
            ShowReceptorDialog(sender, e);
        }
        private void checkBox25_CheckedChanged(object sender, EventArgs e)
        {
            ShowWallDialog(sender, e);
        }
        private void checkBox26_CheckedChanged(object sender, EventArgs e)
        {
            ShowVegetationDialog(sender, e);
        }

        /// <summary>
        /// Generate a concentration time series based on *.con or *.grz files for several evaluation points 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void generateTimeSeriesForSeveralEvaluationPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MouseControl = MouseMode.SetPointConcTimeSeries;
            Cursor = Cursors.Cross;

            GralDomForms.SelectMultiplePoints _selmp = new SelectMultiplePoints();
            SendCoors += _selmp.ReceiveClickedCoordinates;
            _selmp.CancelComputation += CancelMetTimeSeries;
            _selmp.StartComputation += ConcentrationTimeSeries;

            _selmp.MeteoModel = 0;
            _selmp.StartPosition = FormStartPosition.Manual;
            _selmp.Location = new Point(GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160, St_F.GetTopScreenAtMousePosition() + 150);
            _selmp.Owner = this;
            _selmp.MeteoInitFileName = Path.Combine(Gral.Main.ProjectName, "TimeSeries.txt");
            _selmp.Show();
        }

        /// <summary>
        /// Start the evaluation of Time series
        /// </summary>
        private void ConcentrationTimeSeries(object sender, EventArgs e)
        {
            List<GralBackgroundworkers.Point_3D> receptor_points = new List<GralBackgroundworkers.Point_3D>();
            string _prefix = string.Empty;
            int _timeSeriesYear = 2020;

            // Release send coors
            if (sender is SelectMultiplePoints _sl)
            {
                SendCoors -= _sl.ReceiveClickedCoordinates;

                foreach (System.Data.DataRow row in _sl.PointCoorData.Rows)
                {
                    if (row[0] != DBNull.Value && row[1] != DBNull.Value && row[2] != DBNull.Value && row[3] != DBNull.Value)
                    {
                        string a = Convert.ToString(row[0]);

                        GralBackgroundworkers.Point_3D item = new GralBackgroundworkers.Point_3D
                        {
                            FileName = a,
                            X = Convert.ToDouble(row[1]),
                            Y = Convert.ToDouble(row[2]),
                            Z = Convert.ToDouble(row[3])
                        };
                        receptor_points.Add(item);
                    }
                }
                _prefix = _sl.MeteoInitFileName;
                _timeSeriesYear = _sl.TimeSeriesYear;
                _sl.Close();
            }

            if (receptor_points.Count == 0)
            {
                MessageBox.Show(this, "No points defined", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else // start the evaluation
            {
                string _selSourceGrp = string.Empty;
                foreach (ListViewItem itm in MainForm.listView1.Items)
                {
                    _selSourceGrp = _selSourceGrp + itm.Text + ","; // name of selected source groups
                }
                _selSourceGrp = _selSourceGrp.TrimEnd(new char[] { ',' }); // remove last ','

                GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData
                {
                    ProjectName = Gral.Main.ProjectName,
                    DecSep = decsep,
                    UserText = "The process may take some minutes. The data will be saved in the folder Computation",
                    Caption = "Evaluate concentration time series", // + DataCollection.Meteofilename;
                    BackgroundWorkerFunction = GralBackgroundworkers.BWMode.EvalPointsTimeSeries, // ; 38 =  Evaluation points concentration
                    EvalPoints = receptor_points, // evaluation points
                    GFFGridSize = MainForm.HorGridSize,
                    Horgridsize = MainForm.HorGridSize,
                    DomainWest = MainForm.GralDomRect.West,
                    DomainSouth = MainForm.GralDomRect.South,
                    CellsGralX = MainForm.CellsGralX,
                    CellsGralY = MainForm.CellsGralY,
                    Slice = MainForm.GRALSettings.NumHorSlices,
                    SliceHeights = MainForm.GRALSettings.HorSlices,
                    SelectedSourceGroup = _selSourceGrp,
                    MaxSource = MainForm.listView1.Items.Count,
                    Prefix = _prefix,
                    MeteoNotClassified = MainForm.checkBox19.Checked,
                    PathEmissionModulation = Gral.Main.ProjectSetting.EmissionModulationPath,
                    FictiousYear = _timeSeriesYear
                };

                GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                {
                    Text = DataCollection.Caption
                };
                BackgroundStart.Show();
            }
            // now the backgroundworker works
            // gen_meteofile(Convert.ToDouble(trans), GRAMMmeteofile);
            // Reset mousecontrol
            MouseControl = MouseMode.Default;
        }

        /// <summary>
        /// Reset the Cancel Token -> create new token Thread safe (locked)   
        /// </summary>
        public static void CancellationTokenReset()
        {
            lock (LockObject)
            {
                if (CancellationTokenSource == null)
                {
                    GralDomain.Domain.CancellationTokenSource = new System.Threading.CancellationTokenSource();
                }
                else if (CancellationTokenSource.IsCancellationRequested)
                {
                    CancellationTokenSource.Dispose();
                    CancellationTokenSource = null;
                    CancellationTokenSource = new System.Threading.CancellationTokenSource();
                }
            }
        }

        /// <summary>
        /// Resize of the domain form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Domain_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Maximized && WindowState != FormWindowState.Minimized)
            {
                // Reset position of child forms
                ShowFirst.Reset();
            }
        }
    }
}