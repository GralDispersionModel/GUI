namespace GralMainForms
{
    partial class GUI_Settings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GUI_Settings));
            groupBox25 = new System.Windows.Forms.GroupBox();
            radioButton4 = new System.Windows.Forms.RadioButton();
            radioButton3 = new System.Windows.Forms.RadioButton();
            groupBox26 = new System.Windows.Forms.GroupBox();
            radioButton5 = new System.Windows.Forms.RadioButton();
            radioButton6 = new System.Windows.Forms.RadioButton();
            button1 = new System.Windows.Forms.Button();
            checkBox1 = new System.Windows.Forms.CheckBox();
            checkBox2 = new System.Windows.Forms.CheckBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            radioButton12 = new System.Windows.Forms.RadioButton();
            radioButton11 = new System.Windows.Forms.RadioButton();
            radioButton10 = new System.Windows.Forms.RadioButton();
            checkBox3 = new System.Windows.Forms.CheckBox();
            checkBox4 = new System.Windows.Forms.CheckBox();
            checkBox5 = new System.Windows.Forms.CheckBox();
            groupBox25.SuspendLayout();
            groupBox26.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox25
            // 
            groupBox25.Controls.Add(radioButton4);
            groupBox25.Controls.Add(radioButton3);
            groupBox25.Location = new System.Drawing.Point(14, 12);
            groupBox25.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox25.Name = "groupBox25";
            groupBox25.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox25.Size = new System.Drawing.Size(562, 81);
            groupBox25.TabIndex = 1;
            groupBox25.TabStop = false;
            groupBox25.Text = "Default path for the application settings";
            // 
            // radioButton4
            // 
            radioButton4.Location = new System.Drawing.Point(24, 46);
            radioButton4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton4.Name = "radioButton4";
            radioButton4.Size = new System.Drawing.Size(525, 28);
            radioButton4.TabIndex = 1;
            radioButton4.Text = "at the application data folder (Windows:  My Documents / Linux: home directory)";
            radioButton4.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            radioButton3.Location = new System.Drawing.Point(24, 23);
            radioButton3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton3.Name = "radioButton3";
            radioButton3.Size = new System.Drawing.Size(321, 28);
            radioButton3.TabIndex = 0;
            radioButton3.Text = "at the application executable path (default)";
            radioButton3.UseVisualStyleBackColor = true;
            // 
            // groupBox26
            // 
            groupBox26.Controls.Add(radioButton5);
            groupBox26.Controls.Add(radioButton6);
            groupBox26.Location = new System.Drawing.Point(14, 102);
            groupBox26.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox26.Name = "groupBox26";
            groupBox26.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox26.Size = new System.Drawing.Size(562, 81);
            groupBox26.TabIndex = 2;
            groupBox26.TabStop = false;
            groupBox26.Text = "Copy GRAL or GRAMM computation cores to the Computation folder?";
            // 
            // radioButton5
            // 
            radioButton5.Location = new System.Drawing.Point(24, 46);
            radioButton5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton5.Name = "radioButton5";
            radioButton5.Size = new System.Drawing.Size(525, 28);
            radioButton5.TabIndex = 3;
            radioButton5.Text = "No, start the core in the program folder  (default)";
            radioButton5.UseVisualStyleBackColor = true;
            // 
            // radioButton6
            // 
            radioButton6.Location = new System.Drawing.Point(24, 23);
            radioButton6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton6.Name = "radioButton6";
            radioButton6.Size = new System.Drawing.Size(321, 28);
            radioButton6.TabIndex = 2;
            radioButton6.Text = "Yes";
            radioButton6.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            button1.Location = new System.Drawing.Point(224, 433);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(135, 45);
            button1.TabIndex = 5;
            button1.Text = "&OK";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1Click;
            // 
            // checkBox1
            // 
            checkBox1.Location = new System.Drawing.Point(38, 297);
            checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(390, 28);
            checkBox1.TabIndex = 4;
            checkBox1.Text = "File compatibility to GUI version 19.01 and 20.01";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.Click += CheckBox1Click;
            // 
            // checkBox2
            // 
            checkBox2.Location = new System.Drawing.Point(38, 321);
            checkBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new System.Drawing.Size(390, 28);
            checkBox2.TabIndex = 6;
            checkBox2.Text = "Vector automatic scaling";
            checkBox2.UseVisualStyleBackColor = true;
            checkBox2.Click += checkBox2_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(radioButton12);
            groupBox1.Controls.Add(radioButton11);
            groupBox1.Controls.Add(radioButton10);
            groupBox1.Location = new System.Drawing.Point(14, 185);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(562, 98);
            groupBox1.TabIndex = 8;
            groupBox1.TabStop = false;
            groupBox1.Text = "Meteo Data Import";
            // 
            // radioButton12
            // 
            radioButton12.AutoSize = true;
            radioButton12.Location = new System.Drawing.Point(24, 69);
            radioButton12.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton12.Name = "radioButton12";
            radioButton12.Size = new System.Drawing.Size(378, 19);
            radioButton12.TabIndex = 2;
            radioButton12.TabStop = true;
            radioButton12.Text = "Shuffle wind direction for lines with wind velocity and direction = 0";
            radioButton12.UseVisualStyleBackColor = true;
            radioButton12.CheckedChanged += checkBox3_Click;
            // 
            // radioButton11
            // 
            radioButton11.AutoSize = true;
            radioButton11.Location = new System.Drawing.Point(24, 46);
            radioButton11.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton11.Name = "radioButton11";
            radioButton11.Size = new System.Drawing.Size(269, 19);
            radioButton11.TabIndex = 1;
            radioButton11.TabStop = true;
            radioButton11.Text = "Reject lines with wind speed and direction = 0 ";
            radioButton11.UseVisualStyleBackColor = true;
            radioButton11.CheckedChanged += checkBox3_Click;
            // 
            // radioButton10
            // 
            radioButton10.AutoSize = true;
            radioButton10.Location = new System.Drawing.Point(24, 23);
            radioButton10.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton10.Name = "radioButton10";
            radioButton10.Size = new System.Drawing.Size(113, 19);
            radioButton10.TabIndex = 0;
            radioButton10.TabStop = true;
            radioButton10.Text = "All data (default)";
            radioButton10.UseVisualStyleBackColor = true;
            radioButton10.CheckedChanged += checkBox3_Click;
            // 
            // checkBox3
            // 
            checkBox3.Location = new System.Drawing.Point(38, 346);
            checkBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox3.Name = "checkBox3";
            checkBox3.Size = new System.Drawing.Size(532, 28);
            checkBox3.TabIndex = 9;
            checkBox3.Text = "Delete *.con/*.grz/*.gff/*.wnd files to the recycle bin (safer but slower, Windows only)";
            checkBox3.UseVisualStyleBackColor = true;
            checkBox3.Click += checkBox3_Click_1;
            // 
            // checkBox4
            // 
            checkBox4.Location = new System.Drawing.Point(38, 371);
            checkBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox4.Name = "checkBox4";
            checkBox4.Size = new System.Drawing.Size(390, 28);
            checkBox4.TabIndex = 10;
            checkBox4.Text = "Search for updates when starting the application";
            checkBox4.UseVisualStyleBackColor = true;
            checkBox4.Click += checkBox4_Click;
            // 
            // checkBox5
            // 
            checkBox5.Location = new System.Drawing.Point(38, 396);
            checkBox5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox5.Name = "checkBox5";
            checkBox5.Size = new System.Drawing.Size(467, 28);
            checkBox5.TabIndex = 11;
            checkBox5.Text = "Use default windows colors (the app must be restarted)";
            checkBox5.UseVisualStyleBackColor = true;
            checkBox5.Click += checkBox5_Click;
            // 
            // GUI_Settings
            // 
            AcceptButton = button1;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(602, 499);
            Controls.Add(checkBox5);
            Controls.Add(checkBox4);
            Controls.Add(checkBox3);
            Controls.Add(groupBox1);
            Controls.Add(checkBox2);
            Controls.Add(checkBox1);
            Controls.Add(button1);
            Controls.Add(groupBox26);
            Controls.Add(groupBox25);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GUI_Settings";
            Text = "GUI Settings";
            Load += GUI_SettingsLoad;
            groupBox25.ResumeLayout(false);
            groupBox26.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);

        }
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.GroupBox groupBox26;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.GroupBox groupBox25;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton12;
        private System.Windows.Forms.RadioButton radioButton11;
        private System.Windows.Forms.RadioButton radioButton10;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox5;
    }
}
