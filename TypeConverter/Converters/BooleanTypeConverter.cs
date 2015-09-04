namespace TypeConverter.Converters
{
    public class BooleanTypeConverter : IConverter<object, bool>
    {
        public bool Convert(object value)
        {
            return bool.Parse(value.ToString());
        }
    }
}