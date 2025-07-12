#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2019-2020]  [Dietmar Oettl, Markus Kuntner]
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
using System.Globalization;
using GralIO;
using GralData;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;
using GralDomain;

namespace GralDomForms
{
    public delegate void StartMatchingDelegate(object sender, EventArgs e, ref MatchMultipleObservationsData data);
    public delegate void CancelMatchingDelegate(object sender, EventArgs e);
    public delegate void FinishMatchingDelegate(object sender, EventArgs e, MatchMultipleObservationsData data);
    public delegate void LoadWindFileData(object sender, EventArgs e);

    /// <summary>
    /// Match GRAMM wind fields to multiple oberservation stations dialog
    /// </summary>
    public partial class MatchMultipleObservations : Form
    {
        private string decsep;                        //global decimal separator of the system
        private List<string> spaltenbezeichnungen = new List<string>(); //liste mit spaltenbezeichnungen
        private int RemoveLine = -999;
        public bool StartMatch = false;
        
        private string _settings_path;
        public string SettingsPath { set { _settings_path = value; } }
        public string GRAMMPath;

        /// <summary>
        /// wind speed observations for all meteo-stations used for Match to Observation GRAMM wind fields
        /// </summary>
        public List<double[]> WindVelocityObs = new List<double[]>();
        /// <summary>
        /// wind direction observations for all meteo-stations used for Match to Observation GRAMM wind fields
        /// </summary>
        public List<double[]> WindDirectionObs = new List<double[]>();      //
        /// <summary>
        /// stability for all meteo-stations used for Match to Observation GRAMM wind fields
        /// </summary>
        public List<int[]> StabilityClassObs = new List<int[]>();      //
        /// <summary>
        /// names of meteo-stations used for Match to Observation GRAMM wind fields
        /// </summary>
        public List<string> MetFileNames = new List<string>();      //
        /// <summary>
        /// number of data points for all meteo-stations used for Match to Observation GRAMM wind fields
        /// </summary>
        public List<int> MetFileLenght = new List<int>();      //
        /// <summary>
        /// date of meteo-stations used for Match to Observation GRAMM wind fields
        /// </summary>
        public List<string[]> DateObsMetFile = new List<string[]>();      //
        /// <summary>
        /// hours of meteo-stations used for Match to Observation GRAMM wind fields
        /// </summary>
        public List<int[]> HourObsMetFile = new List<int[]>();      //
        /// <summary>
        /// Decimal separator of meteo-stations used for Match to Observation GRAMM wind fields
        /// </summary>
        public List<int> DecsepUser = new List<int>();      //
        /// <summary>
        /// Row separator of meteo-stations used for Match to Observation GRAMM wind fields
        /// </summary>
        public List<int> RowsepUser = new List<int>();      //
        /// <summary>
        /// time stamps of meteo-stations used for Match to Observation GRAMM wind fields
        /// </summary>
        public List<string[]> TimeStapmsMetTimeSeries = new List<string[]>();      //

        public bool LocalStabilityUsed = false;
        public int Match_Mode; // mode 0 = start Match to Observation process, 1 = tune match process, -2 cancel

        public bool Remove_Outliers;

        public event StartMatchingDelegate StartMatchProcess;
        public event CancelMatchingDelegate CancelMatchProcess;
        public event FinishMatchingDelegate FinishMatchProcess;

        public event LoadWindFileData LoadWindData;

        public bool CloseMatchDialogAllowed = false;

        /// <summary>
        /// u component for each meteo station and meteo situation
        /// </summary>
        public double[,] UGramm;
        /// <summary>
        /// v component for each meteo station and meteo situation
        /// </summary>
        public double[,] VGramm;
        /// <summary>
        /// lenght of wind vector for each meteo station and meteo situation
        /// </summary>
        public double[,] WGramm;
        /// <summary>
        /// Local stability class for each meteo station and meteo situation
        /// </summary>
        public int[,] LocalStabilityClass;
        /// <summary>
        /// Remember orignial header of meteopgt.all
        /// </summary>
        public string[] MeteoOriginalHeader;
        /// <summary>
        /// Wind direction
        /// </summary>
        public int[,] WindDir;
        /// <summary>
        /// Wind velocity from meteopgt.all
        /// </summary>
        public double[] WindVelMeteoPGT;
        /// <summary>
        /// Wind direction from meteopgt.all
        /// </summary>
        public double[] WindDirMeteoPGT;
        /// <summary>
        /// Stability class from meteopgt.all
        /// </summary>
        public int[] StabClassMeteoPGT;
        /// <summary>
        /// Height for each meteo station
        /// </summary>
        public double[] MeteoStationHeight;

        /// <summary>
        /// Tuned MetTimeSeries data
        /// </summary>
        public List<string> MettimeSeriesTuned;

        /// <summary>
        /// User or programmatically set Match to Observation data
        /// </summary>
        public MatchMultipleObservationsData MatchingData;

        /// <summary>
        /// Pointer from each meteo station time series entry to the time series with same time stamp of station 0
        /// </summary>
        private int[][] TimeSeriesPointer = null;

        /// <summary>
        /// Dialog for the match to observation process
        /// </summary>
        public MatchMultipleObservations()
        {
            InitializeComponent();
            //User defined column seperator and decimal seperator
            decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

            //create data grid view for input of emission measurements
            spaltenbezeichnungen.Clear();
            CreateDataTable(0, 0, spaltenbezeichnungen);

#if __MonoCS__
            var allNumUpDowns = Gral.Main.GetAllControls<NumericUpDown>(this);
            foreach (NumericUpDown nu in allNumUpDowns)
            {
                nu.TextAlign = HorizontalAlignment.Left;
            }
#endif
        }

