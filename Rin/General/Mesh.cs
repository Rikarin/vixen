using System.Numerics;

namespace Editor.General;

public class Mesh {
    public Vector3[] Vertices { get; set; }
    public Vector2[] UV { get; set; }
    public int[] Triangles { get; set; }
}
