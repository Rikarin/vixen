namespace Vixen.Editor.VixenCompiler.Ast;

public abstract class Expression : Node {
    protected Expression(Location location) : base(location) { }
}
