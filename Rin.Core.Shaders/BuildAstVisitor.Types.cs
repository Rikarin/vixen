using Antlr4.Runtime.Tree;
using Rin.Core.Shaders.Ast;

namespace Rin.Core.Shaders; 

public partial class BuildAstVisitor {
    // TODO: finish these
    
    public override Node VisitType_(RinParser.Type_Context context) {
        return base.VisitType_(context);
    }

    public override Node VisitBase_type(RinParser.Base_typeContext context) {
        return Visit(context.GetChild(0));
    }

    public override Node VisitTuple_type(RinParser.Tuple_typeContext context) {
        return base.VisitTuple_type(context);
    }

    public override Node VisitTuple_element(RinParser.Tuple_elementContext context) {
        return base.VisitTuple_element(context);
    }

    public override Node VisitSimple_type(RinParser.Simple_typeContext context) {
        return context.BOOL() != null ? ScalarType.Bool : Visit(context.numeric_type());
    }

    public override Node VisitNumeric_type(RinParser.Numeric_typeContext context) {
        return Visit(context.GetChild(0));
    }

    public override Node VisitIntegral_type(RinParser.Integral_typeContext context) {
        if (context.GetChild(0) is not ITerminalNode terminalNode) {
            throw new("fatal");
        }

        return terminalNode.Symbol.Type switch {
            RinParser.BYTE => ScalarType.Byte,
            RinParser.CHAR => ScalarType.Char,
            RinParser.SHORT => ScalarType.Short,
            RinParser.USHORT => ScalarType.UShort,
            RinParser.INT => ScalarType.Int,
            RinParser.UINT => ScalarType.UInt,
            RinParser.LONG => ScalarType.Long,
            RinParser.ULONG => ScalarType.ULong,
            
            _ => throw new("fatal")
        };
    }

    public override Node VisitFloating_point_type(RinParser.Floating_point_typeContext context) {
        if (context.FLOAT() != null) {
            return ScalarType.Float;
        }

        if (context.DOUBLE() != null) {
            return ScalarType.Double;
        }

        throw new("fatal");
    }

    public override Node VisitClass_type(RinParser.Class_typeContext context) {
        return base.VisitClass_type(context);
    }

    public override Node VisitType_argument_list(RinParser.Type_argument_listContext context) {
        return base.VisitType_argument_list(context);
    }
}
