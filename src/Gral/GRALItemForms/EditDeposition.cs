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
 * Date: 11.10.2016
 * Time: 10:15
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using GralStaticFunctions;
using GralItemData;
using Gral;

namespace GralItemForms
{
    public partial class EditDeposition : Form
	{
    	private Deposition  _dep;
		public Deposition Dep {set {_dep = value;} get {return _dep;} }
				
		private double  _emission;
		public double Emission {set {_emission = value;} get {return _emission;}}
		
		private int  _pollutant;
		public int Pollutant {set {_pollutant = value;}}
		
		private List<Deposition_Settings> _depo_Settings = new List<Deposition_Settings>();
	    
        /// <summary>
        /// Deposition Dialog
        /// </summary>		
		public EditDeposition()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			_dep.Frac_30 = 100;
			_dep.Frac_2_5 = (int) numericUpDown2.Value;
			_dep.Frac_10  = (int) numericUpDown3.Value;
			_dep.DM_30    = (int) numericUpDown1.Value;
			
			_dep.Density = Convert.ToDouble(numericUpDown5.Value) * 1000; // g/cm³ to kg/m³
			
			_dep.V_Dep1 = Convert.ToDouble(numericUpDown6.Value);
			_dep.V_Dep2 = Convert.ToDouble(numericUpDown7.Value);
			_dep.V_Dep3 = Convert.ToDouble(numericUpDown8.Value);
			
			int i = 1; // PM2,5 = emission
			if (checkBox2.Checked) i = 2; // PM10 = emission 
			if (checkBox3.Checked) i = 3; // PM30 = emission
			_dep.Conc   = i;
		}
		
		void Button3Click(object sender, EventArgs e)
		{
			_dep.init(); // set all values to 0
		}
		
