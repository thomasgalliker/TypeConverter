namespace TypeConverter.Converters
{
    public class StringToIntegerConverter : IConverter<string, int>, IConverter<int, string>
    {
        public int Convert(string value)
        {
            return int.Parse(value);
        }

        public string Convert(int value)
        {
            return value.ToString();
        }
    }
}