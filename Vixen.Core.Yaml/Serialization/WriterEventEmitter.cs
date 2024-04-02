using Vixen.Core.Yaml.Events;

namespace Vixen.Core.Yaml.Serialization;

sealed class WriterEventEmitter(IEmitter emitter) : IEventEmitter {
    public void Emit(ParsingEvent parsingEvent) {
        emitter.Emit(parsingEvent);
    }

    void IEventEmitter.StreamStart() {
        emitter.Emit(new StreamStart());
    }

    void IEventEmitter.DocumentStart() {
        emitter.Emit(new DocumentStart());
    }

    void IEventEmitter.Emit(AliasEventInfo eventInfo) {
        emitter.Emit(new AnchorAlias(eventInfo.Alias));
    }

    void IEventEmitter.Emit(ScalarEventInfo eventInfo) {
        emitter.Emit(
            new Scalar(
                eventInfo.Anchor,
                eventInfo.Tag,
                eventInfo.RenderedValue,
                eventInfo.Style,
                eventInfo.IsPlainImplicit,
                eventInfo.IsQuotedImplicit
            )
        );
    }

    void IEventEmitter.Emit(MappingStartEventInfo eventInfo) {
        emitter.Emit(new MappingStart(eventInfo.Anchor, eventInfo.Tag, eventInfo.IsImplicit, eventInfo.Style));
    }

    void IEventEmitter.Emit(MappingEndEventInfo eventInfo) {
        emitter.Emit(new MappingEnd());
    }

    void IEventEmitter.Emit(SequenceStartEventInfo eventInfo) {
        emitter.Emit(new SequenceStart(eventInfo.Anchor, eventInfo.Tag, eventInfo.IsImplicit, eventInfo.Style));
    }

    void IEventEmitter.Emit(SequenceEndEventInfo eventInfo) {
        emitter.Emit(new SequenceEnd());
    }

    void IEventEmitter.DocumentEnd() {
        emitter.Emit(new DocumentEnd(true));
    }

    void IEventEmitter.StreamEnd() {
        emitter.Emit(new StreamEnd());
    }
}
