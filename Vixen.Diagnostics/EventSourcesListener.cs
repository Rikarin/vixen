using System.Diagnostics.Tracing;

namespace Vixen.Diagnostics;

sealed class EventSourcesListener : EventListener {
    protected override void OnEventSourceCreated(EventSource eventSource) {
        if (!eventSource.Name.Equals("System.Runtime")) {
            return;
        }

        EnableEvents(
            eventSource,
            EventLevel.Verbose,
            EventKeywords.All,
            new Dictionary<string, string?> { ["EventCounterIntervalSec"] = "1" }
        );
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData) {
        base.OnEventWritten(eventData);
        if (!eventData.EventName!.Equals("EventCounters")) {
            return;
        }

        for (var i = 0; i < eventData.Payload!.Count; ++i) {
            if (eventData.Payload[i] is IDictionary<string, object> eventPayload) {
                ProfilerData.PushData(eventPayload);
            }
        }
    }
}

public static class ProfilerData {
    public static Dictionary<string, ProfilerDataEntry> Data { get; } = new();

    public static void PushData(IDictionary<string, object> data) {
        var key = data["Name"].ToString()!;
        var mean = Convert.ToSingle(data["Mean"]);
        
        if (!Data.TryGetValue(key, out var entry)) {
            entry = new();
            Data[key] = entry;
        }

        // TODO: optimize by SIMD
        for (var i = 0; i < entry.MeanData.Length - 1; i++) {
            entry.MeanData[i] = entry.MeanData[i + 1];
        }

        entry.MeanData[^1] = mean;
        entry.DisplayName = data["DisplayName"].ToString() ?? "Unknown";
        entry.DisplayUnits = data["DisplayUnits"].ToString();
        entry.Min = Convert.ToSingle(data["Min"]);
        entry.Max = Convert.ToSingle(data["Max"]);
    }
}

public sealed class ProfilerDataEntry {
    public string DisplayName { get; set; }
    public string? DisplayUnits { get; set; }
    public float[] MeanData { get; } = new float[60];
    
    public float Min { get; set; }
    public float Max { get; set; }
}