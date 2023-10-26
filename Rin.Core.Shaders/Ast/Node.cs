namespace Rin.Core.Shaders.Ast;

public abstract record Node;
public abstract record Statement : Node;
public abstract record Expression : Node;
public abstract record Attribute : Node;


// TODO: attributes?
public record Identifier(string Text) : Node {
    public static implicit operator string(Identifier identifier) => identifier.Text;
    public static implicit operator Identifier(string name) => new(name);
}

public record Shader(Identifier Name, DeclarationList Declarations) : Node;


public record Variable(TypeBase Type, Identifier Name, Expression? InitialValue = null)
    : Node, IAttributes, IDeclaration, IQualifiers {
    public List<Attribute> Attributes { get; } = new();
    public Qualifier Qualifiers { get; } = Qualifier.None;
}

public record Parameter(TypeBase Type, Identifier Name, Expression? InitialValue = null) : Variable(Type, Name, InitialValue);


public abstract record TypeBase : Node, IAttributes, IQualifiers {
    public List<Attribute> Attributes { get; }
    public Qualifier Qualifiers { get; }

    public TypeBase(Identifier name) {
        // TODO
    }
}

public record VarType() : TypeBase(new Identifier("var"));
public record ValType() : TypeBase(new Identifier("val"));


// public record BlockStatement(StatementList Statements) : Statement;
public record EmptyStatement : Statement;
public record IfStatement(Expression Condition, Statement Body, Statement? Else) : Statement;
public record ForStatement(Statement Declaration, Expression Condition, Statement Body) : Statement;
public record WhileStatement(Expression Condition, Statement Body) : Statement;
public record RepeatStatement(Expression Condition, Statement Body) : Statement;
public record BreakStatement : Statement;
public record ContinueStatement : Statement;
public record ReturnStatement(Expression? Value = null) : Statement;



public record UnaryExpression(UnaryOperator Operator, Expression Expression) : Expression;
public record BinaryExpression(Expression Left, Expression Right, BinaryOperator Operator) : Expression;
public record RangeExpression(Expression Start, Expression End) : Expression;
public record CastExpression(Expression Expression, TypeBase Target) : Expression;


public record PackageStatement(Identifier Name) : Statement;
public record ImportStatement(Identifier Name) : Statement;
public record Module(PackageStatement Package, DeclarationList Declarations) : Node;

// TODO: should NULL literal be returned as null value?
public record Literal(object? Value) : Node;
public record LiteralExpression(Literal Value) : Expression;

public record ExpressionStatement(Expression Value) : Statement;
public record DeclarationStatement(Node Value) : Statement;



public record MethodDeclaration(
    Identifier Name,
    ParameterList Parameters,
    TypeBase ReturnType,
    Statement Body
) : Node, IDeclaration;

public record ConstructorDeclaration(ParameterList Parameters, Statement Body) : Node;
