using System;
using System.Globalization;

namespace TypeConverter.Converters
{
    public class StringToDateTimeOffsetConverter : IConvertable<string, DateTimeOffset>, IConvertable<DateTimeOffset, string>
    {
        private const string DateTimeFormat = "O";

        public DateTimeOffset Convert(string value)
        {
            return DateTimeOffset.ParseExact(value, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        public string Convert(DateTimeOffset value)
        {
            return value.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
        }
    }
}