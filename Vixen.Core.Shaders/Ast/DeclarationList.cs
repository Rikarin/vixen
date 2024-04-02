using System.Collections;

namespace Vixen.Core.Shaders.Ast;

public record DeclarationList : Node, IList<Node> {
    public List<Node> Declarations { get; } = new();
    
    public IEnumerator<Node> GetEnumerator() => Declarations.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Add(Node item) => Declarations.Add(item);
    public void Clear() => Declarations.Clear();
    public bool Contains(Node item) => Declarations.Contains(item);
    public void CopyTo(Node[] array, int arrayIndex) => Declarations.CopyTo(array, arrayIndex);
    public bool Remove(Node item) => Declarations.Remove(item);
    public int Count => Declarations.Count;
    public bool IsReadOnly => false; // TODO
    public int IndexOf(Node item) => Declarations.IndexOf(item);
    public void Insert(int index, Node item) => Declarations.Insert(index, item);
    public void RemoveAt(int index) => Declarations.RemoveAt(index);

    public Node this[int index] {
        get => Declarations[index];
        set => Declarations[index] = value;
    }
}