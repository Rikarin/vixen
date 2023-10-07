using Rin.Core.Abstractions;
using Rin.Core.General;
using Rin.Editor;
using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Internal;
using Rin.Rendering;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using System.Diagnostics.Tracing;
using System.Drawing;

var eventSourceListener = new EventSourceCreatedListener();

Thread.CurrentThread.Name = "Main";

// TODO: colored text, thread name, context name
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Rin.Platform.Abstractions.Rendering.IRenderer", LogEventLevel.Information)
    .MinimumLevel.Override("Rin.Platform.Abstractions.Rendering.ISwapchain", LogEventLevel.Information)
    .MinimumLevel.Override("Rin.Platform.Abstractions.Rendering.IRenderCommandBuffer", LogEventLevel.Information)
    .MinimumLevel.Override("Rin.Core.General.Application", LogEventLevel.Information)
    .MinimumLevel.Override("Rin.Editor.ShaderCompiler", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .Enrich.WithThreadName()
    // .Enrich.WithProperty(ThreadNameEnricher.ThreadNamePropertyName, "MyDefault")
    .Enrich.FromLogContext()
    .Enrich.With(new SourceContextEnricher())

    // .Enrich.WithProperty("Environment", builder.Configuration.GetSection("environment").Value)
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


// LoadRuntime, ...
var app = Application.CreateDefault(
    options => {
        options.Name = "Project 1";
        options.ThreadingPolicy = ThreadingPolicy.SingleThreaded;
        options.WindowSize = new(800, 600);
    }
);

// Test API
// var scene = SceneManager.CreateScene("Main Scene");
// SceneManager.SetActiveScene(scene);
//
// var boxObj = new GameObject();
// boxObj.AddComponent<MeshFilter>();
//
// Mesh? box = null;
// Material? material = null;

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

// TODO: stuff

// TODO: finish DescriptorSetManager, VulkanPipeline, VulkanRenderCommandBuffer

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


app.Render += () => {
    // Log.Information("On Update");

    commandBuffer.Begin();
    Renderer.BeginRenderPass(commandBuffer, swapchainRenderPass);
    Renderer.EndRenderPass(commandBuffer);
    commandBuffer.End();


    // RenderCommand.SetClearColor(Color.Gray);
    // RenderCommand.Clear();
    // This can be called from SilkWindow
    // RenderCommand.SetViewport(Point.Empty, new Size(128, 128));
    //
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
    //
    // material?.Render();
    // if (box != null) {
    //     // RenderCommand.Draw(box);
    // }
};

// This needs to be set after the RenderCommand.Clear() is called
// var guiRenderer = new GuiRenderer(app, project);


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


// TODO: use this to show in editor EventSource metrics
sealed class EventSourceCreatedListener : EventListener {
    protected override void OnEventSourceCreated(EventSource eventSource) {
        base.OnEventSourceCreated(eventSource);

        // EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All);
        Console.WriteLine($"New event source: {eventSource.Name}");
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData) {
        base.OnEventWritten(eventData);
        // Console.WriteLine($"event data {eventData.EventName}");
    }
}


class SourceContextEnricher : ILogEventEnricher {
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
        if (logEvent.Properties.TryGetValue("SourceContext", out var property)) {
            var scalarValue = property as ScalarValue;
            var value = scalarValue?.Value as string;

            if (value?.StartsWith("Rin") ?? false) {
                var lastElement = value.Split(".").LastOrDefault();
                if (!string.IsNullOrWhiteSpace(lastElement)) {
                    logEvent.AddOrUpdateProperty(new("SourceContext", new ScalarValue($"[{lastElement}]")));
                }
            }
        }
    }
}
