using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Rin.Platform.Vulkan;

sealed class VulkanPhysicalDevice {
    ILogger log = Log.ForContext<VulkanPhysicalDevice>();
    internal readonly List<DeviceQueueCreateInfo> queueCreateInfos = new();

    // This reference needs to be retained so GC will not free up the memory
    static readonly GlobalMemory defaultQueuePriority;

    readonly PhysicalDeviceFeatures features; // TODO: not used, yet?
    readonly List<QueueFamilyProperties> queueFamilyProperties = new();
    readonly List<string> supportedExtensions = new();

    public PhysicalDevice VkPhysicalDevice { get; }
    public Format DepthFormat { get; private set; }
    public QueueFamilyIndices QueueFamilyIndices { get; }
    public PhysicalDeviceProperties Properties { get; }
    public PhysicalDeviceLimits Limits => Properties.Limits;
    public PhysicalDeviceMemoryProperties MemoryProperties { get; }

    public IReadOnlyList<QueueFamilyProperties> QueueFamilyProperties => queueFamilyProperties.AsReadOnly();

    static VulkanPhysicalDevice() {
        defaultQueuePriority = GlobalMemory.Allocate(sizeof(float));
    }

    public VulkanPhysicalDevice() {
        var vk = VulkanContext.Vulkan;
        var devices = vk.GetPhysicalDevices(vk.CurrentInstance!.Value);
        if (!devices.Any()) {
            throw new NotSupportedException("Failed to find GPUs with Vulkan support.");
        }

        VkPhysicalDevice = devices.FirstOrDefault(
            x => vk.GetPhysicalDeviceProperties(x).DeviceType == PhysicalDeviceType.DiscreteGpu
        );

        if (VkPhysicalDevice.Handle == 0) {
            log.Warning("No device with discrete GPU");

            // We call it a day and set it as is for now but this has more verification check in place
            // https://github.com/dotnet/Silk.NET/blob/main/src/Lab/Experiments/ImGuiVulkan/ImGuiVulkanApplication.cs#L473
            VkPhysicalDevice = devices.FirstOrDefault();
        }

        features = vk.GetPhysicalDeviceFeature(VkPhysicalDevice);
        MemoryProperties = vk.GetPhysicalDeviceMemoryProperties(VkPhysicalDevice);

        LoadQueueFamilyProperties();
        LoadExtensions();

        var requestedQueueTypes = QueueFlags.GraphicsBit | QueueFlags.ComputeBit | QueueFlags.TransferBit;
        QueueFamilyIndices = GetQueueFamilyIndices(requestedQueueTypes);
        LoadCreateInfos(requestedQueueTypes);
        DepthFormat = FindDepthFormat();
    }

    public bool IsExtensionSupported(string extensionName) => supportedExtensions.Contains(extensionName);

    public uint GetMemoryTypeIndex(byte typeBits, MemoryPropertyFlags properties) {
        for (var i = 0; i < MemoryProperties.MemoryTypeCount; i++) {
            if ((typeBits & 1) == 1) {
                if (MemoryProperties.MemoryTypes[i].PropertyFlags.HasFlag(properties)) {
                    return (uint)i;
                }
            }

            typeBits >>= 1;
        }

        log.Fatal("Could not find a suitable memory type");
        return uint.MaxValue;
    }

    public static VulkanPhysicalDevice Select() => new();

    unsafe void LoadQueueFamilyProperties() {
        var vk = VulkanContext.Vulkan;
        uint count = 0;
        vk.GetPhysicalDeviceQueueFamilyProperties(VkPhysicalDevice, ref count, null);

        using var handle = VulkanUtils.Alloc<QueueFamilyProperties>(count, out var queueFamilies);
        vk.GetPhysicalDeviceQueueFamilyProperties(VkPhysicalDevice, ref count, queueFamilies);

        for (var i = 0; i < count; i++) {
            queueFamilyProperties.Add(queueFamilies[i]);
        }
    }

    unsafe void LoadExtensions() {
        var vk = VulkanContext.Vulkan;
        uint count = 0;

        vk.EnumerateDeviceExtensionProperties(VkPhysicalDevice, (byte*)null, ref count, null);
        if (count > 0) {
            using var handle = VulkanUtils.Alloc<ExtensionProperties>(count, out var extensionProperties);
            if (
                vk.EnumerateDeviceExtensionProperties(VkPhysicalDevice, (byte*)null, ref count, extensionProperties)
                == Result.Success
            ) {
                log.Debug("Selected renderer has {Count} properties", count);
                for (var i = 0; i < count; i++) {
                    var name = Marshal.PtrToStringAnsi((IntPtr)extensionProperties[i].ExtensionName)!;
                    supportedExtensions.Add(name);
                    // Log.Information("{PropertyName}", name);
                }
            }
        }
    }

