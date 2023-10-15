using Arch.Core;
using Rin.Core;
using Rin.Core.General;
using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Internal;
using Rin.Rendering;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Rin.Editor;

public class QuadTest {
    IPipeline pipeline;
    IMaterial material;
    IRenderPass renderPass;

    IUniformBufferSet ubsCamera;
    
    public QuadTest(IFramebuffer framebuffer) {
        ubsCamera = ObjectFactory.CreateUniformBufferSet(Marshal.SizeOf<UniformCamera>());
        
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
        renderPass.SetInput("u_Camera", ubsCamera);
        renderPass.Bake();

        material = ObjectFactory.CreateMaterial(shader.Handle, "foo bar");
        material.Set("u_Settings.Scale", 42);
        material.Prepare();
    }

    public void Begin(IRenderCommandBuffer commandBuffer) {
        Renderer.BeginRenderPass(commandBuffer, renderPass);
    }
    
    public void End(IRenderCommandBuffer commandBuffer) {
        Renderer.EndRenderPass(commandBuffer);
    }

    public void Render(IRenderCommandBuffer commandBuffer, Matrix4x4 transform) {
        var matrix = SceneManager.ActiveScene.World.GetSingleton<EditorCamera>().ViewProjection;
        
        Renderer.Submit(
            () => {
                var camera = new UniformCamera { ViewProjectionMatrix = matrix };
                ubsCamera.Get_RT().SetData_RT(camera);
            });
        
        Renderer.RenderQuad(commandBuffer, pipeline, material, transform);
    }

    struct UniformCamera {
        public Matrix4x4 ViewProjectionMatrix;
        // Vector3 test123;
    }
    
    //This matrix handles Vulkan's inverted Y and half Z coordinate system
    static readonly Matrix4x4 VulkanClip = new(
        1.0f,  0.0f,  0.0f,  0.0f,
        0.0f, -1.0f,  0.0f,  0.0f,
        0.0f,  0.0f,  0.5f,  0.0f,
        0.0f,  0.0f,  0.5f,  1.0f);
}
