using System;
using System.IO;
using System.Windows.Forms;

namespace GralMainForms
{
    public partial class OnlineParameters : Form
    {
        public int NumberOfGRAMMLayers;
        public int NumberOfGRALayers;
        public bool GRAMMGroupBoxVisible;
        public bool OnlineGroupBoxVisible;
        public int OnlineRefreshInterval = 1;
        public string ProjectName = string.Empty;
        public Int32 OnlineCheckBoxes = 0;

        public OnlineParameters()
        {
            InitializeComponent();
        }
        private void OnlineParameters_Load(object sender, EventArgs e)
        {
            // initialize checkboxes
            string computationPath = Path.Combine(ProjectName, @"Computation");
            OnlineCheckBoxes = 0;
            if (File.Exists(Path.Combine(computationPath, "uv.txt")))
            {
                checkBox1.Checked = true;
                OnlineCheckBoxes |= (1<<1);
            }
            if (File.Exists(Path.Combine(computationPath, "u.txt")))
            {
                checkBox2.Checked = true;
                OnlineCheckBoxes |= (1 << 2);
            }
            if (File.Exists(Path.Combine(computationPath, "speed.txt")))
            {
                checkBox16.Checked = true;
                OnlineCheckBoxes |= (1 << 16);
            }
            if (File.Exists(Path.Combine(computationPath, "v.txt")))
            {
                checkBox3.Checked = true;
                OnlineCheckBoxes |= (1 << 3);
            }
            if (File.Exists(Path.Combine(computationPath, "w.txt")))
            {
                checkBox4.Checked = true;
                OnlineCheckBoxes |= (1 << 4);
            }
            if (File.Exists(Path.Combine(computationPath, "tabs.txt")))
            {
                checkBox5.Checked = true;
                OnlineCheckBoxes |= (1 << 5);
            }
            if (File.Exists(Path.Combine(computationPath, "tpot.txt")))
            {
                checkBox6.Checked = true;
                OnlineCheckBoxes |= (1 << 6);
            }
            if (File.Exists(Path.Combine(computationPath, "hum.txt")))
            {
                checkBox7.Checked = true;
                OnlineCheckBoxes |= (1 << 7);
            }
            if (File.Exists(Path.Combine(computationPath, "nhp.txt")))
            {
                checkBox8.Checked = true;
                OnlineCheckBoxes |= (1 << 8);
            }
            if (File.Exists(Path.Combine(computationPath, "glob.txt")))
            {
                checkBox9.Checked = true;
                OnlineCheckBoxes |= (1 << 9);
            }
            if (File.Exists(Path.Combine(computationPath, "terr.txt")))
            {
                checkBox10.Checked = true;
                OnlineCheckBoxes |= (1 << 10);
            }
            if (File.Exists(Path.Combine(computationPath, "sensheat.txt")))
            {
                checkBox11.Checked = true;
                OnlineCheckBoxes |= (1 << 11);
            }
            if (File.Exists(Path.Combine(computationPath, "latheat.txt")))
            {
                checkBox12.Checked = true;
                OnlineCheckBoxes |= (1 << 12);
            }
            if (File.Exists(Path.Combine(computationPath, "fricvel.txt")))
            {
                checkBox13.Checked = true;
                OnlineCheckBoxes |= (1 << 13);
            }
            if (File.Exists(Path.Combine(computationPath, "inverseMO.txt")))
            {
                checkBox14.Checked = true;
                OnlineCheckBoxes |= (1 << 14);
            }
            if (File.Exists(Path.Combine(computationPath, "surfTemp.txt")))
            {
                checkBox15.Checked = true;
                OnlineCheckBoxes |= (1 << 15);
            }
            if (File.Exists(Path.Combine(computationPath, "stabilityclass.txt")))
            {
                checkBox28.Checked = true;
                OnlineCheckBoxes |= (1 << 28);
            }
            if (File.Exists(Path.Combine(computationPath, "tke.txt")))
            {
                checkBox17.Checked = true;
                OnlineCheckBoxes |= (1 << 17);
            }
            if (File.Exists(Path.Combine(computationPath, "dis.txt")))
            {
                checkBox18.Checked = true;
                OnlineCheckBoxes |= (1 << 18);
            }

            checkBox1.CheckedChanged += CheckBox1_CheckedChanged;
            checkBox2.CheckedChanged += CheckBox2_CheckedChanged;
            checkBox3.CheckedChanged += CheckBox3_CheckedChanged;
            checkBox4.CheckedChanged += CheckBox4_CheckedChanged;
            checkBox5.CheckedChanged += CheckBox5_CheckedChanged;
            checkBox6.CheckedChanged += CheckBox6_CheckedChanged;
            checkBox7.CheckedChanged += CheckBox7_CheckedChanged;
            checkBox8.CheckedChanged += CheckBox8_CheckedChanged;
            checkBox9.CheckedChanged += CheckBox9_CheckedChanged;
            checkBox10.CheckedChanged += CheckBox10_CheckedChanged;
            checkBox11.CheckedChanged += CheckBox11_CheckedChanged;
            checkBox12.CheckedChanged += CheckBox12_CheckedChanged;
            checkBox13.CheckedChanged += CheckBox13_CheckedChanged;
            checkBox14.CheckedChanged += CheckBox14_CheckedChanged;
            checkBox15.CheckedChanged += CheckBox15_CheckedChanged;
            checkBox16.CheckedChanged += CheckBox16_CheckedChanged;
            checkBox28.CheckedChanged += CheckBox28_CheckedChanged;
            checkBox17.CheckedChanged += CheckBox17_CheckedChanged;
            checkBox18.CheckedChanged += CheckBox18_CheckedChanged;
        }

