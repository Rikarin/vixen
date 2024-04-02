namespace Vixen.Editor.VixenCompiler.Ast;

public class MaterialPropertyDeclaration : Declaration {
    public Name Identifier { get; }
    public Name EditorName { get; }
    public TypeDeclaration Type { get; }
    public Expression Value { get; }

    public MaterialPropertyDeclaration(
        Location location,
        Name identifier,
        Name editorName,
        TypeDeclaration type,
        Expression value
    ) : base(location) {
        Identifier = identifier;
        EditorName = editorName;
        Type = type;
        Value = value;
    }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}
