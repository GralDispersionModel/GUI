#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2022]  [Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using System;
using System.Globalization;
using System.Windows.Forms;

namespace Gral.GRALDomForms
{
    public partial class MathrasterNOxConversion : Form
    {
        public string SelectedEquation = string.Empty;
        private double NO2Value = 0;

        public MathrasterNOxConversion()
        {
            InitializeComponent();
            radioButton1.Checked = true;
#if __MonoCS__
            // set Numericupdowns-Alignement to Left in Linux
            numericUpDown1.TextAlign = HorizontalAlignment.Left;
#else
#endif
        }

        /// <summary>
        /// OK Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            string NOx = Convert.ToString(numericUpDown1.Value, CultureInfo.InvariantCulture);
            if (radioButton1.Checked)
            {
                SelectedEquation = "29*(A+" + NOx + ")/(35+A+" + NOx + ")+0.217*(A+" + NOx + ")";
            }
            else if (radioButton2.Checked)
            {
                SelectedEquation = "(A+" + NOx + ") * (49/(A+" + NOx + "+65)+0.12)";
            }
            else if (radioButton3.Checked)
            {
                SelectedEquation = "69.22*(A+" + NOx + ") / (76.27+A+" + NOx + ")+0.0561*(A+" + NOx + ")";
            }
            else if (radioButton4.Checked)
            {
                SelectedEquation = "103*(A+" + NOx + ") / (130+A+" + NOx + ")+0.005*(A+" + NOx + ")";
            }
            if (checkBox1.Checked)
            {
                numericUpDown1_ValueChanged(null, null);
                SelectedEquation += "-" + Math.Round(NO2Value, 2).ToString(CultureInfo.InvariantCulture);
            }
            this.Close();
        }

        /// <summary>
        /// Cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            SelectedEquation = string.Empty;
            this.Close();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            double temp = Convert.ToDouble(numericUpDown1.Value);

            if (radioButton1.Checked)
            {
                temp = 29 * temp / (35 + temp) + 0.217 * temp;
            }
            else if (radioButton2.Checked)
            {
                temp =  temp * (49/(temp + 65) + 0.12);
            }
            else if (radioButton3.Checked)
            {
                temp = 69.22 * temp / (76.27 + temp) + 0.0561 * temp;
            }
            else if (radioButton4.Checked)
            {
                temp = 103 * temp / (130 + temp) + 0.005 * temp;
            }

            label2.Text = Convert.ToString(Math.Round(temp, 1)) + " " + Gral.Main.My_p_m3;
            NO2Value = temp;
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            string NOx = Convert.ToString(numericUpDown1.Value);
            numericUpDown1_ValueChanged(null, null);
        }

        private void NO2_V_Click(object sender, EventArgs e)
        {

        }
    }
}
