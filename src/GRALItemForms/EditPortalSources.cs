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
using GralDomain;
using GralItemData;
using GralStaticFunctions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GralItemForms
{
    public partial class EditPortalSources : Form
    {
        private TextBox [] portalemission = new TextBox [10];       //textboxes for emission strenght input of portal sources
        private ComboBox [] portalpollutant = new ComboBox [10];    //ComboBoxes for selecting pollutants of portal sources
        private Label [] labelpollutant = new Label [10];           //labels for pollutant types
        private Button [] but1 = new Button [10];                   //Buttons for units
        /// <summary>
        /// Collection of all portal source data
        /// </summary>
        public List<PortalsData> ItemData = new List<PortalsData>(); 
        private List<PollutantsData> SourceGroupEmission = new List<PollutantsData>(); // emission data for various source group
        /// <summary>
        /// Number of actual portal source to be displayed
        /// </summary>
        public int ItemDisplayNr;
        /// <summary>
        /// X corner points of a portal source in meters
        /// </summary>
        public double [] CornerPortalX = new double [2];            //x corner points of a portal source in meters
        /// <summary>
        /// Y corner points of a portal source in meters
        /// </summary>
        public double [] CornerPortalY = new double [2];            //y corner points of a portal source in meters
        private string [] emlinearr = new string [100];             //splitted emission data for various source groups
        public List<string> SG_List = new List<string>();			//All source group names
        private string [] defsourcegroup = new string [2];          //defined sourcegroups

        private Deposition [] dep = new Deposition [10];
        private CultureInfo ic = CultureInfo.InvariantCulture;
        private int Dialog_Initial_Width = 0;
        private int Button1_Width = 50;
        private int Button1_Height = 10;
        private int TabControl1_Height = 520;
        private int TBWidth = 80;
        private int TextBox_x0 = 0;
        private int TrackBar_x0 = 0;
        private int Numericupdown_x0 = 0;
        private int CheckedListbox_x0 = 0;
        private int TabControl_x0 = 0;
        private string TempTimeSeries = string.Empty;
        private string VelTimeSeries = string.Empty;

        // delegate to send a message, that redraw is needed!
        public event ForceDomainRedraw PortalSourceRedraw;

        public bool AllowClosing = false;
        //delegates to send a message, that user uses OK or Cancel button
        public event ForceItemOK ItemFormOK;
        public event ForceItemCancel ItemFormCancel;

        public EditPortalSources ()
        {
            InitializeComponent ();
            
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
                Text = "[MOU/h]"
            };
            Button1_Width = bt_test.Width;
            Button1_Height = bt_test.Height;
            bt_test.Dispose();
            
            y_act = 10;

            for (int i = 0; i < 10; i++)
            {
                createTextbox(6, Convert.ToInt32(y_act) + Convert.ToInt32(i * (TBHeight + 7)), TBWidth, TBHeight, i);
                dep [i] = new Deposition (); // initialize Deposition array
                dep [i].init ();
            }
            
            listBox1.Items.AddRange(Main.NemoTrafficSituations);
            textBox1.KeyPress += new KeyPressEventHandler (Comma1); //only point as decimal seperator is allowed
        }

        private void Editportalsources_Load (object sender, EventArgs e)
        {
            #if __MonoCS__
            int bw = this.Height - this.ClientSize.Height + SystemInformation.HorizontalScrollBarHeight;
            listBox1.Height = 25;
            trackBar1.Height = 12;
            #endif
            
            //fill listboxes with default pollutants
            for (int nr = 0; nr < 10; nr++)
            {
                for (int i = 0; i < Main.PollutantList.Count; i++)
                {
                    portalpollutant[nr].Items.Add(Main.PollutantList[i]);
                }
            }
            
            tabControl1.Height = Math.Max(350, portalemission[9].Bottom + 5);
            tabControl1.Width = Math.Max(273, but1[1].Right + 4);
            Dialog_Initial_Width = ClientSize.Width;
            TrackBar_x0 = trackBar1.Left;
            TextBox_x0 = textBox1.Left;
            Numericupdown_x0 = numericUpDown1.Left;
            CheckedListbox_x0 = listBox2.Left;
            TabControl_x0 = tabControl1.Left;
            
            FillValues();
            
            ClientSize = new Size(Math.Max(textBox2.Right + 5, but1[1].Right + 5), tabControl1.Bottom + 5);
            TabControl1_Height = tabControl1.Height;
            EditportalsourcesResizeEnd(null, null);
        }

        private void createTextbox (int x0, int y0, int b, int h, int nr)
        {
            //list box for selcting pollutants
            portalpollutant[nr] = new ComboBox
            {
                Location = new System.Drawing.Point(x0, y0),
                Font = textBox2.Font, //new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                Height = textBox2.Height,
                Width = 80,
                DropDownHeight = Math.Max(textBox2.Height * 4, tabPage1.Height - y0)
            };
            tabPage1.Controls.Add(portalpollutant[nr]);
            portalpollutant[nr].SelectedValueChanged += new System.EventHandler(Odour);

            //text box for input of emission rates
            portalemission[nr] = new TextBox
            {
                Location = new System.Drawing.Point(portalpollutant[nr].Left + portalpollutant[nr].Width + 5, y0),
                Size = new System.Drawing.Size(b - 20, h),
                TextAlign = System.Windows.Forms.HorizontalAlignment.Right,
                Text = "0"
            };
            portalemission[nr].TextChanged += new System.EventHandler(St_F.CheckInput);
            portalemission[nr].KeyPress += new KeyPressEventHandler(St_F.NumericInput);
            tabPage1.Controls.Add(portalemission[nr]);

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

            //labels
            but1[nr] = new Button
            {
                Location = new System.Drawing.Point(portalemission[nr].Left + portalemission[nr].Width + 5, y0 - 1),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Height = portalemission[0].Height,
                Width = Button1_Width,
                Text = "[kg/h]"
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

        private void Comma(object sender, KeyPressEventArgs e)
        {
            // if (e.KeyChar == ',') e.KeyChar = '.';
            int asc = (int)e.KeyChar; //get ASCII code
            switch (asc)
            {
                    case 8: break;
                    case 44: break;
                    case 46: break;
                    case 48: break;
                    case 49: break;
                    case 50: break;
                    case 51: break;
                    case 52: break;
                    case 53: break;
                    case 54: break;
                    case 55: break;
                    case 56: break;
                    case 57: break;
                    case 69: break; // E
                    case 45: break; // -
                default:
                    e.Handled = true; break;
            }
        }

        private void Odour(object sender, EventArgs args)
        {
            for (int nr = 0; nr < 10; nr++)
            {
                if (portalpollutant[nr].SelectedIndex == 2)
                {
                    but1[nr].Text = "[MOU/h]";
                }
                else
                {
                    but1[nr].Text = "[kg/h]";
                }
            }
        }
        
        private void Comma1(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',')
            {
                e.KeyChar = '.';
            }

            int asc = (int)e.KeyChar; //get ASCII code
        }
        
        //increase the number of portal sources by one
        private void Button1Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                SaveArray(false);

                trackBar1.Maximum += 1;
                trackBar1.Value = trackBar1.Maximum;
                ItemDisplayNr = trackBar1.Maximum - 1;
                RedrawDomain(this, null);
            }
        }

        //scroll between the portal sources
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
            float height = Convert.ToSingle(numericUpDown1.Value);
            int crosssection = Convert.ToInt32(height * Math.Sqrt(Math.Pow(CornerPortalX[0] - CornerPortalX[1], 2) + Math.Pow(CornerPortalY[0] - CornerPortalY[1], 2)));
                
            if (listBox2.Items.Count == 0) // no source group added
            {
                if (crosssection > 0.1 && textBox1.Text.Length > 1)
                {
                    MessageBox.Show(this, "No source group added", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }
            if (crosssection < 0.01)
            {
                if (ItemData.Count > 0)
                {
                    MessageBox.Show(this, "No edge points digitized", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }
            
            PortalsData _ps;
            if (ItemDisplayNr >= ItemData.Count) // new item
            {
                _ps = new PortalsData();
            }
            else // change existing item
            {
                _ps = ItemData[ItemDisplayNr];
            }
            
            if (textBox2.Text != "")
            {
                // collect pollutants
                _ps.Poll.Clear();
                _ps.Poll.TrimExcess();
                if (SourceGroupEmission.Count == 0) // a source group is available, but Save SG not pressed
                {
                    UpdateNewSourcegroupEmission();
                }
                
                foreach(PollutantsData _pd in SourceGroupEmission)
                {
                    _ps.Poll.Add(_pd);
                }
                
                for (int i = 0; i < 10; i++) // save deposition
                {
                    _ps.GetDep()[i] = new Deposition(dep[i]);
                }
                
                string tunneldirection = "1";      //indicates whether the tunnel is bi- or uni-directional
                if(radioButton1.Checked==true)
                {
                    tunneldirection ="1";
                }

                if (radioButton2.Checked==true)
                {
                    tunneldirection ="2";
                }

                _ps.Pt1 = new PointD(CornerPortalX[0], CornerPortalY[0]);
                _ps.Pt2 = new PointD(CornerPortalX[1], CornerPortalY[1]);
                
                listBox1.SelectedIndex = listBox1.TopIndex; // NEMO
                _ps.Name = textBox1.Text;
                _ps.Height = height;
                
                _ps.BaseHeight = Convert.ToSingle(numericUpDown5.Value);
                if (checkBox1.Checked) // absolute height over sea
                {
                    _ps.BaseHeight *= -1;
                }

                _ps.ExitVel = Convert.ToSingle(numericUpDown2.Value);
                _ps.DeltaT = Convert.ToSingle(numericUpDown3.Value);
                _ps.Direction = tunneldirection;
                _ps.Nemo.AvDailyTraffic = Convert.ToInt32(numericUpDown6.Value);
                _ps.Nemo.ShareHDV = Convert.ToSingle(numericUpDown7.Value);
                _ps.Nemo.Slope = Convert.ToSingle(numericUpDown4.Value);
                _ps.Nemo.TrafficSit = listBox1.SelectedIndex;
                _ps.Nemo.BaseYear = Convert.ToInt32(numericUpDown8.Value);
                _ps.TemperatureTimeSeries = TempTimeSeries;
                _ps.VelocityTimeSeries = VelTimeSeries;
                
                if (ItemDisplayNr >= ItemData.Count) // new item
                {
                    ItemData.Add(_ps);
                }
                else
                {
                    ItemData[ItemDisplayNr] = _ps;
                }
                if (redraw)
                {
                    RedrawDomain(this, null);
                }
            }
            else
            {
                MessageBox.Show (this, "Digitize edge points, please", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                PortalsData _pdata;
                if (ItemDisplayNr < ItemData.Count)
                {
                    _pdata = ItemData[ItemDisplayNr];
                }
                else
                {
                    if (ItemData.Count > 0)
                    {
                        _pdata = new PortalsData(ItemData[ItemData.Count - 1]);
                    }
                    else // 1st entry
                    {
                        _pdata = new PortalsData();
                        _pdata.Poll.Add(new PollutantsData(1));
                        _pdata.Name = "Portal";
                        _pdata.Height = 5;
                        
                        for (int i = 0; i < 10; i++)
                        {
                            if (portalpollutant[i].SelectedIndex < 0 && portalpollutant[i].Items.Count > 0)
                            {
                                portalpollutant[i].SelectedIndex = 0;
                            }
                        }
                    }
                }
                
                listBox2.Items.Clear(); // clear Source groups
                
                textBox1.Text = _pdata.Name;    // name
                
                double height = Math.Round(_pdata.Height, 1);
                
                if (_pdata.BaseHeight >= 0) // height above ground
                {
                    checkBox1.Checked = false;
                }
                else // height above sea
                {
                    checkBox1.Checked = true;
                }
                numericUpDown5.Value = St_F.ValueSpan(0, 7999, Math.Abs(_pdata.BaseHeight));	
                
                numericUpDown1.Value = St_F.ValueSpan(0, 7999, (double) Math.Abs(height));
                numericUpDown2.Value = St_F.ValueSpan(0, 100, (double) Math.Abs(_pdata.ExitVel));
                numericUpDown3.Value = St_F.ValueSpan(-499, 499, _pdata.DeltaT);
                if (_pdata.Direction == "1")
                {
                    radioButton1.Checked = true;
                }
                else
                {
                    radioButton2.Checked = true;
                }

                numericUpDown6.Value = St_F.ValueSpan(0, 1000000, _pdata.Nemo.AvDailyTraffic);
                numericUpDown7.Value = St_F.ValueSpan(-0.1, 100, _pdata.Nemo.ShareHDV);
                numericUpDown4.Value = St_F.ValueSpan(-10, 10, _pdata.Nemo.Slope);
                listBox1.SelectedIndex = _pdata.Nemo.TrafficSit;
                numericUpDown8.Value = St_F.ValueSpan(1990, 2200, _pdata.Nemo.BaseYear);
                
                SourceGroupEmission.Clear();
                SourceGroupEmission.TrimExcess();
                if (_pdata.Poll != null)
                {
                    foreach(PollutantsData _poll in _pdata.Poll)
                    {
                        SourceGroupEmission.Add(new PollutantsData(_poll));
                        listBox2.Items.Add(SG_List[_poll.SourceGroup - 1]);
                    }
                }
                if (listBox2.Items.Count > 0)
                {
                    listBox2.SelectedIndex = 0;
                }
                ListBox2SelectedIndexChanged(null, null); // fill values

                CornerPortalX[0] = _pdata.Pt1.X;
                CornerPortalY[0] = _pdata.Pt1.Y;
                CornerPortalX[1] = _pdata.Pt2.X;
                CornerPortalY[1] = _pdata.Pt2.Y;
                
                int crosssection = Convert.ToInt32(height * Math.Sqrt(Math.Pow(CornerPortalX[0] - CornerPortalX[1], 2) + Math.Pow(CornerPortalY[0] - CornerPortalY[1], 2)));
                textBox2.Text = crosssection.ToString();
                
                Domain.EditSourceShape = false;  // block input of new vertices
                
                for (int i = 0; i < 10; i++)
                {
                    dep[i] = _pdata.GetDep()[i];
                    
                    if (dep[i].V_Dep1 > 0 || dep[i].V_Dep2 > 0 || dep[i].V_Dep3 > 0)
                    {
                        but1[i].BackColor = Color.LightGreen; // mark that deposition is set
                    }
                    else
                    {
                        but1[i].BackColor = SystemColors.ButtonFace; // mark that deposition is reset
                    }
                }
                
                TempTimeSeries = _pdata.TemperatureTimeSeries;
                if (!string.IsNullOrEmpty(TempTimeSeries))
                {
                    button8.BackgroundImage = Gral.Properties.Resources.TimeSeriesSelected;
                }
                else
                {
                    button8.BackgroundImage = Gral.Properties.Resources.TimeSeries;
                }
                
                VelTimeSeries = _pdata.VelocityTimeSeries;
                if (!string.IsNullOrEmpty(VelTimeSeries))
                {
                    button9.BackgroundImage = Gral.Properties.Resources.TimeSeriesSelected;
                }
                else
                {
                    button9.BackgroundImage = Gral.Properties.Resources.TimeSeries;
                }
                
            }
            catch//*(Exception ex)*//
            {
                //MessageBox.Show(ex.Message);
                textBox1.Text = "Portal";
                numericUpDown1.Value = 5;
                
                SourceGroupEmission.Clear();
                SourceGroupEmission.TrimExcess();

                for (int i = 0; i < 10; i++)
                {
                    if (portalpollutant[i].Items.Count > 0)
                    {
                        portalpollutant[i].SelectedIndex = 0;
                    }
                    portalemission[i].Text = "0";
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

        //remove actual portal source data
        private void Button2Click(object sender, EventArgs e)
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
                if (St_F.InputBoxYesNo("Attention", "Do you really want to delete this source?", St_F.GetScreenAtMousePosition() + 340, GralStaticFunctions.St_F.GetTopScreenAtMousePosition() + 150) == DialogResult.Yes)
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
                numericUpDown1.Value = 5;
                numericUpDown2.Value = 0;
                numericUpDown3.Value = 0;
                textBox2.Text = "0";
                numericUpDown6.Value = 1000;
                numericUpDown7.Value = Convert.ToDecimal(-0.1);
                numericUpDown4.Value = 0;
                textBox3.Text = "0";
                listBox1.TopIndex = 0;
                listBox1.SelectedIndex = listBox1.TopIndex;
                
                for (int i = 0; i < 10; i++)
                {
                    portalemission[i].Text = "0";
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

        //remove all portal sources
        private void Button3Click(object sender, EventArgs e)
        {
            if (St_F.InputBox("Attention", "Delete all portal sources??", this) == DialogResult.OK)
            {
                SourceGroupEmission.Clear();
                SourceGroupEmission.TrimExcess();
                
                ItemData.Clear();
                ItemData.TrimExcess();
                
                listBox2.Items.Clear();
                
                textBox1.Text = "";
                numericUpDown1.Value = 5;
                numericUpDown2.Value = 0;
                numericUpDown3.Value = 0;
                textBox2.Text = "0";
                numericUpDown6.Value = 1000;
                numericUpDown7.Value = Convert.ToDecimal(-0.1);
                numericUpDown4.Value = 0;
                textBox3.Text = "0";
                listBox1.TopIndex = 0;
                listBox1.SelectedIndex = listBox1.TopIndex;

                for (int i = 0; i < 10; i++)
                {
                    portalpollutant[i].SelectedIndex = 0;
                    portalemission[i].Text = "0";
                    dep[i].init();
                    but1[i].BackColor = SystemColors.ButtonFace;
                }
                
                trackBar1.Maximum = 1;
                trackBar1.Value = 1;
                ItemDisplayNr = trackBar1.Value - 1;
                Domain.EditSourceShape = true;  // allow input of new vertices
                RedrawDomain(this, null);
            }
        }

        //save source group
        private void Button4Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex < 0  && listBox2.Items.Count > 1) // no source group selected & more than 1 source groups available
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
                        SourceGroupEmission[index].Pollutant[i] = portalpollutant[i].SelectedIndex;
                        double _val = 0;
                        double.TryParse(portalemission[i].Text, out _val);
                        SourceGroupEmission[index].EmissionRate[i] = _val;
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
                if(SourceGroupEmission[i].SourceGroup == SourceGroupNumber)
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
                            double slpinput = Convert.ToDouble(numericUpDown4.Value);
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

        void EditportalsourcesResizeEnd(object sender, EventArgs e)
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
                textBox1.Width = dialog_width - TextBox_x0;
                textBox2.Width = dialog_width - Numericupdown_x0;
                numericUpDown1.Width = dialog_width - Numericupdown_x0;
                numericUpDown2.Width = dialog_width - Numericupdown_x0;
                numericUpDown3.Width = dialog_width - Numericupdown_x0;
                numericUpDown5.Width = dialog_width - Numericupdown_x0;
                
                groupBox3.Width = dialog_width - CheckedListbox_x0;
                listBox2.Width = groupBox3.Width - listBox2.Left - 4;
                tabControl1.Width = dialog_width - TabControl_x0;
                button5.Left = groupBox3.Width - 5 - button5.Width;
                button7.Left = groupBox3.Width / 2 - button7.Width / 2;
            }
            
            panel1.Width = ClientSize.Width;

            if (ClientSize.Height > (tabControl1.Top + TabControl1_Height))
            {
                tabControl1.Height = ClientSize.Height - tabControl1.Top;
            }
            
            int element_width = (tabPage1.Width - but1[1].Width - 20) / 2;
            if (element_width < TBWidth)
            {
                element_width = TBWidth;
            }

            button11.Left = Math.Max(100, this.Width - 103);

            for (int nr = 0; nr < 10; nr++)
            {
                portalpollutant[nr].Width  = (int) (element_width);
                labelpollutant[nr].Width = (int) (element_width);
                portalemission [nr].Location = new System.Drawing.Point (portalpollutant[nr].Left + portalpollutant[nr].Width + 5, portalpollutant[nr].Top);
                portalemission [nr].Width = (int) (element_width);
                but1[nr].Location = new System.Drawing.Point(portalemission[nr].Left + portalemission[nr].Width + 5, portalpollutant[nr].Top - 1);
            }
        }
        
        void EditportalsourcesVisibleChanged(object sender, EventArgs e) // save source data if form is hidden
        {
            if (!Visible)
            {
            }
            else // Enable/disable items
            {
                bool enable = !Main.Project_Locked;
                if (enable)
                {
                    labelTitle.Text = "Edit Portal Sources";
                }
                else
                {
                    labelTitle.Text = "Portal Settings (Project Locked)";
                }
                foreach (Control c in Controls)
                {
                    if (c != trackBar1 && c != groupBox3 && c != tabControl1 && c != ScrollRight && c != ScrollLeft)
                    {
                        c.Enabled = enable;
                    }
                }
                button4.Enabled = enable;
                button5.Enabled = enable;
                button7.Enabled = enable;
                
                foreach (Control c in tabControl1.Controls)
                {
                    if (c!= tabPage1)
                    {
                        c.Enabled = enable;
                    }
                }
                foreach (Control c in tabPage1.Controls)
                {
                    if (c != trackBar1 && c.GetType() != typeof(Button) && c.GetType() != typeof(ListBox))
                    {
                        c.Enabled = enable;
                    }
                }
                
                for (int i = 0; i < 10; i++)
                {
                    portalpollutant[i].Visible = enable;
                    labelpollutant[i].Visible = !enable;
                }
            }
            Exit.Enabled = true;
            panel1.Enabled = true;
        }
        
        private void RedrawDomain(object sender, EventArgs e)
        {
            // send Message to domain Form, that redraw is necessary
            try
            {
                if (PortalSourceRedraw != null)
                {
                    PortalSourceRedraw(this, e);
                }
            }
            catch
            {}
        }
        
        public void ShowForm()
        {
            ItemDisplayNr = trackBar1.Value - 1;
            RedrawDomain(this, null);
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
                    edit.Location = new Point(St_F.GetScreenAtMousePosition() + Right + 4, St_F.GetTopScreenAtMousePosition() + 150);
                }
                else
                {
                    edit.Location = new Point(St_F.GetScreenAtMousePosition() + Left - 370, St_F.GetTopScreenAtMousePosition() + 150);
                }
                edit.Dep = dep[nr]; // set actual values
                edit.Emission = St_F.TxtToDbl(portalemission[nr].Text, true);
                edit.Pollutant = portalpollutant[nr].SelectedIndex;
                
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
                            portalpollutant[i].Visible = false;
                            labelpollutant[i].Visible = true;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            portalpollutant[i].Visible = true;
                            labelpollutant[i].Visible = false;
                        }
                    }
                    
                    for (int i = 0; i < 10; i++)
                    {
                        if (index < 0)
                        {
                            portalpollutant[i].SelectedIndex = 0;
                            labelpollutant[i].Text = portalpollutant[i].Text;
                            portalemission[i].Text = "0";
                        }
                        else
                        {
                            if (listBox2.SelectedIndex > 0) // set pollutant from first Source group
                            {
                                portalpollutant[i].SelectedIndex = SourceGroupEmission[0].Pollutant[i];
                            }
                            else // set saved pollutant (compatibility to old projects)
                            {
                                portalpollutant[i].SelectedIndex = SourceGroupEmission[index].Pollutant[i];
                            }
                            
                            labelpollutant[i].Text = portalpollutant[i].Text;
                            portalemission[i].Text = SourceGroupEmission[index].EmissionRate[i].ToString();
                        }
                    }
                }
            }
            catch
            {}
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
                if (indexold >= 0)
                {
                    SourceGroupEmission.RemoveAt(indexold);
                    listBox2.Items.RemoveAt(listBox2.SelectedIndex);
                }
            }
            SaveArray(true);
            // fill in new values
        }

        private void EditPortalSources_FormClosing(object sender, FormClosingEventArgs e)
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
        }

        void EditportalsourcesFormClosed(object sender, FormClosedEventArgs e)
        {
            foreach(TextBox tex in portalemission)
            {
                tex.TextChanged -= new System.EventHandler(St_F.CheckInput);
                tex.KeyPress -= new KeyPressEventHandler(Comma); //only point as decimal seperator is allowed
            }
            foreach(Button but in but1)
            {
                but.Click -= new EventHandler(edit_deposition);
            }

            textBox1.KeyPress -= new KeyPressEventHandler(Comma1); //only point as decimal seperator is allowed
            textBox1.KeyPress -= new KeyPressEventHandler(Comma1); //only point as decimal seperator is allowed
            
            CornerPortalX = null;
            CornerPortalY = null;
            ItemData.Clear();
            ItemData.TrimExcess();
            SourceGroupEmission.Clear();
            SourceGroupEmission.TrimExcess();
            
            for (int nr = 0; nr < 10; nr++)
            {
                portalpollutant[nr].SelectedValueChanged -= new System.EventHandler(Odour);
                portalpollutant[nr].Items.Clear();
                portalemission[nr].KeyPress -= new KeyPressEventHandler(St_F.NumericInput);
                portalpollutant[nr].Dispose();
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
                sgsel.Location = new Point(St_F.GetScreenAtMousePosition() + Right + 4, St_F.GetTopScreenAtMousePosition() + 350);
            }
            else
            {
                sgsel.Location = new Point(St_F.GetScreenAtMousePosition() + Left - 460, St_F.GetTopScreenAtMousePosition() + 350);
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
                        
                        for(int i = 0; i < listBox2.Items.Count; i++)
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
        public string GetCrossSectionText()
        {
            return textBox2.Text;
        }
        public void SetCrossSectionText(string _s)
        {
            textBox2.Text = _s;
        }
        public double GetNumericUpDownHeightValue()
        {
            return Convert.ToDouble(numericUpDown1.Value);
        }
        
        // Time Series and Velocity
        void ButtonTSVelClick(object sender, EventArgs e)
        {
            bool Velocity = true;
            
            Button bt = sender as Button;
            if (bt.Name == "button8")
            {
                Velocity = false;
            }
            
            using (EditTimeSeriesValues edT = new EditTimeSeriesValues())
            {
                if (Velocity)
                {
                    edT.FilePath = Path.Combine(Main.ProjectName, @"Emissions", "TimeSeriesPortalSourceVel.tsd");
                    edT.InitValue = Convert.ToDouble(numericUpDown2.Value);
                    edT.SelectedTimeSeries = VelTimeSeries;
                    edT.TitleBarText = "Exit Velocity [m/s] - Time Series - GRAL Transient Mode";
                }
                else
                {
                    edT.FilePath = Path.Combine(Main.ProjectName, @"Emissions", "TimeSeriesPortalSourceTemp.tsd");
                    edT.InitValue = Convert.ToDouble(numericUpDown3.Value);
                    edT.SelectedTimeSeries = TempTimeSeries;
                    edT.NegativeNumbersAllowed = true;
                    edT.TitleBarText = "Exit Temperature Above or Below Ambient Temperature [K] - Time Series - GRAL Transient Mode";
                }
                edT.StartPosition = FormStartPosition.Manual;
                
                
                if (Right < SystemInformation.PrimaryMonitorSize.Width / 2)
                {
                    edT.Location = new Point(St_F.GetScreenAtMousePosition() + Right + 4, St_F.GetTopScreenAtMousePosition() + 150);
                }
                else
                {
                    edT.Location = new Point(St_F.GetScreenAtMousePosition() + Left - 750, St_F.GetTopScreenAtMousePosition() + 150 );
                }
                
                if (edT.ShowDialog() == DialogResult.OK)
                {
                    edT.Hide();
                    if (Velocity)
                    {
                        VelTimeSeries = edT.SelectedTimeSeries;
                        numericUpDown2.Value = Convert.ToDecimal(edT.MeanValue);
                        button9.BackgroundImage = Gral.Properties.Resources.TimeSeriesSelected;
                    }
                    else
                    {
                        TempTimeSeries = edT.SelectedTimeSeries;
                        numericUpDown3.Value = Convert.ToDecimal(edT.MeanValue);
                        button8.BackgroundImage = Gral.Properties.Resources.TimeSeriesSelected;
                    }
                    
                }
                else // cancel
                {
                    if (Velocity)
                    {
                        VelTimeSeries = string.Empty;
                        button9.BackgroundImage = Gral.Properties.Resources.TimeSeries;
                    }
                    else
                    {
                        TempTimeSeries = string.Empty;
                        button8.BackgroundImage = Gral.Properties.Resources.TimeSeries;
                    }
                }
            }
        }
        
        // switch between absolute and realtive Height
        void CheckBox1CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                label5.BackColor = Color.Yellow;
                label9.BackColor = Color.Yellow;
            }
            else
            {
                label5.BackColor = Color.Transparent;
                label9.BackColor = Color.Transparent;
            }
        }
        /// <summary>
        /// OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            Button4Click(sender, e);
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

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            double height = (double) (numericUpDown1.Value);
            textBox2.Text = Convert.ToInt32(height * Math.Sqrt(Math.Pow(CornerPortalX[0] - CornerPortalX[1], 2) + Math.Pow(CornerPortalY[0] - CornerPortalY[1], 2))).ToString();
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
            PortalsDataIO _pd = new PortalsDataIO();
            string _file = Path.Combine(Gral.Main.ProjectName, "Emissions", "Portalsources.txt");
            _pd.LoadPortalSources(ItemData, _file);
            _pd = null;
            SetTrackBarMaximum();
            FillValues();
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