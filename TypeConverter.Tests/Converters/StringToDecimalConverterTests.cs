using FluentAssertions;

using TypeConverter.Converters;

using Xunit;

namespace TypeConverter.Tests.Converters
{
    public class StringToDecimalConverterTests
    {
        [Fact]
        public void ShouldConvertDecimalMaxValueToString()
        {
            // Arrange
            decimal inputDecimal = decimal.MaxValue;
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<decimal, string>(() => new StringToDecimalConverter());

            // Act
            var outputString = converterRegistry.Convert<decimal, string>(inputDecimal);

            // Assert
            outputString.Should().Be("79228162514264337593543950335");
        }

        [Fact]
        public void ShouldConvertDecimalMinValueToString()
        {
            // Arrange
            decimal inputDecimal = decimal.MinValue;
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<decimal, string>(() => new StringToDecimalConverter());

            // Act
            var outputString = converterRegistry.Convert<decimal, string>(inputDecimal);

            // Assert
            outputString.Should().Be("-79228162514264337593543950335");
        }

        [Fact]
        public void ShouldConvertStringToDecimalMaxValue()
        {
            // Arrange
            const string InputString = "79228162514264337593543950335";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, decimal>(() => new StringToDecimalConverter());

            // Act
            var outputDecimal = converterRegistry.Convert<string, decimal>(InputString);

            // Assert
            outputDecimal.Should().Be(decimal.MaxValue);
        }

        [Fact]
        public void ShouldConvertStringToDecimalMinValue()
        {
            // Arrange
            const string InputString = "-79228162514264337593543950335";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, decimal>(() => new StringToDecimalConverter());

            // Act
            var outputDecimal = converterRegistry.Convert<string, decimal>(InputString);

            // Assert
            outputDecimal.Should().Be(decimal.MinValue);
        }
    }
}