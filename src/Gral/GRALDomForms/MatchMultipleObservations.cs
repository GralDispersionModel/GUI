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
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using GralIO;
using GralData;
using System.Threading.Tasks;

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

        public List<double[]> wind_speeds = new List<double[]>();      //wind speed observations for all meteo-stations used for matching GRAMM wind fields
        public List<double[]> wind_direction = new List<double[]>();      //wind direction observations for all meteo-stations used for matching GRAMM wind fields
        public List<int[]> stability = new List<int[]>();      //stability for all meteo-stations used for matching GRAMM wind fields
        public List<string> metfiles = new List<string>();      //names of meteo-stations used for matching GRAMM wind fields
        public List<int> filelength = new List<int>();      //number of data points for all meteo-stations used for matching GRAMM wind fields
        public List<string[]> datum = new List<string[]>();      //datum of meteo-stations used for matching GRAMM wind fields
        public List<int[]> stunde = new List<int[]>();      //hours of meteo-stations used for matching GRAMM wind fields
        public List<int> DecsepUser = new List<int>();      //Dec separator of meteo-stations used for matching GRAMM wind fields
        public List<int> RowsepUser = new List<int>();      //Row separator of meteo-stations used for matching GRAMM wind fields
        public List<string[]> zeit = new List<string[]>();      //time stamps of meteo-stations used for matching GRAMM wind fields

        public bool Local_Stability = false;
        public int Match_Mode; // mode 0 = start matching process, 1 = tune match process, -2 cancel

        public bool Remove_Outliers;

        public event StartMatchingDelegate StartMatchProcess;
        public event CancelMatchingDelegate CancelMatchProcess;
        public event FinishMatchingDelegate FinishMatchProcess;

        public event LoadWindFileData LoadWindData;

        public bool close_allowed = false;

        /// <summary>
        /// u component for each meteo station and meteo situation
        /// </summary>
        public double[,] UGramm;
        /// <summary>
        /// v component for each meteo station and meteo situation
        /// </summary>
        public double[,] VGramm;
        /// <summary>
        /// w component for each meteo station and meteo situation
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
        /// User or programmatically set matching data
        /// </summary>
        public MatchMultipleObservationsData MatchingData;

        /// <summary>
        /// Frequency for each meteopgt entry
        /// </summary>
        //public List<GralData.PGTAll> PGT = new List<GralData.PGTAll>();

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
            create_table(0, 0, spaltenbezeichnungen);
        }

        //create data grid view for input of emission measurements
        private void create_table(int row, int column, List<string> headers)
        {
            //zuerst löschen der data grid view
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            //erzeugen der neuen data grid view
            dataGridView1.Columns.Add("Name", "Name");
            dataGridView1.Columns.Add("x-coord.", "x-coord.");
            dataGridView1.Columns.Add("y-coord.", "y-coord.");
            dataGridView1.Columns.Add("height(m)", "height(m)");
            dataGridView1.Columns.Add("Start", "Start");
            dataGridView1.Columns.Add("End", "End");
            dataGridView1.Columns.Add("Weighting factor", "Weighting factor");
            dataGridView1.Columns.Add("Direction factor", "Direction factor");
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

            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[1].ReadOnly = true;
            dataGridView1.Columns[2].ReadOnly = true;
            dataGridView1.Columns[4].ReadOnly = true;
            dataGridView1.Columns[5].ReadOnly = true;

            dataGridView1.Columns[8].ReadOnly = true;
            dataGridView1.Columns[9].ReadOnly = true;
            dataGridView1.Columns[10].ReadOnly = true;
            dataGridView1.Columns[11].ReadOnly = true;
            dataGridView1.Columns[12].ReadOnly = true;
            dataGridView1.Columns[13].ReadOnly = true;
        }

        // Change the selection of the datagridview
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

        //delete one line (a meteo station)
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

                        if (removeline_temp < filelength.Count) // Kuntner 18.12.2017
                        {
                            //löschen der Daten der meteo-station
                            wind_speeds.RemoveAt(removeline_temp);
                            wind_direction.RemoveAt(removeline_temp);
                            filelength.RemoveAt(removeline_temp);
                            metfiles.RemoveAt(removeline_temp);
                            zeit.RemoveAt(removeline_temp);
                            datum.RemoveAt(removeline_temp);
                            stunde.RemoveAt(removeline_temp);
                            DecsepUser.RemoveAt(removeline_temp);
                            RowsepUser.RemoveAt(removeline_temp);
                            stability.RemoveAt(removeline_temp);
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

        //start the procedure
        private void button5_Click(object sender, EventArgs e)
        {
            if (dataGridView1.RowCount > 0) // if a meteo line exist
            {
                StartMatch = true;
                Local_Stability = checkBox2.Checked;
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

        //cancel the procedure
        private void button6_Click(object sender, EventArgs e)
        {
            StartMatch = false;
            Cursor = Cursors.Default;
            Match_Mode = 0;
            stability.Clear();
            metfiles.Clear();
            filelength.Clear();
            datum.Clear();
            stunde.Clear();
            DecsepUser.Clear();
            RowsepUser.Clear();
            zeit.Clear();
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
        }

        //show windrose of selected meteo-file
        private void button3_Click(object sender, EventArgs e)
        {
            if (RemoveLine > -1 && RemoveLine <= dataGridView1.Rows.Count)
            {
                List<WindData> wind = new List<WindData>();
                double[,] sectFrequency = new double[16, 8];
                int count = 0;
                int startstunde = 0;
                int endstunden = 23;

                double[] wndclasses = new double[7] { 0.5, 1, 2, 3, 4, 5, 6 };

                if (RemoveLine < filelength.Count) // Kuntner 18.12.2017
                {
                    for (int i = 0; i <= filelength[RemoveLine] - 1; i++)
                    {
                        //wind rose for a certain time interval within a day
                        int sektor = Convert.ToInt32(Math.Round(wind_direction[RemoveLine][i] / 22.5, 0));
                        int wklass = 0; //Convert.ToInt32(Math.Truncate(wind_speeds[removeline][i])) + 1;
                        double vel = wind_speeds[RemoveLine][i];

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
                            Date = datum[RemoveLine][i],
                            Vel = wind_speeds[RemoveLine][i],
                            Dir = wind_direction[RemoveLine][i],
                            Hour = stunde[RemoveLine][i]
                        };
                        if (date.Hour == 24) // if met-file contains 24:00 instead of 00:00
                        {
                            date.Hour = 0;
                        }

                        date.StabClass = stability[RemoveLine][i];
                        wind.Add(date);
                    }

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
                        MetFileName = Path.GetFileName(metfiles[RemoveLine]),
                        WindData = wind,
                        StartHour = startstunde,
                        FinalHour = endstunden,
                        WndClasses = wndclasses,
                        DrawingMode = 0
                    };
                    windrose.Show();
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

            if (Local_Stability)
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
            }

            if (Match_Mode == 1)
            {
                checkBox4.Enabled = false;
                button5.Text = "&Finish";
                toolTip1.SetToolTip(button5, "Finish the matching process");
                button7.Text = "&Repeat";
                toolTip1.SetToolTip(button7, "Repeat the matching process");
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

        // Save MMO Files
        void Button1Click(object sender, EventArgs e) // Save match data to file
        {
            using (SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "MMO-Files (*.mmo)|*.mmo",
                Title = "Save match to oberservation data",
                InitialDirectory = _settings_path
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
                            sr.WriteLine("MMO Settings2");
                            sr.WriteLine(checkBox1.Checked.ToString());
                            sr.WriteLine(checkBox2.Checked.ToString());
                            sr.WriteLine(concatenate.Value.ToString());
                            sr.WriteLine(radioButton1.Checked.ToString());
                            sr.WriteLine(radioButton2.Checked.ToString());
                            sr.WriteLine(checkBox3.Checked.ToString());
                            sr.WriteLine(checkBox4.Checked.ToString());
                            // Paste (get the clipboard and serialize it to a file)
                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                int i = 0;
                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    if (i < 8)
                                    {
                                        sr.Write(cell.Value.ToString() + "\t");
                                    }
                                    i++;
                                }
                                sr.Write(metfiles[row.Index] + "\t");
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

        // Load MMO Files
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
                            bool.TryParse(temp, out t);
                            checkBox1.Checked = t;

                            temp = sr.ReadLine();
                            bool.TryParse(temp, out t);
                            checkBox2.Checked = t;

                            temp = sr.ReadLine();
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

                            spaltenbezeichnungen.Clear();
                            create_table(0, 0, spaltenbezeichnungen);// delete all data from the datagridview
                            wind_speeds.Clear();
                            wind_direction.Clear();
                            filelength.Clear();
                            metfiles.Clear();
                            zeit.Clear();
                            datum.Clear();
                            stunde.Clear();
                            RowsepUser.Clear();
                            DecsepUser.Clear();
                            stability.Clear();

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
                                    dataGridView1.Rows[zeilenindex].Cells[6].Value = Convert.ToDouble(data[6]);
                                    int line = 7;
                                    if (MMOFileFormat > 0)
                                    {
                                        dataGridView1.Rows[zeilenindex].Cells[7].Value = Convert.ToDouble(data[line]);
                                        line++;
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[zeilenindex].Cells[7].Value = 1;
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

                                        if (readwindfile.ReadMeteoFiles(1000000, rowsep, decsep, decsepuser) == false)
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
                                            wind_speeds.Add(new double[wind_length]);
                                            wind_direction.Add(new double[wind_length]);
                                            filelength.Add(0);
                                            metfiles.Add(windfilename);
                                            zeit.Add(new string[wind_length]);
                                            datum.Add(new string[wind_length]);
                                            stunde.Add(new int[wind_length]);
                                            DecsepUser.Add(0);
                                            RowsepUser.Add(0);
                                            stability.Add(new int[wind_length]);
                                            filelength[zeilenindex] = wind_length;

                                            char tempchar = char.Parse(decsepuser);
                                            DecsepUser[zeilenindex] = (int)tempchar;
                                            RowsepUser[zeilenindex] = (int)rowsep;

                                            int length = 0;
                                            foreach (WindData wd in winddata)
                                            {
                                                stunde[zeilenindex][length] = wd.Hour;
                                                wind_speeds[zeilenindex][length] = wd.Vel;
                                                wind_direction[zeilenindex][length] = wd.Dir;
                                                datum[zeilenindex][length] = wd.Date;
                                                zeit[zeilenindex][length] = wd.Time;
                                                stability[zeilenindex][length] = wd.StabClass;
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
                spaltenbezeichnungen.Clear();
                spaltenbezeichnungen.TrimExcess();
                wind_speeds.Clear();
                wind_speeds.TrimExcess();
                wind_direction.Clear();
                wind_direction.TrimExcess();

                stability.Clear();
                stability.TrimExcess();

                metfiles.Clear();
                metfiles.TrimExcess();
                filelength.Clear();
                filelength.TrimExcess();
                datum.Clear();
                datum.TrimExcess();
                stunde.Clear();
                stunde.TrimExcess();
                DecsepUser.Clear();
                DecsepUser.TrimExcess();
                RowsepUser.Clear();
                RowsepUser.TrimExcess();
                zeit.Clear();
                zeit.TrimExcess();
                dataGridView1.Columns.Clear();
                dataGridView1.Dispose();
            }
            catch
            { }
            toolTip1.Dispose();

        }

        void Match_Multiple_ObservationsFormClosing(object sender, FormClosingEventArgs e)
        {
            // do not allow to close this form as long Domain() is open!
            // close_allowed is set to true at DomainFormClosed()
            if (close_allowed == false)
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
                }
                catch
                { }
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

            _dta.WeightingFactor = new double[metfiles.Count + 1];
            _dta.WeightingDirection = new double[metfiles.Count + 1];
            _dta.VectorErrorSum = new int[metfiles.Count, 4];
            _dta.SCErrorSum = new int[metfiles.Count, 2];

            for (int i = 0; i < metfiles.Count; i++)
            {
                _dta.WeightingFactor[i] = 1;
                _dta.WeightingDirection[i] = 1;
            }

            try
            {
                for (int i = 0; i < metfiles.Count; i++)
                {
                    _dta.WeightingFactor[i] = GralStaticFunctions.St_F.TxtToDbl(dataGridView1.Rows[i].Cells[6].Value.ToString(), false);
                    _dta.WeightingFactor[i] = Math.Max(_dta.WeightingFactor[i], 0.0);
                    _dta.WeightingDirection[i] = GralStaticFunctions.St_F.TxtToDbl(dataGridView1.Rows[i].Cells[7].Value.ToString(), false);
                    _dta.WeightingDirection[i] = Math.Min(10, Math.Max(_dta.WeightingDirection[i], 0.0));
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
        private void WriteMetTimeSeries(List<string> MetTimeSeries)
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
            { }
        }

        /// <summary>
        /// Set the tuning results to the data grid view
        /// </summary>
        /// <param name="MatchingSettings"></param>
        private void SetTuningResults(MatchMultipleObservationsData MatchingSettings)
        {
            for (int j = 0; j < metfiles.Count; j++) // write error values to datagrid
            {
                for (int i = 0; i < 4; i++) // 4 values
                {
                    dataGridView1.Rows[j].Cells[8 + i].Value = (100 * MatchingSettings.VectorErrorSum[j, i] / filelength[0]).ToString();
                }

                // error values for SC
                dataGridView1.Rows[j].Cells[12].Value = (100 * MatchingSettings.SCErrorSum[j, 0] / filelength[0]).ToString();
                dataGridView1.Rows[j].Cells[13].Value = (100 * MatchingSettings.SCErrorSum[j, 1] / filelength[0]).ToString();
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
        }

        /// <summary>
        /// Start the match tuning with user defined parameters
        /// </summary>
        private async void StartUserMatchTuning()
        {
            //MessageBox.Show(MatchingData.PGT[1].PGTFrq.ToString() + "/" + MatchingData.WeightingFactor[0].ToString());
            List<string> mettimeseries = await Task.Run(() => { return MatchTuning(MatchingData); });
            WriteMetTimeSeries(mettimeseries);
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
            this.Hide();
            double[] testValuesVector = new double[9] { 0.01, 0.1, 0.5, 1, 5, 10, 20, 40, 60};
            double[] testValuesDirecetion = new double[6] { 0.01, 0.1, 0.5, 1, 5, 10 };
            double[] Error = new double[metfiles.Count];
            double[] BestMatchFactor = new double[metfiles.Count];
            double[] BestMatchDirection = new double[metfiles.Count];
            int[] BestMatchMode = new int[metfiles.Count];
            object lockobj = new object();

            for (int i = 0; i < metfiles.Count; i++)
            {
                Error[i] = 20000000;
            }

            GralMessage.Waitprogressbar wait = new GralMessage.Waitprogressbar("");
#if __MonoCS__
            wait.Width = 350;
#endif
            wait.Text = "Automatical Tuning - Pass 1/3";
            wait.Show();

            // 1st guess: brute force test for all meteo stations
            // Mode 1
            Parallel.ForEach(testValuesVector, (fact) =>
            {
                //local copy of Matching data
                MatchMultipleObservationsData _m = new MatchMultipleObservationsData(MatchingData);
                _m.AutomaticMode = true;
                _m.Optimization = 1;
                // Set all factors
                for (int j = 0; j < metfiles.Count; j++)
                {
                    _m.WeightingFactor[j] = fact;
                }
                Array.Clear(_m.SCErrorSum, 0, _m.SCErrorSum.Length);
                Array.Clear(_m.VectorErrorSum, 0, _m.VectorErrorSum.Length);
               
                MatchTuning(_m);
                double err = AutoTuningError(_m.VectorErrorSum, _m.SCErrorSum);
                lock (lockobj)
                {
                    for (int i = 0; i < metfiles.Count; i++)
                    {
                        if (err < Error[i])
                        {
                            Error[i] = err;
                            BestMatchDirection[i] = _m.WeightingDirection[i];
                            BestMatchFactor[i] = _m.WeightingFactor[i];
                            BestMatchMode[i] = _m.Optimization;
                        }
                    }
                }
            });

            wait.Text = "Automatical Tuning - Pass 2/3";
            // 2nd guess: check Mode 2
            Parallel.ForEach(testValuesVector, (fact) =>
            {
                //local copy of Matching data
                MatchMultipleObservationsData _m = new MatchMultipleObservationsData(MatchingData);
                _m.AutomaticMode = true;
                _m.Optimization = 2;
                
                foreach (double dirfact in testValuesDirecetion)
                {
                    Array.Clear(_m.SCErrorSum, 0, _m.SCErrorSum.Length);
                    Array.Clear(_m.VectorErrorSum, 0, _m.VectorErrorSum.Length);
                    // Set all factors
                    for (int j = 0; j < metfiles.Count; j++)
                    {
                        _m.WeightingFactor[j] = fact;
                        _m.WeightingDirection[j] = dirfact;
                    }
                    
                    MatchTuning(_m);
                    double err = AutoTuningError(_m.VectorErrorSum, _m.SCErrorSum);
                    lock (lockobj)
                    {
                        for (int i = 0; i < metfiles.Count; i++)
                        {
                            if (err < Error[i])
                            {
                                Error[i] = err;
                                BestMatchDirection[i] = _m.WeightingDirection[i];
                                BestMatchFactor[i] = _m.WeightingFactor[i];
                                BestMatchMode[i] = _m.Optimization;
                            }
                        }
                    }
                }
            });

            wait.Text = "Automatical Tuning - Pass 3/3";
            //Iterative fine tuning for all stations
            {
                List<int> MetfileList = new List<int>();
                for (int j = 0; j < metfiles.Count; j++)
                {
                    MetfileList.Add(j);
                }

                for (int iterations = 0; iterations < 5; iterations++)
                {
                    MetfileList.Reverse();

                    foreach (int i in MetfileList)
                    {
                        //local copy of Matching data
                        MatchMultipleObservationsData _m = new MatchMultipleObservationsData(MatchingData);
                        _m.AutomaticMode = true;
                        _m.Optimization = BestMatchMode[0];
                        Array.Copy(BestMatchFactor, _m.WeightingFactor, BestMatchFactor.Length);
                        Array.Copy(BestMatchDirection, _m.WeightingDirection, BestMatchDirection.Length);

                        int direction = 1;
                        if (iterations % 2 == 0)
                        {
                            direction *= -1;
                        }

                        double _fac = 1.1;
                        if (BestMatchFactor[i] < 1)
                        {
                            _fac = 1.2;
                        }
                        for (int j = 0; j < 3; j++)
                        {
                            Array.Clear(_m.SCErrorSum, 0, _m.SCErrorSum.Length);
                            Array.Clear(_m.VectorErrorSum, 0, _m.VectorErrorSum.Length);
                            if (direction > 0)
                            {
                                _m.WeightingFactor[i] *= _fac;
                            }
                            else
                            {
                                _m.WeightingFactor[i] /= _fac;
                            }
                            MatchTuning(_m);
                            double err = AutoTuningError(_m.VectorErrorSum, _m.SCErrorSum);
                            if (err < Error[i])
                            {
                                Error[i] = err;
                                BestMatchDirection[i] = Math.Round(_m.WeightingDirection[i], 2);
                                BestMatchFactor[i] = Math.Round(_m.WeightingFactor[i], 2);
                                BestMatchMode[i] = _m.Optimization;
                            }
                            else if (j > 0) // not improved -> try to reduce the factor
                            {
                                _m.WeightingFactor[i] = BestMatchFactor[i];
                                direction *= -1;
                            }
                            if (_m.WeightingFactor[i] > 100)
                            {
                                j = 1000; // cancel
                            }
                        }
                    }
                }
            }
            
            Array.Clear(MatchingData.SCErrorSum, 0, MatchingData.SCErrorSum.Length);
            Array.Clear(MatchingData.VectorErrorSum, 0, MatchingData.VectorErrorSum.Length);
            Array.Copy(BestMatchDirection, MatchingData.WeightingDirection, BestMatchDirection.Length);
            Array.Copy(BestMatchFactor, MatchingData.WeightingFactor, BestMatchFactor.Length);

            MatchingData.Optimization = BestMatchMode[0];
            MatchTuning(MatchingData);
            SetTuningResults(MatchingData);

            lockobj = null;
            wait.Close();
            wait.Dispose();
            Show();
        }

        /// <summary>
        /// Calculate the weighted error sum for the meteo station Index 
        /// </summary>
        /// <param name="VectorErrorSum">Vector error values</param>
        /// <param name="SCErrorSum">SC error values</param>
        /// <param name="Index">Number of the meteo station</param>
        /// <returns></returns>
        private double AutoTuningError(int[,] VectorErrorSum, int[,] SCErrorSum, int Index)
        {
            double meteopgtlen = Math.Max(1, filelength[0]); //avoid integer division
            double err = ((1.0 - VectorErrorSum[Index, 2] / meteopgtlen) * (100 - hScrollBar1.Value) + (1.0 - SCErrorSum[Index, 1] / meteopgtlen) * hScrollBar1.Value) / 100;
            return err;
        }
        /// <summary>
        /// Calculate the weighted error sum for all meteo stations 
        /// </summary>
        /// <param name="VectorErrorSum">Vector error values</param>
        /// <param name="SCErrorSum">SC error values</param>
        /// <returns></returns>
        private double AutoTuningError(int[,] VectorErrorSum, int[,] SCErrorSum)
        {
            double err = 0;
            double meteopgtlen = Math.Max(1, filelength[0]);  //avoid integer division
            for (int i = 0; i < metfiles.Count; i++)
            {
                err += ((1.0 - VectorErrorSum[i, 2] / meteopgtlen)  * (100 - hScrollBar1.Value) + (1.0 - SCErrorSum[i, 1] / meteopgtlen) * hScrollBar1.Value) / 100;
            }
            err /= metfiles.Count;
            return err;
        }

    }
}
