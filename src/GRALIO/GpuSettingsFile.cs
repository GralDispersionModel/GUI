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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GralIO
{
    /// <summary>
    /// The GPU settings file of a project (GPU.txt in the Computation folder): one "key = value ! comment" per line.
    /// GRAL reads it at every start; the GUI only reads values and changes single keys, every other line stays byte for byte
    /// </summary>
    public static class GpuSettingsFile
    {
        /// <summary>
        /// The keys GRAL accepts; any other key stops the run
        /// </summary>
        public static readonly string[] Keys = { "use_gpu", "reproducible_sums", "device", "device_name", "backend", "allow_integrated", "fallback_on_startup", "nvrtc_dir", "hiprtc_dir", "chunk", "block",
                                                 "max_kernel_ms", "memory_budget_bytes", "memory_reserve_bytes", "pinned_budget_bytes", "cache_dir" };

        /// <summary>
        /// Path of the settings file GRAL reads for a project: INU_GPU_SETTINGS_FILE, or GPU.txt in the Computation folder, which is GRAL's working folder
        /// </summary>
        /// <param name="ProjectFolder">Folder of the project</param>
        /// <returns>The path, or null if INU_GPU_SETTINGS_FILE=off switches the file off or no project is open</returns>
        public static string FilePath(string ProjectFolder)
        {
            string file = Environment.GetEnvironmentVariable("INU_GPU_SETTINGS_FILE");
            if (string.IsNullOrWhiteSpace(file))
            {
                return string.IsNullOrEmpty(ProjectFolder) ? null : Path.Combine(ProjectFolder, "Computation", "GPU.txt");
            }
            return string.Equals(file.Trim(), "off", StringComparison.OrdinalIgnoreCase) ? null : file.Trim();
        }

        /// <summary>
        /// Read the keys that have a value; a missing file or folder gives an empty set
        /// </summary>
        /// <param name="FileName">Path of the settings file; null gives an empty set</param>
        /// <returns>Lower-case keys and trimmed values; the last line of a key counts</returns>
        public static Dictionary<string, string> Read(string FileName)
        {
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (FileName == null || !File.Exists(FileName))
            {
                return values;
            }
            foreach (byte[] line in SplitLines(File.ReadAllBytes(FileName)))
            {
                if (ParseLine(line, out string key, out string value, out _))
                {
                    if (value.Length > 0)
                    {
                        values[key] = value;
                    }
                    else
                    {
                        values.Remove(key);
                    }
                }
            }
            return values;
        }

        /// <summary>
        /// Set or remove single keys. Every other line, its comment and its line end stay byte for byte; a missing file is created from the template
        /// </summary>
        /// <param name="FileName">Path of the settings file</param>
        /// <param name="Changes">Key and new value; null or an empty value removes the line of the key</param>
        public static void Update(string FileName, IDictionary<string, string> Changes)
        {
            Dictionary<string, string> pending = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool anyValue = false;
            foreach (KeyValuePair<string, string> change in Changes)
            {
                string value = (change.Value ?? string.Empty).Trim();
                if (Array.IndexOf(Keys, change.Key.ToLowerInvariant()) < 0 || value.IndexOfAny(new char[] { '!', '\r', '\n' }) >= 0)
                {
                    // GRAL would stop at such a line, or read the rest of the value as a comment
                    throw new ArgumentException("GPU settings file: invalid key or value for '" + change.Key + "'");
                }
                pending[change.Key.ToLowerInvariant()] = value;
                anyValue |= value.Length > 0;
            }
            bool exists = File.Exists(FileName);
            if (!exists && !anyValue)
            {
                return; // nothing to remove
            }
            byte[] content = exists ? File.ReadAllBytes(FileName) : Encoding.UTF8.GetBytes(Template());
            if (content.Length >= 2 && ((content[0] == 0xFF && content[1] == 0xFE) || (content[0] == 0xFE && content[1] == 0xFF)))
            {
                // UTF-8 lines added to such a file would give a file that no program reads
                throw new InvalidDataException("The file is saved as UTF-16; save it as UTF-8.");
            }
            List<byte[]> lines = SplitLines(content);
            string newLine = Environment.NewLine;
            foreach (byte[] line in lines)
            {
                if (line.Length > 0 && line[line.Length - 1] == (byte)'\n')
                {
                    newLine = line.Length > 1 && line[line.Length - 2] == (byte)'\r' ? "\r\n" : "\n";
                    break;
                }
            }

            using (MemoryStream result = new MemoryStream())
            {
                bool changed = false;
                HashSet<string> written = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < lines.Count; i++)
                {
                    byte[] line = lines[i];
                    if (!ParseLine(line, out string key, out string oldValue, out string comment) || !pending.TryGetValue(key, out string value))
                    {
                        result.Write(line, 0, line.Length);
                        continue;
                    }
                    if (value.Length == 0 || written.Contains(key))
                    {
                        changed = true; // the key is removed, or this is a second line of a key that has been written
                        if (i == 0 && HasBom(line))
                        {
                            result.Write(line, 0, 3);
                        }
                        continue;
                    }
                    written.Add(key);
                    if (value == oldValue)
                    {
                        result.Write(line, 0, line.Length);
                        continue;
                    }
                    changed = true;
                    if (i == 0 && HasBom(line))
                    {
                        result.Write(line, 0, 3);
                    }
                    string end = line[line.Length - 1] != (byte)'\n' ? string.Empty : line.Length > 1 && line[line.Length - 2] == (byte)'\r' ? "\r\n" : "\n";
                    byte[] replaced = Encoding.UTF8.GetBytes(key + " = " + value + (comment.Length > 0 ? " " + comment : string.Empty) + end);
                    result.Write(replaced, 0, replaced.Length);
                }
                foreach (KeyValuePair<string, string> change in pending)
                {
                    if (change.Value.Length > 0 && !written.Contains(change.Key))
                    {
                        changed = true;
                        // a last line without a line end gets one, so that the new key starts its own line
                        string start = result.Length > 0 && !EndsWithNewLine(result) ? newLine : string.Empty;
                        byte[] added = Encoding.UTF8.GetBytes(start + change.Key + " = " + change.Value + newLine);
                        result.Write(added, 0, added.Length);
                    }
                }
                if (changed || !exists)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(FileName)));
                    File.WriteAllBytes(FileName, result.ToArray());
                }
            }
        }

        /// <summary>
        /// Create the settings file from the template if it does not exist
        /// </summary>
        public static void CreateIfMissing(string FileName)
        {
            if (!File.Exists(FileName))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(FileName)));
                File.WriteAllText(FileName, Template(), new UTF8Encoding(false));
            }
        }

        /// <summary>
        /// A settings file that leaves the GPU off and sets nothing else: every other key of GRAL as a comment line
        /// </summary>
        public static string Template()
        {
            string[] lines =
            {
                "! GPU settings of GRAL for this project (GPU.txt in the Computation folder). GRAL reads this file at every start; an INU_GPU_* environment variable wins over the line here.",
                "! One \"key = value\" per line, text after \"!\" is a comment. Remove the \"!\" in front of a key to set it; an unknown key or an invalid value stops the run.",
                "!",
                "use_gpu = 0                 ! 1 = particle dispersion on the GPU, 0 = on the CPU (INU_GPU_EXPERIMENTAL=1 or 0 overrides it for one run)",
                "! reproducible_sums = 0     ! with reproducible results (in.dat line 22) and the GPU: 0 = reproducible sums off, 1 or not set = on (INU_GPU_REPRODUCIBLE_SUMS)",
                "! device = 0                ! INU_GPU_DEVICE: number of the GPU, as listed by \"GRAL --gpu-check\"",
                "! device_name =             ! name of the GPU with that number; if another GPU has the number, GRAL selects automatically and warns",
                "! backend = cuda            ! INU_GPU_BACKEND: cuda (NVIDIA) or hip (AMD)",
                "! allow_integrated = 1      ! 1: an integrated GPU may be used when no discrete GPU is found",
                "! fallback_on_startup = 1   ! INU_GPU_FALLBACK_ON_STARTUP: 1 = continue on the CPU if the GPU cannot start, 0 = stop",
                "! nvrtc_dir =               ! INU_GPU_NVRTC_DIR: folder of NVRTC, the NVIDIA runtime compiler",
                "! hiprtc_dir =              ! INU_GPU_HIPRTC_DIR: folder of hiprtc, the AMD runtime compiler",
                "! chunk =                   ! INU_GRAL_GPU_CHUNK",
                "! block =                   ! INU_GRAL_GPU_BLOCK",
                "! max_kernel_ms =           ! INU_GPU_MAX_KERNEL_MS",
                "! memory_budget_bytes =     ! INU_GPU_MEMORY_BUDGET_BYTES",
                "! memory_reserve_bytes =    ! INU_GPU_MEMORY_RESERVE_BYTES",
                "! pinned_budget_bytes =     ! INU_GPU_PINNED_BUDGET_BYTES",
                "! cache_dir =               ! INU_GPU_CACHE_DIR: folder of the compiled GPU modules"
            };
            return string.Join(Environment.NewLine, lines) + Environment.NewLine;
        }

        private static bool HasBom(byte[] Line)
        {
            return Line.Length >= 3 && Line[0] == 0xEF && Line[1] == 0xBB && Line[2] == 0xBF;
        }

        private static bool EndsWithNewLine(MemoryStream Stream)
        {
            return Stream.Length > 0 && Stream.GetBuffer()[Stream.Length - 1] == (byte)'\n';
        }

        /// <summary>
        /// Split into lines that keep their line end, so that joining them gives the same bytes
        /// </summary>
        private static List<byte[]> SplitLines(byte[] Content)
        {
            List<byte[]> lines = new List<byte[]>();
            int start = 0;
            for (int i = 0; i < Content.Length; i++)
            {
                if (Content[i] == (byte)'\n')
                {
                    lines.Add(Content[start..(i + 1)]);
                    start = i + 1;
                }
            }
            if (start < Content.Length)
            {
                lines.Add(Content[start..]);
            }
            return lines;
        }

        /// <summary>
        /// Parse one line as GRAL does
        /// </summary>
        /// <param name="Line">Bytes of the line with its line end</param>
        /// <param name="Key">Lower-case key</param>
        /// <param name="Value">Trimmed value, may be empty</param>
        /// <param name="Comment">The comment from "!" on, or an empty string</param>
        /// <returns>True for a "key = value" line; false for blank lines, comment lines and lines without "="</returns>
        private static bool ParseLine(byte[] Line, out string Key, out string Value, out string Comment)
        {
            Key = Value = Comment = string.Empty;
            string text = Encoding.UTF8.GetString(Line, HasBom(Line) ? 3 : 0, Line.Length - (HasBom(Line) ? 3 : 0)).TrimEnd('\r', '\n');
            if (text.TrimStart().StartsWith('#'))
            {
                return false;
            }
            int mark = text.IndexOf('!');
            if (mark >= 0)
            {
                Comment = text.Substring(mark).TrimEnd();
                text = text.Substring(0, mark);
            }
            int equal = text.IndexOf('=');
            if (equal < 0)
            {
                return false;
            }
            Key = text.Substring(0, equal).Trim().ToLowerInvariant();
            Value = text.Substring(equal + 1).Trim();
            return Key.Length > 0;
        }
    }
}
