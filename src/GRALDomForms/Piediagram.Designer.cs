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
        	this.button22 = new System.Windows.Forms.Button();
        	this.pictureBox1 = new System.Windows.Forms.PictureBox();
        	this.checkBox1 = new System.Windows.Forms.CheckBox();
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// button22
        	// 
        	this.button22.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.button22.AutoSize = true;
        	this.button22.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button22.BackgroundImage")));
        	this.button22.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
        	this.button22.ForeColor = System.Drawing.Color.Blue;
        	this.button22.Location = new System.Drawing.Point(427, -1);
        	this.button22.Name = "button22";
        	this.button22.Size = new System.Drawing.Size(30, 30);
        	this.button22.TabIndex = 31;
        	this.button22.UseVisualStyleBackColor = true;
        	this.button22.Click += new System.EventHandler(this.button22_Click);
        	// 
        	// pictureBox1
        	// 
        	this.pictureBox1.Location = new System.Drawing.Point(0, 0);
        	this.pictureBox1.Name = "pictureBox1";
        	this.pictureBox1.Size = new System.Drawing.Size(458, 311);
        	this.pictureBox1.TabIndex = 32;
        	this.pictureBox1.TabStop = false;
        	this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBox1Paint);
        	// 
        	// checkBox1
        	// 
        	this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.checkBox1.Location = new System.Drawing.Point(308, 287);
        	this.checkBox1.Name = "checkBox1";
        	this.checkBox1.Size = new System.Drawing.Size(149, 24);
        	this.checkBox1.TabIndex = 33;
        	this.checkBox1.Text = "Show all source groups";
        	this.checkBox1.UseVisualStyleBackColor = true;
        	this.checkBox1.CheckedChanged += new System.EventHandler(this.CheckBox1CheckedChanged);
        	// 
        	// Piediagram
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.ClientSize = new System.Drawing.Size(455, 310);
        	this.Controls.Add(this.checkBox1);
        	this.Controls.Add(this.button22);
        	this.Controls.Add(this.pictureBox1);
        	this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        	this.Margin = new System.Windows.Forms.Padding(2);
        	this.Name = "Piediagram";
        	this.Text = "Source apportionment";
        	this.Load += new System.EventHandler(this.Piediagram_Load);
        	this.SizeChanged += new System.EventHandler(this.PiediagramSizeChanged);
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.PictureBox pictureBox1;

        #endregion

        public System.Windows.Forms.Button button22;
    }
}