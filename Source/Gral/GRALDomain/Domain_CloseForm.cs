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
 * User: U0178969
 * Date: 24.01.2019
 * Time: 14:53
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Windows.Forms;

using GralDomForms;
using GralItemForms;

namespace GralDomain
{
	public partial class Domain
	{
		/// <summary>
		/// Close the domain form and release all objects, like bitmaps and list
		/// </summary>
	    public void DomainFormClosed(object sender, FormClosedEventArgs e) // Kuntner Hauptfenster wird geschlossen
        {
            this.FormClosed -= new System.Windows.Forms.FormClosedEventHandler(this.DomainFormClosed); // avoid a 2nd call to this close function
            
            // Kuntner clean up open forms, picturebox, release memory to avoid memory lag
            HideWindows(0);
            
            MMO.StartMatchProcess -= new StartMatchingProcess(StartMatchingProcess);
            MMO.LoadWindData -= new LoadWindFileData(LoadWindFileForMatching);
            
            if (MMO != null)
            {
                MMO.close_allowed = true; // allow closing of the MMO form, otherwise MMO.Close() is locked
                MMO.wind_speeds = null;
                MMO.wind_direction = null;
                MMO.stability  = null;
                MMO.metfiles = null;
                MMO.filelength = null;
                MMO.datum = null;
                MMO.stunde = null;
                MMO.DecsepUser = null;
                MMO.RowsepUser = null;
                MMO.zeit = null;
                
                MMO.Close();
                MMO.Dispose();
                MMO = null;
            }
            
            for(int i = Application.OpenForms.Count-1; i >=0; i--)
            {
                if(Application.OpenForms[i] != MainForm && (Application.OpenForms[i].Text == "Match GRAMM flow fields with multiple meteorological observations"
                                                         || Application.OpenForms[i].Text == "Mathrasteroperation"
                                                         || Application.OpenForms[i].Text == "Source apportionment"
                                                         || Application.OpenForms[i].Text == "GRAL GUI - Section drawing"
                                                         || Application.OpenForms[i].Text == "GRAL -  3D Viewer")
                  )
                    Application.OpenForms[i].Close();
            }
         
            // cancel all await tasks
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource.Dispose();
            }
            CancellationTokenSource = null;

            ReleaseFileSystemWatchers();
            
            if (ObjectManagerForm != null) // Kuntner: close objectmanager
            {
                ObjectManagerForm.button5_Click(null, null); // close objectmanager
                ObjectManagerForm.object_redraw -= DomainRedrawDelegate;
                ObjectManagerForm.Close();
            }
            
            if (VerticalProfileForm != null)
            {
                VerticalProfileForm.Close();
            }
            
            if (ProfileConcentration.VertProfileDirection != null)
            {
                ProfileConcentration.VertProfileDirection.Close();
            }
            if (ProfileConcentration.VertProfileVelocity != null)
            {
                ProfileConcentration.VertProfileVelocity.Close();
            }

            if (picturebox1 != null)
            {
                picturebox1.Dispose();
            }

            picturebox1 = null;

            if (PictureBoxBitmap != null)
            {
                PictureBoxBitmap.Dispose();
            }
            PictureBoxBitmap = null;
            
            
            MMO = null;
            MMOData.WindVel =null; MMOData.WindDir = null; MMOData.SC = null; MMOData.Date = null; MMOData.Time = null; MMOData.Hour = null;
            
            if (NorthArrowBitmap != null)
                NorthArrowBitmap.Dispose();
            NorthArrowBitmap = null;
           
            CornerAreaSource = null;
            
            CellHeights = null;
            
            EditPS.PointSourceRedraw -= DomainRedrawDelegate; // Redraw from Edit Point Sources
            EditAS.AreaSourceRedraw -= DomainRedrawDelegate; // Redraw from areaedit
            EditB.BuildingRedraw -= DomainRedrawDelegate; // Redraw from editbuilding
            EditLS.LinesourceRedraw -= DomainRedrawDelegate; // Redraw from editls
            EditPortals.PortalSourceRedraw -= DomainRedrawDelegate; // Redraw from portal
            EditR.ReceptorRedraw -= DomainRedrawDelegate; // Redraw from editreceptors
            EditWall.WallRedraw -= DomainRedrawDelegate; // Redraw from editwalls
            EditVegetation.VegetationRedraw -= DomainRedrawDelegate; // Redraw from vegetation

            EditPS.ItemFormHide -= DomainItemFormHide; // Hide form 
            EditAS.ItemFormHide -= DomainItemFormHide; // Hide form 
            EditB.ItemFormHide -= DomainItemFormHide; // Hide form 
            EditLS.ItemFormHide -= DomainItemFormHide; // Hide form 
            EditPortals.ItemFormHide -= DomainItemFormHide; // Hide form 
            EditR.ItemFormHide -= DomainItemFormHide; // Hide form 
            EditWall.ItemFormHide -= DomainItemFormHide; // Hide form
            EditVegetation.ItemFormHide -= DomainItemFormHide; // Hide form 

