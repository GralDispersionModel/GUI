/*
 * Created by SharpDevelop.
 * User: Markus_2
 * Date: 08.05.2015
 * Time: 20:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace GralMessage
{
	partial class Waitprogressbar_Cancel
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.cancel_button = new System.Windows.Forms.Button();
            this.progressbar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(100, 45);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(100, 33);
            this.cancel_button.TabIndex = 0;
            this.cancel_button.Text = "&Cancel";
            this.cancel_button.UseVisualStyleBackColor = false;
            this.cancel_button.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Cancel_buttonMouseClick);
            // 
            // progressbar
            // 
            this.progressbar.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.progressbar.Location = new System.Drawing.Point(22, 12);
            this.progressbar.Name = "progressbar";
            this.progressbar.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.progressbar.Size = new System.Drawing.Size(250, 24);
            this.progressbar.TabIndex = 1;
            this.progressbar.UseWaitCursor = true;
            // 
            // Waitprogressbar_Cancel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 87);
            this.ControlBox = false;
            this.Controls.Add(this.progressbar);
            this.Controls.Add(this.cancel_button);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Waitprogressbar_Cancel";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Waitprogressbar_Cancel";
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.ProgressBar progressbar;
		private System.Windows.Forms.Button cancel_button;
	}
}
