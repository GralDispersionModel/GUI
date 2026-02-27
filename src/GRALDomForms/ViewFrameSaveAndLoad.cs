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
using System.IO;
using System.Windows.Forms;

namespace GralDomForms
{
    /// <summary>
    /// Map section (view frames) settings dialog 
    /// </summary>         
    public partial class ViewFrameSaveAndLoad : Form
    {
        public string NewText = String.Empty;
        public string SelText = String.Empty;
        public int SelEntry = -1;

        public ViewFrameSaveAndLoad()
        {
            InitializeComponent();
        }

        private void ViewFrameSaveAndLoad_Load(object sender, EventArgs e)
        {
            string path;
            path = Path.Combine(Gral.Main.ProjectName, @"Settings", "sections.txt");
            if (File.Exists(path))
            {
                try
                {
                    using (StreamReader myreader = new StreamReader(path)) // Open File
                    {
                        string s;
                        while (myreader.EndOfStream == false)
                        {
                            s = myreader.ReadLine();
                            listBox1.Items.Insert(0, s);
                            s = myreader.ReadLine();
                            s = myreader.ReadLine();
                            s = myreader.ReadLine();
                            s = myreader.ReadLine();
                        }
                    }
                }
                catch { }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                if (listBox1.SelectedIndex >= 0)
                {
                    SelText = listBox1.Items[listBox1.SelectedIndex].ToString();
                }
            }
            else
            {
                NewText = textBox1.Text;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            button1_Click(null, null);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                SelEntry = listBox1.Items.Count - listBox1.SelectedIndex;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
