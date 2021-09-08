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
 * Time: 20:57
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Windows.Forms;
using GralIO;
using System.Collections.Generic;

namespace Gral
{
    /// <summary>
    /// Analyzing tools
    /// </summary>
    partial class Main
    {
        ///////////////////////////////////////////////////////////
        /// 
        ///   Analyzing tools for GRAL concentration fields
        ///
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Computing mean concentrations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnalyseMeanConcentration(object sender, EventArgs e)
        {
            //select slice
            using (GralMainForms.Selectslice sel_slice = new GralMainForms.Selectslice())
            {
                sel_slice.HorSlices = CollectSliceHeights();

                if (sel_slice.ShowDialog() == DialogResult.OK)
                {
                    //select source groups
                    using (GralMainForms.SelectSourcegroups sel_sg = new GralMainForms.SelectSourcegroups(this, 0))
                    {
                        sel_sg.checkBox3.Visible = false;
                        sel_sg.checkBox1.Visible = false;
                        sel_sg.Prefix = FilePrefix;

                        if (sel_sg.ShowDialog() == DialogResult.OK)
                        {
                            bool depositionOutput = true; // write deposition at the 1st slice but not at other slices
                            foreach (int slice in sel_slice.SelectedSlices)
                            {
                                int maxsource = Math.Max(DefinedSourceGroups.Count, sel_sg.listBox1.Items.Count);

                                // now start the backgroundworker to calculate the percentils
                                GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData();

                                foreach (int itm in sel_sg.listBox1.SelectedIndices)
                                {
                                    DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup + Convert.ToString(sel_sg.listBox1.Items[itm]) + ","; // name of selected source groups
                                }
                                DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup.TrimEnd(new char[] { ',' }); // remove last ','

                                DataCollection.ProjectName = ProjectName;
                                DataCollection.Prefix = sel_sg.Prefix;
                                if (sel_sg.Prefix.Length > 1)
                                {
                                    FilePrefix = sel_sg.Prefix.Substring(0, sel_sg.Prefix.Length - 1);
                                }

                                DataCollection.DecSep = DecimalSep;
                                DataCollection.MaxSource = maxsource;
                                DataCollection.CellsGralX = CellsGralX;
                                DataCollection.CellsGralY = CellsGralY;

                                DataCollection.Horgridsize = HorGridSize;
                                DataCollection.DomainWest = GralDomRect.West;
                                DataCollection.DomainSouth = GralDomRect.South;
                                DataCollection.Slice = slice + 1;
                                DataCollection.CalculateMaxHour = sel_sg.checkBox1.Checked;
                                DataCollection.CalculateMean = sel_sg.checkBox2.Checked;
                                DataCollection.CalculateDayMax = sel_sg.checkBox3.Checked;
                                DataCollection.Pollutant = Convert.ToString(listBox5.SelectedItem);
                                DataCollection.Slicename = sel_slice.HorSlices[slice];

                                DataCollection.Caption = "Compute Mean concentrations";
                                DataCollection.UserText = "Compute " + "Slice: " + DataCollection.Slicename + Environment.NewLine +
                                    "Source-Groups: " + DataCollection.SelectedSourceGroup + Environment.NewLine;
                                if (DataCollection.CalculateMean)
                                {
                                    DataCollection.UserText += "Mean concentrations / ";
                                }

                                if (DataCollection.CalculateMaxHour)
                                {
                                    DataCollection.UserText += "Max. concentrations / ";
                                }

                                WriteGralLogFile(2, DataCollection.UserText, DataCollection.Caption); // Write Gral-Logfile
                                DataCollection.UserText += Environment.NewLine + "Result file name: Mean_" + sel_sg.Prefix + DataCollection.Pollutant + "_..._" + DataCollection.Slicename + ".txt";
                                DataCollection.UserText += Environment.NewLine + "The process may take some minutes";

                                DataCollection.BackgroundWorkerFunction = GralBackgroundworkers.BWMode.MeanMeteoPGT; // 28 = Compute Mean, Max
                                DataCollection.WriteDepositionOrOdourData = depositionOutput;
                                depositionOutput = false;

                                GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                                {
                                    Text = DataCollection.Caption
                                };
                                BackgroundStart.Show();
                                // now the backgroundworker works
                            }
                        }
                    }
                }
            }
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// routine computes mean, max, and daily maximum concentrations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnalyseMeanMaxMinConcentration(object sender, EventArgs e)
        {
            //select slice
            using (GralMainForms.Selectslice sel_slice = new GralMainForms.Selectslice())
            {
                sel_slice.HorSlices = CollectSliceHeights();

                if (sel_slice.ShowDialog() == DialogResult.OK)
                {
                    //select source groups
                    using (GralMainForms.SelectSourcegroups sel_sg = new GralMainForms.SelectSourcegroups(this, 0))
                    {
                        sel_sg.checkBox1.Visible = true;
                        sel_sg.checkBox2.Visible = true;
                        sel_sg.checkBox3.Visible = true;
                        sel_sg.Prefix = FilePrefix;

                        if (sel_sg.ShowDialog() == DialogResult.OK)
                        {
                            bool depositionOutput = true; // write deposition at the 1st slice but not at other slices
                            foreach (int slice in sel_slice.SelectedSlices)
                            {
                                int maxsource = Math.Max(DefinedSourceGroups.Count, sel_sg.listBox1.Items.Count);

                                // now start the backgroundworker to calculate the percentils
                                GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData();

                                foreach (int itm in sel_sg.listBox1.SelectedIndices)
                                {
                                    DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup + Convert.ToString(sel_sg.listBox1.Items[itm]) + ","; // name of selected source groups
                                }
                                DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup.TrimEnd(new char[] { ',' }); // remove last ','

                                DataCollection.ProjectName = ProjectName;
                                DataCollection.Prefix = sel_sg.Prefix;
                                if (sel_sg.Prefix.Length > 1)
                                {
                                    FilePrefix = sel_sg.Prefix.Substring(0, sel_sg.Prefix.Length - 1);
                                }

                                DataCollection.DecSep = DecimalSep;
                                DataCollection.MaxSource = maxsource;
                                DataCollection.CellsGralX = CellsGralX;
                                DataCollection.CellsGralY = CellsGralY;

                                DataCollection.Horgridsize = HorGridSize;
                                DataCollection.DomainWest = GralDomRect.West;
                                DataCollection.DomainSouth = GralDomRect.South;
                                DataCollection.Slice = slice + 1;
                                DataCollection.CalculateMaxHour = sel_sg.checkBox1.Checked;
                                DataCollection.CalculateMean = sel_sg.checkBox2.Checked;
                                DataCollection.CalculateDayMax = sel_sg.checkBox3.Checked;
                                DataCollection.MeteoNotClassified = checkBox19.Checked;
                                DataCollection.Pollutant = Convert.ToString(listBox5.SelectedItem);
                                DataCollection.Slicename = sel_slice.HorSlices[slice];

                                DataCollection.Caption = "Compute Mean, Max, Daily Max.";
                                DataCollection.UserText = "Compute " + "Slice: " + DataCollection.Slicename + Environment.NewLine +
                                    "Source-Groups: " + DataCollection.SelectedSourceGroup + Environment.NewLine;
                                if (DataCollection.CalculateMean)
                                {
                                    DataCollection.UserText += "Mean concentrations / ";
                                }

                                if (DataCollection.CalculateMaxHour)
                                {
                                    DataCollection.UserText += "Max. concentrations / ";
                                }

                                if (DataCollection.CalculateDayMax)
                                {
                                    DataCollection.UserText += "Daily max. concentrations ";
                                }

                                WriteGralLogFile(2, DataCollection.UserText, DataCollection.Caption); // Write Gral-Logfile
                                DataCollection.UserText += Environment.NewLine + "Result file name: Mean_" + sel_sg.Prefix + DataCollection.Pollutant + "_..._" + DataCollection.Slicename + ".txt";
                                DataCollection.UserText += Environment.NewLine + "The process may take some minutes";

                                DataCollection.BackgroundWorkerFunction = GralBackgroundworkers.BWMode.MeanMaxTimeSeries; // 25 = Compute Mean, Max, DailyMax
                                DataCollection.WriteDepositionOrOdourData = depositionOutput;
                                depositionOutput = false;

                                GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                                {
                                    Text = DataCollection.Caption
                                };
                                BackgroundStart.Show();
                                // now the backgroundworker works
                            }
                        }
                    }
                }
            }
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// routine, which computes time series and wind data at given receptor points
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnalyseReceptorConcentration(object sender, EventArgs e)
        {
            if (checkBox32.Checked == true) // transient GRAL mode
            {
                Button47Click(null, null); // analyze receptor wind data
                return;
            }

            //select source groups
            using (GralMainForms.SelectSourcegroups sel_sg = new GralMainForms.SelectSourcegroups(this, 0))
            {
                sel_sg.checkBox1.Visible = false;
                sel_sg.checkBox2.Visible = false;
                sel_sg.checkBox3.Visible = false;

                sel_sg.Prefix = FilePrefix;
                sel_sg.listBox1.Enabled = false;

                if (sel_sg.ShowDialog() == DialogResult.OK)
                {
                    int maxsource = Math.Max(DefinedSourceGroups.Count, 1);
                    // now start the backgroundworker to calculate the percentils
                    GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData();
                    int c = 0;
                    foreach (int itm in sel_sg.listBox1.SelectedIndices)
                    {
                        c++;
                        DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup + Convert.ToString(sel_sg.listBox1.Items[itm]) + ","; // name of selected source groups
                    }

                    for (int index = 0; index < listView1.Items.Count; index++)
                    {
                        string[] sg = new string[2];
                        sg = listView1.Items[index].SubItems[0].Text.Split(new char[] { ':' });
                        if (sg.Length > 1) // does a ':' exist?
                        {
                            DataCollection.ComputedSourceGroup = DataCollection.ComputedSourceGroup + sg[1] + ","; // number of computed source groups
                        }
                        else
                        {
                            DataCollection.ComputedSourceGroup = DataCollection.ComputedSourceGroup + sg[0] + ","; // number of computed source groups
                        }
                    }
                    //MessageBox.Show(DataCollection.Comp_Source_Grp);
                    if (c >= 1) // at least one SG selected
                    {
                        DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup.TrimEnd(new char[] { ',' }); // remove last ','

                        DataCollection.ProjectName = ProjectName;
                        DataCollection.Prefix = sel_sg.Prefix;
                        DataCollection.DecSep = DecimalSep;
                        DataCollection.MaxSource = maxsource;
                        DataCollection.MaxSourceComputed = Math.Max(listView1.Items.Count, 1);
                        DataCollection.Pollutant = Convert.ToString(listBox5.SelectedItem);

                        DataCollection.CalculateMaxHour = checkBox19.Checked;

                        DataCollection.Caption = "Compute time series at the receptorpoints";
                        DataCollection.UserText = "Compute " + "time series at the receptorpoints " + Environment.NewLine +
                            "Source-Groups: " + DataCollection.SelectedSourceGroup;

                        WriteGralLogFile(2, DataCollection.UserText, DataCollection.Caption); // Write Gral-Logfile
                        DataCollection.UserText += Environment.NewLine + "The process may take some minutes";
                        DataCollection.BackgroundWorkerFunction = GralBackgroundworkers.BWMode.ReceptorTimeSeries; // 37 = Time series

                        GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                        {
                            Text = DataCollection.Caption
                        };
                        BackgroundStart.Show();
                        // now the backgroundworker works
                    }
                    else
                    {
                        MessageBox.Show("No sourcegroup selected", "Receptor concentration", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        /// <summary>
        /// routine, which computes wind data at given receptor points
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AnalyseReceptorMeteoData(object sender, EventArgs e)
        {
            // now start the backgroundworker to calculate the local wind data
            GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData
            {
                SelectedSourceGroup = string.Empty,
                ComputedSourceGroup = string.Empty, // number of computed source groups

                MaxSource = Math.Max(DefinedSourceGroups.Count, 1)
            };
            ;
            DataCollection.MaxSourceComputed = Math.Max(listView1.Items.Count, 1);

            DataCollection.ProjectName = ProjectName;
            DataCollection.DecSep = DecimalSep;

            DataCollection.CalculateMaxHour = checkBox19.Checked;

            DataCollection.Caption = "Compute met files at the receptorpoints";
            DataCollection.UserText = "Compute met files at the receptorpoints ";

            WriteGralLogFile(2, DataCollection.UserText, DataCollection.Caption); // Write Gral-Logfile
            DataCollection.UserText += Environment.NewLine + "The process may take some minutes";
            DataCollection.BackgroundWorkerFunction = GralBackgroundworkers.BWMode.ReceptorTimeSeries; // 37 = Time series / met files

            GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
            {
                Text = DataCollection.Caption
            };
            BackgroundStart.Show();
            // now the backgroundworker works
        }

        /// <summary>
        /// computes high percentiles of pollutant concentrations of a time series
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnalyseHighPercentiles(object sender, EventArgs e)
        {
            //select slice
            using (GralMainForms.Selectslice sel_slice = new GralMainForms.Selectslice())
            {
                sel_slice.HorSlices = CollectSliceHeights();
                if (sel_slice.ShowDialog() == DialogResult.OK)
                {
                    //select source groups
                    using (GralMainForms.SelectSourcegroups sel_sg = new GralMainForms.SelectSourcegroups(this, 1))
                    {
                        sel_sg.checkBox1.Visible = false;
                        sel_sg.checkBox2.Visible = false;
                        sel_sg.checkBox3.Visible = false;
                        sel_sg.Prefix = FilePrefix;

                        if (sel_sg.ShowDialog() == DialogResult.OK)
                        {
                            foreach (int slice in sel_slice.SelectedSlices)
                            {
                                int maxsource = Math.Max(DefinedSourceGroups.Count, sel_sg.listBox1.Items.Count);

                                //select percentile
                                decimal percentile = (decimal)98.0;
                                percentile = (decimal)sel_sg.numericUpDown2.Value;

                                // now start the backgroundworker to calculate the percentils
                                GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData();

                                foreach (int itm in sel_sg.listBox1.SelectedIndices)
                                {
                                    DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup + Convert.ToString(sel_sg.listBox1.Items[itm]) + ","; // name of selected source groups
                                }
                                DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup.TrimEnd(new char[] { ',' }); // remove last ','

                                DataCollection.ProjectName = ProjectName;
                                DataCollection.Prefix = sel_sg.Prefix;
                                if (sel_sg.Prefix.Length > 1)
                                {
                                    FilePrefix = sel_sg.Prefix.Substring(0, sel_sg.Prefix.Length - 1);
                                }

                                DataCollection.DecSep = DecimalSep;
                                DataCollection.MaxSource = maxsource;
                                DataCollection.CellsGralX = CellsGralX;
                                DataCollection.CellsGralY = CellsGralY;
                                DataCollection.Percentile = percentile;
                                DataCollection.Horgridsize = HorGridSize;
                                DataCollection.DomainWest = GralDomRect.West;
                                DataCollection.DomainSouth = GralDomRect.South;
                                DataCollection.MeteoNotClassified = checkBox19.Checked;
                                DataCollection.Slice = slice + 1;
                                DataCollection.CalculateMean = sel_sg.checkBox2.Checked;
                                DataCollection.Pollutant = Convert.ToString(listBox5.SelectedItem);
                                DataCollection.Slicename = sel_slice.HorSlices[slice];
                                DataCollection.Unit = "µg/m³";
                                DataCollection.EmissionFactor = 1.0F;

                                DataCollection.Caption = "Compute High Percentiles  ";
                                DataCollection.UserText = "Compute " + Convert.ToString(percentile) + " Percentile " + Environment.NewLine + "Slice: " + DataCollection.Slicename + Environment.NewLine +
                                    "Source-Groups: " + DataCollection.SelectedSourceGroup;

                                WriteGralLogFile(2, DataCollection.UserText, DataCollection.Caption); // Write Gral-Logfile
                                DataCollection.UserText += Environment.NewLine + "Result file name: " + Convert.ToString(Math.Round(DataCollection.Percentile, 1)).Replace(DecimalSep, "_") + "_" + sel_sg.Prefix + DataCollection.Pollutant + "_..._" + DataCollection.Slicename + ".txt";
                                DataCollection.UserText += Environment.NewLine + "The process may take some minutes";

                                DataCollection.BackgroundWorkerFunction = GralBackgroundworkers.BWMode.HighPercentiles; // 40 = compute high percentils

                                GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                                {
                                    Text = DataCollection.Caption
                                };
                                BackgroundStart.Show();
                                // now the backgroundworker works
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// "Multisource": computes odour hours for multiple sources considering diurnal/seasonal emission modulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnalyseOdourMultipleSources(object sender, EventArgs e)
        {
            //select slice
            using (GralMainForms.Selectslice sel_slice = new GralMainForms.Selectslice())
            {
                sel_slice.HorSlices = CollectSliceHeights();

                if (sel_slice.ShowDialog() == DialogResult.OK)
                {
                    //select source groups
                    using (GralMainForms.SelectSourcegroups sel_sg = new GralMainForms.SelectSourcegroups(this, 2))
                    {
                        sel_sg.checkBox1.Visible = false;
                        sel_sg.checkBox2.Visible = false;
                        sel_sg.checkBox3.Visible = false;
                        sel_sg.Prefix = FilePrefix;

                        if (sel_sg.ShowDialog() == DialogResult.OK)
                        {
                            int maxsource = Math.Max(DefinedSourceGroups.Count, sel_sg.listBox1.Items.Count);

                            //evaluation of odour-hour frequencies
                            if (sel_sg.radioButton1.Checked == true)
                            {
                                //select threshold for odour concentration
                                decimal odourthreshold = 1;
                                bool writeAdditionalFiles = false;

                                odourthreshold = (decimal)sel_sg.numericUpDown3.Value;

                                //define 90 percentil to mean ratio
                                int peakmean = 4;

                                //input either constant peakmean or use variance model. The latter is applicable only if vertically adjecent concentration layers exist
                                bool input_correct = false;
                                string newPath = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                                DirectoryInfo di = new DirectoryInfo(newPath);
                                FileInfo[] files_conc = di.GetFiles("*.odr");
                                if (files_conc.Length == 0)
                                {
                                    files_conc = di.GetFiles("*.grz");
                                }

                                if (files_conc.Length > 0 &&
                                    (Convert.ToDouble(TBox3[0].Value) >= 1.5 * Convert.ToDouble(numericUpDown8.Value))) //use advanced input box if odour files available and lowest conc. layer > 1.5 * vert. extension
                                {
                                    using (GralMainForms.Input_Odour inodour = new GralMainForms.Input_Odour()
                                    {
                                        StartPosition = FormStartPosition.Manual,
                                        Location = new System.Drawing.Point(this.Left, this.Top),
                                        Owner = this
                                    })
                                    {
                                        if (inodour.ShowDialog() == DialogResult.OK)
                                        {
                                            peakmean = inodour.MeanToPeak;
                                            writeAdditionalFiles = inodour.WriteAdditionalFiles;
                                            input_correct = true;
                                        }
                                    }
                                }
                                else //use simple input box
                                {
                                    if (InputBox1("Define 90percentile/mean ratio", "Ratio ?/mean:", 1, 100, ref peakmean) == DialogResult.OK)
                                    {
                                        input_correct = true;
                                    }
                                }
                                if (input_correct == true)
                                {
                                    foreach (int slice in sel_slice.SelectedSlices)
                                    {
                                        // now start the backgroundworker to calculate the percentils
                                        GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData();

                                        foreach (int itm in sel_sg.listBox1.SelectedIndices)
                                        {
                                            DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup + Convert.ToString(sel_sg.listBox1.Items[itm]) + ","; // name of selected source groups
                                        }
                                        DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup.TrimEnd(new char[] { ',' }); // remove last ','

                                        DataCollection.ProjectName = ProjectName;
                                        DataCollection.Prefix = sel_sg.Prefix;
                                        if (sel_sg.Prefix.Length > 1)
                                        {
                                            FilePrefix = sel_sg.Prefix.Substring(0, sel_sg.Prefix.Length - 1);
                                        }

                                        DataCollection.DecSep = DecimalSep;
                                        DataCollection.MaxSource = maxsource;
                                        DataCollection.CellsGralX = CellsGralX;
                                        DataCollection.CellsGralY = CellsGralY;

                                        DataCollection.Horgridsize = HorGridSize;
                                        DataCollection.VertgridSize = (float)numericUpDown8.Value;
                                        DataCollection.DomainWest = GralDomRect.West;
                                        DataCollection.DomainSouth = GralDomRect.South;
                                        DataCollection.Slice = slice + 1;
                                        DataCollection.CalculateMaxHour = sel_sg.checkBox1.Checked;
                                        DataCollection.CalculateMean = sel_sg.checkBox2.Checked;
                                        DataCollection.CalculateDayMax = sel_sg.checkBox3.Checked;
                                        DataCollection.Pollutant = Convert.ToString(listBox5.SelectedItem);
                                        DataCollection.Slicename = sel_slice.HorSlices[slice];
                                        DataCollection.OdourThreshold = odourthreshold;
                                        DataCollection.Peakmean = peakmean;

                                        DataCollection.Caption = "Compute Odour hours";
                                        DataCollection.UserText = "Compute " + "Slice: " + DataCollection.Slicename + Environment.NewLine +
                                            "Source-Groups: " + DataCollection.SelectedSourceGroup + Environment.NewLine +
                                            "Odour threshold: " + Convert.ToString(odourthreshold) + @" OU/m" + CubeString +
                                            "90percentile/mean ratio" + Convert.ToString(peakmean);

                                        WriteGralLogFile(2, DataCollection.UserText, DataCollection.Caption); // Write Gral-Logfile
                                        DataCollection.UserText += Environment.NewLine + "Result file name: Mean_Odour_" + sel_sg.Prefix + DataCollection.Pollutant + "_..._" + DataCollection.Slicename + ".txt";
                                        DataCollection.UserText += Environment.NewLine + "The process may take some minutes";

                                        DataCollection.BackgroundWorkerFunction = GralBackgroundworkers.BWMode.OdorHours; // 27 = Compute Odour hours
                                        DataCollection.WriteDepositionOrOdourData = writeAdditionalFiles;

                                        //check if GRAL simulations were carried out in transient mode
                                        try
                                        {
                                            InDatVariables data = new InDatVariables();
                                            InDatFileIO ReadInData = new InDatFileIO();
                                            data.InDatPath = Path.Combine(ProjectName, "Computation", "in.dat");
                                            ReadInData.Data = data;
                                            if (ReadInData.ReadInDat() == true)
                                            {
                                                if (data.Transientflag == 0)
                                                {
                                                    DataCollection.BackgroundWorkerFunction = GralBackgroundworkers.BWMode.OdorHoursTransient; // 24 = Compute Odour hours in transient mode
                                                }
                                            }
                                        }
                                        catch { }

                                        GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                                        {
                                            Text = DataCollection.Caption
                                        };
                                        BackgroundStart.Show();
                                        // now the backgroundworker works
                                    }
                                }
                            }
                            //evaluation of odour concentrations
                            else
                            {
                                foreach (int slice in sel_slice.SelectedSlices)
                                {
                                    // now start the backgroundworker to calculate the percentils of an odour calculation
                                    GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData();

                                    foreach (int itm in sel_sg.listBox1.SelectedIndices)
                                    {
                                        DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup + Convert.ToString(sel_sg.listBox1.Items[itm]) + ","; // name of selected source groups
                                    }
                                    DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup.TrimEnd(new char[] { ',' }); // remove last ','

                                    DataCollection.ProjectName = ProjectName;
                                    DataCollection.Prefix = sel_sg.Prefix;
                                    if (sel_sg.Prefix.Length > 1)
                                    {
                                        FilePrefix = sel_sg.Prefix.Substring(0, sel_sg.Prefix.Length - 1);
                                    }

                                    DataCollection.DecSep = DecimalSep;
                                    DataCollection.MaxSource = maxsource;
                                    DataCollection.CellsGralX = CellsGralX;
                                    DataCollection.CellsGralY = CellsGralY;
                                    DataCollection.Unit = "OU/m³";
                                    DataCollection.EmissionFactor = 0.001F;

                                    DataCollection.Horgridsize = HorGridSize;
                                    DataCollection.VertgridSize = (float)numericUpDown8.Value;
                                    DataCollection.DomainWest = GralDomRect.West;
                                    DataCollection.DomainSouth = GralDomRect.South;
                                    DataCollection.Slice = slice + 1;
                                    DataCollection.CalculateMaxHour = sel_sg.checkBox1.Checked;
                                    DataCollection.CalculateMean = sel_sg.checkBox2.Checked;
                                    DataCollection.CalculateDayMax = sel_sg.checkBox3.Checked;
                                    DataCollection.Pollutant = Convert.ToString(listBox5.SelectedItem);
                                    DataCollection.Slicename = sel_slice.HorSlices[slice];
                                    DataCollection.Percentile = (decimal)sel_sg.numericUpDown1.Value;

                                    DataCollection.Caption = "Compute Odour concentration";
                                    DataCollection.UserText = "Compute " + "Slice: " + DataCollection.Slicename + Environment.NewLine +
                                        "Source-Groups: " + DataCollection.SelectedSourceGroup + Environment.NewLine +
                                        "Percentile: " + Convert.ToString(DataCollection.Percentile);

                                    WriteGralLogFile(2, DataCollection.UserText, DataCollection.Caption); // Write Gral-Logfile
                                    DataCollection.UserText += Environment.NewLine + "Result file name: Mean_Odour_" + sel_sg.Prefix + DataCollection.Pollutant + "_..._" + DataCollection.Slicename;
                                    DataCollection.UserText += Environment.NewLine + "The process may take some minutes";

                                    DataCollection.BackgroundWorkerFunction = GralBackgroundworkers.BWMode.OdorConcentrationPercentile; // 23 = Compute Odour concentration percentiles

                                    //check if GRAL simulations were carried out in transient mode
                                    try
                                    {
                                        InDatVariables data = new InDatVariables();
                                        InDatFileIO ReadInData = new InDatFileIO();
                                        data.InDatPath = Path.Combine(ProjectName, "Computation", "in.dat");
                                        ReadInData.Data = data;
                                        if (ReadInData.ReadInDat() == true)
                                        {
                                            if (data.Transientflag == 0)
                                            {
                                                DataCollection.BackgroundWorkerFunction = GralBackgroundworkers.BWMode.OdorConcentrationPercentile; // 23 = Compute Odour concentration percentiles in transient mode
                                            }
                                        }
                                    }
                                    catch { }

                                    GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                                    {
                                        Text = DataCollection.Caption
                                    };
                                    BackgroundStart.Show();
                                    // now the backgroundworker works
                                }
                            }
                        }
                    }
                }
            }            
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// compute odour hours from compost works (up to three different processes with varying frequencies)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnalyseOdourCompostWorks(object sender, EventArgs e)
        {
            //select slice
            using (GralMainForms.Selectslice sel_slice = new GralMainForms.Selectslice())
            {
                sel_slice.HorSlices = CollectSliceHeights();

                if (sel_slice.ShowDialog() == DialogResult.OK)
                {
                    //select source groups
                    using (GralMainForms.SelectSourcegroups sel_sg = new GralMainForms.SelectSourcegroups(this, 3))
                    {
                        sel_sg.checkBox1.Visible = false;
                        sel_sg.checkBox2.Visible = false;
                        sel_sg.checkBox3.Visible = false;
                        sel_sg.listBox1.SelectionMode = SelectionMode.One;
                        sel_sg.Prefix = FilePrefix;
                        
                        if (sel_sg.ShowDialog() == DialogResult.OK)
                        {
                            //select source groups
                            GralMainForms.Kompost komp = new GralMainForms.Kompost()
                            {
                                StartPosition = FormStartPosition.Manual,
                                Location = new System.Drawing.Point(this.Left, this.Top),
                                Owner = this
                            };

                            DialogResult dial = new DialogResult();
                            dial = komp.ShowDialog();
                            {
                                bool writeAdditionalFiles = false;
                                double[] odemi = new double[3];
                                double[] odfreq = new double[3];
                                double scale = 1;
                                //select odour emissions and frequencies for different processes on a compost works
                                odemi[0] = Convert.ToDouble(komp.textBox1.Text.Replace(".", DecimalSep));
                                odemi[1] = Convert.ToDouble(komp.textBox4.Text.Replace(".", DecimalSep));
                                odemi[2] = Convert.ToDouble(komp.textBox6.Text.Replace(".", DecimalSep));
                                odfreq[0] = Convert.ToDouble(komp.textBox2.Text.Replace(".", DecimalSep)) / 100;
                                odfreq[1] = Convert.ToDouble(komp.textBox3.Text.Replace(".", DecimalSep)) / 100;
                                odfreq[2] = Convert.ToDouble(komp.textBox5.Text.Replace(".", DecimalSep)) / 100;
                                scale = Convert.ToDouble(komp.textBox7.Text.Replace(".", DecimalSep));

                                double totfreq = odfreq[0] * odfreq[1] * odfreq[2];
                                double totemi = odemi[0] + odemi[1] + odemi[2];


                                int maxsource = 100;
                                //select threshold for odour concentration
                                decimal odourthreshold = 1;
                                if (InputBox2(@"Define odour threshold", "Odour concentration [OU/m" + CubeString + "]:", 0, 50, ref odourthreshold) == DialogResult.OK)
                                {
                                }
                                //define 90 percentil to mean ratio
                                int peakmean = 4;

                                //input either constant peakmean or use variance model. The latter is applicable only if vertically adjecent concentration layers exist
                                bool input_correct = false;
                                string newPath = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                                DirectoryInfo di = new DirectoryInfo(newPath);
                                FileInfo[] files_conc = di.GetFiles("*.odr");
                                if (files_conc.Length == 0)
                                {
                                    files_conc = di.GetFiles("*.grz");
                                }

                                if (files_conc.Length > 0 &&
                                    (Convert.ToDouble(TBox3[0].Value) >= 1.5 * Convert.ToDouble(numericUpDown8.Value))) //use advanced input box if odour files available and lowest conc. layer > 1.5 * vert. extension
                                {
                                    using(GralMainForms.Input_Odour inodour = new GralMainForms.Input_Odour()
                                    {
                                        StartPosition = FormStartPosition.Manual,
                                        Location = new System.Drawing.Point(this.Left, this.Top),
                                        Owner = this
                                    })
                                    {
                                        if (inodour.ShowDialog() == DialogResult.OK)
                                        {
                                            peakmean = inodour.MeanToPeak;
                                            writeAdditionalFiles = inodour.WriteAdditionalFiles;
                                            
                                            input_correct = true;
                                        }
                                    }
                                }
                                else //use simple input box
                                {
                                    if (InputBox1("Define 90percentile/mean ratio", "Ratio ?/mean:", 1, 100, ref peakmean) == DialogResult.OK)
                                    {
                                        input_correct = true;
                                    }
                                }


                                if (input_correct == true)
                                {
                                    foreach (int slice in sel_slice.SelectedSlices)
                                    {
                                        // now start the backgroundworker to calculate the percentils
                                        GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData();

                                        foreach (int itm in sel_sg.listBox1.SelectedIndices)
                                        {
                                            DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup + Convert.ToString(sel_sg.listBox1.Items[itm]) + ","; // name of selected source groups
                                        }
                                        DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup.TrimEnd(new char[] { ',' }); // remove last ','

                                        DataCollection.ProjectName = ProjectName;
                                        DataCollection.Prefix = sel_sg.Prefix;
                                        if (sel_sg.Prefix.Length > 1)
                                        {
                                            FilePrefix = sel_sg.Prefix.Substring(0, sel_sg.Prefix.Length - 1);
                                        }

                                        DataCollection.DecSep = DecimalSep;
                                        DataCollection.MaxSource = maxsource;
                                        DataCollection.CellsGralX = CellsGralX;
                                        DataCollection.CellsGralY = CellsGralY;

                                        DataCollection.Horgridsize = HorGridSize;
                                        DataCollection.VertgridSize = (float)numericUpDown8.Value;
                                        DataCollection.DomainWest = GralDomRect.West;
                                        DataCollection.DomainSouth = GralDomRect.South;
                                        DataCollection.Slice = slice + 1;
                                        DataCollection.CalculateMaxHour = sel_sg.checkBox1.Checked;
                                        DataCollection.CalculateMean = sel_sg.checkBox2.Checked;
                                        DataCollection.CalculateDayMax = sel_sg.checkBox3.Checked;
                                        DataCollection.Pollutant = Convert.ToString(listBox5.SelectedItem);
                                        DataCollection.Slicename = sel_slice.HorSlices[slice];
                                        DataCollection.OdourThreshold = odourthreshold;
                                        DataCollection.Peakmean = peakmean;
                                        DataCollection.Odemi = odemi;
                                        DataCollection.OdFreq = odfreq;
                                        DataCollection.Scale = scale;

                                        DataCollection.Caption = "Compute Odour hours";
                                        DataCollection.UserText = "Compute " + "Slice: " + DataCollection.Slicename + Environment.NewLine +
                                            "Source-Groups: " + DataCollection.SelectedSourceGroup +
                                            "  / Odour threshold = " + Convert.ToString(odourthreshold) + @" OU/m" + CubeString +
                                            "  / 90percentile/mean ratio = " + Convert.ToString(peakmean) + " / Scaling factor = " + Convert.ToString(DataCollection.Scale) + Environment.NewLine;
                                        for (int i = 0; i < 3; i++)
                                        {
                                            DataCollection.UserText += "Process " + Convert.ToString(i + 1) + ": Emission = " + Convert.ToString(DataCollection.Odemi[i]) +
                                                " MOU/h  Frequency = " + Convert.ToString(DataCollection.OdFreq[i] * 100) + " %" + Environment.NewLine;
                                        }
                                        WriteGralLogFile(2, DataCollection.UserText, DataCollection.Caption); // Write Gral-Logfile
                                        DataCollection.UserText += Environment.NewLine + "The process may take some minutes";

                                        DataCollection.BackgroundWorkerFunction = GralBackgroundworkers.BWMode.OdorCompost; // 26 = Compute compost
                                        DataCollection.WriteDepositionOrOdourData = writeAdditionalFiles;

                                        GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                                        {
                                            Text = DataCollection.Caption
                                        };
                                        BackgroundStart.Show();
                                        // now the backgroundworker works
                                    }
                                }
                            }
                            komp.Dispose();
                        }
                    }
                }
            }
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// compute odour hours for "all in all out" stables
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnalyseOdourAllInAllOut(object sender, EventArgs e)
        {
            //select slice
            using (GralMainForms.Selectslice sel_slice = new GralMainForms.Selectslice())
            {
                sel_slice.HorSlices = CollectSliceHeights();

                if (sel_slice.ShowDialog() == DialogResult.OK)
                {
                    //select source groups
                    using (GralMainForms.SelectSourcegroups sel_sg = new GralMainForms.SelectSourcegroups(this, 3))
                    {
                        sel_sg.checkBox1.Visible = false;
                        sel_sg.checkBox2.Visible = false;
                        sel_sg.checkBox3.Visible = false;
                        sel_sg.Prefix = FilePrefix;

                        if (sel_sg.ShowDialog() == DialogResult.OK)
                        {
                            //select source group with "all in all out" system
                            GralMainForms.Allinallout allin = new GralMainForms.Allinallout()
                            {
                                Owner = this,
                                StartPosition = FormStartPosition.Manual,
                                Location = new System.Drawing.Point(this.Left, this.Top)
                            };
                            DialogResult dial = new DialogResult();

                            //fill combobox with selected source groups
                            foreach (int itm in sel_sg.listBox1.SelectedIndices)
                            {
                                allin.comboBox1.Items.Add(sel_sg.listBox1.Items[itm]);
                            }

                            allin.comboBox1.SelectedIndex = 0;

                            dial = allin.ShowDialog();
                            if (dial == DialogResult.OK)
                            {
                                int maxsource = Math.Max(DefinedSourceGroups.Count, sel_sg.listBox1.Items.Count);
                                bool writeAdditionalFiles = false;
                                //select threshold for odour concentration
                                decimal odourthreshold = 1;
                                odourthreshold = (decimal)sel_sg.numericUpDown3.Value;

                                //define 90 percentil to mean ratio
                                int peakmean = 4;

                                //input either constant peakmean or use variance model. The latter is applicable only if vertically adjecent concentration layers exist
                                bool input_correct = false;
                                string newPath = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                                DirectoryInfo di = new DirectoryInfo(newPath);
                                FileInfo[] files_conc = di.GetFiles("*.odr");
                                if (files_conc.Length == 0)
                                {
                                    files_conc = di.GetFiles("*.grz");
                                }

                                if (files_conc.Length > 0 &&
                                    (Convert.ToDouble(TBox3[0].Value) >= 1.5 * Convert.ToDouble(numericUpDown8.Value))) //use advanced input box if odour files available and lowest conc. layer > 1.5 * vert. extension
                                {
                                    using (GralMainForms.Input_Odour inodour = new GralMainForms.Input_Odour()
                                    {
                                        StartPosition = FormStartPosition.Manual,
                                        Location = new System.Drawing.Point(this.Left, this.Top),
                                        Owner = this
                                    })
                                    {
                                        if (inodour.ShowDialog() == DialogResult.OK)
                                        {
                                            //select between constant R90 ratio or variance model
                                            peakmean = inodour.MeanToPeak;
                                            writeAdditionalFiles = inodour.WriteAdditionalFiles;
                                            
                                            input_correct = true;
                                        }
                                    }
                                }
                                else //use simple input box
                                {
                                    if (InputBox1("Define 90percentile/mean ratio", "Ratio ?/mean:", 1, 100, ref peakmean) == DialogResult.OK)
                                    {
                                        input_correct = true;
                                    }
                                }

                                if (input_correct == true)
                                {
                                    foreach (int slice in sel_slice.SelectedSlices)
                                    {
                                        // now start the backgroundworker to calculate the percentils
                                        GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData();

                                        foreach (int itm in sel_sg.listBox1.SelectedIndices)
                                        {
                                            DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup + Convert.ToString(sel_sg.listBox1.Items[itm]) + ","; // name of selected source groups
                                        }
                                        DataCollection.SelectedSourceGroup = DataCollection.SelectedSourceGroup.TrimEnd(new char[] { ',' }); // remove last ','

                                        DataCollection.ProjectName = ProjectName;
                                        DataCollection.Prefix = sel_sg.Prefix;
                                        if (sel_sg.Prefix.Length > 1)
                                        {
                                            FilePrefix = sel_sg.Prefix.Substring(0, sel_sg.Prefix.Length - 1);
                                        }

                                        DataCollection.DecSep = DecimalSep;
                                        DataCollection.MaxSource = maxsource;
                                        DataCollection.CellsGralX = CellsGralX;
                                        DataCollection.CellsGralY = CellsGralY;

                                        DataCollection.Horgridsize = HorGridSize;
                                        DataCollection.VertgridSize = (float)numericUpDown8.Value;
                                        DataCollection.DomainWest = GralDomRect.West;
                                        DataCollection.DomainSouth = GralDomRect.South;
                                        DataCollection.Slice = slice + 1;
                                        DataCollection.CalculateMaxHour = sel_sg.checkBox1.Checked;
                                        DataCollection.CalculateMean = sel_sg.checkBox2.Checked;
                                        DataCollection.CalculateDayMax = sel_sg.checkBox3.Checked;
                                        DataCollection.Pollutant = Convert.ToString(listBox5.SelectedItem);
                                        DataCollection.Slicename = sel_slice.HorSlices[slice];
                                        DataCollection.OdourThreshold = odourthreshold;
                                        DataCollection.Peakmean = peakmean;
                                        DataCollection.Division = (float)allin.numericUpDown1.Value; //breeding cylce in days
                                        DataCollection.Emptytimes = (float)allin.numericUpDown2.Value;   //empty times between two breeding cycles in days
                                        DataCollection.AllInnSelSourceGroup = Convert.ToString(allin.comboBox1.SelectedItem);

                                        DataCollection.Caption = "Compute Odour hours for 'All in all out' stables";
                                        DataCollection.UserText = "Compute " + "Slice: " + DataCollection.Slicename + Environment.NewLine +
                                            "Odour threshold = " + Convert.ToString(odourthreshold) + @" OU/m" + CubeString +
                                            "    90percentile/mean ratio = " + Convert.ToString(peakmean) + Environment.NewLine +
                                            "Breeding cycle defined for source group : " + DataCollection.AllInnSelSourceGroup + Environment.NewLine +
                                            "Breeding cycle duration :" + Convert.ToString(DataCollection.Division) +
                                            " days   Empty times between breeding cycles: " + Convert.ToString(DataCollection.Emptytimes) + " days" + Environment.NewLine;
                                        WriteGralLogFile(2, DataCollection.UserText, DataCollection.Caption); // Write Gral-Logfile
                                        DataCollection.UserText += "The process may take some minutes";

                                        DataCollection.BackgroundWorkerFunction = GralBackgroundworkers.BWMode.OdorAllinAllout; // 29 = AllInAllOut
                                        DataCollection.WriteDepositionOrOdourData = writeAdditionalFiles;

                                        GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                                        {
                                            Text = DataCollection.Caption
                                        };
                                        BackgroundStart.Show();
                                        // now the backgroundworker works
                                    }
                                }
                            }
                            allin.Dispose();
                        }

                    }
                }
            }
            Cursor = Cursors.Default;
        }

        private List<string> CollectSliceHeights()
        {
            List<string> _names = new List<string>();

            for (int i = 0; i < GRALSettings.NumHorSlices; i++)
            {
                _names.Add(GRALSettings.HorSlices[i].ToString() + " m");
            }
            return _names;
        }

    }
}
