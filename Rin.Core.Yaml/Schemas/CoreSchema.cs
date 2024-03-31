using System.Globalization;

namespace Rin.Core.Yaml.Schemas;

/// <summary>
///     Implements the Core schema. <see cref="http://www.yaml.org/spec/1.2/spec.html#id2804356" />
/// </summary>
/// <remarks>
///     The Core schema is an extension of the JSON schema, allowing for more human-readable presentation of the same
///     types.
///     This is the recommended default schema that YAML processor should use unless instructed otherwise.
///     It is also strongly recommended that other schemas should be based on it.
/// </remarks>
public class CoreSchema : JsonSchema {
    protected override void PrepareScalarRules() {
        // 10.2.1.1. Null
        AddScalarRule<object>("!!null", @"null|Null|NULL|\~", _ => null, null);

        AddScalarRule("!!bool", "true|True|TRUE", _ => true, null);
        AddScalarRule("!!bool", "false|False|FALSE", _ => false, null);

        AddScalarRule(
            [typeof(ulong), typeof(long), typeof(int)],
            "!!int",
            "([-+]?(0|[1-9][0-9_]*))",
            DecodeInteger,
            null
        );

        // Make float before 0x/0o to improve parsing as float are more common than 0x and 0o
        AddScalarRule(
            "!!float",
            @"[-+]?(\.[0-9]+|[0-9]+(\.[0-9]*)?)([eE][-+]?[0-9]+)?",
            m => Convert.ToDouble(m.Value.Replace("_", ""), CultureInfo.InvariantCulture),
            null
        );

        AddScalarRule(
            "!!int",
            "0x([0-9a-fA-F_]+)",
            m => Convert.ToInt32(m.Groups[1].Value.Replace("_", ""), 16),
            null
        );

        AddScalarRule("!!int", "0o([0-7_]+)", m => Convert.ToInt32(m.Groups[1].Value.Replace("_", ""), 8), null);

        AddScalarRule("!!float", @"\+?(\.inf|\.Inf|\.INF)", _ => double.PositiveInfinity, null);
        AddScalarRule("!!float", @"-(\.inf|\.Inf|\.INF)", _ => double.NegativeInfinity, null);
        AddScalarRule("!!float", @"\.nan|\.NaN|\.NAN", _ => double.NaN, null);

        AllowFailsafeString = true;

        // We are not calling the base as we want to completely override scalar rules
        // and in order to have a more concise set of regex
    }
}
