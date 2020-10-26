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
 * User: Markus_2
 * Date: 08.05.2015
 * Time: 20:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Windows.Forms;

namespace GralMessage
{
    
    public delegate void ProgressbarUpdateDelegate(object sender, int max);
    
	/// <summary>
    /// Show a progress bar with cancel button
    /// </summary>
    public partial class WaitProgressbarCancel : Form
	{
		public ProgressbarUpdateDelegate UpdateProgressDelegate;
		public WaitProgressbarCancel(string Title)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			progressbar.Minimum = 1;
			progressbar.Maximum = 100;
			progressbar.Value = 100;
			progressbar.Step = 1;
			UpdateProgressDelegate = new ProgressbarUpdateDelegate(ProgressbarUpdate);
            this.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 60;
            this.Top = 60;
        }
		
		public void ProgressbarUpdate(object sender, int max)
		{
			if (max == 0)
			{
				progressbar.PerformStep();	
			}
			else
			{
				progressbar.Maximum = max;
				progressbar.Value = 1;
			}	
		}

		void Cancel_buttonMouseClick(object sender, MouseEventArgs e)
		{
            if (GralDomain.Domain.CancellationTokenSource != null)
            {
                GralDomain.Domain.CancellationTokenSource.Cancel();
            }	
		}
	}
}
