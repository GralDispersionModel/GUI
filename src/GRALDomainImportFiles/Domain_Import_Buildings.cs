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
using System.IO;
using GralStaticFunctions;
using GralShape;
using GralItemData;
using GralMessage;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Import building from *.txt, *.dat or *.shp file
        /// </summary>
        private void ImportBuildings(object sender, EventArgs e)
        {
            OpenFileDialog dialog = openFileDialog1;
            dialog.Filter = "(Buildings.txt;buildings.dat;*.shp)|Buildings.txt;buildings.dat;*.shp";
            dialog.Title = "Select existing buildings data";
            dialog.FileName = "";
            // dialog.ShowHelp = true;
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogSources;
#endif


            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int numbuildings = EditB.ItemData.Count;
                
                if (!dialog.FileName.EndsWith("shp") && dialog.FileName.EndsWith(".dat"))
                {
                    int i = 1;
                    try
                    {
                        string[] text1 = new string[15];
                        string file = dialog.FileName;
                        StreamReader myReader = new StreamReader(file);
                        string dummy = myReader.ReadLine();
                        Cursor = Cursors.WaitCursor;
                        double mindist = 10000;     //minimum distance between two building blocks
                        double x1;
                        double y1;
                        double x2;
                        double y2;
                        for (i = 1; i < 10000000; i++)
                        {
                            try
                            {
                                dummy = myReader.ReadLine();
                                text1 = dummy.Split(new char[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                x1 = Convert.ToDouble(text1[0].Replace(".", decsep));
                                y1 = Convert.ToDouble(text1[1].Replace(".", decsep));
                                dummy = myReader.ReadLine();
                                text1 = dummy.Split(new char[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                x2 = Convert.ToDouble(text1[0].Replace(".", decsep));
                                y2 = Convert.ToDouble(text1[1].Replace(".", decsep));
                                mindist = Math.Min(mindist, Math.Abs(Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2))));
                            }
                            catch
                            {
                                break;
                            }
                        }
                        myReader.Close();
                        
                        myReader = new StreamReader(file);
                        int trans = Convert.ToInt32(mindist);
                        if (InputBox1("Define raster for gridding buildings", "Suggested raster size:", 0, 10000, ref trans) == DialogResult.OK)
                        {
                            for (i = 1; i < 10000000; i++)
                        {
                            dummy = myReader.ReadLine();
                            if (dummy != null)
                            {
                                text1 = dummy.Split(new char[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                x1 = Convert.ToDouble(text1[0].Replace(".", decsep));
                                y1 = Convert.ToDouble(text1[1].Replace(".", decsep));
                                double raster = Convert.ToDouble(trans);
                                double areabuildings = raster * raster;
                                if ((x1 >= MainForm.GralDomRect.West) && (x1 <= MainForm.GralDomRect.East) &&
                                    (y1 >= MainForm.GralDomRect.South) && (y1 <= MainForm.GralDomRect.North))
                                {
                                        BuildingData _bd = new BuildingData
                                        {
                                            Name = "Buildings" + Convert.ToString(i),
                                            Height = (float)(St_F.TxtToDbl(text1[2], false)),
                                            LowerBound = (float)(St_F.TxtToDbl(text1[3], false)),
                                            Area = (float)(areabuildings)
                                        };
                                        _bd.Pt.Add(new PointD(x1 - raster * 0.5, y1 - raster * 0.5));
                                    _bd.Pt.Add(new PointD(x1 - raster * 0.5, y1 + raster * 0.5));
                                    _bd.Pt.Add(new PointD(x1 + raster * 0.5, y1 + raster * 0.5));
                                    _bd.Pt.Add(new PointD(x1 + raster * 0.5, y1 - raster * 0.5));
                                    EditB.ItemData.Add(_bd);
                                }
                            }
                            else
                                {
                                    break;
                                }
                            }
                        }

                        myReader.Close();
                        EditB.SetTrackBarMaximum();
                        EditAndSaveBuildingsData(sender, e);
                        EditB.FillValues();

                        //add BUILDINGS layer if not already existing
                        CheckForExistingDrawingObject("BUILDINGS");
                        
                        MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditB.ItemData.Count - numbuildings) + " buildings imported.", Location);
                        Box.Show();
                        if (EditB.ItemData.Count - numbuildings > 0)
                        {
                            MainForm.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonBuildings, Gral.ButtonColorEnum.RedDot); // Building label red & delete buildings.dat
                        }
                        
                        //MessageBox.Show("Data import successful: \r\n" + Convert.ToString(editb.buildingsdata.Count - numbuildings) + " buildings imported.");
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when reading buildings in line number " + Convert.ToString(i + 1));
                    }
                }
                else if (dialog.FileName.EndsWith("Buildings.txt"))
                {
                    try
                    {
                        BuildingDataIO _bd = new BuildingDataIO();
                        _bd.LoadBuildings(EditB.ItemData, dialog.FileName, true, new RectangleF((float) MainForm.GralDomRect.West, (float) MainForm.GralDomRect.South, (float) (MainForm.GralDomRect.East - MainForm.GralDomRect.West), (float) (MainForm.GralDomRect.North - MainForm.GralDomRect.South)));
                        _bd = null;
                        
                        EditB.SetTrackBarMaximum();
                        EditAndSaveBuildingsData(sender, e);
                        EditB.FillValues();

                        //add BUILDINGS layer if not already existing
                        CheckForExistingDrawingObject("BUILDINGS");
                        
                        MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditB.ItemData.Count - numbuildings) + " buildings imported.", Location);
                        Box.Show();
                        if (EditB.ItemData.Count - numbuildings > 0)
                        {
                            MainForm.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonBuildings, Gral.ButtonColorEnum.RedDot); // Building label red & delete buildings.dat
                        }
                    }
                    catch
                    {
                        //MessageBox.Show("Error when reading building.txt Check or delete some or all entries");
                    }
                }
                else
                {
                    Waitprogressbar wait = new Waitprogressbar("Loading shape file");
                    try
                    {
                        //add BUILDINGS layer if not already existing
                        DrawingObjects _drobj = ItemOptions[CheckForExistingDrawingObject("BUILDINGS")];
                        
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
                            Cursor = Cursors.Default;
                            wait.Hide();
                            DialogResult dial = new DialogResult();
                            shp.StartPosition = FormStartPosition.Manual;
                            shp.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
                            shp.Top = 80;
                            dial = shp.ShowDialog();
                            {
                            }

                            EditB.SetTrackBarMaximum();
                            EditAndSaveBuildingsData(sender, e);
                            EditB.FillValues();
                            
                            MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditB.ItemData.Count - numbuildings) + " buildings imported.", Location);
                            Box.Show();
                            if (EditB.ItemData.Count - numbuildings > 0)
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