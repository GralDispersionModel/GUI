namespace GralDomForms
{
	partial class InputCoordinates
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
            this.input_ok = new System.Windows.Forms.Button();
            this.Input_y = new System.Windows.Forms.TextBox();
            this.Input_x = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // input_ok
            // 
            this.input_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.input_ok.Location = new System.Drawing.Point(35, 75);
            this.input_ok.Name = "input_ok";
            this.input_ok.Size = new System.Drawing.Size(104, 25);
            this.input_ok.TabIndex = 3;
            this.input_ok.Text = "&OK";
            this.input_ok.UseVisualStyleBackColor = true;
            this.input_ok.Click += new System.EventHandler(this.InputOkClick);
            // 
            // Input_y
            // 
            this.Input_y.Location = new System.Drawing.Point(41, 38);
            this.Input_y.Name = "Input_y";
            this.Input_y.Size = new System.Drawing.Size(104, 20);
            this.Input_y.TabIndex = 2;
            this.Input_y.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.Input_y.TextChanged += new System.EventHandler(this.InputYTextChanged);
            this.Input_y.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.InputYKeyPress);
            // 
            // Input_x
            // 
            this.Input_x.Location = new System.Drawing.Point(41, 12);
            this.Input_x.Name = "Input_x";
            this.Input_x.Size = new System.Drawing.Size(104, 20);
            this.Input_x.TabIndex = 1;
            this.Input_x.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.Input_x.TextChanged += new System.EventHandler(this.InputXTextChanged);
            this.Input_x.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.InputXKeyPress);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "X";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "Y";
            // 
            // InputCoordinates
            // 
            this.AcceptButton = this.input_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(168, 113);
            this.ControlBox = false;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Input_x);
            this.Controls.Add(this.Input_y);
            this.Controls.Add(this.input_ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "InputCoordinates";
            this.Text = "Input coordinates";
            this.Load += new System.EventHandler(this.InputCoordinatesLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.TextBox Input_x;
		public System.Windows.Forms.TextBox Input_y;
		private System.Windows.Forms.Button input_ok;
	}
}
