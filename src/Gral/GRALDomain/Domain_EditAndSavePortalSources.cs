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

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Start the portalsource dialog (checkbox12 = checked) or save the portal source data (checkbox12 = unchecked)
        /// </summary>
        /// <param name="sender">if checkbox12.checked == false and sender == null -> EditPortals.SaveArray not called</param>
        private void EditAndSavePortalSourceData(object sender, EventArgs e)
        {
            tunnelPortalsToolStripMenuItem.Checked = checkBox12.Checked;
            
            if (checkBox12.Checked == true)
            {
                //show editing form for tunnel portal
                HideWindows(12); // Kuntner
                
                if (ShowFirst.Ts) // set the inital position of the form
                {
                    EditPortals.Height = 750;
                    if (ShowFirst.Ls == false)
                    {
                        EditPortals.Location = EditLS.Location;
                    }
                    else if (ShowFirst.As == false)
                    {
                        EditPortals.Location = EditAS.Location;
                    }
                    else if (ShowFirst.Bu == false)
                    {
                        EditPortals.Location = EditB.Location;
                    }
                    else if (ShowFirst.Ps == false)
                    {
                        EditPortals.Location = EditPS.Location;
                    }
                    else if (ShowFirst.Wa == false)
                    {
                        EditPortals.Location = EditWall.Location;
                    }
                    else
                    {
                        EditPortals.Location = GetScreenPositionForNewDialog();
                    }

                    ShowFirst.Ts = false;
                }
                MouseControl = 15;
                InfoBoxCloseAllForms(); // close all infoboxes
                EditPortals.Show();
                EditPortals.TopMost = true; // Kuntner
                EditPortals.ShowForm();
                Cursor = Cursors.Cross;
                
                CheckForExistingDrawingObject("TUNNEL PORTALS");   
            }
            else
            {
                EditPortals.Hide(); // Kuntner first hide form to save actual sourcedata
                if (Gral.Main.Project_Locked == true)
                {
                    //Gral.Main.Project_Locked_Message(); // Project locked!
                    MouseControl = 0;
                    Picturebox1_Paint();
                }
                else
                {
                    //save tunnel portal input to file
                    if (sender != null) // do not use the dialogue data, if data has been changed outisde the EditPortals dialogue
                    {
                        EditPortals.SaveArray();
                    }
                    PortalsDataIO _ps = new PortalsDataIO();
                    _ps.SavePortalSources(EditPortals.ItemData, Gral.Main.ProjectName);
                    _ps = null;
                    
                    MainForm.SelectAllUsedSourceGroups();
                    MainForm.listBox5.Items.Clear();
                    MainForm.Pollmod.Clear();
                    MainForm.SetEmissionFilesInvalid();                   
                    MainForm.button18.Visible = false;
                    MainForm.Change_Label(2, -1); // Emission button not visible
                    MainForm.button21.Visible = false; //Show Emissions
                    MainForm.button13.Visible = false; //Nemo
                    
                    Cursor = Cursors.Default;
                    MouseControl = 0;
                    //this.Width = ScreenWidth;
                    //this.Height = ScreenHeight - 50;

                    //add/delete tunnel portal sources in object list
                    if (EditPortals.ItemData.Count == 0)
                    {
                        RemoveItemFromItemOptions("TUNNEL PORTALS");
                    }
                }

                //show/hide button to select tunnel sources
                if (EditPortals.ItemData.Count > 0)
                {
                    button14.Visible = true;
                    button47.Visible = true;
                }
                else
                {
                    button14.Visible = false;
                }
            }

            
            //enable/disable GRAL simulations
            MainForm.Enable_GRAL();
        }
    }
}