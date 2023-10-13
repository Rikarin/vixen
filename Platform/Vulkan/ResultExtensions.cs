using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan; 

static class ResultExtensions {
    public static void EnsureSuccess(this Result result) {
        if (result != Result.Success) {
            throw new($"Command failed {result}");
        }
    }
}
