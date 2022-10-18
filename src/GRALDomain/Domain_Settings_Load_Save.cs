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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using GralIO;
using GralStaticFunctions;
using GralItemData;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Save all properties (colors, transparancy, fonts) of all items to Settings1.txt and Settings2.txt
        /// </summary>
        public void SaveDomainSettings(int mode)
            // mode = 0: do not update object-manager
        {
            if (GRAMMOnline == false) // do not save settings, if domain is opened online
            {
                TypeConverter tc = TypeDescriptor.GetConverter(typeof(Font));
                TypeConverter col = TypeDescriptor.GetConverter(typeof(Color));
                TypeConverter bol = TypeDescriptor.GetConverter(typeof(bool));
                TypeConverter rec = TypeDescriptor.GetConverter(typeof(Rectangle));
                
                SaveSettings2("Settings2.ini", 1); // write compatible settings file
                SaveSettings2("Settings1.ini", 2); // write new setting file
            }
            if (mode > 0)
            {
                ObjectmanagerUpdateListbox(); // update objectmanager
            }
        }

        
        public void SaveSettings2(string file, int version)
        {
            if (GRAMMOnline == false) // do not save settings, if domain is opened online
            {
                TypeConverter tc = TypeDescriptor.GetConverter(typeof(Font));
                TypeConverter col = TypeDescriptor.GetConverter(typeof(Color));
                TypeConverter bol = TypeDescriptor.GetConverter(typeof(bool));
                TypeConverter rec = TypeDescriptor.GetConverter(typeof(Rectangle));
                
                string newPath = Path.Combine(Gral.Main.ProjectName, @"Settings", file);
                
                try
                {
                    using (StreamWriter myWriter = new StreamWriter(newPath))
                    {
                        if (version == 1)
                        {
                            myWriter.WriteLine("Settings for displaying V16");
                        }
                        else if (version == 2)
                        {
                            myWriter.WriteLine("Settings for displaying V20.01");
                        }

                        myWriter.WriteLine(Convert.ToString(ItemOptions.Count));
                        myWriter.WriteLine(Convert.ToString(NorthArrow.Scale));
                        myWriter.WriteLine(Convert.ToString(NorthArrow.X, ic));
                        myWriter.WriteLine(Convert.ToString(NorthArrow.Y));
                        myWriter.WriteLine(Convert.ToString(MapScale.X));
                        myWriter.WriteLine(Convert.ToString(MapScale.Y));
                        myWriter.WriteLine(Convert.ToString(MapScale.Division));
                        
                        foreach(DrawingObjects _drobj in ItemOptions)
                        {
                            if (version == 1 && _drobj.Name.Equals("WINDROSE")) // don't write windrose item in version 16
                            {
                            }
                            else
                            {
                                try
                                {
                                    myWriter.WriteLine("________________________________________________________________");
                                    myWriter.WriteLine(_drobj.Name);
                                    myWriter.WriteLine(_drobj.Label.ToString(ic));
                                    myWriter.WriteLine(bol.ConvertToString(_drobj.Show));
                                    myWriter.WriteLine(col.ConvertToString(_drobj.LineColor));
                                    myWriter.WriteLine(Convert.ToString(_drobj.LineWidth, ic));
                                    myWriter.WriteLine(_drobj.VectorScale.ToString(ic));
                                    myWriter.WriteLine(Convert.ToString(_drobj.DecimalPlaces));
                                    myWriter.WriteLine(col.ConvertToString(_drobj.LabelColor));
                                    myWriter.WriteLine(Convert.ToString(_drobj.Transparancy));
                                    myWriter.WriteLine(Convert.ToString(_drobj.ContourLabelDist));
                                    
                                    if(version > 1)
                                    {
                                        myWriter.WriteLine(bol.ConvertToString(_drobj.DrawSimpleContour));
                                        myWriter.WriteLine(bol.ConvertToString(_drobj.ContourDrawBorder));
                                        myWriter.WriteLine(_drobj.ContourFilter.ToString(ic));
                                        myWriter.WriteLine(_drobj.ContourTension.ToString(ic));
                                        myWriter.WriteLine(bol.ConvertToString(_drobj.ScaleLabelFont));
                                        myWriter.WriteLine(_drobj.RemoveSpikes.ToString(ic));
                                    }
                                    
                                    myWriter.WriteLine(tc.ConvertToInvariantString(_drobj.LabelFont));
                                    
                                    myWriter.WriteLine(bol.ConvertToString(_drobj.Fill));
                                    myWriter.WriteLine(bol.ConvertToString(_drobj.FillYesNo));
                                    
                                    myWriter.WriteLine("R"); // R = reserved for future use
                                    myWriter.WriteLine("R");
                                    myWriter.WriteLine("R");
                                    myWriter.WriteLine(Convert.ToString(_drobj.LabelInterval));
                                    myWriter.WriteLine(Convert.ToString(_drobj.ContourAreaMin));
                                    
                                    myWriter.WriteLine(_drobj.Item);
                                    myWriter.WriteLine(_drobj.SourceGroup);
                                    myWriter.WriteLine(_drobj.ColorScale);
                                    myWriter.WriteLine(_drobj.LegendTitle.Replace("\n", "@__NL").Replace("\r", "@__NR"));
                                    myWriter.WriteLine(_drobj.LegendUnit);
                                    myWriter.WriteLine(_drobj.ContourFilename);
                                    
                                    myWriter.WriteLine(_drobj.SourceRec.Left.ToString() + "\t" +
                                                       _drobj.SourceRec.Top.ToString() +  "\t" +
                                                       _drobj.SourceRec.Width.ToString() +  "\t" +
                                                       _drobj.SourceRec.Height.ToString());
                                    
                                    myWriter.WriteLine(Convert.ToString(_drobj.PixelMx, ic));
                                    myWriter.WriteLine(Convert.ToString(_drobj.West, ic));
                                    myWriter.WriteLine(Convert.ToString(_drobj.North, ic));
                                    myWriter.WriteLine(bol.ConvertToString(_drobj.Filter));
                                    for (int j = 0; j < _drobj.FillColors.Count; j++)
                                    {
                                        myWriter.Write(ColorTranslator.ToHtml(_drobj.FillColors[j]) + "\t");
                                    }

                                    myWriter.WriteLine();
                                    if (_drobj.ItemValues == null)
                                    {
                                        _drobj.ItemValues = new List<double>();
                                        _drobj.ItemValues.Add(0);
                                    }
                                    for (int j = 0; j < _drobj.ItemValues.Count; j++)
                                    {
                                        myWriter.Write((_drobj.ItemValues[j].ToString(ic)) + "\t");
                                    }

                                    myWriter.WriteLine();
                                    if (_drobj.LineColors == null)
                                    {
                                        _drobj.LineColors = new List<Color>();
                                        _drobj.LineColors.Add(Color.Black);
                                    }
                                    for (int j = 0; j < _drobj.LineColors.Count; j++)
                                    {
                                        myWriter.Write(ColorTranslator.ToHtml(_drobj.LineColors[j]) + "\t");
                                    }

                                    myWriter.WriteLine();
                                }
                                catch
                                {
                                    MessageBox.Show(this, "Error ");
                                }
                            }
                        }
                    }
                    
                }
                catch {MessageBox.Show("Error writing Settings.ini"); }
            }
        }

        /// <summary>
        /// Read all properties (colors, transparancy, fonts) of all items
        /// </summary>
        private void LoadDomainSettings()
        {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(Font));
            TypeConverter col = TypeDescriptor.GetConverter(typeof(Color));
            TypeConverter bol = TypeDescriptor.GetConverter(typeof(bool));
            TypeConverter rec = TypeDescriptor.GetConverter(typeof(Rectangle));

            string newPath = Path.Combine(Gral.Main.ProjectName, @"Settings", "Settings1.ini");
            if (File.Exists(newPath))
            {
                if (Load_Settings2("Settings1.ini")) // try to read new settings
                {
                    return;
                }
            }
            
            newPath = Path.Combine(Gral.Main.ProjectName, @"Settings", "Settings2.ini");
            if (File.Exists(newPath))
            {
                if (Load_Settings2("Settings2.ini")) // try to load old settings
                {
                    return;
                }
            }
            
        }
        
        private bool Load_Settings2(string file)
        {
            string newPath = Path.Combine(Gral.Main.ProjectName, @"Settings", file);
            string dummy;
            
            foreach(DrawingObjects _drobj in ItemOptions)
            {
                _drobj.Dispose();
            }
            ItemOptions.Clear();
            ItemOptions.TrimExcess();
            bool OK = true;
            
            try
            {
                using (StreamReader myReader = new StreamReader(newPath))
                {
                    int version = 0;
                    
                    dummy = myReader.ReadLine();
                    if (dummy == "Settings for displaying V16")
                    {
                        version = 1;
                    }

                    if (dummy == "Settings for displaying V18")
                    {
                        version = 1;
                    }

                    if (dummy == "Settings for displaying V20")
                    {
                        version = 2;
                    }

                    if (dummy == "Settings for displaying V20.01")
                    {
                        version = 3;
                    }

                    int count = Convert.ToInt32(myReader.ReadLine());
                    NorthArrow.Scale = Convert.ToDecimal(myReader.ReadLine().Replace(".", decsep));
                    NorthArrow.X = Convert.ToInt32(myReader.ReadLine());
                    NorthArrow.Y = Convert.ToInt32(myReader.ReadLine());
                    MapScale.X = Convert.ToInt32(myReader.ReadLine());
                    MapScale.Y = Convert.ToInt32(myReader.ReadLine());
                    MapScale.Division = Convert.ToInt32(myReader.ReadLine());
                    
                    try
                    {
                        while (myReader.EndOfStream == false)
                        {
                            dummy = myReader.ReadLine();
                            if  (dummy == "________________________________________________________________")
                            {
                                DrawingObjects _drobj = new DrawingObjects(String.Empty);
                                
                                if (ReadSettingsData(myReader, _drobj, version))
                                {
                                    ItemOptions.Add(_drobj);
                                }
                                else // at least one item not readable
                                {
                                    OK = false;
                                }
                            }
                        }
                    }
                    catch
                    {}
                }
                //Save_Settings();
            }
            catch
            {
                //MessageBox.Show(this, "Error reading Settings.ini " + e.ToString() );
                return false;
            }
            return OK;
        }
        
        /// <summary>
        /// Remove an item at position index from the ItemOptions list
        /// </summary>
        private void RemoveItems(int index)
        {
            try
            {
                if (ItemOptions.Count >= index)
                {
                    ItemOptions[index].Dispose();
                    ItemOptions.RemoveAt(index);
                }
            }
            catch
            {}
        }
        
        private bool ReadSettingsData(StreamReader myReader, DrawingObjects _drobj, int version)
        {
            string dummy;
            string[] text = new string[100];
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(Font));
            TypeConverter col = TypeDescriptor.GetConverter(typeof(Color));
            TypeConverter bol = TypeDescriptor.GetConverter(typeof(bool));
            TypeConverter rec = TypeDescriptor.GetConverter(typeof(Rectangle));
            
            try
            {
                _drobj.Name = myReader.ReadLine();
                _drobj.Label = Convert.ToInt32(myReader.ReadLine());
                dummy = myReader.ReadLine();
                _drobj.Show = (bool)bol.ConvertFromString(dummy);
                dummy = myReader.ReadLine();
                _drobj.LineColor = (Color)col.ConvertFromString(dummy);
                _drobj.LineWidth = Convert.ToInt32(myReader.ReadLine());
                _drobj.VectorScale = Convert.ToSingle(myReader.ReadLine(), ic);
                _drobj.DecimalPlaces = Convert.ToInt32(myReader.ReadLine());
                dummy = myReader.ReadLine();
                _drobj.LabelColor = (Color)col.ConvertFromString(dummy);
                _drobj.Transparancy = Convert.ToInt32(myReader.ReadLine());
                _drobj.ContourLabelDist = Convert.ToInt32(myReader.ReadLine());
                
                if (_drobj.Name.Equals("NORTH ARROW"))
                {
                    _drobj.ContourLabelDist = (int) (NorthArrow.Scale * 100);
                }
                
                if (version > 1) // since V 20
                {
                    _drobj.DrawSimpleContour = (bool)bol.ConvertFromString(myReader.ReadLine());
                    _drobj.ContourDrawBorder = (bool)bol.ConvertFromString(myReader.ReadLine());
                    _drobj.ContourFilter = Convert.ToSingle(myReader.ReadLine(), ic);
                    _drobj.ContourTension = Convert.ToSingle(myReader.ReadLine(), ic);
                    _drobj.ScaleLabelFont = (bool)bol.ConvertFromString(myReader.ReadLine());
                    if (version > 2) // since V 20.01
                    {
                        _drobj.RemoveSpikes = Convert.ToInt32(myReader.ReadLine(), ic);
                    }
                }
                
                dummy = myReader.ReadLine();
                _drobj.LabelFont = (Font) tc.ConvertFromInvariantString(dummy);
                dummy = myReader.ReadLine();
                _drobj.Fill = (bool)bol.ConvertFromString(dummy);
                dummy = myReader.ReadLine();
                _drobj.FillYesNo = (bool)bol.ConvertFromString(dummy);
                
                string label_distance = "1";
                if (version > 0) //compatibility to old projects
                {
                    dummy = myReader.ReadLine(); // not used at the moment
                    dummy = myReader.ReadLine();
                    dummy = myReader.ReadLine();
                    label_distance = myReader.ReadLine(); // cotour_label interval
                    int label_d = 1;
                    if (Int32.TryParse(label_distance, out label_d) == false)
                    {
                        label_d = 1;
                    }
                    _drobj.LabelInterval = label_d;
                    
                    dummy = myReader.ReadLine(); // contour_area_min
                    _drobj.ContourAreaMin = Convert.ToInt32(dummy);
                }
                else
                {
                    dummy = "0";
                    _drobj.LabelInterval = 1;
                    _drobj.ContourAreaMin = 0;
                }
                
                _drobj.Item = Convert.ToInt32(myReader.ReadLine());
                _drobj.SourceGroup = Convert.ToInt32(myReader.ReadLine());
                _drobj.ColorScale = myReader.ReadLine();
                _drobj.LegendTitle = myReader.ReadLine().Replace("@__NL", "\n").Replace("@__NR", "\r");
                _drobj.LegendUnit = myReader.ReadLine();
                _drobj.ContourFilename = myReader.ReadLine();
                
                dummy = myReader.ReadLine();
                text = dummy.Split(new char[] { '\t' });
                _drobj.SourceRec = new Rectangle(Convert.ToInt32(text[0]), 
                                            Convert.ToInt32(text[1]),
                                            Convert.ToInt32(text[2]),
                                            Convert.ToInt32(text[3]));
                                            
                _drobj.PixelMx = Convert.ToDouble(Convert.ToString(myReader.ReadLine().Replace(".",decsep)));
                _drobj.West = Convert.ToDouble(Convert.ToString(myReader.ReadLine().Replace(".", decsep)));
                _drobj.North = Convert.ToDouble(Convert.ToString(myReader.ReadLine().Replace(".", decsep)));
                dummy = myReader.ReadLine();
                _drobj.Filter = (bool)bol.ConvertFromString(dummy);
                
                // read fillcolors
                dummy = myReader.ReadLine();
                text = dummy.Split(new char[] { '\t' });
                
                for (int j = 0; j < text.Length; j++)
                {
                    if (text[j] != "")
                    {
                        string _col = text[j];
                        _drobj.FillColors.Add(ColorTranslator.FromHtml(_col));
                    }
                }
                
                dummy = myReader.ReadLine();
                text = dummy.Split(new char[] { '\t' });
                for (int j = 0; j < text.Length; j++)
                {
                    if (text[j] != "")
                    {
                        _drobj.ItemValues.Add(Convert.ToDouble(text[j], ic));
                    }
                }
                
                dummy = myReader.ReadLine();
                text = dummy.Split(new char[] { '\t' });
                for (int j = 0; j < text.Length; j++)
                {
                    if (text[j] != "")
                    {
                        string _col = text[j];
                        _drobj.LineColors.Add(ColorTranslator.FromHtml(_col));
                    }
                }
                
                //load base map
                if (_drobj.Name.Substring(0, 3) == "BM:")
                {
                    try
                    {
                        if (_drobj.ContourFilename == "Blank.gif")
                        {
                            StreamReader SR = new StreamReader(Path.Combine(Gral.Main.ProjectName,@"Maps", _drobj.ContourFilename));
                            _drobj.Picture = new Bitmap(SR.BaseStream);
                            SR.Close();
                        }
                        else
                        {
                            //check if base map file exists
                            if (File.Exists(_drobj.ContourFilename) == false) // check if file exists in the folder "maps"
                            {
                                // check if file exists in a sub folder of this project
                                (string tempfilename, bool saveNewFilePath) = St_F.SearchAbsoluteAndRelativeFilePath(Gral.Main.ProjectName, _drobj.ContourFilename, "Maps");
                                if (File.Exists(tempfilename))
                                {
                                    _drobj.ContourFilename = tempfilename;
                                }
                            }
                            
                            if (File.Exists(_drobj.ContourFilename) == false)
                            {
                                //user can define new location of the file
                                OpenFileDialog dialog = new OpenFileDialog
                                {
                                    Filter = "(*.bmp;*.gif;*.jpg;*.png)|*.bmp;*.gif;*.jpg;*.png",
                                    Title = "Base map " + Convert.ToString(Path.GetFileName(_drobj.ContourFilename)) + " not found - please enter new path",
                                    InitialDirectory = Path.Combine(Gral.Main.ProjectName, "Maps"),
                                    ShowHelp = true
#if NET6_0_OR_GREATER
                                    ,ClientGuid = GralStaticFunctions.St_F.FileDialogMaps
#endif
                                };
                                if (dialog.ShowDialog() == DialogResult.OK)
                                {
                                    _drobj.ContourFilename = dialog.FileName;
                                    using (StreamReader SR = new StreamReader(_drobj.ContourFilename))
                                    {
                                        _drobj.Picture = new Bitmap(SR.BaseStream);
                                    }
                                }
                                else
                                {
                                    _drobj.Picture = new Bitmap(1, 1);
                                }
                                dialog.Dispose();
                            }
                            else
                            {
                                using (StreamReader SR = new StreamReader(_drobj.ContourFilename))
                                {
                                    _drobj.Picture = new Bitmap(SR.BaseStream);
                                }
                            }
                        }
                    }
                    catch
                    {
                        //MessageBox.Show(this, ex.Message + "Error reading Settings.ini " );
                    }
                }
                else
                {
                    _drobj.Picture = new Bitmap(1, 1);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Start 3D view of actual domain area
        /// </summary>
        public void Domain3DView(bool smooth, double vert_fac)
        {
            // Coordinates of the actual screen
            double x0 = Math.Round((0 - TransformX) * BmpScale * MapSize.SizeX + MapSize.West, 1, MidpointRounding.AwayFromZero);
            double y0 = Math.Round((0 - TransformY) * BmpScale * MapSize.SizeY + MapSize.North, 1, MidpointRounding.AwayFromZero);
            double x1 = Math.Round((picturebox1.Width - TransformX) * BmpScale * MapSize.SizeX + MapSize.West, 1, MidpointRounding.AwayFromZero);
            double y1 = Math.Round((picturebox1.Height - TransformY) * BmpScale * MapSize.SizeY + MapSize.North, 1, MidpointRounding.AwayFromZero);
            
            // Index values of the actual screen
            int ix0 = Convert.ToInt32(Math.Floor((x0 - MainForm.GrammDomRect.West)/MainForm.GRAMMHorGridSize)) + 1;
            int iy0 = Convert.ToInt32(Math.Floor((y0 - MainForm.GrammDomRect.South) / MainForm.GRAMMHorGridSize)) + 1;
            int ix1 = Convert.ToInt32(Math.Floor((x1 - MainForm.GrammDomRect.West) /MainForm.GRAMMHorGridSize)) + 1;
            int iy1 = Convert.ToInt32(Math.Floor((y1 - MainForm.GrammDomRect.South) / MainForm.GRAMMHorGridSize))+1;
            
            // Check indices
            try
            {
                if ((ix1 - ix0) > 1 && (iy0 - iy1) > 1 && MainForm.GRAMMwindfield != null)
                {
                    //reading geometry file "ggeom.asc"
                    double[,] AH = new double[1, 1];
                    
                    int NX = 1;
                    int NY = 1;
                    int NZ = 1;
                    string[] text = new string[5];
                    double AH_Min = 10000000; double AH_MAX = 0;

                    GGeomFileIO ggeom = new GGeomFileIO
                    {
                        PathWindfield = Path.GetDirectoryName(MainForm.GRAMMwindfield)
                    };
                    if (ggeom.ReadGGeomAsc(1) == true)
                    {
                        NX = ggeom.NX;
                        NY = ggeom.NY;
                        NZ = ggeom.NZ;
                        AH = ggeom.AH;
                        AH_MAX = ggeom.AHmax;
                        AH_Min = ggeom.AHmin;
                        ggeom = null;
                    }
                    else
                    {
                        throw new FileNotFoundException("Error reading ggeom.asc");
                    }

                    ix0 = Math.Min(Math.Max(ix0, 0), NX);
                    ix1 = Math.Min(Math.Max(ix1, 0), NX);
                    iy0 = Math.Min(Math.Max(iy0, 0), NY);
                    iy1 = Math.Min(Math.Max(iy1, 0), NY);
                    
                    if (ix0 < -30 || iy0 < -30 || ix1 > NX +30 || iy1 > NY +30 || (ix1 - ix0) < 4 || (iy0 - iy1) < 4) // far outside the model
                    {
                        MessageBox.Show(this, "3D area outside GRAMM domain", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    
//					if ((AH_MAX-AH_Min) >10)
//						vert_fac = 5 / (AH_MAX-AH_Min); // 0 -5
//					else
//						vert_fac = 0.5; // 0 -5
                    // Vertikalfaktor in Abhängigkeit des Bildausschnitts
                    vert_fac = vert_fac * 10 / (Math.Abs(ix1 - ix0) * MainForm.GRAMMHorGridSize); // X-Achse hat 10 Teiler
                    
                    double	x_step = 10.0 / (double) (ix1 - ix0); // -5 to +5 = 10

                    
                    Random zufall = new Random();
                    int randnum = zufall.Next(1,100000);
                    string path = Path.Combine(Gral.Main.ProjectName, @"Settings","3D_VIEW" + Convert.ToString(randnum) + ".dta");
                    using (BinaryWriter mywriter = new BinaryWriter(File.Open(path, FileMode.Create)))
                    {
                        mywriter.Write(x_step); // x-Step
                        mywriter.Write(x_step); // y-Step
                        mywriter.Write(ix1 - ix0); // x-Anzahl
                        mywriter.Write(iy0 - iy1); // y-Anzahl
                        
                        for(int i = ix0; i <= ix1; i++)
                        {
                            double height = -1;
                            
                            //for (int z = iy1; z <= iy0; z += 1)
                            for (int z = iy0; z >= iy1; z --)
                            {
                                if (z <= 0 || z >= NY || i <= 0 || i >= NX) // otherwise outside the GRAMM domain
                                {
                                    height = -1; // 0 - 5
                                }
                                else
                                {
                                    height = (AH[i, z] - AH_Min) * vert_fac;
                                }

                                mywriter.Write(height);
                            }							
                        }
                        mywriter.Flush();
                    }
                                        
                    //Elements______________________________________________________________________________________________________________
                    string path2 = Path.Combine(Gral.Main.ProjectName, @"Settings","3D_ELEMENTS" + Convert.ToString(randnum) + ".dta");
                    // write elements
                    if (EditR != null)
                    {
                        using (BinaryWriter mywriter = new BinaryWriter(File.Open(path2, FileMode.Create)))
                        {
                            
                            mywriter.Write(x_step); // x-Step
                            
                            foreach (ReceptorData _rd in EditR.ItemData)
                            {
                                double xr = _rd.Pt.X;
                                double yr = _rd.Pt.Y;
                                double zr = _rd.Height;
                                int ixr =  Convert.ToInt32(Math.Floor((xr -MainForm.GrammDomRect.West)/MainForm.GRAMMHorGridSize)) + 1;
                                int iyr =  Convert.ToInt32(Math.Floor((yr - MainForm.GrammDomRect.South) / MainForm.GRAMMHorGridSize)) + 1;
                                
                                if (ixr > 0 && iyr > 0 && ixr < NX && iyr < NY) // inside the model
                                {
                                    if (ixr > ix0 && iyr > iy1 && ixr < ix1 && iyr < iy0) // inside the actual view
                                    {
                                        double height = (AH[ixr, iyr] - AH_Min)*vert_fac; // heigth of the Base
                                        zr = height + zr*vert_fac; // Top Height of the Point
                                        xr = -5 + (ixr - ix0) * x_step ;
                                        yr = -5 + (iy0 - iyr) * x_step ;
                                        
                                        //MessageBox.Show(this, Convert.ToString(ix0) +"/" +Convert.ToString(iy1) +"/" +Convert.ToString(ixr) +"/" +Convert.ToString(iyr));
                                        //string data = _rd.Name.Replace("/"," ") + "/" + St_F.DblToIvarTxt(xr) +"/" +St_F.DblToIvarTxt(yr) +"/" +St_F.DblToIvarTxt(height) +"/" +St_F.DblToIvarTxt(zr);
                                        mywriter.Write(xr);
                                        mywriter.Write(yr);
                                        mywriter.Write(height);
                                        mywriter.Write(zr);
                                    }
                                }
                            }
                            
                            mywriter.Flush();
                        } // Streamwriter
                    } // Receptors
                    
                    #if __MonoCS__
                    #else
                    
                    Thread thr = new Thread(() =>
                    {
                        Gral3DFunctions.X_3D_Win win = new Gral3DFunctions.X_3D_Win(path, "", path2, smooth);
                        //System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(win);
                        win.Show();
                        win.Closed += (sender2, e2) =>
                            win.Dispatcher.InvokeShutdown();

                        System.Windows.Threading.Dispatcher.Run();
                    });
                    thr.IsBackground = true;
                    thr.SetApartmentState(ApartmentState.STA);
                    thr.Start();
#endif
                }
                else
                {
                    MessageBox.Show(this, "Error creating 3D view \nCan´t read ggeom.asc","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, "Error creating 3D view \n" + ex.Message.ToString(), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void GRAL_3D_View(bool smooth, double vert_fac)
        {
            double flowfieldraster = Convert.ToDouble(MainForm.numericUpDown10.Value);
            
            int NX = CellHeights.GetUpperBound(0);
            int NY = CellHeights.GetUpperBound(1);
            
            double AH_Min = 10000000; double AH_MAX = 0;
            
            for(int i = 1; i < NX; i++)
            {
                for (int z = NY - 1; z >= 1; z --)
                {
                    AH_Min = Math.Min(AH_Min, CellHeights[i, z]);
                    AH_MAX = Math.Max(AH_MAX, CellHeights[i, z]);
                }
            }

            
            // Vertikalfaktor in Abhängigkeit des Bildausschnitts
            vert_fac = vert_fac * 10 / (MainForm.GralDomRect.East - MainForm.GralDomRect.West); // X-Achse hat 10 Teiler
            
            int _nx = NX;
            int _ny = NY;
            int _step = 1;
            if (NX > 3000 || NY > 3000)
            {
                _nx /= 5;
                _ny /= 5;
                _step = 5;
            }
            else if (NX > 2300 || NY > 2300)
            {
                _nx /= 3;
                _ny /= 3;
                _step = 3;
            }
            else if (NX > 1300 || NY > 1300)
            {
                _nx /= 2;
                _ny /= 2;
                _step = 2;
            }
            else if (NX > 1000 || NY > 1000)
            {
                _nx /= 1;
                _ny /= 1;
                _step = 1;
            }
            else if (NX > 400 || NY > 400)
            {
                _nx /= 1;
                _ny /= 1;
                _step = 1;
            }
            double	x_step = 10.0 / (double) (_nx - 2); // -5 to +5 = 10
            

            Random zufall = new Random();
            int randnum = zufall.Next(1,100000);
            string path = Path.Combine(Gral.Main.ProjectName, @"Settings","3D_VIEW" + Convert.ToString(randnum) + ".dta");
            using(BinaryWriter mywriter = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                mywriter.Write(x_step); // x-Step
                mywriter.Write(x_step); // y-Step
                mywriter.Write(_nx - 2); // x-Anzahl
                mywriter.Write(_ny - 2); // y-Anzahl
                
                
                for(int i = 1; i < NX; i += _step)
                {
                    double height = -1;
                    //for (int z = iy1; z <= iy0; z += 1)
                    for (int z = NY - 1; z > 0; z -= _step)
                    {
                        height = (CellHeights[i, z] - AH_Min) * vert_fac;
                        mywriter.Write(height);
                    }
                }
                mywriter.Flush();
            }
            //mywriter.Close();
            
            
            //Elements______________________________________________________________________________________________________________
            string path2 = Path.Combine(Gral.Main.ProjectName, @"Settings","3D_ELEMENTS" + Convert.ToString(randnum) + ".dta");
            // write elements
            if (EditR != null)
            {
                using(StreamWriter mywriter = new StreamWriter(path2))
                {
                    mywriter.WriteLine("3D_V1.0");
                    mywriter.WriteLine("ELEMENTS");
                    mywriter.WriteLine(St_F.DblToIvarTxt(x_step)); // x-Step
                    foreach (ReceptorData _rd in EditR.ItemData)
                    {
                        double xr = _rd.Pt.X;
                        double yr = _rd.Pt.Y;
                        double zr = _rd.Height;
                        int ixr =  Convert.ToInt32(Math.Floor((xr -MainForm.GralDomRect.West) / flowfieldraster)) ;
                        int iyr =  Convert.ToInt32(Math.Floor((yr - MainForm.GralDomRect.South) / flowfieldraster)) ;
                                        
                        if (ixr > 0 && iyr > 0 && ixr < _nx && iyr < _ny) // inside the model
                        {
                            double height = (CellHeights[ixr, iyr] - AH_Min) * vert_fac; // heigth of the Base
                            zr = height + zr*vert_fac; // Top Height of the Point
                            xr = -5 + (ixr) * x_step ;
                            yr = (-5 + _ny * x_step)  - (iyr) * x_step ;
                            
                            //MessageBox.Show(this, Convert.ToString(ix0) +"/" +Convert.ToString(iy1) +"/" +Convert.ToString(ixr) +"/" +Convert.ToString(iyr));
                            string data =_rd.Name.Replace("/"," ") + "/" + St_F.DblToIvarTxt(xr) +"/" +St_F.DblToIvarTxt(yr) +"/" +St_F.DblToIvarTxt(height) +"/" +St_F.DblToIvarTxt(zr);
                            mywriter.WriteLine(data);
                            
                        }
                    }
                    mywriter.WriteLine("*");
                    mywriter.Flush();
                } // Streamwriter
            } // Receptors

#if __MonoCS__
#else
            Thread thr = new Thread(() =>
            {
                Gral3DFunctions.X_3D_Win win = new Gral3DFunctions.X_3D_Win(path, "", path2, smooth);
                //System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(win);
                win.Show();
                win.Closed += (sender2, e2) =>
                    win.Dispatcher.InvokeShutdown();

                System.Windows.Threading.Dispatcher.Run();
            });
            thr.IsBackground = true;
            thr.SetApartmentState(ApartmentState.STA);
            thr.Start();

            #endif
        }
        
    }
}
