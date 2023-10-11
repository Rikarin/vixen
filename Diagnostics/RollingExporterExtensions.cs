using OpenTelemetry.Metrics;

namespace Rin.Diagnostics;

public static class RollingExporterExtensions {
    public static MeterProviderBuilder AddRollingExporter(
        this MeterProviderBuilder builder,
        int exportIntervalMilliSeconds = Timeout.Infinite
    ) {
        if (builder == null) {
            throw new ArgumentNullException(nameof(builder));
        }

        if (exportIntervalMilliSeconds == Timeout.Infinite) {
            // Export triggered manually only.
            return builder.AddReader(new BaseExportingMetricReader(new RollingExporter()));
        }

        // Export is triggered periodically.
        return builder.AddReader(new PeriodicExportingMetricReader(new RollingExporter(), exportIntervalMilliSeconds));
    }
}
