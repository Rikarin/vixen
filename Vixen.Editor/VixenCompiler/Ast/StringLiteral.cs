namespace Vixen.Editor.VixenCompiler.Ast;

public sealed class StringLiteral : CompileTimeExpression {
    public string Value { get; }
    public BuiltinType Type { get; }

    public StringLiteral(Location location, string value, BuiltinType type) : base(location) {
        Value = value;
        Type = type;
    }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}
