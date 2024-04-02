using Vixen.Core.Yaml.Tokens;

namespace Vixen.Core.Yaml;

/// <summary>
///     Defines constants thar relate to the YAML specification.
/// </summary>
static class Constants {
    public const int MajorVersion = 1;
    public const int MinorVersion = 1;

    public const char HandleCharacter = '!';
    public const string DefaultHandle = "!";

    public static readonly TagDirective[] DefaultTagDirectives = [new("!", "!"), new("!!", "tag:yaml.org,2002:")];
}
