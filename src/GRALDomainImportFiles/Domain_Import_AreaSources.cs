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
using GralItemData;
using GralMessage;

namespace GralDomain
{
    public partial class Domain
    {
    	/// <summary>
    	/// Import area Sources from *.txt, *.dat or *.shp file
    	/// </summary>
        private void ImportAreaSources(object sender, EventArgs e)
        {
            OpenFileDialog dialog = openFileDialog1;
            dialog.Filter = "(Asources.txt;cadastre.dat;*.shp)|Asources.txt;cadastre*.dat;*.shp";
            dialog.Title = "Select existing area source data";
            dialog.FileName = "";
            // dialog.ShowHelp = true;
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogSources;
#endif


            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int numbarea = EditAS.ItemData.Count;
                
                if ((!dialog.FileName.EndsWith("shp")) && (!dialog.FileName.EndsWith("Asources.txt")))
                {
                    int i = 1;
                   try
                    {
                        string[] text1 = new string[15];
                        string file = dialog.FileName;
                        using (StreamReader myReader = new StreamReader(file))
                        {
                            string[] polli = new string[4];
                            string dummy = myReader.ReadLine();
                            text1 = dummy.Split(new char[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            for (i = 0; i < 4; i++)
                            {
                                polli[i] = Convert.ToString(Gral.Main.PollutantList.IndexOf("Unknown"));
                                for (int i1 = 0; i1 <  Gral.Main.PollutantList.Count; i1++)
                                {
                                    if (text1[i + 6] == (Gral.Main.PollutantList[i1] + "[kg/h]"))
                                    {
                                        polli[i] = Convert.ToString(i1);
                                    }
                                }
                                if (polli[i] == Convert.ToString(Gral.Main.PollutantList.IndexOf("Unknown")))
                                {
                                    MessageBox.Show(this, text1[i + 6] + " is not a registered pollutant name","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }

                            }

                            Cursor = Cursors.WaitCursor;
                            for (i = 1; i < 10000000; i++)
                            {
                                dummy = myReader.ReadLine();
                                if (dummy != null)
                                {
                                    text1 = dummy.Split(new char[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                    double[] x2 = new double[2];
                                    double[] y2 = new double[2];
                                    x2[0] = Convert.ToDouble(text1[0].Replace(".", decsep)) - Convert.ToDouble(text1[3].Replace(".", decsep)) / 2;
                                    x2[1] = Convert.ToDouble(text1[0].Replace(".", decsep)) + Convert.ToDouble(text1[3].Replace(".", decsep)) / 2;
                                    y2[0] = Convert.ToDouble(text1[1].Replace(".", decsep)) - Convert.ToDouble(text1[4].Replace(".", decsep)) / 2;
                                    y2[1] = Convert.ToDouble(text1[1].Replace(".", decsep)) + Convert.ToDouble(text1[4].Replace(".", decsep)) / 2;
                                    if ((x2[0] >= MainForm.GralDomRect.West) && (x2[1] <= MainForm.GralDomRect.East) &&
                                        (y2[0] >= MainForm.GralDomRect.South) && (y2[1] <= MainForm.GralDomRect.North))
                                    {
                                        double areapolygon = Math.Abs((x2[0] - x2[1]) * (y2[0] - y2[1]));
                                        double r1 = Math.Min(Convert.ToDouble(text1[3].Replace(".",decsep)), Convert.ToDouble(text1[4].Replace(".",decsep)));
                                        AreaSourceData _as = new AreaSourceData
                                        {
                                            Name = "Area" + Convert.ToString(i),
                                            Height = Convert.ToSingle(text1[2], ic),
                                            VerticalExt = Convert.ToSingle(text1[5], ic),
                                            RasterSize = (float)r1
                                        };
                                        PollutantsData _poll = new PollutantsData
                                        {
                                            SourceGroup = Convert.ToInt32(text1[10], ic)
                                        };
                                        _as.Area = (float) areapolygon;
                                        for (int k = 0; k < 4; k++)
                                        {
                                            _poll.Pollutant[k] = Convert.ToInt32(polli[k]);
                                            _poll.EmissionRate[k] =Convert.ToDouble(text1[6 + k], ic);
                                        }
                                        _as.Poll = _poll;
                                        _as.Pt.Add(new PointD(x2[0], y2[0]));
                                        _as.Pt.Add(new PointD(x2[0], y2[1]));
                                        _as.Pt.Add(new PointD(x2[1], y2[1]));
                                        _as.Pt.Add(new PointD(x2[1], y2[0]));
                                        _as.Pt.Add(new PointD(x2[0], y2[0]));
                                        
                                        EditAS.ItemData.Add(_as);
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        
                        EditAS.SetTrackBarMaximum();
                        EditAndSaveAreaSourceData(sender, e);
                        EditAS.FillValues();

                        //add AREA SOURCES layer if not already existing
                        CheckForExistingDrawingObject("AREA SOURCES");
                        
                        MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditAS.ItemData.Count - numbarea) + " area sources imported.", Location);
                        Box.Show();
                        //MessageBox.Show("Data import successful: \r\n" + Convert.ToString(editas.areasourcedata.Count - numbarea) + " area sources imported.");
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when reading area source data in line number " + Convert.ToString(i + 1),"GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                
                if (dialog.FileName.EndsWith("Asources.txt"))
                {
                    
                    AreaSourceDataIO _adata = new AreaSourceDataIO();
                    _adata.LoadAreaData(EditAS.ItemData, dialog.FileName, true, new RectangleF((float) MainForm.GralDomRect.West, (float) MainForm.GralDomRect.South, (float) (MainForm.GralDomRect.East - MainForm.GralDomRect.West), (float) (MainForm.GralDomRect.North - MainForm.GralDomRect.South)));
                    _adata = null;
                    
                    EditAS.SetTrackBarMaximum();
                    EditAndSaveAreaSourceData(sender, e);
                    EditAS.FillValues();

                    //add AREA SOURCES layer if not already existing
                    CheckForExistingDrawingObject("AREA SOURCES");
                    
                    MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditAS.ItemData.Count - numbarea) + " area sources imported.", Location);
                    Box.Show();
                    //MessageBox.Show("Data import successful: \r\n" + Convert.ToString(editas.areasourcedata.Count - numbarea) + " area sources imported.");
                }

                if (dialog.FileName.EndsWith("shp"))
                {
                    Waitprogressbar wait = new Waitprogressbar("Loading attribute table");
                    try
                    {
                        //add AREA SOURCES layer if not already existing
                        CheckForExistingDrawingObject("AREA SOURCES");

                        //read geometry data from shape file
                        Cursor = Cursors.WaitCursor;
                        
                        //open dialog for assigning building height
                        wait.Show();
                        
                        GralShape.ShapeAreaDialog shp = new  GralShape.ShapeAreaDialog(this, MainForm.GralDomRect, dialog.FileName);
                        DialogResult dial = new DialogResult();
                        wait.Hide();
                        Cursor = Cursors.Default;
                        shp.StartPosition = FormStartPosition.Manual;
                        shp.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
                        shp.Top = this.Top + 80;
                        dial = shp.ShowDialog();
                        
                        EditAS.SetTrackBarMaximum();
                        EditAndSaveAreaSourceData(sender, e);
                        EditAS.FillValues();
                        
                        MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditAS.ItemData.Count - numbarea) + " area sources imported.", Location);
                        Box.Show();
                        //MessageBox.Show("Data import successful: \r\n" + Convert.ToString(editas.areasourcedata.Count - numbarea) + " area sources imported.");
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when reading shape file","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    wait.Close();
                }
            }
            dialog.Dispose();
        }
    }
}