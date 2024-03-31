using Rin.Core.Yaml.Events;
using System.Text.RegularExpressions;

namespace Rin.Core.Yaml.Schemas;

/// <summary>
///     Base implementation for a based schema.
/// </summary>
public abstract class SchemaBase : IYamlSchema {
    /// <summary>
    ///     The string short tag: !!str
    /// </summary>
    public const string StrShortTag = "!!str";

    /// <summary>
    ///     The string long tag: tag:yaml.org,2002:str
    /// </summary>
    public const string StrLongTag = "tag:yaml.org,2002:str";

    readonly Dictionary<string, string> shortTagToLongTag = new();
    readonly Dictionary<string, string> longTagToShortTag = new();
    readonly List<ScalarResolutionRule> scalarTagResolutionRules = new();
    readonly Dictionary<string, Regex> algorithms = new();

    readonly Dictionary<string, List<ScalarResolutionRule>> mapTagToScalarResolutionRuleList = new();

    readonly Dictionary<Type, List<ScalarResolutionRule>> mapTypeToScalarResolutionRuleList = new();

    readonly Dictionary<Type, string> mapTypeToShortTag = new();
    readonly Dictionary<string, Type> mapShortTagToType = new();

    int updateCounter;
    bool needFirstUpdate = true;

    protected SchemaBase() {
        RegisterDefaultTagMappings();
    }

    public string? ExpandTag(string? shortTag) {
        if (shortTag == null) {
            return null;
        }

        return shortTagToLongTag.TryGetValue(shortTag, out var tagExpanded) ? tagExpanded : shortTag;
    }

    public string? ShortenTag(string? longTag) {
        if (longTag == null) {
            return null;
        }

        return longTagToShortTag.TryGetValue(longTag, out var tagShortened) ? tagShortened : longTag;
    }

    public string GetDefaultTag(NodeEvent nodeEvent) {
        EnsureScalarRules();

        if (nodeEvent == null) {
            throw new ArgumentNullException(nameof(nodeEvent));
        }

        if (nodeEvent is MappingStart mapping) {
            return GetDefaultTag(mapping);
        }

        if (nodeEvent is SequenceStart sequence) {
            return GetDefaultTag(sequence);
        }

        if (nodeEvent is Scalar scalar) {
            TryParse(scalar, false, out var tag, out _);
            return tag;
        }

        throw new NotSupportedException($"NodeEvent [{nodeEvent.GetType().FullName}] not supported");
    }

    public string GetDefaultTag(Type type) {
        if (type == null) {
            throw new ArgumentNullException(nameof(type));
        }

        EnsureScalarRules();

        mapTypeToShortTag.TryGetValue(type, out var defaultTag);
        return defaultTag;
    }

    public bool IsTagImplicit(string tag) {
        if (tag == null) {
            return true;
        }

        return shortTagToLongTag.ContainsKey(tag);
    }

    /// <summary>
    ///     Registers a long/short tag association.
    /// </summary>
    /// <param name="shortTag">The short tag.</param>
    /// <param name="longTag">The long tag.</param>
    /// <exception cref="System.ArgumentNullException">
    ///     shortTag
    ///     or
    ///     shortTag
    /// </exception>
    public void RegisterTag(string shortTag, string longTag) {
        if (shortTag == null) {
            throw new ArgumentNullException(nameof(shortTag));
        }

        if (longTag == null) {
            throw new ArgumentNullException(nameof(longTag));
        }

        shortTagToLongTag[shortTag] = longTag;
        longTagToShortTag[longTag] = shortTag;
    }

