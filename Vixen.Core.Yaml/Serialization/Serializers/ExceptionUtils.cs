using System.Reflection;

namespace Vixen.Core.Yaml.Serialization.Serializers;

static class ExceptionUtils {
    /// <summary>
    ///     Unwraps some exception such as <see cref="TargetInvocationException" />.
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static Exception Unwrap(this Exception exception) {
        if (exception is TargetInvocationException targetInvocationException) {
            return targetInvocationException.InnerException;
        }

        return exception;
    }
}
