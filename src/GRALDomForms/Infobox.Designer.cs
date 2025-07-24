namespace GralDomForms
{
    partial class Infobox
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
            listBox1 = new System.Windows.Forms.ListBox();
            button1 = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // listBox1
            // 
            listBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            listBox1.BackColor = System.Drawing.SystemColors.Window;
            listBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            listBox1.Enabled = false;
            listBox1.FormattingEnabled = true;
            listBox1.HorizontalScrollbar = true;
            listBox1.Location = new System.Drawing.Point(0, 0);
            listBox1.Margin = new System.Windows.Forms.Padding(2);
            listBox1.Name = "listBox1";
            listBox1.Size = new System.Drawing.Size(183, 135);
            listBox1.TabIndex = 0;
            listBox1.TabStop = false;
            listBox1.UseTabStops = false;
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button1.Location = new System.Drawing.Point(162, -2);
            button1.Margin = new System.Windows.Forms.Padding(2);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(21, 22);
            button1.TabIndex = 1;
            button1.Text = "X";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // Infobox
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            ClientSize = new System.Drawing.Size(180, 129);
            ControlBox = false;
            Controls.Add(button1);
            Controls.Add(listBox1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Margin = new System.Windows.Forms.Padding(2);
            Name = "Infobox";
            TopMost = true;
            Load += InfoboxLoad;
            MouseDown += MainFormMouseDown;
            MouseMove += MainFormMouseMove;
            ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button button1;


    }
}