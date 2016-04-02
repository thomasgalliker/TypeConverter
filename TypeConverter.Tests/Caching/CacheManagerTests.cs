using System;
using System.Collections.Generic;

using FluentAssertions;

using TypeConverter.Attempts;
using TypeConverter.Caching;

using Xunit;

namespace TypeConverter.Tests.Caching
{
    public class ConverterCacheTests
    {
        [Fact]
        public void ShouldNotGetCachedValueWhenEmpty()
        {
            // Arrange
            var cacheManager = new CacheManager();
            cacheManager.IsCacheEnabled = true;

            // Act
            var cacheResult = cacheManager.TryGetCachedValue(typeof(int), typeof(double));

            // Assert
            cacheResult.ConversionAttempt.Should().BeNull();
            cacheResult.IsCached.Should().BeFalse();
        }

        [Fact]
        public void ShouldGetCachedValue()
        {
            // Arrange
            var cacheManager = new CacheManager();
            cacheManager.IsCacheEnabled = true;

            // Act
            cacheManager.UpdateCache(typeof(int), typeof(double), isConvertable: true, conversionAttempt: new CastAttempt());

            // Assert
            var cacheResult = cacheManager.TryGetCachedValue(typeof(int), typeof(double));

            cacheResult.ConversionAttempt.Should().BeOfType(typeof(CastAttempt));
            cacheResult.IsCached.Should().BeTrue();
        }

        [Fact]
        public void ShouldNotGetCachedValueIfCachingIsDisabled()
        {
            // Arrange
            var cacheManager = new CacheManager();
            cacheManager.IsCacheEnabled = true;
            cacheManager.UpdateCache(typeof(int), typeof(double), isConvertable: true, conversionAttempt: new CastAttempt());
            cacheManager.IsCacheEnabled = false;

            // Act
            var cacheResult = cacheManager.TryGetCachedValue(typeof(int), typeof(double));

            // Assert
            cacheResult.ConversionAttempt.Should().BeNull();
            cacheResult.IsCached.Should().BeFalse();
        }

        [Fact]
        public void ShouldResetCache()
        {
            // Arrange
            var cacheManager = new CacheManager();
            cacheManager.IsCacheEnabled = true;

            cacheManager.UpdateCache(typeof(int), typeof(double), isConvertable: true, conversionAttempt: new CastAttempt());

            var cacheResultBeforeReset = cacheManager.TryGetCachedValue(typeof(int), typeof(double));

            // Act
            cacheManager.Reset();

            // Assert
            var cacheResultAfterReset = cacheManager.TryGetCachedValue(typeof(int), typeof(double));

            cacheResultBeforeReset.ConversionAttempt.Should().BeOfType(typeof(CastAttempt));
            cacheResultBeforeReset.IsCached.Should().BeTrue();

            cacheResultAfterReset.ConversionAttempt.Should().BeNull();
            cacheResultAfterReset.IsCached.Should().BeFalse();
        }

        [Fact]
        public void ShouldLimitCacheToMaxCacheSize()
        {
            // Arrange
            var cacheManager = new CacheManager();
            cacheManager.IsCacheEnabled = true;
            cacheManager.MaxCacheSize = 3;
            cacheManager.IsMaxCacheSizeEnabled = true;

            // Seed cache with some data
            cacheManager.UpdateCache(typeof(int), typeof(double), isConvertable: true, conversionAttempt: new CastAttempt());
            cacheManager.UpdateCache(typeof(int), typeof(string), isConvertable: true, conversionAttempt: new CastAttempt());
            cacheManager.UpdateCache(typeof(int), typeof(long), isConvertable: true, conversionAttempt: new CastAttempt());
            
            // Perform some read actions to weight the data
            for (int i = 0; i < 3; i++)
            {
                cacheManager.TryGetCachedValue(typeof(int), typeof(double));
                cacheManager.TryGetCachedValue(typeof(int), typeof(string));
            }

            // Act: Insert new data
            cacheManager.UpdateCache(typeof(int), typeof(decimal), isConvertable: true, conversionAttempt: new CastAttempt());

            // Assert: Check what data has to leave the buffer
            var cacheResultIntDecimal = cacheManager.TryGetCachedValue(typeof(int), typeof(decimal));
            cacheResultIntDecimal.IsCached.Should().BeTrue();

            var cacheResultIntLong = cacheManager.TryGetCachedValue(typeof(int), typeof(long));
            cacheResultIntLong.IsCached.Should().BeFalse();
        }

        [Fact]
        public void ShouldDisableCacheSizeLimit()
        {
            // Arrange
            var cacheManager = new CacheManager();
            cacheManager.IsCacheEnabled = true;
            cacheManager.MaxCacheSize = 3;
            cacheManager.IsMaxCacheSizeEnabled = false;

            // Seed cache with some data
            cacheManager.UpdateCache(typeof(int), typeof(double), isConvertable: true, conversionAttempt: new CastAttempt());
            cacheManager.UpdateCache(typeof(int), typeof(string), isConvertable: true, conversionAttempt: new CastAttempt());
            cacheManager.UpdateCache(typeof(int), typeof(long), isConvertable: true, conversionAttempt: new CastAttempt());

            // Act: Insert new data
            cacheManager.UpdateCache(typeof(int), typeof(decimal), isConvertable: true, conversionAttempt: new CastAttempt());

            // Assert: Check what data has to leave the buffer
            var cacheResultIntDecimal = cacheManager.TryGetCachedValue(typeof(int), typeof(decimal));
            cacheResultIntDecimal.IsCached.Should().BeTrue();

            var cacheResultIntLong = cacheManager.TryGetCachedValue(typeof(int), typeof(long));
            cacheResultIntLong.IsCached.Should().BeTrue();
        }


        [Fact]
        public void ShouldReduceCacheSizeWhenMaxCacheSizeIsEnabled()
        {
            // Arrange
            var cacheManager = new CacheManager();
            cacheManager.IsCacheEnabled = true;
            cacheManager.MaxCacheSize = 3;
            cacheManager.IsMaxCacheSizeEnabled = false;

            cacheManager.UpdateCache(typeof(int), typeof(double), isConvertable: true, conversionAttempt: new CastAttempt());
            cacheManager.UpdateCache(typeof(int), typeof(string), isConvertable: true, conversionAttempt: new CastAttempt());
            cacheManager.UpdateCache(typeof(int), typeof(long), isConvertable: true, conversionAttempt: new CastAttempt());
            cacheManager.UpdateCache(typeof(int), typeof(decimal), isConvertable: true, conversionAttempt: new CastAttempt());

            // Perform some read actions to weight the data
            for (int i = 0; i < 3; i++)
            {
                cacheManager.TryGetCachedValue(typeof(int), typeof(double));
                cacheManager.TryGetCachedValue(typeof(int), typeof(string));
            }
            cacheManager.TryGetCachedValue(typeof(int), typeof(decimal));

            // Act
            cacheManager.IsMaxCacheSizeEnabled = true;

            // Assert
            var cacheResultIntDecimal = cacheManager.TryGetCachedValue(typeof(int), typeof(decimal));
            cacheResultIntDecimal.IsCached.Should().BeTrue();

            var cacheResultIntLong = cacheManager.TryGetCachedValue(typeof(int), typeof(long));
            cacheResultIntLong.IsCached.Should().BeFalse();
        }
    }
}