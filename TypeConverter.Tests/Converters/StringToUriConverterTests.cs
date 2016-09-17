using System;

using FluentAssertions;

using TypeConverter.Converters;

using Xunit;

namespace TypeConverter.Tests.Converters
{
    public class StringToUriConverterTests
    {
        [Fact]
        public void ShouldConvertStringToUri()
        {
            // Arrange
            const string InputString = "http://www.superdev.ch/";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, Uri>(() => new StringToUriConverter());
            converterRegistry.RegisterConverter<Uri, string>(() => new StringToUriConverter());

            // Act
            var outputUri = converterRegistry.Convert<string, Uri>(InputString);

            // Assert
            outputUri.Should().NotBeNull();
            outputUri.AbsoluteUri.Should().Be(InputString);
        }

        [Fact]
        public void ShouldConvertUriToString()
        {
            // Arrange
            var inputUri = new Uri("http://www.superdev.ch/");
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<Uri, string>(() => new StringToUriConverter());

            // Act
            var outputString = converterRegistry.Convert<Uri, string>(inputUri);

            // Assert
            outputString.Should().Be(inputUri.AbsoluteUri);
        }
    }
}