using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Vixen.Core;

/// <summary>
///     Platform specific queries and functions.
/// </summary>
public static class Platform {
#if PLATFORM_ANDROID
        /// <summary>
        /// The current running <see cref="PlatformType"/>.
        /// </summary>
        public static readonly PlatformType Type = PlatformType.Android;
#elif PLATFORM_IOS
        /// <summary>
        /// The current running <see cref="PlatformType"/>.
        /// </summary>
        public static readonly PlatformType Type = PlatformType.iOS;
#else
    /// <summary>
    ///     The current running <see cref="PlatformType" />.
    /// </summary>
    public static readonly PlatformType Type
        = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? PlatformType.Windows
        : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? PlatformType.Linux
        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? PlatformType.MacOS
        : PlatformType.Windows; // For now we use Windows as fallback, but it might be better to throw an exception?
#endif

    /// <summary>
    ///     Gets a value indicating whether the running platform is windows desktop.
    /// </summary>
    /// <value><c>true</c> if this instance is windows desktop; otherwise, <c>false</c>.</value>
    public static readonly bool IsWindowsDesktop = Type == PlatformType.Windows;

    /// <summary>
    ///     Gets a value indicating whether the running assembly is a debug assembly.
    /// </summary>
    public static readonly bool IsRunningDebugAssembly = GetIsRunningDebugAssembly();

    /// <summary>
    ///     Check if running assembly has the DebuggableAttribute set with the `DisableOptimizations` mode enabled.
    ///     This function is called only once.
    /// </summary>
    static bool GetIsRunningDebugAssembly() {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null) {
            var debuggableAttribute = entryAssembly.GetCustomAttributes<DebuggableAttribute>().FirstOrDefault();
            if (debuggableAttribute != null) {
                return (debuggableAttribute.DebuggingFlags & DebuggableAttribute.DebuggingModes.DisableOptimizations)
                    != 0;
            }
        }

        return false;
    }
}
