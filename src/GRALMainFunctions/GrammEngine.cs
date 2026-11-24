using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
#if !__MonoCS__
using System.Reflection.PortableExecutable;
#endif
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Gral
{
    internal readonly record struct GrammEngine(string Generation, bool UsesAdjacentAssembly)
    {
        internal const string Legacy = "legacy";
        internal const string Reset = "steady-case-reset-v1";
        internal const string Unknown = "unknown";
        private enum HostLayout { Native, AppHost, Standalone }

        private static readonly byte[] BundleSignature = [0x8b,0x12,0x02,0xb9,0x6a,0x61,0x20,0x38,0x72,0x7b,0x93,0x02,0x14,0xd7,0xa0,0x32,
                                            0x13,0xf5,0xb9,0xe6,0xef,0xae,0x33,0x18,0xee,0x3b,0x2d,0xce,0x24,0xb3,0x6a,0xae];

        private static string KnownGeneration(string version)
        {
            if (Regex.IsMatch(version, @"\A[^+]+\+" + Reset + @"(?:\.[a-fA-F0-9]+)?\z")) { return Reset; }
            return Regex.IsMatch(version, @"\A\d+(?:\.\d+)+(?:\+[a-fA-F0-9]+)?\z") ? Legacy : Unknown;
        }

        internal static GrammEngine Inspect(string executable)
        {
#if __MonoCS__
            return InspectUnix(executable);
#else
            if (!OperatingSystem.IsWindows() ||
                !string.Equals(Path.GetFileName(executable), "GRAMM.exe", StringComparison.OrdinalIgnoreCase))
            {
                return new GrammEngine(Unknown, false);
            }
            var host = FileVersionInfo.GetVersionInfo(executable);
            if (host.ProductName != "GRAMM") { return new GrammEngine(Unknown, false); }
            string version = host.ProductVersion ?? "";
            string generation = KnownGeneration(version);
            bool marked = generation == Reset;
            var layout = ReadHostLayout(executable);
            if (layout == HostLayout.Standalone) { return new GrammEngine(generation, false); }
            string assembly = Path.ChangeExtension(executable, ".dll");
            bool adjacent = true;
            try { File.GetAttributes(assembly); }
            catch (FileNotFoundException) { adjacent = false; }
            if (layout == HostLayout.Native && !marked) { return new GrammEngine(Unknown, adjacent); }
            if (!adjacent) { throw new IOException("The selected GRAMM.exe requires its matching GRAMM.dll. Select a complete GRAMM distribution."); }
            var core = FileVersionInfo.GetVersionInfo(assembly);
            if (core.ProductName != "GRAMM" || core.ProductVersion != version)
            {
                throw new IOException("The selected GRAMM.exe and GRAMM.dll have different product versions. Select a complete GRAMM distribution.");
            }
            CheckRuntimeFile(Path.ChangeExtension(executable, ".runtimeconfig.json"), "runtimeOptions");
            CheckRuntimeFile(Path.ChangeExtension(executable, ".deps.json"), "targets");
            return new GrammEngine(layout == HostLayout.AppHost ? generation : Unknown, true);
#endif
        }

#if __MonoCS__
        private static GrammEngine InspectUnix(string executable)
        {
            string extension = Path.GetExtension(executable);
            if (extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                try { System.Reflection.AssemblyName.GetAssemblyName(executable); }
                catch (BadImageFormatException ex) { throw new IOException("Select a Linux GRAMM executable or its managed DLL.", ex); }
                var legacy = FileVersionInfo.GetVersionInfo(executable);
                return new GrammEngine(legacy.ProductName == "GRAMM" ? KnownGeneration(legacy.ProductVersion ?? "") : Unknown, false);
            }
            bool managed = extension.Equals(".dll", StringComparison.OrdinalIgnoreCase);
            string assembly = managed ? executable : executable + ".dll";
            if (!managed && (!File.Exists(assembly) || !IsFrameworkDependentGrammHost(executable)))
            {
                // An unrecognized native launcher may need other files next to it.
                return new GrammEngine(Unknown, File.Exists(assembly));
            }
            var core = FileVersionInfo.GetVersionInfo(assembly);
            if (core.ProductName != "GRAMM") { return new GrammEngine(Unknown, true); }
            CheckRuntimeFile(Path.ChangeExtension(assembly, ".runtimeconfig.json"), "runtimeOptions");
            CheckRuntimeFile(Path.ChangeExtension(assembly, ".deps.json"), "targets");
            return new GrammEngine(KnownGeneration(core.ProductVersion ?? ""), true);
        }

        private static bool IsFrameworkDependentGrammHost(string path)
        {
            // Only use adjacent version metadata for a normal, unbundled SDK apphost whose embedded entry point is GRAMM.dll.
            using var stream = File.OpenRead(path);
            if (Path.GetFileName(path) != "GRAMM" || stream.Length < 1024 || stream.Length > 4 * 1024 * 1024) { return false; }
            using var reader = new BinaryReader(stream);
            if (reader.ReadUInt32() != 0x464c457f) { return false; } // ELF
            byte[] data = reader.ReadBytes((int)stream.Length - 4);
            int signature = data.AsSpan().IndexOf(BundleSignature);
            if (signature < sizeof(long) || BinaryPrimitives.ReadInt64LittleEndian(data.AsSpan(signature - sizeof(long))) != 0) { return false; }
            byte[] entry = new byte[1024];
            System.Text.Encoding.ASCII.GetBytes("GRAMM.dll").CopyTo(entry, 0);
            return data.AsSpan().IndexOf(entry) >= 0;
        }
#endif

        private static void CheckRuntimeFile(string path, string property)
        {
            using var stream = File.OpenRead(path);
            using var json = JsonDocument.Parse(stream);
            if (!json.RootElement.TryGetProperty(property, out _)) { throw new InvalidDataException("Invalid GRAMM runtime file: " + path); }
        }

#if !__MonoCS__
        private static HostLayout ReadHostLayout(string executable)
        {
            using var stream = File.OpenRead(executable);
            using var pe = new PEReader(stream);
            if (pe.PEHeaders.IsDll) { throw new InvalidDataException("Select the GRAMM executable, not a renamed DLL."); }
            if (pe.HasMetadata) { return HostLayout.Standalone; }
            // The .NET bundler patches the Int64 before this apphost signature with its bundle header offset.
            foreach (var section in pe.PEHeaders.SectionHeaders)
            {
                if (section.Name != ".data") { continue; }
                var data = pe.GetSectionData(section.VirtualAddress).GetContent().AsSpan();
                int index = data.IndexOf(BundleSignature);
                if (index < sizeof(long)) { continue; }
                long header = BinaryPrimitives.ReadInt64LittleEndian(data.Slice(index - sizeof(long)));
                if (header == 0) { return HostLayout.AppHost; }
                if (header <= section.PointerToRawData + (long)section.SizeOfRawData || header > stream.Length - 12) { return HostLayout.Native; }
                stream.Position = header;
                using var reader = new BinaryReader(stream);
                uint major = reader.ReadUInt32();
                if ((major == 1 || major == 2 || major == 6) && reader.ReadUInt32() == 0 && reader.ReadInt32() > 0)
                {
                    return HostLayout.Standalone;
                }
            }
            return HostLayout.Native;
        }
#endif
    }
}
