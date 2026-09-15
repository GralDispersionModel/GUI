using GralIO;
using Gral.GralMainForms;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Gral
{
    partial class Main
    {
        private void InitializeGrammInitialConditions()
        {
            var button = new Button { Name = "GrammInitialConditions", Text = "Initial conditions...", Location = new Point(285, 104), Size = new Size(213, 30), Font = new Font("Segoe UI", 9.75F), UseVisualStyleBackColor = true, TabIndex = 35 };
            button.Click += (s, e) =>
            {
                if (!EmifileReset || string.IsNullOrEmpty(ProjectName)) return;
                try
                {
                    string folder = Path.Combine(ProjectName, "Computation");
                    if (!File.Exists(Path.Combine(folder, "IIN.dat"))) SaveIINDatFile();
                    using (var dialog = new GrammInitialConditionsDialog(folder, (double)numericUpDown39.Value, checkBox35.Checked)) dialog.ShowDialog(this);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is FormatException || ex is IndexOutOfRangeException)
                { MessageBox.Show(this, ex.Message, "GRAMM initial conditions", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };
            groupBox12.Controls.Add(button);
        }

        private bool ValidateGrammCustomConditions()
        {
            string folder = Path.Combine(ProjectName, "Computation");
            // Keep the engine's existing behavior for unmarked, hand-written input files.
            if (checkBox35.Checked) return true;
            try { if (!GrammInitialConditions.IsGuiGenerated(Path.Combine(folder, "CustomInit.txt"))) return true; GrammInitialConditions.ReadCustom(Path.Combine(folder, "CustomInit.txt"), Path.Combine(folder, "meteopgt.all")); return true; }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is FormatException)
            {
                MessageBox.Show(this, ex.Message + "\nOpen GRAMM input > Initial conditions to check the file.", "GRAMM initial conditions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
    }
}
