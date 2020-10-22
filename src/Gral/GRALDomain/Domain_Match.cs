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
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using GralIO;
using GralStaticFunctions;
using GralMessage;
using GralDomForms;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Read the GRAMM wind fields
        /// </summary>
        void StartMatchingProcess(object sender, EventArgs e, ref MatchMultipleObservationsData MatchSettings)
        {
            //copy all selected meteo files to the project and get weighting factors
            try
            {
                for (int i = 0; i < MMO.metfiles.Count; i++)
                {
                    try
                    {
                        File.Copy(MMO.metfiles[i], Path.Combine(Gral.Main.ProjectName, "Metfiles", Path.GetFileName(MMO.metfiles[i])), true);
                    }
                    catch
                    { }
                }
            }
            catch { }

            //MessageBox.Show(this, Convert.ToString(MMO.WeightingFactor[0]));

            MMO.Hide(); // Kuntner
            WriteGrammLog(3, "", "", ""); // write log-file
            MessageInfoForm = new MessageWindow
            {
                Text = "GRAMM matching error",
                ShowInTaskbar = false
            }; // Kuntner
            MessageInfoForm.Closed += new EventHandler(MessageFormClosed);

            //delete existing meteo files first
            if (File.Exists(Path.Combine(Gral.Main.ProjectName, "Computation", "meteopgt.all")))
            {
                File.Delete(Path.Combine(Gral.Main.ProjectName, "Computation", "meteopgt.all"));
            }
            if (File.Exists(Path.Combine(Gral.Main.ProjectName, "Computation", "mettimeseries.dat")))
            {
                File.Delete(Path.Combine(Gral.Main.ProjectName, "Computation", "mettimeseries.dat"));
            }
            Waitprogressbar wait = new Waitprogressbar(""); // Kuntner: create wait just one times, so wait can be deleted at the end of the method, also if an error occures
#if __MonoCS__
            wait.Width = 350;
#endif
            Waitprogressbar_Cancel wait1 = new Waitprogressbar_Cancel("Copying GRAMM flow fields");

            //copy and reading geometry file "ggeom.asc"
            wait.Text = "Reading geometry file ggeom.asc";
            wait.Show();
            Application.DoEvents(); // Kuntner

            try
            {
                File.Copy(Path.Combine(Path.GetDirectoryName(MainForm.GRAMMwindfield), @"ggeom.asc"), Path.Combine(Gral.Main.ProjectName, @"Computation", "ggeom.asc"), true);

                GGeomFileIO ggeom = new GGeomFileIO
                {
                    PathWindfield = Path.Combine(Gral.Main.ProjectName, "Computation" + Path.DirectorySeparatorChar)
                };

                double[,] AH = new double[1, 1];
                double[,,] ZSP = new double[1, 1, 1];
                int NX = 1;
                int NY = 1;
                int NZ = 1;
                string[] text = new string[1];

                if (ggeom.ReadGGeomAsc(0) == true)
                {
                    NX = ggeom.NX;
                    NY = ggeom.NY;
                    NZ = ggeom.NZ;
                    AH = ggeom.AH;
                    ZSP = ggeom.ZSP;
                    ggeom = null;
                }
                else
                {
                    throw new FileNotFoundException("Error reading ggeom.asc");
                }

                int[] ix = new int[MMO.metfiles.Count];
                int[] iy = new int[MMO.metfiles.Count];
                int[] ischnitt = new int[MMO.metfiles.Count];
                ix[0] = 0;
                iy[0] = 0;
                ischnitt[0] = 1;

                // Calculate Grid Positions of Met-Stations
                double[] xsi = new double[MMO.metfiles.Count];
                double[] eta = new double[MMO.metfiles.Count];
                for (int i = 0; i < MMO.metfiles.Count; i++)
                {
                    xsi[i] = Convert.ToDouble(MMO.dataGridView1.Rows[i].Cells[1].Value) - MainForm.GrammDomRect.West;
                    eta[i] = Convert.ToDouble(MMO.dataGridView1.Rows[i].Cells[2].Value) - MainForm.GrammDomRect.South;
                }

                //obtain indices of selected point
                MMO.MeteoStationHeight = new double[MMO.metfiles.Count];

                for (int met = 0; met < MMO.metfiles.Count; met++) // loop over all met-Stations
                {
                    MMO.MeteoStationHeight[met] = St_F.TxtToDbl(MMO.dataGridView1.Rows[met].Cells[3].Value.ToString(), false);
                    ix[met] = Convert.ToInt32(Math.Floor(xsi[met] / MainForm.GRAMMHorGridSize)) + 1;  //achtung in x-richtung erst ab index 1 werte gespeichert
                    iy[met] = Convert.ToInt32(Math.Floor(eta[met] / MainForm.GRAMMHorGridSize)) + 1;  //achtung in y-richtung ab index 0 werte, aber in der letzten reihe keine werte mehr
                    if (ix[met] <= 0 || ix[met] >= NX || iy[met] <= 0 || iy[met] >= NY)
                    {
                        ix[met] = Math.Max(1, Math.Min(NX - 1, ix[met]));
                        iy[met] = Math.Max(1, Math.Min(NY - 1, iy[met]));
                        MessageBox.Show(this, "Met. station " + (met + 1).ToString() + " is outside the GRAMM Domain", "GUI Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        MatchSettings.WeightingFactor[met] = 0;
                        MMO.dataGridView1.Rows[met].Cells[6].Value = 0.0;
                    }
                    //obtain index in the vertical direction
                    for (int k = 1; k <= NZ; k++)
                    {
                        if (ZSP[ix[met], iy[met], k] - AH[ix[met], iy[met]] >= MMO.MeteoStationHeight[met])
                        {
                            ischnitt[met] = k;
                            break;
                        }
                    }
                }
                wait.Hide(); // Kuntner: close wait at he end of the method

                //windfield file Readers
                wait.Text = "Match - Reading GRAMM flow fields";
                wait.Show();
                Application.DoEvents(); // Kuntner

                string path = Path.Combine(MainForm.GRAMMwindfield, @"meteopgt.all");

                long NumberofWeatherSituations = St_F.CountLinesInFile(Path.Combine(MainForm.GRAMMwindfield, @"meteopgt.all")) - 2;

                MMO.WindDirMeteoPGT = new double[NumberofWeatherSituations + 2];
                MMO.WindVelMeteoPGT = new double[NumberofWeatherSituations + 2];
                MMO.StabClassMeteoPGT = new int[NumberofWeatherSituations + 2];
                MMO.LocalStabilityClass = new int[MMO.metfiles.Count, NumberofWeatherSituations + 2];
                MMO.WindDir = new int[MMO.metfiles.Count, NumberofWeatherSituations + 2];
                MMO.UGramm = new double[MMO.metfiles.Count, NumberofWeatherSituations + 2];
                MMO.VGramm = new double[MMO.metfiles.Count, NumberofWeatherSituations + 2];
                MMO.WGramm = new double[MMO.metfiles.Count, NumberofWeatherSituations + 2];

                int iiwet = 0;

                float[,,] UWI = new float[NX + 1, NY + 1, NZ + 1];
                float[,,] VWI = new float[NX + 1, NY + 1, NZ + 1];
                float[,,] WWI = new float[NX + 1, NY + 1, NZ + 1];
                MMO.MeteoOriginalHeader = new string[2];

                StreamReader meteopgt_ori = new StreamReader(Path.Combine(MainForm.GRAMMwindfield, @"meteopgt.all")); // Read from original meteopgt.all

                // Remember the original Header
                MMO.MeteoOriginalHeader[0] = meteopgt_ori.ReadLine();
                MMO.MeteoOriginalHeader[1] = meteopgt_ori.ReadLine();

                // Read original Meteo-Data
                //loop over all weather situations
                for (int n = 1; n < NumberofWeatherSituations + 1; n++) // n = loop over all meteopgt.all lines
                {
                    iiwet += 1;
                    wait.Text = "Match - Reading GRAMM flow field " + Convert.ToString(iiwet);
                    try
                    {
                        text = meteopgt_ori.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        MMO.WindDirMeteoPGT[n] = Convert.ToDouble(text[0].Replace(".", decsep));
                        MMO.WindVelMeteoPGT[n] = Convert.ToDouble(text[1].Replace(".", decsep));
                        MMO.StabClassMeteoPGT[n] = Convert.ToInt32(text[2]); // global stability class

                        for (int met = 0; met < MMO.metfiles.Count; met++) // loop over all met-Stations
                        {
                            MMO.LocalStabilityClass[met, n] = MMO.StabClassMeteoPGT[n];        // local stability class
                        }

                        string a = text[0] + "," + text[1] + "," + text[2]; // Remember the text-part of original meteopgt.all
                        MatchSettings.PGT.Add(new GralData.PGTAll() { PGTString = a, PGTFrq = -1 }); // negative Frequ is a marker for unused situations
                    }
                    catch
                    {
                        //						mess.listBox1.Items.Add("Error reading meteopgt.all number " + Convert.ToString(iiwet)); // Kuntner
                        //						mess.Show();
                        break;
                    }

                    bool windfield_OK = true;
                    try
                    {
                        //read wind fields
                        string wndfilename = Path.Combine(Path.GetDirectoryName(MainForm.GRAMMwindfield), Convert.ToString(iiwet).PadLeft(5, '0') + ".wnd");
                        Windfield_Reader Reader = new Windfield_Reader();
                        if (Reader.Windfield_read(wndfilename, NX, NY, NZ, ref UWI, ref VWI, ref WWI) == false)
                        {
                            throw new IOException();
                        }

                        // read stability class and set MMO.LocalStabilityClass[n] to a local value
                        if (MMO.Local_Stability)
                        {
                            double[,] zlevel = new double[1, 1];
                            //wait.Text = "Match - Reading GRAMM stability field " + Convert.ToString(iiwet);
                            string stabilityfilename = Path.Combine(Path.GetDirectoryName(MainForm.GRAMMwindfield), Convert.ToString(iiwet).PadLeft(5, '0') + ".scl");
                            ReadSclUstOblClasses ReadStablity = new ReadSclUstOblClasses
                            {
                                FileName = stabilityfilename,
                                Stabclasses = zlevel
                            };

                            if (ReadStablity.ReadSclFile()) // true => reader = OK
                            {
                                zlevel = ReadStablity.Stabclasses;
                                //								mess.listBox1.Items.Add(Convert.ToString(MMO.LocalStabilityClass[n]) + "/" + Convert.ToString(result));
                                //								mess.Show();
                                //								Application.DoEvents();
                                for (int met = 0; met < MMO.metfiles.Count; met++) // loop over all met-Stations
                                {
                                    if (ix[met] > 0 && iy[met] > 0 && ix[met] < NX && iy[met] < NY)
                                    {
                                        //int result = (int) zlevel[ix[met] - 1, iy[met] - 1]; // use coordinates of the 1st station
                                        int result = ReadStablity.SclMean(ix[met] - 1, iy[met] - 1);
                                        MMO.LocalStabilityClass[met, n] = result;
                                    }
                                }

                            }
                        }

                    }
                    catch
                    {
                        if (MessageInfoForm == null)
                        {
                            MessageInfoForm = new MessageWindow
                            {
                                Text = "GRAMM matching error",
                                ShowInTaskbar = false
                            }; // Kuntner
                            MessageInfoForm.Closed += new EventHandler(MessageFormClosed);
                        }
                        MessageInfoForm.listBox1.Items.Add("Unable to read wind field nr. " + Convert.ToString(iiwet));
                        MessageInfoForm.Show();

                        Application.DoEvents(); // Kuntner
                        windfield_OK = false;
                        //break;
                    }

                    //extract wind speeds from GRAMM wind field at the meteo-station locations
                    if (windfield_OK == true)
                    {
                        try
                        {
                            for (int met = 0; met < MMO.metfiles.Count; met++) // loop over all met-Stations
                            {
                                double Uoben = 0; double Voben = 0; double Uunten = 0; double Vunten = 0; double Umittel = 0; double Vmittel = 0;
                                int _ix = 0; int _iy = 0; int _counter = 0; int _countNumber = 0;

                                if (MMO.checkBox4.Checked) // 3x3 average wind vector
                                {
                                    _ix = ix[met] - 1;
                                    _iy = iy[met] - 1;
                                    _countNumber = 9;
                                }
                                else
                                {
                                    _ix = ix[met];
                                    _iy = iy[met];
                                }

                                Application.DoEvents();

                                //Loop needed for the alternative 3x3 average wind vector
                                do
                                {
                                    _ix = Math.Max(1, Math.Min(NX - 1, _ix));
                                    _iy = Math.Max(1, Math.Min(NY - 1, _iy));
                                    Uoben = UWI[_ix, _iy, ischnitt[met]];
                                    Voben = VWI[_ix, _iy, ischnitt[met]];
                                    if (ischnitt[met] > 1)
                                    {
                                        Uunten = UWI[_ix, _iy, ischnitt[met] - 1];
                                        Vunten = VWI[_ix, _iy, ischnitt[met] - 1];
                                        Umittel += Uunten + (Uoben - Uunten) / (ZSP[_ix, _iy, ischnitt[met]] - ZSP[_ix, _iy, ischnitt[met] - 1]) *
                                            (MMO.MeteoStationHeight[met] + AH[_ix, _iy] - ZSP[_ix, _iy, ischnitt[met] - 1]);
                                        Vmittel += Vunten + (Voben - Vunten) / (ZSP[_ix, _iy, ischnitt[met]] - ZSP[_ix, _iy, ischnitt[met] - 1]) *
                                            (MMO.MeteoStationHeight[met] + AH[_ix, _iy] - ZSP[_ix, _iy, ischnitt[met] - 1]);
                                    }
                                    else
                                    {
                                        Umittel += Uoben / (ZSP[_ix, _iy, ischnitt[met]] - AH[_ix, _iy]) * MMO.MeteoStationHeight[met];
                                        Vmittel += Voben / (ZSP[_ix, _iy, ischnitt[met]] - AH[_ix, _iy]) * MMO.MeteoStationHeight[met];
                                    }
                                    _counter++;
                                    _ix++;
                                    if (_counter % 3 == 0) // new line
                                    {
                                        _ix = ix[met] - 1;
                                        _iy++;
                                    }
                                }
                                while (_counter < _countNumber); //one loop if one cell or 9 cells computed

                                // calculate mean value of the wind components
                                if (_counter > 0)
                                {
                                    Umittel /= _counter;
                                    Vmittel /= _counter;
                                }

                                if (Vmittel == 0)
                                {
                                    MMO.WindDir[met, n] = 90;
                                }
                                else
                                {
                                    MMO.WindDir[met, n] = Convert.ToInt32(Math.Abs(Math.Atan(Umittel / Vmittel)) * 180 / 3.14);
                                }

                                if ((Vmittel > 0) && (Umittel <= 0))
                                {
                                    MMO.WindDir[met, n] = 180 - MMO.WindDir[met, n];
                                }

                                if ((Vmittel >= 0) && (Umittel > 0))
                                {
                                    MMO.WindDir[met, n] = 180 + MMO.WindDir[met, n];
                                }

                                if ((Vmittel < 0) && (Umittel >= 0))
                                {
                                    MMO.WindDir[met, n] = 360 - MMO.WindDir[met, n];
                                }

                                // MMO.WGramm = lenght of vector
                                MMO.WGramm[met, n] = Math.Sqrt(Umittel * Umittel + Vmittel * Vmittel);
                                MMO.UGramm[met, n] = Umittel;
                                MMO.VGramm[met, n] = Vmittel;
                            } // loop for all met-Stations
                        }
                        catch { }
                    } // windfield not available
                    else
                    {
                        for (int met = 0; met < MMO.metfiles.Count; met++)
                        {
                            MMO.WGramm[met, n] = 1000;
                            MMO.UGramm[met, n] = -1000;
                            MMO.VGramm[met, n] = -1000;
                        }
                    }
                    Application.DoEvents(); // Kuntner
                } // loopover all classified meteo-data in the orginal meteopgt.all
                meteopgt_ori.Close(); // Close original "meteopgt.all"
                meteopgt_ori.Dispose();
            }
            catch
            { }

            wait1.Close(); // Kuntner now wait and wait 1 are closed
            wait.Close();
            if (MessageInfoForm != null)
            {
                MessageInfoForm.Close();
            }
        }

        /// <summary>
        /// Finish match process
        /// </summary>
        void MatchFinish(object sender, EventArgs e, MatchMultipleObservationsData MatchData)
        {
            if (MMO.StartMatch) // otherwise cancel matching process
            {
                decimal concatenation = MMO.concatenate.Value;
                Waitprogressbar wait = new Waitprogressbar(""); // Kuntner: create wait just one times, so wait can be deleted at the end of the method, also if an error occures
#if __MonoCS__
            wait.Width = 350;
#endif
                Waitprogressbar_Cancel wait1 = new Waitprogressbar_Cancel("Copying GRAMM flow fields");

                try
                {

                    //________________________________________________________________________________________________________________
                    wait.Hide();
                    wait.Text = "Writing meteopgt.all";
                    wait.Show();
                    Application.DoEvents(); // Kuntner

                    // now create a new meteopgt.all
                    // first sort the MatchData.PGT class
                    MatchData.PGT.Sort();
                    MatchData.PGT.Reverse();

                    if (concatenation > 0.0M)  // move situations with <concatenation ‰ to similar situations
                    {
                        try
                        {
                            // read mettimeseries into string array
                            string newPath = Path.Combine(Gral.Main.ProjectName, "Computation", "mettimeseries.dat");
                            long linennumber = St_F.CountLinesInFile(newPath); // gets number of lines
                            string[] mettimestring = new string[linennumber];  // create string array

                            using (StreamReader reader = new StreamReader(newPath))
                            {
                                int i = 0;
                                string met_day;
                                while ((met_day = reader.ReadLine()) != null)
                                {
                                    mettimestring[i] = met_day;
                                    i++;
                                }
                            }

                            // read mettimeseries

                            for (int rep = 1; rep < MatchData.PGT.Count; rep++) // check all fitted situations
                            {
                                if (MatchData.PGT[rep].PGTFrq <= (double)concatenation && MatchData.PGT[rep].PGTFrq > 0) //just some fitting hours - search similar situation
                                {
                                    string[] replace = MatchData.PGT[rep].PGTString.Split(new char[] { ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                    // search for fitting string
                                    for (int ori = 1; ori < rep; ori++) // check all fitted situations < ori
                                    {
                                        if (MatchData.PGT[ori].PGTFrq > 0) // do not compare unused situation
                                        {
                                            string[] original = MatchData.PGT[ori].PGTString.Split(new char[] { ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                                            if (original.Length > 2 && replace.Length > 2 && replace[1] == original[1] && replace[2] == original[2]) // speed & SC fits
                                            {
                                                if (Math.Abs(St_F.TxtToDbl(replace[0], false) - St_F.TxtToDbl(original[0], false)) < 2.5) //+- 25 Degrees
                                                {
                                                    if (ori != rep) // dont copy&delete itself
                                                    {
                                                        MatchData.PGT[ori].PGTFrq = MatchData.PGT[ori].PGTFrq + MatchData.PGT[rep].PGTFrq; // increase this situation
                                                        MatchData.PGT[rep].PGTFrq = -1; // delete this situation

                                                        for (int met = 0; met < mettimestring.Length; met++) // replace string in mettimeseries
                                                        {
                                                            string[] text1 = mettimestring[met].Split(new char[] { ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                                            if (text1[2] == replace[1] && text1[3] == replace[0] && text1[4] == replace[2]) // found entry with same v, Dir, SC
                                                            {
                                                                mettimestring[met] = text1[0] + "," + text1[1] + "," +
                                                                    original[1] + "," + original[0] + "," + original[2]; // create new entry at timeseries
                                                            }
                                                        }
                                                        ori = rep; // stop checking original situations
                                                    }
                                                }
                                                Application.DoEvents(); // Kuntner
                                            }
                                        }
                                    }


                                }
                            }

                            // write new mettimeseries
                            using (StreamWriter write_mettimeseries = File.CreateText(newPath))
                            {
                                for (int i = 0; i < mettimestring.Length; i++)
                                {
                                    write_mettimeseries.WriteLine(mettimestring[i]);
                                }
                            }

                        }
                        catch
                        {
                            if (MessageInfoForm == null)
                            {
                                MessageInfoForm = new MessageWindow
                                {
                                    Text = "GRAMM matching error",
                                    ShowInTaskbar = false
                                }; // Kuntner
                                MessageInfoForm.Closed += new EventHandler(MessageFormClosed);
                            }
                            MessageInfoForm.listBox1.Items.Add("Concatenate failed"); // Kuntner
                            MessageInfoForm.Show();
                        }
                        MatchData.PGT.Sort();
                        MatchData.PGT.Reverse();
                    }

                    try
                    {
                        //save met data to file meteopgt.all
                        string filename = Path.Combine("Computation", "meteopgt.all");
                        string newPath = Path.Combine(Gral.Main.ProjectName, filename);

                        try
                        {
                            using (StreamWriter myWriter = File.CreateText(newPath))
                            {
                                string[] text1 = MMO.MeteoOriginalHeader[0].Split(new char[] { ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                int c = 0;
                                string temp = string.Empty;
                                foreach (string txt in text1)
                                {
                                    if (c == 1) // set flag for classified meteorology to 0 ( = classified)
                                    {
                                        temp += "0" + ",";
                                    }
                                    else
                                    {
                                        temp += txt + ",";
                                    }
                                    ++c;
                                }
                                if (temp.Length >= MMO.MeteoOriginalHeader[0].Length)
                                {
                                    MMO.MeteoOriginalHeader[0] = temp;
                                }

                                myWriter.WriteLine(MMO.MeteoOriginalHeader[0]);
                                myWriter.WriteLine(MMO.MeteoOriginalHeader[1]);
                                foreach (GralData.PGTAll a_Line in MatchData.PGT)
                                {
                                    string a = a_Line.ToString(); // + ", orig "+Convert.ToString(a_Line.PGT_Number);
                                    if (a_Line.PGTFrq >= 0) // otherwise this Situation didn´t fit
                                    {
                                        myWriter.WriteLine(a); // write sortet PGT_ALL
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        wait.Hide();
                        wait1.Show();

                        int i = 0;
                        foreach (GralData.PGTAll a_Line in MatchData.PGT)
                        {
                            if (a_Line.PGTFrq >= 0) // otherwise this Situation didn´t fit
                            {
                                //copy corresponding GRAMM fields
                                string windfieldfile = Convert.ToString(a_Line.PGTNumber).PadLeft(5, '0') + ".wnd";//copy selected wind field file
                                string orifile = Path.Combine(Path.GetDirectoryName(MainForm.GRAMMwindfield), windfieldfile);
                                string targetfile = Path.Combine(Path.Combine(Gral.Main.ProjectName, "Computation"), Convert.ToString(i + 1).PadLeft(5, '0') + ".wnd");
                                File.Copy(orifile, targetfile, true);

                                // copy .scl file
                                windfieldfile = Convert.ToString(a_Line.PGTNumber).PadLeft(5, '0') + ".scl";//copy selected .scl file
                                orifile = Path.Combine(Path.GetDirectoryName(MainForm.GRAMMwindfield), windfieldfile);
                                targetfile = Path.Combine(Path.Combine(Gral.Main.ProjectName, "Computation"), Convert.ToString(i + 1).PadLeft(5, '0') + ".scl");
                                if (File.Exists(orifile))
                                {
                                    File.Copy(orifile, targetfile, true);
                                }

                                // copy .obl file
                                windfieldfile = Convert.ToString(a_Line.PGTNumber).PadLeft(5, '0') + ".obl";//copy selected .scl file
                                orifile = Path.Combine(Path.GetDirectoryName(MainForm.GRAMMwindfield), windfieldfile);
                                targetfile = Path.Combine(Path.Combine(Gral.Main.ProjectName, "Computation"), Convert.ToString(i + 1).PadLeft(5, '0') + ".obl");
                                if (File.Exists(orifile))
                                {
                                    File.Copy(orifile, targetfile, true);
                                }

                                // copy .ust file
                                windfieldfile = Convert.ToString(a_Line.PGTNumber).PadLeft(5, '0') + ".ust";//copy selected .scl file
                                orifile = Path.Combine(Path.GetDirectoryName(MainForm.GRAMMwindfield), windfieldfile);
                                targetfile = Path.Combine(Path.Combine(Gral.Main.ProjectName, "Computation"), Convert.ToString(i + 1).PadLeft(5, '0') + ".ust");
                                if (File.Exists(orifile))
                                {
                                    File.Copy(orifile, targetfile, true);
                                }

                                i++;
                                if (wait1 != null && wait1.Visible) // Kuntner form does exist and is visible
                                {
                                    wait1.Text = "Copying GRAMM flow field nr. " + Convert.ToString(a_Line.PGTNumber);
                                    wait1.Progressbar_Change_Method(this, 0);

                                }
                                else // Cancel button
                                {
                                    //throw new Exception();
                                }
                            }
                            Application.DoEvents(); // Kuntner

                        }
                    }
                    catch
                    {
                        if (MessageInfoForm == null)
                        {
                            MessageInfoForm = new MessageWindow
                            {
                                Text = "GRAMM matching error",
                                ShowInTaskbar = false
                            }; // Kuntner
                            MessageInfoForm.Closed += new EventHandler(MessageFormClosed);
                        }
                        MessageInfoForm.listBox1.Items.Add("Flow field copying error/canceled"); // Kuntner
                        MessageInfoForm.Show();
                    }

                    wait.Hide();
                    wait1.Hide();


                    Gral.Main.MeteoTimeSeries.Clear();
                    Gral.Main.MeteoTimeSeries.TrimExcess();

                    //copy meteorological input file to the project
                    for (int n1 = 0; n1 < MMO.filelength[0]; n1++)
                    {
                        GralData.WindData data = new GralData.WindData
                        {
                            Hour = MMO.stunde[0][n1],
                            Vel = MMO.wind_speeds[0][n1],
                            Dir = MMO.wind_direction[0][n1],
                            StabClass = MMO.stability[0][n1],
                            Date = MMO.datum[0][n1],
                            Time = MMO.zeit[0][n1]
                        };
                        ;
                        Gral.Main.MeteoTimeSeries.Add(data);
                    }

                    //get classification of dispersion situations
                    try
                    {
                        string metclassification = MainForm.GRAMMwindfield.Replace("Computation" + Path.DirectorySeparatorChar, Path.Combine("Settings", "Meteorology.txt"));
                        if (File.Exists(metclassification))
                        {
                            using (StreamReader streamreader = new StreamReader(metclassification))
                            {
                                string dummy;
                                dummy = streamreader.ReadLine();
                                dummy = streamreader.ReadLine();
                                dummy = streamreader.ReadLine();
                                dummy = streamreader.ReadLine();
                                MMOData.WdClasses = Convert.ToInt32(streamreader.ReadLine());
                                MMOData.WsClasses = Convert.ToInt32(streamreader.ReadLine());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }


                    string metfile = Path.Combine(Gral.Main.ProjectName, "Metfiles", Path.GetFileName(MMO.metfiles[0]));

                    try
                    {
                        //update pointer for meteorology file
                        MainForm.MetfileName = metfile;
                        MainForm.listBox1.Items.Clear();
                        if (Path.GetDirectoryName(metfile).Length > 85)
                        {
                            MainForm.listBox1.Items.Add(metfile.Substring(0, 85) + "...." + Path.DirectorySeparatorChar +
                                    Path.GetFileName(metfile));
                        }
                        else
                        {
                            MainForm.listBox1.Items.Add(metfile);
                        }
                        MainForm.MetoColumnSeperator = MMOData.MetColumnSeperator;
                        MainForm.MeteoDecSeperator = MMOData.MetDecSeperator;

                        //avoid deleting files meteopgt.all and mettimeseries.dat
                        MainForm.EmifileReset = false;
                        MMOData.AnemometerHeight = Math.Max(1, Math.Max(MMOData.AnemometerHeight, MMO.MeteoStationHeight[0]));
                        MainForm.Anemometerheight = MMOData.AnemometerHeight;
                        MainForm.numericUpDown7.Value = (decimal)MMOData.AnemometerHeight;

                        if (MMOData.WdClasses > 0)
                        {
                            MainForm.numericUpDown1.Value = Convert.ToInt16(3600 / MMOData.WdClasses / 10);
                        }

                        if (MMOData.WsClasses > 1 && MMOData.WsClasses < 11)
                        {
                            MainForm.numericUpDown2.Value = MMOData.WsClasses;
                        }

                        MainForm.WindSpeedClasses = MMOData.WsClasses;
                        //				for (int i = 0; i < wsclasses - 1; i++)
                        //				{
                        //					form1.tbox2[i].Text = Convert.ToString(wcl[i]);
                        //					//add values in text boxes
                        //					if (i > 0)
                        //						form1.tbox[i].Text = form1.tbox2[i - 1].Text;
                        //				}
                        MainForm.TBox[0].Text = "0";
                        if (MMOData.WsClasses > 1 && MMOData.WsClasses < 11)
                        {
                            MainForm.TBox[MMOData.WsClasses - 1].Text = MainForm.NumUpDown[MMOData.WsClasses - 2].Text;
                        };
                    }
                    catch { }

                    MainForm.SaveMeteoDataFile();
                    MainForm.button7.Visible = true;
                    MainForm.groupBox1.Visible = true;

                    //update pointer for geometry file
                    string topofile = Path.Combine(Gral.Main.ProjectName, "Settings", "Topography.txt");
                    MainForm.Topofile = Path.Combine(Gral.Main.ProjectName, "Computation", "ggeom.asc");

                    try
                    {
                        try
                        {
                            using (StreamWriter GRAMMwrite = new StreamWriter(topofile))
                            {
                                GRAMMwrite.WriteLine(Path.Combine(Gral.Main.ProjectName, "Computation", "ggeom.asc"));
                                GRAMMwrite.WriteLine(Convert.ToString(MainForm.numericUpDown19.Value, ic));
                                GRAMMwrite.WriteLine(Convert.ToString(MainForm.numericUpDown17.Value, ic));
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        try
                        {
                            //show file name in listbox
                            MainForm.listBox2.Items.Clear();
                            MainForm.listBox2.Items.Add(MainForm.Topofile);
                            //label and box for anemometer height
                            MainForm.groupBox23.Visible = true;

                            //checkbox for time series
                            MainForm.checkBox19.Visible = true;
                            //classification of meteorological data
                            MainForm.groupBox2.Visible = true;
                            MainForm.groupBox3.Visible = true;
                            MainForm.button7.Visible = true;
                            MainForm.Change_Label(1, 2); // meteo button green
                        }
                        catch { }

                        //update pointer for landuse file
                        if (File.Exists(Path.Combine(Gral.Main.ProjectName, @"Computation", "landuse.asc")))
                        {
                            MainForm.Landusefile = Path.Combine(Gral.Main.ProjectName, @"Settings", "Landuse.txt");
                            try
                            {
                                using (StreamWriter GRAMMwrite = new StreamWriter(MainForm.Landusefile))
                                {
                                    GRAMMwrite.WriteLine(Path.Combine(Gral.Main.ProjectName, @"Computation", "landuse.asc"));
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            //show file name in listbox
                            MainForm.Landusefile = Path.Combine(Gral.Main.ProjectName, @"Computation", "landuse.asc");
                            MainForm.listBox6.Items.Clear();
                            MainForm.listBox6.Items.Add(MainForm.Landusefile);
                        }

                        //update pointer for new wind field files
                        MainForm.GRAMMwindfield = Path.Combine(Gral.Main.ProjectName, @"Computation") + Path.DirectorySeparatorChar;
                        try
                        {
                            using (StreamWriter GRAMMwrite = new StreamWriter(Path.Combine(Gral.Main.ProjectName, @"Computation", "windfeld.txt")))
                            {
                                GRAMMwrite.WriteLine(MainForm.GRAMMwindfield);
#if __MonoCS__
                            GRAMMwrite.WriteLine(MainForm.GRAMMwindfield);
#endif
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }


                    MainForm.EmifileReset = true;

                    MessageBoxTemporary Box = new MessageBoxTemporary("Process finished", Location);
                    Box.Show();

                    //MessageBox.Show(this, "Process finished.");

                    Cursor = Cursors.Default;
                    wait1.Close(); // Kuntner now wait and wait 1 are closed
                    wait.Close();

                    string error = "";
                    if (MessageInfoForm != null)
                    {
                        for (int i = 0; i < MessageInfoForm.listBox1.Items.Count; i++)
                        {
                            error += MessageInfoForm.listBox1.Items[i].ToString() + Environment.NewLine;
                        }
                        MessageInfoForm.Close();
                    }
                    WriteGrammLog(4, error, "", ""); // write log-file



                    button43.Visible = false; // match is completed -> hide button

                    // release memory held by MMO.
                    try
                    {
                        if (MMO != null)
                        {
                            MMO.dataGridView1.Rows.Clear();
                            MMO.wind_speeds.Clear();
                            MMO.wind_direction.Clear();
                            MMO.filelength.Clear();
                            MMO.metfiles.Clear();
                            MMO.zeit.Clear();
                            MMO.datum.Clear();
                            MMO.stunde.Clear();
                            MMO.stability.Clear();
                        }
                    }
                    catch { }

#if __MonoCS__
                MMO.close_allowed = true;
                MMO.Close ();
#endif
                }
                catch (Exception ex) // reset all message boxes
                {
                    MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (MessageInfoForm != null)
                    {
                        MessageInfoForm.Closed -= new EventHandler(MessageFormClosed);
                        MessageInfoForm.Close();
                        MessageInfoForm = null;
                    }
                    if (wait != null)
                    {
                        wait.Close();
                    }

                    if (wait1 != null)
                    {
                        wait1.Close();
                    }
#if __MonoCS__
                MMO.close_allowed = true;
                MMO.Close ();
#endif
                }
            }
            MMO.StartMatch = false; // set flag, that match process is finished
        }

        /// <summary>
        /// Cancel match process
        /// </summary>
        void MatchCancel(object sender, EventArgs e)
        {
            MMO.StartMatch = false; // set flag, that match process is finished
            MMO.Hide();
        }

        private void MessageFormClosed(object sender, EventArgs e)
        {
            MessageInfoForm = null;
        }
    }
}
