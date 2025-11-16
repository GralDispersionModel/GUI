using System;
using System.IO;
using System.Windows.Forms;

namespace GralMainForms
{
    public partial class SpecialSettings : Form
    {
        public bool WriteASCiiOutput = false;
        public bool KeyStrokeWhenExitGRAL = true;
        public int LogLevel = 0;
        public int RadiusForPrognosticFlowField = 0;
        public bool GRALOnlineFunctions = true;
        public bool GRALReproducibleResults = false;
        private System.Globalization.CultureInfo ic = System.Globalization.CultureInfo.InvariantCulture;
        public SpecialSettings()
        {
            InitializeComponent();
        }

        /// <summary>
        /// OK Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (Gral.Main.Project_Locked == false)
            {
                WriteASCiiOutput = checkBox1.Checked;
                GRALOnlineFunctions = !checkBox5.Checked;
                LogLevel = (int)numericUpDown2.Value;

                string KeepTransientPath = Path.Combine(Gral.Main.ProjectName, "Computation", "KeepAndReadTransientTempFiles.dat");

                if (checkBox2.Checked)
                {
                    try
                    {
                        string interval = numericUpDown1.Value.ToString();
                        using (StreamWriter writer = new StreamWriter(KeepTransientPath))
                        {
                            writer.WriteLine(interval);
                        }
                    }
                    catch { }
                }
                else
                {
                    if (File.Exists(KeepTransientPath))
                    {
                        try
                        {
                            File.Delete(KeepTransientPath);
                        }
                        catch { }
                    }
                }

                if (Gral.Main.ProjectSetting.RelaxationFactorGRALPressure != (double)numericUpDown27.Value || 
                    Gral.Main.ProjectSetting.RelaxationFactorGRALVelocity != (double) numericUpDown28.Value)
                {
                    SetGRALFlowFieldRelaxFactor();
                }
            }
        }

        private void Main_SpecialSettings_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = WriteASCiiOutput;
            checkBox3.Checked = KeyStrokeWhenExitGRAL;

            string KeepTransientPath = Path.Combine(Gral.Main.ProjectName, "Computation", "KeepAndReadTransientTempFiles.dat");
            numericUpDown1.Value = 24;
            numericUpDown28.Value = (decimal) Gral.Main.ProjectSetting.RelaxationFactorGRALVelocity;
            numericUpDown27.Value = (decimal) Gral.Main.ProjectSetting.RelaxationFactorGRALPressure;

            if (File.Exists(KeepTransientPath))
            {
                checkBox2.Checked = true;
                try
                {
                    using (StreamReader reader = new StreamReader(KeepTransientPath))
                    {
                        if (!reader.EndOfStream)
                        {
                            string interval = reader.ReadLine();
                            if (int.TryParse(interval, out int _inter))
                            {
                                numericUpDown1.Value = _inter;
                            }
                        }
                    }
                }
                catch { }
            }

            numericUpDown2.Value = LogLevel;

            if (Gral.Main.Project_Locked)
            {
                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
                checkBox3.Enabled = false;
                numericUpDown2.Enabled = false;
            }

            if (RadiusForPrognosticFlowField >= 50)
            {
                numericUpDown3.Enabled = true;
                numericUpDown3.Value = Math.Min(10000, RadiusForPrognosticFlowField);
                checkBox4.Checked = true;
            }
            else
            {
                numericUpDown3.Enabled = false;
                numericUpDown3.Value = 150;
                checkBox4.Checked = false;
            }
            checkBox5.Checked = !GRALOnlineFunctions;
            if (Gral.Main.Project_Locked)
            {
                foreach (Control c in Controls)
                {
                    c.Enabled = false;
                }
                button1.Enabled = true;
                button2.Enabled = true;
            }
            checkBox6.Checked = GRALReproducibleResults;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            KeyStrokeWhenExitGRAL = checkBox3.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                numericUpDown3.Enabled = true;
                RadiusForPrognosticFlowField = Convert.ToInt32(numericUpDown3.Value);
            }
            else
            {
                RadiusForPrognosticFlowField = 0;
                numericUpDown3.Enabled = false;
            }
        }


        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            RadiusForPrognosticFlowField = Convert.ToInt32(numericUpDown3.Value);
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            GRALReproducibleResults = checkBox6.Checked;
        }
        /// <summary>
        /// change relaxation factor of velocity for microscale flow field model of GRAL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetGRALFlowFieldRelaxFactor()
        {
            Gral.Main.ProjectSetting.RelaxationFactorGRALPressure = (double)numericUpDown27.Value;
            Gral.Main.ProjectSetting.RelaxationFactorGRALVelocity = (double)numericUpDown28.Value;

            try
            {
                string newPath1 = Path.Combine(Gral.Main.ProjectName, @"Computation", "relaxation_factors.txt");
                    try
                    {
                        using (StreamWriter mywriter = new StreamWriter(newPath1))
                        {
                            mywriter.WriteLine(Convert.ToString(Gral.Main.ProjectSetting.RelaxationFactorGRALVelocity, ic));
                            mywriter.WriteLine(Convert.ToString(Gral.Main.ProjectSetting.RelaxationFactorGRALPressure, ic));
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error when writing file \"Computation\\relaxation_factors.txt\"", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
            }
            catch
            {
            }
        }
    }
}
