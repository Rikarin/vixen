namespace Vixen.Core;

/// <summary>
///     Base class for a <see cref="IReferencable" /> class.
/// </summary>
public abstract class ReferenceBase : IReferencable {
    int counter = 1;

    /// <inheritdoc />
    public int ReferenceCount => counter;

    /// <inheritdoc />
    public virtual int AddReference() {
        var newCounter = Interlocked.Increment(ref counter);
        if (newCounter <= 1) {
            // throw new InvalidOperationException(FrameworkResources.AddReferenceError);
            // TODO
            throw new InvalidOperationException();
        }

        return newCounter;
    }

    /// <inheritdoc />
    public virtual int Release() {
        var newCounter = Interlocked.Decrement(ref counter);
        if (newCounter == 0) {
            try {
                Destroy();
            } finally {
                // Reverse back the counter if there are any exceptions in the destroy method
                Interlocked.Exchange(ref counter, 1);
            }
        } else if (newCounter < 0) {
            // throw new InvalidOperationException(FrameworkResources.ReleaseReferenceError);
            // TODO
            throw new InvalidOperationException();
        }

        return newCounter;
    }

    /// <summary>
    ///     Releases unmanaged and - optionally - managed resources
    /// </summary>
    protected abstract void Destroy();
}
