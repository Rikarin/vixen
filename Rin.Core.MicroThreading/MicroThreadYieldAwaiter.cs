using System.Runtime.CompilerServices;

namespace Rin.Core.MicroThreading;

public struct MicroThreadYieldAwaiter : INotifyCompletion {
    readonly MicroThread microThread;

    public bool IsCompleted {
        get {
            if (microThread.IsOver) {
                return true;
            }

            lock (microThread.Scheduler.ScheduledEntries) {
                return microThread.Scheduler.ScheduledEntries.Count == 0;
            }
        }
    }

    public MicroThreadYieldAwaiter(MicroThread microThread) {
        this.microThread = microThread;
    }

    public MicroThreadYieldAwaiter GetAwaiter() => this;

    public void GetResult() {
        microThread.CancellationToken.ThrowIfCancellationRequested();
    }

    public void OnCompleted(Action continuation) {
        microThread.ScheduleContinuation(ScheduleMode.Last, continuation);
    }
}
