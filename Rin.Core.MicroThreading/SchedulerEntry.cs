using Rin.Diagnostics;

namespace Rin.Core.MicroThreading;

/// <summary>
///     Either a MicroThread or an action with priority.
/// </summary>
struct SchedulerEntry : IComparable<SchedulerEntry> {
    public readonly Action Action;
    public readonly MicroThread? MicroThread;
    public long Priority;
    public long SchedulerCounter;
    public object Token;
    public ProfilingKey? ProfilingKey;

    public SchedulerEntry(MicroThread microThread) : this() {
        MicroThread = microThread;
    }

    public SchedulerEntry(Action action, long priority) : this() {
        Action = action;
        Priority = priority;
    }

    public int CompareTo(SchedulerEntry other) {
        var priorityDiff = Priority.CompareTo(other.Priority);
        return priorityDiff != 0 ? priorityDiff : SchedulerCounter.CompareTo(other.SchedulerCounter);
    }
}
