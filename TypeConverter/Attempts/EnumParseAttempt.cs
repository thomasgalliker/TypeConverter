using System;
using System.Reflection;

using TypeConverter.Utils;

namespace TypeConverter.Attempts
{
    // Attempt 4: Try to convert generic enum
    internal class EnumParseAttempt : IConversionAttempt
    {
        public CastResult TryConvert(object value, Type sourceType, Type targetType)
        {
            if (sourceType.GetTypeInfo().IsEnum)
            {
                return new CastResult(value.ToString(), CastFlag.Undefined);
            }

            if (targetType.GetTypeInfo().IsEnum)
            {
                try
                {
                    return new CastResult(Enum.Parse(targetType, value.ToString(), true), CastFlag.Undefined);
                }
                catch (ArgumentException)
                {
                    // Unfortunately, we cannot use Enum.TryParse in this case,
                    // The only way to catch failing parses is this ugly try-catch
                    return null;
                }
            }

            return null;
        }
    }
}