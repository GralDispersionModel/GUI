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
 * Date: 27.12.2016
 * Time: 18:50
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Text;
using GralStaticFunctions;

namespace GralDomForms
{
	/// <summary>
	/// Search an item of all items within the model
	/// </summary>
	public partial class Search_Item : Form
	{
		private DataTable _data;
		public DataTable Data {set {_data = value;} }
		
		private int _selected_item = 0;
		public int Selected_item {get {return _selected_item;} }
		
		private string _selected_type = "";
		public string Selected_Type{get {return _selected_type;} }
		
		private DataView datasorted = new DataView();
		
		private bool[] _visible = new bool[100];
		public bool[] Items_visible{set {_visible = value;} get {return _visible;} }
        public Size FormSize;

        public GralItemForms.EditPointSources EditPSSearch;
        public GralItemForms.EditAreaSources EditASSearch;
        public GralItemForms.EditLinesources EditLSSearch;
        public GralItemForms.EditBuildings EditBSearch;
        public GralItemForms.EditPortalSources EditPortalsSearch;
        public GralItemForms.EditReceptors EditRSearch;
        public GralItemForms.EditWalls EditWallSearch;
        public GralItemForms.EditVegetation EditVegetationSearch;

        public bool Locked = false;

        private DataGridViewCellStyle ChangedCellStyle;

