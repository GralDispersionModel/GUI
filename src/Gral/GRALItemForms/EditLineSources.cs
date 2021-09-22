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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Gral;
using GralDomain;
using GralItemData;
using GralStaticFunctions;

namespace GralItemForms
{
    public partial class EditLinesources : Form
    {
        private TextBox[] lineemission = new TextBox[10];    //textboxes for emission strenght input of line sources
        private ComboBox[] linepollutant = new ComboBox[10];  //Combobox for selecting pollutants of line sources
        private Label[] labelpollutant = new Label[10];       //Labels for pollutant types
        private Button[] but1 = new Button[10];               //Buttons for unit and deposition edit
        /// <summary>
        /// Collection of all line source data
        /// </summary>
        public List<LineSourceData> ItemData = new List<LineSourceData>();
        private List<PollutantsData> SourceGroupEmission = new List<PollutantsData>(); // emission data for various source groups
        /// <summary>
        /// Number of actual line source to be displayed
        /// </summary>
        public int ItemDisplayNr;
        /// <summary>
        /// Recent number of an edited corner point
        /// </summary>
        public int CornerLineSource = 0;
        private const int MaxVertices = 20000;                              // max. number of edge points

        /// <summary>
        /// X corner points of a line source in meters
        /// </summary>
        public double[] CornerLineX = new double[MaxVertices];
        /// <summary>
        /// Y corner points of a portal source in meters
        /// </summary>
        public double[] CornerLineY = new double[MaxVertices];
        /// <summary>
        /// Z corner points of a portal source in meters
        /// </summary>
        public double[] CornerLineZ = new double[MaxVertices];
        /// <summary>
        /// All souce group names
        /// </summary>
        public List<string> SG_List = new List<string>();

        private Deposition[] dep = new Deposition[10];
        CultureInfo ic = CultureInfo.InvariantCulture;
        private int vertices;                              //number of maximum corner points
        private int Dialog_Initial_Width = 0;
        private int TabControl1_Height = 520;
        private int Button1_Width = 50;
        private int Button1_Height = 10;
        private int TBWidth = 80;
        private int TextBox_x0 = 0;
        private int TrackBar_x0 = 0;
        private int Numericupdown_x0 = 0;
        private int listbox_x0 = 0;
        private int TabControl_x0 = 0;

        // delegate to send a message, that redraw is needed!
        public event ForceDomainRedraw LinesourceRedraw;

        public bool AllowClosing = false;
        //delegates to send a message, that user uses OK or Cancel button
        public event ForceItemOK ItemFormOK;
        public event ForceItemCancel ItemFormCancel;

        public EditLinesources()
        {
            InitializeComponent();
            Domain.EditSourceShape = true;  // allow input of new vertices
                                            //User defined column seperator and decimal seperator

#if __MonoCS__
            var allNumUpDowns = Main.GetAllControls<NumericUpDown> (this);
            foreach (NumericUpDown nu in allNumUpDowns)
            {
                nu.TextAlign = HorizontalAlignment.Left;
            }
#endif

            int y_act = groupBox1.Top + groupBox1.Height + 5;
            int TBHeight = textBox2.Height;

            // get width of button for unit
            Button bt_test = new Button
            {
                Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                AutoEllipsis = false,
                Text = "[MOU/h/km]"
            };
            Button1_Width = bt_test.Width;
            Button1_Height = bt_test.Height;
            bt_test.Dispose();

            y_act = 10;
            for (int i = 0; i < 10; i++)
            {
                createTextbox(6, Convert.ToInt32(y_act) + Convert.ToInt32(i * (TBHeight + 7)), TBWidth, TBHeight, i);
                dep[i] = new Deposition(); // initialize Deposition array
                dep[i].init();
            }

            for (int nr = 0; nr < 10; nr++)
            {
                for (int i = 0; i < Main.PollutantList.Count; i++)
                {
                    linepollutant[nr].Items.Add(Main.PollutantList[i]);
                }
            }
            listBox1.Items.AddRange(Main.NemoTrafficSituations);

            textBox1.KeyPress += new KeyPressEventHandler(Comma1); //only point as decimal seperator is allowed
        }

        private void Editlinesources_Load(object sender, EventArgs e)
        {
            tabControl1.Height = Math.Max(350, lineemission[9].Bottom + 5);
            tabControl1.Width = Math.Max(259, but1[1].Right + 4);
            Dialog_Initial_Width = ClientSize.Width;
            TabControl1_Height = tabControl1.Height;
            TrackBar_x0 = trackBar1.Left;
            TextBox_x0 = textBox1.Left;
            Numericupdown_x0 = numericUpDown1.Left;
            listbox_x0 = listBox2.Left;
            TabControl_x0 = tabControl1.Left;

            FillValues();

            ClientSize = new Size(Math.Max(textBox2.Right + 5, but1[1].Right + 5), tabControl1.Bottom + 5);
            EditlinesourcesResizeEnd(null, null);

#if __MonoCS__
            listBox1.Height = 25;
            trackBar1.Height = 12;
#endif
        }

