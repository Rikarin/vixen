using Silk.NET.Vulkan;
using Vixen.Platform.Common.Rendering;
using Vixen.Rendering;
using PrimitiveTopology = Vixen.Platform.Common.Rendering.PrimitiveTopology;

namespace Vixen.Platform.Vulkan;

sealed class VulkanPipeline : IPipeline, IDisposable {
    PipelineCache cache;

    public Pipeline VkPipeline { get; private set; }
    public PipelineLayout VkLayout { get; private set; }

    public bool IsDynamicLineWidth =>
        Options.Topology is PrimitiveTopology.Lines or PrimitiveTopology.LineStrip || Options.WireFrame;

    public PipelineOptions Options { get; }

    public VulkanPipeline(PipelineOptions options) {
        Options = options;
        Invalidate();

        // TODO: Register Shader Dependency
    }

    public unsafe void Dispose() {
        Renderer.SubmitDisposal(
            () => {
                var device = VulkanContext.CurrentDevice.VkLogicalDevice;
                var vk = VulkanContext.Vulkan;

                vk.DestroyPipeline(device, VkPipeline, null);
                vk.DestroyPipelineCache(device, cache, null);
                vk.DestroyPipelineLayout(device, VkLayout, null);
            }
        );
    }

    unsafe void Invalidate() {
        Renderer.Submit(
            () => {
                var device = VulkanContext.CurrentDevice.VkLogicalDevice;
                var vk = VulkanContext.Vulkan;

                var shader = Options.Shader as VulkanShader;
                var framebuffer = Options.TargetFramebuffer as VulkanFramebuffer;

                // Push Constants
                var pushConstantRange = shader.PushConstantRanges.Select(
                        entry => new PushConstantRange {
                            StageFlags = entry.ShaderStage, Offset = (uint)entry.Offset, Size = (uint)entry.Size
                        }
                    )
                    .ToList();

                // Descriptor Set Layouts
                var descriptorSetLayouts = shader.DescriptorSetLayouts;
                fixed (PushConstantRange* pushConstantRangePtr = pushConstantRange.ToArray())
                fixed (DescriptorSetLayout* descriptorSetLayoutPtr = descriptorSetLayouts.Values.ToArray()) {
                    var pipelineLayoutCreateInfo =
                        new PipelineLayoutCreateInfo(StructureType.PipelineLayoutCreateInfo) {
                            SetLayoutCount = (uint)descriptorSetLayouts.Count,
                            PSetLayouts = descriptorSetLayoutPtr,
                            PushConstantRangeCount = (uint)pushConstantRange.Count,
                            PPushConstantRanges = pushConstantRangePtr
                        };

                    vk.CreatePipelineLayout(device, pipelineLayoutCreateInfo, null, out var pipelineLayout)
                        .EnsureSuccess();
                    VkLayout = pipelineLayout;
                }

                // Input Assembly
                var inputAssemblyState =
                    new PipelineInputAssemblyStateCreateInfo(StructureType.PipelineInputAssemblyStateCreateInfo) {
                        Topology = Options.Topology.ToVulkan()
                    };

                // Rasterization
                var rasterizationState =
                    new PipelineRasterizationStateCreateInfo(StructureType.PipelineRasterizationStateCreateInfo) {
                        PolygonMode = Options.WireFrame ? PolygonMode.Line : PolygonMode.Fill,
                        CullMode = Options.BackfaceCulling ? CullModeFlags.BackBit : CullModeFlags.None,
                        FrontFace = FrontFace.Clockwise,
                        LineWidth = Options.LineWidth
                    };

                // Blend State
                var colorAttachmentCount =
                    framebuffer!.Options.IsSwapChainTarget ? 1 : framebuffer.ColorAttachmentCount;
                var blendAttachmentStates = new List<PipelineColorBlendAttachmentState>();

                if (framebuffer.Options.IsSwapChainTarget) {
                    blendAttachmentStates.Add(
                        new() {
                            ColorWriteMask = (ColorComponentFlags)0xF,
                            BlendEnable = true,
                            SrcColorBlendFactor = BlendFactor.SrcAlpha,
                            DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
                            ColorBlendOp = BlendOp.Add,
                            AlphaBlendOp = BlendOp.Add,
                            SrcAlphaBlendFactor = BlendFactor.One,
                            DstAlphaBlendFactor = BlendFactor.Zero
                        }
                    );
                } else {
                    for (var i = 0; i < colorAttachmentCount; i++) {
                        if (!framebuffer.Options.Blend) {
                            break;
                        }

                        var state = new PipelineColorBlendAttachmentState { ColorWriteMask = (ColorComponentFlags)0xF };
                        var attachmentOptions = framebuffer.Options.Attachments.Attachments[i];
                        var blendMode = framebuffer.Options.BlendMode == FramebufferBlendMode.None
                            ? attachmentOptions.BlendMode
                            : framebuffer.Options.BlendMode;

                        state.BlendEnable = attachmentOptions.Blend;
                        state.ColorBlendOp = BlendOp.Add;
                        state.AlphaBlendOp = BlendOp.Add;
                        state.SrcAlphaBlendFactor = BlendFactor.One;
                        state.DstAlphaBlendFactor = BlendFactor.Zero;

                        switch (blendMode) {
                            case FramebufferBlendMode.SrcAlphaOneMinusSrcAlpha:
                                state.SrcColorBlendFactor = BlendFactor.SrcAlpha;
                                state.DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha;
                                state.SrcAlphaBlendFactor = BlendFactor.SrcAlpha;
                                state.DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha;
                                break;

                            case FramebufferBlendMode.OneZero:
                                state.SrcColorBlendFactor = BlendFactor.One;
                                state.DstColorBlendFactor = BlendFactor.Zero;
                                break;

                            case FramebufferBlendMode.ZeroSrcColor:
                                state.SrcColorBlendFactor = BlendFactor.Zero;
                                state.DstColorBlendFactor = BlendFactor.SrcColor;
                                break;

                            default: throw new ArgumentOutOfRangeException();
                        }

                        blendAttachmentStates.Add(state);
                    }
                }

                using var blendAttachmentStatesMemoryHandle =
                    new Memory<PipelineColorBlendAttachmentState>(blendAttachmentStates.ToArray()).Pin();
                var colorBlendState =
                    new PipelineColorBlendStateCreateInfo(StructureType.PipelineColorBlendStateCreateInfo) {
                        PAttachments = (PipelineColorBlendAttachmentState*)blendAttachmentStatesMemoryHandle.Pointer,
                        AttachmentCount = (uint)blendAttachmentStates.Count
                    };

                // Viewport
                var viewportState = new PipelineViewportStateCreateInfo(StructureType.PipelineViewportStateCreateInfo) {
                    ViewportCount = 1, ScissorCount = 1
                };

                // Dynamic states
                var enabledDynamicStates = new List<DynamicState> { DynamicState.Viewport, DynamicState.Scissor };
                if (IsDynamicLineWidth) {
                    enabledDynamicStates.Add(DynamicState.LineWidth);
                }

                using var enabledDynamicStatesMemoryHandle =
                    new Memory<DynamicState>(enabledDynamicStates.ToArray()).Pin();
                var dynamicState = new PipelineDynamicStateCreateInfo(StructureType.PipelineDynamicStateCreateInfo) {
                    PDynamicStates = (DynamicState*)enabledDynamicStatesMemoryHandle.Pointer,
                    DynamicStateCount = (uint)enabledDynamicStates.Count
                };

                // Depth Stencil
                var depthStencilState =
                    new PipelineDepthStencilStateCreateInfo(StructureType.PipelineDepthStencilStateCreateInfo) {
                        DepthTestEnable = Options.DepthTest,
                        DepthWriteEnable = Options.DepthWrite,
                        DepthCompareOp = Options.DepthOperator.ToVulkan(),
                        Back = new() {
                            FailOp = StencilOp.Keep, PassOp = StencilOp.Keep, CompareMask = (uint)CompareOp.Always
                        },
                        Front = new() {
                            FailOp = StencilOp.Keep, PassOp = StencilOp.Keep, CompareMask = (uint)CompareOp.Always
                        }
                    };

                // Multi sample
                var multisampleState =
                    new PipelineMultisampleStateCreateInfo(StructureType.PipelineMultisampleStateCreateInfo) {
                        RasterizationSamples = SampleCountFlags.Count1Bit
                    };

                // Vertex Input Descriptor
                var vertexInputBindings = new List<VertexInputBindingDescription> {
                    new() { Binding = 0, Stride = (uint)Options.Layout.Stride, InputRate = VertexInputRate.Vertex }
                };

                if (Options.InstanceLayout?.HasElements == true) {
                    vertexInputBindings.Add(
                        new() {
                            Binding = 1,
                            Stride = (uint)Options.InstanceLayout.Stride,
                            InputRate = VertexInputRate.Instance
                        }
                    );
                }

                if (Options.BoneInfluenceLayout?.HasElements == true) {
                    vertexInputBindings.Add(
                        new() {
                            Binding = 2,
                            Stride = (uint)Options.BoneInfluenceLayout.Stride,
                            InputRate = VertexInputRate.Vertex
                        }
                    );
                }

                // Input attribute bindings describe shader attribute locations and memory layouts
                var vertexInputAttributes = new List<VertexInputAttributeDescription>();

                var binding = 0;
                var location = 0;
                foreach (var layout in new[] { Options.Layout, Options.InstanceLayout, Options.BoneInfluenceLayout }) {
                    foreach (var element in layout?.Elements ?? ArraySegment<VertexBufferElement>.Empty) {
                        vertexInputAttributes.Add(
                            new() {
                                Binding = (uint)binding,
                                Location = (uint)location,
                                Format = element.Type.ToVulkan(),
                                Offset = (uint)element.Offset
                            }
                        );
                        location++;
                    }

                    binding++;
                }

                using var vertexInputBindingsMemoryHandle =
                    new Memory<VertexInputBindingDescription>(vertexInputBindings.ToArray()).Pin();

                using var vertexInputAttributesMemoryHandle =
                    new Memory<VertexInputAttributeDescription>(vertexInputAttributes.ToArray()).Pin();

                var vertexInputState =
                    new PipelineVertexInputStateCreateInfo(StructureType.PipelineVertexInputStateCreateInfo) {
                        VertexBindingDescriptionCount = (uint)vertexInputBindings.Count,
                        VertexAttributeDescriptionCount = (uint)vertexInputAttributes.Count,
                        PVertexBindingDescriptions =
                            (VertexInputBindingDescription*)vertexInputBindingsMemoryHandle.Pointer,
                        PVertexAttributeDescriptions =
                            (VertexInputAttributeDescription*)vertexInputAttributesMemoryHandle.Pointer
                    };

                var shaderStages =
                    new Memory<PipelineShaderStageCreateInfo>(shader.PipelineShaderStageCreateInfos.ToArray());
                using var shaderStagesMemoryHandle = shaderStages.Pin();

                var pipelineCreateInfo = new GraphicsPipelineCreateInfo(StructureType.GraphicsPipelineCreateInfo) {
                    Layout = VkLayout,
                    RenderPass = framebuffer.VkRenderPass,
                    StageCount = (uint)shaderStages.Length,
                    PStages = (PipelineShaderStageCreateInfo*)shaderStagesMemoryHandle.Pointer,
                    PInputAssemblyState = &inputAssemblyState,
                    PRasterizationState = &rasterizationState,
                    PViewportState = &viewportState,
                    PMultisampleState = &multisampleState,
                    PDynamicState = &dynamicState,
                    PColorBlendState = &colorBlendState,
                    PDepthStencilState = &depthStencilState,
                    PVertexInputState = &vertexInputState
                };

                // Create cache
                var pipelineCacheCreateInfo = new PipelineCacheCreateInfo(StructureType.PipelineCacheCreateInfo);
                vk.CreatePipelineCache(device, pipelineCacheCreateInfo, null, out var pipelineCache).EnsureSuccess();
                cache = pipelineCache;

                // Create rendering pipeline
                var pipelines = new[] { pipelineCreateInfo };
                var outPipeline = new Pipeline[1];
                vk.CreateGraphicsPipelines(device, cache, pipelines.AsSpan(), null, outPipeline).EnsureSuccess();
                VkPipeline = outPipeline[0];
            }
        );
    }
}
