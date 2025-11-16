namespace GralDomForms
{
    partial class ViewFrameSaveAndLoad
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewFrameSaveAndLoad));
            button1 = new System.Windows.Forms.Button();
            listBox1 = new System.Windows.Forms.ListBox();
            label1 = new System.Windows.Forms.Label();
            textBox1 = new System.Windows.Forms.TextBox();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            button3 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            button1.Image = Gral.Properties.Resources.Open1;
            button1.Location = new System.Drawing.Point(340, 50);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(37, 37);
            button1.TabIndex = 2;
            toolTip1.SetToolTip(button1, "Load the selected viewframe");
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Location = new System.Drawing.Point(14, 50);
            listBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listBox1.Name = "listBox1";
            listBox1.Size = new System.Drawing.Size(318, 109);
            listBox1.TabIndex = 1;
            toolTip1.SetToolTip(listBox1, "Available viewframes");
            listBox1.DoubleClick += listBox1_DoubleClick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(14, 18);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(122, 15);
            label1.TabIndex = 2;
            label1.Text = "New viewframe name";
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(164, 15);
            textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(167, 23);
            textBox1.TabIndex = 4;
            toolTip1.SetToolTip(textBox1, "New viewframe");
            // 
            // button3
            // 
            button3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button3.Image = Gral.Properties.Resources.trashcan;
            button3.Location = new System.Drawing.Point(340, 122);
            button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(37, 37);
            button3.TabIndex = 3;
            toolTip1.SetToolTip(button3, "Delete the selected viewframe");
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            button4.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button4.Image = Gral.Properties.Resources.SaveSmall;
            button4.Location = new System.Drawing.Point(340, 7);
            button4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(37, 37);
            button4.TabIndex = 5;
            toolTip1.SetToolTip(button4, "Save a new viewframe");
            button4.UseVisualStyleBackColor = true;
            button4.Click += button1_Click;
            // 
            // ViewFrameSaveAndLoad
            // 
            AcceptButton = button4;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(400, 179);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(listBox1);
            Controls.Add(button1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ViewFrameSaveAndLoad";
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            Text = "Save or load a viewframe";
            Load += ViewFrameSaveAndLoad_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
    }
}