            GeoReferenceOne.Form_Georef1_Closed -= new Georeference1_Closed(CloseGeoRef1); // Message, that georef1 is closed
            GeoReferenceTwo.Form_Georef2_Closed -= new Georeference2_Closed(CloseGeoRef2); // Message, that georef2 is closed
            
            if (GeoReferenceOne != null) GeoReferenceOne.Close();
            if (GeoReferenceTwo != null) GeoReferenceTwo.Close();
            
            if (EditPS != null)
            {
                EditPS.AllowClosing = true;
                EditPS.Close();
                EditPS.Dispose();
            }
            if (EditAS != null)
            {
                EditAS.AllowClosing = true;
                EditAS.Close();
                EditAS.Dispose();
            }
            if (EditLS != null)
            {
                EditLS.AllowClosing = true;
                EditLS.Close();
                EditLS.Dispose();
            }
            if (EditPortals != null)
            {
                EditPortals.AllowClosing = true;
                EditPortals.Close();
                EditPortals.Dispose();
            }
            if (EditB != null)
            {
                EditB.AllowClosing = true;
                EditB.Close();
                EditB.Dispose();
            }
            if (EditWall != null)
            {
                EditWall.AllowClosing = true;
                EditWall.Close();
                EditWall.Dispose();
            }
            if (EditVegetation != null)
            {
                EditVegetation.AllowClosing = true;
                EditVegetation.Close();
                EditVegetation.Dispose();
            }
            if (EditR != null)
            {
                EditR.AllowClosing = true;
                EditR.Close();
                EditR.Dispose();
            }
            
            if (MeteoDialog != null) // 
            {
                MeteoDialog.Close();
                MeteoDialog.Dispose();
            }
            
            InfoBoxCloseAllForms();
            
            // Close Vertical Profile windows
            for(int i = Application.OpenForms.Count-1; i >=0; i--)
            {
                if(Application.OpenForms[i] != MainForm && Application.OpenForms[i].Name.Contains("Profile_"))
                    Application.OpenForms[i].Close();
            }
            
            MouseWheel -= new MouseEventHandler(form1_MouseWheel); // Kuntner
            ToolTipMousePosition.Dispose();
            if (toolTip1 != null)
                toolTip1.Dispose();
            if (menuStrip1 != null)
                menuStrip1.Dispose();
            if (panel1 != null)
                panel1.Dispose();
            if (colorDialog1 != null)
                colorDialog1.Dispose();
            if (openFileDialog1 != null)
                openFileDialog1.Dispose();
            if (fontDialog1 != null)
                fontDialog1.Dispose();
            if (saveFileDialog1 != null)
                saveFileDialog1.Dispose();
            
            foreach(DrawingObjects _drobj  in ItemOptions)
            {
                _drobj.Dispose();
            }
            ItemOptions.Clear();
            ItemOptions.TrimExcess();
            InfoBoxTip.Dispose();
            Application.DoEvents();
            
            try
            {
                if (DomainClosed!= null)
                    DomainClosed(this, e);
            }
            catch
            {}
        }
	    
