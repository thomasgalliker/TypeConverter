using System;


namespace TypeConverter
{
    /// <summary>
    /// The ConverterRegistry is the entrypoint of TypeConverter which allows to register converters
    /// and convert values of a given source type to an expected target type.
    /// </summary>
    public interface IConverterRegistry
    {
        /// <summary>
        /// Registers a converter factory which converts between generic types TSource and TTarget.
        /// </summary>
        /// <typeparam name="TSource">Generic source type.</typeparam>
        /// <typeparam name="TTarget">Generic target type.</typeparam>
        /// <param name="converterFactory">The factory which creates the converter to convert between source and target type.</param>
        void RegisterConverter<TSource, TTarget>(Func<IConverter<TSource, TTarget>> converterFactory);

        /// <summary>
        /// Registers a converter (as a type) which converts between generic types TSource and TTarget.
        /// </summary>
        /// <typeparam name="TSource">Generic source type.</typeparam>
        /// <typeparam name="TTarget">Generic target type.</typeparam>
        /// <param name="converterType">The converter type which will be instanciated and used to convert between source and target type.</param>
        void RegisterConverter<TSource, TTarget>(Type converterType);

        /// <summary>
        /// Returns the registered converter which converts between generic types TSource and TTarget.
        /// </summary>
        /// <typeparam name="TSource">Generic source type.</typeparam>
        /// <typeparam name="TTarget">Generic target type.</typeparam>
        IConverter<TSource, TTarget> GetConverterForType<TSource, TTarget>();

        /// <summary>
        /// Converts the given value into an object of type TTarget.
        /// </summary>
        /// <typeparam name="TTarget">Generic target type.</typeparam>
        /// <param name="value">The source value to be converted.</param>
        TTarget Convert<TTarget>(object value);

        /// <summary>
        /// Converts the given value into an object of type TTarget.
        /// </summary>
        /// <typeparam name="TSource">Generic source type.</typeparam>
        /// <typeparam name="TTarget">Generic target type.</typeparam>
        /// <param name="value">The source value to be converted.</param>
        TTarget Convert<TSource, TTarget>(TSource value);

        /// <summary>
        /// Converts the given value into an object of type TTarget.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <param name="value">The source value to be converted.</param>
        object Convert<TSource>(Type targetType, TSource value);

        /// <summary>
        /// Converts the given value into an object of type TTarget.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="value">The source value to be converted.</param>
        object Convert(Type sourceType, Type targetType, object value);

        /// <summary>
        /// Resets all registrations.
        /// </summary>
        void Reset();
    }
}