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
using System.Windows.Forms;

using Gral;
using GralDomain;
using GralItemData;
using GralStaticFunctions;

namespace GralItemForms
{
	public partial class EditPointSources : Form
	{
        /// <summary>
        /// Collection of all point source data
        /// </summary>
        public List<PointSourceData> ItemData = new List<PointSourceData>();
        /// <summary>
        /// Number of actual point source to be displayed
        /// </summary>
        public int ItemDisplayNr;

		// delegate to send a message, that redraw is needed!
		public event ForceDomainRedraw PointSourceRedraw;

        public bool AllowClosing = false;
        //delegate to send a message, that user tries to close the form
        public event ForceItemFormHide ItemFormHide;

        private int sourcegroup = 1; 			              //sourcegroup
		private TextBox [] pemission = new TextBox [10];      //textboxes for emission strenght input of point sources
		private ComboBox [] ppollutant = new ComboBox [10];   //ComboBoxes for selecting pollutants of point sources
		private Label [] labelpollutant = new Label [10];     //labels for pollutant types
		private Button [] but1 = new Button [10];             //Buttons for unit and deposition
		private Deposition [] dep = new Deposition [10];
		private CultureInfo ic = CultureInfo.InvariantCulture;
		private int Dialog_Initial_Width = 0;
		private int Button1_Width = 50;
		private int Button1_Height = 10;
		private int TBWidth = 80;
		private int TextBox_x0 = 0;
		private int TrackBar_x0 = 0;
		private int Numericupdown_x0 = 0;
		private int Numericupdown_x2 = 0;
		private int TabControl_x0 = 0;
		private string TempTimeSeries = string.Empty;
		private string VelTimeSeries = string.Empty;

