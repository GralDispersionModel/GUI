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
using GralShape;
using GralItemData;
using GralMessage;

namespace GralDomain
{
    public partial class Domain
	{
    	/// <summary>
    	/// Import vegetation from *.dat or *.shp file
    	/// </summary>
		private void ImportVegetationAreas(object sender, EventArgs e)
		{
			OpenFileDialog dialog = openFileDialog1;
			dialog.Filter = "(Vegetation.dat;*.shp)|Vegetation.dat;*.shp";
			dialog.Title = "Select existing vegetation areas";
			dialog.FileName = "";
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				int numvegetation = EditVegetation.ItemData.Count;
				
				if (dialog.FileName.EndsWith("shp"))
				{
					Waitprogressbar wait = new Waitprogressbar("Loading shape file");
					Waitprogressbar wait1 = new Waitprogressbar("Loading attribute table");
					try
					{
						//add Vegetation layer if not already existing
						CheckForExistingDrawingObject("VEGETATION");
						
						//read geometry data from shape file
						Cursor = Cursors.WaitCursor;
						wait1.Show();
                        //						ShapeReader shape = new ShapeReader(this);
                        //						shape.readShapeFile(dialog.FileName, index);
                        wait.Hide();

						//open dialog for assigning point source data
						wait1.Show();
                        Shape_Receptor_Dialog shp = new Shape_Receptor_Dialog(this, MainForm.GralDomRect, MainForm.GRALSettings.Deltaz, dialog.FileName)
                        {
                            Vegetation = true // load vegetation data
                        };

                        DialogResult dial = new DialogResult();
						wait1.Hide();
						Cursor = Cursors.Default;
                        shp.StartPosition = FormStartPosition.Manual;
                        shp.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
                        shp.Top = 80;
                        dial = shp.ShowDialog();
						{
						}
						
						EditVegetation.SetTrackBarMaximum();
						
						EditAndSaveVegetationData(sender, e);
						EditVegetation.FillValues();
						shp.Dispose();
						MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditVegetation.ItemData.Count - numvegetation) + " vegetation areas imported.", Location);
						Box.Show();
					}
					catch
					{
						MessageBox.Show(this, "Error when reading shape file","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
						Cursor = Cursors.Default;
					}
                    wait.Close();
                    wait1.Close();
                }
			}
			dialog.Dispose();
		}
	}
}