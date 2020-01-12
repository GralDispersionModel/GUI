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
        /// Start the line source dialog (checkbox8 = checked) or save the line source data (checkbox8 = unchecked)
        /// </summary>
        /// <param name="sender">if checkbox8.checked == false and sender == null -> EditLS.SaveArray not called</param>
        private void EditAndSaveLineSourceData(object sender, EventArgs e)
        {
            lineSourcesToolStripMenuItem.Checked = checkBox8.Checked;

            if (checkBox8.Checked == true)
            {
                //show editing form for line sources
                HideWindows(8); // Kuntner
                
                if (ShowFirst.Ls) // set the inital position & height of the form
                {
                    EditLS.Height = 750;
                    if (ShowFirst.Ps == false) EditLS.Location = EditPS.Location;
                    else if (ShowFirst.As == false) EditLS.Location = EditAS.Location;
                    else if (ShowFirst.Bu == false) EditLS.Location = EditB.Location;
                    else if (ShowFirst.Wa == false) EditLS.Location = EditWall.Location;
                    else
                        EditLS.Location = GetScreenPositionForNewDialog();
                    ShowFirst.Ls = false;
                }
                MouseControl = 10;
                InfoBoxCloseAllForms(); // close all infoboxes
                EditLS.Show();
                EditLS.TopMost = true; // Kuntner
                EditLS.ShowForm();
                Cursor = Cursors.Cross;
                
                CheckForExistingDrawingObject("LINE SOURCES");
            }
            else
            {
                EditLS.Hide(); // Kuntner first hide form to save actual sourcedata

                if (Gral.Main.Project_Locked == true)
                {
                    //Gral.Main.Project_Locked_Message(); // Project locked!
                    MouseControl = 0;
                    Picturebox1_Paint();
                }
                else
                {

                    //save line sources input to file
                    if (sender != null) // do not use the dialogue data, if data has been changed outisde the EditPortals dialogue
                    {
                        EditLS.SaveArray();
                    }

                    LineSourceDataIO _ls = new LineSourceDataIO();
                    _ls.SaveLineSources(EditLS.ItemData, Gral.Main.ProjectName);
                    _ls = null;

                    MainForm.SelectAllUsedSourceGroups();
                    MainForm.listBox5.Items.Clear();
                    MainForm.Pollmod.Clear();
                    MainForm.SetEmissionFilesInvalid();
                    MainForm.button18.Visible = false;
                    MainForm.Change_Label(2, -1); // Emission button not visible
                    MainForm.button21.Visible = false;
                    
                    Cursor = Cursors.Default;
                    MouseControl = 0;
                    //this.Width = ScreenWidth;
                    //this.Height = ScreenHeight - 50;
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
            }
            
            //enable/disable GRAL simulations
            MainForm.Enable_GRAL();
        }
    }
}