using Rin.Core.Abstractions;
using Rin.Core.General;
using Rin.Editor;
using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Internal;
using Rin.Platform.Silk;
using Rin.Platform.Vulkan;
using Rin.Rendering;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using System.Drawing;

var eventSourceListener = new EventSourcesListener();

Thread.CurrentThread.Name = "Main";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Rin.Platform.Abstractions.Rendering.RendererContext", LogEventLevel.Information)
    .MinimumLevel.Override("Rin.Platform.Abstractions.Rendering.ISwapchain", LogEventLevel.Information)
    .MinimumLevel.Override("Rin.Editor.ShaderCompiler", LogEventLevel.Information)

    // For debugging pipeline
    // .MinimumLevel.Verbose()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .Enrich.WithThreadName()
    .Enrich.FromLogContext()
    .Enrich.With(new SourceContextEnricher())
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}][{ThreadName}]{SourceContext} {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

var project = Project.CreateProject("Example 1", "../Examples/Project1");
Project.OpenProject = project;
project.Save();

var editor = new EditorManager(project);
editor.Watch();


var app = Application.CreateDefault(
    options => {
        options.Name = "Project 1";
        options.ThreadingPolicy = ThreadingPolicy.MultiThreaded;
        options.WindowSize = new(1600, 900);
    }
);

// Test API
// var scene = SceneManager.CreateScene("Main Scene");
// SceneManager.SetActiveScene(scene);
//
// var boxObj = new GameObject();
// boxObj.AddComponent<MeshFilter>();


var shaderImporter = new ShaderImporter("Assets/Shaders/RenderShader.shader");
var testShader = shaderImporter.GetShader();

// TODO: this is based on RuntimeLayer.OnAttach()
var framebuffer = ObjectFactory.CreateFramebuffer(
    new() {
        DebugName = "SceneComposite",
        ClearColor = Color.Aqua,
        IsSwapChainTarget = true,
        Attachments = new(ImageFormat.Rgba)
    }
);

var pipelineOptions = new PipelineOptions {
    Layout = new(
        new VertexBufferElement(ShaderDataType.Float3, "a_Position"),
        new VertexBufferElement(ShaderDataType.Float2, "a_TexCoord")
    ),
    BackfaceCulling = false,
    Shader = testShader.Handle, // TODO
    TargetFramebuffer = framebuffer,
    DebugName = "SceneComposite",
    DepthWrite = false
};

// Render Pass
var swapchainRenderPass = ObjectFactory.CreateRenderPass(
    new() { DebugName = "SceneComposite", Pipeline = ObjectFactory.CreatePipeline(pipelineOptions) }
);

swapchainRenderPass.Bake();

var commandBuffer = ObjectFactory.CreateRenderCommandBufferFromSwapChain("RuntimeLayer");

// vertexArray = VertexArray.Create();
// float[] vertices = {
//     -0.5f, -0.5f, 0.0f, 0.8f, 0.2f, 0.8f, 1.0f,
//     0.5f, -0.5f, 0.0f, 0.2f, 0.3f, 0.8f, 1.0f,
//     0.0f,  0.5f, 0.0f, 0.8f, 0.8f, 0.2f, 1.0f
// };
//
// var vertexBuffer = VertexBuffer.Create(vertices);
// var bufferLayout = new BufferLayout(
//     new[] {
//         new BufferElement(ShaderDataType.Float3, "a_Position"), new BufferElement(ShaderDataType.Float4, "a_Color")
//     }
// );
//
// vertexBuffer.Layout = bufferLayout;
// vertexArray.AddVertexBuffer(vertexBuffer);
//
// int[] indices = { 0, 1, 2 };
// var indexBuffer = IndexBuffer.Create(indices);
// vertexArray.SetIndexBuffer(indexBuffer);

// Shader.Create(
//     "Basic/Shader1",
//     "../Examples/Project1/Assets/Shaders/Shader.vert",
//     "../Examples/Project1/Assets/Shaders/Shader.frag"
// );

// material = new(Shader.Find("Basic/Shader1")!);
// material.SetColor("u_Color", Color.Bisque);

var gui = new GuiRenderer(app, project);
gui.OnStart();

app.Update += () => {
    commandBuffer.Begin();
    Renderer.BeginRenderPass(commandBuffer, swapchainRenderPass);
    Renderer.EndRenderPass(commandBuffer);

    Renderer.Submit(
        () => {
            SilkWindow.MainWindow.imGuiController.Update(0.1f);
            gui.OnRender(0.1f);
            
            var vkCmd = commandBuffer as VulkanRenderCommandBuffer;
            var sw = SilkWindow.MainWindow.Swapchain as VulkanSwapChain;

            if (vkCmd.ActiveCommandBuffer.HasValue) {
                SilkWindow.MainWindow.imGuiController.Render(
                    vkCmd.ActiveCommandBuffer.Value,
                    sw.CurrentFramebuffer,
                    new((uint)sw.Size.Width, (uint)sw.Size.Height)
                );
            }
        }
    );

    commandBuffer.End();


    // if (Input.GetKey(Key.A)) {
    //     Log.Information("Key A pressed");
    // }
    //
    // if (Input.GetKeyDown(Key.W)) {
    //     Log.Information("Key W Down");
    // }
    //
    // if (Input.GetKeyUp(Key.W)) {
    //     Log.Information("Key W Up");
    // }
};

// TODO: Example of creating a few objects
// {
// var box = new GameObject();
// box.AddComponent<Transform>();
//     var meshFilter = box.AddComponent<MeshFilter>();
//     meshFilter.Mesh = Mesh.CreateBox();
//
//     // This should render object in the scene
//     box.AddComponent<MeshRenderer>();
//     
//     // Create scene
//     // Load scene
// }

app.Run();
return 0;
