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
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using GralIO;
using System.Diagnostics;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
    {
        /// <summary>
        /// Start GRAL simulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRALStartCalculation(object sender, EventArgs e)
        {
            if (Convert.ToString(listBox5.SelectedItem).Contains("Odour")) // check if the lowest conc. layer > 1.5 * vert. extension
            {
                if (Convert.ToDouble(TBox3[0].Value) < 1.5 * Convert.ToDouble(numericUpDown8.Value))
                {
                    if (MessageBox.Show(
                        "Lowest heigth above ground < 1.5 * vertical dimension" + Environment.NewLine +
                        "The odour concentration-variance model is not available" + Environment.NewLine +
                        "Start calculation although?", "GRAL GUI",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        return;
                    }
                }
            }

            //get information about the maximum number of dispersion situations for the progress bar
            IO_ReadFiles ReadFile = new IO_ReadFiles
            {
                ProjectName = ProjectName,
                DispsituationFrequ = DispSituationfrequ
            };

            //check if GRAL is operated in transient mode
            int transient = 1; // steady state mode
            int weathersit_count = 1;
            try
            {
                InDatVariables data = new InDatVariables();
                InDatFileIO ReadInData = new InDatFileIO();
                data.InDatPath = Path.Combine(ProjectName, "Computation", "in.dat");
                ReadInData.Data = data;
                if (ReadInData.ReadInDat() == true)
                {
                    if (data.Transientflag == 0)
                    {
                        transient = 0; // transient mode
                        //read mettimeseries.dat
                        string mettimeseries = Path.Combine(ProjectName, "Computation", "mettimeseries.dat");
                        List<string> data_mettimeseries = new List<string>();
                        ReadMetTimeSeries(mettimeseries, ref data_mettimeseries);
                        weathersit_count = Math.Max(data_mettimeseries.Count, 1);
                        //if (data_mettimeseries.Count == 0) // no data available
                        //{
                        //	MessageBox.Show("mettimeseries.dat not available -> correct visualization of the simulation progress not possible");
                        //}
                    }
                }
            }
            catch
            { }

            if (transient == 1)
            {
                if (ReadFile.ReadMeteopgtAllFile() == true)
                {
                    if (ReadFile.MeteoFrequ > 0)
                    {
                        progressBar4.Maximum = 100;
                    }

                    DispSituationfrequ = ReadFile.DispsituationFrequ;
                }
            }
            else
            {
                if (weathersit_count > 0)
                {
                    progressBar4.Maximum = Convert.ToInt32(weathersit_count);
                }
                DispSituationfrequ = ReadFile.DispsituationFrequ;
            }

            ReadFile = null;

            //search for GRAL executables
            string[] filePaths = Directory.GetFiles(Path.Combine(ProjectName, "Computation"), "GRAL*.exe", SearchOption.TopDirectoryOnly);
            string dir;
            if (filePaths.Length > 0)
            {
                dir = Path.GetDirectoryName(filePaths[0]);
            }
            else
            {
#if __MonoCS__
                filePaths = Directory.GetFiles(Path.Combine(ProjectName, "Computation"), "GRAL*.dll", SearchOption.TopDirectoryOnly);
                if (filePaths.Length == 0)
                {
                    filePaths = Directory.GetFiles(Path.Combine(ProjectName, "Computation"), "GRAL", SearchOption.TopDirectoryOnly);
                }
#else
                filePaths = Directory.GetFiles(Path.Combine (ProjectName, "Computation"), "GRAL*.bat", SearchOption.TopDirectoryOnly);
                #endif

                if (filePaths.Length > 0)
                {
                    dir = Path.GetDirectoryName(filePaths[0]);
                }
                else
                {
                    dir = Path.GetDirectoryName(Application.ExecutablePath);
                }
            }

            OpenFileDialog dialog = new OpenFileDialog();
            #if __MonoCS__
            dialog.Filter = "GRAL executables (GRAL*.dll;GRAL)|GRAL*.dll;GRAL";
            #else
            dialog.Filter = "GRAL executables (GRAL*.exe;GRAL*.bat)|GRAL*.exe;GRAL*.bat";
            #endif

            dialog.Title = "Select GRAL executable";
            // dialog.ShowHelp = true;

            if (GUISettings.CopyCoresToProject)
            {
                dialog.InitialDirectory = dir;
            }
            else
            {
                dialog.InitialDirectory = Main.GUISettings.DefaultPathForGRAL;
            }
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogExe;
#endif

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string GRAL_Program_Path = dialog.FileName;
                    string GRAL_Project_Path = String.Empty;

                    if (GUISettings.CopyCoresToProject) // Copy the computation core to the project folder?
                    {
                        //copy the file GRAL*.exe in the directory "Computation"
                        GRAL_Program_Path = Path.Combine(ProjectName, @"Computation", Path.GetFileName(dialog.FileName));
                        try
                        {
                            File.Copy(dialog.FileName, GRAL_Program_Path, true);
                            
                            //copy System.Numerics.Vectors.dll for GRAL Version 19.01
                            string numerics_dll = Path.Combine(Path.GetDirectoryName(dialog.FileName), "System.Numerics.Vectors.dll");
                            if (Path.GetExtension(dialog.FileName) == ".exe" && File.Exists(numerics_dll))
                            {
                                string numerics_dest = Path.Combine(ProjectName, @"Computation", Path.GetFileName(numerics_dll));
                                File.Copy(numerics_dll, numerics_dest, true);
                            }

                            string batch = String.Empty;
                            #if __MonoCS__
                            if (Path.GetExtension(dialog.FileName).ToLower() == ".dll")
                                batch = "GRAL*.dll";
                            #else
                            if (Path.GetExtension(dialog.FileName).ToLower() == ".bat")
                            {
                                batch = "GRAL*.bat";
                            }
#endif

                            if (batch != String.Empty)
                            {
                                filePaths = Directory.GetFiles(Path.GetDirectoryName(dialog.FileName),
                                                               Path.GetFileNameWithoutExtension(dialog.FileName) + ".*",
                                                               SearchOption.TopDirectoryOnly);
                                foreach (string file in filePaths)
                                {
                                    string newpathl = Path.Combine(ProjectName, @"Computation", Path.GetFileName(file));
                                    File.Copy(file, newpathl, true);
                                }
                            }

                        }
                        catch { }
                    }
                    else // do not copy the core to the project folder
                    {
                        GRAL_Project_Path = Path.Combine(ProjectName, @"Computation");
                        GUISettings.DefaultPathForGRAL = Path.GetDirectoryName(dialog.FileName);
                        GUISettings.WriteToFile();
                    }

                    //delete existing concentration fields
                    string newPath1 = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                    DirectoryInfo di = new DirectoryInfo(newPath1);
                    FileInfo[] files_conc = di.GetFiles("*.con");
                    if (files_conc.Length == 0) // compressed files?
                    {
                        files_conc = di.GetFiles("*.grz");
                    }

                    if (transient == 1) // steady state mode
                    {
                        if (files_conc.Length >= numericUpDown5.Value)
                        {
                            DialogResult res = MessageBox.Show("The existing *.con files higher than dispersion number "
                                                               + Convert.ToString(numericUpDown5.Value)
                                                               + " will be deleted", "Delete existing concentration files",
                                                               MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                            if (res == DialogResult.Cancel)
                            {
                                return; // exit, if user cancels
                            }
                        }

                        for (int i = 0; i < files_conc.Length; i++)
                        {
                            try
                            {
                                if (Convert.ToInt32(files_conc[i].Name.Substring(0, 5)) >= Convert.ToInt32(numericUpDown5.Value))
                                {
                                    files_conc[i].Delete();
                                }
                            }
                            catch { }
                        }
                        files_conc = di.GetFiles("*.odr"); // delete *.odr files
                        if (files_conc.Length > 0)
                        {
                            for (int i = 0; i < files_conc.Length; i++)
                            {
                                try
                                {
                                    if (Convert.ToInt32(files_conc[i].Name.Substring(0, 5)) >= Convert.ToInt32(numericUpDown5.Value))
                                    {
                                        files_conc[i].Delete();
                                    }
                                }
                                catch { }
                            }
                        }
                    } // delete *.con & *.odr files in transient mode
                    
                    Project_Locked = true;                  // lock project
                    ProjectLockedButtonClick(null, null); // change locked-Button

                    //generate high resolution GRAL topography from original data defined by the user, if it doesn't exist already
                    if (!File.Exists(Path.Combine(ProjectName, @"Computation", "GRAL_topofile.txt")) && checkBox25.Checked == true)
                    {
                        GralIO.CreateGralTopography Topo = new GralIO.CreateGralTopography();
                        if (Topo.GetFilename())
                        {
                            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                            if (Topo.CreateTopography(this) == false)
                            {
                                MessageBox.Show(this, "Unable to write file 'GRAL_topofile.txt'", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }

                    //start routine GRAL*.exe to compute concentrations
                    #if __MonoCS__
                    try
                    {
                        string command = String.Empty;
                        if (Path.GetFileName(GRAL_Program_Path).Equals("GRAL")) // new GRAL version from .NET5
                        {
                            command = "gnome-terminal -x bash -ic 'cd '" + Path.GetDirectoryName(GRAL_Program_Path) + "'; ./'" + Path.GetFileName(GRAL_Program_Path) + "'; bash'";
                            GRALProcess = new Process();
                            GRALProcess.Exited += new System.EventHandler(GralExited);
                            GRALProcess.StartInfo.FileName = "/bin/bash";
                            GRALProcess.StartInfo.Arguments = "-c \" " + command + " \"";
                            GRALProcess.StartInfo.UseShellExecute = false;
                            GRALProcess.StartInfo.RedirectStandardOutput = true;
                            GRALProcess.Start();
                        }
                        else
                        {
                            if (Path.GetExtension(GRAL_Program_Path).ToLower() == ".dll") // .sh file -> dotnet version
                            {
                                command = "gnome-terminal -x bash -ic 'cd '" + Path.GetDirectoryName(GRAL_Program_Path) + "'; dotnet " + Path.GetFileName(GRAL_Program_Path) + " " + '"' + GRAL_Project_Path + '"' + " ; bash'";
                            }
                            else // .exe file
                            {
                                command = "gnome-terminal -x bash -ic 'cd '" + Path.GetDirectoryName(GRAL_Program_Path) + "'; mono '" + Path.GetFileName(GRAL_Program_Path) + "'; bash'";
                            }
                            //start routine GRAL*.exe to compute concentrations
                            GRALProcess = new Process();
                            GRALProcess.StartInfo.FileName = "/bin/bash";
                            GRALProcess.StartInfo.Arguments = "-c \" " + command + " \"";
                            GRALProcess.StartInfo.UseShellExecute = false;
                            GRALProcess.StartInfo.RedirectStandardOutput = true;
                            GRALProcess.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    #else

                    GRALProcess = new Process
                    {
                        EnableRaisingEvents = true
                    };
                    GRALProcess.Exited += new System.EventHandler(GralExited);
                    GRALProcess.StartInfo.FileName = GRAL_Program_Path;
                    GRALProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                    GRALProcess.StartInfo.WorkingDirectory = @Path.GetDirectoryName(GRAL_Program_Path);
                    if (GUISettings.CopyCoresToProject == false)
                    {
                        GRALProcess.StartInfo.Arguments = " " + "\"" + GRAL_Project_Path + "\"";
                    }
                    if (GRALSettings.Loglevel > 0)
                    {
                        GRALProcess.StartInfo.Arguments += " " + "\"" + "LOGLEVEL0" + GRALSettings.Loglevel.ToString(ic) + "\"";
                    }
                    GRALProcess.Start();
                    
                    #endif

                    Project_Locked = true;                  // lock project
                    ProjectLockedButtonClick(null, null); // change locked-Button

                    WriteGralLogFile(1, "", GRAL_Program_Path);

                }
                catch
                {

                }
            }
        }

        //GRAL simulations finished or interrupted
        private void GralExited(object sender, EventArgs e)
        {
            #if __MonoCS__
            #else
            GRALProcess.EnableRaisingEvents = false;
            GRALProcess.Dispose();
            System.Threading.Thread.Sleep(1000);
            //check if *.con files are existing for postprocessing routines
            Invoke(new showTopo(CheckConFiles));
            WriteGralLogFile(3,"","");
            MessageBox.Show("GRAL simulation stopped or interrupted", "Info",MessageBoxButtons.OK,MessageBoxIcon.Information,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
            #endif
        }
        
        /// <summary>
        /// stop GRAL simulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRALStopCalculation(object sender, EventArgs e)
        {
            #if __MonoCS__
            MessageBox.Show("This function is not available at LINUX", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
            #else
            try
            {
                GRALProcess.Kill();
            }
            catch
            { }
            //check if *.con files are existing for postprocessing routines
            CheckConFiles();
            #endif
        }

        /// <summary>
        /// pause for GRAL simulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRALPauseCalculation(object sender, EventArgs e)
        {
            #if __MonoCS__
            MessageBox.Show("This function is not available at LINUX", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
            #endif
            try
            {
                GRALProcess.Kill();
            }
            catch
            { }

            //check if *.con files are existing for postprocessing routines
            CheckConFiles();

            //refresh actual computed dispersion situation
            int trackbar = 1;
            try
            {
                using (StreamReader myreader = new StreamReader(Path.Combine(ProjectName, @"Computation", "DispNr.txt")))
                {
                    trackbar = Convert.ToInt32(myreader.ReadLine().Replace(" ", ""));
                }
            }
            catch
            { }
            numericUpDown5.Value = Math.Max(numericUpDown5.Minimum, Math.Min(numericUpDown5.Maximum, trackbar));
            GenerateInDat();
        }
    }
}
