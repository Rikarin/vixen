namespace Vixen.Core;

/// <summary>
///     Base interface for all identifiable instances.
/// </summary>
public interface IIdentifiable {
    /// <summary>
    ///     Gets the id of this instance
    /// </summary>
    // [NonOverridable]
    Guid Id { get; set; }
}
