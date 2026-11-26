// SPDX-License-Identifier: GPL-3.0-only
global using MathF = Gral.MonoMath;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Gral
{
    internal static class MonoCompatibility
    {
        internal static string GetRelativePath(string relativeTo, string path)
        {
            string[] start = Path.GetFullPath(relativeTo).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string absolute = Path.GetFullPath(path);
            string[] end = absolute.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            int common = 0;
            while (common < start.Length && common < end.Length && start[common] == end[common]) { common++; }
            if (common == start.Length && common == end.Length) { return "."; }
            string relative = string.Join("/", Enumerable.Repeat("..", start.Length - common).Concat(end.Skip(common)));
            return absolute.EndsWith("/", StringComparison.Ordinal) && common < end.Length ? relative + "/" : relative;
        }

        // Mono parses ProcessStartInfo.Arguments using GLib's shell quoting convention.
        internal static string QuoteArguments(IEnumerable<string> arguments) =>
            string.Join(" ", arguments.Select(argument => "'" + argument.Replace("'", "'\\''") + "'"));

        [DllImport("libc", EntryPoint = "rename", SetLastError = true)]
        private static extern int Rename(string source, string destination);

        internal static void MoveOverwrite(string source, string destination)
        {
            if (Rename(source, destination) == 0) { return; }
            var error = new Win32Exception(Marshal.GetLastWin32Error());
            throw new IOException($"Could not rename '{source}' to '{destination}': {error.Message}", error);
        }
    }

    internal static class MonoMath
    {
        internal const float PI = (float)Math.PI;
        internal static float Cos(float value) => (float)Math.Cos(value);
        internal static float Sin(float value) => (float)Math.Sin(value);
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
