using System.Collections;

namespace Rin.Core.Shaders.Ast;

public record ParameterList : Node, IList<Parameter> {
    public List<Parameter> Parameters { get; } = new();
    
    public IEnumerator<Parameter> GetEnumerator() => Parameters.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Add(Parameter item) => Parameters.Add(item);
    public void Clear() => Parameters.Clear();
    public bool Contains(Parameter item) => Parameters.Contains(item);
    public void CopyTo(Parameter[] array, int arrayIndex) => Parameters.CopyTo(array, arrayIndex);
    public bool Remove(Parameter item) => Parameters.Remove(item);
    public int Count => Parameters.Count;
    public bool IsReadOnly => false; // TODO
    public int IndexOf(Parameter item) => Parameters.IndexOf(item);
    public void Insert(int index, Parameter item) => Parameters.Insert(index, item);
    public void RemoveAt(int index) => Parameters.RemoveAt(index);

    public Parameter this[int index] {
        get => Parameters[index];
        set => Parameters[index] = value;
    }
}
