using Rin.Core.Reflection;
using System.Reflection;

namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     Provides tag discovery from a type and type discovery from a tag.
/// </summary>
public interface ITagTypeRegistry : ITagTypeResolver {
    /// <summary>
    ///     Registers an assembly when trying to resolve types. All types
    ///     having <see cref="DataMemberAttribute" /> will be registered
    ///     automatically.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="attributeRegistry">The attribute registry to use when querying for <see cref="DataMemberAttribute" />.</param>
    void RegisterAssembly(Assembly assembly, IAttributeRegistry attributeRegistry);

    /// <summary>
    ///     Register a mapping between a tag and a type.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="type">The type.</param>
    /// <param name="alias">
    ///     if set to <c>true</c> the specified tag is an alias to an existing type that has already a tag
    ///     associated with it (remap).
    /// </param>
    void RegisterTagMapping(string tag, Type type, bool alias);
}
