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
using System.IO;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Show the receptor dialog (checkbox20 = checked) 
        /// </summary>
        /// <param name="sender"></param>
        private void ShowReceptorDialog(object sender, EventArgs e)
        {
            receptorPointsToolStripMenuItem.Checked = checkBox20.Checked;

            if (checkBox20.Checked == true)
            {
                //show editing form for receptors
                HideWindows(20); // Kuntner
                if (ShowFirst.Re) // set the inital position of the form
                {
                    if (ShowFirst.Ls == false)
                    {
                        EditR.Location = EditLS.Location;
                    }
                    else if (ShowFirst.As == false)
                    {
                        EditR.Location = EditAS.Location;
                    }
                    else if (ShowFirst.Bu == false)
                    {
                        EditR.Location = EditB.Location;
                    }
                    else if (ShowFirst.Ps == false)
                    {
                        EditR.Location = EditPS.Location;
                    }
                    else if (ShowFirst.Wa == false)
                    {
                        EditR.Location = EditWall.Location;
                    }
                    else
                    {
                        EditR.Location = GetScreenPositionForNewDialog();
                    }

                    ShowFirst.Re = false;
                }
                MouseControl = 24;
                InfoBoxCloseAllForms(); // close all infoboxes
                EditR.Show();
                EditR.ShowForm();
                EditR.BringToFront();
                Cursor = Cursors.Cross;

                CheckForExistingDrawingObject("RECEPTORS");
            }
            else
            {
                MouseControl = 0;
                EditR.Hide();
            }
        }

        /// <summary>
        /// Save the receptor data (checkbox20 = unchecked)
        /// </summary>
        /// <param name="sender"></param>
        private void EditAndSaveReceptorData(object sender, EventArgs e)
        {
            checkBox20.Checked = false;
            MouseControl = 0;
            receptorPointsToolStripMenuItem.Checked = checkBox20.Checked;

            if (Gral.Main.Project_Locked == true)
            {
                //Gral.Main.Project_Locked_Message(); // Project locked!
                //Picturebox1_Paint();
            }
            else
            {
                //save receptor input to file
                if (sender != null) // do not use the dialogue data, if data has been changed outisde the EditPortals dialogue
                {
                    EditR.SaveArray();
                }

                ReceptorDataIO _rd = new ReceptorDataIO();
                _rd.SaveReceptor(EditR.ItemData, Gral.Main.ProjectName);
                _rd = null;

                //copy recptor.dat file to receptor_GRAMM.dat
                string newPath = Path.Combine(Gral.Main.ProjectName, @"Computation", "Receptor.dat");
                File.Copy(newPath, newPath.Replace(".dat", "_GRAMM.dat"), true);

                MainForm.GRALSettings.Receptorflag = "1";
                WriteInDat(); // write new in.dat
                MainForm.Change_Label(0, 0); // red label at control button
            }
            Cursor = Cursors.Default;
            
            //add/delete receptors in object list
            if (EditR.ItemData.Count == 0 && Gral.Main.Project_Locked == false) // block this, if project is locked!
            {
                MainForm.GRALSettings.Receptorflag = "0";
                WriteInDat(); // write new in.dat

                RemoveItemFromItemOptions("RECEPTORS");
            }

            //show/hide button to select receptors
            if (EditR.ItemData.Count > 0)
            {
                button23.Visible = true;
                button47.Visible = true;
                button39.Visible = true;
            }
            else
            {
                button23.Visible = false;
                button39.Visible = false;
            }
            EditR.Hide();
            Picturebox1_Paint();
        }
    }
}