using Vixen.Core.Shaders.Ast;

namespace Vixen.Core.Shaders; 

public class VisitorBase {
    protected virtual Node Visit(Node node) {
        return node;
    }
    
    protected virtual Node VisitDynamic(Node node) {
        return Visit((dynamic)node);
    }
}
