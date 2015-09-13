using System;

namespace TypeConverter.Converters
{
    public class StringToUriConverter : IConverter<string, Uri>, IConverter<Uri, string>
    {
        public Uri Convert(string value)
        {
            return new Uri(value);
        }

        public string Convert(Uri value)
        {
            return value.AbsoluteUri;
        }
    }
}