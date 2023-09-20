using System.Numerics;

namespace Rin.Core.General;

public class Mesh {
    public Vector3[] Vertices { get; set; }
    public Vector2[] UV { get; set; }
    public int[] Triangles { get; set; }

    // TODO: consider moving this elsewhere
    public static Mesh CreateBox() {
        // TODO: finish this
        return new() {
            Vertices = new [] {
                new Vector3(0, 0, 0)
            },
            UV = new [] {
                new Vector2(0, 0)
            },
            Triangles = new [] {
                0, 1, 2
            }
        };
    }
}