	    public EditPointSources ()
		{
			InitializeComponent ();

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
            		ppollutant[nr].Items.Add(Main.PollutantList[i]);
            	}
            }
		}

		private void EditPointSources_Load (object sender, EventArgs e)
		{
		    textBox2.TextChanged += new System.EventHandler(St_F.CheckInput);
		    textBox3.TextChanged += new System.EventHandler(St_F.CheckInput);

			Dialog_Initial_Width = ClientSize.Width;

			TrackBar_x0 = trackBar1.Left;
			TextBox_x0 = textBox1.Left;
			Numericupdown_x0 = numericUpDown1.Left;
			Numericupdown_x2 = numericUpDown2.Left;
			TabControl_x0 = groupBox1.Left;

			ClientSize = new Size(Math.Max(textBox2.Right + 5, but1[1].Right + 5), groupBox1.Top + groupBox1.Height + 10);
			EditPointSourcesResizeEnd(null, null);

            textBox2.KeyPress += new KeyPressEventHandler(St_F.NumericInput); //only point as decimal seperator is allowed
            textBox3.KeyPress += new KeyPressEventHandler(St_F.NumericInput); //only point as decimal seperator is allowed

            FillValues();
        }

		private void createTextbox (int x0, int y0, int b, int h, int nr)
		{
            //list box for selecting pollutants
            ppollutant[nr] = new ComboBox
            {
                Location = new System.Drawing.Point(x0, y0),
                Font = textBox2.Font,// new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                Height = textBox2.Height,
                Width = 80,
                DropDownHeight = Math.Max(textBox2.Height * 4, groupBox1.Height - y0)
            };
            groupBox1.Controls.Add(ppollutant[nr]);
			ppollutant[nr].SelectedValueChanged += new System.EventHandler(Odour);

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

            //text box for input of emission rates
            pemission[nr] = new TextBox
            {
                Location = new System.Drawing.Point(ppollutant[nr].Left + ppollutant[nr].Width + 5, y0),
                Size = new System.Drawing.Size(b - 20, h),
                TextAlign = System.Windows.Forms.HorizontalAlignment.Right,
                Text = "0"
            };
            pemission[nr].KeyPress += new KeyPressEventHandler(St_F.NumericInput);
			pemission[nr].TextChanged += new System.EventHandler(St_F.CheckInput);
			groupBox1.Controls.Add(pemission[nr]);

            //labels
            but1[nr] = new Button
            {
                Location = new System.Drawing.Point(pemission[nr].Left + pemission[nr].Width + 5, y0 - 1),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Height = pemission[0].Height,
                Width = Button1_Width,
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


		private void Paste(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                 string txt = Clipboard.GetText();
                 double val = 0;
                 try
                 {
                     if (double.TryParse(txt, out val))
                     {
                         TextBox tbox = sender as TextBox;
                         tbox.Text = txt;
                     }
                 }
                 catch{}
            }
        }

		private void Odour(object sender, EventArgs args)
		{
			for (int nr = 0; nr < 10; nr++)
			{
				if (ppollutant[nr].SelectedIndex == 2)
					but1[nr].Text = "[MOU/h]";
				else
					but1[nr].Text = "[kg/h]";
			}
		}

		private void Comma1(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == ',') e.KeyChar = '.';
			int asc = (int)e.KeyChar; //get ASCII code
		}

		private void Comma2(object sender, KeyPressEventArgs e)
		{
			MessageBox.Show(this, "Use the drop down buttons to choose the source group","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
			int asc = (int)e.KeyChar; //get ASCII code
			switch (asc)
			{
				default:
					e.Handled = true; break;
			}
		}

		//increase the number of point sources by one
		private void button1_Click(object sender, EventArgs e)
		{
			if ((textBox1.Text != "") && (textBox2.Text != ""))
			{
				SaveArray();
				textBox2.Text = "";
				textBox3.Text = "";
				trackBar1.Maximum = trackBar1.Maximum + 1;
				trackBar1.Value = trackBar1.Maximum;
				ItemDisplayNr = trackBar1.Maximum - 1;
			}
		}

		//scroll between the point sources
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
			PointSourceData _pdata;
			if (ItemDisplayNr >= ItemData.Count) // new item
			{
				_pdata = new PointSourceData();
			}
			else // change existing item
			{
				_pdata = ItemData[ItemDisplayNr];
			}

		    bool valid_coors = true;

		    if (St_F.CheckDoubleValid(textBox2.Text) == false ||
		        St_F.CheckDoubleValid(textBox3.Text) == false)
		    {
		        valid_coors = false;
		    }

            if ((textBox1.Text != "") && valid_coors) // Name + valid coordinates
		    {
				for (int i = 0; i < 10; i++) // save pollutants
				{
					_pdata.Poll.Pollutant[i] = ppollutant[i].SelectedIndex;
                    _pdata.Poll.EmissionRate[i] = St_F.TxtToDbl(pemission[i].Text, false);
                }

				for (int i = 0; i < 10; i++) // save deposition
				{
					_pdata.GetDep()[i] = new Deposition(dep[i]);
				}

				string[] text2 = new string[2];
				text2 = comboBox1.Text.Split(new char[] { ',' });
				if (text2.Length > 1)
				{
					int.TryParse(text2[1], out sourcegroup);
				}
				else
				{
					int.TryParse(text2[0], out sourcegroup);
				}

				_pdata.Poll.SourceGroup = sourcegroup;

				_pdata.Height = Convert.ToSingle(numericUpDown1.Value, ic);
				if (checkBox1.Checked) // absolute height over sea
					_pdata.Height *= -1;


				_pdata.Name = St_F.RemoveinvalidChars(textBox1.Text);
				_pdata.Pt = new PointD(textBox2.Text, textBox3.Text, CultureInfo.CurrentCulture);
				_pdata.Velocity = (float) (numericUpDown2.Value);
				_pdata.Temperature = (float) (numericUpDown3.Value + 273);
				_pdata.Diameter = (float) (numericUpDown4.Value);

				_pdata.TemperatureTimeSeries = TempTimeSeries;
				_pdata.VelocityTimeSeries = VelTimeSeries;

				if (ItemDisplayNr >= ItemData.Count) // new item
				{
					ItemData.Add(_pdata);
				}
				else
				{
					ItemData[ItemDisplayNr] = _pdata;
				}

				RedrawDomain(this, null);

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
				PointSourceData _pdata;
				if (ItemDisplayNr < ItemData.Count)
				{
					_pdata = ItemData[ItemDisplayNr];
				}
				else
				{
					if (ItemData.Count > 0)
					{
						_pdata = new PointSourceData(ItemData[ItemData.Count - 1]);
					}
					else
					{
						_pdata = new PointSourceData();
					}
				}

				textBox1.Text = _pdata.Name;
				textBox2.Text = _pdata.Pt.X.ToString();
				textBox3.Text = _pdata.Pt.Y.ToString();

				if (_pdata.Height >= 0) // height above ground
				{
					checkBox1.Checked = false;
				}
				else // height above sea
				{
					checkBox1.Checked = true;
				}
				numericUpDown1.Value = St_F.ValueSpan(0, 7999, Math.Abs(_pdata.Height));

				numericUpDown2.Value = St_F.ValueSpan(0, 100, _pdata.Velocity);
				try
				{
					numericUpDown3.Value = St_F.ValueSpan(0, 1000, _pdata.Temperature - 273);
				}
				catch
				{
					numericUpDown3.Value = 0;
					MessageBox.Show(this, "Problems when reading temperature value - set to zero","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				numericUpDown4.Value = St_F.ValueSpan(0, 1000, _pdata.Diameter);

				combo(_pdata.Poll.SourceGroup);

				for (int i = 0; i < 10; i++)
				{
					ppollutant[i].SelectedIndex = _pdata.Poll.Pollutant[i];

					if (ppollutant[i].SelectedIndex == 2)
						but1[i].Text = "[MOU/h]";
					else
						but1[i].Text = "[kg/h]";

					labelpollutant[i].Text = ppollutant[i].Text;
					pemission[i].Text = St_F.DblToLocTxt(_pdata.Poll.EmissionRate[i]);
					but1[i].BackColor = SystemColors.ButtonFace;
				}

				for (int i = 0; i < 10; i++)
				{
					dep[i] = _pdata.GetDep()[i];

					if (dep[i].V_Dep1 > 0 || dep[i].V_Dep2 > 0 || dep[i].V_Dep3 > 0)
						but1[i].BackColor = Color.LightGreen; // mark that deposition is set
					else
						but1[i].BackColor = SystemColors.ButtonFace; // mark that deposition is reset
				}

				TempTimeSeries = _pdata.TemperatureTimeSeries;
				if (!string.IsNullOrEmpty(TempTimeSeries))
				{
				    button6.BackgroundImage = Gral.Properties.Resources.TimeSeriesSelected;
				}
				else
				{
				    button6.BackgroundImage = Gral.Properties.Resources.TimeSeries;
				}

				VelTimeSeries = _pdata.VelocityTimeSeries;
				if (!string.IsNullOrEmpty(VelTimeSeries))
				{
				    button5.BackgroundImage = Gral.Properties.Resources.TimeSeriesSelected;
				}
				else
				{
				    button5.BackgroundImage = Gral.Properties.Resources.TimeSeries;
				}
			}
			catch
			{

			}
		}

		//remove actual point source data
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
                    ask = false;
				else
					ask = true; // Cancel -> do not delete!
			}
			if (ask == false)
			{
				textBox1.Text = "";
				textBox2.Text = "";
				textBox3.Text = "";
				numericUpDown1.Value = 0;
				numericUpDown2.Value = 0;
				numericUpDown3.Value = 0;
				numericUpDown4.Value = 0;
				comboBox1.SelectedIndex = 0;
				for (int i = 0; i < 10; i++)
					pemission[i].Text = "0";
				
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
			}
		}

		//remove all point sources
		private void button3_Click(object sender, EventArgs e)
		{
			if (St_F.InputBox("Attention", "Delete all point sources??", this) == DialogResult.OK)
			{
				ItemData.Clear();
				ItemData.TrimExcess();

				textBox1.Text = "";
				textBox2.Text = "";
				textBox3.Text = "";
				numericUpDown1.Value = 0;
				numericUpDown2.Value = 0;
				numericUpDown3.Value = 0;
				numericUpDown4.Value = 0;
				comboBox1.SelectedIndex = 0;
				for (int i = 0; i < 10; i++)
				{
					but1[i].BackColor = SystemColors.ButtonFace;
				}
				trackBar1.Value = 1;
				trackBar1.Maximum = 1;
				ItemDisplayNr = trackBar1.Value - 1;
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

		void EditPointSourcesResizeEnd(object sender, EventArgs e)
		{
			int dialog_width = ClientSize.Width;

			if (dialog_width < Dialog_Initial_Width)
			{
				dialog_width = Dialog_Initial_Width;
			}

			if (dialog_width > 130)
			{
				dialog_width -= 12;
				groupBox1.Width = dialog_width - TabControl_x0;

				trackBar1.Width = dialog_width - TrackBar_x0;
				textBox1.Width = dialog_width - TextBox_x0;
				textBox2.Width = dialog_width - TextBox_x0;
				textBox3.Width = dialog_width - TextBox_x0;
				numericUpDown1.Width = dialog_width - Numericupdown_x0;
				numericUpDown2.Width = dialog_width - Numericupdown_x2;
				numericUpDown3.Width = dialog_width - Numericupdown_x2;
				numericUpDown4.Width = dialog_width - Numericupdown_x0;
				comboBox1.Width      = dialog_width - Numericupdown_x0;
			}

			int element_width = (groupBox1.Width - but1[1].Width - 20) / 2;
			groupBox1.Height = Math.Max(10, ClientSize.Height - groupBox1.Top - 5);
            button7.Left = (int)((dialog_width - button7.Width) * 0.5);

            if (element_width < TBWidth)
			{
				element_width = TBWidth;
			}

			for (int nr = 0; nr < 10; nr++)
			{
				ppollutant[nr].Width  = element_width;
				labelpollutant[nr].Width = element_width;
				pemission [nr].Location = new System.Drawing.Point (ppollutant[nr].Left + ppollutant[nr].Width + 5, ppollutant[nr].Top);
				pemission [nr].Width = element_width;
				but1[nr].Location = new System.Drawing.Point(pemission[nr].Left + pemission[nr].Width + 5, ppollutant[nr].Top - 1);
			}
		}

		void TextBox1Leave(object sender, EventArgs e)
		{

		}

		private void RedrawDomain(object sender, EventArgs e)
		{
			// send Message to domain Form, that redraw is necessary
			try
			{
				if (PointSourceRedraw != null)
					PointSourceRedraw(this, e);
			}
			catch
			{}
		}

		private void edit_deposition(object sender, EventArgs e)
		{
			int nr = 0;
			for (int i = 0; i < 10; i++)
			{
				if (sender == but1[i])
					nr = i;
			}
			using (EditDeposition edit = new EditDeposition
			{
			  	StartPosition = FormStartPosition.Manual
			})
			{
				if (Right < SystemInformation.PrimaryMonitorSize.Width / 2)
				{
					edit.Location = new Point(Right + 4, Top);
				}
				else
				{
					edit.Location = new Point(Left - 370, Top);
				}
				edit.Dep = dep[nr]; // set actual values
				edit.Emission = St_F.TxtToDbl(pemission[nr].Text, true);
				edit.Pollutant = ppollutant[nr].SelectedIndex;
				if (edit.ShowDialog() == DialogResult.OK)
					edit.Hide();
			}

			if (Main.Project_Locked == false)
			{
				if (dep[nr].V_Dep1 > 0 || dep[nr].V_Dep2 > 0 || dep[nr].V_Dep3 > 0)
					but1[nr].BackColor = Color.LightGreen; // mark that deposition is set
				else
					but1[nr].BackColor = SystemColors.ButtonFace;

				SaveArray(); // save values
			}
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

        private void EditPointSources_FormClosing(object sender, FormClosingEventArgs e)
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

        void EditPointSourcesFormClosed(object sender, FormClosedEventArgs e)
        {
        	foreach(TextBox tex in pemission)
        	{
        		tex.TextChanged -= new System.EventHandler(St_F.CheckInput);
        		tex.KeyPress -= new KeyPressEventHandler(St_F.NumericInput); //only point as decimal seperator is allowed
			}
        	foreach(Button but in but1)
        		but.Click -= new EventHandler(edit_deposition);

        	textBox2.TextChanged -= new System.EventHandler(St_F.CheckInput);
		    textBox3.TextChanged -= new System.EventHandler(St_F.CheckInput);
		    textBox2.KeyPress -= new KeyPressEventHandler(St_F.NumericInput); //only point as decimal seperator is allowed
			textBox3.KeyPress -= new KeyPressEventHandler(St_F.NumericInput); //only point as decimal seperator is allowed

        	comboBox1.KeyPress -= new KeyPressEventHandler(Comma2); //only point as decimal seperator is allowed
        	textBox1.KeyPress -= new KeyPressEventHandler(Comma1); //only point as decimal seperator is allowed

          	ItemData.Clear();
			ItemData.TrimExcess();

			for (int nr = 0; nr < 10; nr++)
			{
				ppollutant[nr].SelectedIndexChanged -= new System.EventHandler(Odour);
				ppollutant[nr].Items.Clear();
			}

			toolTip1.Dispose();
			comboBox1.Items.Clear();
			comboBox1.Dispose();
		}

        // store and reload the settings
        void Button4Click(object sender, EventArgs e)
        {
            SaveArray();
            FillValues();
        }

        void EditPointSourcesVisibleChanged(object sender, EventArgs e)
        {
        	if (!Visible)
			{
			}
			else // Enable/disable items
			{
				bool enable = !Main.Project_Locked;
				if (enable)
				{
					Text = "Edit point sources";
				}
				else
				{
					Text = "Point source settings (project locked)";
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
					ppollutant[i].Visible = enable;
					labelpollutant[i].Visible = !enable;
				}

			}
        }

        public void SetXCoorText(string _s)
        {
        	textBox2.Text = _s;
        }
        public void SetYCoorText(string _s)
        {
        	textBox3.Text = _s;
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

		// Time Series and Velocity
		void ButtonTSVelClick(object sender, EventArgs e)
		{
			bool Velocity = true;

			Button bt = sender as Button;
			if (bt.Name == "button6")
			{
				Velocity = false;
			}

			using (EditTimeSeriesValues edT = new EditTimeSeriesValues())
			{
				if (Velocity)
				{
					edT.FilePath = Path.Combine(Main.ProjectName, @"Emissions", "TimeSeriesPointSourceVel.tsd");
                    edT.InitValue = Convert.ToDouble(numericUpDown2.Value);
                    edT.SelectedTimeSeries = VelTimeSeries;
                    edT.TitleBarText = "Exit Velocity [m/s] - Time Series - GRAL Transient Mode";
				}
				else
				{
					edT.FilePath = Path.Combine(Main.ProjectName, @"Emissions", "TimeSeriesPointSourceTemp.tsd");
                    edT.InitValue = Convert.ToDouble(numericUpDown3.Value);
					edT.SelectedTimeSeries = TempTimeSeries;
                    edT.TitleBarText = "Exit Temperature Above Ambient Temperature [K] - Time Series - GRAL Transient Mode";
                }
				edT.StartPosition = FormStartPosition.Manual;


				if (Right < SystemInformation.PrimaryMonitorSize.Width / 2)
				{
					edT.Location = new Point(Right + 4, Top);
				}
				else
				{
					edT.Location = new Point(Left - 750, Top);
				}

				if (edT.ShowDialog() == DialogResult.OK)
				{
					edT.Hide();
					if (Velocity)
					{
						VelTimeSeries = edT.SelectedTimeSeries;
						numericUpDown2.Value = Convert.ToDecimal(edT.MeanValue);
						button5.BackgroundImage = Gral.Properties.Resources.TimeSeriesSelected;
					}
					else
					{
						TempTimeSeries = edT.SelectedTimeSeries;
						numericUpDown3.Value = Convert.ToDecimal(edT.MeanValue);
						button6.BackgroundImage = Gral.Properties.Resources.TimeSeriesSelected;
					}

				}
				else // cancel
				{
					if (Velocity)
					{
						VelTimeSeries = string.Empty;
						button5.BackgroundImage = Gral.Properties.Resources.TimeSeries;
					}
					else
					{
						TempTimeSeries = string.Empty;
						button6.BackgroundImage = Gral.Properties.Resources.TimeSeries;
					}
				}
			}
		}

        private void button7_Click(object sender, EventArgs e)
        {
            this.Close(); // does not close the form, because closing hides the form
        }
    }
}
