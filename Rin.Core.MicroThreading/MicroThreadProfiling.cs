using Rin.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Rin.Core.MicroThreading;

public static class MicroThreadProfiling {
    public static readonly ActivitySource MicroThreadingSource = new("Rin.Core.MicroThreading");
    public static readonly Meter MicroThreadingMeter = new("Rin.Core.MicroThreading");
    
    public static readonly Histogram<double> MicroThread = MicroThreadingMeter.CreateHistogram<double>("MicroThread");

    public static readonly ProfilingKey ProfilingKey = new(MicroThread, MicroThreadingSource.StartActivity("MicroThread"));
}
