using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Guards;

using Microsoft.CSharp.RuntimeBinder;

using TypeConverter.Exceptions;
using TypeConverter.Extensions;

using CSharpBinder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace TypeConverter.Utils
{
    internal static class TypeHelper
    {
        internal static CastResult CastTo(object value, Type targetType)
        {
            if (value == null)
            {
                return new CastResult((object)null);
            }

            Guard.ArgumentNotNull(() => targetType);

            var sourceType = value.GetType();
            var sourceTypeInfo = value.GetType().GetTypeInfo();
            var targetTypeInfo = targetType.GetTypeInfo();

            // explicit conversion always works if to : from OR if there's an implicit conversion
            if (targetTypeInfo.IsAssignableFrom(sourceTypeInfo))
            {
                return CastImplicitlyTo(value, targetType);
            }

            var key = new KeyValuePair<Type, Type>(sourceType, targetType);
            bool cachedValue;
            if (CastCache.TryGetCachedValue(key, out cachedValue))
            {
                if (cachedValue)
                {
                    return new CastResult(value);
                }
                return new CastResult(ConversionNotSupportedException.Create(sourceType, targetType, value));
            }

            // for nullable types, we can simply strip off the nullability and evaluate the underyling types
            var underlyingTo = Nullable.GetUnderlyingType(targetType);
            if (underlyingTo != null)
            {
                return CastTo(value, (underlyingTo));
            }

            CastResult castResult = null;

            try
            {
                if (sourceTypeInfo.IsValueType)
                {
                    var castedValue = GenericCast(value, sourceType, targetType, isImplicit: false);
                    castResult = new CastResult(castedValue);
                }
                else
                {
                    // Implicit cast operators have priority in favour of explicit operators
                    // since they should not lose precision. See C# language specification: 
                    // https://msdn.microsoft.com/en-us/library/z5z9kes2.aspx
                    var conversionMethods = GetCastOperatorMethods(sourceType, targetType)
                        .OrderByDescending(m => m.Name == "op_Implicit")
                        .ThenByDescending(m => m.ReturnType == targetType || m.ReturnType.GetTypeInfo().IsAssignableFrom(targetTypeInfo));

                    foreach (var conversionMethod in conversionMethods)
                    {
                        try
                        {
                            var convertedValue = conversionMethod.Invoke(null, new[] { value });
                            castResult = CastTo(convertedValue, targetType);
                            if (castResult.IsSuccessful)
                            {
                                break;
                            }
                            else
                            {
                                castResult = null; 
                            }
                        }
                        catch
                        {
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                castResult = new CastResult(ConversionNotSupportedException.Create(sourceType, targetType, value, ex.InnerException));
            }

            if (castResult == null)
            {
                castResult = new CastResult(ConversionNotSupportedException.Create(sourceType, targetType, value));
            }

            CastCache.UpdateCache(key, castResult.IsSuccessful);

            return castResult;
        }

        /// <summary>
        /// This methods returns a list of cast operation methods (implicit as well as explicit operators).
        /// </summary>
        private static IEnumerable<MethodInfo> GetCastOperatorMethods(Type sourceType, Type targetType)
        {
            var methodsOfSourceType = sourceType.GetDeclaredMethodsRecursively();
            var methodsOfTargetType = targetType.GetDeclaredMethodsRecursively();

            foreach (var mi in methodsOfSourceType.Concat(methodsOfTargetType))
            {
                if (mi.IsSpecialName && (mi.Name == "op_Implicit" || mi.Name == "op_Explicit"))
                {
                    var parameters = mi.GetParameters();

                    if (parameters.Length == 1 &&
                        (parameters[0].ParameterType.GetTypeInfo().IsAssignableFrom(sourceType.GetTypeInfo()) ||
                         sourceType.GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType.GetTypeInfo())))
                    {
                        yield return mi;
                    }
                }
            }
        }

        internal static CastResult CastImplicitlyTo(object value, Type targetType)
        {
            Guard.ArgumentNotNull(() => value);
            Guard.ArgumentNotNull(() => targetType);

            var sourceType = value.GetType();

            var toTypeInfo = targetType.GetTypeInfo();
            var fromTypeInfo = sourceType.GetTypeInfo();

            // not strictly necessary, but speeds things up and avoids polluting the cache
            if (toTypeInfo.IsAssignableFrom(fromTypeInfo))
            {
                return new CastResult(value);
            }

            var key = new KeyValuePair<Type, Type>(sourceType, targetType);
            bool cachedValue;
            if (ImplicitCastCache.TryGetCachedValue(key, out cachedValue))
            {
                if (cachedValue)
                {
                    return new CastResult(value);
                }
                return new CastResult(ConversionNotSupportedException.Create(sourceType, targetType, value));
            }

            CastResult castResult = null;
            try
            {
                var castedValue = GenericCast(value, sourceType, targetType, isImplicit: true);
                castResult = new CastResult(castedValue);
            }
            catch (TargetInvocationException ex)
            {
                castResult = new CastResult(ConversionNotSupportedException.Create(sourceType, targetType, value, ex.InnerException));
            }

            ImplicitCastCache.UpdateCache(key, castResult.IsSuccessful);
            return castResult;
        }

        private static object GenericCast(object value, Type sourceType, Type targetType, bool isImplicit)
        {
            var genericCastMethod = ReflectionHelper.GetMethod(() => DoCast<object, object>(null, false)).GetGenericMethodDefinition();
            var castedValue = genericCastMethod.MakeGenericMethod(sourceType, targetType).Invoke(null, new[] { value, isImplicit });
            return castedValue;
        }

        private static TTo DoCast<TFrom, TTo>(TFrom value, bool isImplicit)
        {
            // based on the IL generated from
            //var x = (TTo)(dynamic)value;

            var flags = isImplicit ? CSharpBinderFlags.None : CSharpBinderFlags.ConvertExplicit;
            var binder = CSharpBinder.Convert(flags, typeof(TTo), typeof(TypeHelper));
            var callSite = CallSite<Func<CallSite, TFrom, TTo>>.Create(binder);
            //dynamic tDynCallSite = callSite;
            return callSite.Target(callSite, value);
        }

        private static TTo AttemptExplicitCast<TFrom, TTo>(TFrom value)
        {
            // based on the IL generated from
            // var x = (TTo)(dynamic)value;

            var binder = CSharpBinder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(TTo), typeof(TypeHelper));
            var callSite = CallSite<Func<CallSite, TFrom, TTo>>.Create(binder);
            return callSite.Target(callSite, value);
        }

        private static TTo AttemptImplicitCast<TFrom, TTo>(TFrom value = default(TFrom))
        {
            // based on the IL produced by:
            // dynamic list = new List<TTo>();
            // list.Add(value);
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
            callSite.Target.Invoke(callSite, list, value);

            return list[0];
        }

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