namespace GralShape
{
	partial class ParseDBF
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
            groupBox1 = new System.Windows.Forms.GroupBox();
            radioButton7 = new System.Windows.Forms.RadioButton();
            radioButton6 = new System.Windows.Forms.RadioButton();
            radioButton5 = new System.Windows.Forms.RadioButton();
            radioButton4 = new System.Windows.Forms.RadioButton();
            radioButton3 = new System.Windows.Forms.RadioButton();
            radioButton2 = new System.Windows.Forms.RadioButton();
            radioButton1 = new System.Windows.Forms.RadioButton();
            button1 = new System.Windows.Forms.Button();
            groupBox2 = new System.Windows.Forms.GroupBox();
            radioButton9 = new System.Windows.Forms.RadioButton();
            radioButton8 = new System.Windows.Forms.RadioButton();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(radioButton7);
            groupBox1.Controls.Add(radioButton6);
            groupBox1.Controls.Add(radioButton5);
            groupBox1.Controls.Add(radioButton4);
            groupBox1.Controls.Add(radioButton3);
            groupBox1.Controls.Add(radioButton2);
            groupBox1.Controls.Add(radioButton1);
            groupBox1.Location = new System.Drawing.Point(15, 14);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(206, 231);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "String encoding";
            // 
            // radioButton7
            // 
            radioButton7.Location = new System.Drawing.Point(8, 196);
            radioButton7.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton7.Name = "radioButton7";
            radioButton7.Size = new System.Drawing.Size(190, 28);
            radioButton7.TabIndex = 1;
            radioButton7.Text = "UTF-32";
            radioButton7.UseVisualStyleBackColor = true;
            // 
            // radioButton6
            // 
            radioButton6.Location = new System.Drawing.Point(8, 167);
            radioButton6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton6.Name = "radioButton6";
            radioButton6.Size = new System.Drawing.Size(190, 28);
            radioButton6.TabIndex = 0;
            radioButton6.Text = "UTF-16 ";
            radioButton6.UseVisualStyleBackColor = true;
            // 
            // radioButton5
            // 
            radioButton5.Location = new System.Drawing.Point(8, 138);
            radioButton5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton5.Name = "radioButton5";
            radioButton5.Size = new System.Drawing.Size(190, 28);
            radioButton5.TabIndex = 0;
            radioButton5.Text = "UTF-7";
            radioButton5.UseVisualStyleBackColor = true;
            radioButton5.Visible = false;
            // 
            // radioButton4
            // 
            radioButton4.Location = new System.Drawing.Point(8, 110);
            radioButton4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton4.Name = "radioButton4";
            radioButton4.Size = new System.Drawing.Size(190, 28);
            radioButton4.TabIndex = 0;
            radioButton4.Text = "Western European (ISO) ";
            radioButton4.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            radioButton3.Location = new System.Drawing.Point(8, 81);
            radioButton3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton3.Name = "radioButton3";
            radioButton3.Size = new System.Drawing.Size(190, 28);
            radioButton3.TabIndex = 0;
            radioButton3.Text = "US-ASCII";
            radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            radioButton2.Checked = true;
            radioButton2.Location = new System.Drawing.Point(8, 23);
            radioButton2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new System.Drawing.Size(190, 28);
            radioButton2.TabIndex = 0;
            radioButton2.TabStop = true;
            radioButton2.Text = "Western European";
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            radioButton1.Location = new System.Drawing.Point(8, 52);
            radioButton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new System.Drawing.Size(190, 28);
            radioButton1.TabIndex = 0;
            radioButton1.Text = "UTF-8";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            button1.Location = new System.Drawing.Point(63, 354);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(100, 31);
            button1.TabIndex = 2;
            button1.Text = "&OK";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(radioButton9);
            groupBox2.Controls.Add(radioButton8);
            groupBox2.Location = new System.Drawing.Point(15, 253);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(206, 82);
            groupBox2.TabIndex = 3;
            groupBox2.TabStop = false;
            groupBox2.Text = "Culture settings";
            // 
            // radioButton9
            // 
            radioButton9.Location = new System.Drawing.Point(8, 45);
            radioButton9.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton9.Name = "radioButton9";
            radioButton9.Size = new System.Drawing.Size(190, 28);
            radioButton9.TabIndex = 3;
            radioButton9.Text = "Local culture";
            radioButton9.UseVisualStyleBackColor = true;
            // 
            // radioButton8
            // 
            radioButton8.Checked = true;
            radioButton8.Location = new System.Drawing.Point(8, 22);
            radioButton8.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton8.Name = "radioButton8";
            radioButton8.Size = new System.Drawing.Size(190, 28);
            radioButton8.TabIndex = 2;
            radioButton8.TabStop = true;
            radioButton8.Text = "Invariant culture";
            radioButton8.UseVisualStyleBackColor = true;
            // 
            // ParseDBF
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(236, 399);
            ControlBox = false;
            Controls.Add(groupBox2);
            Controls.Add(button1);
            Controls.Add(groupBox1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ParseDBF";
            Text = "Select encoding type";
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);

        }
        private System.Windows.Forms.RadioButton radioButton7;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.RadioButton radioButton3;
		private System.Windows.Forms.RadioButton radioButton4;
		private System.Windows.Forms.RadioButton radioButton5;
		private System.Windows.Forms.RadioButton radioButton6;
		private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButton9;
        private System.Windows.Forms.RadioButton radioButton8;
    }
}
