namespace TypeConverter.Converters
{
    public class StringToBoolConverter : IConvertable<string, bool>, IConvertable<bool, string>
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