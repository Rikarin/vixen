using Rin.Core.Yaml.Events;

namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     Provided the base implementation for an IEventEmitter that is a
///     decorator for another IEventEmitter.
/// </summary>
abstract class ChainedEventEmitter : IEventEmitter {
    protected readonly IEventEmitter nextEmitter;

    protected ChainedEventEmitter(IEventEmitter nextEmitter) {
        if (nextEmitter == null) {
            throw new ArgumentNullException(nameof(nextEmitter));
        }

        this.nextEmitter = nextEmitter;
    }

    public virtual void StreamStart() {
        nextEmitter.StreamStart();
    }

    public virtual void DocumentStart() {
        nextEmitter.DocumentStart();
    }

    public virtual void Emit(AliasEventInfo eventInfo) {
        nextEmitter.Emit(eventInfo);
    }

    public virtual void Emit(ScalarEventInfo eventInfo) {
        nextEmitter.Emit(eventInfo);
    }

    public virtual void Emit(MappingStartEventInfo eventInfo) {
        nextEmitter.Emit(eventInfo);
    }

    public virtual void Emit(MappingEndEventInfo eventInfo) {
        nextEmitter.Emit(eventInfo);
    }

    public virtual void Emit(SequenceStartEventInfo eventInfo) {
        nextEmitter.Emit(eventInfo);
    }

    public virtual void Emit(SequenceEndEventInfo eventInfo) {
        nextEmitter.Emit(eventInfo);
    }

    public virtual void Emit(ParsingEvent parsingEvent) {
        nextEmitter.Emit(parsingEvent);
    }

    public virtual void DocumentEnd() {
        nextEmitter.DocumentEnd();
    }

    public virtual void StreamEnd() {
        nextEmitter.StreamEnd();
    }
}
