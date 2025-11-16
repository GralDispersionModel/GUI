namespace GralDomForms
{
	partial class DomainWindRoseDialog
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DomainWindRoseDialog));
            listBox1 = new System.Windows.Forms.ListBox();
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            radioButton1 = new System.Windows.Forms.RadioButton();
            radioButton2 = new System.Windows.Forms.RadioButton();
            checkBox1 = new System.Windows.Forms.CheckBox();
            checkBox2 = new System.Windows.Forms.CheckBox();
            checkBox3 = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            SuspendLayout();
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Location = new System.Drawing.Point(13, 17);
            listBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listBox1.Name = "listBox1";
            listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            listBox1.Size = new System.Drawing.Size(447, 199);
            listBox1.TabIndex = 0;
            listBox1.KeyDown += ListBox1KeyDown;
            listBox1.KeyUp += ListBox1KeyUp;
            // 
            // button2
            // 
            button2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            button2.Image = (System.Drawing.Image)resources.GetObject("button2.Image");
            button2.Location = new System.Drawing.Point(481, 99);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(37, 37);
            button2.TabIndex = 1;
            toolTip1.SetToolTip(button2, "Remove the selected meteo station");
            button2.UseVisualStyleBackColor = true;
            button2.Click += Button2Click;
            // 
            // button1
            // 
            button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            button1.Image = Gral.Properties.Resources.AddSmall;
            button1.Location = new System.Drawing.Point(481, 29);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(37, 37);
            button1.TabIndex = 0;
            toolTip1.SetToolTip(button1, "Add a meteo station");
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1Click;
            // 
            // button3
            // 
            button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button3.Location = new System.Drawing.Point(40, 335);
            button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(125, 38);
            button3.TabIndex = 9;
            button3.Text = "&Cancel";
            button3.UseVisualStyleBackColor = true;
            button3.Click += Button3Click;
            // 
            // button4
            // 
            button4.DialogResult = System.Windows.Forms.DialogResult.OK;
            button4.Location = new System.Drawing.Point(372, 335);
            button4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(125, 38);
            button4.TabIndex = 10;
            button4.Text = "&OK";
            button4.UseVisualStyleBackColor = true;
            button4.Click += Button4Click;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new System.Drawing.Point(40, 255);
            numericUpDown1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDown1.Maximum = new decimal(new int[] { 15000, 0, 0, 0 });
            numericUpDown1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new System.Drawing.Size(70, 23);
            numericUpDown1.TabIndex = 2;
            toolTip1.SetToolTip(numericUpDown1, "Diameter of the wind rose in meters");
            numericUpDown1.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(36, 233);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(110, 18);
            label1.TabIndex = 45;
            label1.Text = "Size [m]";
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(153, 233);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(141, 18);
            label2.TabIndex = 47;
            label2.Text = "Max. velocity [m/s]";
            // 
            // numericUpDown2
            // 
            numericUpDown2.Location = new System.Drawing.Point(153, 255);
            numericUpDown2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDown2.Maximum = new decimal(new int[] { 15, 0, 0, 0 });
            numericUpDown2.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new System.Drawing.Size(70, 23);
            numericUpDown2.TabIndex = 3;
            toolTip1.SetToolTip(numericUpDown2, "Set the maximum wind velocity");
            numericUpDown2.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // radioButton1
            // 
            radioButton1.Location = new System.Drawing.Point(300, 231);
            radioButton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new System.Drawing.Size(121, 28);
            radioButton1.TabIndex = 4;
            radioButton1.TabStop = true;
            radioButton1.Text = "Velocity";
            toolTip1.SetToolTip(radioButton1, "Show the wind velocity rose");
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            radioButton2.Location = new System.Drawing.Point(300, 255);
            radioButton2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new System.Drawing.Size(121, 28);
            radioButton2.TabIndex = 48;
            radioButton2.TabStop = true;
            radioButton2.Text = "Stability";
            toolTip1.SetToolTip(radioButton2, "Show the stability rose");
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Checked = true;
            checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBox1.Location = new System.Drawing.Point(385, 237);
            checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(104, 19);
            checkBox1.TabIndex = 6;
            checkBox1.Text = "Bias correction";
            toolTip1.SetToolTip(checkBox1, "Apply a bias correction for classified meteorological situations ");
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Location = new System.Drawing.Point(385, 263);
            checkBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new System.Drawing.Size(92, 19);
            checkBox2.TabIndex = 7;
            checkBox2.Text = "Draw frames";
            toolTip1.SetToolTip(checkBox2, "Apply a frame for each wind sector");
            checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            checkBox3.AutoSize = true;
            checkBox3.Location = new System.Drawing.Point(385, 290);
            checkBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox3.Name = "checkBox3";
            checkBox3.Size = new System.Drawing.Size(124, 19);
            checkBox3.TabIndex = 8;
            checkBox3.Text = "Draw small sectors";
            toolTip1.SetToolTip(checkBox3, "Draw small wind sectors");
            checkBox3.UseVisualStyleBackColor = true;
            // 
            // DomainWindRoseDialog
            // 
            AcceptButton = button4;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = button3;
            ClientSize = new System.Drawing.Size(532, 387);
            Controls.Add(checkBox3);
            Controls.Add(checkBox2);
            Controls.Add(checkBox1);
            Controls.Add(radioButton2);
            Controls.Add(radioButton1);
            Controls.Add(label2);
            Controls.Add(numericUpDown2);
            Controls.Add(label1);
            Controls.Add(numericUpDown1);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(listBox1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "DomainWindRoseDialog";
            Text = "Add wind rose";
            Load += DomainWindRoseDialogLoad;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }
        private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.NumericUpDown numericUpDown2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
    }
}
