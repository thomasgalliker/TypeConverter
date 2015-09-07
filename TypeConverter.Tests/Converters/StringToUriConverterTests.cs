
using System;

using FluentAssertions;

using TypeConverter.Tests.Testdata;

using Xunit;

namespace TypeConverter.Tests.Converters
{
    public class StringToUriConverterTests
    {
        [Fact]
        public void ShouldConvertStringToUri()
        {
            // Arrange
            const string InputString = "http://www.google.com/";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, Uri>(() => new StringToUriConverter());
            converterRegistry.RegisterConverter<Uri, string>(() => new StringToUriConverter());

            // Act
            var convertedObject = converterRegistry.Convert<string, Uri>(InputString);
            var outputString = converterRegistry.Convert<Uri, string>(convertedObject);

            // Assert
            convertedObject.Should().NotBeNull();
            convertedObject.Should().BeOfType<Uri>();
            convertedObject.As<Uri>().AbsoluteUri.Should().Be(InputString);

            outputString.Should().NotBeNullOrEmpty();
            outputString.Should().Be(InputString);
        }
    }
}
