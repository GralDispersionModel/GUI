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

/*
 * Created by SharpDevelop.
 * User: Markus Kuntner
 * Date: 20.11.2018
 * Time: 18:50
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Mouse Down Events on the picturebox
        /// </summary>
        public void Picturebox1_MouseDown(object sender, MouseEventArgs e)
        {
            #region Mousecontrol-Values
            //				2    Move map
            //				3    Georeferencing 1
            //				4    Set startpoint of GRAL domain
            //				5    Set endpoint of GRAL domain
            //				6    Position of point source
            //				7    Select point source
            //				8    Position of area corner
            //				9    Select area source
            //				10    Position of line corner
            //				11    Select line source
            //				12    Georeferencing 2
            //				13    Panel zoom
            //				14    Get zoom area for panel zoom
            //				15    Position of portal
            //				16    Select portal
            //				17    Position of building corner
            //				19    Select buildings
            //				20    Digitize north arrow
            //				21    Digitize map scale bar
            //				22    Measure distance
            //				23    Measure area
            //				24    Position of receptor
            //				25    Select receptor
            //				26    Delete one mouseclick from queue for receptor
            //				28    Set poition of legend
            //				30    Set startpoint of GRAMM domain
            //				31    Set endpoint of GRAMM domain
            //				32    Sample point for met-timeseries
            //				33    Sample point for concentration-timeseries
            //				35    Sample point for source apportionment
            //				40    Sample point for vertical profile GRAMM online
            //				44    Select section for wind section drawing
            //				45    Select section for concentration section drawing
            //				50    Sample point for concentration from file
            //				62    Sample point for vertical profile
            //				65    Sample point for re-order
            //				66    Sample point for match-to-observation
            //				70    Check single value at GRAMM grid
            //				75	  Position of wall corner
            //				76    Select wall
            //				77    Select vegetation
            //				78	  Position of wall corner
            //				79    Position of area corner
            //				100    final corner point of changed line source point
            //				101    final editing wall point
            //				108    final corner point of changed area source point
            //				109    final corner point of changed vegetation point
            //				117    final corner point of changed building edge point
            //				200    3D concentration - set point
            //				300    Startpoint of exporting GRAMM domain
            //				301    Endpoint of exporting GRAMM domain
            //				700    Delete one mouseclick from queue for point source
            //				1000   Position of line source point inline editing
            //				1001 	Position of wall point inline editing
            //				1080    Position of area source point inline editing
            //				1081    Position of vegetation point inline editing
            //				1170    Position of building edge point inline editing
            //				2400    Position of receptor inline editing
            //				6000    Position of point source inline editing
            //				7000    Move and scale base map
            //				9999    Mofification of the GRAL topography
            #endregion

            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        Picturebox1MouseDownLeft(sender, e);
                        break;
                    }
                case MouseButtons.Right:
                    {
                        Picturebox1MouseDownRight(sender, e);
                        break;
                    }
                case MouseButtons.Middle: // Kuntner mit rechter Maus auch verschieben
                    OldXPosition = e.X; // Kuntner
                    OldYPosition = e.Y; // Kuntner
                    //
                    break;
                default:
                    break;
            }

        }
        

        /// <summary>
        /// Returns the point in the middle between pt1 and pt2
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        PointD GetPointBetween(PointD pt1, PointD pt2)
        {
            return new PointD(pt1.X - (pt1.X - pt2.X) / 2, pt1.Y - (pt1.Y - pt2.Y)/ 2);
        }
        /// <summary>
        /// Returns the point in the middle between pt1 and pt2
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        GralData.PointD_3d GetPointBetween(GralData.PointD_3d pt1, GralData.PointD_3d pt2)
        {
            return new GralData.PointD_3d(pt1.X - (pt1.X - pt2.X) / 2, 
                                          pt1.Y - (pt1.Y - pt2.Y) / 2,
                                          pt1.Z - (pt1.Z - pt2.Z) / 2);
        }

        // Section Form sends a closed message!
        void section_Form_Section_Closed(object sender, EventArgs e)
        {
            sectionpoints.Clear();
            Picturebox1_Paint();
        }
        
        //Mouse up events
        /// <summary>
        /// Mousekey up events on the picturebox
        /// </summary>
        private void Picturebox1_MouseUp(object sender, MouseEventArgs e)
        {
            if ((MouseControl == 2) | (e.Button==MouseButtons.Middle)) // Kuntner auch beim Auslassen des mittleren Buttons verschieben
            {
                double xfac_old = XFac;
                double transformx_old = TransformX;
                double transformy_old = TransformY;
                
                TransformX =TransformX+ e.X - OldXPosition;
                TransformY = TransformY + e.Y - OldYPosition;
                
                foreach(DrawingObjects _dr in ItemOptions)
                {
                    try
                    {
                        _dr.DestRec = new Rectangle(TransformX + Convert.ToInt32((_dr.West - MapSize.West) / BmpScale / MapSize.SizeX),
                                                    TransformY - Convert.ToInt32((_dr.North - MapSize.North) / BmpScale / MapSize.SizeX),
                                                    Convert.ToInt32(_dr.Picture.Width * _dr.PixelMx / MapSize.SizeX * XFac),
                                                    Convert.ToInt32(_dr.Picture.Height * _dr.PixelMx / MapSize.SizeX * XFac));
                        _dr.SourceRec = new Rectangle(0, 0, _dr.Picture.Width, _dr.Picture.Height);
                    }
                    catch{}
                }
                
                MoveCoordinatesOfEditedItems(xfac_old, transformx_old, transformy_old);
                
                Picturebox1_Paint();
            }

            //define GRAL Domain
            if (MouseControl == 5)
            {
                MouseControl = 0;
                Cursor.Clip = Rectangle.Empty;
                
                if (Gral.Main.Project_Locked == true)
                {
                    Gral.Main.ProjectLockedMessage(); // Project locked! - do not save any changes!
                    MouseControl = 0;
                    Picturebox1_Paint();
                    return;
                }
                
                //compute model domain extenstions in natural coordinates and clip them to the chosen raster size of the concentration grid
                MainForm.GralDomRect.North = Math.Round((GRALDomain.Top - TransformY) * BmpScale * MapSize.SizeY + MapSize.North, 1, MidpointRounding.AwayFromZero);
                MainForm.GralDomRect.North = Math.Round(MainForm.GralDomRect.North / MainForm.HorGridSize, 0, MidpointRounding.AwayFromZero) * Convert.ToDouble(MainForm.numericUpDown9.Value);
                MainForm.GralDomRect.South = Math.Round((GRALDomain.Bottom - TransformY) * BmpScale * MapSize.SizeY + MapSize.North, 1, MidpointRounding.AwayFromZero);
                MainForm.GralDomRect.South = Math.Round(MainForm.GralDomRect.South / MainForm.HorGridSize, 0, MidpointRounding.AwayFromZero) * Convert.ToDouble(MainForm.numericUpDown9.Value);
                MainForm.GralDomRect.West = Math.Round((GRALDomain.Left - TransformX) * BmpScale * MapSize.SizeX + MapSize.West, 1, MidpointRounding.AwayFromZero);
                MainForm.GralDomRect.West = Math.Round(MainForm.GralDomRect.West / MainForm.HorGridSize, 0, MidpointRounding.AwayFromZero) * Convert.ToDouble(MainForm.numericUpDown9.Value);
                MainForm.GralDomRect.East = Math.Round((GRALDomain.Right - TransformX) * BmpScale * MapSize.SizeX + MapSize.West, 1, MidpointRounding.AwayFromZero);
                MainForm.GralDomRect.East = Math.Round(MainForm.GralDomRect.East / MainForm.HorGridSize, 0, MidpointRounding.AwayFromZero) * Convert.ToDouble(MainForm.numericUpDown9.Value);
                
                //check if GRAL Domain is smaller or equal to GRAMM domain
                bool alright = true;
                if(MainForm.textBox15.Text!="")
                {
                    if ((MainForm.GralDomRect.West < MainForm.GrammDomRect.West) || (MainForm.GralDomRect.East > MainForm.GrammDomRect.East) || (MainForm.GralDomRect.South < MainForm.GrammDomRect.South) || (MainForm.GralDomRect.North > MainForm.GrammDomRect.North))
                    {
                        alright = false;
                        //write model domain to the text boxes
                        MainForm.textBox2.Text = "";
                        MainForm.textBox5.Text = "";
                        MainForm.textBox6.Text = "";
                        MainForm.textBox7.Text = "";
                        
                        //remove GRAL Domain from object list
                        RemoveItemFromItemOptions("GRAL DOMAIN");
                        
                        MessageBox.Show(this, "GRAL Domain is outside GRAMM Domain", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Picturebox1_Paint();
                    }
                }
                if (alright == true)
                {
                    try
                    {
                        //write model domain to the text boxes
                        MainForm.textBox2.Text = Convert.ToString(MainForm.GralDomRect.North, ic);;
                        MainForm.textBox5.Text = Convert.ToString(MainForm.GralDomRect.South, ic);;
                        MainForm.textBox6.Text = Convert.ToString(MainForm.GralDomRect.West, ic);;
                        MainForm.textBox7.Text = Convert.ToString(MainForm.GralDomRect.East, ic);;

                        //write domain to "GRAL.geb"
                        MainForm.WriteGralGebFile();

                        //clip rectangle to raster
                        int x1 = Convert.ToInt32((MainForm.GralDomRect.West - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                        int y1 = Convert.ToInt32((MainForm.GralDomRect.North - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                        int x2 = Convert.ToInt32((MainForm.GralDomRect.East - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                        int y2 = Convert.ToInt32((MainForm.GralDomRect.South - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                        int width = x2 - x1;
                        int height = y2 - y1;
                        GRALDomain = new Rectangle(x1, y1, width, height);
                        
                        // GRAL topography allowed?
                        if (MainForm.GRALSettings.BuildingMode > 0 && Gral.Main.Project_Locked == false &&
                            MainForm.GralDomRect.East != MainForm.GralDomRect.West && MainForm.GralDomRect.North != MainForm.GralDomRect.South)
                        {
                            originalGRALTopographyToolStripMenuItem.Enabled = true;
                        }
                        else
                        {
                            originalGRALTopographyToolStripMenuItem.Enabled = false;
                        }

                        Cursor = Cursors.Default;
                        
                        //add GRAL domain to object list
                        CheckForExistingDrawingObject("GRAL DOMAIN"); 
                        
                        groupBox5.Visible = true;
                        //refresh the list of source groups within the model domain
                        MainForm.SelectAllUsedSourceGroups();
                        MainForm.listBox5.Items.Clear();
                        MainForm.SetEmissionFilesInvalid();
                        MainForm.button18.Visible = false; // Emission button not visible
                        MainForm.Change_Label(2, -1); // Emission button not visible
                        MainForm.button21.Visible = false;
                        Picturebox1_Paint();

                        MainForm.DeleteGralTopofile();
                        MainForm.DeleteGralGffFile();
                        
                    }
                    catch
                    {
                        GRALDomain = new Rectangle();
                        MessageBox.Show(this, "Please define Project Folder first. Click on the 'New' or 'Open' button.","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            

            //define GRAMM Domain
            if (MouseControl == 31)
            {
                MouseControl = 0;
                Cursor.Clip = Rectangle.Empty;
                
                if (MainForm.GRAMM_Locked == true)
                {
                    Gral.Main.GRAMMLockedMessage(); // Project locked! - do not save any changes!
                    return;
                }
                
                try
                {
                    //delete existing geometry and landusefiles
                    MainForm.HideTopo();

                    //compute model domain extenstions in natural coordinates and clip them to the chosen raster size of the flow field grid
                    MainForm.GrammDomRect.North = Math.Round((GRAMMDomain.Top - TransformY) * BmpScale * MapSize.SizeY + MapSize.North, 1, MidpointRounding.AwayFromZero);
                    MainForm.GrammDomRect.North = Math.Round(MainForm.GrammDomRect.North / MainForm.GRAMMHorGridSize, 0, MidpointRounding.AwayFromZero) * Convert.ToDouble(MainForm.numericUpDown18.Value);
                    MainForm.textBox15.Text = Convert.ToString(MainForm.GrammDomRect.North, ic);;
                    MainForm.GrammDomRect.South = Math.Round((GRAMMDomain.Bottom - TransformY) * BmpScale * MapSize.SizeY + MapSize.North, 1, MidpointRounding.AwayFromZero);
                    MainForm.GrammDomRect.South = Math.Round(MainForm.GrammDomRect.South / MainForm.GRAMMHorGridSize, 0, MidpointRounding.AwayFromZero) * Convert.ToDouble(MainForm.numericUpDown18.Value);
                    MainForm.textBox14.Text = Convert.ToString(MainForm.GrammDomRect.South, ic);;
                    MainForm.GrammDomRect.West = Math.Round((GRAMMDomain.Left - TransformX) * BmpScale * MapSize.SizeX + MapSize.West, 1, MidpointRounding.AwayFromZero);
                    MainForm.GrammDomRect.West = Math.Round(MainForm.GrammDomRect.West / MainForm.GRAMMHorGridSize, 0, MidpointRounding.AwayFromZero) * Convert.ToDouble(MainForm.numericUpDown18.Value);
                    MainForm.textBox13.Text = Convert.ToString(MainForm.GrammDomRect.West, ic);;
                    MainForm.GrammDomRect.East = Math.Round((GRAMMDomain.Right - TransformX) * BmpScale * MapSize.SizeX + MapSize.West, 1, MidpointRounding.AwayFromZero);
                    MainForm.GrammDomRect.East = Math.Round(MainForm.GrammDomRect.East / MainForm.GRAMMHorGridSize, 0, MidpointRounding.AwayFromZero) * Convert.ToDouble(MainForm.numericUpDown18.Value);
                    MainForm.textBox12.Text = Convert.ToString(MainForm.GrammDomRect.East, ic);;
                    
                    //write domain to "GRAMM.geb"
                    MainForm.WriteGrammGebFile();
                    MainForm.radioButton1.Checked = true;
                    MainForm.radioButton2.Checked = false;
                    MainForm.checkBox25.Visible = false;
                    MainForm.groupBox9.Visible = true;

                    //clip rectangle to raster
                    int x1 = Convert.ToInt32((MainForm.GrammDomRect.West - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                    int y1 = Convert.ToInt32((MainForm.GrammDomRect.North - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                    int x2 = Convert.ToInt32((MainForm.GrammDomRect.East - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
                    int y2 = Convert.ToInt32((MainForm.GrammDomRect.South - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
                    int width = x2 - x1;
                    int height = y2 - y1;
                    GRAMMDomain = new Rectangle(x1, y1, width, height);
                    Cursor = Cursors.Default;
                    
                    //add GRAMM domain to object list
                    CheckForExistingDrawingObject("GRAMM DOMAIN"); 
                    Picturebox1_Paint();
                }
                catch
                {
                    GRAMMDomain = new Rectangle();
                    MessageBox.Show(this, "Please define Project Folder first. Click on the 'Open' button","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            //define GRAMM sub-domain for export
            if (MouseControl == 301)
            {
                MouseControl = 0;
                Cursor.Clip = Rectangle.Empty;
                
                //export GRAMM sub-domain
                GrammExportFile();
                
            }

            //get zoom area for panel zoom
            if (MouseControl == 14)
            {
                double fac1 = 1;
                double fac2 = 1;
                MouseControl = 0;
                Cursor = Cursors.Default;
                try
                {
                    fac1 = Convert.ToDouble(picturebox1.Width) / Convert.ToDouble(PanelZoom.Width);
                    fac2 = Convert.ToDouble(picturebox1.Height) / Convert.ToDouble(PanelZoom.Height);
                    fac1 = Math.Min(fac1, fac2);
                    XFac *= fac1;
                    BmpScale = 1 / XFac;
                    TransformX = Convert.ToInt32((TransformX * fac1 + (picturebox1.Width / 2 - (PanelZoom.X + PanelZoom.Width / 2) * fac1)));
                    TransformY = Convert.ToInt32((TransformY * fac1 + (picturebox1.Height / 2 - (PanelZoom.Y + PanelZoom.Height / 2) * fac1)));
                    PanelZoom = new Rectangle();
                    
                    foreach(DrawingObjects _dr in ItemOptions)
                    {
                        try
                        {
                            _dr.DestRec = new Rectangle(TransformX + Convert.ToInt32((_dr.West - MapSize.West) / BmpScale / MapSize.SizeX),
                                                        TransformY - Convert.ToInt32((_dr.North - MapSize.North) / BmpScale / MapSize.SizeX),
                                                        Convert.ToInt32(_dr.Picture.Width * _dr.PixelMx / MapSize.SizeX * XFac),
                                                        Convert.ToInt32(_dr.Picture.Height * _dr.PixelMx / MapSize.SizeX * XFac));
                            _dr.SourceRec = new Rectangle(0, 0, _dr.Picture.Width, _dr.Picture.Height);
                        }
                        catch{}
                    }
                    
                    Picturebox1_Paint();
                }
                catch
                {
                    PanelZoom = new Rectangle();
                }
            }
            
            if (MouseControl == 9999) // if GRAL topography is changed
            {
                // restore Blocked array
                Array.Clear(TopoModifyBlocked, 0, TopoModifyBlocked.Length);
            }
            
            //enable/disable GRAL simulations
            MainForm.Enable_GRAL();
            //enable/disable GRAMM simulations
            MainForm.Enable_GRAMM();
        }
        
        void form1_MouseWheel(object sender, MouseEventArgs e) // Kuntner
        {
            if(e.Delta > 0)
            {
                ZoomPlusMinus(1, e); // Scrollrad Up
            }
            
            if(e.Delta < 0)
            {
                ZoomPlusMinus(-1, e);// Scrollrad Down
            }
            
        }
        
        /// <summary>
        /// Zoom the map using the mousewheel or the Menu Point Zoom and MouseClick
        /// </summary>
        private void ZoomPlusMinus(int zoom, MouseEventArgs e) // Kuntner
        {
            double xfac_old = XFac;
            double transformx_old = TransformX;
            double transformy_old = TransformY;
            
            double faktor=1.5;
            
            //Console.Beep(1000,100);
            // zoom picturebox, zoom=1 +, zoom=-1 -
            if (zoom == 0)
            {
                return; // exit by zoom=0
            }

            if (zoom>0)
            {
                faktor = 1.5; // zoom in
            }

            if (zoom<0)
            {
                faktor = 1 / 1.5; // zoom out
            }

            try // Kuntner: catch 1/0 and convert.ToInt32 Overflow
            {
                XFac *= faktor;
                BmpScale = 1 / XFac;
                TransformX = Convert.ToInt32((TransformX * faktor + (picturebox1.Width / 2 - e.X * faktor)));
                TransformY = Convert.ToInt32((TransformY * faktor + (picturebox1.Height / 2 - e.Y * faktor)));
                
                foreach(DrawingObjects _dr in ItemOptions)
                {
                    try
                    {
                        _dr.DestRec = new Rectangle(TransformX + Convert.ToInt32((_dr.West - MapSize.West) / BmpScale / MapSize.SizeX),
                                                    TransformY - Convert.ToInt32((_dr.North - MapSize.North) / BmpScale / MapSize.SizeX),
                                                    Convert.ToInt32(_dr.Picture.Width * _dr.PixelMx / MapSize.SizeX * XFac),
                                                    Convert.ToInt32(_dr.Picture.Height * _dr.PixelMx / MapSize.SizeX * XFac));
                        _dr.SourceRec = new Rectangle(0, 0, _dr.Picture.Width, _dr.Picture.Height);
                    }
                    catch{}
                }
                
                MoveCoordinatesOfEditedItems(xfac_old, transformx_old, transformy_old);
            }
            catch
            {}
            
            Picturebox1_Paint();
        }
    }
}
