using Rin.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Rin.Core.Diagnostics;

public static class ApplicationProfiling {
    public static readonly ActivitySource ApplicationSource = new("Rin.Application");
    public static readonly Meter ApplicationMeter = new("Rin.Application");
    
    public static readonly Histogram<double> Initialization = ApplicationMeter.CreateHistogram<double>("Initialization");
    public static readonly Histogram<double> WorkTime = ApplicationMeter.CreateHistogram<double>("WorkTime");
    public static readonly Histogram<double> WaitTime = ApplicationMeter.CreateHistogram<double>("WaitTime");

    public static ProfileScope StartInitialization() => new(Initialization, ApplicationSource.StartActivity("Initialization"));
    public static ProfileScope StartWorkTime() => new(WorkTime, ApplicationSource.StartActivity("Work"));
    public static ProfileScope StartWaitTime() => new(WaitTime, ApplicationSource.StartActivity("Wait"));
}
