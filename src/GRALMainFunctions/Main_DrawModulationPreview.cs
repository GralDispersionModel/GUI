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

using GralStaticFunctions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
    {
        // show preview of diurnal/seasonal emission modulation
        public void RedrawPreviewOfModulation(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            
            //load information about the selected diurnal and seasonal modulation for this source group
            string newPath = Path.Combine(ProjectName, @"Settings", "emissionmodulations.txt");
            
            // in case of an emissionmodulation variation use the emissionmodulations.txt from the variation folder
            if (!string.Equals(Path.Combine(ProjectName, @"Computation"), ProjectSetting.EmissionModulationPath))
            {
                newPath = Path.Combine(ProjectSetting.EmissionModulationPath, "emissionmodulations.txt");
            }

            string[] text2 = new string[26];
            
            string[] months = new string[12]{"Jan","Feb","Mar","Apr","Mai","Jun","Jul","Aug","Sep","Oct","Nov","Dec"};
            float[] fac_diurnal = new float[25];
            float[] fac_seasonal = new float[13];
            
            try
            {
                // get the number of the selected Source Group = name
                string[] sg = new string[2];
                sg = listView1.SelectedItems[0].Text.Split(new char[] { ':' });
                string name = "";
                try
                {
                    sg[1] = sg[1].Trim();
                    name = sg[1];
                }
                catch
                {
                    name = sg[0];
                }
                
                //MessageBox.Show(modseasonal_index.ToString() +"/" + moddirunal_index.ToString());
                // now paint the modulation
                //pictureBox5.Refresh();
                if (EmissionModulationMap == null)
                {
                    EmissionModulationMap = new Bitmap(pictureBox5.Width, pictureBox5.Height);
                }

                using (Graphics g = Graphics.FromImage(EmissionModulationMap))
                {
                    int y0 = pictureBox5.Height / 2 - 7;
                    int y1 = pictureBox5.Height - 17;
                    Pen mypen = new Pen(Color.Black, 1);
                    Brush whitebrush = new SolidBrush(Color.White);
                    Brush b1 = new SolidBrush(Color.LightGray);
                    Brush b2 = new SolidBrush(Color.Black);
                    Font _smallFont = new Font("Arial", 6);
                    StringFormat format2 = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Center
                    };


                    g.FillRectangle(whitebrush, pictureBox5.ClientRectangle);


                    // Read emission modulation
                    if (ReadEmissionModulation(name, ref fac_diurnal, ref fac_seasonal) == false)
                    {
                        _smallFont = new Font("Arial", 9);
                        g.DrawString("File emissionmodulations.txt is not available at: ", _smallFont, b2, 14, _smallFont.Height);
                        g.DrawString(ProjectSetting.EmissionModulationPath, _smallFont, b2, 14, (int) (2 + _smallFont.Height * 2));
                        string newpath = Path.Combine(ProjectSetting.EmissionModulationPath, "emissions_timeseries.txt");
                        if (File.Exists(newpath)) // emission_timeseries ist used instead of modulation settings
                        {
                            g.DrawString("The evaluation is using the file emissions_timeseries.txt", _smallFont, b2, 14, (int)(8 + 3 * _smallFont.Height));
                        }
                        pictureBox5.Image = EmissionModulationMap;
                    }
                    else
                    {
                        float max_seasonal = Math.Max(fac_seasonal.Max(), 1);
                        float max_diurnal = Math.Max(fac_diurnal.Max(), 1);
                        //MessageBox.Show(max_seasonal.ToString()+"/"+max_diurnal.ToString());

                        float scalex = (pictureBox5.Width - 18) / 12;
                        float scaley = (y0 - 12) / max_seasonal;
                        for (int i = 0; i < 12; i++)
                        {
                            g.FillRectangle(b1, 13 + i * scalex, y0 - fac_seasonal[i] * scaley, scalex, fac_seasonal[i] * scaley);
                            g.DrawRectangle(mypen, 13 + i * scalex, y0 - fac_seasonal[i] * scaley, scalex, fac_seasonal[i] * scaley);
                            g.DrawString(months[i], _smallFont, b2, (int)(13 + i * scalex + 0.5 * scalex), y0 + 6, format2);
                        }
                        double div = Math.Round(max_seasonal / 2, 1);
                        format2.Alignment = StringAlignment.Near;
                        for (int i = 0; i < 3; i++)
                        {
                            g.DrawLine(mypen, 13, Convert.ToInt32(y0 - i * div * scaley), 15, Convert.ToInt32(y0 - i * div * scaley));
                            g.DrawString((div * i).ToString(), _smallFont, b2, 0, Convert.ToInt32(y0 - i * div * scaley), format2);
                        }

                        scalex = (pictureBox5.Width - 18) / 24;
                        scaley = (y1 - y0 - 22) / max_diurnal;
                        format2.Alignment = StringAlignment.Center;
                        for (int i = 0; i < 24; i++)
                        {
                            g.FillRectangle(b1, 13 + i * scalex, y1 - fac_diurnal[i] * scaley, scalex, fac_diurnal[i] * scaley);
                            g.DrawRectangle(mypen, 13 + i * scalex, y1 - fac_diurnal[i] * scaley, scalex, fac_diurnal[i] * scaley);
                            g.DrawString(i.ToString(), _smallFont, b2, (int)(13 + i * scalex + 0.5 * scalex), y1 + 6, format2);
                        }

                        div = Math.Round(max_diurnal / 2, 1);
                        format2.Alignment = StringAlignment.Near;
                        for (int i = 0; i < 3; i++)
                        {
                            g.DrawLine(mypen, 13, Convert.ToInt32(y1 - i * div * scaley), 15, Convert.ToInt32(y1 - i * div * scaley));
                            g.DrawString((div * i).ToString(), _smallFont, b2, 0, Convert.ToInt32(y1 - i * div * scaley), format2);
                        }


                        g.DrawLine(mypen, 5, y0, pictureBox5.Width - 5, y0);
                        g.DrawLine(mypen, 5, y1, pictureBox5.Width - 5, y1);
                        g.DrawLine(mypen, 13, 12, 13, y0 + 2);
                        g.DrawLine(mypen, 13, y0 + 15, 13, y1 + 2);
                        g.DrawString("Source group: " + listView1.SelectedItems[0].Text, _smallFont, b2, 14, 1);

                        pictureBox5.Image = EmissionModulationMap;
                        //map.Dispose();

                        string newpath = Path.Combine(ProjectSetting.EmissionModulationPath, "emissions_timeseries.txt");

                        if (File.Exists(newpath)) // emission_timeseries ist used instead of modulation settings
                        {
                            button49.Visible = true; // show the button for emissions_timeseries
                            button51.Visible = true; // show the button for emissions_timeseries
                            Font _largeFont = new Font("Arial", 12);
                            Brush b3 = new SolidBrush(Color.Red);
                            g.RotateTransform(330);
                            g.DrawString("The evaluation uses the file emissions_timeseries.txt", _largeFont, b3, -100, pictureBox5.Height - 70, format2);

                            _largeFont.Dispose();
                            b3.Dispose();
                        }
                        else
                        {
                            button49.Visible = false;
                            button51.Visible = false;
                        }
                    }

                    b1.Dispose();
                    b2.Dispose();
                    whitebrush.Dispose();
                    _smallFont.Dispose();
                    format2.Dispose();
                    mypen.Dispose();
                }
            }
            catch{}
        }
        
        private bool ReadEmissionModulation(string SG_name, ref float[] fac_diurnal, ref float[] fac_seasonal)
        {
            List<string> emissionmodulation = new List<string>();
            List<string> moddiurnal = new List<string>(); //collection of diurnal emission modulation data
            List<string> modseasonal = new List<string>(); //collection of seasonal emission modulation data
            string[] sg = new string[2];
            string newPath = Path.Combine(ProjectName, @"Settings", "emissionmodulations.txt");
            // in case of an emissionmodulation variation use the emissionmodulations.txt from the variation folder
            if (!string.Equals(Path.Combine(ProjectName, @"Computation"), ProjectSetting.EmissionModulationPath))
            {
                newPath = Path.Combine(ProjectSetting.EmissionModulationPath, "emissionmodulations.txt");
            }
            
            try
            {
                
                using (StreamReader _myReader = new StreamReader(newPath))
                {
                    while (_myReader.EndOfStream == false)
                    {
                        emissionmodulation.Add(_myReader.ReadLine().Replace('\t', ','));
                    }
                }
                
                newPath = Path.Combine(ProjectName, @"Settings", "seasonal_emissionmod.txt");
                using (StreamReader _myReader = new StreamReader(newPath))
                {
                    while(_myReader.EndOfStream == false)
                    {
                        modseasonal.Add(_myReader.ReadLine().Replace('\t', ','));
                    }
                }
                
                newPath = Path.Combine(ProjectName, @"Settings", "diurnal_emissionmod.txt");
                using (StreamReader _myReader = new StreamReader(newPath))
                {
                    while(_myReader.EndOfStream == false)
                    {
                        moddiurnal.Add(_myReader.ReadLine().Replace('\t', ','));
                    }
                }
                
                foreach(string dum in emissionmodulation)
                {
                    sg = dum.Split(new char[] { ',' }); // sg[0] = Source group
                    if (sg.Count() < 2)
                    {
                        return false;
                    }

                    // find source group
                    if (String.Compare(sg[0], SG_name) == 0)
                    {
                        //find the diurnal modulation
                        string[] text1;
                        foreach (string dum2 in moddiurnal)
                        {
                            text1 = dum2.Split(new char[] { '\t', ',' });
                            if (text1[0].Trim() == sg[1].Trim())
                            {
                                // moddirunal_index = j;
                                for (int k = 1; k < text1.Length; k++)
                                {
                                    if (k < fac_diurnal.Length)
                                    {
                                        fac_diurnal[k - 1] = (float) St_F.TxtToDbl(text1[k], false);
                                    }
                                }
                                break;
                            }
                        }
                        //find the seasonal modulation
                        
                        foreach (string dum2 in modseasonal)
                        {
                            text1 = dum2.Split(new char[] { '\t', ',' });
                            if (text1[0].Trim() == sg[2].Trim())
                            {
                                // modseasonal_index = j;
                                for (int k = 1; k < text1.Length; k++)
                                {
                                    if (k < fac_seasonal.Length)
                                    {
                                        fac_seasonal[k - 1] = (float) St_F.TxtToDbl(text1[k], false);
                                    }
                                }
                                break;
                            }
                        }
                   }
                }
                
                emissionmodulation.Clear();
                moddiurnal.Clear();
                modseasonal.Clear();
                
                return true;	
            }
            catch
            {
                emissionmodulation.Clear();
                moddiurnal.Clear();
                modseasonal.Clear();
                
                return false;
            }
        }
        
        void Panel1Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            FontFamily fontFam = new FontFamily("Arial");
            Font titlefont = new Font(fontFam, 19, FontStyle.Bold);
            Font subtitlefont = new Font(fontFam, 11, FontStyle.Bold);
            Font subtitlefontUnderlined = new Font(fontFam, 11, FontStyle.Bold | FontStyle.Underline);

            StringFormat format1 = new StringFormat
            {
                LineAlignment = StringAlignment.Near,
                Alignment = StringAlignment.Center
            };
            Brush Solid_blue = new SolidBrush(Color.Blue);
            Brush Solid_black = new SolidBrush(Color.Black);
            
            int x = panel1.Width / 2;
            int distance =  (int) (g.MeasureString("J", titlefont, 200).Height);
            int distance2 =  (int) (g.MeasureString("J", subtitlefont, 200).Height);
            string version = Application.ProductVersion.Replace(".", string.Empty);

            if (version.Length > 3)
            {
                version = version.Substring(0, 2) + "." + version.Substring(2);
            }
            g.DrawString("GRAL GUI V" + version + "Beta1 - Graz Lagrangian Model", titlefont, Solid_blue, x, 25, format1);
