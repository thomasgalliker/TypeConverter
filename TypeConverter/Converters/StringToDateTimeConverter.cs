using System;
using System.Globalization;

namespace TypeConverter.Converters
{
    public class StringToDateTimeConverter : IConvertable<string, DateTime>, IConvertable<DateTime, string>
    {
        private const string DateTimeFormat = "O";

        public DateTime Convert(string value)
        {
            return DateTime.ParseExact(value, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        public string Convert(DateTime value)
        {
            return value.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
        }
    }
}