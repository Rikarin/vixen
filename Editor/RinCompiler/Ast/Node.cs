namespace Rin.Editor.RinCompiler.Ast; 

public abstract class Node {
    public Location Location { get; }

    public Node(Location location) {
        Location = location;
    }
    
    public abstract void Accept(IVisitor visitor);
}