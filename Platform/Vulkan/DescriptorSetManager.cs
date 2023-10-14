using Rin.Core.Abstractions;
using Rin.Platform.Abstractions.Rendering;
using Rin.Rendering;
using Serilog;
using Silk.NET.Vulkan;
using System.Buffers;

namespace Rin.Platform.Vulkan;

sealed class DescriptorSetManager {
    readonly ILogger log = Log.ForContext<DescriptorSetManager>();

    readonly DescriptorSetManagerOptions options;
    readonly Dictionary<int, Dictionary<int, RenderPassInput>> inputResources = new();
    readonly Dictionary<int, Dictionary<int, RenderPassInput>> invalidatedInputResources = new();
    readonly Dictionary<string, RenderPassInputDeclaration> inputDeclarations = new();

    // [frameIndex][set][binding]
    readonly List<Dictionary<int, Dictionary<int, WriteDescriptor>>> writeDescriptorMap = new();

    // Per Frame in Flight
    readonly List<List<DescriptorSet>> descriptorSets = new();

    DescriptorPool descriptorPool;
    public bool HasDescriptorSets => descriptorSets.Count > 0 && descriptorSets[0].Count > 0;
    public IReadOnlyList<List<DescriptorSet>> DescriptorSets => descriptorSets.AsReadOnly();

    public DescriptorSetManager(DescriptorSetManagerOptions options) {
        this.options = options;
        Initialize();
    }

    public void SetInput(string name, IUniformBuffer uniformBuffer) {
        if (inputDeclarations.TryGetValue(name, out var declaration)) {
            inputResources[declaration.Set][declaration.Binding].Set(uniformBuffer);
        } else {
            log.Warning("Render Pass {RenderPassName} - Input {InputName} not found", options.DebugName, name);
        }
    }

    public void SetInput(string name, IUniformBufferSet uniformBufferSet) {
        if (inputDeclarations.TryGetValue(name, out var declaration)) {
            inputResources[declaration.Set][declaration.Binding].Set(uniformBufferSet);
        } else {
            log.Warning("Render Pass {RenderPassName} - Input {InputName} not found", options.DebugName, name);
        }
    }

    public void SetInput(string name, IStorageBuffer storageBuffer) {
        if (inputDeclarations.TryGetValue(name, out var declaration)) {
            inputResources[declaration.Set][declaration.Binding].Set(storageBuffer);
        } else {
            log.Warning("Render Pass {RenderPassName} - Input {InputName} not found", options.DebugName, name);
        }
    }

    public void SetInput(string name, IStorageBufferSet storageBufferSet) {
        if (inputDeclarations.TryGetValue(name, out var declaration)) {
            inputResources[declaration.Set][declaration.Binding].Set(storageBufferSet);
        } else {
            log.Warning("Render Pass {RenderPassName} - Input {InputName} not found", options.DebugName, name);
        }
    }

    public void SetInput(string name, ITexture2D texture, int index = 0) {
        if (inputDeclarations.TryGetValue(name, out var declaration)) {
            inputResources[declaration.Set][declaration.Binding].Set(texture, index);
        } else {
            log.Warning("Render Pass {RenderPassName} - Input {InputName} not found", options.DebugName, name);
        }
    }

    public void SetInput(string name, ITextureCube textureCube) {
        if (inputDeclarations.TryGetValue(name, out var declaration)) {
            inputResources[declaration.Set][declaration.Binding].Set(textureCube);
        } else {
            log.Warning("Render Pass {RenderPassName} - Input {InputName} not found", options.DebugName, name);
        }
    }

    public void SetInput(string name, IImage2D image) {
        if (inputDeclarations.TryGetValue(name, out var declaration)) {
            inputResources[declaration.Set][declaration.Binding].Set(image);
        } else {
            log.Warning("Render Pass {RenderPassName} - Input {InputName} not found", options.DebugName, name);
        }
    }

    public void SetInput(string name, IImageView imageView) {
        if (inputDeclarations.TryGetValue(name, out var declaration)) {
            inputResources[declaration.Set][declaration.Binding].Set(imageView);
        } else {
            log.Warning("Render Pass {RenderPassName} - Input {InputName} not found", options.DebugName, name);
        }
    }

