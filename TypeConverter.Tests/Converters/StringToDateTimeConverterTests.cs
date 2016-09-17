using System;

using FluentAssertions;

using TypeConverter.Converters;

using Xunit;

namespace TypeConverter.Tests.Converters
{
    public class StringToDateTimeConverterTests
    {
        [Fact]
        public void ShouldConvertDateTimeToString_Universal()
        {
            // Arrange
            DateTime intputDateTime = new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Utc);
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<DateTime, string>(() => new StringToDateTimeConverter());

            // Act
            var outputString = converterRegistry.Convert<DateTime, string>(intputDateTime);

            // Assert
            outputString.Should().Be("1999-12-31T23:59:59.0000000Z");
        }

        [Fact]
        public void ShouldConvertStringToDateTime_Universal()
        {
            // Arrange
            const string InputString = "1999-12-31T23:59:59.0000000Z";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, DateTime>(() => new StringToDateTimeConverter());

            // Act
            var outputDateTime = converterRegistry.Convert<string, DateTime>(InputString);

            // Assert
            outputDateTime.Should().Be(new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Utc));
            outputDateTime.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void ShouldConvertDateTimeToString_Local()
        {
            // Arrange
            DateTime intputDateTime = new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Local);
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<DateTime, string>(() => new StringToDateTimeConverter());

            // Act
            var outputString = converterRegistry.Convert<DateTime, string>(intputDateTime);

            // Assert
            outputString.Should().Be("1999-12-31T23:59:59.0000000+01:00");
        }

        [Fact]
        public void ShouldConvertStringToDateTime_Local()
        {
            // Arrange
            const string InputString = "1999-12-31T23:59:59.0000000+01:00";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, DateTime>(() => new StringToDateTimeConverter());

            // Act
            var outputDateTime = converterRegistry.Convert<string, DateTime>(InputString);

            // Assert
            outputDateTime.Should().Be(new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Local));
            outputDateTime.Kind.Should().Be(DateTimeKind.Local);
        }
    }
}