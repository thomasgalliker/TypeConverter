namespace TypeConverter
{
    public interface IConverterCache
    {
        /// <summary>
        ///     Enables or disables the caching functionality.
        /// </summary>
        bool IsCacheEnabled { get; set; }

        /// <summary>
        ///     Enables or disables to cache size limit.
        /// </summary>
        bool IsMaxCacheSizeEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the cache size limit.
        /// </summary>
        int MaxCacheSize { get; set; }

        /// <summary>
        ///     Flushes the cache.
        /// </summary>
        void Reset();
    }
}