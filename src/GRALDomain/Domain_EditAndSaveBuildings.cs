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
using System.Drawing;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Show the building dialog (checkbox15 = checked) 
        /// </summary>
        /// <param name="sender"></param>
        private void ShowBuildingDialog(object sender, EventArgs e)
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
                        EditB.Location = GetScreenPositionForNewDialog(0);
                    }

                    ShowFirst.Bu = false;
                }
                MouseControl = MouseMode.BuildingPos;
                InfoBoxCloseAllForms(); // close all infoboxes
                EditB.Show();
                EditB.ShowForm();
                EditB.BringToFront();
                Cursor = Cursors.Cross;

                CheckForExistingDrawingObject("BUILDINGS");
            }
            else
            {
                EditB.Hide();
                MouseControl = MouseMode.Default;
                Picturebox1_Paint();
            }
        }
        /// <summary>
        /// Save the building data 
        /// </summary>
        /// <param name="sender"></param>
        private void EditAndSaveBuildingsData(object sender, EventArgs e)
        {
            checkBox15.Checked = false;
            buildingsToolStripMenuItem.Checked = checkBox15.Checked;
            MouseControl = MouseMode.Default;
            if (Gral.Main.Project_Locked == true)
            {
                //Gral.Main.Project_Locked_Message(); // Project locked!
                //Picturebox1_Paint();
            }
            else if (MainForm.DeleteGralGffFile() == DialogResult.OK) // Warningmessage if gff Files exist!
            {
                //save buildings input to file
                if (sender != null) // do not use the dialogue data, if data has been changed outisde the EditPortals dialogue
                {
                    EditB.SaveArray(false);
                }
                BuildingDataIO _bui = new BuildingDataIO();
                _bui.SaveBuildings(EditB.ItemData, Gral.Main.ProjectName, Gral.Main.GUISettings.CompatibilityToVersion1901);
                _bui = null;

                Cursor = Cursors.Default;
                for (int i = 0; i <= EditB.CornerBuilding; i++)
                {
                    CornerAreaSource[EditB.CornerBuilding] = new Point();
                }
                EditB.CornerBuilding = 0;
                
                MainForm.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonBuildings, Gral.ButtonColorEnum.RedDot); // Building label red & delete buildings.dat

                if (MainForm.GRALSettings.BuildingMode != Gral.BuildingModeEnum.None)
                {
                    if (EditB.ItemData.Count > 0)
                    {
                        MainForm.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonBuildings, Gral.ButtonColorEnum.RedDot); // Building label red & delete buildings.dat
                        MainForm.button9.Visible = true;
                    }
                    else
                    {
                        MainForm.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonBuildings, Gral.ButtonColorEnum.Invisible); // Building label - no buildings
                    }
                }
                //add/delete buildings in object list
                if (EditB.ItemData.Count == 0)
                {
                    RemoveItemFromItemOptions("BUILDINGS");
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
            EditB.Hide();
            //enable/disable GRAL simulations
            MainForm.Enable_GRAL();
            //enable/disable GRAMM simulations
            MainForm.Enable_GRAMM();
            Picturebox1_Paint();
        }

    }
}