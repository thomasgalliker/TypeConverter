using System.Globalization;

namespace TypeConverter.Converters
{
    public class StringToDoubleConverter : ToStringFormattableConvertable<double>, IConvertable<string, double>
    {
        protected override string Format { get { return "R"; } } // https://msdn.microsoft.com/en-us/library/dwhawy9k(v=vs.110).aspx#RFormatString

        public double Convert(string value)
        {
            return double.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}