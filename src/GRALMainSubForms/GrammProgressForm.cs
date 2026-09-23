#if !__MonoCS__
using GralIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GralMainForms
{
    internal static class GrammProgressStyle
    {
        public static Color Background => SystemColors.Control;
        public static Color Text => SystemColors.ControlText;
        public static readonly Font Body = new Font("Segoe UI", 9);
        public static readonly Font Strong = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        public static readonly Font Heading = new Font("Segoe UI", 11.25F, FontStyle.Bold);
        public static Label Label(string name, Font font = null) => new Label {
            Name = name, AutoSize = false, AutoEllipsis = true, UseMnemonic = false,
            TextAlign = ContentAlignment.MiddleLeft, Font = font ?? Body,
            ForeColor = Text, BackColor = Color.Transparent };
    }

    public sealed class GrammProgressBar : ProgressBar
    {
        public int Completed { get; private set; }
        public int Total { get; private set; }
        public GrammProgressBar() { Style = ProgressBarStyle.Continuous; Step = 1; }
        public void SetProgress(int completed, int total)
        {
            Completed = completed; Total = total;
            Maximum = Math.Max(1, total);
            Value = Math.Clamp(completed, 0, Maximum);
            AccessibleName = total > 0 ? $"{completed} / {total}" : "Waiting for assignment";
        }
    }

    public sealed class GrammInstanceCard : GroupBox
    {
        readonly Label state = GrammProgressStyle.Label("instance_state");
        readonly Label range = GrammProgressStyle.Label("instance_range");
        readonly Label done = GrammProgressStyle.Label("instance_completed");
        readonly Label remaining = GrammProgressStyle.Label("instance_remaining");
        readonly Label session = GrammProgressStyle.Label("instance_session");
        readonly Label current = GrammProgressStyle.Label("instance_current");
        readonly Label elapsed = GrammProgressStyle.Label("instance_elapsed");
        readonly Label cpu = GrammProgressStyle.Label("instance_cpu");
        readonly ToolTip tips = new ToolTip();
        readonly Button details = new Button { Name = "instance_details", Text = "Details...",
            Font = GrammProgressStyle.Body, UseVisualStyleBackColor = true };
        Form detailForm;
        TextBox detailText;
        readonly GrammProgressBar bar = new GrammProgressBar { Name = "instance_progress" };
        public GrammInstanceProgress Progress { get; private set; }
        public GrammInstanceCard(int id)
        {
            Name = "instance_" + id; Text = $"Instance {id:00}";
            BackColor = GrammProgressStyle.Background; ForeColor = GrammProgressStyle.Text;
            Font = GrammProgressStyle.Strong;
            AccessibleRole = AccessibleRole.Grouping;
            remaining.TextAlign = ContentAlignment.MiddleRight;
            Controls.AddRange(new Control[] { state, range, done, remaining, session, current, cpu, elapsed, details, bar });
            details.Click += (sender, e) => ShowDetails();
        }
        int S(int value) => (int)Math.Round(value * DeviceDpi / 96.0);
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (state == null) return;
            int margin = S(12), width = Math.Max(0, Width - margin * 2);
            state.SetBounds(margin, S(22), width, S(20));
            range.SetBounds(margin, S(44), width, S(20));
            bar.SetBounds(margin, S(70), width, S(18));
            done.SetBounds(margin, S(93), width / 2 + S(12), S(22));
            remaining.SetBounds(margin + width / 2 + S(12), S(93), width - width / 2 - S(12), S(22));
            session.SetBounds(margin, S(117), width, S(20));
            current.SetBounds(margin, S(140), width, S(20));
            cpu.SetBounds(margin, S(164), width, S(20));
            elapsed.SetBounds(margin, S(190), width - S(88), S(22));
            details.SetBounds(margin + width - S(80), S(186), S(80), S(27));
        }
        public void Apply(GrammInstanceProgress value)
        {
            Progress = value;
            state.Text = value.State switch {
                GrammInstanceState.Starting => "Starting", GrammInstanceState.Running => "Running",
                GrammInstanceState.Verifying => "Checking results", GrammInstanceState.Completed => "Completed",
                GrammInstanceState.Paused => "Paused", GrammInstanceState.Failed => "Error",
                GrammInstanceState.Unused => "No work assigned", _ => "Waiting for assignment" };
            range.Text = value.Assigned > 0 ? $"Situations: {value.AssignedRanges}" :
                value.State == GrammInstanceState.Unused ? "No situations to assign" : "Waiting for the next range";
            done.Text = value.Assigned > 0 ? $"Completed: {value.Completed:N0} / {value.Assigned:N0}" : "Completed: 0";
            remaining.Text = $"Remaining: {value.Remaining:N0}";
            session.Text = value.First > 0 ? $"Current range: {value.First:N0}-{value.Last:N0}" : "Waiting to start";
            cpu.Text = "CPU: " + value.CpuAssignment;
            tips.SetToolTip(range, range.Text); tips.SetToolTip(cpu, cpu.Text);
            var live = value.ConsoleStatus;
            current.Text = value.State == GrammInstanceState.Completed ? "All assigned situations completed" :
                live?.Available == true && live.TargetSeconds > 0 && live.Situation >= value.First && live.Situation <= value.Last
                ? $"Current situation: {live.Situation:N0}  |  {Math.Clamp(live.SimulationSeconds / live.TargetSeconds, 0, 1):P1}" :
                value.State == GrammInstanceState.Running ? (live != null && !live.Available ? "Current progress unavailable" : "Initializing / waiting for progress") : "";
            elapsed.Text = value.Assigned > 0 ? $"Elapsed: {(int)value.Elapsed.TotalHours:00}:{value.Elapsed.Minutes:00}:{value.Elapsed.Seconds:00}" : "";
            details.Enabled = value.Assigned > 0 || live != null;
            if (detailForm != null && !detailForm.IsDisposed) RefreshDetails();
            bar.SetProgress(value.Completed, value.Assigned);
            AccessibleName = Text + ", " + state.Text + ", " + range.Text + ", " + done.Text + ", " + remaining.Text;
        }
        void RefreshDetails()
        {
            var status = Progress?.ConsoleStatus;
            string text = "Assigned situations: " + Progress?.AssignedRanges + "\r\nCPU: " + Progress?.CpuAssignment + "\r\n\r\n" +
                (status?.Available == true ? status.Text : "Console output is unavailable.\r\n" + status?.Error);
            if (detailText.Text != text) { detailText.Text = text; detailText.SelectionStart = detailText.TextLength; detailText.ScrollToCaret(); }
        }
        void ShowDetails()
        {
            if (detailForm == null || detailForm.IsDisposed)
            {
                detailForm = new Form { Text = Text + " - Recent console output", Size = new Size(820, 540),
                    MinimumSize = new Size(460, 300), StartPosition = FormStartPosition.CenterParent, ShowInTaskbar = false,
                    Font = GrammProgressStyle.Body, BackColor = GrammProgressStyle.Background, ForeColor = GrammProgressStyle.Text };
                detailText = new TextBox { Multiline = true, ReadOnly = true, WordWrap = false, ScrollBars = ScrollBars.Both,
                    Dock = DockStyle.Fill, Font = new Font(FontFamily.GenericMonospace, 9), BackColor = SystemColors.Window };
                detailForm.Controls.Add(detailText);
            }
            RefreshDetails(); detailForm.Show(FindForm()); detailForm.Activate();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { detailForm?.Dispose(); tips.Dispose(); }
            base.Dispose(disposing);
        }
    }

    public sealed class GrammProgressForm : Form
    {
        readonly GroupBox header = new GroupBox { Name = "overall_summary", Text = "GRAMM progress", Font = GrammProgressStyle.Heading };
        readonly Panel footer = new Panel();
        readonly FlowLayoutPanel cards = new FlowLayoutPanel { Name = "instance_cards", AutoScroll = true, WrapContents = true, FlowDirection = FlowDirection.LeftToRight };
        readonly Label project = GrammProgressStyle.Label("project_name");
        readonly Label phase = GrammProgressStyle.Label("run_phase");
        readonly Label total = GrammProgressStyle.Label("overall_completed", GrammProgressStyle.Strong);
        readonly Label detail = GrammProgressStyle.Label("overall_remaining");
        readonly Label basis = GrammProgressStyle.Label("progress_basis");
        readonly GrammProgressBar overall = new GrammProgressBar { Name = "overall_progress" };
        readonly Button pause = new Button { Name = "pause_all", Text = "Pause all", UseVisualStyleBackColor = true };
        readonly Button hide = new Button { Name = "hide_progress", Text = "Hide", UseVisualStyleBackColor = true };
        readonly List<GrammInstanceCard> instanceCards = new List<GrammInstanceCard>();
        bool layingOut, finished;
        public GrammProgressSnapshot Snapshot { get; private set; }
        public int ColumnCount { get; private set; }
        public IReadOnlyList<GrammInstanceCard> InstanceCards => instanceCards.AsReadOnly();
        public event EventHandler PauseRequested;
        public GrammProgressForm(string projectName, int requestedInstances)
        {
            Text = "GRAMM instance progress"; Name = "GrammProgressForm";
            Font = GrammProgressStyle.Body; ForeColor = GrammProgressStyle.Text;
            BackColor = GrammProgressStyle.Background; AutoScaleMode = AutoScaleMode.None;
            MinimumSize = new Size(420, 440); StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false; KeyPreview = true;
            project.Text = projectName;
            phase.Text = "Checking existing results";
            total.Text = "Checking completed situations";
            detail.Text = "Reading results for all situations";
            basis.Text = "Progress bars: completed situations.\nCurrent situation %: simulation time.";
            header.Controls.AddRange(new Control[] { project, phase, total, overall, detail });
            footer.Controls.AddRange(new Control[] { basis, pause, hide });
            Controls.AddRange(new Control[] { header, cards, footer });
            pause.Click += (s, e) => { RequestPause(); PauseRequested?.Invoke(this, EventArgs.Empty); };
            hide.Click += (s, e) => Hide();
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { Hide(); e.Handled = true; } };
            FormClosing += (s, e) => { if (e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); } };
            int columns = requestedInstances <= 2 ? Math.Max(1, requestedInstances) : requestedInstances <= 6 ? 2 : 4;
            var area = Screen.FromPoint(Cursor.Position).WorkingArea;
            ClientSize = new Size(Math.Min(Math.Max(640, columns * 316 + 24), area.Width - 60),
                Math.Min(242 + Math.Min(4, (int)Math.Ceiling((double)requestedInstances / columns)) * 236, area.Height - 100));
        }
        int S(int value) => (int)Math.Round(value * DeviceDpi / 96.0);
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (project == null || layingOut) return;
            layingOut = true;
            try
            {
                int margin = S(12), width = Math.Max(0, ClientSize.Width - margin * 2);
                bool narrow = ClientSize.Width < S(660);
                int headerHeight = S(153), footerHeight = S(narrow ? 94 : 62);
                header.SetBounds(margin, S(8), width, headerHeight);
                int innerWidth = Math.Max(0, width - margin * 2);
                project.SetBounds(margin, S(25), innerWidth, S(21));
                total.SetBounds(margin, S(49), innerWidth, S(25));
                overall.SetBounds(margin, S(80), innerWidth, S(18));
                detail.SetBounds(margin, S(102), innerWidth, S(21));
                phase.SetBounds(margin, S(126), innerWidth, S(20));
                cards.SetBounds(margin, headerHeight + S(18), width, Math.Max(1, ClientSize.Height - headerHeight - footerHeight - S(24)));
                footer.SetBounds(0, ClientSize.Height - footerHeight, ClientSize.Width, footerHeight);
                basis.SetBounds(margin, S(5), narrow ? width : Math.Max(0, width - S(208)), S(40));
                int buttonY = narrow ? S(52) : S(16);
                pause.SetBounds(margin + width - S(190), buttonY, S(100), S(29));
                hide.SetBounds(margin + width - S(80), buttonY, S(80), S(29));
                LayoutCards();
            }
            finally { layingOut = false; }
        }
        void LayoutCards()
        {
            int gap = S(10), available = Math.Max(1, cards.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 2);
            int columns = Math.Max(1, Math.Min(Math.Min(4, instanceCards.Count), (available + gap) / (S(300) + gap)));
            ColumnCount = columns;
            int width = Math.Max(1, (available - (columns - 1) * gap) / columns);
            cards.SuspendLayout();
            for (int i = 0; i < instanceCards.Count; i++)
            {
                instanceCards[i].Size = new Size(width, S(224));
                instanceCards[i].Margin = new Padding(0, 0, (i + 1) % columns == 0 ? 0 : gap, gap);
            }
            cards.ResumeLayout();
        }
        public void Apply(GrammProgressSnapshot snapshot)
        {
            Snapshot = snapshot;
            while (instanceCards.Count < snapshot.Instances.Count)
            {
                var card = new GrammInstanceCard(instanceCards.Count + 1); instanceCards.Add(card); cards.Controls.Add(card);
            }
            for (int i = 0; i < snapshot.Instances.Count; i++) instanceCards[i].Apply(snapshot.Instances[i]);
            double percent = snapshot.Total > 0 ? (double)snapshot.Completed / snapshot.Total : 0;
            total.Text = $"Completed: {snapshot.Completed:N0} / {snapshot.Total:N0}  ({percent:P1})";
            detail.Text = $"Remaining: {snapshot.Remaining:N0}  |  Running: {snapshot.Active} / {snapshot.Instances.Count}  |  Queued: {snapshot.Queued:N0}";
            overall.SetProgress(snapshot.Completed, snapshot.Total);
            if (!finished) phase.Text = snapshot.Remaining == 0 ? "Final verification" : "Calculating";
            LayoutCards();
        }
        public void ShowChecking(int scanned, int count)
        {
            total.Text = $"Checked: {scanned:N0} / {count:N0}";
            overall.SetProgress(scanned, count);
            detail.Text = "Verifying existing result files";
        }
        public void RequestPause() { finished = true; pause.Enabled = false; phase.Text = "Pausing"; }
        public void Finish(string text, bool failed = false)
        {
            finished = true; pause.Enabled = false; phase.Text = text;
        }
    }
}
#endif
