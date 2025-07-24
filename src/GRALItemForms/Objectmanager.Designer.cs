namespace GralItemForms
{
    partial class Objectmanager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Objectmanager));
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            checkBox1 = new System.Windows.Forms.CheckBox();
            button5 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            listBox1 = new System.Windows.Forms.ListBox();
            button4 = new System.Windows.Forms.Button();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            button6 = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.BackgroundImage = (System.Drawing.Image)resources.GetObject("button1.BackgroundImage");
            button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            button1.Location = new System.Drawing.Point(14, 128);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(47, 46);
            button1.TabIndex = 6;
            toolTip1.SetToolTip(button1, "increase the display order of the selected item");
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1_Click;
            // 
            // button2
            // 
            button2.BackgroundImage = (System.Drawing.Image)resources.GetObject("button2.BackgroundImage");
            button2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            button2.Location = new System.Drawing.Point(14, 179);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(47, 46);
            button2.TabIndex = 7;
            toolTip1.SetToolTip(button2, "decrease the display order of the selected item");
            button2.UseVisualStyleBackColor = true;
            button2.Click += Button2_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new System.Drawing.Point(7, 80);
            checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(83, 19);
            checkBox1.TabIndex = 5;
            checkBox1.Text = "Show/hide";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += CheckBox1_CheckedChanged;
            // 
            // button5
            // 
            button5.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button5.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button5.Location = new System.Drawing.Point(327, 369);
            button5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(88, 27);
            button5.TabIndex = 4;
            button5.Text = "&Close";
            button5.UseVisualStyleBackColor = true;
            button5.Click += Button5_Click;
            // 
            // button3
            // 
            button3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button3.Location = new System.Drawing.Point(102, 369);
            button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(88, 27);
            button3.TabIndex = 2;
            button3.Text = "&Apply";
            toolTip1.SetToolTip(button3, "Apply the new drawing options");
            button3.UseVisualStyleBackColor = true;
            button3.Click += Button3_Click;
            // 
            // listBox1
            // 
            listBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            listBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            listBox1.Font = new System.Drawing.Font("Arial", 9F);
            listBox1.FormattingEnabled = true;
            listBox1.HorizontalScrollbar = true;
            listBox1.Location = new System.Drawing.Point(102, 8);
            listBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listBox1.Name = "listBox1";
            listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            listBox1.Size = new System.Drawing.Size(312, 308);
            listBox1.TabIndex = 1;
            toolTip1.SetToolTip(listBox1, "Double click for layout settings\r\nDelete key to remove an item\r\nSpace key to toggle show/hide");
            listBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
            listBox1.DoubleClick += ListBox1_DoubleClick;
            listBox1.KeyDown += ListBox1KeyDown;
            listBox1.PreviewKeyDown += ListBox1_PreviewKeyDown;
            // 
            // button4
            // 
            button4.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            button4.BackgroundImage = Gral.Properties.Resources.Rueckgaenging;
            button4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            button4.Location = new System.Drawing.Point(7, 369);
            button4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(29, 29);
            button4.TabIndex = 8;
            toolTip1.SetToolTip(button4, "Remove the selected items");
            button4.UseVisualStyleBackColor = true;
            button4.Visible = false;
            button4.Click += Button4_Click;
            // 
            // button6
            // 
            button6.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button6.Location = new System.Drawing.Point(215, 369);
            button6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button6.Name = "button6";
            button6.Size = new System.Drawing.Size(88, 27);
            button6.TabIndex = 3;
            button6.Text = "&Layout";
            toolTip1.SetToolTip(button6, "Open the layout manager");
            button6.UseVisualStyleBackColor = true;
            button6.Visible = false;
            button6.Click += Button6Click;
            // 
            // Objectmanager
            // 
            AcceptButton = button3;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = button5;
            ClientSize = new System.Drawing.Size(425, 404);
            ControlBox = false;
            Controls.Add(button4);
            Controls.Add(listBox1);
            Controls.Add(button6);
            Controls.Add(button3);
            Controls.Add(button5);
            Controls.Add(checkBox1);
            Controls.Add(button2);
            Controls.Add(button1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "Objectmanager";
            Text = "Objectmanager";
            TopMost = true;
            FormClosed += ObjectmanagerFormClosed;
            Load += Objectmanager_Load;
            ResumeLayout(false);
            PerformLayout();

        }
        private System.Windows.Forms.Button button6;

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ListBox listBox1;
    }
}