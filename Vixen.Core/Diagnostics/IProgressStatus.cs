namespace Vixen.Core.Diagnostics;

/// <summary>
///     Provides progress of an operation.
/// </summary>
public interface IProgressStatus {
    /// <summary>
    ///     Handles the <see cref="E:ProgressChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="ProgressStatusEventArgs" /> instance containing the event data.</param>
    void OnProgressChanged(ProgressStatusEventArgs e);

    /// <summary>
    ///     An event handler to notify the progress of an operation.
    /// </summary>
    event EventHandler<ProgressStatusEventArgs> ProgressChanged;
}
