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
 * User: Markus
 * Date: 17.01.2019
 * Time: 17:05
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Windows.Forms;
using GralMessage;
using GralStaticFunctions;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
    {
        /// <summary>
        /// routine computes mettimeseries.dat and meteopgt.all
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRALGenerateMeteoFiles(object sender, EventArgs e)
        {
            string filename = Path.Combine("Computation", "meteopgt.all");
            string newPath = Path.Combine(ProjectName, filename);
            if (listBox2.Items.Count != 0 && File.Exists(newPath))
            {
                DialogResult result = MessageBox.Show("You are using a GRAMM wind field and met-files exist. Would you delete the existing and create new met.files?", "Create new met-files?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            //this.Cursor = Cursors.WaitCursor;
            //check if all input values and user defined classes are O.K.
            int ok = 0;
            for (int nr = 0; nr < WindSpeedClasses - 1; nr++)
            {
                try
                {
                    if (NumUpDown[nr].Value < decimal.Parse(TBox[nr].Text))
                    {
                        MessageBox.Show("Wind speed needs to be larger", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        NumUpDown[nr].Value = 0M;
                        TBox[nr + 1].Text = NumUpDown[nr].Text;
                        ok = 0;
                        break;
                    }
                    else
                    {
                        ok = 1;
                    }
                }
                catch
                {
                    MessageBox.Show("Wind classes input needs to be a valid number", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
            }

            //generate the gral input files mettimeseries.dat and meteopgt.all
            if (ok > 0)
            {
                try
                {
                    filename = Path.Combine("Computation", "mettimeseries.dat");
                    newPath = Path.Combine(ProjectName, filename);
                    string[] daymonthyear = new string[3];
                    double windclasse = 0;
                    double[] avwclass = new double[WindSpeedClasses];
                    double[] numbwclass = new double[WindSpeedClasses];
                    double avwindri = 0;
                    double[,,] frequmet = new double[WindSpeedClasses, Convert.ToInt32(numericUpDown1.Value), 7];
                    int iwg = 0;
                    int iwr = 0;

                    using (StreamWriter myWriter = File.CreateText(newPath))
                    {
                        //compute average wind speeds for each wind speed class
                        foreach (GralData.WindData data in MeteoTimeSeries)
                        {
                            for (int nr = 0; nr < WindSpeedClasses - 1; nr++)
                            {
                                if (data.Vel <= (double)NumUpDown[nr].Value)
                                {
                                    avwclass[nr] = avwclass[nr] + data.Vel;
                                    numbwclass[nr] = numbwclass[nr] + 1;
                                    break;
                                }
                                if (data.Vel > (double)NumUpDown[WindSpeedClasses - 2].Value)
                                {
                                    avwclass[WindSpeedClasses - 1] = avwclass[WindSpeedClasses - 1] + data.Vel;
                                    numbwclass[WindSpeedClasses - 1] = numbwclass[WindSpeedClasses - 1] + 1;
                                    break;
                                }
                            }
                        }
                        for (int nr = 0; nr < WindSpeedClasses; nr++)
                        {
                            avwclass[nr] = avwclass[nr] / numbwclass[nr];
                        }

                        foreach (GralData.WindData data in MeteoTimeSeries)
                        {
                            daymonthyear = data.Date.Split(new char[] { ':', '-', '.' });
                            for (int nr = 0; nr < WindSpeedClasses - 1; nr++)
                            {
                                if (data.Vel <= (double)(NumUpDown[nr].Value))
                                {
                                    windclasse = Math.Round(avwclass[nr], 2, MidpointRounding.AwayFromZero);
                                    iwg = nr;
                                    break;
                                }
                                if (data.Vel > (double)(NumUpDown[WindSpeedClasses - 2].Value))
                                {
                                    windclasse = Math.Round(avwclass[WindSpeedClasses - 1], 2, MidpointRounding.AwayFromZero);
                                    iwg = WindSpeedClasses - 1;
                                    break;
                                }
                            }
                            WindSectorAngle = 0;
                            for (int j = 0; j < Convert.ToInt32(numericUpDown1.Value) + 1; j++)
                            {
                                WindSectorAngle = WindSectorAngle + 360 / Convert.ToInt32(numericUpDown1.Value);
                                if (Math.Min(data.Dir, 359) < WindSectorAngle)
                                {
                                    avwindri = Math.Round((WindSectorAngle - 0.5 * 360 / Convert.ToInt32(numericUpDown1.Value)) / 10, 1, MidpointRounding.AwayFromZero);
                                    iwr = Math.Min(j, Convert.ToInt32(numericUpDown1.Value) - 1);
                                    break;
                                }
                            }
                            //time series or classification
                            if (checkBox19.Checked == false)
                            {
                                myWriter.WriteLine(daymonthyear[0] + "." + daymonthyear[1] + "," + data.Hour + ","
                                                   + Convert.ToString(windclasse, ic) + "," + Convert.ToString(avwindri, ic) + "," + data.StabClass);
                            }
                            else
                            {
                                myWriter.WriteLine(daymonthyear[0] + "." + daymonthyear[1] + "," + data.Hour + ","
                                                   + Convert.ToString(data.Vel, ic) + "," + Convert.ToString(data.Dir / 10, ic) + "," + data.StabClass);
                            }

                            //compute frequencies of met situations
                            frequmet[iwg, iwr, data.StabClass - 1] = frequmet[iwg, iwr, data.StabClass - 1] + 1;
                        }
                    }

                    //save met data to file meteopgt.all
                    Textbox16_Set(MetfileName);
                    filename = Path.Combine("Computation", "meteopgt.all");
                    newPath = Path.Combine(ProjectName, filename);
                    using (StreamWriter myWriter = new StreamWriter(newPath))
                    {
                        int filelength = MeteoTimeSeries.Count;
                        double[] meteopgtwg = new double[filelength];
                        double[] meteopgtwr = new double[filelength];
                        double[] meteopgtak = new double[filelength];
                        double[] meteopgtfq = new double[filelength];
                        int n = 0;

                        for (iwg = 0; iwg < WindSpeedClasses; iwg++)
                        {
                            WindSectorAngle = 0;
                            for (iwr = 0; iwr < Convert.ToInt32(numericUpDown1.Value); iwr++)
                            {
                                WindSectorAngle = WindSectorAngle + 360 / Convert.ToInt32(numericUpDown1.Value);
                                for (int i = 1; i < 8; i++)
                                {
                                    if (Math.Round(frequmet[iwg, iwr, i - 1] / filelength * 1000, 1, MidpointRounding.AwayFromZero) > 0)
                                    {
                                        meteopgtwr[n] = Math.Round((WindSectorAngle - 0.5 * 360 / Convert.ToInt32(numericUpDown1.Value)) / 10, 1, MidpointRounding.AwayFromZero);
                                        meteopgtfq[n] = Math.Round(frequmet[iwg, iwr, i - 1] / filelength * 1000, 1, MidpointRounding.AwayFromZero);
                                        meteopgtak[n] = i;
                                        meteopgtwg[n] = Math.Round(avwclass[iwg], 2, MidpointRounding.AwayFromZero);
                                        for (int i1 = 0; i1 < n; i1++)
                                        {
                                            if (meteopgtfq[n] > meteopgtfq[i1])
                                            {
                                                for (int i2 = n; i2 > i1; i2--)
                                                {
                                                    meteopgtwr[i2] = meteopgtwr[i2 - 1];
                                                    meteopgtfq[i2] = meteopgtfq[i2 - 1];
                                                    meteopgtak[i2] = meteopgtak[i2 - 1];
                                                    meteopgtwg[i2] = meteopgtwg[i2 - 1];
                                                }
                                                meteopgtwr[i1] = Math.Round((WindSectorAngle - 0.5 * 360 / Convert.ToInt32(numericUpDown1.Value)) / 10, 1, MidpointRounding.AwayFromZero);
                                                meteopgtfq[i1] = Math.Round(frequmet[iwg, iwr, i - 1] / filelength * 1000, 1, MidpointRounding.AwayFromZero);
                                                meteopgtak[i1] = i;
                                                meteopgtwg[i1] = Math.Round(avwclass[iwg], 2, MidpointRounding.AwayFromZero);
                                                break;
                                            }
                                        }
                                        n = n + 1;
                                    }
                                }
                            }
                        }
                        //write anemometer height, flag indicating whether dispersion situations are classified or not, sector width for wind direction in the classification
                        if (checkBox19.Checked == false)
                        {
                            myWriter.WriteLine(Convert.ToString(Anemometerheight, ic) + ",0," + St_F.StrgToICult(textBox1.Text) + ",    !Are dispersion situations classified =0 or not =1");
                        }
                        else
                        {
                            myWriter.WriteLine(Convert.ToString(Anemometerheight, ic) + ",1,0,    !Are dispersion situations classified =0 or not =1");
                        }

                        //myWriter.WriteLine(Convert.ToString(anemometerheight, ic));
                        myWriter.WriteLine("Wind direction sector,Wind speed class,stability class, frequency");

                        //time series or classification
                        if (checkBox19.Checked == false)
                        {
                            for (int i = 0; i < n; i++)
                            {
                                myWriter.WriteLine(Convert.ToString(meteopgtwr[i], ic)
                                                   + "," + Convert.ToString(meteopgtwg[i], ic)
                                                   + "," + Convert.ToString(meteopgtak[i], ic)
                                                   + "," + Convert.ToString(meteopgtfq[i], ic));
                            }
                        }
                        else
                        {
                            foreach (GralData.WindData data in MeteoTimeSeries)
                            {
                                myWriter.WriteLine(Convert.ToString(data.Dir / 10, ic)
                                                   + "," + Convert.ToString(data.Vel, ic)
                                                   + "," + Convert.ToString(data.StabClass, ic)
                                                   + "," + Convert.ToString(1000 / (float)MeteoTimeSeries.Count, ic));
                            }
                        }
                    }

                    //save met input data
                    try
                    {
                        filename = Path.GetFileNameWithoutExtension(MetfileName);
                        newPath = Path.Combine(ProjectName, @"Metfiles" + Path.DirectorySeparatorChar);
                        foreach (string path in Directory.GetFiles(newPath, "*.*", SearchOption.AllDirectories))
                        {
                            File.Delete(path);
                        }

                        newPath = Path.Combine(ProjectName, @"Metfiles", filename + ".met");
                        using (StreamWriter myWriter = new StreamWriter(newPath))
                        {
                            foreach (GralData.WindData data in MeteoTimeSeries)
                            {
                                myWriter.WriteLine(data.Date + "," + data.Time +
                                                   "," + Convert.ToString(data.Vel, ic) +
                                                   "," + Convert.ToString(data.Dir, ic) +
                                                   "," + Convert.ToString(data.StabClass));
                            }
                        }

                        SaveMeteoDataFile();
                        Change_Label(1, 2); // meteo button green
                        ChangedWindData = false;
                    }
                    catch
                    {
                        MessageBox.Show("Could not copy meteorological input file", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    //delete existing windfield and concentration fields
                    newPath = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                    DirectoryInfo di = new DirectoryInfo(newPath);
                    FileInfo[] files_conc = di.GetFiles("*.con");
                    if (files_conc.Length > 0)
                    {
                        using (FileDeleteMessage fdm = new FileDeleteMessage())
                        {
                            if (files_conc.Length > 0)
                            {
                                fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "*.con");
                            }

                            if (fdm.ShowDialog() == DialogResult.OK)
                            {
                                for (int i = 0; i < files_conc.Length; i++)
                                {
                                    files_conc[i].Delete();
                                }
                            }
                        }
                    }
                    files_conc = di.GetFiles("*.grz");
                    if (files_conc.Length > 0)
                    {
                        using (FileDeleteMessage fdm = new FileDeleteMessage())
                        {
                            if (files_conc.Length > 0)
                            {
                                fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "*.grz");
                            }

                            if (fdm.ShowDialog() == DialogResult.OK)
                            {
                                for (int i = 0; i < files_conc.Length; i++)
                                {
                                    files_conc[i].Delete();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Please define Project Folder first. Change to the menu 'Project'.", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            //modify control file for GRAL simulations in.dat
            GenerateInDat();

            //enable/disable GRAL simulations
            Enable_GRAL();
            //enable/disable GRAMM simulations
            Enable_GRAMM();

            // Delete GRAMIn.dat
            try
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "GRAMMin.dat"));
            }
            catch
            { }
            // write new GRAMMin, because of sunrise option
            GRAMMin(true);

            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Create meteopgt.all and mettimeseries.dat for the usage of ERA5 meteo data
        /// </summary>
        private void GRALGenerateMeteoFilesForERA5()
        {
            if (EmifileReset == true)
            {
                if (listBox2.Items.Count != 0 && File.Exists(Path.Combine(ProjectName, "Computation", "meteopgt.all")))
                {
                    DialogResult result = MessageBox.Show(this, "You are using a GRAMM wind field and met-files exist. Would you delete the existing and create new met.files?", "Create new met-files?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                    {
                        return;
                    }
                }

                DateTime _date = dateTimePicker1.Value; // get start date and time

                //generate the gral input files mettimeseries.dat and meteopgt.all
                try
                {
                    string filenameMet = Path.Combine("Computation", "mettimeseries.dat");
                    string newPathMet = Path.Combine(ProjectName, filenameMet);
                    string filenameAll = Path.Combine("Computation", "meteopgt.all");
                    string newPathAll = Path.Combine(ProjectName, filenameAll);
                    string filenameERA = "Artificial Metfile for ERA5 usage.met";
                    string newPathERA = Path.Combine(ProjectName, @"Metfiles" + Path.DirectorySeparatorChar);
                    //foreach (string path in Directory.GetFiles(newPathERA, "*.*", SearchOption.AllDirectories))
                    //{
                    //    File.Delete(path);
                    //}
                    newPathERA = Path.Combine(ProjectName, @"Metfiles", filenameERA);

                    using (StreamWriter myWriterMet = File.CreateText(newPathMet))
                    {
                        using (StreamWriter myWriterAll = new StreamWriter(newPathAll))
                        {
                            using (StreamWriter myWriterERA = new StreamWriter(newPathERA))
                            {
                                for (int i = 1; i < 18000; i++)
                                {
                                    string vel = Math.Round(((int)(i / 360) * 0.1) + 0.1, 2).ToString(ic);
                                    int dirDeg = (i % 360);
                                    string dirDecDeg = (dirDeg * 0.1).ToString(ic);
                                    string _dateTime = _date.Day.ToString(ic) + "." + _date.Month.ToString(ic) + "," + _date.Hour.ToString(ic) + ",";

                                    myWriterMet.WriteLine(_dateTime + vel + "," + dirDecDeg + ",4");

                                    myWriterAll.WriteLine(dirDecDeg + "," + vel + ",4" + ",0.05556");

                                    _dateTime = _date.Day.ToString(ic) + "." + _date.Month.ToString(ic) + "." + _date.Year.ToString(ic) + "," + _date.Hour.ToString(ic) + ":00,";
                                    myWriterERA.WriteLine(_dateTime + vel + "," + dirDeg.ToString(ic) + ",4");
                                    
                                    _date = _date.AddHours(1);
                                }
                            }
                        }
                    }

                    //save met data to file meteopgt.all
                    Textbox16_Set("Artificial Metfile for ERA5 usage.met");

                    MetoColumnSeperator = ',';
                    MeteoDecSeperator = ".";
                    Anemometerheight = 10;

                    SaveMeteoDataFile();
                    Change_Label(1, 2); // meteo button green
                    ChangedWindData = false;
                }
                catch
                {
                    MessageBox.Show("Could not write meteorological input file", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                //modify control file for GRAL simulations in.dat
                GenerateInDat();

                //enable/disable GRAL simulations
                Enable_GRAL();

                //enable/disable GRAMM simulations
                Enable_GRAMM();

                // Delete GRAMIn.dat
                try
                {
                    File.Delete(Path.Combine(ProjectName, @"Computation", "GRAMMin.dat"));
                }
                catch
                { }

                // write new GRAMMin, because of sunrise option
                GRAMMin(true);

                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Create an example python script for downloading pressure level data and surface data for driving GRAMM
        /// </summary>
        private void PythonScriptForERA5()
        {
            //example python script for downloading pressure level data for driving GRAMM
            string newPath = Path.Combine(ProjectName, @"Metfiles" + Path.DirectorySeparatorChar);
            newPath = Path.Combine(ProjectName, @"Metfiles", "ECMWF-ERA5-pl.pyw");
            using (StreamWriter myWriter = new StreamWriter(newPath))
            {
                myWriter.WriteLine("import cdsapi");
                myWriter.WriteLine("c = cdsapi.Client()");
                myWriter.WriteLine("c.retrieve(");
                myWriter.WriteLine("    'reanalysis-era5-complete',");
                myWriter.WriteLine("    {");
                myWriter.WriteLine("		'class'   : 'e5',");
                myWriter.WriteLine("		'expver'  : '1',");
                myWriter.WriteLine("		'stream'  : 'oper',");
                myWriter.WriteLine("		'type'    : 'an',");
                myWriter.WriteLine("		'param'   : '129/130/131/132/133/246/247',");
                myWriter.WriteLine("		'levtype' : 'pl',");
                myWriter.WriteLine("        'levelist': '1000/975/950/925/900/875/850/825/800/775/750/700/650/600/550/500/450/400/350/300/250/225/200/175/150/125/100/70/50/30/20/10/7/5/3/2/1',");
                myWriter.WriteLine("		'date'    : '20180201/to/20180228',");
                myWriter.WriteLine("		'time'    : '00/06/12/18',");
                myWriter.WriteLine("        'area'    : '50/20/40/30',"); //North-West-South-East
                myWriter.WriteLine("        'grid'    : '0.25/0.25',");
                myWriter.WriteLine("    },");
                myWriter.WriteLine("    'Example-ERA5-pressure-levels-Feb18.grib')");
            }

            //example python script for downloading surface data for driving GRAMM
            newPath = Path.Combine(ProjectName, @"Metfiles" + Path.DirectorySeparatorChar);
            newPath = Path.Combine(ProjectName, @"Metfiles", "ECMWF-ERA5-surface.pyw");
            using (StreamWriter myWriter = new StreamWriter(newPath))
            {
                myWriter.WriteLine("import cdsapi");
                myWriter.WriteLine("c = cdsapi.Client()");
                myWriter.WriteLine("c.retrieve(");
                myWriter.WriteLine("    'reanalysis-era5-complete',");
                myWriter.WriteLine("    {");
                myWriter.WriteLine("		'class'   : 'e5',");
                myWriter.WriteLine("		'expver'  : '1',");
                myWriter.WriteLine("		'stream'  : 'oper',");
                myWriter.WriteLine("		'type'    : 'an',");
                myWriter.WriteLine("		'param'   : '34/39/42/139/141/151/164/186/187/188/236',");
                myWriter.WriteLine("		'levtype' : 'sfc',");
                myWriter.WriteLine("		'date'    : '20180201/to/20180228',");
                myWriter.WriteLine("		'time'    : '00/06/12/18',");
                myWriter.WriteLine("        'area'    : '50/20/40/30',"); //North-West-South-East
                myWriter.WriteLine("        'grid'    : '0.25/0.25',");
                myWriter.WriteLine("    },");
                myWriter.WriteLine("    'Example-ERA5-surface-Feb18.grib')");
            }
        }
    }
}