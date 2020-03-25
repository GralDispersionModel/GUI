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
 * Date: 17.01.2019
 * Time: 16:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Windows.Forms;
using GralMessage;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
	{
        /// <summary>
        /// change the horizontal grid resolution of the GRAMM grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetHorizontalGridSize(object sender, EventArgs e)
        {
            //when the horizontal grid size of the GRAMM grid is changed the GRAMM domain has to be defined again
            if (EmifileReset == true)
            {
                GRAMMHorGridSize = Convert.ToDouble(numericUpDown18.Value);
                textBox12.Text = "";
                textBox13.Text = "";
                textBox14.Text = "";
                textBox15.Text = "";
                GrammDomRect.East = 0;
                GrammDomRect.North = 0;
                GrammDomRect.South = 0;
                GrammDomRect.West = 0;
                listBox2.Visible = false;
                label95.Visible = false;
                button19.Visible = false;
                listBox6.Visible = false;
                button20.Visible = false;
                button43.Visible = false;
                button22.Visible = false;
                button23.Visible = false;
                radioButton1.Checked = true;
                radioButton2.Checked = false;
                checkBox25.Visible = false;
                checkBox25.Checked = false;
                label57.Visible = false;
                groupBox12.Visible = false;
                Topofile = "None";
            }
            try
            {
                if (EmifileReset == true)
                {
                    if (File.Exists(Path.Combine(ProjectName, @"Computation", "GRAMM.geb")) ||
                        File.Exists(Path.Combine(ProjectName, @"Computation", "ggeom.asc")) ||
                        File.Exists(Path.Combine(ProjectName, @"Computation", "windfeld.txt")))
                    {
                        FileDeleteMessage fdm = new FileDeleteMessage();
                        DialogResult dia = new DialogResult();
                        if (File.Exists(Path.Combine(ProjectName, @"Computation", "GRAMM.geb")))
                        {
                            fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "GRAMM.geb");
                        }

                        if (File.Exists(Path.Combine(ProjectName, @"Computation", "ggeom.asc")))
                        {
                            fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "ggeom.asc");
                        }

                        if (File.Exists(Path.Combine(ProjectName, @"Computation", "ggeom.txt")))
                        {
                            fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "ggeom.txt");
                        }

                        if (File.Exists(Path.Combine(ProjectName, @"Computation", "windfeld.txt")))
                        {
                            fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "windfeld.txt");
                        }

                        dia = fdm.ShowDialog();
                        if (dia == DialogResult.OK)
                        {
                            string newPath = Path.Combine(ProjectName, @"Computation", "GRAMM.geb");
                            File.Delete(newPath);
                            newPath = Path.Combine(ProjectName, @"Computation", "ggeom.asc");
                            File.Delete(newPath);
                            newPath = Path.Combine(ProjectName, @"Computation", "ggeom.txt");
                            File.Delete(newPath);
                            newPath = Path.Combine(ProjectName, @"Computation", "windfeld.txt");
                            File.Delete(newPath);
                        }
                    }
                }
            }
            catch
            {
            }

            //enable/disable GRAMM simulations
            Enable_GRAMM();
        }

        /// <summary>
        /// change the number of vertical layers in the GRAMM grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetVerticalLayerNumber(object sender, EventArgs e)
        {
            if (EmifileReset == true)
            {
                if (textBox12.Text != "")
                {
                    WriteGrammGebFile();
                }
                //compute top height of GRAMM domain
                ComputeGrammModelHeights();


                //show file name in listbox
                listBox2.Items.Clear();
                //delete files

                if (File.Exists(Path.Combine(ProjectName, @"Computation", "ggeom.asc")) ||
                    File.Exists(Path.Combine(ProjectName, @"Computation", "windfeld.txt")))
                {
                    FileDeleteMessage fdm = new FileDeleteMessage();
                    DialogResult dia = new DialogResult();
                    if (File.Exists(Path.Combine(ProjectName, @"Computation", "windfeld.txt")))
                    {
                        fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "windfeld.txt");
                    }

                    if (File.Exists(Path.Combine(ProjectName, @"Computation", "ggeom.asc")))
                    {
                        fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "ggeom.asc");
                    }

                    dia = fdm.ShowDialog();
                    if (dia == DialogResult.OK)
                    {
                        File.Delete(Path.Combine(ProjectName, @"Computation", "ggeom.asc"));
                        File.Delete(Path.Combine(ProjectName, @"Computation", "windfeld.txt"));
                    }
                    fdm.Dispose();
                }
            }

            //enable/disable GRAMM simulations
            Enable_GRAMM();
        }

        /// <summary>
        /// compute top level height in dependence of the GRAMM streching factor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMComputeTopLevelHeight(object sender, EventArgs e)
        {
            if (EmifileReset == true)
            {
                ComputeGrammModelHeights();

                //show file name in listbox
                listBox2.Items.Clear();
                //delete files
                if (File.Exists(Path.Combine(ProjectName, @"Computation", "ggeom.asc")) ||
                    File.Exists(Path.Combine(ProjectName, @"Computation", "windfeld.txt")))
                {
                    FileDeleteMessage fdm = new FileDeleteMessage();
                    DialogResult dia = new DialogResult();
                    if (File.Exists(Path.Combine(ProjectName, @"Computation", "windfeld.txt")))
                    {
                        fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "windfeld.txt");
                    }

                    if (File.Exists(Path.Combine(ProjectName, @"Computation", "ggeom.asc")))
                    {
                        fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "ggeom.asc");
                    }

                    dia = fdm.ShowDialog();
                    if (dia == DialogResult.OK)
                    {
                        File.Delete(Path.Combine(ProjectName, @"Computation", "ggeom.asc"));
                        File.Delete(Path.Combine(ProjectName, @"Computation", "windfeld.txt"));
                    }
                }

                //enable/disable GRAMM simulations
                Enable_GRAMM();
            }
        }

        /// <summary>
        /// set the maximum time step for the GRAMM simulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetMaximumTimeStep(object sender, EventArgs e)
        {
            SaveIINDatFile();
        }

        /// <summary>
        /// set the modelling time for the GRAMM simulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetModellingTime(object sender, EventArgs e)
        {
            SaveIINDatFile();
        }

        /// <summary>
        /// set the relaxation factor for velocity for the GRAMM simulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetRelaxVelFactor(object sender, EventArgs e)
        {
            SaveIINDatFile();
        }

        /// <summary>
        /// set the relaxation factor for scalars for the GRAMM simulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetRelaxScalFactor(object sender, EventArgs e)
        {
            SaveIINDatFile();
        }

        /// <summary>
        /// save file "reinit.dat" when reinitialization is switched on or off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetReInitOption(object sender, EventArgs e)
        {
            SaveIINDatFile();
        }

        /// <summary>
        /// save file "IIN.dat" when catabatic forcing is switched on/off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetCatabaticForcing(object sender, EventArgs e)
        {
            SaveIINDatFile();
        }

        /// <summary>
        /// save file "IIN.dat" when Debuging level is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetDebugingLevel(object sender, EventArgs e)
        {
            SaveIINDatFile();
        }

        /// <summary>
        /// change the starting number of the flow situation and the steady state option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetStartingSituation(object sender, EventArgs e)
        {
            GRAMMin(EmifileReset);
        }
        
        private bool SunriseResetMeteoFiles()
        {
        	if (File.Exists(Path.Combine(ProjectName, @"Computation", "meteopgt.all")))
        	{
        		if (MessageBox.Show("Reset the meteo files?", "GRAL GUI", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) ==
        		    DialogResult.OK)
        		{
        			GRAMMin(EmifileReset); //save GRAMM input file
        			Set_met_button(true); // meteopgt.all has to be written new -> user has to press "meteo files" button
        			try
        			{
        				File.Delete(Path.Combine(ProjectName, @"Computation", "meteopgt.all"));
        			}
        			catch{}
        			
        			return true;
        		}
        		else
        		{
        			return false;
        		}	
        	}
        	return true;
        }
        
        /// <summary>
        /// set GRAMM sunrise option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GRAMMSetSunriseOption(object sender, EventArgs e)
        {
        	if (SunriseResetMeteoFiles() == false) // Check, if meteopgt can be changed, otherwise set sunrise option to previous value 
        	{
        		checkBox30.Checked =! checkBox30.Checked;
        	}
            // create new GRAMMin.dat
            GRAMMin(true);
        }
	}
}