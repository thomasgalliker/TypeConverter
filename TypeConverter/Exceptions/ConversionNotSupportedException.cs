using System;

using TypeConverter.Extensions;

namespace TypeConverter.Exceptions
{
    public class ConversionNotSupportedException : Exception
    {
        private ConversionNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private ConversionNotSupportedException(string message)
            : base(message)
        {
        }

        public static ConversionNotSupportedException Create(Type sourceType, Type targetType, object sourceValue, Exception innerException = null)
        {
            string exceptionMessage =
                string.Format(
                    "Could not find IConverter<{0}, {1}> for source value of type {2}. " + "Use RegisterConverter method to register a converter which converts between type {0} and type {1}.",
                    sourceType.GetFormattedName(),
                    targetType.GetFormattedName(),
                    sourceValue != null ? sourceValue.GetType().GetFormattedName() : "[null]");

            if (innerException == null)
            {
                return new ConversionNotSupportedException(exceptionMessage);
            }
            return new ConversionNotSupportedException(exceptionMessage, innerException);
        }
    }
}