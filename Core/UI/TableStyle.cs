namespace Rin.Core.UI;

public class TableStyle : PushConfiguration {
    public static TableStyle Default = new() { HasHeader = true };
    public bool HasHeader { get; set; }
    // public bool HasBorder { get; set; }
}
