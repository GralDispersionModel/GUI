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
 * User: U0178969
 * Date: 21.01.2019
 * Time: 17:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace GralDomForms
{
    /// <summary>
    /// Selects overlapping items if an item is selected using the right mouse button
    /// </summary>
    public partial class SelectItem : Form
    {
        public List <string> ItemNames = new List<string>();
        public int SelectedIndex = 0;
        
        public SelectItem()
        {
            InitializeComponent();   
        }
        
        void SelectItemLoad(object sender, EventArgs e)
        {
            #if __MonoCS__
               this.AutoSizeMode = AutoSizeMode.GrowOnly;
            #else
            #endif
            if (ItemNames.Count == 0)
            {
                Close();
            }
            foreach(string mn in ItemNames)
            {
                listBox1.Items.Add(mn);
            }
            listBox1.SelectedIndex = 0;
        }
        
        void Button1Click(object sender, EventArgs e)
        {
            SelectedIndex = (int) (listBox1.SelectedIndex);
        }
        
        void Button2Click(object sender, EventArgs e)
        {
            SelectedIndex = 0; // nothing selected -> select item 1
        }
        
        void ListBox1DoubleClick(object sender, EventArgs e)
        {
            Button1Click(null, null);
        }
    }
    
    
}
