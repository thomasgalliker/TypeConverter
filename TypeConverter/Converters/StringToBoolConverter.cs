namespace TypeConverter.Converters
{
    public class StringToBoolConverter : IConverter<string, bool>, IConverter<bool, string>
    {
        public bool Convert(string value)
        {
            return bool.Parse(value);
        }

        public string Convert(bool value)
        {
            return value.ToString();
        }
    }
}