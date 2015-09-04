namespace TypeConverter.Converters
{
    public class IntegerTypeConverter : IConverter<object, int>
    {
        public int Convert(object value)
        {
            return int.Parse(value.ToString());
        }
    }
}