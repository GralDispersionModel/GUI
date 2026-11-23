using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
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

        internal static GrammEngine Inspect(string executable)
        {
#if __MonoCS__
            return new GrammEngine(Unknown, false);
#else
            if (!OperatingSystem.IsWindows() ||
                !string.Equals(Path.GetFileName(executable), "GRAMM.exe", StringComparison.OrdinalIgnoreCase))
            {
                return new GrammEngine(Unknown, false);
            }
            var host = FileVersionInfo.GetVersionInfo(executable);
            if (host.ProductName != "GRAMM") { return new GrammEngine(Unknown, false); }
            string version = host.ProductVersion ?? "";
            bool marked = Regex.IsMatch(version, @"\A[^+]+\+" + Reset + @"(?:\.[a-fA-F0-9]+)?\z");
            string generation = marked ? Reset : Regex.IsMatch(version, @"\A\d+(?:\.\d+)+(?:\+[a-fA-F0-9]+)?\z") ? Legacy : Unknown;
            var layout = ReadHostLayout(executable);
            if (layout == HostLayout.Standalone) { return new GrammEngine(generation, false); }
            string assembly = Path.ChangeExtension(executable, ".dll");
            bool adjacent = true;
            try { File.GetAttributes(assembly); }
            catch (FileNotFoundException) { adjacent = false; }
            if (layout == HostLayout.Native && !marked) { return new GrammEngine(Unknown, adjacent); }
            if (adjacent)
            {
                var core = FileVersionInfo.GetVersionInfo(assembly);
                if (core.ProductName != "GRAMM" || core.ProductVersion != version)
                {
                    throw new IOException("The selected GRAMM.exe and GRAMM.dll have different product versions. Select a complete GRAMM distribution.");
                }
            }
            if (!adjacent) { throw new IOException("The selected GRAMM.exe requires its matching GRAMM.dll. Select a complete GRAMM distribution."); }
            CheckRuntimeFile(Path.ChangeExtension(executable, ".runtimeconfig.json"), "runtimeOptions");
            CheckRuntimeFile(Path.ChangeExtension(executable, ".deps.json"), "targets");
            return new GrammEngine(layout == HostLayout.AppHost ? generation : Unknown, true);
#endif
        }

        private static void CheckRuntimeFile(string path, string property)
        {
            using var stream = File.OpenRead(path);
            using var json = JsonDocument.Parse(stream);
            if (!json.RootElement.TryGetProperty(property, out _)) { throw new InvalidDataException("Invalid GRAMM runtime file: " + path); }
        }

        private static HostLayout ReadHostLayout(string executable)
        {
            using var stream = File.OpenRead(executable);
            using var pe = new PEReader(stream);
            if (pe.PEHeaders.IsDll) { throw new InvalidDataException("Select the GRAMM executable, not a renamed DLL."); }
            if (pe.HasMetadata) { return HostLayout.Standalone; }
            // The .NET bundler patches the Int64 before this apphost signature with its bundle header offset.
            ReadOnlySpan<byte> signature = [0x8b,0x12,0x02,0xb9,0x6a,0x61,0x20,0x38,0x72,0x7b,0x93,0x02,0x14,0xd7,0xa0,0x32,
                                            0x13,0xf5,0xb9,0xe6,0xef,0xae,0x33,0x18,0xee,0x3b,0x2d,0xce,0x24,0xb3,0x6a,0xae];
            foreach (var section in pe.PEHeaders.SectionHeaders)
            {
                if (section.Name != ".data") { continue; }
                var data = pe.GetSectionData(section.VirtualAddress).GetContent().AsSpan();
                int index = data.IndexOf(signature);
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
    }
}
