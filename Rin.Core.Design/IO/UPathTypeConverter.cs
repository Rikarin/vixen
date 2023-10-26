using System.ComponentModel;
using System.Globalization;

namespace Rin.Core.Design.IO;

/// <summary>
///     An abstract implementation of <see cref="TypeConverter" /> used for types derived from <see cref="UPath" /> in
///     order to convert then from a string.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class UPathTypeConverter<T> : TypeConverter {
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <inheritdoc />
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) {
        return value is string stringPath ? Convert(stringPath) : base.ConvertFrom(context, culture, value);
    }

    /// <summary>
    ///     Performs the actual string conversion.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected abstract T Convert(string value);
}

/// <summary>
///     The implementation of <see cref="TypeConverter" /> for <see cref="UFile" /> that implements conversion from
///     strings.
/// </summary>
public sealed class UFileTypeConverter : UPathTypeConverter<UFile> {
    /// <inheritdoc />
    protected override UFile Convert(string value) => value;
}

/// <summary>
///     The implementation of <see cref="TypeConverter" /> for <see cref="UDirectory" /> that implements conversion from
///     strings.
/// </summary>
public sealed class UDirectoryTypeConverter : UPathTypeConverter<UDirectory> {
    /// <inheritdoc />
    protected override UDirectory Convert(string value) => value;
}
