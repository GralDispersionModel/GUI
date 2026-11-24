// SPDX-License-Identifier: GPL-3.0-only
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Gral
{
    partial class Main
    {
        private void ConfigureMonoComputationLayout()
        {
            label98.AutoSize = true;
            int shift = Math.Max(0, label98.Left + label98.PreferredWidth + 8 - numericUpDown33.Left);
            foreach (var control in new[] { numericUpDown32, numericUpDown33 })
            {
                control.SetBounds(control.Left + shift, control.Top, control.Width - shift, control.Height);
            }

            groupBox17.Font = groupBox18.Font;
            groupBox13.Left = groupBox18.Right;
            groupBox13.Width = groupBox16.ClientSize.Width - groupBox13.Left - 4;
            foreach (Control action in groupBox13.Controls)
            {
                action.Width = groupBox13.ClientSize.Width - 2 * action.Left;
            }
            var caption = new Font(groupBox13.Font.FontFamily, 8F, FontStyle.Bold);
            groupBox13.Font = caption;
            groupBox13.Disposed += (_, _) => caption.Dispose();

            // Mono does not draw a same-size bitmap inside the button's border.
            foreach (Button button in new[] { button41, button42 })
            {
                var icon = new Bitmap(button.BackgroundImage, button.Width - 8, button.Height - 8);
                button.BackgroundImage = null;
                button.Image = icon;
                button.Disposed += (_, _) => icon.Dispose();
            }
        }
    }
}
