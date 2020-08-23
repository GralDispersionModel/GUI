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
 * User: Markus_2
 * Date: 14.02.2016
 * Time: 16:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;

namespace GralDomForms
{
    /// <summary>
    /// Settings for the 3D visualization
    /// </summary>    
    public partial class Dialog_3D : Form
	{
    	private bool _sm = true;
        /// <summary>
        /// Smoothing the 3D view?
        /// </summary>
    	public bool Smoothing { set {_sm = value;} get{return _sm;}}
        /// <summary>
        /// Vertical stretching factor
        /// </summary>
		public double VertFactor { get; set; }
        /// <summary>
        /// GRAL topography available?
        /// </summary>
		public bool GRAL_Topo { get; set; }
				
		public Dialog_3D()
		{
			InitializeComponent();
			ID_OK.DialogResult = DialogResult.OK;
			ID_CANCEL.DialogResult = DialogResult.Cancel;
	    }
		
		void Form1Load(object sender, EventArgs e)
		{
			checkBox1.Checked = _sm;
			
			numericUpDown1.Value = Convert.ToDecimal(VertFactor);
			if (GRAL_Topo)
			{
				radioButton2.Enabled = true;
				radioButton2.Checked = true;
			}
			else
			{
				radioButton2.Enabled = false;
				radioButton1.Checked = true;
			}
			
		}
		
		void ID_OKClick(object sender, EventArgs e)
		{
			VertFactor = Convert.ToDouble(numericUpDown1.Value);
			if (checkBox1.Checked)
            {
                _sm = true;
            }
            else
            {
                _sm = false;
            }

            GRAL_Topo = false;
			if (radioButton2.Checked)
            {
                GRAL_Topo = true;
            }

            Close();
		}
		
		
		void ID_CANCELClick(object sender, EventArgs e)
		{
			Close();
		}
	}
}
