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
using System.Collections.Generic;
using System.Windows.Forms;

namespace GralMessage
{
	/// <summary>
    /// Form to aks for deleteing all files
    /// </summary>
    public partial class FileDeleteMessage : Form
    {
    	private bool _gramm;
		public bool DeleteGramm {set {_gramm = value;} } // Filename 
        public string DeleteMessage = string.Empty;
        public List<string> ListboxEntries;
		
        public FileDeleteMessage()
        {
            InitializeComponent();
        }

        private void FileDeleteMessageLoad(object sender, EventArgs e)
        {
			AcceptButton = button2;
			ActiveControl = button2;
			if (_gramm)
            {
                label1.Text = "Delete all wind field files at the project folder?";
            }
            if (!String.IsNullOrEmpty(DeleteMessage))
            {
                label1.Text = DeleteMessage;
            }

            // default text or individual entries?
            if (ListboxEntries.Count > 0)
            {
                foreach (string _entry in ListboxEntries)
                {
                    listView1.Items.Add(_entry);
                }
            }

            button3.BackgroundImage = System.Drawing.Bitmap.FromHicon(System.Drawing.SystemIcons.Warning.Handle);
        }

        //delete files
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        //do not delete files
        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}