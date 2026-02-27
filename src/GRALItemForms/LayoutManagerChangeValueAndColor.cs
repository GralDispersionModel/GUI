using System;
using System.Drawing;
using System.Windows.Forms;

namespace Gral.GRALItemForms
{
    /// <summary>
    /// User defined values, line and fill colors for one entry
    /// </summary>
    public partial class LayoutManagerChangeValueAndColor : Form
    {
        public double Value;
        public Color LineColor;
        public Color FillColor;
        public int DecimalPlaces;

        public LayoutManagerChangeValueAndColor()
        {
            InitializeComponent();
        }

        private void LayoutManagerChangeValueAndColor_Load(object sender, EventArgs e)
        {
#if __MonoCS__
            numericUpDown1.TextAlign = HorizontalAlignment.Left;
#else
#endif
            numericUpDown1.DecimalPlaces = DecimalPlaces;
            if ((decimal)Value > numericUpDown1.Maximum)
            {
                numericUpDown1.Maximum = (decimal)(Value + 10);
            }
            if ((decimal)Value < numericUpDown1.Minimum)
            {
                numericUpDown1.Minimum = (decimal)(Value - 10);
            }
            numericUpDown1.Value = (decimal)Value;
            button1.BackColor = LineColor;
            button2.BackColor = FillColor;
        }

        /// <summary>
        /// Select Line Color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            using (ColorDialog colDial = new ColorDialog())
            {
                colDial.Color = LineColor;
                colDial.AnyColor = true;
                colDial.CustomColors = new int[] { ColorTranslator.ToOle(LineColor) };
                if (colDial.ShowDialog() == DialogResult.OK)
                {
                    LineColor = colDial.Color;
                    button1.BackColor = LineColor;
                }
            }
        }

        /// <summary>
        /// Select Fill Color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            using (ColorDialog colDial = new ColorDialog())
            {
                colDial.Color = FillColor;
                colDial.AnyColor = true;
                colDial.CustomColors = new int[] { ColorTranslator.ToOle(FillColor) };
                if (colDial.ShowDialog() == DialogResult.OK)
                {
                    FillColor = colDial.Color;
                    button2.BackColor = FillColor;
                }
            }
        }

        //OK
        private void button3_Click(object sender, EventArgs e)
        {
            Value = (double)numericUpDown1.Value;
        }

        /// <summary>
        /// Copy fill color to line color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            LineColor = FillColor;
            button1.BackColor = LineColor;
        }

        /// <summary>
        /// Copy line color to fill color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            FillColor = LineColor;
            button2.BackColor = FillColor;
        }
    }
}