    public virtual bool TryParse(Scalar scalar, bool parseValue, out string defaultTag, out object value) {
        if (scalar == null) {
            throw new ArgumentNullException(nameof(scalar));
        }

        EnsureScalarRules();

        defaultTag = null;
        value = null;

        // DoubleQuoted and SingleQuoted string are always decoded
        if (scalar.Style is ScalarStyle.DoubleQuoted or ScalarStyle.SingleQuoted) {
            defaultTag = StrShortTag;
            if (parseValue) {
                value = scalar.Value;
            }

            return true;
        }

        // Parse only values if we have some rules
        if (scalarTagResolutionRules.Count > 0) {
            foreach (var rule in scalarTagResolutionRules) {
                var match = rule.Pattern.Match(scalar.Value);
                if (!match.Success) {
                    continue;
                }

                defaultTag = rule.Tag;
                if (parseValue) {
                    value = rule.Decode(match);
                }

                return true;
            }
        } else {
            // Expand the tag to a default tag.
            defaultTag = ShortenTag(scalar.Tag);
        }

        // Value was not successfully decoded
        return false;
    }

    public bool TryParse(Scalar scalar, Type type, out object value) {
        if (scalar == null) {
            throw new ArgumentNullException(nameof(scalar));
        }

        if (type == null) {
            throw new ArgumentNullException(nameof(type));
        }

        EnsureScalarRules();

        value = null;

        // DoubleQuoted and SingleQuoted string are always decoded
        if (type == typeof(string) && scalar.Style is ScalarStyle.DoubleQuoted or ScalarStyle.SingleQuoted) {
            value = scalar.Value;
            return true;
        }

        // Parse only values if we have some rules
        if (mapTypeToScalarResolutionRuleList.Count > 0) {
            if (mapTypeToScalarResolutionRuleList.TryGetValue(type, out var rules)) {
                foreach (var rule in rules) {
                    var match = rule.Pattern.Match(scalar.Value);
                    if (match.Success) {
                        value = rule.Decode(match);
                        return true;
                    }
                }
            }
        }

        // Value was not successfully decoded
        return false;
    }

    public Type GetTypeForDefaultTag(string shortTag) {
        if (shortTag == null) {
            return null;
        }

        mapShortTagToType.TryGetValue(shortTag, out var type);
        return type;
    }

    void EnsureScalarRules() {
        lock (this) {
            if (needFirstUpdate || updateCounter != scalarTagResolutionRules.Count) {
                PrepareScalarRules();
                Update();
                needFirstUpdate = false;
            }
        }
    }

    void Update() {
        // Tag to joined regexp source
        var mapTagToPartialRegexPattern = new Dictionary<string, string>();
        foreach (var rule in scalarTagResolutionRules) {
            if (!mapTagToPartialRegexPattern.ContainsKey(rule.Tag)) {
                mapTagToPartialRegexPattern.Add(rule.Tag, rule.PatternSource);
            } else {
                mapTagToPartialRegexPattern[rule.Tag] += "|" + rule.PatternSource;
            }
        }

        // Tag to joined regexp
        algorithms.Clear();
        foreach (var entry in mapTagToPartialRegexPattern) {
            algorithms.Add(
                entry.Key,
                new("^(" + entry.Value + ")$")
            );
        }

        // Tag to decoding methods
        mapTagToScalarResolutionRuleList.Clear();
        foreach (var rule in scalarTagResolutionRules) {
            if (!mapTagToScalarResolutionRuleList.TryGetValue(rule.Tag, out var value)) {
                value = [];
                mapTagToScalarResolutionRuleList[rule.Tag] = value;
            }

            value.Add(rule);
        }

        mapTypeToScalarResolutionRuleList.Clear();
        foreach (var rule in scalarTagResolutionRules) {
            var types = rule.GetTypeOfValue();
            foreach (var type in types) {
                if (!mapTypeToScalarResolutionRuleList.TryGetValue(type, out var value)) {
                    value = [];
                    mapTypeToScalarResolutionRuleList[type] = value;
                }

                value.Add(rule);
            }
        }

        // Update the counter
        updateCounter = scalarTagResolutionRules.Count;
    }

    /// <summary>
    ///     Gets the default tag for a <see cref="MappingStart" /> event.
    /// </summary>
    /// <param name="nodeEvent">The node event.</param>
    /// <returns>The default tag for a map.</returns>
    protected abstract string GetDefaultTag(MappingStart nodeEvent);

