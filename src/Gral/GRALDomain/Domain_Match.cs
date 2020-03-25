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

namespace GralDomain
{
    public partial class Domain
    {
        private List<GralData.PGTAll> PGT = new List<GralData.PGTAll>();

        /// <summary>
        /// Start or cancel matching GRAMM wind fields to mulitple observations  
        /// </summary>
        void StartMatchingProcess(object sender, EventArgs e)
        {
            //copy all selected meteo files to the project and get weighting factors
            double[] weighting_factor = new double[MMO.metfiles.Count + 1];
            double[] weighting_Direction = new double[MMO.metfiles.Count + 1];
            decimal concatenation = MMO.concatenate.Value;

            for (int i = 0; i < MMO.metfiles.Count; i++)
            {
                weighting_factor[i] = 1;
                weighting_Direction[i] = 1;
            }

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
                    weighting_factor[i] = St_F.TxtToDbl(MMO.dataGridView1.Rows[i].Cells[6].Value.ToString(), false);
                    weighting_factor[i] = Math.Max(weighting_factor[i], 0.0);
                    weighting_Direction[i] = St_F.TxtToDbl(MMO.dataGridView1.Rows[i].Cells[7].Value.ToString(), false);
                    weighting_Direction[i] = Math.Min(10, Math.Max(weighting_Direction[i], 0.0));
                }
            }
            catch { }

            //MessageBox.Show(this, Convert.ToString(weighting_factor[0]));

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
                double[] schnitt = new double[MMO.metfiles.Count];

