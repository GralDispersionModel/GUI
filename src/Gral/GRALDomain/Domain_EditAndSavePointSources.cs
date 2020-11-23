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
        /// Show the point source dialog (checkbox4 = checked) 
        /// </summary>
        /// <param name="sender"></param>
        private void ShowPointSourceDialog(object sender, EventArgs e)
        {
            pointSourcesToolStripMenuItem.Checked = checkBox4.Checked;
            if (checkBox4.Checked == true)
            {
                //load existing point source data if available             
                HideWindows(4); // Kuntner
                if (ShowFirst.Ps) // set the inital position of the form
                {
                    if (ShowFirst.Ls == false)
                    {
                        EditPS.Location = EditLS.Location;
                    }
                    else if (ShowFirst.As == false)
                    {
                        EditPS.Location = EditAS.Location;
                    }
                    else if (ShowFirst.Bu == false)
                    {
                        EditPS.Location = EditB.Location;
                    }
                    else if (ShowFirst.Wa == false)
                    {
                        EditPS.Location = EditWall.Location;
                    }
                    else
                    {
                        EditPS.Location = GetScreenPositionForNewDialog();
                    }

                    ShowFirst.Ps = false;
                }
                MouseControl = 6;
                InfoBoxCloseAllForms(); // close all infoboxes

                EditPS.Show();
                EditPS.ShowForm();
                EditPS.BringToFront();
                Cursor = Cursors.Cross;

                CheckForExistingDrawingObject("POINT SOURCES");
            }
            else
            {
                MouseControl = 0;
                EditPS.Hide();
            }
        }
        /// <summary>
        /// Save the point source data 
        /// </summary>
        /// <param name="sender">if checkbox4.checked == false and sender == null -> EditPS.SaveArray not called</param>
        private void EditAndSavePointSourceData(object sender, EventArgs e)
        {
            checkBox4.Checked = false;
            pointSourcesToolStripMenuItem.Checked = checkBox4.Checked;
            MouseControl = 0;
            if (Gral.Main.Project_Locked == true)
            {
                //Gral.Main.Project_Locked_Message(); // Project locked!
                //Picturebox1_Paint();
            }
            else
            {
                //save point sources input to file
                if (sender != null) // do not use the dialogue data, if data has been changed outisde the EditPortals dialogue
                {
                    EditPS.SaveArray();
                }

                PointSourceDataIO _ps = new PointSourceDataIO();
                _ps.SavePointSources(EditPS.ItemData, Gral.Main.ProjectName);
                _ps = null;

                //select source groups within the model domain
                MainForm.SelectAllUsedSourceGroups();
                MainForm.listBox5.Items.Clear();
                MainForm.Pollmod.Clear();
                MainForm.SetEmissionFilesInvalid();
                MainForm.button18.Visible = false;
                MainForm.Change_Label(2, -1); // Emission button not visible
                MainForm.button21.Visible = false;

                Cursor = Cursors.Default;
                //add/delete point sources in object list
                if (EditPS.ItemData.Count == 0)
                {
                    RemoveItemFromItemOptions("POINT SOURCES");
                }
            }

            //show/hide button to select point sources
            if (EditPS.ItemData.Count > 0)
            {
                button8.Visible = true;
                button47.Visible = true;
                button38.Visible = true;
            }
            else
            {
                button8.Visible = false;
                button38.Visible = false;
            }
            EditPS.Hide();
            //enable/disable GRAL simulations
            MainForm.Enable_GRAL();
            Picturebox1_Paint();
        }


        private void CancelItemForms(object sender, EventArgs e)
        {
            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox8.Checked = false;
            checkBox12.Checked = false;
            checkBox15.Checked = false;
            checkBox20.Checked = false;
            checkBox25.Checked = false;
            checkBox26.Checked = false;

            if (sender is Form)
            {
                Form _form = (Form)sender;
                _form.Hide();
            }

            MouseControl = 0;
            Picturebox1_Paint();
        }
    }
}