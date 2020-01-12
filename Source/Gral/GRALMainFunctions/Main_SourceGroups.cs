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
 * Date: 28.10.2016
 * Time: 18:42
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using GralItemData;
using System.Drawing;
using System.Windows.Forms;
using GralMessage;

namespace Gral
{
	public partial class Main
	{
		//select all source groups within the defined model domain
		public void SelectAllUsedSourceGroups()
		{
			listBox4.Items.Clear();
			
			List<PointSourceData> _psList = new List<PointSourceData>();
			PointSourceDataIO _ps = new PointSourceDataIO();
			string _file = Path.Combine(ProjectName,"Emissions","Psources.txt");
			_ps.LoadPointSources(_psList, _file);
			_ps = null;
			
			foreach (PointSourceData _psd in _psList )
			{
				if ((_psd.Pt.X >= GralDomRect.West) && (_psd.Pt.X <= GralDomRect.East) &&
				    (_psd.Pt.Y >= GralDomRect.South) && (_psd.Pt.Y <= GralDomRect.North))
				{
					UpdateSourceGroupsInListBox4(_psd.Poll.SourceGroup);
				}
			}
			
			_psList.Clear();
			_psList.TrimExcess();
			
			List <AreaSourceData> _asList = new List<AreaSourceData>();
			AreaSourceDataIO _as = new AreaSourceDataIO();
			_file = Path.Combine(Main.ProjectName,"Emissions","Asources.txt");
			_as.LoadAreaData(_asList, _file);
			_as = null;
			
			foreach (AreaSourceData _asd in _asList)
			{
				//filter domain
				double xmin = double.MaxValue;
				double xmax = double.MinValue;
				double ymin = double.MaxValue;
				double ymax = double.MinValue;
				
				foreach (GralDomain.PointD _pt in _asd.Pt)
				{
					xmin = Math.Min(xmin, _pt.X);
					xmax = Math.Max(xmax, _pt.X);
					ymin = Math.Min(ymin, _pt.Y);
					ymax = Math.Max(ymax, _pt.Y);
				}
				
				if ((xmin >= GralDomRect.West) && (xmax <= GralDomRect.East) &&
				    (ymin >= GralDomRect.South) && (ymax <= GralDomRect.North))
				{
					UpdateSourceGroupsInListBox4(_asd.Poll.SourceGroup);
				}
			}

			for (int i = 0; i < _asList.Count; i++)
			{
			    _asList[i].Pt.Clear();
			    _asList[i].Pt.TrimExcess();
			}
			_asList.Clear();
			_asList.TrimExcess();
			
			
			List <LineSourceData> _lsList = new List<LineSourceData>();
			LineSourceDataIO _ls = new LineSourceDataIO();
			_file = Path.Combine(Main.ProjectName,"Emissions","Lsources.txt");
			_ls.LoadLineSources(_lsList, _file);
			_ls = null;
			
			foreach (LineSourceData _lsdata in _lsList)
			{
				//filter domain
				double xmin = double.MaxValue;
				double xmax = double.MinValue;
				double ymin = double.MaxValue;
				double ymax = double.MinValue;
				
				foreach (GralDomain.PointD _pt in _lsdata.Pt)
				{
					xmin = Math.Min(xmin, _pt.X);
					xmax = Math.Max(xmax, _pt.X);
					ymin = Math.Min(ymin, _pt.Y);
					ymax = Math.Max(ymax, _pt.Y);
				}
				
				if ((xmin >= GralDomRect.West) && (xmax <= GralDomRect.East) &&
				    (ymin >= GralDomRect.South) && (ymax <= GralDomRect.North))
				{
					foreach (PollutantsData _poll in _lsdata.Poll)
					{
						UpdateSourceGroupsInListBox4(_poll.SourceGroup);
					}
				}
			}
			for (int i = 0; i < _lsList.Count; i++)
			{
			    _lsList[i].Pt.Clear();
			    _lsList[i].Pt.TrimExcess();
			}
			_lsList.Clear();
			_lsList.TrimExcess();
			
			List<PortalsData> _poList = new List<PortalsData>();
			PortalsDataIO _pd = new PortalsDataIO();
			_file = Path.Combine(Main.ProjectName,"Emissions","Portalsources.txt");
			_pd.LoadPortalSources(_poList, _file);
			_pd = null;

			foreach (PortalsData _podata in _poList)
			{
				if ((_podata.Pt1.X >= GralDomRect.West) && (_podata.Pt1.X <= GralDomRect.East) &&
				    (_podata.Pt1.Y >= GralDomRect.South) && (_podata.Pt1.Y <= GralDomRect.North) &&
				    (_podata.Pt2.X >= GralDomRect.West) && (_podata.Pt2.X <= GralDomRect.East) &&
				    (_podata.Pt2.Y >= GralDomRect.South) && (_podata.Pt2.Y <= GralDomRect.North))
				{
					foreach (PollutantsData _poll in _podata.Poll)
					{
						UpdateSourceGroupsInListBox4(_poll.SourceGroup);
					}
				}
			}
			
			_poList.Clear();
			_poList.TrimExcess();
		}
		
