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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace GralDomForms
{
	/// <summary>
	/// Show static vertical profiles
	/// </summary>       
    public partial class VerticalProfile_Static : Form
    {
        public string file;      //file, which is spotted
        private string decsep;    //system decimal seperator
        private int zlevels = 0;      //total number of z-levels of the GRAMM fields
        private int zlevels_userdefined = 0; //user defined zlevels to be drawn
        private double min = 10000000;  //minimum of the GRAMM field
        private double max = -10000000; //maximum of the GRAMM field
        private double[] zsp = new double[2];  //height of the GRAMM grid point
        private double[] val = new double[2];  //value of the GRAMM grid point
        private double horscale = 1;
        private double vertscale = 1;

        public VerticalProfile_Static()
        {
            InitializeComponent();
            //User defined column seperator and decimal seperator
            decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        }

        private void VerticalProfile_Load(object sender, EventArgs e)
        {
            //diagramm header
            string header = Path.GetFileName(file);
            header = header.Substring(9);
            header = header.Replace(".txt", "");
            Text = header;
            readfield();
        }

        //read the Vertical Profile
        private void readfield()
        {
            string[] text = new string[2];
            try
            {
                long number_of_lines = GralStaticFunctions.St_F.CountLinesInFile(file) + 2;
                zsp = new double[number_of_lines];  //height of the GRAMM grid point
                val  = new double[number_of_lines];  //value of the GRAMM grid point

                using (StreamReader reader = new StreamReader(file))
                {
                    int i = 0;
                    while (reader.EndOfStream == false)
                    {
                        text = reader.ReadLine().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        zsp[i] = Convert.ToDouble(text[0].Replace(".", decsep));
                        val[i] = Convert.ToDouble(text[1].Replace(".", decsep));

                        if (zsp[i] < 100)
                        {
                            zlevels_userdefined = i;
                        }
                        i = i + 1;
                    }

                    zlevels = i;
                }
            }
            catch
            {
                
            }
            Refresh();
        }

        //draw diagram
        protected override void OnPaint(PaintEventArgs e)
        {
        	base.OnPaint(e);
            Graphics g = e.Graphics;
           
            int wi = Math.Max(0, Width - 40);
            int he = Math.Max(0, Height - 20);
            int leftbound = Convert.ToInt32(wi / 7);
            int rightbound = wi - Convert.ToInt32(wi / 20);
            int bottombound = he - Convert.ToInt32(he / 7);
            int topbound = Convert.ToInt32(he / 20);

            Font font = new Font("Arial", Convert.ToInt32(Math.Max(he / 50, 1)));
            Pen p1 = new Pen(Color.Red, 3);
            Pen p2 = new Pen(Color.Black, 2)
            {
                EndCap = LineCap.ArrowAnchor
            };

            StringFormat format1 = new StringFormat();
            StringFormat format2 = new StringFormat();
            format1.LineAlignment = StringAlignment.Center;
            format1.Alignment = StringAlignment.Center;
            format2.Alignment = StringAlignment.Far;

            //draw axis
            g.DrawLine(p2, leftbound, bottombound, rightbound, bottombound);
            g.DrawLine(p2, leftbound, bottombound, leftbound, topbound);

            //compute minimum/maximum
            min = 10000000;
            max = -10000000;
            for (int i = 0; i < zlevels_userdefined; i++)
            {
                min = Math.Min(min, val[i]);
                max = Math.Max(max, val[i]);
            }

            //horizontal/vertical scales
            if (zlevels_userdefined > 0)
            {
                if (max - min > 0)
                    horscale = (rightbound - leftbound) / (max - min);
                else
                    horscale = 1;
               
                double vert_diff = zsp[zlevels_userdefined - 1] - zsp[0];
                if (vert_diff > 0)
                	vertscale = (bottombound - topbound) / vert_diff;

                int x1 = Convert.ToInt32(leftbound-2);
                int y1 = Convert.ToInt32(bottombound+2);
                g.DrawString(Convert.ToString(Math.Round(zsp[0],0)), font, new SolidBrush(Color.Black), x1, y1 - font.Size*2, format2);
                g.DrawString(Convert.ToString(Math.Round(zsp[zlevels_userdefined - 1], 0)), font, new SolidBrush(Color.Black), x1, y1 - font.Size * 2 - (bottombound - topbound), format2);
                g.DrawString("<" + Convert.ToString(Math.Round(min, 2)) + ">", font, new SolidBrush(Color.Black), x1 - font.Size * 2, y1 + 2 * font.Size, format1);
                g.DrawString("<" + Convert.ToString(Math.Round(max, 2)) + ">", font, new SolidBrush(Color.Black), x1 + (rightbound - leftbound) - font.Size * 2, y1 + 2 * font.Size, format1);
            
                int y_step = 1000;
                if (zsp[zlevels_userdefined - 1] < 4000)
                	y_step = 500;
                if (zsp[zlevels_userdefined - 1] < 2000)
                	y_step = 250;
                if (zsp[zlevels_userdefined - 1] < 600)
                	y_step = 100;
                if (zsp[zlevels_userdefined - 1] < 250)
                	y_step = 50;
                if (zsp[zlevels_userdefined - 1] < 120)
                	y_step = 20;
                
                double x_step = 45;
                if ((max - min) < 90)
                	x_step = 30;
                if ((max - min) < 60)
                	x_step = 15;
                if ((max - min) < 30)
                	x_step = 5;
                if ((max - min) < 10)
                	x_step = 2;
                if ((max - min) < 5)
                	x_step = 1;
                if ((max - min) < 2)
                	x_step = 0.5;

                Pen p3 = new Pen(Color.Black, 1)
                {
                    DashStyle = DashStyle.Dash
                };


                for (int i = 1; i < 10; i++)
                {
                	int y = (int) (bottombound - y_step * i * vertscale);
                	
                	if (y > (y1 - font.Size * 2 - (bottombound - topbound)+10))
                	{
                		g.DrawLine(p3, leftbound, y, rightbound, y);
                		g.DrawString(Convert.ToString(y_step * i), font, new SolidBrush(Color.Black), x1, y, format2);
                	}
                }
                
                int start = (int) (min / x_step);
                for (int i = start; i < start + 10; i++)
                {
                	int x = (int) (leftbound + (x_step * i - min) * horscale);
                	
                	if (x > (x1 + 5) && x < rightbound)
                	{
                		g.DrawLine(p3, x, bottombound, x, topbound);
                		g.DrawString(Convert.ToString(x_step * i), font, new SolidBrush(Color.Black), x, y1 + font.Size, format1);
                	}
                }
                p3.Dispose();
             }

            //draw profile
            for (int i = 0; i < zlevels-1; i++)
            {
                int x1=Convert.ToInt32(leftbound + (val[i] - min) * horscale);
                int y1 = Convert.ToInt32(bottombound - (zsp[i] - zsp[0]) * vertscale);
                int x2 = Convert.ToInt32(leftbound + (val[i+1] - min) * horscale);
                int y2 = Convert.ToInt32(bottombound - (zsp[i+1] - zsp[0]) * vertscale);
                g.DrawLine(p1, x1, y1, x2, y2);
             }

            p1.Dispose();
            p2.Dispose();
            font.Dispose();
        }

       // zoom in
        private void button1_Click(object sender, EventArgs e)
        {
            zlevels_userdefined = Math.Max(1, zlevels_userdefined - 1);
            Refresh();
        }
        // zoom out
        private void button2_Click(object sender, EventArgs e)
        {
        	zlevels_userdefined = Math.Min(zlevels, zlevels_userdefined + 1);
            Refresh();
        }
        
        void VerticalProfile_StaticResizeEnd(object sender, EventArgs e)
        {
        	Refresh();
        }
        
        void VerticalProfile_StaticKeyDown(object sender, KeyEventArgs e)
        { 
            if (e.KeyCode == Keys.Add)
            {
                button1_Click(null, null);
            }
            if (e.KeyCode == Keys.Subtract)
            {
                button2_Click(null, null);
            }
        }
    }
}