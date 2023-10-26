using System.Reflection;
using System.Runtime.CompilerServices;

namespace Rin.Core.Reflection;

public static class ModuleRuntimeHelpers {
    public static void RunModuleConstructor(Module module) {
        // On some platforms such as Android, ModuleHandle is not set
        if (module.ModuleHandle == ModuleHandle.EmptyHandle) {
            // Instead, initialize any type
            RuntimeHelpers.RunClassConstructor(module.Assembly.DefinedTypes.First().TypeHandle);
        } else {
            RuntimeHelpers.RunModuleConstructor(module.ModuleHandle);
        }
    }
}
