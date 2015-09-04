
namespace TypeConverter.Converters
{
    public class DoubleTypeConverter : IConverter<object, double>
    {
        public double Convert(object value)
        {
            return double.Parse(value.ToString());
        }
    }
}