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

using GralStaticFunctions;
using System;
using System.Windows.Forms;

namespace GralItemForms
{
    /// <summary>
    /// DataGrid to edit edge points
    /// </summary>
    public partial class VerticesEditDialog : Form
    {
        public int Lines = 0;
        private double[] x1 = new double[20];            //x corner points 
        private double[] y1 = new double[20];            //y corner points 		
        private double[] z1 = new double[20];              //z

        // delegate to send redraw Message
        public event ForceDomainRedraw Vertices_redraw;

        /// <summary>
        /// Start DataGrid to edit edge points
        /// </summary>
        /// <param name="lines"> Number of edge points></param>
        /// <param name="x"> Double array of x coordinates></param>
        /// <param name="y"> Double array of y coordinates></param>
        public VerticesEditDialog(int lines, ref double[] x, ref double[] y)
        {
            InitializeComponent();
            Lines = lines;
            x1 = x;
            y1 = y;
            z1 = null;
        }

        /// <summary>
        /// Start DataGrid to edit edge points
        /// </summary>
        /// <param name="lines"> Number of edge points></param>
        /// <param name="x"> Double array of x coordinates></param>
        /// <param name="y"> Double array of y coordinates></param>
        /// <param name="z"> Float array of z values></param>
        public VerticesEditDialog(int lines, ref double[] x, ref double[] y, ref double[] z)
        {
            InitializeComponent();
            Lines = lines;
            x1 = x;
            y1 = y;
            z1 = z;
        }

        void DataGrid1Navigate(object sender, NavigateEventArgs ne)
        {

        }

        void Vertices_editLoad(object sender, EventArgs e)
        {
            datagrid.Rows.Clear();
            datagrid.Columns.Clear();
            datagrid.AllowUserToAddRows = true;
            datagrid.AllowUserToDeleteRows = true;
            datagrid.AllowUserToOrderColumns = false;

            if (z1 != null)
            {
                datagrid.ColumnCount = 3;
            }
            else
            {
                datagrid.ColumnCount = 2;
            }
            //datagrid.Columns[0].Name = "ID";
            datagrid.Columns[0].Name = "x";
            datagrid.Columns[1].Name = "y";
            datagrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            datagrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            datagrid.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            datagrid.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;

            datagrid.Columns[0].Width = 94;
            datagrid.Columns[1].Width = 94;

            if (z1 != null)
            {
                datagrid.Columns[2].Name = "z";
                datagrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                datagrid.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                datagrid.Columns[2].Width = 94;
            }
            //datagrid.Columns.Add("x","x");
            //datagrid.Columns.Add("y","y");

            string[] row;
            for (int i = 0; i < Lines; i++)
            {
                //row = new string[] { Convert.ToString(i), Static_Functions.Dbl_to_txt(x1[i]), Static_Functions.Dbl_to_txt(y1[i])};
                if (z1 != null)
                {
                    row = new string[] {St_F.DblToIvarTxt(Math.Round(x1[i], 1)),
                        St_F.DblToIvarTxt(Math.Round(y1[i], 1)), St_F.DblToIvarTxt(Math.Round(z1[i], 1))};
                }
                else
                {
                    row = new string[] { St_F.DblToIvarTxt(Math.Round(x1[i], 1)), St_F.DblToIvarTxt(Math.Round(y1[i], 1)) };
                }
                datagrid.Rows.Add(row);
            }


        }

        void DatagridCellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        void Button2Click(object sender, EventArgs e)
        {
            Close();
        }

        void Button1Click(object sender, EventArgs e)
        {
            string row;
            Lines = 0;
            for (int i = 0; i < datagrid.Rows.Count; i++)
            {
                //if (dataGridViewx.Rows[i].Cells["Status"].VALUE.ToString() != "1")
                row = (string)datagrid.Rows[i].Cells["x"].Value;
                if (row != null)
                {
                    x1[i] = St_F.TxtToDbl(row, false);
                }
                row = (string)datagrid.Rows[i].Cells["y"].Value;
                if (row != null)
                {
                    y1[i] = St_F.TxtToDbl(row, false);
                }
                if (z1 != null)
                {
                    row = (string)datagrid.Rows[i].Cells["z"].Value;
                    if (row != null)
                    {
                        z1[i] = (float)(St_F.TxtToDbl(row, false));
                    }
                }
                if (row != null)
                {
                    Lines++;
                }
            }

            Close();
        }

        private void RedrawDomain(object sender, EventArgs e)
        {
            // send redraw message 
            try
            {
                if (Vertices_redraw != null)
                {
                    Vertices_redraw(this, e);
                }
            }
            catch
            { }
        }

        void DatagridRowEnter(object sender, DataGridViewCellEventArgs e)
        {

        }


        void DatagridCellEnter(object sender, DataGridViewCellEventArgs e)
        {
            string row = (string)datagrid.Rows[e.RowIndex].Cells["x"].Value;
            if (row != null)
            {
                GralDomain.Domain.MarkerPoint.X = St_F.TxtToDbl(row, true);
            }
            row = (string)datagrid.Rows[e.RowIndex].Cells["y"].Value;
            if (row != null)
            {
                GralDomain.Domain.MarkerPoint.Y = St_F.TxtToDbl(row, true);
            }
            RedrawDomain(this, null);
        }

        void Vertices_editFormClosed(object sender, FormClosedEventArgs e)
        {
            GralDomain.Domain.MarkerPoint.X = 0;
            GralDomain.Domain.MarkerPoint.Y = 0;
            x1 = null;
            y1 = null;
        }

    }
}
