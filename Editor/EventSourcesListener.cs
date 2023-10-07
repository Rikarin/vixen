using Serilog;
using System.Diagnostics.Tracing;

sealed class EventSourcesListener : EventListener {
    static (string counterName, double counterValue) GetRelevantMetric(IDictionary<string, object> eventPayload) {
        var counterName = "";
        double counterValue = 0f;

        if (eventPayload.TryGetValue("DisplayName", out var displayValue)) {
            counterName = displayValue.ToString();
        }

        if (eventPayload.TryGetValue("Mean", out object value) || eventPayload.TryGetValue("Increment", out value)) {
            counterValue = Convert.ToDouble(value);
        }

        return (counterName, counterValue);
    }

    protected override void OnEventSourceCreated(EventSource eventSource) {
        Console.WriteLine(eventSource.Name);

        // TODO: system runtime is for GC, mem, cpu, etc.
        // if (!eventSource.Name.Equals("System.Runtime")) {
        if (!eventSource.Name.StartsWith("Rin.")) {
            return;
        }

        EnableEvents(
            eventSource,
            EventLevel.Verbose,
            EventKeywords.All,
            new Dictionary<string, string?> { ["EventCounterIntervalSec"] = "1" }
        );
        Console.WriteLine($"New event source: {eventSource.Name}");
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData) {
        base.OnEventWritten(eventData);
        if (!eventData.EventName!.Equals("EventCounters")) {
            return;
        }

        for (var i = 0; i < eventData.Payload!.Count; ++i) {
            if (eventData.Payload[i] is IDictionary<string, object> eventPayload) {
                var (counterName, counterValue) = GetRelevantMetric(eventPayload);
                Log.Information("{CounterName}: {CounterValue}", counterName, counterValue);
            }
        }
    }
}
