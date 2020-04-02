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

namespace GralMainForms
{
	/// <summary>
    /// Form for odour analyzation user settings
    /// </summary>
    public partial class Input_Odour : Form
    {
        public int MeanToPeak;
        public bool WriteAdditionalFiles;

        public Input_Odour()
        {
            InitializeComponent();
            ID_OK.DialogResult = DialogResult.OK;
            ID_Cancel.DialogResult = DialogResult.Cancel;
        }

        private void ID_OK_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                MeanToPeak = -1;
            }
            else
            {
                MeanToPeak = (int) numericUpDown1.Value;
            }
            WriteAdditionalFiles = checkBox1.Checked;
        }

        private void ID_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton1.Checked==true)
            {
                numericUpDown1.Enabled = false;
            }
            else
            {
                numericUpDown1.Enabled = true;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                numericUpDown1.Enabled = false;
                checkBox1.Enabled = true;
            }
            else
            {
                numericUpDown1.Enabled = true;
                checkBox1.Checked = false;
                checkBox1.Enabled = false;
            }
        }
    }
}
