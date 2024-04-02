using Antlr4.Runtime.Tree;
using Vixen.Core.Shaders.Ast;

namespace Vixen.Core.Shaders; 

public partial class BuildAstVisitor {
    // TODO: finish these
    
    public override Node VisitType_(VixenParser.Type_Context context) {
        return base.VisitType_(context);
    }

    public override Node VisitBase_type(VixenParser.Base_typeContext context) {
        return Visit(context.GetChild(0));
    }

    public override Node VisitTuple_type(VixenParser.Tuple_typeContext context) {
        return base.VisitTuple_type(context);
    }

    public override Node VisitTuple_element(VixenParser.Tuple_elementContext context) {
        return base.VisitTuple_element(context);
    }

    public override Node VisitSimple_type(VixenParser.Simple_typeContext context) {
        return context.BOOL() != null ? ScalarType.Bool : Visit(context.numeric_type());
    }

    public override Node VisitNumeric_type(VixenParser.Numeric_typeContext context) {
        return Visit(context.GetChild(0));
    }

    public override Node VisitIntegral_type(VixenParser.Integral_typeContext context) {
        if (context.GetChild(0) is not ITerminalNode terminalNode) {
            throw new("fatal");
        }

        return terminalNode.Symbol.Type switch {
            VixenParser.BYTE => ScalarType.Byte,
            VixenParser.CHAR => ScalarType.Char,
            VixenParser.SHORT => ScalarType.Short,
            VixenParser.USHORT => ScalarType.UShort,
            VixenParser.INT => ScalarType.Int,
            VixenParser.UINT => ScalarType.UInt,
            VixenParser.LONG => ScalarType.Long,
            VixenParser.ULONG => ScalarType.ULong,
            
            _ => throw new("fatal")
        };
    }

    public override Node VisitFloating_point_type(VixenParser.Floating_point_typeContext context) {
        if (context.FLOAT() != null) {
            return ScalarType.Float;
        }

        if (context.DOUBLE() != null) {
            return ScalarType.Double;
        }

        throw new("fatal");
    }

    public override Node VisitClass_type(VixenParser.Class_typeContext context) {
        return base.VisitClass_type(context);
    }

    public override Node VisitType_argument_list(VixenParser.Type_argument_listContext context) {
        return base.VisitType_argument_list(context);
    }
}
