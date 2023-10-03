using Rin.Core.Abstractions;
using Rin.Platform.Rendering;
using Serilog;
using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan;

public sealed class DescriptorSetManager {
    readonly DescriptorSetManagerOptions options;
    readonly Dictionary<int, Dictionary<int, RenderPassInput>> inputResources = new();
    readonly Dictionary<int, Dictionary<int, RenderPassInput>> invalidatedInputResources = new();
    readonly Dictionary<string, RenderPassInputDeclaration> inputDeclarations = new();

    List<Dictionary<int, Dictionary<int, WriteDescriptor>>> writeDescriptorMap = new();


    public DescriptorSetManager(DescriptorSetManagerOptions options) {
        this.options = options;
        Init();
    }


    void Init() {
        var framesInFlight = Renderer.Options.FramesInFlight;

        for (var set = options.StartSet; set < options.StopSet; set++) {
            // TODO
        }
    }

    void SetInput(string name, IUniformBuffer uniformBuffer) {
        if (inputDeclarations.TryGetValue(name, out var declaration)) {
            inputResources[declaration.Set][declaration.Binding].Set(uniformBuffer);
        } else {
            Log.Warning("Render Pass {RenderPassName} - Input {InputName} not found", options.DebugName, name);
        }
    }

    void SetInput(string name, IUniformBufferSet uniformBufferSet) {
        if (inputDeclarations.TryGetValue(name, out var declaration)) {
            inputResources[declaration.Set][declaration.Binding].Set(uniformBufferSet);
        } else {
            Log.Warning("Render Pass {RenderPassName} - Input {InputName} not found", options.DebugName, name);
        }
    }

    void SetInput(string name, IStorageBuffer storageBuffer) {
        if (inputDeclarations.TryGetValue(name, out var declaration)) {
            inputResources[declaration.Set][declaration.Binding].Set(storageBuffer);
        } else {
            Log.Warning("Render Pass {RenderPassName} - Input {InputName} not found", options.DebugName, name);
        }
    }

    void SetInput(string name, IStorageBufferSet storageBufferSet) {
        if (inputDeclarations.TryGetValue(name, out var declaration)) {
            inputResources[declaration.Set][declaration.Binding].Set(storageBufferSet);
        } else {
            Log.Warning("Render Pass {RenderPassName} - Input {InputName} not found", options.DebugName, name);
        }
    }

    // TODO: other sets

    bool IsInvalidated(int set, int binding) =>
        invalidatedInputResources.TryGetValue(set, out var bindings) && bindings.ContainsKey(binding);


    unsafe void Bake() {
        // TODO: validate

        DescriptorPoolSize[] poolSizes = {
            new(DescriptorType.Sampler, 1000), new(DescriptorType.CombinedImageSampler, 1000),
            new(DescriptorType.SampledImage, 1000), new(DescriptorType.StorageImage, 1000),
            new(DescriptorType.UniformTexelBuffer, 1000), new(DescriptorType.StorageTexelBuffer, 1000),
            new(DescriptorType.UniformBuffer, 1000), new(DescriptorType.StorageBuffer, 1000),
            new(DescriptorType.UniformBufferDynamic, 1000), new(DescriptorType.StorageBufferDynamic, 1000),
            new(DescriptorType.InputAttachment, 1000)
        };

        fixed (DescriptorPoolSize* poolSizesPtr = poolSizes) {
            var poolInfo = new DescriptorPoolCreateInfo(StructureType.DescriptorPoolCreateInfo) {
                Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit,
                MaxSets = 11 * 3, // TODO: was 10 but poolSizes is larger
                PoolSizeCount = 11, // TODO: was 10
                PPoolSizes = poolSizesPtr
            };

            var device = VulkanContext.CurrentDevice.VkLogicalDevice;
            VulkanContext.Vulkan.CreateDescriptorPool(device, poolInfo, null, out var pool);
        }

        // TODO: finish this
    }


    struct WriteDescriptor {
        public WriteDescriptorSet WriteDescriptorSet { get; set; }
        public List<object> ResourceHandlers { get; set; }
    }
}
