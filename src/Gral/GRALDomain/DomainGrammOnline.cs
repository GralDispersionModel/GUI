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
 * Date: 06.01.2019
 * Time: 12:05
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using GralIO;
using GralDomForms;
using System.Collections.Generic;

namespace GralDomain
{
    public partial class Domain
    {
        ///////////////////////////////////////////////////////////////
        //  GRAMM ONLINE
        ///////////////////////////////////////////////////////////////

        /// <summary>
        /// Draw an online map
        /// </summary>
        private void DrawOnlineMap()
        {
            try
            {
                if (OnlineRedraw != null)
                {
                    EventArgs e = new EventArgs();
                    OnlineRedraw(this, e);
                }
            }
            catch
            { }
        }

        private async void AsyncUVGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => UVGrammChanged(sender, e));
        }
        //read the file uv_Gramm.txt when it is changed by GRAMM.exe and compute vector plot
        public void UVGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute vectors
                    ReDrawVectors = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "VM: uv_GRAMM")
                        {
                            LoadVectors(Path.Combine(Gral.Main.ProjectName, @"Computation", "uv_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncUGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => UGrammChanged(sender, e));
        }
        //read the file u_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void UGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: u_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "u_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncSpeedGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => SpeedGrammChanged(sender, e));
        }
        //read the file speed_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void SpeedGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: speed_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "speed_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncVGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => VGrammChanged(sender, e));
        }
        //read the file v_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void VGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: v_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "v_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncWGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => WGrammChanged(sender, e));
        }
        //read the file w_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void WGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: w_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "w_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncTabsGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => TabsGrammChanged(sender, e));
        }
        //read the file tabs_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void TabsGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: tabs_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "tabs_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncTpotGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => TpotGrammChanged(sender, e));
        }
        //read the file tpot_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void TpotGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: tpot_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "tpot_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncHumGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => HumGrammChanged(sender, e));
        }
        //read the file hum_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void HumGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: hum_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "hum_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncNhpGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => NhpGrammChanged(sender, e));
        }
        //read the file nhp_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void NhpGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: nhp_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "nhp_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncGlobGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => GlobGrammChanged(sender, e));
        }
        //read the file glob_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void GlobGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: glob_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "glob_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncTerrGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => TerrGrammChanged(sender, e));
        }
        //read the file terr_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void TerrGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: terr_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "terr_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncSensheatGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => SensheatGrammChanged(sender, e));
        }
        //read the file sensheat_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void SensheatGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: sensheat_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "sensheat_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncLatheatGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => LatheatGrammChanged(sender, e));
        }
        //read the file latheat_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void LatheatGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: latheat_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "latheat_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncFricvelGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => FricvelGrammChanged(sender, e));
        }
        //read the file fricvel_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void FricvelGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: fricvel_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "fricvel_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncInverseMOGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => InverseMOGrammChanged(sender, e));
        }
        //read the file inverseMO_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void InverseMOGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: inverseMO_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "inverseMO_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncSurfTempGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => SurfTempGrammChanged(sender, e));
        }
        //read the file surfTemp_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void SurfTempGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: surfTemp_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "surfTemp_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncStabilityclassGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => StabilityclassGrammChanged(sender, e));
        }
        //read the file stabilityclass_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void StabilityclassGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: stabilityclass_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "stabilityclass_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncTKEGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => TkeGrammChanged(sender, e));
        }
        //read the file tke_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void TkeGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: tke_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "tke_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        private async void AsyncDisGrammChanged(object sender, FileSystemEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => DisGrammChanged(sender, e));
        }
        //read the file dis_Gramm.txt when it is changed by GRAMM.exe and compute postmap
        void DisGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref OnlineCounter);
                if (OnlineCounter >= MainForm.OnlineRefreshInterval)
                {
                    System.Threading.Interlocked.Exchange(ref OnlineCounter, 0);
                    //compute pixel map
                    ReDrawContours = true;
                    foreach (DrawingObjects _drobj in ItemOptions)
                    {
                        if (_drobj.Name == "PM: dis_GRAMM")
                        {
                            PostmapCalc(Path.Combine(Gral.Main.ProjectName, @"Computation", "dis_GRAMM.txt"), _drobj);
                            DrawOnlineMap();
                        }
                    }
                }
            }
            catch
            { }
        }

        //show vertical profile of a quantity of GRAMM online
        private void Button34_Click(object sender, EventArgs e)
        {
            MouseControl = MouseMode.SetPointVertWindProfileOnline;
            Cursor = Cursors.Cross;
        }

        // show vertical profile of a 3D concentraion
        private void Button52_Click(object sender, EventArgs e)
        {
            MouseControl = MouseMode.SetPointConcProfile;
            Cursor = Cursors.Cross;
        }

        //show vertical profile of a quantity of GRAMM online
        private void VertProfile(PointD TestPt)
        {
            //define GRAMM online field
            string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "vp_speed.txt");
            if (InputBox2("Vertical profile, GRAMM online", "Select field:", ref file) == DialogResult.OK)
            {
                file = Path.Combine(Gral.Main.ProjectName, @"Computation", "vp_" + file + ".txt");
                int x1 = 1;
                int y1 = 1;
                if (MainForm.textBox13.Text != "")
                {
                    x1 = (int)((TestPt.X - MainForm.GrammDomRect.West) / MainForm.GRAMMHorGridSize) + 1;
                    y1 = (int)((TestPt.Y - MainForm.GrammDomRect.South) / MainForm.GRAMMHorGridSize) + 1;
                }
                else
                {
                    x1 = (int)((TestPt.X - Convert.ToDouble(MainForm.textBox6.Text)) / (double) MainForm.numericUpDown10.Value) + 1;
                    y1 = (int)((TestPt.Y - Convert.ToDouble(MainForm.textBox5.Text)) / (double) MainForm.numericUpDown10.Value) + 1;
                }

                using (StreamWriter writer = new StreamWriter(file))
                {
                    writer.WriteLine(Convert.ToString(x1));
                    writer.WriteLine(Convert.ToString(y1));
                }

                //show vertical profile
                VerticalProfile_Dynamic vertprof = new VerticalProfile_Dynamic();
                string filename = "GRAMM-" + Path.GetFileName(file);
                vertprof.file = Path.Combine(Path.GetDirectoryName(file), filename);
                vertprof.Show();
            }
        }

        //show vertical profile of a quantity of GRAMM online
        private void Vert3DConcentration(PointD TestPt)
        {
            if (VerticalProfileForm == null)
            {
                VerticalProfileForm = new VerticalProfileConcentration(this);
            }
            if (VerticalProfileForm != null)
            {
                VerticalProfileForm.filename = Path.Combine(Gral.Main.ProjectName, "Computation", "Vertical_Concentrations.txt");
                VerticalProfileForm.X = TestPt.X;
                VerticalProfileForm.Y = TestPt.Y;
                VerticalProfileForm.Init();
                VerticalProfileForm.Show();
            }
        }

        //show vertical profile of GRAMM windfields
        /// <summary>
        /// This method is used to start the vertical wind profile window
        /// </summary>
        private void VertProfile2(PointD TestPt)
        {
            try
            {
                if (ProfileConcentration != null)
                {
                    if (ProfileConcentration.GRALorGRAMM < 2) // GRAMM
                    {
                        VerticalProfileGramm(ProfileConcentration.DispersionSituation, TestPt);
                    }
                    else
                    {
                        VerticalProfileGral(ProfileConcentration.DispersionSituation, TestPt);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Start the vertical wind profile GRAMM window
        /// </summary>
        private async void VerticalProfileGramm(int dissit, PointD TestPt)
        {
            try
            {
                //get cell indices of GRAMM grid
                string file1 = Path.Combine(Gral.Main.ProjectName, @"Maps", "Vertical_Profile_Velocity.txt");
                string file2 = Path.Combine(Gral.Main.ProjectName, @"Maps", "Vertical_Profile_Direction.txt");

                int x1 = 1;
                int y1 = 1;
                if (MainForm.textBox13.Text != "")
                {
                    x1 = (int) ((TestPt.X - MainForm.GrammDomRect.West) / MainForm.GRAMMHorGridSize) + 1;
                    y1 = (int) ((TestPt.Y - MainForm.GrammDomRect.South) / MainForm.GRAMMHorGridSize) + 1;
                    if (TestPt.X < MainForm.GrammDomRect.West || TestPt.Y < MainForm.GrammDomRect.South)
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    x1 = (int) ((TestPt.X - Convert.ToInt32(MainForm.textBox6.Text)) / (double) MainForm.numericUpDown10.Value) + 1;
                    y1 = (int) ((TestPt.Y - Convert.ToInt32(MainForm.textBox5.Text)) / (double) MainForm.numericUpDown10.Value) + 1;
                }

                //Cursor = Cursors.WaitCursor;
                GralMessage.MessageWindow message = new GralMessage.MessageWindow();
                message.Show();
                message.listBox1.Items.Add("Compute vertical profile...");
                message.Refresh();

                //reading geometry file "ggeom.asc"
                GGeomFileIO ggeom = new GGeomFileIO
                {
                    PathWindfield = Path.GetDirectoryName(MainForm.GRAMMwindfield)
                };
                double[,] AH = new double[1, 1];
                double[,,] ZSP = new double[1, 1, 1];
                int NX = 1;
                int NY = 1;
                int NZ = 1;
                string[] text = new string[1];
                message.listBox1.Items.Add("Reading ggeom.asc...");
                message.Refresh();
                if (ggeom.ReadGGeomAsc(0) == true)
                {
                    NX = ggeom.NX;
                    NY = ggeom.NY;
                    NZ = ggeom.NZ;
                    AH = ggeom.AH;
                    ZSP = ggeom.ZSP;
                    ggeom = null;
                }
                else
                {
                    throw new FileNotFoundException("Error reading ggeom.asc");
                }

                //read wind fields
                message.listBox1.Items.Add("Reading wind field data...");
                message.Refresh();
                float[,,] UWI = new float[NX + 1, NY + 1, NZ + 1];
                float[,,] VWI = new float[NX + 1, NY + 1, NZ + 1];
                float[,,] WWI = new float[NX + 1, NY + 1, NZ + 1];
                try
                {
                    string wndfilename = Path.Combine(Path.GetDirectoryName(MainForm.GRAMMwindfield), Convert.ToString(dissit).PadLeft(5, '0') + ".wnd");
                    Windfield_Reader Reader = new Windfield_Reader();


                    if (await System.Threading.Tasks.Task.Run(() => Reader.Windfield_read(wndfilename, NX, NY, NZ, ref UWI, ref VWI, ref WWI)) == false)
                    {
                        throw new IOException();
                    }

                }
                catch
                {
                    MessageBox.Show(this, "Unable to read wind field", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                message.Close();
                //write text files with vertical profiles
                using (StreamWriter write = new StreamWriter(file1))
                {
                    for (int k = 1; k <= NZ; k++)
                    {
                        text[0] = Convert.ToString(Math.Round(ZSP[x1, y1, k] - AH[x1, y1], 1), ic) + " " + Convert.ToString(Math.Round(Math.Pow(Math.Pow(UWI[x1, y1, k], 2) + Math.Pow(VWI[x1, y1, k], 2), 0.5), 2), ic); ;
                        write.WriteLine(text[0]);
                    }
                }

                //show vertical profile
                if (ProfileConcentration.VertProfileVelocity != null)
                {
                    ProfileConcentration.VertProfileVelocity.Close();
                }
                ProfileConcentration.VertProfileVelocity = new VerticalProfile_Static();
                ProfileConcentration.VertProfileVelocity.Closing += new System.ComponentModel.CancelEventHandler(VertProfileVelocityClosing);
                ProfileConcentration.VertProfileVelocity.file = file1;
                ProfileConcentration.VertProfileVelocity.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                ProfileConcentration.VertProfileVelocity.Location = new Point(this.Left + 50, this.Top + 100);
                ProfileConcentration.VertProfileVelocity.Show();

                //write text files with vertical profiles
                double windrichtung;
                using (StreamWriter write = new StreamWriter(file2))
                {
                    for (int k = 1; k <= NZ; k++)
                    {
                        if (VWI[x1, y1, k] == 0)
                        {
                            windrichtung = 90;
                        }
                        else
                        {
                            windrichtung = Convert.ToInt32(Math.Abs(Math.Atan(UWI[x1, y1, k] / VWI[x1, y1, k])) * 180 / 3.14);
                        }

                        if ((VWI[x1, y1, k] > 0) && (UWI[x1, y1, k] <= 0))
                        {
                            windrichtung = 180 - windrichtung;
                        }

                        if ((VWI[x1, y1, k] >= 0) && (UWI[x1, y1, k] > 0))
                        {
                            windrichtung = 180 + windrichtung;
                        }

                        if ((VWI[x1, y1, k] < 0) && (UWI[x1, y1, k] >= 0))
                        {
                            windrichtung = 360 - windrichtung;
                        }

                        text[0] = Convert.ToString(Math.Round(ZSP[x1, y1, k] - AH[x1, y1], 1), ic) + " " + Convert.ToString(Math.Round(windrichtung, 0));
                        write.WriteLine(text[0]);
                    }
                }

                //show vertical profile
                if (ProfileConcentration.VertProfileDirection != null)
                {
                    ProfileConcentration.VertProfileDirection.Close();
                }
                ProfileConcentration.VertProfileDirection = new VerticalProfile_Static();
                ProfileConcentration.VertProfileDirection.Closing += new System.ComponentModel.CancelEventHandler(VertProfileDirectionClosing);
                ProfileConcentration.VertProfileDirection.file = file2;
                ProfileConcentration.VertProfileDirection.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                ProfileConcentration.VertProfileDirection.Location = new Point(this.Left + 350, this.Top + 100);
                ProfileConcentration.VertProfileDirection.Show();
            }
            catch { }
        }

        private void VertProfileVelocityClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProfileConcentration.VertProfileVelocity = null;
        }
        private void VertProfileDirectionClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProfileConcentration.VertProfileVelocity = null;
        }

        /// <summary>
        /// Start the vertical wind profile GRAL window
        /// </summary>
        private async void VerticalProfileGral(int dissit, PointD TestPt)
        {
            try
            {
                if (TestPt.X < MainForm.GralDomRect.West || TestPt.Y < MainForm.GralDomRect.South)
                {
                    throw new Exception();
                }

                //get cell indices of GRAL grid
                string file1 = Path.Combine(Gral.Main.ProjectName, @"Maps", "Vertical_Profile_Velocity.txt");
                string file2 = Path.Combine(Gral.Main.ProjectName, @"Maps", "Vertical_Profile_Direction.txt");

                string[] text = new string[1];
                float flowfieldraster = Convert.ToSingle(MainForm.numericUpDown10.Value);
                int x1 = (int) ((TestPt.X - MainForm.GralDomRect.West) / flowfieldraster) + 1;
                int y1 = (int) ((TestPt.Y - MainForm.GralDomRect.South) / flowfieldraster) + 1;

                //Cursor = Cursors.WaitCursor;

                //obtain surface heights
                Single[,] AH = new Single[1, 1];
                Single[,] Building_heights = new Single[1, 1];
                int[,] KKart = new int[1, 1];
                Int32 nkk = 0;
                Int32 njj = 0;
                Int32 nii = 0;
                int GRALwest = 0;
                int GRALsued = 0;
                Single DZK = 1;
                Single stretch = 1;
                Single AHMIN = 0;
                List<float[]> StretchFlexible = new List<float[]>();

                //reading geometry file "GRAL_geometries.txt"
                try
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(Path.Combine(Gral.Main.ProjectName, @"Computation", "GRAL_geometries.txt"), FileMode.Open)))
                    {
                        nkk = reader.ReadInt32();
                        njj = reader.ReadInt32();
                        nii = reader.ReadInt32();
                        GRALwest = reader.ReadInt32();
                        GRALsued = reader.ReadInt32();
                        DZK = reader.ReadSingle();
                        stretch = reader.ReadSingle();
                        if (stretch < 0.1)
                        {
                            int sliceCount = reader.ReadInt32();

                            for (int i = 0; i < sliceCount; i++)
                            {
                                StretchFlexible.Add(new float[2]);
                                StretchFlexible[i][0] = reader.ReadSingle(); // Height
                                StretchFlexible[i][1] = reader.ReadSingle(); // Stretch
                            }
                        }
                        AHMIN = reader.ReadSingle();

                        if (x1 < 1 || y1 < 1 || x1 > nii || y1 > njj)
                        {
                            return;
                        }

                        AH = new Single[nii + 2, njj + 2];
                        KKart = new int[nii + 2, njj + 2];
                        Building_heights = new Single[nii + 2, njj + 2];

                        for (int i = 1; i <= nii + 1; i++)
                        {
                            for (int j = 1; j <= njj + 1; j++)
                            {
                                AH[i, j] = reader.ReadSingle();
                                KKart[i, j] = (short)reader.ReadInt32();
                                Building_heights[i, j] = reader.ReadSingle();
                            }
                        }
                    }
                }

                catch
                {
                    MessageBox.Show("Error reading GRAL geometries.txt");
                }

                //windfield file Readers
                float[][][] uk;
                float[][][] vk;
                //float[][][] wk;
                Single GRAL_cellsize_ff = 0;

                //reading *.gff-file
                ReadFlowFieldFiles gff = new ReadFlowFieldFiles
                {
                    filename = Path.Combine(GralStaticFunctions.St_F.GetGffFilePath(Path.Combine(Gral.Main.ProjectName, "Computation")),
                                                                                    Convert.ToString(dissit).PadLeft(5, '0') + ".gff")
                };

                CancellationTokenReset();
                if (gff.ReadGffFile(CancellationTokenSource.Token) == true)
                {
                    nkk = gff.NKK;
                    njj = gff.NJJ;
                    nii = gff.NII;
                    uk = gff.Uk;
                    vk = gff.Vk;
                    //wk = gff.wk;
                    GRAL_cellsize_ff = gff.Cellsize;
                    gff = null;
                }
                else
                {
                    if (CancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                    MessageBox.Show("Error reading windfields", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Cursor = Cursors.Default;
                    return;
                }

                //computation of slice
                Single[] HOKART = new Single[nkk + 1];  //height of vertical layers starting from zero
                Single DZKdummy = DZK;
                int flexstretchindex = 0;
                float stretching = 1;

                for (int k = 1; k <= nkk; k++)
                {
                    HOKART[k] = HOKART[k - 1] + DZKdummy;

                    if (stretch > 0.99)
                    {
                        DZKdummy *= stretch;
                    }
                    else
                    {
                        if (flexstretchindex < StretchFlexible.Count - 1)
                        {
                            if (StretchFlexible[flexstretchindex + 1][1] > 0.99 &&
                                HOKART[k - 1] > StretchFlexible[flexstretchindex + 1][0])
                            {
                                stretching = StretchFlexible[flexstretchindex + 1][1];
                                flexstretchindex++;
                            }
                        }
                        DZKdummy *= stretching;
                    }
                }

                AHMIN = Math.Abs(AHMIN);

                //write text files with vertical profiles
                using (StreamWriter write = new StreamWriter(file1))
                {
                    for (int k = 1; k <= nkk; k++)
                    {
                        if (HOKART[k] + AHMIN >= AH[x1, y1] - Building_heights[x1, y1]) //check if point is above ground level
                        {
                            text[0] = Convert.ToString(Math.Round(HOKART[k] + AHMIN - AH[x1, y1] + Building_heights[x1, y1], 1), ic)
                                + " " + Convert.ToString(Math.Round(Math.Pow(Math.Pow(uk[x1][y1][k], 2) + Math.Pow(vk[x1][y1][k], 2), 0.5), 2), ic);
                            write.WriteLine(text[0]);
                        }
                    }
                }

                //show vertical profile
                if (ProfileConcentration.VertProfileVelocity != null)
                {
                    ProfileConcentration.VertProfileVelocity.Close();
                }
                ProfileConcentration.VertProfileVelocity = new VerticalProfile_Static();
                ProfileConcentration.VertProfileVelocity.Closing += new System.ComponentModel.CancelEventHandler(VertProfileVelocityClosing);
                ProfileConcentration.VertProfileVelocity.file = file1;
                ProfileConcentration.VertProfileVelocity.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                ProfileConcentration.VertProfileVelocity.Location = new Point(this.Left + 50, this.Top + 100);
                ProfileConcentration.VertProfileVelocity.Show();

                //write text files with vertical profiles
                double windrichtung;
                using (StreamWriter write = new StreamWriter(file2))
                {
                    for (int k = 1; k <= nkk; k++)
                    {
                        if (HOKART[k] + AHMIN >= AH[x1, y1] - Building_heights[x1, y1]) //check if point is above ground level
                        {
                            if (vk[x1][y1][k] == 0)
                            {
                                windrichtung = 90;
                            }
                            else
                            {
                                windrichtung = Convert.ToInt32(Math.Abs(Math.Atan(uk[x1][y1][k] / vk[x1][y1][k])) * 180 / 3.14);
                            }

                            if ((vk[x1][y1][k] > 0) && (uk[x1][y1][k] <= 0))
                            {
                                windrichtung = 180 - windrichtung;
                            }

                            if ((vk[x1][y1][k] >= 0) && (uk[x1][y1][k] > 0))
                            {
                                windrichtung = 180 + windrichtung;
                            }

                            if ((vk[x1][y1][k] < 0) && (uk[x1][y1][k] >= 0))
                            {
                                windrichtung = 360 - windrichtung;
                            }

                            text[0] = Convert.ToString(Math.Round(HOKART[k] + AHMIN - AH[x1, y1] + Building_heights[x1, y1], 1), ic)
                                + " " + Convert.ToString(Math.Round(windrichtung, 0));
                            write.WriteLine(text[0]);
                        }
                    }
                }

                //show vertical profile
                if (ProfileConcentration.VertProfileDirection != null)
                {
                    ProfileConcentration.VertProfileDirection.Close();
                }
                ProfileConcentration.VertProfileDirection = new VerticalProfile_Static();
                ProfileConcentration.VertProfileDirection.Closing += new System.ComponentModel.CancelEventHandler(VertProfileDirectionClosing);
                ProfileConcentration.VertProfileDirection.file = file2;
                ProfileConcentration.VertProfileDirection.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                ProfileConcentration.VertProfileDirection.Location = new Point(this.Left + 350, this.Top + 100);
                ProfileConcentration.VertProfileDirection.Show();

            }
            catch { }
        }

        //input box to select quantity for vertical profile of GRAMM online analysis
        public static DialogResult InputBox2(string title, string promptText, ref string file)
        {
            DialogResult dialogResult = DialogResult.Cancel;
            using (Form form = new Form())
            {
                Label label = new Label();
                ComboBox combo = new ComboBox();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();

                form.Text = title;
                label.Text = promptText;

                combo.Items.Add("Wind-speed");
                combo.Items.Add("u-wind-component");
                combo.Items.Add("v-wind-component");
                combo.Items.Add("w-wind-component");
                combo.Items.Add("Temperature");
                combo.Items.Add("potential-Temperature");
                combo.Items.Add("Humidity");
                combo.Items.Add("non-hydr-pressure");
                combo.Items.Add("Turbulent-kinetic-energy");
                combo.Items.Add("Dissipation");
                combo.SelectedIndex = 0;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 20, 372, 13);
                combo.SetBounds(12, 36, 372, 20);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                combo.Anchor |= AnchorStyles.Right;
                buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new Control[] { label, combo, buttonOk, buttonCancel });
                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                dialogResult = form.ShowDialog();
                //file = combo.SelectedText;
                file = Convert.ToString(combo.Items[combo.SelectedIndex]);
            }
            return dialogResult;
        }
        
        private void FileWatcherError(object source, ErrorEventArgs e)
        {
            OnlineCounter = -1;
        }
    }
}