        private void createTextbox(int x0, int y0, int b, int h, int nr)
        {
            //list box for selcting pollutants
            linepollutant[nr] = new ComboBox
            {
                Location = new System.Drawing.Point(x0, y0),
                Font = textBox2.Font, //new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                Height = textBox2.Height,
                Width = 80,
                DropDownHeight = Math.Max(textBox2.Height * 4, tabPage1.Height - y0)
            };
            tabPage1.Controls.Add(linepollutant[nr]);
            linepollutant[nr].SelectedValueChanged += new System.EventHandler(Odour);

            labelpollutant[nr] = new Label
            {
                Location = new System.Drawing.Point(x0, y0),
                Font = textBox2.Font, //new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                Height = textBox2.Height,
                Width = 80,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };
            tabPage1.Controls.Add(labelpollutant[nr]);

            //text box for input of emissions
            lineemission[nr] = new TextBox
            {
                Location = new System.Drawing.Point(linepollutant[nr].Left + linepollutant[nr].Width + 5, y0),
                Size = new System.Drawing.Size(b - 20, h),
                TextAlign = System.Windows.Forms.HorizontalAlignment.Right,
                Text = "0"
            };
            lineemission[nr].TextChanged += new System.EventHandler(St_F.CheckInput);
            lineemission[nr].KeyPress += new KeyPressEventHandler(St_F.NumericInput);

            tabPage1.Controls.Add(lineemission[nr]);

            //labels
            but1[nr] = new Button
            {
                Location = new System.Drawing.Point(lineemission[nr].Left + lineemission[nr].Width + 5, y0 - 1),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Height = lineemission[0].Height,
                Width = Button1_Width,
                Text = "[kg/h/km]"
            };
            tabPage1.Controls.Add(but1[nr]);
            toolTip1.SetToolTip(but1[nr], "Click to set deposition - green: deposition set");
            but1[nr].Click += new EventHandler(edit_deposition);
        }

        private void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            // Define the default color of the brush as black.
            ListBox lb = sender as ListBox;
            string poll = lb.Items[e.Index].ToString();

            // See if the item is selected.
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                // Selected.
                using (var brush = new SolidBrush(Color.RoyalBlue))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }

                // Draw the current item text based on the current Font
                // and the custom brush settings.
                Brush myBrush = Brushes.White;
                {
                    e.Graphics.DrawString(poll, e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
                }
            }
            else
            {
                // Not selected.
                using (var brush = new SolidBrush(Color.AntiqueWhite))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
                // Draw the current item text based on the current Font
                // and the custom brush settings.
                Brush myBrush = Brushes.Black;
                {
                    e.Graphics.DrawString(poll, e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
                }
            }

            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }

        private void Odour(object sender, EventArgs args)
        {
            for (int nr = 0; nr < 10; nr++)
            {
                if (linepollutant[nr].SelectedIndex == 2)
                {
                    if (checkBox3.Checked)
                    {
                        but1[nr].Text = "[MOU/h]";
                    }
                    else
                    {
                       but1[nr].Text = "[MOU/h/km]";
                    }
                }
                else
                {
                    if (checkBox3.Checked)
                    {
                        but1[nr].Text = "[kg/h]";
                    }
                    else
                    {
                        but1[nr].Text = "[kg/h/km]";
                    }
                }
            }
        }

        private void Comma1(object sender, KeyPressEventArgs e)
        {
            //if (e.KeyChar == ',') e.KeyChar = '.';
            int asc = (int)e.KeyChar; //get ASCII code
        }

        //increase the number of line sources by one
        private void button1_Click(object sender, EventArgs e)
        {
            if ((textBox1.Text != "") && (textBox2.Text != ""))
            {
                SaveArray(true);
                for (int i = 0; i < 10; i++)
                {
                    lineemission[i].Text = "0";
                }

                trackBar1.Maximum += 1;
                trackBar1.Value = trackBar1.Maximum;
                ItemDisplayNr = trackBar1.Maximum - 1;
                Domain.EditSourceShape = true;  // allow input of new vertices
                FillValues();
            }
        }

