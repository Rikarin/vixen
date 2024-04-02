namespace Vixen.Editor.VixenCompiler.Ast;

public sealed class CharacterLiteral : CompileTimeExpression {
    public char Value { get; }
    public BuiltinType Type { get; }

    public CharacterLiteral(Location location, char value, BuiltinType type) : base(location) {
        Value = value;
        Type = type;
    }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}
