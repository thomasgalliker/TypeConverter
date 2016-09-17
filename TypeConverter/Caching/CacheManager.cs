using System;
using System.Collections.Generic;
using System.Linq;

using TypeConverter.Attempts;

namespace TypeConverter.Caching
{
    /// <summary>
    ///     CacheManager is an internal data structure which stores conversion strategies
    ///     for type-to-type mappings.
    /// </summary>
    internal class CacheManager
    {
        private Dictionary<KeyValuePair<Type, Type>, WeightedCacheResult> cache = new Dictionary<KeyValuePair<Type, Type>, WeightedCacheResult>();
        private readonly object syncObj = new object();
        private bool isMaxCacheSizeEnabled;

        public CacheManager()
        {
            this.IsCacheEnabled = false;
            this.MaxCacheSize = 5000;
            this.IsMaxCacheSizeEnabled = false;
        }

        internal bool IsCacheEnabled { get; set; }

        internal bool IsMaxCacheSizeEnabled
        {
            get
            {
                return this.isMaxCacheSizeEnabled;
            }
            set
            {
                if (this.isMaxCacheSizeEnabled != value)
                {
                    this.isMaxCacheSizeEnabled = value;
                    this.ReduceCacheSize(this.MaxCacheSize);
                }
            }
        }

        internal int MaxCacheSize { get; set; }

        private void ReduceCacheSize(int newCacheSize)
        {
            lock (this.syncObj)
            {
                if (this.IsMaxCacheSizeEnabled && this.cache.Count >= this.MaxCacheSize)
                {
                    // Take the top-weighted cache items according to ReadAccessCount
                    this.cache = this.cache.OrderByDescending(x => x.Value.ReadAccessCount).Take(newCacheSize).ToDictionary(s => s.Key, s => s.Value);
                }
            }
        }

        internal void UpdateCache(Type sourceType, Type targetType, bool isConvertable, IConversionAttempt conversionAttempt)
        {
            if (this.IsCacheEnabled == false)
            {
                return;
            }

            lock (this.syncObj)
            {
                this.ReduceCacheSize(this.MaxCacheSize - 1); // -1 because we are about to insert a new item

                var key = new KeyValuePair<Type, Type>(sourceType, targetType);
                var value = new WeightedCacheResult(isCached: true, isConvertable: isConvertable, conversionAttempt: conversionAttempt);

                this.cache[key] = value;
            }
        }

        /// <summary>
        ///     Tries to get a CacheResult from given {sourceType, targetType} mapping.
        /// </summary>
        /// <returns>A CacheResult which indicates if and how the given  {sourceType, targetType} mapping can be converted.</returns>
        internal CacheResult TryGetCachedValue(Type sourceType, Type targetType)
        {
            if (this.IsCacheEnabled == false)
            {
                return new CacheResult(isCached: false);
            }

            lock (this.syncObj)
            {
                WeightedCacheResult cacheResult = null;
                var key = new KeyValuePair<Type, Type>(sourceType, targetType);
                this.cache.TryGetValue(key, out cacheResult);

                if (cacheResult != null)
                {
                    cacheResult.ReadAccessCount++;
                    return cacheResult;
                }

                return new CacheResult(isCached: false);
            }
        }

        public void Reset()
        {
            lock (this.syncObj)
            {
                this.cache.Clear();
            }
        }

        private class WeightedCacheResult : CacheResult
        {
            public WeightedCacheResult(bool isCached)
                : base(isCached)
            {
            }

            public WeightedCacheResult(bool isCached, bool isConvertable, IConversionAttempt conversionAttempt)
                : base(isCached, isConvertable, conversionAttempt)
            {
            }

            public int ReadAccessCount { get; set; }
        }
    }
}