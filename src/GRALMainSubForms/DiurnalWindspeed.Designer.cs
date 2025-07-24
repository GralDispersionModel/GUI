namespace GralMainForms
{
    partial class DiurnalWindspeed
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiurnalWindspeed));
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
            button1.Location = new System.Drawing.Point(725, 439);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(37, 37);
            button1.TabIndex = 2;
            toolTip1.SetToolTip(button1, "Copy to Clipboard");
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // panel1
            // 
            panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panel1.Location = new System.Drawing.Point(0, 1);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(719, 475);
            panel1.TabIndex = 3;
            panel1.Paint += panel1_Paint;
            // 
            // DiurnalWindspeed
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            BackColor = System.Drawing.SystemColors.Control;
            ClientSize = new System.Drawing.Size(762, 476);
            Controls.Add(button1);
            Controls.Add(panel1);
            Name = "DiurnalWindspeed";
            ShowIcon = false;
            Text = "Diurnal mean wind speed";
            FormClosed += DiurnalWindspeedFormClosed;
            Load += Form6_Load;
            ResizeEnd += DiurnalWindspeedResizeEnd;
            Resize += DiurnalWindspeedResize;
            ResumeLayout(false);
        }
        private System.Windows.Forms.ToolTip toolTip1;

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;
    }
}