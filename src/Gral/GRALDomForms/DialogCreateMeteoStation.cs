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
 * Date: 18.07.2016
 * Time: 15:44
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.IO;

namespace GralDomForms
{
    public delegate void StartCreateMeteoStation(object sender, EventArgs e);
    public delegate void CancelCreateMeteoStation(object sender, EventArgs e);
    
    /// <summary>
    /// Dialog to start the Meteo Evaluation process
    /// </summary>
    public partial class DialogCreateMeteoStation : Form
    {

        public string Meteo_Title;
        public string Meteo_Init;
        public string Meteo_Ext;
        public string Meteo_Prefix;
        public int Meteo_Height;
        public bool ShowAbsHeightBox = false;
        public bool AbsoluteHeight = false;
        /// <summary>
        /// Values: 0 - GRAMM Windfield,  Bit 1 GRAMM only, Bit 2 GRAL only, Bit 4 Filename / Height only Bit 5: Filename Height / only / Height-String: Max. absolute height
        /// </summary>
        public int Meteo_Model = 0;
        public int X1;
        public int Y1;
        public double Xs;
        public double Ys;
        public double X0;
        public double Y0;
        /// <summary>
        /// true: allow selection of local stability classes
        /// </summary>
        public bool Local_Stability = false;
        /// <summary>
        /// true: allow an evaluation at receptor points
        /// </summary>
        public bool Receptor_Points = false;
        
        // delegate to send Message, that function should start
        public event StartCreateMeteoStation Start_computation;
        // delegate to send Message, that function is cancelled!
        public event CancelCreateMeteoStation Cancel_computation;
        
        // Meteo Model Values: 0 - GRAMM Windfield,  Bit 1 GRAMM only, Bit 2 GRAL only, Bit 4 Filename & Height only
        // Bit 5: Filename & Height only & Height-String: Max. absolute height
        
        /// <summary>
        /// Dialog to start the Meteo Evaluation process
        /// </summary>
        /// <para>Meteo_Model: 0 - GRAMM Windfield,  Bit 1 GRAMM only, Bit 2 GRAL only, Bit 4 Filename and Height only
        /// Bit 5: Filename and Height only and Height-String: Max. absolute height 
        /// Local_Stability: true: allow selection of local stability classes</para>
        /// Receptor_Points: true: allow an evaluation at receptor points</para>
        public DialogCreateMeteoStation()
        {
            InitializeComponent();
        }
        
        
        void Form1Load(object sender, EventArgs e)
        {
            #if __MonoCS__
                numericUpDown1.TextAlign = HorizontalAlignment.Left;
            #endif
            
            TopMost = true;
            Text = Meteo_Title;
            textBox1.Text = Meteo_Init;
            numericUpDown1.Value = Convert.ToDecimal(Meteo_Height);
            Top = Y1;
            Left = X1;
            
            if (Meteo_Model == 0) // No selection of GRAMM or GRAL Model
            {
                groupBox1.Visible = false;
                groupBox2.Visible = true;
                groupBox2.Top = groupBox1.Top;
                groupBox2.Left = groupBox1.Left;
            }
            else
            {
                groupBox1.Visible = true;
                groupBox2.Visible = false;
                radioButton1.Visible = false;
                radioButton2.Visible = false;
                
                if ((Meteo_Model & 1) == 1) // GRAMM
                {
                    radioButton1.Visible = true;
                    radioButton1.Checked = true;
                    label2.Text = "Height above ground";
                    Hide_Coor();
                }
                if ((Meteo_Model & 2) == 2) // GRAL
                {
                    radioButton2.Visible = true;
                    if (radioButton1.Visible == false) 
                    {
                        radioButton2.Checked = true;
                        label2.Text = "Height above lowest" + Environment.NewLine + "model elevation";
                    }
                    Hide_Coor();
                }
                if ((Meteo_Model & 4) == 4) // Filename & Height only
                {
                    radioButton1.Visible = true;
                    radioButton1.Checked = true;
                    Hide_Coor();
                }
                if (Meteo_Model == 32) // Filename & Height only
                {
                    radioButton1.Visible = true;
                    radioButton1.Checked = true;
                    Hide_Coor();
                    checkBox1.Visible = false;
                    groupBox1.Visible = false;
                    label1.Text = "Save vertical concentration profile as";
                    label2.Text = "Max. height [m]";
                    numericUpDown1.Value = Convert.ToDecimal(Meteo_Height);
                }
            }
                
            if (Local_Stability) // true
            {
                checkBox1.Enabled = true;
                checkBox1.Checked = true;
            }
            else
            {
                checkBox1.Checked = false;
                checkBox1.Enabled = false;
            }

            if (Receptor_Points) // true
            {
                groupBox2.Enabled = true;
            }
            else
            {
                groupBox2.Enabled = false;
            }

            if(ShowAbsHeightBox)
            {
                checkBox5.Visible = true;
            }

            if (AbsoluteHeight && (Meteo_Model & 1) == 1) // GRAMM must be available
            {
                checkBox5.Checked = true;
            }
        }
        
