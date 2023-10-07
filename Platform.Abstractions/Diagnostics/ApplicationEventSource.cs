using System.Diagnostics.Tracing;

namespace Rin.Platform.Abstractions.Diagnostics;

[EventSource(Name = "Rin.Rendering")]
public sealed class RenderingEventSource : EventSource {
    public static readonly RenderingEventSource Log = new();

    readonly EventCounter rendererWorkTime;
    readonly EventCounter renderWaitTime;

    public static Watcher RenderWaitTime => new(Log.ReportRenderWaitTime);
    public static Watcher RenderWorkTime => new(Log.ReportRenderWorkTime);

    RenderingEventSource() {
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
}
