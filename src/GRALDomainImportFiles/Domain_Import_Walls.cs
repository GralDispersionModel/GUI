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
using System.Drawing;
using System.Windows.Forms;
using GralShape;
using GralItemData;
using GralMessage;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Import walls from *.txt or *.shp file
        /// </summary>
        private void ImportWalls(object sender, EventArgs e)
        {
            OpenFileDialog dialog = openFileDialog1;
            dialog.Filter = "(Walls.txt;*.shp)|Walls.txt;*.shp";
            dialog.Title = "Select existing wall data";
            dialog.FileName = "";
            dialog.ShowHelp = true;
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogSources;
#endif


            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int numwalls = EditWall.ItemData.Count;
                
                if (dialog.FileName.EndsWith("Walls.txt"))
                {
                    try
                    {
                        WallDataIO _wd = new WallDataIO();
                        _wd.LoadWallData(EditWall.ItemData, dialog.FileName, true, new RectangleF((float) MainForm.GralDomRect.West, (float) MainForm.GralDomRect.South, (float) (MainForm.GralDomRect.East - MainForm.GralDomRect.West), (float) (MainForm.GralDomRect.North - MainForm.GralDomRect.South)));
                        _wd = null;
                        
                        EditWall.SetTrackBarMaximum();
                        EditAndSaveWallData(sender, e);
                        EditWall.FillValues();
                        
                        if (EditWall.ItemData.Count > 0)
                        {
                            button49.Visible = true;
                        }
                        
                        CheckForExistingDrawingObject("WALLS"); 

                        MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditWall.ItemData.Count - numwalls) + " buildings imported.", Location);
                        Box.Show();
                        if (EditWall.ItemData.Count - numwalls > 0)
                        {
                            MainForm.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonBuildings, Gral.ButtonColorEnum.RedDot); // Building label red & delete buildings.dat
                        }
                    }
                    catch
                    {
                       
                    }
                }
                else
                {
                    Waitprogressbar wait = new Waitprogressbar("Loading shape file");
                    try
                    {
                        //add Wall layer if not already existing
                        DrawingObjects _drobj = ItemOptions[CheckForExistingDrawingObject("WALLS")];
                        
                        //read geometry data from shape file
                        Cursor = Cursors.WaitCursor;
                        wait.Show();

                        SHPClear(_drobj);
                        //						ShapeReader shape = new ShapeReader(this);
                        //						shape.readShapeFile(dialog.FileName, index);
                        wait.Hide();

                        //open dialog for assigning building height
                        wait.Text = "Loading attribute table";
                        wait.Show();
                        using (ShapeBuildingDialog shp = new ShapeBuildingDialog(this, MainForm.GralDomRect, dialog.FileName))
                        {
                            shp.Wall = true; // Wall Mode
                            Cursor = Cursors.Default;
                            wait.Hide();
                            DialogResult dial = new DialogResult();
                            shp.StartPosition = FormStartPosition.Manual;
                            shp.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
                            shp.Top = 80;
                            dial = shp.ShowDialog();
                            
                            EditWall.SetTrackBarMaximum();
                            EditAndSaveWallData(sender, e);
                            EditWall.FillValues();
                            
                            MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditWall.ItemData.Count - numwalls) + " walls imported.", Location);
                            Box.Show();
                            if (EditWall.ItemData.Count - numwalls > 0)
                            {
                                MainForm.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonBuildings, Gral.ButtonColorEnum.RedDot); // Building label red & delete buildings.dat
                            }
                            
                            //MessageBox.Show("Data import successful: \r\n" + Convert.ToString(editb.buildingsdata.Count - numbuildings) + " buildings imported.");
                        }
                        SHPClear(_drobj);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(this, ex.Message + " Error when reading .shp file");
                    }
                    wait.Close();
                }
            }
            dialog.Dispose();
        }
    }
}