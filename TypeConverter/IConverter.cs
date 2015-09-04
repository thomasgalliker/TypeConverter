
namespace TypeConverter
{
    public interface IConverter<in TSource, out TTarget> : IConverter
    {
        ////bool CanConvert(TSource value, TTarget target);
        /// 
        TTarget Convert(TSource value);
    }

    public interface IConverter
    {
    }
}
