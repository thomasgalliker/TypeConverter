using System;

namespace TypeConverter.Exceptions
{
    public class ConversionNotSupportedException : Exception
    {
        public ConversionNotSupportedException(String message)
            : base(message)
        {
        }
  
        public ConversionNotSupportedException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public static ConversionNotSupportedException Create(Type sourceType, Type targetType, object sourceValue)
        {
            return new ConversionNotSupportedException(
                   string.Format(
                       "Could not find IConverter<{0}, {1}> for value of type {2}. " + 
                       "Use RegisterConverter method to register a converter which converts between type {0} and type {1}.",
                       sourceType,
                       targetType,
                       sourceValue != null ? sourceValue.GetType().Name : "[null]"));
        }
    }
}