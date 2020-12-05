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
using System.Windows.Forms;
using System.IO;

using Gral;
using GralItemData;
using GralStaticFunctions;
using GralDomain;

namespace GralItemForms
{
    public partial class EditAreaSources : Form
    {
        /// <summary>
        /// Collection of all area source data
        /// </summary>
        public List<AreaSourceData> ItemData = new List<AreaSourceData>();
        /// <summary>
        /// Number of actual area source to be displayed
        /// </summary>
        public int ItemDisplayNr;
        /// <summary>
        /// Recent number of an edited corner point
        /// </summary>
        public int CornerAreaCount = 0;
        /// <summary>
        /// X corner points of an area source in meters
        /// </summary>
        public double [] CornerAreaX = new double [1000];
        /// <summary>
        /// Y corner points of an area source in meters
        /// </summary>
        public double [] CornerAreaY = new double [1000];            
        private int sourcegroup = 1; 			                     //sourcegroup
        
        // delegate to send Message, that redraw is needed!
        public event ForceDomainRedraw AreaSourceRedraw;

        public bool AllowClosing = false;
        //delegates to send a message, that user uses OK or Cancel button
        public event ForceItemOK ItemFormOK;
        public event ForceItemCancel ItemFormCancel;

        private readonly TextBox [] areaemission = new TextBox [10];     //textboxes for emission strenght input of area sources
        private readonly ComboBox [] areapollutant = new ComboBox [10];  //ComboBoxes for selecting pollutants of area sources
        private readonly Label [] labelpollutant = new Label [10];       //labels for pollutant types
        private readonly Button [] but1 = new Button [10];               //Buttons for units
        
        private readonly Deposition [] dep = new Deposition [10];
        private readonly CultureInfo ic = CultureInfo.InvariantCulture;
        private int Dialog_Initial_Width = 0;
        private readonly int Button1_Width = 50;
        private readonly int Button1_Height = 10;
        private readonly int TBWidth = 80;
        private int TrackBar_x0 = 0;
        private int Numericupdown_x0 = 0;
        private int TabControl_x0 = 0;

        public EditAreaSources ()
        {
            InitializeComponent ();
            Domain.EditSourceShape = true;  // allow input of new vertices
            //User defined column seperator and decimal seperator
            #if __MonoCS__
            var allNumUpDowns = Main.GetAllControls<NumericUpDown> (this);
            foreach (NumericUpDown nu in allNumUpDowns) {
                nu.TextAlign = HorizontalAlignment.Left;
            }
            #endif

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
            
            int y_act = 25;
            
            for (int i = 0; i < 10; i++)
            {
                createTextbox(2, Convert.ToInt32(y_act) + Convert.ToInt32(i * (TBHeight + 7)), TBWidth, TBHeight, i);
                dep [i] = new Deposition (); // initialize Deposition array
                dep [i].init ();
            }
            
            for (int nr = 0; nr < 10; nr++)
            {
                for (int i = 0; i < Main.PollutantList.Count; i++)
                {
                    areapollutant[nr].Items.Add(Main.PollutantList[i]);
                }
            }

            textBox1.KeyPress += new KeyPressEventHandler (Comma1); //only point as decimal seperator is allowed
            comboBox1.KeyPress += new KeyPressEventHandler (Comma2); //only point as decimal seperator is allowed
        }

        private void Editareasources_Load (object sender, EventArgs e)
        {
            Dialog_Initial_Width = ClientSize.Width;
            TrackBar_x0 = trackBar1.Left;
            Numericupdown_x0 = numericUpDown1.Left;
            TabControl_x0 = groupBox1.Left;
            
            FillValues();
            
            ClientSize = new Size(Math.Max(textBox2.Right + 5, but1[1].Right + 5), groupBox1.Top + groupBox1.Height + 10);
            EditareasourcesResizeEnd(null, null);
        }

