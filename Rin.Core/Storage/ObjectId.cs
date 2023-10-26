namespace Rin.Core.Storage;

// TODO: finish this
public readonly record struct ObjectId(int Id) {
    public static readonly ObjectId Empty = new(0);
}
