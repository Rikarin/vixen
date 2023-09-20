using Rin.Platform.Renderer;
using System.Drawing;

namespace Rin.Platform.Internal;

interface IRendererApi {
    void Initialize();
    void SetViewport(Point point, Size size);
    void SetClearColor(Color color);
    void Clear();
    void Draw(VertexArray vertexArray, int? count);
}
