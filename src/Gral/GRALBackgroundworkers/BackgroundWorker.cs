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
 * User: Markus Kuntner
 * Date: 21.04.2015
 * Time: 08:26
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;
using GralMessage;

namespace GralBackgroundworkers
{
    /// <summary>
    /// The backgroundworker progress form that starts the background threads
    /// </summary>
    public partial class ProgressFormBackgroundworker : Form
    {
        // needed to allow access to the form
        delegate void SetTextCallback(string text);
        // needed to get Data from Domain.cs
        private readonly GralBackgroundworkers.BackgroundworkerData MyBackData;
        // Flag, if calculation was successful
        private bool Computation_Completed = false;
        
        /// <summary>
        /// Starts the backgroundworker for several functions depending on BackgroundworkerData.Rechenart 
        /// </summary> 		
        public ProgressFormBackgroundworker(GralBackgroundworkers.BackgroundworkerData InputData)
        {
            
            InitializeComponent();
            
            // Get the Data from Domain and Set FormCapture
            MyBackData = InputData;
            
            usertext.Text = MyBackData.UserText;
        }

        void Progress_FormShown(object sender, EventArgs e)
        {
            // start the backgroundworker if form is opened
            // Close the form if backgroundworker has finished
            Rechenknecht.WorkerSupportsCancellation = true;
            Rechenknecht.WorkerReportsProgress = true;
            Rechenknecht.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RechenknechtRunWorkerCompleted); 
            Rechenknecht.ProgressChanged += new ProgressChangedEventHandler(SetProgressbar);
            
            progressBar1.Maximum = 100;
            
            // Update of form for LINUX version, otherwise the backgroundworker sometimes cant access this form -> app crashes
            for (int i = 0; i < 20; i++) {
                Thread.Sleep (5);
                Application.DoEvents(); 
            }
            // and let the backgroundworker working
            Rechenknecht.RunWorkerAsync(MyBackData);
        }

        void Progress_FormLoad(object sender, EventArgs e)
        {
            if (Gral.Main.ActiveForm != null)
            {
                Location = Gral.Main.ActiveForm.Location;
            }
            // local MyData for Backgroundworker
            //Gral.Backgroundworker_Data MyData = new Backgroundworker_Data();
        }

        private void SetProgressbar(object sender, ProgressChangedEventArgs e)
        {
           if (e.ProgressPercentage < progressBar1.Maximum && e.ProgressPercentage > progressBar1.Minimum)
           {
              progressBar1.Value = e.ProgressPercentage;
           }
        }
        
        void Button1Click(object sender, EventArgs e)
        {
            Rechenknecht.CancelAsync();
            button1.Enabled = false;
        }
        
        void RechenknechtDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            GralBackgroundworkers.BackgroundworkerData MyData = (GralBackgroundworkers.BackgroundworkerData)e.Argument;
            
            if (MyData.Rechenart == BWMode.GrammMetFile)
            {
                GenerateMeteofile(MyData, e);
            }

            if (MyData.Rechenart == BWMode.ReOrder)
            {
                Reorder(MyData, e);
            }

            if (MyData.Rechenart == BWMode.GralMetFile)
            {
                GenerateGRALMeteofile(MyData, e);
            }

            if (MyData.Rechenart == BWMode.OdorConcentrationPercentile)
            {
                HighPercentiles(MyData, e);
            }

            if (MyData.Rechenart == BWMode.OdorHoursTransient)
            {
                OdourHoursTransient(MyData, e);
            }

            if (MyData.Rechenart == BWMode.MeanMaxTimeSeries)
            {
                MeanMaxDaymax(MyData, e);
            }

            if (MyData.Rechenart == BWMode.OdorCompost)
            {
                OdourCompost(MyData, e);
            }

            if (MyData.Rechenart == BWMode.OdorHours)
            {
                OdourHours(MyData, e);
            }

            if (MyData.Rechenart == BWMode.MeanMeteoPGT)
            {
                Mean(MyData, e);
            }

            if (MyData.Rechenart == BWMode.OdorAllinAllout)
            {
                OdourAllinAllout(MyData, e);
            }

