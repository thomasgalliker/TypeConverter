namespace TypeConverter.Converters
{
    public class StringToIntegerConverter : IConvertable<string, int>, IConvertable<int, string>
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