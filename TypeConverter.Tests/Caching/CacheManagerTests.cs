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
    }
}