using System;
using System.Reflection;

namespace TypeConverter.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        ///     Checks if the given type is a Nullable type.
        /// </summary>
        public static bool IsNullable(this Type type)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return true;
            }

            return false;
        }

        // TODO GATH: CHeck this: http://stackoverflow.com/questions/2490244/default-value-of-a-type-at-runtime
        /// <summary>
        ///     Returns the default value for the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Value type: Default instance. Reference type: null.</returns>
        public static object GetDefaultValue(this Type type)
        {
            if (type.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(type) == null)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
    }
}