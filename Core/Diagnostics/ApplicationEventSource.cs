using System.Diagnostics.Tracing;

namespace Rin.Core.Diagnostics;

// TODO: check this for performance https://learn.microsoft.com/en-us/dotnet/core/diagnostics/event-counters#conditional-counters
// More examples https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventsource?view=net-7.0#examples
[EventSource(Name = "Rin.Application")]
public sealed class ApplicationEventSource : EventSource {
    public static readonly ApplicationEventSource Log = new();
    
    readonly EventCounter mainThreadWorkTime;
    readonly EventCounter mainThreadWaitTime;

    ApplicationEventSource() {
        mainThreadWorkTime = new("main-thread-work-time", this) {
            DisplayName = "Main Thread Work Time", DisplayUnits = "ms"
        };
        
        mainThreadWaitTime = new("main-thread-wait-time", this) {
            DisplayName = "Main Thread Wait Time", DisplayUnits = "ms"
        };
    }

    public void Startup() => WriteEvent(1);

    public void ReportMainThreadWorkTime(long elapsedMilliseconds) {
        WriteEvent(2, elapsedMilliseconds);
        mainThreadWorkTime.WriteMetric(elapsedMilliseconds);
    }
    
    public void ReportMainThreadWaitTime(long elapsedMilliseconds) {
        WriteEvent(3, elapsedMilliseconds);
        mainThreadWaitTime.WriteMetric(elapsedMilliseconds);
    }
}
