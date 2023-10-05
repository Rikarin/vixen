namespace Rin.Platform.Rendering;

// TODO: use options pattern
public sealed class PipelineOptions {
    public IShader Shader { get; set; }
    public IFramebuffer TargetFramebuffer { get; set; }
    public VertexBufferLayout Layout { get; set; }
    public VertexBufferLayout? InstanceLayout { get; set; }
    public VertexBufferLayout? BoneInfluenceLayout { get; set; }
    public PrimitiveTopology Topology { get; set; } = PrimitiveTopology.Triangles;
    public DepthCompareOperator DepthOperator { get; set; } = DepthCompareOperator.GreaterOrEqual;
    public bool BackfaceCulling { get; set; } = true;
    public bool DepthTest { get; set; } = true;
    public bool DepthWrite { get; set; } = true;
    public bool WireFrame { get; set; }
    public float LineWidth { get; set; } = 1;
    public string DebugName { get; set; }
}