		/// <summary>
		/// Prevent closing the domain form when a match process or a gif recoring is running
		/// </summary>
	    void DomainFormClosing(object sender, FormClosingEventArgs e)
        {
            //if match process is running
            if (MMO != null)
            {
                if (MMO.StartMatch == true && MMO.Visible == false) // match process is working
                {
                    MessageBox.Show(this, "Closing this Window is not possible while the match process is working", "Close the GIS Window", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Cancel = true;
                }
            }
            
            // if Gif recording is running 
            if (OnlineCounter == -1)
            {
                // stop gif recording
                OnlineCounter = 1000;
                Application.DoEvents();
                System.Threading.Thread.Sleep(250);
            }
        }
        
		/// <summary>
		/// Release all file system watchers
		/// </summary>
        private void ReleaseFileSystemWatchers()
        {
            if (FileWatch.UVGramm != null)
            {
                FileWatch.UVGramm.Changed -= new FileSystemEventHandler(UVGrammChanged);
                FileWatch.UVGramm.Dispose();  //control changes in the file uv_Gramm.txt, containing the actual GRAMM windfield online
            }
            if (FileWatch.UGramm != null)
            {
                FileWatch.UGramm.Changed -= new FileSystemEventHandler(UGrammChanged);
                FileWatch.UGramm.Dispose();  //control changes in the file u_Gramm.txt, containing the actual GRAMM windcomponent u online
            }
            if (FileWatch.SpeedGramm != null)
            {
                FileWatch.SpeedGramm.Changed -= new FileSystemEventHandler(SpeedGrammChanged);
                FileWatch.SpeedGramm.Dispose();  //control changes in the file speed_Gramm.txt, containing the actual GRAMM horizontal windspeed online
            }
            if (FileWatch.VGramm != null)
            {
                FileWatch.VGramm.Changed -= new FileSystemEventHandler(VGrammChanged);
                FileWatch.VGramm.Dispose();  //control changes in the file v_Gramm.txt, containing the actual GRAMM windcomponent v online
            }
            if (FileWatch.WGramm != null)
            {
                FileWatch.WGramm.Changed -= new FileSystemEventHandler(WGrammChanged);
                FileWatch.WGramm.Dispose();  //control changes in the file w_Gramm.txt, containing the actual GRAMM windcomponent w online
            }
            if (FileWatch.TAbsGramm != null)
            {
                FileWatch.TAbsGramm.Changed -= new FileSystemEventHandler(TabsGrammChanged);
                FileWatch.TAbsGramm.Dispose();  //control changes in the file tabs_Gramm.txt, containing the actual GRAMM absolut temperature online
            }
            if (FileWatch.TPotGramm != null)
            {
                FileWatch.TPotGramm.Changed -= new FileSystemEventHandler(TpotGrammChanged);
                FileWatch.TPotGramm.Dispose();  //control changes in the file tpot_Gramm.txt, containing the actual GRAMM potential temperature online
            }
            if (FileWatch.HumGramm != null)
            {
                FileWatch.HumGramm.Changed -= new FileSystemEventHandler(HumGrammChanged);
                FileWatch.HumGramm.Dispose();  //control changes in the file hum_Gramm.txt, containing the actual GRAMM humidity online
            }
            if (FileWatch.NhpGramm != null)
            {
                FileWatch.NhpGramm.Changed -= new FileSystemEventHandler(NhpGrammChanged);
                FileWatch.NhpGramm.Dispose();  //control changes in the file nhp_Gramm.txt, containing the actual GRAMM non-hydrostatic pressure online
            }
            if (FileWatch.GlobGramm != null)
            {
                FileWatch.GlobGramm.Changed -= new FileSystemEventHandler(GlobGrammChanged);
                FileWatch.GlobGramm.Dispose();  //control changes in the file glob_Gramm.txt, containing the actual GRAMM global radiation online
            }
            if (FileWatch.TerrGramm != null)
            {
                FileWatch.TerrGramm.Changed -= new FileSystemEventHandler(TerrGrammChanged);
                FileWatch.TerrGramm.Dispose();  //control changes in the file terr_Gramm.txt, containing the actual GRAMM terrestrial radiation online
            }
            if (FileWatch.SensHeatGramm != null)
            {
                FileWatch.SensHeatGramm.Changed -= new FileSystemEventHandler(SensheatGrammChanged);
                FileWatch.SensHeatGramm.Dispose();  //control changes in the file sensheat_Gramm.txt, containing the actual GRAMM sensible heat flux online
            }
            if (FileWatch.LatHeatGramm != null)
            {
                FileWatch.LatHeatGramm.Changed -= new FileSystemEventHandler(LatheatGrammChanged);
                FileWatch.LatHeatGramm.Dispose();  //control changes in the file latheat_Gramm.txt, containing the actual GRAMM sensible latent flux online
            }
            if (FileWatch.VricVelGramm != null)
            {
                FileWatch.VricVelGramm.Changed -= new FileSystemEventHandler(FricvelGrammChanged);
                FileWatch.VricVelGramm.Dispose();  //control changes in the file fricvel_Gramm.txt, containing the actual GRAMM friction velocity online
            }
            if (FileWatch.InverseMOGramm != null)
            {
                FileWatch.InverseMOGramm.Changed -= new FileSystemEventHandler(InverseMOGrammChanged);
                FileWatch.InverseMOGramm.Dispose();  //control changes in the file inverseMO_Gramm.txt, containing the actual GRAMM inverse MO-length online
            }
            if (FileWatch.SurfTempGramm != null)
            {
                FileWatch.SurfTempGramm.Changed -= new FileSystemEventHandler(SurfTempGrammChanged);
                FileWatch.SurfTempGramm.Dispose();  //control changes in the file surfTemp_Gramm.txt, containing the actual GRAMM surface temperature online
            }
            if (FileWatch.StabClassGramm != null)
            {
                FileWatch.StabClassGramm.Changed -= new FileSystemEventHandler(StabilityclassGrammChanged);
                FileWatch.StabClassGramm.Dispose();  //control changes in the file stabilityclass_Gramm.txt, containing the actual GRAMM stabilty classes (1-7) online
            }
            if (FileWatch.TkeGramm != null)
            {
                FileWatch.TkeGramm.Changed -= new FileSystemEventHandler(TkeGrammChanged);
                FileWatch.TkeGramm.Dispose();  //control changes in the file tke_Gramm.txt, containing the actual GRAMM turbulent kinetic energy online
            }
            if (FileWatch.DisGramm != null)
            {
                FileWatch.DisGramm.Changed -= new FileSystemEventHandler(DisGrammChanged);
                FileWatch.DisGramm.Dispose();  //control changes in the file dis_Gramm.txt, containing the actual GRAMM dissipation online
            }
        }

        /// <summary>
        /// Close all info boxes and clear the selected items list
        /// </summary>
        public void InfoBoxCloseAllForms()
        {
            for(int i = Application.OpenForms.Count-1; i >=0; i--)
            {
                if(Application.OpenForms[i] != MainForm && Application.OpenForms[i].Name == "Infobox")
                    Application.OpenForms[i].Close();
            }
            SelectedItems.Clear(); // delete the Select-List
        }
    }
}