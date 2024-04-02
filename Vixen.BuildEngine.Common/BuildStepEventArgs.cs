using Serilog;

namespace Vixen.BuildEngine.Common;

public class BuildStepEventArgs : EventArgs {
    public BuildStep Step { get; private set; }
    public ILogger Log { get; set; }

    public BuildStepEventArgs(BuildStep step, ILogger log) {
        Step = step;
        Log = log;
    }
}
