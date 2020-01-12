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
	/// Show dynamic vertical profiles
	/// </summary>       
    public partial class VerticalProfile_Dynamic : Form
    {
        public string file;      //file, which is spotted
        private string decsep;    //system decimal seperator
        private int zlevels=0;      //total number of z-levels of the GRAMM fields
        private int zlevels_userdefined = 0; //user defined zlevels to be drawn
        private double min = 10000000;  //minimum of the GRAMM field
        private double max = -10000000; //maximum of the GRAMM field
        private double[] zsp = new double[200];  //height of the GRAMM grid point
        private double[] val = new double[200];  //value of the GRAMM grid point
        private double horscale = 1;
        private double vertscale = 1;
        public FileSystemWatcher filewatch = new FileSystemWatcher();  //read the file "file" containing the GRAMM online fields

        public VerticalProfile_Dynamic()
        {
            InitializeComponent();
            //User defined column seperator and decimal seperator
            decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        }

        private void VerticalProfile_Load(object sender, EventArgs e)
        {
            //start file watcher for "File"
            filewatch.Path = Path.GetDirectoryName(file);
            filewatch.Filter = Path.GetFileName(file);
            filewatch.Changed += new FileSystemEventHandler(filewatch_Changed);
            filewatch.SynchronizingObject = this;
            filewatch.EnableRaisingEvents = true;

            //diagramm header
            string header = Path.GetFileName(file);
            header = header.Substring(9);
            header = header.Replace(".txt", "");
            Text = header;
        }

        //redraw vertical profile
        void filewatch_Changed(object sender, FileSystemEventArgs e)
        {
            readfield();
        }

        //read the GRAMM field
        private void readfield()
        {
            string[] text = new string[2];
            try
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    int i = 0;
                    while (reader.EndOfStream == false)
                    {
                        text = reader.ReadLine().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        zsp[i] = Convert.ToDouble(text[0].Replace(".", decsep));
                        val[i] = Convert.ToDouble(text[1].Replace(".", decsep));
                        if (i < zsp.Length)
                            i = i + 1;
                    }
                    zlevels = i;
                    zlevels_userdefined = zlevels;
                }
            }
            catch
            { }
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

            
            //compute minimum/maximum
            min = 10000000;
            max = -10000000;
            for (int i = 0; i < zlevels_userdefined; i++)
            {
                min = Math.Min(min, val[i]);
                max = Math.Max(max, val[i]);
            }

            //draw axis
            g.DrawLine(p2, leftbound, bottombound, rightbound, bottombound);
            g.DrawLine(p2, leftbound, bottombound, leftbound, topbound);
				
           //horizontal/vertical scales
            if (zlevels > 0)
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
                g.DrawString(Convert.ToString(Math.Round(min, 2)), font, new SolidBrush(Color.Black), x1 - font.Size * 2, y1 + font.Size, format1);
                g.DrawString(Convert.ToString(Math.Round(max, 2)), font, new SolidBrush(Color.Black), x1 + (rightbound - leftbound) - font.Size * 2, y1 + font.Size, format1);
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
        
        void VerticalProfile_DynamicResizeEnd(object sender, EventArgs e)
        {
        	 Refresh();
        }
        
        void VerticalProfile_DynamicFormClosed(object sender, FormClosedEventArgs e)
        {
        	 filewatch.Changed -= new FileSystemEventHandler(filewatch_Changed);
        }
    }
}