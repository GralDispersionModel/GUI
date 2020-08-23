namespace GralMainForms
{
    partial class Amend_Landuse
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Amend_Landuse));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Albedo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Emissivity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Soilmoisture = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Roughnesslength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Heatconductivity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Thermalconductivity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button1 = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Code,
            this.Albedo,
            this.Emissivity,
            this.Soilmoisture,
            this.Roughnesslength,
            this.Heatconductivity,
            this.Thermalconductivity});
            this.dataGridView1.Location = new System.Drawing.Point(12, 10);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.Size = new System.Drawing.Size(730, 265);
            this.dataGridView1.TabIndex = 0;
            this.toolTip1.SetToolTip(this.dataGridView1, "Rows can be deleted by clicking with the right mouse button\r\non the row to be del" +
        "eted.");
            this.dataGridView1.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseClick);
            // 
            // Code
            // 
            this.Code.HeaderText = "Code";
            this.Code.Name = "Code";
            this.Code.Width = 57;
            // 
            // Albedo
            // 
            this.Albedo.HeaderText = "Albedo [-]";
            this.Albedo.Name = "Albedo";
            this.Albedo.Width = 71;
            // 
            // Emissivity
            // 
            this.Emissivity.HeaderText = "Emissivity [-]";
            this.Emissivity.Name = "Emissivity";
            this.Emissivity.Width = 82;
            // 
            // Soilmoisture
            // 
            this.Soilmoisture.HeaderText = "Soil moisture [-]";
            this.Soilmoisture.Name = "Soilmoisture";
            this.Soilmoisture.Width = 87;
            // 
            // Roughnesslength
            // 
            this.Roughnesslength.HeaderText = "Roughness length [m]";
            this.Roughnesslength.Name = "Roughnesslength";
            this.Roughnesslength.Width = 111;
            // 
            // Heatconductivity
            // 
            this.Heatconductivity.HeaderText = "Heat conductivity [W/(mK)]";
            this.Heatconductivity.Name = "Heatconductivity";
            this.Heatconductivity.Width = 147;
            // 
            // Thermalconductivity
            // 
            this.Thermalconductivity.HeaderText = "Thermal conductivity [m2/s]";
            this.Thermalconductivity.Name = "Thermalconductivity";
            this.Thermalconductivity.Width = 122;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(667, 284);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "&OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Amend_Landuse
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 318);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Amend_Landuse";
            this.Text = "Amend Land-use categories";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Amend_LanduseFormClosed);
            this.Load += new System.EventHandler(this.Amend_LanduseLoad);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Code;
        private System.Windows.Forms.DataGridViewTextBoxColumn Albedo;
        private System.Windows.Forms.DataGridViewTextBoxColumn Emissivity;
        private System.Windows.Forms.DataGridViewTextBoxColumn Soilmoisture;
        private System.Windows.Forms.DataGridViewTextBoxColumn Roughnesslength;
        private System.Windows.Forms.DataGridViewTextBoxColumn Heatconductivity;
        private System.Windows.Forms.DataGridViewTextBoxColumn Thermalconductivity;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolTip toolTip1;

    }
}