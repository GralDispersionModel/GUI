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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sectiondrawing));
            section_picture = new System.Windows.Forms.PictureBox();
            select_situation = new System.Windows.Forms.ListView();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            filterdissit = new System.Windows.Forms.CheckBox();
            vert_plus = new System.Windows.Forms.Button();
            vert_minus = new System.Windows.Forms.Button();
            section_clipboard = new System.Windows.Forms.Button();
            Show3D = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            show_gramm = new System.Windows.Forms.CheckBox();
            show_gral = new System.Windows.Forms.CheckBox();
            show_buildings = new System.Windows.Forms.CheckBox();
            show_windfied = new System.Windows.Forms.Button();
            vScrollBar1 = new System.Windows.Forms.VScrollBar();
            wind_data_picturebox = new System.Windows.Forms.PictureBox();
            domainUpDown1 = new System.Windows.Forms.DomainUpDown();
            hScrollBar1 = new System.Windows.Forms.HScrollBar();
            ((System.ComponentModel.ISupportInitialize)section_picture).BeginInit();
            ((System.ComponentModel.ISupportInitialize)wind_data_picturebox).BeginInit();
            SuspendLayout();
            // 
            // section_picture
            // 
            section_picture.BackColor = System.Drawing.SystemColors.Window;
            section_picture.Location = new System.Drawing.Point(1, 0);
            section_picture.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            section_picture.Name = "section_picture";
            section_picture.Size = new System.Drawing.Size(1212, 592);
            section_picture.TabIndex = 0;
            section_picture.TabStop = false;
            section_picture.Paint += Section_picturePaint;
            // 
            // select_situation
            // 
            select_situation.Activation = System.Windows.Forms.ItemActivation.OneClick;
            select_situation.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            select_situation.HotTracking = true;
            select_situation.HoverSelection = true;
            select_situation.Location = new System.Drawing.Point(14, 638);
            select_situation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            select_situation.MultiSelect = false;
            select_situation.Name = "select_situation";
            select_situation.Size = new System.Drawing.Size(545, 104);
            select_situation.TabIndex = 0;
            toolTip1.SetToolTip(select_situation, "Select a dispersion situation");
            select_situation.UseCompatibleStateImageBehavior = false;
            select_situation.View = System.Windows.Forms.View.Details;
            // 
            // filterdissit
            // 
            filterdissit.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            filterdissit.Checked = true;
            filterdissit.CheckState = System.Windows.Forms.CheckState.Checked;
            filterdissit.Location = new System.Drawing.Point(861, 726);
            filterdissit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            filterdissit.Name = "filterdissit";
            filterdissit.Size = new System.Drawing.Size(130, 28);
            filterdissit.TabIndex = 9;
            filterdissit.Text = "Filter Disp.Sit.";
            toolTip1.SetToolTip(filterdissit, "Filter the dispersion situation");
            filterdissit.UseVisualStyleBackColor = true;
            filterdissit.CheckedChanged += FilterdissitCheckedChanged;
            // 
            // vert_plus
            // 
            vert_plus.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            vert_plus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            vert_plus.Image = Gral.Properties.Resources.Lupe1;
            vert_plus.Location = new System.Drawing.Point(1220, 646);
            vert_plus.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            vert_plus.Name = "vert_plus";
            vert_plus.Size = new System.Drawing.Size(37, 37);
            vert_plus.TabIndex = 2;
            toolTip1.SetToolTip(vert_plus, "zoom vertical");
            vert_plus.UseVisualStyleBackColor = true;
            vert_plus.Click += Vert_plusClick;
            // 
            // vert_minus
            // 
            vert_minus.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            vert_minus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            vert_minus.Image = Gral.Properties.Resources.Lupe_minus;
            vert_minus.Location = new System.Drawing.Point(1220, 706);
            vert_minus.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            vert_minus.Name = "vert_minus";
            vert_minus.Size = new System.Drawing.Size(37, 37);
            vert_minus.TabIndex = 2;
            toolTip1.SetToolTip(vert_minus, "zoom vertical");
            vert_minus.UseVisualStyleBackColor = true;
            vert_minus.Click += Vert_minusClick;
            // 
            // section_clipboard
            // 
            section_clipboard.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            section_clipboard.BackgroundImage = (System.Drawing.Image)resources.GetObject("section_clipboard.BackgroundImage");
            section_clipboard.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            section_clipboard.Location = new System.Drawing.Point(1059, 706);
            section_clipboard.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            section_clipboard.Name = "section_clipboard";
            section_clipboard.Size = new System.Drawing.Size(37, 37);
            section_clipboard.TabIndex = 14;
            toolTip1.SetToolTip(section_clipboard, "Copy to Clipboard");
            section_clipboard.UseVisualStyleBackColor = true;
            section_clipboard.Click += Section_clipboardClick;
            // 
            // Show3D
            // 
            Show3D.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            Show3D.Location = new System.Drawing.Point(582, 682);
            Show3D.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Show3D.Name = "Show3D";
            Show3D.Size = new System.Drawing.Size(131, 37);
            Show3D.TabIndex = 15;
            Show3D.Text = "&3D View";
            toolTip1.SetToolTip(Show3D, "Show 3D view");
            Show3D.UseVisualStyleBackColor = true;
            Show3D.Click += Show3DClick;
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            button1.Image = Gral.Properties.Resources.Lupe_minus;
            button1.Location = new System.Drawing.Point(1176, 706);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(37, 37);
            button1.TabIndex = 17;
            toolTip1.SetToolTip(button1, "zoom vector");
            button1.UseVisualStyleBackColor = true;
            button1.Click += Vert_minusClick;
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            button2.Image = Gral.Properties.Resources.Lupe1;
            button2.Location = new System.Drawing.Point(1176, 646);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(37, 37);
            button2.TabIndex = 18;
            toolTip1.SetToolTip(button2, " zoom vector\r");
            button2.UseVisualStyleBackColor = true;
            button2.Click += Vert_plusClick;
            // 
            // button3
            // 
            button3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            button3.Image = Gral.Properties.Resources.Lupe_minus;
            button3.Location = new System.Drawing.Point(1132, 706);
            button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(37, 37);
            button3.TabIndex = 19;
            toolTip1.SetToolTip(button3, "zoom horizontal");
            button3.UseVisualStyleBackColor = true;
            button3.Click += Vert_minusClick;
            // 
            // button4
            // 
            button4.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            button4.Image = Gral.Properties.Resources.Lupe1;
            button4.Location = new System.Drawing.Point(1132, 646);
            button4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(37, 37);
            button4.TabIndex = 20;
            toolTip1.SetToolTip(button4, "zoom horizontal");
            button4.UseVisualStyleBackColor = true;
            button4.Click += Vert_plusClick;
            // 
            // show_gramm
            // 
            show_gramm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            show_gramm.Checked = true;
            show_gramm.CheckState = System.Windows.Forms.CheckState.Checked;
            show_gramm.Location = new System.Drawing.Point(861, 639);
            show_gramm.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            show_gramm.Name = "show_gramm";
            show_gramm.Size = new System.Drawing.Size(194, 28);
            show_gramm.TabIndex = 3;
            show_gramm.Text = "Show GRAMM surface/wind";
            show_gramm.UseVisualStyleBackColor = true;
            show_gramm.CheckedChanged += Show_grammCheckedChanged;
            // 
            // show_gral
            // 
            show_gral.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            show_gral.Location = new System.Drawing.Point(861, 668);
            show_gral.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            show_gral.Name = "show_gral";
            show_gral.Size = new System.Drawing.Size(183, 28);
            show_gral.TabIndex = 3;
            show_gral.Text = "Show GRAL surface/wind";
            show_gral.UseVisualStyleBackColor = true;
            show_gral.CheckedChanged += Show_gralCheckedChanged;
            // 
            // show_buildings
            // 
            show_buildings.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            show_buildings.Location = new System.Drawing.Point(861, 697);
            show_buildings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            show_buildings.Name = "show_buildings";
            show_buildings.Size = new System.Drawing.Size(144, 28);
            show_buildings.TabIndex = 3;
            show_buildings.Text = "Show buildings";
            show_buildings.UseVisualStyleBackColor = true;
            show_buildings.CheckedChanged += Show_buildingsCheckedChanged;
            // 
            // show_windfied
            // 
            show_windfied.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            show_windfied.Location = new System.Drawing.Point(582, 637);
            show_windfied.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            show_windfied.Name = "show_windfied";
            show_windfied.Size = new System.Drawing.Size(131, 37);
            show_windfied.TabIndex = 4;
            show_windfied.Text = "&Show windfield";
            show_windfied.UseVisualStyleBackColor = true;
            show_windfied.Click += Show_windfiedClick;
            // 
            // vScrollBar1
            // 
            vScrollBar1.Location = new System.Drawing.Point(1228, 0);
            vScrollBar1.Name = "vScrollBar1";
            vScrollBar1.Size = new System.Drawing.Size(25, 592);
            vScrollBar1.TabIndex = 5;
            vScrollBar1.ValueChanged += VScrollBar1ValueChanged;
            // 
            // wind_data_picturebox
            // 
            wind_data_picturebox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            wind_data_picturebox.BackColor = System.Drawing.SystemColors.Window;
            wind_data_picturebox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            wind_data_picturebox.InitialImage = null;
            wind_data_picturebox.Location = new System.Drawing.Point(720, 639);
            wind_data_picturebox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            wind_data_picturebox.Name = "wind_data_picturebox";
            wind_data_picturebox.Size = new System.Drawing.Size(134, 104);
            wind_data_picturebox.TabIndex = 8;
            wind_data_picturebox.TabStop = false;
            wind_data_picturebox.Paint += Wind_data_pictureboxPaint;
            // 
            // domainUpDown1
            // 
            domainUpDown1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            domainUpDown1.Location = new System.Drawing.Point(582, 726);
            domainUpDown1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            domainUpDown1.Name = "domainUpDown1";
            domainUpDown1.ReadOnly = true;
            domainUpDown1.Size = new System.Drawing.Size(130, 23);
            domainUpDown1.TabIndex = 10;
            domainUpDown1.SelectedItemChanged += DomainUpDown1SelectedItemChanged;
            // 
            // hScrollBar1
            // 
            hScrollBar1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            hScrollBar1.Location = new System.Drawing.Point(1, 595);
            hScrollBar1.Maximum = 1000;
            hScrollBar1.Name = "hScrollBar1";
            hScrollBar1.Size = new System.Drawing.Size(1212, 20);
            hScrollBar1.TabIndex = 16;
            hScrollBar1.Value = 500;
            hScrollBar1.ValueChanged += HScrollBar1ValueChanged;
            // 
            // Sectiondrawing
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.ControlLight;
            ClientSize = new System.Drawing.Size(1261, 757);
            Controls.Add(button3);
            Controls.Add(button4);
            Controls.Add(button1);
            Controls.Add(button2);
            Controls.Add(hScrollBar1);
            Controls.Add(Show3D);
            Controls.Add(filterdissit);
            Controls.Add(section_clipboard);
            Controls.Add(domainUpDown1);
            Controls.Add(wind_data_picturebox);
            Controls.Add(vScrollBar1);
            Controls.Add(show_windfied);
            Controls.Add(show_buildings);
            Controls.Add(show_gral);
            Controls.Add(show_gramm);
            Controls.Add(vert_minus);
            Controls.Add(vert_plus);
            Controls.Add(select_situation);
            Controls.Add(section_picture);
            DoubleBuffered = true;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "Sectiondrawing";
            Text = "Section drawing";
            FormClosed += SectiondrawingFormClosed;
            Load += SectiondrawingLoad;
            ResizeEnd += SectiondrawingResizeEnd;
            Resize += SectiondrawingResize;
            ((System.ComponentModel.ISupportInitialize)section_picture).EndInit();
            ((System.ComponentModel.ISupportInitialize)wind_data_picturebox).EndInit();
            ResumeLayout(false);

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
