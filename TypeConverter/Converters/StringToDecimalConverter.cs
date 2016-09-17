using System.Globalization;

namespace TypeConverter.Converters
{
    public class StringToDecimalConverter : ToStringFormattableConvertable<decimal>, IConvertable<string, decimal>
    {
        protected override string Format { get { return "G"; } } // https://msdn.microsoft.com/en-us/library/dwhawy9k(v=vs.110).aspx#GFormatString

        public decimal Convert(string value)
        {
            return decimal.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}