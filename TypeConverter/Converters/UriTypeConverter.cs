using System;

namespace TypeConverter.Converters
{
    public class UriTypeConverter : IConverter<object, Uri>, IConverter<Uri, string>
    {
        public Uri Convert(object value)
        {
            return new Uri(value.ToString());
        }

        public string Convert(Uri value)
        {
            return value.AbsolutePath;
        }
    }
}