        private void createTextbox (int x0, int y0, int b, int h, int nr)
        {
            //list box for selcting pollutants
            areapollutant[nr] = new ComboBox
            {
                Location = new System.Drawing.Point(x0, y0),
                Font = textBox2.Font, //new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                Height = textBox2.Height,
                Width = 80,
                DropDownHeight = Math.Max(textBox2.Height * 4, groupBox1.Height - y0)
            };
            groupBox1.Controls.Add(areapollutant[nr]);
            areapollutant[nr].SelectedValueChanged += new System.EventHandler(Odour);

            labelpollutant[nr] = new Label
            {
                Location = new System.Drawing.Point(x0, y0),
                Font = textBox2.Font, //new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                Height = textBox2.Height,
                Width = 80,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };
            groupBox1.Controls.Add(labelpollutant[nr]);

            //text box for input of area emission rate
            areaemission[nr] = new TextBox
            {
                Location = new System.Drawing.Point(areapollutant[nr].Left + areapollutant[nr].Width + 5, y0),
                Size = new System.Drawing.Size(b - 20, h),
                TextAlign = System.Windows.Forms.HorizontalAlignment.Right,
                Text = "0"
            };
            areaemission[nr].TextChanged += new System.EventHandler(St_F.CheckInput);
            areaemission[nr].KeyPress += new KeyPressEventHandler(St_F.NumericInput);
            groupBox1.Controls.Add(areaemission[nr]);

            //labels
            but1[nr] = new Button
            {
                Location = new System.Drawing.Point(areaemission[nr].Left + areaemission[nr].Width + 5, y0 - 1),
                Height = areaemission[0].Height,
                Width = Button1_Width,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Text = "[kg/h]"
            };
            groupBox1.Controls.Add(but1[nr]);
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
                if (areapollutant[nr].SelectedIndex == 2)
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
            //if (e.KeyChar == ',') e.KeyChar = '.';
            int asc = (int)e.KeyChar; //get ASCII code
        }

        private void Comma2(object sender, KeyPressEventArgs e)
        {
            MessageBox.Show(this, "Use the drop down buttons to chose the source group", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            int asc = (int)e.KeyChar; //get ASCII code
            switch (asc)
            {
                default:
                    e.Handled = true; break;
            }
        }

        //increase the number of area sources by one
        private void button1_Click(object sender, EventArgs e)
        {
            if ((textBox1.Text != "") && (textBox2.Text != ""))
            {
                SaveArray();
                //textBox1.Text = "";
                textBox2.Text = "0";
                textBox2.Text = "";
                for (int i = 0; i < 10; i++)
                {
                    areaemission[i].Text = "0";
                }

                trackBar1.Maximum += 1;
                trackBar1.Value = trackBar1.Maximum;
                ItemDisplayNr = trackBar1.Maximum - 1;
                Domain.EditSourceShape = true;  // allow input of new vertices
            }
        }

        //scroll between the area sources
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            SaveArray();
            ItemDisplayNr = trackBar1.Value - 1;
            FillValues();
            RedrawDomain(this, null);
        }

        //save data in array list
        /// <summary>
        /// Saves the recent dialog data in the item object and the item list
        /// </summary> 
        public void SaveArray()
        {
            AreaSourceData _as; 
            if (ItemDisplayNr >= ItemData.Count) // new item
            {
                _as = new AreaSourceData();
            }
            else // change existing item
            {
                _as = ItemData[ItemDisplayNr];
            }
            
            if (textBox2.Text != "")
            {
                if ((textBox1.Text != "") && (Convert.ToInt32(textBox2.Text) > 2))
                {
                    // new area
                    int number_of_vertices = Convert.ToInt32(textBox2.Text);
                    
                    _as.Pt.Clear();
                    _as.Pt.TrimExcess();
                    for (int i = 0; i < number_of_vertices; i++)
                    {
                        _as.Pt.Add(new PointD(CornerAreaX[i], CornerAreaY[i]));
                    }
                    
                    double areapolygon = Math.Round(St_F.CalcArea(_as.Pt, false), 1);
                    textBox3.Text = St_F.DblToIvarTxt(areapolygon);
                    _as.Area = (float) areapolygon;
                    
                    if (number_of_vertices > 0)
                    {
                        Domain.EditSourceShape = false;  // block input of new vertices
                    }

                    for (int i = 0; i < 10; i++) // save pollutants
                    {
                        _as.Poll.Pollutant[i] = areapollutant[i].SelectedIndex;
                        _as.Poll.EmissionRate[i] = St_F.TxtToDbl(areaemission[i].Text, false);
                    }
                    _as.Poll.SourceGroup = sourcegroup;
                    
                    for (int i = 0; i < 10; i++) // save deposition
                    {
                        _as.GetDep()[i] = new Deposition(dep[i]);
                    }
                    
                    float height = (float) (numericUpDown1.Value);
                    
                    if (checkBox1.Checked) // absolute height over sea
                    {
                        height *= (-1);
                    }

                    _as.Height = height;
                    _as.VerticalExt = (float) (numericUpDown2.Value);
                    _as.RasterSize = (float) (numericUpDown4.Value);
                    _as.Name = textBox1.Text;
                    
                    if (ItemDisplayNr >= ItemData.Count) // new item
                    {
                        ItemData.Add(_as);
                    }
                    else
                    {
                        ItemData[ItemDisplayNr] = _as;
                    }
                    
                }
            }
            RedrawDomain(this, null);
        }
        

