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

/*
 * Created by SharpDevelop.
 * User: Markus
 * Date: 17.01.2019
 * Time: 16:58
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Windows.Forms;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
	{
		        ////////////////////////////////////////////////////////////////
        //  GRAMM ONLINE
        ////////////////////////////////////////////////////////////////

        //show map
        private void Button36_Click(object sender, EventArgs e)
        {
            GralDomain.Domain domain = new GralDomain.Domain(this, true)
            {
                ReDrawContours = true,
                ReDrawVectors = true
            };
            domain.StartPosition = FormStartPosition.Manual;
            domain.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition();
            domain.Show();
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
                    }
                    catch
                    { }
                }


            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "uv.txt"));
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
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "u.txt"));
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
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "speed.txt"));
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
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "v.txt"));
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
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "w.txt"));
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
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "tabs.txt"));
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
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "tpot.txt"));
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
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "hum.txt"));
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
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "nhp.txt"));
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
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "glob.txt"));
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
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "terr.txt"));
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
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "sensheat.txt"));
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
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "latheat.txt"));
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
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "fricvel.txt"));
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
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "inverseMO.txt"));
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
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "surfTemp.txt"));
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
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "stabilityclass.txt"));
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
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "tke.txt"));
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
                    }
                    catch
                    { }
                }
            }
            else
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "dis.txt"));
            }
        }

        private int OnlineSelectHorizontalLayer(int layer)
        {
            // Input Form to select the  horizontal layer

            int gramm = 1;

            if (groupBox15.Visible == true & groupBox17.Visible == true)
            {
                gramm = 1;
            }

            if (groupBox15.Visible == false & groupBox17.Visible == true)
            {
                gramm = -2;
            }

            if (groupBox15.Visible == true & groupBox17.Visible == false)
            {
                gramm = -1;
            }

            if (layer == -1) // just show GRAMM
            {
                gramm = -1;
            }

            layer = 1;
            GralMainForms.SelectHorizontalLayer selection = new GralMainForms.SelectHorizontalLayer(layer, gramm, Convert.ToInt32(numericUpDown16.Value), Convert.ToInt32(numericUpDown26.Value), OnlineRefreshInterval);
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

	}
}