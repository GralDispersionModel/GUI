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
using System.Globalization;
using System.IO;
using System.Windows.Forms;

using Gral;
using GralItemData;
using GralDomain;

namespace GralItemForms
{
    public partial class Objectmanager : Form
    {
        private GralDomain.Domain domain = null;
        private String decsep;
		// delegate to send Message, that redraw is needed!
		public event ForceDomainRedraw object_redraw;   

        public Objectmanager(GralDomain.Domain f)
        {
            InitializeComponent();
            domain = f;
            listBox1.DrawItem +=
                 new System.Windows.Forms.DrawItemEventHandler(listBox1_DrawItem);
            listBox1.ItemHeight = listBox1.Font.Height;
            decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
     	}

        //OK button
        public void button5_Click(object sender, EventArgs e)
        {
            domain.SaveDomainSettings(0);
            domain.Refresh();
            domain.MouseControl = 0;
            domain.Cursor = Cursors.Default;
            Hide();
            RedrawDomain(this, null);
            //this.Close();
            //domain.objectmanager = null; // Kuntner release objectmanager!
        }
	
        //move selected object and all corresponding properties up
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                
                if (listBox1.SelectedItems.Count > 1) // if more than one item is selected
                {
                	MessageBox.Show(this, "More than one item selected", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                	return;
                }
                	
                
                int index = listBox1.SelectedIndex;
                if (index > 0)
                {
                	// close all Layout-forms
					Layout_Close_Forms();
					DrawingObjects _drobj = domain.ItemOptions[index];
					
					domain.ItemOptions.Insert(index - 1, _drobj);
                    domain.ItemOptions.RemoveAt(index + 1);
                    listBox1.Items.Insert(index - 1, listBox1.SelectedItem);
					listBox1.Items.RemoveAt(index + 1);

					#if __MonoCS__
					listBox1.ClearSelected ();
					#endif
                    listBox1.SelectedIndex = index - 1;
                }
            }
            catch
            {
            }
        }

