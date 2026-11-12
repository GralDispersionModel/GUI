namespace GralMainForms
{
    partial class GpuOptions
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
            groupBox1 = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            checkBox1 = new System.Windows.Forms.CheckBox();
            comboBox1 = new System.Windows.Forms.ComboBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            radioButton2 = new System.Windows.Forms.RadioButton();
            radioButton1 = new System.Windows.Forms.RadioButton();
            groupBox3 = new System.Windows.Forms.GroupBox();
            label2 = new System.Windows.Forms.Label();
            checkBox2 = new System.Windows.Forms.CheckBox();
            groupBox4 = new System.Windows.Forms.GroupBox();
            textBox1 = new System.Windows.Forms.TextBox();
            button3 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            //
            // groupBox1
            //
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(checkBox1);
            groupBox1.Controls.Add(comboBox1);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(420, 136);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Device (for this user, all projects)";
            //
            // label1
            //
            label1.Location = new System.Drawing.Point(12, 80);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(396, 48);
            label1.TabIndex = 2;
            label1.Text = "All GRAL instances of a start share one GPU.";
            //
            // checkBox1
            //
            checkBox1.AutoSize = true;
            checkBox1.Location = new System.Drawing.Point(12, 54);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(318, 19);
            checkBox1.TabIndex = 1;
            checkBox1.Text = "Use an integrated AMD GPU when no discrete GPU is found";
            toolTip1.SetToolTip(checkBox1, "GRAL has been compared with the CPU path on discrete GPUs only.\r\nWithout this permission a PC that has only an integrated AMD GPU does not start on the GPU.\r\nA chosen integrated GPU always has it.");
            checkBox1.UseVisualStyleBackColor = true;
            //
            // comboBox1
            //
            comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new System.Drawing.Point(12, 24);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new System.Drawing.Size(396, 23);
            comboBox1.TabIndex = 0;
            toolTip1.SetToolTip(comboBox1, "Automatic: GRAL selects the GPU.\r\nA chosen GPU is saved with its number and name; if another GPU has the number later,\r\nGRAL selects automatically and warns.");
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            //
            // groupBox2
            //
            groupBox2.Controls.Add(radioButton2);
            groupBox2.Controls.Add(radioButton1);
            groupBox2.Location = new System.Drawing.Point(12, 154);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(420, 76);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "If the GPU cannot start (for this user, all projects)";
            //
            // radioButton2
            //
            radioButton2.AutoSize = true;
            radioButton2.Location = new System.Drawing.Point(12, 46);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new System.Drawing.Size(141, 19);
            radioButton2.TabIndex = 1;
            radioButton2.Text = "Continue on the CPU";
            toolTip1.SetToolTip(radioButton2, "GRAL writes the reason to its log file and computes the whole run on the CPU.\r\nAn error after the GPU has started still stops the run.");
            radioButton2.UseVisualStyleBackColor = true;
            //
            // radioButton1
            //
            radioButton1.AutoSize = true;
            radioButton1.Checked = true;
            radioButton1.Location = new System.Drawing.Point(12, 22);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new System.Drawing.Size(160, 19);
            radioButton1.TabIndex = 0;
            radioButton1.TabStop = true;
            radioButton1.Text = "Stop and show the reason";
            radioButton1.UseVisualStyleBackColor = true;
            //
            // groupBox3
            //
            groupBox3.Controls.Add(label2);
            groupBox3.Controls.Add(checkBox2);
            groupBox3.Location = new System.Drawing.Point(12, 236);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(420, 84);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Project";
            //
            // label2
            //
            label2.Location = new System.Drawing.Point(12, 46);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(396, 32);
            label2.TabIndex = 1;
            label2.Text = "\"GRAL Reproducible Results\" (special GRAL settings) is off.";
            //
            // checkBox2
            //
            checkBox2.AutoSize = true;
            checkBox2.Location = new System.Drawing.Point(12, 22);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new System.Drawing.Size(300, 19);
            checkBox2.TabIndex = 0;
            checkBox2.Text = "Reproducible sums on the GPU with reproducible results";
            toolTip1.SetToolTip(checkBox2, "On: with the reproducible results option, the sums on the GPU are repeatable (slower in some cases).\r\nOff: faster in some cases, less repeatable.\r\nSaved in the project (in.dat line 24).");
            checkBox2.UseVisualStyleBackColor = true;
            //
            // groupBox4
            //
            groupBox4.Controls.Add(textBox1);
            groupBox4.Location = new System.Drawing.Point(12, 326);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new System.Drawing.Size(420, 164);
            groupBox4.TabIndex = 3;
            groupBox4.TabStop = false;
            groupBox4.Text = "Status";
            //
            // textBox1
            //
            textBox1.Location = new System.Drawing.Point(12, 22);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBox1.Size = new System.Drawing.Size(396, 130);
            textBox1.TabIndex = 0;
            textBox1.TabStop = false;
            //
            // button3
            //
            button3.Location = new System.Drawing.Point(12, 500);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(104, 27);
            button3.TabIndex = 4;
            button3.Text = "Show GPU &log";
            toolTip1.SetToolTip(button3, "Opens GRAL_GPU_runtime.log of the project's Computation folder");
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            //
            // button4
            //
            button4.Location = new System.Drawing.Point(122, 500);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(124, 27);
            button4.TabIndex = 5;
            button4.Text = "&Edit settings file";
            toolTip1.SetToolTip(button4, "Opens the GPU settings file of GRAL in the text editor.\r\nA new file lists every key as a comment line.");
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            //
            // button1
            //
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            button1.Location = new System.Drawing.Point(264, 500);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(80, 27);
            button1.TabIndex = 6;
            button1.Text = "&OK";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            //
            // button2
            //
            button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button2.Location = new System.Drawing.Point(352, 500);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(80, 27);
            button2.TabIndex = 7;
            button2.Text = "&Cancel";
            button2.UseVisualStyleBackColor = true;
            //
            // GpuOptions
            //
            AcceptButton = button1;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = button2;
            ClientSize = new System.Drawing.Size(444, 538);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(groupBox4);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GpuOptions";
            ShowInTaskbar = false;
            Text = "GPU options";
            Load += GpuOptions_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
