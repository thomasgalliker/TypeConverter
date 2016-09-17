using System;
using System.Globalization;

namespace TypeConverter.Converters
{
    public class StringToDateTimeOffsetConverter : ToStringFormattableConvertable<DateTimeOffset>, IConvertable<string, DateTimeOffset>
    {
        protected override string Format { get { return "O"; } } // https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Roundtrip

        public DateTimeOffset Convert(string value)
        {
            return DateTimeOffset.ParseExact(value, this.Format, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }
    }
}