namespace Rin.Editor.RinCompiler.Ast;

public class TypeDeclaration : Declaration {
    public TokenType Type { get; }

    public TypeDeclaration(Location location, TokenType type) : base(location) {
        Type = type;
    }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}