        //move selected object and all corresponding properties down
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
            	if (listBox1.SelectedItems.Count > 1) // if more than one item is selected
            	{
            		MessageBox.Show(this, "More than one item selected", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                	return;
            	}
                
                int index = listBox1.SelectedIndex;
                if (index < listBox1.Items.Count - 1 && index >= 0)
                {
                	// close all Layout-forms
					Layout_Close_Forms();
					DrawingObjects _drobj = domain.ItemOptions[index];
					
					domain.ItemOptions.Insert(index + 2, _drobj);
					domain.ItemOptions.RemoveAt(index);
					
					listBox1.Items.Insert(index + 2, _drobj.Name);
                    listBox1.Items.RemoveAt(index);
                    
                    listBox1.SelectedIndex = index + 1;
                }
            }
            catch
            {
            }
        }

        //apply button
        private void button3_Click(object sender, EventArgs e)
        {
            domain.button8.Visible = false;
            domain.button10.Visible = false;
            domain.button12.Visible = false;
            domain.button14.Visible = false;
            domain.button16.Visible = false;
            domain.button23.Visible = false;
            domain.button49.Visible = false;
            
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
            	if ((Convert.ToString(listBox1.Items[i]) == "POINT SOURCES") && (domain.ItemOptions[i].Show == true))
                    domain.button8.Visible = true;
                if ((Convert.ToString(listBox1.Items[i]) == "AREA SOURCES") && (domain.ItemOptions[i].Show == true))
                    domain.button10.Visible = true;
                if ((Convert.ToString(listBox1.Items[i]) == "LINE SOURCES") && (domain.ItemOptions[i].Show == true))
                    domain.button12.Visible = true;
                if ((Convert.ToString(listBox1.Items[i]) == "TUNNEL PORTALS") && (domain.ItemOptions[i].Show == true))
                    domain.button14.Visible = true;
                if ((Convert.ToString(listBox1.Items[i]) == "BUILDINGS") && (domain.ItemOptions[i].Show == true))
                    domain.button16.Visible = true;
                if ((Convert.ToString(listBox1.Items[i]) == "RECEPTORS") && (domain.ItemOptions[i].Show == true))
                    domain.button23.Visible = true;
                if ((Convert.ToString(listBox1.Items[i]) == "WALLS") && (domain.ItemOptions[i].Show == true))
                    domain.button49.Visible = true;
            }
            
           RedrawDomain(this, null);
        }

        void Button6Click(object sender, EventArgs e)
        {
        	if (listBox1.SelectedIndex > -1)
        		listBox1_DoubleClick(null, null);
        }

        //open layout manager
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
        	if (listBox1.SelectedItems.Count > 1) // if more than one item is selected
        	{
        		MessageBox.Show(this, "More than one item selected", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
               	return;
        	}
                
            Layout layout = new Layout(domain);
            //fill combobox1 with source groups and combobox2 with items
            layout.label1.Visible = false;
            layout.comboBox1.Visible = false;
            layout.comboBox1.Items.Add("ALL");
            layout.label2.Visible = false;
            layout.comboBox2.Visible = false;
            layout.comboBox2.Items.Add("NONE");
            layout.DrawObject = domain.ItemOptions[listBox1.SelectedIndex];
            
            Cursor = Cursors.WaitCursor;
            layout.Text = "Layout" +  "  " + Convert.ToString(listBox1.SelectedItem);           
            
                        
            if (Convert.ToString(listBox1.SelectedItem) == "AREA SOURCES")
            {
                layout.label1.Visible = true;
                layout.comboBox1.Visible = true;
                layout.label2.Visible = true;
                layout.comboBox2.Visible = true;
                
                foreach (AreaSourceData _as in domain.EditAS.ItemData)
                {
                    updatecombobox1(_as.Poll.SourceGroup, layout);

                    string poll = "";
                    bool exist = false;
                    for (int j = 0; j < 10; j++)
                    {
                        for (int i1 = 0; i1 < Main.PollutantList.Count; i1++)
                        {
                        	if (_as.Poll.Pollutant[j] == i1)
                        		if (_as.Poll.EmissionRate[j] != 0)
                                {
                                    poll = Main.PollutantList[i1];
                                    exist = false;
                                    foreach (string polllist in layout.comboBox2.Items)
                                    {
                                        if (polllist == poll)
                                        {
                                            exist = true;
                                            break;
                                        }
                                    }
                                    if ((exist == false) && (poll != ""))
                                    {
                                        layout.comboBox2.Items.Add(poll);
                                        break;
                                    }
                                    else
                                        break;
                                }
                                else
                                    break;
                        }
                    }
                }
               
                for (int j = 1; j < layout.comboBox1.Items.Count; j++)
                {
                    string[] text = new string[2];
                    text = Convert.ToString(layout.comboBox1.Items[j]).Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        if ((" " + Convert.ToString(domain.ItemOptions[listBox1.SelectedIndex].SourceGroup)) == text[1])
                        {
                            layout.comboBox1.SelectedIndex = j;
                            break;
                        }
                    }
                    catch
                    {
                        if (Convert.ToString(domain.ItemOptions[listBox1.SelectedIndex].SourceGroup) == text[0])
                        {
                            layout.comboBox1.SelectedIndex = j;
                            break;
                        }
                    }
                }

                for (int j = 1; j < layout.comboBox2.Items.Count; j++)
                {
                    for(int i1=0;i1<Main.PollutantList.Count; i1++)
                        if (Convert.ToString(layout.comboBox2.Items[j]) == Main.PollutantList[i1])
                        {
                            if (i1 == domain.ItemOptions[listBox1.SelectedIndex].Item)
                            {
                                layout.comboBox2.SelectedIndex = j;
                                break;
                            }
                        }

                }
            }

            if (Convert.ToString(listBox1.SelectedItem) == "LINE SOURCES")
            {
                layout.label1.Visible = true;
                layout.comboBox1.Visible = true;
                layout.label2.Visible = true;
                layout.comboBox2.Visible = true;
                layout.comboBox2.Items.Add("Av. daily traffic");
                layout.comboBox2.Items.Add("Slope [%]");
                layout.comboBox2.Items.Add("Share HDV [%]");
                foreach (LineSourceData _ls in domain.EditLS.ItemData)
                {
                    for (int j = 0; j < _ls.Poll.Count; j++)
                    {
                    	updatecombobox1(_ls.Poll[j].SourceGroup, layout);
                    }
                    
                    string poll = "";
                    bool exist = false;
                    for (int k = 0; k < _ls.Poll.Count; k++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            for (int i1 = 0; i1 < Main.PollutantList.Count; i1++)
                            {
                            	if ((_ls.Poll[k].Pollutant[j] == i1))
                                {
                            		if (_ls.Poll[k].EmissionRate[j] != 0)
                                    {
                                        poll = Main.PollutantList[i1];
                                        exist = false;
                                        foreach (string polllist in layout.comboBox2.Items)
                                        {
                                            if (polllist == poll)
                                            {
                                                exist = true;
                                                break;
                                            }
                                        }
                                        if ((exist == false) && (poll != ""))
                                        {
                                            layout.comboBox2.Items.Add(poll);
                                            break;
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        break;
                                }
                            }
                        }
                    }
                }
                for (int j = 1; j < layout.comboBox1.Items.Count; j++)
                {
                    string[] text = new string[2];
                    text = Convert.ToString(layout.comboBox1.Items[j]).Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        if ((" " + Convert.ToString(domain.ItemOptions[listBox1.SelectedIndex].SourceGroup)) == text[1])
                        {
                            layout.comboBox1.SelectedIndex = j;
                            break;
                        }
                    }
                    catch
                    {
                        if (Convert.ToString(domain.ItemOptions[listBox1.SelectedIndex].SourceGroup) == text[0])
                        {
                            layout.comboBox1.SelectedIndex = j;
                            break;
                        }
                    }
                }

                for (int j = 1; j < layout.comboBox2.Items.Count; j++)
                {
                    for (int i1 = 0; i1 < Main.PollutantList.Count; i1++)
                        if (Convert.ToString(layout.comboBox2.Items[j]) == Main.PollutantList[i1])
                        {
                            if (i1 == domain.ItemOptions[listBox1.SelectedIndex].Item)
                            {
                                layout.comboBox2.SelectedIndex = j;
                                break;
                            }
                        }

                }
            }
            
            if (Convert.ToString(listBox1.SelectedItem) == "BUILDINGS")
            {
                layout.label2.Visible = true;
                layout.comboBox2.Visible = true;
                layout.comboBox2.Items.Add("Height [m]");
                if (domain.ItemOptions[listBox1.SelectedIndex].Item == 1)
                    layout.comboBox2.SelectedIndex = 1;
            }

            if (Convert.ToString(listBox1.SelectedItem) == "VEGETATION")
            {
                layout.label2.Visible = true;
                layout.comboBox2.Visible = true;
                layout.comboBox2.Items.Add("Height [m]");
                if (domain.ItemOptions[listBox1.SelectedIndex].Item == 1)
                    layout.comboBox2.SelectedIndex = 1;
            }

            if (Convert.ToString(listBox1.SelectedItem) == "RECEPTORS")
            {
                layout.label2.Visible = true;
                layout.comboBox2.Visible = true;
                layout.comboBox2.Items.Add("Display value");
                if (domain.ItemOptions[listBox1.SelectedIndex].Item == 1)
                    layout.comboBox2.SelectedIndex = 1;
            }

            Cursor = Cursors.Default;
            layout.StartPosition = FormStartPosition.Manual;
            int x0 = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 350;
            if (this.Left < x0 + 600)
            {
                layout.Left = this.Right + 10;
            }
            else
            {
                layout.Left = this.Left - 10 - layout.Width;
            }
            layout.Top = this.Top;
            layout.Show();
        }

        //show/hide selected layer
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
            	for (int index = listBox1.Items.Count-1; index >= 0; index--) // check all items
        		{
            		if (listBox1.GetSelected(index) == true) // change selected items
            		{
            			if (checkBox1.Checked == true)
            			{
            				domain.ItemOptions[index].Show = true;
            			}
            			else
            			{
            				domain.ItemOptions[index].Show = false;
            			}
            		}
            	}
            }
            catch
            {
            }
        }

        //select layer
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (domain.ItemOptions[listBox1.SelectedIndex].Show == true)
                    checkBox1.Checked = true;
                else
                    checkBox1.Checked = false;
            }
            catch
            {
            }

            //hide or show button to remove files
            button4.Visible = false;
            button6.Visible = false;
            
            if (listBox1.SelectedIndex > -1) // item selected
            	button6.Visible = true;	// show button Layout
            
            try
            {
                if (domain.ItemOptions[listBox1.SelectedIndex].Name.Substring(0, 3) == "CM:")
                    button4.Visible = true;
                if (domain.ItemOptions[listBox1.SelectedIndex].Name.Substring(0, 3) == "VM:")
                    button4.Visible = true;
                if (domain.ItemOptions[listBox1.SelectedIndex].Name.Substring(0, 3) == "BM:")
                    button4.Visible = true;
                if (domain.ItemOptions[listBox1.SelectedIndex].Name.Substring(0, 3) == "PM:")
                    button4.Visible = true;
                if (domain.ItemOptions[listBox1.SelectedIndex].Name.Substring(0, 3) == "SM:")
                    button4.Visible = true;
                if (domain.ItemOptions[listBox1.SelectedIndex].Name.Equals("NORTH ARROW"))
                	button4.Visible = true;
                if (domain.ItemOptions[listBox1.SelectedIndex].Name.Equals("SCALE BAR"))
                	button4.Visible = true;
                if (domain.ItemOptions[listBox1.SelectedIndex].Name.StartsWith("CONCENTRATION VALUES"))
                	button4.Visible = true;
                if (domain.ItemOptions[listBox1.SelectedIndex].Name.StartsWith("WINDROSE"))
                    button4.Visible = true;
            }
            catch
            {
            }
        }

        private void Objectmanager_Load(object sender, EventArgs e)
        {
            try
            {
            	domain.ObjectManagerForm.Left = domain.Left+40;
			    domain.ObjectManagerForm.Top = domain.Top +120;
			
                listBox1.SelectedIndex = 0;
                
                if (domain.ItemOptions[0].Show == true)
                    checkBox1.Checked = true;
                else
                    checkBox1.Checked = false;
            }
            catch
            {
            }
        }

        //change the color of the font when an object is hided or shown
        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Brush myBrush = Brushes.Black;
            if (e.Index < domain.ItemOptions.Count)
            {
            	if (domain.ItemOptions[e.Index].Show == false)
            	{
            		myBrush = Brushes.Gray;
            		e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), e.Font, myBrush, e.Bounds);
            	}
            	else
            	{
            		myBrush = Brushes.Black;
            		e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), e.Font, myBrush, e.Bounds);
            	}
            }
        }
        
        //update source groups within the model domain
        public void updatecombobox1(int SGNumber, Layout layout)
        {
            bool exist = false;
            string[] text = new string[2];
            for (int i = 0; i < layout.comboBox1.Items.Count; i++)
            {
                text = Convert.ToString(layout.comboBox1.Items[i]).Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (text.Length > 1)
                {
                	int _sg = 0;
                	int.TryParse(text[1], out _sg);
                    if (_sg == SGNumber)
                    {
                        exist = true;
                        break;
                    }
                }
                else
                {
                    int _sg = 0;
                	int.TryParse(text[0], out _sg);
                    if (_sg == SGNumber)
                    {
                        exist = true;
                        break;
                    }
                }
            }
            if (exist == false)
            {
                int index = -1;
                combo(SGNumber, ref index);
                if (index > -1)
                    layout.comboBox1.Items.Add(Main.DefinedSourceGroups[index].SG_Name + ": " + Main.DefinedSourceGroups[index].SG_Number.ToString());
                else
                    layout.comboBox1.Items.Add(SGNumber);
            }
        }
        
        //search for the correct source group within the defined source groups
        private void combo(int SGNumber, ref int index)
        {
            int i = 0;
            foreach (SG_Class _sg in Main.DefinedSourceGroups)
            {
            	int sg = GralStaticFunctions.St_F.GetSgNumber(_sg.SG_Name);
                if (sg == SGNumber)
                {
                    index = i;
                    break;
                }
                i = i + 1;
            }
        }

        void ListBox1KeyDown(object sender, KeyEventArgs e)
        {
        	if (e.KeyCode == Keys.Delete)
        	{
        		if (MessageBox.Show(this, "Remove selected items?", "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        			button4_Click(null, null);
        	}
            if (e.KeyCode == Keys.Space)
            {
                if (listBox1.SelectedItems.Count > 0)
                {
                    checkBox1.Checked = !checkBox1.Checked;
                }
            }
            if (e.KeyCode == Keys.Enter)
            {
                listBox1_DoubleClick(null, null);
            }

        }

        private void listBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    e.IsInputKey = true;
                    break;
            }
        }
        //remove selected maps
        private void button4_Click(object sender, EventArgs e)
        {
        	//int index = listBox1.SelectedIndex;
        	int index = listBox1.Items.Count;
        	while (index > 0)
        	{
        		// Close Layout forms
				Layout_Close_Forms();
				
        		index --;
        		if (listBox1.GetSelected(index) == true) // item selected?
        		{
        			// just delete allowed items
        			if (domain.ItemOptions[index].Name.Substring(0, 3) == "CM:" ||  
        			    domain.ItemOptions[index].Name.Substring(0, 3) == "VM:" ||
        			    domain.ItemOptions[index].Name.Substring(0, 3) == "BM:" ||
        			    domain.ItemOptions[index].Name.Substring(0, 3) == "PM:" ||
        			    domain.ItemOptions[index].Name.Substring(0, 3) == "SM:" ||
        			    domain.ItemOptions[index].Name.Equals("NORTH ARROW")    ||
        			    domain.ItemOptions[index].Name.Equals("SCALE BAR")      ||
        			    domain.ItemOptions[index].Name.StartsWith("CONCENTRATION VALUES") ||
                        domain.ItemOptions[index].Name.Equals("WINDROSE"))
        			{
        				
        				// delete contour files, because it's not possible to load them again
        				if (Convert.ToString(domain.ItemOptions[index].Name).Substring(0, 3) == "VM:") // Vector map
        				{
        					string file = domain.ItemOptions[index].ContourFilename;
        					try
        					{
        						if (File.Exists(file))
        							File.Delete(file);
        						string path = Path.GetDirectoryName(file);
        						
        						file = Path.Combine(path, Path.GetFileNameWithoutExtension(file) + ".u");
        						if (File.Exists(file))
        							File.Delete(file);
        						
        						file = Path.Combine(path, Path.GetFileNameWithoutExtension(file) + ".v");
        						if (File.Exists(file))
        							File.Delete(file);
        					}
        					catch {}
        				}

                        domain.ItemOptions[index].Show = false;
                        domain.RemoveMap(index);
        				        				
        				listBox1.Items.RemoveAt(index);
        			}
        		}
        	}
        }
        
        void ObjectmanagerFormClosed(object sender, FormClosedEventArgs e)
        {
        	// Close all Layout-Forms
			Layout_Close_Forms();
			listBox1.DrawItem -=
                 new System.Windows.Forms.DrawItemEventHandler(listBox1_DrawItem);
			listBox1.Items.Clear();
			listBox1.Dispose();
			toolTip1.Dispose();
        	domain.ObjectManagerForm = null; // Kuntner release objectmanager!
        }
       
		public void Layout_Close_Forms()
		{
			for(int i = Application.OpenForms.Count-1; i >=0; i--)
			{
				if(Application.OpenForms[i].Name.StartsWith("Layout"))
					Application.OpenForms[i].Close();
			}
		}
		
		private void RedrawDomain(object sender, EventArgs e)
		{
			// send Message to domain Form, that redraw is necessary
			try
			{
				if (object_redraw != null)
					object_redraw(this, e);
			}
			catch
			{}
		}

        
    }
}