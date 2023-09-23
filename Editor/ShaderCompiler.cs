using Serilog;
using Silk.NET.Shaderc;

namespace Rin.Editor;

// reference: https://github.com/LWJGL/lwjgl3-demos/blob/main/src/org/lwjgl/demo/vulkan/VKUtil.java#L62-L63
// TODO: we should wait with this until stable release of Silk.NET.Shaderc is released because interfacing with C is pain
public class ShaderCompiler {
    public unsafe void Test() {
        // Compiler
        // Spir
        var api = Shaderc.GetApi();
        var compiler = api.CompilerInitialize();
        var options = api.CompileOptionsInitialize();

        api.CompileOptionsSetTargetEnv(options, TargetEnv.Opengl, (uint)EnvVersion.Opengl45);
        api.CompileOptionsSetTargetSpirv(options, SpirvVersion.Shaderc16);
        api.CompileOptionsSetOptimizationLevel(options, OptimizationLevel.Performance);

        api.CompileOptionsSetIncludeCallbacks(
            options,
            PfnIncludeResolveFn.From(IncludeResolver),
            PfnIncludeResultReleaseFn.From(IncludeReleaser),
            null
        );

        var entryPoint = "main";
        var path = "/foo/bar";
        var testString = "Foo Bar";

        var result = api.CompileIntoSpv(
            compiler,
            (byte*)&testString,
            (UIntPtr)testString.Length,
            ShaderKind.VertexShader,
            (byte*)&path,
            (byte*)&entryPoint,
            options
        );

        if (result == null) {
            Log.Error("Result is null");
        }
    }

    unsafe void IncludeReleaser(void* arg0, IncludeResult* arg1) {
        throw new NotImplementedException();
    }

    unsafe IncludeResult* IncludeResolver(
        void* userData,
        byte* requestedResource,
        int type,
        byte* requestingResource,
        UIntPtr includePath
    ) {
        // fixed??
        var res = new IncludeResult();

        // var requestedName = 


        return &res;
    }
}
