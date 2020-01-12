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

using Gral;
using GralDomain;
using GralItemData;
using GralStaticFunctions;

namespace GralItemForms
{
    public partial class EditBuildings : Form
    {
        /// <summary>
        /// Collection of all building data
        /// </summary>
        public List<BuildingData> ItemData = new List<BuildingData>();
        /// <summary>
        /// Number of actual building to be displayed
        /// </summary>
        public int ItemDisplayNr;
        /// <summary>
        /// Recent number of an edited corner point
        /// </summary>
        public int CornerBuilding = 0;
        /// <summary>
        /// X corner points of a building in meters
        /// </summary>
        public double[] CornerBuildingX = new double[1000];
        /// <summary>
        /// Y corner points of a building in meters
        /// </summary>
        public double[] CornerBuildingY = new double[1000];           
		
        // delegate to send a message, that redraw is needed!
		public event ForceDomainRedraw BuildingRedraw;

        public bool AllowClosing = false;
        //delegate to send a message, that user tries to close the form
        public event ForceItemFormHide ItemFormHide;

        private CultureInfo ic = CultureInfo.InvariantCulture;
		private int TextBox_x0 = 0;
		private int TrackBar_x0 = 0;
		private int Numericupdown_x0 = 0;
		
        public EditBuildings()
        {
            InitializeComponent();
            Domain.EditSourceShape = true;  // allow input of new vertices
           
			#if __MonoCS__
				var allNumUpDowns  = Main.GetAllControls<NumericUpDown>(this);
				foreach (NumericUpDown nu in allNumUpDowns)
				{
					nu.TextAlign = HorizontalAlignment.Left;
				}
		    	#endif
            textBox1.KeyPress += new KeyPressEventHandler(Comma1); //only point as decimal seperator is allowed
            MouseMove += new MouseEventHandler(Aktiv);                     
        }

		private void Aktiv(object sender, MouseEventArgs e)
        {
            Activate();
        }

        private void Editbuildings_Load(object sender, EventArgs e)
        {
        	TrackBar_x0 = trackBar1.Left;
			TextBox_x0 = textBox1.Left;
			Numericupdown_x0 = numericUpDown1.Left;
            FillValues();
        }

        private void Comma1(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',') e.KeyChar = '.';
            int asc = (int)e.KeyChar; //get ASCII code
        }

        //increase the number of buildings by one
        private void button1_Click(object sender, EventArgs e)
        {
            if ((textBox1.Text != "") && (textBox2.Text != ""))
            {
                SaveArray();
                trackBar1.Maximum = trackBar1.Maximum + 1;
                trackBar1.Value = trackBar1.Maximum;
                ItemDisplayNr = trackBar1.Maximum - 1;
                Domain.EditSourceShape = true;  // allow input of new vertices
            }
        }

        //scroll between the buildings
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
        	BuildingData _bdata; 
			if (ItemDisplayNr >= ItemData.Count) // new item
			{
				_bdata = new BuildingData();
			}
			else // change existing item
			{
				_bdata = ItemData[ItemDisplayNr];
			}
			
