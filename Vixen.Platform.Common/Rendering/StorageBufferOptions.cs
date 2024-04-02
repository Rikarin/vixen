namespace Vixen.Platform.Common.Rendering;

public class StorageBufferOptions {
    public string DebugName { get; set; }
    public bool GpuOnly { get; set; } = true; // TODO: consider using options pattern instead
}
