using Arch.Core;

namespace Rin.Core;

public static class ArchExtensions {
    public static ref T GetSingleton<T>(this World world) {
        var desc = new QueryDescription().WithAll<T>();
        var query = world.Query(desc);

        foreach (ref var chunk in query) {
            return ref chunk.GetFirst<T>();
        }

        throw new("not found");
    }
}
