using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Vixen.Core")]
namespace Vixen.Core.Common;

public static class Time {
    public static float DeltaTime { get; internal set; }
}
