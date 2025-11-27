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
using System.Drawing;
using System.Windows.Forms;

namespace GralMessage
{
    /// <summary>
    /// Show a message -> this message disappears after some seconds
    /// </summary>
    public partial class MessageBoxTemporary : Form
    {
        private System.Windows.Forms.Timer timer1;
        private Point ptm = new Point(GralStaticFunctions.St_F.GetScreenAtMousePosition(), GralStaticFunctions.St_F.GetTopScreenAtMousePosition());
        public MessageBoxTemporary(string a, Point ptt)
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            if (ptt.X > ptm.X && ptt.Y > ptm.Y)
            {
                ptm.X = ptt.X + 20;
                ptm.Y = ptt.Y + 20;
            }
            textBox1.Text = a;

            timer1 = new System.Windows.Forms.Timer
            {
                Enabled = true,
                Interval = 10000
            };
            timer1.Tick += new System.EventHandler(timer1_Tick);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Close();
            Dispose();
            pictureBox1.Dispose();
            timer1.Dispose();
        }

        void PictureBox1Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawIcon(SystemIcons.Information, 0, 0);
        }

        void MessageBox_TemporaryLoad(object sender, EventArgs e)
        {
            Location = ptm;
        }
    }
}
