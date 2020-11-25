#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2020]  [Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Data;

namespace GralDomForms
{
    public delegate void StartMultiplePointsWorker(object sender, EventArgs e);
    public delegate void CancelMultiplePoints(object sender, EventArgs e);

    public partial class SelectMultiplePoints : Form
    {
        /// <summary>
        /// delegate to send Message, that function should start
        /// </summary>
        public event StartMultiplePointsWorker StartComputation;
        /// <summary>
        /// delegate to send Message, that function is cancelled!
        /// </summary>
        public event CancelMultiplePoints CancelComputation;
        /// <summary>
        /// Values: 0 - no model selection,  Bit 1 GRAMM, Bit 2 GRAL
        /// </summary>
        public GralDomain.MeteoModelEmum MeteoModel = GralDomain.MeteoModelEmum.None;
        /// <summary>
        /// true: allow selection of local stability classes
        /// </summary>
        public bool LocalStability = false;
        /// <summary>
        /// User defined data
        /// </summary>
        public DataTable PointCoorData = new DataTable();
        /// <summary>
        /// File name 
        /// </summary>
        public string MeteoInitFileName = string.Empty;
        /// <summary>
        /// Counter for automatic file names
        /// </summary>
        private int NameCounter = 1;

        public SelectMultiplePoints()
        {
            InitializeComponent();
        }

        private void SelectMultiplePoints_Load(object sender, EventArgs e)
        {

            if (MeteoModel == GralDomain.MeteoModelEmum.None) // No selection of GRAMM or GRAL Model
            {
                groupBox1.Visible = false;
            }
            else
            {
                groupBox1.Visible = true;
                radioButton1.Visible = false;
                radioButton2.Visible = false;

                if (((int) MeteoModel & 1) == (int) GralDomain.MeteoModelEmum.GRAMM) // GRAMM
                {
                    radioButton1.Visible = true;
                    radioButton1.Checked = true;
                }
                if (((int) MeteoModel & 2) == (int) GralDomain.MeteoModelEmum.GRAL) // GRAL
                {
                    radioButton2.Visible = true;
                    if (radioButton1.Visible == false)
                    {
                        radioButton2.Checked = true;
                        label1.Text = "Height above lowest model elevation";
                    }
                }
            }

            if (LocalStability) // true
            {
                checkBox1.Enabled = true;
                checkBox1.Checked = true;
            }
            else
            {
                checkBox1.Checked = false;
                checkBox1.Enabled = false;
            }

            PointCoorData.Columns.Add("Name", typeof(string));
            PointCoorData.Columns.Add("X coor", typeof(double));
            PointCoorData.Columns.Add("Y coor", typeof(double));
            PointCoorData.Columns.Add("Z coor", typeof(double));

            if (string.IsNullOrEmpty(MeteoInitFileName))
            {
                button8.Visible = false;
            }
            else
            {
                label2.Text = "Result file name";
                button8.Visible = true;
                textBox1.Text = GralStaticFunctions.St_F.ReduceFileNameLenght(MeteoInitFileName, 60);
                textBox1.ReadOnly = true;
            }

            dataGridView1.DataSource = PointCoorData;
        }

        /// <summary>
        /// Add all receptors to the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            List<GralItemData.ReceptorData> ItemData = new List<GralItemData.ReceptorData>();
            GralItemData.ReceptorDataIO _rd = new GralItemData.ReceptorDataIO();
            _rd.LoadReceptors(ItemData, Path.Combine(Gral.Main.ProjectName, "Computation", "Receptor.dat"));
            _rd = null;

            foreach(GralItemData.ReceptorData _data in ItemData)
            {
                DataRow workrow;
                workrow = PointCoorData.NewRow();
                workrow[0] = _data.Name;
                workrow[1] = _data.Pt.X;
                workrow[2] = _data.Pt.Y;
                workrow[3] = _data.Height;
                PointCoorData.Rows.Add(workrow);
            }
        }

        public void ReceiveClickedCoordinates(Object obj, EventArgs e)
        {
            string _a = "Pt " + Convert.ToString(NameCounter++); 
            if (obj is GralDomain.PointD _pt)
            {
                DataRow workrow;
                workrow = PointCoorData.NewRow();
                workrow[0] = _a;
                workrow[1] = _pt.X;
                workrow[2] = _pt.Y;
                workrow[3] = 10;
                PointCoorData.Rows.Add(workrow);
                this.Activate();
            }
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            try
            {
                DataGridViewCell ocell = dataGridView1[e.ColumnIndex, e.RowIndex];
                MessageBox.Show(this, "Only numeric inputs allowed", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ocell.Value = Convert.ChangeType(PointCoorData.Rows[e.RowIndex][e.ColumnIndex], ocell.ValueType);
            }
            catch { }
            e.Cancel = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // send Message to domain Form, that computation is cancelled
            try
            {
                if (CancelComputation != null)
                {
                    CancelComputation(this, e);
                }
            }
            catch
            { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                MeteoModel = GralDomain.MeteoModelEmum.GRAMM;
            }
            else if (radioButton2.Checked)
            {
                MeteoModel = GralDomain.MeteoModelEmum.GRAL;
            }
            
            if (checkBox1.Checked)
            {
                LocalStability = true;
            }
            else
            {
                LocalStability = false;
            }
            //take string from textbox, if user sets a prefix or postfix, but use string from SaveFileDialog otherwise
            if (string.IsNullOrEmpty(MeteoInitFileName))
            {
                MeteoInitFileName = textBox1.Text;
            }
            // send Message to domain Form, that computation should be started
            try
            {
                if (StartComputation != null)
                {
                    StartComputation(this, e);
                }
            }
            catch
            { }
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
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
                            PasteClipboard();
                            break;
                    }
                }
            }
            catch
            {

            }
        }
        void PasteClipboard()
        {
            try
            {
                string s = Clipboard.GetText();
                string[] lines = s.Split('\n');

                int iRow = dataGridView1.CurrentCell.RowIndex;
                int iCol = dataGridView1.CurrentCell.ColumnIndex;
                DataGridViewCell oCell;

                foreach (string line in lines)
                {
                    if (iRow < dataGridView1.RowCount && line.Length > 0)
                    {
                        string[] sCells = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < sCells.GetLength(0); ++i)
                        {
                            double test = 0;

                            if (iCol + i < dataGridView1.ColumnCount)
                            {
                                oCell = dataGridView1[iCol + i, iRow];
                                //check numeric cells
                                if (oCell.ValueType != typeof(string) && double.TryParse(sCells[i], out test) == false)
                                {
                                    sCells[i] = test.ToString("0.0");
                                }
                               
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
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(MeteoInitFileName),
                FileName = Path.GetFileName(MeteoInitFileName),
                Title = "Set the result file name",
                AddExtension = true,
                OverwritePrompt = true,
                CheckPathExists = true
            };
            saveFile.ShowDialog();
            MeteoInitFileName = saveFile.FileName;
            textBox1.Text = GralStaticFunctions.St_F.ReduceFileNameLenght(MeteoInitFileName, 60);
            saveFile.Dispose();
        }
    }
}
