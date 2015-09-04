using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TypeConverter.Converters;
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

        public void RegisterConverter<TSource, TTarget>(Func<IConverter<TSource, TTarget>> converterFactory)
        {
            lock (this.converters)
            {
                this.converters.Add(new Tuple<Type, Type>(typeof(TSource), typeof(TTarget)), converterFactory);
            }
        }

        public void RegisterConverter<TSource, TTarget>(Type converterType)
        {
            this.RegisterConverter(() => this.CreateConverterInstance<TSource, TTarget>(converterType));
        }

        private IConverter<TSource, TTarget> CreateConverterInstance<TSource, TTarget>(Type converterType)
        {
            if (converterType == null)
            {
                throw new ArgumentNullException("converterType", "CreateConverterInstance cannot create instance, converterType is null");
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

        public object Convert(Type sourceType, Type targetType, object value)
        {
            var convertedValue = this.TryConvertGenerically(sourceType, targetType, value);
            if (convertedValue != null)
            {
                return convertedValue;
            }
            else
            {
                if (targetType.GetTypeInfo().IsEnum)
                {
                    return Enum.Parse(targetType, value.ToString(), true);
                }
                else
                {
                    // We essentially make a guess that to convert from a string
                    // to an arbitrary type T there will be a static method defined on type T called Parse
                    // that will take an argument of type string. i.e. T.Parse(string)->T we call this
                    // method to convert the string to the type required by the property.
                    MethodInfo parseMethod = targetType.GetRuntimeMethod("Parse", new[] { typeof(string) });
                    if (parseMethod != null)
                    {
                        // Call the Parse method
                        return parseMethod.Invoke(value, new object[] { });
                    }
                }
            }

            return null;
        }

        public TTarget Convert<TSource, TTarget>(TSource value)
        {
            var converter = this.GetConverterForType<TSource, TTarget>();
            return converter.Convert(value);
        }

        private object TryConvertGenerically(Type sourceType, Type targetType, object value)
        {
            // Call generic method GetConverterForType to retrieve generic IConverter<TSource, TTarget>
            MethodInfo getConverterForTypeMethod = this.GetType().GetTypeInfo().GetDeclaredMethod("GetConverterForType");
            MethodInfo genericGetConverterForTypeMethod = getConverterForTypeMethod.MakeGenericMethod(sourceType, targetType);

            object genericConverters = genericGetConverterForTypeMethod.Invoke(this, null);
            if (genericConverters == null)
            {
                throw new ConversionNotSupportedException(
                    string.Format("Could not find IConverter<{0}, {1}>. Use RegisterConverter method to register a converter which converts between type {0} and type {1}.",
                        sourceType,
                        targetType));
            }

            var matchingConverterInterface = genericConverters.GetType().GetTypeInfo().ImplementedInterfaces.SingleOrDefault(i =>
                i.GenericTypeArguments.Count() == 2 && 
                i.GenericTypeArguments[0] == sourceType && 
                i.GenericTypeArguments[1] == targetType);

            if (matchingConverterInterface == null)
            {
                throw new Exception();
            }

            // Call Convert method on the particular interface
            var convertMethodGeneric = matchingConverterInterface.GetTypeInfo().GetDeclaredMethod("Convert");
            var convertedValue = convertMethodGeneric.Invoke(genericConverters, new[] { value });
            return convertedValue;
        }

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

        ////public ITypeConverter GetConverterForType(Type targetType)
        ////{
        ////    lock (this.converters)
        ////    {
        ////        // Lookup in the registry
        ////        if (this.converters.ContainsKey(targetType))
        ////        {
        ////            var converter = this.typeConverters[targetType]() as ITypeConverter;
        ////            if (converter == null)
        ////            {
        ////                // Lookup using attributes
        ////                converter = this.CreateConverterInstance(targetType) as ITypeConverter;
        ////                if (converter != null)
        ////                {
        ////                    // Store in registry
        ////                    this.typeConverters[targetType] = () => converter;
        ////                }
        ////            }

        ////            return converter;
        ////        }

        ////        return null;
        ////    }
        ////}

        public void Reset()
        {
            lock (this.converters)
            {
                this.converters.Clear();
            }
        }
    }
}