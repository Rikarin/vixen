using Serilog;
using System.Runtime.CompilerServices;

namespace Rin.Core.Diagnostics;

public static class SafeAction {
    static readonly ILogger Log = Serilog.Log.ForContext<Thread>();

    public static ThreadStart Wrap(
        ThreadStart action,
        [CallerFilePath]
        string sourceFilePath = "",
        [CallerMemberName]
        string memberName = "",
        [CallerLineNumber]
        int sourceLineNumber = 0
    ) {
        return () => {
            try {
                action();
            } catch (ThreadAbortException) {
                // Ignore this exception
            } catch (Exception ex) {
                Log.Fatal(
                    ex,
                    "Unexpected exception {File} {Member}:{Line}",
                    sourceFilePath,
                    memberName,
                    sourceLineNumber
                );
                throw;
            }
        };
    }

    public static ParameterizedThreadStart Wrap(
        ParameterizedThreadStart action,
        [CallerFilePath]
        string sourceFilePath = "",
        [CallerMemberName]
        string memberName = "",
        [CallerLineNumber]
        int sourceLineNumber = 0
    ) {
        return obj => {
            try {
                action(obj);
            } catch (ThreadAbortException) {
                // Ignore this exception
            } catch (Exception ex) {
                Log.Fatal(
                    ex,
                    "Unexpected exception {File} {Member}:{Line}",
                    sourceFilePath,
                    memberName,
                    sourceLineNumber
                );
                throw;
            }
        };
    }
}
