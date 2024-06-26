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
using System.Windows.Forms;
using System.IO;
using GralDomForms;
using System.Globalization;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// OpenFileDialog for the *.met files for the matching process  
        /// </summary>
        private void LoadWindFileForMatching(object sender, EventArgs e)
        {
            //add meteo-station or correct coordinates of selected meteo-station
            MMO.dataGridView1.Rows.Add();
            int zeilenindex = MMO.dataGridView1.Rows.Count-1;
            
            MMO.dataGridView1.Rows[zeilenindex].Cells[1].Value = Convert.ToInt32(XDomain);
            MMO.dataGridView1.Rows[zeilenindex].Cells[2].Value = Convert.ToInt32(YDomain);
            MMO.dataGridView1.Rows[zeilenindex].Cells[6].Value = 1;
            MMO.dataGridView1.Rows[zeilenindex].Cells[7].Value = 1;
            MMO.dataGridView1.Rows[zeilenindex].Cells[8].Value = 1;
            
            //launch routine to read meteo data
            MMOData.Meteo = false;
            MMOData.FileLenght = 0;
            string metfile = "";
            MMO.TopMost = false;
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Met files (*.met)|*.met|DWD (*.akterm; *.akt)|*.akterm;*.akt",
                Title = "Select meteorological data",
                InitialDirectory = Path.Combine(Gral.Main.ProjectName, "Metfiles" + Path.DirectorySeparatorChar)
#if NET6_0_OR_GREATER
                ,ClientGuid = GralStaticFunctions.St_F.FileDialogMeteo
#endif
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                metfile = dialog.FileName;
                MMOData.MetFileExt = Path.GetExtension(metfile).ToLower();
                double Anemometerheight = 0;
                double x0 = double.MaxValue;
                double y0 = 0;
                
                try
                {
                    try
                    {
                        using (StreamReader streamreader = new StreamReader(metfile))
                        {
                            string reihe;
                            
                            //Evaluating the length of the file
                            if (MMOData.MetFileExt == ".met")
                            {
                                int ret;
                                while (streamreader.EndOfStream == false)
                                {
                                    reihe = streamreader.ReadLine();
                                    if (reihe.ToUpper().Contains("X="))
                                    {
                                        x0 = EvalMetFileHeader(reihe);
                                    }
                                    if (reihe.ToUpper().Contains("Y="))
                                    {
                                        y0 = EvalMetFileHeader(reihe);
                                    }
                                    if (reihe.ToUpper().Contains("Z="))
                                    {
                                        Anemometerheight = EvalMetFileHeader(reihe);
                                        if (Anemometerheight == double.MaxValue)
                                        {
                                            Anemometerheight = 10;
                                        }
                                    }
                                    if (Int32.TryParse(reihe.Substring(0, 1), out ret) == true)
                                    {
                                        MMOData.FileLenght++;
                                    }
                                }
                            }
                            else if (MMOData.MetFileExt == ".akterm")
                            {
                                string[] text = new string[50];
                                
                                while (streamreader.EndOfStream == false)
                                {
                                    reihe = streamreader.ReadLine();
                                    text = reihe.Split(new char[] { ' ', '\t' });
                                    if (text[0] == "AK")
                                    {
                                        text = reihe.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                        if((text[7] != "9") && (text[8] != "9") && (text[12] != "7") && (text[12] != "9"))
                                        {
                                            MMOData.FileLenght++;
                                        }
                                    }
                                    if (text[0] == "+")
                                    {
                                        try
                                    {
                                        text = reihe.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        //at this stage it is not allowed to delete existing meteoinput-data
                                        //numericUpDown7.Value = Convert.ToDecimal(text[10]) / 10;
                                        MMOData.AnemometerHeight = Convert.ToDouble(text[10]) / 10;
                                    }
                                    catch
                                    {
                                    }
                                    }
                                }
                            }
                            else if (MMOData.MetFileExt == ".akt") // Kuntner akt
                            {
                                while (streamreader.EndOfStream == false)
                                {
                                    reihe = streamreader.ReadLine();
                                    MMOData.FileLenght++;
                                }
                            }
                        }
                    }
                    catch
                    {
                        MMOData.FileLenght = 0;
                    }
//
                    if (MMOData.FileLenght < 1)
                    {
                        MessageBox.Show(this, "Problems when reading selected met-file.","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    // coordinate-Info available
                    if (x0 < (double.MaxValue - 1) && y0 < (double.MaxValue - 1))
                    {
                        MMO.dataGridView1.Rows[zeilenindex].Cells[1].Value = Convert.ToInt32(x0);
                        MMO.dataGridView1.Rows[zeilenindex].Cells[2].Value = Convert.ToInt32(y0);
                        MMO.dataGridView1.Rows[zeilenindex].Cells[3].Value = Convert.ToInt32((int) (Anemometerheight));
                    }

                    MeteoInput_Matchlocalobs FormatMetFile = new MeteoInput_Matchlocalobs()
                    {
                        MMOData = MMOData,
                        MetFile1 = metfile,
                        FileLength1 = MMOData.FileLenght,
                        DecSep1 = decsep,
                        Anemometerheight = Anemometerheight,
                        Owner = this
                    };
                    if (FormatMetFile.ShowDialog() == DialogResult.OK)
                    {
                        FormatMetFile.Show();
                    }
                    MMOData.Meteo = true;

                    //add met data
                    MMO.WindVelocityObs.Add(new double[MMOData.FileLenght]);
                    MMO.WindDirectionObs.Add(new double[MMOData.FileLenght]);
                    MMO.MetFileLenght.Add(0);
                    MMO.MetFileNames.Add("");
                    MMO.TimeStapmsMetTimeSeries.Add(new string[MMOData.FileLenght]);
                    MMO.DateObsMetFile.Add(new string[MMOData.FileLenght]);
                    MMO.HourObsMetFile.Add(new int[MMOData.FileLenght]);
                    MMO.DecsepUser.Add(0);
                    MMO.RowsepUser.Add(0);
                    MMO.StabilityClassObs.Add(new int[MMOData.FileLenght]);
                    
                    int MMOdatagridindex = zeilenindex;
                    
                    MMO.MetFileLenght[MMOdatagridindex] = MMOData.FileLenght;
                    MMO.MetFileNames[MMOdatagridindex] =metfile;
                    
                    for (int length = 0; length < MMOData.FileLenght; length++)
                    {
                        MMO.HourObsMetFile[MMOdatagridindex][length] = MMOData.GetHour()[length];
                        MMO.WindVelocityObs[MMOdatagridindex][length] = MMOData.GetWindVel()[length];
                        MMO.WindDirectionObs[MMOdatagridindex][length] = MMOData.GetWindDir()[length];
                        MMO.DateObsMetFile[MMOdatagridindex][length] = MMOData.GetDate()[length];
                        MMO.TimeStapmsMetTimeSeries[MMOdatagridindex][length] = MMOData.GetTime()[length];
                        MMO.StabilityClassObs[MMOdatagridindex][length] = MMOData.GetSC()[length];
                    }
                    
                    char temp = char.Parse(FormatMetFile.DecSepUser);
                    MMO.DecsepUser[MMOdatagridindex] = (int) temp;
                    MMO.RowsepUser[MMOdatagridindex] = (int) FormatMetFile.RowSep;
                    
                    
                    //get statistics and write it to the table
                    MMO.dataGridView1.Rows[zeilenindex].Cells[0].Value = Convert.ToString(Path.GetFileName(metfile));
                    MMO.dataGridView1.Rows[zeilenindex].Cells[3].Value = Convert.ToDouble(FormatMetFile.numericUpDown1.Value);
                    MMO.dataGridView1.Rows[zeilenindex].Cells[4].Value = Convert.ToString(FormatMetFile.monthCalendar1.TodayDate);
                    MMO.dataGridView1.Rows[zeilenindex].Cells[5].Value = Convert.ToString(FormatMetFile.monthCalendar2.TodayDate);

                    //unselect rows
                    MMO.dataGridView1.ClearSelection();
                    
                    FormatMetFile.Dispose();
                }
                catch
                {
                    if (MMO.dataGridView1.RowCount <= zeilenindex)
                    {
                        MMO.dataGridView1.Rows.Remove(MMO.dataGridView1.Rows[zeilenindex]);
                        MMO.WindVelocityObs.RemoveAt(zeilenindex);
                        MMO.WindDirectionObs.RemoveAt(zeilenindex);
                        MMO.MetFileLenght.RemoveAt(zeilenindex);
                        MMO.MetFileNames.RemoveAt(zeilenindex);
                        MMO.TimeStapmsMetTimeSeries.RemoveAt(zeilenindex);
                        MMO.DateObsMetFile.RemoveAt(zeilenindex);
                        MMO.HourObsMetFile.RemoveAt(zeilenindex);
                        MMO.DecsepUser.RemoveAt(zeilenindex);
                        MMO.RowsepUser.RemoveAt(zeilenindex);
                        MMO.StabilityClassObs.RemoveAt(zeilenindex);
                    }
                    MessageBox.Show(this, "Problems when reading selected met-file.","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
            dialog.Dispose();
            MMO.TopMost = false;
        }
        
        public static double EvalMetFileHeader(string s)
        {
            string[] _st = s.Split(new char[] { '='});
            double x = double.MaxValue;
            if (_st.Length > 0)
            {
                try
                {
                    x = Convert.ToDouble(_st[1], CultureInfo.InvariantCulture);
                }
                catch
                {}
            }
            return x;
        }
    }
}
