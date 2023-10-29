using Rin.Core.Shaders.Ast;

namespace Rin.Core.Shaders; 

public class VisitorBase {
    protected virtual Node Visit(Node node) {
        return node;
    }
    
    protected virtual Node VisitDynamic(Node node) {
        return Visit((dynamic)node);
    }
}
