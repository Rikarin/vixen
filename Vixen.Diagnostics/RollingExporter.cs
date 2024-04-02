using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace Vixen.Diagnostics;

public class RollingExporter : BaseExporter<Metric> {
    readonly string name;

    public RollingExporter(string name = "MyExporter") {
        this.name = name;
    }

    public override ExportResult Export(in Batch<Metric> batch) {
        using var scope = SuppressInstrumentationScope.Begin();

        lock (Profiler.metrics) {
            foreach (var metric in batch) {
                var name = $"{metric.MeterName}.{metric.Name}";
                
                if (!Profiler.metrics.TryGetValue(name, out var value)) {
                    value = CreateQueue(100);
                    Profiler.metrics[name] = value;
                }

                var e = metric.GetMetricPoints().GetEnumerator();
                e.MoveNext();
                var firstPoint = e.Current;
                value.Enqueue(firstPoint.GetHistogramSum() / firstPoint.GetHistogramCount());
            }
            
            Profiler.CapMetrics();
        }

        return ExportResult.Success;
    }

    Queue<double> CreateQueue(int length) {
        var queue = new Queue<double>();
        for (var i = 0; i < length; i++) {
            queue.Enqueue(0);
        }

        return queue;
    }

    protected override bool OnShutdown(int timeoutMilliseconds) {
        Console.WriteLine($"{name}.OnShutdown(timeoutMilliseconds={timeoutMilliseconds})");
        return true;
    }

    protected override void Dispose(bool disposing) {
        Console.WriteLine($"{name}.Dispose({disposing})");
    }
}
