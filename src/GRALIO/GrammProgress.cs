#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Linq;

namespace GralIO
{
    public enum GrammInstanceState { Waiting, Starting, Running, Verifying, Completed, Paused, Failed, Unused }

    public sealed class GrammInstanceProgress
    {
        public int Id { get; }
        public GrammInstanceState State { get; }
        public int First { get; }
        public int Last { get; }
        public int Completed { get; }
        public int Assigned { get; }
        public string AssignedRanges { get; }
        public string CpuAssignment { get; }
        public int Remaining => Assigned - Completed;
        public int SessionCompleted { get; }
        public int ProcessId { get; }
        public TimeSpan Elapsed { get; }
        public GrammConsoleStatus ConsoleStatus { get; }
        public GrammInstanceProgress(int id, GrammInstanceState state, int first, int last,
            int completed, int sessionCompleted, int processId, TimeSpan elapsed, GrammConsoleStatus consoleStatus = null,
            int assigned = -1, string assignedRanges = null, string cpuAssignment = null)
        {
            Id = id; State = state; First = first; Last = last; Completed = completed;
            SessionCompleted = sessionCompleted; ProcessId = processId; Elapsed = elapsed; ConsoleStatus = consoleStatus;
            Assigned = assigned >= 0 ? assigned : first > 0 ? last - first + 1 : 0;
            AssignedRanges = assignedRanges ?? (first > 0 ? $"{first:N0}-{last:N0}" : "");
            CpuAssignment = cpuAssignment ?? "Windows scheduling";
        }
    }

    public sealed class GrammProgressSnapshot
    {
        public int Total { get; }
        public int CompletedBefore { get; }
        public int CompletedThisRun { get; }
        public int Completed => CompletedBefore + CompletedThisRun;
        public int Remaining => Total - Completed;
        public int Queued { get; }
        public IReadOnlyList<GrammInstanceProgress> Instances { get; }
        public int Active => Instances.Count(i => i.State == GrammInstanceState.Starting ||
            i.State == GrammInstanceState.Running || i.State == GrammInstanceState.Verifying);
        public DateTime CapturedAt { get; }
        public GrammProgressSnapshot(int total, int completedBefore, int completedThisRun, int queued,
            IReadOnlyList<GrammInstanceProgress> instances)
        {
            Total = total; CompletedBefore = completedBefore; CompletedThisRun = completedThisRun;
            Queued = queued; Instances = instances; CapturedAt = DateTime.UtcNow;
        }
    }

    // Called under the runner's gate. Snapshots never expose mutable worker state to the GUI.
    internal sealed class GrammProgressTracker
    {
        private sealed class Slot
        {
            public int Id, First, Last, Completed, SessionCompleted, ProcessId, Assigned;
            public string AssignedRanges, CpuAssignment;
            public GrammInstanceState State;
            public DateTime Started;
            public DateTime? Finished;
            public GrammConsoleStatus ConsoleStatus;
        }
        private readonly Slot[] slots;
        private readonly int total, completedBefore;
        private int completed;
        public GrammProgressTracker(GrammResumePlan plan, int requested)
        {
            total = plan.CompletedCount + plan.PendingCount;
            completedBefore = plan.CompletedCount;
            slots = Enumerable.Range(1, requested).Select(id => new Slot {
                Id = id, State = id <= plan.InstanceAssignments.Count ? GrammInstanceState.Waiting : GrammInstanceState.Unused,
                Assigned = id <= plan.InstanceAssignments.Count ? plan.InstanceAssignments[id - 1].Sum(r => r.Count) : 0,
                AssignedRanges = id <= plan.InstanceAssignments.Count ? string.Join("; ", plan.InstanceAssignments[id - 1].Select(r =>
                    r.First == r.Last ? r.First.ToString("N0") : $"{r.First:N0}-{r.Last:N0}")) : ""
            }).ToArray();
        }
        public void Assign(int id, GrammSituationRange range)
        {
            var slot = slots[id - 1];
            slot.First = range.First; slot.Last = range.Last;
            slot.ConsoleStatus = null; slot.ProcessId = 0; if (slot.Started == default) slot.Started = DateTime.UtcNow; slot.Finished = null; slot.State = GrammInstanceState.Starting;

        }
        public void Update(int id, int added, GrammInstanceState state, int processId = 0, GrammConsoleStatus consoleStatus = null)
        {
            var slot = slots[id - 1]; slot.Completed += added; slot.SessionCompleted += added;
            if (consoleStatus != null) slot.ConsoleStatus = consoleStatus;
            if (state == GrammInstanceState.Completed && slot.Completed < slot.Assigned) state = GrammInstanceState.Waiting;
            completed += added; slot.State = state; slot.ProcessId = processId;
            if (state == GrammInstanceState.Completed || state == GrammInstanceState.Paused || state == GrammInstanceState.Failed)
                slot.Finished ??= DateTime.UtcNow;
        }
        public void SetCpuAssignment(int id, string text) { slots[id - 1].CpuAssignment = text; }
        public GrammProgressSnapshot Snapshot()
        {
            var now = DateTime.UtcNow;
            return new GrammProgressSnapshot(total, completedBefore, completed, 0,
                Array.AsReadOnly(slots.Select(s => new GrammInstanceProgress(s.Id, s.State, s.First, s.Last,
                    s.Completed, s.SessionCompleted, s.ProcessId, s.Started == default ? TimeSpan.Zero : (s.Finished ?? now) - s.Started, s.ConsoleStatus, s.Assigned, s.AssignedRanges, s.CpuAssignment)).ToArray()));
        }
    }
}
#endif
