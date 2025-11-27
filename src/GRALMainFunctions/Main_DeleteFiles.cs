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

using GralMessage;
using System;
using System.IO;
using System.Windows.Forms;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
    {
        /// <summary>
        /// deletes all intermediate GRAL *.con and *.odr files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRALDeleteConcentrationFiles(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ProjectName))
            {
                return; // exit if no project loaded
            }

            try
            {
                string newPath = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                DirectoryInfo di = new DirectoryInfo(newPath);
                FileInfo[] files_conc = di.GetFiles("*.con");
                if (files_conc.Length > 0)
                {
                    using (FileDeleteMessage fdm = new FileDeleteMessage())
                    {
                        System.Collections.Generic.List<string> _message = new System.Collections.Generic.List<string>();
                        _message.Add("..Computation" + Path.DirectorySeparatorChar + "*.con");
                        fdm.ListboxEntries = _message;
                        fdm.DeleteMessage = "Delete all GRAL concentration result files?";

                        if (fdm.ShowDialog() == DialogResult.OK)
                        {
                            if (DeleteFiles(files_conc))
                            {
                                return;
                            }
                            Project_Locked = false;               // unlock project
                            ProjectLockedButtonClick(null, null); // change locked-Button
                            DeleteTempGralFiles();
                        }
                    }
                }

                files_conc = di.GetFiles("*.grz"); //compressed files ?
                if (files_conc.Length > 0)
                {
                    using (FileDeleteMessage fdm = new FileDeleteMessage())
                    {
                        System.Collections.Generic.List<string> _message = new System.Collections.Generic.List<string>();
                        _message.Add("..Computation" + Path.DirectorySeparatorChar + "*.grz");
                        fdm.ListboxEntries = _message;
                        fdm.DeleteMessage = "Delete all GRAL concentration result files?";

                        if (fdm.ShowDialog() == DialogResult.OK)
                        {
                            if (DeleteFiles(files_conc))
                            {
                                return;
                            }
                            Project_Locked = false;                 // unlock project
                            ProjectLockedButtonClick(null, null); // change locked-Button
                            DeleteTempGralFiles();
                        }
                    }
                }

                files_conc = di.GetFiles("*.odr");
                if (files_conc.Length > 0)
                {
                    using (FileDeleteMessage fdm = new FileDeleteMessage())
                    {
                        System.Collections.Generic.List<string> _message = new System.Collections.Generic.List<string>();
                        _message.Add("..Computation" + Path.DirectorySeparatorChar + "*.odr");
                        fdm.ListboxEntries = _message;
                        fdm.DeleteMessage = "Delete all GRAL odour result files?";

                        if (fdm.ShowDialog() == DialogResult.OK)
                        {
                            if (DeleteFiles(files_conc))
                            {
                                return;
                            }
                            Project_Locked = false;                 // unlock project
                            ProjectLockedButtonClick(null, null); // change locked-Button
                            DeleteTempGralFiles();
                        }
                    }
                }

                files_conc = di.GetFiles("*.dep");
                if (files_conc.Length > 0)
                {
                    using (FileDeleteMessage fdm = new FileDeleteMessage())
                    {
                        System.Collections.Generic.List<string> _message = new System.Collections.Generic.List<string>();
                        _message.Add("..Computation" + Path.DirectorySeparatorChar + "*.dep");
                        fdm.ListboxEntries = _message;
                        fdm.DeleteMessage = "Delete all GRAL deposition result files?";

                        if (fdm.ShowDialog() == DialogResult.OK)
                        {
                            if (DeleteFiles(files_conc))
                            {
                                return;
                            }
                            Project_Locked = false;                 // unlock project
                            ProjectLockedButtonClick(null, null); // change locked-Button
                            DeleteTempGralFiles();
                        }
                    }
                }

                newPath = Path.Combine(ProjectName, "Computation", "DispNr.txt");
                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                }

                newPath = Path.Combine(ProjectName, "Computation", "Percent.txt");
                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                }

                DispNrChanged(null, null); // change displaynumber
                UpdateFileSize(null, null);
            }
            catch
            { }
        }

        /// <summary>
        /// Delete file to recycle bin or completely
        /// </summary>
        /// <param name="File">One file string full path</param>
        private void DeleteFile(string FileName)
        {
#if __MonoCS__
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }
#else
            if (Main.GUISettings.DeleteFilesToRecyclingBin)
            {
                if (File.Exists(FileName))
                {
                    GralStaticFunctions.St_F.FileDeleteRecyclingBin(FileName);
                }
            }
            else
            {
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
            }
