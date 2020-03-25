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
using System.IO;
using Gral;

namespace GralMainForms
{
	/// <summary>
    /// Select a source group
    /// </summary>
    public partial class SelectSourcegroups : Form
    {
        private Main form1 = null;
        private string _prefix = "";
        private int Mode = 0;
        
		public string Prefix
		{ get 
        	{ if (_prefix.Length > 0)
                {
                    return _prefix + "_";
                }
                else
                {
                    return String.Empty;
                }
            } set {_prefix = value;}
		} // Filename

		public SelectSourcegroups (Main f, int _mode)
		{
			Mode = _mode; // 0 = standard, 1 = Percentile, 2 = Odour
			
			InitializeComponent ();
			form1 = f;
						
			#if __MonoCS__
			    // set Numericupdowns-Alignement to Left in Linux
				var allNumUpDowns  = Main.GetAllControls<NumericUpDown>(this);
				foreach (NumericUpDown nu in allNumUpDowns)
				{
					nu.TextAlign = HorizontalAlignment.Left;
				}
			#else
			    //
			#endif
		}

        private void button1_Click(object sender, EventArgs e) // OK or Cancel button
        {
            Hide();
        }

        private void SelectSourcegroups_Load(object sender, EventArgs e)
        {
        	//fill listbox with source groups
            if (Main.DefinedSourceGroups.Count > 0)
            {
                for (int i = 0; i < form1.listView1.Items.Count; i++)
                {
                    //listBox1.Items.Add(form1.sourcegroupnames[i]);
                    listBox1.Items.Add(form1.listView1.Items[i].Text);
                }
            }
            else
            {
                for (int i = 0; i < form1.listView1.Items.Count; i++)
                {
                    listBox1.Items.Add(form1.listView1.Items[i].Text);
                }
            }
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                listBox1.SelectedIndex = i;
            }

            textBox1.Text = _prefix;
            
            if (Mode == 0) // do not show odour group box
			{
				groupBox1.Visible = false;
				groupBox2.Visible = false;				
			}
			
			if (Mode == 1)
			{
				groupBox2.Visible = true;
				numericUpDown2.Enabled = true;
				groupBox1.Visible = false;
			}
			
			if (Mode == 2)
			{
				groupBox1.Visible = true;
				groupBox2.Visible = false;		
				radioButton1.Checked = true;
				numericUpDown3.Enabled = true;
            }

            if (Mode == 3)
            {
                groupBox1.Visible = true;
                groupBox2.Visible = false;
                radioButton2.Visible = false;
                radioButton1.Visible = false;
                numericUpDown1.Visible = false;
                label2.Visible = false;
                numericUpDown3.Enabled = true;
            }        
        }
             
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
        
        void SelectSourcegroupsResizeEnd(object sender, EventArgs e)
        {
        	listBox1.Width = ClientSize.Width-5;
        	listBox1.Height = Math.Max(30, label1.Top - 5);
        }
        
        void TextBox1TextChanged(object sender, EventArgs e)
        {
        	_prefix = textBox1.Text;
        	_prefix = string.Join("_", _prefix.Split(Path.GetInvalidFileNameChars()));
        	//label1.Text = _prefix;
        }
        
        void SelectSourcegroupsShown(object sender, EventArgs e)
        {
        	
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                numericUpDown1.Enabled = false;
                numericUpDown3.Enabled = true;
            }
            else
            {
                numericUpDown1.Enabled = true;
                numericUpDown3.Enabled = false;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                numericUpDown1.Enabled = false;
                numericUpDown3.Enabled = true;
            }
            else
            {
                numericUpDown1.Enabled = true;
                numericUpDown3.Enabled = false;
            }
        }
    }
}