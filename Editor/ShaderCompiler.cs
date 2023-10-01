using Rin.Core.Abstractions.Shaders;
using Rin.Core.General;
using Rin.Editor.RinCompiler;
using Rin.Editor.RinCompiler.Parser;
using Rin.Platform.Vulkan;
using Serilog;
using Silk.NET.Shaderc;
using Silk.NET.SPIRV.Cross;
using Silk.NET.Vulkan;
using System.Runtime.InteropServices;

namespace Rin.Editor;

// reference: https://github.com/LWJGL/lwjgl3-demos/blob/main/src/org/lwjgl/demo/vulkan/VKUtil.java#L62-L63
public class ShaderCompiler {
    readonly string shaderPath;
    string? programSource;
    string? name;

    static string CacheDirectory => Path.Combine(Project.OpenProject!.CacheDirectory, "Shaders");

    readonly ILogger logger = Log.ForContext<ShaderCompiler>();
    // TODO: these are generic reflection data of the spir-v file not vulcan-only
    readonly VulkanShader.ReflectionData reflectionData = new();

    ShaderCompiler(string shaderPath) {
        this.shaderPath = shaderPath;
    }

    public static Shader Compile(string shaderPath, bool forceCompile, bool debug) {
        var compiler = new ShaderCompiler(Path.Combine(Project.OpenProject!.RootDirectory, shaderPath));
        compiler.Reload(forceCompile);
        
        // TODO: load this from our shader compiler
        var shader = new Shader(null!, compiler.name, shaderPath);
        

        Log.Information("Debug: {Variable}", compiler.name);
        
        
        return shader;
    }

    public void Reload(bool forceCompile) {
        name = null;
        programSource = null;

        Directory.CreateDirectory(CacheDirectory);
        
        var source = File.ReadAllText(shaderPath);
        Preprocess(source);
    }
    
    // static Compile and TryRecompile(Shader)

    void Preprocess(string source) {
        var tokenRange = new TokenRange(source);
        var ast = new Parser().Parse(tokenRange);

        var builder = new ShaderBuilder();
        builder.Visit(ast);

        name = builder.Name;
        programSource = builder.ProgramSource;
    }
    
    
    
    
    

    public unsafe void Compile(string shader, bool debug) {
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
        var fileName = Marshal.StringToHGlobalAnsi("foo/bar.hlsl");
        var entryPoint = Marshal.StringToHGlobalAnsi("main");

        var result = api.CompileIntoSpv(
            compiler,
            (byte*)content,
            (UIntPtr)shader.Length,
            ShaderKind.FragmentShader,
            (byte*)fileName,
            (byte*)entryPoint,
            options
        );

        Marshal.FreeHGlobal(content);
        Marshal.FreeHGlobal(fileName);
        Marshal.FreeHGlobal(entryPoint);

        if (api.ResultGetCompilationStatus(result) != CompilationStatus.Success) {
            throw new ShaderCompilationException(api.ResultGetErrorMessageS(result));
        }

        var compiled = new Span<byte>(api.ResultGetBytes(result), (int)api.ResultGetLength(result));
        // TODO: test only
        Reflect(compiled, ShaderStageFlags.FragmentBit);

        api.ResultRelease(result);
        api.CompilerRelease(compiler);
    }

    unsafe void IncludeReleaser(void* userData, IncludeResult* includeResult) {
        Marshal.FreeHGlobal((nint)includeResult->Content);
        Marshal.FreeHGlobal((nint)includeResult->SourceName);
        Marshal.FreeHGlobal((nint)includeResult);
        Log.Information("Releaser");
    }

    unsafe IncludeResult* IncludeResolver(
        void* userData,
        byte* requestedResource,
        int type,
        byte* requestingResource,
        UIntPtr includePath
    ) {
        var result = (IncludeResult*)Marshal.AllocHGlobal(sizeof(IncludeResult));
        var reqested = Marshal.PtrToStringAnsi((nint)requestedResource);

        var name = "asdfasdf.hlsl";
        var content = """
                      [[vk::image_format("rgba8")]]
                      RWBuffer<float4> Buf;

                      Texture2D texture2D42l;

                      [[vk::image_format("rg16f")]]
                      RWTexture2D<float2> Tex;

                      RWTexture2D<float2> Tex2; // Works like before

                      struct aasdf {
                        float4x4 foobar;
                      };
                      [[vk::binding(2,2)]] ConstantBuffer<aasdf> u_aasdf;
                      """;
        result->SourceName = (byte*)Marshal.StringToHGlobalAnsi(name);
        result->SourceNameLength = (nuint)name.Length;
        result->Content = (byte*)Marshal.StringToHGlobalAnsi(content);
        result->ContentLength = (nuint)content.Length;

        // Log.Information("Resolver {str}", str);

        return result;
    }

    void Reflect(ReadOnlySpan<byte> irCode, ShaderStageFlags shaderStage) {
        using var cross = new CrossWrapper();
        var compiler = cross.CreateCompiler(irCode);
        var shaderResources = compiler.CreateShaderResources();

        for (var i = 1; i < 15; i++) {
            var res = shaderResources.GetResourceListForType((ResourceType)i);
            foreach (var resource in res) {
                Log.Information("Debug: {a} {Variable}", (ResourceType)i, resource.Name);
            }
        }

        // ============= Uniform Buffers =============
        logger.Information("Uniform Buffers:");
        var uniformBuffers = shaderResources.GetResourceListForType(ResourceType.UniformBuffer);
        foreach (var resource in uniformBuffers) {
            Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);

            reflectionData
                .ShaderDescriptorSets
                .GetOrCreateDefault(resource.DescriptorSet)
                .UniformBuffers[resource.BindingPoint] = new() {
                BindingPoint = resource.BindingPoint,
                Size = resource.DeclaredStructSize,
                Name = resource.Name,
                ShaderStage = ShaderStageFlags.All
            };

            Log.Information(
                "{Name} ({DescriptorSet}, {Binding})",
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint
            );

            Log.Information("Member Count: {MemberCount}", resource.MemberCount);
            Log.Information("Size: {Size}", resource.DeclaredStructSize);
            Log.Information("---------------------");
        }

