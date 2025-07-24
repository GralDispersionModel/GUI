namespace GralMainForms
{
    partial class DiurnalWinddirections
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiurnalWinddirections));
            checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            button1 = new System.Windows.Forms.Button();
            panel1 = new System.Windows.Forms.Panel();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // checkedListBox1
            // 
            checkedListBox1.BackColor = System.Drawing.SystemColors.Control;
            checkedListBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            checkedListBox1.CheckOnClick = true;
            checkedListBox1.Font = new System.Drawing.Font("Arial", 8F);
            checkedListBox1.FormattingEnabled = true;
            checkedListBox1.Items.AddRange(new object[] { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" });
            checkedListBox1.Location = new System.Drawing.Point(763, 94);
            checkedListBox1.Name = "checkedListBox1";
            checkedListBox1.Size = new System.Drawing.Size(78, 315);
            checkedListBox1.TabIndex = 0;
            checkedListBox1.ThreeDCheckBoxes = true;
            checkedListBox1.SelectedIndexChanged += checkedListBox1_SelectedIndexChanged;
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button1.BackgroundImage = (System.Drawing.Image)resources.GetObject("button1.BackgroundImage");
            button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            button1.Location = new System.Drawing.Point(858, 459);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(37, 37);
            button1.TabIndex = 3;
            toolTip1.SetToolTip(button1, "Copy to Clipboard");
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // panel1
            // 
            panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panel1.Controls.Add(checkedListBox1);
            panel1.Location = new System.Drawing.Point(0, 1);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(852, 495);
            panel1.TabIndex = 4;
            panel1.Paint += panel1_Paint;
            // 
            // DiurnalWinddirections
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            BackColor = System.Drawing.SystemColors.Control;
            ClientSize = new System.Drawing.Size(900, 496);
            Controls.Add(button1);
            Controls.Add(panel1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "DiurnalWinddirections";
            Text = "Diurnal wind direction frequencies";
            FormClosed += DiurnalWinddirectionsFormClosed;
            Load += WindDirectionsFormLoad;
            ResizeEnd += DiurnalWinddirectionsResizeEnd;
            Resize += DiurnalWinddirectionsResize;
            panel1.ResumeLayout(false);
            ResumeLayout(false);

        }
        private System.Windows.Forms.ToolTip toolTip1;

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;
    }
}