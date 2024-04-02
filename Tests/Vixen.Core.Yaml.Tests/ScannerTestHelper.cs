using Vixen.Core.Yaml.Tokens;

namespace Vixen.Core.Yaml.Tests;

public class ScannerTestHelper : YamlTest {
    protected static StreamStart StreamStart => new();

    protected static StreamEnd StreamEnd => new();

    protected static DocumentStart DocumentStart => new();

    protected static DocumentEnd DocumentEnd => new();

    protected static FlowSequenceStart FlowSequenceStart => new();

    protected static FlowSequenceEnd FlowSequenceEnd => new();

    protected static BlockSequenceStart BlockSequenceStart => new();

    protected static FlowMappingStart FlowMappingStart => new();

    protected static FlowMappingEnd FlowMappingEnd => new();

    protected static BlockMappingStart BlockMappingStart => new();

    protected static Key Key => new();

    protected static Value Value => new();

    protected static FlowEntry FlowEntry => new();

    protected static BlockEntry BlockEntry => new();

    protected static BlockEnd BlockEnd => new();

    protected static VersionDirective VersionDirective(int major, int minor) => new(new(major, minor));

    protected static TagDirective TagDirective(string handle, string prefix) => new(handle, prefix);

    protected static Tag Tag(string handle, string suffix) => new(handle, suffix);

    protected static Scalar PlainScalar(string text) => new(text, ScalarStyle.Plain);

    protected static Scalar SingleQuotedScalar(string text) => new(text, ScalarStyle.SingleQuoted);

    protected static Scalar DoubleQuotedScalar(string text) => new(text, ScalarStyle.DoubleQuoted);

    protected static Scalar LiteralScalar(string text) => new(text, ScalarStyle.Literal);

    protected static Scalar FoldedScalar(string text) => new(text, ScalarStyle.Folded);

    protected static Anchor Anchor(string anchor) => new(anchor);

    protected static AnchorAlias AnchorAlias(string alias) => new(alias);
}