    /// <summary>
    ///     Gets the default tag for a <see cref="SequenceStart" /> event.
    /// </summary>
    /// <param name="nodeEvent">The node event.</param>
    /// <returns>The default tag for a seq.</returns>
    protected abstract string GetDefaultTag(SequenceStart nodeEvent);

    /// <summary>
    ///     Prepare scalar rules. In the implementation of this method, should call <see cref="AddScalarRule{T}" />
    /// </summary>
    protected virtual void PrepareScalarRules() { }

    /// <summary>
    ///     Add a tag resolution rule that is invoked when <paramref name="regex" /> matches
    ///     the <see cref="Scalar">Value of</see> a <see cref="Scalar" /> node.
    ///     The tag is resolved to <paramref name="tag" /> and <paramref name="decode" /> is
    ///     invoked when actual value of type <typeparamref name="T" /> is extracted from
    ///     the node text.
    /// </summary>
    /// <typeparam name="T">Type of the scalar</typeparam>
    /// <param name="tag">The tag.</param>
    /// <param name="regex">The regex.</param>
    /// <param name="decode">The decode function.</param>
    /// <param name="encode">The encode function.</param>
    /// <example>
    ///     <code>
    /// BeginUpdate(); // to avoid invoking slow internal calculation method many times.
    /// Add( ... );
    /// Add( ... );
    /// Add( ... );
    /// Add( ... );
    /// EndUpdate();   // automaticall invoke internal calculation method
    ///   </code>
    /// </example>
    protected void AddScalarRule<T>(string tag, string regex, Func<Match, T> decode, Func<T, string> encode) {
        // Make sure the tag is expanded to its long form
        var longTag = ShortenTag(tag);
        scalarTagResolutionRules.Add(new(longTag, regex, m => decode(m), m => encode((T)m), typeof(T)));
    }

    protected void AddScalarRule(
        Type[] types,
        string tag,
        string regex,
        Func<Match, object> decode,
        Func<object, string> encode
    ) {
        // Make sure the tag is expanded to its long form
        var longTag = ShortenTag(tag);
        scalarTagResolutionRules.Add(new(longTag, regex, decode, encode, types));
    }

    protected void RegisterDefaultTagMapping<T>(string tag, bool isDefault = false) {
        if (tag == null) {
            throw new ArgumentNullException(nameof(tag));
        }

        RegisterDefaultTagMapping(tag, typeof(T), isDefault);
    }

    protected void RegisterDefaultTagMapping(string tag, Type type, bool isDefault) {
        if (tag == null) {
            throw new ArgumentNullException(nameof(tag));
        }

        if (type == null) {
            throw new ArgumentNullException(nameof(type));
        }

        mapTypeToShortTag.TryAdd(type, tag);
        if (isDefault) {
            mapShortTagToType[tag] = type;
        }
    }

    /// <summary>
    ///     Allows to register tag mapping for all primitive types (e.g. int -> !!int)
    /// </summary>
    protected virtual void RegisterDefaultTagMappings() { }

    class ScalarResolutionRule {
        readonly Type[] types;
        readonly Func<Match, object> Decoder;
        readonly Func<object, string> Encoder;

        public string Tag { get; protected set; }
        public Regex Pattern { get; protected set; }
        public string PatternSource { get; protected set; }

        public ScalarResolutionRule(
            string shortTag,
            string regex,
            Func<Match, object> decoder,
            Func<object, string> encoder,
            params Type[] types
        ) {
            Tag = shortTag;
            PatternSource = regex;
            Pattern = new("^(?:" + regex + ")$");
            this.types = types;
            Decoder = decoder;
            Encoder = encoder;
        }

        public object Decode(Match m) => Decoder(m);
        public string Encode(object obj) => Encoder(obj);
        public Type[] GetTypeOfValue() => types;
        public bool HasEncoder() => Encoder != null;
        public bool IsMatch(string value) => Pattern.IsMatch(value);
    }
}
