namespace GralMainForms
{
    partial class UpdateNotification
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateNotification));
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            linkLabel1 = new System.Windows.Forms.LinkLabel();
            label4 = new System.Windows.Forms.Label();
            linkLabel2 = new System.Windows.Forms.LinkLabel();
            button1 = new System.Windows.Forms.Button();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            label1.Location = new System.Drawing.Point(32, 22);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(116, 20);
            label1.TabIndex = 0;
            label1.Text = "Your version is V";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            label2.Location = new System.Drawing.Point(32, 53);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(116, 20);
            label2.TabIndex = 0;
            label2.Text = "Your version is V";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            label3.Location = new System.Drawing.Point(36, 97);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(235, 20);
            label3.TabIndex = 0;
            label3.Text = "Download link for the new version";
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            linkLabel1.Location = new System.Drawing.Point(36, 117);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new System.Drawing.Size(76, 20);
            linkLabel1.TabIndex = 1;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "linkLabel1";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            label4.Location = new System.Drawing.Point(36, 173);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(211, 20);
            label4.TabIndex = 0;
            label4.Text = "Changelog for the new version";
            // 
            // linkLabel2
            // 
            linkLabel2.AutoSize = true;
            linkLabel2.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            linkLabel2.Location = new System.Drawing.Point(36, 193);
            linkLabel2.Name = "linkLabel2";
            linkLabel2.Size = new System.Drawing.Size(76, 20);
            linkLabel2.TabIndex = 1;
            linkLabel2.TabStop = true;
            linkLabel2.Text = "linkLabel1";
            linkLabel2.LinkClicked += linkLabel2_LinkClicked;
            // 
            // button1
            // 
            button1.Font = new System.Drawing.Font("Segoe UI", 12F);
            button1.Location = new System.Drawing.Point(266, 251);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(110, 45);
            button1.TabIndex = 2;
            button1.Text = "&OK";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BackgroundImage = Gral.Properties.Resources.update;
            pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBox1.Location = new System.Drawing.Point(528, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(100, 100);
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // UpdateNotification
            // 
            AcceptButton = button1;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(645, 324);
            Controls.Add(pictureBox1);
            Controls.Add(button1);
            Controls.Add(linkLabel2);
            Controls.Add(linkLabel1);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UpdateNotification";
            Text = "GRAL Update Notification";
            Load += UpdateNotification_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}