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
//using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using GralIO;
using Gral.GRALDomForms;
using System.Drawing;

namespace GralDomForms
{
    /// <summary>
    /// Mathematical raster operation dialog
    /// </summary>
    public partial class Mathrasteroperation : Form
    {
        private Gral.Main form1 = null;
        private GralDomain.Domain domain = null;
        private string unit = "";
        public string RasterA;
        public string RasterB;
        public string RasterC;
        public string RasterD;
        public string RasterG;
        public string RasterF;

        private string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;  


        public Mathrasteroperation(Gral.Main f, GralDomain.Domain f1)
        {
            InitializeComponent();
            form1 = f;
            domain = f1;
            textBox1.KeyPress += new KeyPressEventHandler(Comma); //only point as decimal seperator is allowed
         }

        private void Mathrasteroperation_Load(object sender, EventArgs e)
        {
            try
            {
                Left = Math.Max(10, domain.Left + (int) (domain.Width * 0.5F) - 360);
                Top = domain.Top + 80;
            }
            catch
            { }
        }
        
        private string OpenFileDialogRaster(ListBox listbox)
        {
            string raster = string.Empty;
            using (OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "(*.dat;*.txt)|*.dat;*.txt",
                Title = "Select raster data (ASCII Format)",
                InitialDirectory = Path.Combine(Gral.Main.ProjectName, @"Maps" + Path.DirectorySeparatorChar),
                FileName = String.Empty,
                //ShowHelp = true,
                RestoreDirectory = true
#if NET6_0_OR_GREATER
                ,ClientGuid = GralStaticFunctions.St_F.FileDialogMaps
#endif
            })
            {
                dialog.HelpRequest += new System.EventHandler(LoadRasterFiles_HelpRequest);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    listbox.Items.Clear();
                    raster = dialog.FileName;
                    string lboxstring = raster;
                    if (raster.Length > 55)
                    {
                        try
                        {
                            lboxstring = raster.Substring(0, 25) + "...." + Path.DirectorySeparatorChar +
                                Path.GetFileName(raster);
                        }
                        catch { }
                    }
                    listbox.Items.Add(lboxstring);
                }
            }
            return raster;
        }

        private void LoadRasterFiles_HelpRequest(object sender, System.EventArgs e)
        {
            MessageBox.Show(this, "Select an ESRI ASCii raster file \n *.txt or *.dat file");
        }

        //open raster data set A
        private void button1_Click(object sender, EventArgs e)
        {
            RasterA = OpenFileDialogRaster(listBox1);
            
            unit = String.Empty;
            GralIO.ESRIHeader readESRIHeader = new ESRIHeader();
            if (readESRIHeader.ReadESRIHeader(RasterA))
            {
                unit = readESRIHeader.Unit;
            }
            
            textBox2.Text = unit;
            groupBox2.Enabled = true;
            groupBox3.Enabled = true;
            groupBox5.Enabled = true;
            groupBox6.Enabled = true;
            groupBox7.Enabled = true;
        }

        //open raster data set B
        private void button2_Click(object sender, EventArgs e)
        {
            RasterB = OpenFileDialogRaster(listBox2);
        }

        //open raster data set C
        private void button7_Click(object sender, EventArgs e)
        {
            RasterC = OpenFileDialogRaster(listBox4);
        }

        //open raster data set D
        private void button8_Click(object sender, EventArgs e)
        {
            RasterD = OpenFileDialogRaster(listBox5);
        }

        //open raster data set E
        private void button9_Click(object sender, EventArgs e)
        {
            RasterG = OpenFileDialogRaster(listBox6);
        }

        //define raster data set F
        private void button3_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = saveFileDialog1)
            {
                dialog.Filter = "(*.dat;*.txt)|*.dat;*.txt";
                dialog.Title = "Define output raster data (ASCII Format)";
                dialog.InitialDirectory = Path.Combine(Gral.Main.ProjectName, @"Maps" + Path.DirectorySeparatorChar);
                // dialog.ShowHelp = true;
                dialog.RestoreDirectory = true;
#if NET6_0_OR_GREATER
                dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogMaps;
#endif

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    listBox3.Items.Clear();
                    RasterF = dialog.FileName;
                    listBox3.Items.Add(RasterF);
                }
            }
        }

        //Cancel
        private void button5_Click(object sender, EventArgs e)
        {
            Close();
        }

        //perform math. operation
        private void button4_Click(object sender, EventArgs e)
        {
            if (RasterF != null && RasterF != String.Empty)
            {
                // call backgroundworker
                GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData
                {
                    RasterA = RasterA,
                    RasterB = RasterB,
                    RasterC = RasterC,
                    RasterD = RasterD,
                    RasterE = RasterG,
                    RasterF = RasterF,
                    DecSep = decsep,
                    MathRasterEquation = textBox1.Text,
                    UserText = "The calculation may take some time.",
                    Caption = "Performing raster operations",
                    BackgroundWorkerFunction = GralBackgroundworkers.BWMode.MathRasterOperations,
                    Unit = textBox2.Text.Trim()
                };

                GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                {
                    Text = DataCollection.Caption
                };
                BackgroundStart.Show();
                // now the backgroundworker works
                
                Close();
            }
            else
            {
                MessageBox.Show(this, "No output raster has been defined", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        //set default equation to NOx conversion function according to Baechlin 2008
        private void button6_Click(object sender, EventArgs e)
        {
            using (MathrasterNOxConversion mathNox = new MathrasterNOxConversion())
            {
                mathNox.StartPosition = FormStartPosition.Manual;
                mathNox.Location = new Point(this.Left + 80, this.Top + 50);
                mathNox.Owner = this;
                mathNox.ShowDialog();

                if (mathNox.SelectedEquation != String.Empty)
                {
                    textBox1.Text = mathNox.SelectedEquation;
                }
            }
        }

        //only defined characters are allowed as input in textbox1
        private void Comma(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',')
            {
                e.KeyChar = '.';
            }

            int asc = (int)e.KeyChar; //get ASCII code
        }
    }
}