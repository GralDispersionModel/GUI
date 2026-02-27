namespace GralMainForms
{
    partial class ShowEmissionTimeseries
    {
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary>
        /// Disposes resources used by the form.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowEmissionTimeseries));
            button4 = new System.Windows.Forms.Button();
            hScrollBar1 = new System.Windows.Forms.HScrollBar();
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // button4
            // 
            button4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button4.BackgroundImage = (System.Drawing.Image)resources.GetObject("button4.BackgroundImage");
            button4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            button4.Location = new System.Drawing.Point(850, 2);
            button4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(40, 37);
            button4.TabIndex = 1;
            button4.UseVisualStyleBackColor = true;
            button4.Click += Button4Click;
            // 
            // hScrollBar1
            // 
            hScrollBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            hScrollBar1.Location = new System.Drawing.Point(0, 539);
            hScrollBar1.Name = "hScrollBar1";
            hScrollBar1.Size = new System.Drawing.Size(892, 24);
            hScrollBar1.TabIndex = 2;
            hScrollBar1.Scroll += HScrollBar1Scroll;
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button2.BackColor = System.Drawing.SystemColors.Control;
            button2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button2.Location = new System.Drawing.Point(812, 2);
            button2.Margin = new System.Windows.Forms.Padding(2);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(37, 37);
            button2.TabIndex = 5;
            button2.Text = "-";
            button2.UseVisualStyleBackColor = false;
            button2.Click += Button2Click;
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button1.BackColor = System.Drawing.SystemColors.Control;
            button1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button1.Location = new System.Drawing.Point(771, 2);
            button1.Margin = new System.Windows.Forms.Padding(2);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(37, 37);
            button1.TabIndex = 4;
            button1.Text = "+";
            button1.UseVisualStyleBackColor = false;
            button1.Click += Button1Click;
            // 
            // ShowEmissionTimeseries
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(892, 563);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(hScrollBar1);
            Controls.Add(button4);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ShowEmissionTimeseries";
            Text = "Modulation file \"Emissions_timeseries.txt\"";
            FormClosed += Show_Emission_TimeseriesFormClosed;
            Load += ShowEmissionTimeseriesLoad;
            ResizeBegin += Show_Emission_TimeseriesResizeBegin;
            ResizeEnd += Show_Emission_TimeseriesResizeEnd;
            Resize += ShowEmissionTimeseriesResize;
            ResumeLayout(false);

        }
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.HScrollBar hScrollBar1;
        private System.Windows.Forms.Button button4;
    }
}
