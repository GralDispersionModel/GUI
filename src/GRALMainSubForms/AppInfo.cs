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
using System.Drawing;
using System.Windows.Forms;
using Gral;

namespace GralMainForms
{
    public partial class AppInfo : Form
    {
        public AppInfo()
        {
            InitializeComponent();
        }

        private void AppInfo_Load(object sender, EventArgs e)
        {
            string a = "[GRAL]  Copyright (C) <2019>  <Dietmar Oettl, Markus Kuntner>" + Environment.NewLine + Environment.NewLine;
            a += "This program comes with ABSOLUTELY NO WARRANTY" + Environment.NewLine;
            a += "This is free software, and you are welcome to redistribute it under certain conditions" + Environment.NewLine + Environment.NewLine;
            a += "This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of" +
                 " MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details." + Environment.NewLine + Environment.NewLine;
            a += "This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by" +
                 "the Free Software Foundation, either version 3 of the License." + Environment.NewLine;
            a += "You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.";
            textBox1.Text = a;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = label5.Text, UseShellExecute = true }); 
        }

        private void label4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = label4.Text, UseShellExecute = true });
        }

        private void label5_MouseHover(object sender, EventArgs e)
        {
            Label _l = (Label) sender;
            _l.ForeColor = Color.DarkBlue;
        }

        private void label5_MouseLeave(object sender, EventArgs e)
        {
            Label _l = (Label)sender;
            _l.ForeColor = Color.Blue;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //set Main.GUISettings.AutoCheckForUpdates to false -> force output if the version is still up to date
            Main.AutoUpdateStart(true);
        }
    }

}
