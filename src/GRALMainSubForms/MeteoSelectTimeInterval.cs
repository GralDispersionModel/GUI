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

namespace GralMainForms
{
    /// <summary>
    /// Input box for start and endtime of a meteorological time series
    /// </summary>
    public partial class MeteoSelectTimeInterval : Form
    {
        public GralData.WindRoseSettings WindRoseSet;


        /// <summary>
        /// Input box for start and endtime of a meteorological time series
        /// </summary>
        public MeteoSelectTimeInterval()
        {
            InitializeComponent();
        }

        private void MeteoSelectTimeInterval_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 24; i++)
            {
                comboBox1.Items.Add(Convert.ToString(i) + ":00");
                comboBox2.Items.Add(Convert.ToString(i) + ":00");
            }
            comboBox1.SelectedIndex = WindRoseSet.StartStunde;
            comboBox2.SelectedIndex = WindRoseSet.EndStunde;
            if (WindRoseSet.MaxVelocity == 0)
            {
                numericUpDown1.Visible = false;
                label3.Visible = false;
                label4.Visible = false;
            }
            else if (WindRoseSet.MaxVelocity >= 6 && WindRoseSet.MaxVelocity <= 20)
            {
                numericUpDown1.Value = (decimal)WindRoseSet.MaxVelocity;
            }
            if (!WindRoseSet.ShowBias)
            {
                checkBox1.Visible = false;
                checkBox2.Visible = false;
                checkBox3.Visible = false;
            }
            checkBox1.Checked = WindRoseSet.BiasCorrection;
            checkBox2.Checked = WindRoseSet.ShowFrames;
            checkBox3.Checked = WindRoseSet.DrawSmallSectors;

            if (!WindRoseSet.ShowWindSectorGroupBox)
            {
                groupBox1.Visible = false;
                if (WindRoseSet.ShowMaxScaleGroupBox)
                {
                    groupBox2.Visible = true;
                    comboBox3.SelectedIndex = WindRoseSet.MaxScaleVertical;
                }
            }
            else
            {
                switch (WindRoseSet.SectorCount)
                {
                    case 24:
                        radioButton2.Checked = true;
                        break;
                    case 36:
                        radioButton3.Checked = true;
                        break;
                    default:
                        radioButton1.Checked = true;
                        break;
                }
            }

            checkBox4.Checked = WindRoseSet.Ignore00Values;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WindRoseSet.StartStunde = comboBox1.SelectedIndex;
            WindRoseSet.EndStunde = comboBox2.SelectedIndex;
            WindRoseSet.MaxVelocity = (int)numericUpDown1.Value;
            WindRoseSet.BiasCorrection = checkBox1.Checked;
            WindRoseSet.ShowFrames = checkBox2.Checked;
            WindRoseSet.DrawSmallSectors = checkBox3.Checked;
            if (radioButton1.Checked)
            {
                WindRoseSet.SectorCount = 16;
            }
            else if (radioButton2.Checked)
            {
                WindRoseSet.SectorCount = 24;
            }
            else if (radioButton3.Checked)
            {
                WindRoseSet.SectorCount = 36;
            }
            WindRoseSet.Ignore00Values = checkBox4.Checked;

            if (WindRoseSet.ShowMaxScaleGroupBox)
            {
                WindRoseSet.MaxScaleVertical = comboBox3.SelectedIndex;
            }

            this.Close();
        }
    }
}
