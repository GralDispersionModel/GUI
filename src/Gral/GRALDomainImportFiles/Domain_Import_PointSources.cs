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
    	/// Import point Sources from *.txt, *.dat or *.shp file
    	/// </summary>
        private void ImportPointSources(object sender, EventArgs e)
        {
            OpenFileDialog dialog = openFileDialog1;
            dialog.Filter = "(Psources.txt;point.dat;*.shp)|Psources.txt;point.dat;*.shp";
            dialog.Title = "Select existing point source data";
            dialog.FileName = "";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int numbpoints = EditPS.ItemData.Count;
                
                if (dialog.FileName.EndsWith("point.dat"))
                {
                    int i = 1;
                    try
                    {
                        string[] text1 = new string[28];
                        string file = dialog.FileName;
                        using (StreamReader myReader = new StreamReader(file))
                        {
                            string dummy = myReader.ReadLine();
                            int[] polli = new int[4];
                            dummy = myReader.ReadLine();
                            text1 = dummy.Split(new char[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            for (i = 0; i < 4; i++)
                            {
                                polli[i] = Gral.Main.PollutantList.IndexOf("Unknown");
                                for (int i1 = 0; i1 < Gral.Main.PollutantList.Count; i1++)
                                {
                                    if (text1[i + 3] == (Gral.Main.PollutantList[i1] + "[kg/h]"))
                                    {
                                        polli[i] = i1;
                                    }
                                }
                                
                                if (polli[i] == Gral.Main.PollutantList.IndexOf("Unknown"))
                                {
                                    MessageBox.Show(this, text1[i + 3] + " is not a registered pollutant name","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }

                            }
                            Cursor = Cursors.WaitCursor;

                            for (i = 1; i < 10000000; i++)
                            {
                                dummy = myReader.ReadLine();
                                if (dummy != null)
                                {
                                    text1 = dummy.Split(new char[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                    double x0 = Convert.ToDouble(text1[0], ic);
                                    double y0 = Convert.ToDouble(text1[1], ic);
                                    
                                    if ((x0 >= MainForm.GralDomRect.West) && (x0 <= MainForm.GralDomRect.East) &&
                                        (y0 >= MainForm.GralDomRect.South) && (y0 <= MainForm.GralDomRect.North))
                                    {
                                        PointSourceData _psdata = new PointSourceData
                                        {
                                            Name = "Stack" + Convert.ToString(i),
                                            Pt = new PointD(x0, y0),
                                            Height = Convert.ToSingle(text1[2], ic),
                                            Velocity = Convert.ToSingle(text1[7], ic),
                                            Diameter = Convert.ToSingle(text1[8], ic),
                                            Temperature = Convert.ToSingle(text1[9], ic)
                                        };

                                        _psdata.Poll.SourceGroup = Convert.ToInt32(text1[10]);
                                        for (i = 0; i < 4; i++)
                                        {
                                            _psdata.Poll.Pollutant[i] = polli[i];
                                            _psdata.Poll.EmissionRate[i] = Convert.ToDouble(text1[3 + i], ic);
                                        }
                                        if (text1.Length > 16)
                                        {
                                            _psdata.GetDep()[0].Frac_2_5 = Convert.ToInt32(text1[11]);
                                            _psdata.GetDep()[0].Frac_10 = Convert.ToInt32(text1[12]);
                                            _psdata.GetDep()[0].DM_30 = Convert.ToInt32(text1[13]);
                                            _psdata.GetDep()[0].Density = Convert.ToDouble(text1[14], ic);
                                            _psdata.GetDep()[0].V_Dep1 = Convert.ToDouble(text1[15], ic);
                                            _psdata.GetDep()[0].V_Dep2 = Convert.ToDouble(text1[16], ic);
                                            _psdata.GetDep()[0].V_Dep3 = Convert.ToDouble(text1[17], ic);
                                            _psdata.GetDep()[0].Conc = Convert.ToInt32(text1[18], ic);
                                        }
                                        EditPS.ItemData.Add(_psdata);
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        
                        EditPS.SetTrackBarMaximum();
                        EditAndSavePointSourceData(sender, e);
                        EditPS.FillValues();

                        //add POINT SOURCES layer if not already existing
                        CheckForExistingDrawingObject("POINT SOURCES");
                        
                        MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditPS.ItemData.Count - numbpoints) + " point sources imported.", Location);
                        Box.Show();
                        //MessageBox.Show("Data import successful: \r\n" + Convert.ToString(editps.psourcedata.Count - numbpoints) + " point sources imported.");
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when reading point source data in line number " + Convert.ToString(i + 2),"GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                if (dialog.FileName.EndsWith("Psources.txt"))
                {
                    int j = 0;
                    try
                    {
                        try
                        {
                            PointSourceDataIO _ps = new PointSourceDataIO();
                            _ps.LoadPointSources(EditPS.ItemData, dialog.FileName, true, new RectangleF((float) MainForm.GralDomRect.West, (float) MainForm.GralDomRect.South, (float) (MainForm.GralDomRect.East - MainForm.GralDomRect.West), (float) (MainForm.GralDomRect.North - MainForm.GralDomRect.South)));
                            _ps = null;
                            
                            EditPS.SetTrackBarMaximum();
                            if (EditPS.ItemData.Count > 0)
                            {
                                button8.Visible = true;
                                button38.Visible = true;
                            }
                        }
                        catch
                        {
                            //MessageBox.Show("Error when reading point source data ..\\Emissions\\Psources.txt in line number " + Convert.ToString(j + 2) + "\t\nCheck or delete some or all entries");
                        }


                        EditPS.SetTrackBarMaximum();
                        EditAndSavePointSourceData(sender, e);
                        EditPS.FillValues();

                        //add POINT SOURCES layer if not already existing
                        CheckForExistingDrawingObject("POINT SOURCES");
                        
                        MessageBox.Show(this, "Data import successful: \r\n" + Convert.ToString(EditPS.ItemData.Count - numbpoints) + " point sources imported.","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when reading point source data in line number " + Convert.ToString(j + 2),"GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                
                if (dialog.FileName.EndsWith("shp"))
                {
                    Waitprogressbar wait = new Waitprogressbar("Loading shape file");
                    try
                    {
                        //add POINT SOURCES layer if not already existing
                        int index = CheckForExistingDrawingObject("POINT SOURCES");
                        
                        //read geometry data from shape file
                        Cursor = Cursors.WaitCursor;
                        wait.Show();
                        //						ShapeReader shape = new ShapeReader(this);
                        //						shape.readShapeFile(dialog.FileName, index);
                        wait.Hide();

                        //open dialog for assigning point source data
                        wait.Text = "Loading attribute table";
                        wait.Show();
                        ShapePointDialog shp = new ShapePointDialog(this, MainForm.GralDomRect, dialog.FileName);
                        DialogResult dial = new DialogResult();
                        wait.Hide();
                        Cursor = Cursors.Default;
                        shp.StartPosition = FormStartPosition.Manual;
                        shp.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
                        shp.Top = 80;
                        dial = shp.ShowDialog();
                        {
                        }
                        EditPS.SetTrackBarMaximum();
                        EditAndSavePointSourceData(sender, e);
                        EditPS.FillValues();
                        
                        MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditPS.ItemData.Count - numbpoints) + " point sources imported.", Location);
                        Box.Show();
                        //MessageBox.Show("Data import successful: \r\n" + Convert.ToString(editps.psourcedata.Count - numbpoints) + " point sources imported.");
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