        public Search_Item()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void Search_ItemLoad(object sender, EventArgs e)
		{		
			datasorted = new DataView(_data); // create DataView from DataTable
			dataGridView1.DataSource = datasorted; // connect DataView to GridView
			dataGridView1.ColumnHeadersHeight = 26;
			dataGridView1.Columns["Height"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

			dataGridView1.Columns["Vert. ext."].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

			dataGridView1.Columns["Exit_velocity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

			dataGridView1.Columns["Temperature"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

			dataGridView1.Columns["Diameter"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

			#if __MonoCS__
			#else
			dataGridView1.Columns["Vert. ext."].Name = DataGridViewContentAlignment.MiddleRight.ToString();
			dataGridView1.Columns["Diameter"].Name = DataGridViewContentAlignment.MiddleRight.ToString();
			dataGridView1.Columns["Exit_velocity"].Name = DataGridViewContentAlignment.MiddleRight.ToString();
			dataGridView1.Columns["Temperature"].Name = DataGridViewContentAlignment.MiddleRight.ToString();
            dataGridView1.Columns["ER_SG"].Name = DataGridViewContentAlignment.MiddleRight.ToString();
#endif
            if (Locked == false)
            {
                dataGridView1.Columns[0].ReadOnly = true;
                dataGridView1.Columns[2].ReadOnly = true;
                dataGridView1.Columns[9].ReadOnly = true;
                button3.Enabled = true;
                button4.Enabled = true;
            }
            else
            {
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].ReadOnly = true;
                }
                button3.Enabled = false;
                button4.Enabled = false;
            }
            for (int i = 1; i < 11; i++)
			{
				dataGridView1.Columns["ER Nr. " + Convert.ToString(i)].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
				#if __MonoCS__
				#else
				dataGridView1.Columns["ER Nr. " + Convert.ToString(i)].Name = DataGridViewContentAlignment.MiddleRight.ToString();
#endif
                dataGridView1.Columns["Poll. " + Convert.ToString(i)].ReadOnly = true;
            }
			dataGridView1.Columns["Source_group"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			#if __MonoCS__
			#else
			dataGridView1.Columns["Source_group"].Name = DataGridViewContentAlignment.MiddleCenter.ToString();
#endif
            for (int i = 0; i < _data.Columns.Count; i++)
            {
                dataGridView1.Columns[i].Visible = _visible[i];
            }
            ChangedCellStyle = new DataGridViewCellStyle();
            ChangedCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
            ChangedCellStyle.BackColor = Color.Beige;
            dataGridView1.CellValueChanged += new DataGridViewCellEventHandler(dataGridView1_CellValueChanged);
            Search_ItemResizeEnd(null, null);
        }

        void Button1Click(object sender, EventArgs e) //OK
        {
            int selectedRowCount =
                dataGridView1.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (selectedRowCount > 0) // a line selected?
            {
                int index = dataGridView1.SelectedRows[0].Index;
                _selected_item = Convert.ToInt32(dataGridView1[0, index].Value);
                _selected_type = Convert.ToString(dataGridView1[2, index].Value);
            }
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Locked == false)
            {
                for (int i = dataGridView1.RowCount - 1; i >= 0; i--)
                {
                    int index = dataGridView1.Rows[i].Index;
                    int selected_item = Convert.ToInt32(dataGridView1[0, index].Value) - 1;
                    string selected_type = Convert.ToString(dataGridView1[2, index].Value);

                    if (selected_type == "Point source" && EditPSSearch != null)
                    {
                        int k = 0;
                        foreach (GralItemData.PointSourceData _dta in EditPSSearch.ItemData)
                        {
                            try
                            {
                                if (k == selected_item)
                                {
                                    _dta.Name = Convert.ToString(dataGridView1[1, index].Value);
                                    _dta.Height = Convert.ToSingle(dataGridView1[3, index].Value);
                                    _dta.Poll.SourceGroup = Convert.ToInt32(dataGridView1[5, index].Value);
                                    if (string.IsNullOrEmpty(_dta.VelocityTimeSeries))
                                    {
                                        _dta.Velocity = Convert.ToSingle(dataGridView1[6, index].Value);
                                    }
                                    if (string.IsNullOrEmpty(_dta.TemperatureTimeSeries))
                                    {
                                        _dta.Temperature = Convert.ToSingle(dataGridView1[7, index].Value) + 273F;
                                    }
                                    _dta.Diameter = Convert.ToSingle(dataGridView1[8, index].Value);
                                    for (int j = 0; j < 10; j++)
                                    {
                                        int _col = 10 + 2 * j;
                                        // MessageBox.Show(dataGridView1.Columns.Count.ToString());
                                        //MessageBox.Show(Convert.ToDouble(dataGridView1[_col, index].Value).ToString());
                                        if (dataGridView1.Rows[i].Cells[_col].Value != DBNull.Value)
                                        {
                                            _dta.Poll.EmissionRate[j] = Convert.ToDouble(dataGridView1[_col, index].Value);
                                        }
                                    }
                                }
                            }
                            catch { }
                            k++;
                        }
                    }

                    if (selected_type == "Line source" && EditLSSearch != null)
                    {
                        int k = 0;
                        foreach (GralItemData.LineSourceData _dta in EditLSSearch.ItemData)
                        {
                            try
                            {
                                if (k == selected_item)
                                {
                                    _dta.Name = Convert.ToString(dataGridView1[1, index].Value);
                                    _dta.Height = Convert.ToSingle(dataGridView1[3, index].Value);
                                    _dta.VerticalExt = Convert.ToSingle(dataGridView1[4, index].Value);
                                    int SGIndex = Convert.ToInt32(dataGridView1[9, index].Value);
                                    _dta.Poll[SGIndex].SourceGroup = Convert.ToInt32(dataGridView1[5, index].Value);
                                    for (int j = 0; j < 10; j++)
                                    {
                                        int _col = 10 + 2 * j;
                                        // MessageBox.Show(dataGridView1.Columns.Count.ToString());
                                        //MessageBox.Show(Convert.ToDouble(dataGridView1[_col, index].Value).ToString());
                                        if (dataGridView1.Rows[i].Cells[_col].Value != DBNull.Value)
                                        {
                                            _dta.Poll[SGIndex].EmissionRate[j] = Convert.ToDouble(dataGridView1[_col, index].Value);
                                        }
                                    }
                                }
                            }
                            catch { }
                            k++;
                        }
                    }

                    if (selected_type == "Portal source" && EditPortalsSearch != null)
                    {
                        int k = 0;
                        foreach (GralItemData.PortalsData _dta in EditPortalsSearch.ItemData)
                        {
                            try
                            {
                                if (k == selected_item)
                                {
                                    _dta.Name = Convert.ToString(dataGridView1[1, index].Value);
                                    _dta.Height = Convert.ToSingle(dataGridView1[4, index].Value);
                                    
                                    if (string.IsNullOrEmpty(_dta.VelocityTimeSeries))
                                    {
                                        _dta.ExitVel = Convert.ToSingle(dataGridView1[6, index].Value);
                                    }
                                    if (string.IsNullOrEmpty(_dta.TemperatureTimeSeries))
                                    {
                                        _dta.DeltaT = Convert.ToSingle(dataGridView1[7, index].Value);
                                    }
                                    int SGIndex = Convert.ToInt32(dataGridView1[9, index].Value);
                                    _dta.Poll[SGIndex].SourceGroup = Convert.ToInt32(dataGridView1[5, index].Value);
                                    for (int j = 0; j < 10; j++)
                                    {
                                        int _col = 10 + 2 * j;
                                        // MessageBox.Show(dataGridView1.Columns.Count.ToString());
                                        //MessageBox.Show(Convert.ToDouble(dataGridView1[_col, index].Value).ToString());
                                        if (dataGridView1.Rows[i].Cells[_col].Value != DBNull.Value)
                                        {
                                            _dta.Poll[SGIndex].EmissionRate[j] = Convert.ToDouble(dataGridView1[_col, index].Value);
                                        }
                                    }
                                }
                            }
                            catch { }
                            k++;
                        }
                    }

                    if (selected_type == "Area source" && EditASSearch != null)
                    {
                        int k = 0;
                        foreach (GralItemData.AreaSourceData _dta in EditASSearch.ItemData)
                        {
                            try
                            {
                                if (k == selected_item)
                                {
                                    _dta.Name = Convert.ToString(dataGridView1[1, index].Value);
                                    _dta.Height = Convert.ToSingle(dataGridView1[3, index].Value);
                                    _dta.Poll.SourceGroup = Convert.ToInt32(dataGridView1[5, index].Value);
                                    _dta.VerticalExt = Convert.ToSingle(dataGridView1[4, index].Value);
                                    for (int j = 0; j < 10; j++)
                                    {
                                        int _col = 10 + 2 * j;
                                        // MessageBox.Show(dataGridView1.Columns.Count.ToString());
                                        //MessageBox.Show(Convert.ToDouble(dataGridView1[_col, index].Value).ToString());
                                        if (dataGridView1.Rows[i].Cells[_col].Value != DBNull.Value)
                                        {
                                            _dta.Poll.EmissionRate[j] = Convert.ToDouble(dataGridView1[_col, index].Value);
                                        }
                                    }
                                }
                            }
                            catch { }
                            k++;
                        }
                    }

                    if (selected_type == "Receptor point" && EditRSearch != null)
                    {
                        int k = 0;
                        foreach (GralItemData.ReceptorData _dta in EditRSearch.ItemData)
                        {
                            try
                            {
                                if (k == selected_item)
                                {
                                    _dta.Name = Convert.ToString(dataGridView1[1, index].Value);
                                    _dta.Height = Convert.ToSingle(dataGridView1[3, index].Value);
                                }
                            }
                            catch { }
                            k++;
                        }
                    }
                    if (selected_type == "Building" && EditBSearch != null)
                    {
                        int k = 0;
                        foreach (GralItemData.BuildingData _dta in EditBSearch.ItemData)
                        {
                            try
                            {
                                if (k == selected_item)
                                {
                                    _dta.Name = Convert.ToString(dataGridView1[1, index].Value);
                                    _dta.Height = Convert.ToSingle(dataGridView1[3, index].Value);
                                }
                            }
                            catch { }
                            k++;
                        }
                    }
                    if (selected_type == "Wall" && EditWallSearch != null)
                    {
                        int k = 0;
                        foreach (GralItemData.WallData _dta in EditWallSearch.ItemData)
                        {
                            try
                            {
                                if (k == selected_item)
                                {
                                    _dta.Name = Convert.ToString(dataGridView1[1, index].Value);
                                }
                            }
                            catch { }
                            k++;
                        }
                    }
                    if (selected_type == "Vegetation" && EditVegetationSearch != null)
                    {
                        int k = 0;
                        foreach (GralItemData.VegetationData _dta in EditVegetationSearch.ItemData)
                        {
                            try
                            {
                                if (k == selected_item)
                                {
                                    _dta.Name = Convert.ToString(dataGridView1[1, index].Value);
                                    _dta.VerticalExt = Convert.ToSingle(dataGridView1[3, index].Value);
                                }
                            }
                            catch { }
                            k++;
                        }
                    }
                }
            }

            _selected_item = -1; // marker that values should be saved
            Close();
        }

        void Button2Click(object sender, EventArgs e) // Cancel
        {
            _selected_item = 0;
            Close();
        }

        void TextBox1TextChanged(object sender, EventArgs e)
		{
			try
			{
				string filter1 = String.Format("Name LIKE '{0}*'", convertvalue(textBox1.Text));
				string filter2 = String.Format("Type LIKE '{0}*'", convertvalue(textBox2.Text));
				string filter4 = String.Format("Source_group LIKE '{0}*'", convertvalue(textBox4.Text));
				string number = textBox3.Text;
				string compare = ""; 
				
				if (number.StartsWith(">="))
				{
					compare = ">=";
				    number = number.Substring(2);
				}
				else if (number.StartsWith("<="))
				{
					compare = "<=";
					number = number.Substring(2);
				}
				else if (number.StartsWith("="))
				{
					compare = "=";
					number = number.Substring(1);
				}
				else if (number.StartsWith(">"))
				{
					compare = ">";
					number = number.Substring(1);
				}
				else if (number.StartsWith("<"))
				{
					compare = "<";
					number = number.Substring(1);
				}
				
				if (compare.Length > 0)
				{
					string filter3 = "Height " + compare + " '" + Convert.ToString(St_F.TxtToDbl(number, false)) + "'";
					if (textBox4.Text.Length > 0)
						datasorted.RowFilter = filter1 + " AND " + filter2 + " AND " + filter3 + " AND " + filter4;
					else
						datasorted.RowFilter = filter1 + " AND " + filter2 + " AND " + filter3;
				}
				else
				{
					if (textBox4.Text.Length > 0)
						datasorted.RowFilter = filter1 + " AND " + filter2 + " AND " + filter4;
					else
						datasorted.RowFilter = filter1 + " AND " + filter2;
				}
				textBox3.BackColor = textBox2.BackColor;
			}
			catch
			{
				textBox3.BackColor = Color.Yellow;
			}
		}
		
		string convertvalue(string st)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < st.Length; i++)
			{
				char c = st[i];
				if (c == '*' || c == '%' || c == '[' || c == ']')
					sb.Append("[").Append(c).Append("]");
				else if (c == '\'')
					sb.Append("''");
				else
					sb.Append(c);
			}
			return sb.ToString();
		}

