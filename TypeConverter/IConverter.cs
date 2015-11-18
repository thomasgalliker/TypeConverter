
using System;

namespace TypeConverter
{
    /// <summary>
    /// IConverter interface allows to perform conversion operations
    /// using all configured conversion strategies.
    /// Use <see cref="IConverterRegistry"/> to configure conversion strategies.
    /// </summary>
    public interface IConverter
    {
        /// <summary>
        ///     Converts the given value into an object of type TTarget.
        /// </summary>
        /// <typeparam name="TTarget">Generic target type.</typeparam>
        /// <param name="value">The source value to be converted.</param>
        TTarget Convert<TTarget>(object value);

        /// <summary>
        ///     Converts the given value into an object of type TTarget.
        /// </summary>
        /// <typeparam name="TSource">Generic source type.</typeparam>
        /// <typeparam name="TTarget">Generic target type.</typeparam>
        /// <param name="value">The source value to be converted.</param>
        TTarget Convert<TSource, TTarget>(TSource value);

        /// <summary>
        ///     Converts the given value into an object of type TTarget.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <typeparam name="TSource">Generic source type.</typeparam>
        /// <param name="value">The source value to be converted.</param>
        object Convert<TSource>(Type targetType, TSource value);

        /// <summary>
        ///     Converts the given value into an object of type TTarget.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="value">The source value to be converted.</param>
        object Convert(Type sourceType, Type targetType, object value);

        /// <summary>
        ///     Tries to convert the given value into an object of type TTarget.
        /// </summary>
        /// <typeparam name="TTarget">Generic target type.</typeparam>
        /// <param name="value">The source value to be converted.</param>
        /// <param name="defaultReturnValue">The default return value if the conversion failed.</param>
        TTarget TryConvert<TTarget>(object value, TTarget defaultReturnValue = default(TTarget));

        /// <summary>
        ///     Tries to convert the given value into an object of type TTarget.
        /// </summary>
        /// <typeparam name="TSource">Generic source type.</typeparam>
        /// <typeparam name="TTarget">Generic target type.</typeparam>
        /// <param name="value">The source value to be converted.</param>
        /// <param name="defaultReturnValue">The default return value if the conversion failed.</param>
        TTarget TryConvert<TSource, TTarget>(TSource value, TTarget defaultReturnValue = default(TTarget));

        /// <summary>
        ///     Tries to convert the given value into an object of type TTarget.
        /// </summary>
        /// <typeparam name="TSource">Generic source type.</typeparam>
        /// <param name="targetType">The target type.</param>
        /// <param name="value">The source value to be converted.</param>
        /// <param name="defaultReturnValue">The default return value if the conversion failed.</param>
        object TryConvert<TSource>(Type targetType, TSource value, object defaultReturnValue);

        /// <summary>
        ///     Tries to convert the given value into an object of type TTarget.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="value">The source value to be converted.</param>
        /// <param name="defaultReturnValue">The default return value if the conversion failed.</param>
        object TryConvert(Type sourceType, Type targetType, object value, object defaultReturnValue);
    }
}
