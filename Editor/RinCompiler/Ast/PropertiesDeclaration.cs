namespace Rin.Editor.RinCompiler.Ast;

public class PropertiesDeclaration : Declaration {
    readonly List<MaterialPropertyDeclaration> declarations;

    public IReadOnlyList<MaterialPropertyDeclaration> MaterialPropertyDeclarations => declarations.AsReadOnly();

    public PropertiesDeclaration(Location location, List<MaterialPropertyDeclaration> declarations) : base(location) {
        this.declarations = declarations;
    }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}
