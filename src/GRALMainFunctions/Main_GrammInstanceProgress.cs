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
            int top = label66.Top - 2;
            grammPanel.SetBounds(groupBox30.Left, top, groupBox30.Width, groupBox30.Top - top - 2);
            groupBox15.Controls.Add(grammPanel);
            grammPanel.Start();
        }
    }

    internal sealed class GrammProgressPanel : Panel
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int SetWindowTheme(IntPtr window, string app, string classes);

        private readonly Label summary = new() { Name = "grammProgressSummary", AutoEllipsis = true, UseMnemonic = false };
        private readonly Label remaining = new() { Name = "grammProgressRemaining", TextAlign = ContentAlignment.TopRight, AutoEllipsis = true };
        private readonly Font heading;
        private readonly ProgressBar total = new() { Name = "grammTotalProgress", AccessibleName = "All GRAMM instances" };
        private readonly ListView rows = new()
        {
            Name = "grammInstanceList",
            AccessibleName = "Progress by GRAMM instance",
            View = View.Details,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            FullRowSelect = true,
            MultiSelect = false,
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Control,
            ForeColor = SystemColors.ControlText,
            ShowItemToolTips = true,
            OwnerDraw = true
        };
        private readonly GrammProgressRun run;
        private readonly System.Windows.Forms.Timer timer = new() { Interval = 3000 };
        private readonly CancellationTokenSource closed = new();
        private readonly Dictionary<int, DateTime> verified = new();
        private int[] counts;
        private int instanceWidth;
        private bool reading;

        internal GrammProgressPanel(GrammProgressRun run, Font font)
        {
            this.run = run;
            Font = font;
            BackColor = SystemColors.Control;
            ForeColor = SystemColors.ControlText;
            summary.Font = heading = new Font(font, FontStyle.Bold);
            rows.Columns.Add("Instance", 1);
            rows.Columns.Add("Done", 1, HorizontalAlignment.Right);
            rows.Columns.Add("Left", 1, HorizontalAlignment.Right);
            rows.DrawColumnHeader += (_, e) => e.DrawDefault = true;
            rows.DrawSubItem += DrawProgress;
            foreach (var range in run.Ranges)
            {
                rows.Items.Add(new ListViewItem(new[] { range.Instance.ToString(), "", "" }));
                UpdateRow(rows.Items.Count - 1, null);
            }
            Controls.AddRange(new Control[] { summary, remaining, total, rows });
            summary.Text = run.UnavailableReason ?? "Checking wind fields…";
            summary.AccessibleDescription = "Completed original weather situations across all assigned instances.";
        }

        internal void Start()
        {
            summary.SetBounds(0, 0, Width, heading.Height + 2);
            remaining.SetBounds(Width, 0, 0, summary.Height);
            total.SetBounds(0, summary.Bottom, Width, LogicalToDeviceUnits(7));
            rows.SetBounds(0, total.Bottom + 2, Width, Height - total.Bottom - 2);
            // Use the full-width system scrollbar, including both arrow buttons, for overflow.
            if (rows.Items.Count > 6 && OperatingSystem.IsWindows() && !SystemInformation.HighContrast)
            {
                SetWindowTheme(rows.Handle, "", "");
            }
            if (rows.Items.Count > 0)
            {
                // Size the viewport from the native row metrics so the seventh instance scrolls.
                Rectangle first = rows.GetItemRect(0);
                rows.Height = Math.Min(rows.Height, first.Top + Math.Min(6, rows.Items.Count) * first.Height + rows.Height - rows.ClientSize.Height);
                int largest = run.Ranges.Max(range => range.Last - range.First + 1);
                instanceWidth = TextRenderer.MeasureText(run.Ranges.Max(range => range.Instance).ToString(), Font).Width + 2;
                int available = rows.ClientSize.Width;
                int left = Math.Max(TextRenderer.MeasureText("Left", heading).Width, TextRenderer.MeasureText(largest.ToString(), Font).Width) + 6;
                int done = Math.Max(TextRenderer.MeasureText("Done", heading).Width, TextRenderer.MeasureText($"{largest}/{largest}", Font).Width) + 6;
                left = Math.Min(left, available / 4);
                done = Math.Min(done, available / 2);
                rows.Columns[0].Width = available - done - left;
                rows.Columns[1].Width = done;
                rows.Columns[2].Width = left;
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
            int[] result = null;
            try { result = await Task.Run(() => run.CountWindFields(verified, token)); }
            catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException) { } // Retry on the next tick.
            finally { reading = false; }
            if (token.IsCancellationRequested) { return; }

            counts = result;
            int done = counts?.Sum() ?? 0;
            total.Maximum = run.PlannedSituations;
            total.Value = done;
            remaining.Text = counts == null ? "" : $"{total.Maximum - done} left";
            remaining.Width = counts == null ? 0 : Math.Min(Width / 2, TextRenderer.MeasureText(remaining.Text, Font).Width + 2);
            remaining.Left = Width - remaining.Width;
            summary.Width = Math.Max(0, remaining.Left - 2);
            if (counts == null)
            {
                summary.Text = summary.AccessibleDescription = "Wind fields unavailable. Retrying…";
            }
            else
            {
                int unassigned = total.Maximum - run.Ranges.Sum(range => range.Last - range.First + 1);
                string progress = $"{done}/{total.Maximum}";
                string notStarted = $"{unassigned} not started";
                summary.AccessibleDescription = $"{done} of {total.Maximum} planned weather situations completed.";
                summary.Text = unassigned == 0 ? $"All instances: {progress}" : $"{progress}, {notStarted}";
                if (TextRenderer.MeasureText(summary.Text, heading).Width > summary.Width)
                {
                    summary.Text = unassigned == 0 ? $"All: {progress}" : notStarted;
                }
            }
            for (int i = 0; i < rows.Items.Count; i++) { UpdateRow(i, counts?[i]); }
            rows.Invalidate();
        }

        private void UpdateRow(int index, int? done)
        {
            var range = run.Ranges[index];
            int assigned = range.Last - range.First + 1;
            ListViewItem row = rows.Items[index];
            row.SubItems[1].Text = $"{done?.ToString() ?? "—"}/{assigned}";
            row.SubItems[2].Text = (assigned - done)?.ToString() ?? "—";
            row.ToolTipText = $"Original situations {range.First}–{range.Last}\n{row.SubItems[1].Text} completed/assigned, {row.SubItems[2].Text} left";
        }

        private void DrawProgress(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex != 0 || counts == null)
            {
                e.DrawDefault = true;
                return;
            }
            bool selected = e.Item.Selected && rows.Focused;
            e.Graphics.FillRectangle(selected ? SystemBrushes.Highlight : SystemBrushes.Control, e.Bounds);
            Rectangle cell = Rectangle.Inflate(e.Bounds, -3, 0);
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, Font, cell, selected ? SystemColors.HighlightText : rows.ForeColor,
                TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            var range = run.Ranges[e.ItemIndex];
            int height = Math.Min(LogicalToDeviceUnits(8), cell.Height);
            var track = new Rectangle(cell.Left + instanceWidth, cell.Top + (cell.Height - height) / 2,
                cell.Width - instanceWidth, height);
            if (track.Width < 4) { return; }
            bool themed = ProgressBarRenderer.IsSupported && !SystemInformation.HighContrast;
            if (themed) { ProgressBarRenderer.DrawHorizontalBar(e.Graphics, track); }
            else
            {
                e.Graphics.FillRectangle(SystemBrushes.Window, track);
                ControlPaint.DrawBorder(e.Graphics, track, SystemColors.WindowText, ButtonBorderStyle.Solid);
            }
            track.Inflate(-1, -1);
            track.Width = (int)((long)track.Width * counts[e.ItemIndex] / (range.Last - range.First + 1));
            if (track.Width == 0) { return; }
            if (themed) { ProgressBarRenderer.DrawHorizontalChunks(e.Graphics, track); }
            else { e.Graphics.FillRectangle(SystemBrushes.Highlight, track); }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                timer.Dispose();
                closed.Cancel();
                closed.Dispose();
                heading.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