        void Hide_Coor()
        {
            label7.Visible = false;
            label8.Visible = false;
            X_Coor.Visible = false;
            Y_Coor.Visible = false;
        }
        
        // OK Button
        void Button1Click(object sender, EventArgs e)
        {
            Meteo_Init = textBox1.Text;
            if (!Meteo_Init.EndsWith(Meteo_Ext))
            {
                Meteo_Init += Meteo_Ext;
            }
            // if Meteo_Init contains invalid characters for a file!					
            Meteo_Init = string.Join("_", Meteo_Init.Split(Path.GetInvalidFileNameChars()));
            Meteo_Height = Convert.ToInt32(numericUpDown1.Value);
            if (radioButton1.Checked)
            {
                Meteo_Model = 1;
            }
            else if (radioButton2.Checked)
            {
                Meteo_Model = 2;
            }

            if (checkBox1.Checked)
            {
                Local_Stability = true;
            }
            else
            {
                Local_Stability = false;
            }

            if (checkBox2.Checked)
            {
                Receptor_Points = true;
                Meteo_Init = textBox2.Text;
            }
            else
            {
                Receptor_Points = false;
            }
            
            // send Message to domain Form, that computation should start
            try
            {
                if (Start_computation != null)
                {
                    Start_computation(this, e);
                }
            }
            catch
            {}
        }

        //Receptor point evaluation selected yes/no
        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox2.Checked==true)
            {
                label1.Enabled = false;
                textBox1.Enabled = false;
                numericUpDown1.Enabled = false;
                X_Coor.Visible = false;
                Y_Coor.Visible = false;
            }
            else
            {
                label1.Enabled = true;
                textBox1.Enabled = true;
                numericUpDown1.Enabled = true;
                X_Coor.Visible = true;
                Y_Coor.Visible = true;
            }
        }

        void Button2Click(object sender, EventArgs e)
        {
            // send Message to domain Form, that computation is cancelled
            try
            {
                if (Cancel_computation != null)
                {
                    Cancel_computation(this, e);
                }
            }
            catch
            {}
            Close();
            Dispose();
        }
        void Dialog_CreateMeteoStationFormClosed(object sender, FormClosedEventArgs e)
        {
            toolTip1.Dispose();
        }
        
        void RadioButton1CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                label2.Text = "Height above ground";
                checkBox5.Checked = false;
                checkBox5.Enabled = true;
            }
        }
        void RadioButton2CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                label2.Text = "Height above lowest" + Environment.NewLine + "model elevation";
                checkBox5.Checked = false;
                checkBox5.Enabled = false;
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                label2.Text = "Height above sea (abs)";
            }
            else
            {
                label2.Text = "Height above ground";
            }
            AbsoluteHeight = checkBox5.Checked;
        }
    }
}
