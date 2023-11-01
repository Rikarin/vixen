namespace Rin.BuildEngine.Common;

public enum BuildResultCode {
    Successful = 0,
    BuildError = 1,
    CommandLineError = 2,
    Cancelled = 100
}
