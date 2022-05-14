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
using GralShape;
using GralItemData;
using GralMessage;

namespace GralDomain
{
    public partial class Domain
    {	
        /// <summary>
        /// Import line Sources from *.txt, *.dat or *.shp file
        /// </summary>
        private void ImportGralLineSource(object sender, EventArgs e)
        {
            OpenFileDialog dialog = openFileDialog1;
            dialog.Filter = "(Lsources.txt;LineSourceData.txt,line.dat;*.shp)|Lsources.txt;LineSourceData.txt,line*.dat;*.shp";
            dialog.Title = "Select existing line source data";
            dialog.FileName = "";
            dialog.ShowHelp = true;
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogSources;
#endif


            int numbvorher = EditLS.ItemData.Count;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int numblines = EditLS.ItemData.Count;
                if ((!dialog.FileName.EndsWith("shp")) && (!dialog.FileName.EndsWith("Lsources.txt")) && (!dialog.FileName.EndsWith("LineSourceData.txt")))
                {
                    int i = 1;
                    try
                    {
                        string[] text1 = new string[50];
                        string[] baseyear = new string[2];
                        string file = dialog.FileName;
                        using (StreamReader myReader = new StreamReader(file))
                        {
                            string[] polli = new string[6];
                            string dummy = myReader.ReadLine();
                            dummy = myReader.ReadLine();
                            dummy = myReader.ReadLine();
                            baseyear = dummy.Split(new char[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            dummy = myReader.ReadLine();
                            dummy = myReader.ReadLine();
                            text1 = dummy.Split(new char[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            for (i = 0; i < 6; i++)
                            {
                                polli[i] = Convert.ToString(Gral.Main.PollutantList.IndexOf("Unknown"));
                                for (int i1 = 0; i1 < Gral.Main.PollutantList.Count; i1++)
                                {
                                    if (text1[i + 13] == (Gral.Main.PollutantList[i1] + "[kg/(km*h)]"))
                                    {
                                        polli[i] = Convert.ToString(i1);
                                    }
                                }
                                if (polli[i] == Convert.ToString(Gral.Main.PollutantList.IndexOf("Unknown")))
                                {
                                    MessageBox.Show(this, text1[i + 13] + " is not a registered pollutant name", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }

                            }

                            Cursor = Cursors.WaitCursor;
                            //double zmax = -1000000;
                            //double zmin = 1000000;
                            for (i = 1; i < 10000000; i++)
                            {
                                dummy = myReader.ReadLine();
                                if (dummy != null)
                                {
                                    text1 = dummy.Split(new char[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                    double[] x2 = new double[2];
                                    double[] y2 = new double[2];
                                    double[] z2 = new double[2];
                                    x2[0] = Convert.ToDouble(text1[3].Replace(".", decsep));
                                    y2[0] = Convert.ToDouble(text1[4].Replace(".", decsep));
                                    z2[0] = Convert.ToDouble(text1[5].Replace(".", decsep));
                                    
                                    x2[1] = Convert.ToDouble(text1[6].Replace(".", decsep));
                                    y2[1] = Convert.ToDouble(text1[7].Replace(".", decsep));
                                    z2[1] = Convert.ToDouble(text1[8].Replace(".", decsep));
                                    
                                    double length = Math.Pow((x2[1] - x2[0]), 2) + Math.Pow((y2[1] - y2[0]), 2);

                                    if ((x2[0] >= MainForm.GralDomRect.West) && (x2[1] <= MainForm.GralDomRect.East) &&
                                        (y2[0] >= MainForm.GralDomRect.South) && (y2[1] <= MainForm.GralDomRect.North)
                                        && length > 0)
                                    {
                                        LineSourceData _ls = new LineSourceData
                                        {
                                            Name = text1[0],
                                            Section = text1[1],
                                            Height = Convert.ToSingle(text1[5], ic),
                                            Width = Convert.ToSingle(text1[9], ic)
                                        };

                                        _ls.Nemo.AvDailyTraffic = 1;
                                        if (baseyear.Length > 1)
                                        {
                                            _ls.Nemo.BaseYear = Convert.ToInt32(baseyear[1]);
                                        }
                                        else
                                        {
                                            _ls.Nemo.BaseYear = 2015;
                                        }

                                        PollutantsData _poll = new PollutantsData
                                        {
                                            SourceGroup = Convert.ToInt32(text1[2])
                                        };
                                        for (int k = 0; k < 6; k++)
                                        {
                                            _poll.Pollutant[k] = Convert.ToInt32(polli[k]);
                                            _poll.EmissionRate[k] = Convert.ToDouble(text1[13 + k], ic);
                                        }
                                        _ls.Poll.Add(_poll);
                                        _ls.Pt.Add(new GralData.PointD_3d(x2[0], y2[0], z2[0]));
                                        _ls.Pt.Add(new GralData.PointD_3d(x2[1], y2[1], z2[1]));
                                        if (Math.Abs(z2[0] - z2[1]) < 0.1)
                                        {
                                            _ls.Lines3D = false;
                                        }
                                        else
                                        {
                                            _ls.Lines3D = true;
                                        }

                                        EditLS.ItemData.Add(_ls);
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        //						//union all street segments with the same name, segment number, width, emissions
                        //						string[] dumtext1 = new string[1000];
                        //						string[] dumtext2 = new string[1000];
                        //						for (i = numbvorher; i < editls.ItemData.Count; i++)
                        //						{
                        //							for (int j = i + 1; j < editls.ItemData.Count; j++)
                        //							{
                        //								LineSourceData _ls1 = editls.ItemData[i];
                        //								LineSourceData _ls2 = editls.ItemData[j];
                        //								
                        //								if ((_ls1.Name == _ls2.Name) && (_ls1.Section == _ls2.Section) && (_ls1.Width == _ls2.Width) &&
                        //								    (_ls1.Poll[0].SourceGroup == _ls2.Poll[0].SourceGroup) && 
                        //								    (_ls1.Poll[0].EmissionRate[0] == _ls2.Poll[0].EmissionRate[0]) && 
                        //								    (_ls1.Poll[0].EmissionRate[1] == _ls2.Poll[0].EmissionRate[1]) &&
                        //								    (_ls1.Poll[0].EmissionRate[2] == _ls2.Poll[0].EmissionRate[2]) && 
                        //								    (_ls1.Poll[0].EmissionRate[3] == _ls2.Poll[0].EmissionRate[3]) &&
                        //								    (_ls1.Poll[0].EmissionRate[4] == _ls1.Poll[0].EmissionRate[4]) &&
                        //								    (_ls1.Poll[0].EmissionRate[5] == _ls1.Poll[0].EmissionRate[5]))
                        //								{
                        //									//check identity of all points in both line segments
                        //									if (_ls1.Pt[0] == _ls2.Pt[0] && _ls1.Pt[1] == _ls2.Pt[1])
                        //									{
                        //										editls.ItemData.RemoveAt(j);
                        //										j = Math.Max(i, j - 1);
                        //									}
                        //									//check identity of all points in both line segments
                        //									else if (_ls1.Pt[1] == _ls2.Pt[0] && _ls1.Pt[0] == _ls2.Pt[1])
                        //									{
                        //										editls.ItemData.RemoveAt(j);
                        //										j = Math.Max(i, j - 1);
                        //									}
                        //									//check identity of first points in both line segments
                        //									if (_ls1.Pt[0] == _ls2.Pt[1])
                        //									{
                        //										editls.ItemData[i].Pt.Insert(0, _ls2.Pt[0]);
                        //										editls.ItemData.RemoveAt(j);
                        //										j = Math.Max(i, j - 1);
                        //									}
                        //									//check identity of second points in both line segments
                        //									if (_ls1.Pt[_ls1.Pt.Count] == _ls2.Pt[0])
                        //									{
                        //										editls.ItemData[i].Pt.Add(_ls2.Pt[1]);
                        //										editls.ItemData.RemoveAt(j);
                        //										j = Math.Max(i, j - 1);
                        //									}
                        //									//check identity of first point and second in both line segments
                        //									if (_ls1.Pt[0] == _ls2.Pt[0])
                        //									{
                        //										editls.ItemData[i].Pt.Insert(0, _ls2.Pt[1]);
                        //										editls.ItemData.RemoveAt(j);
                        //										j = Math.Max(i, j - 1);
                        //									}
                        //									//check identity of second point and first point in both line segments
                        //									if (_ls1.Pt[_ls1.Pt.Count] == _ls2.Pt[1])
                        //									{
                        //										editls.ItemData[i].Pt.Add(_ls2.Pt[0]);
                        //										editls.ItemData.RemoveAt(j);
                        //										j = Math.Max(i, j - 1);
                        //									}
                        //								}
                        //							}
                        //						}
                        //						
                        //						//union a second time
                        //						for (i = numbvorher; i < editls.ItemData.Count; i++)
                        //						{
                        //							for (int j = i + 1; j < editls.ItemData.Count; j++)
                        //							{
                        //								LineSourceData _ls1 = editls.ItemData[i];
                        //								LineSourceData _ls2 = editls.ItemData[j];
                        //								
                        //								if ((_ls1.Name == _ls2.Name) && (_ls1.Section == _ls2.Section) && (_ls1.Width == _ls2.Width) &&
                        //								    (_ls1.Poll[0].SourceGroup == _ls2.Poll[0].SourceGroup) && 
                        //								    (_ls1.Poll[0].EmissionRate[0] == _ls2.Poll[0].EmissionRate[0]) && 
                        //								    (_ls1.Poll[0].EmissionRate[1] == _ls2.Poll[0].EmissionRate[1]) &&
                        //								    (_ls1.Poll[0].EmissionRate[2] == _ls2.Poll[0].EmissionRate[2]) && 
                        //								    (_ls1.Poll[0].EmissionRate[3] == _ls2.Poll[0].EmissionRate[3]) &&
                        //								    (_ls1.Poll[0].EmissionRate[4] == _ls1.Poll[0].EmissionRate[4]) &&
                        //								    (_ls1.Poll[0].EmissionRate[5] == _ls1.Poll[0].EmissionRate[5]))
                        //								{
                        //									//check identity of all points in both line segments
                        //									if (_ls1.Pt[0] == _ls2.Pt[0] && _ls1.Pt[1] == _ls2.Pt[1])
                        //									{
                        //										editls.ItemData.RemoveAt(j);
                        //										j = Math.Max(i, j - 1);
                        //									}
                        //									//check identity of all points in both line segments
                        //									else if (_ls1.Pt[1] == _ls2.Pt[0] && _ls1.Pt[0] == _ls2.Pt[1])
                        //									{
                        //										editls.ItemData.RemoveAt(j);
                        //										j = Math.Max(i, j - 1);
                        //									}
                        //									//check identity of first points in both line segments
                        //									if (_ls1.Pt[0] == _ls2.Pt[1])
                        //									{
                        //										editls.ItemData[i].Pt.Insert(0, _ls2.Pt[0]);
                        //										editls.ItemData.RemoveAt(j);
                        //										j = Math.Max(i, j - 1);
                        //									}
                        //									//check identity of second points in both line segments
                        //									if (_ls1.Pt[_ls1.Pt.Count] == _ls2.Pt[0])
                        //									{
                        //										editls.ItemData[i].Pt.Add(_ls2.Pt[1]);
                        //										editls.ItemData.RemoveAt(j);
                        //										j = Math.Max(i, j - 1);
                        //									}
                        //									//check identity of first point and second in both line segments
                        //									if (_ls1.Pt[0] == _ls2.Pt[0])
                        //									{
                        //										editls.ItemData[i].Pt.Insert(0, _ls2.Pt[1]);
                        //										editls.ItemData.RemoveAt(j);
                        //										j = Math.Max(i, j - 1);
                        //									}
                        //									//check identity of second point and first point in both line segments
                        //									if (_ls1.Pt[_ls1.Pt.Count] == _ls2.Pt[1])
                        //									{
                        //										editls.ItemData[i].Pt.Add(_ls2.Pt[0]);
                        //										editls.ItemData.RemoveAt(j);
                        //										j = Math.Max(i, j - 1);
                        //									}
                        //								}
                        //							}
                        //						}
                        //
                        //						//union all segments with the same location, name, segment, width
                        //						for (i = numbvorher; i < editls.ItemData.Count; i++)
                        //						{
                        //							for (int j = i + 1; j < editls.ItemData.Count; j++)
                        //							{
                        //								LineSourceData _ls1 = editls.ItemData[i];
                        //								LineSourceData _ls2 = editls.ItemData[j];
                        //								
                        //								if ((_ls1.Name == _ls2.Name) && (_ls1.Section == _ls2.Section) && (_ls1.Width == _ls2.Width))
                        //								{
                        //									//check identity of first points in both line segments
                        //									if (((_ls2.Pt[0] == _ls1.Pt[0]) && (_ls2.Pt[_ls2.Pt.Count] == _ls1.Pt[_ls1.Pt.Count]))
                        //									    || ((_ls2.Pt[_ls2.Pt.Count] == _ls1.Pt[0]) && (_ls2.Pt[0] == _ls1.Pt[_ls1.Pt.Count])))
                        //									{
                        //										PollutantsData _poll = _ls2.Poll[0];
                        //										editls.ItemData[i].Poll.Add(_poll);
                        //										editls.ItemData.RemoveAt(j);
                        //										j = Math.Max(i, j - 1);
                        //									}
                        //								}
                        //							}
                        //						}

                        EditLS.SetTrackBarMaximum();
                        
                        EditAndSaveLineSourceData(sender, e);
                        EditLS.FillValues();

                        //add LINE SOURCE layer if not already existing
                        CheckForExistingDrawingObject("LINE SOURCES");
                        
                        MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditLS.ItemData.Count - numblines) + " line sources imported.", Location);
                        Box.Show();
                        //MessageBox.Show("Data import successful: \r\n" + Convert.ToString(editls.linesourcedata.Count - numblines) + " line sources imported.");
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when reading or filtering line source data","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        EditLS.ItemData.RemoveRange(numbvorher, EditLS.ItemData.Count - numbvorher);
                    }
                }
                if (dialog.FileName.EndsWith("Lsources.txt") || dialog.FileName.EndsWith("LineSourceData.txt"))
                {
                    try
                    {
                        LineSourceDataIO _ls = new LineSourceDataIO();
                        _ls.LoadLineSources(EditLS.ItemData, dialog.FileName, true, new RectangleF((float) MainForm.GralDomRect.West, (float) MainForm.GralDomRect.South, (float) (MainForm.GralDomRect.East - MainForm.GralDomRect.West), (float) (MainForm.GralDomRect.North - MainForm.GralDomRect.South)));
                        _ls = null;
                        
                        EditAndSaveLineSourceData(sender, e);
                        EditLS.SetTrackBarMaximum();
                        EditLS.FillValues();

                        //add LINE SOURCE layer if not already existing
                        CheckForExistingDrawingObject("LINE SOURCES");
                        
                        MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditLS.ItemData.Count - numblines) + " line sources imported.", Location);
                        Box.Show();
                        //MessageBox.Show("Data import successful: \r\n" + Convert.ToString(editls.linesourcedata.Count - numblines) + " line sources imported.");
                    }
                    catch
                    {
                        MessageBox.Show("Error when reading line source data ..\\Emissions\\Lsources.txt");
                    }
                }
                if (dialog.FileName.EndsWith("shp"))
                {
                    Waitprogressbar wait = new Waitprogressbar("Loading shape file");
                    try
                    {
                        //add LINE SOURCES layer if not already existing
                        DrawingObjects _drobj = ItemOptions[CheckForExistingDrawingObject("LINE SOURCES")];
                        
                        //read geometry data from shape file
                        Cursor = Cursors.WaitCursor;
                        wait.Show();
                        SHPClear(_drobj);
                        //						ShapeReader shape = new ShapeReader(this);
                        //						shape.readShapeFile(dialog.FileName, index);
                        wait.Hide();

                        //open dialog for assigning line source attributes
                        wait.Text = "Loading attribute table";
                        wait.Show();
                        
                        
                        using (ShapeLineDialog shp = new ShapeLineDialog(this, MainForm.GralDomRect, dialog.FileName))
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
                            EditLS.SetTrackBarMaximum();
                            EditAndSaveLineSourceData(sender, e);
                            EditLS.FillValues();
                           
                            MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditLS.ItemData.Count - numblines) + " lines imported.", Location);
                            Box.Show();
                        }
                        SHPClear(_drobj);
                        //MessageBox.Show("Data import successful: \r\n" + Convert.ToString(editls.linesourcedata.Count - numblines) + " lines imported.");
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when reading .shp file","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    wait.Close();
                }
            }
            dialog.Dispose();
        }
        
        private void SHPClear(DrawingObjects _drobj) // delete SHP Files list
        {
            _drobj.ShpPoints.Clear();
            _drobj.ShpLines.Clear();
            _drobj.ShpPolygons.Clear();
            _drobj.ShpPoints.TrimExcess(); // Kuntner Memory
            _drobj.ShpLines.TrimExcess(); // Kuntner
            _drobj.ShpPolygons.TrimExcess(); // Kuntner
        }
        
        private bool ComparePoints(PointD p1, PointD p2)
        {
            if (Math.Abs(p1.X - p2.X) < 0.1 && Math.Abs(p1.Y - p2.Y) < 0.1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
    }
}
