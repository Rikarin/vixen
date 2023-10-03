using Rin.Platform.Internal;
using System.Drawing;

namespace Rin.Core.General;

public static class RenderCommand {
    static readonly IRendererApi api = null; //new OpenGLRendererApi();

    public static void Initialize() => api.Initialize();
    public static void SetViewport(Point point, Size size) => api.SetViewport(point, size);
    public static void SetClearColor(Color color) => api.SetClearColor(color);
    public static void Clear() => api.Clear();

    // TODO: we need to have VertexArray and buffers public due to this
    // public static void Draw(VertexArray vertexArray, int? count = null) => api.Draw(vertexArray, count);
    // public static void Draw(Mesh mesh) => api.Draw(mesh.vertexArray, null);
}
