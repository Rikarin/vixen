namespace Rin.Core.Shaders.Ast; 

public enum BinaryOperator {
    None,
    
    /// <summary>
    /// &&
    /// </summary>
    LogicalAnd,
    
    /// <summary>
    /// ||
    /// </summary>
    LogicalOr,
    
    /// <summary>
    /// &
    /// </summary>
    BitwiseAnd,
    
    /// <summary>
    /// |
    /// </summary>
    BitwiseOr,
    
    /// <summary>
    /// ^
    /// </summary>
    BitwiseXor,
    
    /// <summary>
    /// <<
    /// </summary>
    LeftShift,
    
    /// <summary>
    /// >>
    /// </summary>
    RightShift,
    
    /// <summary>
    /// -
    /// </summary>
    Minus,
    
    /// <summary>
    /// +
    /// </summary>
    Plus,
    
    /// <summary>
    /// *
    /// </summary>
    Multiply,
    
    /// <summary>
    /// /
    /// </summary>
    Divide,
    
    /// <summary>
    /// %
    /// </summary>
    Modulo,
    
    /// <summary>
    /// <
    /// </summary>
    Less,
    
    /// <summary>
    /// <=
    /// </summary>
    LessEqual,
    
    /// <summary>
    /// >
    /// </summary>
    Greater,
    
    /// <summary>
    /// >=
    /// </summary>
    GreaterEqual,
    
    /// <summary>
    /// ==
    /// </summary>
    Equality,
    
    /// <summary>
    /// !=
    /// </summary>
    Inequality
}
