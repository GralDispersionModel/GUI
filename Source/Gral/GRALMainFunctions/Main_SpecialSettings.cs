using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gral
{
    public partial class Main_SpecialSettings : Form
    {
        public bool WriteASCiiOutput = false;

        public Main_SpecialSettings()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Main.Project_Locked == false)
            {
                WriteASCiiOutput = checkBox1.Checked;

                string KeepTransientPath = Path.Combine(Main.ProjectName, "Computation", "KeepAndReadTransientTempFiles.dat");

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

            string KeepTransientPath = Path.Combine(Main.ProjectName, "Computation", "KeepAndReadTransientTempFiles.dat");
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

            if (Main.Project_Locked)
            {
                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
            }
        }
    }
}
