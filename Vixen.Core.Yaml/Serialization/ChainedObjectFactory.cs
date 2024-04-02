namespace Vixen.Core.Yaml.Serialization;

/// <summary>
///     An <see cref="IObjectFactory" /> that can be chained with another object factory;
/// </summary>
public class ChainedObjectFactory : IObjectFactory {
    readonly IObjectFactory nextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChainedObjectFactory" /> class.
    /// </summary>
    /// <param name="nextFactory">The next factory.</param>
    public ChainedObjectFactory(IObjectFactory nextFactory) {
        this.nextFactory = nextFactory;
    }

    public virtual object Create(Type type) => nextFactory != null ? nextFactory.Create(type) : null;
}
