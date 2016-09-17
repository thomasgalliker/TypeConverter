using System;
using System.Globalization;

namespace TypeConverter.Converters
{
    public class StringToDateTimeConverter : ToStringFormattableConvertable<DateTime>, IConvertable<string, DateTime>
    {
        protected override string Format { get { return "O"; } } // https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Roundtrip

        public DateTime Convert(string value)
        {
            return DateTime.ParseExact(value, this.Format, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }
    }
}