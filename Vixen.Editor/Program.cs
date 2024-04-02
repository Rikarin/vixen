using Arch.Core;
using Arch.Core.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using System.Drawing;
using System.Numerics;
using Vixen.Core.Common;
using Vixen.Core.Components;
using Vixen.Core.General;
using Vixen.Diagnostics;
using Vixen.Editor;
using Vixen.Importers;
using Vixen.Platform.Common.Rendering;
using Vixen.Platform.Internal;
using Vixen.Platform.Silk;
using Vixen.Platform.Vulkan;
using Vixen.Rendering;
using Profiler = Vixen.Diagnostics.Profiler;

Thread.CurrentThread.Name = "Main";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Vixen.Platform.Abstractions.Rendering.RendererContext", LogEventLevel.Information)
    .MinimumLevel.Override("Vixen.Platform.Abstractions.Rendering.ISwapchain", LogEventLevel.Information)
    // .MinimumLevel.Override("Vixen.Editor.ShaderCompiler", LogEventLevel.Information)

    // For debugging pipeline
    // .MinimumLevel.Verbose()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .Enrich.WithThreadName()
    .Enrich.With(new SourceContextEnricher())
    .WriteTo.EditorSink()
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}][{ThreadName}]{SourceContext} {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

// TODO Register additional assemblies
// AssemblyRegistry.Register(typeof);

// test
// var a = BuildDependencyManager.AssetCompilerRegistry;
// a.GetCompiler(t)


SceneManager.Initialize();

var project = Project.CreateProject("Example 1", "../Examples/Project1");
Project.OpenProject = project;
project.Save();

var editor = new EditorManager(project);
editor.Watch();

// SceneManager.SetActiveScene(SceneManager.LoadScene("Scene01.json"));
if (SceneManager.ActiveScene == null) {
    SceneManager.SetActiveScene(SceneManager.CreateScene("TestScene 1"));
    
    var world1 = SceneManager.ActiveScene!.World;
    var parent = world1.Create<LocalTransform, LocalToWorld, MeshFilter>();
    // var player = world1.Create<LocalTransform, LocalToWorld>();
    // player.AddRelationship<Parent>(parent);

    parent.Set(new LocalTransform(new(0, 0, 0), Quaternion.Zero, 1));
    // player.Set(new LocalTransform(new(1, 2, 3), Quaternion.Identity, 1));
    // player.Add(new Name("Player"));
    // parent.Add<IsScriptEnabled>();
    // parent.Add(new PlayerControllerScript());
    
    var plane2 = world1.Create<LocalTransform, LocalToWorld, MeshFilter>();
    plane2.Set(new LocalTransform(new(0, 0, 0), Quaternion.CreateFromAxisAngle(Vector3.UnitX, 90), 1));
    
    // Entity child = default;
    // for (var i = 0; i < 10; i++) {
    //     child = world1.Create();
    //     child.AddRelationship<Parent>(parent);
    // }
    //
    // var child2 = world1.Create();
    // child2.AddRelationship<Parent>(child);
}

var app = Application.CreateDefault(
    options => {
        options.Name = "Project 1";
        options.ThreadingPolicy = ThreadingPolicy.MultiThreaded;
        options.WindowSize = new(1600, 900);
        options.VSync = true;
    }
);


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

// TODO: get new command buffer when viewport changes?
var commandBuffer = ObjectFactory.CreateRenderCommandBufferFromSwapChain("RuntimeLayer");


var quadTest = new QuadTest(framebuffer);


// ECS Test
var world = SceneManager.ActiveScene!.World;
var query = new QueryDescription().WithAll<LocalToWorld, MeshFilter>().WithNone<IsDisabledTag>();

// Editor Camera
var editorCamera = new EditorCamera(45, new(1280, 720), 0.1f, 1000);
var editorCameraEntity = SceneManager.ActiveScene.World.Create(
    new IsScriptEnabled(),
    new LocalTransform(new(0, -3.6f,  1.25f), Quaternion.CreateFromAxisAngle(Vector3.UnitX, 45), 1),
    new LocalToWorld(),
    editorCamera,
    new Name("Editor Camera")
);


// Assimp model importer
var converter = new AssimpConverter();
// converter.ExtractEntity(Path.Combine(project.RootDirectory, "Assets/pillar.fbx"));



var gui = new GuiRenderer(app, project);
gui.OnStart();

// TODO: VulkanRenderer
// TODO: VulkanShader(CreateDescriptors)
// TODO: RenderCommandBuffer(Submit, Statistics)
// TODO: ComputePass, ComputePipeline
// TODO: Renderer(All shapes and passes)

// TODO: use options pattern for ObjectFactory
// TODO: Assimp(mesh loader)
// TODO: render first quad thru vulkan

// Stage 2
// TODO: Texture2D, TextureCube
// TODO: DescriptorSetManager(Texture, Image)
// TODO: VulkanMaterial(Texture, Image)

Profiler.Initialize(options => options.AddRollingExporter(1000));

app.Update += () => {
    commandBuffer.Begin();
    var size = SilkWindow.MainWindow.Size;
    editorCamera.SetViewportSize(size);

    quadTest.Begin(commandBuffer);

    world.Query(
        query,
        (ref LocalToWorld t) => {
            // Log.Information("Debug: {Variable}", t.Value.ToString());
            quadTest.Render(commandBuffer, t.Value);
        }
    );
    
    quadTest.End(commandBuffer);

    Renderer.Submit(
        () => {
            SilkWindow.MainWindow.imGuiController.Update(Time.DeltaTime);
            gui.OnRender(Time.DeltaTime);
            
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

app.Run();
SceneManager.ActiveScene.Systems.Dispose();
// SceneManager.ActiveScene.Save();
return 0;



// public class PlayerControllerScript : Script {
//     public override void OnStart() {
//         Log.Information("start");
//     }
//
//     public override void OnUpdate() {
//         ref var transform = ref Entity.Get<LocalTransform>();
//         
//         if (Input.GetKey(Key.W)) {
//             transform.Position += transform.Forward * Time.DeltaTime;
//         } else if (Input.GetKey(Key.S)) {
//             transform.Position += -transform.Forward * Time.DeltaTime;
//         }
//         
//         if (Input.GetKey(Key.A)) {
//             transform.Position += -transform.Right * Time.DeltaTime;
//         } else if (Input.GetKey(Key.D)) {
//             transform.Position += transform.Right * Time.DeltaTime;
//         }
//         
//         if (Input.GetKey(Key.Space)) {
//             transform.Position += transform.Up * Time.DeltaTime;
//         } else if (Input.GetKey(Key.ShiftLeft)) {
//             transform.Position += -transform.Up * Time.DeltaTime;
//         }
//     }
// }