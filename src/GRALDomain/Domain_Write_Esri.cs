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
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using GralItemData;
using GralMessage;
using GralShape;
using GralStaticFunctions;
using SocialExplorer.IO.FastDBF;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Export a line sources to a shape file with a dbf database
        /// </summary>
        private void ExportShapeLineSource(object sender, EventArgs e)
        {
            SaveFileDialog dialog = saveFileDialog1;
            dialog.Filter = "*.shp|*.shp";
            dialog.Title = "Export line source to .shp file";
            dialog.InitialDirectory = Gral.Main.ProjectName;
            dialog.FileName = "LineSources.shp";
            // dialog.ShowHelp = true;
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogMaps;
#endif

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Waitprogressbar wait = new Waitprogressbar("Export to shape file");
                try
                {
                    //write geometry data to shape file
                    Cursor = Cursors.WaitCursor;
                    wait.Show();
                    ShapeWriter shape = new ShapeWriter(this);
                    shape.WriteShapeFile(dialog.FileName,3);
                    wait.Hide();
                    
                    //create data set with attributes of the line sources
                    DataSet dsline = new DataSet();
                    dsline.Tables.Add();
                    dsline.Tables[0].Columns.Add("Name", typeof(string));
                    dsline.Tables[0].Columns.Add("Part", typeof(string));
                    dsline.Tables[0].Columns.Add("Height", typeof(decimal));
                    dsline.Tables[0].Columns.Add("Vert_Ext", typeof(decimal));
                    dsline.Tables[0].Columns.Add("Width", typeof(decimal));
                    dsline.Tables[0].Columns.Add("Length", typeof(decimal));
                    dsline.Tables[0].Columns.Add("AADT", typeof(decimal));
                    dsline.Tables[0].Columns.Add("HDV", typeof(decimal));
                    dsline.Tables[0].Columns.Add("Slope", typeof(decimal));
                    dsline.Tables[0].Columns.Add("Baseyear", typeof(string));
                    dsline.Tables[0].Columns.Add("Traf_Sit", typeof(string));
                    dsline.Tables[0].Columns.Add("SourceGrp", typeof(int));
                    dsline.Tables[0].Columns.Add("NOx_kg_h_km", typeof(decimal));
                    dsline.Tables[0].Columns.Add("PM10_kg_h_km", typeof(decimal));
                    dsline.Tables[0].Columns.Add("SO2_kg_h_km", typeof(decimal));
                    dsline.Tables[0].Columns.Add("NH3_kg_h_km", typeof(decimal));
                    dsline.Tables[0].Columns.Add("NO2_kg_h_km", typeof(decimal));
                    dsline.Tables[0].Columns.Add("NMVOC_kg_h_km", typeof(decimal));
                    dsline.Tables[0].Columns.Add("HC_kg_h_km", typeof(decimal));
                    dsline.Tables[0].Columns.Add("CO_kg_h_km", typeof(decimal));
                    dsline.Tables[0].Columns.Add("BaP_kg_h_km", typeof(decimal));
                    dsline.Tables[0].Columns.Add("PM25_kg_h_km", typeof(decimal));

                    foreach (LineSourceData _ls in EditLS.ItemData)
                    {
                        //compute integral pollution over all source groups
                        double[] poll = new double[Gral.Main.PollutantList.Count];
                        for (int polindex = 0; polindex < Gral.Main.PollutantList.Count; polindex++)
                        {
                            foreach (PollutantsData _poll in _ls.Poll)
                            {
                                for (int j = 0; j < 10; j++)
                                {
                                    if ((Gral.Main.PollutantList[_poll.Pollutant[j]] == Gral.Main.PollutantList[polindex]) &&
                                        (_poll.EmissionRate[j] != 0))
                                    {
                                        poll[polindex] += _poll.EmissionRate[j];
                                        break;
                                    }
                                }
                            }
                        }

                        string traffic_situation = Gral.Main.NemoTrafficSituations[_ls.Nemo.TrafficSit];

                        double length = St_F.CalcLenght(_ls.Pt);
                        
                        // get vertical extension
                        double vert_extension = _ls.VerticalExt;
                       
                        dsline.Tables[0].Rows.Add(_ls.Name, _ls.Section, _ls.Height.ToString(), _ls.VerticalExt.ToString(),
                            _ls.Width.ToString(), Math.Round(length, 3).ToString(), 
                            _ls.Nemo.AvDailyTraffic.ToString(), _ls.Nemo.ShareHDV.ToString(), _ls.Nemo.Slope.ToString(),
                            _ls.Nemo.BaseYear.ToString(), traffic_situation, _ls.Poll[0].SourceGroup.ToString(),
                            poll[0].ToString(), poll[1].ToString(), poll[3].ToString(), poll[5].ToString(), poll[6].ToString(),
                            poll[7].ToString(), poll[8].ToString(), poll[13].ToString(), poll[14].ToString(), poll[4].ToString());
                    }
                    //write attributes to dbase file
                    TestWriteNewDbf(dsline, dialog.FileName);
                    //WriteDBF wdbf = new WriteDBF();
                    //wdbf.EportDBF(dsline, dialog.FileName);

                    //write shape-index file .shx

                    MessageBoxTemporary Box = new MessageBoxTemporary("Data export successful: \r\n" + Convert.ToString(EditLS.ItemData.Count) + " lines exported.", Location);
                    Box.Show();
                    //MessageBox.Show(this, "Data export successful: \r\n" + Convert.ToString(editls.linesourcedata.Count) + " lines exported.");
                }
                catch(Exception ex)
                {
                    MessageBox.Show(this, ex.Message + "Error when exporting line source data","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Cursor = Cursors.Default;
                wait.Close();
                wait.Dispose();
            }
            dialog.Dispose();	
        }

        /// <summary>
        /// Export a contour line to a shape file with a dbf database
        /// </summary>
        private void ExportShapeContourLine(object sender, EventArgs e)
        {
            DrawingObjects _drobj = null;
            foreach (DrawingObjects _dr in ItemOptions)
            {
                if (_dr.Name.StartsWith("CM") && _dr.Show)
                {
                    _drobj = _dr;
                    break;
                }
            }

            if (_drobj == null)
            {
                MessageBox.Show(this, "No visible contour map available in the drawing stack", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_drobj.DrawSimpleContour && _drobj.ContourPolygons != null) // draw simple contour lines
            {
                SaveFileDialog dialog = saveFileDialog1;
                dialog.Filter = "*.shp|*.shp";
                dialog.Title = "Export contour line to .shp file";
                dialog.InitialDirectory = Gral.Main.ProjectName;
                dialog.FileName = "ContourLine.shp";
                // dialog.ShowHelp = true;
#if NET6_0_OR_GREATER
                dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogMaps;
#endif


                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Waitprogressbar wait = new Waitprogressbar("Export to shape file");
                    try
                    {
                        //write geometry data to shape file
                        Cursor = Cursors.WaitCursor;
                        wait.Show();
                        ShapeWriter shape = new ShapeWriter(this);
                        shape.WriteShapeFile(dialog.FileName, 12);
                        wait.Hide();

                        //create data set with attributes of the line sources
                        DataSet dsline = new DataSet();
                        dsline.Tables.Add();
                        dsline.Tables[0].Columns.Add("Value", typeof(decimal));
                        dsline.Tables[0].Columns.Add("Unit", typeof(string));

                        // MessageBox.Show(_drobj.ContourPolygons.Count.ToString());
                        for (int i = 0; i < _drobj.ContourPolygons.Count; i++)  // loop over all iso-rings
                        {
                            int k = _drobj.ContourPolygons[i].ItemValueIndex; // index of contour value
                            k = Math.Max(0, Math.Min(k, _drobj.FillColors.Count));
                            decimal label_value = Convert.ToDecimal(Math.Round(_drobj.ItemValues[k], _drobj.DecimalPlaces));
                           // MessageBox.Show(label_value.ToString());
                            dsline.Tables[0].Rows.Add(label_value.ToString(), _drobj.LegendUnit);
                        }

                        //write attributes to dbase file
                        TestWriteNewDbf(dsline, dialog.FileName);
                        //WriteDBF wdbf = new WriteDBF();
                        //wdbf.EportDBF(dsline, dialog.FileName);

                        //write shape-index file .shx

                        MessageBoxTemporary Box = new MessageBoxTemporary("Data export successful: \r\n", Location);
                        Box.Show();
                        //MessageBox.Show(this, "Data export successful: \r\n" + Convert.ToString(editls.linesourcedata.Count) + " lines exported.");
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when exporting contour line data", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Cursor = Cursors.Default;
                    wait.Close();
                    wait.Dispose();
                }
                dialog.Dispose();
            }
            else // not simple draw
            {
                MessageBox.Show(this, "The top contour map in the drawing stack is to be exported" +
                    " \nThis map needs the drawing mode 'Spline lines'" +
                    " \nYou can define this mode in the Layout Manager", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Export an area sources to a shape file with a dbf database
        /// </summary>
        private void ExportShapeAreaSource(object sender, EventArgs e)
        {
            SaveFileDialog dialog = saveFileDialog1;
            dialog.Filter = "*.shp|*.shp";
            dialog.Title = "Export area source to .shp file";
            dialog.InitialDirectory = Gral.Main.ProjectName;
            dialog.FileName = "AreaSources.shp";
            // dialog.ShowHelp = true;
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogMaps;
#endif


            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //write geometry data to shape file
                    Cursor = Cursors.WaitCursor;
                    Waitprogressbar wait = new Waitprogressbar("Export to shape file");
                    wait.Show();
                    ShapeWriter shape = new ShapeWriter(this);
                    shape.WriteShapeFile(dialog.FileName, 5);
                    wait.Close();

                    //create data set with attributes of the area sources
                    DataSet dsarea = new DataSet();
                    dsarea.Tables.Add();
                    dsarea.Tables[0].Columns.Add("Name", typeof(string));
                    dsarea.Tables[0].Columns.Add("Height", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("Vert_Ext", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("Area", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("SourceGrp", typeof(int));
                    dsarea.Tables[0].Columns.Add("NOx_kg_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("PM10_kg_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("Odo_MGE_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("SO2_kg_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("PM2_kg_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("NH3_kg_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("NO2_kg_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("NMVOC_kg_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("HC_kg_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("HF_kg_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("HCl_kg_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("H2S_kg_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("F_kg_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("CO_kg_h", typeof(decimal));
                    dsarea.Tables[0].Columns.Add("BaP_kg_h", typeof(decimal));
                    
                    foreach (AreaSourceData _as in EditAS.ItemData)
                    {
                        double[] poll = new double[Gral.Main.PollutantList.Count];
                        for (int j = 0; j < 10; j++)
                        {
                            if (_as.Poll.EmissionRate[j] != 0)
                            {
                                int polindex = _as.Poll.Pollutant[j];
                                poll[polindex] = _as.Poll.EmissionRate[j];
                            }
                        }

                        dsarea.Tables[0].Rows.Add(_as.Name, _as.Height.ToString(), _as.VerticalExt.ToString(),
                                                  _as.Area.ToString(), _as.Poll.SourceGroup.ToString(),
                                                  poll[0].ToString(), poll[1].ToString(), poll[2].ToString(), poll[3].ToString(), 
                                                  poll[4].ToString(), poll[5].ToString(), poll[6].ToString(),
                                                  poll[7].ToString(), poll[8].ToString(), poll[9].ToString(), 
                                                  poll[10].ToString(), poll[11].ToString(), poll[12].ToString(),
                                                  poll[13].ToString(), poll[14].ToString());
                    }
                    //write attributes to dbase file
                    TestWriteNewDbf(dsarea, dialog.FileName);
                    //WriteDBF wdbf = new WriteDBF();
                    //wdbf.EportDBF(dsarea, dialog.FileName);

                    MessageBoxTemporary Box = new MessageBoxTemporary("Data export successful: \r\n" + Convert.ToString(EditAS.ItemData.Count) + " areas exported.", Location);
                    Box.Show();
                    //MessageBox.Show(this, "Data export successful: \r\n" + Convert.ToString(editas.areasourcedata.Count) + " areas exported.");
                }
                catch
                {
                    MessageBox.Show(this, "Error when exporting area source data","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Cursor = Cursors.Default;
            }	
            dialog.Dispose();
        }
        
        /// <summary>
        /// Export a point sources to a shape file with a dbf database
        /// </summary>
        private void ExportShapePointSource(object sender, EventArgs e)
        {
            SaveFileDialog dialog = saveFileDialog1;
            dialog.Filter = "*.shp|*.shp";
            dialog.Title = "Export point source to .shp file";
            dialog.InitialDirectory = Gral.Main.ProjectName;
            dialog.FileName = "PointSources.shp";
            // dialog.ShowHelp = true;
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogMaps;
#endif


            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //write geometry data to shape file
                    Cursor = Cursors.WaitCursor;
                    Waitprogressbar wait = new Waitprogressbar("Export to shape file");
                    wait.Show();
                    ShapeWriter shape = new ShapeWriter(this);
                    shape.WriteShapeFile(dialog.FileName, 1);
                    wait.Close();

                    //create data set with attributes of the point sources
                    DataSet dspoint = new DataSet();
                    dspoint.Tables.Add();
                    dspoint.Tables[0].Columns.Add("Name", typeof(string));
                    dspoint.Tables[0].Columns.Add("Height", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("Exitvel", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("Exittemp", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("Diam", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("SourceGrp", typeof(int));
                    dspoint.Tables[0].Columns.Add("NOx_kg_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("PM10_kg_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("Odo_MGE_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("SO2_kg_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("PM2_kg_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("NH3_kg_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("NO2_kg_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("NMVOC_kg_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("HC_kg_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("HF_kg_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("HCl_kg_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("H2S_kg_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("F_kg_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("CO_kg_h", typeof(decimal));
                    dspoint.Tables[0].Columns.Add("BaP_kg_h", typeof(decimal));
                    
                    foreach (PointSourceData _psdata in EditPS.ItemData)
                    {
                        double[] poll = new double[Gral.Main.PollutantList.Count];
                        for (int j = 0; j < 10; j++)
                        {
                            if (_psdata.Poll.EmissionRate[j] != 0)
                            {
                                int polindex = _psdata.Poll.Pollutant[j];
                                poll[polindex] = _psdata.Poll.EmissionRate[j];
                            }
                        }

                        dspoint.Tables[0].Rows.Add(_psdata.Name, _psdata.Height.ToString(), _psdata.Velocity.ToString(), 
                                                   _psdata.Temperature.ToString(), _psdata.Diameter.ToString(), _psdata.Poll.SourceGroup.ToString(),
                                                   poll[0].ToString(), poll[1].ToString(), poll[2].ToString(), poll[3].ToString(), 
                                                   poll[4].ToString(), poll[5].ToString(), poll[6].ToString(),
                                                   poll[7].ToString(), poll[8].ToString(), poll[9].ToString(),
                                                   poll[10].ToString(), poll[11].ToString(), poll[12].ToString(),
                                                   poll[13].ToString(), poll[14].ToString());
                    }
                    //write attributes to dbase file
                    TestWriteNewDbf(dspoint, dialog.FileName);
                    //WriteDBF wdbf = new WriteDBF();
                    //wdbf.EportDBF(dspoint, dialog.FileName);

                    MessageBoxTemporary Box = new MessageBoxTemporary("Data export successful: \r\n" + Convert.ToString(EditPS.ItemData.Count) + " point sources exported.", Location);
                    Box.Show();
                    //MessageBox.Show(this, "Data export successful: \r\n" + Convert.ToString(editps.psourcedata.Count) + " point sources exported.");
                }
                catch
                {
                    MessageBox.Show(this, "Error when exporting point source data","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Cursor = Cursors.Default;
            }
            dialog.Dispose();
        }
        
        /// <summary>
        /// Export receptors to a shape file with a dbf database
        /// </summary>
        private void ExportShapeReceptor(object sender, EventArgs e)
        {
            SaveFileDialog dialog = saveFileDialog1;
            dialog.Filter = "*.shp|*.shp";
            dialog.Title = "Export receptor points to .shp file";
            dialog.InitialDirectory = Gral.Main.ProjectName;
            dialog.FileName = "ReceptorPoints.shp";
            // dialog.ShowHelp = true;
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogMaps;
#endif


            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //write geometry data to shape file
                    Cursor = Cursors.WaitCursor;
                    Waitprogressbar wait = new Waitprogressbar("Export to shape file");
                    wait.Show();
                    ShapeWriter shape = new ShapeWriter(this);
                    shape.WriteShapeFile(dialog.FileName, 0);
                    wait.Close();

                    //create data set with attributes of the point sources
                    DataSet dsrec = new DataSet();
                    dsrec.Tables.Add();
                    dsrec.Tables[0].Columns.Add("Name", typeof(string));
                    dsrec.Tables[0].Columns.Add("Height", typeof(decimal));
                    
                    foreach (ReceptorData _rd in EditR.ItemData)
                    {
                        dsrec.Tables[0].Rows.Add(_rd.Name, Math.Round(_rd.Height, 1).ToString());
                    }
                    //write attributes to dbase file
                    TestWriteNewDbf(dsrec, dialog.FileName);
                    //WriteDBF wdbf = new WriteDBF();
                    //wdbf.EportDBF(dsrec, dialog.FileName);
                    
                    MessageBoxTemporary Box = new MessageBoxTemporary("Data export successful: \r\n" + Convert.ToString(EditR.ItemData.Count) + " receptors exported.", Location);
                    Box.Show();
                    //MessageBox.Show(this, "Data export successful: \r\n" + Convert.ToString(editr.receptors.Count) + " receptors exported.");
                }
                catch
                {
                    MessageBox.Show(this, "Error when exporting receptors","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Cursor = Cursors.Default;
            }
            dialog.Dispose();
        }
        
        /// <summary>
        /// Routine written by Ahmed Lacevic to write dbf files
        /// </summary>
        private static void TestWriteNewDbf(DataSet dsExport, string filePath)
        {
            CultureInfo ic = CultureInfo.InvariantCulture;
            string tableName = Path.GetFileNameWithoutExtension(filePath);
            tableName = Path.GetFileNameWithoutExtension(filePath) + ".dbf";
            string folderPath = Path.GetDirectoryName(filePath);

            string columnname = " ";
            string[] inhalt = new string[2];

            //create a simple DBF file and output to args[0]
            var odbf = new DbfFile(Encoding.GetEncoding(1252));
            odbf.Open(Path.Combine(folderPath, tableName), FileMode.Create);


            DbfColumn.DbfColumnType dtype = new DbfColumn.DbfColumnType();
            int length1 = 20;
            int length2 = 0;

            //create a header
            for (int iCol = 0; iCol < dsExport.Tables[0].Columns.Count; iCol++)
            {
                //get data type
                switch (Type.GetTypeCode(dsExport.Tables[0].Columns[iCol].DataType))
                {
                    case TypeCode.Decimal:
                        dtype = DbfColumn.DbfColumnType.Number;
                        length1 = 18;
                        length2 = 6;
                        break;
                    case TypeCode.String:
                        dtype = DbfColumn.DbfColumnType.Character;
                        length1 = 100;
                        length2 = 0;
                        break;
                    case TypeCode.DateTime:
                        dtype = DbfColumn.DbfColumnType.Date;
                        length1 = 0;
                        length2 = 0;
                        break;
                }
                columnname = dsExport.Tables[0].Columns[iCol].ColumnName.ToString().Replace("[", "_").Replace("]", "_").Replace("?", "3").Replace(" ", "").Replace(@"/", "");
                //limit length to 11 characters
                if (columnname.Length > 11)
                {
                    columnname = columnname.Substring(0, 11);
                }

                if (length1 != 0)
                {
                    odbf.Header.AddColumn(new DbfColumn(columnname, dtype, length1, length2));
                }
                else
                {
                    odbf.Header.AddColumn(new DbfColumn(columnname, dtype));
                }
            }

            //add records...
            var orec = new DbfRecord(odbf.Header) { AllowDecimalTruncate = true, AllowIntegerTruncate = false };

            for (int row = 0; row < dsExport.Tables[0].Rows.Count; row++)
            {
                for (int col = 0; col < dsExport.Tables[0].Columns.Count; col++)
                {
                    // Remove Special character if any like dot,semicolon,colon,comma // etc
                    dsExport.Tables[0].Rows[row][col].ToString().Replace("LF", "").Replace("-", "");
                    dsExport.Tables[0].Rows[row][col].ToString().Trim();
                    //nur zahlenwerte von OENACE und SNAP werden exportiert
                    inhalt = Convert.ToString(dsExport.Tables[0].Rows[row][col]).Split(new char[] { ':' });
                    if (inhalt[0] == "")
                    {
                        inhalt[0] = "0";
                    }

                    if (double.TryParse(inhalt[0], out double value))
                    {
                        orec[col] = value.ToString(ic);
                    }
                    else
                    {
                        orec[col] = inhalt[0].Replace(",", ".");
                    }
                    
                }
                
                odbf.Write(orec, true);
            }

            odbf.Close();
        }
                    
    }
}