#endif
        }

        /// <summary>
        /// Delete FileInfo[] files to recycle bin or completely
        /// </summary>
        /// <param name="Files"></param>
        private bool DeleteFiles(FileInfo[] Files)
        {
            bool cancel = false;
            Cursor.Current = Cursors.WaitCursor;
#if __MonoCS__
            for (int i = 0; i < Files.Length; i++)
            {
                File.Delete(Files[i].FullName);
            }
#else
            if (Main.GUISettings.DeleteFilesToRecyclingBin)
            {
                System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                WaitProgressbarCancel wait = new WaitProgressbarCancel("Move files to the recycling bin", ref cts);
                wait.ProgressbarUpdate(this, Math.Max(2, Files.Length / 40 + Files.Length % 40));
                wait.Show();

                //Move multiple files to the recycling bin
                int i = 0;
                for (; i < Files.Length - 40; i += 40)
                {
                    string collect = "";
                    for (int j = 0; j < 40; j++)
                    {
                        collect += Files[i + j].FullName + '\0';
                    }
                    GralStaticFunctions.St_F.FileDeleteRecyclingBin(collect);
                    wait.ProgressbarUpdate(this, 0);
                    Application.DoEvents();
                    if (cts.IsCancellationRequested)
                    {
                        cancel = true;
                        break;
                    }
                }

                for (; i < Files.Length; i++)
                {
                    wait.ProgressbarUpdate(this, 0);
                    Application.DoEvents();
                    if (cts.IsCancellationRequested)
                    {
                        cancel = true;
                        break;
                    }
                    GralStaticFunctions.St_F.FileDeleteRecyclingBin(Files[i].FullName);
                }
                if (wait != null)
                {
                    wait.Close();
                }
                if (cts != null)
                {
                    cts.Dispose();
                }
            }
            else
            {
                for (int i = 0; i < Files.Length; i++)
                {
                    File.Delete(Files[i].FullName);
                }
            }
#endif
            Cursor.Current = Cursors.Default;
            return cancel;
        }

        private void DeleteTempGralFiles()
        {
            try
            {
                string file = Path.Combine(ProjectName, "Computation", "Transient_Concentrations1.tmp");
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                file = Path.Combine(ProjectName, "Computation", "Transient_Concentrations2.tmp");
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                file = Path.Combine(ProjectName, "Computation", "Vertical_Concentrations.tmp");
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch { }
        }

        /// <summary>
        /// deletes all GRAMM Windfield files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GRAMMDeleteWindFields(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ProjectName))
            {
                return; // exit if no project loaded
            }

            try
            {
                string newPath = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                DirectoryInfo di = new DirectoryInfo(newPath);

                FileInfo[] files_wnd = di.GetFiles("*.wnd");
                if (files_wnd.Length > 0)
                {
                    using (FileDeleteMessage fdm = new FileDeleteMessage
                    {
                        DeleteGramm = true
                    })
                    {
                        System.Collections.Generic.List<string> _message = new System.Collections.Generic.List<string>();
                        if (files_wnd.Length > 0)
                        {
                            _message.Add("..Computation" + Path.DirectorySeparatorChar + "*.wnd");
                        }
                        fdm.ListboxEntries = _message;

                        if (fdm.ShowDialog() == DialogResult.OK)
                        {
                            if (DeleteFiles(files_wnd))
                            {
                                return;
                            }
                            //delete *.scl files
                            files_wnd = di.GetFiles("*.scl");
                            if (files_wnd.Length > 0)
                            {
                                if (DeleteFiles(files_wnd))
                                {
                                    return;
                                }
                            }

                            //delete *.obl files
                            files_wnd = di.GetFiles("*.obl");
                            if (files_wnd.Length > 0)
                            {
                                if (DeleteFiles(files_wnd))
                                {
                                    return;
                                }
                            }

                            //delete *.ust files
                            files_wnd = di.GetFiles("*.ust");
                            if (files_wnd.Length > 0)
                            {
                                if (DeleteFiles(files_wnd))
                                {
                                    return;
                                }
                            }

                            //delete steady_state.txt files
                            files_wnd = di.GetFiles("?????_steady_state.txt");
                            if (files_wnd.Length > 0)
                            {
                                if (DeleteFiles(files_wnd))
                                {
                                    return;
                                }
                            }

                            GRAMM_Locked = false;                 // unlock GRAMM project
                            Gramm_locked_buttonClick(null, null); // change locked-Button
                        }
                    }
                }

                newPath = Path.Combine(ProjectName, "Computation", "DispNrGramm.txt");
                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                }

                DispnrGrammChanged(null, null); // change displaynumber
                UpdateFileSize(null, null); // actualize file size
            }
            catch
            { }
        }

        /// <summary>
        /// deletes all intermediate GRAL *.gff files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRALDeleteFlowFieldFiles(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ProjectName))
            {
                return; // exit if no project loaded
            }

            DeleteGralGffFile();
        }

        //delete file GRAL_topofile.txt whenever items are changed by the user affecting the topography
        /// <summary>
        /// Deletes the high resolution GRAL_topofile.txt
        /// </summary>
        public void DeleteGralTopofile()
        {
            try
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "GRAL_topofile.txt"));
            }
            catch
            { }
        }

        private void DeleteGralGeometries()
        {
            try
            {
                //File.Delete(Path.Combine(ProjectName, @"Computation", "GRAL_geometries.txt"));
                GralStaticFunctions.St_F.FileDeleteRecyclingBin(Path.Combine(ProjectName, @"Computation", "GRAL_geometries.txt"));
            }
            catch
            { }
        }

        //delete intermediate *gff-files whenever items are changed by the user affecting the flow field calculations
        /// <summary>
        /// Ask if files should be deleted and deletes all *.gff files and the file GRAL_geometries.txt
        /// </summary>
        /// <returns></returns>
        public DialogResult DeleteGralGffFile()
        {
            DialogResult dia = new DialogResult();
            dia = DialogResult.OK;

            string GffFilePath = GralStaticFunctions.St_F.GetGffFilePath(Path.Combine(ProjectName, "Computation")) + Path.DirectorySeparatorChar;

            DirectoryInfo di = new DirectoryInfo(GffFilePath);
            FileInfo[] files_conc = di.GetFiles("*.gff");

            if (files_conc.Length > 0)
            {
                using (FileDeleteMessage fdm = new FileDeleteMessage())
                {
                    System.Collections.Generic.List<string> _message = new System.Collections.Generic.List<string>();
                    if (files_conc.Length > 0)
                    {
                        _message.Add("..." + GffFilePath.Substring(Math.Max(0, GffFilePath.Length - 45)) + "*.gff");
                    }
                    fdm.ListboxEntries = _message;

                    if (fdm.ShowDialog() == DialogResult.OK)
                    {
                        DeleteFiles(files_conc);
                        DeleteGralGeometries();
                    }
                }
            }
            UpdateFileSize(null, null);
            return dia;
        }

        /// <summary>
        /// Delete Emission Timeseries.txt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DeleteEmissionTimeseriesFile(object sender, EventArgs e)
        {
            string newpath = Path.Combine(ProjectName, "Computation", "emissions_timeseries.txt");
            if (Directory.Exists(ProjectSetting.EmissionModulationPath))
            {
                newpath = Path.Combine(ProjectSetting.EmissionModulationPath, "emissions_timeseries.txt");
            }

            if (File.Exists(newpath) == true && MessageBox.Show(this, "Delete emissions_timeseries.txt ?",
                                                                "GRAL Message", MessageBoxButtons.OKCancel,
                                                                MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    File.Delete(newpath);
                    button49.Visible = false;
                    button51.Visible = false;
                    TabControl1Click(null, null);
                }
                catch { }
            }
        }
    }
}

