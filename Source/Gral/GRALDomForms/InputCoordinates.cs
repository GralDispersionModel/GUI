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
 * Date: 09.07.2015
 * Time: 17:21
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.Globalization;
using GralStaticFunctions;

namespace GralDomForms
{
	/// <summary>
	/// Simple input form for coordinates
	/// </summary>
	public partial class InputCoordinates : Form
	{
		public string S_X;
		public string S_Y;
		public bool Show_at_Mouse_Position = true;
		
		public InputCoordinates(string x, string y)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			S_X = x;
			S_Y = y;
			Input_x.Text = x;
			Input_y.Text = y;
		}
		
		void InputOkClick(object sender, EventArgs e)
		{
			// Check, if string is a valid number
			decimal num = 0;
			if (decimal.TryParse(Input_x.Text, out num))
			{}
			else
				Input_x.Text = S_X;
			if (decimal.TryParse(Input_y.Text, out num))
			{}
			else
				Input_y.Text = S_Y;
			
		}
		
		void InputXKeyPress(object sender, KeyPressEventArgs e)
		{
			char decsep = Convert.ToChar(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
			if (!char.IsDigit(e.KeyChar ) && !char.IsControl(e.KeyChar) && e.KeyChar != decsep && e.KeyChar != '-')
				e.Handled = true;

		}
		
		void InputYKeyPress(object sender, KeyPressEventArgs e)
		{
			char decsep = Convert.ToChar(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
			if (!char.IsDigit(e.KeyChar ) && !char.IsControl(e.KeyChar) && e.KeyChar != decsep && e.KeyChar != '-')
				e.Handled = true;
		}
		
		void InputCoordinatesLoad(object sender, EventArgs e)
		{
			if (Show_at_Mouse_Position)
			{
				System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
				Left = point.X-10;
				Top = point.Y-10;
			}
			#if __MonoCS__
			this.Height = 140;
			#endif
		}
		
		void InputXTextChanged(object sender, EventArgs e)
		{
			St_F.CheckInput(sender, e);
		}
		
		void InputYTextChanged(object sender, EventArgs e)
		{
			St_F.CheckInput(sender, e);
		}
	}
}
