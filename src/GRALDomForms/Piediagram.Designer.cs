namespace GralDomForms
{
    partial class Piediagram
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Piediagram));
            button22 = new System.Windows.Forms.Button();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            checkBox1 = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // button22
            // 
            button22.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button22.AutoSize = true;
            button22.BackColor = System.Drawing.SystemColors.Control;
            button22.BackgroundImage = (System.Drawing.Image)resources.GetObject("button22.BackgroundImage");
            button22.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            button22.ForeColor = System.Drawing.Color.Blue;
            button22.Location = new System.Drawing.Point(498, -1);
            button22.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button22.Name = "button22";
            button22.Size = new System.Drawing.Size(35, 35);
            button22.TabIndex = 31;
            button22.UseVisualStyleBackColor = false;
            button22.Click += button22_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = System.Drawing.Color.White;
            pictureBox1.Location = new System.Drawing.Point(0, 0);
            pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(534, 359);
            pictureBox1.TabIndex = 32;
            pictureBox1.TabStop = false;
            pictureBox1.Paint += PictureBox1Paint;
            // 
            // checkBox1
            // 
            checkBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            checkBox1.BackColor = System.Drawing.SystemColors.Control;
            checkBox1.Location = new System.Drawing.Point(359, 331);
            checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(174, 28);
            checkBox1.TabIndex = 33;
            checkBox1.Text = "Show all source groups";
            checkBox1.UseVisualStyleBackColor = false;
            checkBox1.CheckedChanged += CheckBox1CheckedChanged;
            // 
            // Piediagram
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(531, 358);
            Controls.Add(checkBox1);
            Controls.Add(button22);
            Controls.Add(pictureBox1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(2);
            Name = "Piediagram";
            Text = "Source apportionment";
            Load += Piediagram_Load;
            SizeChanged += PiediagramSizeChanged;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.PictureBox pictureBox1;

        #endregion

        public System.Windows.Forms.Button button22;
    }
}