#if __MonoCS__
            g.DrawString("Compiled for Linux - MONO", subtitlefont, Solid_blue, x, 25 + distance, format1);
#else
#if NET6_0
            g.DrawString("Compiled for Windows .NET6", subtitlefont, Solid_blue, x, 25 + distance, format1);
#elif NET7_0
            g.DrawString("Compiled for Windows .NET7", subtitlefont, Solid_blue, x, 25 + distance, format1);
#elif NET8_0
            g.DrawString("Compiled for Windows .NET8", subtitlefont, Solid_blue, x, 25 + distance, format1);
#else
            g.DrawString("Compiled for Windows", subtitlefont, Solid_blue, x, 25 + distance, format1);
#endif
#endif
            g.DrawString("Development Team: Dietmar Oettl and Markus Kuntner", subtitlefont, Solid_black, x, 25 + 2 * distance, format1);

            int stringlen = (int)(g.MeasureString("Support and Training: Graz University of Technology gral@ivt.tugraz.at", subtitlefont).Width);
            g.DrawString("Support and Training: Graz University of Technology", subtitlefont, Solid_black, x - stringlen / 2, 25 + 2 * distance + distance2);
            int stringlen2 = (int)(g.MeasureString("Support and Training: Graz University of Technology", subtitlefont).Width);
            g.DrawString("gral@ivt.tugraz.at", subtitlefontUnderlined, Solid_blue, x - stringlen / 2 + stringlen2, 25 + 2 * distance + distance2);

            OpenMailToIVT = new Rectangle(x - stringlen / 2 + stringlen2, 
                                          25 + 2 * distance + distance2, 
                                          (int)(g.MeasureString("gral@ivt.tugraz.at", subtitlefontUnderlined).Width), 
                                          distance);

            format1.Dispose();
            titlefont.Dispose();
            subtitlefont.Dispose();
            subtitlefontUnderlined.Dispose();
            Solid_black.Dispose();
            Solid_blue.Dispose();
            fontFam.Dispose();
            g.Dispose();	
        }
    }
}