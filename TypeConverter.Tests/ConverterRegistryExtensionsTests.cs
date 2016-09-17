using System;

using FluentAssertions;

using TypeConverter.Converters;
using TypeConverter.Extensions;

using Xunit;

namespace TypeConverter.Tests
{
    public class ConverterRegistryExtensionsTests
    {
        [Fact]
        public void ShouldRegisterMultipleConvertables()
        {
            // Arrange
            const string InputUriString = "http://www.superdev.ch/";
            const string InputBoolString = "True";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            IConvertable[] converters = { new StringToUriConverter(), new StringToBoolConverter() };
            converterRegistry.RegisterConverters(converters);

            // Act
            var outputUri = converterRegistry.Convert<string, Uri>(InputUriString);
            var outputUriString = converterRegistry.Convert<Uri, string>(outputUri);

            var outputBool = converterRegistry.Convert<string, bool>(InputBoolString);
            var outputBoolString = converterRegistry.Convert<bool, string>(outputBool);


            // Assert
            outputUriString.Should().Be(InputUriString);
            outputBoolString.Should().Be(InputBoolString);
        }
    }
}