namespace Vixen.Core.Shaders.Ast; 

public enum UnaryOperator {
    /// <summary>
    /// !
    /// </summary>
    LogicalNot,
    
    /// <summary>
    /// ~
    /// </summary>
    BitwiseNot,
    
    /// <summary>
    /// -
    /// </summary>
    Minus,
    
    /// <summary>
    /// +
    /// </summary>
    Plus,
    
    /// <summary>
    /// --
    /// </summary>
    PreDecrement,
    
    /// <summary>
    /// ++
    /// </summary>
    PreIncrement,
    
    /// <summary>
    /// --
    /// </summary>
    PostDecrement,
    
    /// <summary>
    /// ++
    /// </summary>
    PostIncrement
}
