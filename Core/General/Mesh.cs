using Rin.Platform.Renderer;
using System.Numerics;

namespace Rin.Core.General;

public class Mesh {
    internal readonly VertexArray vertexArray = VertexArray.Create();
    VertexBuffer vertexBuffer;
    IndexBuffer indexBuffer;

    public Vector3[] Vertices { get; set; }
    public Vector2[] UV { get; set; }
    public int[] Triangles { get; set; }


    // TODO: consider moving this elsewhere
    public static Mesh CreateBox() {
        // TODO: finish this
        return new() {
            Vertices = new[] {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(0, 0.5f, 0)
            },
            UV = new[] { new Vector2(0, 0) },
            Triangles = new[] { 0, 1, 2 }
        };
    }

    
    // TODO: this method is exposed for testing purpose only

    public void SetupMesh() {
        var vertices = new float[Vertices.Length * 3];
        for (var i = 0; i < Vertices.Length; i++) {
            vertices[i * 3 + 0] = Vertices[i].X;
            vertices[i * 3 + 1] = Vertices[i].Y;
            vertices[i * 3 + 2] = Vertices[i].Z;
        }

        vertexBuffer = VertexBuffer.Create(vertices);
        vertexBuffer.Layout = new(new[] { new VertexBufferElement(ShaderDataType.Float3, "a_Position") });
        vertexArray.AddVertexBuffer(vertexBuffer);
        
        indexBuffer = IndexBuffer.Create(Triangles);
        vertexArray.SetIndexBuffer(indexBuffer);
    }
}
