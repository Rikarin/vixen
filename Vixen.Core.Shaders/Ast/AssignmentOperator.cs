namespace Vixen.Core.Shaders.Ast;

public enum AssignmentOperator {
    /// <summary>
    ///     Operator =
    /// </summary>
    Default,

    /// <summary>
    ///     Operator +=
    /// </summary>
    Addition,

    /// <summary>
    ///     Operator -=
    /// </summary>
    Subtraction,

    /// <summary>
    ///     Operator *=
    /// </summary>
    Multiplication,

    /// <summary>
    ///     Operator /=
    /// </summary>
    Division,

    /// <summary>
    ///     Operator %=
    /// </summary>
    Modulo,

    /// <summary>
    ///     Operator &amp;=
    /// </summary>
    BitwiseAnd,

    /// <summary>
    ///     Operator |=
    /// </summary>
    BitwiseOr,

    /// <summary>
    ///     Operator ^=
    /// </summary>
    BitwiseXor,

    /// <summary>
    ///     Operator &lt;&lt;=
    /// </summary>
    BitwiseShiftLeft,

    /// <summary>
    ///     Operator >>=
    /// </summary>
    BitwiseShiftRight,
    
    /// <summary>
    /// ??=
    /// </summary>
    CoalescingAssignment
}
