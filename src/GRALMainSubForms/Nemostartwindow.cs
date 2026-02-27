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

using Gral;
using System;
using System.IO;
using System.Windows.Forms;

namespace GralMainForms
{
    /// <summary>
    /// Start a nemo procedure
    /// </summary>
    public partial class Nemostartwindow : Form
    {
        private Main form1 = null;

        public Nemostartwindow(Main f)
        {
            InitializeComponent();
            form1 = f;
            //fill combobox with defined source groups
            try
            {
                //import source group definitions
                string newPath = Path.Combine(Main.ProjectName, @"Settings", "Sourcegroups.txt");
                StreamReader myReader = new StreamReader(newPath);
                string[] text = new string[2];
                string text1;
                text1 = myReader.ReadLine();
                while (text1 != null)
                {
                    text = text1.Split(new char[] { ',' });
                    comboBox1.Items.Add(text1);
                    comboBox2.Items.Add(text1);
                    comboBox3.Items.Add(text1);
                    comboBox4.Items.Add(text1);
                    text1 = myReader.ReadLine();
                }
                myReader.Close();
            }
            catch
            {
            }
            //fill comboboxes to define source groups for the different sources
            int comboboxitems = comboBox1.Items.Count;
            for (int i = 1; i < 100; i++)
            {
                string[] text = new string[2];
                string text3;
                int sg = 0;
                for (int k = 0; k < comboboxitems; k++)
                {
                    text3 = Convert.ToString(comboBox1.Items[k]);
                    text = text3.Split(new char[] { ',' });
                    try
                    {
                        if (i == Convert.ToInt32(text[1]))
                        {
                            sg = i;
                            break;
                        }
                    }
                    catch
                    {
                    }
                }
                if (sg == 0)
                {
                    comboBox1.Items.Add(Convert.ToString(i));
                    comboBox2.Items.Add(Convert.ToString(i));
                    comboBox3.Items.Add(Convert.ToString(i));
                    comboBox4.Items.Add(Convert.ToString(i));
                }
            }
            comboBox1.SelectedIndex = 0;
            comboBox4.SelectedIndex = 1;
            comboBox2.SelectedIndex = 2;
            comboBox3.SelectedIndex = 3;
        }

        //show/hide group box for source group definitions
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                groupBox1.Visible = true;
            }
            else
            {
                groupBox1.Visible = false;
            }
        }

        //close the form
        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
        }
    }
}