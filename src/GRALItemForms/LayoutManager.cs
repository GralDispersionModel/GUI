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

using Gral;
using Gral.GRALItemForms;
using GralItemData;
using GralStaticFunctions;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace GralItemForms
{
    /// <summary>
    /// The Layout form for the objects (layers)
    /// </summary>
    public partial class Layout : Form
    {
        readonly GralDomain.Domain domain = null;
        private readonly CultureInfo ic = CultureInfo.InvariantCulture;
        private bool init = false;               //flag that prevents overwriting data fields during the initialization procedure

        /// <summary>
        /// Settings of recent DrawingObject
        /// </summary>
        public GralDomain.DrawingObjects DrawObject;
        private readonly string listsep;
        private bool UpdateObjectManager = false;
        public event ForceObjectManagerUpdate UpdateListbox;
        private HCLColorMap HCLColor;

        public Layout(GralDomain.Domain f)
        {
            domain = f;
            InitializeComponent();
            listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(ListBox1_DrawItem);

#if __MonoCS__
                var allNumUpDowns  = Main.GetAllControls<NumericUpDown>(this);
                foreach (NumericUpDown nu in allNumUpDowns)
                {
                    nu.TextAlign = HorizontalAlignment.Left;
                }
#endif

            listBox1.ItemHeight = listBox1.Font.Height;
            listsep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        }

        /// <summary>
        /// Set visibility of possible options 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Layout_Load(object sender, EventArgs e)
        {
            CheckBox3.Visible = true;
            groupBox1.Visible = true;
            groupBox5.Visible = true;

            trackBar1.Value = Math.Max(0, Math.Min(100, (int)(100 - DrawObject.Transparancy / 2.55)));

            //number of decimal places
            numericUpDown4.Value = 15;
            numericUpDown4.Value = DrawObject.DecimalPlaces;

            numericUpDown5.Value = DrawObject.ContourAreaMin;

            //labels/names hide/show
            if (DrawObject.Label == 2)
            {
                CheckBox3.Checked = true;
            }
            else if (DrawObject.Label == 3)
            {
                CheckBox3.Checked = true;
                CheckBox3.CheckState = CheckState.Indeterminate;
            }
            else if (DrawObject.Label == 1)
            {
                CheckBox3.Checked = false;
            }
            else if (DrawObject.Label == 0)
            {
                CheckBox3.Visible = false;
                groupBox1.Visible = false;
                label4.Visible = false;
                numericUpDown1.Visible = false;
                label11.Visible = false;
                numericUpDown6.Visible = false;
            }

            checkBox7.Checked = DrawObject.ScaleLabelFont;

            if (DrawObject.Name == "POINT SOURCES")
            {
                groupBox2.Text = "Symbol properties";
                button11.Text = "Size";
                toolTip1.SetToolTip(button11, "Enter the size of point-symbols\r\n 0 = no zoom, 1=standard width\r\n 2 and above = width in m");
                checkBox7.Visible = true;
            }

            if (DrawObject.Name == "RECEPTORS")
            {
                groupBox2.Text = "Symbol properties";
                button11.Text = "Size";
                toolTip1.SetToolTip(button11, "Enter the size of receptor-symbols\r\n 0 = no zoom, 1=standard width\r\n 2 and above = width in m");
                checkBox7.Visible = true;
                CheckBox3.Visible = true;
            }

            if (DrawObject.Name == "TUNNEL PORTALS")
            {
                groupBox2.Text = "Symbol properties";
                button11.Text = "Size";
                toolTip1.SetToolTip(button11, "Enter the size of tunnel-symbols");
                checkBox7.Visible = true;
            }

            if (DrawObject.Name.StartsWith("LINE SOURCES") || DrawObject.Name.Equals("WALLS") ||
                DrawObject.Name.Equals("AREA SOURCES") || DrawObject.Name.Equals("VEGETATION"))
            {
                checkBox7.Visible = true;
            }

            if (DrawObject.Name == "WALLS" ||
                DrawObject.Name == "GRAL DOMAIN" ||
                DrawObject.Name == "GRAMM DOMAIN")
            {
                groupBox4.Visible = false;
                label9.Visible = false;
                numericUpDown4.Visible = false;
                checkBox5.Visible = false;
            }

            //show vector scale when layer is vector map
            if (DrawObject.Name.StartsWith("VM:"))
            {
                label8.Visible = true;
                numericUpDown3.Visible = true;
                numericUpDown3.Value = Convert.ToDecimal(DrawObject.VectorScale);
                checkBox4.Text = "Smart draw";
                toolTip1.SetToolTip(checkBox4, "Draw faster but less accurate");
            }

            //line properties
            groupBox2.Visible = true;
            if (DrawObject.LineColor == Color.Transparent)
            {
                groupBox2.Visible = false;
                groupBox5.Visible = false;
            }

            //color scale yes/now
            if (DrawObject.ColorScale == "-999,-999,-999")
            {
                checkBox2.Checked = false;
            }
            else
            {
                checkBox2.Checked = true;
            }

            //label frequency
            if (DrawObject.LabelInterval == 0)
            {
                label4.Visible = false;
                numericUpDown1.Visible = false;
                label11.Visible = false;
                numericUpDown6.Visible = false;
            }
            else
            {
                label4.Visible = true;
                numericUpDown1.Visible = true;
                numericUpDown1.Value = Math.Max(1, DrawObject.ContourLabelDist);

                if (DrawObject.Name.StartsWith("CM:")) // show label distance at CM only
                {
                    label11.Visible = true;
                    numericUpDown6.Visible = true;
                    numericUpDown6.Value = Math.Max(1, Math.Min(10, DrawObject.LabelInterval));

                    if (DrawObject.DrawSimpleContour)
                    {
                        radioButton2.Checked = true;
                        checkBox6.Enabled = true;
                        groupBox6.Enabled = true;
                        numericUpDown7.Value = (decimal)(Math.Round(DrawObject.ContourFilter, 1));
                    }
                    else
                    {
                        radioButton1.Checked = true;
                        checkBox6.Enabled = false;
                        groupBox6.Enabled = false;
                        label12.Text = "Number of filter points";
                        numericUpDown7.DecimalPlaces = 0;
                        numericUpDown7.Increment = 1;
                        numericUpDown7.Value = (decimal)(Math.Round(DrawObject.ContourFilter, 0));
                        label13.Text = "Filter sigma";
                        numericUpDown8.Maximum = 10;
                    }
                    radioButton1.Visible = true;
                    radioButton2.Visible = true;
                    checkBox6.Visible = true;

                    numericUpDown8.Visible = true;
                    numericUpDown7.Visible = true;
                    label12.Visible = true;
                    label13.Visible = true;
                    checkBox6.Checked = DrawObject.ContourDrawBorder;
                    numericUpDown8.Value = (decimal)(Math.Round(DrawObject.ContourTension, 1));
                    numericUpDown10.Value = (decimal)(DrawObject.RemoveSpikes);
                    groupBox6.Visible = true;
                    label3.Visible = true;
                }
            }

            //fill property enabled or not
            if (DrawObject.Fill == false)
            {
                checkBox1.Visible = false;
                groupBox3.Visible = false;
            }
            else
            {
                checkBox1.Visible = true;
                groupBox3.Visible = true;
            }

            // fill box on GRAL and GRAMM Domain = Show Raster
            if (DrawObject.Name == "GRAMM DOMAIN" || DrawObject.Name == "GRAL DOMAIN")
            {
                checkBox1.Text = "Show Raster";
                checkBox1.Visible = true;
            }

            //objects filled or not
            if (DrawObject.FillYesNo == false)
            {
                checkBox1.Checked = false;
            }
            else
            {
                checkBox1.Checked = true;
            }

            //fill text boxes with legendtitle and legend unit
            textBox1.Text = DrawObject.LegendTitle;
            textBox2.Text = DrawObject.LegendUnit;

            //enable or disable color scale 
            if (DrawObject.ColorScale != "-999,-999,-999")
            {
                //scale of legend
                string[] dummy = new string[3];
                dummy = DrawObject.ColorScale.Split(new char[] { ',' });
                numericUpDown2.Value = Convert.ToDecimal(dummy[2], ic);
                SetNumericupDownIncrement(numericUpDown2.Value, 1m);
                domain.ActualEditedDrawingObject = DrawObject;
                domain.MouseControl = GralDomain.MouseMode.ViewLegendPos;
                domain.Cursor = System.Windows.Forms.Cursors.Cross;
            }
            else
            {
                numericUpDown2.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                label6.Enabled = false;
                label7.Enabled = false;
            }

            //selected item
            if (DrawObject.Item == -1)
            {
                comboBox2.SelectedIndex = 0;
            }
            else if (DrawObject.Item == 100)
            {
                comboBox2.SelectedIndex = 1;
            }
            else if (DrawObject.Item == 200)
            {
                comboBox2.SelectedIndex = 2;
            }
            else if (DrawObject.Item == 300)
            {
                comboBox2.SelectedIndex = 3;
            }

            //selected source group
            try
            {
                if (DrawObject.SourceGroup == -1)
                {
                    comboBox1.SelectedIndex = 0;
                }
            }
            catch
            {
            }

            //values
            if (listBox1.Items.Count == 0)
            {
                for (int i = 0; i < DrawObject.ItemValues.Count; i++)
                {
                    if (DrawObject.ItemValues[i] != 0)
                    {
                        listBox1.Items.Add(DrawObject.ItemValues[i]);
                    }
                    else
                    {
                        listBox1.Items.Add(0);
                    }
                }
            }

            if (comboBox2.SelectedIndex > 0)
            {
                button3.Visible = true;
                button4.Visible = true;
                button5.Visible = true;
                button6.Visible = true;
                button9.Visible = true;
                button13.Visible = true;
                button14.Visible = true;
                button2.Visible = true;
                button12.Visible = true;
            }

            //enable changing values and colors for contour, post- and vector maps
            try
            {
                if (DrawObject.Name.Substring(0, 3) == "CM:")
                {
                    button3.Visible = true;
                    button4.Visible = true;
                    button5.Visible = true;
                    button6.Visible = true;
                    button9.Visible = true;
                    button13.Visible = true;
                    button14.Visible = true;
                    button2.Visible = true;
                    button12.Visible = true;
                    label10.Visible = true;
                    numericUpDown5.Visible = true;
                    checkBox7.Visible = true;
                }
                else
                {
                    CheckBox3.ThreeState = false;
                }
                if (DrawObject.Name.Substring(0, 3) == "VM:")
                {
                    button3.Visible = true;
                    button4.Visible = true;
                    button5.Visible = true;
                    button6.Visible = true;
                    button9.Visible = true;
                    button13.Visible = true;
                    button14.Visible = true;
                    button2.Visible = true;
                    button12.Visible = true;
                    CheckBox3.Visible = false;
                    checkBox4.Visible = false;
                    label4.Text = "Drawing iterator";
                }
                if (DrawObject.Name.Substring(0, 3) == "PM:")
                {
                    button3.Visible = true;
                    button4.Visible = true;
                    button5.Visible = true;
                    button6.Visible = true;
                    button9.Visible = true;
                    button13.Visible = true;
                    button14.Visible = true;
                    button2.Visible = true;
                    button12.Visible = true;
                }

                if (DrawObject.Name.StartsWith("CONCENTRATION VALUES") || DrawObject.Name.StartsWith("ITEM INFO"))
                {
                    groupBox4.Visible = false;
                    label9.Visible = true;
                    numericUpDown1.Visible = true;
                    numericUpDown4.Visible = true;
                    textBox1.Visible = true;
                    textBox2.Visible = true;
                    label4.Visible = true;
                    label6.Visible = true;
                    checkBox2.Enabled = false;
                    checkBox7.Visible = true;
                    numericUpDown2.Enabled = false;
                    label7.Visible = false;

                    listBox1.Visible = false;
                    groupBox5.Visible = false;

                    CheckBox3.Visible = false; // show labels
                    checkBox4.Visible = false; //LP filter
                    checkBox5.Visible = false; //default interval values
                    checkBox1.Visible = true;  // fill
                    checkBox1.Checked = DrawObject.FillYesNo;
                    if (DrawObject.Name.StartsWith("CONCENTRATION VALUES"))
                    {
                        // decimal places
                        groupBox3.Visible = true;
                        toolTip1.SetToolTip(checkBox1, "Fill the background of a concentration label");
                    }
                    else
                    {
                        groupBox3.Visible = false;
                    }
                }
            }
            catch
            {
            }

            //enable low pass filtering of raster data (contour maps)
            if (!string.IsNullOrEmpty(DrawObject.ContourFilename) && DrawObject.ContourFilename != "x")
            {
                if (!DrawObject.Name.StartsWith("CONCENTRATION VALUES") && !DrawObject.Name.StartsWith("ITEM INFO"))
                {
                    checkBox4.Visible = true;
                }
            }
            else
            {
                checkBox4.Visible = false;
            }

            //low pass filtering on/off
            if (DrawObject.Filter == true)
            {
                checkBox4.Checked = true;
            }
            else
            {
                checkBox4.Checked = false;
            }

            if (DrawObject.Name == "LINE SOURCES" || DrawObject.Name == "BUILDINGS")
            {
                checkBox4.Text = "Smart draw";
                toolTip1.SetToolTip(checkBox4, "Draw faster but less accurate");
                checkBox4.Visible = true;
            }
            if (DrawObject.Name.StartsWith("SM:"))
            {
                checkBox4.Visible = false;
                groupBox2.Visible = true;
                checkBox1.Visible = true;
                groupBox3.Visible = true;
                groupBox4.Visible = false;
                label8.Visible = false;
                numericUpDown3.Visible = false;
                numericUpDown1.Visible = false;
                label4.Visible = false;
            }

            // Bitmap
            if (DrawObject.Name.Substring(0, 3) == "BM:")
            {
                groupBox5.Visible = true;
                groupBox5.Text = "Opacity";
                if (DrawObject.Transparancy <= 0 | DrawObject.Transparancy >= 100)
                {
                    DrawObject.Transparancy = 100;
                }

                trackBar1.Value = DrawObject.Transparancy;

                label4.Text = "Raster size";
                if (DrawObject.LabelInterval == 0)
                {
                    DrawObject.LabelInterval = 500;
                    CheckBox3.Checked = false;
                    DrawObject.LineColors[0] = Color.Gray;
                    DrawObject.LabelFont = new Font("ARIAL", 8);
                }

                CheckBox3.Text = "Show raster";
                toolTip1.SetToolTip(CheckBox3, "Draw a grid raster");
                CheckBox3.Visible = true;
                if (DrawObject.ContourLabelDist >= numericUpDown1.Minimum && DrawObject.ContourLabelDist <= numericUpDown1.Maximum)
                {
                    numericUpDown1.Value = DrawObject.ContourLabelDist;
                }
                label4.Visible = true;
                numericUpDown1.Visible = true;
                toolTip1.SetToolTip(CheckBox3, "Show grid with fixed size");

                checkBox4.Visible = false;
                groupBox2.Visible = true;
                button11.Visible = false;
                groupBox1.Visible = true;
                button7.Visible = false; // font-color = line color at coordinate grid
                //domain.picture[_object_index] = SetImageOpacity(domain.picture[_object_index], 0.4F);
            }

            if (DrawObject.Name == "WINDROSE")
            {
                checkBox7.Visible = true;
                label4.Text = "Size [m]";
                //				label11.Text = "Max. wind vel [m/s]";
                //				label11.Visible = true;
                //				numericUpDown6.Visible = true;
                //				numericUpDown6.Value = Math.Max(6, Math.Min(12, DrawObject.LabelInterval));
                checkBox4.Visible = false;
                groupBox5.Visible = true;
                label10.Top = numericUpDown1.Bottom + 20;
                numericUpDown5.Top = label10.Bottom + 5;
                label10.Visible = true;
                numericUpDown5.Visible = true;
                label10.Text = "Max. scale [%]\n 0 = auto";
                numericUpDown5.Value = (decimal)(DrawObject.ContourAreaMin);
                groupBox2.Visible = false;
            }

            if (DrawObject.Name == "SCALE BAR")
            {
                checkBox7.Visible = true;
                if (DrawObject.LabelFont.Height < DrawObject.ContourLabelDist / 20)
                {
                    DrawObject.LabelFont = new Font(DrawObject.LabelFont.FontFamily, Math.Min(16000, (int)(DrawObject.ContourLabelDist / 20)));
                }
            }

            if (DrawObject.Name.Equals("NORTH ARROW"))
            {
                label4.Text = "Scale [%]";
                numericUpDown1.Value = DrawObject.ContourLabelDist;
                DrawObject.LabelInterval = 1;
            }

            if (!string.IsNullOrEmpty(DrawObject.ContourFilename) && DrawObject.ContourFilename != "x" && DrawObject.Name.StartsWith("CM:") && DrawObject.Name.Length > 4)
            {
                groupBox7.Visible = true;
                textBox3.Text = DrawObject.Name.Substring(3);
                textBox4.Text = DrawObject.ContourFilename;
            }
            else
            {
                // reduce height of the form
                button1.Top = groupBox7.Top;
                Size rect = ClientSize;
                rect.Height = button1.Bottom + 20;
                ClientSize = rect;
            }

            HCLColor = new HCLColorMap();
            for (int i = 0; i < HCLColor.HCLName.Length; i++)
            {
                comboBox3.Items.Add(HCLColor.HCLName[i]);
            }
            comboBox3.SelectedIndex = 0;

            listBox1.Refresh();
            init = true;

            checkBox8.Checked = DrawObject.BasedOnMap;
        }

        /// <summary>
        /// Save all settings and close the layout manager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            this.Hide();

            if (DrawObject.LabelInterval != 0)
            {
                DrawObject.ContourLabelDist = Convert.ToInt32(numericUpDown1.Value);
            }

            DrawObject.LabelInterval = Convert.ToInt32(numericUpDown6.Value);

            DrawObject.ContourDrawBorder = checkBox6.Checked;

            if (Math.Abs(DrawObject.ContourFilter - Convert.ToSingle(numericUpDown7.Value)) > 0.01) // new value
            {
                DrawObject.ContourFilter = Convert.ToSingle(numericUpDown7.Value);
                domain.ReDrawContours = true;
            }
            if (Math.Abs(DrawObject.ContourTension - Convert.ToSingle(numericUpDown8.Value)) > 0.01) // new value
            {
                DrawObject.ContourTension = Convert.ToSingle(numericUpDown8.Value);
                domain.ReDrawContours = true;
            }
            if (Math.Abs(DrawObject.RemoveSpikes - (int)(numericUpDown10.Value)) > 0.01) // new value
            {
                DrawObject.RemoveSpikes = Convert.ToInt32(numericUpDown10.Value);
                domain.ReDrawContours = true;
            }


            DrawObject.Transparancy = Convert.ToInt32((100 - trackBar1.Value) * 2.55);
            if (checkBox1.Checked == true)
            {
                DrawObject.Fill = true;
            }

#if __MonoCS__
            // Update of form for LINUX version, otherwise the backgroundworker sometimes cant access this form -> app crashes 
            for (int i = 0; i < 20; i++) {
                System.Threading.Thread.Sleep(5);
                Application.DoEvents();
            }
#endif

            DrawObject.ScaleLabelFont = checkBox7.Checked;

            //changes in contour map
            try
            {
                if (DrawObject.Name.Substring(0, 3) == "CM:")
                {
                    Cursor = System.Windows.Forms.Cursors.WaitCursor;
                    domain.Contours(DrawObject.ContourFilename, DrawObject);
                    Cursor = System.Windows.Forms.Cursors.Default;
                }
            }
            catch
            {
            }

            //changes in post map
            try
            {
                if (DrawObject.Name.Substring(0, 3) == "PM:")
                {
                    Cursor = System.Windows.Forms.Cursors.WaitCursor;
                    domain.Contours(DrawObject.ContourFilename, DrawObject);
                    Cursor = System.Windows.Forms.Cursors.Default;
                }
            }
            catch
            {
            }

            //changes in vector map
            try
            {
                if (DrawObject.Name.Substring(0, 3) == "VM:")
                {
                    Cursor = System.Windows.Forms.Cursors.WaitCursor;
                    domain.LoadVectors(DrawObject.ContourFilename, DrawObject);
                    Cursor = System.Windows.Forms.Cursors.Default;
                }
            }
            catch
            {
            }

            // Changes at the opacity of a bitmap
            try
            {
                if (DrawObject.Name.Substring(0, 3) == "BM:")
                {
                    if (trackBar1.Value < 1)
                    {
                        trackBar1.Value = 1; // min opacity
                    }

                    if (DrawObject.Transparancy != trackBar1.Value) // changed opacity
                    {
                        // reload picture
                        DrawObject.Picture = null;
                        StreamReader SR = new StreamReader(DrawObject.ContourFilename);
                        DrawObject.Picture = new Bitmap(SR.BaseStream);
                        SR.Close();

                        float opacity = ((float)trackBar1.Value) / 100; // 0 - 100 %
                                                                        //                		domain.picture[_object_index] = SetImageOpacity(domain.picture[_object_index], opacity);
                        if (opacity < 1)
                        {
                            DrawObject.Picture.MakeTransparent(Color.White);
                        }
                        DrawObject.Transparancy = trackBar1.Value;
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(this, err.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (DrawObject.Name.Equals("SCALE BAR"))
            {
                //MessageBox.Show(DrawObject.LabelFont.Height.ToString() + "/" + DrawObject.ContourLabelDist.ToString());
                if (DrawObject.ScaleLabelFont && (DrawObject.LabelFont.Height < (DrawObject.ContourLabelDist / 10)))
                {
                    DrawObject.LabelFont = new Font(DrawObject.LabelFont.FontFamily, Math.Min(16000, (int)(DrawObject.ContourLabelDist / 5)));
                }
            }

            this.Close();
        }

        /// <summary>
        /// show/hide labels/names of the selected object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CheckBox3CheckStateChanged(object sender, EventArgs e)
        {
            if (init == true)
            {
                if (CheckBox3.Checked == true)
                {
                    if (CheckBox3.CheckState == CheckState.Indeterminate)
                    {
                        DrawObject.Label = 3;
                    }
                    else
                    {
                        DrawObject.Label = 2;
                    }
                }
                else
                {
                    DrawObject.Label = 1;
                }
            }
        }

        /// <summary>
        /// show/hide fill properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (init == true)
            {
                if (checkBox1.Checked == true)
                {
                    DrawObject.FillYesNo = true;
                }
                else
                {
                    DrawObject.FillYesNo = false;
                }
            }
        }

        /// <summary>
        /// set line color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button10_Click(object sender, EventArgs e)
        {
            ColorDialog ct = new ColorDialog();
            if (DrawObject.LineColors.Count > 0 && DrawObject.LineColors[0] != null)
            {
                ct.Color = DrawObject.LineColors[0];
                if (ct.ShowDialog() == DialogResult.OK)
                {
                    if (DrawObject.Name.StartsWith("CM:"))
                    {
                        DrawObject.LineColor = ct.Color;
                    }
                    else
                    {
                        DrawObject.LineColors[0] = ct.Color;
                    }
                }
            }
            ct.Dispose();
        }

        /// <summary>
        /// set label color and font
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button7_Click(object sender, EventArgs e)
        {
            ColorDialog ct = new ColorDialog();
            if (DrawObject.LabelColor != null)
            {
                ct.Color = DrawObject.LabelColor;
            }

            if (ct.ShowDialog() == DialogResult.OK)
            {
                DrawObject.LabelColor = ct.Color;
            }
            ct.Dispose();
        }

        /// <summary>
        /// set font type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button8_Click(object sender, EventArgs e)
        {
            FontDialog ft = new FontDialog();
            if (DrawObject.LabelFont != null)
            {
                ft.Font = DrawObject.LabelFont;
            }

            if (ft.ShowDialog() == DialogResult.OK)
            {
                DrawObject.LabelFont = ft.Font;
            }
            ft.Dispose();
        }

        /// <summary>
        /// change the color/font/fill properties in the listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            try
            {
                StringFormat drawFormat = new StringFormat
                {
                    Alignment = StringAlignment.Far,
                };

                int width = (int)(e.Bounds.Width / 5D * 2);
                RectangleF textColumn = new RectangleF(e.Bounds.Left, e.Bounds.Top, width, e.Bounds.Height);
                e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), e.Font, new SolidBrush(SystemColors.ControlText), textColumn, drawFormat);

                int graphwidth = (e.Bounds.Width - width) / 2;
                int x0 = width + graphwidth + 4;
                int y0 = e.Bounds.Top + 1;
                int height = Math.Max(1, e.Bounds.Height - 2);
                e.Graphics.FillRectangle(new SolidBrush(DrawObject.FillColors[e.Index]), x0, y0, Math.Max(1, graphwidth - 8), height);

                x0 = width + 4;
                y0 = e.Bounds.Top + e.Bounds.Height / 2;
                e.Graphics.DrawLine(new Pen(DrawObject.LineColors[e.Index], 3), x0, y0, x0 + Math.Max(1, graphwidth - 8), y0);

                Pen myPen = new Pen(SystemColors.ControlText, 1);
                myPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                e.Graphics.DrawLine(myPen, e.Bounds.Left + 4, e.Bounds.Bottom - 1, e.Bounds.Right - 8, e.Bounds.Bottom - 1);
            }
            catch
            {
            }
        }

        /// <summary>
        /// change the label values and line and fill colors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            using (LayoutManagerChangeValueAndColor chVal = new LayoutManagerChangeValueAndColor())
            {
                chVal.Value = DrawObject.ItemValues[index];
                chVal.LineColor = DrawObject.LineColors[index];
                chVal.FillColor = DrawObject.FillColors[index];
                chVal.DecimalPlaces = (int)numericUpDown4.Value;
                chVal.TopMost = true;

                if (chVal.ShowDialog() == DialogResult.OK)
                {
                    DrawObject.ItemValues[index] = chVal.Value;
                    DrawObject.LineColors[index] = chVal.LineColor;
                    DrawObject.FillColors[index] = chVal.FillColor;

                    try
                    {
                        listBox1.Items.Insert(index, DrawObject.ItemValues[index]);
                        listBox1.Items.RemoveAt(index + 1);

                        SortListBox();

                        //enable re-calculation of contours
                        if (DrawObject.Name.Substring(0, 3) == "CM:")
                        {
                            domain.ReDrawContours = true;
                        }
                        //enable re-calculation of post maps
                        if (DrawObject.Name.Substring(0, 3) == "PM:")
                        {
                            domain.ReDrawContours = true;
                        }
                        //enable re-calculation of vectors
                        if (DrawObject.Name.Substring(0, 3) == "VM:")
                        {
                            domain.ReDrawVectors = true;
                        }
                    }
                    catch
                    {
                        MessageBox.Show(this, "Invalid number", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Sort the listbox for values for contour lines 
        /// </summary>
        private void SortListBox()
        {
            double x = 0;
            double y = 0;
            //sort list box
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                for (int j = i + 1; j < listBox1.Items.Count; j++)
                {
                    x = (double)listBox1.Items[i];
                    y = (double)listBox1.Items[j];
                    if (x > y)
                    {
                        listBox1.Items[i] = listBox1.Items[j];
                        listBox1.Items[j] = x;

                        Color temp = DrawObject.LineColors[i];
                        DrawObject.LineColors[i] = DrawObject.LineColors[j];
                        DrawObject.LineColors[j] = temp;

                        temp = DrawObject.FillColors[i];
                        DrawObject.FillColors[i] = DrawObject.FillColors[j];
                        DrawObject.FillColors[j] = temp;
                    }
                }
            }

            DrawObject.ItemValues.Clear();
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                DrawObject.ItemValues.Add((double)listBox1.Items[i]);
            }

            if (DrawObject.ItemValues.Count == 0)
            {
                DrawObject.ItemValues.Add(0);
            }
        }

        /// <summary>
        /// compute value range for the selected item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor = System.Windows.Forms.Cursors.WaitCursor;
            if (init == true)
            {
                DrawObject.SourceGroup = -1; // reset
                DrawObject.Item = -1;        // reset

                if (comboBox2.SelectedIndex > 0)
                {
                    button3.Visible = true;
                    button4.Visible = true;
                    button5.Visible = true;
                    button6.Visible = true;
                    button9.Visible = true;
                    button13.Visible = true;
                    button14.Visible = true;
                    button2.Visible = true;
                    button12.Visible = true;
                }
                else
                {
                    button3.Visible = false;
                    button4.Visible = false;
                    button5.Visible = false;
                    button6.Visible = false;
                    button9.Visible = false;
                    button13.Visible = false;
                    button14.Visible = false;
                    button2.Visible = false;
                    button12.Visible = false;
                    ResetColors(Color.Red);
                }

                if (DrawObject.Name == "BUILDINGS")
                {
                    if (comboBox2.SelectedIndex == 0)
                    {
                        ResetColors(Color.Blue);
                    }
                    else
                    {
                        DrawObject.Item = comboBox2.SelectedIndex;

                        double height;
                        double max = -1;
                        double min = 100000;
                        foreach (BuildingData _bd in domain.EditB.ItemData)
                        {
                            height = _bd.Height;
                            max = Math.Max(max, height);
                            min = Math.Min(min, height);
                        }
                        if ((max - min) < 5)
                        {
                            max = (int)min + 5;
                        }

                        DrawObject.FillColors.Clear();
                        DrawObject.LineColors.Clear();
                        listBox1.Items.Clear();

                        if (checkBox5.Checked)
                        {
                            listBox1.Items.Insert(0, Normalize(min, double.MinValue));
                        }
                        else
                        {
                            listBox1.Items.Insert(0, min);
                        }

                        DrawObject.LineColors.Insert(0, Color.LightGreen);
                        DrawObject.FillColors.Insert(0, Color.LightGreen);

                        if (checkBox5.Checked)
                        {
                            listBox1.Items.Add(Normalize(max, double.MinValue));
                        }
                        else
                        {
                            listBox1.Items.Add(max);
                        }

                        DrawObject.LineColors.Add(Color.Red);
                        DrawObject.FillColors.Add(Color.Red);
                        double valPrev = max;
                        for (int i = 0; i < 3; i++)
                        {
                            double val = min + (max - min) / Math.Pow(2, Convert.ToDouble(i + 1));
                            if (checkBox5.Checked)
                            {
                                val = Normalize(val, valPrev);
                            }
                            valPrev = val;
                            listBox1.Items.Insert(1, val);
                        }
                        DrawObject.LineColors.Insert(1, Color.Pink);
                        DrawObject.LineColors.Insert(1, Color.Yellow);
                        DrawObject.LineColors.Insert(1, Color.YellowGreen);
                        DrawObject.FillColors.Insert(1, Color.Pink);
                        DrawObject.FillColors.Insert(1, Color.Yellow);
                        DrawObject.FillColors.Insert(1, Color.YellowGreen);
                    }
                }

                if (DrawObject.Name == "VEGETATION")
                {
                    if (comboBox2.SelectedIndex == 0)
                    {
                        ResetColors(Color.Green);
                    }
                    else
                    {
                        DrawObject.Item = comboBox2.SelectedIndex;

                        DrawObject.FillColors.Clear();
                        DrawObject.LineColors.Clear();
                        listBox1.Items.Clear();

                        listBox1.Items.Insert(0, 0);

                        DrawObject.LineColors.Add(Color.LightGreen);
                        DrawObject.FillColors.Add(Color.LightGreen);

                        listBox1.Items.Add(30);

                        DrawObject.LineColors.Add(Color.YellowGreen);
                        DrawObject.FillColors.Add(Color.YellowGreen);

                        for (int i = 0; i < 3; i++)
                        {
                            int val = 5 * (4 - i);

                            listBox1.Items.Insert(2, val);
                        }

                        DrawObject.LineColors.Add(Color.ForestGreen);
                        DrawObject.LineColors.Add(Color.Green);
                        DrawObject.LineColors.Add(Color.DarkGreen);
                        DrawObject.FillColors.Add(Color.ForestGreen);
                        DrawObject.FillColors.Add(Color.Green);
                        DrawObject.FillColors.Add(Color.DarkGreen);
                    }
                }

                if ((DrawObject.Name == "RECEPTORS") && (comboBox2.SelectedIndex > 0))
                {
                    DrawObject.Item = comboBox2.SelectedIndex;

                    double display_value;
                    double max = -1000000;
                    double min = 1000000;
                    foreach (ReceptorData _rd in domain.EditR.ItemData)
                    {
                        display_value = Math.Abs(_rd.DisplayValue);
                        max = Math.Max(max, display_value);
                        min = Math.Min(min, display_value);
                    }
                    DrawObject.FillColors.Clear();
                    DrawObject.LineColors.Clear();
                    listBox1.Items.Clear();

                    if (checkBox5.Checked)
                    {
                        listBox1.Items.Insert(0, Normalize(min, double.MinValue));
                    }
                    else
                    {
                        listBox1.Items.Insert(0, min);
                    }

                    DrawObject.LineColors.Insert(0, Color.LightGreen);
                    DrawObject.FillColors.Insert(0, Color.LightGreen);
                    if (checkBox5.Checked)
                    {
                        listBox1.Items.Add(Normalize(max, double.MinValue));
                    }
                    else
                    {
                        listBox1.Items.Add(max);
                    }

                    DrawObject.LineColors.Add(Color.Red);
                    DrawObject.FillColors.Add(Color.Red);
                    double valPrev = max;
                    for (int i = 0; i < 3; i++)
                    {
                        double val = min + (max - min) / Math.Pow(2, Convert.ToDouble(i + 1));
                        if (checkBox5.Checked)
                        {
                            val = Normalize(val, valPrev);
                        }
                        valPrev = val;
                        listBox1.Items.Insert(1, val);
                    }
                    DrawObject.LineColors.Insert(1, Color.Pink);
                    DrawObject.LineColors.Insert(1, Color.Yellow);
                    DrawObject.LineColors.Insert(1, Color.YellowGreen);
                    DrawObject.FillColors.Insert(1, Color.Pink);
                    DrawObject.FillColors.Insert(1, Color.Yellow);
                    DrawObject.FillColors.Insert(1, Color.YellowGreen);
                }

                if (DrawObject.Name == "AREA SOURCES")
                {
                    double poll;
                    double max = -10000000;
                    double min = 10000000;

                    foreach (AreaSourceData _as in domain.EditAS.ItemData)
                    {
                        bool exist = false;
                        //filter source groups
                        string selpoll = Convert.ToString(comboBox1.SelectedItem, ic);
                        if (comboBox1.SelectedIndex == 0)
                        {
                            exist = true;
                        }

                        string[] dummy = new string[2];
                        dummy = selpoll.Split(new char[] { ':' });
                        if (dummy.Length > 1)
                        {
                            dummy[1] = dummy[1].Trim();
                            int _sg = 0;
                            int.TryParse(dummy[1], out _sg);
                            if (_sg == _as.Poll.SourceGroup)
                            {
                                exist = true;
                                DrawObject.SourceGroup = Convert.ToInt32(dummy[1], ic);
                            }
                        }
                        else
                        {
                            dummy[0] = dummy[0].Trim();
                            int _sg = 0;
                            int.TryParse(dummy[0], out _sg);
                            if (_sg == _as.Poll.SourceGroup)
                            {
                                exist = true;
                                DrawObject.SourceGroup = Convert.ToInt32(dummy[0], ic);
                            }
                        }
                        if ((exist == true) && (comboBox2.SelectedIndex > 0))
                        {
                            //filter pollutant 
                            for (int j = 0; j < 10; j++)
                            {
                                if ((Main.PollutantList[_as.Poll.Pollutant[j]] == Convert.ToString(comboBox2.SelectedItem)) &&
                                    (_as.Poll.EmissionRate[j] != 0))
                                {
                                    poll = _as.Poll.EmissionRate[j];
                                    max = Math.Max(max, poll);
                                    min = Math.Min(min, poll);
                                    DrawObject.Item = _as.Poll.Pollutant[j];
                                }
                            }
                            if (max == -10000000)
                            {
                                button3.Visible = false;
                                button4.Visible = false;
                                button5.Visible = false;
                                button6.Visible = false;
                                button9.Visible = false;
                                button13.Visible = false;
                                button14.Visible = false;
                                button2.Visible = false;
                                button12.Visible = false;
                                DrawObject.FillColors.Clear();
                                DrawObject.LineColors.Clear();
                                listBox1.Items.Clear();
                                listBox1.Items.Add("0");

                                DrawObject.LineColors.Add(Color.Red);
                                DrawObject.FillColors.Add(Color.Red);
                            }
                            else
                            {
                                DrawObject.LineColors.Clear();
                                DrawObject.FillColors.Clear();
                                listBox1.Items.Clear();

                                if (checkBox5.Checked)
                                {
                                    listBox1.Items.Insert(0, Normalize(min, double.MinValue));
                                }
                                else
                                {
                                    listBox1.Items.Insert(0, min);
                                }

                                DrawObject.LineColors.Insert(0, Color.LightGreen);
                                DrawObject.FillColors.Insert(0, Color.LightGreen);
                                if (checkBox5.Checked)
                                {
                                    listBox1.Items.Add(Normalize(max, double.MinValue));
                                }
                                else
                                {
                                    listBox1.Items.Add(max);
                                }

                                DrawObject.LineColors.Add(Color.Red);
                                DrawObject.FillColors.Add(Color.Red);
                                double valPrev = max;
                                for (int i = 0; i < 3; i++)
                                {
                                    double val = min + (max - min) / Math.Pow(2, Convert.ToDouble(i + 1));
                                    if (checkBox5.Checked)
                                    {
                                        val = Normalize(val, valPrev);
                                    }
                                    valPrev = val;
                                    listBox1.Items.Insert(1, val);
                                }
                                DrawObject.LineColors.Insert(1, Color.Pink);
                                DrawObject.LineColors.Insert(1, Color.Yellow);
                                DrawObject.LineColors.Insert(1, Color.YellowGreen);
                                DrawObject.FillColors.Insert(1, Color.Pink);
                                DrawObject.FillColors.Insert(1, Color.Yellow);
                                DrawObject.FillColors.Insert(1, Color.YellowGreen);
                            }
                        }
                    }
                }

                if (DrawObject.Name == "LINE SOURCES")
                {
                    double poll;
                    double max = -10000000;
                    double min = 10000000;
                    foreach (LineSourceData _ls in domain.EditLS.ItemData)
                    {
                        bool exist = false;
                        int index = 0;
                        //filter source groups
                        string selpoll = Convert.ToString(comboBox1.SelectedItem, ic);
                        if (comboBox1.SelectedIndex == 0)
                        {
                            exist = true;
                        }
                        else
                        {
                            string[] dummy = new string[2];
                            dummy = selpoll.Split(new char[] { ':' });
                            int loop = 0;
                            foreach (PollutantsData _poll in _ls.Poll)
                            {
                                exist = false;
                                if (dummy.Length > 1)
                                {
                                    dummy[1] = dummy[1].Trim();
                                    int _sg = 0;
                                    int.TryParse(dummy[1], out _sg);
                                    if (_sg == _poll.SourceGroup)
                                    {
                                        index = loop;
                                        exist = true;
                                        DrawObject.SourceGroup = Convert.ToInt32(dummy[1], ic);
                                    }
                                }
                                else
                                {
                                    dummy[0] = dummy[0].Trim();
                                    int _sg = 0;
                                    int.TryParse(dummy[0], out _sg);
                                    if (_sg == _poll.SourceGroup)
                                    {
                                        index = loop;
                                        exist = true;
                                        DrawObject.SourceGroup = Convert.ToInt32(dummy[0], ic);
                                    }
                                }
                                //neu
                                if ((exist == true) && (comboBox2.SelectedIndex > 0))
                                {
                                    //filter pollutant
                                    if (comboBox1.SelectedIndex == 0)
                                    {
                                        poll = 0;
                                        for (int j = 0; j < 10; j++)
                                        {
                                            if ((Main.PollutantList[_poll.Pollutant[j]] == Convert.ToString(comboBox2.SelectedItem, ic)) &&
                                                (_poll.EmissionRate[j] != 0))
                                            {
                                                poll += _poll.EmissionRate[j];
                                                max = Math.Max(max, poll);
                                                min = Math.Min(min, poll);
                                                DrawObject.Item = _poll.Pollutant[j];
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int j = 0; j < 10; j++)
                                        {
                                            if ((Main.PollutantList[_ls.Poll[index].Pollutant[j]] == Convert.ToString(comboBox2.SelectedItem, ic)) &&
                                                (_ls.Poll[index].EmissionRate[j] != 0))
                                            {
                                                poll = _ls.Poll[index].EmissionRate[j];
                                                max = Math.Max(max, poll);
                                                min = Math.Min(min, poll);
                                                DrawObject.Item = _ls.Poll[index].Pollutant[j];
                                            }
                                        }
                                    }
                                }
                                loop++;
                            }
                        }
                        if ((exist == true) && (comboBox2.SelectedIndex > 0))
                        {
                            //filter pollutant                            
                            if (comboBox1.SelectedIndex == 0)
                            {
                                poll = 0;
                                foreach (PollutantsData _poll in _ls.Poll)
                                {
                                    for (int j = 0; j < 10; j++)
                                    {
                                        if ((Main.PollutantList[_poll.Pollutant[j]] == Convert.ToString(comboBox2.SelectedItem, ic)) &&
                                            (_poll.EmissionRate[j] != 0))
                                        {
                                            poll += _poll.EmissionRate[j];
                                            max = Math.Max(max, poll);
                                            min = Math.Min(min, poll);
                                            DrawObject.Item = _poll.Pollutant[j];
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 0; j < 10; j++)
                                {
                                    if ((Main.PollutantList[_ls.Poll[index].Pollutant[j]] == Convert.ToString(comboBox2.SelectedItem, ic)) &&
                                        (_ls.Poll[index].EmissionRate[j] != 0))
                                    {
                                        poll = _ls.Poll[index].EmissionRate[j];
                                        max = Math.Max(max, poll);
                                        min = Math.Min(min, poll);
                                        DrawObject.Item = _ls.Poll[index].Pollutant[j];
                                    }
                                }
                            }


                            //filter AADT
                            if (comboBox2.SelectedIndex == 1)
                            {
                                poll = _ls.Nemo.AvDailyTraffic;
                                max = Math.Max(max, poll);
                                min = Math.Min(min, poll);
                                DrawObject.Item = 100;
                            }
                            //filter slope
                            if (comboBox2.SelectedIndex == 2)
                            {
                                poll = _ls.Nemo.Slope;
                                max = Math.Max(max, poll);
                                min = Math.Min(min, poll);
                                DrawObject.Item = 200;
                            }
                            //filter HDV
                            if (comboBox2.SelectedIndex == 3)
                            {
                                poll = _ls.Nemo.ShareHDV;
                                max = Math.Max(max, poll);
                                min = Math.Min(min, poll);
                                DrawObject.Item = 300;
                            }

                            if (max == -10000000)
                            {
                                button3.Visible = false;
                                button4.Visible = false;
                                button5.Visible = false;
                                button6.Visible = false;
                                button9.Visible = false;
                                button13.Visible = false;
                                button14.Visible = false;
                                button2.Visible = false;
                                button12.Visible = false;
                                DrawObject.FillColors.Clear();
                                DrawObject.LineColors.Clear();
                                listBox1.Items.Clear();
                                listBox1.Items.Add("0");
                                DrawObject.LineColors.Add(Color.Red);
                                DrawObject.FillColors.Add(Color.Red);
                            }
                            else
                            {
                                DrawObject.LineColors.Clear();
                                DrawObject.FillColors.Clear();
                                listBox1.Items.Clear();
                                if (checkBox5.Checked)
                                {
                                    listBox1.Items.Insert(0, Normalize(min, double.MinValue));
                                }
                                else
                                {
                                    listBox1.Items.Insert(0, min);
                                }

                                DrawObject.LineColors.Insert(0, Color.LightGreen);
                                DrawObject.FillColors.Insert(0, Color.LightGreen);
                                if (checkBox5.Checked)
                                {
                                    listBox1.Items.Add(Normalize(max, double.MinValue));
                                }
                                else
                                {
                                    listBox1.Items.Add(max);
                                }

                                DrawObject.LineColors.Add(Color.Red);
                                DrawObject.FillColors.Add(Color.Red);
                                double valPrev = max;
                                for (int i = 0; i < 3; i++)
                                {
                                    double val = min + (max - min) / Math.Pow(2, Convert.ToDouble(i + 1));
                                    if (checkBox5.Checked)
                                    {
                                        val = Normalize(val, valPrev);
                                    }
                                    valPrev = val;
                                    listBox1.Items.Insert(1, val);
                                }
                                DrawObject.LineColors.Insert(1, Color.Pink);
                                DrawObject.LineColors.Insert(1, Color.Yellow);
                                DrawObject.LineColors.Insert(1, Color.YellowGreen);
                                DrawObject.FillColors.Insert(1, Color.Pink);
                                DrawObject.FillColors.Insert(1, Color.Yellow);
                                DrawObject.FillColors.Insert(1, Color.YellowGreen);
                            }
                        }
                    }
                }
                DrawObject.ItemValues.Clear();
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    DrawObject.ItemValues.Add(Convert.ToDouble(listBox1.Items[i]));
                }

                if (DrawObject.ItemValues.Count == 0)
                {
                    DrawObject.ItemValues.Add(0);
                }
            }

            Cursor = System.Windows.Forms.Cursors.Default;
        }

        /// <summary>
        /// Reset all colors of an object
        /// </summary>
        /// <param name="_col"></param>
        private void ResetColors(Color _col)
        {
            DrawObject.FillColors.Clear();
            DrawObject.LineColors.Clear();
            listBox1.Items.Clear();
            listBox1.Items.Add(0);
            DrawObject.LineColors.Add(_col);
            DrawObject.FillColors.Add(_col);
        }

        /// <summary>
        /// select source group
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox2_SelectedIndexChanged(sender, e);
        }

        /// <summary>
        /// Add a value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddValue(object sender, EventArgs e)
        {
            using (LayoutManagerChangeValueAndColor chVal = new LayoutManagerChangeValueAndColor())
            {
                chVal.Value = 0;
                chVal.LineColor = Color.Black;
                chVal.FillColor = Color.Black;
                chVal.DecimalPlaces = (int)numericUpDown4.Value;
                chVal.TopMost = true;

                if (chVal.ShowDialog() == DialogResult.OK)
                {
                    DrawObject.ItemValues.Add(chVal.Value);

                    listBox1.Items.Insert(0, chVal.Value);

                    DrawObject.FillColors.Insert(0, chVal.FillColor);
                    DrawObject.LineColors.Insert(0, chVal.LineColor);

                    SortListBox();

                    //enable re-calculation of contours
                    if (DrawObject.Name.Substring(0, 3) == "CM:")
                    {
                        domain.ReDrawContours = true;
                    }
                    //enable re-calculation of vectors
                    if (DrawObject.Name.Substring(0, 3) == "VM:")
                    {
                        domain.ReDrawVectors = true;
                    }
                }
            }
        }

        /// <summary>
        /// remove selected labels
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveSelectedLabel(object sender, EventArgs e)
        {
            for (int isel = listBox1.SelectedIndices.Count - 1; isel >= 0; isel--)
            {
                int index = listBox1.SelectedIndices[isel];
                if ((index >= 0) && (index < listBox1.Items.Count) && (listBox1.Items.Count > 2))
                {
                    DrawObject.ItemValues.RemoveAt(index);
                    DrawObject.LineColors.RemoveAt(index);
                    DrawObject.FillColors.RemoveAt(index);
                }
            }
            listBox1.Items.Clear();
            for (int i = 0; i < DrawObject.ItemValues.Count; i++)
            {
                listBox1.Items.Add(Math.Round(Convert.ToDouble(DrawObject.ItemValues[i], ic), Convert.ToInt32(numericUpDown4.Value)));
            }

            //enable re-calculation of contours
            if (DrawObject.Name.Substring(0, 3) == "CM:")
            {
                domain.ReDrawContours = true;
            }
            //enable re-calculation of vectors
            if (DrawObject.Name.Substring(0, 3) == "VM:")
            {
                domain.ReDrawVectors = true;
            }
        }

        /// <summary>
        /// Save color scales and settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveColorSettings(object sender, EventArgs e)
        {
            string newPath = Path.Combine(Main.ProjectName, "Settings" + Path.DirectorySeparatorChar);
            SaveFileDialog dialog = saveFileDialog1;
            dialog.Filter = "*.lay|*.lay";
            dialog.Title = "Save color scale";
            dialog.InitialDirectory = newPath;
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogSettings;
#endif

            TypeConverter col = TypeDescriptor.GetConverter(typeof(Color));
            if (dialog.ShowDialog() == DialogResult.OK)
            {

                try
                {
                    using (StreamWriter mywriter = new StreamWriter(dialog.FileName))
                    {
                        mywriter.WriteLine("Version02");
                        for (int i = 0; i < DrawObject.ItemValues.Count; i++)
                        {
                            mywriter.WriteLine(DrawObject.ItemValues[i].ToString(ic));
                            mywriter.WriteLine(ColorTranslator.ToHtml(DrawObject.LineColors[i]));
                            mywriter.WriteLine(ColorTranslator.ToHtml(DrawObject.FillColors[i]));
                        }

                        mywriter.WriteLine("-----");
                        mywriter.WriteLine(DrawObject.LineWidth.ToString());
                        mywriter.WriteLine(numericUpDown1.Value.ToString(ic));
                        mywriter.WriteLine(numericUpDown6.Value.ToString(ic));
                        mywriter.WriteLine(numericUpDown4.Value.ToString(ic));
                        mywriter.WriteLine(ColorTranslator.ToHtml(DrawObject.LabelColor));
                        mywriter.WriteLine(DrawObject.LabelFont.FontFamily.Name + ":" +
                                           DrawObject.LabelFont.Size + ":" +
                                           (int)DrawObject.LabelFont.Style);

                        mywriter.WriteLine(numericUpDown2.Value.ToString(ic));
                        mywriter.WriteLine(numericUpDown3.Value.ToString(ic));
                        mywriter.WriteLine(numericUpDown5.Value.ToString(ic));
                        mywriter.WriteLine(numericUpDown7.Value.ToString(ic));
                        mywriter.WriteLine(numericUpDown8.Value.ToString(ic));
                        mywriter.WriteLine(numericUpDown9.Value.ToString(ic));

                        mywriter.WriteLine(checkBox1.Checked.ToString(ic));
                        mywriter.WriteLine(checkBox4.Checked.ToString(ic));
                        mywriter.WriteLine(checkBox6.Checked.ToString(ic));
                        mywriter.WriteLine(checkBox7.Checked.ToString(ic));

                        mywriter.WriteLine(radioButton1.Checked.ToString(ic));
                        mywriter.WriteLine(radioButton2.Checked.ToString(ic));
                        mywriter.WriteLine(DrawObject.Label.ToString(ic));

                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Load color scale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadColorSettings(object sender, EventArgs e)
        {
            string newPath = Path.Combine(Main.ProjectName, "Settings" + Path.DirectorySeparatorChar);
            OpenFileDialog dialog = openFileDialog1;
            dialog.Filter = "*.lay|*.lay";
            dialog.Title = "Load color scale";
            dialog.InitialDirectory = newPath;
#if NET6_0_OR_GREATER
            dialog.ClientGuid = GralStaticFunctions.St_F.FileDialogSettings;
#endif

            TypeConverter col = TypeDescriptor.GetConverter(typeof(Color));
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DrawObject.ItemValues.Clear();
                    DrawObject.FillColors.Clear();
                    DrawObject.LineColors.Clear();
                    listBox1.Items.Clear();

                    int FileVersion = 0; // flag for format of layout file
                    string _col = string.Empty;

                    using (StreamReader myreader = new StreamReader(dialog.FileName))
                    {
                        string text = myreader.ReadLine();

                        // check version
                        if (text == "Version01")
                        {
                            FileVersion = 1;
                            text = myreader.ReadLine();
                        }
                        if (text == "Version02")
                        {
                            FileVersion = 2;
                            text = myreader.ReadLine();
                        }

                        while (text != null && text != "-----")
                        {
                            DrawObject.ItemValues.Add(St_F.TxtToDbl(text, false));

                            double val = St_F.TxtToDblICult(text, false);
                            val = Math.Round(val, (int)numericUpDown4.Value);

                            listBox1.Items.Add(val);

                            text = myreader.ReadLine();

                            if (FileVersion == 0) // old format
                            {
#if __MonoCS__
                                _col = text.Replace(";", ",");
#else
                                _col = text.Replace(";", listsep);
#endif
                                DrawObject.LineColors.Add((Color)col.ConvertFromString(_col));
                            }
                            else if (FileVersion >= 1) // new format
                            {
                                DrawObject.LineColors.Add((Color)ColorTranslator.FromHtml(text));
                            }

                            text = myreader.ReadLine();

                            if (FileVersion == 0) // old format
                            {
#if __MonoCS__
                                _col = text.Replace(";", ",");
#else
                                _col = text.Replace(";", listsep);
#endif
                                DrawObject.FillColors.Add((Color)col.ConvertFromString(_col));
                            }
                            else if (FileVersion >= 1) // new format
                            {
                                DrawObject.FillColors.Add((Color)ColorTranslator.FromHtml(text));
                            }

                            text = myreader.ReadLine();
                        }

                        if (text == "-----" && sender.Equals(button14) && FileVersion > 1) // read additional data
                        {
                            text = myreader.ReadLine();
                            DrawObject.LineWidth = Convert.ToInt32(text, ic);
                            text = myreader.ReadLine();
                            numericUpDown1.Value = Convert.ToDecimal(text, ic);
                            text = myreader.ReadLine();
                            numericUpDown6.Value = Convert.ToDecimal(text, ic);
                            text = myreader.ReadLine();
                            numericUpDown4.Value = Convert.ToDecimal(text, ic);
                            text = myreader.ReadLine();
                            DrawObject.LabelColor = (Color)ColorTranslator.FromHtml(text);

                            text = myreader.ReadLine();
                            string[] parts = text.Split(':');
                            if (parts.Length == 3)
                            {
                                Font font = new Font(parts[0], float.Parse(parts[1]), (FontStyle)int.Parse(parts[2]));
                                DrawObject.LabelFont = font;
                            }

                            text = myreader.ReadLine();
                            numericUpDown2.Value = Convert.ToDecimal(text, ic);
                            text = myreader.ReadLine();
                            numericUpDown3.Value = Convert.ToDecimal(text, ic);
                            text = myreader.ReadLine();
                            numericUpDown5.Value = Convert.ToDecimal(text, ic);
                            text = myreader.ReadLine();
                            numericUpDown7.Value = Convert.ToDecimal(text, ic);
                            text = myreader.ReadLine();
                            numericUpDown8.Value = Convert.ToDecimal(text, ic);
                            text = myreader.ReadLine();
                            numericUpDown9.Value = Convert.ToDecimal(text, ic);

                            text = myreader.ReadLine();
                            checkBox1.Checked = Convert.ToBoolean(text, ic);
                            text = myreader.ReadLine();
                            checkBox4.Checked = Convert.ToBoolean(text, ic);
                            text = myreader.ReadLine();
                            checkBox6.Checked = Convert.ToBoolean(text, ic);
                            text = myreader.ReadLine();
                            checkBox7.Checked = Convert.ToBoolean(text, ic);
                            text = myreader.ReadLine();

                            radioButton1.Checked = Convert.ToBoolean(text, ic);
                            text = myreader.ReadLine();
                            radioButton2.Checked = Convert.ToBoolean(text, ic);
                            text = myreader.ReadLine();
                            DrawObject.Label = Convert.ToInt32(text, ic);
                            if (DrawObject.Label == 2)
                            {
                                CheckBox3.Checked = true;
                                CheckBox3.CheckState = CheckState.Checked;
                            }
                            else if (DrawObject.Label == 3)
                            {
                                CheckBox3.Checked = true;
                                CheckBox3.CheckState = CheckState.Indeterminate;
                            }
                            else if (DrawObject.Label == 1)
                            {
                                CheckBox3.Checked = false;
                                CheckBox3.CheckState = CheckState.Unchecked;
                            }
                            else if (DrawObject.Label == 0)
                            {
                                CheckBox3.Visible = false;
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show(this, "Unable to read the settings file", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

            //enable re-calculation of contours
            if (DrawObject.Name.Substring(0, 3) == "CM:")
            {
                domain.ReDrawContours = true;
            }
            //enable re-calculation of contours
            if (DrawObject.Name.Substring(0, 3) == "PM:")
            {
                domain.ReDrawContours = true;
            }
            //enable re-calculation of vectors
            if (DrawObject.Name.Substring(0, 3) == "VM:")
            {
                domain.ReDrawVectors = true;
            }
        }

        /// <summary>
        /// Apply a color gradient between the first and last color of the color scale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button9_Click(object sender, EventArgs e)
        {
            Button bt = sender as Button;

            if (DrawObject.FillColors.Count > 2)
            {
                // default color gradient
                int r1 = DrawObject.FillColors[0].R;
                int g1 = DrawObject.FillColors[0].G;
                int b1 = DrawObject.FillColors[0].B;
                int r2 = DrawObject.FillColors[DrawObject.FillColors.Count - 1].R;
                int g2 = DrawObject.FillColors[DrawObject.FillColors.Count - 1].G;
                int b2 = DrawObject.FillColors[DrawObject.FillColors.Count - 1].B;


                int r11 = DrawObject.LineColors[0].R;
                int g11 = DrawObject.LineColors[0].G;
                int b11 = DrawObject.LineColors[0].B;
                int r21 = DrawObject.LineColors[DrawObject.FillColors.Count - 1].R;
                int g21 = DrawObject.LineColors[DrawObject.FillColors.Count - 1].G;
                int b21 = DrawObject.LineColors[DrawObject.FillColors.Count - 1].B;

                for (int i = 1; i < DrawObject.FillColors.Count - 1; i++)
                {
                    int intr = r1 + (r2 - r1) / DrawObject.FillColors.Count * i;
                    int intg = g1 + (g2 - g1) / DrawObject.FillColors.Count * i;
                    int intb = b1 + (b2 - b1) / DrawObject.FillColors.Count * i;

                    int intr1 = r11 + (r21 - r11) / DrawObject.LineColors.Count * i;
                    int intg1 = g11 + (g21 - g11) / DrawObject.LineColors.Count * i;
                    int intb1 = b11 + (b21 - b11) / DrawObject.LineColors.Count * i;

                    if (bt == button9)
                    {
                        DrawObject.FillColors[i] = Color.FromArgb(intr, intg, intb);
                    }
                    else if (bt == button13)
                    {
                        DrawObject.LineColors[i] = Color.FromArgb(intr1, intg1, intb1);
                    }
                }
                listBox1.Refresh();
            }
            else
            {
                MessageBox.Show(this, "At least three levels are necessary", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Use predefined color scales
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int hclType = Math.Min(comboBox3.SelectedIndex, HCLColor.HCLColor.GetLength(0) - 1);
            if (hclType > 0)
            {
                for (int i = 0; i < DrawObject.FillColors.Count; i++)
                {
                    DrawObject.FillColors[i] = HCLColor.HCLColor[hclType, Math.Min(i, HCLColor.HCLColor.GetLength(1) - 1)];
                    DrawObject.LineColors[i] = HCLColor.HCLColor[hclType, Math.Min(i, HCLColor.HCLColor.GetLength(1) - 1)];
                }
            }
            if (e != null)
            {
                listBox1.Refresh();
            }
        }

        /// <summary>
        /// Show/hide color scale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (init == true)
            {
                if (checkBox2.Checked == true)
                {
                    DrawObject.ColorScale = Convert.ToString(Math.Max(10, domain.picturebox1.Width - 350)) + "," + Convert.ToString(Math.Max(10, domain.picturebox1.Height - 300)) + "," + "1";
                    label6.Enabled = true;
                    label7.Enabled = true;
                    numericUpDown2.Enabled = true;
                    textBox1.Enabled = true;
                    textBox2.Enabled = true;
                    domain.ActualEditedDrawingObject = DrawObject;
                    domain.MouseControl = GralDomain.MouseMode.ViewLegendPos;
                    domain.Cursor = System.Windows.Forms.Cursors.Cross;
                }
                else
                {
                    DrawObject.ColorScale = "-999,-999,-999";
                    label6.Enabled = false;
                    label7.Enabled = false;
                    numericUpDown2.Enabled = false;
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    domain.MouseControl = GralDomain.MouseMode.Default;
                    domain.Cursor = System.Windows.Forms.Cursors.Default;
                }
            }
        }

        /// <summary>
        /// Move the scale along with the map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            DrawObject.BasedOnMap = checkBox8.Checked;
        }

        /// <summary>
        /// change size of color scale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (init == true)
            {
                string[] dummy = new string[3];
                dummy = DrawObject.ColorScale.Split(new char[] { ',' });
                decimal oldValue = Convert.ToDecimal(dummy[2], ic);
                double s = Convert.ToDouble(numericUpDown2.Value, ic);
                DrawObject.ColorScale = dummy[0] + "," + dummy[1] + "," + Convert.ToString(s, ic);

                SetNumericupDownIncrement(numericUpDown2.Value, oldValue);
            }
        }

        private void SetNumericupDownIncrement(decimal value, decimal oldValue)
        {
            if (value <= 0.1m)
            {
                numericUpDown2.Increment = 0.01m;
            }

            if (value > 0.09m && oldValue < 0.1m)
            {
                numericUpDown2.Increment = 0.1m;
            }
        }

        /// <summary>
        /// change title of legend
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (init == true)
            {
                DrawObject.LegendTitle = textBox1.Text;
            }
        }

        /// <summary>
        /// change unit of legend
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            if (init == true)
            {
                DrawObject.LegendUnit = textBox2.Text;
            }
        }

        /// <summary>
        /// change the width of lines of objects
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button11_Click(object sender, EventArgs e)
        {
            string trans = Convert.ToString(DrawObject.LineWidth);
            if (St_F.InputBoxValue("Set line width", "Value:", ref trans, this) == DialogResult.OK)
            {
                try
                {
                    int w_old = DrawObject.LineWidth;
                    int w = Convert.ToInt32(trans);
                    DrawObject.LineWidth = w;
                    //enable re-calculation of contours
                    if (DrawObject.Name.Substring(0, 3) == "CM:" && ((w_old == 0 && w > 0) || (w_old > 0 && w == 0)))
                    {
                        domain.ReDrawContours = true;
                    }
                }
                catch
                {
                    MessageBox.Show(this, "Invalid integer number", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// low pass filter for contour maps on/off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (init == true)
            {
                domain.ReDrawContours = true;
            }
            if (checkBox4.Checked == true)
            {
                DrawObject.Filter = true;
            }
            else
            {
                DrawObject.Filter = false;
            }
        }

        /// <summary>
        /// change the scale of vectors for a vector map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            DrawObject.VectorScale = (float)(numericUpDown3.Value);

            //enable re-calculation of vectors
            if (DrawObject.Name.Substring(0, 3) == "VM:")
            {
                domain.ReDrawVectors = true;
            }
        }

        /// <summary>
        /// change the number of decimal places
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            DrawObject.DecimalPlaces = Convert.ToInt32(numericUpDown4.Value);
            for (int i = 0; i < DrawObject.ItemValues.Count; i++)
            {
                listBox1.Items.Add(Math.Round(Convert.ToDouble(DrawObject.ItemValues[i], ic), Convert.ToInt32(numericUpDown4.Value)));
            }
        }

        /// <summary>
        /// Change the minimum area that shpuld be drawn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NumericUpDown5ValueChanged(object sender, EventArgs e)
        {
            if (DrawObject.ContourAreaMin != Convert.ToInt32(numericUpDown5.Value))
            {
                DrawObject.ContourAreaMin = Convert.ToInt32(numericUpDown5.Value);
                domain.ReDrawContours = true;
            }
        }

        /// <summary>
        /// computes equidistant levels
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button2_Click(object sender, EventArgs e)
        {
            double min = (double)listBox1.Items[0];
            double max = (double)listBox1.Items[listBox1.Items.Count - 1];
            string trans = Convert.ToString((max - min) / 10);
            if (St_F.InputBoxValue("Equidistance", "Value:", ref trans, this) == DialogResult.OK)
            {
                try
                {
                    double distance = 0;
                    if (double.TryParse(trans, out distance))
                    {
                        distance = Math.Abs(distance);
                        int r1 = DrawObject.FillColors[0].R;
                        int g1 = DrawObject.FillColors[0].G;
                        int b1 = DrawObject.FillColors[0].B;
                        int r2 = DrawObject.FillColors[DrawObject.FillColors.Count - 1].R;
                        int g2 = DrawObject.FillColors[DrawObject.FillColors.Count - 1].G;
                        int b2 = DrawObject.FillColors[DrawObject.FillColors.Count - 1].B;


                        int r11 = DrawObject.LineColors[0].R;
                        int g11 = DrawObject.LineColors[0].G;
                        int b11 = DrawObject.LineColors[0].B;
                        int r21 = DrawObject.LineColors[DrawObject.FillColors.Count - 1].R;
                        int g21 = DrawObject.LineColors[DrawObject.FillColors.Count - 1].G;
                        int b21 = DrawObject.LineColors[DrawObject.FillColors.Count - 1].B;

                        DrawObject.FillColors.Clear();
                        DrawObject.LineColors.Clear();
                        DrawObject.ItemValues.Clear();

                        DrawObject.FillColors.Add(Color.FromArgb(r1, g1, b1));
                        DrawObject.LineColors.Add(Color.FromArgb(r11, g11, b11));
                        DrawObject.ItemValues.Add(min);

                        int numblevels = Convert.ToInt32((max - min) / distance) + 1;

                        for (int i = 1; i < numblevels - 1; i++)
                        {
                            int intr = r1 + (r2 - r1) / numblevels * i;
                            int intg = g1 + (g2 - g1) / numblevels * i;
                            int intb = b1 + (b2 - b1) / numblevels * i;

                            int intr1 = r11 + (r21 - r11) / numblevels * i;
                            int intg1 = g11 + (g21 - g11) / numblevels * i;
                            int intb1 = b11 + (b21 - b11) / numblevels * i;

                            DrawObject.FillColors.Add(Color.FromArgb(intr, intg, intb));
                            DrawObject.LineColors.Add(Color.FromArgb(intr1, intg1, intb1));
                            DrawObject.ItemValues.Add(min + i * distance);
                        }

                        DrawObject.FillColors.Add(Color.FromArgb(r2, g2, b2));
                        DrawObject.LineColors.Add(Color.FromArgb(r21, g21, b21));
                        DrawObject.ItemValues.Add(max);

                        listBox1.Items.Clear();

                        for (int i = 0; i < DrawObject.ItemValues.Count; i++)
                        {
                            listBox1.Items.Add(DrawObject.ItemValues[i]);
                        }

                        listBox1.Refresh();

                        //enable re-calculation of contours
                        if (DrawObject.Name.Substring(0, 3) == "CM:")
                        {
                            domain.ReDrawContours = true;
                        }
                        //enable re-calculation of vectors
                        if (DrawObject.Name.Substring(0, 3) == "VM:")
                        {
                            domain.ReDrawVectors = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, "Invalid input", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                    MessageBox.Show(this, "Invalid input", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// set contour levels to default values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button12_Click(object sender, EventArgs e)
        {
            string file = DrawObject.ContourFilename;
            string[] data = new string[100];
            //open data of raster file

            if (System.IO.File.Exists(file) == false) // try source strenght if no map is available
            {
                ComboBox2_SelectedIndexChanged(null, null);
                return;
            }

            try
            {
                StreamReader myReader = new StreamReader(file);
                data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                int nx = Convert.ToInt32(data[1]);
                data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                int ny = Convert.ToInt32(data[1]);
                data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                double x11 = Convert.ToDouble(data[1], ic);
                data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                double y11 = Convert.ToDouble(data[1], ic);
                data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                double dx = Convert.ToDouble(data[1], ic);
                data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                int nodata = Convert.ToInt32(data[1]);
                data = new string[nx];
                double[,] zlevel = new double[nx, ny];
                double min = 100000000;
                double max = -100000000;
                for (int i = ny - 1; i > -1; i--)
                {
                    data = myReader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < nx; j++)
                    {
                        if (DrawObject.Name.Substring(0, 3) == "VM:") // Vector-Maps
                        {
                            double u = Convert.ToDouble(data[j * 2], ic);
                            double v = Convert.ToDouble(data[j * 2 + 1], ic);
                            zlevel[j, i] = Math.Sqrt(u * u + v * v); // u and v components are stored at the .vec file
                        }
                        else
                        {
                            zlevel[j, i] = Convert.ToDouble(data[j], ic);
                        }

                        if (Convert.ToInt32(zlevel[j, i]) != nodata)
                        {
                            min = Math.Min(min, zlevel[j, i]);
                            max = Math.Max(max, zlevel[j, i]);
                        }
                    }
                }
                myReader.Close();
                myReader.Dispose();

                listBox1.Items.Clear();

                DrawObject.FillColors.Clear();
                DrawObject.LineColors.Clear();
                DrawObject.ItemValues.Clear();

                //compute values for 9 contours
                for (int i = 0; i < 9; i++)
                {
                    DrawObject.ItemValues.Add(0);
                    DrawObject.FillColors.Add(Color.Red);
                    DrawObject.LineColors.Add(Color.Red);
                }
                DrawObject.FillColors[0] = Color.Yellow;
                DrawObject.LineColors[0] = Color.Yellow;

                double val = min + (max - min) / Math.Pow(2, Convert.ToDouble(8));

                if (checkBox5.Checked)
                {
                    val = Normalize(val, double.MinValue);
                }

                DrawObject.ItemValues[0] = val;

                val = max;
                if (checkBox5.Checked)
                {
                    val = Normalize(val, double.MinValue);
                }

                DrawObject.ItemValues[8] = val;
                //apply color gradient between light green and red
                int r1 = DrawObject.FillColors[0].R;
                int g1 = DrawObject.FillColors[0].G;
                int b1 = DrawObject.FillColors[0].B;
                int r2 = DrawObject.FillColors[8].R;
                int g2 = DrawObject.FillColors[8].G;
                int b2 = DrawObject.FillColors[8].B;
                double valPrev = max;
                for (int i = 0; i < 7; i++)
                {
                    val = min + (max - min) / Math.Pow(2, Convert.ToDouble(8 - (i + 1)));

                    if (DrawObject.Name.Substring(0, 3) == "VM:")
                    {
                        val = min + (max - min) / 10 * Convert.ToDouble(i + 1);
                    }

                    if (checkBox5.Checked)
                    {
                        val = Normalize(val, valPrev);
                    }
                    valPrev = val;
                    DrawObject.ItemValues[i + 1] = val;
                    int intr = r1 + (r2 - r1) / 10 * (i + 1);
                    int intg = g1 + (g2 - g1) / 10 * (i + 1);
                    int intb = b1 + (b2 - b1) / 10 * (i + 1);
                    DrawObject.FillColors[i + 1] = Color.FromArgb(intr, intg, intb);
                    DrawObject.LineColors[i + 1] = Color.FromArgb(intr, intg, intb);
                }

                for (int i = 0; i < DrawObject.ItemValues.Count; i++)
                {
                    listBox1.Items.Add(DrawObject.ItemValues[i]);
                }

                listBox1.Refresh();

                //enable re-calculation of contours
                if (DrawObject.Name.Substring(0, 3) == "CM:")
                {
                    domain.ReDrawContours = true;
                }
                //enable re-calculation of vectors
                if (DrawObject.Name.Substring(0, 3) == "VM:")
                {
                    domain.ReDrawVectors = true;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(this, err.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        /// <summary>
        /// Set values to the digits 1, 2, 4, 5, 8 or 10
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private double Normalize(double val, double previous)
        {
            int exp = 0;
            int negative = 1;
            if (val < 0)
            {
                negative = -1;
            }
            val = Math.Abs(val);
            double off = 0;
            double norm = 0;
            do
            {
                val += off * Math.Pow(10, exp);
                if (val > double.MinValue)
                {
                    exp = (int)Math.Floor(Math.Log10(val)) * (-1);
                }

                norm = val * Math.Pow(10, exp);

                if (norm < 1.5)
                {
                    norm = 1;
                }
                else if (norm < 3)
                {
                    norm = 2;
                }
                else if (norm < 4)
                {
                    norm = 4;
                }
                else if (norm < 6.5)
                {
                    norm = 5;
                }
                else if (norm < 9)
                {
                    norm = 8;
                }
                else
                {
                    norm = 10;
                }

                exp *= -1;
                off -= 0.2;
                double temp = norm * Math.Pow(10, exp) * negative;
            }
            while (norm * Math.Pow(10, exp) * negative == previous && off > -10);


            return norm * Math.Pow(10, exp) * negative;
        }

        /// <summary>
        /// method for changing the opacity of an image
        /// </summary>
        /// <param name="image">image to set opacity on</param>
        /// <param name="opacity">percentage of opacity</param>
        /// <returns></returns>
        private Bitmap SetImageOpacity(Image image, float opacity)
        {
            try
            {
                //create a Bitmap the size of the image provided
                Bitmap bmp = new Bitmap(image.Width, image.Height);

                //create a graphics object from the image
                using (Graphics gfx = Graphics.FromImage(bmp))
                {

                    {
                        gfx.Clear(Color.White);
                    }

                    //create a color matrix object
                    ColorMatrix matrix = new ColorMatrix
                    {

                        //set the opacity
                        Matrix33 = opacity
                    };

                    //create image attributes
                    ImageAttributes attributes = new ImageAttributes();

                    //set the color(opacity) of the image
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    //now draw the image
                    gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
                return bmp;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "GRAL GUI");
                return null;
            }
        }

        /// <summary>
        /// Close the layout manager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LayoutFormClosed(object sender, FormClosedEventArgs e)
        {
            listBox1.DrawItem -=
                new System.Windows.Forms.DrawItemEventHandler(ListBox1_DrawItem);

            listBox1.Items.Clear();
            listBox1.Dispose();
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();
            comboBox1.Dispose();
            comboBox2.Dispose();
            comboBox3.Dispose();
            toolTip1.Dispose();
            fontDialog1.Dispose();
            colorDialog1.Dispose();
            saveFileDialog1.Dispose();
            openFileDialog1.Dispose();
            if (UpdateObjectManager)
            {
                // send Message to object manager, that an update is needed
                try
                {
                    if (UpdateListbox != null)
                    {
                        UpdateListbox(this, null);
                    }
                }
                catch
                { }
            }
        }

        /// <summary>
        /// Disable simple countour mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RadioButton1CheckedChanged(object sender, EventArgs e) // Bourke Contour lines
        {
            if (radioButton1.Checked)
            {
                if (DrawObject.DrawSimpleContour == true)
                {
                    DrawObject.DrawSimpleContour = false;
                    domain.ReDrawContours = true;
                    label12.Text = "Number of filter points";
                    label13.Text = "Filter sigma value";
                    numericUpDown7.Enabled = true;
                    numericUpDown7.Value = 0;
                    numericUpDown7.DecimalPlaces = 0;
                    numericUpDown7.Increment = 1;
                    numericUpDown8.Maximum = 10;
                }
                checkBox6.Enabled = false;
                groupBox6.Enabled = false;
            }
        }

        /// <summary>
        /// Enable simple countour mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RadioButton2CheckedChanged(object sender, EventArgs e) // Kuntner Contour lines
        {
            if (radioButton2.Checked)
            {
                if (DrawObject.DrawSimpleContour == false)
                {
                    DrawObject.DrawSimpleContour = true;
                    domain.ReDrawContours = true;
                    label12.Text = "Filter lines [m]";
                    label13.Text = "Spline Tension";
                    numericUpDown7.Enabled = true;
                    numericUpDown7.Value = 0.1M;
                    numericUpDown7.DecimalPlaces = 1;
                    numericUpDown7.Increment = 0.1M;
                    numericUpDown8.Value = 0.1M;
                    numericUpDown8.Maximum = 1;
                }
                checkBox6.Enabled = true;
                groupBox6.Enabled = true;
            }
        }

        /// <summary>
        /// Set opacity from numericupdown to trackbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NumericUpDown9ValueChanged(object sender, EventArgs e)
        {
            trackBar1.Value = (int)(numericUpDown9.Value);
        }

        /// <summary>
        /// Set opacity from trackbar to numericupdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TrackBar1ValueChanged(object sender, EventArgs e)
        {
            numericUpDown9.Value = trackBar1.Value;
        }

        /// <summary>
        /// Change the name of an object 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            string temp = DrawObject.Name;
            if (textBox3.Text.Length > 4)
            {
                if (textBox3.Text.StartsWith("CM:"))
                {
                    DrawObject.Name = textBox3.Text;
                }
                else
                {
                    DrawObject.Name = "CM:" + textBox3.Text;
                }
            }
            if (!string.Equals(temp, DrawObject.Name))
            {
                UpdateObjectManager = true;
            }
        }

        /// <summary>
        /// Change the file, associated to an object or  layer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button15_Click(object sender, EventArgs e)
        {
            string filename = string.Empty;
            string folder = Path.Combine(Main.ProjectName, "Maps");
            if (System.IO.File.Exists(DrawObject.ContourFilename))
            {
                filename = DrawObject.ContourFilename;
                folder = Path.GetFullPath(DrawObject.ContourFilename);
            }
            else
            {
                filename = Path.GetFileName(DrawObject.Name);
            }

            using (OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "(*.dat;*.txt)|*.dat;*.txt",
                Title = "Select raster data (ASCII Format)",
                FileName = Path.GetFileName(filename),
                InitialDirectory = folder
#if NET6_0_OR_GREATER
                ,
                ClientGuid = GralStaticFunctions.St_F.FileDialogMaps
#endif
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    filename = dialog.FileName;
                }
                else
                {
                    return;
                }
            }

            if (filename != DrawObject.ContourFilename)
            {
                DrawObject.ContourFilename = filename;
                domain.ReDrawContours = true;
                textBox4.Text = DrawObject.ContourFilename;
            }
        }

        private class HCLColorMap
        {
            public Color[,] HCLColor = new Color[7, 7];
            public string[] HCLName = new string[7] { "Default colors", "Sequential Blue", "Sequential Purple", "Sequential Red", "Sequential Green", "Diverging Green Orange", "Diverging Blue Red" };

            public HCLColorMap()
            {
                // SeqBlue
                HCLColor[1, 6] = Color.FromArgb(0, 54, 108);
                HCLColor[1, 5] = Color.FromArgb(0, 93, 154);
                HCLColor[1, 4] = Color.FromArgb(44, 134, 202);
                HCLColor[1, 3] = Color.FromArgb(121, 171, 226);
                HCLColor[1, 2] = Color.FromArgb(173, 204, 246);
                HCLColor[1, 1] = Color.FromArgb(216, 233, 255);
                HCLColor[1, 0] = Color.FromArgb(248, 248, 248);

                // SeqPurple
                HCLColor[2, 6] = Color.FromArgb(49, 34, 113);
                HCLColor[2, 5] = Color.FromArgb(89, 76, 160);
                HCLColor[2, 4] = Color.FromArgb(129, 117, 203);
                HCLColor[2, 3] = Color.FromArgb(167, 159, 225);
                HCLColor[2, 2] = Color.FromArgb(202, 197, 243);
                HCLColor[2, 1] = Color.FromArgb(231, 228, 254);
                HCLColor[2, 0] = Color.FromArgb(248, 248, 248);

                // SeqRed
                HCLColor[3, 6] = Color.FromArgb(105, 0, 12);
                HCLColor[3, 5] = Color.FromArgb(170, 19, 36);
                HCLColor[3, 4] = Color.FromArgb(238, 37, 58);
                HCLColor[3, 3] = Color.FromArgb(255, 112, 120);
                HCLColor[3, 2] = Color.FromArgb(255, 166, 170);
                HCLColor[3, 1] = Color.FromArgb(255, 212, 214);
                HCLColor[3, 0] = Color.FromArgb(248, 248, 248);

                // SeqGreen
                HCLColor[4, 6] = Color.FromArgb(0, 70, 22);
                HCLColor[4, 5] = Color.FromArgb(13, 116, 52);
                HCLColor[4, 4] = Color.FromArgb(32, 161, 78);
                HCLColor[4, 3] = Color.FromArgb(116, 194, 134);
                HCLColor[4, 2] = Color.FromArgb(172, 221, 182);
                HCLColor[4, 1] = Color.FromArgb(217, 241, 222);
                HCLColor[4, 0] = Color.FromArgb(248, 248, 248);

                // Div Green Orange
                HCLColor[5, 0] = Color.FromArgb(17, 198, 56);
                HCLColor[5, 1] = Color.FromArgb(119, 209, 127);
                HCLColor[5, 2] = Color.FromArgb(176, 218, 179);
                HCLColor[5, 3] = Color.FromArgb(226, 226, 226);
                HCLColor[5, 4] = Color.FromArgb(237, 201, 176);
                HCLColor[5, 5] = Color.FromArgb(241, 176, 119);
                HCLColor[5, 6] = Color.FromArgb(239, 151, 8);

                // Div Blue Red
                HCLColor[6, 0] = Color.FromArgb(0, 47, 112);
                HCLColor[6, 1] = Color.FromArgb(81, 122, 201);
                HCLColor[6, 2] = Color.FromArgb(180, 194, 235);
                HCLColor[6, 3] = Color.FromArgb(226, 226, 226);
                HCLColor[6, 4] = Color.FromArgb(237, 180, 181);
                HCLColor[6, 5] = Color.FromArgb(192, 93, 93);
                HCLColor[6, 6] = Color.FromArgb(95, 20, 21);
            }
        }
    }
}