using System.Globalization;

namespace TypeConverter.Converters
{
    public class StringToFloatConverter : ToStringFormattableConvertable<float>, IConvertable<string, float>
    {
        protected override string Format { get { return "R"; } } // https://msdn.microsoft.com/en-us/library/dwhawy9k(v=vs.110).aspx#RFormatString

        public float Convert(string value)
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}