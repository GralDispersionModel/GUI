namespace GralItemForms
{
    partial class EditReceptors
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
            this.components = new System.ComponentModel.Container();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.button4 = new System.Windows.Forms.Button();
            this.Exit = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.labelTitle = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ScrollLeft = new System.Windows.Forms.Button();
            this.ScrollRight = new System.Windows.Forms.Button();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 113);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Xpos. [m]";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(81, 110);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(106, 20);
            this.textBox2.TabIndex = 3;
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Paste);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(81, 135);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(106, 20);
            this.textBox3.TabIndex = 4;
            this.textBox3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Paste);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 138);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Ypos. [m]";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 164);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Receptor height [m]";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.DecimalPlaces = 1;
            this.numericUpDown1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDown1.Location = new System.Drawing.Point(126, 162);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(62, 20);
            this.numericUpDown1.TabIndex = 5;
            this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.numericUpDown1, "Receptor height\r\nabove ground level in [m]");
            // 
            // button1
            // 
            this.button1.Image = global::Gral.Properties.Resources.AddSmall1;
            this.button1.Location = new System.Drawing.Point(12, 260);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(32, 32);
            this.button1.TabIndex = 7;
            this.toolTip1.SetToolTip(this.button1, "Add one receptor");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Image = global::Gral.Properties.Resources.DeleteSmall;
            this.button2.Location = new System.Drawing.Point(108, 260);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(32, 32);
            this.button2.TabIndex = 9;
            this.toolTip1.SetToolTip(this.button2, "Remove one receptor");
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Image = global::Gral.Properties.Resources.TrashcanSmall;
            this.button3.Location = new System.Drawing.Point(156, 260);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(32, 32);
            this.button3.TabIndex = 10;
            this.toolTip1.SetToolTip(this.button3, "Remove all receptors");
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.DecimalPlaces = 3;
            this.numericUpDown2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numericUpDown2.Location = new System.Drawing.Point(127, 220);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown2.Minimum = new decimal(new int[] {
            1000000,
            0,
            0,
            -2147483648});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(62, 20);
            this.numericUpDown2.TabIndex = 6;
            this.numericUpDown2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.numericUpDown2, "Receptor height\r\nabove ground level in [m]");
            // 
            // button4
            // 
            this.button4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button4.Image = global::Gral.Properties.Resources.RestoreSmall1;
            this.button4.Location = new System.Drawing.Point(60, 260);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(32, 32);
            this.button4.TabIndex = 8;
            this.toolTip1.SetToolTip(this.button4, "Store & reload settings");
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.Button4Click);
            // 
            // Exit
            // 
            this.Exit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Exit.BackgroundImage = global::Gral.Properties.Resources.DeleteSmall;
            this.Exit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.Exit.Cursor = System.Windows.Forms.Cursors.Default;
            this.Exit.Location = new System.Drawing.Point(169, 3);
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(24, 24);
            this.Exit.TabIndex = 57;
            this.toolTip1.SetToolTip(this.Exit, "Close form");
            this.Exit.UseVisualStyleBackColor = true;
            this.Exit.Click += new System.EventHandler(this.cancelButtonClick);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(81, 77);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(106, 20);
            this.textBox1.TabIndex = 2;
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 42;
            this.label2.Text = "Name";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 222);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 44;
            this.label1.Text = "Value for display";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 202);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 13);
            this.label6.TabIndex = 46;
            this.label6.Text = "Optional:";
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(12, 312);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 24);
            this.button6.TabIndex = 77;
            this.button6.Text = "&OK";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            this.button5.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button5.Location = new System.Drawing.Point(112, 312);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 24);
            this.button5.TabIndex = 77;
            this.button5.Text = "&Cancel";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.cancelButtonClick);
            // 
            // labelTitle
            // 
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle.Location = new System.Drawing.Point(3, 3);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(153, 25);
            this.labelTitle.TabIndex = 56;
            this.labelTitle.Text = "Edit Walls";
            this.labelTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.panel1.Controls.Add(this.Exit);
            this.panel1.Controls.Add(this.labelTitle);
            this.panel1.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 34);
            this.panel1.TabIndex = 80;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            // 
            // ScrollLeft
            // 
            this.ScrollLeft.BackgroundImage = global::Gral.Properties.Resources.ArrowLeft;
            this.ScrollLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ScrollLeft.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ScrollLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ScrollLeft.Location = new System.Drawing.Point(5, 48);
            this.ScrollLeft.Name = "ScrollLeft";
            this.ScrollLeft.Size = new System.Drawing.Size(20, 20);
            this.ScrollLeft.TabIndex = 85;
            this.toolTip1.SetToolTip(this.ScrollLeft, "Prev. item");
            this.ScrollLeft.UseVisualStyleBackColor = true;
            this.ScrollLeft.Click += new System.EventHandler(this.ScrollLeft_Click);
            // 
            // ScrollRight
            // 
            this.ScrollRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ScrollRight.BackgroundImage = global::Gral.Properties.Resources.ArrowRight;
            this.ScrollRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ScrollRight.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ScrollRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ScrollRight.Location = new System.Drawing.Point(170, 48);
            this.ScrollRight.Name = "ScrollRight";
            this.ScrollRight.Size = new System.Drawing.Size(20, 20);
            this.ScrollRight.TabIndex = 84;
            this.toolTip1.SetToolTip(this.ScrollRight, "Next item");
            this.ScrollRight.UseVisualStyleBackColor = true;
            this.ScrollRight.Click += new System.EventHandler(this.ScrollRight_Click);
            // 
            // trackBar1
            // 
            this.trackBar1.AutoSize = false;
            this.trackBar1.Location = new System.Drawing.Point(25, 48);
            this.trackBar1.Maximum = 1;
            this.trackBar1.Minimum = 1;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(140, 19);
            this.trackBar1.TabIndex = 83;
            this.trackBar1.Value = 1;
            this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // EditReceptors
            // 
            this.AcceptButton = this.button6;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button5;
            this.ClientSize = new System.Drawing.Size(200, 348);
            this.ControlBox = false;
            this.Controls.Add(this.ScrollLeft);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.ScrollRight);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numericUpDown2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditReceptors";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditReceptors_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EditReceptorsFormClosed);
            this.Load += new System.EventHandler(this.EditReceptors_Load);
            this.ResizeEnd += new System.EventHandler(this.EditReceptorsResizeEnd);
            this.VisibleChanged += new System.EventHandler(this.EditReceptorsVisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Button button4;

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button Exit;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button ScrollLeft;
        private System.Windows.Forms.Button ScrollRight;
        private System.Windows.Forms.TrackBar trackBar1;
    }
}