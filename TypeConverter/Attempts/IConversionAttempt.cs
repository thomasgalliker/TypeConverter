using System;

namespace TypeConverter.Attempts
{
    /// <summary>
    ///     Abstraction of a conversion attempt.
    /// </summary>
    internal interface IConversionAttempt
    {
        ConversionResult TryConvert(object value, Type sourceType, Type targetType);
    }
}