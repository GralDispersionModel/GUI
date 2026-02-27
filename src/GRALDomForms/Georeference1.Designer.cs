namespace GralDomForms
{
    partial class Georeference1
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
            groupBox1 = new System.Windows.Forms.GroupBox();
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            textBox5 = new System.Windows.Forms.TextBox();
            textBox4 = new System.Windows.Forms.TextBox();
            textBox3 = new System.Windows.Forms.TextBox();
            textBox2 = new System.Windows.Forms.TextBox();
            textBox1 = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            button3 = new System.Windows.Forms.Button();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            textBox8 = new System.Windows.Forms.TextBox();
            textBox9 = new System.Windows.Forms.TextBox();
            textBox7 = new System.Windows.Forms.TextBox();
            textBox6 = new System.Windows.Forms.TextBox();
            button4 = new System.Windows.Forms.Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button2);
            groupBox1.Controls.Add(button1);
            groupBox1.Controls.Add(textBox5);
            groupBox1.Controls.Add(textBox4);
            groupBox1.Controls.Add(textBox3);
            groupBox1.Controls.Add(textBox2);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            groupBox1.Location = new System.Drawing.Point(14, 13);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(202, 191);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Define scale";
            // 
            // button2
            // 
            button2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            button2.Image = Gral.Properties.Resources.Rueckgaenging;
            button2.Location = new System.Drawing.Point(174, 80);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(22, 22);
            button2.TabIndex = 9;
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            button1.Image = Gral.Properties.Resources.Rueckgaenging;
            button1.Location = new System.Drawing.Point(175, 37);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(22, 22);
            button1.TabIndex = 8;
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox5
            // 
            textBox5.Location = new System.Drawing.Point(3, 123);
            textBox5.Name = "textBox5";
            textBox5.Size = new System.Drawing.Size(106, 22);
            textBox5.TabIndex = 1;
            textBox5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            textBox5.Visible = false;
            textBox5.TextChanged += textBox5_TextChanged;
            // 
            // textBox4
            // 
            textBox4.Location = new System.Drawing.Point(89, 80);
            textBox4.Name = "textBox4";
            textBox4.ReadOnly = true;
            textBox4.Size = new System.Drawing.Size(80, 22);
            textBox4.TabIndex = 6;
            textBox4.TabStop = false;
            textBox4.Text = "0";
            textBox4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            textBox4.Visible = false;
            textBox4.TextChanged += TextBox1TextChanged;
            // 
            // textBox3
            // 
            textBox3.Location = new System.Drawing.Point(6, 80);
            textBox3.Name = "textBox3";
            textBox3.ReadOnly = true;
            textBox3.Size = new System.Drawing.Size(80, 22);
            textBox3.TabIndex = 5;
            textBox3.TabStop = false;
            textBox3.Text = "0";
            textBox3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            textBox3.Visible = false;
            textBox3.TextChanged += TextBox1TextChanged;
            // 
            // textBox2
            // 
            textBox2.Location = new System.Drawing.Point(89, 37);
            textBox2.Name = "textBox2";
            textBox2.ReadOnly = true;
            textBox2.Size = new System.Drawing.Size(80, 22);
            textBox2.TabIndex = 4;
            textBox2.TabStop = false;
            textBox2.Text = "0";
            textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            textBox2.TextChanged += TextBox1TextChanged;
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(6, 37);
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new System.Drawing.Size(80, 22);
            textBox1.TabIndex = 3;
            textBox1.TabStop = false;
            textBox1.Text = "0";
            textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            textBox1.TextChanged += TextBox1TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(2, 104);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(126, 16);
            label3.TabIndex = 2;
            label3.Text = "Natural distance [m]";
            label3.Visible = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(2, 61);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(62, 16);
            label2.TabIndex = 1;
            label2.Text = "2nd Point";
            label2.Visible = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(2, 18);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(57, 16);
            label1.TabIndex = 0;
            label1.Text = "1st Point";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(button3);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(textBox8);
            groupBox2.Controls.Add(textBox9);
            groupBox2.Controls.Add(textBox7);
            groupBox2.Controls.Add(textBox6);
            groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            groupBox2.Location = new System.Drawing.Point(14, 162);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(202, 138);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Reference point";
            groupBox2.Visible = false;
            // 
            // button3
            // 
            button3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            button3.Image = Gral.Properties.Resources.Rueckgaenging;
            button3.Location = new System.Drawing.Point(174, 44);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(22, 22);
            button3.TabIndex = 10;
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(3, 22);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(148, 16);
            label5.TabIndex = 5;
            label5.Text = "Actual map coordinates";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(2, 78);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(154, 16);
            label4.TabIndex = 4;
            label4.Text = "Natural coordinates (x, y)";
            // 
            // textBox8
            // 
            textBox8.Location = new System.Drawing.Point(87, 99);
            textBox8.Name = "textBox8";
            textBox8.Size = new System.Drawing.Size(80, 22);
            textBox8.TabIndex = 3;
            textBox8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            textBox8.TextChanged += TextBox1TextChanged;
            // 
            // textBox9
            // 
            textBox9.Location = new System.Drawing.Point(4, 99);
            textBox9.Name = "textBox9";
            textBox9.Size = new System.Drawing.Size(80, 22);
            textBox9.TabIndex = 2;
            textBox9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            textBox9.TextChanged += TextBox1TextChanged;
            // 
            // textBox7
            // 
            textBox7.Location = new System.Drawing.Point(87, 44);
            textBox7.Name = "textBox7";
            textBox7.ReadOnly = true;
            textBox7.Size = new System.Drawing.Size(80, 22);
            textBox7.TabIndex = 1;
            textBox7.TabStop = false;
            textBox7.Text = "0";
            textBox7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            textBox7.TextChanged += TextBox1TextChanged;
            // 
            // textBox6
            // 
            textBox6.Location = new System.Drawing.Point(4, 44);
            textBox6.Name = "textBox6";
            textBox6.ReadOnly = true;
            textBox6.Size = new System.Drawing.Size(80, 22);
            textBox6.TabIndex = 0;
            textBox6.TabStop = false;
            textBox6.Text = "0";
            textBox6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            textBox6.TextChanged += TextBox1TextChanged;
            // 
            // button4
            // 
            button4.DialogResult = System.Windows.Forms.DialogResult.OK;
            button4.Location = new System.Drawing.Point(13, 307);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(203, 32);
            button4.TabIndex = 11;
            button4.Text = "&OK";
            button4.UseVisualStyleBackColor = true;
            button4.Click += Button4Click;
            // 
            // Georeference1
            // 
            AcceptButton = button4;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            AutoScroll = true;
            ClientSize = new System.Drawing.Size(228, 358);
            ControlBox = false;
            Controls.Add(button4);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Georeference1";
            Text = "Georeferencing";
            FormClosed += Georeference1FormClosed;
            Load += Georeference1_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);

        }
        private System.Windows.Forms.Button button4;

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox textBox2;
        public System.Windows.Forms.TextBox textBox5;
        public System.Windows.Forms.TextBox textBox4;
        public System.Windows.Forms.TextBox textBox3;
        public System.Windows.Forms.TextBox textBox7;
        public System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox textBox8;
        public System.Windows.Forms.TextBox textBox9;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}