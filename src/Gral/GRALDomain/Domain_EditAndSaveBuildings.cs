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
 * Date: 24.01.2019
 * Time: 14:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using GralItemData;
using System.Drawing;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Start the building dialog (checkbox15 = checked) or save the building data (checkbox15 = unchecked)
        /// </summary>
        /// <param name="sender">if checkbox15.checked == false and sender == null -> EditB.SaveArray not called</param>
        private void EditAndSaveBuildingsData(object sender, EventArgs e)
        {
            buildingsToolStripMenuItem.Checked = checkBox15.Checked;
            
            if (checkBox15.Checked == true)
            {
                HideWindows(15); // Kuntner
                if (ShowFirst.Bu) // set the inital position of the form
                {
                    EditB.StartPosition = FormStartPosition.Manual;
                    if (ShowFirst.Ls == false)
                    {
                        EditB.Location = EditLS.Location;
                    }
                    else if (ShowFirst.As == false)
                    {
                        EditB.Location = EditAS.Location;
                    }
                    else if (ShowFirst.Ps == false)
                    {
                        EditB.Location = EditPS.Location;
                    }
                    else if (ShowFirst.Wa == false)
                    {
                        EditB.Location = EditWall.Location;
                    }
                    else
                    {
                        EditB.Location = GetScreenPositionForNewDialog();
                    }

                    ShowFirst.Bu = false;
                }
                MouseControl = 17;
                InfoBoxCloseAllForms(); // close all infoboxes
                EditB.Show();
                EditB.TopMost = true; // Kuntner
                EditB.ShowForm();
                Cursor = Cursors.Cross;
                
                CheckForExistingDrawingObject("BUILDINGS");   
            }
            else
            {
                EditB.Hide(); // Kuntner first hide form to save actual sourcedata
                if (Gral.Main.Project_Locked == true)
                {
                    //Gral.Main.Project_Locked_Message(); // Project locked!
                    MouseControl = 0;
                    Picturebox1_Paint();
                }
                else if (MainForm.DeleteGralGffFile() == DialogResult.OK) // Warningmessage if gff Files exist!
                {
                    //save buildings input to file
                    if (sender != null) // do not use the dialogue data, if data has been changed outisde the EditPortals dialogue
                    {
                        EditB.SaveArray();
                    }
                    BuildingDataIO _bui = new BuildingDataIO();
                    _bui.SaveBuildings(EditB.ItemData, Gral.Main.ProjectName, Gral.Main.CompatibilityToVersion1901);
                    _bui = null;
                    
                    Cursor = Cursors.Default;
                    MouseControl = 0;
                    //this.Width = ScreenWidth;
                    //this.Height = ScreenHeight - 50;
                    for (int i = 0; i <= EditB.CornerBuilding; i++)
                    {
                        CornerAreaSource[EditB.CornerBuilding] = new Point();
                    }
                    EditB.CornerBuilding = 0;
                    Picturebox1_Paint();
                    MainForm.Change_Label(3, 0); // Building label red & delete buildings.dat
                    
                    if (MainForm.GRALSettings.BuildingMode != 0)
                    {
                        if (EditB.ItemData.Count > 0)
                        {
                            MainForm.Change_Label(3, 0); // Building label red & delete buildings.dat
                            MainForm.button9.Visible = true;
                        }
                        else
                        {
                            MainForm.Change_Label(3, -1); // Building label - no buildings
                        }
                    }
                    //add/delete buildings in object list
                    if (EditB.ItemData.Count == 0)
                    {
                        RemoveItemFromItemOptions("BUILDINGS");
                    }
                    
                }
            }
            //show/hide button to select buildings
            if (EditB.ItemData.Count > 0)
            {
                button16.Visible = true;
            }
            else
            {
                button16.Visible = false;
            }

            //enable/disable GRAL simulations
            MainForm.Enable_GRAL();
            //enable/disable GRAMM simulations
            MainForm.Enable_GRAMM();
        }
        
    }
}