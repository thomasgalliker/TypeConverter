using System;

namespace TypeConverter.Converters
{
    public class StringToGuidConverter : ToStringFormattableConvertable<Guid>, IConvertable<string, Guid>
    {
        protected override string Format { get { return "B"; } } // https://msdn.microsoft.com/de-de/library/s6tk2z69(v=vs.110).aspx

        public Guid Convert(string value)
        {
            return new Guid(value);
        }
    }
}