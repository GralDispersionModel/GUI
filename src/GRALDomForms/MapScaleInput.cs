#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2019]  [Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using GralItemData;
using System;
using System.Windows.Forms;

namespace Gral.GRALDomForms
{
    public partial class MapScaleInput : Form
    {
        public MapScaleData MapScale;

        public MapScaleInput()
        {
            InitializeComponent();
#if __MonoCS__
            var allNumUpDowns = Main.GetAllControls<NumericUpDown>(this);
            foreach (NumericUpDown nu in allNumUpDowns)
            {
                nu.TextAlign = HorizontalAlignment.Left;
            }
#endif
        }

        private void MapScaleInput_Load(object sender, EventArgs e)
        {
            numericUpDown1.Value = MapScale.Length;
            numericUpDown2.Value = MapScale.Division;
            textBox1.Text = MapScale.X.ToString();
            textBox2.Text = MapScale.Y.ToString();
            if (MapScale.RelativeTo == MapScaleData.ToScreen)
            {
                radioButton1.Checked = true;
            }
            else
            {
                radioButton2.Checked = true;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            MapScale.Length = Convert.ToInt32(numericUpDown1.Value);
            MapScale.Division = Convert.ToInt32(numericUpDown2.Value);
            if (Int32.TryParse(textBox1.Text, out int x))
            {
                MapScale.X = x;
            }
            if (Int32.TryParse(textBox2.Text, out int y))
            {
                MapScale.Y = y;
            }
            if (radioButton1.Checked)
            {
                MapScale.RelativeTo = MapScaleData.ToScreen;
            }
            else
            {
                MapScale.RelativeTo = MapScaleData.ToMap;
            }
            this.Close();
            this.Dispose();
        }
    }
}