    QueueFamilyIndices GetQueueFamilyIndices(QueueFlags flags) {
        var indices = new QueueFamilyIndices();

        // Dedicated queue for compute
        if (flags.HasFlag(QueueFlags.ComputeBit)) {
            for (var i = 0; i < queueFamilyProperties.Count; i++) {
                var pFlags = queueFamilyProperties[i].QueueFlags;
                if (pFlags.HasFlag(QueueFlags.ComputeBit) && !pFlags.HasFlag(QueueFlags.GraphicsBit)) {
                    indices.Compute = (uint)i;
                    break;
                }
            }
        }

        // Dedicated queue for transfer
        if (flags.HasFlag(QueueFlags.TransferBit)) {
            for (var i = 0; i < queueFamilyProperties.Count; i++) {
                var pFlags = queueFamilyProperties[i].QueueFlags;
                if (
                    pFlags.HasFlag(QueueFlags.TransferBit)
                    && !pFlags.HasFlag(QueueFlags.GraphicsBit)
                    && !pFlags.HasFlag(QueueFlags.ComputeBit)
                ) {
                    indices.Transfer = (uint)i;
                    break;
                }
            }
        }

        // Fill rest
        for (var i = 0; i < queueFamilyProperties.Count; i++) {
            var pFlags = queueFamilyProperties[i].QueueFlags;

            if (flags.HasFlag(QueueFlags.TransferBit) && !indices.Transfer.HasValue) {
                if (pFlags.HasFlag(QueueFlags.TransferBit)) {
                    indices.Transfer = (uint)i;
                }
            }

            if (flags.HasFlag(QueueFlags.ComputeBit) && !indices.Compute.HasValue) {
                if (pFlags.HasFlag(QueueFlags.ComputeBit)) {
                    indices.Compute = (uint)i;
                }
            }

            if (flags.HasFlag(QueueFlags.GraphicsBit)) {
                if (pFlags.HasFlag(QueueFlags.GraphicsBit)) {
                    indices.Graphics = (uint)i;
                }
            }
        }

        return indices;
    }

    unsafe void LoadCreateInfos(QueueFlags flags) {
        var queuePriority = (float*)Unsafe.AsPointer(ref defaultQueuePriority.GetPinnableReference());
        *queuePriority = 0f;

        if (flags.HasFlag(QueueFlags.GraphicsBit) && QueueFamilyIndices.Graphics.HasValue) {
            var queueInfo = new DeviceQueueCreateInfo {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = QueueFamilyIndices.Graphics.Value,
                QueueCount = 1,
                PQueuePriorities = queuePriority
            };

            queueCreateInfos.Add(queueInfo);
        }

        if (flags.HasFlag(QueueFlags.ComputeBit) && QueueFamilyIndices.Compute.HasValue) {
            if (QueueFamilyIndices.Compute != QueueFamilyIndices.Graphics) {
                var queueInfo = new DeviceQueueCreateInfo {
                    SType = StructureType.DeviceQueueCreateInfo,
                    QueueFamilyIndex = QueueFamilyIndices.Compute.Value,
                    QueueCount = 1,
                    PQueuePriorities = queuePriority
                };

                queueCreateInfos.Add(queueInfo);
            }
        }

        if (flags.HasFlag(QueueFlags.TransferBit) && QueueFamilyIndices.Transfer.HasValue) {
            if (
                QueueFamilyIndices.Transfer != QueueFamilyIndices.Graphics
                && QueueFamilyIndices.Transfer != QueueFamilyIndices.Compute
            ) {
                var queueInfo = new DeviceQueueCreateInfo {
                    SType = StructureType.DeviceQueueCreateInfo,
                    QueueFamilyIndex = QueueFamilyIndices.Transfer.Value,
                    QueueCount = 1,
                    PQueuePriorities = queuePriority
                };

                queueCreateInfos.Add(queueInfo);
            }
        }
    }

    Format FindDepthFormat() {
        var vk = VulkanContext.Vulkan;
        var depthFormats = new[] {
            Format.D32SfloatS8Uint, Format.D32Sfloat, Format.D24UnormS8Uint, Format.D16UnormS8Uint, Format.D16Unorm
        };

        foreach (var format in depthFormats) {
            var formatProps = vk.GetPhysicalDeviceFormatProperties(VkPhysicalDevice, format);
            if (formatProps.OptimalTilingFeatures.HasFlag(FormatFeatureFlags.DepthStencilAttachmentBit)) {
                return format;
            }
        }

        return Format.Undefined;
    }
}

struct QueueFamilyIndices {
    public uint? Graphics { get; set; }
    public uint? Compute { get; set; }
    public uint? Transfer { get; set; }
}
