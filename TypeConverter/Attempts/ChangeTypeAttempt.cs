using System;

namespace TypeConverter.Attempts
{
    // Attempt 3: Use System.Convert.ChangeType to change value to targetType
    internal class ChangeTypeAttempt : IConversionAttempt
    {
        public ConversionResult TryConvert(object value, Type sourceType, Type targetType)
        {
            try
            {
                if (Nullable.GetUnderlyingType(targetType) != null)
                {
                    return this.TryConvert(value, sourceType, Nullable.GetUnderlyingType(targetType));
                }

                // ChangeType basically does some conversion checks
                // and then tries to perform the according Convert.ToWhatever(value) method.
                // See: http://referencesource.microsoft.com/#mscorlib/system/convert.cs,3bcca7a9bda4114e
                return new ConversionResult(Convert.ChangeType(value, targetType));
            }
            catch (Exception ex)
            {
                return new ConversionResult(ex);
            }
        }
    }
}