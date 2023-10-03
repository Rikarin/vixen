using Rin.Core.Abstractions.Shaders;
using Rin.Core.General;
using Rin.Editor.RinCompiler;
using Rin.Editor.RinCompiler.Parser;
using Rin.Platform.Rendering;
using Rin.Platform.Vulkan;
using Serilog;
using Silk.NET.Shaderc;
using Silk.NET.SPIRV.Cross;
using Silk.NET.Vulkan;
using System.Runtime.InteropServices;

namespace Rin.Editor;

public class ShaderCompiler {
    readonly ILogger logger = Log.ForContext<ShaderCompiler>();
    readonly string shaderPath;
    string? name;

    // These are default entry points of *.shader file
    // If shader contains compute or geometry shader it needs to set it explicitly in the shader file
    // as  TODO compute <entry_point>
    readonly Dictionary<ShaderStageFlags, string> shaderEntryPoints = new() {
        { ShaderStageFlags.VertexBit, "vert" }, { ShaderStageFlags.FragmentBit, "frag" }
    };

    // Compiled shaders
    readonly ShaderCollection shaderData = new();
    readonly ShaderCollection shaderDebugData = new();

    public string? ProgramSource { get; private set; }

    internal ShaderResource.ReflectionData ReflectionData { get; } = new();

    static string CacheDirectory => Path.Combine(Project.OpenProject!.CacheDirectory, "Shaders");

    ShaderCompiler(string shaderPath) {
        this.shaderPath = shaderPath;
    }

    public static Shader Compile(string shaderPath, bool forceCompile, bool debug) {
        Log.Information("Compiling shader");
        var compiler = new ShaderCompiler(Path.Combine(Project.OpenProject!.RootDirectory, shaderPath));
        compiler.Reload(forceCompile);

        // Vulkan Shader
        var vulkanShader = new VulkanShader();
        vulkanShader.LoadAndCreateShaders(compiler.shaderData);
        // set reflection data
        // create descriptors

        var shader = new Shader(vulkanShader, compiler.name!, shaderPath);

        // Renderer acknowledge parsed global marcros
        // on shader reloaded


        Log.Information("Debug asdf: {Variable}", compiler.name);

        ShaderCache.Serialize(compiler);

        return shader;
    }

    public void Reload(bool forceCompile) {
        name = null;
        ProgramSource = null;

        Directory.CreateDirectory(CacheDirectory);

        var source = File.ReadAllText(shaderPath);
        Preprocess(source);


        // TODO: caching and stuff

        foreach (var entryPoint in shaderEntryPoints) {
            Compile(ProgramSource, entryPoint.Key, true, shaderDebugData);
            Compile(ProgramSource, entryPoint.Key, false, shaderData);
        }

        // TODO
        if (forceCompile || true) {
            ReflectAllShaderStages(shaderDebugData);
        }
    }

    public unsafe void Compile(string shader, ShaderStageFlags stage, bool debug, ShaderCollection dataStorage) {
        var api = Shaderc.GetApi();
        var compiler = api.CompilerInitialize();
        var options = api.CompileOptionsInitialize();

        api.CompileOptionsSetTargetEnv(options, TargetEnv.Vulkan, (uint)EnvVersion.Vulkan12);
        api.CompileOptionsSetTargetSpirv(options, SpirvVersion.Shaderc15);
        api.CompileOptionsSetSourceLanguage(options, SourceLanguage.Hlsl);

        if (debug) {
            api.CompileOptionsSetGenerateDebugInfo(options);
            api.CompileOptionsSetOptimizationLevel(options, OptimizationLevel.Zero);
            api.CompileOptionsSetPreserveBindings(options, true);
        } else {
            api.CompileOptionsSetOptimizationLevel(options, OptimizationLevel.Performance);
        }

        api.CompileOptionsSetIncludeCallbacks(
            options,
            PfnIncludeResolveFn.From(IncludeResolver),
            PfnIncludeResultReleaseFn.From(IncludeReleaser),
            null
        );

        var content = Marshal.StringToHGlobalAnsi(shader);
        var fileName = Marshal.StringToHGlobalAnsi(shaderPath);
        var entryPointPtr = Marshal.StringToHGlobalAnsi(shaderEntryPoints[stage]);

        var result = api.CompileIntoSpv(
            compiler,
            (byte*)content,
            (UIntPtr)shader.Length,
            ShaderStageToShaderC(stage),
            (byte*)fileName,
            (byte*)entryPointPtr,
            options
        );

        Marshal.FreeHGlobal(content);
        Marshal.FreeHGlobal(fileName);
        Marshal.FreeHGlobal(entryPointPtr);

        if (api.ResultGetCompilationStatus(result) != CompilationStatus.Success) {
            throw new ShaderCompilationException(api.ResultGetErrorMessageS(result));
        }

        var compiled = new Span<byte>(api.ResultGetBytes(result), (int)api.ResultGetLength(result));
        dataStorage[stage] = compiled.ToArray();

        api.ResultRelease(result);
        api.CompilerRelease(compiler);
    }

