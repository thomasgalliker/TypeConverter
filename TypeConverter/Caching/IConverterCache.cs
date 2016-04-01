namespace TypeConverter.Caching
{
    public interface IConverterCache
    {
        /// <summary>
        /// Enables or disables the caching functionality.
        /// </summary>
        bool IsCacheEnabled { get; set; }

        /// <summary>
        /// Flushes the cache.
        /// </summary>
        void Reset();
    }
}