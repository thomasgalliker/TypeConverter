using FluentAssertions;

using TypeConverter.Converters;

using Xunit;

namespace TypeConverter.Tests.Converters
{
    public class StringToDoubleConverterTests
    {
        [Fact]
        public void ShouldConvertDoubleMaxValueToString()
        {
            // Arrange
            double inputDouble = double.MaxValue;
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<double, string>(() => new StringToDoubleConverter());

            // Act
            var outputString = converterRegistry.Convert<double, string>(inputDouble);

            // Assert
            outputString.Should().Be("1.7976931348623157E+308");
        }

        [Fact]
        public void ShouldConvertDoubleMinValueToString()
        {
            // Arrange
            double inputDouble = double.MinValue;
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<double, string>(() => new StringToDoubleConverter());

            // Act
            var outputString = converterRegistry.Convert<double, string>(inputDouble);

            // Assert
            outputString.Should().Be("-1.7976931348623157E+308");
        }

        [Fact]
        public void ShouldConvertStringToDoubleMaxValue()
        {
            // Arrange
            const string InputString = "1.7976931348623157E+308";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, double>(() => new StringToDoubleConverter());

            // Act
            var outputDouble = converterRegistry.Convert<string, double>(InputString);

            // Assert
            outputDouble.Should().Be(double.MaxValue);
        }

        [Fact]
        public void ShouldConvertStringToDoubleMinValue()
        {
            // Arrange
            const string InputString = "-1.7976931348623157E+308";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, double>(() => new StringToDoubleConverter());

            // Act
            var outputDouble = converterRegistry.Convert<string, double>(InputString);

            // Assert
            outputDouble.Should().Be(double.MinValue);
        }
    }
}