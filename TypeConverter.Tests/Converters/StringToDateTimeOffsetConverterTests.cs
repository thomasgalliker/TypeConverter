using System;

using FluentAssertions;

using TypeConverter.Converters;

using Xunit;

namespace TypeConverter.Tests.Converters
{
    public class StringToDateTimeOffsetConverterTests
    {
        [Fact]
        public void ShouldConvertDateTimeToString_Universal()
        {
            // Arrange
            DateTimeOffset intputDateTime = new DateTimeOffset(new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Utc));
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<DateTimeOffset, string>(() => new StringToDateTimeOffsetConverter());

            // Act
            var outputString = converterRegistry.Convert<DateTimeOffset, string>(intputDateTime);

            // Assert
            outputString.Should().Be("1999-12-31T23:59:59.0000000+00:00");
        }

        [Fact]
        public void ShouldConvertStringToDateTime_Universal()
        {
            // Arrange
            const string InputString = "1999-12-31T23:59:59.0000000+00:00";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, DateTimeOffset>(() => new StringToDateTimeOffsetConverter());

            // Act
            var outputDateTime = converterRegistry.Convert<string, DateTimeOffset>(InputString);

            // Assert
            outputDateTime.Should().Be(new DateTimeOffset(new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Utc)));
        }

        [Fact]
        public void ShouldConvertDateTimeToString_Local()
        {
            // Arrange
            DateTimeOffset intputDateTime = new DateTimeOffset(new DateTime(1999, 12, 31, 23, 59, 59), new TimeSpan(-7, 0, 0));
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<DateTimeOffset, string>(() => new StringToDateTimeOffsetConverter());

            // Act
            var outputString = converterRegistry.Convert<DateTimeOffset, string>(intputDateTime);

            // Assert
            outputString.Should().Be("1999-12-31T23:59:59.0000000-07:00");
        }

        [Fact]
        public void ShouldConvertStringToDateTime_Local()
        {
            // Arrange
            const string InputString = "1999-12-31T23:59:59.0000000-07:00";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, DateTimeOffset>(() => new StringToDateTimeOffsetConverter());

            // Act
            var outputDateTime = converterRegistry.Convert<string, DateTimeOffset>(InputString);

            // Assert
            outputDateTime.Should().Be(new DateTimeOffset(new DateTime(1999, 12, 31, 23, 59, 59), new TimeSpan(-7, 0, 0)));
        }
    }
}