		/// <summary>
        /// Show the dialog to define the source groups
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowDefineSourceGroupsDialog(object sender, EventArgs e)
		{
			using (GralMainForms.Sourcegroups defsources = new GralMainForms.Sourcegroups(this))
			{
				defsources.ShowDialog();
			}
		}

        /// <summary>
        /// update source groups within the model domain
        /// </summary>
        /// <param name="sg"></param>
        public void UpdateSourceGroupsInListBox4(int sg)
		{
			if (textBox2.Text != "")
			{
				bool exist = false;
				string[] text = new string[2];
				for (int i = 0; i < listBox4.Items.Count; i++)
				{
					text = Convert.ToString(listBox4.Items[i]).Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
					if (text.Length > 1)
					{
						int _sg = 0;
						int.TryParse(text[1], out _sg);
						if (_sg == sg)
						{
							exist = true;
							break;
						}
					}
					else
					{
						int _sg = 0;
						int.TryParse(text[0], out _sg);
						if (_sg == sg)
						{
							exist = true;
							break;
						}
					}
				}
				
				if (exist == false)
				{
					int index = -1;
					ComboSearchSourceGroup(sg, ref index);
					if (index > -1)
						listBox4.Items.Add(DefinedSourceGroups[index].SG_Name + ": " + DefinedSourceGroups[index].SG_Number.ToString());
					else
						listBox4.Items.Add(sg);
					
					// sort Listbox entries
					List <SG_Class> _sg = new List<SG_Class>();
					for (int i = 0; i < listBox4.Items.Count; i++)
					{
						text = Convert.ToString(listBox4.Items[i]).Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
						if (text.Length > 1)
						{
							int s_numb = 0;
							if (int.TryParse(text[1], out s_numb))
							{
								_sg.Add(new SG_Class{SG_Name = text[0], SG_Number = s_numb});
							}
						}
						else if (text.Length == 1) // Source group not defined but used!
						{
							int s_numb = 0;
							if (int.TryParse(text[0], out s_numb))
							{
								_sg.Add(new SG_Class{SG_Name = text[0], SG_Number = s_numb});
							}
						}
					}
					_sg.Sort();
					
					listBox4.Items.Clear();
					foreach(SG_Class _sgi in _sg)
					{
						listBox4.Items.Add(_sgi.SG_Name + ": " + _sgi.SG_Number.ToString());
					}
					// sort LIstbox entries
					
					listView1.Items.Clear();
				}
				
			}
			else
			{
				listBox4.Items.Clear();
				listView1.Items.Clear();
			}
		}

        /// <summary>
        /// search for the correct source group within the defined source groups
        /// </summary>
        /// <param name="SG_Number"></param>
        /// <param name="index"></param>
        private void ComboSearchSourceGroup(int SG_Number, ref int index)
		{
			int i = 0;
			foreach (SG_Class _sg in DefinedSourceGroups)
			{
				if (_sg.SG_Number == SG_Number)
				{
					index = i;
					break;
				}
				i++;
			}
		}

