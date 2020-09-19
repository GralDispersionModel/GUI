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
using System.IO;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Load and define GRAMM Online settings
        /// </summary>
		public void Domain_Load_GRAMMOnline()
        {
            groupBox1.Visible = false;
            groupBox2.Visible = false;
            groupBox3.Visible = false;
            groupBox4.Visible = false;
            groupBox5.Visible = false;
            groupBox6.Visible = false;
            button6.Visible = false;
            button29.Visible = false;
            button26.Visible = false;
            button27.Visible = false;
            button28.Visible = false;
            button33.Visible = false;
            button35.Visible = false;
            button34.Visible = true;

            menuStrip1.Enabled = false;

            //start file watcher for uv_Gramm.txt
            if (MainForm.checkBox1.Checked == true)
            {
                FileWatch.UVGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "uv_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.UVGramm.Changed += new FileSystemEventHandler(AsyncUVGrammChanged);
                FileWatch.UVGramm.EnableRaisingEvents = true;
                FileWatch.UVGramm.Error += new ErrorEventHandler(FileWatcherError);

                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "uv_GRAMM.txt");

                //add vector map to object list
                DrawingObjects _drobj = new DrawingObjects("VM: " + Path.GetFileNameWithoutExtension(file));

                //compute values for 10 contours
                for (int i = 0; i < 10; i++)
                {
                    _drobj.ItemValues.Add(0);
                    _drobj.FillColors.Add(Color.Red);
                    _drobj.LineColors.Add(Color.Black);
                }
                _drobj.FillColors[0] = Color.Yellow;
                _drobj.LineColors[0] = Color.Black;

                double min = 0;
                double max = 10;
                _drobj.ItemValues[0] = min;
                _drobj.ItemValues[9] = max;
                //apply color gradient between yellow and red
                int r1 = _drobj.FillColors[0].R;
                int g1 = _drobj.FillColors[0].G;
                int b1 = _drobj.FillColors[0].B;
                int r2 = _drobj.FillColors[9].R;
                int g2 = _drobj.FillColors[9].G;
                int b2 = _drobj.FillColors[9].B;
                for (int i = 0; i < 8; i++)
                {
                    _drobj.ItemValues[i + 1] = min + (max - min) / 10 * Convert.ToDouble(i + 1);
                    int intr = r1 + (r2 - r1) / 10 * (i + 1);
                    int intg = g1 + (g2 - g1) / 10 * (i + 1);
                    int intb = b1 + (b2 - b1) / 10 * (i + 1);
                    _drobj.FillColors[i + 1] = Color.FromArgb(intr, intg, intb);
                }

                _drobj.LegendTitle = "Vectormap";
                _drobj.LegendUnit = "m/s";
                //
                //add list to save contourpoints
                //
                _drobj.ContourFilename = file;
                ItemOptions.Insert(0, _drobj);
            }
            else
            {
                if (FileWatch.UVGramm != null)
                {
                    FileWatch.UVGramm.Dispose();
                }
                FileWatch.UVGramm = null;
            }
            //start file watcher for u_Gramm.txt
            if (MainForm.checkBox2.Checked == true)
            {
                FileWatch.UGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "u_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.UGramm.Changed += new FileSystemEventHandler(AsyncUGrammChanged);
                FileWatch.UGramm.EnableRaisingEvents = true;
                FileWatch.UGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "u_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.UGramm != null)
                {
                    FileWatch.UGramm.Dispose();
                }
                FileWatch.UGramm = null;
            }
            //start file watcher for speed_Gramm.txt
            if (MainForm.checkBox16.Checked == true)
            {
                FileWatch.SpeedGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "speed_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.SpeedGramm.Changed += new FileSystemEventHandler(AsyncSpeedGrammChanged);
                FileWatch.SpeedGramm.EnableRaisingEvents = true;
                FileWatch.SpeedGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "speed_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                FileWatch.SpeedGramm = null;
            }
            //start file watcher for v_Gramm.txt
            if (MainForm.checkBox3.Checked == true)
            {
                FileWatch.VGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "v_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.VGramm.Changed += new FileSystemEventHandler(AsyncVGrammChanged);
                FileWatch.VGramm.EnableRaisingEvents = true;
                FileWatch.VGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "v_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.VGramm != null)
                {
                    FileWatch.VGramm.Dispose();
                }
                FileWatch.VGramm = null;
            }
            //start file watcher for w_Gramm.txt
            if (MainForm.checkBox4.Checked == true)
            {
                FileWatch.WGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "w_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.WGramm.Changed += new FileSystemEventHandler(AsyncWGrammChanged);
                FileWatch.WGramm.EnableRaisingEvents = true;
                FileWatch.WGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "w_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.WGramm != null)
                {
                    FileWatch.WGramm.Dispose();
                }
                FileWatch.WGramm = null;
            }
            //start file watcher for tabs_Gramm.txt
            if (MainForm.checkBox5.Checked == true)
            {
                FileWatch.TAbsGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "tabs_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.TAbsGramm.Changed += new FileSystemEventHandler(AsyncTabsGrammChanged);
                FileWatch.TAbsGramm.EnableRaisingEvents = true;
                FileWatch.TAbsGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "tabs_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.TAbsGramm != null)
                {
                    FileWatch.TAbsGramm.Dispose();
                }
                FileWatch.TAbsGramm = null;
            }
            //start file watcher for tpot_Gramm.txt
            if (MainForm.checkBox6.Checked == true)
            {
                FileWatch.TPotGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "tpot_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.TPotGramm.Changed += new FileSystemEventHandler(AsyncTpotGrammChanged);
                FileWatch.TPotGramm.EnableRaisingEvents = true;
                FileWatch.TPotGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "tpot_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.TPotGramm != null)
                {
                    FileWatch.TPotGramm.Dispose();
                }
                FileWatch.TPotGramm = null;
            }
            //start file watcher for hum_Gramm.txt
            if (MainForm.checkBox7.Checked == true)
            {
                FileWatch.HumGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "hum_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.HumGramm.Changed += new FileSystemEventHandler(AsyncHumGrammChanged);
                FileWatch.HumGramm.EnableRaisingEvents = true;
                FileWatch.HumGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "hum_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.HumGramm != null)
                {
                    FileWatch.HumGramm.Dispose();
                }
                FileWatch.HumGramm = null;
            }
            //start file watcher for nhp_Gramm.txt
            if (MainForm.checkBox8.Checked == true)
            {
                FileWatch.NhpGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "nhp_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.NhpGramm.Changed += new FileSystemEventHandler(AsyncNhpGrammChanged);
                FileWatch.NhpGramm.EnableRaisingEvents = true;
                FileWatch.NhpGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "nhp_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.NhpGramm != null)
                {
                    FileWatch.NhpGramm.Dispose();
                }
                FileWatch.NhpGramm = null;
            }
            //start file watcher for glob_Gramm.txt
            if (MainForm.checkBox9.Checked == true)
            {
                FileWatch.GlobGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "glob_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.GlobGramm.Changed += new FileSystemEventHandler(AsyncGlobGrammChanged);
                FileWatch.GlobGramm.EnableRaisingEvents = true;
                FileWatch.GlobGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "glob_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.GlobGramm != null)
                {
                    FileWatch.GlobGramm.Dispose();
                }
                FileWatch.GlobGramm = null;
            }
            //start file watcher for terr_Gramm.txt
            if (MainForm.checkBox10.Checked == true)
            {
                FileWatch.TerrGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "terr_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.TerrGramm.Changed += new FileSystemEventHandler(AsyncTerrGrammChanged);
                FileWatch.TerrGramm.EnableRaisingEvents = true;
                FileWatch.TerrGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "terr_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.TerrGramm != null)
                {
                    FileWatch.TerrGramm.Dispose();
                }
                FileWatch.TerrGramm = null;
            }
            //start file watcher for sensheat_Gramm.txt
            if (MainForm.checkBox11.Checked == true)
            {
                FileWatch.SensHeatGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "sensheat_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.SensHeatGramm.Changed += new FileSystemEventHandler(AsyncSensheatGrammChanged);
                FileWatch.SensHeatGramm.EnableRaisingEvents = true;
                FileWatch.SensHeatGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "sensheat_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.SensHeatGramm != null)
                {
                    FileWatch.SensHeatGramm.Dispose();
                }
                FileWatch.SensHeatGramm = null;
            }
            //start file watcher for latheat_Gramm.txt
            if (MainForm.checkBox12.Checked == true)
            {
                FileWatch.LatHeatGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "latheat_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.LatHeatGramm.Changed += new FileSystemEventHandler(AsyncLatheatGrammChanged);
                FileWatch.LatHeatGramm.EnableRaisingEvents = true;
                FileWatch.LatHeatGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "latheat_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.LatHeatGramm != null)
                {
                    FileWatch.LatHeatGramm.Dispose();
                }
                FileWatch.LatHeatGramm = null;
            }
            //start file watcher for latheat_Gramm.txt
            if (MainForm.checkBox13.Checked == true)
            {
                FileWatch.VricVelGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "fricvel_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.VricVelGramm.Changed += new FileSystemEventHandler(AsyncFricvelGrammChanged);
                FileWatch.VricVelGramm.EnableRaisingEvents = true;
                FileWatch.VricVelGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "fricvel_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.VricVelGramm != null)
                {
                    FileWatch.VricVelGramm.Dispose();
                }
                FileWatch.VricVelGramm = null;
            }
            //start file watcher for inverseMO_Gramm.txt
            if (MainForm.checkBox14.Checked == true)
            {
                FileWatch.InverseMOGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "inverseMO_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.InverseMOGramm.Changed += new FileSystemEventHandler(AsyncInverseMOGrammChanged);
                FileWatch.InverseMOGramm.EnableRaisingEvents = true;
                FileWatch.InverseMOGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "inverseMO_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.InverseMOGramm != null)
                {
                    FileWatch.InverseMOGramm.Dispose();
                }
                FileWatch.InverseMOGramm = null;
            }
            //start file watcher for surfTemp_Gramm.txt
            if (MainForm.checkBox15.Checked == true)
            {
                FileWatch.SurfTempGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "surfTemp_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.SurfTempGramm.Changed += new FileSystemEventHandler(AsyncSurfTempGrammChanged);
                FileWatch.SurfTempGramm.EnableRaisingEvents = true;
                FileWatch.SurfTempGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "surfTemp_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.SurfTempGramm != null)
                {
                    FileWatch.SurfTempGramm.Dispose();
                }
                FileWatch.SurfTempGramm = null;
            }
            //start file watcher for stabilityclass_Gramm.txt
            if (MainForm.checkBox28.Checked == true)
            {
                FileWatch.StabClassGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "stabilityclass_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.StabClassGramm.Changed += new FileSystemEventHandler(AsyncStabilityclassGrammChanged);
                FileWatch.StabClassGramm.EnableRaisingEvents = true;
                FileWatch.StabClassGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "stabilityclass_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.StabClassGramm != null)
                {
                    FileWatch.StabClassGramm.Dispose();
                }
                FileWatch.StabClassGramm = null;
            }
            //start file watcher for tke_Gramm.txt
            if (MainForm.checkBox17.Checked == true)
            {
                FileWatch.TkeGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "tke_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.TkeGramm.Changed += new FileSystemEventHandler(AsyncTKEGrammChanged);
                FileWatch.TkeGramm.EnableRaisingEvents = true;
                FileWatch.TkeGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "tke_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.TkeGramm != null)
                {
                    FileWatch.TkeGramm.Dispose();
                }
                FileWatch.TkeGramm = null;
            }
            //start file watcher for dis_Gramm.txt
            if (MainForm.checkBox18.Checked == true)
            {
                FileWatch.DisGramm = new FileSystemWatcher
                {
                    Path = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar),
                    Filter = "dis_GRAMM.txt",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileWatch.DisGramm.Changed += new FileSystemEventHandler(AsyncDisGrammChanged);
                FileWatch.DisGramm.EnableRaisingEvents = true;
                FileWatch.DisGramm.Error += new ErrorEventHandler(FileWatcherError);
                string file = Path.Combine(Gral.Main.ProjectName, @"Computation", "dis_GRAMM.txt");
                AddPostMap(file);
            }
            else
            {
                if (FileWatch.DisGramm != null)
                {
                    FileWatch.DisGramm.Dispose();
                }
                FileWatch.DisGramm = null;
            }
        }
    }
}