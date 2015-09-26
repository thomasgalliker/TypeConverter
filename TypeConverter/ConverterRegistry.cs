using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Guards;

using TypeConverter.Exceptions;
using TypeConverter.Utils;

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
            Guard.ArgumentNotNull(() => converterFactory);

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
            Guard.ArgumentNotNull(() => converterType);

            if (typeof(IConverter<TSource, TTarget>).GetTypeInfo().IsAssignableFrom(converterType.GetTypeInfo()))
            {
                try
                {
                    return (IConverter<TSource, TTarget>)Activator.CreateInstance(converterType);
                }
                catch (Exception ex)
                {
                    ////LogLog.Error(DeclaringType, "Cannot CreateConverterInstance of type [" + converterType.FullName + "], Exception in call to Activator.CreateInstance", ex);
                    throw;
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
            Guard.ArgumentNotNull(() => value);

            return (TTarget)this.DoConvert(value.GetType(), typeof(TTarget), value);
        }

        /// <inheritdoc />
        public TTarget TryConvert<TTarget>(object value, TTarget defaultReturnValue = default(TTarget))
        {

            return (TTarget)this.DoConvert(value.GetType(), typeof(TTarget), value, defaultReturnValue, throwIfConvertFails: false);
        }

        /// <inheritdoc />
        public TTarget Convert<TSource, TTarget>(TSource value)
        {
            return (TTarget)this.DoConvert(typeof(TSource), typeof(TTarget), value);
        }

        /// <inheritdoc />
        public TTarget TryConvert<TSource, TTarget>(TSource value, TTarget defaultReturnValue = default(TTarget))
        {
            return (TTarget)this.DoConvert(typeof(TSource), typeof(TTarget), value, defaultReturnValue, throwIfConvertFails: false);
        }

        /// <inheritdoc />
        public object Convert<TSource>(Type targetType, TSource value)
        {
            return this.DoConvert(typeof(TSource), targetType, value);
        }

        /// <inheritdoc />
        public object TryConvert<TSource>(Type targetType, TSource value)
        {
            return this.TryConvert(targetType, value, null);
        }

        /// <inheritdoc />
        public object TryConvert<TSource>(Type targetType, TSource value, object defaultReturnValue)
        {
            return this.DoConvert(typeof(TSource), targetType, value, defaultReturnValue, throwIfConvertFails: false);
        }

        /// <inheritdoc />
        public object Convert(Type sourceType, Type targetType, object value)
        {
            return this.DoConvert(sourceType, targetType, value);
        }

        /// <inheritdoc />
        public object TryConvert(Type sourceType, Type targetType, object value)
        {
            return this.TryConvert(sourceType, targetType, value, null);
        }

        /// <inheritdoc />
        public object TryConvert(Type sourceType, Type targetType, object value, object defaultReturnValue)
        {
            return this.DoConvert(sourceType, targetType, value, defaultReturnValue, throwIfConvertFails: false);
        }

        private object DoConvert(Type sourceType, Type targetType, object value, object defaultReturnValue = null, bool throwIfConvertFails = true)
        {
            Guard.ArgumentNotNull(() => value);
            Guard.ArgumentNotNull(() => sourceType);
            Guard.ArgumentNotNull(() => targetType);

            // Attempt 1: Try to convert using registered converter
            // Having TryConvertGenericallyUsingConverterStrategy as a first attempt, the user of this library has the chance
            // to influence the conversion process with first priority.
            var convertedValue = this.TryConvertGenericallyUsingConverterStrategy(sourceType, targetType, value);
            if (convertedValue != null)
            {
                return convertedValue;
            }

            // Attempt 2: Is source type same as target type
            if (sourceType == targetType)
            {
                return value;
            }

            // Attempt 3: Use implicit or explicit casting if supported
            var castedValue = TypeHelper.CastTo(value, targetType);
            if (castedValue != null && castedValue.IsSuccessful)
            {
                return castedValue.Value;
            }

            // Attempt 4: Try to convert generic enum
            var convertedEnum = this.TryConvertEnumGenerically(sourceType, targetType, value);
            if (convertedEnum != null)
            {
                return convertedEnum;
            }
            
            // Attempt 5: We essentially make a guess that to convert from a string
            // to an arbitrary type T there will be a static method defined on type T called Parse
            // that will take an argument of type string. i.e. T.Parse(string)->T we call this
            // method to convert the string to the type required by the property.
            var parsedValue = this.TryParseGenerically(sourceType, targetType, value);
            if (parsedValue != null)
            {
                return parsedValue;
            }

            // If all fails, we either throw an exception or return a default target value
            if (throwIfConvertFails)
            {
                throw ConversionNotSupportedException.Create(sourceType, targetType, value);
            }
            ////if (defaultReturnValue == null && targetType.GetTypeInfo().IsValueType)
            ////{
            ////    return 
            ////}

            return defaultReturnValue;
        }

        private object TryConvertGenericallyUsingConverterStrategy(Type sourceType, Type targetType, object value)
        {
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