        /// <summary>
        /// create data grid view for input of emission measurements
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="headers"></param>
        private void CreateDataTable(int row, int column, List<string> headers)
        {
            //zuerst löschen der data grid view
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            //erzeugen der neuen data grid view
            dataGridView1.Columns.Add("Name", "Name");
            dataGridView1.Columns.Add("x-coord.", "x-coord.");
            dataGridView1.Columns.Add("y-coord.", "y-coord.");
            dataGridView1.Columns.Add("Height(m)", "Height(m)");
            dataGridView1.Columns.Add("Start", "Start");
            dataGridView1.Columns.Add("End", "End");
            dataGridView1.Columns.Add("Weighting Factor", "Weighting Factor");
            dataGridView1.Columns.Add("Direction Factor", "Direction Factor");
            dataGridView1.Columns.Add("Auto Tuning Factor", "Auto Tuning Factor");
            dataGridView1.Columns.Add("V 10%", "V 10%");
            dataGridView1.Columns.Add("V 20%", "V 20%");
            dataGridView1.Columns.Add("V 40%", "V 40%");
            dataGridView1.Columns.Add("V 60%", "V 60%");
            dataGridView1.Columns.Add("SC 0", "SC 0");
            dataGridView1.Columns.Add("SC 1", "SC 1");

            //verhindern dass spalten sortiert werden können
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            for (int i = 0; i < row; i++)
            {
                dataGridView1.Rows.Add();
            }
            for (int i = 0; i < column; i++)
            {
                dataGridView1.Columns.Add(headers[i], headers[i]);
            }
            dataGridView1.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns[8].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[1].ReadOnly = true;
            dataGridView1.Columns[2].ReadOnly = true;
            dataGridView1.Columns[4].ReadOnly = true;
            dataGridView1.Columns[5].ReadOnly = true;
            dataGridView1.Columns[9].ReadOnly = true;
            dataGridView1.Columns[10].ReadOnly = true;
            dataGridView1.Columns[11].ReadOnly = true;
            dataGridView1.Columns[12].ReadOnly = true;
            dataGridView1.Columns[13].ReadOnly = true;
            dataGridView1.Columns[14].ReadOnly = true;

            dataGridView1.Columns[8].DefaultCellStyle.BackColor = System.Drawing.Color.WhiteSmoke;
        }

        /// <summary>
        /// Change the selection of the datagridview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DataGridView1SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0) // no rows selected
            {
                RemoveLine = -999;
            }
            else
            {
                RemoveLine = dataGridView1.SelectedRows[0].Index;
            }
        }

        /// <summary>
        /// delete one line (a meteo station)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (RemoveLine > -1 && RemoveLine <= dataGridView1.Rows.Count)
            {
                if (GralStaticFunctions.St_F.InputBox("Warning", "Do you really want to remove this station?", this) == DialogResult.OK)
                {
                    try
                    {
                        int removeline_temp = RemoveLine; // removeline is changed by Rows.Remove() 18.12.2017
                        dataGridView1.Rows.Remove(dataGridView1.Rows[RemoveLine]);

                        if (removeline_temp < MetFileLenght.Count) // Kuntner 18.12.2017
                        {
                            //löschen der Daten der meteo-station
                            WindVelocityObs.RemoveAt(removeline_temp);
                            WindDirectionObs.RemoveAt(removeline_temp);
                            MetFileLenght.RemoveAt(removeline_temp);
                            MetFileNames.RemoveAt(removeline_temp);
                            TimeStapmsMetTimeSeries.RemoveAt(removeline_temp);
                            DateObsMetFile.RemoveAt(removeline_temp);
                            HourObsMetFile.RemoveAt(removeline_temp);
                            DecsepUser.RemoveAt(removeline_temp);
                            RowsepUser.RemoveAt(removeline_temp);
                            StabilityClassObs.RemoveAt(removeline_temp);
                        }
                        //if (dataGridView1.RowCount < 1) // Kuntner: check if just one line does exist!
                        //{
                        RemoveLine = -999; // KuntnerReset the DataGridView
                        dataGridView1.ClearSelection();
                        //}
                    }
                    catch
                    { }
                }
            }
            else if (RemoveLine == -999)
            {
                MessageBox.Show(this, "No station selected - click again on the line to be removed", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Start the Match to Observation process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            if (dataGridView1.RowCount > 0) // if a meteo line exist
            {
                StartMatch = true;
                LocalStabilityUsed = checkBox2.Checked;
                Hide();
                Cursor = Cursors.WaitCursor;

                if (Match_Mode == 0) // first call
                {
                    MatchingData = FillUserMatchingData(new MatchMultipleObservationsData());
                    // send delegate - Message to domain Form, that match process should start
                    try
                    {
                        // Set default matching data
                        if (StartMatchProcess != null)
                        {
                            StartMatchProcess(this, e, ref MatchingData);
                        }
                        StartUserMatchTuning();
                        groupBox2.Enabled = true;
                    }
                    catch
                    { }
                }
                else if (Match_Mode >= 1) // tune finished
                {
                    Hide();
                    // send delegate - Message to domain Form, that match process should finish
                    try
                    {
                        if (FinishMatchProcess != null)
                        {
                            FinishMatchProcess(this, e, MatchingData);
                        }
                    }
                    catch
                    { }
                }
            }
        }

        /// <summary>
        /// cancel the MMO process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            StartMatch = false;
            Cursor = Cursors.Default;
            Match_Mode = 0;
            StabilityClassObs.Clear();
            MetFileNames.Clear();
            MetFileLenght.Clear();
            DateObsMetFile.Clear();
            HourObsMetFile.Clear();
            DecsepUser.Clear();
            RowsepUser.Clear();
            TimeStapmsMetTimeSeries.Clear();
            dataGridView1.Rows.Clear();
            groupBox2.Enabled = false;
            
            // send delegate - Message to domain Form, that match process should finish
            try
            {
                if (CancelMatchProcess != null)
                {
                    CancelMatchProcess(this, e);
                }
            }
            catch
            { }
        }

