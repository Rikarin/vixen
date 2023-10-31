using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Rin.Diagnostics;

public static class Profiler {
    public static readonly int HistoryLength = 100;
    
    static TracerProvider? tracerProvider;
    static MeterProvider? meterProvider;
    static EventSourcesListener? eventSourcesListener;

    internal static readonly Dictionary<string, Queue<double>> metrics = new();
    // static readonly List<Activity?> traces = new();

    public static bool TryGetMetrics(string name, out float[] value) {
        lock (metrics) {
            if (metrics.TryGetValue(name, out var queue)) {
                value = queue.Select(x => (float)x).ToArray();
                return true;
            }
        }

        value = Array.Empty<float>();
        return false;
    }

    public static void CapMetrics() {
        lock (metrics) {
            foreach (var (name, metric) in metrics) {
                if (metric.Count > HistoryLength) {
                    metric.Dequeue();
                }
            }
        }
    }

    public static void Initialize(Action<MeterProviderBuilder>? configure = null) {
        eventSourcesListener = new();

        tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Rin.*")
            // .AddConsoleExporter()
            // .AddInMemoryExporter(traces)
            .Build();

        // Metrics
        var meterBuilder = Sdk.CreateMeterProviderBuilder().AddMeter("*");
        configure?.Invoke(meterBuilder);
        meterProvider = meterBuilder.Build();
    }

    public static void Shutdown() {
        meterProvider?.Dispose();
        tracerProvider?.Dispose();
        eventSourcesListener?.Dispose();
    }
}
