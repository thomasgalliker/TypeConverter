using System;
using System.Reflection;

namespace TypeConverter.Attempts
{
    // Attempt 5: We essentially make a guess that to convert from a string
    // to an arbitrary type T there will be a static method defined on type T called Parse
    // that will take an argument of type string. i.e. T.Parse(string)->T we call this
    // method to convert the string to the type required by the property.
    internal class StringParseAttempt : IConversionAttempt
    {
        public ConversionResult TryConvert(object value, Type sourceType, Type targetType)
        {
            // Either of both, sourceType or targetType, need to be typeof(string)
            if (sourceType == typeof(string) && targetType != typeof(string))
            {
                var parseMethod = targetType.GetRuntimeMethod("Parse", new[] { sourceType });
                if (parseMethod != null)
                {
                    try
                    {
                        return new ConversionResult(parseMethod.Invoke(this, new[] { value }));
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
            else if (targetType == typeof(string) && sourceType != typeof(string))
            {
                return new ConversionResult(value.ToString());
            }

            return null;
        }
    }
}