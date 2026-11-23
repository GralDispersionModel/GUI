using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gral
{
    partial class Main
    {
        private GrammProgressRun grammProgress;
        private GrammProgressPanel grammPanel;

        private void ResetGrammProgress()
        {
            if (grammProgress != null) { grammProgress.Retired = true; }
            grammProgress = null;
            if (groupBox30 == null) { return; }
            groupBox30.Enabled = true;
            if (grammPanel == null) { return; }
            grammPanel.Dispose();
            grammPanel = null;
            groupBox30.Top = progressBar2.Bottom + 12;
            label66.Visible = label67.Visible = progressBar1.Visible = progressBar2.Visible = true;
        }

        private void ShowGrammProgress(GrammProgressRun run)
        {
            label66.Visible = label67.Visible = progressBar1.Visible = progressBar2.Visible = false;
            groupBox30.Top = groupBox15.ClientSize.Height - groupBox30.Height - 5;
            grammPanel = new GrammProgressPanel(run, label66.Font);
            grammPanel.SetBounds(4, 60, groupBox15.ClientSize.Width - 10, groupBox30.Top - 62);
            groupBox15.Controls.Add(grammPanel);
            grammPanel.Start();
        }
    }

    internal sealed class GrammProgressPanel : Panel
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int SetWindowTheme(IntPtr window, string app, string classes);

        private readonly Label summary = new() { Name = "grammProgressSummary", AutoEllipsis = true };
        private readonly ProgressBar total = new() { Name = "grammTotalProgress", AccessibleName = "All GRAMM instances" };
        private readonly ListView rows = new()
        {
            Name = "grammInstanceList",
            AccessibleName = "Progress by GRAMM instance",
            View = View.Details,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            FullRowSelect = true,
            MultiSelect = false,
            ShowItemToolTips = true,
            OwnerDraw = true
        };
        private readonly GrammProgressRun run;
        private readonly System.Windows.Forms.Timer timer = new() { Interval = 3000 };
        private readonly CancellationTokenSource closed = new();
        private readonly Dictionary<int, DateTime> verified = new();
        private int[] counts;
        private bool reading;

        internal GrammProgressPanel(GrammProgressRun run, Font font)
        {
            this.run = run;
            Font = font;
            rows.Columns.Add("Instance", 78);
            rows.Columns.Add("Done", 53, HorizontalAlignment.Right);
            rows.Columns.Add("Left", 51, HorizontalAlignment.Right);
            rows.DrawColumnHeader += (_, e) => e.DrawDefault = true;
            rows.DrawSubItem += DrawProgress;
            foreach (var range in run.Ranges)
            {
                rows.Items.Add(new ListViewItem(new[] { range.Instance.ToString(), "—", "—" })
                {
                    ToolTipText = $"Original situations {range.First}–{range.Last}"
                });
            }
            Controls.AddRange(new Control[] { summary, total, rows });
            summary.Text = run.UnavailableReason ?? "Checking wind fields…";
            summary.AccessibleDescription = "Completed original weather situations across all assigned instances.";
        }

        internal void Start()
        {
            summary.SetBounds(0, 0, Width, 18);
            total.SetBounds(0, 20, Width, 9);
            rows.SetBounds(0, 33, Width, Height - 33);
            // Use the full-width system scrollbar, including both arrow buttons, for overflow.
            if (rows.Items.Count > 6 && OperatingSystem.IsWindows() && !SystemInformation.HighContrast)
            {
                SetWindowTheme(rows.Handle, "", "");
            }
            if (rows.Items.Count > 0)
            {
                // Size the viewport from the native row metrics so the seventh instance scrolls.
                Rectangle first = rows.GetItemRect(0);
                rows.Height = Math.Min(rows.Height, first.Top + 6 * first.Height + rows.Height - rows.ClientSize.Height);
            }
            if (run.UnavailableReason != null) { return; }
            timer.Tick += RefreshCounts;
            timer.Start();
            RefreshCounts(this, EventArgs.Empty);
        }

        private async void RefreshCounts(object sender, EventArgs e)
        {
            if (reading || !Visible) { return; }
            reading = true;
            var token = closed.Token;
            try
            {
                var result = await Task.Run(() => run.CountWindFields(verified, token));
                if (token.IsCancellationRequested) { return; }
                counts = result;
                int done = counts.Sum();
                total.Maximum = run.PlannedSituations;
                total.Value = done;
                int unassigned = total.Maximum - run.Ranges.Sum(range => range.Last - range.First + 1);
                summary.Text = unassigned == 0 ? $"{done} / {total.Maximum} done, {total.Maximum - done} left"
                    : $"{total.Maximum - done} left, {unassigned} not started";
                for (int i = 0; i < counts.Length; i++)
                {
                    var range = run.Ranges[i];
                    rows.Items[i].SubItems[1].Text = counts[i].ToString();
                    rows.Items[i].SubItems[2].Text = (range.Last - range.First + 1 - counts[i]).ToString();
                }
                rows.Invalidate();
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException)
            {
                if (!token.IsCancellationRequested)
                {
                    counts = null;
                    summary.Text = "Wind fields unavailable. Retrying…";
                    total.Value = 0;
                    foreach (ListViewItem row in rows.Items) { row.SubItems[1].Text = row.SubItems[2].Text = "—"; }
                    rows.Invalidate();
                }
            }
            finally { reading = false; }
        }

        private void DrawProgress(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex != 0 || counts == null) { e.DrawDefault = true; return; }
            e.DrawBackground();
            e.DrawText(TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            var range = run.Ranges[e.ItemIndex];
            var track = new Rectangle(e.Bounds.Left + 23, e.Bounds.Top + (e.Bounds.Height - 8) / 2, e.Bounds.Width - 29, 8);
            ControlPaint.DrawBorder(e.Graphics, track, SystemColors.GrayText, ButtonBorderStyle.Solid);
            track.Inflate(-1, -1);
            track.Width = (int)((long)track.Width * counts[e.ItemIndex] / (range.Last - range.First + 1));
            e.Graphics.FillRectangle(SystemBrushes.Highlight, track);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                timer.Dispose();
                closed.Cancel();
                closed.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
