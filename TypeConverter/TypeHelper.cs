using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using Guards;

using Microsoft.CSharp.RuntimeBinder;

using TypeConverter.Extensions;

using CSharpBinder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace TypeConverter
{
    public static class TypeHelper
    {
        #region ---- Explicit casts ----

        public static bool IsCastableTo(this Type from, Type to)
        {
            var fromTypeInfo = from.GetTypeInfo();
            var toTypeInfo = to.GetTypeInfo();

            // explicit conversion always works if to : from OR if there's an implicit conversion
            if (fromTypeInfo.IsAssignableFrom(toTypeInfo) || from.IsImplicitlyCastableTo(to))
            {
                return true;
            }

            var key = new KeyValuePair<Type, Type>(from, to);
            bool cachedValue;
            if (CastCache.TryGetCachedValue(key, out cachedValue))
            {
                return cachedValue;
            }

            // for nullable types, we can simply strip off the nullability and evaluate the underyling types
            var underlyingFrom = Nullable.GetUnderlyingType(from);
            var underlyingTo = Nullable.GetUnderlyingType(to);
            if (underlyingFrom != null || underlyingTo != null)
            {
                return (underlyingFrom ?? from).IsCastableTo(underlyingTo ?? to);
            }

            bool result;

            if (fromTypeInfo.IsValueType)
            {
                try
                {
                    ReflectionHelper.GetMethod(() => AttemptExplicitCast<object, object>()).GetGenericMethodDefinition().MakeGenericMethod(from, to).Invoke(null, new object[0]);
                    result = true;
                }
                catch (TargetInvocationException ex)
                {
                    result = !(ex.InnerException is RuntimeBinderException
                        // if the code runs in an environment where this message is localized, we could attempt a known failure first and base the regex on it's message
                               && Regex.IsMatch(ex.InnerException.Message, @"^Cannot convert type '.*' to '.*'$"));
                }
            }
            else
            {
                // if the from type is null, the dynamic logic above won't be of any help because 
                // either both types are nullable and thus a runtime cast of null => null will 
                // succeed OR we get a runtime failure related to the inability to cast null to 
                // the desired type, which may or may not indicate an actual issue. thus, we do 
                // the work manually
                result = from.IsNonValueTypeExplicitlyCastableTo(to);
            }

            CastCache.UpdateCache(key, result);
            return result;
        }

        private static bool IsNonValueTypeExplicitlyCastableTo(this Type from, Type to)
        {
            var fromTypeInfo = from.GetTypeInfo();
            var toTypeInfo = to.GetTypeInfo();

            if ((toTypeInfo.IsInterface && !fromTypeInfo.IsSealed) || (fromTypeInfo.IsInterface && !toTypeInfo.IsSealed))
            {
                // any non-sealed type can be cast to any interface since the runtime type MIGHT implement
                // that interface. The reverse is also true; we can cast to any non-sealed type from any interface
                // since the runtime type that implements the interface might be a derived type of to.
                return true;
            }

            // arrays are complex because of array covariance 
            // (see http://msmvps.com/blogs/jon_skeet/archive/2013/06/22/array-covariance-not-just-ugly-but-slow-too.aspx).
            // Thus, we have to allow for things like var x = (IEnumerable<string>)new object[0];
            // and var x = (object[])default(IEnumerable<string>);
            var arrayType = from.IsArray && !from.GetElementType().GetTypeInfo().IsValueType ? from : to.IsArray && !to.GetElementType().GetTypeInfo().IsValueType ? to : null;
            if (arrayType != null)
            {
                var genericInterfaceType = fromTypeInfo.IsInterface && fromTypeInfo.IsGenericType ? from : toTypeInfo.IsInterface && toTypeInfo.IsGenericType ? to : null;
                if (genericInterfaceType != null)
                {
                    return
                        arrayType.GetTypeInfo()
                            .ImplementedInterfaces.Any(
                                i =>
                                i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType.GetGenericTypeDefinition()
                                && i.GenericTypeArguments.Zip(
                                    to.GenericTypeArguments,
                                    (ia, ta) => ta.GetTypeInfo().IsAssignableFrom(ia.GetTypeInfo()) || ia.GetTypeInfo().IsAssignableFrom(ta.GetTypeInfo())).All(b => b));
                }
            }

            // look for conversion operators. Even though we already checked for implicit conversions, we have to look
            // for operators of both types because, for example, if a class defines an implicit conversion to int then it can be explicitly
            // cast to uint

            var conversionMethods =
                fromTypeInfo.GetDeclaredMethodsRecursively()
                    .Where(m => (m.Name == "op_Explicit" || m.Name == "op_Implicit") &&
                        m.Attributes.HasFlag(MethodAttributes.SpecialName) &&
                        m.GetParameters().Length == 1 &&
                        (
                        // the from argument of the conversion function can be an indirect match to from in
                        // either direction. For example, if we have A : B and Foo defines a conversion from B => Foo,
                        // then C# allows A to be cast to Foo
                        m.GetParameters()[0].ParameterType.GetTypeInfo().IsAssignableFrom(fromTypeInfo) ||
                        fromTypeInfo.IsAssignableFrom(m.GetParameters()[0].ParameterType.GetTypeInfo())));

            if (toTypeInfo.IsPrimitive && toTypeInfo.ImplementedInterfaces.Any(x => x.Name == "IConvertible"))
            {
                // as mentioned above, primitive convertible types (i. e. not IntPtr) get special 
                // treatment in the sense that if you can convert from Foo => int, you can convert
                // from Foo => double as well
                return conversionMethods.Any(m => m.ReturnType.IsCastableTo(to));
            }

            return conversionMethods.Any(m => m.ReturnType == to);
        }

        public static TTo AttemptExplicitCast<TFrom, TTo>()
        {
            // based on the IL generated from
            // var x = (TTo)(dynamic)default(TFrom);

            var binder = CSharpBinder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(TTo), typeof(TypeHelper));
            var callSite = CallSite<Func<CallSite, TFrom, TTo>>.Create(binder);
            return callSite.Target(callSite, default(TFrom));
        }

        public static object ExplicitCast(object value, Type sourceType, Type targetType, bool explict)
        {
            return ReflectionHelper.GetMethod(() => DoExplicitCast<object, object>(null, false)).GetGenericMethodDefinition().MakeGenericMethod(sourceType, targetType).Invoke(null, new[] { value, explict });
        }

        private static TTo DoExplicitCast<TFrom, TTo>(TFrom value, bool explict)
        {
            // based on the IL generated from
            // var x = (TTo)(dynamic)value;

            var flags = explict ? CSharpBinderFlags.ConvertExplicit : CSharpBinderFlags.None;
            var binder = CSharpBinder.Convert(flags, typeof(TTo), typeof(TypeHelper));
            var callSite = CallSite<Func<CallSite, TFrom, TTo>>.Create(binder);
            //dynamic tDynCallSite = callSite;
            return callSite.Target(callSite, value);
        }

        ////public static object ExplicitCast(object value, Type targetType, bool explict)
        ////{
        ////    // based on the IL generated from
        ////    // var x = (TTo)(dynamic)default(TFrom);

        ////    var tFlags = explict ? CSharpBinderFlags.ConvertExplicit : CSharpBinderFlags.None;

         

        ////    var binder = CSharpBinder.Convert(tFlags, targetType, typeof(TypeHelper));
        ////    var callSite = CallSite<Func<CallSite, object, object>>.Create(binder);
        ////    return callSite.Target(callSite, value);
        ////}

        ////public static object InvokeConvertCallSite<TFrom, TTo>(object target, bool explict)
        ////{

        ////    var tFlags = explict ? CSharpBinderFlags.ConvertExplicit : CSharpBinderFlags.None;

        ////    var tBinder = CSharpBinder.Convert(tFlags, typeof(TTo), typeof(TypeHelper));
        ////    var callSite = CallSite<Func<CallSite, TFrom, TTo>>.Create(tBinder);

        ////    dynamic tDynCallSite = callSite;

        ////    return tDynCallSite.Target(callSite, target);
        ////}


        #endregion

        #region ---- Implicit casts ----

        public static bool IsImplicitlyCastableTo(this Type from, Type to)
        {
            Guard.ArgumentNotNull(() => from);
            Guard.ArgumentNotNull(() => to);

            var toTypeInfo = to.GetTypeInfo();
            var fromTypeInfo = from.GetTypeInfo();

            // not strictly necessary, but speeds things up and avoids polluting the cache
            if (toTypeInfo.IsAssignableFrom(fromTypeInfo))
            {
                return true;
            }

            var key = new KeyValuePair<Type, Type>(from, to);
            bool cachedValue;
            if (ImplicitCastCache.TryGetCachedValue(key, out cachedValue))
            {
                return cachedValue;
            }

            bool result;
            try
            {
                // overload of GetMethod() from http://www.codeducky.org/10-utilities-c-developers-should-know-part-two/ 
                // that takes Expression<Action>
                ReflectionHelper.GetMethod(() => AttemptImplicitCast<object, object>()).GetGenericMethodDefinition().MakeGenericMethod(from, to).Invoke(null, new object[0]);
                result = true;
            }
            catch (TargetInvocationException ex)
            {
                result = !(ex.InnerException is RuntimeBinderException
                    // if the code runs in an environment where this message is localized, we could attempt a known failure first and base the regex on it's message
                           && Regex.IsMatch(ex.InnerException.Message, @"^The best overloaded method match for 'System.Collections.Generic.List<.*>.Add(.*)' has some invalid arguments$"));
            }

            ImplicitCastCache.UpdateCache(key, result);
            return result;
        }

        public static TTo AttemptImplicitCast<TFrom, TTo>()
        {
            // based on the IL produced by:
            // dynamic list = new List<TTo>();
            // list.Add(default(TFrom));
            // We can't use the above code because it will mimic a cast in a generic method
            // which doesn't have the same semantics as a cast in a non-generic method

            var list = new List<TTo>(capacity: 1);
            var binder = CSharpBinder.InvokeMember(
                flags: CSharpBinderFlags.ResultDiscarded,
                name: "Add",
                typeArguments: null,
                context: typeof(TypeHelper),
                argumentInfo:
                    new[] { CSharpArgumentInfo.Create(flags: CSharpArgumentInfoFlags.None, name: null), CSharpArgumentInfo.Create(flags: CSharpArgumentInfoFlags.UseCompileTimeType, name: null), });
            var callSite = CallSite<Action<CallSite, object, TFrom>>.Create(binder);
            callSite.Target.Invoke(callSite, list, default(TFrom));

            return list[0];
        }

        #endregion

        #region ---- Caching ----

        public static bool IsCacheEnabled = true;
        private const int MaxCacheSize = 5000;
        private static readonly Dictionary<KeyValuePair<Type, Type>, bool> CastCache = new Dictionary<KeyValuePair<Type, Type>, bool>();
        private static readonly Dictionary<KeyValuePair<Type, Type>, bool> ImplicitCastCache = new Dictionary<KeyValuePair<Type, Type>, bool>();

        private static readonly object SyncObj = new object();

        private static bool TryGetCachedValue<TKey, TValue>(this Dictionary<TKey, TValue> cache, TKey key, out TValue value)
        {
            if (IsCacheEnabled == false)
            {
                value = default(TValue);
                return false;
            }

            lock (SyncObj)
            {
                return cache.TryGetValue(key, out value);
            }
        }

        private static void UpdateCache<TKey, TValue>(this Dictionary<TKey, TValue> cache, TKey key, TValue value)
        {
            if (IsCacheEnabled == false)
            {
                return;
            }

            lock (SyncObj)
            {
                if (cache.Count > MaxCacheSize)
                {
                    cache.Clear();
                }
                cache[key] = value;
            }
        }

        #endregion
    }
}