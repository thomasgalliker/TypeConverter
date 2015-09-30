using System;

namespace TypeConverter.Converters
{
    public class StringToUriConverter : IConvertable<string, Uri>, IConvertable<Uri, string>
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