            if (MyData.Rechenart == BWMode.GrammMeanWindVel)
            {
                MeanWindVelocity(MyData, e);
            }

            if (MyData.Rechenart == BWMode.ReceptorTimeSeries)
            {
                ReceptorConcentration(MyData, e);
            }

            if (MyData.Rechenart == BWMode.EvalPointsTimeSeries)
            {
                GenerateTimeSeries(MyData, e);
            }

            if (MyData.Rechenart == BWMode.HighPercentiles)
            {
                HighPercentiles(MyData, e);
            }

            if (MyData.Rechenart == BWMode.MathRasterOperations)
            {
                MathRasterOperation(MyData, e);
            }

            if (MyData.Rechenart == BWMode.GrammExportSubDomain)
            {
                GRAMMExport(MyData, e);
            }
        }

        private void SetText(string text)
        {
            // hier wird der text an das Fortschritt-Form geschickt
            // InvokeRequired fragt ab, ob Thread-ID der Thread-ID des aktuellen Prozesses entspricht
            // Bei unterschiedlichen Threads=true
            if (BGW_Done.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                Invoke(d, new object[] { text }); // text über Callback schicken bei unterschiedlichen threads
            }
            else
            {
                BGW_Done.Text = text; // text direkt zuweisen bei gleichem thread
            }
        }
        
        
        void RechenknechtRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // jetzt bin ich aus dem Rechenknecht-Thread wieder draußen
            Hide();   // this = aktives Fenster = form, wird geschlossen, damit läuft das Hauptprogramm wieder
            
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                MessageBoxTemporary Box = new MessageBoxTemporary("Process cancelled", Location);
                Box.Show();
            }
            else
            {
                
                if (MyBackData.Rechenart == BWMode.GrammMetFile) // Meteo Files
                {
                    MyBackData.Rechenart = BWMode.None;
                    MessageBoxTemporary Box = new MessageBoxTemporary("Process finished. Meteodata can now be analysed in the menu \"Meteorology\".", Location);
                    Box.Show();
                }
                if (MyBackData.Rechenart == BWMode.ReOrder) // Re-Order
                {
                    MyBackData.Rechenart = BWMode.None;
                    MessageBoxTemporary Box = new MessageBoxTemporary("Re-ordering finished!", Location);
                    Box.Show();
                }
                if (MyBackData.Rechenart == BWMode.GralMetFile) // GRAL Meteo Files
                {
                    MyBackData.Rechenart = BWMode.None;
                    MessageBoxTemporary Box = new MessageBoxTemporary("Process finished. Meteodata can now be analysed in the menu \"Meteorology\".", Location);
                    Box.Show();
                }

                if (MyBackData.Rechenart == BWMode.MeanMaxTimeSeries) // Mean, Max, daily Max
                {
                    MyBackData.Rechenart = BWMode.None;
                    // calculation finished and first height slice
                    if (Computation_Completed && MyBackData.WriteDepositionOrOdourData)
                    {
                        MessageBoxTemporary Box = new MessageBoxTemporary("Contour plots can now be created in the menu Domain", Location);
                        Box.Show();
                    }
                    //MessageBox.Show("Contour plots can now be created in the menu Domain");
                }
                
                if (MyBackData.Rechenart == BWMode.OdorCompost) // Compost
                {
                    MyBackData.Rechenart = BWMode.None;
                    // calculation finished and first height slice
                    if (Computation_Completed && MyBackData.WriteDepositionOrOdourData)
                    {
                        MessageBoxTemporary Box = new MessageBoxTemporary("Contour plots can now be created in the menu Domain", Location);
                        Box.Show();
                    }
                    //MessageBox.Show("Contour plots can now be created in the menu Domain");
                }
                
                if (MyBackData.Rechenart == BWMode.OdorHours) // Odour hours
                {
                    MyBackData.Rechenart = BWMode.None;
                    // calculation finished and first height slice
                    if (Computation_Completed && MyBackData.WriteDepositionOrOdourData)
                    {
                        MessageBoxTemporary Box = new MessageBoxTemporary("Contour plots can now be created in the menu Domain", Location);
                        Box.Show();
                    }
                    //MessageBox.Show("Contour plots can now be created in the menu Domain");
                }
                
                if (MyBackData.Rechenart == BWMode.MeanMeteoPGT) // Mean, Max concentrations
                {
                    MyBackData.Rechenart = BWMode.None;
                    // calculation finished and first height slice
                    if (Computation_Completed && MyBackData.WriteDepositionOrOdourData)
                    {
                        MessageBoxTemporary Box = new MessageBoxTemporary("Contour plots can now be created in the menu Domain", Location);
                        Box.Show();
                    }
                    //MessageBox.Show("Contour plots can now be created in the menu Domain");
                }
                
                if (MyBackData.Rechenart == BWMode.OdorAllinAllout) // All in all out
                {
                    MyBackData.Rechenart = BWMode.None;
                    // calculation finished and first height slice
                    if (Computation_Completed && MyBackData.WriteDepositionOrOdourData)
                    {
                        MessageBoxTemporary Box = new MessageBoxTemporary("Contour plots can now be created in the menu Domain", Location);
                        Box.Show();
                    }
                    //MessageBox.Show("Contour plots can now be created in the menu Domain");
                }
                
                if (MyBackData.Rechenart == BWMode.GrammMeanWindVel) // Mean wind velocity
                {
                    MyBackData.Rechenart = BWMode.None;
                    if (Computation_Completed)
                    {
                        MessageBoxTemporary Box = new MessageBoxTemporary("Calculation finished: contour plots can now be created in the menu Domain", Location);
                        Box.Show();
                    }
                    //MessageBox.Show("Contour plots can now be created in the menu Domain");
                }
                
                if (MyBackData.Rechenart == BWMode.ReceptorTimeSeries) // Receptor Concentration
                {
                    MyBackData.Rechenart = BWMode.None;
                    if (Computation_Completed)
                    {
                        MessageBoxTemporary Box = new MessageBoxTemporary("File(s) GRAL_meteostation.met written to Subdirectory Metfiles", Location);
                        Box.Show();
                    }
                    //MessageBox.Show("File(s) GRAL_meteostation.met written to Subdirectory Metfiles");
                }

                if (MyBackData.Rechenart == BWMode.EvalPointsTimeSeries) // Evaluation points concentration
                {
                    MyBackData.Rechenart = BWMode.None;
                    if (Computation_Completed)
                    {
                        MessageBoxTemporary Box = new MessageBoxTemporary("A time series file for all evaluation points has been created", Location);
                        Box.Show();
                    }
                    //MessageBox.Show("File(s) GRAL_meteostation.met written to Subdirectory Metfiles");
                }

                if (MyBackData.Rechenart == BWMode.HighPercentiles) // High Percentils
                {
                    MyBackData.Rechenart = BWMode.None;
                    if (Computation_Completed)
                    {
                        MessageBoxTemporary Box = new MessageBoxTemporary("Contour plots can now be created in the menu Domain", Location);
                        Box.Show();
                    }
                    //MessageBox.Show("Contour plots can now be created in the menu Domain");
                }

                if (MyBackData.Rechenart == BWMode.MathRasterOperations) // Mathematical raster operations
                {
                    MyBackData.Rechenart = BWMode.None;
                    if (Computation_Completed)
                    {
                        MessageBoxTemporary Box = new MessageBoxTemporary("Calculations finished", Location);
                        Box.Show();
                    }
                    //MessageBox.Show("Calculations finished.");
                }

                if (MyBackData.Rechenart == BWMode.GrammExportSubDomain) // GRAMM Export
                {
                    MyBackData.Rechenart = BWMode.None;
                    if (Computation_Completed)
                    {
                        MessageBoxTemporary Box = new MessageBoxTemporary("GRAMM export finished", Location);
                        Box.Show();
                    }
                }
            }
            Rechenknecht.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(RechenknechtRunWorkerCompleted);

        }
        
        private void WriteHeader(GralBackgroundworkers.BackgroundworkerData my_data, string unit)
        {
            my_data.Writer.WriteLine("ncols         " + Convert.ToString(my_data.CellsGralX));
            my_data.Writer.WriteLine("nrows         " + Convert.ToString(my_data.CellsGralY));
            my_data.Writer.WriteLine("xllcorner     " + Convert.ToString(my_data.DomainWest));
            my_data.Writer.WriteLine("yllcorner     " + Convert.ToString(my_data.DomainSouth));
            my_data.Writer.WriteLine("cellsize      " + Convert.ToString(my_data.Horgridsize));
            my_data.Writer.WriteLine("NODATA_value  " + "-9999 \t Unit:\t" + unit);
        }
        
        private string GetSgNumbers(string Sourcegroupname)
        {
            int snumber = 1;
            string[] text11 = Sourcegroupname.Split(new char[] { ':' });
            try
            {
                snumber = Convert.ToInt32(text11[1]);
            }
            catch
            {
                Int32.TryParse(text11[0], out snumber); //snumber = Convert.ToInt16(text11[0]);
            }
            
            return Convert.ToString(snumber);
        }
            
        public bool ReadConFiles(string filename, GralBackgroundworkers.BackgroundworkerData mydata, int itm, ref float[][][] conc)
        {
            bool compressed = false;
            string comp_file = Path.GetFileName(filename).Substring(0, 5) + ".grz";   // filename of a compressed file
            string comp_filepath = Path.Combine(Path.GetDirectoryName(filename), comp_file);    // compressed file with path
            string con_file = Path.GetFileName(filename);  // original *.con filename
                                                           //			MessageBox.Show(comp_filepath);
                                                           //			MessageBox.Show(comp_file);

            bool result = false;
            if (File.Exists(comp_filepath))
            {
                compressed = true;
            }
            else if (!File.Exists(filename)) // no file available
            {
                return result;
            }

            try
            {
                if (compressed)
                {
                    using(ZipArchive archive = ZipFile.OpenRead(comp_filepath))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)  // search for the fitting *.con file
                        {
                            //MessageBox.Show(entry.ToString() + "/" + con_file);
                            if (entry.FullName.Contains(con_file))
                            {
                                //MessageBox.Show(con_file);
                                using (BinaryReader reader = new BinaryReader(entry.Open())) //Open Zip entry
                                {
                                    result = ReadCon(reader, mydata, itm, ref conc);
                                }
                            }
                        }
                        
                    }
                }
                else
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
                    {
                        result = ReadCon(reader, mydata, itm, ref conc);
                    }
                }
                
            }
            catch
            {
                result = false;
            }
            //MessageBox.Show(filename + "/" + result.ToString());
            return result;
        }
        
        private bool ReadCon(BinaryReader reader, GralBackgroundworkers.BackgroundworkerData mydata, int itm, ref float[][][] conc)
        {
            try
            {
                int indexi, indexj;
                
                if (mydata.Horgridsize == 0)
                {
                    return false; // no division by zero
                }

                bool sequential = true; // sequential file with seperators
                int dummy = reader.ReadInt32(); // read 4 bytes from stream = "Header"

                if (dummy == -1)
                {
                    sequential = false; // stream file without seperators
                }
                if (dummy == -2) // new strong compressed file format
                {
                    return ReadConStrongCompressed(reader, mydata, itm, ref conc);
                }
                if (dummy == -3) // new all compressed file format
                {
                    return ReadConAllCompressed(reader, mydata, itm, ref conc);
                }

                for (;;) // endless loop until eof()
                {
                    int  x = reader.ReadInt32(); // read 4 bytes = i
                    indexi = Convert.ToInt32((x - 0.5 * mydata.Horgridsize - mydata.DomainWest) / mydata.Horgridsize);
                    x = reader.ReadInt32();     // read 4 bytes = j
                    indexj = Convert.ToInt32((x - 0.5 * mydata.Horgridsize - mydata.DomainSouth) / mydata.Horgridsize);
                    Single tempconc = reader.ReadSingle(); // read 4 bytes = Single concentration value
                    
                    if (indexi >= 0 & indexj >= 0 & indexi <= (mydata.CellsGralX + 1) & indexj <= (mydata.CellsGralY + 1))
                    {
                        conc[indexi][indexj][itm] = tempconc;
                    }
                    if (sequential == true)
                    {
                        // Read seperator bytes at the end of the loop, otherwise an error may occur and the last value wouldn´t be computed
                        dummy = reader.ReadInt32(); // read 4 bytes from stream = "Seperator" if stream is formatted
                        dummy = reader.ReadInt32(); // read 4 bytes from stream = "Seperator" if strem is formatted
                    }
                }
            }
            catch(System.IO.EndOfStreamException)
            {
               return true;
            }
            catch // other error
            {
                RestoreJaggedArray(conc);
                return false;
            }
        }

        private bool ReadConStrongCompressed(BinaryReader reader, GralBackgroundworkers.BackgroundworkerData mydata, int itm, ref float[][][] conc)
        {
            try
            {
                int indexi, indexj;

                int IKOOAGRAL = reader.ReadInt32(); // western border
                int JKOOAGRAL = reader.ReadInt32(); // southern border
                float dx = reader.ReadSingle();
                float dy = reader.ReadSingle();

                int end = 0;
                
                // read entire file
                do
                {
                    int lenght = reader.ReadInt32(); // number of points at one row

                    if (lenght > 0) // new dataset
                    {
                        int xi = reader.ReadInt32();
                        int xj = reader.ReadInt32();
                        double x = IKOOAGRAL + xi * dx - dx * 0.5F;
                        double y = JKOOAGRAL + xj * dy - dy * 0.5F;

                        indexi = Convert.ToInt32((x - 0.5 * mydata.Horgridsize - mydata.DomainWest) / mydata.Horgridsize);
                        indexj = Convert.ToInt32((y - 0.5 * mydata.Horgridsize - mydata.DomainSouth) / mydata.Horgridsize);

                        Single tempconc = 0;
                        for (int i = 0; i < lenght; i++)
                        {
                            tempconc = reader.ReadSingle(); // read 4 bytes = Single concentration value
                            if (indexi >= 0 & indexj >= 0 & indexi <= (mydata.CellsGralX + 1) & indexj <= (mydata.CellsGralY + 1))
                            {
                                conc[indexi][indexj][itm] = tempconc;
                            }

                            y += dy; // new y- coordinate of next point
                            indexj = Convert.ToInt32((y - 0.5 * mydata.Horgridsize - mydata.DomainSouth) / mydata.Horgridsize);
                        }
                    }
                    else if (lenght == -1) // end of file
                    {
                        end = lenght; // finish
                    }
                    else // wrong value -> error
                    {
                        throw new IOException();
                    }

                }
                while (end != -1);
                return true;
            }
            catch
            {
                RestoreJaggedArray(conc);
                return false;
            }
        }

        private bool ReadConAllCompressed(BinaryReader reader, GralBackgroundworkers.BackgroundworkerData mydata, int itm, ref float[][][] conc)
        {
            try
            {
                int indexi, indexj;

                int IKOOAGRAL = reader.ReadInt32(); // western border
                int JKOOAGRAL = reader.ReadInt32(); // southern border
                float dx = reader.ReadSingle();
                float dy = reader.ReadSingle();
                int NX = reader.ReadInt32();
                int NY = reader.ReadInt32();

                for (int i = 1; i <= NX; i++)
                {
                    double x = IKOOAGRAL + i * dx - dx * 0.5F;
                    indexi = Convert.ToInt32((x - 0.5 * mydata.Horgridsize - mydata.DomainWest) / mydata.Horgridsize);

                    for (int j = 1; j <= NY; j++)
                    {
                        double y = JKOOAGRAL + j * dy - dy * 0.5F;
                        indexj = Convert.ToInt32((y - 0.5 * mydata.Horgridsize - mydata.DomainSouth) / mydata.Horgridsize);
                        if (indexi >= 0 & indexj >= 0 & indexi <= (mydata.CellsGralX + 1) & indexj <= (mydata.CellsGralY + 1))
                        {
                            conc[indexi][indexj][itm] = reader.ReadSingle(); // read 4 bytes = Single concentration value
                        }
                    }
                }
                return true;
            }
            catch
            {
                RestoreJaggedArray(conc);
                return false;
            }
        }

        private bool ReadOdrFiles(string filename, GralBackgroundworkers.BackgroundworkerData mydata, int itm, ref float[][][] concp, ref float[][][] concm, ref float[][][] Q_cv0, ref float[][][] td)
        {
            bool compressed = false;
            string comp_file = Path.GetFileName(filename).Substring(0,5) + ".grz";   // filename of a compressed file
            string comp_filepath = Path.Combine(Path.GetDirectoryName(filename), comp_file);    // compressed file with path
            string con_file  = Path.GetFileName(filename);  // original *.con filename
                                                            //			MessageBox.Show(comp_filepath);
                                                            //			MessageBox.Show(comp_file);
            bool result = false;
            if (File.Exists(comp_filepath))
            {
                compressed = true;
            }
            else if (!File.Exists(filename)) // no file available
            {
                return result;
            }

            try
            {
                if (compressed)
                {
                    using(ZipArchive archive = ZipFile.OpenRead(comp_filepath))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)  // search for the fitting *.con file
                        {
                            //MessageBox.Show(entry.ToString() + "/" + con_file);
                            if (entry.FullName.Contains(con_file))
                            {
                                //MessageBox.Show(con_file);
                                using (BinaryReader reader = new BinaryReader(entry.Open())) //Open Zip entry
                                {
                                    result = ReadOdr(reader, mydata, itm, ref concp, ref concm, ref Q_cv0, ref td);
                                }
                            }
                        }
                        
                    }
                }
                else
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
                    {
                        result = ReadOdr(reader, mydata, itm, ref concp, ref concm, ref Q_cv0, ref td);
                    }
                }               
            }
            catch
            {
                result =  false;
            }
            return result;
        }
        
        private bool ReadOdr(BinaryReader reader, GralBackgroundworkers.BackgroundworkerData mydata, int itm, ref float[][][] concp, ref float[][][] concm, ref float[][][] Q_cv0, ref float[][][] td)
        {
            try
            {
                int indexi, indexj;
                if (mydata.Horgridsize == 0)
                {
                    return false; // no division by zero
                }

                bool sequential = true; // sequential file with seperators
                int dummy = reader.ReadInt32(); // read 4 bytes from stream = "Header"

                if (dummy == -1)
                {
                    sequential = false; // stream file without seperators
                }
                if (dummy == -2) // new strong compressed file format
                {
                    return ReadOdrStrongCompressed(reader, mydata, itm, ref concp, ref concm, ref Q_cv0, ref td);
                }
                if (dummy == -3) // new all compressed file format
                {
                    return ReadOdrAllCompressed(reader, mydata, itm, ref concp, ref concm, ref Q_cv0, ref td);
                }

                for (; ; ) // endless loop until eof()
                {
                    int x = reader.ReadInt32(); // read 4 bytes = i
                    indexi = Convert.ToInt32((x - 0.5 * mydata.Horgridsize - mydata.DomainWest) / mydata.Horgridsize);
                    x = reader.ReadInt32();     // read 4 bytes = j
                    indexj = Convert.ToInt32((x - 0.5 * mydata.Horgridsize - mydata.DomainSouth) / mydata.Horgridsize);

                    Single tempconc = reader.ReadSingle(); // read 4 bytes = Single concentration value
                    if (indexi >= 0 & indexj >= 0 & indexi <= (mydata.CellsGralX + 1) & indexj <= (mydata.CellsGralY + 1))
                    {
                        concp[indexi][indexj][itm] = tempconc;
                    }
                    tempconc = reader.ReadSingle(); // read 4 bytes = Single concentration value
                    if (indexi >= 0 & indexj >= 0 & indexi <= (mydata.CellsGralX + 1) & indexj <= (mydata.CellsGralY + 1))
                    {
                        concm[indexi][indexj][itm] = tempconc;
                    }
                    tempconc = reader.ReadSingle(); // read 4 bytes = Single concentration value
                    if (indexi >= 0 & indexj >= 0 & indexi <= (mydata.CellsGralX + 1) & indexj <= (mydata.CellsGralY + 1))
                    {
                        Q_cv0[indexi][indexj][itm] = tempconc;
                    }
                    tempconc = reader.ReadSingle(); // read 4 bytes = Single concentration value
                    if (indexi >= 0 & indexj >= 0 & indexi <= (mydata.CellsGralX + 1) & indexj <= (mydata.CellsGralY + 1))
                    {
                        td[indexi][indexj][itm] = tempconc;
                    }

                    if (sequential == true)
                    {
                        // Read seperator bytes at the end of the loop, otherwise an error may occur and the last value wouldn´t be computed
                        dummy = reader.ReadInt32(); // read 4 bytes from stream = "Seperator" if stream is formatted
                        dummy = reader.ReadInt32(); // read 4 bytes from stream = "Seperator" if strem is formatted
                    }
                }
            }
            catch (System.IO.EndOfStreamException)
            {
                return true;
            }
            catch // other error
            {
                return false;
            }
            return true;
        }

        private bool ReadOdrStrongCompressed(BinaryReader reader, GralBackgroundworkers.BackgroundworkerData mydata, int itm, ref float[][][] concp, ref float[][][] concm, ref float[][][] Q_cv0, ref float[][][] td)
        {
            try
            {
                int indexi, indexj;

                int IKOOAGRAL = reader.ReadInt32(); // western border
                int JKOOAGRAL = reader.ReadInt32(); // southern border
                float dx = reader.ReadSingle();
                float dy = reader.ReadSingle();

                int end = 0;

                // read entire file
                do
                {
                    int lenght = reader.ReadInt32(); // number of points at one row

                    if (lenght > 0) // new dataset
                    {
                        int xi = reader.ReadInt32();
                        int xj = reader.ReadInt32();
                        double x = IKOOAGRAL + xi * dx - dx * 0.5F;
                        double y = JKOOAGRAL + xj * dy - dy * 0.5F;

                        indexi = Convert.ToInt32((x - 0.5 * mydata.Horgridsize - mydata.DomainWest) / mydata.Horgridsize);
                        indexj = Convert.ToInt32((y - 0.5 * mydata.Horgridsize - mydata.DomainSouth) / mydata.Horgridsize);

                        Single tempconcp = 0;
                        Single tempconcm = 0;
                        Single tempQ_cv = 0;
                        Single temptd = 0;
                        for (int i = 0; i < lenght; i += 4)
                        {
                            tempconcp = reader.ReadSingle(); // read 4 bytes = Single concentration value
                            tempconcm = reader.ReadSingle(); // read 4 bytes = Single concentration value
                            tempQ_cv = reader.ReadSingle();  // read 4 bytes = Single concentration value
                            temptd = reader.ReadSingle();    // read 4 bytes = Single concentration value

                            if (indexi >= 0 & indexj >= 0 & indexi <= (mydata.CellsGralX + 1) & indexj <= (mydata.CellsGralY + 1))
                            {
                                concp[indexi][indexj][itm] = tempconcp;
                                concm[indexi][indexj][itm] = tempconcm;
                                Q_cv0[indexi][indexj][itm] = tempQ_cv;
                                td[indexi][indexj][itm] = temptd;
                            }

                            y += dy; // new y- coordinate of next point
                            indexj = Convert.ToInt32((y - 0.5 * mydata.Horgridsize - mydata.DomainSouth) / mydata.Horgridsize);
                        }
                    }
                    else if (lenght == -1) // end of file
                    {
                        end = lenght; // finish
                    }
                    else // wrong value -> error
                    {
                        throw new IOException();
                    }

                }
                while (end != -1);

                return true;
            }
            catch
            {
                RestoreJaggedArray(concp);
                RestoreJaggedArray(concm);
                RestoreJaggedArray(Q_cv0);
                RestoreJaggedArray(td);
                return false;
            }
        }

        private bool ReadOdrAllCompressed(BinaryReader reader, GralBackgroundworkers.BackgroundworkerData mydata, int itm, ref float[][][] concp, ref float[][][] concm, ref float[][][] Q_cv0, ref float[][][] td)
        {
            try
            {
                int indexi, indexj;

                int IKOOAGRAL = reader.ReadInt32(); // western border
                int JKOOAGRAL = reader.ReadInt32(); // southern border
                float dx = reader.ReadSingle();
                float dy = reader.ReadSingle();
                int NX = reader.ReadInt32();
                int NY = reader.ReadInt32();

                Single tempconcp = 0;
                Single tempconcm = 0;
                Single tempQ_cv = 0;
                Single temptd = 0;

                for (int i = 1; i <= NX; i++)
                {
                    double x = IKOOAGRAL + i * dx - dx * 0.5F;
                    indexi = Convert.ToInt32((x - 0.5 * mydata.Horgridsize - mydata.DomainWest) / mydata.Horgridsize);

                    for (int j = 1; j <= NY; j++)
                    {
                        double y = JKOOAGRAL + j * dy - dy * 0.5F;
                        indexj = Convert.ToInt32((y - 0.5 * mydata.Horgridsize - mydata.DomainSouth) / mydata.Horgridsize);

                        tempconcp = reader.ReadSingle(); // read 4 bytes = Single concentration value
                        tempconcm = reader.ReadSingle(); // read 4 bytes = Single concentration value
                        tempQ_cv = reader.ReadSingle();  // read 4 bytes = Single concentration value
                        temptd = reader.ReadSingle();    // read 4 bytes = Single concentration value

                        if (indexi >= 0 & indexj >= 0 & indexi <= (mydata.CellsGralX + 1) & indexj <= (mydata.CellsGralY + 1))
                        {
                            concp[indexi][indexj][itm] = tempconcp;
                            concm[indexi][indexj][itm] = tempconcm;
                            Q_cv0[indexi][indexj][itm] = tempQ_cv;
                            td[indexi][indexj][itm] = temptd;
                        }
                    }
                }

                return true;
            }
            catch
            {
                RestoreJaggedArray(concp);
                RestoreJaggedArray(concm);
                RestoreJaggedArray(Q_cv0);
                RestoreJaggedArray(td);
                return false;
            }
        }

        public bool ReadMeteopgtAll(string filename, ref List<string> data)
        {
            bool ok = true;
            data.Clear();
            try
            {
                FileStream fs_meteopgt = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                using (StreamReader meteopgt = new StreamReader(fs_meteopgt))
                {
                    // read header
                    string text = meteopgt.ReadLine();
                    text = meteopgt.ReadLine();
                    
                    // read data
                    while (meteopgt.EndOfStream == false)
                    {
                        text = meteopgt.ReadLine();
                        data.Add(text);
                    }
                    
                }
            }
            catch
            {
                ok = false;
            }
            return ok;
        }
        
        public bool ReadMettimeseries(string filename, ref List<string> data)
        {
            bool ok = true;
            data.Clear();
            try
            {
                FileStream fs_mettimeseries = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                using (StreamReader mettimeseries = new StreamReader(fs_mettimeseries))
                {
                    string text;
                    
                    // read data
                    while (mettimeseries.EndOfStream == false)
                    {
                        text = mettimeseries.ReadLine();
                        data.Add(text);
                    }
                    
                }
            }
            catch
            {
                ok = false;
            }
            return ok;
        }

        private void BackgroundThreadMessageBox(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => 
                    { MessageBox.Show(this, text, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error); }));
            }
            else
            {
                MessageBox.Show(text, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProgressFormBackgroundworker_FormClosed(object sender, FormClosedEventArgs e)
        {
            Rechenknecht.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(RechenknechtRunWorkerCompleted);
            Rechenknecht.ProgressChanged -= new ProgressChangedEventHandler(SetProgressbar);
        }

        ///<summary>
        /// Create a jagged array
        /// </summary>
        private static T[] CreateArray<T>(int cnt, Func<T> itemCreator)
        {
            T[] result = new T[cnt];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = itemCreator();
            }
            return result;
        }
        ///<summary>
        /// Restore a jagged array to 0
        /// </summary>
        private void RestoreJaggedArray(float[][][] T)
        {
            System.Threading.Tasks.Parallel.For(0, T.Length, i =>
            //for (int i = 0; i < T.Length; i++)
            {
                for (int j = 0; j < T[i].Length; j++)
                {
                    float[] _t = T[i][j];
                    for (int k = 0; k < _t.Length; k++)
                    {
                        _t[k] = 0;
                    }
                }
            });
        }

    }
}
