namespace Vixen.Core.Yaml.Serialization.Serializers;

/// <summary>
///     An implementation of <see cref="IYamlSerializable" /> that will call the <see cref="ReadYaml" /> and
///     <see cref="WriteYaml" /> methods
///     of another serializer when invoked.
/// </summary>
public abstract class ChainedSerializer : IYamlSerializable {
    public ChainedSerializer Prev { get; private set; }
    public ChainedSerializer Next { get; private set; }
    public ChainedSerializer First => FindBoundary(x => x.Prev);
    public ChainedSerializer Last => FindBoundary(x => x.Next);

    public T? FindPrevious<T>() where T : ChainedSerializer => FindByType<T>(x => x.Prev);

    public T? FindNext<T>() where T : ChainedSerializer => FindByType<T>(x => x.Next);

    /// <summary>
    ///     Prepends the given <see cref="ChainedSerializer" /> to this serializer.
    /// </summary>
    /// <param name="previousSerializer">The serializer to prepend.</param>
    public void Prepend(ChainedSerializer? previousSerializer) {
        // Update current Prev if non-null to target the first of the chain we're prepending
        Prev?.SetNext(previousSerializer?.First);
        previousSerializer?.First.SetPrev(Prev);
        // Set the current Prev to the given serializer
        Prev = previousSerializer;
        // Make sure that the link with the old Next of the given serializer is cleared
        previousSerializer?.Next?.SetPrev(null);
        // And set the Next of the given serializer to be this one.
        previousSerializer?.SetNext(this);
    }

    /// <summary>
    ///     Appends the given <see cref="ChainedSerializer" /> to this serializer.
    /// </summary>
    /// <param name="nextSerializer">The serializer to append.</param>
    public void Append(ChainedSerializer? nextSerializer) {
        // Update current Next if non-null to target the last of the chain we're appending
        Next?.SetPrev(nextSerializer?.Last);
        nextSerializer?.Last.SetNext(Next);
        // Set the current Next to the given serializer
        Next = nextSerializer;
        // Make sure that the link with the old Prev of the given serializer is cleared
        nextSerializer?.Prev?.SetNext(null);
        // And set the Prev of the given serializer to be this one.
        nextSerializer?.SetPrev(this);
    }

    /// <inheritdoc />
    public virtual object ReadYaml(ref ObjectContext objectContext) {
        if (Next == null) {
            throw new InvalidOperationException("The last chained serializer is invoking non-existing next serializer");
        }

        return Next.ReadYaml(ref objectContext);
    }

    /// <inheritdoc />
    public virtual void WriteYaml(ref ObjectContext objectContext) {
        if (Next == null) {
            throw new InvalidOperationException("The last chained serializer is invoking non-existing next serializer");
        }

        Next.WriteYaml(ref objectContext);
    }

    ChainedSerializer FindBoundary(Func<ChainedSerializer, ChainedSerializer> navigate) {
        var current = this;
        while (navigate(current) != null) {
            current = navigate(current);
        }

        return current;
    }

    T? FindByType<T>(Func<ChainedSerializer, ChainedSerializer> navigate) where T : ChainedSerializer {
        var current = navigate(this);
        while (current != null) {
            if (current is T found) {
                return found;
            }

            current = navigate(current);
        }

        return null;
    }

    void SetPrev(ChainedSerializer prev) => Prev = prev;
    void SetNext(ChainedSerializer next) => Next = next;
}
