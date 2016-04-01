using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using TypeConverter.Attempts;

namespace TypeConverter.Caching
{
    internal class CacheManager
    {
        private readonly Dictionary<KeyValuePair<Type, Type>, Tuple<bool, IConversionAttempt>> cache = new Dictionary<KeyValuePair<Type, Type>, Tuple<bool, IConversionAttempt>>();
        private readonly object syncObj = new object();

        ////private const int MaxCacheSize = 5000; // TODO: Implement read-weighted buffer system

        public CacheManager()
        {
            this.IsCacheEnabled = false;
        }

        internal bool IsCacheEnabled { get; set; }

        internal void UpdateCache(Type sourceType, Type targetType, bool isConvertable, IConversionAttempt conversionAttempt)
        {
            if (this.IsCacheEnabled == false)
            {
                return;
            }

            lock (this.syncObj)
            {
                ////if (this.cache.Count > MaxCacheSize)
                ////{
                ////    this.cache.Clear();
                ////}

                var key = new KeyValuePair<Type, Type>(sourceType, targetType);
                var value = new Tuple<bool, IConversionAttempt>(isConvertable, conversionAttempt);
                this.cache[key] = value;
            }
        }

        internal CacheResult TryGetCachedValue(Type sourceType, Type targetType)
        {
            if (this.IsCacheEnabled == false)
            {
                return new CacheResult(isCached: false);
            }

            lock (this.syncObj)
            {
                Tuple<bool, IConversionAttempt> cachedValue = null;
                var key = new KeyValuePair<Type, Type>(sourceType, targetType);
                var isCached = this.cache.TryGetValue(key, out cachedValue);
                if (cachedValue == null)
                {
                    return new CacheResult(isCached: false);
                }
                return new CacheResult(
                    isCached: isCached, 
                    isConvertable: cachedValue.Item1,
                    conversionAttempt: cachedValue.Item2);
            }
        }

        public void Reset()
        {
            lock (this.syncObj)
            {
                this.cache.Clear();
            }
        }
    }
}