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
 * Time: 17:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;
using GralIO;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
    {
        //read the file Percent.txt when it is changed by GRAL.exe
        void PercentChanged(object sender, FileSystemEventArgs e)
        {
            progressBar3.Minimum = 0;
            progressBar3.Maximum = 101;
            int trackbar = -1;
            try
            {
                using (StreamReader myreader = new StreamReader(Path.Combine(ProjectName, @"Computation", "Percent.txt")))
                {
                    trackbar = Convert.ToInt32(myreader.ReadLine().Replace(" ", ""));
                }
            }
            catch
            { }

            try
            {
                if (trackbar != -1)
                {
                    if (trackbar < progressBar3.Maximum)
                    {
                        UpdateProgressBar3(Math.Max(0, Math.Min(100, trackbar)));
                    }
                    UpdateLabel68("Actual dispersion situation: " + Convert.ToString(trackbar) + "%");
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// read the file DispNr.txt when it is changed by GRAL.exe
        /// </summary>
        void DispNrChanged(object sender, FileSystemEventArgs e)
        {
            //progressBar3.Value = 0;
            UpdateProgressBar3(0);
            progressBar4.Minimum = 0;

            UpdateLabel68("Actual dispersion situation: 0%");
            double frequency = 0;
            int trackbar = -1;
            //check if GRAL is operated in transient mode
            int transient = 1;
            int weathersit_count = 0;
            try
            {
                InDatVariables data = new InDatVariables();
                InDatFileIO ReadInData = new InDatFileIO();
                data.InDatPath = Path.Combine(ProjectName, @"Computation", "in.dat");
                ReadInData.Data = data;
                if (ReadInData.ReadInDat() == true)
                {
                    if (data.Transientflag == 0)
                    {
                        transient = 0;
                        //read mettimeseries.dat
                        string mettimeseries = Path.Combine(ProjectName, @"Computation", "mettimeseries.dat");
                        List<string> data_mettimeseries = new List<string>();
                        ReadMetTimeSeries(mettimeseries, ref data_mettimeseries);
                        weathersit_count = Math.Max(data_mettimeseries.Count, 1);
                        //if (data_mettimeseries.Count == 0) // no data available
                        //{
                        //    MessageBox.Show("mettimeseries.dat not available -> correct visualization of the simulation progress not possible");
                        //}
                    }
                }
            }
            catch
            { }

            try
            {
                using (StreamReader myreader = new StreamReader(Path.Combine(ProjectName, @"Computation", "DispNr.txt")))
                {
                    trackbar = Convert.ToInt32(myreader.ReadLine().Replace(" ", "")) - 1;
                }
            }
            catch
            { }
            try
            {
                if (trackbar != -1)
                {
                    //compute completed dispersion situations in %
                    if (transient == 1)
                    {
                        for (int i = 0; i < trackbar; i++)
                        {
                            frequency += DispSituationfrequ[i];
                        }
                        if (Convert.ToInt32(frequency) < progressBar4.Maximum)
                        {
                            UpdateProgressBar4(trackbar);
                        }
                    }
                    else
                    {
                        progressBar4.Maximum = Convert.ToInt32(weathersit_count);
                        frequency = (float)trackbar / (float)weathersit_count * 1000;
                        if ((float)trackbar <= progressBar4.Maximum)
                        {
                            UpdateProgressBar4(trackbar);
                        }
                    }

                    UpdateLabel69("Dispersion situation: " + Convert.ToString(trackbar) + "/" + Convert.ToString(Math.Round(frequency / 10.0, 1)) + "%");
                }
            }
            catch
            {
            }
        }

        void UpdateLabel66(string _value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateLabel66), _value);
                return;
            }
            label66.Text = _value;
        }
        void UpdateLabel67(string _value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateLabel67), _value);
                return;
            }
            label67.Text = _value;
        }
        void UpdateProgressBar1(int _value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(UpdateProgressBar1), _value);
                return;
            }

            progressBar1.Value = _value;
        }
        void UpdateProgressBar2(int _value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(UpdateProgressBar2), _value);
                return;
            }

            progressBar2.Value = _value;
        }
        void UpdateLabel68(string _value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateLabel68), _value);
                return;
            }
            label68.Text = _value;
        }
        void UpdateLabel69(string _value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateLabel69), _value);
                return;
            }
            label69.Text = _value;
        }
        void UpdateProgressBar3(int _value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(UpdateProgressBar3), _value);
                return;
            }

            progressBar3.Value = _value;
        }
        void UpdateProgressBar4(int _value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(UpdateProgressBar4), _value);
                return;
            }

            progressBar4.Value = _value;
        }

        //read the file PercentGramm.txt when it is changed by GRAMM.exe
        void PercentGrammChanged(object sender, FileSystemEventArgs e)
        {
            //progressBar2.Minimum = 0;
            //progressBar2.Maximum = 101;

            double trackbar = -1;
            string text;
            try
            {
                using (StreamReader myreader = new StreamReader(Path.Combine(ProjectName, @"Computation", "PercentGramm.txt")))
                {
                    text = myreader.ReadLine().Replace(" ", "");
                    trackbar = Convert.ToDouble(text.Replace(".", DecimalSep));
                }
            }
            catch
            { }

            if (trackbar != -1)
            {
                try
                {
                    UpdateProgressBar2(Math.Max(0, Math.Min(100, Convert.ToInt32(Math.Round((float)trackbar / (float)numericUpDown21.Value * 100, 0)))));
                    UpdateLabel67("Actual flow situation: " + Convert.ToString(Math.Round((float)trackbar / (float)numericUpDown21.Value * 100, 0)) + "%");
                }
                catch
                { }
            }
            else
            {
                UpdateProgressBar2(0);
                UpdateLabel67("Actual flow situation: 0 %");
            }
        }

        //read the file DispNrGramm.txt when it is changed by GRAMM.exe
        void DispnrGrammChanged(object sender, FileSystemEventArgs e)
        {
            progressBar1.Minimum = 0;
            UpdateProgressBar2(0); 
            UpdateLabel67("Actual flow situation: 0 %");
            double frequency = 0;
            int trackbar = -1;
            try
            {
                using (StreamReader myreader = new StreamReader(Path.Combine(ProjectName, @"Computation", "DispNrGramm.txt")))
                {
                    trackbar = Convert.ToInt32(myreader.ReadLine().Replace(" ", ""));
                }
            }
            catch
            { }

            if (trackbar != -1)
            {
                try
                {
                    for (int i = 0; i < trackbar - 1; i++)
                    {
                        frequency += DispSituationfrequ[i];
                    }
                    if (Convert.ToInt32(frequency) < progressBar1.Maximum)
                    {
                        UpdateProgressBar1(Math.Max(0, Convert.ToInt32(frequency)));
                    }
                    UpdateLabel66("Flow situation: " + Convert.ToString(trackbar - 1) + "/" + Convert.ToString(Math.Round(frequency / 10, 1)) + "%");
                }
                catch
                {

                }
            }
            else
            {
                UpdateProgressBar1(0);
                UpdateLabel66("Flow situation: 0 / 0 %");
            }
        }

        //read the file Problemreport.txt when it is changed by GRAMM.exe
        void ProblemreportGrammChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                string report = "0";
                bool end = false;
                using (StreamReader myreader = new StreamReader(Path.Combine(ProjectName, @"Computation", "Problemreport.txt")))
                {
                    while (end == false)
                    {
                        report = myreader.ReadLine();
                        end = myreader.EndOfStream;
                    }
                }
                //MessageBox.Show(Convert.ToString(report));
            }
            catch
            { }
        }

        //read the file Problemreport_GRAL.txt when it is changed by GRAL.exe
        void ProblemreportGralChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                string report = "0";
                bool end = false;
                using (StreamReader myreader = new StreamReader(Path.Combine(ProjectName, @"Computation", "Problemreport_GRAL.txt")))
                {
                    while (end == false)
                    {
                        report = myreader.ReadLine();
                        end = myreader.EndOfStream;
                    }
                }

                //MessageBox.Show(Convert.ToString(report));
            }
            catch
            { }
        }
    }
}