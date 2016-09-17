using System.Globalization;

namespace TypeConverter.Converters
{
    public class StringToDoubleConverter : IConvertable<string, double>, IConvertable<double, string>
    {
        public double Convert(string value)
        {
            return double.Parse(value);
        }

        public string Convert(double value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}