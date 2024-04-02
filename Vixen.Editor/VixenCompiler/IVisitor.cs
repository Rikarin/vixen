using Vixen.Editor.VixenCompiler.Ast;

namespace Vixen.Editor.VixenCompiler;

public interface IVisitor {
    void Visit(ShaderDeclaration declaration);
    void Visit(MaterialPropertyDeclaration declaration);
    void Visit(PropertiesDeclaration declaration);
    void Visit(RangeTypeDeclaration declaration);
    void Visit(SubShaderDeclaration declaration);
    void Visit(SubShaderPassDeclaration declaration);
    void Visit(TypeDeclaration declaration);

    void Visit(CharacterLiteral literal);
    void Visit(IntegerLiteral literal);
    void Visit(StringLiteral literal);
}