            if (textBox2.Text != "")
            {
                if ((textBox1.Text != "") && (Convert.ToInt32(textBox2.Text) > 2))
                {
          			// new area
                	int number_of_vertices = Convert.ToInt32(textBox2.Text);
                	_bdata.Pt.Clear();
                	_bdata.Pt.TrimExcess();
                	for (int i = 0; i < number_of_vertices; i++)
                    {
                		PointD _pt = new PointD(CornerBuildingX[i], CornerBuildingY[i]);
                		_bdata.Pt.Add(_pt);
                    }
                	double areapolygon = St_F.CalcArea(_bdata.Pt);
                	textBox3.Text = St_F.DblToIvarTxt(areapolygon);
                	
                	if (number_of_vertices > 0) Domain.EditSourceShape = false;  // block input of new vertices
                	
                	float height = Convert.ToSingle(numericUpDown1.Value);
                	if (checkBox1.Checked) // absolute height over sea
                		height *= (-1);
                	_bdata.Name =  St_F.RemoveinvalidChars(textBox1.Text) + '\t' + St_F.RemoveinvalidChars(textBox4.Text) + '\t' + St_F.RemoveinvalidChars(textBox5.Text);
                	_bdata.Area = (float) areapolygon;
                	_bdata.Height = height;
                	
                	if (ItemDisplayNr >= ItemData.Count) // new item
                	{
                		ItemData.Add(_bdata);
                	}
                	else
                	{
                		ItemData[ItemDisplayNr] = _bdata;
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
            	BuildingData _bdata;
				if (ItemDisplayNr < ItemData.Count)
				{
					_bdata = ItemData[ItemDisplayNr];
				}
				else
				{
					if (ItemData.Count > 0)
					{
						_bdata = new BuildingData(ItemData[ItemData.Count - 1]);
					}
					else
					{
						_bdata = new BuildingData();
					}
				}
				
                string[] name = new string[4];
                name = _bdata.Name.Split(new char[] {'\t'});
                
                if (name.Length <= 1) // just name but no street/number
                	textBox1.Text = name[0];
                else
                {
                	textBox1.Text = name[0];
                	if (name.Length > 1)
                		textBox4.Text = name[1];
                	if (name.Length > 2)
                		textBox5.Text = name[2];
                }
                
                textBox3.Text = St_F.DblToIvarTxt(Math.Round(_bdata.Area,1));
                
                decimal height = Convert.ToDecimal(_bdata.Height);
				if (height >= 0) // height above ground
					checkBox1.Checked = false;
				else // height above sea
					checkBox1.Checked = true;
				numericUpDown1.Value = St_F.ValueSpan(0, 7999, (double) Math.Abs(height));
                
				// Lower bound - not active at the moment				
				numericUpDown2.Value = St_F.ValueSpan(0, 1000, Convert.ToDouble(_bdata.LowerBound));
                
				int i = 0;
				foreach (PointD _pt in _bdata.Pt)
                {
                    CornerBuildingX[i] = _pt.X;
                    CornerBuildingY[i] = _pt.Y;
                    i++;
                }
				
				textBox2.Text = _bdata.Pt.Count.ToString();
                if (_bdata.Pt.Count > 0) Domain.EditSourceShape = false;  // block input of new vertices
            }
            catch
            {
            }
        }

        //remove actual building
        private void button2_Click(object sender, EventArgs e)
        {
            RemoveOne(true, true);
        }
        
        /// <summary>
    	/// Remove the recent item object from the item list
    	/// </summary> 
        public void RemoveOne(bool ask, bool redraw)
        { 
        	// if ask = false do not ask and delete immediality
        	if (ask == true)
        	{
              if (St_F.InputBoxYesNo("Attention", "Do you really want to delete this building?", St_F.GetScreenAtMousePosition() + 340, 400) == DialogResult.Yes)
                ask = false;
              else
              	ask = true; // Cancel -> do not delete!   
        	}
        	
        	if (ask == false)
            {
                textBox1.Text = "";
                textBox2.Text = "0";
                textBox3.Text = "0";
                numericUpDown1.Value = 0;
                numericUpDown2.Value = 0;
                if (ItemDisplayNr >= 0)
                {
                    try
                    {
                        if (trackBar1.Maximum > 1)
                            trackBar1.Maximum = trackBar1.Maximum - 1;
                        trackBar1.Value = Math.Min(trackBar1.Maximum, trackBar1.Value);
                        ItemData.RemoveAt(ItemDisplayNr);
                        ItemDisplayNr = trackBar1.Value - 1;
                        FillValues();
                    }
                    catch
                    {
                    }
                }
                if (redraw) RedrawDomain(this, null);
            }
        }

        //remove all buildings
        private void button3_Click(object sender, EventArgs e)
        {
            if (St_F.InputBox("Attention", "Delete all buildings??", this) == DialogResult.OK)
            {
            	for (int i = 0; i < ItemData.Count; i++)
				{
					ItemData[i].Pt.Clear();
				    ItemData[i].Pt.TrimExcess();
				}	            	
                ItemData.Clear();
                ItemData.TrimExcess(); // Kuntner
                
				for (int i = 0; i < CornerBuildingX.Length; i++)
                {
                	CornerBuildingX[i] = 0; // Kuntner
                	CornerBuildingY[i] = 0; // Kuntner
                }
             
                textBox1.Text = "";
                textBox2.Text = "0";
                textBox3.Text = "0";
                numericUpDown1.Value = 0;
                numericUpDown2.Value = 0;
                trackBar1.Value = 1;
                trackBar1.Maximum = 1;
                ItemDisplayNr = trackBar1.Value - 1;
                Domain.EditSourceShape = true;  // allow input of new vertices
                RedrawDomain(this, null);
            }
        }
        
        private void RedrawDomain(object sender, EventArgs e)
        {
        	// send Message to domain Form, that Section-Form is closed
			try
			{
			if (BuildingRedraw != null)
				BuildingRedraw(this, e); 
			}
			catch
			{}
        }
        
        public void ShowForm()
        {
        	ItemDisplayNr = trackBar1.Value - 1;
        	RedrawDomain(this, null);
        }

        
        void CheckBox1CheckedChanged(object sender, EventArgs e)
        {
        	if (checkBox1.Checked)
        		label5.BackColor = Color.Yellow;
        	else
        		label5.BackColor = Color.Transparent;
        }

        private void EditBuildings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.ApplicationExitCall || e.CloseReason == CloseReason.WindowsShutDown)
            {
                // allow Closing in this cases
                e.Cancel = false;
            }
            else
            {
                // Hide form and send message to caller when user tries to close this form
                if (!AllowClosing)
                {
                    // send Message to domain Form, that redraw is necessary
                    try
                    {
                        if (ItemFormHide != null)
                        {
                            ItemFormHide(this, e);
                        }
                    }
                    catch
                    { }
                    this.Hide();
                    e.Cancel = true;
                }
            }
        }

        void EditbuildingsFormClosed(object sender, FormClosedEventArgs e)
        {
        	textBox1.KeyPress -= new KeyPressEventHandler(Comma1); //only point as decimal seperator is allowed
        	MouseMove -= new MouseEventHandler(Aktiv);
        	
        	CornerBuildingX = null;
			CornerBuildingY = null;
			
			for (int nr = 0; nr < ItemData.Count; nr++)
			{
			    ItemData[nr].Pt.Clear();
			    ItemData[nr].Pt.TrimExcess();
			}
        	ItemData.Clear();
			ItemData.TrimExcess();
			
			toolTip1.Dispose();
        }
        
        void EditbuildingsVisibleChanged(object sender, EventArgs e)
        {
        	if (!Visible)
			{
			}
			else // Enable/disable items
			{
				bool enable = !Main.Project_Locked;
				if (enable)
				{
					Text = "Edit buildings";
				}
				else
				{
					Text = "Building settings (project locked)";
				}
				foreach (Control c in Controls)
				{
					if (c != trackBar1)
					{
						c.Enabled = enable;
					}
				}
			}
        }
        void EditbuildingsResizeEnd(object sender, EventArgs e)
        {
        	int dialog_width = ClientSize.Width;
        	if (dialog_width > 130)
			{
				dialog_width -= 12;
				trackBar1.Width = dialog_width - TrackBar_x0;
				textBox1.Width = dialog_width - TextBox_x0;
				textBox4.Width = dialog_width - TextBox_x0;
				textBox5.Width = dialog_width - TextBox_x0;
			}
            button6.Left = (int)((dialog_width - button6.Width) * 0.5);
        }
        
        public void SetNumberOfVerticesText(string _s)
		{
			textBox2.Text = _s;
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

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close(); // does not close the form, because closing hides the form
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                int number_of_vertices = Convert.ToInt32(textBox2.Text);
                if (number_of_vertices < 2) throw new System.InvalidOperationException("Nothing digitized");
                VerticesEditDialog vert = new VerticesEditDialog(number_of_vertices, ref CornerBuildingX, ref CornerBuildingY)
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
                }

                SaveArray();
                vert.Dispose();

            }
            catch
            {
                MessageBox.Show(this, "Nothing digitized", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}