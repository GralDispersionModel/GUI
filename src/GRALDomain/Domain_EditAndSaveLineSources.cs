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
        /// Show the point source dialog (checkbox8 = checked) 
        /// </summary>
        /// <param name="sender"></param>
        private void ShowLineSourceDialog(object sender, EventArgs e)
        {
            lineSourcesToolStripMenuItem.Checked = checkBox8.Checked;

            if (checkBox8.Checked == true)
            {
                //show editing form for line sources
                HideWindows(8); // Kuntner

                if (ShowFirst.Ls) // set the inital position & height of the form
                {
                    EditLS.Height = 750;
                    if (ShowFirst.Ps == false)
                    {
                        EditLS.Location = EditPS.Location;
                    }
                    else if (ShowFirst.As == false)
                    {
                        EditLS.Location = EditAS.Location;
                    }
                    else if (ShowFirst.Bu == false)
                    {
                        EditLS.Location = EditB.Location;
                    }
                    else if (ShowFirst.Wa == false)
                    {
                        EditLS.Location = EditWall.Location;
                    }
                    else
                    {
                        EditLS.Location = GetScreenPositionForNewDialog(0);
                    }

                    ShowFirst.Ls = false;
                }
                MouseControl = MouseMode.LineSourcePos;
                InfoBoxCloseAllForms(); // close all infoboxes
                EditLS.Show();
                EditLS.ShowForm();
                EditLS.BringToFront();
                Cursor = Cursors.Cross;

                CheckForExistingDrawingObject("LINE SOURCES");
            }
            else
            {
                MouseControl = MouseMode.Default;
                EditLS.Hide();
            }
        }
        /// <summary>
        /// Start the line source dialog (checkbox8 = checked) or save the line source data (checkbox8 = unchecked)
        /// </summary>
        /// <param name="sender">if checkbox8.checked == false and sender == null -> EditLS.SaveArray not called</param>
        private void EditAndSaveLineSourceData(object sender, EventArgs e)
        {
            checkBox8.Checked = false;
            lineSourcesToolStripMenuItem.Checked = checkBox8.Checked;
            MouseControl = MouseMode.Default;
            Cursor = Cursors.Default;

            if (Gral.Main.Project_Locked == true)
            {
                //Gral.Main.Project_Locked_Message(); // Project locked!
                //Picturebox1_Paint();
            }
            else
            {

                //save line sources input to file
                if (sender != null) // do not use the dialogue data, if data has been changed outisde the EditPortals dialogue
                {
                    EditLS.SaveArray(false);
                }

                LineSourceDataIO _ls = new LineSourceDataIO();
                _ls.SaveLineSources(EditLS.ItemData, Gral.Main.ProjectName);
                _ls = null;

                MainForm.SelectAllUsedSourceGroups();
                MainForm.listBox5.Items.Clear();
                MainForm.Pollmod.Clear();
                MainForm.SetEmissionFilesInvalid();
                MainForm.button18.Visible = false;
                MainForm.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonEmission, Gral.ButtonColorEnum.Invisible); // Emission button not visible
                MainForm.button21.Visible = false;

                for (int i = 0; i <= EditLS.CornerLineSource; i++)
                {
                    CornerAreaSource[EditLS.CornerLineSource] = new Point();
                }
                EditLS.CornerLineSource = 0;

                //add/delete line sources in object list
                if (EditLS.ItemData.Count == 0)
                {
                    RemoveItemFromItemOptions("LINE SOURCES");
                }
            }

            //show/hide buttons to select line sources and to use NEMO
            if (EditLS.ItemData.Count > 0)
            {
                button12.Visible = true;
                button47.Visible = true;
                button36.Visible = true;
                MainForm.button13.Visible = true;
            }
            else
            {
                button12.Visible = false;
                button36.Visible = false;
                MainForm.button13.Visible = false;
            }
            EditLS.Hide();
            //enable/disable GRAL simulations
            MainForm.Enable_GRAL();
            Picturebox1_Paint();
        }
    }
}