using System.Diagnostics;
using TypeConverter.Attempts;

namespace TypeConverter.Caching
{
    [DebuggerDisplay("IsCached = {IsCached}, IsConvertable = {IsConvertable}, ConversionAttempt = {ConversionAttempt}")]
    internal class CacheResult
    {
        public CacheResult(bool isCached)
        {
            this.IsCached = isCached;
        }

        public CacheResult(bool isCached, bool isConvertable, IConversionAttempt conversionAttempt)
            : this(isCached)
        {
            this.IsConvertable = isConvertable;
            this.ConversionAttempt = conversionAttempt;
        }

        internal bool IsCached { get; private set; }

        internal bool IsConvertable { get; private set; }

        internal IConversionAttempt ConversionAttempt { get; private set; }
    }
}