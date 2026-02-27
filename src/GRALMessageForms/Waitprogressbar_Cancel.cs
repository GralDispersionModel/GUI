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

using System.Windows.Forms;

namespace GralMessage
{

    public delegate void ProgressbarUpdateDelegate(object sender, int max);

    /// <summary>
    /// Show a progress bar with cancel button
    /// </summary>
    public partial class WaitProgressbarCancel : Form
    {
        private System.Threading.CancellationTokenSource cts;
        public ProgressbarUpdateDelegate UpdateProgressDelegate;
        /// <summary>
        /// Progress and Cancel form
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="CTS">Cancellation token source</param>
        public WaitProgressbarCancel(string Title, ref System.Threading.CancellationTokenSource CTS)
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            cts = CTS;
            progressbar.Minimum = 1;
            progressbar.Maximum = 100;
            progressbar.Value = 100;
            progressbar.Step = 1;
            UpdateProgressDelegate = new ProgressbarUpdateDelegate(ProgressbarUpdate);
            this.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
            this.Top = GralStaticFunctions.St_F.GetTopScreenAtMousePosition() + 140;
            this.Text = Title;
        }

        /// <summary>
        /// Update the progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="max">0 = increment, >0 = set maximum</param>
        public void ProgressbarUpdate(object sender, int max)
        {
            if (max == 0)
            {
                progressbar.PerformStep();
            }
            else
            {
                progressbar.Maximum = max;
                progressbar.Value = 1;
            }
        }

        /// <summary>
        /// Set Cancellation Token
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Cancel_buttonMouseClick(object sender, MouseEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }
    }
}