        //fill actual values
        /// <summary>
        /// Fills the dialog with data from the recent item object
        /// </summary> 
        public void FillValues()
        {
            try
            {
                AreaSourceData _as;
                if (ItemDisplayNr < ItemData.Count)
                {
                    _as = ItemData[ItemDisplayNr];
                }
                else
                {
                    if (ItemData.Count > 0)
                    {
                        _as = new AreaSourceData(ItemData[ItemData.Count - 1]);
                    }
                    else
                    {
                        _as = new AreaSourceData();
                    }
                    _as.Area = 0;
                }
                
                textBox1.Text = _as.Name;
                textBox2.Text = _as.Pt.Count.ToString();
                textBox3.Text = St_F.DblToLocTxt(Math.Round(_as.Area, 1)); // area
                
                decimal height = (decimal) (_as.Height);
                if (height >= 0) // height above ground
                {
                    checkBox1.Checked = false;
                }
                else // height above sea
                {
                    checkBox1.Checked = true;
                }

                numericUpDown1.Value = St_F.ValueSpan(0, 7999, (double) Math.Abs(height));
                
                numericUpDown2.Value = St_F.ValueSpan(0, 999, _as.VerticalExt);
                
                combo(_as.Poll.SourceGroup);
                sourcegroup = _as.Poll.SourceGroup;
                numericUpDown4.Value = St_F.ValueSpan(0.5, 100000, _as.RasterSize);
                
                for (int i = 0; i < 10; i++)
                {
                    areapollutant[i].SelectedIndex = _as.Poll.Pollutant[i];
                    if (areapollutant[i].SelectedIndex == 2)
                    {
                        but1[i].Text = "[MOU/h]";
                    }
                    else
                    {
                        but1[i].Text = "[kg/h]";
                    }

                    labelpollutant[i].Text = areapollutant[i].Text;
                    areaemission[i].Text   = St_F.DblToLocTxt(_as.Poll.EmissionRate[i]);
                    but1[i].BackColor = SystemColors.ButtonFace;
                }
                
                if (_as.Pt.Count > CornerAreaX.Length)
                {
                    Array.Resize(ref CornerAreaX, _as.Pt.Count + 1);
                    Array.Resize(ref CornerAreaY, _as.Pt.Count + 1);
                }

                int j = 0;
                foreach (PointD _pt in _as.Pt)
                {
                    CornerAreaX[j] = _pt.X;
                    CornerAreaY[j] = _pt.Y;
                    j++;
                }
                
                if (_as.Pt.Count > 0)
                {
                    Domain.EditSourceShape = false;  // block input of new vertices
                }

                for (int i = 0; i < 10; i++)
                {
                    dep[i] = _as.GetDep()[i];
                    
                    if (dep[i].V_Dep1 > 0 || dep[i].V_Dep2 > 0 || dep[i].V_Dep3 > 0)
                    {
                        but1[i].BackColor = Color.LightGreen; // mark that deposition is set
                    }
                    else
                    {
                        but1[i].BackColor = SystemColors.ButtonFace; // mark that deposition is reset
                    }
                }
            }
            catch{}		
        }

