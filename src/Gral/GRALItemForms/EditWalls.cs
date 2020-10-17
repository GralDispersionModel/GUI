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

/*
 * Created by SharpDevelop.
 * User: Markus Kuntner
 * Date: 26.07.2017
 * Time: 09:17
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using Gral;
using GralData;
using GralDomain;
using GralItemData;
using GralStaticFunctions;

namespace GralItemForms
{
    public delegate void ForceDomainRedraw(object sender, EventArgs e);
    public delegate void ForceItemFormHide(object sender, EventArgs e);
    public delegate void ForceItemOK(object sender, EventArgs e);
    public delegate void ForceItemCancel(object sender, EventArgs e);

    public partial class EditWalls : Form
    {
        /// <summary>
        /// Collection of all wall data
        /// </summary>
        public List<WallData> ItemData = new List<WallData>();
        /// <summary>
        /// Number of actual walls to be displayed
        /// </summary>
        public int ItemDisplayNr;
        /// <summary>
        /// Recent number of an edited corner point
        /// </summary>
        public int CornerWallCount = 0;
        /// <summary>
        /// X corner points of a wall in meters
        /// </summary>
        public double[] CornerWallX = new double[1000];
        /// <summary>
        /// Y corner points of a wall in meters
        /// </summary>
        public double[] CornerWallY = new double[1000];
        /// <summary>
        /// Z corner points of a wall in meters
        /// </summary>
        public double[] CornerWallZ  = new double[1000];   
        
        private int vertices;                              //number of maximum corner points
        private readonly CultureInfo ic = CultureInfo.InvariantCulture;
        
        // delegate to send a message, that redraw is needed!
        public event ForceDomainRedraw WallRedraw;

        public bool AllowClosing = false;
        //delegates to send a message, that user uses OK or Cancel button
        public event ForceItemOK ItemFormOK;
        public event ForceItemCancel ItemFormCancel;

        private int TextBox_x0 = 0;
        private int TrackBar_x0 = 0;
        private int Numericupdown_x0 = 0;
    
        
        public EditWalls()
        {
            InitializeComponent();
            Domain.EditSourceShape = true;  // allow input of new vertices
            
            #if __MonoCS__
            numericUpDown1.TextAlign = HorizontalAlignment.Left;
            #endif
        }

        void EditWallsLoad(object sender, EventArgs e)
        {
            TrackBar_x0 = trackBar1.Left;
            TextBox_x0 = textBox1.Left;
            Numericupdown_x0 = numericUpDown1.Left;
            FillValues();
        }
        
        //increase the number of Walls by one
        void Button1Click(object sender, EventArgs e)
        {
          if ((textBox1.Text != "") && (textBox2.Text != ""))
            {
                SaveArray();
                trackBar1.Maximum += 1;
                trackBar1.Value = trackBar1.Maximum;
                ItemDisplayNr = trackBar1.Maximum - 1;
                Domain.EditSourceShape = true;  // allow input of new vertices
            }	
        }
        
        //scroll between the line sources
        void TrackBar1Scroll(object sender, EventArgs e)
        {
            SaveArray();
            ItemDisplayNr = trackBar1.Value - 1;
            FillValues();
            RedrawDomain(this, e);	
        }
        
        //save data in array list
        /// <summary>
        /// Saves the recent dialog data in the item object and the item list
        /// </summary> 
        public void SaveArray()
        {
            WallData _wdata;
            if (ItemDisplayNr >= ItemData.Count) // new item
            {
                _wdata = new WallData();
            }
            else // change existing item
            {
                _wdata = ItemData[ItemDisplayNr];
            }
            
            if (textBox2.Text != "") // Vertices
            {
                if ((textBox1.Text != "") && (Convert.ToInt32(textBox2.Text) > 1))
                {
                    int number_of_vertices = Convert.ToInt32(textBox2.Text);
                    _wdata.Pt.Clear();
                    _wdata.Pt.TrimExcess();
                    
                    // new lenght
                    int absHeight = 1;
                    if (checkBox1.Checked == true)
                    {
                        absHeight = -1;
                    }

                    for (int i = 0; i <number_of_vertices; i++)
                    {
                        _wdata.Pt.Add(new PointD_3d(CornerWallX[i], CornerWallY[i], Math.Abs(CornerWallZ[i]) * absHeight));
                    }
                    
                    double lenght = Math.Round(_wdata.CalcLenght(), 1);
                    textBox3.Text = St_F.DblToIvarTxt(lenght);
                    
                    if (Convert.ToInt32(textBox2.Text) > 0) 
                    {
                        Domain.EditSourceShape = false;  // block input of new vertices
                    }
                    
                    // get recent edgepoint height
                    Get_edgepoint_height();
                    
                    _wdata.Name = St_F.RemoveinvalidChars(textBox1.Text);
                    _wdata.Lenght = (float) (lenght);
                    
                    if (ItemDisplayNr >= ItemData.Count) // new item
                    {
                        ItemData.Add(_wdata);
                    }
                    else
                    {
                        ItemData[ItemDisplayNr] = _wdata;
                    }
                }
            }
            RedrawDomain(this, null);
        }

        void TextBox2TextChanged(object sender, EventArgs e)
        {
            Get_edgepoint_height(); // get recent edgepoint height
            vertices = Convert.ToInt32(textBox2.Text);
            if ( vertices <= 1)
            {
                return; // not enough vertices
            }

            trackBar2.Maximum = vertices;
        }
        
        void NumericUpDown1ValueChanged(object sender, EventArgs e)
        {
            Get_edgepoint_height(); // get recent edgepoint height	
        }
        
        void TrackBar2Scroll(object sender, EventArgs e)
        {
            int edge = trackBar2.Value - 1;
            
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
                numericUpDown1.Value = (decimal) Math.Abs(CornerWallZ[edge]);
                
                Domain.MarkerPoint.X =  CornerWallX[edge];
                Domain.MarkerPoint.Y =  CornerWallY[edge];
                
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

        private void Get_edgepoint_height()
        {
            int edge = trackBar2.Value - 1;
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

                CornerWallZ[edge] = height;
            }
        }

        //fill actual values
        /// <summary>
        /// Fills the dialog with data from the recent item object
        /// </summary> 
        public void FillValues()
        {
            WallData _wdata;
            if (ItemDisplayNr < ItemData.Count)
            {
                _wdata = ItemData[ItemDisplayNr];
            }
            else
            {
                if (ItemData.Count > 0)
                {
                    _wdata = new WallData(ItemData[ItemData.Count - 1]);
                }
                else
                {
                    _wdata = new WallData();
                }
            }
            
            textBox1.Text = _wdata.Name; // name of wall
            textBox2.Text = _wdata.Pt.Count.ToString(); // number of vertices
            textBox3.Text = _wdata.Lenght.ToString(); // lenght
            
            if (_wdata.Pt.Count == 0)
            {
                return;
            }

            trackBar2.Value = 1;
            trackBar2.Minimum = 1;
            trackBar2.Maximum = _wdata.Pt.Count + 1;
            
            decimal height = Convert.ToDecimal (_wdata.Pt[0].Z); // check heigt at first edge point
            if (height >= 0) // height above ground
            {
                checkBox1.Checked = false;
            }
            else // height above sea
            {
                checkBox1.Checked = true;
            }

            if (_wdata.Pt.Count > CornerWallX.Length)
            {
                Array.Resize(ref CornerWallX, _wdata.Pt.Count + 1);
                Array.Resize(ref CornerWallY, _wdata.Pt.Count + 1);
                Array.Resize(ref CornerWallZ, _wdata.Pt.Count + 1);
            }
            for (int i = 0; i < _wdata.Pt.Count; i++)
            {
                CornerWallX[i] = _wdata.Pt[i].X;
                CornerWallY[i] = _wdata.Pt[i].Y;
                CornerWallZ[i] = (float) (_wdata.Pt[i].Z);
            }
            
            if (Convert.ToInt32(textBox2.Text) > 0)
            {
                Domain.EditSourceShape = false;  // block input of new vertices
            }

            numericUpDown1.Value = St_F.ValueSpan(0, 8000, (double) Math.Abs(CornerWallZ[0]));
        }

        //remove actual wall 
        void Button2Click(object sender, EventArgs e)
        {
            RemoveOne(true);
            RedrawDomain(this, e);
        }
        
        /// <summary>
        /// Remove the recent item object from the item list
        /// </summary> 
        public void RemoveOne(bool ask)
        {
            // if ask = false do not ask and delete immediality
            if (ask == true)
            {
                if (St_F.InputBox("Attention", "Do you really want to delete this wall?", this) == DialogResult.OK)
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
                textBox1.Text = "Wall";
                textBox2.Text = "0";
                textBox3.Text = "0";
                numericUpDown1.Value = 0;
                
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
                RedrawDomain(this, null);
            }
        }

        //remove all walls
        void Button3Click(object sender, EventArgs e)
        {
            if (St_F.InputBox("Attention", "Delete all walls??", this) == DialogResult.OK)
            {
                ItemData.Clear();
                ItemData.TrimExcess(); // Kuntner Memory
                
                for (int i = 0; i < CornerWallX.Length; i++)
                {
                    CornerWallX[i] = 0; // Kuntner
                    CornerWallY[i] = 0; // Kuntner
                    CornerWallZ[i] = 0; // Kuntner
                }
             
                textBox1.Text = "Wall";
                textBox2.Text = "0";
                textBox3.Text = "0";
                numericUpDown1.Value = 4;
                trackBar1.Value = 1;
                trackBar1.Maximum = 1;
                trackBar2.Value = 1;
                trackBar2.Maximum = 1;
                ItemDisplayNr = trackBar1.Value - 1;
                Domain.EditSourceShape = true;  // allow input of new vertices
                RedrawDomain(this, e);
            }
        }
        
        
       private void RedrawDomain(object sender, EventArgs e)
        {
            // send Message to domain Form, that Section-Form is closed
            try
            {
            if (WallRedraw != null)
                {
                    WallRedraw(this, e);
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

                    VerticesEditDialog vert = new VerticesEditDialog(number_of_vertices, ref CornerWallX, ref CornerWallY, ref CornerWallZ)
                    {
                        StartPosition = FormStartPosition.Manual
                    };
                    if (Right < SystemInformation.PrimaryMonitorSize.Width / 2)
                    {
                        vert.Location = new Point(Right + 4, Top);
                    }
                    else
                    {
                        vert.Location = new Point(Left - 250, Top);
                    }
                
                    vert.Vertices_redraw += new ForceDomainRedraw(RedrawDomain);
                    if (vert.ShowDialog() == DialogResult.OK)
                    {
                        int cornerCount = vert.Lines;
                        SetNumberOfVerticesText(cornerCount.ToString());
                        trackBar2.Value = 1;
                        trackBar2.Minimum = 1;
                        trackBar2.Maximum = cornerCount;
                        TrackBar2Scroll(null, null); // refresh dialog data
                    }
                    vert.Dispose();
                    SaveArray();	
                }
                catch
                {
                    MessageBox.Show(this, "Nothing digitized","GRAL GUI",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
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

        private void EditWalls_FormClosing(object sender, FormClosingEventArgs e)
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

        void EditWallsFormClosed(object sender, FormClosedEventArgs e)
        {
            CornerWallX = null;
            CornerWallY = null;
            CornerWallZ = null;
            ItemData.Clear();
            ItemData.TrimExcess();
            toolTip1.Dispose();
        }
        
        void EditWallsVisibleChanged(object sender, EventArgs e)
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
                    Text = "Edit walls";
                }
                else
                {
                    Text = "Wall settings (project locked)";
                }
                foreach (Control c in Controls)
                {
                    if (c != trackBar1 && c != trackBar2)
                    {
                        c.Enabled = enable;
                    }
                }
            }
        }
        
        void EditWallsResizeEnd(object sender, EventArgs e)
        {
            int dialog_width = ClientSize.Width;
            if (dialog_width > 130)
            {
                dialog_width -= 12;
                trackBar1.Width = dialog_width - TrackBar_x0;
                trackBar2.Width = dialog_width - TrackBar_x0;
                textBox1.Width = dialog_width - TextBox_x0;
                textBox2.Width = dialog_width - button5.Right - 5;
            }
            button4.Left = Math.Max(100, this.Width - 110);
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
        public string GetNumberOfVerticesText()
        {
            return textBox2.Text;
        }
        public void SetNumberOfVerticesText(string _s)
        {
            textBox2.Text = _s;
        }
        
        public Single GetNumericUpDownHeightValue()
        {
            return (Single) (numericUpDown1.Value);
        }
        public bool CheckboxAbsHeightChecked()
        {
            return checkBox1.Checked;
        }
        /// <summary>
        /// OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button6_Click(object sender, EventArgs e)
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
            WallDataIO _wd = new WallDataIO();
            string _file = System.IO.Path.Combine(Gral.Main.ProjectName, "Emissions", "Walls.txt");
            _wd.LoadWallData(ItemData, _file);
            _wd = null;
            SetTrackBarMaximum();
            FillValues();
        }
    }
}
