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
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Import portal Sources from *.txt or *.dat file
        /// </summary>
        private void ImportTunnelPortals(object sender, EventArgs e)
        {
            OpenFileDialog dialog = openFileDialog1;
            dialog.Filter = "(Portalsources.txt;portals.dat)|Portalsources.txt;portals.dat";
            dialog.Title = "Select existing tunnel portal data";
            dialog.FileName = "";
            // dialog.ShowHelp = true;
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogSources;
#endif


            int numbvorher = EditPortals.ItemData.Count;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FileName.EndsWith("portals.dat"))
                {
                    int numbportal = EditPortals.ItemData.Count;
                    int i = 1;
                    try
                    {
                        string[] text1 = new string[50];
                        string file = dialog.FileName;
                        using (StreamReader myReader = new StreamReader(file))
                        {
                            string[] polli = new string[4];
                            string dummy = myReader.ReadLine();
                            dummy = myReader.ReadLine();
                            polli[0] = "0";
                            polli[1] = "7";
                            polli[2] = "13";
                            polli[3] = "1";
                            Cursor = Cursors.WaitCursor;
                            for (i = 1; i < 10000000; i++)
                            {
                                dummy = myReader.ReadLine();
                                if (dummy != null)
                                {
                                    text1 = dummy.Split(new char[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                    double[] x2 = new double[2];
                                    double[] y2 = new double[2];
                                    x2[0] = Convert.ToDouble(text1[0].Replace(".", decsep));
                                    x2[1] = Convert.ToDouble(text1[2].Replace(".", decsep));
                                    y2[0] = Convert.ToDouble(text1[1].Replace(".", decsep));
                                    y2[1] = Convert.ToDouble(text1[3].Replace(".", decsep));

                                    double Height = Convert.ToDouble(text1[5].Replace(".", decsep));
                                    double BaseHeight = Convert.ToDouble(text1[4].Replace(".", decsep));

                                    int crosssec = Convert.ToInt32((Math.Abs(Height) - Math.Abs(BaseHeight)) *
                                                   Math.Sqrt(Math.Pow(x2[0] - x2[1], 2) + Math.Pow(y2[0] - y2[1], 2)));

                                    if ((x2[0] >= MainForm.GralDomRect.West) && (x2[0] <= MainForm.GralDomRect.East) &&
                                        (y2[0] >= MainForm.GralDomRect.South) && (y2[0] <= MainForm.GralDomRect.North) &&
                                        (x2[1] >= MainForm.GralDomRect.West) && (x2[1] <= MainForm.GralDomRect.East) &&
                                        (y2[1] >= MainForm.GralDomRect.South) && (y2[1] <= MainForm.GralDomRect.North))
                                    {
                                        PortalsData _po = new PortalsData
                                        {
                                            Name = "Portal " + i.ToString(),
                                            Height = Convert.ToSingle(text1[5], ic),
                                            ExitVel = 1,
                                            DeltaT = 0,
                                            Direction = "1",
                                            Crosssection = crosssec
                                        };
                                        PollutantsData _poll = new PollutantsData
                                        {
                                            SourceGroup = Convert.ToInt32(text1[10])
                                        };
                                        for (i = 0; i < 4; i++)
                                        {
                                            _poll.Pollutant[i] = Convert.ToInt32(polli[i]);
                                            _poll.EmissionRate[i] = Convert.ToDouble(text1[6 + i], ic);
                                        }
                                        _po.Poll.Add(_poll);
                                        _po.Pt1 = new PointD(x2[0], y2[0]);
                                        _po.Pt2 = new PointD(x2[1], y2[1]);

                                        EditPortals.ItemData.Add(_po);

                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }

                            //union all portals with the same location and name
                            for (i = numbvorher; i < EditPortals.ItemData.Count; i++)
                            {
                                for (int j = i + 1; j < EditPortals.ItemData.Count; j++)
                                {
                                    PortalsData _p1 = EditPortals.ItemData[i];
                                    PortalsData _p2 = EditPortals.ItemData[j];
                                    //check identity of first points in both line segments
                                    if (_p1.Name == _p2.Name && ComparePoints(_p1.Pt1, _p2.Pt1))
                                    {
                                        EditPortals.ItemData.RemoveAt(j);
                                        j = Math.Max(i, j - 1);
                                    }
                                }
                            }
                        }
                        EditPortals.SetTrackBarMaximum();
                        EditAndSavePortalSourceData(sender, e);
                        EditPortals.FillValues();

                        MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditPortals.ItemData.Count - numbportal) + " portal sources imported.", Location);
                        Box.Show();
                        //MessageBox.Show("Data import successful: \r\n" + Convert.ToString(editportals.portalsourcedata.Count - numbportal) + " portal sources imported.");
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when reading or filtering portal source data", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //MessageBox.Show(Convert.ToString(i));
                        EditPortals.ItemData.RemoveRange(numbvorher, EditPortals.ItemData.Count - numbvorher);
                    }

                    //add TUNNEL PORTALS layer if not already existing
                    CheckForExistingDrawingObject("TUNNEL PORTALS");
                }

                if (dialog.FileName.EndsWith("Portalsources.txt"))
                {
                    int numbportal = EditPortals.ItemData.Count;
                    PortalsDataIO _pd = new PortalsDataIO();
                    _pd.LoadPortalSources(EditPortals.ItemData, dialog.FileName, true, new RectangleF((float)MainForm.GralDomRect.West, (float)MainForm.GralDomRect.South, (float)(MainForm.GralDomRect.East - MainForm.GralDomRect.West), (float)(MainForm.GralDomRect.North - MainForm.GralDomRect.South)));
                    _pd = null;

                    EditPortals.SetTrackBarMaximum();
                    EditAndSavePortalSourceData(sender, e);
                    EditPortals.FillValues();

                    MessageBoxTemporary Box = new MessageBoxTemporary("Data import successful: \r\n" + Convert.ToString(EditPortals.ItemData.Count - numbportal) + " portal sources imported.", Location);
                    Box.Show();
                    //MessageBox.Show("Data import successful: \r\n" + Convert.ToString(editportals.portalsourcedata.Count - numbportal) + " portal sources imported.");


                    //add TUNNEL PORTALS layer if not already existing
                    CheckForExistingDrawingObject("TUNNEL PORTALS");
                }
            }
            dialog.Dispose();

        }
    }
}