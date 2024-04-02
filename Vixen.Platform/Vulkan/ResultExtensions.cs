using Silk.NET.Vulkan;

namespace Vixen.Platform.Vulkan; 

static class ResultExtensions {
    public static void EnsureSuccess(this Result result) {
        if (result != Result.Success) {
            throw new($"Command failed {result}");
        }
    }
}
