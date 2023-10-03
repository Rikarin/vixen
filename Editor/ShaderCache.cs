using MessagePack;
using MessagePack.Resolvers;
using Rin.Platform.Vulkan;
using Silk.NET.Vulkan;
using System.Security.Cryptography;
using System.Text;

namespace Rin.Editor;

public static class ShaderCache {
    static string CacheDirectory => Path.Combine(Project.OpenProject!.CacheDirectory, "Shaders");

    public static bool HasChanged(ShaderCompiler compiler) =>
        throw
            // calculate crc or something from compiler source code
            // lookup for cache file on disk
            // if found, deserialize
            new NotImplementedException();

    public static void Serialize(ShaderCompiler compiler) {
        if (compiler.ProgramSource == null) {
            return;
        }

        var hash = BitConverter
            .ToString(SHA256.HashData(Encoding.UTF8.GetBytes(compiler.ProgramSource)))
            .Replace("-", "")
            .ToLowerInvariant();

        var cache = new ShaderCacheInfo {
            CompiledShaders = new() { { ShaderStageFlags.FragmentBit, new byte[] { 0, 2, 4 } } },
            ReflectionData = compiler.ReflectionData
        };

        var bytes = MessagePackSerializer.Serialize(cache, ContractlessStandardResolverAllowPrivate.Options);
        var json = MessagePackSerializer.ConvertToJson(bytes);

        File.WriteAllBytes(Path.Combine(CacheDirectory, hash + ".bin"), bytes);
        File.WriteAllText(Path.Combine(CacheDirectory, hash + ".json"), json);
        // TODO: json is for debuging purpose only
    }

    [Serializable]
    class ShaderCacheInfo {
        // TODO: use our custom enum
        public Dictionary<ShaderStageFlags, byte[]> CompiledShaders { get; set; }
        public ShaderResource.ReflectionData ReflectionData { get; set; }
    }
}
