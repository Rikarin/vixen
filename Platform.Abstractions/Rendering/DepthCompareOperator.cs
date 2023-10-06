namespace Rin.Platform.Abstractions.Rendering;

public enum DepthCompareOperator {
    None = 0, // TODO: remove none?
    Never,
    NotEqual,
    Less,
    LessOrEqual,
    Greater,
    GreaterOrEqual,
    Equal,
    Always
}
