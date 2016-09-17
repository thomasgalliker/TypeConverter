using FluentAssertions;

using TypeConverter.Converters;

using Xunit;

namespace TypeConverter.Tests.Converters
{
    public class StringToFloatConverterTests
    {
        [Fact]
        public void ShouldConvertFloatMaxValueToString()
        {
            // Arrange
            float inputFloat = float.MaxValue;
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<float, string>(() => new StringToFloatConverter());

            // Act
            var outputString = converterRegistry.Convert<float, string>(inputFloat);

            // Assert
            outputString.Should().Be("3.40282347E+38");
        }

        [Fact]
        public void ShouldConvertFloatMinValueToString()
        {
            // Arrange
            float inputFloat = float.MinValue;
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<float, string>(() => new StringToFloatConverter());

            // Act
            var outputString = converterRegistry.Convert<float, string>(inputFloat);

            // Assert
            outputString.Should().Be("-3.40282347E+38");
        }

        [Fact]
        public void ShouldConvertStringToFloatMaxValue()
        {
            // Arrange
            const string InputString = "3.40282347E+38";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, float>(() => new StringToFloatConverter());

            // Act
            var outputFloat = converterRegistry.Convert<string, float>(InputString);

            // Assert
            outputFloat.Should().Be(float.MaxValue);
        }

        [Fact]
        public void ShouldConvertStringToFloatMinValue()
        {
            // Arrange
            const string InputString = "-3.40282347E+38";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, float>(() => new StringToFloatConverter());

            // Act
            var outputFloat = converterRegistry.Convert<string, float>(InputString);

            // Assert
            outputFloat.Should().Be(float.MinValue);
        }
    }
}