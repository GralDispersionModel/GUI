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
 * User: Markus Kuntner
 * Date: 11.11.2018
 * Time: 13:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

namespace GralItemForms
{
    /// <summary>
    /// Description of EditTimeSeriesValues.
    /// </summary>
    public partial class EditTimeSeriesValues : Form
	{
		public string FilePath = string.Empty;
		public string SelectedTimeSeries = string.Empty;
        public double InitValue = 0;
        public double MeanValue = 0;
		public bool NegativeNumbersAllowed = false;
        public string TitleBarText = string.Empty;
		
        private List <string> EntryNames = new List<string>();
		private float[,] GridValues = new float[12, 4];
		private CultureInfo ic = CultureInfo.InvariantCulture;
		
		public EditTimeSeriesValues()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			dataGridView1.CellEndEdit += new DataGridViewCellEventHandler(dataGridView1_CellEndEdit);
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void EditTimeSeriesValuesLoad(object sender, EventArgs e)
		{
			if (String.IsNullOrEmpty(FilePath))
			{
				return;
			}
			
			for (int row = 0; row < 4; row++) // time slices
			{
				dataGridView1.Rows.Add();
				dataGridView1.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
				dataGridView1.Rows[row].Cells[0].Value = (row * 6).ToString() + " - " + ((row + 1) * 6).ToString();
				for (int cells = 0; cells < 12; cells++)
				{
                    GridValues[cells, row] = (float) InitValue;
                    dataGridView1.Columns[cells + 1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
					dataGridView1.Rows[row].Cells[cells + 1].Value = Math.Round(GridValues[cells, row], 1).ToString("0.0");
				}
			}


            EntryNames = ReadEntries(FilePath);
			
			comboBox1.Items.Clear();
			foreach(string en in EntryNames)
			{
				comboBox1.Items.Add(en);
			}
			
			int i = 0;
			foreach(string en in EntryNames)
			{
				if (en.Equals(SelectedTimeSeries))
				{
					comboBox1.SelectedIndex = i;	
				}
				i++;
			}
            this.Text = TitleBarText;
		}
		
		private List<string> ReadEntries(String FilePath)
		{
			List <string> entries = new List<string>();
			// read all available entries 
			if (File.Exists(FilePath))
			{
				try
				{
					using (StreamReader reader = new StreamReader(FilePath))
					{
						while(reader.EndOfStream == false)
						{
							string[] _text =  reader.ReadLine().Split(new char[] { '\t'});
							if (_text.Length > 2)
							{
								entries.Add(_text[0]);
							}
						}
					}
				}
				catch
				{
					MessageBox.Show(this, "Unable to read file \n" + FilePath,"GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return entries;
				}
			}
			
			return entries;
		}
		
		public float[,] ReadValues(String FilePath, string Entry, bool message)
		{
			float[,] GridValues = new float[12, 4];

            // read all available entries 
            if (File.Exists(FilePath))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(FilePath))
                    {
                        while (reader.EndOfStream == false)
						{
							string[] _text =  reader.ReadLine().Split(new char[] { '\t'});
							if (_text.Length > 2)
							{
								if (_text[0].Equals(Entry))
								{
									for (int row = 0; row < 4; row++) // time slices
									{
										for (int cells = 0; cells < 12; cells++)
										{
											if (row * 12 + cells < _text.Length)
											{
												GridValues[cells, row] = Convert.ToSingle(_text[1 + row * 12 + cells], ic);
												MeanValue += GridValues[cells, row];
											}
										}
									}
								}
							}
						}
					}
					MeanValue /= (4 * 12);
					return GridValues;
				}
				catch
				{
					if (message)
					{
						MessageBox.Show(this, "Unable to read file \n" + FilePath,"GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					return null;
				}
			}
			else
			{
				return null;
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
					if (val < 0 && NegativeNumbersAllowed == false)
					{
						MessageBox.Show(this, "The number needs to be > 0", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
						err = true;
					}
					
					if (err)
					{
						dataGridView1.Rows[row].Cells[col].Value = "0";
						//dataGridView1.Rows[row].Cells[col].Selected = true;
						//dataGridView1.CurrentCell = dataGridView1.Rows[row].Cells[col];
					}

				}
				else
				{
					dataGridView1.Rows[row].Cells[col].Value = "0";
					MessageBox.Show(this, "Only numerical inputs are allowed", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}
		
		//Save & Exit
		void Button1Click(object sender, EventArgs e)
		{
		    if (SelectedTimeSeries != null && SelectedTimeSeries.Length > 0)
		    {
		        for (int row = 0; row < 4; row++) // time slices
		        {
		            for (int cells = 0; cells < 12; cells++)
		            {
		                
		                GridValues[cells, row] = Convert.ToSingle((
		                    dataGridView1.Rows[row].Cells[cells + 1].Value));
		            }
		        }
		        
		        ChangeEntry();// save changed entries
		        DialogResult = DialogResult.OK;
		    }
		    else
		    {
		        DialogResult = DialogResult.Cancel;
		    }
			Close();
		}
		
		void Button2Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
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
		
		// append new entry
		void Button3Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(textBox1.Text))
			{
				MessageBox.Show(this, "Please enter a name","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			string newName = textBox1.Text;
			
			while(EntryNames.Contains(newName))
			{
				newName += "_";	
			}
			
			try
			{
				using (StreamWriter writer = new StreamWriter(FilePath, true)) // Append
				{
					string _text = String.Empty;
					for (int row = 0; row < 4; row++) // time slices
					{
						for (int cells = 0; cells < 12; cells++)
						{
							GridValues[cells, row] = Convert.ToSingle(
								dataGridView1.Rows[row].Cells[cells + 1].Value);
							_text += Math.Round(GridValues[cells, row], 1).ToString(ic) +"\t";
						}
					}
					
					writer.WriteLine(newName + "\t" + _text);
				}
			}
			catch
			{
				MessageBox.Show(this, "Error when exporting time series values","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			EditTimeSeriesValuesLoad(null, null);
			if (comboBox1.Items.Count > 0)
			{
				comboBox1.SelectedIndex = comboBox1.Items.Count -1;
			}
		}
		
		private void ChangeEntry()
		{
			List <string> AllEntries = new List<string>();
				
			if (File.Exists(FilePath))
			{
				try
				{
					using (StreamReader reader = new StreamReader(FilePath))
					{
						while(reader.EndOfStream == false)
						{
							AllEntries.Add(reader.ReadLine());
						}
					}
				}
				catch
				{			
				}
				
				try
				{
					using (StreamWriter writer = new StreamWriter(FilePath)) // New File
					{
						
						foreach(string entry in AllEntries)
						{
							string newLine = string.Empty;
							string[] _text =  entry.Split(new char[] { '\t'});
							
							if (_text.Length > 1 && _text[0].Equals(SelectedTimeSeries)) // overwrite selected entry
							{
								MeanValue = 0;
								for (int row = 0; row < 4; row++) // time slices
								{
									for (int cells = 0; cells < 12; cells++)
									{
										newLine += Math.Round(GridValues[cells, row], 1).ToString(ic) +"\t";
										MeanValue += GridValues[cells, row];
									}
								}
								MeanValue /= (4 * 12);
								writer.WriteLine(SelectedTimeSeries + "\t" + newLine);
							}
							else
							{
								writer.WriteLine(entry);
							}
						}
					}
				}
				catch{}				
			}		
		}
		
		void ComboBox1SelectedIndexChanged(object sender, EventArgs e)
		{
			string selectedName = comboBox1.SelectedItem.ToString();
			if (string.IsNullOrEmpty(selectedName)) // if no data has been defined
			{
				SelectedTimeSeries = string.Empty;
				return;
			}
			
			// read values from File
			GridValues = ReadValues(FilePath, selectedName, true);
			if (GridValues != null)
			{
				SelectedTimeSeries = selectedName;
				
				for (int row = 0; row < 4; row++) // time slices
				{
					dataGridView1.Rows.Add();
					dataGridView1.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
					dataGridView1.Rows[row].Cells[0].Value = (row * 6).ToString() + " - " + ((row + 1) * 6).ToString();
					for (int cells = 0; cells < 12; cells++)
					{
						dataGridView1.Columns[cells + 1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
						dataGridView1.Rows[row].Cells[cells + 1].Value = Math.Round(GridValues[cells, row], 1).ToString("0.0");
					}
				}
			}
		}
	}

}
