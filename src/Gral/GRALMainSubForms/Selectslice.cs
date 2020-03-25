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
using System.Windows.Forms;
using System.IO;
using GralIO;
using Gral;

namespace GralMainForms
{
	/// <summary>
    /// Select a horizontal slice
    /// </summary>
    public partial class Selectslice : Form
    {
        private Main form1 = null;

        public Selectslice(Main f)
        {
            InitializeComponent();
            button1.DialogResult = DialogResult.OK;
            form1 = f;
        }

        private void Selectslice_Load(object sender, EventArgs e)
        {
            //fill listbox with heights of horizontal slices
            InDatVariables data = new InDatVariables();
			InDatFileIO ReadInData = new InDatFileIO();
			
			double[] horslice = new double[10];
			data.InDatPath = Path.Combine(Main.ProjectName, @"Computation","in.dat");
			data.HorSlices = horslice; // initialize array object !
			ReadInData.Data = data;
			
			if (ReadInData.ReadInDat() == true)
			{
				data = ReadInData.Data;
				
				horslice = data.HorSlices; // hoirizontal slice heights
				
				for (int i = 0; i < data.NumHorSlices; i++)
				{
					if (i < horslice.Length && horslice[i] > 0)
                    {
                        listBox1.Items.Add(horslice[i]+"m");
                    }
                }
			}
			else // in.dat not available
			{
				for (int i = 0; i < form1.GRALSettings.NumHorSlices; i++)
				{
					if (i < form1.GRALSettings.HorSlices.Length && form1.GRALSettings.HorSlices[i] > 0)
                    {
                        listBox1.Items.Add(form1.GRALSettings.HorSlices[i]+"m");
                    }
                }
			}
			data = null;
			ReadInData = null;
			
            listBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
        }
    }
}