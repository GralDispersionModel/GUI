namespace GralMainForms
{
    partial class SelectSourcegroups
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectSourcegroups));
            listBox1 = new System.Windows.Forms.ListBox();
            button1 = new System.Windows.Forms.Button();
            checkBox1 = new System.Windows.Forms.CheckBox();
            checkBox2 = new System.Windows.Forms.CheckBox();
            checkBox3 = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            textBox1 = new System.Windows.Forms.TextBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            label2 = new System.Windows.Forms.Label();
            numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            radioButton2 = new System.Windows.Forms.RadioButton();
            radioButton1 = new System.Windows.Forms.RadioButton();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label3 = new System.Windows.Forms.Label();
            numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            button2 = new System.Windows.Forms.Button();
            groupBox3 = new System.Windows.Forms.GroupBox();
            buttonResultPath = new System.Windows.Forms.Button();
            buttonModulationPath = new System.Windows.Forms.Button();
            textBox3 = new System.Windows.Forms.TextBox();
            label7 = new System.Windows.Forms.Label();
            textBox2 = new System.Windows.Forms.TextBox();
            label6 = new System.Windows.Forms.Label();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            checkBox4 = new System.Windows.Forms.CheckBox();
            checkBox5 = new System.Windows.Forms.CheckBox();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new System.Drawing.Point(0, 0);
            listBox1.Margin = new System.Windows.Forms.Padding(2);
            listBox1.Name = "listBox1";
            listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            listBox1.Size = new System.Drawing.Size(413, 199);
            listBox1.TabIndex = 0;
            toolTip1.SetToolTip(listBox1, "Calculated source groups");
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            button1.Location = new System.Drawing.Point(13, 532);
            button1.Margin = new System.Windows.Forms.Padding(2);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(105, 40);
            button1.TabIndex = 8;
            button1.Text = "&OK";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // checkBox1
            // 
            checkBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkBox1.Location = new System.Drawing.Point(262, 403);
            checkBox1.Margin = new System.Windows.Forms.Padding(2);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(150, 36);
            checkBox1.TabIndex = 3;
            checkBox1.Text = "Calculate\r\nmax. concentrations";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            checkBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkBox2.Checked = true;
            checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBox2.Location = new System.Drawing.Point(262, 364);
            checkBox2.Margin = new System.Windows.Forms.Padding(2);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new System.Drawing.Size(150, 36);
            checkBox2.TabIndex = 2;
            checkBox2.Text = "Calculate\r\nmean concentrations";
            checkBox2.UseVisualStyleBackColor = true;
            checkBox2.CheckedChanged += checkBox2_CheckedChanged;
            // 
            // checkBox3
            // 
            checkBox3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkBox3.Checked = true;
            checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBox3.Location = new System.Drawing.Point(262, 443);
            checkBox3.Margin = new System.Windows.Forms.Padding(2);
            checkBox3.Name = "checkBox3";
            checkBox3.Size = new System.Drawing.Size(150, 36);
            checkBox3.TabIndex = 4;
            checkBox3.Text = "Calculate daily\r\nmax. concentrations";
            checkBox3.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(8, 102);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(110, 28);
            label1.TabIndex = 5;
            label1.Text = "Result file-prefix";
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(164, 99);
            textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(219, 23);
            textBox1.TabIndex = 1;
            toolTip1.SetToolTip(textBox1, "A file prefix for the result files");
            textBox1.TextChanged += TextBox1TextChanged;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            groupBox1.Controls.Add(checkBox5);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(numericUpDown3);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(numericUpDown1);
            groupBox1.Controls.Add(radioButton2);
            groupBox1.Controls.Add(radioButton1);
            groupBox1.Location = new System.Drawing.Point(14, 357);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(229, 170);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Odour";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(183, 52);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(44, 15);
            label5.TabIndex = 6;
            label5.Text = "OU/m³";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(23, 52);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(94, 15);
            label4.TabIndex = 5;
            label4.Text = "Odour threshold";
            // 
            // numericUpDown3
            // 
            numericUpDown3.DecimalPlaces = 1;
            numericUpDown3.Enabled = false;
            numericUpDown3.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numericUpDown3.Location = new System.Drawing.Point(126, 46);
            numericUpDown3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDown3.Name = "numericUpDown3";
            numericUpDown3.Size = new System.Drawing.Size(56, 23);
            numericUpDown3.TabIndex = 4;
            numericUpDown3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            numericUpDown3.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(23, 115);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(59, 15);
            label2.TabIndex = 3;
            label2.Text = "Percentile";
            // 
            // numericUpDown1
            // 
            numericUpDown1.DecimalPlaces = 1;
            numericUpDown1.Enabled = false;
            numericUpDown1.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numericUpDown1.Location = new System.Drawing.Point(126, 110);
            numericUpDown1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new System.Drawing.Size(56, 23);
            numericUpDown1.TabIndex = 6;
            numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            toolTip1.SetToolTip(numericUpDown1, "Percentile to be evaluated");
            numericUpDown1.Value = new decimal(new int[] { 98, 0, 0, 0 });
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Location = new System.Drawing.Point(8, 85);
            radioButton2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new System.Drawing.Size(136, 19);
            radioButton2.TabIndex = 7;
            radioButton2.Text = "Odour concentration";
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Checked = true;
            radioButton1.Location = new System.Drawing.Point(8, 23);
            radioButton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new System.Drawing.Size(143, 19);
            radioButton1.TabIndex = 0;
            radioButton1.TabStop = true;
            radioButton1.Text = "Odour hour frequency";
            radioButton1.UseVisualStyleBackColor = true;
            radioButton1.CheckedChanged += radioButton1_CheckedChanged;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            groupBox2.Controls.Add(checkBox4);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(numericUpDown2);
            groupBox2.Location = new System.Drawing.Point(13, 357);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(230, 75);
            groupBox2.TabIndex = 8;
            groupBox2.TabStop = false;
            groupBox2.Text = "Percentile Evaluation";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(24, 25);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(59, 15);
            label3.TabIndex = 3;
            label3.Text = "Percentile";
            // 
            // numericUpDown2
            // 
            numericUpDown2.DecimalPlaces = 1;
            numericUpDown2.Enabled = false;
            numericUpDown2.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numericUpDown2.Location = new System.Drawing.Point(127, 23);
            numericUpDown2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDown2.Minimum = new decimal(new int[] { 90, 0, 0, 0 });
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new System.Drawing.Size(56, 23);
            numericUpDown2.TabIndex = 5;
            numericUpDown2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            toolTip1.SetToolTip(numericUpDown2, "Percentile to be evaluated");
            numericUpDown2.Value = new decimal(new int[] { 98, 0, 0, 0 });
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button2.Location = new System.Drawing.Point(295, 532);
            button2.Margin = new System.Windows.Forms.Padding(2);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(105, 40);
            button2.TabIndex = 9;
            button2.Text = "&Cancel";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button1_Click;
            // 
            // groupBox3
            // 
            groupBox3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            groupBox3.Controls.Add(buttonResultPath);
            groupBox3.Controls.Add(buttonModulationPath);
            groupBox3.Controls.Add(textBox3);
            groupBox3.Controls.Add(label7);
            groupBox3.Controls.Add(textBox2);
            groupBox3.Controls.Add(label6);
            groupBox3.Controls.Add(textBox1);
            groupBox3.Controls.Add(label1);
            groupBox3.Location = new System.Drawing.Point(14, 211);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(390, 135);
            groupBox3.TabIndex = 10;
            groupBox3.TabStop = false;
            groupBox3.Text = "Emission Modulation/Result Files";
            // 
            // buttonResultPath
            // 
            buttonResultPath.BackColor = System.Drawing.Color.Gainsboro;
            buttonResultPath.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonResultPath.BackgroundImage");
            buttonResultPath.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            buttonResultPath.Font = new System.Drawing.Font("Segoe UI", 9F);
            buttonResultPath.Location = new System.Drawing.Point(119, 60);
            buttonResultPath.Name = "buttonResultPath";
            buttonResultPath.Size = new System.Drawing.Size(32, 32);
            buttonResultPath.TabIndex = 47;
            toolTip1.SetToolTip(buttonResultPath, "Set the path for the result files");
            buttonResultPath.UseVisualStyleBackColor = false;
            buttonResultPath.Click += buttonSelectPath_Click;
            // 
            // buttonModulationPath
            // 
            buttonModulationPath.BackColor = System.Drawing.Color.Gainsboro;
            buttonModulationPath.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonModulationPath.BackgroundImage");
            buttonModulationPath.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            buttonModulationPath.Font = new System.Drawing.Font("Segoe UI", 9F);
            buttonModulationPath.Location = new System.Drawing.Point(119, 22);
            buttonModulationPath.Name = "buttonModulationPath";
            buttonModulationPath.Size = new System.Drawing.Size(32, 32);
            buttonModulationPath.TabIndex = 46;
            toolTip1.SetToolTip(buttonModulationPath, "Set the path with emission modulation files for one evaluation");
            buttonModulationPath.UseVisualStyleBackColor = false;
            buttonModulationPath.Click += buttonSelectPath_Click;
            // 
            // textBox3
            // 
            textBox3.Enabled = false;
            textBox3.Location = new System.Drawing.Point(164, 64);
            textBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox3.Name = "textBox3";
            textBox3.Size = new System.Drawing.Size(219, 23);
            textBox3.TabIndex = 9;
            textBox3.WordWrap = false;
            // 
            // label7
            // 
            label7.Location = new System.Drawing.Point(8, 67);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(109, 28);
            label7.TabIndex = 8;
            label7.Text = "Path for result files";
            // 
            // textBox2
            // 
            textBox2.Enabled = false;
            textBox2.Location = new System.Drawing.Point(164, 24);
            textBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(219, 23);
            textBox2.TabIndex = 7;
            textBox2.WordWrap = false;
            // 
            // label6
            // 
            label6.Location = new System.Drawing.Point(8, 24);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(109, 35);
            label6.TabIndex = 6;
            label6.Text = "Path to emission modulation files";
            // 
            // checkBox4
            // 
            checkBox4.AutoSize = true;
            checkBox4.Location = new System.Drawing.Point(26, 53);
            checkBox4.Name = "checkBox4";
            checkBox4.Size = new System.Drawing.Size(180, 19);
            checkBox4.TabIndex = 6;
            checkBox4.Text = "based on hourly mean values";
            checkBox4.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            checkBox5.AutoSize = true;
            checkBox5.Location = new System.Drawing.Point(25, 143);
            checkBox5.Name = "checkBox5";
            checkBox5.Size = new System.Drawing.Size(180, 19);
            checkBox5.TabIndex = 8;
            checkBox5.Text = "based on hourly mean values";
            checkBox5.UseVisualStyleBackColor = true;
            // 
            // SelectSourcegroups
            // 
            AcceptButton = button1;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = button2;
            ClientSize = new System.Drawing.Size(413, 586);
            ControlBox = false;
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(checkBox3);
            Controls.Add(checkBox2);
            Controls.Add(checkBox1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(listBox1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(2);
            MinimumSize = new System.Drawing.Size(370, 600);
            Name = "SelectSourcegroups";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Select Source Groups";
            Load += SelectSourcegroups_Load;
            Shown += SelectSourcegroupsShown;
            ResizeEnd += SelectSourcegroupsResizeEnd;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
        }

        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;

        #endregion

        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.CheckBox checkBox1;
        public System.Windows.Forms.ListBox listBox1;
        public System.Windows.Forms.CheckBox checkBox2;
        public System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.RadioButton radioButton2;
        public System.Windows.Forms.RadioButton radioButton1;
        public System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonResultPath;
        private System.Windows.Forms.Button buttonModulationPath;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox4;
    }
}