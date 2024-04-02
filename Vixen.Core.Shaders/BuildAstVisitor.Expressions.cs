using Antlr4.Runtime.Tree;
using Vixen.Core.Shaders.Ast;

namespace Vixen.Core.Shaders; 

public partial class BuildAstVisitor {
    public override Node VisitExpression(VixenParser.ExpressionContext context) {
        return Visit(context.GetChild(0));
    }

    public override Node VisitNon_assignment_expression(VixenParser.Non_assignment_expressionContext context) {
        return Visit(context.GetChild(0));
    }
    
    public override Node VisitAssignment(VixenParser.AssignmentContext context) {
        var left = Visit(context.unary_expression()) as Expression;
        
        var opToken = context.assignment_operator().GetChild(0);
        if (opToken is not ITerminalNode terminalNode) {
            throw new("fatal");
        }
            
        var op = terminalNode.Symbol.Type switch {
            VixenParser.OP_COALESCING_ASSIGNMENT => AssignmentOperator.CoalescingAssignment,
            VixenParser.ASSIGNMENT => AssignmentOperator.Default,
            VixenParser.OP_ADD_ASSIGNMENT => AssignmentOperator.Addition,
            VixenParser.OP_SUB_ASSIGNMENT => AssignmentOperator.Subtraction,
            VixenParser.OP_MULT_ASSIGNMENT => AssignmentOperator.Multiplication,
            VixenParser.OP_DIV_ASSIGNMENT => AssignmentOperator.Division,
            VixenParser.OP_MOD_ASSIGNMENT => AssignmentOperator.Modulo,
            VixenParser.OP_AND_ASSIGNMENT => AssignmentOperator.BitwiseAnd,
            VixenParser.OP_OR_ASSIGNMENT => AssignmentOperator.BitwiseOr,
            VixenParser.OP_XOR_ASSIGNMENT => AssignmentOperator.BitwiseXor,
            VixenParser.OP_LEFT_SHIFT_ASSIGNMENT => AssignmentOperator.BitwiseShiftLeft,
            // TODO: right shift assignment
            
            _ => throw new("fatal")
        };
        
        var right = Visit(context.expression()) as Expression;
        return new AssignmentExpression(left, right, op);
    }

    public override Node VisitConditional_expression(VixenParser.Conditional_expressionContext context) {
        var left = Visit(context.null_coalescing_expression()) as Expression;

        if (context.INTERR() != null) {
            var then = Visit(context.GetChild(2)) as Expression;
            var @else = Visit(context.GetChild(4)) as Expression;

            return new ConditionalExpression(left, then, @else);
        }

        return left;
    }

    public override Node VisitNull_coalescing_expression(VixenParser.Null_coalescing_expressionContext context) {
        var left = Visit(context.conditional_or_expression()) as Expression;

        if (context.OP_COALESCING() != null) {
            var right = Visit(context.GetChild(2)) as Expression;
            return new NullCoalescingExpression(left, right);
        }

        return left;
    }

    public override Node VisitConditional_or_expression(VixenParser.Conditional_or_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitConditional_and_expression(VixenParser.Conditional_and_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitInclusive_or_expression(VixenParser.Inclusive_or_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitExclusive_or_expression(VixenParser.Exclusive_or_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitAnd_expression(VixenParser.And_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitEquality_expression(VixenParser.Equality_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    // TODO: relational expression should be handled differently

    public override Node VisitShift_expression(VixenParser.Shift_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitAdditive_expression(VixenParser.Additive_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitMultiplicative_expression(VixenParser.Multiplicative_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    Node VisitBinaryExpression(IParseTree context) {
        if (context.ChildCount == 1) {
            return Visit(context.GetChild(0));
        }
        
        var left = Visit(context.GetChild(0)) as Expression;
        for (var i = 1; i < context.ChildCount; i++) {
            var opToken = context.GetChild(i++);
            if (opToken is not ITerminalNode terminalNode) {
                throw new("fatal");
            }
            
            var op = terminalNode.Symbol.Type switch {
                VixenParser.STAR => BinaryOperator.Multiply,
                VixenParser.DIV => BinaryOperator.Divide,
                VixenParser.PERCENT => BinaryOperator.Modulo,
                VixenParser.PLUS => BinaryOperator.Plus,
                VixenParser.MINUS => BinaryOperator.Minus,
                VixenParser.OP_LEFT_SHIFT => BinaryOperator.LeftShift,
                // TODO: right shift
                VixenParser.OP_EQ => BinaryOperator.Equality,
                VixenParser.OP_NE => BinaryOperator.Inequality,
                VixenParser.AMP => BinaryOperator.BitwiseAnd,
                VixenParser.CARET => BinaryOperator.BitwiseXor,
                VixenParser.BITWISE_OR => BinaryOperator.BitwiseOr,
                VixenParser.OP_AND => BinaryOperator.LogicalAnd,
                VixenParser.OP_OR => BinaryOperator.LogicalOr,
                _ => throw new("fatal")
            };
            
            var right = Visit(context.GetChild(i)) as Expression;
            left = new BinaryExpression(left, right, op);
        }

        return left;
    }


    // TODO: switch
    
    public override Node VisitRange_expression(VixenParser.Range_expressionContext context) {
        if (context.unary_expression().Length == 2) {
            var start = Visit(context.unary_expression(0)) as Expression;
            var end = Visit(context.unary_expression(1)) as Expression;
            return new RangeExpression(start, end);
        }

        return Visit(context.unary_expression(0));
    }

    public override Node VisitUnary_expression(VixenParser.Unary_expressionContext context) {
        if (context.cast_expression() is { } castExpression) {
            return Visit(castExpression);
        }

        if (context.primary_expression() is { } primaryExpression) {
            return Visit(primaryExpression);
        }
        
        // TODO: post increment expressions needs to be handled elsewhere
        
        var opTerminalNode = context.GetChild(0) as ITerminalNode;
        var op = opTerminalNode.Symbol.Type switch {
            VixenParser.PLUS => UnaryOperator.Plus,
            VixenParser.MINUS => UnaryOperator.Minus,
            VixenParser.BANG => UnaryOperator.LogicalNot,
            VixenParser.TILDE => UnaryOperator.BitwiseNot,
            VixenParser.OP_INC => UnaryOperator.PreIncrement,
            VixenParser.OP_DEC => UnaryOperator.PreDecrement,
            _ => throw new("fatal")
        };
        
        var expr = Visit(context.unary_expression()) as Expression;
        return new UnaryExpression(op, expr);
    }

    public override Node VisitCast_expression(VixenParser.Cast_expressionContext context) {
        // TODO: type
        var expression = Visit(context.unary_expression()) as Expression;
        
        return new CastExpression(expression, null);
    }
}
