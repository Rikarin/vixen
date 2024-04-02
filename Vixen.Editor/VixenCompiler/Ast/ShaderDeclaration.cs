namespace Vixen.Editor.VixenCompiler.Ast;

public sealed class ShaderDeclaration : Declaration {
    readonly List<Declaration> declarations;

    public Name Name { get; }
    public IReadOnlyList<Declaration> Declarations => declarations.AsReadOnly();

    public ShaderDeclaration(Location location, Name name, List<Declaration> declarations) : base(location) {
        Name = name;
        this.declarations = declarations;
    }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}
