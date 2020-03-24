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
using GralStaticFunctions;

namespace GralDomForms
{
	public delegate void Georeference2_Closed(object sender, EventArgs e);
	
	/// <summary>
    /// Dialog geo referencing using 2 points
    /// </summary>
    public partial class Georeference2 : Form
    {
        public double XMouse;
        public double YMouse;
        // delegate to send Message, that georeference2 is closed!
		public event Georeference2_Closed Form_Georef2_Closed;

        public Georeference2()
        {
            InitializeComponent();
            MouseMove += new MouseEventHandler(Aktiv);
            textBox1.TextChanged += new System.EventHandler(St_F.CheckInput);
            textBox2.TextChanged += new System.EventHandler(St_F.CheckInput);
            textBox8.TextChanged += new System.EventHandler(St_F.CheckInput);
            textBox9.TextChanged += new System.EventHandler(St_F.CheckInput);

            textBox1.KeyPress += new KeyPressEventHandler(St_F.NumericInput);
            textBox2.KeyPress += new KeyPressEventHandler(St_F.NumericInput);
			textBox8.KeyPress += new KeyPressEventHandler(St_F.NumericInput);
			textBox9.KeyPress += new KeyPressEventHandler(St_F.NumericInput);

        }

        private void Aktiv(object sender, MouseEventArgs e)
        {
            Activate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (textBox6.Text == "0")
            {
                textBox6.Text = Convert.ToString(XMouse);
                textBox7.Text = Convert.ToString(YMouse);
                XMouse = 0;
                YMouse = 0;                
            }
            else if (textBox4.Text == "0")
            {
                textBox4.Text = Convert.ToString(XMouse);
                textBox3.Text = Convert.ToString(YMouse);
                XMouse = 0;
                YMouse = 0;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox6.Text = "0";
            textBox7.Text = "0";
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            textBox3.Text = "0";
            textBox4.Text = "0";
        }
        
        void TextBox6TextChanged(object sender, EventArgs e)
        {
        	St_F.CheckInput(sender, e); // check if a valid number or show yellow background
        }
        
       	void Button2Click(object sender, EventArgs e)
        {
        	try
			{
				if (Form_Georef2_Closed != null)
					Form_Georef2_Closed(this, e); // call function closed()
			}
			catch{}
        }
        
        void Georeference2Load(object sender, EventArgs e)
        {
        	#if __MonoCS__
				this.Height = 400;
			#endif	
        }
        
        void Georeference2FormClosed(object sender, FormClosedEventArgs e)
        {
        	MouseMove -= new MouseEventHandler(Aktiv);
        	textBox1.TextChanged -= new System.EventHandler(St_F.CheckInput);
            textBox2.TextChanged -= new System.EventHandler(St_F.CheckInput);
            textBox8.TextChanged -= new System.EventHandler(St_F.CheckInput);
            textBox9.TextChanged -= new System.EventHandler(St_F.CheckInput);
            textBox1.KeyPress -= new KeyPressEventHandler(St_F.NumericInput);
            textBox2.KeyPress -= new KeyPressEventHandler(St_F.NumericInput);
			textBox8.KeyPress -= new KeyPressEventHandler(St_F.NumericInput);
			textBox9.KeyPress -= new KeyPressEventHandler(St_F.NumericInput);
        }
    }
}