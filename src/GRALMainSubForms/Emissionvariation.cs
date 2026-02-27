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

using Gral;
using GralStaticFunctions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GralMainForms
{
    /// <summary>
    /// Form shows the emission variation
    /// </summary>
    public partial class Emissionvariation : Form
    {
        private readonly Main form1 = null;
        private readonly List<string> moddiurnal = new List<string>(); //collection of diurnal emission modulation data
        private readonly List<string> modseasonal = new List<string>(); //collection of seasonal emission modulation data
        private readonly string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        private float pixel = 1;
        private int index;
        private readonly string[] months = new string[12] { "Jan", "Feb", "Mar", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        private bool changes_saved = true;
        private string emissionModulationPath = string.Empty;

        //private bool _set_GRAL_Files_Only = false;

        public Emissionvariation(Main f, string EmissionModulationPath)
        {
            form1 = f;
            DoubleBuffered = true;
            InitializeComponent();
            if (Directory.Exists(EmissionModulationPath))
            {
                emissionModulationPath = EmissionModulationPath;
            }

            Graphics g = CreateGraphics();
            float dx = 96;
            try
            {
                dx = g.DpiX;
            }
            finally
            {
                g.Dispose();
            }
            pixel = dx / 96;

            Assembly _assembly;

            //read diurnal pre-defined diurnal emission variations from ressources
            try
            {
                string newPath;
                _assembly = Assembly.GetExecutingAssembly();
                newPath = Path.Combine(Main.ProjectName, @"Settings", "diurnal_emissionmod.txt");

                StreamReader myReader;

                if (File.Exists(newPath) == true) // already existing diurnal emission data
                {
                    myReader = new StreamReader(newPath);
                }
                else
                {
                    newPath = Path.Combine(Main.GUISettings.AppSettingsPath, "Emission_Mod_Diurnal.txt");
                    if (File.Exists(newPath) == true) // already existing diurnal emission data
                    {
                        myReader = new StreamReader(newPath);
                    }
                    else
                    {
                        myReader = new StreamReader(_assembly.GetManifestResourceStream("Gral.Resources.Modulation_diurnal.txt"));
                    }
                }

                string text = myReader.ReadLine();
                string[] text1 = new string[30];
                int i = 0;
                while (myReader.EndOfStream == false)
                {
                    text = myReader.ReadLine();
                    text1 = text.Split(new char[] { '\t', ',' });

                    comboBox2.Items.Add(text1[0]);
                    text = text.Replace('\t', ',');

                    moddiurnal.Add(text);
                    if (i == 0)
                    {
                        DataTable _data = new DataTable();
                        _data.Columns.Add("Hour", typeof(string));
                        _data.Columns.Add("Mod.", typeof(double));

                        for (int j = 1; j < 25; j++)
                        {
                            DataRow workrow;
                            workrow = _data.NewRow();
                            workrow[1] = St_F.TxtToDbl(text1[j], false);
                            workrow[0] = Convert.ToString(j - 1) + ":00h";
                            _data.Rows.Add(workrow);
                        }
                        DataView datasorted = new DataView();
                        datasorted = new DataView(_data); // create DataView from DataTable
                        dataGridView2.DataSource = datasorted; // connect DataView to GridView
                        dataGridView2.Columns["Mod."].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        dataGridView2.Columns["Hour"].ReadOnly = true;
                    }
                    i++;
                }
                comboBox2.SelectedIndex = 0;
                myReader.Close();
                myReader.Dispose();
            }
            catch
            {
                MessageBox.Show(this, "Error accessing resource file Modulation_diurnal.txt!", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //read diurnal pre-defined seasonal emission variations from ressources
            try
            {
                string newPath;
                _assembly = Assembly.GetExecutingAssembly();
                newPath = Path.Combine(Main.ProjectName, @"Settings", "seasonal_emissionmod.txt");

                StreamReader myReader;

                if (File.Exists(newPath) == true) // Try to read emission modulation
                {
                    myReader = new StreamReader(newPath);
                }
                else
                {
                    newPath = Path.Combine(Main.GUISettings.AppSettingsPath, "Emission_Mod_Seasonal.txt");
                    if (File.Exists(newPath) == true) // already existing diurnal emission data
                    {
                        myReader = new StreamReader(newPath);
                    }
                    else
                    {
                        myReader = new StreamReader(_assembly.GetManifestResourceStream("Gral.Resources.Modulation_seasonal.txt"));
                    }
                }

                string text = myReader.ReadLine();
                string[] text1 = new string[30];
                int i = 0;
                while (myReader.EndOfStream == false)
                {
                    text = myReader.ReadLine();
                    text1 = text.Split(new char[] { '\t', ',' });
                    comboBox1.Items.Add(text1[0]);
                    text = text.Replace('\t', ',');
                    modseasonal.Add(text);
                    if (i == 0)
                    {
                        DataTable _data = new DataTable();
                        _data.Columns.Add("Month", typeof(string));
                        _data.Columns.Add("Mod.", typeof(double));

                        for (int j = 1; j < 13; j++)
                        {
                            DataRow workrow;
                            workrow = _data.NewRow();
                            workrow[1] = St_F.TxtToDbl(text1[j], false);
                            workrow[0] = months[j - 1];
                            _data.Rows.Add(workrow);
                        }
                        DataView datasorted = new DataView();
                        datasorted = new DataView(_data); // create DataView from DataTable
                        dataGridView1.DataSource = datasorted; // connect DataView to GridView
                        dataGridView1.Columns["Mod."].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        dataGridView1.Columns["Month"].ReadOnly = true;
                    }
                    i++;
                }
                comboBox1.SelectedIndex = 0;
                myReader.Close();
                myReader.Dispose();

            }
            catch
            {
                MessageBox.Show(this, "Error accessing resource file Modulation_seasonal.txt!", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Close_Emissionvariation(int ind)
        {
            index = ind;
            Emissionvariation_Load(null, null);
            Button1_Click(null, null);
        }

        private void Emissionvariation_Load(object sender, EventArgs e)
        {

            if (e != null) // otherwise hidden form
            {
                //get information about the source group
                if (form1.listView1.SelectedIndices.Count <= 0)
                {
                    MessageBox.Show(this, "No source group selected!", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                    return;
                }
                index = Convert.ToInt32(form1.listView1.SelectedIndices[0]);
            }

            dataGridView1.RowHeadersWidth = 10;
            dataGridView1.Columns.Cast<DataGridViewColumn>().ToList().ForEach(fs => fs.SortMode = DataGridViewColumnSortMode.NotSortable);
            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView2.RowHeadersWidth = 10;
            dataGridView2.Columns.Cast<DataGridViewColumn>().ToList().ForEach(fs => fs.SortMode = DataGridViewColumnSortMode.NotSortable);
            dataGridView2.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridView2.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            string[] sg = new string[2];
            sg = form1.listView1.Items[index].SubItems[0].Text.Split(new char[] { ':' });
            string name = "";
            try
            {
                sg[1] = sg[1].Trim();
                name = sg[1];
            }
            catch
            {
                name = sg[0];
            }
            Text = "Emissionvariation      Source Group " + form1.listView1.Items[index].SubItems[0].Text;

            //load information about the selected diurnal and seasonal modulation for this source group
            string newPath = Path.Combine(Main.ProjectName, @"Settings", "emissionmodulations.txt");
            // in case of an emissionmodulation variation use the emissionmodulations.txt from the variation folder
            if (!string.Equals(Path.Combine(Main.ProjectName, @"Computation"), Main.ProjectSetting.EmissionModulationPath))
            {
                newPath = Path.Combine(Main.ProjectSetting.EmissionModulationPath, "emissionmodulations.txt");
            }

            string[] text2 = new string[26];
            string dummy;
            try
            {
                StreamReader _myReader = new StreamReader(newPath);

                for (int i = 0; i < 101; i++)
                {
                    dummy = _myReader.ReadLine();
                    if (dummy == null)
                    {
                        break;
                    }

                    text2 = dummy.Split(new char[] { ',' });
                    if (text2[0] == name)
                    {
                        //find the diurnal modulation
                        int j = 0;
                        foreach (string dum2 in comboBox2.Items)
                        {
                            if (dum2 == text2[1])
                            {
                                comboBox2.SelectedIndex = j;
                                break;
                            }
                            j += 1;
                        }
                        //find the seasonal modulation
                        j = 0;
                        foreach (string dum2 in comboBox1.Items)
                        {
                            if (dum2 == text2[2])
                            {
                                comboBox1.SelectedIndex = j;
                                break;
                            }
                            j += 1;
                        }
                    }
                }
                _myReader.Close();
                _myReader.Dispose();

                if (form1 != null)
                {
                    Location = new Point(Math.Max(0, form1.Location.X + form1.Width / 2 - Width / 2 - 100),
                                         Math.Max(0, form1.Location.Y + form1.Height / 2 - Height / 2 - 100));
                }

                changes_saved = true;
            }
            catch
            {
            }
        }

        //show listview for input of a new diurnal emission modulation and add new data
        private void Button20_Click(object sender, EventArgs e)
        {
            label2.Show();
            textBox1.Show();
            dataGridView2.Show();

            button6.Visible = true;
            string text = "";
            bool ok = true;
            if (textBox1.Text != "")
            {
                text += textBox1.Text;
                for (int i = 0; i < 24; i++)
                {
                    try
                    {
                        double dummy = (double)dataGridView2.Rows[i].Cells[1].Value;
                        text = text + "," + St_F.DblToIvarTxt(dummy);
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when reading modulation factor for " + Convert.ToString(i) + ":00 h", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ok = false;
                    }
                }
                if (ok == true)
                {
                    moddiurnal.Add(text);
                    comboBox2.Items.Add(textBox1.Text);
                    comboBox2.SelectedIndex = comboBox2.Items.Count - 1;
                }
            }

        }

        private string Check_Modulation_Name(string newText, List<string> mod_list)
        {
            string[] text1;
            foreach (string text in mod_list)
            {
                text1 = text.Split(new char[] { '\t', ',' });
                if (text1.Length > 0 && String.CompareOrdinal(newText, text1[0]) == 0)
                {
                    newText += "_";
                }
            }
            return newText;
        }

        //add new data for emission modulation SAVE Button
        private void Button6_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == String.Empty && textBox2.Text == String.Empty)
            {
                MessageBox.Show(this, "Define a name for this modulation, please", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            changes_saved = false;

            string text = "";
            bool ok = true;
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                textBox1.Text = Check_Modulation_Name(textBox1.Text, moddiurnal);

                text += textBox1.Text;
                for (int i = 0; i < 24; i++)
                {
                    try
                    {
                        double dummy = (double)dataGridView2.Rows[i].Cells[1].Value;
                        text = text + "," + St_F.DblToIvarTxt(dummy); //listView2.Items[i].Text.Replace(decsep, ".");
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when reading modulation factor for " + Convert.ToString(i) + ":00 h", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ok = false;
                    }
                }
                if (ok == true)
                {
                    moddiurnal.Add(text);
                    comboBox2.Items.Add(textBox1.Text);
                    comboBox2.SelectedIndex = comboBox2.Items.Count - 1;
                }
            }

            text = "";
            ok = true;
            if (textBox2.Text != "")
            {
                textBox2.Text = Check_Modulation_Name(textBox2.Text, modseasonal);

                text += textBox2.Text;
                for (int i = 0; i < 12; i++)
                {
                    try
                    {
                        double dummy = (double)dataGridView1.Rows[i].Cells[1].Value;
                        text = text + "," + St_F.DblToIvarTxt(dummy); //listView2.Items[i].Text.Replace(decsep, ".");
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when reading modulation factor in line " + Convert.ToString(i), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ok = false;
                    }
                }
                if (ok == true)
                {
                    modseasonal.Add(text);
                    comboBox1.Items.Add(textBox2.Text);
                    comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
                }
            }

        }

        //remove selected diurnal emission modulation
        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                changes_saved = false;
                if (moddiurnal.Count > 5)
                {
                    moddiurnal.RemoveAt(comboBox2.SelectedIndex);
                    comboBox2.Items.RemoveAt(comboBox2.SelectedIndex);
                    //listView1.Items.Clear();
                    comboBox2.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show(this, "There must remain at least five modulations", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
            }
        }

        //remove selected seasonal emission modulation
        private void Button3_Click(object sender, EventArgs e)
        {

            try
            {
                changes_saved = false;
                if (modseasonal.Count > 5)
                {
                    modseasonal.RemoveAt(comboBox1.SelectedIndex);
                    comboBox1.Items.RemoveAt(comboBox1.SelectedIndex);
                    //listView2.Items.Clear();
                    comboBox1.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show(this, "There must remain at least five modulations", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
            }
        }

        //show listview for input of a new seasonal emission modulation
        private void Button19_Click(object sender, EventArgs e)
        {
            label1.Show();
            textBox2.Show();
            dataGridView1.Show();
            changes_saved = false;

            button6.Visible = true;
            string text = "";
            bool ok = true;
            if (textBox2.Text != "")
            {
                text += textBox2.Text;
                for (int i = 0; i < 12; i++)
                {
                    try
                    {
                        //double dummy = Convert.ToDouble(listView2.Items[i].Text.Replace(decsep, "."));
                        double dummy = (double)dataGridView1.Rows[i].Cells[1].Value;
                        text = text + "," + dataGridView1.Rows[i].Cells[1].Value.ToString().Replace(decsep, ".");
                    }
                    catch
                    {
                        MessageBox.Show(this, "Error when reading modulation factor in line " + Convert.ToString(i), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ok = false;
                    }
                }
                if (ok == true)
                {
                    modseasonal.Add(text);
                    comboBox1.Items.Add(textBox2.Text);
                    comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
                }
            }
        }

        //close form and save all entries
        private void Button1_Click(object sender, EventArgs e)
        {
            string newPath = String.Empty;
            //write emission variations to files
            newPath = Path.Combine(Main.ProjectName, @"Settings", "diurnal_emissionmod.txt");
            try
            {
                using (StreamWriter myWriter = new StreamWriter(newPath))
                {
                    myWriter.WriteLine("Diurnal variation of emissions");
                    foreach (string text in moddiurnal)
                    {
                        myWriter.WriteLine(text);
                    }
                }
            }
            catch { }

            newPath = Path.Combine(Main.ProjectName, @"Settings", "seasonal_emissionmod.txt");
            try
            {
                using (StreamWriter myWriter = new StreamWriter(newPath))
                {
                    myWriter.WriteLine("Seasonal variation of emissions");
                    foreach (string text in modseasonal)
                    {
                        myWriter.WriteLine(text);
                    }
                }
            }
            catch { }


            //write emission variations to the necessary input files for GRAL
            //int index = Convert.ToInt32(form1.listView1.SelectedIndices[0]); // Kuntner index is global now and saved at the start!
            string[] sg = new string[2];
            sg = form1.listView1.Items[index].SubItems[0].Text.Split(new char[] { ':' });
            string name = "";
            try
            {
                sg[1] = sg[1].Trim();
                name = "00" + sg[1];
            }
            catch
            {
                name = "00" + sg[0];
            }
            name = name.Substring(name.Length - 3);
            newPath = Path.Combine(Main.ProjectName, @"Computation", "emissions" + name + ".dat");
            if (Directory.Exists(Main.ProjectSetting.EmissionModulationPath))
            {
                newPath = Path.Combine(Main.ProjectSetting.EmissionModulationPath, "emissions" + name + ".dat");
            }

            string[] text1 = new string[25];
            string[] text2 = new string[13];

            try
            {
                using (StreamWriter myWriter = new StreamWriter(newPath))
                {
                    text1 = moddiurnal[comboBox2.SelectedIndex].Split(new char[] { ',' });
                    text2 = modseasonal[comboBox1.SelectedIndex].Split(new char[] { ',' });
                    for (int i = 1; i < 25; i++)
                    {
                        if (i < 13)
                        {
                            myWriter.WriteLine(Convert.ToInt32(i - 1) + "," + St_F.DblToIvarTxt(St_F.TxtToDbl(text1[i], false)) + "," + St_F.DblToIvarTxt(St_F.TxtToDbl(text2[i], false)));
                        }
                        else
                        {
                            myWriter.WriteLine(Convert.ToInt32(i - 1) + "," + St_F.DblToIvarTxt(St_F.TxtToDbl(text1[i], false)));
                        }
                    }
                }
            }
            catch { }

            form1.listView1.Items[index].SubItems[0].BackColor = Color.LightGreen;

            //save information about the selected diurnal and seasonal modulation for this source group
            newPath = Path.Combine(Main.ProjectName, @"Settings", "emissionmodulations.txt");
            // in case of an emissionmodulation variation use the emissionmodulations.txt from the variation folder
            if (!string.Equals(Path.Combine(Main.ProjectName, @"Computation"), Main.ProjectSetting.EmissionModulationPath))
            {
                newPath = Path.Combine(Main.ProjectSetting.EmissionModulationPath, "emissionmodulations.txt");
            }

            string[] dummy = new string[101];
            int ind = 0;
            try
            {
                using (StreamReader _myReader = new StreamReader(newPath))
                {
                    ind = -1;
                    for (int i = 0; i < 101; i++)
                    {
                        dummy[i] = _myReader.ReadLine();
                        if (dummy[i] == null)
                        {
                            if (ind == -1)
                            {
                                ind = Math.Max(i, ind);
                            }

                            break;
                        }
                        text2 = dummy[i].Split(new char[] { ',' });
                        if (text2[0] == (name.Substring(0, 2).Replace("0", "") + name.Substring(2, 1)))
                        {
                            ind = i;
                        }
                    }
                }

            }
            catch
            {
                ind = 0;
            }

            try
            {
                using (StreamWriter myWriter = new StreamWriter(newPath))
                {
                    for (int i = 0; i < ind; i++)
                    {
                        myWriter.WriteLine(dummy[i]);
                    }
                    myWriter.WriteLine(name.Substring(0, 2).Replace("0", "") + name.Substring(2, 1) + "," + comboBox2.SelectedItem + "," + comboBox1.SelectedItem);
                    for (int i = ind + 1; i < 101; i++)
                    {
                        if (dummy[i] != null)
                        {
                            myWriter.WriteLine(dummy[i]);
                        }
                    }
                }
            }
            catch { }

            Application.DoEvents();

            //check if all source groups have defined emission modulations
            int check = 1;
            for (int i = 0; i < form1.listView1.Items.Count; i++)
            {
                if (form1.listView1.Items[i].SubItems[0].BackColor == Color.LightGoldenrodYellow)
                {
                    check = 0;
                }
            }

            //fill form1.listbox5 with available pollutants
            if (check == 1 && !Main.Project_Locked)
            {
                form1.listBox5.Items.Clear();
                form1.button18.Visible = false;
                form1.button21.Visible = false;
                form1.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonEmission, ButtonColorEnum.Invisible); // Emission label invisible

                foreach (string text in form1.Pollmod)
                {
                    form1.listBox5.Items.Add(text);
                    form1.listBox5.SelectedIndex = 0;
                    form1.button18.Visible = true;
                    form1.button21.Visible = true;
                    form1.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonEmission, ButtonColorEnum.RedDot); // Emission label red
                }
            }

            //enable/disable GRAL simulations
            form1.Enable_GRAL();
            changes_saved = true;
            Close();
        }

        //select diurnal emission variation
        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] text1 = new string[30];
            //listView1.Items.Clear();
            text1 = moddiurnal[comboBox2.SelectedIndex].Split(new char[] { ',' });
            changes_saved = false;

            DataTable _data = new DataTable();
            _data.Columns.Add("Hour", typeof(string));
            _data.Columns.Add("Mod.", typeof(double));

            for (int i = 1; i < 25; i++)
            {
                DataRow workrow;
                workrow = _data.NewRow();
                workrow[1] = St_F.TxtToDbl(text1[i], false);
                workrow[0] = Convert.ToString(i - 1) + ":00h";
                _data.Rows.Add(workrow);
            }
            DataView datasorted = new DataView();
            datasorted = new DataView(_data); // create DataView from DataTable
            dataGridView2.DataSource = datasorted; // connect DataView to GridView
            dataGridView2.Columns["Mod."].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView2.Columns["Hour"].ReadOnly = true;

            panel2.Refresh();
        }

        //select seasonal emission variation
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] text1 = new string[30];
            //listView2.Items.Clear();
            text1 = modseasonal[comboBox1.SelectedIndex].Split(new char[] { ',' });
            changes_saved = false;

            DataTable _data = new DataTable();
            _data.Columns.Add("Month", typeof(string));
            _data.Columns.Add("Mod.", typeof(double));

            for (int j = 1; j < 13; j++)
            {
                DataRow workrow;
                workrow = _data.NewRow();
                workrow[1] = St_F.TxtToDbl(text1[j], false);
                workrow[0] = months[j - 1];
                _data.Rows.Add(workrow);
            }
            DataView datasorted = new DataView();
            datasorted = new DataView(_data); // create DataView from DataTable
            dataGridView1.DataSource = datasorted; // connect DataView to GridView
            dataGridView1.Columns["Mod."].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns["Month"].ReadOnly = true;

            panel1.Refresh();
        }

        //draw selected seasonal emission modulation
        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            Pen p = new Pen(Color.Black, 2);
            Pen p1 = new Pen(Color.Black, 1);
            Brush b1 = new HatchBrush(HatchStyle.LightDownwardDiagonal, Color.CornflowerBlue, Color.Transparent);
            Brush b2 = new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.Brown, Color.Transparent);
            Font _smallFont = new Font("Arial", 8);
            StringFormat format2 = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };
            Brush _blackBrush = new SolidBrush(Color.Black);

            e.Graphics.DrawLine(p, Convert.ToInt32(30 * pixel), Convert.ToInt32(10 * pixel), Convert.ToInt32(30 * pixel), Convert.ToInt32(280 * pixel));
            e.Graphics.DrawLine(p, Convert.ToInt32(30 * pixel), Convert.ToInt32(280 * pixel), Convert.ToInt32(540 * pixel), Convert.ToInt32(280 * pixel));
            //compute the maximum
            string[] text1 = new string[30];
            double[] emfac = new double[12];
            double emfacmax = 0;
            text1 = modseasonal[comboBox1.SelectedIndex].Split(new char[] { ',' });
            for (int j = 1; j < 13; j++)
            {
                emfac[j - 1] = Convert.ToDouble(text1[j].Replace(".", decsep));
                emfacmax = Math.Max(emfacmax, emfac[j - 1]);
            }
            emfacmax = Math.Max(0.1, emfacmax);
            double scale = Convert.ToInt32(260 * pixel) / emfacmax;
            double div = Math.Round(emfacmax / 5, 1);
            for (int i = 0; i < 12; i++)
            {
                e.Graphics.FillRectangle(b1, Convert.ToInt32(30 * pixel) + Convert.ToInt32(42 * pixel) * i, Convert.ToInt32(280 * pixel) - Convert.ToInt32(emfac[i] * scale), Convert.ToInt32(42 * pixel), Convert.ToInt32(emfac[i] * scale));
                e.Graphics.DrawRectangle(p1, Convert.ToInt32(30 * pixel) + Convert.ToInt32(42 * pixel) * i, Convert.ToInt32(280 * pixel) - Convert.ToInt32(emfac[i] * scale), Convert.ToInt32(42 * pixel), Convert.ToInt32(emfac[i] * scale));
                e.Graphics.DrawString(months[i], _smallFont, _blackBrush, Convert.ToInt32(51 * pixel) + Convert.ToInt32(42 * pixel) * i, Convert.ToInt32(290 * pixel), format2);
            }
            for (int i = 0; i < 5; i++)
            {
                e.Graphics.DrawLine(p, Convert.ToInt32(25 * pixel), Convert.ToInt32(Convert.ToInt32(280 * pixel) - (i + 1) * div * scale), Convert.ToInt32(30 * pixel), Convert.ToInt32(Convert.ToInt32(280 * pixel) - (i + 1) * div * scale));
                e.Graphics.DrawString(Math.Round(div * (i + 1), 2).ToString(), _smallFont, _blackBrush, Convert.ToInt32(12 * pixel), Convert.ToInt32(Convert.ToInt32(280 * pixel) - (i + 1) * div * scale), format2);
            }
            e.Graphics.DrawString("Seasonal emission modulation", new Font("Arial", 12), _blackBrush, Convert.ToInt32(275 * pixel), Convert.ToInt32(10 * pixel), format2);

            p.Dispose(); p1.Dispose();
            b1.Dispose(); b2.Dispose();
            _smallFont.Dispose();
            format2.Dispose();
            _blackBrush.Dispose();
        }

        //draw selected diurnal emission modulation
        private void Panel2_Paint(object sender, PaintEventArgs e)
        {
            Pen p = new Pen(Color.Black, 2);
            Pen p1 = new Pen(Color.Black, 1);
            Brush b1 = new HatchBrush(HatchStyle.LightDownwardDiagonal, Color.CornflowerBlue, Color.Transparent);
            Brush b2 = new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.Brown, Color.Transparent);
            Font _smallFont = new Font("Arial", 8);
            StringFormat format2 = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };
            Brush _blackBrush = new SolidBrush(Color.Black);

            e.Graphics.DrawLine(p, Convert.ToInt32(30 * pixel), Convert.ToInt32(10 * pixel), Convert.ToInt32(30 * pixel), Convert.ToInt32(280 * pixel));
            e.Graphics.DrawLine(p, Convert.ToInt32(30 * pixel), Convert.ToInt32(280 * pixel), Convert.ToInt32(540 * pixel), Convert.ToInt32(280 * pixel));
            //compute the maximum
            string[] text1 = new string[30];
            double[] emfac = new double[24];
            double emfacmax = 0;
            text1 = moddiurnal[comboBox2.SelectedIndex].Split(new char[] { ',' });
            for (int j = 1; j < 25; j++)
            {
                emfac[j - 1] = Convert.ToDouble(text1[j].Replace(".", decsep));
                emfacmax = Math.Max(emfacmax, emfac[j - 1]);
            }
            emfacmax = Math.Max(0.1, emfacmax);
            double scale = Convert.ToInt32(260 * pixel) / emfacmax;
            double div = Math.Round(emfacmax / 5, 1);
            int counter = 3;
            for (int i = 0; i < 24; i++)
            {
                e.Graphics.FillRectangle(b2, Convert.ToInt32(30 * pixel) + Convert.ToInt32(21 * pixel) * i, Convert.ToInt32(280 * pixel) - Convert.ToInt32(emfac[i] * scale), Convert.ToInt32(21 * pixel), Convert.ToInt32(emfac[i] * scale));
                e.Graphics.DrawRectangle(p1, Convert.ToInt32(30 * pixel) + Convert.ToInt32(21 * pixel) * i, Convert.ToInt32(280 * pixel) - Convert.ToInt32(emfac[i] * scale), Convert.ToInt32(21 * pixel), Convert.ToInt32(emfac[i] * scale));
                if (counter == 3)
                {
                    e.Graphics.DrawString(Convert.ToString(i) + ":00h", _smallFont, _blackBrush, Convert.ToInt32(42 * pixel) + Convert.ToInt32(21 * pixel) * i, Convert.ToInt32(290 * pixel), format2);
                    counter = 0;
                }
                counter += 1;
            }
            for (int i = 0; i < 5; i++)
            {
                e.Graphics.DrawLine(p, Convert.ToInt32(25 * pixel), Convert.ToInt32(Convert.ToInt32(280 * pixel) - (i + 1) * div * scale), Convert.ToInt32(30 * pixel), Convert.ToInt32(Convert.ToInt32(280 * pixel) - (i + 1) * div * scale));
                e.Graphics.DrawString(Math.Round(div * (i + 1), 2).ToString(), _smallFont, _blackBrush, Convert.ToInt32(12 * pixel), Convert.ToInt32(Convert.ToInt32(280 * pixel) - (i + 1) * div * scale), format2);
            }
            e.Graphics.DrawString("Diurnal emission modulation", new Font("Arial", 12), _blackBrush, Convert.ToInt32(275 * pixel), Convert.ToInt32(10 * pixel), format2);

            p.Dispose(); p1.Dispose();
            b1.Dispose(); b2.Dispose();
            _smallFont.Dispose();
            format2.Dispose();
            _blackBrush.Dispose();
        }

        //copy figure to clipboard
        private void Button4_Click(object sender, EventArgs e)
        {
            Bitmap bitMap = new Bitmap(panel1.Width, panel1.Height);
            panel1.DrawToBitmap(bitMap, new Rectangle(0, 0, panel1.Width, panel1.Height));
            Clipboard.SetDataObject(bitMap);
        }

        private void Button5_Click_1(object sender, EventArgs e)
        {
            Bitmap bitMap = new Bitmap(panel2.Width, panel2.Height);
            panel2.DrawToBitmap(bitMap, new Rectangle(0, 0, panel2.Width, panel2.Height));
            Clipboard.SetDataObject(bitMap);
        }


        void DataGridView2DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0.0;
        }


        void DataGridView1DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0.0;
        }

        void EmissionvariationFormClosing(object sender, FormClosingEventArgs e)
        {
            if (changes_saved == false)
            {
                if (MessageBox.Show(this, "The settings are not saved. Do you like to save the new settings?", "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Button1_Click(null, null);
                }
            }

        }

        void DataGridView1KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Modifiers == Keys.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.C:
                            break;

                        case Keys.V:
                            PasteClipboard(dataGridView1);
                            break;
                    }
                }
            }
            catch
            {

            }
        }
        void DataGridView2KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Modifiers == Keys.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.C:
                            break;

                        case Keys.V:
                            PasteClipboard(dataGridView2);
                            break;
                    }
                }
            }
            catch
            {

            }
        }

        //input of values
        void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dataGrid = sender as DataGridView;
            int col = e.ColumnIndex;
            int row = e.RowIndex;

            //check that only numbers are input
            double val = 0;

            if (col >= 0 && row >= 0 && dataGrid.Rows[row].Cells[col].Value != null)
            {
                if (double.TryParse(dataGrid.Rows[row].Cells[col].Value.ToString(), out val))
                {
                    bool err = false;
                    if (val < 0)
                    {
                        MessageBox.Show(this, "The number needs to be > 0", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        err = true;
                    }

                    if (err)
                    {
                        dataGrid.Rows[row].Cells[col].Value = "0";
                        //dataGridView1.Rows[row].Cells[col].Selected = true;
                        //dataGridView1.CurrentCell = dataGridView1.Rows[row].Cells[col];
                    }

                }
                else
                {
                    dataGrid.Rows[row].Cells[col].Value = "0";
                    MessageBox.Show(this, "Only numerical inputs are allowed", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        void EmissionvariationFormClosed(object sender, FormClosedEventArgs e)
        {
            form1.ListView1ItemActivate(null, null);

            toolTip1.Dispose();
            dataGridView1.Dispose();
            dataGridView2.Dispose();
            panel1.Dispose();
            panel2.Dispose();
            button1.Dispose();
        }

        void PasteClipboard(DataGridView dataGrid)
        {
            try
            {
                string s = Clipboard.GetText();
                string[] lines = s.Split('\n');

                int iRow = dataGrid.CurrentCell.RowIndex;
                int iCol = dataGrid.CurrentCell.ColumnIndex;
                DataGridViewCell oCell;

                foreach (string line in lines)
                {
                    if (iRow < dataGrid.RowCount && line.Length > 0)
                    {
                        string[] sCells = line.Split('\t');
                        for (int i = 0; i < sCells.GetLength(0); ++i)
                        {
                            double test = 0;
                            if (double.TryParse(sCells[i], out test) == false)
                            {
                                sCells[i] = test.ToString("0.0");
                            }
                            if (iCol + i < dataGrid.ColumnCount)
                            {
                                oCell = dataGrid[iCol + i, iRow];
                                oCell.Value = Convert.ChangeType(sCells[i].Replace("\r", ""), oCell.ValueType);
                            }
                            else
                            {
                                break;
                            }
                        }
                        iRow++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (FormatException)
            {
                return;
            }
        }
    }
}