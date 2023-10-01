namespace Rin.Editor.RinCompiler.Ast;

public class RangeTypeDeclaration : TypeDeclaration {
    public float From { get; }
    public float To { get; }
    
    public RangeTypeDeclaration(Location location, float from, float to) : base(location, TokenType.Range) {
        From = from;
        To = to;
    }
    
    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}
