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
using System.Windows.Forms;

namespace GralMainForms
{
    /// <summary>
    /// Form to show all vertical layer heights for the GRAL flow field
    /// </summary>
    public partial class VerticalLayerHeights : Form
    {
        public List<float[]> StretchFlexible;
        public float StretchingFactor;
        public float LowestCellHeight;

        public VerticalLayerHeights()
        {
            InitializeComponent();
        }

        private void VerticalLayerHeights_Load(object sender, EventArgs e)
        {
            double top = LowestCellHeight;
            double[] heights = new double[152];
            heights[1] = LowestCellHeight;
            int flexstretchindex = 0;
            double stretching = StretchingFactor;
            listView1.Items.Add("01: " + Math.Round(top, 1).ToString() + " m");

            for (int i = 2; i < 150; i++)
            {
                if (StretchFlexible == null || StretchFlexible.Count == 0)
                {
                    top += LowestCellHeight * Math.Pow(StretchingFactor, i - 2);
                }
                else
                {
                    if (flexstretchindex < StretchFlexible.Count)
                    {
                        if (StretchFlexible[flexstretchindex][1] > 0.99 && top > StretchFlexible[flexstretchindex][0])
                        {
                            stretching = StretchFlexible[flexstretchindex][1];
                            flexstretchindex++;
                        }
                    }
                    heights[i] = heights[i - 1] * stretching;
                    top += heights[i];
                }
                listView1.Items.Add((i - 1).ToString("000") + ": " + Math.Round(top, 1).ToString() + " m");
                if (top > 10000)
                {
                    break;
                }
            }
        }

        private void VerticalLayerHeights_FormClosed(object sender, FormClosedEventArgs e)
        {
            listView1.Items.Clear();
        }
    }
}