    public unsafe void Bake() {
        Log.Information("Start baking");
        if (!Validate()) {
            log.Error("[Render Pass ({RenderPass})] Bake - Validation failed", options.DebugName);
        }

        DescriptorPoolSize[] poolSizes = {
            new(DescriptorType.Sampler, 1000), new(DescriptorType.CombinedImageSampler, 1000),
            new(DescriptorType.SampledImage, 1000), new(DescriptorType.StorageImage, 1000),
            new(DescriptorType.UniformTexelBuffer, 1000), new(DescriptorType.StorageTexelBuffer, 1000),
            new(DescriptorType.UniformBuffer, 1000), new(DescriptorType.StorageBuffer, 1000),
            new(DescriptorType.UniformBufferDynamic, 1000), new(DescriptorType.StorageBufferDynamic, 1000),
            new(DescriptorType.InputAttachment, 1000)
        };

        var vk = VulkanContext.Vulkan;
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;
        fixed (DescriptorPoolSize* poolSizesPtr = poolSizes) {
            var poolInfo = new DescriptorPoolCreateInfo(StructureType.DescriptorPoolCreateInfo) {
                Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit,
                MaxSets = 11 * 3, // TODO: was 10 but poolSizes is larger
                PoolSizeCount = 11, // TODO: was 10
                PPoolSizes = poolSizesPtr
            };

            VulkanContext.Vulkan.CreateDescriptorPool(device, poolInfo, null, out var pool).EnsureSuccess();
            descriptorPool = pool;
        }

        var descriptorSetCount = Renderer.Options.FramesInFlight;
        // TODO: not sure about this
        descriptorSets.Clear();

        Log.Information("Input Declarations: {Variable}", inputDeclarations);
        Log.Information("Input Resources: {Variable}", inputResources);
        foreach (var (set, setData) in inputResources) {
            for (var frameIndex = 0; frameIndex < descriptorSetCount; frameIndex++) {
                var dsl = options.Shader.DescriptorSetLayouts[set];

                var descriptorSetAllocInfo = new DescriptorSetAllocateInfo(StructureType.DescriptorSetAllocateInfo) {
                    PSetLayouts = &dsl, DescriptorSetCount = 1, DescriptorPool = descriptorPool
                };

                vk.AllocateDescriptorSets(device, descriptorSetAllocInfo, out var descriptorSet).EnsureSuccess();
                descriptorSets.Add(new());
                descriptorSets[frameIndex].Add(descriptorSet);

                var writeDescriptorMap = this.writeDescriptorMap[frameIndex][set];
                List<List<DescriptorImageInfo>> imageInfos = new();

                Log.Information("Set: {Variable}", set);
                foreach (var (binding, input) in setData) {
                    var storedWriteDescriptor = writeDescriptorMap[binding];
                    storedWriteDescriptor.WriteDescriptorSet = storedWriteDescriptor.WriteDescriptorSet with {
                        DstSet = descriptorSet
                    };

                    switch (input.Type) {
                        case RenderPassResourceType.StorageBuffer:
                        case RenderPassResourceType.UniformBuffer: {
                            var buffer = input.Input[0] as IVulkanBuffer;
                            SetBuffer(
                                ref storedWriteDescriptor,
                                buffer.DescriptorBufferInfo,
                                input,
                                set,
                                binding,
                                true
                            );
                            break;
                        }

                        case RenderPassResourceType.StorageBufferSet:
                        case RenderPassResourceType.UniformBufferSet: {
                            var buffer = input.Input[0] as IVulkanBufferSet;

                            SetBuffer(
                                ref storedWriteDescriptor,
                                ((VulkanUniformBuffer)buffer.GetVulkanBuffer(frameIndex)).DescriptorBufferInfo,
                                input,
                                set,
                                binding,
                                true
                            );
                            break;
                        }

                        case RenderPassResourceType.Texture2D:
                            throw new NotImplementedException();

                        case RenderPassResourceType.TextureCube:
                            throw new NotImplementedException();

                        case RenderPassResourceType.Image2D:
                            throw new NotImplementedException();

                        default: throw new ArgumentOutOfRangeException();
                    }

                    writeDescriptorMap[binding] = storedWriteDescriptor;
                }

                List<WriteDescriptorSet> writeDescriptors = new();
                foreach (var (binding, input) in writeDescriptorMap) {
                    if (!IsInvalidated(set, binding)) {
                        writeDescriptors.Add(input.WriteDescriptorSet);
                        Log.Information("Debug: {Variable}", input.WriteDescriptorSet.DescriptorType);
                    }
                }

                if (writeDescriptors.Count > 0) {
                    vk.UpdateDescriptorSets(device, writeDescriptors.ToArray(), 0, null);
                    log.Debug("Render pass update {Size} descriptors in set {Set}", writeDescriptors.Count, set);
                }
            }
        }
    }

