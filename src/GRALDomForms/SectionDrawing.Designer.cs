namespace GralDomForms
{
	partial class Sectiondrawing
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sectiondrawing));
            this.section_picture = new System.Windows.Forms.PictureBox();
            this.select_situation = new System.Windows.Forms.ListView();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.filterdissit = new System.Windows.Forms.CheckBox();
            this.vert_plus = new System.Windows.Forms.Button();
            this.vert_minus = new System.Windows.Forms.Button();
            this.section_clipboard = new System.Windows.Forms.Button();
            this.Show3D = new System.Windows.Forms.Button();
            this.show_gramm = new System.Windows.Forms.CheckBox();
            this.show_gral = new System.Windows.Forms.CheckBox();
            this.show_buildings = new System.Windows.Forms.CheckBox();
            this.show_windfied = new System.Windows.Forms.Button();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.wind_data_picturebox = new System.Windows.Forms.PictureBox();
            this.domainUpDown1 = new System.Windows.Forms.DomainUpDown();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.section_picture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.wind_data_picturebox)).BeginInit();
            this.SuspendLayout();
            // 
            // section_picture
            // 
            this.section_picture.BackColor = System.Drawing.SystemColors.Window;
            this.section_picture.Location = new System.Drawing.Point(1, 0);
            this.section_picture.Name = "section_picture";
            this.section_picture.Size = new System.Drawing.Size(1039, 513);
            this.section_picture.TabIndex = 0;
            this.section_picture.TabStop = false;
            this.section_picture.Paint += new System.Windows.Forms.PaintEventHandler(this.Section_picturePaint);
            // 
            // select_situation
            // 
            this.select_situation.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.select_situation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.select_situation.HotTracking = true;
            this.select_situation.HoverSelection = true;
            this.select_situation.Location = new System.Drawing.Point(12, 553);
            this.select_situation.MultiSelect = false;
            this.select_situation.Name = "select_situation";
            this.select_situation.Size = new System.Drawing.Size(468, 91);
            this.select_situation.TabIndex = 0;
            this.toolTip1.SetToolTip(this.select_situation, "Select a dispersion situation");
            this.select_situation.UseCompatibleStateImageBehavior = false;
            this.select_situation.View = System.Windows.Forms.View.Details;
            // 
            // filterdissit
            // 
            this.filterdissit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.filterdissit.Checked = true;
            this.filterdissit.CheckState = System.Windows.Forms.CheckState.Checked;
            this.filterdissit.Location = new System.Drawing.Point(738, 629);
            this.filterdissit.Name = "filterdissit";
            this.filterdissit.Size = new System.Drawing.Size(111, 24);
            this.filterdissit.TabIndex = 9;
            this.filterdissit.Text = "Filter Disp.Sit.";
            this.toolTip1.SetToolTip(this.filterdissit, "Filter the dispersion situation");
            this.filterdissit.UseVisualStyleBackColor = true;
            this.filterdissit.CheckedChanged += new System.EventHandler(this.FilterdissitCheckedChanged);
            // 
            // vert_plus
            // 
            this.vert_plus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.vert_plus.BackgroundImage = global::Gral.Properties.Resources.Lupe1;
            this.vert_plus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.vert_plus.Location = new System.Drawing.Point(1046, 560);
            this.vert_plus.Name = "vert_plus";
            this.vert_plus.Size = new System.Drawing.Size(32, 32);
            this.vert_plus.TabIndex = 2;
            this.toolTip1.SetToolTip(this.vert_plus, "zoom vertical");
            this.vert_plus.UseVisualStyleBackColor = true;
            this.vert_plus.Click += new System.EventHandler(this.Vert_plusClick);
            // 
            // vert_minus
            // 
            this.vert_minus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.vert_minus.BackgroundImage = global::Gral.Properties.Resources.Lupe_minus;
            this.vert_minus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.vert_minus.Location = new System.Drawing.Point(1046, 612);
            this.vert_minus.Name = "vert_minus";
            this.vert_minus.Size = new System.Drawing.Size(32, 32);
            this.vert_minus.TabIndex = 2;
            this.toolTip1.SetToolTip(this.vert_minus, "zoom vertical");
            this.vert_minus.UseVisualStyleBackColor = true;
            this.vert_minus.Click += new System.EventHandler(this.Vert_minusClick);
            // 
            // section_clipboard
            // 
            this.section_clipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.section_clipboard.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("section_clipboard.BackgroundImage")));
            this.section_clipboard.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.section_clipboard.Location = new System.Drawing.Point(908, 612);
            this.section_clipboard.Name = "section_clipboard";
            this.section_clipboard.Size = new System.Drawing.Size(32, 32);
            this.section_clipboard.TabIndex = 14;
            this.toolTip1.SetToolTip(this.section_clipboard, "Copy to Clipboard");
            this.section_clipboard.UseVisualStyleBackColor = true;
            this.section_clipboard.Click += new System.EventHandler(this.Section_clipboardClick);
            // 
            // Show3D
            // 
            this.Show3D.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Show3D.Location = new System.Drawing.Point(499, 591);
            this.Show3D.Name = "Show3D";
            this.Show3D.Size = new System.Drawing.Size(112, 32);
            this.Show3D.TabIndex = 15;
            this.Show3D.Text = "&3D View";
            this.toolTip1.SetToolTip(this.Show3D, "Show 3D view");
            this.Show3D.UseVisualStyleBackColor = true;
            this.Show3D.Click += new System.EventHandler(this.Show3DClick);
            // 
            // show_gramm
            // 
            this.show_gramm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.show_gramm.Checked = true;
            this.show_gramm.CheckState = System.Windows.Forms.CheckState.Checked;
            this.show_gramm.Location = new System.Drawing.Point(738, 554);
            this.show_gramm.Name = "show_gramm";
            this.show_gramm.Size = new System.Drawing.Size(166, 24);
            this.show_gramm.TabIndex = 3;
            this.show_gramm.Text = "Show GRAMM surface/wind";
            this.show_gramm.UseVisualStyleBackColor = true;
            this.show_gramm.CheckedChanged += new System.EventHandler(this.Show_grammCheckedChanged);
            // 
            // show_gral
            // 
            this.show_gral.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.show_gral.Location = new System.Drawing.Point(738, 579);
            this.show_gral.Name = "show_gral";
            this.show_gral.Size = new System.Drawing.Size(157, 24);
            this.show_gral.TabIndex = 3;
            this.show_gral.Text = "Show GRAL surface/wind";
            this.show_gral.UseVisualStyleBackColor = true;
            this.show_gral.CheckedChanged += new System.EventHandler(this.Show_gralCheckedChanged);
            // 
            // show_buildings
            // 
            this.show_buildings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.show_buildings.Location = new System.Drawing.Point(738, 604);
            this.show_buildings.Name = "show_buildings";
            this.show_buildings.Size = new System.Drawing.Size(123, 24);
            this.show_buildings.TabIndex = 3;
            this.show_buildings.Text = "Show buildings";
            this.show_buildings.UseVisualStyleBackColor = true;
            this.show_buildings.CheckedChanged += new System.EventHandler(this.Show_buildingsCheckedChanged);
            // 
            // show_windfied
            // 
            this.show_windfied.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.show_windfied.Location = new System.Drawing.Point(499, 552);
            this.show_windfied.Name = "show_windfied";
            this.show_windfied.Size = new System.Drawing.Size(112, 32);
            this.show_windfied.TabIndex = 4;
            this.show_windfied.Text = "&Show windfield";
            this.show_windfied.UseVisualStyleBackColor = true;
            this.show_windfied.Click += new System.EventHandler(this.Show_windfiedClick);
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Location = new System.Drawing.Point(1053, 0);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(25, 513);
            this.vScrollBar1.TabIndex = 5;
            this.vScrollBar1.ValueChanged += new System.EventHandler(this.VScrollBar1ValueChanged);
            // 
            // wind_data_picturebox
            // 
            this.wind_data_picturebox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.wind_data_picturebox.BackColor = System.Drawing.SystemColors.Window;
            this.wind_data_picturebox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.wind_data_picturebox.InitialImage = null;
            this.wind_data_picturebox.Location = new System.Drawing.Point(617, 554);
            this.wind_data_picturebox.Name = "wind_data_picturebox";
            this.wind_data_picturebox.Size = new System.Drawing.Size(115, 90);
            this.wind_data_picturebox.TabIndex = 8;
            this.wind_data_picturebox.TabStop = false;
            this.wind_data_picturebox.Paint += new System.Windows.Forms.PaintEventHandler(this.Wind_data_pictureboxPaint);
            // 
            // domainUpDown1
            // 
            this.domainUpDown1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.domainUpDown1.Location = new System.Drawing.Point(499, 629);
            this.domainUpDown1.Name = "domainUpDown1";
            this.domainUpDown1.ReadOnly = true;
            this.domainUpDown1.Size = new System.Drawing.Size(111, 20);
            this.domainUpDown1.TabIndex = 10;
            this.domainUpDown1.SelectedItemChanged += new System.EventHandler(this.DomainUpDown1SelectedItemChanged);
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.hScrollBar1.Location = new System.Drawing.Point(1, 516);
            this.hScrollBar1.Maximum = 1000;
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(1039, 20);
            this.hScrollBar1.TabIndex = 16;
            this.hScrollBar1.Value = 500;
            this.hScrollBar1.ValueChanged += new System.EventHandler(this.HScrollBar1ValueChanged);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.BackgroundImage = global::Gral.Properties.Resources.Lupe_minus;
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button1.Location = new System.Drawing.Point(1008, 612);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(32, 32);
            this.button1.TabIndex = 17;
            this.toolTip1.SetToolTip(this.button1, "zoom vector");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Vert_minusClick);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.BackgroundImage = global::Gral.Properties.Resources.Lupe1;
            this.button2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button2.Location = new System.Drawing.Point(1008, 560);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(32, 32);
            this.button2.TabIndex = 18;
            this.toolTip1.SetToolTip(this.button2, " zoom vector\r");
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Vert_plusClick);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.BackgroundImage = global::Gral.Properties.Resources.Lupe_minus;
            this.button3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button3.Location = new System.Drawing.Point(970, 612);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(32, 32);
            this.button3.TabIndex = 19;
            this.toolTip1.SetToolTip(this.button3, "zoom horizontal");
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Vert_minusClick);
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.BackgroundImage = global::Gral.Properties.Resources.Lupe1;
            this.button4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button4.Location = new System.Drawing.Point(970, 560);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(32, 32);
            this.button4.TabIndex = 20;
            this.toolTip1.SetToolTip(this.button4, "zoom horizontal");
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.Vert_plusClick);
            // 
            // Sectiondrawing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(1081, 656);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.hScrollBar1);
            this.Controls.Add(this.Show3D);
            this.Controls.Add(this.filterdissit);
            this.Controls.Add(this.section_clipboard);
            this.Controls.Add(this.domainUpDown1);
            this.Controls.Add(this.wind_data_picturebox);
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.show_windfied);
            this.Controls.Add(this.show_buildings);
            this.Controls.Add(this.show_gral);
            this.Controls.Add(this.show_gramm);
            this.Controls.Add(this.vert_minus);
            this.Controls.Add(this.vert_plus);
            this.Controls.Add(this.select_situation);
            this.Controls.Add(this.section_picture);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Sectiondrawing";
            this.Text = "Section drawing";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SectiondrawingFormClosed);
            this.Load += new System.EventHandler(this.SectiondrawingLoad);
            this.ResizeEnd += new System.EventHandler(this.SectiondrawingResizeEnd);
            this.Resize += new System.EventHandler(this.SectiondrawingResize);
            ((System.ComponentModel.ISupportInitialize)(this.section_picture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.wind_data_picturebox)).EndInit();
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.HScrollBar hScrollBar1;
		private System.Windows.Forms.Button Show3D;
		private System.Windows.Forms.Button section_clipboard;
		private System.Windows.Forms.DomainUpDown domainUpDown1;
		private System.Windows.Forms.CheckBox filterdissit;
		private System.Windows.Forms.PictureBox wind_data_picturebox;
		private System.Windows.Forms.VScrollBar vScrollBar1;
		private System.Windows.Forms.Button show_windfied;
		private System.Windows.Forms.CheckBox show_buildings;
		private System.Windows.Forms.CheckBox show_gral;
		private System.Windows.Forms.CheckBox show_gramm;
		private System.Windows.Forms.Button vert_minus;
		private System.Windows.Forms.Button vert_plus;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ListView select_situation;
		private System.Windows.Forms.PictureBox section_picture;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
    }
}
