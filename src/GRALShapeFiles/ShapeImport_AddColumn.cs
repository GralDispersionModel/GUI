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
using System.Data;
using System.Globalization;
using System.Windows.Forms;

namespace GralShape
{
    /// <summary>
    /// Add a column to a shape file data table
    /// </summary>
    public partial class ShapeImport_AddColumn : Form
    {
        private readonly string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        private string _columnName;
        public string ColumnName { get { return _columnName; } }
        private string _equation;
        public string Equation { get { return _equation; } }

        public ShapeImport_AddColumn()
        {
            InitializeComponent();
            //only point as decimal seperator is allowed
            //textBox2.KeyPress += new KeyPressEventHandler(comma);
            button2.DialogResult = DialogResult.OK;
            _columnName = String.Empty;
            _equation = String.Empty;
        }

        /// <summary>
        /// Close form, do nothing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            _columnName = String.Empty;
            _equation = String.Empty;
            Close();
        }

        /// <summary>
        /// Add column and perform math operation if required
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            _columnName = textBox1.Text;
            _equation = textBox2.Text;
            Hide();
        }

        /// <summary>
        /// Set complete or partial DataTable to a DataGridView 
        /// </summary>
        /// <param name="dataGridView">A DataGridView</param>
        /// <param name="dataTable">The Data Table</param>
        /// <param name="Complete">Show complete (true) or a subset (false)</param>
        public static void SetDataGridViewSource(DataGridView DataGridView1, DataTable DataTable1, bool Complete)
        {
            if (Complete)
            {
                DataGridView1.DataSource = DataTable1;
            }
            else
            {
                //show a subset of 150 rows at the dataGridView
                DataTable _dtLoc = DataTable1.Clone();
                int _c = 0;
                foreach (DataRow _row in DataTable1.Rows)
                {
                    _dtLoc.ImportRow(_row);
                    _c++;
                    if (_c > 150)
                    {
                        break;
                    }
                }
                DataGridView1.DataSource = _dtLoc;
            }
        }

        /// <summary>
        /// Add a column to the data table
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dataGridView1"></param>
        /// <param name="Column"></param>
        /// <param name="Equation"></param>
        /// <param name="ShowEntireDataset"></param>
        /// <returns>true if successful</returns>
        public bool AddColumn(DataTable dt, DataGridView dataGridView1, string Column, string Equation, bool ShowEntireDataset)
        {
            bool ok = false;
            try
            {
                dataGridView1.DataSource = null;
                if (!dt.Columns.Contains(Column))
                {
                    dt.Columns.Add(Column, typeof(String));

                    //check if there is a fix value or an equation given
                    if (Equation.Substring(0, 1) == "=")
                    {
                        //math operation
                        MathFunctions.MathParser mp = new MathFunctions.MathParser();
                        string mathtext;
                        decimal a;
                        decimal result;
                        mathtext = Equation.Replace(".", decsep);
                        mathtext = mathtext.Replace(",", decsep);
                        int stringcount = 0;
                        string headername = "";

                        //search for column names in mathtext
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            if (mathtext.Contains(dt.Columns[j].ColumnName) == true)
                            {
                                if (dt.Columns[j].ColumnName.Length > stringcount)
                                {
                                    stringcount = dt.Columns[j].ColumnName.Length;
                                    headername = dt.Columns[j].ColumnName;
                                }
                            }
                        }

                        if (headername.Length > 0)
                        {
                            mathtext = mathtext.Replace(headername, "A");
                            mathtext = mathtext.Remove(0, 1);

                            //perform calculation
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                try
                                {
                                    a = Convert.ToDecimal(dt.Rows[i][headername]);
                                    mp.Parameters.Clear();
                                    mp.Parameters.Add(MathFunctions.Parameters.A, a);
                                    result = mp.Calculate(mathtext);
                                    dt.Rows[i][Column] = Convert.ToString(result);
                                }
                                catch
                                {
                                }
                            }
                            dt.AcceptChanges(); // accept changes
                        }
                        else
                        {
                            dt.Columns.Remove(Column);
                        }
                    }
                    else
                    {
                        //fill data grid with fix value
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            dt.Rows[i][Column] = Equation;
                        }
                        dt.AcceptChanges(); // accept changes
                    }
                    ok = true;
                }
                else
                {
                    MessageBox.Show(this, "This column name already exist", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                ShapeImport_AddColumn.SetDataGridViewSource(dataGridView1, dt, ShowEntireDataset);
            }
            return ok;
        }

        /// <summary>
        /// Remove one column
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dataGridView1"></param>
        /// <param name="ShowEntireDataset"></param>
        /// <returns>string.empty if not successful</returns>
        public string RemoveColumn(DataTable dt, DataGridView dataGridView1, bool ShowEntireDataset)
        {
            string header = "Height";
            if (GralStaticFunctions.St_F.InputBoxValue("Remove column", "Name:", ref header, this) == DialogResult.OK)
            {
                dataGridView1.DataSource = null;
                try
                {
                    dt.Columns.Remove(header);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.ToString(), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    header = string.Empty;
                }
                finally
                {
                    ShapeImport_AddColumn.SetDataGridViewSource(dataGridView1, dt, ShowEntireDataset);
                }
            }
            else
            {
                header = string.Empty;
            }
            return header;
        }
    }
}