namespace Vixen.Editor.VixenCompiler.Ast;

public sealed class SubShaderPassDeclaration : Declaration {
    public Name ShaderProgram { get; }

    public SubShaderPassDeclaration(Location location, Name shaderProgram) : base(location) {
        ShaderProgram = shaderProgram;
    }

    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}
