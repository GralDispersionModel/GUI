namespace GralDomForms
{
    partial class VerticalProfile_Dynamic
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
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerticalProfile_Dynamic));
        	this.button1 = new System.Windows.Forms.Button();
        	this.button2 = new System.Windows.Forms.Button();
        	this.SuspendLayout();
        	// 
        	// button1
        	// 
        	this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.button1.Location = new System.Drawing.Point(222, 0);
        	this.button1.Margin = new System.Windows.Forms.Padding(2);
        	this.button1.Name = "button1";
        	this.button1.Size = new System.Drawing.Size(20, 19);
        	this.button1.TabIndex = 0;
        	this.button1.Text = "+";
        	this.button1.UseVisualStyleBackColor = true;
        	this.button1.Click += new System.EventHandler(this.button1_Click);
        	// 
        	// button2
        	// 
        	this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.button2.Location = new System.Drawing.Point(241, 0);
        	this.button2.Margin = new System.Windows.Forms.Padding(2);
        	this.button2.Name = "button2";
        	this.button2.Size = new System.Drawing.Size(19, 19);
        	this.button2.TabIndex = 1;
        	this.button2.Text = "-";
        	this.button2.UseVisualStyleBackColor = true;
        	this.button2.Click += new System.EventHandler(this.button2_Click);
        	// 
        	// VerticalProfile_Dynamic
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.ClientSize = new System.Drawing.Size(259, 354);
        	this.Controls.Add(this.button2);
        	this.Controls.Add(this.button1);
        	this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        	this.Margin = new System.Windows.Forms.Padding(2);
        	this.Name = "VerticalProfile_Dynamic";
        	this.Text = "Vertical Profile";
        	this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.VerticalProfile_DynamicFormClosed);
        	this.Load += new System.EventHandler(this.VerticalProfile_Load);
        	this.ResizeEnd += new System.EventHandler(this.VerticalProfile_DynamicResizeEnd);
        	this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}