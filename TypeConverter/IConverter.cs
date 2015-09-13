
namespace TypeConverter
{
    public interface IConverter<in TSource, out TTarget> : IConverter
    {
        ////bool CanConvert(TSource value, TTarget target);
        
        /// <summary>
        /// Converts the given value of type TSource into an object of type TTarget.
        /// </summary>
        /// <param name="value">The source value to be converted.</param>
        TTarget Convert(TSource value);
    }

    public interface IConverter
    {
    }
}
