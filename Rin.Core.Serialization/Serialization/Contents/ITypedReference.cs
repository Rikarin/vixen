namespace Rin.Core.Serialization.Serialization.Contents;

/// <summary>
///     A typed <see cref="IReference" />
/// </summary>
public interface ITypedReference : IReference {
    /// <summary>
    ///     Gets the type of this content reference.
    /// </summary>
    /// <value>The type.</value>
    Type Type { get; }
}
