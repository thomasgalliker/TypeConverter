using System;

namespace TypeConverter
{
    /// <summary>
    ///     The ConverterRegistry is the entrypoint of TypeConverter which allows to register converters
    ///     and convert values of a given source type to an expected target type.
    /// </summary>
    public interface IConverterRegistry : IConverter
    {
        /// <summary>
        ///     Registers a converter factory which converts between generic types TSource and TTarget.
        /// </summary>
        /// <typeparam name="TSource">Generic source type.</typeparam>
        /// <typeparam name="TTarget">Generic target type.</typeparam>
        /// <param name="converterFactory">The factory which creates the converter to convert between source and target type.</param>
        void RegisterConverter<TSource, TTarget>(Func<IConvertable<TSource, TTarget>> converterFactory);

        /// <summary>
        ///     Registers a converter (as a type) which converts between generic types TSource and TTarget.
        /// </summary>
        /// <typeparam name="TSource">Generic source type.</typeparam>
        /// <typeparam name="TTarget">Generic target type.</typeparam>
        /// <param name="converterType">
        ///     The converter type which will be instanciated and used to convert between source and target
        ///     type.
        /// </param>
        void RegisterConverter<TSource, TTarget>(Type converterType);

        /// <summary>
        ///     Returns the registered converter which converts between generic types TSource and TTarget.
        /// </summary>
        /// <typeparam name="TSource">Generic source type.</typeparam>
        /// <typeparam name="TTarget">Generic target type.</typeparam>
        IConvertable<TSource, TTarget> GetConverterForType<TSource, TTarget>();

        /// <summary>
        ///     Clears all registered IConverters
        ///     and purges the cache.
        /// </summary>
        void Reset();
    }
}