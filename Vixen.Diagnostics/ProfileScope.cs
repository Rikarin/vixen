using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Vixen.Diagnostics;

public readonly struct ProfileScope : IDisposable {
    readonly Histogram<double> reportHistogram;
    readonly Activity? activity;

    public ProfileScope(Histogram<double> reportHistogram, Activity? activity) {
        this.reportHistogram = reportHistogram;
        this.activity = activity;
    }
        
    public void Dispose() {
        if (activity == null) {
            return;
        }
            
        activity.Stop();
        reportHistogram.Record(activity.Duration.TotalMilliseconds);
        activity.Dispose();
    }
}