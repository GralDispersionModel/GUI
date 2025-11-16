namespace GralMainForms
{
    partial class SpecialSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpecialSettings));
            groupBox1 = new System.Windows.Forms.GroupBox();
            checkBox1 = new System.Windows.Forms.CheckBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            label1 = new System.Windows.Forms.Label();
            checkBox2 = new System.Windows.Forms.CheckBox();
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            checkBox3 = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            checkBox4 = new System.Windows.Forms.CheckBox();
            numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            checkBox5 = new System.Windows.Forms.CheckBox();
            checkBox6 = new System.Windows.Forms.CheckBox();
            label76 = new System.Windows.Forms.Label();
            label77 = new System.Windows.Forms.Label();
            numericUpDown28 = new System.Windows.Forms.NumericUpDown();
            numericUpDown27 = new System.Windows.Forms.NumericUpDown();
            groupBox3 = new System.Windows.Forms.GroupBox();
            label3 = new System.Windows.Forms.Label();
            groupBox4 = new System.Windows.Forms.GroupBox();
            groupBox5 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown28).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown27).BeginInit();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox5.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(checkBox1);
            groupBox1.Location = new System.Drawing.Point(18, 17);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(338, 65);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Write additional ASCii result files";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new System.Drawing.Point(12, 25);
            checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(76, 19);
            checkBox1.TabIndex = 0;
            checkBox1.Text = "Activated";
            toolTip1.SetToolTip(checkBox1, "Write result files in ASCii format");
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(numericUpDown1);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(checkBox2);
            groupBox2.Location = new System.Drawing.Point(18, 90);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(338, 98);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Read temp. transient files when restarting with situation 1";
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new System.Drawing.Point(205, 55);
            numericUpDown1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDown1.Maximum = new decimal(new int[] { 240, 0, 0, 0 });
            numericUpDown1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new System.Drawing.Size(54, 23);
            numericUpDown1.TabIndex = 3;
            numericUpDown1.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(8, 58);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(169, 15);
            label1.TabIndex = 2;
            label1.Text = "Save interval of temporary files";
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Location = new System.Drawing.Point(12, 25);
            checkBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new System.Drawing.Size(76, 19);
            checkBox2.TabIndex = 1;
            checkBox2.Text = "Activated";
            toolTip1.SetToolTip(checkBox2, "Override the default continuation of a calculation and start a calculation again with\r\nSituation 1 by reusing the temporary concentration ");
            checkBox2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            button1.Location = new System.Drawing.Point(17, 554);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(105, 33);
            button1.TabIndex = 2;
            button1.Text = "&OK";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button2.Location = new System.Drawing.Point(250, 554);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(105, 33);
            button2.TabIndex = 2;
            button2.Text = "&Cancel";
            button2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            checkBox3.AutoSize = true;
            checkBox3.Checked = true;
            checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 3.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Millimeter);
            checkBox3.Location = new System.Drawing.Point(18, 455);
            checkBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox3.Name = "checkBox3";
            checkBox3.Size = new System.Drawing.Size(200, 20);
            checkBox3.TabIndex = 60;
            checkBox3.Text = "Keystroke when exiting GRAL";
            toolTip1.SetToolTip(checkBox3, "Wait for a keystroke at the end of a calculation");
            checkBox3.UseVisualStyleBackColor = true;
            checkBox3.CheckedChanged += checkBox3_CheckedChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(26, 421);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(54, 15);
            label2.TabIndex = 2;
            label2.Text = "Log level";
            // 
            // numericUpDown2
            // 
            numericUpDown2.Location = new System.Drawing.Point(104, 419);
            numericUpDown2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDown2.Maximum = new decimal(new int[] { 3, 0, 0, 0 });
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new System.Drawing.Size(54, 23);
            numericUpDown2.TabIndex = 3;
            toolTip1.SetToolTip(numericUpDown2, "Enables an additional output for troubleshooting or for checking the internal particle assignment\r\nand the number of reflections or time loops. This value is not stored.");
            // 
            // checkBox4
            // 
            checkBox4.AutoSize = true;
            checkBox4.Location = new System.Drawing.Point(12, 29);
            checkBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox4.Name = "checkBox4";
            checkBox4.Size = new System.Drawing.Size(76, 19);
            checkBox4.TabIndex = 1;
            checkBox4.Text = "Activated";
            toolTip1.SetToolTip(checkBox4, "Override the default continuation of a calculation and start a calculation again with\r\nSituation 1 by reusing the temporary concentration ");
            checkBox4.UseVisualStyleBackColor = true;
            checkBox4.CheckedChanged += checkBox4_CheckedChanged;
            // 
            // numericUpDown3
            // 
            numericUpDown3.Enabled = false;
            numericUpDown3.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDown3.Location = new System.Drawing.Point(187, 59);
            numericUpDown3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDown3.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numericUpDown3.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            numericUpDown3.Name = "numericUpDown3";
            numericUpDown3.Size = new System.Drawing.Size(72, 23);
            numericUpDown3.TabIndex = 3;
            toolTip1.SetToolTip(numericUpDown3, "Beyond this radius around sources, the wind field is calculated diagnostically \r\neven in the presence of buildings or vegetation areas");
            numericUpDown3.Value = new decimal(new int[] { 150, 0, 0, 0 });
            numericUpDown3.ValueChanged += numericUpDown3_ValueChanged;
            // 
            // checkBox5
            // 
            checkBox5.AutoSize = true;
            checkBox5.Checked = true;
            checkBox5.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 3.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Millimeter);
            checkBox5.Location = new System.Drawing.Point(18, 485);
            checkBox5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox5.Name = "checkBox5";
            checkBox5.Size = new System.Drawing.Size(213, 20);
            checkBox5.TabIndex = 61;
            checkBox5.Text = "Disable GRAL Online Functions";
            toolTip1.SetToolTip(checkBox5, "Wait for a keystroke at the end of a calculation");
            checkBox5.UseVisualStyleBackColor = true;
            // 
            // checkBox6
            // 
            checkBox6.AutoSize = true;
            checkBox6.Checked = true;
            checkBox6.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 3.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Millimeter);
            checkBox6.Location = new System.Drawing.Point(18, 515);
            checkBox6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox6.Name = "checkBox6";
            checkBox6.Size = new System.Drawing.Size(195, 20);
            checkBox6.TabIndex = 62;
            checkBox6.Text = "GRAL Reproducible Results";
            toolTip1.SetToolTip(checkBox6, "This GRAL option gives reproducible results, but the flow field calculation is significantly slower\r\nGRAL V24.09 or higher is required");
            checkBox6.UseVisualStyleBackColor = true;
            checkBox6.CheckedChanged += checkBox6_CheckedChanged;
            // 
            // label76
            // 
            label76.AutoSize = true;
            label76.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label76.Location = new System.Drawing.Point(11, 55);
            label76.Name = "label76";
            label76.Size = new System.Drawing.Size(151, 15);
            label76.TabIndex = 39;
            label76.Text = "Relaxation pressure correct.";
            toolTip1.SetToolTip(label76, "Relaxation factor for temperature,\r\nhumidity, TKE, dissipation");
            // 
            // label77
            // 
            label77.AutoSize = true;
            label77.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label77.Location = new System.Drawing.Point(11, 30);
            label77.Name = "label77";
            label77.Size = new System.Drawing.Size(105, 15);
            label77.TabIndex = 37;
            label77.Text = "Relaxation velocity";
            toolTip1.SetToolTip(label77, "Relaxation factor for velocity");
            // 
            // numericUpDown28
            // 
            numericUpDown28.DecimalPlaces = 2;
            numericUpDown28.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            numericUpDown28.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numericUpDown28.Location = new System.Drawing.Point(190, 23);
            numericUpDown28.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown28.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            numericUpDown28.Name = "numericUpDown28";
            numericUpDown28.Size = new System.Drawing.Size(72, 23);
            numericUpDown28.TabIndex = 38;
            numericUpDown28.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            toolTip1.SetToolTip(numericUpDown28, "The lower the relaxation factor,\r\nthe higher the numerical stability");
            numericUpDown28.Value = new decimal(new int[] { 1, 0, 0, 65536 });
            // 
            // numericUpDown27
            // 
            numericUpDown27.DecimalPlaces = 2;
            numericUpDown27.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            numericUpDown27.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numericUpDown27.Location = new System.Drawing.Point(190, 51);
            numericUpDown27.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown27.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            numericUpDown27.Name = "numericUpDown27";
            numericUpDown27.Size = new System.Drawing.Size(72, 23);
            numericUpDown27.TabIndex = 40;
            numericUpDown27.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            toolTip1.SetToolTip(numericUpDown27, "The lower the relaxation factor,\r\nthe higher the numerical stability");
            numericUpDown27.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(numericUpDown3);
            groupBox3.Controls.Add(label3);
            groupBox3.Controls.Add(checkBox4);
            groupBox3.Location = new System.Drawing.Point(23, 22);
            groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Size = new System.Drawing.Size(295, 91);
            groupBox3.TabIndex = 4;
            groupBox3.TabStop = false;
            groupBox3.Text = "Reduce the size of prognostic sub domains";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(8, 61);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(77, 15);
            label3.TabIndex = 2;
            label3.Text = "Radius in [m]";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(label76);
            groupBox4.Controls.Add(label77);
            groupBox4.Controls.Add(numericUpDown28);
            groupBox4.Controls.Add(numericUpDown27);
            groupBox4.Location = new System.Drawing.Point(23, 119);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new System.Drawing.Size(295, 80);
            groupBox4.TabIndex = 63;
            groupBox4.TabStop = false;
            groupBox4.Text = "Individual GRAL Relaxation factors";
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(groupBox3);
            groupBox5.Controls.Add(groupBox4);
            groupBox5.Location = new System.Drawing.Point(18, 199);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new System.Drawing.Size(338, 207);
            groupBox5.TabIndex = 64;
            groupBox5.TabStop = false;
            groupBox5.Text = "Flow Field Settings";
            // 
            // SpecialSettings
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(373, 608);
            Controls.Add(groupBox5);
            Controls.Add(checkBox6);
            Controls.Add(checkBox5);
            Controls.Add(numericUpDown2);
            Controls.Add(checkBox3);
            Controls.Add(label2);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "SpecialSettings";
            Text = "Special GRAL Settings";
            Load += Main_SpecialSettings_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown28).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown27).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox5.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label76;
        private System.Windows.Forms.Label label77;
        private System.Windows.Forms.NumericUpDown numericUpDown28;
        private System.Windows.Forms.NumericUpDown numericUpDown27;
        private System.Windows.Forms.GroupBox groupBox5;
    }
}