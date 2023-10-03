using Rin.Core.Abstractions;
using Rin.Platform.Silk;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System.Runtime.InteropServices;

namespace Rin.Platform.Vulkan;

sealed class VulkanContext : RendererContext, IDisposable {
    readonly string[] validationLayers = { "VK_LAYER_KHRONOS_validation" };
    readonly ILogger logger;

    ExtDebugUtils? debugUtils;
    DebugUtilsMessengerEXT debugMessenger;
    PipelineCache pipelineCache;


    VulkanPhysicalDevice physicalDevice;

    public static Vk Vulkan { get; private set; }
    public static VulkanDevice CurrentDevice { get; private set; }

    bool EnableValidationLayers { get; } = true;

    public VulkanContext() {
        logger = Log.ForContext<VulkanContext>();

        CreateInstance();
        SetupDebugMessenger();
    }

    public unsafe void CreateInstance() {
        var appInfo = new ApplicationInfo {
            SType = StructureType.ApplicationInfo,
            // TODO: fix these
            PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("Hello Triangle"),
            ApplicationVersion = Vk.MakeVersion(1, 0),
            PEngineName = (byte*)Marshal.StringToHGlobalAnsi("Rin"),
            EngineVersion = Vk.MakeVersion(1, 0),
            ApiVersion = Vk.MakeVersion(1, 2)
        };

        var extensions = GetRequiredExtensions();
        var isMacOs = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        if (isMacOs) {
            extensions = extensions.Append("VK_KHR_portability_enumeration").ToArray();
        }

        Log.Information("Vulkan Extensions: {Extensions}", extensions);
        InstanceCreateInfo createInfo = new() {
            SType = StructureType.InstanceCreateInfo,
            Flags = isMacOs ? InstanceCreateFlags.EnumeratePortabilityBitKhr : InstanceCreateFlags.None,
            PApplicationInfo = &appInfo,
            EnabledExtensionCount = (uint)extensions.Length,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions)
        };

        Vulkan = Vk.GetApi();
        if (EnableValidationLayers) {
            if (CheckValidationLayerSupport()) {
                createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);
                createInfo.EnabledLayerCount = (uint)validationLayers.Length;

                var debugCreateInfo = new DebugUtilsMessengerCreateInfoEXT();
                PopulateDebugMessengerCreateInfo(ref debugCreateInfo);
                createInfo.PNext = &debugCreateInfo;
            } else {
                Log.Error("Validation layer not present but required! Validation is disabled");
            }
        }

        if (Vulkan.CreateInstance(createInfo, null, out var instance) != Result.Success) {
            throw new("Failed to create Vulkan instance");
        }

        Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
        Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);
        SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);
        SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);

        // Physical device initialization
        physicalDevice = VulkanPhysicalDevice.Select();

        var enabledFeatures = new PhysicalDeviceFeatures {
            SamplerAnisotropy = true,
            // WideLines = true,
            FillModeNonSolid = true,
            IndependentBlend = true
            // PipelineStatisticsQuery = true
        };

        CurrentDevice = new(physicalDevice, enabledFeatures);
        VulkanAllocator.Init();

        // TODO: this should be moved to pipeline
        var pipelineCacheCreateInfo = new PipelineCacheCreateInfo { SType = StructureType.PipelineCacheCreateInfo };
        Vulkan.CreatePipelineCache(CurrentDevice.VkLogicalDevice, pipelineCacheCreateInfo, null, out pipelineCache);
    }

    public unsafe void Dispose() {
        VulkanAllocator.Shutdown();

        var instance = Vulkan.CurrentInstance!.Value;
        if (EnableValidationLayers) {
            debugUtils?.DestroyDebugUtilsMessenger(instance, debugMessenger, null);
        }

        Vulkan.DestroyInstance(instance, null);
        Vulkan.Dispose();
    }

    unsafe string[] GetRequiredExtensions() {
        var window = SilkWindow.MainWindow.silkWindow;
        var glfwExtensions = window.VkSurface!.GetRequiredExtensions(out var glfwExtensionCount);
        var extensions = SilkMarshal.PtrToStringArray((nint)glfwExtensions, (int)glfwExtensionCount);

        if (EnableValidationLayers) {
            return extensions
                .Append(ExtDebugReport.ExtensionName)
                .Append(KhrGetPhysicalDeviceProperties2.ExtensionName)
                .Append(ExtDebugUtils.ExtensionName)
                .ToArray();
        }

        return extensions;
    }

    unsafe bool CheckValidationLayerSupport() {
        uint layerCount = 0;
        Vulkan.EnumerateInstanceLayerProperties(ref layerCount, null);
        var availableLayers = new LayerProperties[layerCount];

        fixed (LayerProperties* availableLayersPtr = availableLayers) {
            Vulkan.EnumerateInstanceLayerProperties(ref layerCount, availableLayersPtr);
        }

        var availableLayerNames = availableLayers
            .Select(layer => Marshal.PtrToStringAnsi((IntPtr)layer.LayerName))
            .ToHashSet();

        return validationLayers.All(availableLayerNames.Contains);
    }

    unsafe void PopulateDebugMessengerCreateInfo(ref DebugUtilsMessengerCreateInfoEXT createInfo) {
        createInfo.SType = StructureType.DebugUtilsMessengerCreateInfoExt;
        createInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt
            | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt
            | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt;
        createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt
            | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt
            | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt;
        createInfo.PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT)DebugCallback;
    }

    unsafe uint DebugCallback(
        DebugUtilsMessageSeverityFlagsEXT messageSeverity,
        DebugUtilsMessageTypeFlagsEXT messageTypes,
        DebugUtilsMessengerCallbackDataEXT* pCallbackData,
        void* pUserData
    ) {
        logger.Information("{Message}", Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));
        return Vk.False;
    }

    unsafe void SetupDebugMessenger() {
        if (!EnableValidationLayers) {
            return;
        }

        var instance = Vulkan.CurrentInstance!.Value;
        if (!Vulkan.TryGetInstanceExtension(instance, out debugUtils)) {
            return;
        }

        var createInfo = new DebugUtilsMessengerCreateInfoEXT();
        PopulateDebugMessengerCreateInfo(ref createInfo);

        if (debugUtils!.CreateDebugUtilsMessenger(instance, in createInfo, null, out debugMessenger)
            != Result.Success) {
            throw new("failed to set up debug messenger!");
        }
    }
}
