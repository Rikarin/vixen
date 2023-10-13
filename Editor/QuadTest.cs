using Rin.Core.Abstractions;
using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Internal;
using System.Drawing;

namespace Rin.Editor;

public class QuadTest {
    public QuadTest() {
        var shaderImporter = new ShaderImporter("Assets/Shaders/Quad.shader");
        var shader = shaderImporter.GetShader();

        var framebufferOptions = new FramebufferOptions {
            DebugName = "Quad FB", Attachments = new(ImageFormat.Rgba), ClearColor = Color.Pink
        };

        var framebuffer = ObjectFactory.CreateFramebuffer(framebufferOptions);

        var pipelineOptions = new PipelineOptions {
            Layout = new(
                new VertexBufferElement(ShaderDataType.Float3, "a_Position"),
                new VertexBufferElement(ShaderDataType.Float2, "a_TexCoord")
            ),
            BackfaceCulling = false,
            Shader = shader.Handle,
            TargetFramebuffer = framebuffer,
            DebugName = "Quad"
        };

        var renderPassOptions = new RenderPassOptions {
            DebugName = "Quad", Pipeline = ObjectFactory.CreatePipeline(pipelineOptions)
        };

        var renderPass = ObjectFactory.CreateRenderPass(renderPassOptions);
        renderPass.Bake();
        
        // var material = ObjectFactory.createma
    }
}
