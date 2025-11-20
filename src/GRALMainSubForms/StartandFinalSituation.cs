using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Gral.GralMainForms
{
    public partial class StartandFinalSituation : Form
    {
        private int _maxSituation;
        public int StartSituation;
        public int FinalSituation;

        public StartandFinalSituation(int MaxSituation)
        {
            _maxSituation = MaxSituation;

            InitializeComponent();
        }

        public void StartandFinalSituation_Load(object sender, EventArgs e)
        {
            numericUpDown1.Minimum = 1;
            numericUpDown1.Maximum = _maxSituation;
            numericUpDown1.Value = 1;
            numericUpDown2.Minimum = 1;
            numericUpDown2.Maximum = _maxSituation;
            numericUpDown2.Value = _maxSituation;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StartSituation = (int) numericUpDown1.Value;
            FinalSituation = (int) numericUpDown2.Value;
        }
    }
}
