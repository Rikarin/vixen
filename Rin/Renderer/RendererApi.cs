namespace Editor.Renderer;

class RendererApi {
    public static Api CurrentApi => Api.OpenGl;

    public enum Api {
        None,
        OpenGl
    }
}
