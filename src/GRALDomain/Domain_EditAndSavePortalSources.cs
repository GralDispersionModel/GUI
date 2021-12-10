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
using GralItemData;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Show the portal source dialog (checkbox12 = checked) 
        /// </summary>
        /// <param name="sender"></param>
        private void ShowPortalSourceDialog(object sender, EventArgs e)
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
                        EditPortals.Location = GetScreenPositionForNewDialog(0);
                    }

                    ShowFirst.Ts = false;
                }
                MouseControl = MouseMode.PortalSourcePos;
                InfoBoxCloseAllForms(); // close all infoboxes
                EditPortals.Show();
                EditPortals.ShowForm();
                EditPortals.BringToFront();
                Cursor = Cursors.Cross;

                CheckForExistingDrawingObject("TUNNEL PORTALS");
            }
            else
            {
                MouseControl = MouseMode.Default;
                EditPortals.Hide();
            }
        }
        /// <summary>
        /// Save the portal source data (checkbox12 = unchecked)
        /// </summary>
        /// <param name="sender">if checkbox12.checked == false and sender == null -> EditPortals.SaveArray not called</param>
        private void EditAndSavePortalSourceData(object sender, EventArgs e)
        {
            checkBox12.Checked = false;
            tunnelPortalsToolStripMenuItem.Checked = checkBox12.Checked;
            MouseControl = MouseMode.Default;
            Cursor = Cursors.Default;
            EditPortals.Hide(); // Kuntner first hide form to save actual sourcedata
            if (Gral.Main.Project_Locked == true)
            {
                //Gral.Main.Project_Locked_Message(); // Project locked!
                //Picturebox1_Paint();
            }
            else
            {
                //save tunnel portal input to file
                if (sender != null) // do not use the dialogue data, if data has been changed outisde the EditPortals dialogue
                {
                    EditPortals.SaveArray(false);
                }
                PortalsDataIO _ps = new PortalsDataIO();
                _ps.SavePortalSources(EditPortals.ItemData, Gral.Main.ProjectName);
                _ps = null;

                MainForm.SelectAllUsedSourceGroups();
                MainForm.listBox5.Items.Clear();
                MainForm.Pollmod.Clear();
                MainForm.SetEmissionFilesInvalid();
                MainForm.button18.Visible = false;
                MainForm.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonEmission, Gral.ButtonColorEnum.Invisible); // Emission button not visible
                MainForm.button21.Visible = false; //Show Emissions
                MainForm.button13.Visible = false; //Nemo

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
            Picturebox1_Paint();
            //enable/disable GRAL simulations
            MainForm.Enable_GRAL();
        }
    }
}