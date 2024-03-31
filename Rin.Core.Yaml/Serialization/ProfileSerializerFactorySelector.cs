using System.Reflection;

namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     A factory selector that will select factories depending on the profiles specified in the
///     <see cref="YamlSerializerFactoryAttribute" />.
/// </summary>
public class ProfileSerializerFactorySelector(params string[]? profiles) : SerializerFactorySelector {
    static readonly string[] EmptyProfiles = Array.Empty<string>();
    readonly string[] profiles = profiles ?? EmptyProfiles;

    protected override bool CanAddSerializerFactory(IYamlSerializableFactory factory) {
        var attribute = factory.GetType().GetCustomAttribute<YamlSerializerFactoryAttribute>();
        if (attribute == null) {
            return profiles.Any(
                x => YamlSerializerFactoryAttribute.AreProfilesEqual(x, YamlSerializerFactoryAttribute.Default)
            );
        }

        return profiles.Any(x => attribute.ContainsProfile(x));
    }
}
