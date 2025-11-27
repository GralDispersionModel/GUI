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

using GralStaticFunctions;
using System;
using System.Windows.Forms;

namespace GralMainForms
{
    /// <summary>
    /// Form for user defined compost settings
    /// </summary>
    public partial class Kompost : Form
    {
        public Kompost()
        {
            InitializeComponent();

            button1.DialogResult = DialogResult.OK;
            textBox1.KeyPress += new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed
            textBox2.KeyPress += new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed
            textBox3.KeyPress += new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed
            textBox4.KeyPress += new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed
            textBox5.KeyPress += new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed
            textBox6.KeyPress += new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed
            textBox7.KeyPress += new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed

            textBox1.TextChanged += new System.EventHandler(St_F.CheckInput);
            textBox2.TextChanged += new System.EventHandler(St_F.CheckInput);
            textBox3.TextChanged += new System.EventHandler(St_F.CheckInput);
            textBox4.TextChanged += new System.EventHandler(St_F.CheckInput);
            textBox5.TextChanged += new System.EventHandler(St_F.CheckInput);
            textBox6.TextChanged += new System.EventHandler(St_F.CheckInput);
            textBox7.TextChanged += new System.EventHandler(St_F.CheckInput);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void Kompost_Load(object sender, EventArgs e)
        {

        }

        void KompostFormClosed(object sender, FormClosedEventArgs e)
        {
            textBox1.KeyPress -= new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed
            textBox2.KeyPress -= new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed
            textBox3.KeyPress -= new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed
            textBox4.KeyPress -= new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed
            textBox5.KeyPress -= new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed
            textBox6.KeyPress -= new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed
            textBox7.KeyPress -= new KeyPressEventHandler(St_F.NumericInput);  //only point as decimal seperator is allowed

            textBox1.TextChanged -= new System.EventHandler(St_F.CheckInput);
            textBox2.TextChanged -= new System.EventHandler(St_F.CheckInput);
            textBox3.TextChanged -= new System.EventHandler(St_F.CheckInput);
            textBox4.TextChanged -= new System.EventHandler(St_F.CheckInput);
            textBox5.TextChanged -= new System.EventHandler(St_F.CheckInput);
            textBox6.TextChanged -= new System.EventHandler(St_F.CheckInput);
            textBox7.TextChanged -= new System.EventHandler(St_F.CheckInput);
        }
    }
}