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

using GralItemData;
using GralMessage;
using GralShape;
using System;
using System.Windows.Forms;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Import receptor points from *.dat or *.shp file
        /// </summary>
        private void ImportReceptorPoints(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = openFileDialog1)
            {
                dialog.Filter = "(Receptor.dat;*.shp)|Receptor.dat;*.shp";
                dialog.Title = "Select existing receptors";
                dialog.FileName = "";
                // dialog.ShowHelp = true;
#if NET6_0_OR_GREATER
                dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogSources;
#endif


                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int numbrec = EditR.ItemData.Count;

                    if (numbrec == 1 && EditR.ItemData[0].Name.Length == 0)
                    {
                        EditR.ItemData.Clear();
                    }

                    if (!dialog.FileName.EndsWith("shp"))
                    {
                        int i = 1;
                        try
                        {
                            string file = dialog.FileName;
                            ReceptorDataIO _rd = new ReceptorDataIO();
                            _rd.LoadReceptors(EditR.ItemData, file);
                            _rd = null;

                            EditR.SetTrackBarMaximum();

                            EditAndSaveReceptorData(sender, e);
                            EditR.FillValues();

                            //add RECEPTORS layer if not already existing
                            CheckForExistingDrawingObject("RECEPTORS");

                            MessageBox.Show(this, "Data import successful: \r\n" + Convert.ToString(EditR.ItemData.Count - numbrec) + " receptor points imported.", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch
                        {
                            MessageBox.Show(this, "Error when reading point source data in line number " + Convert.ToString(i + 2), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        Waitprogressbar wait = new Waitprogressbar("Loading shape file");
                        Waitprogressbar wait1 = new Waitprogressbar("Loading attribute table");
                        try
                        {
                            //add RECEPTORS layer if not already existing
                            CheckForExistingDrawingObject("RECEPTORS");

                            //read geometry data from shape file
                            Cursor = Cursors.WaitCursor;
                            wait1.Show();
                            //						ShapeReader shape = new ShapeReader(this);
                            //						shape.readShapeFile(dialog.FileName, index);
                            wait.Hide();

                            //open dialog for assigning point source data
                            wait1.Show();
                            using (Shape_Receptor_Dialog shp = new Shape_Receptor_Dialog(this, MainForm.GralDomRect, MainForm.GRALSettings.Deltaz, dialog.FileName))
                            {
                                DialogResult dial = new DialogResult();
                                wait1.Hide();
                                Cursor = Cursors.Default;
                                shp.StartPosition = FormStartPosition.Manual;
                                shp.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
                                shp.Top = GralStaticFunctions.St_F.GetTopScreenAtMousePosition() + 150;
                                dial = shp.ShowDialog();

                                EditR.SetTrackBarMaximum();
                                EditAndSaveReceptorData(sender, e);
                                EditR.FillValues();
                            }

                            MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditR.ItemData.Count - numbrec) + " receptor points imported.", Location);
                            Box.Show();
                            //MessageBox.Show("Data import successful: \r\n" + Convert.ToString(editr.receptors.Count - numbrec) + " receptor points imported.");
                        }
                        catch
                        {
                            MessageBox.Show(this, "Error when reading shape file", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Cursor = Cursors.Default;
                        }
                        wait.Close();
                        wait1.Close();
                    }
                }
            }
        }
    }
}