        //remove actual area source data
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
                textBox2.Text = "0";
                textBox3.Text = "0";
                numericUpDown1.Value = 0;
                numericUpDown2.Value = 0;
                numericUpDown4.Value = 1;
                comboBox1.SelectedIndex = 0;
                for (int i = 0; i < 10; i++)
                {
                    areaemission[i].Text = "0";
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

        //remove all area sources
        private void button3_Click(object sender, EventArgs e)
        {
            if (St_F.InputBox("Attention", "Delete all area sources??", this) == DialogResult.OK)
            {
                for (int i = 0; i < ItemData.Count; i++)
                {
                    ItemData[i].Pt.Clear();
                    ItemData[i].Pt.TrimExcess();
                }
                ItemData.Clear();
                ItemData.TrimExcess(); // Kuntner Memory
                
                textBox1.Text = "";
                textBox2.Text = "0";
                textBox3.Text = "";
                numericUpDown1.Value = 0;
                numericUpDown2.Value = 0;
                numericUpDown4.Value = 1;
                comboBox1.SelectedIndex = 0;
                for (int i = 0; i < 10; i++)
                {
                    areaemission[i].Text = "0";
                    dep[i].init();
                    but1[i].BackColor = SystemColors.ButtonFace;
                }
                trackBar1.Value = 1;
                trackBar1.Maximum = 1;
                ItemDisplayNr = trackBar1.Value - 1;
                Domain.EditSourceShape = true;  // allow input of new vertices
                RedrawDomain(this, null);
            }
        }

        //define source group
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] text = new string[2];
            text = comboBox1.Text.Split(new char[] { ',' });
            if (text.Length > 1)
            {
                int.TryParse(text[1], out sourcegroup);
            }
            else
            {
                int.TryParse(text[0], out sourcegroup);
            }
        }

        //search for the correct source group within the combobox
        private void combo(int SourceGroupNumber)
        {
            string[] text = new string[2];
            int i = 0;
            int sg = 0;
            foreach (string text1 in comboBox1.Items)
            {
                text = text1.Split(new char[] { ',' });
                
                if (text.Length > 1)
                {
                    int.TryParse(text[1], out sg);
                    if (sg == SourceGroupNumber)
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
                }
                else
                {
                    int.TryParse(text[0], out sg);
                    if (sg == SourceGroupNumber)
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
                }
                i++;
            }
        }

        void EditareasourcesResizeEnd(object sender, EventArgs e)
        {
            int dialog_width = ClientSize.Width;
            
            if (dialog_width < Dialog_Initial_Width)
            {
                dialog_width = Dialog_Initial_Width;
            }
            
            if (dialog_width > 150)
            {
                dialog_width -= 12;
                groupBox1.Width = dialog_width - TabControl_x0;
                trackBar1.Width = dialog_width - TrackBar_x0;
                textBox1.Width = dialog_width - textBox1.Left;
                textBox2.Width = dialog_width - textBox2.Left;
                textBox3.Width = dialog_width - textBox3.Left;
                numericUpDown1.Width = dialog_width - numericUpDown1.Left;
                numericUpDown2.Width = dialog_width - numericUpDown2.Left;
                numericUpDown4.Width = dialog_width - numericUpDown4.Left;
                comboBox1.Width      = dialog_width - comboBox1.Left;
            }

            button7.Left = Math.Max(100, this.Width - 103);

            int element_width = (groupBox1.Width - but1[1].Width - 20) / 2;
            groupBox1.Height = Math.Max(10, ClientSize.Height - groupBox1.Top - 5);
            
            if (element_width < TBWidth)
            {
                element_width = TBWidth;
            }
            
            for (int nr = 0; nr < 10; nr++)
            {
                areapollutant[nr].Width  = (int) (element_width);
                labelpollutant[nr].Width = (int) (element_width);
                areaemission [nr].Location = new System.Drawing.Point (areapollutant[nr].Left + areapollutant[nr].Width + 5, areapollutant[nr].Top);
                areaemission [nr].Width = (int) (element_width);
                but1[nr].Location = new System.Drawing.Point(areaemission[nr].Left + areaemission[nr].Width + 5, areapollutant[nr].Top - 1);
            }
            panel1.Width = ClientSize.Width;
        }
        