                for (int met = 0; met < MMO.metfiles.Count; met++) // loop over all met-Stations
                {
                    schnitt[met] = St_F.TxtToDbl(MMO.dataGridView1.Rows[met].Cells[3].Value.ToString(), false);
                    ix[met] = Convert.ToInt32(Math.Floor(xsi[met] / MainForm.GRAMMHorGridSize)) + 1;  //achtung in x-richtung erst ab index 1 werte gespeichert
                    iy[met] = Convert.ToInt32(Math.Floor(eta[met] / MainForm.GRAMMHorGridSize)) + 1;  //achtung in y-richtung ab index 0 werte, aber in der letzten reihe keine werte mehr
                    if (ix[met] <= 0 || ix[met] >= NX || iy[met] <= 0 || iy[met] >= NY)
                    {
                        ix[met] = Math.Max(1, Math.Min(NX - 1, ix[met]));
                        iy[met] = Math.Max(1, Math.Min(NY - 1, iy[met]));
                        MessageBox.Show(this, "Met. station " + (met + 1).ToString() + " is outside the GRAMM Domain", "GUI Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        weighting_factor[met] = 0;
                        MMO.dataGridView1.Rows[met].Cells[6].Value = 0.0;
                    }
                    //obtain index in the vertical direction
                    for (int k = 1; k <= NZ; k++)
                    {
                        if (ZSP[ix[met], iy[met], k] - AH[ix[met], iy[met]] >= schnitt[met])
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

                double[] nsektor = new double[100000];
                double[] wg = new double[100000];
                int[] iakla = new int[100000];
                int[,] localstability = new int[MMO.metfiles.Count, 100000];
                int[,] iwr = new int[MMO.metfiles.Count, 100000];
                double[,] u_GRAMM = new double[MMO.metfiles.Count, 100000];
                double[,] v_GRAMM = new double[MMO.metfiles.Count, 100000];
                double u_METEO;  //= new double[MMO.metfiles.Count, 100000];
                double v_METEO; // = new double[MMO.metfiles.Count, 100000];
                int iiwet = 0;
                double[,] wg_GRAMM = new double[MMO.metfiles.Count, 100000];
                double wg_METEO = 0; // = new double[MMO.metfiles.Count, 100000];
                double richtung = 0;
                float[,,] UWI = new float[NX + 1, NY + 1, NZ + 1];
                float[,,] VWI = new float[NX + 1, NY + 1, NZ + 1];
                float[,,] WWI = new float[NX + 1, NY + 1, NZ + 1];
                string[] Meteo_orig_Header = new string[2];

                int optimization = 1;
                bool outliers = true;
                if (MMO.checkBox1.Checked == false)
                {
                    outliers = false;
                }

                if (MMO.radioButton1.Checked == true)
                {
                    optimization = 1;
                }

                if (MMO.radioButton2.Checked == true)
                {
                    optimization = 2;
                }

                StreamReader meteopgt_ori = new StreamReader(Path.Combine(MainForm.GRAMMwindfield, @"meteopgt.all")); // Read from original meteopgt.all
                                                                                                                      //loop over all weather situations

                // Remember the original Header
                Meteo_orig_Header[0] = meteopgt_ori.ReadLine();
                Meteo_orig_Header[1] = meteopgt_ori.ReadLine();

                // Read original Meteo-Data
                for (int n = 1; n < 100000; n++) // n = loop over all meteopgt.all lines
                {
                    iiwet = iiwet + 1;
                    wait.Text = "Match - Reading GRAMM flow field " + Convert.ToString(iiwet);
                    try
                    {
                        text = meteopgt_ori.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        nsektor[n] = Convert.ToDouble(text[0].Replace(".", decsep));
                        wg[n] = Convert.ToDouble(text[1].Replace(".", decsep));
                        iakla[n] = Convert.ToInt32(text[2]); // global stability class

                        for (int met = 0; met < MMO.metfiles.Count; met++) // loop over all met-Stations
                        {
                            localstability[met, n] = iakla[n];        // local stability class
                        }

                        string a = text[0] + "," + text[1] + "," + text[2]; // Remember the text-part of original meteopgt.all
                        PGT.Add(new GralData.PGTAll() { PGTString = a, PGTFrq = -1 }); // negative Frequ is a marker for unused situations
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

                        // read stability class and set localstability[n] to a local value
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
                                //								mess.listBox1.Items.Add(Convert.ToString(localstability[n]) + "/" + Convert.ToString(result));
                                //								mess.Show();
                                //								Application.DoEvents();
                                for (int met = 0; met < MMO.metfiles.Count; met++) // loop over all met-Stations
                                {
                                    if (ix[met] > 0 && iy[met] > 0 && ix[met] < NX && iy[met] < NY)
                                    {
                                        //int result = (int) zlevel[ix[met] - 1, iy[met] - 1]; // use coordinates of the 1st station
                                        int result = ReadStablity.SclMean(ix[met] - 1, iy[met] - 1);
                                        localstability[met, n] = result;
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
                                            (schnitt[met] + AH[_ix, _iy] - ZSP[_ix, _iy, ischnitt[met] - 1]);
                                        Vmittel += Vunten + (Voben - Vunten) / (ZSP[_ix, _iy, ischnitt[met]] - ZSP[_ix, _iy, ischnitt[met] - 1]) *
                                            (schnitt[met] + AH[_ix, _iy] - ZSP[_ix, _iy, ischnitt[met] - 1]);
                                    }
                                    else
                                    {
                                        Umittel += Uoben / (ZSP[_ix, _iy, ischnitt[met]] - AH[_ix, _iy]) * schnitt[met];
                                        Vmittel += Voben / (ZSP[_ix, _iy, ischnitt[met]] - AH[_ix, _iy]) * schnitt[met];
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
                                    iwr[met, n] = 90;
                                }
                                else
                                {
                                    iwr[met, n] = Convert.ToInt32(Math.Abs(Math.Atan(Umittel / Vmittel)) * 180 / 3.14);
                                }

                                if ((Vmittel > 0) && (Umittel <= 0))
                                {
                                    iwr[met, n] = 180 - iwr[met, n];
                                }

                                if ((Vmittel >= 0) && (Umittel > 0))
                                {
                                    iwr[met, n] = 180 + iwr[met, n];
                                }

                                if ((Vmittel < 0) && (Umittel >= 0))
                                {
                                    iwr[met, n] = 360 - iwr[met, n];
                                }

                                // wg_GRAMM = lenght of vector
                                wg_GRAMM[met, n] = Math.Sqrt(Umittel * Umittel + Vmittel * Vmittel);
                                u_GRAMM[met, n] = Umittel;
                                v_GRAMM[met, n] = Vmittel;
                            } // loop for all met-Stations
                        }
                        catch { }
                    } // windfield not available
                    else
                    {
                        for (int met = 0; met < MMO.metfiles.Count; met++)
                        {
                            wg_GRAMM[met, n] = 1000;
                            u_GRAMM[met, n] = -1000;
                            v_GRAMM[met, n] = -1000;
                        }
                    }
                    Application.DoEvents(); // Kuntner
                } // loopover all classified meteo-data in the orginal meteopgt.all
                meteopgt_ori.Close(); // Close original "meteopgt.all"
                meteopgt_ori.Dispose();

                int[,] Vector_Error_Sum = new int[MMO.metfiles.Count, 4];
                int[,] SC_Error_Sum = new int[MMO.metfiles.Count, 2];

                do // Loop over the Match-Fine tuning
                {
                    Array.Clear(Vector_Error_Sum, 0, Vector_Error_Sum.Length);
                    Array.Clear(SC_Error_Sum, 0, SC_Error_Sum.Length);
                    if (MMO.checkBox1.Checked == false)
                    {
                        outliers = false;
                    }
                    else
                    {
                        outliers = true;
                    }

                    if (MMO.radioButton1.Checked == true)
                    {
                        optimization = 1;
                    }
                    if (MMO.radioButton2.Checked == true)
                    {
                        optimization = 2;
                    }

                    bool stronger_weighted_SC1_and_7 = true;
                    if (MMO.checkBox3.Checked == false)
                    {
                        stronger_weighted_SC1_and_7 = false;
                    }

                    foreach (GralData.PGTAll reset in PGT) // reset PGT_FRQ
                    {
                        reset.PGTFrq = -1;
                    }

                    try
                    {
                        for (int i = 0; i < MMO.metfiles.Count; i++)
                        {
                            weighting_factor[i] = St_F.TxtToDbl(MMO.dataGridView1.Rows[i].Cells[6].Value.ToString(), false);
                            weighting_factor[i] = Math.Max(weighting_factor[i], 0.0);
                            weighting_Direction[i] = St_F.TxtToDbl(MMO.dataGridView1.Rows[i].Cells[7].Value.ToString(), false);
                            weighting_Direction[i] = Math.Min(10, Math.Max(weighting_Direction[i], 0.0));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    iiwet = iiwet - 1; // the maximum of weather situations in the original meteopgt.all

                    wait.Hide();
                    wait.Text = "Matching GRAMM flow fields";
                    wait.Show();
                    Application.DoEvents(); // Kuntner

                    int[] counter = new int[MMO.metfiles.Count]; //zeitzeiger fuer die zeitreihen der einzelnen stationen
                    Array.Clear(counter, 0, MMO.metfiles.Count);
                    bool[] synchron = new bool[MMO.metfiles.Count];
                    string[] dummytext = new string[4];

                    //newPath = Path.Combine(form1.projectname,@"Computation\debug.txt");
                    //StreamWriter write_debug = File.CreateText(newPath);

                    string filename = Path.Combine("Computation", "mettimeseries.dat");

                    StreamWriter write_mettimeseries = File.CreateText(Path.Combine(Gral.Main.ProjectName, filename));

                    //loop over the entire time series
                    //__________________________________________________________________________________________________________________
                    for (int met_count = 0; met_count < MMO.filelength[0]; met_count++) // Loop over all Weather situations
                    {
                        counter[0] = met_count;
                        synchron[0] = true;
                        DateTime date_station0 = new DateTime(1800, 1, 1, 0, 0, 0);
                        DateTime date_stationj = new DateTime(1800, 1, 1, 0, 0, 0);
                        int met_year = 0, met_day = 0, met_month = 0, met_hour = 0;

                        try
                        {
                            // read date and time from forst element
                            dummytext = MMO.datum[0][met_count].Split(new char[] { ' ', ':', '-', '.' });

                            met_year = Convert.ToInt32(dummytext[2]);
                            //in case that the year is given only by two digits the century has to be guessed
                            if (met_year < 100)
                            {
                                if (met_year < 70)
                                {
                                    met_year += 2000;
                                }
                                else
                                {
                                    met_year += 1900;
                                }
                            }
                            met_day = Convert.ToInt32(dummytext[0]);
                            met_month = Convert.ToInt32(dummytext[1]);
                            met_hour = Convert.ToInt32(MMO.stunde[0][met_count]);

                            date_station0 = new DateTime(met_year, met_month, met_day, met_hour, 0, 0);
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
                            MessageInfoForm.listBox1.Items.Add("Can't parse date of obs. station 1 at line " + met_count.ToString()); // Kuntner
                            MessageInfoForm.Show();

                            synchron[0] = false;
                        }

                        Application.DoEvents(); // Kuntner
                        if (synchron[0] == true) // otherwise problem with the time stamp of station0
                        {
                            //synchronizing date and time of all meteo-stations
                            for (int j = 1; j < MMO.metfiles.Count; j++) // loop over all Met-stations
                            {

                                Application.DoEvents(); // Kuntner
                                                        //get year, month, and day regardless the separator character
                                synchron[j] = false;

                                for (int timestep = counter[j]; timestep < MMO.filelength[j]; timestep++)
                                {
                                    int comp = -1;
                                    try
                                    {
                                        dummytext = MMO.datum[j][timestep].Split(new char[] { ' ', ':', '-', '.' });
                                        int met_yearj = Convert.ToInt32(dummytext[2]);
                                        //in case that the year is given only by two digits the century has to be guessed
                                        if (met_yearj < 100)
                                        {
                                            if (met_yearj < 70)
                                            {
                                                met_yearj += 2000;
                                            }
                                            else
                                            {
                                                met_yearj += 1900;
                                            }
                                        }
                                        int met_dayj = Convert.ToInt32(dummytext[0]);
                                        int met_monthj = Convert.ToInt32(dummytext[1]);
                                        int met_hourj = Convert.ToInt32(MMO.stunde[j][timestep]);

                                        date_stationj = new DateTime(met_yearj, met_monthj, met_dayj, met_hourj, 0, 0);
                                        comp = DateTime.Compare(date_stationj, date_station0);
                                        //MessageBox.Show(date_station0 + "/" + date_stationj);
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
                                        MessageInfoForm.listBox1.Items.Add("Can't parse date of obs. station " + (j + 1).ToString() + " at line " + timestep.ToString()); // Kuntner
                                        MessageInfoForm.Show();

                                        comp = -1;
                                    }

                                    if (comp < 0) // t1 is earlier than t2 -> increase t1, do nothing else
                                    {
                                        //counter[j] = timestep;
                                        //MessageInfoForm.listBox1.Items.Add(date_station0.ToString() + "waiting...." + date_stationj.ToString());
                                    }
                                    else if (comp > 0) // t1 is later than t2 -> interrupt for-next loop
                                    {
                                        synchron[j] = false;
                                        timestep = MMO.filelength[j]; //break loop
                                                                      //mess.listBox1.Items.Add("Wating for sync ");
                                        //MessageInfoForm.listBox1.Items.Add(date_station0.ToString() + "waiting...." + date_stationj.ToString());
                                    }
                                    else if (comp == 0) // found same time stamp
                                    {
                                        synchron[j] = true;
                                        counter[j] = timestep;
                                        timestep = MMO.filelength[j]; //break loop
                                                                      //mess.listBox1.Items.Add("Date sync at station " + j.ToString() + " and line " + counter[j].ToString());
                                    }
                                } // time loop for each met station
                            } //loop over met stations

                            //
                            //double debugwr=0; double debugwsp=0;
                            Application.DoEvents(); // Kuntner
                            try
                            {
                                int best_fit = 1; // best fitting weather number
                                double best_err = 100000000; // error of best fitting weather number

                                //search for the GRAMM wind field, which fits best the observed wind data at the observation sites
                                for (int n = 1; n <= iiwet; n++) // n = number of calculated GRAMM fields
                                {                               //optimization criterion: wind vector + weighted wind direction + weighted wind speed
                                    double err = 10000; double err_min = 1000000;
                                    double[] err_st = new double[MMO.metfiles.Count];

                                    for (int j = 0; j < MMO.metfiles.Count; j++) // j = number of actual Met-Station
                                    {
                                        /* TEST OETTL 3.2.17
									    richtung = (270-Convert.ToDouble(MMO.wind_direction[j][counter[j]]))*Math.PI/180;
									    u_METEO = MMO.wind_speeds[j][counter[j]]* Math.Cos(richtung);
									    v_METEO = MMO.wind_speeds[j][counter[j]]* Math.Sin(richtung);
									    wg_METEO = Math.Sqrt(u_METEO*u_METEO + v_METEO*v_METEO);
										 */

                                        //comparison is only possible when date and time are similar
                                        if (synchron[j] == true)
                                        {
                                            richtung = (270 - Convert.ToDouble(MMO.wind_direction[j][counter[j]])) * Math.PI / 180;
                                            u_METEO = MMO.wind_speeds[j][counter[j]] * Math.Cos(richtung);
                                            v_METEO = MMO.wind_speeds[j][counter[j]] * Math.Sin(richtung);
                                            wg_METEO = Math.Sqrt(u_METEO * u_METEO + v_METEO * v_METEO);

                                            //difference in wind directions between GRAMM and Observations - used as additional weighting factor

                                            double Wind_Dir_Meas = Convert.ToDouble(MMO.wind_direction[j][counter[j]]); // Wind direction from Measurement

                                            if (optimization == 2) // error using components
                                            {
                                                // error for direction
                                                err = Math.Abs(iwr[j, n] - Wind_Dir_Meas);
                                                if (err > 180)
                                                {
                                                    err = 360 - err;
                                                }

                                                err = Math.Abs(err);
                                                err = Math.Pow(Math.Max(0, (err - 12)), 1.8) * weighting_Direction[j]; // weighting factor
                                                                                                                       // error for speed - normalized
                                                err += Math.Abs(wg_GRAMM[j, n] - wg_METEO) / Math.Max(0.35, Math.Min(wg_GRAMM[j, n], wg_METEO)) * 100;
                                            }
                                            else if (optimization == 1) // error using vectors
                                            {
                                                // error using vectors
                                                err = Math.Sqrt(Math.Pow((u_GRAMM[j, n] - u_METEO) * 400, 2) +
                                                                Math.Pow((v_GRAMM[j, n] - v_METEO) * 400, 2));
                                            }

                                            // use weighting factor for met-station-error
                                            if (weighting_factor[j] >= 0)
                                            {
                                                err_st[j] = err * weighting_factor[j];
                                            }
                                            else
                                            {
                                                err_st[j] = err;
                                            }

                                            //										    double xxx;
                                            //										    // error for stability - +- 1 ak = no error - stability error not weighted
                                            if (stronger_weighted_SC1_and_7 &&
                                                MMO.stability[0][met_count] <= 2 || MMO.stability[0][met_count] >= 6) // keep original SC 6 and 7
                                            {
                                                double temp = Math.Abs(localstability[0, n] - MMO.stability[0][met_count]) * 200; // stability
                                                                                                                                  //											    xxx = temp;
                                                if (optimization == 2) // error using components
                                                {
                                                    err_st[j] += temp; // stability
                                                }
                                                else // error using vectors
                                                {
                                                    err_st[j] += Math.Sqrt(err_st[j] * err_st[j] + temp * temp);
                                                }
                                            }
                                            else
                                            {
                                                double temp = Math.Max(0, Math.Abs(localstability[0, n] - MMO.stability[0][met_count]) - 1) * 200; // stability
                                                temp += Math.Abs(localstability[0, n] - MMO.stability[0][met_count]) * 4; // little additional error. that best SCL can be found
                                                                                                                          //											    xxx = temp;
                                                if (optimization == 2) // error using components
                                                {
                                                    err_st[j] += temp; // stability
                                                }
                                                else // error using vectors
                                                {
                                                    err_st[j] += Math.Sqrt(err_st[j] * err_st[j] + temp * temp);
                                                }
                                            }
                                            //										    mess.listBox1.Items.Add(Convert.ToString(localstability[n]) + "/" + Convert.ToString(err_st[j]-xxx) + "/" + Convert.ToString(xxx));
                                            //									       	mess.Show();
                                            //									    	Application.DoEvents();
                                            //
                                            err_min = Math.Min(err_min, err_st[j]); // error min

                                        } // synchron fits
                                        else // no synchronity
                                        {
                                            if (n == 1)
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
                                                MessageInfoForm.listBox1.Items.Add("No date sync with obs. station " + (j + 1).ToString() + " at line " + counter[j].ToString() + " and orig. date " + date_station0.ToString()); // Kuntner
                                                MessageInfoForm.Show();

                                                err_st[j] = 0;
                                            }
                                        }
                                    } // Number of actual Met-Station

                                    int match_station = 0; // counter,how much stations did match
                                    err = 0;
                                    // find sum error without bad met-stations
                                    for (int j = 0; j < MMO.metfiles.Count; j++) // j = number of actual Met-Station
                                    {
                                        if (synchron[j] == true)
                                        {
                                            if ((outliers == false) || ((err_st[j] / Math.Max(0.01, weighting_factor[j])) < err_min * 2)) // don´t use bad stations if outliers = true;
                                            {
                                                err += err_st[j];
                                                match_station++;
                                            }
                                        }
                                    }

                                    if (match_station > 0)
                                    {
                                        err = err / match_station; // norm to the count of matches stations
                                    }
                                    else
                                    {
                                        err = err_st[0];
                                    }

                                    if (err < best_err) // find best fitting weather situation
                                    {
                                        best_err = err;  // error of best fitting situation
                                        best_fit = n;    // best fitting situation
                                                         //debugwr = iwr[0, n];
                                                         //debugwsp = wg_GRAMM[0,n];
                                    }
                                } // loop over calculated GRAMM - Fields

                                // write actual weather-situation of Mettimeseries.dat with the original data from meteopgt.all
                                write_mettimeseries.WriteLine(met_day + "." + met_month + "," + met_hour + ","
                                                              + Convert.ToString(wg[best_fit], ic) + "," +
                                                              Convert.ToString(nsektor[best_fit], ic) + "," +
                                                              Convert.ToString(iakla[best_fit], ic));

                                if (PGT[best_fit - 1].PGTFrq < 0)
                                {
                                    PGT[best_fit - 1].PGTFrq = 0; // marker, that situation is used
                                }

                                PGT[best_fit - 1].PGTFrq = PGT[best_fit - 1].PGTFrq + (double)1000d / MMO.filelength[0];
                                PGT[best_fit - 1].PGTNumber = best_fit; // original number of weather situation

                                // Compute error values for the Match-Fine tuning
                                for (int j = 0; j < MMO.metfiles.Count; j++) // j = number of actual Met-Station
                                {
                                    richtung = (270 - Convert.ToDouble(MMO.wind_direction[j][counter[j]])) * Math.PI / 180;
                                    u_METEO = MMO.wind_speeds[j][counter[j]] * Math.Cos(richtung);
                                    v_METEO = MMO.wind_speeds[j][counter[j]] * Math.Sin(richtung);
                                    wg_METEO = Math.Sqrt(u_METEO * u_METEO + v_METEO * v_METEO);

                                    double err = Math.Sqrt(Math.Pow((u_GRAMM[j, best_fit] - u_METEO), 2) + Math.Pow((v_GRAMM[j, best_fit] - v_METEO), 2));
                                    //double wg_Matched = Math.Sqrt(Math.Pow(u_GRAMM[j, best_fit], 2) + Math.Pow(v_GRAMM[j, best_fit], 2));

                                    //double err = Math.Abs(wg_METEO - wg_Matched);

                                    //evaluation of error values for user feedback
                                    double errp = 0;
                                    if (wg_METEO > 0.5)
                                    {
                                        errp = err / wg_METEO;
                                    }
                                    else
                                    {
                                        errp = err / 0.5;
                                    }

                                    if (errp <= 0.1)
                                    {
                                        Vector_Error_Sum[j, 0]++;
                                    }

                                    if (errp <= 0.2)
                                    {
                                        Vector_Error_Sum[j, 1]++;
                                    }

                                    if (errp <= 0.4)
                                    {
                                        Vector_Error_Sum[j, 2]++;
                                    }

                                    if (errp <= 0.6)
                                    {
                                        Vector_Error_Sum[j, 3]++;
                                    }

                                    err = Math.Abs(localstability[j, best_fit] - MMO.stability[j][met_count]);
                                    if (err == 0)
                                    {
                                        SC_Error_Sum[j, 0]++;
                                    }

                                    if (err <= 1)
                                    {
                                        SC_Error_Sum[j, 1]++;
                                    }
                                }
                                //write_debug.WriteLine(Convert.ToString(best_fit)+"/"+Convert.ToString((double) 1000/MMO.filelength[0]));
                                //write_debug.WriteLine(Convert.ToString(PGT[best_fit-1].PGT_Number)+"/"+Convert.ToString(PGT[best_fit-1].PGT_FRQ));
                                //write_debug.WriteLine(met_day + "." + met_month + "," + met_hour + "," + Convert.ToString(Math.Round(debugwsp,1)).Replace(decsep, ".") +"," + Convert.ToString(Math.Round(debugwr,1)).Replace(decsep, "."));
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
                                MessageInfoForm.listBox1.Items.Add("Error when computing wind deviations Met-File line" + Convert.ToString(met_count)); // Kuntner
                                MessageInfoForm.Show();
                            }

                        } // Problem with time stamp of station 0
                    } // loop over all weather situations "met_count"
                    write_mettimeseries.Close();
                    write_mettimeseries.Dispose();
                    //write_debug.Close();

                    MMO.Match_Mode = 1;                 // tune match process
                    MMO.Match_Methode = optimization;   // vectorial or component
                    MMO.Remove_Outliers = outliers;     // Remove outliers on/off
                    wait.Hide();

                    for (int j = 0; j < MMO.metfiles.Count; j++) // write error values to MMO.datagrid
                    {
                        for (int i = 0; i < 4; i++) // 4 values
                        {
                            MMO.dataGridView1.Rows[j].Cells[8 + i].Value = (100 * Vector_Error_Sum[j, i] / MMO.filelength[0]).ToString();
                        }

                        // error values for SC
                        MMO.dataGridView1.Rows[j].Cells[12].Value = (100 * SC_Error_Sum[j, 0] / MMO.filelength[0]).ToString();
                        MMO.dataGridView1.Rows[j].Cells[13].Value = (100 * SC_Error_Sum[j, 1] / MMO.filelength[0]).ToString();
                    }

#if __MonoCS__
					MMO.Dialog_Message = -1;
					MMO.Show();
					// Kuntner: this message loop is needed because of the MONO ShowDialog() bug
					do
					{
						Application.DoEvents ();
					}
					while(MMO.Dialog_Message == -1);

					MMO.Hide ();
#else
                    MMO.ShowDialog();
#endif
                }
                while (MMO.Dialog_Message == 0); // Show Match Dialog and wait until process is finished

                if (MMO.StartMatch) // otherwise cancel matching process
                {

                    //________________________________________________________________________________________________________________
                    wait.Hide();
                    wait.Text = "Writing meteopgt.all";
                    wait.Show();
                    Application.DoEvents(); // Kuntner

                    // now create a new meteopgt.all
                    // first sort the PGT class
                    PGT.Sort();
                    PGT.Reverse();

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

                            for (int rep = 1; rep < PGT.Count; rep++) // check all fitted situations
                            {
                                if (PGT[rep].PGTFrq <= (double)concatenation && PGT[rep].PGTFrq > 0) //just some fitting hours - search similar situation
                                {
                                    string[] replace = PGT[rep].PGTString.Split(new char[] { ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                    // search for fitting string
                                    for (int ori = 1; ori < rep; ori++) // check all fitted situations < ori
                                    {
                                        if (PGT[ori].PGTFrq > 0) // do not compare unused situation
                                        {
                                            string[] original = PGT[ori].PGTString.Split(new char[] { ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                                            if (original.Length > 2 && replace.Length > 2 && replace[1] == original[1] && replace[2] == original[2]) // speed & SC fits
                                            {
                                                if (Math.Abs(St_F.TxtToDbl(replace[0], false) - St_F.TxtToDbl(original[0], false)) < 2.5) //+- 25 Degrees
                                                {
                                                    if (ori != rep) // dont copy&delete itself
                                                    {
                                                        PGT[ori].PGTFrq = PGT[ori].PGTFrq + PGT[rep].PGTFrq; // increase this situation
                                                        PGT[rep].PGTFrq = -1; // delete this situation

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
                        PGT.Sort();
                        PGT.Reverse();
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
                                string[] text1 = Meteo_orig_Header[0].Split(new char[] { ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
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
                                if (temp.Length >= Meteo_orig_Header[0].Length)
                                {
                                    Meteo_orig_Header[0] = temp;
                                }

                                myWriter.WriteLine(Meteo_orig_Header[0]);
                                myWriter.WriteLine(Meteo_orig_Header[1]);
                                foreach (GralData.PGTAll a_Line in PGT)
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
                        foreach (GralData.PGTAll a_Line in PGT)
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
                        MMOData.AnemometerHeight = Math.Max(1, Math.Max(MMOData.AnemometerHeight, schnitt[0]));
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
                }
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

            MMO.StartMatch = false; // set flag, that match process is finished
        }

        private void MessageFormClosed(object sender, EventArgs e)
        {
            MessageInfoForm = null;
        }
    }
}
