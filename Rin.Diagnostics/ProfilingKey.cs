using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Rin.Diagnostics;

public readonly struct ProfilingKey {
    readonly Histogram<double> reportHistogram;
    readonly Activity? activity;

    public ProfilingKey(Histogram<double> reportHistogram, Activity? activity) {
        this.reportHistogram = reportHistogram;
        this.activity = activity;
    }

    public ProfileScope Begin() => new(reportHistogram, activity);
}
