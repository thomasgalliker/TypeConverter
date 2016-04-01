using System;

using TypeConverter.Utils;

namespace TypeConverter.Attempts
{
    /// <summary>
    /// Abstraction of a conversion attempt.
    /// </summary>
    internal interface IConversionAttempt
    {
        CastResult TryConvert(object value, Type sourceType, Type targetType);
    }
}