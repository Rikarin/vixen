namespace Vixen.BuildEngine.Common;

/// <summary>
///     Status of a command.
/// </summary>
public enum ResultStatus {
    /// <summary>
    ///     The command has not finished yet
    /// </summary>
    NotProcessed,

    /// <summary>
    ///     The command was successfully executed
    /// </summary>
    Successful,

    /// <summary>
    ///     The command execution failed
    /// </summary>
    Failed,

    /// <summary>
    ///     The command was started but cancelled, output is undeterminated
    /// </summary>
    Cancelled,

    /// <summary>
    ///     A command may not be triggered if its input data haven't changed since the successful last execution
    /// </summary>
    NotTriggeredWasSuccessful,

    /// <summary>
    ///     One of the prerequisite command failed and the command was not executed
    /// </summary>
    NotTriggeredPrerequisiteFailed
}