        //scroll between the line sources
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            SaveArray(false);
            ItemDisplayNr = trackBar1.Value - 1;
            FillValues();
            RedrawDomain(this, null);
        }

        //save data in array list
        /// <summary>
        /// Saves the recent dialog data in the item object and the item list
        /// </summary> 
        public void SaveArray(bool redraw)
        {
            if (Main.Project_Locked)
            {
                return;
            }

            int number_of_vertices = 0;
            Int32.TryParse(textBox2.Text, out number_of_vertices);

            if (listBox2.Items.Count == 0) // no source group added
            {
                if (number_of_vertices > 1 && textBox1.Text.Length > 1)
                {
                    MessageBox.Show(this, "No source group added", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }
            if (number_of_vertices < 2)
            {
                MessageBox.Show(this, "Not at least 2 edge points digitized", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            LineSourceData _ls;
            if (ItemDisplayNr >= ItemData.Count) // new item
            {
                _ls = new LineSourceData();
            }
            else // change existing item
            {
                _ls = ItemData[ItemDisplayNr];
            }

            if (textBox2.Text != "" && number_of_vertices > 1)
            {
                if ((textBox1.Text != "") && (Convert.ToInt32(textBox2.Text) > 1))
                {
                    if (textBox4.Text == "") // section
                    {
                        textBox4.Text = "1";
                    }

                    _ls.Pt.Clear();
                    _ls.Pt.TrimExcess();

                    // new lenght
                    GralData.PointD_3d[] mypoint = new GralData.PointD_3d[MaxVertices];

                    float height = Convert.ToSingle(numericUpDown1.Value);
                    int absHeight = 1;
                    if (checkBox1.Checked) // absolute height over sea
                    {
                        height *= (-1);
                        absHeight = -1;
                    }

                    if (checkBox2.Checked) // 3D Lines
                    {
                        for (int i = 0; i < number_of_vertices; i++)
                        {
                            _ls.Pt.Add(new GralData.PointD_3d(CornerLineX[i], CornerLineY[i], Math.Abs(CornerLineZ[i]) * absHeight));
                        }
                        _ls.Lines3D = true;
                    }
                    else
                    {
                        for (int i = 0; i < number_of_vertices; i++)
                        {
                            _ls.Pt.Add(new GralData.PointD_3d(CornerLineX[i], CornerLineY[i], height));
                            CornerLineZ[i] = height;
                        }
                        _ls.Lines3D = false;
                    }

                    double lenght = St_F.CalcLenght(_ls.Pt);
                    label15.Text = lenght.ToString();

                    if (Convert.ToInt32(textBox2.Text) > 0)
                    {
                        Domain.EditSourceShape = false;  // block input of new vertices
                    }

                    // collect pollutants
                    _ls.Poll.Clear();
                    _ls.Poll.TrimExcess();
                    if (SourceGroupEmission.Count == 0) // a source group is available, but Save SG not pressed
                    {
                        UpdateNewSourcegroupEmission();
                    }

                    foreach (PollutantsData _pd in SourceGroupEmission)
                    {
                        _ls.Poll.Add(_pd);
                    }

                    for (int i = 0; i < 10; i++) // save deposition
                    {
                        _ls.GetDep()[i] = new Deposition(dep[i]);
                    }

                    listBox1.SelectedIndex = listBox1.TopIndex; // NEMO
                    
                    _ls.Name = textBox1.Text;
                    _ls.Section = textBox4.Text;
                    _ls.Height = height;
                    _ls.Width = Convert.ToSingle(numericUpDown2.Value);
                    _ls.VerticalExt = Convert.ToSingle(numericUpDown3.Value);
                    _ls.Nemo.AvDailyTraffic = Convert.ToInt32(numericUpDown6.Value);
                    _ls.Nemo.ShareHDV = Convert.ToSingle(numericUpDown7.Value);
                    _ls.Nemo.Slope = Convert.ToSingle(numericUpDown4.Value);
                    _ls.Nemo.TrafficSit = listBox1.SelectedIndex;
                    _ls.Nemo.BaseYear = Convert.ToInt32(numericUpDown8.Value);

                    if (ItemDisplayNr >= ItemData.Count) // new item
                    {
                        ItemData.Add(_ls);
                    }
                    else
                    {
                        ItemData[ItemDisplayNr] = _ls;
                    }
                    if (redraw)
                    {
                        RedrawDomain(this, null);
                    }
                }
            }
            else
            {
                if (textBox1.Text.Length > 0)
                {
                    MessageBox.Show(this, "Digitize edge points, please", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            // enable Remove SG if SG_count > 1 && a selected entry
            if (listBox2.Items.Count > 1 && listBox2.SelectedIndex >= 0)
            {
                button5.Enabled = true;
            }
            else
            {
                button5.Enabled = false;
            }
        }

        //fill actual values
        /// <summary>
        /// Fills the dialog with data from the recent item object
        /// </summary> 
        public void FillValues()
        {
            try
            {
                //set line source emission rate to kg/h/km
                checkBox3.Checked = false;

                LineSourceData _ldata;
                if (ItemDisplayNr < ItemData.Count)
                {
                    _ldata = ItemData[ItemDisplayNr];
                }
                else
                {
                    if (ItemData.Count > 0)
                    {
                        _ldata = new LineSourceData(ItemData[ItemData.Count - 1]);
                    }
                    else // 1st entry 
                    {
                        _ldata = new LineSourceData();
                        _ldata.Poll.Add(new PollutantsData(1));

                        for (int i = 0; i < 10; i++)
                        {
                            if (linepollutant[i].SelectedIndex < 0)
                            {
                                linepollutant[i].SelectedIndex = 0;
                            }
                        }
                    }
                }

                listBox2.Items.Clear(); // clear Source groups

                textBox1.Text = _ldata.Name;    // name
                textBox4.Text = _ldata.Section; // section
                textBox2.Text = _ldata.Pt.Count.ToString();

                double height = Math.Round(_ldata.Height, 1);
                if (_ldata.Lines3D && _ldata.Pt != null && _ldata.Pt.Count > 0)
                {
                    height = _ldata.Pt[0].Z;
                }

                if (height >= 0) // height above ground
                {
                    checkBox1.Checked = false;
                }
                else // height above sea
                {
                    checkBox1.Checked = true;
                }

                numericUpDown1.Value = St_F.ValueSpan(0, 7999, (double)Math.Abs(height));
                numericUpDown3.Value = St_F.ValueSpan(0, 7999, (double)Math.Abs(_ldata.VerticalExt));

                numericUpDown2.Value = St_F.ValueSpan(0, 1000, _ldata.Width);
                numericUpDown6.Value = St_F.ValueSpan(0, 1000000, _ldata.Nemo.AvDailyTraffic);
                numericUpDown7.Value = St_F.ValueSpan(-0.1, 100, _ldata.Nemo.ShareHDV);
                numericUpDown4.Value = St_F.ValueSpan(-10, 10, _ldata.Nemo.Slope);
                listBox1.SelectedIndex = _ldata.Nemo.TrafficSit;
                numericUpDown8.Value = St_F.ValueSpan(1990, 2200, _ldata.Nemo.BaseYear);

                SourceGroupEmission.Clear();
                SourceGroupEmission.TrimExcess();
                foreach (PollutantsData _poll in _ldata.Poll)
                {
                    SourceGroupEmission.Add(new PollutantsData(_poll));
                    listBox2.Items.Add(SG_List[_poll.SourceGroup - 1]);
                }

                if (listBox2.Items.Count > 0)
                {
                    listBox2.SelectedIndex = 0;
                }
                ListBox2SelectedIndexChanged(null, null); // fill values

                if (_ldata.Pt.Count > CornerLineX.Length)
                {
                    Array.Resize(ref CornerLineX, _ldata.Pt.Count + 1);
                    Array.Resize(ref CornerLineY, _ldata.Pt.Count + 1);
                    Array.Resize(ref CornerLineZ, _ldata.Pt.Count + 1);
                }
                for (int i = 0; i < _ldata.Pt.Count; i++)
                {
                    CornerLineX[i] = _ldata.Pt[i].X;
                    CornerLineY[i] = _ldata.Pt[i].Y;
                    CornerLineZ[i] = Math.Abs(_ldata.Pt[i].Z);
                }

                if (_ldata.Pt.Count > 0)
                {
                    Domain.EditSourceShape = false;  // block input of new vertices
                }

                // new lenght
                double lenght = St_F.CalcLenght(_ldata.Pt);
                label15.Text = St_F.DblToLocTxt(lenght);

                //compute PC speed
                PC_speed();

                for (int i = 0; i < 10; i++)
                {
                    dep[i] = _ldata.GetDep()[i];

                    if (dep[i].V_Dep1 > 0 || dep[i].V_Dep2 > 0 || dep[i].V_Dep3 > 0)
                    {
                        but1[i].BackColor = Color.LightGreen; // mark that deposition is set
                    }
                    else
                    {
                        but1[i].BackColor = SystemColors.ButtonFace; // mark that deposition is reset
                    }
                }

                
                trackBar2.Minimum = 1;
                trackBar2.Maximum = _ldata.Pt.Count;
                trackBar2.Value = 1;

                if (_ldata.Lines3D)
                {
                    checkBox2.Checked = true;
                    trackBar2.Enabled = true;
                    numericUpDown1.Value = St_F.ValueSpan(0, 7999, (double)Math.Abs(CornerLineZ[0]));
                }
                else
                {
                    checkBox2.Checked = false;
                    trackBar2.Enabled = false;
                }
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
                textBox1.Text = "Line Source";

                SourceGroupEmission.Clear();
                SourceGroupEmission.TrimExcess();

                numericUpDown3.Value = 3; // old standard value = 3 m

                for (int i = 0; i < 10; i++)
                {
                    linepollutant[i].SelectedIndex = 0;
                    lineemission[i].Text = "0";
                    but1[i].BackColor = SystemColors.ButtonFace;
                    dep[i].init();
                }
            }

            // enable Remove SG if SG_count > 1 && a selected entry
            if (listBox2.Items.Count > 1 && listBox2.SelectedIndex >= 0)
            {
                button5.Enabled = true;
            }
            else
            {
                button5.Enabled = false;
            }
        }

        //remove actual line source data
        private void button2_Click(object sender, EventArgs e)
        {
            RemoveOne(true);
            RedrawDomain(this, null);
        }

        /// <summary>
        /// Remove the recent item object from the item list
        /// </summary> 
        public void RemoveOne(bool ask)
        {
            // if ask = false do not ask and delete immediality
            if (ask == true)
            {
                if (St_F.InputBoxYesNo("Attention", "Do you really want to delete this source?", St_F.GetScreenAtMousePosition() + 340, 400) == DialogResult.Yes)
                {
                    ask = false;
                }
                else
                {
                    ask = true; // Cancel -> do not delete!
                }
            }
            if (ask == false)
            {
                textBox1.Text = "";
                textBox4.Text = "1";
                numericUpDown1.Value = 0;
                numericUpDown2.Value = 7;
                numericUpDown6.Value = 1000;
                numericUpDown7.Value = Convert.ToDecimal(-0.1);
                numericUpDown4.Value = 0;
                for (int i = 0; i < 10; i++)
                {
                    lineemission[i].Text = "0";
                }

                if (ItemDisplayNr >= 0)
                {
                    try
                    {
                        if (trackBar1.Maximum > 1)
                        {
                            trackBar1.Maximum -= 1;
                        }

                        trackBar1.Value = Math.Min(trackBar1.Maximum, trackBar1.Value);

                        ItemData.RemoveAt(ItemDisplayNr);

                        ItemDisplayNr = trackBar1.Value - 1;
                        FillValues();
                    }
                    catch
                    {
                    }
                }
            }
        }

        //remove all line sources
        private void button3_Click(object sender, EventArgs e)
        {
            if (St_F.InputBox("Attention", "Delete all line sources??", this) == DialogResult.OK)
            {
                SourceGroupEmission.Clear();
                SourceGroupEmission.TrimExcess();

                ItemData.Clear();
                ItemData.TrimExcess();

                listBox2.Items.Clear();

                textBox1.Text = "";
                textBox4.Text = "1";
                numericUpDown1.Value = 0;
                numericUpDown3.Value = 3;
                numericUpDown2.Value = 7;
                numericUpDown6.Value = 1000;
                numericUpDown7.Value = Convert.ToDecimal(-0.1);
                numericUpDown4.Value = 0;
                for (int i = 0; i < 10; i++)
                {
                    linepollutant[i].SelectedIndex = 0;
                    lineemission[i].Text = "0";
                    dep[i].init();
                    but1[i].BackColor = SystemColors.ButtonFace;
                }

                trackBar1.Value = 1;
                trackBar1.Maximum = 1;
                ItemDisplayNr = trackBar1.Value - 1;

                label15.Text = "0";
                textBox2.Text = "0";
                Domain.EditSourceShape = true;  // allow input of new vertices
                RedrawDomain(this, null);
            }
        }

        //save source group
        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex < 0 && listBox2.Items.Count > 1) // no source group selected & more than 1 source groups available
            {
                Color _col = listBox2.BackColor;
                listBox2.BackColor = Color.Yellow;
                MessageBox.Show(this, "No source group selected, please \n - select a source group \n - enter the emission rates \n - save the source group", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                listBox2.BackColor = _col;
                return;
            }
            if (listBox2.Items.Count == 0) // no source group  available
            {
                Color _col = listBox2.BackColor;
                listBox2.BackColor = Color.Yellow;
                MessageBox.Show(this, "No source group added, please \n - add a source group \n - enter the emission rates \n - save the source group", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                listBox2.BackColor = _col;
                return;
            }

            UpdateNewSourcegroupEmission();
            SaveArray(true);
        }

        private void UpdateNewSourcegroupEmission()
        {
            try
            {
                string sg_name = String.Empty;
                if (listBox2.Items.Count == 1) // nothing selected but only 1 sg available
                {
                    sg_name = listBox2.Items[0].ToString();
                }
                else if (listBox2.SelectedIndex >= 0)
                {
                    sg_name = listBox2.SelectedItem.ToString();
                }

                if (sg_name.Length > 0)
                {
                    int sg_nr = St_F.GetSgNumber(sg_name);
                    int index = GetPollutantListIndex(sg_nr);

                    if (index < 0) // new entry
                    {
                        SourceGroupEmission.Add(new PollutantsData());
                        index = SourceGroupEmission.Count - 1;
                    }

                    SourceGroupEmission[index].SourceGroup = sg_nr;

                    for (int i = 0; i < 10; i++)
                    {
                        SourceGroupEmission[index].Pollutant[i] = linepollutant[i].SelectedIndex;
                        double _val = 0;
                        double.TryParse(lineemission[i].Text, out _val);

                        double lenght = 1; // 1km default lenght for kg/h/km
                        if (checkBox3.Checked) // use kg/h as input and recalculate to kg/h/km
                        {
                            lenght = Convert.ToDouble(label15.Text);
                            if (lenght <= 0)
                            {
                                lenght = 1;
                            }
                            else
                            {
                                lenght = 1000 / lenght;
                            }
                        }
                        SourceGroupEmission[index].EmissionRate[i] = _val * lenght;
                    }
                }
            }
            catch
            {

            }
        }

        private int GetPollutantListIndex(int SourceGroupNumber)
        {
            int index = -1;

            for (int i = 0; i < SourceGroupEmission.Count; i++)
            {
                if (SourceGroupEmission[i].SourceGroup == SourceGroupNumber)
                {
                    index = i;
                }
            }

            return index;
        }

        //compute average PC speed based on the chosen traffic situation and the slope
        private void PC_speed()
        {
            if (listBox1.SelectedIndex != 0)
            {
                //read velocities for each vehicle-type according to the driving pattern and the slope
                Assembly _assembly;
                _assembly = Assembly.GetExecutingAssembly();
                StreamReader myReader = new StreamReader(_assembly.GetManifestResourceStream("Gral.Resources.NEMO_velocities.txt"));
                string[] line = new string[400];
                line[0] = myReader.ReadLine();
                string[] velo = new string[11];
                for (int k = 1; k < 400; k++)
                {
                    try
                    {
                        line[k] = myReader.ReadLine();
                    }
                    catch
                    {
                        break;
                    }
                }
                myReader.Close();
                myReader.Dispose();

                for (int k = 1; k < 400; k++)
                {
                    try
                    {
                        velo = line[k].Split(new char[] { ('\t') }, StringSplitOptions.RemoveEmptyEntries);
                        if (velo[0] == Convert.ToString(listBox1.SelectedItem))
                        {
                            //check for the slope
                            double slpnemo = Convert.ToDouble(velo[2], ic);
                            double slpinput = Convert.ToDouble(numericUpDown4.Value, ic);
                            if (slpnemo >= slpinput)
                            {
                                textBox3.Text = St_F.ICultToLCult(velo[4]);
                                break;
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }


        //change traffic situation
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //compute PC speed
            PC_speed();
        }

        //change slope
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            //compute PC speed
            PC_speed();
        }

        void EditlinesourcesResizeEnd(object sender, EventArgs e)
        {
            int dialog_width = ClientSize.Width;

            if (dialog_width < Dialog_Initial_Width)
            {
                dialog_width = Dialog_Initial_Width;
            }

            if (dialog_width > 200)
            {
                dialog_width -= 12;
                trackBar1.Width = ScrollRight.Left - TrackBar_x0;
                trackBar2.Width = dialog_width - TrackBar_x0;
                textBox1.Width = dialog_width - TextBox_x0;
                textBox2.Width = dialog_width - button8.Right - 5;
                textBox4.Width = dialog_width - TextBox_x0;
                label15.Width = dialog_width - TextBox_x0;
                numericUpDown1.Width = dialog_width - TextBox_x0;
                numericUpDown2.Width = dialog_width - TextBox_x0;
                numericUpDown3.Width = dialog_width - TextBox_x0;
                groupBox2.Width = dialog_width - listbox_x0;
                listBox2.Width = groupBox2.Width - listBox2.Left - 4;
                tabControl1.Width = dialog_width - TabControl_x0;
                button5.Left = groupBox2.Width - 5 - button5.Width;
                button7.Left = groupBox2.Width / 2 - button7.Width / 2;
            }

            if (ClientSize.Height > (tabControl1.Top + TabControl1_Height))
            {
                tabControl1.Height = ClientSize.Height - tabControl1.Top;
            }

            int element_width = (tabPage1.Width - but1[1].Width - 20) / 2;
            if (element_width < TBWidth)
            {
                element_width = TBWidth;
            }

            button10.Left = Math.Max(100, this.Width - 100);

            for (int nr = 0; nr < 10; nr++)
            {
                linepollutant[nr].Width = (int)(element_width);
                labelpollutant[nr].Width = (int)(element_width);
                lineemission[nr].Location = new System.Drawing.Point(linepollutant[nr].Left + linepollutant[nr].Width + 5, linepollutant[nr].Top);
                lineemission[nr].Width = (int)(element_width);
                but1[nr].Location = new System.Drawing.Point(lineemission[nr].Left + lineemission[nr].Width + 5, linepollutant[nr].Top - 1);
            }
            panel1.Width = ClientSize.Width;
        }

        void EditlinesourcesVisibleChanged(object sender, EventArgs e) // Kuntner: save sourcedata if form is hidden
        {
            if (!Visible)
            {
                GralDomain.Domain.MarkerPoint.X = 0;
                GralDomain.Domain.MarkerPoint.Y = 0;
            }
            else // Enable/disable items 
            {
                bool enable = !Main.Project_Locked;
                if (enable)
                {
                    labelTitle.Text = "Edit Line Sources";
                }
                else
                {
                    labelTitle.Text = "Line Source Settings (Project Locked)";
                }
                foreach (Control c in Controls)
                {
                    if (c != trackBar1 && c != groupBox2 && c != tabControl1 && c != trackBar2 && c != ScrollRight && c != ScrollLeft)
                    {
                        c.Enabled = enable;
                    }
                }
                button4.Enabled = enable;
                button5.Enabled = enable;
                button7.Enabled = enable;

                foreach (Control c in tabControl1.Controls)
                {
                    if (c != tabPage1 && c != trackBar2)
                    {
                        c.Enabled = enable;
                    }    
                }

                foreach (Control c in tabPage1.Controls)
                {
                    if (c != trackBar1 && c.GetType() != typeof(Button) && (c.GetType() != typeof(ListBox)))
                    {
                        c.Enabled = enable;
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    linepollutant[i].Visible = enable;
                    labelpollutant[i].Visible = !enable;
                }
                // enable switch to show emission rate in kg/h/km or kg/h
                checkBox3.Enabled = true; 
            }
            Exit.Enabled = true;
            panel1.Enabled = true;
        }

        private void RedrawDomain(object sender, EventArgs e)
        {
            // send Message to domain Form, that redraw is necessary
            try
            {
                if (LinesourceRedraw != null)
                {
                    LinesourceRedraw(this, e);
                }
            }
            catch
            { }
        }

        public void ShowForm()
        {
            ItemDisplayNr = trackBar1.Value - 1;
            RedrawDomain(this, null);
        }


        void TextBox2Click(object sender, EventArgs e)
        {
            {
                try
                {
                    int number_of_vertices = Convert.ToInt32(textBox2.Text);
                    if (number_of_vertices < 2)
                    {
                        throw new System.InvalidOperationException("Nothing digitized");
                    }

                    VerticesEditDialog vert;

                    if (checkBox2.Checked) // 3D Lines
                    {
                        vert = new VerticesEditDialog(number_of_vertices, ref CornerLineX, ref CornerLineY, ref CornerLineZ)
                        {
                            StartPosition = FormStartPosition.Manual
                        };
                    }
                    else
                    {
                        vert = new VerticesEditDialog(number_of_vertices, ref CornerLineX, ref CornerLineY)
                        {
                            StartPosition = FormStartPosition.Manual
                        };
                    }

                    if (Right < SystemInformation.PrimaryMonitorSize.Width / 2)
                    {
                        vert.Location = new Point(St_F.GetScreenAtMousePosition() + Right + 4, Top);
                    }
                    else
                    {
                        vert.Location = new Point(St_F.GetScreenAtMousePosition() + Left - 250, Top);
                    }

                    vert.Vertices_redraw += new ForceDomainRedraw(RedrawDomain);
                    if (vert.ShowDialog() == DialogResult.OK)
                    {
                        int cornerCount = vert.Lines;
                        SetNumberOfVerticesText(cornerCount.ToString());
                    }

                    SaveArray(true);
                    vert.Dispose();
                }
                catch
                {
                    MessageBox.Show(this, "Nothing digitized", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void edit_deposition(object sender, EventArgs e)
        {
            int nr = 0;
            for (int i = 0; i < 10; i++)
            {
                if (sender == but1[i])
                {
                    nr = i;
                }
            }

            using (EditDeposition edit = new EditDeposition
            {
                StartPosition = FormStartPosition.Manual
            })
            {
                if (Right < SystemInformation.PrimaryMonitorSize.Width / 2)
                {
                    edit.Location = new Point(St_F.GetScreenAtMousePosition() + Right + 4, Top);
                }
                else
                {
                    edit.Location = new Point(St_F.GetScreenAtMousePosition() + Left - 370, Top);
                }
                edit.Dep = dep[nr]; // set actual values
                edit.Emission = St_F.TxtToDbl(lineemission[nr].Text, true);
                edit.Pollutant = linepollutant[nr].SelectedIndex;
                if (edit.ShowDialog() == DialogResult.OK)
                {
                    edit.Hide();
                }
            }
            if (Main.Project_Locked == false)
            {
                if (dep[nr].V_Dep1 > 0 || dep[nr].V_Dep2 > 0 || dep[nr].V_Dep3 > 0)
                {
                    but1[nr].BackColor = Color.LightGreen; // mark that deposition is set
                }
                else
                {
                    but1[nr].BackColor = SystemColors.ButtonFace;
                }

                SaveArray(true); // save values
            }
        }

        void ListBox2SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string sg_name = string.Empty;
                if (listBox2.Items.Count > 0)
                {
                    sg_name = listBox2.Items[0].ToString();
                }
                if (listBox2.SelectedIndex >= 0 || listBox2.Items.Count == 1) // an entry is selected or only one entry
                {
                    if (listBox2.SelectedIndex >= 0)
                    {
                        sg_name = listBox2.SelectedItem.ToString();
                    }
                    else if (listBox2.Items.Count == 1)
                    {
                        sg_name = listBox2.Items[0].ToString();
                    }
                }

                if (listBox2.Items.Count >= 1)
                {
                    if (Main.Project_Locked == false)
                    {
                        button4.Enabled = true; // enable save source group button
                        if (listBox2.Items.Count > 1)
                        {
                            button5.Enabled = true; // enable delete source group button
                        }
                    }
                }
                else
                {
                    button4.Enabled = false; // disable save source group button
                    button5.Enabled = false; // disable delete source group button
                }

                if (sg_name.Length > 0)
                {
                    int sg_nr = St_F.GetSgNumber(sg_name);
                    int index = GetPollutantListIndex(sg_nr);

                    // block change of pollutants
                    if (listBox2.SelectedIndex > 0 || Main.Project_Locked == true)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            linepollutant[i].Visible = false;
                            labelpollutant[i].Visible = true;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            linepollutant[i].Visible = true;
                            labelpollutant[i].Visible = false;
                        }
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        if (index < 0)
                        {
                            linepollutant[i].SelectedIndex = 0;
                            labelpollutant[i].Text = linepollutant[i].Text;
                            lineemission[i].Text = "0";
                        }
                        else
                        {
                            if (listBox2.SelectedIndex > 0) // set pollutant from first Source group
                            {
                                linepollutant[i].SelectedIndex = SourceGroupEmission[0].Pollutant[i];
                            }
                            else // set saved pollutant (compatibility to old projects)
                            {
                                linepollutant[i].SelectedIndex = SourceGroupEmission[index].Pollutant[i];
                            }

                            labelpollutant[i].Text = linepollutant[i].Text;
                            double lenght = 1;
                            if (checkBox3.Checked) // use kg/h as input and recalculate to kg/h/km
                            {
                                lenght = Convert.ToDouble(label15.Text) / 1000;
                            }
                            lineemission[i].Text = (SourceGroupEmission[index].EmissionRate[i] * lenght).ToString();
                        }
                    }
                }
            }
            catch
            { }
        }

        // remove a SG - uncheck the checkbox
        void Button5Click(object sender, EventArgs e)
        {
            // reset emission of this sg
            if (listBox2.SelectedIndex >= 0)
            {
                string sg_name = listBox2.SelectedItem.ToString();
                int sg_nr = St_F.GetSgNumber(sg_name);
                int indexold = GetPollutantListIndex(sg_nr);

                SourceGroupEmission.RemoveAt(indexold);
                listBox2.Items.RemoveAt(listBox2.SelectedIndex);
            }
            SaveArray(true);
            // fill in new values
        }

        void CheckBox1CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                label5.BackColor = Color.Yellow;
            }
            else
            {
                label5.BackColor = Color.Transparent;
            }
        }

        private void EditLinesources_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.ApplicationExitCall || e.CloseReason == CloseReason.WindowsShutDown)
            {
                // allow Closing in this cases
                e.Cancel = false;
            }
            else
            {
                if (((sender as Form).ActiveControl is Button) == false)
                {
                    // cancel if x has been pressed = restore old values!
                    cancelButtonClick(null, null);
                }
                // Hide form and send message to caller when user tries to close this form
                if (!AllowClosing)
                {
                    this.Hide();
                    e.Cancel = true;
                }
            }
            GralDomain.Domain.MarkerPoint.X = 0;
            GralDomain.Domain.MarkerPoint.Y = 0;
        }

        void EditlinesourcesFormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (TextBox tex in lineemission)
            {
                tex.TextChanged -= new System.EventHandler(St_F.CheckInput);
            }

            foreach (Button but in but1)
            {
                but.Click -= new EventHandler(edit_deposition);
            }

            textBox1.KeyPress -= new KeyPressEventHandler(Comma1); //only point as decimal seperator is allowed

            CornerLineX = null;
            CornerLineY = null;
            ItemData.Clear();
            ItemData.TrimExcess();
            SourceGroupEmission.Clear();
            SourceGroupEmission.TrimExcess();

            for (int nr = 0; nr < 10; nr++)
            {
                linepollutant[nr].SelectedValueChanged -= new System.EventHandler(Odour);
                linepollutant[nr].Items.Clear();
                lineemission[nr].KeyPress -= new KeyPressEventHandler(St_F.NumericInput);
                linepollutant[nr].Dispose();
                labelpollutant[nr].Dispose();
            }

            listBox2.Items.Clear();
            SG_List.Clear();
            SG_List.TrimExcess();

            toolTip1.Dispose();
        }

        /// <summary>
        /// Select a source group
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button7Click(object sender, EventArgs e)
        {
            EditSelectSourcegroup sgsel = new EditSelectSourcegroup
            {
                StartPosition = FormStartPosition.Manual
            };
            if (Right < SystemInformation.PrimaryMonitorSize.Width / 2)
            {
                sgsel.Location = new Point(St_F.GetScreenAtMousePosition() + Right + 4, Top);
            }
            else
            {
                sgsel.Location = new Point(St_F.GetScreenAtMousePosition() + Left - 460, Top);
            }

            sgsel.SourceGroup = SG_List;
            sgsel.CopyFrom = new List<string>();
            foreach (var text in listBox2.Items)
            {
                sgsel.CopyFrom.Add(text.ToString());
            }

            if (sgsel.ShowDialog() == DialogResult.OK)
            {
                if (sgsel.ShiftCopy == 0) // add new sg
                {
                    int new_sg = St_F.GetSgNumber(sgsel.SelectedSourceGroup);
                    PollutantsData _poll = new PollutantsData
                    {
                        SourceGroup = new_sg
                    };
                    SourceGroupEmission.Add(_poll);
                    listBox2.Items.Add(sgsel.SelectedSourceGroup);
                    listBox2.SelectedIndex = listBox2.Items.Count - 1;
                }
                else if (sgsel.ShiftCopy == 1 && sgsel.SelCopyFrom.Length > 0) // copy a Source group
                {
                    int new_sg = St_F.GetSgNumber(sgsel.SelectedSourceGroup);
                    int old_sg = St_F.GetSgNumber(sgsel.SelCopyFrom);

                    int indexold = GetPollutantListIndex(old_sg);

                    PollutantsData _poll = new PollutantsData(SourceGroupEmission[indexold])
                    {
                        SourceGroup = new_sg
                    }; // copy from old to new sourcegroup

                    SourceGroupEmission.Add(_poll);
                    listBox2.Items.Add(sgsel.SelectedSourceGroup);
                    SaveArray(true);
                    //FillValues();
                    listBox2.SelectedIndex = listBox2.Items.Count - 1;
                }
                else if (sgsel.ShiftCopy == 2 && sgsel.SelCopyFrom.Length > 0) // Shift a Source group?
                {
                    if (MessageBox.Show(this, "Move source group " + sgsel.SelCopyFrom +
                                        "\n to " + sgsel.SelectedSourceGroup + " ?", "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        int new_sg = St_F.GetSgNumber(sgsel.SelectedSourceGroup);
                        int old_sg = St_F.GetSgNumber(sgsel.SelCopyFrom);

                        int indexold = GetPollutantListIndex(old_sg);
                        SourceGroupEmission[indexold].SourceGroup = new_sg; // set new source group

                        for (int i = 0; i < listBox2.Items.Count; i++)
                        {
                            if (St_F.GetSgNumber(listBox2.Items[i].ToString()) == old_sg)
                            {
                                listBox2.Items.RemoveAt(i);
                            }
                        }
                        listBox2.Items.Add(sgsel.SelectedSourceGroup);
                        SaveArray(true);
                        listBox2.SelectedIndex = listBox2.Items.Count - 1;
                        //FillValues();
                    }
                } //shift a source group

            }
            sgsel.Dispose();
        }

        /// <summary>
        /// Set the trackbar to the desired item number
        /// </summary> 
        public bool SetTrackBar(int _nr)
        {
            if (_nr >= trackBar1.Minimum && _nr <= trackBar1.Maximum)
            {
                trackBar1.Value = _nr;
                return true;
            }
            else
            {
                trackBar1.Value = trackBar1.Minimum;
                return false;
            }
        }

        /// <summary>
        /// Set the trackbar maximum to the maximum count in the item list
        /// </summary> 
        public void SetTrackBarMaximum()
        {
            trackBar1.Minimum = 1;
            trackBar1.Maximum = Math.Max(ItemData.Count, 1);
        }

        public string GetSelectedListBox1Item()
        {
            return Convert.ToString(listBox1.SelectedItem);
        }

        public string GetNumberOfVerticesText()
        {
            return textBox2.Text;
        }
        public void SetNumberOfVerticesText(string _s)
        {
            textBox2.Text = _s;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            int edge = Math.Max(0, trackBar2.Value - 1);

            // absolut height allowed at point 0
            if (trackBar2.Value > 1 || Main.Project_Locked == true)
            {
                checkBox1.Enabled = false;
            }
            else
            {
                checkBox1.Enabled = true;
            }

            if (edge < Convert.ToInt32(textBox2.Text))
            {
                numericUpDown1.Value = (decimal)Math.Abs(CornerLineZ[edge]);

                Domain.MarkerPoint.X = CornerLineX[edge];
                Domain.MarkerPoint.Y = CornerLineY[edge];

                if (checkBox1.Checked)
                {
                    label5.BackColor = Color.Yellow;
                }
                else
                {
                    label5.BackColor = Color.Transparent;
                }

                RedrawDomain(this, e);
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                trackBar2.Enabled = true;
            }
            else
            {
                trackBar2.Value = 1;
                trackBar2.Enabled = false;             
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            {
                get_edgepoint_height(); // get recent edgepoint height
                vertices = Convert.ToInt32(textBox2.Text);
                if (vertices <= 1)
                {
                    return; // not enough vertices
                }

                trackBar2.Maximum = vertices;
            }
        }
        private void get_edgepoint_height()
        {
            int edge = Math.Max(0, trackBar2.Value - 1);
            if (edge < Convert.ToInt32(textBox2.Text))
            {
                float height = 0;
                if (checkBox1.Checked == true) // absolute height
                {
                    height = (float)numericUpDown1.Value * (-1);
                }
                else
                {
                    height = (float)numericUpDown1.Value;
                }

                CornerLineZ[edge] = height;
                //MessageBox.Show(edge.ToString());
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            get_edgepoint_height(); // get recent edgepoint height	
        }

        /// <summary>
        /// OK Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            button4_Click(sender, e);
            FillValues();
            // send Message to domain Form, that OK button has been pressed
            try
            {
                if (ItemFormOK != null)
                {
                    ItemFormOK(this, e);
                }
            }
            catch
            { }
        }
        /// <summary>
        /// Cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButtonClick(object sender, EventArgs e)
        {
            ReloadItemData();
            // send Message to domain Form, that Cancel button has been pressed
            try
            {
                if (ItemFormCancel != null)
                {
                    ItemFormCancel(this, e);
                }
            }
            catch
            { }
        }
        /// <summary>
        /// Reset item data when cancelling the dialog
        /// </summary>
        public void ReloadItemData()
        {
            ItemData.Clear();
            ItemData.TrimExcess();
            LineSourceDataIO _ls = new LineSourceDataIO();
            string _file = Path.Combine(Gral.Main.ProjectName, "Emissions", "Lsources.txt");
            _ls.LoadLineSources(ItemData, _file);
            _ls = null;
            SetTrackBarMaximum();
            FillValues();
        }

        /// <summary>
        /// Show and edit emission rate in kg/km/h or kg/h
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            //lenght in km
            double lenght = Convert.ToDouble(label15.Text) /1000;
            
            // kg/h
            if (checkBox3.Checked)
            {
                for (int nr = 0; nr < but1.Length; nr++)
                {
                    double _val = St_F.TxtToDbl(lineemission[nr].Text, false);
                    lineemission[nr].Text = Convert.ToString(_val * lenght);
                }

            }
            else if (lenght > 0)
            {
                for (int nr = 0; nr < but1.Length; nr++)
                {
                    double _val = St_F.TxtToDbl(lineemission[nr].Text, false);
                    lineemission[nr].Text = Convert.ToString(_val / lenght);
                }
            }
            //update the unit of each pollutant
            Odour(sender, e);
        }

        /// <summary>
        /// Check for lenght > 0 and lock unit switch kg/km/h 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label15_TextChanged(object sender, EventArgs e)
        {
            double lenght = 0;
            double.TryParse(label15.Text, out lenght);
            if (lenght > 0)
            {
                checkBox3.Enabled = true;
            }
            else
            {
                checkBox3.Enabled = false;
                checkBox3.Checked = false;
            }
        }

        /// <summary>
        /// Use panel1 to move the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            const int WM_NCLBUTTONDOWN = 0x00A1;
            const int HTCAPTION = 2;
            panel1.Capture = false;
            labelTitle.Capture = false;
            Message msg = Message.Create(this.Handle, WM_NCLBUTTONDOWN, new IntPtr(HTCAPTION), IntPtr.Zero);
            this.DefWndProc(ref msg);
        }

        private void ScrollRight_Click(object sender, EventArgs e)
        {
            if (trackBar1.Value < trackBar1.Maximum)
            {
                trackBar1.Value++;
                trackBar1_Scroll(null, null);
            }
        }

        private void ScrollLeft_Click(object sender, EventArgs e)
        {
            if (trackBar1.Value > trackBar1.Minimum)
            {
                trackBar1.Value--;
                trackBar1_Scroll(null, null);
            }
        }
    }
}