namespace Vixen.Editor.VixenCompiler.Ast;

public abstract class CompileTimeExpression : Expression {
    protected CompileTimeExpression(Location location) : base(location) { }
}
