using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Internal;
using Rin.Rendering;
using System.Numerics;

namespace Rin.Editor;

public class QuadTest {
    IPipeline pipeline;
    IMaterial material;
    IRenderPass renderPass;
    
    public QuadTest(IFramebuffer framebuffer) {
        var uniformCamera = ObjectFactory.CreateUniformBufferSet(19 * sizeof(float));
        
        var shaderImporter = new ShaderImporter("Assets/Shaders/Quad.shader");
        var shader = shaderImporter.GetShader();

        // var framebufferOptions = new FramebufferOptions {
        //     DebugName = "Quad FB", Attachments = new(ImageFormat.Rgba), ClearColor = Color.Pink
        // };
        // framebuffer = ObjectFactory.CreateFramebuffer(framebufferOptions);

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

        pipeline = ObjectFactory.CreatePipeline(pipelineOptions);
        var renderPassOptions = new RenderPassOptions {
            DebugName = "Quad Render Pass", Pipeline = pipeline
        };

        renderPass = ObjectFactory.CreateRenderPass(renderPassOptions);
        renderPass.SetInput("u_Camera", uniformCamera);
        renderPass.Bake();

        material = ObjectFactory.CreateMaterial(shader.Handle, "foo bar");
        // material.Set("pushConstants.someVariable", 42);
        material.Prepare();
    }

    public void Render(IRenderCommandBuffer commandBuffer, Matrix4x4 transform) {
        Renderer.BeginRenderPass(commandBuffer, renderPass);
        Renderer.RenderQuad(commandBuffer, pipeline, material, transform);
        Renderer.EndRenderPass(commandBuffer); 
    }

    struct UniformCamera {
        Matrix4x4 position;
        Vector3 test123;
    }
}
