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
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

namespace GralMainForms
{
    /// <summary>
    /// Form to show total annual emissions for all source groups with or without emission modulation
    /// </summary>
    public partial class TotalEmissions : Form
    {
        //private Domain domain = null;
        private Gral.Main form1 = null;
        private string polli;
        private double[,] moddiurnal = new double[100,24];     //collection of diurnal emission modulation data
        private double[,] modseasonal = new double[100, 12];   //collection of seasonal emission modulation data
        private double[] EmissionFactor = new double[100];
        private double scalefactor = 1;
        private double[] totalemissions;
        private CultureInfo ic = CultureInfo.InvariantCulture;
        private bool Emissions_Time_Series_Used = false;
        private List <string> Date_Time = new List<string>();
        private string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

        public TotalEmissions(double[] totemi, Gral.Main f, string poll)
        {
            totalemissions = totemi;
            InitializeComponent();
            polli = poll;
            form1 = f;
        }
        
        //compute mean emission factor for each source group based on the chosen diurnal/seasonal variation
        private void TotalEmissionsLoad(object sender, EventArgs e)
        {
            //compute mean emission factor for each source group based on the chosen diurnal/seasonal variation
            string[] selpoll = new string[2];
            int sgroup = 0;
            string newpath;
            string snumb;
            string[] text = new string[3];
            double emifac_diurnal = 0;
            double emifac_seasonal = 0;
            
            if (Emission_Timeseries_Read() == false)
            {
                try
                {
                    for(int i = 0; i < form1.listView1.Items.Count; i++)
                    {
                        emifac_diurnal = 0;
                        emifac_seasonal = 0;
                        
                        //get source group
                        selpoll = form1.listView1.Items[i].SubItems[0].Text.Split(new char[] { ':' });
                        try
                        {
                            sgroup = Convert.ToInt32(selpoll[1]);
                            if(sgroup>9)
                            {
                                snumb ="0" + selpoll[1].Trim();
                            }
                            else
                            {
                                snumb = "00" + selpoll[1].Trim();
                            }
                        }
                        catch
                        {
                            sgroup = Convert.ToInt32(selpoll[0]);
                            if(sgroup>9)
                            {
                                snumb = "0" + selpoll[0].Trim();
                            }
                            else
                            {
                                snumb = "00" + selpoll[0].Trim();
                            }
                        }
                        
                        //get variation for source group
                        newpath = Path.Combine("Computation", "emissions" + snumb + ".dat");
                        
                        if (sgroup < moddiurnal.GetUpperBound(0))
                        {
                            newpath = Path.Combine(Gral.Main.ProjectName,newpath);
                            if (File.Exists(newpath))
                            {
                                using (StreamReader myreader = new StreamReader(newpath))
                                {
                                    for(int j=0;j<24;j++)
                                    {
                                        text = myreader.ReadLine().Split(new char[] { ',' });
                                        moddiurnal[sgroup, j] = Convert.ToDouble(text[1].Replace(".", decsep));
                                        if(j<12)
                                        {
                                            modseasonal[sgroup, j] = Convert.ToDouble(text[2].Replace(".", decsep));
                                        }
                                    }
                                }
                            }
                            
                            //diurnal variation
                            for(int j=0;j<24;j++)
                            {
                                emifac_diurnal += moddiurnal[sgroup, j] / 24;
                            }
                            //seasonal variation
                            for (int j=0;j<12;j++)
                            {
                                emifac_seasonal += modseasonal[sgroup, j] / 12;
                            }

                            EmissionFactor[i] = emifac_diurnal * emifac_seasonal;
                        }
                    }
                }
                catch
                {
                }
            }

            pictureBox1.Width = ClientSize.Width;
            pictureBox1.Height = ClientSize.Height;
            pictureBox1.Refresh();
            if (form1 != null)
            {
                Location = new Point(Math.Max(0,form1.Location.X + form1.Width / 2 - Width / 2 - 100),
                                     Math.Max(0, form1.Location.Y + form1.Height / 2 - Height / 2 -100));
            }
        }