		void Search_ItemResizeEnd(object sender, EventArgs e)
		{
            int h = Height - 60 - (Height - button1.Top);
            if (h > 0)
            {
                dataGridView1.Height = h;
            }
            dataGridView1.Width = Math.Max(400, Width - SystemInformation.VerticalScrollBarWidth);
            if (sender != null && WindowState != FormWindowState.Maximized && WindowState != FormWindowState.Minimized)
            {
                FormSize = this.Size;
            }
		}
		
		void DataGridView1RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			Button1Click(null, null);
		}
		
		void DataGridView1ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right) // right mouse button 
			{
				ContextMenu m = new ContextMenu();
				for (int i = 0; i < _data.Columns.Count; i++)
				{
					m.MenuItems.Add(new MenuItem(_data.Columns[i].ToString()));
					m.MenuItems[i].Checked = _visible[i];
					// Add functionality to the menu items using the Click event.
					m.MenuItems[i].Click += new System.EventHandler(menuItem_Click);
				}
				
				m.Show(dataGridView1, new Point(e.X, e.Y));
				
			}
		}
		
		private void menuItem_Click(object sender, System.EventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			_visible[mi.Index] = !_visible[mi.Index];
			mi.Checked = _visible[mi.Index];
			dataGridView1.Columns[mi.Index].Visible = _visible[mi.Index];
			//MessageBox.Show(sender.ToString() + mi.Index.ToString());
		}

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && dataGridView1.SelectedRows.Count > 0 && Locked == false)
            {
                DeleteItems();
            }
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

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0 && Locked == false)
            {
                DeleteItems();
            }
        }

        private void DeleteItems()
        {
            string _it = "items?";
            if (dataGridView1.SelectedRows.Count == 1)
            {
                _it = "item?";
            }
            if (MessageBox.Show(this, "Delete " + dataGridView1.SelectedRows.Count.ToString() +
                    " selected " + _it, "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                for (int i = dataGridView1.RowCount - 1; i >= 0; i--)
                {
                    if (dataGridView1.SelectedRows.Contains(dataGridView1.Rows[i]))
                    {
                        int index = dataGridView1.Rows[i].Index;
                        int selected_item = Convert.ToInt32(dataGridView1[0, index].Value) - 1;
                        string selected_type = Convert.ToString(dataGridView1[2, index].Value);

                        if (selected_type == "Point source" && EditPSSearch != null)
                        {
                            EditPSSearch.ItemDisplayNr = selected_item;
                            EditPSSearch.RemoveOne(false);
                        }
                        if (selected_type == "Portal source" && EditPortalsSearch != null)
                        {
                            EditPortalsSearch.ItemDisplayNr = selected_item;
                            EditPortalsSearch.RemoveOne(false);
                        }
                        if (selected_type == "Line source" && EditLSSearch != null)
                        {
                            EditLSSearch.ItemDisplayNr = selected_item;
                            EditLSSearch.RemoveOne(false);
                        }
                        if (selected_type == "Area source" && EditASSearch != null)
                        {
                            EditASSearch.ItemDisplayNr = selected_item;
                            EditASSearch.RemoveOne(false);
                        }
                        if (selected_type == "Building" && EditBSearch != null)
                        {
                            EditBSearch.ItemDisplayNr = selected_item;
                            EditBSearch.RemoveOne(false, false);
                        }
                        if (selected_type == "Receptor point" && EditRSearch != null)
                        {
                            EditRSearch.ItemDisplayNr = selected_item;
                            EditRSearch.RemoveOne(false);
                        }
                        if (selected_type == "Wall" && EditWallSearch != null)
                        {
                            EditWallSearch.ItemDisplayNr = selected_item;
                            EditWallSearch.RemoveOne(false);
                        }
                        if (selected_type == "Vegetation" && EditVegetationSearch != null)
                        {
                            EditVegetationSearch.ItemDisplayNr = selected_item;
                            EditVegetationSearch.RemoveOne(false);
                        }

                        _selected_item = -1; // marker, that changed items need to be written to disk
                        Close();
                    }
                }
            }
        }

        private void Search_Item_FormClosed(object sender, FormClosedEventArgs e)
        {
            _data = null;
            Data = null;
            ChangedCellStyle.Font.Dispose();
            ChangedCellStyle = null;
            dataGridView1.CellValueChanged -= new DataGridViewCellEventHandler(dataGridView1_CellValueChanged);         
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (sender == dataGridView1)
            {
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style = ChangedCellStyle;
            }
        }

        private void Search_Item_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                Search_ItemResizeEnd(null, null);
            }
            if (WindowState == FormWindowState.Normal)
            {
                Search_ItemResizeEnd(null, null);
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
                        string[] sCells = line.Split('\t');
                        for (int i = 0; i < sCells.GetLength(0); ++i)
                        {
                            double test = 0;
                            if (double.TryParse(sCells[i], out test) == false)
                            {
                                sCells[i] = test.ToString("0.0");
                            }
                            if (iCol + i < dataGridView1.ColumnCount)
                            {
                                oCell = dataGridView1[iCol + i, iRow];
                                oCell.Value = Convert.ChangeType(sCells[i].Replace("\r", ""), oCell.ValueType);
                                oCell.Style = ChangedCellStyle;
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

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            DataGridViewCell ocell = dataGridView1[e.ColumnIndex, e.RowIndex];
            MessageBox.Show(this, "Only numeric inputs allowed", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ocell.Value = Convert.ChangeType(_data.Rows[e.RowIndex][e.ColumnIndex], ocell.ValueType);
            e.Cancel = false;
        }
    }
}
