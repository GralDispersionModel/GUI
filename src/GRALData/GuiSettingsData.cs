#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2021]  [Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion
using System;
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace GralData
{
    /// <summary>
    /// GUI Settings data structure and IO
    /// </summary>
    public class GuiSettings
    {
        /// <summary>
        /// Default path for the GUI Settings
        /// </summary>
        public string AppSettingsPath;
        /// <summary>
        /// Path for the last used (loaded) project
        /// </summary>
        public string PreviousUsedProjectPath;
        /// <summary>
        /// Name of high resolution GRAL topography file
        /// </summary>
        public string TopoFileName;
        /// <summary>
        /// Default path for the GRAL core
        /// </summary>
        public string DefaultPathForGRAL;
        /// <summary>
        /// Default path for the GRAMM core
        /// </summary>
        public string DefaultPathForGRAMM;
        /// <summary>
        /// Copy the computation cores to the project folder?
        /// </summary>
        public bool CopyCoresToProject;
        /// <summary>
        /// Keep compatibility to version 19.01 (file paths)?
        /// </summary>
        public bool CompatibilityToVersion1901;
        /// <summary>
        /// Automatic Scaling for Vector Maps?
        /// </summary>
        public bool VectorMapAutoScaling;
        /// <summary>
        /// Ignore 00 Vales at meteo import
        /// </summary>
        public Gral.WindData00Enum IgnoreMeteo00Values;
        /// <summary>
        /// Delete *.con, *.gff and *.wnd files permanently
        /// </summary>
        public bool DeleteFilesToRecyclingBin;

        private CultureInfo ic = CultureInfo.InvariantCulture;

        /// <summary>
        /// Initialize and set default values for the GUI settings
        /// </summary>
        public GuiSettings()
        {
            //default path = application path, new path = user folder
            AppSettingsPath = Path.GetDirectoryName(Application.ExecutablePath);
            string app_settings_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "GRAL_GUI_Settings");
            if (Directory.Exists(app_settings_path))
            {
                AppSettingsPath = app_settings_path;
            }

            DefaultPathForGRAL = Path.GetDirectoryName(Application.ExecutablePath);
            DefaultPathForGRAMM = DefaultPathForGRAL;
            PreviousUsedProjectPath = string.Empty;
            TopoFileName = string.Empty;
            CopyCoresToProject = false;
            CompatibilityToVersion1901 = true;
            VectorMapAutoScaling = true;
            IgnoreMeteo00Values = Gral.WindData00Enum.All;
            DeleteFilesToRecyclingBin = true;
        }

        /// <summary>
        /// Write GUI Settings to the file GUISettings.txt
        /// </summary>
        /// <param name="Gui"></param>
        /// <returns></returns>
        public bool WriteToFile()
        {
            try
            {
                using (StreamWriter write = new StreamWriter(Path.Combine(AppSettingsPath, @"GUISettings.txt")))
                {
                    write.WriteLine(PreviousUsedProjectPath);
                    write.WriteLine(TopoFileName);
                    write.WriteLine(CopyCoresToProject.ToString(ic));
                    write.WriteLine(CompatibilityToVersion1901.ToString(ic));
                    write.WriteLine(DefaultPathForGRAL);
                    write.WriteLine(VectorMapAutoScaling.ToString(ic));
                    write.WriteLine(IgnoreMeteo00Values.ToString("d"));
                    write.WriteLine(DeleteFilesToRecyclingBin.ToString(ic));
                    write.WriteLine(DefaultPathForGRAMM);
                }
            }
            catch
            {
                MessageBox.Show("Your application folder is read-only", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read GUI Settings from the file DefaultPath or GUISettings.txt
        /// </summary>
        /// <returns>Reading OK?</returns>
        public bool ReadFromFile()
        {
            try
            {
                string settingsFile = "GUISettings.txt";
                //compatibility to version 21.09 and older versions
                if (!File.Exists(Path.Combine(AppSettingsPath, settingsFile)))
                {
                    settingsFile = "DefaultPath";
                }
                using (StreamReader read = new StreamReader(Path.Combine(AppSettingsPath, settingsFile)))
                {
                    PreviousUsedProjectPath = read.ReadLine();
                    if (!read.EndOfStream)
                    {
                        TopoFileName = read.ReadLine();
                    }

                    if (!read.EndOfStream)
                    {
                        CopyCoresToProject = Convert.ToBoolean(read.ReadLine(), ic);
                    }

                    if (!read.EndOfStream)
                    {
                        CompatibilityToVersion1901 = Convert.ToBoolean(read.ReadLine(), ic);
                    }

                    if (!read.EndOfStream)
                    {
                        DefaultPathForGRAL = read.ReadLine();
                    }

                    if (!read.EndOfStream)
                    {
                        VectorMapAutoScaling = Convert.ToBoolean(read.ReadLine(), ic);
                    }

                    if (!read.EndOfStream)
                    {
                        try
                        {
                            IgnoreMeteo00Values = (Gral.WindData00Enum)Convert.ToInt32(read.ReadLine());
                        }
                        catch { }
                    }
                    if (!read.EndOfStream)
                    {
                        try
                        {
                            DeleteFilesToRecyclingBin = Convert.ToBoolean(read.ReadLine(), ic);
                        }
                        catch { }
                    }
                    if (!read.EndOfStream)
                    {
                        DefaultPathForGRAMM = read.ReadLine();
                    }
                    else
                    {
                        DefaultPathForGRAMM = DefaultPathForGRAL;
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return PreviousUsedProjectPath + Environment.NewLine +
            TopoFileName + Environment.NewLine +
            CopyCoresToProject.ToString(ic) + Environment.NewLine +
            CompatibilityToVersion1901.ToString(ic) + Environment.NewLine +
            DefaultPathForGRAL + Environment.NewLine +
            VectorMapAutoScaling.ToString(ic) + Environment.NewLine +
            IgnoreMeteo00Values.ToString("d") + Environment.NewLine +
            DeleteFilesToRecyclingBin.ToString(ic) + Environment.NewLine +
            DefaultPathForGRAMM;
        }

        public override bool Equals(object obj)
        {
            return obj is GuiSettings && this == (GuiSettings)obj;
        }
        public override int GetHashCode()
        {
            return PreviousUsedProjectPath.GetHashCode() ^ TopoFileName.GetHashCode() ^ DefaultPathForGRAL.GetHashCode()
                ^ CopyCoresToProject.GetHashCode() ^ CompatibilityToVersion1901.GetHashCode() ^ VectorMapAutoScaling.GetHashCode()
                ^ IgnoreMeteo00Values.GetHashCode() ^ DeleteFilesToRecyclingBin.GetHashCode() ^ DefaultPathForGRAMM.GetHashCode();
        }
        public static bool operator ==(GuiSettings a, GuiSettings b)
        {
            return a.PreviousUsedProjectPath.Equals(b.PreviousUsedProjectPath) && a.TopoFileName.Equals(b.TopoFileName) && a.DefaultPathForGRAL.Equals(b.DefaultPathForGRAL) &&
                   a.CopyCoresToProject == b.CopyCoresToProject && a.CompatibilityToVersion1901 == b.CompatibilityToVersion1901 &&
                   a.VectorMapAutoScaling == b.VectorMapAutoScaling && a.IgnoreMeteo00Values == b.IgnoreMeteo00Values &&
                   a.DeleteFilesToRecyclingBin == b.DeleteFilesToRecyclingBin && a.DefaultPathForGRAMM.Equals(b.DefaultPathForGRAMM);
        }
        public static bool operator !=(GuiSettings a, GuiSettings b)
        {
            return !(a == b);
        }
    }
}

