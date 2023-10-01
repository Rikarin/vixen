namespace Rin.Editor.RinCompiler.Ast; 

public abstract class Expression : Node {
    protected Expression(Location location) : base(location) { }
}