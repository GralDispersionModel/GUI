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
			#endif

			for (int i = 1; i < 11; i++)
			{
				dataGridView1.Columns["ER Nr. " + Convert.ToString(i)].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
				#if __MonoCS__
				#else
				dataGridView1.Columns["ER Nr. " + Convert.ToString(i)].Name = DataGridViewContentAlignment.MiddleRight.ToString();
				#endif
			}
			dataGridView1.Columns["Source_group"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			#if __MonoCS__
			#else
			dataGridView1.Columns["Source_group"].Name = DataGridViewContentAlignment.MiddleCenter.ToString();
			#endif
			for (int i = 0; i < _data.Columns.Count; i++)
				dataGridView1.Columns[i].Visible = _visible[i];
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
			dataGridView1.Height = Math.Max(300, Height - 177);
			dataGridView1.Width = Math.Max(400, Width - 50);
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

	}
}
