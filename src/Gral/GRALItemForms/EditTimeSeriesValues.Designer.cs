/*
 * Created by SharpDevelop.
 * User: Markus Kuntner
 * Date: 11.11.2018
 * Time: 13:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace GralItemForms
{
	partial class EditTimeSeriesValues
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditTimeSeriesValues));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Span = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Jan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Feb = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Mar = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Apr = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Mai = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Jun = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Jul = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Aug = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Sep = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Oct = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Nov = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Dec = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowDrop = true;
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Span,
            this.Jan,
            this.Feb,
            this.Mar,
            this.Apr,
            this.Mai,
            this.Jun,
            this.Jul,
            this.Aug,
            this.Sep,
            this.Oct,
            this.Nov,
            this.Dec});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Top;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(754, 128);
            this.dataGridView1.TabIndex = 6;
            this.dataGridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataGridView1KeyDown);
            // 
            // Span
            // 
            this.Span.Frozen = true;
            this.Span.HeaderText = "Time Span";
            this.Span.Name = "Span";
            this.Span.ReadOnly = true;
            this.Span.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Span.Width = 80;
            // 
            // Jan
            // 
            this.Jan.HeaderText = "Jan";
            this.Jan.Name = "Jan";
            this.Jan.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Jan.Width = 50;
            // 
            // Feb
            // 
            this.Feb.HeaderText = "Feb";
            this.Feb.Name = "Feb";
            this.Feb.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Feb.Width = 50;
            // 
            // Mar
            // 
            this.Mar.HeaderText = "Mar";
            this.Mar.Name = "Mar";
            this.Mar.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Mar.Width = 50;
            // 
            // Apr
            // 
            this.Apr.HeaderText = "Apr";
            this.Apr.Name = "Apr";
            this.Apr.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Apr.Width = 50;
            // 
            // Mai
            // 
            this.Mai.HeaderText = "Mai";
            this.Mai.Name = "Mai";
            this.Mai.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Mai.Width = 50;
            // 
            // Jun
            // 
            this.Jun.HeaderText = "Jun";
            this.Jun.Name = "Jun";
            this.Jun.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Jun.Width = 50;
            // 
            // Jul
            // 
            this.Jul.HeaderText = "Jul";
            this.Jul.Name = "Jul";
            this.Jul.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Jul.Width = 50;
            // 
            // Aug
            // 
            this.Aug.HeaderText = "Aug";
            this.Aug.Name = "Aug";
            this.Aug.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Aug.Width = 50;
            // 
            // Sep
            // 
            this.Sep.HeaderText = "Sep";
            this.Sep.Name = "Sep";
            this.Sep.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Sep.Width = 50;
            // 
            // Oct
            // 
            this.Oct.HeaderText = "Oct";
            this.Oct.Name = "Oct";
            this.Oct.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Oct.Width = 50;
            // 
            // Nov
            // 
            this.Nov.HeaderText = "Nov";
            this.Nov.Name = "Nov";
            this.Nov.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Nov.Width = 50;
            // 
            // Dec
            // 
            this.Dec.HeaderText = "Dec";
            this.Dec.Name = "Dec";
            this.Dec.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Dec.Width = 50;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(638, 176);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(104, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "&Exit and Save";
            this.toolTip1.SetToolTip(this.button1, "Save current entries and exit");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(525, 176);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(104, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "&Restore";
            this.toolTip1.SetToolTip(this.button2, "Reset and exit");
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(129, 42);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(121, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "&Create new entry";
            this.toolTip1.SetToolTip(this.button3, "Create a new entry");
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Button3Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Location = new System.Drawing.Point(4, 137);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 71);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Create a new entry";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 23);
            this.label1.TabIndex = 5;
            this.label1.Text = "New name:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(91, 19);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(159, 20);
            this.textBox1.TabIndex = 4;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownHeight = 250;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.IntegralHeight = false;
            this.comboBox1.Location = new System.Drawing.Point(6, 18);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(219, 21);
            this.comboBox1.TabIndex = 1;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.ComboBox1SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.comboBox1);
            this.groupBox2.Location = new System.Drawing.Point(271, 137);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(236, 65);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Select an entry";
            // 
            // EditTimeSeriesValues
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(754, 211);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditTimeSeriesValues";
            this.Text = "EditTimeSeriesValues";
            this.Load += new System.EventHandler(this.EditTimeSeriesValuesLoad);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.DataGridViewTextBoxColumn Span;
		private System.Windows.Forms.DataGridViewTextBoxColumn Dec;
		private System.Windows.Forms.DataGridViewTextBoxColumn Nov;
		private System.Windows.Forms.DataGridViewTextBoxColumn Oct;
		private System.Windows.Forms.DataGridViewTextBoxColumn Sep;
		private System.Windows.Forms.DataGridViewTextBoxColumn Aug;
		private System.Windows.Forms.DataGridViewTextBoxColumn Jul;
		private System.Windows.Forms.DataGridViewTextBoxColumn Jun;
		private System.Windows.Forms.DataGridViewTextBoxColumn Mai;
		private System.Windows.Forms.DataGridViewTextBoxColumn Apr;
		private System.Windows.Forms.DataGridViewTextBoxColumn Mar;
		private System.Windows.Forms.DataGridViewTextBoxColumn Feb;
		private System.Windows.Forms.DataGridViewTextBoxColumn Jan;
		private System.Windows.Forms.DataGridView dataGridView1;
	}
}
