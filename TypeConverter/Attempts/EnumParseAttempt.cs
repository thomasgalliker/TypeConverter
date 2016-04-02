using System;
using System.Reflection;

namespace TypeConverter.Attempts
{
    // Attempt 4: Try to convert generic enum
    internal class EnumParseAttempt : IConversionAttempt
    {
        public ConversionResult TryConvert(object value, Type sourceType, Type targetType)
        {
            if (sourceType.GetTypeInfo().IsEnum)
            {
                return new ConversionResult(value.ToString());
            }

            if (targetType.GetTypeInfo().IsEnum)
            {
                try
                {
                    return new ConversionResult(Enum.Parse(targetType, value.ToString(), true));
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