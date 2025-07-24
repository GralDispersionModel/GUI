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
using System.Diagnostics.Eventing.Reader;

namespace GralData
{
    /// <summary>
    /// GUI Settings data structure and IO
    /// </summary>
    public class ProjectSettings
    {
        /// <summary>
        /// Path for the emission modulation, default values: projectPath\\Computation
        /// </summary>
        public string EmissionModulationPath = string.Empty;
        /// <summary>
        /// Path for the evaluation results, default values: projectPath\\Maps
        /// </summary>
        public string EvaluationPath = string.Empty;

        private string projectName = string.Empty;
        private CultureInfo ic = CultureInfo.InvariantCulture;
       
        /// <summary>
        /// Initialize and set default values for the GUI settings
        /// </summary>
        public ProjectSettings(string ProjectName)
        {
            projectName = ProjectName;
            SetDefaultValues();
        }

        /// <summary>
        /// Write GUI Settings to the file GUISettings.txt
        /// </summary>
        /// <param name="Gui"></param>
        /// <returns></returns>
        public bool WriteToFile()
        {
            string settingsFile = Path.Combine(projectName, "Settings", @"ProjectSettings.txt");
            if (Directory.Exists(Path.GetDirectoryName(settingsFile)))
            {
                try
                {
                    using (StreamWriter write = new StreamWriter(settingsFile))
                    {
                        write.WriteLine(EmissionModulationPath);
                        write.WriteLine(EvaluationPath);
                    }
                }
                catch
                {
                    SetDefaultValues();
                    return false;
                }
                return true;
            }
            else
            {
                SetDefaultValues();
                return false;
            }
        }

        /// <summary>
        /// Read GUI Settings from the file DefaultPath or GUISettings.txt
        /// </summary>
        /// <returns>Reading OK?</returns>
        public bool ReadFromFile()
        {
            string settingsFile = Path.Combine(projectName, "Settings", @"ProjectSettings.txt");
            if (File.Exists(settingsFile))
            {
                try
                {
                    using (StreamReader read = new StreamReader(settingsFile))
                    {
                        if (!read.EndOfStream)
                        {
                            try
                            {
                                EmissionModulationPath = read.ReadLine();
                            }
                            catch { }
                            if (!Directory.Exists(EmissionModulationPath))
                            {
                                EmissionModulationPath = System.IO.Path.Combine(projectName, "Computation");
                            }
                        }
                        if (!read.EndOfStream)
                        {
                            try
                            {
                                EvaluationPath = read.ReadLine();
                            }
                            catch { }
                            if (!Directory.Exists(EvaluationPath))
                            {
                                EvaluationPath = System.IO.Path.Combine(projectName, "Maps");
                            }
                        }
                    }
                }
                catch
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetDefaultValues()
        {
            EmissionModulationPath = System.IO.Path.Combine(projectName, "Computation");
            EvaluationPath = System.IO.Path.Combine(projectName, "Maps");
        }

        public override string ToString()
        {
            return EmissionModulationPath;
        }

        public override bool Equals(object obj)
        {
            return obj is ProjectSettings && this == (ProjectSettings)obj;
        }
        public override int GetHashCode()
        {
            return  EmissionModulationPath.GetHashCode();
        }
        public static bool operator == (ProjectSettings a, ProjectSettings b)
        {
            return a.EmissionModulationPath.Equals(b.EmissionModulationPath);
        }
        public static bool operator != (ProjectSettings a, ProjectSettings b)
        {
            return !(a == b);
        }
    }
}

