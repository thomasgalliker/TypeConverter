namespace TypeConverter.Converters
{
    public class StringTypeConverter : IConverter<object, string>
    {
        public string Convert(object value)
        {
            return value.ToString();
        }
    }
}