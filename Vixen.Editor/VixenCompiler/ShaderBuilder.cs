using Vixen.Editor.VixenCompiler.Ast;

namespace Vixen.Editor.VixenCompiler;

public class ShaderBuilder : IVisitor {
    public string Name { get; private set; }
    public string ProgramSource { get; private set; }

    public void Visit(ShaderDeclaration declaration) {
        Name = declaration.Name.Value;

        foreach (var x in declaration.Declarations) {
            x.Accept(this);
        }
    }

    public void Visit(MaterialPropertyDeclaration declaration) { }

    public void Visit(PropertiesDeclaration declaration) { }

    public void Visit(RangeTypeDeclaration declaration) { }

    public void Visit(SubShaderDeclaration declaration) {
        declaration.Pass.Accept(this);
    }

    public void Visit(SubShaderPassDeclaration declaration) {
        ProgramSource = declaration.ShaderProgram.Value;
        // Log.Information("Debug: {Variable}", declaration.ShaderProgram.Value);
    }

    public void Visit(TypeDeclaration declaration) { }

    public void Visit(CharacterLiteral literal) { }

    public void Visit(IntegerLiteral literal) { }

    public void Visit(StringLiteral literal) { }
}
