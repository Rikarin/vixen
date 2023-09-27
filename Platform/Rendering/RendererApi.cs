namespace Rin.Platform.Rendering;

sealed class RendererApi {
    public static Api CurrentApi => Api.Vulkan;

    public enum Api {
        None,
        OpenGl,
        Vulkan
    }
}