    // static Compile and TryRecompile(Shader)

    void Preprocess(string source) {
        var tokenRange = new TokenRange(source);
        var ast = new Parser().Parse(tokenRange);

        var builder = new ShaderBuilder();
        builder.Visit(ast);

        name = builder.Name;
        ProgramSource = builder.ProgramSource;
    }

    void ReflectAllShaderStages(ShaderCollection data) {
        ClearReflectionData();

        foreach (var entry in data) {
            Reflect(entry.Key, entry.Value);
        }
    }

    void ClearReflectionData() {
        ReflectionData.ShaderDescriptorSets.Clear();
        ReflectionData.Resources.Clear();
        ReflectionData.ConstantBuffers.Clear();
        ReflectionData.PushConstantRanges.Clear();
    }

    unsafe void IncludeReleaser(void* userData, IncludeResult* includeResult) {
        Marshal.FreeHGlobal((nint)includeResult->Content);
        Marshal.FreeHGlobal((nint)includeResult->SourceName);
        Marshal.FreeHGlobal((nint)includeResult);
    }

    unsafe IncludeResult* IncludeResolver(
        void* userData,
        byte* requestedResource,
        int type,
        byte* requestingResource,
        UIntPtr includePath
    ) {
        var result = (IncludeResult*)Marshal.AllocHGlobal(sizeof(IncludeResult));
        var requesting = Marshal.PtrToStringAnsi((nint)requestingResource);
        var requested = Marshal.PtrToStringAnsi((nint)requestedResource);

        var relativePath = Path.Combine(Path.GetDirectoryName(requesting)!, requested!) + ".hlsl";
        if (!File.Exists(relativePath)) {
            throw new FileNotFoundException($"File '{relativePath}' not found");
        }

        var program = File.ReadAllText(relativePath);

        result->SourceName = (byte*)Marshal.StringToHGlobalAnsi(relativePath);
        result->SourceNameLength = (nuint)relativePath.Length;
        result->Content = (byte*)Marshal.StringToHGlobalAnsi(program);
        result->ContentLength = (nuint)program.Length;

        return result;
    }

