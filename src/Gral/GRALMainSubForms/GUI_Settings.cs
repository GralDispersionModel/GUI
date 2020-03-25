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
 * Date: 01.08.2018
 * Time: 07:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.IO;
using Gral;

namespace GralMainForms
{
    /// <summary>
    /// GUI Settings form
    /// </summary>
    public partial class GUI_Settings : Form
    {
        public GUI_Settings()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            //
            // TODO: Add constructor code after the InitializeComponent() call.
            //
             // Folder of the GUI settings files
            string app_settings_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "GRAL_GUI_Settings");
            if (Directory.Exists(app_settings_path))
            {
                radioButton3.Checked = false;
                radioButton4.Checked = true;
            }
            else
            {
                radioButton4.Checked = false;
                radioButton3.Checked = true;
            }

            // Copy GRAL/GRAMM.exe
            #if __MonoCS__
            Main.CopyCorestoProject = true;
            radioButton5.Checked = false; // No
            radioButton6.Checked = true; // Yes
            #else
            radioButton5.Checked =! Main.CopyCorestoProject; // No
            radioButton6.Checked = Main.CopyCorestoProject; // Yes
            #endif
            
            checkBox1.Checked = Main.CompatibilityToVersion1901;
            
            radioButton3.CheckedChanged += new EventHandler(RadioButton3Click);
            radioButton4.CheckedChanged += new EventHandler(RadioButton4Click);
            radioButton5.CheckedChanged += new EventHandler(RadioButton5_6Click);
            radioButton6.CheckedChanged += new EventHandler(RadioButton5_6Click);
            
       }
        		
        // Change GUI Settings Path to application path
		void RadioButton3Click(object sender, EventArgs e)
		{
		    if (radioButton3.Checked)
		    {
		        string app_settings_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "GRAL_GUI_Settings");
		        
		        if (Directory.Exists(app_settings_path))
		        {
		            if (MessageBox.Show(this, "Change settings path to application path and delete recent settings?",
		                                "Change GUI Settings Path", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
		            {
		                Main.App_Settings_Path = Path.GetDirectoryName(Application.ExecutablePath);
		                try
		                {
		                    Directory.Delete(app_settings_path, true);
		                }
		                catch{}
		            }
		            else
		            {
		                radioButton3.CheckedChanged -= new EventHandler(RadioButton3Click);
                        radioButton4.CheckedChanged -= new EventHandler(RadioButton4Click);
                        radioButton4.Checked = true;
                        radioButton3.Checked = false;
                        radioButton3.CheckedChanged += new EventHandler(RadioButton3Click);
                        radioButton4.CheckedChanged += new EventHandler(RadioButton4Click);
		            }
		        }
		    }
		}
		
		// Change GUI Settings Path to user folder
		void RadioButton4Click(object sender, EventArgs e)
		{
		    if (radioButton4.Checked)
		    {
		        string app_settings_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "GRAL_GUI_Settings");
		        
		        if (MessageBox.Show(this, "Create new settings path " + app_settings_path + " and copy all setting to this path?",
		                            "Change GUI Settings Path", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
		        {
		            try
		            {
		                Directory.CreateDirectory(app_settings_path);
		                
		                string _source = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "DepositionSettings.txt");
		                string _dest = String.Empty;
		                if (File.Exists(_source))
		                {
		                    _dest = Path.Combine(app_settings_path, "DepositionSettings.txt");
		                    File.Copy(_source, _dest, true);
		                }
		                
		                _source = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"DefaultPath");
		                if (File.Exists(_source))
		                {
		                    _dest = Path.Combine(app_settings_path, @"DefaultPath");
		                    File.Copy(_source, _dest, true);
		                }
		                
		                _source = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"RecentFiles.txt");
		                if (File.Exists(_source))
		                {
		                    _dest = Path.Combine(app_settings_path, @"RecentFiles.txt");
		                    File.Copy(_source, _dest, true);
		                }
		                
		                _source = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Emission_Mod_Diurnal.txt");
		                if (File.Exists(_source))
		                {
		                    _dest = Path.Combine(app_settings_path, "Emission_Mod_Diurnal.txt");
		                    File.Copy(_source, _dest, true);
		                }
		                
		                _source = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Emission_Mod_Seasonal.txt");
		                if (File.Exists(_source))
		                {
		                    _dest = Path.Combine(app_settings_path, "Emission_Mod_Seasonal.txt");
		                    File.Copy(_source, _dest, true);
		                }
		                
		                _source = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"GRAL.nemo");
		                if (File.Exists(_source))
		                {
		                    _dest = Path.Combine(app_settings_path, @"GRAL.nemo");
		                    File.Copy(_source, _dest, true);
		                }
		                
		                _source = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"Landuse_Default.txt");
		                if (File.Exists(_source))
		                {
		                    _dest = Path.Combine(app_settings_path, @"Landuse_Default.txt");
		                    File.Copy(_source, _dest, true);
		                }
		                
		                Main.App_Settings_Path = app_settings_path;
		            }
		            catch
		            {
		                radioButton3.Checked = true;
		                radioButton4.Checked = false;
		            }
		        }
		        else
		        {
		            radioButton3.CheckedChanged -= new EventHandler(RadioButton3Click);
		            radioButton4.CheckedChanged -= new EventHandler(RadioButton4Click);
		            radioButton4.Checked = false;
		            radioButton3.Checked = true;
		            radioButton3.CheckedChanged += new EventHandler(RadioButton3Click);
		            radioButton4.CheckedChanged += new EventHandler(RadioButton4Click);
		        }
		    }
		}
		
		// Copy cores to output?
		void RadioButton5_6Click(object sender, EventArgs e)
        {
		    Main.CopyCorestoProject = radioButton6.Checked;
            ReadAndWriteAppSettingsPaths();
        }
        
        void ReadAndWriteAppSettingsPaths()
        {
            string a = "", b = "";
            try
            {
                using (StreamReader read = new StreamReader(Path.Combine(Main.App_Settings_Path, @"DefaultPath")))
                {
                    a = read.ReadLine();
                    if (!read.EndOfStream)
                    {
                        b = read.ReadLine();
                    }
                }

                using (StreamWriter write = new StreamWriter(Path.Combine(Main.App_Settings_Path, @"DefaultPath")))
                {
                    write.WriteLine(a);
                    write.WriteLine(b);
                    write.WriteLine(Main.CopyCorestoProject.ToString());
                    write.WriteLine(Main.CompatibilityToVersion1901.ToString());
                    write.WriteLine(Main.CalculationCoresPath);
                }
            }
            catch { }
        }

        void GUI_SettingsLoad(object sender, EventArgs e)
        {
        }
        
        void Button1Click(object sender, EventArgs e)
        {
        	Main.CompatibilityToVersion1901 = checkBox1.Checked;
            Close();
        }
       
        void CheckBox1Click(object sender, EventArgs e)
        {
            Main.CompatibilityToVersion1901 = checkBox1.Checked;
            RadioButton5_6Click(null, null); // write file
        }
    }
}
