using System;

using TypeConverter.Converters;

namespace TypeConverter
{
    public interface IConverterRegistry
    {
        void RegisterConverter<TSource, TTarget>(Func<IConverter<TSource, TTarget>> converterFactory);

        void RegisterConverter<TSource, TTarget>(Type converterType);

        IConverter<TSource, TTarget> GetConverterForType<TSource, TTarget>();

        object Convert(Type sourceType, Type targetType, object value);

        TTarget Convert<TSource, TTarget>(TSource value);

        void Reset();
    }
}