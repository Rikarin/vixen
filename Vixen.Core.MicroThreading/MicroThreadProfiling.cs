using System.Diagnostics;
using System.Diagnostics.Metrics;
using Vixen.Diagnostics;

namespace Vixen.Core.MicroThreading;

public static class MicroThreadProfiling {
    public static readonly ActivitySource MicroThreadingSource = new("Vixen.Core.MicroThreading");
    public static readonly Meter MicroThreadingMeter = new("Vixen.Core.MicroThreading");
    
    public static readonly Histogram<double> MicroThread = MicroThreadingMeter.CreateHistogram<double>("MicroThread");

    public static readonly ProfilingKey ProfilingKey = new(MicroThread, MicroThreadingSource.StartActivity("MicroThread"));
}
