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
        /// Start the area source dialog (checkbox5 = checked) or save the area source data (checkbox5 = unchecked)
        /// </summary>
        /// <param name="sender">if checkbox5.checked == false and sender == null -> EditAS.SaveArray not called</param>
        private void EditAndSaveAreaSourceData(object sender, EventArgs e)
        {
            areaSourcesToolStripMenuItem.Checked = checkBox5.Checked;
            
            if (checkBox5.Checked == true)
            {
                //show editing form for area sources
                HideWindows(5); // Kuntner
                if (ShowFirst.As) // set the inital position of the form
                {
                    if (ShowFirst.Ls == false) EditAS.Location = EditLS.Location;
                    else if (ShowFirst.Ps == false) EditAS.Location = EditPS.Location;
                    else if (ShowFirst.Bu == false) EditAS.Location = EditB.Location;
                    else if (ShowFirst.Wa == false) EditAS.Location = EditWall.Location;
                    else
                    {
                        EditAS.Location = GetScreenPositionForNewDialog();
                    }
                    ShowFirst.As = false;
                }
                MouseControl = 8;
                InfoBoxCloseAllForms(); // close all infoboxes
                EditAS.Show();
                EditAS.TopMost = true; // Kuntner
                EditAS.ShowForm();
                Cursor = Cursors.Cross;
                
                CheckForExistingDrawingObject("AREA SOURCES");
            }
            else
            {
                EditAS.Hide(); // Kuntner first hide form to save actual sourcedata
                if (Gral.Main.Project_Locked == true)
                {
                    //Gral.Main.Project_Locked_Message(); // Project locked!
                    MouseControl = 0;
                    Picturebox1_Paint();
                }
                else
                {
                    //save area sources input to file
                    if (sender != null) // do not use the dialogue data, if data has been changed outisde the EditPortals dialogue
                    {
                        EditAS.SaveArray();
                    }

                    AreaSourceDataIO _as = new AreaSourceDataIO();
                    _as.SaveAreaData(EditAS.ItemData, Gral.Main.ProjectName);
                    _as = null;
                    
                    MainForm.SelectAllUsedSourceGroups();
                    MainForm.listBox5.Items.Clear();
                    MainForm.Pollmod.Clear();
                    MainForm.SetEmissionFilesInvalid();
                    MainForm.button18.Visible = false;
                    MainForm.Change_Label(2, -1); // Emission button not visible
                    MainForm.button21.Visible = false;
                    
                    Cursor = Cursors.Default;
                    MouseControl = 0;
                    //this.Width = ScreenWidth; // RM Kuntner
                    //this.Height = ScreenHeight - 50;
                    for (int i = 0; i <= EditAS.CornerAreaCount; i++)
                    {
                        CornerAreaSource[EditAS.CornerAreaCount] = new Point();
                    }
                    EditAS.CornerAreaCount = 0;
                    
                    //add/delete area sources in object list
                    if (EditAS.ItemData.Count == 0)
                    {
                        RemoveItemFromItemOptions("AREA SOURCES");
                    }
                }

                //show/hide button to select area sources
                if (EditAS.ItemData.Count > 0)
                {
                    button10.Visible = true;
                    button47.Visible = true;
                    button37.Visible = true;
                }
                else
                {
                    button10.Visible = false;
                    button37.Visible = false;
                }
            }
            //enable/disable GRAL simulations
            MainForm.Enable_GRAL();
        }
        
        /// <summary>
        /// Check for the entry ObjecType in the ItemOptions List and add entry if not already exist
        /// </summary>
        /// <returns>The index in the ItemOptions List</returns>
        private int CheckForExistingDrawingObject(string ObjectType)
        {
            bool exist = false;
            int counter = 0;
            foreach (DrawingObjects _drobj in ItemOptions)
            {
                if (_drobj.Name.Equals(ObjectType))
                {
                    exist = true;
                    break;
                }
                counter++;
            }
            if (exist == false)
            {
                DrawingObjects _drobj = new DrawingObjects(ObjectType);
                
                ItemOptions.Insert(0, _drobj);
                SaveDomainSettings(1);
                counter = 0;
            }
            return counter; 
         }
        
        /// <summary>
        /// Remove the entry ObjecType in the ItemOptions List
        /// </summary>
        private void RemoveItemFromItemOptions(string ObjectType)
        {
            int k = 0;
            foreach (DrawingObjects _drobj in ItemOptions)
            {
                if (_drobj.Name.Equals(ObjectType))
                {
                    break;
                }
                k++;
            }
            
            RemoveItems(k);
            SaveDomainSettings(1);
        }
    }
}