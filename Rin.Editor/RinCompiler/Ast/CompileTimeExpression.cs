namespace Rin.Editor.RinCompiler.Ast;

public abstract class CompileTimeExpression : Expression {
    protected CompileTimeExpression(Location location) : base(location) { }
}
