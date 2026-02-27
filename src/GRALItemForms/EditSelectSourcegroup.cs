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

using GralStaticFunctions;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GralItemForms
{
    /// <summary>
    /// Description of EditSelectSourcegroup.
    /// </summary>
    public partial class EditSelectSourcegroup : Form
    {
        public List<string> SourceGroup = new List<string>();
        public List<string> CopyFrom = new List<string>();
        public string SelectedSourceGroup = string.Empty;
        public string SelCopyFrom = string.Empty;
        public int ShiftCopy = 0; // not copy, no shift -> new sg

        public EditSelectSourcegroup()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
        }

        void EditSelectSourcegroupLoad(object sender, EventArgs e)
        {
            // import source groups and fill source dialogs 
            List<int> _sg = new List<int>();
            foreach (string sgstring in CopyFrom)
            {
                _sg.Add(St_F.GetSgNumber(sgstring));
                comboBox2.Items.Add(sgstring);
            }

            for (int i = 0; i < SourceGroup.Count; i++)
            {
                if (_sg.Contains(St_F.GetSgNumber(SourceGroup[i])) == false)
                {
                    comboBox1.Items.Add(SourceGroup[i]);
                }
            }
            radioButton1.Checked = true;
            comboBox2.Enabled = false;
            if (CopyFrom.Count > 0)
            {
                groupBox1.Enabled = true;
            }
            else
            {
                groupBox1.Enabled = false;
            }
        } // load dialog

        void Button2Click(object sender, EventArgs e)
        {
            Close();
        }

        void Button1Click(object sender, EventArgs e)
        {
            SelectedSourceGroup = comboBox1.Text;
            SelCopyFrom = comboBox2.Text;
            if (radioButton1.Checked)
            {
                ShiftCopy = 0;
            }
            else if (radioButton2.Checked)
            {
                ShiftCopy = 1;
            }
            else
            {
                ShiftCopy = 2;
            }
            Close();
        }

        void EditSelectSourcegroupFormClosed(object sender, FormClosedEventArgs e)
        {

        }

        void RadioButton2CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked || radioButton3.Checked)
            {
                comboBox2.Enabled = true;
            }
            else
            {
                comboBox2.Enabled = false;
            }
        }
    }
}
