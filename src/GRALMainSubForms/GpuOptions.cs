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

using GralIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace GralMainForms
{
    /// <summary>
    /// GPU options of a project: the device, the start behaviour and the reproducible sums are kept in GPU.txt of the project's Computation folder
    /// </summary>
    public partial class GpuOptions : Form
    {
        /// <summary>
        /// A GPU check ran at a GRAL start in this session
        /// </summary>
        public bool CheckDone = false;
        /// <summary>
        /// Status of that check, or null if this GRAL has no check mode
        /// </summary>
        public string CheckStatus = null;
        public string CheckMessage = string.Empty;
        /// <summary>
        /// Report of that check ("GRAL --gpu-check"); an older GRAL reports no devices and no settings
        /// </summary>
        public JsonElement CheckReport;
        /// <summary>
        /// The GRAL executable of the last start in this session
        /// </summary>
        public string GralExecutable = string.Empty;
        /// <summary>
        /// in.dat line 22
        /// </summary>
        public bool GRALReproducibleResults = false;
        /// <summary>
        /// GPU.txt of the project, or the file INU_GPU_SETTINGS_FILE names; null if there is none
        /// </summary>
        public string SettingsFile = null;
        /// <summary>
        /// The GPU runtime log of the project
        /// </summary>
        public string GpuLogFile = string.Empty;

        private sealed class DeviceChoice
        {
            public string Text;
            public string Backend;       // null = automatic
            public int Ordinal = -1;     // -1 = automatic device
            public string Name = string.Empty;
            public bool Integrated;
            public bool Saved;           // the entry of the settings file, which no listed device matches
            public override string ToString()
            {
                return Text;
            }
        }

        private string settingsFile;
        private int initialDevice;
        private bool initialIntegrated;
        private bool initialFallback;
        private bool initialSums;
        private bool integratedTicked; // the permission was ticked by the choice of an integrated GPU, not by the user

        public GpuOptions()
        {
            InitializeComponent();
        }

        private void GpuOptions_Load(object sender, EventArgs e)
        {
            settingsFile = SettingsFile;
            Dictionary<string, string> saved = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string readError = null;
            try
            {
                saved = GpuSettingsFile.Read(settingsFile);
            }
            catch (Exception ex)
            {
                readError = ex.Message;
            }

            // Device list: automatic, the devices of the backend that the check started, and the other backends found
            comboBox1.Items.Add(new DeviceChoice() { Text = "Automatic" });
            bool listed = false;
            if (CheckReport.ValueKind == JsonValueKind.Object)
            {
                if (CheckReport.TryGetProperty("devices", out JsonElement devices) && devices.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement device in devices.EnumerateArray())
                    {
                        if (device.ValueKind != JsonValueKind.Object || !device.TryGetProperty("ordinal", out JsonElement ordinal) || ordinal.ValueKind != JsonValueKind.Number || !ordinal.TryGetInt32(out int number))
                        {
                            continue;
                        }
                        DeviceChoice choice = new DeviceChoice() { Ordinal = number, Name = JsonText(device, "name"), Backend = JsonText(device, "backend") };
                        choice.Integrated = device.TryGetProperty("integrated", out JsonElement integrated) && integrated.ValueKind == JsonValueKind.True;
                        choice.Text = (choice.Name.Length > 0 ? choice.Name : "Device " + number.ToString()) + ", " + choice.Backend +
                                      (device.TryGetProperty("total_memory_bytes", out JsonElement memory) && memory.ValueKind == JsonValueKind.Number && memory.TryGetInt64(out long bytes) ? ", " + (bytes >> 20).ToString() + " MiB" : string.Empty) +
                                      (choice.Integrated ? ", integrated" : string.Empty);
                        comboBox1.Items.Add(choice);
                        listed = true;
                    }
                }
                if (CheckReport.TryGetProperty("other_backends", out JsonElement others) && others.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement other in others.EnumerateArray())
                    {
                        if (other.ValueKind == JsonValueKind.String && other.GetString().Length > 0)
                        {
                            comboBox1.Items.Add(new DeviceChoice() { Text = other.GetString() + " (automatic device)", Backend = other.GetString() });
                        }
                    }
                }
            }

            // Select what GPU.txt holds, also when no listed device matches it
            saved.TryGetValue("device", out string savedDevice);
            saved.TryGetValue("device_name", out string savedName);
            saved.TryGetValue("backend", out string savedBackend);
            checkBox1.Checked = saved.TryGetValue("allow_integrated", out string allowed) && allowed == "1";
            comboBox1.SelectedIndex = 0;
            if (!string.IsNullOrEmpty(savedDevice) || !string.IsNullOrEmpty(savedBackend))
            {
                int savedOrdinal = int.TryParse(savedDevice, out int parsed) ? parsed : -1;
                int match = -1;
                for (int i = 1; i < comboBox1.Items.Count && match < 0; i++)
                {
                    DeviceChoice choice = (DeviceChoice)comboBox1.Items[i];
                    if (!string.IsNullOrEmpty(savedDevice))
                    {
                        if (choice.Ordinal >= 0 && choice.Ordinal == savedOrdinal && (string.IsNullOrEmpty(savedBackend) || string.Equals(choice.Backend, savedBackend, StringComparison.OrdinalIgnoreCase)) &&
                            (string.IsNullOrEmpty(savedName) || choice.Name == savedName))
                        {
                            match = i;
                        }
                    }
                    else if (choice.Ordinal < 0 && string.Equals(choice.Backend, savedBackend, StringComparison.OrdinalIgnoreCase))
                    {
                        match = i;
                    }
                }
                if (match < 0)
                {
                    string text = string.IsNullOrEmpty(savedDevice) ? savedBackend + " (automatic device)" :
                                  (string.IsNullOrEmpty(savedName) ? "Device " + savedDevice : savedName + " (device " + savedDevice + ")") + (string.IsNullOrEmpty(savedBackend) ? string.Empty : ", " + savedBackend);
                    match = comboBox1.Items.Add(new DeviceChoice() { Text = "Saved: " + text + (listed ? " (not found at the last check)" : string.Empty), Saved = true });
                }
                comboBox1.SelectedIndex = match;
            }
            initialDevice = comboBox1.SelectedIndex;
            initialIntegrated = checkBox1.Checked; // ticked as well when the saved device is an integrated GPU
            initialFallback = saved.TryGetValue("fallback_on_startup", out string fallback) && fallback == "1";
            radioButton2.Checked = initialFallback;
            radioButton1.Checked = !initialFallback;
            label1.Text = (listed ? string.Empty : (CheckDone ? "The GRAL of the last start did not list the GPUs." : "The GPUs are listed here after a GRAL start with the GPU option in this session.") + Environment.NewLine) +
                          "All GRAL instances of a start share one GPU.";

            // reproducible_sums counts only together with the reproducible results option (in.dat line 22); not set means on
            initialSums = !(saved.TryGetValue("reproducible_sums", out string sums) && sums == "0");
            checkBox2.Checked = initialSums;
            checkBox2.Enabled = GRALReproducibleResults && !Gral.Main.Project_Locked;
            label2.Text = "\"GRAL Reproducible Results\" (special GRAL settings) is " + (GRALReproducibleResults ? "on" : "off; this option needs it") + "." +
                          (Gral.Main.Project_Locked ? " The project is locked." : string.Empty);

            if (settingsFile == null)
            {
                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
                checkBox2.Enabled = false;
                button4.Enabled = false;
            }
            button3.Enabled = File.Exists(GpuLogFile);
            textBox1.Text = DisplayPaths(StatusText(saved, readError));
            textBox1.Select(0, 0);
        }

        private static string JsonText(JsonElement Parent, string Name)
        {
            return Parent.ValueKind == JsonValueKind.Object && Parent.TryGetProperty(Name, out JsonElement value) && value.ValueKind == JsonValueKind.String ? value.GetString() : string.Empty;
        }

        /// <summary>
        /// Paths below the user's folders are shown with %LOCALAPPDATA% and %USERPROFILE%, so that a picture of the window does not carry the user name
        /// </summary>
        private static string DisplayPaths(string Text)
        {
            if (!OperatingSystem.IsWindows())
            {
                return Text;
            }
            foreach ((Environment.SpecialFolder folder, string name) in new[] { (Environment.SpecialFolder.LocalApplicationData, "%LOCALAPPDATA%"), (Environment.SpecialFolder.UserProfile, "%USERPROFILE%") })
            {
                string path = Environment.GetFolderPath(folder).TrimEnd(Path.DirectorySeparatorChar);
                if (path.Length > 3)
                {
                    Text = Text.Replace(path + Path.DirectorySeparatorChar, name + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
                }
            }
            return Text;
        }

        /// <summary>
        /// The result of the last check in plain words, and the settings file
        /// </summary>
        private string StatusText(Dictionary<string, string> Saved, string ReadError)
        {
            StringBuilder text = new StringBuilder();
            if (!CheckDone)
            {
                text.AppendLine("The GPU has not been checked in this session. GRAL checks it at each start with the GPU option.");
            }
            else if (CheckStatus == null)
            {
                text.AppendLine("This GRAL has no GPU check; it reports GPU problems itself at the start.");
            }
            else
            {
                text.AppendLine(CheckStatus == "ok" ? "The GPU was ready at the last check." : "The GPU could not be used at the last check (" + CheckStatus + "): " + CheckMessage);
                if (CheckReport.ValueKind == JsonValueKind.Object)
                {
                    string name = JsonText(CheckReport, "name");
                    if (name.Length > 0)
                    {
                        text.AppendLine("Device: " + name + (CheckReport.TryGetProperty("ordinal", out JsonElement ordinal) && ordinal.ValueKind == JsonValueKind.Number ? " (device " + ordinal.ToString() + ")" : string.Empty) +
                                        (CheckReport.TryGetProperty("total_memory_bytes", out JsonElement memory) && memory.ValueKind == JsonValueKind.Number && memory.TryGetInt64(out long bytes) ? ", " + (bytes >> 20).ToString() + " MiB" : string.Empty));
                    }
                    string backend = JsonText(CheckReport, "backend");
                    if (backend.Length > 0)
                    {
                        text.AppendLine("Backend: " + backend + (backend == "cuda" ? " (NVIDIA)" : backend == "hip" ? " (AMD)" : string.Empty) +
                                        (JsonText(CheckReport, "driver_cuda").Length > 0 ? ", driver supports " + JsonText(CheckReport, "driver_cuda") : string.Empty));
                    }
                    string compiler = JsonText(CheckReport, "nvrtc_version");
                    if (compiler.Length > 0)
                    {
                        text.AppendLine("Runtime compiler: " + (compiler.StartsWith("hiprtc") ? compiler : "NVRTC " + compiler) + (JsonText(CheckReport, "nvrtc_path").Length > 0 ? " in " + JsonText(CheckReport, "nvrtc_path") : string.Empty));
                    }
                    if (CheckReport.TryGetProperty("settings", out JsonElement settings) && settings.ValueKind == JsonValueKind.Object)
                    {
                        foreach (JsonProperty setting in settings.EnumerateObject())
                        {
                            if (JsonText(setting.Value, "source") == "env")
                            {
                                text.AppendLine("An environment variable sets " + setting.Name + " = " + JsonText(setting.Value, "value") + "; it wins over GPU.txt.");
                            }
                        }
                    }
                    if (CheckReport.TryGetProperty("warnings", out JsonElement warnings) && warnings.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement warning in warnings.EnumerateArray())
                        {
                            if (warning.ValueKind == JsonValueKind.String)
                            {
                                text.AppendLine("Warning: " + warning.GetString());
                            }
                        }
                    }
                }
            }
            if (CheckDone && (CheckStatus == null || (CheckReport.ValueKind == JsonValueKind.Object && !CheckReport.TryGetProperty("settings_file_status", out _))))
            {
                text.AppendLine("This GRAL does not read GPU.txt: the device, the start behaviour and the sums chosen here take effect with a newer GRAL.");
            }
            text.AppendLine();
            bool fromVariable = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("INU_GPU_SETTINGS_FILE"));
            if (settingsFile == null)
            {
                text.AppendLine("Settings file: " + (fromVariable ? "switched off (INU_GPU_SETTINGS_FILE=off)" : "none (no project is open)") + ". The choices of this window cannot be saved.");
            }
            else
            {
                // the project's own file is named without its path; a file named by the variable is shown in full
                text.AppendLine("Settings file: " + (fromVariable ? settingsFile + " (INU_GPU_SETTINGS_FILE)" : "GPU.txt in the project's Computation folder") + (File.Exists(settingsFile) ? string.Empty : " (not created yet)"));
                if (!Path.IsPathFullyQualified(settingsFile))
                {
                    text.AppendLine("INU_GPU_SETTINGS_FILE is a relative path: GRAL looks for it in its own working folder. Give the full path.");
                }
                if (ReadError != null)
                {
                    text.AppendLine("The file could not be read: " + ReadError);
                }
                foreach (string key in GpuSettingsFile.Keys)
                {
                    // settings that this form does not show are named, so that the user knows they exist
                    if (Saved.TryGetValue(key, out string value) && key != "use_gpu" && key != "reproducible_sums" && key != "device" && key != "device_name" && key != "backend" && key != "allow_integrated" && key != "fallback_on_startup")
                    {
                        text.AppendLine("  " + key + " = " + value);
                    }
                }
            }
            if (GralExecutable.Length > 0)
            {
                text.AppendLine("GRAL: " + GralExecutable);
            }
            return text.ToString();
        }

        /// <summary>
        /// Choosing an integrated GPU needs the permission to use one
        /// </summary>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(comboBox1.SelectedItem is DeviceChoice choice))
            {
                return;
            }
            if (choice.Integrated)
            {
                if (!checkBox1.Checked)
                {
                    checkBox1.Checked = true;
                    integratedTicked = true;
                }
                checkBox1.Enabled = false; // a chosen integrated GPU is always saved with the permission
            }
            else
            {
                checkBox1.Enabled = true;
                if (integratedTicked)
                {
                    checkBox1.Checked = false; // ticked by an integrated GPU that is not chosen any more
                    integratedTicked = false;
                }
            }
        }

        /// <summary>
        /// OK Button: write the changed keys to GPU.txt; every other line of the file stays as it is
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            if (settingsFile == null)
            {
                return;
            }
            Dictionary<string, string> changes = new Dictionary<string, string>();
            if (comboBox1.SelectedIndex != initialDevice && comboBox1.SelectedItem is DeviceChoice choice && !choice.Saved)
            {
                changes["device"] = choice.Ordinal >= 0 ? choice.Ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture) : null;
                // a name that the file cannot hold is left out; GRAL then takes the device number as it is
                changes["device_name"] = choice.Ordinal >= 0 && choice.Name.IndexOfAny(new char[] { '!', '\r', '\n' }) < 0 ? choice.Name : null;
                changes["backend"] = choice.Backend;
                if (choice.Integrated)
                {
                    changes["allow_integrated"] = "1";
                }
            }
            if (checkBox1.Checked != initialIntegrated && !changes.ContainsKey("allow_integrated"))
            {
                changes["allow_integrated"] = checkBox1.Checked ? "1" : null;
            }
            if (radioButton2.Checked != initialFallback)
            {
                changes["fallback_on_startup"] = radioButton2.Checked ? "1" : null;
            }
            if (checkBox2.Enabled && checkBox2.Checked != initialSums)
            {
                changes["reproducible_sums"] = checkBox2.Checked ? "1" : "0";
            }
            if (changes.Count == 0)
            {
                return;
            }
            try
            {
                GpuSettingsFile.Update(settingsFile, changes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The GPU settings file could not be written: " + settingsFile + Environment.NewLine + ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None; // keep the form open
            }
        }

        /// <summary>
        /// Show GPU log
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            if (!File.Exists(GpuLogFile))
            {
                MessageBox.Show(this, "There is no GPU log yet: " + GpuLogFile, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            OpenFile(GpuLogFile);
        }

        /// <summary>
        /// Edit settings file: open it in the text editor; create it first from the template
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                GpuSettingsFile.CreateIfMissing(settingsFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The GPU settings file could not be created: " + settingsFile + Environment.NewLine + ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            OpenFile(settingsFile); // OK writes only the choices changed in this window, so keys edited in the editor stay
        }

        /// <summary>
        /// Open a text file with the program the system uses for it
        /// </summary>
        private void OpenFile(string FileName)
        {
            try
            {
                Process.Start(new ProcessStartInfo() { FileName = FileName, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + Environment.NewLine + FileName, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
