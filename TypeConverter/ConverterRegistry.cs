using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Guards;

using TypeConverter.Attempts;
using TypeConverter.Caching;
using TypeConverter.Exceptions;
using TypeConverter.Extensions;
using TypeConverter.Utils;

namespace TypeConverter
{
    public class ConverterRegistry : IConverterRegistry, IConverterCache
    {
        private readonly CacheManager cacheManager;
        private readonly IList<IConversionAttempt> conversionAttempts;

        public ConverterRegistry()
        {
            this.conversionAttempts = new List<IConversionAttempt>
            {
                new CustomConvertAttempt(),
                new CastAttempt(),
                new ChangeTypeAttempt(),
                new EnumParseAttempt(),
                new StringParseAttempt()
            };

            this.cacheManager = new CacheManager();
            this.cacheManager.IsCacheEnabled = true;
        }

        /// <inheritdoc />
        public void RegisterConverter<TSource, TTarget>(Func<IConvertable<TSource, TTarget>> converterFactory)
        {
            Guard.ArgumentNotNull(() => converterFactory);

            var customConvertStrategy = this.conversionAttempts.Single(a => a.GetType() == typeof(CustomConvertAttempt));
            ((CustomConvertAttempt)customConvertStrategy).RegisterConverter(converterFactory);
        }

        /// <inheritdoc />
        public void RegisterConverter<TSource, TTarget>(Type converterType)
        {
            this.RegisterConverter(() => this.CreateConverterInstance<TSource, TTarget>(converterType));
        }

        private IConvertable<TSource, TTarget> CreateConverterInstance<TSource, TTarget>(Type converterType)
        {
            Guard.ArgumentNotNull(() => converterType);

            if (typeof(IConvertable<TSource, TTarget>).GetTypeInfo().IsAssignableFrom(converterType.GetTypeInfo()))
            {
                return (IConvertable<TSource, TTarget>)Activator.CreateInstance(converterType);
            }

            return null;
        }

        /// <inheritdoc />
        public TTarget Convert<TTarget>(object value)
        {
            Guard.ArgumentNotNull(() => value);

            return (TTarget)this.ConvertInternal(value.GetType(), typeof(TTarget), value);
        }

        /// <inheritdoc />
        public TTarget TryConvert<TTarget>(object value, TTarget defaultReturnValue = default(TTarget))
        {
            return (TTarget)this.ConvertInternal(value.GetType(), typeof(TTarget), value, defaultReturnValue, throwIfConvertFails: false);
        }

        /// <inheritdoc />
        public TTarget Convert<TSource, TTarget>(TSource value)
        {
            return (TTarget)this.ConvertInternal(typeof(TSource), typeof(TTarget), value);
        }

        /// <inheritdoc />
        public TTarget TryConvert<TSource, TTarget>(TSource value, TTarget defaultReturnValue = default(TTarget))
        {
            return (TTarget)this.ConvertInternal(typeof(TSource), typeof(TTarget), value, defaultReturnValue, throwIfConvertFails: false);
        }

        /// <inheritdoc />
        public object Convert<TSource>(Type targetType, TSource value)
        {
            return this.ConvertInternal(typeof(TSource), targetType, value);
        }

        /// <inheritdoc />
        public object TryConvert<TSource>(Type targetType, TSource value, object defaultReturnValue)
        {
            return this.ConvertInternal(typeof(TSource), targetType, value, defaultReturnValue, throwIfConvertFails: false);
        }

        /// <inheritdoc />
        public object Convert(Type sourceType, Type targetType, object value)
        {
            return this.ConvertInternal(sourceType, targetType, value);
        }

        /// <inheritdoc />
        public object TryConvert(Type sourceType, Type targetType, object value, object defaultReturnValue)
        {
            return this.ConvertInternal(sourceType, targetType, value, defaultReturnValue, throwIfConvertFails: false);
        }

        private object ConvertInternal(Type sourceType, Type targetType, object value, object defaultReturnValue = null, bool throwIfConvertFails = true)
        {
            Guard.ArgumentNotNull(() => value);
            Guard.ArgumentNotNull(() => sourceType);
            Guard.ArgumentNotNull(() => targetType);

            // Try to read conversion method from cache
            var cachedValue = this.TryGetCachedValue(value, sourceType, targetType);
            if (cachedValue != null && cachedValue.IsSuccessful)
            {
                return cachedValue.Value;
            }

            // Try to convert using defined sequence of conversion attempts
            foreach (var conversionAttempt in this.conversionAttempts)
            {
                var conversionResult = conversionAttempt.TryConvert(value, sourceType, targetType);
                if (conversionResult != null && conversionResult.IsSuccessful)
                {
                    this.cacheManager.UpdateCache(
                        sourceType: sourceType,
                        targetType: targetType, 
                        isConvertable: conversionResult.IsSuccessful, 
                        conversionAttempt: conversionAttempt);

                    return conversionResult.Value;
                }
            }

            // If all fails, we either throw an exception
            if (throwIfConvertFails)
            {
                throw ConversionNotSupportedException.Create(sourceType, targetType);
            }

            // ...or return a default target value
            if (defaultReturnValue == null)
            {
                return targetType.GetDefault();
            }

            return defaultReturnValue;
        }

        private ConversionResult TryGetCachedValue(object value, Type sourceType, Type targetType)
        {
            var cacheResult = this.cacheManager.TryGetCachedValue(sourceType, targetType);
            if (cacheResult.IsCached)
            {
                if (cacheResult.IsConvertable)
                {
                    var cachedAttempt = this.conversionAttempts.Single(a => a == cacheResult.ConversionAttempt);
                    var convertedValue = cachedAttempt.TryConvert(value, sourceType, targetType);
                    return convertedValue;
                }

                return new ConversionResult(ConversionNotSupportedException.Create(sourceType, targetType));
            }

            return null;
        }

        #region IConverterRegistry facade implementation
        /// <inheritdoc />
        void IConverterRegistry.Reset()
        {
            var customConvertStrategy = this.conversionAttempts.Single(a => a.GetType() == typeof(CustomConvertAttempt));
            ((CustomConvertAttempt)customConvertStrategy).Reset();
        }

        /// <inheritdoc />
        bool IConverterCache.IsCacheEnabled
        {
            get
            {
                return this.cacheManager.IsCacheEnabled;
            }
            set
            {
                this.cacheManager.IsCacheEnabled = value;
            }
        }

        /// <inheritdoc />
        bool IConverterCache.IsMaxCacheSizeEnabled
        {
            get
            {
                return this.cacheManager.IsMaxCacheSizeEnabled;
            }
            set
            {
                this.cacheManager.IsMaxCacheSizeEnabled = value;
            }
        }

        /// <inheritdoc />
        public int MaxCacheSize
        {
            get
            {
                return this.cacheManager.MaxCacheSize;
            }
            set
            {
                this.cacheManager.MaxCacheSize = value;
            }
        }

        /// <inheritdoc />
        void IConverterCache.Reset()
        {
            this.cacheManager.Reset();
        }
        #endregion
    }
}