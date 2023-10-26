using Serilog;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Rin.Core.Diagnostics;

/// <summary>
///     A logger that stores messages locally useful for internal log scenarios.
/// </summary>
[DebuggerDisplay("HasErrors: {HasErrors} Messages: [{Messages.Count}]")]
public class LoggerResult : IProgressStatus {
    public ILogger Log { get; }

    /// <summary>
    ///     Gets the module name. read-write.
    /// </summary>
    /// <value>The module name.</value>
    public Type? Module { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is logging progress as information. Default is true.
    /// </summary>
    /// <value><c>true</c> if this instance is logging progress as information; otherwise, <c>false</c>.</value>
    public bool IsLoggingProgressAsInfo { get; set; }

    /// <summary>
    ///     Gets the messages logged to this instance.
    /// </summary>
    /// <value>The messages.</value>
    // public TrackingCollection<ILogMessage> Messages { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoggerResult" /> class.
    /// </summary>
    public LoggerResult(Type? module = null) {
        Module = module;
        Log = Serilog.Log.ForContext(Module!);
        // Messages = new TrackingCollection<ILogMessage>();
        IsLoggingProgressAsInfo = false;
        // By default, all logs are enabled for a local logger.
        // ActivateLog(LogMessageType.Debug);
    }

    /// <summary>
    ///     Clears all messages.
    /// </summary>
    // public virtual void Clear() {
    //     Messages.Clear();
    // }

    /// <summary>
    ///     Notifies progress on this instance.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Progress([NotNull] string message) {
        OnProgressChanged(new(message));
    }

    /// <summary>
    ///     Notifies progress on this instance.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="currentStep">The current step.</param>
    /// <param name="stepCount">The step count.</param>
    public void Progress([NotNull] string message, int currentStep, int stepCount) {
        OnProgressChanged(new(message, currentStep, stepCount));
    }

    /// <summary>
    ///     Copies all messages to another instance.
    /// </summary>
    /// <param name="results">The results.</param>
    // public void CopyTo(ILogger results) {
    //     foreach (var reportMessage in Messages) {
    //         results.Log(reportMessage);
    //     }
    // }

    /// <summary>
    ///     Returns a string representation of this
    /// </summary>
    /// <returns>System.String.</returns>
    // public string ToText() {
    //     var text = new StringBuilder();
    //     foreach (var logMessage in Messages) {
    //         text.AppendLine(logMessage.ToString());
    //     }
    //
    //     return text.ToString();
    // }
    void OnProgressChanged(ProgressStatusEventArgs e) {
        if (IsLoggingProgressAsInfo) {
            Log.Information(e.Message);
        }

        ProgressChanged?.Invoke(this, e);
    }

    void IProgressStatus.OnProgressChanged(ProgressStatusEventArgs e) {
        OnProgressChanged(e);
    }

    /// <summary>
    ///     Occurs when the progress changed for this logger.
    /// </summary>
    public event EventHandler<ProgressStatusEventArgs> ProgressChanged;
}
