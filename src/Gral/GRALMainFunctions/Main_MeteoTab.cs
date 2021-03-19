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
 * Date: 16.01.2019
 * Time: 20:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using GralData;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
    {
        /// <summary>
        /// Open and read meteorological data from file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenMeteoFile(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Met files (*.met)|*.met|DWD (*.akterm;*.akt)|*.akterm;*.akt",
                Title = "Select meteorological data"
            })
            {
                if (Directory.Exists(MeteoDirectory))
                {
                    dialog.InitialDirectory = MeteoDirectory;
                 }
                else
                {
                    dialog.InitialDirectory = Path.Combine(ProjectName, @"Metfiles");
                }

                //dialog.InitialDirectory = projectname;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    MetfileName = dialog.FileName;
                    MeteoDirectory = Path.GetDirectoryName(dialog.FileName);
                    //metfileext = Path.GetExtension(metfile);
                }
                else
                {
                    return;
                }
            }

            for (int i = 1; i < WindSpeedClasses; i++)
            {
                RemoveTextbox(i);
            }
            groupBox1.Visible = false;
            groupBox2.Visible = false;
            checkBox19.Visible = false;
            groupBox3.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            ChangeButtonLabel(ButtonColorEnum.ButtonMeteo, ButtonColorEnum.RedDot); // meteo button red

            groupBox23.Visible = false; // Anemometer heigth
            label100.Visible = false;
            groupBox20.Visible = false;

            ChangedWindData = false;
            int filelength = 0;

            //show file name in listbox
            listBox1.Items.Clear();
            if (Path.GetDirectoryName(MetfileName).Length > 85)
            {
                listBox1.Items.Add(MetfileName.Substring(0, 85) + "...." + Path.DirectorySeparatorChar +
                                Path.GetFileName(MetfileName));
            }
            else
            {
                listBox1.Items.Add(MetfileName);
            }

            try
            {
                using (StreamReader streamreader = new StreamReader(MetfileName))
                {
                    string reihe;

                    //Evaluating the length of the file
                    if (Path.GetExtension(MetfileName).ToLower() == ".met")
                    {
                        while (streamreader.EndOfStream == false)
                        {
                            reihe = streamreader.ReadLine();
                            int ret;
                            if (Int32.TryParse(reihe.Substring(0, 1), out ret) == true)
                            {
                                filelength += 1;
                            }
                        }
                    }
                    else if (Path.GetExtension(MetfileName).ToLower() == ".akterm")
                    {
                        string[] text = new string[50];

                        while (streamreader.EndOfStream == false)
                        {
                            reihe = streamreader.ReadLine();
                            text = reihe.Split(new char[] { ' ', '\t' });
                            if (text[0] == "AK")
                            {
                                text = reihe.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                if ((text[7] != "9") && (text[8] != "9") && (text[12] != "7") && (text[12] != "9"))
                                {
                                    filelength += 1;
                                }
                            }
                            if (text[0] == "+")
                            {
                                try
                                {
                                    text = reihe.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                    //at this stage it is not allowed to delete existing meteoinput-data
                                    //numericUpDown7.Value = Convert.ToDecimal(text[10]) / 10;
                                    Anemometerheight = Convert.ToDouble(text[10]) / 10;
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                    else if (Path.GetExtension(MetfileName).ToLower() == ".akt")
                    {
                        while (streamreader.EndOfStream == false)
                        {
                            reihe = streamreader.ReadLine();
                            filelength += 1;
                        }
                        //at this stage it is not allowed to delete existing meteoinput-data
                        //numericUpDown7.Value = 10; // Anemometer height
                    }
                }

                //MessageBox.Show(filelength.ToString());
                GralMainForms.MeteoInput FormatMetFile = new GralMainForms.MeteoInput(this)
                {
                    MetFile1 = MetfileName,
                    FfileLength1 = filelength,
                    DecSep1 = DecimalSep,
                    Owner = this,
                    Met_Time_Ser = MeteoTimeSeries
                };
                FormatMetFile.Show();
            }
            catch
            {
                MessageBox.Show("Unable to open selected met-file.");
            }

            //enable/disable GRAL simulations
            Enable_GRAL();
            //enable/disable GRAMM simulations
            Enable_GRAMM();
        }

        /// <summary>
        /// Compute and show wind rose velocity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowWindRoseVelocity(object sender, EventArgs e)
        {
            int WindSectCount = 16;
            double[] wndclasses = new double[7] { 0.5, 1, 2, 3, 4, 5, 6 };
            int count = 0;
            int startstunde = 0;
            int endstunden = 23;
            bool biascorrection = true;
            List<WindData> wind = new List<WindData>();

            using (GralMainForms.MeteoSelectTimeInterval mts = new GralMainForms.MeteoSelectTimeInterval
            {
                WindRoseSet = WindroseSetting,
                StartPosition = FormStartPosition.Manual,
                Left = this.Left + 20,
                Top = this.Top + 150
            })
            {
                if (mts.ShowDialog() == DialogResult.OK)
                {
                    startstunde = mts.WindRoseSet.StartStunde;
                    endstunden = mts.WindRoseSet.EndStunde;
                    int maxwind = mts.WindRoseSet.MaxVelocity;
                    biascorrection = mts.WindRoseSet.BiasCorrection;
                    WindSectCount = mts.WindRoseSet.SectorCount;
                    double[,] sectFrequency = new double[WindSectCount, 8];
                    bool ignore00Values = mts.WindRoseSet.Ignore00Values;

                    float sectorWidth = 1;
                    if (biascorrection)
                    {
                        sectorWidth = GralStaticFunctions.GetMetFileSectionWidth.GetMetSectionWidth(MeteoTimeSeries);
                    }
                    if (startstunde == endstunden)
                    {
                        MessageBox.Show("Start- and endtime must be different values");
                    }
                    else
                    {
                        if (maxwind <= 6)
                        {
                            wndclasses = new double[] { 0.5, 1, 2, 3, 4, 5, 6 };
                        }
                        else if (maxwind <= 7)
                        {
                            wndclasses = new double[] { 0.5, 1, 2, 3, 4, 5, 7 };
                        }
                        else if (maxwind <= 8)
                        {
                            wndclasses = new double[] { 0.5, 1, 2, 3, 4, 6, 8 };
                        }
                        else if (maxwind <= 10)
                        {
                            wndclasses = new double[] { 0.5, 1, 2, 3, 4, 7, 10 };
                        }
                        else if (maxwind <= 14)
                        {
                            wndclasses = new double[] { 0.5, 1, 2, 4, 7, 10, 14 };
                        }
                        else if (maxwind <= 20)
                        {
                            wndclasses = new double[] { 0.5, 1, 2, 4, 8, 12, 20 };
                        }

                        foreach (WindData data in MeteoTimeSeries)
                        {
                            //wind rose for a certain time interval within a day
                            if (((startstunde < endstunden) && (data.Hour >= startstunde) && (data.Hour <= endstunden)) ||
                                ((startstunde > endstunden) && ((data.Hour >= startstunde) || (data.Hour <= endstunden))))
                            {
                                if (ignore00Values == false || data.Vel > 0.000001 || data.Dir > 0.000001)
                                {
                                    double SectAngle = 360D / WindSectCount;
                                    int sektor = (int)(Math.Round(data.Dir / SectAngle, 0));
                                    int wklass = 0; //Convert.ToInt32(Math.Truncate(windge[i])) + 1;

                                    for (int c = 0; c < 6; c++)
                                    {
                                        if (data.Vel > wndclasses[c] && data.Vel <= wndclasses[c + 1])
                                        {
                                            wklass = c + 1;
                                        }
                                    }

                                    if (data.Vel <= wndclasses[0])
                                    {
                                        wklass = 0;
                                    }

                                    if (data.Vel > wndclasses[6])
                                    {
                                        wklass = 7;
                                    }

                                    if (sektor > WindSectCount - 1)
                                    {
                                        sektor = 0;
                                    }

                                    if (biascorrection && sectorWidth > 1)
                                    {
                                        double start = data.Dir - sectorWidth * 0.49F;
                                        double ende = data.Dir + sectorWidth * 0.49F;
                                        double inc = 0.5;
                                        if (WindSectCount == 24)
                                        {
                                            inc = 0.1;
                                        }
                                        else if (WindSectCount == 32)
                                        {
                                            inc = 0.03;
                                        }

                                        for (double subsect = start; subsect < ende; subsect += inc)
                                        {
                                            double _sect = subsect;
                                            if (_sect < 0)
                                            {
                                                _sect += 360;
                                            }
                                            if (_sect > 360)
                                            {
                                                _sect -= 360;
                                            }
                                            sektor = (int)(_sect / SectAngle);
                                            if (sektor >= 0 && sektor < WindSectCount)
                                            {
                                                count++;
                                                sectFrequency[sektor, wklass]++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        count++;
                                        sectFrequency[sektor, wklass]++;
                                    }
                                    wind.Add(data);
                                }
                            }
                        }

                        for (int sektor = 0; sektor < WindSectCount; sektor++)
                        {
                            for (int wklass = 0; wklass < 8; wklass++)
                            {
                                sectFrequency[sektor, wklass] = sectFrequency[sektor, wklass] / Convert.ToDouble(count);
                            }
                        }

                        GralMainForms.Windrose windrose = new GralMainForms.Windrose
                        {
                            SectFrequ = sectFrequency,
                            MetFileName = Path.GetFileName(MetfileName),
                            WindData = wind,
                            StartHour = startstunde,
                            FinalHour = endstunden,
                            WndClasses = wndclasses,
                            DrawFrames = mts.WindRoseSet.ShowFrames,
                            SmallSectors = mts.WindRoseSet.DrawSmallSectors,
                            DrawingMode = 0,
                            WindSectorCount = WindSectCount
                        };
                        if (biascorrection)
                        {
                            windrose.BiasCorrection = 2;
                            if (sectorWidth > 1)
                            {
                                windrose.BiasCorrection = 1;
                            }
                        }

                        if (wind.Count > 1)
                        {
                            windrose.Show();
                        }
                        else
                        {
                            MessageBox.Show(this, "No meteo data available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// compute windrose combined with stability classes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowWindRoseStability(object sender, EventArgs e)
        {
            int WindSectCount = 16;
            int count = 0;
            int startstunde = 0;
            int endstunden = 23;
            
            List<WindData> wind = new List<WindData>();
            int maxvelocity = WindroseSetting.MaxVelocity;
            WindroseSetting.MaxVelocity = 0;
            
            using (GralMainForms.MeteoSelectTimeInterval mts = new GralMainForms.MeteoSelectTimeInterval
            {
                WindRoseSet = WindroseSetting,
                StartPosition = FormStartPosition.Manual,
                Left = this.Left + 20,
                Top = this.Top + 150
            })
            {
                if (mts.ShowDialog() == DialogResult.OK)
                {
                    WindSectCount = mts.WindRoseSet.SectorCount;
                    double[,] sectFrequency = new double[WindSectCount, 8];
                    startstunde = mts.WindRoseSet.StartStunde;
                    endstunden = mts.WindRoseSet.EndStunde;
                    bool biascorrection = mts.WindRoseSet.BiasCorrection;
                    float sectorWidth = 1;
                    bool ignore00Values = mts.WindRoseSet.Ignore00Values;
                    if (biascorrection)
                    {
                        sectorWidth = GralStaticFunctions.GetMetFileSectionWidth.GetMetSectionWidth(MeteoTimeSeries);
                    }

                    if (startstunde == endstunden)
                    {
                        MessageBox.Show("Start- and endtime must be different values");
                    }
                    else
                    {
                        foreach (WindData data in MeteoTimeSeries)
                        {
                            //wind rose for a certain time interval within a day
                            if (((startstunde < endstunden) && (data.Hour >= startstunde) && (data.Hour <= endstunden)) ||
                                ((startstunde > endstunden) && ((data.Hour >= startstunde) || (data.Hour <= endstunden))))
                            {
                                if (ignore00Values == false || data.Vel > 0.000001 || data.Dir > 0.000001)
                                {
                                    double SectAngle = 360D / WindSectCount;
                                    int sektor = (int)(Math.Round(data.Dir / SectAngle, 0));

                                    if (sektor > WindSectCount - 1)
                                    {
                                        sektor = 0;
                                    }

                                    if (biascorrection && sectorWidth > 1)
                                    {
                                        double start = data.Dir - sectorWidth * 0.49F;
                                        double ende = data.Dir + sectorWidth * 0.49F;
                                        double inc = 0.5;
                                        if (WindSectCount == 24)
                                        {
                                            inc = 0.1;
                                        }
                                        else if (WindSectCount == 32)
                                        {
                                            inc = 0.03;
                                        }

                                        for (double subsect = start; subsect < ende; subsect += inc)
                                        {
                                            double _sect = subsect;
                                            if (_sect < 0)
                                            {
                                                _sect += 360;
                                            }
                                            if (_sect > 360)
                                            {
                                                _sect -= 360;
                                            }
                                            sektor = (int)(_sect / SectAngle);
                                            if (sektor >= 0 && sektor < WindSectCount)
                                            {
                                                count++;
                                                sectFrequency[sektor, data.StabClass]++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        count++;
                                        sectFrequency[sektor, data.StabClass]++;
                                    }
                                    wind.Add(data);
                                }
                            }
                        }

                        for (int sektor = 0; sektor < WindSectCount; sektor++)
                        {
                            for (int aklass = 1; aklass < 8; aklass++)
                            {
                                sectFrequency[sektor, aklass] = sectFrequency[sektor, aklass] / Convert.ToDouble(count);
                            }
                        }

                        GralMainForms.Windrose windrose = new GralMainForms.Windrose()
                        {
                            StartPosition = FormStartPosition.Manual,
                            Location = new System.Drawing.Point(this.Left, this.Top),
                            SectFrequ = sectFrequency,
                            MetFileName = Path.GetFileName(MetfileName),
                            WindData = wind,
                            StartHour = startstunde,
                            FinalHour = endstunden,
                            DrawFrames = mts.WindRoseSet.ShowFrames,
                            SmallSectors = mts.WindRoseSet.DrawSmallSectors,
                            DrawingMode = 1,
                            WindSectorCount = WindSectCount
                        };

                        if (biascorrection)
                        {
                            windrose.BiasCorrection = 2;
                            if (sectorWidth > 1)
                            {
                                windrose.BiasCorrection = 1;
                            }
                        }
                        if (wind.Count > 1)
                        {
                            windrose.Show();
                        }
                        else
                        {
                            MessageBox.Show(this, "No meteo data available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            WindroseSetting.MaxVelocity = maxvelocity;
        }

        /// <summary>
        /// wind veolcity classes view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowWindVelocityClasses(object sender, EventArgs e)
        {
            int count = 0;
            int startstunde = 0;
            int endstunden = 23;
            int maxwind = 6;
            List<WindData> wind = new List<WindData>();
            bool showBias = WindroseSetting.ShowBias;
            WindroseSetting.ShowBias = false;

            using (GralMainForms.MeteoSelectTimeInterval mts = new GralMainForms.MeteoSelectTimeInterval
            {
                WindRoseSet = WindroseSetting,
                StartPosition = FormStartPosition.Manual,
                Left = this.Left + 20,
                Top = this.Top + 150,
                
            })
            {
                if (mts.ShowDialog() == DialogResult.OK)
                {
                    startstunde = mts.WindRoseSet.StartStunde;
                    endstunden = mts.WindRoseSet.EndStunde;
                    maxwind = mts.WindRoseSet.MaxVelocity;

                    maxwind++;
                    double[] wclassFrequency = new double[maxwind + 1];

                    if (startstunde == endstunden)
                    {
                        MessageBox.Show("Start- and endtime must be different values");
                    }
                    else
                    {
                        foreach (WindData data in MeteoTimeSeries)
                        {
                            //wind rose for a certain time interval within a day
                            if (((startstunde < endstunden) && (data.Hour >= startstunde) && (data.Hour <= endstunden)) ||
                                ((startstunde > endstunden) && ((data.Hour >= startstunde) || (data.Hour <= endstunden))))
                            {
                                //compute wind classes
                                int wklass = Convert.ToInt32(Math.Truncate(data.Vel)) + 1;
                                if (data.Vel <= 1.0)
                                {
                                    wklass = 1;
                                    if (data.Vel <= 0.5)
                                    {
                                        wklass = 0;
                                    }
                                }
                                if (data.Vel > maxwind - 1.0)
                                {
                                    wklass = maxwind;
                                }

                                wclassFrequency[wklass] = wclassFrequency[wklass] + 1;
                                count += 1;
                                wind.Add(data);
                            }
                        }
                        //compute percent values
                        for (int i = 0; i < (maxwind + 1); i++)
                        {
                            wclassFrequency[i] = wclassFrequency[i] / Convert.ToDouble(count);
                        }

                        GralMainForms.Windclasses wclass = new GralMainForms.Windclasses()
                        {
                            StartPosition = FormStartPosition.Manual,
                            Location = new System.Drawing.Point(this.Left, this.Top),
                            WClassFrequency = wclassFrequency,
                            MetFile = Path.GetFileName(MetfileName),
                            Wind = wind,
                            StartHour = startstunde,
                            FinalHour = endstunden
                        };
                        if (wind.Count > 1)
                        {
                            wclass.Show();
                        }
                        else
                        {
                            MessageBox.Show(this, "No meteo data available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information); 
                        }
                    }
                }
            }
            WindroseSetting.ShowBias = showBias;
        }

        /// <summary>
        /// wind veolcity distribution view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowWindVelocityDistribution(object sender, EventArgs e)
        {
            int count = 0;
            int startstunde = 0;
            int endstunden = 23;
            int maxwind = 10;
            List<WindData> wind = new List<WindData>();
            bool showBias = WindroseSetting.ShowBias;
            WindroseSetting.ShowBias = false;

            using (GralMainForms.MeteoSelectTimeInterval mts = new GralMainForms.MeteoSelectTimeInterval
            {
                WindRoseSet = WindroseSetting,
                StartPosition = FormStartPosition.Manual,
                Left = this.Left + 20,
                Top = this.Top + 150,

            })
            {
                if (mts.ShowDialog() == DialogResult.OK)
                {
                    startstunde = mts.WindRoseSet.StartStunde;
                    endstunden = mts.WindRoseSet.EndStunde;
                    maxwind = mts.WindRoseSet.MaxVelocity;
                    double meanwind = 0;
                    maxwind++;
                    double[] wclassFrequency = new double[(maxwind + 1) * 4];

                    if (startstunde == endstunden)
                    {
                        MessageBox.Show("Start- and endtime must be different values");
                    }
                    else
                    {
                        foreach (WindData data in MeteoTimeSeries)
                        {
                            //wind rose for a certain time interval within a day
                            if (((startstunde < endstunden) && (data.Hour >= startstunde) && (data.Hour <= endstunden)) ||
                                ((startstunde > endstunden) && ((data.Hour >= startstunde) || (data.Hour <= endstunden))))
                            {
                                //compute wind classes
                                int j = 0;

                                for (double i = 0; i < maxwind; i += 0.25)
                                {
                                    if (data.Vel >= i)
                                    {
                                        wclassFrequency[j]++;
                                    }
                                    j++;
                                }
                                meanwind += data.Vel;
                                count += 1;
                            }
                        }

                        if (count > 0)
                        {
                            meanwind /= count;
                            //compute percent values
                            for (int i = 0; i < (maxwind + 1) * 4; i++)
                            {
                                wclassFrequency[i] = wclassFrequency[i] / Convert.ToDouble(count);
                            }

                            GralMainForms.WindDistribution wDistr = new GralMainForms.WindDistribution()
                            {
                                StartPosition = FormStartPosition.Manual,
                                Location = new System.Drawing.Point(this.Left, this.Top),
                                WClassFrequency = wclassFrequency,
                                MetFile = Path.GetFileName(MetfileName),
                                MaxWind = maxwind,
                                StartHour = startstunde,
                                FinalHour = endstunden,
                                MeanWindSpeed = meanwind,
                                StartDate = MeteoTimeSeries[0].Date,
                                EndDate = MeteoTimeSeries[MeteoTimeSeries.Count - 1].Date
                            };

                            wDistr.Show();
                        }
                        else
                        {
                            MessageBox.Show(this, "No meteo data available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// show stability classes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowWindStabilityClasses(object sender, EventArgs e)
        {
            double[] sclassFrequency = new double[7];

            int count = 0;
            int startstunde = 0;
            int endstunden = 23;
            
            List<WindData> wind = new List<WindData>();
            int maxvelocity = WindroseSetting.MaxVelocity;
            bool showBias = WindroseSetting.ShowBias;
            WindroseSetting.ShowBias = false;
            WindroseSetting.MaxVelocity = 0;

            using (GralMainForms.MeteoSelectTimeInterval mts = new GralMainForms.MeteoSelectTimeInterval
            {
                WindRoseSet = WindroseSetting,
                StartPosition = FormStartPosition.Manual,
                Left = this.Left + 20,
                Top = this.Top + 150
            })
            {
                if (mts.ShowDialog() == DialogResult.OK)
                {
                    startstunde = mts.WindRoseSet.StartStunde;
                    endstunden = mts.WindRoseSet.EndStunde;
                   
                    if (startstunde == endstunden)
                    {
                        MessageBox.Show("Start- and endtime must be different values");
                    }
                    else
                    {
                        //for (int i = 0; i <= filelength - 1; i++)
                        foreach (WindData data in MeteoTimeSeries)
                        {
                            //wind rose for a certain time interval within a day
                            if (((startstunde < endstunden) && (data.Hour >= startstunde) && (data.Hour <= endstunden)) ||
                                ((startstunde > endstunden) && ((data.Hour >= startstunde) || (data.Hour <= endstunden))))
                            {
                                //compute stability classes frequency
                                int index = data.StabClass - 1;
                                sclassFrequency[index] = sclassFrequency[index] + 1;
                                count += 1;
                                wind.Add(data);
                            }
                        }
                        //compute percent values
                        for (int i = 0; i < 7; i++)
                        {
                            sclassFrequency[i] = sclassFrequency[i] / Convert.ToDouble(count);
                        }

                        GralMainForms.Stabilityclasses sclass = new GralMainForms.Stabilityclasses()
                        {
                            StartPosition = FormStartPosition.Manual,
                            Location = new System.Drawing.Point(this.Left, this.Top),
                            ScClassFrequency = sclassFrequency,
                            MetFile = Path.GetFileName(MetfileName),
                            Wind = wind
                        };
                        if (MeteoTimeSeries.Count > 1)
                        {
                            sclass.Show();
                        }
                        else
                        {
                            MessageBox.Show(this, "No meteo data available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            WindroseSetting.ShowBias = showBias;
            WindroseSetting.MaxVelocity = maxvelocity;
        }

        /// <summary>
        /// Show diurnal mean wind velocity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowWindVelocityMean(object sender, EventArgs e)
        {
            double[] meanwind = new double[24];
            double[] anzstunden = new double[24];

            //compute mean wind speed for each hour
            //for (int i = 0; i <= filelength - 1; i++)
            foreach (WindData data in MeteoTimeSeries)
            {
                int hour = data.Hour;
                if (hour == 24)
                {
                    hour = 0;
                }

                meanwind[hour] = meanwind[hour] + data.Vel;
                anzstunden[hour] += 1;
            }

            for (int i = 0; i < 24; i++)
            {
                if (anzstunden[i] > 0)
                {
                    meanwind[i] = meanwind[i] / anzstunden[i];
                }
                else
                {
                    meanwind[i] = 0;
                }
            }

            GralMainForms.DiurnalWindspeed mwind = new GralMainForms.DiurnalWindspeed()
            {
                StartPosition = FormStartPosition.Manual,
                Location = new System.Drawing.Point(this.Left, this.Top),
                meanwind = meanwind,
                metfile = Path.GetFileName(MetfileName),
                wind = MeteoTimeSeries
            };
            if (MeteoTimeSeries.Count > 1)
            {
                mwind.Show();
            }
            else
            {
                MessageBox.Show(this, "No meteo data available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Show diurnal frequency of wind direction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowWindDirectionFrequency(object sender, EventArgs e)
        {
            double[,] meanwinddir = new double[24, 16];
            double[] anzstunden = new double[24];

            //compute mean wind direction frequency for each hour
            foreach (WindData data in MeteoTimeSeries)
            {
                int hour = data.Hour;
                if (hour == 24)
                {
                    hour = 0;
                }

                int sektor = Convert.ToInt32(data.Dir / 22.5);
                if (sektor > 15)
                {
                    sektor = 0;
                }

                meanwinddir[hour, sektor] += 1;
                anzstunden[hour] += 1;
            }
            for (int i = 0; i < 24; i++)
            {
                for (int n = 0; n < 16; n++)
                {
                    if (anzstunden[i] > 0)
                    {
                        meanwinddir[i, n] = meanwinddir[i, n] / anzstunden[i];
                    }
                    else
                    {
                        meanwinddir[i, n] = 0;
                    }
                }
            }

            GralMainForms.DiurnalWinddirections mwinddir = new GralMainForms.DiurnalWinddirections()
            {
                StartPosition = FormStartPosition.Manual,
                Location = new System.Drawing.Point(this.Left, this.Top),
                meanwinddir = meanwinddir,
                metfile = Path.GetFileName(MetfileName),
                wind = MeteoTimeSeries
            };
            if (MeteoTimeSeries.Count > 1)
            {
                mwinddir.Show();
            }
            else
            {
                MessageBox.Show(this, "No meteo data available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Meteorology Classification/Default
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClassifiyMeteoData(object sender, EventArgs e)
        {
            // Kuntner - Warnabfrage
            if (Project_Locked == true)
            {
                ProjectLockedMessage(); // Project locked!
                return;
            }
            if (GRAMM_Locked == true)
            {
                GRAMMLockedMessage(); // Project locked! - do not save any changes!
                return;
            }

            DialogResult result = DialogResult.Yes;
            /*
            if (listBox2.Items.Count != 0)
            {
                MessageBox.Show("You are using a GRAMM windfield and can't create new a new met-file","Create new met-files?", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
             */

            if (File.Exists(Path.Combine(ProjectName, @"Computation", "meteopgt.all")))
            {
                result = MessageBox.Show("Do you really want to overwrite the existing meteorological files?", "Create new met-files?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            }

            if (result == DialogResult.Yes)
            {
                //label and box for anemometer height
                groupBox23.Visible = true;

                //checkbox for time series
                checkBox19.Visible = true;
                //classification of meteorological data
                groupBox2.Visible = true;
                groupBox3.Visible = true;
                button7.Visible = true;
                ChangeButtonLabel(ButtonColorEnum.ButtonMeteo, ButtonColorEnum.RedDot); // meteo button red

                label100.Visible = true;
                groupBox20.Visible = true;
                ChangedWindData = true;
                for (int i = 0; i < WindSpeedClasses; i++)
                {
                    RemoveTextbox(i);
                }
                //make sure that value in numericUpDown2 is changed and corresponding event is activated
                WindSpeedClasses = 4;
                numericUpDown2.Value = 4;
                WindSpeedClasses = 5;
                numericUpDown2.Value = 5;
                numericUpDown1.Value = 36;
                //fill values in text boxes
                TBox[0].Text = "0";
                NumUpDown[0].Value = 0.5M;
                NumUpDown[1].Value = 1.0M;
                NumUpDown[2].Value = 2.0M;
                NumUpDown[3].Value = 3.0M;

                for (int i = 1; i < WindSpeedClasses; i++)
                {
                    TBox[i].Text = NumUpDown[i - 1].Value.ToString();
                }

                TBox[WindSpeedClasses - 1].Text = NumUpDown[WindSpeedClasses - 2].Value.ToString();

                // Delete GRAMIn.dat
                try
                {
                    File.Delete(Path.Combine(ProjectName, @"Computation", "GRAMMin.dat"));
                }
                catch
                { }
                //enable/disable GRAL simulations
                Enable_GRAL();
                //enable/disable GRAMM simulations
                Enable_GRAMM();

            }
        }

        /// <summary>
        /// Set angle for classified meteorology
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeteoSectorAngleSet(object sender, EventArgs e)
        {
            WindSectorAngle = 3600 / Convert.ToInt32(numericUpDown1.Value);
            textBox1.Text = Convert.ToString(WindSectorAngle / 10);
            //reset the button to generate meteorology
            if (EmifileReset == true)
            {
                ChangedWindData = true;
                Set_met_button(ChangedWindData);
                string newPath = Path.Combine(ProjectName, @"Computation", "meteopgt.all");
                File.Delete(newPath);
                Textbox16_Set("");
                newPath = Path.Combine(ProjectName, @"Computation", "mettimeseries.dat");
                File.Delete(newPath);
            }
        }

        private void Set_met_button(bool changedwind)
        {
            if (changedwind)
            {
                button7.Visible = true;
                ChangeButtonLabel(ButtonColorEnum.ButtonMeteo, ButtonColorEnum.RedDot); // meteo button red
            }
        }

        /// <summary>
        /// Set number of velocity classes for classified meteorology
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeteoVelocityClassesCount(object sender, EventArgs e)
        {
            for (int i = 0; i < WindSpeedClasses; i++)
            {
                RemoveTextbox(i);
            }
            //remove last box and labels
            groupBox2.Controls.Remove(TBox[WindSpeedClasses - 1]);
            groupBox2.Controls.Remove(LbTbox[WindSpeedClasses - 1]);
            groupBox2.Controls.Remove(LbTbox2[WindSpeedClasses - 1]);

            WindSpeedClasses = Convert.ToInt32(numericUpDown2.Value);

            for (int i = 0; i < WindSpeedClasses - 1; i++)
            {
                CreateTextbox(9, 65 + i * 30, 53, 22, i);
            }
            //create last box and labels
            TBox[WindSpeedClasses - 1] = new TextBox
            {
                Location = new System.Drawing.Point(9 + 105, 65 + (WindSpeedClasses - 1) * 30),
                Size = new System.Drawing.Size(53, 22),
                ReadOnly = true,
                TextAlign = System.Windows.Forms.HorizontalAlignment.Right,
                Enabled = false // Kuntner
            };

            groupBox2.Controls.Add(TBox[WindSpeedClasses - 1]);

            LbTbox[WindSpeedClasses - 1] = new Label
            {
                AutoSize = true,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Location = new System.Drawing.Point(9 + 80, 65 + 3 + (WindSpeedClasses - 1) * 30),
                Size = new System.Drawing.Size(46, 16),
                Text = ">"
            };
            groupBox2.Controls.Add(LbTbox[WindSpeedClasses - 1]);

            LbTbox2[WindSpeedClasses - 1] = new Label
            {
                AutoSize = true,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Location = new System.Drawing.Point(9 + 160, 65 + 3 + (WindSpeedClasses - 1) * 30),
                Size = new System.Drawing.Size(46, 16),
                Text = "m/s"
            };
            groupBox2.Controls.Add(LbTbox2[WindSpeedClasses - 1]);

            //add values in text boxes
            TBox[0].Text = "0";
            TBox[WindSpeedClasses - 1].Text = NumUpDown[WindSpeedClasses - 2].Value.ToString();
            if (WindSpeedClasses > 2) // set standard values
            {
                for (int i = 0; i < WindSpeedClasses - 1; i++)
                {
                    if (i == 0)
                    {
                        NumUpDown[i].Value = 0.5M;
                    }
                    else
                    {
                        TBox[i].Text = NumUpDown[i - 1].Text;
                        NumUpDown[i].Value = (decimal)i;
                        if (i > 4)
                        {
                            NumUpDown[i].Value = (decimal)(i + (i - 4) * 2 - 1);
                        }
                    }
                }
                TBox[WindSpeedClasses - 1].Text = NumUpDown[WindSpeedClasses - 2].Value.ToString();
            }
            //reset the button to generate meteorology
            if (EmifileReset == true)
            {
                ChangedWindData = true;
                Set_met_button(ChangedWindData);
                string newPath = Path.Combine(ProjectName, @"Computation", "meteopgt.all");
                File.Delete(newPath);
                Textbox16_Set("");
                newPath = Path.Combine(ProjectName, @"Computation", "mettimeseries.dat");
                File.Delete(newPath);
            }
        }

        private void CreateTextbox(int x0, int y0, int b, int h, int nr)
        {
            //text box read only
            TBox[nr] = new TextBox
            {
                Location = new System.Drawing.Point(x0, y0),
                Size = new System.Drawing.Size(b, h),
                ReadOnly = true,
                TextAlign = System.Windows.Forms.HorizontalAlignment.Right,
                Enabled = false // Kuntner
            };
            groupBox2.Controls.Add(TBox[nr]);

            //text box for input of wind speed classes
            NumUpDown[nr] = new NumericUpDown
            {
                Location = new System.Drawing.Point(x0 + 105, y0),
                Size = new System.Drawing.Size(b + 20, h),
                Minimum = 0.1M,
                Maximum = 50M,
                Increment = 0.25M,
                DecimalPlaces = 2
            };
            NumUpDown[nr].ValueChanged += new System.EventHandler(CheckWindClassesInput); //input values are written to the following text box
            groupBox2.Controls.Add(NumUpDown[nr]);
#if __MonoCS__
#else
            NumUpDown[nr].TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
#endif

            //labels for text boxes
            LbTbox[nr] = new Label
            {
                AutoSize = true,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Location = new System.Drawing.Point(x0 + 55, y0 + 3),
                Size = new System.Drawing.Size(46, 16),
                Text = "m/s  -"
            };
            groupBox2.Controls.Add(LbTbox[nr]);
            LbTbox2[nr] = new Label
            {
                AutoSize = true,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Location = new System.Drawing.Point(x0 + 180, y0 + 3),
                Size = new System.Drawing.Size(46, 16),
                Text = "m/s"
            };
            groupBox2.Controls.Add(LbTbox2[nr]);
        }

        private void RemoveTextbox(int nr)
        {
            groupBox2.Controls.Remove(TBox[nr]);
            groupBox2.Controls.Remove(NumUpDown[nr]);
            groupBox2.Controls.Remove(LbTbox[nr]);
            groupBox2.Controls.Remove(LbTbox2[nr]);
        }

        /// <summary>
        /// Change anemometer height
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeteoAnemometerHeight(object sender, EventArgs e)
        {
            //input of anemometerheight
            Anemometerheight = Convert.ToDouble(numericUpDown7.Value);
            if (EmifileReset == true)
            {
                ChangedWindData = true;
                Set_met_button(ChangedWindData);
                string newPath = Path.Combine(ProjectName, @"Computation", "meteopgt.all");
                File.Delete(newPath);
                Textbox16_Set("");
                newPath = Path.Combine(ProjectName, @"Computation", "mettimeseries.dat");
                File.Delete(newPath);
            }
        }

        /// <summary>
        /// decide whether dispersion situations are classified or not
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeteoUseNotClassifiedData(object sender, EventArgs e)
        {
            if (EmifileReset == true)
            {
                ChangedWindData = true;
                Set_met_button(ChangedWindData);
                string newPath = Path.Combine(ProjectName, @"Computation", "meteopgt.all");
                File.Delete(newPath);
                Textbox16_Set("");
                newPath = Path.Combine(ProjectName, @"Computation", "mettimeseries.dat");
                File.Delete(newPath);
            }
        }

    }
}
