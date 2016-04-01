using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                return new CastResult((object)null, CastFlag.Undefined);
            }

            Guard.ArgumentNotNull(() => targetType);

            var sourceType = value.GetType();
            var sourceTypeInfo = value.GetType().GetTypeInfo();
            var targetTypeInfo = targetType.GetTypeInfo();

            if (targetTypeInfo.IsGenericType && !targetTypeInfo.GenericTypeArguments.Any())
            {
                return
                    new CastResult(
                        ConversionNotSupportedException.Create(
                            sourceType,
                            targetType,
                            string.Format("The target type {0} does not have sufficient generic type arguments specified.", targetType.GetFormattedName())),
                        CastFlag.Undefined);
            }

            CastResult castResult = null;
            CastFlag castFlag = CastFlag.Undefined;

            try
            {
                // explicit conversion always works if to : from OR if there's an implicit conversion
                if (targetType.IsSameOrParent(sourceType))
                {
                    castFlag = CastFlag.Implicit;
                    var castedValue = GenericCast(() => AttemptImplicitCast<object, object>(null), sourceType, targetType, value);
                    castResult = new CastResult(castedValue, castFlag);
                }
                // for nullable types, we can simply strip off the nullability and evaluate the underyling types
                else if (Nullable.GetUnderlyingType(targetType) != null)
                {
                    castResult = CastTo(value, Nullable.GetUnderlyingType(targetType));
                }
                else if (sourceTypeInfo.IsValueType)
                {
                    castFlag = CastFlag.Explicit;
                    var castedValue = GenericCast(() => AttemptExplicitCast<object, object>(null), sourceType, targetType, value);
                    castResult = new CastResult(castedValue, castFlag);
                }
                else
                {
                    // Implicit cast operators have priority in favour of explicit operators
                    // since they should not lose precision. See C# language specification: 
                    // https://msdn.microsoft.com/en-us/library/z5z9kes2.aspx
                    var conversionMethods =
                        GetCastOperatorMethods(sourceType, targetType)
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
                castResult = new CastResult(ConversionNotSupportedException.Create(sourceType, targetType, ex.InnerException), castFlag);
            }

            if (castResult == null)
            {
                castResult = new CastResult(ConversionNotSupportedException.Create(sourceType, targetType), castFlag);
            }

            return castResult;
        }

        /// <summary>
        ///     This methods returns a list of cast operation methods (implicit as well as explicit operators).
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

                    if (parameters.Length == 1
                        && (parameters[0].ParameterType.GetTypeInfo().IsAssignableFrom(sourceType.GetTypeInfo()) || sourceType.GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType.GetTypeInfo())))
                    {
                        yield return mi;
                    }
                }
            }
        }

        private static object GenericCast<T>(Expression<Func<T>> expression, Type sourceType, Type targetType, object value)
        {
            var genericCastMethodDefinition = ReflectionHelper.GetMethod(expression).GetGenericMethodDefinition();
            var genericCastMethod = genericCastMethodDefinition.MakeGenericMethod(sourceType, targetType);
            var castedValue = genericCastMethod.Invoke(null, new[] { value });

            return castedValue;
        }

        private static TTo AttemptExplicitCast<TFrom, TTo>(TFrom value)
        {
            // based on the IL generated from
            //var x = (TTo)(dynamic)value;

            var binder = CSharpBinder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(TTo), typeof(TypeHelper));
            var callSite = CallSite<Func<CallSite, TFrom, TTo>>.Create(binder);
            //dynamic tDynCallSite = callSite;
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
    }
}