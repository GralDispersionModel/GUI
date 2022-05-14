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

namespace GralDomForms
{
    /// <summary>
    /// Create animated GIFS dialog
    /// </summary>
    public partial class CreateAnimatedGIFs : Form
    {
        public int StartSituation { get; set; }
        public int FinalSituation { get; set; }
        public int FrameDelay { get; set; }
        public string GIFFileName { get; set; }

        public CreateAnimatedGIFs()
        {
            InitializeComponent();
            #if __MonoCS__
                var allNumUpDowns  = Gral.Main.GetAllControls<NumericUpDown>(this);
                foreach (NumericUpDown nu in allNumUpDowns)
                {
                    nu.TextAlign = HorizontalAlignment.Left;
                }
            #endif
        }

        private void CreateAnimatedGIFs_Load(object sender, EventArgs e)
        {
            numericUpDown1.Maximum = FinalSituation;
            numericUpDown2.Maximum = FinalSituation;
            numericUpDown1.Value = StartSituation;
            numericUpDown2.Value = FinalSituation;
            numericUpDown3.Value = FrameDelay;
            label5.Text = GIFFileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StartSituation = (int) (numericUpDown1.Value);
            FinalSituation = (int) (numericUpDown2.Value);
            FrameDelay = (int) (numericUpDown3.Value);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "(*.gif)|*.gif",
                Title = "Set GIF file name",
                InitialDirectory = GIFFileName,
                ShowHelp = true
#if NET6_0_OR_GREATER
                ,ClientGuid = GralStaticFunctions.St_F.FileDialogMaps
#endif
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    GIFFileName = dialog.FileName;
                    label5.Text = GIFFileName;
                }
            }
        }
    }
}
