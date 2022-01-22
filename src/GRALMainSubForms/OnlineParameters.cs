using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace GralMainForms
{
    /// <summary>
    /// Show all online parameters for GRAMM or GRAL online view
    /// </summary>
    public partial class OnlineParameters : Form
    {
        public int NumberOfGRAMMLayers;
        public int NumberOfGRALayers;
        public bool GRAMMGroupBoxVisible;
        public bool OnlineGroupBoxVisible;
        public int OnlineRefreshInterval = 1;
        public string ProjectName = string.Empty;
        public Int32 OnlineCheckBoxes = 0;
        private List<string> FileNames = new List<string>();

        public OnlineParameters()
        {
            InitializeComponent();
        }
        private void OnlineParameters_Load(object sender, EventArgs e)
        {
            // initialize checkboxes
            string computationPath = Path.Combine(ProjectName, @"Computation");
            OnlineCheckBoxes = 0;
            string filename = Path.Combine(computationPath, "uv.txt");
            if (File.Exists(filename))
            {
                checkBox1.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1<<1);
            }
            filename = Path.Combine(computationPath, "u.txt");
            if (File.Exists(filename))
            {
                checkBox2.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 2);
            }
            filename = Path.Combine(computationPath, "speed.txt");
            if (File.Exists(filename))
            {
                checkBox16.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 16);
            }
            filename = Path.Combine(computationPath, "v.txt");
            if (File.Exists(filename))
            {
                checkBox3.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 3);
            }
            filename = Path.Combine(computationPath, "w.txt");
            if (File.Exists(filename))
            {
                checkBox4.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 4);
            }
            filename = Path.Combine(computationPath, "tabs.txt");
            if (File.Exists(filename))
            {
                checkBox5.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 5);
            }
            filename = Path.Combine(computationPath, "tpot.txt");
            if (File.Exists(filename))
            {
                checkBox6.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 6);
            }
            filename = Path.Combine(computationPath, "hum.txt");
            if (File.Exists(filename))
            {
                checkBox7.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 7);
            }
            filename = Path.Combine(computationPath, "nhp.txt");
            if (File.Exists(filename))
            {
                checkBox8.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 8);
            }
            filename = Path.Combine(computationPath, "glob.txt");
            if (File.Exists(filename))
            {
                checkBox9.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 9);
            }
            filename = Path.Combine(computationPath, "terr.txt");
            if (File.Exists(filename))
            {
                checkBox10.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 10);
            }
            filename = Path.Combine(computationPath, "sensheat.txt");
            if (File.Exists(filename))
            {
                checkBox11.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 11);
            }
            filename = Path.Combine(computationPath, "latheat.txt");
            if (File.Exists(filename))
            {
                checkBox12.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 12);
            }
            filename = Path.Combine(computationPath, "fricvel.txt");
            if (File.Exists(filename))
            {
                checkBox13.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 13);
            }
            filename = Path.Combine(computationPath, "inverseMO.txt");
            if (File.Exists(filename))
            {
                checkBox14.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 14);
            }
            filename = Path.Combine(computationPath, "surfTemp.txt");
            if (File.Exists(filename))
            {
                checkBox15.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 15);
            }
            filename = Path.Combine(computationPath, "stabilityclass.txt");
            if (File.Exists(filename))
            {
                checkBox28.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 28);
            }
            filename = Path.Combine(computationPath, "tke.txt");
            if (File.Exists(filename))
            {
                checkBox17.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
                OnlineCheckBoxes |= (1 << 17);
            }
            filename = Path.Combine(computationPath, "dis.txt");
            if (File.Exists(filename))
            {
                checkBox18.Checked = true;
                (int layer, int sit, int LY) = ReadLayerAndSituation(filename);
                numericUpDown1.Value = sit;
                FileNames.Add(filename);
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
                            writer.WriteLine(LayerAndSituation(layer));
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
                            writer.WriteLine(LayerAndSituation(layer));
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
                            writer.WriteLine(LayerAndSituation(layer));
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
                            writer.WriteLine(LayerAndSituation(layer));
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
                            writer.WriteLine(LayerAndSituation(layer));
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
                            writer.WriteLine(LayerAndSituation(layer));
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
                            writer.WriteLine(LayerAndSituation(layer));
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
                            writer.WriteLine(LayerAndSituation(layer));
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
                            writer.WriteLine(LayerAndSituation(layer));
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
                    writer.WriteLine(LayerAndSituation(layer));
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
                    writer.WriteLine(LayerAndSituation(layer));
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
                    writer.WriteLine(LayerAndSituation(layer));
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
                    writer.WriteLine(LayerAndSituation(layer));
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
                    writer.WriteLine(LayerAndSituation(layer));
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
                    writer.WriteLine(LayerAndSituation(layer));
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
                    writer.WriteLine(LayerAndSituation(layer));
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
                    writer.WriteLine(LayerAndSituation(layer));
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
                            writer.WriteLine(LayerAndSituation(layer));
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
                            writer.WriteLine(LayerAndSituation(layer));
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

        /// <summary>
        /// Combine layer and (if needed) the situation number for GRAMM online 
        /// </summary>
        private string LayerAndSituation(int layer)
        {
            string layerString = Convert.ToString(layer);
            
            //add situation number if > 0
            if (numericUpDown1.Value > 0)
            {
                layerString += "," + Convert.ToString(numericUpDown1.Value);
            }
            return layerString;
        }
        
        /// <summary>
        /// Read layer and (if available) the situation number for GRAMM online 
        /// </summary>
        private (int, int, int) ReadLayerAndSituation(string filename)
        {
            int layer = 1;
            int LY = 1; // default
            int situation = 0;
            string config1 = string.Empty;
            string config2 = string.Empty;
            try
            {
                using (StreamReader rd = new StreamReader(filename))
                {
                    config1 = rd.ReadLine();
                    if (!rd.EndOfStream)
                    {
                        config2 = rd.ReadLine();
                    }
                }
                string[] textSplit = config1.Split(new char[] { ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                Int32.TryParse(textSplit[0], out layer); // Layer 
                if (textSplit.Length > 1)
                {
                    Int32.TryParse(textSplit[1], out situation); // situation that should be shown
                }
                
                Int32.TryParse(config2, out LY); // immer LY
            }
            catch
            { }

            return (layer, situation, LY);
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

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            foreach(string filename in FileNames)
            {
                if (File.Exists(filename))
                {
                    (int layer, int sit, int LY) = ReadLayerAndSituation(filename);

                    if (layer > 0)
                    {
                        using (StreamWriter writer = new StreamWriter(filename))
                        {
                            writer.WriteLine(LayerAndSituation(layer));
                            writer.WriteLine(LY);
                        }
                    }

                }
            }
        }
    }
}
