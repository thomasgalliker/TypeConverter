using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TypeConverter.Exceptions;

namespace TypeConverter
{
    public class ConverterRegistry : IConverterRegistry
    {
        private readonly Dictionary<Tuple<Type, Type>, Func<IConverter>> converters;

        public ConverterRegistry()
        {
            this.converters = new Dictionary<Tuple<Type, Type>, Func<IConverter>>();
        }

        /// <inheritdoc />
        public void RegisterConverter<TSource, TTarget>(Func<IConverter<TSource, TTarget>> converterFactory)
        {
            if (converterFactory == null)
            {
                throw new ArgumentNullException("converterFactory");
            }

            lock (this.converters)
            {
                this.converters.Add(new Tuple<Type, Type>(typeof(TSource), typeof(TTarget)), converterFactory);
            }
        }

        /// <inheritdoc />
        public void RegisterConverter<TSource, TTarget>(Type converterType)
        {
            this.RegisterConverter(() => this.CreateConverterInstance<TSource, TTarget>(converterType));
        }

        private IConverter<TSource, TTarget> CreateConverterInstance<TSource, TTarget>(Type converterType)
        {
            if (converterType == null)
            {
                throw new ArgumentNullException("converterType");
            }

            // Check type is a converter
            if (typeof(IConverter<TSource, TTarget>).GetTypeInfo().IsAssignableFrom(converterType.GetTypeInfo()))
            {
                try
                {
                    // Create the type converter
                    return (IConverter<TSource, TTarget>)Activator.CreateInstance(converterType);
                }
                catch (Exception ex)
                {
                    ////LogLog.Error(DeclaringType, "Cannot CreateConverterInstance of type [" + converterType.FullName + "], Exception in call to Activator.CreateInstance", ex);
                }
            }
            else
            {
                ////LogLog.Error(DeclaringType, "Cannot CreateConverterInstance of type [" + converterType.FullName + "], type does not implement ITypeConverter or IConvertTo");
            }
            return null;
        }

        /// <inheritdoc />
        public TTarget Convert<TTarget>(object value)
        {
            return (TTarget)this.Convert(value.GetType(), typeof(TTarget), value);
        }

        /// <inheritdoc />
        public TTarget Convert<TSource, TTarget>(TSource value)
        {
            return (TTarget)this.Convert(typeof(TSource), typeof(TTarget), value);
        }

        /// <inheritdoc />
        public object Convert<TSource>(Type targetType, TSource value)
        {
            return this.Convert(typeof(TSource), targetType, value);
        }

        /// <inheritdoc />
        public object Convert(Type sourceType, Type targetType, object value)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException("sourceType");
            }

            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            // Attempt 1: Is source type same as target type
            if (sourceType == targetType)
            {
                return value;
            }

            // Attempt 2: Try to convert using registered converter
            var convertedValue = this.TryConvertGenerically(sourceType, targetType, value);
            if (convertedValue != null)
            {
                return convertedValue;
            }

            // Attempt 3: Try to convert generic enum
            var convertedEnum = this.TryConvertEnumGenerically(sourceType, targetType, value);
            if (convertedEnum != null)
            {
                return convertedEnum;
            }
            
            // Attempt 4: We essentially make a guess that to convert from a string
            // to an arbitrary type T there will be a static method defined on type T called Parse
            // that will take an argument of type string. i.e. T.Parse(string)->T we call this
            // method to convert the string to the type required by the property.
            var parsedValue = this.TryParseGenerically(sourceType, targetType, value);
            if (parsedValue != null)
            {
                return parsedValue;
            }

            // If all fails, we throw an exception
            throw ConversionNotSupportedException.Create(sourceType, targetType, value);
        }

        private object TryConvertGenerically(Type sourceType, Type targetType, object value)
        {
            // Call generic method GetConverterForType to retrieve generic IConverter<TSource, TTarget>
            var getConverterForTypeMethod = this.GetType().GetTypeInfo().GetDeclaredMethod("GetConverterForType");
            var genericGetConverterForTypeMethod = getConverterForTypeMethod.MakeGenericMethod(sourceType, targetType);

            var genericConverter = genericGetConverterForTypeMethod.Invoke(this, null);
            if (genericConverter == null)
            {
                return null;
            }

            var matchingConverterInterface = genericConverter.GetType().GetTypeInfo().ImplementedInterfaces.SingleOrDefault(i =>
                i.GenericTypeArguments.Count() == 2 && 
                i.GenericTypeArguments[0] == sourceType && 
                i.GenericTypeArguments[1] == targetType);

            // Call Convert method on the particular interface
            var convertMethodGeneric = matchingConverterInterface.GetTypeInfo().GetDeclaredMethod("Convert");
            var convertedValue = convertMethodGeneric.Invoke(genericConverter, new[] { value });
            return convertedValue;
        }

        /// <inheritdoc />
        public IConverter<TSource, TTarget> GetConverterForType<TSource, TTarget>()
        {
            lock (this.converters)
            {
                var key = new Tuple<Type, Type>(typeof(TSource), typeof(TTarget));
                if (this.converters.ContainsKey(key))
                {
                    var converterFactory = this.converters[key];
                    return (IConverter<TSource, TTarget>)converterFactory();
                }

                return null;
            }
        }

        private object TryConvertEnumGenerically(Type sourceType, Type targetType, object value)
        {
            if (sourceType.GetTypeInfo().IsEnum)
            {
                return value.ToString();
            }

            if (targetType.GetTypeInfo().IsEnum)
            {
                return Enum.Parse(targetType, value.ToString(), true);
            }

            return null;
        }

        private object TryParseGenerically(Type sourceType, Type targetType, object value)
        {
            // Either of both, sourceType or targetType, need to be typeof(string)
            if (sourceType == typeof(string) && targetType != typeof(string))
            {
                var parseMethod = targetType.GetRuntimeMethod("Parse", new[] { sourceType });
                if (parseMethod != null)
                {

                    return parseMethod.Invoke(this, new[] { value });
                }
            }
            else if (targetType == typeof(string) && sourceType != typeof(string))
            {
                return value.ToString();
            }

            return null;
        }

        /// <inheritdoc />
        public void Reset()
        {
            lock (this.converters)
            {
                this.converters.Clear();
            }
        }
    }
}