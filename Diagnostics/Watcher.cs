using System.Diagnostics;

namespace Rin.Diagnostics;

public readonly struct Watcher : IDisposable {
    readonly Action<long> reportCallback;
    readonly Stopwatch stopwatch = new();

    public Watcher(Action<long> reportCallback) {
        this.reportCallback = reportCallback;
        stopwatch.Start();
    }
    
    public void Dispose() {
        reportCallback(stopwatch.ElapsedMilliseconds);
    }
}
