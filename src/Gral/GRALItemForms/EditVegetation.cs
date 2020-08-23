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
 * Date: 07.02.2018
 * Time: 16:18
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
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
	/// <summary>
	/// Description of EditForests.
	/// </summary>
	public partial class EditVegetation : Form
	{
        /// <summary>
        /// Collection of all vegetation data
        /// </summary>
		public List<VegetationData> ItemData = new List<VegetationData>();
        /// <summary>
        /// Number of actual vegetation area to be displayed
        /// </summary>
		public int ItemDisplayNr;
        /// <summary>
        /// Recent number of an edited corner point
        /// </summary>
        public int CornerVegetation = 0;
        /// <summary>
        /// X corner points of a vegetation area in meters
        /// </summary>
        public double[] CornerVegX = new double[1000];
        /// <summary>
        /// Y corner points of a vegetation area in meters
        /// </summary>
        public double[] CornerVegY = new double[1000];           
		
		// delegate to send a message, that redraw is needed!
		public event ForceDomainRedraw VegetationRedraw;

        public bool AllowClosing = false;
        //delegate to send a message, that user tries to close the form
        public event ForceItemFormHide ItemFormHide;

        private List<string> vegetation_type = new List<string>();  //vegetation type list
		private int TextBox_x0 = 0;
		private int TrackBar_x0 = 0;
		private int Numericupdown_x0 = 0;
		
		public EditVegetation()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			Domain.EditSourceShape = true;  // allow input of new vertices
			
			#if __MonoCS__
			// set Numericupdowns-Alignement to Left in Linux
			var allNumUpDowns  = Main.GetAllControls<NumericUpDown>(this);
			foreach (NumericUpDown nu in allNumUpDowns)
			{
				nu.TextAlign = HorizontalAlignment.Left;
			}

			#else
			
			#endif

			//fill vegetation list: type, height(m), trunk zone (%), Leave-area-index trunk (m²/m³), Leave-area-index crown (m²/m³),
			vegetation_type.Add("Deciduous forest,25,10,0.1,0.5");
			vegetation_type.Add("Coniferuous forest,30,10,0.1,2.0");
			vegetation_type.Add("Mixed forest,30,10,0.1,1.25");
			vegetation_type.Add("Tropical forest,35,0,0.15,0.15");
			vegetation_type.Add("Orchard,3,1,0.1,1.00");
			vegetation_type.Add("Spruce,30,10,0.1,2.0");
			vegetation_type.Add("Pine,25,10,0.15,0.30");
			vegetation_type.Add("Larch,25,10,0.15,0.30");
			vegetation_type.Add("Maple,20,20,0.1,0.7");
			vegetation_type.Add("Oak,25,45,0.05,1.2");
			vegetation_type.Add("Birch,20,30,0.1,0.8");
			vegetation_type.Add("Beech,20,20,0.1,2.0");
			vegetation_type.Add("Japanese Zelkova,12,60,0.1,0.75");
			vegetation_type.Add("Magnolia,5,10,0.5,0.3");
			string[] text = new string[5];

			for(int i=0;i<vegetation_type.Count;i++)
			{
				text = vegetation_type[i].Split(new char[] { ',' });
				comboBox1.Items.Add(text[0]);
			}
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}

		private void RedrawDomain(object sender, EventArgs e)
		{
			// send Message to domain Form, that Section-Form is closed
			try
			{
				if (VegetationRedraw != null)
                {
                    VegetationRedraw(this, e);
                }
            }
			catch
			{}
		}
        
        void EditForestsFormClosing(object sender, FormClosingEventArgs e)
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

        public void ShowForm()
		{
			ItemDisplayNr = trackBar1.Value - 1;
			RedrawDomain(this, null);
		}
		
		void EditForestsLoad(object sender, EventArgs e)
		{
			TrackBar_x0 = trackBar1.Left;
			TextBox_x0 = textBox1.Left;
			Numericupdown_x0 = numericUpDown1.Left;
			FillValues();
		}
		
		//increase the number of area sources by one
		private void Button1_Click(object sender, EventArgs e)
		{
			if ((textBox1.Text != "") && (textBox2.Text != ""))
			{
				SaveArray();
				//textBox1.Text = "";
				textBox2.Text = "0";
				textBox2.Text = "";
				
				trackBar1.Maximum += 1;
				trackBar1.Value = trackBar1.Maximum;
				ItemDisplayNr = trackBar1.Maximum - 1;
				Domain.EditSourceShape = true;  // allow input of new vertices
			}
		}
		
		//scroll between the area sources
		private void TrackBar1_Scroll(object sender, EventArgs e)
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
			VegetationData _vdata;
			if (ItemDisplayNr >= ItemData.Count) // new item
			{
				_vdata = new VegetationData();
			}
			else // change existing item
			{
				_vdata = ItemData[ItemDisplayNr];
			}
			
			if (textBox2.Text != "")
			{
				if ((textBox1.Text != "") && (Convert.ToInt32(textBox2.Text) > 2))
				{
					// new area
					int number_of_vertices = Convert.ToInt32(textBox2.Text);
					_vdata.Pt.Clear();
					_vdata.Pt.TrimExcess();
					
					for (int i = 0; i <number_of_vertices; i++)
					{
						_vdata.Pt.Add(new PointD(CornerVegX[i], CornerVegY[i]));
					}
					double areapolygon = St_F.CalcArea(_vdata.Pt, false);
					textBox3.Text = St_F.DblToIvarTxt(areapolygon);
					
					if (number_of_vertices > 0)
                    {
                        Domain.EditSourceShape = false;  // block input of new vertices
                    }

                    _vdata.VerticalExt = (float)(numericUpDown2.Value);
					_vdata.TrunkZone = (float)(numericUpDown1.Value);
					_vdata.LADTrunk = (float)(numericUpDown3.Value);
					_vdata.LADCrown = (float)(numericUpDown5.Value);
					_vdata.Coverage = (float)(numericUpDown4.Value);
					_vdata.Name = textBox1.Text;
					_vdata.Area = (float) (areapolygon);
					
					if (ItemDisplayNr >= ItemData.Count) // new item
					{
						ItemData.Add(_vdata);
					}
					else
					{
						ItemData[ItemDisplayNr] = _vdata;
					}
					
					RedrawDomain(this, null);
				}
			}
		}
		
		//fill actual values
		/// <summary>
    	/// Fills the dialog with data from the recent item object
    	/// </summary> 
		public void FillValues()
		{
			VegetationData _vdata;
			if (ItemDisplayNr < ItemData.Count)
			{
				_vdata = ItemData[ItemDisplayNr];
			}
			else
			{
				if (ItemData.Count > 0)
				{
					_vdata = new VegetationData(ItemData[ItemData.Count - 1]);
				}
				else
				{
					_vdata = new VegetationData();
				}
			}
			
			textBox1.Text = _vdata.Name;
			textBox2.Text = _vdata.Pt.Count.ToString();
			textBox3.Text = _vdata.Area.ToString();
			
			CultureInfo ic  = CultureInfo.InvariantCulture;
			
			numericUpDown2.Value = St_F.ValueSpan(0, 1000, (double) _vdata.VerticalExt);
			numericUpDown1.Value = St_F.ValueSpan(0, 100, (double) _vdata.TrunkZone);
			numericUpDown3.Value = St_F.ValueSpan(0, 10, (double) _vdata.LADTrunk);
			numericUpDown5.Value = St_F.ValueSpan(0, 10, (double) _vdata.LADCrown);
			numericUpDown4.Value = St_F.ValueSpan(0, 100, (double) _vdata.Coverage);

			if (_vdata.Pt.Count > CornerVegX.Length)
			{
				Array.Resize(ref CornerVegX, _vdata.Pt.Count + 1);
				Array.Resize(ref CornerVegY, _vdata.Pt.Count + 1);
			}
			for (int i = 0; i < Math.Min(_vdata.Pt.Count, CornerVegX.Length); i++)
			{
				CornerVegX[i] = _vdata.Pt[i].X;
				CornerVegY[i] = _vdata.Pt[i].Y;
			}
			
			if (Convert.ToInt32(textBox2.Text) > 0)
            {
                Domain.EditSourceShape = false;  // block input of new vertices
            }
        }
	
		//remove actual vegetation
		private void Button2_Click(object sender, EventArgs e)
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
				if (St_F.InputBox("Attention", "Do you really want to delete this vegetation area?", this) == DialogResult.OK)
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
				numericUpDown2.Value = 5;
				
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

		//remove all forests
		private void Button3_Click(object sender, EventArgs e)
		{
			if (St_F.InputBox("Attention", "Delete all vegetation areas??", this) == DialogResult.OK)
			{
				ItemData.Clear();
				ItemData.TrimExcess(); // Kuntner Memory
				
				textBox1.Text = "";
				textBox2.Text = "0";
				textBox3.Text = "0";
				numericUpDown2.Value = 5;
				
				trackBar1.Value = 1;
				trackBar1.Maximum = 1;
				ItemDisplayNr = trackBar1.Value - 1;
				Domain.EditSourceShape = true;  // allow input of new vertices
				RedrawDomain(this, null);
			}
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

                    VerticesEditDialog vert = new VerticesEditDialog(number_of_vertices, ref CornerVegX, ref CornerVegY)
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
					MessageBox.Show(this, "Nothing digitized","GRAL GUI",MessageBoxButtons.OK,MessageBoxIcon.Error);
				}
			}
		}
		
		void EditForestsFormClosed(object sender, FormClosedEventArgs e)
		{
			FormClosing -= new FormClosingEventHandler(EditForestsFormClosing);
			
			CornerVegX = null;
			CornerVegY = null;
			for (int nr = 0; nr < ItemData.Count; nr++)
			{
			    ItemData[nr].Pt.Clear();
			    ItemData[nr].Pt.TrimExcess();
			}
			ItemData.Clear();
			ItemData.TrimExcess();
		}

		//select vegetation type
		public void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			CultureInfo ic = CultureInfo.InvariantCulture;
			string[] text = new string[5];
			text = vegetation_type[comboBox1.SelectedIndex].Split(new char[] { ',' });

			numericUpDown1.Value = Convert.ToDecimal(text[2], ic); // trunk zone
			numericUpDown2.Value = Convert.ToDecimal(text[1], ic); // height
			numericUpDown3.Value = Convert.ToDecimal(text[3], ic); // Leave area index Trunk
			numericUpDown5.Value = Convert.ToDecimal(text[4], ic); // Leave area index Trunk
			numericUpDown4.Value = Convert.ToDecimal(100, ic);     // Coverage
		}
		
		
		void EditForestsVisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
			}
			else // Enable/disable items
			{
				bool enable = !Main.Project_Locked;
				if (enable)
				{
					Text = "Edit vegetation";
				}
				else
				{
					Text = "Vegetation data (project locked)";
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
		
		void EditForestsResizeEnd(object sender, EventArgs e)
		{
			int dialog_width = ClientSize.Width;
			if (dialog_width > 130)
			{
				dialog_width -= 12;
				trackBar1.Width = dialog_width - TrackBar_x0;
				textBox2.Width = dialog_width - button5.Right - 5;
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

        private void Button6_Click(object sender, EventArgs e)
        {
            this.Close(); // does not close the form, because closing hides the form
        }
    }
}