		void Edit_DepositionLoad(object sender, EventArgs e)
		{
		    
		    #if __MonoCS__
            // set Numericupdowns-Alignement to Left in Linux
            var allNumUpDowns = Main.GetAllControls<NumericUpDown>(this);
            foreach (NumericUpDown nu in allNumUpDowns)
            {
                nu.TextAlign = HorizontalAlignment.Left;
            }
            #else
			#endif
		    
			if (_pollutant == 2)  // don't edit odour -> no deposition 
			{
				_dep.init(); // set all values to 0
				Close();
			}
			
			if (_dep.Frac_30 == 0) init_pollutant(0);
			else init_pollutant(1);

            // read deposition settings if available from application path
			string settings = Path.Combine(Main.App_Settings_Path, "DepositionSettings.txt");
			if (File.Exists(settings))
			{
                try
				{
					string[] text;
					using(StreamReader myReader = new StreamReader(settings))
					{
						myReader.ReadLine(); // 1st Line = Title
						while (myReader.EndOfStream == false) // read until EOF
						{
							text = myReader.ReadLine().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
							Deposition_Settings i = new Deposition_Settings();
							
							if (text.Length > 6)
							{
								i.Title = text[0];
                                i.Frac_25 = Convert.ToInt32(text[1].Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
								i.Frac_10 = Convert.ToInt32(text[2].Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
								i.DM_30 = Convert.ToInt32(text[3].Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
								i.Density = Convert.ToDouble(text[4].Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
								i.V_Dep1 = Convert.ToDouble(text[5].Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
								i.V_Dep2 = Convert.ToDouble(text[6].Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
								i.V_Dep3 = Convert.ToDouble(text[7].Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                                
								listBox1.Items.Add(i.Title);
								_depo_Settings.Add(i);
							}
						} 	
					}
				}
				catch{}
			}
			
			numericUpDown1.Value = St_F.ValueSpan(11, 1000, _dep.DM_30);
			numericUpDown2.Value = St_F.ValueSpan(0, 100, _dep.Frac_2_5);
			numericUpDown3.Value = St_F.ValueSpan(0, 100,_dep.Frac_10);
			
			label15.Text = Convert.ToString(_dep.Frac_30);
			
			numericUpDown5.Value = Convert.ToDecimal(_dep.Density / 1000); // kg/m³ to g /cm³
			numericUpDown6.Value = Convert.ToDecimal(_dep.V_Dep1);
			numericUpDown7.Value = Convert.ToDecimal(_dep.V_Dep2);
			numericUpDown8.Value = Convert.ToDecimal(_dep.V_Dep3);
			
			NumericUpDown2ValueChanged(null, null);
			
			numericUpDown2.ValueChanged += new EventHandler(NumericUpDown2ValueChanged);
			numericUpDown3.ValueChanged += new EventHandler(NumericUpDown2ValueChanged);
			
			if (_dep.Conc == 2) checkBox2.Checked = true;
			if (_dep.Conc == 3)
			{
				checkBox2.Checked = true;
				checkBox3.Checked = true;
			}
			
		}
		
		void init_pollutant(int type)
		{
			if (type == 0 && Main.Project_Locked == false) // first intialization
			{
				_dep.Frac_30 = 100;
				_dep.DM_30 = 30;
				_dep.Density = 0;
				_dep.Frac_2_5 = 100;
				_dep.Frac_10 = 100;
			}
			numericUpDown1.Enabled = false;
			numericUpDown2.Enabled = false;
			numericUpDown3.Enabled = false;
			//checkBox1.Checked = false;
			checkBox2.Checked = false;
			checkBox3.Checked = false;
			checkBox2.Enabled = false;
			checkBox3.Enabled = false;		
			listBox1.Enabled = false;			
			
			switch (_pollutant)
			{
				case 0: // NOx
				case 6: // NO2
					if (type == 0) _dep.V_Dep1 = 0.003;
					_dep.Conc    = 1;
					numericUpDown5.Enabled = false;
					numericUpDown7.Enabled = false;
					numericUpDown8.Enabled = false;
					break;
				case 5: // NH3
					if (type == 0) _dep.V_Dep1 = 0.01;
					_dep.Conc    = 1;
					numericUpDown5.Enabled = false;
					numericUpDown7.Enabled = false;
					numericUpDown8.Enabled = false;
					break;
				case 4: // PM 2,5
					if (type == 0)
					{
						_dep.V_Dep1 = 0.001;
						_dep.V_Dep2 = 0.01;
						_dep.V_Dep3 = 0.05;
						_dep.Frac_2_5 = 100;
						_dep.Density = 1800;
						_dep.Conc    = 1;
					}
					numericUpDown1.Enabled = true;
					numericUpDown2.Enabled = true;
					numericUpDown3.Enabled = true;
					listBox1.Enabled = true;
					break;
				case 1: // PM10
					if (type == 0)
					{
						_dep.V_Dep1 = 0.001;
						_dep.V_Dep2 = 0.01;
						_dep.V_Dep3 = 0.05;
						_dep.Frac_2_5 = 25;
						_dep.Density = 1800;
					}
					numericUpDown1.Enabled = true;
					numericUpDown2.Enabled = true;
					numericUpDown3.Enabled = true;
					listBox1.Enabled = true;
					_dep.Conc    = 2;
					break;
				case 3: // SO2
					if (type == 0)
						_dep.V_Dep1 = 0.01;
					_dep.Conc    = 1;
					numericUpDown5.Enabled = false;
					numericUpDown7.Enabled = false;
					numericUpDown8.Enabled = false;
					break;
				
				case 15: //bp
				case 17: //Ni
					if (type == 0)
					{
						_dep.V_Dep1 = 0.001;
						_dep.V_Dep2 = 0.01;
						_dep.V_Dep3 = 0.05;
						_dep.Frac_2_5 = 100;
						if (_pollutant == 15) _dep.Density = 11430;
						if (_pollutant == 17) _dep.Density = 8900;
						_dep.Conc    = 2;
					}
					numericUpDown1.Enabled = true;
					numericUpDown2.Enabled = true;
					numericUpDown3.Enabled = true;
					listBox1.Enabled = true;
					break;
				case 19: //HG
					if (type == 0)
						_dep.V_Dep1 = 0.005;
					_dep.Conc    = 1;
					numericUpDown5.Enabled = true;
					numericUpDown7.Enabled = false;
					numericUpDown8.Enabled = false;
					break;
					
				default:
					 break;
			}				
			
		}
		
		void NumericUpDown2ValueChanged(object sender, EventArgs e)
		{
			if (numericUpDown3.Value < numericUpDown2.Value)
				numericUpDown2.Value = numericUpDown3.Value;
			
			if (numericUpDown2.Value > numericUpDown3.Value)
				numericUpDown3.Value = numericUpDown2.Value;
			
			
			//MessageBox.Show(Convert.ToString(_poll) + "/" +Convert.ToString(_em));
			
			float p1 = (float) numericUpDown2.Value;
			float p2 = (float) numericUpDown3.Value;
			float p3 = 100;
			int comma = 5;
			if (_emission > 0)
				comma = Math.Max(0, Convert.ToInt32(5 - (Math.Log10(_emission) + 1)));
			
			if (_dep.Conc == 1) // PM 2.5
			{
				float em100 = (float) _emission * 100 / p1; // 100 % of emission
				label12.Text = Convert.ToString(Math.Round(em100 * p1 / 100, comma)); // PM 2,5
				label13.Text = Convert.ToString(Math.Round(em100 * (p2 - p1) / 100, comma));
				label14.Text = Convert.ToString(Math.Round(em100 * (p3 - p2) / 100, comma));
			}
			else if (_dep.Conc == 2) // PM 10
			{
				float em100 = (float) _emission * 100 / p2; // 100 % of emission
				label12.Text = Convert.ToString(Math.Round(em100 * p1 / 100, comma)); // PM 2,5
				label13.Text = Convert.ToString(Math.Round(em100 * p2 / 100 - em100 * p1 / 100, comma));
				label14.Text = Convert.ToString(Math.Round(em100 * (p3 - p2) / 100, comma));
			}
			
		}
		
		
		void CheckBox3CheckedChanged(object sender, EventArgs e)
		{
			if (checkBox3.Checked) checkBox2.Checked = true;
		}
		
		
		void CheckBox2CheckedChanged(object sender, EventArgs e)
		{
			if (checkBox2.Checked == false) checkBox3.Checked = false;
		}
		
		void ListBox1SelectedIndexChanged(object sender, EventArgs e)
		{
			for (int i = 0; i < listBox1.Items.Count; i++)
			{
				if (listBox1.SelectedItem.ToString() == _depo_Settings[i].Title)
				{
					numericUpDown3.ValueChanged -= new System.EventHandler(NumericUpDown2ValueChanged);
					numericUpDown3.Value = (int) _depo_Settings[i].Frac_10;
					numericUpDown2.Value = (int) _depo_Settings[i].Frac_25;
					numericUpDown3.ValueChanged += new EventHandler(NumericUpDown2ValueChanged);
					NumericUpDown2ValueChanged(null, null);
					numericUpDown5.Value = Convert.ToDecimal(_depo_Settings[i].Density / 1000d); // kg/m³ to g/cm³
					numericUpDown1.Value = (int) _depo_Settings[i].DM_30;
					numericUpDown6.Value = Convert.ToDecimal(_depo_Settings[i].V_Dep1);
					numericUpDown7.Value = Convert.ToDecimal(_depo_Settings[i].V_Dep2);
					numericUpDown8.Value = Convert.ToDecimal(_depo_Settings[i].V_Dep3);

					break;
				}
			}
		}
		
		void TextBox6TextChanged(object sender, EventArgs e)
		{
			St_F.CheckInput(sender, e); // check if a valid number or show yellow background
		}
		
		void Edit_DepositionFormClosed(object sender, FormClosedEventArgs e)
		{
			toolTip1.Dispose();
			panel1.Dispose();
			panel2.Dispose();
			panel3.Dispose();
		}
		
		
		void Edit_DepositionVisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
			}
			else // Enable/disable items
			{
				bool enable = !Main.Project_Locked;
                if (enable)
                {
                    Text = "Dry deposition";
                }
                else
                {
                    Text = "Dry deposition (project locked)";

                    foreach (Control c in Controls)
                    {
                        if (c != button1)
                        {
                            c.Enabled = enable;
                        }
                    }
                }
			}
		}
		
	}
	
	
}
