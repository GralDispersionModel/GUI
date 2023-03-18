namespace GralBackgroundworkers
{
	partial class ProgressFormBackgroundworker
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
            this.BGW_Done = new System.Windows.Forms.Label();
            this.Rechenknecht = new System.ComponentModel.BackgroundWorker();
            this.usertext = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BGW_Done
            // 
            this.BGW_Done.Location = new System.Drawing.Point(12, 9);
            this.BGW_Done.Name = "BGW_Done";
            this.BGW_Done.Size = new System.Drawing.Size(635, 21);
            this.BGW_Done.TabIndex = 0;
            this.BGW_Done.Text = "working....";
            // 
            // Rechenknecht
            // 
            this.Rechenknecht.DoWork += new System.ComponentModel.DoWorkEventHandler(this.RechenknechtDoWork);
            // 
            // usertext
            // 
            this.usertext.Location = new System.Drawing.Point(10, 62);
            this.usertext.Multiline = true;
            this.usertext.Name = "usertext";
            this.usertext.ReadOnly = true;
            this.usertext.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.usertext.Size = new System.Drawing.Size(638, 261);
            this.usertext.TabIndex = 1;
            this.usertext.TabStop = false;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(571, 341);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "&Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 33);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(634, 23);
            this.progressBar1.TabIndex = 3;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(12, 341);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "C&lose";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // ProgressFormBackgroundworker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(660, 376);
            this.ControlBox = false;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.usertext);
            this.Controls.Add(this.BGW_Done);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(676, 234);
            this.Name = "ProgressFormBackgroundworker";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ProgressFormBackgroundworker_FormClosed);
            this.Load += new System.EventHandler(this.Progress_FormLoad);
            this.Shown += new System.EventHandler(this.Progress_FormShown);
            this.SizeChanged += new System.EventHandler(this.ProgressFormBackgroundworker_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox usertext;
		private System.ComponentModel.BackgroundWorker Rechenknecht;
		private System.Windows.Forms.Label BGW_Done;
        private System.Windows.Forms.Button button2;
    }
}