        private bool Emission_Timeseries_Read()
        {
            int[] sg_numbers = new int[102];
            int[] sg_Listbox = new int[102];
            
            // Get SG Numbers from Listbox
            for(int i = 0; i < form1.listView1.Items.Count; i++)
            {
                //get source group
                string[] selpoll = new string[2];
                selpoll = form1.listView1.Items[i].SubItems[0].Text.Split(new char[] { ':' });
                try
                {
                    int sgroup = Convert.ToInt32(selpoll[1]);
                    if (sgroup < 100)
                    {
                        sg_Listbox[i] = sgroup;
                    }
                }
                catch
                {
                    int sgroup = Convert.ToInt32(selpoll[0]);
                    if (sgroup < 100)
                    {
                        sg_Listbox[i] = sgroup;
                    }
                }
            }
            
            
            string newpath = Path.Combine(Gral.Main.ProjectName, "Computation", "emissions_timeseries.txt");
            if (File.Exists(newpath) == true)
            {
                try
                {
                    //read timeseries of emissions
                    string[] text10 = new string[1];
                    int _sg_number = 0;
                    int _count = 0;
                    
                    using (StreamReader read = new StreamReader(newpath))
                    {
                        //get source group numbers
                        text10 = read.ReadLine().Split(new char[] { ' ', ':', '-', '\t', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        _sg_number = text10.Length - 2;
                        
                        if (text10.Length > 1)
                        {
                            for (int i = 2; i < text10.Length; i++)
                            {
                                //get the column corresponding with the source group number stored in sg_numbers
                                string sg_temp = text10[i];
                                int sg = 0;
                                if (Int32.TryParse(sg_temp, out sg))
                                {
                                    if (sg < 100)
                                    {
                                        sg_numbers[i - 2] = sg;
                                    }
                                    //MessageBox.Show(sg.ToString());
                                }
                            }
                        }
                        
                        while (read.EndOfStream == false)
                        {
                            text10 = read.ReadLine().Split(new char[] { ' ', ':', '-', '\t', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                            
                            if (text10.Length >= _sg_number + 2)
                            {
                                Date_Time.Add(text10[0] + ". " + text10[1] + ":00");
                                
                                for (int i = 0; i < _sg_number; i++)
                                {
                                    float _val = Convert.ToSingle(text10[i + 2], ic);
                                    
                                    for(int j = 0; j < form1.listView1.Items.Count; j++)
                                    {
                                        if (sg_numbers[i] == sg_Listbox[j])
                                        {
                                            EmissionFactor[j] += _val;
                                            //MessageBox.Show(emifac[sg_numbers[i]].ToString());
                                        }
                                    }
                                }
                                _count++;
                            }
                        }
                        
                        for (int i = 0; i < 100; i++)
                        {
                            if (_count > 0)
                            {
                                EmissionFactor[i] = EmissionFactor[i] / _count;
                            }
                            // set emission factor to 1 by default (like GRAL does)
                            if (EmissionFactor[i] < float.Epsilon)
                            {
                                EmissionFactor[i] = 1;
                            }
                        }
                        
                    } // using
                    Emissions_Time_Series_Used = true;
                    return true;
                }
                catch {  }
            }
            return false;
        }

        void PictureBox1Paint(object sender, PaintEventArgs e)
        {
            if (pictureBox1.Width < 20 || pictureBox1.Height < 20)
            {
                return;
            }

            int anz_sources = form1.listView1.Items.Count;
            // no more sources available -> close the form
            if (anz_sources == 0)
            {
                this.Close();
                return;
            }

            Graphics g = e.Graphics;
            
            g.ScaleTransform(pictureBox1.Width / 1000f, pictureBox1.Width / 1000f);
            g.Clear(Color.White);
            base.OnPaint(e);
            
            StringFormat format1 = new StringFormat();
            StringFormat format2 = new StringFormat();
            format1.LineAlignment = StringAlignment.Center;
            format1.Alignment = StringAlignment.Center;
            format2.Alignment = StringAlignment.Far;
            Brush black = new SolidBrush(Color.Black);
            
            string tile_string;
            
            if (Emissions_Time_Series_Used && checkBox1.Checked == true)
            {
                tile_string = "Total ";
                if (Date_Time.Count > 1)
                {
                    tile_string += Date_Time[0] + " - " + Date_Time[Date_Time.Count - 1] + " ";
                }
            }
            else
            {
                tile_string = "Total annual ";
            }
            
            if(polli == "Odour")
            {
                tile_string +=  polli + " Emissions within the Model Domain [MOU/a*10-3]";
            }
            else
            {
                tile_string +=  polli + " Emissions within the Model Domain [t/a]";
            }

            Font tile_font = new Font("Arial", 10);
            SizeF title_size = g.MeasureString(tile_string, tile_font);
            Font legend = new Font("Arial", 8);
            
            g.DrawString(tile_string, tile_font, black, (int) (1000 - title_size.Width) / 2, 13);
            
            //scaling factor
            double classmax = 0;
            
            string[] selpoll = new string[2];
            int sgroup = 0;
                
            for (int i = 0; i < anz_sources; i++)
            {
                selpoll = form1.listView1.Items[i].SubItems[0].Text.Split(new char[] {':'});
                try
                {
                    sgroup = Convert.ToInt32(selpoll[1]);
                }
                catch
                {
                    sgroup = Convert.ToInt32(selpoll[0]);
                }
                
                if (sgroup <= totalemissions.GetUpperBound(0))
                {
                    if(checkBox1.Checked==true)
                    {
                        classmax = Math.Max(totalemissions[sgroup]*EmissionFactor[i], classmax);
                    }
                    else
                    {
                        classmax = Math.Max(totalemissions[sgroup], classmax);
                    }
                }
            }
            
            if (classmax == 0)
            {
                return;
            }

            double scale = 400 / classmax * scalefactor;

            //draw diagram
            int chartwidth = Convert.ToInt32(650*scalefactor / anz_sources);
            int hoehe=0;
            int hoehe_max = 0;
            int ecke1=0;
            int round = 1;

            //draw frequency levels
            Pen p3 = new Pen(Color.DarkGray, 0.5f)
            {
                DashStyle = DashStyle.Dash
            };
            ecke1 = Convert.ToInt32((Convert.ToInt32(55 + (anz_sources - 1) * chartwidth)) * scalefactor);
            for (int i = 1; i < 6; i++)
            {
                int levels = Convert.ToInt32(Convert.ToDouble(i) * classmax * scale / 5);
                int lev1 = Convert.ToInt32((500 - levels) * scalefactor);
                g.DrawLine(p3, Convert.ToInt32(55 * scalefactor) - 3, lev1, ecke1 + chartwidth + 20, lev1);
                string s = Convert.ToString(Math.Round(Convert.ToDouble(i) * classmax / 5, round));
                g.DrawString(s, legend, black, Convert.ToInt32(50 * scalefactor), lev1 - 5, format2);
            }

            for (int i = 0; i < anz_sources; i++)
            {
                selpoll = form1.listView1.Items[i].SubItems[0].Text.Split(new char[] {':'});
                try
                {
                    sgroup = Convert.ToInt32(selpoll[1]);
                }
                catch
                {
                    sgroup = Convert.ToInt32(selpoll[0]);
                }
                
                double value = 0;
                
                if (sgroup <= totalemissions.GetUpperBound(0))
                {
                    if (checkBox1.Checked == true)
                    {
                        value = totalemissions[sgroup] * EmissionFactor[i];
                    }
                    else
                    {
                        value = totalemissions[sgroup];
                    }
                }
                
                round = get_round(value);
                
                hoehe = Convert.ToInt32(value * scale * scalefactor);
                
                hoehe_max = Math.Max(hoehe, hoehe_max);
                
                ecke1 = Convert.ToInt32((Convert.ToInt32(55 + i * chartwidth))*scalefactor);
                
                g.FillRectangle(new HatchBrush(HatchStyle.Percent90, Color.White), ecke1, Convert.ToInt32(500*scalefactor - hoehe), Convert.ToInt32(chartwidth*scalefactor), hoehe);
                g.DrawRectangle(new Pen(Color.Black), ecke1, Convert.ToInt32(500 * scalefactor - hoehe), Convert.ToInt32(chartwidth*scalefactor), hoehe);
                g.DrawString(Convert.ToString(Math.Round(value, round)), legend, black, ecke1 + Convert.ToInt32(chartwidth*scalefactor / 2), Convert.ToInt32(500 * scalefactor - hoehe-10), format1);
            }

            round = get_round(classmax);
            
            //draw axis
            Pen p1 = new Pen(Color.Black, 3);
            Pen p2 = new Pen(Color.Black, 3);
            
            p2.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(p1, Convert.ToInt32(55 * scalefactor), Convert.ToInt32(500 * scalefactor), ecke1 + chartwidth + 20, Convert.ToInt32(500 * scalefactor));
            g.DrawLine(p2, Convert.ToInt32(55 * scalefactor), Convert.ToInt32(500 * scalefactor), Convert.ToInt32(55 * scalefactor), (int) (500 * scalefactor - hoehe_max - 20));
            g.DrawString("  0", tile_font, black, Convert.ToInt32(25 * scalefactor), Convert.ToInt32(495 * scalefactor));
            base.OnPaint(e);

            //draw x-axis legend
            int addy = 0;
            for (int i = 0; i < anz_sources; i++)
            {
                ecke1 = Convert.ToInt32((55 + i * chartwidth) * scalefactor);
                selpoll = form1.listView1.Items[i].SubItems[0].Text.Split(new char[] { ':' });

                g.DrawString(selpoll[0], legend, black, ecke1 + Convert.ToInt32(chartwidth*scalefactor / 2), Convert.ToInt32((520 + addy) * scalefactor), format1);
                if (addy == 0)
                {
                    addy = Convert.ToInt32(15*scalefactor);
                }
                else
                {
                    addy = 0;
                }
            }

            p1.Dispose();p2.Dispose();p3.Dispose();
            format1.Dispose();
            format2.Dispose();
            legend.Dispose();
            tile_font.Dispose();
            black.Dispose();
        }

        private int get_round(double value)
        {
            int round = 1;
            if (value < 1)
            {
                round = 2;
            }

            if (value < 0.1)
            {
                round = 3;
            }

            if (value < 0.01)
            {
                round = 4;
            }

            if (value < 0.001)
            {
                round = 5;
            }

            return round;
        }
        
        //save image to clipboard
        private void button1_Click_1(object sender, EventArgs e)
        {
            int CopyToClipboardScale = 2;
            if (pictureBox1.Width < 800)
            {
                CopyToClipboardScale = 4;
            }
            pictureBox1.Width *= CopyToClipboardScale;
            pictureBox1.Height *= CopyToClipboardScale;
            pictureBox1.Refresh();
            Application.DoEvents();
            Bitmap bitMap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.DrawToBitmap(bitMap, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
            Clipboard.SetDataObject(bitMap);
            pictureBox1.Width /= CopyToClipboardScale;
            pictureBox1.Height /= CopyToClipboardScale;
            pictureBox1.Refresh();
        }

        //consider emission variation or not
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        //increase image
        private void button2_Click(object sender, EventArgs e)
        {
            scalefactor *= 1.1;
            Refresh();
        }

        //decrease image
        private void button3_Click(object sender, EventArgs e)
        {
            scalefactor *= 0.9;
            Refresh();
        }
        
        
        
        void Tot_EmissionsResizeEnd(object sender, EventArgs e)
        {
            pictureBox1.Width = ClientSize.Width;
            pictureBox1.Height = ClientSize.Height;
            pictureBox1.Refresh();
        }
        
        void Tot_EmissionsResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                Tot_EmissionsResizeEnd(null, null);
                // Maximized!
            }
            if (WindowState == FormWindowState.Normal)
            {
                Tot_EmissionsResizeEnd(null, null);
                // Restored!
            }
        }
        
        void Tot_EmissionsFormClosed(object sender, FormClosedEventArgs e)
        {
       
        }
    }
}