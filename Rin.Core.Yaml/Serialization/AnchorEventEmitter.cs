using Rin.Core.Yaml.Events;

namespace Rin.Core.Yaml.Serialization;

class AnchorEventEmitter(IEventEmitter nextEmitter) : ChainedEventEmitter(nextEmitter) {
    readonly List<object> events = [];
    readonly HashSet<string> alias = [];

    public override void Emit(AliasEventInfo eventInfo) {
        alias.Add(eventInfo.Alias);
        events.Add(eventInfo);
    }

    public override void Emit(ScalarEventInfo eventInfo) {
        events.Add(eventInfo);
    }

    public override void Emit(MappingStartEventInfo eventInfo) {
        events.Add(eventInfo);
    }

    public override void Emit(MappingEndEventInfo eventInfo) {
        events.Add(eventInfo);
    }

    public override void Emit(SequenceStartEventInfo eventInfo) {
        events.Add(eventInfo);
    }

    public override void Emit(SequenceEndEventInfo eventInfo) {
        events.Add(eventInfo);
    }

    public override void Emit(ParsingEvent parsingEvent) {
        events.Add(parsingEvent);
    }

    public override void DocumentEnd() {
        // remove all unused anchor
        foreach (var objectEventInfo in events.OfType<ObjectEventInfo>()) {
            if (objectEventInfo.Anchor != null && !alias.Contains(objectEventInfo.Anchor)) {
                objectEventInfo.Anchor = null;
            }
        }

        // Flush all events to emitter.
        foreach (var evt in events) {
            if (evt is AliasEventInfo info) {
                nextEmitter.Emit(info);
            } else if (evt is ScalarEventInfo eventInfo) {
                nextEmitter.Emit(eventInfo);
            } else if (evt is MappingStartEventInfo startEventInfo) {
                nextEmitter.Emit(startEventInfo);
            } else if (evt is MappingEndEventInfo endEventInfo) {
                nextEmitter.Emit(endEventInfo);
            } else if (evt is SequenceStartEventInfo sequenceStartEventInfo) {
                nextEmitter.Emit(sequenceStartEventInfo);
            } else if (evt is SequenceEndEventInfo sequenceEndEventInfo) {
                nextEmitter.Emit(sequenceEndEventInfo);
            } else if (evt is ParsingEvent parsingEvent) {
                nextEmitter.Emit(parsingEvent);
            }
        }

        nextEmitter.DocumentEnd();
    }
}
