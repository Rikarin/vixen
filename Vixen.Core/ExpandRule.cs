namespace Vixen.Core;

public enum ExpandRule {
    /// <summary>
    ///     The control representing the associated object will use the default rule.
    /// </summary>
    Auto,

    /// <summary>
    ///     The control representing the associated object will be expanded only the first time it is displayed.
    /// </summary>
    Once,

    /// <summary>
    ///     The control representing the associated object will be collapsed.
    /// </summary>
    Never,

    /// <summary>
    ///     The control representing the associated object will be expanded.
    /// </summary>
    Always
}
