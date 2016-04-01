using System;

using TypeConverter.Utils;

namespace TypeConverter.Attempts
{
    // Attempt 2: Use implicit or explicit casting if supported
    internal class CastAttempt : IConversionAttempt
    {
        public CastResult TryConvert(object value, Type sourceType, Type targetType)
        {
            var castedValue = TypeHelper.CastTo(value, targetType);
            return castedValue;
        }
    }
}