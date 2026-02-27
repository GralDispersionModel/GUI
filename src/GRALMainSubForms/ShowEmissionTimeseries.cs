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
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GralMainForms
{
    /// <summary>
    /// Show an emission time series
    /// </summary>
    public partial class ShowEmissionTimeseries : Form
    {
        private List<float>[] emifac = new List<float>[0];

        private List<string> Date_Time = new List<string>();
        private List<int> SG_List = new List<int>();
        private CultureInfo ic = CultureInfo.InvariantCulture;
        private float emifacmax = 0;
        private List<float> emi_mean = new List<float>();
        private List<Color> Col_List = new List<Color>();
        private int SG_Number = 0;
        private bool block_drawing_while_resize = false;
        private CheckBox[] check = new CheckBox[1];
        private int Hor_Scale = 1;

        private int _SGShow;
        public int SG_Show { set { _SGShow = value; } }
        public string ModulationPath;

        public ShowEmissionTimeseries()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            hScrollBar1.Maximum = 100 + hScrollBar1.LargeChange - 1;
        }

        void ShowEmissionTimeseriesLoad(object sender, EventArgs e)
        {
            string newpath = Path.Combine(Gral.Main.ProjectName, "Computation", "emissions_timeseries.txt");
            if (Directory.Exists(ModulationPath))
            {
                newpath = Path.Combine(ModulationPath, "emissions_timeseries.txt");
            }

            if (File.Exists(newpath) == true)
            {
                try
                {
                    long lines = GralStaticFunctions.St_F.CountLinesInFile(newpath);
                    int _sg_number = 0;
                    //read timeseries of emissions
                    string[] text10 = new string[1];

                    using (StreamReader read = new StreamReader(newpath))
                    {
                        // Header get source group numbers
                        text10 = read.ReadLine().Split(new char[] { ' ', ':', '-', '\t', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        _sg_number = text10.Length - 2;
                        if (_sg_number > 0)
                        {
                            for (int i = 2; i < text10.Length; i++)
                            {
                                int sg = 0;
                                if (Int32.TryParse(text10[i], out sg))
                                {
                                    SG_List.Add(sg);
                                }
                            }
                        }
                    }

                    emifac = new List<float>[_sg_number];

                    for (int i = 0; i < _sg_number; i++)
                    {
                        emifac[i] = new List<float>();
                        emi_mean.Add(0);
                    }

                    using (StreamReader read = new StreamReader(newpath))
                    {
                        //Header
                        text10 = read.ReadLine().Split(new char[] { ' ', ':', '-', '\t', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

                        while (read.EndOfStream == false)
                        {
                            text10 = read.ReadLine().Split(new char[] { ' ', ':', '-', '\t', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

                            if (text10.Length >= _sg_number + 2)
                            {
                                Date_Time.Add(text10[0] + "." + text10[1] + ":00");
                                for (int i = 0; i < _sg_number; i++)
                                {
                                    float _val = Convert.ToSingle(text10[i + 2], ic);
                                    emifac[i].Add(_val);
                                    emifacmax = Math.Max(emifacmax, _val);
                                    emi_mean[i] += _val;
                                    //MessageBox.Show(emifac[i].Last().ToString());
                                }
                            }
                        }
                    }

                    for (int i = 0; i < _sg_number; i++)
                    {
                        if (emifac[i].Count > 0)
                        {
                            emi_mean[i] = (float)Math.Round(emi_mean[i] / emifac[i].Count, 2);
                        }
                    }

                    SG_Number = _sg_number;
                    //MessageBox.Show(emifac[0].Count.ToString());

                    if (emifac[0].Count == 0)
                    {
                        Close();
                    }

                    Col_List.Add(Color.Blue);
                    Col_List.Add(Color.Red);
                    Col_List.Add(Color.SpringGreen);
                    Col_List.Add(Color.Turquoise);
                    Col_List.Add(Color.Beige);
                    Col_List.Add(Color.Cyan);
                    Col_List.Add(Color.OrangeRed);
                    Col_List.Add(Color.LightSteelBlue);
                    Col_List.Add(Color.Khaki);

                    check = new CheckBox[SG_Number];

                    for (int i = 0; i < SG_Number; i++)
                    {
                        if (i < SG_List.Count && (_SGShow == -1 || _SGShow == SG_List[i])) // Show selected SG
                        {
                            int c_i = i % Col_List.Count;
                            check[i] = new CheckBox
                            {
                                Font = new Font("Arial", 12, FontStyle.Regular, GraphicsUnit.Pixel),
                                Visible = true
                            };
                            check[i].Top = 35 + i * (check[i].Font.Height + 6);
                            check[i].Left = Math.Max(0, ClientSize.Width - 220);
                            check[i].Text = "Source group nr. " + SG_List[i].ToString() + " Mean : " + emi_mean[i].ToString();
                            check[i].Width = 219;
                            check[i].Height = check[i].Font.Height + 4;
                            check[i].Anchor = AnchorStyles.Right | AnchorStyles.Top;
                            check[i].Checked = true;
                            check[i].ForeColor = Col_List[c_i];
                            check[i].Tag = SG_List[i];
                            check[i].CheckedChanged += new EventHandler(CheckStatusChanged);
                            Controls.Add(check[i]);
                        }
                        else
                        {
                            check[i] = new CheckBox
                            {
                                Visible = false,
                                Checked = false
                            };
                        }
                    }
                }
                catch
                {
                    //MessageBox.Show(ex.Message);
                    Close();
                    return;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (block_drawing_while_resize) // avoid drawing while resize
            {
                return;
            }

            Graphics g = e.Graphics;
            g.Clear(Color.White);
            base.OnPaint(e);

            double scalefactor = (ClientSize.Width - 205.0) / emifac[0].Count * Hor_Scale;
            double vertscale = 1;
            if (emifacmax > 0)
            {
                vertscale = (ClientSize.Height - 80 - hScrollBar1.Height) / emifacmax;
            }
            float Hor_Scroll = (float)((emifac[0].Count) * hScrollBar1.Value / 100F);

            if (scalefactor > 0 && vertscale > 0)
            {
                int y0 = ClientSize.Height - 30 - hScrollBar1.Height;
                Font _smallFont = new Font("Arial", 8);
                StringFormat format1 = new StringFormat();
                StringFormat format2 = new StringFormat();
                format1.LineAlignment = StringAlignment.Center;
                format1.Alignment = StringAlignment.Center;
                format2.Alignment = StringAlignment.Center;
                Brush black = new SolidBrush(Color.Black);

                //draw axis
                int yb = ClientSize.Height - 35 - hScrollBar1.Height;

                Pen p1 = new Pen(Color.Black, 1);
                Pen p3 = new Pen(Color.Black, 0.5f)
                {
                    DashStyle = DashStyle.Dash
                };

                g.DrawLine(p1, 35, y0, 35, 20);
                g.DrawLine(p1, 30, yb, y0, yb);

                float div = (float)(emifacmax / 4);

                for (int i = 0; i < 6; i++)
                {
                    g.DrawLine(p1, 33, (int)(yb - i * div * vertscale), 38, (int)(yb - i * div * vertscale));
                    g.DrawLine(p3, 35, (int)(yb - i * div * vertscale), ClientSize.Width - 30, (int)(yb - i * div * vertscale));
                    g.DrawString(Math.Round(div * i, 1).ToString(), _smallFont, black, 20, (int)(yb - i * div * vertscale), format1);
                }

                g.DrawLine(p3, 33, yb, ClientSize.Width - 30, (int)yb);

                div = (float)(emifac[0].Count / (12F * Hor_Scale));
                for (int i = 0; i < 12 * Hor_Scale; i++)
                {
                    float x = (float)(35 + (i) * div * scalefactor - Hor_Scroll * scalefactor);
                    if (x > 0 && x < ClientSize.Width)
                    {
                        g.DrawLine(p1, x, yb - 4, x, yb + 4);
                        int index = (int)(div * i);
                        if (/*emifac[0].Count() < 100 ||*/ index > Date_Time.Count())
                        {
                            g.DrawString(Math.Round(div * i, 0).ToString(), _smallFont, black, x, yb + 15, format2);
                        }
                        else
                        {
                            g.DrawString(Date_Time[index].ToString(), _smallFont, black, x, yb + 15, format2);
                        }
                    }
                }

                base.OnPaint(e);

                int count = 0;
                // clip output
                g.Clip = new Region(new Rectangle(35, 35, ClientSize.Width - 35, yb + 4));

                List<PointF> graph = new List<PointF>();
                PointF pt = new PointF();

                for (int i = 0; i < SG_Number; i++)
                {
                    if (i < SG_List.Count && check[i].Checked) // Show selected SG
                    {
                        int c_i = i % Col_List.Count;
                        int x_old = -99999;

                        Pen p2 = new Pen(Col_List[c_i], 2);
                        Brush cbrush = new SolidBrush(Col_List[c_i]);

                        for (int t = 0; t < emifac[i].Count; t++)
                        {
                            int x = (int)(35 + (t - Hor_Scroll) * scalefactor);
                            if (x > -1 * ClientSize.Width && x < (ClientSize.Width * 2))
                            {
                                if (x != x_old) // show points with new x-coordinate (performance)
                                {
                                    int y = (int)(yb - emifac[i][t] * vertscale);
                                    pt.X = x;
                                    pt.Y = y;
                                    graph.Add(pt);
                                    if (scalefactor > 1) // show steps
                                    {
                                        pt.X = (int)(x + scalefactor);
                                        pt.Y = y;
                                        graph.Add(pt);
                                    }
                                    x_old = x;
                                }
                            }
                        }

                        if (graph.Count > 1)
                        {
                            g.DrawLines(p2, graph.ToArray());
                        }
                        graph.Clear();

                        p2.Dispose();
                        cbrush.Dispose();
                        count++;
                    }
                }

                p1.Dispose(); p3.Dispose();
                format1.Dispose();
                format2.Dispose();
                black.Dispose();
                g.ResetClip();
                _smallFont.Dispose();
            }
        }

        void Show_Emission_TimeseriesResizeEnd(object sender, EventArgs e)
        {
            block_drawing_while_resize = false;
            Invalidate();
            Update();
        }

        void Button4Click(object sender, EventArgs e)
        {
            Bitmap bitMap = new Bitmap(Width, Height);
            DrawToBitmap(bitMap, new Rectangle(0, 0, Width, Height));
            Clipboard.SetDataObject(bitMap);
        }

        void CheckStatusChanged(object sender, EventArgs e)
        {
            Invalidate();
            Update();
        }

        void Show_Emission_TimeseriesResizeBegin(object sender, EventArgs e)
        {
            block_drawing_while_resize = true;
        }

        void Show_Emission_TimeseriesFormClosed(object sender, FormClosedEventArgs e)
        {
            for (int i = 0; i < SG_Number; i++)
            {
                if (i < SG_List.Count && (_SGShow == -1 || _SGShow == SG_List[i])) // Show selected SG
                {

                    check[i].CheckedChanged -= new EventHandler(CheckStatusChanged);
                    check[i] = null;
                }
                else
                {
                    check[i] = null;
                }
            }
            Col_List = null;
            SG_List = null;
            emi_mean = null;
            emifac = null;
        }

        void HScrollBar1Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
            Update();
        }

        void Button2Click(object sender, EventArgs e)
        {
            if (Hor_Scale > 1)
            {
                Hor_Scale /= 2;
                Invalidate();
                Update();
            }
        }

        void Button1Click(object sender, EventArgs e)
        {
            if (Hor_Scale < 40)
            {
                Hor_Scale *= 2;
                Invalidate();
                Update();
            }
        }

        void ShowEmissionTimeseriesResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                Invalidate();
                Update();
            }
            if (WindowState == FormWindowState.Normal)
            {
                Invalidate();
                Update();
            }
        }
    }
}
