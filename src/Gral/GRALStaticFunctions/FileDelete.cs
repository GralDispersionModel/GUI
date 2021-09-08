#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2020]  [Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion
using System;
using System.Runtime.InteropServices;

namespace GralStaticFunctions
{
    static class FileDelete
    {
        private const int FO_DELETE = 0x0003;
        private const int FOF_ALLOWUNDO = 0x0040;
        private const int FOF_NOCONFIRMATION = 0x0010;
        private static SHFILEOPSTRUCT64 fileop = new SHFILEOPSTRUCT64
        {
            wFunc = FO_DELETE,
            fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION,
            lpszProgressTitle = "Move files to the recycling bin" + '\0'
        };

        // Struct which contains information that the SHFileOperation function uses to perform file operations. 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEOPSTRUCT64
        {
            public IntPtr hwnd;
            public uint wFunc;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pTo;
            public ushort fFlags;
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern int SHFileOperation(ref SHFILEOPSTRUCT64 FileOp);

        public static void DeleteFileToRecyclingBin(string path)
        {
            fileop.pFrom = path + '\0' + '\0';
            SHFileOperation(ref fileop);
        }
    }
}