using System;
using System.Globalization;

namespace TypeConverter.Converters
{
    public abstract class ToStringFormattableConvertable<TSource> : IConvertable<TSource, string>
        where TSource : IFormattable
    {
        protected abstract string Format { get; }

        public string Convert(TSource value)
        {
            return value.ToString(this.Format, CultureInfo.InvariantCulture);
        }
    }
}