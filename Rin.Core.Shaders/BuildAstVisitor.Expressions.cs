using Antlr4.Runtime.Tree;
using Rin.Core.Shaders.Ast;

namespace Rin.Core.Shaders; 

public partial class BuildAstVisitor {
    public override Node VisitExpression(RinParser.ExpressionContext context) {
        return Visit(context.GetChild(0));
    }

    public override Node VisitNon_assignment_expression(RinParser.Non_assignment_expressionContext context) {
        return Visit(context.GetChild(0));
    }
    
    public override Node VisitAssignment(RinParser.AssignmentContext context) {
        var left = Visit(context.unary_expression()) as Expression;
        
        var opToken = context.assignment_operator().GetChild(0);
        if (opToken is not ITerminalNode terminalNode) {
            throw new("fatal");
        }
            
        var op = terminalNode.Symbol.Type switch {
            RinParser.OP_COALESCING_ASSIGNMENT => AssignmentOperator.CoalescingAssignment,
            RinParser.ASSIGNMENT => AssignmentOperator.Default,
            RinParser.OP_ADD_ASSIGNMENT => AssignmentOperator.Addition,
            RinParser.OP_SUB_ASSIGNMENT => AssignmentOperator.Subtraction,
            RinParser.OP_MULT_ASSIGNMENT => AssignmentOperator.Multiplication,
            RinParser.OP_DIV_ASSIGNMENT => AssignmentOperator.Division,
            RinParser.OP_MOD_ASSIGNMENT => AssignmentOperator.Modulo,
            RinParser.OP_AND_ASSIGNMENT => AssignmentOperator.BitwiseAnd,
            RinParser.OP_OR_ASSIGNMENT => AssignmentOperator.BitwiseOr,
            RinParser.OP_XOR_ASSIGNMENT => AssignmentOperator.BitwiseXor,
            RinParser.OP_LEFT_SHIFT_ASSIGNMENT => AssignmentOperator.BitwiseShiftLeft,
            // TODO: right shift assignment
            
            _ => throw new("fatal")
        };
        
        var right = Visit(context.expression()) as Expression;
        return new AssignmentExpression(left, right, op);
    }

    public override Node VisitConditional_expression(RinParser.Conditional_expressionContext context) {
        var left = Visit(context.null_coalescing_expression()) as Expression;

        if (context.INTERR() != null) {
            var then = Visit(context.GetChild(2)) as Expression;
            var @else = Visit(context.GetChild(4)) as Expression;

            return new ConditionalExpression(left, then, @else);
        }

        return left;
    }

    public override Node VisitNull_coalescing_expression(RinParser.Null_coalescing_expressionContext context) {
        var left = Visit(context.conditional_or_expression()) as Expression;

        if (context.OP_COALESCING() != null) {
            var right = Visit(context.GetChild(2)) as Expression;
            return new NullCoalescingExpression(left, right);
        }

        return left;
    }

    public override Node VisitConditional_or_expression(RinParser.Conditional_or_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitConditional_and_expression(RinParser.Conditional_and_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitInclusive_or_expression(RinParser.Inclusive_or_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitExclusive_or_expression(RinParser.Exclusive_or_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitAnd_expression(RinParser.And_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitEquality_expression(RinParser.Equality_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    // TODO: relational expression should be handled differently

    public override Node VisitShift_expression(RinParser.Shift_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitAdditive_expression(RinParser.Additive_expressionContext context) {
        return VisitBinaryExpression(context);
    }

    public override Node VisitMultiplicative_expression(RinParser.Multiplicative_expressionContext context) {
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
                RinParser.STAR => BinaryOperator.Multiply,
                RinParser.DIV => BinaryOperator.Divide,
                RinParser.PERCENT => BinaryOperator.Modulo,
                RinParser.PLUS => BinaryOperator.Plus,
                RinParser.MINUS => BinaryOperator.Minus,
                RinParser.OP_LEFT_SHIFT => BinaryOperator.LeftShift,
                // TODO: right shift
                RinParser.OP_EQ => BinaryOperator.Equality,
                RinParser.OP_NE => BinaryOperator.Inequality,
                RinParser.AMP => BinaryOperator.BitwiseAnd,
                RinParser.CARET => BinaryOperator.BitwiseXor,
                RinParser.BITWISE_OR => BinaryOperator.BitwiseOr,
                RinParser.OP_AND => BinaryOperator.LogicalAnd,
                RinParser.OP_OR => BinaryOperator.LogicalOr,
                _ => throw new("fatal")
            };
            
            var right = Visit(context.GetChild(i)) as Expression;
            left = new BinaryExpression(left, right, op);
        }

        return left;
    }


    // TODO: switch
    
    public override Node VisitRange_expression(RinParser.Range_expressionContext context) {
        if (context.unary_expression().Length == 2) {
            var start = Visit(context.unary_expression(0)) as Expression;
            var end = Visit(context.unary_expression(1)) as Expression;
            return new RangeExpression(start, end);
        }

        return Visit(context.unary_expression(0));
    }

    public override Node VisitUnary_expression(RinParser.Unary_expressionContext context) {
        if (context.cast_expression() is { } castExpression) {
            return Visit(castExpression);
        }

        if (context.primary_expression() is { } primaryExpression) {
            return Visit(primaryExpression);
        }
        
        // TODO: post increment expressions needs to be handled elsewhere
        
        var opTerminalNode = context.GetChild(0) as ITerminalNode;
        var op = opTerminalNode.Symbol.Type switch {
            RinParser.PLUS => UnaryOperator.Plus,
            RinParser.MINUS => UnaryOperator.Minus,
            RinParser.BANG => UnaryOperator.LogicalNot,
            RinParser.TILDE => UnaryOperator.BitwiseNot,
            RinParser.OP_INC => UnaryOperator.PreIncrement,
            RinParser.OP_DEC => UnaryOperator.PreDecrement,
            _ => throw new("fatal")
        };
        
        var expr = Visit(context.unary_expression()) as Expression;
        return new UnaryExpression(op, expr);
    }

    public override Node VisitCast_expression(RinParser.Cast_expressionContext context) {
        // TODO: type
        var expression = Visit(context.unary_expression()) as Expression;
        
        return new CastExpression(expression, null);
    }
}
