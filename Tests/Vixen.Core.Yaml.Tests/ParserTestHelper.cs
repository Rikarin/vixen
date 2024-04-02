using Vixen.Core.Yaml.Events;
using Vixen.Core.Yaml.Tokens;
using AnchorAlias = Vixen.Core.Yaml.Events.AnchorAlias;
using DocumentEnd = Vixen.Core.Yaml.Events.DocumentEnd;
using DocumentStart = Vixen.Core.Yaml.Events.DocumentStart;
using Scalar = Vixen.Core.Yaml.Events.Scalar;
using StreamEnd = Vixen.Core.Yaml.Events.StreamEnd;
using StreamStart = Vixen.Core.Yaml.Events.StreamStart;


namespace Vixen.Core.Yaml.Tests;

public class ParserTestHelper : YamlTest {
    protected const bool Explicit = false;
    protected const bool Implicit = true;
    protected const string TagYaml = "tag:yaml.org,2002:";

    protected static readonly TagDirective[] DefaultTags = [new("!", "!"), new("!!", TagYaml)];

    protected static StreamStart StreamStart => new();

    protected static StreamEnd StreamEnd => new();

    protected SequenceStart BlockSequenceStart => new(null, null, true, DataStyle.Normal);

    protected SequenceStart FlowSequenceStart => new(null, null, true, DataStyle.Compact);

    protected SequenceEnd SequenceEnd => new();

    protected MappingStart BlockMappingStart => new(null, null, true, DataStyle.Normal);

    protected MappingStart FlowMappingStart => new(null, null, true, DataStyle.Compact);

    protected MappingEnd MappingEnd => new();

    protected DocumentStart DocumentStart(bool isImplicit) => DocumentStart(isImplicit, null, DefaultTags);

    protected DocumentStart DocumentStart(bool isImplicit, VersionDirective version, params TagDirective[] tags) =>
        new(version, new(tags), isImplicit);

    protected VersionDirective Version(int major, int minor) => new(new(major, minor));

    protected TagDirective TagDirective(string handle, string prefix) => new(handle, prefix);

    protected DocumentEnd DocumentEnd(bool isImplicit) => new(isImplicit);

    protected Scalar PlainScalar(string text) => new(null, null, text, ScalarStyle.Plain, true, false);

    protected Scalar SingleQuotedScalar(string text) => new(null, null, text, ScalarStyle.SingleQuoted, false, true);

    protected Scalar DoubleQuotedScalar(string text) => DoubleQuotedScalar(null, text);

    protected Scalar ExplicitDoubleQuotedScalar(string tag, string text) => DoubleQuotedScalar(tag, text, false);

    protected Scalar DoubleQuotedScalar(string tag, string text, bool quotedImplicit = true) =>
        new(null, tag, text, ScalarStyle.DoubleQuoted, false, quotedImplicit);

    protected Scalar LiteralScalar(string text) => new(null, null, text, ScalarStyle.Literal, false, true);

    protected Scalar FoldedScalar(string text) => new(null, null, text, ScalarStyle.Folded, false, true);

    protected SequenceStart AnchoredFlowSequenceStart(string anchor) => new(anchor, null, true, DataStyle.Compact);

    protected MappingStart TaggedBlockMappingStart(string tag) => new(null, tag, false, DataStyle.Normal);

    protected AnchorAlias AnchorAlias(string alias) => new(alias);
}
