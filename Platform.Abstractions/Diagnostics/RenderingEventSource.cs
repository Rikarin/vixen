using Rin.Diagnostics;
using System.Diagnostics.Tracing;

namespace Rin.Platform.Abstractions.Diagnostics;

[EventSource(Name = "Rin.Rendering")]
public sealed class RenderingEventSource : EventSource {
    public static readonly RenderingEventSource Log = new();

    readonly EventCounter submitCount;
    readonly EventCounter submitDisposalCount;
    readonly EventCounter rendererWorkTime;
    readonly EventCounter renderWaitTime;

    public static Watcher RenderWaitTime => new(Log.ReportRenderWaitTime);
    public static Watcher RenderWorkTime => new(Log.ReportRenderWorkTime);

    RenderingEventSource() {
        submitCount = new("render-submit-count", this) {
            DisplayName = "Render Submit Count"
        };
        
        submitDisposalCount = new("render-submit-disposal-count", this) {
            DisplayName = "Render Submit for Disposal Count"
        };
        
        rendererWorkTime = new("render-work-time", this) {
            DisplayName = "Render Work Time", DisplayUnits = "ms"
        };

        renderWaitTime = new("render-wait-time", this) {
            DisplayName = "Render Wait Time", DisplayUnits = "ms"
        };
    }

    public void ReportRenderWorkTime(long elapsedMilliseconds) {
        WriteEvent(1, elapsedMilliseconds);
        rendererWorkTime.WriteMetric(elapsedMilliseconds);
    }

    public void ReportRenderWaitTime(long elapsedMilliseconds) {
        WriteEvent(2, elapsedMilliseconds);
        renderWaitTime.WriteMetric(elapsedMilliseconds);
    }

    public void ReportSubmitCount(long count) {
        WriteEvent(3, count);
        submitCount.WriteMetric(count);
    }
    
    public void ReportSubmitDisposalCount(long count) {
        WriteEvent(4, count);
        submitDisposalCount.WriteMetric(count);
    }
}
