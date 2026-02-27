namespace GralDomForms
{
	partial class VerticalProfileConcentration
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerticalProfileConcentration));
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            panel1 = new System.Windows.Forms.Panel();
            button3 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            button7 = new System.Windows.Forms.Button();
            button6 = new System.Windows.Forms.Button();
            button5 = new System.Windows.Forms.Button();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button2.Location = new System.Drawing.Point(292, 0);
            button2.Margin = new System.Windows.Forms.Padding(2);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(19, 19);
            button2.TabIndex = 5;
            button2.Text = "-";
            button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button1.Location = new System.Drawing.Point(273, 0);
            button1.Margin = new System.Windows.Forms.Padding(2);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(20, 19);
            button1.TabIndex = 4;
            button1.Text = "+";
            button1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.Color.White;
            panel1.Controls.Add(button3);
            panel1.Controls.Add(button4);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(284, 433);
            panel1.TabIndex = 0;
            // 
            // button3
            // 
            button3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button3.Location = new System.Drawing.Point(265, 2);
            button3.Margin = new System.Windows.Forms.Padding(2);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(19, 19);
            button3.TabIndex = 5;
            button3.Text = "-";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button4.Location = new System.Drawing.Point(246, 2);
            button4.Margin = new System.Windows.Forms.Padding(2);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(20, 19);
            button4.TabIndex = 4;
            button4.Text = "+";
            button4.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = System.Drawing.Color.White;
            pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox1.Location = new System.Drawing.Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(284, 493);
            pictureBox1.TabIndex = 15;
            pictureBox1.TabStop = false;
            pictureBox1.Paint += PictureBox1Paint;
            // 
            // button7
            // 
            button7.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button7.BackgroundImage = (System.Drawing.Image)resources.GetObject("button7.BackgroundImage");
            button7.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            button7.Location = new System.Drawing.Point(251, 39);
            button7.Name = "button7";
            button7.Size = new System.Drawing.Size(32, 32);
            button7.TabIndex = 16;
            button7.UseVisualStyleBackColor = true;
            button7.Click += Button7Click;
            // 
            // button6
            // 
            button6.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button6.Location = new System.Drawing.Point(215, 2);
            button6.Margin = new System.Windows.Forms.Padding(2);
            button6.Name = "button6";
            button6.Size = new System.Drawing.Size(32, 32);
            button6.TabIndex = 17;
            button6.Text = "+";
            button6.UseVisualStyleBackColor = true;
            button6.Click += Button6Click;
            // 
            // button5
            // 
            button5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button5.Location = new System.Drawing.Point(251, 2);
            button5.Margin = new System.Windows.Forms.Padding(2);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(32, 32);
            button5.TabIndex = 18;
            button5.Text = "-";
            button5.UseVisualStyleBackColor = true;
            button5.Click += Button5Click;
            // 
            // VerticalProfileConcentration
            // 
            ClientSize = new System.Drawing.Size(284, 493);
            Controls.Add(button5);
            Controls.Add(button6);
            Controls.Add(button7);
            Controls.Add(pictureBox1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Name = "VerticalProfileConcentration";
            Text = "Vertical Concentration Profile";
            TopMost = true;
            FormClosed += VerticalProfileConcentrationFormClosed;
            Load += VerticalProfileConcentrationLoad;
            ResizeEnd += VerticalProfileConcentrationResizeEnd;
            KeyDown += VerticalProfileConcentrationKeyDown;
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }
        private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
	}
}