    void Reflect(ShaderStageFlags shaderStage, ReadOnlyMemory<byte> irCode) {
        using var cross = new CrossWrapper();
        var compiler = cross.CreateCompiler(irCode);
        var shaderResources = compiler.CreateShaderResources();

        // for (var i = 1; i < 15; i++) {
        //     var res = shaderResources.GetResourceListForType((ResourceType)i);
        //     foreach (var resource in res) {
        //         Log.Information("Debug: {a} {Variable}", (ResourceType)i, resource.Name);
        //     }
        // }

        // ============= Uniform Buffers =============
        logger.Information("Uniform Buffers:");
        var uniformBuffers = shaderResources.GetResourceListForType(ResourceType.UniformBuffer);
        foreach (var resource in uniformBuffers) {
            // Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);

            ReflectionData
                .ShaderDescriptorSets
                .GetOrCreateDefault(resource.DescriptorSet)
                .UniformBuffers[resource.BindingPoint] = new() {
                BindingPoint = resource.BindingPoint,
                Size = resource.DeclaredStructSize,
                Name = resource.Name,
                ShaderStage = ShaderStageFlags.All
            };

            logger.Information(
                "{Name} ({DescriptorSet}, {Binding}) | Member Count: {Members} | Size: {Size}",
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint,
                resource.MemberCount,
                resource.DeclaredStructSize
            );
        }

        // ============= Storage Buffers =============
        logger.Information("Storage Buffers:");
        var storageBuffers = shaderResources.GetResourceListForType(ResourceType.StorageBuffer);
        foreach (var resource in storageBuffers) {
            // Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);

            ReflectionData
                .ShaderDescriptorSets
                .GetOrCreateDefault(resource.DescriptorSet)
                .StorageBuffers[resource.BindingPoint] = new() {
                BindingPoint = resource.BindingPoint,
                Size = resource.DeclaredStructSize,
                Name = resource.Name,
                ShaderStage = ShaderStageFlags.All
            };

            logger.Information(
                "{Name} ({DescriptorSet}, {Binding}) | Member Count: {Members} | Size: {Size}",
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint,
                resource.MemberCount,
                resource.DeclaredStructSize
            );
        }

        // ============= Push Constants Buffers =============
        logger.Information("Push Constant Buffers:");
        var pushConstantBuffers = shaderResources.GetResourceListForType(ResourceType.PushConstant);
        foreach (var resource in pushConstantBuffers) {
            // Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);

            var bufferOffset = 0;
            if (ReflectionData.PushConstantRanges.Count > 0) {
                var last = ReflectionData.PushConstantRanges[^1];
                bufferOffset = last.Offset + last.Size;
            }

            ReflectionData.PushConstantRanges.Add(
                new() {
                    ShaderStage = shaderStage, Size = resource.DeclaredStructSize - bufferOffset, Offset = bufferOffset
                }
            );

            if (string.IsNullOrEmpty(resource.Name) || resource.Name == "u_Renderer") {
                continue;
            }

            var shaderBuffer = new ShaderBuffer {
                Name = resource.Name, Size = resource.DeclaredStructSize - bufferOffset
            };

            for (var i = 0; i < resource.MemberCount; i++) {
                var memberData = resource.GetMemoryInfo(i);
                var uniformName = $"{resource.Name}.{memberData.Name}";

                shaderBuffer.Uniforms[uniformName] = new(
                    memberData.Name,
                    SpirTypeToShaderUniformType(memberData.Type),
                    memberData.Size,
                    memberData.Offset
                );
            }

            ReflectionData.ConstantBuffers[resource.Name] = shaderBuffer;

            logger.Information(
                "{Name} | Member Count: {Members} | Size: {Size}",
                resource.Name,
                resource.MemberCount,
                resource.DeclaredStructSize
            );
        }

        // ============= Sampled Images =============
        logger.Information("Sampled Images:");
        var sampledImages = shaderResources.GetResourceListForType(ResourceType.SampledImage);
        foreach (var resource in sampledImages) {
            // Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);

            var arraySize = 42;

            ReflectionData
                .ShaderDescriptorSets
                .GetOrCreateDefault(resource.DescriptorSet)
                .ImageSamplers[resource.BindingPoint] = new() {
                BindingPoint = resource.BindingPoint,
                DescriptorSet = resource.DescriptorSet,
                Name = resource.Name,
                ShaderStage = shaderStage
                // TODO: Dimension, ArraySize
            };

            logger.Information(
                "{Name} ({DescriptorSet}, {Binding})",
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint
            );

            ReflectionData.Resources[resource.Name] = new(
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint,
                arraySize
            );
        }

        // ============= Separate Images =============
        logger.Information("Separate Images:");
        var separateImages = shaderResources.GetResourceListForType(ResourceType.SeparateImage);
        foreach (var resource in separateImages) {
            // Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);
            var arraySize = 42;

            ReflectionData
                .ShaderDescriptorSets
                .GetOrCreateDefault(resource.DescriptorSet)
                .SeparateTextures[resource.BindingPoint] = new() {
                BindingPoint = resource.BindingPoint,
                DescriptorSet = resource.DescriptorSet,
                Name = resource.Name,
                ShaderStage = shaderStage
                // TODO: Dimension, ArraySize
            };

            logger.Information(
                "{Name} ({DescriptorSet}, {Binding})",
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint
            );

            ReflectionData.Resources[resource.Name] = new(
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint,
                arraySize
            );
        }

        // ============= Separate Samplers =============
        logger.Information("Separate Samplers:");
        var separateSamplers = shaderResources.GetResourceListForType(ResourceType.SeparateSamplers);
        foreach (var resource in separateSamplers) {
            // Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);

            var arraySize = 42;

            ReflectionData
                .ShaderDescriptorSets
                .GetOrCreateDefault(resource.DescriptorSet)
                .SeparateSamplers[resource.BindingPoint] = new() {
                BindingPoint = resource.BindingPoint,
                DescriptorSet = resource.DescriptorSet,
                Name = resource.Name,
                ShaderStage = shaderStage
                // TODO: Dimension, ArraySize
            };

            logger.Information(
                "{Name} ({DescriptorSet}, {Binding})",
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint
            );

            ReflectionData.Resources[resource.Name] = new(
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint,
                arraySize
            );
        }

        // ============= Storage Images =============
        logger.Information("Storage Images:");
        var storageImages = shaderResources.GetResourceListForType(ResourceType.StorageImage);
        foreach (var resource in storageImages) {
            // Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);

            var arraySize = 42;

            ReflectionData
                .ShaderDescriptorSets
                .GetOrCreateDefault(resource.DescriptorSet)
                .StorageImages[resource.BindingPoint] = new() {
                BindingPoint = resource.BindingPoint,
                DescriptorSet = resource.DescriptorSet,
                Name = resource.Name,
                ShaderStage = shaderStage
                // TODO: Dimension, ArraySize
            };

            logger.Information(
                "{Name} ({DescriptorSet}, {Binding})",
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint
            );

            ReflectionData.Resources[resource.Name] = new(
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint,
                arraySize
            );
        }
    }

    ShaderUniformType SpirTypeToShaderUniformType(int type) =>
        // TODO
        ShaderUniformType.None;

    ShaderKind ShaderStageToShaderC(ShaderStageFlags flags) =>
        flags switch {
            ShaderStageFlags.VertexBit => ShaderKind.VertexShader,
            ShaderStageFlags.FragmentBit => ShaderKind.FragmentShader,
            ShaderStageFlags.ComputeBit => ShaderKind.ComputeShader,
            _ => throw new NotImplementedException()
        };
}

// TODO: include shader in ctor
public class ShaderCompilationException : Exception {
    public ShaderCompilationException() { }
    public ShaderCompilationException(string message) : base(message) { }
    public ShaderCompilationException(string message, Exception inner) : base(message, inner) { }
}
