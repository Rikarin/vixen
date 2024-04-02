using System.Diagnostics;
using System.Diagnostics.Metrics;
using Vixen.Diagnostics;

namespace Vixen.Platform.Common.Diagnostics;

public static class RendererProfiling {
    public static readonly ActivitySource ApplicationSource = new("Vixen.Renderer");
    public static readonly Meter ApplicationMeter = new("Vixen.Renderer");
    
    public static readonly Histogram<double> WorkTime = ApplicationMeter.CreateHistogram<double>("WorkTime");
    public static readonly Histogram<double> WaitTime = ApplicationMeter.CreateHistogram<double>("WaitTime");

    public static readonly Histogram<int> SubmitCount = ApplicationMeter.CreateHistogram<int>("SubmitCount");
    // public static readonly UpDownCounter<int> SubmitCount = ApplicationMeter.CreateUpDownCounter<int>("SubmitCount");
    public static readonly Histogram<int> SubmitDisposalCount = ApplicationMeter.CreateHistogram<int>("SubmitDisposalCount");

    public static ProfileScope StartWorkTime() => new(WorkTime, ApplicationSource.StartActivity("Work"));
    public static ProfileScope StartWaitTime() => new(WaitTime, ApplicationSource.StartActivity("Wait"));
}
