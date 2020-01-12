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
using System.Windows.Forms;
using System.Globalization;

namespace GralShape
{
	/// <summary>
    /// Add a column to a shape file data table
    /// </summary>
    public partial class ShapeImport_AddColumn : Form
    {
        public string decsep;

        public ShapeImport_AddColumn()
        {
            InitializeComponent();
            //User defined column seperator and decimal seperator
            decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            //only point as decimal seperator is allowed
            //textBox2.KeyPress += new KeyPressEventHandler(comma);
            button2.DialogResult = DialogResult.OK;
        }

        //close form, do nothing
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        //add column and perform math operation if required
        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
        }

    }
}