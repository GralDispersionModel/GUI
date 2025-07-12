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

using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using GralStaticFunctions;
using GralItemData;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Load Contour Maps and Vector Maps at the Start of the Domain Form
        /// </summary>
        public void Domain_Load(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            //load base maps
            bool existbasemap = false;
            try
            {
                int k = -1;
                double xmin = 10000000;
                double ymin = 10000000;
                double xmax = -10000000;
                double ymax = -10000000;
                double fact1 = 1;
                double fact2 = 1;

                DrawingObjects _drobj;

                foreach (DrawingObjects _dr in ItemOptions)
                {
                    k += 1;
                    if ((_dr.Name.Substring(0, 3) == "BM:") || (_dr.Name.Substring(0, 3) == "SM:"))
                    {
                        existbasemap = true;
                        _drobj = _dr;
                        groupBox5.Visible = true;
                        SwitchMenuGeoreference();

                        MapSize.SizeX = _dr.PixelMx;
                        MapSize.SizeY = -_dr.PixelMx;
                        MapSize.West = _dr.West;
                        MapSize.North = _dr.North;
                        //compute destination rectangle
                        xmin = Math.Min(xmin, _dr.West);
                        xmax = Math.Max(xmax, _dr.West + _dr.PixelMx * _dr.Picture.Width);
                        ymax = Math.Max(ymax, _dr.North);
                        ymin = Math.Min(ymin, _dr.North - _dr.PixelMx * _dr.Picture.Height);
                    }

                    if (_dr.Name.Equals("WINDROSE"))
                    {
                        {
                            LoadWindroseData(_dr);
                        }
                    }
                }

                if (existbasemap == true)
                {
                    fact1 = Convert.ToDouble(picturebox1.Width) / (xmax - xmin) * MapSize.SizeX;
                    fact2 = Convert.ToDouble(picturebox1.Height) / (ymax - ymin) * MapSize.SizeX;
                    XFac = Math.Min(fact1, fact2);
                    BmpScale = 1 / XFac;

                    TransformX = Convert.ToInt32(-(xmin - MapSize.West) / BmpScale / MapSize.SizeX);
                    TransformY = Convert.ToInt32(-(ymax - MapSize.North) / BmpScale / MapSize.SizeY);

                    //set source - and destination rectangle
                    foreach (DrawingObjects _dr in ItemOptions)
                    {
                        _dr.DestRec = new Rectangle(TransformX + Convert.ToInt32((_dr.West - MapSize.West) / BmpScale / MapSize.SizeX),
                                                    TransformY - Convert.ToInt32((_dr.North - MapSize.North) / BmpScale / MapSize.SizeX),
                                                    Convert.ToInt32(_dr.Picture.Width * _dr.PixelMx / MapSize.SizeX * XFac),
                                                    Convert.ToInt32(_dr.Picture.Height * _dr.PixelMx / MapSize.SizeX * XFac));
                        _dr.SourceRec = new Rectangle(0, 0, _dr.Picture.Width, _dr.Picture.Height);
                    }
                }

                if (existbasemap == false)
                {
                    //add base map to object list
                    _drobj = new DrawingObjects("BM: " + Path.GetFileNameWithoutExtension(MapFileName));

                    StreamReader SR = new StreamReader(MapFileName);
                    _drobj.Picture.Dispose();
                    _drobj.Picture = new Bitmap(SR.BaseStream);
                    SR.Close();

                    //set source - and destination rectangle
                    double fac1 = Convert.ToDouble(picturebox1.Width) / Convert.ToDouble(_drobj.Picture.Width);
                    double fac2 = Convert.ToDouble(picturebox1.Height) / Convert.ToDouble(_drobj.Picture.Height);
                    XFac = Math.Min(fac1, fac2);
                    BmpScale = 1 / XFac;


                    _drobj.Label = 0;
                    _drobj.ContourFilename = MapFileName;
                    _drobj.DestRec = new Rectangle(0, 0, Convert.ToInt32(_drobj.Picture.Width * XFac), Convert.ToInt32(_drobj.Picture.Height * XFac));
                    _drobj.SourceRec = new Rectangle(0, 0, _drobj.Picture.Width, _drobj.Picture.Height);
                    _drobj.PixelMx = MapSize.SizeX;
                    _drobj.West = MapSize.West;
                    _drobj.North = MapSize.North;
                    ItemOptions.Insert(0, _drobj);
                    SaveDomainSettings(1);
                }

                //add unreferenced image file
                if (existbasemap == true)
                {
                    if (!File.Exists(MapFileName))
                    {
                        // check if file exists in a sub folder of this project
                        (string tempfilename, bool saveNewFilePath) = St_F.SearchAbsoluteAndRelativeFilePath(Gral.Main.ProjectName, MapFileName, "Maps");
                        if (File.Exists(tempfilename))
                        {
                            MapFileName = tempfilename;
                        }
                    }

                    //add base map to object list
                    _drobj = new DrawingObjects("BM: " + Path.GetFileNameWithoutExtension(MapFileName));

                    using (StreamReader SR = new StreamReader(MapFileName))
                    {
                        _drobj.Picture.Dispose();
                        _drobj.Picture = new Bitmap(SR.BaseStream);
                    }

                    //compute artificial coordinates, such that image appears with full extent at screen
                    _drobj.West = Convert.ToInt32((-TransformX) * MapSize.SizeX * BmpScale + MapSize.West);
                    _drobj.North = Convert.ToInt32((TransformY) * MapSize.SizeX * BmpScale + MapSize.North);
                    double dummy1 = Convert.ToDouble(picturebox1.Width) / Convert.ToDouble(_drobj.Picture.Width) * MapSize.SizeX / XFac;
                    double dummy2 = Convert.ToDouble(picturebox1.Height) / Convert.ToDouble(_drobj.Picture.Height) * MapSize.SizeX / XFac;
                    _drobj.PixelMx = Math.Min(dummy1, dummy2);


                    _drobj.Label = 0;
                    _drobj.ContourFilename = MapFileName;
                    _drobj.DestRec = new Rectangle(TransformX + Convert.ToInt32((_drobj.West - MapSize.West) /
                                                                                  BmpScale / MapSize.SizeX), TransformY -
                                                     Convert.ToInt32((_drobj.North - MapSize.North) / BmpScale / MapSize.SizeX),
                                                     Convert.ToInt32(_drobj.Picture.Width * _drobj.PixelMx / MapSize.SizeX * XFac),
                                                     Convert.ToInt32(_drobj.Picture.Height * _drobj.PixelMx / MapSize.SizeX * XFac));

                    _drobj.SourceRec = new Rectangle(0, 0, _drobj.Picture.Width, _drobj.Picture.Height);

                    ItemOptions.Insert(0, _drobj);
                    SaveDomainSettings(1);
                }
            }
            catch
            { }

            //if GRAMM.geb is imported from existing file, it must be loaded here for the first time
            //add GRAMM domain to object list, when the file GRAMM.geb exists
            if (MainForm.EmifileReset == true)
            {
                if (MainForm.textBox15.Text != "")
                {
                    CheckForExistingDrawingObject("GRAMM DOMAIN");
                }
                else
                {
                    //remove GRAMM Domain if no Domain is defined anymore
                    RemoveItemFromItemOptions("GRAMM DOMAIN");
                }
            }

            //if GRAL.geb is imported from existing file, it must be loaded here for the first time
            //add GRAL domain to object list, when the file GRAL.geb exists
            if (MainForm.textBox2.Text != "")
            {
                CheckForExistingDrawingObject("GRAL DOMAIN");
            }
            else
            {
                //remove GRAL Domain if no Domain is defined anymore
                RemoveItemFromItemOptions("GRAL DOMAIN");
            }


            //Other initialisations
            if (MainForm.GralDomRect.East - MainForm.GralDomRect.West != 0)
            {
                groupBox5.Visible = true;
            }

            EditPS.ItemDisplayNr = 0;
            EditAS.ItemDisplayNr = 0;
            EditLS.ItemDisplayNr = 0;
            EditPortals.ItemDisplayNr = 0;
            EditB.ItemDisplayNr = 0;
            EditWall.ItemDisplayNr = 0;

            //import source group definitions to source dialogs
            LoadSourceGroups();

            // import point sources
            EditPS.ReloadItemData();
            if (EditPS.ItemData.Count > 0)
            {
                button8.Visible = true;
                button38.Visible = true;
            }
            // import receptors
            EditR.ResetItemData();
            if (EditR.ItemData.Count > 0)
            {
                button23.Visible = true;
                button39.Visible = true;
            }
            // import area sources
            EditAS.ReloadItemData();
            if (EditAS.ItemData.Count > 0)
            {
                button10.Visible = true;
                button37.Visible = true;
            }
            // import line sources
            EditLS.ReloadItemData();
            if (EditLS.ItemData.Count > 0)
            {
                button12.Visible = true;
                button36.Visible = true;
            }
            // import walls
            EditWall.ReloadItemData();
            if (EditWall.ItemData.Count > 0)
            {
                button49.Visible = true;
            }
            // import portal sources
            EditPortals.ReloadItemData();
            if (EditPortals.ItemData.Count > 0)
            {
                button14.Visible = true;
            }
            // import buildings
            EditB.ReloadItemData();
            if (EditB.ItemData.Count > 0)
            {
                button16.Visible = true;
                button58.Visible = true;
            }
            // import Vegetation
            EditVegetation.ReloadItemData();
            if (EditVegetation.ItemData.Count > 0)
            {
                button50.Visible = true;
            }

            //enable source apportionment, if GRAL results are available
            try
            {
                string files = Path.Combine(Gral.Main.ProjectName, "Maps" + Path.DirectorySeparatorChar);
                DirectoryInfo di = new DirectoryInfo(files);
                FileInfo[] files_conc = di.GetFiles("Mean*.txt");
                if (files_conc.Length > 1)
                {
                    button33.Visible = true;
                }
            }
            catch
            {
            }

            //enable concentration extraction, if GRAL results are available
            try
            {
                string files = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar);
                DirectoryInfo di = new DirectoryInfo(files);
                FileInfo[] files_conc = di.GetFiles("*.txt");
                if (files_conc.Length > 1)
                {
                    button35.Visible = true;
                }
            }
            catch
            {
            }

            //enable NEMO line source emission computation when line-sources are defined
            if (EditLS.ItemData.Count != 0)
            {
                MainForm.button13.Visible = true;
            }
            else
            {
                MainForm.button13.Visible = false;
            }


            // IF transient 3D result is available
            if (File.Exists(Path.Combine(Gral.Main.ProjectName, "Computation", "Vertical_Concentrations.txt")))
            {
                groupBox7.Visible = true;
                verticalConcentrationProfileToolStripMenuItem.Enabled = true;
                verticalConcentrationProfileSectionToolStripMenuItem.Enabled = true;
            }
            else
            {
                groupBox7.Visible = false;
                verticalConcentrationProfileToolStripMenuItem.Enabled = false;
                verticalConcentrationProfileSectionToolStripMenuItem.Enabled = false;
            }

            //GRAMM Online options
            if (GRAMMOnline == true)
            {
                Domain_Load_GRAMMOnline();
            }

            ViewFrameMenuBarLoad(); // Load View-Frame Values
            Cursor = Cursors.Default;

            if (Gral.Main.Project_Locked == true)
            {
                Bitmap myIcon = Gral.Properties.Resources.Locked;
                myIcon.MakeTransparent(Color.White);
                IntPtr Hicon = myIcon.GetHicon();
                Icon newIcon = Icon.FromHandle(Hicon);

                Icon = newIcon;
                newIcon.Dispose();
                myIcon.Dispose();
                //domain_lock_button.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.lock_closed));
                //toolTip1.SetToolTip(domain_lock_button, "GRAL project is locked");
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
                //domain_lock_button.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.lock_open));
                //toolTip1.SetToolTip(domain_lock_button, "GRAL project is unlocked");
            }
            //enable support for high contrast themes
            if (System.Windows.SystemParameters.HighContrast || Gral.Main.GUISettings.UseDefaultColors)
            {
                Gral.Main.LoopAllControls(this.Controls);
            }

            Picturebox1_Paint();
        }

        public void LoadSourceGroups()
        {
            // import source groups and fill source dialogs 
            for (int i = 1; i < 100; i++)
            {
                EditLS.SG_List.Add(i.ToString());
                EditPortals.SG_List.Add(i.ToString());
            }

            try
            {
                //import source group definitions
                string newPath = Path.Combine(Gral.Main.ProjectName, @"Settings", "Sourcegroups.txt");

                using (StreamReader myReader = new StreamReader(newPath))
                {
                    string[] text = new string[2];
                    string text1;
                    EditAS.comboBox1.Items.Clear();
                    EditPS.comboBox1.Items.Clear();

                    while (myReader.EndOfStream == false)
                    {
                        text1 = myReader.ReadLine();
                        text = text1.Split(new char[] { ',' });
                        EditAS.comboBox1.Items.Add(text1);
                        EditPS.comboBox1.Items.Add(text1);
                        EditLS.SG_List[St_F.GetSgNumber(text1) - 1] = text1;
                        EditPortals.SG_List[St_F.GetSgNumber(text1) - 1] = text1;
                    }
                }
            }
            catch
            {
            }

            //fill comboboxes to define source groups for the different sources
            int comboboxitems = EditPS.comboBox1.Items.Count;
            for (int i = 1; i < 100; i++)
            {
                string[] text = new string[2];
                string text3;
                int sg = 0;
                for (int k = 0; k < comboboxitems; k++)
                {
                    text3 = Convert.ToString(EditPS.comboBox1.Items[k]);
                    text = text3.Split(new char[] { ',' });
                    try
                    {
                        if (i == Convert.ToInt32(text[1]))
                        {
                            sg = i;
                            break;
                        }
                    }
                    catch
                    {
                    }
                }
                if (sg == 0)
                {
                    EditAS.comboBox1.Items.Add(Convert.ToString(i));
                    EditPS.comboBox1.Items.Add(Convert.ToString(i));
                }
            }

        }
    }
}