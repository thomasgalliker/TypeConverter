
namespace TypeConverter.Converters
{
    public class StringToDoubleConverter : IConverter<string, double>, IConverter<double, string>
    {
        public double Convert(string value)
        {
            return double.Parse(value);
        }

        public string Convert(double value)
        {
            return value.ToString();
        }
    }
}