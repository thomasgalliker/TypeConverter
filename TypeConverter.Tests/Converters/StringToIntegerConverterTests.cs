

using FluentAssertions;

using TypeConverter.Converters;

using Xunit;

namespace TypeConverter.Tests.Converters
{
    public class StringToIntegerConverterTests
    {
        [Fact]
        public void ShouldConvertBothWays()
        {
            // Arrange
            const string InputString = "999";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, int>(() => new StringToIntegerConverter());
            converterRegistry.RegisterConverter<int, string>(() => new StringToIntegerConverter());

            // Act
            var convertedObject = converterRegistry.Convert<string, int>(InputString);
            var outputString = converterRegistry.Convert<int, string>(convertedObject);

            // Assert
            convertedObject.Should().Be(999);

            outputString.Should().NotBeNullOrEmpty();
            outputString.Should().Be(InputString);
        }
    }
}
