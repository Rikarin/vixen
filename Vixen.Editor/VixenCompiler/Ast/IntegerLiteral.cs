namespace Vixen.Editor.VixenCompiler.Ast;

public sealed class IntegerLiteral : CompileTimeExpression {
    public ulong Value { get; }
    public BuiltinType Type { get; }

    public IntegerLiteral(Location location, ulong value, BuiltinType type) : base(location) {
        Value = value;
        Type = type;
    }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}
