using System;

using TypeConverter.Extensions;

namespace TypeConverter.Exceptions
{
    internal class ConversionNotSupportedException : Exception
    {
        private ConversionNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private ConversionNotSupportedException(string message)
            : base(message)
        {
        }

        public static ConversionNotSupportedException Create(Type sourceType, Type targetType, Exception innerException = null)
        {
            var convertableInterfaceType = typeof(IConvertable);
            string exceptionMessage =
                string.Format(
                    "Could not find {0}<{1}, {2}>. Use RegisterConverter method to register a converter which converts between type {1} and type {2}.",
                    convertableInterfaceType.GetFormattedName(),
                    sourceType.GetFormattedName(),
                    targetType.GetFormattedName());

            if (innerException == null)
            {
                return new ConversionNotSupportedException(exceptionMessage);
            }
            return new ConversionNotSupportedException(exceptionMessage, innerException);
        }

        public static ConversionNotSupportedException Create(Type sourceType, Type targetType, string message)
        {
            string exceptionMessage =
               string.Format(
                   "Could not convert between type {0} and {1}. " + message,
                   sourceType.GetFormattedName(),
                   targetType.GetFormattedName());

            return new ConversionNotSupportedException(exceptionMessage);
        }
    }
}