        // ============= Storage Buffers =============
        logger.Information("Storage Buffers:");
        var storageBuffers = shaderResources.GetResourceListForType(ResourceType.StorageBuffer);
        foreach (var resource in storageBuffers) {
            Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);

            reflectionData
                .ShaderDescriptorSets
                .GetOrCreateDefault(resource.DescriptorSet)
                .StorageBuffers[resource.BindingPoint] = new() {
                BindingPoint = resource.BindingPoint,
                Size = resource.DeclaredStructSize,
                Name = resource.Name,
                ShaderStage = ShaderStageFlags.All
            };

            Log.Information(
                "{Name} ({DescriptorSet}, {Binding})",
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint
            );

            Log.Information("Member Count: {MemberCount}", resource.MemberCount);
            Log.Information("Size: {Size}", resource.DeclaredStructSize);
            Log.Information("---------------------");
        }

        // ============= Push Constants Buffers =============
        logger.Information("Push Constant Buffers:");
        var pushConstantBuffers = shaderResources.GetResourceListForType(ResourceType.PushConstant);
        foreach (var resource in pushConstantBuffers) {
            Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);

            var bufferOffset = 0;
            if (reflectionData.PushConstantRanges.Count > 0) {
                var last = reflectionData.PushConstantRanges[^1];
                bufferOffset = last.Offset + last.Size;
            }

            reflectionData.PushConstantRanges.Add(
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

                Log.Information("Debug: {Variable} {DAta}", uniformName, memberData);
            }

            reflectionData.ConstantBuffers[resource.Name] = shaderBuffer;

            Log.Information("{Name}", resource.Name);
            Log.Information("Member Count: {MemberCount}", resource.MemberCount);
            Log.Information("Size: {Size}", resource.DeclaredStructSize);
            Log.Information("---------------------");
        }

        // ============= Sampled Images =============
        logger.Information("Sampled Images:");
        var sampledImages = shaderResources.GetResourceListForType(ResourceType.SampledImage);
        foreach (var resource in sampledImages) {
            Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);

            var arraySize = 42;

            reflectionData
                .ShaderDescriptorSets
                .GetOrCreateDefault(resource.DescriptorSet)
                .ImageSamplers[resource.BindingPoint] = new() {
                BindingPoint = resource.BindingPoint,
                DescriptorSet = resource.DescriptorSet,
                Name = resource.Name,
                ShaderStage = shaderStage
                // TODO: Dimension, ArraySize
            };

            Log.Information(
                "{Name} ({DescriptorSet}, {Binding})",
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint
            );

            reflectionData.Resources[resource.Name] = new(
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
            Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);
            var arraySize = 42;

            reflectionData
                .ShaderDescriptorSets
                .GetOrCreateDefault(resource.DescriptorSet)
                .SeparateTextures[resource.BindingPoint] = new() {
                BindingPoint = resource.BindingPoint,
                DescriptorSet = resource.DescriptorSet,
                Name = resource.Name,
                ShaderStage = shaderStage
                // TODO: Dimension, ArraySize
            };

            Log.Information(
                "{Name} ({DescriptorSet}, {Binding})",
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint
            );
            
            reflectionData.Resources[resource.Name] = new(
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
            Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);

            var arraySize = 42;

            reflectionData
                .ShaderDescriptorSets
                .GetOrCreateDefault(resource.DescriptorSet)
                .SeparateSamplers[resource.BindingPoint] = new() {
                BindingPoint = resource.BindingPoint,
                DescriptorSet = resource.DescriptorSet,
                Name = resource.Name,
                ShaderStage = shaderStage
                // TODO: Dimension, ArraySize
            };

            Log.Information(
                "{Name} ({DescriptorSet}, {Binding})",
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint
            );
            
            reflectionData.Resources[resource.Name] = new(
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
            Log.Information("IsActive DEBUG: {Variable}", resource.IsActive);

            var arraySize = 42;

            reflectionData
                .ShaderDescriptorSets
                .GetOrCreateDefault(resource.DescriptorSet)
                .StorageImages[resource.BindingPoint] = new() {
                BindingPoint = resource.BindingPoint,
                DescriptorSet = resource.DescriptorSet,
                Name = resource.Name,
                ShaderStage = shaderStage
                // TODO: Dimension, ArraySize
            };

            Log.Information(
                "{Name} ({DescriptorSet}, {Binding})",
                resource.Name,
                resource.DescriptorSet,
                resource.BindingPoint
            );
            
            reflectionData.Resources[resource.Name] = new(
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
}

// TODO: include shader in ctor
public class ShaderCompilationException : Exception {
    public ShaderCompilationException() { }
    public ShaderCompilationException(string message) : base(message) { }
    public ShaderCompilationException(string message, Exception inner) : base(message, inner) { }
}

static class DictionaryExtensions {
    public static TValue GetOrCreateDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        where TValue : new() where TKey : notnull {
        if (!dictionary.TryGetValue(key, out var value)) {
            value = new();
            dictionary[key] = value;
        }

        return value;
    }
}
