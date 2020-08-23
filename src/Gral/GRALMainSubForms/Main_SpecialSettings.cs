using System;
using System.IO;
using System.Windows.Forms;

namespace GralMainForms
{
    public partial class Main_SpecialSettings : Form
    {
        public bool WriteASCiiOutput = false;
        public bool KeyStrokeWhenExitGRAL = true;
        public int LogLevel = 0;

        public Main_SpecialSettings()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Gral.Main.Project_Locked == false)
            {
                WriteASCiiOutput = checkBox1.Checked;
                LogLevel = (int) numericUpDown2.Value;

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
            }
        }

        private void Main_SpecialSettings_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = WriteASCiiOutput;
            checkBox3.Checked = KeyStrokeWhenExitGRAL;

            string KeepTransientPath = Path.Combine(Gral.Main.ProjectName, "Computation", "KeepAndReadTransientTempFiles.dat");
            numericUpDown1.Value = 24;
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
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            KeyStrokeWhenExitGRAL = checkBox3.Checked;
        }
    }
}