    public unsafe void InvalidateAndUpdate() {
        var currentFrameIndex = Renderer.CurrentFrameIndex_RT;

        foreach (var (set, inputs) in inputResources) {
            foreach (var (binding, input) in inputs) {
                Log.Information("{set} {binfing} {frame}", set, binding, currentFrameIndex);
                var resourceHandlers = writeDescriptorMap[currentFrameIndex][set][binding].ResourceHandlers;
                
                switch (input.Type) {
                    case RenderPassResourceType.StorageBuffer:
                    case RenderPassResourceType.UniformBuffer: {
                        var bufferInfo = ((IVulkanBuffer)input.Input[0]).DescriptorBufferInfo;
                        if (!resourceHandlers.Contains(bufferInfo.Buffer)) {
                            invalidatedInputResources[set][binding] = input;
                        }
                        break;
                    }
                    
                    case RenderPassResourceType.StorageBufferSet:
                    case RenderPassResourceType.UniformBufferSet: {
                        var inputSet = (IVulkanBufferSet)input.Input[0];
                        var bufferInfo = inputSet.GetVulkanBuffer(currentFrameIndex).DescriptorBufferInfo;
                        if (!resourceHandlers.Contains(bufferInfo.Buffer)) {
                            invalidatedInputResources[set][binding] = input;
                        }
                        break;
                    }
                    
                    case RenderPassResourceType.Texture2D:
                        throw new NotImplementedException();
                    case RenderPassResourceType.TextureCube:
                        throw new NotImplementedException();
                    case RenderPassResourceType.Image2D:
                        throw new NotImplementedException();
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        if (invalidatedInputResources.Count == 0) {
            return;
        }

        foreach (var (set, setData) in invalidatedInputResources) {
            List<WriteDescriptorSet> updateDescriptors = new();

            foreach (var (binding, input) in setData) {
                var wd = writeDescriptorMap[currentFrameIndex][set][binding];
                var storedWriteDescriptor = wd.WriteDescriptorSet;
                
                switch (input.Type) {
                    case RenderPassResourceType.StorageBuffer:
                    case RenderPassResourceType.UniformBuffer: {
                        var buffer = input.Input[0] as IVulkanBuffer;

                        var dbi = new[] { buffer.DescriptorBufferInfo };
                        var memHandle = new Memory<DescriptorBufferInfo>(dbi).Pin();
                        storedWriteDescriptor.PBufferInfo = (DescriptorBufferInfo*)memHandle.Pointer;

                        wd.ResourceMemmoryHandlers[0] = memHandle;
                        wd.ResourceHandlers[0] = storedWriteDescriptor.PBufferInfo->Buffer;
                        break;
                    }

                    case RenderPassResourceType.StorageBufferSet:
                    case RenderPassResourceType.UniformBufferSet: {
                        var bufferSet = input.Input[0] as IVulkanBufferSet;
                        var buffer = bufferSet.GetVulkanBuffer(currentFrameIndex);

                        var dbi = new[] { buffer.DescriptorBufferInfo };
                        var memHandle = new Memory<DescriptorBufferInfo>(dbi).Pin();
                        storedWriteDescriptor.PBufferInfo = (DescriptorBufferInfo*)memHandle.Pointer;

                        wd.ResourceMemmoryHandlers[0] = memHandle;
                        wd.ResourceHandlers[0] = storedWriteDescriptor.PBufferInfo->Buffer;
                        break;
                    }
                    
                    case RenderPassResourceType.Texture2D:
                        throw new NotImplementedException();
                    case RenderPassResourceType.TextureCube:
                        throw new NotImplementedException();
                    case RenderPassResourceType.Image2D:
                        throw new NotImplementedException();
                }

                wd.WriteDescriptorSet = storedWriteDescriptor;
                writeDescriptorMap[currentFrameIndex][set][binding] = wd;
            }

            Log.Information("invalidating=========");
            
            VulkanContext.Vulkan.UpdateDescriptorSets(
                VulkanContext.CurrentDevice.VkLogicalDevice,
                updateDescriptors.ToArray(),
                0, null
                );
        }

        invalidatedInputResources.Clear();
    }

    public bool Validate() {
        var shaderDescriptorSets = options.Shader.ShaderDescriptorSets;

        for (var set = options.StartSet; set <= options.EndSet; set++) {
            if (set >= shaderDescriptorSets.Count) {
                break;
            }

            if (!inputResources.TryGetValue(set, out var setInputResources)) {
                log.Error("[Render Pass ({RenderPass})] No input resources for set {Set}", options.DebugName, set);
                return false;
            }

            var shaderDescriptor = shaderDescriptorSets[set];
            foreach (var entry in shaderDescriptor.WriteDescriptorSets) {
                var binding = (int)entry.Value.DstBinding;
                if (!setInputResources.ContainsKey(binding)) {
                    log.Error(
                        "[Render Pass ({RenderPass})] No input resource for {Set}.{Binding}",
                        options.DebugName,
                        set,
                        binding
                    );

                    log.Error(
                        "[Render Pass ({RenderPass})] Required resource is {Name} ({Type})",
                        options.DebugName,
                        entry.Key,
                        entry.Value.DescriptorType
                    );
                    return false;
                }

                // TODO: finish these checks. Not important for now
            }
        }

        return true;
    }

    unsafe void SetBuffer(
        ref WriteDescriptor storedWriteDescriptor,
        in DescriptorBufferInfo bufferInfo,
        in RenderPassInput input,
        int set,
        int binding,
        bool defer
    ) {
        var dbi = new[] { bufferInfo };
        var memHandle = new Memory<DescriptorBufferInfo>(dbi).Pin();
        storedWriteDescriptor.WriteDescriptorSet = storedWriteDescriptor.WriteDescriptorSet with {
            PBufferInfo = (DescriptorBufferInfo*)memHandle.Pointer
        };

        storedWriteDescriptor.ResourceMemmoryHandlers.Add(memHandle);
        // TODO: not sure about this
        storedWriteDescriptor.ResourceHandlers.Add(storedWriteDescriptor.WriteDescriptorSet.PBufferInfo->Buffer);

        // Defer if resource doesn't exist
        if (defer && storedWriteDescriptor.WriteDescriptorSet.PBufferInfo->Buffer.Handle == 0) {
            invalidatedInputResources[set][binding] = input;
        }
    }

    void Initialize() {
        var shaderDescriptorSets = options.Shader.ShaderDescriptorSets;
        var framesInFlight = Renderer.Options.FramesInFlight;

        for (var set = options.StartSet; set <= options.EndSet; set++) {
            if (set >= shaderDescriptorSets.Count) {
                break;
            }

            var shaderDescriptor = shaderDescriptorSets[set];
            foreach (var (name, writeDescriptorSet) in shaderDescriptor.WriteDescriptorSets) {
                var binding = (int)writeDescriptorSet.DstBinding;
                var inputDeclaration = new RenderPassInputDeclaration(
                    writeDescriptorSet.DescriptorType.ToRenderPassInputType(),
                    set,
                    binding,
                    (int)writeDescriptorSet.DescriptorCount,
                    name
                );

                if (options.DefaultResources || true) {
                    inputResources.GetOrCreateDefault(set)[binding] =
                        new() { Type = writeDescriptorSet.DescriptorType.GetDefaultResourceType() };

                    if (inputDeclaration.Type == RenderPassInputType.ImageSampler2D) {
                        for (var i = 0; i < writeDescriptorSet.DescriptorCount; i++) {
                            inputResources[set][binding].Input.Add(i, Renderer.WhiteTexture);
                        }
                    } else if (inputDeclaration.Type == RenderPassInputType.ImageSampler3D) {
                        for (var i = 0; i < writeDescriptorSet.DescriptorCount; i++) {
                            inputResources[set][binding].Input.Add(i, Renderer.BlackCubeTexture);
                        }
                    }
                }

                for (var frameIndex = 0; frameIndex < framesInFlight; frameIndex++) {
                    writeDescriptorMap.Add(new());
                    writeDescriptorMap[frameIndex].GetOrCreateDefault(set)[binding] = new() {
                        WriteDescriptorSet = writeDescriptorSet
                    };
                }

                if (shaderDescriptor.ImageSamplers.TryGetValue(binding, out var imageSampler)) {
                    if (writeDescriptorSet.DescriptorType is DescriptorType.SampledImage
                        or DescriptorType.CombinedImageSampler) {
                        var type = imageSampler.Dimension switch {
                            1 => RenderPassInputType.ImageSampler1D,
                            2 => RenderPassInputType.ImageSampler2D,
                            3 => RenderPassInputType.ImageSampler3D,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        inputDeclaration = inputDeclaration with { Type = type };
                    } else if (writeDescriptorSet.DescriptorType == DescriptorType.StorageImage) {
                        var type = imageSampler.Dimension switch {
                            1 => RenderPassInputType.StorageImage1D,
                            2 => RenderPassInputType.StorageImage2D,
                            3 => RenderPassInputType.StorageImage3D,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        inputDeclaration = inputDeclaration with { Type = type };
                    }
                }

                inputDeclarations[name] = inputDeclaration;
            }
        }
    }

    bool IsInvalidated(int set, int binding) =>
        invalidatedInputResources.TryGetValue(set, out var bindings) && bindings.ContainsKey(binding);


    struct WriteDescriptor {
        public WriteDescriptorSet WriteDescriptorSet { get; set; }
        public List<object> ResourceHandlers { get; set; } = new();
        public List<MemoryHandle> ResourceMemmoryHandlers { get; set; } = new();

        public WriteDescriptor() { }
    }
}
