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
using System.Windows.Forms;

namespace Gral
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if NET6_0_OR_GREATER
            //ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.DpiUnawareGdiScaled);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#else
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
#endif
#if NET9_0_OR_GREATER
            //#pragma warning disable WFO5001
            GralData.GuiSettings guiSettings = new GralData.GuiSettings();
            guiSettings.ReadFromFile();
            if (guiSettings.UseDefaultColors)
            {
                Application.SetColorMode(SystemColorMode.Classic);
            }
            //#pragma warning restore WFO5001
#endif
            Application.Run(new Main());
        }
    }
}