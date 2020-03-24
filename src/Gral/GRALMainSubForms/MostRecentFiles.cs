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

/*
 * Created by SharpDevelop.
 * User: Markus
 * Date: 16.09.2016
 * Time: 18:53
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace GralMainForms
{
	/// <summary>
	/// Show the most recent files and allow the user to select a project to be opened
	/// </summary>
	public partial class MostRecentFiles : Form
	{
		private List<string> _folderlist = new List<string>();
		public List<string> FolderList {set {_folderlist = value;} get {return _folderlist;} }
		
		private string _newFile;
		public string NewFile { set {_newFile = value;}}
		
		private string _selectedFile;
		public string SelectedFile { get { return _selectedFile;}}
		
		public MostRecentFiles()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
		}
		
		void MostRecentFilesLoad(object sender, EventArgs e)
		{
			listView1.Width = ClientRectangle.Width;
			listView1.TileSize = new Size(listView1.Width - 20, 25);
			Read();
			listView1.Clear();
			foreach(string item in _folderlist)
			{
				listView1.Items.Add(item);
			}
			_selectedFile = ""; // initally no file selected
		}
		
		// Read the file "RecentFiles.txt"
		public bool Read()
		{
			string RecentFiles = Path.Combine(Gral.Main.App_Settings_Path, @"RecentFiles.txt");
			if (File.Exists(RecentFiles))
			{
				try
				{
					using (StreamReader myreader = new StreamReader(RecentFiles))
					{
						while (!myreader.EndOfStream)
						{
							string temp = myreader.ReadLine();
							_folderlist.Add(temp);
						}
					}
				
				}
				catch
				{
					return false;
				}
			}
			return true;
		}
		
		// Write _newFile to file "RecentFiles.txt"
		public bool Write()
		{
			// Read the folderlist and add _newFile
			Read();
			if (_folderlist.Count == 0)
			{
				_folderlist.Add(_newFile);
			}
			else
			{
				if (_folderlist[0] == _newFile) // re-open actual top file -> do nothing & exit
					return true;
				
				_folderlist.Insert(0, _newFile); // insert _newFile at top of the list
				
				for (int i = 1; i < FolderList.Count; i++) //  
				{
					if (_folderlist[i] == _newFile)
					{
						_folderlist.RemoveAt(i); // Remove duplicate
						break; // exit loop
					}
				}
			}
			
			string RecentFiles = Path.Combine(Gral.Main.App_Settings_Path, @"RecentFiles.txt");
			try
			{
				using (StreamWriter mywriter = new StreamWriter(RecentFiles))
				{
					int i = 0;
					foreach (string temp in _folderlist)
					{
						mywriter.WriteLine(temp);
						i++;
						if (i > 9) break; // save max. 10 files 
					}
				}

			}
			catch
			{
				return false;
			}
			return true;
		}
		
		
		void ListView1DoubleClick(object sender, EventArgs e)
		{
			_selectedFile = listView1.SelectedItems[0].Text;
			DialogResult = DialogResult.OK;
		}
		void Button2Click(object sender, EventArgs e)
		{
			Close();
		}
		void Button1Click(object sender, EventArgs e)
		{
			if (listView1.SelectedIndices.Count > 0)
				_selectedFile = listView1.SelectedItems[0].Text;
			Close();
		}
		void MostRecentFilesSizeChanged(object sender, EventArgs e)
		{
			listView1.Width = ClientRectangle.Width;
			listView1.Height = Math.Max(30, ClientRectangle.Height - 123);
			listView1.TileSize = new Size(Math.Max(0, listView1.Width - 20), 25);
		}
		
	}
}