        /// <summary>
        /// select and add existing source groups in the defined model domain for the simulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSourceGroups(object sender, EventArgs e)
		{
			if (ProjectName == "") return; // exit if no project loaded
			foreach (string text in listBox4.SelectedItems)
			{
				bool exist = false;
				for (int i = 0; i < listView1.Items.Count; i++)
				{
					string text1 = listView1.Items[i].SubItems[0].Text;
					if (text == text1)
					{
						exist = true;
						break;
					}
				}
				if (exist == false)
				{
					listView1.Items.Add(text);
					listView1.Items[listView1.Items.Count - 1].SubItems[0].BackColor = Color.LightGoldenrodYellow;
					DeleteEmissionFiles();
				}
			}
			WriteGralGebFile();

			Cursor = Cursors.WaitCursor;
			MessageWindow message = new MessageWindow();
			message.Show();
			message.listBox1.Items.Add("Collect pollutants for selected source groups...");
			message.Refresh();
			
			//select existing pollutants for the selected source groups
			CollectAllUsedPollutants();
			CheckIfAllSourcegroupsHaveDefinedEmissionModulations(true);

			message.Close();
			message.Dispose();
			Cursor = Cursors.Default;

			for (int i = 0; i < listView1.Items.Count; ++i)
			{
				if (listView1.Items.Count > 0)
				{
					GralMainForms.Emissionvariation emvar = new GralMainForms.Emissionvariation(this);
					emvar.Close_Emissionvariation(i);
					emvar.Dispose();
				}
			}
			SetWetDepositionData();
			//			listBox5.Items.Clear();
			//			button18.Visible = false;
			//			button21.Visible = false;
			//			Change_Label(2, -1); // Emission label invisible

			//enable/disable GRAL simulations
			Enable_GRAL();
		}

