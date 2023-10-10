using Rin.Platform.Abstractions.Rendering;
using Rin.Rendering;
using Serilog;
using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan;

public sealed class DescriptorSetManager {
    readonly ILogger log = Log.ForContext<DescriptorSetManager>();
    
    readonly DescriptorSetManagerOptions options;
    readonly Dictionary<int, Dictionary<int, RenderPassInput>> inputResources = new();
    readonly Dictionary<int, Dictionary<int, RenderPassInput>> invalidatedInputResources = new();
    readonly Dictionary<string, RenderPassInputDeclaration> inputDeclarations = new();

    readonly List<Dictionary<int, Dictionary<int, WriteDescriptor>>> writeDescriptorMap = new();

    // Per Frame in Flight
    readonly List<List<DescriptorSet>> descriptorSets = new();

    DescriptorPool descriptorPool;
    public bool HasDescriptorSets => descriptorSets.Count > 0 && descriptorSets[0].Count > 0;


    public DescriptorSetManager(DescriptorSetManagerOptions options) {
        this.options = options;
        Init();
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

    public void SetInput(string name, ITexture2D texture) {
        if (inputDeclarations.TryGetValue(name, out var declaration)) {
            inputResources[declaration.Set][declaration.Binding].Set(texture);
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

        fixed (DescriptorPoolSize* poolSizesPtr = poolSizes) {
            var poolInfo = new DescriptorPoolCreateInfo(StructureType.DescriptorPoolCreateInfo) {
                Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit,
                MaxSets = 11 * 3, // TODO: was 10 but poolSizes is larger
                PoolSizeCount = 11, // TODO: was 10
                PPoolSizes = poolSizesPtr
            };

            var device = VulkanContext.CurrentDevice.VkLogicalDevice;
            VulkanContext.Vulkan.CreateDescriptorPool(device, poolInfo, null, out var pool).EnsureSuccess();
            descriptorPool = pool;
        }

        // TODO: not sure about this
        descriptorSets.Clear();

        foreach (var (set, setData) in inputResources) {
            
        }
        
        // TODO: finish this



        // foreach (var entry in writeDescriptorMap) {
        //     if (!IsInvalidated(set, entry.Keys))
        // }
    }


    public void InvalidateAndUpdate() {
        throw new NotImplementedException();
    }

    public bool Validate() {
        var shaderDescriptorSets = options.Shader.ShaderDescriptorSets;

        for (var set = options.StartSet; set <= options.StopSet; set++) {
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

    void Init() {
        var shaderDescriptorSets = options.Shader.ShaderDescriptorSets;
        var framesInFlight = Renderer.Options.FramesInFlight;

        for (var set = options.StartSet; set <= options.StopSet; set++) {
            if (set >= shaderDescriptorSets.Count) {
                break;
            }

            var shaderDescriptor = shaderDescriptorSets[set];
            foreach (var entry in shaderDescriptor.WriteDescriptorSets) {
                var binding = (int)entry.Value.DstBinding;
                var inputDeclaration = new RenderPassInputDeclaration(
                    entry.Value.DescriptorType.ToRenderPassInputType(),
                    set,
                    binding,
                    (int)entry.Value.DescriptorCount,
                    entry.Key
                );

                if (options.DefaultResources || true) {
                    inputResources[set][binding] = new() { Type = entry.Value.DescriptorType.GetDefaultResourceType() };

                    if (inputDeclaration.Type == RenderPassInputType.ImageSampler2D) {
                        for (var i = 0; i < entry.Value.DescriptorCount; i++) {
                            inputResources[set][binding].Input.Add(Renderer.WhiteTexture);
                        }
                    } else if (inputDeclaration.Type == RenderPassInputType.ImageSampler3D) {
                        for (var i = 0; i < entry.Value.DescriptorCount; i++) {
                            inputResources[set][binding].Input.Add(Renderer.BlackCubeTexture);
                        }
                    }
                }

                for (var frameIndex = 0; frameIndex < framesInFlight; frameIndex++) {
                    writeDescriptorMap[frameIndex][set][binding] = new() {
                        WriteDescriptorSet = entry.Value, ResourceHandlers = new()
                    };
                }

                if (shaderDescriptor.ImageSamplers.TryGetValue(binding, out var imageSampler)) {
                    if (entry.Value.DescriptorType is DescriptorType.SampledImage
                        or DescriptorType.CombinedImageSampler) {
                        var type = imageSampler.Dimension switch {
                            1 => RenderPassInputType.ImageSampler1D,
                            2 => RenderPassInputType.ImageSampler2D,
                            3 => RenderPassInputType.ImageSampler3D,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        inputDeclaration = inputDeclaration with { Type = type };
                    } else if (entry.Value.DescriptorType == DescriptorType.StorageImage) {
                        var type = imageSampler.Dimension switch {
                            1 => RenderPassInputType.StorageImage1D,
                            2 => RenderPassInputType.StorageImage2D,
                            3 => RenderPassInputType.StorageImage3D,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        inputDeclaration = inputDeclaration with { Type = type };
                    }
                }

                inputDeclarations[entry.Key] = inputDeclaration;
            }
        }
    }

    bool IsInvalidated(int set, int binding) =>
        invalidatedInputResources.TryGetValue(set, out var bindings) && bindings.ContainsKey(binding);


    struct WriteDescriptor {
        public WriteDescriptorSet WriteDescriptorSet { get; set; }
        public List<object> ResourceHandlers { get; set; }
    }
}