        private void Match_Multiple_Observations_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = false;

            if (MatchingData != null && MatchingData.Optimization == 2)
            {
                radioButton2.Checked = true;
            }
            else
            {
                radioButton1.Checked = true;
            }
            //enable support for high contrast themes
            if (System.Windows.SystemParameters.HighContrast || Gral.Main.GUISettings.UseDefaultColors)
            {
                Gral.Main.LoopAllControls(this.Controls);
                this.BackColor = System.Drawing.SystemColors.Window;
            }
            Domain.CancellationTokenReset();
        }

        /// <summary>
        /// Calculates the number of unique weather situations based on the mettime series for a classified meteorology
        /// </summary>
        /// <param name="MetTimeSeries">List for a MetTimeSeries</param>
        /// <returns>Number of unique weather situations in the mettime series</returns>
        private int CalculateUsedNumberOfWeatherSituations(List<string> MetTimeSeries)
        {
            List<string> _meteoSituations = new List<string>();
            if (MetTimeSeries != null)
            {
                foreach (string _mettime in MetTimeSeries)
                {
                    string[] text = _mettime.Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (text.Length > 4)
                    {
                        string recentMeteo = text[2] + "," + text[3] + "," + text[4];
                        //search if this text exits in the meteoSituations
                        bool exists = false;
                        foreach (string compare in _meteoSituations)
                        {
                            if (compare.Equals(recentMeteo))
                            {
                                exists = true;
                                break;
                            }
                        }
                        if (!exists)
                        {
                            _meteoSituations.Add(recentMeteo);
                        }
                    }
                }
            }
            return _meteoSituations.Count;
        }

        /// <summary>
        /// Show the original or the matched windrose of selected meteo-file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (RemoveLine > -1 && RemoveLine <= dataGridView1.Rows.Count)
            {
                List<WindData> wind = new List<WindData>();
                double[,] sectFrequency = new double[16, 8];
                int count = 0;
                int startstunde = 0;
                int endstunden = 23;
                string metFileName = string.Empty;

                double[] wndclasses = new double[7] { 0.5, 1, 2, 3, 4, 5, 6 };

                if (RemoveLine < MetFileLenght.Count) // Kuntner 18.12.2017
                {
                    // October 2020: Kuntner: analyze and show matched wind rose
                    Button _btn = (Button)sender;
                    if (_btn == button9 || _btn == button10)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        try
                        {
                            metFileName = "Matched: " + Path.GetFileName(MetFileNames[RemoveLine]);
                            //load mettime series
                            List<string> _mettimeSeries = new List<string>();
                            if (File.Exists(Path.Combine(Gral.Main.ProjectName, "Computation", "mettimeseries.dat")))
                            {
                                List<string> _meteopgt = new List<string>();
                                GralBackgroundworkers.BackgroundworkerData _bwdata = new GralBackgroundworkers.BackgroundworkerData();
                                GralBackgroundworkers.ProgressFormBackgroundworker bgw = new GralBackgroundworkers.ProgressFormBackgroundworker(_bwdata);
                                //read new mettimeseries and original meteopgt.all
                                if (bgw.ReadMettimeseries(Path.Combine(Gral.Main.ProjectName, "Computation", "mettimeseries.dat"), ref _mettimeSeries) &&
                                    bgw.ReadMeteopgtAll(Path.Combine(GRAMMPath, @"meteopgt.all"), ref _meteopgt))
                                {
                                    // create windclasses for the meteo preview
                                    if (_btn == button9)
                                    {
                                        for (int i = 0; i < _mettimeSeries.Count; i++)
                                        {
                                            int index = bgw.SearchCorrespondingMeteopgtAllSituation(_mettimeSeries, _meteopgt, i) + 1; //+1 because match arrays are starting with index 1
                                            double uGramm = UGramm[RemoveLine, index];
                                            double vGramm = VGramm[RemoveLine, index];
                                            if (uGramm > -999 && vGramm > -999)
                                            {
                                                double vel = WGramm[RemoveLine, index];
                                                float dir = bgw.WindDirection(uGramm, vGramm);
                                                int sektor = Convert.ToInt32(Math.Round(dir / 22.5, 0));
                                                int wklass = 0;

                                                for (int c = 0; c < 6; c++)
                                                {
                                                    if (vel > wndclasses[c] && vel <= wndclasses[c + 1])
                                                    {
                                                        wklass = c + 1;
                                                    }
                                                }
                                                if (vel <= wndclasses[0])
                                                {
                                                    wklass = 0;
                                                }
                                                if (vel > wndclasses[6])
                                                {
                                                    wklass = 7;
                                                }
                                                if (sektor > 15)
                                                {
                                                    sektor = 0;
                                                }
                                                count += 1;
                                                sectFrequency[sektor, wklass]++;
                                                WindData date = new WindData
                                                {
                                                    Date = DateObsMetFile[0][i],
                                                    Vel = vel,
                                                    Dir = dir,
                                                    Hour = HourObsMetFile[0][i]
                                                };
                                                if (date.Hour == 24) // if met-file contains 24:00 instead of 00:00
                                                {
                                                    date.Hour = 0;
                                                }
                                                date.StabClass = LocalStabilityClass[RemoveLine, index];
                                                wind.Add(date);
                                            }
                                        }
                                    }
                                    else if (_btn == button10)
                                    {
                                        // create a meteo file 
                                        string filename = "MatchPreview_" + Path.GetFileName(MetFileNames[RemoveLine]);
                                        
                                        System.Globalization.CultureInfo ic = System.Globalization.CultureInfo.InvariantCulture;
                                        //write new meteo file
                                        using (StreamWriter writer = new StreamWriter(Path.Combine(Gral.Main.ProjectName, "Metfiles", filename)))
                                        {
                                            string[] text;
                                            string[] date;
                                            for (int i = 0; i < _mettimeSeries.Count; i++)
                                            {
                                                int index = bgw.SearchCorrespondingMeteopgtAllSituation(_mettimeSeries, _meteopgt, i) + 1; //+1 because match arrays are starting with index 1
                                                double uGramm = UGramm[RemoveLine, index];
                                                double vGramm = VGramm[RemoveLine, index];
                                                if (uGramm > -999 && vGramm > -999)
                                                {
                                                    double vel = WGramm[RemoveLine, index];
                                                    float dir = bgw.WindDirection(uGramm, vGramm);
                                                    int stabClass = LocalStabilityClass[RemoveLine, index];
                                                    text = _mettimeSeries[i].Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                                    date = text[0].Split('.');
                                                    DateTime _date = new DateTime(DateTime.Now.Year, Convert.ToInt32(date[1]), Convert.ToInt32(date[0]), Convert.ToInt32(text[1]), 0, 0);
                                                    writer.WriteLine(_date.ToString("d", CultureInfo.CreateSpecificCulture("de-DE")) + "," +
                                                                     _date.ToString("t", CultureInfo.CreateSpecificCulture("de-DE")) + "," +
                                                                     Math.Round(vel, 1).ToString(ic) + "," + Math.Round(dir).ToString(ic) + "," + stabClass.ToString(ic));
                                                }
                                            }
                                        }
                                        count = 0;
                                    }
                                }
                                _bwdata = null;
                                bgw = null;
                            }
                        }
                        catch
                        {
                            count = 0;
                        }
                        this.Cursor = Cursors.Default;
                    }
                    else
                    {
                        metFileName = Path.GetFileName(MetFileNames[RemoveLine]);
                        for (int i = 0; i <= MetFileLenght[RemoveLine] - 1; i++)
                        {
                            //wind rose for a certain time interval within a day
                            int sektor = Convert.ToInt32(Math.Round(WindDirectionObs[RemoveLine][i] / 22.5, 0));
                            int wklass = 0; //Convert.ToInt32(Math.Truncate(wind_speeds[removeline][i])) + 1;
                            double vel = WindVelocityObs[RemoveLine][i];

                            for (int c = 0; c < 6; c++)
                            {
                                if (vel > wndclasses[c] && vel <= wndclasses[c + 1])
                                {
                                    wklass = c + 1;
                                }
                            }

                            if (vel <= wndclasses[0])
                            {
                                wklass = 0;
                            }

                            if (vel > wndclasses[6])
                            {
                                wklass = 7;
                            }

                            if (sektor > 15)
                            {
                                sektor = 0;
                            }

                            count += 1;

                            sectFrequency[sektor, wklass]++;

                            WindData date = new WindData
                            {
                                Date = DateObsMetFile[RemoveLine][i],
                                Vel = WindVelocityObs[RemoveLine][i],
                                Dir = WindDirectionObs[RemoveLine][i],
                                Hour = HourObsMetFile[RemoveLine][i]
                            };
                            if (date.Hour == 24) // if met-file contains 24:00 instead of 00:00
                            {
                                date.Hour = 0;
                            }

                            date.StabClass = StabilityClassObs[RemoveLine][i];
                            wind.Add(date);
                        }
                    }

                    if (count > 0)
                    {
                        for (int sektor = 0; sektor < 16; sektor++)
                        {
                            for (int wklass = 0; wklass < 8; wklass++)
                            {
                                sectFrequency[sektor, wklass] = sectFrequency[sektor, wklass] / Convert.ToDouble(count);
                            }
                        }

                        GralMainForms.Windrose windrose = new GralMainForms.Windrose
                        {
                            SectFrequ = sectFrequency,
                            MetFileName = metFileName,
                            WindData = wind,
                            StartHour = startstunde,
                            FinalHour = endstunden,
                            WndClasses = wndclasses,
                            DrawingMode = 0
                        };
                        windrose.Show();
                        windrose.BringToFront();
                    }
                }
                else
                {
                    MessageBox.Show(this, "No wind data available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show(this, "No data set selected - click on a line in the table", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        void Match_Multiple_ObservationsVisibleChanged(object sender, EventArgs e)
        {
            if (LocalStabilityUsed)
            {
                checkBox2.Checked = true;
                checkBox2.Enabled = true;
            }
            else
            {
                checkBox2.Checked = false;
                checkBox2.Enabled = false;
            }
            // Check mode 0 = select meteo stations, 1 = modify parameters
            if (Match_Mode == 0)
            {
                button5.Text = "&Start";
                toolTip1.SetToolTip(button5, "Start the procedure");
                button7.Text = "&Add station";
                toolTip1.SetToolTip(button7, "Add a meteorological station");
                button4.Visible = true;
                button2.Visible = true;
                checkBox2.Visible = true;
                checkBox4.Enabled = true;
                // reset the time series pointer
                TimeSeriesPointer = null;
                // reset auto mode passes
                checkBox5.Checked = true;
                checkBox6.Checked = false;
            }

            if (Match_Mode == 1)
            {
                checkBox4.Enabled = false;
                button5.Text = "&Finish";
                toolTip1.SetToolTip(button5, "Finish the Match to Observation process");
                button7.Text = "&Repeat";
                toolTip1.SetToolTip(button7, "Repeat the Match to Observation process");
                button4.Visible = false;
                button2.Visible = false;
                checkBox2.Visible = false;
                dataGridView1.Columns[3].ReadOnly = true; // Change height not alowed if match is tuned

                if (MatchingData.Optimization == 2)
                {
                    radioButton2.Checked = true;
                }
                else if (MatchingData.Optimization == 1)
                {
                    radioButton1.Checked = true;
                }

                if (Remove_Outliers)
                {
                    checkBox1.Checked = true;
                }
                else
                {
                    checkBox1.Checked = false;
                }

                dataGridView1.Enabled = true;

                Match_Mode = 2; // don't change values if form is hidden
            }

            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Save MMO Files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button1Click(object sender, EventArgs e) // Save match data to file
        {
            using (SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "MMO-Files (*.mmo)|*.mmo",
                Title = "Save match to oberservation data",
                InitialDirectory = _settings_path
#if NET6_0_OR_GREATER
                ,ClientGuid = GralStaticFunctions.St_F.FileDialogSettings
#endif
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string filename = dialog.FileName;
                    try
                    {
                        // Choose whether to write header. Use EnableWithoutHeaderText instead to omit header.
                        dataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
                        // Select all the cells
                        dataGridView1.SelectAll();
                        // Copy (set clipboard)
                        Clipboard.SetDataObject(dataGridView1.GetClipboardContent());

                        using (StreamWriter sr = new StreamWriter(filename))
                        {
                            sr.WriteLine("MMO Settings3");
                            sr.WriteLine(checkBox1.Checked.ToString());
                            sr.WriteLine(checkBox2.Checked.ToString());
                            sr.WriteLine(concatenate.Value.ToString(CultureInfo.InvariantCulture));
                            sr.WriteLine(radioButton1.Checked.ToString());
                            sr.WriteLine(radioButton2.Checked.ToString());
                            sr.WriteLine(checkBox3.Checked.ToString());
                            sr.WriteLine(checkBox4.Checked.ToString());
                            sr.WriteLine(numericUpDown2.Value.ToString());

                            // Paste (get the clipboard and serialize it to a file)
                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                int i = 0;
                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    if (i < 9)
                                    {
                                        if (double.TryParse(cell.Value.ToString(), out double _val))
                                        {
                                            sr.Write(_val.ToString(CultureInfo.InvariantCulture) + "\t");
                                        }
                                        else
                                        {
                                            sr.Write(cell.Value.ToString() + "\t");
                                        }
                                    }
                                    i++;
                                }
                                sr.Write(MetFileNames[row.Index] + "\t");
                                sr.Write(DecsepUser[row.Index].ToString() + "\t");
                                sr.WriteLine(RowsepUser[row.Index].ToString() + "\t");
                            }
                        }
                    }
                    catch (Exception ex)
                    { MessageBox.Show(this, ex.Message.ToString()); }
                }
            }
        }

        /// <summary>
        /// Load MMO Files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button4Click(object sender, EventArgs e)
        {
            char rowsep = ',';
            string decsepuser = ".";
            int MMOFileFormat = 0;

            using (OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "MMO-Files (*.mmo)|*.mmo",
                Title = "Load match to oberservation data",
                InitialDirectory = _settings_path
#if NET6_0_OR_GREATER
                ,ClientGuid = GralStaticFunctions.St_F.FileDialogSettings
#endif
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string filename = dialog.FileName;
                    try
                    {
                        using (StreamReader sr = new StreamReader(filename))
                        {
                            bool t = false;
                            decimal v = 0;

                            string temp = sr.ReadLine();
                            if (temp == "MMO Settings")
                            {
                                MMOFileFormat = 1;
                                temp = sr.ReadLine();
                            }
                            else if (temp.Equals("MMO Settings2"))
                            {
                                MMOFileFormat = 2;
                                temp = sr.ReadLine();
                            }
                            else if (temp.Equals("MMO Settings3"))
                            {
                                MMOFileFormat = 3;
                                temp = sr.ReadLine();
                            }
                            bool.TryParse(temp, out t);
                            checkBox1.Checked = t;

                            temp = sr.ReadLine();
                            bool.TryParse(temp, out t);
                            checkBox2.Checked = t;

                            temp = sr.ReadLine().Replace(".", decsep);
                            decimal.TryParse(temp, out v);
                            concatenate.Value = v;

                            temp = sr.ReadLine();
                            bool.TryParse(temp, out t);
                            radioButton1.Checked = t;

                            temp = sr.ReadLine();
                            bool.TryParse(temp, out t);
                            radioButton2.Checked = t;

                            if (MMOFileFormat > 1)
                            {
                                temp = sr.ReadLine();
                                bool.TryParse(temp, out t);
                                checkBox3.Checked = t;
                                temp = sr.ReadLine();
                                bool.TryParse(temp, out t);
                                checkBox4.Checked = t;
                            }

                            if (MMOFileFormat > 2)
                            {
                                temp = sr.ReadLine().Replace(".", decsep); 
                                decimal.TryParse(temp, out v);
                                numericUpDown2.Value = v;
                            }

                            spaltenbezeichnungen.Clear();
                            CreateDataTable(0, 0, spaltenbezeichnungen);// delete all data from the datagridview
                            WindVelocityObs.Clear();
                            WindDirectionObs.Clear();
                            MetFileLenght.Clear();
                            MetFileNames.Clear();
                            TimeStapmsMetTimeSeries.Clear();
                            DateObsMetFile.Clear();
                            HourObsMetFile.Clear();
                            RowsepUser.Clear();
                            DecsepUser.Clear();
                            StabilityClassObs.Clear();

                            while (sr.EndOfStream == false)
                            {
                                bool data_loaded = false;
                                // Paste data to the datagridview
                                string[] data = sr.ReadLine().Split('\t');
                                if (data.Length > 6)
                                {
                                    dataGridView1.Rows.Add();
                                    int zeilenindex = dataGridView1.Rows.Count - 1;
                                    dataGridView1.Rows[zeilenindex].Cells[0].Value = data[0];
                                    dataGridView1.Rows[zeilenindex].Cells[1].Value = Convert.ToInt32(data[1]);
                                    dataGridView1.Rows[zeilenindex].Cells[2].Value = Convert.ToInt32(data[2]);
                                    dataGridView1.Rows[zeilenindex].Cells[3].Value = Convert.ToDouble(data[3].Replace(".", decsep));
                                    dataGridView1.Rows[zeilenindex].Cells[4].Value = data[4];
                                    dataGridView1.Rows[zeilenindex].Cells[5].Value = data[5];
                                    dataGridView1.Rows[zeilenindex].Cells[6].Value = Convert.ToDouble(data[6].Replace(".", decsep));
                                    int line = 7;
                                    if (MMOFileFormat > 0)
                                    {
                                        dataGridView1.Rows[zeilenindex].Cells[7].Value = Convert.ToDouble(data[line++]);
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[zeilenindex].Cells[7].Value = 1;
                                    }

                                    //Auto Mode factor
                                    if (MMOFileFormat > 2)
                                    {
                                        dataGridView1.Rows[zeilenindex].Cells[8].Value = Convert.ToDouble(data[line++]); ;
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[zeilenindex].Cells[8].Value = 1;
                                    }

                                    string windfilename = data[line];
                                    line++;

                                    if (data.Length > 8)
                                    {
                                        try
                                        {
                                            int tempasc = Convert.ToInt32(data[line + 1]);
                                            rowsep = Convert.ToChar(tempasc);
                                            tempasc = Convert.ToInt32(data[line]);
                                            char tempchar = Convert.ToChar(tempasc);
                                            decsepuser = Convert.ToString(tempchar);
                                        }
                                        catch
                                        { }
                                    }

                                    // Read meteo data from files
                                    if (File.Exists(windfilename) == false)
                                    {
                                        OpenFileDialog dialog_winddata = new OpenFileDialog
                                        {
                                            Title = Path.GetFileName(windfilename) +
                                                " file not found - please select the file",
                                            FileName = Path.GetFileName(windfilename)
#if NET6_0_OR_GREATER
                                            ,ClientGuid = GralStaticFunctions.St_F.FileDialogSettings
#endif
                                        };

                                        if (dialog_winddata.ShowDialog() == DialogResult.OK)
                                        {
                                            windfilename = dialog_winddata.FileName;
                                        }
                                    }

                                    // read wind data from file
                                    if (File.Exists(windfilename))
                                    {
                                        bool ok = true;

                                        List<WindData> winddata = new List<WindData>();
                                        IO_ReadFiles readwindfile = new IO_ReadFiles
                                        {
                                            WindDataFile = windfilename,
                                            WindData = winddata
                                        };

                                        if (readwindfile.ReadMeteoFiles(1000000, rowsep, decsep, decsepuser, Gral.Main.GUISettings.IgnoreMeteo00Values) == false)
                                        {
                                            MessageBox.Show(this, "Error when reading Meteo-File " + Path.GetFileName(windfilename) +
                                                            " in line" + winddata.Count, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            if (winddata.Count == 0)
                                            {
                                                ok = false;
                                            }
                                        }

                                        winddata = readwindfile.WindData;
                                        readwindfile = null;
                                        int wind_length = winddata.Count;

                                        if (ok && wind_length > 0) // wind data ok
                                        {
                                            //add met data
                                            WindVelocityObs.Add(new double[wind_length]);
                                            WindDirectionObs.Add(new double[wind_length]);
                                            MetFileLenght.Add(0);
                                            MetFileNames.Add(windfilename);
                                            TimeStapmsMetTimeSeries.Add(new string[wind_length]);
                                            DateObsMetFile.Add(new string[wind_length]);
                                            HourObsMetFile.Add(new int[wind_length]);
                                            DecsepUser.Add(0);
                                            RowsepUser.Add(0);
                                            StabilityClassObs.Add(new int[wind_length]);
                                            MetFileLenght[zeilenindex] = wind_length;

                                            char tempchar = char.Parse(decsepuser);
                                            DecsepUser[zeilenindex] = (int)tempchar;
                                            RowsepUser[zeilenindex] = (int)rowsep;

                                            int length = 0;
                                            foreach (WindData wd in winddata)
                                            {
                                                HourObsMetFile[zeilenindex][length] = wd.Hour;
                                                WindVelocityObs[zeilenindex][length] = wd.Vel;
                                                WindDirectionObs[zeilenindex][length] = wd.Dir;
                                                DateObsMetFile[zeilenindex][length] = wd.Date;
                                                TimeStapmsMetTimeSeries[zeilenindex][length] = wd.Time;
                                                StabilityClassObs[zeilenindex][length] = wd.StabClass;
                                                length++;
                                            }
                                            data_loaded = true;
                                        }
                                        else
                                        {
                                            MessageBox.Show(this, "Error when reading Meteo-File " + Path.GetFileName(windfilename), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                        winddata = null;

                                    }
                                }
                                if (data_loaded == false) // loading failed - > remove line
                                {
                                    dataGridView1.Rows.RemoveAt(dataGridView1.Rows.Count);
                                }
                            } // Loop over all meteo stations

                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "GRAL GUI");
                    }
                }
            }
        }

        void Match_Multiple_ObservationsFormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (spaltenbezeichnungen != null)
                {
                    spaltenbezeichnungen.Clear();
                    spaltenbezeichnungen.TrimExcess();
                }
                if (WindVelocityObs != null)
                {
                    WindVelocityObs.Clear();
                    WindVelocityObs.TrimExcess();
                }
                if (WindDirectionObs != null)
                {
                    WindDirectionObs.Clear();
                    WindDirectionObs.TrimExcess();
                }
                if (StabilityClassObs != null)
                {
                    StabilityClassObs.Clear();
                    StabilityClassObs.TrimExcess();
                }
                if (MetFileNames != null)
                {
                    MetFileNames.Clear();
                    MetFileNames.TrimExcess();
                }
                if (MetFileLenght != null)
                {
                    MetFileLenght.Clear();
                    MetFileLenght.TrimExcess();
                }
                if (DateObsMetFile != null)
                {
                    DateObsMetFile.Clear();
                    DateObsMetFile.TrimExcess();
                }
                if (HourObsMetFile != null)
                {
                    HourObsMetFile.Clear();
                    HourObsMetFile.TrimExcess();
                }
                if (DecsepUser != null)
                {
                    DecsepUser.Clear();
                    DecsepUser.TrimExcess();
                }
                if (RowsepUser != null)
                {
                    RowsepUser.Clear();
                    RowsepUser.TrimExcess();
                }
                if (TimeStapmsMetTimeSeries != null)
                {
                    TimeStapmsMetTimeSeries.Clear();
                    TimeStapmsMetTimeSeries.TrimExcess();
                }
                if (dataGridView1 != null)
                {
                    dataGridView1.Columns.Clear();
                    dataGridView1.Dispose();
                }
            }
            catch
            { }
            toolTip1.Dispose();
        }

        /// <summary>
        /// Do not allow to close this form as long Domain() is open!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Match_Multiple_ObservationsFormClosing(object sender, FormClosingEventArgs e)
        {
            // close_allowed is set to true at DomainFormClosed()
            if (CloseMatchDialogAllowed == false)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Add a line (a station) or repeat the tuning process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button7Click(object sender, EventArgs e)
        {
            Button bt = sender as Button;

            if (bt.Text.Equals("&Add station"))
            {
                RemoveLine = -999;
                dataGridView1.ClearSelection();
                // send delegate - Message to domain Form, that wind data should be selected by the user
                try
                {
                    if (LoadWindData != null)
                    {
                        LoadWindData(this, e);
                    }
                    int lastRowIndex = dataGridView1.Rows.Count - 1;
                    if (lastRowIndex > 0)
                    {
                        dataGridView1.CurrentCell = dataGridView1.Rows[lastRowIndex].Cells[0];
                    }
                }
                catch
                { }
                this.Show();
                this.BringToFront();
            }
            if (bt.Text.Equals("&Repeat"))
            {
                if (Match_Mode >= 1) // tune mode
                {
                    Hide();
                    MatchingData = FillUserMatchingData(MatchingData);
                    StartUserMatchTuning();
                }
            }
        }

        /// <summary>
        /// Fill a MatchData class with user defined match settings
        /// </summary>
        /// <param name="_dta"></param>
        /// <returns></returns>
        private MatchMultipleObservationsData FillUserMatchingData(MatchMultipleObservationsData _dta)
        {
            if (radioButton1.Checked == true)
            {
                _dta.Optimization = 1;
            }
            if (radioButton2.Checked == true)
            {
                _dta.Optimization = 2;
            }
            if (checkBox1.Checked == false)
            {
                _dta.Outliers = false;
            }
            else
            {
                _dta.Outliers = true;
            }
            if (checkBox3.Checked == false)
            {
                _dta.StrongerWeightedSC1AndSC7 = false;
            }
            else
            {
                _dta.StrongerWeightedSC1AndSC7 = true;
            }
            // vectorial auto mode
            _dta.AtomaticModePasses = 0;
            if (checkBox5.Checked)
            {
                _dta.AtomaticModePasses = 1;
            }
            // componentes auto mode
            if (checkBox6.Checked)
            {
                _dta.AtomaticModePasses = _dta.AtomaticModePasses + 2;
            }
            // iterative auto mode
            if (checkBox7.Checked)
            {
                _dta.AtomaticModePasses = _dta.AtomaticModePasses + 4;
            }

            _dta.ReduceSituations = (int)numericUpDown2.Value;
            _dta.WeightingFactor = new double[MetFileNames.Count + 1];
            _dta.WeightingDirection = new double[MetFileNames.Count + 1];
            _dta.WeightingAutoMode = new double[MetFileNames.Count + 1];
            _dta.VectorErrorSum = new int[MetFileNames.Count, 4];
            _dta.SCErrorSum = new int[MetFileNames.Count, 2];

            for (int i = 0; i < MetFileNames.Count; i++)
            {
                _dta.WeightingFactor[i] = 1;
                _dta.WeightingDirection[i] = 1;
            }

            try
            {
                for (int i = 0; i < MetFileNames.Count; i++)
                {
                    _dta.WeightingFactor[i] = GralStaticFunctions.St_F.TxtToDbl(dataGridView1.Rows[i].Cells[6].Value.ToString(), false);
                    _dta.WeightingFactor[i] = Math.Max(_dta.WeightingFactor[i], 0.0);
                    _dta.WeightingDirection[i] = GralStaticFunctions.St_F.TxtToDbl(dataGridView1.Rows[i].Cells[7].Value.ToString(), false);
                    _dta.WeightingDirection[i] = Math.Min(10, Math.Max(_dta.WeightingDirection[i], 0.0));
                    _dta.WeightingAutoMode[i] = GralStaticFunctions.St_F.TxtToDbl(dataGridView1.Rows[i].Cells[8].Value.ToString(), false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return _dta;
        }

        /// <summary>
        /// Write a mettime series based on the tuning results
        /// </summary>
        /// <param name="MetTimeSeries"></param>
        private bool WriteMetTimeSeries(List<string> MetTimeSeries)
        {
            if (MetTimeSeries != null)
            {
                try
                {
                    using (StreamWriter writeMetTimeSeries = File.CreateText(Path.Combine(Gral.Main.ProjectName, "Computation", "mettimeseries.dat")))
                    {
                        foreach (string _dta in MetTimeSeries)
                        {
                            writeMetTimeSeries.WriteLine(_dta);
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set the tuning results to the data grid view
        /// </summary>
        /// <param name="MatchingSettings"></param>
        private void SetTuningResults(MatchMultipleObservationsData MatchingSettings)
        {
            int TuningResultRowIndex = 9;
            for (int j = 0; j < MetFileNames.Count; j++) // write error values to datagrid
            {
                for (int i = 0; i < 4; i++) // 4 values
                {
                    dataGridView1.Rows[j].Cells[TuningResultRowIndex + i].Value = (100 * MatchingSettings.VectorErrorSum[j, i] / MetFileLenght[0]).ToString();
                }

                // error values for SC
                dataGridView1.Rows[j].Cells[TuningResultRowIndex + 4].Value = (100 * MatchingSettings.SCErrorSum[j, 0] / MetFileLenght[0]).ToString();
                dataGridView1.Rows[j].Cells[TuningResultRowIndex + 5].Value = (100 * MatchingSettings.SCErrorSum[j, 1] / MetFileLenght[0]).ToString();

                dataGridView1.Rows[j].Cells[6].Value = MatchingSettings.WeightingFactor[j];
                dataGridView1.Rows[j].Cells[7].Value = MatchingSettings.WeightingDirection[j];

                if (MatchingSettings.Optimization == 1)
                {
                    radioButton2.Checked = false;
                    radioButton1.Checked = true;
                }
                else if (MatchingSettings.Optimization == 2)
                {
                    radioButton1.Checked = false;
                    radioButton2.Checked = true;
                }
            }
            button9.Enabled = true;
        }

        /// <summary>
        /// Start the match tuning with user defined parameters
        /// </summary>
        private void StartUserMatchTuning()
        {
            //MessageBox.Show(MatchingData.PGT[1].PGTFrq.ToString() + "/" + MatchingData.WeightingFactor[0].ToString());
            Domain.CancellationTokenReset();
            (List<string> mettimeseries, int UsedWeatherSit) = MatchTuning(MatchingData, GralDomain.Domain.CancellationTokenSource.Token);
            WriteMetTimeSeries(mettimeseries);
            label6.Text = "Used situations: " + UsedWeatherSit.ToString();
            SetTuningResults(MatchingData);
            Show();
        }

        /// <summary>
        /// Automatic Tuning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            MatchingData = FillUserMatchingData(MatchingData);
            AutoTuning(MatchingData.AtomaticModePasses);
            checkBox5.Checked = false;
            checkBox6.Checked = false;
        }

        /// <summary>
        /// Change the scrollbar for tuning setting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            // Reset tuning mode
            checkBox5.Checked = true;
            //checkBox6.Checked = true;
        }
    }
}