        //show online GRAMM horizontal windvectors
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                int layer;
                layer = OnlineSelectHorizontalLayer(0);
                if (layer > 0)
                //if (InputBox1("Define horizontal layer", "Layer #:", layer, Convert.ToInt32(numericUpDown16.Value), ref layer) == DialogResult.OK)
                {
                    try
                    {
                        //save layer information to file
                        using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "uv.txt")))
                        {
                            writer.WriteLine(Convert.ToString(layer));
                        }
                        OnlineCheckBoxes |= (1 << 1);
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "uv.txt"));
                OnlineCheckBoxes &= ~(1 << 1);
            }
        }

        //show online GRAMM horizontal windcomponent u (west/east)
        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                //define horizontal layer
                int layer = OnlineSelectHorizontalLayer(0);
                if (layer > 0)
                {
                    try
                    {
                        //save layer information to file
                        using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "u.txt")))
                        {
                            writer.WriteLine(Convert.ToString(layer));
                        }
                        OnlineCheckBoxes |= (1 << 2);
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "u.txt"));
                OnlineCheckBoxes &= ~(1 << 2);
            }
        }

        //show online GRAMM horizontal windspeed
        private void CheckBox16_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox16.Checked == true)
            {
                //define horizontal layer
                int layer = OnlineSelectHorizontalLayer(0);
                if (layer > 0)
                {
                    try
                    {
                        //save layer information to file
                        using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "speed.txt")))
                        {
                            writer.WriteLine(Convert.ToString(layer));
                        }
                        OnlineCheckBoxes |= (1 << 16);
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "speed.txt"));
                OnlineCheckBoxes &= ~(1 << 16);
            }
        }

        //show online GRAMM horizontal windcomponent v (north/south)
        private void CheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked == true)
            {
                //define horizontal layer
                int layer = OnlineSelectHorizontalLayer(0);
                if (layer > 0)
                {
                    try
                    {
                        //save layer information to file
                        using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "v.txt")))
                        {
                            writer.WriteLine(Convert.ToString(layer));
                        }
                        OnlineCheckBoxes |= (1 << 3);
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "v.txt"));
                OnlineCheckBoxes &= ~(1 << 3);
            }
        }

        //show online GRAMM vertical windcomponent w
        private void CheckBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked == true)
            {
                //define horizontal layer
                int layer = OnlineSelectHorizontalLayer(0);
                if (layer > 0)
                {
                    try
                    {
                        //save layer information to file
                        using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "w.txt")))
                        {
                            writer.WriteLine(Convert.ToString(layer));
                        }
                        OnlineCheckBoxes |= (1 << 4);
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "w.txt"));
                OnlineCheckBoxes &= ~(1 << 4);
            }
        }

        //show online GRAMM absolut temperature
        private void CheckBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked == true)
            {
                //define horizontal layer
                int layer = OnlineSelectHorizontalLayer(-1);
                if (layer > 0)
                {
                    try
                    {
                        //save layer information to file
                        using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "tabs.txt")))
                        {
                            writer.WriteLine(Convert.ToString(layer));
                        }
                        OnlineCheckBoxes |= (1 << 5);
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "tabs.txt"));
                OnlineCheckBoxes &= ~(1 << 5);
            }
        }

        //show online GRAMM potential temperature
        private void CheckBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked == true)
            {
                //define horizontal layer
                int layer = OnlineSelectHorizontalLayer(-1);
                if (layer > 0)
                {
                    try
                    {
                        //save layer information to file
                        using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "tpot.txt")))
                        {
                            writer.WriteLine(Convert.ToString(layer));
                        }
                        OnlineCheckBoxes |= (1 << 6);
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "tpot.txt"));
                OnlineCheckBoxes &= ~(1 << 6);
            }
        }

        //show online GRAMM humidity
        private void CheckBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked == true)
            {
                //define horizontal layer
                int layer = OnlineSelectHorizontalLayer(-1);
                if (layer > 0)
                {
                    try
                    {
                        //save layer information to file
                        using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "hum.txt")))
                        {
                            writer.WriteLine(Convert.ToString(layer));
                        }
                        OnlineCheckBoxes |= (1 << 7);
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "hum.txt"));
                OnlineCheckBoxes &= ~(1 << 7);
            }
        }

        //show online GRAMM non-hydrostatic pressure
        private void CheckBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked == true)
            {
                //define horizontal layer
                int layer = OnlineSelectHorizontalLayer(-1);
                if (layer > 0)
                {
                    try
                    {
                        //save layer information to file
                        using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "nhp.txt")))
                        {
                            writer.WriteLine(Convert.ToString(layer));
                        }
                        OnlineCheckBoxes |= (1 << 8);
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "nhp.txt"));
                OnlineCheckBoxes &= ~(1 << 8);
            }
        }

        //show online GRAMM global radiation
        private void CheckBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox9.Checked == true)
            {
                //define horizontal layer
                int layer = 1;
                //save layer information to file
                using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "glob.txt")))
                {
                    writer.WriteLine(Convert.ToString(layer));
                }
                OnlineCheckBoxes |= (1 << 9);
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "glob.txt"));
                OnlineCheckBoxes &= ~(1 << 9);
            }
        }

        //show online GRAMM terrestrial radiation
        private void CheckBox10_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox10.Checked == true)
            {
                //define horizontal layer
                int layer = 1;
                //save layer information to file
                using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "terr.txt")))
                {
                    writer.WriteLine(Convert.ToString(layer));
                }
                OnlineCheckBoxes |= (1 << 10);
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "terr.txt"));
                OnlineCheckBoxes &= ~(1 << 10);
            }
        }

        //show online GRAMM sensible heat flux
        private void CheckBox11_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox11.Checked == true)
            {
                //define horizontal layer
                int layer = 1;
                //save layer information to file
                using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "sensheat.txt")))
                {
                    writer.WriteLine(Convert.ToString(layer));
                }
                OnlineCheckBoxes |= (1 << 11);
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "sensheat.txt"));
                OnlineCheckBoxes &= ~(1 << 11);
            }
        }

        //show online GRAMM latent heat flux
        private void CheckBox12_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox12.Checked == true)
            {
                //define horizontal layer
                int layer = 1;
                //save layer information to file
                using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "latheat.txt")))
                {
                    writer.WriteLine(Convert.ToString(layer));
                }
                OnlineCheckBoxes |= (1 << 12);
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "latheat.txt"));
                OnlineCheckBoxes &= ~(1 << 12);
            }
        }

        //show online GRAMM friction velocity
        private void CheckBox13_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox13.Checked == true)
            {
                //define horizontal layer
                int layer = 1;
                //save layer information to file
                using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "fricvel.txt")))
                {
                    writer.WriteLine(Convert.ToString(layer));
                }
                OnlineCheckBoxes |= (1 << 13);
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "fricvel.txt"));
                OnlineCheckBoxes &= ~(1 << 13);
            }
        }

        //show online GRAMM inverse Monin-Obukhov length
        private void CheckBox14_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox14.Checked == true)
            {
                //define horizontal layer
                int layer = 1;
                //save layer information to file
                using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "inverseMO.txt")))
                {
                    writer.WriteLine(Convert.ToString(layer));
                }
                OnlineCheckBoxes |= (1 << 14);
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "inverseMO.txt"));
                OnlineCheckBoxes &= ~(1 << 14);
            }
        }

        //show online GRAMM surface temperature
        private void CheckBox15_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox15.Checked == true)
            {
                //define horizontal layer
                int layer = 1;
                //save layer information to file
                using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "surfTemp.txt")))
                {
                    writer.WriteLine(Convert.ToString(layer));
                }
                OnlineCheckBoxes |= (1 << 15);
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "surfTemp.txt"));
                OnlineCheckBoxes &= ~(1 << 15);
            }
        }

        //show online GRAMM stability classes
        private void CheckBox28_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox28.Checked == true)
            {
                //define horizontal layer
                int layer = 1;
                //save layer information to file
                using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "stabilityclass.txt")))
                {
                    writer.WriteLine(Convert.ToString(layer));
                }
                OnlineCheckBoxes |= (1 << 28);
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "stabilityclass.txt"));
                OnlineCheckBoxes &= ~(1 << 28);
            }
        }

        //show online GRAMM turbulent kinetic energy
        private void CheckBox17_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox17.Checked == true)
            {
                //define horizontal layer
                int layer = OnlineSelectHorizontalLayer(-1);
                if (layer > 0)
                {
                    try
                    {
                        //save layer information to file
                        using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "tke.txt")))
                        {
                            writer.WriteLine(Convert.ToString(layer));
                        }
                        OnlineCheckBoxes |= (1 << 17);
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "tke.txt"));
                OnlineCheckBoxes &= ~(1 << 17);
            }
        }

        //show online GRAMM dissipation
        private void CheckBox18_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox18.Checked == true)
            {
                //define horizontal layer
                int layer = OnlineSelectHorizontalLayer(-1);
                if (layer > 0)
                {
                    try
                    {
                        //save layer information to file
                        using (StreamWriter writer = new StreamWriter(Path.Combine(ProjectName, @"Computation", "dis.txt")))
                        {
                            writer.WriteLine(Convert.ToString(layer));
                        }
                        OnlineCheckBoxes |= (1 << 18);
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "dis.txt"));
                OnlineCheckBoxes &= ~(1 << 18);
            }
        }

        private int OnlineSelectHorizontalLayer(int layer)
        {
            // Input Form to select the  horizontal layer
            int gramm = 1;

            if (GRAMMGroupBoxVisible == true & OnlineGroupBoxVisible == true)
            {
                gramm = 1;
            }

            if (GRAMMGroupBoxVisible == false & OnlineGroupBoxVisible == true)
            {
                gramm = -2;
            }

            if (GRAMMGroupBoxVisible == true & OnlineGroupBoxVisible == false)
            {
                gramm = -1;
            }

            if (layer == -1) // just show GRAMM
            {
                gramm = -1;
            }

            layer = 1;
            GralMainForms.SelectHorizontalLayer selection = new GralMainForms.SelectHorizontalLayer(layer, gramm, NumberOfGRAMMLayers, NumberOfGRALayers, OnlineRefreshInterval);
            selection.ShowDialog();

            layer = Convert.ToInt32(selection.LayerUpDown.Value);
            OnlineRefreshInterval = Convert.ToInt32(selection.refreshUpDown.Value);
            selection.Dispose();

            if (selection.DialogResult == DialogResult.OK)
            {
                return layer;
            }
            else
            {
                return -1;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            checkBox1.CheckedChanged -= CheckBox1_CheckedChanged;
            checkBox2.CheckedChanged -= CheckBox2_CheckedChanged;
            checkBox3.CheckedChanged -= CheckBox3_CheckedChanged;
            checkBox4.CheckedChanged -= CheckBox4_CheckedChanged;
            checkBox5.CheckedChanged -= CheckBox5_CheckedChanged;
            checkBox6.CheckedChanged -= CheckBox6_CheckedChanged;
            checkBox7.CheckedChanged -= CheckBox7_CheckedChanged;
            checkBox8.CheckedChanged -= CheckBox8_CheckedChanged;
            checkBox9.CheckedChanged -= CheckBox9_CheckedChanged;
            checkBox10.CheckedChanged -= CheckBox10_CheckedChanged;
            checkBox11.CheckedChanged -= CheckBox11_CheckedChanged;
            checkBox12.CheckedChanged -= CheckBox12_CheckedChanged;
            checkBox13.CheckedChanged -= CheckBox13_CheckedChanged;
            checkBox14.CheckedChanged -= CheckBox14_CheckedChanged;
            checkBox15.CheckedChanged -= CheckBox15_CheckedChanged;
            checkBox16.CheckedChanged -= CheckBox16_CheckedChanged;
            checkBox28.CheckedChanged -= CheckBox28_CheckedChanged;
            checkBox17.CheckedChanged -= CheckBox17_CheckedChanged;
            checkBox18.CheckedChanged -= CheckBox18_CheckedChanged;
            this.Close();
        }

        
    }
}
