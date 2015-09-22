﻿using System;
using System.Collections.Generic;
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

        public static IEnumerable<MethodInfo> GetDeclaredMethodsRecursively(this Type type)
        {
            return GetDeclaredMethodsRecursively(type.GetTypeInfo());
        }

        public static IEnumerable<MethodInfo> GetDeclaredMethodsRecursively(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                return null;
            }

            var methods = GetDeclaredMethodsRecursively(typeInfo.AsType(), new List<MethodInfo>());
            return methods;
        }

        private static IEnumerable<MethodInfo> GetDeclaredMethodsRecursively(this Type type, List<MethodInfo> methods)
        {
            if (type == null)
            {
                return methods;
            }

            var typeInfo = type.GetTypeInfo();
            methods.AddRange(typeInfo.DeclaredMethods);

            return GetDeclaredMethodsRecursively(typeInfo.BaseType, methods);
        }
    }
}