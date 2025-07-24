namespace GralMainForms
{
    partial class Emissionvariation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Emissionvariation));
            button20 = new System.Windows.Forms.Button();
            button19 = new System.Windows.Forms.Button();
            label33 = new System.Windows.Forms.Label();
            label32 = new System.Windows.Forms.Label();
            comboBox2 = new System.Windows.Forms.ComboBox();
            comboBox1 = new System.Windows.Forms.ComboBox();
            panel1 = new System.Windows.Forms.Panel();
            panel2 = new System.Windows.Forms.Panel();
            button1 = new System.Windows.Forms.Button();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            textBox1 = new System.Windows.Forms.TextBox();
            textBox2 = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            button2 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            button5 = new System.Windows.Forms.Button();
            button6 = new System.Windows.Forms.Button();
            dataGridView1 = new System.Windows.Forms.DataGridView();
            dataGridView2 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            SuspendLayout();
            // 
            // button20
            // 
            button20.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button20.Location = new System.Drawing.Point(853, 152);
            button20.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button20.Name = "button20";
            button20.Size = new System.Drawing.Size(70, 50);
            button20.TabIndex = 20;
            button20.Text = "Add diurnal";
            button20.UseVisualStyleBackColor = true;
            button20.Click += Button20_Click;
            // 
            // button19
            // 
            button19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button19.Location = new System.Drawing.Point(668, 152);
            button19.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button19.Name = "button19";
            button19.Size = new System.Drawing.Size(72, 50);
            button19.TabIndex = 19;
            button19.Text = "Add seasonal";
            button19.UseVisualStyleBackColor = true;
            button19.Click += Button19_Click;
            // 
            // label33
            // 
            label33.AutoSize = true;
            label33.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label33.Location = new System.Drawing.Point(735, 74);
            label33.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label33.Name = "label33";
            label33.Size = new System.Drawing.Size(130, 26);
            label33.TabIndex = 17;
            label33.Text = "Diurnal variation\r\nfor selected source group:";
            // 
            // label32
            // 
            label32.AutoSize = true;
            label32.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label32.Location = new System.Drawing.Point(735, 9);
            label32.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label32.Name = "label32";
            label32.Size = new System.Drawing.Size(130, 26);
            label32.TabIndex = 16;
            label32.Text = "Seasonal variation\r\nfor selected source group:";
            // 
            // comboBox2
            // 
            comboBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new System.Drawing.Point(735, 107);
            comboBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new System.Drawing.Size(177, 21);
            comboBox2.TabIndex = 15;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
            // 
            // comboBox1
            // 
            comboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new System.Drawing.Point(735, 43);
            comboBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new System.Drawing.Size(177, 21);
            comboBox1.TabIndex = 14;
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.SystemColors.Window;
            panel1.Location = new System.Drawing.Point(10, 9);
            panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(640, 346);
            panel1.TabIndex = 21;
            panel1.Paint += Panel1_Paint;
            // 
            // panel2
            // 
            panel2.BackColor = System.Drawing.SystemColors.Window;
            panel2.Location = new System.Drawing.Point(10, 367);
            panel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(640, 346);
            panel2.TabIndex = 22;
            panel2.Paint += Panel2_Paint;
            // 
            // button1
            // 
            button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button1.Location = new System.Drawing.Point(945, 43);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(70, 62);
            button1.TabIndex = 23;
            button1.Text = "&OK";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1_Click;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Hour";
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Modulation";
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(853, 226);
            textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(162, 23);
            textBox1.TabIndex = 26;
            textBox1.Visible = false;
            // 
            // textBox2
            // 
            textBox2.Location = new System.Drawing.Point(668, 226);
            textBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(162, 23);
            textBox2.TabIndex = 27;
            textBox2.Visible = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
            label1.Location = new System.Drawing.Point(668, 205);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(38, 13);
            label1.TabIndex = 28;
            label1.Text = "Name:";
            label1.Visible = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
            label2.Location = new System.Drawing.Point(853, 207);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(38, 13);
            label2.TabIndex = 29;
            label2.Text = "Name:";
            label2.Visible = false;
            // 
            // button2
            // 
            button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button2.Location = new System.Drawing.Point(945, 152);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(70, 50);
            button2.TabIndex = 30;
            button2.Text = "Remove diurnal";
            button2.UseVisualStyleBackColor = true;
            button2.Click += Button2_Click;
            // 
            // button3
            // 
            button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button3.Location = new System.Drawing.Point(758, 152);
            button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(72, 50);
            button3.TabIndex = 31;
            button3.Text = "Remove seasonal";
            button3.UseVisualStyleBackColor = true;
            button3.Click += Button3_Click;
            // 
            // button4
            // 
            button4.BackgroundImage = (System.Drawing.Image)resources.GetObject("button4.BackgroundImage");
            button4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            button4.Location = new System.Drawing.Point(668, 13);
            button4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(43, 42);
            button4.TabIndex = 0;
            toolTip1.SetToolTip(button4, "Copy figure to clipboard");
            button4.UseVisualStyleBackColor = true;
            button4.Click += Button4_Click;
            // 
            // button5
            // 
            button5.BackgroundImage = (System.Drawing.Image)resources.GetObject("button5.BackgroundImage");
            button5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            button5.Location = new System.Drawing.Point(668, 672);
            button5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(43, 42);
            button5.TabIndex = 32;
            toolTip1.SetToolTip(button5, "Copy figure to clipboard.");
            button5.UseVisualStyleBackColor = true;
            button5.Click += Button5_Click_1;
            // 
            // button6
            // 
            button6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button6.Location = new System.Drawing.Point(668, 615);
            button6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button6.Name = "button6";
            button6.Size = new System.Drawing.Size(162, 50);
            button6.TabIndex = 33;
            button6.Text = "Save";
            button6.UseVisualStyleBackColor = true;
            button6.Visible = false;
            button6.Click += Button6_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowDrop = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new System.Drawing.Point(668, 254);
            dataGridView1.Margin = new System.Windows.Forms.Padding(2);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowTemplate.Height = 24;
            dataGridView1.Size = new System.Drawing.Size(162, 343);
            dataGridView1.TabIndex = 34;
            dataGridView1.Visible = false;
            dataGridView1.CellEndEdit += DataGridView_CellEndEdit;
            dataGridView1.DataError += DataGridView1DataError;
            dataGridView1.KeyDown += DataGridView1KeyDown;
            // 
            // dataGridView2
            // 
            dataGridView2.AllowDrop = true;
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Location = new System.Drawing.Point(853, 254);
            dataGridView2.Margin = new System.Windows.Forms.Padding(2);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.RowTemplate.Height = 24;
            dataGridView2.Size = new System.Drawing.Size(162, 458);
            dataGridView2.TabIndex = 35;
            dataGridView2.Visible = false;
            dataGridView2.CellEndEdit += DataGridView_CellEndEdit;
            dataGridView2.DataError += DataGridView2DataError;
            dataGridView2.KeyDown += DataGridView2KeyDown;
            // 
            // Emissionvariation
            // 
            AcceptButton = button1;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1034, 725);
            Controls.Add(dataGridView2);
            Controls.Add(dataGridView1);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(button20);
            Controls.Add(button19);
            Controls.Add(label33);
            Controls.Add(label32);
            Controls.Add(comboBox2);
            Controls.Add(comboBox1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "Emissionvariation";
            Text = "Emissionvariation";
            FormClosing += EmissionvariationFormClosing;
            FormClosed += EmissionvariationFormClosed;
            Load += Emissionvariation_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.DataGridView dataGridView1;

        #endregion

        private System.Windows.Forms.Button button20;
        private System.Windows.Forms.Button button19;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
    }
}