		        //write source groups to "GRAL.geb"
        public void WriteGralGebFile()
        {
            if (EmifileReset == true) // block writing while open a project
            {
                try
                {
                    string filename1 = Path.Combine("Computation", "GRAL.geb");
                    string newPath1 = Path.Combine(ProjectName, filename1);

                    using (StreamWriter myWriter1 = File.CreateText(newPath1))
                    {
                        CellsGralX = Convert.ToInt32((GralDomRect.East - GralDomRect.West) / HorGridSize);
                        CellsGralY = Convert.ToInt32((GralDomRect.North - GralDomRect.South) / HorGridSize);
                        myWriter1.WriteLine(Convert.ToString(numericUpDown10.Value, ic) + "               !cell-size for cartesian wind field in GRAL in x-direction");
                        myWriter1.WriteLine(Convert.ToString(numericUpDown10.Value, ic) + "               !cell-size for cartesian wind field in GRAL in y-direction");

                        string a = Convert.ToString(numericUpDown11.Value, ic) + "," + Convert.ToString(numericUpDown12.Value, ic);
                        if (FlowFieldStretchFlexible.Count > 0) // flexible stretching factors
                        {
                            foreach(float[] _val in FlowFieldStretchFlexible)
                            {
                                a = a + "," + _val[0].ToString(ic) + "," + _val[1].ToString(ic); // write height and stretching factor
                            }
                        }

                        a = a +  "                !cell-size for cartesian wind field in GRAL in z-direction, streching factor for increasing cells heights with height";
                        myWriter1.WriteLine(a);


                        myWriter1.WriteLine(Convert.ToString(CellsGralX) + "                !number of cells for counting grid in GRAL in x-direction");
                        myWriter1.WriteLine(Convert.ToString(CellsGralY) + "                !number of cells for counting grid in GRAL in y-direction");
                        myWriter1.WriteLine(Convert.ToString(GRALSettings.NumHorSlices) + "                !Number of horizontal slices");
                        for (int index = 0; index < listView1.Items.Count; index++)
                        {
                            string[] sg = new string[2];
                            sg = listView1.Items[index].SubItems[0].Text.Split(new char[] { ':' });
                            try
                            {
                                sg[1] = sg[1].Trim();
                                myWriter1.Write(sg[1] + ",");
                            }
                            catch
                            {
                                myWriter1.Write(sg[0] + ",");
                            }
                        }
                        myWriter1.WriteLine("                !Source groups to be computed seperated by a comma");
                        myWriter1.WriteLine(textBox6.Text + "                !West border of GRAL model domain [m]");
                        myWriter1.WriteLine(textBox7.Text + "                !East border of GRAL model domain [m]");
                        myWriter1.WriteLine(textBox5.Text + "                !South border of GRAL model domain [m]");
                        myWriter1.WriteLine(textBox2.Text + "                !North border of GRAL model domain [m]");
                    }
                }
                catch
                {
                    MessageBox.Show("Can't write Gral.geb", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //generate file GRAMM_geb, when GRAMM is used for flow simulations around buildings
                if (GRALSettings.BuildingMode == 3)
                {
                    textBox13.Text = textBox6.Text;
                    textBox12.Text = textBox7.Text;
                    textBox14.Text = textBox5.Text;
                    textBox15.Text = textBox2.Text;
                    GrammDomRect.East = GralDomRect.East;
                    GrammDomRect.West = GralDomRect.West;
                    GrammDomRect.North = GralDomRect.North;
                    GrammDomRect.South = GralDomRect.South;
                    GRAMMHorGridSize = HorGridSize;
                    numericUpDown18.Value = Convert.ToDecimal(GRAMMHorGridSize);
                    numericUpDown17.Value = Convert.ToDecimal("1");
                    numericUpDown19.Value = Convert.ToDecimal(1.1);
                    WriteGrammGebFile();
                    groupBox10.Visible = true;
                }
            }
        }

        /// <summary>
        /// unselect existing source groups in the defined model domain for the simulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveSourceGroups(object sender, EventArgs e)
        {
            if (ProjectName == "") return; // exit if no project loaded
            for (int index = 0; index < listView1.Items.Count; index++)
            {
                if (listView1.Items[index].Selected == true)
                {
                    //delete the modulation file for the unselected source group
                    string[] sg = new string[2];
                    sg = listView1.Items[index].SubItems[0].Text.Split(new char[] { ':' });
                    string name = "";
                    try
                    {
                        sg[1] = sg[1].Trim();
                        name = "00" + sg[1];
                    }
                    catch
                    {
                        name = "00" + sg[0];
                    }
                    name = name.Substring(name.Length - 3);
                    try
                    {
                        string newPath = Path.Combine(ProjectName, @"Computation", "emissions" + name + ".dat");
                        File.Delete(newPath);
                    }
                    catch
                    {
                    }
                    listView1.Items.RemoveAt(index);
                    index = index - 1;
                }
            }
            WriteGralGebFile();
            DeleteEmissionFiles();

            //select existing pollutants for the selected source groups
            Cursor = Cursors.WaitCursor;
            MessageWindow message = new MessageWindow();
            message.Show();
            message.listBox1.Items.Add("Collect pollutants for selected source groups...");
            message.Refresh();
            listBox5.Items.Clear();
            Change_Label(2, -1); // Emission label invisible

            button18.Visible = false;
            button21.Visible = false;
            button48.Visible = false;

            Pollmod.Clear();
            
            CollectAllUsedPollutants();

            CheckIfAllSourcegroupsHaveDefinedEmissionModulations(true);
            
            message.Close();
            message.Dispose();

            Cursor = Cursors.Default;

            //enable/disable GRAL simulations
            Enable_GRAL();
        }
        
        private void CheckIfAllSourcegroupsHaveDefinedEmissionModulations(bool Change_Emission_Label)
        {
           //check if all source groups have defined emission modulations
            int check = 1;
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].SubItems[0].BackColor == Color.LightGoldenrodYellow)
                    check = 0;
            }
            
            //fill form1.listbox5 with available pollutants
            if (check == 1)
            {
                if (Change_Emission_Label == true)
                {
                    listBox5.Items.Clear();
                    foreach (string text in Pollmod)
                    {
                        listBox5.Items.Add(text);
                        listBox5.SelectedIndex = 0;
                    }
                }
                
                if (Pollmod.Count > 0)
                {
                    button18.Visible = true;
                    button21.Visible = true;
                    button48.Visible = true;
                    if (Change_Emission_Label == true)
                    {
                        Change_Label(2, 0); // Emission label red
                    }
                }
                else
                {
                    button18.Visible = false;
                    button21.Visible = false;
                    button48.Visible = false;
                }
            } 
        }

        /// <summary>
        /// compute emissions for the selected source groups and pollutant within the model domain
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowTotalEmissions(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            double[] totalemissions = new double[100];           //total emissions for the selected pollutant and source groups within the model domain

            ComputeTotalEmissions(totalemissions);

            GralMainForms.TotalEmissions totemi = new GralMainForms.TotalEmissions(totalemissions, this, Pollmod[listBox5.SelectedIndex]);
            totemi.Show();

            Cursor = Cursors.Default;
        }

	}
}
