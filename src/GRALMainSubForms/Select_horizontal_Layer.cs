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
    /// Seelct a horizontal layer for the analyation of results
    /// </summary>
    public partial class SelectHorizontalLayer : Form
    {
        public int MaxGramm;
        public int MaxGral;
        public SelectHorizontalLayer(int layer, int gramm, int _maxgramm, int _maxgral, int interval)
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            MaxGramm = _maxgramm;
            MaxGral = _maxgral;
            radioButton1.Show();
            radioButton2.Show();
            LayerUpDown.Minimum = 1;
            refreshUpDown.Minimum = 1;
            refreshUpDown.Maximum = 20;
            refreshUpDown.Value = interval;

            switch (gramm)
            {
                case -2: // show GRAL only
                    radioButton1.Hide();
                    radioButton2.Checked = true;
                    LayerUpDown.Maximum = _maxgral;
                    break;
                case -1: // show GRAMM only
                    radioButton2.Hide();
                    radioButton1.Checked = true;
                    LayerUpDown.Maximum = _maxgramm;
                    break;
                case 1: // Check GRAMM
                    radioButton1.Checked = true;
                    LayerUpDown.Maximum = _maxgramm;
                    break;

                case 2: // Check GRAL
                    radioButton2.Checked = true;
                    LayerUpDown.Maximum = _maxgral;
                    break;

            }

            LayerUpDown.Value = layer;
        }



        void Button2Click(object sender, EventArgs e) // Cancel
        {

        }



        void Button1Click(object sender, EventArgs e)
        {

        }



        void RadioButton1CheckedChanged(object sender, EventArgs e)
        {
            LayerUpDown.Maximum = MaxGramm;
        }

        void RadioButton2CheckedChanged(object sender, EventArgs e)
        {
            LayerUpDown.Maximum = MaxGral;
        }
    }
}
