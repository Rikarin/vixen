namespace Vixen.Editor.VixenCompiler.Ast;

public sealed class SubShaderDeclaration : Declaration {
    public Dictionary<string, string> Tags { get; }
    public int? Lod { get; }
    public SubShaderPassDeclaration Pass { get; }

    public SubShaderDeclaration(
        Location location,
        Dictionary<string, string> tags,
        int? lod,
        SubShaderPassDeclaration pass
    ) : base(location) {
        Tags = tags;
        Lod = lod;
        Pass = pass;
    }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}
