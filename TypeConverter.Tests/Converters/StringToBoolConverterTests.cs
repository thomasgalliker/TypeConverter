

using FluentAssertions;

using TypeConverter.Converters;

using Xunit;

namespace TypeConverter.Tests.Converters
{
    public class StringToBoolConverterTests
    {
        [Fact]
        public void ShouldConvertBothWays()
        {
            // Arrange
            const string InputString = "True";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, bool>(() => new StringToBoolConverter());
            converterRegistry.RegisterConverter<bool, string>(() => new StringToBoolConverter());

            // Act
            var convertedObject = converterRegistry.Convert<string, bool>(InputString);
            var outputString = converterRegistry.Convert<bool, string>(convertedObject);

            // Assert
            convertedObject.Should().BeTrue();

            outputString.Should().NotBeNullOrEmpty();
            outputString.Should().Be(InputString);
        }
    }
}
