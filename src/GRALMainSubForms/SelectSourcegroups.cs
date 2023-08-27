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
    /// Select a source group
    /// </summary>
    public partial class SelectSourcegroups : Form
    {
        private Main form1 = null;
        private string _prefix = "";
        private int Mode = 0;
        private bool transientMode = false;
        public string PathToEmissionModulation = string.Empty;
        public string PathForResultFiles = string.Empty;             
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
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f">Main form reference</param>
        /// <param name="_mode"> 0 = default, 1 = Percentile, 2 = Odour</param>
        /// <param name="_PathToEmissionModulation"></param>
        /// <param name="_PathForResultFiles"></param>
        public SelectSourcegroups (Main f, int _mode, string _PathToEmissionModulation, string _PathForResultFiles, bool TransientMode)
		{
			Mode = _mode; // 0 = standard, 1 = Percentile, 2 = Odour
            transientMode = TransientMode;

			if (!string.IsNullOrEmpty(_PathToEmissionModulation))
            {
                PathToEmissionModulation = _PathToEmissionModulation;
            }
            if (!string.IsNullOrEmpty(_PathForResultFiles))
            {
                PathForResultFiles = _PathForResultFiles;
            }

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
            SetPathTextBoxes();
            if (transientMode)
            {
                textBox2.Enabled = false;
                buttonModulationPath.Enabled = false;  
                label6.Enabled = false;
            }

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

                buttonModulationPath.Enabled = false;
                buttonResultPath.Enabled = false;
                textBox2.Visible = false;
                textBox3.Visible = false;

                radioButton2.Visible = false;
                radioButton1.Visible = false;
                numericUpDown1.Visible = false;
                label2.Visible = false;
                numericUpDown3.Enabled = true;
            }        
        }

        private void SetPathTextBoxes()
        {
            GralStaticFunctions.St_F.SetTrimmedTextToTextBox(textBox2, PathToEmissionModulation);
            GralStaticFunctions.St_F.SetTrimmedTextToTextBox(textBox3, PathForResultFiles);
        }
             
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
        
        void SelectSourcegroupsResizeEnd(object sender, EventArgs e)
        {
        	listBox1.Width = ClientSize.Width - 5;
        	listBox1.Height = Math.Max(30, groupBox3.Top - 10);
            groupBox3.Width = Math.Max(10, ClientSize.Width - groupBox3.Left - 15);
            textBox1.Width = Math.Max(10, groupBox3.Width - textBox1.Left - 5);
            textBox2.Width = Math.Max(10, groupBox3.Width - textBox2.Left - 5);
            textBox3.Width = Math.Max(10, groupBox3.Width - textBox3.Left - 5);
            float l1 = TextRenderer.MeasureText(PathToEmissionModulation, textBox2.Font).Width;
            textBox2.Text = GralStaticFunctions.St_F.ReduceFileNameLenght(PathToEmissionModulation, (int)(PathToEmissionModulation.Length * (textBox2.Width / l1)));
            l1 = TextRenderer.MeasureText(PathForResultFiles, textBox3.Font).Width;
            textBox3.Text = GralStaticFunctions.St_F.ReduceFileNameLenght(PathForResultFiles, (int)(PathForResultFiles.Length * (textBox3.Width / l1)));
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

        /// <summary>
        /// Select a directory for result files or the emission modulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSelectPath_Click(object sender, EventArgs e)
        {
            string path = PathForResultFiles;
            string descripton = "Select the path for the evaluation result files";
            if (sender == buttonModulationPath)
            {
                path = PathToEmissionModulation;
                descripton = "Select the path to the emission modulation files";
            }
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.InvariantCulture))
            {
                path += Path.DirectorySeparatorChar;
            }

            using (FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = descripton,
                SelectedPath = path
            })
            {
#if NET6_0_OR_GREATER
                dialog.UseDescriptionForTitle = true;
#endif
                dialog.ShowDialog();
                path = dialog.SelectedPath;
                if (Directory.Exists(path))
                {
                    if (sender == buttonModulationPath)
                    {
                        PathToEmissionModulation = path;
                    }
                    else if (sender == buttonResultPath)
                    {
                        PathForResultFiles = path;
                    }
                    SetPathTextBoxes();
                    Main.ProjectSetting.WriteToFile();
                }
            }
        }

    }
}