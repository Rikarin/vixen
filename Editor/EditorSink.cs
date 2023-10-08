using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Rin.Editor;

public sealed class EditorSink : ILogEventSink {
    public static List<string> Messages { get; } = new();
    readonly IFormatProvider? formatProvider;

    public EditorSink(IFormatProvider? formatProvider) {
        this.formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent) {
        var message = logEvent.RenderMessage(formatProvider);
        Messages.Add(message);
    }
}

public static class EditorSinkExtensions {
    public static LoggerConfiguration EditorSink(
        this LoggerSinkConfiguration loggerConfiguration,
        IFormatProvider? formatProvider = null
    ) =>
        loggerConfiguration.Sink(new EditorSink(formatProvider));
}
