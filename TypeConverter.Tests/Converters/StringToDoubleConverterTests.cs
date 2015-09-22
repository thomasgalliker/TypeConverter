using FluentAssertions;

using TypeConverter.Converters;

using Xunit;

namespace TypeConverter.Tests.Converters
{
    public class StringToDoubleConverterTests
    {
        [Fact]
        public void ShouldConvertBothWays()
        {
            // Arrange
            const string InputString = "1.9998";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, double>(() => new StringToDoubleConverter());
            converterRegistry.RegisterConverter<double, string>(() => new StringToDoubleConverter());

            // Act
            var convertedObject = converterRegistry.Convert<string, double>(InputString);
            var outputString = converterRegistry.Convert<double, string>(convertedObject);

            // Assert
            convertedObject.Should().Be(1.9998d);

            outputString.Should().NotBeNullOrEmpty();
            outputString.Should().Be(InputString);
        }
    }
}