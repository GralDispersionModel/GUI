namespace GralMainForms
{
    partial class Stabilityclasses
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Stabilityclasses));
            button1 = new System.Windows.Forms.Button();
            panel1 = new System.Windows.Forms.Panel();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button1.BackgroundImage = (System.Drawing.Image)resources.GetObject("button1.BackgroundImage");
            button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            button1.Location = new System.Drawing.Point(788, 438);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(36, 37);
            button1.TabIndex = 1;
            toolTip1.SetToolTip(button1, "Copy to the clipoard");
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // panel1
            // 
            panel1.Location = new System.Drawing.Point(0, 2);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(784, 473);
            panel1.TabIndex = 2;
            panel1.Paint += panel1_Paint;
            panel1.MouseDown += panel1_MouseDown;
            panel1.MouseMove += panel1_MouseMove;
            panel1.MouseUp += panel1_MouseUp;
            // 
            // Stabilityclasses
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            BackColor = System.Drawing.SystemColors.Control;
            ClientSize = new System.Drawing.Size(825, 476);
            Controls.Add(button1);
            Controls.Add(panel1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "Stabilityclasses";
            Text = "Frequency distribution stability classes";
            FormClosed += StabilityclassesFormClosed;
            Load += Form5_Load;
            ResizeEnd += StabilityclassesResizeEnd;
            Resize += StabilityclassesResize;
            ResumeLayout(false);

        }
        private System.Windows.Forms.ToolTip toolTip1;

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;

        
    }
}