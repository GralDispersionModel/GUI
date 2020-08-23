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
 * User: Markus
 * Date: 14.10.2017
 * Time: 18:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;

namespace GralDomForms
{
    /// <summary>
    /// Dialog for the modification of GRAL high resolution topography
    /// </summary>
    public partial class DialogModifyGRALTopography : Form
	{
		public GralData.TopoModifyClass modify;
		
		public DialogModifyGRALTopography()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void DialogModifyGRALTopographyLoad(object sender, EventArgs e)
		{
		    #if __MonoCS__
            // set Numericupdowns-Alignement to Left in Linux
            var allNumUpDowns = Gral.Main.GetAllControls<NumericUpDown>(this);
            foreach (NumericUpDown nu in allNumUpDowns)
            {
                nu.TextAlign = HorizontalAlignment.Left;
            }
            #else
			#endif
			
			numericUpDown1.Value = Convert.ToDecimal(modify.Height);
			numericUpDown2.Value = Convert.ToDecimal(modify.Hmax);
			numericUpDown1.Value = Convert.ToDecimal(modify.Hmin);
			
			checkBox1.Checked = modify.AbsoluteHeight;
			
			if ((modify.Raster) < comboBox1.Items.Count && (modify.Raster) > 0)
            {
                comboBox1.SelectedIndex = modify.Raster;
            }
            else
            {
                comboBox1.SelectedIndex = 0;
            }
        }
		
		// OK
		void Button2Click(object sender, EventArgs e)
		{
			modify.Height = (float) (numericUpDown1.Value);
			modify.AbsoluteHeight = checkBox1.Checked;
			modify.Raster = comboBox1.SelectedIndex;
			modify.Hmax = (float) (numericUpDown2.Value);
			modify.Hmin = (float) (numericUpDown3.Value);
		}
		
		// Cancel
		void Button1Click(object sender, EventArgs e)
		{
			
		}
		
		void DialogModifyGRALTopographyFormClosed(object sender, FormClosedEventArgs e)
		{
		
		}
		
		void CheckBox1CheckedChanged(object sender, EventArgs e)
		{
			if (checkBox1.Checked)
			{
				numericUpDown2.Enabled = false;
				numericUpDown3.Enabled = false;
				label3.Enabled = false;
				label4.Enabled = false;
				label1.Text = "Height [m] (abs)";
			}
			else
			{
				numericUpDown2.Enabled = true;
				numericUpDown3.Enabled = true;
				label3.Enabled = true;
				label4.Enabled = true;
				label1.Text = "Height [m] (rel)";
			}
				
		}
	}
}
