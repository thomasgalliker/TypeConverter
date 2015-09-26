using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Guards;

namespace TypeConverter.Extensions
{
    internal static class TypeExtensions
    {
        internal static IEnumerable<MethodInfo> GetDeclaredMethodsRecursively(this Type type)
        {
            return GetDeclaredMethodsRecursively(type.GetTypeInfo());
        }

        internal static IEnumerable<MethodInfo> GetDeclaredMethodsRecursively(this TypeInfo typeInfo)
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
            var temp = methods.ToList();
            methods = new List<MethodInfo>(typeInfo.DeclaredMethods);
            methods.AddRange(temp);

            return GetDeclaredMethodsRecursively(typeInfo.BaseType, methods);
        }

        internal static string GetFormattedName(this Type type)
        {
            Guard.ArgumentNotNull(() => type);

            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                return type.Name;
            }

            return string.Format("{0}<{1}>", type.Name.Substring(0, type.Name.IndexOf('`')), string.Join(", ", typeInfo.GenericTypeArguments.Select(t => t.GetFormattedName())));
        }

        internal static string GetFormattedFullname(this Type type)
        {
            Guard.ArgumentNotNull(() => type);

            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                return type.ToString();
            }

            return string.Format("{0}.{1}<{2}>", type.Namespace, type.Name.Substring(0, type.Name.IndexOf('`')), string.Join(", ", typeInfo.GenericTypeArguments.Select(t => t.GetFormattedFullname())));
        }
    }
}