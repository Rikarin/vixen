using System.Diagnostics.Tracing;

sealed class EventSourcesListener : EventListener {
    protected override void OnEventSourceCreated(EventSource eventSource) {
        base.OnEventSourceCreated(eventSource);

        // EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All);
        Console.WriteLine($"New event source: {eventSource.Name}");
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData) {
        base.OnEventWritten(eventData);
        // Console.WriteLine($"event data {eventData.EventName}");
    }
}
