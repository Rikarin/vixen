namespace Rin.Core.MicroThreading;

public enum MicroThreadState {
    None,
    Starting,
    Running,
    Completed,
    Canceled,
    Failed
}