        private void RedrawDomain(object sender, EventArgs e)
        {
            // send Message to domain Form, that redraw is necessary
            try
            {
                if (AreaSourceRedraw != null)
                {
                    AreaSourceRedraw(this, e);
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

                    VerticesEditDialog vert = new VerticesEditDialog(number_of_vertices, ref CornerAreaX, ref CornerAreaY)
                    {
                        Location = new System.Drawing.Point(Right - 180 - 280, Top + 60),
                        StartPosition = FormStartPosition.Manual
                    };
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
                    
                    SaveArray();
                    vert.Dispose();
                }
                catch
                {
                    MessageBox.Show(this, "Nothing digitized","GRAL GUI",MessageBoxButtons.OK,MessageBoxIcon.Error);
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
                edit.Emission = St_F.TxtToDbl(areaemission[nr].Text,true);
                edit.Pollutant = areapollutant[nr].SelectedIndex;
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

                SaveArray(); // save values
            }
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

        private void EditAreaSources_FormClosing(object sender, FormClosingEventArgs e)
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

        void EditareasourcesFormClosed(object sender, FormClosedEventArgs e)
        {
            foreach(TextBox tex in areaemission)
            {
                tex.TextChanged -= new System.EventHandler(St_F.CheckInput);
            }
            foreach(Button but in but1)
            {
                but.Click -= new EventHandler(edit_deposition);
            }

            textBox1.KeyPress -= new KeyPressEventHandler(Comma1); //only point as decimal seperator is allowed
            comboBox1.KeyPress -= new KeyPressEventHandler(Comma2); //only point as decimal seperator is allowed
            
            CornerAreaX = null;
            CornerAreaY = null;
                        
            for (int nr = 0; nr < 10; nr++)
            {
                areapollutant[nr].SelectedValueChanged -= new System.EventHandler(Odour);
                areapollutant[nr].Items.Clear();
                areaemission[nr].KeyPress -= new KeyPressEventHandler(St_F.NumericInput);
                areapollutant[nr].Dispose();
                labelpollutant[nr].Dispose();
            }
            
            comboBox1.Items.Clear();
            
            for (int i = 0; i < ItemData.Count; i++)
            {
                ItemData[i].Pt.Clear();
                ItemData[i].Pt.TrimExcess();
            }
            ItemData.Clear();
            ItemData.TrimExcess();
        }
        
        // store and reload the settings
        void Button4Click(object sender, EventArgs e)
        {
            SaveArray();
            FillValues();
        }
        
        void EditareasourcesVisibleChanged(object sender, EventArgs e)
        {
            if (!Visible)
            {
            }
            else // Enable/disable items
            {
                bool enable = !Main.Project_Locked;
                if (enable)
                {
                    labelTitle.Text = "Edit Area Sources";
                    comboBox1.DropDownStyle = ComboBoxStyle.DropDown;
                }
                else
                {
                    labelTitle.Text = "Area Source Settings (Project Locked)";
                    comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                }

                foreach (Control c in Controls)
                {
                    if (c != trackBar1 && c!= groupBox1)
                    {
                        c.Enabled = enable;
                    }
                }
                
                foreach (Control c in groupBox1.Controls)
                {
                    if (c.GetType() != typeof(Button) && (c.GetType() != typeof(ListBox)))
                    {
                        c.Enabled = enable;
                    }
                }
                
                for (int i = 0; i < 10; i++)
                {
                    areapollutant[i].Visible = enable;
                    labelpollutant[i].Visible = !enable;
                }
            }
            Exit.Enabled = true;
            panel1.Enabled = true;
        }
        
        public void SetNumberOfVerticesText(string _s)
        {
            textBox2.Text = _s;
        }
                
        public void SetRasterSize(decimal _val)
        {
            numericUpDown4.Value = _val;
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

        /// <summary>
        /// OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            SaveArray();
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
            AreaSourceDataIO _as = new AreaSourceDataIO();
            string _file = Path.Combine(Gral.Main.ProjectName, "Emissions", "Asources.txt");
            _as.LoadAreaData(ItemData, _file);
            _as = null;
            SetTrackBarMaximum();
            FillValues();
            _as = null;
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
    }
}