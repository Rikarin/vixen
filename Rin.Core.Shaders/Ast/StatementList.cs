using System.Collections;

namespace Rin.Core.Shaders.Ast;

public record StatementList : Statement, IList<Statement> {
    public List<Statement> Statements { get; } = new();
    
    public IEnumerator<Statement> GetEnumerator() => Statements.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Add(Statement item) => Statements.Add(item);
    public void Clear() => Statements.Clear();
    public bool Contains(Statement item) => Statements.Contains(item);
    public void CopyTo(Statement[] array, int arrayIndex) => Statements.CopyTo(array, arrayIndex);
    public bool Remove(Statement item) => Statements.Remove(item);
    public int Count => Statements.Count;
    public bool IsReadOnly => false; // TODO
    public int IndexOf(Statement item) => Statements.IndexOf(item);
    public void Insert(int index, Statement item) => Statements.Insert(index, item);
    public void RemoveAt(int index) => Statements.RemoveAt(index);

    public Statement this[int index] {
        get => Statements[index];
        set => Statements[index] = value;
    }
}
