namespace Rin.Core;

/// <summary>
///     Delegate ValidateValueCallback used by <see cref="ValidateValueMetadata" />.
/// </summary>
/// <param name="value">The value to validate and coerce.</param>
public delegate void ValidateValueCallback<T>(ref T value);

public abstract class ValidateValueMetadata : PropertyKeyMetadata {
    public static ValidateValueMetadata<T> New<T>(ValidateValueCallback<T> invalidationCallback) =>
        new(invalidationCallback);

    public abstract void Validate(ref object obj);
}

/// <summary>
///     A metadata to allow validation/coercion of a value before storing the value into the
///     <see cref="PropertyContainer" />.
/// </summary>
public class ValidateValueMetadata<T> : ValidateValueMetadata {
    /// <summary>
    ///     Gets the validate value callback.
    /// </summary>
    /// <value>The validate value callback.</value>
    public ValidateValueCallback<T> ValidateValueCallback { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValidateValueMetadata{T}" /> class.
    /// </summary>
    /// <param name="validateValueCallback">The validate value callback.</param>
    /// <exception cref="System.ArgumentNullException">validateValueCallback</exception>
    public ValidateValueMetadata(ValidateValueCallback<T> validateValueCallback) {
        ValidateValueCallback = validateValueCallback;
    }

    public override void Validate(ref object obj) {
        var objCopy = (T)obj;
        ValidateValueCallback(ref objCopy);
        obj = objCopy;
    }
}
