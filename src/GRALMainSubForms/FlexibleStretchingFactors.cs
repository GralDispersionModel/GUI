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
using System.Collections.Generic;
using GralStaticFunctions;
using System.Windows.Forms;

namespace GralMainForms
{
	/// <summary>
    /// Form to set user defined flexible stretching factors
    /// </summary>
    public partial class FlexibleStretchingFactors : Form
    {
        public List<float[]> FlexibleStretchingFactor;
        public bool ProjectLocked;

        public FlexibleStretchingFactors()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!ProjectLocked)
            {
                FlexibleStretchingFactor.Clear();
            }
            ReleaseHandler();
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!ProjectLocked)
            {
                FlexibleStretchingFactor.Clear();
                for (int i = 0; i < 4; i++)
                {
                    FlexibleStretchingFactor.Add(new float[2]);
                }

                FlexibleStretchingFactor[0][0] = (float)numericUpDown1.Value;
                FlexibleStretchingFactor[0][1] = (float)numericUpDown2.Value;
                FlexibleStretchingFactor[1][0] = (float)numericUpDown4.Value;
                FlexibleStretchingFactor[1][1] = (float)numericUpDown3.Value;
                FlexibleStretchingFactor[2][0] = (float)numericUpDown6.Value;
                FlexibleStretchingFactor[2][1] = (float)numericUpDown5.Value;
                FlexibleStretchingFactor[3][0] = (float)numericUpDown8.Value;
                FlexibleStretchingFactor[3][1] = (float)numericUpDown7.Value;
            }
            ReleaseHandler();
            this.Close();
        }

        private void ReleaseHandler()
        {
            numericUpDown1.ValueChanged -= new EventHandler(numericUpDown1_ValueChanged);
            numericUpDown2.ValueChanged -= new EventHandler(numericUpDown2_ValueChanged);
            numericUpDown3.ValueChanged -= new EventHandler(numericUpDown3_ValueChanged);
            numericUpDown4.ValueChanged -= new EventHandler(numericUpDown4_ValueChanged);
            numericUpDown5.ValueChanged -= new EventHandler(numericUpDown5_ValueChanged);
            numericUpDown6.ValueChanged -= new EventHandler(numericUpDown6_ValueChanged);
        }

        private void AddHandler()
        {
            numericUpDown1.ValueChanged += new EventHandler(numericUpDown1_ValueChanged);
            numericUpDown2.ValueChanged += new EventHandler(numericUpDown2_ValueChanged);
            numericUpDown3.ValueChanged += new EventHandler(numericUpDown3_ValueChanged);
            numericUpDown4.ValueChanged += new EventHandler(numericUpDown4_ValueChanged);
            numericUpDown5.ValueChanged += new EventHandler(numericUpDown5_ValueChanged);
            numericUpDown6.ValueChanged += new EventHandler(numericUpDown6_ValueChanged);
        }

        private void FlexibleStretchingFactors_Load(object sender, EventArgs e)
        {
#if __MonoCS__
            var allNumUpDowns = Gral.Main.GetAllControls<NumericUpDown>(this);
            foreach (NumericUpDown nu in allNumUpDowns)
            {
                nu.TextAlign = HorizontalAlignment.Left;
            }
#endif
            if (ProjectLocked)
            {
                button2.Enabled = !ProjectLocked;
                numericUpDown1.Enabled = !ProjectLocked;
                numericUpDown2.Enabled = !ProjectLocked;
                numericUpDown3.Enabled = !ProjectLocked;
                numericUpDown4.Enabled = !ProjectLocked;
                numericUpDown5.Enabled = !ProjectLocked;
                numericUpDown6.Enabled = !ProjectLocked;
                numericUpDown7.Enabled = !ProjectLocked;
                numericUpDown8.Enabled = !ProjectLocked;
            }

            if (FlexibleStretchingFactor == null || FlexibleStretchingFactor.Count == 0)
            {
                AddHandler();
                return;
            }

            numericUpDown1.Value = (decimal)FlexibleStretchingFactor[0][0];
            numericUpDown2.Value = St_F.ValueSpan(1, 1.5, FlexibleStretchingFactor[0][1]);

            if (FlexibleStretchingFactor.Count > 1)
            {
                numericUpDown4.Value = (decimal)FlexibleStretchingFactor[1][0];
                numericUpDown3.Value = St_F.ValueSpan(1, 1.5, FlexibleStretchingFactor[1][1]);
            }
            if (FlexibleStretchingFactor.Count > 2)
            {
                numericUpDown6.Value = (decimal)FlexibleStretchingFactor[2][0];
                numericUpDown5.Value = St_F.ValueSpan(1, 1.5, FlexibleStretchingFactor[2][1]);
            }
            if (FlexibleStretchingFactor.Count > 3)
            {
                numericUpDown8.Value = (decimal)FlexibleStretchingFactor[3][0];
                numericUpDown7.Value = St_F.ValueSpan(1, 1.5, FlexibleStretchingFactor[3][1]);
            }
            AddHandler();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value > numericUpDown4.Value - 10)
            {
                numericUpDown4.Value = numericUpDown1.Value + 10;
            }
        }
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown4.Value > numericUpDown6.Value - 20)
            {
                numericUpDown6.Value = numericUpDown4.Value + 20;
            }
        }
        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown6.Value > numericUpDown8.Value - 30)
            {
                numericUpDown8.Value = numericUpDown6.Value + 30;
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown2.Value > numericUpDown3.Value)
            {
                numericUpDown3.Value = numericUpDown2.Value;
            }
        }
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown3.Value > numericUpDown5.Value)
            {
                numericUpDown5.Value = numericUpDown3.Value;
            }
        }
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown5.Value > numericUpDown7.Value)
            {
                numericUpDown7.Value = numericUpDown5.Value;
            }
        }

    }


}
