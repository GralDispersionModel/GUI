namespace GralDomForms
{
    partial class Georeference2
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
            groupBox2 = new System.Windows.Forms.GroupBox();
            label6 = new System.Windows.Forms.Label();
            button1 = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            textBox1 = new System.Windows.Forms.TextBox();
            textBox2 = new System.Windows.Forms.TextBox();
            textBox3 = new System.Windows.Forms.TextBox();
            textBox4 = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            button3 = new System.Windows.Forms.Button();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            textBox8 = new System.Windows.Forms.TextBox();
            textBox9 = new System.Windows.Forms.TextBox();
            textBox7 = new System.Windows.Forms.TextBox();
            textBox6 = new System.Windows.Forms.TextBox();
            button2 = new System.Windows.Forms.Button();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(button1);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(textBox1);
            groupBox2.Controls.Add(textBox2);
            groupBox2.Controls.Add(textBox3);
            groupBox2.Controls.Add(textBox4);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(button3);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(textBox8);
            groupBox2.Controls.Add(textBox9);
            groupBox2.Controls.Add(textBox7);
            groupBox2.Controls.Add(textBox6);
            groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            groupBox2.Location = new System.Drawing.Point(13, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(202, 271);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Reference points";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
            label6.Location = new System.Drawing.Point(3, 150);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(73, 16);
            label6.TabIndex = 18;
            label6.Text = "Ref. point 2";
            // 
            // button1
            // 
            button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            button1.Image = Gral.Properties.Resources.Rueckgaenging;
            button1.Location = new System.Drawing.Point(176, 191);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(22, 22);
            button1.TabIndex = 16;
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 170);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(148, 16);
            label1.TabIndex = 15;
            label1.Text = "Actual map coordinates";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(4, 219);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(154, 16);
            label2.TabIndex = 14;
            label2.Text = "Natural coordinates (x, y)";
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(90, 240);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(80, 22);
            textBox1.TabIndex = 7;
            textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            textBox1.TextChanged += TextBox6TextChanged;
            // 
            // textBox2
            // 
            textBox2.Location = new System.Drawing.Point(6, 240);
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(80, 22);
            textBox2.TabIndex = 5;
            textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            textBox2.TextChanged += TextBox6TextChanged;
            // 
            // textBox3
            // 
            textBox3.Location = new System.Drawing.Point(90, 191);
            textBox3.Name = "textBox3";
            textBox3.ReadOnly = true;
            textBox3.Size = new System.Drawing.Size(80, 22);
            textBox3.TabIndex = 11;
            textBox3.TabStop = false;
            textBox3.Text = "0";
            textBox3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBox4
            // 
            textBox4.Location = new System.Drawing.Point(6, 191);
            textBox4.Name = "textBox4";
            textBox4.ReadOnly = true;
            textBox4.Size = new System.Drawing.Size(80, 22);
            textBox4.TabIndex = 10;
            textBox4.TabStop = false;
            textBox4.Text = "0";
            textBox4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
            label3.Location = new System.Drawing.Point(4, 22);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(73, 16);
            label3.TabIndex = 17;
            label3.Text = "Ref. point 1";
            // 
            // button3
            // 
            button3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            button3.Image = Gral.Properties.Resources.Rueckgaenging;
            button3.Location = new System.Drawing.Point(174, 62);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(22, 22);
            button3.TabIndex = 9;
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(3, 40);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(148, 16);
            label5.TabIndex = 5;
            label5.Text = "Actual map coordinates";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(2, 89);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(154, 16);
            label4.TabIndex = 4;
            label4.Text = "Natural coordinates (x, y)";
            // 
            // textBox8
            // 
            textBox8.Location = new System.Drawing.Point(87, 110);
            textBox8.Name = "textBox8";
            textBox8.Size = new System.Drawing.Size(80, 22);
            textBox8.TabIndex = 3;
            textBox8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            textBox8.TextChanged += TextBox6TextChanged;
            // 
            // textBox9
            // 
            textBox9.Location = new System.Drawing.Point(4, 110);
            textBox9.Name = "textBox9";
            textBox9.Size = new System.Drawing.Size(80, 22);
            textBox9.TabIndex = 1;
            textBox9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            textBox9.TextChanged += TextBox6TextChanged;
            // 
            // textBox7
            // 
            textBox7.Location = new System.Drawing.Point(87, 62);
            textBox7.Name = "textBox7";
            textBox7.ReadOnly = true;
            textBox7.Size = new System.Drawing.Size(80, 22);
            textBox7.TabIndex = 1;
            textBox7.TabStop = false;
            textBox7.Text = "0";
            textBox7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBox6
            // 
            textBox6.Location = new System.Drawing.Point(4, 62);
            textBox6.Name = "textBox6";
            textBox6.ReadOnly = true;
            textBox6.Size = new System.Drawing.Size(80, 22);
            textBox6.TabIndex = 0;
            textBox6.TabStop = false;
            textBox6.Text = "0";
            textBox6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button2
            // 
            button2.DialogResult = System.Windows.Forms.DialogResult.OK;
            button2.Location = new System.Drawing.Point(13, 291);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(202, 32);
            button2.TabIndex = 20;
            button2.Text = "&OK";
            button2.UseVisualStyleBackColor = true;
            button2.Click += Button2Click;
            // 
            // Georeference2
            // 
            AcceptButton = button2;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            AutoScroll = true;
            ClientSize = new System.Drawing.Size(228, 332);
            ControlBox = false;
            Controls.Add(button2);
            Controls.Add(groupBox2);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Georeference2";
            Text = "Georeferencing";
            FormClosed += Georeference2FormClosed;
            Load += Georeference2Load;
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);

        }
        private System.Windows.Forms.Button button2;

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.TextBox textBox7;
        public System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox textBox8;
        public System.Windows.Forms.TextBox textBox9;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox textBox1;
        public System.Windows.Forms.TextBox textBox2;
        public System.Windows.Forms.TextBox textBox3;
        public System.Windows.Forms.TextBox textBox4;
    }
}