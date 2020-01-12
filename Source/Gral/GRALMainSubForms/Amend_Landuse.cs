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

using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using GralStaticFunctions;

namespace GralMainForms
{
	/// <summary>
    /// Dialog to set user defined land-use settings
    /// </summary>
    public partial class Amend_Landuse : Form
    {
        private CultureInfo ic = CultureInfo.InvariantCulture;
        
        public Amend_Landuse()
        {
            InitializeComponent();

            dataGridView1.CellEndEdit += new DataGridViewCellEventHandler(dataGridView1_CellEndEdit);

            dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;

            //searching and reading for file containing default values or additional landuse values or updated landuse values
            bool defaultvalues = File.Exists(Path.Combine(Gral.Main.ProjectName, @"Computation", "Landuse_Default.txt"));
            if (defaultvalues == true)
            {
                try
                {
                    using (StreamReader reader1 = new StreamReader(Path.Combine(Gral.Main.ProjectName, @"Computation","Landuse_Default.txt")))
                    {
                        string[] text1 = new string[10];
                        try
                        {
                            //header line
                            text1 = reader1.ReadLine().Split(new char[] {'\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            int n = 0;
                            //consecutive lines containing landuse values
                            while ((text1 = reader1.ReadLine().Split(new char[] {'\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) != null)
                            {
                                dataGridView1.Rows.Add();
                                dataGridView1.Rows[n].Cells[0].Value = St_F.ICultToLCult(text1[0]);
                                dataGridView1.Rows[n].Cells[1].Value = St_F.ICultToLCult(text1[1]);
                                dataGridView1.Rows[n].Cells[2].Value = St_F.ICultToLCult(text1[2]);
                                dataGridView1.Rows[n].Cells[3].Value = St_F.ICultToLCult(text1[3]);
                                dataGridView1.Rows[n].Cells[4].Value = St_F.ICultToLCult(text1[4]);
                                dataGridView1.Rows[n].Cells[5].Value = St_F.ICultToLCult(text1[5]);
                                dataGridView1.Rows[n].Cells[6].Value = St_F.ICultToLCult(text1[6]);
                                n++;
                            }
                        }
                        catch { }
                    }
                    
                }
                catch
                {
                    MessageBox.Show(this, "Unable to read file Landuse_Default.txt","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                dataGridView1.Rows.Add();
            }
        }

        //input of values
        void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int col = e.ColumnIndex;
            int row = e.RowIndex;
            
            //check that only numbers are input
            double val = 0;

            if (col >= 0 && row >= 0 && dataGridView1.Rows[row].Cells[col].Value != null)
            {
                if (double.TryParse(dataGridView1.Rows[row].Cells[col].Value.ToString(), out val))
                {
                    bool err = false;
                    if (col == 0 && val > 999)
                    {
                        MessageBox.Show(this, "The land-use code needs to be lower than 1000", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        err = true;
                    }
                    if (val < 0)
                    {
                        MessageBox.Show(this, "The number needs to be > 0", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        err = true;
                    }
                    if (col == 1 && val > 1)
                    {
                        MessageBox.Show(this, "Albedo needs to be <= 1", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        err = true;
                    }
                    if (col == 2 && val > 1)
                    {
                        MessageBox.Show(this, "Emissivity needs to be <= 1", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        err = true;
                    }
                    if (col == 3 && val > 1)
                    {
                        MessageBox.Show(this, "Soil moisture needs to be <= 1", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        err = true;
                    }
                    if (col == 4 && val > 5)
                    {
                        MessageBox.Show(this, "Roughness length needs to be <= 5m", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        err = true;
                    }
                    if (col == 5 && val > 100)
                    {
                        MessageBox.Show(this, "Heat conductivity needs to be <= 100", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        err = true;
                    }
                    if (col == 6 && val > 5E-6)
                    {
                        MessageBox.Show(this, "Thermal conductivity needs to be <= 5E-6", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        err = true;
                    }

                    if (err)
                    {
                        dataGridView1.Rows[row].Cells[col].Value = String.Empty;
                        //dataGridView1.Rows[row].Cells[col].Selected = true;
                        //dataGridView1.CurrentCell = dataGridView1.Rows[row].Cells[col];
                    }

                }
                else
                {
                    dataGridView1.Rows[row].Cells[col].Value = String.Empty;
                    MessageBox.Show(this, "Only numerical inputs are allowed", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        
        void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //löschen einer zeile
            if (e.Button == MouseButtons.Right)
            {
                //löschen einer ganzen eingabezeile
                if (e.RowIndex > 0)
                {
                    if (MessageBox.Show(this, "Do you really want to delete this row?", "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            dataGridView1.Rows.Remove(dataGridView1.Rows[e.RowIndex]);
                        }
                        catch
                        { }
                    }
                }
            }
        }


        //hide window and save file
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                
                bool Writing_OK = false;
                
                using (StreamWriter wr = new StreamWriter(Path.Combine(Gral.Main.ProjectName, @"Computation","Landuse_Default.txt")))
                {
                    wr.WriteLine("Code \t Albedo [-] \t Emissivity [-] \t Soil moisture [-] \t Roughness length [m] \t Heat conductivity [W/(mK)] \t Thermal conductivity [m2/s]");

                    //check if there are empty cells
                    bool notsave = false;
                    for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                    {
                        int nrow = 0;
                        int ncol = 0;

                        int nrow1 = 0;
                        int nrow2 = 0;
                        int ncol2 = 0;
                        int nrow3 = 0;
                        int nrow4 = 0;
                        int nrow5 = 0;
                        int nrow6 = 0;
                        int nrow7 = 0;
                        int nrow8 = 0;

                        bool leer = false;
                        bool CLCmax = false;
                        bool CLCmin = false;
                        bool albedo = false;
                        bool emi = false;
                        bool FP = false;
                        bool Z0 = false;
                        bool HC = false;
                        bool TC = false;

                        for (int j = 0; j < dataGridView1.Columns.Count; j++)
                        {
                            //check if land-use code is in the range 0-999
                            if (Convert.ToInt32(dataGridView1.Rows[i].Cells[0].Value) > 999)
                            {
                                CLCmax = true;
                                nrow1 = i;
                                break;
                            }
                            //check if any data is below zero
                            if (Convert.ToDouble(Convert.ToString(dataGridView1.Rows[i].Cells[j].Value)) < 0)
                            {
                                CLCmin = true;
                                nrow2 = i;
                                ncol2 = j;
                                break;
                            }
                            //check if any field is empty
                            if (dataGridView1.Rows[i].Cells[j].Value == null)
                            {
                                leer = true;
                                nrow = i;
                                ncol = j;
                                break;
                            }
                            //check if albedo is below one
                            if (Convert.ToDouble(Convert.ToString(dataGridView1.Rows[i].Cells[1].Value)) > 1)
                            {
                                albedo = true;
                                nrow3 = i;
                                break;
                            }
                            //check if emissivity is below one
                            if (Convert.ToDouble(Convert.ToString(dataGridView1.Rows[i].Cells[2].Value)) > 1)
                            {
                                emi = true;
                                nrow4 = i;
                                break;
                            }
                            //check if moisture parameter is below one
                            if (Convert.ToDouble(Convert.ToString(dataGridView1.Rows[i].Cells[3].Value)) > 1)
                            {
                                FP = true;
                                nrow5 = i;
                                break;
                            }
                            //check if roughness length is below 5
                            if (Convert.ToDouble(Convert.ToString(dataGridView1.Rows[i].Cells[4].Value)) > 5)
                            {
                                Z0 = true;
                                nrow6 = i;
                                break;
                            }
                            //check if heat conductivity is below 100
                            if (Convert.ToDouble(Convert.ToString(dataGridView1.Rows[i].Cells[5].Value)) > 100)
                            {
                                HC = true;
                                nrow7 = i;
                                break;
                            }
                            //check if thermal conductivity is below 5E-6
                            if (Convert.ToDouble(Convert.ToString(dataGridView1.Rows[i].Cells[6].Value)) > 0.000005)
                            {
                                TC = true;
                                nrow8 = i;
                                break;
                            }
                        }
                        if (leer == true)
                        {
                            MessageBox.Show(this, "There are empty cells. Please fill in a suitable value or delete complete row.","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            dataGridView1.CurrentCell = dataGridView1.Rows[nrow].Cells[ncol];
                            notsave = true;
                            wr.Close();
                            break;
                        }
                        if (CLCmax == true)
                        {
                            MessageBox.Show(this, "The land-use code needs to be lower than 1000","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            dataGridView1.CurrentCell = dataGridView1.Rows[nrow1].Cells[0];
                            notsave = true;
                            wr.Close();
                            break;
                        }
                        if (CLCmin == true)
                        {
                            MessageBox.Show(this, "The number needs to be > 0","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            dataGridView1.CurrentCell = dataGridView1.Rows[nrow2].Cells[ncol2];
                            notsave = true;
                            wr.Close();
                            break;
                        }
                        if (albedo == true)
                        {
                            MessageBox.Show(this, "Albedo needs to be <= 1","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            dataGridView1.CurrentCell = dataGridView1.Rows[nrow3].Cells[1];
                            notsave = true;
                            wr.Close();
                            break;
                        }
                        if (emi == true)
                        {
                            MessageBox.Show(this, "Emissivity needs to be <= 1","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            dataGridView1.CurrentCell = dataGridView1.Rows[nrow4].Cells[2];
                            notsave = true;
                            wr.Close();
                            break;
                        }
                        if (FP == true)
                        {
                            MessageBox.Show(this, "Soil moisture needs to be <= 1","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            dataGridView1.CurrentCell = dataGridView1.Rows[nrow5].Cells[3];
                            notsave = true;
                            wr.Close();
                            break;
                        }
                        if (Z0 == true)
                        {
                            MessageBox.Show(this, "Roughness length needs to be <= 5m","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            dataGridView1.CurrentCell = dataGridView1.Rows[nrow6].Cells[4];
                            notsave = true;
                            wr.Close();
                            break;
                        }
                        if (HC == true)
                        {
                            MessageBox.Show(this, "Heat conductivity needs to be <= 100","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            dataGridView1.CurrentCell = dataGridView1.Rows[nrow7].Cells[5];
                            notsave = true;
                            wr.Close();
                            break;
                        }
                        if (TC == true)
                        {
                            MessageBox.Show(this, "Thermal conductivity needs to be <= 5E-6","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            dataGridView1.CurrentCell = dataGridView1.Rows[nrow8].Cells[6];
                            notsave = true;
                            wr.Close();
                            break;
                        }
                    }
                    if (notsave == false)
                    {

                        //save data
                        for (int j = 0; j < dataGridView1.Rows.Count - 1; j++)
                        {
                            if (dataGridView1.Rows[j].Cells[0] != null && Convert.ToString(dataGridView1.Rows[j].Cells[0].Value) != String.Empty)
                            {
                                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                                {
                                    wr.Write(St_F.StrgToICult(Convert.ToString(dataGridView1.Rows[j].Cells[i].Value)) + "\t");
                                }
                                wr.WriteLine();
                            }
                        }

                        Writing_OK = true;
                    }
                }
                if (Writing_OK)
                {
                    Close();
                }
            }
            catch
            {
                MessageBox.Show(this, "Unable to write file Landuse_Default.txt","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void Amend_LanduseLoad(object sender, EventArgs e)
        {
            if (Owner != null)
                Location = new Point(Math.Max(0,Owner.Location.X + Owner.Width / 2 - Width / 2 - 100),
                                     Math.Max(0, Owner.Location.Y + Owner.Height / 2 - Height / 2 -100));
        }
        void Amend_LanduseFormClosed(object sender, FormClosedEventArgs e)
        {
            toolTip1.Dispose();
        }
        
        
    }
}
