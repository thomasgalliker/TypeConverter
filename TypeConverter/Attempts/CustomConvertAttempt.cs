using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TypeConverter.Utils;

namespace TypeConverter.Attempts
{
    // Attempt 1: Try to convert using registered converter
    // Having TryConvertGenericallyUsingConverterStrategy as a first attempt, the user of this library has the chance
    // to influence the conversion process with first priority.
    internal class CustomConvertAttempt : IConversionAttempt
    {
        private readonly Dictionary<Tuple<Type, Type>, Func<IConvertable>> converters;

        public CustomConvertAttempt()
        {
            this.converters = new Dictionary<Tuple<Type, Type>, Func<IConvertable>>();
        }

        internal void RegisterConverter(IConvertable converter)
        {
            lock (this.converters)
            {
                var convertables = converter.GetType().GetTypeInfo().ImplementedInterfaces.Where(i =>
                    i.GetTypeInfo().IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IConvertable<,>));

                foreach (var convertable in convertables)
                {
                    var sourceType = convertable.GenericTypeArguments[0];
                    var targetType = convertable.GenericTypeArguments[1];
                    this.converters.Add(new Tuple<Type, Type>(sourceType, targetType), () => converter);
                }
            }
        }

        internal void RegisterConverter<TSource, TTarget>(Func<IConvertable<TSource, TTarget>> converterFactory)
        {
            lock (this.converters)
            {
                this.converters.Add(new Tuple<Type, Type>(typeof(TSource), typeof(TTarget)), converterFactory);
            }
        }

        public ConversionResult TryConvert(object value, Type sourceType, Type targetType)
        {
            if (sourceType.GetTypeInfo().ContainsGenericParameters || targetType.GetTypeInfo().ContainsGenericParameters)
            {
                // Cannot deal with open generics, like IGenericOperators<>
                return null;
            }

            // Call generic method GetConverterForType to retrieve generic IConverter<TSource, TTarget>
            var getConverterForTypeMethod = ReflectionHelper.GetMethod(() => this.GetConverterForType<object, object>()).GetGenericMethodDefinition();
            var genericGetConverterForTypeMethod = getConverterForTypeMethod.MakeGenericMethod(sourceType, targetType);

            var genericConverter = genericGetConverterForTypeMethod.Invoke(this, null);
            if (genericConverter == null)
            {
                return null;
            }

            var matchingConverterInterface = genericConverter.GetType().GetTypeInfo().ImplementedInterfaces.SingleOrDefault(i => 
                        i.GenericTypeArguments.Length == 2 &&
                        i.GenericTypeArguments[0] == sourceType && 
                        i.GenericTypeArguments[1] == targetType);

            // Call Convert method on the particular interface
            var convertMethodGeneric = matchingConverterInterface.GetTypeInfo().GetDeclaredMethod("Convert");
            var convertedValue = convertMethodGeneric.Invoke(genericConverter, new[] { value });
            return new ConversionResult(convertedValue);
        }

        internal IConvertable<TSource, TTarget> GetConverterForType<TSource, TTarget>()
        {
            lock (this.converters)
            {
                var key = new Tuple<Type, Type>(typeof(TSource), typeof(TTarget));
                if (this.converters.ContainsKey(key))
                {
                    var converterFactory = this.converters[key];
                    return (IConvertable<TSource, TTarget>)converterFactory();
                }

                return null;
            }
        }

        internal void Reset()
        {
            lock (this.converters)
            {
                this.converters.Clear();
            }
        }
    }
}