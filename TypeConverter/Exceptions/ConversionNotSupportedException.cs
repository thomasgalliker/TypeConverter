using System;

namespace TypeConverter.Exceptions
{
    public class ConversionNotSupportedException : Exception
    {
        private ConversionNotSupportedException(string message)
            : base(message)
        {
        }

        public static ConversionNotSupportedException Create(Type sourceType, Type targetType, object sourceValue)
        {
            return
                new ConversionNotSupportedException(
                    string.Format(
                        "Could not find IConverter<{0}, {1}> for source value of type {2}. " + "Use RegisterConverter method to register a converter which converts between type {0} and type {1}.",
                        sourceType.Name,
                        targetType.Name,
                        sourceValue != null ? sourceValue.GetType().Name : "[null]"));
        }
    }
}