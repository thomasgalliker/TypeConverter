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

        /// <summary>
        ///     Determines whether the specified types are considered equal.
        /// </summary>
        /// <param name="parent">A <see cref="System.Type" /> instance. </param>
        /// <param name="child">A type possible derived from the <c>parent</c> type</param>
        /// <returns>
        ///     True, when an object instance of the type <c>child</c>
        ///     can be used as an object of the type <c>parent</c>; otherwise, false.
        /// </returns>
        /// <remarks>
        ///     Note that nullable types does not have a parent-child relation to it's underlying type.
        ///     For example, the 'int?' type (nullable int) and the 'int' type
        ///     aren't a parent and it's child.
        /// </remarks>
        internal static bool IsSameOrParent(this Type parent, Type child)
        {
            Guard.ArgumentNotNull(parent, nameof(parent));
            Guard.ArgumentNotNull(child, nameof(child));

            var parentTypeInfo = parent.GetTypeInfo();
            var childTypeInfo = child.GetTypeInfo();

            if (parent == child || 
                parentTypeInfo.IsAssignableFrom(childTypeInfo) || 
                childTypeInfo.IsEnum && Enum.GetUnderlyingType(child) == parent ||
                childTypeInfo.IsSubclassOf(parent))
            {
                return true;
            }

            if (parentTypeInfo.IsGenericTypeDefinition)
            {
                var objectTypeInfo = typeof(object).GetTypeInfo();
                for (var t = childTypeInfo; t != objectTypeInfo && t != null; t = t.BaseType != null ? t.BaseType.GetTypeInfo() : null)
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == parent)
                    {
                        return true;
                    }
                }
            }

            if (parentTypeInfo.IsInterface)
            {
                var interfaces = childTypeInfo.ImplementedInterfaces.Select(i => i.GetTypeInfo());

                foreach (var t in interfaces)
                {
                    if (parentTypeInfo.IsGenericTypeDefinition)
                    {
                        if (t.IsGenericType && t.GetGenericTypeDefinition() == parent)
                        {
                            return true;
                        }
                    }
                    else if (t == parentTypeInfo)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static object GetDefault(this Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        internal static string GetFormattedName(this Type type)
        {
            Guard.ArgumentNotNull(type, nameof(type));

            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                return type.Name;
            }

            return string.Format("{0}<{1}>", type.Name.Substring(0, type.Name.IndexOf('`')), string.Join(", ", typeInfo.GenericTypeArguments.Select(t => t.GetFormattedName())));
        }

        internal static string GetFormattedFullname(this Type type)
        {
            Guard.ArgumentNotNull(type, nameof(type));

            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                return type.ToString();
            }

            return string.Format("{0}.{1}<{2}>", type.Namespace, type.Name.Substring(0, type.Name.IndexOf('`')), string.Join(", ", typeInfo.GenericTypeArguments.Select(t => t.GetFormattedFullname())));
        }
    }
}