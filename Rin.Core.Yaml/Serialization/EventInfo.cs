namespace Rin.Core.Yaml.Serialization;

public abstract class EventInfo {
    public object SourceValue { get; private set; }
    public Type SourceType { get; private set; }

    protected EventInfo(object sourceValue, Type sourceType) {
        SourceValue = sourceValue;
        SourceType = sourceType;
    }
}

public class AliasEventInfo : EventInfo {
    public string Alias { get; set; }

    public AliasEventInfo(object sourceValue, Type sourceType) : base(sourceValue, sourceType) { }
}

public class ObjectEventInfo : EventInfo {
    public string Anchor { get; set; }
    public string Tag { get; set; }

    protected ObjectEventInfo(object sourceValue, Type sourceType) : base(sourceValue, sourceType) { }
}

public sealed class ScalarEventInfo : ObjectEventInfo {
    public string RenderedValue { get; set; }
    public ScalarStyle Style { get; set; }
    public bool IsPlainImplicit { get; set; }
    public bool IsQuotedImplicit { get; set; }

    public ScalarEventInfo(object sourceValue, Type sourceType) : base(sourceValue, sourceType) { }
}

public sealed class MappingStartEventInfo(object sourceValue, Type sourceType)
    : ObjectEventInfo(sourceValue, sourceType) {
    public bool IsImplicit { get; set; }
    public DataStyle Style { get; set; }
}

public sealed class MappingEndEventInfo(object sourceValue, Type sourceType) : EventInfo(sourceValue, sourceType);

public sealed class SequenceStartEventInfo(object sourceValue, Type sourceType)
    : ObjectEventInfo(sourceValue, sourceType) {
    public bool IsImplicit { get; set; }
    public DataStyle Style { get; set; }
}

public sealed class SequenceEndEventInfo(object sourceValue, Type sourceType) : EventInfo(sourceValue, sourceType);
