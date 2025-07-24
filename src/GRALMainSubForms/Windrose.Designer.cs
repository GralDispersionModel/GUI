namespace GralMainForms
{
    partial class Windrose
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Windrose));
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            label10 = new System.Windows.Forms.Label();
            button4 = new System.Windows.Forms.Button();
            label9 = new System.Windows.Forms.Label();
            button5 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            button6 = new System.Windows.Forms.Button();
            button7 = new System.Windows.Forms.Button();
            button8 = new System.Windows.Forms.Button();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button1.BackColor = System.Drawing.SystemColors.Control;
            button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button1.Location = new System.Drawing.Point(784, 110);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(36, 33);
            button1.TabIndex = 10;
            button1.Text = "+";
            toolTip1.SetToolTip(button1, "Enlarge the Rose Key: Shift and +");
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button2.BackColor = System.Drawing.SystemColors.Control;
            button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button2.Location = new System.Drawing.Point(784, 170);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(36, 33);
            button2.TabIndex = 11;
            button2.Text = "-";
            toolTip1.SetToolTip(button2, "Shrink the Rose Key: Shift and -");
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // label10
            // 
            label10.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(784, 314);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(34, 15);
            label10.TabIndex = 20;
            label10.Text = "Scale";
            // 
            // button4
            // 
            button4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button4.BackColor = System.Drawing.SystemColors.Control;
            button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button4.Location = new System.Drawing.Point(784, 334);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(36, 33);
            button4.TabIndex = 19;
            button4.Text = "-";
            toolTip1.SetToolTip(button4, "Decrease the Scale Key: -");
            button4.UseVisualStyleBackColor = false;
            button4.Click += button4_Click;
            // 
            // label9
            // 
            label9.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(786, 150);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(39, 15);
            label9.TabIndex = 17;
            label9.Text = "Zoom";
            // 
            // button5
            // 
            button5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button5.BackColor = System.Drawing.SystemColors.Control;
            button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button5.Location = new System.Drawing.Point(784, 274);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(36, 33);
            button5.TabIndex = 18;
            button5.Text = "+";
            toolTip1.SetToolTip(button5, "Increase the Scale Key: +");
            button5.UseVisualStyleBackColor = false;
            button5.Click += button5_Click;
            // 
            // button3
            // 
            button3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button3.BackColor = System.Drawing.SystemColors.Control;
            button3.BackgroundImage = (System.Drawing.Image)resources.GetObject("button3.BackgroundImage");
            button3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            button3.Location = new System.Drawing.Point(784, 4);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(36, 37);
            button3.TabIndex = 13;
            toolTip1.SetToolTip(button3, "Copy to Clipboard");
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // button6
            // 
            button6.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button6.BackColor = System.Drawing.SystemColors.Control;
            button6.BackgroundImage = (System.Drawing.Image)resources.GetObject("button6.BackgroundImage");
            button6.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            button6.Location = new System.Drawing.Point(784, 47);
            button6.Name = "button6";
            button6.Size = new System.Drawing.Size(36, 37);
            button6.TabIndex = 23;
            toolTip1.SetToolTip(button6, "Set the Font");
            button6.UseVisualStyleBackColor = false;
            button6.Click += Button6Click;
            // 
            // button7
            // 
            button7.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button7.BackColor = System.Drawing.SystemColors.Control;
            button7.BackgroundImage = Gral.Properties.Resources.Pin;
            button7.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            button7.Location = new System.Drawing.Point(784, 384);
            button7.Name = "button7";
            button7.Size = new System.Drawing.Size(36, 37);
            button7.TabIndex = 24;
            toolTip1.SetToolTip(button7, "Pin the scale");
            button7.UseVisualStyleBackColor = false;
            button7.Click += Button7Click;
            // 
            // button8
            // 
            button8.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button8.BackColor = System.Drawing.SystemColors.Control;
            button8.BackgroundImage = (System.Drawing.Image)resources.GetObject("button8.BackgroundImage");
            button8.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            button8.Location = new System.Drawing.Point(784, 441);
            button8.Name = "button8";
            button8.Size = new System.Drawing.Size(36, 37);
            button8.TabIndex = 25;
            toolTip1.SetToolTip(button8, "Show the data table");
            button8.UseVisualStyleBackColor = false;
            button8.Click += button8_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new System.Drawing.Point(1, 1);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(780, 608);
            pictureBox1.TabIndex = 22;
            pictureBox1.TabStop = false;
            pictureBox1.Paint += PictureBox1Paint;
            pictureBox1.MouseDown += pictureBox1_MouseDown;
            pictureBox1.MouseMove += pictureBox1_MouseMove;
            pictureBox1.MouseUp += pictureBox1_MouseUp;
            // 
            // Windrose
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            BackColor = System.Drawing.SystemColors.Control;
            ClientSize = new System.Drawing.Size(832, 611);
            Controls.Add(button8);
            Controls.Add(button7);
            Controls.Add(button6);
            Controls.Add(pictureBox1);
            Controls.Add(label10);
            Controls.Add(button3);
            Controls.Add(button4);
            Controls.Add(button2);
            Controls.Add(button5);
            Controls.Add(label9);
            Controls.Add(button1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "Windrose";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Windrose";
            FormClosed += WindroseFormClosed;
            Load += Form3_Load;
            ResizeEnd += WindroseResizeEnd;
            Resize += WindroseResize;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.PictureBox pictureBox1;

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button8;
    }
}