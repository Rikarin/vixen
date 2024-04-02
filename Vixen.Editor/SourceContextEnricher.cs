using Serilog.Core;
using Serilog.Events;

namespace Vixen.Editor;

class SourceContextEnricher : ILogEventEnricher {
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
        if (logEvent.Properties.TryGetValue("SourceContext", out var property)) {
            var scalarValue = property as ScalarValue;
            var value = scalarValue?.Value as string;

            if (value?.StartsWith("Rin") ?? false) {
                var lastElement = value.Split(".").LastOrDefault();
                if (!string.IsNullOrWhiteSpace(lastElement)) {
                    logEvent.AddOrUpdateProperty(new("SourceContext", new ScalarValue($"[{lastElement}]")));
                }
            }
        }
    }
}