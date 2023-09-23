using Rin.Platform.Silk;

namespace Rin.Platform.Internal; 

static class ObjectFactory {
    public static IInternalWindow CreateWindow() {
        return new SilkWindow();
    }
}
